// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using testing = testing_package;

partial class errors_test_package {

public static void TestNewEqual(ж<testing.T> Ꮡt) {
    // Different allocations should not be equal.
    if (AreEqual(errors.New("abc"u8), errors.New("abc"u8))) {
        Ꮡt.Errorf(@"New(""abc"") == New(""abc"")"u8);
    }
    if (AreEqual(errors.New("abc"u8), errors.New("xyz"u8))) {
        Ꮡt.Errorf(@"New(""abc"") == New(""xyz"")"u8);
    }
    // Same allocation should be equal to itself (not crash).
    var err = errors.New("jkl"u8);
    if (!AreEqual(err, err)) {
        Ꮡt.Errorf(@"err != err"u8);
    }
}

public static void TestErrorMethod(ж<testing.T> Ꮡt) {
    var err = errors.New("abc"u8);
    if (err.Error() != "abc"u8) {
        Ꮡt.Errorf(@"New(""abc"").Error() = %q, want %q"u8, err.Error(), "abc");
    }
}

} // end errors_test_package
