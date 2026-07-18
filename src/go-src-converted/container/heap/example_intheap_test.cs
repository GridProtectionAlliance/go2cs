// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This example demonstrates an integer heap built using the heap interface.
namespace go.container;

using heap = go.container.heap_package;
using fmt = fmt_package;
using go.container;

partial class heap_test_package {

[GoType("[]nint")] partial struct IntHeap;

public static nint Len(this IntHeap h) {
    return len(h);
}

public static bool Less(this IntHeap h, nint i, nint j) {
    return h[i] < h[j];
}

public static void Swap(this IntHeap h, nint i, nint j) {
    (h[i], h[j]) = (h[j], h[i]);
}

[GoRecv] public static void Push(this ref IntHeap h, any x) {
    // Push and Pop use pointer receivers because they modify the slice's length,
    // not just its contents.
    h = append(h, x._<nint>());
}

[GoRecv] public static any Pop(this ref IntHeap h) {
    var old = h;
    nint n = len(old);
    nint x = old[n - 1];
    h = old[0..(int)(n - 1)];
    return x;
}

// This example inserts several ints into an IntHeap, checks the minimum,
// and removes them in order of priority.
public static void Example_intHeap() {
    var h = Ꮡ(new IntHeap(new nint[]{2, 1, 5}.slice()));
    heap.Init(new IntHeapжInterface(h));
    heap.Push(new IntHeapжInterface(h), (nint)(3));
    fmt.Printf("minimum: %d\n"u8, (h.ValueSlot)[0]);
    while ((~h).Len() > 0) {
        fmt.Printf("%d "u8, heap.Pop(new IntHeapжInterface(h)));
    }
}

// Output:
// minimum: 1
// 1 2 3 5

} // end heap_test_package
