// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gofuzz

// package json -- go2cs converted at 2020 October 08 03:42:52 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\fuzz.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        public static long Fuzz(slice<byte> data) => func((_, panic, __) =>
        {
            long score = default;

            foreach (var (_, ctor) in new slice<Action>(new Action[] { func()interface{}{returnnew(interface{})}, func()interface{}{returnnew(map[string]interface{})}, func()interface{}{returnnew([]interface{})} }))
            {
                var v = ctor();
                var err = Unmarshal(data, v);
                if (err != null)
                {
                    continue;
                }
                score = 1L;

                var (m, err) = Marshal(v);
                if (err != null)
                {
                    fmt.Printf("v=%#v\n", v);
                    panic(err);
                }
                var u = ctor();
                err = Unmarshal(m, u);
                if (err != null)
                {
                    fmt.Printf("v=%#v\n", v);
                    fmt.Printf("m=%s\n", m);
                    panic(err);
                }
            }            return ;

        });
    }
}}
