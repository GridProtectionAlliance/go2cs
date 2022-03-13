// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue31540 -- go2cs converted at 2022 March 13 06:42:30 UTC
// import "go/internal/gccgoimporter.issue31540" ==> using issue31540 = go.go.@internal.gccgoimporter.issue31540_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\issue31540.go
namespace go.go.@internal;

public static partial class issue31540_package {

public partial struct Y {
    public nint q;
}

public partial struct Z { // : map<nint, nint>
}

public partial struct X { // : map<Y, Z>
}

public partial struct A1 { // : X
}

public partial struct A2 { // : A1
}

public partial struct S {
    public nint b;
    public ref A2 A2 => ref A2_val;
}

public static S Hallo() {
    return new S();
}

} // end issue31540_package
