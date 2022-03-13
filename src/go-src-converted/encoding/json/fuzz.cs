// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gofuzz
// +build gofuzz

// package json -- go2cs converted at 2022 March 13 05:39:52 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Program Files\Go\src\encoding\json\fuzz.go
namespace go.encoding;

using fmt = fmt_package;
using System;

public static partial class json_package {

public static nint Fuzz(slice<byte> data) => func((_, panic, _) => {
    nint score = default;

    foreach (var (_, ctor) in new slice<Action>(new Action[] { func()interface{}{returnnew(interface{})}, func()interface{}{returnnew(map[string]interface{})}, func()interface{}{returnnew([]interface{})} })) {
        var v = ctor();
        var err = Unmarshal(data, v);
        if (err != null) {
            continue;
        }
        score = 1;

        var (m, err) = Marshal(v);
        if (err != null) {
            fmt.Printf("v=%#v\n", v);
            panic(err);
        }
        var u = ctor();
        err = Unmarshal(m, u);
        if (err != null) {
            fmt.Printf("v=%#v\n", v);
            fmt.Printf("m=%s\n", m);
            panic(err);
        }
    }    return ;
});

} // end json_package
