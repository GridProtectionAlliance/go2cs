// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fmtsort provides a general stable ordering mechanism
// for maps, on behalf of the fmt and text/template packages.
// It is not guaranteed to be efficient and works only for types
// that are valid map keys.
// package fmtsort -- go2cs converted at 2020 October 08 03:26:03 UTC
// import "internal/fmtsort" ==> using fmtsort = go.@internal.fmtsort_package
// Original source: C:\Go\src\internal\fmtsort\sort.go
using reflect = go.reflect_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class fmtsort_package
    {
        // Note: Throughout this package we avoid calling reflect.Value.Interface as
        // it is not always legal to do so and it's easier to avoid the issue than to face it.

        // SortedMap represents a map's keys and values. The keys and values are
        // aligned in index order: Value[i] is the value in the map corresponding to Key[i].
        public partial struct SortedMap
        {
            public slice<reflect.Value> Key;
            public slice<reflect.Value> Value;
        }

        private static long Len(this ptr<SortedMap> _addr_o)
        {
            ref SortedMap o = ref _addr_o.val;

            return len(o.Key);
        }
        private static bool Less(this ptr<SortedMap> _addr_o, long i, long j)
        {
            ref SortedMap o = ref _addr_o.val;

            return compare(o.Key[i], o.Key[j]) < 0L;
        }
        private static void Swap(this ptr<SortedMap> _addr_o, long i, long j)
        {
            ref SortedMap o = ref _addr_o.val;

            o.Key[i] = o.Key[j];
            o.Key[j] = o.Key[i];
            o.Value[i] = o.Value[j];
            o.Value[j] = o.Value[i];

        }

        // Sort accepts a map and returns a SortedMap that has the same keys and
        // values but in a stable sorted order according to the keys, modulo issues
        // raised by unorderable key values such as NaNs.
        //
        // The ordering rules are more general than with Go's < operator:
        //
        //  - when applicable, nil compares low
        //  - ints, floats, and strings order by <
        //  - NaN compares less than non-NaN floats
        //  - bool compares false before true
        //  - complex compares real, then imag
        //  - pointers compare by machine address
        //  - channel values compare by machine address
        //  - structs compare each field in turn
        //  - arrays compare each element in turn.
        //    Otherwise identical arrays compare by length.
        //  - interface values compare first by reflect.Type describing the concrete type
        //    and then by concrete value as described in the previous rules.
        //
        public static ptr<SortedMap> Sort(reflect.Value mapValue)
        {
            if (mapValue.Type().Kind() != reflect.Map)
            {
                return _addr_null!;
            } 
            // Note: this code is arranged to not panic even in the presence
            // of a concurrent map update. The runtime is responsible for
            // yelling loudly if that happens. See issue 33275.
            var n = mapValue.Len();
            var key = make_slice<reflect.Value>(0L, n);
            var value = make_slice<reflect.Value>(0L, n);
            var iter = mapValue.MapRange();
            while (iter.Next())
            {
                key = append(key, iter.Key());
                value = append(value, iter.Value());
            }

            ptr<SortedMap> sorted = addr(new SortedMap(Key:key,Value:value,));
            sort.Stable(sorted);
            return _addr_sorted!;

        }

        // compare compares two values of the same type. It returns -1, 0, 1
        // according to whether a > b (1), a == b (0), or a < b (-1).
        // If the types differ, it returns -1.
        // See the comment on Sort for the comparison rules.
        private static long compare(reflect.Value aVal, reflect.Value bVal) => func((_, panic, __) =>
        {
            var aType = aVal.Type();
            var bType = bVal.Type();
            if (aType != bType)
            {
                return -1L; // No good answer possible, but don't return 0: they're not equal.
            }


            if (aVal.Kind() == reflect.Int || aVal.Kind() == reflect.Int8 || aVal.Kind() == reflect.Int16 || aVal.Kind() == reflect.Int32 || aVal.Kind() == reflect.Int64) 
                var a = aVal.Int();
                var b = bVal.Int();

                if (a < b) 
                    return -1L;
                else if (a > b) 
                    return 1L;
                else 
                    return 0L;
                            else if (aVal.Kind() == reflect.Uint || aVal.Kind() == reflect.Uint8 || aVal.Kind() == reflect.Uint16 || aVal.Kind() == reflect.Uint32 || aVal.Kind() == reflect.Uint64 || aVal.Kind() == reflect.Uintptr) 
                a = aVal.Uint();
                b = bVal.Uint();

                if (a < b) 
                    return -1L;
                else if (a > b) 
                    return 1L;
                else 
                    return 0L;
                            else if (aVal.Kind() == reflect.String) 
                a = aVal.String();
                b = bVal.String();

                if (a < b) 
                    return -1L;
                else if (a > b) 
                    return 1L;
                else 
                    return 0L;
                            else if (aVal.Kind() == reflect.Float32 || aVal.Kind() == reflect.Float64) 
                return floatCompare(aVal.Float(), bVal.Float());
            else if (aVal.Kind() == reflect.Complex64 || aVal.Kind() == reflect.Complex128) 
                a = aVal.Complex();
                b = bVal.Complex();
                {
                    var c__prev1 = c;

                    var c = floatCompare(real(a), real(b));

                    if (c != 0L)
                    {
                        return c;
                    }

                    c = c__prev1;

                }

                return floatCompare(imag(a), imag(b));
            else if (aVal.Kind() == reflect.Bool) 
                a = aVal.Bool();
                b = bVal.Bool();

                if (a == b) 
                    return 0L;
                else if (a) 
                    return 1L;
                else 
                    return -1L;
                            else if (aVal.Kind() == reflect.Ptr) 
                a = aVal.Pointer();
                b = bVal.Pointer();

                if (a < b) 
                    return -1L;
                else if (a > b) 
                    return 1L;
                else 
                    return 0L;
                            else if (aVal.Kind() == reflect.Chan) 
                {
                    var c__prev1 = c;

                    var (c, ok) = nilCompare(aVal, bVal);

                    if (ok)
                    {
                        return c;
                    }

                    c = c__prev1;

                }

                var ap = aVal.Pointer();
                var bp = bVal.Pointer();

                if (ap < bp) 
                    return -1L;
                else if (ap > bp) 
                    return 1L;
                else 
                    return 0L;
                            else if (aVal.Kind() == reflect.Struct) 
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < aVal.NumField(); i++)
                    {
                        {
                            var c__prev1 = c;

                            c = compare(aVal.Field(i), bVal.Field(i));

                            if (c != 0L)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                    }


                    i = i__prev1;
                }
                return 0L;
            else if (aVal.Kind() == reflect.Array) 
                {
                    long i__prev1 = i;

                    for (i = 0L; i < aVal.Len(); i++)
                    {
                        {
                            var c__prev1 = c;

                            c = compare(aVal.Index(i), bVal.Index(i));

                            if (c != 0L)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                    }


                    i = i__prev1;
                }
                return 0L;
            else if (aVal.Kind() == reflect.Interface) 
                {
                    var c__prev1 = c;

                    (c, ok) = nilCompare(aVal, bVal);

                    if (ok)
                    {
                        return c;
                    }

                    c = c__prev1;

                }

                c = compare(reflect.ValueOf(aVal.Elem().Type()), reflect.ValueOf(bVal.Elem().Type()));
                if (c != 0L)
                {
                    return c;
                }

                return compare(aVal.Elem(), bVal.Elem());
            else 
                // Certain types cannot appear as keys (maps, funcs, slices), but be explicit.
                panic("bad type in compare: " + aType.String());
            
        });

        // nilCompare checks whether either value is nil. If not, the boolean is false.
        // If either value is nil, the boolean is true and the integer is the comparison
        // value. The comparison is defined to be 0 if both are nil, otherwise the one
        // nil value compares low. Both arguments must represent a chan, func,
        // interface, map, pointer, or slice.
        private static (long, bool) nilCompare(reflect.Value aVal, reflect.Value bVal)
        {
            long _p0 = default;
            bool _p0 = default;

            if (aVal.IsNil())
            {
                if (bVal.IsNil())
                {
                    return (0L, true);
                }

                return (-1L, true);

            }

            if (bVal.IsNil())
            {
                return (1L, true);
            }

            return (0L, false);

        }

        // floatCompare compares two floating-point values. NaNs compare low.
        private static long floatCompare(double a, double b)
        {

            if (isNaN(a)) 
                return -1L; // No good answer if b is a NaN so don't bother checking.
            else if (isNaN(b)) 
                return 1L;
            else if (a < b) 
                return -1L;
            else if (a > b) 
                return 1L;
                        return 0L;

        }

        private static bool isNaN(double a)
        {
            return a != a;
        }
    }
}}
