// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package doc extracts source code documentation from a Go AST.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using comment = go.doc.comment_package;
using token = go.token_package;
using strings = strings_package;
using go.doc;
using ꓸꓸꓸany = Span<any>;

partial class doc_package {

// Package is the documentation for an entire package.
[GoType] partial struct Package {
    public @string Doc;
    public @string Name;
    public @string ImportPath;
    public slice<@string> Imports;
    public slice<@string> Filenames;
    public map<@string, slice<ж<Note>>> Notes;
    // Deprecated: For backward compatibility Bugs is still populated,
    // but all new code should use Notes instead.
    public slice<@string> Bugs;
    // declarations
    public slice<ж<Value>> Consts;
    public slice<ж<Type>> Types;
    public slice<ж<Value>> Vars;
    public slice<ж<Func>> Funcs;
    // Examples is a sorted list of examples associated with
    // the package. Examples are extracted from _test.go files
    // provided to NewFromFiles.
    public slice<ж<Example>> Examples;
    internal map<@string, @string> importByName;
    internal map<@string, bool> syms;
}

// Value is the documentation for a (possibly grouped) var or const declaration.
[GoType] partial struct Value {
    public @string Doc;
    public slice<@string> Names; // var or const names in declaration order
    public ж<go.ast_package.GenDecl> Decl;
    internal nint order;
}

// Type is the documentation for a type declaration.
[GoType] partial struct Type {
    public @string Doc;
    public @string Name;
    public ж<go.ast_package.GenDecl> Decl;
    // associated declarations
    public slice<ж<Value>> Consts; // sorted list of constants of (mostly) this type
    public slice<ж<Value>> Vars; // sorted list of variables of (mostly) this type
    public slice<ж<Func>> Funcs; // sorted list of functions returning this type
    public slice<ж<Func>> Methods; // sorted list of methods (including embedded ones) of this type
    // Examples is a sorted list of examples associated with
    // this type. Examples are extracted from _test.go files
    // provided to NewFromFiles.
    public slice<ж<Example>> Examples;
}

// Func is the documentation for a func declaration.
[GoType] partial struct Func {
    public @string Doc;
    public @string Name;
    public ж<go.ast_package.FuncDecl> Decl;
    // methods
    // (for functions, these fields have the respective zero value)
    public @string Recv; // actual   receiver "T" or "*T" possibly followed by type parameters [P1, ..., Pn]
    public @string Orig; // original receiver "T" or "*T"
    public nint Level;   // embedding level; 0 means not embedded
    // Examples is a sorted list of examples associated with this
    // function or method. Examples are extracted from _test.go files
    // provided to NewFromFiles.
    public slice<ж<Example>> Examples;
}

// A Note represents a marked comment starting with "MARKER(uid): note body".
// Any note with a marker of 2 or more upper case [A-Z] letters and a uid of
// at least one character is recognized. The ":" following the uid is optional.
// Notes are collected in the Package.Notes map indexed by the notes marker.
[GoType] partial struct Note {
    public go.token_package.ΔPos Pos; // position range of the comment containing the marker
    public go.token_package.ΔPos End;
    public @string UID;   // uid found with the marker
    public @string Body;   // note body text
}

[GoType("num:nint")] partial struct Mode;

public static readonly Mode AllDecls = /* 1 << iota */ 1;
public static readonly Mode AllMethods = 2;
public static readonly Mode PreserveAST = 4;

// New computes the package documentation for the given package AST.
// New takes ownership of the AST pkg and may edit or overwrite it.
// To have the [Examples] fields populated, use [NewFromFiles] and include
// the package's _test.go files.
public static ж<Package> New(ж<ast.Package> Ꮡpkg, @string importPath, Mode mode) {
    ref var pkg = ref Ꮡpkg.val;

    reader r = default!;
    r.readPackage(Ꮡpkg, mode);
    r.computeMethodSets();
    r.cleanupTypes();
    var p = Ꮡ(new Package(
        Doc: r.doc,
        Name: pkg.Name,
        ImportPath: importPath,
        Imports: sortedKeys(r.imports),
        Filenames: r.filenames,
        Notes: r.notes,
        Bugs: noteBodies(r.notes["BUG"u8]),
        Consts: sortedValues(r.values, token.CONST),
        Types: sortedTypes(r.types, (Mode)(mode & AllMethods) != 0),
        Vars: sortedValues(r.values, token.VAR),
        Funcs: sortedFuncs(r.funcs, true),
        importByName: r.importByName,
        syms: new map<@string, bool>()
    ));
    p.collectValues((~p).Consts);
    p.collectValues((~p).Vars);
    p.collectTypes((~p).Types);
    p.collectFuncs((~p).Funcs);
    return p;
}

[GoRecv] internal static void collectValues(this ref Package p, slice<ж<Value>> values) {
    foreach (var (_, v) in values) {
        foreach (var (_, name) in (~v).Names) {
            p.syms[name] = true;
        }
    }
}

[GoRecv] internal static void collectTypes(this ref Package p, slice<ж<Type>> types) {
    foreach (var (_, t) in types) {
        if (p.syms[(~t).Name]) {
            // Shouldn't be any cycles but stop just in case.
            continue;
        }
        p.syms[(~t).Name] = true;
        p.collectValues((~t).Consts);
        p.collectValues((~t).Vars);
        p.collectFuncs((~t).Funcs);
        p.collectFuncs((~t).Methods);
    }
}

[GoRecv] internal static void collectFuncs(this ref Package p, slice<ж<Func>> funcs) {
    foreach (var (_, f) in funcs) {
        if ((~f).Recv != ""u8){
            @string r = strings.TrimPrefix((~f).Recv, "*"u8);
            {
                nint i = strings.IndexByte(r, (rune)'['); if (i >= 0) {
                    r = r[..(int)(i)];
                }
            }
            // remove type parameters
            p.syms[r + "."u8 + (~f).Name] = true;
        } else {
            p.syms[(~f).Name] = true;
        }
    }
}

// NewFromFiles computes documentation for a package.
//
// The package is specified by a list of *ast.Files and corresponding
// file set, which must not be nil.
// NewFromFiles uses all provided files when computing documentation,
// so it is the caller's responsibility to provide only the files that
// match the desired build context. "go/build".Context.MatchFile can
// be used for determining whether a file matches a build context with
// the desired GOOS and GOARCH values, and other build constraints.
// The import path of the package is specified by importPath.
//
// Examples found in _test.go files are associated with the corresponding
// type, function, method, or the package, based on their name.
// If the example has a suffix in its name, it is set in the
// [Example.Suffix] field. [Examples] with malformed names are skipped.
//
// Optionally, a single extra argument of type [Mode] can be provided to
// control low-level aspects of the documentation extraction behavior.
//
// NewFromFiles takes ownership of the AST files and may edit them,
// unless the PreserveAST Mode bit is on.
public static (ж<Package>, error) NewFromFiles(ж<token.FileSet> Ꮡfset, slice<ast.File> files, @string importPath, params ꓸꓸꓸany optsʗp) {
    var opts = optsʗp.slice();

    ref var fset = ref Ꮡfset.val;
    // Check for invalid API usage.
    if (fset == nil) {
        throw panic(fmt.Errorf("doc.NewFromFiles: no token.FileSet provided (fset == nil)"u8));
    }
    Mode mode = default!;
    switch (len(opts)) {
    case 0: {
        break;
    }
    case 1: {
        var (m, ok) = opts[0]._<Mode>(ᐧ);
        if (!ok) {
            // There can only be 0 or 1 options, so a simple switch works for now.
            // Nothing to do.
            throw panic(fmt.Errorf("doc.NewFromFiles: option argument type must be doc.Mode"u8));
        }
        mode = m;
        break;
    }
    default: {
        throw panic(fmt.Errorf("doc.NewFromFiles: there must not be more than 1 option argument"u8));
        break;
    }}

    // Collect .go and _test.go files.
    ast.File goFiles = new ast.File();
    
    slice<ast.File> testGoFiles = default!;
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in files) {
        var f = fset.File(files[i].Pos());
        if (f == nil) {
            return (default!, fmt.Errorf("file files[%d] is not found in the provided file set"u8, i));
        }
        {
            @string name = f.Name();
            switch (ᐧ) {
            case {} when strings.HasSuffix(name, ".go"u8) && !strings.HasSuffix(name, "_test.go"u8): {
                goFiles[name] = files[i];
                break;
            }
            case {} when strings.HasSuffix(name, "_test.go"u8): {
                testGoFiles = append(testGoFiles, files[i]);
                break;
            }
            default: {
                return (default!, fmt.Errorf("file files[%d] filename %q does not have a .go extension"u8, i, name));
            }}
        }

    }
    // TODO(dmitshur,gri): A relatively high level call to ast.NewPackage with a simpleImporter
    // ast.Importer implementation is made below. It might be possible to short-circuit and simplify.
    // Compute package documentation.
    (pkg, _) = ast.NewPackage(Ꮡfset, goFiles, simpleImporter, nil);
    // Ignore errors that can happen due to unresolved identifiers.
    var p = New(pkg, importPath, mode);
    classifyExamples(p, Examples(ᏑtestGoFiles.ꓸꓸꓸ));
    return (p, default!);
}

// simpleImporter returns a (dummy) package object named by the last path
// component of the provided package path (as is the convention for packages).
// This is sufficient to resolve package identifiers without doing an actual
// import. It never returns an error.
internal static (ж<ast.Object>, error) simpleImporter(ast.Object imports, @string path) {
    var pkg = imports[path];
    if (pkg == nil) {
        // note that strings.LastIndex returns -1 if there is no "/"
        pkg = ast.NewObj(ast.Pkg, path[(int)(strings.LastIndex(path, "/"u8) + 1)..]);
        pkg.val.Data = ast.NewScope(nil);
        // required by ast.NewPackage for dot-import
        imports[path] = pkg;
    }
    return (pkg, default!);
}

// lookupSym reports whether the package has a given symbol or method.
//
// If recv == "", HasSym reports whether the package has a top-level
// const, func, type, or var named name.
//
// If recv != "", HasSym reports whether the package has a type
// named recv with a method named name.
[GoRecv] internal static bool lookupSym(this ref Package p, @string recv, @string name) {
    if (recv != ""u8) {
        return p.syms[recv + "."u8 + name];
    }
    return p.syms[name];
}

// lookupPackage returns the import path identified by name
// in the given package. If name uniquely identifies a single import,
// then lookupPackage returns that import.
// If multiple packages are imported as name, importPath returns "", false.
// Otherwise, if name is the name of p itself, importPath returns "", true,
// to signal a reference to p.
// Otherwise, importPath returns "", false.
[GoRecv] internal static (@string importPath, bool ok) lookupPackage(this ref Package p, @string name) {
    @string importPath = default!;
    bool ok = default!;

    {
        @string path = p.importByName[name];
        var okΔ1 = p.importByName[name]; if (okΔ1) {
            if (path == ""u8) {
                return ("", false);
            }
            // multiple imports used the name
            return (path, true);
        }
    }
    // found import
    if (p.Name == name) {
        return ("", true);
    }
    // allow reference to this package
    return ("", false);
}

// unknown name

// Parser returns a doc comment parser configured
// for parsing doc comments from package p.
// Each call returns a new parser, so that the caller may
// customize it before use.
[GoRecv] public static ж<comment.Parser> Parser(this ref Package p) {
    return Ꮡ(new comment.Parser(
        LookupPackage: p.lookupPackage,
        LookupSym: p.lookupSym
    ));
}

// Printer returns a doc comment printer configured
// for printing doc comments from package p.
// Each call returns a new printer, so that the caller may
// customize it before use.
[GoRecv] public static ж<comment.Printer> Printer(this ref Package p) {
    // No customization today, but having p.Printer()
    // gives us flexibility in the future, and it is convenient for callers.
    return Ꮡ(new comment.Printer(nil));
}

// HTML returns formatted HTML for the doc comment text.
//
// To customize details of the HTML, use [Package.Printer]
// to obtain a [comment.Printer], and configure it
// before calling its HTML method.
[GoRecv] public static slice<byte> HTML(this ref Package p, @string text) {
    return p.Printer().HTML(p.Parser().Parse(text));
}

// Markdown returns formatted Markdown for the doc comment text.
//
// To customize details of the Markdown, use [Package.Printer]
// to obtain a [comment.Printer], and configure it
// before calling its Markdown method.
[GoRecv] public static slice<byte> Markdown(this ref Package p, @string text) {
    return p.Printer().Markdown(p.Parser().Parse(text));
}

// Text returns formatted text for the doc comment text,
// wrapped to 80 Unicode code points and using tabs for
// code block indentation.
//
// To customize details of the formatting, use [Package.Printer]
// to obtain a [comment.Printer], and configure it
// before calling its Text method.
[GoRecv] public static slice<byte> Text(this ref Package p, @string text) {
    return p.Printer().Text(p.Parser().Parse(text));
}

} // end doc_package
