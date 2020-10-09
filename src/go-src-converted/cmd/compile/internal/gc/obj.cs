// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:05 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\obj.go
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sha256 = go.crypto.sha256_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // architecture-independent object file output
        public static readonly long ArhdrSize = (long)60L;



        private static void formathdr(slice<byte> arhdr, @string name, long size)
        {
            copy(arhdr[..], fmt.Sprintf("%-16s%-12d%-6d%-6d%-8o%-10d`\n", name, 0L, 0L, 0L, 0644L, size));
        }

        // These modes say which kind of object file to generate.
        // The default use of the toolchain is to set both bits,
        // generating a combined compiler+linker object, one that
        // serves to describe the package to both the compiler and the linker.
        // In fact the compiler and linker read nearly disjoint sections of
        // that file, though, so in a distributed build setting it can be more
        // efficient to split the output into two files, supplying the compiler
        // object only to future compilations and the linker object only to
        // future links.
        //
        // By default a combined object is written, but if -linkobj is specified
        // on the command line then the default -o output is a compiler object
        // and the -linkobj output is a linker object.
        private static readonly long modeCompilerObj = (long)1L << (int)(iota);
        private static readonly var modeLinkerObj = 0;


        private static void dumpobj()
        {
            if (linkobj == "")
            {
                dumpobj1(outfile, modeCompilerObj | modeLinkerObj);
                return ;
            }

            dumpobj1(outfile, modeCompilerObj);
            dumpobj1(linkobj, modeLinkerObj);

        }

        private static void dumpobj1(@string outfile, long mode) => func((defer, _, __) =>
        {
            var (bout, err) = bio.Create(outfile);
            if (err != null)
            {
                flusherrors();
                fmt.Printf("can't create %s: %v\n", outfile, err);
                errorexit();
            }

            defer(bout.Close());
            bout.WriteString("!<arch>\n");

            if (mode & modeCompilerObj != 0L)
            {
                var start = startArchiveEntry(_addr_bout);
                dumpCompilerObj(_addr_bout);
                finishArchiveEntry(_addr_bout, start, "__.PKGDEF");
            }

            if (mode & modeLinkerObj != 0L)
            {
                start = startArchiveEntry(_addr_bout);
                dumpLinkerObj(_addr_bout);
                finishArchiveEntry(_addr_bout, start, "_go_.o");
            }

        });

        private static void printObjHeader(ptr<bio.Writer> _addr_bout)
        {
            ref bio.Writer bout = ref _addr_bout.val;

            fmt.Fprintf(bout, "go object %s %s %s %s\n", objabi.GOOS, objabi.GOARCH, objabi.Version, objabi.Expstring());
            if (buildid != "")
            {
                fmt.Fprintf(bout, "build id %q\n", buildid);
            }

            if (localpkg.Name == "main")
            {
                fmt.Fprintf(bout, "main\n");
            }

            fmt.Fprintf(bout, "\n"); // header ends with blank line
        }

        private static long startArchiveEntry(ptr<bio.Writer> _addr_bout)
        {
            ref bio.Writer bout = ref _addr_bout.val;

            array<byte> arhdr = new array<byte>(ArhdrSize);
            bout.Write(arhdr[..]);
            return bout.Offset();
        }

        private static void finishArchiveEntry(ptr<bio.Writer> _addr_bout, long start, @string name)
        {
            ref bio.Writer bout = ref _addr_bout.val;

            bout.Flush();
            var size = bout.Offset() - start;
            if (size & 1L != 0L)
            {
                bout.WriteByte(0L);
            }

            bout.MustSeek(start - ArhdrSize, 0L);

            array<byte> arhdr = new array<byte>(ArhdrSize);
            formathdr(arhdr[..], name, size);
            bout.Write(arhdr[..]);
            bout.Flush();
            bout.MustSeek(start + size + (size & 1L), 0L);

        }

        private static void dumpCompilerObj(ptr<bio.Writer> _addr_bout)
        {
            ref bio.Writer bout = ref _addr_bout.val;

            printObjHeader(_addr_bout);
            dumpexport(bout);
        }

        private static void dumpdata()
        {
            var externs = len(externdcl);

            dumpglobls();
            addptabs();
            addsignats(externdcl);
            dumpsignats();
            dumptabs();
            dumpimportstrings();
            dumpbasictypes(); 

            // Calls to dumpsignats can generate functions,
            // like method wrappers and hash and equality routines.
            // Compile any generated functions, process any new resulting types, repeat.
            // This can't loop forever, because there is no way to generate an infinite
            // number of types in a finite amount of code.
            // In the typical case, we loop 0 or 1 times.
            // It was not until issue 24761 that we found any code that required a loop at all.
            while (len(compilequeue) > 0L)
            {
                compileFunctions();
                dumpsignats();
            } 

            // Dump extra globals.
 

            // Dump extra globals.
            var tmp = externdcl;

            if (externdcl != null)
            {
                externdcl = externdcl[externs..];
            }

            dumpglobls();
            externdcl = tmp;

            if (zerosize > 0L)
            {
                var zero = mappkg.Lookup("zero");
                ggloblsym(zero.Linksym(), int32(zerosize), obj.DUPOK | obj.RODATA);
            }

            addGCLocals();

        }

        private static void dumpLinkerObj(ptr<bio.Writer> _addr_bout)
        {
            ref bio.Writer bout = ref _addr_bout.val;

            printObjHeader(_addr_bout);

            if (len(pragcgobuf) != 0L)
            { 
                // write empty export section; must be before cgo section
                fmt.Fprintf(bout, "\n$$\n\n$$\n\n");
                fmt.Fprintf(bout, "\n$$  // cgo\n");
                {
                    var err = json.NewEncoder(bout).Encode(pragcgobuf);

                    if (err != null)
                    {
                        Fatalf("serializing pragcgobuf: %v", err);
                    }

                }

                fmt.Fprintf(bout, "\n$$\n\n");

            }

            fmt.Fprintf(bout, "\n!\n");

            obj.WriteObjFile(Ctxt, bout, myimportpath);

        }

        private static void addptabs()
        {
            if (!Ctxt.Flag_dynlink || localpkg.Name != "main")
            {
                return ;
            }

            foreach (var (_, exportn) in exportlist)
            {
                var s = exportn.Sym;
                var n = asNode(s.Def);
                if (n == null)
                {
                    continue;
                }

                if (n.Op != ONAME)
                {
                    continue;
                }

                if (!types.IsExported(s.Name))
                {
                    continue;
                }

                if (s.Pkg.Name != "main")
                {
                    continue;
                }

                if (n.Type.Etype == TFUNC && n.Class() == PFUNC)
                { 
                    // function
                    ptabs = append(ptabs, new ptabEntry(s:s,t:asNode(s.Def).Type));

                }
                else
                { 
                    // variable
                    ptabs = append(ptabs, new ptabEntry(s:s,t:types.NewPtr(asNode(s.Def).Type)));

                }

            }

        }

        private static void dumpGlobal(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Type == null)
            {
                Fatalf("external %v nil type\n", n);
            }

            if (n.Class() == PFUNC)
            {
                return ;
            }

            if (n.Sym.Pkg != localpkg)
            {
                return ;
            }

            dowidth(n.Type);
            ggloblnod(n);

        }

        private static void dumpGlobalConst(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // only export typed constants
            var t = n.Type;
            if (t == null)
            {
                return ;
            }

            if (n.Sym.Pkg != localpkg)
            {
                return ;
            } 
            // only export integer constants for now

            if (t.Etype == TINT8)             else if (t.Etype == TINT16)             else if (t.Etype == TINT32)             else if (t.Etype == TINT64)             else if (t.Etype == TINT)             else if (t.Etype == TUINT8)             else if (t.Etype == TUINT16)             else if (t.Etype == TUINT32)             else if (t.Etype == TUINT64)             else if (t.Etype == TUINT)             else if (t.Etype == TUINTPTR)             else if (t.Etype == TIDEAL) 
                if (!Isconst(n, CTINT))
                {
                    return ;
                }

                ptr<Mpint> x = n.Val().U._<ptr<Mpint>>();
                if (x.Cmp(minintval[TINT]) < 0L || x.Cmp(maxintval[TINT]) > 0L)
                {
                    return ;
                } 
                // Ideal integers we export as int (if they fit).
                t = types.Types[TINT];
            else 
                return ;
                        Ctxt.DwarfIntConst(myimportpath, n.Sym.Name, typesymname(t), n.Int64());

        }

        private static void dumpglobls()
        { 
            // add globals
            foreach (var (_, n) in externdcl)
            {

                if (n.Op == ONAME) 
                    dumpGlobal(_addr_n);
                else if (n.Op == OLITERAL) 
                    dumpGlobalConst(_addr_n);
                
            }
            sort.Slice(funcsyms, (i, j) =>
            {
                return funcsyms[i].LinksymName() < funcsyms[j].LinksymName();
            });
            foreach (var (_, s) in funcsyms)
            {
                var sf = s.Pkg.Lookup(funcsymname(s)).Linksym();
                dsymptr(_addr_sf, 0L, _addr_s.Linksym(), 0L);
                ggloblsym(sf, int32(Widthptr), obj.DUPOK | obj.RODATA);
            } 

            // Do not reprocess funcsyms on next dumpglobls call.
            funcsyms = null;

        }

        // addGCLocals adds gcargs, gclocals, gcregs, and stack object symbols to Ctxt.Data.
        //
        // This is done during the sequential phase after compilation, since
        // global symbols can't be declared during parallel compilation.
        private static void addGCLocals()
        {
            foreach (var (_, s) in Ctxt.Text)
            {
                if (s.Func == null)
                {
                    continue;
                }

                foreach (var (_, gcsym) in new slice<ptr<obj.LSym>>(new ptr<obj.LSym>[] { s.Func.GCArgs, s.Func.GCLocals, s.Func.GCRegs }))
                {
                    if (gcsym != null && !gcsym.OnList())
                    {
                        ggloblsym(gcsym, int32(len(gcsym.P)), obj.RODATA | obj.DUPOK);
                    }

                }
                {
                    var x__prev1 = x;

                    var x = s.Func.StackObjects;

                    if (x != null)
                    {
                        var attr = int16(obj.RODATA);
                        if (s.DuplicateOK())
                        {
                            attr |= obj.DUPOK;
                        }

                        ggloblsym(x, int32(len(x.P)), attr);

                    }

                    x = x__prev1;

                }

                {
                    var x__prev1 = x;

                    x = s.Func.OpenCodedDeferInfo;

                    if (x != null)
                    {
                        ggloblsym(x, int32(len(x.P)), obj.RODATA | obj.DUPOK);
                    }

                    x = x__prev1;

                }

            }

        }

        private static long duintxx(ptr<obj.LSym> _addr_s, long off, ulong v, long wid)
        {
            ref obj.LSym s = ref _addr_s.val;

            if (off & (wid - 1L) != 0L)
            {
                Fatalf("duintxxLSym: misaligned: v=%d wid=%d off=%d", v, wid, off);
            }

            s.WriteInt(Ctxt, int64(off), wid, int64(v));
            return off + wid;

        }

        private static long duint8(ptr<obj.LSym> _addr_s, long off, byte v)
        {
            ref obj.LSym s = ref _addr_s.val;

            return duintxx(_addr_s, off, uint64(v), 1L);
        }

        private static long duint16(ptr<obj.LSym> _addr_s, long off, ushort v)
        {
            ref obj.LSym s = ref _addr_s.val;

            return duintxx(_addr_s, off, uint64(v), 2L);
        }

        private static long duint32(ptr<obj.LSym> _addr_s, long off, uint v)
        {
            ref obj.LSym s = ref _addr_s.val;

            return duintxx(_addr_s, off, uint64(v), 4L);
        }

        private static long duintptr(ptr<obj.LSym> _addr_s, long off, ulong v)
        {
            ref obj.LSym s = ref _addr_s.val;

            return duintxx(_addr_s, off, v, Widthptr);
        }

        private static long dbvec(ptr<obj.LSym> _addr_s, long off, bvec bv)
        {
            ref obj.LSym s = ref _addr_s.val;
 
            // Runtime reads the bitmaps as byte arrays. Oblige.
            {
                long j = 0L;

                while (int32(j) < bv.n)
                {
                    var word = bv.b[j / 32L];
                    off = duint8(_addr_s, off, uint8(word >> (int)((uint(j) % 32L))));
                    j += 8L;
                }

            }
            return off;

        }

        private static ptr<obj.LSym> stringsym(src.XPos pos, @string s)
        {
            ptr<obj.LSym> data = default!;

            @string symname = default;
            if (len(s) > 100L)
            { 
                // Huge strings are hashed to avoid long names in object files.
                // Indulge in some paranoia by writing the length of s, too,
                // as protection against length extension attacks.
                var h = sha256.New();
                io.WriteString(h, s);
                symname = fmt.Sprintf(".gostring.%d.%x", len(s), h.Sum(null));

            }
            else
            { 
                // Small strings get named directly by their contents.
                symname = strconv.Quote(s);

            }

            const @string prefix = (@string)"go.string.";

            var symdataname = prefix + symname;

            var symdata = Ctxt.Lookup(symdataname);

            if (!symdata.SeenGlobl())
            { 
                // string data
                var off = dsname(_addr_symdata, 0L, s, pos, "string");
                ggloblsym(symdata, int32(off), obj.DUPOK | obj.RODATA | obj.LOCAL);

            }

            return _addr_symdata!;

        }

        private static long slicebytes_gen = default;

        private static void slicebytes(ptr<Node> _addr_nam, @string s)
        {
            ref Node nam = ref _addr_nam.val;

            slicebytes_gen++;
            var symname = fmt.Sprintf(".gobytes.%d", slicebytes_gen);
            var sym = localpkg.Lookup(symname);
            var symnode = newname(sym);
            sym.Def = asTypesNode(symnode);

            var lsym = sym.Linksym();
            var off = dsname(_addr_lsym, 0L, s, nam.Pos, "slice");
            ggloblsym(lsym, int32(off), obj.NOPTR | obj.LOCAL);

            if (nam.Op != ONAME)
            {
                Fatalf("slicebytes %v", nam);
            }

            slicesym(_addr_nam, _addr_symnode, int64(len(s)));

        }

        private static long dsname(ptr<obj.LSym> _addr_s, long off, @string t, src.XPos pos, @string what)
        {
            ref obj.LSym s = ref _addr_s.val;
 
            // Objects that are too large will cause the data section to overflow right away,
            // causing a cryptic error message by the linker. Check for oversize objects here
            // and provide a useful error message instead.
            if (int64(len(t)) > 2e9F)
            {
                yyerrorl(pos, "%v with length %v is too big", what, len(t));
                return 0L;
            }

            s.WriteString(Ctxt, int64(off), len(t), t);
            return off + len(t);

        }

        private static long dsymptr(ptr<obj.LSym> _addr_s, long off, ptr<obj.LSym> _addr_x, long xoff)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref obj.LSym x = ref _addr_x.val;

            off = int(Rnd(int64(off), int64(Widthptr)));
            s.WriteAddr(Ctxt, int64(off), Widthptr, x, int64(xoff));
            off += Widthptr;
            return off;
        }

        private static long dsymptrOff(ptr<obj.LSym> _addr_s, long off, ptr<obj.LSym> _addr_x)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref obj.LSym x = ref _addr_x.val;

            s.WriteOff(Ctxt, int64(off), x, 0L);
            off += 4L;
            return off;
        }

        private static long dsymptrWeakOff(ptr<obj.LSym> _addr_s, long off, ptr<obj.LSym> _addr_x)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref obj.LSym x = ref _addr_x.val;

            s.WriteWeakOff(Ctxt, int64(off), x, 0L);
            off += 4L;
            return off;
        }

        // slicesym writes a static slice symbol {&arr, lencap, lencap} to n.
        // arr must be an ONAME. slicesym does not modify n.
        private static void slicesym(ptr<Node> _addr_n, ptr<Node> _addr_arr, long lencap)
        {
            ref Node n = ref _addr_n.val;
            ref Node arr = ref _addr_arr.val;

            var s = n.Sym.Linksym();
            var @base = n.Xoffset;
            if (arr.Op != ONAME)
            {
                Fatalf("slicesym non-name arr %v", arr);
            }

            s.WriteAddr(Ctxt, base, Widthptr, arr.Sym.Linksym(), arr.Xoffset);
            s.WriteInt(Ctxt, base + sliceLenOffset, Widthptr, lencap);
            s.WriteInt(Ctxt, base + sliceCapOffset, Widthptr, lencap);

        }

        // addrsym writes the static address of a to n. a must be an ONAME.
        // Neither n nor a is modified.
        private static void addrsym(ptr<Node> _addr_n, ptr<Node> _addr_a)
        {
            ref Node n = ref _addr_n.val;
            ref Node a = ref _addr_a.val;

            if (n.Op != ONAME)
            {
                Fatalf("addrsym n op %v", n.Op);
            }

            if (n.Sym == null)
            {
                Fatalf("addrsym nil n sym");
            }

            if (a.Op != ONAME)
            {
                Fatalf("addrsym a op %v", a.Op);
            }

            var s = n.Sym.Linksym();
            s.WriteAddr(Ctxt, n.Xoffset, Widthptr, a.Sym.Linksym(), a.Xoffset);

        }

        // pfuncsym writes the static address of f to n. f must be a global function.
        // Neither n nor f is modified.
        private static void pfuncsym(ptr<Node> _addr_n, ptr<Node> _addr_f)
        {
            ref Node n = ref _addr_n.val;
            ref Node f = ref _addr_f.val;

            if (n.Op != ONAME)
            {
                Fatalf("pfuncsym n op %v", n.Op);
            }

            if (n.Sym == null)
            {
                Fatalf("pfuncsym nil n sym");
            }

            if (f.Class() != PFUNC)
            {
                Fatalf("pfuncsym class not PFUNC %d", f.Class());
            }

            var s = n.Sym.Linksym();
            s.WriteAddr(Ctxt, n.Xoffset, Widthptr, funcsym(f.Sym).Linksym(), f.Xoffset);

        }

        // litsym writes the static literal c to n.
        // Neither n nor c is modified.
        private static void litsym(ptr<Node> _addr_n, ptr<Node> _addr_c, long wid)
        {
            ref Node n = ref _addr_n.val;
            ref Node c = ref _addr_c.val;

            if (n.Op != ONAME)
            {
                Fatalf("litsym n op %v", n.Op);
            }

            if (c.Op != OLITERAL)
            {
                Fatalf("litsym c op %v", c.Op);
            }

            if (n.Sym == null)
            {
                Fatalf("litsym nil n sym");
            }

            var s = n.Sym.Linksym();
            switch (c.Val().U.type())
            {
                case bool u:
                    var i = int64(obj.Bool2int(u));
                    s.WriteInt(Ctxt, n.Xoffset, wid, i);
                    break;
                case ptr<Mpint> u:
                    s.WriteInt(Ctxt, n.Xoffset, wid, u.Int64());
                    break;
                case ptr<Mpflt> u:
                    var f = u.Float64();

                    if (n.Type.Etype == TFLOAT32) 
                        s.WriteFloat32(Ctxt, n.Xoffset, float32(f));
                    else if (n.Type.Etype == TFLOAT64) 
                        s.WriteFloat64(Ctxt, n.Xoffset, f);
                                        break;
                case ptr<Mpcplx> u:
                    var r = u.Real.Float64();
                    i = u.Imag.Float64();

                    if (n.Type.Etype == TCOMPLEX64) 
                        s.WriteFloat32(Ctxt, n.Xoffset, float32(r));
                        s.WriteFloat32(Ctxt, n.Xoffset + 4L, float32(i));
                    else if (n.Type.Etype == TCOMPLEX128) 
                        s.WriteFloat64(Ctxt, n.Xoffset, r);
                        s.WriteFloat64(Ctxt, n.Xoffset + 8L, i);
                                        break;
                case @string u:
                    var symdata = stringsym(n.Pos, u);
                    s.WriteAddr(Ctxt, n.Xoffset, Widthptr, symdata, 0L);
                    s.WriteInt(Ctxt, n.Xoffset + int64(Widthptr), Widthptr, int64(len(u)));
                    break;
                default:
                {
                    var u = c.Val().U.type();
                    Fatalf("litsym unhandled OLITERAL %v", c);
                    break;
                }
            }

        }
    }
}}}}
