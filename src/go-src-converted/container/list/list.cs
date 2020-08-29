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
// package list -- go2cs converted at 2020 August 29 08:31:04 UTC
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
        private static ref Element Next(this ref Element e)
        {
            {
                var p = e.next;

                if (e.list != null && p != ref e.list.root)
                {
                    return p;
                }

            }
            return null;
        }

        // Prev returns the previous list element or nil.
        private static ref Element Prev(this ref Element e)
        {
            {
                var p = e.prev;

                if (e.list != null && p != ref e.list.root)
                {
                    return p;
                }

            }
            return null;
        }

        // List represents a doubly linked list.
        // The zero value for List is an empty list ready to use.
        public partial struct List
        {
            public Element root; // sentinel list element, only &root, root.prev, and root.next are used
            public long len; // current list length excluding (this) sentinel element
        }

        // Init initializes or clears list l.
        private static ref List Init(this ref List l)
        {
            l.root.next = ref l.root;
            l.root.prev = ref l.root;
            l.len = 0L;
            return l;
        }

        // New returns an initialized list.
        public static ref List New()
        {
            return @new<List>().Init();
        }

        // Len returns the number of elements of list l.
        // The complexity is O(1).
        private static long Len(this ref List l)
        {
            return l.len;
        }

        // Front returns the first element of list l or nil if the list is empty.
        private static ref Element Front(this ref List l)
        {
            if (l.len == 0L)
            {
                return null;
            }
            return l.root.next;
        }

        // Back returns the last element of list l or nil if the list is empty.
        private static ref Element Back(this ref List l)
        {
            if (l.len == 0L)
            {
                return null;
            }
            return l.root.prev;
        }

        // lazyInit lazily initializes a zero List value.
        private static void lazyInit(this ref List l)
        {
            if (l.root.next == null)
            {
                l.Init();
            }
        }

        // insert inserts e after at, increments l.len, and returns e.
        private static ref Element insert(this ref List l, ref Element e, ref Element at)
        {
            var n = at.next;
            at.next = e;
            e.prev = at;
            e.next = n;
            n.prev = e;
            e.list = l;
            l.len++;
            return e;
        }

        // insertValue is a convenience wrapper for insert(&Element{Value: v}, at).
        private static ref Element insertValue(this ref List l, object v, ref Element at)
        {
            return l.insert(ref new Element(Value:v), at);
        }

        // remove removes e from its list, decrements l.len, and returns e.
        private static ref Element remove(this ref List l, ref Element e)
        {
            e.prev.next = e.next;
            e.next.prev = e.prev;
            e.next = null; // avoid memory leaks
            e.prev = null; // avoid memory leaks
            e.list = null;
            l.len--;
            return e;
        }

        // Remove removes e from l if e is an element of list l.
        // It returns the element value e.Value.
        // The element must not be nil.
        private static void Remove(this ref List l, ref Element e)
        {
            if (e.list == l)
            { 
                // if e.list == l, l must have been initialized when e was inserted
                // in l or l == nil (e is a zero Element) and l.remove will crash
                l.remove(e);
            }
            return e.Value;
        }

        // PushFront inserts a new element e with value v at the front of list l and returns e.
        private static ref Element PushFront(this ref List l, object v)
        {
            l.lazyInit();
            return l.insertValue(v, ref l.root);
        }

        // PushBack inserts a new element e with value v at the back of list l and returns e.
        private static ref Element PushBack(this ref List l, object v)
        {
            l.lazyInit();
            return l.insertValue(v, l.root.prev);
        }

        // InsertBefore inserts a new element e with value v immediately before mark and returns e.
        // If mark is not an element of l, the list is not modified.
        // The mark must not be nil.
        private static ref Element InsertBefore(this ref List l, object v, ref Element mark)
        {
            if (mark.list != l)
            {
                return null;
            } 
            // see comment in List.Remove about initialization of l
            return l.insertValue(v, mark.prev);
        }

        // InsertAfter inserts a new element e with value v immediately after mark and returns e.
        // If mark is not an element of l, the list is not modified.
        // The mark must not be nil.
        private static ref Element InsertAfter(this ref List l, object v, ref Element mark)
        {
            if (mark.list != l)
            {
                return null;
            } 
            // see comment in List.Remove about initialization of l
            return l.insertValue(v, mark);
        }

        // MoveToFront moves element e to the front of list l.
        // If e is not an element of l, the list is not modified.
        // The element must not be nil.
        private static void MoveToFront(this ref List l, ref Element e)
        {
            if (e.list != l || l.root.next == e)
            {
                return;
            } 
            // see comment in List.Remove about initialization of l
            l.insert(l.remove(e), ref l.root);
        }

        // MoveToBack moves element e to the back of list l.
        // If e is not an element of l, the list is not modified.
        // The element must not be nil.
        private static void MoveToBack(this ref List l, ref Element e)
        {
            if (e.list != l || l.root.prev == e)
            {
                return;
            } 
            // see comment in List.Remove about initialization of l
            l.insert(l.remove(e), l.root.prev);
        }

        // MoveBefore moves element e to its new position before mark.
        // If e or mark is not an element of l, or e == mark, the list is not modified.
        // The element and mark must not be nil.
        private static void MoveBefore(this ref List l, ref Element e, ref Element mark)
        {
            if (e.list != l || e == mark || mark.list != l)
            {
                return;
            }
            l.insert(l.remove(e), mark.prev);
        }

        // MoveAfter moves element e to its new position after mark.
        // If e or mark is not an element of l, or e == mark, the list is not modified.
        // The element and mark must not be nil.
        private static void MoveAfter(this ref List l, ref Element e, ref Element mark)
        {
            if (e.list != l || e == mark || mark.list != l)
            {
                return;
            }
            l.insert(l.remove(e), mark);
        }

        // PushBackList inserts a copy of an other list at the back of list l.
        // The lists l and other may be the same. They must not be nil.
        private static void PushBackList(this ref List l, ref List other)
        {
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

        // PushFrontList inserts a copy of an other list at the front of list l.
        // The lists l and other may be the same. They must not be nil.
        private static void PushFrontList(this ref List l, ref List other)
        {
            l.lazyInit();
            {
                var i = other.Len();
                var e = other.Back();

                while (i > 0L)
                {
                    l.insertValue(e.Value, ref l.root);
                    i = i - 1L;
                e = e.Prev();
                }

            }
        }
    }
}}
