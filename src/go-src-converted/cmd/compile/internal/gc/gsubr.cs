// Derived from Inferno utils/6c/txt.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6c/txt.c
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package gc -- go2cs converted at 2020 October 09 05:41:31 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\gsubr.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ptr<var> sharedProgArray = @new<[10000]obj.Prog>(); // *T instead of T to work around issue 19839

        // Progs accumulates Progs for a function and converts them into machine code.
        public partial struct Progs
        {
            public ptr<obj.Prog> Text; // ATEXT Prog for this function
            public ptr<obj.Prog> next; // next Prog
            public long pc; // virtual PC; count of Progs
            public src.XPos pos; // position to use for new Progs
            public ptr<Node> curfn; // fn these Progs are for
            public slice<obj.Prog> progcache; // local progcache
            public long cacheidx; // first free element of progcache

            public LivenessIndex nextLive; // liveness index for the next Prog
            public LivenessIndex prevLive; // last emitted liveness index
        }

        // newProgs returns a new Progs for fn.
        // worker indicates which of the backend workers will use the Progs.
        private static ptr<Progs> newProgs(ptr<Node> _addr_fn, long worker)
        {
            ref Node fn = ref _addr_fn.val;

            ptr<Progs> pp = @new<Progs>();
            if (Ctxt.CanReuseProgs())
            {
                var sz = len(sharedProgArray) / nBackendWorkers;
                pp.progcache = sharedProgArray[sz * worker..sz * (worker + 1L)];
            }

            pp.curfn = fn; 

            // prime the pump
            pp.next = pp.NewProg();
            pp.clearp(pp.next);

            pp.pos = fn.Pos;
            pp.settext(fn); 
            // PCDATA tables implicitly start with index -1.
            pp.prevLive = new LivenessIndex(-1,-1,false);
            if (go115ReduceLiveness)
            {
                pp.nextLive = pp.prevLive;
            }
            else
            {
                pp.nextLive = LivenessInvalid;
            }

            return _addr_pp!;

        }

        private static ptr<obj.Prog> NewProg(this ptr<Progs> _addr_pp)
        {
            ref Progs pp = ref _addr_pp.val;

            ptr<obj.Prog> p;
            if (pp.cacheidx < len(pp.progcache))
            {
                p = _addr_pp.progcache[pp.cacheidx];
                pp.cacheidx++;
            }
            else
            {
                p = @new<obj.Prog>();
            }

            p.Ctxt = Ctxt;
            return _addr_p!;

        }

        // Flush converts from pp to machine code.
        private static void Flush(this ptr<Progs> _addr_pp)
        {
            ref Progs pp = ref _addr_pp.val;

            ptr<obj.Plist> plist = addr(new obj.Plist(Firstpc:pp.Text,Curfn:pp.curfn));
            obj.Flushplist(Ctxt, plist, pp.NewProg, myimportpath);
        }

        // Free clears pp and any associated resources.
        private static void Free(this ptr<Progs> _addr_pp)
        {
            ref Progs pp = ref _addr_pp.val;

            if (Ctxt.CanReuseProgs())
            { 
                // Clear progs to enable GC and avoid abuse.
                var s = pp.progcache[..pp.cacheidx];
                foreach (var (i) in s)
                {
                    s[i] = new obj.Prog();
                }

            } 
            // Clear pp to avoid abuse.
            pp.val = new Progs();

        }

        // Prog adds a Prog with instruction As to pp.
        private static ptr<obj.Prog> Prog(this ptr<Progs> _addr_pp, obj.As @as)
        {
            ref Progs pp = ref _addr_pp.val;

            if (pp.nextLive.StackMapValid() && pp.nextLive.stackMapIndex != pp.prevLive.stackMapIndex)
            { 
                // Emit stack map index change.
                var idx = pp.nextLive.stackMapIndex;
                pp.prevLive.stackMapIndex = idx;
                var p = pp.Prog(obj.APCDATA);
                Addrconst(_addr_p.From, objabi.PCDATA_StackMapIndex);
                Addrconst(_addr_p.To, int64(idx));

            }

            if (!go115ReduceLiveness)
            {
                if (pp.nextLive.isUnsafePoint)
                { 
                    // Unsafe points are encoded as a special value in the
                    // register map.
                    pp.nextLive.regMapIndex = objabi.PCDATA_RegMapUnsafe;

                }

                if (pp.nextLive.regMapIndex != pp.prevLive.regMapIndex)
                { 
                    // Emit register map index change.
                    idx = pp.nextLive.regMapIndex;
                    pp.prevLive.regMapIndex = idx;
                    p = pp.Prog(obj.APCDATA);
                    Addrconst(_addr_p.From, objabi.PCDATA_RegMapIndex);
                    Addrconst(_addr_p.To, int64(idx));

                }

            }
            else
            {
                if (pp.nextLive.isUnsafePoint != pp.prevLive.isUnsafePoint)
                { 
                    // Emit unsafe-point marker.
                    pp.prevLive.isUnsafePoint = pp.nextLive.isUnsafePoint;
                    p = pp.Prog(obj.APCDATA);
                    Addrconst(_addr_p.From, objabi.PCDATA_UnsafePoint);
                    if (pp.nextLive.isUnsafePoint)
                    {
                        Addrconst(_addr_p.To, objabi.PCDATA_UnsafePointUnsafe);
                    }
                    else
                    {
                        Addrconst(_addr_p.To, objabi.PCDATA_UnsafePointSafe);
                    }

                }

            }

            p = pp.next;
            pp.next = pp.NewProg();
            pp.clearp(pp.next);
            p.Link = pp.next;

            if (!pp.pos.IsKnown() && Debug['K'] != 0L)
            {
                Warn("prog: unknown position (line 0)");
            }

            p.As = as;
            p.Pos = pp.pos;
            if (pp.pos.IsStmt() == src.PosIsStmt)
            { 
                // Clear IsStmt for later Progs at this pos provided that as can be marked as a stmt
                if (ssa.LosesStmtMark(as))
                {
                    return _addr_p!;
                }

                pp.pos = pp.pos.WithNotStmt();

            }

            return _addr_p!;

        }

        private static void clearp(this ptr<Progs> _addr_pp, ptr<obj.Prog> _addr_p)
        {
            ref Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;

            obj.Nopout(p);
            p.As = obj.AEND;
            p.Pc = pp.pc;
            pp.pc++;
        }

        private static ptr<obj.Prog> Appendpp(this ptr<Progs> _addr_pp, ptr<obj.Prog> _addr_p, obj.As @as, obj.AddrType ftype, short freg, long foffset, obj.AddrType ttype, short treg, long toffset)
        {
            ref Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;

            var q = pp.NewProg();
            pp.clearp(q);
            q.As = as;
            q.Pos = p.Pos;
            q.From.Type = ftype;
            q.From.Reg = freg;
            q.From.Offset = foffset;
            q.To.Type = ttype;
            q.To.Reg = treg;
            q.To.Offset = toffset;
            q.Link = p.Link;
            p.Link = q;
            return _addr_q!;
        }

        private static void settext(this ptr<Progs> _addr_pp, ptr<Node> _addr_fn)
        {
            ref Progs pp = ref _addr_pp.val;
            ref Node fn = ref _addr_fn.val;

            if (pp.Text != null)
            {
                Fatalf("Progs.settext called twice");
            }

            var ptxt = pp.Prog(obj.ATEXT);
            pp.Text = ptxt;

            fn.Func.lsym.Func.Text = ptxt;
            ptxt.From.Type = obj.TYPE_MEM;
            ptxt.From.Name = obj.NAME_EXTERN;
            ptxt.From.Sym = fn.Func.lsym;

        }

        // initLSym defines f's obj.LSym and initializes it based on the
        // properties of f. This includes setting the symbol flags and ABI and
        // creating and initializing related DWARF symbols.
        //
        // initLSym must be called exactly once per function and must be
        // called for both functions with bodies and functions without bodies.
        private static void initLSym(this ptr<Func> _addr_f, bool hasBody)
        {
            ref Func f = ref _addr_f.val;

            if (f.lsym != null)
            {
                Fatalf("Func.initLSym called twice");
            }

            {
                var nam = f.Nname;

                if (!nam.isBlank())
                {
                    f.lsym = nam.Sym.Linksym();
                    if (f.Pragma & Systemstack != 0L)
                    {
                        f.lsym.Set(obj.AttrCFunc, true);
                    }

                    obj.ABI aliasABI = default;
                    var needABIAlias = false;
                    var (defABI, hasDefABI) = symabiDefs[f.lsym.Name];
                    if (hasDefABI && defABI == obj.ABI0)
                    { 
                        // Symbol is defined as ABI0. Create an
                        // Internal -> ABI0 wrapper.
                        f.lsym.SetABI(obj.ABI0);
                        needABIAlias = true;
                        aliasABI = obj.ABIInternal;

                    }
                    else
                    { 
                        // No ABI override. Check that the symbol is
                        // using the expected ABI.
                        var want = obj.ABIInternal;
                        if (f.lsym.ABI() != want)
                        {
                            Fatalf("function symbol %s has the wrong ABI %v, expected %v", f.lsym.Name, f.lsym.ABI(), want);
                        }

                    }

                    var isLinknameExported = nam.Sym.Linkname != "" && (hasBody || hasDefABI);
                    {
                        var (abi, ok) = symabiRefs[f.lsym.Name];

                        if ((ok && abi == obj.ABI0) || isLinknameExported)
                        { 
                            // Either 1) this symbol is definitely
                            // referenced as ABI0 from this package; or 2)
                            // this symbol is defined in this package but
                            // given a linkname, indicating that it may be
                            // referenced from another package. Create an
                            // ABI0 -> Internal wrapper so it can be
                            // called as ABI0. In case 2, it's important
                            // that we know it's defined in this package
                            // since other packages may "pull" symbols
                            // using linkname and we don't want to create
                            // duplicate ABI wrappers.
                            if (f.lsym.ABI() != obj.ABI0)
                            {
                                needABIAlias = true;
                                aliasABI = obj.ABI0;

                            }

                        }

                    }


                    if (needABIAlias)
                    { 
                        // These LSyms have the same name as the
                        // native function, so we create them directly
                        // rather than looking them up. The uniqueness
                        // of f.lsym ensures uniqueness of asym.
                        ptr<obj.LSym> asym = addr(new obj.LSym(Name:f.lsym.Name,Type:objabi.SABIALIAS,R:[]obj.Reloc{{Sym:f.lsym}},));
                        asym.SetABI(aliasABI);
                        asym.Set(obj.AttrDuplicateOK, true);
                        Ctxt.ABIAliases = append(Ctxt.ABIAliases, asym);

                    }

                }

            }


            if (!hasBody)
            { 
                // For body-less functions, we only create the LSym.
                return ;

            }

            long flag = default;
            if (f.Dupok())
            {
                flag |= obj.DUPOK;
            }

            if (f.Wrapper())
            {
                flag |= obj.WRAPPER;
            }

            if (f.Needctxt())
            {
                flag |= obj.NEEDCTXT;
            }

            if (f.Pragma & Nosplit != 0L)
            {
                flag |= obj.NOSPLIT;
            }

            if (f.ReflectMethod())
            {
                flag |= obj.REFLECTMETHOD;
            } 

            // Clumsy but important.
            // See test/recover.go for test cases and src/reflect/value.go
            // for the actual functions being considered.
            if (myimportpath == "reflect")
            {
                switch (f.Nname.Sym.Name)
                {
                    case "callReflect": 

                    case "callMethod": 
                        flag |= obj.WRAPPER;
                        break;
                }

            }

            Ctxt.InitTextSym(f.lsym, flag);

        }

        private static void ggloblnod(ptr<Node> _addr_nam)
        {
            ref Node nam = ref _addr_nam.val;

            var s = nam.Sym.Linksym();
            s.Gotype = ngotype(nam).Linksym();
            long flags = 0L;
            if (nam.Name.Readonly())
            {
                flags = obj.RODATA;
            }

            if (nam.Type != null && !types.Haspointers(nam.Type))
            {
                flags |= obj.NOPTR;
            }

            Ctxt.Globl(s, nam.Type.Width, flags);
            if (nam.Name.LibfuzzerExtraCounter())
            {
                s.Type = objabi.SLIBFUZZER_EXTRA_COUNTER;
            }

        }

        private static void ggloblsym(ptr<obj.LSym> _addr_s, int width, short flags)
        {
            ref obj.LSym s = ref _addr_s.val;

            if (flags & obj.LOCAL != 0L)
            {
                s.Set(obj.AttrLocal, true);
                flags &= obj.LOCAL;
            }

            Ctxt.Globl(s, int64(width), int(flags));

        }

        public static void Addrconst(ptr<obj.Addr> _addr_a, long v)
        {
            ref obj.Addr a = ref _addr_a.val;

            a.Sym = null;
            a.Type = obj.TYPE_CONST;
            a.Offset = v;
        }

        public static void Patch(ptr<obj.Prog> _addr_p, ptr<obj.Prog> _addr_to)
        {
            ref obj.Prog p = ref _addr_p.val;
            ref obj.Prog to = ref _addr_to.val;

            if (p.To.Type != obj.TYPE_BRANCH)
            {
                Fatalf("patch: not a branch");
            }

            p.To.Val = to;
            p.To.Offset = to.Pc;

        }
    }
}}}}
