// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types2 -- go2cs converted at 2022 March 13 06:26:04 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\instantiate.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;
using fmt = fmt_package;


// Instantiate instantiates the type typ with the given type arguments.
// typ must be a *Named or a *Signature type, it must be generic, and
// its number of type parameters must match the number of provided type
// arguments. The result is a new, instantiated (not generic) type of
// the same kind (either a *Named or a *Signature). The type arguments
// are not checked against the constraints of the type parameters.
// Any methods attached to a *Named are simply copied; they are not
// instantiated.

using System;
public static partial class types2_package {

public static Type Instantiate(syntax.Pos pos, Type typ, slice<Type> targs) => func((defer, panic, _) => {
    Type res = default;
 
    // TODO(gri) This code is basically identical to the prolog
    //           in Checker.instantiate. Factor.
    slice<ptr<TypeName>> tparams = default;
    switch (typ.type()) {
        case ptr<Named> t:
            tparams = t.tparams;
            break;
        case ptr<Signature> t:
            tparams = t.tparams;
            defer(() => { 
                // If we had an unexpected failure somewhere don't panic below when
                // asserting res.(*Signature). Check for *Signature in case Typ[Invalid]
                // is returned.
                {
                    ptr<Signature> (_, ok) = res._<ptr<Signature>>();

                    if (!ok) {
                        return ;
                    }
                } 
                // If the signature doesn't use its type parameters, subst
                // will not make a copy. In that case, make a copy now (so
                // we can set tparams to nil w/o causing side-effects).
                if (t == res) {
                    ref var copy = ref heap(t.val, out ptr<var> _addr_copy);
                    _addr_res = _addr_copy;
                    res = ref _addr_res.val;
                }
                res._<ptr<Signature>>().tparams = null;
            }());
            break;
        default:
        {
            var t = typ.type();
            panic(fmt.Sprintf("%v: cannot instantiate %v", pos, typ));
            break;
        } 

        // the number of supplied types must match the number of type parameters
    } 

    // the number of supplied types must match the number of type parameters
    if (len(targs) != len(tparams)) {
        panic(fmt.Sprintf("%v: got %d arguments but %d type parameters", pos, len(targs), len(tparams)));
    }
    if (len(tparams) == 0) {
        return typ; // nothing to do (minor optimization)
    }
    var smap = makeSubstMap(tparams, targs);
    return (Checker.val)(null).subst(pos, typ, smap);
});

} // end types2_package
