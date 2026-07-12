// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using race = go.@internal.race_package;
using windows = go.@internal.syscall.windows_package;
using io = io_package;
using sync = sync_package;
using Δsyscall = syscall_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.syscall;
using unicode;

partial class poll_package {

internal static error initErr;
internal static ж<uint64> ᏑioSync = new(default(uint64));
internal static ref uint64 ioSync => ref ᏑioSync.Value;

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
    var err = Δsyscall.LoadSetFileCompletionNotificationModes();
    if (err != default!) {
        return;
    }
    ref var protos = ref heap<array<int32>>(out var Ꮡprotos);
    protos = new int32[]{Δsyscall.IPPROTO_TCP, 0}.array();
    ref var buf = ref heap(new array<Δsyscall.WSAProtocolInfo>(32), out var Ꮡbuf);
    ref var len = ref heap<uint32>(out var Ꮡlen);
    len = (uint32)@unsafe.Sizeof(buf);
    (var n, err) = Δsyscall.WSAEnumProtocols(Ꮡprotos.at<int32>(0), Ꮡbuf.at<Δsyscall.WSAProtocolInfo>(0), Ꮡlen);
    if (err != default!) {
        return;
    }
    for (var i = (int32)0; i < n; i++) {
        if ((uint32)(buf[i].ServiceFlags1 & (uint32)Δsyscall.XP1_IFS_HANDLES) == 0) {
            return;
        }
    }
    useSetFileCompletionNotificationModes = true;
}

// InitWSA initiates the use of the Winsock DLL by the current process.
// It is called from the net package at init time to avoid
// loading ws2_32.dll when net is not used.
public static Action InitWSA = sync.OnceFunc(() => {
    Δsyscall.WSAData d = default!;
    var e = Δsyscall.WSAStartup((uint32)0x202, Ꮡ(d));
    if (e != default!) {
        initErr = e;
    }
    checkSetFileCompletionNotificationModes();
});

// operation contains superset of data necessary to perform all async IO.
[GoType] partial struct operation {
    // Used by IOCP interface, it must be first field
    // of the struct, as our code rely on it.
    internal Δsyscall.Overlapped o;
    // fields used by runtime.netpoll
    internal uintptr runtimeCtx;
    internal int32 mode;
    // fields used only by net package
    internal ж<FD> fd;
    internal Δsyscall.WSABuf buf;
    internal windows.WSAMsg msg;
    internal syscallꓸSockaddr sa;
    internal ж<Δsyscall.RawSockaddrAny> rsa;
    internal int32 rsan;
    internal syscallꓸHandle handle;
    internal uint32 flags;
    internal uint32 qty;
    internal slice<Δsyscall.WSABuf> bufs;
}

[GoRecv] internal static void InitBuf(this ref operation o, slice<byte> buf) {
    o.buf.Len = (uint32)len(buf);
    o.buf.Buf = default!;
    if (len(buf) != 0) {
        o.buf.Buf = Ꮡ(buf, 0);
    }
}

[GoRecv] internal static void InitBufs(this ref operation o, ж<slice<slice<byte>>> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.Value;

    if (o.bufs == default!){
        o.bufs = new slice<Δsyscall.WSABuf>(0, len(buf));
    } else {
        o.bufs = o.bufs[..0];
    }
    foreach (var (_, vᴛ1) in buf) {
        var b = vᴛ1;

        if (len(b) == 0) {
            o.bufs = append(o.bufs, new Δsyscall.WSABuf(nil));
            continue;
        }
        while (len(b) > maxRW) {
            o.bufs = append(o.bufs, new Δsyscall.WSABuf(Len: maxRW, Buf: Ꮡ(b, 0)));
            b = b[(int)(maxRW)..];
        }
        if (len(b) > 0) {
            o.bufs = append(o.bufs, new Δsyscall.WSABuf(Len: (uint32)len(b), Buf: Ꮡ(b, 0)));
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

internal static void InitMsg(this ж<operation> Ꮡo, slice<byte> p, slice<byte> oob) {
    ref var o = ref Ꮡo.Value;

    o.InitBuf(p);
    o.msg.Buffers = Ꮡo.of(operation.Ꮡbuf);
    o.msg.BufferCount = 1;
    o.msg.Name = default!;
    o.msg.Namelen = 0;
    o.msg.Flags = 0;
    o.msg.Control.Len = (uint32)len(oob);
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
    ref var o = ref Ꮡo.Value;

    if ((~o.fd).pd.runtimeCtx == 0) {
        return (0, errors.New("internal error: polling on unsupported descriptor type"u8));
    }
    var fd = o.fd;
    // Notify runtime netpoll about starting IO.
    var err = fd.of(FD.Ꮡpd).prepare((nint)o.mode, (~fd).isFile);
    if (err != default!) {
        return (0, err);
    }
    // Start IO.
    err = submit(Ꮡo);
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, default!)) {
        if ((~o.fd).skipSyncNotif) {
            // IO completed immediately
            // No completion message will follow, so return immediately.
            return ((nint)o.qty, default!);
        }
    }
    if (AreEqual(exprᴛ1, Δsyscall.ERROR_IO_PENDING)) {
        err = default!;
    }
    else { /* default: */
        return (0, err);
    }

    // Need to get our completion message anyway.
    // IO started, and we have to wait for its completion.
    // Wait for our request to complete.
    err = fd.of(FD.Ꮡpd).wait((nint)o.mode, (~fd).isFile);
    if (err == default!) {
        err = windows.WSAGetOverlappedResult((~fd).Sysfd, Ꮡo.of(operation.Ꮡo), Ꮡo.of(operation.Ꮡqty), false, Ꮡo.of(operation.Ꮡflags));
        // All is good. Extract our IO results and return.
        if (err != default!) {
            // More data available. Return back the size of received data.
            if (AreEqual(err, Δsyscall.ERROR_MORE_DATA) || AreEqual(err, windows.WSAEMSGSIZE)) {
                return ((nint)o.qty, err);
            }
            return (0, err);
        }
        return ((nint)o.qty, default!);
    }
    // IO is interrupted by "close" or "timeout"
    var netpollErr = err;
    var exprᴛ2 = netpollErr;
    if (AreEqual(exprᴛ2, ErrNetClosing) || AreEqual(exprᴛ2, ErrFileClosing) || AreEqual(exprᴛ2, ErrDeadlineExceeded)) {
    }
    else { /* default: */
        throw panic("unexpected runtime.netpoll error: " + netpollErr.Error());
    }

    // will deal with those.
    // Cancel our request.
    err = Δsyscall.CancelIoEx((~fd).Sysfd, Ꮡo.of(operation.Ꮡo));
    // Assuming ERROR_NOT_FOUND is returned, if IO is completed.
    if (err != default! && !AreEqual(err, Δsyscall.ERROR_NOT_FOUND)) {
        // TODO(brainman): maybe do something else, but panic.
        throw panic(err);
    }
    // Wait for cancellation to complete.
    fd.of(FD.Ꮡpd).waitCanceled((nint)o.mode);
    err = windows.WSAGetOverlappedResult((~fd).Sysfd, Ꮡo.of(operation.Ꮡo), Ꮡo.of(operation.Ꮡqty), false, Ꮡo.of(operation.Ꮡflags));
    if (err != default!) {
        if (AreEqual(err, Δsyscall.ERROR_OPERATION_ABORTED)) {
            // IO Canceled
            err = netpollErr;
        }
        return (0, err);
    }
    // We issued a cancellation request. But, it seems, IO operation succeeded
    // before the cancellation request run. We need to treat the IO operation as
    // succeeded (the bytes are actually sent/recv from network).
    return ((nint)o.qty, default!);
}

// FD is a file descriptor. The net and os packages embed this type in
// a larger type representing a network connection or OS file.
[GoType] partial struct FD {
    // Lock sysfd and serialize access to Read and Write methods.
    internal fdMutex fdmu;
    // System file descriptor. Immutable until Close.
    public syscallꓸHandle Sysfd;
    // Read operation.
    internal operation rop;
    // Write operation.
    internal operation wop;
    // I/O poller.
    internal pollDesc pd;
    // Used to implement pread/pwrite.
    internal sync.Mutex l;
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
public static (@string, error) Init(this ж<FD> Ꮡfd, @string net, bool pollable) {
    ref var fd = ref Ꮡfd.Value;

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
        err = fd.pd.init(Ꮡfd);
    }
    if (logInitFD != default!) {
        logInitFD(net, Ꮡfd, err);
    }
    if (err != default!) {
        return ("", err);
    }
    if (pollable && useSetFileCompletionNotificationModes) {
        // We do not use events, so we can skip them always.
        var flags = (uint8)Δsyscall.FILE_SKIP_SET_EVENT_ON_HANDLE;
        var exprᴛ2 = net;
        if (exprᴛ2 == "tcp"u8 || exprᴛ2 == "tcp4"u8 || exprᴛ2 == "tcp6"u8 || exprᴛ2 == "udp"u8 || exprᴛ2 == "udp4"u8 || exprᴛ2 == "udp6"u8) {
            flags |= (uint8)(Δsyscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS);
        }

        var errΔ1 = Δsyscall.SetFileCompletionNotificationModes(fd.Sysfd, flags);
        if (errΔ1 == default! && (uint8)(flags & (uint8)Δsyscall.FILE_SKIP_COMPLETION_PORT_ON_SUCCESS) != 0) {
            fd.skipSyncNotif = true;
        }
    }
    // Disable SIO_UDP_CONNRESET behavior.
    // http://support.microsoft.com/kb/263823
    var exprᴛ3 = net;
    if (exprᴛ3 == "udp"u8 || exprᴛ3 == "udp4"u8 || exprᴛ3 == "udp6"u8) {
        ref var ret = ref heap<uint32>(out var Ꮡret);
        ret = (uint32)0;
        ref var flag = ref heap<uint32>(out var Ꮡflag);
        flag = (uint32)0;
        var size = (uint32)@unsafe.Sizeof(flag);
        var errΔ3 = Δsyscall.WSAIoctl(fd.Sysfd, Δsyscall.SIO_UDP_CONNRESET, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡflag)), size, nil, 0, Ꮡret, nil, 0);
        if (errΔ3 != default!) {
            return ("wsaioctl", errΔ3);
        }
    }

    fd.rop.mode = (rune)'r';
    fd.wop.mode = (rune)'w';
    fd.rop.fd = Ꮡfd;
    fd.wop.fd = Ꮡfd;
    fd.rop.runtimeCtx = fd.pd.runtimeCtx;
    fd.wop.runtimeCtx = fd.pd.runtimeCtx;
    return ("", default!);
}

internal static error destroy(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (fd.Sysfd == Δsyscall.InvalidHandle) {
        return Δsyscall.EINVAL;
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
        err = Δsyscall.CloseHandle(fd.Sysfd);
    }

    // The net package uses the CloseFunc variable for testing.
    fd.Sysfd = Δsyscall.InvalidHandle;
    runtime_Semrelease(Ꮡfd.of(FD.Ꮡcsema));
    return err;
}

// Close closes the FD. The underlying file descriptor is closed by
// the destroy method when there are no remaining references.
public static error Close(this ж<FD> Ꮡfd) {
    ref var fd = ref Ꮡfd.Value;

    if (!Ꮡfd.of(FD.Ꮡfdmu).increfAndClose()) {
        return errClosing(fd.isFile);
    }
    if (fd.kind == kindPipe) {
        Δsyscall.CancelIoEx(fd.Sysfd, nil);
    }
    // unblock pending reader and writer
    fd.pd.evict();
    var err = Ꮡfd.decref();
    // Wait until the descriptor is closed. If this was the only
    // reference, it is already closed.
    runtime_Semacquire(Ꮡfd.of(FD.Ꮡcsema));
    return err;
}

// Windows ReadFile and WSARecv use DWORD (uint32) parameter to pass buffer length.
// This prevents us reading blocks larger than 4GB.
// See golang.org/issue/26923.
internal static readonly UntypedInt maxRW = /* 1 << 30 */ 1073741824; // 1GB is large enough and keeps subsequent reads aligned

// Read implements io.Reader.
public static (nint, error) Read(this ж<FD> Ꮡfd, slice<byte> buf) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    nint n = default!;
    error err = default!;
    if (fd.isFile){
        Ꮡfd.of(FD.Ꮡl).Lock();
        defer(Ꮡfd.of(FD.Ꮡl).Unlock);
        var exprᴛ1 = fd.kind;
        if (exprᴛ1 == kindConsole) {
            (n, err) = fd.readConsole(buf);
        }
        else { /* default: */
            (n, err) = Δsyscall.Read(fd.Sysfd, buf);
            if (fd.kind == kindPipe && AreEqual(err, Δsyscall.ERROR_OPERATION_ABORTED)) {
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
        var o = Ꮡfd.of(FD.Ꮡrop);
        o.InitBuf(buf);
        (n, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSARecv((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡflags), oΔ1.of(operation.Ꮡo), nil));
        if (race.Enabled) {
            race.Acquire(new @unsafe.Pointer(ᏑioSync));
        }
    }
    if (len(buf) != 0) {
        err = fd.eofError(n, err);
    }
    return (n, err);
});

public static Func<syscallꓸHandle, ж<uint16>, uint32, ж<uint32>, ж<byte>, error> ReadConsole = Δsyscall.ReadConsole;                                                         // changed for testing

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
        var err = ReadConsole(fd.Sysfd, Ꮡ(fd.readuint16[..(int)(len(fd.readuint16) + 1)], len(fd.readuint16)), (uint32)n, Ꮡnw, nil);
        if (err != default!) {
            return (0, err);
        }
        var uint16s = fd.readuint16[..(int)(len(fd.readuint16) + (nint)nw)];
        fd.readuint16 = fd.readuint16[..0];
        var buf = fd.readbyte[..0];
        for (nint iΔ1 = 0; iΔ1 < len(uint16s); iΔ1++) {
            var r = (rune)uint16s[iΔ1];
            if (utf16.IsSurrogate(r)) {
                if (iΔ1 + 1 == len(uint16s)){
                    if (nw > 0) {
                        // Save half surrogate pair for next time.
                        fd.readuint16 = fd.readuint16[..1];
                        fd.readuint16[0] = (uint16)r;
                        break;
                    }
                    r = utf8.RuneError;
                } else {
                    r = utf16.DecodeRune(r, (rune)uint16s[iΔ1 + 1]);
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
        if (x == 0x1A) {
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
public static (nint, error) Pread(this ж<FD> Ꮡfd, slice<byte> b, int64 off) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (fd.kind == kindPipe) {
        // Pread does not work with pipes
        return (0, Δsyscall.ESPIPE);
    }
    // Call incref, not readLock, because since pread specifies the
    // offset it is independent from other reads.
    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(() => Ꮡfd.decref());
    if (len(b) > maxRW) {
        b = b[..(int)(maxRW)];
    }
    Ꮡfd.of(FD.Ꮡl).Lock();
    defer(Ꮡfd.of(FD.Ꮡl).Unlock);
    var (curoffset, e) = Δsyscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
    if (e != default!) {
        return (0, e);
    }
    deferǃ(Δsyscall.Seek, Ꮡfd.Value.Sysfd, curoffset, (nint)(io.SeekStart), defer);
    ref var o = ref heap<Δsyscall.Overlapped>(out var Ꮡo);
    o = new Δsyscall.Overlapped(
        OffsetHigh: (uint32)((off >> (int)(32))),
        Offset: (uint32)off
    );
    ref var done = ref heap(new uint32(), out var Ꮡdone);
    e = Δsyscall.ReadFile(fd.Sysfd, b, Ꮡdone, Ꮡo);
    if (e != default!) {
        done = 0;
        if (AreEqual(e, Δsyscall.ERROR_HANDLE_EOF)) {
            e = io.EOF;
        }
    }
    if (len(b) != 0) {
        e = fd.eofError((nint)done, e);
    }
    return ((nint)done, e);
});

// ReadFrom wraps the recvfrom network call.
public static (nint, syscallꓸSockaddr, error) ReadFrom(this ж<FD> Ꮡfd, slice<byte> buf) => func<(nint, syscallꓸSockaddr, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(buf) == 0) {
        return (0, default!, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, default!, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> oΔ1) => {
        if ((~oΔ1).rsa == nil) {
            oΔ1.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
        }
        oΔ1.Value.rsan = (int32)@unsafe.Sizeof((~oΔ1).rsa.Value);
        return Δsyscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡflags), (~oΔ1).rsa, oΔ1.of(operation.Ꮡrsan), oΔ1.of(operation.Ꮡo), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, default!, err);
    }
    var (sa, _) = (~o).rsa.Sockaddr();
    return (n, sa, default!);
});

// ReadFromInet4 wraps the recvfrom network call for IPv4.
public static (nint, error) ReadFromInet4(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet4> Ꮡsa4) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(buf) == 0) {
        return (0, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> oΔ1) => {
        if ((~oΔ1).rsa == nil) {
            oΔ1.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
        }
        oΔ1.Value.rsan = (int32)@unsafe.Sizeof((~oΔ1).rsa.Value);
        return Δsyscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡflags), (~oΔ1).rsa, oΔ1.of(operation.Ꮡrsan), oΔ1.of(operation.Ꮡo), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, err);
    }
    rawToSockaddrInet4((~o).rsa, Ꮡsa4);
    return (n, err);
});

// ReadFromInet6 wraps the recvfrom network call for IPv6.
public static (nint, error) ReadFromInet6(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet6> Ꮡsa6) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(buf) == 0) {
        return (0, default!);
    }
    if (len(buf) > maxRW) {
        buf = buf[..(int)(maxRW)];
    }
    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitBuf(buf);
    var (n, err) = execIO(o, (ж<operation> oΔ1) => {
        if ((~oΔ1).rsa == nil) {
            oΔ1.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
        }
        oΔ1.Value.rsan = (int32)@unsafe.Sizeof((~oΔ1).rsa.Value);
        return Δsyscall.WSARecvFrom((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡflags), (~oΔ1).rsa, oΔ1.of(operation.Ꮡrsan), oΔ1.of(operation.Ꮡo), nil);
    });
    err = fd.eofError(n, err);
    if (err != default!) {
        return (n, err);
    }
    rawToSockaddrInet6((~o).rsa, Ꮡsa6);
    return (n, err);
});

// Write implements io.Writer.
public static (nint, error) Write(this ж<FD> Ꮡfd, slice<byte> buf) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (fd.isFile) {
        Ꮡfd.of(FD.Ꮡl).Lock();
        defer(Ꮡfd.of(FD.Ꮡl).Unlock);
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
                (n, err) = Δsyscall.Write(fd.Sysfd, b);
                if (fd.kind == kindPipe && AreEqual(err, Δsyscall.ERROR_OPERATION_ABORTED)) {
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
                race.ReleaseMerge(new @unsafe.Pointer(ᏑioSync));
            }
            var o = Ꮡfd.of(FD.Ꮡwop);
            o.InitBuf(b);
            (n, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSASend((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, oΔ1.of(operation.Ꮡo), nil));
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
    UntypedInt maxWrite = 16000;
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
            var err = Δsyscall.WriteConsole(fd.Sysfd, Ꮡ(uint16s, 0), (uint32)len(uint16s), Ꮡwritten, nil);
            if (err != default!) {
                return (0, err);
            }
            uint16s = uint16s[(int)(written)..];
        }
    }
    return (n, default!);
}

// Pwrite emulates the Unix pwrite system call.
public static (nint, error) Pwrite(this ж<FD> Ꮡfd, slice<byte> buf, int64 off) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (fd.kind == kindPipe) {
        // Pwrite does not work with pipes
        return (0, Δsyscall.ESPIPE);
    }
    // Call incref, not writeLock, because since pwrite specifies the
    // offset it is independent from other writes.
    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(() => Ꮡfd.decref());
    Ꮡfd.of(FD.Ꮡl).Lock();
    defer(Ꮡfd.of(FD.Ꮡl).Unlock);
    var (curoffset, e) = Δsyscall.Seek(fd.Sysfd, 0, io.SeekCurrent);
    if (e != default!) {
        return (0, e);
    }
    deferǃ(Δsyscall.Seek, Ꮡfd.Value.Sysfd, curoffset, (nint)(io.SeekStart), defer);
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        ref var n = ref heap(new uint32(), out var Ꮡn);
        ref var o = ref heap<Δsyscall.Overlapped>(out var Ꮡo);
        o = new Δsyscall.Overlapped(
            OffsetHigh: (uint32)((off >> (int)(32))),
            Offset: (uint32)off
        );
        e = Δsyscall.WriteFile(fd.Sysfd, b, Ꮡn, Ꮡo);
        ntotal += (nint)n;
        if (e != default!) {
            return (ntotal, e);
        }
        buf = buf[(int)(n)..];
        off += (int64)n;
    }
    return (ntotal, default!);
});

// Writev emulates the Unix writev system call.
public static (int64, error) Writev(this ж<FD> Ꮡfd, ж<slice<slice<byte>>> Ꮡbuf) => func<(int64, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;
    ref var buf = ref Ꮡbuf.Value;

    if (len(buf) == 0) {
        return (0, default!);
    }
    {
        var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (race.Enabled) {
        race.ReleaseMerge(new @unsafe.Pointer(ᏑioSync));
    }
    var o = Ꮡfd.of(FD.Ꮡwop);
    o.InitBufs(Ꮡbuf);
    var (n, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSASend((~(~oΔ1).fd).Sysfd, Ꮡ((~oΔ1).bufs, 0), (uint32)len((~oΔ1).bufs), oΔ1.of(operation.Ꮡqty), 0, oΔ1.of(operation.Ꮡo), nil));
    o.ClearBufs();
    TestHookDidWritev(n);
    consume(Ꮡbuf, (int64)n);
    return ((int64)n, err);
});

// WriteTo wraps the sendto network call.
public static (nint, error) WriteTo(this ж<FD> Ꮡfd, slice<byte> buf, syscallꓸSockaddr sa) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(buf);
        o.Value.sa = sa;
        var (n, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSASendto((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, (~oΔ1).sa, oΔ1.of(operation.Ꮡo), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(b);
        o.Value.sa = sa;
        var (n, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSASendto((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, (~oΔ1).sa, oΔ1.of(operation.Ꮡo), nil));
        ntotal += (nint)n;
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// WriteToInet4 is WriteTo, specialized for syscall.SockaddrInet4.
public static (nint, error) WriteToInet4(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet4> Ꮡsa4) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(buf);
        var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendtoInet4((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, Ꮡsa4, oΔ1.of(operation.Ꮡo), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(b);
        var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendtoInet4((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, Ꮡsa4, oΔ1.of(operation.Ꮡo), nil));
        ntotal += (nint)n;
        if (err != default!) {
            return (ntotal, err);
        }
        buf = buf[(int)(n)..];
    }
    return (ntotal, default!);
});

// WriteToInet6 is WriteTo, specialized for syscall.SockaddrInet6.
public static (nint, error) WriteToInet6(this ж<FD> Ꮡfd, slice<byte> buf, ж<Δsyscall.SockaddrInet6> Ꮡsa6) => func<(nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.writeLock(); if (err != default!) {
            return (0, err);
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (len(buf) == 0) {
        // handle zero-byte payload
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(buf);
        var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendtoInet6((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, Ꮡsa6, oΔ1.of(operation.Ꮡo), nil));
        return (n, err);
    }
    nint ntotal = 0;
    while (len(buf) > 0) {
        var b = buf;
        if (len(b) > maxRW) {
            b = b[..(int)(maxRW)];
        }
        var o = Ꮡfd.of(FD.Ꮡwop);
        o.InitBuf(b);
        var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendtoInet6((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), 0, Ꮡsa6, oΔ1.of(operation.Ꮡo), nil));
        ntotal += (nint)n;
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
public static error ConnectEx(this ж<FD> Ꮡfd, syscallꓸSockaddr ra) {
    ref var fd = ref Ꮡfd.Value;

    var o = Ꮡfd.of(FD.Ꮡwop);
    o.Value.sa = ra;
    var (_, err) = execIO(o, (ж<operation> oΔ1) => ConnectExFunc((~(~oΔ1).fd).Sysfd, (~oΔ1).sa, nil, 0, nil, oΔ1.of(operation.Ꮡo)));
    return err;
}

internal static (@string, error) acceptOne(this ж<FD> Ꮡfd, syscallꓸHandle s, slice<Δsyscall.RawSockaddrAny> rawsa, ж<operation> Ꮡo) {
    ref var fd = ref Ꮡfd.Value;
    ref var o = ref Ꮡo.Value;

    // Submit accept request.
    o.handle = s;
    o.rsan = (int32)@unsafe.Sizeof(rawsa[0]);
    var rawsaʗ1 = rawsa;
    var (_, err) = execIO(Ꮡo, (ж<operation> oΔ1) => AcceptFunc((~(~oΔ1).fd).Sysfd, (~oΔ1).handle, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(rawsaʗ1, 0))), 0, (uint32)(~oΔ1).rsan, (uint32)(~oΔ1).rsan, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo)));
    if (err != default!) {
        CloseFunc(s);
        return ("acceptex", err);
    }
    // Inherit properties of the listening socket.
    err = Δsyscall.Setsockopt(s, Δsyscall.SOL_SOCKET, Δsyscall.SO_UPDATE_ACCEPT_CONTEXT, (ж<byte>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡfd.of(FD.ᏑSysfd)).Value)), (int32)@unsafe.Sizeof(fd.Sysfd));
    if (err != default!) {
        CloseFunc(s);
        return ("setsockopt", err);
    }
    return ("", default!);
}

// Accept handles accepting a socket. The sysSocket parameter is used
// to allocate the net socket.
public static (syscallꓸHandle, slice<Δsyscall.RawSockaddrAny>, uint32, @string, error) Accept(this ж<FD> Ꮡfd, Func<(syscallꓸHandle, error)> sysSocket) => func<(syscallꓸHandle, slice<Δsyscall.RawSockaddrAny>, uint32, @string, error)>((defer, recover) => {
    {
        var err = Ꮡfd.readLock(); if (err != default!) {
            return (Δsyscall.InvalidHandle, default!, 0, "", err);
        }
    }
    defer(Ꮡfd.readUnlock);
    var o = Ꮡfd.of(FD.Ꮡrop);
    array<Δsyscall.RawSockaddrAny> rawsa = new(2);
    while (ᐧ) {
        var (s, err) = sysSocket();
        if (err != default!) {
            return (Δsyscall.InvalidHandle, default!, 0, "", err);
        }
        (var errcall, err) = Ꮡfd.acceptOne(s, rawsa[..], o);
        if (err == default!) {
            return (s, rawsa[..], (uint32)(~o).rsan, "", default!);
        }
        // Sometimes we see WSAECONNRESET and ERROR_NETNAME_DELETED is
        // returned here. These happen if connection reset is received
        // before AcceptEx could complete. These errors relate to new
        // connection, not to AcceptEx, so ignore broken connection and
        // try AcceptEx again for more connections.
        var (errno, ok) = err._<Δsyscall.Errno>(ᐧ);
        if (!ok) {
            return (Δsyscall.InvalidHandle, default!, 0, errcall, err);
        }
        var exprᴛ1 = errno;
        if (exprᴛ1 == Δsyscall.ERROR_NETNAME_DELETED || exprᴛ1 == Δsyscall.WSAECONNRESET) {
        }
        else { /* default: */
            return (Δsyscall.InvalidHandle, default!, 0, errcall, err);
        }

    }
});

// ignore these and try again

// Seek wraps syscall.Seek.
public static (int64, error) Seek(this ж<FD> Ꮡfd, int64 offset, nint whence) => func<(int64, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (fd.kind == kindPipe) {
        return (0, Δsyscall.ESPIPE);
    }
    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(() => Ꮡfd.decref());
    Ꮡfd.of(FD.Ꮡl).Lock();
    defer(Ꮡfd.of(FD.Ꮡl).Unlock);
    return Δsyscall.Seek(fd.Sysfd, offset, whence);
});

// Fchmod updates syscall.ByHandleFileInformation.Fileattributes when needed.
public static error Fchmod(this ж<FD> Ꮡfd, uint32 mode) => func<error>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    ref var d = ref heap(new Δsyscall.ByHandleFileInformation(), out var Ꮡd);
    {
        var err = Δsyscall.GetFileInformationByHandle(fd.Sysfd, Ꮡd); if (err != default!) {
            return err;
        }
    }
    var attrs = d.FileAttributes;
    if ((uint32)(mode & (uint32)Δsyscall.S_IWRITE) != 0){
        attrs &= unchecked((uint32)~(uint32)(Δsyscall.FILE_ATTRIBUTE_READONLY));
    } else {
        attrs |= (uint32)(Δsyscall.FILE_ATTRIBUTE_READONLY);
    }
    if (attrs == d.FileAttributes) {
        return default!;
    }
    ref var du = ref heap(new windows.FILE_BASIC_INFO(), out var Ꮡdu);
    du.FileAttributes = attrs;
    return windows.SetFileInformationByHandle(fd.Sysfd, windows.FileBasicInfo, new @unsafe.Pointer(Ꮡdu), (uint32)@unsafe.Sizeof(du));
});

// Fchdir wraps syscall.Fchdir.
public static error Fchdir(this ж<FD> Ꮡfd) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.Fchdir(fd.Sysfd);
});

// GetFileType wraps syscall.GetFileType.
public static (uint32, error) GetFileType(this ж<FD> Ꮡfd) => func<(uint32, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return (0, err);
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.GetFileType(fd.Sysfd);
});

// GetFileInformationByHandle wraps GetFileInformationByHandle.
public static error GetFileInformationByHandle(this ж<FD> Ꮡfd, ж<Δsyscall.ByHandleFileInformation> Ꮡdata) => func((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.incref(); if (err != default!) {
            return err;
        }
    }
    defer(() => Ꮡfd.decref());
    return Δsyscall.GetFileInformationByHandle(fd.Sysfd, Ꮡdata);
});

// RawRead invokes the user-defined function f for a read operation.
public static error RawRead(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => func<error>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.readLock(); if (err != default!) {
            return err;
        }
    }
    defer(Ꮡfd.readUnlock);
    while (ᐧ) {
        if (f((uintptr)fd.Sysfd)) {
            return default!;
        }
        // Use a zero-byte read as a way to get notified when this
        // socket is readable. h/t https://stackoverflow.com/a/42019668/332798
        var o = Ꮡfd.of(FD.Ꮡrop);
        o.InitBuf(default!);
        if (!fd.IsStream) {
            o.Value.flags |= windows.MSG_PEEK;
        }
        var (_, err) = execIO(o, (ж<operation> oΔ1) => Δsyscall.WSARecv((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡbuf), 1, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡflags), oΔ1.of(operation.Ꮡo), nil));
        if (AreEqual(err, windows.WSAEMSGSIZE)){
        } else 
        if (err != default!) {
            // expected with a 0-byte peek, ignore.
            return err;
        }
    }
});

// RawWrite invokes the user-defined function f for a write operation.
public static error RawWrite(this ж<FD> Ꮡfd, Func<uintptr, bool> f) => func<error>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var err = Ꮡfd.writeLock(); if (err != default!) {
            return err;
        }
    }
    defer(Ꮡfd.writeUnlock);
    if (f((uintptr)fd.Sysfd)) {
        return default!;
    }
    // TODO(tmm1): find a way to detect socket writability
    return Δsyscall.EWINDOWS;
});

internal static int32 sockaddrInet4ToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ж<Δsyscall.SockaddrInet4> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.Value;
    ref var sa = ref Ꮡsa.Value;

    rsa = new Δsyscall.RawSockaddrAny(nil);
    var raw = (ж<Δsyscall.RawSockaddrInet4>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    raw.Value.Family = Δsyscall.AF_INET;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(raw.of(Δsyscall.RawSockaddrInet4.ᏑPort)));
    p.Value[0] = (byte)((sa.Port >> (int)(8)));
    p.Value[1] = (byte)sa.Port;
    raw.Value.Addr = sa.Addr;
    return (int32)@unsafe.Sizeof(raw.Value);
}

internal static int32 sockaddrInet6ToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ж<Δsyscall.SockaddrInet6> Ꮡsa) {
    ref var rsa = ref Ꮡrsa.Value;
    ref var sa = ref Ꮡsa.Value;

    rsa = new Δsyscall.RawSockaddrAny(nil);
    var raw = (ж<Δsyscall.RawSockaddrInet6>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    raw.Value.Family = Δsyscall.AF_INET6;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(raw.of(Δsyscall.RawSockaddrInet6.ᏑPort)));
    p.Value[0] = (byte)((sa.Port >> (int)(8)));
    p.Value[1] = (byte)sa.Port;
    raw.Value.Scope_id = sa.ZoneId;
    raw.Value.Addr = sa.Addr;
    return (int32)@unsafe.Sizeof(raw.Value);
}

internal static void rawToSockaddrInet4(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ж<Δsyscall.SockaddrInet4> Ꮡsa) {
    ref var sa = ref Ꮡsa.Value;

    var pp = (ж<Δsyscall.RawSockaddrInet4>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(pp.of(Δsyscall.RawSockaddrInet4.ᏑPort)));
    sa.Port = ((nint)p.Value[0] << (int)(8)) + (nint)p.Value[1];
    sa.Addr = pp.Value.Addr;
}

internal static void rawToSockaddrInet6(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, ж<Δsyscall.SockaddrInet6> Ꮡsa) {
    ref var sa = ref Ꮡsa.Value;

    var pp = (ж<Δsyscall.RawSockaddrInet6>)(uintptr)(new @unsafe.Pointer(Ꮡrsa));
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(pp.of(Δsyscall.RawSockaddrInet6.ᏑPort)));
    sa.Port = ((nint)p.Value[0] << (int)(8)) + (nint)p.Value[1];
    sa.ZoneId = pp.Value.Scope_id;
    sa.Addr = pp.Value.Addr;
}

internal static (int32, error) sockaddrToRaw(ж<Δsyscall.RawSockaddrAny> Ꮡrsa, syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<Δsyscall.SockaddrInet4> saΔ1: {
        var sz = sockaddrInet4ToRaw(Ꮡrsa, saΔ1);
        return (sz, default!);
    }
    case ж<Δsyscall.SockaddrInet6> saΔ1: {
        var sz = sockaddrInet6ToRaw(Ꮡrsa, saΔ1);
        return (sz, default!);
    }
    default: {
        var saΔ1 = sa;
        return (0, Δsyscall.EWINDOWS);
    }}
}

// ReadMsg wraps the WSARecvMsg network call.
public static (nint, nint, nint, syscallꓸSockaddr, error) ReadMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags) => func<(nint, nint, nint, syscallꓸSockaddr, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, default!, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
    }
    o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
    o.Value.msg.Namelen = (int32)@unsafe.Sizeof((~o).rsa.Value);
    o.Value.msg.Flags = (uint32)flags;
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    err = fd.eofError(n, err);
    syscallꓸSockaddr sa = default!;
    if (err == default!) {
        (sa, err) = (~o).rsa.Sockaddr();
    }
    return (n, (nint)(~o).msg.Control.Len, (nint)(~o).msg.Flags, sa, err);
});

// ReadMsgInet4 is ReadMsg, but specialized to return a syscall.SockaddrInet4.
public static (nint, nint, nint, error) ReadMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet4> Ꮡsa4) => func<(nint, nint, nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
    }
    o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
    o.Value.msg.Namelen = (int32)@unsafe.Sizeof((~o).rsa.Value);
    o.Value.msg.Flags = (uint32)flags;
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    err = fd.eofError(n, err);
    if (err == default!) {
        rawToSockaddrInet4((~o).rsa, Ꮡsa4);
    }
    return (n, (nint)(~o).msg.Control.Len, (nint)(~o).msg.Flags, err);
});

// ReadMsgInet6 is ReadMsg, but specialized to return a syscall.SockaddrInet6.
public static (nint, nint, nint, error) ReadMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, nint flags, ж<Δsyscall.SockaddrInet6> Ꮡsa6) => func<(nint, nint, nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    {
        var errΔ1 = Ꮡfd.readLock(); if (errΔ1 != default!) {
            return (0, 0, 0, errΔ1);
        }
    }
    defer(Ꮡfd.readUnlock);
    if (len(p) > maxRW) {
        p = p[..(int)(maxRW)];
    }
    var o = Ꮡfd.of(FD.Ꮡrop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
    }
    o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
    o.Value.msg.Namelen = (int32)@unsafe.Sizeof((~o).rsa.Value);
    o.Value.msg.Flags = (uint32)flags;
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSARecvMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    err = fd.eofError(n, err);
    if (err == default!) {
        rawToSockaddrInet6((~o).rsa, Ꮡsa6);
    }
    return (n, (nint)(~o).msg.Control.Len, (nint)(~o).msg.Flags, err);
});

// WriteMsg wraps the WSASendMsg network call.
public static (nint, nint, error) WriteMsg(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, syscallꓸSockaddr sa) => func<(nint, nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(Ꮡfd.writeUnlock);
    var o = Ꮡfd.of(FD.Ꮡwop);
    o.InitMsg(p, oob);
    if (sa != default!) {
        if ((~o).rsa == nil) {
            o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
        }
        var (lenΔ1, errΔ2) = sockaddrToRaw((~o).rsa, sa);
        if (errΔ2 != default!) {
            return (0, 0, errΔ2);
        }
        o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
        o.Value.msg.Namelen = lenΔ1;
    }
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), 0, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    return (n, (nint)(~o).msg.Control.Len, err);
});

// WriteMsgInet4 is WriteMsg specialized for syscall.SockaddrInet4.
public static (nint, nint, error) WriteMsgInet4(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet4> Ꮡsa) => func<(nint, nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(Ꮡfd.writeUnlock);
    var o = Ꮡfd.of(FD.Ꮡwop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
    }
    var lenΔ1 = sockaddrInet4ToRaw((~o).rsa, Ꮡsa);
    o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
    o.Value.msg.Namelen = lenΔ1;
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), 0, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    return (n, (nint)(~o).msg.Control.Len, err);
});

// WriteMsgInet6 is WriteMsg specialized for syscall.SockaddrInet6.
public static (nint, nint, error) WriteMsgInet6(this ж<FD> Ꮡfd, slice<byte> p, slice<byte> oob, ж<Δsyscall.SockaddrInet6> Ꮡsa) => func<(nint, nint, error)>((defer, recover) => {
    ref var fd = ref Ꮡfd.Value;

    if (len(p) > maxRW) {
        return (0, 0, errors.New("packet is too large (only 1GB is allowed)"u8));
    }
    {
        var errΔ1 = Ꮡfd.writeLock(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    defer(Ꮡfd.writeUnlock);
    var o = Ꮡfd.of(FD.Ꮡwop);
    o.InitMsg(p, oob);
    if ((~o).rsa == nil) {
        o.Value.rsa = @new<Δsyscall.RawSockaddrAny>();
    }
    var lenΔ1 = sockaddrInet6ToRaw((~o).rsa, Ꮡsa);
    o.Value.msg.Name = ((Δsyscall.Pointer)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer((~o).rsa)));
    o.Value.msg.Namelen = lenΔ1;
    var (n, err) = execIO(o, (ж<operation> oΔ1) => windows.WSASendMsg((~(~oΔ1).fd).Sysfd, oΔ1.of(operation.Ꮡmsg), 0, oΔ1.of(operation.Ꮡqty), oΔ1.of(operation.Ꮡo), nil));
    return (n, (nint)(~o).msg.Control.Len, err);
});

public static (nint, @string, error) DupCloseOnExec(nint fd) {
    var (proc, err) = Δsyscall.GetCurrentProcess();
    if (err != default!) {
        return (0, "GetCurrentProcess", err);
    }
    ref var nfd = ref heap(new syscallꓸHandle(), out var Ꮡnfd);
    const bool inherit = false; // analogous to CLOEXEC
    {
        var errΔ1 = Δsyscall.DuplicateHandle(proc, ((syscallꓸHandle)(uintptr)fd), proc, Ꮡnfd, 0, inherit, Δsyscall.DUPLICATE_SAME_ACCESS); if (errΔ1 != default!) {
            return (0, "DuplicateHandle", errΔ1);
        }
    }
    return ((nint)(uintptr)nfd, "", default!);
}

} // end poll_package
