// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is a modified copy of $GOROOT/src/go/internal/gcimporter/gcimporter.go,
// but it also contains the original source-based importer code for Go1.6.
// Once we stop supporting 1.6, we can remove that code.

// Package gcimporter provides various functions for reading
// gc-generated object files that can be used to implement the
// Importer interface defined by the Go 1.5 standard library package.
// package gcimporter -- go2cs converted at 2022 March 06 23:32:06 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\gcimporter.go
// import "golang.org/x/tools/go/internal/gcimporter"

using bufio = go.bufio_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;
using System;


namespace go.golang.org.x.tools.go.@internal;

public static partial class gcimporter_package {

    // debugging/development support
private static readonly var debug = false;



private static array<@string> pkgExts = new array<@string>(new @string[] { ".a", ".o" });

// FindPkg returns the filename and unique package id for an import
// path based on package information provided by build.Import (using
// the build.Default build.Context). A relative srcDir is interpreted
// relative to the current working directory.
// If no file was found, an empty filename is returned.
//
public static (@string, @string) FindPkg(@string path, @string srcDir) {
    @string filename = default;
    @string id = default;

    if (path == "") {
        return ;
    }
    @string noext = default;

    if (build.IsLocalImport(path)) 
        // "./x" -> "/this/directory/x.ext", "/this/directory/x"
        noext = filepath.Join(srcDir, path);
        id = noext;
    else if (filepath.IsAbs(path)) 
        // for completeness only - go/build.Import
        // does not support absolute imports
        // "/x" -> "/x.ext", "/x"
        noext = path;
        id = path;
    else 
        // "x" -> "$GOPATH/pkg/$GOOS_$GOARCH/x.ext", "x"
        // Don't require the source files to be present.
        {
            var (abs, err) = filepath.Abs(srcDir);

            if (err == null) { // see issue 14282
                srcDir = abs;

            }

        }

        var (bp, _) = build.Import(path, srcDir, build.FindOnly | build.AllowBinary);
        if (bp.PkgObj == "") {
            id = path; // make sure we have an id to print in error message
            return ;

        }
        noext = strings.TrimSuffix(bp.PkgObj, ".a");
        id = bp.ImportPath;
        if (false) { // for debugging
        if (path != id) {
            fmt.Printf("%s -> %s\n", path, id);
        }
    }
    foreach (var (_, ext) in pkgExts) {
        filename = noext + ext;
        {
            var (f, err) = os.Stat(filename);

            if (err == null && !f.IsDir()) {
                return ;
            }

        }

    }    filename = ""; // not found
    return ;

}

// ImportData imports a package by reading the gc-generated export data,
// adds the corresponding package object to the packages map indexed by id,
// and returns the object.
//
// The packages map must contains all packages already imported. The data
// reader position must be the beginning of the export data section. The
// filename is only used in error messages.
//
// If packages[id] contains the completely imported package, that package
// can be used directly, and there is no need to call this function (but
// there is also no harm but for extra time used).
//
public static (ptr<types.Package>, error) ImportData(map<@string, ptr<types.Package>> packages, @string filename, @string id, io.Reader data) => func((defer, panic, _) => {
    ptr<types.Package> pkg = default!;
    error err = default!;
 
    // support for parser error handling
    defer(() => {
        switch (recover().type()) {
            case 
                break;
            case importError r:
                err = r;
                break;
            default:
            {
                var r = recover().type();
                panic(r); // internal error
                break;
            }
        }

    }());

    parser p = default;
    p.init(filename, id, data, packages);
    pkg = p.parseExport();

    return ;

});

// Import imports a gc-generated package given its import path and srcDir, adds
// the corresponding package object to the packages map, and returns the object.
// The packages map must contain all packages already imported.
//
public static (ptr<types.Package>, error) Import(map<@string, ptr<types.Package>> packages, @string path, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) => func((defer, _, _) => {
    ptr<types.Package> pkg = default!;
    error err = default!;

    io.ReadCloser rc = default;
    @string filename = default;    @string id = default;

    if (lookup != null) { 
        // With custom lookup specified, assume that caller has
        // converted path to a canonical import path for use in the map.
        if (path == "unsafe") {
            return (_addr_types.Unsafe!, error.As(null!)!);
        }
        id = path; 

        // No need to re-import if the package was imported completely before.
        pkg = packages[id];

        if (pkg != null && pkg.Complete()) {
            return ;
        }
        var (f, err) = lookup(path);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        rc = f;

    }
    else
 {
        filename, id = FindPkg(path, srcDir);
        if (filename == "") {
            if (path == "unsafe") {
                return (_addr_types.Unsafe!, error.As(null!)!);
            }
            return (_addr_null!, error.As(fmt.Errorf("can't find import: %q", id))!);
        }
        pkg = packages[id];

        if (pkg != null && pkg.Complete()) {
            return ;
        }
        (f, err) = os.Open(filename);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        defer(() => {
            if (err != null) { 
                // add file name to error
                err = fmt.Errorf("%s: %v", filename, err);

            }

        }());
        rc = f;

    }
    defer(rc.Close());

    @string hdr = default;
    var buf = bufio.NewReader(rc);
    hdr, err = FindExportData(buf);

    if (err != null) {
        return ;
    }
    switch (hdr) {
        case "$$\n": 
            // Work-around if we don't have a filename; happens only if lookup != nil.
            // Either way, the filename is only needed for importer error messages, so
            // this is fine.
            if (filename == "") {
                filename = path;
            }
            return _addr_ImportData(packages, filename, id, buf)!;

            break;
        case "$$B\n": 
                   slice<byte> data = default;
                   data, err = ioutil.ReadAll(buf);
                   if (err != null) {
                       break;
                   }
                   var fset = token.NewFileSet(); 

                   // The indexed export format starts with an 'i'; the older
                   // binary export format starts with a 'c', 'd', or 'v'
                   // (from "version"). Select appropriate importer.
                   if (len(data) > 0 && data[0] == 'i') {
                       _, pkg, err = IImportData(fset, packages, data[(int)1..], id);
                   }
                   else
            {
                       _, pkg, err = BImportData(fset, packages, data, id);
                   }
            break;
        default: 
            err = fmt.Errorf("unknown export data header: %q", hdr);
            break;
    }

    return ;

});

// ----------------------------------------------------------------------------
// Parser

// TODO(gri) Imported objects don't have position information.
//           Ideally use the debug table line info; alternatively
//           create some fake position (or the position of the
//           import). That way error messages referring to imported
//           objects can print meaningful information.

// parser parses the exports inside a gc compiler-produced
// object/archive file and populates its scope with the results.
private partial struct parser {
    public scanner.Scanner scanner;
    public int tok; // current token
    public @string lit; // literal string; only valid for Ident, Int, String tokens
    public @string id; // package id of imported package
    public map<@string, ptr<types.Package>> sharedPkgs; // package id -> package object (across importer)
    public map<@string, ptr<types.Package>> localPkgs; // package id -> package object (just this package)
}

private static void init(this ptr<parser> _addr_p, @string filename, @string id, io.Reader src, map<@string, ptr<types.Package>> packages) {
    ref parser p = ref _addr_p.val;

    p.scanner.Init(src);
    p.scanner.Error = (_, msg) => {
        p.error(msg);
    };
    p.scanner.Mode = scanner.ScanIdents | scanner.ScanInts | scanner.ScanChars | scanner.ScanStrings | scanner.ScanComments | scanner.SkipComments;
    p.scanner.Whitespace = 1 << (int)('\t') | 1 << (int)(' ');
    p.scanner.Filename = filename; // for good error messages
    p.next();
    p.id = id;
    p.sharedPkgs = packages;
    if (debug) { 
        // check consistency of packages map
        foreach (var (_, pkg) in packages) {
            if (pkg.Name() == "") {
                fmt.Printf("no package name for %s\n", pkg.Path());
            }
        }
    }
}

private static void next(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.tok = p.scanner.Scan();

    if (p.tok == scanner.Ident || p.tok == scanner.Int || p.tok == scanner.Char || p.tok == scanner.String || p.tok == '·') 
        p.lit = p.scanner.TokenText();
    else 
        p.lit = "";
        if (debug) {
        fmt.Printf("%s: %q -> %q\n", scanner.TokenString(p.tok), p.scanner.TokenText(), p.lit);
    }
}

private static ptr<types.TypeName> declTypeName(ptr<types.Package> _addr_pkg, @string name) {
    ref types.Package pkg = ref _addr_pkg.val;

    var scope = pkg.Scope();
    {
        var obj__prev1 = obj;

        var obj = scope.Lookup(name);

        if (obj != null) {
            return obj._<ptr<types.TypeName>>();
        }
        obj = obj__prev1;

    }

    obj = types.NewTypeName(token.NoPos, pkg, name, null); 
    // a named type may be referred to before the underlying type
    // is known - set it up
    types.NewNamed(obj, null, null);
    scope.Insert(obj);
    return _addr_obj!;

}

// ----------------------------------------------------------------------------
// Error handling

// Internal errors are boxed as importErrors.
private partial struct importError {
    public scanner.Position pos;
    public error err;
}

private static @string Error(this importError e) {
    return fmt.Sprintf("import error %s (byte offset = %d): %s", e.pos, e.pos.Offset, e.err);
}

private static void error(this ptr<parser> _addr_p, object err) => func((_, panic, _) => {
    ref parser p = ref _addr_p.val;

    {
        @string (s, ok) = err._<@string>();

        if (ok) {
            err = errors.New(s);
        }
    } 
    // panic with a runtime.Error if err is not an error
    panic(new importError(p.scanner.Pos(),err.(error)));

});

private static void errorf(this ptr<parser> _addr_p, @string format, params object[] args) {
    args = args.Clone();
    ref parser p = ref _addr_p.val;

    p.error(fmt.Sprintf(format, args));
}

private static @string expect(this ptr<parser> _addr_p, int tok) {
    ref parser p = ref _addr_p.val;

    var lit = p.lit;
    if (p.tok != tok) {
        p.errorf("expected %s, got %s (%s)", scanner.TokenString(tok), scanner.TokenString(p.tok), lit);
    }
    p.next();
    return lit;

}

private static void expectSpecial(this ptr<parser> _addr_p, @string tok) {
    ref parser p = ref _addr_p.val;

    char sep = 'x'; // not white space
    nint i = 0;
    while (i < len(tok) && p.tok == rune(tok[i]) && sep > ' ') {
        sep = p.scanner.Peek(); // if sep <= ' ', there is white space before the next token
        p.next();
        i++;

    }
    if (i < len(tok)) {
        p.errorf("expected %q, got %q", tok, tok[(int)0..(int)i]);
    }
}

private static void expectKeyword(this ptr<parser> _addr_p, @string keyword) {
    ref parser p = ref _addr_p.val;

    var lit = p.expect(scanner.Ident);
    if (lit != keyword) {
        p.errorf("expected keyword %s, got %q", keyword, lit);
    }
}

// ----------------------------------------------------------------------------
// Qualified and unqualified names

// PackageId = string_lit .
//
private static @string parsePackageID(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    var (id, err) = strconv.Unquote(p.expect(scanner.String));
    if (err != null) {
        p.error(err);
    }
    if (id == "") {
        id = p.id;
    }
    return id;

}

// PackageName = ident .
//
private static @string parsePackageName(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    return p.expect(scanner.Ident);
}

// dotIdentifier = ( ident | '·' ) { ident | int | '·' } .
private static @string parseDotIdent(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    @string ident = "";
    if (p.tok != scanner.Int) {
        char sep = 'x'; // not white space
        while ((p.tok == scanner.Ident || p.tok == scanner.Int || p.tok == '·') && sep > ' ') {
            ident += p.lit;
            sep = p.scanner.Peek(); // if sep <= ' ', there is white space before the next token
            p.next();

        }

    }
    if (ident == "") {
        p.expect(scanner.Ident); // use expect() for error handling
    }
    return ident;

}

// QualifiedName = "@" PackageId "." ( "?" | dotIdentifier ) .
//
private static (@string, @string) parseQualifiedName(this ptr<parser> _addr_p) {
    @string id = default;
    @string name = default;
    ref parser p = ref _addr_p.val;

    p.expect('@');
    id = p.parsePackageID();
    p.expect('.'); 
    // Per rev f280b8a485fd (10/2/2013), qualified names may be used for anonymous fields.
    if (p.tok == '?') {
        p.next();
    }
    else
 {
        name = p.parseDotIdent();
    }
    return ;

}

// getPkg returns the package for a given id. If the package is
// not found, create the package and add it to the p.localPkgs
// and p.sharedPkgs maps. name is the (expected) name of the
// package. If name == "", the package name is expected to be
// set later via an import clause in the export data.
//
// id identifies a package, usually by a canonical package path like
// "encoding/json" but possibly by a non-canonical import path like
// "./json".
//
private static ptr<types.Package> getPkg(this ptr<parser> _addr_p, @string id, @string name) {
    ref parser p = ref _addr_p.val;
 
    // package unsafe is not in the packages maps - handle explicitly
    if (id == "unsafe") {
        return _addr_types.Unsafe!;
    }
    var pkg = p.localPkgs[id];
    if (pkg == null) { 
        // first import of id from this package
        pkg = p.sharedPkgs[id];
        if (pkg == null) { 
            // first import of id by this importer;
            // add (possibly unnamed) pkg to shared packages
            pkg = types.NewPackage(id, name);
            p.sharedPkgs[id] = pkg;

        }
        if (p.localPkgs == null) {
            p.localPkgs = make_map<@string, ptr<types.Package>>();
        }
        p.localPkgs[id] = pkg;

    }
    else if (name != "") { 
        // package exists already and we have an expected package name;
        // make sure names match or set package name if necessary
        {
            var pname = pkg.Name();

            if (pname == "") {
                pkg.SetName(name);
            }
            else if (pname != name) {
                p.errorf("%s package name mismatch: %s (given) vs %s (expected)", id, pname, name);
            }


        }

    }
    return _addr_pkg!;

}

// parseExportedName is like parseQualifiedName, but
// the package id is resolved to an imported *types.Package.
//
private static (ptr<types.Package>, @string) parseExportedName(this ptr<parser> _addr_p) {
    ptr<types.Package> pkg = default!;
    @string name = default;
    ref parser p = ref _addr_p.val;

    var (id, name) = p.parseQualifiedName();
    pkg = p.getPkg(id, "");
    return ;
}

// ----------------------------------------------------------------------------
// Types

// BasicType = identifier .
//
private static types.Type parseBasicType(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    var id = p.expect(scanner.Ident);
    var obj = types.Universe.Lookup(id);
    {
        var obj__prev1 = obj;

        ptr<types.TypeName> (obj, ok) = obj._<ptr<types.TypeName>>();

        if (ok) {
            return obj.Type();
        }
        obj = obj__prev1;

    }

    p.errorf("not a basic type: %s", id);
    return null;

}

// ArrayType = "[" int_lit "]" Type .
//
private static types.Type parseArrayType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;
 
    // "[" already consumed and lookahead known not to be "]"
    var lit = p.expect(scanner.Int);
    p.expect(']');
    var elem = p.parseType(parent);
    var (n, err) = strconv.ParseInt(lit, 10, 64);
    if (err != null) {
        p.error(err);
    }
    return types.NewArray(elem, n);

}

// MapType = "map" "[" Type "]" Type .
//
private static types.Type parseMapType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    p.expectKeyword("map");
    p.expect('[');
    var key = p.parseType(parent);
    p.expect(']');
    var elem = p.parseType(parent);
    return types.NewMap(key, elem);
}

// Name = identifier | "?" | QualifiedName .
//
// For unqualified and anonymous names, the returned package is the parent
// package unless parent == nil, in which case the returned package is the
// package being imported. (The parent package is not nil if the the name
// is an unqualified struct field or interface method name belonging to a
// type declared in another package.)
//
// For qualified names, the returned package is nil (and not created if
// it doesn't exist yet) unless materializePkg is set (which creates an
// unnamed package with valid package path). In the latter case, a
// subsequent import clause is expected to provide a name for the package.
//
private static (ptr<types.Package>, @string) parseName(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent, bool materializePkg) {
    ptr<types.Package> pkg = default!;
    @string name = default;
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    pkg = parent;
    if (pkg == null) {
        pkg = p.sharedPkgs[p.id];
    }

    if (p.tok == scanner.Ident) 
        name = p.lit;
        p.next();
    else if (p.tok == '?') 
        // anonymous
        p.next();
    else if (p.tok == '@') 
        // exported name prefixed with package path
        pkg = null;
        @string id = default;
        id, name = p.parseQualifiedName();
        if (materializePkg) {
            pkg = p.getPkg(id, "");
        }
    else 
        p.error("name expected");
        return ;

}

private static types.Type deref(types.Type typ) {
    {
        ptr<types.Pointer> (p, _) = typ._<ptr<types.Pointer>>();

        if (p != null) {
            return p.Elem();
        }
    }

    return typ;

}

// Field = Name Type [ string_lit ] .
//
private static (ptr<types.Var>, @string) parseField(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ptr<types.Var> _p0 = default!;
    @string _p0 = default;
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    var (pkg, name) = p.parseName(parent, true);

    if (name == "_") { 
        // Blank fields should be package-qualified because they
        // are unexported identifiers, but gc does not qualify them.
        // Assuming that the ident belongs to the current package
        // causes types to change during re-exporting, leading
        // to spurious "can't assign A to B" errors from go/types.
        // As a workaround, pretend all blank fields belong
        // to the same unique dummy package.
        const @string blankpkg = "<_>";

        pkg = p.getPkg(blankpkg, blankpkg);

    }
    var typ = p.parseType(parent);
    var anonymous = false;
    if (name == "") { 
        // anonymous field - typ must be T or *T and T must be a type name
        switch (deref(typ).type()) {
            case ptr<types.Basic> typ:
                pkg = null; // objects defined in Universe scope have no package
                name = typ.Name();
                break;
            case ptr<types.Named> typ:
                name = typ.Obj().Name();
                break;
            default:
            {
                var typ = deref(typ).type();
                p.errorf("anonymous field expected");
                break;
            }
        }
        anonymous = true;

    }
    @string tag = "";
    if (p.tok == scanner.String) {
        var s = p.expect(scanner.String);
        error err = default!;
        tag, err = strconv.Unquote(s);
        if (err != null) {
            p.errorf("invalid struct tag %s: %s", s, err);
        }
    }
    return (_addr_types.NewField(token.NoPos, pkg, name, typ, anonymous)!, tag);

}

// StructType = "struct" "{" [ FieldList ] "}" .
// FieldList  = Field { ";" Field } .
//
private static types.Type parseStructType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    slice<ptr<types.Var>> fields = default;
    slice<@string> tags = default;

    p.expectKeyword("struct");
    p.expect('{');
    for (nint i = 0; p.tok != '}' && p.tok != scanner.EOF; i++) {
        if (i > 0) {
            p.expect(';');
        }
        var (fld, tag) = p.parseField(parent);
        if (tag != "" && tags == null) {
            tags = make_slice<@string>(i);
        }
        if (tags != null) {
            tags = append(tags, tag);
        }
        fields = append(fields, fld);

    }
    p.expect('}');

    return types.NewStruct(fields, tags);

}

// Parameter = ( identifier | "?" ) [ "..." ] Type [ string_lit ] .
//
private static (ptr<types.Var>, bool) parseParameter(this ptr<parser> _addr_p) {
    ptr<types.Var> par = default!;
    bool isVariadic = default;
    ref parser p = ref _addr_p.val;

    var (_, name) = p.parseName(null, false); 
    // remove gc-specific parameter numbering
    {
        var i = strings.Index(name, "·");

        if (i >= 0) {
            name = name[..(int)i];
        }
    }

    if (p.tok == '.') {
        p.expectSpecial("...");
        isVariadic = true;
    }
    var typ = p.parseType(null);
    if (isVariadic) {
        typ = types.NewSlice(typ);
    }
    if (p.tok == scanner.String) {
        p.next();
    }
    par = types.NewVar(token.NoPos, null, name, typ);
    return ;

}

// Parameters    = "(" [ ParameterList ] ")" .
// ParameterList = { Parameter "," } Parameter .
//
private static (slice<ptr<types.Var>>, bool) parseParameters(this ptr<parser> _addr_p) {
    slice<ptr<types.Var>> list = default;
    bool isVariadic = default;
    ref parser p = ref _addr_p.val;

    p.expect('(');
    while (p.tok != ')' && p.tok != scanner.EOF) {
        if (len(list) > 0) {
            p.expect(',');
        }
        var (par, variadic) = p.parseParameter();
        list = append(list, par);
        if (variadic) {
            if (isVariadic) {
                p.error("... not on final argument");
            }
            isVariadic = true;
        }
    }
    p.expect(')');

    return ;

}

// Signature = Parameters [ Result ] .
// Result    = Type | Parameters .
//
private static ptr<types.Signature> parseSignature(this ptr<parser> _addr_p, ptr<types.Var> _addr_recv) {
    ref parser p = ref _addr_p.val;
    ref types.Var recv = ref _addr_recv.val;

    var (params, isVariadic) = p.parseParameters(); 

    // optional result type
    slice<ptr<types.Var>> results = default;
    if (p.tok == '(') {
        bool variadic = default;
        results, variadic = p.parseParameters();
        if (variadic) {
            p.error("... not permitted on result type");
        }
    }
    return _addr_types.NewSignature(recv, types.NewTuple(params), types.NewTuple(results), isVariadic)!;

}

// InterfaceType = "interface" "{" [ MethodList ] "}" .
// MethodList    = Method { ";" Method } .
// Method        = Name Signature .
//
// The methods of embedded interfaces are always "inlined"
// by the compiler and thus embedded interfaces are never
// visible in the export data.
//
private static types.Type parseInterfaceType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    slice<ptr<types.Func>> methods = default;

    p.expectKeyword("interface");
    p.expect('{');
    for (nint i = 0; p.tok != '}' && p.tok != scanner.EOF; i++) {
        if (i > 0) {
            p.expect(';');
        }
        var (pkg, name) = p.parseName(parent, true);
        var sig = p.parseSignature(null);
        methods = append(methods, types.NewFunc(token.NoPos, pkg, name, sig));

    }
    p.expect('}'); 

    // Complete requires the type's embedded interfaces to be fully defined,
    // but we do not define any
    return newInterface(methods, null).Complete();

}

// ChanType = ( "chan" [ "<-" ] | "<-" "chan" ) Type .
//
private static types.Type parseChanType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;

    var dir = types.SendRecv;
    if (p.tok == scanner.Ident) {
        p.expectKeyword("chan");
        if (p.tok == '<') {
            p.expectSpecial("<-");
            dir = types.SendOnly;
        }
    }
    else
 {
        p.expectSpecial("<-");
        p.expectKeyword("chan");
        dir = types.RecvOnly;
    }
    var elem = p.parseType(parent);
    return types.NewChan(dir, elem);

}

// Type =
//    BasicType | TypeName | ArrayType | SliceType | StructType |
//      PointerType | FuncType | InterfaceType | MapType | ChanType |
//      "(" Type ")" .
//
// BasicType   = ident .
// TypeName    = ExportedName .
// SliceType   = "[" "]" Type .
// PointerType = "*" Type .
// FuncType    = "func" Signature .
//
private static types.Type parseType(this ptr<parser> _addr_p, ptr<types.Package> _addr_parent) {
    ref parser p = ref _addr_p.val;
    ref types.Package parent = ref _addr_parent.val;


    if (p.tok == scanner.Ident) 
        switch (p.lit) {
            case "struct": 
                return p.parseStructType(parent);
                break;
            case "func": 
                // FuncType
                p.next();
                return p.parseSignature(null);
                break;
            case "interface": 
                return p.parseInterfaceType(parent);
                break;
            case "map": 
                return p.parseMapType(parent);
                break;
            case "chan": 
                return p.parseChanType(parent);
                break;
            default: 
                return p.parseBasicType();
                break;
        }
    else if (p.tok == '@') 
        // TypeName
        var (pkg, name) = p.parseExportedName();
        return declTypeName(_addr_pkg, name).Type();
    else if (p.tok == '[') 
        p.next(); // look ahead
        if (p.tok == ']') { 
            // SliceType
            p.next();
            return types.NewSlice(p.parseType(parent));

        }
        return p.parseArrayType(parent);
    else if (p.tok == '*') 
        // PointerType
        p.next();
        return types.NewPointer(p.parseType(parent));
    else if (p.tok == '<') 
        return p.parseChanType(parent);
    else if (p.tok == '(') 
        // "(" Type ")"
        p.next();
        var typ = p.parseType(parent);
        p.expect(')');
        return typ;
        p.errorf("expected type, got %s (%q)", scanner.TokenString(p.tok), p.lit);
    return null;

}

// ----------------------------------------------------------------------------
// Declarations

// ImportDecl = "import" PackageName PackageId .
//
private static void parseImportDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.expectKeyword("import");
    var name = p.parsePackageName();
    p.getPkg(p.parsePackageID(), name);
}

// int_lit = [ "+" | "-" ] { "0" ... "9" } .
//
private static @string parseInt(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    @string s = "";
    switch (p.tok) {
        case '-': 
            s = "-";
            p.next();
            break;
        case '+': 
            p.next();
            break;
    }
    return s + p.expect(scanner.Int);

}

// number = int_lit [ "p" int_lit ] .
//
private static (ptr<types.Basic>, constant.Value) parseNumber(this ptr<parser> _addr_p) => func((_, panic, _) => {
    ptr<types.Basic> typ = default!;
    constant.Value val = default;
    ref parser p = ref _addr_p.val;
 
    // mantissa
    var mant = constant.MakeFromLiteral(p.parseInt(), token.INT, 0);
    if (mant == null) {
        panic("invalid mantissa");
    }
    if (p.lit == "p") { 
        // exponent (base 2)
        p.next();
        var (exp, err) = strconv.ParseInt(p.parseInt(), 10, 0);
        if (err != null) {
            p.error(err);
        }
        if (exp < 0) {
            var denom = constant.MakeInt64(1);
            denom = constant.Shift(denom, token.SHL, uint(-exp));
            typ = types.Typ[types.UntypedFloat];
            val = constant.BinaryOp(mant, token.QUO, denom);
            return ;
        }
        if (exp > 0) {
            mant = constant.Shift(mant, token.SHL, uint(exp));
        }
        typ = types.Typ[types.UntypedFloat];
        val = mant;
        return ;

    }
    typ = types.Typ[types.UntypedInt];
    val = mant;
    return ;

});

// ConstDecl   = "const" ExportedName [ Type ] "=" Literal .
// Literal     = bool_lit | int_lit | float_lit | complex_lit | rune_lit | string_lit .
// bool_lit    = "true" | "false" .
// complex_lit = "(" float_lit "+" float_lit "i" ")" .
// rune_lit    = "(" int_lit "+" int_lit ")" .
// string_lit  = `"` { unicode_char } `"` .
//
private static void parseConstDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.expectKeyword("const");
    var (pkg, name) = p.parseExportedName();

    types.Type typ0 = default;
    if (p.tok != '=') { 
        // constant types are never structured - no need for parent type
        typ0 = p.parseType(null);

    }
    p.expect('=');
    types.Type typ = default;
    constant.Value val = default;

    if (p.tok == scanner.Ident) 
        // bool_lit
        if (p.lit != "true" && p.lit != "false") {
            p.error("expected true or false");
        }
        typ = types.Typ[types.UntypedBool];
        val = constant.MakeBool(p.lit == "true");
        p.next();
    else if (p.tok == '-' || p.tok == scanner.Int) 
        // int_lit
        typ, val = p.parseNumber();
    else if (p.tok == '(') 
        // complex_lit or rune_lit
        p.next();
        if (p.tok == scanner.Char) {
            p.next();
            p.expect('+');
            typ = types.Typ[types.UntypedRune];
            _, val = p.parseNumber();
            p.expect(')');
            break;
        }
        var (_, re) = p.parseNumber();
        p.expect('+');
        var (_, im) = p.parseNumber();
        p.expectKeyword("i");
        p.expect(')');
        typ = types.Typ[types.UntypedComplex];
        val = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
    else if (p.tok == scanner.Char) 
        // rune_lit
        typ = types.Typ[types.UntypedRune];
        val = constant.MakeFromLiteral(p.lit, token.CHAR, 0);
        p.next();
    else if (p.tok == scanner.String) 
        // string_lit
        typ = types.Typ[types.UntypedString];
        val = constant.MakeFromLiteral(p.lit, token.STRING, 0);
        p.next();
    else 
        p.errorf("expected literal got %s", scanner.TokenString(p.tok));
        if (typ0 == null) {
        typ0 = typ;
    }
    pkg.Scope().Insert(types.NewConst(token.NoPos, pkg, name, typ0, val));

}

// TypeDecl = "type" ExportedName Type .
//
private static void parseTypeDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.expectKeyword("type");
    var (pkg, name) = p.parseExportedName();
    var obj = declTypeName(_addr_pkg, name); 

    // The type object may have been imported before and thus already
    // have a type associated with it. We still need to parse the type
    // structure, but throw it away if the object already has a type.
    // This ensures that all imports refer to the same type object for
    // a given type declaration.
    var typ = p.parseType(pkg);

    {
        ptr<types.Named> name = obj.Type()._<ptr<types.Named>>();

        if (name.Underlying() == null) {
            name.SetUnderlying(typ);
        }
    }

}

// VarDecl = "var" ExportedName Type .
//
private static void parseVarDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.expectKeyword("var");
    var (pkg, name) = p.parseExportedName();
    var typ = p.parseType(pkg);
    pkg.Scope().Insert(types.NewVar(token.NoPos, pkg, name, typ));
}

// Func = Signature [ Body ] .
// Body = "{" ... "}" .
//
private static ptr<types.Signature> parseFunc(this ptr<parser> _addr_p, ptr<types.Var> _addr_recv) {
    ref parser p = ref _addr_p.val;
    ref types.Var recv = ref _addr_recv.val;

    var sig = p.parseSignature(recv);
    if (p.tok == '{') {
        p.next();
        for (nint i = 1; i > 0; p.next()) {
            switch (p.tok) {
                case '{': 
                    i++;
                    break;
                case '}': 
                    i--;
                    break;
            }

        }

    }
    return _addr_sig!;

}

// MethodDecl = "func" Receiver Name Func .
// Receiver   = "(" ( identifier | "?" ) [ "*" ] ExportedName ")" .
//
private static void parseMethodDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;
 
    // "func" already consumed
    p.expect('(');
    var (recv, _) = p.parseParameter(); // receiver
    p.expect(')'); 

    // determine receiver base type object
    ptr<types.Named> @base = deref(recv.Type())._<ptr<types.Named>>(); 

    // parse method name, signature, and possibly inlined body
    var (_, name) = p.parseName(null, false);
    var sig = p.parseFunc(recv); 

    // methods always belong to the same package as the base type object
    var pkg = @base.Obj().Pkg(); 

    // add method to type unless type was imported before
    // and method exists already
    // TODO(gri) This leads to a quadratic algorithm - ok for now because method counts are small.
    @base.AddMethod(types.NewFunc(token.NoPos, pkg, name, sig));

}

// FuncDecl = "func" ExportedName Func .
//
private static void parseFuncDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;
 
    // "func" already consumed
    var (pkg, name) = p.parseExportedName();
    var typ = p.parseFunc(null);
    pkg.Scope().Insert(types.NewFunc(token.NoPos, pkg, name, typ));

}

// Decl = [ ImportDecl | ConstDecl | TypeDecl | VarDecl | FuncDecl | MethodDecl ] "\n" .
//
private static void parseDecl(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    if (p.tok == scanner.Ident) {
        switch (p.lit) {
            case "import": 
                p.parseImportDecl();
                break;
            case "const": 
                p.parseConstDecl();
                break;
            case "type": 
                p.parseTypeDecl();
                break;
            case "var": 
                p.parseVarDecl();
                break;
            case "func": 
                           p.next(); // look ahead
                           if (p.tok == '(') {
                               p.parseMethodDecl();
                           }
                           else
                {
                               p.parseFuncDecl();
                           }

                break;
        }

    }
    p.expect('\n');

}

// ----------------------------------------------------------------------------
// Export

// Export        = "PackageClause { Decl } "$$" .
// PackageClause = "package" PackageName [ "safe" ] "\n" .
//
private static ptr<types.Package> parseExport(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.expectKeyword("package");
    var name = p.parsePackageName();
    if (p.tok == scanner.Ident && p.lit == "safe") { 
        // package was compiled with -u option - ignore
        p.next();

    }
    p.expect('\n');

    var pkg = p.getPkg(p.id, name);

    while (p.tok != '$' && p.tok != scanner.EOF) {
        p.parseDecl();
    }

    {
        var ch = p.scanner.Peek();

        if (p.tok != '$' || ch != '$') { 
            // don't call next()/expect() since reading past the
            // export data may cause scanner errors (e.g. NUL chars)
            p.errorf("expected '$$', got %s %c", scanner.TokenString(p.tok), ch);

        }
    }


    {
        var n = p.scanner.ErrorCount;

        if (n != 0) {
            p.errorf("expected no scanner errors, got %d", n);
        }
    } 

    // Record all locally referenced packages as imports.
    slice<ptr<types.Package>> imports = default;
    foreach (var (id, pkg2) in p.localPkgs) {
        if (pkg2.Name() == "") {
            p.errorf("%s package has no name", id);
        }
        if (id == p.id) {
            continue; // avoid self-edge
        }
        imports = append(imports, pkg2);

    }    sort.Sort(byPath(imports));
    pkg.SetImports(imports); 

    // package was imported completely and without errors
    pkg.MarkComplete();

    return _addr_pkg!;

}

private partial struct byPath { // : slice<ptr<types.Package>>
}

private static nint Len(this byPath a) {
    return len(a);
}
private static void Swap(this byPath a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}
private static bool Less(this byPath a, nint i, nint j) {
    return a[i].Path() < a[j].Path();
}

} // end gcimporter_package
