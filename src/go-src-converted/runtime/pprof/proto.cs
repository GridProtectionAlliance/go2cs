// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 13 05:28:51 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\proto.go
namespace go.runtime;

using bytes = bytes_package;
using gzip = compress.gzip_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using runtime = runtime_package;
using strconv = strconv_package;
using time = time_package;
using @unsafe = @unsafe_package;


// lostProfileEvent is the function to which lost profiling
// events are attributed.
// (The name shows up in the pprof graphs.)

using System;
public static partial class pprof_package {

private static void lostProfileEvent() {
    lostProfileEvent();
}

// funcPC returns the PC for the func value f.
private static System.UIntPtr funcPC(object f) {
    return new ptr<ptr<ptr<array<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_f))[1];
}

// A profileBuilder writes a profile incrementally from a
// stream of profile samples delivered by the runtime.
private partial struct profileBuilder {
    public time.Time start;
    public time.Time end;
    public bool havePeriod;
    public long period;
    public profMap m; // encoding state
    public io.Writer w;
    public ptr<gzip.Writer> zw;
    public protobuf pb;
    public slice<@string> strings;
    public map<@string, nint> stringMap;
    public map<System.UIntPtr, locInfo> locs; // list of locInfo starting with the given PC.
    public map<@string, nint> funcs; // Package path-qualified function name to Function.ID
    public slice<memMap> mem;
    public pcDeck deck;
}

private partial struct memMap {
    public System.UIntPtr start;
    public System.UIntPtr end;
    public ulong offset;
    public @string file;
    public @string buildID;
    public symbolizeFlag funcs;
    public bool fake; // map entry was faked; /proc/self/maps wasn't available
}

// symbolizeFlag keeps track of symbolization result.
//   0                  : no symbol lookup was performed
//   1<<0 (lookupTried) : symbol lookup was performed
//   1<<1 (lookupFailed): symbol lookup was performed but failed
private partial struct symbolizeFlag { // : byte
}

private static readonly symbolizeFlag lookupTried = 1 << (int)(iota);
private static readonly symbolizeFlag lookupFailed = 1 << (int)(iota);

 
// message Profile
private static readonly nint tagProfile_SampleType = 1; // repeated ValueType
private static readonly nint tagProfile_Sample = 2; // repeated Sample
private static readonly nint tagProfile_Mapping = 3; // repeated Mapping
private static readonly nint tagProfile_Location = 4; // repeated Location
private static readonly nint tagProfile_Function = 5; // repeated Function
private static readonly nint tagProfile_StringTable = 6; // repeated string
private static readonly nint tagProfile_DropFrames = 7; // int64 (string table index)
private static readonly nint tagProfile_KeepFrames = 8; // int64 (string table index)
private static readonly nint tagProfile_TimeNanos = 9; // int64
private static readonly nint tagProfile_DurationNanos = 10; // int64
private static readonly nint tagProfile_PeriodType = 11; // ValueType (really optional string???)
private static readonly nint tagProfile_Period = 12; // int64
private static readonly nint tagProfile_Comment = 13; // repeated int64
private static readonly nint tagProfile_DefaultSampleType = 14; // int64

// message ValueType
private static readonly nint tagValueType_Type = 1; // int64 (string table index)
private static readonly nint tagValueType_Unit = 2; // int64 (string table index)

// message Sample
private static readonly nint tagSample_Location = 1; // repeated uint64
private static readonly nint tagSample_Value = 2; // repeated int64
private static readonly nint tagSample_Label = 3; // repeated Label

// message Label
private static readonly nint tagLabel_Key = 1; // int64 (string table index)
private static readonly nint tagLabel_Str = 2; // int64 (string table index)
private static readonly nint tagLabel_Num = 3; // int64

// message Mapping
private static readonly nint tagMapping_ID = 1; // uint64
private static readonly nint tagMapping_Start = 2; // uint64
private static readonly nint tagMapping_Limit = 3; // uint64
private static readonly nint tagMapping_Offset = 4; // uint64
private static readonly nint tagMapping_Filename = 5; // int64 (string table index)
private static readonly nint tagMapping_BuildID = 6; // int64 (string table index)
private static readonly nint tagMapping_HasFunctions = 7; // bool
private static readonly nint tagMapping_HasFilenames = 8; // bool
private static readonly nint tagMapping_HasLineNumbers = 9; // bool
private static readonly nint tagMapping_HasInlineFrames = 10; // bool

// message Location
private static readonly nint tagLocation_ID = 1; // uint64
private static readonly nint tagLocation_MappingID = 2; // uint64
private static readonly nint tagLocation_Address = 3; // uint64
private static readonly nint tagLocation_Line = 4; // repeated Line

// message Line
private static readonly nint tagLine_FunctionID = 1; // uint64
private static readonly nint tagLine_Line = 2; // int64

// message Function
private static readonly nint tagFunction_ID = 1; // uint64
private static readonly nint tagFunction_Name = 2; // int64 (string table index)
private static readonly nint tagFunction_SystemName = 3; // int64 (string table index)
private static readonly nint tagFunction_Filename = 4; // int64 (string table index)
private static readonly nint tagFunction_StartLine = 5; // int64

// stringIndex adds s to the string table if not already present
// and returns the index of s in the string table.
private static long stringIndex(this ptr<profileBuilder> _addr_b, @string s) {
    ref profileBuilder b = ref _addr_b.val;

    var (id, ok) = b.stringMap[s];
    if (!ok) {
        id = len(b.strings);
        b.strings = append(b.strings, s);
        b.stringMap[s] = id;
    }
    return int64(id);
}

private static void flush(this ptr<profileBuilder> _addr_b) {
    ref profileBuilder b = ref _addr_b.val;

    const nint dataFlush = 4096;

    if (b.pb.nest == 0 && len(b.pb.data) > dataFlush) {
        b.zw.Write(b.pb.data);
        b.pb.data = b.pb.data[..(int)0];
    }
}

// pbValueType encodes a ValueType message to b.pb.
private static void pbValueType(this ptr<profileBuilder> _addr_b, nint tag, @string typ, @string unit) {
    ref profileBuilder b = ref _addr_b.val;

    var start = b.pb.startMessage();
    b.pb.int64(tagValueType_Type, b.stringIndex(typ));
    b.pb.int64(tagValueType_Unit, b.stringIndex(unit));
    b.pb.endMessage(tag, start);
}

// pbSample encodes a Sample message to b.pb.
private static void pbSample(this ptr<profileBuilder> _addr_b, slice<long> values, slice<ulong> locs, Action labels) {
    ref profileBuilder b = ref _addr_b.val;

    var start = b.pb.startMessage();
    b.pb.int64s(tagSample_Value, values);
    b.pb.uint64s(tagSample_Location, locs);
    if (labels != null) {
        labels();
    }
    b.pb.endMessage(tagProfile_Sample, start);
    b.flush();
}

// pbLabel encodes a Label message to b.pb.
private static void pbLabel(this ptr<profileBuilder> _addr_b, nint tag, @string key, @string str, long num) {
    ref profileBuilder b = ref _addr_b.val;

    var start = b.pb.startMessage();
    b.pb.int64Opt(tagLabel_Key, b.stringIndex(key));
    b.pb.int64Opt(tagLabel_Str, b.stringIndex(str));
    b.pb.int64Opt(tagLabel_Num, num);
    b.pb.endMessage(tag, start);
}

// pbLine encodes a Line message to b.pb.
private static void pbLine(this ptr<profileBuilder> _addr_b, nint tag, ulong funcID, long line) {
    ref profileBuilder b = ref _addr_b.val;

    var start = b.pb.startMessage();
    b.pb.uint64Opt(tagLine_FunctionID, funcID);
    b.pb.int64Opt(tagLine_Line, line);
    b.pb.endMessage(tag, start);
}

// pbMapping encodes a Mapping message to b.pb.
private static void pbMapping(this ptr<profileBuilder> _addr_b, nint tag, ulong id, ulong @base, ulong limit, ulong offset, @string file, @string buildID, bool hasFuncs) {
    ref profileBuilder b = ref _addr_b.val;

    var start = b.pb.startMessage();
    b.pb.uint64Opt(tagMapping_ID, id);
    b.pb.uint64Opt(tagMapping_Start, base);
    b.pb.uint64Opt(tagMapping_Limit, limit);
    b.pb.uint64Opt(tagMapping_Offset, offset);
    b.pb.int64Opt(tagMapping_Filename, b.stringIndex(file));
    b.pb.int64Opt(tagMapping_BuildID, b.stringIndex(buildID)); 
    // TODO: we set HasFunctions if all symbols from samples were symbolized (hasFuncs).
    // Decide what to do about HasInlineFrames and HasLineNumbers.
    // Also, another approach to handle the mapping entry with
    // incomplete symbolization results is to dupliace the mapping
    // entry (but with different Has* fields values) and use
    // different entries for symbolized locations and unsymbolized locations.
    if (hasFuncs) {
        b.pb.@bool(tagMapping_HasFunctions, true);
    }
    b.pb.endMessage(tag, start);
}

private static (slice<runtime.Frame>, symbolizeFlag) allFrames(System.UIntPtr addr) {
    slice<runtime.Frame> _p0 = default;
    symbolizeFlag _p0 = default;
 
    // Expand this one address using CallersFrames so we can cache
    // each expansion. In general, CallersFrames takes a whole
    // stack, but in this case we know there will be no skips in
    // the stack and we have return PCs anyway.
    var frames = runtime.CallersFrames(new slice<System.UIntPtr>(new System.UIntPtr[] { addr }));
    var (frame, more) = frames.Next();
    if (frame.Function == "runtime.goexit") { 
        // Short-circuit if we see runtime.goexit so the loop
        // below doesn't allocate a useless empty location.
        return (null, 0);
    }
    var symbolizeResult = lookupTried;
    if (frame.PC == 0 || frame.Function == "" || frame.File == "" || frame.Line == 0) {
        symbolizeResult |= lookupFailed;
    }
    if (frame.PC == 0) { 
        // If we failed to resolve the frame, at least make up
        // a reasonable call PC. This mostly happens in tests.
        frame.PC = addr - 1;
    }
    runtime.Frame ret = new slice<runtime.Frame>(new runtime.Frame[] { frame });
    while (frame.Function != "runtime.goexit" && more == true) {
        frame, more = frames.Next();
        ret = append(ret, frame);
    }
    return (ret, symbolizeResult);
}

private partial struct locInfo {
    public ulong id; // sequence of PCs, including the fake PCs returned by the traceback
// to represent inlined functions
// https://github.com/golang/go/blob/d6f2f833c93a41ec1c68e49804b8387a06b131c5/src/runtime/traceback.go#L347-L368
    public slice<System.UIntPtr> pcs;
}

// newProfileBuilder returns a new profileBuilder.
// CPU profiling data obtained from the runtime can be added
// by calling b.addCPUData, and then the eventual profile
// can be obtained by calling b.finish.
private static ptr<profileBuilder> newProfileBuilder(io.Writer w) {
    var (zw, _) = gzip.NewWriterLevel(w, gzip.BestSpeed);
    ptr<profileBuilder> b = addr(new profileBuilder(w:w,zw:zw,start:time.Now(),strings:[]string{""},stringMap:map[string]int{"":0},locs:map[uintptr]locInfo{},funcs:map[string]int{},));
    b.readMapping();
    return _addr_b!;
}

// addCPUData adds the CPU profiling data to the profile.
// The data must be a whole number of records,
// as delivered by the runtime.
private static error addCPUData(this ptr<profileBuilder> _addr_b, slice<ulong> data, slice<unsafe.Pointer> tags) {
    ref profileBuilder b = ref _addr_b.val;

    if (!b.havePeriod) { 
        // first record is period
        if (len(data) < 3) {
            return error.As(fmt.Errorf("truncated profile"))!;
        }
        if (data[0] != 3 || data[2] == 0) {
            return error.As(fmt.Errorf("malformed profile"))!;
        }
        b.period = 1e9F / int64(data[2]);
        b.havePeriod = true;
        data = data[(int)3..];
    }
    while (len(data) > 0) {
        if (len(data) < 3 || data[0] > uint64(len(data))) {
            return error.As(fmt.Errorf("truncated profile"))!;
        }
        if (data[0] < 3 || tags != null && len(tags) < 1) {
            return error.As(fmt.Errorf("malformed profile"))!;
        }
        var count = data[2];
        var stk = data[(int)3..(int)data[0]];
        data = data[(int)data[0]..];
        unsafe.Pointer tag = default;
        if (tags != null) {
            tag = tags[0];
            tags = tags[(int)1..];
        }
        if (count == 0 && len(stk) == 1) { 
            // overflow record
            count = uint64(stk[0]);
            stk = new slice<ulong>(new ulong[] { uint64(funcPC(lostProfileEvent)+1) });
        }
        b.m.lookup(stk, tag).count += int64(count);
    }
    return error.As(null!)!;
}

// build completes and returns the constructed profile.
private static void build(this ptr<profileBuilder> _addr_b) {
    ref profileBuilder b = ref _addr_b.val;

    b.end = time.Now();

    b.pb.int64Opt(tagProfile_TimeNanos, b.start.UnixNano());
    if (b.havePeriod) { // must be CPU profile
        b.pbValueType(tagProfile_SampleType, "samples", "count");
        b.pbValueType(tagProfile_SampleType, "cpu", "nanoseconds");
        b.pb.int64Opt(tagProfile_DurationNanos, b.end.Sub(b.start).Nanoseconds());
        b.pbValueType(tagProfile_PeriodType, "cpu", "nanoseconds");
        b.pb.int64Opt(tagProfile_Period, b.period);
    }
    long values = new slice<long>(new long[] { 0, 0 });
    slice<ulong> locs = default;

    {
        var e = b.m.all;

        while (e != null) {
            values[0] = e.count;
            values[1] = e.count * b.period;

            Action labels = default;
            if (e.tag != null) {
                labels = () => {
                    foreach (var (k, v) in new ptr<ptr<ptr<labelMap>>>(e.tag)) {
                        b.pbLabel(tagSample_Label, k, v, 0);
                    }
            e = e.nextAll;
                }
;
            }
            locs = b.appendLocsForStack(locs[..(int)0], e.stk);

            b.pbSample(values, locs, labels);
        }
    }

    foreach (var (i, m) in b.mem) {
        var hasFunctions = m.funcs == lookupTried; // lookupTried but not lookupFailed
        b.pbMapping(tagProfile_Mapping, uint64(i + 1), uint64(m.start), uint64(m.end), m.offset, m.file, m.buildID, hasFunctions);
    }    b.pb.strings(tagProfile_StringTable, b.strings);
    b.zw.Write(b.pb.data);
    b.zw.Close();
}

// appendLocsForStack appends the location IDs for the given stack trace to the given
// location ID slice, locs. The addresses in the stack are return PCs or 1 + the PC of
// an inline marker as the runtime traceback function returns.
//
// It may emit to b.pb, so there must be no message encoding in progress.
private static slice<ulong> appendLocsForStack(this ptr<profileBuilder> _addr_b, slice<ulong> locs, slice<System.UIntPtr> stk) {
    slice<ulong> newLocs = default;
    ref profileBuilder b = ref _addr_b.val;

    b.deck.reset(); 

    // The last frame might be truncated. Recover lost inline frames.
    stk = runtime_expandFinalInlineFrame(stk);

    while (len(stk) > 0) {
        var addr = stk[0];
        {
            var l__prev1 = l;

            var (l, ok) = b.locs[addr];

            if (ok) { 
                // first record the location if there is any pending accumulated info.
                {
                    var id__prev2 = id;

                    var id = b.emitLocation();

                    if (id > 0) {
                        locs = append(locs, id);
                    } 

                    // then, record the cached location.

                    id = id__prev2;

                } 

                // then, record the cached location.
                locs = append(locs, l.id); 

                // Skip the matching pcs.
                //
                // Even if stk was truncated due to the stack depth
                // limit, expandFinalInlineFrame above has already
                // fixed the truncation, ensuring it is long enough.
                stk = stk[(int)len(l.pcs)..];
                continue;
            }

            l = l__prev1;

        }

        var (frames, symbolizeResult) = allFrames(addr);
        if (len(frames) == 0) { // runtime.goexit.
            {
                var id__prev2 = id;

                id = b.emitLocation();

                if (id > 0) {
                    locs = append(locs, id);
                }

                id = id__prev2;

            }
            stk = stk[(int)1..];
            continue;
        }
        {
            var added = b.deck.tryAdd(addr, frames, symbolizeResult);

            if (added) {
                stk = stk[(int)1..];
                continue;
            } 
            // add failed because this addr is not inlined with the
            // existing PCs in the deck. Flush the deck and retry handling
            // this pc.

        } 
        // add failed because this addr is not inlined with the
        // existing PCs in the deck. Flush the deck and retry handling
        // this pc.
        {
            var id__prev1 = id;

            id = b.emitLocation();

            if (id > 0) {
                locs = append(locs, id);
            } 

            // check cache again - previous emitLocation added a new entry

            id = id__prev1;

        } 

        // check cache again - previous emitLocation added a new entry
        {
            var l__prev1 = l;

            (l, ok) = b.locs[addr];

            if (ok) {
                locs = append(locs, l.id);
                stk = stk[(int)len(l.pcs)..]; // skip the matching pcs.
            }
            else
 {
                b.deck.tryAdd(addr, frames, symbolizeResult); // must succeed.
                stk = stk[(int)1..];
            }

            l = l__prev1;

        }
    }
    {
        var id__prev1 = id;

        id = b.emitLocation();

        if (id > 0) { // emit remaining location.
            locs = append(locs, id);
        }
        id = id__prev1;

    }
    return locs;
}

// pcDeck is a helper to detect a sequence of inlined functions from
// a stack trace returned by the runtime.
//
// The stack traces returned by runtime's trackback functions are fully
// expanded (at least for Go functions) and include the fake pcs representing
// inlined functions. The profile proto expects the inlined functions to be
// encoded in one Location message.
// https://github.com/google/pprof/blob/5e965273ee43930341d897407202dd5e10e952cb/proto/profile.proto#L177-L184
//
// Runtime does not directly expose whether a frame is for an inlined function
// and looking up debug info is not ideal, so we use a heuristic to filter
// the fake pcs and restore the inlined and entry functions. Inlined functions
// have the following properties:
//   Frame's Func is nil (note: also true for non-Go functions), and
//   Frame's Entry matches its entry function frame's Entry (note: could also be true for recursive calls and non-Go functions), and
//   Frame's Name does not match its entry function frame's name (note: inlined functions cannot be directly recursive).
//
// As reading and processing the pcs in a stack trace one by one (from leaf to the root),
// we use pcDeck to temporarily hold the observed pcs and their expanded frames
// until we observe the entry function frame.
private partial struct pcDeck {
    public slice<System.UIntPtr> pcs;
    public slice<runtime.Frame> frames;
    public symbolizeFlag symbolizeResult;
}

private static void reset(this ptr<pcDeck> _addr_d) {
    ref pcDeck d = ref _addr_d.val;

    d.pcs = d.pcs[..(int)0];
    d.frames = d.frames[..(int)0];
    d.symbolizeResult = 0;
}

// tryAdd tries to add the pc and Frames expanded from it (most likely one,
// since the stack trace is already fully expanded) and the symbolizeResult
// to the deck. If it fails the caller needs to flush the deck and retry.
private static bool tryAdd(this ptr<pcDeck> _addr_d, System.UIntPtr pc, slice<runtime.Frame> frames, symbolizeFlag symbolizeResult) {
    bool success = default;
    ref pcDeck d = ref _addr_d.val;

    {
        var existing = len(d.pcs);

        if (existing > 0) { 
            // 'd.frames' are all expanded from one 'pc' and represent all
            // inlined functions so we check only the last one.
            var newFrame = frames[0];
            var last = d.frames[existing - 1];
            if (last.Func != null) { // the last frame can't be inlined. Flush.
                return false;
            }
            if (last.Entry == 0 || newFrame.Entry == 0) { // Possibly not a Go function. Don't try to merge.
                return false;
            }
            if (last.Entry != newFrame.Entry) { // newFrame is for a different function.
                return false;
            }
            if (last.Function == newFrame.Function) { // maybe recursion.
                return false;
            }
        }
    }
    d.pcs = append(d.pcs, pc);
    d.frames = append(d.frames, frames);
    d.symbolizeResult |= symbolizeResult;
    return true;
}

// emitLocation emits the new location and function information recorded in the deck
// and returns the location ID encoded in the profile protobuf.
// It emits to b.pb, so there must be no message encoding in progress.
// It resets the deck.
private static ulong emitLocation(this ptr<profileBuilder> _addr_b) => func((defer, _, _) => {
    ref profileBuilder b = ref _addr_b.val;

    if (len(b.deck.pcs) == 0) {
        return 0;
    }
    defer(b.deck.reset());

    var addr = b.deck.pcs[0];
    var firstFrame = b.deck.frames[0]; 

    // We can't write out functions while in the middle of the
    // Location message, so record new functions we encounter and
    // write them out after the Location.
    private partial struct newFunc {
        public ulong id;
        public @string name;
        public @string file;
    }
    var newFuncs = make_slice<newFunc>(0, 8);

    var id = uint64(len(b.locs)) + 1;
    b.locs[addr] = new locInfo(id:id,pcs:append([]uintptr{},b.deck.pcs...));

    var start = b.pb.startMessage();
    b.pb.uint64Opt(tagLocation_ID, id);
    b.pb.uint64Opt(tagLocation_Address, uint64(firstFrame.PC));
    foreach (var (_, frame) in b.deck.frames) { 
        // Write out each line in frame expansion.
        var funcID = uint64(b.funcs[frame.Function]);
        if (funcID == 0) {
            funcID = uint64(len(b.funcs)) + 1;
            b.funcs[frame.Function] = int(funcID);
            newFuncs = append(newFuncs, new newFunc(funcID,frame.Function,frame.File));
        }
        b.pbLine(tagLocation_Line, funcID, int64(frame.Line));
    }    foreach (var (i) in b.mem) {
        if (b.mem[i].start <= addr && addr < b.mem[i].end || b.mem[i].fake) {
            b.pb.uint64Opt(tagLocation_MappingID, uint64(i + 1));

            var m = b.mem[i];
            m.funcs |= b.deck.symbolizeResult;
            b.mem[i] = m;
            break;
        }
    }    b.pb.endMessage(tagProfile_Location, start); 

    // Write out functions we found during frame expansion.
    foreach (var (_, fn) in newFuncs) {
        start = b.pb.startMessage();
        b.pb.uint64Opt(tagFunction_ID, fn.id);
        b.pb.int64Opt(tagFunction_Name, b.stringIndex(fn.name));
        b.pb.int64Opt(tagFunction_SystemName, b.stringIndex(fn.name));
        b.pb.int64Opt(tagFunction_Filename, b.stringIndex(fn.file));
        b.pb.endMessage(tagProfile_Function, start);
    }    b.flush();
    return id;
});

// readMapping reads /proc/self/maps and writes mappings to b.pb.
// It saves the address ranges of the mappings in b.mem for use
// when emitting locations.
private static void readMapping(this ptr<profileBuilder> _addr_b) {
    ref profileBuilder b = ref _addr_b.val;

    var (data, _) = os.ReadFile("/proc/self/maps");
    parseProcSelfMaps(data, b.addMapping);
    if (len(b.mem) == 0) { // pprof expects a map entry, so fake one.
        b.addMappingEntry(0, 0, 0, "", "", true); 
        // TODO(hyangah): make addMapping return *memMap or
        // take a memMap struct, and get rid of addMappingEntry
        // that takes a bunch of positional arguments.
    }
}

private static void parseProcSelfMaps(slice<byte> data, Action<ulong, ulong, ulong, @string, @string> addMapping) { 
    // $ cat /proc/self/maps
    // 00400000-0040b000 r-xp 00000000 fc:01 787766                             /bin/cat
    // 0060a000-0060b000 r--p 0000a000 fc:01 787766                             /bin/cat
    // 0060b000-0060c000 rw-p 0000b000 fc:01 787766                             /bin/cat
    // 014ab000-014cc000 rw-p 00000000 00:00 0                                  [heap]
    // 7f7d76af8000-7f7d7797c000 r--p 00000000 fc:01 1318064                    /usr/lib/locale/locale-archive
    // 7f7d7797c000-7f7d77b36000 r-xp 00000000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77b36000-7f7d77d36000 ---p 001ba000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d36000-7f7d77d3a000 r--p 001ba000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d3a000-7f7d77d3c000 rw-p 001be000 fc:01 1180226                    /lib/x86_64-linux-gnu/libc-2.19.so
    // 7f7d77d3c000-7f7d77d41000 rw-p 00000000 00:00 0
    // 7f7d77d41000-7f7d77d64000 r-xp 00000000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f3f000-7f7d77f42000 rw-p 00000000 00:00 0
    // 7f7d77f61000-7f7d77f63000 rw-p 00000000 00:00 0
    // 7f7d77f63000-7f7d77f64000 r--p 00022000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f64000-7f7d77f65000 rw-p 00023000 fc:01 1180217                    /lib/x86_64-linux-gnu/ld-2.19.so
    // 7f7d77f65000-7f7d77f66000 rw-p 00000000 00:00 0
    // 7ffc342a2000-7ffc342c3000 rw-p 00000000 00:00 0                          [stack]
    // 7ffc34343000-7ffc34345000 r-xp 00000000 00:00 0                          [vdso]
    // ffffffffff600000-ffffffffff601000 r-xp 00000000 00:00 0                  [vsyscall]

    slice<byte> line = default; 
    // next removes and returns the next field in the line.
    // It also removes from line any spaces following the field.
    Func<slice<byte>> next = () => {
        var j = bytes.IndexByte(line, ' ');
        if (j < 0) {
            var f = line;
            line = null;
            return f;
        }
        f = line[..(int)j];
        line = line[(int)j + 1..];
        while (len(line) > 0 && line[0] == ' ') {
            line = line[(int)1..];
        }
        return f;
    };

    while (len(data) > 0) {
        var i = bytes.IndexByte(data, '\n');
        if (i < 0) {
            (line, data) = (data, null);
        }
        else
 {
            (line, data) = (data[..(int)i], data[(int)i + 1..]);
        }
        var addr = next();
        i = bytes.IndexByte(addr, '-');
        if (i < 0) {
            continue;
        }
        var (lo, err) = strconv.ParseUint(string(addr[..(int)i]), 16, 64);
        if (err != null) {
            continue;
        }
        var (hi, err) = strconv.ParseUint(string(addr[(int)i + 1..]), 16, 64);
        if (err != null) {
            continue;
        }
        var perm = next();
        if (len(perm) < 4 || perm[2] != 'x') { 
            // Only interested in executable mappings.
            continue;
        }
        var (offset, err) = strconv.ParseUint(string(next()), 16, 64);
        if (err != null) {
            continue;
        }
        next(); // dev
        var inode = next(); // inode
        if (line == null) {
            continue;
        }
        var file = string(line); 

        // Trim deleted file marker.
        @string deletedStr = " (deleted)";
        var deletedLen = len(deletedStr);
        if (len(file) >= deletedLen && file[(int)len(file) - deletedLen..] == deletedStr) {
            file = file[..(int)len(file) - deletedLen];
        }
        if (len(inode) == 1 && inode[0] == '0' && file == "") { 
            // Huge-page text mappings list the initial fragment of
            // mapped but unpopulated memory as being inode 0.
            // Don't report that part.
            // But [vdso] and [vsyscall] are inode 0, so let non-empty file names through.
            continue;
        }
        var (buildID, _) = elfBuildID(file);
        addMapping(lo, hi, offset, file, buildID);
    }
}

private static void addMapping(this ptr<profileBuilder> _addr_b, ulong lo, ulong hi, ulong offset, @string file, @string buildID) {
    ref profileBuilder b = ref _addr_b.val;

    b.addMappingEntry(lo, hi, offset, file, buildID, false);
}

private static void addMappingEntry(this ptr<profileBuilder> _addr_b, ulong lo, ulong hi, ulong offset, @string file, @string buildID, bool fake) {
    ref profileBuilder b = ref _addr_b.val;

    b.mem = append(b.mem, new memMap(start:uintptr(lo),end:uintptr(hi),offset:offset,file:file,buildID:buildID,fake:fake,));
}

} // end pprof_package
