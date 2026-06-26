// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using race = @internal.race_package;
using windows = @internal.syscall.windows_package;
using io = io_package;
using sync = sync_package;
using syscall = syscall_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal.syscall;
using unicode;

partial class poll_package {

internal static error initErr;
internal static uint64 ioSync;

// This package uses the SetFileCompletionNotificationModes Windows
// API to skip calling GetQueuedCompletionStatus if an IO operation
// completes synchronously. There is a known bug where
// SetFileCompletionNotificationModes crashes on some systems (see
// https://support.microsoft.com/kb/2568167 for details).
internal static bool useSetFileCompletionNotificationModes; // determines is SetFileCompletionNotificationModes is present and safe to use

// checkSetFileCompletionNotificationModes verifies that
// SetFileCompletionNotificationModes Windows API is present
// on the system and is safe to use.
// See https://support.microsoft.com/kb/2568167 for details.
internal static void checkSetFileCompletionNotificationModes() {
    var err = syscall.LoadSetFileCompletionNotificationModes();
    if (err != default!) {
        return;
    }
    ref var protos = ref heap<array<int32>>(out var Ꮡprotos);
    protos = new int32[]{syscall.IPPROTO_TCP, 0}.array();
    ref var buf = ref heap(new array<syscall.WSAProtocolInfo>(32), out var Ꮡbuf);
    ref var len = ref heap<uint32>(out var Ꮡlen);
    len = ((uint32)@unsafe.Sizeof(buf));
    var (n, err) = syscall.WSAEnumProtocols(Ꮡprotos.at<int32>(0), Ꮡbuf.at<syscall.WSAProtocolInfo>(0), Ꮡlen);
    if (err != default!) {
        return;
    }
    for (var i = ((int32)0); i < n; i++) {
        if ((uint32)(buf[i].ServiceFlags1 & syscall.XP1_IFS_HANDLES) == 0) {
            return;
        }
    }
    useSetFileCompletionNotificationModes = true;
}

// InitWSA initiates the use of the Winsock DLL by the current process.
// It is called from the net package at init time to avoid
// loading ws2_32.dll when net is not used.
public static Action InitWSA = sync.OnceFunc(() => {
    internal static syscall.WSAData d;

    var e = syscall.WSAStartup(((uint32)514), Ꮡ(d));
    if (e != default!) {
        var initErr = e;
    }
    checkSetFileCompletionNotificationModes();
});

// operation contains superset of data necessary to perform all async IO.
[GoType] partial struct operation {
    // Used by IOCP interface, it must be first field
    // of the struct, as our code rely on it.
    internal syscall_package.Overlapped o;
    // fields used by runtime.netpoll
    internal uintptr runtimeCtx;
    internal int32 mode;
    // fields used only by net package
    internal ж<FD> fd;
    internal syscall_package.WSABuf buf;
    internal @internal.syscall.windows_package.WSAMsg msg;
    internal syscall_package.ΔSockaddr sa;
    internal ж<syscall_package.RawSockaddrAny> rsa;
    internal int32 rsan;
    internal syscall_package.ΔHandle handle;
    internal uint32 flags;
    internal uint32 qty;
    internal slice<syscall.WSABuf> bufs;
}

[GoRecv] internal static void InitBuf(this ref operation o, slice<byte> buf) {
    o.buf.Len = ((uint32)len(buf));
    o.buf.Buf = default!;
    if (len(buf) != 0) {
        o.buf.Buf = Ꮡ(buf, 0);
    }
}

[GoRecv] internal static void InitBufs(this ref operation o, ж<slice<slice<byte>>> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.val;

    if (o.bufs == default!){
        o.bufs = new slice<syscall.WSABuf>(0, len(buf));
    } else {
        o.bufs = o.bufs[..0];
    }
    foreach (var (_, b) in buf) {
        if (len(b) == 0) {
            o.bufs = append(o.bufs, new syscall.WSABuf(nil));
            continue;
        }
        while (len(b) > maxRW) {
            o.bufs = append(o.bufs, new syscall.WSABuf(Len: maxRW, Buf: Ꮡ(b, 0)));
            b = b[(int)(maxRW)..];
        }
        if (len(b) > 0) {
            o.bufs = append(o.bufs, new syscall.WSABuf(Len: ((uint32)len(b)), Buf: Ꮡ(b, 0)));
        }
    }
}

// ClearBufs clears all pointers to Buffers parameter captured
// by InitBufs, so it can be released by garbage collector.
[GoRecv] internal static void ClearBufs(this ref operation o) {
    foreach (var (i, _) in o.bufs) {
        o.bufs[i].Buf = default!;
    }
    o.bufs = o.bufs[..0];
}

[GoRecv] internal static void InitMsg(this ref operation o, slice<byte> p, slice<byte> oob) {
    o.InitBuf(p);
    o.msg.Buffers = Ꮡ(o.buf);
    o.msg.BufferCount = 1;
    o.msg.Name = default!;
    o.msg.Namelen = 0;
    o.msg.Flags = 0;
    o.msg.Control.Len = ((uint32)len(oob));
    o.msg.Control.Buf = default!;
    if (len(oob) != 0) {
        o.msg.Control.Buf = Ꮡ(oob, 0);
    }
}

// execIO executes a single IO operation o. It submits and cancels
// IO in the current thread for systems where Windows CancelIoEx API
// is available. Alternatively, it passes the request onto
// runtime netpoll and waits for completion or cancels request.
internal static (nint, error) execIO(ж<operation> Ꮡo, Func<ж<operation>, error> submit) {
    ref var o = ref Ꮡo.val;

    if (o.fd.pd.runtimeCtx == 0) {
        return (0, errors.New("internal error: polling on unsupported descriptor type"u8));
    }
    var fd = o.fd;
    // Notify runtime netpoll about starting IO.
    var err = (~fd).pd.prepare(((nint)o.mode), (~fd).isFile);
    if (err != default!) {
        return (0, err);
    }
    // Start IO.
    err = submit(Ꮡo);
    var exprᴛ1 = err;
    if (exprᴛ1 == default!) {
        if (o.fd.skipSyncNotif) {
            // IO completed immediately
            // No completion message will follow, so return immediately.
            return (((nint)o.qty), default!);
        }
    }
    if (exprᴛ1 == syscall.ERROR_IO_PENDING) {
        err = default!;
    }
    else { /* default: */
        return (0, err);
    }

    // Need to get our completion message anyway.
    // IO started, and we have to wait for its completion.
    // Wait for our request to complete.
    err = (~fd).pd.wait(((nint)o.mode), (~fd).isFile);
    if (err == default!) {
        err = windows.WSAGetOverlappedResult((~fd).Sysfd, Ꮡ(o.o), Ꮡ(o.qty), false, Ꮡ(o.flags));
        // All is good. Extract our IO results and return.
        if (err != default!) {
            // More data available. Return back the size of received data.
            if (err == syscall.ERROR_MORE_DATA || err == windows.WSAEMSGSIZE) {
                return (((nint)o.qty), err);
            }
            return (0, err);
        }
        return (((nint)o.qty), default!);
    }
    // IO is interrupted by "close" or "timeout"
    var netpollErr = err;
    var exprᴛ2 = netpollErr;
    if (exprᴛ2 == ErrNetClosing || exprᴛ2 == ErrFileClosing || exprᴛ2 == ErrDeadlineExceeded) {
    }
    else { /* default: */
        throw panic("unexpected runtime.netpoll error: "u8 + netpollErr.Error());
    }

    // will deal with those.
    // Cancel our request.
    err = syscall.CancelIoEx((~fd).Sysfd, Ꮡ(o.o));
    // Assuming ERROR_NOT_FOUND is returned, if IO is completed.
    if (err != default! && err != syscall.ERROR_NOT_FOUND) {
        // TODO(brainman): maybe do something else, but panic.
        throw panic(err);
    }
    // Wait for cancellation to complete.
    (~fd).pd.waitCanceled(((nint)o.mode));
    err = windows.WSAGetOverlappedResult((~fd).Sysfd, Ꮡ(o.o), Ꮡ(o.qty), false, Ꮡ(o.flags));
    if (err != default!) {
        if (err == syscall.ERROR_OPERATION_ABORTED) {
            // IO Canceled
            err = netpollErr;
        }
        return (0, err);
    }
    // We issued a cancellation request. But, it seems, IO operation succeeded
    // before the cancellation request run. We need to treat the IO operation as
    // succeeded (the bytes are actually sent/recv from network).
    return (((nint)o.qty), default!);
}

// FD is a file descriptor. The net and os packages embed this type in
// a larger type representing a network connection or OS file.
[GoType] partial struct FD {
    // Lock sysfd and serialize access to Read and Write methods.
    internal fdMutex fdmu;
    // System file descriptor. Immutable until Close.
    public syscall_package.ΔHandle Sysfd;
    // Read operation.
    internal operation rop;
    // Write operation.
    internal operation wop;
    // I/O poller.
    internal pollDesc pd;
    // Used to implement pread/pwrite.
    internal sync_package.Mutex l;
    // For console I/O.
    internal slice<byte> lastbits; // first few bytes of the last incomplete rune in last write
    internal slice<uint16> readuint16; // buffer to hold uint16s obtained with ReadConsole
    internal slice<byte> readbyte; // buffer to hold decoding of readuint16 from utf16 to utf8
    internal nint readbyteOffset;     // readbyte[readOffset:] is yet to be consumed with file.Read
    // Semaphore signaled when file is closed.
    internal uint32 csema;
    internal bool skipSyncNotif;
    // Whether this is a streaming descriptor, as opposed to a
    // packet-based descriptor like a UDP socket.
    public bool IsStream;
    // Whether a zero byte read indicates EOF. This is false for a
    // message based socket connection.
    public bool ZeroReadIsEOF;
    // Whether this is a file rather than a network socket.
    internal bool isFile;
    // The kind of this file.
    internal fileKind kind;
}

[GoType("num:byte")] partial struct fileKind;

internal static readonly fileKind kindNet = /* iota */ 0;
internal static readonly fileKind kindFile = 1;
internal static readonly fileKind kindConsole = 2;
internal static readonly fileKind kindPipe = 3;

// logInitFD is set by tests to enable file descriptor initialization logging.
internal static Action<@string, ж<FD>, error> logInitFD;

// Init initializes the FD. The Sysfd field should already be set.
// This can be called multiple times on a single FD.
// The net argument is a network name from the net package (e.g., "tcp"),
// or "file" or "console" or "dir".
// Set pollable to true if fd should be managed by runtime netpoll.
[GoRecv] public static (@string, error) Init(this ref FD fd, @string net, bool pollable) {
    if (initErr != default!) {
        return ("", initErr);
    }
    var exprᴛ1 = net;
    if (exprᴛ1 == "file"u8 || exprᴛ1 == "dir"u8) {
        fd.kind = kindFile;
    }
    else if (exprᴛ1 == "console"u8) {
        fd.kind = kindConsole;
    }
    else if (exprᴛ1 == "pipe"u8) {
        fd.kind = kindPipe;
    }
    else if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8 || exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8 || exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8 || exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        fd.kind = kindNet;
    }
    else { /* default: */
        return ("", errors.New("internal error: unknown network type "u8 + net));
    }

    fd.isFile = fd.kind != kindNet;
    error err = default!;
    if (pollable) {
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
        err = fd.pd.init(fd);
    }
    if (logInitFD != default!) {
        logInitFD(net, fd, err);
    }
    if (err != default!) {
        return ("", err);
    }
    if (pollable && useSetFileCompletionNotificationModes) {
        // We do not use events, so we can skip them always.
        var flags = ((uint8)syscall.FILE_SKIP_SET_EVENT_ON_HANDLE);
        var exprᴛ2 = net;
        if (exprᴛ2 == "tcp"u8 || exprᴛ2 == "tcp4"u8 || exprᴛ2 == "tcp6"u8 || exprᴛ2 == "udp"u8 || exprᴛ2 == "udp4"u8 || exprᴛ2 == "udp6"u8) {
            flags |= (uint8)(syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS);
        }

        var err = syscall.SetFileCompletionNotificationModes(fd.Sysfd, flags);
        if (err == default! && (uint8)(flags & syscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS) != 0) {
            fd.skipSyncNotif = true;
        }
    }
    // Disable SIO_UDP_CONNRESET behavior.
    // http://support.microsoft.com/kb/263823
    var exprᴛ3 = net;
    if (exprᴛ3 == "udp"u8 || exprᴛ3 == "udp4"u8 || exprᴛ3 == "udp6"u8) {
        ref var ret = ref heap<uint32>(out var Ꮡret);
        ret = ((uint32)0);
        ref var flag = ref heap<uint32>(out var Ꮡflag);
        flag = ((uint32)0);
        var size = ((uint32)@unsafe.Sizeof(flag));
        var errΔ2 = syscall.WSAIoctl(fd.Sysfd, syscall.SIO_UDP_CONNRESET, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡflag)), size, nil, 0, Ꮡret, nil, 0);
        if (errΔ2 != default!) {
            return ("wsaioctl", errΔ2);
        }
    }

    fd.rop.mode = (rune)'r';
    fd.wop.mode = (rune)'w';
    fd.rop.fd = fd;
    fd.wop.fd = fd;
    fd.rop.runtimeCtx = fd.pd.runtimeCtx;
    fd.wop.runtimeCtx = fd.pd.runtimeCtx;
    return ("", default!);
}

[GoRecv] internal static error destroy(this ref FD fd) {
    if (fd.Sysfd == syscall.InvalidHandle) {
        return syscall.EINVAL;
    }
    // Poller may want to unregister fd in readiness notification mechanism,
    // so this must be executed before fd.CloseFunc.
    fd.pd.close();
    error err = default!;
    var exprᴛ1 = fd.kind;
    if (exprᴛ1 == kindNet) {
        err = CloseFunc(fd.Sysfd);
    }
    else { /* default: */
        err = syscall.CloseHandle(fd.Sysfd);
    }

    // The net package uses the CloseFunc variable for testing.
    fd.Sysfd = syscall.InvalidHandle;
    runtime_Semrelease(Ꮡ(fd.csema));
    return err;
}

// Close closes the FD. The underlying file descriptor is closed by
// the destroy method when there are no remaining references.
[GoRecv] public static error Close(this ref FD fd) {
    if (!fd.fdmu.increfAndClose()) {
        return errClosing(fd.isFile);
    }
    if (fd.kind == kindPipe) {
        syscall.CancelIoEx(fd.Sysfd, nil);
    }
    // unblock pending reader and writer
    fd.pd.evict();
    var err = fd.decref();
    // Wait until the descriptor is closed. If this was the only
    // reference, it is already closed.
    runtime_Semacquire(Ꮡ(fd.csema));
    return err;
}

// Windows ReadFile and WSARecv use DWORD (uint32) parameter to pass buffer length.
// This prevents us reading blocks larger than 4GB.
// See golang.org/issue/26923.
internal static readonly UntypedInt maxRW = /* 1 << 30 */ 1073741824; // 1GB is large enough and keeps subsequent reads aligned

// Read implements io.Reader.
[GoRecv] public static (nint, error) Read(this ref FD fd, slice<byte> buf) => func((defer, _) => {
    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.readUnlock);
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    nint n = default!;
    error err = default!;
    if (fd.isFile){
        fd.l.Lock();
        defer(fd.l.Unlock);
        var exprᴛ1 = fd.kind;
        if (exprᴛ1 == kindConsole) {
            (n, err) = fd.readConsole(buf);
        }
        else { /* default: */
            (n, err) = syscall.Read(fd.Sysfd, buf);
            if (fd.kind == kindPipe && err == syscall.ERROR_OPERATION_ABORTED) {
                // Close uses CancelIoEx to interrupt concurrent I/O for pipes.
                // If the fd is a pipe and the Read was interrupted by CancelIoEx,
                // we assume it is interrupted by Close.
                err = ErrFileClosing;
            }
        }

        if (err != default!) {
            n = 0;
        }
    } else {
        var o = Ꮡ(fd.rop);
        o.InitBuf(buf);
        (n, err) = execIO(o, (ж<operation> o) => syscall.WSARecv((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).flags), Ꮡ((~oΔ1).o), nil));
        if (race.Enabled) {
            race.Acquire(new @unsafe.Pointer(Ꮡ(ioSync)));
        }
    }
    if (len(buf) != 0) {
        err = fd.eofError(n, err);
    }
    return (n, err);
});

public static Func<syscallꓸHandle, ж<uint16>, uint32, ж<uint32>, ж<byte>, (err error)> ReadConsole = syscall.ReadConsole;                                                               // changed for testing

// readConsole reads utf16 characters from console File,
// encodes them into utf8 and stores them in buffer b.
// It returns the number of utf8 bytes read and an error, if any.
[GoRecv] internal static (nint, error) readConsole(this ref FD fd, slice<byte> b) {
    if (len(b) == 0) {
        return (0, default!);
    }
    if (fd.readuint16 == default!) {
        // Note: syscall.ReadConsole fails for very large buffers.
        // The limit is somewhere around (but not exactly) 16384.
        // Stay well below.
        fd.readuint16 = new slice<uint16>(0, 10000);
        fd.readbyte = new slice<byte>(0, 4 * cap(fd.readuint16));
    }
    while (fd.readbyteOffset >= len(fd.readbyte)) {
        nint n = cap(fd.readuint16) - len(fd.readuint16);
        if (n > len(b)) {
            n = len(b);
        }
        ref var nw = ref heap(new uint32(), out var Ꮡnw);
        var err = ReadConsole(fd.Sysfd, Ꮡfd.readuint16[..(int)(len(fd.readuint16) + 1)].at<uint16>(len(fd.readuint16)), ((uint32)n), Ꮡnw, nil);
        if (err != default!) {
            return (0, err);
        }
        var uint16s = fd.readuint16[..(int)(len(fd.readuint16) + ((nint)nw))];
        fd.readuint16 = fd.readuint16[..0];
        var buf = fd.readbyte[..0];
        for (nint iΔ1 = 0; iΔ1 < len(uint16s); iΔ1++) {
            var r = ((rune)uint16s[iΔ1]);
            if (utf16.IsSurrogate(r)) {
                if (iΔ1 + 1 == len(uint16s)){
                    if (nw > 0) {
                        // Save half surrogate pair for next time.
                        fd.readuint16 = fd.readuint16[..1];
                        fd.readuint16[0] = ((uint16)r);
                        break;
                    }
                    r = utf8.RuneError;
                } else {
                    r = utf16.DecodeRune(r, ((rune)uint16s[iΔ1 + 1]));
                    if (r != utf8.RuneError) {
                        iΔ1++;
                    }
                }
            }
            buf = utf8.AppendRune(buf, r);
        }
        fd.readbyte = buf;
        fd.readbyteOffset = 0;
        if (nw == 0) {
            break;
        }
    }
    var src = fd.readbyte[(int)(fd.readbyteOffset)..];
    nint i = default!;
    for (i = 0; i < len(src) && i < len(b); i++) {
        var x = src[i];
        if (x == 26) {
            // Ctrl-Z
            if (i == 0) {
                fd.readbyteOffset++;
            }
            break;
        }
        b[i] = x;
    }
    fd.readbyteOffset += i;
    return (i, default!);
}

// Pread emulates the Unix pread system call.
[GoRecv] public static (nint, error) Pread(this ref FD fd, slice<byte> b, int64 off) => func((defer, _) => {
    if (fd.kind == kindPipe) {
        // Pread does not work with pipes
        return (0, syscall.ESPIPE);
    }
    // Call incref, not readLock, because since pread specifies the
    // offset it is independent from other reads.
    {
        var err = fd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.decref);
    if (len(b) > maxRW) {
        b = b[..(int)(maxRW)];
    }
    fd.l.Lock();
    defer(fd.l.Unlock);
    var (curoffset, e) = syscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
    if (e != default!) {
        return (0, e);
    }
    deferǃ(syscall.Seek, fd.Sysfd, curoffset, io.SeekStart, defer);
    ref var o = ref heap<syscall_package.Overlapped>(out var Ꮡo);
    o = new syscall.Overlapped(
        OffsetHigh: ((uint32)(off >> (int)(32))),
        Offset: ((uint32)off)
    );
    ref var done = ref heap(new uint32(), out var Ꮡdone);
    e = syscall.ReadFile(fd.Sysfd, b, Ꮡdone, Ꮡo);
    if (e != default!) {
        done = 0;
        if (e == syscall.ERROR_HANDLE_EOF) {
            e = io.EOF;
        }
    }
    if (len(b) != 0) {
        e = fd.eofError(((nint)done), e);
    }
    return (((nint)done), e);
});

// ReadFrom wraps the recvfrom network call.
[GoRecv] public static (nint, syscallꓸSockaddr, error) ReadFrom(this ref FD fd, slice<byte> buf) => func((defer, _) => {
    if (len(buf) == 0) {
        return (0, default!, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, default!, errΔ1);
        }
    }
    defer(fd.readUnlock);
    var o = Ꮡ(fd.rop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> o) => {
        if ((~oΔ1).rsa == nil) {
            o.val.rsa = @new<syscall.RawSockaddrAny>();
        }
        o.val.rsan = ((int32)@unsafe.Sizeof((~oΔ1).rsa.val));
        return syscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).flags), (~oΔ1).rsa, Ꮡ((~oΔ1).rsan), Ꮡ((~oΔ1).o), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, default!, err);
    }
    (sa, _) = (~o).rsa.Sockaddr();
    return (n, sa, default!);
});

// ReadFromInet4 wraps the recvfrom network call for IPv4.
[GoRecv] public static (nint, error) ReadFromInet4(this ref FD fd, slice<byte> buf, ж<syscall.SockaddrInet4> Ꮡsa4) => func((defer, _) => {
    ref var sa4 = ref Ꮡsa4.val;

    if (len(buf) == 0) {
        return (0, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.readUnlock);
    var o = Ꮡ(fd.rop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> o) => {
        if ((~oΔ1).rsa == nil) {
            o.val.rsa = @new<syscall.RawSockaddrAny>();
        }
        o.val.rsan = ((int32)@unsafe.Sizeof((~oΔ1).rsa.val));
        return syscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).flags), (~oΔ1).rsa, Ꮡ((~oΔ1).rsan), Ꮡ((~oΔ1).o), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, err);
    }
    rawToSockaddrInet4((~o).rsa, Ꮡsa4);
    return (n, err);
});

// ReadFromInet6 wraps the recvfrom network call for IPv6.
[GoRecv] public static (nint, error) ReadFromInet6(this ref FD fd, slice<byte> buf, ж<syscall.SockaddrInet6> Ꮡsa6) => func((defer, _) => {
    ref var sa6 = ref Ꮡsa6.val;

    if (len(buf) == 0) {
        return (0, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.readUnlock);
    var o = Ꮡ(fd.rop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> o) => {
        if ((~oΔ1).rsa == nil) {
            o.val.rsa = @new<syscall.RawSockaddrAny>();
        }
        o.val.rsan = ((int32)@unsafe.Sizeof((~oΔ1).rsa.val));
        return syscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).flags), (~oΔ1).rsa, Ꮡ((~oΔ1).rsan), Ꮡ((~oΔ1).o), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, err);
    }
    rawToSockaddrInet6((~o).rsa, Ꮡsa6);
    return (n, err);
});

// Write implements io.Writer.
[GoRecv] public static (nint, error) Write(this ref FD fd, slice<byte> buf) => func((defer, _) => {
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    if (fd.isFile) {
        fd.l.Lock();
        defer(fd.l.Unlock);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        nint n = default!;
        error err = default!;
        if (fd.isFile){
            var exprᴛ1 = fd.kind;
            if (exprᴛ1 == kindConsole) {
                (n, err) = fd.writeConsole(b);
            }
            else { /* default: */
                (n, err) = syscall.Write(fd.Sysfd, b);
                if (fd.kind == kindPipe && err == syscall.ERROR_OPERATION_ABORTED) {
                    // Close uses CancelIoEx to interrupt concurrent I/O for pipes.
                    // If the fd is a pipe and the Write was interrupted by CancelIoEx,
                    // we assume it is interrupted by Close.
                    err = ErrFileClosing;
                }
            }

            if (err != default!) {
                n = 0;
            }
        } else {
            if (race.Enabled) {
                race.ReleaseMerge(new @unsafe.Pointer(Ꮡ(ioSync)));
            }
            var o = Ꮡ(fd.wop);
            o.InitBuf(b);
            (n, err) = execIO(o, (ж<operation> o) => syscall.WSASend((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, Ꮡ((~oΔ1).o), nil));
        }
        ntotal += n;
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// writeConsole writes len(b) bytes to the console File.
// It returns the number of bytes written and an error, if any.
[GoRecv] internal static (nint, error) writeConsole(this ref FD fd, slice<byte> b) {
    nint n = len(b);
    var runes = new slice<rune>(0, 256);
    if (len(fd.lastbits) > 0) {
        b = append(fd.lastbits, b.ꓸꓸꓸ);
        fd.lastbits = default!;
    }
    while (len(b) >= utf8.UTFMax || utf8.FullRune(b)) {
        var (r, l) = utf8.DecodeRune(b);
        runes = append(runes, r);
        b = b[(int)(l)..];
    }
    if (len(b) > 0) {
        fd.lastbits = new slice<byte>(len(b));
        copy(fd.lastbits, b);
    }
    // syscall.WriteConsole seems to fail, if given large buffer.
    // So limit the buffer to 16000 characters. This number was
    // discovered by experimenting with syscall.WriteConsole.
    static readonly UntypedInt maxWrite = 16000;
    while (len(runes) > 0) {
        nint m = len(runes);
        if (m > maxWrite) {
            m = maxWrite;
        }
        var chunk = runes[..(int)(m)];
        runes = runes[(int)(m)..];
        var uint16s = utf16.Encode(chunk);
        while (len(uint16s) > 0) {
            ref var written = ref heap(new uint32(), out var Ꮡwritten);
            var err = syscall.WriteConsole(fd.Sysfd, Ꮡ(uint16s, 0), ((uint32)len(uint16s)), Ꮡwritten, nil);
            if (err != default!) {
                return (0, err);
            }
            uint16s = uint16s[(int)(written)..];
        }
    }
    return (n, default!);
}

// Pwrite emulates the Unix pwrite system call.
[GoRecv] public static (nint, error) Pwrite(this ref FD fd, slice<byte> buf, int64 off) => func((defer, _) => {
    if (fd.kind == kindPipe) {
        // Pwrite does not work with pipes
        return (0, syscall.ESPIPE);
    }
    // Call incref, not writeLock, because since pwrite specifies the
    // offset it is independent from other writes.
    {
        var err = fd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.decref);
    fd.l.Lock();
    defer(fd.l.Unlock);
    var (curoffset, e) = syscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
    if (e != default!) {
        return (0, e);
    }
    deferǃ(syscall.Seek, fd.Sysfd, curoffset, io.SeekStart, defer);
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        ref var n = ref heap(new uint32(), out var Ꮡn);
        ref var o = ref heap<syscall_package.Overlapped>(out var Ꮡo);
        o = new syscall.Overlapped(
            OffsetHigh: ((uint32)(off >> (int)(32))),
            Offset: ((uint32)off)
        );
        e = syscall.WriteFile(fd.Sysfd, b, Ꮡn, Ꮡo);
        ntotal += ((nint)n);
        if (e != default!) {
            return (ntotal, e);
        }
        buf = buf[(int)(n)..];
        off += ((int64)n);
    }
    return (ntotal, default!);
});

// Writev emulates the Unix writev system call.
[GoRecv] public static (int64, error) Writev(this ref FD fd, ж<slice<slice<byte>>> Ꮡbuf) => func((defer, _) => {
    ref var buf = ref Ꮡbuf.val;

    if (len(buf) == 0) {
        return (0, default!);
    }
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    if (race.Enabled) {
        race.ReleaseMerge(new @unsafe.Pointer(Ꮡ(ioSync)));
    }
    var o = Ꮡ(fd.wop);
    o.InitBufs(Ꮡbuf);
    var (n, err) = execIO(o, (ж<operation> o) => syscall.WSASend((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).bufs, 0), ((uint32)len((~oΔ1).bufs)), Ꮡ((~oΔ1).qty), 0, Ꮡ((~oΔ1).o), nil));
    o.ClearBufs();
    TestHookDidWritev(n);
    consume(Ꮡbuf, ((int64)n));
    return (((int64)n), err);
});

// WriteTo wraps the sendto network call.
[GoRecv] public static (nint, error) WriteTo(this ref FD fd, slice<byte> buf, syscallꓸSockaddr sa) => func((defer, _) => {
    {
        var err = fd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡ(fd.wop);
        o.InitBuf(buf);
        o.val.sa = sa;
        var (n, err) = execIO(o, (ж<operation> o) => syscall.WSASendto((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, (~oΔ1).sa, Ꮡ((~oΔ1).o), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡ(fd.wop);
        o.InitBuf(b);
        o.val.sa = sa;
        var (n, err) = execIO(o, (ж<operation> o) => syscall.WSASendto((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, (~oΔ1).sa, Ꮡ((~oΔ1).o), nil));
        ntotal += ((nint)n);
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// WriteToInet4 is WriteTo, specialized for syscall.SockaddrInet4.
[GoRecv] public static (nint, error) WriteToInet4(this ref FD fd, slice<byte> buf, ж<syscall.SockaddrInet4> Ꮡsa4) => func((defer, _) => {
    ref var sa4 = ref Ꮡsa4.val;

    {
        var err = fd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡ(fd.wop);
        o.InitBuf(buf);
        var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendtoInet4((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, Ꮡsa4, Ꮡ((~oΔ1).o), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡ(fd.wop);
        o.InitBuf(b);
        var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendtoInet4((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, Ꮡsa4, Ꮡ((~oΔ1).o), nil));
        ntotal += ((nint)n);
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// WriteToInet6 is WriteTo, specialized for syscall.SockaddrInet6.
[GoRecv] public static (nint, error) WriteToInet6(this ref FD fd, slice<byte> buf, ж<syscall.SockaddrInet6> Ꮡsa6) => func((defer, _) => {
    ref var sa6 = ref Ꮡsa6.val;

    {
        var err = fd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡ(fd.wop);
        o.InitBuf(buf);
        var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendtoInet6((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, Ꮡsa6, Ꮡ((~oΔ1).o), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡ(fd.wop);
        o.InitBuf(b);
        var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendtoInet6((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), 0, Ꮡsa6, Ꮡ((~oΔ1).o), nil));
        ntotal += ((nint)n);
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// Call ConnectEx. This doesn't need any locking, since it is only
// called when the descriptor is first created. This is here rather
// than in the net package so that it can use fd.wop.
[GoRecv] public static error ConnectEx(this ref FD fd, syscallꓸSockaddr ra) {
    var o = Ꮡ(fd.wop);
    o.val.sa = ra;
    var (_, err) = execIO(o, (ж<operation> o) => ConnectExFunc((~(~oΔ1).fd).Sysfd, (~oΔ1).sa, nil, 0, nil, Ꮡ((~oΔ1).o)));
    return err;
}

[GoRecv] public static (@string, error) acceptOne(this ref FD fd, syscallꓸHandle s, slice<syscall.RawSockaddrAny> rawsa, ж<operation> Ꮡo) {
    ref var o = ref Ꮡo.val;

    // Submit accept request.
    o.handle = s;
    o.rsan = ((int32)@unsafe.Sizeof(rawsa[0]));
    var (_, err) = execIO(Ꮡo, 
    var rawsaʗ1 = rawsa;
    (ж<operation> o) => AcceptFunc((~(~oΔ1).fd).Sysfd, (~oΔ1).handle, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(rawsaʗ1, 0))), 0, ((uint32)(~oΔ1).rsan), ((uint32)(~oΔ1).rsan), Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o)));
    if (err != default!) {
        CloseFunc(s);
        return ("acceptex", err);
    }
    // Inherit properties of the listening socket.
    err = syscall.Setsockopt(s, syscall.SOL_SOCKET, syscall.SO_UPDATE_ACCEPT_CONTEXT, (ж<byte>)(uintptr)(((@unsafe.Pointer)(Ꮡ(fd.Sysfd)))), ((int32)@unsafe.Sizeof(fd.Sysfd)));
    if (err != default!) {
        CloseFunc(s);
        return ("setsockopt", err);
    }
    return ("", default!);
}

// Accept handles accepting a socket. The sysSocket parameter is used
// to allocate the net socket.
[GoRecv] public static (syscallꓸHandle, slice<syscall.RawSockaddrAny>, uint32, @string, error) Accept(this ref FD fd, Func<(syscall.Handle, error)> sysSocket) => func((defer, _) => {
    {
        var err = fd.readLock(); if (err != default!) {
            return (syscall.InvalidHandle, default!, 0, "", err);
        }
    }
    defer(fd.readUnlock);
    var o = Ꮡ(fd.rop);
    array<syscall.RawSockaddrAny> rawsa = new(2);
    while (ᐧ) {
        var (s, err) = sysSocket();
        if (err != default!) {
            return (syscall.InvalidHandle, default!, 0, "", err);
        }
        var (errcall, err) = fd.acceptOne(s, rawsa[..], o);
        if (err == default!) {
            return (s, rawsa[..], ((uint32)(~o).rsan), "", default!);
        }
        // Sometimes we see WSAECONNRESET and ERROR_NETNAME_DELETED is
        // returned here. These happen if connection reset is received
        // before AcceptEx could complete. These errors relate to new
        // connection, not to AcceptEx, so ignore broken connection and
        // try AcceptEx again for more connections.
        var (errno, ok) = err._<syscall.Errno>(ᐧ);
        if (!ok) {
            return (syscall.InvalidHandle, default!, 0, errcall, err);
        }
        var exprᴛ1 = errno;
        if (exprᴛ1 == syscall.ERROR_NETNAME_DELETED || exprᴛ1 == syscall.WSAECONNRESET) {
        }
        else { /* default: */
            return (syscall.InvalidHandle, default!, 0, errcall, err);
        }

    }
});

// ignore these and try again

// Seek wraps syscall.Seek.
[GoRecv] public static (int64, error) Seek(this ref FD fd, int64 offset, nint whence) => func((defer, _) => {
    if (fd.kind == kindPipe) {
        return (0, syscall.ESPIPE);
    }
    {
        var err = fd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.decref);
    fd.l.Lock();
    defer(fd.l.Unlock);
    return syscall.Seek(fd.Sysfd, offset, whence);
});

// Fchmod updates syscall.ByHandleFileInformation.Fileattributes when needed.
[GoRecv] public static error Fchmod(this ref FD fd, uint32 mode) => func((defer, _) => {
    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    ref var d = ref heap(new syscall_package.ByHandleFileInformation(), out var Ꮡd);
    {
        var err = syscall.GetFileInformationByHandle(fd.Sysfd, Ꮡd); if (err != default!) {
            return err;
        }
    }
    var attrs = d.FileAttributes;
    if ((uint32)(mode & syscall.S_IWRITE) != 0){
        attrs &= ~(uint32)(syscall.FILE_ATTRIBUTE_READONLY);
    } else {
        attrs |= (uint32)(syscall.FILE_ATTRIBUTE_READONLY);
    }
    if (attrs == d.FileAttributes) {
        return default!;
    }
    ref var du = ref heap(new @internal.syscall.windows_package.FILE_BASIC_INFO(), out var Ꮡdu);
    du.FileAttributes = attrs;
    return windows.SetFileInformationByHandle(fd.Sysfd, windows.FileBasicInfo, new @unsafe.Pointer(Ꮡdu), ((uint32)@unsafe.Sizeof(du)));
});

// Fchdir wraps syscall.Fchdir.
[GoRecv] public static error Fchdir(this ref FD fd) => func((defer, _) => {
    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.Fchdir(fd.Sysfd);
});

// GetFileType wraps syscall.GetFileType.
[GoRecv] public static (uint32, error) GetFileType(this ref FD fd) => func((defer, _) => {
    {
        var err = fd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(fd.decref);
    return syscall.GetFileType(fd.Sysfd);
});

// GetFileInformationByHandle wraps GetFileInformationByHandle.
[GoRecv] public static error GetFileInformationByHandle(this ref FD fd, ж<syscall.ByHandleFileInformation> Ꮡdata) => func((defer, _) => {
    ref var data = ref Ꮡdata.val;

    {
        var err = fd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(fd.decref);
    return syscall.GetFileInformationByHandle(fd.Sysfd, Ꮡdata);
});

// RawRead invokes the user-defined function f for a read operation.
[GoRecv] public static error RawRead(this ref FD fd, Func<uintptr, bool> f) => func((defer, _) => {
    {
        var err = fd.readLock(); if (err != default!) {
            return err;
        }
    }
    defer(fd.readUnlock);
    while (ᐧ) {
        if (f(((uintptr)fd.Sysfd))) {
            return default!;
        }
        // Use a zero-byte read as a way to get notified when this
        // socket is readable. h/t https://stackoverflow.com/a/42019668/332798
        var o = Ꮡ(fd.rop);
        o.InitBuf(default!);
        if (!fd.IsStream) {
            o.val.flags |= (uint32)(windows.MSG_PEEK);
        }
        var (_, err) = execIO(o, (ж<operation> o) => syscall.WSARecv((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).buf), 1, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).flags), Ꮡ((~oΔ1).o), nil));
        if (err == windows.WSAEMSGSIZE){
        } else 
        if (err != default!) {
            // expected with a 0-byte peek, ignore.
            return err;
        }
    }
});

// RawWrite invokes the user-defined function f for a write operation.
[GoRecv] public static error RawWrite(this ref FD fd, Func<uintptr, bool> f) => func((defer, _) => {
    {
        var err = fd.writeLock(); if (err != default!) {
            return err;
        }
    }
    defer(fd.writeUnlock);
    if (f(((uintptr)fd.Sysfd))) {
        return default!;
    }
    // TODO(tmm1): find a way to detect socket writability
    return syscall.EWINDOWS;
});

internal static int32 sockaddrInet4ToRaw(ж<syscall.RawSockaddrAny> Ꮡrsa, ж<syscall.SockaddrInet4> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.val;
    ref var sa = ref Ꮡsa.val;

    rsa = new syscall.RawSockaddrAny(nil);
    var raw = (ж<syscall.RawSockaddrInet4>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    raw.val.Family = syscall.AF_INET;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~raw).Port)));
    p.val[0] = ((byte)(sa.Port >> (int)(8)));
    p.val[1] = ((byte)sa.Port);
    raw.val.Addr = sa.Addr;
    return ((int32)@unsafe.Sizeof(raw.val));
}

internal static int32 sockaddrInet6ToRaw(ж<syscall.RawSockaddrAny> Ꮡrsa, ж<syscall.SockaddrInet6> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.val;
    ref var sa = ref Ꮡsa.val;

    rsa = new syscall.RawSockaddrAny(nil);
    var raw = (ж<syscall.RawSockaddrInet6>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    raw.val.Family = syscall.AF_INET6;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~raw).Port)));
    p.val[0] = ((byte)(sa.Port >> (int)(8)));
    p.val[1] = ((byte)sa.Port);
    raw.val.Scope_id = sa.ZoneId;
    raw.val.Addr = sa.Addr;
    return ((int32)@unsafe.Sizeof(raw.val));
}

internal static void rawToSockaddrInet4(ж<syscall.RawSockaddrAny> Ꮡrsa, ж<syscall.SockaddrInet4> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.val;
    ref var sa = ref Ꮡsa.val;

    var pp = (ж<syscall.RawSockaddrInet4>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~pp).Port)));
    sa.Port = ((nint)p.val[0]) << (int)(8) + ((nint)p.val[1]);
    sa.Addr = pp.val.Addr;
}

internal static void rawToSockaddrInet6(ж<syscall.RawSockaddrAny> Ꮡrsa, ж<syscall.SockaddrInet6> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.val;
    ref var sa = ref Ꮡsa.val;

    var pp = (ж<syscall.RawSockaddrInet6>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~pp).Port)));
    sa.Port = ((nint)p.val[0]) << (int)(8) + ((nint)p.val[1]);
    sa.ZoneId = pp.val.Scope_id;
    sa.Addr = pp.val.Addr;
}

internal static (int32, error) sockaddrToRaw(ж<syscall.RawSockaddrAny> Ꮡrsa, syscallꓸSockaddr sa) {
    ref var rsa = ref Ꮡrsa.val;

    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> sa: {
        var sz = sockaddrInet4ToRaw(Ꮡrsa, Ꮡsa);
        return (sz, default!);
    }
    case ж<syscall.SockaddrInet6> sa: {
        sz = sockaddrInet6ToRaw(Ꮡrsa, Ꮡsa);
        return (sz, default!);
    }
    default: {
        var sa = sa.type();
        return (0, syscall.EWINDOWS);
    }}
}

// ReadMsg wraps the WSARecvMsg network call.
[GoRecv] public static (nint, nint, nint, syscallꓸSockaddr, error) ReadMsg(this ref FD fd, slice<byte> p, slice<byte> oob, nint flags) => func((defer, _) => {
    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, default!, errΔ1);
        }
    }
    defer(fd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡ(fd.rop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.val.rsa = @new<syscall.RawSockaddrAny>();
    }
    (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
    (~o).msg.Namelen = ((int32)@unsafe.Sizeof((~o).rsa.val));
    (~o).msg.Flags = ((uint32)flags);
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    err = fd.eofError(n, err);
    syscallꓸSockaddr sa = default!;
    if (err == default!) {
        (sa, err) = (~o).rsa.Sockaddr();
    }
    return (n, ((nint)(~o).msg.Control.Len), ((nint)(~o).msg.Flags), sa, err);
});

// ReadMsgInet4 is ReadMsg, but specialized to return a syscall.SockaddrInet4.
[GoRecv] public static (nint, nint, nint, error) ReadMsgInet4(this ref FD fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> Ꮡsa4) => func((defer, _) => {
    ref var sa4 = ref Ꮡsa4.val;

    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, errΔ1);
        }
    }
    defer(fd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡ(fd.rop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.val.rsa = @new<syscall.RawSockaddrAny>();
    }
    (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
    (~o).msg.Namelen = ((int32)@unsafe.Sizeof((~o).rsa.val));
    (~o).msg.Flags = ((uint32)flags);
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    err = fd.eofError(n, err);
    if (err == default!) {
        rawToSockaddrInet4((~o).rsa, Ꮡsa4);
    }
    return (n, ((nint)(~o).msg.Control.Len), ((nint)(~o).msg.Flags), err);
});

// ReadMsgInet6 is ReadMsg, but specialized to return a syscall.SockaddrInet6.
[GoRecv] public static (nint, nint, nint, error) ReadMsgInet6(this ref FD fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> Ꮡsa6) => func((defer, _) => {
    ref var sa6 = ref Ꮡsa6.val;

    {
        var errΔ1 = fd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, errΔ1);
        }
    }
    defer(fd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡ(fd.rop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.val.rsa = @new<syscall.RawSockaddrAny>();
    }
    (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
    (~o).msg.Namelen = ((int32)@unsafe.Sizeof((~o).rsa.val));
    (~o).msg.Flags = ((uint32)flags);
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    err = fd.eofError(n, err);
    if (err == default!) {
        rawToSockaddrInet6((~o).rsa, Ꮡsa6);
    }
    return (n, ((nint)(~o).msg.Control.Len), ((nint)(~o).msg.Flags), err);
});

// WriteMsg wraps the WSASendMsg network call.
[GoRecv] public static (nint, nint, error) WriteMsg(this ref FD fd, slice<byte> p, slice<byte> oob, syscallꓸSockaddr sa) => func((defer, _) => {
    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    var o = Ꮡ(fd.wop);
    o.InitMsg(p, oob);
    if (sa != default!) {
        if ((~o).rsa == nil) {
            o.val.rsa = @new<syscall.RawSockaddrAny>();
        }
        var (len, errΔ2) = sockaddrToRaw((~o).rsa, sa);
        if (errΔ2 != default!) {
            return (0, 0, errΔ2);
        }
        (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
        (~o).msg.Namelen = len;
    }
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), 0, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    return (n, ((nint)(~o).msg.Control.Len), err);
});

// WriteMsgInet4 is WriteMsg specialized for syscall.SockaddrInet4.
[GoRecv] public static (nint, nint, error) WriteMsgInet4(this ref FD fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> Ꮡsa) => func((defer, _) => {
    ref var sa = ref Ꮡsa.val;

    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    var o = Ꮡ(fd.wop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.val.rsa = @new<syscall.RawSockaddrAny>();
    }
    var len = sockaddrInet4ToRaw((~o).rsa, Ꮡsa);
    (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
    (~o).msg.Namelen = len;
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), 0, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    return (n, ((nint)(~o).msg.Control.Len), err);
});

// WriteMsgInet6 is WriteMsg specialized for syscall.SockaddrInet6.
[GoRecv] public static (nint, nint, error) WriteMsgInet6(this ref FD fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> Ꮡsa) => func((defer, _) => {
    ref var sa = ref Ꮡsa.val;

    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = fd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(fd.writeUnlock);
    var o = Ꮡ(fd.wop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.val.rsa = @new<syscall.RawSockaddrAny>();
    }
    var len = sockaddrInet6ToRaw((~o).rsa, Ꮡsa);
    (~o).msg.Name = ((syscall.Pointer)new @unsafe.Pointer((~o).rsa));
    (~o).msg.Namelen = len;
    var (n, err) = execIO(o, (ж<operation> o) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).msg), 0, Ꮡ((~oΔ1).qty), Ꮡ((~oΔ1).o), nil));
    return (n, ((nint)(~o).msg.Control.Len), err);
});

public static (nint, @string, error) DupCloseOnExec(nint fd) {
    var (proc, err) = syscall.GetCurrentProcess();
    if (err != default!) {
        return (0, "GetCurrentProcess", err);
    }
    ref var nfd = ref heap(new syscall_package.ΔHandle(), out var Ꮡnfd);
    const bool inherit = false; // analogous to CLOEXEC
    {
        var errΔ1 = syscall.DuplicateHandle(proc, ((syscallꓸHandle)fd), proc, Ꮡnfd, 0, inherit, syscall.DUPLICATE_SAME_ACCESS); if (errΔ1 != default!) {
            return (0, "DuplicateHandle", errΔ1);
        }
    }
    return (((nint)nfd), "", default!);
}

} // end poll_package
