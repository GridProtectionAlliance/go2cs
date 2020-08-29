// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:53 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\pgen.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using fmt = go.fmt_package;
using math = go.math_package;
using rand = go.math.rand_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // "Portable" code generation.
        private static long nBackendWorkers = default;        private static slice<ref Node> compilequeue = default;

        private static void emitptrargsmap()
        {
            if (Curfn.funcname() == "_")
            {
                return;
            }
            var sym = lookup(fmt.Sprintf("%s.args_stackmap", Curfn.funcname()));
            var lsym = sym.Linksym();

            var nptr = int(Curfn.Type.ArgWidth() / int64(Widthptr));
            var bv = bvalloc(int32(nptr) * 2L);
            long nbitmap = 1L;
            if (Curfn.Type.NumResults() > 0L)
            {
                nbitmap = 2L;
            }
            var off = duint32(lsym, 0L, uint32(nbitmap));
            off = duint32(lsym, off, uint32(bv.n));

            if (Curfn.IsMethod())
            {
                onebitwalktype1(Curfn.Type.Recvs(), 0L, bv);
            }
            if (Curfn.Type.NumParams() > 0L)
            {
                onebitwalktype1(Curfn.Type.Params(), 0L, bv);
            }
            off = dbvec(lsym, off, bv);

            if (Curfn.Type.NumResults() > 0L)
            {
                onebitwalktype1(Curfn.Type.Results(), 0L, bv);
                off = dbvec(lsym, off, bv);
            }
            ggloblsym(lsym, int32(off), obj.RODATA | obj.LOCAL);
        }

        // cmpstackvarlt reports whether the stack variable a sorts before b.
        //
        // Sort the list of stack variables. Autos after anything else,
        // within autos, unused after used, within used, things with
        // pointers first, zeroed things first, and then decreasing size.
        // Because autos are laid out in decreasing addresses
        // on the stack, pointers first, zeroed things first and decreasing size
        // really means, in memory, things with pointers needing zeroing at
        // the top of the stack and increasing in size.
        // Non-autos sort on offset.
        private static bool cmpstackvarlt(ref Node a, ref Node b)
        {
            if ((a.Class() == PAUTO) != (b.Class() == PAUTO))
            {
                return b.Class() == PAUTO;
            }
            if (a.Class() != PAUTO)
            {
                return a.Xoffset < b.Xoffset;
            }
            if (a.Name.Used() != b.Name.Used())
            {
                return a.Name.Used();
            }
            var ap = types.Haspointers(a.Type);
            var bp = types.Haspointers(b.Type);
            if (ap != bp)
            {
                return ap;
            }
            ap = a.Name.Needzero();
            bp = b.Name.Needzero();
            if (ap != bp)
            {
                return ap;
            }
            if (a.Type.Width != b.Type.Width)
            {
                return a.Type.Width > b.Type.Width;
            }
            return a.Sym.Name < b.Sym.Name;
        }

        // byStackvar implements sort.Interface for []*Node using cmpstackvarlt.
        private partial struct byStackVar // : slice<ref Node>
        {
        }

        private static long Len(this byStackVar s)
        {
            return len(s);
        }
        private static bool Less(this byStackVar s, long i, long j)
        {
            return cmpstackvarlt(s[i], s[j]);
        }
        private static void Swap(this byStackVar s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        private static void AllocFrame(this ref ssafn s, ref ssa.Func f)
        {
            s.stksize = 0L;
            s.stkptrsize = 0L;
            var fn = s.curfn.Func; 

            // Mark the PAUTO's unused.
            foreach (var (_, ln) in fn.Dcl)
            {
                if (ln.Class() == PAUTO)
                {
                    ln.Name.SetUsed(false);
                }
            }
            foreach (var (_, l) in f.RegAlloc)
            {
                {
                    ssa.LocalSlot (ls, ok) = l._<ssa.LocalSlot>();

                    if (ok)
                    {
                        ls.N._<ref Node>().Name.SetUsed(true);
                    }

                }
            }
            var scratchUsed = false;
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    {
                        ref Node n__prev1 = n;

                        ref Node (n, ok) = v.Aux._<ref Node>();

                        if (ok)
                        {

                            if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                                // Don't modify nodfp; it is a global.
                                if (n != nodfp)
                                {
                                    n.Name.SetUsed(true);
                                }
                            else if (n.Class() == PAUTO) 
                                n.Name.SetUsed(true);
                                                    }

                        n = n__prev1;

                    }
                    if (!scratchUsed)
                    {
                        scratchUsed = v.Op.UsesScratch();
                    }
                }
            }
            if (f.Config.NeedsFpScratch && scratchUsed)
            {
                s.scratchFpMem = tempAt(src.NoXPos, s.curfn, types.Types[TUINT64]);
            }
            sort.Sort(byStackVar(fn.Dcl)); 

            // Reassign stack offsets of the locals that are used.
            {
                ref Node n__prev1 = n;

                foreach (var (__i, __n) in fn.Dcl)
                {
                    i = __i;
                    n = __n;
                    if (n.Op != ONAME || n.Class() != PAUTO)
                    {
                        continue;
                    }
                    if (!n.Name.Used())
                    {
                        fn.Dcl = fn.Dcl[..i];
                        break;
                    }
                    dowidth(n.Type);
                    var w = n.Type.Width;
                    if (w >= thearch.MAXWIDTH || w < 0L)
                    {
                        Fatalf("bad width");
                    }
                    s.stksize += w;
                    s.stksize = Rnd(s.stksize, int64(n.Type.Align));
                    if (types.Haspointers(n.Type))
                    {
                        s.stkptrsize = s.stksize;
                    }
                    if (thearch.LinkArch.InFamily(sys.MIPS, sys.MIPS64, sys.ARM, sys.ARM64, sys.PPC64, sys.S390X))
                    {
                        s.stksize = Rnd(s.stksize, int64(Widthptr));
                    }
                    n.Xoffset = -s.stksize;
                }

                n = n__prev1;
            }

            s.stksize = Rnd(s.stksize, int64(Widthreg));
            s.stkptrsize = Rnd(s.stkptrsize, int64(Widthreg));
        }

        private static void compile(ref Node fn)
        {
            Curfn = fn;
            dowidth(fn.Type);

            if (fn.Nbody.Len() == 0L)
            {
                emitptrargsmap();
                return;
            }
            saveerrors();

            order(fn);
            if (nerrors != 0L)
            {
                return;
            }
            walk(fn);
            if (nerrors != 0L)
            {
                return;
            }
            if (instrumenting)
            {
                instrument(fn);
            } 

            // From this point, there should be no uses of Curfn. Enforce that.
            Curfn = null; 

            // Set up the function's LSym early to avoid data races with the assemblers.
            fn.Func.initLSym();

            if (compilenow())
            {
                compileSSA(fn, 0L);
            }
            else
            {
                compilequeue = append(compilequeue, fn);
            }
        }

        // compilenow reports whether to compile immediately.
        // If functions are not compiled immediately,
        // they are enqueued in compilequeue,
        // which is drained by compileFunctions.
        private static bool compilenow()
        {
            return nBackendWorkers == 1L && Debug_compilelater == 0L;
        }

        private static readonly long maxStackSize = 1L << (int)(30L);

        // compileSSA builds an SSA backend function,
        // uses it to generate a plist,
        // and flushes that plist to machine code.
        // worker indicates which of the backend workers is doing the processing.


        // compileSSA builds an SSA backend function,
        // uses it to generate a plist,
        // and flushes that plist to machine code.
        // worker indicates which of the backend workers is doing the processing.
        private static void compileSSA(ref Node fn, long worker)
        {
            var f = buildssa(fn, worker);
            if (f.Frontend()._<ref ssafn>().stksize >= maxStackSize)
            {
                largeStackFramesMu.Lock();
                largeStackFrames = append(largeStackFrames, fn.Pos);
                largeStackFramesMu.Unlock();
                return;
            }
            var pp = newProgs(fn, worker);
            genssa(f, pp);
            pp.Flush(); 
            // fieldtrack must be called after pp.Flush. See issue 20014.
            fieldtrack(pp.Text.From.Sym, fn.Func.FieldTrack);
            pp.Free();
        }

        private static void init()
        {
            if (raceEnabled)
            {
                rand.Seed(time.Now().UnixNano());
            }
        }

        // compileFunctions compiles all functions in compilequeue.
        // It fans out nBackendWorkers to do the work
        // and waits for them to complete.
        private static void compileFunctions()
        {
            if (len(compilequeue) != 0L)
            {
                sizeCalculationDisabled = true; // not safe to calculate sizes concurrently
                if (raceEnabled)
                { 
                    // Randomize compilation order to try to shake out races.
                    var tmp = make_slice<ref Node>(len(compilequeue));
                    var perm = rand.Perm(len(compilequeue));
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __v) in perm)
                        {
                            i = __i;
                            v = __v;
                            tmp[v] = compilequeue[i];
                        }
                else

                        i = i__prev1;
                    }

                    copy(compilequeue, tmp);
                }                { 
                    // Compile the longest functions first,
                    // since they're most likely to be the slowest.
                    // This helps avoid stragglers.
                    obj.SortSlice(compilequeue, (i, j) =>
                    {
                        return compilequeue[i].Nbody.Len() > compilequeue[j].Nbody.Len();
                    });
                }
                sync.WaitGroup wg = default;
                Ctxt.InParallel = true;
                var c = make_channel<ref Node>(nBackendWorkers);
                {
                    var i__prev1 = i;

                    for (long i = 0L; i < nBackendWorkers; i++)
                    {
                        wg.Add(1L);
                        go_(() => worker =>
                        {
                            {
                                var fn__prev2 = fn;

                                foreach (var (__fn) in c)
                                {
                                    fn = __fn;
                                    compileSSA(fn, worker);
                                }

                                fn = fn__prev2;
                            }

                            wg.Done();
                        }(i));
                    }


                    i = i__prev1;
                }
                {
                    var fn__prev1 = fn;

                    foreach (var (_, __fn) in compilequeue)
                    {
                        fn = __fn;
                        c.Send(fn);
                    }

                    fn = fn__prev1;
                }

                close(c);
                compilequeue = null;
                wg.Wait();
                Ctxt.InParallel = false;
                sizeCalculationDisabled = false;
            }
        }

        private static (slice<dwarf.Scope>, dwarf.InlCalls) debuginfo(ref obj.LSym fnsym, object curfn)
        {
            ref Node fn = curfn._<ref Node>();
            var debugInfo = fn.Func.DebugInfo;
            fn.Func.DebugInfo = null;
            if (fn.Func.Nname != null)
            {
                {
                    var expect = fn.Func.Nname.Sym.Linksym();

                    if (fnsym != expect)
                    {
                        Fatalf("unexpected fnsym: %v != %v", fnsym, expect);
                    }

                }
            }
            slice<ref Node> automDecls = default; 
            // Populate Automs for fn.
            foreach (var (_, n) in fn.Func.Dcl)
            {
                if (n.Op != ONAME)
                { // might be OTYPE or OLITERAL
                    continue;
                }
                obj.AddrName name = default;

                if (n.Class() == PAUTO) 
                    if (!n.Name.Used())
                    { 
                        // Text == nil -> generating abstract function
                        if (fnsym.Func.Text != null)
                        {
                            Fatalf("debuginfo unused node (AllocFrame should truncate fn.Func.Dcl)");
                        }
                        continue;
                    }
                    name = obj.NAME_AUTO;
                else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    name = obj.NAME_PARAM;
                else 
                    continue;
                                automDecls = append(automDecls, n);
                var gotype = ngotype(n).Linksym();
                fnsym.Func.Autom = append(fnsym.Func.Autom, ref new obj.Auto(Asym:Ctxt.Lookup(n.Sym.Name),Aoffset:int32(n.Xoffset),Name:name,Gotype:gotype,));
            }
            var (decls, dwarfVars) = createDwarfVars(fnsym, debugInfo, automDecls);

            slice<ScopeID> varScopes = default;
            foreach (var (_, decl) in decls)
            {
                var pos = decl.Pos;
                if (decl.Name.Defn != null && (decl.Name.Captured() || decl.Name.Byval()))
                { 
                    // It's not clear which position is correct for captured variables here:
                    // * decl.Pos is the wrong position for captured variables, in the inner
                    //   function, but it is the right position in the outer function.
                    // * decl.Name.Defn is nil for captured variables that were arguments
                    //   on the outer function, however the decl.Pos for those seems to be
                    //   correct.
                    // * decl.Name.Defn is the "wrong" thing for variables declared in the
                    //   header of a type switch, it's their position in the header, rather
                    //   than the position of the case statement. In principle this is the
                    //   right thing, but here we prefer the latter because it makes each
                    //   instance of the header variable local to the lexical block of its
                    //   case statement.
                    // This code is probably wrong for type switch variables that are also
                    // captured.
                    pos = decl.Name.Defn.Pos;
                }
                varScopes = append(varScopes, findScope(fn.Func.Marks, pos));
            }
            var scopes = assembleScopes(fnsym, fn, dwarfVars, varScopes);
            dwarf.InlCalls inlcalls = default;
            if (genDwarfInline > 0L)
            {
                inlcalls = assembleInlines(fnsym, fn, dwarfVars);
            }
            return (scopes, inlcalls);
        }

        // createSimpleVars creates a DWARF entry for every variable declared in the
        // function, claiming that they are permanently on the stack.
        private static (slice<ref Node>, slice<ref dwarf.Var>, map<ref Node, bool>) createSimpleVars(slice<ref Node> automDecls)
        {
            slice<ref dwarf.Var> vars = default;
            slice<ref Node> decls = default;
            var selected = make_map<ref Node, bool>();
            foreach (var (_, n) in automDecls)
            {
                if (n.IsAutoTmp())
                {
                    continue;
                }
                long abbrev = default;
                var offs = n.Xoffset;


                if (n.Class() == PAUTO) 
                    abbrev = dwarf.DW_ABRV_AUTO;
                    if (Ctxt.FixedFrameSize() == 0L)
                    {
                        offs -= int64(Widthptr);
                    }
                    if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                    {
                        offs -= int64(Widthptr);
                    }
                else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    abbrev = dwarf.DW_ABRV_PARAM;
                    offs += Ctxt.FixedFrameSize();
                else 
                    Fatalf("createSimpleVars unexpected type %v for node %v", n.Class(), n);
                                selected[n] = true;
                var typename = dwarf.InfoPrefix + typesymname(n.Type);
                decls = append(decls, n);
                long inlIndex = 0L;
                if (genDwarfInline > 1L)
                {
                    if (n.InlFormal() || n.InlLocal())
                    {
                        inlIndex = posInlIndex(n.Pos) + 1L;
                        if (n.InlFormal())
                        {
                            abbrev = dwarf.DW_ABRV_PARAM;
                        }
                    }
                }
                var declpos = Ctxt.InnermostPos(n.Pos);
                vars = append(vars, ref new dwarf.Var(Name:n.Sym.Name,IsReturnValue:n.Class()==PPARAMOUT,IsInlFormal:n.InlFormal(),Abbrev:abbrev,StackOffset:int32(offs),Type:Ctxt.Lookup(typename),DeclFile:declpos.Base().SymFilename(),DeclLine:declpos.Line(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,));
            }
            return (decls, vars, selected);
        }

        private partial struct varPart
        {
            public long varOffset;
            public ssa.SlotID slot;
        }

        private static (slice<ref Node>, slice<ref dwarf.Var>, map<ref Node, bool>) createComplexVars(ref obj.LSym fnsym, ref ssa.FuncDebug debugInfo, slice<ref Node> automDecls)
        {
            foreach (var (_, blockDebug) in debugInfo.Blocks)
            {
                foreach (var (_, locList) in blockDebug.Variables)
                {
                    foreach (var (_, loc) in locList.Locations)
                    {
                        if (loc.StartProg != null)
                        {
                            loc.StartPC = loc.StartProg.Pc;
                        }
                        if (loc.EndProg != null)
                        {
                            loc.EndPC = loc.EndProg.Pc;
                        }
                        else
                        {
                            loc.EndPC = fnsym.Size;
                        }
                        if (Debug_locationlist == 0L)
                        {
                            loc.EndProg = null;
                            loc.StartProg = null;
                        }
                    }
                }
            } 

            // Group SSA variables by the user variable they were decomposed from.
            map varParts = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, slice<varPart>>{};
            var ssaVars = make_map<ref Node, bool>();
            {
                var slot__prev1 = slot;

                foreach (var (__slotID, __slot) in debugInfo.VarSlots)
                {
                    slotID = __slotID;
                    slot = __slot;
                    while (slot.SplitOf != null)
                    {
                        slot = slot.SplitOf;
                    }

                    ref Node n = slot.N._<ref Node>();
                    ssaVars[n] = true;
                    varParts[n] = append(varParts[n], new varPart(varOffset(slot),ssa.SlotID(slotID)));
                } 

                // Produce a DWARF variable entry for each user variable.
                // Don't iterate over the map -- that's nondeterministic, and
                // createComplexVar has side effects. Instead, go by slot.

                slot = slot__prev1;
            }

            slice<ref Node> decls = default;
            slice<ref dwarf.Var> vars = default;
            {
                var slot__prev1 = slot;

                foreach (var (_, __slot) in debugInfo.VarSlots)
                {
                    slot = __slot;
                    while (slot.SplitOf != null)
                    {
                        slot = slot.SplitOf;
                    }

                    n = slot.N._<ref Node>();
                    var parts = varParts[n];
                    if (parts == null)
                    {
                        continue;
                    } 
                    // Don't work on this variable again, no matter how many slots it has.
                    delete(varParts, n); 

                    // Get the order the parts need to be in to represent the memory
                    // of the decomposed user variable.
                    sort.Sort(partsByVarOffset(parts));

                    {
                        var dvar = createComplexVar(debugInfo, n, parts);

                        if (dvar != null)
                        {
                            decls = append(decls, n);
                            vars = append(vars, dvar);
                        }

                    }
                }

                slot = slot__prev1;
            }

            return (decls, vars, ssaVars);
        }

        private static (slice<ref Node>, slice<ref dwarf.Var>) createDwarfVars(ref obj.LSym fnsym, ref ssa.FuncDebug debugInfo, slice<ref Node> automDecls)
        { 
            // Collect a raw list of DWARF vars.
            slice<ref dwarf.Var> vars = default;
            slice<ref Node> decls = default;
            map<ref Node, bool> selected = default;
            if (Ctxt.Flag_locationlists && Ctxt.Flag_optimize && debugInfo != null)
            {
                decls, vars, selected = createComplexVars(fnsym, debugInfo, automDecls);
            }
            else
            {
                decls, vars, selected = createSimpleVars(automDecls);
            }
            slice<ref Node> dcl = default;
            if (fnsym.WasInlined())
            {
                dcl = preInliningDcls(fnsym);
            }
            else
            {
                dcl = automDecls;
            } 

            // If optimization is enabled, the list above will typically be
            // missing some of the original pre-optimization variables in the
            // function (they may have been promoted to registers, folded into
            // constants, dead-coded away, etc). Here we add back in entries
            // for selected missing vars. Note that the recipe below creates a
            // conservative location. The idea here is that we want to
            // communicate to the user that "yes, there is a variable named X
            // in this function, but no, I don't have enough information to
            // reliably report its contents."
            foreach (var (_, n) in dcl)
            {
                {
                    var (_, found) = selected[n];

                    if (found)
                    {
                        continue;
                    }

                }
                var c = n.Sym.Name[0L];
                if (c == '.' || n.Type.IsUntyped())
                {
                    continue;
                }
                var typename = dwarf.InfoPrefix + typesymname(n.Type);
                decls = append(decls, n);
                var abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
                if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
                {
                    abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                }
                long inlIndex = 0L;
                if (genDwarfInline > 1L)
                {
                    if (n.InlFormal() || n.InlLocal())
                    {
                        inlIndex = posInlIndex(n.Pos) + 1L;
                        if (n.InlFormal())
                        {
                            abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                        }
                    }
                }
                var declpos = Ctxt.InnermostPos(n.Pos);
                vars = append(vars, ref new dwarf.Var(Name:n.Sym.Name,IsReturnValue:n.Class()==PPARAMOUT,Abbrev:abbrev,StackOffset:int32(n.Xoffset),Type:Ctxt.Lookup(typename),DeclFile:declpos.Base().SymFilename(),DeclLine:declpos.Line(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,)); 
                // Append a "deleted auto" entry to the autom list so as to
                // insure that the type in question is picked up by the linker.
                // See issue 22941.
                var gotype = ngotype(n).Linksym();
                fnsym.Func.Autom = append(fnsym.Func.Autom, ref new obj.Auto(Asym:Ctxt.Lookup(n.Sym.Name),Aoffset:int32(-1),Name:obj.NAME_DELETED_AUTO,Gotype:gotype,));

            }
            return (decls, vars);
        }

        // Given a function that was inlined at some point during the
        // compilation, return a sorted list of nodes corresponding to the
        // autos/locals in that function prior to inlining. If this is a
        // function that is not local to the package being compiled, then the
        // names of the variables may have been "versioned" to avoid conflicts
        // with local vars; disregard this versioning when sorting.
        private static slice<ref Node> preInliningDcls(ref obj.LSym fnsym)
        {
            ref Node fn = Ctxt.DwFixups.GetPrecursorFunc(fnsym)._<ref Node>();
            slice<ref Node> dcl = default;            slice<ref Node> rdcl = default;

            if (fn.Name.Defn != null)
            {
                dcl = fn.Func.Inldcl.Slice(); // local function
            }
            else
            {
                dcl = fn.Func.Dcl; // imported function
            }
            foreach (var (_, n) in dcl)
            {
                var c = n.Sym.Name[0L]; 
                // Avoid reporting "_" parameters, since if there are more than
                // one, it can result in a collision later on, as in #23179.
                if (unversion(n.Sym.Name) == "_" || c == '.' || n.Type.IsUntyped())
                {
                    continue;
                }
                rdcl = append(rdcl, n);
            }
            sort.Sort(byNodeName(rdcl));
            return rdcl;
        }

        private static bool cmpNodeName(ref Node a, ref Node b)
        {
            long aart = 0L;
            if (strings.HasPrefix(a.Sym.Name, "~"))
            {
                aart = 1L;
            }
            long bart = 0L;
            if (strings.HasPrefix(b.Sym.Name, "~"))
            {
                bart = 1L;
            }
            if (aart != bart)
            {
                return aart < bart;
            }
            var aname = unversion(a.Sym.Name);
            var bname = unversion(b.Sym.Name);
            return aname < bname;
        }

        // byNodeName implements sort.Interface for []*Node using cmpNodeName.
        private partial struct byNodeName // : slice<ref Node>
        {
        }

        private static long Len(this byNodeName s)
        {
            return len(s);
        }
        private static bool Less(this byNodeName s, long i, long j)
        {
            return cmpNodeName(s[i], s[j]);
        }
        private static void Swap(this byNodeName s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        // varOffset returns the offset of slot within the user variable it was
        // decomposed from. This has nothing to do with its stack offset.
        private static long varOffset(ref ssa.LocalSlot slot)
        {
            var offset = slot.Off;
            while (slot.SplitOf != null)
            {
                offset += slot.SplitOffset;
                slot = slot.SplitOf;
            }

            return offset;
        }

        private partial struct partsByVarOffset // : slice<varPart>
        {
        }

        private static long Len(this partsByVarOffset a)
        {
            return len(a);
        }
        private static bool Less(this partsByVarOffset a, long i, long j)
        {
            return a[i].varOffset < a[j].varOffset;
        }
        private static void Swap(this partsByVarOffset a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        // stackOffset returns the stack location of a LocalSlot relative to the
        // stack pointer, suitable for use in a DWARF location entry. This has nothing
        // to do with its offset in the user variable.
        private static int stackOffset(ref ssa.LocalSlot slot)
        {
            ref Node n = slot.N._<ref Node>();
            long @base = default;

            if (n.Class() == PAUTO) 
                if (Ctxt.FixedFrameSize() == 0L)
                {
                    base -= int64(Widthptr);
                }
                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                {
                    base -= int64(Widthptr);
                }
            else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                base += Ctxt.FixedFrameSize();
                        return int32(base + n.Xoffset + slot.Off);
        }

        // createComplexVar builds a DWARF variable entry and location list representing n.
        private static ref dwarf.Var createComplexVar(ref ssa.FuncDebug debugInfo, ref Node n, slice<varPart> parts)
        {
            var slots = debugInfo.Slots;
            long offs = default; // base stack offset for this kind of variable
            long abbrev = default;

            if (n.Class() == PAUTO) 
                abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
                if (Ctxt.FixedFrameSize() == 0L)
                {
                    offs -= int64(Widthptr);
                }
                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH))
                {
                    offs -= int64(Widthptr);
                }
            else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                offs += Ctxt.FixedFrameSize();
            else 
                return null;
                        var gotype = ngotype(n).Linksym();
            var typename = dwarf.InfoPrefix + gotype.Name[len("type.")..];
            long inlIndex = 0L;
            if (genDwarfInline > 1L)
            {
                if (n.InlFormal() || n.InlLocal())
                {
                    inlIndex = posInlIndex(n.Pos) + 1L;
                    if (n.InlFormal())
                    {
                        abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                    }
                }
            }
            var declpos = Ctxt.InnermostPos(n.Pos);
            dwarf.Var dvar = ref new dwarf.Var(Name:n.Sym.Name,IsReturnValue:n.Class()==PPARAMOUT,IsInlFormal:n.InlFormal(),Abbrev:abbrev,Type:Ctxt.Lookup(typename),StackOffset:int32(stackOffset(slots[parts[0].slot])),DeclFile:declpos.Base().SymFilename(),DeclLine:declpos.Line(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,);

            if (Debug_locationlist != 0L)
            {
                Ctxt.Logf("Building location list for %+v. Parts:\n", n);
                {
                    var part__prev1 = part;

                    foreach (var (_, __part) in parts)
                    {
                        part = __part;
                        Ctxt.Logf("\t%v => %v\n", debugInfo.Slots[part.slot], debugInfo.SlotLocsString(part.slot));
                    }

                    part = part__prev1;
                }

            } 

            // Given a variable that's been decomposed into multiple parts,
            // its location list may need a new entry after the beginning or
            // end of every location entry for each of its parts. For example:
            //
            // [variable]    [pc range]
            // string.ptr    |----|-----|    |----|
            // string.len    |------------|  |--|
            // ... needs a location list like:
            // string        |----|-----|-|  |--|-|
            //
            // Note that location entries may or may not line up with each other,
            // and some of the result will only have one or the other part.
            //
            // To build the resulting list:
            // - keep a "current" pointer for each part
            // - find the next transition point
            // - advance the current pointer for each part up to that transition point
            // - build the piece for the range between that transition point and the next
            // - repeat
            private partial struct locID
            {
                public long block;
                public long loc;
            }
            Func<varPart, locID, ref ssa.VarLoc> findLoc = (part, id) =>
            {
                if (id.block >= len(debugInfo.Blocks))
                {
                    return null;
                }
                return debugInfo.Blocks[id.block].Variables[part.slot].Locations[id.loc];
            }
;
            Func<varPart, locID, (locID, ref ssa.VarLoc)> nextLoc = (part, id) =>
            { 
                // Check if there's another loc in this block
                id.loc++;
                {
                    var b__prev1 = b;

                    var b = debugInfo.Blocks[id.block];

                    if (b != null && id.loc < len(b.Variables[part.slot].Locations))
                    {
                        return (id, findLoc(part, id));
                    } 
                    // Find the next block that has a loc for this part.

                    b = b__prev1;

                } 
                // Find the next block that has a loc for this part.
                id.loc = 0L;
                id.block++;
                while (id.block < len(debugInfo.Blocks))
                {
                    {
                        var b__prev1 = b;

                        b = debugInfo.Blocks[id.block];

                        if (b != null && len(b.Variables[part.slot].Locations) != 0L)
                        {
                            return (id, findLoc(part, id));
                    id.block++;
                        }

                        b = b__prev1;

                    }
                }

                return (id, null);
            }
;
            var curLoc = make_slice<locID>(len(slots)); 
            // Position each pointer at the first entry for its slot.
            {
                var part__prev1 = part;

                foreach (var (_, __part) in parts)
                {
                    part = __part;
                    {
                        var b__prev1 = b;

                        b = debugInfo.Blocks[0L];

                        if (b != null && len(b.Variables[part.slot].Locations) != 0L)
                        { 
                            // Block 0 has an entry; no need to advance.
                            continue;
                        }

                        b = b__prev1;

                    }
                    curLoc[part.slot], _ = nextLoc(part, curLoc[part.slot]);
                } 

                // findBoundaryAfter finds the next beginning or end of a piece after currentPC.

                part = part__prev1;
            }

            Func<long, long> findBoundaryAfter = currentPC =>
            {
                var min = int64(math.MaxInt64);
                {
                    var part__prev1 = part;

                    foreach (var (_, __part) in parts)
                    {
                        part = __part; 
                        // For each part, find the first PC greater than current. Doesn't
                        // matter if it's a start or an end, since we're looking for any boundary.
                        // If it's the new winner, save it.
onePart:
                        {
                            var i__prev2 = i;
                            var loc__prev2 = loc;

                            var i = curLoc[part.slot];
                            var loc = findLoc(part, curLoc[part.slot]);

                            while (loc != null)
                            {
                                foreach (var (_, pc) in new array<long>(new long[] { loc.StartPC, loc.EndPC }))
                                {
                                    if (pc > currentPC)
                                    {
                                        if (pc < min)
                                        {
                                            min = pc;
                                        }
                                        _breakonePart = true;
                                        break;
                                i, loc = nextLoc(part, i);
                                    }
                                }
                            }


                            i = i__prev2;
                            loc = loc__prev2;
                        }
                    }

                    part = part__prev1;
                }

                return min;
            }
;
            long start = default;
            var end = findBoundaryAfter(0L);
            while (true)
            { 
                // Advance to the next chunk.
                start = end;
                end = findBoundaryAfter(start);
                if (end == math.MaxInt64)
                {
                    break;
                }
                dwarf.Location dloc = new dwarf.Location(StartPC:start,EndPC:end);
                if (Debug_locationlist != 0L)
                {
                    Ctxt.Logf("Processing range %x -> %x\n", start, end);
                } 

                // Advance curLoc to the last location that starts before/at start.
                // After this loop, if there's a location that covers [start, end), it will be current.
                // Otherwise the current piece will be too early.
                {
                    var part__prev2 = part;

                    foreach (var (_, __part) in parts)
                    {
                        part = __part;
                        locID choice = new locID(-1,-1);
                        {
                            var i__prev3 = i;
                            var loc__prev3 = loc;

                            i = curLoc[part.slot];
                            loc = findLoc(part, curLoc[part.slot]);

                            while (loc != null)
                            {
                                if (loc.StartPC > start)
                                {
                                    break; //overshot
                                i, loc = nextLoc(part, i);
                                }
                                choice = i; // best yet
                            }


                            i = i__prev3;
                            loc = loc__prev3;
                        }
                        if (choice.block != -1L)
                        {
                            curLoc[part.slot] = choice;
                        }
                        if (Debug_locationlist != 0L)
                        {
                            Ctxt.Logf("\t %v => %v", slots[part.slot], curLoc[part.slot]);
                        }
                    }

                    part = part__prev2;
                }

                if (Debug_locationlist != 0L)
                {
                    Ctxt.Logf("\n");
                } 
                // Assemble the location list entry for this chunk.
                long present = 0L;
                {
                    var part__prev2 = part;

                    foreach (var (_, __part) in parts)
                    {
                        part = __part;
                        dwarf.Piece dpiece = new dwarf.Piece(Length:slots[part.slot].Type.Size(),);
                        loc = findLoc(part, curLoc[part.slot]);
                        if (loc == null || start >= loc.EndPC || end <= loc.StartPC)
                        {
                            if (Debug_locationlist != 0L)
                            {
                                Ctxt.Logf("\t%v: missing", slots[part.slot]);
                            }
                            dpiece.Missing = true;
                            dloc.Pieces = append(dloc.Pieces, dpiece);
                            continue;
                        }
                        present++;
                        if (Debug_locationlist != 0L)
                        {
                            Ctxt.Logf("\t%v: %v", slots[part.slot], debugInfo.Blocks[curLoc[part.slot].block].LocString(loc));
                        }
                        if (loc.OnStack)
                        {
                            dpiece.OnStack = true;
                            dpiece.StackOffset = stackOffset(slots[loc.StackLocation]);
                        }
                        else
                        {
                            for (long reg = 0L; reg < len(debugInfo.Registers); reg++)
                            {
                                if (loc.Registers & (1L << (int)(uint8(reg))) != 0L)
                                {
                                    dpiece.RegNum = Ctxt.Arch.DWARFRegisters[debugInfo.Registers[reg].ObjNum()];
                                }
                            }

                        }
                        dloc.Pieces = append(dloc.Pieces, dpiece);
                    }

                    part = part__prev2;
                }

                if (present == 0L)
                {
                    if (Debug_locationlist != 0L)
                    {
                        Ctxt.Logf(" -> totally missing\n");
                    }
                    continue;
                } 
                // Extend the previous entry if possible.
                if (len(dvar.LocationList) > 0L)
                {
                    var prev = ref dvar.LocationList[len(dvar.LocationList) - 1L];
                    if (prev.EndPC == dloc.StartPC && len(prev.Pieces) == len(dloc.Pieces))
                    {
                        var equal = true;
                        {
                            var i__prev2 = i;

                            foreach (var (__i) in prev.Pieces)
                            {
                                i = __i;
                                if (prev.Pieces[i] != dloc.Pieces[i])
                                {
                                    equal = false;
                                }
                            }

                            i = i__prev2;
                        }

                        if (equal)
                        {
                            prev.EndPC = end;
                            if (Debug_locationlist != 0L)
                            {
                                Ctxt.Logf("-> merged with previous, now %#v\n", prev);
                            }
                            continue;
                        }
                    }
                }
                dvar.LocationList = append(dvar.LocationList, dloc);
                if (Debug_locationlist != 0L)
                {
                    Ctxt.Logf("-> added: %#v\n", dloc);
                }
            }

            return dvar;
        }

        // fieldtrack adds R_USEFIELD relocations to fnsym to record any
        // struct fields that it used.
        private static void fieldtrack(ref obj.LSym fnsym, object tracked)
        {
            if (fnsym == null)
            {
                return;
            }
            if (objabi.Fieldtrack_enabled == 0L || len(tracked) == 0L)
            {
                return;
            }
            var trackSyms = make_slice<ref types.Sym>(0L, len(tracked));
            {
                var sym__prev1 = sym;

                foreach (var (__sym) in tracked)
                {
                    sym = __sym;
                    trackSyms = append(trackSyms, sym);
                }

                sym = sym__prev1;
            }

            sort.Sort(symByName(trackSyms));
            {
                var sym__prev1 = sym;

                foreach (var (_, __sym) in trackSyms)
                {
                    sym = __sym;
                    var r = obj.Addrel(fnsym);
                    r.Sym = sym.Linksym();
                    r.Type = objabi.R_USEFIELD;
                }

                sym = sym__prev1;
            }

        }

        private partial struct symByName // : slice<ref types.Sym>
        {
        }

        private static long Len(this symByName a)
        {
            return len(a);
        }
        private static bool Less(this symByName a, long i, long j)
        {
            return a[i].Name < a[j].Name;
        }
        private static void Swap(this symByName a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }
    }
}}}}
