// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:29:52 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\pgen.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using race = go.@internal.race_package;
using rand = go.math.rand_package;
using sort = go.sort_package;
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
        private static long nBackendWorkers = default;        private static slice<ptr<Node>> compilequeue = default;

        private static void emitptrargsmap(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.funcname() == "_" || fn.Func.Nname.Sym.Linkname != "")
            {
                return ;
            }

            var lsym = Ctxt.Lookup(fn.Func.lsym.Name + ".args_stackmap");

            var nptr = int(fn.Type.ArgWidth() / int64(Widthptr));
            var bv = bvalloc(int32(nptr) * 2L);
            long nbitmap = 1L;
            if (fn.Type.NumResults() > 0L)
            {
                nbitmap = 2L;
            }

            var off = duint32(lsym, 0L, uint32(nbitmap));
            off = duint32(lsym, off, uint32(bv.n));

            if (fn.IsMethod())
            {
                onebitwalktype1(fn.Type.Recvs(), 0L, bv);
            }

            if (fn.Type.NumParams() > 0L)
            {
                onebitwalktype1(fn.Type.Params(), 0L, bv);
            }

            off = dbvec(lsym, off, bv);

            if (fn.Type.NumResults() > 0L)
            {
                onebitwalktype1(fn.Type.Results(), 0L, bv);
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
        private static bool cmpstackvarlt(ptr<Node> _addr_a, ptr<Node> _addr_b)
        {
            ref Node a = ref _addr_a.val;
            ref Node b = ref _addr_b.val;

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
        private partial struct byStackVar // : slice<ptr<Node>>
        {
        }

        private static long Len(this byStackVar s)
        {
            return len(s);
        }
        private static bool Less(this byStackVar s, long i, long j)
        {
            return cmpstackvarlt(_addr_s[i], _addr_s[j]);
        }
        private static void Swap(this byStackVar s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }

        private static void AllocFrame(this ptr<ssafn> _addr_s, ptr<ssa.Func> _addr_f)
        {
            ref ssafn s = ref _addr_s.val;
            ref ssa.Func f = ref _addr_f.val;

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
                        ls.N._<ptr<Node>>().Name.SetUsed(true);
                    }

                }

            }
            var scratchUsed = false;
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    {
                        ptr<Node> n__prev1 = n;

                        ptr<Node> (n, ok) = v.Aux._<ptr<Node>>();

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
            var lastHasPtr = false;
            {
                ptr<Node> n__prev1 = n;

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

                    if (w == 0L && lastHasPtr)
                    { 
                        // Pad between a pointer-containing object and a zero-sized object.
                        // This prevents a pointer to the zero-sized object from being interpreted
                        // as a pointer to the pointer-containing object (and causing it
                        // to be scanned when it shouldn't be). See issue 24993.
                        w = 1L;

                    }

                    s.stksize += w;
                    s.stksize = Rnd(s.stksize, int64(n.Type.Align));
                    if (types.Haspointers(n.Type))
                    {
                        s.stkptrsize = s.stksize;
                        lastHasPtr = true;
                    }
                    else
                    {
                        lastHasPtr = false;
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

        private static void funccompile(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (Curfn != null)
            {
                Fatalf("funccompile %v inside %v", fn.Func.Nname.Sym, Curfn.Func.Nname.Sym);
            }

            if (fn.Type == null)
            {
                if (nerrors == 0L)
                {
                    Fatalf("funccompile missing type");
                }

                return ;

            } 

            // assign parameter offsets
            dowidth(fn.Type);

            if (fn.Nbody.Len() == 0L)
            { 
                // Initialize ABI wrappers if necessary.
                fn.Func.initLSym(false);
                emitptrargsmap(_addr_fn);
                return ;

            }

            dclcontext = PAUTO;
            Curfn = fn;

            compile(_addr_fn);

            Curfn = null;
            dclcontext = PEXTERN;

        }

        private static void compile(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            saveerrors();

            order(fn);
            if (nerrors != 0L)
            {
                return ;
            }

            walk(fn);
            if (nerrors != 0L)
            {
                return ;
            }

            if (instrumenting)
            {
                instrument(fn);
            } 

            // From this point, there should be no uses of Curfn. Enforce that.
            Curfn = null;

            if (fn.funcname() == "_")
            { 
                // We don't need to generate code for this function, just report errors in its body.
                // At this point we've generated any errors needed.
                // (Beyond here we generate only non-spec errors, like "stack frame too large".)
                // See issue 29870.
                return ;

            } 

            // Set up the function's LSym early to avoid data races with the assemblers.
            fn.Func.initLSym(true); 

            // Make sure type syms are declared for all types that might
            // be types of stack objects. We need to do this here
            // because symbols must be allocated before the parallel
            // phase of the compiler.
            foreach (var (_, n) in fn.Func.Dcl)
            {

                if (n.Class() == PPARAM || n.Class() == PPARAMOUT || n.Class() == PAUTO) 
                    if (livenessShouldTrack(n) && n.Name.Addrtaken())
                    {
                        dtypesym(n.Type); 
                        // Also make sure we allocate a linker symbol
                        // for the stack object data, for the same reason.
                        if (fn.Func.lsym.Func.StackObjects == null)
                        {
                            fn.Func.lsym.Func.StackObjects = Ctxt.Lookup(fn.Func.lsym.Name + ".stkobj");
                        }

                    }

                            }
            if (compilenow(_addr_fn))
            {
                compileSSA(_addr_fn, 0L);
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
        private static bool compilenow(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;
 
            // Issue 38068: if this function is a method AND an inline
            // candidate AND was not inlined (yet), put it onto the compile
            // queue instead of compiling it immediately. This is in case we
            // wind up inlining it into a method wrapper that is generated by
            // compiling a function later on in the xtop list.
            if (fn.IsMethod() && isInlinableButNotInlined(_addr_fn))
            {
                return false;
            }

            return nBackendWorkers == 1L && Debug_compilelater == 0L;

        }

        // isInlinableButNotInlined returns true if 'fn' was marked as an
        // inline candidate but then never inlined (presumably because we
        // found no call sites).
        private static bool isInlinableButNotInlined(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Func.Nname.Func.Inl == null)
            {
                return false;
            }

            if (fn.Sym == null)
            {
                return true;
            }

            return !fn.Sym.Linksym().WasInlined();

        }

        private static readonly long maxStackSize = (long)1L << (int)(30L);

        // compileSSA builds an SSA backend function,
        // uses it to generate a plist,
        // and flushes that plist to machine code.
        // worker indicates which of the backend workers is doing the processing.


        // compileSSA builds an SSA backend function,
        // uses it to generate a plist,
        // and flushes that plist to machine code.
        // worker indicates which of the backend workers is doing the processing.
        private static void compileSSA(ptr<Node> _addr_fn, long worker) => func((defer, _, __) =>
        {
            ref Node fn = ref _addr_fn.val;

            var f = buildssa(fn, worker); 
            // Note: check arg size to fix issue 25507.
            if (f.Frontend()._<ptr<ssafn>>().stksize >= maxStackSize || fn.Type.ArgWidth() >= maxStackSize)
            {
                largeStackFramesMu.Lock();
                largeStackFrames = append(largeStackFrames, new largeStack(locals:f.Frontend().(*ssafn).stksize,args:fn.Type.ArgWidth(),pos:fn.Pos));
                largeStackFramesMu.Unlock();
                return ;
            }

            var pp = newProgs(fn, worker);
            defer(pp.Free());
            genssa(f, pp); 
            // Check frame size again.
            // The check above included only the space needed for local variables.
            // After genssa, the space needed includes local variables and the callee arg region.
            // We must do this check prior to calling pp.Flush.
            // If there are any oversized stack frames,
            // the assembler may emit inscrutable complaints about invalid instructions.
            if (pp.Text.To.Offset >= maxStackSize)
            {
                largeStackFramesMu.Lock();
                ptr<ssafn> locals = f.Frontend()._<ptr<ssafn>>().stksize;
                largeStackFrames = append(largeStackFrames, new largeStack(locals:locals,args:fn.Type.ArgWidth(),callee:pp.Text.To.Offset-locals,pos:fn.Pos));
                largeStackFramesMu.Unlock();
                return ;
            }

            pp.Flush(); // assemble, fill in boilerplate, etc.
            // fieldtrack must be called after pp.Flush. See issue 20014.
            fieldtrack(_addr_pp.Text.From.Sym, fn.Func.FieldTrack);

        });

        private static void init()
        {
            if (race.Enabled)
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
                if (race.Enabled)
                { 
                    // Randomize compilation order to try to shake out races.
                    var tmp = make_slice<ptr<Node>>(len(compilequeue));
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
                    sort.Slice(compilequeue, (i, j) =>
                    {
                        return compilequeue[i].Nbody.Len() > compilequeue[j].Nbody.Len();
                    });

                }

                sync.WaitGroup wg = default;
                Ctxt.InParallel = true;
                var c = make_channel<ptr<Node>>(nBackendWorkers);
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
                                    compileSSA(_addr_fn, worker);
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

        private static (slice<dwarf.Scope>, dwarf.InlCalls) debuginfo(ptr<obj.LSym> _addr_fnsym, ptr<obj.LSym> _addr_infosym, object curfn)
        {
            slice<dwarf.Scope> _p0 = default;
            dwarf.InlCalls _p0 = default;
            ref obj.LSym fnsym = ref _addr_fnsym.val;
            ref obj.LSym infosym = ref _addr_infosym.val;

            ptr<Node> fn = curfn._<ptr<Node>>();
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

            slice<ptr<Node>> apdecls = default; 
            // Populate decls for fn.
            foreach (var (_, n) in fn.Func.Dcl)
            {
                if (n.Op != ONAME)
                { // might be OTYPE or OLITERAL
                    continue;

                }


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

                else if (n.Class() == PPARAM || n.Class() == PPARAMOUT)                 else 
                    continue;
                                apdecls = append(apdecls, n);
                fnsym.Func.RecordAutoType(ngotype(n).Linksym());

            }
            var (decls, dwarfVars) = createDwarfVars(_addr_fnsym, _addr_fn.Func, apdecls); 

            // For each type referenced by the functions auto vars, attach a
            // dummy relocation to the function symbol to insure that the type
            // included in DWARF processing during linking.
            ptr<obj.LSym> typesyms = new slice<ptr<obj.LSym>>(new ptr<obj.LSym>[] {  });
            foreach (var (t, _) in fnsym.Func.Autot)
            {
                typesyms = append(typesyms, t);
            }
            sort.Sort(obj.BySymName(typesyms));
            foreach (var (_, sym) in typesyms)
            {
                var r = obj.Addrel(infosym);
                r.Sym = sym;
                r.Type = objabi.R_USETYPE;
            }
            fnsym.Func.Autot = null;

            slice<ScopeID> varScopes = default;
            foreach (var (_, decl) in decls)
            {
                var pos = declPos(_addr_decl);
                varScopes = append(varScopes, findScope(fn.Func.Marks, pos));
            }
            var scopes = assembleScopes(fnsym, fn, dwarfVars, varScopes);
            dwarf.InlCalls inlcalls = default;
            if (genDwarfInline > 0L)
            {
                inlcalls = assembleInlines(fnsym, dwarfVars);
            }

            return (scopes, inlcalls);

        }

        private static src.XPos declPos(ptr<Node> _addr_decl)
        {
            ref Node decl = ref _addr_decl.val;

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
                return decl.Name.Defn.Pos;

            }

            return decl.Pos;

        }

        // createSimpleVars creates a DWARF entry for every variable declared in the
        // function, claiming that they are permanently on the stack.
        private static (slice<ptr<Node>>, slice<ptr<dwarf.Var>>, map<ptr<Node>, bool>) createSimpleVars(slice<ptr<Node>> apDecls)
        {
            slice<ptr<Node>> _p0 = default;
            slice<ptr<dwarf.Var>> _p0 = default;
            map<ptr<Node>, bool> _p0 = default;

            slice<ptr<dwarf.Var>> vars = default;
            slice<ptr<Node>> decls = default;
            var selected = make_map<ptr<Node>, bool>();
            foreach (var (_, n) in apDecls)
            {
                if (n.IsAutoTmp())
                {
                    continue;
                }

                decls = append(decls, n);
                vars = append(vars, createSimpleVar(_addr_n));
                selected[n] = true;

            }
            return (decls, vars, selected);

        }

        private static ptr<dwarf.Var> createSimpleVar(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            long abbrev = default;
            var offs = n.Xoffset;


            if (n.Class() == PAUTO) 
                abbrev = dwarf.DW_ABRV_AUTO;
                if (Ctxt.FixedFrameSize() == 0L)
                {
                    offs -= int64(Widthptr);
                }

                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH) || objabi.GOARCH == "arm64")
                { 
                    // There is a word space for FP on ARM64 even if the frame pointer is disabled
                    offs -= int64(Widthptr);

                }

            else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                abbrev = dwarf.DW_ABRV_PARAM;
                offs += Ctxt.FixedFrameSize();
            else 
                Fatalf("createSimpleVar unexpected class %v for node %v", n.Class(), n);
                        var typename = dwarf.InfoPrefix + typesymname(n.Type);
            long inlIndex = 0L;
            if (genDwarfInline > 1L)
            {
                if (n.Name.InlFormal() || n.Name.InlLocal())
                {
                    inlIndex = posInlIndex(n.Pos) + 1L;
                    if (n.Name.InlFormal())
                    {
                        abbrev = dwarf.DW_ABRV_PARAM;
                    }

                }

            }

            var declpos = Ctxt.InnermostPos(declPos(_addr_n));
            return addr(new dwarf.Var(Name:n.Sym.Name,IsReturnValue:n.Class()==PPARAMOUT,IsInlFormal:n.Name.InlFormal(),Abbrev:abbrev,StackOffset:int32(offs),Type:Ctxt.Lookup(typename),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,));

        }

        // createComplexVars creates recomposed DWARF vars with location lists,
        // suitable for describing optimized code.
        private static (slice<ptr<Node>>, slice<ptr<dwarf.Var>>, map<ptr<Node>, bool>) createComplexVars(ptr<Func> _addr_fn)
        {
            slice<ptr<Node>> _p0 = default;
            slice<ptr<dwarf.Var>> _p0 = default;
            map<ptr<Node>, bool> _p0 = default;
            ref Func fn = ref _addr_fn.val;

            var debugInfo = fn.DebugInfo; 

            // Produce a DWARF variable entry for each user variable.
            slice<ptr<Node>> decls = default;
            slice<ptr<dwarf.Var>> vars = default;
            var ssaVars = make_map<ptr<Node>, bool>();

            {
                var dvar__prev1 = dvar;

                foreach (var (__varID, __dvar) in debugInfo.Vars)
                {
                    varID = __varID;
                    dvar = __dvar;
                    ptr<Node> n = dvar._<ptr<Node>>();
                    ssaVars[n] = true;
                    foreach (var (_, slot) in debugInfo.VarSlots[varID])
                    {
                        ssaVars[debugInfo.Slots[slot].N._<ptr<Node>>()] = true;
                    }
                    {
                        var dvar__prev1 = dvar;

                        var dvar = createComplexVar(_addr_fn, ssa.VarID(varID));

                        if (dvar != null)
                        {
                            decls = append(decls, n);
                            vars = append(vars, dvar);
                        }

                        dvar = dvar__prev1;

                    }

                }

                dvar = dvar__prev1;
            }

            return (decls, vars, ssaVars);

        }

        // createDwarfVars process fn, returning a list of DWARF variables and the
        // Nodes they represent.
        private static (slice<ptr<Node>>, slice<ptr<dwarf.Var>>) createDwarfVars(ptr<obj.LSym> _addr_fnsym, ptr<Func> _addr_fn, slice<ptr<Node>> apDecls)
        {
            slice<ptr<Node>> _p0 = default;
            slice<ptr<dwarf.Var>> _p0 = default;
            ref obj.LSym fnsym = ref _addr_fnsym.val;
            ref Func fn = ref _addr_fn.val;
 
            // Collect a raw list of DWARF vars.
            slice<ptr<dwarf.Var>> vars = default;
            slice<ptr<Node>> decls = default;
            map<ptr<Node>, bool> selected = default;
            if (Ctxt.Flag_locationlists && Ctxt.Flag_optimize && fn.DebugInfo != null)
            {
                decls, vars, selected = createComplexVars(_addr_fn);
            }
            else
            {
                decls, vars, selected = createSimpleVars(apDecls);
            }

            var dcl = apDecls;
            if (fnsym.WasInlined())
            {
                dcl = preInliningDcls(_addr_fnsym);
            } 

            // If optimization is enabled, the list above will typically be
            // missing some of the original pre-optimization variables in the
            // function (they may have been promoted to registers, folded into
            // constants, dead-coded away, etc).  Input arguments not eligible
            // for SSA optimization are also missing.  Here we add back in entries
            // for selected missing vars. Note that the recipe below creates a
            // conservative location. The idea here is that we want to
            // communicate to the user that "yes, there is a variable named X
            // in this function, but no, I don't have enough information to
            // reliably report its contents."
            // For non-SSA-able arguments, however, the correct information
            // is known -- they have a single home on the stack.
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

                if (n.Class() == PPARAM && !canSSAType(n.Type))
                { 
                    // SSA-able args get location lists, and may move in and
                    // out of registers, so those are handled elsewhere.
                    // Autos and named output params seem to get handled
                    // with VARDEF, which creates location lists.
                    // Args not of SSA-able type are treated here; they
                    // are homed on the stack in a single place for the
                    // entire call.
                    vars = append(vars, createSimpleVar(_addr_n));
                    decls = append(decls, n);
                    continue;

                }

                var typename = dwarf.InfoPrefix + typesymname(n.Type);
                decls = append(decls, n);
                var abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
                var isReturnValue = (n.Class() == PPARAMOUT);
                if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
                {
                    abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                }
                else if (n.Class() == PAUTOHEAP)
                { 
                    // If dcl in question has been promoted to heap, do a bit
                    // of extra work to recover original class (auto or param);
                    // see issue 30908. This insures that we get the proper
                    // signature in the abstract function DIE, but leaves a
                    // misleading location for the param (we want pointer-to-heap
                    // and not stack).
                    // TODO(thanm): generate a better location expression
                    var stackcopy = n.Name.Param.Stackcopy;
                    if (stackcopy != null && (stackcopy.Class() == PPARAM || stackcopy.Class() == PPARAMOUT))
                    {
                        abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                        isReturnValue = (stackcopy.Class() == PPARAMOUT);
                    }

                }

                long inlIndex = 0L;
                if (genDwarfInline > 1L)
                {
                    if (n.Name.InlFormal() || n.Name.InlLocal())
                    {
                        inlIndex = posInlIndex(n.Pos) + 1L;
                        if (n.Name.InlFormal())
                        {
                            abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                        }

                    }

                }

                var declpos = Ctxt.InnermostPos(n.Pos);
                vars = append(vars, addr(new dwarf.Var(Name:n.Sym.Name,IsReturnValue:isReturnValue,Abbrev:abbrev,StackOffset:int32(n.Xoffset),Type:Ctxt.Lookup(typename),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,))); 
                // Record go type of to insure that it gets emitted by the linker.
                fnsym.Func.RecordAutoType(ngotype(n).Linksym());

            }
            return (decls, vars);

        }

        // Given a function that was inlined at some point during the
        // compilation, return a sorted list of nodes corresponding to the
        // autos/locals in that function prior to inlining. If this is a
        // function that is not local to the package being compiled, then the
        // names of the variables may have been "versioned" to avoid conflicts
        // with local vars; disregard this versioning when sorting.
        private static slice<ptr<Node>> preInliningDcls(ptr<obj.LSym> _addr_fnsym)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;

            ptr<Node> fn = Ctxt.DwFixups.GetPrecursorFunc(fnsym)._<ptr<Node>>();
            slice<ptr<Node>> rdcl = default;
            foreach (var (_, n) in fn.Func.Inl.Dcl)
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
            return rdcl;

        }

        // stackOffset returns the stack location of a LocalSlot relative to the
        // stack pointer, suitable for use in a DWARF location entry. This has nothing
        // to do with its offset in the user variable.
        private static int stackOffset(ssa.LocalSlot slot)
        {
            ptr<Node> n = slot.N._<ptr<Node>>();
            long @base = default;

            if (n.Class() == PAUTO) 
                if (Ctxt.FixedFrameSize() == 0L)
                {
                    base -= int64(Widthptr);
                }

                if (objabi.Framepointer_enabled(objabi.GOOS, objabi.GOARCH) || objabi.GOARCH == "arm64")
                { 
                    // There is a word space for FP on ARM64 even if the frame pointer is disabled
                    base -= int64(Widthptr);

                }

            else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                base += Ctxt.FixedFrameSize();
                        return int32(base + n.Xoffset + slot.Off);

        }

        // createComplexVar builds a single DWARF variable entry and location list.
        private static ptr<dwarf.Var> createComplexVar(ptr<Func> _addr_fn, ssa.VarID varID)
        {
            ref Func fn = ref _addr_fn.val;

            var debug = fn.DebugInfo;
            ptr<Node> n = debug.Vars[varID]._<ptr<Node>>();

            long abbrev = default;

            if (n.Class() == PAUTO) 
                abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
            else if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
            else 
                return _addr_null!;
                        var gotype = ngotype(n).Linksym();
            var typename = dwarf.InfoPrefix + gotype.Name[len("type.")..];
            long inlIndex = 0L;
            if (genDwarfInline > 1L)
            {
                if (n.Name.InlFormal() || n.Name.InlLocal())
                {
                    inlIndex = posInlIndex(n.Pos) + 1L;
                    if (n.Name.InlFormal())
                    {
                        abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                    }

                }

            }

            var declpos = Ctxt.InnermostPos(n.Pos);
            ptr<dwarf.Var> dvar = addr(new dwarf.Var(Name:n.Sym.Name,IsReturnValue:n.Class()==PPARAMOUT,IsInlFormal:n.Name.InlFormal(),Abbrev:abbrev,Type:Ctxt.Lookup(typename),StackOffset:stackOffset(debug.Slots[debug.VarSlots[varID][0]]),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,));
            var list = debug.LocationLists[varID];
            if (len(list) != 0L)
            {
                dvar.PutLocationList = (listSym, startPC) =>
                {
                    debug.PutLocationList(list, Ctxt, listSym._<ptr<obj.LSym>>(), startPC._<ptr<obj.LSym>>());
                }
;

            }

            return _addr_dvar!;

        }

        // fieldtrack adds R_USEFIELD relocations to fnsym to record any
        // struct fields that it used.
        private static void fieldtrack(ptr<obj.LSym> _addr_fnsym, object tracked)
        {
            ref obj.LSym fnsym = ref _addr_fnsym.val;

            if (fnsym == null)
            {
                return ;
            }

            if (objabi.Fieldtrack_enabled == 0L || len(tracked) == 0L)
            {
                return ;
            }

            var trackSyms = make_slice<ptr<types.Sym>>(0L, len(tracked));
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

        private partial struct symByName // : slice<ptr<types.Sym>>
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
