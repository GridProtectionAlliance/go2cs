// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package heap provides heap operations for any type that implements
// heap.Interface. A heap is a tree with the property that each node is the
// minimum-valued node in its subtree.
//
// The minimum element in the tree is the root, at index 0.
//
// A heap is a common way to implement a priority queue. To build a priority
// queue, implement the Heap interface with the (negative) priority as the
// ordering for the Less method, so Push adds items while Pop removes the
// highest-priority item from the queue. The Examples include such an
// implementation; the file example_pq_test.go has the complete source.
//
// package heap -- go2cs converted at 2020 October 09 05:19:29 UTC
// import "container/heap" ==> using heap = go.container.heap_package
// Original source: C:\Go\src\container\heap\heap.go
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace container
{
    public static partial class heap_package
    {
        // The Interface type describes the requirements
        // for a type using the routines in this package.
        // Any type that implements it may be used as a
        // min-heap with the following invariants (established after
        // Init has been called or if the data is empty or sorted):
        //
        //    !h.Less(j, i) for 0 <= i < h.Len() and 2*i+1 <= j <= 2*i+2 and j < h.Len()
        //
        // Note that Push and Pop in this interface are for package heap's
        // implementation to call. To add and remove things from the heap,
        // use heap.Push and heap.Pop.
        public partial interface Interface : sort.Interface
        {
            void Push(object x); // add x as element Len()
            void Pop(); // remove and return element Len() - 1.
        }

        // Init establishes the heap invariants required by the other routines in this package.
        // Init is idempotent with respect to the heap invariants
        // and may be called whenever the heap invariants may have been invalidated.
        // The complexity is O(n) where n = h.Len().
        public static void Init(Interface h)
        { 
            // heapify
            var n = h.Len();
            for (var i = n / 2L - 1L; i >= 0L; i--)
            {
                down(h, i, n);
            }


        }

        // Push pushes the element x onto the heap.
        // The complexity is O(log n) where n = h.Len().
        public static void Push(Interface h, object x)
        {
            h.Push(x);
            up(h, h.Len() - 1L);
        }

        // Pop removes and returns the minimum element (according to Less) from the heap.
        // The complexity is O(log n) where n = h.Len().
        // Pop is equivalent to Remove(h, 0).
        public static void Pop(Interface h)
        {
            var n = h.Len() - 1L;
            h.Swap(0L, n);
            down(h, 0L, n);
            return h.Pop();
        }

        // Remove removes and returns the element at index i from the heap.
        // The complexity is O(log n) where n = h.Len().
        public static void Remove(Interface h, long i)
        {
            var n = h.Len() - 1L;
            if (n != i)
            {
                h.Swap(i, n);
                if (!down(h, i, n))
                {
                    up(h, i);
                }

            }

            return h.Pop();

        }

        // Fix re-establishes the heap ordering after the element at index i has changed its value.
        // Changing the value of the element at index i and then calling Fix is equivalent to,
        // but less expensive than, calling Remove(h, i) followed by a Push of the new value.
        // The complexity is O(log n) where n = h.Len().
        public static void Fix(Interface h, long i)
        {
            if (!down(h, i, h.Len()))
            {
                up(h, i);
            }

        }

        private static void up(Interface h, long j)
        {
            while (true)
            {
                var i = (j - 1L) / 2L; // parent
                if (i == j || !h.Less(j, i))
                {
                    break;
                }

                h.Swap(i, j);
                j = i;

            }


        }

        private static bool down(Interface h, long i0, long n)
        {
            var i = i0;
            while (true)
            {
                long j1 = 2L * i + 1L;
                if (j1 >= n || j1 < 0L)
                { // j1 < 0 after int overflow
                    break;

                }

                var j = j1; // left child
                {
                    var j2 = j1 + 1L;

                    if (j2 < n && h.Less(j2, j1))
                    {
                        j = j2; // = 2*i + 2  // right child
                    }

                }

                if (!h.Less(j, i))
                {
                    break;
                }

                h.Swap(i, j);
                i = j;

            }

            return i > i0;

        }
    }
}}
