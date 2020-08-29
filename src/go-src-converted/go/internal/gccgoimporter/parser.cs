// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gccgoimporter -- go2cs converted at 2020 August 29 10:09:23 UTC
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
            public scanner.Scanner scanner;
            public @string version; // format version
            public int tok; // current token
            public @string lit; // literal string; only valid for Ident, Int, String tokens
            public @string pkgpath; // package path of imported package
            public @string pkgname; // name of imported package
            public ptr<types.Package> pkg; // reference to imported package
            public map<@string, ref types.Package> imports; // package path -> package object
            public map<long, types.Type> typeMap; // type number -> type
            public InitData initdata; // package init priority data
        }

        private static void init(this ref parser p, @string filename, io.Reader src, map<@string, ref types.Package> imports)
        {
            p.scanner.Init(src);
            p.scanner.Error = (_, msg) =>
            {
                p.error(msg);

            }
;
            p.scanner.Mode = scanner.ScanIdents | scanner.ScanInts | scanner.ScanFloats | scanner.ScanStrings | scanner.ScanComments | scanner.SkipComments;
            p.scanner.Whitespace = 1L << (int)('\t') | 1L << (int)('\n') | 1L << (int)(' ');
            p.scanner.Filename = filename; // for good error messages
            p.next();
            p.imports = imports;
            p.typeMap = make_map<long, types.Type>();
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

        private static void error(this ref parser _p, object err) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
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

        private static void errorf(this ref parser p, @string format, params object[] args)
        {
            p.error(fmt.Errorf(format, args));
        }

        private static @string expect(this ref parser p, int tok)
        {
            var lit = p.lit;
            if (p.tok != tok)
            {
                p.errorf("expected %s, got %s (%s)", scanner.TokenString(tok), scanner.TokenString(p.tok), lit);
            }
            p.next();
            return lit;
        }

        private static void expectKeyword(this ref parser p, @string keyword)
        {
            var lit = p.expect(scanner.Ident);
            if (lit != keyword)
            {
                p.errorf("expected keyword %s, got %q", keyword, lit);
            }
        }

        private static @string parseString(this ref parser p)
        {
            var (str, err) = strconv.Unquote(p.expect(scanner.String));
            if (err != null)
            {
                p.error(err);
            }
            return str;
        }

        // unquotedString     = { unquotedStringChar } .
        // unquotedStringChar = <neither a whitespace nor a ';' char> .
        private static @string parseUnquotedString(this ref parser p)
        {
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

                while (ch != ';' && ch != scanner.EOF && p.scanner.Whitespace & (1L << (int)(uint(ch))) == 0L)
                {
                    buf.WriteRune(ch);
                    p.scanner.Next();
                    ch = p.scanner.Peek();
                }

            }
            p.next();
            return buf.String();
        }

        private static void next(this ref parser p)
        {
            p.tok = p.scanner.Scan();

            if (p.tok == scanner.Ident || p.tok == scanner.Int || p.tok == scanner.Float || p.tok == scanner.String || p.tok == '·') 
                p.lit = p.scanner.TokenText();
            else 
                p.lit = "";
                    }

        private static (@string, @string) parseQualifiedName(this ref parser p)
        {
            return p.parseQualifiedNameStr(p.parseString());
        }

        private static (@string, @string) parseUnquotedQualifiedName(this ref parser p)
        {
            return p.parseQualifiedNameStr(p.parseUnquotedString());
        }

        // qualifiedName = [ ["."] unquotedString "." ] unquotedString .
        //
        // The above production uses greedy matching.
        private static (@string, @string) parseQualifiedNameStr(this ref parser p, @string unquotedName)
        {
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

            return;
        }

        // getPkg returns the package for a given path. If the package is
        // not found but we have a package name, create the package and
        // add it to the p.imports map.
        //
        private static ref types.Package getPkg(this ref parser p, @string pkgpath, @string name)
        { 
            // package unsafe is not in the imports map - handle explicitly
            if (pkgpath == "unsafe")
            {
                return types.Unsafe;
            }
            var pkg = p.imports[pkgpath];
            if (pkg == null && name != "")
            {
                pkg = types.NewPackage(pkgpath, name);
                p.imports[pkgpath] = pkg;
            }
            return pkg;
        }

        // parseExportedName is like parseQualifiedName, but
        // the package path is resolved to an imported *types.Package.
        //
        // ExportedName = string [string] .
        private static (ref types.Package, @string) parseExportedName(this ref parser p)
        {
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
            return;
        }

        // Name = QualifiedName | "?" .
        private static @string parseName(this ref parser p)
        {
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
                ref types.Pointer (p, _) = typ._<ref types.Pointer>();

                if (p != null)
                {
                    typ = p.Elem();
                }

            }
            return typ;
        }

        // Field = Name Type [string] .
        private static (ref types.Var, @string) parseField(this ref parser p, ref types.Package pkg)
        {
            var name = p.parseName();
            var typ = p.parseType(pkg);
            var anon = false;
            if (name == "")
            {
                anon = true;
                switch (deref(typ).type())
                {
                    case ref types.Basic typ:
                        name = typ.Name();
                        break;
                    case ref types.Named typ:
                        name = typ.Obj().Name();
                        break;
                    default:
                    {
                        var typ = deref(typ).type();
                        p.error("anonymous field expected");
                        break;
                    }
                }
            }
            field = types.NewField(token.NoPos, pkg, name, typ, anon);
            if (p.tok == scanner.String)
            {
                tag = p.parseString();
            }
            return;
        }

        // Param = Name ["..."] Type .
        private static (ref types.Var, bool) parseParam(this ref parser p, ref types.Package pkg)
        {
            var name = p.parseName();
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
            return;
        }

        // Var = Name Type .
        private static ref types.Var parseVar(this ref parser p, ref types.Package pkg)
        {
            var name = p.parseName();
            return types.NewVar(token.NoPos, pkg, name, p.parseType(pkg));
        }

        // Conversion = "convert" "(" Type "," ConstValue ")" .
        private static (constant.Value, types.Type) parseConversion(this ref parser p, ref types.Package pkg)
        {
            p.expectKeyword("convert");
            p.expect('(');
            typ = p.parseType(pkg);
            p.expect(',');
            val, _ = p.parseConstValue(pkg);
            p.expect(')');
            return;
        }

        // ConstValue     = string | "false" | "true" | ["-"] (int ["'"] | FloatOrComplex) | Conversion .
        // FloatOrComplex = float ["i" | ("+"|"-") float "i"] .
        private static (constant.Value, types.Type) parseConstValue(this ref parser p, ref types.Package pkg)
        {

            if (p.tok == scanner.String) 
                var str = p.parseString();
                val = constant.MakeString(str);
                typ = types.Typ[types.UntypedString];
                return;
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
                return;
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
                    return;
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
                        return;
        }

        // Const = Name [Type] "=" ConstValue .
        private static ref types.Const parseConst(this ref parser p, ref types.Package pkg)
        {
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
            return types.NewConst(token.NoPos, pkg, name, typ, val);
        }

        // NamedType = TypeName [ "=" ] Type { Method } .
        // TypeName  = ExportedName .
        // Method    = "func" "(" Param ")" Name ParamList ResultList ";" .
        private static types.Type parseNamedType(this ref parser p, long n)
        {
            var (pkg, name) = p.parseExportedName();
            var scope = pkg.Scope();

            if (p.tok == '=')
            { 
                // type alias
                p.next();
                var typ = p.parseType(pkg);
                {
                    var obj__prev2 = obj;

                    var obj = scope.Lookup(name);

                    if (obj != null)
                    {
                        typ = obj.Type(); // use previously imported type
                        if (typ == null)
                        {
                            p.errorf("%v (type alias) used in cycle", obj);
                        }
                    }
                    else
                    {
                        obj = types.NewTypeName(token.NoPos, pkg, name, typ);
                        scope.Insert(obj);
                    }

                    obj = obj__prev2;

                }
                p.typeMap[n] = typ;
                return typ;
            } 

            // named type
            obj = scope.Lookup(name);
            if (obj == null)
            { 
                // a named type may be referred to before the underlying type
                // is known - set it up
                var tname = types.NewTypeName(token.NoPos, pkg, name, null);
                types.NewNamed(tname, null, null);
                scope.Insert(tname);
                obj = tname;
            }
            typ = obj.Type();
            p.typeMap[n] = typ;

            ref types.Named (nt, ok) = typ._<ref types.Named>();
            if (!ok)
            { 
                // This can happen for unsafe.Pointer, which is a TypeName holding a Basic type.
                var pt = p.parseType(pkg);
                if (pt != typ)
                {
                    p.error("unexpected underlying type for non-named TypeName");
                }
                return typ;
            }
            var underlying = p.parseType(pkg);
            if (nt.Underlying() == null)
            {
                nt.SetUnderlying(underlying.Underlying());
            } 

            // collect associated methods
            while (p.tok == scanner.Ident)
            {
                p.expectKeyword("func");
                p.expect('(');
                var (receiver, _) = p.parseParam(pkg);
                p.expect(')');
                var name = p.parseName();
                var (params, isVariadic) = p.parseParamList(pkg);
                var results = p.parseResultList(pkg);
                p.expect(';');

                var sig = types.NewSignature(receiver, params, results, isVariadic);
                nt.AddMethod(types.NewFunc(token.NoPos, pkg, name, sig));
            }


            return nt;
        }

        private static long parseInt(this ref parser p)
        {
            var lit = p.expect(scanner.Int);
            var (n, err) = strconv.ParseInt(lit, 10L, 0L);
            if (err != null)
            {
                p.error(err);
            }
            return n;
        }

        // ArrayOrSliceType = "[" [ int ] "]" Type .
        private static types.Type parseArrayOrSliceType(this ref parser p, ref types.Package pkg)
        {
            p.expect('[');
            if (p.tok == ']')
            {
                p.next();
                return types.NewSlice(p.parseType(pkg));
            }
            var n = p.parseInt();
            p.expect(']');
            return types.NewArray(p.parseType(pkg), n);
        }

        // MapType = "map" "[" Type "]" Type .
        private static types.Type parseMapType(this ref parser p, ref types.Package pkg)
        {
            p.expectKeyword("map");
            p.expect('[');
            var key = p.parseType(pkg);
            p.expect(']');
            var elem = p.parseType(pkg);
            return types.NewMap(key, elem);
        }

        // ChanType = "chan" ["<-" | "-<"] Type .
        private static types.Type parseChanType(this ref parser p, ref types.Package pkg)
        {
            p.expectKeyword("chan");
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

            return types.NewChan(dir, p.parseType(pkg));
        }

        // StructType = "struct" "{" { Field } "}" .
        private static types.Type parseStructType(this ref parser p, ref types.Package pkg)
        {
            p.expectKeyword("struct");

            slice<ref types.Var> fields = default;
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

            return types.NewStruct(fields, tags);
        }

        // ParamList = "(" [ { Parameter "," } Parameter ] ")" .
        private static (ref types.Tuple, bool) parseParamList(this ref parser p, ref types.Package pkg)
        {
            slice<ref types.Var> list = default;
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

            return (types.NewTuple(list), isVariadic);
        }

        // ResultList = Type | ParamList .
        private static ref types.Tuple parseResultList(this ref parser p, ref types.Package pkg)
        {
            switch (p.tok)
            {
                case '<': 
                    return types.NewTuple(types.NewParam(token.NoPos, pkg, "", p.parseType(pkg)));
                    break;
                case '(': 
                    var (params, _) = p.parseParamList(pkg);
                    return params;
                    break;
                default: 
                    return null;
                    break;
            }
        }

        // FunctionType = ParamList ResultList .
        private static ref types.Signature parseFunctionType(this ref parser p, ref types.Package pkg)
        {
            var (params, isVariadic) = p.parseParamList(pkg);
            var results = p.parseResultList(pkg);
            return types.NewSignature(null, params, results, isVariadic);
        }

        // Func = Name FunctionType .
        private static ref types.Func parseFunc(this ref parser p, ref types.Package pkg)
        {
            var name = p.parseName();
            if (strings.ContainsRune(name, '$'))
            { 
                // This is a Type$equal or Type$hash function, which we don't want to parse,
                // except for the types.
                p.discardDirectiveWhileParsingTypes(pkg);
                return null;
            }
            return types.NewFunc(token.NoPos, pkg, name, p.parseFunctionType(pkg));
        }

        // InterfaceType = "interface" "{" { ("?" Type | Func) ";" } "}" .
        private static types.Type parseInterfaceType(this ref parser p, ref types.Package pkg)
        {
            p.expectKeyword("interface");

            slice<ref types.Func> methods = default;
            slice<ref types.Named> typs = default;

            p.expect('{');
            while (p.tok != '}' && p.tok != scanner.EOF)
            {
                if (p.tok == '?')
                {
                    p.next();
                    typs = append(typs, p.parseType(pkg)._<ref types.Named>());
                }
                else
                {
                    var method = p.parseFunc(pkg);
                    methods = append(methods, method);
                }
                p.expect(';');
            }

            p.expect('}');

            return types.NewInterface(methods, typs);
        }

        // PointerType = "*" ("any" | Type) .
        private static types.Type parsePointerType(this ref parser p, ref types.Package pkg)
        {
            p.expect('*');
            if (p.tok == scanner.Ident)
            {
                p.expectKeyword("any");
                return types.Typ[types.UnsafePointer];
            }
            return types.NewPointer(p.parseType(pkg));
        }

        // TypeDefinition = NamedType | MapType | ChanType | StructType | InterfaceType | PointerType | ArrayOrSliceType | FunctionType .
        private static types.Type parseTypeDefinition(this ref parser p, ref types.Package pkg, long n)
        {
            types.Type t = default;

            if (p.tok == scanner.String) 
                t = p.parseNamedType(n);
            else if (p.tok == scanner.Ident) 
                switch (p.lit)
                {
                    case "map": 
                        t = p.parseMapType(pkg);
                        break;
                    case "chan": 
                        t = p.parseChanType(pkg);
                        break;
                    case "struct": 
                        t = p.parseStructType(pkg);
                        break;
                    case "interface": 
                        t = p.parseInterfaceType(pkg);
                        break;
                }
            else if (p.tok == '*') 
                t = p.parsePointerType(pkg);
            else if (p.tok == '[') 
                t = p.parseArrayOrSliceType(pkg);
            else if (p.tok == '(') 
                t = p.parseFunctionType(pkg);
                        p.typeMap[n] = t;
            return t;
        }

 
        // From gofrontend/go/export.h
        // Note that these values are negative in the gofrontend and have been made positive
        // in the gccgoimporter.
        private static readonly long gccgoBuiltinINT8 = 1L;
        private static readonly long gccgoBuiltinINT16 = 2L;
        private static readonly long gccgoBuiltinINT32 = 3L;
        private static readonly long gccgoBuiltinINT64 = 4L;
        private static readonly long gccgoBuiltinUINT8 = 5L;
        private static readonly long gccgoBuiltinUINT16 = 6L;
        private static readonly long gccgoBuiltinUINT32 = 7L;
        private static readonly long gccgoBuiltinUINT64 = 8L;
        private static readonly long gccgoBuiltinFLOAT32 = 9L;
        private static readonly long gccgoBuiltinFLOAT64 = 10L;
        private static readonly long gccgoBuiltinINT = 11L;
        private static readonly long gccgoBuiltinUINT = 12L;
        private static readonly long gccgoBuiltinUINTPTR = 13L;
        private static readonly long gccgoBuiltinBOOL = 15L;
        private static readonly long gccgoBuiltinSTRING = 16L;
        private static readonly long gccgoBuiltinCOMPLEX64 = 17L;
        private static readonly long gccgoBuiltinCOMPLEX128 = 18L;
        private static readonly long gccgoBuiltinERROR = 19L;
        private static readonly long gccgoBuiltinBYTE = 20L;
        private static readonly long gccgoBuiltinRUNE = 21L;

        private static types.Type lookupBuiltinType(long typ)
        {
            return new array<types.Type>(InitKeyedValues<types.Type>((gccgoBuiltinINT8, types.Typ[types.Int8]), (gccgoBuiltinINT16, types.Typ[types.Int16]), (gccgoBuiltinINT32, types.Typ[types.Int32]), (gccgoBuiltinINT64, types.Typ[types.Int64]), (gccgoBuiltinUINT8, types.Typ[types.Uint8]), (gccgoBuiltinUINT16, types.Typ[types.Uint16]), (gccgoBuiltinUINT32, types.Typ[types.Uint32]), (gccgoBuiltinUINT64, types.Typ[types.Uint64]), (gccgoBuiltinFLOAT32, types.Typ[types.Float32]), (gccgoBuiltinFLOAT64, types.Typ[types.Float64]), (gccgoBuiltinINT, types.Typ[types.Int]), (gccgoBuiltinUINT, types.Typ[types.Uint]), (gccgoBuiltinUINTPTR, types.Typ[types.Uintptr]), (gccgoBuiltinBOOL, types.Typ[types.Bool]), (gccgoBuiltinSTRING, types.Typ[types.String]), (gccgoBuiltinCOMPLEX64, types.Typ[types.Complex64]), (gccgoBuiltinCOMPLEX128, types.Typ[types.Complex128]), (gccgoBuiltinERROR, types.Universe.Lookup("error").Type()), (gccgoBuiltinBYTE, types.Universe.Lookup("byte").Type()), (gccgoBuiltinRUNE, types.Universe.Lookup("rune").Type())))[typ];
        }

        // Type = "<" "type" ( "-" int | int [ TypeDefinition ] ) ">" .
        private static types.Type parseType(this ref parser p, ref types.Package pkg)
        {
            p.expect('<');
            p.expectKeyword("type");


            if (p.tok == scanner.Int) 
                var n = p.parseInt();

                if (p.tok == '>')
                {
                    t = p.typeMap[int(n)];
                }
                else
                {
                    t = p.parseTypeDefinition(pkg, int(n));
                }
            else if (p.tok == '-') 
                p.next();
                n = p.parseInt();
                t = lookupBuiltinType(int(n));
            else 
                p.errorf("expected type number, got %s (%q)", scanner.TokenString(p.tok), p.lit);
                return null;
                        p.expect('>');
            return;
        }

        // PackageInit = unquotedString unquotedString int .
        private static PackageInit parsePackageInit(this ref parser p)
        {
            var name = p.parseUnquotedString();
            var initfunc = p.parseUnquotedString();
            long priority = -1L;
            if (p.version == "v1")
            {
                priority = int(p.parseInt());
            }
            return new PackageInit(Name:name,InitFunc:initfunc,Priority:priority);
        }

        // Throw away tokens until we see a ';'. If we see a '<', attempt to parse as a type.
        private static void discardDirectiveWhileParsingTypes(this ref parser p, ref types.Package pkg)
        {
            while (true)
            {

                if (p.tok == ';') 
                    return;
                else if (p.tok == '<') 
                    p.parseType(pkg);
                else if (p.tok == scanner.EOF) 
                    p.error("unexpected EOF");
                else 
                    p.next();
                            }

        }

        // Create the package if we have parsed both the package path and package name.
        private static void maybeCreatePackage(this ref parser p)
        {
            if (p.pkgname != "" && p.pkgpath != "")
            {
                p.pkg = p.getPkg(p.pkgpath, p.pkgname);
            }
        }

        // InitDataDirective = ( "v1" | "v2" ) ";" |
        //                     "priority" int ";" |
        //                     "init" { PackageInit } ";" |
        //                     "checksum" unquotedString ";" .
        private static void parseInitDataDirective(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.tok != scanner.Ident)
            { 
                // unexpected token kind; panic
                p.expect(scanner.Ident);
            }
            switch (p.lit)
            {
                case "v1": 

                case "v2": 
                    p.version = p.lit;
                    p.next();
                    p.expect(';');
                    break;
                case "priority": 
                    p.next();
                    p.initdata.Priority = int(p.parseInt());
                    p.expect(';');
                    break;
                case "init": 
                    p.next();
                    while (p.tok != ';' && p.tok != scanner.EOF)
                    {
                        p.initdata.Inits = append(p.initdata.Inits, p.parsePackageInit());
                    }

                    p.expect(';');
                    break;
                case "init_graph": 
                    p.next(); 
                    // The graph data is thrown away for now.
                    while (p.tok != ';' && p.tok != scanner.EOF)
                    {
                        p.parseInt();
                        p.parseInt();
                    }

                    p.expect(';');
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
                    p.expect(';');
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
        //             "func" Func ";" |
        //             "type" Type ";" |
        //             "var" Var ";" |
        //             "const" Const ";" .
        private static void parseDirective(this ref parser p)
        {
            if (p.tok != scanner.Ident)
            { 
                // unexpected token kind; panic
                p.expect(scanner.Ident);
            }
            switch (p.lit)
            {
                case "v1": 

                case "v2": 

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
                    if (p.version == "v2" && p.tok != ';')
                    {
                        p.parseUnquotedString();
                        p.parseUnquotedString();
                    }
                    p.expect(';');
                    break;
                case "pkgpath": 
                    p.next();
                    p.pkgpath = p.parseUnquotedString();
                    p.maybeCreatePackage();
                    p.expect(';');
                    break;
                case "prefix": 
                    p.next();
                    p.pkgpath = p.parseUnquotedString();
                    p.expect(';');
                    break;
                case "import": 
                    p.next();
                    var pkgname = p.parseUnquotedString();
                    var pkgpath = p.parseUnquotedString();
                    p.getPkg(pkgpath, pkgname);
                    p.parseString();
                    p.expect(';');
                    break;
                case "func": 
                    p.next();
                    var fun = p.parseFunc(p.pkg);
                    if (fun != null)
                    {
                        p.pkg.Scope().Insert(fun);
                    }
                    p.expect(';');
                    break;
                case "type": 
                    p.next();
                    p.parseType(p.pkg);
                    p.expect(';');
                    break;
                case "var": 
                    p.next();
                    var v = p.parseVar(p.pkg);
                    p.pkg.Scope().Insert(v);
                    p.expect(';');
                    break;
                case "const": 
                    p.next();
                    var c = p.parseConst(p.pkg);
                    p.pkg.Scope().Insert(c);
                    p.expect(';');
                    break;
                default: 
                    p.errorf("unexpected identifier: %q", p.lit);
                    break;
            }
        }

        // Package = { Directive } .
        private static ref types.Package parsePackage(this ref parser p)
        {
            while (p.tok != scanner.EOF)
            {
                p.parseDirective();
            }

            foreach (var (_, typ) in p.typeMap)
            {
                {
                    ref types.Interface (it, ok) = typ._<ref types.Interface>();

                    if (ok)
                    {
                        it.Complete();
                    }

                }
            }
            p.pkg.MarkComplete();
            return p.pkg;
        }

        // InitData = { InitDataDirective } .
        private static void parseInitData(this ref parser p)
        {
            while (p.tok != scanner.EOF)
            {
                p.parseInitDataDirective();
            }

        }
    }
}}}
