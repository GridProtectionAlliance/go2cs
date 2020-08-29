// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 August 29 08:25:33 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_windows.go
using errors = go.errors_package;
using race = go.@internal.race_package;
using windows = go.@internal.syscall.windows_package;
using io = go.io_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private static error initErr = default;        private static ulong ioSync = default;

        // CancelIo Windows API cancels all outstanding IO for a particular
        // socket on current thread. To overcome that limitation, we run
        // special goroutine, locked to OS single thread, that both starts
        // and cancels IO. It means, there are 2 unavoidable thread switches
        // for every IO.
        // Some newer versions of Windows has new CancelIoEx API, that does
        // not have that limitation and can be used from any thread. This
        // package uses CancelIoEx API, if present, otherwise it fallback
        // to CancelIo.

        private static bool canCancelIO = default; // determines if CancelIoEx API is present

        // This package uses SetFileCompletionNotificationModes Windows API
        // to skip calling GetQueuedCompletionStatus if an IO operation completes
        // synchronously. Unfortuently SetFileCompletionNotificationModes is not
        // available on Windows XP. Also there is a known bug where
        // SetFileCompletionNotificationModes crashes on some systems
        // (see http://support.microsoft.com/kb/2568167 for details).

        private static bool useSetFileCompletionNotificationModes = default; // determines is SetFileCompletionNotificationModes is present and safe to use

        // checkSetFileCompletionNotificationModes verifies that
        // SetFileCompletionNotificationModes Windows API is present
        // on the system and is safe to use.
        // See http://support.microsoft.com/kb/2568167 for details.
        private static void checkSetFileCompletionNotificationModes()
        {
            var err = syscall.LoadSetFileCompletionNotificationModes();
            if (err != null)
            {
                return;
            }
            array<int> protos = new array<int>(new int[] { syscall.IPPROTO_TCP, 0 });
            array<syscall.WSAProtocolInfo> buf = new array<syscall.WSAProtocolInfo>(32L);
            var len = uint32(@unsafe.Sizeof(buf));
            var (n, err) = syscall.WSAEnumProtocols(ref protos[0L], ref buf[0L], ref len);
            if (err != null)
            {
                return;
            }
            for (var i = int32(0L); i < n; i++)
            {
                if (buf[i].ServiceFlags1 & syscall.XP1_IFS_HANDLES == 0L)
                {
                    return;
                }
            }

            useSetFileCompletionNotificationModes = true;
        }

        private static void init()
        {
            syscall.WSAData d = default;
            var e = syscall.WSAStartup(uint32(0x202UL), ref d);
            if (e != null)
            {
                initErr = e;
            }
            canCancelIO = syscall.LoadCancelIoEx() == null;
            checkSetFileCompletionNotificationModes();
        }

        // operation contains superset of data necessary to perform all async IO.
        private partial struct operation
        {
            public syscall.Overlapped o; // fields used by runtime.netpoll
            public System.UIntPtr runtimeCtx;
            public int mode;
            public int errno;
            public uint qty; // fields used only by net package
            public ptr<FD> fd;
            public channel<error> errc;
            public syscall.WSABuf buf;
            public windows.WSAMsg msg;
            public syscall.Sockaddr sa;
            public ptr<syscall.RawSockaddrAny> rsa;
            public int rsan;
            public syscall.Handle handle;
            public uint flags;
            public slice<syscall.WSABuf> bufs;
        }

        private static void InitBuf(this ref operation o, slice<byte> buf)
        {
            o.buf.Len = uint32(len(buf));
            o.buf.Buf = null;
            if (len(buf) != 0L)
            {
                o.buf.Buf = ref buf[0L];
            }
        }

        private static void InitBufs(this ref operation o, ref slice<slice<byte>> buf)
        {
            if (o.bufs == null)
            {
                o.bufs = make_slice<syscall.WSABuf>(0L, len(buf.Value));
            }
            else
            {
                o.bufs = o.bufs[..0L];
            }
            foreach (var (_, b) in buf.Value)
            {
                ref byte p = default;
                if (len(b) > 0L)
                {
                    p = ref b[0L];
                }
                o.bufs = append(o.bufs, new syscall.WSABuf(Len:uint32(len(b)),Buf:p));
            }
        }

        // ClearBufs clears all pointers to Buffers parameter captured
        // by InitBufs, so it can be released by garbage collector.
        private static void ClearBufs(this ref operation o)
        {
            foreach (var (i) in o.bufs)
            {
                o.bufs[i].Buf = null;
            }
            o.bufs = o.bufs[..0L];
        }

        private static void InitMsg(this ref operation o, slice<byte> p, slice<byte> oob)
        {
            o.InitBuf(p);
            o.msg.Buffers = ref o.buf;
            o.msg.BufferCount = 1L;

            o.msg.Name = null;
            o.msg.Namelen = 0L;

            o.msg.Flags = 0L;
            o.msg.Control.Len = uint32(len(oob));
            o.msg.Control.Buf = null;
            if (len(oob) != 0L)
            {
                o.msg.Control.Buf = ref oob[0L];
            }
        }

        // ioSrv executes net IO requests.
        private partial struct ioSrv
        {
            public channel<ioSrvReq> req;
        }

        private partial struct ioSrvReq
        {
            public ptr<operation> o;
            public Func<ref operation, error> submit; // if nil, cancel the operation
        }

        // ProcessRemoteIO will execute submit IO requests on behalf
        // of other goroutines, all on a single os thread, so it can
        // cancel them later. Results of all operations will be sent
        // back to their requesters via channel supplied in request.
        // It is used only when the CancelIoEx API is unavailable.
        private static void ProcessRemoteIO(this ref ioSrv _s) => func(_s, (ref ioSrv s, Defer defer, Panic _, Recover __) =>
        {
            runtime.LockOSThread();
            defer(runtime.UnlockOSThread());
            foreach (var (r) in s.req)
            {
                if (r.submit != null)
                {
                    r.o.errc.Send(r.submit(r.o));
                }
                else
                {
                    r.o.errc.Send(syscall.CancelIo(r.o.fd.Sysfd));
                }
            }
        });

        // ExecIO executes a single IO operation o. It submits and cancels
        // IO in the current thread for systems where Windows CancelIoEx API
        // is available. Alternatively, it passes the request onto
        // runtime netpoll and waits for completion or cancels request.
        private static (long, error) ExecIO(this ref ioSrv _s, ref operation _o, Func<ref operation, error> submit) => func(_s, _o, (ref ioSrv s, ref operation o, Defer _, Panic panic, Recover __) =>
        {
            if (o.fd.pd.runtimeCtx == 0L)
            {
                return (0L, errors.New("internal error: polling on unsupported descriptor type"));
            }
            if (!canCancelIO)
            {
                onceStartServer.Do(startServer);
            }
            var fd = o.fd; 
            // Notify runtime netpoll about starting IO.
            var err = fd.pd.prepare(int(o.mode), fd.isFile);
            if (err != null)
            {
                return (0L, err);
            } 
            // Start IO.
            if (canCancelIO)
            {
                err = submit(o);
            }
            else
            { 
                // Send request to a special dedicated thread,
                // so it can stop the IO with CancelIO later.
                s.req.Send(new ioSrvReq(o,submit));
                err = o.errc.Receive();
            }

            if (err == null) 
                // IO completed immediately
                if (o.fd.skipSyncNotif)
                { 
                    // No completion message will follow, so return immediately.
                    return (int(o.qty), null);
                } 
                // Need to get our completion message anyway.
            else if (err == syscall.ERROR_IO_PENDING) 
                // IO started, and we have to wait for its completion.
                err = null;
            else 
                return (0L, err);
            // Wait for our request to complete.
            err = fd.pd.wait(int(o.mode), fd.isFile);
            if (err == null)
            { 
                // All is good. Extract our IO results and return.
                if (o.errno != 0L)
                {
                    err = syscall.Errno(o.errno);
                    return (0L, err);
                }
                return (int(o.qty), null);
            } 
            // IO is interrupted by "close" or "timeout"
            var netpollErr = err;

            if (netpollErr == ErrNetClosing || netpollErr == ErrFileClosing || netpollErr == ErrTimeout)             else 
                panic("unexpected runtime.netpoll error: " + netpollErr.Error());
            // Cancel our request.
            if (canCancelIO)
            {
                err = syscall.CancelIoEx(fd.Sysfd, ref o.o); 
                // Assuming ERROR_NOT_FOUND is returned, if IO is completed.
                if (err != null && err != syscall.ERROR_NOT_FOUND)
                { 
                    // TODO(brainman): maybe do something else, but panic.
                    panic(err);
                }
            }
            else
            {
                s.req.Send(new ioSrvReq(o,nil));
                o.errc.Receive();
            } 
            // Wait for cancelation to complete.
            fd.pd.waitCanceled(int(o.mode));
            if (o.errno != 0L)
            {
                err = syscall.Errno(o.errno);
                if (err == syscall.ERROR_OPERATION_ABORTED)
                { // IO Canceled
                    err = netpollErr;
                }
                return (0L, err);
            } 
            // We issued a cancelation request. But, it seems, IO operation succeeded
            // before the cancelation request run. We need to treat the IO operation as
            // succeeded (the bytes are actually sent/recv from network).
            return (int(o.qty), null);
        });

        // Start helper goroutines.
        private static ioSrv rsrv = default;        private static ioSrv wsrv = default;

        private static sync.Once onceStartServer = default;

        private static void startServer()
        { 
            // This is called, once, when only the CancelIo API is available.
            // Start two special goroutines, both locked to an OS thread,
            // that start and cancel IO requests.
            // One will process read requests, while the other will do writes.
            rsrv.req = make_channel<ioSrvReq>();
            go_(() => rsrv.ProcessRemoteIO());
            wsrv.req = make_channel<ioSrvReq>();
            go_(() => wsrv.ProcessRemoteIO());
        }

        // FD is a file descriptor. The net and os packages embed this type in
        // a larger type representing a network connection or OS file.
        public partial struct FD
        {
            public fdMutex fdmu; // System file descriptor. Immutable until Close.
            public syscall.Handle Sysfd; // Read operation.
            public operation rop; // Write operation.
            public operation wop; // I/O poller.
            public pollDesc pd; // Used to implement pread/pwrite.
            public sync.Mutex l; // For console I/O.
            public bool isConsole;
            public slice<byte> lastbits; // first few bytes of the last incomplete rune in last write
            public slice<ushort> readuint16; // buffer to hold uint16s obtained with ReadConsole
            public slice<byte> readbyte; // buffer to hold decoding of readuint16 from utf16 to utf8
            public long readbyteOffset; // readbyte[readOffset:] is yet to be consumed with file.Read

// Semaphore signaled when file is closed.
            public uint csema;
            public bool skipSyncNotif; // Whether this is a streaming descriptor, as opposed to a
// packet-based descriptor like a UDP socket.
            public bool IsStream; // Whether a zero byte read indicates EOF. This is false for a
// message based socket connection.
            public bool ZeroReadIsEOF; // Whether this is a normal file.
            public bool isFile; // Whether this is a directory.
            public bool isDir;
        }

        // logInitFD is set by tests to enable file descriptor initialization logging.
        private static Action<@string, ref FD, error> logInitFD = default;

        // Init initializes the FD. The Sysfd field should already be set.
        // This can be called multiple times on a single FD.
        // The net argument is a network name from the net package (e.g., "tcp"),
        // or "file" or "console" or "dir".
        // Set pollable to true if fd should be managed by runtime netpoll.
        private static (@string, error) Init(this ref FD fd, @string net, bool pollable)
        {
            if (initErr != null)
            {
                return ("", initErr);
            }
            switch (net)
            {
                case "file": 
                    fd.isFile = true;
                    break;
                case "console": 
                    fd.isConsole = true;
                    break;
                case "dir": 
                    fd.isDir = true;
                    break;
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    break;
                case "udp": 

                case "udp4": 

                case "udp6": 
                    break;
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    break;
                default: 
                    return ("", errors.New("internal error: unknown network type " + net));
                    break;
            }

            error err = default;
            if (pollable)
            { 
                // Only call init for a network socket.
                // This means that we don't add files to the runtime poller.
                // Adding files to the runtime poller can confuse matters
                // if the user is doing their own overlapped I/O.
                // See issue #21172.
                //
                // In general the code below avoids calling the ExecIO
                // method for non-network sockets. If some method does
                // somehow call ExecIO, then ExecIO, and therefore the
                // calling method, will return an error, because
                // fd.pd.runtimeCtx will be 0.
                err = error.As(fd.pd.init(fd));
            }
            if (logInitFD != null)
            {
                logInitFD(net, fd, err);
            }
            if (err != null)
            {
                return ("", err);
            }
            if (pollable && useSetFileCompletionNotificationModes)
            { 
                // We do not use events, so we can skip them always.
                var flags = uint8(syscall.FILE_SKIP_SET_EVENT_ON_HANDLE); 
                // It's not safe to skip completion notifications for UDP:
                // http://blogs.technet.com/b/winserverperformance/archive/2008/06/26/designing-applications-for-high-performance-part-iii.aspx
                if (net == "tcp")
                {
                    flags |= syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS;
                }
                err = syscall.SetFileCompletionNotificationModes(fd.Sysfd, flags);
                if (err == null && flags & syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS != 0L)
                {
                    fd.skipSyncNotif = true;
                }
            } 
            // Disable SIO_UDP_CONNRESET behavior.
            // http://support.microsoft.com/kb/263823
            switch (net)
            {
                case "udp": 

                case "udp4": 

                case "udp6": 
                    var ret = uint32(0L);
                    var flag = uint32(0L);
                    var size = uint32(@unsafe.Sizeof(flag));
                    err = syscall.WSAIoctl(fd.Sysfd, syscall.SIO_UDP_CONNRESET, (byte.Value)(@unsafe.Pointer(ref flag)), size, null, 0L, ref ret, null, 0L);
                    if (err != null)
                    {
                        return ("wsaioctl", err);
                    }
                    break;
            }
            fd.rop.mode = 'r';
            fd.wop.mode = 'w';
            fd.rop.fd = fd;
            fd.wop.fd = fd;
            fd.rop.runtimeCtx = fd.pd.runtimeCtx;
            fd.wop.runtimeCtx = fd.pd.runtimeCtx;
            if (!canCancelIO)
            {
                fd.rop.errc = make_channel<error>();
                fd.wop.errc = make_channel<error>();
            }
            return ("", null);
        }

        private static error destroy(this ref FD fd)
        {
            if (fd.Sysfd == syscall.InvalidHandle)
            {
                return error.As(syscall.EINVAL);
            } 
            // Poller may want to unregister fd in readiness notification mechanism,
            // so this must be executed before fd.CloseFunc.
            fd.pd.close();
            error err = default;
            if (fd.isFile || fd.isConsole)
            {
                err = error.As(syscall.CloseHandle(fd.Sysfd));
            }
            else if (fd.isDir)
            {
                err = error.As(syscall.FindClose(fd.Sysfd));
            }
            else
            { 
                // The net package uses the CloseFunc variable for testing.
                err = error.As(CloseFunc(fd.Sysfd));
            }
            fd.Sysfd = syscall.InvalidHandle;
            runtime_Semrelease(ref fd.csema);
            return error.As(err);
        }

        // Close closes the FD. The underlying file descriptor is closed by
        // the destroy method when there are no remaining references.
        private static error Close(this ref FD fd)
        {
            if (!fd.fdmu.increfAndClose())
            {
                return error.As(errClosing(fd.isFile));
            } 
            // unblock pending reader and writer
            fd.pd.evict();
            var err = fd.decref(); 
            // Wait until the descriptor is closed. If this was the only
            // reference, it is already closed.
            runtime_Semacquire(ref fd.csema);
            return error.As(err);
        }

        // Shutdown wraps the shutdown network call.
        private static error Shutdown(this ref FD _fd, long how) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Shutdown(fd.Sysfd, how));
        });

        // Read implements io.Reader.
        private static (long, error) Read(this ref FD _fd, slice<byte> buf) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            defer(fd.readUnlock());

            long n = default;
            err = default;
            if (fd.isFile || fd.isDir || fd.isConsole)
            {
                fd.l.Lock();
                defer(fd.l.Unlock());
                if (fd.isConsole)
                {
                    n, err = fd.readConsole(buf);
                }
                else
                {
                    n, err = syscall.Read(fd.Sysfd, buf);
                }
                if (err != null)
                {
                    n = 0L;
                }
            }
            else
            {
                var o = ref fd.rop;
                o.InitBuf(buf);
                n, err = rsrv.ExecIO(o, o =>
                {
                    return syscall.WSARecv(o.fd.Sysfd, ref o.buf, 1L, ref o.qty, ref o.flags, ref o.o, null);
                });
                if (race.Enabled)
                {
                    race.Acquire(@unsafe.Pointer(ref ioSync));
                }
            }
            if (len(buf) != 0L)
            {
                err = fd.eofError(n, err);
            }
            return (n, err);
        });

        public static var ReadConsole = syscall.ReadConsole; // changed for testing

        // readConsole reads utf16 characters from console File,
        // encodes them into utf8 and stores them in buffer b.
        // It returns the number of utf8 bytes read and an error, if any.
        private static (long, error) readConsole(this ref FD fd, slice<byte> b)
        {
            if (len(b) == 0L)
            {
                return (0L, null);
            }
            if (fd.readuint16 == null)
            { 
                // Note: syscall.ReadConsole fails for very large buffers.
                // The limit is somewhere around (but not exactly) 16384.
                // Stay well below.
                fd.readuint16 = make_slice<ushort>(0L, 10000L);
                fd.readbyte = make_slice<byte>(0L, 4L * cap(fd.readuint16));
            }
            while (fd.readbyteOffset >= len(fd.readbyte))
            {
                var n = cap(fd.readuint16) - len(fd.readuint16);
                if (n > len(b))
                {
                    n = len(b);
                }
                uint nw = default;
                var err = ReadConsole(fd.Sysfd, ref fd.readuint16[..len(fd.readuint16) + 1L][len(fd.readuint16)], uint32(n), ref nw, null);
                if (err != null)
                {
                    return (0L, err);
                }
                var uint16s = fd.readuint16[..len(fd.readuint16) + int(nw)];
                fd.readuint16 = fd.readuint16[..0L];
                var buf = fd.readbyte[..0L];
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < len(uint16s); i++)
                    {
                        var r = rune(uint16s[i]);
                        if (utf16.IsSurrogate(r))
                        {
                            if (i + 1L == len(uint16s))
                            {
                                if (nw > 0L)
                                { 
                                    // Save half surrogate pair for next time.
                                    fd.readuint16 = fd.readuint16[..1L];
                                    fd.readuint16[0L] = uint16(r);
                                    break;
                                }
                                r = utf8.RuneError;
                            }
                            else
                            {
                                r = utf16.DecodeRune(r, rune(uint16s[i + 1L]));
                                if (r != utf8.RuneError)
                                {
                                    i++;
                                }
                            }
                        }
                        n = utf8.EncodeRune(buf[len(buf)..cap(buf)], r);
                        buf = buf[..len(buf) + n];
                    }


                    i = i__prev2;
                }
                fd.readbyte = buf;
                fd.readbyteOffset = 0L;
                if (nw == 0L)
                {
                    break;
                }
            }


            var src = fd.readbyte[fd.readbyteOffset..];
            i = default;
            for (i = 0L; i < len(src) && i < len(b); i++)
            {
                var x = src[i];
                if (x == 0x1AUL)
                { // Ctrl-Z
                    if (i == 0L)
                    {
                        fd.readbyteOffset++;
                    }
                    break;
                }
                b[i] = x;
            }

            fd.readbyteOffset += i;
            return (i, null);
        }

        // Pread emulates the Unix pread system call.
        private static (long, error) Pread(this ref FD _fd, slice<byte> b, long off) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        { 
            // Call incref, not readLock, because since pread specifies the
            // offset it is independent from other reads.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());

            fd.l.Lock();
            defer(fd.l.Unlock());
            var (curoffset, e) = syscall.Seek(fd.Sysfd, 0L, io.SeekCurrent);
            if (e != null)
            {
                return (0L, e);
            }
            defer(syscall.Seek(fd.Sysfd, curoffset, io.SeekStart));
            syscall.Overlapped o = new syscall.Overlapped(OffsetHigh:uint32(off>>32),Offset:uint32(off),);
            uint done = default;
            e = syscall.ReadFile(fd.Sysfd, b, ref done, ref o);
            if (e != null)
            {
                done = 0L;
                if (e == syscall.ERROR_HANDLE_EOF)
                {
                    e = io.EOF;
                }
            }
            if (len(b) != 0L)
            {
                e = fd.eofError(int(done), e);
            }
            return (int(done), e);
        });

        // ReadFrom wraps the recvfrom network call.
        private static (long, syscall.Sockaddr, error) ReadFrom(this ref FD _fd, slice<byte> buf) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            if (len(buf) == 0L)
            {
                return (0L, null, null);
            }
            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, null, err);
                }

            }
            defer(fd.readUnlock());
            var o = ref fd.rop;
            o.InitBuf(buf);
            var (n, err) = rsrv.ExecIO(o, o =>
            {
                if (o.rsa == null)
                {
                    o.rsa = @new<syscall.RawSockaddrAny>();
                }
                o.rsan = int32(@unsafe.Sizeof(o.rsa.Value));
                return syscall.WSARecvFrom(o.fd.Sysfd, ref o.buf, 1L, ref o.qty, ref o.flags, o.rsa, ref o.rsan, ref o.o, null);
            });
            err = fd.eofError(n, err);
            if (err != null)
            {
                return (n, null, err);
            }
            var (sa, _) = o.rsa.Sockaddr();
            return (n, sa, null);
        });

        // Write implements io.Writer.
        private static (long, error) Write(this ref FD _fd, slice<byte> buf) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

                err = err__prev1;

            }
            defer(fd.writeUnlock());

            long n = default;
            err = default;
            if (fd.isFile || fd.isDir || fd.isConsole)
            {
                fd.l.Lock();
                defer(fd.l.Unlock());
                if (fd.isConsole)
                {
                    n, err = fd.writeConsole(buf);
                }
                else
                {
                    n, err = syscall.Write(fd.Sysfd, buf);
                }
                if (err != null)
                {
                    n = 0L;
                }
            }
            else
            {
                if (race.Enabled)
                {
                    race.ReleaseMerge(@unsafe.Pointer(ref ioSync));
                }
                var o = ref fd.wop;
                o.InitBuf(buf);
                n, err = wsrv.ExecIO(o, o =>
                {
                    return syscall.WSASend(o.fd.Sysfd, ref o.buf, 1L, ref o.qty, 0L, ref o.o, null);
                });
            }
            return (n, err);
        });

        // writeConsole writes len(b) bytes to the console File.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) writeConsole(this ref FD fd, slice<byte> b)
        {
            var n = len(b);
            var runes = make_slice<int>(0L, 256L);
            if (len(fd.lastbits) > 0L)
            {
                b = append(fd.lastbits, b);
                fd.lastbits = null;

            }
            while (len(b) >= utf8.UTFMax || utf8.FullRune(b))
            {
                var (r, l) = utf8.DecodeRune(b);
                runes = append(runes, r);
                b = b[l..];
            }

            if (len(b) > 0L)
            {
                fd.lastbits = make_slice<byte>(len(b));
                copy(fd.lastbits, b);
            } 
            // syscall.WriteConsole seems to fail, if given large buffer.
            // So limit the buffer to 16000 characters. This number was
            // discovered by experimenting with syscall.WriteConsole.
            const long maxWrite = 16000L;

            while (len(runes) > 0L)
            {
                var m = len(runes);
                if (m > maxWrite)
                {
                    m = maxWrite;
                }
                var chunk = runes[..m];
                runes = runes[m..];
                var uint16s = utf16.Encode(chunk);
                while (len(uint16s) > 0L)
                {
                    uint written = default;
                    var err = syscall.WriteConsole(fd.Sysfd, ref uint16s[0L], uint32(len(uint16s)), ref written, null);
                    if (err != null)
                    {
                        return (0L, err);
                    }
                    uint16s = uint16s[written..];
                }

            }

            return (n, null);
        }

        // Pwrite emulates the Unix pwrite system call.
        private static (long, error) Pwrite(this ref FD _fd, slice<byte> b, long off) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        { 
            // Call incref, not writeLock, because since pwrite specifies the
            // offset it is independent from other writes.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());

            fd.l.Lock();
            defer(fd.l.Unlock());
            var (curoffset, e) = syscall.Seek(fd.Sysfd, 0L, io.SeekCurrent);
            if (e != null)
            {
                return (0L, e);
            }
            defer(syscall.Seek(fd.Sysfd, curoffset, io.SeekStart));
            syscall.Overlapped o = new syscall.Overlapped(OffsetHigh:uint32(off>>32),Offset:uint32(off),);
            uint done = default;
            e = syscall.WriteFile(fd.Sysfd, b, ref done, ref o);
            if (e != null)
            {
                return (0L, e);
            }
            return (int(done), null);
        });

        // Writev emulates the Unix writev system call.
        private static (long, error) Writev(this ref FD _fd, ref slice<slice<byte>> _buf) => func(_fd, _buf, (ref FD fd, ref slice<slice<byte>> buf, Defer defer, Panic _, Recover __) =>
        {
            if (len(buf.Value) == 0L)
            {
                return (0L, null);
            }
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.writeUnlock());
            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(ref ioSync));
            }
            var o = ref fd.wop;
            o.InitBufs(buf);
            var (n, err) = wsrv.ExecIO(o, o =>
            {
                return syscall.WSASend(o.fd.Sysfd, ref o.bufs[0L], uint32(len(o.bufs)), ref o.qty, 0L, ref o.o, null);
            });
            o.ClearBufs();
            TestHookDidWritev(n);
            consume(buf, int64(n));
            return (int64(n), err);
        });

        // WriteTo wraps the sendto network call.
        private static (long, error) WriteTo(this ref FD _fd, slice<byte> buf, syscall.Sockaddr sa) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            if (len(buf) == 0L)
            {
                return (0L, null);
            }
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.writeUnlock());
            var o = ref fd.wop;
            o.InitBuf(buf);
            o.sa = sa;
            var (n, err) = wsrv.ExecIO(o, o =>
            {
                return syscall.WSASendto(o.fd.Sysfd, ref o.buf, 1L, ref o.qty, 0L, o.sa, ref o.o, null);
            });
            return (n, err);
        });

        // Call ConnectEx. This doesn't need any locking, since it is only
        // called when the descriptor is first created. This is here rather
        // than in the net package so that it can use fd.wop.
        private static error ConnectEx(this ref FD fd, syscall.Sockaddr ra)
        {
            var o = ref fd.wop;
            o.sa = ra;
            var (_, err) = wsrv.ExecIO(o, o =>
            {
                return error.As(ConnectExFunc(o.fd.Sysfd, o.sa, null, 0L, null, ref o.o));
            });
            return error.As(err);
        }

        private static (@string, error) acceptOne(this ref FD fd, syscall.Handle s, slice<syscall.RawSockaddrAny> rawsa, ref operation o)
        { 
            // Submit accept request.
            o.handle = s;
            o.rsan = int32(@unsafe.Sizeof(rawsa[0L]));
            var (_, err) = rsrv.ExecIO(o, o =>
            {
                return AcceptFunc(o.fd.Sysfd, o.handle, (byte.Value)(@unsafe.Pointer(ref rawsa[0L])), 0L, uint32(o.rsan), uint32(o.rsan), ref o.qty, ref o.o);
            });
            if (err != null)
            {
                CloseFunc(s);
                return ("acceptex", err);
            } 

            // Inherit properties of the listening socket.
            err = syscall.Setsockopt(s, syscall.SOL_SOCKET, syscall.SO_UPDATE_ACCEPT_CONTEXT, (byte.Value)(@unsafe.Pointer(ref fd.Sysfd)), int32(@unsafe.Sizeof(fd.Sysfd)));
            if (err != null)
            {
                CloseFunc(s);
                return ("setsockopt", err);
            }
            return ("", null);
        }

        // Accept handles accepting a socket. The sysSocket parameter is used
        // to allocate the net socket.
        private static (syscall.Handle, slice<syscall.RawSockaddrAny>, uint, @string, error) Accept(this ref FD _fd, Func<(syscall.Handle, error)> sysSocket) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (syscall.InvalidHandle, null, 0L, "", err);
                }

            }
            defer(fd.readUnlock());

            var o = ref fd.rop;
            array<syscall.RawSockaddrAny> rawsa = new array<syscall.RawSockaddrAny>(2L);
            while (true)
            {
                var (s, err) = sysSocket();
                if (err != null)
                {
                    return (syscall.InvalidHandle, null, 0L, "", err);
                }
                var (errcall, err) = fd.acceptOne(s, rawsa[..], o);
                if (err == null)
                {
                    return (s, rawsa[..], uint32(o.rsan), "", null);
                } 

                // Sometimes we see WSAECONNRESET and ERROR_NETNAME_DELETED is
                // returned here. These happen if connection reset is received
                // before AcceptEx could complete. These errors relate to new
                // connection, not to AcceptEx, so ignore broken connection and
                // try AcceptEx again for more connections.
                syscall.Errno (errno, ok) = err._<syscall.Errno>();
                if (!ok)
                {
                    return (syscall.InvalidHandle, null, 0L, errcall, err);
                }

                if (errno == syscall.ERROR_NETNAME_DELETED || errno == syscall.WSAECONNRESET)                 else 
                    return (syscall.InvalidHandle, null, 0L, errcall, err);
                            }

        });

        // Seek wraps syscall.Seek.
        private static (long, error) Seek(this ref FD _fd, long offset, long whence) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());

            fd.l.Lock();
            defer(fd.l.Unlock());

            return syscall.Seek(fd.Sysfd, offset, whence);
        });

        // FindNextFile wraps syscall.FindNextFile.
        private static error FindNextFile(this ref FD _fd, ref syscall.Win32finddata _data) => func(_fd, _data, (ref FD fd, ref syscall.Win32finddata data, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.FindNextFile(fd.Sysfd, data));
        });

        // Fchdir wraps syscall.Fchdir.
        private static error Fchdir(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.Fchdir(fd.Sysfd));
        });

        // GetFileType wraps syscall.GetFileType.
        private static (uint, error) GetFileType(this ref FD _fd) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, err);
                }

            }
            defer(fd.decref());
            return syscall.GetFileType(fd.Sysfd);
        });

        // GetFileInformationByHandle wraps GetFileInformationByHandle.
        private static error GetFileInformationByHandle(this ref FD _fd, ref syscall.ByHandleFileInformation _data) => func(_fd, _data, (ref FD fd, ref syscall.ByHandleFileInformation data, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            return error.As(syscall.GetFileInformationByHandle(fd.Sysfd, data));
        });

        // RawControl invokes the user-defined function f for a non-IO
        // operation.
        private static error RawControl(this ref FD _fd, Action<System.UIntPtr> f) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            defer(fd.decref());
            f(uintptr(fd.Sysfd));
            return error.As(null);
        });

        // RawRead invokes the user-defined function f for a read operation.
        private static error RawRead(this ref FD fd, Func<System.UIntPtr, bool> f)
        {
            return error.As(errors.New("not implemented"));
        }

        // RawWrite invokes the user-defined function f for a write operation.
        private static error RawWrite(this ref FD fd, Func<System.UIntPtr, bool> f)
        {
            return error.As(errors.New("not implemented"));
        }

        private static (unsafe.Pointer, int, error) sockaddrToRaw(syscall.Sockaddr sa)
        {
            switch (sa.type())
            {
                case ref syscall.SockaddrInet4 sa:
                    syscall.RawSockaddrInet4 raw = default;
                    raw.Family = syscall.AF_INET;
                    ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref raw.Port));
                    p[0L] = byte(sa.Port >> (int)(8L));
                    p[1L] = byte(sa.Port);
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < len(sa.Addr); i++)
                        {
                            raw.Addr[i] = sa.Addr[i];
                        }


                        i = i__prev1;
                    }
                    return (@unsafe.Pointer(ref raw), int32(@unsafe.Sizeof(raw)), null);
                    break;
                case ref syscall.SockaddrInet6 sa:
                    raw = default;
                    raw.Family = syscall.AF_INET6;
                    p = new ptr<ref array<byte>>(@unsafe.Pointer(ref raw.Port));
                    p[0L] = byte(sa.Port >> (int)(8L));
                    p[1L] = byte(sa.Port);
                    raw.Scope_id = sa.ZoneId;
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < len(sa.Addr); i++)
                        {
                            raw.Addr[i] = sa.Addr[i];
                        }


                        i = i__prev1;
                    }
                    return (@unsafe.Pointer(ref raw), int32(@unsafe.Sizeof(raw)), null);
                    break;
                default:
                {
                    var sa = sa.type();
                    return (null, 0L, syscall.EWINDOWS);
                    break;
                }
            }
        }

        // ReadMsg wraps the WSARecvMsg network call.
        private static (long, long, long, syscall.Sockaddr, error) ReadMsg(this ref FD _fd, slice<byte> p, slice<byte> oob) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, 0L, 0L, null, err);
                }

            }
            defer(fd.readUnlock());

            var o = ref fd.rop;
            o.InitMsg(p, oob);
            o.rsa = @new<syscall.RawSockaddrAny>();
            o.msg.Name = o.rsa;
            o.msg.Namelen = int32(@unsafe.Sizeof(o.rsa.Value));
            var (n, err) = rsrv.ExecIO(o, o =>
            {
                return windows.WSARecvMsg(o.fd.Sysfd, ref o.msg, ref o.qty, ref o.o, null);
            });
            err = fd.eofError(n, err);
            syscall.Sockaddr sa = default;
            if (err == null)
            {
                sa, err = o.rsa.Sockaddr();
            }
            return (n, int(o.msg.Control.Len), int(o.msg.Flags), sa, err);
        });

        // WriteMsg wraps the WSASendMsg network call.
        private static (long, long, error) WriteMsg(this ref FD _fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => func(_fd, (ref FD fd, Defer defer, Panic _, Recover __) =>
        {
            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, 0L, err);
                }

            }
            defer(fd.writeUnlock());

            var o = ref fd.wop;
            o.InitMsg(p, oob);
            if (sa != null)
            {
                var (rsa, len, err) = sockaddrToRaw(sa);
                if (err != null)
                {
                    return (0L, 0L, err);
                }
                o.msg.Name = (syscall.RawSockaddrAny.Value)(rsa);
                o.msg.Namelen = len;
            }
            var (n, err) = wsrv.ExecIO(o, o =>
            {
                return windows.WSASendMsg(o.fd.Sysfd, ref o.msg, 0L, ref o.qty, ref o.o, null);
            });
            return (n, int(o.msg.Control.Len), err);
        });
    }
}}
