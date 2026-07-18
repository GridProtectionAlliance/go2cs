// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This example demonstrates a priority queue built using the heap interface.
namespace go.container;

using heap = go.container.heap_package;
using fmt = fmt_package;
using go.container;

partial class heap_test_package {

// An Item is something we manage in a priority queue.
[GoType] partial struct Item {
    internal @string value; // The value of the item; arbitrary.
    internal nint priority;   // The priority of the item in the queue.
    // The index is needed by update and is maintained by the heap.Interface methods.
    internal nint index; // The index of the item in the heap.
}

[GoType("[]ж<Item>")] partial struct PriorityQueue;

public static nint Len(this PriorityQueue pq) {
    return len(pq);
}

public static bool Less(this PriorityQueue pq, nint i, nint j) {
    // We want Pop to give us the highest, not lowest, priority so we use greater than here.
    return (~pq[i]).priority > (~pq[j]).priority;
}

public static void Swap(this PriorityQueue pq, nint i, nint j) {
    (pq[i], pq[j]) = (pq[j], pq[i]);
    pq[i].Value.index = i;
    pq[j].Value.index = j;
}

[GoRecv] public static void Push(this ref PriorityQueue pq, any x) {
    nint n = len(pq);
    var item = x._<ж<Item>>();
    item.Value.index = n;
    pq = append(pq, item);
}

[GoRecv] public static any Pop(this ref PriorityQueue pq) {
    var old = pq;
    nint n = len(old);
    var item = old[n - 1];
    old[n - 1] = default!;
    // don't stop the GC from reclaiming the item eventually
    item.Value.index = -1;
    // for safety
    pq = old[0..(int)(n - 1)];
    return item;
}

// update modifies the priority and value of an Item in the queue.
internal static void update(this ж<PriorityQueue> Ꮡpq, ж<Item> Ꮡitem, @string value, nint priority) {
    ref var item = ref Ꮡitem.Value;

    item.value = value;
    item.priority = priority;
    heap.Fix(new PriorityQueueжInterface(Ꮡpq), item.index);
}

// This example creates a PriorityQueue with some items, adds and manipulates an item,
// and then removes the items in priority order.
public static void Example_priorityQueue() {
    // Some items and their priorities.
    var items = new map<@string, nint>{
        ["banana"u8] = 3, ["apple"u8] = 2, ["pear"u8] = 4
    };
    // Create a priority queue, put the items in it, and
    // establish the priority queue (heap) invariants.
    ref var pq = ref heap<PriorityQueue>(out var Ꮡpq);
    pq = new PriorityQueue(len(items));
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 0;
    foreach (var (kᴛ1, vᴛ1) in items) {
        ref var value = ref heap(new @string(), out var Ꮡvalue);
        value = kᴛ1;
        ref var priority = ref heap(new nint(), out var Ꮡpriority);
        priority = vᴛ1;

        pq[i] = Ꮡ(new Item(
            value: value,
            priority: priority,
            index: i
        ));
        i++;
    }
    heap.Init(new PriorityQueueжInterface(Ꮡpq));
    // Insert a new item and then modify its priority.
    var item = Ꮡ(new Item(
        value: "orange"u8,
        priority: 1
    ));
    heap.Push(new PriorityQueueжInterface(Ꮡpq), item);
    Ꮡpq.update(item, (~item).value, 5);
    // Take the items out; they arrive in decreasing priority order.
    while (pq.Len() > 0) {
        var itemΔ1 = heap.Pop(new PriorityQueueжInterface(Ꮡpq))._<ж<Item>>();
        fmt.Printf("%.2d:%s "u8, (~itemΔ1).priority, (~itemΔ1).value);
    }
}

// Output:
// 05:orange 04:pear 03:banana 02:apple

} // end heap_test_package
