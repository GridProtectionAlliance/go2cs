// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package c -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.c" ==> using c = go.go.doc.c_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\c.go
namespace go.go;

using a = a_package;

public static partial class c_package {

// ----------------------------------------------------------------------------
// Test that empty declarations don't cause problems


// ----------------------------------------------------------------------------
// Test that types with documentation on both, the Decl and the Spec node
// are handled correctly.

// A (should see this)
public partial struct A {
}

// B (should see this)
public partial struct B {
} 
// C (should see this)
public partial struct C {
} 
// D (should see this)
public partial struct D {
} 
// E1 (should see this)
public partial struct E1 {
}
public partial struct E2 {
}
public partial struct E3 {
} 
// E4 (should see this)
public partial struct E4 {
}public partial struct T1 {
}

private static void M(this ptr<T1> _addr_t1) {
    ref T1 t1 = ref _addr_t1.val;

}

// T2 must not show methods of local T1
public partial struct T2 {
    public ref a.T1 T1 => ref T1_val; // not the same as locally declared T1
}

} // end c_package
