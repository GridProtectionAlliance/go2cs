// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using context = context_package;
using os = os_package;
using sync = sync_package;
using ꓸꓸꓸosꓸSignal = Span<osꓸSignal>;

partial class signal_package {


[GoType("dyn")] partial struct handlersᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    // Map a channel to the signals that should be sent to it.
    internal map<channel/*<-*/<os.Signal>*handler>, > m;
    // Map a signal to the number of channels receiving it.
    internal array<int64> @ref = new(numSig);
    // Map channels to signals while the channel is being stopped.
    // Not a map because entries live here only very briefly.
    // We need a separate container because we need m to correspond to ref
    // at all times, and we also need to keep track of the *handler
    // value for a channel being stopped. See the Stop function.
    internal slice<stopping> stopping;
}
internal static handlersᴛ1 handlers;

[GoType] partial struct stopping {
    internal channel/*<-*/<osꓸSignal> c;
    internal ж<handler> h;
}

[GoType] partial struct handler {
    internal array<uint32> mask = new((numSig + 31) / 32);
}

[GoRecv] internal static bool want(this ref handler h, nint sig) {
    return (uint32)((h.mask[sig / 32] >> (int)(((nuint)((nint)(sig & 31))))) & 1) != 0;
}

[GoRecv] internal static void set(this ref handler h, nint sig) {
    h.mask[sig / 32] |= (uint32)(1 << (int)(((nuint)((nint)(sig & 31)))));
}

[GoRecv] internal static void clear(this ref handler h, nint sig) {
    h.mask[sig / 32] &= ~(uint32)(1 << (int)(((nuint)((nint)(sig & 31)))));
}

// Stop relaying the signals, sigs, to any channels previously registered to
// receive them and either reset the signal handlers to their original values
// (action=disableSignal) or ignore the signals (action=ignoreSignal).
internal static void cancel(slice<osꓸSignal> sigs, Action<nint> action) => func((defer, _) => {
    handlers.Lock();
    var handlersʗ1 = handlers;
    defer(handlersʗ1.Unlock);
    var remove = 
    var handlersʗ2 = handlers;
    (nint n) => {
        ref var zerohandler = ref heap(new handler(), out var Ꮡzerohandler);
        foreach (var (c, h) in handlersʗ2.m) {
            if (h.want(n)) {
                handlersʗ2.@ref[n]--;
                h.clear(n);
                if ((~h).mask == zerohandler.mask) {
                    delete(handlersʗ2.m, c);
                }
            }
        }
        action(n);
    };
    if (len(sigs) == 0){
        for (nint n = 0; n < numSig; n++) {
            remove(n);
        }
    } else {
        foreach (var (_, s) in sigs) {
            remove(signum(s));
        }
    }
});

// Ignore causes the provided signals to be ignored. If they are received by
// the program, nothing will happen. Ignore undoes the effect of any prior
// calls to [Notify] for the provided signals.
// If no signals are provided, all incoming signals will be ignored.
public static void Ignore(params ꓸꓸꓸosꓸSignal sigʗp) {
    var sig = sigʗp.slice();

    cancel(sig, ignoreSignal);
}

// Ignored reports whether sig is currently ignored.
public static bool Ignored(osꓸSignal sig) {
    nint sn = signum(sig);
    return sn >= 0 && signalIgnored(sn);
}

internal static sync.Once watchSignalLoopOnce;
internal static Action watchSignalLoop;

// Notify causes package signal to relay incoming signals to c.
// If no signals are provided, all incoming signals will be relayed to c.
// Otherwise, just the provided signals will.
//
// Package signal will not block sending to c: the caller must ensure
// that c has sufficient buffer space to keep up with the expected
// signal rate. For a channel used for notification of just one signal value,
// a buffer of size 1 is sufficient.
//
// It is allowed to call Notify multiple times with the same channel:
// each call expands the set of signals sent to that channel.
// The only way to remove signals from the set is to call [Stop].
//
// It is allowed to call Notify multiple times with different channels
// and the same signals: each channel receives copies of incoming
// signals independently.
public static void Notify(channel/*<-*/<osꓸSignal> c, params ꓸꓸꓸosꓸSignal sigʗp) => func((defer, _) => {
    var sig = sigʗp.slice();

    if (c == default!) {
        throw panic("os/signal: Notify using nil channel");
    }
    handlers.Lock();
    var handlersʗ1 = handlers;
    defer(handlersʗ1.Unlock);
    var h = handlers.m[c];
    if (h == nil) {
        if (handlers.m == default!) {
            handlers.m = new map<channel/*<-*/<os.Signal>*handler>, >();
        }
        h = @new<handler>();
        handlers.m[c] = h;
    }
    var add = 
    var hʗ1 = h;
    var handlersʗ2 = handlers;
    var watchSignalLoopOnceʗ1 = watchSignalLoopOnce;
    (nint n) => {
        if (n < 0) {
            return;
        }
        if (!hʗ1.want(n)) {
            hʗ1.set(n);
            if (handlersʗ2.@ref[n] == 0) {
                enableSignal(n);
                // The runtime requires that we enable a
                // signal before starting the watcher.
                watchSignalLoopOnceʗ1.Do(
                () => {
                    if (watchSignalLoop != default!) {
                        goǃ(watchSignalLoop);
                    }
                });
            }
            handlers.@ref[n]++;
        }
    };
    if (len(sig) == 0){
        for (nint n = 0; n < numSig; n++) {
            add(n);
        }
    } else {
        foreach (var (_, s) in sig) {
            add(signum(s));
        }
    }
});

// Reset undoes the effect of any prior calls to [Notify] for the provided
// signals.
// If no signals are provided, all signal handlers will be reset.
public static void Reset(params ꓸꓸꓸosꓸSignal sigʗp) {
    var sig = sigʗp.slice();

    cancel(sig, disableSignal);
}

// Stop causes package signal to stop relaying incoming signals to c.
// It undoes the effect of all prior calls to [Notify] using c.
// When Stop returns, it is guaranteed that c will receive no more signals.
public static void Stop(channel/*<-*/<osꓸSignal> c) {
    handlers.Lock();
    var h = handlers.m[c];
    if (h == nil) {
        handlers.Unlock();
        return;
    }
    delete(handlers.m, c);
    for (nint n = 0; n < numSig; n++) {
        if (h.want(n)) {
            handlers.@ref[n]--;
            if (handlers.@ref[n] == 0) {
                disableSignal(n);
            }
        }
    }
    // Signals will no longer be delivered to the channel.
    // We want to avoid a race for a signal such as SIGINT:
    // it should be either delivered to the channel,
    // or the program should take the default action (that is, exit).
    // To avoid the possibility that the signal is delivered,
    // and the signal handler invoked, and then Stop deregisters
    // the channel before the process function below has a chance
    // to send it on the channel, put the channel on a list of
    // channels being stopped and wait for signal delivery to
    // quiesce before fully removing it.
    handlers.stopping = append(handlers.stopping, new stopping(c, h));
    handlers.Unlock();
    signalWaitUntilIdle();
    handlers.Lock();
    foreach (var (i, s) in handlers.stopping) {
        if (s.c == c) {
            handlers.stopping = append(handlers.stopping[..(int)(i)], handlers.stopping[(int)(i + 1)..].ꓸꓸꓸ);
            break;
        }
    }
    handlers.Unlock();
}

// Wait until there are no more signals waiting to be delivered.
// Defined by the runtime package.
internal static partial void signalWaitUntilIdle();

internal static void process(osꓸSignal sig) => func((defer, _) => {
    nint n = signum(sig);
    if (n < 0) {
        return;
    }
    handlers.Lock();
    var handlersʗ1 = handlers;
    defer(handlersʗ1.Unlock);
    foreach (var (c, h) in handlers.m) {
        if (h.want(n)) {
            // send but do not block for it
            switch (ᐧ) {
            case ᐧ: {
                break;
            }
            default: {
                break;
            }}
        }
    }
    // Avoid the race mentioned in Stop.
    foreach (var (_, d) in handlers.stopping) {
        if (d.h.want(n)) {
            switch (ᐧ) {
            case ᐧ: {
                break;
            }
            default: {
                break;
            }}
        }
    }
});

// NotifyContext returns a copy of the parent context that is marked done
// (its Done channel is closed) when one of the listed signals arrives,
// when the returned stop function is called, or when the parent context's
// Done channel is closed, whichever happens first.
//
// The stop function unregisters the signal behavior, which, like [signal.Reset],
// may restore the default behavior for a given signal. For example, the default
// behavior of a Go program receiving [os.Interrupt] is to exit. Calling
// NotifyContext(parent, os.Interrupt) will change the behavior to cancel
// the returned context. Future interrupts received will not trigger the default
// (exit) behavior until the returned stop function is called.
//
// The stop function releases resources associated with it, so code should
// call stop as soon as the operations running in this Context complete and
// signals no longer need to be diverted to the context.
public static (context.Context ctx, context.CancelFunc stop) NotifyContext(context.Context parent, params ꓸꓸꓸosꓸSignal signalsʗp) {
    context.Context ctx = default!;
    context.CancelFunc stop = default!;
    var signals = signalsʗp.slice();

    (ctx, cancel) = context.WithCancel(parent);
    var c = Ꮡ(new signalCtx(
        Context: ctx,
        cancel: cancel,
        signals: signals
    ));
    c.val.ch = new channel<osꓸSignal>(1);
    Notify((~c).ch, (~c).signals.ꓸꓸꓸ);
    if (ctx.Err() == default!) {
        var cʗ1 = c;
        goǃ(() => {
            switch (select(ᐸꟷ((~cʗ1).ch, ꓸꓸꓸ), ᐸꟷ(cʗ1.Done(), ꓸꓸꓸ))) {
            case 0 when (~cʗ1).ch.ꟷᐳ(out _): {
                (~cʗ1).cancel();
                break;
            }
            case 1 when cʗ1.Done().ꟷᐳ(out _): {
                break;
            }}
        });
    }
    return (~c, c.stop);
}

[GoType] partial struct signalCtx {
    public partial ref context_package.Context Context { get; }
    internal context_package.CancelFunc cancel;
    internal slice<osꓸSignal> signals;
    internal channel<osꓸSignal> ch;
}

[GoRecv] internal static void stop(this ref signalCtx c) {
    c.cancel();
    Stop(c.ch);
}

[GoType] partial interface stringer {
    @string String();
}

[GoRecv] internal static @string String(this ref signalCtx c) {
    slice<byte> buf = default!;
    // We know that the type of c.Context is context.cancelCtx, and we know that the
    // String method of cancelCtx returns a string that ends with ".WithCancel".
    @string name = c.Context._<stringer>().String();
    name = name[..(int)(len(name) - len(".WithCancel"))];
    buf = append(buf, "signal.NotifyContext("u8 + name.ꓸꓸꓸ);
    if (len(c.signals) != 0) {
        buf = append(buf, ", ["u8.ꓸꓸꓸ);
        foreach (var (i, s) in c.signals) {
            buf = append(buf, s.String().ꓸꓸꓸ);
            if (i != len(c.signals) - 1) {
                buf = append(buf, (rune)' ');
            }
        }
        buf = append(buf, (rune)']');
    }
    buf = append(buf, (rune)')');
    return ((@string)buf);
}

} // end signal_package
