// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:00:11 UTC
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

        private static bool ctxfix(ref ast.File f)
        {
            return rewriteImport(f, "golang.org/x/net/context", "context");
        }
    }
}
