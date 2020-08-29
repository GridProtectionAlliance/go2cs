// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:23 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\scope.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // See golang.org/issue/20390.
        private static bool xposBefore(src.XPos p, src.XPos q)
        {
            return Ctxt.PosTable.Pos(p).Before(Ctxt.PosTable.Pos(q));
        }

        private static ScopeID findScope(slice<Mark> marks, src.XPos pos)
        {
            var i = sort.Search(len(marks), i =>
            {
                return xposBefore(pos, marks[i].Pos);
            });
            if (i == 0L)
            {
                return 0L;
            }
            return marks[i - 1L].Scope;
        }

        private static slice<dwarf.Scope> assembleScopes(ref obj.LSym fnsym, ref Node fn, slice<ref dwarf.Var> dwarfVars, slice<ScopeID> varScopes)
        { 
            // Initialize the DWARF scope tree based on lexical scopes.
            var dwarfScopes = make_slice<dwarf.Scope>(1L + len(fn.Func.Parents));
            foreach (var (i, parent) in fn.Func.Parents)
            {
                dwarfScopes[i + 1L].Parent = int32(parent);
            }
            scopeVariables(dwarfVars, varScopes, dwarfScopes);
            scopePCs(fnsym, fn.Func.Marks, dwarfScopes);
            return compactScopes(dwarfScopes);
        }

        // scopeVariables assigns DWARF variable records to their scopes.
        private static void scopeVariables(slice<ref dwarf.Var> dwarfVars, slice<ScopeID> varScopes, slice<dwarf.Scope> dwarfScopes)
        {
            sort.Stable(new varsByScopeAndOffset(dwarfVars,varScopes));

            long i0 = 0L;
            foreach (var (i) in dwarfVars)
            {
                if (varScopes[i] == varScopes[i0])
                {
                    continue;
                }
                dwarfScopes[varScopes[i0]].Vars = dwarfVars[i0..i];
                i0 = i;
            }
            if (i0 < len(dwarfVars))
            {
                dwarfScopes[varScopes[i0]].Vars = dwarfVars[i0..];
            }
        }

        // A scopedPCs represents a non-empty half-open interval of PCs that
        // share a common source position.
        private partial struct scopedPCs
        {
            public long start;
            public long end;
            public src.XPos pos;
            public ScopeID scope;
        }

        // scopePCs assigns PC ranges to their scopes.
        private static void scopePCs(ref obj.LSym fnsym, slice<Mark> marks, slice<dwarf.Scope> dwarfScopes)
        { 
            // If there aren't any child scopes (in particular, when scope
            // tracking is disabled), we can skip a whole lot of work.
            if (len(marks) == 0L)
            {
                return;
            } 

            // Break function text into scopedPCs.
            slice<scopedPCs> pcs = default;
            var p0 = fnsym.Func.Text;
            {
                var p = fnsym.Func.Text;

                while (p != null)
                {
                    if (p.Pos == p0.Pos)
                    {
                        continue;
                    p = p.Link;
                    }
                    if (p0.Pc < p.Pc)
                    {
                        pcs = append(pcs, new scopedPCs(start:p0.Pc,end:p.Pc,pos:p0.Pos));
                    }
                    p0 = p;
                }

            }
            if (p0.Pc < fnsym.Size)
            {
                pcs = append(pcs, new scopedPCs(start:p0.Pc,end:fnsym.Size,pos:p0.Pos));
            } 

            // Sort PCs by source position, and walk in parallel with
            // scope marks to assign a lexical scope to each PC interval.
            sort.Sort(pcsByPos(pcs));
            long marki = default;
            ScopeID scope = default;
            {
                var i__prev1 = i;

                foreach (var (__i) in pcs)
                {
                    i = __i;
                    while (marki < len(marks) && !xposBefore(pcs[i].pos, marks[marki].Pos))
                    {
                        scope = marks[marki].Scope;
                        marki++;
                    }

                    pcs[i].scope = scope;
                } 

                // Re-sort to create sorted PC ranges for each DWARF scope.

                i = i__prev1;
            }

            sort.Sort(pcsByPC(pcs));
            foreach (var (_, pc) in pcs)
            {
                var r = ref dwarfScopes[pc.scope].Ranges;
                {
                    var i__prev1 = i;

                    var i = len(r.Value);

                    if (i > 0L && (r.Value)[i - 1L].End == pc.start)
                    {
                        (r.Value)[i - 1L].End = pc.end;
                    }
                    else
                    {
                        r.Value = append(r.Value, new dwarf.Range(Start:pc.start,End:pc.end));
                    }

                    i = i__prev1;

                }
            }
        }

        private static slice<dwarf.Scope> compactScopes(slice<dwarf.Scope> dwarfScopes)
        { 
            // Forward pass to collapse empty scopes into parents.
            var remap = make_slice<int>(len(dwarfScopes));
            var j = int32(1L);
            {
                long i__prev1 = i;

                for (long i = 1L; i < len(dwarfScopes); i++)
                {
                    var s = ref dwarfScopes[i];
                    s.Parent = remap[s.Parent];
                    if (len(s.Vars) == 0L)
                    {
                        dwarfScopes[s.Parent].UnifyRanges(s);
                        remap[i] = s.Parent;
                        continue;
                    }
                    remap[i] = j;
                    dwarfScopes[j] = s.Value;
                    j++;
                }


                i = i__prev1;
            }
            dwarfScopes = dwarfScopes[..j]; 

            // Reverse pass to propagate PC ranges to parent scopes.
            {
                long i__prev1 = i;

                for (i = len(dwarfScopes) - 1L; i > 0L; i--)
                {
                    s = ref dwarfScopes[i];
                    dwarfScopes[s.Parent].UnifyRanges(s);
                }


                i = i__prev1;
            }

            return dwarfScopes;
        }

        private partial struct pcsByPC // : slice<scopedPCs>
        {
        }

        private static long Len(this pcsByPC s)
        {
            return len(s);
        }
        private static void Swap(this pcsByPC s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }
        private static bool Less(this pcsByPC s, long i, long j)
        {
            return s[i].start < s[j].start;
        }

        private partial struct pcsByPos // : slice<scopedPCs>
        {
        }

        private static long Len(this pcsByPos s)
        {
            return len(s);
        }
        private static void Swap(this pcsByPos s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }
        private static bool Less(this pcsByPos s, long i, long j)
        {
            return xposBefore(s[i].pos, s[j].pos);
        }

        private partial struct varsByScopeAndOffset
        {
            public slice<ref dwarf.Var> vars;
            public slice<ScopeID> scopes;
        }

        private static long Len(this varsByScopeAndOffset v)
        {
            return len(v.vars);
        }

        private static bool Less(this varsByScopeAndOffset v, long i, long j)
        {
            if (v.scopes[i] != v.scopes[j])
            {
                return v.scopes[i] < v.scopes[j];
            }
            return v.vars[i].StackOffset < v.vars[j].StackOffset;
        }

        private static void Swap(this varsByScopeAndOffset v, long i, long j)
        {
            v.vars[i] = v.vars[j];
            v.vars[j] = v.vars[i];
            v.scopes[i] = v.scopes[j];
            v.scopes[j] = v.scopes[i];
        }
    }
}}}}
