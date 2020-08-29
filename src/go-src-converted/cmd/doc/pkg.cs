// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:00:08 UTC
// Original source: C:\Go\src\cmd\doc\pkg.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using doc = go.go.doc_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly long punchedCardWidth = 80L; // These things just won't leave us alone.
        private static readonly var indentedWidth = punchedCardWidth - len(indent);
        private static readonly @string indent = "    ";

        public partial struct Package
        {
            public io.Writer writer; // Destination for output.
            public @string name; // Package name, json for encoding/json.
            public @string userPath; // String the user used to find this package.
            public ptr<ast.Package> pkg; // Parsed package.
            public ptr<ast.File> file; // Merged from all files in the package
            public ptr<doc.Package> doc;
            public ptr<build.Package> build;
            public ptr<token.FileSet> fs; // Needed for printing.
            public bytes.Buffer buf;
        }

        public partial struct PackageError // : @string
        {
        } // type returned by pkg.Fatalf.

        public static @string Error(this PackageError p)
        {
            return string(p);
        }

        // prettyPath returns a version of the package path that is suitable for an
        // error message. It obeys the import comment if present. Also, since
        // pkg.build.ImportPath is sometimes the unhelpful "" or ".", it looks for a
        // directory name in GOROOT or GOPATH if that happens.
        private static @string prettyPath(this ref Package pkg)
        {
            var path = pkg.build.ImportComment;
            if (path == "")
            {
                path = pkg.build.ImportPath;
            }
            if (path != "." && path != "")
            {
                return path;
            } 
            // Convert the source directory into a more useful path.
            // Also convert everything to slash-separated paths for uniform handling.
            path = filepath.Clean(filepath.ToSlash(pkg.build.Dir)); 
            // Can we find a decent prefix?
            var goroot = filepath.Join(build.Default.GOROOT, "src");
            {
                var p__prev1 = p;

                var (p, ok) = trim(path, filepath.ToSlash(goroot));

                if (ok)
                {
                    return p;
                }

                p = p__prev1;

            }
            foreach (var (_, gopath) in splitGopath())
            {
                {
                    var p__prev1 = p;

                    (p, ok) = trim(path, filepath.ToSlash(gopath));

                    if (ok)
                    {
                        return p;
                    }

                    p = p__prev1;

                }
            }
            return path;
        }

        // trim trims the directory prefix from the path, paying attention
        // to the path separator. If they are the same string or the prefix
        // is not present the original is returned. The boolean reports whether
        // the prefix is present. That path and prefix have slashes for separators.
        private static (@string, bool) trim(@string path, @string prefix)
        {
            if (!strings.HasPrefix(path, prefix))
            {
                return (path, false);
            }
            if (path == prefix)
            {
                return (path, true);
            }
            if (path[len(prefix)] == '/')
            {
                return (path[len(prefix) + 1L..], true);
            }
            return (path, false); // Textual prefix but not a path prefix.
        }

        // pkg.Fatalf is like log.Fatalf, but panics so it can be recovered in the
        // main do function, so it doesn't cause an exit. Allows testing to work
        // without running a subprocess. The log prefix will be added when
        // logged in main; it is not added here.
        private static void Fatalf(this ref Package _pkg, @string format, params object[] args) => func(_pkg, (ref Package pkg, Defer _, Panic panic, Recover __) =>
        {
            panic(PackageError(fmt.Sprintf(format, args)));
        });

        // parsePackage turns the build package we found into a parsed package
        // we can then use to generate documentation.
        private static ref Package parsePackage(io.Writer writer, ref build.Package pkg, @string userPath)
        {
            var fs = token.NewFileSet(); 
            // include tells parser.ParseDir which files to include.
            // That means the file must be in the build package's GoFiles or CgoFiles
            // list only (no tag-ignored files, tests, swig or other non-Go files).
            Func<os.FileInfo, bool> include = info =>
            {
                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in pkg.GoFiles)
                    {
                        name = __name;
                        if (name == info.Name())
                        {
                            return true;
                        }
                    }

                    name = name__prev1;
                }

                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in pkg.CgoFiles)
                    {
                        name = __name;
                        if (name == info.Name())
                        {
                            return true;
                        }
                    }

                    name = name__prev1;
                }

                return false;
            }
;
            var (pkgs, err) = parser.ParseDir(fs, pkg.Dir, include, parser.ParseComments);
            if (err != null)
            {
                log.Fatal(err);
            } 
            // Make sure they are all in one package.
            if (len(pkgs) != 1L)
            {
                log.Fatalf("multiple packages in directory %s", pkg.Dir);
            }
            var astPkg = pkgs[pkg.Name]; 

            // TODO: go/doc does not include typed constants in the constants
            // list, which is what we want. For instance, time.Sunday is of type
            // time.Weekday, so it is defined in the type but not in the
            // Consts list for the package. This prevents
            //    go doc time.Sunday
            // from finding the symbol. Work around this for now, but we
            // should fix it in go/doc.
            // A similar story applies to factory functions.
            var docPkg = doc.New(astPkg, pkg.ImportPath, doc.AllDecls);
            foreach (var (_, typ) in docPkg.Types)
            {
                docPkg.Consts = append(docPkg.Consts, typ.Consts);
                docPkg.Vars = append(docPkg.Vars, typ.Vars);
                docPkg.Funcs = append(docPkg.Funcs, typ.Funcs);
            }
            return ref new Package(writer:writer,name:pkg.Name,userPath:userPath,pkg:astPkg,file:ast.MergePackageFiles(astPkg,0),doc:docPkg,build:pkg,fs:fs,);
        }

        private static void Printf(this ref Package pkg, @string format, params object[] args)
        {
            fmt.Fprintf(ref pkg.buf, format, args);
        }

        private static void flush(this ref Package pkg)
        {
            var (_, err) = pkg.writer.Write(pkg.buf.Bytes());
            if (err != null)
            {
                log.Fatal(err);
            }
            pkg.buf.Reset(); // Not needed, but it's a flush.
        }

        private static slice<byte> newlineBytes = (slice<byte>)"\n\n"; // We never ask for more than 2.

        // newlines guarantees there are n newlines at the end of the buffer.
        private static void newlines(this ref Package pkg, long n)
        {
            while (!bytes.HasSuffix(pkg.buf.Bytes(), newlineBytes[..n]))
            {
                pkg.buf.WriteRune('\n');
            }

        }

        // emit prints the node.
        private static void emit(this ref Package pkg, @string comment, ast.Node node)
        {
            if (node != null)
            {
                var err = format.Node(ref pkg.buf, pkg.fs, node);
                if (err != null)
                {
                    log.Fatal(err);
                }
                if (comment != "")
                {
                    pkg.newlines(1L);
                    doc.ToText(ref pkg.buf, comment, "    ", indent, indentedWidth);
                    pkg.newlines(2L); // Blank line after comment to separate from next item.
                }
                else
                {
                    pkg.newlines(1L);
                }
            }
        }

        // oneLineNode returns a one-line summary of the given input node.
        private static @string oneLineNode(this ref Package pkg, ast.Node node)
        {
            const long maxDepth = 10L;

            return pkg.oneLineNodeDepth(node, maxDepth);
        }

        // oneLineNodeDepth returns a one-line summary of the given input node.
        // The depth specifies the maximum depth when traversing the AST.
        private static @string oneLineNodeDepth(this ref Package pkg, ast.Node node, long depth)
        {
            const @string dotDotDot = "...";

            if (depth == 0L)
            {
                return dotDotDot;
            }
            depth--;

            switch (node.type())
            {
                case 
                    return "";
                    break;
                case ref ast.GenDecl n:
                    @string trailer = "";
                    if (len(n.Specs) > 1L)
                    {
                        trailer = " " + dotDotDot;
                    } 

                    // Find the first relevant spec.
                    @string typ = "";
                    foreach (var (i, spec) in n.Specs)
                    {
                        ref ast.ValueSpec valueSpec = spec._<ref ast.ValueSpec>(); // Must succeed; we can't mix types in one GenDecl.

                        // The type name may carry over from a previous specification in the
                        // case of constants and iota.
                        if (valueSpec.Type != null)
                        {
                            typ = fmt.Sprintf(" %s", pkg.oneLineNodeDepth(valueSpec.Type, depth));
                        }
                        else if (len(valueSpec.Values) > 0L)
                        {
                            typ = "";
                        }
                        if (!isExported(valueSpec.Names[0L].Name))
                        {
                            continue;
                        }
                        @string val = "";
                        if (i < len(valueSpec.Values) && valueSpec.Values[i] != null)
                        {
                            val = fmt.Sprintf(" = %s", pkg.oneLineNodeDepth(valueSpec.Values[i], depth));
                        }
                        return fmt.Sprintf("%s %s%s%s%s", n.Tok, valueSpec.Names[0L], typ, val, trailer);
                    }
                    return "";
                    break;
                case ref ast.FuncDecl n:
                    var name = n.Name.Name;
                    var recv = pkg.oneLineNodeDepth(n.Recv, depth);
                    if (len(recv) > 0L)
                    {
                        recv = "(" + recv + ") ";
                    }
                    var fnc = pkg.oneLineNodeDepth(n.Type, depth);
                    if (strings.Index(fnc, "func") == 0L)
                    {
                        fnc = fnc[4L..];
                    }
                    return fmt.Sprintf("func %s%s%s", recv, name, fnc);
                    break;
                case ref ast.TypeSpec n:
                    @string sep = " ";
                    if (n.Assign.IsValid())
                    {
                        sep = " = ";
                    }
                    return fmt.Sprintf("type %s%s%s", n.Name.Name, sep, pkg.oneLineNodeDepth(n.Type, depth));
                    break;
                case ref ast.FuncType n:
                    slice<@string> @params = default;
                    if (n.Params != null)
                    {
                        {
                            var field__prev1 = field;

                            foreach (var (_, __field) in n.Params.List)
                            {
                                field = __field;
                                params = append(params, pkg.oneLineField(field, depth));
                            }

                            field = field__prev1;
                        }

                    }
                    var needParens = false;
                    slice<@string> results = default;
                    if (n.Results != null)
                    {
                        needParens = needParens || len(n.Results.List) > 1L;
                        {
                            var field__prev1 = field;

                            foreach (var (_, __field) in n.Results.List)
                            {
                                field = __field;
                                needParens = needParens || len(field.Names) > 0L;
                                results = append(results, pkg.oneLineField(field, depth));
                            }

                            field = field__prev1;
                        }

                    }
                    var param = joinStrings(params);
                    if (len(results) == 0L)
                    {
                        return fmt.Sprintf("func(%s)", param);
                    }
                    var result = joinStrings(results);
                    if (!needParens)
                    {
                        return fmt.Sprintf("func(%s) %s", param, result);
                    }
                    return fmt.Sprintf("func(%s) (%s)", param, result);
                    break;
                case ref ast.StructType n:
                    if (n.Fields == null || len(n.Fields.List) == 0L)
                    {
                        return "struct{}";
                    }
                    return "struct{ ... }";
                    break;
                case ref ast.InterfaceType n:
                    if (n.Methods == null || len(n.Methods.List) == 0L)
                    {
                        return "interface{}";
                    }
                    return "interface{ ... }";
                    break;
                case ref ast.FieldList n:
                    if (n == null || len(n.List) == 0L)
                    {
                        return "";
                    }
                    if (len(n.List) == 1L)
                    {
                        return pkg.oneLineField(n.List[0L], depth);
                    }
                    return dotDotDot;
                    break;
                case ref ast.FuncLit n:
                    return pkg.oneLineNodeDepth(n.Type, depth) + " { ... }";
                    break;
                case ref ast.CompositeLit n:
                    typ = pkg.oneLineNodeDepth(n.Type, depth);
                    if (len(n.Elts) == 0L)
                    {
                        return fmt.Sprintf("%s{}", typ);
                    }
                    return fmt.Sprintf("%s{ %s }", typ, dotDotDot);
                    break;
                case ref ast.ArrayType n:
                    var length = pkg.oneLineNodeDepth(n.Len, depth);
                    var element = pkg.oneLineNodeDepth(n.Elt, depth);
                    return fmt.Sprintf("[%s]%s", length, element);
                    break;
                case ref ast.MapType n:
                    var key = pkg.oneLineNodeDepth(n.Key, depth);
                    var value = pkg.oneLineNodeDepth(n.Value, depth);
                    return fmt.Sprintf("map[%s]%s", key, value);
                    break;
                case ref ast.CallExpr n:
                    fnc = pkg.oneLineNodeDepth(n.Fun, depth);
                    slice<@string> args = default;
                    foreach (var (_, arg) in n.Args)
                    {
                        args = append(args, pkg.oneLineNodeDepth(arg, depth));
                    }
                    return fmt.Sprintf("%s(%s)", fnc, joinStrings(args));
                    break;
                case ref ast.UnaryExpr n:
                    return fmt.Sprintf("%s%s", n.Op, pkg.oneLineNodeDepth(n.X, depth));
                    break;
                case ref ast.Ident n:
                    return n.Name;
                    break;
                default:
                {
                    var n = node.type();
                    ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
                    format.Node(buf, pkg.fs, node);
                    var s = buf.String();
                    if (strings.Contains(s, "\n"))
                    {
                        return dotDotDot;
                    }
                    return s;
                    break;
                }
            }
        }

        // oneLineField returns a one-line summary of the field.
        private static @string oneLineField(this ref Package pkg, ref ast.Field field, long depth)
        {
            slice<@string> names = default;
            foreach (var (_, name) in field.Names)
            {
                names = append(names, name.Name);
            }
            if (len(names) == 0L)
            {
                return pkg.oneLineNodeDepth(field.Type, depth);
            }
            return joinStrings(names) + " " + pkg.oneLineNodeDepth(field.Type, depth);
        }

        // joinStrings formats the input as a comma-separated list,
        // but truncates the list at some reasonable length if necessary.
        private static @string joinStrings(slice<@string> ss)
        {
            long n = default;
            foreach (var (i, s) in ss)
            {
                n += len(s) + len(", ");
                if (n > punchedCardWidth)
                {
                    ss = append(ss.slice(-1, i, i), "...");
                    break;
                }
            }
            return strings.Join(ss, ", ");
        }

        // packageDoc prints the docs for the package (package doc plus one-liners of the rest).
        private static void packageDoc(this ref Package _pkg) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            defer(pkg.flush());
            if (pkg.showInternals())
            {
                pkg.packageClause(false);
            }
            doc.ToText(ref pkg.buf, pkg.doc.Doc, "", indent, indentedWidth);
            pkg.newlines(1L);

            if (!pkg.showInternals())
            { 
                // Show only package docs for commands.
                return;
            }
            pkg.newlines(2L); // Guarantee blank line before the components.
            pkg.valueSummary(pkg.doc.Consts, false);
            pkg.valueSummary(pkg.doc.Vars, false);
            pkg.funcSummary(pkg.doc.Funcs, false);
            pkg.typeSummary();
            pkg.bugs();
        });

        // showInternals reports whether we should show the internals
        // of a package as opposed to just the package docs.
        // Used to decide whether to suppress internals for commands.
        // Called only by Package.packageDoc.
        private static bool showInternals(this ref Package pkg)
        {
            return pkg.pkg.Name != "main" || showCmd;
        }

        // packageClause prints the package clause.
        // The argument boolean, if true, suppresses the output if the
        // user's argument is identical to the actual package path or
        // is empty, meaning it's the current directory.
        private static void packageClause(this ref Package pkg, bool checkUserPath)
        {
            if (checkUserPath)
            {
                if (pkg.userPath == "" || pkg.userPath == pkg.build.ImportPath)
                {
                    return;
                }
            }
            var importPath = pkg.build.ImportComment;
            if (importPath == "")
            {
                importPath = pkg.build.ImportPath;
            }
            pkg.Printf("package %s // import %q\n\n", pkg.name, importPath);
            if (importPath != pkg.build.ImportPath)
            {
                pkg.Printf("WARNING: package source is installed in %q\n", pkg.build.ImportPath);
            }
        }

        // valueSummary prints a one-line summary for each set of values and constants.
        // If all the types in a constant or variable declaration belong to the same
        // type they can be printed by typeSummary, and so can be suppressed here.
        private static void valueSummary(this ref Package pkg, slice<ref doc.Value> values, bool showGrouped)
        {
            map<ref doc.Value, bool> isGrouped = default;
            if (!showGrouped)
            {
                isGrouped = make_map<ref doc.Value, bool>();
                foreach (var (_, typ) in pkg.doc.Types)
                {
                    if (!isExported(typ.Name))
                    {
                        continue;
                    }
                    foreach (var (_, c) in typ.Consts)
                    {
                        isGrouped[c] = true;
                    }
                    foreach (var (_, v) in typ.Vars)
                    {
                        isGrouped[v] = true;
                    }
                }
            }
            foreach (var (_, value) in values)
            {
                if (!isGrouped[value])
                {
                    {
                        var decl = pkg.oneLineNode(value.Decl);

                        if (decl != "")
                        {
                            pkg.Printf("%s\n", decl);
                        }

                    }
                }
            }
        }

        // funcSummary prints a one-line summary for each function. Constructors
        // are printed by typeSummary, below, and so can be suppressed here.
        private static void funcSummary(this ref Package pkg, slice<ref doc.Func> funcs, bool showConstructors)
        { 
            // First, identify the constructors. Don't bother figuring out if they're exported.
            map<ref doc.Func, bool> isConstructor = default;
            if (!showConstructors)
            {
                isConstructor = make_map<ref doc.Func, bool>();
                foreach (var (_, typ) in pkg.doc.Types)
                {
                    if (isExported(typ.Name))
                    {
                        foreach (var (_, f) in typ.Funcs)
                        {
                            isConstructor[f] = true;
                        }
                    }
                }
            }
            foreach (var (_, fun) in funcs)
            { 
                // Exported functions only. The go/doc package does not include methods here.
                if (isExported(fun.Name))
                {
                    if (!isConstructor[fun])
                    {
                        pkg.Printf("%s\n", pkg.oneLineNode(fun.Decl));
                    }
                }
            }
        }

        // typeSummary prints a one-line summary for each type, followed by its constructors.
        private static void typeSummary(this ref Package pkg)
        {
            foreach (var (_, typ) in pkg.doc.Types)
            {
                foreach (var (_, spec) in typ.Decl.Specs)
                {
                    ref ast.TypeSpec typeSpec = spec._<ref ast.TypeSpec>(); // Must succeed.
                    if (isExported(typeSpec.Name.Name))
                    {
                        pkg.Printf("%s\n", pkg.oneLineNode(typeSpec)); 
                        // Now print the consts, vars, and constructors.
                        foreach (var (_, c) in typ.Consts)
                        {
                            {
                                var decl__prev2 = decl;

                                var decl = pkg.oneLineNode(c.Decl);

                                if (decl != "")
                                {
                                    pkg.Printf(indent + "%s\n", decl);
                                }

                                decl = decl__prev2;

                            }
                        }
                        foreach (var (_, v) in typ.Vars)
                        {
                            {
                                var decl__prev2 = decl;

                                decl = pkg.oneLineNode(v.Decl);

                                if (decl != "")
                                {
                                    pkg.Printf(indent + "%s\n", decl);
                                }

                                decl = decl__prev2;

                            }
                        }
                        foreach (var (_, constructor) in typ.Funcs)
                        {
                            if (isExported(constructor.Name))
                            {
                                pkg.Printf(indent + "%s\n", pkg.oneLineNode(constructor.Decl));
                            }
                        }
                    }
                }
            }
        }

        // bugs prints the BUGS information for the package.
        // TODO: Provide access to TODOs and NOTEs as well (very noisy so off by default)?
        private static void bugs(this ref Package pkg)
        {
            if (pkg.doc.Notes["BUG"] == null)
            {
                return;
            }
            pkg.Printf("\n");
            foreach (var (_, note) in pkg.doc.Notes["BUG"])
            {
                pkg.Printf("%s: %v\n", "BUG", note.Body);
            }
        }

        // findValues finds the doc.Values that describe the symbol.
        private static slice<ref doc.Value> findValues(this ref Package pkg, @string symbol, slice<ref doc.Value> docValues)
        {
            foreach (var (_, value) in docValues)
            {
                foreach (var (_, name) in value.Names)
                {
                    if (match(symbol, name))
                    {
                        values = append(values, value);
                    }
                }
            }
            return;
        }

        // findFuncs finds the doc.Funcs that describes the symbol.
        private static slice<ref doc.Func> findFuncs(this ref Package pkg, @string symbol)
        {
            foreach (var (_, fun) in pkg.doc.Funcs)
            {
                if (match(symbol, fun.Name))
                {
                    funcs = append(funcs, fun);
                }
            }
            return;
        }

        // findTypes finds the doc.Types that describes the symbol.
        // If symbol is empty, it finds all exported types.
        private static slice<ref doc.Type> findTypes(this ref Package pkg, @string symbol)
        {
            foreach (var (_, typ) in pkg.doc.Types)
            {
                if (symbol == "" && isExported(typ.Name) || match(symbol, typ.Name))
                {
                    types = append(types, typ);
                }
            }
            return;
        }

        // findTypeSpec returns the ast.TypeSpec within the declaration that defines the symbol.
        // The name must match exactly.
        private static ref ast.TypeSpec findTypeSpec(this ref Package pkg, ref ast.GenDecl decl, @string symbol)
        {
            foreach (var (_, spec) in decl.Specs)
            {
                ref ast.TypeSpec typeSpec = spec._<ref ast.TypeSpec>(); // Must succeed.
                if (symbol == typeSpec.Name.Name)
                {
                    return typeSpec;
                }
            }
            return null;
        }

        // symbolDoc prints the docs for symbol. There may be multiple matches.
        // If symbol matches a type, output includes its methods factories and associated constants.
        // If there is no top-level symbol, symbolDoc looks for methods that match.
        private static bool symbolDoc(this ref Package _pkg, @string symbol) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            defer(pkg.flush());
            var found = false; 
            // Functions.
            foreach (var (_, fun) in pkg.findFuncs(symbol))
            {
                if (!found)
                {
                    pkg.packageClause(true);
                } 
                // Symbol is a function.
                var decl = fun.Decl;
                decl.Body = null;
                pkg.emit(fun.Doc, decl);
                found = true;
            } 
            // Constants and variables behave the same.
            var values = pkg.findValues(symbol, pkg.doc.Consts);
            values = append(values, pkg.findValues(symbol, pkg.doc.Vars)); 
            // A declaration like
            //    const ( c = 1; C = 2 )
            // could be printed twice if the -u flag is set, as it matches twice.
            // So we remember which declarations we've printed to avoid duplication.
            var printed = make_map<ref ast.GenDecl, bool>();
            foreach (var (_, value) in values)
            { 
                // Print each spec only if there is at least one exported symbol in it.
                // (See issue 11008.)
                // TODO: Should we elide unexported symbols from a single spec?
                // It's an unlikely scenario, probably not worth the trouble.
                // TODO: Would be nice if go/doc did this for us.
                var specs = make_slice<ast.Spec>(0L, len(value.Decl.Specs));
                ast.Expr typ = default;
                {
                    var spec__prev2 = spec;

                    foreach (var (_, __spec) in value.Decl.Specs)
                    {
                        spec = __spec;
                        ref ast.ValueSpec vspec = spec._<ref ast.ValueSpec>(); 

                        // The type name may carry over from a previous specification in the
                        // case of constants and iota.
                        if (vspec.Type != null)
                        {
                            typ = vspec.Type;
                        }
                        foreach (var (_, ident) in vspec.Names)
                        {
                            if (isExported(ident.Name))
                            {
                                if (vspec.Type == null && vspec.Values == null && typ != null)
                                { 
                                    // This a standalone identifier, as in the case of iota usage.
                                    // Thus, assume the type comes from the previous type.
                                    vspec.Type = ref new ast.Ident(Name:string(pkg.oneLineNode(typ)),NamePos:vspec.End()-1,);
                                }
                                specs = append(specs, vspec);
                                typ = null; // Only inject type on first exported identifier
                                break;
                            }
                        }
                    }

                    spec = spec__prev2;
                }

                if (len(specs) == 0L || printed[value.Decl])
                {
                    continue;
                }
                value.Decl.Specs = specs;
                if (!found)
                {
                    pkg.packageClause(true);
                }
                pkg.emit(value.Doc, value.Decl);
                printed[value.Decl] = true;
                found = true;
            } 
            // Types.
            {
                ast.Expr typ__prev1 = typ;

                foreach (var (_, __typ) in pkg.findTypes(symbol))
                {
                    typ = __typ;
                    if (!found)
                    {
                        pkg.packageClause(true);
                    }
                    decl = typ.Decl;
                    var spec = pkg.findTypeSpec(decl, typ.Name);
                    trimUnexportedElems(spec); 
                    // If there are multiple types defined, reduce to just this one.
                    if (len(decl.Specs) > 1L)
                    {
                        decl.Specs = new slice<ast.Spec>(new ast.Spec[] { spec });
                    }
                    pkg.emit(typ.Doc, decl); 
                    // Show associated methods, constants, etc.
                    if (len(typ.Consts) > 0L || len(typ.Vars) > 0L || len(typ.Funcs) > 0L || len(typ.Methods) > 0L)
                    {
                        pkg.Printf("\n");
                    }
                    pkg.valueSummary(typ.Consts, true);
                    pkg.valueSummary(typ.Vars, true);
                    pkg.funcSummary(typ.Funcs, true);
                    pkg.funcSummary(typ.Methods, true);
                    found = true;
                }

                typ = typ__prev1;
            }

            if (!found)
            { 
                // See if there are methods.
                if (!pkg.printMethodDoc("", symbol))
                {
                    return false;
                }
            }
            return true;
        });

        // trimUnexportedElems modifies spec in place to elide unexported fields from
        // structs and methods from interfaces (unless the unexported flag is set).
        private static void trimUnexportedElems(ref ast.TypeSpec spec)
        {
            if (unexported)
            {
                return;
            }
            switch (spec.Type.type())
            {
                case ref ast.StructType typ:
                    typ.Fields = trimUnexportedFields(typ.Fields, false);
                    break;
                case ref ast.InterfaceType typ:
                    typ.Methods = trimUnexportedFields(typ.Methods, true);
                    break;
            }
        }

        // trimUnexportedFields returns the field list trimmed of unexported fields.
        private static ref ast.FieldList trimUnexportedFields(ref ast.FieldList fields, bool isInterface)
        {
            @string what = "methods";
            if (!isInterface)
            {
                what = "fields";
            }
            var trimmed = false;
            var list = make_slice<ref ast.Field>(0L, len(fields.List));
            foreach (var (_, field) in fields.List)
            {
                var names = field.Names;
                if (len(names) == 0L)
                { 
                    // Embedded type. Use the name of the type. It must be of the form ident or
                    // pkg.ident (for structs and interfaces), or *ident or *pkg.ident (structs only).
                    // Nothing else is allowed.
                    var ty = field.Type;
                    {
                        ref ast.StarExpr (se, ok) = field.Type._<ref ast.StarExpr>();

                        if (!isInterface && ok)
                        { 
                            // The form *ident or *pkg.ident is only valid on
                            // embedded types in structs.
                            ty = se.X;
                        }

                    }
                    switch (ty.type())
                    {
                        case ref ast.Ident ident:
                            if (isInterface && ident.Name == "error" && ident.Obj == null)
                            { 
                                // For documentation purposes, we consider the builtin error
                                // type special when embedded in an interface, such that it
                                // always gets shown publicly.
                                list = append(list, field);
                                continue;
                            }
                            names = new slice<ref ast.Ident>(new ref ast.Ident[] { ident });
                            break;
                        case ref ast.SelectorExpr ident:
                            names = new slice<ref ast.Ident>(new ref ast.Ident[] { ident.Sel });
                            break;
                    }
                    if (names == null)
                    { 
                        // Can only happen if AST is incorrect. Safe to continue with a nil list.
                        log.Print("invalid program: unexpected type for embedded field");
                    }
                } 
                // Trims if any is unexported. Good enough in practice.
                var ok = true;
                foreach (var (_, name) in names)
                {
                    if (!isExported(name.Name))
                    {
                        trimmed = true;
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    list = append(list, field);
                }
            }
            if (!trimmed)
            {
                return fields;
            }
            ast.Field unexportedField = ref new ast.Field(Type:&ast.Ident{Name:"",NamePos:fields.Closing-1,},Comment:&ast.CommentGroup{List:[]*ast.Comment{{Text:fmt.Sprintf("// Has unexported %s.\n",what)}},},);
            return ref new ast.FieldList(Opening:fields.Opening,List:append(list,unexportedField),Closing:fields.Closing,);
        }

        // printMethodDoc prints the docs for matches of symbol.method.
        // If symbol is empty, it prints all methods that match the name.
        // It reports whether it found any methods.
        private static bool printMethodDoc(this ref Package _pkg, @string symbol, @string method) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            defer(pkg.flush());
            var types = pkg.findTypes(symbol);
            if (types == null)
            {
                if (symbol == "")
                {
                    return false;
                }
                pkg.Fatalf("symbol %s is not a type in package %s installed in %q", symbol, pkg.name, pkg.build.ImportPath);
            }
            var found = false;
            foreach (var (_, typ) in types)
            {
                if (len(typ.Methods) > 0L)
                {
                    foreach (var (_, meth) in typ.Methods)
                    {
                        if (match(method, meth.Name))
                        {
                            var decl = meth.Decl;
                            decl.Body = null;
                            pkg.emit(meth.Doc, decl);
                            found = true;
                        }
                    }
                    continue;
                } 
                // Type may be an interface. The go/doc package does not attach
                // an interface's methods to the doc.Type. We need to dig around.
                var spec = pkg.findTypeSpec(typ.Decl, typ.Name);
                ref ast.InterfaceType (inter, ok) = spec.Type._<ref ast.InterfaceType>();
                if (!ok)
                { 
                    // Not an interface type.
                    continue;
                }
                foreach (var (_, iMethod) in inter.Methods.List)
                { 
                    // This is an interface, so there can be only one name.
                    // TODO: Anonymous methods (embedding)
                    if (len(iMethod.Names) == 0L)
                    {
                        continue;
                    }
                    var name = iMethod.Names[0L].Name;
                    if (match(method, name))
                    {
                        if (iMethod.Doc != null)
                        {
                            foreach (var (_, comment) in iMethod.Doc.List)
                            {
                                doc.ToText(ref pkg.buf, comment.Text, "", indent, indentedWidth);
                            }
                        }
                        var s = pkg.oneLineNode(iMethod.Type); 
                        // Hack: s starts "func" but there is no name present.
                        // We could instead build a FuncDecl but it's not worthwhile.
                        @string lineComment = "";
                        if (iMethod.Comment != null)
                        {
                            lineComment = fmt.Sprintf("  %s", iMethod.Comment.List[0L].Text);
                        }
                        pkg.Printf("func %s%s%s\n", name, s[4L..], lineComment);
                        found = true;
                    }
                }
            }
            return found;
        });

        // printFieldDoc prints the docs for matches of symbol.fieldName.
        // It reports whether it found any field.
        // Both symbol and fieldName must be non-empty or it returns false.
        private static bool printFieldDoc(this ref Package _pkg, @string symbol, @string fieldName) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            if (symbol == "" || fieldName == "")
            {
                return false;
            }
            defer(pkg.flush());
            var types = pkg.findTypes(symbol);
            if (types == null)
            {
                pkg.Fatalf("symbol %s is not a type in package %s installed in %q", symbol, pkg.name, pkg.build.ImportPath);
            }
            var found = false;
            long numUnmatched = 0L;
            foreach (var (_, typ) in types)
            { 
                // Type must be a struct.
                var spec = pkg.findTypeSpec(typ.Decl, typ.Name);
                ref ast.StructType (structType, ok) = spec.Type._<ref ast.StructType>();
                if (!ok)
                { 
                    // Not a struct type.
                    continue;
                }
                foreach (var (_, field) in structType.Fields.List)
                { 
                    // TODO: Anonymous fields.
                    foreach (var (_, name) in field.Names)
                    {
                        if (!match(fieldName, name.Name))
                        {
                            numUnmatched++;
                            continue;
                        }
                        if (!found)
                        {
                            pkg.Printf("type %s struct {\n", typ.Name);
                        }
                        if (field.Doc != null)
                        {
                            foreach (var (_, comment) in field.Doc.List)
                            {
                                doc.ToText(ref pkg.buf, comment.Text, indent, indent, indentedWidth);
                            }
                        }
                        var s = pkg.oneLineNode(field.Type);
                        @string lineComment = "";
                        if (field.Comment != null)
                        {
                            lineComment = fmt.Sprintf("  %s", field.Comment.List[0L].Text);
                        }
                        pkg.Printf("%s%s %s%s\n", indent, name, s, lineComment);
                        found = true;
                    }
                }
            }
            if (found)
            {
                if (numUnmatched > 0L)
                {
                    pkg.Printf("\n    // ... other fields elided ...\n");
                }
                pkg.Printf("}\n");
            }
            return found;
        });

        // methodDoc prints the docs for matches of symbol.method.
        private static bool methodDoc(this ref Package _pkg, @string symbol, @string method) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            defer(pkg.flush());
            return pkg.printMethodDoc(symbol, method);
        });

        // fieldDoc prints the docs for matches of symbol.field.
        private static bool fieldDoc(this ref Package _pkg, @string symbol, @string field) => func(_pkg, (ref Package pkg, Defer defer, Panic _, Recover __) =>
        {
            defer(pkg.flush());
            return pkg.printFieldDoc(symbol, field);
        });

        // match reports whether the user's symbol matches the program's.
        // A lower-case character in the user's string matches either case in the program's.
        // The program string must be exported.
        private static bool match(@string user, @string program)
        {
            if (!isExported(program))
            {
                return false;
            }
            if (matchCase)
            {
                return user == program;
            }
            foreach (var (_, u) in user)
            {
                var (p, w) = utf8.DecodeRuneInString(program);
                program = program[w..];
                if (u == p)
                {
                    continue;
                }
                if (unicode.IsLower(u) && simpleFold(u) == simpleFold(p))
                {
                    continue;
                }
                return false;
            }
            return program == "";
        }

        // simpleFold returns the minimum rune equivalent to r
        // under Unicode-defined simple case folding.
        private static int simpleFold(int r)
        {
            while (true)
            {
                var r1 = unicode.SimpleFold(r);
                if (r1 <= r)
                {
                    return r1; // wrapped around, found min
                }
                r = r1;
            }

        }
    }
}
