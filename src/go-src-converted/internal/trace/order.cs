// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2022 March 13 06:35:59 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Program Files\Go\src\internal\trace\order.go
namespace go.@internal;

using fmt = fmt_package;
using sort = sort_package;

public static partial class trace_package {

private partial struct eventBatch {
    public slice<ptr<Event>> events;
    public bool selected;
}

private partial struct orderEvent {
    public ptr<Event> ev;
    public nint batch;
    public ulong g;
    public gState init;
    public gState next;
}

private partial struct gStatus { // : nint
}

private partial struct gState {
    public ulong seq;
    public gStatus status;
}

private static readonly gStatus gDead = iota;
private static readonly var gRunnable = 0;
private static readonly var gRunning = 1;
private static readonly unordered gWaiting = ~uint64(0);
private static readonly var garbage = ~uint64(0) - 1;
private static readonly var noseq = ~uint64(0);
private static readonly var seqinc = ~uint64(0) - 1;

// order1007 merges a set of per-P event batches into a single, consistent stream.
// The high level idea is as follows. Events within an individual batch are in
// correct order, because they are emitted by a single P. So we need to produce
// a correct interleaving of the batches. To do this we take first unmerged event
// from each batch (frontier). Then choose subset that is "ready" to be merged,
// that is, events for which all dependencies are already merged. Then we choose
// event with the lowest timestamp from the subset, merge it and repeat.
// This approach ensures that we form a consistent stream even if timestamps are
// incorrect (condition observed on some machines).
private static (slice<ptr<Event>>, error) order1007(map<nint, slice<ptr<Event>>> m) => func((_, panic, _) => {
    slice<ptr<Event>> events = default;
    error err = default!;

    nint pending = 0;
    slice<ptr<eventBatch>> batches = default;
    foreach (var (_, v) in m) {
        pending += len(v);
        batches = append(batches, addr(new eventBatch(v,false)));
    }    var gs = make_map<ulong, gState>();
    slice<orderEvent> frontier = default;
    while (pending != 0) {
        foreach (var (i, b) in batches) {
            if (b.selected || len(b.events) == 0) {
                continue;
            }
            var ev = b.events[0];
            var (g, init, next) = stateTransition(_addr_ev);
            if (!transitionReady(g, gs[g], init)) {
                continue;
        pending--;
            }
            frontier = append(frontier, new orderEvent(ev,i,g,init,next));
            b.events = b.events[(int)1..];
            b.selected = true; 
            // Get rid of "Local" events, they are intended merely for ordering.

            if (ev.Type == EvGoStartLocal) 
                ev.Type = EvGoStart;
            else if (ev.Type == EvGoUnblockLocal) 
                ev.Type = EvGoUnblock;
            else if (ev.Type == EvGoSysExitLocal) 
                ev.Type = EvGoSysExit;
                    }        if (len(frontier) == 0) {
            return (null, error.As(fmt.Errorf("no consistent ordering of events possible"))!);
        }
        sort.Sort(orderEventList(frontier));
        var f = frontier[0];
        frontier[0] = frontier[len(frontier) - 1];
        frontier = frontier[..(int)len(frontier) - 1];
        events = append(events, f.ev);
        transition(gs, f.g, f.init, f.next);
        if (!batches[f.batch].selected) {
            panic("frontier batch is not selected");
        }
        batches[f.batch].selected = false;
    } 

    // At this point we have a consistent stream of events.
    // Make sure time stamps respect the ordering.
    // The tests will skip (not fail) the test case if they see this error.
    if (!sort.IsSorted(eventList(events))) {
        return (null, error.As(ErrTimeOrder)!);
    }
    var lastSysBlock = make_map<ulong, long>();
    {
        var ev__prev1 = ev;

        foreach (var (_, __ev) in events) {
            ev = __ev;

            if (ev.Type == EvGoSysBlock || ev.Type == EvGoInSyscall) 
                lastSysBlock[ev.G] = ev.Ts;
            else if (ev.Type == EvGoSysExit) 
                var ts = int64(ev.Args[2]);
                if (ts == 0) {
                    continue;
                }
                var block = lastSysBlock[ev.G];
                if (block == 0) {
                    return (null, error.As(fmt.Errorf("stray syscall exit"))!);
                }
                if (ts < block) {
                    return (null, error.As(ErrTimeOrder)!);
                }
                ev.Ts = ts;
                    }
        ev = ev__prev1;
    }

    sort.Stable(eventList(events));

    return ;
});

// stateTransition returns goroutine state (sequence and status) when the event
// becomes ready for merging (init) and the goroutine state after the event (next).
private static (ulong, gState, gState) stateTransition(ptr<Event> _addr_ev) {
    ulong g = default;
    gState init = default;
    gState next = default;
    ref Event ev = ref _addr_ev.val;


    if (ev.Type == EvGoCreate) 
        g = ev.Args[0];
        init = new gState(0,gDead);
        next = new gState(1,gRunnable);
    else if (ev.Type == EvGoWaiting || ev.Type == EvGoInSyscall) 
        g = ev.G;
        init = new gState(1,gRunnable);
        next = new gState(2,gWaiting);
    else if (ev.Type == EvGoStart || ev.Type == EvGoStartLabel) 
        g = ev.G;
        init = new gState(ev.Args[1],gRunnable);
        next = new gState(ev.Args[1]+1,gRunning);
    else if (ev.Type == EvGoStartLocal) 
        // noseq means that this event is ready for merging as soon as
        // frontier reaches it (EvGoStartLocal is emitted on the same P
        // as the corresponding EvGoCreate/EvGoUnblock, and thus the latter
        // is already merged).
        // seqinc is a stub for cases when event increments g sequence,
        // but since we don't know current seq we also don't know next seq.
        g = ev.G;
        init = new gState(noseq,gRunnable);
        next = new gState(seqinc,gRunning);
    else if (ev.Type == EvGoBlock || ev.Type == EvGoBlockSend || ev.Type == EvGoBlockRecv || ev.Type == EvGoBlockSelect || ev.Type == EvGoBlockSync || ev.Type == EvGoBlockCond || ev.Type == EvGoBlockNet || ev.Type == EvGoSleep || ev.Type == EvGoSysBlock || ev.Type == EvGoBlockGC) 
        g = ev.G;
        init = new gState(noseq,gRunning);
        next = new gState(noseq,gWaiting);
    else if (ev.Type == EvGoSched || ev.Type == EvGoPreempt) 
        g = ev.G;
        init = new gState(noseq,gRunning);
        next = new gState(noseq,gRunnable);
    else if (ev.Type == EvGoUnblock || ev.Type == EvGoSysExit) 
        g = ev.Args[0];
        init = new gState(ev.Args[1],gWaiting);
        next = new gState(ev.Args[1]+1,gRunnable);
    else if (ev.Type == EvGoUnblockLocal || ev.Type == EvGoSysExitLocal) 
        g = ev.Args[0];
        init = new gState(noseq,gWaiting);
        next = new gState(seqinc,gRunnable);
    else if (ev.Type == EvGCStart) 
        g = garbage;
        init = new gState(ev.Args[0],gDead);
        next = new gState(ev.Args[0]+1,gDead);
    else 
        // no ordering requirements
        g = unordered;
        return ;
}

private static bool transitionReady(ulong g, gState curr, gState init) {
    return g == unordered || (init.seq == noseq || init.seq == curr.seq) && init.status == curr.status;
}

private static void transition(map<ulong, gState> gs, ulong g, gState init, gState next) => func((_, panic, _) => {
    if (g == unordered) {
        return ;
    }
    var curr = gs[g];
    if (!transitionReady(g, curr, init)) {
        panic("event sequences are broken");
    }

    if (next.seq == noseq) 
        next.seq = curr.seq;
    else if (next.seq == seqinc) 
        next.seq = curr.seq + 1;
        gs[g] = next;
});

// order1005 merges a set of per-P event batches into a single, consistent stream.
private static (slice<ptr<Event>>, error) order1005(map<nint, slice<ptr<Event>>> m) {
    slice<ptr<Event>> events = default;
    error err = default!;

    foreach (var (_, batch) in m) {
        events = append(events, batch);
    }    foreach (var (_, ev) in events) {
        if (ev.Type == EvGoSysExit) { 
            // EvGoSysExit emission is delayed until the thread has a P.
            // Give it the real sequence number and time stamp.
            ev.seq = int64(ev.Args[1]);
            if (ev.Args[2] != 0) {
                ev.Ts = int64(ev.Args[2]);
            }
        }
    }    sort.Sort(eventSeqList(events));
    if (!sort.IsSorted(eventList(events))) {
        return (null, error.As(ErrTimeOrder)!);
    }
    return ;
}

private partial struct orderEventList { // : slice<orderEvent>
}

private static nint Len(this orderEventList l) {
    return len(l);
}

private static bool Less(this orderEventList l, nint i, nint j) {
    return l[i].ev.Ts < l[j].ev.Ts;
}

private static void Swap(this orderEventList l, nint i, nint j) {
    (l[i], l[j]) = (l[j], l[i]);
}

private partial struct eventList { // : slice<ptr<Event>>
}

private static nint Len(this eventList l) {
    return len(l);
}

private static bool Less(this eventList l, nint i, nint j) {
    return l[i].Ts < l[j].Ts;
}

private static void Swap(this eventList l, nint i, nint j) {
    (l[i], l[j]) = (l[j], l[i]);
}

private partial struct eventSeqList { // : slice<ptr<Event>>
}

private static nint Len(this eventSeqList l) {
    return len(l);
}

private static bool Less(this eventSeqList l, nint i, nint j) {
    return l[i].seq < l[j].seq;
}

private static void Swap(this eventSeqList l, nint i, nint j) {
    (l[i], l[j]) = (l[j], l[i]);
}

} // end trace_package
