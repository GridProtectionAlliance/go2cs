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

// package profile -- go2cs converted at 2020 October 09 05:53:52 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\encode.go
using errors = go.errors_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
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
                                    s.labelX = append(s.labelX, new label(keyX:addString(strings,k),strX:addString(strings,v),));
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
                            var keyX = addString(strings, k);
                            vs = s.NumLabel[k];
                            var units = s.NumUnit[k];
                            {
                                var i__prev3 = i;
                                var v__prev3 = v;

                                foreach (var (__i, __v) in vs)
                                {
                                    i = __i;
                                    v = __v;
                                    long unitX = default;
                                    if (len(units) != 0L)
                                    {
                                        unitX = addString(strings, units[i]);
                                    }

                                    s.labelX = append(s.labelX, new label(keyX:keyX,numX:v,unitX:unitX,));

                                }

                                i = i__prev3;
                                v = v__prev3;
                            }
                        }

                        k = k__prev2;
                    }

                    s.locationIDX = make_slice<ulong>(len(s.Location));
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __loc) in s.Location)
                        {
                            i = __i;
                            loc = __loc;
                            s.locationIDX[i] = loc.ID;
                        }

                        i = i__prev2;
                    }
                }

                s = s__prev1;
            }

            foreach (var (_, m) in p.Mapping)
            {
                m.fileX = addString(strings, m.File);
                m.buildIDX = addString(strings, m.BuildID);
            }
            foreach (var (_, l) in p.Location)
            {
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


            p.commentX = null;
            foreach (var (_, c) in p.Comments)
            {
                p.commentX = append(p.commentX, addString(strings, c));
            }
            p.defaultSampleTypeX = addString(strings, p.DefaultSampleType);

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
            encodeInt64s(b, 13L, p.commentX);
            encodeInt64(b, 14L, p.defaultSampleTypeX);

        }

        private static decoder profileDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{x:=new(ValueType)pp:=m.(*Profile)pp.SampleType=append(pp.SampleType,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Sample)pp:=m.(*Profile)pp.Sample=append(pp.Sample,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Mapping)pp:=m.(*Profile)pp.Mapping=append(pp.Mapping,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{x:=new(Location)x.Line=make([]Line,0,8)pp:=m.(*Profile)pp.Location=append(pp.Location,x)err:=decodeMessage(b,x)vartmp[]Linex.Line=append(tmp,x.Line...)returnerr}, func(b*buffer,mmessage)error{x:=new(Function)pp:=m.(*Profile)pp.Function=append(pp.Function,x)returndecodeMessage(b,x)}, func(b*buffer,mmessage)error{err:=decodeStrings(b,&m.(*Profile).stringTable)iferr!=nil{returnerr}ifm.(*Profile).stringTable[0]!=""{returnerrors.New("string_table[0] must be ''")}returnnil}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).dropFramesX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).keepFramesX)}, func(b*buffer,mmessage)error{ifm.(*Profile).TimeNanos!=0{returnerrConcatProfile}returndecodeInt64(b,&m.(*Profile).TimeNanos)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).DurationNanos)}, func(b*buffer,mmessage)error{x:=new(ValueType)pp:=m.(*Profile)pp.PeriodType=xreturndecodeMessage(b,x)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).Period)}, func(b*buffer,mmessage)error{returndecodeInt64s(b,&m.(*Profile).commentX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*Profile).defaultSampleTypeX)} });

        // postDecode takes the unexported fields populated by decode (with
        // suffix X) and populates the corresponding exported fields.
        // The unexported fields are cleared up to facilitate testing.
        private static error postDecode(this ptr<Profile> _addr_p)
        {
            ref Profile p = ref _addr_p.val;

            error err = default!;
            var mappings = make_map<ulong, ptr<Mapping>>(len(p.Mapping));
            var mappingIds = make_slice<ptr<Mapping>>(len(p.Mapping) + 1L);
            foreach (var (_, m) in p.Mapping)
            {
                m.File, err = getString(p.stringTable, _addr_m.fileX, err);
                m.BuildID, err = getString(p.stringTable, _addr_m.buildIDX, err);
                if (m.ID < uint64(len(mappingIds)))
                {
                    mappingIds[m.ID] = m;
                }
                else
                {
                    mappings[m.ID] = m;
                }

            }
            var functions = make_map<ulong, ptr<Function>>(len(p.Function));
            var functionIds = make_slice<ptr<Function>>(len(p.Function) + 1L);
            foreach (var (_, f) in p.Function)
            {
                f.Name, err = getString(p.stringTable, _addr_f.nameX, err);
                f.SystemName, err = getString(p.stringTable, _addr_f.systemNameX, err);
                f.Filename, err = getString(p.stringTable, _addr_f.filenameX, err);
                if (f.ID < uint64(len(functionIds)))
                {
                    functionIds[f.ID] = f;
                }
                else
                {
                    functions[f.ID] = f;
                }

            }
            var locations = make_map<ulong, ptr<Location>>(len(p.Location));
            var locationIds = make_slice<ptr<Location>>(len(p.Location) + 1L);
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    {
                        var id__prev1 = id;

                        var id = l.mappingIDX;

                        if (id < uint64(len(mappingIds)))
                        {
                            l.Mapping = mappingIds[id];
                        }
                        else
                        {
                            l.Mapping = mappings[id];
                        }

                        id = id__prev1;

                    }

                    l.mappingIDX = 0L;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __ln) in l.Line)
                        {
                            i = __i;
                            ln = __ln;
                            {
                                var id__prev1 = id;

                                id = ln.functionIDX;

                                if (id != 0L)
                                {
                                    l.Line[i].functionIDX = 0L;
                                    if (id < uint64(len(functionIds)))
                                    {
                                        l.Line[i].Function = functionIds[id];
                                    }
                                    else
                                    {
                                        l.Line[i].Function = functions[id];
                                    }

                                }

                                id = id__prev1;

                            }

                        }

                        i = i__prev2;
                    }

                    if (l.ID < uint64(len(locationIds)))
                    {
                        locationIds[l.ID] = l;
                    }
                    else
                    {
                        locations[l.ID] = l;
                    }

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
                var labels = make_map<@string, slice<@string>>(len(s.labelX));
                var numLabels = make_map<@string, slice<long>>(len(s.labelX));
                var numUnits = make_map<@string, slice<@string>>(len(s.labelX));
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
                        else if (l.numX != 0L)
                        {
                            var numValues = numLabels[key];
                            var units = numUnits[key];
                            if (l.unitX != 0L)
                            {
                                @string unit = default;
                                unit, err = getString(p.stringTable, _addr_l.unitX, err);
                                units = padStringArray(units, len(numValues));
                                numUnits[key] = append(units, unit);
                            }

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
                    {
                        @string key__prev2 = key;
                        var units__prev2 = units;

                        foreach (var (__key, __units) in numUnits)
                        {
                            key = __key;
                            units = __units;
                            if (len(units) > 0L)
                            {
                                numUnits[key] = padStringArray(units, len(numLabels[key]));
                            }

                        }

                        key = key__prev2;
                        units = units__prev2;
                    }

                    s.NumUnit = numUnits;

                }

                s.Location = make_slice<ptr<Location>>(len(s.locationIDX));
                {
                    var i__prev2 = i;

                    foreach (var (__i, __lid) in s.locationIDX)
                    {
                        i = __i;
                        lid = __lid;
                        if (lid < uint64(len(locationIds)))
                        {
                            s.Location[i] = locationIds[lid];
                        }
                        else
                        {
                            s.Location[i] = locations[lid];
                        }

                    }

                    i = i__prev2;
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
            return error.As(err)!;

        }

        // padStringArray pads arr with enough empty strings to make arr
        // length l when arr's length is less than l.
        private static slice<@string> padStringArray(slice<@string> arr, long l)
        {
            if (l <= len(arr))
            {
                return arr;
            }

            return append(arr, make_slice<@string>(l - len(arr)));

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
            encodeInt64s(b, 2L, p.Value);
            foreach (var (_, x) in p.labelX)
            {
                encodeMessage(b, 3L, x);
            }

        }

        private static decoder sampleDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64s(b,&m.(*Sample).locationIDX)}, func(b*buffer,mmessage)error{returndecodeInt64s(b,&m.(*Sample).Value)}, func(b*buffer,mmessage)error{s:=m.(*Sample)n:=len(s.labelX)s.labelX=append(s.labelX,label{})returndecodeMessage(b,&s.labelX[n])} });

        private static slice<decoder> decoder(this label p)
        {
            return labelDecoder;
        }

        private static void encode(this label p, ptr<buffer> _addr_b)
        {
            ref buffer b = ref _addr_b.val;

            encodeInt64Opt(b, 1L, p.keyX);
            encodeInt64Opt(b, 2L, p.strX);
            encodeInt64Opt(b, 3L, p.numX);
            encodeInt64Opt(b, 4L, p.unitX);
        }

        private static decoder labelDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*label).keyX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*label).strX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*label).numX)}, func(b*buffer,mmessage)error{returndecodeInt64(b,&m.(*label).unitX)} });

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
            encodeBoolOpt(b, 5L, p.IsFolded);

        }

        private static decoder locationDecoder = new slice<decoder>(new decoder[] { nil, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).ID)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).mappingIDX)}, func(b*buffer,mmessage)error{returndecodeUint64(b,&m.(*Location).Address)}, func(b*buffer,mmessage)error{pp:=m.(*Location)n:=len(pp.Line)pp.Line=append(pp.Line,Line{})returndecodeMessage(b,&pp.Line[n])}, func(b*buffer,mmessage)error{returndecodeBool(b,&m.(*Location).IsFolded)} });

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
}}}}}}
