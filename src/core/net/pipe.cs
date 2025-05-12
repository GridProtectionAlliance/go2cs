// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using io = io_package;
using os = os_package;
using sync = sync_package;
using time = time_package;

partial class net_package {

// pipeDeadline is an abstraction for handling timeouts.
[GoType] partial struct pipeDeadline {
    internal sync_package.Mutex mu; // Guards timer and cancel
    internal ж<time_package.Timer> timer;
    internal channel<EmptyStruct> cancel; // Must be non-nil
}

internal static pipeDeadline makePipeDeadline() {
    return new pipeDeadline(cancel: new channel<EmptyStruct>(1));
}

// set sets the point in time when the deadline will time out.
// A timeout event is signaled by closing the channel returned by waiter.
// Once a timeout has occurred, the deadline can be refreshed by specifying a
// t value in the future.
//
// A zero value for t prevents timeout.
[GoRecv] internal static void set(this ref pipeDeadline d, time.Time t) => func((defer, _) => {
    d.mu.Lock();
    defer(d.mu.Unlock);
    if (d.timer != nil && !d.timer.Stop()) {
        ᐸꟷ(d.cancel);
    }
    // Wait for the timer callback to finish and close cancel
    d.timer = default!;
    // Time is zero, then there is no deadline.
    var closed = isClosedChan(d.cancel);
    if (t.IsZero()) {
        if (closed) {
            d.cancel = new channel<EmptyStruct>(1);
        }
        return;
    }
    // Time in the future, setup a timer to cancel in the future.
    {
        var dur = time.Until(t); if (dur > 0) {
            if (closed) {
                d.cancel = new channel<EmptyStruct>(1);
            }
            d.timer = time.AfterFunc(dur, () => {
                close(d.cancel);
            });
            return;
        }
    }
    // Time in the past, so close immediately.
    if (!closed) {
        close(d.cancel);
    }
});

// wait returns a channel that is closed when the deadline is exceeded.
[GoRecv] internal static channel<EmptyStruct> wait(this ref pipeDeadline d) => func((defer, _) => {
    d.mu.Lock();
    defer(d.mu.Unlock);
    return d.cancel;
});

internal static bool isClosedChan(/*<-*/channel<EmptyStruct> c) {
    switch (ᐧ) {
    case ᐧ when c.ꟷᐳ(out _): {
        return true;
    }
    default: {
        return false;
    }}
}

[GoType] partial struct pipeAddr {
}

internal static @string Network(this pipeAddr _) {
    return "pipe"u8;
}

internal static @string String(this pipeAddr _) {
    return "pipe"u8;
}

[GoType] partial struct pipe {
    internal sync_package.Mutex wrMu; // Serialize Write operations
    // Used by local Read to interact with remote Write.
    // Successful receive on rdRx is always followed by send on rdTx.
    internal /*<-*/channel<slice<byte>> rdRx;
    internal channel/*<-*/<nint> rdTx;
    // Used by local Write to interact with remote Read.
    // Successful send on wrTx is always followed by receive on wrRx.
    internal channel/*<-*/<slice<byte>> wrTx;
    internal /*<-*/channel<nint> wrRx;
    internal sync_package.Once once; // Protects closing localDone
    internal channel<EmptyStruct> localDone;
    internal /*<-*/channel<EmptyStruct> remoteDone;
    internal pipeDeadline readDeadline;
    internal pipeDeadline writeDeadline;
}

// Pipe creates a synchronous, in-memory, full duplex
// network connection; both ends implement the [Conn] interface.
// Reads on one end are matched with writes on the other,
// copying data directly between the two; there is no internal
// buffering.
public static (Conn, Conn) Pipe() {
    var cb1 = new channel<slice<byte>>(1);
    var cb2 = new channel<slice<byte>>(1);
    var cn1 = new channel<nint>(1);
    var cn2 = new channel<nint>(1);
    var done1 = new channel<EmptyStruct>(1);
    var done2 = new channel<EmptyStruct>(1);
    var p1 = Ꮡ(new pipe(
        rdRx: cb1, rdTx: cn1,
        wrTx: cb2, wrRx: cn2,
        localDone: done1, remoteDone: done2,
        readDeadline: makePipeDeadline(),
        writeDeadline: makePipeDeadline()
    ));
    var p2 = Ꮡ(new pipe(
        rdRx: cb2, rdTx: cn2,
        wrTx: cb1, wrRx: cn1,
        localDone: done2, remoteDone: done1,
        readDeadline: makePipeDeadline(),
        writeDeadline: makePipeDeadline()
    ));
    return (~p1, ~p2);
}

[GoRecv] internal static ΔAddr LocalAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

[GoRecv] internal static ΔAddr RemoteAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

[GoRecv] internal static (nint, error) Read(this ref pipe p, slice<byte> b) {
    var (n, err) = p.read(b);
    if (err != default! && !AreEqual(err, io.EOF) && !AreEqual(err, io.ErrClosedPipe)) {
        Ꮡerr = new OpError(Op: "read"u8, Net: "pipe"u8, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

[GoRecv] internal static (nint n, error err) read(this ref pipe p, slice<byte> b) {
    nint n = default!;
    error err = default!;

    switch (ᐧ) {
    case {} when isClosedChan(p.localDone): {
        return (0, io.ErrClosedPipe);
    }
    case {} when isClosedChan(p.remoteDone): {
        return (0, io.EOF);
    }
    case {} when isClosedChan(p.readDeadline.wait()): {
        return (0, os.ErrDeadlineExceeded);
    }}

    switch (select(ᐸꟷ(p.rdRx, ꓸꓸꓸ), ᐸꟷ(p.localDone, ꓸꓸꓸ), ᐸꟷ(p.remoteDone, ꓸꓸꓸ), ᐸꟷ(p.readDeadline.wait(), ꓸꓸꓸ))) {
    case 0 when p.rdRx.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdTx.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when p.localDone.ꟷᐳ(out _): {
        return (0, io.ErrClosedPipe);
    }
    case 2 when p.remoteDone.ꟷᐳ(out _): {
        return (0, io.EOF);
    }
    case 3 when p.readDeadline.wait().ꟷᐳ(out _): {
        return (0, os.ErrDeadlineExceeded);
    }}
}

[GoRecv] internal static (nint, error) Write(this ref pipe p, slice<byte> b) {
    var (n, err) = p.write(b);
    if (err != default! && !AreEqual(err, io.ErrClosedPipe)) {
        Ꮡerr = new OpError(Op: "write"u8, Net: "pipe"u8, Err: err); err = ref Ꮡerr.val;
    }
    return (n, err);
}

[GoRecv] internal static (nint n, error err) write(this ref pipe p, slice<byte> b) => func((defer, _) => {
    nint n = default!;
    error err = default!;

    switch (ᐧ) {
    case {} when isClosedChan(p.localDone): {
        return (0, io.ErrClosedPipe);
    }
    case {} when isClosedChan(p.remoteDone): {
        return (0, io.ErrClosedPipe);
    }
    case {} when isClosedChan(p.writeDeadline.wait()): {
        return (0, os.ErrDeadlineExceeded);
    }}

    p.wrMu.Lock();
    // Ensure entirety of b is written together
    defer(p.wrMu.Unlock);
    for (var once = true; once || len(b) > 0; once = false) {
        switch (select(p.wrTx.ᐸꟷ(b, ꓸꓸꓸ), ᐸꟷ(p.localDone, ꓸꓸꓸ), ᐸꟷ(p.remoteDone, ꓸꓸꓸ), ᐸꟷ(p.writeDeadline.wait(), ꓸꓸꓸ))) {
        case 0: {
            nint nw = ᐸꟷ(p.wrRx);
            b = b[(int)(nw)..];
            n += nw;
            break;
        }
        case 1 when p.localDone.ꟷᐳ(out _): {
            return (n, io.ErrClosedPipe);
        }
        case 2 when p.remoteDone.ꟷᐳ(out _): {
            return (n, io.ErrClosedPipe);
        }
        case 3 when p.writeDeadline.wait().ꟷᐳ(out _): {
            return (n, os.ErrDeadlineExceeded);
        }}
    }
    return (n, default!);
});

[GoRecv] internal static error SetDeadline(this ref pipe p, time.Time t) {
    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return io.ErrClosedPipe;
    }
    p.readDeadline.set(t);
    p.writeDeadline.set(t);
    return default!;
}

[GoRecv] internal static error SetReadDeadline(this ref pipe p, time.Time t) {
    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return io.ErrClosedPipe;
    }
    p.readDeadline.set(t);
    return default!;
}

[GoRecv] internal static error SetWriteDeadline(this ref pipe p, time.Time t) {
    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return io.ErrClosedPipe;
    }
    p.writeDeadline.set(t);
    return default!;
}

[GoRecv] internal static error Close(this ref pipe p) {
    p.once.Do(() => {
        close(p.localDone);
    });
    return default!;
}

} // end net_package
