// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 09 05:24:13 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Go\src\cmd\compile\internal\types\sym.go
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        // Sym represents an object name in a segmented (pkg, name) namespace.
        // Most commonly, this is a Go identifier naming an object declared within a package,
        // but Syms are also used to name internal synthesized objects.
        //
        // As an exception, field and method names that are exported use the Sym
        // associated with localpkg instead of the package that declared them. This
        // allows using Sym pointer equality to test for Go identifier uniqueness when
        // handling selector expressions.
        //
        // Ideally, Sym should be used for representing Go language constructs,
        // while cmd/internal/obj.LSym is used for representing emitted artifacts.
        //
        // NOTE: In practice, things can be messier than the description above
        // for various reasons (historical, convenience).
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

        private static readonly long symOnExportList = (long)1L << (int)(iota); // added to exportlist (no need to add again)
        private static readonly var symUniq = 0;
        private static readonly var symSiggen = 1; // type symbol has been generated
        private static readonly var symAsm = 2; // on asmlist, for writing to -asmhdr
        private static readonly var symFunc = 3; // function symbol; uses internal ABI

        private static bool OnExportList(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym.flags & symOnExportList != 0L;
        }
        private static bool Uniq(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym.flags & symUniq != 0L;
        }
        private static bool Siggen(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym.flags & symSiggen != 0L;
        }
        private static bool Asm(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym.flags & symAsm != 0L;
        }
        private static bool Func(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym.flags & symFunc != 0L;
        }

        private static void SetOnExportList(this ptr<Sym> _addr_sym, bool b)
        {
            ref Sym sym = ref _addr_sym.val;

            sym.flags.set(symOnExportList, b);
        }
        private static void SetUniq(this ptr<Sym> _addr_sym, bool b)
        {
            ref Sym sym = ref _addr_sym.val;

            sym.flags.set(symUniq, b);
        }
        private static void SetSiggen(this ptr<Sym> _addr_sym, bool b)
        {
            ref Sym sym = ref _addr_sym.val;

            sym.flags.set(symSiggen, b);
        }
        private static void SetAsm(this ptr<Sym> _addr_sym, bool b)
        {
            ref Sym sym = ref _addr_sym.val;

            sym.flags.set(symAsm, b);
        }
        private static void SetFunc(this ptr<Sym> _addr_sym, bool b)
        {
            ref Sym sym = ref _addr_sym.val;

            sym.flags.set(symFunc, b);
        }

        private static bool IsBlank(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            return sym != null && sym.Name == "_";
        }

        private static @string LinksymName(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

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

        private static ptr<obj.LSym> Linksym(this ptr<Sym> _addr_sym)
        {
            ref Sym sym = ref _addr_sym.val;

            if (sym == null)
            {
                return _addr_null!;
            }

            Action<ptr<obj.LSym>> initPkg = r =>
            {
                if (sym.Linkname != "")
                {
                    r.Pkg = "_";
                }
                else
                {
                    r.Pkg = sym.Pkg.Prefix;
                }

            }
;
            if (sym.Func())
            { 
                // This is a function symbol. Mark it as "internal ABI".
                return _addr_Ctxt.LookupABIInit(sym.LinksymName(), obj.ABIInternal, initPkg)!;

            }

            return _addr_Ctxt.LookupInit(sym.LinksymName(), initPkg)!;

        }

        // Less reports whether symbol a is ordered before symbol b.
        //
        // Symbols are ordered exported before non-exported, then by name, and
        // finally (for non-exported symbols) by package height and path.
        //
        // Ordering by package height is necessary to establish a consistent
        // ordering for non-exported names with the same spelling but from
        // different packages. We don't necessarily know the path for the
        // package being compiled, but by definition it will have a height
        // greater than any other packages seen within the compilation unit.
        // For more background, see issue #24693.
        private static bool Less(this ptr<Sym> _addr_a, ptr<Sym> _addr_b)
        {
            ref Sym a = ref _addr_a.val;
            ref Sym b = ref _addr_b.val;

            if (a == b)
            {
                return false;
            } 

            // Exported symbols before non-exported.
            var ea = IsExported(a.Name);
            var eb = IsExported(b.Name);
            if (ea != eb)
            {
                return ea;
            } 

            // Order by name and then (for non-exported names) by package
            // height and path.
            if (a.Name != b.Name)
            {
                return a.Name < b.Name;
            }

            if (!ea)
            {
                if (a.Pkg.Height != b.Pkg.Height)
                {
                    return a.Pkg.Height < b.Pkg.Height;
                }

                return a.Pkg.Path < b.Pkg.Path;

            }

            return false;

        }

        // IsExported reports whether name is an exported Go symbol (that is,
        // whether it begins with an upper-case letter).
        public static bool IsExported(@string name)
        {
            {
                var r__prev1 = r;

                var r = name[0L];

                if (r < utf8.RuneSelf)
                {
                    return 'A' <= r && r <= 'Z';
                }

                r = r__prev1;

            }

            var (r, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(r);

        }
    }
}}}}
