// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Annotate Ref in Prog with C types by parsing gcc debug output.
// Conversion of debug output to Go types.

// package main -- go2cs converted at 2022 March 13 05:58:10 UTC
// Original source: C:\Program Files\Go\src\cmd\cgo\gcc.go
namespace go;

using bytes = bytes_package;
using dwarf = debug.dwarf_package;
using elf = debug.elf_package;
using macho = debug.macho_package;
using pe = debug.pe_package;
using binary = encoding.binary_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using token = go.token_package;
using xcoff = @internal.xcoff_package;
using math = math_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class main_package {

private static var debugDefine = flag.Bool("debug-define", false, "print relevant #defines");
private static var debugGcc = flag.Bool("debug-gcc", false, "print gcc invocations");

private static map nameToC = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"schar":"signed char","uchar":"unsigned char","ushort":"unsigned short","uint":"unsigned int","ulong":"unsigned long","longlong":"long long","ulonglong":"unsigned long long","complexfloat":"float _Complex","complexdouble":"double _Complex",};

// cname returns the C name to use for C.s.
// The expansions are listed in nameToC and also
// struct_foo becomes "struct foo", and similarly for
// union and enum.
private static @string cname(@string s) {
    {
        var (t, ok) = nameToC[s];

        if (ok) {
            return t;
        }
    }

    if (strings.HasPrefix(s, "struct_")) {
        return "struct " + s[(int)len("struct_")..];
    }
    if (strings.HasPrefix(s, "union_")) {
        return "union " + s[(int)len("union_")..];
    }
    if (strings.HasPrefix(s, "enum_")) {
        return "enum " + s[(int)len("enum_")..];
    }
    if (strings.HasPrefix(s, "sizeof_")) {
        return "sizeof(" + cname(s[(int)len("sizeof_")..]) + ")";
    }
    return s;
}

// DiscardCgoDirectives processes the import C preamble, and discards
// all #cgo CFLAGS and LDFLAGS directives, so they don't make their
// way into _cgo_export.h.
private static void DiscardCgoDirectives(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    var linesIn = strings.Split(f.Preamble, "\n");
    var linesOut = make_slice<@string>(0, len(linesIn));
    foreach (var (_, line) in linesIn) {
        var l = strings.TrimSpace(line);
        if (len(l) < 5 || l[..(int)4] != "#cgo" || !unicode.IsSpace(rune(l[4]))) {
            linesOut = append(linesOut, line);
        }
        else
 {
            linesOut = append(linesOut, "");
        }
    }    f.Preamble = strings.Join(linesOut, "\n");
}

// addToFlag appends args to flag. All flags are later written out onto the
// _cgo_flags file for the build system to use.
private static void addToFlag(this ptr<Package> _addr_p, @string flag, slice<@string> args) {
    ref Package p = ref _addr_p.val;

    p.CgoFlags[flag] = append(p.CgoFlags[flag], args);
    if (flag == "CFLAGS") { 
        // We'll also need these when preprocessing for dwarf information.
        // However, discard any -g options: we need to be able
        // to parse the debug info, so stick to what we expect.
        foreach (var (_, arg) in args) {
            if (!strings.HasPrefix(arg, "-g")) {
                p.GccOptions = append(p.GccOptions, arg);
            }
        }
    }
}

// splitQuoted splits the string s around each instance of one or more consecutive
// white space characters while taking into account quotes and escaping, and
// returns an array of substrings of s or an empty list if s contains only white space.
// Single quotes and double quotes are recognized to prevent splitting within the
// quoted region, and are removed from the resulting substrings. If a quote in s
// isn't closed err will be set and r will have the unclosed argument as the
// last element. The backslash is used for escaping.
//
// For example, the following string:
//
//     `a b:"c d" 'e''f'  "g\""`
//
// Would be parsed as:
//
//     []string{"a", "b:c d", "ef", `g"`}
//
private static (slice<@string>, error) splitQuoted(@string s) {
    slice<@string> r = default;
    error err = default!;

    slice<@string> args = default;
    var arg = make_slice<int>(len(s));
    var escaped = false;
    var quoted = false;
    char quote = '\x00';
    nint i = 0;
    foreach (var (_, r) in s) {

        if (escaped) 
            escaped = false;
        else if (r == '\\') 
            escaped = true;
            continue;
        else if (quote != 0) 
            if (r == quote) {
                quote = 0;
                continue;
            }
        else if (r == '"' || r == '\'') 
            quoted = true;
            quote = r;
            continue;
        else if (unicode.IsSpace(r)) 
            if (quoted || i > 0) {
                quoted = false;
                args = append(args, string(arg[..(int)i]));
                i = 0;
            }
            continue;
                arg[i] = r;
        i++;
    }    if (quoted || i > 0) {
        args = append(args, string(arg[..(int)i]));
    }
    if (quote != 0) {
        err = errors.New("unclosed quote");
    }
    else if (escaped) {
        err = errors.New("unfinished escaping");
    }
    return (args, error.As(err)!);
}

// Translate rewrites f.AST, the original Go input, to remove
// references to the imported package C, replacing them with
// references to the equivalent Go types, functions, and variables.
private static void Translate(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    foreach (var (_, cref) in f.Ref) { 
        // Convert C.ulong to C.unsigned long, etc.
        cref.Name.C = cname(cref.Name.Go);
    }    ref typeConv conv = ref heap(out ptr<typeConv> _addr_conv);
    conv.Init(p.PtrSize, p.IntSize);

    p.loadDefines(f);
    p.typedefs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    p.typedefList = null;
    nint numTypedefs = -1;
    while (len(p.typedefs) > numTypedefs) {
        numTypedefs = len(p.typedefs); 
        // Also ask about any typedefs we've seen so far.
        foreach (var (_, info) in p.typedefList) {
            if (f.Name[info.typedef] != null) {
                continue;
            }
            ptr<Name> n = addr(new Name(Go:info.typedef,C:info.typedef,));
            f.Name[info.typedef] = n;
            f.NamePos[n] = info.pos;
        }        var needType = p.guessKinds(f);
        if (len(needType) > 0) {
            p.loadDWARF(f, _addr_conv, needType);
        }
        if (godefs.val) {
            break;
        }
    }
    p.prepareNames(f);
    if (p.rewriteCalls(f)) { 
        // Add `import _cgo_unsafe "unsafe"` after the package statement.
        f.Edit.Insert(f.offset(f.AST.Name.End()), "; import _cgo_unsafe \"unsafe\"");
    }
    p.rewriteRef(f);
}

// loadDefines coerces gcc into spitting out the #defines in use
// in the file f and saves relevant renamings in f.Name[name].Define.
private static void loadDefines(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    bytes.Buffer b = default;
    b.WriteString(builtinProlog);
    b.WriteString(f.Preamble);
    var stdout = p.gccDefines(b.Bytes());

    foreach (var (_, line) in strings.Split(stdout, "\n")) {
        if (len(line) < 9 || line[(int)0..(int)7] != "#define") {
            continue;
        }
        line = strings.TrimSpace(line[(int)8..]);

        @string key = default;        @string val = default;

        var spaceIndex = strings.Index(line, " ");
        var tabIndex = strings.Index(line, "\t");

        if (spaceIndex == -1 && tabIndex == -1) {
            continue;
        }
        else if (tabIndex == -1 || (spaceIndex != -1 && spaceIndex < tabIndex)) {
            key = line[(int)0..(int)spaceIndex];
            val = strings.TrimSpace(line[(int)spaceIndex..]);
        }
        else
 {
            key = line[(int)0..(int)tabIndex];
            val = strings.TrimSpace(line[(int)tabIndex..]);
        }
        if (key == "__clang__") {
            p.GccIsClang = true;
        }
        {
            var n = f.Name[key];

            if (n != null) {
                if (debugDefine.val) {
                    fmt.Fprintf(os.Stderr, "#define %s %s\n", key, val);
                }
                n.Define = val;
            }

        }
    }
}

// guessKinds tricks gcc into revealing the kind of each
// name xxx for the references C.xxx in the Go input.
// The kind is either a constant, type, or variable.
private static slice<ptr<Name>> guessKinds(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
 
    // Determine kinds for names we already know about,
    // like #defines or 'struct foo', before bothering with gcc.
    slice<ptr<Name>> names = default;    slice<ptr<Name>> needType = default;

    map optional = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Name>, bool>{};
    foreach (var (_, key) in nameKeys(f.Name)) {
        var n = f.Name[key]; 
        // If we've already found this name as a #define
        // and we can translate it as a constant value, do so.
        if (n.Define != "") {
            {
                var i__prev2 = i;

                var (i, err) = strconv.ParseInt(n.Define, 0, 64);

                if (err == null) {
                    n.Kind = "iconst"; 
                    // Turn decimal into hex, just for consistency
                    // with enum-derived constants. Otherwise
                    // in the cgo -godefs output half the constants
                    // are in hex and half are in whatever the #define used.
                    n.Const = fmt.Sprintf("%#x", i);
                }
                else if (n.Define[0] == '\'') {
                    {
                        var (_, err) = parser.ParseExpr(n.Define);

                        if (err == null) {
                            n.Kind = "iconst";
                            n.Const = n.Define;
                        }

                    }
                }
                else if (n.Define[0] == '"') {
                    {
                        (_, err) = parser.ParseExpr(n.Define);

                        if (err == null) {
                            n.Kind = "sconst";
                            n.Const = n.Define;
                        }

                    }
                }

                i = i__prev2;

            }

            if (n.IsConst()) {
                continue;
            }
        }
        if (strings.HasPrefix(n.C, "struct ") || strings.HasPrefix(n.C, "union ") || strings.HasPrefix(n.C, "enum ")) {
            n.Kind = "type";
            needType = append(needType, n);
            continue;
        }
        if ((goos == "darwin" || goos == "ios") && strings.HasSuffix(n.C, "Ref")) { 
            // For FooRef, find out if FooGetTypeID exists.
            var s = n.C[..(int)len(n.C) - 3] + "GetTypeID";
            n = addr(new Name(Go:s,C:s));
            names = append(names, n);
            optional[n] = true;
        }
        names = append(names, n);
    }    if (len(names) == 0) {
        return needType;
    }
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    b.WriteString(builtinProlog);
    b.WriteString(f.Preamble);

    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;
            fmt.Fprintf(_addr_b, "#line %d \"not-declared\"\n" + "void __cgo_f_%d_1(void) { __typeof__(%s) *__cgo_undefined__1; }\n" + "#line %d \"not-type\"\n" + "void __cgo_f_%d_2(void) { %s *__cgo_undefined__2; }\n" + "#line %d \"not-int-const\"\n" + "void __cgo_f_%d_3(void) { enum { __cgo_undefined__3 = (%s)*1 }; }\n" + "#line %d \"not-num-const\"\n" + "void __cgo_f_%d_4(void) { static const double __cgo_undefined__4 = (%s); }\n" + "#line %d \"not-str-lit\"\n" + "void __cgo_f_%d_5(void) { static const char __cgo_undefined__5[] = (%s); }\n", i + 1, i + 1, n.C, i + 1, i + 1, n.C, i + 1, i + 1, n.C, i + 1, i + 1, n.C, i + 1, i + 1, n.C);
        }
        i = i__prev1;
        n = n__prev1;
    }

    fmt.Fprintf(_addr_b, "#line 1 \"completed\"\n" + "int __cgo__1 = __cgo__2;\n"); 

    // We need to parse the output from this gcc command, so ensure that it
    // doesn't have any ANSI escape sequences in it. (TERM=dumb is
    // insufficient; if the user specifies CGO_CFLAGS=-fdiagnostics-color,
    // GCC will ignore TERM, and GCC can also be configured at compile-time
    // to ignore TERM.)
    var stderr = p.gccErrors(b.Bytes(), "-fdiagnostics-color=never");
    if (strings.Contains(stderr, "unrecognized command line option")) { 
        // We're using an old version of GCC that doesn't understand
        // -fdiagnostics-color. Those versions can't print color anyway,
        // so just rerun without that option.
        stderr = p.gccErrors(b.Bytes());
    }
    if (stderr == "") {
        fatalf("%s produced no output\non input:\n%s", p.gccBaseCmd()[0], b.Bytes());
    }
    var completed = false;
    var sniff = make_slice<nint>(len(names));
    const nint notType = 1 << (int)(iota);
    const var notIntConst = 0;
    const var notNumConst = 1;
    const var notStrLiteral = 2;
    const var notDeclared = 3;
    var sawUnmatchedErrors = false;
    foreach (var (_, line) in strings.Split(stderr, "\n")) { 
        // Ignore warnings and random comments, with one
        // exception: newer GCC versions will sometimes emit
        // an error on a macro #define with a note referring
        // to where the expansion occurs. We care about where
        // the expansion occurs, so in that case treat the note
        // as an error.
        var isError = strings.Contains(line, ": error:");
        var isErrorNote = strings.Contains(line, ": note:") && sawUnmatchedErrors;
        if (!isError && !isErrorNote) {
            continue;
        }
        var c1 = strings.Index(line, ":");
        if (c1 < 0) {
            continue;
        }
        var c2 = strings.Index(line[(int)c1 + 1..], ":");
        if (c2 < 0) {
            continue;
        }
        c2 += c1 + 1;

        var filename = line[..(int)c1];
        var (i, _) = strconv.Atoi(line[(int)c1 + 1..(int)c2]);
        i--;
        if (i < 0 || i >= len(names)) {
            if (isError) {
                sawUnmatchedErrors = true;
            }
            continue;
        }
        switch (filename) {
            case "completed": 
                // Strictly speaking, there is no guarantee that seeing the error at completed:1
                // (at the end of the file) means we've seen all the errors from earlier in the file,
                // but usually it does. Certainly if we don't see the completed:1 error, we did
                // not get all the errors we expected.
                completed = true;
                break;
            case "not-declared": 
                sniff[i] |= notDeclared;
                break;
            case "not-type": 
                sniff[i] |= notType;
                break;
            case "not-int-const": 
                sniff[i] |= notIntConst;
                break;
            case "not-num-const": 
                sniff[i] |= notNumConst;
                break;
            case "not-str-lit": 
                sniff[i] |= notStrLiteral;
                break;
            default: 
                if (isError) {
                    sawUnmatchedErrors = true;
                }
                continue;
                break;
        }

        sawUnmatchedErrors = false;
    }    if (!completed) {
        fatalf("%s did not produce error at completed:1\non input:\n%s\nfull error output:\n%s", p.gccBaseCmd()[0], b.Bytes(), stderr);
    }
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;

            if (sniff[i] == notStrLiteral | notType) 
                n.Kind = "iconst";
            else if (sniff[i] == notIntConst | notStrLiteral | notType) 
                n.Kind = "fconst";
            else if (sniff[i] == notIntConst | notNumConst | notType) 
                n.Kind = "sconst";
            else if (sniff[i] == notIntConst | notNumConst | notStrLiteral) 
                n.Kind = "type";
            else if (sniff[i] == notIntConst | notNumConst | notStrLiteral | notType) 
                n.Kind = "not-type";
            else 
                if (sniff[i] & notDeclared != 0 && optional[n]) { 
                    // Ignore optional undeclared identifiers.
                    // Don't report an error, and skip adding n to the needType array.
                    continue;
                }
                error_(f.NamePos[n], "could not determine kind of name for C.%s", fixGo(n.Go));
                        needType = append(needType, n);
        }
        i = i__prev1;
        n = n__prev1;
    }

    if (nerrors > 0) { 
        // Check if compiling the preamble by itself causes any errors,
        // because the messages we've printed out so far aren't helpful
        // to users debugging preamble mistakes. See issue 8442.
        var preambleErrors = p.gccErrors((slice<byte>)f.Preamble);
        if (len(preambleErrors) > 0) {
            error_(token.NoPos, "\n%s errors for preamble:\n%s", p.gccBaseCmd()[0], preambleErrors);
        }
        fatalf("unresolved names");
    }
    return needType;
}

// loadDWARF parses the DWARF debug information generated
// by gcc to learn the details of the constants, variables, and types
// being referred to as C.xxx.
private static void loadDWARF(this ptr<Package> _addr_p, ptr<File> _addr_f, ptr<typeConv> _addr_conv, slice<ptr<Name>> names) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
    ref typeConv conv = ref _addr_conv.val;
 
    // Extract the types from the DWARF section of an object
    // from a well-formed C program. Gcc only generates DWARF info
    // for symbols in the object file, so it is not enough to print the
    // preamble and hope the symbols we care about will be there.
    // Instead, emit
    //    __typeof__(names[i]) *__cgo__i;
    // for each entry in names and then dereference the type we
    // learn for __cgo__i.
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    b.WriteString(builtinProlog);
    b.WriteString(f.Preamble);
    b.WriteString("#line 1 \"cgo-dwarf-inference\"\n");
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;
            fmt.Fprintf(_addr_b, "__typeof__(%s) *__cgo__%d;\n", n.C, i);
            if (n.Kind == "iconst") {
                fmt.Fprintf(_addr_b, "enum { __cgo_enum__%d = %s };\n", i, n.C);
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    fmt.Fprintf(_addr_b, "long long __cgodebug_ints[] = {\n");
    {
        var n__prev1 = n;

        foreach (var (_, __n) in names) {
            n = __n;
            if (n.Kind == "iconst") {
                fmt.Fprintf(_addr_b, "\t%s,\n", n.C);
            }
            else
 {
                fmt.Fprintf(_addr_b, "\t0,\n");
            }
        }
        n = n__prev1;
    }

    fmt.Fprintf(_addr_b, "\t1\n");
    fmt.Fprintf(_addr_b, "};\n"); 

    // do the same work for floats.
    fmt.Fprintf(_addr_b, "double __cgodebug_floats[] = {\n");
    {
        var n__prev1 = n;

        foreach (var (_, __n) in names) {
            n = __n;
            if (n.Kind == "fconst") {
                fmt.Fprintf(_addr_b, "\t%s,\n", n.C);
            }
            else
 {
                fmt.Fprintf(_addr_b, "\t0,\n");
            }
        }
        n = n__prev1;
    }

    fmt.Fprintf(_addr_b, "\t1\n");
    fmt.Fprintf(_addr_b, "};\n"); 

    // do the same work for strings.
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;
            if (n.Kind == "sconst") {
                fmt.Fprintf(_addr_b, "const char __cgodebug_str__%d[] = %s;\n", i, n.C);
                fmt.Fprintf(_addr_b, "const unsigned long long __cgodebug_strlen__%d = sizeof(%s)-1;\n", i, n.C);
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    var (d, ints, floats, strs) = p.gccDebug(b.Bytes(), len(names)); 

    // Scan DWARF info for top-level TagVariable entries with AttrName __cgo__i.
    var types = make_slice<dwarf.Type>(len(names));
    var r = d.Reader();
    while (true) {
        var (e, err) = r.Next();
        if (err != null) {
            fatalf("reading DWARF entry: %s", err);
        }
        if (e == null) {
            break;
        }

        if (e.Tag == dwarf.TagVariable) 
            @string (name, _) = e.Val(dwarf.AttrName)._<@string>();
            dwarf.Offset (typOff, _) = e.Val(dwarf.AttrType)._<dwarf.Offset>();
            if (name == "" || typOff == 0) {
                if (e.Val(dwarf.AttrSpecification) != null) { 
                    // Since we are reading all the DWARF,
                    // assume we will see the variable elsewhere.
                    break;
                }
                fatalf("malformed DWARF TagVariable entry");
            }
            if (!strings.HasPrefix(name, "__cgo__")) {
                break;
            }
            var (typ, err) = d.Type(typOff);
            if (err != null) {
                fatalf("loading DWARF type: %s", err);
            }
            ptr<dwarf.PtrType> (t, ok) = typ._<ptr<dwarf.PtrType>>();
            if (!ok || t == null) {
                fatalf("internal error: %s has non-pointer type", name);
            }
            var (i, err) = strconv.Atoi(name[(int)7..]);
            if (err != null) {
                fatalf("malformed __cgo__ name: %s", name);
            }
            types[i] = t.Type;
            p.recordTypedefs(t.Type, f.NamePos[names[i]]);
                if (e.Tag != dwarf.TagCompileUnit) {
            r.SkipChildren();
        }
    } 

    // Record types and typedef information.
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;
            if (strings.HasSuffix(n.Go, "GetTypeID") && types[i].String() == "func() CFTypeID") {
                conv.getTypeIDs[n.Go[..(int)len(n.Go) - 9]] = true;
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in names) {
            i = __i;
            n = __n;
            if (types[i] == null) {
                continue;
            }
            var pos = f.NamePos[n];
            ptr<dwarf.FuncType> (f, fok) = types[i]._<ptr<dwarf.FuncType>>();
            if (n.Kind != "type" && fok) {
                n.Kind = "func";
                n.FuncType = conv.FuncType(f, pos);
            }
            else
 {
                n.Type = conv.Type(types[i], pos);
                switch (n.Kind) {
                    case "iconst": 
                                           if (i < len(ints)) {
                                               {
                                                   ptr<dwarf.UintType> (_, ok) = types[i]._<ptr<dwarf.UintType>>();

                                                   if (ok) {
                                                       n.Const = fmt.Sprintf("%#x", uint64(ints[i]));
                                                   }
                                                   else
                        {
                                                       n.Const = fmt.Sprintf("%#x", ints[i]);
                                                   }

                                               }
                                           }
                        break;
                    case "fconst": 
                        if (i >= len(floats)) {
                            break;
                        }
                        switch (base(types[i]).type()) {
                            case ptr<dwarf.IntType> _:
                                n.Kind = "var";
                                break;
                            case ptr<dwarf.UintType> _:
                                n.Kind = "var";
                                break;
                            default:
                            {
                                n.Const = fmt.Sprintf("%f", floats[i]);
                                break;
                            }
                        }
                        break;
                    case "sconst": 
                        if (i < len(strs)) {
                            n.Const = fmt.Sprintf("%q", strs[i]);
                        }
                        break;
                }
            }
            conv.FinishType(pos);
        }
        i = i__prev1;
        n = n__prev1;
    }
}

// recordTypedefs remembers in p.typedefs all the typedefs used in dtypes and its children.
private static void recordTypedefs(this ptr<Package> _addr_p, dwarf.Type dtype, token.Pos pos) {
    ref Package p = ref _addr_p.val;

    p.recordTypedefs1(dtype, pos, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<dwarf.Type, bool>{});
}

private static void recordTypedefs1(this ptr<Package> _addr_p, dwarf.Type dtype, token.Pos pos, map<dwarf.Type, bool> visited) {
    ref Package p = ref _addr_p.val;

    if (dtype == null) {
        return ;
    }
    if (visited[dtype]) {
        return ;
    }
    visited[dtype] = true;
    switch (dtype.type()) {
        case ptr<dwarf.TypedefType> dt:
            if (strings.HasPrefix(dt.Name, "__builtin")) { 
                // Don't look inside builtin types. There be dragons.
                return ;
            }
            if (!p.typedefs[dt.Name]) {
                p.typedefs[dt.Name] = true;
                p.typedefList = append(p.typedefList, new typedefInfo(dt.Name,pos));
                p.recordTypedefs1(dt.Type, pos, visited);
            }
            break;
        case ptr<dwarf.PtrType> dt:
            p.recordTypedefs1(dt.Type, pos, visited);
            break;
        case ptr<dwarf.ArrayType> dt:
            p.recordTypedefs1(dt.Type, pos, visited);
            break;
        case ptr<dwarf.QualType> dt:
            p.recordTypedefs1(dt.Type, pos, visited);
            break;
        case ptr<dwarf.FuncType> dt:
            p.recordTypedefs1(dt.ReturnType, pos, visited);
            foreach (var (_, a) in dt.ParamType) {
                p.recordTypedefs1(a, pos, visited);
            }
            break;
        case ptr<dwarf.StructType> dt:
            foreach (var (_, f) in dt.Field) {
                p.recordTypedefs1(f.Type, pos, visited);
            }
            break;
    }
}

// prepareNames finalizes the Kind field of not-type names and sets
// the mangled name of all names.
private static void prepareNames(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    foreach (var (_, n) in f.Name) {
        if (n.Kind == "not-type") {
            if (n.Define == "") {
                n.Kind = "var";
            }
            else
 {
                n.Kind = "macro";
                n.FuncType = addr(new FuncType(Result:n.Type,Go:&ast.FuncType{Results:&ast.FieldList{List:[]*ast.Field{{Type:n.Type.Go}}},},));
            }
        }
        p.mangleName(n);
        if (n.Kind == "type" && typedef[n.Mangle] == null) {
            typedef[n.Mangle] = n.Type;
        }
    }
}

// mangleName does name mangling to translate names
// from the original Go source files to the names
// used in the final Go files generated by cgo.
private static void mangleName(this ptr<Package> _addr_p, ptr<Name> _addr_n) {
    ref Package p = ref _addr_p.val;
    ref Name n = ref _addr_n.val;
 
    // When using gccgo variables have to be
    // exported so that they become global symbols
    // that the C code can refer to.
    @string prefix = "_C";
    if (gccgo && n.IsVar().val) {
        prefix = "C";
    }
    n.Mangle = prefix + n.Kind + "_" + n.Go;
}

private static bool isMangledName(this ptr<File> _addr_f, @string s) {
    ref File f = ref _addr_f.val;

    @string prefix = "_C";
    if (strings.HasPrefix(s, prefix)) {
        var t = s[(int)len(prefix)..];
        foreach (var (_, k) in nameKinds) {
            if (strings.HasPrefix(t, k + "_")) {
                return true;
            }
        }
    }
    return false;
}

// rewriteCalls rewrites all calls that pass pointers to check that
// they follow the rules for passing pointers between Go and C.
// This reports whether the package needs to import unsafe as _cgo_unsafe.
private static bool rewriteCalls(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    var needsUnsafe = false; 
    // Walk backward so that in C.f1(C.f2()) we rewrite C.f2 first.
    foreach (var (_, call) in f.Calls) {
        if (call.Done) {
            continue;
        }
        var start = f.offset(call.Call.Pos());
        var end = f.offset(call.Call.End());
        var (str, nu) = p.rewriteCall(f, call);
        if (str != "") {
            f.Edit.Replace(start, end, str);
            if (nu) {
                needsUnsafe = true;
            }
        }
    }    return needsUnsafe;
}

// rewriteCall rewrites one call to add pointer checks.
// If any pointer checks are required, we rewrite the call into a
// function literal that calls _cgoCheckPointer for each pointer
// argument and then calls the original function.
// This returns the rewritten call and whether the package needs to
// import unsafe as _cgo_unsafe.
// If it returns the empty string, the call did not need to be rewritten.
private static (@string, bool) rewriteCall(this ptr<Package> _addr_p, ptr<File> _addr_f, ptr<Call> _addr_call) {
    @string _p0 = default;
    bool _p0 = default;
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
    ref Call call = ref _addr_call.val;
 
    // This is a call to C.xxx; set goname to "xxx".
    // It may have already been mangled by rewriteName.
    @string goname = default;
    switch (call.Call.Fun.type()) {
        case ptr<ast.SelectorExpr> fun:
            goname = fun.Sel.Name;
            break;
        case ptr<ast.Ident> fun:
            goname = strings.TrimPrefix(fun.Name, "_C2func_");
            goname = strings.TrimPrefix(goname, "_Cfunc_");
            break;
    }
    if (goname == "" || goname == "malloc") {
        return ("", false);
    }
    var name = f.Name[goname];
    if (name == null || name.Kind != "func") { 
        // Probably a type conversion.
        return ("", false);
    }
    var @params = name.FuncType.Params;
    var args = call.Call.Args; 

    // Avoid a crash if the number of arguments doesn't match
    // the number of parameters.
    // This will be caught when the generated file is compiled.
    if (len(args) != len(params)) {
        return ("", false);
    }
    var any = false;
    {
        var i__prev1 = i;
        var param__prev1 = param;

        foreach (var (__i, __param) in params) {
            i = __i;
            param = __param;
            if (p.needsPointerCheck(f, param.Go, args[i])) {
                any = true;
                break;
            }
        }
        i = i__prev1;
        param = param__prev1;
    }

    if (!any) {
        return ("", false);
    }
    ref bytes.Buffer sb = ref heap(out ptr<bytes.Buffer> _addr_sb);
    sb.WriteString("func() ");
    if (call.Deferred) {
        sb.WriteString("func() ");
    }
    var needsUnsafe = false;
    var result = false;
    var twoResults = false;
    if (!call.Deferred) { 
        // Check whether this call expects two results.
        foreach (var (_, ref) in f.Ref) {
            if (@ref.Expr != _addr_call.Call.Fun) {
                continue;
            }
            if (@ref.Context == ctxCall2) {
                sb.WriteString("(");
                result = true;
                twoResults = true;
            }
            break;
        }        if (name.FuncType.Result != null) {
            var rtype = p.rewriteUnsafe(name.FuncType.Result.Go);
            if (rtype != name.FuncType.Result.Go) {
                needsUnsafe = true;
            }
            sb.WriteString(gofmtLine(rtype));
            result = true;
        }
        if (twoResults) {
            if (name.FuncType.Result == null) { 
                // An explicit void result looks odd but it
                // seems to be how cgo has worked historically.
                sb.WriteString("_Ctype_void");
            }
            sb.WriteString(", error)");
        }
    }
    sb.WriteString("{ "); 

    // Define _cgoN for each argument value.
    // Write _cgoCheckPointer calls to sbCheck.
    ref bytes.Buffer sbCheck = ref heap(out ptr<bytes.Buffer> _addr_sbCheck);
    {
        var i__prev1 = i;
        var param__prev1 = param;

        foreach (var (__i, __param) in params) {
            i = __i;
            param = __param;
            var origArg = args[i];
            var (arg, nu) = p.mangle(f, _addr_args[i], true);
            if (nu) {
                needsUnsafe = true;
            } 

            // Use "var x T = ..." syntax to explicitly convert untyped
            // constants to the parameter type, to avoid a type mismatch.
            var ptype = p.rewriteUnsafe(param.Go);

            if (!p.needsPointerCheck(f, param.Go, args[i]) || param.BadPointer) {
                if (ptype != param.Go) {
                    needsUnsafe = true;
                }
                fmt.Fprintf(_addr_sb, "var _cgo%d %s = %s; ", i, gofmtLine(ptype), gofmtPos(arg, origArg.Pos()));
                continue;
            } 

            // Check for &a[i].
            if (p.checkIndex(_addr_sb, _addr_sbCheck, arg, i)) {
                continue;
            } 

            // Check for &x.
            if (p.checkAddr(_addr_sb, _addr_sbCheck, arg, i)) {
                continue;
            }
            fmt.Fprintf(_addr_sb, "_cgo%d := %s; ", i, gofmtPos(arg, origArg.Pos()));
            fmt.Fprintf(_addr_sbCheck, "_cgoCheckPointer(_cgo%d, nil); ", i);
        }
        i = i__prev1;
        param = param__prev1;
    }

    if (call.Deferred) {
        sb.WriteString("return func() { ");
    }
    sb.WriteString(sbCheck.String());

    if (result) {
        sb.WriteString("return ");
    }
    var (m, nu) = p.mangle(f, _addr_call.Call.Fun, false);
    if (nu) {
        needsUnsafe = true;
    }
    sb.WriteString(gofmtLine(m));

    sb.WriteString("(");
    {
        var i__prev1 = i;

        foreach (var (__i) in params) {
            i = __i;
            if (i > 0) {
                sb.WriteString(", ");
            }
            fmt.Fprintf(_addr_sb, "_cgo%d", i);
        }
        i = i__prev1;
    }

    sb.WriteString("); ");
    if (call.Deferred) {
        sb.WriteString("}");
    }
    sb.WriteString("}");
    if (call.Deferred) {
        sb.WriteString("()");
    }
    sb.WriteString("()");

    return (sb.String(), needsUnsafe);
}

// needsPointerCheck reports whether the type t needs a pointer check.
// This is true if t is a pointer and if the value to which it points
// might contain a pointer.
private static bool needsPointerCheck(this ptr<Package> _addr_p, ptr<File> _addr_f, ast.Expr t, ast.Expr arg) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
 
    // An untyped nil does not need a pointer check, and when
    // _cgoCheckPointer returns the untyped nil the type assertion we
    // are going to insert will fail.  Easier to just skip nil arguments.
    // TODO: Note that this fails if nil is shadowed.
    {
        ptr<ast.Ident> (id, ok) = arg._<ptr<ast.Ident>>();

        if (ok && id.Name == "nil") {
            return false;
        }
    }

    return p.hasPointer(f, t, true);
}

// hasPointer is used by needsPointerCheck. If top is true it returns
// whether t is or contains a pointer that might point to a pointer.
// If top is false it reports whether t is or contains a pointer.
// f may be nil.
private static bool hasPointer(this ptr<Package> _addr_p, ptr<File> _addr_f, ast.Expr t, bool top) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    switch (t.type()) {
        case ptr<ast.ArrayType> t:
            if (t.Len == null) {
                if (!top) {
                    return true;
                }
                return p.hasPointer(f, t.Elt, false);
            }
            return p.hasPointer(f, t.Elt, top);
            break;
        case ptr<ast.StructType> t:
            foreach (var (_, field) in t.Fields.List) {
                if (p.hasPointer(f, field.Type, top)) {
                    return true;
                }
            }
            return false;
            break;
        case ptr<ast.StarExpr> t:
            if (!top) {
                return true;
            } 
            // Check whether this is a pointer to a C union (or class)
            // type that contains a pointer.
            if (unionWithPointer[t.X]) {
                return true;
            }
            return p.hasPointer(f, t.X, false);
            break;
        case ptr<ast.FuncType> t:
            return true;
            break;
        case ptr<ast.InterfaceType> t:
            return true;
            break;
        case ptr<ast.MapType> t:
            return true;
            break;
        case ptr<ast.ChanType> t:
            return true;
            break;
        case ptr<ast.Ident> t:
            foreach (var (_, d) in p.Decl) {
                ptr<ast.GenDecl> (gd, ok) = d._<ptr<ast.GenDecl>>();
                if (!ok || gd.Tok != token.TYPE) {
                    continue;
                }
                foreach (var (_, spec) in gd.Specs) {
                    ptr<ast.TypeSpec> (ts, ok) = spec._<ptr<ast.TypeSpec>>();
                    if (!ok) {
                        continue;
                    }
                    if (ts.Name.Name == t.Name) {
                        return p.hasPointer(f, ts.Type, top);
                    }
                }
            }
            {
                var def = typedef[t.Name];

                if (def != null) {
                    return p.hasPointer(f, def.Go, top);
                }

            }
            if (t.Name == "string") {
                return !top;
            }
            if (t.Name == "error") {
                return true;
            }
            if (goTypes[t.Name] != null) {
                return false;
            } 
            // We can't figure out the type. Conservative
            // approach is to assume it has a pointer.
            return true;
            break;
        case ptr<ast.SelectorExpr> t:
            {
                ptr<ast.Ident> (l, ok) = t.X._<ptr<ast.Ident>>();

                if (!ok || l.Name != "C") { 
                    // Type defined in a different package.
                    // Conservative approach is to assume it has a
                    // pointer.
                    return true;
                }

            }
            if (f == null) { 
                // Conservative approach: assume pointer.
                return true;
            }
            var name = f.Name[t.Sel.Name];
            if (name != null && name.Kind == "type" && name.Type != null && name.Type.Go != null) {
                return p.hasPointer(f, name.Type.Go, top);
            } 
            // We can't figure out the type. Conservative
            // approach is to assume it has a pointer.
            return true;
            break;
        default:
        {
            var t = t.type();
            error_(t.Pos(), "could not understand type %s", gofmt(t));
            return true;
            break;
        }
    }
}

// mangle replaces references to C names in arg with the mangled names,
// rewriting calls when it finds them.
// It removes the corresponding references in f.Ref and f.Calls, so that we
// don't try to do the replacement again in rewriteRef or rewriteCall.
// If addPosition is true, add position info to the idents of C names in arg.
private static (ast.Expr, bool) mangle(this ptr<Package> _addr_p, ptr<File> _addr_f, ptr<ast.Expr> _addr_arg, bool addPosition) {
    ast.Expr _p0 = default;
    bool _p0 = default;
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
    ref ast.Expr arg = ref _addr_arg.val;

    var needsUnsafe = false;
    f.walk(arg, ctxExpr, (f, arg, context) => {
        ptr<ast.Expr> (px, ok) = arg._<ptr<ast.Expr>>();
        if (!ok) {
            return ;
        }
        ptr<ast.SelectorExpr> (sel, ok) = (px.val)._<ptr<ast.SelectorExpr>>();
        if (ok) {
            {
                ptr<ast.Ident> (l, ok) = sel.X._<ptr<ast.Ident>>();

                if (!ok || l.Name != "C") {
                    return ;
                }

            }

            foreach (var (_, r) in f.Ref) {
                if (r.Expr == px) {
                    px.val = p.rewriteName(f, r, addPosition);
                    r.Done = true;
                    break;
                }
            }
            return ;
        }
        ptr<ast.CallExpr> (call, ok) = (px.val)._<ptr<ast.CallExpr>>();
        if (!ok) {
            return ;
        }
        foreach (var (_, c) in f.Calls) {
            if (!c.Done && c.Call.Lparen == call.Lparen) {
                var (cstr, nu) = p.rewriteCall(f, c);
                if (cstr != "") { 
                    // Smuggle the rewritten call through an ident.
                    px.val = ast.NewIdent(cstr);
                    if (nu) {
                        needsUnsafe = true;
                    }
                    c.Done = true;
                }
            }
        }
    });
    return (arg, needsUnsafe);
}

// checkIndex checks whether arg has the form &a[i], possibly inside
// type conversions. If so, then in the general case it writes
//    _cgoIndexNN := a
//    _cgoNN := &cgoIndexNN[i] // with type conversions, if any
// to sb, and writes
//    _cgoCheckPointer(_cgoNN, _cgoIndexNN)
// to sbCheck, and returns true. If a is a simple variable or field reference,
// it writes
//    _cgoIndexNN := &a
// and dereferences the uses of _cgoIndexNN. Taking the address avoids
// making a copy of an array.
//
// This tells _cgoCheckPointer to check the complete contents of the
// slice or array being indexed, but no other part of the memory allocation.
private static bool checkIndex(this ptr<Package> _addr_p, ptr<bytes.Buffer> _addr_sb, ptr<bytes.Buffer> _addr_sbCheck, ast.Expr arg, nint i) {
    ref Package p = ref _addr_p.val;
    ref bytes.Buffer sb = ref _addr_sb.val;
    ref bytes.Buffer sbCheck = ref _addr_sbCheck.val;
 
    // Strip type conversions.
    var x = arg;
    while (true) {
        ptr<ast.CallExpr> (c, ok) = x._<ptr<ast.CallExpr>>();
        if (!ok || len(c.Args) != 1 || !p.isType(c.Fun)) {
            break;
        }
        x = c.Args[0];
    }
    ptr<ast.UnaryExpr> (u, ok) = x._<ptr<ast.UnaryExpr>>();
    if (!ok || u.Op != token.AND) {
        return false;
    }
    ptr<ast.IndexExpr> (index, ok) = u.X._<ptr<ast.IndexExpr>>();
    if (!ok) {
        return false;
    }
    @string addr = "";
    @string deref = "";
    if (p.isVariable(index.X)) {
        addr = "&";
        deref = "*";
    }
    fmt.Fprintf(sb, "_cgoIndex%d := %s%s; ", i, addr, gofmtPos(index.X, index.X.Pos()));
    var origX = index.X;
    index.X = ast.NewIdent(fmt.Sprintf("_cgoIndex%d", i));
    if (deref == "*") {
        index.X = addr(new ast.StarExpr(X:index.X));
    }
    fmt.Fprintf(sb, "_cgo%d := %s; ", i, gofmtPos(arg, arg.Pos()));
    index.X = origX;

    fmt.Fprintf(sbCheck, "_cgoCheckPointer(_cgo%d, %s_cgoIndex%d); ", i, deref, i);

    return true;
}

// checkAddr checks whether arg has the form &x, possibly inside type
// conversions. If so, it writes
//    _cgoBaseNN := &x
//    _cgoNN := _cgoBaseNN // with type conversions, if any
// to sb, and writes
//    _cgoCheckPointer(_cgoBaseNN, true)
// to sbCheck, and returns true. This tells _cgoCheckPointer to check
// just the contents of the pointer being passed, not any other part
// of the memory allocation. This is run after checkIndex, which looks
// for the special case of &a[i], which requires different checks.
private static bool checkAddr(this ptr<Package> _addr_p, ptr<bytes.Buffer> _addr_sb, ptr<bytes.Buffer> _addr_sbCheck, ast.Expr arg, nint i) {
    ref Package p = ref _addr_p.val;
    ref bytes.Buffer sb = ref _addr_sb.val;
    ref bytes.Buffer sbCheck = ref _addr_sbCheck.val;
 
    // Strip type conversions.
    var px = _addr_arg;
    while (true) {
        ptr<ast.CallExpr> (c, ok) = (px.val)._<ptr<ast.CallExpr>>();
        if (!ok || len(c.Args) != 1 || !p.isType(c.Fun)) {
            break;
        }
        px = _addr_c.Args[0];
    }
    {
        ptr<ast.UnaryExpr> (u, ok) = (px.val)._<ptr<ast.UnaryExpr>>();

        if (!ok || u.Op != token.AND) {
            return false;
        }
    }

    fmt.Fprintf(sb, "_cgoBase%d := %s; ", i, gofmtPos(px.val, (px.val).Pos()));

    var origX = px.val;
    px.val = ast.NewIdent(fmt.Sprintf("_cgoBase%d", i));
    fmt.Fprintf(sb, "_cgo%d := %s; ", i, gofmtPos(arg, arg.Pos()));
    px.val = origX; 

    // Use "0 == 0" to do the right thing in the unlikely event
    // that "true" is shadowed.
    fmt.Fprintf(sbCheck, "_cgoCheckPointer(_cgoBase%d, 0 == 0); ", i);

    return true;
}

// isType reports whether the expression is definitely a type.
// This is conservative--it returns false for an unknown identifier.
private static bool isType(this ptr<Package> _addr_p, ast.Expr t) {
    ref Package p = ref _addr_p.val;

    switch (t.type()) {
        case ptr<ast.SelectorExpr> t:
            ptr<ast.Ident> (id, ok) = t.X._<ptr<ast.Ident>>();
            if (!ok) {
                return false;
            }
            if (id.Name == "unsafe" && t.Sel.Name == "Pointer") {
                return true;
            }
            if (id.Name == "C" && typedef["_Ctype_" + t.Sel.Name] != null) {
                return true;
            }
            return false;
            break;
        case ptr<ast.Ident> t:
            switch (t.Name) {
                case "unsafe.Pointer": 


                case "bool": 


                case "byte": 


                case "complex64": 


                case "complex128": 


                case "error": 


                case "float32": 


                case "float64": 


                case "int": 


                case "int8": 


                case "int16": 


                case "int32": 


                case "int64": 


                case "rune": 


                case "string": 


                case "uint": 


                case "uint8": 


                case "uint16": 


                case "uint32": 


                case "uint64": 


                case "uintptr": 

                    return true;
                    break;
            }
            if (strings.HasPrefix(t.Name, "_Ctype_")) {
                return true;
            }
            break;
        case ptr<ast.ParenExpr> t:
            return p.isType(t.X);
            break;
        case ptr<ast.StarExpr> t:
            return p.isType(t.X);
            break;
        case ptr<ast.ArrayType> t:
            return true;
            break;
        case ptr<ast.StructType> t:
            return true;
            break;
        case ptr<ast.FuncType> t:
            return true;
            break;
        case ptr<ast.InterfaceType> t:
            return true;
            break;
        case ptr<ast.MapType> t:
            return true;
            break;
        case ptr<ast.ChanType> t:
            return true;
            break;
    }
    return false;
}

// isVariable reports whether x is a variable, possibly with field references.
private static bool isVariable(this ptr<Package> _addr_p, ast.Expr x) {
    ref Package p = ref _addr_p.val;

    switch (x.type()) {
        case ptr<ast.Ident> x:
            return true;
            break;
        case ptr<ast.SelectorExpr> x:
            return p.isVariable(x.X);
            break;
        case ptr<ast.IndexExpr> x:
            return true;
            break;
    }
    return false;
}

// rewriteUnsafe returns a version of t with references to unsafe.Pointer
// rewritten to use _cgo_unsafe.Pointer instead.
private static ast.Expr rewriteUnsafe(this ptr<Package> _addr_p, ast.Expr t) {
    ref Package p = ref _addr_p.val;

    switch (t.type()) {
        case ptr<ast.Ident> t:
            if (t.Name == "unsafe.Pointer") {
                return ast.NewIdent("_cgo_unsafe.Pointer");
            }
            break;
        case ptr<ast.ArrayType> t:
            var t1 = p.rewriteUnsafe(t.Elt);
            if (t1 != t.Elt) {
                ref var r = ref heap(t.val, out ptr<var> _addr_r);
                r.Elt = t1;
                return _addr_r;
            }
            break;
        case ptr<ast.StructType> t:
            var changed = false;
            ref var fields = ref heap(t.Fields.val, out ptr<var> _addr_fields);
            fields.List = null;
            foreach (var (_, f) in t.Fields.List) {
                var ft = p.rewriteUnsafe(f.Type);
                if (ft == f.Type) {
                    fields.List = append(fields.List, f);
                }
                else
 {
                    ref var fn = ref heap(f.val, out ptr<var> _addr_fn);
                    fn.Type = ft;
                    fields.List = append(fields.List, _addr_fn);
                    changed = true;
                }
            }
            if (changed) {
                r = t.val;
                _addr_r.Fields = _addr_fields;
                r.Fields = ref _addr_r.Fields.val;
                return _addr_r;
            }
            break;
        case ptr<ast.StarExpr> t:
            var x1 = p.rewriteUnsafe(t.X);
            if (x1 != t.X) {
                r = t.val;
                r.X = x1;
                return _addr_r;
            }
            break;
    }
    return t;
}

// rewriteRef rewrites all the C.xxx references in f.AST to refer to the
// Go equivalents, now that we have figured out the meaning of all
// the xxx. In *godefs mode, rewriteRef replaces the names
// with full definitions instead of mangled names.
private static void rewriteRef(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
 
    // Keep a list of all the functions, to remove the ones
    // only used as expressions and avoid generating bridge
    // code for them.
    var functions = make_map<@string, bool>();

    foreach (var (_, n) in f.Name) {
        if (n.Kind == "func") {
            functions[n.Go] = false;
        }
    }    foreach (var (_, r) in f.Ref) {
        if (r.Name.IsConst() && r.Name.Const == "") {
            error_(r.Pos(), "unable to find value of constant C.%s", fixGo(r.Name.Go));
        }
        if (r.Name.Kind == "func") {

            if (r.Context == ctxCall || r.Context == ctxCall2) 
                functions[r.Name.Go] = true;
                    }
        var expr = p.rewriteName(f, r, false);

        if (godefs.val) { 
            // Substitute definition for mangled type name.
            if (r.Name.Type != null && r.Name.Kind == "type") {
                expr = r.Name.Type.Go;
            }
            {
                ptr<ast.Ident> (id, ok) = expr._<ptr<ast.Ident>>();

                if (ok) {
                    {
                        var t = typedef[id.Name];

                        if (t != null) {
                            expr = t.Go;
                        }

                    }
                    if (id.Name == r.Name.Mangle && r.Name.Const != "") {
                        expr = ast.NewIdent(r.Name.Const);
                    }
                }

            }
        }
        var pos = (r.Expr.val).Pos();
        {
            ptr<ast.Ident> (x, ok) = expr._<ptr<ast.Ident>>();

            if (ok) {
                expr = addr(new ast.Ident(NamePos:pos,Name:x.Name));
            } 

            // Change AST, because some later processing depends on it,
            // and also because -godefs mode still prints the AST.

        } 

        // Change AST, because some later processing depends on it,
        // and also because -godefs mode still prints the AST.
        var old = r.Expr.val;
        r.Expr.val = expr; 

        // Record source-level edit for cgo output.
        if (!r.Done) { 
            // Prepend a space in case the earlier code ends
            // with '/', which would give us a "//" comment.
            @string repl = " " + gofmtPos(expr, old.Pos());
            var end = fset.Position(old.End()); 
            // Subtract 1 from the column if we are going to
            // append a close parenthesis. That will set the
            // correct column for the following characters.
            nint sub = 0;
            if (r.Name.Kind != "type") {
                sub = 1;
            }
            if (end.Column > sub) {
                repl = fmt.Sprintf("%s /*line :%d:%d*/", repl, end.Line, end.Column - sub);
            }
            if (r.Name.Kind != "type") {
                repl = "(" + repl + ")";
            }
            f.Edit.Replace(f.offset(old.Pos()), f.offset(old.End()), repl);
        }
    }    foreach (var (name, used) in functions) {
        if (!used) {
            delete(f.Name, name);
        }
    }
}

// rewriteName returns the expression used to rewrite a reference.
// If addPosition is true, add position info in the ident name.
private static ast.Expr rewriteName(this ptr<Package> _addr_p, ptr<File> _addr_f, ptr<Ref> _addr_r, bool addPosition) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;
    ref Ref r = ref _addr_r.val;

    var getNewIdent = ast.NewIdent;
    if (addPosition) {
        getNewIdent = newName => {
            var mangledIdent = ast.NewIdent(newName);
            if (len(newName) == len(r.Name.Go)) {
                return mangledIdent;
            }
            var p = fset.Position((r.Expr.val).End());
            if (p.Column == 0) {
                return mangledIdent;
            }
            return ast.NewIdent(fmt.Sprintf("%s /*line :%d:%d*/", newName, p.Line, p.Column));
        };
    }
    ast.Expr expr = getNewIdent(r.Name.Mangle); // default

    if (r.Context == ctxCall || r.Context == ctxCall2) 
        if (r.Name.Kind != "func") {
            if (r.Name.Kind == "type") {
                r.Context = ctxType;
                if (r.Name.Type == null) {
                    error_(r.Pos(), "invalid conversion to C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
                }
                break;
            }
            error_(r.Pos(), "call of non-function C.%s", fixGo(r.Name.Go));
            break;
        }
        if (r.Context == ctxCall2) {
            if (r.Name.Go == "_CMalloc") {
                error_(r.Pos(), "no two-result form for C.malloc");
                break;
            } 
            // Invent new Name for the two-result function.
            var n = f.Name["2" + r.Name.Go];
            if (n == null) {
                n = @new<Name>();
                n.val = r.Name.val;
                n.AddError = true;
                n.Mangle = "_C2func_" + n.Go;
                f.Name["2" + r.Name.Go] = n;
            }
            expr = getNewIdent(n.Mangle);
            r.Name = n;
            break;
        }
    else if (r.Context == ctxExpr) 
        switch (r.Name.Kind) {
            case "func": 
                if (builtinDefs[r.Name.C] != "") {
                    error_(r.Pos(), "use of builtin '%s' not in function call", fixGo(r.Name.C));
                } 

                // Function is being used in an expression, to e.g. pass around a C function pointer.
                // Create a new Name for this Ref which causes the variable to be declared in Go land.
                @string fpName = "fp_" + r.Name.Go;
                var name = f.Name[fpName];
                if (name == null) {
                    name = addr(new Name(Go:fpName,C:r.Name.C,Kind:"fpvar",Type:&Type{Size:p.PtrSize,Align:p.PtrSize,C:c("void*"),Go:ast.NewIdent("unsafe.Pointer")},));
                    p.mangleName(name);
                    f.Name[fpName] = name;
                }
                r.Name = name; 
                // Rewrite into call to _Cgo_ptr to prevent assignments. The _Cgo_ptr
                // function is defined in out.go and simply returns its argument. See
                // issue 7757.
                expr = addr(new ast.CallExpr(Fun:&ast.Ident{NamePos:(*r.Expr).Pos(),Name:"_Cgo_ptr"},Args:[]ast.Expr{getNewIdent(name.Mangle)},));
                break;
            case "type": 
                // Okay - might be new(T)
                if (r.Name.Type == null) {
                    error_(r.Pos(), "expression C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
                }
                break;
            case "var": 
                expr = addr(new ast.StarExpr(Star:(*r.Expr).Pos(),X:expr));
                break;
            case "macro": 
                expr = addr(new ast.CallExpr(Fun:expr));
                break;
        }
    else if (r.Context == ctxSelector) 
        if (r.Name.Kind == "var") {
            expr = addr(new ast.StarExpr(Star:(*r.Expr).Pos(),X:expr));
        }
        else
 {
            error_(r.Pos(), "only C variables allowed in selector expression %s", fixGo(r.Name.Go));
        }
    else if (r.Context == ctxType) 
        if (r.Name.Kind != "type") {
            error_(r.Pos(), "expression C.%s used as type", fixGo(r.Name.Go));
        }
        else if (r.Name.Type == null) { 
            // Use of C.enum_x, C.struct_x or C.union_x without C definition.
            // GCC won't raise an error when using pointers to such unknown types.
            error_(r.Pos(), "type C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
        }
    else 
        if (r.Name.Kind == "func") {
            error_(r.Pos(), "must call C.%s", fixGo(r.Name.Go));
        }
        return expr;
}

// gofmtPos returns the gofmt-formatted string for an AST node,
// with a comment setting the position before the node.
private static @string gofmtPos(ast.Expr n, token.Pos pos) {
    var s = gofmtLine(n);
    var p = fset.Position(pos);
    if (p.Column == 0) {
        return s;
    }
    return fmt.Sprintf("/*line :%d:%d*/%s", p.Line, p.Column, s);
}

// gccBaseCmd returns the start of the compiler command line.
// It uses $CC if set, or else $GCC, or else the compiler recorded
// during the initial build as defaultCC.
// defaultCC is defined in zdefaultcc.go, written by cmd/dist.
private static slice<@string> gccBaseCmd(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;
 
    // Use $CC if set, since that's what the build uses.
    {
        var ret__prev1 = ret;

        var ret = strings.Fields(os.Getenv("CC"));

        if (len(ret) > 0) {
            return ret;
        }
        ret = ret__prev1;

    } 
    // Try $GCC if set, since that's what we used to use.
    {
        var ret__prev1 = ret;

        ret = strings.Fields(os.Getenv("GCC"));

        if (len(ret) > 0) {
            return ret;
        }
        ret = ret__prev1;

    }
    return strings.Fields(defaultCC(goos, goarch));
}

// gccMachine returns the gcc -m flag to use, either "-m32", "-m64" or "-marm".
private static slice<@string> gccMachine(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    switch (goarch) {
        case "amd64": 
            if (goos == "darwin") {
                return new slice<@string>(new @string[] { "-arch", "x86_64", "-m64" });
            }
            return new slice<@string>(new @string[] { "-m64" });
            break;
        case "arm64": 
            if (goos == "darwin") {
                return new slice<@string>(new @string[] { "-arch", "arm64" });
            }
            break;
        case "386": 
            return new slice<@string>(new @string[] { "-m32" });
            break;
        case "arm": 
            return new slice<@string>(new @string[] { "-marm" }); // not thumb
            break;
        case "s390": 
            return new slice<@string>(new @string[] { "-m31" });
            break;
        case "s390x": 
            return new slice<@string>(new @string[] { "-m64" });
            break;
        case "mips64": 

        case "mips64le": 
            if (gomips64 == "hardfloat") {
                return new slice<@string>(new @string[] { "-mabi=64", "-mhard-float" });
            }
            else if (gomips64 == "softfloat") {
                return new slice<@string>(new @string[] { "-mabi=64", "-msoft-float" });
            }
            break;
        case "mips": 

        case "mipsle": 
            if (gomips == "hardfloat") {
                return new slice<@string>(new @string[] { "-mabi=32", "-mfp32", "-mhard-float", "-mno-odd-spreg" });
            }
            else if (gomips == "softfloat") {
                return new slice<@string>(new @string[] { "-mabi=32", "-msoft-float" });
            }
            break;
    }
    return null;
}

private static @string gccTmp() {
    return objDir + "_cgo_.o".val;
}

// gccCmd returns the gcc command line to use for compiling
// the input.
private static slice<@string> gccCmd(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    var c = append(p.gccBaseCmd(), "-w", "-Wno-error", "-o" + gccTmp(), "-gdwarf-2", "-c", "-xc");
    if (p.GccIsClang) {
        c = append(c, "-ferror-limit=0", "-Wno-unknown-warning-option", "-Wno-unneeded-internal-declaration", "-Wno-unused-function", "-Qunused-arguments", "-fno-builtin");
    }
    c = append(c, p.GccOptions);
    c = append(c, p.gccMachine());
    if (goos == "aix") {
        c = append(c, "-maix64");
        c = append(c, "-mcmodel=large");
    }
    c = append(c, "-fno-lto");
    c = append(c, "-"); //read input from standard input
    return c;
}

// gccDebug runs gcc -gdwarf-2 over the C program stdin and
// returns the corresponding DWARF data and, if present, debug data block.
private static (ptr<dwarf.Data>, slice<long>, slice<double>, slice<@string>) gccDebug(this ptr<Package> _addr_p, slice<byte> stdin, nint nnames) => func((defer, panic, _) => {
    ptr<dwarf.Data> d = default!;
    slice<long> ints = default;
    slice<double> floats = default;
    slice<@string> strs = default;
    ref Package p = ref _addr_p.val;

    runGcc(stdin, p.gccCmd());

    Func<@string, bool> isDebugInts = s => { 
        // Some systems use leading _ to denote non-assembly symbols.
        return _addr_s == "__cgodebug_ints" || s == "___cgodebug_ints"!;
    };
    Func<@string, bool> isDebugFloats = s => { 
        // Some systems use leading _ to denote non-assembly symbols.
        return _addr_s == "__cgodebug_floats" || s == "___cgodebug_floats"!;
    };
    Func<@string, nint> indexOfDebugStr = s => { 
        // Some systems use leading _ to denote non-assembly symbols.
        if (strings.HasPrefix(s, "___")) {
            s = s[(int)1..];
        }
        if (strings.HasPrefix(s, "__cgodebug_str__")) {
            {
                var n__prev2 = n;

                var (n, err) = strconv.Atoi(s[(int)len("__cgodebug_str__")..]);

                if (err == null) {
                    return _addr_n!;
                }

                n = n__prev2;

            }
        }
        return _addr_-1!;
    };
    Func<@string, nint> indexOfDebugStrlen = s => { 
        // Some systems use leading _ to denote non-assembly symbols.
        if (strings.HasPrefix(s, "___")) {
            s = s[(int)1..];
        }
        if (strings.HasPrefix(s, "__cgodebug_strlen__")) {
            {
                var n__prev2 = n;

                (n, err) = strconv.Atoi(s[(int)len("__cgodebug_strlen__")..]);

                if (err == null) {
                    return _addr_n!;
                }

                n = n__prev2;

            }
        }
        return _addr_-1!;
    };

    strs = make_slice<@string>(nnames);

    var strdata = make_map<nint, @string>(nnames);
    var strlens = make_map<nint, nint>(nnames);

    Action buildStrings = () => {
        {
            var n__prev1 = n;
            var strlen__prev1 = strlen;

            foreach (var (__n, __strlen) in strlens) {
                n = __n;
                strlen = __strlen;
                var data = strdata[n];
                if (len(data) <= strlen) {
                    fatalf("invalid string literal");
                }
                strs[n] = data[..(int)strlen];
            }

            n = n__prev1;
            strlen = strlen__prev1;
        }
    };

    {
        var f__prev1 = f;

        var (f, err) = macho.Open(gccTmp());

        if (err == null) {
            defer(f.Close());
            var (d, err) = f.DWARF();
            if (err != null) {
                fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
            }
            var bo = f.ByteOrder;
            if (f.Symtab != null) {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in f.Symtab.Syms) {
                        i = __i;
                        var s = _addr_f.Symtab.Syms[i];

                        if (isDebugInts(s.Name)) 
                            // Found it. Now find data section.
                            {
                                var i__prev3 = i;

                                var i = int(s.Sect) - 1;

                                if (0 <= i && i < len(f.Sections)) {
                                    var sect = f.Sections[i];
                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                        {
                                            var sdat__prev5 = sdat;

                                            var (sdat, err) = sect.Data();

                                            if (err == null) {
                                                data = sdat[(int)s.Value - sect.Addr..];
                                                ints = make_slice<long>(len(data) / 8);
                                                {
                                                    var i__prev2 = i;

                                                    foreach (var (__i) in ints) {
                                                        i = __i;
                                                        ints[i] = int64(bo.Uint64(data[(int)i * 8..]));
                                                    }

                                                    i = i__prev2;
                                                }
                                            }

                                            sdat = sdat__prev5;

                                        }
                                    }
                                }

                                i = i__prev3;

                            }
                        else if (isDebugFloats(s.Name)) 
                            // Found it. Now find data section.
                            {
                                var i__prev3 = i;

                                i = int(s.Sect) - 1;

                                if (0 <= i && i < len(f.Sections)) {
                                    sect = f.Sections[i];
                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                        {
                                            var sdat__prev5 = sdat;

                                            (sdat, err) = sect.Data();

                                            if (err == null) {
                                                data = sdat[(int)s.Value - sect.Addr..];
                                                floats = make_slice<double>(len(data) / 8);
                                                {
                                                    var i__prev2 = i;

                                                    foreach (var (__i) in floats) {
                                                        i = __i;
                                                        floats[i] = math.Float64frombits(bo.Uint64(data[(int)i * 8..]));
                                                    }

                                                    i = i__prev2;
                                                }
                                            }

                                            sdat = sdat__prev5;

                                        }
                                    }
                                }

                                i = i__prev3;

                            }
                        else 
                            {
                                var n__prev3 = n;

                                var n = indexOfDebugStr(s.Name);

                                if (n != -1) { 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev4 = i;

                                        i = int(s.Sect) - 1;

                                        if (0 <= i && i < len(f.Sections)) {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                                {
                                                    var sdat__prev6 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null) {
                                                        data = sdat[(int)s.Value - sect.Addr..];
                                                        strdata[n] = string(data);
                                                    }

                                                    sdat = sdat__prev6;

                                                }
                                            }
                                        }

                                        i = i__prev4;

                                    }
                                    break;
                                }

                                n = n__prev3;

                            }
                            {
                                var n__prev3 = n;

                                n = indexOfDebugStrlen(s.Name);

                                if (n != -1) { 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev4 = i;

                                        i = int(s.Sect) - 1;

                                        if (0 <= i && i < len(f.Sections)) {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                                {
                                                    var sdat__prev6 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null) {
                                                        data = sdat[(int)s.Value - sect.Addr..];
                                                        var strlen = bo.Uint64(data[..(int)8]);
                                                        if (strlen > (1 << (int)((uint(p.IntSize * 8) - 1)) - 1)) { // greater than MaxInt?
                                                            fatalf("string literal too big");
                                                        }
                                                        strlens[n] = int(strlen);
                                                    }

                                                    sdat = sdat__prev6;

                                                }
                                            }
                                        }

                                        i = i__prev4;

                                    }
                                    break;
                                }

                                n = n__prev3;

                            }
                                            }

                    i = i__prev1;
                }

                buildStrings();
            }
            return (_addr_d!, ints, floats, strs);
        }
        f = f__prev1;

    }

    {
        var f__prev1 = f;

        (f, err) = elf.Open(gccTmp());

        if (err == null) {
            defer(f.Close());
            (d, err) = f.DWARF();
            if (err != null) {
                fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
            }
            bo = f.ByteOrder;
            var (symtab, err) = f.Symbols();
            if (err == null) {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in symtab) {
                        i = __i;
                        s = _addr_symtab[i];

                        if (isDebugInts(s.Name)) 
                            // Found it. Now find data section.
                            {
                                var i__prev3 = i;

                                i = int(s.Section);

                                if (0 <= i && i < len(f.Sections)) {
                                    sect = f.Sections[i];
                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                        {
                                            var sdat__prev5 = sdat;

                                            (sdat, err) = sect.Data();

                                            if (err == null) {
                                                data = sdat[(int)s.Value - sect.Addr..];
                                                ints = make_slice<long>(len(data) / 8);
                                                {
                                                    var i__prev2 = i;

                                                    foreach (var (__i) in ints) {
                                                        i = __i;
                                                        ints[i] = int64(bo.Uint64(data[(int)i * 8..]));
                                                    }

                                                    i = i__prev2;
                                                }
                                            }

                                            sdat = sdat__prev5;

                                        }
                                    }
                                }

                                i = i__prev3;

                            }
                        else if (isDebugFloats(s.Name)) 
                            // Found it. Now find data section.
                            {
                                var i__prev3 = i;

                                i = int(s.Section);

                                if (0 <= i && i < len(f.Sections)) {
                                    sect = f.Sections[i];
                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                        {
                                            var sdat__prev5 = sdat;

                                            (sdat, err) = sect.Data();

                                            if (err == null) {
                                                data = sdat[(int)s.Value - sect.Addr..];
                                                floats = make_slice<double>(len(data) / 8);
                                                {
                                                    var i__prev2 = i;

                                                    foreach (var (__i) in floats) {
                                                        i = __i;
                                                        floats[i] = math.Float64frombits(bo.Uint64(data[(int)i * 8..]));
                                                    }

                                                    i = i__prev2;
                                                }
                                            }

                                            sdat = sdat__prev5;

                                        }
                                    }
                                }

                                i = i__prev3;

                            }
                        else 
                            {
                                var n__prev3 = n;

                                n = indexOfDebugStr(s.Name);

                                if (n != -1) { 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev4 = i;

                                        i = int(s.Section);

                                        if (0 <= i && i < len(f.Sections)) {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                                {
                                                    var sdat__prev6 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null) {
                                                        data = sdat[(int)s.Value - sect.Addr..];
                                                        strdata[n] = string(data);
                                                    }

                                                    sdat = sdat__prev6;

                                                }
                                            }
                                        }

                                        i = i__prev4;

                                    }
                                    break;
                                }

                                n = n__prev3;

                            }
                            {
                                var n__prev3 = n;

                                n = indexOfDebugStrlen(s.Name);

                                if (n != -1) { 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev4 = i;

                                        i = int(s.Section);

                                        if (0 <= i && i < len(f.Sections)) {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size) {
                                                {
                                                    var sdat__prev6 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null) {
                                                        data = sdat[(int)s.Value - sect.Addr..];
                                                        strlen = bo.Uint64(data[..(int)8]);
                                                        if (strlen > (1 << (int)((uint(p.IntSize * 8) - 1)) - 1)) { // greater than MaxInt?
                                                            fatalf("string literal too big");
                                                        }
                                                        strlens[n] = int(strlen);
                                                    }

                                                    sdat = sdat__prev6;

                                                }
                                            }
                                        }

                                        i = i__prev4;

                                    }
                                    break;
                                }

                                n = n__prev3;

                            }
                                            }

                    i = i__prev1;
                }

                buildStrings();
            }
            return (_addr_d!, ints, floats, strs);
        }
        f = f__prev1;

    }

    {
        var f__prev1 = f;

        (f, err) = pe.Open(gccTmp());

        if (err == null) {
            defer(f.Close());
            (d, err) = f.DWARF();
            if (err != null) {
                fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
            }
            bo = binary.LittleEndian;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in f.Symbols) {
                    s = __s;

                    if (isDebugInts(s.Name)) 
                        {
                            var i__prev2 = i;

                            i = int(s.SectionNumber) - 1;

                            if (0 <= i && i < len(f.Sections)) {
                                sect = f.Sections[i];
                                if (s.Value < sect.Size) {
                                    {
                                        var sdat__prev4 = sdat;

                                        (sdat, err) = sect.Data();

                                        if (err == null) {
                                            data = sdat[(int)s.Value..];
                                            ints = make_slice<long>(len(data) / 8);
                                            {
                                                var i__prev2 = i;

                                                foreach (var (__i) in ints) {
                                                    i = __i;
                                                    ints[i] = int64(bo.Uint64(data[(int)i * 8..]));
                                                }

                                                i = i__prev2;
                                            }
                                        }

                                        sdat = sdat__prev4;

                                    }
                                }
                            }

                            i = i__prev2;

                        }
                    else if (isDebugFloats(s.Name)) 
                        {
                            var i__prev2 = i;

                            i = int(s.SectionNumber) - 1;

                            if (0 <= i && i < len(f.Sections)) {
                                sect = f.Sections[i];
                                if (s.Value < sect.Size) {
                                    {
                                        var sdat__prev4 = sdat;

                                        (sdat, err) = sect.Data();

                                        if (err == null) {
                                            data = sdat[(int)s.Value..];
                                            floats = make_slice<double>(len(data) / 8);
                                            {
                                                var i__prev2 = i;

                                                foreach (var (__i) in floats) {
                                                    i = __i;
                                                    floats[i] = math.Float64frombits(bo.Uint64(data[(int)i * 8..]));
                                                }

                                                i = i__prev2;
                                            }
                                        }

                                        sdat = sdat__prev4;

                                    }
                                }
                            }

                            i = i__prev2;

                        }
                    else 
                        {
                            var n__prev2 = n;

                            n = indexOfDebugStr(s.Name);

                            if (n != -1) {
                                {
                                    var i__prev3 = i;

                                    i = int(s.SectionNumber) - 1;

                                    if (0 <= i && i < len(f.Sections)) {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size) {
                                            {
                                                var sdat__prev5 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null) {
                                                    data = sdat[(int)s.Value..];
                                                    strdata[n] = string(data);
                                                }

                                                sdat = sdat__prev5;

                                            }
                                        }
                                    }

                                    i = i__prev3;

                                }
                                break;
                            }

                            n = n__prev2;

                        }
                        {
                            var n__prev2 = n;

                            n = indexOfDebugStrlen(s.Name);

                            if (n != -1) {
                                {
                                    var i__prev3 = i;

                                    i = int(s.SectionNumber) - 1;

                                    if (0 <= i && i < len(f.Sections)) {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size) {
                                            {
                                                var sdat__prev5 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null) {
                                                    data = sdat[(int)s.Value..];
                                                    strlen = bo.Uint64(data[..(int)8]);
                                                    if (strlen > (1 << (int)((uint(p.IntSize * 8) - 1)) - 1)) { // greater than MaxInt?
                                                        fatalf("string literal too big");
                                                    }
                                                    strlens[n] = int(strlen);
                                                }

                                                sdat = sdat__prev5;

                                            }
                                        }
                                    }

                                    i = i__prev3;

                                }
                                break;
                            }

                            n = n__prev2;

                        }
                                    }

                s = s__prev1;
            }

            buildStrings();

            return (_addr_d!, ints, floats, strs);
        }
        f = f__prev1;

    }

    {
        var f__prev1 = f;

        (f, err) = xcoff.Open(gccTmp());

        if (err == null) {
            defer(f.Close());
            (d, err) = f.DWARF();
            if (err != null) {
                fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
            }
            bo = binary.BigEndian;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in f.Symbols) {
                    s = __s;

                    if (isDebugInts(s.Name)) 
                        {
                            var i__prev2 = i;

                            i = int(s.SectionNumber) - 1;

                            if (0 <= i && i < len(f.Sections)) {
                                sect = f.Sections[i];
                                if (s.Value < sect.Size) {
                                    {
                                        var sdat__prev4 = sdat;

                                        (sdat, err) = sect.Data();

                                        if (err == null) {
                                            data = sdat[(int)s.Value..];
                                            ints = make_slice<long>(len(data) / 8);
                                            {
                                                var i__prev2 = i;

                                                foreach (var (__i) in ints) {
                                                    i = __i;
                                                    ints[i] = int64(bo.Uint64(data[(int)i * 8..]));
                                                }

                                                i = i__prev2;
                                            }
                                        }

                                        sdat = sdat__prev4;

                                    }
                                }
                            }

                            i = i__prev2;

                        }
                    else if (isDebugFloats(s.Name)) 
                        {
                            var i__prev2 = i;

                            i = int(s.SectionNumber) - 1;

                            if (0 <= i && i < len(f.Sections)) {
                                sect = f.Sections[i];
                                if (s.Value < sect.Size) {
                                    {
                                        var sdat__prev4 = sdat;

                                        (sdat, err) = sect.Data();

                                        if (err == null) {
                                            data = sdat[(int)s.Value..];
                                            floats = make_slice<double>(len(data) / 8);
                                            {
                                                var i__prev2 = i;

                                                foreach (var (__i) in floats) {
                                                    i = __i;
                                                    floats[i] = math.Float64frombits(bo.Uint64(data[(int)i * 8..]));
                                                }

                                                i = i__prev2;
                                            }
                                        }

                                        sdat = sdat__prev4;

                                    }
                                }
                            }

                            i = i__prev2;

                        }
                    else 
                        {
                            var n__prev2 = n;

                            n = indexOfDebugStr(s.Name);

                            if (n != -1) {
                                {
                                    var i__prev3 = i;

                                    i = int(s.SectionNumber) - 1;

                                    if (0 <= i && i < len(f.Sections)) {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size) {
                                            {
                                                var sdat__prev5 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null) {
                                                    data = sdat[(int)s.Value..];
                                                    strdata[n] = string(data);
                                                }

                                                sdat = sdat__prev5;

                                            }
                                        }
                                    }

                                    i = i__prev3;

                                }
                                break;
                            }

                            n = n__prev2;

                        }
                        {
                            var n__prev2 = n;

                            n = indexOfDebugStrlen(s.Name);

                            if (n != -1) {
                                {
                                    var i__prev3 = i;

                                    i = int(s.SectionNumber) - 1;

                                    if (0 <= i && i < len(f.Sections)) {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size) {
                                            {
                                                var sdat__prev5 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null) {
                                                    data = sdat[(int)s.Value..];
                                                    strlen = bo.Uint64(data[..(int)8]);
                                                    if (strlen > (1 << (int)((uint(p.IntSize * 8) - 1)) - 1)) { // greater than MaxInt?
                                                        fatalf("string literal too big");
                                                    }
                                                    strlens[n] = int(strlen);
                                                }

                                                sdat = sdat__prev5;

                                            }
                                        }
                                    }

                                    i = i__prev3;

                                }
                                break;
                            }

                            n = n__prev2;

                        }
                                    }

                s = s__prev1;
            }

            buildStrings();
            return (_addr_d!, ints, floats, strs);
        }
        f = f__prev1;

    }
    fatalf("cannot parse gcc output %s as ELF, Mach-O, PE, XCOFF object", gccTmp());
    panic("not reached");
});

// gccDefines runs gcc -E -dM -xc - over the C program stdin
// and returns the corresponding standard output, which is the
// #defines that gcc encountered while processing the input
// and its included files.
private static @string gccDefines(this ptr<Package> _addr_p, slice<byte> stdin) {
    ref Package p = ref _addr_p.val;

    var @base = append(p.gccBaseCmd(), "-E", "-dM", "-xc");
    base = append(base, p.gccMachine());
    var (stdout, _) = runGcc(stdin, append(append(base, p.GccOptions), "-"));
    return stdout;
}

// gccErrors runs gcc over the C program stdin and returns
// the errors that gcc prints. That is, this function expects
// gcc to fail.
private static @string gccErrors(this ptr<Package> _addr_p, slice<byte> stdin, params @string[] extraArgs) {
    extraArgs = extraArgs.Clone();
    ref Package p = ref _addr_p.val;
 
    // TODO(rsc): require failure
    var args = p.gccCmd(); 

    // Optimization options can confuse the error messages; remove them.
    var nargs = make_slice<@string>(0, len(args) + len(extraArgs));
    foreach (var (_, arg) in args) {
        if (!strings.HasPrefix(arg, "-O")) {
            nargs = append(nargs, arg);
        }
    }    var li = len(nargs) - 1;
    var last = nargs[li];
    nargs[li] = "-O0";
    nargs = append(nargs, extraArgs);
    nargs = append(nargs, last);

    if (debugGcc.val) {
        fmt.Fprintf(os.Stderr, "$ %s <<EOF\n", strings.Join(nargs, " "));
        os.Stderr.Write(stdin);
        fmt.Fprint(os.Stderr, "EOF\n");
    }
    var (stdout, stderr, _) = run(stdin, nargs);
    if (debugGcc.val) {
        os.Stderr.Write(stdout);
        os.Stderr.Write(stderr);
    }
    return string(stderr);
}

// runGcc runs the gcc command line args with stdin on standard input.
// If the command exits with a non-zero exit status, runGcc prints
// details about what was run and exits.
// Otherwise runGcc returns the data written to standard output and standard error.
// Note that for some of the uses we expect useful data back
// on standard error, but for those uses gcc must still exit 0.
private static (@string, @string) runGcc(slice<byte> stdin, slice<@string> args) {
    @string _p0 = default;
    @string _p0 = default;

    if (debugGcc.val) {
        fmt.Fprintf(os.Stderr, "$ %s <<EOF\n", strings.Join(args, " "));
        os.Stderr.Write(stdin);
        fmt.Fprint(os.Stderr, "EOF\n");
    }
    var (stdout, stderr, ok) = run(stdin, args);
    if (debugGcc.val) {
        os.Stderr.Write(stdout);
        os.Stderr.Write(stderr);
    }
    if (!ok) {
        os.Stderr.Write(stderr);
        os.Exit(2);
    }
    return (string(stdout), string(stderr));
}

// A typeConv is a translator from dwarf types to Go types
// with equivalent memory layout.
private partial struct typeConv {
    public map<@string, ptr<Type>> m; // Map from types to incomplete pointers to those types.
    public map<@string, slice<ptr<Type>>> ptrs; // Keys of ptrs in insertion order (deterministic worklist)
// ptrKeys contains exactly the keys in ptrs.
    public slice<dwarf.Type> ptrKeys; // Type names X for which there exists an XGetTypeID function with type func() CFTypeID.
    public map<@string, bool> getTypeIDs; // Predeclared types.
    public ast.Expr @bool;
    public ast.Expr @byte; // denotes padding
    public ast.Expr int8;
    public ast.Expr int16;
    public ast.Expr int32;
    public ast.Expr int64;
    public ast.Expr uint8;
    public ast.Expr uint16;
    public ast.Expr uint32;
    public ast.Expr uint64;
    public ast.Expr uintptr;
    public ast.Expr float32;
    public ast.Expr float64;
    public ast.Expr complex64;
    public ast.Expr complex128;
    public ast.Expr @void;
    public ast.Expr @string;
    public ast.Expr goVoid; // _Ctype_void, denotes C's void
    public ast.Expr goVoidPtr; // unsafe.Pointer or *byte

    public long ptrSize;
    public long intSize;
}

private static nint tagGen = default;
private static var typedef = make_map<@string, ptr<Type>>();
private static var goIdent = make_map<@string, ptr<ast.Ident>>();

// unionWithPointer is true for a Go type that represents a C union (or class)
// that may contain a pointer. This is used for cgo pointer checking.
private static var unionWithPointer = make_map<ast.Expr, bool>();

// anonymousStructTag provides a consistent tag for an anonymous struct.
// The same dwarf.StructType pointer will always get the same tag.
private static var anonymousStructTag = make_map<ptr<dwarf.StructType>, @string>();

private static void Init(this ptr<typeConv> _addr_c, long ptrSize, long intSize) {
    ref typeConv c = ref _addr_c.val;

    c.ptrSize = ptrSize;
    c.intSize = intSize;
    c.m = make_map<@string, ptr<Type>>();
    c.ptrs = make_map<@string, slice<ptr<Type>>>();
    c.getTypeIDs = make_map<@string, bool>();
    c.@bool = c.Ident("bool");
    c.@byte = c.Ident("byte");
    c.int8 = c.Ident("int8");
    c.int16 = c.Ident("int16");
    c.int32 = c.Ident("int32");
    c.int64 = c.Ident("int64");
    c.uint8 = c.Ident("uint8");
    c.uint16 = c.Ident("uint16");
    c.uint32 = c.Ident("uint32");
    c.uint64 = c.Ident("uint64");
    c.uintptr = c.Ident("uintptr");
    c.float32 = c.Ident("float32");
    c.float64 = c.Ident("float64");
    c.complex64 = c.Ident("complex64");
    c.complex128 = c.Ident("complex128");
    c.@void = c.Ident("void");
    c.@string = c.Ident("string");
    c.goVoid = c.Ident("_Ctype_void"); 

    // Normally cgo translates void* to unsafe.Pointer,
    // but for historical reasons -godefs uses *byte instead.
    if (godefs.val) {
        c.goVoidPtr = addr(new ast.StarExpr(X:c.byte));
    }
    else
 {
        c.goVoidPtr = c.Ident("unsafe.Pointer");
    }
}

// base strips away qualifiers and typedefs to get the underlying type
private static dwarf.Type @base(dwarf.Type dt) {
    while (true) {
        {
            ptr<dwarf.QualType> d__prev1 = d;

            ptr<dwarf.QualType> (d, ok) = dt._<ptr<dwarf.QualType>>();

            if (ok) {
                dt = d.Type;
                continue;
            }

            d = d__prev1;

        }
        {
            ptr<dwarf.QualType> d__prev1 = d;

            (d, ok) = dt._<ptr<dwarf.TypedefType>>();

            if (ok) {
                dt = d.Type;
                continue;
            }

            d = d__prev1;

        }
        break;
    }
    return dt;
}

// unqual strips away qualifiers from a DWARF type.
// In general we don't care about top-level qualifiers.
private static dwarf.Type unqual(dwarf.Type dt) {
    while (true) {
        {
            ptr<dwarf.QualType> (d, ok) = dt._<ptr<dwarf.QualType>>();

            if (ok) {
                dt = d.Type;
            }
            else
 {
                break;
            }

        }
    }
    return dt;
}

// Map from dwarf text names to aliases we use in package "C".
private static map dwarfToName = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"long int":"long","long unsigned int":"ulong","unsigned int":"uint","short unsigned int":"ushort","unsigned short":"ushort","short int":"short","long long int":"longlong","long long unsigned int":"ulonglong","signed char":"schar","unsigned char":"uchar",};

private static readonly nint signedDelta = 64;

// String returns the current type representation. Format arguments
// are assembled within this method so that any changes in mutable
// values are taken into account.


// String returns the current type representation. Format arguments
// are assembled within this method so that any changes in mutable
// values are taken into account.
private static @string String(this ptr<TypeRepr> _addr_tr) {
    ref TypeRepr tr = ref _addr_tr.val;

    if (len(tr.Repr) == 0) {
        return "";
    }
    if (len(tr.FormatArgs) == 0) {
        return tr.Repr;
    }
    return fmt.Sprintf(tr.Repr, tr.FormatArgs);
}

// Empty reports whether the result of String would be "".
private static bool Empty(this ptr<TypeRepr> _addr_tr) {
    ref TypeRepr tr = ref _addr_tr.val;

    return len(tr.Repr) == 0;
}

// Set modifies the type representation.
// If fargs are provided, repr is used as a format for fmt.Sprintf.
// Otherwise, repr is used unprocessed as the type representation.
private static void Set(this ptr<TypeRepr> _addr_tr, @string repr, params object[] fargs) {
    fargs = fargs.Clone();
    ref TypeRepr tr = ref _addr_tr.val;

    tr.Repr = repr;
    tr.FormatArgs = fargs;
}

// FinishType completes any outstanding type mapping work.
// In particular, it resolves incomplete pointer types.
private static void FinishType(this ptr<typeConv> _addr_c, token.Pos pos) {
    ref typeConv c = ref _addr_c.val;
 
    // Completing one pointer type might produce more to complete.
    // Keep looping until they're all done.
    while (len(c.ptrKeys) > 0) {
        var dtype = c.ptrKeys[0];
        var dtypeKey = dtype.String();
        c.ptrKeys = c.ptrKeys[(int)1..];
        var ptrs = c.ptrs[dtypeKey];
        delete(c.ptrs, dtypeKey); 

        // Note Type might invalidate c.ptrs[dtypeKey].
        var t = c.Type(dtype, pos);
        foreach (var (_, ptr) in ptrs) {
            ptr.Go._<ptr<ast.StarExpr>>().X = t.Go;
            ptr.C.Set("%s*", t.C);
        }
    }
}

// Type returns a *Type with the same memory layout as
// dtype when used as the type of a variable or a struct field.
private static ptr<Type> Type(this ptr<typeConv> _addr_c, dwarf.Type dtype, token.Pos pos) {
    ref typeConv c = ref _addr_c.val;

    return _addr_c.loadType(dtype, pos, "")!;
}

// loadType recursively loads the requested dtype and its dependency graph.
private static ptr<Type> loadType(this ptr<typeConv> _addr_c, dwarf.Type dtype, token.Pos pos, @string parent) {
    ref typeConv c = ref _addr_c.val;
 
    // Always recompute bad pointer typedefs, as the set of such
    // typedefs changes as we see more types.
    var checkCache = true;
    {
        ptr<dwarf.TypedefType> (dtt, ok) = dtype._<ptr<dwarf.TypedefType>>();

        if (ok && c.badPointerTypedef(dtt)) {
            checkCache = false;
        }
    } 

    // The cache key should be relative to its parent.
    // See issue https://golang.org/issue/31891
    var key = parent + " > " + dtype.String();

    if (checkCache) {
        {
            var t__prev2 = t;

            var (t, ok) = c.m[key];

            if (ok) {
                if (t.Go == null) {
                    fatalf("%s: type conversion loop at %s", lineno(pos), dtype);
                }
                return _addr_t!;
            }

            t = t__prev2;

        }
    }
    ptr<Type> t = @new<Type>();
    t.Size = dtype.Size(); // note: wrong for array of pointers, corrected below
    t.Align = -1;
    t.C = addr(new TypeRepr(Repr:dtype.Common().Name));
    c.m[key] = t;

    switch (dtype.type()) {
        case ptr<dwarf.AddrType> dt:
            if (t.Size != c.ptrSize) {
                fatalf("%s: unexpected: %d-byte address type - %s", lineno(pos), t.Size, dtype);
            }
            t.Go = c.uintptr;
            t.Align = t.Size;
            break;
        case ptr<dwarf.ArrayType> dt:
            if (dt.StrideBitSize > 0) { 
                // Cannot represent bit-sized elements in Go.
                t.Go = c.Opaque(t.Size);
                break;
            }
            var count = dt.Count;
            if (count == -1) { 
                // Indicates flexible array member, which Go doesn't support.
                // Translate to zero-length array instead.
                count = 0;
            }
            var sub = c.Type(dt.Type, pos);
            t.Align = sub.Align;
            t.Go = addr(new ast.ArrayType(Len:c.intExpr(count),Elt:sub.Go,)); 
            // Recalculate t.Size now that we know sub.Size.
            t.Size = count * sub.Size;
            t.C.Set("__typeof__(%s[%d])", sub.C, dt.Count);
            break;
        case ptr<dwarf.BoolType> dt:
            t.Go = c.@bool;
            t.Align = 1;
            break;
        case ptr<dwarf.CharType> dt:
            if (t.Size != 1) {
                fatalf("%s: unexpected: %d-byte char type - %s", lineno(pos), t.Size, dtype);
            }
            t.Go = c.int8;
            t.Align = 1;
            break;
        case ptr<dwarf.EnumType> dt:
            t.Align = t.Size;

            if (t.Align >= c.ptrSize) {
                t.Align = c.ptrSize;
            }
            t.C.Set("enum " + dt.EnumName);
            nint signed = 0;
            t.EnumValues = make_map<@string, long>();
            foreach (var (_, ev) in dt.Val) {
                t.EnumValues[ev.Name] = ev.Val;
                if (ev.Val < 0) {
                    signed = signedDelta;
                }
            }
            switch (t.Size + int64(signed)) {
                case 1: 
                    t.Go = c.uint8;
                    break;
                case 2: 
                    t.Go = c.uint16;
                    break;
                case 4: 
                    t.Go = c.uint32;
                    break;
                case 8: 
                    t.Go = c.uint64;
                    break;
                case 1 + signedDelta: 
                    t.Go = c.int8;
                    break;
                case 2 + signedDelta: 
                    t.Go = c.int16;
                    break;
                case 4 + signedDelta: 
                    t.Go = c.int32;
                    break;
                case 8 + signedDelta: 
                    t.Go = c.int64;
                    break;
                default: 
                    fatalf("%s: unexpected: %d-byte enum type - %s", lineno(pos), t.Size, dtype);
                    break;
            }
            break;
        case ptr<dwarf.FloatType> dt:
            switch (t.Size) {
                case 4: 
                    t.Go = c.float32;
                    break;
                case 8: 
                    t.Go = c.float64;
                    break;
                default: 
                    fatalf("%s: unexpected: %d-byte float type - %s", lineno(pos), t.Size, dtype);
                    break;
            }
            t.Align = t.Size;

            if (t.Align >= c.ptrSize) {
                t.Align = c.ptrSize;
            }
            break;
        case ptr<dwarf.ComplexType> dt:
            switch (t.Size) {
                case 8: 
                    t.Go = c.complex64;
                    break;
                case 16: 
                    t.Go = c.complex128;
                    break;
                default: 
                    fatalf("%s: unexpected: %d-byte complex type - %s", lineno(pos), t.Size, dtype);
                    break;
            }
            t.Align = t.Size / 2;

            if (t.Align >= c.ptrSize) {
                t.Align = c.ptrSize;
            }
            break;
        case ptr<dwarf.FuncType> dt:
            t.Go = c.uintptr;
            t.Align = c.ptrSize;
            break;
        case ptr<dwarf.IntType> dt:
            if (dt.BitSize > 0) {
                fatalf("%s: unexpected: %d-bit int type - %s", lineno(pos), dt.BitSize, dtype);
            }
            switch (t.Size) {
                case 1: 
                    t.Go = c.int8;
                    break;
                case 2: 
                    t.Go = c.int16;
                    break;
                case 4: 
                    t.Go = c.int32;
                    break;
                case 8: 
                    t.Go = c.int64;
                    break;
                case 16: 
                    t.Go = addr(new ast.ArrayType(Len:c.intExpr(t.Size),Elt:c.uint8,));
                    break;
                default: 
                    fatalf("%s: unexpected: %d-byte int type - %s", lineno(pos), t.Size, dtype);
                    break;
            }
            t.Align = t.Size;

            if (t.Align >= c.ptrSize) {
                t.Align = c.ptrSize;
            }
            break;
        case ptr<dwarf.PtrType> dt:
            if (t.Size != c.ptrSize && t.Size != -1) {
                fatalf("%s: unexpected: %d-byte pointer type - %s", lineno(pos), t.Size, dtype);
            }
            t.Size = c.ptrSize;
            t.Align = c.ptrSize;

            {
                ptr<dwarf.VoidType> (_, ok) = base(dt.Type)._<ptr<dwarf.VoidType>>();

                if (ok) {
                    t.Go = c.goVoidPtr;
                    t.C.Set("void*");
                    var dq = dt.Type;
                    while (true) {
                        {
                            ptr<dwarf.QualType> (d, ok) = dq._<ptr<dwarf.QualType>>();

                            if (ok) {
                                t.C.Set(d.Qual + " " + t.C.String());
                                dq = d.Type;
                            }
                            else
 {
                                break;
                            }

                        }
                    }

                    break;
                } 

                // Placeholder initialization; completed in FinishType.

            } 

            // Placeholder initialization; completed in FinishType.
            t.Go = addr(new ast.StarExpr());
            t.C.Set("<incomplete>*");
            key = dt.Type.String();
            {
                (_, ok) = c.ptrs[key];

                if (!ok) {
                    c.ptrKeys = append(c.ptrKeys, dt.Type);
                }

            }
            c.ptrs[key] = append(c.ptrs[key], t);
            break;
        case ptr<dwarf.QualType> dt:
            var t1 = c.Type(dt.Type, pos);
            t.Size = t1.Size;
            t.Align = t1.Align;
            t.Go = t1.Go;
            if (unionWithPointer[t1.Go]) {
                unionWithPointer[t.Go] = true;
            }
            t.EnumValues = null;
            t.Typedef = "";
            t.C.Set("%s " + dt.Qual, t1.C);
            return _addr_t!;
            break;
        case ptr<dwarf.StructType> dt:
            var tag = dt.StructName;
            if (dt.ByteSize < 0 && tag == "") { // opaque unnamed struct - should not be possible
                break;
            }
            if (tag == "") {
                tag = anonymousStructTag[dt];
                if (tag == "") {
                    tag = "__" + strconv.Itoa(tagGen);
                    tagGen++;
                    anonymousStructTag[dt] = tag;
                }
            }
            else if (t.C.Empty()) {
                t.C.Set(dt.Kind + " " + tag);
            }
            var name = c.Ident("_Ctype_" + dt.Kind + "_" + tag);
            t.Go = name; // publish before recursive calls
            goIdent[name.Name] = name;
            if (dt.ByteSize < 0) { 
                // Size calculation in c.Struct/c.Opaque will die with size=-1 (unknown),
                // so execute the basic things that the struct case would do
                // other than try to determine a Go representation.
                ref var tt = ref heap(t.val, out ptr<var> _addr_tt);
                tt.C = addr(new TypeRepr("%s %s",[]interface{}{dt.Kind,tag}));
                tt.Go = c.Ident("struct{}");
                if (dt.Kind == "struct") { 
                    // We don't know what the representation of this struct is, so don't let
                    // anyone allocate one on the Go side. As a side effect of this annotation,
                    // pointers to this type will not be considered pointers in Go. They won't
                    // get writebarrier-ed or adjusted during a stack copy. This should handle
                    // all the cases badPointerTypedef used to handle, but hopefully will
                    // continue to work going forward without any more need for cgo changes.
                    tt.NotInHeap = true; 
                    // TODO: we should probably do the same for unions. Unions can't live
                    // on the Go heap, right? It currently doesn't work for unions because
                    // they are defined as a type alias for struct{}, not a defined type.
                }
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                break;
            }
            switch (dt.Kind) {
                case "class": 

                case "union": 
                    t.Go = c.Opaque(t.Size);
                    if (c.dwarfHasPointer(dt, pos)) {
                        unionWithPointer[t.Go] = true;
                    }
                    if (t.C.Empty()) {
                        t.C.Set("__typeof__(unsigned char[%d])", t.Size);
                    }
                    t.Align = 1; // TODO: should probably base this on field alignment.
                    typedef[name.Name] = t;
                    break;
                case "struct": 
                    var (g, csyntax, align) = c.Struct(dt, pos);
                    if (t.C.Empty()) {
                        t.C.Set(csyntax);
                    }
                    t.Align = align;
                    tt = t.val;
                    if (tag != "") {
                        tt.C = addr(new TypeRepr("struct %s",[]interface{}{tag}));
                    }
                    tt.Go = g;
                    _addr_typedef[name.Name] = _addr_tt;
                    typedef[name.Name] = ref _addr_typedef[name.Name].val;
                    break;
            }
            break;
        case ptr<dwarf.TypedefType> dt:
            if (dt.Name == "_GoString_") { 
                // Special C name for Go string type.
                // Knows string layout used by compilers: pointer plus length,
                // which rounds up to 2 pointers after alignment.
                t.Go = c.@string;
                t.Size = c.ptrSize * 2;
                t.Align = c.ptrSize;
                break;
            }
            if (dt.Name == "_GoBytes_") { 
                // Special C name for Go []byte type.
                // Knows slice layout used by compilers: pointer, length, cap.
                t.Go = c.Ident("[]byte");
                t.Size = c.ptrSize + 4 + 4;
                t.Align = c.ptrSize;
                break;
            }
            name = c.Ident("_Ctype_" + dt.Name);
            goIdent[name.Name] = name;
            @string akey = "";
            if (c.anonymousStructTypedef(dt)) { 
                // only load type recursively for typedefs of anonymous
                // structs, see issues 37479 and 37621.
                akey = key;
            }
            sub = c.loadType(dt.Type, pos, akey);
            if (c.badPointerTypedef(dt)) { 
                // Treat this typedef as a uintptr.
                ref var s = ref heap(sub.val, out ptr<var> _addr_s);
                s.Go = c.uintptr;
                s.BadPointer = true;
                _addr_sub = _addr_s;
                sub = ref _addr_sub.val; 
                // Make sure we update any previously computed type.
                {
                    var oldType__prev2 = oldType;

                    var oldType = typedef[name.Name];

                    if (oldType != null) {
                        oldType.Go = sub.Go;
                        oldType.BadPointer = true;
                    }

                    oldType = oldType__prev2;

                }
            }
            t.Go = name;
            t.BadPointer = sub.BadPointer;
            t.NotInHeap = sub.NotInHeap;
            if (unionWithPointer[sub.Go]) {
                unionWithPointer[t.Go] = true;
            }
            t.Size = sub.Size;
            t.Align = sub.Align;
            oldType = typedef[name.Name];
            if (oldType == null) {
                tt = t.val;
                tt.Go = sub.Go;
                tt.BadPointer = sub.BadPointer;
                tt.NotInHeap = sub.NotInHeap;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
            } 

            // If sub.Go.Name is "_Ctype_struct_foo" or "_Ctype_union_foo" or "_Ctype_class_foo",
            // use that as the Go form for this typedef too, so that the typedef will be interchangeable
            // with the base type.
            // In -godefs mode, do this for all typedefs.
            if (isStructUnionClass(sub.Go) || godefs.val) {
                t.Go = sub.Go;

                if (isStructUnionClass(sub.Go)) { 
                    // Use the typedef name for C code.
                    typedef[sub.Go._<ptr<ast.Ident>>().Name].C = t.C;
                } 

                // If we've seen this typedef before, and it
                // was an anonymous struct/union/class before
                // too, use the old definition.
                // TODO: it would be safer to only do this if
                // we verify that the types are the same.
                if (oldType != null && isStructUnionClass(oldType.Go)) {
                    t.Go = oldType.Go;
                }
            }
            break;
        case ptr<dwarf.UcharType> dt:
            if (t.Size != 1) {
                fatalf("%s: unexpected: %d-byte uchar type - %s", lineno(pos), t.Size, dtype);
            }
            t.Go = c.uint8;
            t.Align = 1;
            break;
        case ptr<dwarf.UintType> dt:
            if (dt.BitSize > 0) {
                fatalf("%s: unexpected: %d-bit uint type - %s", lineno(pos), dt.BitSize, dtype);
            }
            switch (t.Size) {
                case 1: 
                    t.Go = c.uint8;
                    break;
                case 2: 
                    t.Go = c.uint16;
                    break;
                case 4: 
                    t.Go = c.uint32;
                    break;
                case 8: 
                    t.Go = c.uint64;
                    break;
                case 16: 
                    t.Go = addr(new ast.ArrayType(Len:c.intExpr(t.Size),Elt:c.uint8,));
                    break;
                default: 
                    fatalf("%s: unexpected: %d-byte uint type - %s", lineno(pos), t.Size, dtype);
                    break;
            }
            t.Align = t.Size;

            if (t.Align >= c.ptrSize) {
                t.Align = c.ptrSize;
            }
            break;
        case ptr<dwarf.VoidType> dt:
            t.Go = c.goVoid;
            t.C.Set("void");
            t.Align = 1;
            break;
        default:
        {
            var dt = dtype.type();
            fatalf("%s: unexpected type: %s", lineno(pos), dtype);
            break;
        }

    }

    switch (dtype.type()) {
        case ptr<dwarf.AddrType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.BoolType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.CharType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.ComplexType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.IntType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.FloatType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.UcharType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;
        case ptr<dwarf.UintType> _:
            s = dtype.Common().Name;
            if (s != "") {
                {
                    var (ss, ok) = dwarfToName[s];

                    if (ok) {
                        s = ss;
                    }

                }
                s = strings.Replace(s, " ", "", -1);
                name = c.Ident("_Ctype_" + s);
                tt = t.val;
                _addr_typedef[name.Name] = _addr_tt;
                typedef[name.Name] = ref _addr_typedef[name.Name].val;
                if (!godefs.val) {
                    t.Go = name;
                }
            }
            break;

    }

    if (t.Size < 0) { 
        // Unsized types are [0]byte, unless they're typedefs of other types
        // or structs with tags.
        // if so, use the name we've already defined.
        t.Size = 0;
        switch (dtype.type()) {
            case ptr<dwarf.TypedefType> dt:
                break;
            case ptr<dwarf.StructType> dt:
                if (dt.StructName != "") {
                    break;
                }
                t.Go = c.Opaque(0);
                break;
            default:
            {
                var dt = dtype.type();
                t.Go = c.Opaque(0);
                break;
            }
        }
        if (t.C.Empty()) {
            t.C.Set("void");
        }
    }
    if (t.C.Empty()) {
        fatalf("%s: internal error: did not create C name for %s", lineno(pos), dtype);
    }
    return _addr_t!;
}

// isStructUnionClass reports whether the type described by the Go syntax x
// is a struct, union, or class with a tag.
private static bool isStructUnionClass(ast.Expr x) {
    ptr<ast.Ident> (id, ok) = x._<ptr<ast.Ident>>();
    if (!ok) {
        return false;
    }
    var name = id.Name;
    return strings.HasPrefix(name, "_Ctype_struct_") || strings.HasPrefix(name, "_Ctype_union_") || strings.HasPrefix(name, "_Ctype_class_");
}

// FuncArg returns a Go type with the same memory layout as
// dtype when used as the type of a C function argument.
private static ptr<Type> FuncArg(this ptr<typeConv> _addr_c, dwarf.Type dtype, token.Pos pos) {
    ref typeConv c = ref _addr_c.val;

    var t = c.Type(unqual(dtype), pos);
    switch (dtype.type()) {
        case ptr<dwarf.ArrayType> dt:
            ptr<TypeRepr> tr = addr(new TypeRepr());
            tr.Set("%s*", t.C);
            return addr(new Type(Size:c.ptrSize,Align:c.ptrSize,Go:&ast.StarExpr{X:t.Go},C:tr,));
            break;
        case ptr<dwarf.TypedefType> dt:
            {
                ptr<dwarf.PtrType> (ptr, ok) = base(dt.Type)._<ptr<dwarf.PtrType>>();

                if (ok) { 
                    // Unless the typedef happens to point to void* since
                    // Go has special rules around using unsafe.Pointer.
                    {
                        ptr<dwarf.VoidType> (_, void) = base(ptr.Type)._<ptr<dwarf.VoidType>>();

                        if (void) {
                            break;
                        } 
                        // ...or the typedef is one in which we expect bad pointers.
                        // It will be a uintptr instead of *X.

                    } 
                    // ...or the typedef is one in which we expect bad pointers.
                    // It will be a uintptr instead of *X.
                    if (c.baseBadPointerTypedef(dt)) {
                        break;
                    }
                    t = c.Type(ptr, pos);
                    if (t == null) {
                        return _addr_null!;
                    } 

                    // For a struct/union/class, remember the C spelling,
                    // in case it has __attribute__((unavailable)).
                    // See issue 2888.
                    if (isStructUnionClass(t.Go)) {
                        t.Typedef = dt.Name;
                    }
                }

            }
            break;
    }
    return _addr_t!;
}

// FuncType returns the Go type analogous to dtype.
// There is no guarantee about matching memory layout.
private static ptr<FuncType> FuncType(this ptr<typeConv> _addr_c, ptr<dwarf.FuncType> _addr_dtype, token.Pos pos) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.FuncType dtype = ref _addr_dtype.val;

    var p = make_slice<ptr<Type>>(len(dtype.ParamType));
    var gp = make_slice<ptr<ast.Field>>(len(dtype.ParamType));
    foreach (var (i, f) in dtype.ParamType) { 
        // gcc's DWARF generator outputs a single DotDotDotType parameter for
        // function pointers that specify no parameters (e.g. void
        // (*__cgo_0)()).  Treat this special case as void. This case is
        // invalid according to ISO C anyway (i.e. void (*__cgo_1)(...) is not
        // legal).
        {
            ptr<dwarf.DotDotDotType> (_, ok) = f._<ptr<dwarf.DotDotDotType>>();

            if (ok && i == 0) {
                (p, gp) = (null, null);                break;
            }

        }
        p[i] = c.FuncArg(f, pos);
        gp[i] = addr(new ast.Field(Type:p[i].Go));
    }    ptr<Type> r;
    slice<ptr<ast.Field>> gr = default;
    {
        (_, ok) = base(dtype.ReturnType)._<ptr<dwarf.VoidType>>();

        if (ok) {
            gr = new slice<ptr<ast.Field>>(new ptr<ast.Field>[] { {Type:c.goVoid} });
        }
        else if (dtype.ReturnType != null) {
            r = c.Type(unqual(dtype.ReturnType), pos);
            gr = new slice<ptr<ast.Field>>(new ptr<ast.Field>[] { {Type:r.Go} });
        }

    }
    return addr(new FuncType(Params:p,Result:r,Go:&ast.FuncType{Params:&ast.FieldList{List:gp},Results:&ast.FieldList{List:gr},},));
}

// Identifier
private static ptr<ast.Ident> Ident(this ptr<typeConv> _addr_c, @string s) {
    ref typeConv c = ref _addr_c.val;

    return _addr_ast.NewIdent(s)!;
}

// Opaque type of n bytes.
private static ast.Expr Opaque(this ptr<typeConv> _addr_c, long n) {
    ref typeConv c = ref _addr_c.val;

    return addr(new ast.ArrayType(Len:c.intExpr(n),Elt:c.byte,));
}

// Expr for integer n.
private static ast.Expr intExpr(this ptr<typeConv> _addr_c, long n) {
    ref typeConv c = ref _addr_c.val;

    return addr(new ast.BasicLit(Kind:token.INT,Value:strconv.FormatInt(n,10),));
}

// Add padding of given size to fld.
private static (slice<ptr<ast.Field>>, slice<long>) pad(this ptr<typeConv> _addr_c, slice<ptr<ast.Field>> fld, slice<long> sizes, long size) {
    slice<ptr<ast.Field>> _p0 = default;
    slice<long> _p0 = default;
    ref typeConv c = ref _addr_c.val;

    var n = len(fld);
    fld = fld[(int)0..(int)n + 1];
    fld[n] = addr(new ast.Field(Names:[]*ast.Ident{c.Ident("_")},Type:c.Opaque(size)));
    sizes = sizes[(int)0..(int)n + 1];
    sizes[n] = size;
    return (fld, sizes);
}

// Struct conversion: return Go and (gc) C syntax for type.
private static (ptr<ast.StructType>, @string, long) Struct(this ptr<typeConv> _addr_c, ptr<dwarf.StructType> _addr_dt, token.Pos pos) {
    ptr<ast.StructType> expr = default!;
    @string csyntax = default;
    long align = default;
    ref typeConv c = ref _addr_c.val;
    ref dwarf.StructType dt = ref _addr_dt.val;
 
    // Minimum alignment for a struct is 1 byte.
    align = 1;

    bytes.Buffer buf = default;
    buf.WriteString("struct {");
    var fld = make_slice<ptr<ast.Field>>(0, 2 * len(dt.Field) + 1); // enough for padding around every field
    var sizes = make_slice<long>(0, 2 * len(dt.Field) + 1);
    var off = int64(0); 

    // Rename struct fields that happen to be named Go keywords into
    // _{keyword}.  Create a map from C ident -> Go ident. The Go ident will
    // be mangled. Any existing identifier that already has the same name on
    // the C-side will cause the Go-mangled version to be prefixed with _.
    // (e.g. in a struct with fields '_type' and 'type', the latter would be
    // rendered as '__type' in Go).
    var ident = make_map<@string, @string>();
    var used = make_map<@string, bool>();
    {
        var f__prev1 = f;

        foreach (var (_, __f) in dt.Field) {
            f = __f;
            ident[f.Name] = f.Name;
            used[f.Name] = true;
        }
        f = f__prev1;
    }

    if (!godefs.val) {
        foreach (var (cid, goid) in ident) {
            if (token.Lookup(goid).IsKeyword()) { 
                // Avoid keyword
                goid = "_" + goid; 

                // Also avoid existing fields
                {
                    var (_, exist) = used[goid];

                    while (exist) {
                        goid = "_" + goid;
                        _, exist = used[goid];
                    }

                }

                used[goid] = true;
                ident[cid] = goid;
            }
        }
    }
    nint anon = 0;
    {
        var f__prev1 = f;

        foreach (var (_, __f) in dt.Field) {
            f = __f;
            var name = f.Name;
            var ft = f.Type; 

            // In godefs mode, if this field is a C11
            // anonymous union then treat the first field in the
            // union as the field in the struct. This handles
            // cases like the glibc <sys/resource.h> file; see
            // issue 6677.
            if (godefs.val) {
                {
                    ptr<dwarf.StructType> (st, ok) = f.Type._<ptr<dwarf.StructType>>();

                    if (ok && name == "" && st.Kind == "union" && len(st.Field) > 0 && !used[st.Field[0].Name]) {
                        name = st.Field[0].Name;
                        ident[name] = name;
                        ft = st.Field[0].Type;
                    }

                }
            } 

            // TODO: Handle fields that are anonymous structs by
            // promoting the fields of the inner struct.
            var t = c.Type(ft, pos);
            var tgo = t.Go;
            var size = t.Size;
            var talign = t.Align;
            if (f.BitOffset > 0 || f.BitSize > 0) { 
                // The layout of bitfields is implementation defined,
                // so we don't know how they correspond to Go fields
                // even if they are aligned at byte boundaries.
                continue;
            }
            if (talign > 0 && f.ByteOffset % talign != 0) { 
                // Drop misaligned fields, the same way we drop integer bit fields.
                // The goal is to make available what can be made available.
                // Otherwise one bad and unneeded field in an otherwise okay struct
                // makes the whole program not compile. Much of the time these
                // structs are in system headers that cannot be corrected.
                continue;
            } 

            // Round off up to talign, assumed to be a power of 2.
            off = (off + talign - 1) & ~(talign - 1);

            if (f.ByteOffset > off) {
                fld, sizes = c.pad(fld, sizes, f.ByteOffset - off);
                off = f.ByteOffset;
            }
            if (f.ByteOffset < off) { 
                // Drop a packed field that we can't represent.
                continue;
            }
            var n = len(fld);
            fld = fld[(int)0..(int)n + 1];
            if (name == "") {
                name = fmt.Sprintf("anon%d", anon);
                anon++;
                ident[name] = name;
            }
            fld[n] = addr(new ast.Field(Names:[]*ast.Ident{c.Ident(ident[name])},Type:tgo));
            sizes = sizes[(int)0..(int)n + 1];
            sizes[n] = size;
            off += size;
            buf.WriteString(t.C.String());
            buf.WriteString(" ");
            buf.WriteString(name);
            buf.WriteString("; ");
            if (talign > align) {
                align = talign;
            }
        }
        f = f__prev1;
    }

    if (off < dt.ByteSize) {
        fld, sizes = c.pad(fld, sizes, dt.ByteSize - off);
        off = dt.ByteSize;
    }
    while (off > 0 && sizes[len(sizes) - 1] == 0) {
        n = len(sizes);
        fld = fld[(int)0..(int)n - 1];
        sizes = sizes[(int)0..(int)n - 1];
    }

    if (off != dt.ByteSize) {
        fatalf("%s: struct size calculation error off=%d bytesize=%d", lineno(pos), off, dt.ByteSize);
    }
    buf.WriteString("}");
    csyntax = buf.String();

    if (godefs.val) {
        godefsFields(fld);
    }
    expr = addr(new ast.StructType(Fields:&ast.FieldList{List:fld}));
    return ;
}

// dwarfHasPointer reports whether the DWARF type dt contains a pointer.
private static bool dwarfHasPointer(this ptr<typeConv> _addr_c, dwarf.Type dt, token.Pos pos) {
    ref typeConv c = ref _addr_c.val;

    switch (dt.type()) {
        case ptr<dwarf.AddrType> dt:
            return false;
            break;
        case ptr<dwarf.BoolType> dt:
            return false;
            break;
        case ptr<dwarf.CharType> dt:
            return false;
            break;
        case ptr<dwarf.EnumType> dt:
            return false;
            break;
        case ptr<dwarf.FloatType> dt:
            return false;
            break;
        case ptr<dwarf.ComplexType> dt:
            return false;
            break;
        case ptr<dwarf.FuncType> dt:
            return false;
            break;
        case ptr<dwarf.IntType> dt:
            return false;
            break;
        case ptr<dwarf.UcharType> dt:
            return false;
            break;
        case ptr<dwarf.UintType> dt:
            return false;
            break;
        case ptr<dwarf.VoidType> dt:
            return false;
            break;
        case ptr<dwarf.ArrayType> dt:
            return c.dwarfHasPointer(dt.Type, pos);
            break;
        case ptr<dwarf.PtrType> dt:
            return true;
            break;
        case ptr<dwarf.QualType> dt:
            return c.dwarfHasPointer(dt.Type, pos);
            break;
        case ptr<dwarf.StructType> dt:
            foreach (var (_, f) in dt.Field) {
                if (c.dwarfHasPointer(f.Type, pos)) {
                    return true;
                }
            }
            return false;
            break;
        case ptr<dwarf.TypedefType> dt:
            if (dt.Name == "_GoString_" || dt.Name == "_GoBytes_") {
                return true;
            }
            return c.dwarfHasPointer(dt.Type, pos);
            break;
        default:
        {
            var dt = dt.type();
            fatalf("%s: unexpected type: %s", lineno(pos), dt);
            return false;
            break;
        }
    }
}

private static @string upper(@string s) {
    if (s == "") {
        return "";
    }
    var (r, size) = utf8.DecodeRuneInString(s);
    if (r == '_') {
        return "X" + s;
    }
    return string(unicode.ToUpper(r)) + s[(int)size..];
}

// godefsFields rewrites field names for use in Go or C definitions.
// It strips leading common prefixes (like tv_ in tv_sec, tv_usec)
// converts names to upper case, and rewrites _ into Pad_godefs_n,
// so that all fields are exported.
private static void godefsFields(slice<ptr<ast.Field>> fld) {
    var prefix = fieldPrefix(fld);
    nint npad = 0;
    foreach (var (_, f) in fld) {
        foreach (var (_, n) in f.Names) {
            if (n.Name != prefix) {
                n.Name = strings.TrimPrefix(n.Name, prefix);
            }
            if (n.Name == "_") { 
                // Use exported name instead.
                n.Name = "Pad_cgo_" + strconv.Itoa(npad);
                npad++;
            }
            n.Name = upper(n.Name);
        }
    }
}

// fieldPrefix returns the prefix that should be removed from all the
// field names when generating the C or Go code. For generated
// C, we leave the names as is (tv_sec, tv_usec), since that's what
// people are used to seeing in C.  For generated Go code, such as
// package syscall's data structures, we drop a common prefix
// (so sec, usec, which will get turned into Sec, Usec for exporting).
private static @string fieldPrefix(slice<ptr<ast.Field>> fld) {
    @string prefix = "";
    foreach (var (_, f) in fld) {
        foreach (var (_, n) in f.Names) { 
            // Ignore field names that don't have the prefix we're
            // looking for. It is common in C headers to have fields
            // named, say, _pad in an otherwise prefixed header.
            // If the struct has 3 fields tv_sec, tv_usec, _pad1, then we
            // still want to remove the tv_ prefix.
            // The check for "orig_" here handles orig_eax in the
            // x86 ptrace register sets, which otherwise have all fields
            // with reg_ prefixes.
            if (strings.HasPrefix(n.Name, "orig_") || strings.HasPrefix(n.Name, "_")) {
                continue;
            }
            var i = strings.Index(n.Name, "_");
            if (i < 0) {
                continue;
            }
            if (prefix == "") {
                prefix = n.Name[..(int)i + 1];
            }
            else if (prefix != n.Name[..(int)i + 1]) {
                return "";
            }
        }
    }    return prefix;
}

// anonymousStructTypedef reports whether dt is a C typedef for an anonymous
// struct.
private static bool anonymousStructTypedef(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;

    ptr<dwarf.StructType> (st, ok) = dt.Type._<ptr<dwarf.StructType>>();
    return ok && st.StructName == "";
}

// badPointerTypedef reports whether dt is a C typedef that should not be
// considered a pointer in Go. A typedef is bad if C code sometimes stores
// non-pointers in this type.
// TODO: Currently our best solution is to find these manually and list them as
// they come up. A better solution is desired.
// Note: DEPRECATED. There is now a better solution. Search for NotInHeap in this file.
private static bool badPointerTypedef(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;

    if (c.badCFType(dt)) {
        return true;
    }
    if (c.badJNI(dt)) {
        return true;
    }
    if (c.badEGLType(dt)) {
        return true;
    }
    return false;
}

// baseBadPointerTypedef reports whether the base of a chain of typedefs is a bad typedef
// as badPointerTypedef reports.
private static bool baseBadPointerTypedef(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;

    while (true) {
        {
            ptr<dwarf.TypedefType> (t, ok) = dt.Type._<ptr<dwarf.TypedefType>>();

            if (ok) {
                dt = t;
                continue;
            }

        }
        break;
    }
    return c.badPointerTypedef(dt);
}

private static bool badCFType(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;
 
    // The real bad types are CFNumberRef and CFDateRef.
    // Sometimes non-pointers are stored in these types.
    // CFTypeRef is a supertype of those, so it can have bad pointers in it as well.
    // We return true for the other *Ref types just so casting between them is easier.
    // We identify the correct set of types as those ending in Ref and for which
    // there exists a corresponding GetTypeID function.
    // See comment below for details about the bad pointers.
    if (goos != "darwin" && goos != "ios") {
        return false;
    }
    var s = dt.Name;
    if (!strings.HasSuffix(s, "Ref")) {
        return false;
    }
    s = s[..(int)len(s) - 3];
    if (s == "CFType") {
        return true;
    }
    if (c.getTypeIDs[s]) {
        return true;
    }
    {
        var i = strings.Index(s, "Mutable");

        if (i >= 0 && c.getTypeIDs[s[..(int)i] + s[(int)i + 7..]]) { 
            // Mutable and immutable variants share a type ID.
            return true;
        }
    }
    return false;
}

// Comment from Darwin's CFInternal.h
/*
// Tagged pointer support
// Low-bit set means tagged object, next 3 bits (currently)
// define the tagged object class, next 4 bits are for type
// information for the specific tagged object class.  Thus,
// the low byte is for type info, and the rest of a pointer
// (32 or 64-bit) is for payload, whatever the tagged class.
//
// Note that the specific integers used to identify the
// specific tagged classes can and will change from release
// to release (that's why this stuff is in CF*Internal*.h),
// as can the definition of type info vs payload above.
//
#if __LP64__
#define CF_IS_TAGGED_OBJ(PTR)    ((uintptr_t)(PTR) & 0x1)
#define CF_TAGGED_OBJ_TYPE(PTR)    ((uintptr_t)(PTR) & 0xF)
#else
#define CF_IS_TAGGED_OBJ(PTR)    0
#define CF_TAGGED_OBJ_TYPE(PTR)    0
#endif

enum {
    kCFTaggedObjectID_Invalid = 0,
    kCFTaggedObjectID_Atom = (0 << 1) + 1,
    kCFTaggedObjectID_Undefined3 = (1 << 1) + 1,
    kCFTaggedObjectID_Undefined2 = (2 << 1) + 1,
    kCFTaggedObjectID_Integer = (3 << 1) + 1,
    kCFTaggedObjectID_DateTS = (4 << 1) + 1,
    kCFTaggedObjectID_ManagedObjectID = (5 << 1) + 1, // Core Data
    kCFTaggedObjectID_Date = (6 << 1) + 1,
    kCFTaggedObjectID_Undefined7 = (7 << 1) + 1,
};
*/

private static bool badJNI(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;
 
    // In Dalvik and ART, the jobject type in the JNI interface of the JVM has the
    // property that it is sometimes (always?) a small integer instead of a real pointer.
    // Note: although only the android JVMs are bad in this respect, we declare the JNI types
    // bad regardless of platform, so the same Go code compiles on both android and non-android.
    {
        var (parent, ok) = jniTypes[dt.Name];

        if (ok) { 
            // Try to make sure we're talking about a JNI type, not just some random user's
            // type that happens to use the same name.
            // C doesn't have the notion of a package, so it's hard to be certain.

            // Walk up to jobject, checking each typedef on the way.
            var w = dt;
            while (parent != "") {
                ptr<dwarf.TypedefType> (t, ok) = w.Type._<ptr<dwarf.TypedefType>>();
                if (!ok || t.Name != parent) {
                    return false;
                }
                w = t;
                parent, ok = jniTypes[w.Name];
                if (!ok) {
                    return false;
                }
            } 

            // Check that the typedef is either:
            // 1:
            //         struct _jobject;
            //         typedef struct _jobject *jobject;
            // 2: (in NDK16 in C++)
            //         class _jobject {};
            //         typedef _jobject* jobject;
            // 3: (in NDK16 in C)
            //         typedef void* jobject;
 

            // Check that the typedef is either:
            // 1:
            //         struct _jobject;
            //         typedef struct _jobject *jobject;
            // 2: (in NDK16 in C++)
            //         class _jobject {};
            //         typedef _jobject* jobject;
            // 3: (in NDK16 in C)
            //         typedef void* jobject;
            {
                ptr<dwarf.PtrType> (ptr, ok) = w.Type._<ptr<dwarf.PtrType>>();

                if (ok) {
                    switch (ptr.Type.type()) {
                        case ptr<dwarf.VoidType> v:
                            return true;
                            break;
                        case ptr<dwarf.StructType> v:
                            if (v.StructName == "_jobject" && len(v.Field) == 0) {
                                switch (v.Kind) {
                                    case "struct": 
                                        if (v.Incomplete) {
                                            return true;
                                        }
                                        break;
                                    case "class": 
                                        if (!v.Incomplete) {
                                            return true;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }

            }
        }
    }
    return false;
}

private static bool badEGLType(this ptr<typeConv> _addr_c, ptr<dwarf.TypedefType> _addr_dt) {
    ref typeConv c = ref _addr_c.val;
    ref dwarf.TypedefType dt = ref _addr_dt.val;

    if (dt.Name != "EGLDisplay" && dt.Name != "EGLConfig") {
        return false;
    }
    {
        ptr<dwarf.PtrType> (ptr, ok) = dt.Type._<ptr<dwarf.PtrType>>();

        if (ok) {
            {
                ptr<dwarf.VoidType> (_, ok) = ptr.Type._<ptr<dwarf.VoidType>>();

                if (ok) {
                    return true;
                }

            }
        }
    }
    return false;
}

// jniTypes maps from JNI types that we want to be uintptrs, to the underlying type to which
// they are mapped. The base "jobject" maps to the empty string.
private static map jniTypes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"jobject":"","jclass":"jobject","jthrowable":"jobject","jstring":"jobject","jarray":"jobject","jbooleanArray":"jarray","jbyteArray":"jarray","jcharArray":"jarray","jshortArray":"jarray","jintArray":"jarray","jlongArray":"jarray","jfloatArray":"jarray","jdoubleArray":"jarray","jobjectArray":"jarray","jweak":"jobject",};

} // end main_package
