// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gofuzz
// +build gofuzz

// package csv -- go2cs converted at 2022 March 13 05:39:25 UTC
// import "encoding/csv" ==> using csv = go.encoding.csv_package
// Original source: C:\Program Files\Go\src\encoding\csv\fuzz.go
namespace go.encoding;

using bytes = bytes_package;
using fmt = fmt_package;
using reflect = reflect_package;

public static partial class csv_package {

public static nint Fuzz(slice<byte> data) => func((_, panic, _) => {
    nint score = 0;
    ptr<object> buf = @new<bytes.Buffer>();

    foreach (var (_, tt) in new slice<Reader>(new Reader[] { {}, {Comma:';'}, {Comma:'\t'}, {LazyQuotes:true}, {TrimLeadingSpace:true}, {Comment:'#'}, {Comment:';'} })) {
        var r = NewReader(bytes.NewReader(data));
        r.Comma = tt.Comma;
        r.Comment = tt.Comment;
        r.LazyQuotes = tt.LazyQuotes;
        r.TrimLeadingSpace = tt.TrimLeadingSpace;

        var (records, err) = r.ReadAll();
        if (err != null) {
            continue;
        }
        score = 1;

        buf.Reset();
        var w = NewWriter(buf);
        w.Comma = tt.Comma;
        err = w.WriteAll(records);
        if (err != null) {
            fmt.Printf("writer  = %#v\n", w);
            fmt.Printf("records = %v\n", records);
            panic(err);
        }
        r = NewReader(buf);
        r.Comma = tt.Comma;
        r.Comment = tt.Comment;
        r.LazyQuotes = tt.LazyQuotes;
        r.TrimLeadingSpace = tt.TrimLeadingSpace;
        var (result, err) = r.ReadAll();
        if (err != null) {
            fmt.Printf("reader  = %#v\n", r);
            fmt.Printf("records = %v\n", records);
            panic(err);
        }
        if (!reflect.DeepEqual(records, result)) {
            fmt.Println("records = \n", records);
            fmt.Println("result  = \n", records);
            panic("not equal");
        }
    }    return score;
});

} // end csv_package
