// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:53:14 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Go\src\cmd\compile\internal\types\sym.go
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        // Sym represents an object name. Most commonly, this is a Go identifier naming
        // an object declared within a package, but Syms are also used to name internal
        // synthesized objects.
        //
        // As an exception, field and method names that are exported use the Sym
        // associated with localpkg instead of the package that declared them. This
        // allows using Sym pointer equality to test for Go identifier uniqueness when
        // handling selector expressions.
        public partial struct Sym
        {
            public ptr<Pkg> Importdef; // where imported definition was found
            public @string Linkname; // link name

            public ptr<Pkg> Pkg;
            public @string Name; // object name

// saved and restored by dcopy
            public ptr<Node> Def; // definition: ONAME OTYPE OPACK or OLITERAL
            public int Block; // blocknumber to catch redeclaration
            public src.XPos Lastlineno; // last declaration for diagnostic

            public bitset8 flags;
            public ptr<Node> Label; // corresponding label (ephemeral)
            public ptr<Pkg> Origpkg; // original package for . import
        }

        private static readonly long symExport = 1L << (int)(iota); // added to exportlist (no need to add again)
        private static readonly var symPackage = 0;
        private static readonly var symExported = 1; // already written out by export
        private static readonly var symUniq = 2;
        private static readonly var symSiggen = 3;
        private static readonly var symAsm = 4;
        private static readonly var symAlgGen = 5;

        private static bool Export(this ref Sym sym)
        {
            return sym.flags & symExport != 0L;
        }
        private static bool Package(this ref Sym sym)
        {
            return sym.flags & symPackage != 0L;
        }
        private static bool Exported(this ref Sym sym)
        {
            return sym.flags & symExported != 0L;
        }
        private static bool Uniq(this ref Sym sym)
        {
            return sym.flags & symUniq != 0L;
        }
        private static bool Siggen(this ref Sym sym)
        {
            return sym.flags & symSiggen != 0L;
        }
        private static bool Asm(this ref Sym sym)
        {
            return sym.flags & symAsm != 0L;
        }
        private static bool AlgGen(this ref Sym sym)
        {
            return sym.flags & symAlgGen != 0L;
        }

        private static void SetExport(this ref Sym sym, bool b)
        {
            sym.flags.set(symExport, b);

        }
        private static void SetPackage(this ref Sym sym, bool b)
        {
            sym.flags.set(symPackage, b);

        }
        private static void SetExported(this ref Sym sym, bool b)
        {
            sym.flags.set(symExported, b);

        }
        private static void SetUniq(this ref Sym sym, bool b)
        {
            sym.flags.set(symUniq, b);

        }
        private static void SetSiggen(this ref Sym sym, bool b)
        {
            sym.flags.set(symSiggen, b);

        }
        private static void SetAsm(this ref Sym sym, bool b)
        {
            sym.flags.set(symAsm, b);

        }
        private static void SetAlgGen(this ref Sym sym, bool b)
        {
            sym.flags.set(symAlgGen, b);

        }

        private static bool IsBlank(this ref Sym sym)
        {
            return sym != null && sym.Name == "_";
        }

        private static @string LinksymName(this ref Sym sym)
        {
            if (sym.IsBlank())
            {
                return "_";
            }
            if (sym.Linkname != "")
            {
                return sym.Linkname;
            }
            return sym.Pkg.Prefix + "." + sym.Name;
        }

        private static ref obj.LSym Linksym(this ref Sym sym)
        {
            if (sym == null)
            {
                return null;
            }
            return Ctxt.Lookup(sym.LinksymName());
        }
    }
}}}}
