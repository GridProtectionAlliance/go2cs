// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Page heap.
//
// See malloc.go for the general overview.
//
// Large spans are the subject of this file. Spans consisting of less than
// _MaxMHeapLists are held in lists of like sized spans. Larger spans
// are held in a treap. See https://en.wikipedia.org/wiki/Treap or
// http://faculty.washington.edu/aragon/pubs/rst89.pdf for an overview.
// sema.go also holds an implementation of a treap.
//
// Each treapNode holds a single span. The treap is sorted by page size
// and for spans of the same size a secondary sort based on start address
// is done.
// Spans are returned based on a best fit algorithm and for spans of the same
// size the one at the lowest address is selected.
//
// The primary routines are
// insert: adds a span to the treap
// remove: removes the span from that treap that best fits the required size
// removeSpan: which removes a specific span from the treap
//
// _mheap.lock must be held when manipulating this data structure.

// package runtime -- go2cs converted at 2020 August 29 08:18:03 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgclarge.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        //go:notinheap
        private partial struct mTreap
        {
            public ptr<treapNode> treap;
        }

        //go:notinheap
        private partial struct treapNode
        {
            public ptr<treapNode> right; // all treapNodes > this treap node
            public ptr<treapNode> left; // all treapNodes < this treap node
            public ptr<treapNode> parent; // direct parent of this node, nil if root
            public System.UIntPtr npagesKey; // number of pages in spanKey, used as primary sort key
            public ptr<mspan> spanKey; // span of size npagesKey, used as secondary sort key
            public uint priority; // random number used by treap algorithm keep tree probablistically balanced
        }

        private static void init(this ref treapNode t)
        {
            t.right = null;
            t.left = null;
            t.parent = null;
            t.spanKey = null;
            t.npagesKey = 0L;
            t.priority = 0L;
        }

        // isSpanInTreap is handy for debugging. One should hold the heap lock, usually
        // mheap_.lock().
        private static bool isSpanInTreap(this ref treapNode t, ref mspan s)
        {
            if (t == null)
            {
                return false;
            }
            return t.spanKey == s || t.left.isSpanInTreap(s) || t.right.isSpanInTreap(s);
        }

        // walkTreap is handy for debugging.
        // Starting at some treapnode t, for example the root, do a depth first preorder walk of
        // the tree executing fn at each treap node. One should hold the heap lock, usually
        // mheap_.lock().
        private static void walkTreap(this ref treapNode t, Action<ref treapNode> fn)
        {
            if (t == null)
            {
                return;
            }
            fn(t);
            t.left.walkTreap(fn);
            t.right.walkTreap(fn);
        }

        // checkTreapNode when used in conjunction with walkTreap can usually detect a
        // poorly formed treap.
        private static void checkTreapNode(ref treapNode t)
        { 
            // lessThan is used to order the treap.
            // npagesKey and npages are the primary keys.
            // spanKey and span are the secondary keys.
            // span == nil (0) will always be lessThan all
            // spans of the same size.
            Func<System.UIntPtr, ref mspan, bool> lessThan = (npages, s) =>
            {
                if (t.npagesKey != npages)
                {
                    return t.npagesKey < npages;
                } 
                // t.npagesKey == npages
                return uintptr(@unsafe.Pointer(t.spanKey)) < uintptr(@unsafe.Pointer(s));
            }
;

            if (t == null)
            {
                return;
            }
            if (t.spanKey.npages != t.npagesKey || t.spanKey.next != null)
            {
                println("runtime: checkTreapNode treapNode t=", t, "     t.npagesKey=", t.npagesKey, "t.spanKey.npages=", t.spanKey.npages);
                throw("why does span.npages and treap.ngagesKey do not match?");
            }
            if (t.left != null && lessThan(t.left.npagesKey, t.left.spanKey))
            {
                throw("t.lessThan(t.left.npagesKey, t.left.spanKey) is not false");
            }
            if (t.right != null && !lessThan(t.right.npagesKey, t.right.spanKey))
            {
                throw("!t.lessThan(t.left.npagesKey, t.left.spanKey) is not false");
            }
        }

        // insert adds span to the large span treap.
        private static void insert(this ref mTreap root, ref mspan span)
        {
            var npages = span.npages;
            ref treapNode last = default;
            var pt = ref root.treap;
            {
                var t__prev1 = t;

                var t = pt.Value;

                while (t != null)
                {
                    last = t;
                    if (t.npagesKey < npages)
                    {
                        pt = ref t.right;
                    t = pt.Value;
                    }
                    else if (t.npagesKey > npages)
                    {
                        pt = ref t.left;
                    }
                    else if (uintptr(@unsafe.Pointer(t.spanKey)) < uintptr(@unsafe.Pointer(span)))
                    { 
                        // t.npagesKey == npages, so sort on span addresses.
                        pt = ref t.right;
                    }
                    else if (uintptr(@unsafe.Pointer(t.spanKey)) > uintptr(@unsafe.Pointer(span)))
                    {
                        pt = ref t.left;
                    }
                    else
                    {
                        throw("inserting span already in treap");
                    }
                } 

                // Add t as new leaf in tree of span size and unique addrs.
                // The balanced tree is a treap using priority as the random heap priority.
                // That is, it is a binary tree ordered according to the npagesKey,
                // but then among the space of possible binary trees respecting those
                // npagesKeys, it is kept balanced on average by maintaining a heap ordering
                // on the priority: s.priority <= both s.right.priority and s.right.priority.
                // https://en.wikipedia.org/wiki/Treap
                // http://faculty.washington.edu/aragon/pubs/rst89.pdf


                t = t__prev1;
            } 

            // Add t as new leaf in tree of span size and unique addrs.
            // The balanced tree is a treap using priority as the random heap priority.
            // That is, it is a binary tree ordered according to the npagesKey,
            // but then among the space of possible binary trees respecting those
            // npagesKeys, it is kept balanced on average by maintaining a heap ordering
            // on the priority: s.priority <= both s.right.priority and s.right.priority.
            // https://en.wikipedia.org/wiki/Treap
            // http://faculty.washington.edu/aragon/pubs/rst89.pdf

            t = (treapNode.Value)(mheap_.treapalloc.alloc());
            t.init();
            t.npagesKey = span.npages;
            t.priority = fastrand();
            t.spanKey = span;
            t.parent = last;
            pt.Value = t; // t now at a leaf.
            // Rotate up into tree according to priority.
            while (t.parent != null && t.parent.priority > t.priority)
            {
                if (t != null && t.spanKey.npages != t.npagesKey)
                {
                    println("runtime: insert t=", t, "t.npagesKey=", t.npagesKey);
                    println("runtime:      t.spanKey=", t.spanKey, "t.spanKey.npages=", t.spanKey.npages);
                    throw("span and treap sizes do not match?");
                }
                if (t.parent.left == t)
                {
                    root.rotateRight(t.parent);
                }
                else
                {
                    if (t.parent.right != t)
                    {
                        throw("treap insert finds a broken treap");
                    }
                    root.rotateLeft(t.parent);
                }
            }

        }

        private static void removeNode(this ref mTreap root, ref treapNode t)
        {
            if (t.spanKey.npages != t.npagesKey)
            {
                throw("span and treap node npages do not match");
            } 

            // Rotate t down to be leaf of tree for removal, respecting priorities.
            while (t.right != null || t.left != null)
            {
                if (t.right == null || t.left != null && t.left.priority < t.right.priority)
                {
                    root.rotateRight(t);
                }
                else
                {
                    root.rotateLeft(t);
                }
            } 
            // Remove t, now a leaf.
 
            // Remove t, now a leaf.
            if (t.parent != null)
            {
                if (t.parent.left == t)
                {
                    t.parent.left = null;
                }
                else
                {
                    t.parent.right = null;
                }
            }
            else
            {
                root.treap = null;
            } 
            // Return the found treapNode's span after freeing the treapNode.
            t.spanKey = null;
            t.npagesKey = 0L;
            mheap_.treapalloc.free(@unsafe.Pointer(t));
        }

        // remove searches for, finds, removes from the treap, and returns the smallest
        // span that can hold npages. If no span has at least npages return nil.
        // This is slightly more complicated than a simple binary tree search
        // since if an exact match is not found the next larger node is
        // returned.
        // If the last node inspected > npagesKey not holding
        // a left node (a smaller npages) is the "best fit" node.
        private static ref mspan remove(this ref mTreap root, System.UIntPtr npages)
        {
            var t = root.treap;
            while (t != null)
            {
                if (t.spanKey == null)
                {
                    throw("treap node with nil spanKey found");
                }
                if (t.npagesKey < npages)
                {
                    t = t.right;
                }
                else if (t.left != null && t.left.npagesKey >= npages)
                {
                    t = t.left;
                }
                else
                {
                    var result = t.spanKey;
                    root.removeNode(t);
                    return result;
                }
            }

            return null;
        }

        // removeSpan searches for, finds, deletes span along with
        // the associated treap node. If the span is not in the treap
        // then t will eventually be set to nil and the t.spanKey
        // will throw.
        private static void removeSpan(this ref mTreap root, ref mspan span)
        {
            var npages = span.npages;
            var t = root.treap;
            while (t.spanKey != span)
            {
                if (t.npagesKey < npages)
                {
                    t = t.right;
                }
                else if (t.npagesKey > npages)
                {
                    t = t.left;
                }
                else if (uintptr(@unsafe.Pointer(t.spanKey)) < uintptr(@unsafe.Pointer(span)))
                {
                    t = t.right;
                }
                else if (uintptr(@unsafe.Pointer(t.spanKey)) > uintptr(@unsafe.Pointer(span)))
                {
                    t = t.left;
                }
            }

            root.removeNode(t);
        }

        // scavengetreap visits each node in the treap and scavenges the
        // treapNode's span.
        private static System.UIntPtr scavengetreap(ref treapNode treap, ulong now, ulong limit)
        {
            if (treap == null)
            {
                return 0L;
            }
            return scavengeTreapNode(treap, now, limit) + scavengetreap(treap.left, now, limit) + scavengetreap(treap.right, now, limit);
        }

        // rotateLeft rotates the tree rooted at node x.
        // turning (x a (y b c)) into (y (x a b) c).
        private static void rotateLeft(this ref mTreap root, ref treapNode x)
        { 
            // p -> (x a (y b c))
            var p = x.parent;
            var a = x.left;
            var y = x.right;
            var b = y.left;
            var c = y.right;

            y.left = x;
            x.parent = y;
            y.right = c;
            if (c != null)
            {
                c.parent = y;
            }
            x.left = a;
            if (a != null)
            {
                a.parent = x;
            }
            x.right = b;
            if (b != null)
            {
                b.parent = x;
            }
            y.parent = p;
            if (p == null)
            {
                root.treap = y;
            }
            else if (p.left == x)
            {
                p.left = y;
            }
            else
            {
                if (p.right != x)
                {
                    throw("large span treap rotateLeft");
                }
                p.right = y;
            }
        }

        // rotateRight rotates the tree rooted at node y.
        // turning (y (x a b) c) into (x a (y b c)).
        private static void rotateRight(this ref mTreap root, ref treapNode y)
        { 
            // p -> (y (x a b) c)
            var p = y.parent;
            var x = y.left;
            var c = y.right;
            var a = x.left;
            var b = x.right;

            x.left = a;
            if (a != null)
            {
                a.parent = x;
            }
            x.right = y;
            y.parent = x;
            y.left = b;
            if (b != null)
            {
                b.parent = y;
            }
            y.right = c;
            if (c != null)
            {
                c.parent = y;
            }
            x.parent = p;
            if (p == null)
            {
                root.treap = x;
            }
            else if (p.left == y)
            {
                p.left = x;
            }
            else
            {
                if (p.right != y)
                {
                    throw("large span treap rotateRight");
                }
                p.right = x;
            }
        }
    }
}
