// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Deep equality test via reflection
namespace go;

using bytealg = @internal.bytealg_package;
using @unsafe = unsafe_package;
using @internal;

partial class reflect_package {

// During deepValueEqual, must keep track of checks that are
// in progress. The comparison algorithm assumes that all
// checks in progress are true when it reencounters them.
// Visited comparisons are stored in a map indexed by visit.
[GoType] partial struct visit {
    internal @unsafe.Pointer a1;
    internal @unsafe.Pointer a2;
    internal ΔType typ;
}

// Tests for deep equality using reflected types. The map argument tracks
// comparisons that have already been seen, which allows short circuiting on
// recursive types.
internal static bool deepValueEqual(ΔValue v1, ΔValue v2, map<visit, bool> visited) {
    if (!v1.IsValid() || !v2.IsValid()) {
        return v1.IsValid() == v2.IsValid();
    }
    if (!AreEqual(v1.Type(), v2.Type())) {
        return false;
    }
    // We want to avoid putting more in the visited map than we need to.
    // For any possible reference cycle that might be encountered,
    // hard(v1, v2) needs to return true for at least one of the types in the cycle,
    // and it's safe and valid to get Value's internal pointer.
    var hard = (ΔValue v1, ΔValue v2) => {
        var exprᴛ1 = v1Δ1.Kind();
        var matchᴛ1 = false;
        if (exprᴛ1 == ΔPointer) {
            if (!v1Δ1.typ().Pointers()) {
                // not-in-heap pointers can't be cyclic.
                // At least, all of our current uses of runtime/internal/sys.NotInHeap
                // have that property. The runtime ones aren't cyclic (and we don't use
                // DeepEqual on them anyway), and the cgo-generated ones are
                // all empty structs.
                return false;
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (exprᴛ1 == Map || exprᴛ1 == ΔSlice || exprᴛ1 == ΔInterface)) { matchᴛ1 = true;
            return !v1Δ1.IsNil() && !v2Δ1.IsNil();
        }

        // Nil pointers cannot be cyclic. Avoid putting them in the visited map.
        return false;
    };
    if (hard(v1, v2)) {
        // For a Pointer or Map value, we need to check flagIndir,
        // which we do by calling the pointer method.
        // For Slice or Interface, flagIndir is always set,
        // and using v.ptr suffices.
        var ptrval = (ΔValue v) => {
            var exprᴛ2 = v.Kind();
            if (exprᴛ2 == ΔPointer || exprᴛ2 == Map) {
                return (uintptr)v.pointer();
            }
            { /* default: */
                return v.ptr;
            }

        };
        @unsafe.Pointer addr1 = (uintptr)ptrval(v1);
        @unsafe.Pointer addr2 = (uintptr)ptrval(v2);
        if (((uintptr)addr1) > ((uintptr)addr2)) {
            // Canonicalize order to reduce number of entries in visited.
            // Assumes non-moving garbage collector.
            (addr1, addr2) = (addr2, addr1);
        }
        // Short circuit if references are already seen.
        var typ = v1.Type();
        var v = new visit(addr1.val, addr2.val, typ);
        if (visited[v]) {
            return true;
        }
        // Remember for later.
        visited[v] = true;
    }
    var exprᴛ3 = v1.Kind();
    if (exprᴛ3 == Array) {
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqual(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ3 == ΔSlice) {
        if (v1.IsNil() != v2.IsNil()) {
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        if ((uintptr)v1.UnsafePointer() == (uintptr)v2.UnsafePointer()) {
            return true;
        }
        if (v1.Type().Elem().Kind() == Uint8) {
            // Special case for []byte, which is common.
            return bytealg.Equal(v1.Bytes(), v2.Bytes());
        }
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqual(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ3 == ΔInterface) {
        if (v1.IsNil() || v2.IsNil()) {
            return v1.IsNil() == v2.IsNil();
        }
        return deepValueEqual(v1.Elem(), v2.Elem(), visited);
    }
    if (exprᴛ3 == ΔPointer) {
        if ((uintptr)v1.UnsafePointer() == (uintptr)v2.UnsafePointer()) {
            return true;
        }
        return deepValueEqual(v1.Elem(), v2.Elem(), visited);
    }
    if (exprᴛ3 == Struct) {
        for (nint i = 0;nint n = v1.NumField(); i < n; i++) {
            if (!deepValueEqual(v1.Field(i), v2.Field(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ3 == Map) {
        if (v1.IsNil() != v2.IsNil()) {
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        if ((uintptr)v1.UnsafePointer() == (uintptr)v2.UnsafePointer()) {
            return true;
        }
        var iter = v1.MapRange();
        while (iter.Next()) {
            var val1 = iter.Value();
            var val2 = v2.MapIndex(iter.Key());
            if (!val1.IsValid() || !val2.IsValid() || !deepValueEqual(val1, val2, visited)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ3 == Func) {
        if (v1.IsNil() && v2.IsNil()) {
            return true;
        }
        return false;
    }
    if (exprᴛ3 == ΔInt || exprᴛ3 == Int8 || exprᴛ3 == Int16 || exprᴛ3 == Int32 || exprᴛ3 == Int64) {
        return v1.Int() == v2.Int();
    }
    if (exprᴛ3 == ΔUint || exprᴛ3 == Uint8 || exprᴛ3 == Uint16 || exprᴛ3 == Uint32 || exprᴛ3 == Uint64 || exprᴛ3 == Uintptr) {
        return v1.Uint() == v2.Uint();
    }
    if (exprᴛ3 == ΔString) {
        return v1.String() == v2.String();
    }
    if (exprᴛ3 == ΔBool) {
        return v1.Bool() == v2.Bool();
    }
    if (exprᴛ3 == Float32 || exprᴛ3 == Float64) {
        return v1.Float() == v2.Float();
    }
    if (exprᴛ3 == Complex64 || exprᴛ3 == Complex128) {
        return v1.Complex() == v2.Complex();
    }
    { /* default: */
        return AreEqual(valueInterface(v1, // Can't do better than this:
 // Normal equality suffices
 false), valueInterface(v2, false));
    }

}

// DeepEqual reports whether x and y are “deeply equal,” defined as follows.
// Two values of identical type are deeply equal if one of the following cases applies.
// Values of distinct types are never deeply equal.
//
// Array values are deeply equal when their corresponding elements are deeply equal.
//
// Struct values are deeply equal if their corresponding fields,
// both exported and unexported, are deeply equal.
//
// Func values are deeply equal if both are nil; otherwise they are not deeply equal.
//
// Interface values are deeply equal if they hold deeply equal concrete values.
//
// Map values are deeply equal when all of the following are true:
// they are both nil or both non-nil, they have the same length,
// and either they are the same map object or their corresponding keys
// (matched using Go equality) map to deeply equal values.
//
// Pointer values are deeply equal if they are equal using Go's == operator
// or if they point to deeply equal values.
//
// Slice values are deeply equal when all of the following are true:
// they are both nil or both non-nil, they have the same length,
// and either they point to the same initial entry of the same underlying array
// (that is, &x[0] == &y[0]) or their corresponding elements (up to length) are deeply equal.
// Note that a non-nil empty slice and a nil slice (for example, []byte{} and []byte(nil))
// are not deeply equal.
//
// Other values - numbers, bools, strings, and channels - are deeply equal
// if they are equal using Go's == operator.
//
// In general DeepEqual is a recursive relaxation of Go's == operator.
// However, this idea is impossible to implement without some inconsistency.
// Specifically, it is possible for a value to be unequal to itself,
// either because it is of func type (uncomparable in general)
// or because it is a floating-point NaN value (not equal to itself in floating-point comparison),
// or because it is an array, struct, or interface containing
// such a value.
// On the other hand, pointer values are always equal to themselves,
// even if they point at or contain such problematic values,
// because they compare equal using Go's == operator, and that
// is a sufficient condition to be deeply equal, regardless of content.
// DeepEqual has been defined so that the same short-cut applies
// to slices and maps: if x and y are the same slice or the same map,
// they are deeply equal regardless of content.
//
// As DeepEqual traverses the data values it may find a cycle. The
// second and subsequent times that DeepEqual compares two pointer
// values that have been compared before, it treats the values as
// equal rather than examining the values to which they point.
// This ensures that DeepEqual terminates.
public static bool DeepEqual(any x, any y) {
    if (x == default! || y == default!) {
        return AreEqual(x, y);
    }
    var v1 = ValueOf(x);
    var v2 = ValueOf(y);
    if (!AreEqual(v1.Type(), v2.Type())) {
        return false;
    }
    return deepValueEqual(v1, v2, new map<visit, bool>());
}

} // end reflect_package
