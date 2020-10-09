// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gccgoimporter -- go2cs converted at 2020 October 09 06:02:52 UTC
// import "go/internal/gccgoimporter" ==> using gccgoimporter = go.go.@internal.gccgoimporter_package
// Original source: C:\Go\src\go\internal\gccgoimporter\parser.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gccgoimporter_package
    {
        private partial struct parser
        {
            public ptr<scanner.Scanner> scanner;
            public @string version; // format version
            public int tok; // current token
            public @string lit; // literal string; only valid for Ident, Int, String tokens
            public @string pkgpath; // package path of imported package
            public @string pkgname; // name of imported package
            public ptr<types.Package> pkg; // reference to imported package
            public map<@string, ptr<types.Package>> imports; // package path -> package object
            public slice<types.Type> typeList; // type number -> type
            public slice<@string> typeData; // unparsed type data (v3 and later)
            public slice<fixupRecord> fixups; // fixups to apply at end of parsing
            public InitData initdata; // package init priority data
            public map<long, @string> aliases; // maps saved type number to alias name
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

        private partial struct fixupRecord
        {
            public ptr<types.Named> toUpdate; // type to modify when fixup is processed
            public types.Type target; // type that was incomplete when fixup was created
        }

        private static void init(this ptr<parser> _addr_p, @string filename, io.Reader src, map<@string, ptr<types.Package>> imports)
        {
            ref parser p = ref _addr_p.val;

            p.scanner = @new<scanner.Scanner>();
            p.initScanner(filename, src);
            p.imports = imports;
            p.aliases = make_map<long, @string>();
            p.typeList = make_slice<types.Type>(1L, 16L);
        }

        private static void initScanner(this ptr<parser> _addr_p, @string filename, io.Reader src)
        {
            ref parser p = ref _addr_p.val;

            p.scanner.Init(src);
            p.scanner.Error = (_, msg) =>
            {
                p.error(msg);
            }
;
            p.scanner.Mode = scanner.ScanIdents | scanner.ScanInts | scanner.ScanFloats | scanner.ScanStrings;
            p.scanner.Whitespace = 1L << (int)('\t') | 1L << (int)(' ');
            p.scanner.Filename = filename; // for good error messages
            p.next();

        }

        private partial struct importError
        {
            public scanner.Position pos;
            public error err;
        }

        private static @string Error(this importError e)
        {
            return fmt.Sprintf("import error %s (byte offset = %d): %s", e.pos, e.pos.Offset, e.err);
        }

        private static void error(this ptr<parser> _addr_p, object err) => func((_, panic, __) =>
        {
            ref parser p = ref _addr_p.val;

            {
                @string (s, ok) = err._<@string>();

                if (ok)
                {
                    err = errors.New(s);
                } 
                // panic with a runtime.Error if err is not an error

            } 
            // panic with a runtime.Error if err is not an error
            panic(new importError(p.scanner.Pos(),err.(error)));

        });

        private static void errorf(this ptr<parser> _addr_p, @string format, params object[] args)
        {
            args = args.Clone();
            ref parser p = ref _addr_p.val;

            p.error(fmt.Errorf(format, args));
        }

        private static @string expect(this ptr<parser> _addr_p, int tok)
        {
            ref parser p = ref _addr_p.val;

            var lit = p.lit;
            if (p.tok != tok)
            {
                p.errorf("expected %s, got %s (%s)", scanner.TokenString(tok), scanner.TokenString(p.tok), lit);
            }

            p.next();
            return lit;

        }

        private static void expectEOL(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            if (p.version == "v1" || p.version == "v2")
            {
                p.expect(';');
            }

            p.expect('\n');

        }

        private static void expectKeyword(this ptr<parser> _addr_p, @string keyword)
        {
            ref parser p = ref _addr_p.val;

            var lit = p.expect(scanner.Ident);
            if (lit != keyword)
            {
                p.errorf("expected keyword %s, got %q", keyword, lit);
            }

        }

        private static @string parseString(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            var (str, err) = strconv.Unquote(p.expect(scanner.String));
            if (err != null)
            {
                p.error(err);
            }

            return str;

        }

        // unquotedString     = { unquotedStringChar } .
        // unquotedStringChar = <neither a whitespace nor a ';' char> .
        private static @string parseUnquotedString(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            if (p.tok == scanner.EOF)
            {
                p.error("unexpected EOF");
            }

            bytes.Buffer buf = default;
            buf.WriteString(p.scanner.TokenText()); 
            // This loop needs to examine each character before deciding whether to consume it. If we see a semicolon,
            // we need to let it be consumed by p.next().
            {
                var ch = p.scanner.Peek();

                while (ch != '\n' && ch != ';' && ch != scanner.EOF && p.scanner.Whitespace & (1L << (int)(uint(ch))) == 0L)
                {
                    buf.WriteRune(ch);
                    p.scanner.Next();
                    ch = p.scanner.Peek();
                }

            }
            p.next();
            return buf.String();

        }

        private static void next(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            p.tok = p.scanner.Scan();

            if (p.tok == scanner.Ident || p.tok == scanner.Int || p.tok == scanner.Float || p.tok == scanner.String || p.tok == '·') 
                p.lit = p.scanner.TokenText();
            else 
                p.lit = "";
            
        }

        private static (@string, @string) parseQualifiedName(this ptr<parser> _addr_p)
        {
            @string path = default;
            @string name = default;
            ref parser p = ref _addr_p.val;

            return p.parseQualifiedNameStr(p.parseString());
        }

        private static (@string, @string) parseUnquotedQualifiedName(this ptr<parser> _addr_p)
        {
            @string path = default;
            @string name = default;
            ref parser p = ref _addr_p.val;

            return p.parseQualifiedNameStr(p.parseUnquotedString());
        }

        // qualifiedName = [ ["."] unquotedString "." ] unquotedString .
        //
        // The above production uses greedy matching.
        private static (@string, @string) parseQualifiedNameStr(this ptr<parser> _addr_p, @string unquotedName)
        {
            @string pkgpath = default;
            @string name = default;
            ref parser p = ref _addr_p.val;

            var parts = strings.Split(unquotedName, ".");
            if (parts[0L] == "")
            {
                parts = parts[1L..];
            }

            switch (len(parts))
            {
                case 0L: 
                    p.errorf("malformed qualified name: %q", unquotedName);
                    break;
                case 1L: 
                    // unqualified name
                    pkgpath = p.pkgpath;
                    name = parts[0L];
                    break;
                default: 
                    // qualified name, which may contain periods
                    pkgpath = strings.Join(parts[0L..len(parts) - 1L], ".");
                    name = parts[len(parts) - 1L];
                    break;
            }

            return ;

        }

        // getPkg returns the package for a given path. If the package is
        // not found but we have a package name, create the package and
        // add it to the p.imports map.
        //
        private static ptr<types.Package> getPkg(this ptr<parser> _addr_p, @string pkgpath, @string name)
        {
            ref parser p = ref _addr_p.val;
 
            // package unsafe is not in the imports map - handle explicitly
            if (pkgpath == "unsafe")
            {
                return _addr_types.Unsafe!;
            }

            var pkg = p.imports[pkgpath];
            if (pkg == null && name != "")
            {
                pkg = types.NewPackage(pkgpath, name);
                p.imports[pkgpath] = pkg;
            }

            return _addr_pkg!;

        }

        // parseExportedName is like parseQualifiedName, but
        // the package path is resolved to an imported *types.Package.
        //
        // ExportedName = string [string] .
        private static (ptr<types.Package>, @string) parseExportedName(this ptr<parser> _addr_p)
        {
            ptr<types.Package> pkg = default!;
            @string name = default;
            ref parser p = ref _addr_p.val;

            var (path, name) = p.parseQualifiedName();
            @string pkgname = default;
            if (p.tok == scanner.String)
            {
                pkgname = p.parseString();
            }

            pkg = p.getPkg(path, pkgname);
            if (pkg == null)
            {
                p.errorf("package %s (path = %q) not found", name, path);
            }

            return ;

        }

        // Name = QualifiedName | "?" .
        private static @string parseName(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            if (p.tok == '?')
            { 
                // Anonymous.
                p.next();
                return "";

            } 
            // The package path is redundant for us. Don't try to parse it.
            var (_, name) = p.parseUnquotedQualifiedName();
            return name;

        }

        private static types.Type deref(types.Type typ)
        {
            {
                ptr<types.Pointer> (p, _) = typ._<ptr<types.Pointer>>();

                if (p != null)
                {
                    typ = p.Elem();
                }

            }

            return typ;

        }

        // Field = Name Type [string] .
        private static (ptr<types.Var>, @string) parseField(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ptr<types.Var> field = default!;
            @string tag = default;
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var name = p.parseName();
            var (typ, n) = p.parseTypeExtended(pkg);
            var anon = false;
            if (name == "")
            {
                anon = true; 
                // Alias?
                {
                    var (aname, ok) = p.aliases[n];

                    if (ok)
                    {
                        name = aname;
                    }
                    else
                    {
                        switch (deref(typ).type())
                        {
                            case ptr<types.Basic> typ:
                                name = typ.Name();
                                break;
                            case ptr<types.Named> typ:
                                name = typ.Obj().Name();
                                break;
                            default:
                            {
                                var typ = deref(typ).type();
                                p.error("embedded field expected");
                                break;
                            }
                        }

                    }

                }

            }

            field = types.NewField(token.NoPos, pkg, name, typ, anon);
            if (p.tok == scanner.String)
            {
                tag = p.parseString();
            }

            return ;

        }

        // Param = Name ["..."] Type .
        private static (ptr<types.Var>, bool) parseParam(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ptr<types.Var> param = default!;
            bool isVariadic = default;
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var name = p.parseName(); 
            // Ignore names invented for inlinable functions.
            if (strings.HasPrefix(name, "p.") || strings.HasPrefix(name, "r.") || strings.HasPrefix(name, "$ret"))
            {
                name = "";
            }

            if (p.tok == '<' && p.scanner.Peek() == 'e')
            { 
                // EscInfo = "<esc:" int ">" . (optional and ignored)
                p.next();
                p.expectKeyword("esc");
                p.expect(':');
                p.expect(scanner.Int);
                p.expect('>');

            }

            if (p.tok == '.')
            {
                p.next();
                p.expect('.');
                p.expect('.');
                isVariadic = true;
            }

            var typ = p.parseType(pkg);
            if (isVariadic)
            {
                typ = types.NewSlice(typ);
            }

            param = types.NewParam(token.NoPos, pkg, name, typ);
            return ;

        }

        // Var = Name Type .
        private static ptr<types.Var> parseVar(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var name = p.parseName();
            var v = types.NewVar(token.NoPos, pkg, name, p.parseType(pkg));
            if (name[0L] == '.' || name[0L] == '<')
            { 
                // This is an unexported variable,
                // or a variable defined in a different package.
                // We only want to record exported variables.
                return _addr_null!;

            }

            return _addr_v!;

        }

        // Conversion = "convert" "(" Type "," ConstValue ")" .
        private static (constant.Value, types.Type) parseConversion(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            constant.Value val = default;
            types.Type typ = default;
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("convert");
            p.expect('(');
            typ = p.parseType(pkg);
            p.expect(',');
            val, _ = p.parseConstValue(pkg);
            p.expect(')');
            return ;
        }

        // ConstValue     = string | "false" | "true" | ["-"] (int ["'"] | FloatOrComplex) | Conversion .
        // FloatOrComplex = float ["i" | ("+"|"-") float "i"] .
        private static (constant.Value, types.Type) parseConstValue(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            constant.Value val = default;
            types.Type typ = default;
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;
 
            // v3 changed to $false, $true, $convert, to avoid confusion
            // with variable names in inline function bodies.
            if (p.tok == '$')
            {
                p.next();
                if (p.tok != scanner.Ident)
                {
                    p.errorf("expected identifier after '$', got %s (%q)", scanner.TokenString(p.tok), p.lit);
                }

            }


            if (p.tok == scanner.String) 
                var str = p.parseString();
                val = constant.MakeString(str);
                typ = types.Typ[types.UntypedString];
                return ;
            else if (p.tok == scanner.Ident) 
                var b = false;
                switch (p.lit)
                {
                    case "false": 
                        break;
                    case "true": 
                        b = true;
                        break;
                    case "convert": 
                        return p.parseConversion(pkg);
                        break;
                    default: 
                        p.errorf("expected const value, got %s (%q)", scanner.TokenString(p.tok), p.lit);
                        break;
                }

                p.next();
                val = constant.MakeBool(b);
                typ = types.Typ[types.UntypedBool];
                return ;
                        @string sign = "";
            if (p.tok == '-')
            {
                p.next();
                sign = "-";
            }


            if (p.tok == scanner.Int) 
                val = constant.MakeFromLiteral(sign + p.lit, token.INT, 0L);
                if (val == null)
                {
                    p.error("could not parse integer literal");
                }

                p.next();
                if (p.tok == '\'')
                {
                    p.next();
                    typ = types.Typ[types.UntypedRune];
                }
                else
                {
                    typ = types.Typ[types.UntypedInt];
                }

            else if (p.tok == scanner.Float) 
                var re = sign + p.lit;
                p.next();

                @string im = default;

                if (p.tok == '+') 
                    p.next();
                    im = p.expect(scanner.Float);
                else if (p.tok == '-') 
                    p.next();
                    im = "-" + p.expect(scanner.Float);
                else if (p.tok == scanner.Ident) 
                    // re is in fact the imaginary component. Expect "i" below.
                    im = re;
                    re = "0";
                else 
                    val = constant.MakeFromLiteral(re, token.FLOAT, 0L);
                    if (val == null)
                    {
                        p.error("could not parse float literal");
                    }

                    typ = types.Typ[types.UntypedFloat];
                    return ;
                                p.expectKeyword("i");
                var reval = constant.MakeFromLiteral(re, token.FLOAT, 0L);
                if (reval == null)
                {
                    p.error("could not parse real component of complex literal");
                }

                var imval = constant.MakeFromLiteral(im + "i", token.IMAG, 0L);
                if (imval == null)
                {
                    p.error("could not parse imag component of complex literal");
                }

                val = constant.BinaryOp(reval, token.ADD, imval);
                typ = types.Typ[types.UntypedComplex];
            else 
                p.errorf("expected const value, got %s (%q)", scanner.TokenString(p.tok), p.lit);
                        return ;

        }

        // Const = Name [Type] "=" ConstValue .
        private static ptr<types.Const> parseConst(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var name = p.parseName();
            types.Type typ = default;
            if (p.tok == '<')
            {
                typ = p.parseType(pkg);
            }

            p.expect('=');
            var (val, vtyp) = p.parseConstValue(pkg);
            if (typ == null)
            {
                typ = vtyp;
            }

            return _addr_types.NewConst(token.NoPos, pkg, name, typ, val)!;

        }

        // reserved is a singleton type used to fill type map slots that have
        // been reserved (i.e., for which a type number has been parsed) but
        // which don't have their actual type yet. When the type map is updated,
        // the actual type must replace a reserved entry (or we have an internal
        // error). Used for self-verification only - not required for correctness.
        private static ptr<object> reserved = @new<>();

        // reserve reserves the type map entry n for future use.
        private static void reserve(this ptr<parser> _addr_p, long n)
        {
            ref parser p = ref _addr_p.val;
 
            // Notes:
            // - for pre-V3 export data, the type numbers we see are
            //   guaranteed to be in increasing order, so we append a
            //   reserved entry onto the list.
            // - for V3+ export data, type numbers can appear in
            //   any order, however the 'types' section tells us the
            //   total number of types, hence typeList is pre-allocated.
            if (len(p.typeData) == 0L)
            {
                if (n != len(p.typeList))
                {
                    p.errorf("invalid type number %d (out of sync)", n);
                }

                p.typeList = append(p.typeList, reserved);

            }
            else
            {
                if (p.typeList[n] != null)
                {
                    p.errorf("previously visited type number %d", n);
                }

                p.typeList[n] = reserved;

            }

        }

        // update sets the type map entries for the entries in nlist to t.
        // An entry in nlist can be a type number in p.typeList,
        // used to resolve named types, or it can be a *types.Pointer,
        // used to resolve pointers to named types in case they are referenced
        // by embedded fields.
        private static void update(this ptr<parser> _addr_p, types.Type t, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;

            if (t == reserved)
            {
                p.errorf("internal error: update(%v) invoked on reserved", nlist);
            }

            if (t == null)
            {
                p.errorf("internal error: update(%v) invoked on nil", nlist);
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in nlist)
                {
                    n = __n;
                    switch (n.type())
                    {
                        case long n:
                            if (p.typeList[n] == t)
                            {
                                continue;
                            }

                            if (p.typeList[n] != reserved)
                            {
                                p.errorf("internal error: update(%v): %d not reserved", nlist, n);
                            }

                            p.typeList[n] = t;
                            break;
                        case ptr<types.Pointer> n:
                            if (n != (new types.Pointer()).val)
                            {
                                var elem = n.Elem();
                                if (elem == t)
                                {
                                    continue;
                                }

                                p.errorf("internal error: update: pointer already set to %v, expected %v", elem, t);

                            }

                            n.val = new ptr<ptr<types.NewPointer>>(t);
                            break;
                        default:
                        {
                            var n = n.type();
                            p.errorf("internal error: %T on nlist", n);
                            break;
                        }
                    }

                }

                n = n__prev1;
            }
        }

        // NamedType = TypeName [ "=" ] Type { Method } .
        // TypeName  = ExportedName .
        // Method    = "func" "(" Param ")" Name ParamList ResultList [InlineBody] ";" .
        private static types.Type parseNamedType(this ptr<parser> _addr_p, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;

            var (pkg, name) = p.parseExportedName();
            var scope = pkg.Scope();
            var obj = scope.Lookup(name);
            if (obj != null && obj.Type() == null)
            {
                p.errorf("%v has nil type", obj);
            } 

            // type alias
            if (p.tok == '=')
            {
                p.next();
                p.aliases[nlist[len(nlist) - 1L]._<long>()] = name;
                if (obj != null)
                { 
                    // use the previously imported (canonical) type
                    var t = obj.Type();
                    p.update(t, nlist);
                    p.parseType(pkg); // discard
                    return t;

                }

                t = p.parseType(pkg, nlist);
                obj = types.NewTypeName(token.NoPos, pkg, name, t);
                scope.Insert(obj);
                return t;

            } 

            // defined type
            if (obj == null)
            { 
                // A named type may be referred to before the underlying type
                // is known - set it up.
                var tname = types.NewTypeName(token.NoPos, pkg, name, null);
                types.NewNamed(tname, null, null);
                scope.Insert(tname);
                obj = tname;

            } 

            // use the previously imported (canonical), or newly created type
            t = obj.Type();
            p.update(t, nlist);

            ptr<types.Named> (nt, ok) = t._<ptr<types.Named>>();
            if (!ok)
            { 
                // This can happen for unsafe.Pointer, which is a TypeName holding a Basic type.
                var pt = p.parseType(pkg);
                if (pt != t)
                {
                    p.error("unexpected underlying type for non-named TypeName");
                }

                return t;

            }

            var underlying = p.parseType(pkg);
            if (nt.Underlying() == null)
            {
                if (underlying.Underlying() == null)
                {
                    fixupRecord fix = new fixupRecord(toUpdate:nt,target:underlying);
                    p.fixups = append(p.fixups, fix);
                }
                else
                {
                    nt.SetUnderlying(underlying.Underlying());
                }

            }

            if (p.tok == '\n')
            {
                p.next(); 
                // collect associated methods
                while (p.tok == scanner.Ident)
                {
                    p.expectKeyword("func");
                    if (p.tok == '/')
                    { 
                        // Skip a /*nointerface*/ or /*asm ID */ comment.
                        p.expect('/');
                        p.expect('*');
                        if (p.expect(scanner.Ident) == "asm")
                        {
                            p.parseUnquotedString();
                        }

                        p.expect('*');
                        p.expect('/');

                    }

                    p.expect('(');
                    var (receiver, _) = p.parseParam(pkg);
                    p.expect(')');
                    var name = p.parseName();
                    var (params, isVariadic) = p.parseParamList(pkg);
                    var results = p.parseResultList(pkg);
                    p.skipInlineBody();
                    p.expectEOL();

                    var sig = types.NewSignature(receiver, params, results, isVariadic);
                    nt.AddMethod(types.NewFunc(token.NoPos, pkg, name, sig));

                }


            }

            return nt;

        }

        private static long parseInt64(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            var lit = p.expect(scanner.Int);
            var (n, err) = strconv.ParseInt(lit, 10L, 64L);
            if (err != null)
            {
                p.error(err);
            }

            return n;

        }

        private static long parseInt(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            var lit = p.expect(scanner.Int);
            var (n, err) = strconv.ParseInt(lit, 10L, 0L);
            if (err != null)
            {
                p.error(err);
            }

            return int(n);

        }

        // ArrayOrSliceType = "[" [ int ] "]" Type .
        private static types.Type parseArrayOrSliceType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expect('[');
            if (p.tok == ']')
            {
                p.next();

                ptr<object> t = @new<types.Slice>();
                p.update(t, nlist);

                t.val = new ptr<ptr<types.NewSlice>>(p.parseType(pkg));
                return t;
            }

            t = @new<types.Array>();
            p.update(t, nlist);

            var len = p.parseInt64();
            p.expect(']');

            t.val = types.NewArray(p.parseType(pkg), len).val;
            return t;

        }

        // MapType = "map" "[" Type "]" Type .
        private static types.Type parseMapType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("map");

            ptr<object> t = @new<types.Map>();
            p.update(t, nlist);

            p.expect('[');
            var key = p.parseType(pkg);
            p.expect(']');
            var elem = p.parseType(pkg);

            t.val = types.NewMap(key, elem).val;
            return t;
        }

        // ChanType = "chan" ["<-" | "-<"] Type .
        private static types.Type parseChanType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("chan");

            ptr<object> t = @new<types.Chan>();
            p.update(t, nlist);

            var dir = types.SendRecv;
            switch (p.tok)
            {
                case '-': 
                    p.next();
                    p.expect('<');
                    dir = types.SendOnly;
                    break;
                case '<': 
                    // don't consume '<' if it belongs to Type
                    if (p.scanner.Peek() == '-')
                    {
                        p.next();
                        p.expect('-');
                        dir = types.RecvOnly;
                    }

                    break;
            }

            t.val = types.NewChan(dir, p.parseType(pkg)).val;
            return t;

        }

        // StructType = "struct" "{" { Field } "}" .
        private static types.Type parseStructType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("struct");

            ptr<object> t = @new<types.Struct>();
            p.update(t, nlist);

            slice<ptr<types.Var>> fields = default;
            slice<@string> tags = default;

            p.expect('{');
            while (p.tok != '}' && p.tok != scanner.EOF)
            {
                var (field, tag) = p.parseField(pkg);
                p.expect(';');
                fields = append(fields, field);
                tags = append(tags, tag);
            }

            p.expect('}');

            t.val = types.NewStruct(fields, tags).val;
            return t;

        }

        // ParamList = "(" [ { Parameter "," } Parameter ] ")" .
        private static (ptr<types.Tuple>, bool) parseParamList(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ptr<types.Tuple> _p0 = default!;
            bool _p0 = default;
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            slice<ptr<types.Var>> list = default;
            var isVariadic = false;

            p.expect('(');
            while (p.tok != ')' && p.tok != scanner.EOF)
            {
                if (len(list) > 0L)
                {
                    p.expect(',');
                }

                var (par, variadic) = p.parseParam(pkg);
                list = append(list, par);
                if (variadic)
                {
                    if (isVariadic)
                    {
                        p.error("... not on final argument");
                    }

                    isVariadic = true;

                }

            }

            p.expect(')');

            return (_addr_types.NewTuple(list)!, isVariadic);

        }

        // ResultList = Type | ParamList .
        private static ptr<types.Tuple> parseResultList(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            switch (p.tok)
            {
                case '<': 
                    p.next();
                    if (p.tok == scanner.Ident && p.lit == "inl")
                    {
                        return _addr_null!;
                    }

                    var (taa, _) = p.parseTypeAfterAngle(pkg);
                    return _addr_types.NewTuple(types.NewParam(token.NoPos, pkg, "", taa))!;
                    break;
                case '(': 
                    var (params, _) = p.parseParamList(pkg);
                    return _addr_params!;
                    break;
                default: 
                    return _addr_null!;
                    break;
            }

        }

        // FunctionType = ParamList ResultList .
        private static ptr<types.Signature> parseFunctionType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            ptr<types.Signature> t = @new<types.Signature>();
            p.update(t, nlist);

            var (params, isVariadic) = p.parseParamList(pkg);
            var results = p.parseResultList(pkg);

            t.val = types.NewSignature(null, params, results, isVariadic).val;
            return _addr_t!;
        }

        // Func = Name FunctionType [InlineBody] .
        private static ptr<types.Func> parseFunc(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            if (p.tok == '/')
            { 
                // Skip an /*asm ID */ comment.
                p.expect('/');
                p.expect('*');
                if (p.expect(scanner.Ident) == "asm")
                {
                    p.parseUnquotedString();
                }

                p.expect('*');
                p.expect('/');

            }

            var name = p.parseName();
            var f = types.NewFunc(token.NoPos, pkg, name, p.parseFunctionType(pkg, null));
            p.skipInlineBody();

            if (name[0L] == '.' || name[0L] == '<' || strings.ContainsRune(name, '$'))
            { 
                // This is an unexported function,
                // or a function defined in a different package,
                // or a type$equal or type$hash function.
                // We only want to record exported functions.
                return _addr_null!;

            }

            return _addr_f!;

        }

        // InterfaceType = "interface" "{" { ("?" Type | Func) ";" } "}" .
        private static types.Type parseInterfaceType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("interface");

            ptr<object> t = @new<types.Interface>();
            p.update(t, nlist);

            slice<ptr<types.Func>> methods = default;
            slice<types.Type> embeddeds = default;

            p.expect('{');
            while (p.tok != '}' && p.tok != scanner.EOF)
            {
                if (p.tok == '?')
                {
                    p.next();
                    embeddeds = append(embeddeds, p.parseType(pkg));
                }
                else
                {
                    var method = p.parseFunc(pkg);
                    if (method != null)
                    {
                        methods = append(methods, method);
                    }

                }

                p.expect(';');

            }

            p.expect('}');

            t.val = types.NewInterfaceType(methods, embeddeds).val;
            return t;

        }

        // PointerType = "*" ("any" | Type) .
        private static types.Type parsePointerType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expect('*');
            if (p.tok == scanner.Ident)
            {
                p.expectKeyword("any");
                var t = types.Typ[types.UnsafePointer];
                p.update(t, nlist);
                return t;
            }

            t = @new<types.Pointer>();
            p.update(t, nlist);

            t.val = new ptr<ptr<types.NewPointer>>(p.parseType(pkg, t));

            return t;

        }

        // TypeSpec = NamedType | MapType | ChanType | StructType | InterfaceType | PointerType | ArrayOrSliceType | FunctionType .
        private static types.Type parseTypeSpec(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, slice<object> nlist)
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;


            if (p.tok == scanner.String) 
                return p.parseNamedType(nlist);
            else if (p.tok == scanner.Ident) 
                switch (p.lit)
                {
                    case "map": 
                        return p.parseMapType(pkg, nlist);
                        break;
                    case "chan": 
                        return p.parseChanType(pkg, nlist);
                        break;
                    case "struct": 
                        return p.parseStructType(pkg, nlist);
                        break;
                    case "interface": 
                        return p.parseInterfaceType(pkg, nlist);
                        break;
                }
            else if (p.tok == '*') 
                return p.parsePointerType(pkg, nlist);
            else if (p.tok == '[') 
                return p.parseArrayOrSliceType(pkg, nlist);
            else if (p.tok == '(') 
                return p.parseFunctionType(pkg, nlist);
                        p.errorf("expected type name or literal, got %s", scanner.TokenString(p.tok));
            return null;

        }

 
        // From gofrontend/go/export.h
        // Note that these values are negative in the gofrontend and have been made positive
        // in the gccgoimporter.
        private static readonly long gccgoBuiltinINT8 = (long)1L;
        private static readonly long gccgoBuiltinINT16 = (long)2L;
        private static readonly long gccgoBuiltinINT32 = (long)3L;
        private static readonly long gccgoBuiltinINT64 = (long)4L;
        private static readonly long gccgoBuiltinUINT8 = (long)5L;
        private static readonly long gccgoBuiltinUINT16 = (long)6L;
        private static readonly long gccgoBuiltinUINT32 = (long)7L;
        private static readonly long gccgoBuiltinUINT64 = (long)8L;
        private static readonly long gccgoBuiltinFLOAT32 = (long)9L;
        private static readonly long gccgoBuiltinFLOAT64 = (long)10L;
        private static readonly long gccgoBuiltinINT = (long)11L;
        private static readonly long gccgoBuiltinUINT = (long)12L;
        private static readonly long gccgoBuiltinUINTPTR = (long)13L;
        private static readonly long gccgoBuiltinBOOL = (long)15L;
        private static readonly long gccgoBuiltinSTRING = (long)16L;
        private static readonly long gccgoBuiltinCOMPLEX64 = (long)17L;
        private static readonly long gccgoBuiltinCOMPLEX128 = (long)18L;
        private static readonly long gccgoBuiltinERROR = (long)19L;
        private static readonly long gccgoBuiltinBYTE = (long)20L;
        private static readonly long gccgoBuiltinRUNE = (long)21L;


        private static types.Type lookupBuiltinType(long typ)
        {
            return new array<types.Type>(InitKeyedValues<types.Type>((gccgoBuiltinINT8, types.Typ[types.Int8]), (gccgoBuiltinINT16, types.Typ[types.Int16]), (gccgoBuiltinINT32, types.Typ[types.Int32]), (gccgoBuiltinINT64, types.Typ[types.Int64]), (gccgoBuiltinUINT8, types.Typ[types.Uint8]), (gccgoBuiltinUINT16, types.Typ[types.Uint16]), (gccgoBuiltinUINT32, types.Typ[types.Uint32]), (gccgoBuiltinUINT64, types.Typ[types.Uint64]), (gccgoBuiltinFLOAT32, types.Typ[types.Float32]), (gccgoBuiltinFLOAT64, types.Typ[types.Float64]), (gccgoBuiltinINT, types.Typ[types.Int]), (gccgoBuiltinUINT, types.Typ[types.Uint]), (gccgoBuiltinUINTPTR, types.Typ[types.Uintptr]), (gccgoBuiltinBOOL, types.Typ[types.Bool]), (gccgoBuiltinSTRING, types.Typ[types.String]), (gccgoBuiltinCOMPLEX64, types.Typ[types.Complex64]), (gccgoBuiltinCOMPLEX128, types.Typ[types.Complex128]), (gccgoBuiltinERROR, types.Universe.Lookup("error").Type()), (gccgoBuiltinBYTE, types.Universe.Lookup("byte").Type()), (gccgoBuiltinRUNE, types.Universe.Lookup("rune").Type())))[typ];
        }

        // Type = "<" "type" ( "-" int | int [ TypeSpec ] ) ">" .
        //
        // parseType updates the type map to t for all type numbers n.
        //
        private static types.Type parseType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, params object[] n)
        {
            n = n.Clone();
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expect('<');
            var (t, _) = p.parseTypeAfterAngle(pkg, n);
            return t;
        }

        // (*parser).Type after reading the "<".
        private static (types.Type, long) parseTypeAfterAngle(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, params object[] n)
        {
            types.Type t = default;
            long n1 = default;
            n = n.Clone();
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expectKeyword("type");

            n1 = 0L;

            if (p.tok == scanner.Int) 
                n1 = p.parseInt();
                if (p.tok == '>')
                {
                    if (len(p.typeData) > 0L && p.typeList[n1] == null)
                    {
                        p.parseSavedType(pkg, n1, n);
                    }

                    t = p.typeList[n1];
                    if (len(p.typeData) == 0L && t == reserved)
                    {
                        p.errorf("invalid type cycle, type %d not yet defined (nlist=%v)", n1, n);
                    }

                    p.update(t, n);

                }
                else
                {
                    p.reserve(n1);
                    t = p.parseTypeSpec(pkg, append(n, n1));
                }

            else if (p.tok == '-') 
                p.next();
                var n1 = p.parseInt();
                t = lookupBuiltinType(n1);
                p.update(t, n);
            else 
                p.errorf("expected type number, got %s (%q)", scanner.TokenString(p.tok), p.lit);
                return (null, 0L);
                        if (t == null || t == reserved)
            {
                p.errorf("internal error: bad return from parseType(%v)", n);
            }

            p.expect('>');
            return ;

        }

        // parseTypeExtended is identical to parseType, but if the type in
        // question is a saved type, returns the index as well as the type
        // pointer (index returned is zero if we parsed a builtin).
        private static (types.Type, long) parseTypeExtended(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, params object[] n)
        {
            types.Type t = default;
            long n1 = default;
            n = n.Clone();
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            p.expect('<');
            t, n1 = p.parseTypeAfterAngle(pkg, n);
            return ;
        }

        // InlineBody = "<inl:NN>" .{NN}
        // Reports whether a body was skipped.
        private static void skipInlineBody(this ptr<parser> _addr_p) => func((defer, _, __) =>
        {
            ref parser p = ref _addr_p.val;
 
            // We may or may not have seen the '<' already, depending on
            // whether the function had a result type or not.
            if (p.tok == '<')
            {
                p.next();
                p.expectKeyword("inl");
            }
            else if (p.tok != scanner.Ident || p.lit != "inl")
            {
                return ;
            }
            else
            {
                p.next();
            }

            p.expect(':');
            var want = p.parseInt();
            p.expect('>');

            defer(w =>
            {
                p.scanner.Whitespace = w;
            }(p.scanner.Whitespace));
            p.scanner.Whitespace = 0L;

            long got = 0L;
            while (got < want)
            {
                var r = p.scanner.Next();
                if (r == scanner.EOF)
                {
                    p.error("unexpected EOF");
                }

                got += utf8.RuneLen(r);

            }


        });

        // Types = "types" maxp1 exportedp1 (offset length)* .
        private static void parseTypes(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg) => func((defer, _, __) =>
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var maxp1 = p.parseInt();
            var exportedp1 = p.parseInt();
            p.typeList = make_slice<types.Type>(maxp1, maxp1);

            private partial struct typeOffset
            {
                public long offset;
                public long length;
            }
            slice<typeOffset> typeOffsets = default;

            long total = 0L;
            {
                long i__prev1 = i;

                for (long i = 1L; i < maxp1; i++)
                {
                    var len = p.parseInt();
                    typeOffsets = append(typeOffsets, new typeOffset(total,len));
                    total += len;
                }


                i = i__prev1;
            }

            defer(w =>
            {
                p.scanner.Whitespace = w;
            }(p.scanner.Whitespace));
            p.scanner.Whitespace = 0L; 

            // We should now have p.tok pointing to the final newline.
            // The next runes from the scanner should be the type data.

            strings.Builder sb = default;
            while (sb.Len() < total)
            {
                var r = p.scanner.Next();
                if (r == scanner.EOF)
                {
                    p.error("unexpected EOF");
                }

                sb.WriteRune(r);

            }

            var allTypeData = sb.String();

            p.typeData = new slice<@string>(new @string[] { "" }); // type 0, unused
            foreach (var (_, to) in typeOffsets)
            {
                p.typeData = append(p.typeData, allTypeData[to.offset..to.offset + to.length]);
            }
            {
                long i__prev1 = i;

                for (i = 1L; i < int(exportedp1); i++)
                {
                    p.parseSavedType(pkg, i, null);
                }


                i = i__prev1;
            }

        });

        // parseSavedType parses one saved type definition.
        private static void parseSavedType(this ptr<parser> _addr_p, ptr<types.Package> _addr_pkg, long i, slice<object> nlist) => func((defer, _, __) =>
        {
            ref parser p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            defer((s, tok, lit) =>
            {
                p.scanner = s;
                p.tok = tok;
                p.lit = lit;
            }(p.scanner, p.tok, p.lit));

            p.scanner = @new<scanner.Scanner>();
            p.initScanner(p.scanner.Filename, strings.NewReader(p.typeData[i]));
            p.expectKeyword("type");
            var id = p.parseInt();
            if (id != i)
            {
                p.errorf("type ID mismatch: got %d, want %d", id, i);
            }

            if (p.typeList[i] == reserved)
            {
                p.errorf("internal error: %d already reserved in parseSavedType", i);
            }

            if (p.typeList[i] == null)
            {
                p.reserve(i);
                p.parseTypeSpec(pkg, append(nlist, i));
            }

            if (p.typeList[i] == null || p.typeList[i] == reserved)
            {
                p.errorf("internal error: parseSavedType(%d,%v) reserved/nil", i, nlist);
            }

        });

        // PackageInit = unquotedString unquotedString int .
        private static PackageInit parsePackageInit(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            var name = p.parseUnquotedString();
            var initfunc = p.parseUnquotedString();
            long priority = -1L;
            if (p.version == "v1")
            {
                priority = p.parseInt();
            }

            return new PackageInit(Name:name,InitFunc:initfunc,Priority:priority);

        }

        // Create the package if we have parsed both the package path and package name.
        private static void maybeCreatePackage(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            if (p.pkgname != "" && p.pkgpath != "")
            {
                p.pkg = p.getPkg(p.pkgpath, p.pkgname);
            }

        }

        // InitDataDirective = ( "v1" | "v2" | "v3" ) ";" |
        //                     "priority" int ";" |
        //                     "init" { PackageInit } ";" |
        //                     "checksum" unquotedString ";" .
        private static void parseInitDataDirective(this ptr<parser> _addr_p) => func((defer, _, __) =>
        {
            ref parser p = ref _addr_p.val;

            if (p.tok != scanner.Ident)
            { 
                // unexpected token kind; panic
                p.expect(scanner.Ident);

            }

            switch (p.lit)
            {
                case "v1": 

                case "v2": 

                case "v3": 
                    p.version = p.lit;
                    p.next();
                    p.expect(';');
                    p.expect('\n');
                    break;
                case "priority": 
                    p.next();
                    p.initdata.Priority = p.parseInt();
                    p.expectEOL();
                    break;
                case "init": 
                    p.next();
                    while (p.tok != '\n' && p.tok != ';' && p.tok != scanner.EOF)
                    {
                        p.initdata.Inits = append(p.initdata.Inits, p.parsePackageInit());
                    }

                    p.expectEOL();
                    break;
                case "init_graph": 
                    p.next(); 
                    // The graph data is thrown away for now.
                    while (p.tok != '\n' && p.tok != ';' && p.tok != scanner.EOF)
                    {
                        p.parseInt64();
                        p.parseInt64();
                    }

                    p.expectEOL();
                    break;
                case "checksum": 
                    // Don't let the scanner try to parse the checksum as a number.
                    defer(mode =>
                    {
                        p.scanner.Mode = mode;
                    }(p.scanner.Mode));
                    p.scanner.Mode &= scanner.ScanInts | scanner.ScanFloats;
                    p.next();
                    p.parseUnquotedString();
                    p.expectEOL();
                    break;
                default: 
                    p.errorf("unexpected identifier: %q", p.lit);
                    break;
            }

        });

        // Directive = InitDataDirective |
        //             "package" unquotedString [ unquotedString ] [ unquotedString ] ";" |
        //             "pkgpath" unquotedString ";" |
        //             "prefix" unquotedString ";" |
        //             "import" unquotedString unquotedString string ";" |
        //             "indirectimport" unquotedString unquotedstring ";" |
        //             "func" Func ";" |
        //             "type" Type ";" |
        //             "var" Var ";" |
        //             "const" Const ";" .
        private static void parseDirective(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            if (p.tok != scanner.Ident)
            { 
                // unexpected token kind; panic
                p.expect(scanner.Ident);

            }

            switch (p.lit)
            {
                case "v1": 

                case "v2": 

                case "v3": 

                case "priority": 

                case "init": 

                case "init_graph": 

                case "checksum": 
                    p.parseInitDataDirective();
                    break;
                case "package": 
                    p.next();
                    p.pkgname = p.parseUnquotedString();
                    p.maybeCreatePackage();
                    if (p.version != "v1" && p.tok != '\n' && p.tok != ';')
                    {
                        p.parseUnquotedString();
                        p.parseUnquotedString();
                    }

                    p.expectEOL();
                    break;
                case "pkgpath": 
                    p.next();
                    p.pkgpath = p.parseUnquotedString();
                    p.maybeCreatePackage();
                    p.expectEOL();
                    break;
                case "prefix": 
                    p.next();
                    p.pkgpath = p.parseUnquotedString();
                    p.expectEOL();
                    break;
                case "import": 
                    p.next();
                    var pkgname = p.parseUnquotedString();
                    var pkgpath = p.parseUnquotedString();
                    p.getPkg(pkgpath, pkgname);
                    p.parseString();
                    p.expectEOL();
                    break;
                case "indirectimport": 
                    p.next();
                    pkgname = p.parseUnquotedString();
                    pkgpath = p.parseUnquotedString();
                    p.getPkg(pkgpath, pkgname);
                    p.expectEOL();
                    break;
                case "types": 
                    p.next();
                    p.parseTypes(p.pkg);
                    p.expectEOL();
                    break;
                case "func": 
                    p.next();
                    var fun = p.parseFunc(p.pkg);
                    if (fun != null)
                    {
                        p.pkg.Scope().Insert(fun);
                    }

                    p.expectEOL();
                    break;
                case "type": 
                    p.next();
                    p.parseType(p.pkg);
                    p.expectEOL();
                    break;
                case "var": 
                    p.next();
                    var v = p.parseVar(p.pkg);
                    if (v != null)
                    {
                        p.pkg.Scope().Insert(v);
                    }

                    p.expectEOL();
                    break;
                case "const": 
                    p.next();
                    var c = p.parseConst(p.pkg);
                    p.pkg.Scope().Insert(c);
                    p.expectEOL();
                    break;
                default: 
                    p.errorf("unexpected identifier: %q", p.lit);
                    break;
            }

        }

        // Package = { Directive } .
        private static ptr<types.Package> parsePackage(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            while (p.tok != scanner.EOF)
            {
                p.parseDirective();
            }

            foreach (var (_, f) in p.fixups)
            {
                if (f.target.Underlying() == null)
                {
                    p.errorf("internal error: fixup can't be applied, loop required");
                }

                f.toUpdate.SetUnderlying(f.target.Underlying());

            }
            p.fixups = null;
            foreach (var (_, typ) in p.typeList)
            {
                {
                    ptr<types.Interface> (it, ok) = typ._<ptr<types.Interface>>();

                    if (ok)
                    {
                        it.Complete();
                    }

                }

            }
            p.pkg.MarkComplete();
            return _addr_p.pkg!;

        }
    }
}}}
