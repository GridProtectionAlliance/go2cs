// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δio = io_package;
using os = os_package;
using Δsync = sync_package;
using time = time_package;

partial class net_package {

// pipeDeadline is an abstraction for handling timeouts.
[GoType] partial struct pipeDeadline {
    internal Δsync.Mutex mu; // Guards timer and cancel
    internal ж<time.Timer> timer;
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
internal static void set(this ж<pipeDeadline> Ꮡd, time.Time t) => func((defer, recover) => {
    ref var d = ref Ꮡd.Value;

    Ꮡd.of(pipeDeadline.Ꮡmu).Lock();
    defer(Ꮡd.of(pipeDeadline.Ꮡmu).Unlock);
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
                builtin.close(Ꮡd.Value.cancel);
            });
            return;
        }
    }
    // Time in the past, so close immediately.
    if (!closed) {
        builtin.close(d.cancel);
    }
});

// wait returns a channel that is closed when the deadline is exceeded.
internal static channel<EmptyStruct> wait(this ж<pipeDeadline> Ꮡd) => func((defer, recover) => {
    ref var d = ref Ꮡd.Value;

    Ꮡd.of(pipeDeadline.Ꮡmu).Lock();
    defer(Ꮡd.of(pipeDeadline.Ꮡmu).Unlock);
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
    internal Δsync.Mutex wrMu; // Serialize Write operations
    // Used by local Read to interact with remote Write.
    // Successful receive on rdRx is always followed by send on rdTx.
    internal /*<-*/channel<slice<byte>> rdRx;
    internal channel/*<-*/<nint> rdTx;
    // Used by local Write to interact with remote Read.
    // Successful send on wrTx is always followed by receive on wrRx.
    internal channel/*<-*/<slice<byte>> wrTx;
    internal /*<-*/channel<nint> wrRx;
    internal Δsync.Once once; // Protects closing localDone
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
    return (new pipeжConn(p1), new pipeжConn(p2));
}

[GoRecv] internal static ΔAddr LocalAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

[GoRecv] internal static ΔAddr RemoteAddr(this ref pipe _) {
    return new pipeAddr(nil);
}

internal static (nint, error) Read(this ж<pipe> Ꮡp, slice<byte> b) {
    var (n, err) = Ꮡp.read(b);
    if (err != default! && !AreEqual(err, Δio.EOF) && !AreEqual(err, Δio.ErrClosedPipe)) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "read"u8, Net: "pipe"u8, Err: err)));
    }
    return (n, err);
}

internal static (nint n, error err) read(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;

    ref var p = ref Ꮡp.Value;
    switch (ᐧ) {
    case {} when isClosedChan(p.localDone): {
        return (0, Δio.ErrClosedPipe);
    }
    case {} when isClosedChan(p.remoteDone): {
        return (0, Δio.EOF);
    }
    case {} when isClosedChan(Ꮡp.of(pipe.ᏑreadDeadline).wait()): {
        return (0, os.ErrDeadlineExceeded);
    }}

    switch (select(ᐸꟷ(p.rdRx, ꓸꓸꓸ), ᐸꟷ(p.localDone, ꓸꓸꓸ), ᐸꟷ(p.remoteDone, ꓸꓸꓸ), ᐸꟷ(Ꮡp.of(pipe.ᏑreadDeadline).wait(), ꓸꓸꓸ))) {
    case 0 when p.rdRx.ꟷᐳ(out var bw): {
        nint nr = copy(b, bw);
        p.rdTx.ᐸꟷ(nr);
        return (nr, default!);
    }
    case 1 when p.localDone.ꟷᐳ(out _): {
        return (0, Δio.ErrClosedPipe);
    }
    case 2 when p.remoteDone.ꟷᐳ(out _): {
        return (0, Δio.EOF);
    }
    case 3 when Ꮡp.of(pipe.ᏑreadDeadline).wait().ꟷᐳ(out _): {
        return (0, os.ErrDeadlineExceeded);
    }}
    return default!;
}

internal static (nint, error) Write(this ж<pipe> Ꮡp, slice<byte> b) {
    var (n, err) = Ꮡp.write(b);
    if (err != default! && !AreEqual(err, Δio.ErrClosedPipe)) {
        err = new OpErrorжerror(Ꮡ(new OpError(Op: "write"u8, Net: "pipe"u8, Err: err)));
    }
    return (n, err);
}

internal static (nint n, error err) write(this ж<pipe> Ꮡp, slice<byte> b) {
    nint n = default!;
    error err = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        switch (ᐧ) {
        case {} when isClosedChan(p.localDone): {
            (n, err) = (0, Δio.ErrClosedPipe); return;
        }
        case {} when isClosedChan(p.remoteDone): {
            (n, err) = (0, Δio.ErrClosedPipe); return;
        }
        case {} when isClosedChan(Ꮡp.of(pipe.ᏑwriteDeadline).wait()): {
            (n, err) = (0, os.ErrDeadlineExceeded); return;
        }}

        Ꮡp.of(pipe.ᏑwrMu).Lock();
        // Ensure entirety of b is written together
        defer(Ꮡp.of(pipe.ᏑwrMu).Unlock);
        for (var once = true; once || len(b) > 0; once = false) {
            switch (select(p.wrTx.ᐸꟷ(b, ꓸꓸꓸ), ᐸꟷ(p.localDone, ꓸꓸꓸ), ᐸꟷ(p.remoteDone, ꓸꓸꓸ), ᐸꟷ(Ꮡp.of(pipe.ᏑwriteDeadline).wait(), ꓸꓸꓸ))) {
            case 0: {
                nint nw = ᐸꟷ(p.wrRx);
                b = b[(int)(nw)..];
                n += nw;
                break;
            }
            case 1 when p.localDone.ꟷᐳ(out _): {
                (n, err) = (n, Δio.ErrClosedPipe); return;
            }
            case 2 when p.remoteDone.ꟷᐳ(out _): {
                (n, err) = (n, Δio.ErrClosedPipe); return;
            }
            case 3 when Ꮡp.of(pipe.ᏑwriteDeadline).wait().ꟷᐳ(out _): {
                (n, err) = (n, os.ErrDeadlineExceeded); return;
            }}
        }
        (n, err) = (n, default!);
    });
    return (n, err);
}

internal static error SetDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.Value;

    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑreadDeadline).set(t);
    Ꮡp.of(pipe.ᏑwriteDeadline).set(t);
    return default!;
}

internal static error SetReadDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.Value;

    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑreadDeadline).set(t);
    return default!;
}

internal static error SetWriteDeadline(this ж<pipe> Ꮡp, time.Time t) {
    ref var p = ref Ꮡp.Value;

    if (isClosedChan(p.localDone) || isClosedChan(p.remoteDone)) {
        return Δio.ErrClosedPipe;
    }
    Ꮡp.of(pipe.ᏑwriteDeadline).set(t);
    return default!;
}

internal static error Close(this ж<pipe> Ꮡp) {
    Ꮡp.of(pipe.Ꮡonce).Do(() => {
        builtin.close(Ꮡp.Value.localDone);
    });
    return default!;
}

} // end net_package
