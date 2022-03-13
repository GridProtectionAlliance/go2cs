// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package doc extracts source code documentation from a Go AST.

// package doc -- go2cs converted at 2022 March 13 05:52:32 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Program Files\Go\src\go\doc\doc.go
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using strings = strings_package;


// Package is the documentation for an entire package.

public static partial class doc_package {

public partial struct Package {
    public @string Doc;
    public @string Name;
    public @string ImportPath;
    public slice<@string> Imports;
    public slice<@string> Filenames;
    public map<@string, slice<ptr<Note>>> Notes; // Deprecated: For backward compatibility Bugs is still populated,
// but all new code should use Notes instead.
    public slice<@string> Bugs; // declarations
    public slice<ptr<Value>> Consts;
    public slice<ptr<Type>> Types;
    public slice<ptr<Value>> Vars;
    public slice<ptr<Func>> Funcs; // Examples is a sorted list of examples associated with
// the package. Examples are extracted from _test.go files
// provided to NewFromFiles.
    public slice<ptr<Example>> Examples;
}

// Value is the documentation for a (possibly grouped) var or const declaration.
public partial struct Value {
    public @string Doc;
    public slice<@string> Names; // var or const names in declaration order
    public ptr<ast.GenDecl> Decl;
    public nint order;
}

// Type is the documentation for a type declaration.
public partial struct Type {
    public @string Doc;
    public @string Name;
    public ptr<ast.GenDecl> Decl; // associated declarations
    public slice<ptr<Value>> Consts; // sorted list of constants of (mostly) this type
    public slice<ptr<Value>> Vars; // sorted list of variables of (mostly) this type
    public slice<ptr<Func>> Funcs; // sorted list of functions returning this type
    public slice<ptr<Func>> Methods; // sorted list of methods (including embedded ones) of this type

// Examples is a sorted list of examples associated with
// this type. Examples are extracted from _test.go files
// provided to NewFromFiles.
    public slice<ptr<Example>> Examples;
}

// Func is the documentation for a func declaration.
public partial struct Func {
    public @string Doc;
    public @string Name;
    public ptr<ast.FuncDecl> Decl; // methods
// (for functions, these fields have the respective zero value)
    public @string Recv; // actual   receiver "T" or "*T"
    public @string Orig; // original receiver "T" or "*T"
    public nint Level; // embedding level; 0 means not embedded

// Examples is a sorted list of examples associated with this
// function or method. Examples are extracted from _test.go files
// provided to NewFromFiles.
    public slice<ptr<Example>> Examples;
}

// A Note represents a marked comment starting with "MARKER(uid): note body".
// Any note with a marker of 2 or more upper case [A-Z] letters and a uid of
// at least one character is recognized. The ":" following the uid is optional.
// Notes are collected in the Package.Notes map indexed by the notes marker.
public partial struct Note {
    public token.Pos Pos; // position range of the comment containing the marker
    public token.Pos End; // position range of the comment containing the marker
    public @string UID; // uid found with the marker
    public @string Body; // note body text
}

// Mode values control the operation of New and NewFromFiles.
public partial struct Mode { // : nint
}

 
// AllDecls says to extract documentation for all package-level
// declarations, not just exported ones.
public static readonly Mode AllDecls = 1 << (int)(iota); 

// AllMethods says to show all embedded methods, not just the ones of
// invisible (unexported) anonymous fields.
public static readonly var AllMethods = 0; 

// PreserveAST says to leave the AST unmodified. Originally, pieces of
// the AST such as function bodies were nil-ed out to save memory in
// godoc, but not all programs want that behavior.
public static readonly var PreserveAST = 1;

// New computes the package documentation for the given package AST.
// New takes ownership of the AST pkg and may edit or overwrite it.
// To have the Examples fields populated, use NewFromFiles and include
// the package's _test.go files.
//
public static ptr<Package> New(ptr<ast.Package> _addr_pkg, @string importPath, Mode mode) {
    ref ast.Package pkg = ref _addr_pkg.val;

    reader r = default;
    r.readPackage(pkg, mode);
    r.computeMethodSets();
    r.cleanupTypes();
    return addr(new Package(Doc:r.doc,Name:pkg.Name,ImportPath:importPath,Imports:sortedKeys(r.imports),Filenames:r.filenames,Notes:r.notes,Bugs:noteBodies(r.notes["BUG"]),Consts:sortedValues(r.values,token.CONST),Types:sortedTypes(r.types,mode&AllMethods!=0),Vars:sortedValues(r.values,token.VAR),Funcs:sortedFuncs(r.funcs,true),));
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
// Example.Suffix field. Examples with malformed names are skipped.
//
// Optionally, a single extra argument of type Mode can be provided to
// control low-level aspects of the documentation extraction behavior.
//
// NewFromFiles takes ownership of the AST files and may edit them,
// unless the PreserveAST Mode bit is on.
//
public static (ptr<Package>, error) NewFromFiles(ptr<token.FileSet> _addr_fset, slice<ptr<ast.File>> files, @string importPath, params object[] opts) => func((_, panic, _) => {
    ptr<Package> _p0 = default!;
    error _p0 = default!;
    opts = opts.Clone();
    ref token.FileSet fset = ref _addr_fset.val;
 
    // Check for invalid API usage.
    if (fset == null) {
        panic(fmt.Errorf("doc.NewFromFiles: no token.FileSet provided (fset == nil)"));
    }
    Mode mode = default;
    switch (len(opts)) { // There can only be 0 or 1 options, so a simple switch works for now.
        case 0: 

            break;
        case 1: 
            Mode (m, ok) = opts[0]._<Mode>();
            if (!ok) {
                panic(fmt.Errorf("doc.NewFromFiles: option argument type must be doc.Mode"));
            }
            mode = m;
            break;
        default: 
            panic(fmt.Errorf("doc.NewFromFiles: there must not be more than 1 option argument"));
            break;
    } 

    // Collect .go and _test.go files.
    var goFiles = make_map<@string, ptr<ast.File>>();    slice<ptr<ast.File>> testGoFiles = default;
    foreach (var (i) in files) {
        var f = fset.File(files[i].Pos());
        if (f == null) {
            return (_addr_null!, error.As(fmt.Errorf("file files[%d] is not found in the provided file set", i))!);
        }
        {
            var name = f.Name();


            if (strings.HasSuffix(name, ".go") && !strings.HasSuffix(name, "_test.go")) 
                goFiles[name] = files[i];
            else if (strings.HasSuffix(name, "_test.go")) 
                testGoFiles = append(testGoFiles, files[i]);
            else 
                return (_addr_null!, error.As(fmt.Errorf("file files[%d] filename %q does not have a .go extension", i, name))!);

        }
    }    var (pkg, _) = ast.NewPackage(fset, goFiles, simpleImporter, null); // Ignore errors that can happen due to unresolved identifiers.
    var p = New(_addr_pkg, importPath, mode);
    classifyExamples(p, Examples(testGoFiles));
    return (_addr_p!, error.As(null!)!);
});

// simpleImporter returns a (dummy) package object named by the last path
// component of the provided package path (as is the convention for packages).
// This is sufficient to resolve package identifiers without doing an actual
// import. It never returns an error.
private static (ptr<ast.Object>, error) simpleImporter(map<@string, ptr<ast.Object>> imports, @string path) {
    ptr<ast.Object> _p0 = default!;
    error _p0 = default!;

    var pkg = imports[path];
    if (pkg == null) { 
        // note that strings.LastIndex returns -1 if there is no "/"
        pkg = ast.NewObj(ast.Pkg, path[(int)strings.LastIndex(path, "/") + 1..]);
        pkg.Data = ast.NewScope(null); // required by ast.NewPackage for dot-import
        imports[path] = pkg;
    }
    return (_addr_pkg!, error.As(null!)!);
}

} // end doc_package
