// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:11:28 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\poset.go
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // If true, check poset integrity after every mutation
        private static var debugPoset = false;

        private static readonly long uintSize = (long)32L << (int)((~uint(0L) >> (int)(32L) & 1L)); // 32 or 64

        // bitset is a bit array for dense indexes.
 // 32 or 64

        // bitset is a bit array for dense indexes.
        private partial struct bitset // : slice<ulong>
        {
        }

        private static bitset newBitset(long n)
        {
            return make(bitset, (n + uintSize - 1L) / uintSize);
        }

        private static void Reset(this bitset bs)
        {
            foreach (var (i) in bs)
            {
                bs[i] = 0L;
            }

        }

        private static void Set(this bitset bs, uint idx)
        {
            bs[idx / uintSize] |= 1L << (int)((idx % uintSize));
        }

        private static void Clear(this bitset bs, uint idx)
        {
            bs[idx / uintSize] &= 1L << (int)((idx % uintSize));
        }

        private static bool Test(this bitset bs, uint idx)
        {
            return bs[idx / uintSize] & (1L << (int)((idx % uintSize))) != 0L;
        }

        private partial struct undoType // : byte
        {
        }

        private static readonly undoType undoInvalid = (undoType)iota;
        private static readonly var undoCheckpoint = (var)0; // a checkpoint to group undo passes
        private static readonly var undoSetChl = (var)1; // change back left child of undo.idx to undo.edge
        private static readonly var undoSetChr = (var)2; // change back right child of undo.idx to undo.edge
        private static readonly var undoNonEqual = (var)3; // forget that SSA value undo.ID is non-equal to undo.idx (another ID)
        private static readonly var undoNewNode = (var)4; // remove new node created for SSA value undo.ID
        private static readonly var undoNewConstant = (var)5; // remove the constant node idx from the constants map
        private static readonly var undoAliasNode = (var)6; // unalias SSA value undo.ID so that it points back to node index undo.idx
        private static readonly var undoNewRoot = (var)7; // remove node undo.idx from root list
        private static readonly var undoChangeRoot = (var)8; // remove node undo.idx from root list, and put back undo.edge.Target instead
        private static readonly var undoMergeRoot = (var)9; // remove node undo.idx from root list, and put back its children instead

        // posetUndo represents an undo pass to be performed.
        // It's an union of fields that can be used to store information,
        // and typ is the discriminant, that specifies which kind
        // of operation must be performed. Not all fields are always used.
        private partial struct posetUndo
        {
            public undoType typ;
            public uint idx;
            public ID ID;
            public posetEdge edge;
        }

 
        // Make poset handle constants as unsigned numbers.
        private static readonly long posetFlagUnsigned = (long)1L << (int)(iota);


        // A poset edge. The zero value is the null/empty edge.
        // Packs target node index (31 bits) and strict flag (1 bit).
        private partial struct posetEdge // : uint
        {
        }

        private static posetEdge newedge(uint t, bool strict)
        {
            var s = uint32(0L);
            if (strict)
            {
                s = 1L;
            }

            return posetEdge(t << (int)(1L) | s);

        }
        private static uint Target(this posetEdge e)
        {
            return uint32(e) >> (int)(1L);
        }
        private static bool Strict(this posetEdge e)
        {
            return uint32(e) & 1L != 0L;
        }
        private static @string String(this posetEdge e)
        {
            var s = fmt.Sprint(e.Target());
            if (e.Strict())
            {
                s += "*";
            }

            return s;

        }

        // posetNode is a node of a DAG within the poset.
        private partial struct posetNode
        {
            public posetEdge l;
            public posetEdge r;
        }

        // poset is a union-find data structure that can represent a partially ordered set
        // of SSA values. Given a binary relation that creates a partial order (eg: '<'),
        // clients can record relations between SSA values using SetOrder, and later
        // check relations (in the transitive closure) with Ordered. For instance,
        // if SetOrder is called to record that A<B and B<C, Ordered will later confirm
        // that A<C.
        //
        // It is possible to record equality relations between SSA values with SetEqual and check
        // equality with Equal. Equality propagates into the transitive closure for the partial
        // order so that if we know that A<B<C and later learn that A==D, Ordered will return
        // true for D<C.
        //
        // It is also possible to record inequality relations between nodes with SetNonEqual;
        // non-equality relations are not transitive, but they can still be useful: for instance
        // if we know that A<=B and later we learn that A!=B, we can deduce that A<B.
        // NonEqual can be used to check whether it is known that the nodes are different, either
        // because SetNonEqual was called before, or because we know that they are strictly ordered.
        //
        // poset will refuse to record new relations that contradict existing relations:
        // for instance if A<B<C, calling SetOrder for C<A will fail returning false; also
        // calling SetEqual for C==A will fail.
        //
        // poset is implemented as a forest of DAGs; in each DAG, if there is a path (directed)
        // from node A to B, it means that A<B (or A<=B). Equality is represented by mapping
        // two SSA values to the same DAG node; when a new equality relation is recorded
        // between two existing nodes,the nodes are merged, adjusting incoming and outgoing edges.
        //
        // Constants are specially treated. When a constant is added to the poset, it is
        // immediately linked to other constants already present; so for instance if the
        // poset knows that x<=3, and then x is tested against 5, 5 is first added and linked
        // 3 (using 3<5), so that the poset knows that x<=3<5; at that point, it is able
        // to answer x<5 correctly. This means that all constants are always within the same
        // DAG; as an implementation detail, we enfoce that the DAG containtining the constants
        // is always the first in the forest.
        //
        // poset is designed to be memory efficient and do little allocations during normal usage.
        // Most internal data structures are pre-allocated and flat, so for instance adding a
        // new relation does not cause any allocation. For performance reasons,
        // each node has only up to two outgoing edges (like a binary tree), so intermediate
        // "dummy" nodes are required to represent more than two relations. For instance,
        // to record that A<I, A<J, A<K (with no known relation between I,J,K), we create the
        // following DAG:
        //
        //         A
        //        / \
        //       I  dummy
        //           /  \
        //          J    K
        //
        private partial struct poset
        {
            public uint lastidx; // last generated dense index
            public byte flags; // internal flags
            public map<ID, uint> values; // map SSA values to dense indexes
            public map<long, uint> constants; // record SSA constants together with their value
            public slice<posetNode> nodes; // nodes (in all DAGs)
            public slice<uint> roots; // list of root nodes (forest)
            public map<uint, bitset> noneq; // non-equal relations
            public slice<posetUndo> undo; // undo chain
        }

        private static ptr<poset> newPoset()
        {
            return addr(new poset(values:make(map[ID]uint32),constants:make(map[int64]uint32,8),nodes:make([]posetNode,1,16),roots:make([]uint32,0,4),noneq:make(map[uint32]bitset),undo:make([]posetUndo,0,4),));
        }

        private static void SetUnsigned(this ptr<poset> _addr_po, bool uns)
        {
            ref poset po = ref _addr_po.val;

            if (uns)
            {
                po.flags |= posetFlagUnsigned;
            }
            else
            {
                po.flags &= posetFlagUnsigned;
            }

        }

        // Handle children
        private static void setchl(this ptr<poset> _addr_po, uint i, posetEdge l)
        {
            ref poset po = ref _addr_po.val;

            po.nodes[i].l = l;
        }
        private static void setchr(this ptr<poset> _addr_po, uint i, posetEdge r)
        {
            ref poset po = ref _addr_po.val;

            po.nodes[i].r = r;
        }
        private static uint chl(this ptr<poset> _addr_po, uint i)
        {
            ref poset po = ref _addr_po.val;

            return po.nodes[i].l.Target();
        }
        private static uint chr(this ptr<poset> _addr_po, uint i)
        {
            ref poset po = ref _addr_po.val;

            return po.nodes[i].r.Target();
        }
        private static (posetEdge, posetEdge) children(this ptr<poset> _addr_po, uint i)
        {
            posetEdge _p0 = default;
            posetEdge _p0 = default;
            ref poset po = ref _addr_po.val;

            return (po.nodes[i].l, po.nodes[i].r);
        }

        // upush records a new undo step. It can be used for simple
        // undo passes that record up to one index and one edge.
        private static void upush(this ptr<poset> _addr_po, undoType typ, uint p, posetEdge e)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:typ,idx:p,edge:e));
        }

        // upushnew pushes an undo pass for a new node
        private static void upushnew(this ptr<poset> _addr_po, ID id, uint idx)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:undoNewNode,ID:id,idx:idx));
        }

        // upushneq pushes a new undo pass for a nonequal relation
        private static void upushneq(this ptr<poset> _addr_po, uint idx1, uint idx2)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:undoNonEqual,ID:ID(idx1),idx:idx2));
        }

        // upushalias pushes a new undo pass for aliasing two nodes
        private static void upushalias(this ptr<poset> _addr_po, ID id, uint i2)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:undoAliasNode,ID:id,idx:i2));
        }

        // upushconst pushes a new undo pass for a new constant
        private static void upushconst(this ptr<poset> _addr_po, uint idx, uint old)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:undoNewConstant,idx:idx,ID:ID(old)));
        }

        // addchild adds i2 as direct child of i1.
        private static void addchild(this ptr<poset> _addr_po, uint i1, uint i2, bool strict)
        {
            ref poset po = ref _addr_po.val;

            var (i1l, i1r) = po.children(i1);
            var e2 = newedge(i2, strict);

            if (i1l == 0L)
            {
                po.setchl(i1, e2);
                po.upush(undoSetChl, i1, 0L);
            }
            else if (i1r == 0L)
            {
                po.setchr(i1, e2);
                po.upush(undoSetChr, i1, 0L);
            }
            else
            { 
                // If n1 already has two children, add an intermediate dummy
                // node to record the relation correctly (without relating
                // n2 to other existing nodes). Use a non-deterministic value
                // to decide whether to append on the left or the right, to avoid
                // creating degenerated chains.
                //
                //      n1
                //     /  \
                //   i1l  dummy
                //        /   \
                //      i1r   n2
                //
                var dummy = po.newnode(null);
                if ((i1 ^ i2) & 1L != 0L)
                { // non-deterministic
                    po.setchl(dummy, i1r);
                    po.setchr(dummy, e2);
                    po.setchr(i1, newedge(dummy, false));
                    po.upush(undoSetChr, i1, i1r);

                }
                else
                {
                    po.setchl(dummy, i1l);
                    po.setchr(dummy, e2);
                    po.setchl(i1, newedge(dummy, false));
                    po.upush(undoSetChl, i1, i1l);
                }

            }

        }

        // newnode allocates a new node bound to SSA value n.
        // If n is nil, this is a dummy node (= only used internally).
        private static uint newnode(this ptr<poset> _addr_po, ptr<Value> _addr_n) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n = ref _addr_n.val;

            var i = po.lastidx + 1L;
            po.lastidx++;
            po.nodes = append(po.nodes, new posetNode());
            if (n != null)
            {
                if (po.values[n.ID] != 0L)
                {
                    panic("newnode for Value already inserted");
                }

                po.values[n.ID] = i;
                po.upushnew(n.ID, i);

            }
            else
            {
                po.upushnew(0L, i);
            }

            return i;

        });

        // lookup searches for a SSA value into the forest of DAGS, and return its node.
        // Constants are materialized on the fly during lookup.
        private static (uint, bool) lookup(this ptr<poset> _addr_po, ptr<Value> _addr_n)
        {
            uint _p0 = default;
            bool _p0 = default;
            ref poset po = ref _addr_po.val;
            ref Value n = ref _addr_n.val;

            var (i, f) = po.values[n.ID];
            if (!f && n.isGenericIntConst())
            {
                po.newconst(n);
                i, f = po.values[n.ID];
            }

            return (i, f);

        }

        // newconst creates a node for a constant. It links it to other constants, so
        // that n<=5 is detected true when n<=3 is known to be true.
        // TODO: this is O(N), fix it.
        private static void newconst(this ptr<poset> _addr_po, ptr<Value> _addr_n) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n = ref _addr_n.val;

            if (!n.isGenericIntConst())
            {
                panic("newconst on non-constant");
            } 

            // If the same constant is already present in the poset through a different
            // Value, just alias to it without allocating a new node.
            var val = n.AuxInt;
            if (po.flags & posetFlagUnsigned != 0L)
            {
                val = int64(n.AuxUnsigned());
            }

            {
                var (c, found) = po.constants[val];

                if (found)
                {
                    po.values[n.ID] = c;
                    po.upushalias(n.ID, 0L);
                    return ;
                } 

                // Create the new node for this constant

            } 

            // Create the new node for this constant
            var i = po.newnode(n); 

            // If this is the first constant, put it as a new root, as
            // we can't record an existing connection so we don't have
            // a specific DAG to add it to. Notice that we want all
            // constants to be in root #0, so make sure the new root
            // goes there.
            if (len(po.constants) == 0L)
            {
                var idx = len(po.roots);
                po.roots = append(po.roots, i);
                po.roots[0L] = po.roots[idx];
                po.roots[idx] = po.roots[0L];
                po.upush(undoNewRoot, i, 0L);
                po.constants[val] = i;
                po.upushconst(i, 0L);
                return ;

            } 

            // Find the lower and upper bound among existing constants. That is,
            // find the higher constant that is lower than the one that we're adding,
            // and the lower constant that is higher.
            // The loop is duplicated to handle signed and unsigned comparison,
            // depending on how the poset was configured.
            uint lowerptr = default;            uint higherptr = default;



            if (po.flags & posetFlagUnsigned != 0L)
            {
                ulong lower = default;                ulong higher = default;

                var val1 = n.AuxUnsigned();
                {
                    var val2__prev1 = val2;
                    var ptr__prev1 = ptr;

                    foreach (var (__val2, __ptr) in po.constants)
                    {
                        val2 = __val2;
                        ptr = __ptr;
                        var val2 = uint64(val2);
                        if (val1 == val2)
                        {
                            panic("unreachable");
                        }

                        if (val2 < val1 && (lowerptr == 0L || val2 > lower))
                        {
                            lower = val2;
                            lowerptr = ptr;
                        }
                        else if (val2 > val1 && (higherptr == 0L || val2 < higher))
                        {
                            higher = val2;
                            higherptr = ptr;
                        }

                    }
            else

                    val2 = val2__prev1;
                    ptr = ptr__prev1;
                }
            }            {
                lower = default;                higher = default;

                val1 = n.AuxInt;
                {
                    var val2__prev1 = val2;
                    var ptr__prev1 = ptr;

                    foreach (var (__val2, __ptr) in po.constants)
                    {
                        val2 = __val2;
                        ptr = __ptr;
                        if (val1 == val2)
                        {
                            panic("unreachable");
                        }

                        if (val2 < val1 && (lowerptr == 0L || val2 > lower))
                        {
                            lower = val2;
                            lowerptr = ptr;
                        }
                        else if (val2 > val1 && (higherptr == 0L || val2 < higher))
                        {
                            higher = val2;
                            higherptr = ptr;
                        }

                    }

                    val2 = val2__prev1;
                    ptr = ptr__prev1;
                }
            }

            if (lowerptr == 0L && higherptr == 0L)
            { 
                // This should not happen, as at least one
                // other constant must exist if we get here.
                panic("no constant found");

            } 

            // Create the new node and connect it to the bounds, so that
            // lower < n < higher. We could have found both bounds or only one
            // of them, depending on what other constants are present in the poset.
            // Notice that we always link constants together, so they
            // are always part of the same DAG.

            if (lowerptr != 0L && higherptr != 0L) 
                // Both bounds are present, record lower < n < higher.
                po.addchild(lowerptr, i, true);
                po.addchild(i, higherptr, true);
            else if (lowerptr != 0L) 
                // Lower bound only, record lower < n.
                po.addchild(lowerptr, i, true);
            else if (higherptr != 0L) 
                // Higher bound only. To record n < higher, we need
                // a dummy root:
                //
                //        dummy
                //        /   \
                //      root   \
                //       /      n
                //     ....    /
                //       \    /
                //       higher
                //
                var i2 = higherptr;
                var r2 = po.findroot(i2);
                if (r2 != po.roots[0L])
                { // all constants should be in root #0
                    panic("constant not in root #0");

                }

                var dummy = po.newnode(null);
                po.changeroot(r2, dummy);
                po.upush(undoChangeRoot, dummy, newedge(r2, false));
                po.addchild(dummy, r2, false);
                po.addchild(dummy, i, false);
                po.addchild(i, i2, true);
                        po.constants[val] = i;
            po.upushconst(i, 0L);

        });

        // aliasnewnode records that a single node n2 (not in the poset yet) is an alias
        // of the master node n1.
        private static void aliasnewnode(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            var i1 = po.values[n1.ID];
            var i2 = po.values[n2.ID];
            if (i1 == 0L || i2 != 0L)
            {
                panic("aliasnewnode invalid arguments");
            }

            po.values[n2.ID] = i1;
            po.upushalias(n2.ID, 0L);

        });

        // aliasnodes records that all the nodes i2s are aliases of a single master node n1.
        // aliasnodes takes care of rearranging the DAG, changing references of parent/children
        // of nodes in i2s, so that they point to n1 instead.
        // Complexity is O(n) (with n being the total number of nodes in the poset, not just
        // the number of nodes being aliased).
        private static void aliasnodes(this ptr<poset> _addr_po, ptr<Value> _addr_n1, bitset i2s) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;

            var i1 = po.values[n1.ID];
            if (i1 == 0L)
            {
                panic("aliasnode for non-existing node");
            }

            if (i2s.Test(i1))
            {
                panic("aliasnode i2s contains n1 node");
            } 

            // Go through all the nodes to adjust parent/chidlren of nodes in i2s
            {
                var idx__prev1 = idx;

                foreach (var (__idx, __n) in po.nodes)
                {
                    idx = __idx;
                    n = __n; 
                    // Do not touch i1 itself, otherwise we can create useless self-loops
                    if (uint32(idx) == i1)
                    {
                        continue;
                    }

                    var l = n.l;
                    var r = n.r; 

                    // Rename all references to i2s into i1
                    if (i2s.Test(l.Target()))
                    {
                        po.setchl(uint32(idx), newedge(i1, l.Strict()));
                        po.upush(undoSetChl, uint32(idx), l);
                    }

                    if (i2s.Test(r.Target()))
                    {
                        po.setchr(uint32(idx), newedge(i1, r.Strict()));
                        po.upush(undoSetChr, uint32(idx), r);
                    } 

                    // Connect all chidren of i2s to i1 (unless those children
                    // are in i2s as well, in which case it would be useless)
                    if (i2s.Test(uint32(idx)))
                    {
                        if (l != 0L && !i2s.Test(l.Target()))
                        {
                            po.addchild(i1, l.Target(), l.Strict());
                        }

                        if (r != 0L && !i2s.Test(r.Target()))
                        {
                            po.addchild(i1, r.Target(), r.Strict());
                        }

                        po.setchl(uint32(idx), 0L);
                        po.setchr(uint32(idx), 0L);
                        po.upush(undoSetChl, uint32(idx), l);
                        po.upush(undoSetChr, uint32(idx), r);

                    }

                } 

                // Reassign all existing IDs that point to i2 to i1.
                // This includes n2.ID.

                idx = idx__prev1;
            }

            foreach (var (k, v) in po.values)
            {
                if (i2s.Test(v))
                {
                    po.values[k] = i1;
                    po.upushalias(k, v);
                }

            } 

            // If one of the aliased nodes is a constant, then make sure
            // po.constants is updated to point to the master node.
            {
                var idx__prev1 = idx;

                foreach (var (__val, __idx) in po.constants)
                {
                    val = __val;
                    idx = __idx;
                    if (i2s.Test(idx))
                    {
                        po.constants[val] = i1;
                        po.upushconst(i1, idx);
                    }

                }

                idx = idx__prev1;
            }
        });

        private static bool isroot(this ptr<poset> _addr_po, uint r)
        {
            ref poset po = ref _addr_po.val;

            foreach (var (i) in po.roots)
            {
                if (po.roots[i] == r)
                {
                    return true;
                }

            }
            return false;

        }

        private static void changeroot(this ptr<poset> _addr_po, uint oldr, uint newr) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;

            foreach (var (i) in po.roots)
            {
                if (po.roots[i] == oldr)
                {
                    po.roots[i] = newr;
                    return ;
                }

            }
            panic("changeroot on non-root");

        });

        private static void removeroot(this ptr<poset> _addr_po, uint r) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;

            foreach (var (i) in po.roots)
            {
                if (po.roots[i] == r)
                {
                    po.roots = append(po.roots[..i], po.roots[i + 1L..]);
                    return ;
                }

            }
            panic("removeroot on non-root");

        });

        // dfs performs a depth-first search within the DAG whose root is r.
        // f is the visit function called for each node; if it returns true,
        // the search is aborted and true is returned. The root node is
        // visited too.
        // If strict, ignore edges across a path until at least one
        // strict edge is found. For instance, for a chain A<=B<=C<D<=E<F,
        // a strict walk visits D,E,F.
        // If the visit ends, false is returned.
        private static bool dfs(this ptr<poset> _addr_po, uint r, bool strict, Func<uint, bool> f)
        {
            ref poset po = ref _addr_po.val;

            var closed = newBitset(int(po.lastidx + 1L));
            var open = make_slice<uint>(1L, 64L);
            open[0L] = r;

            if (strict)
            { 
                // Do a first DFS; walk all paths and stop when we find a strict
                // edge, building a "next" list of nodes reachable through strict
                // edges. This will be the bootstrap open list for the real DFS.
                var next = make_slice<uint>(0L, 64L);

                while (len(open) > 0L)
                {
                    var i = open[len(open) - 1L];
                    open = open[..len(open) - 1L]; 

                    // Don't visit the same node twice. Notice that all nodes
                    // across non-strict paths are still visited at least once, so
                    // a non-strict path can never obscure a strict path to the
                    // same node.
                    if (!closed.Test(i))
                    {
                        closed.Set(i);

                        var (l, r) = po.children(i);
                        if (l != 0L)
                        {
                            if (l.Strict())
                            {
                                next = append(next, l.Target());
                            }
                            else
                            {
                                open = append(open, l.Target());
                            }

                        }

                        if (r != 0L)
                        {
                            if (r.Strict())
                            {
                                next = append(next, r.Target());
                            }
                            else
                            {
                                open = append(open, r.Target());
                            }

                        }

                    }

                }

                open = next;
                closed.Reset();

            }

            while (len(open) > 0L)
            {
                i = open[len(open) - 1L];
                open = open[..len(open) - 1L];

                if (!closed.Test(i))
                {
                    if (f(i))
                    {
                        return true;
                    }

                    closed.Set(i);
                    (l, r) = po.children(i);
                    if (l != 0L)
                    {
                        open = append(open, l.Target());
                    }

                    if (r != 0L)
                    {
                        open = append(open, r.Target());
                    }

                }

            }

            return false;

        }

        // Returns true if there is a path from i1 to i2.
        // If strict ==  true: if the function returns true, then i1 <  i2.
        // If strict == false: if the function returns true, then i1 <= i2.
        // If the function returns false, no relation is known.
        private static bool reaches(this ptr<poset> _addr_po, uint i1, uint i2, bool strict)
        {
            ref poset po = ref _addr_po.val;

            return po.dfs(i1, strict, n =>
            {
                return n == i2;
            });

        }

        // findroot finds i's root, that is which DAG contains i.
        // Returns the root; if i is itself a root, it is returned.
        // Panic if i is not in any DAG.
        private static uint findroot(this ptr<poset> _addr_po, uint i) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
 
            // TODO(rasky): if needed, a way to speed up this search is
            // storing a bitset for each root using it as a mini bloom filter
            // of nodes present under that root.
            foreach (var (_, r) in po.roots)
            {
                if (po.reaches(r, i, false))
                {
                    return r;
                }

            }
            panic("findroot didn't find any root");

        });

        // mergeroot merges two DAGs into one DAG by creating a new dummy root
        private static uint mergeroot(this ptr<poset> _addr_po, uint r1, uint r2)
        {
            ref poset po = ref _addr_po.val;
 
            // Root #0 is special as it contains all constants. Since mergeroot
            // discards r2 as root and keeps r1, make sure that r2 is not root #0,
            // otherwise constants would move to a different root.
            if (r2 == po.roots[0L])
            {
                r1 = r2;
                r2 = r1;

            }

            var r = po.newnode(null);
            po.setchl(r, newedge(r1, false));
            po.setchr(r, newedge(r2, false));
            po.changeroot(r1, r);
            po.removeroot(r2);
            po.upush(undoMergeRoot, r, 0L);
            return r;

        }

        // collapsepath marks n1 and n2 as equal and collapses as equal all
        // nodes across all paths between n1 and n2. If a strict edge is
        // found, the function does not modify the DAG and returns false.
        // Complexity is O(n).
        private static bool collapsepath(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2)
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            var i1 = po.values[n1.ID];
            var i2 = po.values[n2.ID];
            if (po.reaches(i1, i2, true))
            {
                return false;
            } 

            // Find all the paths from i1 to i2
            var paths = po.findpaths(i1, i2); 
            // Mark all nodes in all the paths as aliases of n1
            // (excluding n1 itself)
            paths.Clear(i1);
            po.aliasnodes(n1, paths);
            return true;

        }

        // findpaths is a recursive function that calculates all paths from cur to dst
        // and return them as a bitset (the index of a node is set in the bitset if
        // that node is on at least one path from cur to dst).
        // We do a DFS from cur (stopping going deep any time we reach dst, if ever),
        // and mark as part of the paths any node that has a children which is already
        // part of the path (or is dst itself).
        private static bitset findpaths(this ptr<poset> _addr_po, uint cur, uint dst)
        {
            ref poset po = ref _addr_po.val;

            var seen = newBitset(int(po.lastidx + 1L));
            var path = newBitset(int(po.lastidx + 1L));
            path.Set(dst);
            po.findpaths1(cur, dst, seen, path);
            return path;
        }

        private static void findpaths1(this ptr<poset> _addr_po, uint cur, uint dst, bitset seen, bitset path)
        {
            ref poset po = ref _addr_po.val;

            if (cur == dst)
            {
                return ;
            }

            seen.Set(cur);
            var l = po.chl(cur);
            var r = po.chr(cur);
            if (!seen.Test(l))
            {
                po.findpaths1(l, dst, seen, path);
            }

            if (!seen.Test(r))
            {
                po.findpaths1(r, dst, seen, path);
            }

            if (path.Test(l) || path.Test(r))
            {
                path.Set(cur);
            }

        }

        // Check whether it is recorded that i1!=i2
        private static bool isnoneq(this ptr<poset> _addr_po, uint i1, uint i2)
        {
            ref poset po = ref _addr_po.val;

            if (i1 == i2)
            {
                return false;
            }

            if (i1 < i2)
            {
                i1 = i2;
                i2 = i1;

            } 

            // Check if we recorded a non-equal relation before
            {
                var (bs, ok) = po.noneq[i1];

                if (ok && bs.Test(i2))
                {
                    return true;
                }

            }

            return false;

        }

        // Record that i1!=i2
        private static void setnoneq(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2); 

            // If any of the nodes do not exist in the poset, allocate them. Since
            // we don't know any relation (in the partial order) about them, they must
            // become independent roots.
            if (!f1)
            {
                i1 = po.newnode(n1);
                po.roots = append(po.roots, i1);
                po.upush(undoNewRoot, i1, 0L);
            }

            if (!f2)
            {
                i2 = po.newnode(n2);
                po.roots = append(po.roots, i2);
                po.upush(undoNewRoot, i2, 0L);
            }

            if (i1 == i2)
            {
                panic("setnoneq on same node");
            }

            if (i1 < i2)
            {
                i1 = i2;
                i2 = i1;

            }

            var bs = po.noneq[i1];
            if (bs == null)
            { 
                // Given that we record non-equality relations using the
                // higher index as a key, the bitsize will never change size.
                // TODO(rasky): if memory is a problem, consider allocating
                // a small bitset and lazily grow it when higher indices arrive.
                bs = newBitset(int(i1));
                po.noneq[i1] = bs;

            }
            else if (bs.Test(i2))
            { 
                // Already recorded
                return ;

            }

            bs.Set(i2);
            po.upushneq(i1, i2);

        });

        // CheckIntegrity verifies internal integrity of a poset. It is intended
        // for debugging purposes.
        private static void CheckIntegrity(this ptr<poset> _addr_po) => func((_, panic, __) =>
        {
            ref poset po = ref _addr_po.val;
 
            // Record which index is a constant
            var constants = newBitset(int(po.lastidx + 1L));
            foreach (var (_, c) in po.constants)
            {
                constants.Set(c);
            } 

            // Verify that each node appears in a single DAG, and that
            // all constants are within the first DAG
            var seen = newBitset(int(po.lastidx + 1L));
            foreach (var (ridx, r) in po.roots)
            {
                if (r == 0L)
                {
                    panic("empty root");
                }

                po.dfs(r, false, i =>
                {
                    if (seen.Test(i))
                    {
                        panic("duplicate node");
                    }

                    seen.Set(i);
                    if (constants.Test(i))
                    {
                        if (ridx != 0L)
                        {
                            panic("constants not in the first DAG");
                        }

                    }

                    return false;

                });

            } 

            // Verify that values contain the minimum set
            foreach (var (id, idx) in po.values)
            {
                if (!seen.Test(idx))
                {
                    panic(fmt.Errorf("spurious value [%d]=%d", id, idx));
                }

            } 

            // Verify that only existing nodes have non-zero children
            foreach (var (i, n) in po.nodes)
            {
                if (n.l | n.r != 0L)
                {
                    if (!seen.Test(uint32(i)))
                    {
                        panic(fmt.Errorf("children of unknown node %d->%v", i, n));
                    }

                    if (n.l.Target() == uint32(i) || n.r.Target() == uint32(i))
                    {
                        panic(fmt.Errorf("self-loop on node %d", i));
                    }

                }

            }

        });

        // CheckEmpty checks that a poset is completely empty.
        // It can be used for debugging purposes, as a poset is supposed to
        // be empty after it's fully rolled back through Undo.
        private static error CheckEmpty(this ptr<poset> _addr_po)
        {
            ref poset po = ref _addr_po.val;

            if (len(po.nodes) != 1L)
            {
                return error.As(fmt.Errorf("non-empty nodes list: %v", po.nodes))!;
            }

            if (len(po.values) != 0L)
            {
                return error.As(fmt.Errorf("non-empty value map: %v", po.values))!;
            }

            if (len(po.roots) != 0L)
            {
                return error.As(fmt.Errorf("non-empty root list: %v", po.roots))!;
            }

            if (len(po.constants) != 0L)
            {
                return error.As(fmt.Errorf("non-empty constants: %v", po.constants))!;
            }

            if (len(po.undo) != 0L)
            {
                return error.As(fmt.Errorf("non-empty undo list: %v", po.undo))!;
            }

            if (po.lastidx != 0L)
            {
                return error.As(fmt.Errorf("lastidx index is not zero: %v", po.lastidx))!;
            }

            foreach (var (_, bs) in po.noneq)
            {
                foreach (var (_, x) in bs)
                {
                    if (x != 0L)
                    {
                        return error.As(fmt.Errorf("non-empty noneq map"))!;
                    }

                }

            }
            return error.As(null!)!;

        }

        // DotDump dumps the poset in graphviz format to file fn, with the specified title.
        private static error DotDump(this ptr<poset> _addr_po, @string fn, @string title) => func((defer, _, __) =>
        {
            ref poset po = ref _addr_po.val;

            var (f, err) = os.Create(fn);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(f.Close()); 

            // Create reverse index mapping (taking aliases into account)
            var names = make_map<uint, @string>();
            foreach (var (id, i) in po.values)
            {
                var s = names[i];
                if (s == "")
                {
                    s = fmt.Sprintf("v%d", id);
                }
                else
                {
                    s += fmt.Sprintf(", v%d", id);
                }

                names[i] = s;

            } 

            // Create reverse constant mapping
            var consts = make_map<uint, long>();
            {
                var val__prev1 = val;

                foreach (var (__val, __idx) in po.constants)
                {
                    val = __val;
                    idx = __idx;
                    consts[idx] = val;
                }

                val = val__prev1;
            }

            fmt.Fprintf(f, "digraph poset {\n");
            fmt.Fprintf(f, "\tedge [ fontsize=10 ]\n");
            foreach (var (ridx, r) in po.roots)
            {
                fmt.Fprintf(f, "\tsubgraph root%d {\n", ridx);
                po.dfs(r, false, i =>
                {
                    {
                        var val__prev1 = val;

                        var (val, ok) = consts[i];

                        if (ok)
                        { 
                            // Constant
                            @string vals = default;
                            if (po.flags & posetFlagUnsigned != 0L)
                            {
                                vals = fmt.Sprint(uint64(val));
                            }
                            else
                            {
                                vals = fmt.Sprint(int64(val));
                            }

                            fmt.Fprintf(f, "\t\tnode%d [shape=box style=filled fillcolor=cadetblue1 label=<%s <font point-size=\"6\">%s [%d]</font>>]\n", i, vals, names[i], i);

                        }
                        else
                        { 
                            // Normal SSA value
                            fmt.Fprintf(f, "\t\tnode%d [label=<%s <font point-size=\"6\">[%d]</font>>]\n", i, names[i], i);

                        }

                        val = val__prev1;

                    }

                    var (chl, chr) = po.children(i);
                    foreach (var (_, ch) in new slice<posetEdge>(new posetEdge[] { chl, chr }))
                    {
                        if (ch != 0L)
                        {
                            if (ch.Strict())
                            {
                                fmt.Fprintf(f, "\t\tnode%d -> node%d [label=\" <\" color=\"red\"]\n", i, ch.Target());
                            }
                            else
                            {
                                fmt.Fprintf(f, "\t\tnode%d -> node%d [label=\" <=\" color=\"green\"]\n", i, ch.Target());
                            }

                        }

                    }
                    return error.As(false)!;

                });
                fmt.Fprintf(f, "\t}\n");

            }
            fmt.Fprintf(f, "\tlabelloc=\"t\"\n");
            fmt.Fprintf(f, "\tlabeldistance=\"3.0\"\n");
            fmt.Fprintf(f, "\tlabel=%q\n", title);
            fmt.Fprintf(f, "}\n");
            return error.As(null!)!;

        });

        // Ordered reports whether n1<n2. It returns false either when it is
        // certain that n1<n2 is false, or if there is not enough information
        // to tell.
        // Complexity is O(n).
        private static bool Ordered(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call Ordered with n1==n2");
            }

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);
            if (!f1 || !f2)
            {
                return false;
            }

            return i1 != i2 && po.reaches(i1, i2, true);

        });

        // Ordered reports whether n1<=n2. It returns false either when it is
        // certain that n1<=n2 is false, or if there is not enough information
        // to tell.
        // Complexity is O(n).
        private static bool OrderedOrEqual(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call Ordered with n1==n2");
            }

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);
            if (!f1 || !f2)
            {
                return false;
            }

            return i1 == i2 || po.reaches(i1, i2, false);

        });

        // Equal reports whether n1==n2. It returns false either when it is
        // certain that n1==n2 is false, or if there is not enough information
        // to tell.
        // Complexity is O(1).
        private static bool Equal(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call Equal with n1==n2");
            }

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);
            return f1 && f2 && i1 == i2;

        });

        // NonEqual reports whether n1!=n2. It returns false either when it is
        // certain that n1!=n2 is false, or if there is not enough information
        // to tell.
        // Complexity is O(n) (because it internally calls Ordered to see if we
        // can infer n1!=n2 from n1<n2 or n2<n1).
        private static bool NonEqual(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call NonEqual with n1==n2");
            } 

            // If we never saw the nodes before, we don't
            // have a recorded non-equality.
            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);
            if (!f1 || !f2)
            {
                return false;
            } 

            // Check if we recored inequality
            if (po.isnoneq(i1, i2))
            {
                return true;
            } 

            // Check if n1<n2 or n2<n1, in which case we can infer that n1!=n2
            if (po.Ordered(n1, n2) || po.Ordered(n2, n1))
            {
                return true;
            }

            return false;

        });

        // setOrder records that n1<n2 or n1<=n2 (depending on strict). Returns false
        // if this is a contradiction.
        // Implements SetOrder() and SetOrderOrEqual()
        private static bool setOrder(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2, bool strict)
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);


            if (!f1 && !f2) 
                // Neither n1 nor n2 are in the poset, so they are not related
                // in any way to existing nodes.
                // Create a new DAG to record the relation.
                i1 = po.newnode(n1);
                i2 = po.newnode(n2);
                po.roots = append(po.roots, i1);
                po.upush(undoNewRoot, i1, 0L);
                po.addchild(i1, i2, strict);
            else if (f1 && !f2) 
                // n1 is in one of the DAGs, while n2 is not. Add n2 as children
                // of n1.
                i2 = po.newnode(n2);
                po.addchild(i1, i2, strict);
            else if (!f1 && f2) 
                // n1 is not in any DAG but n2 is. If n2 is a root, we can put
                // n1 in its place as a root; otherwise, we need to create a new
                // dummy root to record the relation.
                i1 = po.newnode(n1);

                if (po.isroot(i2))
                {
                    po.changeroot(i2, i1);
                    po.upush(undoChangeRoot, i1, newedge(i2, strict));
                    po.addchild(i1, i2, strict);
                    return true;
                } 

                // Search for i2's root; this requires a O(n) search on all
                // DAGs
                var r = po.findroot(i2); 

                // Re-parent as follows:
                //
                //                  dummy
                //     r            /   \
                //      \   ===>   r    i1
                //      i2          \   /
                //                    i2
                //
                var dummy = po.newnode(null);
                po.changeroot(r, dummy);
                po.upush(undoChangeRoot, dummy, newedge(r, false));
                po.addchild(dummy, r, false);
                po.addchild(dummy, i1, false);
                po.addchild(i1, i2, strict);
            else if (f1 && f2) 
                // If the nodes are aliased, fail only if we're setting a strict order
                // (that is, we cannot set n1<n2 if n1==n2).
                if (i1 == i2)
                {
                    return !strict;
                } 

                // If we are trying to record n1<=n2 but we learned that n1!=n2,
                // record n1<n2, as it provides more information.
                if (!strict && po.isnoneq(i1, i2))
                {
                    strict = true;
                } 

                // Both n1 and n2 are in the poset. This is the complex part of the algorithm
                // as we need to find many different cases and DAG shapes.

                // Check if n1 somehow reaches n2
                if (po.reaches(i1, i2, false))
                { 
                    // This is the table of all cases we need to handle:
                    //
                    //      DAG          New      Action
                    //      ---------------------------------------------------
                    // #1:  N1<=X<=N2 |  N1<=N2 | do nothing
                    // #2:  N1<=X<=N2 |  N1<N2  | add strict edge (N1<N2)
                    // #3:  N1<X<N2   |  N1<=N2 | do nothing (we already know more)
                    // #4:  N1<X<N2   |  N1<N2  | do nothing

                    // Check if we're in case #2
                    if (strict && !po.reaches(i1, i2, true))
                    {
                        po.addchild(i1, i2, true);
                        return true;
                    } 

                    // Case #1, #3 o #4: nothing to do
                    return true;

                } 

                // Check if n2 somehow reaches n1
                if (po.reaches(i2, i1, false))
                { 
                    // This is the table of all cases we need to handle:
                    //
                    //      DAG           New      Action
                    //      ---------------------------------------------------
                    // #5:  N2<=X<=N1  |  N1<=N2 | collapse path (learn that N1=X=N2)
                    // #6:  N2<=X<=N1  |  N1<N2  | contradiction
                    // #7:  N2<X<N1    |  N1<=N2 | contradiction in the path
                    // #8:  N2<X<N1    |  N1<N2  | contradiction

                    if (strict)
                    { 
                        // Cases #6 and #8: contradiction
                        return false;

                    } 

                    // We're in case #5 or #7. Try to collapse path, and that will
                    // fail if it realizes that we are in case #7.
                    return po.collapsepath(n2, n1);

                } 

                // We don't know of any existing relation between n1 and n2. They could
                // be part of the same DAG or not.
                // Find their roots to check whether they are in the same DAG.
                var r1 = po.findroot(i1);
                var r2 = po.findroot(i2);
                if (r1 != r2)
                { 
                    // We need to merge the two DAGs to record a relation between the nodes
                    po.mergeroot(r1, r2);

                } 

                // Connect n1 and n2
                po.addchild(i1, i2, strict);
                        return true;

        }

        // SetOrder records that n1<n2. Returns false if this is a contradiction
        // Complexity is O(1) if n2 was never seen before, or O(n) otherwise.
        private static bool SetOrder(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call SetOrder with n1==n2");
            }

            return po.setOrder(n1, n2, true);

        });

        // SetOrderOrEqual records that n1<=n2. Returns false if this is a contradiction
        // Complexity is O(1) if n2 was never seen before, or O(n) otherwise.
        private static bool SetOrderOrEqual(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call SetOrder with n1==n2");
            }

            return po.setOrder(n1, n2, false);

        });

        // SetEqual records that n1==n2. Returns false if this is a contradiction
        // (that is, if it is already recorded that n1<n2 or n2<n1).
        // Complexity is O(1) if n2 was never seen before, or O(n) otherwise.
        private static bool SetEqual(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call Add with n1==n2");
            }

            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2);


            if (!f1 && !f2) 
                i1 = po.newnode(n1);
                po.roots = append(po.roots, i1);
                po.upush(undoNewRoot, i1, 0L);
                po.aliasnewnode(n1, n2);
            else if (f1 && !f2) 
                po.aliasnewnode(n1, n2);
            else if (!f1 && f2) 
                po.aliasnewnode(n2, n1);
            else if (f1 && f2) 
                if (i1 == i2)
                { 
                    // Already aliased, ignore
                    return true;

                } 

                // If we recorded that n1!=n2, this is a contradiction.
                if (po.isnoneq(i1, i2))
                {
                    return false;
                } 

                // If we already knew that n1<=n2, we can collapse the path to
                // record n1==n2 (and viceversa).
                if (po.reaches(i1, i2, false))
                {
                    return po.collapsepath(n1, n2);
                }

                if (po.reaches(i2, i1, false))
                {
                    return po.collapsepath(n2, n1);
                }

                var r1 = po.findroot(i1);
                var r2 = po.findroot(i2);
                if (r1 != r2)
                { 
                    // Merge the two DAGs so we can record relations between the nodes
                    po.mergeroot(r1, r2);

                } 

                // Set n2 as alias of n1. This will also update all the references
                // to n2 to become references to n1
                var i2s = newBitset(int(po.lastidx) + 1L);
                i2s.Set(i2);
                po.aliasnodes(n1, i2s);
                        return true;

        });

        // SetNonEqual records that n1!=n2. Returns false if this is a contradiction
        // (that is, if it is already recorded that n1==n2).
        // Complexity is O(n).
        private static bool SetNonEqual(this ptr<poset> _addr_po, ptr<Value> _addr_n1, ptr<Value> _addr_n2) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;
            ref Value n1 = ref _addr_n1.val;
            ref Value n2 = ref _addr_n2.val;

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            if (n1.ID == n2.ID)
            {
                panic("should not call SetNonEqual with n1==n2");
            } 

            // Check whether the nodes are already in the poset
            var (i1, f1) = po.lookup(n1);
            var (i2, f2) = po.lookup(n2); 

            // If either node wasn't present, we just record the new relation
            // and exit.
            if (!f1 || !f2)
            {
                po.setnoneq(n1, n2);
                return true;
            } 

            // See if we already know this, in which case there's nothing to do.
            if (po.isnoneq(i1, i2))
            {
                return true;
            } 

            // Check if we're contradicting an existing equality relation
            if (po.Equal(n1, n2))
            {
                return false;
            } 

            // Record non-equality
            po.setnoneq(n1, n2); 

            // If we know that i1<=i2 but not i1<i2, learn that as we
            // now know that they are not equal. Do the same for i2<=i1.
            // Do this check only if both nodes were already in the DAG,
            // otherwise there cannot be an existing relation.
            if (po.reaches(i1, i2, false) && !po.reaches(i1, i2, true))
            {
                po.addchild(i1, i2, true);
            }

            if (po.reaches(i2, i1, false) && !po.reaches(i2, i1, true))
            {
                po.addchild(i2, i1, true);
            }

            return true;

        });

        // Checkpoint saves the current state of the DAG so that it's possible
        // to later undo this state.
        // Complexity is O(1).
        private static void Checkpoint(this ptr<poset> _addr_po)
        {
            ref poset po = ref _addr_po.val;

            po.undo = append(po.undo, new posetUndo(typ:undoCheckpoint));
        }

        // Undo restores the state of the poset to the previous checkpoint.
        // Complexity depends on the type of operations that were performed
        // since the last checkpoint; each Set* operation creates an undo
        // pass which Undo has to revert with a worst-case complexity of O(n).
        private static void Undo(this ptr<poset> _addr_po) => func((defer, panic, _) =>
        {
            ref poset po = ref _addr_po.val;

            if (len(po.undo) == 0L)
            {
                panic("empty undo stack");
            }

            if (debugPoset)
            {
                defer(po.CheckIntegrity());
            }

            while (len(po.undo) > 0L)
            {
                var pass = po.undo[len(po.undo) - 1L];
                po.undo = po.undo[..len(po.undo) - 1L];


                if (pass.typ == undoCheckpoint) 
                    return ;
                else if (pass.typ == undoSetChl) 
                    po.setchl(pass.idx, pass.edge);
                else if (pass.typ == undoSetChr) 
                    po.setchr(pass.idx, pass.edge);
                else if (pass.typ == undoNonEqual) 
                    po.noneq[uint32(pass.ID)].Clear(pass.idx);
                else if (pass.typ == undoNewNode) 
                    if (pass.idx != po.lastidx)
                    {
                        panic("invalid newnode index");
                    }

                    if (pass.ID != 0L)
                    {
                        if (po.values[pass.ID] != pass.idx)
                        {
                            panic("invalid newnode undo pass");
                        }

                        delete(po.values, pass.ID);

                    }

                    po.setchl(pass.idx, 0L);
                    po.setchr(pass.idx, 0L);
                    po.nodes = po.nodes[..pass.idx];
                    po.lastidx--;
                else if (pass.typ == undoNewConstant) 
                    // FIXME: remove this O(n) loop
                    long val = default;
                    uint i = default;
                    foreach (var (__val, __i) in po.constants)
                    {
                        val = __val;
                        i = __i;
                        if (i == pass.idx)
                        {
                            break;
                        }

                    }

                    if (i != pass.idx)
                    {
                        panic("constant not found in undo pass");
                    }

                    if (pass.ID == 0L)
                    {
                        delete(po.constants, val);
                    }
                    else
                    { 
                        // Restore previous index as constant node
                        // (also restoring the invariant on correct bounds)
                        var oldidx = uint32(pass.ID);
                        po.constants[val] = oldidx;

                    }

                else if (pass.typ == undoAliasNode) 
                    var ID = pass.ID;
                    var prev = pass.idx;
                    var cur = po.values[ID];
                    if (prev == 0L)
                    { 
                        // Born as an alias, die as an alias
                        delete(po.values, ID);

                    }
                    else
                    {
                        if (cur == prev)
                        {
                            panic("invalid aliasnode undo pass");
                        } 
                        // Give it back previous value
                        po.values[ID] = prev;

                    }

                else if (pass.typ == undoNewRoot) 
                    i = pass.idx;
                    var (l, r) = po.children(i);
                    if (l | r != 0L)
                    {
                        panic("non-empty root in undo newroot");
                    }

                    po.removeroot(i);
                else if (pass.typ == undoChangeRoot) 
                    i = pass.idx;
                    (l, r) = po.children(i);
                    if (l | r != 0L)
                    {
                        panic("non-empty root in undo changeroot");
                    }

                    po.changeroot(i, pass.edge.Target());
                else if (pass.typ == undoMergeRoot) 
                    i = pass.idx;
                    (l, r) = po.children(i);
                    po.changeroot(i, l.Target());
                    po.roots = append(po.roots, r.Target());
                else 
                    panic(pass.typ);
                
            }


            if (debugPoset && po.CheckEmpty() != null)
            {
                panic("poset not empty at the end of undo");
            }

        });
    }
}}}}
