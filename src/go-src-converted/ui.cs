// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typeutil -- go2cs converted at 2020 October 09 06:02:26 UTC
// import "golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\types\typeutil\ui.go
// This file defines utilities for user interfaces that display types.

using types = go.go.types_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace types
{
    public static partial class typeutil_package
    {
        // IntuitiveMethodSet returns the intuitive method set of a type T,
        // which is the set of methods you can call on an addressable value of
        // that type.
        //
        // The result always contains MethodSet(T), and is exactly MethodSet(T)
        // for interface types and for pointer-to-concrete types.
        // For all other concrete types T, the result additionally
        // contains each method belonging to *T if there is no identically
        // named method on T itself.
        //
        // This corresponds to user intuition about method sets;
        // this function is intended only for user interfaces.
        //
        // The order of the result is as for types.MethodSet(T).
        //
        public static slice<ptr<types.Selection>> IntuitiveMethodSet(types.Type T, ptr<MethodSetCache> _addr_msets)
        {
            ref MethodSetCache msets = ref _addr_msets.val;

            Func<types.Type, bool> isPointerToConcrete = T =>
            {
                ptr<types.Pointer> (ptr, ok) = T._<ptr<types.Pointer>>();
                return ok && !types.IsInterface(ptr.Elem());
            };

            slice<ptr<types.Selection>> result = default;
            var mset = msets.MethodSet(T);
            if (types.IsInterface(T) || isPointerToConcrete(T))
            {
                {
                    long i__prev1 = i;
                    var n__prev1 = n;

                    for (long i = 0L;
                    var n = mset.Len(); i < n; i++)
                    {
                        result = append(result, mset.At(i));
                    }
            else


                    i = i__prev1;
                    n = n__prev1;
                }

            }            { 
                // T is some other concrete type.
                // Report methods of T and *T, preferring those of T.
                var pmset = msets.MethodSet(types.NewPointer(T));
                {
                    long i__prev1 = i;
                    var n__prev1 = n;

                    for (i = 0L;
                    n = pmset.Len(); i < n; i++)
                    {
                        var meth = pmset.At(i);
                        {
                            var m = mset.Lookup(meth.Obj().Pkg(), meth.Obj().Name());

                            if (m != null)
                            {
                                meth = m;
                            }
                        }

                        result = append(result, meth);

                    }

                    i = i__prev1;
                    n = n__prev1;
                }


            }
            return result;

        }
    }
}}}}}}
