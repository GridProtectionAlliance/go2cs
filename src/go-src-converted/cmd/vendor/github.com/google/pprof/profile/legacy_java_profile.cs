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

// This file implements parsers to convert java legacy profiles into
// the profile.proto format.

// package profile -- go2cs converted at 2022 March 13 06:37:01 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\profile\legacy_java_profile.go
namespace go.cmd.vendor.github.com.google.pprof;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;
using System;

public static partial class profile_package {

private static var attributeRx = regexp.MustCompile("([\\w ]+)=([\\w ]+)");private static var javaSampleRx = regexp.MustCompile(" *(\\d+) +(\\d+) +@ +([ x0-9a-f]*)");private static var javaLocationRx = regexp.MustCompile("^\\s*0x([[:xdigit:]]+)\\s+(.*)\\s*$");private static var javaLocationFileLineRx = regexp.MustCompile("^(.*)\\s+\\((.+):(-?[[:digit:]]+)\\)$");private static var javaLocationPathRx = regexp.MustCompile("^(.*)\\s+\\((.*)\\)$");

// javaCPUProfile returns a new Profile from profilez data.
// b is the profile bytes after the header, period is the profiling
// period, and parse is a function to parse 8-byte chunks from the
// profile in its native endianness.
private static (ptr<Profile>, error) javaCPUProfile(slice<byte> b, long period, Func<slice<byte>, (ulong, slice<byte>)> parse) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    ptr<Profile> p = addr(new Profile(Period:period*1000,PeriodType:&ValueType{Type:"cpu",Unit:"nanoseconds"},SampleType:[]*ValueType{{Type:"samples",Unit:"count"},{Type:"cpu",Unit:"nanoseconds"}},));
    error err = default!;
    map<ulong, ptr<Location>> locs = default;
    b, locs, err = parseCPUSamples(b, parse, false, p);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    err = error.As(parseJavaLocations(b, locs, p))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    err = error.As(p.Aggregate(true, true, true, true, false))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// parseJavaProfile returns a new profile from heapz or contentionz
// data. b is the profile bytes after the header.
private static (ptr<Profile>, error) parseJavaProfile(slice<byte> b) {
    ptr<Profile> _p0 = default!;
    error _p0 = default!;

    var h = bytes.SplitAfterN(b, (slice<byte>)"\n", 2);
    if (len(h) < 2) {
        return (_addr_null!, error.As(errUnrecognized)!);
    }
    ptr<Profile> p = addr(new Profile(PeriodType:&ValueType{},));
    var header = string(bytes.TrimSpace(h[0]));

    error err = default!;
    @string pType = default;
    switch (header) {
        case "--- heapz 1 ---": 
            pType = "heap";
            break;
        case "--- contentionz 1 ---": 
            pType = "contention";
            break;
        default: 
            return (_addr_null!, error.As(errUnrecognized)!);
            break;
    }

    b, err = parseJavaHeader(pType, h[1], p);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    map<ulong, ptr<Location>> locs = default;
    b, locs, err = parseJavaSamples(pType, b, p);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    err = error.As(parseJavaLocations(b, locs, p))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    err = error.As(p.Aggregate(true, true, true, true, false))!;

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_p!, error.As(null!)!);
}

// parseJavaHeader parses the attribute section on a java profile and
// populates a profile. Returns the remainder of the buffer after all
// attributes.
private static (slice<byte>, error) parseJavaHeader(@string pType, slice<byte> b, ptr<Profile> _addr_p) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Profile p = ref _addr_p.val;

    var nextNewLine = bytes.IndexByte(b, byte('\n'));
    while (nextNewLine != -1) {
        var line = string(bytes.TrimSpace(b[(int)0..(int)nextNewLine]));
        if (line != "") {
            var h = attributeRx.FindStringSubmatch(line);
            if (h == null) { 
                // Not a valid attribute, exit.
                return (b, error.As(null!)!);
            }
            var attribute = strings.TrimSpace(h[1]);
            var value = strings.TrimSpace(h[2]);
            error err = default!;
            switch (pType + "/" + attribute) {
                case "heap/format": 

                case "cpu/format": 

                case "contention/format": 
                    if (value != "java") {
                        return (null, error.As(errUnrecognized)!);
                    }
                    break;
                case "heap/resolution": 
                    p.SampleType = new slice<ptr<ValueType>>(new ptr<ValueType>[] { {Type:"inuse_objects",Unit:"count"}, {Type:"inuse_space",Unit:value} });
                    break;
                case "contention/resolution": 
                    p.SampleType = new slice<ptr<ValueType>>(new ptr<ValueType>[] { {Type:"contentions",Unit:"count"}, {Type:"delay",Unit:value} });
                    break;
                case "contention/sampling period": 
                    p.PeriodType = addr(new ValueType(Type:"contentions",Unit:"count",));
                    p.Period, err = strconv.ParseInt(value, 0, 64);

                    if (err != null) {
                        return (null, error.As(fmt.Errorf("failed to parse attribute %s: %v", line, err))!);
                    }
                    break;
                case "contention/ms since reset": 
                    var (millis, err) = strconv.ParseInt(value, 0, 64);
                    if (err != null) {
                        return (null, error.As(fmt.Errorf("failed to parse attribute %s: %v", line, err))!);
                    }
                    p.DurationNanos = millis * 1000 * 1000;
                    break;
                default: 
                    return (null, error.As(errUnrecognized)!);
                    break;
            }
        }
        b = b[(int)nextNewLine + 1..];
        nextNewLine = bytes.IndexByte(b, byte('\n'));
    }
    return (b, error.As(null!)!);
}

// parseJavaSamples parses the samples from a java profile and
// populates the Samples in a profile. Returns the remainder of the
// buffer after the samples.
private static (slice<byte>, map<ulong, ptr<Location>>, error) parseJavaSamples(@string pType, slice<byte> b, ptr<Profile> _addr_p) {
    slice<byte> _p0 = default;
    map<ulong, ptr<Location>> _p0 = default;
    error _p0 = default!;
    ref Profile p = ref _addr_p.val;

    var nextNewLine = bytes.IndexByte(b, byte('\n'));
    var locs = make_map<ulong, ptr<Location>>();
    while (nextNewLine != -1) {
        var line = string(bytes.TrimSpace(b[(int)0..(int)nextNewLine]));
        if (line != "") {
            var sample = javaSampleRx.FindStringSubmatch(line);
            if (sample == null) { 
                // Not a valid sample, exit.
                return (b, locs, error.As(null!)!);
            } 

            // Java profiles have data/fields inverted compared to other
            // profile types.
            error err = default!;
            var value1 = sample[2];
            var value2 = sample[1];
            var value3 = sample[3];
            var (addrs, err) = parseHexAddresses(value3);
            if (err != null) {
                return (null, null, error.As(fmt.Errorf("malformed sample: %s: %v", line, err))!);
            }
            slice<ptr<Location>> sloc = default;
            foreach (var (_, addr) in addrs) {
                var loc = locs[addr];
                if (locs[addr] == null) {
                    loc = addr(new Location(Address:addr,));
                    p.Location = append(p.Location, loc);
                    locs[addr] = loc;
                }
                sloc = append(sloc, loc);
            }
            ptr<Sample> s = addr(new Sample(Value:make([]int64,2),Location:sloc,));

            s.Value[0], err = strconv.ParseInt(value1, 0, 64);

            if (err != null) {
                return (null, null, error.As(fmt.Errorf("parsing sample %s: %v", line, err))!);
            }
            s.Value[1], err = strconv.ParseInt(value2, 0, 64);

            if (err != null) {
                return (null, null, error.As(fmt.Errorf("parsing sample %s: %v", line, err))!);
            }
            switch (pType) {
                case "heap": 
                                   const nint javaHeapzSamplingRate = 524288; // 512K
                    // 512K
                                   if (s.Value[0] == 0) {
                                       return (null, null, error.As(fmt.Errorf("parsing sample %s: second value must be non-zero", line))!);
                                   }
                                   s.NumLabel = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<long>>{"bytes":{s.Value[1]/s.Value[0]}};
                                   s.Value[0], s.Value[1] = scaleHeapSample(s.Value[0], s.Value[1], javaHeapzSamplingRate);
                    break;
                case "contention": 
                    {
                        var period = p.Period;

                        if (period != 0) {
                            s.Value[0] = s.Value[0] * p.Period;
                            s.Value[1] = s.Value[1] * p.Period;
                        }

                    }
                    break;
            }
            p.Sample = append(p.Sample, s);
        }
        b = b[(int)nextNewLine + 1..];
        nextNewLine = bytes.IndexByte(b, byte('\n'));
    }
    return (b, locs, error.As(null!)!);
}

// parseJavaLocations parses the location information in a java
// profile and populates the Locations in a profile. It uses the
// location addresses from the profile as both the ID of each
// location.
private static error parseJavaLocations(slice<byte> b, map<ulong, ptr<Location>> locs, ptr<Profile> _addr_p) {
    ref Profile p = ref _addr_p.val;

    var r = bytes.NewBuffer(b);
    var fns = make_map<@string, ptr<Function>>();
    while (true) {
        var (line, err) = r.ReadString('\n');
        if (err != null) {
            if (err != io.EOF) {
                return error.As(err)!;
            }
            if (line == "") {
                break;
            }
        }
        line = strings.TrimSpace(line);

        if (line == "") {
            continue;
        }
        var jloc = javaLocationRx.FindStringSubmatch(line);
        if (len(jloc) != 3) {
            continue;
        }
        var (addr, err) = strconv.ParseUint(jloc[1], 16, 64);
        if (err != null) {
            return error.As(fmt.Errorf("parsing sample %s: %v", line, err))!;
        }
        var loc = locs[addr];
        if (loc == null) { 
            // Unused/unseen
            continue;
        }
        @string lineFunc = default;        @string lineFile = default;

        long lineNo = default;

        {
            var fileLine = javaLocationFileLineRx.FindStringSubmatch(jloc[2]);

            if (len(fileLine) == 4) { 
                // Found a line of the form: "function (file:line)"
                (lineFunc, lineFile) = (fileLine[1], fileLine[2]);                {
                    var (n, err) = strconv.ParseInt(fileLine[3], 10, 64);

                    if (err == null && n > 0) {
                        lineNo = n;
                    }

                }
            }            {
                var filePath = javaLocationPathRx.FindStringSubmatch(jloc[2]);


                else if (len(filePath) == 3) { 
                    // If there's not a file:line, it's a shared library path.
                    // The path isn't interesting, so just give the .so.
                    (lineFunc, lineFile) = (filePath[1], filepath.Base(filePath[2]));
                }
                else if (strings.Contains(jloc[2], "generated stub/JIT")) {
                    lineFunc = "STUB";
                }
                else
 { 
                    // Treat whole line as the function name. This is used by the
                    // java agent for internal states such as "GC" or "VM".
                    lineFunc = jloc[2];
                }

            }

        }
        var fn = fns[lineFunc];

        if (fn == null) {
            fn = addr(new Function(Name:lineFunc,SystemName:lineFunc,Filename:lineFile,));
            fns[lineFunc] = fn;
            p.Function = append(p.Function, fn);
        }
        loc.Line = new slice<Line>(new Line[] { {Function:fn,Line:lineNo,} });
        loc.Address = 0;
    }

    p.remapLocationIDs();
    p.remapFunctionIDs();
    p.remapMappingIDs();

    return error.As(null!)!;
}

} // end profile_package
