// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;

partial class profile_package {

[GoRecv] internal static slice<Δdecoder> decoder(this ref Profile p) {
    return profileDecoder;
}

// preEncode populates the unexported fields to be used by encode
// (with suffix X) from the corresponding exported fields. The
// exported fields are cleared up to facilitate testing.
[GoRecv] internal static void preEncode(this ref Profile p) {
    var strings = new map<@string, nint>();
    addString(strings, ""u8);
    foreach (var (_, st) in p.SampleType) {
        st.val.typeX = addString(strings, (~st).Type);
        st.val.unitX = addString(strings, (~st).Unit);
    }
    foreach (var (_, s) in p.Sample) {
        s.val.labelX = default!;
        slice<@string> keys = default!;
        foreach (var (k, _) in (~s).Label) {
            keys = append(keys, k);
        }
        sort.Strings(keys);
        foreach (var (_, k) in keys) {
            var vs = (~s).Label[k];
            foreach (var (_, v) in vs) {
                s.val.labelX = append((~s).labelX,
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
                s.val.labelX = append((~s).labelX,
                    new Label(
                        keyX: addString(strings, k),
                        numX: v
                    ));
            }
        }
        s.val.locationIDX = default!;
        foreach (var (_, l) in (~s).Location) {
            s.val.locationIDX = append((~s).locationIDX, (~l).ID);
        }
    }
    foreach (var (_, m) in p.Mapping) {
        m.val.fileX = addString(strings, (~m).File);
        m.val.buildIDX = addString(strings, (~m).BuildID);
    }
    foreach (var (_, l) in p.Location) {
        foreach (var (i, ln) in (~l).Line) {
            if (ln.Function != nil){
                (~l).Line[i].functionIDX = ln.Function.val.ID;
            } else {
                (~l).Line[i].functionIDX = 0;
            }
        }
        if ((~l).Mapping != nil){
            l.val.mappingIDX = (~l).Mapping.val.ID;
        } else {
            l.val.mappingIDX = 0;
        }
    }
    foreach (var (_, f) in p.Function) {
        f.val.nameX = addString(strings, (~f).Name);
        f.val.systemNameX = addString(strings, (~f).SystemName);
        f.val.filenameX = addString(strings, (~f).Filename);
    }
    p.dropFramesX = addString(strings, p.DropFrames);
    p.keepFramesX = addString(strings, p.KeepFrames);
    {
        var pt = p.PeriodType; if (pt != nil) {
            pt.val.typeX = addString(strings, (~pt).Type);
            pt.val.unitX = addString(strings, (~pt).Unit);
        }
    }
    p.stringTable = new slice<@string>(len(strings));
    foreach (var (s, i) in strings) {
        p.stringTable[i] = s;
    }
}

[GoRecv] public static void encode(this ref Profile p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    foreach (var (_, x) in p.SampleType) {
        encodeMessage(Ꮡb, 1, ~x);
    }
    foreach (var (_, x) in p.Sample) {
        encodeMessage(Ꮡb, 2, ~x);
    }
    foreach (var (_, x) in p.Mapping) {
        encodeMessage(Ꮡb, 3, ~x);
    }
    foreach (var (_, x) in p.Location) {
        encodeMessage(Ꮡb, 4, ~x);
    }
    foreach (var (_, x) in p.Function) {
        encodeMessage(Ꮡb, 5, ~x);
    }
    encodeStrings(Ꮡb, 6, p.stringTable);
    encodeInt64Opt(Ꮡb, 7, p.dropFramesX);
    encodeInt64Opt(Ꮡb, 8, p.keepFramesX);
    encodeInt64Opt(Ꮡb, 9, p.TimeNanos);
    encodeInt64Opt(Ꮡb, 10, p.DurationNanos);
    {
        var pt = p.PeriodType; if (pt != nil && ((~pt).typeX != 0 || (~pt).unitX != 0)) {
            encodeMessage(Ꮡb, 11, ~p.PeriodType);
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
internal static slice<Δdecoder> profileDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<Profile.val>();
        var pp.val.SampleType = append((~pp).SampleType, x);
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => {
        var x = @new<Sample>();
        var pp = m._<Profile.val>();
        var pp.val.Sample = append((~pp).Sample, x);
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => {
        var x = @new<Mapping>();
        var pp = m._<Profile.val>();
        var pp.val.Mapping = append((~pp).Mapping, x);
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => {
        var x = @new<Location>();
        var pp = m._<Profile.val>();
        var pp.val.Location = append((~pp).Location, x);
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => {
        var x = @new<Function>();
        var pp = m._<Profile.val>();
        var pp.val.Function = append((~pp).Function, x);
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => {
        var err = decodeStrings(Ꮡb, Ꮡ(m._<Profile.val>().stringTable));
        if (err != default!) {
            return err;
        }
        if (m._<Profile.val>().stringTable[0] != "") {
            return errors.New("string_table[0] must be ''"u8);
        }
        return default!;
    },
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().dropFramesX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().keepFramesX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().TimeNanos)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().DurationNanos)),
    (ж<buffer> b, message m) => {
        var x = @new<ValueType>();
        var pp = m._<Profile.val>();
        var pp.val.PeriodType = x;
        return decodeMessage(Ꮡb, ~x);
    },
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().Period)),
    (ж<buffer> b, message m) => decodeInt64s(Ꮡb, Ꮡ(m._<Profile.val>().commentX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Profile.val>().defaultSampleTypeX))
}.slice();

// postDecode takes the unexported fields populated by decode (with
// suffix X) and populates the corresponding exported fields.
// The unexported fields are cleared up to facilitate testing.
[GoRecv] internal static error postDecode(this ref Profile p) {
    error err = default!;
    var mappings = new map<uint64, ж<Mapping>>();
    foreach (var (_, m) in p.Mapping) {
        (m.val.File, err) = getString(p.stringTable, Ꮡ((~m).fileX), err);
        (m.val.BuildID, err) = getString(p.stringTable, Ꮡ((~m).buildIDX), err);
        mappings[(~m).ID] = m;
    }
    var functions = new map<uint64, ж<Function>>();
    foreach (var (_, f) in p.Function) {
        (f.val.Name, err) = getString(p.stringTable, Ꮡ((~f).nameX), err);
        (f.val.SystemName, err) = getString(p.stringTable, Ꮡ((~f).systemNameX), err);
        (f.val.Filename, err) = getString(p.stringTable, Ꮡ((~f).filenameX), err);
        functions[(~f).ID] = f;
    }
    var locations = new map<uint64, ж<Location>>();
    foreach (var (_, l) in p.Location) {
        l.val.Mapping = mappings[(~l).mappingIDX];
        l.val.mappingIDX = 0;
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
    foreach (var (_, st) in p.SampleType) {
        (st.val.Type, err) = getString(p.stringTable, Ꮡ((~st).typeX), err);
        (st.val.Unit, err) = getString(p.stringTable, Ꮡ((~st).unitX), err);
    }
    foreach (var (_, s) in p.Sample) {
        var labels = new map<@string, slice<@string>>();
        var numLabels = new map<@string, slice<int64>>();
        ref var l = ref heap(new Label(), out var Ꮡl);

        foreach (var (_, l) in (~s).labelX) {
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
            s.val.Label = labels;
        }
        if (len(numLabels) > 0) {
            s.val.NumLabel = numLabels;
        }
        s.val.Location = default!;
        ref var lid = ref heap(new uint64(), out var Ꮡlid);

        foreach (var (_, lid) in (~s).locationIDX) {
            s.val.Location = append((~s).Location, locations[lid]);
        }
        s.val.locationIDX = default!;
    }
    (p.DropFrames, err) = getString(p.stringTable, Ꮡ(p.dropFramesX), err);
    (p.KeepFrames, err) = getString(p.stringTable, Ꮡ(p.keepFramesX), err);
    {
        var pt = p.PeriodType; if (pt == nil) {
            p.PeriodType = Ꮡ(new ValueType(nil));
        }
    }
    {
        var pt = p.PeriodType; if (pt != nil) {
            (pt.val.Type, err) = getString(p.stringTable, Ꮡ((~pt).typeX), err);
            (pt.val.Unit, err) = getString(p.stringTable, Ꮡ((~pt).unitX), err);
        }
    }
    ref var i = ref heap(new int64(), out var Ꮡi);

    foreach (var (_, i) in p.commentX) {
        @string c = default!;
        (c, err) = getString(p.stringTable, Ꮡi, err);
        p.Comments = append(p.Comments, c);
    }
    p.commentX = default!;
    (p.DefaultSampleType, err) = getString(p.stringTable, Ꮡ(p.defaultSampleTypeX), err);
    p.stringTable = default!;
    return err;
}

[GoRecv] internal static slice<Δdecoder> decoder(this ref ValueType p) {
    return valueTypeDecoder;
}

[GoRecv] public static void encode(this ref ValueType p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    encodeInt64Opt(Ꮡb, 1, p.typeX);
    encodeInt64Opt(Ꮡb, 2, p.unitX);
}

// 0
// optional int64 type = 1
// optional int64 unit = 2
internal static slice<Δdecoder> valueTypeDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<ValueType.val>().typeX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<ValueType.val>().unitX))
}.slice();

[GoRecv] internal static slice<Δdecoder> decoder(this ref Sample p) {
    return sampleDecoder;
}

[GoRecv] public static void encode(this ref Sample p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

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
internal static slice<Δdecoder> sampleDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64s(Ꮡb, Ꮡ(m._<Sample.val>().locationIDX)),
    (ж<buffer> b, message m) => decodeInt64s(Ꮡb, Ꮡ(m._<Sample.val>().Value)),
    (ж<buffer> b, message m) => {
        var s = m._<Sample.val>();
        nint n = len((~s).labelX);
        var s.val.labelX = append((~s).labelX, new Label(nil));
        return decodeMessage(Ꮡb, (~s).labelX, n);
    }
}.slice();

internal static slice<Δdecoder> decoder(this Label p) {
    return labelDecoder;
}

public static void encode(this Label p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    encodeInt64Opt(Ꮡb, 1, p.keyX);
    encodeInt64Opt(Ꮡb, 2, p.strX);
    encodeInt64Opt(Ꮡb, 3, p.numX);
}

// 0
// optional int64 key = 1
// optional int64 str = 2
// optional int64 num = 3
internal static slice<Δdecoder> labelDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Label.val>().keyX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Label.val>().strX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Label.val>().numX))
}.slice();

[GoRecv] internal static slice<Δdecoder> decoder(this ref Mapping p) {
    return mappingDecoder;
}

[GoRecv] public static void encode(this ref Mapping p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

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
internal static slice<Δdecoder> mappingDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Mapping.val>().ID)),
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Mapping.val>().Start)),
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Mapping.val>().Limit)),
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Mapping.val>().Offset)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Mapping.val>().fileX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Mapping.val>().buildIDX)),
    (ж<buffer> b, message m) => decodeBool(Ꮡb, Ꮡ(m._<Mapping.val>().HasFunctions)),
    (ж<buffer> b, message m) => decodeBool(Ꮡb, Ꮡ(m._<Mapping.val>().HasFilenames)),
    (ж<buffer> b, message m) => decodeBool(Ꮡb, Ꮡ(m._<Mapping.val>().HasLineNumbers)),
    (ж<buffer> b, message m) => decodeBool(Ꮡb, Ꮡ(m._<Mapping.val>().HasInlineFrames))
}.slice();

[GoRecv] internal static slice<Δdecoder> decoder(this ref Location p) {
    return locationDecoder;
}

[GoRecv] public static void encode(this ref Location p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    encodeUint64Opt(Ꮡb, 1, p.ID);
    encodeUint64Opt(Ꮡb, 2, p.mappingIDX);
    encodeUint64Opt(Ꮡb, 3, p.Address);
    foreach (var (i, _) in p.Line) {
        encodeMessage(Ꮡb, 4, p.Line[i]);
    }
}

// 0
// optional uint64 id = 1;
// optional uint64 mapping_id = 2;
// optional uint64 address = 3;
// repeated Line line = 4
internal static slice<Δdecoder> locationDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Location.val>().ID)),
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Location.val>().mappingIDX)),
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Location.val>().Address)),
    (ж<buffer> b, message m) => {
        var pp = m._<Location.val>();
        nint n = len((~pp).Line);
        var pp.val.Line = append((~pp).Line, new Line(nil));
        return decodeMessage(Ꮡb, (~pp).Line, n);
    }
}.slice();

[GoRecv] internal static slice<Δdecoder> decoder(this ref Line p) {
    return lineDecoder;
}

[GoRecv] public static void encode(this ref Line p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    encodeUint64Opt(Ꮡb, 1, p.functionIDX);
    encodeInt64Opt(Ꮡb, 2, p.Line);
}

// 0
// optional uint64 function_id = 1
// optional int64 line = 2
internal static slice<Δdecoder> lineDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Line.val>().functionIDX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Line.val>().Line))
}.slice();

[GoRecv] internal static slice<Δdecoder> decoder(this ref Function p) {
    return functionDecoder;
}

[GoRecv] public static void encode(this ref Function p, ж<buffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

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
internal static slice<Δdecoder> functionDecoder = new Δdecoder[]{
    default!,
    (ж<buffer> b, message m) => decodeUint64(Ꮡb, Ꮡ(m._<Function.val>().ID)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Function.val>().nameX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Function.val>().systemNameX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Function.val>().filenameX)),
    (ж<buffer> b, message m) => decodeInt64(Ꮡb, Ꮡ(m._<Function.val>().StartLine))
}.slice();

internal static int64 addString(map<@string, nint> strings, @string s) {
    nint i = strings[s];
    var ok = strings[s];
    if (!ok) {
        i = len(strings);
        strings[s] = i;
    }
    return ((int64)i);
}

internal static (@string, error) getString(slice<@string> strings, ж<int64> Ꮡstrng, error err) {
    ref var strng = ref Ꮡstrng.val;

    if (err != default!) {
        return ("", err);
    }
    nint s = ((nint)(strng));
    if (s < 0 || s >= len(strings)) {
        return ("", errMalformed);
    }
    strng = 0;
    return (strings[s], default!);
}

} // end profile_package
