// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements (error and trace) message formatting support.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using strconv = strconv_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;

partial class types_package {

internal static @string sprintf(ж<token.FileSet> Ꮡfset, Qualifier qf, bool tpSubscripts, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var fset = ref Ꮡfset.val;
    foreach (var (i, arg) in args) {
        switch (arg.type()) {
        case default! a: {
            arg = "<nil>"u8;
            break;
        }
        case operand a: {
            throw panic("got operand instead of *operand");
            break;
        }
        case operand.val a: {
            arg = operandString(a, qf);
            break;
        }
        case tokenꓸPos a: {
            if (fset != nil) {
                arg = fset.Position(a).String();
            }
            break;
        }
        case ast.Expr a: {
            arg = ExprString(a);
            break;
        }
        case slice<ast.Expr> a: {
            ref var bufΔ1 = ref heap(new bytes_package.Buffer(), out var ᏑbufΔ1);
            bufΔ1.WriteByte((rune)'[');
            writeExprList(ᏑbufΔ1, a);
            bufΔ1.WriteByte((rune)']');
            arg = bufΔ1.String();
            break;
        }
        case Object a: {
            arg = ObjectString(a, qf);
            break;
        }
        case ΔType a: {
            ref var bufΔ2 = ref heap(new bytes_package.Buffer(), out var ᏑbufΔ2);
            var w = newTypeWriter(ᏑbufΔ2, qf);
            w.val.tpSubscripts = tpSubscripts;
            w.typ(a);
            arg = bufΔ2.String();
            break;
        }
        case slice<ΔType> a: {
            ref var bufΔ3 = ref heap(new bytes_package.Buffer(), out var ᏑbufΔ3);
            w = newTypeWriter(ᏑbufΔ3, qf);
            w.val.tpSubscripts = tpSubscripts;
            bufΔ3.WriteByte((rune)'[');
            foreach (var (iΔ1, x) in a) {
                if (iΔ1 > 0) {
                    bufΔ3.WriteString(", "u8);
                }
                w.typ(x);
            }
            bufΔ3.WriteByte((rune)']');
            arg = bufΔ3.String();
            break;
        }
        case slice<ж<TypeParam>> a: {
            ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
            w = newTypeWriter(Ꮡbuf, qf);
            w.val.tpSubscripts = tpSubscripts;
            buf.WriteByte((rune)'[');
            foreach (var (iΔ2, x) in a) {
                if (iΔ2 > 0) {
                    buf.WriteString(", "u8);
                }
                w.typ(~x);
            }
            buf.WriteByte((rune)']');
            arg = buf.String();
            break;
        }}
        args[i] = arg;
    }
    return fmt.Sprintf(format, args.ꓸꓸꓸ);
}

// check may be nil.
[GoRecv] internal static @string sprintf(this ref Checker check, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ж<token.FileSet> fset = default!;
    Qualifier qf = default!;
    if (check != nil) {
        fset = check.fset;
        qf = () => check.qualifier();
    }
    return sprintf(fset, qf, false, format, args.ꓸꓸꓸ);
}

[GoRecv] internal static void trace(this ref Checker check, tokenꓸPos pos, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    fmt.Printf("%s:\t%s%s\n"u8,
        check.fset.Position(pos),
        strings.Repeat(".  "u8, check.indent),
        sprintf(check.fset, check.qualifier, true, format, args.ꓸꓸꓸ));
}

// dump is only needed for debugging
[GoRecv] internal static void dump(this ref Checker check, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    fmt.Println(sprintf(check.fset, check.qualifier, true, format, args.ꓸꓸꓸ));
}

[GoRecv] public static @string qualifier(this ref Checker check, ж<Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    // Qualify the package unless it's the package being type-checked.
    if (Ꮡpkg != check.pkg) {
        if (check.pkgPathMap == default!) {
            check.pkgPathMap = new map<@string, map<@string, bool>>();
            check.seenPkgMap = new map<ж<Package>, bool>();
            check.markImports(check.pkg);
        }
        // If the same package name was used by multiple packages, display the full path.
        if (len(check.pkgPathMap[pkg.name]) > 1) {
            return strconv.Quote(pkg.path);
        }
        return pkg.name;
    }
    return ""u8;
}

// markImports recursively walks pkg and its imports, to record unique import
// paths in pkgPathMap.
[GoRecv] public static void markImports(this ref Checker check, ж<Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    if (check.seenPkgMap[pkg]) {
        return;
    }
    check.seenPkgMap[pkg] = true;
    var forName = check.pkgPathMap[pkg.name];
    var ok = check.pkgPathMap[pkg.name];
    if (!ok) {
        forName = new map<@string, bool>();
        check.pkgPathMap[pkg.name] = forName;
    }
    forName[pkg.path] = true;
    foreach (var (_, imp) in pkg.imports) {
        check.markImports(imp);
    }
}

// stripAnnotations removes internal (type) annotations from s.
internal static @string stripAnnotations(@string s) {
    strings.Builder buf = default!;
    foreach (var (_, r) in s) {
        // strip #'s and subscript digits
        if (r < (rune)'₀' || (rune)'₀' + 10 <= r) {
            // '₀' == U+2080
            buf.WriteRune(r);
        }
    }
    if (buf.Len() < len(s)) {
        return buf.String();
    }
    return s;
}

} // end types_package
