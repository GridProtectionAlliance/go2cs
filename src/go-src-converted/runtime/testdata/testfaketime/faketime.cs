// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test faketime support. This is its own test program because we have
// to build it with custom build tags and hence want to minimize
// dependencies.

// package main -- go2cs converted at 2022 March 13 05:29:17 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testfaketime\faketime.go
namespace go;

using os = os_package;
using time = time_package;

public static partial class main_package {

private static void Main() {
    println("line 1"); 
    // Stream switch, increments time
    os.Stdout.WriteString("line 2\n");
    os.Stdout.WriteString("line 3\n"); 
    // Stream switch, increments time
    os.Stderr.WriteString("line 4\n"); 
    // Time jump
    time.Sleep(1 * time.Second);
    os.Stdout.WriteString("line 5\n"); 
    // Print the current time.
    os.Stdout.WriteString(time.Now().UTC().Format(time.RFC3339));
}

} // end main_package
