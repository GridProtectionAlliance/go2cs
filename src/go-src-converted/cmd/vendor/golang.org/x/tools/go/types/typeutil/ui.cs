// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typeutil -- go2cs converted at 2022 March 13 06:42:44 UTC
// import "cmd/vendor/golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.cmd.vendor.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\types\typeutil\ui.go
namespace go.cmd.vendor.golang.org.x.tools.go.types;
// This file defines utilities for user interfaces that display types.

using types = go.types_package;
using System;

public static partial class typeutil_package {

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
public static slice<ptr<types.Selection>> IntuitiveMethodSet(types.Type T, ptr<MethodSetCache> _addr_msets) {
    ref MethodSetCache msets = ref _addr_msets.val;

    Func<types.Type, bool> isPointerToConcrete = T => {
        ptr<types.Pointer> (ptr, ok) = T._<ptr<types.Pointer>>();
        return ok && !types.IsInterface(ptr.Elem());
    };

    slice<ptr<types.Selection>> result = default;
    var mset = msets.MethodSet(T);
    if (types.IsInterface(T) || isPointerToConcrete(T)) {
        {
            nint i__prev1 = i;
            var n__prev1 = n;

            for (nint i = 0;
            var n = mset.Len(); i < n; i++) {
                result = append(result, mset.At(i));
            }
    else


            i = i__prev1;
            n = n__prev1;
        }
    } { 
        // T is some other concrete type.
        // Report methods of T and *T, preferring those of T.
        var pmset = msets.MethodSet(types.NewPointer(T));
        {
            nint i__prev1 = i;
            var n__prev1 = n;

            for (i = 0;
            n = pmset.Len(); i < n; i++) {
                var meth = pmset.At(i);
                {
                    var m = mset.Lookup(meth.Obj().Pkg(), meth.Obj().Name());

                    if (m != null) {
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

} // end typeutil_package
