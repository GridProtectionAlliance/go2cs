// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package runtime -- go2cs converted at 2022 March 06 22:08:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\lock_js.go
using _@unsafe_ = go.@unsafe_package;
using System.Threading;
using System;


namespace go;

public static partial class runtime_package {

    // js/wasm has no support for threads yet. There is no preemption.
private static readonly nint mutex_unlocked = 0;
private static readonly nint mutex_locked = 1;

private static readonly nint note_cleared = 0;
private static readonly nint note_woken = 1;
private static readonly nint note_timeout = 2;

private static readonly nint active_spin = 4;
private static readonly nint active_spin_cnt = 30;
private static readonly nint passive_spin = 1;


private static void @lock(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    lockWithRank(l, getLockRank(l));
}

private static void lock2(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    if (l.key == mutex_locked) { 
        // js/wasm is single-threaded so we should never
        // observe this.
        throw("self deadlock");

    }
    var gp = getg();
    if (gp.m.locks < 0) {
        throw("lock count");
    }
    gp.m.locks++;
    l.key = mutex_locked;

}

private static void unlock(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    unlockWithRank(l);
}

private static void unlock2(ptr<mutex> _addr_l) {
    ref mutex l = ref _addr_l.val;

    if (l.key == mutex_unlocked) {
        throw("unlock of unlocked lock");
    }
    var gp = getg();
    gp.m.locks--;
    if (gp.m.locks < 0) {
        throw("lock count");
    }
    l.key = mutex_unlocked;

}

// One-time notifications.

private partial struct noteWithTimeout {
    public ptr<g> gp;
    public long deadline;
}

private static var notes = make_map<ptr<note>, ptr<g>>();private static var notesWithTimeout = make_map<ptr<note>, noteWithTimeout>();

private static void noteclear(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    n.key = note_cleared;
}

private static void notewakeup(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;
 
    // gp := getg()
    if (n.key == note_woken) {
        throw("notewakeup - double wakeup");
    }
    var cleared = n.key == note_cleared;
    n.key = note_woken;
    if (cleared) {
        goready(notes[n], 1);
    }
}

private static void notesleep(ptr<note> _addr_n) {
    ref note n = ref _addr_n.val;

    throw("notesleep not supported by js");
}

private static bool notetsleep(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    throw("notetsleep not supported by js");
    return false;
}

// same as runtime·notetsleep, but called on user g (not g0)
private static bool notetsleepg(ptr<note> _addr_n, long ns) {
    ref note n = ref _addr_n.val;

    var gp = getg();
    if (gp == gp.m.g0) {
        throw("notetsleepg on g0");
    }
    if (ns >= 0) {
        var deadline = nanotime() + ns;
        var delay = ns / 1000000 + 1; // round up
        if (delay > 1 << 31 - 1) {
            delay = 1 << 31 - 1; // cap to max int32
        }
        var id = scheduleTimeoutEvent(delay);
        var mp = acquirem();
        notes[n] = gp;
        notesWithTimeout[n] = new noteWithTimeout(gp:gp,deadline:deadline);
        releasem(mp);

        gopark(null, null, waitReasonSleep, traceEvNone, 1);

        clearTimeoutEvent(id); // note might have woken early, clear timeout
        clearIdleID();

        mp = acquirem();
        delete(notes, n);
        delete(notesWithTimeout, n);
        releasem(mp);

        return n.key == note_woken;

    }
    while (n.key != note_woken) {
        mp = acquirem();
        notes[n] = gp;
        releasem(mp);

        gopark(null, null, waitReasonZero, traceEvNone, 1);

        mp = acquirem();
        delete(notes, n);
        releasem(mp);
    }
    return true;

}

// checkTimeouts resumes goroutines that are waiting on a note which has reached its deadline.
private static void checkTimeouts() {
    var now = nanotime();
    foreach (var (n, nt) in notesWithTimeout) {
        if (n.key == note_cleared && now >= nt.deadline) {
            n.key = note_timeout;
            goready(nt.gp, 1);
        }
    }
}

// events is a stack of calls from JavaScript into Go.
private static slice<ptr<event>> events = default;

private partial struct @event {
    public ptr<g> gp; // returned reports whether the event handler has returned.
// When all goroutines are idle and the event handler has returned,
// then g gets resumed and returns the execution to JavaScript.
    public bool returned;
}

// The timeout event started by beforeIdle.
private static int idleID = default;

// beforeIdle gets called by the scheduler if no goroutine is awake.
// If we are not already handling an event, then we pause for an async event.
// If an event handler returned, we resume it and it will pause the execution.
// beforeIdle either returns the specific goroutine to schedule next or
// indicates with otherReady that some goroutine became ready.
private static (ptr<g>, bool) beforeIdle(long now, long pollUntil) {
    ptr<g> gp = default!;
    bool otherReady = default;

    var delay = int64(-1);
    if (pollUntil != 0) {
        delay = pollUntil - now;
    }
    if (delay > 0) {
        clearIdleID();
        if (delay < 1e6F) {
            delay = 1;
        }
        else if (delay < 1e15F) {
            delay = delay / 1e6F;
        }
        else
 { 
            // An arbitrary cap on how long to wait for a timer.
            // 1e9 ms == ~11.5 days.
            delay = 1e9F;

        }
        idleID = scheduleTimeoutEvent(delay);

    }
    if (len(events) == 0) {
        go_(() => handleAsyncEvent());
        return (_addr_null!, true);
    }
    var e = events[len(events) - 1];
    if (e.returned) {
        return (_addr_e.gp!, false);
    }
    return (_addr_null!, false);

}

private static void handleAsyncEvent() {
    pause(getcallersp() - 16);
}

// clearIdleID clears our record of the timeout started by beforeIdle.
private static void clearIdleID() {
    if (idleID != 0) {
        clearTimeoutEvent(idleID);
        idleID = 0;
    }
}

// pause sets SP to newsp and pauses the execution of Go's WebAssembly code until an event is triggered.
private static void pause(System.UIntPtr newsp);

// scheduleTimeoutEvent tells the WebAssembly environment to trigger an event after ms milliseconds.
// It returns a timer id that can be used with clearTimeoutEvent.
private static int scheduleTimeoutEvent(long ms);

// clearTimeoutEvent clears a timeout event scheduled by scheduleTimeoutEvent.
private static void clearTimeoutEvent(int id);

// handleEvent gets invoked on a call from JavaScript into Go. It calls the event handler of the syscall/js package
// and then parks the handler goroutine to allow other goroutines to run before giving execution back to JavaScript.
// When no other goroutine is awake any more, beforeIdle resumes the handler goroutine. Now that the same goroutine
// is running as was running when the call came in from JavaScript, execution can be safely passed back to JavaScript.
private static void handleEvent() {
    ptr<event> e = addr(new event(gp:getg(),returned:false,));
    events = append(events, e);

    eventHandler();

    clearIdleID(); 

    // wait until all goroutines are idle
    e.returned = true;
    gopark(null, null, waitReasonZero, traceEvNone, 1);

    events[len(events) - 1] = null;
    events = events[..(int)len(events) - 1]; 

    // return execution to JavaScript
    pause(getcallersp() - 16);

}

private static Action eventHandler = default;

//go:linkname setEventHandler syscall/js.setEventHandler
private static void setEventHandler(Action fn) {
    eventHandler = fn;
}

} // end runtime_package
