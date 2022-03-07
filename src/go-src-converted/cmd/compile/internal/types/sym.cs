// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:47:54 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\sym.go
using @base = go.cmd.compile.@internal.@base_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

namespace go.cmd.compile.@internal;

public static partial class types_package {

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
public partial struct Sym {
    public @string Linkname; // link name

    public ptr<Pkg> Pkg;
    public @string Name; // object name

// Def, Block, and Lastlineno are saved and restored by Pushdcl/Popdcl.

// The unique ONAME, OTYPE, OPACK, or OLITERAL node that this symbol is
// bound to within the current scope. (Most parts of the compiler should
// prefer passing the Node directly, rather than relying on this field.)
    public Object Def;
    public int Block; // blocknumber to catch redeclaration
    public src.XPos Lastlineno; // last declaration for diagnostic

    public bitset8 flags;
}

private static readonly nint symOnExportList = 1 << (int)(iota); // added to exportlist (no need to add again)
private static readonly var symUniq = 0;
private static readonly var symSiggen = 1; // type symbol has been generated
private static readonly var symAsm = 2; // on asmlist, for writing to -asmhdr
private static readonly var symFunc = 3; // function symbol

private static bool OnExportList(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym.flags & symOnExportList != 0;
}
private static bool Uniq(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym.flags & symUniq != 0;
}
private static bool Siggen(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym.flags & symSiggen != 0;
}
private static bool Asm(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym.flags & symAsm != 0;
}
private static bool Func(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym.flags & symFunc != 0;
}

private static void SetOnExportList(this ptr<Sym> _addr_sym, bool b) {
    ref Sym sym = ref _addr_sym.val;

    sym.flags.set(symOnExportList, b);
}
private static void SetUniq(this ptr<Sym> _addr_sym, bool b) {
    ref Sym sym = ref _addr_sym.val;

    sym.flags.set(symUniq, b);
}
private static void SetSiggen(this ptr<Sym> _addr_sym, bool b) {
    ref Sym sym = ref _addr_sym.val;

    sym.flags.set(symSiggen, b);
}
private static void SetAsm(this ptr<Sym> _addr_sym, bool b) {
    ref Sym sym = ref _addr_sym.val;

    sym.flags.set(symAsm, b);
}
private static void SetFunc(this ptr<Sym> _addr_sym, bool b) {
    ref Sym sym = ref _addr_sym.val;

    sym.flags.set(symFunc, b);
}

private static bool IsBlank(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    return sym != null && sym.Name == "_";
}

// Deprecated: This method should not be used directly. Instead, use a
// higher-level abstraction that directly returns the linker symbol
// for a named object. For example, reflectdata.TypeLinksym(t) instead
// of reflectdata.TypeSym(t).Linksym().
private static ptr<obj.LSym> Linksym(this ptr<Sym> _addr_sym) {
    ref Sym sym = ref _addr_sym.val;

    var abi = obj.ABI0;
    if (sym.Func()) {
        abi = obj.ABIInternal;
    }
    return _addr_sym.LinksymABI(abi)!;

}

// Deprecated: This method should not be used directly. Instead, use a
// higher-level abstraction that directly returns the linker symbol
// for a named object. For example, (*ir.Name).LinksymABI(abi) instead
// of (*ir.Name).Sym().LinksymABI(abi).
private static ptr<obj.LSym> LinksymABI(this ptr<Sym> _addr_sym, obj.ABI abi) {
    ref Sym sym = ref _addr_sym.val;

    if (sym == null) {
        @base.Fatalf("nil symbol");
    }
    if (sym.Linkname != "") {
        return _addr_@base.Linkname(sym.Linkname, abi)!;
    }
    return _addr_@base.PkgLinksym(sym.Pkg.Prefix, sym.Name, abi)!;

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
private static bool Less(this ptr<Sym> _addr_a, ptr<Sym> _addr_b) {
    ref Sym a = ref _addr_a.val;
    ref Sym b = ref _addr_b.val;

    if (a == b) {
        return false;
    }
    var ea = IsExported(a.Name);
    var eb = IsExported(b.Name);
    if (ea != eb) {
        return ea;
    }
    if (a.Name != b.Name) {
        return a.Name < b.Name;
    }
    if (!ea) {
        if (a.Pkg.Height != b.Pkg.Height) {
            return a.Pkg.Height < b.Pkg.Height;
        }
        return a.Pkg.Path < b.Pkg.Path;

    }
    return false;

}

// IsExported reports whether name is an exported Go symbol (that is,
// whether it begins with an upper-case letter).
public static bool IsExported(@string name) {
    {
        var r__prev1 = r;

        var r = name[0];

        if (r < utf8.RuneSelf) {
            return 'A' <= r && r <= 'Z';
        }
        r = r__prev1;

    }

    var (r, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(r);

}

} // end types_package
