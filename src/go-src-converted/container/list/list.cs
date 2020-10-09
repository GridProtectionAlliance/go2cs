// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package list implements a doubly linked list.
//
// To iterate over a list (where l is a *List):
//    for e := l.Front(); e != nil; e = e.Next() {
//        // do something with e.Value
//    }
//
// package list -- go2cs converted at 2020 October 09 04:55:12 UTC
// import "container/list" ==> using list = go.container.list_package
// Original source: C:\Go\src\container\list\list.go

using static go.builtin;

namespace go {
namespace container
{
    public static partial class list_package
    {
        // Element is an element of a linked list.
        public partial struct Element
        {
            public ptr<Element> next; // The list to which this element belongs.
            public ptr<Element> prev; // The list to which this element belongs.
            public ptr<List> list; // The value stored with this element.
        }

        // Next returns the next list element or nil.
        private static ptr<Element> Next(this ptr<Element> _addr_e)
        {
            ref Element e = ref _addr_e.val;

            {
                var p = e.next;

                if (e.list != null && p != _addr_e.list.root)
                {
                    return _addr_p!;
                }

            }

            return _addr_null!;

        }

        // Prev returns the previous list element or nil.
        private static ptr<Element> Prev(this ptr<Element> _addr_e)
        {
            ref Element e = ref _addr_e.val;

            {
                var p = e.prev;

                if (e.list != null && p != _addr_e.list.root)
                {
                    return _addr_p!;
                }

            }

            return _addr_null!;

        }

        // List represents a doubly linked list.
        // The zero value for List is an empty list ready to use.
        public partial struct List
        {
            public Element root; // sentinel list element, only &root, root.prev, and root.next are used
            public long len; // current list length excluding (this) sentinel element
        }

        // Init initializes or clears list l.
        private static ptr<List> Init(this ptr<List> _addr_l)
        {
            ref List l = ref _addr_l.val;

            l.root.next = _addr_l.root;
            l.root.prev = _addr_l.root;
            l.len = 0L;
            return _addr_l!;
        }

        // New returns an initialized list.
        public static ptr<List> New()
        {
            return @new<List>().Init();
        }

        // Len returns the number of elements of list l.
        // The complexity is O(1).
        private static long Len(this ptr<List> _addr_l)
        {
            ref List l = ref _addr_l.val;

            return l.len;
        }

        // Front returns the first element of list l or nil if the list is empty.
        private static ptr<Element> Front(this ptr<List> _addr_l)
        {
            ref List l = ref _addr_l.val;

            if (l.len == 0L)
            {
                return _addr_null!;
            }

            return _addr_l.root.next!;

        }

        // Back returns the last element of list l or nil if the list is empty.
        private static ptr<Element> Back(this ptr<List> _addr_l)
        {
            ref List l = ref _addr_l.val;

            if (l.len == 0L)
            {
                return _addr_null!;
            }

            return _addr_l.root.prev!;

        }

        // lazyInit lazily initializes a zero List value.
        private static void lazyInit(this ptr<List> _addr_l)
        {
            ref List l = ref _addr_l.val;

            if (l.root.next == null)
            {
                l.Init();
            }

        }

        // insert inserts e after at, increments l.len, and returns e.
        private static ptr<Element> insert(this ptr<List> _addr_l, ptr<Element> _addr_e, ptr<Element> _addr_at)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;
            ref Element at = ref _addr_at.val;

            e.prev = at;
            e.next = at.next;
            e.prev.next = e;
            e.next.prev = e;
            e.list = l;
            l.len++;
            return _addr_e!;
        }

        // insertValue is a convenience wrapper for insert(&Element{Value: v}, at).
        private static ptr<Element> insertValue(this ptr<List> _addr_l, object v, ptr<Element> _addr_at)
        {
            ref List l = ref _addr_l.val;
            ref Element at = ref _addr_at.val;

            return _addr_l.insert(addr(new Element(Value:v)), at)!;
        }

        // remove removes e from its list, decrements l.len, and returns e.
        private static ptr<Element> remove(this ptr<List> _addr_l, ptr<Element> _addr_e)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;

            e.prev.next = e.next;
            e.next.prev = e.prev;
            e.next = null; // avoid memory leaks
            e.prev = null; // avoid memory leaks
            e.list = null;
            l.len--;
            return _addr_e!;

        }

        // move moves e to next to at and returns e.
        private static ptr<Element> move(this ptr<List> _addr_l, ptr<Element> _addr_e, ptr<Element> _addr_at)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;
            ref Element at = ref _addr_at.val;

            if (e == at)
            {
                return _addr_e!;
            }

            e.prev.next = e.next;
            e.next.prev = e.prev;

            e.prev = at;
            e.next = at.next;
            e.prev.next = e;
            e.next.prev = e;

            return _addr_e!;

        }

        // Remove removes e from l if e is an element of list l.
        // It returns the element value e.Value.
        // The element must not be nil.
        private static void Remove(this ptr<List> _addr_l, ptr<Element> _addr_e)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;

            if (e.list == l)
            { 
                // if e.list == l, l must have been initialized when e was inserted
                // in l or l == nil (e is a zero Element) and l.remove will crash
                l.remove(e);

            }

            return e.Value;

        }

        // PushFront inserts a new element e with value v at the front of list l and returns e.
        private static ptr<Element> PushFront(this ptr<List> _addr_l, object v)
        {
            ref List l = ref _addr_l.val;

            l.lazyInit();
            return _addr_l.insertValue(v, _addr_l.root)!;
        }

        // PushBack inserts a new element e with value v at the back of list l and returns e.
        private static ptr<Element> PushBack(this ptr<List> _addr_l, object v)
        {
            ref List l = ref _addr_l.val;

            l.lazyInit();
            return _addr_l.insertValue(v, l.root.prev)!;
        }

        // InsertBefore inserts a new element e with value v immediately before mark and returns e.
        // If mark is not an element of l, the list is not modified.
        // The mark must not be nil.
        private static ptr<Element> InsertBefore(this ptr<List> _addr_l, object v, ptr<Element> _addr_mark)
        {
            ref List l = ref _addr_l.val;
            ref Element mark = ref _addr_mark.val;

            if (mark.list != l)
            {
                return _addr_null!;
            } 
            // see comment in List.Remove about initialization of l
            return _addr_l.insertValue(v, mark.prev)!;

        }

        // InsertAfter inserts a new element e with value v immediately after mark and returns e.
        // If mark is not an element of l, the list is not modified.
        // The mark must not be nil.
        private static ptr<Element> InsertAfter(this ptr<List> _addr_l, object v, ptr<Element> _addr_mark)
        {
            ref List l = ref _addr_l.val;
            ref Element mark = ref _addr_mark.val;

            if (mark.list != l)
            {
                return _addr_null!;
            } 
            // see comment in List.Remove about initialization of l
            return _addr_l.insertValue(v, mark)!;

        }

        // MoveToFront moves element e to the front of list l.
        // If e is not an element of l, the list is not modified.
        // The element must not be nil.
        private static void MoveToFront(this ptr<List> _addr_l, ptr<Element> _addr_e)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;

            if (e.list != l || l.root.next == e)
            {
                return ;
            } 
            // see comment in List.Remove about initialization of l
            l.move(e, _addr_l.root);

        }

        // MoveToBack moves element e to the back of list l.
        // If e is not an element of l, the list is not modified.
        // The element must not be nil.
        private static void MoveToBack(this ptr<List> _addr_l, ptr<Element> _addr_e)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;

            if (e.list != l || l.root.prev == e)
            {
                return ;
            } 
            // see comment in List.Remove about initialization of l
            l.move(e, l.root.prev);

        }

        // MoveBefore moves element e to its new position before mark.
        // If e or mark is not an element of l, or e == mark, the list is not modified.
        // The element and mark must not be nil.
        private static void MoveBefore(this ptr<List> _addr_l, ptr<Element> _addr_e, ptr<Element> _addr_mark)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;
            ref Element mark = ref _addr_mark.val;

            if (e.list != l || e == mark || mark.list != l)
            {
                return ;
            }

            l.move(e, mark.prev);

        }

        // MoveAfter moves element e to its new position after mark.
        // If e or mark is not an element of l, or e == mark, the list is not modified.
        // The element and mark must not be nil.
        private static void MoveAfter(this ptr<List> _addr_l, ptr<Element> _addr_e, ptr<Element> _addr_mark)
        {
            ref List l = ref _addr_l.val;
            ref Element e = ref _addr_e.val;
            ref Element mark = ref _addr_mark.val;

            if (e.list != l || e == mark || mark.list != l)
            {
                return ;
            }

            l.move(e, mark);

        }

        // PushBackList inserts a copy of another list at the back of list l.
        // The lists l and other may be the same. They must not be nil.
        private static void PushBackList(this ptr<List> _addr_l, ptr<List> _addr_other)
        {
            ref List l = ref _addr_l.val;
            ref List other = ref _addr_other.val;

            l.lazyInit();
            {
                var i = other.Len();
                var e = other.Front();

                while (i > 0L)
                {
                    l.insertValue(e.Value, l.root.prev);
                    i = i - 1L;
                e = e.Next();
                }

            }

        }

        // PushFrontList inserts a copy of another list at the front of list l.
        // The lists l and other may be the same. They must not be nil.
        private static void PushFrontList(this ptr<List> _addr_l, ptr<List> _addr_other)
        {
            ref List l = ref _addr_l.val;
            ref List other = ref _addr_other.val;

            l.lazyInit();
            {
                var i = other.Len();
                var e = other.Back();

                while (i > 0L)
                {
                    l.insertValue(e.Value, _addr_l.root);
                    i = i - 1L;
                e = e.Prev();
                }

            }

        }
    }
}}
