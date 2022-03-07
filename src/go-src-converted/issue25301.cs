// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue25301 -- go2cs converted at 2022 March 06 23:32:14 UTC
// import "golang.org/x/tools/go/internal/gcimporter.issue25301" ==> using issue25301 = go.golang.org.x.tools.go.@internal.gcimporter.issue25301_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\testdata\issue25301.go


namespace go.golang.org.x.tools.go.@internal;

public static partial class issue25301_package {

public partial interface A {
    void M();
}
public partial interface T {
}
public partial struct S {
}
public static void M(this S _p0) {
    println("m");
}

} // end issue25301_package
