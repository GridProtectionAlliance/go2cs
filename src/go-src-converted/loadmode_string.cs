// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packages -- go2cs converted at 2022 March 06 23:31:47 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\loadmode_string.go
using fmt = go.fmt_package;
using strings = go.strings_package;

namespace go.golang.org.x.tools.go;

public static partial class packages_package {

private static LoadMode allModes = new slice<LoadMode>(new LoadMode[] { NeedName, NeedFiles, NeedCompiledGoFiles, NeedImports, NeedDeps, NeedExportsFile, NeedTypes, NeedSyntax, NeedTypesInfo, NeedTypesSizes });

private static @string modeStrings = new slice<@string>(new @string[] { "NeedName", "NeedFiles", "NeedCompiledGoFiles", "NeedImports", "NeedDeps", "NeedExportsFile", "NeedTypes", "NeedSyntax", "NeedTypesInfo", "NeedTypesSizes" });

public static @string String(this LoadMode mod) {
    var m = mod;
    if (m == 0) {
        return "LoadMode(0)";
    }
    slice<@string> @out = default;
    foreach (var (i, x) in allModes) {
        if (x > m) {
            break;
        }
        if ((m & x) != 0) {
            out = append(out, modeStrings[i]);
            m = m ^ x;
        }
    }    if (m != 0) {
        out = append(out, "Unknown");
    }
    return fmt.Sprintf("LoadMode(%s)", strings.Join(out, "|"));

}

} // end packages_package
