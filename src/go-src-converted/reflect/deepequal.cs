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

// go2cs generated this placeholder — func deepValueEqual is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// We want to avoid putting more in the visited map than we need to.
// For any possible reference cycle that might be encountered,
// hard(v1, v2) needs to return true for at least one of the types in the cycle,
// and it's safe and valid to get Value's internal pointer.
// not-in-heap pointers can't be cyclic.
// At least, all of our current uses of runtime/internal/sys.NotInHeap
// have that property. The runtime ones aren't cyclic (and we don't use
// DeepEqual on them anyway), and the cgo-generated ones are
// all empty structs.
// Nil pointers cannot be cyclic. Avoid putting them in the visited map.
// For a Pointer or Map value, we need to check flagIndir,
// which we do by calling the pointer method.
// For Slice or Interface, flagIndir is always set,
// and using v.ptr suffices.
// Canonicalize order to reduce number of entries in visited.
// Assumes non-moving garbage collector.
// Short circuit if references are already seen.
// Remember for later.
// Special case for []byte, which is common.
// Can't do better than this:
// Normal equality suffices

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
