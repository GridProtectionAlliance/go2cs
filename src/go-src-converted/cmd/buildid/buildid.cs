// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:57:52 UTC
// Original source: C:\Program Files\Go\src\cmd\buildid\buildid.go
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using log = log_package;
using os = os_package;
using strings = strings_package;

using buildid = cmd.@internal.buildid_package;

public static partial class main_package {

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: go tool buildid [-w] file\n");
    flag.PrintDefaults();
    os.Exit(2);
}

private static var wflag = flag.Bool("w", false, "write build ID");

private static void Main() {
    log.SetPrefix("buildid: ");
    log.SetFlags(0);
    flag.Usage = usage;
    flag.Parse();
    if (flag.NArg() != 1) {
        usage();
    }
    var file = flag.Arg(0);
    var (id, err) = buildid.ReadFile(file);
    if (err != null) {
        log.Fatal(err);
    }
    if (!wflag.val) {
        fmt.Printf("%s\n", id);
        return ;
    }
    var (f, err) = os.Open(file);
    if (err != null) {
        log.Fatal(err);
    }
    var (matches, hash, err) = buildid.FindAndHash(f, id, 0);
    f.Close();
    if (err != null) {
        log.Fatal(err);
    }
    var newID = id[..(int)strings.LastIndex(id, "/")] + "/" + buildid.HashToString(hash);
    if (len(newID) != len(id)) {
        log.Fatalf("%s: build ID length mismatch %q vs %q", file, id, newID);
    }
    if (len(matches) == 0) {
        return ;
    }
    f, err = os.OpenFile(file, os.O_RDWR, 0);
    if (err != null) {
        log.Fatal(err);
    }
    {
        var err__prev1 = err;

        var err = buildid.Rewrite(f, matches, newID);

        if (err != null) {
            log.Fatal(err);
        }
        err = err__prev1;

    }
    {
        var err__prev1 = err;

        err = f.Close();

        if (err != null) {
            log.Fatal(err);
        }
        err = err__prev1;

    }
}

} // end main_package
