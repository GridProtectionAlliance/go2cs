// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:29:10 UTC
// Original source: C:\Program Files\Go\src\cmd\doc\pkg.go
namespace go;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using ast = go.ast_package;
using build = go.build_package;
using doc = go.doc_package;
using format = go.format_package;
using parser = go.parser_package;
using printer = go.printer_package;
using token = go.token_package;
using io = io_package;
using fs = io.fs_package;
using log = log_package;
using filepath = path.filepath_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class main_package {

private static readonly nint punchedCardWidth = 80; // These things just won't leave us alone.
private static readonly var indentedWidth = punchedCardWidth - len(indent);
private static readonly @string indent = "    ";

public partial struct Package {
    public io.Writer writer; // Destination for output.
    public @string name; // Package name, json for encoding/json.
    public @string userPath; // String the user used to find this package.
    public ptr<ast.Package> pkg; // Parsed package.
    public ptr<ast.File> file; // Merged from all files in the package
    public ptr<doc.Package> doc;
    public ptr<build.Package> build;
    public map<ptr<doc.Value>, bool> typedValue; // Consts and vars related to types.
    public map<ptr<doc.Func>, bool> constructor; // Constructors.
    public ptr<token.FileSet> fs; // Needed for printing.
    public pkgBuffer buf;
}

// pkgBuffer is a wrapper for bytes.Buffer that prints a package clause the
// first time Write is called.
private partial struct pkgBuffer {
    public ptr<Package> pkg;
    public bool printed; // Prevent repeated package clauses.
    public ref bytes.Buffer Buffer => ref Buffer_val;
}

private static (nint, error) Write(this ptr<pkgBuffer> _addr_pb, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref pkgBuffer pb = ref _addr_pb.val;

    pb.packageClause();
    return pb.Buffer.Write(p);
}

private static void packageClause(this ptr<pkgBuffer> _addr_pb) {
    ref pkgBuffer pb = ref _addr_pb.val;

    if (!pb.printed) {
        pb.printed = true; 
        // Only show package clause for commands if requested explicitly.
        if (pb.pkg.pkg.Name != "main" || showCmd) {
            pb.pkg.packageClause();
        }
    }
}

public partial struct PackageError { // : @string
} // type returned by pkg.Fatalf.

public static @string Error(this PackageError p) {
    return string(p);
}

// prettyPath returns a version of the package path that is suitable for an
// error message. It obeys the import comment if present. Also, since
// pkg.build.ImportPath is sometimes the unhelpful "" or ".", it looks for a
// directory name in GOROOT or GOPATH if that happens.
private static @string prettyPath(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    var path = pkg.build.ImportComment;
    if (path == "") {
        path = pkg.build.ImportPath;
    }
    if (path != "." && path != "") {
        return path;
    }
    path = filepath.Clean(filepath.ToSlash(pkg.build.Dir)); 
    // Can we find a decent prefix?
    var goroot = filepath.Join(buildCtx.GOROOT, "src");
    {
        var p__prev1 = p;

        var (p, ok) = trim(path, filepath.ToSlash(goroot));

        if (ok) {
            return p;
        }
        p = p__prev1;

    }
    foreach (var (_, gopath) in splitGopath()) {
        {
            var p__prev1 = p;

            (p, ok) = trim(path, filepath.ToSlash(gopath));

            if (ok) {
                return p;
            }

            p = p__prev1;

        }
    }    return path;
}

// trim trims the directory prefix from the path, paying attention
// to the path separator. If they are the same string or the prefix
// is not present the original is returned. The boolean reports whether
// the prefix is present. That path and prefix have slashes for separators.
private static (@string, bool) trim(@string path, @string prefix) {
    @string _p0 = default;
    bool _p0 = default;

    if (!strings.HasPrefix(path, prefix)) {
        return (path, false);
    }
    if (path == prefix) {
        return (path, true);
    }
    if (path[len(prefix)] == '/') {
        return (path[(int)len(prefix) + 1..], true);
    }
    return (path, false); // Textual prefix but not a path prefix.
}

// pkg.Fatalf is like log.Fatalf, but panics so it can be recovered in the
// main do function, so it doesn't cause an exit. Allows testing to work
// without running a subprocess. The log prefix will be added when
// logged in main; it is not added here.
private static void Fatalf(this ptr<Package> _addr_pkg, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref Package pkg = ref _addr_pkg.val;

    panic(PackageError(fmt.Sprintf(format, args)));
});

// parsePackage turns the build package we found into a parsed package
// we can then use to generate documentation.
private static ptr<Package> parsePackage(io.Writer writer, ptr<build.Package> _addr_pkg, @string userPath) {
    ref build.Package pkg = ref _addr_pkg.val;
 
    // include tells parser.ParseDir which files to include.
    // That means the file must be in the build package's GoFiles or CgoFiles
    // list only (no tag-ignored files, tests, swig or other non-Go files).
    Func<fs.FileInfo, bool> include = info => {
        {
            var name__prev1 = name;

            foreach (var (_, __name) in pkg.GoFiles) {
                name = __name;
                if (name == info.Name()) {
                    return _addr_true!;
                }
            }

            name = name__prev1;
        }

        {
            var name__prev1 = name;

            foreach (var (_, __name) in pkg.CgoFiles) {
                name = __name;
                if (name == info.Name()) {
                    return _addr_true!;
                }
            }

            name = name__prev1;
        }

        return _addr_false!;
    };
    var fset = token.NewFileSet();
    var (pkgs, err) = parser.ParseDir(fset, pkg.Dir, include, parser.ParseComments);
    if (err != null) {
        log.Fatal(err);
    }
    if (len(pkgs) == 0) {
        log.Fatalf("no source-code package in directory %s", pkg.Dir);
    }
    if (len(pkgs) > 1) {
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
    var mode = doc.AllDecls;
    if (showSrc) {
        mode |= doc.PreserveAST; // See comment for Package.emit.
    }
    var docPkg = doc.New(astPkg, pkg.ImportPath, mode);
    var typedValue = make_map<ptr<doc.Value>, bool>();
    var constructor = make_map<ptr<doc.Func>, bool>();
    foreach (var (_, typ) in docPkg.Types) {
        docPkg.Consts = append(docPkg.Consts, typ.Consts);
        docPkg.Vars = append(docPkg.Vars, typ.Vars);
        docPkg.Funcs = append(docPkg.Funcs, typ.Funcs);
        if (isExported(typ.Name)) {
            {
                var value__prev2 = value;

                foreach (var (_, __value) in typ.Consts) {
                    value = __value;
                    typedValue[value] = true;
                }

                value = value__prev2;
            }

            {
                var value__prev2 = value;

                foreach (var (_, __value) in typ.Vars) {
                    value = __value;
                    typedValue[value] = true;
                }

                value = value__prev2;
            }

            foreach (var (_, fun) in typ.Funcs) { 
                // We don't count it as a constructor bound to the type
                // if the type itself is not exported.
                constructor[fun] = true;
            }
        }
    }    ptr<Package> p = addr(new Package(writer:writer,name:pkg.Name,userPath:userPath,pkg:astPkg,file:ast.MergePackageFiles(astPkg,0),doc:docPkg,typedValue:typedValue,constructor:constructor,build:pkg,fs:fset,));
    p.buf.pkg = p;
    return _addr_p!;
}

private static void Printf(this ptr<Package> _addr_pkg, @string format, params object[] args) {
    args = args.Clone();
    ref Package pkg = ref _addr_pkg.val;

    fmt.Fprintf(_addr_pkg.buf, format, args);
}

private static void flush(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    var (_, err) = pkg.writer.Write(pkg.buf.Bytes());
    if (err != null) {
        log.Fatal(err);
    }
    pkg.buf.Reset(); // Not needed, but it's a flush.
}

private static slice<byte> newlineBytes = (slice<byte>)"\n\n"; // We never ask for more than 2.

// newlines guarantees there are n newlines at the end of the buffer.
private static void newlines(this ptr<Package> _addr_pkg, nint n) {
    ref Package pkg = ref _addr_pkg.val;

    while (!bytes.HasSuffix(pkg.buf.Bytes(), newlineBytes[..(int)n])) {
        pkg.buf.WriteRune('\n');
    }
}

// emit prints the node. If showSrc is true, it ignores the provided comment,
// assuming the comment is in the node itself. Otherwise, the go/doc package
// clears the stuff we don't want to print anyway. It's a bit of a magic trick.
private static void emit(this ptr<Package> _addr_pkg, @string comment, ast.Node node) {
    ref Package pkg = ref _addr_pkg.val;

    if (node != null) {
        var arg = node;
        if (showSrc) { 
            // Need an extra little dance to get internal comments to appear.
            arg = addr(new printer.CommentedNode(Node:node,Comments:pkg.file.Comments,));
        }
        var err = format.Node(_addr_pkg.buf, pkg.fs, arg);
        if (err != null) {
            log.Fatal(err);
        }
        if (comment != "" && !showSrc) {
            pkg.newlines(1);
            doc.ToText(_addr_pkg.buf, comment, indent, indent + indent, indentedWidth);
            pkg.newlines(2); // Blank line after comment to separate from next item.
        }
        else
 {
            pkg.newlines(1);
        }
    }
}

// oneLineNode returns a one-line summary of the given input node.
private static @string oneLineNode(this ptr<Package> _addr_pkg, ast.Node node) {
    ref Package pkg = ref _addr_pkg.val;

    const nint maxDepth = 10;

    return pkg.oneLineNodeDepth(node, maxDepth);
}

// oneLineNodeDepth returns a one-line summary of the given input node.
// The depth specifies the maximum depth when traversing the AST.
private static @string oneLineNodeDepth(this ptr<Package> _addr_pkg, ast.Node node, nint depth) {
    ref Package pkg = ref _addr_pkg.val;

    const @string dotDotDot = "...";

    if (depth == 0) {
        return dotDotDot;
    }
    depth--;

    switch (node.type()) {
        case 
            return "";
            break;
        case ptr<ast.GenDecl> n:
            @string trailer = "";
            if (len(n.Specs) > 1) {
                trailer = " " + dotDotDot;
            } 

            // Find the first relevant spec.
            @string typ = "";
            foreach (var (i, spec) in n.Specs) {
                ptr<ast.ValueSpec> valueSpec = spec._<ptr<ast.ValueSpec>>(); // Must succeed; we can't mix types in one GenDecl.

                // The type name may carry over from a previous specification in the
                // case of constants and iota.
                if (valueSpec.Type != null) {
                    typ = fmt.Sprintf(" %s", pkg.oneLineNodeDepth(valueSpec.Type, depth));
                }
                else if (len(valueSpec.Values) > 0) {
                    typ = "";
                }
                if (!isExported(valueSpec.Names[0].Name)) {
                    continue;
                }
                @string val = "";
                if (i < len(valueSpec.Values) && valueSpec.Values[i] != null) {
                    val = fmt.Sprintf(" = %s", pkg.oneLineNodeDepth(valueSpec.Values[i], depth));
                }
                return fmt.Sprintf("%s %s%s%s%s", n.Tok, valueSpec.Names[0], typ, val, trailer);
            }
            return "";
            break;
        case ptr<ast.FuncDecl> n:
            var name = n.Name.Name;
            var recv = pkg.oneLineNodeDepth(n.Recv, depth);
            if (len(recv) > 0) {
                recv = "(" + recv + ") ";
            }
            var fnc = pkg.oneLineNodeDepth(n.Type, depth);
            if (strings.Index(fnc, "func") == 0) {
                fnc = fnc[(int)4..];
            }
            return fmt.Sprintf("func %s%s%s", recv, name, fnc);
            break;
        case ptr<ast.TypeSpec> n:
            @string sep = " ";
            if (n.Assign.IsValid()) {
                sep = " = ";
            }
            return fmt.Sprintf("type %s%s%s", n.Name.Name, sep, pkg.oneLineNodeDepth(n.Type, depth));
            break;
        case ptr<ast.FuncType> n:
            slice<@string> @params = default;
            if (n.Params != null) {
                {
                    var field__prev1 = field;

                    foreach (var (_, __field) in n.Params.List) {
                        field = __field;
                        params = append(params, pkg.oneLineField(field, depth));
                    }

                    field = field__prev1;
                }
            }
            var needParens = false;
            slice<@string> results = default;
            if (n.Results != null) {
                needParens = needParens || len(n.Results.List) > 1;
                {
                    var field__prev1 = field;

                    foreach (var (_, __field) in n.Results.List) {
                        field = __field;
                        needParens = needParens || len(field.Names) > 0;
                        results = append(results, pkg.oneLineField(field, depth));
                    }

                    field = field__prev1;
                }
            }
            var param = joinStrings(params);
            if (len(results) == 0) {
                return fmt.Sprintf("func(%s)", param);
            }
            var result = joinStrings(results);
            if (!needParens) {
                return fmt.Sprintf("func(%s) %s", param, result);
            }
            return fmt.Sprintf("func(%s) (%s)", param, result);
            break;
        case ptr<ast.StructType> n:
            if (n.Fields == null || len(n.Fields.List) == 0) {
                return "struct{}";
            }
            return "struct{ ... }";
            break;
        case ptr<ast.InterfaceType> n:
            if (n.Methods == null || len(n.Methods.List) == 0) {
                return "interface{}";
            }
            return "interface{ ... }";
            break;
        case ptr<ast.FieldList> n:
            if (n == null || len(n.List) == 0) {
                return "";
            }
            if (len(n.List) == 1) {
                return pkg.oneLineField(n.List[0], depth);
            }
            return dotDotDot;
            break;
        case ptr<ast.FuncLit> n:
            return pkg.oneLineNodeDepth(n.Type, depth) + " { ... }";
            break;
        case ptr<ast.CompositeLit> n:
            typ = pkg.oneLineNodeDepth(n.Type, depth);
            if (len(n.Elts) == 0) {
                return fmt.Sprintf("%s{}", typ);
            }
            return fmt.Sprintf("%s{ %s }", typ, dotDotDot);
            break;
        case ptr<ast.ArrayType> n:
            var length = pkg.oneLineNodeDepth(n.Len, depth);
            var element = pkg.oneLineNodeDepth(n.Elt, depth);
            return fmt.Sprintf("[%s]%s", length, element);
            break;
        case ptr<ast.MapType> n:
            var key = pkg.oneLineNodeDepth(n.Key, depth);
            var value = pkg.oneLineNodeDepth(n.Value, depth);
            return fmt.Sprintf("map[%s]%s", key, value);
            break;
        case ptr<ast.CallExpr> n:
            fnc = pkg.oneLineNodeDepth(n.Fun, depth);
            slice<@string> args = default;
            foreach (var (_, arg) in n.Args) {
                args = append(args, pkg.oneLineNodeDepth(arg, depth));
            }
            return fmt.Sprintf("%s(%s)", fnc, joinStrings(args));
            break;
        case ptr<ast.UnaryExpr> n:
            return fmt.Sprintf("%s%s", n.Op, pkg.oneLineNodeDepth(n.X, depth));
            break;
        case ptr<ast.Ident> n:
            return n.Name;
            break;
        default:
        {
            var n = node.type();
            ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
            format.Node(buf, pkg.fs, node);
            var s = buf.String();
            if (strings.Contains(s, "\n")) {
                return dotDotDot;
            }
            return s;
            break;
        }
    }
}

// oneLineField returns a one-line summary of the field.
private static @string oneLineField(this ptr<Package> _addr_pkg, ptr<ast.Field> _addr_field, nint depth) {
    ref Package pkg = ref _addr_pkg.val;
    ref ast.Field field = ref _addr_field.val;

    slice<@string> names = default;
    foreach (var (_, name) in field.Names) {
        names = append(names, name.Name);
    }    if (len(names) == 0) {
        return pkg.oneLineNodeDepth(field.Type, depth);
    }
    return joinStrings(names) + " " + pkg.oneLineNodeDepth(field.Type, depth);
}

// joinStrings formats the input as a comma-separated list,
// but truncates the list at some reasonable length if necessary.
private static @string joinStrings(slice<@string> ss) {
    nint n = default;
    foreach (var (i, s) in ss) {
        n += len(s) + len(", ");
        if (n > punchedCardWidth) {
            ss = append(ss.slice(-1, i, i), "...");
            break;
        }
    }    return strings.Join(ss, ", ");
}

// allDoc prints all the docs for the package.
private static void allDoc(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    pkg.Printf(""); // Trigger the package clause; we know the package exists.
    doc.ToText(_addr_pkg.buf, pkg.doc.Doc, "", indent, indentedWidth);
    pkg.newlines(1);

    var printed = make_map<ptr<ast.GenDecl>, bool>();

    @string hdr = "";
    Action<@string> printHdr = s => {
        if (hdr != s) {
            pkg.Printf("\n%s\n\n", s);
            hdr = s;
        }
    }; 

    // Constants.
    {
        var value__prev1 = value;

        foreach (var (_, __value) in pkg.doc.Consts) {
            value = __value; 
            // Constants and variables come in groups, and valueDoc prints
            // all the items in the group. We only need to find one exported symbol.
            {
                var name__prev2 = name;

                foreach (var (_, __name) in value.Names) {
                    name = __name;
                    if (isExported(name) && !pkg.typedValue[value]) {
                        printHdr("CONSTANTS");
                        pkg.valueDoc(value, printed);
                        break;
                    }
                }

                name = name__prev2;
            }
        }
        value = value__prev1;
    }

    {
        var value__prev1 = value;

        foreach (var (_, __value) in pkg.doc.Vars) {
            value = __value; 
            // Constants and variables come in groups, and valueDoc prints
            // all the items in the group. We only need to find one exported symbol.
            {
                var name__prev2 = name;

                foreach (var (_, __name) in value.Names) {
                    name = __name;
                    if (isExported(name) && !pkg.typedValue[value]) {
                        printHdr("VARIABLES");
                        pkg.valueDoc(value, printed);
                        break;
                    }
                }

                name = name__prev2;
            }
        }
        value = value__prev1;
    }

    foreach (var (_, fun) in pkg.doc.Funcs) {
        if (isExported(fun.Name) && !pkg.constructor[fun]) {
            printHdr("FUNCTIONS");
            pkg.emit(fun.Doc, fun.Decl);
        }
    }    foreach (var (_, typ) in pkg.doc.Types) {
        if (isExported(typ.Name)) {
            printHdr("TYPES");
            pkg.typeDoc(typ);
        }
    }
}

// packageDoc prints the docs for the package (package doc plus one-liners of the rest).
private static void packageDoc(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    pkg.Printf(""); // Trigger the package clause; we know the package exists.
    if (!short) {
        doc.ToText(_addr_pkg.buf, pkg.doc.Doc, "", indent, indentedWidth);
        pkg.newlines(1);
    }
    if (pkg.pkg.Name == "main" && !showCmd) { 
        // Show only package docs for commands.
        return ;
    }
    if (!short) {
        pkg.newlines(2); // Guarantee blank line before the components.
    }
    pkg.valueSummary(pkg.doc.Consts, false);
    pkg.valueSummary(pkg.doc.Vars, false);
    pkg.funcSummary(pkg.doc.Funcs, false);
    pkg.typeSummary();
    if (!short) {
        pkg.bugs();
    }
}

// packageClause prints the package clause.
private static void packageClause(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    if (short) {
        return ;
    }
    var importPath = pkg.build.ImportComment;
    if (importPath == "") {
        importPath = pkg.build.ImportPath;
    }
    if (usingModules) {
        foreach (var (_, root) in codeRoots()) {
            if (pkg.build.Dir == root.dir) {
                importPath = root.importPath;
                break;
            }
            if (strings.HasPrefix(pkg.build.Dir, root.dir + string(filepath.Separator))) {
                var suffix = filepath.ToSlash(pkg.build.Dir[(int)len(root.dir) + 1..]);
                if (root.importPath == "") {
                    importPath = suffix;
                }
                else
 {
                    importPath = root.importPath + "/" + suffix;
                }
                break;
            }
        }
    }
    pkg.Printf("package %s // import %q\n\n", pkg.name, importPath);
    if (!usingModules && importPath != pkg.build.ImportPath) {
        pkg.Printf("WARNING: package source is installed in %q\n", pkg.build.ImportPath);
    }
}

// valueSummary prints a one-line summary for each set of values and constants.
// If all the types in a constant or variable declaration belong to the same
// type they can be printed by typeSummary, and so can be suppressed here.
private static void valueSummary(this ptr<Package> _addr_pkg, slice<ptr<doc.Value>> values, bool showGrouped) {
    ref Package pkg = ref _addr_pkg.val;

    map<ptr<doc.Value>, bool> isGrouped = default;
    if (!showGrouped) {
        isGrouped = make_map<ptr<doc.Value>, bool>();
        foreach (var (_, typ) in pkg.doc.Types) {
            if (!isExported(typ.Name)) {
                continue;
            }
            foreach (var (_, c) in typ.Consts) {
                isGrouped[c] = true;
            }
            foreach (var (_, v) in typ.Vars) {
                isGrouped[v] = true;
            }
        }
    }
    foreach (var (_, value) in values) {
        if (!isGrouped[value]) {
            {
                var decl = pkg.oneLineNode(value.Decl);

                if (decl != "") {
                    pkg.Printf("%s\n", decl);
                }

            }
        }
    }
}

// funcSummary prints a one-line summary for each function. Constructors
// are printed by typeSummary, below, and so can be suppressed here.
private static void funcSummary(this ptr<Package> _addr_pkg, slice<ptr<doc.Func>> funcs, bool showConstructors) {
    ref Package pkg = ref _addr_pkg.val;

    foreach (var (_, fun) in funcs) { 
        // Exported functions only. The go/doc package does not include methods here.
        if (isExported(fun.Name)) {
            if (showConstructors || !pkg.constructor[fun]) {
                pkg.Printf("%s\n", pkg.oneLineNode(fun.Decl));
            }
        }
    }
}

// typeSummary prints a one-line summary for each type, followed by its constructors.
private static void typeSummary(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    foreach (var (_, typ) in pkg.doc.Types) {
        foreach (var (_, spec) in typ.Decl.Specs) {
            ptr<ast.TypeSpec> typeSpec = spec._<ptr<ast.TypeSpec>>(); // Must succeed.
            if (isExported(typeSpec.Name.Name)) {
                pkg.Printf("%s\n", pkg.oneLineNode(typeSpec)); 
                // Now print the consts, vars, and constructors.
                foreach (var (_, c) in typ.Consts) {
                    {
                        var decl__prev2 = decl;

                        var decl = pkg.oneLineNode(c.Decl);

                        if (decl != "") {
                            pkg.Printf(indent + "%s\n", decl);
                        }

                        decl = decl__prev2;

                    }
                }
                foreach (var (_, v) in typ.Vars) {
                    {
                        var decl__prev2 = decl;

                        decl = pkg.oneLineNode(v.Decl);

                        if (decl != "") {
                            pkg.Printf(indent + "%s\n", decl);
                        }

                        decl = decl__prev2;

                    }
                }
                foreach (var (_, constructor) in typ.Funcs) {
                    if (isExported(constructor.Name)) {
                        pkg.Printf(indent + "%s\n", pkg.oneLineNode(constructor.Decl));
                    }
                }
            }
        }
    }
}

// bugs prints the BUGS information for the package.
// TODO: Provide access to TODOs and NOTEs as well (very noisy so off by default)?
private static void bugs(this ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    if (pkg.doc.Notes["BUG"] == null) {
        return ;
    }
    pkg.Printf("\n");
    foreach (var (_, note) in pkg.doc.Notes["BUG"]) {
        pkg.Printf("%s: %v\n", "BUG", note.Body);
    }
}

// findValues finds the doc.Values that describe the symbol.
private static slice<ptr<doc.Value>> findValues(this ptr<Package> _addr_pkg, @string symbol, slice<ptr<doc.Value>> docValues) {
    slice<ptr<doc.Value>> values = default;
    ref Package pkg = ref _addr_pkg.val;

    foreach (var (_, value) in docValues) {
        foreach (var (_, name) in value.Names) {
            if (match(symbol, name)) {
                values = append(values, value);
            }
        }
    }    return ;
}

// findFuncs finds the doc.Funcs that describes the symbol.
private static slice<ptr<doc.Func>> findFuncs(this ptr<Package> _addr_pkg, @string symbol) {
    slice<ptr<doc.Func>> funcs = default;
    ref Package pkg = ref _addr_pkg.val;

    foreach (var (_, fun) in pkg.doc.Funcs) {
        if (match(symbol, fun.Name)) {
            funcs = append(funcs, fun);
        }
    }    return ;
}

// findTypes finds the doc.Types that describes the symbol.
// If symbol is empty, it finds all exported types.
private static slice<ptr<doc.Type>> findTypes(this ptr<Package> _addr_pkg, @string symbol) {
    slice<ptr<doc.Type>> types = default;
    ref Package pkg = ref _addr_pkg.val;

    foreach (var (_, typ) in pkg.doc.Types) {
        if (symbol == "" && isExported(typ.Name) || match(symbol, typ.Name)) {
            types = append(types, typ);
        }
    }    return ;
}

// findTypeSpec returns the ast.TypeSpec within the declaration that defines the symbol.
// The name must match exactly.
private static ptr<ast.TypeSpec> findTypeSpec(this ptr<Package> _addr_pkg, ptr<ast.GenDecl> _addr_decl, @string symbol) {
    ref Package pkg = ref _addr_pkg.val;
    ref ast.GenDecl decl = ref _addr_decl.val;

    foreach (var (_, spec) in decl.Specs) {
        ptr<ast.TypeSpec> typeSpec = spec._<ptr<ast.TypeSpec>>(); // Must succeed.
        if (symbol == typeSpec.Name.Name) {
            return _addr_typeSpec!;
        }
    }    return _addr_null!;
}

// symbolDoc prints the docs for symbol. There may be multiple matches.
// If symbol matches a type, output includes its methods factories and associated constants.
// If there is no top-level symbol, symbolDoc looks for methods that match.
private static bool symbolDoc(this ptr<Package> _addr_pkg, @string symbol) {
    ref Package pkg = ref _addr_pkg.val;

    var found = false; 
    // Functions.
    foreach (var (_, fun) in pkg.findFuncs(symbol)) { 
        // Symbol is a function.
        var decl = fun.Decl;
        pkg.emit(fun.Doc, decl);
        found = true;
    }    var values = pkg.findValues(symbol, pkg.doc.Consts);
    values = append(values, pkg.findValues(symbol, pkg.doc.Vars)); 
    // A declaration like
    //    const ( c = 1; C = 2 )
    // could be printed twice if the -u flag is set, as it matches twice.
    // So we remember which declarations we've printed to avoid duplication.
    var printed = make_map<ptr<ast.GenDecl>, bool>();
    foreach (var (_, value) in values) {
        pkg.valueDoc(value, printed);
        found = true;
    }    foreach (var (_, typ) in pkg.findTypes(symbol)) {
        pkg.typeDoc(typ);
        found = true;
    }    if (!found) { 
        // See if there are methods.
        if (!pkg.printMethodDoc("", symbol)) {
            return false;
        }
    }
    return true;
}

// valueDoc prints the docs for a constant or variable.
private static void valueDoc(this ptr<Package> _addr_pkg, ptr<doc.Value> _addr_value, map<ptr<ast.GenDecl>, bool> printed) {
    ref Package pkg = ref _addr_pkg.val;
    ref doc.Value value = ref _addr_value.val;

    if (printed[value.Decl]) {
        return ;
    }
    var specs = make_slice<ast.Spec>(0, len(value.Decl.Specs));
    ast.Expr typ = default;
    foreach (var (_, spec) in value.Decl.Specs) {
        ptr<ast.ValueSpec> vspec = spec._<ptr<ast.ValueSpec>>(); 

        // The type name may carry over from a previous specification in the
        // case of constants and iota.
        if (vspec.Type != null) {
            typ = vspec.Type;
        }
        foreach (var (_, ident) in vspec.Names) {
            if (showSrc || isExported(ident.Name)) {
                if (vspec.Type == null && vspec.Values == null && typ != null) { 
                    // This a standalone identifier, as in the case of iota usage.
                    // Thus, assume the type comes from the previous type.
                    vspec.Type = addr(new ast.Ident(Name:pkg.oneLineNode(typ),NamePos:vspec.End()-1,));
                }
                specs = append(specs, vspec);
                typ = null; // Only inject type on first exported identifier
                break;
            }
        }
    }    if (len(specs) == 0) {
        return ;
    }
    value.Decl.Specs = specs;
    pkg.emit(value.Doc, value.Decl);
    printed[value.Decl] = true;
}

// typeDoc prints the docs for a type, including constructors and other items
// related to it.
private static void typeDoc(this ptr<Package> _addr_pkg, ptr<doc.Type> _addr_typ) {
    ref Package pkg = ref _addr_pkg.val;
    ref doc.Type typ = ref _addr_typ.val;

    var decl = typ.Decl;
    var spec = pkg.findTypeSpec(decl, typ.Name);
    trimUnexportedElems(_addr_spec); 
    // If there are multiple types defined, reduce to just this one.
    if (len(decl.Specs) > 1) {
        decl.Specs = new slice<ast.Spec>(new ast.Spec[] { spec });
    }
    pkg.emit(typ.Doc, decl);
    pkg.newlines(2); 
    // Show associated methods, constants, etc.
    if (showAll) {
        var printed = make_map<ptr<ast.GenDecl>, bool>(); 
        // We can use append here to print consts, then vars. Ditto for funcs and methods.
        var values = typ.Consts;
        values = append(values, typ.Vars);
        foreach (var (_, value) in values) {
            foreach (var (_, name) in value.Names) {
                if (isExported(name)) {
                    pkg.valueDoc(value, printed);
                    break;
                }
            }
    else
        }        var funcs = typ.Funcs;
        funcs = append(funcs, typ.Methods);
        foreach (var (_, fun) in funcs) {
            if (isExported(fun.Name)) {
                pkg.emit(fun.Doc, fun.Decl);
                if (fun.Doc == "") {
                    pkg.newlines(2);
                }
            }
        }
    } {
        pkg.valueSummary(typ.Consts, true);
        pkg.valueSummary(typ.Vars, true);
        pkg.funcSummary(typ.Funcs, true);
        pkg.funcSummary(typ.Methods, true);
    }
}

// trimUnexportedElems modifies spec in place to elide unexported fields from
// structs and methods from interfaces (unless the unexported flag is set or we
// are asked to show the original source).
private static void trimUnexportedElems(ptr<ast.TypeSpec> _addr_spec) {
    ref ast.TypeSpec spec = ref _addr_spec.val;

    if (unexported || showSrc) {
        return ;
    }
    switch (spec.Type.type()) {
        case ptr<ast.StructType> typ:
            typ.Fields = trimUnexportedFields(_addr_typ.Fields, false);
            break;
        case ptr<ast.InterfaceType> typ:
            typ.Methods = trimUnexportedFields(_addr_typ.Methods, true);
            break;
    }
}

// trimUnexportedFields returns the field list trimmed of unexported fields.
private static ptr<ast.FieldList> trimUnexportedFields(ptr<ast.FieldList> _addr_fields, bool isInterface) {
    ref ast.FieldList fields = ref _addr_fields.val;

    @string what = "methods";
    if (!isInterface) {
        what = "fields";
    }
    var trimmed = false;
    var list = make_slice<ptr<ast.Field>>(0, len(fields.List));
    foreach (var (_, field) in fields.List) {
        var names = field.Names;
        if (len(names) == 0) { 
            // Embedded type. Use the name of the type. It must be of the form ident or
            // pkg.ident (for structs and interfaces), or *ident or *pkg.ident (structs only).
            // Nothing else is allowed.
            var ty = field.Type;
            {
                ptr<ast.StarExpr> (se, ok) = field.Type._<ptr<ast.StarExpr>>();

                if (!isInterface && ok) { 
                    // The form *ident or *pkg.ident is only valid on
                    // embedded types in structs.
                    ty = se.X;
                }

            }
            switch (ty.type()) {
                case ptr<ast.Ident> ident:
                    if (isInterface && ident.Name == "error" && ident.Obj == null) { 
                        // For documentation purposes, we consider the builtin error
                        // type special when embedded in an interface, such that it
                        // always gets shown publicly.
                        list = append(list, field);
                        continue;
                    }
                    names = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { ident });
                    break;
                case ptr<ast.SelectorExpr> ident:
                    names = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { ident.Sel });
                    break;
            }
            if (names == null) { 
                // Can only happen if AST is incorrect. Safe to continue with a nil list.
                log.Print("invalid program: unexpected type for embedded field");
            }
        }
        var ok = true;
        foreach (var (_, name) in names) {
            if (!isExported(name.Name)) {
                trimmed = true;
                ok = false;
                break;
            }
        }        if (ok) {
            list = append(list, field);
        }
    }    if (!trimmed) {
        return _addr_fields!;
    }
    ptr<ast.Field> unexportedField = addr(new ast.Field(Type:&ast.Ident{Name:"",NamePos:fields.Closing-1,},Comment:&ast.CommentGroup{List:[]*ast.Comment{{Text:fmt.Sprintf("// Has unexported %s.\n",what)}},},));
    return addr(new ast.FieldList(Opening:fields.Opening,List:append(list,unexportedField),Closing:fields.Closing,));
}

// printMethodDoc prints the docs for matches of symbol.method.
// If symbol is empty, it prints all methods for any concrete type
// that match the name. It reports whether it found any methods.
private static bool printMethodDoc(this ptr<Package> _addr_pkg, @string symbol, @string method) {
    ref Package pkg = ref _addr_pkg.val;

    var types = pkg.findTypes(symbol);
    if (types == null) {
        if (symbol == "") {
            return false;
        }
        pkg.Fatalf("symbol %s is not a type in package %s installed in %q", symbol, pkg.name, pkg.build.ImportPath);
    }
    var found = false;
    foreach (var (_, typ) in types) {
        if (len(typ.Methods) > 0) {
            foreach (var (_, meth) in typ.Methods) {
                if (match(method, meth.Name)) {
                    var decl = meth.Decl;
                    pkg.emit(meth.Doc, decl);
                    found = true;
                }
            }
            continue;
        }
        if (symbol == "") {
            continue;
        }
        var spec = pkg.findTypeSpec(typ.Decl, typ.Name);
        ptr<ast.InterfaceType> (inter, ok) = spec.Type._<ptr<ast.InterfaceType>>();
        if (!ok) { 
            // Not an interface type.
            continue;
        }
        slice<ptr<ast.Field>> methods = default;
        foreach (var (_, iMethod) in inter.Methods.List) { 
            // This is an interface, so there can be only one name.
            // TODO: Anonymous methods (embedding)
            if (len(iMethod.Names) == 0) {
                continue;
            }
            var name = iMethod.Names[0].Name;
            if (match(method, name)) {
                methods = append(methods, iMethod);
                found = true;
            }
        }        if (found) {
            pkg.Printf("type %s ", spec.Name);
            (inter.Methods.List, methods) = (methods, inter.Methods.List);            var err = format.Node(_addr_pkg.buf, pkg.fs, inter);
            if (err != null) {
                log.Fatal(err);
            }
            pkg.newlines(1); 
            // Restore the original methods.
            inter.Methods.List = methods;
        }
    }    return found;
}

// printFieldDoc prints the docs for matches of symbol.fieldName.
// It reports whether it found any field.
// Both symbol and fieldName must be non-empty or it returns false.
private static bool printFieldDoc(this ptr<Package> _addr_pkg, @string symbol, @string fieldName) {
    ref Package pkg = ref _addr_pkg.val;

    if (symbol == "" || fieldName == "") {
        return false;
    }
    var types = pkg.findTypes(symbol);
    if (types == null) {
        pkg.Fatalf("symbol %s is not a type in package %s installed in %q", symbol, pkg.name, pkg.build.ImportPath);
    }
    var found = false;
    nint numUnmatched = 0;
    foreach (var (_, typ) in types) { 
        // Type must be a struct.
        var spec = pkg.findTypeSpec(typ.Decl, typ.Name);
        ptr<ast.StructType> (structType, ok) = spec.Type._<ptr<ast.StructType>>();
        if (!ok) { 
            // Not a struct type.
            continue;
        }
        foreach (var (_, field) in structType.Fields.List) { 
            // TODO: Anonymous fields.
            foreach (var (_, name) in field.Names) {
                if (!match(fieldName, name.Name)) {
                    numUnmatched++;
                    continue;
                }
                if (!found) {
                    pkg.Printf("type %s struct {\n", typ.Name);
                }
                if (field.Doc != null) { 
                    // To present indented blocks in comments correctly, process the comment as
                    // a unit before adding the leading // to each line.
                    ref bytes.Buffer docBuf = ref heap(new bytes.Buffer(), out ptr<bytes.Buffer> _addr_docBuf);
                    doc.ToText(_addr_docBuf, field.Doc.Text(), "", indent, indentedWidth);
                    var scanner = bufio.NewScanner(_addr_docBuf);
                    while (scanner.Scan()) {
                        fmt.Fprintf(_addr_pkg.buf, "%s// %s\n", indent, scanner.Bytes());
                    }
                }
                var s = pkg.oneLineNode(field.Type);
                @string lineComment = "";
                if (field.Comment != null) {
                    lineComment = fmt.Sprintf("  %s", field.Comment.List[0].Text);
                }
                pkg.Printf("%s%s %s%s\n", indent, name, s, lineComment);
                found = true;
            }
        }
    }    if (found) {
        if (numUnmatched > 0) {
            pkg.Printf("\n    // ... other fields elided ...\n");
        }
        pkg.Printf("}\n");
    }
    return found;
}

// methodDoc prints the docs for matches of symbol.method.
private static bool methodDoc(this ptr<Package> _addr_pkg, @string symbol, @string method) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.printMethodDoc(symbol, method);
}

// fieldDoc prints the docs for matches of symbol.field.
private static bool fieldDoc(this ptr<Package> _addr_pkg, @string symbol, @string field) {
    ref Package pkg = ref _addr_pkg.val;

    return pkg.printFieldDoc(symbol, field);
}

// match reports whether the user's symbol matches the program's.
// A lower-case character in the user's string matches either case in the program's.
// The program string must be exported.
private static bool match(@string user, @string program) {
    if (!isExported(program)) {
        return false;
    }
    if (matchCase) {
        return user == program;
    }
    foreach (var (_, u) in user) {
        var (p, w) = utf8.DecodeRuneInString(program);
        program = program[(int)w..];
        if (u == p) {
            continue;
        }
        if (unicode.IsLower(u) && simpleFold(u) == simpleFold(p)) {
            continue;
        }
        return false;
    }    return program == "";
}

// simpleFold returns the minimum rune equivalent to r
// under Unicode-defined simple case folding.
private static int simpleFold(int r) {
    while (true) {
        var r1 = unicode.SimpleFold(r);
        if (r1 <= r) {
            return r1; // wrapped around, found min
        }
        r = r1;
    }
}

} // end main_package
