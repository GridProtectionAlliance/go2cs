// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package blank is a go/doc test for the handling of _.
// See issue 5397.

// package blank -- go2cs converted at 2022 March 13 05:52:39 UTC
// import "go/doc.blank" ==> using blank = go.go.doc.blank_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\blank.go
namespace go.go;

using os = os_package;

public static partial class blank_package {

public partial struct T { // : nint
}

// T constants counting from a blank constant.
private static readonly T _ = iota;
public static readonly var T1 = 0;
public static readonly var T2 = 1;

// T constants counting from unexported constants.
private static readonly T tweedledee = iota;
private static readonly var tweedledum = 0;
public static readonly var C1 = 1;
public static readonly var C2 = 2;
private static readonly var alice = 3;
public static readonly var C3 = 4;
private static readonly nint redQueen = iota;
public static readonly var C4 = 5;

// Constants with a single type that is not propagated.
private static readonly os.FileMode zero = 0;
public static readonly nint Default = 0644;
public static readonly nint Useless = 0312;
public static readonly nint WideOpen = 0777;

// Constants with an imported type that is propagated.
private static readonly os.FileMode zero = 0;
public static readonly var M1 = 0;
public static readonly var M2 = 1;
public static readonly var M3 = 2;

// Package constants.
private static readonly nint _ = iota;
public static readonly var I1 = 0;
public static readonly var I2 = 1;

// Unexported constants counting from blank iota.
// See issue 9615.
private static readonly var _ = iota;
private static readonly var one = iota + 1;

// Blanks not in doc output:

// S has a padding field.
public partial struct S {
    public uint H;
    public byte _;
    public byte A;
}

private static void _() {
}

private partial struct _ { // : T
}

private static var _ = T(55);

} // end blank_package
