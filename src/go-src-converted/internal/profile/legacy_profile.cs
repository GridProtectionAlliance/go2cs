// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements parsers to convert legacy profiles into the
// profile.proto format.

// package profile -- go2cs converted at 2022 March 13 05:38:43 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Program Files\Go\src\internal\profile\legacy_profile.go
namespace go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;
using System;

public static partial class profile_package {

private static var countStartRE = regexp.MustCompile("\\A(\\w+) profile: total \\d+\\n\\z");private static var countRE = regexp.MustCompile("\\A(\\d+) @(( 0x[0-9a-f]+)+)\\n\\z");private static var heapHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] *@ *(heap[_a-z0-9]*)/?(\\d*)");private static var heapSampleRE = regexp.MustCompile("(-?\\d+): *(-?\\d+) *\\[ *(\\d+): *(\\d+) *] @([ x0-9a-f]*)");private static var contentionSampleRE = regexp.MustCompile("(\\d+) *(\\d+) @([ x0-9a-f]*)");private static var hexNumberRE = regexp.MustCompile("0x[0-9a-f]+");private static var growthHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] @ growthz");private static var fragmentationHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] @ fragmentationz");private static var threadzStartRE = regexp.MustCompile("--- threadz \\d+ ---");private static var threadStartRE = regexp.MustCompile("--- Thread ([[:xdigit:]]+) \\(name: (.*)/(\\d+)\\) stack: ---");private static var procMapsRE = regexp.MustCompile("([[:xdigit:]]+)-([[:xdigit:]]+)\\s+([-rwxp]+)\\s+([[:xdigit:]]+)\\s+([[:xdigit:]]+):" +
    "([[:xdigit:]]+)\\s+([[:digit:]]+)\\s*(\\S+)?");private static var briefMapsRE = regexp.MustCompile("\\s*([[:xdigit:]]+)-([[:xdigit:]]+):\\s*(\\S+)(\\s.*@)?([[:xdigit:]]+)?");public static bool LegacyHeapAllocated = default;

private static bool isSpaceOrComment(@string line) {
    var trimmed = strings.TrimSpace(line);
    return len(trimmed) == 0 || trimmed[0] == '#';
}

// parseGoCount parses a Go count profile (e.g., threadcreate or
// goroutine) and returns a new Profile.
private static (ptr<Profile>, error) parseGoCount(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var r = bytes.NewBuffer(b);

    @string line = default;
    error err = default!;
    while (true) { 
        // Skip past comments and empty lines seeking a real header.
        line, err = r.ReadString('\n');
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (!isSpaceOrComment(line)) {
            break;
        }
    }

    var m = countStartRE.FindStringSubmatch(line);
    if (m == null) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    var profileType = m[1];
    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{Type:profileType,Unit:"count"},Period:1,SampleType:[]*ValueType{{Type:profileType,Unit:"count"}},));
    var locations = make_map<ulong, ptr<Location>>();
    while (true) {
        line, err = r.ReadString('\n');
        if (err != null) {
            if (err == io.EOF) {
                break;
            }
            return (_addr_null!, error.As(err)!);
        }
        if (isSpaceOrComment(line)) {
            continue;
        }
        if (strings.HasPrefix(line, "---")) {
            break;
        }
        m = countRE.FindStringSubmatch(line);
        if (m == null) {
            return (_addr_null!, error.As(errMalformed)!);
        }
        var (n, err) = strconv.ParseInt(m[1], 0, 64);
        if (err != null) {
            return (_addr_null!, error.As(errMalformed)!);
        }
        var fields = strings.Fields(m[2]);
        var locs = make_slice<ptr<Location>>(0, len(fields));
        foreach (var (_, stk) in fields) {
            var (addr, err) = strconv.ParseUint(stk, 0, 64);
            if (err != null) {
                return (_addr_null!, error.As(errMalformed)!);
            } 
            // Adjust all frames by -1 to land on the call instruction.
            addr--;
            var loc = locations[addr];
            if (loc == null) {
                loc = addr(new Location(Address:addr,));
                locations[addr] = loc;
                p.Location = append(p.Location, loc);
            }
            locs = append(locs, loc);
        }        p.Sample = append(p.Sample, addr(new Sample(Location:locs,Value:[]int64{n},)));
    }

    err = error.As(parseAdditionalSections(strings.TrimSpace(line), _addr_r, p))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// remapLocationIDs ensures there is a location for each address
// referenced by a sample, and remaps the samples to point to the new
// location ids.
private static void remapLocationIDs(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    var seen = make_map<ptr<Location>, bool>(len(p.Location));
    slice<ptr<Location>> locs = default;

    foreach (var (_, s) in p.Sample) {
        foreach (var (_, l) in s.Location) {
            if (seen[l]) {
                continue;
            }
            l.ID = uint64(len(locs) + 1);
            locs = append(locs, l);
            seen[l] = true;
        }
    }    p.Location = locs;
}

private static void remapFunctionIDs(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    var seen = make_map<ptr<Function>, bool>(len(p.Function));
    slice<ptr<Function>> fns = default;

    foreach (var (_, l) in p.Location) {
        foreach (var (_, ln) in l.Line) {
            var fn = ln.Function;
            if (fn == null || seen[fn]) {
                continue;
            }
            fn.ID = uint64(len(fns) + 1);
            fns = append(fns, fn);
            seen[fn] = true;
        }
    }    p.Function = fns;
}

// remapMappingIDs matches location addresses with existing mappings
// and updates them appropriately. This is O(N*M), if this ever shows
// up as a bottleneck, evaluate sorting the mappings and doing a
// binary search, which would make it O(N*log(M)).
private static void remapMappingIDs(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    if (len(p.Mapping) == 0) {
        return ;
    }
    {
        var m__prev1 = m;

        var m = p.Mapping[0];

        if (strings.HasPrefix(m.File, "/anon_hugepage")) {
            if (len(p.Mapping) > 1 && m.Limit == p.Mapping[1].Start) {
                p.Mapping = p.Mapping[(int)1..];
            }
        }
        m = m__prev1;

    } 

    // Subtract the offset from the start of the main mapping if it
    // ends up at a recognizable start address.
    const nuint expectedStart = 0x400000;

    {
        var m__prev1 = m;

        m = p.Mapping[0];

        if (m.Start - m.Offset == expectedStart) {
            m.Start = expectedStart;
            m.Offset = 0;
        }
        m = m__prev1;

    }

    foreach (var (_, l) in p.Location) {
        {
            var a = l.Address;

            if (a != 0) {
                {
                    var m__prev2 = m;

                    foreach (var (_, __m) in p.Mapping) {
                        m = __m;
                        if (m.Start <= a && a < m.Limit) {
                            l.Mapping = m;
                            break;
                        }
                    }

                    m = m__prev2;
                }
            }

        }
    }    {
        var m__prev1 = m;

        foreach (var (__i, __m) in p.Mapping) {
            i = __i;
            m = __m;
            m.ID = uint64(i + 1);
        }
        m = m__prev1;
    }
}

private static Func<slice<byte>, (ulong, slice<byte>)> cpuInts = new slice<Func<slice<byte>, (ulong, slice<byte>)>>(new Func<slice<byte>, (ulong, slice<byte>)>[] { get32l, get32b, get64l, get64b });

private static (ulong, slice<byte>) get32l(slice<byte> b) {
    ulong _p0 = default;
    slice<byte> _p0 = default;

    if (len(b) < 4) {
        return (0, null);
    }
    return (uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24, b[(int)4..]);
}

private static (ulong, slice<byte>) get32b(slice<byte> b) {
    ulong _p0 = default;
    slice<byte> _p0 = default;

    if (len(b) < 4) {
        return (0, null);
    }
    return (uint64(b[3]) | uint64(b[2]) << 8 | uint64(b[1]) << 16 | uint64(b[0]) << 24, b[(int)4..]);
}

private static (ulong, slice<byte>) get64l(slice<byte> b) {
    ulong _p0 = default;
    slice<byte> _p0 = default;

    if (len(b) < 8) {
        return (0, null);
    }
    return (uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56, b[(int)8..]);
}

private static (ulong, slice<byte>) get64b(slice<byte> b) {
    ulong _p0 = default;
    slice<byte> _p0 = default;

    if (len(b) < 8) {
        return (0, null);
    }
    return (uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56, b[(int)8..]);
}

// ParseTracebacks parses a set of tracebacks and returns a newly
// populated profile. It will accept any text file and generate a
// Profile out of it with any hex addresses it can identify, including
// a process map if it can recognize one. Each sample will include a
// tag "source" with the addresses recognized in string format.
public static (ptr<Profile>, error) ParseTracebacks(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var r = bytes.NewBuffer(b);

    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{Type:"trace",Unit:"count"},Period:1,SampleType:[]*ValueType{{Type:"trace",Unit:"count"},},));

    slice<@string> sources = default;
    slice<ptr<Location>> sloc = default;

    var locs = make_map<ulong, ptr<Location>>();
    while (true) {
        var (l, err) = r.ReadString('\n');
        if (err != null) {
            if (err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }
            if (l == "") {
                break;
            }
        }
        if (sectionTrigger(l) == memoryMapSection) {
            break;
        }
        {
            var (s, addrs) = extractHexAddresses(l);

            if (len(s) > 0) {
                foreach (var (_, addr) in addrs) { 
                    // Addresses from stack traces point to the next instruction after
                    // each call. Adjust by -1 to land somewhere on the actual call.
                    addr--;
                    var loc = locs[addr];
                    if (locs[addr] == null) {
                        loc = addr(new Location(Address:addr,));
                        p.Location = append(p.Location, loc);
                        locs[addr] = loc;
                    }
                    sloc = append(sloc, loc);
                }
            else

                sources = append(sources, s);
            } {
                if (len(sources) > 0 || len(sloc) > 0) {
                    addTracebackSample(sloc, sources, p);
                    (sloc, sources) = (null, null);
                }
            }

        }
    } 

    // Add final sample to save any leftover data.
    if (len(sources) > 0 || len(sloc) > 0) {
        addTracebackSample(sloc, sources, p);
    }
    {
        var err = p.ParseMemoryMap(r);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    return (_addr_p!, error.As(null!)!);
}

private static void addTracebackSample(slice<ptr<Location>> l, slice<@string> s, ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    p.Sample = append(p.Sample, addr(new Sample(Value:[]int64{1},Location:l,Label:map[string][]string{"source":s},)));
}

// parseCPU parses a profilez legacy profile and returns a newly
// populated Profile.
//
// The general format for profilez samples is a sequence of words in
// binary format. The first words are a header with the following data:
//   1st word -- 0
//   2nd word -- 3
//   3rd word -- 0 if a c++ application, 1 if a java application.
//   4th word -- Sampling period (in microseconds).
//   5th word -- Padding.
private static (ptr<Profile>, error) parseCPU(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    Func<slice<byte>, (ulong, slice<byte>)> parse = default;
    ulong n1 = default;    ulong n2 = default;    ulong n3 = default;    ulong n4 = default;    ulong n5 = default;

    foreach (var (_, __parse) in cpuInts) {
        parse = __parse;
        slice<byte> tmp = default;
        n1, tmp = parse(b);
        n2, tmp = parse(tmp);
        n3, tmp = parse(tmp);
        n4, tmp = parse(tmp);
        n5, tmp = parse(tmp);

        if (tmp != null && n1 == 0 && n2 == 3 && n3 == 0 && n4 > 0 && n5 == 0) {
            b = tmp;
            return _addr_cpuProfile(b, int64(n4), parse)!;
        }
    }
    return (_addr_null!, error.As(errUnrecognized)!);
}

// cpuProfile returns a new Profile from C++ profilez data.
// b is the profile bytes after the header, period is the profiling
// period, and parse is a function to parse 8-byte chunks from the
// profile in its native endianness.
private static (ptr<Profile>, error) cpuProfile(slice<byte> b, long period, Func<slice<byte>, (ulong, slice<byte>)> parse) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    ptr<Profile> p = addr(new Profile(Period:period*1000,PeriodType:&ValueType{Type:"cpu",Unit:"nanoseconds"},SampleType:[]*ValueType{{Type:"samples",Unit:"count"},{Type:"cpu",Unit:"nanoseconds"},},));
    error err = default!;
    b, _, err = parseCPUSamples(b, parse, true, p);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (len(p.Sample) > 1 && len(p.Sample[0].Location) > 1) {
        var allSame = true;
        var id1 = p.Sample[0].Location[1].Address;
        {
            var s__prev1 = s;

            foreach (var (_, __s) in p.Sample) {
                s = __s;
                if (len(s.Location) < 2 || id1 != s.Location[1].Address) {
                    allSame = false;
                    break;
                }
            }

            s = s__prev1;
        }

        if (allSame) {
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample) {
                    s = __s;
                    s.Location = append(s.Location[..(int)1], s.Location[(int)2..]);
                }

                s = s__prev1;
            }
        }
    }
    {
        error err__prev1 = err;

        err = p.ParseMemoryMap(bytes.NewBuffer(b));

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }
    return (_addr_p!, error.As(null!)!);
}

// parseCPUSamples parses a collection of profilez samples from a
// profile.
//
// profilez samples are a repeated sequence of stack frames of the
// form:
//    1st word -- The number of times this stack was encountered.
//    2nd word -- The size of the stack (StackSize).
//    3rd word -- The first address on the stack.
//    ...
//    StackSize + 2 -- The last address on the stack
// The last stack trace is of the form:
//   1st word -- 0
//   2nd word -- 1
//   3rd word -- 0
//
// Addresses from stack traces may point to the next instruction after
// each call. Optionally adjust by -1 to land somewhere on the actual
// call (except for the leaf, which is not a call).
private static (slice<byte>, map<ulong, ptr<Location>>, error) parseCPUSamples(slice<byte> b, Func<slice<byte>, (ulong, slice<byte>)> parse, bool adjust, ptr<Profile> _addr_p) {
    slice<byte> _p0 = default;
    map<ulong, ptr<Location>> _p0 = default;
    error _p0 = default!;
    ref Profile p = ref _addr_p.val;

    var locs = make_map<ulong, ptr<Location>>();
    while (len(b) > 0) {
        ulong count = default;        ulong nstk = default;

        count, b = parse(b);
        nstk, b = parse(b);
        if (b == null || nstk > uint64(len(b) / 4)) {
            return (null, null, error.As(errUnrecognized)!);
        }
        slice<ptr<Location>> sloc = default;
        var addrs = make_slice<ulong>(nstk);
        {
            nint i__prev2 = i;

            for (nint i = 0; i < int(nstk); i++) {
                addrs[i], b = parse(b);
            }


            i = i__prev2;
        }

        if (count == 0 && nstk == 1 && addrs[0] == 0) { 
            // End of data marker
            break;
        }
        {
            nint i__prev2 = i;

            foreach (var (__i, __addr) in addrs) {
                i = __i;
                addr = __addr;
                if (adjust && i > 0) {
                    addr--;
                }
                var loc = locs[addr];
                if (loc == null) {
                    loc = addr(new Location(Address:addr,));
                    locs[addr] = loc;
                    p.Location = append(p.Location, loc);
                }
                sloc = append(sloc, loc);
            }

            i = i__prev2;
        }

        p.Sample = append(p.Sample, addr(new Sample(Value:[]int64{int64(count),int64(count)*p.Period},Location:sloc,)));
    } 
    // Reached the end without finding the EOD marker.
    return (b, locs, error.As(null!)!);
}

// parseHeap parses a heapz legacy or a growthz profile and
// returns a newly populated Profile.
private static (ptr<Profile>, error) parseHeap(slice<byte> b) {
    ptr<Profile> p = default!;
    error err = default!;

    var r = bytes.NewBuffer(b);
    var (l, err) = r.ReadString('\n');
    if (err != null) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    @string sampling = "";

    {
        var header = heapHeaderRE.FindStringSubmatch(l);

        if (header != null) {
            p = addr(new Profile(SampleType:[]*ValueType{{Type:"objects",Unit:"count"},{Type:"space",Unit:"bytes"},},PeriodType:&ValueType{Type:"objects",Unit:"bytes"},));

            long period = default;
            if (len(header[6]) > 0) {
                period, err = strconv.ParseInt(header[6], 10, 64);

                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
            }
            switch (header[5]) {
                case "heapz_v2": 

                case "heap_v2": 
                    (sampling, p.Period) = ("v2", period);
                    break;
                case "heapprofile": 
                    (sampling, p.Period) = ("", 1);
                    break;
                case "heap": 
                    (sampling, p.Period) = ("v2", period / 2);
                    break;
                default: 
                    return (_addr_null!, error.As(errUnrecognized)!);
                    break;
            }
        }        header = growthHeaderRE.FindStringSubmatch(l);


        else if (header != null) {
            p = addr(new Profile(SampleType:[]*ValueType{{Type:"objects",Unit:"count"},{Type:"space",Unit:"bytes"},},PeriodType:&ValueType{Type:"heapgrowth",Unit:"count"},Period:1,));
        }        header = fragmentationHeaderRE.FindStringSubmatch(l);


        else if (header != null) {
            p = addr(new Profile(SampleType:[]*ValueType{{Type:"objects",Unit:"count"},{Type:"space",Unit:"bytes"},},PeriodType:&ValueType{Type:"allocations",Unit:"count"},Period:1,));
        }
        else
 {
            return (_addr_null!, error.As(errUnrecognized)!);
        }

    }

    if (LegacyHeapAllocated) {
        {
            var st__prev1 = st;

            foreach (var (_, __st) in p.SampleType) {
                st = __st;
                st.Type = "alloc_" + st.Type;
            }
    else

            st = st__prev1;
        }
    } {
        {
            var st__prev1 = st;

            foreach (var (_, __st) in p.SampleType) {
                st = __st;
                st.Type = "inuse_" + st.Type;
            }

            st = st__prev1;
        }
    }
    var locs = make_map<ulong, ptr<Location>>();
    while (true) {
        l, err = r.ReadString('\n');
        if (err != null) {
            if (err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }
            if (l == "") {
                break;
            }
        }
        if (isSpaceOrComment(l)) {
            continue;
        }
        l = strings.TrimSpace(l);

        if (sectionTrigger(l) != unrecognizedSection) {
            break;
        }
        var (value, blocksize, addrs, err) = parseHeapSample(l, p.Period, sampling);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        slice<ptr<Location>> sloc = default;
        foreach (var (_, addr) in addrs) { 
            // Addresses from stack traces point to the next instruction after
            // each call. Adjust by -1 to land somewhere on the actual call.
            addr--;
            var loc = locs[addr];
            if (locs[addr] == null) {
                loc = addr(new Location(Address:addr,));
                p.Location = append(p.Location, loc);
                locs[addr] = loc;
            }
            sloc = append(sloc, loc);
        }        p.Sample = append(p.Sample, addr(new Sample(Value:value,Location:sloc,NumLabel:map[string][]int64{"bytes":{blocksize}},)));
    }

    err = parseAdditionalSections(l, _addr_r, _addr_p);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// parseHeapSample parses a single row from a heap profile into a new Sample.
private static (slice<long>, long, slice<ulong>, error) parseHeapSample(@string line, long rate, @string sampling) {
    slice<long> value = default;
    long blocksize = default;
    slice<ulong> addrs = default;
    error err = default!;

    var sampleData = heapSampleRE.FindStringSubmatch(line);
    if (len(sampleData) != 6) {
        return (value, blocksize, addrs, error.As(fmt.Errorf("unexpected number of sample values: got %d, want 6", len(sampleData)))!);
    }
    nint valueIndex = 1;
    if (LegacyHeapAllocated) {
        valueIndex = 3;
    }
    long v1 = default;    long v2 = default;

    v1, err = strconv.ParseInt(sampleData[valueIndex], 10, 64);

    if (err != null) {
        return (value, blocksize, addrs, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    v2, err = strconv.ParseInt(sampleData[valueIndex + 1], 10, 64);

    if (err != null) {
        return (value, blocksize, addrs, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    if (v1 == 0) {
        if (v2 != 0) {
            return (value, blocksize, addrs, error.As(fmt.Errorf("allocation count was 0 but allocation bytes was %d", v2))!);
        }
    }
    else
 {
        blocksize = v2 / v1;
        if (sampling == "v2") {
            v1, v2 = scaleHeapSample(v1, v2, rate);
        }
    }
    value = new slice<long>(new long[] { v1, v2 });
    addrs = parseHexAddresses(sampleData[5]);

    return (value, blocksize, addrs, error.As(null!)!);
}

// extractHexAddresses extracts hex numbers from a string and returns
// them, together with their numeric value, in a slice.
private static (slice<@string>, slice<ulong>) extractHexAddresses(@string s) => func((_, panic, _) => {
    slice<@string> _p0 = default;
    slice<ulong> _p0 = default;

    var hexStrings = hexNumberRE.FindAllString(s, -1);
    slice<ulong> ids = default;
    foreach (var (_, s) in hexStrings) {
        {
            var (id, err) = strconv.ParseUint(s, 0, 64);

            if (err == null) {
                ids = append(ids, id);
            }
            else
 { 
                // Do not expect any parsing failures due to the regexp matching.
                panic("failed to parse hex value:" + s);
            }

        }
    }    return (hexStrings, ids);
});

// parseHexAddresses parses hex numbers from a string and returns them
// in a slice.
private static slice<ulong> parseHexAddresses(@string s) {
    var (_, ids) = extractHexAddresses(s);
    return ids;
}

// scaleHeapSample adjusts the data from a heapz Sample to
// account for its probability of appearing in the collected
// data. heapz profiles are a sampling of the memory allocations
// requests in a program. We estimate the unsampled value by dividing
// each collected sample by its probability of appearing in the
// profile. heapz v2 profiles rely on a poisson process to determine
// which samples to collect, based on the desired average collection
// rate R. The probability of a sample of size S to appear in that
// profile is 1-exp(-S/R).
private static (long, long) scaleHeapSample(long count, long size, long rate) {
    long _p0 = default;
    long _p0 = default;

    if (count == 0 || size == 0) {
        return (0, 0);
    }
    if (rate <= 1) { 
        // if rate==1 all samples were collected so no adjustment is needed.
        // if rate<1 treat as unknown and skip scaling.
        return (count, size);
    }
    var avgSize = float64(size) / float64(count);
    nint scale = 1 / (1 - math.Exp(-avgSize / float64(rate)));

    return (int64(float64(count) * scale), int64(float64(size) * scale));
}

// parseContention parses a mutex or contention profile. There are 2 cases:
// "--- contentionz " for legacy C++ profiles (and backwards compatibility)
// "--- mutex:" or "--- contention:" for profiles generated by the Go runtime.
// This code converts the text output from runtime into a *Profile. (In the future
// the runtime might write a serialized Profile directly making this unnecessary.)
private static (ptr<Profile>, error) parseContention(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var r = bytes.NewBuffer(b);
    @string l = default;
    error err = default!;
    while (true) { 
        // Skip past comments and empty lines seeking a real header.
        l, err = r.ReadString('\n');
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (!isSpaceOrComment(l)) {
            break;
        }
    }

    if (strings.HasPrefix(l, "--- contentionz ")) {
        return _addr_parseCppContention(_addr_r)!;
    }
    else if (strings.HasPrefix(l, "--- mutex:")) {
        return _addr_parseCppContention(_addr_r)!;
    }
    else if (strings.HasPrefix(l, "--- contention:")) {
        return _addr_parseCppContention(_addr_r)!;
    }
    return (_addr_null!, error.As(errUnrecognized)!);
}

// parseCppContention parses the output from synchronization_profiling.cc
// for backward compatibility, and the compatible (non-debug) block profile
// output from the Go runtime.
private static (ptr<Profile>, error) parseCppContention(ptr<bytes.Buffer> _addr_r) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;
    ref bytes.Buffer r = ref _addr_r.val;

    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{Type:"contentions",Unit:"count"},Period:1,SampleType:[]*ValueType{{Type:"contentions",Unit:"count"},{Type:"delay",Unit:"nanoseconds"},},));

    long cpuHz = default;
    @string l = default;
    error err = default!; 
    // Parse text of the form "attribute = value" before the samples.
    const @string delimiter = "=";

    while (true) {
        l, err = r.ReadString('\n');
        if (err != null) {
            if (err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }
            if (l == "") {
                break;
            }
        }
        if (isSpaceOrComment(l)) {
            continue;
        }
        l = strings.TrimSpace(l);

        if (l == "") {
            continue;
        }
        if (strings.HasPrefix(l, "---")) {
            break;
        }
        var attr = strings.SplitN(l, delimiter, 2);
        if (len(attr) != 2) {
            break;
        }
        var key = strings.TrimSpace(attr[0]);
        var val = strings.TrimSpace(attr[1]);
        err = default!;
        switch (key) {
            case "cycles/second": 
                cpuHz, err = strconv.ParseInt(val, 0, 64);

                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
                break;
            case "sampling period": 
                p.Period, err = strconv.ParseInt(val, 0, 64);

                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
                break;
            case "ms since reset": 
                var (ms, err) = strconv.ParseInt(val, 0, 64);
                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
                p.DurationNanos = ms * 1000 * 1000;
                break;
            case "format": 
                // CPP contentionz profiles don't have format.
                return (_addr_null!, error.As(errUnrecognized)!);
                break;
            case "resolution": 
                // CPP contentionz profiles don't have resolution.
                return (_addr_null!, error.As(errUnrecognized)!);
                break;
            case "discarded samples": 

                break;
            default: 
                return (_addr_null!, error.As(errUnrecognized)!);
                break;
        }
    }

    var locs = make_map<ulong, ptr<Location>>();
    while (true) {
        if (!isSpaceOrComment(l)) {
            l = strings.TrimSpace(l);

            if (strings.HasPrefix(l, "---")) {
                break;
            }
            var (value, addrs, err) = parseContentionSample(l, p.Period, cpuHz);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
            slice<ptr<Location>> sloc = default;
            foreach (var (_, addr) in addrs) { 
                // Addresses from stack traces point to the next instruction after
                // each call. Adjust by -1 to land somewhere on the actual call.
                addr--;
                var loc = locs[addr];
                if (locs[addr] == null) {
                    loc = addr(new Location(Address:addr,));
                    p.Location = append(p.Location, loc);
                    locs[addr] = loc;
                }
                sloc = append(sloc, loc);
            }
            p.Sample = append(p.Sample, addr(new Sample(Value:value,Location:sloc,)));
        }
        l, err = r.ReadString('\n');

        if (err != null) {
            if (err != io.EOF) {
                return (_addr_null!, error.As(err)!);
            }
            if (l == "") {
                break;
            }
        }
    }

    err = error.As(parseAdditionalSections(l, _addr_r, p))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// parseContentionSample parses a single row from a contention profile
// into a new Sample.
private static (slice<long>, slice<ulong>, error) parseContentionSample(@string line, long period, long cpuHz) {
    slice<long> value = default;
    slice<ulong> addrs = default;
    error err = default!;

    var sampleData = contentionSampleRE.FindStringSubmatch(line);
    if (sampleData == null) {
        return (value, addrs, error.As(errUnrecognized)!);
    }
    var (v1, err) = strconv.ParseInt(sampleData[1], 10, 64);
    if (err != null) {
        return (value, addrs, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    var (v2, err) = strconv.ParseInt(sampleData[2], 10, 64);
    if (err != null) {
        return (value, addrs, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    if (period > 0) {
        if (cpuHz > 0) {
            var cpuGHz = float64(cpuHz) / 1e9F;
            v1 = int64(float64(v1) * float64(period) / cpuGHz);
        }
        v2 = v2 * period;
    }
    value = new slice<long>(new long[] { v2, v1 });
    addrs = parseHexAddresses(sampleData[3]);

    return (value, addrs, error.As(null!)!);
}

// parseThread parses a Threadz profile and returns a new Profile.
private static (ptr<Profile>, error) parseThread(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var r = bytes.NewBuffer(b);

    @string line = default;
    error err = default!;
    while (true) { 
        // Skip past comments and empty lines seeking a real header.
        line, err = r.ReadString('\n');
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (!isSpaceOrComment(line)) {
            break;
        }
    }

    {
        var m = threadzStartRE.FindStringSubmatch(line);

        if (m != null) { 
            // Advance over initial comments until first stack trace.
            while (true) {
                line, err = r.ReadString('\n');
                if (err != null) {
                    if (err != io.EOF) {
                        return (_addr_null!, error.As(err)!);
                    }
                    if (line == "") {
                        break;
                    }
                }
                if (sectionTrigger(line) != unrecognizedSection || line[0] == '-') {
                    break;
                }
            }
        }        {
            var t__prev2 = t;

            var t = threadStartRE.FindStringSubmatch(line);


            else if (len(t) != 4) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }

            t = t__prev2;

        }


    }

    ptr<Profile> p = addr(new Profile(SampleType:[]*ValueType{{Type:"thread",Unit:"count"}},PeriodType:&ValueType{Type:"thread",Unit:"count"},Period:1,));

    var locs = make_map<ulong, ptr<Location>>(); 
    // Recognize each thread and populate profile samples.
    while (sectionTrigger(line) == unrecognizedSection) {
        if (strings.HasPrefix(line, "---- no stack trace for")) {
            line = "";
            break;
        }
        {
            var t__prev1 = t;

            t = threadStartRE.FindStringSubmatch(line);

            if (len(t) != 4) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }

            t = t__prev1;

        }

        slice<ulong> addrs = default;
        line, addrs, err = parseThreadSample(_addr_r);
        if (err != null) {
            return (_addr_null!, error.As(errUnrecognized)!);
        }
        if (len(addrs) == 0) { 
            // We got a --same as previous threads--. Bump counters.
            if (len(p.Sample) > 0) {
                var s = p.Sample[len(p.Sample) - 1];
                s.Value[0]++;
            }
            continue;
        }
        slice<ptr<Location>> sloc = default;
        foreach (var (_, addr) in addrs) { 
            // Addresses from stack traces point to the next instruction after
            // each call. Adjust by -1 to land somewhere on the actual call.
            addr--;
            var loc = locs[addr];
            if (locs[addr] == null) {
                loc = addr(new Location(Address:addr,));
                p.Location = append(p.Location, loc);
                locs[addr] = loc;
            }
            sloc = append(sloc, loc);
        }        p.Sample = append(p.Sample, addr(new Sample(Value:[]int64{1},Location:sloc,)));
    }

    err = error.As(parseAdditionalSections(line, _addr_r, p))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// parseThreadSample parses a symbolized or unsymbolized stack trace.
// Returns the first line after the traceback, the sample (or nil if
// it hits a 'same-as-previous' marker) and an error.
private static (@string, slice<ulong>, error) parseThreadSample(ptr<bytes.Buffer> _addr_b) {
    @string nextl = default;
    slice<ulong> addrs = default;
    error err = default!;
    ref bytes.Buffer b = ref _addr_b.val;

    @string l = default;
    var sameAsPrevious = false;
    while (true) {
        l, err = b.ReadString('\n');

        if (err != null) {
            if (err != io.EOF) {
                return ("", null, error.As(err)!);
            }
            if (l == "") {
                break;
            }
        }
        l = strings.TrimSpace(l);

        if (l == "") {
            continue;
        }
        if (strings.HasPrefix(l, "---")) {
            break;
        }
        if (strings.Contains(l, "same as previous thread")) {
            sameAsPrevious = true;
            continue;
        }
        addrs = append(addrs, parseHexAddresses(l));
    }

    if (sameAsPrevious) {
        return (l, null, error.As(null!)!);
    }
    return (l, addrs, error.As(null!)!);
}

// parseAdditionalSections parses any additional sections in the
// profile, ignoring any unrecognized sections.
private static error parseAdditionalSections(@string l, ptr<bytes.Buffer> _addr_b, ptr<Profile> _addr_p) {
    error err = default!;
    ref bytes.Buffer b = ref _addr_b.val;
    ref Profile p = ref _addr_p.val;

    while (true) {
        if (sectionTrigger(l) == memoryMapSection) {
            break;
        }
        {
            var (l, err) = b.ReadString('\n');

            if (err != null) {
                if (err != io.EOF) {
                    return error.As(err)!;
                }
                if (l == "") {
                    break;
                }
            }

        }
    }
    return error.As(p.ParseMemoryMap(b))!;
}

// ParseMemoryMap parses a memory map in the format of
// /proc/self/maps, and overrides the mappings in the current profile.
// It renumbers the samples and locations in the profile correspondingly.
private static error ParseMemoryMap(this ptr<Profile> _addr_p, io.Reader rd) {
    ref Profile p = ref _addr_p.val;

    var b = bufio.NewReader(rd);

    slice<@string> attrs = default;
    ptr<strings.Replacer> r;
    const @string delimiter = "=";

    while (true) {
        var (l, err) = b.ReadString('\n');
        if (err != null) {
            if (err != io.EOF) {
                return error.As(err)!;
            }
            if (l == "") {
                break;
            }
        }
        l = strings.TrimSpace(l);

        if (l == "") {
            continue;
        }
        if (r != null) {
            l = r.Replace(l);
        }
        var (m, err) = parseMappingEntry(l);
        if (err != null) {
            if (err == errUnrecognized) { 
                // Recognize assignments of the form: attr=value, and replace
                // $attr with value on subsequent mappings.
                {
                    var attr = strings.SplitN(l, delimiter, 2);

                    if (len(attr) == 2) {
                        attrs = append(attrs, "$" + strings.TrimSpace(attr[0]), strings.TrimSpace(attr[1]));
                        r = strings.NewReplacer(attrs);
                    } 
                    // Ignore any unrecognized entries

                } 
                // Ignore any unrecognized entries
                continue;
            }
            return error.As(err)!;
        }
        if (m == null || (m.File == "" && len(p.Mapping) != 0)) { 
            // In some cases the first entry may include the address range
            // but not the name of the file. It should be followed by
            // another entry with the name.
            continue;
        }
        if (len(p.Mapping) == 1 && p.Mapping[0].File == "") { 
            // Update the name if this is the entry following that empty one.
            p.Mapping[0].File = m.File;
            continue;
        }
        p.Mapping = append(p.Mapping, m);
    }
    p.remapLocationIDs();
    p.remapFunctionIDs();
    p.remapMappingIDs();
    return error.As(null!)!;
}

private static (ptr<Mapping>, error) parseMappingEntry(@string l) {
    ptr<Mapping> _p0 = default!;
    error _p0 = default!;

    ptr<Mapping> mapping = addr(new Mapping());
    error err = default!;
    {
        var me__prev1 = me;

        var me = procMapsRE.FindStringSubmatch(l);

        if (len(me) == 9) {
            if (!strings.Contains(me[3], "x")) { 
                // Skip non-executable entries.
                return (_addr_null!, error.As(null!)!);
            }
            mapping.Start, err = strconv.ParseUint(me[1], 16, 64);

            if (err != null) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }
            mapping.Limit, err = strconv.ParseUint(me[2], 16, 64);

            if (err != null) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }
            if (me[4] != "") {
                mapping.Offset, err = strconv.ParseUint(me[4], 16, 64);

                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
            }
            mapping.File = me[8];
            return (_addr_mapping!, error.As(null!)!);
        }
        me = me__prev1;

    }

    {
        var me__prev1 = me;

        me = briefMapsRE.FindStringSubmatch(l);

        if (len(me) == 6) {
            mapping.Start, err = strconv.ParseUint(me[1], 16, 64);

            if (err != null) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }
            mapping.Limit, err = strconv.ParseUint(me[2], 16, 64);

            if (err != null) {
                return (_addr_null!, error.As(errUnrecognized)!);
            }
            mapping.File = me[3];
            if (me[5] != "") {
                mapping.Offset, err = strconv.ParseUint(me[5], 16, 64);

                if (err != null) {
                    return (_addr_null!, error.As(errUnrecognized)!);
                }
            }
            return (_addr_mapping!, error.As(null!)!);
        }
        me = me__prev1;

    }

    return (_addr_null!, error.As(errUnrecognized)!);
}

private partial struct sectionType { // : nint
}

private static readonly sectionType unrecognizedSection = iota;
private static readonly var memoryMapSection = 0;

private static @string memoryMapTriggers = new slice<@string>(new @string[] { "--- Memory map: ---", "MAPPED_LIBRARIES:" });

private static sectionType sectionTrigger(@string line) {
    foreach (var (_, trigger) in memoryMapTriggers) {
        if (strings.Contains(line, trigger)) {
            return memoryMapSection;
        }
    }    return unrecognizedSection;
}

private static void addLegacyFrameInfo(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;


    if (isProfileType(_addr_p, heapzSampleTypes) || isProfileType(_addr_p, heapzInUseSampleTypes) || isProfileType(_addr_p, heapzAllocSampleTypes)) 
        (p.DropFrames, p.KeepFrames) = (allocRxStr, allocSkipRxStr);    else if (isProfileType(_addr_p, contentionzSampleTypes)) 
        (p.DropFrames, p.KeepFrames) = (lockRxStr, "");    else 
        (p.DropFrames, p.KeepFrames) = (cpuProfilerRxStr, "");    
}

private static @string heapzSampleTypes = new slice<@string>(new @string[] { "allocations", "size" }); // early Go pprof profiles
private static @string heapzInUseSampleTypes = new slice<@string>(new @string[] { "inuse_objects", "inuse_space" });
private static @string heapzAllocSampleTypes = new slice<@string>(new @string[] { "alloc_objects", "alloc_space" });
private static @string contentionzSampleTypes = new slice<@string>(new @string[] { "contentions", "delay" });

private static bool isProfileType(ptr<Profile> _addr_p, slice<@string> t) {
    ref Profile p = ref _addr_p.val;

    var st = p.SampleType;
    if (len(st) != len(t)) {
        return false;
    }
    foreach (var (i) in st) {
        if (st[i].Type != t[i]) {
            return false;
        }
    }    return true;
}

private static var allocRxStr = strings.Join(new slice<@string>(new @string[] { `calloc`, `cfree`, `malloc`, `free`, `memalign`, `do_memalign`, `(__)?posix_memalign`, `pvalloc`, `valloc`, `realloc`, `tcmalloc::.*`, `tc_calloc`, `tc_cfree`, `tc_malloc`, `tc_free`, `tc_memalign`, `tc_posix_memalign`, `tc_pvalloc`, `tc_valloc`, `tc_realloc`, `tc_new`, `tc_delete`, `tc_newarray`, `tc_deletearray`, `tc_new_nothrow`, `tc_newarray_nothrow`, `malloc_zone_malloc`, `malloc_zone_calloc`, `malloc_zone_valloc`, `malloc_zone_realloc`, `malloc_zone_memalign`, `malloc_zone_free`, `runtime\..*`, `BaseArena::.*`, `(::)?do_malloc_no_errno`, `(::)?do_malloc_pages`, `(::)?do_malloc`, `DoSampledAllocation`, `MallocedMemBlock::MallocedMemBlock`, `_M_allocate`, `__builtin_(vec_)?delete`, `__builtin_(vec_)?new`, `__gnu_cxx::new_allocator::allocate`, `__libc_malloc`, `__malloc_alloc_template::allocate`, `allocate`, `cpp_alloc`, `operator new(\[\])?`, `simple_alloc::allocate` }), "|");

private static var allocSkipRxStr = strings.Join(new slice<@string>(new @string[] { `runtime\.panic`, `runtime\.reflectcall`, `runtime\.call[0-9]*` }), "|");

private static var cpuProfilerRxStr = strings.Join(new slice<@string>(new @string[] { `ProfileData::Add`, `ProfileData::prof_handler`, `CpuProfiler::prof_handler`, `__pthread_sighandler`, `__restore` }), "|");

private static var lockRxStr = strings.Join(new slice<@string>(new @string[] { `RecordLockProfileData`, `(base::)?RecordLockProfileData.*`, `(base::)?SubmitMutexProfileData.*`, `(base::)?SubmitSpinLockProfileData.*`, `(Mutex::)?AwaitCommon.*`, `(Mutex::)?Unlock.*`, `(Mutex::)?UnlockSlow.*`, `(Mutex::)?ReaderUnlock.*`, `(MutexLock::)?~MutexLock.*`, `(SpinLock::)?Unlock.*`, `(SpinLock::)?SlowUnlock.*`, `(SpinLockHolder::)?~SpinLockHolder.*` }), "|");

} // end profile_package
