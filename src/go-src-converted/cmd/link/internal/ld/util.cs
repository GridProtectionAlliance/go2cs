// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:26 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\util.go
using loader = go.cmd.link.@internal.loader_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using os = go.os_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

private static slice<Action> atExitFuncs = default;

public static void AtExit(Action f) {
    atExitFuncs = append(atExitFuncs, f);
}

// runAtExitFuncs runs the queued set of AtExit functions.
private static void runAtExitFuncs() {
    for (var i = len(atExitFuncs) - 1; i >= 0; i--) {
        atExitFuncs[i]();
    }
    atExitFuncs = null;
}

// Exit exits with code after executing all atExitFuncs.
public static void Exit(nint code) {
    runAtExitFuncs();
    os.Exit(code);
}

// Exitf logs an error message then calls Exit(2).
public static void Exitf(@string format, params object[] a) {
    a = a.Clone();

    fmt.Fprintf(os.Stderr, os.Args[0] + ": " + format + "\n", a);
    nerrors++;
    Exit(2);
}

// afterErrorAction updates 'nerrors' on error and invokes exit or
// panics in the proper circumstances.
private static void afterErrorAction() => func((_, panic, _) => {
    nerrors++;
    if (flagH.val) {
        panic("error");
    }
    if (nerrors > 20) {
        Exitf("too many errors");
    }
});

// Errorf logs an error message.
//
// If more than 20 errors have been printed, exit with an error.
//
// Logging an error means that on exit cmd/link will delete any
// output file and return a non-zero error code.
//
// TODO: remove. Use ctxt.Errorf instead.
// All remaining calls use nil as first arg.
public static void Errorf(ptr<nint> _addr_dummy, @string format, params object[] args) {
    args = args.Clone();
    ref nint dummy = ref _addr_dummy.val;

    format += "\n";
    fmt.Fprintf(os.Stderr, format, args);
    afterErrorAction();
}

// Errorf method logs an error message.
//
// If more than 20 errors have been printed, exit with an error.
//
// Logging an error means that on exit cmd/link will delete any
// output file and return a non-zero error code.
private static void Errorf(this ptr<Link> _addr_ctxt, loader.Sym s, @string format, params object[] args) {
    args = args.Clone();
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.loader != null) {
        ctxt.loader.Errorf(s, format, args);
        return ;
    }
    format = fmt.Sprintf("sym %d: %s", s, format);
    format += "\n";
    fmt.Fprintf(os.Stderr, format, args);
    afterErrorAction();

}

private static @string artrim(slice<byte> x) {
    nint i = 0;
    var j = len(x);
    while (i < len(x) && x[i] == ' ') {
        i++;
    }
    while (j > i && x[j - 1] == ' ') {
        j--;
    }
    return string(x[(int)i..(int)j]);
}

private static void stringtouint32(slice<uint> x, @string s) {
    for (nint i = 0; len(s) > 0; i++) {
        array<byte> buf = new array<byte>(4);
        s = s[(int)copy(buf[..], s)..];
        x[i] = binary.LittleEndian.Uint32(buf[..]);
    }
}

// contains reports whether v is in s.
private static bool contains(slice<@string> s, @string v) {
    foreach (var (_, x) in s) {
        if (x == v) {
            return true;
        }
    }    return false;

}

} // end ld_package
