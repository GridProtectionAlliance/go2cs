// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package profile -- go2cs converted at 2020 October 09 04:59:04 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Go\src\internal\profile\encode.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class profile_package
    {
        private static slice<decoder> decoder(this ptr<Profile> _addr_p)
        {
            ref Profile p = ref _addr_p.val;

            return profileDecoder;
        }

        // preEncode populates the unexported fields to be used by encode
        // (with suffix X) from the corresponding exported fields. The
        // exported fields are cleared up to facilitate testing.
        private static void preEncode(this ptr<Profile> _addr_p)
        {
            ref Profile p = ref _addr_p.val;

            var strings = make_map<@string, long>();
            addString(strings, "");

            foreach (var (_, st) in p.SampleType)
            {
                st.typeX = addString(strings, st.Type);
                st.unitX = addString(strings, st.Unit);
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in p.Sample)
                {
                    s = __s;
                    s.labelX = null;
                    slice<@string> keys = default;
                    {
                        var k__prev2 = k;

                        foreach (var (__k) in s.Label)
                        {
                            k = __k;
                            keys = append(keys, k);
                        }

                        k = k__prev2;
                    }

                    sort.Strings(keys);
                    {
                        var k__prev2 = k;

                        foreach (var (_, __k) in keys)
                        {
                            k = __k;
                            var vs = s.Label[k];
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in vs)
                                {
                                    v = __v;
                                    s.labelX = append(s.labelX, new Label(keyX:addString(strings,k),strX:addString(strings,v),));
                                }

                                v = v__prev3;
                            }
                        }

                        k = k__prev2;
                    }

                    slice<@string> numKeys = default;
                    {
                        var k__prev2 = k;

                        foreach (var (__k) in s.NumLabel)
                        {
                            k = __k;
                            numKeys = append(numKeys, k);
                        }

                        k = k__prev2;
                    }

                    sort.Strings(numKeys);
                    {
                        var k__prev2 = k;

                        foreach (var (_, __k) in numKeys)
                        {
                            k = __k;
                            vs = s.NumLabel[k];
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in vs)
                                {
                                    v = __v;
                                    s.labelX = append(s.labelX, new Label(keyX:addString(strings,k),numX:v,));
                                }

                                v = v__prev3;
                            }
                        }

                        k = k__prev2;
                    }

                    s.locationIDX = null;
                    {
                        var l__prev2 = l;

                        foreach (var (_, __l) in s.Location)
                        {
                            l = __l;
                            s.locationIDX = append(s.locationIDX, l.ID);
                        }

                        l = l__prev2;
                    }
                }

                s = s__prev1;
            }

            foreach (var (_, m) in p.Mapping)
            {
                m.fileX = addString(strings, m.File);
                m.buildIDX = addString(strings, m.BuildID);
            }
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __ln) in l.Line)
                        {
                            i = __i;
                            ln = __ln;
                            if (ln.Function != null)
                            {
                                l.Line[i].functionIDX = ln.Function.ID;
                            }
                            else
                            {
                                l.Line[i].functionIDX = 0L;
                            }

                        }

                        i = i__prev2;
                    }

                    if (l.Mapping != null)
                    {
                        l.mappingIDX = l.Mapping.ID;
                    }
                    else
                    {
                        l.mappingIDX = 0L;
                    }

                }

                l = l__prev1;
            }

            foreach (var (_, f) in p.Function)
            {
                f.nameX = addString(strings, f.Name);
                f.systemNameX = addString(strings, f.SystemName);
                f.filenameX = addString(strings, f.Filename);
            }
            p.dropFramesX = addString(strings, p.DropFrames);
            p.keepFramesX = addString(strings, p.KeepFrames);

            {
                var pt = p.PeriodType;

                if (pt != null)
                {
                    pt.typeX = addString(strings, pt.Type);
                    pt.unitX = addString(strings, pt.Unit);
                }

            }


            p.stringTable = make_slice<@string>(len(strings));
            {
                var s__prev1 = s;
                var i__prev1 = i;

                foreach (var (__s, __i) in strings)
                {
                    s = __s;
                    i = __i;
                    p.stringTable[i] = s;
                }

                s = s__prev1;
                i = i__prev1;
            }
        }

        private static void encode(this ptr<Profile> _addr_p, ptr<buffer> _addr_b)
        {
            ref Profile p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.SampleType)
                {
                    x = __x;
                    encodeMessage(b, 1L, x);
                }

                x = x__prev1;
            }

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.Sample)
                {
                    x = __x;
                    encodeMessage(b, 2L, x);
                }

                x = x__prev1;
            }

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.Mapping)
                {
                    x = __x;
                    encodeMessage(b, 3L, x);
                }

                x = x__prev1;
            }

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.Location)
                {
                    x = __x;
                    encodeMessage(b, 4L, x);
                }

                x = x__prev1;
            }

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.Function)
                {
                    x = __x;
                    encodeMessage(b, 5L, x);
                }

                x = x__prev1;
            }

            encodeStrings(b, 6L, p.stringTable);
            encodeInt64Opt(b, 7L, p.dropFramesX);
            encodeInt64Opt(b, 8L, p.keepFramesX);
            encodeInt64Opt(b, 9L, p.TimeNanos);
            encodeInt64Opt(b, 10L, p.DurationNanos);
            {
                var pt = p.PeriodType;

                if (pt != null && (pt.typeX != 0L || pt.unitX != 0L))
                {
                    encodeMessage(b, 11L, p.PeriodType);
                }

            }

            encodeInt64Opt(b, 12L, p.Period);

        }

        private static decoder profileDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{x:=new(ValueType)pp:=m.(*Profile)pp.SampleType=append(pp.SampleType,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Sample)pp:=m.(*Profile)pp.Sample=append(pp.Sample,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Mapping)pp:=m.(*Profile)pp.Mapping=append(pp.Mapping,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Location)pp:=m.(*Profile)pp.Location=append(pp.Location,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Function)pp:=m.(*Profile)pp.Function=append(pp.Function,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{err:=decodeStrings(b,&m.(*Profile).stringTable)iferr!=nil{returnerr}if*&m.(*Profile).stringTable[0]!=""{returnerrors.New("string_table[0] must be ''")}returnnil}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).dropFramesX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).keepFramesX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).TimeNanos)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).DurationNanos)}, func(b*buffer,mmessage)error{x:=new(ValueType)pp:=m.(*Profile)pp.PeriodType=xreturndecodeMessage(b,x)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).Period)}, func(b*buffer,mmessage)error{returndecodeInt64s(b,&m.(*Profile).commentX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).defaultSampleTypeX)} });

        // postDecode takes the unexported fields populated by decode (with
        // suffix X) and populates the corresponding exported fields.
        // The unexported fields are cleared up to facilitate testing.
        private static error postDecode(this ptr<Profile> _addr_p)
        {
            ref Profile p = ref _addr_p.val;

            error err = default!;

            var mappings = make_map<ulong, ptr<Mapping>>();
            foreach (var (_, m) in p.Mapping)
            {
                m.File, err = getString(p.stringTable, _addr_m.fileX, err);
                m.BuildID, err = getString(p.stringTable, _addr_m.buildIDX, err);
                mappings[m.ID] = m;
            }
            var functions = make_map<ulong, ptr<Function>>();
            foreach (var (_, f) in p.Function)
            {
                f.Name, err = getString(p.stringTable, _addr_f.nameX, err);
                f.SystemName, err = getString(p.stringTable, _addr_f.systemNameX, err);
                f.Filename, err = getString(p.stringTable, _addr_f.filenameX, err);
                functions[f.ID] = f;
            }
            var locations = make_map<ulong, ptr<Location>>();
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    l.Mapping = mappings[l.mappingIDX];
                    l.mappingIDX = 0L;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __ln) in l.Line)
                        {
                            i = __i;
                            ln = __ln;
                            {
                                var id = ln.functionIDX;

                                if (id != 0L)
                                {
                                    l.Line[i].Function = functions[id];
                                    if (l.Line[i].Function == null)
                                    {
                                        return error.As(fmt.Errorf("Function ID %d not found", id))!;
                                    }

                                    l.Line[i].functionIDX = 0L;

                                }

                            }

                        }

                        i = i__prev2;
                    }

                    locations[l.ID] = l;

                }

                l = l__prev1;
            }

            foreach (var (_, st) in p.SampleType)
            {
                st.Type, err = getString(p.stringTable, _addr_st.typeX, err);
                st.Unit, err = getString(p.stringTable, _addr_st.unitX, err);
            }
            foreach (var (_, s) in p.Sample)
            {
                var labels = make_map<@string, slice<@string>>();
                var numLabels = make_map<@string, slice<long>>();
                {
                    var l__prev2 = l;

                    foreach (var (_, __l) in s.labelX)
                    {
                        l = __l;
                        @string key = default;                        @string value = default;

                        key, err = getString(p.stringTable, _addr_l.keyX, err);
                        if (l.strX != 0L)
                        {
                            value, err = getString(p.stringTable, _addr_l.strX, err);
                            labels[key] = append(labels[key], value);
                        }
                        else
                        {
                            numLabels[key] = append(numLabels[key], l.numX);
                        }

                    }

                    l = l__prev2;
                }

                if (len(labels) > 0L)
                {
                    s.Label = labels;
                }

                if (len(numLabels) > 0L)
                {
                    s.NumLabel = numLabels;
                }

                s.Location = null;
                foreach (var (_, lid) in s.locationIDX)
                {
                    s.Location = append(s.Location, locations[lid]);
                }
                s.locationIDX = null;

            }
            p.DropFrames, err = getString(p.stringTable, _addr_p.dropFramesX, err);
            p.KeepFrames, err = getString(p.stringTable, _addr_p.keepFramesX, err);

            {
                var pt__prev1 = pt;

                var pt = p.PeriodType;

                if (pt == null)
                {
                    p.PeriodType = addr(new ValueType());
                }

                pt = pt__prev1;

            }


            {
                var pt__prev1 = pt;

                pt = p.PeriodType;

                if (pt != null)
                {
                    pt.Type, err = getString(p.stringTable, _addr_pt.typeX, err);
                    pt.Unit, err = getString(p.stringTable, _addr_pt.unitX, err);
                }

                pt = pt__prev1;

            }

            {
                var i__prev1 = i;

                foreach (var (_, __i) in p.commentX)
                {
                    i = __i;
                    @string c = default;
                    c, err = getString(p.stringTable, _addr_i, err);
                    p.Comments = append(p.Comments, c);
                }

                i = i__prev1;
            }

            p.commentX = null;
            p.DefaultSampleType, err = getString(p.stringTable, _addr_p.defaultSampleTypeX, err);
            p.stringTable = null;
            return error.As(null!)!;

        }

        private static slice<decoder> decoder(this ptr<ValueType> _addr_p)
        {
            ref ValueType p = ref _addr_p.val;

            return valueTypeDecoder;
        }

        private static void encode(this ptr<ValueType> _addr_p, ptr<buffer> _addr_b)
        {
            ref ValueType p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeInt64Opt(b, 1L, p.typeX);
            encodeInt64Opt(b, 2L, p.unitX);
        }

        private static decoder valueTypeDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*ValueType).typeX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*ValueType).unitX)} });

        private static slice<decoder> decoder(this ptr<Sample> _addr_p)
        {
            ref Sample p = ref _addr_p.val;

            return sampleDecoder;
        }

        private static void encode(this ptr<Sample> _addr_p, ptr<buffer> _addr_b)
        {
            ref Sample p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeUint64s(b, 1L, p.locationIDX);
            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.Value)
                {
                    x = __x;
                    encodeInt64(b, 2L, x);
                }

                x = x__prev1;
            }

            {
                var x__prev1 = x;

                foreach (var (_, __x) in p.labelX)
                {
                    x = __x;
                    encodeMessage(b, 3L, x);
                }

                x = x__prev1;
            }
        }

        private static decoder sampleDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64s(b,&m.(*Sample).locationIDX)}, func(b*buffer,mmessage)error{returndecodeInt64s(b,&m.(*Sample).Value)}, func(b*buffer,mmessage)error{s:=m.(*Sample)n:=len(s.labelX)s.labelX=append(s.labelX,Label{})returndecodeMessage(b,&s.labelX[n])} });

        public static slice<decoder> decoder(this Label p)
        {
            return labelDecoder;
        }

        public static void encode(this Label p, ptr<buffer> _addr_b)
        {
            ref buffer b = ref _addr_b.val;

            encodeInt64Opt(b, 1L, p.keyX);
            encodeInt64Opt(b, 2L, p.strX);
            encodeInt64Opt(b, 3L, p.numX);
        }

        private static decoder labelDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Label).keyX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Label).strX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Label).numX)} });

        private static slice<decoder> decoder(this ptr<Mapping> _addr_p)
        {
            ref Mapping p = ref _addr_p.val;

            return mappingDecoder;
        }

        private static void encode(this ptr<Mapping> _addr_p, ptr<buffer> _addr_b)
        {
            ref Mapping p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeUint64Opt(b, 1L, p.ID);
            encodeUint64Opt(b, 2L, p.Start);
            encodeUint64Opt(b, 3L, p.Limit);
            encodeUint64Opt(b, 4L, p.Offset);
            encodeInt64Opt(b, 5L, p.fileX);
            encodeInt64Opt(b, 6L, p.buildIDX);
            encodeBoolOpt(b, 7L, p.HasFunctions);
            encodeBoolOpt(b, 8L, p.HasFilenames);
            encodeBoolOpt(b, 9L, p.HasLineNumbers);
            encodeBoolOpt(b, 10L, p.HasInlineFrames);
        }

        private static decoder mappingDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Mapping).ID)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Mapping).Start)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Mapping).Limit)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Mapping).Offset)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Mapping).fileX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Mapping).buildIDX)}, func(b*buffer,mmessage)error{returndecodeBool(b,&m.(*Mapping).HasFunctions)}, func(b*buffer,mmessage)error{returndecodeBool(b,&m.(*Mapping).HasFilenames)}, func(b*buffer,mmessage)error{returndecodeBool(b,&m.(*Mapping).HasLineNumbers)}, func(b*buffer,mmessage)error{returndecodeBool(b,&m.(*Mapping).HasInlineFrames)} });

        private static slice<decoder> decoder(this ptr<Location> _addr_p)
        {
            ref Location p = ref _addr_p.val;

            return locationDecoder;
        }

        private static void encode(this ptr<Location> _addr_p, ptr<buffer> _addr_b)
        {
            ref Location p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeUint64Opt(b, 1L, p.ID);
            encodeUint64Opt(b, 2L, p.mappingIDX);
            encodeUint64Opt(b, 3L, p.Address);
            foreach (var (i) in p.Line)
            {
                encodeMessage(b, 4L, _addr_p.Line[i]);
            }

        }

        private static decoder locationDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).ID)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).mappingIDX)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).Address)}, func(b*buffer,mmessage)error{pp:=m.(*Location)n:=len(pp.Line)pp.Line=append(pp.Line,Line{})returndecodeMessage(b,&pp.Line[n])} });

        private static slice<decoder> decoder(this ptr<Line> _addr_p)
        {
            ref Line p = ref _addr_p.val;

            return lineDecoder;
        }

        private static void encode(this ptr<Line> _addr_p, ptr<buffer> _addr_b)
        {
            ref Line p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeUint64Opt(b, 1L, p.functionIDX);
            encodeInt64Opt(b, 2L, p.Line);
        }

        private static decoder lineDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Line).functionIDX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Line).Line)} });

        private static slice<decoder> decoder(this ptr<Function> _addr_p)
        {
            ref Function p = ref _addr_p.val;

            return functionDecoder;
        }

        private static void encode(this ptr<Function> _addr_p, ptr<buffer> _addr_b)
        {
            ref Function p = ref _addr_p.val;
            ref buffer b = ref _addr_b.val;

            encodeUint64Opt(b, 1L, p.ID);
            encodeInt64Opt(b, 2L, p.nameX);
            encodeInt64Opt(b, 3L, p.systemNameX);
            encodeInt64Opt(b, 4L, p.filenameX);
            encodeInt64Opt(b, 5L, p.StartLine);
        }

        private static decoder functionDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Function).ID)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Function).nameX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Function).systemNameX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Function).filenameX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Function).StartLine)} });

        private static long addString(map<@string, long> strings, @string s)
        {
            var (i, ok) = strings[s];
            if (!ok)
            {
                i = len(strings);
                strings[s] = i;
            }

            return int64(i);

        }

        private static (@string, error) getString(slice<@string> strings, ptr<long> _addr_strng, error err)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref long strng = ref _addr_strng.val;

            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var s = int(strng);
            if (s < 0L || s >= len(strings))
            {
                return ("", error.As(errMalformed)!);
            }

            strng = 0L;
            return (strings[s], error.As(null!)!);

        }
    }
}}
