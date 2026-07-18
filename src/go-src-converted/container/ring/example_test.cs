// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using ring = go.container.ring_package;
using fmt = fmt_package;
using go.container;

partial class ring_test_package {

public static void ExampleRing_Len() {
    // Create a new ring of size 4
    var r = ring.New(4);
    // Print out its length
    fmt.Println(r.Len());
}

// Output:
// 4
public static void ExampleRing_Next() {
    // Create a new ring of size 5
    var r = ring.New(5);
    // Get the length of the ring
    nint n = r.Len();
    // Initialize the ring with some integer values
    for (nint i = 0; i < n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    // Iterate through the ring and print its contents
    for (nint j = 0; j < n; j++) {
        fmt.Println((~r).Value);
        r = r.Next();
    }
}

// Output:
// 0
// 1
// 2
// 3
// 4
public static void ExampleRing_Prev() {
    // Create a new ring of size 5
    var r = ring.New(5);
    // Get the length of the ring
    nint n = r.Len();
    // Initialize the ring with some integer values
    for (nint i = 0; i < n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    // Iterate through the ring backwards and print its contents
    for (nint j = 0; j < n; j++) {
        r = r.Prev();
        fmt.Println((~r).Value);
    }
}

// Output:
// 4
// 3
// 2
// 1
// 0
public static void ExampleRing_Do() {
    // Create a new ring of size 5
    var r = ring.New(5);
    // Get the length of the ring
    nint n = r.Len();
    // Initialize the ring with some integer values
    for (nint i = 0; i < n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    // Iterate through the ring and print its contents
    r.Do((any p) => {
        fmt.Println(p._<nint>());
    });
}

// Output:
// 0
// 1
// 2
// 3
// 4
public static void ExampleRing_Move() {
    // Create a new ring of size 5
    var r = ring.New(5);
    // Get the length of the ring
    nint n = r.Len();
    // Initialize the ring with some integer values
    for (nint i = 0; i < n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    // Move the pointer forward by three steps
    r = r.Move(3);
    // Iterate through the ring and print its contents
    r.Do((any p) => {
        fmt.Println(p._<nint>());
    });
}

// Output:
// 3
// 4
// 0
// 1
// 2
public static void ExampleRing_Link() {
    // Create two rings, r and s, of size 2
    var r = ring.New(2);
    var s = ring.New(2);
    // Get the length of the ring
    nint lr = r.Len();
    nint ls = s.Len();
    // Initialize r with 0s
    for (nint i = 0; i < lr; i++) {
        r.Value.Value = (nint)(0);
        r = r.Next();
    }
    // Initialize s with 1s
    for (nint j = 0; j < ls; j++) {
        s.Value.Value = (nint)(1);
        s = s.Next();
    }
    // Link ring r and ring s
    var rs = r.Link(s);
    // Iterate through the combined ring and print its contents
    rs.Do((any p) => {
        fmt.Println(p._<nint>());
    });
}

// Output:
// 0
// 0
// 1
// 1
public static void ExampleRing_Unlink() {
    // Create a new ring of size 6
    var r = ring.New(6);
    // Get the length of the ring
    nint n = r.Len();
    // Initialize the ring with some integer values
    for (nint i = 0; i < n; i++) {
        r.Value.Value = i;
        r = r.Next();
    }
    // Unlink three elements from r, starting from r.Next()
    r.Unlink(3);
    // Iterate through the remaining ring and print its contents
    r.Do((any p) => {
        fmt.Println(p._<nint>());
    });
}

// Output:
// 0
// 4
// 5

} // end ring_test_package
