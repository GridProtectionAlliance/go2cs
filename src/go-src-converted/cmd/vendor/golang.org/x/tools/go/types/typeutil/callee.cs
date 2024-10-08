// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typeutil -- go2cs converted at 2022 March 13 06:42:41 UTC
// import "cmd/vendor/golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.cmd.vendor.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\types\typeutil\callee.go
namespace go.cmd.vendor.golang.org.x.tools.go.types;

using ast = go.ast_package;
using types = go.types_package;

using astutil = golang.org.x.tools.go.ast.astutil_package;


// Callee returns the named target of a function call, if any:
// a function, method, builtin, or variable.

public static partial class typeutil_package {

public static types.Object Callee(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_call) {
    ref types.Info info = ref _addr_info.val;
    ref ast.CallExpr call = ref _addr_call.val;

    types.Object obj = default;
    switch (astutil.Unparen(call.Fun).type()) {
        case ptr<ast.Ident> fun:
            obj = info.Uses[fun]; // type, var, builtin, or declared func
            break;
        case ptr<ast.SelectorExpr> fun:
            {
                var (sel, ok) = info.Selections[fun];

                if (ok) {
                    obj = sel.Obj(); // method or field
                }
                else
 {
                    obj = info.Uses[fun.Sel]; // qualified identifier?
                }
            }
            break;
    }
    {
        ptr<types.TypeName> (_, ok) = obj._<ptr<types.TypeName>>();

        if (ok) {
            return null; // T(x) is a conversion, not a call
        }
    }
    return obj;
}

// StaticCallee returns the target (function or method) of a static
// function call, if any. It returns nil for calls to builtins.
public static ptr<types.Func> StaticCallee(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_call) {
    ref types.Info info = ref _addr_info.val;
    ref ast.CallExpr call = ref _addr_call.val;

    {
        ptr<types.Func> (f, ok) = Callee(_addr_info, _addr_call)._<ptr<types.Func>>();

        if (ok && !interfaceMethod(f)) {
            return _addr_f!;
        }
    }
    return _addr_null!;
}

private static bool interfaceMethod(ptr<types.Func> _addr_f) {
    ref types.Func f = ref _addr_f.val;

    ptr<types.Signature> recv = f.Type()._<ptr<types.Signature>>().Recv();
    return recv != null && types.IsInterface(recv.Type());
}

} // end typeutil_package
