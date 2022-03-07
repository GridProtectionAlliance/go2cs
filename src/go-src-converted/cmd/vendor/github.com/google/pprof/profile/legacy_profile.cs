// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This file implements parsers to convert legacy profiles into the
// profile.proto format.

// package profile -- go2cs converted at 2022 March 06 23:23:59 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\profile\legacy_profile.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof;

public static partial class profile_package {

private static var countStartRE = regexp.MustCompile("\\A(\\S+) profile: total \\d+\\z");private static var countRE = regexp.MustCompile("\\A(\\d+) @(( 0x[0-9a-f]+)+)\\z");private static var heapHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] *@ *(heap[_a-z0-9]*)/?(\\d*)");private static var heapSampleRE = regexp.MustCompile("(-?\\d+): *(-?\\d+) *\\[ *(\\d+): *(\\d+) *] @([ x0-9a-f]*)");private static var contentionSampleRE = regexp.MustCompile("(\\d+) *(\\d+) @([ x0-9a-f]*)");private static var hexNumberRE = regexp.MustCompile("0x[0-9a-f]+");private static var growthHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] @ growthz?");private static var fragmentationHeaderRE = regexp.MustCompile("heap profile: *(\\d+): *(\\d+) *\\[ *(\\d+): *(\\d+) *\\] @ fragmentationz?");private static var threadzStartRE = regexp.MustCompile("--- threadz \\d+ ---");private static var threadStartRE = regexp.MustCompile("--- Thread ([[:xdigit:]]+) \\(name: (.*)/(\\d+)\\) stack: ---");private static @string spaceDigits = "\\s+[[:digit:]]+";private static @string hexPair = "\\s+[[:xdigit:]]+:[[:xdigit:]]+";private static @string oSpace = "\\s*";private static @string cHex = "(?:0x)?([[:xdigit:]]+)";private static @string cHexRange = "\\s*" + cHex + "[\\s-]?" + oSpace + cHex + ":?";private static @string cSpaceString = "(?:\\s+(\\S+))?";private static @string cSpaceHex = "(?:\\s+([[:xdigit:]]+))?";private static @string cSpaceAtOffset = "(?:\\s+\\(@([[:xdigit:]]+)\\))?";private static @string cPerm = "(?:\\s+([-rwxp]+))?";private static var procMapsRE = regexp.MustCompile("^" + cHexRange + cPerm + cSpaceHex + hexPair + spaceDigits + cSpaceString);private static var briefMapsRE = regexp.MustCompile("^" + cHexRange + cPerm + cSpaceString + cSpaceAtOffset + cSpaceHex);private static var logInfoRE = regexp.MustCompile("^[^\\[\\]]+:[0-9]+]\\s");

private static bool isSpaceOrComment(@string line) {
    var trimmed = strings.TrimSpace(line);
    return len(trimmed) == 0 || trimmed[0] == '#';
}

// parseGoCount parses a Go count profile (e.g., threadcreate or
// goroutine) and returns a new Profile.
private static (ptr<Profile>, error) parseGoCount(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var s = bufio.NewScanner(bytes.NewBuffer(b)); 
    // Skip comments at the beginning of the file.
    while (s.Scan() && isSpaceOrComment(s.Text()))     }
    {
        var err__prev1 = err;

        var err = s.Err();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    var m = countStartRE.FindStringSubmatch(s.Text());
    if (m == null) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    var profileType = m[1];
    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{Type:profileType,Unit:"count"},Period:1,SampleType:[]*ValueType{{Type:profileType,Unit:"count"}},));
    var locations = make_map<ulong, ptr<Location>>();
    while (s.Scan()) {
        var line = s.Text();
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
            // Adjust all frames by -1 to land on top of the call instruction.
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
    {
        var err__prev1 = err;

        err = s.Err();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    {
        var err__prev1 = err;

        err = parseAdditionalSections(_addr_s, p);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

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
 
    // Some profile handlers will incorrectly set regions for the main
    // executable if its section is remapped. Fix them through heuristics.

    if (len(p.Mapping) > 0) { 
        // Remove the initial mapping if named '/anon_hugepage' and has a
        // consecutive adjacent mapping.
        {
            var m__prev2 = m;

            var m = p.Mapping[0];

            if (strings.HasPrefix(m.File, "/anon_hugepage")) {
                if (len(p.Mapping) > 1 && m.Limit == p.Mapping[1].Start) {
                    p.Mapping = p.Mapping[(int)1..];
                }
            }

            m = m__prev2;

        }

    }
    if (len(p.Mapping) > 0) {
        const nuint expectedStart = 0x400000;

        {
            var m__prev2 = m;

            m = p.Mapping[0];

            if (m.Start - m.Offset == expectedStart) {
                m.Start = expectedStart;
                m.Offset = 0;
            }

            m = m__prev2;

        }

    }
    ptr<Mapping> fake;
nextLocation: 

    // Reset all mapping IDs.
    foreach (var (_, l) in p.Location) {
        var a = l.Address;
        if (l.Mapping != null || a == 0) {
            continue;
        }
        {
            var m__prev2 = m;

            foreach (var (_, __m) in p.Mapping) {
                m = __m;
                if (m.Start <= a && a < m.Limit) {
                    l.Mapping = m;
                    _continuenextLocation = true;
                    break;
                }

            } 
            // Work around legacy handlers failing to encode the first
            // part of mappings split into adjacent ranges.

            m = m__prev2;
        }

        {
            var m__prev2 = m;

            foreach (var (_, __m) in p.Mapping) {
                m = __m;
                if (m.Offset != 0 && m.Start - m.Offset <= a && a < m.Start) {
                    m.Start -= m.Offset;
                    m.Offset = 0;
                    l.Mapping = m;
                    _continuenextLocation = true;
                    break;
                }

            } 
            // If there is still no mapping, create a fake one.
            // This is important for the Go legacy handler, which produced
            // no mappings.

            m = m__prev2;
        }

        if (fake == null) {
            fake = addr(new Mapping(ID:1,Limit:^uint64(0),));
            p.Mapping = append(p.Mapping, fake);
        }
        l.Mapping = fake;

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
        if (tmp != null && n1 == 0 && n2 == 3 && n3 == 1 && n4 > 0 && n5 == 0) {
            b = tmp;
            return _addr_javaCPUProfile(b, int64(n4), parse)!;
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
    nint maxiter = 2; 
    // Allow one different sample for this many samples with the same
    // second-to-last frame.
    nint similarSamples = 32;
    var margin = len(p.Sample) / similarSamples;

    for (nint iter = 0; iter < maxiter; iter++) {
        var addr1 = make_map<ulong, nint>();
        {
            var s__prev2 = s;

            foreach (var (_, __s) in p.Sample) {
                s = __s;
                if (len(s.Location) > 1) {
                    var a = s.Location[1].Address;
                    addr1[a] = addr1[a] + 1;
                }
            }

            s = s__prev2;
        }

        foreach (var (id1, count) in addr1) {
            if (count >= len(p.Sample) - margin) { 
                // Found uninteresting frame, strip it out from all samples
                {
                    var s__prev3 = s;

                    foreach (var (_, __s) in p.Sample) {
                        s = __s;
                        if (len(s.Location) > 1 && s.Location[1].Address == id1) {
                            s.Location = append(s.Location[..(int)1], s.Location[(int)2..]);
                        }
                    }

                    s = s__prev3;
                }

                break;

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


    cleanupDuplicateLocations(p);
    return (_addr_p!, error.As(null!)!);

}

private static void cleanupDuplicateLocations(ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;
 
    // The profile handler may duplicate the leaf frame, because it gets
    // its address both from stack unwinding and from the signal
    // context. Detect this and delete the duplicate, which has been
    // adjusted by -1. The leaf address should not be adjusted as it is
    // not a call.
    foreach (var (_, s) in p.Sample) {
        if (len(s.Location) > 1 && s.Location[0].Address == s.Location[1].Address + 1) {
            s.Location = append(s.Location[..(int)1], s.Location[(int)2..]);
        }
    }
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

    var s = bufio.NewScanner(bytes.NewBuffer(b));
    if (!s.Scan()) {
        {
            var err__prev2 = err;

            var err = s.Err();

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

        return (_addr_null!, error.As(errUnrecognized)!);

    }
    p = addr(new Profile());

    @string sampling = "";
    var hasAlloc = false;

    var line = s.Text();
    p.PeriodType = addr(new ValueType(Type:"space",Unit:"bytes"));
    {
        var header = heapHeaderRE.FindStringSubmatch(line);

        if (header != null) {
            sampling, p.Period, hasAlloc, err = parseHeapHeader(line);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
        }        header = growthHeaderRE.FindStringSubmatch(line);


        else if (header != null) {
            p.Period = 1;
        }        header = fragmentationHeaderRE.FindStringSubmatch(line);


        else if (header != null) {
            p.Period = 1;
        }
        else
 {
            return (_addr_null!, error.As(errUnrecognized)!);
        }

    }


    if (hasAlloc) { 
        // Put alloc before inuse so that default pprof selection
        // will prefer inuse_space.
        p.SampleType = new slice<ptr<ValueType>>(new ptr<ValueType>[] { {Type:"alloc_objects",Unit:"count"}, {Type:"alloc_space",Unit:"bytes"}, {Type:"inuse_objects",Unit:"count"}, {Type:"inuse_space",Unit:"bytes"} });

    }
    else
 {
        p.SampleType = new slice<ptr<ValueType>>(new ptr<ValueType>[] { {Type:"objects",Unit:"count"}, {Type:"space",Unit:"bytes"} });
    }
    var locs = make_map<ulong, ptr<Location>>();
    while (s.Scan()) {
        line = strings.TrimSpace(s.Text());

        if (isSpaceOrComment(line)) {
            continue;
        }
        if (isMemoryMapSentinel(line)) {
            break;
        }
        var (value, blocksize, addrs, err) = parseHeapSample(line, p.Period, sampling, hasAlloc);
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
    {
        var err__prev1 = err;

        err = s.Err();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = parseAdditionalSections(_addr_s, _addr_p);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }

    return (_addr_p!, error.As(null!)!);

}

private static (@string, long, bool, error) parseHeapHeader(@string line) {
    @string sampling = default;
    long period = default;
    bool hasAlloc = default;
    error err = default!;

    var header = heapHeaderRE.FindStringSubmatch(line);
    if (header == null) {
        return ("", 0, false, error.As(errUnrecognized)!);
    }
    if (len(header[6]) > 0) {
        period, err = strconv.ParseInt(header[6], 10, 64);

        if (err != null) {
            return ("", 0, false, error.As(errUnrecognized)!);
        }
    }
    if ((header[3] != header[1] && header[3] != "0") || (header[4] != header[2] && header[4] != "0")) {
        hasAlloc = true;
    }
    switch (header[5]) {
        case "heapz_v2": 

        case "heap_v2": 
            return ("v2", period, hasAlloc, error.As(null!)!);
            break;
        case "heapprofile": 
            return ("", 1, hasAlloc, error.As(null!)!);
            break;
        case "heap": 
            return ("v2", period / 2, hasAlloc, error.As(null!)!);
            break;
        default: 
            return ("", 0, false, error.As(errUnrecognized)!);
            break;
    }

}

// parseHeapSample parses a single row from a heap profile into a new Sample.
private static (slice<long>, long, slice<ulong>, error) parseHeapSample(@string line, long rate, @string sampling, bool includeAlloc) {
    slice<long> value = default;
    long blocksize = default;
    slice<ulong> addrs = default;
    error err = default!;

    var sampleData = heapSampleRE.FindStringSubmatch(line);
    if (len(sampleData) != 6) {
        return (null, 0, null, error.As(fmt.Errorf("unexpected number of sample values: got %d, want 6", len(sampleData)))!);
    }
    Func<@string, @string, @string, error> addValues = (countString, sizeString, label) => {
        var (count, err) = strconv.ParseInt(countString, 10, 64);
        if (err != null) {
            return fmt.Errorf("malformed sample: %s: %v", line, err);
        }
        var (size, err) = strconv.ParseInt(sizeString, 10, 64);
        if (err != null) {
            return fmt.Errorf("malformed sample: %s: %v", line, err);
        }
        if (count == 0 && size != 0) {
            return fmt.Errorf("%s count was 0 but %s bytes was %d", label, label, size);
        }
        if (count != 0) {
            blocksize = size / count;
            if (sampling == "v2") {
                count, size = scaleHeapSample(count, size, rate);
            }
        }
        value = append(value, count, size);
        return null;

    };

    if (includeAlloc) {
        {
            var err__prev2 = err;

            var err = addValues(sampleData[3], sampleData[4], "allocation");

            if (err != null) {
                return (null, 0, null, error.As(err)!);
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        err = addValues(sampleData[1], sampleData[2], "inuse");

        if (err != null) {
            return (null, 0, null, error.As(err)!);
        }
        err = err__prev1;

    }


    addrs, err = parseHexAddresses(sampleData[5]);
    if (err != null) {
        return (null, 0, null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    return (value, blocksize, addrs, error.As(null!)!);

}

// parseHexAddresses extracts hex numbers from a string, attempts to convert
// each to an unsigned 64-bit number and returns the resulting numbers as a
// slice, or an error if the string contains hex numbers which are too large to
// handle (which means a malformed profile).
private static (slice<ulong>, error) parseHexAddresses(@string s) {
    slice<ulong> _p0 = default;
    error _p0 = default!;

    var hexStrings = hexNumberRE.FindAllString(s, -1);
    slice<ulong> addrs = default;
    foreach (var (_, s) in hexStrings) {
        {
            var (addr, err) = strconv.ParseUint(s, 0, 64);

            if (err == null) {
                addrs = append(addrs, addr);
            }
            else
 {
                return (null, error.As(fmt.Errorf("failed to parse as hex 64-bit number: %s", s))!);
            }

        }

    }    return (addrs, error.As(null!)!);

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
private static (ptr<Profile>, error) parseContention(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var s = bufio.NewScanner(bytes.NewBuffer(b));
    if (!s.Scan()) {
        {
            var err__prev2 = err;

            var err = s.Err();

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

        return (_addr_null!, error.As(errUnrecognized)!);

    }
    {
        var l = s.Text();


        if (strings.HasPrefix(l, "--- contentionz "))         else if (strings.HasPrefix(l, "--- mutex:"))         else if (strings.HasPrefix(l, "--- contention:"))         else 
            return (_addr_null!, error.As(errUnrecognized)!);

    }

    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{Type:"contentions",Unit:"count"},Period:1,SampleType:[]*ValueType{{Type:"contentions",Unit:"count"},{Type:"delay",Unit:"nanoseconds"},},));

    long cpuHz = default; 
    // Parse text of the form "attribute = value" before the samples.
    const @string delimiter = "=";

    while (s.Scan()) {
        var line = s.Text();
        line = strings.TrimSpace(line);

        if (isSpaceOrComment(line)) {
            continue;
        }
        if (strings.HasPrefix(line, "---")) {
            break;
        }
        var attr = strings.SplitN(line, delimiter, 2);
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
    {
        var err__prev1 = err;

        err = s.Err();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    var locs = make_map<ulong, ptr<Location>>();
    while (true) {
        line = strings.TrimSpace(s.Text());
        if (strings.HasPrefix(line, "---")) {
            break;
        }
        if (!isSpaceOrComment(line)) {
            var (value, addrs, err) = parseContentionSample(line, p.Period, cpuHz);
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
        if (!s.Scan()) {
            break;
        }
    }
    {
        var err__prev1 = err;

        err = s.Err();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    {
        var err__prev1 = err;

        err = parseAdditionalSections(_addr_s, p);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

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
        return (null, null, error.As(errUnrecognized)!);
    }
    var (v1, err) = strconv.ParseInt(sampleData[1], 10, 64);
    if (err != null) {
        return (null, null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    var (v2, err) = strconv.ParseInt(sampleData[2], 10, 64);
    if (err != null) {
        return (null, null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    if (period > 0) {
        if (cpuHz > 0) {
            var cpuGHz = float64(cpuHz) / 1e9F;
            v1 = int64(float64(v1) * float64(period) / cpuGHz);
        }
        v2 = v2 * period;

    }
    value = new slice<long>(new long[] { v2, v1 });
    addrs, err = parseHexAddresses(sampleData[3]);
    if (err != null) {
        return (null, null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
    }
    return (value, addrs, error.As(null!)!);

}

// parseThread parses a Threadz profile and returns a new Profile.
private static (ptr<Profile>, error) parseThread(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var s = bufio.NewScanner(bytes.NewBuffer(b)); 
    // Skip past comments and empty lines seeking a real header.
    while (s.Scan() && isSpaceOrComment(s.Text()))     }

    var line = s.Text();
    {
        var m = threadzStartRE.FindStringSubmatch(line);

        if (m != null) { 
            // Advance over initial comments until first stack trace.
            while (s.Scan()) {
                line = s.Text();

                if (isMemoryMapSentinel(line) || strings.HasPrefix(line, "-")) {
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
    while (!isMemoryMapSentinel(line)) {
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
        error err = default!;
        line, addrs, err = parseThreadSample(_addr_s);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (len(addrs) == 0) { 
            // We got a --same as previous threads--. Bump counters.
            if (len(p.Sample) > 0) {
                s = p.Sample[len(p.Sample) - 1];
                s.Value[0]++;
            }

            continue;

        }
        slice<ptr<Location>> sloc = default;
        foreach (var (i, addr) in addrs) { 
            // Addresses from stack traces point to the next instruction after
            // each call. Adjust by -1 to land somewhere on the actual call
            // (except for the leaf, which is not a call).
            if (i > 0) {
                addr--;
            }

            var loc = locs[addr];
            if (locs[addr] == null) {
                loc = addr(new Location(Address:addr,));
                p.Location = append(p.Location, loc);
                locs[addr] = loc;
            }

            sloc = append(sloc, loc);

        }        p.Sample = append(p.Sample, addr(new Sample(Value:[]int64{1},Location:sloc,)));

    }

    {
        error err__prev1 = err;

        err = parseAdditionalSections(_addr_s, p);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    cleanupDuplicateLocations(p);
    return (_addr_p!, error.As(null!)!);

}

// parseThreadSample parses a symbolized or unsymbolized stack trace.
// Returns the first line after the traceback, the sample (or nil if
// it hits a 'same-as-previous' marker) and an error.
private static (@string, slice<ulong>, error) parseThreadSample(ptr<bufio.Scanner> _addr_s) {
    @string nextl = default;
    slice<ulong> addrs = default;
    error err = default!;
    ref bufio.Scanner s = ref _addr_s.val;

    @string line = default;
    var sameAsPrevious = false;
    while (s.Scan()) {
        line = strings.TrimSpace(s.Text());
        if (line == "") {
            continue;
        }
        if (strings.HasPrefix(line, "---")) {
            break;
        }
        if (strings.Contains(line, "same as previous thread")) {
            sameAsPrevious = true;
            continue;
        }
        var (curAddrs, err) = parseHexAddresses(line);
        if (err != null) {
            return ("", null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
        }
        addrs = append(addrs, curAddrs);

    }
    {
        var err = s.Err();

        if (err != null) {
            return ("", null, error.As(err)!);
        }
    }

    if (sameAsPrevious) {
        return (line, null, error.As(null!)!);
    }
    return (line, addrs, error.As(null!)!);

}

// parseAdditionalSections parses any additional sections in the
// profile, ignoring any unrecognized sections.
private static error parseAdditionalSections(ptr<bufio.Scanner> _addr_s, ptr<Profile> _addr_p) {
    ref bufio.Scanner s = ref _addr_s.val;
    ref Profile p = ref _addr_p.val;

    while (!isMemoryMapSentinel(s.Text()) && s.Scan())     }
    {
        var err = s.Err();

        if (err != null) {
            return error.As(err)!;
        }
    }

    return error.As(p.ParseMemoryMapFromScanner(s))!;

}

// ParseProcMaps parses a memory map in the format of /proc/self/maps.
// ParseMemoryMap should be called after setting on a profile to
// associate locations to the corresponding mapping based on their
// address.
public static (slice<ptr<Mapping>>, error) ParseProcMaps(io.Reader rd) {
    slice<ptr<Mapping>> _p0 = default;
    error _p0 = default!;

    var s = bufio.NewScanner(rd);
    return parseProcMapsFromScanner(_addr_s);
}

private static (slice<ptr<Mapping>>, error) parseProcMapsFromScanner(ptr<bufio.Scanner> _addr_s) {
    slice<ptr<Mapping>> _p0 = default;
    error _p0 = default!;
    ref bufio.Scanner s = ref _addr_s.val;

    slice<ptr<Mapping>> mapping = default;

    slice<@string> attrs = default;
    const @string delimiter = "=";

    var r = strings.NewReplacer();
    while (s.Scan()) {
        var line = r.Replace(removeLoggingInfo(s.Text()));
        var (m, err) = parseMappingEntry(line);
        if (err != null) {
            if (err == errUnrecognized) { 
                // Recognize assignments of the form: attr=value, and replace
                // $attr with value on subsequent mappings.
                {
                    var attr = strings.SplitN(line, delimiter, 2);

                    if (len(attr) == 2) {
                        attrs = append(attrs, "$" + strings.TrimSpace(attr[0]), strings.TrimSpace(attr[1]));
                        r = strings.NewReplacer(attrs);
                    } 
                    // Ignore any unrecognized entries

                } 
                // Ignore any unrecognized entries
                continue;

            }

            return (null, error.As(err)!);

        }
        if (m == null) {
            continue;
        }
        mapping = append(mapping, m);

    }
    {
        var err = s.Err();

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return (mapping, error.As(null!)!);

}

// removeLoggingInfo detects and removes log prefix entries generated
// by the glog package. If no logging prefix is detected, the string
// is returned unmodified.
private static @string removeLoggingInfo(@string line) {
    {
        var match = logInfoRE.FindStringIndex(line);

        if (match != null) {
            return line[(int)match[1]..];
        }
    }

    return line;

}

// ParseMemoryMap parses a memory map in the format of
// /proc/self/maps, and overrides the mappings in the current profile.
// It renumbers the samples and locations in the profile correspondingly.
private static error ParseMemoryMap(this ptr<Profile> _addr_p, io.Reader rd) {
    ref Profile p = ref _addr_p.val;

    return error.As(p.ParseMemoryMapFromScanner(bufio.NewScanner(rd)))!;
}

// ParseMemoryMapFromScanner parses a memory map in the format of
// /proc/self/maps or a variety of legacy format, and overrides the
// mappings in the current profile.  It renumbers the samples and
// locations in the profile correspondingly.
private static error ParseMemoryMapFromScanner(this ptr<Profile> _addr_p, ptr<bufio.Scanner> _addr_s) {
    ref Profile p = ref _addr_p.val;
    ref bufio.Scanner s = ref _addr_s.val;

    var (mapping, err) = parseProcMapsFromScanner(_addr_s);
    if (err != null) {
        return error.As(err)!;
    }
    p.Mapping = append(p.Mapping, mapping);
    p.massageMappings();
    p.remapLocationIDs();
    p.remapFunctionIDs();
    p.remapMappingIDs();
    return error.As(null!)!;

}

private static (ptr<Mapping>, error) parseMappingEntry(@string l) {
    ptr<Mapping> _p0 = default!;
    error _p0 = default!;

    @string start = default;    @string end = default;    @string perm = default;    @string file = default;    @string offset = default;    @string buildID = default;

    {
        var me__prev1 = me;

        var me = procMapsRE.FindStringSubmatch(l);

        if (len(me) == 6) {
            (start, end, perm, offset, file) = (me[1], me[2], me[3], me[4], me[5]);
        }        {
            var me__prev2 = me;

            me = briefMapsRE.FindStringSubmatch(l);


            else if (len(me) == 7) {
                (start, end, perm, file, offset, buildID) = (me[1], me[2], me[3], me[4], me[5], me[6]);
            }
            else
 {
                return (_addr_null!, error.As(errUnrecognized)!);
            }

            me = me__prev2;

        }



        me = me__prev1;

    }


    error err = default!;
    ptr<Mapping> mapping = addr(new Mapping(File:file,BuildID:buildID,));
    if (perm != "" && !strings.Contains(perm, "x")) { 
        // Skip non-executable entries.
        return (_addr_null!, error.As(null!)!);

    }
    mapping.Start, err = strconv.ParseUint(start, 16, 64);

    if (err != null) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    mapping.Limit, err = strconv.ParseUint(end, 16, 64);

    if (err != null) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    if (offset != "") {
        mapping.Offset, err = strconv.ParseUint(offset, 16, 64);

        if (err != null) {
            return (_addr_null!, error.As(errUnrecognized)!);
        }
    }
    return (_addr_mapping!, error.As(null!)!);

}

private static @string memoryMapSentinels = new slice<@string>(new @string[] { "--- Memory map: ---", "MAPPED_LIBRARIES:" });

// isMemoryMapSentinel returns true if the string contains one of the
// known sentinels for memory map information.
private static bool isMemoryMapSentinel(@string line) {
    foreach (var (_, s) in memoryMapSentinels) {
        if (strings.Contains(line, s)) {
            return true;
        }
    }    return false;

}

private static void addLegacyFrameInfo(this ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;


    if (isProfileType(_addr_p, heapzSampleTypes)) 
        (p.DropFrames, p.KeepFrames) = (allocRxStr, allocSkipRxStr);    else if (isProfileType(_addr_p, contentionzSampleTypes)) 
        (p.DropFrames, p.KeepFrames) = (lockRxStr, "");    else 
        (p.DropFrames, p.KeepFrames) = (cpuProfilerRxStr, "");    
}

private static slice<@string> heapzSampleTypes = new slice<slice<@string>>(new slice<@string>[] { {"allocations","size"}, {"objects","space"}, {"inuse_objects","inuse_space"}, {"alloc_objects","alloc_space"}, {"alloc_objects","alloc_space","inuse_objects","inuse_space"} });
private static slice<@string> contentionzSampleTypes = new slice<slice<@string>>(new slice<@string>[] { {"contentions","delay"} });

private static bool isProfileType(ptr<Profile> _addr_p, slice<slice<@string>> types) {
    ref Profile p = ref _addr_p.val;

    var st = p.SampleType;
nextType:
    foreach (var (_, t) in types) {
        if (len(st) != len(t)) {
            continue;
        }
        foreach (var (i) in st) {
            if (st[i].Type != t[i]) {
                _continuenextType = true;
                break;
            }

        }        return true;

    }    return false;

}

private static var allocRxStr = strings.Join(new slice<@string>(new @string[] { `calloc`, `cfree`, `malloc`, `free`, `memalign`, `do_memalign`, `(__)?posix_memalign`, `pvalloc`, `valloc`, `realloc`, `tcmalloc::.*`, `tc_calloc`, `tc_cfree`, `tc_malloc`, `tc_free`, `tc_memalign`, `tc_posix_memalign`, `tc_pvalloc`, `tc_valloc`, `tc_realloc`, `tc_new`, `tc_delete`, `tc_newarray`, `tc_deletearray`, `tc_new_nothrow`, `tc_newarray_nothrow`, `malloc_zone_malloc`, `malloc_zone_calloc`, `malloc_zone_valloc`, `malloc_zone_realloc`, `malloc_zone_memalign`, `malloc_zone_free`, `runtime\..*`, `BaseArena::.*`, `(::)?do_malloc_no_errno`, `(::)?do_malloc_pages`, `(::)?do_malloc`, `DoSampledAllocation`, `MallocedMemBlock::MallocedMemBlock`, `_M_allocate`, `__builtin_(vec_)?delete`, `__builtin_(vec_)?new`, `__gnu_cxx::new_allocator::allocate`, `__libc_malloc`, `__malloc_alloc_template::allocate`, `allocate`, `cpp_alloc`, `operator new(\[\])?`, `simple_alloc::allocate` }), "|");

private static var allocSkipRxStr = strings.Join(new slice<@string>(new @string[] { `runtime\.panic`, `runtime\.reflectcall`, `runtime\.call[0-9]*` }), "|");

private static var cpuProfilerRxStr = strings.Join(new slice<@string>(new @string[] { `ProfileData::Add`, `ProfileData::prof_handler`, `CpuProfiler::prof_handler`, `__pthread_sighandler`, `__restore` }), "|");

private static var lockRxStr = strings.Join(new slice<@string>(new @string[] { `RecordLockProfileData`, `(base::)?RecordLockProfileData.*`, `(base::)?SubmitMutexProfileData.*`, `(base::)?SubmitSpinLockProfileData.*`, `(base::Mutex::)?AwaitCommon.*`, `(base::Mutex::)?Unlock.*`, `(base::Mutex::)?UnlockSlow.*`, `(base::Mutex::)?ReaderUnlock.*`, `(base::MutexLock::)?~MutexLock.*`, `(Mutex::)?AwaitCommon.*`, `(Mutex::)?Unlock.*`, `(Mutex::)?UnlockSlow.*`, `(Mutex::)?ReaderUnlock.*`, `(MutexLock::)?~MutexLock.*`, `(SpinLock::)?Unlock.*`, `(SpinLock::)?SlowUnlock.*`, `(SpinLockHolder::)?~SpinLockHolder.*` }), "|");

} // end profile_package
