// Derived from Inferno utils/6c/txt.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6c/txt.c
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

// package gc -- go2cs converted at 2020 August 29 09:27:10 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\gsubr.go
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
        private static var sharedProgArray = @new<array<obj.Prog>>(); // *T instead of T to work around issue 19839

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
        }

        // newProgs returns a new Progs for fn.
        // worker indicates which of the backend workers will use the Progs.
        private static ref Progs newProgs(ref Node fn, long worker)
        {
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
            return pp;
        }

        private static ref obj.Prog NewProg(this ref Progs pp)
        {
            ref obj.Prog p = default;
            if (pp.cacheidx < len(pp.progcache))
            {
                p = ref pp.progcache[pp.cacheidx];
                pp.cacheidx++;
            }
            else
            {
                p = @new<obj.Prog>();
            }
            p.Ctxt = Ctxt;
            return p;
        }

        // Flush converts from pp to machine code.
        private static void Flush(this ref Progs pp)
        {
            obj.Plist plist = ref new obj.Plist(Firstpc:pp.Text,Curfn:pp.curfn);
            obj.Flushplist(Ctxt, plist, pp.NewProg, myimportpath);
        }

        // Free clears pp and any associated resources.
        private static void Free(this ref Progs pp)
        {
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
            pp.Value = new Progs();
        }

        // Prog adds a Prog with instruction As to pp.
        private static ref obj.Prog Prog(this ref Progs pp, obj.As @as)
        {
            var p = pp.next;
            pp.next = pp.NewProg();
            pp.clearp(pp.next);
            p.Link = pp.next;

            if (!pp.pos.IsKnown() && Debug['K'] != 0L)
            {
                Warn("prog: unknown position (line 0)");
            }
            p.As = as;
            p.Pos = pp.pos;
            return p;
        }

        private static void clearp(this ref Progs pp, ref obj.Prog p)
        {
            obj.Nopout(p);
            p.As = obj.AEND;
            p.Pc = pp.pc;
            pp.pc++;
        }

        private static ref obj.Prog Appendpp(this ref Progs pp, ref obj.Prog p, obj.As @as, obj.AddrType ftype, short freg, long foffset, obj.AddrType ttype, short treg, long toffset)
        {
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
            return q;
        }

        private static void settext(this ref Progs pp, ref Node fn)
        {
            if (pp.Text != null)
            {
                Fatalf("Progs.settext called twice");
            }
            var ptxt = pp.Prog(obj.ATEXT);
            pp.Text = ptxt;

            if (fn.Func.lsym == null)
            { 
                // func _() { }
                return;
            }
            fn.Func.lsym.Func.Text = ptxt;
            ptxt.From.Type = obj.TYPE_MEM;
            ptxt.From.Name = obj.NAME_EXTERN;
            ptxt.From.Sym = fn.Func.lsym;

            var p = pp.Prog(obj.AFUNCDATA);
            Addrconst(ref p.From, objabi.FUNCDATA_ArgsPointerMaps);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = ref fn.Func.lsym.Func.GCArgs;

            p = pp.Prog(obj.AFUNCDATA);
            Addrconst(ref p.From, objabi.FUNCDATA_LocalsPointerMaps);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = ref fn.Func.lsym.Func.GCLocals;
        }

        private static void initLSym(this ref Func f)
        {
            if (f.lsym != null)
            {
                Fatalf("Func.initLSym called twice");
            }
            {
                var nam = f.Nname;

                if (!isblank(nam))
                {
                    f.lsym = nam.Sym.Linksym();
                    if (f.Pragma & Systemstack != 0L)
                    {
                        f.lsym.Set(obj.AttrCFunc, true);
                    }
                }

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
            if (f.NoFramePointer())
            {
                flag |= obj.NOFRAME;
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

        private static void ggloblnod(ref Node nam)
        {
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
        }

        private static void ggloblsym(ref obj.LSym s, int width, short flags)
        {
            if (flags & obj.LOCAL != 0L)
            {
                s.Set(obj.AttrLocal, true);
                flags &= obj.LOCAL;
            }
            Ctxt.Globl(s, int64(width), int(flags));
        }

        private static bool isfat(ref types.Type t)
        {
            if (t != null)
            {

                if (t.Etype == TSTRUCT || t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TSTRING || t.Etype == TINTER) // maybe remove later
                    return true;
                            }
            return false;
        }

        public static void Addrconst(ref obj.Addr a, long v)
        {
            a.Sym = null;
            a.Type = obj.TYPE_CONST;
            a.Offset = v;
        }

        public static void Patch(ref obj.Prog p, ref obj.Prog to)
        {
            if (p.To.Type != obj.TYPE_BRANCH)
            {
                Fatalf("patch: not a branch");
            }
            p.To.Val = to;
            p.To.Offset = to.Pc;
        }
    }
}}}}
