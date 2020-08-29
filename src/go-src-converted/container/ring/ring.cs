// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ring implements operations on circular lists.
// package ring -- go2cs converted at 2020 August 29 10:10:44 UTC
// import "container/ring" ==> using ring = go.container.ring_package
// Original source: C:\Go\src\container\ring\ring.go

using static go.builtin;
using System;

namespace go {
namespace container
{
    public static partial class ring_package
    {
        // A Ring is an element of a circular list, or ring.
        // Rings do not have a beginning or end; a pointer to any ring element
        // serves as reference to the entire ring. Empty rings are represented
        // as nil Ring pointers. The zero value for a Ring is a one-element
        // ring with a nil Value.
        //
        public partial struct Ring
        {
            public ptr<Ring> next;
            public ptr<Ring> prev;
        }

        private static ref Ring init(this ref Ring r)
        {
            r.next = r;
            r.prev = r;
            return r;
        }

        // Next returns the next ring element. r must not be empty.
        private static ref Ring Next(this ref Ring r)
        {
            if (r.next == null)
            {
                return r.init();
            }
            return r.next;
        }

        // Prev returns the previous ring element. r must not be empty.
        private static ref Ring Prev(this ref Ring r)
        {
            if (r.next == null)
            {
                return r.init();
            }
            return r.prev;
        }

        // Move moves n % r.Len() elements backward (n < 0) or forward (n >= 0)
        // in the ring and returns that ring element. r must not be empty.
        //
        private static ref Ring Move(this ref Ring r, long n)
        {
            if (r.next == null)
            {
                return r.init();
            }

            if (n < 0L) 
                while (n < 0L)
                {
                    r = r.prev;
                    n++;
                }
            else if (n > 0L) 
                while (n > 0L)
                {
                    r = r.next;
                    n--;
                }
                        return r;
        }

        // New creates a ring of n elements.
        public static ref Ring New(long n)
        {
            if (n <= 0L)
            {
                return null;
            }
            ptr<Ring> r = @new<Ring>();
            var p = r;
            for (long i = 1L; i < n; i++)
            {
                p.next = ref new Ring(prev:p);
                p = p.next;
            }

            p.next = r;
            r.prev = p;
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
        //
        private static ref Ring Link(this ref Ring r, ref Ring s)
        {
            var n = r.Next();
            if (s != null)
            {
                var p = s.Prev(); 
                // Note: Cannot use multiple assignment because
                // evaluation order of LHS is not specified.
                r.next = s;
                s.prev = r;
                n.prev = p;
                p.next = n;
            }
            return n;
        }

        // Unlink removes n % r.Len() elements from the ring r, starting
        // at r.Next(). If n % r.Len() == 0, r remains unchanged.
        // The result is the removed subring. r must not be empty.
        //
        private static ref Ring Unlink(this ref Ring r, long n)
        {
            if (n <= 0L)
            {
                return null;
            }
            return r.Link(r.Move(n + 1L));
        }

        // Len computes the number of elements in ring r.
        // It executes in time proportional to the number of elements.
        //
        private static long Len(this ref Ring r)
        {
            long n = 0L;
            if (r != null)
            {
                n = 1L;
                {
                    var p = r.Next();

                    while (p != r)
                    {
                        n++;
                        p = p.next;
                    }

                }
            }
            return n;
        }

        // Do calls function f on each element of the ring, in forward order.
        // The behavior of Do is undefined if f changes *r.
        private static void Do(this ref Ring r, Action<object> f)
        {
            if (r != null)
            {
                f(r.Value);
                {
                    var p = r.Next();

                    while (p != r)
                    {
                        f(p.Value);
                        p = p.next;
                    }

                }
            }
        }
    }
}}
