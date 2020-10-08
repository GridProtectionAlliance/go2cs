// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:50 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\export.go
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        public static long Debug_export = default;

        private static void exportf(ptr<bio.Writer> _addr_bout, @string format, params object[] args)
        {
            args = args.Clone();
            ref bio.Writer bout = ref _addr_bout.val;

            fmt.Fprintf(bout, format, args);
            if (Debug_export != 0L)
            {
                fmt.Printf(format, args);
            }

        }

        private static slice<ptr<Node>> asmlist = default;

        // exportsym marks n for export (or reexport).
        private static void exportsym(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Sym.OnExportList())
            {
                return ;
            }

            n.Sym.SetOnExportList(true);

            if (Debug['E'] != 0L)
            {
                fmt.Printf("export symbol %v\n", n.Sym);
            }

            exportlist = append(exportlist, n);

        }

        private static bool initname(@string s)
        {
            return s == "init";
        }

        private static void autoexport(ptr<Node> _addr_n, Class ctxt)
        {
            ref Node n = ref _addr_n.val;

            if (n.Sym.Pkg != localpkg)
            {
                return ;
            }

            if ((ctxt != PEXTERN && ctxt != PFUNC) || dclcontext != PEXTERN)
            {
                return ;
            }

            if (n.Type != null && n.Type.IsKind(TFUNC) && n.IsMethod())
            {
                return ;
            }

            if (types.IsExported(n.Sym.Name) || initname(n.Sym.Name))
            {
                exportsym(_addr_n);
            }

            if (asmhdr != "" && !n.Sym.Asm())
            {
                n.Sym.SetAsm(true);
                asmlist = append(asmlist, n);
            }

        }

        private static void dumpexport(ptr<bio.Writer> _addr_bout)
        {
            ref bio.Writer bout = ref _addr_bout.val;
 
            // The linker also looks for the $$ marker - use char after $$ to distinguish format.
            exportf(_addr_bout, "\n$$B\n"); // indicate binary export format
            var off = bout.Offset();
            iexport(bout.Writer);
            var size = bout.Offset() - off;
            exportf(_addr_bout, "\n$$\n");

            if (Debug_export != 0L)
            {
                fmt.Printf("BenchmarkExportSize:%s 1 %d bytes\n", myimportpath, size);
            }

        }

        private static ptr<Node> importsym(ptr<types.Pkg> _addr_ipkg, ptr<types.Sym> _addr_s, Op op)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;

            var n = asNode(s.PkgDef());
            if (n == null)
            { 
                // iimport should have created a stub ONONAME
                // declaration for all imported symbols. The exception
                // is declarations for Runtimepkg, which are populated
                // by loadsys instead.
                if (s.Pkg != Runtimepkg)
                {
                    Fatalf("missing ONONAME for %v\n", s);
                }

                n = dclname(s);
                s.SetPkgDef(asTypesNode(n));
                s.Importdef = ipkg;

            }

            if (n.Op != ONONAME && n.Op != op)
            {
                redeclare(lineno, s, fmt.Sprintf("during import %q", ipkg.Path));
            }

            return _addr_n!;

        }

        // pkgtype returns the named type declared by symbol s.
        // If no such type has been declared yet, a forward declaration is returned.
        // ipkg is the package being imported
        private static ptr<types.Type> importtype(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;

            var n = importsym(_addr_ipkg, _addr_s, OTYPE);
            if (n.Op != OTYPE)
            {
                var t = types.New(TFORW);
                t.Sym = s;
                t.Nod = asTypesNode(n);

                n.Op = OTYPE;
                n.Pos = pos;
                n.Type = t;
                n.SetClass(PEXTERN);
            }

            t = n.Type;
            if (t == null)
            {
                Fatalf("importtype %v", s);
            }

            return _addr_t!;

        }

        // importobj declares symbol s as an imported object representable by op.
        // ipkg is the package being imported
        private static ptr<Node> importobj(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, Op op, Class ctxt, ptr<types.Type> _addr_t)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var n = importsym(_addr_ipkg, _addr_s, op);
            if (n.Op != ONONAME)
            {
                if (n.Op == op && (n.Class() != ctxt || !types.Identical(n.Type, t)))
                {
                    redeclare(lineno, s, fmt.Sprintf("during import %q", ipkg.Path));
                }

                return _addr_null!;

            }

            n.Op = op;
            n.Pos = pos;
            n.SetClass(ctxt);
            if (ctxt == PFUNC)
            {
                n.Sym.SetFunc(true);
            }

            n.Type = t;
            return _addr_n!;

        }

        // importconst declares symbol s as an imported constant with type t and value val.
        // ipkg is the package being imported
        private static void importconst(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, Val val)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var n = importobj(_addr_ipkg, pos, _addr_s, OLITERAL, PEXTERN, _addr_t);
            if (n == null)
            { // TODO: Check that value matches.
                return ;

            }

            n.SetVal(val);

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import const %v %L = %v\n", s, t, val);
            }

        }

        // importfunc declares symbol s as an imported function with type t.
        // ipkg is the package being imported
        private static void importfunc(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var n = importobj(_addr_ipkg, pos, _addr_s, ONAME, PFUNC, _addr_t);
            if (n == null)
            {
                return ;
            }

            n.Func = @new<Func>();
            t.SetNname(asTypesNode(n));

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import func %v%S\n", s, t);
            }

        }

        // importvar declares symbol s as an imported variable with type t.
        // ipkg is the package being imported
        private static void importvar(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var n = importobj(_addr_ipkg, pos, _addr_s, ONAME, PEXTERN, _addr_t);
            if (n == null)
            {
                return ;
            }

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import var %v %L\n", s, t);
            }

        }

        // importalias declares symbol s as an imported type alias with type t.
        // ipkg is the package being imported
        private static void importalias(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t)
        {
            ref types.Pkg ipkg = ref _addr_ipkg.val;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var n = importobj(_addr_ipkg, pos, _addr_s, OTYPE, PEXTERN, _addr_t);
            if (n == null)
            {
                return ;
            }

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import type %v = %L\n", s, t);
            }

        }

        private static void dumpasmhdr()
        {
            var (b, err) = bio.Create(asmhdr);
            if (err != null)
            {
                Fatalf("%v", err);
            }

            fmt.Fprintf(b, "// generated by compile -asmhdr from package %s\n\n", localpkg.Name);
            foreach (var (_, n) in asmlist)
            {
                if (n.Sym.IsBlank())
                {
                    continue;
                }


                if (n.Op == OLITERAL) 
                    var t = n.Val().Ctype();
                    if (t == CTFLT || t == CTCPLX)
                    {
                        break;
                    }

                    fmt.Fprintf(b, "#define const_%s %#v\n", n.Sym.Name, n.Val());
                else if (n.Op == OTYPE) 
                    t = n.Type;
                    if (!t.IsStruct() || t.StructType().Map != null || t.IsFuncArgStruct())
                    {
                        break;
                    }

                    fmt.Fprintf(b, "#define %s__size %d\n", n.Sym.Name, int(t.Width));
                    foreach (var (_, f) in t.Fields().Slice())
                    {
                        if (!f.Sym.IsBlank())
                        {
                            fmt.Fprintf(b, "#define %s_%s %d\n", n.Sym.Name, f.Sym.Name, int(f.Offset));
                        }

                    }
                
            }
            b.Close();

        }
    }
}}}}
