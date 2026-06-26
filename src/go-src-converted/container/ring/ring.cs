// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ring implements operations on circular lists.
namespace go.container;

partial class ring_package {

// A Ring is an element of a circular list, or ring.
// Rings do not have a beginning or end; a pointer to any ring element
// serves as reference to the entire ring. Empty rings are represented
// as nil Ring pointers. The zero value for a Ring is a one-element
// ring with a nil Value.
[GoType] partial struct Ring {
    internal ж<Ring> next;
    internal ж<Ring> prev;
    public any Value; // for use by client; untouched by this library
}

[GoRecv("capture")] internal static ж<Ring> init(this ref Ring r) {
    r.next = r;
    r.prev = r;
    return initꓸᏑr;
}

// Next returns the next ring element. r must not be empty.
[GoRecv] public static ж<Ring> Next(this ref Ring r) {
    if (r.next == nil) {
        return r.init();
    }
    return r.next;
}

// Prev returns the previous ring element. r must not be empty.
[GoRecv] public static ж<Ring> Prev(this ref Ring r) {
    if (r.next == nil) {
        return r.init();
    }
    return r.prev;
}

// Move moves n % r.Len() elements backward (n < 0) or forward (n >= 0)
// in the ring and returns that ring element. r must not be empty.
[GoRecv("capture")] public static ж<Ring> Move(this ref Ring r, nint n) {
    if (r.next == nil) {
        return r.init();
    }
    switch (ᐧ) {
    case {} when n is < 0: {
        for (; n < 0; n++) {
            r = r.prev;
        }
        break;
    }
    case {} when n is > 0: {
        for (; n > 0; n--) {
            r = r.next;
        }
        break;
    }}

    return MoveꓸᏑr;
}

// New creates a ring of n elements.
public static ж<Ring> New(nint n) {
    if (n <= 0) {
        return default!;
    }
    var r = @new<Ring>();
    var p = r;
    for (nint i = 1; i < n; i++) {
        p.val.next = Ꮡ(new Ring(prev: p));
        p = p.val.next;
    }
    p.val.next = r;
    r.val.prev = p;
    return r;
}

// Link connects ring r with ring s such that r.Next()
// becomes s and returns the original value for r.Next().
// r must not be empty.
//
// If r and s point to the same ring, linking
// them removes the elements between r and s from the ring.
// The removed elements form a subring and the result is a
// reference to that subring (if no elements were removed,
// the result is still the original value for r.Next(),
// and not nil).
//
// If r and s point to different rings, linking
// them creates a single ring with the elements of s inserted
// after r. The result points to the element following the
// last element of s after insertion.
[GoRecv] public static ж<Ring> Link(this ref Ring r, ж<Ring> Ꮡs) {
    ref var s = ref Ꮡs.val;

    var n = r.Next();
    if (s != nil) {
        var p = s.Prev();
        // Note: Cannot use multiple assignment because
        // evaluation order of LHS is not specified.
        r.next = s;
        s.prev = r;
        n.val.prev = p;
        p.val.next = n;
    }
    return n;
}

// Unlink removes n % r.Len() elements from the ring r, starting
// at r.Next(). If n % r.Len() == 0, r remains unchanged.
// The result is the removed subring. r must not be empty.
[GoRecv] public static ж<Ring> Unlink(this ref Ring r, nint n) {
    if (n <= 0) {
        return default!;
    }
    return r.Link(r.Move(n + 1));
}

// Len computes the number of elements in ring r.
// It executes in time proportional to the number of elements.
[GoRecv] public static nint Len(this ref Ring r) {
    nint n = 0;
    if (r != nil) {
        n = 1;
        for (var p = r.Next(); p != r; p = p.val.next) {
            n++;
        }
    }
    return n;
}

// Do calls function f on each element of the ring, in forward order.
// The behavior of Do is undefined if f changes *r.
[GoRecv] public static void Do(this ref Ring r, Action<any> f) {
    if (r != nil) {
        f(r.Value);
        for (var p = r.Next(); p != r; p = p.val.next) {
            f((~p).Value);
        }
    }
}

} // end ring_package
