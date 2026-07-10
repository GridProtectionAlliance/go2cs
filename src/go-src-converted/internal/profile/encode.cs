// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;

partial class profile_package {

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Profile p) {
    return profileDecoder;
}

// preEncode populates the unexported fields to be used by encode
// (with suffix X) from the corresponding exported fields. The
// exported fields are cleared up to facilitate testing.
[GoRecv] internal static void preEncode(this ref Profile p) {
    var strings = new map<@string, nint>();
    addString(strings, ""u8);
    foreach (var (_, vᴛ1) in p.SampleType) {
        var st = vᴛ1;

        st.Value.typeX = addString(strings, (~st).Type);
        st.Value.unitX = addString(strings, (~st).Unit);
    }
    foreach (var (_, vᴛ2) in p.Sample) {
        var s = vᴛ2;

        s.Value.labelX = default!;
        slice<@string> keys = default!;
        foreach (var (k, _) in (~s).Label) {
            keys = append(keys, k);
        }
        sort.Strings(keys);
        foreach (var (_, k) in keys) {
            var vs = (~s).Label[k];
            foreach (var (_, v) in vs) {
                s.Value.labelX = append((~s).labelX,
                    new Label(
                        keyX: addString(strings, k),
                        strX: addString(strings, v)
                    ));
            }
        }
        slice<@string> numKeys = default!;
        foreach (var (k, _) in (~s).NumLabel) {
            numKeys = append(numKeys, k);
        }
        sort.Strings(numKeys);
        foreach (var (_, k) in numKeys) {
            var vs = (~s).NumLabel[k];
            foreach (var (_, v) in vs) {
                s.Value.labelX = append((~s).labelX,
                    new Label(
                        keyX: addString(strings, k),
                        numX: v
                    ));
            }
        }
        s.Value.locationIDX = default!;
        foreach (var (_, l) in (~s).Location) {
            s.Value.locationIDX = append((~s).locationIDX, (~l).ID);
        }
    }
    foreach (var (_, vᴛ3) in p.Mapping) {
        var m = vᴛ3;

        m.Value.fileX = addString(strings, (~m).File);
        m.Value.buildIDX = addString(strings, (~m).BuildID);
    }
    foreach (var (_, vᴛ4) in p.Location) {
        var l = vᴛ4;

        foreach (var (i, ln) in (~l).Line) {
            if (ln.Function != nil){
                (~l).Line[i].functionIDX = ln.Function.Value.ID;
            } else {
                (~l).Line[i].functionIDX = 0;
            }
        }
        if ((~l).Mapping != nil){
            l.Value.mappingIDX = l.Value.Mapping.Value.ID;
        } else {
            l.Value.mappingIDX = 0;
        }
    }
    foreach (var (_, vᴛ5) in p.Function) {
        var f = vᴛ5;

        f.Value.nameX = addString(strings, (~f).Name);
        f.Value.systemNameX = addString(strings, (~f).SystemName);
        f.Value.filenameX = addString(strings, (~f).Filename);
    }
    p.dropFramesX = addString(strings, p.DropFrames);
    p.keepFramesX = addString(strings, p.KeepFrames);
    {
        var pt = p.PeriodType; if (pt != nil) {
            pt.Value.typeX = addString(strings, (~pt).Type);
            pt.Value.unitX = addString(strings, (~pt).Unit);
        }
    }
    p.stringTable = new slice<@string>(len(strings));
    foreach (var (s, i) in strings) {
        p.stringTable[i] = s;
    }
}

[GoRecv] internal static void encode(this ref Profile p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    foreach (var (_, x) in p.SampleType) {
        encodeMessage(Ꮡb, 1, new ValueTypeжmessage(x));
    }
    foreach (var (_, x) in p.Sample) {
        encodeMessage(Ꮡb, 2, new Sampleжmessage(x));
    }
    foreach (var (_, x) in p.Mapping) {
        encodeMessage(Ꮡb, 3, new Mappingжmessage(x));
    }
    foreach (var (_, x) in p.Location) {
        encodeMessage(Ꮡb, 4, new Locationжmessage(x));
    }
    foreach (var (_, x) in p.Function) {
        encodeMessage(Ꮡb, 5, new Functionжmessage(x));
    }
    encodeStrings(Ꮡb, 6, p.stringTable);
    encodeInt64Opt(Ꮡb, 7, p.dropFramesX);
    encodeInt64Opt(Ꮡb, 8, p.keepFramesX);
    encodeInt64Opt(Ꮡb, 9, p.TimeNanos);
    encodeInt64Opt(Ꮡb, 10, p.DurationNanos);
    {
        var pt = p.PeriodType; if (pt != nil && ((~pt).typeX != 0 || (~pt).unitX != 0)) {
            encodeMessage(Ꮡb, 11, new ValueTypeжmessage(p.PeriodType));
        }
    }
    encodeInt64Opt(Ꮡb, 12, p.Period);
}

// 0
// repeated ValueType sample_type = 1
// repeated Sample sample = 2
// repeated Mapping mapping = 3
// repeated Location location = 4
// repeated Function function = 5
// repeated string string_table = 6
// repeated int64 drop_frames = 7
// repeated int64 keep_frames = 8
// repeated int64 time_nanos = 9
// repeated int64 duration_nanos = 10
// optional string period_type = 11
// repeated int64 period = 12
// repeated int64 comment = 13
// int64 defaultSampleType = 14
internal static slice<Func<ж<buffer>, message, error>> profileDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<ж<Profile>>();
        pp.Value.SampleType = append((~pp).SampleType, x);
        return decodeMessage(b, new ValueTypeжmessage(x));
    },
    (ж<buffer> b, message m) => {
        var x = @new<Sample>();
        var pp = m._<ж<Profile>>();
        pp.Value.Sample = append((~pp).Sample, x);
        return decodeMessage(b, new Sampleжmessage(x));
    },
    (ж<buffer> b, message m) => {
        var x = @new<Mapping>();
        var pp = m._<ж<Profile>>();
        pp.Value.Mapping = append((~pp).Mapping, x);
        return decodeMessage(b, new Mappingжmessage(x));
    },
    (ж<buffer> b, message m) => {
        var x = @new<Location>();
        var pp = m._<ж<Profile>>();
        pp.Value.Location = append((~pp).Location, x);
        return decodeMessage(b, new Locationжmessage(x));
    },
    (ж<buffer> b, message m) => {
        var x = @new<Function>();
        var pp = m._<ж<Profile>>();
        pp.Value.Function = append((~pp).Function, x);
        return decodeMessage(b, new Functionжmessage(x));
    },
    error (ж<buffer> b, message m) => {
        var err = decodeStrings(b, Ꮡ((~m._<ж<Profile>>()).stringTable));
        if (err != default!) {
            return err;
        }
        if ((~m._<ж<Profile>>()).stringTable[0] != "") {
            return errors.New("string_table[0] must be ''"u8);
        }
        return default!;
    },
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).dropFramesX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).keepFramesX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).TimeNanos)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).DurationNanos)),
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<ж<Profile>>();
        pp.Value.PeriodType = x;
        return decodeMessage(b, new ValueTypeжmessage(x));
    },
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).Period)),
    (ж<buffer> b, message m) => decodeInt64s(b, Ꮡ((~m._<ж<Profile>>()).commentX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Profile>>()).defaultSampleTypeX))
}.slice();

// postDecode takes the unexported fields populated by decode (with
// suffix X) and populates the corresponding exported fields.
// The unexported fields are cleared up to facilitate testing.
internal static error postDecode(this ж<Profile> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    error err = default!;
    var mappings = new map<uint64, ж<Mapping>>();
    foreach (var (_, vᴛ1) in p.Mapping) {
        var m = vᴛ1;

        (m.Value.File, err) = getString(p.stringTable, m.of(Mapping.ᏑfileX), err);
        (m.Value.BuildID, err) = getString(p.stringTable, m.of(Mapping.ᏑbuildIDX), err);
        mappings[(~m).ID] = m;
    }
    var functions = new map<uint64, ж<Function>>();
    foreach (var (_, vᴛ2) in p.Function) {
        var f = vᴛ2;

        (f.Value.Name, err) = getString(p.stringTable, f.of(Function.ᏑnameX), err);
        (f.Value.SystemName, err) = getString(p.stringTable, f.of(Function.ᏑsystemNameX), err);
        (f.Value.Filename, err) = getString(p.stringTable, f.of(Function.ᏑfilenameX), err);
        functions[(~f).ID] = f;
    }
    var locations = new map<uint64, ж<Location>>();
    foreach (var (_, vᴛ3) in p.Location) {
        var l = vᴛ3;

        l.Value.Mapping = mappings[(~l).mappingIDX];
        l.Value.mappingIDX = 0;
        foreach (var (i, ln) in (~l).Line) {
            {
                var id = ln.functionIDX; if (id != 0) {
                    (~l).Line[i].Function = functions[id];
                    if ((~l).Line[i].Function == nil) {
                        return fmt.Errorf("Function ID %d not found"u8, id);
                    }
                    (~l).Line[i].functionIDX = 0;
                }
            }
        }
        locations[(~l).ID] = l;
    }
    foreach (var (_, vᴛ4) in p.SampleType) {
        var st = vᴛ4;

        (st.Value.Type, err) = getString(p.stringTable, st.of(ValueType.ᏑtypeX), err);
        (st.Value.Unit, err) = getString(p.stringTable, st.of(ValueType.ᏑunitX), err);
    }
    foreach (var (_, vᴛ5) in p.Sample) {
        var s = vᴛ5;

        var labels = new map<@string, slice<@string>>();
        var numLabels = new map<@string, slice<int64>>();
        foreach (var (_, vᴛ6) in (~s).labelX) {
            ref var l = ref heap(new Label(), out var Ꮡl);
            l = vᴛ6;

            @string key = default!;
            @string value = default!;
            (key, err) = getString(p.stringTable, Ꮡl.of(Label.ᏑkeyX), err);
            if (l.strX != 0){
                (value, err) = getString(p.stringTable, Ꮡl.of(Label.ᏑstrX), err);
                labels[key] = append(labels[key], value);
            } else {
                numLabels[key] = append(numLabels[key], l.numX);
            }
        }
        if (len(labels) > 0) {
            s.Value.Label = labels;
        }
        if (len(numLabels) > 0) {
            s.Value.NumLabel = numLabels;
        }
        s.Value.Location = default!;
        foreach (var (_, lid) in (~s).locationIDX) {
            s.Value.Location = append((~s).Location, locations[lid]);
        }
        s.Value.locationIDX = default!;
    }
    (p.DropFrames, err) = getString(p.stringTable, Ꮡp.of(Profile.ᏑdropFramesX), err);
    (p.KeepFrames, err) = getString(p.stringTable, Ꮡp.of(Profile.ᏑkeepFramesX), err);
    {
        var pt = p.PeriodType; if (pt == nil) {
            p.PeriodType = Ꮡ(new ValueType(nil));
        }
    }
    {
        var pt = p.PeriodType; if (pt != nil) {
            (pt.Value.Type, err) = getString(p.stringTable, pt.of(ValueType.ᏑtypeX), err);
            (pt.Value.Unit, err) = getString(p.stringTable, pt.of(ValueType.ᏑunitX), err);
        }
    }
    foreach (var (_, vᴛ7) in p.commentX) {
        ref var i = ref heap(new int64(), out var Ꮡi);
        i = vᴛ7;

        @string c = default!;
        (c, err) = getString(p.stringTable, Ꮡi, err);
        p.Comments = append(p.Comments, c);
    }
    p.commentX = default!;
    (p.DefaultSampleType, err) = getString(p.stringTable, Ꮡp.of(Profile.ᏑdefaultSampleTypeX), err);
    p.stringTable = default!;
    return err;
}

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref ValueType p) {
    return valueTypeDecoder;
}

[GoRecv] internal static void encode(this ref ValueType p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeInt64Opt(Ꮡb, 1, p.typeX);
    encodeInt64Opt(Ꮡb, 2, p.unitX);
}

// 0
// optional int64 type = 1
// optional int64 unit = 2
internal static slice<Func<ж<buffer>, message, error>> valueTypeDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<ValueType>>()).typeX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<ValueType>>()).unitX))
}.slice();

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Sample p) {
    return sampleDecoder;
}

[GoRecv] internal static void encode(this ref Sample p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeUint64s(Ꮡb, 1, p.locationIDX);
    foreach (var (_, x) in p.Value) {
        encodeInt64(Ꮡb, 2, x);
    }
    foreach (var (_, x) in p.labelX) {
        encodeMessage(Ꮡb, 3, x);
    }
}

// 0
// repeated uint64 location = 1
// repeated int64 value = 2
// repeated Label label = 3
internal static slice<Func<ж<buffer>, message, error>> sampleDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64s(b, Ꮡ((~m._<ж<Sample>>()).locationIDX)),
    (ж<buffer> b, message m) => decodeInt64s(b, Ꮡ((~m._<ж<Sample>>()).Value)),
    (ж<buffer> b, message m) => {
        var s = m._<ж<Sample>>();
        nint n = len((~s).labelX);
        s.Value.labelX = append((~s).labelX, new Label(nil));
        return decodeMessage(b, new Labelжmessage(Ꮡ((~s).labelX, n)));
    }
}.slice();

internal static slice<Func<ж<buffer>, message, error>> decoder(this Label p) {
    return labelDecoder;
}

internal static void encode(this Label p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeInt64Opt(Ꮡb, 1, p.keyX);
    encodeInt64Opt(Ꮡb, 2, p.strX);
    encodeInt64Opt(Ꮡb, 3, p.numX);
}

// 0
// optional int64 key = 1
// optional int64 str = 2
// optional int64 num = 3
internal static slice<Func<ж<buffer>, message, error>> labelDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Label>>()).keyX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Label>>()).strX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Label>>()).numX))
}.slice();

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Mapping p) {
    return mappingDecoder;
}

[GoRecv] internal static void encode(this ref Mapping p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeUint64Opt(Ꮡb, 1, p.ID);
    encodeUint64Opt(Ꮡb, 2, p.Start);
    encodeUint64Opt(Ꮡb, 3, p.Limit);
    encodeUint64Opt(Ꮡb, 4, p.Offset);
    encodeInt64Opt(Ꮡb, 5, p.fileX);
    encodeInt64Opt(Ꮡb, 6, p.buildIDX);
    encodeBoolOpt(Ꮡb, 7, p.HasFunctions);
    encodeBoolOpt(Ꮡb, 8, p.HasFilenames);
    encodeBoolOpt(Ꮡb, 9, p.HasLineNumbers);
    encodeBoolOpt(Ꮡb, 10, p.HasInlineFrames);
}

// 0
// optional uint64 id = 1
// optional uint64 memory_offset = 2
// optional uint64 memory_limit = 3
// optional uint64 file_offset = 4
// optional int64 filename = 5
// optional int64 build_id = 6
// optional bool has_functions = 7
// optional bool has_filenames = 8
// optional bool has_line_numbers = 9
// optional bool has_inline_frames = 10
internal static slice<Func<ж<buffer>, message, error>> mappingDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Mapping>>()).ID)),
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Mapping>>()).Start)),
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Mapping>>()).Limit)),
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Mapping>>()).Offset)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Mapping>>()).fileX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Mapping>>()).buildIDX)),
    (ж<buffer> b, message m) => decodeBool(b, Ꮡ((~m._<ж<Mapping>>()).HasFunctions)),
    (ж<buffer> b, message m) => decodeBool(b, Ꮡ((~m._<ж<Mapping>>()).HasFilenames)),
    (ж<buffer> b, message m) => decodeBool(b, Ꮡ((~m._<ж<Mapping>>()).HasLineNumbers)),
    (ж<buffer> b, message m) => decodeBool(b, Ꮡ((~m._<ж<Mapping>>()).HasInlineFrames))
}.slice();

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Location p) {
    return locationDecoder;
}

[GoRecv] internal static void encode(this ref Location p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeUint64Opt(Ꮡb, 1, p.ID);
    encodeUint64Opt(Ꮡb, 2, p.mappingIDX);
    encodeUint64Opt(Ꮡb, 3, p.Address);
    foreach (var (i, _) in p.Line) {
        encodeMessage(Ꮡb, 4, new Lineжmessage(Ꮡ(p.Line[i])));
    }
}

// 0
// optional uint64 id = 1;
// optional uint64 mapping_id = 2;
// optional uint64 address = 3;
// repeated Line line = 4
internal static slice<Func<ж<buffer>, message, error>> locationDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Location>>()).ID)),
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Location>>()).mappingIDX)),
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Location>>()).Address)),
    (ж<buffer> b, message m) => {
        var pp = m._<ж<Location>>();
        nint n = len((~pp).Line);
        pp.Value.Line = append((~pp).Line, new Line(nil));
        return decodeMessage(b, new Lineжmessage(Ꮡ((~pp).Line, n)));
    }
}.slice();

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Line p) {
    return lineDecoder;
}

[GoRecv] internal static void encode(this ref Line p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeUint64Opt(Ꮡb, 1, p.functionIDX);
    encodeInt64Opt(Ꮡb, 2, p.ΔLine);
}

// 0
// optional uint64 function_id = 1
// optional int64 line = 2
internal static slice<Func<ж<buffer>, message, error>> lineDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Line>>()).functionIDX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Line>>()).ΔLine))
}.slice();

[GoRecv] internal static slice<Func<ж<buffer>, message, error>> decoder(this ref Function p) {
    return functionDecoder;
}

[GoRecv] internal static void encode(this ref Function p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    encodeUint64Opt(Ꮡb, 1, p.ID);
    encodeInt64Opt(Ꮡb, 2, p.nameX);
    encodeInt64Opt(Ꮡb, 3, p.systemNameX);
    encodeInt64Opt(Ꮡb, 4, p.filenameX);
    encodeInt64Opt(Ꮡb, 5, p.StartLine);
}

// 0
// optional uint64 id = 1
// optional int64 function_name = 2
// optional int64 function_system_name = 3
// repeated int64 filename = 4
// optional int64 start_line = 5
internal static slice<Func<ж<buffer>, message, error>> functionDecoder = new Func<ж<buffer>, message, error>[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(b, Ꮡ((~m._<ж<Function>>()).ID)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Function>>()).nameX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Function>>()).systemNameX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Function>>()).filenameX)),
    (ж<buffer> b, message m) => decodeInt64(b, Ꮡ((~m._<ж<Function>>()).StartLine))
}.slice();

internal static int64 addString(map<@string, nint> strings, @string s) {
    var (i, ok) = strings[s, ꟷ];
    if (!ok) {
        i = len(strings);
        strings[s] = i;
    }
    return (int64)i;
}

internal static (@string, error) getString(slice<@string> strings, ж<int64> Ꮡstrng, error err) {
    ref var strng = ref Ꮡstrng.Value;

    if (err != default!) {
        return ("", err);
    }
    nint s = (nint)(strng);
    if (s < 0 || s >= len(strings)) {
        return ("", errMalformed);
    }
    strng = 0;
    return (strings[s], default!);
}

} // end profile_package
