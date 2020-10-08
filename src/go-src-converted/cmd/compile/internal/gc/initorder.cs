// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:29:14 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\initorder.go
using bytes = go.bytes_package;
using heap = go.container.heap_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // Package initialization
        //
        // Here we implement the algorithm for ordering package-level variable
        // initialization. The spec is written in terms of variable
        // initialization, but multiple variables initialized by a single
        // assignment are handled together, so here we instead focus on
        // ordering initialization assignments. Conveniently, this maps well
        // to how we represent package-level initializations using the Node
        // AST.
        //
        // Assignments are in one of three phases: NotStarted, Pending, or
        // Done. For assignments in the Pending phase, we use Xoffset to
        // record the number of unique variable dependencies whose
        // initialization assignment is not yet Done. We also maintain a
        // "blocking" map that maps assignments back to all of the assignments
        // that depend on it.
        //
        // For example, for an initialization like:
        //
        //     var x = f(a, b, b)
        //     var a, b = g()
        //
        // the "x = f(a, b, b)" assignment depends on two variables (a and b),
        // so its Xoffset will be 2. Correspondingly, the "a, b = g()"
        // assignment's "blocking" entry will have two entries back to x's
        // assignment.
        //
        // Logically, initialization works by (1) taking all NotStarted
        // assignments, calculating their dependencies, and marking them
        // Pending; (2) adding all Pending assignments with Xoffset==0 to a
        // "ready" priority queue (ordered by variable declaration position);
        // and (3) iteratively processing the next Pending assignment from the
        // queue, decreasing the Xoffset of assignments it's blocking, and
        // adding them to the queue if decremented to 0.
        //
        // As an optimization, we actually apply each of these three steps for
        // each assignment. This yields the same order, but keeps queue size
        // down and thus also heap operation costs.

        // Static initialization phase.
        // These values are stored in two bits in Node.flags.
        public static readonly var InitNotStarted = (var)iota;
        public static readonly var InitDone = (var)0;
        public static readonly var InitPending = (var)1;


        public partial struct InitOrder
        {
            public map<ptr<Node>, slice<ptr<Node>>> blocking; // ready is the queue of Pending initialization assignments
// that are ready for initialization.
            public declOrder ready;
        }

        // initOrder computes initialization order for a list l of
        // package-level declarations (in declaration order) and outputs the
        // corresponding list of statements to include in the init() function
        // body.
        private static slice<ptr<Node>> initOrder(slice<ptr<Node>> l)
        {
            InitSchedule s = new InitSchedule(initplans:make(map[*Node]*InitPlan),inittemps:make(map[*Node]*Node),);
            InitOrder o = new InitOrder(blocking:make(map[*Node][]*Node),); 

            // Process all package-level assignment in declaration order.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in l)
                {
                    n = __n;

                    if (n.Op == OAS || n.Op == OAS2DOTTYPE || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2RECV) 
                        o.processAssign(n);
                        o.flushReady(s.staticInit);
                    else if (n.Op == ODCLCONST || n.Op == ODCLFUNC || n.Op == ODCLTYPE)                     else 
                        Fatalf("unexpected package-level statement: %v", n);
                    
                } 

                // Check that all assignments are now Done; if not, there must
                // have been a dependency cycle.

                n = n__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in l)
                {
                    n = __n;

                    if (n.Op == OAS || n.Op == OAS2DOTTYPE || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2RECV) 
                        if (n.Initorder() != InitDone)
                        { 
                            // If there have already been errors
                            // printed, those errors may have
                            // confused us and there might not be
                            // a loop. Let the user fix those
                            // first.
                            if (nerrors > 0L)
                            {
                                errorexit();
                            }

                            findInitLoopAndExit(_addr_firstLHS(_addr_n), @new<*Node>());
                            Fatalf("initialization unfinished, but failed to identify loop");

                        }

                                    } 

                // Invariant consistency check. If this is non-zero, then we
                // should have found a cycle above.

                n = n__prev1;
            }

            if (len(o.blocking) != 0L)
            {
                Fatalf("expected empty map: %v", o.blocking);
            }

            return s.@out;

        }

        private static void processAssign(this ptr<InitOrder> _addr_o, ptr<Node> _addr_n)
        {
            ref InitOrder o = ref _addr_o.val;
            ref Node n = ref _addr_n.val;

            if (n.Initorder() != InitNotStarted || n.Xoffset != BADWIDTH)
            {
                Fatalf("unexpected state: %v, %v, %v", n, n.Initorder(), n.Xoffset);
            }

            n.SetInitorder(InitPending);
            n.Xoffset = 0L; 

            // Compute number of variable dependencies and build the
            // inverse dependency ("blocking") graph.
            foreach (var (dep) in collectDeps(_addr_n, true))
            {
                var defn = dep.Name.Defn; 
                // Skip dependencies on functions (PFUNC) and
                // variables already initialized (InitDone).
                if (dep.Class() != PEXTERN || defn.Initorder() == InitDone)
                {
                    continue;
                }

                n.Xoffset++;
                o.blocking[defn] = append(o.blocking[defn], n);

            }
            if (n.Xoffset == 0L)
            {
                heap.Push(_addr_o.ready, n);
            }

        }

        // flushReady repeatedly applies initialize to the earliest (in
        // declaration order) assignment ready for initialization and updates
        // the inverse dependency ("blocking") graph.
        private static void flushReady(this ptr<InitOrder> _addr_o, Action<ptr<Node>> initialize)
        {
            ref InitOrder o = ref _addr_o.val;

            while (o.ready.Len() != 0L)
            {
                ptr<Node> n = heap.Pop(_addr_o.ready)._<ptr<Node>>();
                if (n.Initorder() != InitPending || n.Xoffset != 0L)
                {
                    Fatalf("unexpected state: %v, %v, %v", n, n.Initorder(), n.Xoffset);
                }

                initialize(n);
                n.SetInitorder(InitDone);
                n.Xoffset = BADWIDTH;

                var blocked = o.blocking[n];
                delete(o.blocking, n);

                foreach (var (_, m) in blocked)
                {
                    m.Xoffset--;
                    if (m.Xoffset == 0L)
                    {
                        heap.Push(_addr_o.ready, m);
                    }

                }

            }


        }

        // findInitLoopAndExit searches for an initialization loop involving variable
        // or function n. If one is found, it reports the loop as an error and exits.
        //
        // path points to a slice used for tracking the sequence of
        // variables/functions visited. Using a pointer to a slice allows the
        // slice capacity to grow and limit reallocations.
        private static void findInitLoopAndExit(ptr<Node> _addr_n, ptr<slice<ptr<Node>>> _addr_path)
        {
            ref Node n = ref _addr_n.val;
            ref slice<ptr<Node>> path = ref _addr_path.val;
 
            // We implement a simple DFS loop-finding algorithm. This
            // could be faster, but initialization cycles are rare.

            foreach (var (i, x) in path.val)
            {
                if (x == n)
                {
                    reportInitLoopAndExit((path.val)[i..]);
                    return ;
                }

            } 

            // There might be multiple loops involving n; by sorting
            // references, we deterministically pick the one reported.
            var refers = collectDeps(_addr_n.Name.Defn, false).Sorted((ni, nj) =>
            {
                return ni.Pos.Before(nj.Pos);
            });

            path.val = append(path.val, n);
            foreach (var (_, ref) in refers)
            { 
                // Short-circuit variables that were initialized.
                if (@ref.Class() == PEXTERN && @ref.Name.Defn.Initorder() == InitDone)
                {
                    continue;
                }

                findInitLoopAndExit(_addr_ref, _addr_path);

            }
            path.val = (path.val)[..len(path.val) - 1L];

        }

        // reportInitLoopAndExit reports and initialization loop as an error
        // and exits. However, if l is not actually an initialization loop, it
        // simply returns instead.
        private static void reportInitLoopAndExit(slice<ptr<Node>> l)
        { 
            // Rotate loop so that the earliest variable declaration is at
            // the start.
            long i = -1L;
            {
                var n__prev1 = n;

                foreach (var (__j, __n) in l)
                {
                    j = __j;
                    n = __n;
                    if (n.Class() == PEXTERN && (i == -1L || n.Pos.Before(l[i].Pos)))
                    {
                        i = j;
                    }

                }

                n = n__prev1;
            }

            if (i == -1L)
            { 
                // False positive: loop only involves recursive
                // functions. Return so that findInitLoop can continue
                // searching.
                return ;

            }

            l = append(l[i..], l[..i]); 

            // TODO(mdempsky): Method values are printed as "T.m-fm"
            // rather than "T.m". Figure out how to avoid that.

            ref bytes.Buffer msg = ref heap(out ptr<bytes.Buffer> _addr_msg);
            fmt.Fprintf(_addr_msg, "initialization loop:\n");
            {
                var n__prev1 = n;

                foreach (var (_, __n) in l)
                {
                    n = __n;
                    fmt.Fprintf(_addr_msg, "\t%v: %v refers to\n", n.Line(), n);
                }

                n = n__prev1;
            }

            fmt.Fprintf(_addr_msg, "\t%v: %v", l[0L].Line(), l[0L]);

            yyerrorl(l[0L].Pos, msg.String());
            errorexit();

        }

        // collectDeps returns all of the package-level functions and
        // variables that declaration n depends on. If transitive is true,
        // then it also includes the transitive dependencies of any depended
        // upon functions (but not variables).
        private static NodeSet collectDeps(ptr<Node> _addr_n, bool transitive)
        {
            ref Node n = ref _addr_n.val;

            initDeps d = new initDeps(transitive:transitive);

            if (n.Op == OAS) 
                d.inspect(n.Right);
            else if (n.Op == OAS2DOTTYPE || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2RECV) 
                d.inspect(n.Right);
            else if (n.Op == ODCLFUNC) 
                d.inspectList(n.Nbody);
            else 
                Fatalf("unexpected Op: %v", n.Op);
                        return d.seen;

        }

        private partial struct initDeps
        {
            public bool transitive;
            public NodeSet seen;
        }

        private static void inspect(this ptr<initDeps> _addr_d, ptr<Node> _addr_n)
        {
            ref initDeps d = ref _addr_d.val;
            ref Node n = ref _addr_n.val;

            inspect(n, d.visit);
        }
        private static void inspectList(this ptr<initDeps> _addr_d, Nodes l)
        {
            ref initDeps d = ref _addr_d.val;

            inspectList(l, d.visit);
        }

        // visit calls foundDep on any package-level functions or variables
        // referenced by n, if any.
        private static bool visit(this ptr<initDeps> _addr_d, ptr<Node> _addr_n)
        {
            ref initDeps d = ref _addr_d.val;
            ref Node n = ref _addr_n.val;


            if (n.Op == ONAME) 
                if (n.isMethodExpression())
                {
                    d.foundDep(asNode(n.Type.FuncType().Nname));
                    return false;
                }


                if (n.Class() == PEXTERN || n.Class() == PFUNC) 
                    d.foundDep(n);
                            else if (n.Op == OCLOSURE) 
                d.inspectList(n.Func.Closure.Nbody);
            else if (n.Op == ODOTMETH || n.Op == OCALLPART) 
                d.foundDep(asNode(n.Type.FuncType().Nname));
                        return true;

        }

        // foundDep records that we've found a dependency on n by adding it to
        // seen.
        private static void foundDep(this ptr<initDeps> _addr_d, ptr<Node> _addr_n)
        {
            ref initDeps d = ref _addr_d.val;
            ref Node n = ref _addr_n.val;
 
            // Can happen with method expressions involving interface
            // types; e.g., fixedbugs/issue4495.go.
            if (n == null)
            {
                return ;
            } 

            // Names without definitions aren't interesting as far as
            // initialization ordering goes.
            if (n.Name.Defn == null)
            {
                return ;
            }

            if (d.seen.Has(n))
            {
                return ;
            }

            d.seen.Add(n);
            if (d.transitive && n.Class() == PFUNC)
            {
                d.inspectList(n.Name.Defn.Nbody);
            }

        }

        // declOrder implements heap.Interface, ordering assignment statements
        // by the position of their first LHS expression.
        //
        // N.B., the Pos of the first LHS expression is used because because
        // an OAS node's Pos may not be unique. For example, given the
        // declaration "var a, b = f(), g()", "a" must be ordered before "b",
        // but both OAS nodes use the "=" token's position as their Pos.
        private partial struct declOrder // : slice<ptr<Node>>
        {
        }

        private static long Len(this declOrder s)
        {
            return len(s);
        }
        private static bool Less(this declOrder s, long i, long j)
        {
            return firstLHS(_addr_s[i]).Pos.Before(firstLHS(_addr_s[j]).Pos);
        }
        private static void Swap(this declOrder s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }

        private static void Push(this ptr<declOrder> _addr_s, object x)
        {
            ref declOrder s = ref _addr_s.val;

            s.val = append(s.val, x._<ptr<Node>>());
        }
        private static void Pop(this ptr<declOrder> _addr_s)
        {
            ref declOrder s = ref _addr_s.val;

            var n = (s.val)[len(s.val) - 1L];
            s.val = (s.val)[..len(s.val) - 1L];
            return n;
        }

        // firstLHS returns the first expression on the left-hand side of
        // assignment n.
        private static ptr<Node> firstLHS(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == OAS) 
                return _addr_n.Left!;
            else if (n.Op == OAS2DOTTYPE || n.Op == OAS2FUNC || n.Op == OAS2RECV || n.Op == OAS2MAPR) 
                return _addr_n.List.First()!;
                        Fatalf("unexpected Op: %v", n.Op);
            return _addr_null!;

        }
    }
}}}}
