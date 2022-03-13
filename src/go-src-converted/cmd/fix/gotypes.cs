// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:29:15 UTC
// Original source: C:\Program Files\Go\src\cmd\fix\gotypes.go
namespace go;

using ast = go.ast_package;
using strconv = strconv_package;
using System;

public static partial class main_package {

private static void init() {
    register(gotypesFix);
}

private static fix gotypesFix = new fix(name:"gotypes",date:"2015-07-16",f:gotypes,desc:`Change imports of golang.org/x/tools/go/{exact,types} to go/{constant,types}`,);

private static bool gotypes(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;

    var @fixed = fixGoTypes(_addr_f);
    if (fixGoExact(_addr_f)) {
        fixed = true;
    }
    return fixed;
}

private static bool fixGoTypes(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;

    return rewriteImport(f, "golang.org/x/tools/go/types", "go/types");
}

private static bool fixGoExact(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;
 
    // This one is harder because the import name changes.
    // First find the import spec.
    ptr<ast.ImportSpec> importSpec;
    walk(f, n => {
        if (importSpec != null) {
            return ;
        }
        ptr<ast.ImportSpec> (spec, ok) = n._<ptr<ast.ImportSpec>>();
        if (!ok) {
            return ;
        }
        var (path, err) = strconv.Unquote(spec.Path.Value);
        if (err != null) {
            return ;
        }
        if (path == "golang.org/x/tools/go/exact") {
            importSpec = spec;
        }
    });
    if (importSpec == null) {
        return false;
    }
    var exists = renameTop(f, "constant", "constant");
    @string suffix = "";
    if (exists) {
        suffix = "_";
    }
    renameTop(f, "exact", "constant" + suffix);
    rewriteImport(f, "golang.org/x/tools/go/exact", "go/constant"); 
    // renameTop will also rewrite the imported package name. Fix that;
    // we know it should be missing.
    importSpec.Name = null;
    return true;
}

} // end main_package
