// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:51:12 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\fd_windows.go
using errors = go.errors_package;
using race = go.@internal.race_package;
using windows = go.@internal.syscall.windows_package;
using io = go.io_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private static error initErr = default!;        private static ulong ioSync = default;

        // This package uses the SetFileCompletionNotificationModes Windows
        // API to skip calling GetQueuedCompletionStatus if an IO operation
        // completes synchronously. There is a known bug where
        // SetFileCompletionNotificationModes crashes on some systems (see
        // https://support.microsoft.com/kb/2568167 for details).

        private static bool useSetFileCompletionNotificationModes = default; // determines is SetFileCompletionNotificationModes is present and safe to use

        // checkSetFileCompletionNotificationModes verifies that
        // SetFileCompletionNotificationModes Windows API is present
        // on the system and is safe to use.
        // See https://support.microsoft.com/kb/2568167 for details.
        private static void checkSetFileCompletionNotificationModes()
        {
            var err = syscall.LoadSetFileCompletionNotificationModes();
            if (err != null)
            {
                return ;
            }

            array<int> protos = new array<int>(new int[] { syscall.IPPROTO_TCP, 0 });
            array<syscall.WSAProtocolInfo> buf = new array<syscall.WSAProtocolInfo>(32L);
            ref var len = ref heap(uint32(@unsafe.Sizeof(buf)), out ptr<var> _addr_len);
            var (n, err) = syscall.WSAEnumProtocols(_addr_protos[0L], _addr_buf[0L], _addr_len);
            if (err != null)
            {
                return ;
            }

            for (var i = int32(0L); i < n; i++)
            {
                if (buf[i].ServiceFlags1 & syscall.XP1_IFS_HANDLES == 0L)
                {
                    return ;
                }

            }

            useSetFileCompletionNotificationModes = true;

        }

        private static void init()
        {
            ref syscall.WSAData d = ref heap(out ptr<syscall.WSAData> _addr_d);
            var e = syscall.WSAStartup(uint32(0x202UL), _addr_d);
            if (e != null)
            {
                initErr = e;
            }

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
            public syscall.WSABuf buf;
            public windows.WSAMsg msg;
            public syscall.Sockaddr sa;
            public ptr<syscall.RawSockaddrAny> rsa;
            public int rsan;
            public syscall.Handle handle;
            public uint flags;
            public slice<syscall.WSABuf> bufs;
        }

        private static void InitBuf(this ptr<operation> _addr_o, slice<byte> buf)
        {
            ref operation o = ref _addr_o.val;

            o.buf.Len = uint32(len(buf));
            o.buf.Buf = null;
            if (len(buf) != 0L)
            {
                o.buf.Buf = _addr_buf[0L];
            }

        }

        private static void InitBufs(this ptr<operation> _addr_o, ptr<slice<slice<byte>>> _addr_buf)
        {
            ref operation o = ref _addr_o.val;
            ref slice<slice<byte>> buf = ref _addr_buf.val;

            if (o.bufs == null)
            {
                o.bufs = make_slice<syscall.WSABuf>(0L, len(buf));
            }
            else
            {
                o.bufs = o.bufs[..0L];
            }

            foreach (var (_, b) in buf)
            {
                if (len(b) == 0L)
                {
                    o.bufs = append(o.bufs, new syscall.WSABuf());
                    continue;
                }

                while (len(b) > maxRW)
                {
                    o.bufs = append(o.bufs, new syscall.WSABuf(Len:maxRW,Buf:&b[0]));
                    b = b[maxRW..];
                }

                if (len(b) > 0L)
                {
                    o.bufs = append(o.bufs, new syscall.WSABuf(Len:uint32(len(b)),Buf:&b[0]));
                }

            }

        }

        // ClearBufs clears all pointers to Buffers parameter captured
        // by InitBufs, so it can be released by garbage collector.
        private static void ClearBufs(this ptr<operation> _addr_o)
        {
            ref operation o = ref _addr_o.val;

            foreach (var (i) in o.bufs)
            {
                o.bufs[i].Buf = null;
            }
            o.bufs = o.bufs[..0L];

        }

        private static void InitMsg(this ptr<operation> _addr_o, slice<byte> p, slice<byte> oob)
        {
            ref operation o = ref _addr_o.val;

            o.InitBuf(p);
            o.msg.Buffers = _addr_o.buf;
            o.msg.BufferCount = 1L;

            o.msg.Name = null;
            o.msg.Namelen = 0L;

            o.msg.Flags = 0L;
            o.msg.Control.Len = uint32(len(oob));
            o.msg.Control.Buf = null;
            if (len(oob) != 0L)
            {
                o.msg.Control.Buf = _addr_oob[0L];
            }

        }

        // execIO executes a single IO operation o. It submits and cancels
        // IO in the current thread for systems where Windows CancelIoEx API
        // is available. Alternatively, it passes the request onto
        // runtime netpoll and waits for completion or cancels request.
        private static (long, error) execIO(ptr<operation> _addr_o, Func<ptr<operation>, error> submit) => func((_, panic, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref operation o = ref _addr_o.val;

            if (o.fd.pd.runtimeCtx == 0L)
            {
                return (0L, error.As(errors.New("internal error: polling on unsupported descriptor type"))!);
            }

            var fd = o.fd; 
            // Notify runtime netpoll about starting IO.
            var err = fd.pd.prepare(int(o.mode), fd.isFile);
            if (err != null)
            {
                return (0L, error.As(err)!);
            } 
            // Start IO.
            err = submit(o);

            if (err == null) 
                // IO completed immediately
                if (o.fd.skipSyncNotif)
                { 
                    // No completion message will follow, so return immediately.
                    return (int(o.qty), error.As(null!)!);

                } 
                // Need to get our completion message anyway.
            else if (err == syscall.ERROR_IO_PENDING) 
                // IO started, and we have to wait for its completion.
                err = null;
            else 
                return (0L, error.As(err)!);
            // Wait for our request to complete.
            err = fd.pd.wait(int(o.mode), fd.isFile);
            if (err == null)
            { 
                // All is good. Extract our IO results and return.
                if (o.errno != 0L)
                {
                    err = syscall.Errno(o.errno); 
                    // More data available. Return back the size of received data.
                    if (err == syscall.ERROR_MORE_DATA || err == windows.WSAEMSGSIZE)
                    {
                        return (int(o.qty), error.As(err)!);
                    }

                    return (0L, error.As(err)!);

                }

                return (int(o.qty), error.As(null!)!);

            } 
            // IO is interrupted by "close" or "timeout"
            var netpollErr = err;

            if (netpollErr == ErrNetClosing || netpollErr == ErrFileClosing || netpollErr == ErrDeadlineExceeded)             else 
                panic("unexpected runtime.netpoll error: " + netpollErr.Error());
            // Cancel our request.
            err = syscall.CancelIoEx(fd.Sysfd, _addr_o.o); 
            // Assuming ERROR_NOT_FOUND is returned, if IO is completed.
            if (err != null && err != syscall.ERROR_NOT_FOUND)
            { 
                // TODO(brainman): maybe do something else, but panic.
                panic(err);

            } 
            // Wait for cancellation to complete.
            fd.pd.waitCanceled(int(o.mode));
            if (o.errno != 0L)
            {
                err = syscall.Errno(o.errno);
                if (err == syscall.ERROR_OPERATION_ABORTED)
                { // IO Canceled
                    err = netpollErr;

                }

                return (0L, error.As(err)!);

            } 
            // We issued a cancellation request. But, it seems, IO operation succeeded
            // before the cancellation request run. We need to treat the IO operation as
            // succeeded (the bytes are actually sent/recv from network).
            return (int(o.qty), error.As(null!)!);

        });

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
            public bool ZeroReadIsEOF; // Whether this is a file rather than a network socket.
            public bool isFile; // The kind of this file.
            public fileKind kind;
        }

        // fileKind describes the kind of file.
        private partial struct fileKind // : byte
        {
        }

        private static readonly fileKind kindNet = (fileKind)iota;
        private static readonly var kindFile = 0;
        private static readonly var kindConsole = 1;
        private static readonly var kindDir = 2;
        private static readonly var kindPipe = 3;


        // logInitFD is set by tests to enable file descriptor initialization logging.
        private static Action<@string, ptr<FD>, error> logInitFD = default;

        // Init initializes the FD. The Sysfd field should already be set.
        // This can be called multiple times on a single FD.
        // The net argument is a network name from the net package (e.g., "tcp"),
        // or "file" or "console" or "dir".
        // Set pollable to true if fd should be managed by runtime netpoll.
        private static (@string, error) Init(this ptr<FD> _addr_fd, @string net, bool pollable)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            if (initErr != null)
            {
                return ("", error.As(initErr)!);
            }

            switch (net)
            {
                case "file": 
                    fd.kind = kindFile;
                    break;
                case "console": 
                    fd.kind = kindConsole;
                    break;
                case "dir": 
                    fd.kind = kindDir;
                    break;
                case "pipe": 
                    fd.kind = kindPipe;
                    break;
                case "tcp": 

                case "tcp4": 

                case "tcp6": 

                case "udp": 

                case "udp4": 

                case "udp6": 

                case "ip": 

                case "ip4": 

                case "ip6": 

                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    fd.kind = kindNet;
                    break;
                default: 
                    return ("", error.As(errors.New("internal error: unknown network type " + net))!);
                    break;
            }
            fd.isFile = fd.kind != kindNet;

            error err = default!;
            if (pollable)
            { 
                // Only call init for a network socket.
                // This means that we don't add files to the runtime poller.
                // Adding files to the runtime poller can confuse matters
                // if the user is doing their own overlapped I/O.
                // See issue #21172.
                //
                // In general the code below avoids calling the execIO
                // function for non-network sockets. If some method does
                // somehow call execIO, then execIO, and therefore the
                // calling method, will return an error, because
                // fd.pd.runtimeCtx will be 0.
                err = error.As(fd.pd.init(fd))!;

            }

            if (logInitFD != null)
            {
                logInitFD(net, fd, err);
            }

            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (pollable && useSetFileCompletionNotificationModes)
            { 
                // We do not use events, so we can skip them always.
                var flags = uint8(syscall.FILE_SKIP_SET_EVENT_ON_HANDLE); 
                // It's not safe to skip completion notifications for UDP:
                // https://docs.microsoft.com/en-us/archive/blogs/winserverperformance/designing-applications-for-high-performance-part-iii
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
                    ref var ret = ref heap(uint32(0L), out ptr<var> _addr_ret);
                    ref var flag = ref heap(uint32(0L), out ptr<var> _addr_flag);
                    var size = uint32(@unsafe.Sizeof(flag));
                    err = syscall.WSAIoctl(fd.Sysfd, syscall.SIO_UDP_CONNRESET, (byte.val)(@unsafe.Pointer(_addr_flag)), size, null, 0L, _addr_ret, null, 0L);
                    if (err != null)
                    {
                        return ("wsaioctl", error.As(err)!);
                    }

                    break;
            }
            fd.rop.mode = 'r';
            fd.wop.mode = 'w';
            fd.rop.fd = fd;
            fd.wop.fd = fd;
            fd.rop.runtimeCtx = fd.pd.runtimeCtx;
            fd.wop.runtimeCtx = fd.pd.runtimeCtx;
            return ("", error.As(null!)!);

        }

        private static error destroy(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (fd.Sysfd == syscall.InvalidHandle)
            {
                return error.As(syscall.EINVAL)!;
            } 
            // Poller may want to unregister fd in readiness notification mechanism,
            // so this must be executed before fd.CloseFunc.
            fd.pd.close();
            error err = default!;

            if (fd.kind == kindNet) 
                // The net package uses the CloseFunc variable for testing.
                err = error.As(CloseFunc(fd.Sysfd))!;
            else if (fd.kind == kindDir) 
                err = error.As(syscall.FindClose(fd.Sysfd))!;
            else 
                err = error.As(syscall.CloseHandle(fd.Sysfd))!;
                        fd.Sysfd = syscall.InvalidHandle;
            runtime_Semrelease(_addr_fd.csema);
            return error.As(err)!;

        }

        // Close closes the FD. The underlying file descriptor is closed by
        // the destroy method when there are no remaining references.
        private static error Close(this ptr<FD> _addr_fd)
        {
            ref FD fd = ref _addr_fd.val;

            if (!fd.fdmu.increfAndClose())
            {
                return error.As(errClosing(fd.isFile))!;
            }

            if (fd.kind == kindPipe)
            {
                syscall.CancelIoEx(fd.Sysfd, null);
            } 
            // unblock pending reader and writer
            fd.pd.evict();
            var err = fd.decref(); 
            // Wait until the descriptor is closed. If this was the only
            // reference, it is already closed.
            runtime_Semacquire(_addr_fd.csema);
            return error.As(err)!;

        }

        // Windows ReadFile and WSARecv use DWORD (uint32) parameter to pass buffer length.
        // This prevents us reading blocks larger than 4GB.
        // See golang.org/issue/26923.
        private static readonly long maxRW = (long)1L << (int)(30L); // 1GB is large enough and keeps subsequent reads aligned

        // Read implements io.Reader.
 // 1GB is large enough and keeps subsequent reads aligned

        // Read implements io.Reader.
        private static (long, error) Read(this ptr<FD> _addr_fd, slice<byte> buf) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err__prev1 = err;

                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(fd.readUnlock());

            if (len(buf) > maxRW)
            {
                buf = buf[..maxRW];
            }

            long n = default;
            err = default!;
            if (fd.isFile)
            {
                fd.l.Lock();
                defer(fd.l.Unlock());

                if (fd.kind == kindConsole) 
                    n, err = fd.readConsole(buf);
                else 
                    n, err = syscall.Read(fd.Sysfd, buf);
                    if (fd.kind == kindPipe && err == syscall.ERROR_OPERATION_ABORTED)
                    { 
                        // Close uses CancelIoEx to interrupt concurrent I/O for pipes.
                        // If the fd is a pipe and the Read was interrupted by CancelIoEx,
                        // we assume it is interrupted by Close.
                        err = ErrFileClosing;

                    }

                                if (err != null)
                {
                    n = 0L;
                }

            }
            else
            {
                var o = _addr_fd.rop;
                o.InitBuf(buf);
                n, err = execIO(_addr_o, o =>
                {
                    return syscall.WSARecv(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, _addr_o.flags, _addr_o.o, null);
                });
                if (race.Enabled)
                {
                    race.Acquire(@unsafe.Pointer(_addr_ioSync));
                }

            }

            if (len(buf) != 0L)
            {
                err = fd.eofError(n, err);
            }

            return (n, error.As(err)!);

        });

        public static var ReadConsole = syscall.ReadConsole; // changed for testing

        // readConsole reads utf16 characters from console File,
        // encodes them into utf8 and stores them in buffer b.
        // It returns the number of utf8 bytes read and an error, if any.
        private static (long, error) readConsole(this ptr<FD> _addr_fd, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            if (len(b) == 0L)
            {
                return (0L, error.As(null!)!);
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

                ref uint nw = ref heap(out ptr<uint> _addr_nw);
                var err = ReadConsole(fd.Sysfd, _addr_fd.readuint16[..len(fd.readuint16) + 1L][len(fd.readuint16)], uint32(n), _addr_nw, null);
                if (err != null)
                {
                    return (0L, error.As(err)!);
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
            return (i, error.As(null!)!);

        }

        // Pread emulates the Unix pread system call.
        private static (long, error) Pread(this ptr<FD> _addr_fd, slice<byte> b, long off) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
 
            // Call incref, not readLock, because since pread specifies the
            // offset it is independent from other reads.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.decref());

            if (len(b) > maxRW)
            {
                b = b[..maxRW];
            }

            fd.l.Lock();
            defer(fd.l.Unlock());
            var (curoffset, e) = syscall.Seek(fd.Sysfd, 0L, io.SeekCurrent);
            if (e != null)
            {
                return (0L, error.As(e)!);
            }

            defer(syscall.Seek(fd.Sysfd, curoffset, io.SeekStart));
            ref syscall.Overlapped o = ref heap(new syscall.Overlapped(OffsetHigh:uint32(off>>32),Offset:uint32(off),), out ptr<syscall.Overlapped> _addr_o);
            ref uint done = ref heap(out ptr<uint> _addr_done);
            e = syscall.ReadFile(fd.Sysfd, b, _addr_done, _addr_o);
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

            return (int(done), error.As(e)!);

        });

        // ReadFrom wraps the recvfrom network call.
        private static (long, syscall.Sockaddr, error) ReadFrom(this ptr<FD> _addr_fd, slice<byte> buf) => func((defer, _, __) =>
        {
            long _p0 = default;
            syscall.Sockaddr _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            if (len(buf) == 0L)
            {
                return (0L, null, error.As(null!)!);
            }

            if (len(buf) > maxRW)
            {
                buf = buf[..maxRW];
            }

            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, null, error.As(err)!);
                }

            }

            defer(fd.readUnlock());
            var o = _addr_fd.rop;
            o.InitBuf(buf);
            var (n, err) = execIO(_addr_o, o =>
            {
                if (o.rsa == null)
                {
                    o.rsa = @new<syscall.RawSockaddrAny>();
                }

                o.rsan = int32(@unsafe.Sizeof(o.rsa.val));
                return syscall.WSARecvFrom(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, _addr_o.flags, o.rsa, _addr_o.rsan, _addr_o.o, null);

            });
            err = fd.eofError(n, err);
            if (err != null)
            {
                return (n, null, error.As(err)!);
            }

            var (sa, _) = o.rsa.Sockaddr();
            return (n, sa, error.As(null!)!);

        });

        // Write implements io.Writer.
        private static (long, error) Write(this ptr<FD> _addr_fd, slice<byte> buf) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err__prev1 = err;

                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(fd.writeUnlock());
            if (fd.isFile)
            {
                fd.l.Lock();
                defer(fd.l.Unlock());
            }

            long ntotal = 0L;
            while (len(buf) > 0L)
            {
                var b = buf;
                if (len(b) > maxRW)
                {
                    b = b[..maxRW];
                }

                long n = default;
                err = default!;
                if (fd.isFile)
                {

                    if (fd.kind == kindConsole) 
                        n, err = fd.writeConsole(b);
                    else 
                        n, err = syscall.Write(fd.Sysfd, b);
                        if (fd.kind == kindPipe && err == syscall.ERROR_OPERATION_ABORTED)
                        { 
                            // Close uses CancelIoEx to interrupt concurrent I/O for pipes.
                            // If the fd is a pipe and the Write was interrupted by CancelIoEx,
                            // we assume it is interrupted by Close.
                            err = ErrFileClosing;

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
                        race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
                    }

                    var o = _addr_fd.wop;
                    o.InitBuf(b);
                    n, err = execIO(_addr_o, o =>
                    {
                        return syscall.WSASend(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, 0L, _addr_o.o, null);
                    });

                }

                ntotal += n;
                if (err != null)
                {
                    return (ntotal, error.As(err)!);
                }

                buf = buf[n..];

            }

            return (ntotal, error.As(null!)!);

        });

        // writeConsole writes len(b) bytes to the console File.
        // It returns the number of bytes written and an error, if any.
        private static (long, error) writeConsole(this ptr<FD> _addr_fd, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

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
            const long maxWrite = (long)16000L;

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
                    ref uint written = ref heap(out ptr<uint> _addr_written);
                    var err = syscall.WriteConsole(fd.Sysfd, _addr_uint16s[0L], uint32(len(uint16s)), _addr_written, null);
                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    }

                    uint16s = uint16s[written..];

                }


            }

            return (n, error.As(null!)!);

        }

        // Pwrite emulates the Unix pwrite system call.
        private static (long, error) Pwrite(this ptr<FD> _addr_fd, slice<byte> buf, long off) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
 
            // Call incref, not writeLock, because since pwrite specifies the
            // offset it is independent from other writes.
            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.decref());

            fd.l.Lock();
            defer(fd.l.Unlock());
            var (curoffset, e) = syscall.Seek(fd.Sysfd, 0L, io.SeekCurrent);
            if (e != null)
            {
                return (0L, error.As(e)!);
            }

            defer(syscall.Seek(fd.Sysfd, curoffset, io.SeekStart));

            long ntotal = 0L;
            while (len(buf) > 0L)
            {
                var b = buf;
                if (len(b) > maxRW)
                {
                    b = b[..maxRW];
                }

                ref uint n = ref heap(out ptr<uint> _addr_n);
                ref syscall.Overlapped o = ref heap(new syscall.Overlapped(OffsetHigh:uint32(off>>32),Offset:uint32(off),), out ptr<syscall.Overlapped> _addr_o);
                e = syscall.WriteFile(fd.Sysfd, b, _addr_n, _addr_o);
                ntotal += int(n);
                if (e != null)
                {
                    return (ntotal, error.As(e)!);
                }

                buf = buf[n..];
                off += int64(n);

            }

            return (ntotal, error.As(null!)!);

        });

        // Writev emulates the Unix writev system call.
        private static (long, error) Writev(this ptr<FD> _addr_fd, ptr<slice<slice<byte>>> _addr_buf) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
            ref slice<slice<byte>> buf = ref _addr_buf.val;

            if (len(buf) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.writeUnlock());
            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            var o = _addr_fd.wop;
            o.InitBufs(buf);
            var (n, err) = execIO(_addr_o, o =>
            {
                return syscall.WSASend(o.fd.Sysfd, _addr_o.bufs[0L], uint32(len(o.bufs)), _addr_o.qty, 0L, _addr_o.o, null);
            });
            o.ClearBufs();
            TestHookDidWritev(n);
            consume(buf, int64(n));
            return (int64(n), error.As(err)!);

        });

        // WriteTo wraps the sendto network call.
        private static (long, error) WriteTo(this ptr<FD> _addr_fd, slice<byte> buf, syscall.Sockaddr sa) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.writeUnlock());

            if (len(buf) == 0L)
            { 
                // handle zero-byte payload
                var o = _addr_fd.wop;
                o.InitBuf(buf);
                o.sa = sa;
                var (n, err) = execIO(_addr_o, o =>
                {
                    return syscall.WSASendto(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, 0L, o.sa, _addr_o.o, null);
                });
                return (n, error.As(err)!);

            }

            long ntotal = 0L;
            while (len(buf) > 0L)
            {
                var b = buf;
                if (len(b) > maxRW)
                {
                    b = b[..maxRW];
                }

                o = _addr_fd.wop;
                o.InitBuf(b);
                o.sa = sa;
                (n, err) = execIO(_addr_o, o =>
                {
                    return syscall.WSASendto(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, 0L, o.sa, _addr_o.o, null);
                });
                ntotal += int(n);
                if (err != null)
                {
                    return (ntotal, error.As(err)!);
                }

                buf = buf[n..];

            }

            return (ntotal, error.As(null!)!);

        });

        // Call ConnectEx. This doesn't need any locking, since it is only
        // called when the descriptor is first created. This is here rather
        // than in the net package so that it can use fd.wop.
        private static error ConnectEx(this ptr<FD> _addr_fd, syscall.Sockaddr ra)
        {
            ref FD fd = ref _addr_fd.val;

            var o = _addr_fd.wop;
            o.sa = ra;
            var (_, err) = execIO(_addr_o, o =>
            {
                return error.As(ConnectExFunc(o.fd.Sysfd, o.sa, null, 0L, null, _addr_o.o))!;
            });
            return error.As(err)!;

        }

        private static (@string, error) acceptOne(this ptr<FD> _addr_fd, syscall.Handle s, slice<syscall.RawSockaddrAny> rawsa, ptr<operation> _addr_o)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;
            ref operation o = ref _addr_o.val;
 
            // Submit accept request.
            o.handle = s;
            o.rsan = int32(@unsafe.Sizeof(rawsa[0L]));
            var (_, err) = execIO(_addr_o, o =>
            {
                return AcceptFunc(o.fd.Sysfd, o.handle, (byte.val)(@unsafe.Pointer(_addr_rawsa[0L])), 0L, uint32(o.rsan), uint32(o.rsan), _addr_o.qty, _addr_o.o);
            });
            if (err != null)
            {
                CloseFunc(s);
                return ("acceptex", error.As(err)!);
            } 

            // Inherit properties of the listening socket.
            err = syscall.Setsockopt(s, syscall.SOL_SOCKET, syscall.SO_UPDATE_ACCEPT_CONTEXT, (byte.val)(@unsafe.Pointer(_addr_fd.Sysfd)), int32(@unsafe.Sizeof(fd.Sysfd)));
            if (err != null)
            {
                CloseFunc(s);
                return ("setsockopt", error.As(err)!);
            }

            return ("", error.As(null!)!);

        }

        // Accept handles accepting a socket. The sysSocket parameter is used
        // to allocate the net socket.
        private static (syscall.Handle, slice<syscall.RawSockaddrAny>, uint, @string, error) Accept(this ptr<FD> _addr_fd, Func<(syscall.Handle, error)> sysSocket) => func((defer, _, __) =>
        {
            syscall.Handle _p0 = default;
            slice<syscall.RawSockaddrAny> _p0 = default;
            uint _p0 = default;
            @string _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (syscall.InvalidHandle, null, 0L, "", error.As(err)!);
                }

            }

            defer(fd.readUnlock());

            var o = _addr_fd.rop;
            array<syscall.RawSockaddrAny> rawsa = new array<syscall.RawSockaddrAny>(2L);
            while (true)
            {
                var (s, err) = sysSocket();
                if (err != null)
                {
                    return (syscall.InvalidHandle, null, 0L, "", error.As(err)!);
                }

                var (errcall, err) = fd.acceptOne(s, rawsa[..], o);
                if (err == null)
                {
                    return (s, rawsa[..], uint32(o.rsan), "", error.As(null!)!);
                } 

                // Sometimes we see WSAECONNRESET and ERROR_NETNAME_DELETED is
                // returned here. These happen if connection reset is received
                // before AcceptEx could complete. These errors relate to new
                // connection, not to AcceptEx, so ignore broken connection and
                // try AcceptEx again for more connections.
                syscall.Errno (errno, ok) = err._<syscall.Errno>();
                if (!ok)
                {
                    return (syscall.InvalidHandle, null, 0L, errcall, error.As(err)!);
                }


                if (errno == syscall.ERROR_NETNAME_DELETED || errno == syscall.WSAECONNRESET)                 else 
                    return (syscall.InvalidHandle, null, 0L, errcall, error.As(err)!);
                
            }


        });

        // Seek wraps syscall.Seek.
        private static (long, error) Seek(this ptr<FD> _addr_fd, long offset, long whence) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.decref());

            fd.l.Lock();
            defer(fd.l.Unlock());

            return syscall.Seek(fd.Sysfd, offset, whence);

        });

        // FindNextFile wraps syscall.FindNextFile.
        private static error FindNextFile(this ptr<FD> _addr_fd, ptr<syscall.Win32finddata> _addr_data) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;
            ref syscall.Win32finddata data = ref _addr_data.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.decref());
            return error.As(syscall.FindNextFile(fd.Sysfd, data))!;

        });

        // Fchdir wraps syscall.Fchdir.
        private static error Fchdir(this ptr<FD> _addr_fd) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.decref());
            return error.As(syscall.Fchdir(fd.Sysfd))!;

        });

        // GetFileType wraps syscall.GetFileType.
        private static (uint, error) GetFileType(this ptr<FD> _addr_fd) => func((defer, _, __) =>
        {
            uint _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            defer(fd.decref());
            return syscall.GetFileType(fd.Sysfd);

        });

        // GetFileInformationByHandle wraps GetFileInformationByHandle.
        private static error GetFileInformationByHandle(this ptr<FD> _addr_fd, ptr<syscall.ByHandleFileInformation> _addr_data) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;
            ref syscall.ByHandleFileInformation data = ref _addr_data.val;

            {
                var err = fd.incref();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.decref());
            return error.As(syscall.GetFileInformationByHandle(fd.Sysfd, data))!;

        });

        // RawRead invokes the user-defined function f for a read operation.
        private static error RawRead(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.readLock();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.readUnlock());
            while (true)
            {
                if (f(uintptr(fd.Sysfd)))
                {
                    return error.As(null!)!;
                } 

                // Use a zero-byte read as a way to get notified when this
                // socket is readable. h/t https://stackoverflow.com/a/42019668/332798
                var o = _addr_fd.rop;
                o.InitBuf(null);
                if (!fd.IsStream)
                {
                    o.flags |= windows.MSG_PEEK;
                }

                var (_, err) = execIO(_addr_o, o =>
                {
                    return error.As(syscall.WSARecv(o.fd.Sysfd, _addr_o.buf, 1L, _addr_o.qty, _addr_o.flags, _addr_o.o, null))!;
                });
                if (err == windows.WSAEMSGSIZE)
                { 
                    // expected with a 0-byte peek, ignore.
                }
                else if (err != null)
                {
                    return error.As(err)!;
                }

            }


        });

        // RawWrite invokes the user-defined function f for a write operation.
        private static error RawWrite(this ptr<FD> _addr_fd, Func<System.UIntPtr, bool> f) => func((defer, _, __) =>
        {
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            defer(fd.writeUnlock());

            if (f(uintptr(fd.Sysfd)))
            {
                return error.As(null!)!;
            } 

            // TODO(tmm1): find a way to detect socket writability
            return error.As(syscall.EWINDOWS)!;

        });

        private static (unsafe.Pointer, int, error) sockaddrToRaw(syscall.Sockaddr sa)
        {
            unsafe.Pointer _p0 = default;
            int _p0 = default;
            error _p0 = default!;

            switch (sa.type())
            {
                case ptr<syscall.SockaddrInet4> sa:
                    ref syscall.RawSockaddrInet4 raw = ref heap(out ptr<syscall.RawSockaddrInet4> _addr_raw);
                    raw.Family = syscall.AF_INET;
                    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_raw.Port));
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
                    return (@unsafe.Pointer(_addr_raw), int32(@unsafe.Sizeof(raw)), error.As(null!)!);
                    break;
                case ptr<syscall.SockaddrInet6> sa:
                    raw = default;
                    raw.Family = syscall.AF_INET6;
                    p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_raw.Port));
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
                    return (@unsafe.Pointer(_addr_raw), int32(@unsafe.Sizeof(raw)), error.As(null!)!);
                    break;
                default:
                {
                    var sa = sa.type();
                    return (null, 0L, error.As(syscall.EWINDOWS)!);
                    break;
                }
            }

        }

        // ReadMsg wraps the WSARecvMsg network call.
        private static (long, long, long, syscall.Sockaddr, error) ReadMsg(this ptr<FD> _addr_fd, slice<byte> p, slice<byte> oob) => func((defer, _, __) =>
        {
            long _p0 = default;
            long _p0 = default;
            long _p0 = default;
            syscall.Sockaddr _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            {
                var err = fd.readLock();

                if (err != null)
                {
                    return (0L, 0L, 0L, null, error.As(err)!);
                }

            }

            defer(fd.readUnlock());

            if (len(p) > maxRW)
            {
                p = p[..maxRW];
            }

            var o = _addr_fd.rop;
            o.InitMsg(p, oob);
            o.rsa = @new<syscall.RawSockaddrAny>();
            o.msg.Name = (syscall.Pointer)(@unsafe.Pointer(o.rsa));
            o.msg.Namelen = int32(@unsafe.Sizeof(o.rsa.val));
            var (n, err) = execIO(_addr_o, o =>
            {
                return windows.WSARecvMsg(o.fd.Sysfd, _addr_o.msg, _addr_o.qty, _addr_o.o, null);
            });
            err = fd.eofError(n, err);
            syscall.Sockaddr sa = default;
            if (err == null)
            {
                sa, err = o.rsa.Sockaddr();
            }

            return (n, int(o.msg.Control.Len), int(o.msg.Flags), sa, error.As(err)!);

        });

        // WriteMsg wraps the WSASendMsg network call.
        private static (long, long, error) WriteMsg(this ptr<FD> _addr_fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa) => func((defer, _, __) =>
        {
            long _p0 = default;
            long _p0 = default;
            error _p0 = default!;
            ref FD fd = ref _addr_fd.val;

            if (len(p) > maxRW)
            {
                return (0L, 0L, error.As(errors.New("packet is too large (only 1GB is allowed)"))!);
            }

            {
                var err = fd.writeLock();

                if (err != null)
                {
                    return (0L, 0L, error.As(err)!);
                }

            }

            defer(fd.writeUnlock());

            var o = _addr_fd.wop;
            o.InitMsg(p, oob);
            if (sa != null)
            {
                var (rsa, len, err) = sockaddrToRaw(sa);
                if (err != null)
                {
                    return (0L, 0L, error.As(err)!);
                }

                o.msg.Name = (syscall.Pointer)(rsa);
                o.msg.Namelen = len;

            }

            var (n, err) = execIO(_addr_o, o =>
            {
                return windows.WSASendMsg(o.fd.Sysfd, _addr_o.msg, 0L, _addr_o.qty, _addr_o.o, null);
            });
            return (n, int(o.msg.Control.Len), error.As(err)!);

        });
    }
}}
