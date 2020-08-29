// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:08 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\racewalk.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // The instrument pass modifies the code tree for instrumentation.
        //
        // For flag_race it modifies the function as follows:
        //
        // 1. It inserts a call to racefuncenterfp at the beginning of each function.
        // 2. It inserts a call to racefuncexit at the end of each function.
        // 3. It inserts a call to raceread before each memory read.
        // 4. It inserts a call to racewrite before each memory write.
        //
        // For flag_msan:
        //
        // 1. It inserts a call to msanread before each memory read.
        // 2. It inserts a call to msanwrite before each memory write.
        //
        // The rewriting is not yet complete. Certain nodes are not rewritten
        // but should be.

        // TODO(dvyukov): do not instrument initialization as writes:
        // a := make([]int, 10)

        // Do not instrument the following packages at all,
        // at best instrumentation would cause infinite recursion.
        private static @string omit_pkgs = new slice<@string>(new @string[] { "runtime/internal/atomic", "runtime/internal/sys", "runtime", "runtime/race", "runtime/msan" });

        // Only insert racefuncenterfp/racefuncexit into the following packages.
        // Memory accesses in the packages are either uninteresting or will cause false positives.
        private static @string norace_inst_pkgs = new slice<@string>(new @string[] { "sync", "sync/atomic" });

        private static bool ispkgin(slice<@string> pkgs)
        {
            if (myimportpath != "")
            {
                foreach (var (_, p) in pkgs)
                {
                    if (myimportpath == p)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void instrument(ref Node fn)
        {
            if (ispkgin(omit_pkgs) || fn.Func.Pragma & Norace != 0L)
            {
                return;
            }
            if (!flag_race || !ispkgin(norace_inst_pkgs))
            {
                instrumentlist(fn.Nbody, null); 

                // nothing interesting for race detector in fn->enter
                instrumentlist(fn.Func.Exit, null);
            }
            if (flag_race)
            { 
                // nodpc is the PC of the caller as extracted by
                // getcallerpc. We use -widthptr(FP) for x86.
                // BUG: this will not work on arm.
                var nodpc = nodfp.Value;
                nodpc.Type = types.Types[TUINTPTR];
                nodpc.Xoffset = int64(-Widthptr);
                var savedLineno = lineno;
                lineno = src.NoXPos;
                var nd = mkcall("racefuncenter", null, null, ref nodpc);

                fn.Func.Enter.Prepend(nd);
                nd = mkcall("racefuncexit", null, null);
                fn.Func.Exit.Append(nd);
                fn.Func.Dcl = append(fn.Func.Dcl, ref nodpc);
                lineno = savedLineno;
            }
            if (Debug['W'] != 0L)
            {
                var s = fmt.Sprintf("after instrument %v", fn.Func.Nname.Sym);
                dumplist(s, fn.Nbody);
                s = fmt.Sprintf("enter %v", fn.Func.Nname.Sym);
                dumplist(s, fn.Func.Enter);
                s = fmt.Sprintf("exit %v", fn.Func.Nname.Sym);
                dumplist(s, fn.Func.Exit);
            }
        }

        private static void instrumentlist(Nodes l, ref Nodes init)
        {
            var s = l.Slice();
            foreach (var (i) in s)
            {
                Nodes instr = default;
                instrumentnode(ref s[i], ref instr, 0L, 0L);
                if (init == null)
                {
                    s[i].Ninit.AppendNodes(ref instr);
                }
                else
                {
                    init.AppendNodes(ref instr);
                }
            }
        }

        // walkexpr and walkstmt combined
        // walks the tree and adds calls to the
        // instrumentation code to top-level (statement) nodes' init
        private static void instrumentnode(ptr<ptr<Node>> np, ref Nodes init, long wr, long skip)
        {
            var n = np.Value;

            if (n == null)
            {
                return;
            }
            if (Debug['w'] > 1L)
            {
                Dump("instrument-before", n);
            }
            setlineno(n);
            if (init == null)
            {
                Fatalf("instrument: bad init list");
            }
            if (init == ref n.Ninit)
            { 
                // If init == &n->ninit and n->ninit is non-nil,
                // instrumentnode might append it to itself.
                // nil it out and handle it separately before putting it back.
                var l = n.Ninit;

                n.Ninit.Set(null);
                instrumentlist(l, null);
                instrumentnode(ref n, ref l, wr, skip); // recurse with nil n->ninit
                appendinit(ref n, l);
                np.Value = n;
                return;
            }
            instrumentlist(n.Ninit, null);


            if (n.Op == OAS || n.Op == OAS2FUNC) 
                instrumentnode(ref n.Left, init, 1L, 0L);
                instrumentnode(ref n.Right, init, 0L, 0L); 

                // can't matter
            else if (n.Op == OCFUNC || n.Op == OVARKILL || n.Op == OVARLIVE)             else if (n.Op == OBLOCK) 
                var ls = n.List.Slice();
                var afterCall = false;
                foreach (var (i) in ls)
                {
                    var op = ls[i].Op; 
                    // Scan past OAS nodes copying results off stack.
                    // Those must not be instrumented, because the
                    // instrumentation calls will smash the results.
                    // The assignments are to temporaries, so they cannot
                    // be involved in races and need not be instrumented.
                    if (afterCall && op == OAS && iscallret(ls[i].Right))
                    {
                        continue;
                    }
                    instrumentnode(ref ls[i], ref ls[i].Ninit, 0L, 0L);
                    afterCall = (op == OCALLFUNC || op == OCALLMETH || op == OCALLINTER);
                }
            else if (n.Op == ODEFER) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == OPROC) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == OCALLINTER) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == OCALLFUNC) 
                // Note that runtime.typedslicecopy is the only
                // assignment-like function call in the AST at this
                // point (between walk and SSA); since we don't
                // instrument it here, typedslicecopy is manually
                // instrumented in runtime. Calls to the write barrier
                // and typedmemmove are created later by SSA, so those
                // still appear as OAS nodes at this point.
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == ONOT || n.Op == OMINUS || n.Op == OPLUS || n.Op == OREAL || n.Op == OIMAG || n.Op == OCOM) 
                instrumentnode(ref n.Left, init, wr, 0L);
            else if (n.Op == ODOTINTER) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == ODOT) 
                instrumentnode(ref n.Left, init, 0L, 1L);
                callinstr(ref n, init, wr, skip);
            else if (n.Op == ODOTPTR) // dst = (*x).f with implicit *; otherwise it's ODOT+OIND
                instrumentnode(ref n.Left, init, 0L, 0L);

                callinstr(ref n, init, wr, skip);
            else if (n.Op == OIND) // *p
                instrumentnode(ref n.Left, init, 0L, 0L);

                callinstr(ref n, init, wr, skip);
            else if (n.Op == OSPTR || n.Op == OLEN || n.Op == OCAP) 
                instrumentnode(ref n.Left, init, 0L, 0L);
                if (n.Left.Type.IsMap())
                {
                    var n1 = nod(OCONVNOP, n.Left, null);
                    n1.Type = types.NewPtr(types.Types[TUINT8]);
                    n1 = nod(OIND, n1, null);
                    n1 = typecheck(n1, Erv);
                    callinstr(ref n1, init, 0L, skip);
                }
            else if (n.Op == OLSH || n.Op == ORSH || n.Op == OAND || n.Op == OANDNOT || n.Op == OOR || n.Op == OXOR || n.Op == OSUB || n.Op == OMUL || n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGE || n.Op == OGT || n.Op == OADD || n.Op == OCOMPLEX) 
                instrumentnode(ref n.Left, init, wr, 0L);
                instrumentnode(ref n.Right, init, wr, 0L);
            else if (n.Op == OANDAND || n.Op == OOROR) 
                instrumentnode(ref n.Left, init, wr, 0L); 

                // walk has ensured the node has moved to a location where
                // side effects are safe.
                // n->right may not be executed,
                // so instrumentation goes to n->right->ninit, not init.
                instrumentnode(ref n.Right, ref n.Right.Ninit, wr, 0L);
            else if (n.Op == ONAME) 
                callinstr(ref n, init, wr, skip);
            else if (n.Op == OCONV) 
                instrumentnode(ref n.Left, init, wr, 0L);
            else if (n.Op == OCONVNOP) 
                instrumentnode(ref n.Left, init, wr, 0L);
            else if (n.Op == ODIV || n.Op == OMOD) 
                instrumentnode(ref n.Left, init, wr, 0L);
                instrumentnode(ref n.Right, init, wr, 0L);
            else if (n.Op == OINDEX) 
                if (!n.Left.Type.IsArray())
                {
                    instrumentnode(ref n.Left, init, 0L, 0L);
                }
                else if (!islvalue(n.Left))
                { 
                    // index of unaddressable array, like Map[k][i].
                    instrumentnode(ref n.Left, init, wr, 0L);

                    instrumentnode(ref n.Right, init, 0L, 0L);
                    break;
                }
                instrumentnode(ref n.Right, init, 0L, 0L);
                if (!n.Left.Type.IsString())
                {
                    callinstr(ref n, init, wr, skip);
                }
            else if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR || n.Op == OSLICESTR) 
                instrumentnode(ref n.Left, init, 0L, 0L);
                var (low, high, max) = n.SliceBounds();
                instrumentnode(ref low, init, 0L, 0L);
                instrumentnode(ref high, init, 0L, 0L);
                instrumentnode(ref max, init, 0L, 0L);
                n.SetSliceBounds(low, high, max);
            else if (n.Op == OADDR) 
                instrumentnode(ref n.Left, init, 0L, 1L); 

                // n->left is Type* which is not interesting.
            else if (n.Op == OEFACE) 
                instrumentnode(ref n.Right, init, 0L, 0L);
            else if (n.Op == OITAB || n.Op == OIDATA) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == OSTRARRAYBYTETMP) 
                instrumentnode(ref n.Left, init, 0L, 0L);
            else if (n.Op == OAS2DOTTYPE) 
                instrumentnode(ref n.Left, init, 1L, 0L);
                instrumentnode(ref n.Right, init, 0L, 0L);
            else if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2) 
                instrumentnode(ref n.Left, init, 0L, 0L); 

                // should not appear in AST by now
            else if (n.Op == OSEND || n.Op == ORECV || n.Op == OCLOSE || n.Op == ONEW || n.Op == OXCASE || n.Op == OCASE || n.Op == OPANIC || n.Op == ORECOVER || n.Op == OCONVIFACE || n.Op == OCMPIFACE || n.Op == OMAKECHAN || n.Op == OMAKEMAP || n.Op == OMAKESLICE || n.Op == OCALL || n.Op == OCOPY || n.Op == OAPPEND || n.Op == ORUNESTR || n.Op == OARRAYBYTESTR || n.Op == OARRAYRUNESTR || n.Op == OSTRARRAYBYTE || n.Op == OSTRARRAYRUNE || n.Op == OINDEXMAP || n.Op == OCMPSTR || n.Op == OADDSTR || n.Op == OCALLPART || n.Op == OCLOSURE || n.Op == ORANGE || n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OMAPLIT || n.Op == OSTRUCTLIT || n.Op == OAS2 || n.Op == OAS2RECV || n.Op == OAS2MAPR || n.Op == OASOP) 
                Fatalf("instrument: %v must be lowered by now", n.Op);
            else if (n.Op == OGETG) 
                Fatalf("instrument: OGETG can happen only in runtime which we don't instrument");
            else if (n.Op == OFOR || n.Op == OFORUNTIL) 
                if (n.Left != null)
                {
                    instrumentnode(ref n.Left, ref n.Left.Ninit, 0L, 0L);
                }
                if (n.Right != null)
                {
                    instrumentnode(ref n.Right, ref n.Right.Ninit, 0L, 0L);
                }
            else if (n.Op == OIF || n.Op == OSWITCH) 
                if (n.Left != null)
                {
                    instrumentnode(ref n.Left, ref n.Left.Ninit, 0L, 0L);
                } 

                // just do generic traversal
            else if (n.Op == OCALLMETH || n.Op == ORETURN || n.Op == ORETJMP || n.Op == OSELECT || n.Op == OEMPTY || n.Op == OBREAK || n.Op == OCONTINUE || n.Op == OFALL || n.Op == OGOTO || n.Op == OLABEL)             else if (n.Op == OPRINT || n.Op == OPRINTN || n.Op == OCHECKNIL || n.Op == OCLOSUREVAR || n.Op == ODOTMETH || n.Op == OINDREGSP || n.Op == ODCL || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OTYPE || n.Op == ONONAME || n.Op == OLITERAL || n.Op == OTYPESW)             else 
                Fatalf("instrument: unknown node type %v", n.Op);
                        if (n.Op != OBLOCK)
            { // OBLOCK is handled above in a special way.
                instrumentlist(n.List, init);
            }
            instrumentlist(n.Nbody, null);
            instrumentlist(n.Rlist, null);
            np.Value = n;
        }

        private static bool isartificial(ref Node n)
        { 
            // compiler-emitted artificial things that we do not want to instrument,
            // can't possibly participate in a data race.
            // can't be seen by C/C++ and therefore irrelevant for msan.
            if (n.Op == ONAME && n.Sym != null && n.Sym.Name != "")
            {
                if (n.Sym.Name == "_")
                {
                    return true;
                } 

                // autotmp's are always local
                if (n.IsAutoTmp())
                {
                    return true;
                } 

                // statictmp's are read-only
                if (strings.HasPrefix(n.Sym.Name, "statictmp_"))
                {
                    return true;
                } 

                // go.itab is accessed only by the compiler and runtime (assume safe)
                if (n.Sym.Pkg != null && n.Sym.Pkg.Name != "" && n.Sym.Pkg.Name == "go.itab")
                {
                    return true;
                }
            }
            return false;
        }

        private static bool callinstr(ptr<ptr<Node>> np, ref Nodes init, long wr, long skip)
        {
            var n = np.Value; 

            //fmt.Printf("callinstr for %v [ %v ] etype=%v class=%v\n",
            //    n, n.Op, n.Type.Etype, n.Class)

            if (skip != 0L || n.Type == null || n.Type.Etype >= TIDEAL)
            {
                return false;
            }
            var t = n.Type; 
            // dowidth may not have been called for PEXTERN.
            dowidth(t);
            var w = t.Width;
            if (w == BADWIDTH)
            {
                Fatalf("instrument: %v badwidth", t);
            }
            if (w == 0L)
            {
                return false; // can't race on zero-sized things
            }
            if (isartificial(n))
            {
                return false;
            }
            var b = outervalue(n); 

            // it skips e.g. stores to ... parameter array
            if (isartificial(b))
            {
                return false;
            }
            var @class = b.Class(); 

            // BUG: we _may_ want to instrument PAUTO sometimes
            // e.g. if we've got a local variable/method receiver
            // that has got a pointer inside. Whether it points to
            // the heap or not is impossible to know at compile time
            if (class == PAUTOHEAP || class == PEXTERN || b.Op == OINDEX || b.Op == ODOTPTR || b.Op == OIND)
            {
                var hasCalls = false;
                inspect(n, n =>
                {

                    if (n.Op == OCALL || n.Op == OCALLFUNC || n.Op == OCALLMETH || n.Op == OCALLINTER) 
                        hasCalls = true;
                                        return !hasCalls;
                });
                if (hasCalls)
                {
                    n = detachexpr(n, init);
                    np.Value = n;
                }
                n = treecopy(n, src.NoXPos);
                makeaddable(n);
                ref Node f = default;
                if (flag_msan)
                {
                    @string name = "msanread";
                    if (wr != 0L)
                    {
                        name = "msanwrite";
                    }
                    f = mkcall(name, null, init, uintptraddr(n), nodintconst(w));
                }
                else if (flag_race && t.NumComponents() > 1L)
                { 
                    // for composite objects we have to write every address
                    // because a write might happen to any subobject.
                    // composites with only one element don't have subobjects, though.
                    name = "racereadrange";
                    if (wr != 0L)
                    {
                        name = "racewriterange";
                    }
                    f = mkcall(name, null, init, uintptraddr(n), nodintconst(w));
                }
                else if (flag_race)
                { 
                    // for non-composite objects we can write just the start
                    // address, as any write must write the first byte.
                    name = "raceread";
                    if (wr != 0L)
                    {
                        name = "racewrite";
                    }
                    f = mkcall(name, null, init, uintptraddr(n));
                }
                init.Append(f);
                return true;
            }
            return false;
        }

        // makeaddable returns a node whose memory location is the
        // same as n, but which is addressable in the Go language
        // sense.
        // This is different from functions like cheapexpr that may make
        // a copy of their argument.
        private static void makeaddable(ref Node n)
        { 
            // The arguments to uintptraddr technically have an address but
            // may not be addressable in the Go sense: for example, in the case
            // of T(v).Field where T is a struct type and v is
            // an addressable value.

            if (n.Op == OINDEX) 
                if (n.Left.Type.IsArray())
                {
                    makeaddable(n.Left);
                } 

                // Turn T(v).Field into v.Field
            else if (n.Op == ODOT || n.Op == OXDOT) 
                if (n.Left.Op == OCONVNOP)
                {
                    n.Left = n.Left.Left;
                }
                makeaddable(n.Left); 

                // nothing to do
                    }

        private static ref Node uintptraddr(ref Node n)
        {
            var r = nod(OADDR, n, null);
            r.SetBounded(true);
            r = conv(r, types.Types[TUNSAFEPTR]);
            r = conv(r, types.Types[TUINTPTR]);
            return r;
        }

        private static ref Node detachexpr(ref Node n, ref Nodes init)
        {
            var addr = nod(OADDR, n, null);
            var l = temp(types.NewPtr(n.Type));
            var @as = nod(OAS, l, addr);
            as = typecheck(as, Etop);
            as = walkexpr(as, init);
            init.Append(as);
            var ind = nod(OIND, l, null);
            ind = typecheck(ind, Erv);
            ind = walkexpr(ind, init);
            return ind;
        }

        // appendinit is like addinit in subr.go
        // but appends rather than prepends.
        private static void appendinit(ptr<ptr<Node>> np, Nodes init)
        {
            if (init.Len() == 0L)
            {
                return;
            }
            var n = np.Value;

            // There may be multiple refs to this node;
            // introduce OCONVNOP to hold init list.
            if (n.Op == ONAME || n.Op == OLITERAL) 
                n = nod(OCONVNOP, n, null);

                n.Type = n.Left.Type;
                n.SetTypecheck(1L);
                np.Value = n;
                        n.Ninit.AppendNodes(ref init);
            n.SetHasCall(true);
        }
    }
}}}}
