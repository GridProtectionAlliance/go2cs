// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:42 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\dwinl.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // To identify variables by original source position.
        private partial struct varPos
        {
            public @string DeclName;
            public @string DeclFile;
            public ulong DeclLine;
            public ulong DeclCol;
        }

        // This is the main entry point for collection of raw material to
        // drive generation of DWARF "inlined subroutine" DIEs. See proposal
        // 22080 for more details and background info.
        private static dwarf.InlCalls assembleInlines(ptr<obj.LSym> _addr_fnsym, slice<ptr<dwarf.Var>> dwVars)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;

            ref dwarf.InlCalls inlcalls = ref heap(out ptr<dwarf.InlCalls> _addr_inlcalls);

            if (Debug_gendwarfinl != 0L)
            {
                Ctxt.Logf("assembling DWARF inlined routine info for %v\n", fnsym.Name);
            } 

            // This maps inline index (from Ctxt.InlTree) to index in inlcalls.Calls
            var imap = make_map<long, long>(); 

            // Walk progs to build up the InlCalls data structure
            src.XPos prevpos = default;
            {
                var p__prev1 = p;

                var p = fnsym.Func.Text;

                while (p != null)
                {
                    if (p.Pos == prevpos)
                    {
                        continue;
                    p = p.Link;
                    }

                    var ii = posInlIndex(p.Pos);
                    if (ii >= 0L)
                    {
                        insertInlCall(_addr_inlcalls, ii, imap);
                    }

                    prevpos = p.Pos;

                } 

                // This is used to partition DWARF vars by inline index. Vars not
                // produced by the inliner will wind up in the vmap[0] entry.


                p = p__prev1;
            } 

            // This is used to partition DWARF vars by inline index. Vars not
            // produced by the inliner will wind up in the vmap[0] entry.
            var vmap = make_map<int, slice<ptr<dwarf.Var>>>(); 

            // Now walk the dwarf vars and partition them based on whether they
            // were produced by the inliner (dwv.InlIndex > 0) or were original
            // vars/params from the function (dwv.InlIndex == 0).
            foreach (var (_, dwv) in dwVars)
            {
                vmap[dwv.InlIndex] = append(vmap[dwv.InlIndex], dwv); 

                // Zero index => var was not produced by an inline
                if (dwv.InlIndex == 0L)
                {
                    continue;
                } 

                // Look up index in our map, then tack the var in question
                // onto the vars list for the correct inlined call.
                ii = int(dwv.InlIndex) - 1L;
                var (idx, ok) = imap[ii];
                if (!ok)
                { 
                    // We can occasionally encounter a var produced by the
                    // inliner for which there is no remaining prog; add a new
                    // entry to the call list in this scenario.
                    idx = insertInlCall(_addr_inlcalls, ii, imap);

                }

                inlcalls.Calls[idx].InlVars = append(inlcalls.Calls[idx].InlVars, dwv);

            } 

            // Post process the map above to assign child indices to vars.
            //
            // A given variable is treated differently depending on whether it
            // is part of the top-level function (ii == 0) or if it was
            // produced as a result of an inline (ii != 0).
            //
            // If a variable was not produced by an inline and its containing
            // function was not inlined, then we just assign an ordering of
            // based on variable name.
            //
            // If a variable was not produced by an inline and its containing
            // function was inlined, then we need to assign a child index
            // based on the order of vars in the abstract function (in
            // addition, those vars that don't appear in the abstract
            // function, such as "~r1", are flagged as such).
            //
            // If a variable was produced by an inline, then we locate it in
            // the pre-inlining decls for the target function and assign child
            // index accordingly.
            {
                var ii__prev1 = ii;

                foreach (var (__ii, __sl) in vmap)
                {
                    ii = __ii;
                    sl = __sl;
                    map<varPos, long> m = default;
                    if (ii == 0L)
                    {
                        if (!fnsym.WasInlined())
                        {
                            {
                                var v__prev2 = v;

                                foreach (var (__j, __v) in sl)
                                {
                                    j = __j;
                                    v = __v;
                                    v.ChildIndex = int32(j);
                                }

                                v = v__prev2;
                            }

                            continue;

                        }
                    else
                        m = makePreinlineDclMap(_addr_fnsym);

                    }                    {
                        var ifnlsym = Ctxt.InlTree.InlinedFunction(int(ii - 1L));
                        m = makePreinlineDclMap(_addr_ifnlsym);
                    } 

                    // Here we assign child indices to variables based on
                    // pre-inlined decls, and set the "IsInAbstract" flag
                    // appropriately. In addition: parameter and local variable
                    // names are given "middle dot" version numbers as part of the
                    // writing them out to export data (see issue 4326). If DWARF
                    // inlined routine generation is turned on, we want to undo
                    // this versioning, since DWARF variables in question will be
                    // parented by the inlined routine and not the top-level
                    // caller.
                    var synthCount = len(m);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in sl)
                        {
                            v = __v;
                            var canonName = unversion(v.Name);
                            varPos vp = new varPos(DeclName:canonName,DeclFile:v.DeclFile,DeclLine:v.DeclLine,DeclCol:v.DeclCol,);
                            var synthesized = strings.HasPrefix(v.Name, "~r") || canonName == "_" || strings.HasPrefix(v.Name, "~b");
                            {
                                var idx__prev1 = idx;

                                var (idx, found) = m[vp];

                                if (found)
                                {
                                    v.ChildIndex = int32(idx);
                                    v.IsInAbstract = !synthesized;
                                    v.Name = canonName;
                                }
                                else
                                { 
                                    // Variable can't be found in the pre-inline dcl list.
                                    // In the top-level case (ii=0) this can happen
                                    // because a composite variable was split into pieces,
                                    // and we're looking at a piece. We can also see
                                    // return temps (~r%d) that were created during
                                    // lowering, or unnamed params ("_").
                                    v.ChildIndex = int32(synthCount);
                                    synthCount++;

                                }

                                idx = idx__prev1;

                            }

                        }

                        v = v__prev2;
                    }
                } 

                // Make a second pass through the progs to compute PC ranges for
                // the various inlined calls.

                ii = ii__prev1;
            }

            var start = int64(-1L);
            long curii = -1L;
            ptr<obj.Prog> prevp;
            {
                var p__prev1 = p;

                p = fnsym.Func.Text;

                while (p != null)
                {
                    if (prevp != null && p.Pos == prevp.Pos)
                    {
                        continue;
                    prevp = p;
                p = p.Link;
                    }

                    ii = posInlIndex(p.Pos);
                    if (ii == curii)
                    {
                        continue;
                    } 
                    // Close out the current range
                    if (start != -1L)
                    {
                        addRange(inlcalls.Calls, start, p.Pc, curii, imap);
                    } 
                    // Begin new range
                    start = p.Pc;
                    curii = ii;

                }


                p = p__prev1;
            }
            if (start != -1L)
            {
                addRange(inlcalls.Calls, start, fnsym.Size, curii, imap);
            } 

            // Debugging
            if (Debug_gendwarfinl != 0L)
            {
                dumpInlCalls(inlcalls);
                dumpInlVars(dwVars);
            }

            return inlcalls;

        }

        // Secondary hook for DWARF inlined subroutine generation. This is called
        // late in the compilation when it is determined that we need an
        // abstract function DIE for an inlined routine imported from a
        // previously compiled package.
        private static void genAbstractFunc(ptr<obj.LSym> _addr_fn)
        {
            ref obj.LSym fn = ref _addr_fn.val;

            var ifn = Ctxt.DwFixups.GetPrecursorFunc(fn);
            if (ifn == null)
            {
                Ctxt.Diag("failed to locate precursor fn for %v", fn);
                return ;
            }

            if (Debug_gendwarfinl != 0L)
            {
                Ctxt.Logf("DwarfAbstractFunc(%v)\n", fn.Name);
            }

            Ctxt.DwarfAbstractFunc(ifn, fn, myimportpath);

        }

        // Undo any versioning performed when a name was written
        // out as part of export data.
        private static @string unversion(@string name)
        {
            {
                var i = strings.Index(name, "·");

                if (i > 0L)
                {
                    name = name[..i];
                }

            }

            return name;

        }

        // Given a function that was inlined as part of the compilation, dig
        // up the pre-inlining DCL list for the function and create a map that
        // supports lookup of pre-inline dcl index, based on variable
        // position/name. NB: the recipe for computing variable pos/file/line
        // needs to be kept in sync with the similar code in gc.createSimpleVars
        // and related functions.
        private static map<varPos, long> makePreinlineDclMap(ptr<obj.LSym> _addr_fnsym)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;

            var dcl = preInliningDcls(fnsym);
            var m = make_map<varPos, long>();
            foreach (var (i, n) in dcl)
            {
                var pos = Ctxt.InnermostPos(n.Pos);
                varPos vp = new varPos(DeclName:unversion(n.Sym.Name),DeclFile:pos.RelFilename(),DeclLine:pos.RelLine(),DeclCol:pos.Col(),);
                {
                    var (_, found) = m[vp];

                    if (found)
                    {
                        Fatalf("child dcl collision on symbol %s within %v\n", n.Sym.Name, fnsym.Name);
                    }

                }

                m[vp] = i;

            }
            return m;

        }

        private static long insertInlCall(ptr<dwarf.InlCalls> _addr_dwcalls, long inlIdx, map<long, long> imap)
        {
            ref dwarf.InlCalls dwcalls = ref _addr_dwcalls.val;

            var (callIdx, found) = imap[inlIdx];
            if (found)
            {
                return callIdx;
            } 

            // Haven't seen this inline yet. Visit parent of inline if there
            // is one. We do this first so that parents appear before their
            // children in the resulting table.
            long parCallIdx = -1L;
            var parInlIdx = Ctxt.InlTree.Parent(inlIdx);
            if (parInlIdx >= 0L)
            {
                parCallIdx = insertInlCall(_addr_dwcalls, parInlIdx, imap);
            } 

            // Create new entry for this inline
            var inlinedFn = Ctxt.InlTree.InlinedFunction(inlIdx);
            var callXPos = Ctxt.InlTree.CallPos(inlIdx);
            var absFnSym = Ctxt.DwFixups.AbsFuncDwarfSym(inlinedFn);
            var pb = Ctxt.PosTable.Pos(callXPos).Base();
            var callFileSym = Ctxt.Lookup(pb.SymFilename());
            dwarf.InlCall ic = new dwarf.InlCall(InlIndex:inlIdx,CallFile:callFileSym,CallLine:uint32(callXPos.Line()),AbsFunSym:absFnSym,Root:parCallIdx==-1,);
            dwcalls.Calls = append(dwcalls.Calls, ic);
            callIdx = len(dwcalls.Calls) - 1L;
            imap[inlIdx] = callIdx;

            if (parCallIdx != -1L)
            { 
                // Add this inline to parent's child list
                dwcalls.Calls[parCallIdx].Children = append(dwcalls.Calls[parCallIdx].Children, callIdx);

            }

            return callIdx;

        }

        // Given a src.XPos, return its associated inlining index if it
        // corresponds to something created as a result of an inline, or -1 if
        // there is no inline info. Note that the index returned will refer to
        // the deepest call in the inlined stack, e.g. if you have "A calls B
        // calls C calls D" and all three callees are inlined (B, C, and D),
        // the index for a node from the inlined body of D will refer to the
        // call to D from C. Whew.
        private static long posInlIndex(src.XPos xpos)
        {
            var pos = Ctxt.PosTable.Pos(xpos);
            {
                var b = pos.Base();

                if (b != null)
                {
                    var ii = b.InliningIndex();
                    if (ii >= 0L)
                    {
                        return ii;
                    }

                }

            }

            return -1L;

        }

        private static void addRange(slice<dwarf.InlCall> calls, long start, long end, long ii, map<long, long> imap) => func((_, panic, __) =>
        {
            if (start == -1L)
            {
                panic("bad range start");
            }

            if (end == -1L)
            {
                panic("bad range end");
            }

            if (ii == -1L)
            {
                return ;
            }

            if (start == end)
            {
                return ;
            } 
            // Append range to correct inlined call
            var (callIdx, found) = imap[ii];
            if (!found)
            {
                Fatalf("can't find inlIndex %d in imap for prog at %d\n", ii, start);
            }

            var call = _addr_calls[callIdx];
            call.Ranges = append(call.Ranges, new dwarf.Range(Start:start,End:end));

        });

        private static void dumpInlCall(dwarf.InlCalls inlcalls, long idx, long ilevel)
        {
            for (long i = 0L; i < ilevel; i++)
            {
                Ctxt.Logf("  ");
            }

            var ic = inlcalls.Calls[idx];
            var callee = Ctxt.InlTree.InlinedFunction(ic.InlIndex);
            Ctxt.Logf("  %d: II:%d (%s) V: (", idx, ic.InlIndex, callee.Name);
            foreach (var (_, f) in ic.InlVars)
            {
                Ctxt.Logf(" %v", f.Name);
            }
            Ctxt.Logf(" ) C: (");
            {
                var k__prev1 = k;

                foreach (var (_, __k) in ic.Children)
                {
                    k = __k;
                    Ctxt.Logf(" %v", k);
                }

                k = k__prev1;
            }

            Ctxt.Logf(" ) R:");
            foreach (var (_, r) in ic.Ranges)
            {
                Ctxt.Logf(" [%d,%d)", r.Start, r.End);
            }
            Ctxt.Logf("\n");
            {
                var k__prev1 = k;

                foreach (var (_, __k) in ic.Children)
                {
                    k = __k;
                    dumpInlCall(inlcalls, k, ilevel + 1L);
                }

                k = k__prev1;
            }
        }

        private static void dumpInlCalls(dwarf.InlCalls inlcalls)
        {
            foreach (var (k, c) in inlcalls.Calls)
            {
                if (c.Root)
                {
                    dumpInlCall(inlcalls, k, 0L);
                }

            }

        }

        private static void dumpInlVars(slice<ptr<dwarf.Var>> dwvars)
        {
            foreach (var (i, dwv) in dwvars)
            {
                @string typ = "local";
                if (dwv.Abbrev == dwarf.DW_ABRV_PARAM_LOCLIST || dwv.Abbrev == dwarf.DW_ABRV_PARAM)
                {
                    typ = "param";
                }

                long ia = 0L;
                if (dwv.IsInAbstract)
                {
                    ia = 1L;
                }

                Ctxt.Logf("V%d: %s CI:%d II:%d IA:%d %s\n", i, dwv.Name, dwv.ChildIndex, dwv.InlIndex - 1L, ia, typ);

            }

        }
    }
}}}}
