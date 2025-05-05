// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package list implements a doubly linked list.
//
// To iterate over a list (where l is a *List):
//
//	for e := l.Front(); e != nil; e = e.Next() {
//		// do something with e.Value
//	}
namespace go.container;

partial class list_package {

// Element is an element of a linked list.
[GoType] partial struct Element {
    // Next and previous pointers in the doubly-linked list of elements.
    // To simplify the implementation, internally a list l is implemented
    // as a ring, such that &l.root is both the next element of the last
    // list element (l.Back()) and the previous element of the first list
    // element (l.Front()).
    internal ж<Element> next;
    internal ж<Element> prev;
    // The list to which this element belongs.
    internal ж<List> list;
    // The value stored with this element.
    public any Value;
}

// Next returns the next list element or nil.
[GoRecv] public static ж<Element> Next(this ref Element e) {
    {
        var p = e.next; if (e.list != nil && p != Ꮡ(e.list.root)) {
            return p;
        }
    }
    return default!;
}

// Prev returns the previous list element or nil.
[GoRecv] public static ж<Element> Prev(this ref Element e) {
    {
        var p = e.prev; if (e.list != nil && p != Ꮡ(e.list.root)) {
            return p;
        }
    }
    return default!;
}

// List represents a doubly linked list.
// The zero value for List is an empty list ready to use.
[GoType] partial struct List {
    internal Element root; // sentinel list element, only &root, root.prev, and root.next are used
    internal nint len;    // current list length excluding (this) sentinel element
}

// Init initializes or clears list l.
[GoRecv("capture")] public static ж<List> Init(this ref List l) {
    l.root.next = Ꮡ(l.root);
    l.root.prev = Ꮡ(l.root);
    l.len = 0;
    return InitꓸᏑl;
}

// New returns an initialized list.
public static ж<List> New() {
    return @new<List>().Init();
}

// Len returns the number of elements of list l.
// The complexity is O(1).
[GoRecv] public static nint Len(this ref List l) {
    return l.len;
}

// Front returns the first element of list l or nil if the list is empty.
[GoRecv] public static ж<Element> Front(this ref List l) {
    if (l.len == 0) {
        return default!;
    }
    return l.root.next;
}

// Back returns the last element of list l or nil if the list is empty.
[GoRecv] public static ж<Element> Back(this ref List l) {
    if (l.len == 0) {
        return default!;
    }
    return l.root.prev;
}

// lazyInit lazily initializes a zero List value.
[GoRecv] internal static void lazyInit(this ref List l) {
    if (l.root.next == nil) {
        l.Init();
    }
}

// insert inserts e after at, increments l.len, and returns e.
[GoRecv] public static ж<Element> insert(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡat) {
    ref var e = ref Ꮡe.val;
    ref var at = ref Ꮡat.val;

    e.prev = at;
    e.next = at.next;
    e.prev.next = e;
    e.next.prev = e;
    e.list = l;
    l.len++;
    return Ꮡe;
}

// insertValue is a convenience wrapper for insert(&Element{Value: v}, at).
[GoRecv] public static ж<Element> insertValue(this ref List l, any v, ж<Element> Ꮡat) {
    ref var at = ref Ꮡat.val;

    return l.insert(Ꮡ(new Element(Value: v)), Ꮡat);
}

// remove removes e from its list, decrements l.len
[GoRecv] public static void remove(this ref List l, ж<Element> Ꮡe) {
    ref var e = ref Ꮡe.val;

    e.prev.next = e.next;
    e.next.prev = e.prev;
    e.next = default!;
    // avoid memory leaks
    e.prev = default!;
    // avoid memory leaks
    e.list = default!;
    l.len--;
}

// move moves e to next to at.
[GoRecv] public static void move(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡat) {
    ref var e = ref Ꮡe.val;
    ref var at = ref Ꮡat.val;

    if (Ꮡe == Ꮡat) {
        return;
    }
    e.prev.next = e.next;
    e.next.prev = e.prev;
    e.prev = at;
    e.next = at.next;
    e.prev.next = e;
    e.next.prev = e;
}

// Remove removes e from l if e is an element of list l.
// It returns the element value e.Value.
// The element must not be nil.
[GoRecv] public static any Remove(this ref List l, ж<Element> Ꮡe) {
    ref var e = ref Ꮡe.val;

    if (e.list == l) {
        // if e.list == l, l must have been initialized when e was inserted
        // in l or l == nil (e is a zero Element) and l.remove will crash
        l.remove(Ꮡe);
    }
    return e.Value;
}

// PushFront inserts a new element e with value v at the front of list l and returns e.
[GoRecv] public static ж<Element> PushFront(this ref List l, any v) {
    l.lazyInit();
    return l.insertValue(v, Ꮡ(l.root));
}

// PushBack inserts a new element e with value v at the back of list l and returns e.
[GoRecv] public static ж<Element> PushBack(this ref List l, any v) {
    l.lazyInit();
    return l.insertValue(v, l.root.prev);
}

// InsertBefore inserts a new element e with value v immediately before mark and returns e.
// If mark is not an element of l, the list is not modified.
// The mark must not be nil.
[GoRecv] public static ж<Element> InsertBefore(this ref List l, any v, ж<Element> Ꮡmark) {
    ref var mark = ref Ꮡmark.val;

    if (mark.list != l) {
        return default!;
    }
    // see comment in List.Remove about initialization of l
    return l.insertValue(v, mark.prev);
}

// InsertAfter inserts a new element e with value v immediately after mark and returns e.
// If mark is not an element of l, the list is not modified.
// The mark must not be nil.
[GoRecv] public static ж<Element> InsertAfter(this ref List l, any v, ж<Element> Ꮡmark) {
    ref var mark = ref Ꮡmark.val;

    if (mark.list != l) {
        return default!;
    }
    // see comment in List.Remove about initialization of l
    return l.insertValue(v, Ꮡmark);
}

// MoveToFront moves element e to the front of list l.
// If e is not an element of l, the list is not modified.
// The element must not be nil.
[GoRecv] public static void MoveToFront(this ref List l, ж<Element> Ꮡe) {
    ref var e = ref Ꮡe.val;

    if (e.list != l || l.root.next == Ꮡe) {
        return;
    }
    // see comment in List.Remove about initialization of l
    l.move(Ꮡe, Ꮡ(l.root));
}

// MoveToBack moves element e to the back of list l.
// If e is not an element of l, the list is not modified.
// The element must not be nil.
[GoRecv] public static void MoveToBack(this ref List l, ж<Element> Ꮡe) {
    ref var e = ref Ꮡe.val;

    if (e.list != l || l.root.prev == Ꮡe) {
        return;
    }
    // see comment in List.Remove about initialization of l
    l.move(Ꮡe, l.root.prev);
}

// MoveBefore moves element e to its new position before mark.
// If e or mark is not an element of l, or e == mark, the list is not modified.
// The element and mark must not be nil.
[GoRecv] public static void MoveBefore(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡmark) {
    ref var e = ref Ꮡe.val;
    ref var mark = ref Ꮡmark.val;

    if (e.list != l || Ꮡe == Ꮡmark || mark.list != l) {
        return;
    }
    l.move(Ꮡe, mark.prev);
}

// MoveAfter moves element e to its new position after mark.
// If e or mark is not an element of l, or e == mark, the list is not modified.
// The element and mark must not be nil.
[GoRecv] public static void MoveAfter(this ref List l, ж<Element> Ꮡe, ж<Element> Ꮡmark) {
    ref var e = ref Ꮡe.val;
    ref var mark = ref Ꮡmark.val;

    if (e.list != l || Ꮡe == Ꮡmark || mark.list != l) {
        return;
    }
    l.move(Ꮡe, Ꮡmark);
}

// PushBackList inserts a copy of another list at the back of list l.
// The lists l and other may be the same. They must not be nil.
[GoRecv] public static void PushBackList(this ref List l, ж<List> Ꮡother) {
    ref var other = ref Ꮡother.val;

    l.lazyInit();
    for (nint i = other.Len();var e = other.Front(); i > 0; (i, e) = (i - 1, e.Next())) {
        l.insertValue((~e).Value, l.root.prev);
    }
}

// PushFrontList inserts a copy of another list at the front of list l.
// The lists l and other may be the same. They must not be nil.
[GoRecv] public static void PushFrontList(this ref List l, ж<List> Ꮡother) {
    ref var other = ref Ꮡother.val;

    l.lazyInit();
    for (nint i = other.Len();var e = other.Back(); i > 0; (i, e) = (i - 1, e.Prev())) {
        l.insertValue((~e).Value, Ꮡ(l.root));
    }
}

} // end list_package
