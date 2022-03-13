// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:32 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\main.go
namespace go;

using os = os_package;
using System;

public static partial class main_package {

private static map cmds = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Action>{};

private static void register(@string name, Action f) => func((_, panic, _) => {
    if (cmds[name] != null) {
        panic("duplicate registration: " + name);
    }
    cmds[name] = f;
});

private static void registerInit(@string name, Action f) {
    if (len(os.Args) >= 2 && os.Args[1] == name) {
        f();
    }
}

private static void Main() {
    if (len(os.Args) < 2) {
        println("usage: " + os.Args[0] + " name-of-test");
        return ;
    }
    var f = cmds[os.Args[1]];
    if (f == null) {
        println("unknown function: " + os.Args[1]);
        return ;
    }
    f();
}

} // end main_package
