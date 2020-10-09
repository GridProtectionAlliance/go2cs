// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ring implements operations on circular lists.
// package ring -- go2cs converted at 2020 October 09 06:05:16 UTC
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

        private static ptr<Ring> init(this ptr<Ring> _addr_r)
        {
            ref Ring r = ref _addr_r.val;

            r.next = r;
            r.prev = r;
            return _addr_r!;
        }

        // Next returns the next ring element. r must not be empty.
        private static ptr<Ring> Next(this ptr<Ring> _addr_r)
        {
            ref Ring r = ref _addr_r.val;

            if (r.next == null)
            {
                return _addr_r.init()!;
            }

            return _addr_r.next!;

        }

        // Prev returns the previous ring element. r must not be empty.
        private static ptr<Ring> Prev(this ptr<Ring> _addr_r)
        {
            ref Ring r = ref _addr_r.val;

            if (r.next == null)
            {
                return _addr_r.init()!;
            }

            return _addr_r.prev!;

        }

        // Move moves n % r.Len() elements backward (n < 0) or forward (n >= 0)
        // in the ring and returns that ring element. r must not be empty.
        //
        private static ptr<Ring> Move(this ptr<Ring> _addr_r, long n)
        {
            ref Ring r = ref _addr_r.val;

            if (r.next == null)
            {
                return _addr_r.init()!;
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
                        return _addr_r!;

        }

        // New creates a ring of n elements.
        public static ptr<Ring> New(long n)
        {
            if (n <= 0L)
            {
                return _addr_null!;
            }

            ptr<Ring> r = @new<Ring>();
            var p = r;
            for (long i = 1L; i < n; i++)
            {
                p.next = addr(new Ring(prev:p));
                p = p.next;
            }

            p.next = r;
            r.prev = p;
            return _addr_r!;

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
        private static ptr<Ring> Link(this ptr<Ring> _addr_r, ptr<Ring> _addr_s)
        {
            ref Ring r = ref _addr_r.val;
            ref Ring s = ref _addr_s.val;

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

            return _addr_n!;

        }

        // Unlink removes n % r.Len() elements from the ring r, starting
        // at r.Next(). If n % r.Len() == 0, r remains unchanged.
        // The result is the removed subring. r must not be empty.
        //
        private static ptr<Ring> Unlink(this ptr<Ring> _addr_r, long n)
        {
            ref Ring r = ref _addr_r.val;

            if (n <= 0L)
            {
                return _addr_null!;
            }

            return _addr_r.Link(r.Move(n + 1L))!;

        }

        // Len computes the number of elements in ring r.
        // It executes in time proportional to the number of elements.
        //
        private static long Len(this ptr<Ring> _addr_r)
        {
            ref Ring r = ref _addr_r.val;

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
        private static void Do(this ptr<Ring> _addr_r, Action<object> f)
        {
            ref Ring r = ref _addr_r.val;

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
