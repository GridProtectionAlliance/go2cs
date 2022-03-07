// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:14:08 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\scopes.go
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using types2 = go.cmd.compile.@internal.types2_package;

namespace go.cmd.compile.@internal;

public static partial class noder_package {

    // recordScopes populates fn.Parents and fn.Marks based on the scoping
    // information provided by types2.
private static void recordScopes(this ptr<irgen> _addr_g, ptr<ir.Func> _addr_fn, ptr<syntax.FuncType> _addr_sig) {
    ref irgen g = ref _addr_g.val;
    ref ir.Func fn = ref _addr_fn.val;
    ref syntax.FuncType sig = ref _addr_sig.val;

    var (scope, ok) = g.info.Scopes[sig];
    if (!ok) {
        @base.FatalfAt(fn.Pos(), "missing scope for %v", fn);
    }
    for (nint i = 0;
    var n = scope.NumChildren(); i < n; i++) {
        g.walkScope(scope.Child(i));
    }

    g.marker.WriteTo(fn);

}

private static bool walkScope(this ptr<irgen> _addr_g, ptr<types2.Scope> _addr_scope) {
    ref irgen g = ref _addr_g.val;
    ref types2.Scope scope = ref _addr_scope.val;
 
    // types2 doesn't provide a proper API for determining the
    // lexical element a scope represents, so we have to resort to
    // string matching. Conveniently though, this allows us to
    // skip both function types and function literals, neither of
    // which are interesting to us here.
    if (strings.HasPrefix(scope.String(), "function scope ")) {
        return false;
    }
    g.marker.Push(g.pos(scope));

    var haveVars = false;
    foreach (var (_, name) in scope.Names()) {
        {
            ptr<types2.Var> (obj, ok) = scope.Lookup(name)._<ptr<types2.Var>>();

            if (ok && obj.Name() != "_") {
                haveVars = true;
                break;
            }

        }

    }    for (nint i = 0;
    var n = scope.NumChildren(); i < n; i++) {
        if (g.walkScope(scope.Child(i))) {
            haveVars = true;
        }
    }

    if (haveVars) {
        g.marker.Pop(g.end(scope));
    }
    else
 {
        g.marker.Unpush();
    }
    return haveVars;

}

} // end noder_package
