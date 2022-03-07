// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2022 March 06 23:22:58 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Program Files\Go\src\internal\trace\parser.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using _@unsafe_ = go.@unsafe_package;
using System;


namespace go.@internal;

public static partial class trace_package {

private static @string goCmd() {
    @string exeSuffix = default;
    if (runtime.GOOS == "windows") {
        exeSuffix = ".exe";
    }
    var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
    {
        var (_, err) = os.Stat(path);

        if (err == null) {
            return path;
        }
    }

    return "go";

}

// Event describes one event in the trace.
public partial struct Event {
    public nint Off; // offset in input file (for debugging and error reporting)
    public byte Type; // one of Ev*
    public long seq; // sequence number
    public long Ts; // timestamp in nanoseconds
    public nint P; // P on which the event happened (can be one of TimerP, NetpollP, SyscallP)
    public ulong G; // G on which the event happened
    public ulong StkID; // unique stack ID
    public slice<ptr<Frame>> Stk; // stack trace (can be empty)
    public array<ulong> Args; // event-type-specific arguments
    public slice<@string> SArgs; // event-type-specific string args
// linked event (can be nil), depends on event type:
// for GCStart: the GCStop
// for GCSTWStart: the GCSTWDone
// for GCSweepStart: the GCSweepDone
// for GoCreate: first GoStart of the created goroutine
// for GoStart/GoStartLabel: the associated GoEnd, GoBlock or other blocking event
// for GoSched/GoPreempt: the next GoStart
// for GoBlock and other blocking events: the unblock event
// for GoUnblock: the associated GoStart
// for blocking GoSysCall: the associated GoSysExit
// for GoSysExit: the next GoStart
// for GCMarkAssistStart: the associated GCMarkAssistDone
// for UserTaskCreate: the UserTaskEnd
// for UserRegion: if the start region, the corresponding UserRegion end event
    public ptr<Event> Link;
}

// Frame is a frame in stack traces.
public partial struct Frame {
    public ulong PC;
    public @string Fn;
    public @string File;
    public nint Line;
}

 
// Special P identifiers:
public static readonly nint FakeP = 1000000 + iota;
public static readonly var TimerP = 0; // depicts timer unblocks
public static readonly var NetpollP = 1; // depicts network unblocks
public static readonly var SyscallP = 2; // depicts returns from syscalls
public static readonly var GCP = 3; // depicts GC state

// ParseResult is the result of Parse.
public partial struct ParseResult {
    public slice<ptr<Event>> Events; // Stacks is the stack traces keyed by stack IDs from the trace.
    public map<ulong, slice<ptr<Frame>>> Stacks;
}

// Parse parses, post-processes and verifies the trace.
public static (ParseResult, error) Parse(io.Reader r, @string bin) {
    ParseResult _p0 = default;
    error _p0 = default!;

    var (ver, res, err) = parse(r, bin);
    if (err != null) {
        return (new ParseResult(), error.As(err)!);
    }
    if (ver < 1007 && bin == "") {
        return (new ParseResult(), error.As(fmt.Errorf("for traces produced by go 1.6 or below, the binary argument must be provided"))!);
    }
    return (res, error.As(null!)!);

}

// parse parses, post-processes and verifies the trace. It returns the
// trace version and the list of events.
private static (nint, ParseResult, error) parse(io.Reader r, @string bin) {
    nint _p0 = default;
    ParseResult _p0 = default;
    error _p0 = default!;

    var (ver, rawEvents, strings, err) = readTrace(r);
    if (err != null) {
        return (0, new ParseResult(), error.As(err)!);
    }
    var (events, stacks, err) = parseEvents(ver, rawEvents, strings);
    if (err != null) {
        return (0, new ParseResult(), error.As(err)!);
    }
    events = removeFutile(events);
    err = postProcessTrace(ver, events);
    if (err != null) {
        return (0, new ParseResult(), error.As(err)!);
    }
    foreach (var (_, ev) in events) {
        if (ev.StkID != 0) {
            ev.Stk = stacks[ev.StkID];
        }
    }    if (ver < 1007 && bin != "") {
        {
            var err = symbolize(events, bin);

            if (err != null) {
                return (0, new ParseResult(), error.As(err)!);
            }

        }

    }
    return (ver, new ParseResult(Events:events,Stacks:stacks), error.As(null!)!);

}

// rawEvent is a helper type used during parsing.
private partial struct rawEvent {
    public nint off;
    public byte typ;
    public slice<ulong> args;
    public slice<@string> sargs;
}

// readTrace does wire-format parsing and verification.
// It does not care about specific event types and argument meaning.
private static (nint, slice<rawEvent>, map<ulong, @string>, error) readTrace(io.Reader r) {
    nint ver = default;
    slice<rawEvent> events = default;
    map<ulong, @string> strings = default;
    error err = default!;
 
    // Read and validate trace header.
    array<byte> buf = new array<byte>(16);
    var (off, err) = io.ReadFull(r, buf[..]);
    if (err != null) {
        err = fmt.Errorf("failed to read header: read %v, err %v", off, err);
        return ;
    }
    ver, err = parseHeader(buf[..]);
    if (err != null) {
        return ;
    }
    switch (ver) {
        case 1005: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.

        case 1007: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.

        case 1008: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.

        case 1009: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.

        case 1010: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.

        case 1011: 
            // Note: When adding a new version, add canned traces
            // from the old version to the test suite using mkcanned.bash.
            break;
            break;
        default: 
            err = fmt.Errorf("unsupported trace file version %v.%v (update Go toolchain) %v", ver / 1000, ver % 1000, ver);
            return ;
            break;
    } 

    // Read events.
    strings = make_map<ulong, @string>();
    while (true) { 
        // Read event type and number of arguments (1 byte).
        var off0 = off;
        nint n = default;
        n, err = r.Read(buf[..(int)1]);
        if (err == io.EOF) {
            err = null;
            break;
        }
        if (err != null || n != 1) {
            err = fmt.Errorf("failed to read trace at offset 0x%x: n=%v err=%v", off0, n, err);
            return ;
        }
        off += n;
        var typ = buf[0] << 2 >> 2;
        var narg = buf[0] >> 6 + 1;
        var inlineArgs = byte(4);
        if (ver < 1007) {
            narg++;
            inlineArgs++;
        }
        if (typ == EvNone || typ >= EvCount || EventDescriptions[typ].minVersion > ver) {
            err = fmt.Errorf("unknown event type %v at offset 0x%x", typ, off0);
            return ;
        }
        if (typ == EvString) { 
            // String dictionary entry [ID, length, string].
            ulong id = default;
            id, off, err = readVal(r, off);
            if (err != null) {
                return ;
            }

            if (id == 0) {
                err = fmt.Errorf("string at offset %d has invalid id 0", off);
                return ;
            }

            if (strings[id] != "") {
                err = fmt.Errorf("string at offset %d has duplicate id %v", off, id);
                return ;
            }

            ulong ln = default;
            ln, off, err = readVal(r, off);
            if (err != null) {
                return ;
            }

            if (ln == 0) {
                err = fmt.Errorf("string at offset %d has invalid length 0", off);
                return ;
            }

            if (ln > 1e6F) {
                err = fmt.Errorf("string at offset %d has too large length %v", off, ln);
                return ;
            }

            buf = make_slice<byte>(ln);
            n = default;
            n, err = io.ReadFull(r, buf);
            if (err != null) {
                err = fmt.Errorf("failed to read trace at offset %d: read %v, want %v, error %v", off, n, ln, err);
                return ;
            }

            off += n;
            strings[id] = string(buf);
            continue;

        }
        rawEvent ev = new rawEvent(typ:typ,off:off0);
        if (narg < inlineArgs) {
            for (nint i = 0; i < int(narg); i++) {
                ulong v = default;
                v, off, err = readVal(r, off);
                if (err != null) {
                    err = fmt.Errorf("failed to read event %v argument at offset %v (%v)", typ, off, err);
                    return ;
                }
                ev.args = append(ev.args, v);
            }
        else


        } { 
            // More than inlineArgs args, the first value is length of the event in bytes.
            v = default;
            v, off, err = readVal(r, off);
            if (err != null) {
                err = fmt.Errorf("failed to read event %v argument at offset %v (%v)", typ, off, err);
                return ;
            }

            var evLen = v;
            var off1 = off;
            while (evLen > uint64(off - off1)) {
                v, off, err = readVal(r, off);
                if (err != null) {
                    err = fmt.Errorf("failed to read event %v argument at offset %v (%v)", typ, off, err);
                    return ;
                }
                ev.args = append(ev.args, v);
            }

            if (evLen != uint64(off - off1)) {
                err = fmt.Errorf("event has wrong length at offset 0x%x: want %v, got %v", off0, evLen, off - off1);
                return ;
            }

        }

        if (ev.typ == EvUserLog) // EvUserLog records are followed by a value string of length ev.args[len(ev.args)-1]
            @string s = default;
            s, off, err = readStr(r, off);
            ev.sargs = append(ev.sargs, s);
                events = append(events, ev);

    }
    return ;

}

private static (@string, nint, error) readStr(io.Reader r, nint off0) {
    @string s = default;
    nint off = default;
    error err = default!;

    ulong sz = default;
    sz, off, err = readVal(r, off0);
    if (err != null || sz == 0) {
        return ("", off, error.As(err)!);
    }
    if (sz > 1e6F) {
        return ("", off, error.As(fmt.Errorf("string at offset %d is too large (len=%d)", off, sz))!);
    }
    var buf = make_slice<byte>(sz);
    var (n, err) = io.ReadFull(r, buf);
    if (err != null || sz != uint64(n)) {
        return ("", off + n, error.As(fmt.Errorf("failed to read trace at offset %d: read %v, want %v, error %v", off, n, sz, err))!);
    }
    return (string(buf), off + n, error.As(null!)!);

}

// parseHeader parses trace header of the form "go 1.7 trace\x00\x00\x00\x00"
// and returns parsed version as 1007.
private static (nint, error) parseHeader(slice<byte> buf) {
    nint _p0 = default;
    error _p0 = default!;

    if (len(buf) != 16) {
        return (0, error.As(fmt.Errorf("bad header length"))!);
    }
    if (buf[0] != 'g' || buf[1] != 'o' || buf[2] != ' ' || buf[3] < '1' || buf[3] > '9' || buf[4] != '.' || buf[5] < '1' || buf[5] > '9') {
        return (0, error.As(fmt.Errorf("not a trace file"))!);
    }
    var ver = int(buf[5] - '0');
    nint i = 0;
    while (buf[6 + i] >= '0' && buf[6 + i] <= '9' && i < 2) {
        ver = ver * 10 + int(buf[6 + i] - '0');
        i++;
    }
    ver += int(buf[3] - '0') * 1000;
    if (!bytes.Equal(buf[(int)6 + i..], (slice<byte>)" trace\x00\x00\x00\x00"[..(int)10 - i])) {
        return (0, error.As(fmt.Errorf("not a trace file"))!);
    }
    return (ver, error.As(null!)!);

}

// Parse events transforms raw events into events.
// It does analyze and verify per-event-type arguments.
private static (slice<ptr<Event>>, map<ulong, slice<ptr<Frame>>>, error) parseEvents(nint ver, slice<rawEvent> rawEvents, map<ulong, @string> strings) {
    slice<ptr<Event>> events = default;
    map<ulong, slice<ptr<Frame>>> stacks = default;
    error err = default!;

    long ticksPerSec = default;    long lastSeq = default;    long lastTs = default;

    ulong lastG = default;
    nint lastP = default;
    var timerGoids = make_map<ulong, bool>();
    var lastGs = make_map<nint, ulong>(); // last goroutine running on P
    stacks = make_map<ulong, slice<ptr<Frame>>>();
    var batches = make_map<nint, slice<ptr<Event>>>(); // events by P
    foreach (var (_, raw) in rawEvents) {
        var desc = EventDescriptions[raw.typ];
        if (desc.Name == "") {
            err = fmt.Errorf("missing description for event type %v", raw.typ);
            return ;
        }
        var narg = argNum(raw, ver);
        if (len(raw.args) != narg) {
            err = fmt.Errorf("%v has wrong number of arguments at offset 0x%x: want %v, got %v", desc.Name, raw.off, narg, len(raw.args));
            return ;
        }

        if (raw.typ == EvBatch) 
            lastGs[lastP] = lastG;
            lastP = int(raw.args[0]);
            lastG = lastGs[lastP];
            if (ver < 1007) {
                lastSeq = int64(raw.args[1]);
                lastTs = int64(raw.args[2]);
            }
            else
 {
                lastTs = int64(raw.args[1]);
            }

        else if (raw.typ == EvFrequency) 
            ticksPerSec = int64(raw.args[0]);
            if (ticksPerSec <= 0) { 
                // The most likely cause for this is tick skew on different CPUs.
                // For example, solaris/amd64 seems to have wildly different
                // ticks on different CPUs.
                err = ErrTimeOrder;
                return ;

            }

        else if (raw.typ == EvTimerGoroutine) 
            timerGoids[raw.args[0]] = true;
        else if (raw.typ == EvStack) 
            if (len(raw.args) < 2) {
                err = fmt.Errorf("EvStack has wrong number of arguments at offset 0x%x: want at least 2, got %v", raw.off, len(raw.args));
                return ;
            }
            var size = raw.args[1];
            if (size > 1000) {
                err = fmt.Errorf("EvStack has bad number of frames at offset 0x%x: %v", raw.off, size);
                return ;
            }
            nint want = 2 + 4 * size;
            if (ver < 1007) {
                want = 2 + size;
            }
            if (uint64(len(raw.args)) != want) {
                err = fmt.Errorf("EvStack has wrong number of arguments at offset 0x%x: want %v, got %v", raw.off, want, len(raw.args));
                return ;
            }
            var id = raw.args[0];
            if (id != 0 && size > 0) {
                var stk = make_slice<ptr<Frame>>(size);
                {
                    nint i__prev2 = i;

                    for (nint i = 0; i < int(size); i++) {
                        if (ver < 1007) {
                            stk[i] = addr(new Frame(PC:raw.args[2+i]));
                        }
                        else
 {
                            var pc = raw.args[2 + i * 4 + 0];
                            var fn = raw.args[2 + i * 4 + 1];
                            var file = raw.args[2 + i * 4 + 2];
                            var line = raw.args[2 + i * 4 + 3];
                            stk[i] = addr(new Frame(PC:pc,Fn:strings[fn],File:strings[file],Line:int(line)));
                        }

                    }


                    i = i__prev2;
                }
                stacks[id] = stk;

            }

        else 
            ptr<Event> e = addr(new Event(Off:raw.off,Type:raw.typ,P:lastP,G:lastG));
            nint argOffset = default;
            if (ver < 1007) {
                e.seq = lastSeq + int64(raw.args[0]);
                e.Ts = lastTs + int64(raw.args[1]);
                lastSeq = e.seq;
                argOffset = 2;
            }
            else
 {
                e.Ts = lastTs + int64(raw.args[0]);
                argOffset = 1;
            }

            lastTs = e.Ts;
            {
                nint i__prev2 = i;

                for (i = argOffset; i < narg; i++) {
                    if (i == narg - 1 && desc.Stack) {
                        e.StkID = raw.args[i];
                    }
                    else
 {
                        e.Args[i - argOffset] = raw.args[i];
                    }

                }


                i = i__prev2;
            }

            if (raw.typ == EvGoStart || raw.typ == EvGoStartLocal || raw.typ == EvGoStartLabel) 
                lastG = e.Args[0];
                e.G = lastG;
                if (raw.typ == EvGoStartLabel) {
                    e.SArgs = new slice<@string>(new @string[] { strings[e.Args[2]] });
                }
            else if (raw.typ == EvGCSTWStart) 
                e.G = 0;
                switch (e.Args[0]) {
                    case 0: 
                        e.SArgs = new slice<@string>(new @string[] { "mark termination" });
                        break;
                    case 1: 
                        e.SArgs = new slice<@string>(new @string[] { "sweep termination" });
                        break;
                    default: 
                        err = fmt.Errorf("unknown STW kind %d", e.Args[0]);
                        return ;
                        break;
                }
            else if (raw.typ == EvGCStart || raw.typ == EvGCDone || raw.typ == EvGCSTWDone) 
                e.G = 0;
            else if (raw.typ == EvGoEnd || raw.typ == EvGoStop || raw.typ == EvGoSched || raw.typ == EvGoPreempt || raw.typ == EvGoSleep || raw.typ == EvGoBlock || raw.typ == EvGoBlockSend || raw.typ == EvGoBlockRecv || raw.typ == EvGoBlockSelect || raw.typ == EvGoBlockSync || raw.typ == EvGoBlockCond || raw.typ == EvGoBlockNet || raw.typ == EvGoSysBlock || raw.typ == EvGoBlockGC) 
                lastG = 0;
            else if (raw.typ == EvGoSysExit || raw.typ == EvGoWaiting || raw.typ == EvGoInSyscall) 
                e.G = e.Args[0];
            else if (raw.typ == EvUserTaskCreate) 
                // e.Args 0: taskID, 1:parentID, 2:nameID
                e.SArgs = new slice<@string>(new @string[] { strings[e.Args[2]] });
            else if (raw.typ == EvUserRegion) 
                // e.Args 0: taskID, 1: mode, 2:nameID
                e.SArgs = new slice<@string>(new @string[] { strings[e.Args[2]] });
            else if (raw.typ == EvUserLog) 
                // e.Args 0: taskID, 1:keyID, 2: stackID
                e.SArgs = new slice<@string>(new @string[] { strings[e.Args[1]], raw.sargs[0] });
                        batches[lastP] = append(batches[lastP], e);
        
    }    if (len(batches) == 0) {
        err = fmt.Errorf("trace is empty");
        return ;
    }
    if (ticksPerSec == 0) {
        err = fmt.Errorf("no EvFrequency event");
        return ;
    }
    if (BreakTimestampsForTesting) {
        slice<slice<ptr<Event>>> batchArr = default;
        {
            var batch__prev1 = batch;

            foreach (var (_, __batch) in batches) {
                batch = __batch;
                batchArr = append(batchArr, batch);
            }

            batch = batch__prev1;
        }

        {
            nint i__prev1 = i;

            for (i = 0; i < 5; i++) {
                var batch = batchArr[rand.Intn(len(batchArr))];
                batch[rand.Intn(len(batch))].Ts += int64(rand.Intn(2000) - 1000);
            }


            i = i__prev1;
        }

    }
    if (ver < 1007) {
        events, err = order1005(batches);
    }
    else
 {
        events, err = order1007(batches);
    }
    if (err != null) {
        return ;
    }
    var minTs = events[0].Ts; 
    // Use floating point to avoid integer overflows.
    float freq = 1e9F / float64(ticksPerSec);
    foreach (var (_, ev) in events) {
        ev.Ts = int64(float64(ev.Ts - minTs) * freq); 
        // Move timers and syscalls to separate fake Ps.
        if (timerGoids[ev.G] && ev.Type == EvGoUnblock) {
            ev.P = TimerP;
        }
        if (ev.Type == EvGoSysExit) {
            ev.P = SyscallP;
        }
    }    return ;

}

// removeFutile removes all constituents of futile wakeups (block, unblock, start).
// For example, a goroutine was unblocked on a mutex, but another goroutine got
// ahead and acquired the mutex before the first goroutine is scheduled,
// so the first goroutine has to block again. Such wakeups happen on buffered
// channels and sync.Mutex, but are generally not interesting for end user.
private static slice<ptr<Event>> removeFutile(slice<ptr<Event>> events) { 
    // Two non-trivial aspects:
    // 1. A goroutine can be preempted during a futile wakeup and migrate to another P.
    //    We want to remove all of that.
    // 2. Tracing can start in the middle of a futile wakeup.
    //    That is, we can see a futile wakeup event w/o the actual wakeup before it.
    // postProcessTrace runs after us and ensures that we leave the trace in a consistent state.

    // Phase 1: determine futile wakeup sequences.
    public partial struct G {
        public bool futile;
        public slice<ptr<Event>> wakeup; // wakeup sequence (subject for removal)
    }
    var gs = make_map<ulong, G>();
    var futile = make_map<ptr<Event>, bool>();
    {
        var ev__prev1 = ev;

        foreach (var (_, __ev) in events) {
            ev = __ev;

            if (ev.Type == EvGoUnblock) 
                var g = gs[ev.Args[0]];
                g.wakeup = new slice<ptr<Event>>(new ptr<Event>[] { ev });
                gs[ev.Args[0]] = g;
            else if (ev.Type == EvGoStart || ev.Type == EvGoPreempt || ev.Type == EvFutileWakeup) 
                g = gs[ev.G];
                g.wakeup = append(g.wakeup, ev);
                if (ev.Type == EvFutileWakeup) {
                    g.futile = true;
                }
                gs[ev.G] = g;
            else if (ev.Type == EvGoBlock || ev.Type == EvGoBlockSend || ev.Type == EvGoBlockRecv || ev.Type == EvGoBlockSelect || ev.Type == EvGoBlockSync || ev.Type == EvGoBlockCond) 
                g = gs[ev.G];
                if (g.futile) {
                    futile[ev] = true;
                    foreach (var (_, ev1) in g.wakeup) {
                        futile[ev1] = true;
                    }
                }
                delete(gs, ev.G);
            
        }
        ev = ev__prev1;
    }

    var newEvents = events[..(int)0]; // overwrite the original slice
    {
        var ev__prev1 = ev;

        foreach (var (_, __ev) in events) {
            ev = __ev;
            if (!futile[ev]) {
                newEvents = append(newEvents, ev);
            }
        }
        ev = ev__prev1;
    }

    return newEvents;

}

// ErrTimeOrder is returned by Parse when the trace contains
// time stamps that do not respect actual event ordering.
public static var ErrTimeOrder = fmt.Errorf("time stamps out of order");

// postProcessTrace does inter-event verification and information restoration.
// The resulting trace is guaranteed to be consistent
// (for example, a P does not run two Gs at the same time, or a G is indeed
// blocked before an unblock event).
private static error postProcessTrace(nint ver, slice<ptr<Event>> events) {
    const var gDead = iota;
    const var gRunnable = 0;
    const var gRunning = 1;
    const var gWaiting = 2;
    private partial struct gdesc {
        public nint state;
        public ptr<Event> ev;
        public ptr<Event> evStart;
        public ptr<Event> evCreate;
        public ptr<Event> evMarkAssist;
    }
    private partial struct pdesc {
        public bool running;
        public ulong g;
        public ptr<Event> evSTW;
        public ptr<Event> evSweep;
    }

    var gs = make_map<ulong, gdesc>();
    var ps = make_map<nint, pdesc>();
    var tasks = make_map<ulong, ptr<Event>>(); // task id to task creation events
    var activeRegions = make_map<ulong, slice<ptr<Event>>>(); // goroutine id to stack of regions
    gs[0] = new gdesc(state:gRunning);
    ptr<Event> evGC;    ptr<Event> evSTW;



    Func<pdesc, gdesc, ptr<Event>, bool, error> checkRunning = (p, g, ev, allowG0) => {
        var name = EventDescriptions[ev.Type].Name;
        if (g.state != gRunning) {
            return error.As(fmt.Errorf("g %v is not running while %v (offset %v, time %v)", ev.G, name, ev.Off, ev.Ts))!;
        }
        if (p.g != ev.G) {
            return error.As(fmt.Errorf("p %v is not running g %v while %v (offset %v, time %v)", ev.P, ev.G, name, ev.Off, ev.Ts))!;
        }
        if (!allowG0 && ev.G == 0) {
            return error.As(fmt.Errorf("g 0 did %v (offset %v, time %v)", EventDescriptions[ev.Type].Name, ev.Off, ev.Ts))!;
        }
        return error.As(null!)!;

    };

    foreach (var (_, ev) in events) {
        var g = gs[ev.G];
        var p = ps[ev.P];


        if (ev.Type == EvProcStart) 
            if (p.running) {
                return error.As(fmt.Errorf("p %v is running before start (offset %v, time %v)", ev.P, ev.Off, ev.Ts))!;
            }
            p.running = true;
        else if (ev.Type == EvProcStop) 
            if (!p.running) {
                return error.As(fmt.Errorf("p %v is not running before stop (offset %v, time %v)", ev.P, ev.Off, ev.Ts))!;
            }
            if (p.g != 0) {
                return error.As(fmt.Errorf("p %v is running a goroutine %v during stop (offset %v, time %v)", ev.P, p.g, ev.Off, ev.Ts))!;
            }
            p.running = false;
        else if (ev.Type == EvGCStart) 
            if (evGC != null) {
                return error.As(fmt.Errorf("previous GC is not ended before a new one (offset %v, time %v)", ev.Off, ev.Ts))!;
            }
            evGC = ev; 
            // Attribute this to the global GC state.
            ev.P = GCP;
        else if (ev.Type == EvGCDone) 
            if (evGC == null) {
                return error.As(fmt.Errorf("bogus GC end (offset %v, time %v)", ev.Off, ev.Ts))!;
            }
            evGC.Link = ev;
            evGC = null;
        else if (ev.Type == EvGCSTWStart) 
            var evp = _addr_evSTW;
            if (ver < 1010) { 
                // Before 1.10, EvGCSTWStart was per-P.
                evp = _addr_p.evSTW;

            }

            if (evp != null.val) {
                return error.As(fmt.Errorf("previous STW is not ended before a new one (offset %v, time %v)", ev.Off, ev.Ts))!;
            }

            evp.val = ev;
        else if (ev.Type == EvGCSTWDone) 
            evp = _addr_evSTW;
            if (ver < 1010) { 
                // Before 1.10, EvGCSTWDone was per-P.
                evp = _addr_p.evSTW;

            }

            if (evp == null.val) {
                return error.As(fmt.Errorf("bogus STW end (offset %v, time %v)", ev.Off, ev.Ts))!;
            }

            (evp.val).Link = ev;
            evp.val = null;
        else if (ev.Type == EvGCSweepStart) 
            if (p.evSweep != null) {
                return error.As(fmt.Errorf("previous sweeping is not ended before a new one (offset %v, time %v)", ev.Off, ev.Ts))!;
            }
            p.evSweep = ev;
        else if (ev.Type == EvGCMarkAssistStart) 
            if (g.evMarkAssist != null) {
                return error.As(fmt.Errorf("previous mark assist is not ended before a new one (offset %v, time %v)", ev.Off, ev.Ts))!;
            }
            g.evMarkAssist = ev;
        else if (ev.Type == EvGCMarkAssistDone) 
            // Unlike most events, mark assists can be in progress when a
            // goroutine starts tracing, so we can't report an error here.
            if (g.evMarkAssist != null) {
                g.evMarkAssist.Link = ev;
                g.evMarkAssist = null;
            }
        else if (ev.Type == EvGCSweepDone) 
            if (p.evSweep == null) {
                return error.As(fmt.Errorf("bogus sweeping end (offset %v, time %v)", ev.Off, ev.Ts))!;
            }
            p.evSweep.Link = ev;
            p.evSweep = null;
        else if (ev.Type == EvGoWaiting) 
            if (g.state != gRunnable) {
                return error.As(fmt.Errorf("g %v is not runnable before EvGoWaiting (offset %v, time %v)", ev.G, ev.Off, ev.Ts))!;
            }
            g.state = gWaiting;
            g.ev = ev;
        else if (ev.Type == EvGoInSyscall) 
            if (g.state != gRunnable) {
                return error.As(fmt.Errorf("g %v is not runnable before EvGoInSyscall (offset %v, time %v)", ev.G, ev.Off, ev.Ts))!;
            }
            g.state = gWaiting;
            g.ev = ev;
        else if (ev.Type == EvGoCreate) 
            {
                var err__prev1 = err;

                var err = checkRunning(p, g, ev, true);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var (_, ok) = gs[ev.Args[0]];

                if (ok) {
                    return error.As(fmt.Errorf("g %v already exists (offset %v, time %v)", ev.Args[0], ev.Off, ev.Ts))!;
                }

            }

            gs[ev.Args[0]] = new gdesc(state:gRunnable,ev:ev,evCreate:ev);
        else if (ev.Type == EvGoStart || ev.Type == EvGoStartLabel) 
            if (g.state != gRunnable) {
                return error.As(fmt.Errorf("g %v is not runnable before start (offset %v, time %v)", ev.G, ev.Off, ev.Ts))!;
            }
            if (p.g != 0) {
                return error.As(fmt.Errorf("p %v is already running g %v while start g %v (offset %v, time %v)", ev.P, p.g, ev.G, ev.Off, ev.Ts))!;
            }
            g.state = gRunning;
            g.evStart = ev;
            p.g = ev.G;
            if (g.evCreate != null) {
                if (ver < 1007) { 
                    // +1 because symbolizer expects return pc.
                    ev.Stk = new slice<ptr<Frame>>(new ptr<Frame>[] { {PC:g.evCreate.Args[1]+1} });

                }
                else
 {
                    ev.StkID = g.evCreate.Args[1];
                }

                g.evCreate = null;

            }

            if (g.ev != null) {
                g.ev.Link = ev;
                g.ev = null;
            }

        else if (ev.Type == EvGoEnd || ev.Type == EvGoStop) 
            {
                var err__prev1 = err;

                err = checkRunning(p, g, ev, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            g.evStart.Link = ev;
            g.evStart = null;
            g.state = gDead;
            p.g = 0;

            if (ev.Type == EvGoEnd) { // flush all active regions
                var regions = activeRegions[ev.G];
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in regions) {
                        s = __s;
                        s.Link = ev;
                    }

                    s = s__prev2;
                }

                delete(activeRegions, ev.G);

            }

        else if (ev.Type == EvGoSched || ev.Type == EvGoPreempt) 
            {
                var err__prev1 = err;

                err = checkRunning(p, g, ev, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            g.state = gRunnable;
            g.evStart.Link = ev;
            g.evStart = null;
            p.g = 0;
            g.ev = ev;
        else if (ev.Type == EvGoUnblock) 
            if (g.state != gRunning) {
                return error.As(fmt.Errorf("g %v is not running while unpark (offset %v, time %v)", ev.G, ev.Off, ev.Ts))!;
            }
            if (ev.P != TimerP && p.g != ev.G) {
                return error.As(fmt.Errorf("p %v is not running g %v while unpark (offset %v, time %v)", ev.P, ev.G, ev.Off, ev.Ts))!;
            }
            var g1 = gs[ev.Args[0]];
            if (g1.state != gWaiting) {
                return error.As(fmt.Errorf("g %v is not waiting before unpark (offset %v, time %v)", ev.Args[0], ev.Off, ev.Ts))!;
            }
            if (g1.ev != null && g1.ev.Type == EvGoBlockNet && ev.P != TimerP) {
                ev.P = NetpollP;
            }
            if (g1.ev != null) {
                g1.ev.Link = ev;
            }
            g1.state = gRunnable;
            g1.ev = ev;
            gs[ev.Args[0]] = g1;
        else if (ev.Type == EvGoSysCall) 
            {
                var err__prev1 = err;

                err = checkRunning(p, g, ev, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            g.ev = ev;
        else if (ev.Type == EvGoSysBlock) 
            {
                var err__prev1 = err;

                err = checkRunning(p, g, ev, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            g.state = gWaiting;
            g.evStart.Link = ev;
            g.evStart = null;
            p.g = 0;
        else if (ev.Type == EvGoSysExit) 
            if (g.state != gWaiting) {
                return error.As(fmt.Errorf("g %v is not waiting during syscall exit (offset %v, time %v)", ev.G, ev.Off, ev.Ts))!;
            }
            if (g.ev != null && g.ev.Type == EvGoSysCall) {
                g.ev.Link = ev;
            }
            g.state = gRunnable;
            g.ev = ev;
        else if (ev.Type == EvGoSleep || ev.Type == EvGoBlock || ev.Type == EvGoBlockSend || ev.Type == EvGoBlockRecv || ev.Type == EvGoBlockSelect || ev.Type == EvGoBlockSync || ev.Type == EvGoBlockCond || ev.Type == EvGoBlockNet || ev.Type == EvGoBlockGC) 
            {
                var err__prev1 = err;

                err = checkRunning(p, g, ev, false);

                if (err != null) {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            g.state = gWaiting;
            g.ev = ev;
            g.evStart.Link = ev;
            g.evStart = null;
            p.g = 0;
        else if (ev.Type == EvUserTaskCreate) 
            var taskid = ev.Args[0];
            {
                var (prevEv, ok) = tasks[taskid];

                if (ok) {
                    return error.As(fmt.Errorf("task id conflicts (id:%d), %q vs %q", taskid, ev, prevEv))!;
                }

            }

            tasks[ev.Args[0]] = ev;
        else if (ev.Type == EvUserTaskEnd) 
            taskid = ev.Args[0];
            {
                var (taskCreateEv, ok) = tasks[taskid];

                if (ok) {
                    taskCreateEv.Link = ev;
                    delete(tasks, taskid);
                }

            }

        else if (ev.Type == EvUserRegion) 
            var mode = ev.Args[1];
            regions = activeRegions[ev.G];
            if (mode == 0) { // region start
                activeRegions[ev.G] = append(regions, ev); // push
            }
            else if (mode == 1) { // region end
                var n = len(regions);
                if (n > 0) { // matching region start event is in the trace.
                    var s = regions[n - 1];
                    if (s.Args[0] != ev.Args[0] || s.SArgs[0] != ev.SArgs[0]) { // task id, region name mismatch
                        return error.As(fmt.Errorf("misuse of region in goroutine %d: span end %q when the inner-most active span start event is %q", ev.G, ev, s))!;

                    } 
                    // Link region start event with span end event
                    s.Link = ev;

                    if (n > 1) {
                        activeRegions[ev.G] = regions[..(int)n - 1];
                    }
                    else
 {
                        delete(activeRegions, ev.G);
                    }

                }

            }
            else
 {
                return error.As(fmt.Errorf("invalid user region mode: %q", ev))!;
            }

                gs[ev.G] = g;
        ps[ev.P] = p;

    }    return error.As(null!)!;

}

// symbolize attaches func/file/line info to stack traces.
private static error symbolize(slice<ptr<Event>> events, @string bin) { 
    // First, collect and dedup all pcs.
    var pcs = make_map<ulong, ptr<Frame>>();
    {
        var ev__prev1 = ev;

        foreach (var (_, __ev) in events) {
            ev = __ev;
            {
                var f__prev2 = f;

                foreach (var (_, __f) in ev.Stk) {
                    f = __f;
                    pcs[f.PC] = null;
                }

                f = f__prev2;
            }
        }
        ev = ev__prev1;
    }

    var cmd = exec.Command(goCmd(), "tool", "addr2line", bin);
    var (in, err) = cmd.StdinPipe();
    if (err != null) {
        return error.As(fmt.Errorf("failed to pipe addr2line stdin: %v", err))!;
    }
    cmd.Stderr = os.Stderr;
    var (out, err) = cmd.StdoutPipe();
    if (err != null) {
        return error.As(fmt.Errorf("failed to pipe addr2line stdout: %v", err))!;
    }
    err = cmd.Start();
    if (err != null) {
        return error.As(fmt.Errorf("failed to start addr2line: %v", err))!;
    }
    var outb = bufio.NewReader(out); 

    // Write all pcs to addr2line.
    // Need to copy pcs to an array, because map iteration order is non-deterministic.
    slice<ulong> pcArray = default;
    {
        var pc__prev1 = pc;

        foreach (var (__pc) in pcs) {
            pc = __pc;
            pcArray = append(pcArray, pc);
            var (_, err) = fmt.Fprintf(in, "0x%x\n", pc - 1);
            if (err != null) {
                return error.As(fmt.Errorf("failed to write to addr2line: %v", err))!;
            }
        }
        pc = pc__prev1;
    }

    @in.Close(); 

    // Read in answers.
    {
        var pc__prev1 = pc;

        foreach (var (_, __pc) in pcArray) {
            pc = __pc;
            var (fn, err) = outb.ReadString('\n');
            if (err != null) {
                return error.As(fmt.Errorf("failed to read from addr2line: %v", err))!;
            }
            var (file, err) = outb.ReadString('\n');
            if (err != null) {
                return error.As(fmt.Errorf("failed to read from addr2line: %v", err))!;
            }
            ptr<Frame> f = addr(new Frame(PC:pc));
            f.Fn = fn[..(int)len(fn) - 1];
            f.File = file[..(int)len(file) - 1];
            {
                var colon = strings.LastIndex(f.File, ":");

                if (colon != -1) {
                    var (ln, err) = strconv.Atoi(f.File[(int)colon + 1..]);
                    if (err == null) {
                        f.File = f.File[..(int)colon];
                        f.Line = ln;
                    }
                }

            }

            pcs[pc] = f;

        }
        pc = pc__prev1;
    }

    cmd.Wait(); 

    // Replace frames in events array.
    {
        var ev__prev1 = ev;

        foreach (var (_, __ev) in events) {
            ev = __ev;
            {
                var f__prev2 = f;

                foreach (var (__i, __f) in ev.Stk) {
                    i = __i;
                    f = __f;
                    ev.Stk[i] = pcs[f.PC];
                }

                f = f__prev2;
            }
        }
        ev = ev__prev1;
    }

    return error.As(null!)!;

}

// readVal reads unsigned base-128 value from r.
private static (ulong, nint, error) readVal(io.Reader r, nint off0) {
    ulong v = default;
    nint off = default;
    error err = default!;

    off = off0;
    for (nint i = 0; i < 10; i++) {
        array<byte> buf = new array<byte>(1);
        nint n = default;
        n, err = r.Read(buf[..]);
        if (err != null || n != 1) {
            return (0, 0, error.As(fmt.Errorf("failed to read trace at offset %d: read %v, error %v", off0, n, err))!);
        }
        off++;
        v |= uint64(buf[0] & 0x7f) << (int)((uint(i) * 7));
        if (buf[0] & 0x80 == 0) {
            return ;
        }
    }
    return (0, 0, error.As(fmt.Errorf("bad value at offset 0x%x", off0))!);

}

// Print dumps events to stdout. For debugging.
public static void Print(slice<ptr<Event>> events) {
    foreach (var (_, ev) in events) {
        PrintEvent(_addr_ev);
    }
}

// PrintEvent dumps the event to stdout. For debugging.
public static void PrintEvent(ptr<Event> _addr_ev) {
    ref Event ev = ref _addr_ev.val;

    fmt.Printf("%s\n", ev);
}

private static @string String(this ptr<Event> _addr_ev) {
    ref Event ev = ref _addr_ev.val;

    var desc = EventDescriptions[ev.Type];
    ptr<object> w = @new<bytes.Buffer>();
    fmt.Fprintf(w, "%v %v p=%v g=%v off=%v", ev.Ts, desc.Name, ev.P, ev.G, ev.Off);
    {
        var i__prev1 = i;
        var a__prev1 = a;

        foreach (var (__i, __a) in desc.Args) {
            i = __i;
            a = __a;
            fmt.Fprintf(w, " %v=%v", a, ev.Args[i]);
        }
        i = i__prev1;
        a = a__prev1;
    }

    {
        var i__prev1 = i;
        var a__prev1 = a;

        foreach (var (__i, __a) in desc.SArgs) {
            i = __i;
            a = __a;
            fmt.Fprintf(w, " %v=%v", a, ev.SArgs[i]);
        }
        i = i__prev1;
        a = a__prev1;
    }

    return w.String();

}

// argNum returns total number of args for the event accounting for timestamps,
// sequence numbers and differences between trace format versions.
private static nint argNum(rawEvent raw, nint ver) {
    var desc = EventDescriptions[raw.typ];
    if (raw.typ == EvStack) {
        return len(raw.args);
    }
    var narg = len(desc.Args);
    if (desc.Stack) {
        narg++;
    }

    if (raw.typ == EvBatch || raw.typ == EvFrequency || raw.typ == EvTimerGoroutine) 
        if (ver < 1007) {
            narg++; // there was an unused arg before 1.7
        }
        return narg;
        narg++; // timestamp
    if (ver < 1007) {
        narg++; // sequence
    }

    if (raw.typ == EvGCSweepDone) 
        if (ver < 1009) {
            narg -= 2; // 1.9 added two arguments
        }
    else if (raw.typ == EvGCStart || raw.typ == EvGoStart || raw.typ == EvGoUnblock) 
        if (ver < 1007) {
            narg--; // 1.7 added an additional seq arg
        }
    else if (raw.typ == EvGCSTWStart) 
        if (ver < 1010) {
            narg--; // 1.10 added an argument
        }
        return narg;

}

// BreakTimestampsForTesting causes the parser to randomly alter timestamps (for testing of broken cputicks).
public static bool BreakTimestampsForTesting = default;

// Event types in the trace.
// Verbatim copy from src/runtime/trace.go with the "trace" prefix removed.
public static readonly nint EvNone = 0; // unused
public static readonly nint EvBatch = 1; // start of per-P batch of events [pid, timestamp]
public static readonly nint EvFrequency = 2; // contains tracer timer frequency [frequency (ticks per second)]
public static readonly nint EvStack = 3; // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]
public static readonly nint EvGomaxprocs = 4; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]
public static readonly nint EvProcStart = 5; // start of P [timestamp, thread id]
public static readonly nint EvProcStop = 6; // stop of P [timestamp]
public static readonly nint EvGCStart = 7; // GC start [timestamp, seq, stack id]
public static readonly nint EvGCDone = 8; // GC done [timestamp]
public static readonly nint EvGCSTWStart = 9; // GC mark termination start [timestamp, kind]
public static readonly nint EvGCSTWDone = 10; // GC mark termination done [timestamp]
public static readonly nint EvGCSweepStart = 11; // GC sweep start [timestamp, stack id]
public static readonly nint EvGCSweepDone = 12; // GC sweep done [timestamp, swept, reclaimed]
public static readonly nint EvGoCreate = 13; // goroutine creation [timestamp, new goroutine id, new stack id, stack id]
public static readonly nint EvGoStart = 14; // goroutine starts running [timestamp, goroutine id, seq]
public static readonly nint EvGoEnd = 15; // goroutine ends [timestamp]
public static readonly nint EvGoStop = 16; // goroutine stops (like in select{}) [timestamp, stack]
public static readonly nint EvGoSched = 17; // goroutine calls Gosched [timestamp, stack]
public static readonly nint EvGoPreempt = 18; // goroutine is preempted [timestamp, stack]
public static readonly nint EvGoSleep = 19; // goroutine calls Sleep [timestamp, stack]
public static readonly nint EvGoBlock = 20; // goroutine blocks [timestamp, stack]
public static readonly nint EvGoUnblock = 21; // goroutine is unblocked [timestamp, goroutine id, seq, stack]
public static readonly nint EvGoBlockSend = 22; // goroutine blocks on chan send [timestamp, stack]
public static readonly nint EvGoBlockRecv = 23; // goroutine blocks on chan recv [timestamp, stack]
public static readonly nint EvGoBlockSelect = 24; // goroutine blocks on select [timestamp, stack]
public static readonly nint EvGoBlockSync = 25; // goroutine blocks on Mutex/RWMutex [timestamp, stack]
public static readonly nint EvGoBlockCond = 26; // goroutine blocks on Cond [timestamp, stack]
public static readonly nint EvGoBlockNet = 27; // goroutine blocks on network [timestamp, stack]
public static readonly nint EvGoSysCall = 28; // syscall enter [timestamp, stack]
public static readonly nint EvGoSysExit = 29; // syscall exit [timestamp, goroutine id, seq, real timestamp]
public static readonly nint EvGoSysBlock = 30; // syscall blocks [timestamp]
public static readonly nint EvGoWaiting = 31; // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]
public static readonly nint EvGoInSyscall = 32; // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]
public static readonly nint EvHeapAlloc = 33; // gcController.heapLive change [timestamp, heap live bytes]
public static readonly nint EvHeapGoal = 34; // gcController.heapGoal change [timestamp, heap goal bytes]
public static readonly nint EvTimerGoroutine = 35; // denotes timer goroutine [timer goroutine id]
public static readonly nint EvFutileWakeup = 36; // denotes that the previous wakeup of this goroutine was futile [timestamp]
public static readonly nint EvString = 37; // string dictionary entry [ID, length, string]
public static readonly nint EvGoStartLocal = 38; // goroutine starts running on the same P as the last event [timestamp, goroutine id]
public static readonly nint EvGoUnblockLocal = 39; // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]
public static readonly nint EvGoSysExitLocal = 40; // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]
public static readonly nint EvGoStartLabel = 41; // goroutine starts running with label [timestamp, goroutine id, seq, label string id]
public static readonly nint EvGoBlockGC = 42; // goroutine blocks on GC assist [timestamp, stack]
public static readonly nint EvGCMarkAssistStart = 43; // GC mark assist start [timestamp, stack]
public static readonly nint EvGCMarkAssistDone = 44; // GC mark assist done [timestamp]
public static readonly nint EvUserTaskCreate = 45; // trace.NewContext [timestamp, internal task id, internal parent id, stack, name string]
public static readonly nint EvUserTaskEnd = 46; // end of task [timestamp, internal task id, stack]
public static readonly nint EvUserRegion = 47; // trace.WithRegion [timestamp, internal task id, mode(0:start, 1:end), stack, name string]
public static readonly nint EvUserLog = 48; // trace.Log [timestamp, internal id, key string id, stack, value string]
public static readonly nint EvCount = 49;




} // end trace_package
