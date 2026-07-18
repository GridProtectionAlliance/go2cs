// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Deep equality test via reflection
using go;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

// Hand-finished conversion (the reflection bridge — Phase 4, DeepEqual). Go's deepValueEqual keys its
// cycle-detection `visited` map on the values' internal data words (v.ptr / v.pointer()) — raw eface
// addresses that the managed bridge never populates, so the auto form NREs converting the null
// unsafe.Pointer slot (first operational hits: strings/bytes TestSplit/TestSplitAfter). The managed
// form recurses over the SAME boxed values the bridge Value carries (value_impl.cs) and keys cycle
// detection on managed reference identity instead: a pointer is its ж<T> box, a map is its backing
// Dictionary, and a slice is its backing array + window offset (Go keys on &s[0] — base plus offset).
// DeepEqual itself stays auto (it only uses the bridged ValueOf/Type/AreEqual); the converter skips
// only deepValueEqual via the manualConversionFuncs registry (go2cs/manualTypeOperations.go); this
// module marker also makes go2cs skip re-converting this file.
// See docs/Phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go;

partial class reflect_package {

// Tests for deep equality using reflected types. Mirrors Go's deepValueEqual over the bridge's boxed
// managed values. The map argument is Go's address-keyed visited map — unusable in the managed model
// (no data words); the managed recursion carries a reference-identity set instead, with the same
// semantics: all checks in progress are assumed true when re-encountered, and entries persist for the
// whole DeepEqual call.
internal static bool deepValueEqual(ΔValue v1, ΔValue v2, map<visit, bool> visited) {
    return deepValueEqualBoxed(v1, v2, new HashSet<visitPair>());
}

private static bool deepValueEqualBoxed(ΔValue v1, ΔValue v2, HashSet<visitPair> visited) {
    if (!v1.IsValid() || !v2.IsValid()) {
        return v1.IsValid() == v2.IsValid();
    }
    if (!AreEqual(v1.Type(), v2.Type())) {
        return false;
    }
    // Go's hard()/visited step: only pointer, map, and slice values can head a reference cycle in the
    // managed model (a bridge Value never has Kind Interface — the boxed value is always concrete).
    // Go also keys the visit on the Type; managed identity roots are per-variable objects with a fixed
    // type, so the (root1, root2) pair alone cannot collide across types.
    ΔKind kind = v1.Kind();
    if (kind == ΔPointer || kind == Map || kind == ΔSlice) {
        (object? root1, nint off1) = identityRoot(v1.boxed);
        (object? root2, nint off2) = identityRoot(v2.boxed);
        if (root1 is not null && root2 is not null && !visited.Add(new visitPair(root1, off1, root2, off2))) {
            // Already seen further up the recursion — the comparison algorithm assumes checks in
            // progress are true when it reencounters them (this is what makes DeepEqual terminate).
            return true;
        }
    }
    if (kind == Array) {
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqualBoxed(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == ΔSlice) {
        (object? data1, nint low1) = sliceData(v1.boxed);
        (object? data2, nint low2) = sliceData(v2.boxed);
        if (data1 is null != data2 is null) {
            // A nil slice (null backing — the golib `default`) and a non-nil empty slice are not
            // deeply equal, per the DeepEqual doc.
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        if (v1.Len() == 0) {
            return true;
        }
        if (ReferenceEquals(data1, data2) && low1 == low2) {
            // Same initial entry of the same underlying array (&x[0] == &y[0]).
            return true;
        }
        if (v1.boxed is slice<byte> b1 && v2.boxed is slice<byte> b2) {
            // Special case for []byte, which is common (Go routes this through bytealg.Equal).
            return b1.ToSpan().SequenceEqual(b2.ToSpan());
        }
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqualBoxed(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == ΔInterface) {
        if (v1.IsNil() || v2.IsNil()) {
            return v1.IsNil() == v2.IsNil();
        }
        return deepValueEqualBoxed(v1.Elem(), v2.Elem(), visited);
    }
    if (kind == ΔPointer) {
        if (ReferenceEquals(v1.boxed, v2.boxed)) {
            // Same ж<T> box — Go's same-address short-circuit (one box per variable).
            return true;
        }
        // Elem maps a nil box to the invalid Value, so two distinct nil pointers compare equal
        // through the invalid==invalid rule, and nil-vs-non-nil compares false — matching Go.
        return deepValueEqualBoxed(v1.Elem(), v2.Elem(), visited);
    }
    if (kind == Struct) {
        for ((nint i, nint n) = (0, v1.NumField()); i < n; i++) {
            if (!deepValueEqualBoxed(v1.Field(i), v2.Field(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == Map) {
        if (v1.IsNil() != v2.IsNil()) {
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        IDictionary? m1 = mapBacking(v1.boxed);
        IDictionary? m2 = mapBacking(v2.boxed);
        if (ReferenceEquals(m1, m2)) {
            // The same map object (or both nil) — deeply equal regardless of content.
            return true;
        }
        if (m1 is null || m2 is null) {
            return m1 is null == m2 is null;
        }
        foreach (DictionaryEntry entry in m1) {
            if (!m2.Contains(entry.Key)) {
                // Go: MapIndex yields the invalid Value for a missing key → not equal.
                return false;
            }
            // Two stored nil interface values recurse to invalid==invalid → equal, matching Go.
            if (!deepValueEqualBoxed(makeReflectValue(entry.Value), makeReflectValue(m2[entry.Key]), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == Func) {
        // Func values are deeply equal only if both are nil — a nil func boxed as `any` is the null
        // object, so two nils already matched through the invalid==invalid rule above; two non-nil
        // funcs are never deeply equal (even the same func).
        return false;
    }
    if (kind == ΔInt || kind == Int8 || kind == Int16 || kind == Int32 || kind == Int64) {
        return v1.Int() == v2.Int();
    }
    if (kind == ΔUint || kind == Uint8 || kind == Uint16 || kind == Uint32 || kind == Uint64 || kind == Uintptr) {
        return v1.Uint() == v2.Uint();
    }
    if (kind == ΔString) {
        return v1.String() == v2.String();
    }
    if (kind == ΔBool) {
        return v1.Bool() == v2.Bool();
    }
    if (kind == Float32 || kind == Float64) {
        // C# double == carries IEEE semantics: a NaN is not equal to itself, exactly like Go.
        return v1.Float() == v2.Float();
    }
    if (kind == Complex64 || kind == Complex128) {
        return v1.Complex() == v2.Complex();
    }
    { /* default: */
        // Can't do better than this: normal equality suffices.
        return AreEqual(valueInterface(v1, false), valueInterface(v2, false));
    }
}

// A visited entry: the identity roots of two values under in-progress comparison, compared by managed
// reference identity plus the slice window offset (Go keys on the data addresses; a pointer's root is
// its ж<T> box, a map's its backing Dictionary, a slice's its backing array + Low).
private readonly struct visitPair(object a1, nint off1, object a2, nint off2) : IEquatable<visitPair> {
    private readonly object m_a1 = a1;
    private readonly nint m_off1 = off1;
    private readonly object m_a2 = a2;
    private readonly nint m_off2 = off2;

    public bool Equals(visitPair other) {
        return ReferenceEquals(m_a1, other.m_a1) && ReferenceEquals(m_a2, other.m_a2) &&
               m_off1 == other.m_off1 && m_off2 == other.m_off2;
    }

    public override bool Equals(object? obj) {
        return obj is visitPair other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(RuntimeHelpers.GetHashCode(m_a1), m_off1, RuntimeHelpers.GetHashCode(m_a2), m_off2);
    }
}

// identityRoot returns the managed object that stands for a value's Go data address, for cycle
// detection: a pointer's ж<T> box, a map's backing Dictionary, a slice's backing array + Low. A nil
// value (null box, IsNull pointer, null backing) has no root — Go never puts nil in the visited map.
private static (object? root, nint offset) identityRoot(object? boxed) {
    switch (boxed) {
        case null:
            return (null, 0);
        case ISlice:
            return sliceData(boxed);
        case IMap:
            return (mapBacking(boxed), 0);
        default:
            var isNull = boxed.GetType().GetProperty("IsNull");
            if (isNull != null && isNull.PropertyType == typeof(bool) && (bool)isNull.GetValue(boxed)!) {
                return (null, 0);
            }
            return (boxed, 0);
    }
}

// Per-closed-generic-type accessors for the REAL backing store of a boxed golib container. slice<T>'s
// public Source materializes a detached copy, so identity (and nil-ness — a nil slice is the golib
// `default`, null m_array) must come from the actual m_array/m_low fields; map<K,V> likewise only
// exposes its Dictionary internally. Field reads are cached per type.
private static readonly ConcurrentDictionary<System.Type, (FieldInfo? array, FieldInfo? low)> s_sliceFields = new();
private static readonly ConcurrentDictionary<System.Type, FieldInfo?> s_mapField = new();

// sliceData returns a boxed slice's backing array and window offset — (null, 0) for the nil slice.
private static (object? data, nint low) sliceData(object? boxed) {
    if (boxed is null) {
        return (null, 0);
    }
    (FieldInfo? array, FieldInfo? low) = s_sliceFields.GetOrAdd(boxed.GetType(), static t =>
        (t.GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic),
         t.GetField("m_low", BindingFlags.Instance | BindingFlags.NonPublic)));
    if (array is null) {
        return (null, 0);
    }
    object? data = array.GetValue(boxed);
    return data is null ? (null, 0) : (data, low is null ? 0 : (nint)low.GetValue(boxed)!);
}

// mapBacking returns a boxed map's backing Dictionary — null for the nil map (no backing store).
private static IDictionary? mapBacking(object? boxed) {
    if (boxed is null) {
        return null;
    }
    FieldInfo? field = s_mapField.GetOrAdd(boxed.GetType(), static t => {
        foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
            if (typeof(IDictionary).IsAssignableFrom(f.FieldType)) {
                return f;
            }
        }
        return null;
    });
    return field?.GetValue(boxed) as IDictionary;
}

} // end reflect_package
