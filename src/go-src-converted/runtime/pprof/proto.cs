// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 August 29 08:23:03 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\proto.go
using bytes = go.bytes_package;
using gzip = go.compress.gzip_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace runtime
{
    public static unsafe partial class pprof_package
    {
        // lostProfileEvent is the function to which lost profiling
        // events are attributed.
        // (The name shows up in the pprof graphs.)
        private static void lostProfileEvent()
        {
            lostProfileEvent();

        }

        // funcPC returns the PC for the func value f.
        private static System.UIntPtr funcPC(object f)
        {
            return new ptr<*(ref array<ref System.UIntPtr>)>(@unsafe.Pointer(ref f))[1L];
        }

        // A profileBuilder writes a profile incrementally from a
        // stream of profile samples delivered by the runtime.
        private partial struct profileBuilder
        {
            public time.Time start;
            public time.Time end;
            public bool havePeriod;
            public long period;
            public profMap m; // encoding state
            public io.Writer w;
            public ptr<gzip.Writer> zw;
            public protobuf pb;
            public slice<@string> strings;
            public map<@string, long> stringMap;
            public map<System.UIntPtr, long> locs;
            public map<@string, long> funcs; // Package path-qualified function name to Function.ID
            public slice<memMap> mem;
        }

        private partial struct memMap
        {
            public System.UIntPtr start;
            public System.UIntPtr end;
        }

 
        // message Profile
        private static readonly long tagProfile_SampleType = 1L; // repeated ValueType
        private static readonly long tagProfile_Sample = 2L; // repeated Sample
        private static readonly long tagProfile_Mapping = 3L; // repeated Mapping
        private static readonly long tagProfile_Location = 4L; // repeated Location
        private static readonly long tagProfile_Function = 5L; // repeated Function
        private static readonly long tagProfile_StringTable = 6L; // repeated string
        private static readonly long tagProfile_DropFrames = 7L; // int64 (string table index)
        private static readonly long tagProfile_KeepFrames = 8L; // int64 (string table index)
        private static readonly long tagProfile_TimeNanos = 9L; // int64
        private static readonly long tagProfile_DurationNanos = 10L; // int64
        private static readonly long tagProfile_PeriodType = 11L; // ValueType (really optional string???)
        private static readonly long tagProfile_Period = 12L; // int64

        // message ValueType
        private static readonly long tagValueType_Type = 1L; // int64 (string table index)
        private static readonly long tagValueType_Unit = 2L; // int64 (string table index)

        // message Sample
        private static readonly long tagSample_Location = 1L; // repeated uint64
        private static readonly long tagSample_Value = 2L; // repeated int64
        private static readonly long tagSample_Label = 3L; // repeated Label

        // message Label
        private static readonly long tagLabel_Key = 1L; // int64 (string table index)
        private static readonly long tagLabel_Str = 2L; // int64 (string table index)
        private static readonly long tagLabel_Num = 3L; // int64

        // message Mapping
        private static readonly long tagMapping_ID = 1L; // uint64
        private static readonly long tagMapping_Start = 2L; // uint64
        private static readonly long tagMapping_Limit = 3L; // uint64
        private static readonly long tagMapping_Offset = 4L; // uint64
        private static readonly long tagMapping_Filename = 5L; // int64 (string table index)
        private static readonly long tagMapping_BuildID = 6L; // int64 (string table index)
        private static readonly long tagMapping_HasFunctions = 7L; // bool
        private static readonly long tagMapping_HasFilenames = 8L; // bool
        private static readonly long tagMapping_HasLineNumbers = 9L; // bool
        private static readonly long tagMapping_HasInlineFrames = 10L; // bool

        // message Location
        private static readonly long tagLocation_ID = 1L; // uint64
        private static readonly long tagLocation_MappingID = 2L; // uint64
        private static readonly long tagLocation_Address = 3L; // uint64
        private static readonly long tagLocation_Line = 4L; // repeated Line

        // message Line
        private static readonly long tagLine_FunctionID = 1L; // uint64
        private static readonly long tagLine_Line = 2L; // int64

        // message Function
        private static readonly long tagFunction_ID = 1L; // uint64
        private static readonly long tagFunction_Name = 2L; // int64 (string table index)
        private static readonly long tagFunction_SystemName = 3L; // int64 (string table index)
        private static readonly long tagFunction_Filename = 4L; // int64 (string table index)
        private static readonly long tagFunction_StartLine = 5L; // int64

        // stringIndex adds s to the string table if not already present
        // and returns the index of s in the string table.
        private static long stringIndex(this ref profileBuilder b, @string s)
        {
            var (id, ok) = b.stringMap[s];
            if (!ok)
            {
                id = len(b.strings);
                b.strings = append(b.strings, s);
                b.stringMap[s] = id;
            }
            return int64(id);
        }

        private static void flush(this ref profileBuilder b)
        {
            const long dataFlush = 4096L;

            if (b.pb.nest == 0L && len(b.pb.data) > dataFlush)
            {
                b.zw.Write(b.pb.data);
                b.pb.data = b.pb.data[..0L];
            }
        }

        // pbValueType encodes a ValueType message to b.pb.
        private static void pbValueType(this ref profileBuilder b, long tag, @string typ, @string unit)
        {
            var start = b.pb.startMessage();
            b.pb.int64(tagValueType_Type, b.stringIndex(typ));
            b.pb.int64(tagValueType_Unit, b.stringIndex(unit));
            b.pb.endMessage(tag, start);
        }

        // pbSample encodes a Sample message to b.pb.
        private static void pbSample(this ref profileBuilder b, slice<long> values, slice<ulong> locs, Action labels)
        {
            var start = b.pb.startMessage();
            b.pb.int64s(tagSample_Value, values);
            b.pb.uint64s(tagSample_Location, locs);
            if (labels != null)
            {
                labels();
            }
            b.pb.endMessage(tagProfile_Sample, start);
            b.flush();
        }

        // pbLabel encodes a Label message to b.pb.
        private static void pbLabel(this ref profileBuilder b, long tag, @string key, @string str, long num)
        {
            var start = b.pb.startMessage();
            b.pb.int64Opt(tagLabel_Key, b.stringIndex(key));
            b.pb.int64Opt(tagLabel_Str, b.stringIndex(str));
            b.pb.int64Opt(tagLabel_Num, num);
            b.pb.endMessage(tag, start);
        }

        // pbLine encodes a Line message to b.pb.
        private static void pbLine(this ref profileBuilder b, long tag, ulong funcID, long line)
        {
            var start = b.pb.startMessage();
            b.pb.uint64Opt(tagLine_FunctionID, funcID);
            b.pb.int64Opt(tagLine_Line, line);
            b.pb.endMessage(tag, start);
        }

        // pbMapping encodes a Mapping message to b.pb.
        private static void pbMapping(this ref profileBuilder b, long tag, ulong id, ulong @base, ulong limit, ulong offset, @string file, @string buildID)
        {
            var start = b.pb.startMessage();
            b.pb.uint64Opt(tagMapping_ID, id);
            b.pb.uint64Opt(tagMapping_Start, base);
            b.pb.uint64Opt(tagMapping_Limit, limit);
            b.pb.uint64Opt(tagMapping_Offset, offset);
            b.pb.int64Opt(tagMapping_Filename, b.stringIndex(file));
            b.pb.int64Opt(tagMapping_BuildID, b.stringIndex(buildID)); 
            // TODO: Set any of HasInlineFrames, HasFunctions, HasFilenames, HasLineNumbers?
            // It seems like they should all be true, but they've never been set.
            b.pb.endMessage(tag, start);
        }

        // locForPC returns the location ID for addr.
        // addr must be a return PC. This returns the location of the call.
        // It may emit to b.pb, so there must be no message encoding in progress.
        private static ulong locForPC(this ref profileBuilder b, System.UIntPtr addr)
        {
            var id = uint64(b.locs[addr]);
            if (id != 0L)
            {
                return id;
            } 

            // Expand this one address using CallersFrames so we can cache
            // each expansion. In general, CallersFrames takes a whole
            // stack, but in this case we know there will be no skips in
            // the stack and we have return PCs anyway.
            var frames = runtime.CallersFrames(new slice<System.UIntPtr>(new System.UIntPtr[] { addr }));
            var (frame, more) = frames.Next();
            if (frame.Function == "runtime.goexit")
            { 
                // Short-circuit if we see runtime.goexit so the loop
                // below doesn't allocate a useless empty location.
                return 0L;
            }
            if (frame.PC == 0L)
            { 
                // If we failed to resolve the frame, at least make up
                // a reasonable call PC. This mostly happens in tests.
                frame.PC = addr - 1L;
            } 

            // We can't write out functions while in the middle of the
            // Location message, so record new functions we encounter and
            // write them out after the Location.
            private partial struct newFunc
            {
                public ulong id;
                public @string name;
                public @string file;
            }
            var newFuncs = make_slice<newFunc>(0L, 8L);

            id = uint64(len(b.locs)) + 1L;
            b.locs[addr] = int(id);
            var start = b.pb.startMessage();
            b.pb.uint64Opt(tagLocation_ID, id);
            b.pb.uint64Opt(tagLocation_Address, uint64(frame.PC));
            while (frame.Function != "runtime.goexit")
            { 
                // Write out each line in frame expansion.
                var funcID = uint64(b.funcs[frame.Function]);
                if (funcID == 0L)
                {
                    funcID = uint64(len(b.funcs)) + 1L;
                    b.funcs[frame.Function] = int(funcID);
                    newFuncs = append(newFuncs, new newFunc(funcID,frame.Function,frame.File));
                }
                b.pbLine(tagLocation_Line, funcID, int64(frame.Line));
                if (!more)
                {
                    break;
                }
                frame, more = frames.Next();
            }

            if (len(b.mem) > 0L)
            {
                var i = sort.Search(len(b.mem), i =>
                {
                    return b.mem[i].end > addr;
                });
                if (i < len(b.mem) && b.mem[i].start <= addr && addr < b.mem[i].end)
                {
                    b.pb.uint64Opt(tagLocation_MappingID, uint64(i + 1L));
                }
            }
            b.pb.endMessage(tagProfile_Location, start); 

            // Write out functions we found during frame expansion.
            foreach (var (_, fn) in newFuncs)
            {
                start = b.pb.startMessage();
                b.pb.uint64Opt(tagFunction_ID, fn.id);
                b.pb.int64Opt(tagFunction_Name, b.stringIndex(fn.name));
                b.pb.int64Opt(tagFunction_SystemName, b.stringIndex(fn.name));
                b.pb.int64Opt(tagFunction_Filename, b.stringIndex(fn.file));
                b.pb.endMessage(tagProfile_Function, start);
            }
            b.flush();
            return id;
        }

        // newProfileBuilder returns a new profileBuilder.
        // CPU profiling data obtained from the runtime can be added
        // by calling b.addCPUData, and then the eventual profile
        // can be obtained by calling b.finish.
        private static ref profileBuilder newProfileBuilder(io.Writer w)
        {
            var (zw, _) = gzip.NewWriterLevel(w, gzip.BestSpeed);
            profileBuilder b = ref new profileBuilder(w:w,zw:zw,start:time.Now(),strings:[]string{""},stringMap:map[string]int{"":0},locs:map[uintptr]int{},funcs:map[string]int{},);
            b.readMapping();
            return b;
        }

        // addCPUData adds the CPU profiling data to the profile.
        // The data must be a whole number of records,
        // as delivered by the runtime.
        private static error addCPUData(this ref profileBuilder b, slice<ulong> data, slice<unsafe.Pointer> tags)
        {
            if (!b.havePeriod)
            { 
                // first record is period
                if (len(data) < 3L)
                {
                    return error.As(fmt.Errorf("truncated profile"));
                }
                if (data[0L] != 3L || data[2L] == 0L)
                {
                    return error.As(fmt.Errorf("malformed profile"));
                } 
                // data[2] is sampling rate in Hz. Convert to sampling
                // period in nanoseconds.
                b.period = 1e9F / int64(data[2L]);
                b.havePeriod = true;
                data = data[3L..];
            } 

            // Parse CPU samples from the profile.
            // Each sample is 3+n uint64s:
            //    data[0] = 3+n
            //    data[1] = time stamp (ignored)
            //    data[2] = count
            //    data[3:3+n] = stack
            // If the count is 0 and the stack has length 1,
            // that's an overflow record inserted by the runtime
            // to indicate that stack[0] samples were lost.
            // Otherwise the count is usually 1,
            // but in a few special cases like lost non-Go samples
            // there can be larger counts.
            // Because many samples with the same stack arrive,
            // we want to deduplicate immediately, which we do
            // using the b.m profMap.
            while (len(data) > 0L)
            {
                if (len(data) < 3L || data[0L] > uint64(len(data)))
                {
                    return error.As(fmt.Errorf("truncated profile"));
                }
                if (data[0L] < 3L || tags != null && len(tags) < 1L)
                {
                    return error.As(fmt.Errorf("malformed profile"));
                }
                var count = data[2L];
                var stk = data[3L..data[0L]];
                data = data[data[0L]..];
                unsafe.Pointer tag = default;
                if (tags != null)
                {
                    tag = tags[0L];
                    tags = tags[1L..];
                }
                if (count == 0L && len(stk) == 1L)
                { 
                    // overflow record
                    count = uint64(stk[0L]);
                    stk = new slice<ulong>(new ulong[] { uint64(funcPC(lostProfileEvent)) });
                }
                b.m.lookup(stk, tag).count += int64(count);
            }

            return error.As(null);
        }

        // build completes and returns the constructed profile.
        private static error build(this ref profileBuilder b)
        {
            b.end = time.Now();

            b.pb.int64Opt(tagProfile_TimeNanos, b.start.UnixNano());
            if (b.havePeriod)
            { // must be CPU profile
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

                while (e != null)
                {
                    values[0L] = e.count;
                    values[1L] = e.count * b.period;

                    Action labels = default;
                    if (e.tag != null)
                    {
                        labels = () =>
                        {
                            foreach (var (k, v) in e.tag.Value)
                            {
                                b.pbLabel(tagSample_Label, k, v, 0L);
                            }
                    e = e.nextAll;
                        }
;
                    }
                    locs = locs[..0L];
                    foreach (var (i, addr) in e.stk)
                    { 
                        // Addresses from stack traces point to the
                        // next instruction after each call, except
                        // for the leaf, which points to where the
                        // signal occurred. locForPC expects return
                        // PCs, so increment the leaf address to look
                        // like a return PC.
                        if (i == 0L)
                        {
                            addr++;
                        }
                        var l = b.locForPC(addr);
                        if (l == 0L)
                        { // runtime.goexit
                            continue;
                        }
                        locs = append(locs, l);
                    }
                    b.pbSample(values, locs, labels);
                } 

                // TODO: Anything for tagProfile_DropFrames?
                // TODO: Anything for tagProfile_KeepFrames?

            } 

            // TODO: Anything for tagProfile_DropFrames?
            // TODO: Anything for tagProfile_KeepFrames?

            b.pb.strings(tagProfile_StringTable, b.strings);
            b.zw.Write(b.pb.data);
            b.zw.Close();
            return error.As(null);
        }

        // readMapping reads /proc/self/maps and writes mappings to b.pb.
        // It saves the address ranges of the mappings in b.mem for use
        // when emitting locations.
        private static void readMapping(this ref profileBuilder b)
        {
            var (data, _) = ioutil.ReadFile("/proc/self/maps");
            parseProcSelfMaps(data, b.addMapping);
        }

        private static void parseProcSelfMaps(slice<byte> data, Action<ulong, ulong, ulong, @string, @string> addMapping)
        { 
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
            Func<slice<byte>> next = () =>
            {
                var j = bytes.IndexByte(line, ' ');
                if (j < 0L)
                {
                    var f = line;
                    line = null;
                    return f;
                }
                f = line[..j];
                line = line[j + 1L..];
                while (len(line) > 0L && line[0L] == ' ')
                {
                    line = line[1L..];
                }

                return f;
            }
;

            while (len(data) > 0L)
            {
                var i = bytes.IndexByte(data, '\n');
                if (i < 0L)
                {
                    line = data;
                    data = null;
                }
                else
                {
                    line = data[..i];
                    data = data[i + 1L..];
                }
                var addr = next();
                i = bytes.IndexByte(addr, '-');
                if (i < 0L)
                {
                    continue;
                }
                var (lo, err) = strconv.ParseUint(string(addr[..i]), 16L, 64L);
                if (err != null)
                {
                    continue;
                }
                var (hi, err) = strconv.ParseUint(string(addr[i + 1L..]), 16L, 64L);
                if (err != null)
                {
                    continue;
                }
                var perm = next();
                if (len(perm) < 4L || perm[2L] != 'x')
                { 
                    // Only interested in executable mappings.
                    continue;
                }
                var (offset, err) = strconv.ParseUint(string(next()), 16L, 64L);
                if (err != null)
                {
                    continue;
                }
                next(); // dev
                var inode = next(); // inode
                if (line == null)
                {
                    continue;
                }
                var file = string(line);
                if (len(inode) == 1L && inode[0L] == '0' && file == "")
                { 
                    // Huge-page text mappings list the initial fragment of
                    // mapped but unpopulated memory as being inode 0.
                    // Don't report that part.
                    // But [vdso] and [vsyscall] are inode 0, so let non-empty file names through.
                    continue;
                } 

                // TODO: pprof's remapMappingIDs makes two adjustments:
                // 1. If there is an /anon_hugepage mapping first and it is
                // consecutive to a next mapping, drop the /anon_hugepage.
                // 2. If start-offset = 0x400000, change start to 0x400000 and offset to 0.
                // There's no indication why either of these is needed.
                // Let's try not doing these and see what breaks.
                // If we do need them, they would go here, before we
                // enter the mappings into b.mem in the first place.
                var (buildID, _) = elfBuildID(file);
                addMapping(lo, hi, offset, file, buildID);
            }

        }

        private static void addMapping(this ref profileBuilder b, ulong lo, ulong hi, ulong offset, @string file, @string buildID)
        {
            b.mem = append(b.mem, new memMap(uintptr(lo),uintptr(hi)));
            b.pbMapping(tagProfile_Mapping, uint64(len(b.mem)), lo, hi, offset, file, buildID);
        }
    }
}}
