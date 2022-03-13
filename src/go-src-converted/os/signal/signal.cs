// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package signal -- go2cs converted at 2022 March 13 05:28:33 UTC
// import "os/signal" ==> using signal = go.os.signal_package
// Original source: C:\Program Files\Go\src\os\signal\signal.go
namespace go.os;

using context = context_package;
using os = os_package;
using sync = sync_package;
using System;
using System.Threading;

public static partial class signal_package {

private static var handlers = default;

private partial struct stopping {
    public channel<os.Signal> c;
    public ptr<handler> h;
}

private partial struct handler {
    public array<uint> mask;
}

private static bool want(this ptr<handler> _addr_h, nint sig) {
    ref handler h = ref _addr_h.val;

    return (h.mask[sig / 32] >> (int)(uint(sig & 31))) & 1 != 0;
}

private static void set(this ptr<handler> _addr_h, nint sig) {
    ref handler h = ref _addr_h.val;

    h.mask[sig / 32] |= 1 << (int)(uint(sig & 31));
}

private static void clear(this ptr<handler> _addr_h, nint sig) {
    ref handler h = ref _addr_h.val;

    h.mask[sig / 32] &= 1 << (int)(uint(sig & 31));
}

// Stop relaying the signals, sigs, to any channels previously registered to
// receive them and either reset the signal handlers to their original values
// (action=disableSignal) or ignore the signals (action=ignoreSignal).
private static void cancel(slice<os.Signal> sigs, Action<nint> action) => func((defer, _, _) => {
    handlers.Lock();
    defer(handlers.Unlock());

    Action<nint> remove = n => {
        handler zerohandler = default;

        foreach (var (c, h) in handlers.m) {
            if (h.want(n)) {
                handlers.@ref[n]--;
                h.clear(n);
                if (h.mask == zerohandler.mask) {
                    delete(handlers.m, c);
                }
            }
        }        action(n);
    };

    if (len(sigs) == 0) {
        for (nint n = 0; n < numSig; n++) {
            remove(n);
        }
    else
    } {
        foreach (var (_, s) in sigs) {
            remove(signum(s));
        }
    }
});

// Ignore causes the provided signals to be ignored. If they are received by
// the program, nothing will happen. Ignore undoes the effect of any prior
// calls to Notify for the provided signals.
// If no signals are provided, all incoming signals will be ignored.
public static void Ignore(params os.Signal[] sig) {
    sig = sig.Clone();

    cancel(sig, ignoreSignal);
}

// Ignored reports whether sig is currently ignored.
public static bool Ignored(os.Signal sig) {
    var sn = signum(sig);
    return sn >= 0 && signalIgnored(sn);
}

 
// watchSignalLoopOnce guards calling the conditionally
// initialized watchSignalLoop. If watchSignalLoop is non-nil,
// it will be run in a goroutine lazily once Notify is invoked.
// See Issue 21576.
private static sync.Once watchSignalLoopOnce = default;private static Action watchSignalLoop = default;

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
// The only way to remove signals from the set is to call Stop.
//
// It is allowed to call Notify multiple times with different channels
// and the same signals: each channel receives copies of incoming
// signals independently.
public static void Notify(channel<os.Signal> c, params os.Signal[] sig) => func((defer, panic, _) => {
    sig = sig.Clone();

    if (c == null) {
        panic("os/signal: Notify using nil channel");
    }
    handlers.Lock();
    defer(handlers.Unlock());

    var h = handlers.m[c];
    if (h == null) {
        if (handlers.m == null) {
            handlers.m = make_map<channel<os.Signal>, ptr<handler>>();
        }
        h = @new<handler>();
        handlers.m[c] = h;
    }
    Action<nint> add = n => {
        if (n < 0) {
            return ;
        }
        if (!h.want(n)) {
            h.set(n);
            if (handlers.@ref[n] == 0) {
                enableSignal(n); 

                // The runtime requires that we enable a
                // signal before starting the watcher.
                watchSignalLoopOnce.Do(() => {
                    if (watchSignalLoop != null) {
                        go_(() => watchSignalLoop());
                    }
                });
            }
            handlers.@ref[n]++;
        }
    };

    if (len(sig) == 0) {
        for (nint n = 0; n < numSig; n++) {
            add(n);
        }
    else
    } {
        foreach (var (_, s) in sig) {
            add(signum(s));
        }
    }
});

// Reset undoes the effect of any prior calls to Notify for the provided
// signals.
// If no signals are provided, all signal handlers will be reset.
public static void Reset(params os.Signal[] sig) {
    sig = sig.Clone();

    cancel(sig, disableSignal);
}

// Stop causes package signal to stop relaying incoming signals to c.
// It undoes the effect of all prior calls to Notify using c.
// When Stop returns, it is guaranteed that c will receive no more signals.
public static void Stop(channel<os.Signal> c) {
    handlers.Lock();

    var h = handlers.m[c];
    if (h == null) {
        handlers.Unlock();
        return ;
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

    handlers.stopping = append(handlers.stopping, new stopping(c,h));

    handlers.Unlock();

    signalWaitUntilIdle();

    handlers.Lock();

    foreach (var (i, s) in handlers.stopping) {
        if (s.c == c) {
            handlers.stopping = append(handlers.stopping[..(int)i], handlers.stopping[(int)i + 1..]);
            break;
        }
    }    handlers.Unlock();
}

// Wait until there are no more signals waiting to be delivered.
// Defined by the runtime package.
private static void signalWaitUntilIdle();

private static void process(os.Signal sig) => func((defer, _, _) => {
    var n = signum(sig);
    if (n < 0) {>>MARKER:FUNCTION_signalWaitUntilIdle_BLOCK_PREFIX<<
        return ;
    }
    handlers.Lock();
    defer(handlers.Unlock());

    foreach (var (c, h) in handlers.m) {
        if (h.want(n)) { 
            // send but do not block for it
        }
    }    foreach (var (_, d) in handlers.stopping) {
        if (d.h.want(n)) {
        }
    }
});

// NotifyContext returns a copy of the parent context that is marked done
// (its Done channel is closed) when one of the listed signals arrives,
// when the returned stop function is called, or when the parent context's
// Done channel is closed, whichever happens first.
//
// The stop function unregisters the signal behavior, which, like signal.Reset,
// may restore the default behavior for a given signal. For example, the default
// behavior of a Go program receiving os.Interrupt is to exit. Calling
// NotifyContext(parent, os.Interrupt) will change the behavior to cancel
// the returned context. Future interrupts received will not trigger the default
// (exit) behavior until the returned stop function is called.
//
// The stop function releases resources associated with it, so code should
// call stop as soon as the operations running in this Context complete and
// signals no longer need to be diverted to the context.
public static (context.Context, context.CancelFunc) NotifyContext(context.Context parent, params os.Signal[] signals) {
    context.Context ctx = default;
    context.CancelFunc stop = default;
    signals = signals.Clone();

    var (ctx, cancel) = context.WithCancel(parent);
    ptr<signalCtx> c = addr(new signalCtx(Context:ctx,cancel:cancel,signals:signals,));
    c.ch = make_channel<os.Signal>(1);
    Notify(c.ch, c.signals);
    if (ctx.Err() == null) {
        go_(() => () => {
            c.cancel();
        }());
    }
    return (c, c.stop);
}

private partial struct signalCtx : context.Context {
    public ref context.Context Context => ref Context_val;
    public context.CancelFunc cancel;
    public slice<os.Signal> signals;
    public channel<os.Signal> ch;
}

private static void stop(this ptr<signalCtx> _addr_c) {
    ref signalCtx c = ref _addr_c.val;

    c.cancel();
    Stop(c.ch);
}

private partial interface stringer {
    @string String();
}

private static @string String(this ptr<signalCtx> _addr_c) {
    ref signalCtx c = ref _addr_c.val;

    slice<byte> buf = default; 
    // We know that the type of c.Context is context.cancelCtx, and we know that the
    // String method of cancelCtx returns a string that ends with ".WithCancel".
    stringer name = stringer.As(c.Context._<stringer>().String())!;
    name = stringer.As(name[..(int)len(name) - len(".WithCancel")])!;
    buf = append(buf, "signal.NotifyContext(" + name);
    if (len(c.signals) != 0) {
        buf = append(buf, ", [");
        foreach (var (i, s) in c.signals) {
            buf = append(buf, s.String());
            if (i != len(c.signals) - 1) {
                buf = append(buf, ' ');
            }
        }        buf = append(buf, ']');
    }
    buf = append(buf, ')');
    return string(buf);
}

} // end signal_package
