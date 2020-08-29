// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:49 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\redblack32.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static readonly rbrank rankLeaf = 1L;
        private static readonly rbrank rankZero = 0L;

        private partial struct rbrank // : sbyte
        {
        }

        // RBTint32 is a red-black tree with data stored at internal nodes,
        // following Tarjan, Data Structures and Network Algorithms,
        // pp 48-52, using explicit rank instead of red and black.
        // Deletion is not yet implemented because it is not yet needed.
        // Extra operations glb, lub, glbEq, lubEq are provided for
        // use in sparse lookup algorithms.
        public partial struct RBTint32
        {
            public ptr<node32> root; // An extra-clever implementation will have special cases
// for small sets, but we are not extra-clever today.
        }

        private static @string String(this ref RBTint32 t)
        {
            if (t.root == null)
            {
                return "[]";
            }
            return "[" + t.root.String() + "]";
        }

        private static @string String(this ref node32 t)
        {
            @string s = "";
            if (t.left != null)
            {
                s = t.left.String() + " ";
            }
            s = s + fmt.Sprintf("k=%d,d=%v", t.key, t.data);
            if (t.right != null)
            {
                s = s + " " + t.right.String();
            }
            return s;
        }

        private partial struct node32
        {
            public ptr<node32> left;
            public ptr<node32> right;
            public ptr<node32> parent;
            public int key;
            public rbrank rank; // From Tarjan pp 48-49:
// If x is a node with a parent, then x.rank <= x.parent.rank <= x.rank+1.
// If x is a node with a grandparent, then x.rank < x.parent.parent.rank.
// If x is an "external [null] node", then x.rank = 0 && x.parent.rank = 1.
// Any node with one or more null children should have rank = 1.
        }

        // makeNode returns a new leaf node with the given key and nil data.
        private static ref node32 makeNode(this ref RBTint32 t, int key)
        {
            return ref new node32(key:key,rank:rankLeaf);
        }

        // IsEmpty reports whether t is empty.
        private static bool IsEmpty(this ref RBTint32 t)
        {
            return t.root == null;
        }

        // IsSingle reports whether t is a singleton (leaf).
        private static bool IsSingle(this ref RBTint32 t)
        {
            return t.root != null && t.root.isLeaf();
        }

        // VisitInOrder applies f to the key and data pairs in t,
        // with keys ordered from smallest to largest.
        private static void VisitInOrder(this ref RBTint32 t, Action<int, object> f)
        {
            if (t.root == null)
            {
                return;
            }
            t.root.visitInOrder(f);
        }

        private static void Data(this ref node32 n)
        {
            if (n == null)
            {
                return null;
            }
            return n.data;
        }

        private static (int, object) keyAndData(this ref node32 n)
        {
            if (n == null)
            {
                k = 0L;
                d = null;
            }
            else
            {
                k = n.key;
                d = n.data;
            }
            return;
        }

        private static rbrank Rank(this ref node32 n)
        {
            if (n == null)
            {
                return 0L;
            }
            return n.rank;
        }

        // Find returns the data associated with key in the tree, or
        // nil if key is not in the tree.
        private static void Find(this ref RBTint32 t, int key)
        {
            return t.root.find(key).Data();
        }

        // Insert adds key to the tree and associates key with data.
        // If key was already in the tree, it updates the associated data.
        // Insert returns the previous data associated with key,
        // or nil if key was not present.
        // Insert panics if data is nil.
        private static void Insert(this ref RBTint32 _t, int key, object data) => func(_t, (ref RBTint32 t, Defer _, Panic panic, Recover __) =>
        {
            if (data == null)
            {
                panic("Cannot insert nil data into tree");
            }
            var n = t.root;
            ref node32 newroot = default;
            if (n == null)
            {
                n = t.makeNode(key);
                newroot = n;
            }
            else
            {
                newroot, n = n.insert(key, t);
            }
            var r = n.data;
            n.data = data;
            t.root = newroot;
            return r;
        });

        // Min returns the minimum element of t and its associated data.
        // If t is empty, then (0, nil) is returned.
        private static (int, object) Min(this ref RBTint32 t)
        {
            return t.root.min().keyAndData();
        }

        // Max returns the maximum element of t and its associated data.
        // If t is empty, then (0, nil) is returned.
        private static (int, object) Max(this ref RBTint32 t)
        {
            return t.root.max().keyAndData();
        }

        // Glb returns the greatest-lower-bound-exclusive of x and its associated
        // data.  If x has no glb in the tree, then (0, nil) is returned.
        private static (int, object) Glb(this ref RBTint32 t, int x)
        {
            return t.root.glb(x, false).keyAndData();
        }

        // GlbEq returns the greatest-lower-bound-inclusive of x and its associated
        // data.  If x has no glbEQ in the tree, then (0, nil) is returned.
        private static (int, object) GlbEq(this ref RBTint32 t, int x)
        {
            return t.root.glb(x, true).keyAndData();
        }

        // Lub returns the least-upper-bound-exclusive of x and its associated
        // data.  If x has no lub in the tree, then (0, nil) is returned.
        private static (int, object) Lub(this ref RBTint32 t, int x)
        {
            return t.root.lub(x, false).keyAndData();
        }

        // LubEq returns the least-upper-bound-inclusive of x and its associated
        // data.  If x has no lubEq in the tree, then (0, nil) is returned.
        private static (int, object) LubEq(this ref RBTint32 t, int x)
        {
            return t.root.lub(x, true).keyAndData();
        }

        private static bool isLeaf(this ref node32 t)
        {
            return t.left == null && t.right == null;
        }

        private static void visitInOrder(this ref node32 t, Action<int, object> f)
        {
            if (t.left != null)
            {
                t.left.visitInOrder(f);
            }
            f(t.key, t.data);
            if (t.right != null)
            {
                t.right.visitInOrder(f);
            }
        }

        private static rbrank maxChildRank(this ref node32 t)
        {
            if (t.left == null)
            {
                if (t.right == null)
                {
                    return rankZero;
                }
                return t.right.rank;
            }
            if (t.right == null)
            {
                return t.left.rank;
            }
            if (t.right.rank > t.left.rank)
            {
                return t.right.rank;
            }
            return t.left.rank;
        }

        private static rbrank minChildRank(this ref node32 t)
        {
            if (t.left == null || t.right == null)
            {
                return rankZero;
            }
            if (t.right.rank < t.left.rank)
            {
                return t.right.rank;
            }
            return t.left.rank;
        }

        private static ref node32 find(this ref node32 t, int key)
        {
            while (t != null)
            {
                if (key < t.key)
                {
                    t = t.left;
                }
                else if (key > t.key)
                {
                    t = t.right;
                }
                else
                {
                    return t;
                }
            }

            return null;
        }

        private static ref node32 min(this ref node32 t)
        {
            if (t == null)
            {
                return t;
            }
            while (t.left != null)
            {
                t = t.left;
            }

            return t;
        }

        private static ref node32 max(this ref node32 t)
        {
            if (t == null)
            {
                return t;
            }
            while (t.right != null)
            {
                t = t.right;
            }

            return t;
        }

        private static ref node32 glb(this ref node32 t, int key, bool allow_eq)
        {
            ref node32 best = default;
            while (t != null)
            {
                if (key <= t.key)
                {
                    if (key == t.key && allow_eq)
                    {
                        return t;
                    } 
                    // t is too big, glb is to left.
                    t = t.left;
                }
                else
                { 
                    // t is a lower bound, record it and seek a better one.
                    best = t;
                    t = t.right;
                }
            }

            return best;
        }

        private static ref node32 lub(this ref node32 t, int key, bool allow_eq)
        {
            ref node32 best = default;
            while (t != null)
            {
                if (key >= t.key)
                {
                    if (key == t.key && allow_eq)
                    {
                        return t;
                    } 
                    // t is too small, lub is to right.
                    t = t.right;
                }
                else
                { 
                    // t is a upper bound, record it and seek a better one.
                    best = t;
                    t = t.left;
                }
            }

            return best;
        }

        private static (ref node32, ref node32) insert(this ref node32 t, int x, ref RBTint32 w)
        { 
            // defaults
            newroot = t;
            newnode = t;
            if (x == t.key)
            {
                return;
            }
            if (x < t.key)
            {
                if (t.left == null)
                {
                    var n = w.makeNode(x);
                    n.parent = t;
                    t.left = n;
                    newnode = n;
                    return;
                }
                ref node32 new_l = default;
                new_l, newnode = t.left.insert(x, w);
                t.left = new_l;
                new_l.parent = t;
                long newrank = 1L + new_l.maxChildRank();
                if (newrank > t.rank)
                {
                    if (newrank > 1L + t.right.Rank())
                    { // rotations required
                        if (new_l.left.Rank() < new_l.right.Rank())
                        { 
                            // double rotation
                            t.left = new_l.rightToRoot();
                        }
                        newroot = t.leftToRoot();
                        return;
                    }
                    else
                    {
                        t.rank = newrank;
                    }
                }
            }
            else
            { // x > t.key
                if (t.right == null)
                {
                    n = w.makeNode(x);
                    n.parent = t;
                    t.right = n;
                    newnode = n;
                    return;
                }
                ref node32 new_r = default;
                new_r, newnode = t.right.insert(x, w);
                t.right = new_r;
                new_r.parent = t;
                newrank = 1L + new_r.maxChildRank();
                if (newrank > t.rank)
                {
                    if (newrank > 1L + t.left.Rank())
                    { // rotations required
                        if (new_r.right.Rank() < new_r.left.Rank())
                        { 
                            // double rotation
                            t.right = new_r.leftToRoot();
                        }
                        newroot = t.rightToRoot();
                        return;
                    }
                    else
                    {
                        t.rank = newrank;
                    }
                }
            }
            return;
        }

        private static ref node32 rightToRoot(this ref node32 t)
        { 
            //    this
            // left  right
            //      rl   rr
            //
            // becomes
            //
            //       right
            //    this   rr
            // left  rl
            //
            var right = t.right;
            var rl = right.left;
            right.parent = t.parent;
            right.left = t;
            t.parent = right; 
            // parent's child ptr fixed in caller
            t.right = rl;
            if (rl != null)
            {
                rl.parent = t;
            }
            return right;
        }

        private static ref node32 leftToRoot(this ref node32 t)
        { 
            //     this
            //  left  right
            // ll  lr
            //
            // becomes
            //
            //    left
            //   ll  this
            //      lr  right
            //
            var left = t.left;
            var lr = left.right;
            left.parent = t.parent;
            left.right = t;
            t.parent = left; 
            // parent's child ptr fixed in caller
            t.left = lr;
            if (lr != null)
            {
                lr.parent = t;
            }
            return left;
        }

        // next returns the successor of t in a left-to-right
        // walk of the tree in which t is embedded.
        private static ref node32 next(this ref node32 t)
        { 
            // If there is a right child, it is to the right
            var r = t.right;
            if (r != null)
            {
                return r.min();
            } 
            // if t is p.left, then p, else repeat.
            var p = t.parent;
            while (p != null)
            {
                if (p.left == t)
                {
                    return p;
                }
                t = p;
                p = t.parent;
            }

            return null;
        }

        // prev returns the predecessor of t in a left-to-right
        // walk of the tree in which t is embedded.
        private static ref node32 prev(this ref node32 t)
        { 
            // If there is a left child, it is to the left
            var l = t.left;
            if (l != null)
            {
                return l.max();
            } 
            // if t is p.right, then p, else repeat.
            var p = t.parent;
            while (p != null)
            {
                if (p.right == t)
                {
                    return p;
                }
                t = p;
                p = t.parent;
            }

            return null;
        }
    }
}}}}
