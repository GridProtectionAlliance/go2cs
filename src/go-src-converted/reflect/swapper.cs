// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflect -- go2cs converted at 2020 August 29 08:43:00 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Go\src\reflect\swapper.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class reflect_package
    {
        // Swapper returns a function that swaps the elements in the provided
        // slice.
        //
        // Swapper panics if the provided interface is not a slice.
        public static Action<long, long> Swapper(object slice) => func((_, panic, __) =>
        {
            var v = ValueOf(slice);
            if (v.Kind() != Slice)
            {
                panic(ref new ValueError(Method:"Swapper",Kind:v.Kind()));
            }
            switch (v.Len())
            {
                case 0L: 
                    return (i, j) =>
                    {
                        panic("reflect: slice index out of range");

                    };
                    break;
                case 1L: 
                    return (i, j) =>
                    {
                        if (i != 0L || j != 0L)
                        {
                            panic("reflect: slice index out of range");
                        }
                    };
                    break;
            }

            ref rtype typ = v.Type().Elem()._<ref rtype>();
            var size = typ.Size();
            var hasPtr = typ.kind & kindNoPointers == 0L; 

            // Some common & small cases, without using memmove:
            if (hasPtr)
            {
                if (size == ptrSize)
                {
                    *(*slice<unsafe.Pointer>) ps = v.ptr.Value;
                    return (i, j) =>
                    {
                        ps[i] = ps[j];
                        ps[j] = ps[i];

                    };
                }
            else
                if (typ.Kind() == String)
                {
                    *(*slice<@string>) ss = v.ptr.Value;
                    return (i, j) =>
                    {
                        ss[i] = ss[j];
                        ss[j] = ss[i];

                    };
                }
            }            {
                switch (size)
                {
                    case 8L: 
                        *(*slice<long>) @is = v.ptr.Value;
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];

                        };
                        break;
                    case 4L: 
                        @is = v.ptr.Value;
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];

                        };
                        break;
                    case 2L: 
                        @is = v.ptr.Value;
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];

                        };
                        break;
                    case 1L: 
                        @is = v.ptr.Value;
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];

                        };
                        break;
                }
            }
            var s = (sliceHeader.Value)(v.ptr);
            var tmp = unsafe_New(typ); // swap scratch space

            return (i, j) =>
            {
                if (uint(i) >= uint(s.Len) || uint(j) >= uint(s.Len))
                {
                    panic("reflect: slice index out of range");
                }
                var val1 = arrayAt(s.Data, i, size, "i < s.Len");
                var val2 = arrayAt(s.Data, j, size, "j < s.Len");
                typedmemmove(typ, tmp, val1);
                typedmemmove(typ, val1, val2);
                typedmemmove(typ, val2, tmp);
            };
        });
    }
}
