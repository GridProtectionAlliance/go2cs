// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:11:36 UTC
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
        private static readonly rbrank rankLeaf = (rbrank)1L;
        private static readonly rbrank rankZero = (rbrank)0L;


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

        private static @string String(this ptr<RBTint32> _addr_t)
        {
            ref RBTint32 t = ref _addr_t.val;

            if (t.root == null)
            {
                return "[]";
            }

            return "[" + t.root.String() + "]";

        }

        private static @string String(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

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
        private static ptr<node32> makeNode(this ptr<RBTint32> _addr_t, int key)
        {
            ref RBTint32 t = ref _addr_t.val;

            return addr(new node32(key:key,rank:rankLeaf));
        }

        // IsEmpty reports whether t is empty.
        private static bool IsEmpty(this ptr<RBTint32> _addr_t)
        {
            ref RBTint32 t = ref _addr_t.val;

            return t.root == null;
        }

        // IsSingle reports whether t is a singleton (leaf).
        private static bool IsSingle(this ptr<RBTint32> _addr_t)
        {
            ref RBTint32 t = ref _addr_t.val;

            return t.root != null && t.root.isLeaf();
        }

        // VisitInOrder applies f to the key and data pairs in t,
        // with keys ordered from smallest to largest.
        private static void VisitInOrder(this ptr<RBTint32> _addr_t, Action<int, object> f)
        {
            ref RBTint32 t = ref _addr_t.val;

            if (t.root == null)
            {
                return ;
            }

            t.root.visitInOrder(f);

        }

        private static void Data(this ptr<node32> _addr_n)
        {
            ref node32 n = ref _addr_n.val;

            if (n == null)
            {
                return null;
            }

            return n.data;

        }

        private static (int, object) keyAndData(this ptr<node32> _addr_n)
        {
            int k = default;
            object d = default;
            ref node32 n = ref _addr_n.val;

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

            return ;

        }

        private static rbrank Rank(this ptr<node32> _addr_n)
        {
            ref node32 n = ref _addr_n.val;

            if (n == null)
            {
                return 0L;
            }

            return n.rank;

        }

        // Find returns the data associated with key in the tree, or
        // nil if key is not in the tree.
        private static void Find(this ptr<RBTint32> _addr_t, int key)
        {
            ref RBTint32 t = ref _addr_t.val;

            return t.root.find(key).Data();
        }

        // Insert adds key to the tree and associates key with data.
        // If key was already in the tree, it updates the associated data.
        // Insert returns the previous data associated with key,
        // or nil if key was not present.
        // Insert panics if data is nil.
        private static void Insert(this ptr<RBTint32> _addr_t, int key, object data) => func((_, panic, __) =>
        {
            ref RBTint32 t = ref _addr_t.val;

            if (data == null)
            {
                panic("Cannot insert nil data into tree");
            }

            var n = t.root;
            ptr<node32> newroot;
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
        private static (int, object) Min(this ptr<RBTint32> _addr_t)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.min().keyAndData();
        }

        // Max returns the maximum element of t and its associated data.
        // If t is empty, then (0, nil) is returned.
        private static (int, object) Max(this ptr<RBTint32> _addr_t)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.max().keyAndData();
        }

        // Glb returns the greatest-lower-bound-exclusive of x and its associated
        // data.  If x has no glb in the tree, then (0, nil) is returned.
        private static (int, object) Glb(this ptr<RBTint32> _addr_t, int x)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.glb(x, false).keyAndData();
        }

        // GlbEq returns the greatest-lower-bound-inclusive of x and its associated
        // data.  If x has no glbEQ in the tree, then (0, nil) is returned.
        private static (int, object) GlbEq(this ptr<RBTint32> _addr_t, int x)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.glb(x, true).keyAndData();
        }

        // Lub returns the least-upper-bound-exclusive of x and its associated
        // data.  If x has no lub in the tree, then (0, nil) is returned.
        private static (int, object) Lub(this ptr<RBTint32> _addr_t, int x)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.lub(x, false).keyAndData();
        }

        // LubEq returns the least-upper-bound-inclusive of x and its associated
        // data.  If x has no lubEq in the tree, then (0, nil) is returned.
        private static (int, object) LubEq(this ptr<RBTint32> _addr_t, int x)
        {
            int k = default;
            object d = default;
            ref RBTint32 t = ref _addr_t.val;

            return t.root.lub(x, true).keyAndData();
        }

        private static bool isLeaf(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

            return t.left == null && t.right == null;
        }

        private static void visitInOrder(this ptr<node32> _addr_t, Action<int, object> f)
        {
            ref node32 t = ref _addr_t.val;

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

        private static rbrank maxChildRank(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

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

        private static rbrank minChildRank(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

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

        private static ptr<node32> find(this ptr<node32> _addr_t, int key)
        {
            ref node32 t = ref _addr_t.val;

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
                    return _addr_t!;
                }

            }

            return _addr_null!;

        }

        private static ptr<node32> min(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_t!;
            }

            while (t.left != null)
            {
                t = t.left;
            }

            return _addr_t!;

        }

        private static ptr<node32> max(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_t!;
            }

            while (t.right != null)
            {
                t = t.right;
            }

            return _addr_t!;

        }

        private static ptr<node32> glb(this ptr<node32> _addr_t, int key, bool allow_eq)
        {
            ref node32 t = ref _addr_t.val;

            ptr<node32> best;
            while (t != null)
            {
                if (key <= t.key)
                {
                    if (key == t.key && allow_eq)
                    {
                        return _addr_t!;
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

            return _addr_best!;

        }

        private static ptr<node32> lub(this ptr<node32> _addr_t, int key, bool allow_eq)
        {
            ref node32 t = ref _addr_t.val;

            ptr<node32> best;
            while (t != null)
            {
                if (key >= t.key)
                {
                    if (key == t.key && allow_eq)
                    {
                        return _addr_t!;
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

            return _addr_best!;

        }

        private static (ptr<node32>, ptr<node32>) insert(this ptr<node32> _addr_t, int x, ptr<RBTint32> _addr_w)
        {
            ptr<node32> newroot = default!;
            ptr<node32> newnode = default!;
            ref node32 t = ref _addr_t.val;
            ref RBTint32 w = ref _addr_w.val;
 
            // defaults
            newroot = t;
            newnode = t;
            if (x == t.key)
            {
                return ;
            }

            if (x < t.key)
            {
                if (t.left == null)
                {
                    var n = w.makeNode(x);
                    n.parent = t;
                    t.left = n;
                    newnode = n;
                    return ;
                }

                ptr<node32> new_l;
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
                        return ;

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
                    return ;
                }

                ptr<node32> new_r;
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
                        return ;

                    }
                    else
                    {
                        t.rank = newrank;
                    }

                }

            }

            return ;

        }

        private static ptr<node32> rightToRoot(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;
 
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

            return _addr_right!;

        }

        private static ptr<node32> leftToRoot(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;
 
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

            return _addr_left!;

        }

        // next returns the successor of t in a left-to-right
        // walk of the tree in which t is embedded.
        private static ptr<node32> next(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;
 
            // If there is a right child, it is to the right
            var r = t.right;
            if (r != null)
            {
                return _addr_r.min()!;
            } 
            // if t is p.left, then p, else repeat.
            var p = t.parent;
            while (p != null)
            {
                if (p.left == t)
                {
                    return _addr_p!;
                }

                t = p;
                p = t.parent;

            }

            return _addr_null!;

        }

        // prev returns the predecessor of t in a left-to-right
        // walk of the tree in which t is embedded.
        private static ptr<node32> prev(this ptr<node32> _addr_t)
        {
            ref node32 t = ref _addr_t.val;
 
            // If there is a left child, it is to the left
            var l = t.left;
            if (l != null)
            {
                return _addr_l.max()!;
            } 
            // if t is p.right, then p, else repeat.
            var p = t.parent;
            while (p != null)
            {
                if (p.right == t)
                {
                    return _addr_p!;
                }

                t = p;
                p = t.parent;

            }

            return _addr_null!;

        }
    }
}}}}
