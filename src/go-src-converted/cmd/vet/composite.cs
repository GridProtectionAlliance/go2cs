// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the test for unkeyed struct literals.

// package main -- go2cs converted at 2020 August 29 10:08:50 UTC
// Original source: C:\Go\src\cmd\vet\composite.go
using whitelist = go.cmd.vet.@internal.whitelist_package;
using flag = go.flag_package;
using ast = go.go.ast_package;
using types = go.go.types_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var compositeWhiteList = flag.Bool("compositewhitelist", true, "use composite white list; for testing only");

        private static void init()
        {
            register("composites", "check that composite literals used field-keyed elements", checkUnkeyedLiteral, compositeLit);
        }

        // checkUnkeyedLiteral checks if a composite literal is a struct literal with
        // unkeyed fields.
        private static void checkUnkeyedLiteral(ref File f, ast.Node node)
        {
            ref ast.CompositeLit cl = node._<ref ast.CompositeLit>();

            var typ = f.pkg.types[cl].Type;
            if (typ == null)
            { 
                // cannot determine composite literals' type, skip it
                return;
            }
            var typeName = typ.String();
            if (compositeWhiteList && whitelist.UnkeyedLiteral[typeName].Value)
            { 
                // skip whitelisted types
                return;
            }
            {
                ref types.Struct (_, ok) = typ.Underlying()._<ref types.Struct>();

                if (!ok)
                { 
                    // skip non-struct composite literals
                    return;
                }

            }
            if (isLocalType(f, typeName))
            { 
                // allow unkeyed locally defined composite literal
                return;
            } 

            // check if the CompositeLit contains an unkeyed field
            var allKeyValue = true;
            foreach (var (_, e) in cl.Elts)
            {
                {
                    (_, ok) = e._<ref ast.KeyValueExpr>();

                    if (!ok)
                    {
                        allKeyValue = false;
                        break;
                    }

                }
            }
            if (allKeyValue)
            { 
                // all the composite literal fields are keyed
                return;
            }
            f.Badf(cl.Pos(), "%s composite literal uses unkeyed fields", typeName);
        }

        private static bool isLocalType(ref File f, @string typeName)
        {
            if (strings.HasPrefix(typeName, "struct{"))
            { 
                // struct literals are local types
                return true;
            }
            var pkgname = f.pkg.path;
            if (strings.HasPrefix(typeName, pkgname + "."))
            {
                return true;
            } 

            // treat types as local inside test packages with _test name suffix
            if (strings.HasSuffix(pkgname, "_test"))
            {
                pkgname = pkgname[..len(pkgname) - len("_test")];
            }
            return strings.HasPrefix(typeName, pkgname + ".");
        }
    }
}
