// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:44:54 UTC
// Original source: C:\Go\src\cmd\fix\context.go
using ast = go.go.ast_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(contextFix);
        }

        private static fix contextFix = new fix(name:"context",date:"2016-09-09",f:ctxfix,desc:`Change imports of golang.org/x/net/context to context`,disabled:false,);

        private static bool ctxfix(ptr<ast.File> _addr_f)
        {
            ref ast.File f = ref _addr_f.val;

            return rewriteImport(f, "golang.org/x/net/context", "context");
        }
    }
}
