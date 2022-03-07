// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package xerrors -- go2cs converted at 2022 March 06 23:16:45 UTC
// import "golang.org/x/xerrors" ==> using xerrors = go.golang.org.x.xerrors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\xerrors\errors.go
using fmt = go.fmt_package;

namespace go.golang.org.x;

public static partial class xerrors_package {

    // errorString is a trivial implementation of error.
private partial struct errorString {
    public @string s;
    public Frame frame;
}

// New returns an error that formats as the given text.
//
// The returned error contains a Frame set to the caller's location and
// implements Formatter to show this information when printed with details.
public static error New(@string text) {
    return error.As(addr(new errorString(text,Caller(1)))!)!;
}

private static @string Error(this ptr<errorString> _addr_e) {
    ref errorString e = ref _addr_e.val;

    return e.s;
}

private static void Format(this ptr<errorString> _addr_e, fmt.State s, int v) {
    ref errorString e = ref _addr_e.val;

    FormatError(e, s, v);
}

private static error FormatError(this ptr<errorString> _addr_e, Printer p) {
    error next = default!;
    ref errorString e = ref _addr_e.val;

    p.Print(e.s);
    e.frame.Format(p);
    return error.As(null!)!;
}

} // end xerrors_package
