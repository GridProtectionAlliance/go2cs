// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using sort = go.sort_package;
using strings = strings_package;
using go;

partial class sort_test_package {

// This example demonstrates searching a list sorted in ascending order.
public static void ExampleSearch() {
    var a = new nint[]{1, 3, 6, 10, 15, 21, 28, 36, 45, 55}.slice();
    nint x = 6;
    var aʗ1 = a;
    nint i = sort.Search(len(a), (nint iΔ1) => aʗ1[iΔ1] >= x);
    if (i < len(a) && a[i] == x){
        fmt.Printf("found %d at index %d in %v\n"u8, x, i, a);
    } else {
        fmt.Printf("%d not found in %v\n"u8, x, a);
    }
}

// Output:
// found 6 at index 2 in [1 3 6 10 15 21 28 36 45 55]

// This example demonstrates searching a list sorted in descending order.
// The approach is the same as searching a list in ascending order,
// but with the condition inverted.
public static void ExampleSearch_descendingOrder() {
    var a = new nint[]{55, 45, 36, 28, 21, 15, 10, 6, 3, 1}.slice();
    nint x = 6;
    var aʗ1 = a;
    nint i = sort.Search(len(a), (nint iΔ1) => aʗ1[iΔ1] <= x);
    if (i < len(a) && a[i] == x){
        fmt.Printf("found %d at index %d in %v\n"u8, x, i, a);
    } else {
        fmt.Printf("%d not found in %v\n"u8, x, a);
    }
}

// Output:
// found 6 at index 7 in [55 45 36 28 21 15 10 6 3 1]

// This example demonstrates finding a string in a list sorted in ascending order.
public static void ExampleFind() {
    var a = new @string[]{"apple", "banana", "lemon", "mango", "pear", "strawberry"}.slice();
    foreach (var (_, x) in new @string[]{"banana", "orange"}.slice()) {
        var aʗ1 = a;
        var (i, found) = sort.Find(len(a), (nint iΔ1) => strings.Compare(x, aʗ1[iΔ1]));
        if (found){
            fmt.Printf("found %s at index %d\n"u8, x, i);
        } else {
            fmt.Printf("%s not found, would insert at %d\n"u8, x, i);
        }
    }
}

// Output:
// found banana at index 1
// orange not found, would insert at 4

// This example demonstrates searching for float64 in a list sorted in ascending order.
public static void ExampleSearchFloat64s() {
    var a = new float64[]{1.0D, 2.0D, 3.3D, 4.6D, 6.1D, 7.2D, 8.0D}.slice();
    var x = 2.0D;
    nint i = sort.SearchFloat64s(a, x);
    fmt.Printf("found %g at index %d in %v\n"u8, x, i, a);
    x = 0.5D;
    i = sort.SearchFloat64s(a, x);
    fmt.Printf("%g not found, can be inserted at index %d in %v\n"u8, x, i, a);
}

// Output:
// found 2 at index 1 in [1 2 3.3 4.6 6.1 7.2 8]
// 0.5 not found, can be inserted at index 0 in [1 2 3.3 4.6 6.1 7.2 8]

// This example demonstrates searching for int in a list sorted in ascending order.
public static void ExampleSearchInts() {
    var a = new nint[]{1, 2, 3, 4, 6, 7, 8}.slice();
    nint x = 2;
    nint i = sort.SearchInts(a, x);
    fmt.Printf("found %d at index %d in %v\n"u8, x, i, a);
    x = 5;
    i = sort.SearchInts(a, x);
    fmt.Printf("%d not found, can be inserted at index %d in %v\n"u8, x, i, a);
}

// Output:
// found 2 at index 1 in [1 2 3 4 6 7 8]
// 5 not found, can be inserted at index 4 in [1 2 3 4 6 7 8]

} // end sort_test_package
