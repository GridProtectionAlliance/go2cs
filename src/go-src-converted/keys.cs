// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package keys -- go2cs converted at 2022 March 06 23:31:35 UTC
// import "golang.org/x/tools/internal/event/keys" ==> using keys = go.golang.org.x.tools.@internal.@event.keys_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\keys\keys.go
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using strconv = go.strconv_package;

using label = go.golang.org.x.tools.@internal.@event.label_package;

namespace go.golang.org.x.tools.@internal.@event;

public static partial class keys_package {

    // Value represents a key for untyped values.
public partial struct Value {
    public @string name;
    public @string description;
}

// New creates a new Key for untyped values.
public static ptr<Value> New(@string name, @string description) {
    return addr(new Value(name:name,description:description));
}

private static @string Name(this ptr<Value> _addr_k) {
    ref Value k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Value> _addr_k) {
    ref Value k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Value> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Value k = ref _addr_k.val;

    fmt.Fprint(w, k.From(l));
}

// Get can be used to get a label for the key from a label.Map.
private static void Get(this ptr<Value> _addr_k, label.Map lm) {
    ref Value k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return null;

}

// From can be used to get a value from a Label.
private static void From(this ptr<Value> _addr_k, label.Label t) {
    ref Value k = ref _addr_k.val;

    return t.UnpackValue();
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Value> _addr_k, object value) {
    ref Value k = ref _addr_k.val;

    return label.OfValue(k, value);
}

// Tag represents a key for tagging labels that have no value.
// These are used when the existence of the label is the entire information it
// carries, such as marking events to be of a specific kind, or from a specific
// package.
public partial struct Tag {
    public @string name;
    public @string description;
}

// NewTag creates a new Key for tagging labels.
public static ptr<Tag> NewTag(@string name, @string description) {
    return addr(new Tag(name:name,description:description));
}

private static @string Name(this ptr<Tag> _addr_k) {
    ref Tag k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Tag> _addr_k) {
    ref Tag k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Tag> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Tag k = ref _addr_k.val;

}

// New creates a new Label with this key.
private static label.Label New(this ptr<Tag> _addr_k) {
    ref Tag k = ref _addr_k.val;

    return label.OfValue(k, null);
}

// Int represents a key
public partial struct Int {
    public @string name;
    public @string description;
}

// NewInt creates a new Key for int values.
public static ptr<Int> NewInt(@string name, @string description) {
    return addr(new Int(name:name,description:description));
}

private static @string Name(this ptr<Int> _addr_k) {
    ref Int k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Int> _addr_k) {
    ref Int k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Int> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Int k = ref _addr_k.val;

    w.Write(strconv.AppendInt(buf, int64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Int> _addr_k, nint v) {
    ref Int k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static nint Get(this ptr<Int> _addr_k, label.Map lm) {
    ref Int k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static nint From(this ptr<Int> _addr_k, label.Label t) {
    ref Int k = ref _addr_k.val;

    return int(t.Unpack64());
}

// Int8 represents a key
public partial struct Int8 {
    public @string name;
    public @string description;
}

// NewInt8 creates a new Key for int8 values.
public static ptr<Int8> NewInt8(@string name, @string description) {
    return addr(new Int8(name:name,description:description));
}

private static @string Name(this ptr<Int8> _addr_k) {
    ref Int8 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Int8> _addr_k) {
    ref Int8 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Int8> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Int8 k = ref _addr_k.val;

    w.Write(strconv.AppendInt(buf, int64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Int8> _addr_k, sbyte v) {
    ref Int8 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static sbyte Get(this ptr<Int8> _addr_k, label.Map lm) {
    ref Int8 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static sbyte From(this ptr<Int8> _addr_k, label.Label t) {
    ref Int8 k = ref _addr_k.val;

    return int8(t.Unpack64());
}

// Int16 represents a key
public partial struct Int16 {
    public @string name;
    public @string description;
}

// NewInt16 creates a new Key for int16 values.
public static ptr<Int16> NewInt16(@string name, @string description) {
    return addr(new Int16(name:name,description:description));
}

private static @string Name(this ptr<Int16> _addr_k) {
    ref Int16 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Int16> _addr_k) {
    ref Int16 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Int16> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Int16 k = ref _addr_k.val;

    w.Write(strconv.AppendInt(buf, int64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Int16> _addr_k, short v) {
    ref Int16 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static short Get(this ptr<Int16> _addr_k, label.Map lm) {
    ref Int16 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static short From(this ptr<Int16> _addr_k, label.Label t) {
    ref Int16 k = ref _addr_k.val;

    return int16(t.Unpack64());
}

// Int32 represents a key
public partial struct Int32 {
    public @string name;
    public @string description;
}

// NewInt32 creates a new Key for int32 values.
public static ptr<Int32> NewInt32(@string name, @string description) {
    return addr(new Int32(name:name,description:description));
}

private static @string Name(this ptr<Int32> _addr_k) {
    ref Int32 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Int32> _addr_k) {
    ref Int32 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Int32> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Int32 k = ref _addr_k.val;

    w.Write(strconv.AppendInt(buf, int64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Int32> _addr_k, int v) {
    ref Int32 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static int Get(this ptr<Int32> _addr_k, label.Map lm) {
    ref Int32 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static int From(this ptr<Int32> _addr_k, label.Label t) {
    ref Int32 k = ref _addr_k.val;

    return int32(t.Unpack64());
}

// Int64 represents a key
public partial struct Int64 {
    public @string name;
    public @string description;
}

// NewInt64 creates a new Key for int64 values.
public static ptr<Int64> NewInt64(@string name, @string description) {
    return addr(new Int64(name:name,description:description));
}

private static @string Name(this ptr<Int64> _addr_k) {
    ref Int64 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Int64> _addr_k) {
    ref Int64 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Int64> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Int64 k = ref _addr_k.val;

    w.Write(strconv.AppendInt(buf, k.From(l), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Int64> _addr_k, long v) {
    ref Int64 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static long Get(this ptr<Int64> _addr_k, label.Map lm) {
    ref Int64 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static long From(this ptr<Int64> _addr_k, label.Label t) {
    ref Int64 k = ref _addr_k.val;

    return int64(t.Unpack64());
}

// UInt represents a key
public partial struct UInt {
    public @string name;
    public @string description;
}

// NewUInt creates a new Key for uint values.
public static ptr<UInt> NewUInt(@string name, @string description) {
    return addr(new UInt(name:name,description:description));
}

private static @string Name(this ptr<UInt> _addr_k) {
    ref UInt k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<UInt> _addr_k) {
    ref UInt k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<UInt> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref UInt k = ref _addr_k.val;

    w.Write(strconv.AppendUint(buf, uint64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<UInt> _addr_k, nuint v) {
    ref UInt k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static nuint Get(this ptr<UInt> _addr_k, label.Map lm) {
    ref UInt k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static nuint From(this ptr<UInt> _addr_k, label.Label t) {
    ref UInt k = ref _addr_k.val;

    return uint(t.Unpack64());
}

// UInt8 represents a key
public partial struct UInt8 {
    public @string name;
    public @string description;
}

// NewUInt8 creates a new Key for uint8 values.
public static ptr<UInt8> NewUInt8(@string name, @string description) {
    return addr(new UInt8(name:name,description:description));
}

private static @string Name(this ptr<UInt8> _addr_k) {
    ref UInt8 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<UInt8> _addr_k) {
    ref UInt8 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<UInt8> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref UInt8 k = ref _addr_k.val;

    w.Write(strconv.AppendUint(buf, uint64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<UInt8> _addr_k, byte v) {
    ref UInt8 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static byte Get(this ptr<UInt8> _addr_k, label.Map lm) {
    ref UInt8 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static byte From(this ptr<UInt8> _addr_k, label.Label t) {
    ref UInt8 k = ref _addr_k.val;

    return uint8(t.Unpack64());
}

// UInt16 represents a key
public partial struct UInt16 {
    public @string name;
    public @string description;
}

// NewUInt16 creates a new Key for uint16 values.
public static ptr<UInt16> NewUInt16(@string name, @string description) {
    return addr(new UInt16(name:name,description:description));
}

private static @string Name(this ptr<UInt16> _addr_k) {
    ref UInt16 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<UInt16> _addr_k) {
    ref UInt16 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<UInt16> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref UInt16 k = ref _addr_k.val;

    w.Write(strconv.AppendUint(buf, uint64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<UInt16> _addr_k, ushort v) {
    ref UInt16 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static ushort Get(this ptr<UInt16> _addr_k, label.Map lm) {
    ref UInt16 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static ushort From(this ptr<UInt16> _addr_k, label.Label t) {
    ref UInt16 k = ref _addr_k.val;

    return uint16(t.Unpack64());
}

// UInt32 represents a key
public partial struct UInt32 {
    public @string name;
    public @string description;
}

// NewUInt32 creates a new Key for uint32 values.
public static ptr<UInt32> NewUInt32(@string name, @string description) {
    return addr(new UInt32(name:name,description:description));
}

private static @string Name(this ptr<UInt32> _addr_k) {
    ref UInt32 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<UInt32> _addr_k) {
    ref UInt32 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<UInt32> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref UInt32 k = ref _addr_k.val;

    w.Write(strconv.AppendUint(buf, uint64(k.From(l)), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<UInt32> _addr_k, uint v) {
    ref UInt32 k = ref _addr_k.val;

    return label.Of64(k, uint64(v));
}

// Get can be used to get a label for the key from a label.Map.
private static uint Get(this ptr<UInt32> _addr_k, label.Map lm) {
    ref UInt32 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static uint From(this ptr<UInt32> _addr_k, label.Label t) {
    ref UInt32 k = ref _addr_k.val;

    return uint32(t.Unpack64());
}

// UInt64 represents a key
public partial struct UInt64 {
    public @string name;
    public @string description;
}

// NewUInt64 creates a new Key for uint64 values.
public static ptr<UInt64> NewUInt64(@string name, @string description) {
    return addr(new UInt64(name:name,description:description));
}

private static @string Name(this ptr<UInt64> _addr_k) {
    ref UInt64 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<UInt64> _addr_k) {
    ref UInt64 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<UInt64> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref UInt64 k = ref _addr_k.val;

    w.Write(strconv.AppendUint(buf, k.From(l), 10));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<UInt64> _addr_k, ulong v) {
    ref UInt64 k = ref _addr_k.val;

    return label.Of64(k, v);
}

// Get can be used to get a label for the key from a label.Map.
private static ulong Get(this ptr<UInt64> _addr_k, label.Map lm) {
    ref UInt64 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static ulong From(this ptr<UInt64> _addr_k, label.Label t) {
    ref UInt64 k = ref _addr_k.val;

    return t.Unpack64();
}

// Float32 represents a key
public partial struct Float32 {
    public @string name;
    public @string description;
}

// NewFloat32 creates a new Key for float32 values.
public static ptr<Float32> NewFloat32(@string name, @string description) {
    return addr(new Float32(name:name,description:description));
}

private static @string Name(this ptr<Float32> _addr_k) {
    ref Float32 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Float32> _addr_k) {
    ref Float32 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Float32> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Float32 k = ref _addr_k.val;

    w.Write(strconv.AppendFloat(buf, float64(k.From(l)), 'E', -1, 32));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Float32> _addr_k, float v) {
    ref Float32 k = ref _addr_k.val;

    return label.Of64(k, uint64(math.Float32bits(v)));
}

// Get can be used to get a label for the key from a label.Map.
private static float Get(this ptr<Float32> _addr_k, label.Map lm) {
    ref Float32 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static float From(this ptr<Float32> _addr_k, label.Label t) {
    ref Float32 k = ref _addr_k.val;

    return math.Float32frombits(uint32(t.Unpack64()));
}

// Float64 represents a key
public partial struct Float64 {
    public @string name;
    public @string description;
}

// NewFloat64 creates a new Key for int64 values.
public static ptr<Float64> NewFloat64(@string name, @string description) {
    return addr(new Float64(name:name,description:description));
}

private static @string Name(this ptr<Float64> _addr_k) {
    ref Float64 k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Float64> _addr_k) {
    ref Float64 k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Float64> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Float64 k = ref _addr_k.val;

    w.Write(strconv.AppendFloat(buf, k.From(l), 'E', -1, 64));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Float64> _addr_k, double v) {
    ref Float64 k = ref _addr_k.val;

    return label.Of64(k, math.Float64bits(v));
}

// Get can be used to get a label for the key from a label.Map.
private static double Get(this ptr<Float64> _addr_k, label.Map lm) {
    ref Float64 k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return 0;

}

// From can be used to get a value from a Label.
private static double From(this ptr<Float64> _addr_k, label.Label t) {
    ref Float64 k = ref _addr_k.val;

    return math.Float64frombits(t.Unpack64());
}

// String represents a key
public partial struct String {
    public @string name;
    public @string description;
}

// NewString creates a new Key for int64 values.
public static ptr<String> NewString(@string name, @string description) {
    return addr(new String(name:name,description:description));
}

private static @string Name(this ptr<String> _addr_k) {
    ref String k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<String> _addr_k) {
    ref String k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<String> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref String k = ref _addr_k.val;

    w.Write(strconv.AppendQuote(buf, k.From(l)));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<String> _addr_k, @string v) {
    ref String k = ref _addr_k.val;

    return label.OfString(k, v);
}

// Get can be used to get a label for the key from a label.Map.
private static @string Get(this ptr<String> _addr_k, label.Map lm) {
    ref String k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return "";

}

// From can be used to get a value from a Label.
private static @string From(this ptr<String> _addr_k, label.Label t) {
    ref String k = ref _addr_k.val;

    return t.UnpackString();
}

// Boolean represents a key
public partial struct Boolean {
    public @string name;
    public @string description;
}

// NewBoolean creates a new Key for bool values.
public static ptr<Boolean> NewBoolean(@string name, @string description) {
    return addr(new Boolean(name:name,description:description));
}

private static @string Name(this ptr<Boolean> _addr_k) {
    ref Boolean k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Boolean> _addr_k) {
    ref Boolean k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Boolean> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Boolean k = ref _addr_k.val;

    w.Write(strconv.AppendBool(buf, k.From(l)));
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Boolean> _addr_k, bool v) {
    ref Boolean k = ref _addr_k.val;

    if (v) {
        return label.Of64(k, 1);
    }
    return label.Of64(k, 0);

}

// Get can be used to get a label for the key from a label.Map.
private static bool Get(this ptr<Boolean> _addr_k, label.Map lm) {
    ref Boolean k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return k.From(t);
        }
    }

    return false;

}

// From can be used to get a value from a Label.
private static bool From(this ptr<Boolean> _addr_k, label.Label t) {
    ref Boolean k = ref _addr_k.val;

    return t.Unpack64() > 0;
}

// Error represents a key
public partial struct Error {
    public @string name;
    public @string description;
}

// NewError creates a new Key for int64 values.
public static ptr<Error> NewError(@string name, @string description) {
    return addr(new Error(name:name,description:description));
}

private static @string Name(this ptr<Error> _addr_k) {
    ref Error k = ref _addr_k.val;

    return k.name;
}
private static @string Description(this ptr<Error> _addr_k) {
    ref Error k = ref _addr_k.val;

    return k.description;
}

private static void Format(this ptr<Error> _addr_k, io.Writer w, slice<byte> buf, label.Label l) {
    ref Error k = ref _addr_k.val;

    io.WriteString(w, k.From(l).Error());
}

// Of creates a new Label with this key and the supplied value.
private static label.Label Of(this ptr<Error> _addr_k, error v) {
    ref Error k = ref _addr_k.val;

    return label.OfValue(k, v);
}

// Get can be used to get a label for the key from a label.Map.
private static error Get(this ptr<Error> _addr_k, label.Map lm) {
    ref Error k = ref _addr_k.val;

    {
        var t = lm.Find(k);

        if (t.Valid()) {
            return error.As(k.From(t))!;
        }
    }

    return error.As(null!)!;

}

// From can be used to get a value from a Label.
private static error From(this ptr<Error> _addr_k, label.Label t) {
    ref Error k = ref _addr_k.val;

    error (err, _) = error.As(t.UnpackValue()._<error>())!;
    return error.As(err)!;
}

} // end keys_package
