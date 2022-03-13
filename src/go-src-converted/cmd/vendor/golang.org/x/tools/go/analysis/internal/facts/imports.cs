// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package facts -- go2cs converted at 2022 March 13 06:41:40 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/internal/facts" ==> using facts = go.cmd.vendor.golang.org.x.tools.go.analysis.@internal.facts_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\internal\facts\imports.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.@internal;

using types = go.types_package;
using System;

public static partial class facts_package {

// importMap computes the import map for a package by traversing the
// entire exported API each of its imports.
//
// This is a workaround for the fact that we cannot access the map used
// internally by the types.Importer returned by go/importer. The entries
// in this map are the packages and objects that may be relevant to the
// current analysis unit.
//
// Packages in the map that are only indirectly imported may be
// incomplete (!pkg.Complete()).
//
private static map<@string, ptr<types.Package>> importMap(slice<ptr<types.Package>> imports) {
    var objects = make_map<types.Object, bool>();
    var packages = make_map<@string, ptr<types.Package>>();

    Func<types.Object, bool> addObj = default;
    Action<types.Type> addType = default;

    addObj = obj => {
        if (!objects[obj]) {
            objects[obj] = true;
            addType(obj.Type());
            {
                var pkg = obj.Pkg();

                if (pkg != null) {
                    packages[pkg.Path()] = pkg;
                }
            }
            return true;
        }
        return false;
    };

    addType = T => {
        switch (T.type()) {
            case ptr<types.Basic> T:
                break;
            case ptr<types.Named> T:
                if (addObj(T.Obj())) {
                    {
                        nint i__prev1 = i;

                        for (nint i = 0; i < T.NumMethods(); i++) {
                            addObj(T.Method(i));
                        }

                        i = i__prev1;
                    }
                }
                break;
            case ptr<types.Pointer> T:
                addType(T.Elem());
                break;
            case ptr<types.Slice> T:
                addType(T.Elem());
                break;
            case ptr<types.Array> T:
                addType(T.Elem());
                break;
            case ptr<types.Chan> T:
                addType(T.Elem());
                break;
            case ptr<types.Map> T:
                addType(T.Key());
                addType(T.Elem());
                break;
            case ptr<types.Signature> T:
                addType(T.Params());
                addType(T.Results());
                break;
            case ptr<types.Struct> T:
                {
                    nint i__prev1 = i;

                    for (i = 0; i < T.NumFields(); i++) {
                        addObj(T.Field(i));
                    }

                    i = i__prev1;
                }
                break;
            case ptr<types.Tuple> T:
                {
                    nint i__prev1 = i;

                    for (i = 0; i < T.Len(); i++) {
                        addObj(T.At(i));
                    }

                    i = i__prev1;
                }
                break;
            case ptr<types.Interface> T:
                {
                    nint i__prev1 = i;

                    for (i = 0; i < T.NumMethods(); i++) {
                        addObj(T.Method(i));
                    }

                    i = i__prev1;
                }
                break;
        }
    };

    foreach (var (_, imp) in imports) {
        packages[imp.Path()] = imp;

        var scope = imp.Scope();
        foreach (var (_, name) in scope.Names()) {
            addObj(scope.Lookup(name));
        }
    }    return packages;
}

} // end facts_package
