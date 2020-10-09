// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:44:58 UTC
// Original source: C:\Go\src\cmd\fix\jnitype.go
using ast = go.go.ast_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(jniFix);
        }

        private static fix jniFix = new fix(name:"jni",date:"2017-12-04",f:jnifix,desc:`Fixes initializers of JNI's jobject and subtypes`,disabled:false,);

        // Old state:
        //   type jobject *_jobject
        // New state:
        //   type jobject uintptr
        // and similar for subtypes of jobject.
        // This fix finds nils initializing these types and replaces the nils with 0s.
        private static bool jnifix(ptr<ast.File> _addr_f)
        {
            ref ast.File f = ref _addr_f.val;

            return typefix(f, s =>
            {
                switch (s)
                {
                    case "C.jobject": 
                        return true;
                        break;
                    case "C.jclass": 
                        return true;
                        break;
                    case "C.jthrowable": 
                        return true;
                        break;
                    case "C.jstring": 
                        return true;
                        break;
                    case "C.jarray": 
                        return true;
                        break;
                    case "C.jbooleanArray": 
                        return true;
                        break;
                    case "C.jbyteArray": 
                        return true;
                        break;
                    case "C.jcharArray": 
                        return true;
                        break;
                    case "C.jshortArray": 
                        return true;
                        break;
                    case "C.jintArray": 
                        return true;
                        break;
                    case "C.jlongArray": 
                        return true;
                        break;
                    case "C.jfloatArray": 
                        return true;
                        break;
                    case "C.jdoubleArray": 
                        return true;
                        break;
                    case "C.jobjectArray": 
                        return true;
                        break;
                    case "C.jweak": 
                        return true;
                        break;
                }
                return false;

            });

        }
    }
}
