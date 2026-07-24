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
// managed System.Type) and the flag's Kind bits, so Kind()/IsValid()/CanAddr() keep working from
// value.cs (Type() is hand-owned below so it returns a CANONICAL reflect.Type). The converter skips
// these declarations via the manualConversionFuncs registry
// (go2cs/manualTypeOperations.go); this module marker also makes go2cs skip re-converting this file.
// INCREMENT 1: scalars, slices, arrays, pointers. Struct Field/NumField + map MapRange land next.
// See docs/Phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go;

partial class reflect_package {

// The managed backing for a Value: the boxed Go value this Value represents (null for the zero
// Value — or for a VALID typed-nil/nil-interface Value, distinguished by typ_/flag being set),
// plus, when the Value is ADDRESSABLE (flagAddr), the ж<T> box it ALIASES: every read goes
// through the box lazily (a write through another alias of the same box — poser.As's direct
// `x.Value = …` — must be visible to a later Interface() read), and Set writes through it.
partial struct ΔValue {
    internal object? boxed;
    internal object? addrBox;

    // The LIVE value this Value represents (read-through for an addressable Value).
    internal object? live => addrBox is null ? boxed : GoReflect.ReadPointerSlot(addrBox);
}

// makeReflectValue builds a Value carrying a boxed managed value. typ_ is the Phase-1 synthetic
// abi.Type (Kind_ classified from the value's System.Type); the flag holds the Kind so Kind()/IsValid()
// resolve from value.cs unchanged.
internal static ΔValue makeReflectValue(object? boxed) {
    if (boxed is null) {
        return new ΔValue(nil);
    }
    var t = abi.TypeOf(boxed);
    var v = new ΔValue(t, default!, ((flag)(uintptr)(uint8)GoReflect.KindOf(GoReflect.GoDynamicTypeOf(boxed))));
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

// Interface returns v's current value as an interface{}. A valid typed-nil pointer Value
// yields its canonical nil box — a NON-nil `any` holding `(*T)(nil)`, exactly Go's packEface
// (the type is never erased to a bare null one call after X2 restored it).
public static any /*i*/ Interface(this ΔValue v) {
    return v.live!;
}

internal static any /*i*/ valueInterface(ΔValue v, bool safe) {
    return v.live!;
}

public static bool Bool(this ΔValue v) {
    return (bool)v.live!;
}

public static int64 Int(this ΔValue v) {
    return numericValue(v.live) switch {
        nint n => (int64)n,
        int i => i,
        long l => l,
        short s => s,
        sbyte b => b,
        var n => System.Convert.ToInt64(n)
    };
}

public static uint64 Uint(this ΔValue v) {
    return numericValue(v.live) switch {
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
    return System.Convert.ToDouble(numericValue(v.live));
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
    return (complex128)v.live!;
}

public static @string String(this ΔValue v) {
    // fmt only calls String() for Kind String; a boxed @string returns itself, anything else the Go
    // "<T Value>" placeholder.
    if (v.live is @string s) {
        return s;
    }
    if (v.live is null) {
        return "<invalid Value>";
    }
    return (@string)("<" + v.Type().String().ToString() + " Value>");
}

// IsNil reports whether its argument v is nil (v must be a chan, func, interface, map, pointer, or slice).
// STRUCTURAL nil for pointers (INilPointer — the canonical typed-nil form): a heap box holding a
// nil value is a NON-nil pointer holding nil, and an adapter-held *T asks its receiver box.
public static bool IsNil(this ΔValue v) {
    object? cur = v.live;
    while (cur is IInterfaceAdapter { Value: not null } interfaceAdapter) {
        cur = interfaceAdapter.Value;
    }
    if (cur is IжAdapter { Box: not null } pointerAdapter) {
        cur = pointerAdapter.Box;
    }
    switch (cur) {
    case null:
        return true;
    case INilPointer nilable:
        return nilable.IsNilPointer;
    case IMap m:
        return m.IsNil;
    default:
        return false;
    }
}

// Len returns v's length (v must be an Array, Chan, Map, Slice, String, or pointer-to-Array).
public static nint Len(this ΔValue v) {
    return v.live switch {
        @string s => s.Length,
        IArray a => a.Length,
        IMap m => m.Length,
        _ => 0
    };
}

// Index returns v's i'th element (v must be an Array, Slice, or String).
public static ΔValue Index(this ΔValue v, nint i) {
    if (v.live is IArray a) {
        return makeReflectValue(a[i]);
    }
    throw panic(Ꮡ(new ValueError("reflect.Value.Index", v.kind())));
}

// Elem returns the value that the interface v contains or that the pointer v points to.
// The pointer form returns an ADDRESSABLE Value ALIASING the receiver box (Go: "the returned
// value's address is v's value") — reads go through the box lazily and Set writes through it.
// An adapter-held *T aliases the adapter's receiver box; a structurally nil pointer yields the
// invalid zero Value (Go).
public static ΔValue Elem(this ΔValue v) {
    ΔKind k = v.kind();
    if (k == ΔInterface) {
        return makeReflectValue(v.live);
    }
    if (k == ΔPointer) {
        object? cur = v.live;
        while (cur is IInterfaceAdapter { Value: not null } interfaceAdapter) {
            cur = interfaceAdapter.Value;
        }
        if (cur is IжAdapter { Box: not null } pointerAdapter) {
            cur = pointerAdapter.Box;
        }
        if (cur is null || (cur is INilPointer nilable && nilable.IsNilPointer)) {
            return new ΔValue(nil);
        }
        Type? pointee = GoReflect.ElementType(cur.GetType());
        if (pointee is null) {
            // Not a ж<T>-shaped box (a named-pointer wrapper — increment 2): fall back to a
            // detached read so existing readers keep working.
            return makeReflectValue(GoReflect.ReadPointerSlot(cur));
        }
        var t = abi.synthType(pointee);
        var elem = new ΔValue(t, default!, ((flag)(uintptr)(uint8)GoReflect.KindOf(pointee)) | flagAddr | flagIndir);
        elem.addrBox = cur;
        return elem;
    }
    throw panic(Ꮡ(new ValueError("reflect.Value.Elem", v.kind())));
}

// Bytes returns v's underlying value (v's underlying value must be a slice of bytes or an addressable array of bytes).
public static slice<byte> Bytes(this ΔValue v) {
    return (slice<byte>)v.live!;
}

// NumField returns the number of fields in the struct v.
public static nint NumField(this ΔValue v) {
    return v.live is null ? 0 : goStructFields(v.live.GetType()).Length;
}

// Field returns the i'th field of the struct v.
public static ΔValue Field(this ΔValue v, nint i) {
    object? cur = v.live;
    if (cur is null) {
        throw panic(Ꮡ(new ValueError("reflect.Value.Field", v.kind())));
    }
    FieldInfo[] fields = goStructFields(cur.GetType());
    if ((nuint)i >= (nuint)fields.Length) {
        throw panic("reflect: Field index out of range");
    }
    return makeReflectValue(fields[(int)i].GetValue(cur));
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
    object? cur = v.live;
    while (cur is IInterfaceAdapter { Value: not null } interfaceAdapter) {
        cur = interfaceAdapter.Value;
    }
    if (cur is IжAdapter { Box: not null } pointerAdapter) {
        cur = pointerAdapter.Box;
    }
    if (cur is null || (cur is INilPointer nilable && nilable.IsNilPointer)) {
        return 0;
    }
    return ((uintptr)(nuint)(uint)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(cur));
}

// The managed backing for a MapIter: the map's enumerator (a golib map<K,V> enumerates as
// IEnumerable of KeyValuePair<K,V>). The Go hiter-based iteration has no managed form.
partial struct MapIter {
    internal IEnumerator? mapEnum;
}

// MapRange returns a range iterator for a map.
public static ж<MapIter> MapRange(this ΔValue v) {
    ref var it = ref heap<MapIter>(out var Ꮡit);
    if (v.live is IEnumerable e) {
        it.mapEnum = e.GetEnumerator();
    }
    return Ꮡit;
}

// ==== Phase-3 write-back: Set, Zero, methodName ====

// Set assigns x to the value v (v must be addressable and x assignable to v's type — Go's
// assignTo). Marshalling and the assignability decision share the golib machinery emitted
// asserts use (GoReflect.TryMarshalAssignable): identity — with adapter/box unwrap, so an
// interface-held *T stores its receiver box — or interface-implements, where a typed-nil
// pointer source stores its canonical nil box wrapped for the destination (a NON-nil interface
// holding (*T)(nil), Go's packEface result). The store writes through the aliased ж box's slot
// ref; a structurally nil box panics Go-style before any write (blessing condition Q1a).
public static void Set(this ΔValue v, ΔValue x) {
    v.flag.mustBeAssignable();
    x.flag.mustBeExported();
    System.Type? dstType = v.typ_ == nil ? null : v.typ_.Value.sysType;
    if (dstType is null || v.addrBox is null) {
        throw panic("reflect: Set using unaddressable value");
    }
    if (!GoReflect.TryMarshalAssignable(x.live, dstType, out object? marshalled)) {
        throw panic("reflect.Set: value of type " + GoReflect.GoTypeName(x.live?.GetType()) +
                    " is not assignable to type " + GoReflect.GoTypeName(dstType));
    }
    GoReflect.WritePointerSlot(v.addrBox, marshalled);
}

// Zero returns a Value representing the zero value for the specified type. A pointer kind
// yields a VALID typed-nil Value whose boxed value is the type's canonical nil box (one nil
// encoding system-wide — Interface() of it is a non-nil any holding (*T)(nil)); interface and
// func kinds a valid nil Value (boxed null); value kinds a zero instance.
public static ΔValue Zero(ΔType typ) {
    if (typ == default!) {
        throw panic("reflect: Zero(nil)");
    }
    System.Type? st = sysTypeOfReflectType(typ);
    if (st is null) {
        throw panic("reflect: Zero of non-synthesized type");
    }
    int kind = GoReflect.KindOf(st);
    var t = abi.synthType(st);
    var zero = new ΔValue(t, default!, ((flag)(uintptr)(uint8)kind));
    switch (kind) {
    case GoReflect.Pointer:
        zero.boxed = GoReflect.CanonicalNilPointer(st);
        break;
    case GoReflect.Interface:
    case GoReflect.Func:
        zero.boxed = null;
        break;
    case GoReflect.String:
        zero.boxed = (@string)"";
        break;
    default:
        // Value kinds (numerics, structs, arrays) and the nil-able container structs
        // (slice/map/chan wrappers) — a default instance IS the Go zero value.
        zero.boxed = System.Activator.CreateInstance(st);
        break;
    }
    return zero;
}

// sysTypeOfReflectType recovers the managed System.Type a canonical reflect.Type wrapper
// describes (the rtype's abi.Type carries it — synthType stamped it).
private static System.Type? sysTypeOfReflectType(ΔType typ) {
    var (rt, ok) = typ._<ж<rtype>>(ᐧ);
    return ok && rt != nil ? rt.Value.t.sysType : null;
}

// methodName returns a best-effort Go-shaped name of the calling reflect method for panic
// messages ("reflect.Value.Set using unaddressable value"). Go resolves it from the PC via
// runtime.Caller — unimplementable here (no Go stack); walk the managed stack to the first
// converted-package frame instead. The name is only ever observed in panic text.
internal static @string methodName() {
    var trace = new System.Diagnostics.StackTrace(2, false);
    for (int i = 0; i < trace.FrameCount; i++) {
        var method = trace.GetFrame(i)?.GetMethod();
        System.Type? decl = method?.DeclaringType;
        if (method is null || decl is null) {
            continue;
        }
        if (decl.Name.EndsWith("_package") && !method.Name.StartsWith("mustBe")) {
            return (@string)(decl.Name[..^"_package".Length] + "." + method.Name);
        }
    }
    return "unknown method"u8;
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

// ==== reflect.Type canonicalization (hand-owned Value.Type + toType) ====
// Go's reflect.Type is a canonical interned descriptor: TypeOf(x) == TypeOf(y) exactly when x and y
// have the same dynamic type, so `aType == bType` is a pointer compare that internal/fmtsort.compare
// relies on (`if aType != bType { return -1 }`). The managed bridge synthesizes a fresh abi.Type box
// per TypeOf call and wraps it in a fresh rtypeжΔType (an IжAdapter compared by box identity), so two
// Types describing the same Go type never compared equal — compare() always returned -1 and the stable
// sort REVERSED the map keys (map[b:2 a:1] instead of map[a:1 b:2]). Intern the ΔType wrapper by the
// underlying System.Type so identity-equality matches Go. The cache is process-lifetime (type
// descriptors are permanent, exactly like Go's). See docs/Phase4/DESIGN-reflection-bridge.md.
private static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Type, ΔType> s_canonTypeCache = new();

// canonType returns the canonical reflect.Type wrapper for the underlying type of Ꮡt (keyed by the
// managed System.Type synthType stamped on the abi.Type). A nil descriptor maps to the nil Type; a
// descriptor with no System.Type (never synthesized) falls back to a fresh, uninterned wrapper.
internal static ΔType canonType(ж<abi.Type> Ꮡt) {
    if (Ꮡt == nil) {
        return default!;
    }
    System.Type? st = Ꮡt.Value.sysType;
    if (st is null) {
        // No System.Type stamped on the descriptor: the feeding path did not go through
        // abi.synthType. Such a wrapper is UN-interned — it would compare unequal to the
        // canonical Type for the same Go type, silently reintroducing the reversed-map-sort
        // bug this file fixes. This branch is dead today (synthType always stamps sysType
        // after its own nil guard, and every canonType caller feeds a synthType/abi.TypeOf
        // box or nil), so assert to surface a future non-canonical feeder LOUDLY in dev
        // (Debug builds) while still degrading gracefully in Release rather than crashing.
        System.Diagnostics.Debug.Assert(false,
            "reflect.canonType: abi.Type has no System.Type (synthType was bypassed); the " +
            "resulting reflect.Type is non-canonical. Route the feeding path through abi.synthType.");
        return new rtypeжΔType(toRType(Ꮡt));
    }
    return s_canonTypeCache.GetOrAdd(st, _ => new rtypeжΔType(toRType(Ꮡt)));
}

// Type returns v's type. Hand-owned so the common (non-method) fast path returns the CANONICAL Type
// (canonType); the method-value path stays in the auto typeSlow. Mirrors the auto Value.Type shape.
public static ΔType Type(this ΔValue v) {
    if (v.flag != 0 && (flag)(v.flag & flagMethod) == 0) {
        return canonType(v.typ_);
    }
    return v.typeSlow();
}

// toType converts a *rtype to a client-facing reflect.Type, coalescing multiple descriptors for the
// same underlying type into a single canonical Type (Go's gc interns descriptors; the managed bridge
// interns here). Hand-owned so reflect.TypeOf routes through canonType. The hand-owned rtype.Elem/
// Field re-synthesize their element/field descriptor via abi.synthType and route here too, so they
// are canonical as well. NOTE: rtype.In/Out/Key also call toType, but they read func/map sub-
// descriptors that synthType never populates, so they currently NRE / return the nil Type — an
// unimplemented bridge gap, NOT canonical (tracked separately); do not rely on their identity.
internal static ΔType toType(ж<abi.Type> Ꮡt) {
    return canonType(Ꮡt);
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
