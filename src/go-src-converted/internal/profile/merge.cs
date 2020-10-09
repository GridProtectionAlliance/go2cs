// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package profile -- go2cs converted at 2020 October 09 04:59:10 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Go\src\internal\profile\merge.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class profile_package
    {
        // Merge merges all the profiles in profs into a single Profile.
        // Returns a new profile independent of the input profiles. The merged
        // profile is compacted to eliminate unused samples, locations,
        // functions and mappings. Profiles must have identical profile sample
        // and period types or the merge will fail. profile.Period of the
        // resulting profile will be the maximum of all profiles, and
        // profile.TimeNanos will be the earliest nonzero one.
        public static (ptr<Profile>, error) Merge(slice<ptr<Profile>> srcs)
        {
            ptr<Profile> _p0 = default!;
            error _p0 = default!;

            if (len(srcs) == 0L)
            {
                return (_addr_null!, error.As(fmt.Errorf("no profiles to merge"))!);
            }
            var (p, err) = combineHeaders(srcs);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }
            ptr<profileMerger> pm = addr(new profileMerger(p:p,samples:make(map[sampleKey]*Sample,len(srcs[0].Sample)),locations:make(map[locationKey]*Location,len(srcs[0].Location)),functions:make(map[functionKey]*Function,len(srcs[0].Function)),mappings:make(map[mappingKey]*Mapping,len(srcs[0].Mapping)),));

            foreach (var (_, src) in srcs)
            { 
                // Clear the profile-specific hash tables
                pm.locationsByID = make_map<ulong, ptr<Location>>(len(src.Location));
                pm.functionsByID = make_map<ulong, ptr<Function>>(len(src.Function));
                pm.mappingsByID = make_map<ulong, mapInfo>(len(src.Mapping));

                if (len(pm.mappings) == 0L && len(src.Mapping) > 0L)
                { 
                    // The Mapping list has the property that the first mapping
                    // represents the main binary. Take the first Mapping we see,
                    // otherwise the operations below will add mappings in an
                    // arbitrary order.
                    pm.mapMapping(src.Mapping[0L]);

                }
                {
                    var s__prev2 = s;

                    foreach (var (_, __s) in src.Sample)
                    {
                        s = __s;
                        if (!isZeroSample(_addr_s))
                        {
                            pm.mapSample(s);
                        }
                    }
                    s = s__prev2;
                }
            }            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    if (isZeroSample(_addr_s))
                    { 
                        // If there are any zero samples, re-merge the profile to GC
                        // them.
                        return _addr_Merge(new slice<ptr<Profile>>(new ptr<Profile>[] { p }))!;

                    }
                }
                s = s__prev1;
            }

            return (_addr_p!, error.As(null!)!);

        }

        // Normalize normalizes the source profile by multiplying each value in profile by the
        // ratio of the sum of the base profile's values of that sample type to the sum of the
        // source profile's value of that sample type.
        private static error Normalize(this ptr<Profile> _addr_p, ptr<Profile> _addr_pb)
        {
            ref Profile p = ref _addr_p.val;
            ref Profile pb = ref _addr_pb.val;

            {
                var err = p.compatible(pb);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var baseVals = make_slice<long>(len(p.SampleType));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in pb.Sample)
                {
                    s = __s;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in s.Value)
                        {
                            i = __i;
                            v = __v;
                            baseVals[i] += v;
                        }

                        i = i__prev2;
                        v = v__prev2;
                    }
                }

                s = s__prev1;
            }

            var srcVals = make_slice<long>(len(p.SampleType));
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in s.Value)
                        {
                            i = __i;
                            v = __v;
                            srcVals[i] += v;
                        }

                        i = i__prev2;
                        v = v__prev2;
                    }
                }

                s = s__prev1;
            }

            var normScale = make_slice<double>(len(baseVals));
            {
                var i__prev1 = i;

                foreach (var (__i) in baseVals)
                {
                    i = __i;
                    if (srcVals[i] == 0L)
                    {
                        normScale[i] = 0.0F;
                    }
                    else
                    {
                        normScale[i] = float64(baseVals[i]) / float64(srcVals[i]);
                    }

                }

                i = i__prev1;
            }

            p.ScaleN(normScale);
            return error.As(null!)!;

        }

        private static bool isZeroSample(ptr<Sample> _addr_s)
        {
            ref Sample s = ref _addr_s.val;

            foreach (var (_, v) in s.Value)
            {
                if (v != 0L)
                {
                    return false;
                }

            }
            return true;

        }

        private partial struct profileMerger
        {
            public ptr<Profile> p; // Memoization tables within a profile.
            public map<ulong, ptr<Location>> locationsByID;
            public map<ulong, ptr<Function>> functionsByID;
            public map<ulong, mapInfo> mappingsByID; // Memoization tables for profile entities.
            public map<sampleKey, ptr<Sample>> samples;
            public map<locationKey, ptr<Location>> locations;
            public map<functionKey, ptr<Function>> functions;
            public map<mappingKey, ptr<Mapping>> mappings;
        }

        private partial struct mapInfo
        {
            public ptr<Mapping> m;
            public long offset;
        }

        private static ptr<Sample> mapSample(this ptr<profileMerger> _addr_pm, ptr<Sample> _addr_src)
        {
            ref profileMerger pm = ref _addr_pm.val;
            ref Sample src = ref _addr_src.val;

            ptr<Sample> s = addr(new Sample(Location:make([]*Location,len(src.Location)),Value:make([]int64,len(src.Value)),Label:make(map[string][]string,len(src.Label)),NumLabel:make(map[string][]int64,len(src.NumLabel)),NumUnit:make(map[string][]string,len(src.NumLabel)),));
            {
                var i__prev1 = i;

                foreach (var (__i, __l) in src.Location)
                {
                    i = __i;
                    l = __l;
                    s.Location[i] = pm.mapLocation(l);
                }

                i = i__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in src.Label)
                {
                    k = __k;
                    v = __v;
                    var vv = make_slice<@string>(len(v));
                    copy(vv, v);
                    s.Label[k] = vv;
                }

                k = k__prev1;
                v = v__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in src.NumLabel)
                {
                    k = __k;
                    v = __v;
                    var u = src.NumUnit[k];
                    vv = make_slice<long>(len(v));
                    var uu = make_slice<@string>(len(u));
                    copy(vv, v);
                    copy(uu, u);
                    s.NumLabel[k] = vv;
                    s.NumUnit[k] = uu;
                } 
                // Check memoization table. Must be done on the remapped location to
                // account for the remapped mapping. Add current values to the
                // existing sample.

                k = k__prev1;
                v = v__prev1;
            }

            var k = s.key();
            {
                var (ss, ok) = pm.samples[k];

                if (ok)
                {
                    {
                        var i__prev1 = i;
                        var v__prev1 = v;

                        foreach (var (__i, __v) in src.Value)
                        {
                            i = __i;
                            v = __v;
                            ss.Value[i] += v;
                        }

                        i = i__prev1;
                        v = v__prev1;
                    }

                    return _addr_ss!;

                }

            }

            copy(s.Value, src.Value);
            pm.samples[k] = s;
            pm.p.Sample = append(pm.p.Sample, s);
            return _addr_s!;

        }

        // key generates sampleKey to be used as a key for maps.
        private static sampleKey key(this ptr<Sample> _addr_sample)
        {
            ref Sample sample = ref _addr_sample.val;

            var ids = make_slice<@string>(len(sample.Location));
            foreach (var (i, l) in sample.Location)
            {
                ids[i] = strconv.FormatUint(l.ID, 16L);
            }
            var labels = make_slice<@string>(0L, len(sample.Label));
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in sample.Label)
                {
                    k = __k;
                    v = __v;
                    labels = append(labels, fmt.Sprintf("%q%q", k, v));
                }

                k = k__prev1;
                v = v__prev1;
            }

            sort.Strings(labels);

            var numlabels = make_slice<@string>(0L, len(sample.NumLabel));
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in sample.NumLabel)
                {
                    k = __k;
                    v = __v;
                    numlabels = append(numlabels, fmt.Sprintf("%q%x%x", k, v, sample.NumUnit[k]));
                }

                k = k__prev1;
                v = v__prev1;
            }

            sort.Strings(numlabels);

            return new sampleKey(strings.Join(ids,"|"),strings.Join(labels,""),strings.Join(numlabels,""),);

        }

        private partial struct sampleKey
        {
            public @string locations;
            public @string labels;
            public @string numlabels;
        }

        private static ptr<Location> mapLocation(this ptr<profileMerger> _addr_pm, ptr<Location> _addr_src)
        {
            ref profileMerger pm = ref _addr_pm.val;
            ref Location src = ref _addr_src.val;

            if (src == null)
            {
                return _addr_null!;
            }

            {
                var l__prev1 = l;

                var (l, ok) = pm.locationsByID[src.ID];

                if (ok)
                {
                    pm.locationsByID[src.ID] = l;
                    return _addr_l!;
                }

                l = l__prev1;

            }


            var mi = pm.mapMapping(src.Mapping);
            ptr<Location> l = addr(new Location(ID:uint64(len(pm.p.Location)+1),Mapping:mi.m,Address:uint64(int64(src.Address)+mi.offset),Line:make([]Line,len(src.Line)),IsFolded:src.IsFolded,));
            foreach (var (i, ln) in src.Line)
            {
                l.Line[i] = pm.mapLine(ln);
            } 
            // Check memoization table. Must be done on the remapped location to
            // account for the remapped mapping ID.
            var k = l.key();
            {
                var (ll, ok) = pm.locations[k];

                if (ok)
                {
                    pm.locationsByID[src.ID] = ll;
                    return _addr_ll!;
                }

            }

            pm.locationsByID[src.ID] = l;
            pm.locations[k] = l;
            pm.p.Location = append(pm.p.Location, l);
            return _addr_l!;

        }

        // key generates locationKey to be used as a key for maps.
        private static locationKey key(this ptr<Location> _addr_l)
        {
            ref Location l = ref _addr_l.val;

            locationKey key = new locationKey(addr:l.Address,isFolded:l.IsFolded,);
            if (l.Mapping != null)
            { 
                // Normalizes address to handle address space randomization.
                key.addr -= l.Mapping.Start;
                key.mappingID = l.Mapping.ID;

            }

            var lines = make_slice<@string>(len(l.Line) * 2L);
            foreach (var (i, line) in l.Line)
            {
                if (line.Function != null)
                {
                    lines[i * 2L] = strconv.FormatUint(line.Function.ID, 16L);
                }

                lines[i * 2L + 1L] = strconv.FormatInt(line.Line, 16L);

            }
            key.lines = strings.Join(lines, "|");
            return key;

        }

        private partial struct locationKey
        {
            public ulong addr;
            public ulong mappingID;
            public @string lines;
            public bool isFolded;
        }

        private static mapInfo mapMapping(this ptr<profileMerger> _addr_pm, ptr<Mapping> _addr_src)
        {
            ref profileMerger pm = ref _addr_pm.val;
            ref Mapping src = ref _addr_src.val;

            if (src == null)
            {
                return new mapInfo();
            }

            {
                var mi__prev1 = mi;

                var (mi, ok) = pm.mappingsByID[src.ID];

                if (ok)
                {
                    return mi;
                } 

                // Check memoization tables.

                mi = mi__prev1;

            } 

            // Check memoization tables.
            var mk = src.key();
            {
                var m__prev1 = m;

                var (m, ok) = pm.mappings[mk];

                if (ok)
                {
                    mapInfo mi = new mapInfo(m,int64(m.Start)-int64(src.Start));
                    pm.mappingsByID[src.ID] = mi;
                    return mi;
                }

                m = m__prev1;

            }

            ptr<Mapping> m = addr(new Mapping(ID:uint64(len(pm.p.Mapping)+1),Start:src.Start,Limit:src.Limit,Offset:src.Offset,File:src.File,BuildID:src.BuildID,HasFunctions:src.HasFunctions,HasFilenames:src.HasFilenames,HasLineNumbers:src.HasLineNumbers,HasInlineFrames:src.HasInlineFrames,));
            pm.p.Mapping = append(pm.p.Mapping, m); 

            // Update memoization tables.
            pm.mappings[mk] = m;
            mi = new mapInfo(m,0);
            pm.mappingsByID[src.ID] = mi;
            return mi;

        }

        // key generates encoded strings of Mapping to be used as a key for
        // maps.
        private static mappingKey key(this ptr<Mapping> _addr_m)
        {
            ref Mapping m = ref _addr_m.val;
 
            // Normalize addresses to handle address space randomization.
            // Round up to next 4K boundary to avoid minor discrepancies.
            const ulong mapsizeRounding = (ulong)0x1000UL;



            var size = m.Limit - m.Start;
            size = size + mapsizeRounding - 1L;
            size = size - (size % mapsizeRounding);
            mappingKey key = new mappingKey(size:size,offset:m.Offset,);


            if (m.BuildID != "") 
                key.buildIDOrFile = m.BuildID;
            else if (m.File != "") 
                key.buildIDOrFile = m.File;
            else                         return key;

        }

        private partial struct mappingKey
        {
            public ulong size;
            public ulong offset;
            public @string buildIDOrFile;
        }

        private static Line mapLine(this ptr<profileMerger> _addr_pm, Line src)
        {
            ref profileMerger pm = ref _addr_pm.val;

            Line ln = new Line(Function:pm.mapFunction(src.Function),Line:src.Line,);
            return ln;
        }

        private static ptr<Function> mapFunction(this ptr<profileMerger> _addr_pm, ptr<Function> _addr_src)
        {
            ref profileMerger pm = ref _addr_pm.val;
            ref Function src = ref _addr_src.val;

            if (src == null)
            {
                return _addr_null!;
            }

            {
                var f__prev1 = f;

                var (f, ok) = pm.functionsByID[src.ID];

                if (ok)
                {
                    return _addr_f!;
                }

                f = f__prev1;

            }

            var k = src.key();
            {
                var f__prev1 = f;

                (f, ok) = pm.functions[k];

                if (ok)
                {
                    pm.functionsByID[src.ID] = f;
                    return _addr_f!;
                }

                f = f__prev1;

            }

            ptr<Function> f = addr(new Function(ID:uint64(len(pm.p.Function)+1),Name:src.Name,SystemName:src.SystemName,Filename:src.Filename,StartLine:src.StartLine,));
            pm.functions[k] = f;
            pm.functionsByID[src.ID] = f;
            pm.p.Function = append(pm.p.Function, f);
            return _addr_f!;

        }

        // key generates a struct to be used as a key for maps.
        private static functionKey key(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            return new functionKey(f.StartLine,f.Name,f.SystemName,f.Filename,);
        }

        private partial struct functionKey
        {
            public long startLine;
            public @string name;
            public @string systemName;
            public @string fileName;
        }

        // combineHeaders checks that all profiles can be merged and returns
        // their combined profile.
        private static (ptr<Profile>, error) combineHeaders(slice<ptr<Profile>> srcs)
        {
            ptr<Profile> _p0 = default!;
            error _p0 = default!;

            {
                var s__prev1 = s;

                foreach (var (_, __s) in srcs[1L..])
                {
                    s = __s;
                    {
                        var err = srcs[0L].compatible(s);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                    }

                }

                s = s__prev1;
            }

            long timeNanos = default;            long durationNanos = default;            long period = default;

            slice<@string> comments = default;
            map seenComments = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            @string defaultSampleType = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in srcs)
                {
                    s = __s;
                    if (timeNanos == 0L || s.TimeNanos < timeNanos)
                    {
                        timeNanos = s.TimeNanos;
                    }

                    durationNanos += s.DurationNanos;
                    if (period == 0L || period < s.Period)
                    {
                        period = s.Period;
                    }

                    foreach (var (_, c) in s.Comments)
                    {
                        {
                            var seen = seenComments[c];

                            if (!seen)
                            {
                                comments = append(comments, c);
                                seenComments[c] = true;
                            }

                        }

                    }
                    if (defaultSampleType == "")
                    {
                        defaultSampleType = s.DefaultSampleType;
                    }

                }

                s = s__prev1;
            }

            ptr<Profile> p = addr(new Profile(SampleType:make([]*ValueType,len(srcs[0].SampleType)),DropFrames:srcs[0].DropFrames,KeepFrames:srcs[0].KeepFrames,TimeNanos:timeNanos,DurationNanos:durationNanos,PeriodType:srcs[0].PeriodType,Period:period,Comments:comments,DefaultSampleType:defaultSampleType,));
            copy(p.SampleType, srcs[0L].SampleType);
            return (_addr_p!, error.As(null!)!);

        }

        // compatible determines if two profiles can be compared/merged.
        // returns nil if the profiles are compatible; otherwise an error with
        // details on the incompatibility.
        private static error compatible(this ptr<Profile> _addr_p, ptr<Profile> _addr_pb)
        {
            ref Profile p = ref _addr_p.val;
            ref Profile pb = ref _addr_pb.val;

            if (!equalValueType(_addr_p.PeriodType, _addr_pb.PeriodType))
            {
                return error.As(fmt.Errorf("incompatible period types %v and %v", p.PeriodType, pb.PeriodType))!;
            }

            if (len(p.SampleType) != len(pb.SampleType))
            {
                return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType))!;
            }

            foreach (var (i) in p.SampleType)
            {
                if (!equalValueType(_addr_p.SampleType[i], _addr_pb.SampleType[i]))
                {
                    return error.As(fmt.Errorf("incompatible sample types %v and %v", p.SampleType, pb.SampleType))!;
                }

            }
            return error.As(null!)!;

        }

        // equalValueType returns true if the two value types are semantically
        // equal. It ignores the internal fields used during encode/decode.
        private static bool equalValueType(ptr<ValueType> _addr_st1, ptr<ValueType> _addr_st2)
        {
            ref ValueType st1 = ref _addr_st1.val;
            ref ValueType st2 = ref _addr_st2.val;

            return st1.Type == st2.Type && st1.Unit == st2.Unit;
        }
    }
}}
