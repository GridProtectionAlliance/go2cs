// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !go1.8

// package sort -- go2cs converted at 2020 October 08 03:44:11 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Go\src\sort\slice_go14.go
using reflect = go.reflect_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class sort_package
    {
        private static var reflectValueOf = reflect.ValueOf;

        private static Action<long, long> reflectSwapper(object x)
        {
            var v = reflectValueOf(x);
            var tmp = reflect.New(v.Type().Elem()).Elem();
            return (i, j) =>
            {
                var a = v.Index(i);
                var b = v.Index(j);
                tmp.Set(a);
                a.Set(b);
                b.Set(tmp);

            };

        }
    }
}
