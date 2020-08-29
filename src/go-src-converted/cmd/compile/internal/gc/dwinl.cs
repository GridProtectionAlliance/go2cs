// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:26:42 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\dwinl.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using sort = go.sort_package;
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
        private static dwarf.InlCalls assembleInlines(ref obj.LSym fnsym, ref Node fn, slice<ref dwarf.Var> dwVars)
        {
            dwarf.InlCalls inlcalls = default;

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
                        insertInlCall(ref inlcalls, ii, imap);
                    }
                    prevpos = p.Pos;
                } 

                // This is used to partition DWARF vars by inline index. Vars not
                // produced by the inliner will wind up in the vmap[0] entry.


                p = p__prev1;
            } 

            // This is used to partition DWARF vars by inline index. Vars not
            // produced by the inliner will wind up in the vmap[0] entry.
            var vmap = make_map<int, slice<ref dwarf.Var>>(); 

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
                    idx = insertInlCall(ref inlcalls, ii, imap);
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
                    sort.Sort(byClassThenName(sl));
                    map<varPos, long> m = default;
                    if (ii == 0L)
                    {
                        if (!fnsym.WasInlined())
                        {
                            {
                                long j__prev2 = j;

                                for (long j = 0L; j < len(sl); j++)
                                {
                                    sl[j].ChildIndex = int32(j);
                                }


                                j = j__prev2;
                            }
                            continue;
                        }
                    else
                        m = makePreinlineDclMap(fnsym);
                    }                    {
                        var ifnlsym = Ctxt.InlTree.InlinedFunction(int(ii - 1L));
                        m = makePreinlineDclMap(ifnlsym);
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
                        long j__prev2 = j;

                        for (j = 0L; j < len(sl); j++)
                        {
                            var canonName = unversion(sl[j].Name);
                            varPos vp = new varPos(DeclName:canonName,DeclFile:sl[j].DeclFile,DeclLine:sl[j].DeclLine,DeclCol:sl[j].DeclCol,);
                            var synthesized = strings.HasPrefix(sl[j].Name, "~r") || canonName == "_";
                            {
                                var idx__prev1 = idx;

                                var (idx, found) = m[vp];

                                if (found)
                                {
                                    sl[j].ChildIndex = int32(idx);
                                    sl[j].IsInAbstract = !synthesized;
                                    sl[j].Name = canonName;
                                }
                                else
                                { 
                                    // Variable can't be found in the pre-inline dcl list.
                                    // In the top-level case (ii=0) this can happen
                                    // because a composite variable was split into pieces,
                                    // and we're looking at a piece. We can also see
                                    // return temps (~r%d) that were created during
                                    // lowering, or unnamed params ("_").
                                    sl[j].ChildIndex = int32(synthCount);
                                    synthCount += 1L;
                                }

                                idx = idx__prev1;

                            }
                        }


                        j = j__prev2;
                    }
                } 

                // Make a second pass through the progs to compute PC ranges for
                // the various inlined calls.

                ii = ii__prev1;
            }

            long curii = -1L;
            ref dwarf.Range crange = default;
            ref obj.Prog prevp = default;
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
                    else
                    { 
                        // Close out the current range
                        endRange(crange, prevp); 

                        // Begin new range
                        crange = beginRange(inlcalls.Calls, p, ii, imap);
                        curii = ii;
                    }
                }


                p = p__prev1;
            }
            if (prevp != null)
            {
                endRange(crange, prevp);
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
        private static void genAbstractFunc(ref obj.LSym fn)
        {
            var ifn = Ctxt.DwFixups.GetPrecursorFunc(fn);
            if (ifn == null)
            {
                Ctxt.Diag("failed to locate precursor fn for %v", fn);
                return;
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
        // position/name.
        private static map<varPos, long> makePreinlineDclMap(ref obj.LSym fnsym)
        {
            var dcl = preInliningDcls(fnsym);
            var m = make_map<varPos, long>();
            for (long i = 0L; i < len(dcl); i++)
            {
                var n = dcl[i];
                var pos = Ctxt.InnermostPos(n.Pos);
                varPos vp = new varPos(DeclName:unversion(n.Sym.Name),DeclFile:pos.Base().SymFilename(),DeclLine:pos.Line(),DeclCol:pos.Col(),);
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

        private static long insertInlCall(ref dwarf.InlCalls dwcalls, long inlIdx, map<long, long> imap)
        {
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
                parCallIdx = insertInlCall(dwcalls, parInlIdx, imap);
            } 

            // Create new entry for this inline
            var inlinedFn = Ctxt.InlTree.InlinedFunction(int(inlIdx));
            var callXPos = Ctxt.InlTree.CallPos(int(inlIdx));
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

        private static void endRange(ref dwarf.Range crange, ref obj.Prog p)
        {
            if (crange == null)
            {
                return;
            }
            crange.End = p.Pc;
        }

        private static ref dwarf.Range beginRange(slice<dwarf.InlCall> calls, ref obj.Prog p, long ii, map<long, long> imap)
        {
            if (ii == -1L)
            {
                return null;
            }
            var (callIdx, found) = imap[ii];
            if (!found)
            {
                Fatalf("internal error: can't find inlIndex %d in imap for prog at %d\n", ii, p.Pc);
            }
            var call = ref calls[callIdx]; 

            // Set up range and append to correct inlined call
            call.Ranges = append(call.Ranges, new dwarf.Range(Start:p.Pc,End:-1));
            return ref call.Ranges[len(call.Ranges) - 1L];
        }

        private static bool cmpDwarfVar(ref dwarf.Var a, ref dwarf.Var b)
        { 
            // named before artificial
            long aart = 0L;
            if (strings.HasPrefix(a.Name, "~r"))
            {
                aart = 1L;
            }
            long bart = 0L;
            if (strings.HasPrefix(b.Name, "~r"))
            {
                bart = 1L;
            }
            if (aart != bart)
            {
                return aart < bart;
            } 

            // otherwise sort by name
            return a.Name < b.Name;
        }

        // byClassThenName implements sort.Interface for []*dwarf.Var using cmpDwarfVar.
        private partial struct byClassThenName // : slice<ref dwarf.Var>
        {
        }

        private static long Len(this byClassThenName s)
        {
            return len(s);
        }
        private static bool Less(this byClassThenName s, long i, long j)
        {
            return cmpDwarfVar(s[i], s[j]);
        }
        private static void Swap(this byClassThenName s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        private static void dumpInlCall(dwarf.InlCalls inlcalls, long idx, long ilevel)
        {
            {
                long i = 0L;

                while (i < ilevel)
                {
                    Ctxt.Logf("  ");
                    i += 1L;
                }

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
            var n = len(inlcalls.Calls);
            {
                long k = 0L;

                while (k < n)
                {
                    if (inlcalls.Calls[k].Root)
                    {
                        dumpInlCall(inlcalls, k, 0L);
                    k += 1L;
                    }
                }

            }
        }

        private static void dumpInlVars(slice<ref dwarf.Var> dwvars)
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
