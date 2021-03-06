// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:44:54 UTC
// Original source: C:\Go\src\cmd\fix\egltype.go
using ast = go.go.ast_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(eglFixDisplay);
            register(eglFixConfig);
        }

        private static fix eglFixDisplay = new fix(name:"egl",date:"2018-12-15",f:eglfixDisp,desc:`Fixes initializers of EGLDisplay`,disabled:false,);

        // Old state:
        //   type EGLDisplay unsafe.Pointer
        // New state:
        //   type EGLDisplay uintptr
        // This fix finds nils initializing these types and replaces the nils with 0s.
        private static bool eglfixDisp(ptr<ast.File> _addr_f)
        {
            ref ast.File f = ref _addr_f.val;

            return typefix(f, s =>
            {
                return s == "C.EGLDisplay";
            });

        }

        private static fix eglFixConfig = new fix(name:"eglconf",date:"2020-05-30",f:eglfixConfig,desc:`Fixes initializers of EGLConfig`,disabled:false,);

        // Old state:
        //   type EGLConfig unsafe.Pointer
        // New state:
        //   type EGLConfig uintptr
        // This fix finds nils initializing these types and replaces the nils with 0s.
        private static bool eglfixConfig(ptr<ast.File> _addr_f)
        {
            ref ast.File f = ref _addr_f.val;

            return typefix(f, s =>
            {
                return s == "C.EGLConfig";
            });

        }
    }
}
