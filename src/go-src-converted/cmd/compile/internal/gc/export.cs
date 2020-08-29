// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:26:56 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\export.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        public static long Debug_export = default;

        private static void exportf(ref bio.Writer bout, @string format, params object[] args)
        {
            args = args.Clone();

            fmt.Fprintf(bout, format, args);
            if (Debug_export != 0L)
            {
                fmt.Printf(format, args);
            }
        }

        private static slice<ref Node> asmlist = default;

        // Mark n's symbol as exported
        private static void exportsym(ref Node n)
        {
            if (n == null || n.Sym == null)
            {
                return;
            }
            if (n.Sym.Export() || n.Sym.Package())
            {
                if (n.Sym.Package())
                {
                    Fatalf("export/package mismatch: %v", n.Sym);
                }
                return;
            }
            n.Sym.SetExport(true);
            if (Debug['E'] != 0L)
            {
                fmt.Printf("export symbol %v\n", n.Sym);
            } 

            // Ensure original types are on exportlist before type aliases.
            if (IsAlias(n.Sym))
            {
                exportlist = append(exportlist, asNode(n.Sym.Def));
            }
            exportlist = append(exportlist, n);
        }

        private static bool exportname(@string s)
        {
            {
                var r__prev1 = r;

                var r = s[0L];

                if (r < utf8.RuneSelf)
                {
                    return 'A' <= r && r <= 'Z';
                }

                r = r__prev1;

            }
            var (r, _) = utf8.DecodeRuneInString(s);
            return unicode.IsUpper(r);
        }

        private static bool initname(@string s)
        {
            return s == "init";
        }

        // exportedsym reports whether a symbol will be visible
        // to files that import our package.
        private static bool exportedsym(ref types.Sym sym)
        { 
            // Builtins are visible everywhere.
            if (sym.Pkg == builtinpkg || sym.Origpkg == builtinpkg)
            {
                return true;
            }
            return sym.Pkg == localpkg && exportname(sym.Name);
        }

        private static void autoexport(ref Node n, Class ctxt)
        {
            if (n == null || n.Sym == null)
            {
                return;
            }
            if ((ctxt != PEXTERN && ctxt != PFUNC) || dclcontext != PEXTERN)
            {
                return;
            }
            if (n.Type != null && n.Type.IsKind(TFUNC) && n.IsMethod())
            {
                return;
            }
            if (exportname(n.Sym.Name) || initname(n.Sym.Name))
            {
                exportsym(n);
            }
            if (asmhdr != "" && n.Sym.Pkg == localpkg && !n.Sym.Asm())
            {
                n.Sym.SetAsm(true);
                asmlist = append(asmlist, n);
            }
        }

        // Look for anything we need for the inline body
        private static void reexportdeplist(Nodes ll)
        {
            foreach (var (_, n) in ll.Slice())
            {
                reexportdep(n);
            }
        }

        private static void reexportdep(ref Node n)
        {
            if (n == null)
            {
                return;
            } 

            //print("reexportdep %+hN\n", n);

            if (n.Op == ONAME)
            {

                if (n.Class() == PFUNC) 
                {
                    // methods will be printed along with their type
                    // nodes for T.Method expressions
                    if (n.isMethodExpression())
                    {
                        break;
                    } 

                    // nodes for method calls.
                    if (n.Type == null || n.IsMethod())
                    {
                        break;
                    }
                    fallthrough = true;

                }
                if (fallthrough || n.Class() == PEXTERN)
                {
                    if (n.Sym != null && !exportedsym(n.Sym))
                    {
                        if (Debug['E'] != 0L)
                        {
                            fmt.Printf("reexport name %v\n", n.Sym);
                        }
                        exportlist = append(exportlist, n);
                    }
                    goto __switch_break0;
                }

                __switch_break0:; 

                // Local variables in the bodies need their type.
                goto __switch_break1;
            }
            if (n.Op == ODCL)
            {
                var t = n.Left.Type;

                if (t != types.Types[t.Etype] && t != types.Idealbool && t != types.Idealstring)
                {
                    if (t.IsPtr())
                    {
                        t = t.Elem();
                    }
                    if (t != null && t.Sym != null && t.Sym.Def != null && !exportedsym(t.Sym))
                    {
                        if (Debug['E'] != 0L)
                        {
                            fmt.Printf("reexport type %v from declaration\n", t.Sym);
                        }
                        exportlist = append(exportlist, asNode(t.Sym.Def));
                    }
                }
                goto __switch_break1;
            }
            if (n.Op == OLITERAL)
            {
                t = n.Type;
                if (t != types.Types[n.Type.Etype] && t != types.Idealbool && t != types.Idealstring)
                {
                    if (t.IsPtr())
                    {
                        t = t.Elem();
                    }
                    if (t != null && t.Sym != null && t.Sym.Def != null && !exportedsym(t.Sym))
                    {
                        if (Debug['E'] != 0L)
                        {
                            fmt.Printf("reexport literal type %v\n", t.Sym);
                        }
                        exportlist = append(exportlist, asNode(t.Sym.Def));
                    }
                }
                fallthrough = true;

            }
            if (fallthrough || n.Op == OTYPE)
            {
                if (n.Sym != null && n.Sym.Def != null && !exportedsym(n.Sym))
                {
                    if (Debug['E'] != 0L)
                    {
                        fmt.Printf("reexport literal/type %v\n", n.Sym);
                    }
                    exportlist = append(exportlist, n);
                } 

                // for operations that need a type when rendered, put the type on the export list.
                goto __switch_break1;
            }
            if (n.Op == OCONV || n.Op == OCONVIFACE || n.Op == OCONVNOP || n.Op == ORUNESTR || n.Op == OARRAYBYTESTR || n.Op == OARRAYRUNESTR || n.Op == OSTRARRAYBYTE || n.Op == OSTRARRAYRUNE || n.Op == ODOTTYPE || n.Op == ODOTTYPE2 || n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OPTRLIT || n.Op == OMAKEMAP || n.Op == OMAKESLICE || n.Op == OMAKECHAN)
            {
                t = n.Type;


                if (t.Etype == TARRAY || t.Etype == TCHAN || t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TSLICE) 
                    if (t.Sym == null)
                    {
                        t = t.Elem();
                    }
                                if (t != null && t.Sym != null && t.Sym.Def != null && !exportedsym(t.Sym))
                {
                    if (Debug['E'] != 0L)
                    {
                        fmt.Printf("reexport type for expression %v\n", t.Sym);
                    }
                    exportlist = append(exportlist, asNode(t.Sym.Def));
                }
                goto __switch_break1;
            }

            __switch_break1:;

            reexportdep(n.Left);
            reexportdep(n.Right);
            reexportdeplist(n.List);
            reexportdeplist(n.Rlist);
            reexportdeplist(n.Ninit);
            reexportdeplist(n.Nbody);
        }

        // methodbyname sorts types by symbol name.
        private partial struct methodbyname // : slice<ref types.Field>
        {
        }

        private static long Len(this methodbyname x)
        {
            return len(x);
        }
        private static void Swap(this methodbyname x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }
        private static bool Less(this methodbyname x, long i, long j)
        {
            return x[i].Sym.Name < x[j].Sym.Name;
        }

        private static void dumpexport(ref bio.Writer bout)
        {
            if (buildid != "")
            {
                exportf(bout, "build id %q\n", buildid);
            }
            long size = 0L; // size of export section without enclosing markers
            // The linker also looks for the $$ marker - use char after $$ to distinguish format.
            exportf(bout, "\n$$B\n"); // indicate binary export format
            if (debugFormat)
            { 
                // save a copy of the export data
                bytes.Buffer copy = default;
                var bcopy = bufio.NewWriter(ref copy);
                size = export(bcopy, Debug_export != 0L);
                bcopy.Flush(); // flushing to bytes.Buffer cannot fail
                {
                    var (n, err) = bout.Write(copy.Bytes());

                    if (n != size || err != null)
                    {
                        Fatalf("error writing export data: got %d bytes, want %d bytes, err = %v", n, size, err);
                    } 
                    // export data must contain no '$' so that we can find the end by searching for "$$"
                    // TODO(gri) is this still needed?

                } 
                // export data must contain no '$' so that we can find the end by searching for "$$"
                // TODO(gri) is this still needed?
                if (bytes.IndexByte(copy.Bytes(), '$') >= 0L)
                {
                    Fatalf("export data contains $");
                } 

                // verify that we can read the copied export data back in
                // (use empty package map to avoid collisions)
                types.CleanroomDo(() =>
                {
                    Import(types.NewPkg("", ""), bufio.NewReader(ref copy)); // must not die
                }
            else
);
            }            {
                size = export(bout.Writer, Debug_export != 0L);
            }
            exportf(bout, "\n$$\n");

            if (Debug_export != 0L)
            {
                fmt.Printf("export data size = %d bytes\n", size);
            }
        }

        // importsym declares symbol s as an imported object representable by op.
        // pkg is the package being imported
        private static void importsym(ref types.Pkg pkg, ref types.Sym s, Op op)
        {
            if (asNode(s.Def) != null && asNode(s.Def).Op != op)
            {
                var pkgstr = fmt.Sprintf("during import %q", pkg.Path);
                redeclare(s, pkgstr);
            } 

            // mark the symbol so it is not reexported
            if (asNode(s.Def) == null)
            {
                if (exportname(s.Name) || initname(s.Name))
                {
                    s.SetExport(true);
                }
                else
                {
                    s.SetPackage(true); // package scope
                }
            }
        }

        // pkgtype returns the named type declared by symbol s.
        // If no such type has been declared yet, a forward declaration is returned.
        // pkg is the package being imported
        private static ref types.Type pkgtype(src.XPos pos, ref types.Pkg pkg, ref types.Sym s)
        {
            importsym(pkg, s, OTYPE);
            if (asNode(s.Def) == null || asNode(s.Def).Op != OTYPE)
            {
                var t = types.New(TFORW);
                t.Sym = s;
                s.Def = asTypesNode(typenodl(pos, t));
                asNode(s.Def).Name;

                @new<Name>();
            }
            if (asNode(s.Def).Type == null)
            {
                Fatalf("pkgtype %v", s);
            }
            return asNode(s.Def).Type;
        }

        // importconst declares symbol s as an imported constant with type t and value n.
        // pkg is the package being imported
        private static void importconst(ref types.Pkg pkg, ref types.Sym s, ref types.Type t, ref Node n)
        {
            importsym(pkg, s, OLITERAL);
            n = convlit(n, t);

            if (asNode(s.Def) != null)
            { // TODO: check if already the same.
                return;
            }
            if (n.Op != OLITERAL)
            {
                yyerror("expression must be a constant");
                return;
            }
            if (n.Sym != null)
            {
                var n1 = n.Value;
                n = ref n1;
            }
            n.Orig = newname(s);
            n.Sym = s;
            declare(n, PEXTERN);

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import const %v\n", s);
            }
        }

        // importvar declares symbol s as an imported variable with type t.
        // pkg is the package being imported
        private static void importvar(src.XPos pos, ref types.Pkg pkg, ref types.Sym s, ref types.Type t)
        {
            importsym(pkg, s, ONAME);
            if (asNode(s.Def) != null && asNode(s.Def).Op == ONAME)
            {
                if (eqtype(t, asNode(s.Def).Type))
                {
                    return;
                }
                yyerror("inconsistent definition for var %v during import\n\t%v (in %q)\n\t%v (in %q)", s, asNode(s.Def).Type, s.Importdef.Path, t, pkg.Path);
            }
            var n = newnamel(pos, s);
            s.Importdef = pkg;
            n.Type = t;
            declare(n, PEXTERN);

            if (Debug['E'] != 0L)
            {
                fmt.Printf("import var %v %L\n", s, t);
            }
        }

        // importalias declares symbol s as an imported type alias with type t.
        // pkg is the package being imported
        private static void importalias(src.XPos pos, ref types.Pkg pkg, ref types.Sym s, ref types.Type t)
        {
            importsym(pkg, s, OTYPE);
            if (asNode(s.Def) != null && asNode(s.Def).Op == OTYPE)
            {
                if (eqtype(t, asNode(s.Def).Type))
                {
                    return;
                }
                yyerror("inconsistent definition for type alias %v during import\n\t%v (in %q)\n\t%v (in %q)", s, asNode(s.Def).Type, s.Importdef.Path, t, pkg.Path);
            }
            var n = newnamel(pos, s);
            n.Op = OTYPE;
            s.Importdef = pkg;
            n.Type = t;
            declare(n, PEXTERN);

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
                    fmt.Fprintf(b, "#define const_%s %#v\n", n.Sym.Name, n.Val());
                else if (n.Op == OTYPE) 
                    var t = n.Type;
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
