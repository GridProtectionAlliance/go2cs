// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class sort_package {

public static void Heapsort(Interface data) {
    heapSort(data, 0, data.Len());
}

public static void ReverseRange(Interface data, nint a, nint b) {
    reverseRange(data, a, b);
}

} // end sort_package
