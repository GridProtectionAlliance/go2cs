// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;
using System;
using System.Collections;
using System.Reflection;
using abi = go.@internal.abi_package;
using @unsafe = go.unsafe_package;

// Hand-finished conversion (the reflection bridge — Phase 4, value side). Go's reflect.Value reads the
// value through v.ptr as flat memory at computed field/element offsets — reinterpreting an interface's
// data word — which has no managed form. Instead, reflect.Value carries the boxed managed value
// DIRECTLY (a companion `partial struct Value { object boxed }` field), and the value-reader methods
// read it with System.Reflection + the golib container interfaces (IArray for slices/arrays, ж<T> for
// pointers). The entry (ValueOf/unpackEface) sets typ_ (the Phase-1 synthetic abi.Type, Kind_ from the
// managed System.Type) and the flag's Kind bits, so Kind()/IsValid()/CanAddr()/Type() keep working from
// value.cs. The converter skips these declarations via the manualConversionFuncs registry
// (go2cs/manualTypeOperations.go); this module marker also makes go2cs skip re-converting this file.
// INCREMENT 1: scalars, slices, arrays, pointers. Struct Field/NumField + map MapRange land next.
// See docs/Phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go;

partial class reflect_package {

// The managed backing for a Value: the boxed Go value this Value represents (null for the zero Value).
partial struct ΔValue {
    internal object? boxed;
}

// makeReflectValue builds a Value carrying a boxed managed value. typ_ is the Phase-1 synthetic
// abi.Type (Kind_ classified from the value's System.Type); the flag holds the Kind so Kind()/IsValid()
// resolve from value.cs unchanged.
internal static ΔValue makeReflectValue(object? boxed) {
    if (boxed is null) {
        return new ΔValue(nil);
    }
    var t = abi.TypeOf(boxed);
    var v = new ΔValue(t, default!, ((flag)(uintptr)(uint8)GoReflect.KindOf(boxed.GetType())));
    v.boxed = boxed;
    return v;
}

// ValueOf returns a new Value initialized to the concrete value stored in the interface i.
public static ΔValue ValueOf(any i) {
    return i == default! ? new ΔValue(nil) : makeReflectValue(i);
}

internal static ΔValue unpackEface(any i) {
    return ValueOf(i);
}

// Interface returns v's current value as an interface{}.
public static any /*i*/ Interface(this ΔValue v) {
    return v.boxed!;
}

internal static any /*i*/ valueInterface(ΔValue v, bool safe) {
    return v.boxed!;
}

public static bool Bool(this ΔValue v) {
    return (bool)v.boxed!;
}

public static int64 Int(this ΔValue v) {
    return numericValue(v.boxed) switch {
        nint n => (int64)n,
        int i => i,
        long l => l,
        short s => s,
        sbyte b => b,
        var n => System.Convert.ToInt64(n)
    };
}

public static uint64 Uint(this ΔValue v) {
    return numericValue(v.boxed) switch {
        nuint n => (uint64)n,
        uintptr up => (uint64)up.Value,
        uint u => u,
        ulong l => l,
        ushort s => s,
        byte b => b,
        var n => System.Convert.ToUInt64(n)
    };
}

public static float64 Float(this ΔValue v) {
    return System.Convert.ToDouble(numericValue(v.boxed));
}

// numericValue unwraps a NAMED numeric type (`type Celsius float64` → a [GoType("num:float64")] struct)
// to its underlying primitive so Int/Uint/Float can read it — a primitive (int/double/…) or golib
// uintptr is returned unchanged; a wrapper struct yields its single primitive field.
private static object? numericValue(object? boxed) {
    if (boxed is null || boxed.GetType().IsPrimitive || boxed is uintptr) {
        return boxed;
    }
    foreach (FieldInfo f in boxed.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
        object? val = f.GetValue(boxed);
        if (val is not null && (val.GetType().IsPrimitive || val is uintptr)) {
            return val;
        }
    }
    return boxed;
}

public static complex128 Complex(this ΔValue v) {
    return (complex128)v.boxed!;
}

public static @string String(this ΔValue v) {
    // fmt only calls String() for Kind String; a boxed @string returns itself, anything else the Go
    // "<T Value>" placeholder.
    if (v.boxed is @string s) {
        return s;
    }
    if (v.boxed is null) {
        return "<invalid Value>";
    }
    return (@string)("<" + v.Type().String().ToString() + " Value>");
}

// IsNil reports whether its argument v is nil (v must be a chan, func, interface, map, pointer, or slice).
public static bool IsNil(this ΔValue v) {
    switch (v.boxed) {
    case null:
        return true;
    case IMap m:
        return m.IsNil;
    default:
        // A pointer box (ж<T>) exposes IsNull; a nil vs empty slice both format identically under %v,
        // so a slice reports non-nil here.
        var isNull = v.boxed.GetType().GetProperty("IsNull");
        return isNull != null && isNull.PropertyType == typeof(bool) && (bool)isNull.GetValue(v.boxed)!;
    }
}

// Len returns v's length (v must be an Array, Chan, Map, Slice, String, or pointer-to-Array).
public static nint Len(this ΔValue v) {
    return v.boxed switch {
        @string s => s.Length,
        IArray a => a.Length,
        IMap m => m.Length,
        _ => 0
    };
}

// Index returns v's i'th element (v must be an Array, Slice, or String).
public static ΔValue Index(this ΔValue v, nint i) {
    if (v.boxed is IArray a) {
        return makeReflectValue(a[i]);
    }
    throw panic(Ꮡ(new ValueError("reflect.Value.Index", v.kind())));
}

// Elem returns the value that the interface v contains or that the pointer v points to.
public static ΔValue Elem(this ΔValue v) {
    ΔKind k = v.kind();
    if (k == ΔInterface) {
        return makeReflectValue(v.boxed);
    }
    if (k == ΔPointer) {
        if (v.boxed is null) {
            return new ΔValue(nil);
        }
        Type bt = v.boxed.GetType();
        var isNull = bt.GetProperty("IsNull");
        if (isNull != null && (bool)isNull.GetValue(v.boxed)!) {
            return new ΔValue(nil);
        }
        var valProp = bt.GetProperty("Value");
        return makeReflectValue(valProp?.GetValue(v.boxed));
    }
    throw panic(Ꮡ(new ValueError("reflect.Value.Elem", v.kind())));
}

// Bytes returns v's underlying value (v's underlying value must be a slice of bytes or an addressable array of bytes).
public static slice<byte> Bytes(this ΔValue v) {
    return (slice<byte>)v.boxed!;
}

// NumField returns the number of fields in the struct v.
public static nint NumField(this ΔValue v) {
    return v.boxed is null ? 0 : goStructFields(v.boxed.GetType()).Length;
}

// Field returns the i'th field of the struct v.
public static ΔValue Field(this ΔValue v, nint i) {
    if (v.boxed is null) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Field", v.kind())));
    }
    FieldInfo[] fields = goStructFields(v.boxed.GetType());
    if ((nuint)i >= (nuint)fields.Length) {
        throw panic("reflect: Field index out of range");
    }
    return makeReflectValue(fields[(int)i].GetValue(v.boxed));
}

// goStructFields returns the DECLARED Go fields of a [GoType] struct in source order. A converted
// struct emits each Go field as a C# instance field; the box/promotion machinery the TypeGenerator
// adds is static or property-shaped (never a plain instance field), except the reflection bridge's own
// `boxed` companion on Value — excluded by name.
private static FieldInfo[] goStructFields(Type t) {
    FieldInfo[] all = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    int n = 0;
    foreach (FieldInfo f in all) {
        if (isGoField(f)) {
            n++;
        }
    }
    var result = new FieldInfo[n];
    int j = 0;
    foreach (FieldInfo f in all) {
        if (isGoField(f)) {
            result[j++] = f;
        }
    }
    return result;
}

private static bool isGoField(FieldInfo f) {
    return !f.Name.Contains("k__BackingField") && f.Name != "boxed";
}

// UnsafePointer returns v's value as an unsafe.Pointer (v must be a Chan, Func, Map, Pointer, or
// UnsafePointer). A managed pointer (ж<T>) has no numeric address, so return a STABLE non-zero
// object-identity token for a non-nil pointer (opaque, like the guintptr manual model) and 0 for nil —
// fmt uses it only to test nil-ness (`f.UnsafePointer() != nil`) and to print an address for %p.
public static @unsafe.Pointer UnsafePointer(this ΔValue v) {
    return ((@unsafe.Pointer)reflectPointerToken(v));
}

// Pointer returns v's value as a uintptr (the deprecated form of UnsafePointer).
public static uintptr Pointer(this ΔValue v) {
    return reflectPointerToken(v);
}

private static uintptr reflectPointerToken(ΔValue v) {
    if (v.boxed is null) {
        return 0;
    }
    var isNull = v.boxed.GetType().GetProperty("IsNull");
    if (isNull != null && (bool)isNull.GetValue(v.boxed)!) {
        return 0;
    }
    return ((uintptr)(nuint)(uint)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(v.boxed));
}

// The managed backing for a MapIter: the map's enumerator (a golib map<K,V> enumerates as
// IEnumerable of KeyValuePair<K,V>). The Go hiter-based iteration has no managed form.
partial struct MapIter {
    internal IEnumerator? mapEnum;
}

// MapRange returns a range iterator for a map.
public static ж<MapIter> MapRange(this ΔValue v) {
    ref var it = ref heap<MapIter>(out var Ꮡit);
    if (v.boxed is IEnumerable e) {
        it.mapEnum = e.GetEnumerator();
    }
    return Ꮡit;
}

// Next advances the map iterator and reports whether there is another entry.
[GoRecv] public static bool Next(this ref MapIter iter) {
    return iter.mapEnum is not null && iter.mapEnum.MoveNext();
}

// Key returns the key of the iterator's current map entry.
[GoRecv] public static ΔValue Key(this ref MapIter iter) {
    object? cur = iter.mapEnum?.Current;
    return makeReflectValue(cur?.GetType().GetProperty("Key")?.GetValue(cur));
}

// Value returns the value of the iterator's current map entry.
[GoRecv] public static ΔValue Value(this ref MapIter iter) {
    object? cur = iter.mapEnum?.Current;
    return makeReflectValue(cur?.GetType().GetProperty("Value")?.GetValue(cur));
}

// ==== Type side: reflect.rtype's ΔType methods over the abi.Type's carried System.Type ====
// rtype wraps an abi.Type by value, so `Ꮡt.Value.t.sysType` is the managed System.Type the Phase-1
// synthType stamped on the descriptor. These bypass Go's name/offset resolution (resolveNameOff, a
// stub) entirely, deriving Go type info from System.Type via GoReflect.

// String returns the Go source type string (`main.Point`, `[]int`, `*T`) — the value of %T.
internal static @string String(this ж<rtype> Ꮡt) {
    return (@string)GoReflect.GoTypeName(Ꮡt.Value.t.sysType);
}

// Name returns the type's name within its package (empty for an unnamed composite).
internal static @string Name(this ж<rtype> Ꮡt) {
    System.Type? st = Ꮡt.Value.t.sysType;
    if (st is null || GoReflect.ElementType(st) is not null) {
        return "";
    }
    string full = GoReflect.GoTypeName(st);
    int dot = full.LastIndexOf('.');
    return (@string)(dot >= 0 ? full[(dot + 1)..] : full);
}

// Elem returns the element type of a slice/array/pointer/map/chan.
internal static ΔType Elem(this ж<rtype> Ꮡt) {
    return toType(abi.synthType(GoReflect.ElementType(Ꮡt.Value.t.sysType)));
}

// NumField returns the number of fields in a struct type.
internal static nint NumField(this ж<rtype> Ꮡt) {
    System.Type? st = Ꮡt.Value.t.sysType;
    return st is null ? 0 : goStructFields(st).Length;
}

// Field returns the i'th struct field's descriptor (fmt reads .Name for %+v).
internal static StructField Field(this ж<rtype> Ꮡt, nint i) {
    System.Type st = Ꮡt.Value.t.sysType!;
    FieldInfo f = goStructFields(st)[(int)i];
    return new StructField(
        Name: (@string)f.Name,
        Type: toType(abi.synthType(f.FieldType))
    );
}

} // end reflect_package
