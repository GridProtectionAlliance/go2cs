// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Deep equality test via reflection

// package reflect -- go2cs converted at 2020 August 29 08:16:04 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Go\src\reflect\deepequal.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class reflect_package
    {
        // During deepValueEqual, must keep track of checks that are
        // in progress. The comparison algorithm assumes that all
        // checks in progress are true when it reencounters them.
        // Visited comparisons are stored in a map indexed by visit.
        private partial struct visit
        {
            public unsafe.Pointer a1;
            public unsafe.Pointer a2;
            public Type typ;
        }

        // Tests for deep equality using reflected types. The map argument tracks
        // comparisons that have already been seen, which allows short circuiting on
        // recursive types.
        private static bool deepValueEqual(Value v1, Value v2, map<visit, bool> visited, long depth)
        {
            if (!v1.IsValid() || !v2.IsValid())
            {
                return v1.IsValid() == v2.IsValid();
            }
            if (v1.Type() != v2.Type())
            {
                return false;
            } 

            // if depth > 10 { panic("deepValueEqual") }    // for debugging

            // We want to avoid putting more in the visited map than we need to.
            // For any possible reference cycle that might be encountered,
            // hard(t) needs to return true for at least one of the types in the cycle.
            Func<Kind, bool> hard = k =>
            {

                if (k == Map || k == Slice || k == Ptr || k == Interface) 
                    return true;
                                return false;
            }
;

            if (v1.CanAddr() && v2.CanAddr() && hard(v1.Kind()))
            {
                var addr1 = @unsafe.Pointer(v1.UnsafeAddr());
                var addr2 = @unsafe.Pointer(v2.UnsafeAddr());
                if (uintptr(addr1) > uintptr(addr2))
                { 
                    // Canonicalize order to reduce number of entries in visited.
                    // Assumes non-moving garbage collector.
                    addr1 = addr2;
                    addr2 = addr1;
                } 

                // Short circuit if references are already seen.
                var typ = v1.Type();
                visit v = new visit(addr1,addr2,typ);
                if (visited[v])
                {
                    return true;
                } 

                // Remember for later.
                visited[v] = true;
            }

            if (v1.Kind() == Array) 
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < v1.Len(); i++)
                    {
                        if (!deepValueEqual(v1.Index(i), v2.Index(i), visited, depth + 1L))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (v1.Kind() == Slice) 
                if (v1.IsNil() != v2.IsNil())
                {
                    return false;
                }
                if (v1.Len() != v2.Len())
                {
                    return false;
                }
                if (v1.Pointer() == v2.Pointer())
                {
                    return true;
                }
                {
                    long i__prev1 = i;

                    for (i = 0L; i < v1.Len(); i++)
                    {
                        if (!deepValueEqual(v1.Index(i), v2.Index(i), visited, depth + 1L))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (v1.Kind() == Interface) 
                if (v1.IsNil() || v2.IsNil())
                {
                    return v1.IsNil() == v2.IsNil();
                }
                return deepValueEqual(v1.Elem(), v2.Elem(), visited, depth + 1L);
            else if (v1.Kind() == Ptr) 
                if (v1.Pointer() == v2.Pointer())
                {
                    return true;
                }
                return deepValueEqual(v1.Elem(), v2.Elem(), visited, depth + 1L);
            else if (v1.Kind() == Struct) 
                {
                    long i__prev1 = i;

                    for (i = 0L;
                    var n = v1.NumField(); i < n; i++)
                    {
                        if (!deepValueEqual(v1.Field(i), v2.Field(i), visited, depth + 1L))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (v1.Kind() == Map) 
                if (v1.IsNil() != v2.IsNil())
                {
                    return false;
                }
                if (v1.Len() != v2.Len())
                {
                    return false;
                }
                if (v1.Pointer() == v2.Pointer())
                {
                    return true;
                }
                foreach (var (_, k) in v1.MapKeys())
                {
                    var val1 = v1.MapIndex(k);
                    var val2 = v2.MapIndex(k);
                    if (!val1.IsValid() || !val2.IsValid() || !deepValueEqual(v1.MapIndex(k), v2.MapIndex(k), visited, depth + 1L))
                    {
                        return false;
                    }
                }
                return true;
            else if (v1.Kind() == Func) 
                if (v1.IsNil() && v2.IsNil())
                {
                    return true;
                } 
                // Can't do better than this:
                return false;
            else 
                // Normal equality suffices
                return valueInterface(v1, false) == valueInterface(v2, false);
                    }

        // DeepEqual reports whether x and y are ``deeply equal,'' defined as follows.
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
        public static bool DeepEqual(object x, object y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            var v1 = ValueOf(x);
            var v2 = ValueOf(y);
            if (v1.Type() != v2.Type())
            {
                return false;
            }
            return deepValueEqual(v1, v2, make_map<visit, bool>(), 0L);
        }
    }
}
