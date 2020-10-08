// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflect -- go2cs converted at 2020 October 08 03:24:34 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Go\src\reflect\swapper.go
using unsafeheader = go.@internal.unsafeheader_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class reflect_package
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
                panic(addr(new ValueError(Method:"Swapper",Kind:v.Kind())));
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

            ptr<rtype> typ = v.Type().Elem()._<ptr<rtype>>();
            var size = typ.Size();
            var hasPtr = typ.ptrdata != 0L; 

            // Some common & small cases, without using memmove:
            if (hasPtr)
            {
                if (size == ptrSize)
                {
                    ptr<ptr<slice<unsafe.Pointer>>> ps = new ptr<ptr<ptr<slice<unsafe.Pointer>>>>(v.ptr);
                    return (i, j) =>
                    {
                        ps[i] = ps[j];
                        ps[j] = ps[i];
                    };

                }
            else
                if (typ.Kind() == String)
                {
                    ptr<ptr<slice<@string>>> ss = new ptr<ptr<ptr<slice<@string>>>>(v.ptr);
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
                        ptr<ptr<slice<long>>> @is = new ptr<ptr<ptr<slice<long>>>>(v.ptr);
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];
                        };
                        break;
                    case 4L: 
                        @is = new ptr<ptr<ptr<slice<int>>>>(v.ptr);
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];
                        };
                        break;
                    case 2L: 
                        @is = new ptr<ptr<ptr<slice<short>>>>(v.ptr);
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];
                        };
                        break;
                    case 1L: 
                        @is = new ptr<ptr<ptr<slice<sbyte>>>>(v.ptr);
                        return (i, j) =>
                        {
                            is[i] = is[j];
                            is[j] = is[i];
                        };
                        break;
                }

            }
            var s = (unsafeheader.Slice.val)(v.ptr);
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
