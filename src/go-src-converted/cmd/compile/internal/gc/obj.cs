// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:43 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\obj.go
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sha256 = go.crypto.sha256_package;
using fmt = go.fmt_package;
using io = go.io_package;
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
        public static readonly long ArhdrSize = 60L;



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
        private static readonly long modeCompilerObj = 1L << (int)(iota);
        private static readonly var modeLinkerObj = 0;

        private static void dumpobj()
        {
            if (!dolinkobj)
            {
                dumpobj1(outfile, modeCompilerObj);
                return;
            }
            if (linkobj == "")
            {
                dumpobj1(outfile, modeCompilerObj | modeLinkerObj);
                return;
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

            var startobj = int64(0L);
            array<byte> arhdr = new array<byte>(ArhdrSize);
            if (writearchive)
            {
                bout.WriteString("!<arch>\n");
                arhdr = new array<byte>(new byte[] {  });
                bout.Write(arhdr[..]);
                startobj = bout.Offset();
            }
            Action printheader = () =>
            {
                fmt.Fprintf(bout, "go object %s %s %s %s\n", objabi.GOOS, objabi.GOARCH, objabi.Version, objabi.Expstring());
                if (buildid != "")
                {
                    fmt.Fprintf(bout, "build id %q\n", buildid);
                }
                if (localpkg.Name == "main")
                {
                    fmt.Fprintf(bout, "main\n");
                }
                if (safemode)
                {
                    fmt.Fprintf(bout, "safe\n");
                }
                else
                {
                    fmt.Fprintf(bout, "----\n"); // room for some other tool to write "safe"
                }
                fmt.Fprintf(bout, "\n"); // header ends with blank line
            }
;

            printheader();

            if (mode & modeCompilerObj != 0L)
            {
                dumpexport(bout);
            }
            if (writearchive)
            {
                bout.Flush();
                var size = bout.Offset() - startobj;
                if (size & 1L != 0L)
                {
                    bout.WriteByte(0L);
                }
                bout.Seek(startobj - ArhdrSize, 0L);
                formathdr(arhdr[..], "__.PKGDEF", size);
                bout.Write(arhdr[..]);
                bout.Flush();
                bout.Seek(startobj + size + (size & 1L), 0L);
            }
            if (mode & modeLinkerObj == 0L)
            {
                return;
            }
            if (writearchive)
            { 
                // start object file
                arhdr = new array<byte>(new byte[] {  });
                bout.Write(arhdr[..]);
                startobj = bout.Offset();
                printheader();
            }
            if (pragcgobuf != "")
            {
                if (writearchive)
                { 
                    // write empty export section; must be before cgo section
                    fmt.Fprintf(bout, "\n$$\n\n$$\n\n");
                }
                fmt.Fprintf(bout, "\n$$  // cgo\n");
                fmt.Fprintf(bout, "%s\n$$\n\n", pragcgobuf);
            }
            fmt.Fprintf(bout, "\n!\n");

            var externs = len(externdcl);

            dumpglobls();
            addptabs();
            addsignats(externdcl);
            dumpsignats();
            dumptabs();
            dumpimportstrings();
            dumpbasictypes(); 

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

            obj.WriteObjFile(Ctxt, bout.Writer);

            if (writearchive)
            {
                bout.Flush();
                size = bout.Offset() - startobj;
                if (size & 1L != 0L)
                {
                    bout.WriteByte(0L);
                }
                bout.Seek(startobj - ArhdrSize, 0L);
                formathdr(arhdr[..], "_go_.o", size);
                bout.Write(arhdr[..]);
            }
        });

        private static void addptabs()
        {
            if (!Ctxt.Flag_dynlink || localpkg.Name != "main")
            {
                return;
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
                if (!exportname(s.Name))
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

        private static void dumpGlobal(ref Node n)
        {
            if (n.Type == null)
            {
                Fatalf("external %v nil type\n", n);
            }
            if (n.Class() == PFUNC)
            {
                return;
            }
            if (n.Sym.Pkg != localpkg)
            {
                return;
            }
            dowidth(n.Type);
            ggloblnod(n);
        }

        private static void dumpGlobalConst(ref Node n)
        { 
            // only export typed constants
            var t = n.Type;
            if (t == null)
            {
                return;
            }
            if (n.Sym.Pkg != localpkg)
            {
                return;
            } 
            // only export integer constants for now

            if (t.Etype == TINT8)             else if (t.Etype == TINT16)             else if (t.Etype == TINT32)             else if (t.Etype == TINT64)             else if (t.Etype == TINT)             else if (t.Etype == TUINT8)             else if (t.Etype == TUINT16)             else if (t.Etype == TUINT32)             else if (t.Etype == TUINT64)             else if (t.Etype == TUINT)             else if (t.Etype == TUINTPTR)             else if (t.Etype == TIDEAL) 
                if (!Isconst(n, CTINT))
                {
                    return;
                }
                ref Mpint x = n.Val().U._<ref Mpint>();
                if (x.Cmp(minintval[TINT]) < 0L || x.Cmp(maxintval[TINT]) > 0L)
                {
                    return;
                } 
                // Ideal integers we export as int (if they fit).
                t = types.Types[TINT];
            else 
                return;
                        Ctxt.DwarfIntConst(myimportpath, n.Sym.Name, typesymname(t), n.Int64());
        }

        private static void dumpglobls()
        { 
            // add globals
            foreach (var (_, n) in externdcl)
            {

                if (n.Op == ONAME) 
                    dumpGlobal(n);
                else if (n.Op == OLITERAL) 
                    dumpGlobalConst(n);
                            }
            obj.SortSlice(funcsyms, (i, j) =>
            {
                return funcsyms[i].LinksymName() < funcsyms[j].LinksymName();
            });
            foreach (var (_, s) in funcsyms)
            {
                var sf = s.Pkg.Lookup(funcsymname(s)).Linksym();
                dsymptr(sf, 0L, s.Linksym(), 0L);
                ggloblsym(sf, int32(Widthptr), obj.DUPOK | obj.RODATA);
            } 

            // Do not reprocess funcsyms on next dumpglobls call.
            funcsyms = null;
        }

        // addGCLocals adds gcargs and gclocals symbols to Ctxt.Data.
        // It takes care not to add any duplicates.
        // Though the object file format handles duplicates efficiently,
        // storing only a single copy of the data,
        // failure to remove these duplicates adds a few percent to object file size.
        private static void addGCLocals()
        {
            var seen = make_map<@string, bool>();
            foreach (var (_, s) in Ctxt.Text)
            {
                if (s.Func == null)
                {
                    continue;
                }
                foreach (var (_, gcsym) in new slice<ref obj.LSym>(new ref obj.LSym[] { &s.Func.GCArgs, &s.Func.GCLocals }))
                {
                    if (seen[gcsym.Name])
                    {
                        continue;
                    }
                    Ctxt.Data = append(Ctxt.Data, gcsym);
                    seen[gcsym.Name] = true;
                }
            }
        }

        private static long duintxx(ref obj.LSym s, long off, ulong v, long wid)
        {
            if (off & (wid - 1L) != 0L)
            {
                Fatalf("duintxxLSym: misaligned: v=%d wid=%d off=%d", v, wid, off);
            }
            s.WriteInt(Ctxt, int64(off), wid, int64(v));
            return off + wid;
        }

        private static long duint8(ref obj.LSym s, long off, byte v)
        {
            return duintxx(s, off, uint64(v), 1L);
        }

        private static long duint16(ref obj.LSym s, long off, ushort v)
        {
            return duintxx(s, off, uint64(v), 2L);
        }

        private static long duint32(ref obj.LSym s, long off, uint v)
        {
            return duintxx(s, off, uint64(v), 4L);
        }

        private static long duintptr(ref obj.LSym s, long off, ulong v)
        {
            return duintxx(s, off, v, Widthptr);
        }

        private static long dbvec(ref obj.LSym s, long off, bvec bv)
        { 
            // Runtime reads the bitmaps as byte arrays. Oblige.
            {
                long j = 0L;

                while (int32(j) < bv.n)
                {
                    var word = bv.b[j / 32L];
                    off = duint8(s, off, uint8(word >> (int)((uint(j) % 32L))));
                    j += 8L;
                }

            }
            return off;
        }

        private static ref obj.LSym stringsym(src.XPos pos, @string s)
        {
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
            const @string prefix = "go.string.";

            var symdataname = prefix + symname;

            var symdata = Ctxt.Lookup(symdataname);

            if (!symdata.SeenGlobl())
            { 
                // string data
                var off = dsname(symdata, 0L, s, pos, "string");
                ggloblsym(symdata, int32(off), obj.DUPOK | obj.RODATA | obj.LOCAL);
            }
            return symdata;
        }

        private static long slicebytes_gen = default;

        private static void slicebytes(ref Node nam, @string s, long len)
        {
            slicebytes_gen++;
            var symname = fmt.Sprintf(".gobytes.%d", slicebytes_gen);
            var sym = localpkg.Lookup(symname);
            sym.Def = asTypesNode(newname(sym));

            var lsym = sym.Linksym();
            var off = dsname(lsym, 0L, s, nam.Pos, "slice");
            ggloblsym(lsym, int32(off), obj.NOPTR | obj.LOCAL);

            if (nam.Op != ONAME)
            {
                Fatalf("slicebytes %v", nam);
            }
            var nsym = nam.Sym.Linksym();
            off = int(nam.Xoffset);
            off = dsymptr(nsym, off, lsym, 0L);
            off = duintptr(nsym, off, uint64(len));
            duintptr(nsym, off, uint64(len));
        }

        private static long dsname(ref obj.LSym s, long off, @string t, src.XPos pos, @string what)
        { 
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

        private static long dsymptr(ref obj.LSym s, long off, ref obj.LSym x, long xoff)
        {
            off = int(Rnd(int64(off), int64(Widthptr)));
            s.WriteAddr(Ctxt, int64(off), Widthptr, x, int64(xoff));
            off += Widthptr;
            return off;
        }

        private static long dsymptrOff(ref obj.LSym s, long off, ref obj.LSym x, long xoff)
        {
            s.WriteOff(Ctxt, int64(off), x, int64(xoff));
            off += 4L;
            return off;
        }

        private static long dsymptrWeakOff(ref obj.LSym s, long off, ref obj.LSym x)
        {
            s.WriteWeakOff(Ctxt, int64(off), x, 0L);
            off += 4L;
            return off;
        }

        private static void gdata(ref Node nam, ref Node nr, long wid)
        {
            if (nam.Op != ONAME)
            {
                Fatalf("gdata nam op %v", nam.Op);
            }
            if (nam.Sym == null)
            {
                Fatalf("gdata nil nam sym");
            }
            var s = nam.Sym.Linksym();


            if (nr.Op == OLITERAL) 
                switch (nr.Val().U.type())
                {
                    case bool u:
                        var i = int64(obj.Bool2int(u));
                        s.WriteInt(Ctxt, nam.Xoffset, wid, i);
                        break;
                    case ref Mpint u:
                        s.WriteInt(Ctxt, nam.Xoffset, wid, u.Int64());
                        break;
                    case ref Mpflt u:
                        var f = u.Float64();

                        if (nam.Type.Etype == TFLOAT32) 
                            s.WriteFloat32(Ctxt, nam.Xoffset, float32(f));
                        else if (nam.Type.Etype == TFLOAT64) 
                            s.WriteFloat64(Ctxt, nam.Xoffset, f);
                                                break;
                    case ref Mpcplx u:
                        var r = u.Real.Float64();
                        i = u.Imag.Float64();

                        if (nam.Type.Etype == TCOMPLEX64) 
                            s.WriteFloat32(Ctxt, nam.Xoffset, float32(r));
                            s.WriteFloat32(Ctxt, nam.Xoffset + 4L, float32(i));
                        else if (nam.Type.Etype == TCOMPLEX128) 
                            s.WriteFloat64(Ctxt, nam.Xoffset, r);
                            s.WriteFloat64(Ctxt, nam.Xoffset + 8L, i);
                                                break;
                    case @string u:
                        var symdata = stringsym(nam.Pos, u);
                        s.WriteAddr(Ctxt, nam.Xoffset, Widthptr, symdata, 0L);
                        s.WriteInt(Ctxt, nam.Xoffset + int64(Widthptr), Widthptr, int64(len(u)));
                        break;
                    default:
                    {
                        var u = nr.Val().U.type();
                        Fatalf("gdata unhandled OLITERAL %v", nr);
                        break;
                    }

                }
            else if (nr.Op == OADDR) 
                if (nr.Left.Op != ONAME)
                {
                    Fatalf("gdata ADDR left op %v", nr.Left.Op);
                }
                var to = nr.Left;
                s.WriteAddr(Ctxt, nam.Xoffset, wid, to.Sym.Linksym(), to.Xoffset);
            else if (nr.Op == ONAME) 
                if (nr.Class() != PFUNC)
                {
                    Fatalf("gdata NAME not PFUNC %d", nr.Class());
                }
                s.WriteAddr(Ctxt, nam.Xoffset, wid, funcsym(nr.Sym).Linksym(), nr.Xoffset);
            else 
                Fatalf("gdata unhandled op %v %v\n", nr, nr.Op);
                    }
    }
}}}}
