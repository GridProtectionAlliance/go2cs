// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue34182 -- go2cs converted at 2022 March 13 06:42:30 UTC
// import "go/internal/gccgoimporter.issue34182" ==> using issue34182 = go.go.@internal.gccgoimporter.issue34182_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\issue34182.go
namespace go.go.@internal;

public static partial class issue34182_package {

public partial struct T1 {
    public ptr<T2> f;
}

public partial struct T2 {
    public T3 f;
}

public partial struct T3 {
    public ref ptr<T2> ptr<T2> => ref ptr<T2>_ptr;
}

} // end issue34182_package
