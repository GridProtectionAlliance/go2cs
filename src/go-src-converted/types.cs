// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typesinternal -- go2cs converted at 2022 March 06 23:31:52 UTC
// import "golang.org/x/tools/internal/typesinternal" ==> using typesinternal = go.golang.org.x.tools.@internal.typesinternal_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\typesinternal\types.go
using types = go.go.types_package;
using reflect = go.reflect_package;
using @unsafe = go.@unsafe_package;

namespace go.golang.org.x.tools.@internal;

public static partial class typesinternal_package {

public static bool SetUsesCgo(ptr<types.Config> _addr_conf) {
    ref types.Config conf = ref _addr_conf.val;

    var v = reflect.ValueOf(conf).Elem();

    var f = v.FieldByName("go115UsesCgo");
    if (!f.IsValid()) {
        f = v.FieldByName("UsesCgo");
        if (!f.IsValid()) {
            return false;
        }
    }
    var addr = @unsafe.Pointer(f.UnsafeAddr()) * (bool.val)(addr);

    true;

    return true;

}

} // end typesinternal_package
