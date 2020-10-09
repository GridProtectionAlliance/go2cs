// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:32 UTC
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

        private static slice<dwarf.Scope> assembleScopes(ptr<obj.LSym> _addr_fnsym, ptr<Node> _addr_fn, slice<ptr<dwarf.Var>> dwarfVars, slice<ScopeID> varScopes)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;
            ref Node fn = ref _addr_fn.val;
 
            // Initialize the DWARF scope tree based on lexical scopes.
            var dwarfScopes = make_slice<dwarf.Scope>(1L + len(fn.Func.Parents));
            foreach (var (i, parent) in fn.Func.Parents)
            {
                dwarfScopes[i + 1L].Parent = int32(parent);
            }
            scopeVariables(dwarfVars, varScopes, dwarfScopes);
            scopePCs(_addr_fnsym, fn.Func.Marks, dwarfScopes);
            return compactScopes(dwarfScopes);

        }

        // scopeVariables assigns DWARF variable records to their scopes.
        private static void scopeVariables(slice<ptr<dwarf.Var>> dwarfVars, slice<ScopeID> varScopes, slice<dwarf.Scope> dwarfScopes)
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

        // scopePCs assigns PC ranges to their scopes.
        private static void scopePCs(ptr<obj.LSym> _addr_fnsym, slice<Mark> marks, slice<dwarf.Scope> dwarfScopes)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;
 
            // If there aren't any child scopes (in particular, when scope
            // tracking is disabled), we can skip a whole lot of work.
            if (len(marks) == 0L)
            {
                return ;
            }

            var p0 = fnsym.Func.Text;
            var scope = findScope(marks, p0.Pos);
            {
                var p = fnsym.Func.Text;

                while (p != null)
                {
                    if (p.Pos == p0.Pos)
                    {
                        continue;
                    p = p.Link;
                    }

                    dwarfScopes[scope].AppendRange(new dwarf.Range(Start:p0.Pc,End:p.Pc));
                    p0 = p;
                    scope = findScope(marks, p0.Pos);

                }

            }
            if (p0.Pc < fnsym.Size)
            {
                dwarfScopes[scope].AppendRange(new dwarf.Range(Start:p0.Pc,End:fnsym.Size));
            }

        }

        private static slice<dwarf.Scope> compactScopes(slice<dwarf.Scope> dwarfScopes)
        { 
            // Reverse pass to propagate PC ranges to parent scopes.
            for (var i = len(dwarfScopes) - 1L; i > 0L; i--)
            {
                var s = _addr_dwarfScopes[i];
                dwarfScopes[s.Parent].UnifyRanges(s);
            }


            return dwarfScopes;

        }

        private partial struct varsByScopeAndOffset
        {
            public slice<ptr<dwarf.Var>> vars;
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
