// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using types = go.types_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using scanner = text.scanner_package;
using utf8 = unicode.utf8_package;
using go;
using text;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class gccgoimporter_package {

[GoType] partial struct parser {
    internal ж<text.scanner_package.Scanner> scanner;
    internal @string version;                   // format version
    internal rune tok;                      // current token
    internal @string lit;                   // literal string; only valid for Ident, Int, String tokens
    internal @string pkgpath;                   // package path of imported package
    internal @string pkgname;                   // name of imported package
    internal ж<go.types_package.Package> pkg;         // reference to imported package
    internal types.Package imports; // package path -> package object
    internal typesꓸType typeList;            // type number -> type
    internal slice<@string> typeData;            // unparsed type data (v3 and later)
    internal slice<fixupRecord> fixups;        // fixups to apply at end of parsing
    internal InitData initdata;                  // package init priority data
    internal map<nint, @string> aliases;        // maps saved type number to alias name
}

// When reading export data it's possible to encounter a defined type
// N1 with an underlying defined type N2 while we are still reading in
// that defined type N2; see issues #29006 and #29198 for instances
// of this. Example:
//
//   type N1 N2
//   type N2 struct {
//      ...
//      p *N1
//   }
//
// To handle such cases, the parser generates a fixup record (below) and
// delays setting of N1's underlying type until parsing is complete, at
// which point fixups are applied.
[GoType] partial struct fixupRecord {
    internal ж<go.types_package.Named> toUpdate; // type to modify when fixup is processed
    internal go.types_package.ΔType target; // type that was incomplete when fixup was created
}

[GoRecv] internal static void init(this ref parser p, @string filename, io.Reader src, types.Package imports) {
    p.scanner = @new<scanner.Scanner>();
    p.initScanner(filename, src);
    p.imports = imports;
    p.aliases = new map<nint, @string>();
    p.typeList = new slice<typesꓸType>(1, /* type numbers start at 1 */
 16);
}

[GoRecv] internal static void initScanner(this ref parser p, @string filename, io.Reader src) {
    p.scanner.Init(src);
    p.scanner.Error = (ж<scanner.Scanner> _, @string msg) => {
        p.error(msg);
    };
    p.scanner.Mode = (nuint)((UntypedInt)((UntypedInt)(scanner.ScanIdents | scanner.ScanInts) | scanner.ScanFloats) | scanner.ScanStrings);
    p.scanner.Whitespace = (uint64)(1 << (int)((rune)'\t') | 1 << (int)((rune)' '));
    p.scanner.Filename = filename;
    // for good error messages
    p.next();
}

[GoType] partial struct importError {
    internal text.scanner_package.Position pos;
    internal error err;
}

internal static @string Error(this importError e) {
    return fmt.Sprintf("import error %s (byte offset = %d): %s"u8, e.pos, e.pos.Offset, e.err);
}

[GoRecv] internal static void error(this ref parser p, any err) {
    {
        var (s, ok) = err._<@string>(ᐧ); if (ok) {
            err = errors.New(s);
        }
    }
    // panic with a runtime.Error if err is not an error
    throw panic(new importError(p.scanner.Pos(), err._<error>()));
}

[GoRecv] internal static void errorf(this ref parser p, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    p.error(fmt.Errorf(format, args.ꓸꓸꓸ));
}

[GoRecv] internal static @string expect(this ref parser p, rune tok) {
    @string lit = p.lit;
    if (p.tok != tok) {
        p.errorf("expected %s, got %s (%s)"u8, scanner.TokenString(tok), scanner.TokenString(p.tok), lit);
    }
    p.next();
    return lit;
}

[GoRecv] internal static void expectEOL(this ref parser p) {
    if (p.version == "v1"u8 || p.version == "v2"u8) {
        p.expect((rune)';');
    }
    p.expect((rune)'\n');
}

[GoRecv] internal static void expectKeyword(this ref parser p, @string keyword) {
    @string lit = p.expect(scanner.Ident);
    if (lit != keyword) {
        p.errorf("expected keyword %s, got %q"u8, keyword, lit);
    }
}

[GoRecv] internal static @string parseString(this ref parser p) {
    var (str, err) = strconv.Unquote(p.expect(scanner.ΔString));
    if (err != default!) {
        p.error(err);
    }
    return str;
}

// unquotedString     = { unquotedStringChar } .
// unquotedStringChar = <neither a whitespace nor a ';' char> .
[GoRecv] internal static @string parseUnquotedString(this ref parser p) {
    if (p.tok == scanner.EOF) {
        p.error("unexpected EOF");
    }
    strings.Builder b = default!;
    b.WriteString(p.scanner.TokenText());
    // This loop needs to examine each character before deciding whether to consume it. If we see a semicolon,
    // we need to let it be consumed by p.next().
    for (var ch = p.scanner.Peek(); ch != (rune)'\n' && ch != (rune)';' && ch != scanner.EOF && (uint64)(p.scanner.Whitespace & (1 << (int)(((nuint)ch)))) == 0; ch = p.scanner.Peek()) {
        b.WriteRune(ch);
        p.scanner.Next();
    }
    p.next();
    return b.String();
}

[GoRecv] internal static void next(this ref parser p) {
    p.tok = p.scanner.Scan();
    switch (p.tok) {
    case scanner.Ident or scanner.Int or scanner.Float or scanner.ΔString or (rune)'·': {
        p.lit = p.scanner.TokenText();
        break;
    }
    default: {
        p.lit = ""u8;
        break;
    }}

}

[GoRecv] internal static (@string path, @string name) parseQualifiedName(this ref parser p) {
    @string path = default!;
    @string name = default!;

    return p.parseQualifiedNameStr(p.parseString());
}

[GoRecv] internal static (@string path, @string name) parseUnquotedQualifiedName(this ref parser p) {
    @string path = default!;
    @string name = default!;

    return p.parseQualifiedNameStr(p.parseUnquotedString());
}

// qualifiedName = [ ["."] unquotedString "." ] unquotedString .
//
// The above production uses greedy matching.
[GoRecv] internal static (@string pkgpath, @string name) parseQualifiedNameStr(this ref parser p, @string unquotedName) {
    @string pkgpath = default!;
    @string name = default!;

    var parts = strings.Split(unquotedName, "."u8);
    if (parts[0] == "") {
        parts = parts[1..];
    }
    switch (len(parts)) {
    case 0: {
        p.errorf("malformed qualified name: %q"u8, unquotedName);
        break;
    }
    case 1: {
        pkgpath = p.pkgpath;
        name = parts[0];
        break;
    }
    default: {
        pkgpath = strings.Join(parts[0..(int)(len(parts) - 1)], // unqualified name
 // qualified name, which may contain periods
 "."u8);
        name = parts[len(parts) - 1];
        break;
    }}

    return (pkgpath, name);
}

// getPkg returns the package for a given path. If the package is
// not found but we have a package name, create the package and
// add it to the p.imports map.
[GoRecv] internal static ж<types.Package> getPkg(this ref parser p, @string pkgpath, @string name) {
    // package unsafe is not in the imports map - handle explicitly
    if (pkgpath == "unsafe"u8) {
        return types.Unsafe;
    }
    var pkg = p.imports[pkgpath];
    if (pkg == nil && name != ""u8) {
        pkg = types.NewPackage(pkgpath, name);
        p.imports[pkgpath] = pkg;
    }
    return pkg;
}

// parseExportedName is like parseQualifiedName, but
// the package path is resolved to an imported *types.Package.
//
// ExportedName = string [string] .
[GoRecv] internal static (ж<types.Package> pkg, @string name) parseExportedName(this ref parser p) {
    ж<types.Package> pkg = default!;
    @string name = default!;

    var (path, name) = p.parseQualifiedName();
    @string pkgname = default!;
    if (p.tok == scanner.ΔString) {
        pkgname = p.parseString();
    }
    pkg = p.getPkg(path, pkgname);
    if (pkg == nil) {
        p.errorf("package %s (path = %q) not found"u8, name, path);
    }
    return (pkg, name);
}

// Name = QualifiedName | "?" .
[GoRecv] internal static @string parseName(this ref parser p) {
    if (p.tok == (rune)'?') {
        // Anonymous.
        p.next();
        return ""u8;
    }
    // The package path is redundant for us. Don't try to parse it.
    var (_, name) = p.parseUnquotedQualifiedName();
    return name;
}

internal static typesꓸType deref(typesꓸType typ) {
    {
        var (p, _) = typ._<ж<types.Pointer>>(ᐧ); if (p != nil) {
            typ = p.Elem();
        }
    }
    return typ;
}

// Field = Name Type [string] .
[GoRecv] internal static (ж<types.Var> field, @string tag) parseField(this ref parser p, ж<types.Package> Ꮡpkg) {
    ж<types.Var> field = default!;
    @string tag = default!;

    ref var pkg = ref Ꮡpkg.val;
    @string name = p.parseName();
    var (typ, n) = p.parseTypeExtended(Ꮡpkg);
    var anon = false;
    if (name == ""u8) {
        anon = true;
        // Alias?
        {
            @string aname = p.aliases[n];
            var ok = p.aliases[n]; if (ok){
                name = aname;
            } else {
                switch (deref(typ).type()) {
                case ж<types.Basic> typ: {
                    name = typ.Name();
                    break;
                }
                case ж<types.Named> typ: {
                    name = typ.Obj().Name();
                    break;
                }
                default: {
                    var typ = deref(typ).type();
                    p.error("embedded field expected");
                    break;
                }}
            }
        }
    }
    field = types.NewField(token.NoPos, Ꮡpkg, name, typ, anon);
    if (p.tok == scanner.ΔString) {
        tag = p.parseString();
    }
    return (field, tag);
}

// Param = Name ["..."] Type .
[GoRecv] internal static (ж<types.Var> param, bool isVariadic) parseParam(this ref parser p, ж<types.Package> Ꮡpkg) {
    ж<types.Var> param = default!;
    bool isVariadic = default!;

    ref var pkg = ref Ꮡpkg.val;
    @string name = p.parseName();
    // Ignore names invented for inlinable functions.
    if (strings.HasPrefix(name, "p."u8) || strings.HasPrefix(name, "r."u8) || strings.HasPrefix(name, "$ret"u8)) {
        name = ""u8;
    }
    if (p.tok == (rune)'<' && p.scanner.Peek() == (rune)'e') {
        // EscInfo = "<esc:" int ">" . (optional and ignored)
        p.next();
        p.expectKeyword("esc"u8);
        p.expect((rune)':');
        p.expect(scanner.Int);
        p.expect((rune)'>');
    }
    if (p.tok == (rune)'.') {
        p.next();
        p.expect((rune)'.');
        p.expect((rune)'.');
        isVariadic = true;
    }
    var typ = p.parseType(Ꮡpkg);
    if (isVariadic) {
        typ = ~types.NewSlice(typ);
    }
    param = types.NewParam(token.NoPos, Ꮡpkg, name, typ);
    return (param, isVariadic);
}

// Var = Name Type .
[GoRecv] internal static ж<types.Var> parseVar(this ref parser p, ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    @string name = p.parseName();
    var v = types.NewVar(token.NoPos, Ꮡpkg, name, p.parseType(Ꮡpkg));
    if (name[0] == (rune)'.' || name[0] == (rune)'<') {
        // This is an unexported variable,
        // or a variable defined in a different package.
        // We only want to record exported variables.
        return default!;
    }
    return v;
}

// Conversion = "convert" "(" Type "," ConstValue ")" .
[GoRecv] internal static (constant.Value val, typesꓸType typ) parseConversion(this ref parser p, ж<types.Package> Ꮡpkg) {
    constant.Value val = default!;
    typesꓸType typ = default!;

    ref var pkg = ref Ꮡpkg.val;
    p.expectKeyword("convert"u8);
    p.expect((rune)'(');
    typ = p.parseType(Ꮡpkg);
    p.expect((rune)',');
    (val, _) = p.parseConstValue(Ꮡpkg);
    p.expect((rune)')');
    return (val, typ);
}

// ConstValue     = string | "false" | "true" | ["-"] (int ["'"] | FloatOrComplex) | Conversion .
// FloatOrComplex = float ["i" | ("+"|"-") float "i"] .
[GoRecv] internal static (constant.Value val, typesꓸType typ) parseConstValue(this ref parser p, ж<types.Package> Ꮡpkg) {
    constant.Value val = default!;
    typesꓸType typ = default!;

    ref var pkg = ref Ꮡpkg.val;
    // v3 changed to $false, $true, $convert, to avoid confusion
    // with variable names in inline function bodies.
    if (p.tok == (rune)'$') {
        p.next();
        if (p.tok != scanner.Ident) {
            p.errorf("expected identifier after '$', got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        }
    }
    switch (p.tok) {
    case scanner.ΔString: {
        @string str = p.parseString();
        val = constant.MakeString(str);
        typ = ~types.Typ[types.UntypedString];
        return (val, typ);
    }
    case scanner.Ident: {
        var b = false;
        var exprᴛ1 = p.lit;
        if (exprᴛ1 == "false"u8) {
        }
        else if (exprᴛ1 == "true"u8) {
            b = true;
        }
        else if (exprᴛ1 == "convert"u8) {
            return p.parseConversion(Ꮡpkg);
        }
        { /* default: */
            p.errorf("expected const value, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        }

        p.next();
        val = constant.MakeBool(b);
        typ = ~types.Typ[types.UntypedBool];
        return (val, typ);
    }}

    @string sign = ""u8;
    if (p.tok == (rune)'-') {
        p.next();
        sign = "-"u8;
    }
    switch (p.tok) {
    case scanner.Int: {
        val = constant.MakeFromLiteral(sign + p.lit, token.INT, 0);
        if (val == default!) {
            p.error("could not parse integer literal");
        }
        p.next();
        if (p.tok == (rune)'\''){
            p.next();
            typ = ~types.Typ[types.UntypedRune];
        } else {
            typ = ~types.Typ[types.ΔUntypedInt];
        }
        break;
    }
    case scanner.Float: {
        @string re = sign + p.lit;
        p.next();
        @string im = default!;
        switch (p.tok) {
        case (rune)'+': {
            p.next();
            im = p.expect(scanner.Float);
            break;
        }
        case (rune)'-': {
            p.next();
            im = "-"u8 + p.expect(scanner.Float);
            break;
        }
        case scanner.Ident: {
            im = re;
            re = "0"u8;
            break;
        }
        default: {
            val = constant.MakeFromLiteral(re, // re is in fact the imaginary component. Expect "i" below.
 token.FLOAT, 0);
            if (val == default!) {
                p.error("could not parse float literal");
            }
            typ = ~types.Typ[types.ΔUntypedFloat];
            return (val, typ);
        }}

        p.expectKeyword("i"u8);
        var reval = constant.MakeFromLiteral(re, token.FLOAT, 0);
        if (reval == default!) {
            p.error("could not parse real component of complex literal");
        }
        var imval = constant.MakeFromLiteral(im + "i"u8, token.IMAG, 0);
        if (imval == default!) {
            p.error("could not parse imag component of complex literal");
        }
        val = constant.BinaryOp(reval, token.ADD, imval);
        typ = ~types.Typ[types.ΔUntypedComplex];
        break;
    }
    default: {
        p.errorf("expected const value, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        break;
    }}

    return (val, typ);
}

// Const = Name [Type] "=" ConstValue .
[GoRecv] internal static ж<types.Const> parseConst(this ref parser p, ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    @string name = p.parseName();
    typesꓸType typ = default!;
    if (p.tok == (rune)'<') {
        typ = p.parseType(Ꮡpkg);
    }
    p.expect((rune)'=');
    (val, vtyp) = p.parseConstValue(Ꮡpkg);
    if (typ == default!) {
        typ = vtyp;
    }
    return types.NewConst(token.NoPos, Ꮡpkg, name, typ, val);
}

// reserved is a singleton type used to fill type map slots that have
// been reserved (i.e., for which a type number has been parsed) but
// which don't have their actual type yet. When the type map is updated,
// the actual type must replace a reserved entry (or we have an internal
// error). Used for self-verification only - not required for correctness.

    [GoType("dyn")] partial struct  {
        public partial ref go.types_package.ΔType Type { get; }
    }
internal static ж<types.Type}> reserved = @new<>();

// reserve reserves the type map entry n for future use.
[GoRecv] internal static void reserve(this ref parser p, nint n) {
    // Notes:
    // - for pre-V3 export data, the type numbers we see are
    //   guaranteed to be in increasing order, so we append a
    //   reserved entry onto the list.
    // - for V3+ export data, type numbers can appear in
    //   any order, however the 'types' section tells us the
    //   total number of types, hence typeList is pre-allocated.
    if (len(p.typeData) == 0){
        if (n != len(p.typeList)) {
            p.errorf("invalid type number %d (out of sync)"u8, n);
        }
        p.typeList = append(p.typeList, ~reserved);
    } else {
        if (p.typeList[n] != default!) {
            p.errorf("previously visited type number %d"u8, n);
        }
        p.typeList[n] = reserved;
    }
}

// update sets the type map entries for the entries in nlist to t.
// An entry in nlist can be a type number in p.typeList,
// used to resolve named types, or it can be a *types.Pointer,
// used to resolve pointers to named types in case they are referenced
// by embedded fields.
[GoRecv] internal static void update(this ref parser p, typesꓸType t, slice<any> nlist) {
    if (Ꮡt == ~reserved) {
        p.errorf("internal error: update(%v) invoked on reserved"u8, nlist);
    }
    if (t == default!) {
        p.errorf("internal error: update(%v) invoked on nil"u8, nlist);
    }
    foreach (var (_, n) in nlist) {
        switch (n.type()) {
        case nint n: {
            if (AreEqual(p.typeList[n], t)) {
                continue;
            }
            if (p.typeList[n] != ~reserved) {
                p.errorf("internal error: update(%v): %d not reserved"u8, nlist, n);
            }
            p.typeList[n] = t;
            break;
        }
        case int32 n: {
            if (AreEqual(p.typeList[n], t)) {
                continue;
            }
            if (p.typeList[n] != ~reserved) {
                p.errorf("internal error: update(%v): %d not reserved"u8, nlist, n);
            }
            p.typeList[n] = t;
            break;
        }
        case ж<types.Pointer> n: {
            if (n.val != (new types.Pointer(nil))) {
                var elem = n.Elem();
                if (AreEqual(elem, t)) {
                    continue;
                }
                p.errorf("internal error: update: pointer already set to %v, expected %v"u8, elem, t);
            }
            n.val = types.NewPointer(t).val;
            break;
        }
        default: {
            var n = n.type();
            p.errorf("internal error: %T on nlist"u8, n);
            break;
        }}
    }
}

// NamedType = TypeName [ "=" ] Type { Method } .
// TypeName  = ExportedName .
// Method    = "func" "(" Param ")" Name ParamList ResultList [InlineBody] ";" .
[GoRecv] internal static typesꓸType parseNamedType(this ref parser p, slice<any> nlist) {
    var (pkg, name) = p.parseExportedName();
    var scope = pkg.Scope();
    var obj = scope.Lookup(name);
    if (obj != default! && obj.Type() == default!) {
        p.errorf("%v has nil type"u8, obj);
    }
    if (p.tok == scanner.Ident && p.lit == "notinheap"u8) {
        p.next();
    }
    // The go/types package has no way of recording that
    // this type is marked notinheap. Presumably no user
    // of this package actually cares.
    // type alias
    if (p.tok == (rune)'=') {
        p.next();
        p.aliases[nlist[len(nlist) - 1]._<nint>()] = name;
        if (obj != default!) {
            // use the previously imported (canonical) type
            var tΔ1 = obj.Type();
            p.update(tΔ1, nlist);
            p.parseType(pkg);
            // discard
            return tΔ1;
        }
        var tΔ2 = p.parseType(pkg, nlist.ꓸꓸꓸ);
        obj = ~types.NewTypeName(token.NoPos, pkg, name, tΔ2);
        scope.Insert(obj);
        return tΔ2;
    }
    // defined type
    if (obj == default!) {
        // A named type may be referred to before the underlying type
        // is known - set it up.
        var tname = types.NewTypeName(token.NoPos, pkg, name, default!);
        types.NewNamed(tname, default!, default!);
        scope.Insert(~tname);
        obj = ~tname;
    }
    // use the previously imported (canonical), or newly created type
    var t = obj.Type();
    p.update(t, nlist);
    var (nt, ok) = t._<ж<types.Named>>(ᐧ);
    if (!ok) {
        // This can happen for unsafe.Pointer, which is a TypeName holding a Basic type.
        var pt = p.parseType(pkg);
        if (!AreEqual(pt, t)) {
            p.error("unexpected underlying type for non-named TypeName");
        }
        return t;
    }
    var underlying = p.parseType(pkg);
    if (nt.Underlying() == default!) {
        if (underlying.Underlying() == default!){
            var fix = new fixupRecord(toUpdate: nt, target: underlying);
            p.fixups = append(p.fixups, fix);
        } else {
            nt.SetUnderlying(underlying.Underlying());
        }
    }
    if (p.tok == (rune)'\n') {
        p.next();
        // collect associated methods
        while (p.tok == scanner.Ident) {
            p.expectKeyword("func"u8);
            if (p.tok == (rune)'/') {
                // Skip a /*nointerface*/ or /*asm ID */ comment.
                p.expect((rune)'/');
                p.expect((rune)'*');
                if (p.expect(scanner.Ident) == "asm"u8) {
                    p.parseUnquotedString();
                }
                p.expect((rune)'*');
                p.expect((rune)'/');
            }
            p.expect((rune)'(');
            var (receiver, _) = p.parseParam(pkg);
            p.expect((rune)')');
            @string name = p.parseName();
            var (@params, isVariadic) = p.parseParamList(pkg);
            var results = p.parseResultList(pkg);
            p.skipInlineBody();
            p.expectEOL();
            var sig = types.NewSignatureType(receiver, default!, default!, @params, results, isVariadic);
            nt.AddMethod(types.NewFunc(token.NoPos, pkg, name, sig));
        }
    }
    return ~nt;
}

[GoRecv] internal static int64 parseInt64(this ref parser p) {
    @string lit = p.expect(scanner.Int);
    var (n, err) = strconv.ParseInt(lit, 10, 64);
    if (err != default!) {
        p.error(err);
    }
    return n;
}

[GoRecv] internal static nint parseInt(this ref parser p) {
    @string lit = p.expect(scanner.Int);
    var (n, err) = strconv.ParseInt(lit, 10, 0);
    /* int */
    if (err != default!) {
        p.error(err);
    }
    return ((nint)n);
}

// ArrayOrSliceType = "[" [ int ] "]" Type .
[GoRecv] internal static typesꓸType parseArrayOrSliceType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expect((rune)'[');
    if (p.tok == (rune)']') {
        p.next();
        var tΔ1 = @new<types.Slice>();
        p.update(~tΔ1, nlist);
        .val = types.NewSlice(p.parseType(Ꮡpkg)).val;
        return ~tΔ1;
    }
    var t = @new<types.Array>();
    p.update(~t, nlist);
    var len = p.parseInt64();
    p.expect((rune)']');
    t.val = types.NewArray(p.parseType(Ꮡpkg), len).val;
    return ~t;
}

// MapType = "map" "[" Type "]" Type .
[GoRecv] internal static typesꓸType parseMapType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expectKeyword("map"u8);
    var t = @new<types.Map>();
    p.update(~t, nlist);
    p.expect((rune)'[');
    var key = p.parseType(Ꮡpkg);
    p.expect((rune)']');
    var elem = p.parseType(Ꮡpkg);
    t.val = types.NewMap(key, elem).val;
    return ~t;
}

// ChanType = "chan" ["<-" | "-<"] Type .
[GoRecv] internal static typesꓸType parseChanType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expectKeyword("chan"u8);
    var t = @new<types.Chan>();
    p.update(~t, nlist);
    types.ChanDir dir = types.SendRecv;
    switch (p.tok) {
    case (rune)'-': {
        p.next();
        p.expect((rune)'<');
        dir = types.SendOnly;
        break;
    }
    case (rune)'<': {
        if (p.scanner.Peek() == (rune)'-') {
            // don't consume '<' if it belongs to Type
            p.next();
            p.expect((rune)'-');
            dir = types.RecvOnly;
        }
        break;
    }}

    t.val = types.NewChan(dir, p.parseType(Ꮡpkg)).val;
    return ~t;
}

// StructType = "struct" "{" { Field } "}" .
[GoRecv] internal static typesꓸType parseStructType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expectKeyword("struct"u8);
    var t = @new<types.Struct>();
    p.update(~t, nlist);
    slice<types.Var> fields = default!;
    slice<@string> tags = default!;
    p.expect((rune)'{');
    while (p.tok != (rune)'}' && p.tok != scanner.EOF) {
        var (field, tag) = p.parseField(Ꮡpkg);
        p.expect((rune)';');
        fields = append(fields, field);
        tags = append(tags, tag);
    }
    p.expect((rune)'}');
    t.val = types.NewStruct(fields, tags).val;
    return ~t;
}

// ParamList = "(" [ { Parameter "," } Parameter ] ")" .
[GoRecv] internal static (ж<types.Tuple>, bool) parseParamList(this ref parser p, ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    slice<types.Var> list = default!;
    var isVariadic = false;
    p.expect((rune)'(');
    while (p.tok != (rune)')' && p.tok != scanner.EOF) {
        if (len(list) > 0) {
            p.expect((rune)',');
        }
        var (par, variadic) = p.parseParam(Ꮡpkg);
        list = append(list, par);
        if (variadic) {
            if (isVariadic) {
                p.error("... not on final argument");
            }
            isVariadic = true;
        }
    }
    p.expect((rune)')');
    return (types.NewTuple(Ꮡlist.ꓸꓸꓸ), isVariadic);
}

// ResultList = Type | ParamList .
[GoRecv] internal static ж<types.Tuple> parseResultList(this ref parser p, ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    switch (p.tok) {
    case (rune)'<': {
        p.next();
        if (p.tok == scanner.Ident && p.lit == "inl"u8) {
            return default!;
        }
        var (taa, _) = p.parseTypeAfterAngle(Ꮡpkg);
        return types.NewTuple(types.NewParam(token.NoPos, Ꮡpkg, ""u8, taa));
    }
    case (rune)'(': {
        var (@params, _) = p.parseParamList(Ꮡpkg);
        return @params;
    }
    default: {
        return default!;
    }}

}

// FunctionType = ParamList ResultList .
[GoRecv] internal static ж<typesꓸSignature> parseFunctionType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    var t = @new<typesꓸSignature>();
    p.update(~t, nlist);
    var (@params, isVariadic) = p.parseParamList(Ꮡpkg);
    var results = p.parseResultList(Ꮡpkg);
    t.val = types.NewSignatureType(nil, default!, default!, @params, results, isVariadic).val;
    return t;
}

// Func = Name FunctionType [InlineBody] .
[GoRecv] internal static ж<types.Func> parseFunc(this ref parser p, ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    if (p.tok == (rune)'/') {
        // Skip an /*asm ID */ comment.
        p.expect((rune)'/');
        p.expect((rune)'*');
        if (p.expect(scanner.Ident) == "asm"u8) {
            p.parseUnquotedString();
        }
        p.expect((rune)'*');
        p.expect((rune)'/');
    }
    @string name = p.parseName();
    var f = types.NewFunc(token.NoPos, Ꮡpkg, name, p.parseFunctionType(Ꮡpkg, default!));
    p.skipInlineBody();
    if (name[0] == (rune)'.' || name[0] == (rune)'<' || strings.ContainsRune(name, (rune)'$')) {
        // This is an unexported function,
        // or a function defined in a different package,
        // or a type$equal or type$hash function.
        // We only want to record exported functions.
        return default!;
    }
    return f;
}

// InterfaceType = "interface" "{" { ("?" Type | Func) ";" } "}" .
[GoRecv] internal static typesꓸType parseInterfaceType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expectKeyword("interface"u8);
    var t = @new<types.Interface>();
    p.update(~t, nlist);
    slice<types.Func> methods = default!;
    slice<typesꓸType> embeddeds = default!;
    p.expect((rune)'{');
    while (p.tok != (rune)'}' && p.tok != scanner.EOF) {
        if (p.tok == (rune)'?'){
            p.next();
            embeddeds = append(embeddeds, p.parseType(Ꮡpkg));
        } else {
            var method = p.parseFunc(Ꮡpkg);
            if (method != nil) {
                methods = append(methods, method);
            }
        }
        p.expect((rune)';');
    }
    p.expect((rune)'}');
    t.val = types.NewInterfaceType(methods, embeddeds).val;
    return ~t;
}

// PointerType = "*" ("any" | Type) .
[GoRecv] internal static typesꓸType parsePointerType(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    p.expect((rune)'*');
    if (p.tok == scanner.Ident) {
        p.expectKeyword("any"u8);
        var tΔ1 = types.Typ[types.UnsafePointer];
        p.update(~tΔ1, nlist);
        return ~tΔ1;
    }
    var t = @new<types.Pointer>();
    p.update(~t, nlist);
    t.val = types.NewPointer(p.parseType(Ꮡpkg, t)).val;
    return ~t;
}

// TypeSpec = NamedType | MapType | ChanType | StructType | InterfaceType | PointerType | ArrayOrSliceType | FunctionType .
[GoRecv] internal static typesꓸType parseTypeSpec(this ref parser p, ж<types.Package> Ꮡpkg, slice<any> nlist) {
    ref var pkg = ref Ꮡpkg.val;

    switch (p.tok) {
    case scanner.ΔString: {
        return p.parseNamedType(nlist);
    }
    case scanner.Ident: {
        var exprᴛ1 = p.lit;
        if (exprᴛ1 == "map"u8) {
            return p.parseMapType(Ꮡpkg, nlist);
        }
        if (exprᴛ1 == "chan"u8) {
            return p.parseChanType(Ꮡpkg, nlist);
        }
        if (exprᴛ1 == "struct"u8) {
            return p.parseStructType(Ꮡpkg, nlist);
        }
        if (exprᴛ1 == "interface"u8) {
            return p.parseInterfaceType(Ꮡpkg, nlist);
        }

        break;
    }
    case (rune)'*': {
        return p.parsePointerType(Ꮡpkg, nlist);
    }
    case (rune)'[': {
        return p.parseArrayOrSliceType(Ꮡpkg, nlist);
    }
    case (rune)'(': {
        return ~p.parseFunctionType(Ꮡpkg, nlist);
    }}

    p.errorf("expected type name or literal, got %s"u8, scanner.TokenString(p.tok));
    return default!;
}

internal static readonly UntypedInt gccgoBuiltinINT8 = 1;
internal static readonly UntypedInt gccgoBuiltinINT16 = 2;
internal static readonly UntypedInt gccgoBuiltinINT32 = 3;
internal static readonly UntypedInt gccgoBuiltinINT64 = 4;
internal static readonly UntypedInt gccgoBuiltinUINT8 = 5;
internal static readonly UntypedInt gccgoBuiltinUINT16 = 6;
internal static readonly UntypedInt gccgoBuiltinUINT32 = 7;
internal static readonly UntypedInt gccgoBuiltinUINT64 = 8;
internal static readonly UntypedInt gccgoBuiltinFLOAT32 = 9;
internal static readonly UntypedInt gccgoBuiltinFLOAT64 = 10;
internal static readonly UntypedInt gccgoBuiltinINT = 11;
internal static readonly UntypedInt gccgoBuiltinUINT = 12;
internal static readonly UntypedInt gccgoBuiltinUINTPTR = 13;
internal static readonly UntypedInt gccgoBuiltinBOOL = 15;
internal static readonly UntypedInt gccgoBuiltinSTRING = 16;
internal static readonly UntypedInt gccgoBuiltinCOMPLEX64 = 17;
internal static readonly UntypedInt gccgoBuiltinCOMPLEX128 = 18;
internal static readonly UntypedInt gccgoBuiltinERROR = 19;
internal static readonly UntypedInt gccgoBuiltinBYTE = 20;
internal static readonly UntypedInt gccgoBuiltinRUNE = 21;
internal static readonly UntypedInt gccgoBuiltinANY = 22;

internal static typesꓸType lookupBuiltinType(nint typ) {
    return new runtime.SparseArray<typesꓸType>{gccgoBuiltinINT8, ~types.Typ[types.Int8], gccgoBuiltinINT16, ~types.Typ[types.Int16], gccgoBuiltinINT32, ~types.Typ[types.Int32], gccgoBuiltinINT64, ~types.Typ[types.Int64], gccgoBuiltinUINT8, ~types.Typ[types.Uint8], gccgoBuiltinUINT16, ~types.Typ[types.Uint16], gccgoBuiltinUINT32, ~types.Typ[types.Uint32], gccgoBuiltinUINT64, ~types.Typ[types.Uint64], gccgoBuiltinFLOAT32, ~types.Typ[types.Float32], gccgoBuiltinFLOAT64, ~types.Typ[types.Float64], gccgoBuiltinINT, ~types.Typ[types.Int], gccgoBuiltinUINT, ~types.Typ[types.Uint], gccgoBuiltinUINTPTR, ~types.Typ[types.Uintptr], gccgoBuiltinBOOL, ~types.Typ[types.Bool], gccgoBuiltinSTRING, ~types.Typ[types.ΔString], gccgoBuiltinCOMPLEX64, ~types.Typ[types.Complex64], gccgoBuiltinCOMPLEX128, ~types.Typ[types.Complex128], gccgoBuiltinERROR, types.Universe.Lookup("error"u8).Type(), gccgoBuiltinBYTE, types.Universe.Lookup("byte"u8).Type(), gccgoBuiltinRUNE, types.Universe.Lookup("rune"u8).Type(), gccgoBuiltinANY, types.Universe.Lookup("any"u8).Type()
    }.array()[typ];
}

// Type = "<" "type" ( "-" int | int [ TypeSpec ] ) ">" .
//
// parseType updates the type map to t for all type numbers n.
[GoRecv] internal static typesꓸType parseType(this ref parser p, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) {
    var n = nʗp.slice();

    ref var pkg = ref Ꮡpkg.val;
    p.expect((rune)'<');
    var (t, _) = p.parseTypeAfterAngle(Ꮡpkg, n.ꓸꓸꓸ);
    return t;
}

// (*parser).Type after reading the "<".
[GoRecv] internal static (typesꓸType t, nint n1) parseTypeAfterAngle(this ref parser p, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) {
    typesꓸType t = default!;
    nint n1 = default!;
    var n = nʗp.slice();

    ref var pkg = ref Ꮡpkg.val;
    p.expectKeyword("type"u8);
    n1 = 0;
    switch (p.tok) {
    case scanner.Int: {
        n1 = p.parseInt();
        if (p.tok == (rune)'>'){
            if (len(p.typeData) > 0 && p.typeList[n1] == default!) {
                p.parseSavedType(Ꮡpkg, n1, n);
            }
            t = p.typeList[n1];
            if (len(p.typeData) == 0 && Ꮡt == ~reserved) {
                p.errorf("invalid type cycle, type %d not yet defined (nlist=%v)"u8, n1, n);
            }
            p.update(t, n);
        } else {
            p.reserve(n1);
            t = p.parseTypeSpec(Ꮡpkg, append(n, n1));
        }
        break;
    }
    case (rune)'-': {
        p.next();
        nint n1Δ2 = p.parseInt();
        t = lookupBuiltinType(n1Δ2);
        p.update(t, n);
        break;
    }
    default: {
        p.errorf("expected type number, got %s (%q)"u8, scanner.TokenString(p.tok), p.lit);
        return (default!, 0);
    }}

    if (t == default! || Ꮡt == ~reserved) {
        p.errorf("internal error: bad return from parseType(%v)"u8, n);
    }
    p.expect((rune)'>');
    return (t, n1);
}

// parseTypeExtended is identical to parseType, but if the type in
// question is a saved type, returns the index as well as the type
// pointer (index returned is zero if we parsed a builtin).
[GoRecv] internal static (typesꓸType t, nint n1) parseTypeExtended(this ref parser p, ж<types.Package> Ꮡpkg, params ꓸꓸꓸany nʗp) {
    typesꓸType t = default!;
    nint n1 = default!;
    var n = nʗp.slice();

    ref var pkg = ref Ꮡpkg.val;
    p.expect((rune)'<');
    (t, n1) = p.parseTypeAfterAngle(Ꮡpkg, n.ꓸꓸꓸ);
    return (t, n1);
}

// InlineBody = "<inl:NN>" .{NN}
// Reports whether a body was skipped.
[GoRecv] internal static void skipInlineBody(this ref parser p) => func((defer, _) => {
    // We may or may not have seen the '<' already, depending on
    // whether the function had a result type or not.
    if (p.tok == (rune)'<'){
        p.next();
        p.expectKeyword("inl"u8);
    } else 
    if (p.tok != scanner.Ident || p.lit != "inl"u8){
        return;
    } else {
        p.next();
    }
    p.expect((rune)':');
    nint want = p.parseInt();
    p.expect((rune)'>');
    deferǃ((uint64 w) => {
        p.scanner.Whitespace = w;
    }, p.scanner.Whitespace, defer);
    p.scanner.Whitespace = 0;
    nint got = 0;
    while (got < want) {
        var r = p.scanner.Next();
        if (r == scanner.EOF) {
            p.error("unexpected EOF");
        }
        got += utf8.RuneLen(r);
    }
});

[GoType("dyn")] partial struct parseTypes_typeOffset {
    internal nint offset;
    internal nint length;
}

// Types = "types" maxp1 exportedp1 (offset length)* .
[GoRecv] internal static void parseTypes(this ref parser p, ж<types.Package> Ꮡpkg) => func((defer, _) => {
    ref var pkg = ref Ꮡpkg.val;

    nint maxp1 = p.parseInt();
    nint exportedp1 = p.parseInt();
    p.typeList = new slice<typesꓸType>(maxp1, maxp1);
    slice<typeOffset> typeOffsets = default!;
    nint total = 0;
    for (nint i = 1; i < maxp1; i++) {
        nint len = p.parseInt();
        typeOffsets = append(typeOffsets, new typeOffset(total, len));
        total += len;
    }
    deferǃ((uint64 w) => {
        p.scanner.Whitespace = w;
    }, p.scanner.Whitespace, defer);
    p.scanner.Whitespace = 0;
    // We should now have p.tok pointing to the final newline.
    // The next runes from the scanner should be the type data.
    strings.Builder sb = default!;
    while (sb.Len() < total) {
        var r = p.scanner.Next();
        if (r == scanner.EOF) {
            p.error("unexpected EOF");
        }
        sb.WriteRune(r);
    }
    @string allTypeData = sb.String();
    p.typeData = new @string[]{""}.slice();
    // type 0, unused
    foreach (var (_, to) in typeOffsets) {
        p.typeData = append(p.typeData, allTypeData[(int)(to.offset)..(int)(to.offset + to.length)]);
    }
    for (nint i = 1; i < exportedp1; i++) {
        p.parseSavedType(Ꮡpkg, i, default!);
    }
});

// parseSavedType parses one saved type definition.
[GoRecv] internal static void parseSavedType(this ref parser p, ж<types.Package> Ꮡpkg, nint i, slice<any> nlist) => func((defer, _) => {
    ref var pkg = ref Ꮡpkg.val;

    deferǃ((ж<scanner.Scanner> s, rune tok, @string lit) => {
        p.scanner = s;
        p.tok = tok;
        p.lit = lit;
    }, p.scanner, p.tok, p.lit, defer);
    p.scanner = @new<scanner.Scanner>();
    p.initScanner(p.scanner.Filename, ~strings.NewReader(p.typeData[i]));
    p.expectKeyword("type"u8);
    nint id = p.parseInt();
    if (id != i) {
        p.errorf("type ID mismatch: got %d, want %d"u8, id, i);
    }
    if (p.typeList[i] == ~reserved) {
        p.errorf("internal error: %d already reserved in parseSavedType"u8, i);
    }
    if (p.typeList[i] == default!) {
        p.reserve(i);
        p.parseTypeSpec(Ꮡpkg, append(nlist, i));
    }
    if (p.typeList[i] == default! || p.typeList[i] == ~reserved) {
        p.errorf("internal error: parseSavedType(%d,%v) reserved/nil"u8, i, nlist);
    }
});

// PackageInit = unquotedString unquotedString int .
[GoRecv] internal static PackageInit parsePackageInit(this ref parser p) {
    @string name = p.parseUnquotedString();
    @string initfunc = p.parseUnquotedString();
    nint priority = -1;
    if (p.version == "v1"u8) {
        priority = p.parseInt();
    }
    return new PackageInit(Name: name, InitFunc: initfunc, Priority: priority);
}

// Create the package if we have parsed both the package path and package name.
[GoRecv] internal static void maybeCreatePackage(this ref parser p) {
    if (p.pkgname != ""u8 && p.pkgpath != ""u8) {
        p.pkg = p.getPkg(p.pkgpath, p.pkgname);
    }
}

// InitDataDirective = ( "v1" | "v2" | "v3" ) ";" |
//
//	"priority" int ";" |
//	"init" { PackageInit } ";" |
//	"checksum" unquotedString ";" .
[GoRecv] internal static void parseInitDataDirective(this ref parser p) => func((defer, _) => {
    if (p.tok != scanner.Ident) {
        // unexpected token kind; panic
        p.expect(scanner.Ident);
    }
    var exprᴛ1 = p.lit;
    if (exprᴛ1 == "v1"u8 || exprᴛ1 == "v2"u8 || exprᴛ1 == "v3"u8) {
        p.version = p.lit;
        p.next();
        p.expect((rune)';');
        p.expect((rune)'\n');
    }
    else if (exprᴛ1 == "priority"u8) {
        p.next();
        p.initdata.Priority = p.parseInt();
        p.expectEOL();
    }
    else if (exprᴛ1 == "init"u8) {
        p.next();
        while (p.tok != (rune)'\n' && p.tok != (rune)';' && p.tok != scanner.EOF) {
            p.initdata.Inits = append(p.initdata.Inits, p.parsePackageInit());
        }
        p.expectEOL();
    }
    else if (exprᴛ1 == "init_graph"u8) {
        p.next();
        while (p.tok != (rune)'\n' && p.tok != (rune)';' && p.tok != scanner.EOF) {
            // The graph data is thrown away for now.
            p.parseInt64();
            p.parseInt64();
        }
        p.expectEOL();
    }
    else if (exprᴛ1 == "checksum"u8) {
        deferǃ((nuint mode) => {
            p.scanner.Mode = mode;
        }, p.scanner.Mode, defer);
        p.scanner.Mode &= ~(nuint)((nuint)(scanner.ScanInts | scanner.ScanFloats));
        p.next();
        p.parseUnquotedString();
        p.expectEOL();
    }
    else { /* default: */
        p.errorf("unexpected identifier: %q"u8, p.lit);
    }

});

// Directive = InitDataDirective |
//
//	"package" unquotedString [ unquotedString ] [ unquotedString ] ";" |
//	"pkgpath" unquotedString ";" |
//	"prefix" unquotedString ";" |
//	"import" unquotedString unquotedString string ";" |
//	"indirectimport" unquotedString unquotedstring ";" |
//	"func" Func ";" |
//	"type" Type ";" |
//	"var" Var ";" |
//	"const" Const ";" .
[GoRecv] internal static void parseDirective(this ref parser p) {
    if (p.tok != scanner.Ident) {
        // unexpected token kind; panic
        p.expect(scanner.Ident);
    }
    var exprᴛ1 = p.lit;
    if (exprᴛ1 == "v1"u8 || exprᴛ1 == "v2"u8 || exprᴛ1 == "v3"u8 || exprᴛ1 == "priority"u8 || exprᴛ1 == "init"u8 || exprᴛ1 == "init_graph"u8 || exprᴛ1 == "checksum"u8) {
        p.parseInitDataDirective();
    }
    else if (exprᴛ1 == "package"u8) {
        p.next();
        p.pkgname = p.parseUnquotedString();
        p.maybeCreatePackage();
        if (p.version != "v1"u8 && p.tok != (rune)'\n' && p.tok != (rune)';') {
            p.parseUnquotedString();
            p.parseUnquotedString();
        }
        p.expectEOL();
    }
    else if (exprᴛ1 == "pkgpath"u8) {
        p.next();
        p.pkgpath = p.parseUnquotedString();
        p.maybeCreatePackage();
        p.expectEOL();
    }
    else if (exprᴛ1 == "prefix"u8) {
        p.next();
        p.pkgpath = p.parseUnquotedString();
        p.expectEOL();
    }
    else if (exprᴛ1 == "import"u8) {
        p.next();
        @string pkgname = p.parseUnquotedString();
        @string pkgpath = p.parseUnquotedString();
        p.getPkg(pkgpath, pkgname);
        p.parseString();
        p.expectEOL();
    }
    else if (exprᴛ1 == "indirectimport"u8) {
        p.next();
        @string pkgname = p.parseUnquotedString();
        @string pkgpath = p.parseUnquotedString();
        p.getPkg(pkgpath, pkgname);
        p.expectEOL();
    }
    else if (exprᴛ1 == "types"u8) {
        p.next();
        p.parseTypes(p.pkg);
        p.expectEOL();
    }
    else if (exprᴛ1 == "func"u8) {
        p.next();
        var fun = p.parseFunc(p.pkg);
        if (fun != nil) {
            p.pkg.Scope().Insert(~fun);
        }
        p.expectEOL();
    }
    else if (exprᴛ1 == "type"u8) {
        p.next();
        p.parseType(p.pkg);
        p.expectEOL();
    }
    else if (exprᴛ1 == "var"u8) {
        p.next();
        var v = p.parseVar(p.pkg);
        if (v != nil) {
            p.pkg.Scope().Insert(~v);
        }
        p.expectEOL();
    }
    else if (exprᴛ1 == "const"u8) {
        p.next();
        var c = p.parseConst(p.pkg);
        p.pkg.Scope().Insert(~c);
        p.expectEOL();
    }
    else { /* default: */
        p.errorf("unexpected identifier: %q"u8, p.lit);
    }

}

// Package = { Directive } .
[GoRecv] internal static ж<types.Package> parsePackage(this ref parser p) {
    while (p.tok != scanner.EOF) {
        p.parseDirective();
    }
    foreach (var (_, f) in p.fixups) {
        if (f.target.Underlying() == default!) {
            p.errorf("internal error: fixup can't be applied, loop required"u8);
        }
        f.toUpdate.SetUnderlying(f.target.Underlying());
    }
    p.fixups = default!;
    foreach (var (_, typ) in p.typeList) {
        {
            var (it, ok) = typ._<ж<types.Interface>>(ᐧ); if (ok) {
                it.Complete();
            }
        }
    }
    p.pkg.MarkComplete();
    return p.pkg;
}

} // end gccgoimporter_package
