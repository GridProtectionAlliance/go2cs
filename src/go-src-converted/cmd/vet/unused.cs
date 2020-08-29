// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file defines the check for unused results of calls to certain
// pure functions.

// package main -- go2cs converted at 2020 August 29 10:09:33 UTC
// Original source: C:\Go\src\cmd\vet\unused.go
using flag = go.flag_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var unusedFuncsFlag = flag.String("unusedfuncs", "errors.New,fmt.Errorf,fmt.Sprintf,fmt.Sprint,sort.Reverse", "comma-separated list of functions whose results must be used");

        private static var unusedStringMethodsFlag = flag.String("unusedstringmethods", "Error,String", "comma-separated list of names of methods of type func() string whose results must be used");

        private static void init()
        {
            register("unusedresult", "check for unused result of calls to functions in -unusedfuncs list and methods in -unusedstringmethods list", checkUnusedResult, exprStmt);
        }

        // func() string
        private static var sigNoArgsStringResult = types.NewSignature(null, null, types.NewTuple(types.NewVar(token.NoPos, null, "", types.Typ[types.String])), false);

        private static var unusedFuncs = make_map<@string, bool>();
        private static var unusedStringMethods = make_map<@string, bool>();

        private static void initUnusedFlags()
        {
            Action<@string, map<@string, bool>> commaSplit = (s, m) =>
            {
                if (s != "")
                {
                    foreach (var (_, name) in strings.Split(s, ","))
                    {
                        if (len(name) == 0L)
                        {
                            flag.Usage();
                        }
                        m[name] = true;
                    }
                }
            }
;
            commaSplit(unusedFuncsFlag.Value, unusedFuncs);
            commaSplit(unusedStringMethodsFlag.Value, unusedStringMethods);
        }

        private static void checkUnusedResult(ref File f, ast.Node n)
        {
            ref ast.CallExpr (call, ok) = unparen(n._<ref ast.ExprStmt>().X)._<ref ast.CallExpr>();
            if (!ok)
            {
                return; // not a call statement
            }
            var fun = unparen(call.Fun);

            if (f.pkg.types[fun].IsType())
            {
                return; // a conversion, not a call
            }
            ref ast.SelectorExpr (selector, ok) = fun._<ref ast.SelectorExpr>();
            if (!ok)
            {
                return; // neither a method call nor a qualified ident
            }
            var (sel, ok) = f.pkg.selectors[selector];
            if (ok && sel.Kind() == types.MethodVal)
            { 
                // method (e.g. foo.String())
                ref types.Func obj = sel.Obj()._<ref types.Func>();
                ref types.Signature sig = sel.Type()._<ref types.Signature>();
                if (types.Identical(sig, sigNoArgsStringResult))
                {
                    if (unusedStringMethods[obj.Name()])
                    {
                        f.Badf(call.Lparen, "result of (%s).%s call not used", sig.Recv().Type(), obj.Name());
                    }
                }
            }
            else if (!ok)
            { 
                // package-qualified function (e.g. fmt.Errorf)
                obj = f.pkg.uses[selector.Sel];
                {
                    ref types.Func obj__prev3 = obj;

                    ref types.Func (obj, ok) = obj._<ref types.Func>();

                    if (ok)
                    {
                        var qname = obj.Pkg().Path() + "." + obj.Name();
                        if (unusedFuncs[qname])
                        {
                            f.Badf(call.Lparen, "result of %v call not used", qname);
                        }
                    }

                    obj = obj__prev3;

                }
            }
        }
    }
}
