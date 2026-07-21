// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.container;

using list = go.container.list_package;
using fmt = fmt_package;
using go.container;

partial class list_test_package {

public static void Example() {
    // Create a new list and put some numbers in it.
    var l = list.New();
    var e4 = l.PushBack((nint)(4));
    var e1 = l.PushFront((nint)(1));
    l.InsertBefore((nint)(3), e4);
    l.InsertAfter((nint)(2), e1);
    // Iterate through list and print its contents.
    for (var e = l.Front(); e != nil; e = e.Next()) {
        fmt.Println((~e).Value);
    }
}

// Output:
// 1
// 2
// 3
// 4

} // end list_test_package
