// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Annotate Ref in Prog with C types by parsing gcc debug output.
// Conversion of debug output to Go types.

// package main -- go2cs converted at 2020 August 29 08:52:13 UTC
// Original source: C:\Go\src\cmd\cgo\gcc.go
using bytes = go.bytes_package;
using dwarf = go.debug.dwarf_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using math = go.math_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static var debugDefine = flag.Bool("debug-define", false, "print relevant #defines");
        private static var debugGcc = flag.Bool("debug-gcc", false, "print gcc invocations");

        private static map nameToC = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"schar":"signed char","uchar":"unsigned char","ushort":"unsigned short","uint":"unsigned int","ulong":"unsigned long","longlong":"long long","ulonglong":"unsigned long long","complexfloat":"float _Complex","complexdouble":"double _Complex",};

        // cname returns the C name to use for C.s.
        // The expansions are listed in nameToC and also
        // struct_foo becomes "struct foo", and similarly for
        // union and enum.
        private static @string cname(@string s)
        {
            {
                var (t, ok) = nameToC[s];

                if (ok)
                {
                    return t;
                }

            }

            if (strings.HasPrefix(s, "struct_"))
            {
                return "struct " + s[len("struct_")..];
            }
            if (strings.HasPrefix(s, "union_"))
            {
                return "union " + s[len("union_")..];
            }
            if (strings.HasPrefix(s, "enum_"))
            {
                return "enum " + s[len("enum_")..];
            }
            if (strings.HasPrefix(s, "sizeof_"))
            {
                return "sizeof(" + cname(s[len("sizeof_")..]) + ")";
            }
            return s;
        }

        // DiscardCgoDirectives processes the import C preamble, and discards
        // all #cgo CFLAGS and LDFLAGS directives, so they don't make their
        // way into _cgo_export.h.
        private static void DiscardCgoDirectives(this ref File f)
        {
            var linesIn = strings.Split(f.Preamble, "\n");
            var linesOut = make_slice<@string>(0L, len(linesIn));
            foreach (var (_, line) in linesIn)
            {
                var l = strings.TrimSpace(line);
                if (len(l) < 5L || l[..4L] != "#cgo" || !unicode.IsSpace(rune(l[4L])))
                {
                    linesOut = append(linesOut, line);
                }
                else
                {
                    linesOut = append(linesOut, "");
                }
            }
            f.Preamble = strings.Join(linesOut, "\n");
        }

        // addToFlag appends args to flag. All flags are later written out onto the
        // _cgo_flags file for the build system to use.
        private static void addToFlag(this ref Package p, @string flag, slice<@string> args)
        {
            p.CgoFlags[flag] = append(p.CgoFlags[flag], args);
            if (flag == "CFLAGS")
            { 
                // We'll also need these when preprocessing for dwarf information.
                p.GccOptions = append(p.GccOptions, args);
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
        private static (slice<@string>, error) splitQuoted(@string s)
        {
            slice<@string> args = default;
            var arg = make_slice<int>(len(s));
            var escaped = false;
            var quoted = false;
            char quote = '\x00';
            long i = 0L;
            foreach (var (_, r) in s)
            {

                if (escaped) 
                    escaped = false;
                else if (r == '\\') 
                    escaped = true;
                    continue;
                else if (quote != 0L) 
                    if (r == quote)
                    {
                        quote = 0L;
                        continue;
                    }
                else if (r == '"' || r == '\'') 
                    quoted = true;
                    quote = r;
                    continue;
                else if (unicode.IsSpace(r)) 
                    if (quoted || i > 0L)
                    {
                        quoted = false;
                        args = append(args, string(arg[..i]));
                        i = 0L;
                    }
                    continue;
                                arg[i] = r;
                i++;
            }
            if (quoted || i > 0L)
            {
                args = append(args, string(arg[..i]));
            }
            if (quote != 0L)
            {
                err = errors.New("unclosed quote");
            }
            else if (escaped)
            {
                err = errors.New("unfinished escaping");
            }
            return (args, err);
        }

        // Translate rewrites f.AST, the original Go input, to remove
        // references to the imported package C, replacing them with
        // references to the equivalent Go types, functions, and variables.
        private static void Translate(this ref Package p, ref File f)
        {
            foreach (var (_, cref) in f.Ref)
            { 
                // Convert C.ulong to C.unsigned long, etc.
                cref.Name.C = cname(cref.Name.Go);
            }
            p.loadDefines(f);
            var needType = p.guessKinds(f);
            if (len(needType) > 0L)
            {
                p.loadDWARF(f, needType);
            }
            if (p.rewriteCalls(f))
            { 
                // Add `import _cgo_unsafe "unsafe"` after the package statement.
                f.Edit.Insert(f.offset(f.AST.Name.End()), "; import _cgo_unsafe \"unsafe\"");
            }
            p.rewriteRef(f);
        }

        // loadDefines coerces gcc into spitting out the #defines in use
        // in the file f and saves relevant renamings in f.Name[name].Define.
        private static void loadDefines(this ref Package p, ref File f)
        {
            bytes.Buffer b = default;
            b.WriteString(builtinProlog);
            b.WriteString(f.Preamble);
            var stdout = p.gccDefines(b.Bytes());

            foreach (var (_, line) in strings.Split(stdout, "\n"))
            {
                if (len(line) < 9L || line[0L..7L] != "#define")
                {
                    continue;
                }
                line = strings.TrimSpace(line[8L..]);

                @string key = default;                @string val = default;

                var spaceIndex = strings.Index(line, " ");
                var tabIndex = strings.Index(line, "\t");

                if (spaceIndex == -1L && tabIndex == -1L)
                {
                    continue;
                }
                else if (tabIndex == -1L || (spaceIndex != -1L && spaceIndex < tabIndex))
                {
                    key = line[0L..spaceIndex];
                    val = strings.TrimSpace(line[spaceIndex..]);
                }
                else
                {
                    key = line[0L..tabIndex];
                    val = strings.TrimSpace(line[tabIndex..]);
                }
                if (key == "__clang__")
                {
                    p.GccIsClang = true;
                }
                {
                    var n = f.Name[key];

                    if (n != null)
                    {
                        if (debugDefine.Value)
                        {
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
        private static slice<ref Name> guessKinds(this ref Package p, ref File f)
        { 
            // Determine kinds for names we already know about,
            // like #defines or 'struct foo', before bothering with gcc.
            slice<ref Name> names = default;            slice<ref Name> needType = default;

            map optional = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Name, bool>{};
            foreach (var (_, key) in nameKeys(f.Name))
            {
                var n = f.Name[key]; 
                // If we've already found this name as a #define
                // and we can translate it as a constant value, do so.
                if (n.Define != "")
                {
                    {
                        var i__prev2 = i;

                        var (i, err) = strconv.ParseInt(n.Define, 0L, 64L);

                        if (err == null)
                        {
                            n.Kind = "iconst"; 
                            // Turn decimal into hex, just for consistency
                            // with enum-derived constants. Otherwise
                            // in the cgo -godefs output half the constants
                            // are in hex and half are in whatever the #define used.
                            n.Const = fmt.Sprintf("%#x", i);
                        }
                        else if (n.Define[0L] == '\'')
                        {
                            {
                                var (_, err) = parser.ParseExpr(n.Define);

                                if (err == null)
                                {
                                    n.Kind = "iconst";
                                    n.Const = n.Define;
                                }

                            }
                        }
                        else if (n.Define[0L] == '"')
                        {
                            {
                                (_, err) = parser.ParseExpr(n.Define);

                                if (err == null)
                                {
                                    n.Kind = "sconst";
                                    n.Const = n.Define;
                                }

                            }
                        }

                        i = i__prev2;

                    }

                    if (n.IsConst())
                    {
                        continue;
                    }
                } 

                // If this is a struct, union, or enum type name, no need to guess the kind.
                if (strings.HasPrefix(n.C, "struct ") || strings.HasPrefix(n.C, "union ") || strings.HasPrefix(n.C, "enum "))
                {
                    n.Kind = "type";
                    needType = append(needType, n);
                    continue;
                }
                if (goos == "darwin" && strings.HasSuffix(n.C, "Ref"))
                { 
                    // For FooRef, find out if FooGetTypeID exists.
                    var s = n.C[..len(n.C) - 3L] + "GetTypeID";
                    n = ref new Name(Go:s,C:s);
                    names = append(names, n);
                    optional[n] = true;
                } 

                // Otherwise, we'll need to find out from gcc.
                names = append(names, n);
            } 

            // Bypass gcc if there's nothing left to find out.
            if (len(names) == 0L)
            {
                return needType;
            } 

            // Coerce gcc into telling us whether each name is a type, a value, or undeclared.
            // For names, find out whether they are integer constants.
            // We used to look at specific warning or error messages here, but that tied the
            // behavior too closely to specific versions of the compilers.
            // Instead, arrange that we can infer what we need from only the presence or absence
            // of an error on a specific line.
            //
            // For each name, we generate these lines, where xxx is the index in toSniff plus one.
            //
            //    #line xxx "not-declared"
            //    void __cgo_f_xxx_1(void) { __typeof__(name) *__cgo_undefined__1; }
            //    #line xxx "not-type"
            //    void __cgo_f_xxx_2(void) { name *__cgo_undefined__2; }
            //    #line xxx "not-int-const"
            //    void __cgo_f_xxx_3(void) { enum { __cgo_undefined__3 = (name)*1 }; }
            //    #line xxx "not-num-const"
            //    void __cgo_f_xxx_4(void) { static const double __cgo_undefined__4 = (name); }
            //    #line xxx "not-str-lit"
            //    void __cgo_f_xxx_5(void) { static const char __cgo_undefined__5[] = (name); }
            //
            // If we see an error at not-declared:xxx, the corresponding name is not declared.
            // If we see an error at not-type:xxx, the corresponding name is a type.
            // If we see an error at not-int-const:xxx, the corresponding name is not an integer constant.
            // If we see an error at not-num-const:xxx, the corresponding name is not a number constant.
            // If we see an error at not-str-lit:xxx, the corresponding name is not a string literal.
            //
            // The specific input forms are chosen so that they are valid C syntax regardless of
            // whether name denotes a type or an expression.
            bytes.Buffer b = default;
            b.WriteString(builtinProlog);
            b.WriteString(f.Preamble);

            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
                    i = __i;
                    n = __n;
                    fmt.Fprintf(ref b, "#line %d \"not-declared\"\n" + "void __cgo_f_%d_1(void) { __typeof__(%s) *__cgo_undefined__1; }\n" + "#line %d \"not-type\"\n" + "void __cgo_f_%d_2(void) { %s *__cgo_undefined__2; }\n" + "#line %d \"not-int-const\"\n" + "void __cgo_f_%d_3(void) { enum { __cgo_undefined__3 = (%s)*1 }; }\n" + "#line %d \"not-num-const\"\n" + "void __cgo_f_%d_4(void) { static const double __cgo_undefined__4 = (%s); }\n" + "#line %d \"not-str-lit\"\n" + "void __cgo_f_%d_5(void) { static const char __cgo_undefined__5[] = (%s); }\n", i + 1L, i + 1L, n.C, i + 1L, i + 1L, n.C, i + 1L, i + 1L, n.C, i + 1L, i + 1L, n.C, i + 1L, i + 1L, n.C);
                }

                i = i__prev1;
                n = n__prev1;
            }

            fmt.Fprintf(ref b, "#line 1 \"completed\"\n" + "int __cgo__1 = __cgo__2;\n");

            var stderr = p.gccErrors(b.Bytes());
            if (stderr == "")
            {
                fatalf("%s produced no output\non input:\n%s", p.gccBaseCmd()[0L], b.Bytes());
            }
            var completed = false;
            var sniff = make_slice<long>(len(names));
            const long notType = 1L << (int)(iota);
            const var notIntConst = 0;
            const var notNumConst = 1;
            const var notStrLiteral = 2;
            const var notDeclared = 3;
            var sawUnmatchedErrors = false;
            foreach (var (_, line) in strings.Split(stderr, "\n"))
            { 
                // Ignore warnings and random comments, with one
                // exception: newer GCC versions will sometimes emit
                // an error on a macro #define with a note referring
                // to where the expansion occurs. We care about where
                // the expansion occurs, so in that case treat the note
                // as an error.
                var isError = strings.Contains(line, ": error:");
                var isErrorNote = strings.Contains(line, ": note:") && sawUnmatchedErrors;
                if (!isError && !isErrorNote)
                {
                    continue;
                }
                var c1 = strings.Index(line, ":");
                if (c1 < 0L)
                {
                    continue;
                }
                var c2 = strings.Index(line[c1 + 1L..], ":");
                if (c2 < 0L)
                {
                    continue;
                }
                c2 += c1 + 1L;

                var filename = line[..c1];
                var (i, _) = strconv.Atoi(line[c1 + 1L..c2]);
                i--;
                if (i < 0L || i >= len(names))
                {
                    if (isError)
                    {
                        sawUnmatchedErrors = true;
                    }
                    continue;
                }
                switch (filename)
                {
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
                        if (isError)
                        {
                            sawUnmatchedErrors = true;
                        }
                        continue;
                        break;
                }

                sawUnmatchedErrors = false;
            }
            if (!completed)
            {
                fatalf("%s did not produce error at completed:1\non input:\n%s\nfull error output:\n%s", p.gccBaseCmd()[0L], b.Bytes(), stderr);
            }
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
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
                        if (sniff[i] & notDeclared != 0L && optional[n])
                        { 
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

            if (nerrors > 0L)
            { 
                // Check if compiling the preamble by itself causes any errors,
                // because the messages we've printed out so far aren't helpful
                // to users debugging preamble mistakes. See issue 8442.
                var preambleErrors = p.gccErrors((slice<byte>)f.Preamble);
                if (len(preambleErrors) > 0L)
                {
                    error_(token.NoPos, "\n%s errors for preamble:\n%s", p.gccBaseCmd()[0L], preambleErrors);
                }
                fatalf("unresolved names");
            }
            return needType;
        }

        // loadDWARF parses the DWARF debug information generated
        // by gcc to learn the details of the constants, variables, and types
        // being referred to as C.xxx.
        private static void loadDWARF(this ref Package p, ref File f, slice<ref Name> names)
        { 
            // Extract the types from the DWARF section of an object
            // from a well-formed C program. Gcc only generates DWARF info
            // for symbols in the object file, so it is not enough to print the
            // preamble and hope the symbols we care about will be there.
            // Instead, emit
            //    __typeof__(names[i]) *__cgo__i;
            // for each entry in names and then dereference the type we
            // learn for __cgo__i.
            bytes.Buffer b = default;
            b.WriteString(builtinProlog);
            b.WriteString(f.Preamble);
            b.WriteString("#line 1 \"cgo-dwarf-inference\"\n");
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
                    i = __i;
                    n = __n;
                    fmt.Fprintf(ref b, "__typeof__(%s) *__cgo__%d;\n", n.C, i);
                    if (n.Kind == "iconst")
                    {
                        fmt.Fprintf(ref b, "enum { __cgo_enum__%d = %s };\n", i, n.C);
                    }
                } 

                // We create a data block initialized with the values,
                // so we can read them out of the object file.

                i = i__prev1;
                n = n__prev1;
            }

            fmt.Fprintf(ref b, "long long __cgodebug_ints[] = {\n");
            {
                var n__prev1 = n;

                foreach (var (_, __n) in names)
                {
                    n = __n;
                    if (n.Kind == "iconst")
                    {
                        fmt.Fprintf(ref b, "\t%s,\n", n.C);
                    }
                    else
                    {
                        fmt.Fprintf(ref b, "\t0,\n");
                    }
                } 
                // for the last entry, we cannot use 0, otherwise
                // in case all __cgodebug_data is zero initialized,
                // LLVM-based gcc will place the it in the __DATA.__common
                // zero-filled section (our debug/macho doesn't support
                // this)

                n = n__prev1;
            }

            fmt.Fprintf(ref b, "\t1\n");
            fmt.Fprintf(ref b, "};\n"); 

            // do the same work for floats.
            fmt.Fprintf(ref b, "double __cgodebug_floats[] = {\n");
            {
                var n__prev1 = n;

                foreach (var (_, __n) in names)
                {
                    n = __n;
                    if (n.Kind == "fconst")
                    {
                        fmt.Fprintf(ref b, "\t%s,\n", n.C);
                    }
                    else
                    {
                        fmt.Fprintf(ref b, "\t0,\n");
                    }
                }

                n = n__prev1;
            }

            fmt.Fprintf(ref b, "\t1\n");
            fmt.Fprintf(ref b, "};\n"); 

            // do the same work for strings.
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
                    i = __i;
                    n = __n;
                    if (n.Kind == "sconst")
                    {
                        fmt.Fprintf(ref b, "const char __cgodebug_str__%d[] = %s;\n", i, n.C);
                        fmt.Fprintf(ref b, "const unsigned long long __cgodebug_strlen__%d = sizeof(%s)-1;\n", i, n.C);
                    }
                }

                i = i__prev1;
                n = n__prev1;
            }

            var (d, ints, floats, strs) = p.gccDebug(b.Bytes(), len(names)); 

            // Scan DWARF info for top-level TagVariable entries with AttrName __cgo__i.
            var types = make_slice<dwarf.Type>(len(names));
            var r = d.Reader();
            while (true)
            {
                var (e, err) = r.Next();
                if (err != null)
                {
                    fatalf("reading DWARF entry: %s", err);
                }
                if (e == null)
                {
                    break;
                }

                if (e.Tag == dwarf.TagVariable) 
                    @string (name, _) = e.Val(dwarf.AttrName)._<@string>();
                    dwarf.Offset (typOff, _) = e.Val(dwarf.AttrType)._<dwarf.Offset>();
                    if (name == "" || typOff == 0L)
                    {
                        if (e.Val(dwarf.AttrSpecification) != null)
                        { 
                            // Since we are reading all the DWARF,
                            // assume we will see the variable elsewhere.
                            break;
                        }
                        fatalf("malformed DWARF TagVariable entry");
                    }
                    if (!strings.HasPrefix(name, "__cgo__"))
                    {
                        break;
                    }
                    var (typ, err) = d.Type(typOff);
                    if (err != null)
                    {
                        fatalf("loading DWARF type: %s", err);
                    }
                    ref dwarf.PtrType (t, ok) = typ._<ref dwarf.PtrType>();
                    if (!ok || t == null)
                    {
                        fatalf("internal error: %s has non-pointer type", name);
                    }
                    var (i, err) = strconv.Atoi(name[7L..]);
                    if (err != null)
                    {
                        fatalf("malformed __cgo__ name: %s", name);
                    }
                    types[i] = t.Type;
                                if (e.Tag != dwarf.TagCompileUnit)
                {
                    r.SkipChildren();
                }
            } 

            // Record types and typedef information.
 

            // Record types and typedef information.
            typeConv conv = default;
            conv.Init(p.PtrSize, p.IntSize);
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
                    i = __i;
                    n = __n;
                    if (strings.HasSuffix(n.Go, "GetTypeID") && types[i].String() == "func() CFTypeID")
                    {
                        conv.getTypeIDs[n.Go[..len(n.Go) - 9L]] = true;
                    }
                }

                i = i__prev1;
                n = n__prev1;
            }

            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in names)
                {
                    i = __i;
                    n = __n;
                    if (types[i] == null)
                    {
                        continue;
                    }
                    var pos = f.NamePos[n];
                    ref dwarf.FuncType (f, fok) = types[i]._<ref dwarf.FuncType>();
                    if (n.Kind != "type" && fok)
                    {
                        n.Kind = "func";
                        n.FuncType = conv.FuncType(f, pos);
                    }
                    else
                    {
                        n.Type = conv.Type(types[i], pos);
                        switch (n.Kind)
                        {
                            case "iconst": 
                                if (i < len(ints))
                                {
                                    {
                                        ref dwarf.UintType (_, ok) = types[i]._<ref dwarf.UintType>();

                                        if (ok)
                                        {
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
                                if (i < len(floats))
                                {
                                    n.Const = fmt.Sprintf("%f", floats[i]);
                                }
                                break;
                            case "sconst": 
                                if (i < len(strs))
                                {
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

        // mangleName does name mangling to translate names
        // from the original Go source files to the names
        // used in the final Go files generated by cgo.
        private static void mangleName(this ref Package p, ref Name n)
        { 
            // When using gccgo variables have to be
            // exported so that they become global symbols
            // that the C code can refer to.
            @string prefix = "_C";
            if (gccgo && n.IsVar().Value)
            {
                prefix = "C";
            }
            n.Mangle = prefix + n.Kind + "_" + n.Go;
        }

        // rewriteCalls rewrites all calls that pass pointers to check that
        // they follow the rules for passing pointers between Go and C.
        // This returns whether the package needs to import unsafe as _cgo_unsafe.
        private static bool rewriteCalls(this ref Package p, ref File f)
        {
            var needsUnsafe = false;
            foreach (var (_, call) in f.Calls)
            { 
                // This is a call to C.xxx; set goname to "xxx".
                ref ast.SelectorExpr goname = call.Call.Fun._<ref ast.SelectorExpr>().Sel.Name;
                if (goname == "malloc")
                {
                    continue;
                }
                var name = f.Name[goname];
                if (name.Kind != "func")
                { 
                    // Probably a type conversion.
                    continue;
                }
                if (p.rewriteCall(f, call, name))
                {
                    needsUnsafe = true;
                }
            }
            return needsUnsafe;
        }

        // rewriteCall rewrites one call to add pointer checks.
        // If any pointer checks are required, we rewrite the call into a
        // function literal that calls _cgoCheckPointer for each pointer
        // argument and then calls the original function.
        // This returns whether the package needs to import unsafe as _cgo_unsafe.
        private static bool rewriteCall(this ref Package p, ref File f, ref Call call, ref Name name)
        { 
            // Avoid a crash if the number of arguments is
            // less than the number of parameters.
            // This will be caught when the generated file is compiled.
            if (len(call.Call.Args) < len(name.FuncType.Params))
            {
                return false;
            }
            var any = false;
            {
                var i__prev1 = i;
                var param__prev1 = param;

                foreach (var (__i, __param) in name.FuncType.Params)
                {
                    i = __i;
                    param = __param;
                    if (p.needsPointerCheck(f, param.Go, call.Call.Args[i]))
                    {
                        any = true;
                        break;
                    }
                }

                i = i__prev1;
                param = param__prev1;
            }

            if (!any)
            {
                return false;
            } 

            // We need to rewrite this call.
            //
            // We are going to rewrite C.f(p) to
            //    func (_cgo0 ptype) {
            //            _cgoCheckPointer(_cgo0)
            //            C.f(_cgo0)
            //    }(p)
            // Using a function literal like this lets us do correct
            // argument type checking, and works correctly if the call is
            // deferred.
            var needsUnsafe = false;
            var @params = make_slice<ref ast.Field>(len(name.FuncType.Params));
            var nargs = make_slice<ast.Expr>(len(name.FuncType.Params));
            slice<ast.Stmt> stmts = default;
            {
                var i__prev1 = i;
                var param__prev1 = param;

                foreach (var (__i, __param) in name.FuncType.Params)
                {
                    i = __i;
                    param = __param; 
                    // params is going to become the parameters of the
                    // function literal.
                    // nargs is going to become the list of arguments made
                    // by the call within the function literal.
                    // nparam is the parameter of the function literal that
                    // corresponds to param.

                    var origArg = call.Call.Args[i];
                    var nparam = ast.NewIdent(fmt.Sprintf("_cgo%d", i));
                    nargs[i] = nparam; 

                    // The Go version of the C type might use unsafe.Pointer,
                    // but the file might not import unsafe.
                    // Rewrite the Go type if necessary to use _cgo_unsafe.
                    var ptype = p.rewriteUnsafe(param.Go);
                    if (ptype != param.Go)
                    {
                        needsUnsafe = true;
                    }
                    params[i] = ref new ast.Field(Names:[]*ast.Ident{nparam},Type:ptype,);

                    if (!p.needsPointerCheck(f, param.Go, origArg))
                    {
                        continue;
                    } 

                    // Run the cgo pointer checks on nparam.

                    // Change the function literal to call the real function
                    // with the parameter passed through _cgoCheckPointer.
                    ast.CallExpr c = ref new ast.CallExpr(Fun:ast.NewIdent("_cgoCheckPointer"),Args:[]ast.Expr{nparam,},); 

                    // Add optional additional arguments for an address
                    // expression.
                    c.Args = p.checkAddrArgs(f, c.Args, origArg);

                    ast.ExprStmt stmt = ref new ast.ExprStmt(X:c,);
                    stmts = append(stmts, stmt);
                }

                i = i__prev1;
                param = param__prev1;
            }

            const @string cgoMarker = "__cgo__###__marker__";

            ast.CallExpr fcall = ref new ast.CallExpr(Fun:ast.NewIdent(cgoMarker),Args:nargs,);
            ast.FuncType ftype = ref new ast.FuncType(Params:&ast.FieldList{List:params,},);
            if (name.FuncType.Result != null)
            {
                var rtype = p.rewriteUnsafe(name.FuncType.Result.Go);
                if (rtype != name.FuncType.Result.Go)
                {
                    needsUnsafe = true;
                }
                ftype.Results = ref new ast.FieldList(List:[]*ast.Field{&ast.Field{Type:rtype,},},);
            } 

            // If this call expects two results, we have to
            // adjust the results of the function we generated.
            foreach (var (_, ref) in f.Ref)
            {
                if (@ref.Expr == ref call.Call.Fun && @ref.Context == ctxCall2)
                {
                    if (ftype.Results == null)
                    { 
                        // An explicit void argument
                        // looks odd but it seems to
                        // be how cgo has worked historically.
                        ftype.Results = ref new ast.FieldList(List:[]*ast.Field{&ast.Field{Type:ast.NewIdent("_Ctype_void"),},},);
                    }
                    ftype.Results.List = append(ftype.Results.List, ref new ast.Field(Type:ast.NewIdent("error"),));
                }
            }
            ast.Stmt fbody = default;
            if (ftype.Results == null)
            {
                fbody = ref new ast.ExprStmt(X:fcall,);
            }
            else
            {
                fbody = ref new ast.ReturnStmt(Results:[]ast.Expr{fcall},);
            }
            ast.FuncLit lit = ref new ast.FuncLit(Type:ftype,Body:&ast.BlockStmt{List:append(stmts,fbody),},);
            var text = strings.Replace(gofmt(lit), "\n", ";", -1L);
            var repl = strings.Split(text, cgoMarker);
            f.Edit.Insert(f.offset(call.Call.Fun.Pos()), repl[0L]);
            f.Edit.Insert(f.offset(call.Call.Fun.End()), repl[1L]);

            return needsUnsafe;
        }

        // needsPointerCheck returns whether the type t needs a pointer check.
        // This is true if t is a pointer and if the value to which it points
        // might contain a pointer.
        private static bool needsPointerCheck(this ref Package p, ref File f, ast.Expr t, ast.Expr arg)
        { 
            // An untyped nil does not need a pointer check, and when
            // _cgoCheckPointer returns the untyped nil the type assertion we
            // are going to insert will fail.  Easier to just skip nil arguments.
            // TODO: Note that this fails if nil is shadowed.
            {
                ref ast.Ident (id, ok) = arg._<ref ast.Ident>();

                if (ok && id.Name == "nil")
                {
                    return false;
                }

            }

            return p.hasPointer(f, t, true);
        }

        // hasPointer is used by needsPointerCheck. If top is true it returns
        // whether t is or contains a pointer that might point to a pointer.
        // If top is false it returns whether t is or contains a pointer.
        // f may be nil.
        private static bool hasPointer(this ref Package p, ref File f, ast.Expr t, bool top)
        {
            switch (t.type())
            {
                case ref ast.ArrayType t:
                    if (t.Len == null)
                    {
                        if (!top)
                        {
                            return true;
                        }
                        return p.hasPointer(f, t.Elt, false);
                    }
                    return p.hasPointer(f, t.Elt, top);
                    break;
                case ref ast.StructType t:
                    foreach (var (_, field) in t.Fields.List)
                    {
                        if (p.hasPointer(f, field.Type, top))
                        {
                            return true;
                        }
                    }
                    return false;
                    break;
                case ref ast.StarExpr t:
                    if (!top)
                    {
                        return true;
                    } 
                    // Check whether this is a pointer to a C union (or class)
                    // type that contains a pointer.
                    if (unionWithPointer[t.X])
                    {
                        return true;
                    }
                    return p.hasPointer(f, t.X, false);
                    break;
                case ref ast.FuncType t:
                    return true;
                    break;
                case ref ast.InterfaceType t:
                    return true;
                    break;
                case ref ast.MapType t:
                    return true;
                    break;
                case ref ast.ChanType t:
                    return true;
                    break;
                case ref ast.Ident t:
                    foreach (var (_, d) in p.Decl)
                    {
                        ref ast.GenDecl (gd, ok) = d._<ref ast.GenDecl>();
                        if (!ok || gd.Tok != token.TYPE)
                        {
                            continue;
                        }
                        foreach (var (_, spec) in gd.Specs)
                        {
                            ref ast.TypeSpec (ts, ok) = spec._<ref ast.TypeSpec>();
                            if (!ok)
                            {
                                continue;
                            }
                            if (ts.Name.Name == t.Name)
                            {
                                return p.hasPointer(f, ts.Type, top);
                            }
                        }
                    }
                    {
                        var def = typedef[t.Name];

                        if (def != null)
                        {
                            return p.hasPointer(f, def.Go, top);
                        }

                    }
                    if (t.Name == "string")
                    {
                        return !top;
                    }
                    if (t.Name == "error")
                    {
                        return true;
                    }
                    if (goTypes[t.Name] != null)
                    {
                        return false;
                    } 
                    // We can't figure out the type. Conservative
                    // approach is to assume it has a pointer.
                    return true;
                    break;
                case ref ast.SelectorExpr t:
                    {
                        ref ast.Ident (l, ok) = t.X._<ref ast.Ident>();

                        if (!ok || l.Name != "C")
                        { 
                            // Type defined in a different package.
                            // Conservative approach is to assume it has a
                            // pointer.
                            return true;
                        }

                    }
                    if (f == null)
                    { 
                        // Conservative approach: assume pointer.
                        return true;
                    }
                    var name = f.Name[t.Sel.Name];
                    if (name != null && name.Kind == "type" && name.Type != null && name.Type.Go != null)
                    {
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

        // checkAddrArgs tries to add arguments to the call of
        // _cgoCheckPointer when the argument is an address expression. We
        // pass true to mean that the argument is an address operation of
        // something other than a slice index, which means that it's only
        // necessary to check the specific element pointed to, not the entire
        // object. This is for &s.f, where f is a field in a struct. We can
        // pass a slice or array, meaning that we should check the entire
        // slice or array but need not check any other part of the object.
        // This is for &s.a[i], where we need to check all of a. However, we
        // only pass the slice or array if we can refer to it without side
        // effects.
        private static slice<ast.Expr> checkAddrArgs(this ref Package p, ref File f, slice<ast.Expr> args, ast.Expr x)
        { 
            // Strip type conversions.
            while (true)
            {
                ref ast.CallExpr (c, ok) = x._<ref ast.CallExpr>();
                if (!ok || len(c.Args) != 1L || !p.isType(c.Fun))
                {
                    break;
                }
                x = c.Args[0L];
            }

            ref ast.UnaryExpr (u, ok) = x._<ref ast.UnaryExpr>();
            if (!ok || u.Op != token.AND)
            {
                return args;
            }
            ref ast.IndexExpr (index, ok) = u.X._<ref ast.IndexExpr>();
            if (!ok)
            { 
                // This is the address of something that is not an
                // index expression. We only need to examine the
                // single value to which it points.
                // TODO: what if true is shadowed?
                return append(args, ast.NewIdent("true"));
            }
            if (!p.hasSideEffects(f, index.X))
            { 
                // Examine the entire slice.
                return append(args, index.X);
            } 
            // Treat the pointer as unknown.
            return args;
        }

        // hasSideEffects returns whether the expression x has any side
        // effects.  x is an expression, not a statement, so the only side
        // effect is a function call.
        private static bool hasSideEffects(this ref Package p, ref File f, ast.Expr x)
        {
            var found = false;
            f.walk(x, ctxExpr, (f, x, context) =>
            {
                switch (x.type())
                {
                    case ref ast.CallExpr _:
                        found = true;
                        break;
                }
            });
            return found;
        }

        // isType returns whether the expression is definitely a type.
        // This is conservative--it returns false for an unknown identifier.
        private static bool isType(this ref Package p, ast.Expr t)
        {
            switch (t.type())
            {
                case ref ast.SelectorExpr t:
                    ref ast.Ident (id, ok) = t.X._<ref ast.Ident>();
                    if (!ok)
                    {
                        return false;
                    }
                    if (id.Name == "unsafe" && t.Sel.Name == "Pointer")
                    {
                        return true;
                    }
                    if (id.Name == "C" && typedef["_Ctype_" + t.Sel.Name] != null)
                    {
                        return true;
                    }
                    return false;
                    break;
                case ref ast.Ident t:
                    switch (t.Name)
                    {
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
                    break;
                case ref ast.StarExpr t:
                    return p.isType(t.X);
                    break;
                case ref ast.ArrayType t:
                    return true;
                    break;
                case ref ast.StructType t:
                    return true;
                    break;
                case ref ast.FuncType t:
                    return true;
                    break;
                case ref ast.InterfaceType t:
                    return true;
                    break;
                case ref ast.MapType t:
                    return true;
                    break;
                case ref ast.ChanType t:
                    return true;
                    break;
            }
            return false;
        }

        // rewriteUnsafe returns a version of t with references to unsafe.Pointer
        // rewritten to use _cgo_unsafe.Pointer instead.
        private static ast.Expr rewriteUnsafe(this ref Package p, ast.Expr t)
        {
            switch (t.type())
            {
                case ref ast.Ident t:
                    if (t.Name == "unsafe.Pointer")
                    {
                        return ast.NewIdent("_cgo_unsafe.Pointer");
                    }
                    break;
                case ref ast.ArrayType t:
                    var t1 = p.rewriteUnsafe(t.Elt);
                    if (t1 != t.Elt)
                    {
                        var r = t.Value;
                        r.Elt = t1;
                        return ref r;
                    }
                    break;
                case ref ast.StructType t:
                    var changed = false;
                    var fields = t.Fields.Value;
                    fields.List = null;
                    foreach (var (_, f) in t.Fields.List)
                    {
                        var ft = p.rewriteUnsafe(f.Type);
                        if (ft == f.Type)
                        {
                            fields.List = append(fields.List, f);
                        }
                        else
                        {
                            var fn = f.Value;
                            fn.Type = ft;
                            fields.List = append(fields.List, ref fn);
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        r = t.Value;
                        r.Fields = ref fields;
                        return ref r;
                    }
                    break;
                case ref ast.StarExpr t:
                    var x1 = p.rewriteUnsafe(t.X);
                    if (x1 != t.X)
                    {
                        r = t.Value;
                        r.X = x1;
                        return ref r;
                    }
                    break;
            }
            return t;
        }

        // rewriteRef rewrites all the C.xxx references in f.AST to refer to the
        // Go equivalents, now that we have figured out the meaning of all
        // the xxx. In *godefs mode, rewriteRef replaces the names
        // with full definitions instead of mangled names.
        private static void rewriteRef(this ref Package p, ref File f)
        { 
            // Keep a list of all the functions, to remove the ones
            // only used as expressions and avoid generating bridge
            // code for them.
            var functions = make_map<@string, bool>(); 

            // Assign mangled names.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in f.Name)
                {
                    n = __n;
                    if (n.Kind == "not-type")
                    {
                        if (n.Define == "")
                        {
                            n.Kind = "var";
                        }
                        else
                        {
                            n.Kind = "macro";
                            n.FuncType = ref new FuncType(Result:n.Type,Go:&ast.FuncType{Results:&ast.FieldList{List:[]*ast.Field{{Type:n.Type.Go}}},},);
                        }
                    }
                    if (n.Mangle == "")
                    {
                        p.mangleName(n);
                    }
                    if (n.Kind == "func")
                    {
                        functions[n.Go] = false;
                    }
                } 

                // Now that we have all the name types filled in,
                // scan through the Refs to identify the ones that
                // are trying to do a ,err call. Also check that
                // functions are only used in calls.

                n = n__prev1;
            }

            foreach (var (_, r) in f.Ref)
            {
                if (r.Name.IsConst() && r.Name.Const == "")
                {
                    error_(r.Pos(), "unable to find value of constant C.%s", fixGo(r.Name.Go));
                }
                ast.Expr expr = ast.NewIdent(r.Name.Mangle); // default

                if (r.Context == ctxCall || r.Context == ctxCall2) 
                    if (r.Name.Kind != "func")
                    {
                        if (r.Name.Kind == "type")
                        {
                            r.Context = ctxType;
                            if (r.Name.Type == null)
                            {
                                error_(r.Pos(), "invalid conversion to C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
                                break;
                            }
                            expr = r.Name.Type.Go;
                            break;
                        }
                        error_(r.Pos(), "call of non-function C.%s", fixGo(r.Name.Go));
                        break;
                    }
                    functions[r.Name.Go] = true;
                    if (r.Context == ctxCall2)
                    {
                        if (r.Name.Go == "_CMalloc")
                        {
                            error_(r.Pos(), "no two-result form for C.malloc");
                            break;
                        } 
                        // Invent new Name for the two-result function.
                        var n = f.Name["2" + r.Name.Go];
                        if (n == null)
                        {
                            n = @new<Name>();
                            n.Value = r.Name.Value;
                            n.AddError = true;
                            n.Mangle = "_C2func_" + n.Go;
                            f.Name["2" + r.Name.Go] = n;
                        }
                        expr = ast.NewIdent(n.Mangle);
                        r.Name = n;
                        break;
                    }
                else if (r.Context == ctxExpr) 
                    switch (r.Name.Kind)
                    {
                        case "func": 
                            if (builtinDefs[r.Name.C] != "")
                            {
                                error_(r.Pos(), "use of builtin '%s' not in function call", fixGo(r.Name.C));
                            } 

                            // Function is being used in an expression, to e.g. pass around a C function pointer.
                            // Create a new Name for this Ref which causes the variable to be declared in Go land.
                            @string fpName = "fp_" + r.Name.Go;
                            var name = f.Name[fpName];
                            if (name == null)
                            {
                                name = ref new Name(Go:fpName,C:r.Name.C,Kind:"fpvar",Type:&Type{Size:p.PtrSize,Align:p.PtrSize,C:c("void*"),Go:ast.NewIdent("unsafe.Pointer")},);
                                p.mangleName(name);
                                f.Name[fpName] = name;
                            }
                            r.Name = name; 
                            // Rewrite into call to _Cgo_ptr to prevent assignments. The _Cgo_ptr
                            // function is defined in out.go and simply returns its argument. See
                            // issue 7757.
                            expr = ref new ast.CallExpr(Fun:&ast.Ident{NamePos:(*r.Expr).Pos(),Name:"_Cgo_ptr"},Args:[]ast.Expr{ast.NewIdent(name.Mangle)},);
                            break;
                        case "type": 
                            // Okay - might be new(T)
                            if (r.Name.Type == null)
                            {
                                error_(r.Pos(), "expression C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
                                break;
                            }
                            expr = r.Name.Type.Go;
                            break;
                        case "var": 
                            expr = ref new ast.StarExpr(Star:(*r.Expr).Pos(),X:expr);
                            break;
                        case "macro": 
                            expr = ref new ast.CallExpr(Fun:expr);
                            break;
                    }
                else if (r.Context == ctxSelector) 
                    if (r.Name.Kind == "var")
                    {
                        expr = ref new ast.StarExpr(Star:(*r.Expr).Pos(),X:expr);
                    }
                    else
                    {
                        error_(r.Pos(), "only C variables allowed in selector expression %s", fixGo(r.Name.Go));
                    }
                else if (r.Context == ctxType) 
                    if (r.Name.Kind != "type")
                    {
                        error_(r.Pos(), "expression C.%s used as type", fixGo(r.Name.Go));
                    }
                    else if (r.Name.Type == null)
                    { 
                        // Use of C.enum_x, C.struct_x or C.union_x without C definition.
                        // GCC won't raise an error when using pointers to such unknown types.
                        error_(r.Pos(), "type C.%s: undefined C type '%s'", fixGo(r.Name.Go), r.Name.C);
                    }
                    else
                    {
                        expr = r.Name.Type.Go;
                    }
                else 
                    if (r.Name.Kind == "func")
                    {
                        error_(r.Pos(), "must call C.%s", fixGo(r.Name.Go));
                    }
                                if (godefs.Value)
                { 
                    // Substitute definition for mangled type name.
                    {
                        ref ast.Ident (id, ok) = expr._<ref ast.Ident>();

                        if (ok)
                        {
                            {
                                var t = typedef[id.Name];

                                if (t != null)
                                {
                                    expr = t.Go;
                                }

                            }
                            if (id.Name == r.Name.Mangle && r.Name.Const != "")
                            {
                                expr = ast.NewIdent(r.Name.Const);
                            }
                        }

                    }
                } 

                // Copy position information from old expr into new expr,
                // in case expression being replaced is first on line.
                // See golang.org/issue/6563.
                object pos = ref r.Expr();
                switch (expr.type())
                {
                    case ref ast.Ident x:
                        expr = ref new ast.Ident(NamePos:pos,Name:x.Name);
                        break; 

                    // Change AST, because some later processing depends on it,
                    // and also because -godefs mode still prints the AST.
                } 

                // Change AST, because some later processing depends on it,
                // and also because -godefs mode still prints the AST.
                var old = r.Expr.Value;
                r.Expr.Value = expr; 

                // Record source-level edit for cgo output.
                var repl = gofmt(expr);
                if (r.Name.Kind != "type")
                {
                    repl = "(" + repl + ")";
                }
                f.Edit.Replace(f.offset(old.Pos()), f.offset(old.End()), repl);
            } 

            // Remove functions only used as expressions, so their respective
            // bridge functions are not generated.
            {
                var name__prev1 = name;

                foreach (var (__name, __used) in functions)
                {
                    name = __name;
                    used = __used;
                    if (!used)
                    {
                        delete(f.Name, name);
                    }
                }

                name = name__prev1;
            }

        }

        // gccBaseCmd returns the start of the compiler command line.
        // It uses $CC if set, or else $GCC, or else the compiler recorded
        // during the initial build as defaultCC.
        // defaultCC is defined in zdefaultcc.go, written by cmd/dist.
        private static slice<@string> gccBaseCmd(this ref Package p)
        { 
            // Use $CC if set, since that's what the build uses.
            {
                var ret__prev1 = ret;

                var ret = strings.Fields(os.Getenv("CC"));

                if (len(ret) > 0L)
                {
                    return ret;
                } 
                // Try $GCC if set, since that's what we used to use.

                ret = ret__prev1;

            } 
            // Try $GCC if set, since that's what we used to use.
            {
                var ret__prev1 = ret;

                ret = strings.Fields(os.Getenv("GCC"));

                if (len(ret) > 0L)
                {
                    return ret;
                }

                ret = ret__prev1;

            }
            return strings.Fields(defaultCC(goos, goarch));
        }

        // gccMachine returns the gcc -m flag to use, either "-m32", "-m64" or "-marm".
        private static slice<@string> gccMachine(this ref Package p)
        {
            switch (goarch)
            {
                case "amd64": 
                    return new slice<@string>(new @string[] { "-m64" });
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
                    return new slice<@string>(new @string[] { "-mabi=64" });
                    break;
                case "mips": 

                case "mipsle": 
                    return new slice<@string>(new @string[] { "-mabi=32" });
                    break;
            }
            return null;
        }

        private static @string gccTmp()
        {
            return objDir + "_cgo_.o".Value;
        }

        // gccCmd returns the gcc command line to use for compiling
        // the input.
        private static slice<@string> gccCmd(this ref Package p)
        {
            var c = append(p.gccBaseCmd(), "-w", "-Wno-error", "-o" + gccTmp(), "-gdwarf-2", "-c", "-xc");
            if (p.GccIsClang)
            {
                c = append(c, "-ferror-limit=0", "-Wno-unknown-warning-option", "-Wno-unneeded-internal-declaration", "-Wno-unused-function", "-Qunused-arguments", "-fno-builtin");
            }
            c = append(c, p.GccOptions);
            c = append(c, p.gccMachine());
            c = append(c, "-"); //read input from standard input
            return c;
        }

        // gccDebug runs gcc -gdwarf-2 over the C program stdin and
        // returns the corresponding DWARF data and, if present, debug data block.
        private static (ref dwarf.Data, slice<long>, slice<double>, slice<@string>) gccDebug(this ref Package _p, slice<byte> stdin, long nnames) => func(_p, (ref Package p, Defer defer, Panic panic, Recover _) =>
        {
            runGcc(stdin, p.gccCmd());

            Func<@string, bool> isDebugInts = s =>
            { 
                // Some systems use leading _ to denote non-assembly symbols.
                return s == "__cgodebug_ints" || s == "___cgodebug_ints";
            }
;
            Func<@string, bool> isDebugFloats = s =>
            { 
                // Some systems use leading _ to denote non-assembly symbols.
                return s == "__cgodebug_floats" || s == "___cgodebug_floats";
            }
;
            Func<@string, long> indexOfDebugStr = s =>
            { 
                // Some systems use leading _ to denote non-assembly symbols.
                if (strings.HasPrefix(s, "___"))
                {
                    s = s[1L..];
                }
                if (strings.HasPrefix(s, "__cgodebug_str__"))
                {
                    {
                        var n__prev2 = n;

                        var (n, err) = strconv.Atoi(s[len("__cgodebug_str__")..]);

                        if (err == null)
                        {
                            return n;
                        }

                        n = n__prev2;

                    }
                }
                return -1L;
            }
;
            Func<@string, long> indexOfDebugStrlen = s =>
            { 
                // Some systems use leading _ to denote non-assembly symbols.
                if (strings.HasPrefix(s, "___"))
                {
                    s = s[1L..];
                }
                if (strings.HasPrefix(s, "__cgodebug_strlen__"))
                {
                    {
                        var n__prev2 = n;

                        (n, err) = strconv.Atoi(s[len("__cgodebug_strlen__")..]);

                        if (err == null)
                        {
                            return n;
                        }

                        n = n__prev2;

                    }
                }
                return -1L;
            }
;

            strs = make_slice<@string>(nnames);

            var strdata = make_map<long, @string>(nnames);
            var strlens = make_map<long, long>(nnames);

            Action buildStrings = () =>
            {
                {
                    var n__prev1 = n;
                    var strlen__prev1 = strlen;

                    foreach (var (__n, __strlen) in strlens)
                    {
                        n = __n;
                        strlen = __strlen;
                        var data = strdata[n];
                        if (len(data) <= strlen)
                        {
                            fatalf("invalid string literal");
                        }
                        strs[n] = string(data[..strlen]);
                    }

                    n = n__prev1;
                    strlen = strlen__prev1;
                }

            }
;

            {
                var f__prev1 = f;

                var (f, err) = macho.Open(gccTmp());

                if (err == null)
                {
                    defer(f.Close());
                    var (d, err) = f.DWARF();
                    if (err != null)
                    {
                        fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
                    }
                    var bo = f.ByteOrder;
                    if (f.Symtab != null)
                    {
                        {
                            var i__prev1 = i;

                            foreach (var (__i) in f.Symtab.Syms)
                            {
                                i = __i;
                                var s = ref f.Symtab.Syms[i];

                                if (isDebugInts(s.Name)) 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev3 = i;

                                        var i = int(s.Sect) - 1L;

                                        if (0L <= i && i < len(f.Sections))
                                        {
                                            var sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                            {
                                                {
                                                    var sdat__prev5 = sdat;

                                                    var (sdat, err) = sect.Data();

                                                    if (err == null)
                                                    {
                                                        data = sdat[s.Value - sect.Addr..];
                                                        ints = make_slice<long>(len(data) / 8L);
                                                        {
                                                            var i__prev2 = i;

                                                            foreach (var (__i) in ints)
                                                            {
                                                                i = __i;
                                                                ints[i] = int64(bo.Uint64(data[i * 8L..]));
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

                                        i = int(s.Sect) - 1L;

                                        if (0L <= i && i < len(f.Sections))
                                        {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                            {
                                                {
                                                    var sdat__prev5 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null)
                                                    {
                                                        data = sdat[s.Value - sect.Addr..];
                                                        floats = make_slice<double>(len(data) / 8L);
                                                        {
                                                            var i__prev2 = i;

                                                            foreach (var (__i) in floats)
                                                            {
                                                                i = __i;
                                                                floats[i] = math.Float64frombits(bo.Uint64(data[i * 8L..]));
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

                                        if (n != -1L)
                                        { 
                                            // Found it. Now find data section.
                                            {
                                                var i__prev4 = i;

                                                i = int(s.Sect) - 1L;

                                                if (0L <= i && i < len(f.Sections))
                                                {
                                                    sect = f.Sections[i];
                                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                                    {
                                                        {
                                                            var sdat__prev6 = sdat;

                                                            (sdat, err) = sect.Data();

                                                            if (err == null)
                                                            {
                                                                data = sdat[s.Value - sect.Addr..];
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

                                        if (n != -1L)
                                        { 
                                            // Found it. Now find data section.
                                            {
                                                var i__prev4 = i;

                                                i = int(s.Sect) - 1L;

                                                if (0L <= i && i < len(f.Sections))
                                                {
                                                    sect = f.Sections[i];
                                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                                    {
                                                        {
                                                            var sdat__prev6 = sdat;

                                                            (sdat, err) = sect.Data();

                                                            if (err == null)
                                                            {
                                                                data = sdat[s.Value - sect.Addr..];
                                                                var strlen = bo.Uint64(data[..8L]);
                                                                if (strlen > (1L << (int)((uint(p.IntSize * 8L) - 1L)) - 1L))
                                                                { // greater than MaxInt?
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
                    return (d, ints, floats, strs);
                }

                f = f__prev1;

            }

            {
                var f__prev1 = f;

                (f, err) = elf.Open(gccTmp());

                if (err == null)
                {
                    defer(f.Close());
                    (d, err) = f.DWARF();
                    if (err != null)
                    {
                        fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
                    }
                    bo = f.ByteOrder;
                    var (symtab, err) = f.Symbols();
                    if (err == null)
                    {
                        {
                            var i__prev1 = i;

                            foreach (var (__i) in symtab)
                            {
                                i = __i;
                                s = ref symtab[i];

                                if (isDebugInts(s.Name)) 
                                    // Found it. Now find data section.
                                    {
                                        var i__prev3 = i;

                                        i = int(s.Section);

                                        if (0L <= i && i < len(f.Sections))
                                        {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                            {
                                                {
                                                    var sdat__prev5 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null)
                                                    {
                                                        data = sdat[s.Value - sect.Addr..];
                                                        ints = make_slice<long>(len(data) / 8L);
                                                        {
                                                            var i__prev2 = i;

                                                            foreach (var (__i) in ints)
                                                            {
                                                                i = __i;
                                                                ints[i] = int64(bo.Uint64(data[i * 8L..]));
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

                                        if (0L <= i && i < len(f.Sections))
                                        {
                                            sect = f.Sections[i];
                                            if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                            {
                                                {
                                                    var sdat__prev5 = sdat;

                                                    (sdat, err) = sect.Data();

                                                    if (err == null)
                                                    {
                                                        data = sdat[s.Value - sect.Addr..];
                                                        floats = make_slice<double>(len(data) / 8L);
                                                        {
                                                            var i__prev2 = i;

                                                            foreach (var (__i) in floats)
                                                            {
                                                                i = __i;
                                                                floats[i] = math.Float64frombits(bo.Uint64(data[i * 8L..]));
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

                                        if (n != -1L)
                                        { 
                                            // Found it. Now find data section.
                                            {
                                                var i__prev4 = i;

                                                i = int(s.Section);

                                                if (0L <= i && i < len(f.Sections))
                                                {
                                                    sect = f.Sections[i];
                                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                                    {
                                                        {
                                                            var sdat__prev6 = sdat;

                                                            (sdat, err) = sect.Data();

                                                            if (err == null)
                                                            {
                                                                data = sdat[s.Value - sect.Addr..];
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

                                        if (n != -1L)
                                        { 
                                            // Found it. Now find data section.
                                            {
                                                var i__prev4 = i;

                                                i = int(s.Section);

                                                if (0L <= i && i < len(f.Sections))
                                                {
                                                    sect = f.Sections[i];
                                                    if (sect.Addr <= s.Value && s.Value < sect.Addr + sect.Size)
                                                    {
                                                        {
                                                            var sdat__prev6 = sdat;

                                                            (sdat, err) = sect.Data();

                                                            if (err == null)
                                                            {
                                                                data = sdat[s.Value - sect.Addr..];
                                                                strlen = bo.Uint64(data[..8L]);
                                                                if (strlen > (1L << (int)((uint(p.IntSize * 8L) - 1L)) - 1L))
                                                                { // greater than MaxInt?
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
                    return (d, ints, floats, strs);
                }

                f = f__prev1;

            }

            {
                var f__prev1 = f;

                (f, err) = pe.Open(gccTmp());

                if (err == null)
                {
                    defer(f.Close());
                    (d, err) = f.DWARF();
                    if (err != null)
                    {
                        fatalf("cannot load DWARF output from %s: %v", gccTmp(), err);
                    }
                    bo = binary.LittleEndian;
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in f.Symbols)
                        {
                            s = __s;

                            if (isDebugInts(s.Name)) 
                                {
                                    var i__prev2 = i;

                                    i = int(s.SectionNumber) - 1L;

                                    if (0L <= i && i < len(f.Sections))
                                    {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size)
                                        {
                                            {
                                                var sdat__prev4 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null)
                                                {
                                                    data = sdat[s.Value..];
                                                    ints = make_slice<long>(len(data) / 8L);
                                                    {
                                                        var i__prev2 = i;

                                                        foreach (var (__i) in ints)
                                                        {
                                                            i = __i;
                                                            ints[i] = int64(bo.Uint64(data[i * 8L..]));
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

                                    i = int(s.SectionNumber) - 1L;

                                    if (0L <= i && i < len(f.Sections))
                                    {
                                        sect = f.Sections[i];
                                        if (s.Value < sect.Size)
                                        {
                                            {
                                                var sdat__prev4 = sdat;

                                                (sdat, err) = sect.Data();

                                                if (err == null)
                                                {
                                                    data = sdat[s.Value..];
                                                    floats = make_slice<double>(len(data) / 8L);
                                                    {
                                                        var i__prev2 = i;

                                                        foreach (var (__i) in floats)
                                                        {
                                                            i = __i;
                                                            floats[i] = math.Float64frombits(bo.Uint64(data[i * 8L..]));
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

                                    if (n != -1L)
                                    {
                                        {
                                            var i__prev3 = i;

                                            i = int(s.SectionNumber) - 1L;

                                            if (0L <= i && i < len(f.Sections))
                                            {
                                                sect = f.Sections[i];
                                                if (s.Value < sect.Size)
                                                {
                                                    {
                                                        var sdat__prev5 = sdat;

                                                        (sdat, err) = sect.Data();

                                                        if (err == null)
                                                        {
                                                            data = sdat[s.Value..];
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

                                    if (n != -1L)
                                    {
                                        {
                                            var i__prev3 = i;

                                            i = int(s.SectionNumber) - 1L;

                                            if (0L <= i && i < len(f.Sections))
                                            {
                                                sect = f.Sections[i];
                                                if (s.Value < sect.Size)
                                                {
                                                    {
                                                        var sdat__prev5 = sdat;

                                                        (sdat, err) = sect.Data();

                                                        if (err == null)
                                                        {
                                                            data = sdat[s.Value..];
                                                            strlen = bo.Uint64(data[..8L]);
                                                            if (strlen > (1L << (int)((uint(p.IntSize * 8L) - 1L)) - 1L))
                                                            { // greater than MaxInt?
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

                    return (d, ints, floats, strs);
                }

                f = f__prev1;

            }

            fatalf("cannot parse gcc output %s as ELF, Mach-O, PE object", gccTmp());
            panic("not reached");
        });

        // gccDefines runs gcc -E -dM -xc - over the C program stdin
        // and returns the corresponding standard output, which is the
        // #defines that gcc encountered while processing the input
        // and its included files.
        private static @string gccDefines(this ref Package p, slice<byte> stdin)
        {
            var @base = append(p.gccBaseCmd(), "-E", "-dM", "-xc");
            base = append(base, p.gccMachine());
            var (stdout, _) = runGcc(stdin, append(append(base, p.GccOptions), "-"));
            return stdout;
        }

        // gccErrors runs gcc over the C program stdin and returns
        // the errors that gcc prints. That is, this function expects
        // gcc to fail.
        private static @string gccErrors(this ref Package p, slice<byte> stdin)
        { 
            // TODO(rsc): require failure
            var args = p.gccCmd(); 

            // Optimization options can confuse the error messages; remove them.
            var nargs = make_slice<@string>(0L, len(args));
            foreach (var (_, arg) in args)
            {
                if (!strings.HasPrefix(arg, "-O"))
                {
                    nargs = append(nargs, arg);
                }
            }
            if (debugGcc.Value)
            {
                fmt.Fprintf(os.Stderr, "$ %s <<EOF\n", strings.Join(nargs, " "));
                os.Stderr.Write(stdin);
                fmt.Fprint(os.Stderr, "EOF\n");
            }
            var (stdout, stderr, _) = run(stdin, nargs);
            if (debugGcc.Value)
            {
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
        private static (@string, @string) runGcc(slice<byte> stdin, slice<@string> args)
        {
            if (debugGcc.Value)
            {
                fmt.Fprintf(os.Stderr, "$ %s <<EOF\n", strings.Join(args, " "));
                os.Stderr.Write(stdin);
                fmt.Fprint(os.Stderr, "EOF\n");
            }
            var (stdout, stderr, ok) = run(stdin, args);
            if (debugGcc.Value)
            {
                os.Stderr.Write(stdout);
                os.Stderr.Write(stderr);
            }
            if (!ok)
            {
                os.Stderr.Write(stderr);
                os.Exit(2L);
            }
            return (string(stdout), string(stderr));
        }

        // A typeConv is a translator from dwarf types to Go types
        // with equivalent memory layout.
        private partial struct typeConv
        {
            public map<dwarf.Type, ref Type> m; // Map from types to incomplete pointers to those types.
            public map<dwarf.Type, slice<ref Type>> ptrs; // Keys of ptrs in insertion order (deterministic worklist)
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

        private static long tagGen = default;
        private static var typedef = make_map<@string, ref Type>();
        private static var goIdent = make_map<@string, ref ast.Ident>();

        // unionWithPointer is true for a Go type that represents a C union (or class)
        // that may contain a pointer. This is used for cgo pointer checking.
        private static var unionWithPointer = make_map<ast.Expr, bool>();

        private static void Init(this ref typeConv c, long ptrSize, long intSize)
        {
            c.ptrSize = ptrSize;
            c.intSize = intSize;
            c.m = make_map<dwarf.Type, ref Type>();
            c.ptrs = make_map<dwarf.Type, slice<ref Type>>();
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
            if (godefs.Value)
            {
                c.goVoidPtr = ref new ast.StarExpr(X:c.byte);
            }
            else
            {
                c.goVoidPtr = c.Ident("unsafe.Pointer");
            }
        }

        // base strips away qualifiers and typedefs to get the underlying type
        private static dwarf.Type @base(dwarf.Type dt)
        {
            while (true)
            {
                {
                    ref dwarf.QualType d__prev1 = d;

                    ref dwarf.QualType (d, ok) = dt._<ref dwarf.QualType>();

                    if (ok)
                    {
                        dt = d.Type;
                        continue;
                    }

                    d = d__prev1;

                }
                {
                    ref dwarf.QualType d__prev1 = d;

                    (d, ok) = dt._<ref dwarf.TypedefType>();

                    if (ok)
                    {
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
        private static dwarf.Type unqual(dwarf.Type dt)
        {
            while (true)
            {
                {
                    ref dwarf.QualType (d, ok) = dt._<ref dwarf.QualType>();

                    if (ok)
                    {
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

        private static readonly long signedDelta = 64L;

        // String returns the current type representation. Format arguments
        // are assembled within this method so that any changes in mutable
        // values are taken into account.


        // String returns the current type representation. Format arguments
        // are assembled within this method so that any changes in mutable
        // values are taken into account.
        private static @string String(this ref TypeRepr tr)
        {
            if (len(tr.Repr) == 0L)
            {
                return "";
            }
            if (len(tr.FormatArgs) == 0L)
            {
                return tr.Repr;
            }
            return fmt.Sprintf(tr.Repr, tr.FormatArgs);
        }

        // Empty reports whether the result of String would be "".
        private static bool Empty(this ref TypeRepr tr)
        {
            return len(tr.Repr) == 0L;
        }

        // Set modifies the type representation.
        // If fargs are provided, repr is used as a format for fmt.Sprintf.
        // Otherwise, repr is used unprocessed as the type representation.
        private static void Set(this ref TypeRepr tr, @string repr, params object[] fargs)
        {
            tr.Repr = repr;
            tr.FormatArgs = fargs;
        }

        // FinishType completes any outstanding type mapping work.
        // In particular, it resolves incomplete pointer types.
        private static void FinishType(this ref typeConv c, token.Pos pos)
        { 
            // Completing one pointer type might produce more to complete.
            // Keep looping until they're all done.
            while (len(c.ptrKeys) > 0L)
            {
                var dtype = c.ptrKeys[0L];
                c.ptrKeys = c.ptrKeys[1L..]; 

                // Note Type might invalidate c.ptrs[dtype].
                var t = c.Type(dtype, pos);
                foreach (var (_, ptr) in c.ptrs[dtype])
                {
                    ptr.Go._<ref ast.StarExpr>().X = t.Go;
                    ptr.C.Set("%s*", t.C);
                }
                c.ptrs[dtype] = null; // retain the map key
            }

        }

        // Type returns a *Type with the same memory layout as
        // dtype when used as the type of a variable or a struct field.
        private static ref Type Type(this ref typeConv c, dwarf.Type dtype, token.Pos pos)
        {
            {
                var t__prev1 = t;

                var (t, ok) = c.m[dtype];

                if (ok)
                {
                    if (t.Go == null)
                    {
                        fatalf("%s: type conversion loop at %s", lineno(pos), dtype);
                    }
                    return t;
                }

                t = t__prev1;

            }

            ptr<Type> t = @new<Type>();
            t.Size = dtype.Size(); // note: wrong for array of pointers, corrected below
            t.Align = -1L;
            t.C = ref new TypeRepr(Repr:dtype.Common().Name);
            c.m[dtype] = t;

            switch (dtype.type())
            {
                case ref dwarf.AddrType dt:
                    if (t.Size != c.ptrSize)
                    {
                        fatalf("%s: unexpected: %d-byte address type - %s", lineno(pos), t.Size, dtype);
                    }
                    t.Go = c.uintptr;
                    t.Align = t.Size;
                    break;
                case ref dwarf.ArrayType dt:
                    if (dt.StrideBitSize > 0L)
                    { 
                        // Cannot represent bit-sized elements in Go.
                        t.Go = c.Opaque(t.Size);
                        break;
                    }
                    var count = dt.Count;
                    if (count == -1L)
                    { 
                        // Indicates flexible array member, which Go doesn't support.
                        // Translate to zero-length array instead.
                        count = 0L;
                    }
                    var sub = c.Type(dt.Type, pos);
                    t.Align = sub.Align;
                    t.Go = ref new ast.ArrayType(Len:c.intExpr(count),Elt:sub.Go,); 
                    // Recalculate t.Size now that we know sub.Size.
                    t.Size = count * sub.Size;
                    t.C.Set("__typeof__(%s[%d])", sub.C, dt.Count);
                    break;
                case ref dwarf.BoolType dt:
                    t.Go = c.@bool;
                    t.Align = 1L;
                    break;
                case ref dwarf.CharType dt:
                    if (t.Size != 1L)
                    {
                        fatalf("%s: unexpected: %d-byte char type - %s", lineno(pos), t.Size, dtype);
                    }
                    t.Go = c.int8;
                    t.Align = 1L;
                    break;
                case ref dwarf.EnumType dt:
                    t.Align = t.Size;

                    if (t.Align >= c.ptrSize)
                    {
                        t.Align = c.ptrSize;
                    }
                    t.C.Set("enum " + dt.EnumName);
                    long signed = 0L;
                    t.EnumValues = make_map<@string, long>();
                    foreach (var (_, ev) in dt.Val)
                    {
                        t.EnumValues[ev.Name] = ev.Val;
                        if (ev.Val < 0L)
                        {
                            signed = signedDelta;
                        }
                    }
                    switch (t.Size + int64(signed))
                    {
                        case 1L: 
                            t.Go = c.uint8;
                            break;
                        case 2L: 
                            t.Go = c.uint16;
                            break;
                        case 4L: 
                            t.Go = c.uint32;
                            break;
                        case 8L: 
                            t.Go = c.uint64;
                            break;
                        case 1L + signedDelta: 
                            t.Go = c.int8;
                            break;
                        case 2L + signedDelta: 
                            t.Go = c.int16;
                            break;
                        case 4L + signedDelta: 
                            t.Go = c.int32;
                            break;
                        case 8L + signedDelta: 
                            t.Go = c.int64;
                            break;
                        default: 
                            fatalf("%s: unexpected: %d-byte enum type - %s", lineno(pos), t.Size, dtype);
                            break;
                    }
                    break;
                case ref dwarf.FloatType dt:
                    switch (t.Size)
                    {
                        case 4L: 
                            t.Go = c.float32;
                            break;
                        case 8L: 
                            t.Go = c.float64;
                            break;
                        default: 
                            fatalf("%s: unexpected: %d-byte float type - %s", lineno(pos), t.Size, dtype);
                            break;
                    }
                    t.Align = t.Size;

                    if (t.Align >= c.ptrSize)
                    {
                        t.Align = c.ptrSize;
                    }
                    break;
                case ref dwarf.ComplexType dt:
                    switch (t.Size)
                    {
                        case 8L: 
                            t.Go = c.complex64;
                            break;
                        case 16L: 
                            t.Go = c.complex128;
                            break;
                        default: 
                            fatalf("%s: unexpected: %d-byte complex type - %s", lineno(pos), t.Size, dtype);
                            break;
                    }
                    t.Align = t.Size / 2L;

                    if (t.Align >= c.ptrSize)
                    {
                        t.Align = c.ptrSize;
                    }
                    break;
                case ref dwarf.FuncType dt:
                    t.Go = c.uintptr;
                    t.Align = c.ptrSize;
                    break;
                case ref dwarf.IntType dt:
                    if (dt.BitSize > 0L)
                    {
                        fatalf("%s: unexpected: %d-bit int type - %s", lineno(pos), dt.BitSize, dtype);
                    }
                    switch (t.Size)
                    {
                        case 1L: 
                            t.Go = c.int8;
                            break;
                        case 2L: 
                            t.Go = c.int16;
                            break;
                        case 4L: 
                            t.Go = c.int32;
                            break;
                        case 8L: 
                            t.Go = c.int64;
                            break;
                        case 16L: 
                            t.Go = ref new ast.ArrayType(Len:c.intExpr(t.Size),Elt:c.uint8,);
                            break;
                        default: 
                            fatalf("%s: unexpected: %d-byte int type - %s", lineno(pos), t.Size, dtype);
                            break;
                    }
                    t.Align = t.Size;

                    if (t.Align >= c.ptrSize)
                    {
                        t.Align = c.ptrSize;
                    }
                    break;
                case ref dwarf.PtrType dt:
                    if (t.Size != c.ptrSize && t.Size != -1L)
                    {
                        fatalf("%s: unexpected: %d-byte pointer type - %s", lineno(pos), t.Size, dtype);
                    }
                    t.Size = c.ptrSize;
                    t.Align = c.ptrSize;

                    {
                        ref dwarf.VoidType (_, ok) = base(dt.Type)._<ref dwarf.VoidType>();

                        if (ok)
                        {
                            t.Go = c.goVoidPtr;
                            t.C.Set("void*");
                            var dq = dt.Type;
                            while (true)
                            {
                                {
                                    ref dwarf.QualType (d, ok) = dq._<ref dwarf.QualType>();

                                    if (ok)
                                    {
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
                    t.Go = ref new ast.StarExpr();
                    t.C.Set("<incomplete>*");
                    {
                        (_, ok) = c.ptrs[dt.Type];

                        if (!ok)
                        {
                            c.ptrKeys = append(c.ptrKeys, dt.Type);
                        }

                    }
                    c.ptrs[dt.Type] = append(c.ptrs[dt.Type], t);
                    break;
                case ref dwarf.QualType dt:
                    var t1 = c.Type(dt.Type, pos);
                    t.Size = t1.Size;
                    t.Align = t1.Align;
                    t.Go = t1.Go;
                    if (unionWithPointer[t1.Go])
                    {
                        unionWithPointer[t.Go] = true;
                    }
                    t.EnumValues = null;
                    t.Typedef = "";
                    t.C.Set("%s " + dt.Qual, t1.C);
                    return t;
                    break;
                case ref dwarf.StructType dt:
                    var tag = dt.StructName;
                    if (dt.ByteSize < 0L && tag == "")
                    { // opaque unnamed struct - should not be possible
                        break;
                    }
                    if (tag == "")
                    {
                        tag = "__" + strconv.Itoa(tagGen);
                        tagGen++;
                    }
                    else if (t.C.Empty())
                    {
                        t.C.Set(dt.Kind + " " + tag);
                    }
                    var name = c.Ident("_Ctype_" + dt.Kind + "_" + tag);
                    t.Go = name; // publish before recursive calls
                    goIdent[name.Name] = name;
                    if (dt.ByteSize < 0L)
                    { 
                        // Size calculation in c.Struct/c.Opaque will die with size=-1 (unknown),
                        // so execute the basic things that the struct case would do
                        // other than try to determine a Go representation.
                        var tt = t.Value;
                        tt.C = ref new TypeRepr("%s %s",[]interface{}{dt.Kind,tag});
                        tt.Go = c.Ident("struct{}");
                        typedef[name.Name] = ref tt;
                        break;
                    }
                    switch (dt.Kind)
                    {
                        case "class": 

                        case "union": 
                            t.Go = c.Opaque(t.Size);
                            if (c.dwarfHasPointer(dt, pos))
                            {
                                unionWithPointer[t.Go] = true;
                            }
                            if (t.C.Empty())
                            {
                                t.C.Set("__typeof__(unsigned char[%d])", t.Size);
                            }
                            t.Align = 1L; // TODO: should probably base this on field alignment.
                            typedef[name.Name] = t;
                            break;
                        case "struct": 
                            var (g, csyntax, align) = c.Struct(dt, pos);
                            if (t.C.Empty())
                            {
                                t.C.Set(csyntax);
                            }
                            t.Align = align;
                            tt = t.Value;
                            if (tag != "")
                            {
                                tt.C = ref new TypeRepr("struct %s",[]interface{}{tag});
                            }
                            tt.Go = g;
                            typedef[name.Name] = ref tt;
                            break;
                    }
                    break;
                case ref dwarf.TypedefType dt:
                    if (dt.Name == "_GoString_")
                    { 
                        // Special C name for Go string type.
                        // Knows string layout used by compilers: pointer plus length,
                        // which rounds up to 2 pointers after alignment.
                        t.Go = c.@string;
                        t.Size = c.ptrSize * 2L;
                        t.Align = c.ptrSize;
                        break;
                    }
                    if (dt.Name == "_GoBytes_")
                    { 
                        // Special C name for Go []byte type.
                        // Knows slice layout used by compilers: pointer, length, cap.
                        t.Go = c.Ident("[]byte");
                        t.Size = c.ptrSize + 4L + 4L;
                        t.Align = c.ptrSize;
                        break;
                    }
                    name = c.Ident("_Ctype_" + dt.Name);
                    goIdent[name.Name] = name;
                    sub = c.Type(dt.Type, pos);
                    if (c.badPointerTypedef(dt))
                    { 
                        // Treat this typedef as a uintptr.
                        var s = sub.Value;
                        s.Go = c.uintptr;
                        sub = ref s;
                    }
                    t.Go = name;
                    if (unionWithPointer[sub.Go])
                    {
                        unionWithPointer[t.Go] = true;
                    }
                    t.Size = sub.Size;
                    t.Align = sub.Align;
                    var oldType = typedef[name.Name];
                    if (oldType == null)
                    {
                        tt = t.Value;
                        tt.Go = sub.Go;
                        typedef[name.Name] = ref tt;
                    } 

                    // If sub.Go.Name is "_Ctype_struct_foo" or "_Ctype_union_foo" or "_Ctype_class_foo",
                    // use that as the Go form for this typedef too, so that the typedef will be interchangeable
                    // with the base type.
                    // In -godefs mode, do this for all typedefs.
                    if (isStructUnionClass(sub.Go) || godefs.Value)
                    {
                        t.Go = sub.Go;

                        if (isStructUnionClass(sub.Go))
                        { 
                            // Use the typedef name for C code.
                            typedef[sub.Go._<ref ast.Ident>().Name].C = t.C;
                        } 

                        // If we've seen this typedef before, and it
                        // was an anonymous struct/union/class before
                        // too, use the old definition.
                        // TODO: it would be safer to only do this if
                        // we verify that the types are the same.
                        if (oldType != null && isStructUnionClass(oldType.Go))
                        {
                            t.Go = oldType.Go;
                        }
                    }
                    break;
                case ref dwarf.UcharType dt:
                    if (t.Size != 1L)
                    {
                        fatalf("%s: unexpected: %d-byte uchar type - %s", lineno(pos), t.Size, dtype);
                    }
                    t.Go = c.uint8;
                    t.Align = 1L;
                    break;
                case ref dwarf.UintType dt:
                    if (dt.BitSize > 0L)
                    {
                        fatalf("%s: unexpected: %d-bit uint type - %s", lineno(pos), dt.BitSize, dtype);
                    }
                    switch (t.Size)
                    {
                        case 1L: 
                            t.Go = c.uint8;
                            break;
                        case 2L: 
                            t.Go = c.uint16;
                            break;
                        case 4L: 
                            t.Go = c.uint32;
                            break;
                        case 8L: 
                            t.Go = c.uint64;
                            break;
                        case 16L: 
                            t.Go = ref new ast.ArrayType(Len:c.intExpr(t.Size),Elt:c.uint8,);
                            break;
                        default: 
                            fatalf("%s: unexpected: %d-byte uint type - %s", lineno(pos), t.Size, dtype);
                            break;
                    }
                    t.Align = t.Size;

                    if (t.Align >= c.ptrSize)
                    {
                        t.Align = c.ptrSize;
                    }
                    break;
                case ref dwarf.VoidType dt:
                    t.Go = c.goVoid;
                    t.C.Set("void");
                    t.Align = 1L;
                    break;
                default:
                {
                    var dt = dtype.type();
                    fatalf("%s: unexpected type: %s", lineno(pos), dtype);
                    break;
                }

            }

            switch (dtype.type())
            {
                case ref dwarf.AddrType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.BoolType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.CharType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.ComplexType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.IntType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.FloatType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.UcharType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;
                case ref dwarf.UintType _:
                    s = dtype.Common().Name;
                    if (s != "")
                    {
                        {
                            var (ss, ok) = dwarfToName[s];

                            if (ok)
                            {
                                s = ss;
                            }

                        }
                        s = strings.Replace(s, " ", "", -1L);
                        name = c.Ident("_Ctype_" + s);
                        tt = t.Value;
                        typedef[name.Name] = ref tt;
                        if (!godefs.Value)
                        {
                            t.Go = name;
                        }
                    }
                    break;

            }

            if (t.Size < 0L)
            { 
                // Unsized types are [0]byte, unless they're typedefs of other types
                // or structs with tags.
                // if so, use the name we've already defined.
                t.Size = 0L;
                switch (dtype.type())
                {
                    case ref dwarf.TypedefType dt:
                        break;
                    case ref dwarf.StructType dt:
                        if (dt.StructName != "")
                        {
                            break;
                        }
                        t.Go = c.Opaque(0L);
                        break;
                    default:
                    {
                        var dt = dtype.type();
                        t.Go = c.Opaque(0L);
                        break;
                    }
                }
                if (t.C.Empty())
                {
                    t.C.Set("void");
                }
            }
            if (t.C.Empty())
            {
                fatalf("%s: internal error: did not create C name for %s", lineno(pos), dtype);
            }
            return t;
        }

        // isStructUnionClass reports whether the type described by the Go syntax x
        // is a struct, union, or class with a tag.
        private static bool isStructUnionClass(ast.Expr x)
        {
            ref ast.Ident (id, ok) = x._<ref ast.Ident>();
            if (!ok)
            {
                return false;
            }
            var name = id.Name;
            return strings.HasPrefix(name, "_Ctype_struct_") || strings.HasPrefix(name, "_Ctype_union_") || strings.HasPrefix(name, "_Ctype_class_");
        }

        // FuncArg returns a Go type with the same memory layout as
        // dtype when used as the type of a C function argument.
        private static ref Type FuncArg(this ref typeConv c, dwarf.Type dtype, token.Pos pos)
        {
            var t = c.Type(unqual(dtype), pos);
            switch (dtype.type())
            {
                case ref dwarf.ArrayType dt:
                    TypeRepr tr = ref new TypeRepr();
                    tr.Set("%s*", t.C);
                    return ref new Type(Size:c.ptrSize,Align:c.ptrSize,Go:&ast.StarExpr{X:t.Go},C:tr,);
                    break;
                case ref dwarf.TypedefType dt:
                    {
                        ref dwarf.PtrType (ptr, ok) = base(dt.Type)._<ref dwarf.PtrType>();

                        if (ok)
                        { 
                            // Unless the typedef happens to point to void* since
                            // Go has special rules around using unsafe.Pointer.
                            {
                                ref dwarf.VoidType (_, void) = base(ptr.Type)._<ref dwarf.VoidType>();

                                if (void)
                                {
                                    break;
                                } 
                                // ...or the typedef is one in which we expect bad pointers.
                                // It will be a uintptr instead of *X.

                            } 
                            // ...or the typedef is one in which we expect bad pointers.
                            // It will be a uintptr instead of *X.
                            if (c.badPointerTypedef(dt))
                            {
                                break;
                            }
                            t = c.Type(ptr, pos);
                            if (t == null)
                            {
                                return null;
                            } 

                            // For a struct/union/class, remember the C spelling,
                            // in case it has __attribute__((unavailable)).
                            // See issue 2888.
                            if (isStructUnionClass(t.Go))
                            {
                                t.Typedef = dt.Name;
                            }
                        }

                    }
                    break;
            }
            return t;
        }

        // FuncType returns the Go type analogous to dtype.
        // There is no guarantee about matching memory layout.
        private static ref FuncType FuncType(this ref typeConv c, ref dwarf.FuncType dtype, token.Pos pos)
        {
            var p = make_slice<ref Type>(len(dtype.ParamType));
            var gp = make_slice<ref ast.Field>(len(dtype.ParamType));
            foreach (var (i, f) in dtype.ParamType)
            { 
                // gcc's DWARF generator outputs a single DotDotDotType parameter for
                // function pointers that specify no parameters (e.g. void
                // (*__cgo_0)()).  Treat this special case as void. This case is
                // invalid according to ISO C anyway (i.e. void (*__cgo_1)(...) is not
                // legal).
                {
                    ref dwarf.DotDotDotType (_, ok) = f._<ref dwarf.DotDotDotType>();

                    if (ok && i == 0L)
                    {
                        p = null;
                        gp = null;
                        break;
                    }

                }
                p[i] = c.FuncArg(f, pos);
                gp[i] = ref new ast.Field(Type:p[i].Go);
            }
            ref Type r = default;
            slice<ref ast.Field> gr = default;
            {
                (_, ok) = base(dtype.ReturnType)._<ref dwarf.VoidType>();

                if (ok)
                {
                    gr = new slice<ref ast.Field>(new ref ast.Field[] { {Type:c.goVoid} });
                }
                else if (dtype.ReturnType != null)
                {
                    r = c.Type(unqual(dtype.ReturnType), pos);
                    gr = new slice<ref ast.Field>(new ref ast.Field[] { {Type:r.Go} });
                }

            }
            return ref new FuncType(Params:p,Result:r,Go:&ast.FuncType{Params:&ast.FieldList{List:gp},Results:&ast.FieldList{List:gr},},);
        }

        // Identifier
        private static ref ast.Ident Ident(this ref typeConv c, @string s)
        {
            return ast.NewIdent(s);
        }

        // Opaque type of n bytes.
        private static ast.Expr Opaque(this ref typeConv c, long n)
        {
            return ref new ast.ArrayType(Len:c.intExpr(n),Elt:c.byte,);
        }

        // Expr for integer n.
        private static ast.Expr intExpr(this ref typeConv c, long n)
        {
            return ref new ast.BasicLit(Kind:token.INT,Value:strconv.FormatInt(n,10),);
        }

        // Add padding of given size to fld.
        private static (slice<ref ast.Field>, slice<long>) pad(this ref typeConv c, slice<ref ast.Field> fld, slice<long> sizes, long size)
        {
            var n = len(fld);
            fld = fld[0L..n + 1L];
            fld[n] = ref new ast.Field(Names:[]*ast.Ident{c.Ident("_")},Type:c.Opaque(size));
            sizes = sizes[0L..n + 1L];
            sizes[n] = size;
            return (fld, sizes);
        }

        // Struct conversion: return Go and (gc) C syntax for type.
        private static (ref ast.StructType, @string, long) Struct(this ref typeConv c, ref dwarf.StructType dt, token.Pos pos)
        { 
            // Minimum alignment for a struct is 1 byte.
            align = 1L;

            bytes.Buffer buf = default;
            buf.WriteString("struct {");
            var fld = make_slice<ref ast.Field>(0L, 2L * len(dt.Field) + 1L); // enough for padding around every field
            var sizes = make_slice<long>(0L, 2L * len(dt.Field) + 1L);
            var off = int64(0L); 

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

                foreach (var (_, __f) in dt.Field)
                {
                    f = __f;
                    ident[f.Name] = f.Name;
                    used[f.Name] = true;
                }

                f = f__prev1;
            }

            if (!godefs.Value)
            {
                foreach (var (cid, goid) in ident)
                {
                    if (token.Lookup(goid).IsKeyword())
                    { 
                        // Avoid keyword
                        goid = "_" + goid; 

                        // Also avoid existing fields
                        {
                            var (_, exist) = used[goid];

                            while (exist)
                            {
                                goid = "_" + goid;
                                _, exist = used[goid];
                            }

                        }

                        used[goid] = true;
                        ident[cid] = goid;
                    }
                }
            }
            long anon = 0L;
            {
                var f__prev1 = f;

                foreach (var (_, __f) in dt.Field)
                {
                    f = __f;
                    if (f.ByteOffset > off)
                    {
                        fld, sizes = c.pad(fld, sizes, f.ByteOffset - off);
                        off = f.ByteOffset;
                    }
                    var name = f.Name;
                    var ft = f.Type; 

                    // In godefs mode, if this field is a C11
                    // anonymous union then treat the first field in the
                    // union as the field in the struct. This handles
                    // cases like the glibc <sys/resource.h> file; see
                    // issue 6677.
                    if (godefs.Value)
                    {
                        {
                            ref dwarf.StructType (st, ok) = f.Type._<ref dwarf.StructType>();

                            if (ok && name == "" && st.Kind == "union" && len(st.Field) > 0L && !used[st.Field[0L].Name])
                            {
                                name = st.Field[0L].Name;
                                ident[name] = name;
                                ft = st.Field[0L].Type;
                            }

                        }
                    } 

                    // TODO: Handle fields that are anonymous structs by
                    // promoting the fields of the inner struct.
                    var t = c.Type(ft, pos);
                    var tgo = t.Go;
                    var size = t.Size;
                    var talign = t.Align;
                    if (f.BitSize > 0L)
                    {
                        switch (f.BitSize)
                        {
                            case 8L: 

                            case 16L: 

                            case 32L: 

                            case 64L: 
                                break;
                            default: 
                                continue;
                                break;
                        }
                        size = f.BitSize / 8L;
                        name = tgo._<ref ast.Ident>().String();
                        if (strings.HasPrefix(name, "int"))
                        {
                            name = "int";
                        }
                        else
                        {
                            name = "uint";
                        }
                        tgo = ast.NewIdent(name + fmt.Sprint(f.BitSize));
                        talign = size;
                    }
                    if (talign > 0L && f.ByteOffset % talign != 0L)
                    { 
                        // Drop misaligned fields, the same way we drop integer bit fields.
                        // The goal is to make available what can be made available.
                        // Otherwise one bad and unneeded field in an otherwise okay struct
                        // makes the whole program not compile. Much of the time these
                        // structs are in system headers that cannot be corrected.
                        continue;
                    }
                    var n = len(fld);
                    fld = fld[0L..n + 1L];
                    if (name == "")
                    {
                        name = fmt.Sprintf("anon%d", anon);
                        anon++;
                        ident[name] = name;
                    }
                    fld[n] = ref new ast.Field(Names:[]*ast.Ident{c.Ident(ident[name])},Type:tgo);
                    sizes = sizes[0L..n + 1L];
                    sizes[n] = size;
                    off += size;
                    buf.WriteString(t.C.String());
                    buf.WriteString(" ");
                    buf.WriteString(name);
                    buf.WriteString("; ");
                    if (talign > align)
                    {
                        align = talign;
                    }
                }

                f = f__prev1;
            }

            if (off < dt.ByteSize)
            {
                fld, sizes = c.pad(fld, sizes, dt.ByteSize - off);
                off = dt.ByteSize;
            } 

            // If the last field in a non-zero-sized struct is zero-sized
            // the compiler is going to pad it by one (see issue 9401).
            // We can't permit that, because then the size of the Go
            // struct will not be the same as the size of the C struct.
            // Our only option in such a case is to remove the field,
            // which means that it cannot be referenced from Go.
            while (off > 0L && sizes[len(sizes) - 1L] == 0L)
            {
                n = len(sizes);
                fld = fld[0L..n - 1L];
                sizes = sizes[0L..n - 1L];
            }


            if (off != dt.ByteSize)
            {
                fatalf("%s: struct size calculation error off=%d bytesize=%d", lineno(pos), off, dt.ByteSize);
            }
            buf.WriteString("}");
            csyntax = buf.String();

            if (godefs.Value)
            {
                godefsFields(fld);
            }
            expr = ref new ast.StructType(Fields:&ast.FieldList{List:fld});
            return;
        }

        // dwarfHasPointer returns whether the DWARF type dt contains a pointer.
        private static bool dwarfHasPointer(this ref typeConv c, dwarf.Type dt, token.Pos pos)
        {
            switch (dt.type())
            {
                case ref dwarf.AddrType dt:
                    return false;
                    break;
                case ref dwarf.BoolType dt:
                    return false;
                    break;
                case ref dwarf.CharType dt:
                    return false;
                    break;
                case ref dwarf.EnumType dt:
                    return false;
                    break;
                case ref dwarf.FloatType dt:
                    return false;
                    break;
                case ref dwarf.ComplexType dt:
                    return false;
                    break;
                case ref dwarf.FuncType dt:
                    return false;
                    break;
                case ref dwarf.IntType dt:
                    return false;
                    break;
                case ref dwarf.UcharType dt:
                    return false;
                    break;
                case ref dwarf.UintType dt:
                    return false;
                    break;
                case ref dwarf.VoidType dt:
                    return false;
                    break;
                case ref dwarf.ArrayType dt:
                    return c.dwarfHasPointer(dt.Type, pos);
                    break;
                case ref dwarf.PtrType dt:
                    return true;
                    break;
                case ref dwarf.QualType dt:
                    return c.dwarfHasPointer(dt.Type, pos);
                    break;
                case ref dwarf.StructType dt:
                    foreach (var (_, f) in dt.Field)
                    {
                        if (c.dwarfHasPointer(f.Type, pos))
                        {
                            return true;
                        }
                    }
                    return false;
                    break;
                case ref dwarf.TypedefType dt:
                    if (dt.Name == "_GoString_" || dt.Name == "_GoBytes_")
                    {
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

        private static @string upper(@string s)
        {
            if (s == "")
            {
                return "";
            }
            var (r, size) = utf8.DecodeRuneInString(s);
            if (r == '_')
            {
                return "X" + s;
            }
            return string(unicode.ToUpper(r)) + s[size..];
        }

        // godefsFields rewrites field names for use in Go or C definitions.
        // It strips leading common prefixes (like tv_ in tv_sec, tv_usec)
        // converts names to upper case, and rewrites _ into Pad_godefs_n,
        // so that all fields are exported.
        private static void godefsFields(slice<ref ast.Field> fld)
        {
            var prefix = fieldPrefix(fld);
            long npad = 0L;
            foreach (var (_, f) in fld)
            {
                foreach (var (_, n) in f.Names)
                {
                    if (n.Name != prefix)
                    {
                        n.Name = strings.TrimPrefix(n.Name, prefix);
                    }
                    if (n.Name == "_")
                    { 
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
        private static @string fieldPrefix(slice<ref ast.Field> fld)
        {
            @string prefix = "";
            foreach (var (_, f) in fld)
            {
                foreach (var (_, n) in f.Names)
                { 
                    // Ignore field names that don't have the prefix we're
                    // looking for. It is common in C headers to have fields
                    // named, say, _pad in an otherwise prefixed header.
                    // If the struct has 3 fields tv_sec, tv_usec, _pad1, then we
                    // still want to remove the tv_ prefix.
                    // The check for "orig_" here handles orig_eax in the
                    // x86 ptrace register sets, which otherwise have all fields
                    // with reg_ prefixes.
                    if (strings.HasPrefix(n.Name, "orig_") || strings.HasPrefix(n.Name, "_"))
                    {
                        continue;
                    }
                    var i = strings.Index(n.Name, "_");
                    if (i < 0L)
                    {
                        continue;
                    }
                    if (prefix == "")
                    {
                        prefix = n.Name[..i + 1L];
                    }
                    else if (prefix != n.Name[..i + 1L])
                    {
                        return "";
                    }
                }
            }
            return prefix;
        }

        // badPointerTypedef reports whether t is a C typedef that should not be considered a pointer in Go.
        // A typedef is bad if C code sometimes stores non-pointers in this type.
        // TODO: Currently our best solution is to find these manually and list them as
        // they come up. A better solution is desired.
        private static bool badPointerTypedef(this ref typeConv c, ref dwarf.TypedefType dt)
        {
            if (c.badCFType(dt))
            {
                return true;
            }
            if (c.badJNI(dt))
            {
                return true;
            }
            return false;
        }

        private static bool badCFType(this ref typeConv c, ref dwarf.TypedefType dt)
        { 
            // The real bad types are CFNumberRef and CFDateRef.
            // Sometimes non-pointers are stored in these types.
            // CFTypeRef is a supertype of those, so it can have bad pointers in it as well.
            // We return true for the other *Ref types just so casting between them is easier.
            // We identify the correct set of types as those ending in Ref and for which
            // there exists a corresponding GetTypeID function.
            // See comment below for details about the bad pointers.
            if (goos != "darwin")
            {
                return false;
            }
            var s = dt.Name;
            if (!strings.HasSuffix(s, "Ref"))
            {
                return false;
            }
            s = s[..len(s) - 3L];
            if (s == "CFType")
            {
                return true;
            }
            if (c.getTypeIDs[s])
            {
                return true;
            }
            {
                var i = strings.Index(s, "Mutable");

                if (i >= 0L && c.getTypeIDs[s[..i] + s[i + 7L..]])
                { 
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

        private static bool badJNI(this ref typeConv c, ref dwarf.TypedefType dt)
        { 
            // In Dalvik and ART, the jobject type in the JNI interface of the JVM has the
            // property that it is sometimes (always?) a small integer instead of a real pointer.
            // Note: although only the android JVMs are bad in this respect, we declare the JNI types
            // bad regardless of platform, so the same Go code compiles on both android and non-android.
            {
                var (parent, ok) = jniTypes[dt.Name];

                if (ok)
                { 
                    // Try to make sure we're talking about a JNI type, not just some random user's
                    // type that happens to use the same name.
                    // C doesn't have the notion of a package, so it's hard to be certain.

                    // Walk up to jobject, checking each typedef on the way.
                    var w = dt;
                    while (parent != "")
                    {
                        ref dwarf.TypedefType (t, ok) = w.Type._<ref dwarf.TypedefType>();
                        if (!ok || t.Name != parent)
                        {
                            return false;
                        }
                        w = t;
                        parent, ok = jniTypes[w.Name];
                        if (!ok)
                        {
                            return false;
                        }
                    } 

                    // Check that the typedef is:
                    //     struct _jobject;
                    //     typedef struct _jobject *jobject;
 

                    // Check that the typedef is:
                    //     struct _jobject;
                    //     typedef struct _jobject *jobject;
                    {
                        ref dwarf.PtrType (ptr, ok) = w.Type._<ref dwarf.PtrType>();

                        if (ok)
                        {
                            {
                                ref dwarf.StructType (str, ok) = ptr.Type._<ref dwarf.StructType>();

                                if (ok)
                                {
                                    if (str.StructName == "_jobject" && str.Kind == "struct" && len(str.Field) == 0L && str.Incomplete)
                                    {
                                        return true;
                                    }
                                }

                            }
                        }

                    }
                }

            }
            return false;
        }

        // jniTypes maps from JNI types that we want to be uintptrs, to the underlying type to which
        // they are mapped.  The base "jobject" maps to the empty string.
        private static map jniTypes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"jobject":"","jclass":"jobject","jthrowable":"jobject","jstring":"jobject","jarray":"jobject","jbooleanArray":"jarray","jbyteArray":"jarray","jcharArray":"jarray","jshortArray":"jarray","jintArray":"jarray","jlongArray":"jarray","jfloatArray":"jarray","jdoubleArray":"jarray","jobjectArray":"jarray","jweak":"jobject",};
    }
}
