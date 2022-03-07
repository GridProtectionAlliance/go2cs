// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the exported entry points for invoking the parser.

// package parser -- go2cs converted at 2022 March 06 22:41:17 UTC
// import "go/parser" ==> using parser = go.go.parser_package
// Original source: C:\Program Files\Go\src\go\parser\interface.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using System;


namespace go.go;

public static partial class parser_package {

    // If src != nil, readSource converts src to a []byte if possible;
    // otherwise it returns an error. If src == nil, readSource returns
    // the result of reading the file specified by filename.
    //
private static (slice<byte>, error) readSource(@string filename, object src) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (src != null) {
        switch (src.type()) {
            case @string s:
                return ((slice<byte>)s, error.As(null!)!);
                break;
            case slice<byte> s:
                return (s, error.As(null!)!);
                break;
            case ptr<bytes.Buffer> s:
                if (s != null) {
                    return (s.Bytes(), error.As(null!)!);
                }
                break;
            case io.Reader s:
                return io.ReadAll(s);
                break;
        }
        return (null, error.As(errors.New("invalid source"))!);

    }
    return os.ReadFile(filename);

}

// A Mode value is a set of flags (or 0).
// They control the amount of source code parsed and other optional
// parser functionality.
//
public partial struct Mode { // : nuint
}

public static readonly Mode PackageClauseOnly = 1 << (int)(iota); // stop parsing after package clause
public static readonly var ImportsOnly = 0; // stop parsing after import declarations
public static readonly var ParseComments = 1; // parse comments and add them to AST
public static readonly var Trace = 2; // print a trace of parsed productions
public static readonly var DeclarationErrors = 3; // report declaration errors
public static readonly var SpuriousErrors = 4; // same as AllErrors, for backward-compatibility
public static readonly AllErrors SkipObjectResolution = SpuriousErrors; // report all errors (not just the first 10 on different lines)

// ParseFile parses the source code of a single Go source file and returns
// the corresponding ast.File node. The source code may be provided via
// the filename of the source file, or via the src parameter.
//
// If src != nil, ParseFile parses the source from src and the filename is
// only used when recording position information. The type of the argument
// for the src parameter must be string, []byte, or io.Reader.
// If src == nil, ParseFile parses the file specified by filename.
//
// The mode parameter controls the amount of source text parsed and other
// optional parser functionality. If the SkipObjectResolution mode bit is set,
// the object resolution phase of parsing will be skipped, causing File.Scope,
// File.Unresolved, and all Ident.Obj fields to be nil.
//
// Position information is recorded in the file set fset, which must not be
// nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with ast.Bad* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
//
public static (ptr<ast.File>, error) ParseFile(ptr<token.FileSet> _addr_fset, @string filename, object src, Mode mode) => func((defer, panic, _) => {
    ptr<ast.File> f = default!;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;

    if (fset == null) {
        panic("parser.ParseFile: no token.FileSet provided (fset == nil)");
    }
    var (text, err) = readSource(filename, src);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    parser p = default;
    defer(() => {
        {
            var e = recover();

            if (e != null) { 
                // resume same panic if it's not a bailout
                {
                    bailout (_, ok) = e._<bailout>();

                    if (!ok) {
                        panic(e);
                    }

                }

            } 

            // set result values

        } 

        // set result values
        if (f == null) { 
            // source is not a valid Go source file - satisfy
            // ParseFile API and return a valid (but) empty
            // *ast.File
            f = addr(new ast.File(Name:new(ast.Ident),Scope:ast.NewScope(nil),));

        }
        p.errors.Sort();
        err = p.errors.Err();

    }()); 

    // parse source
    p.init(fset, filename, text, mode);
    f = p.parseFile();

    return ;

});

// ParseDir calls ParseFile for all files with names ending in ".go" in the
// directory specified by path and returns a map of package name -> package
// AST with all the packages found.
//
// If filter != nil, only the files with fs.FileInfo entries passing through
// the filter (and ending in ".go") are considered. The mode bits are passed
// to ParseFile unchanged. Position information is recorded in fset, which
// must not be nil.
//
// If the directory couldn't be read, a nil map and the respective error are
// returned. If a parse error occurred, a non-nil but incomplete map and the
// first error encountered are returned.
//
public static (map<@string, ptr<ast.Package>>, error) ParseDir(ptr<token.FileSet> _addr_fset, @string path, Func<fs.FileInfo, bool> filter, Mode mode) {
    map<@string, ptr<ast.Package>> pkgs = default;
    error first = default!;
    ref token.FileSet fset = ref _addr_fset.val;

    var (list, err) = os.ReadDir(path);
    if (err != null) {
        return (null, error.As(err)!);
    }
    pkgs = make_map<@string, ptr<ast.Package>>();
    foreach (var (_, d) in list) {
        if (d.IsDir() || !strings.HasSuffix(d.Name(), ".go")) {
            continue;
        }
        if (filter != null) {
            var (info, err) = d.Info();
            if (err != null) {
                return (null, error.As(err)!);
            }
            if (!filter(info)) {
                continue;
            }
        }
        var filename = filepath.Join(path, d.Name());
        {
            var (src, err) = ParseFile(_addr_fset, filename, null, mode);

            if (err == null) {
                var name = src.Name.Name;
                var (pkg, found) = pkgs[name];
                if (!found) {
                    pkg = addr(new ast.Package(Name:name,Files:make(map[string]*ast.File),));
                    pkgs[name] = pkg;
                }
                pkg.Files[filename] = src;
            }
            else if (first == null) {
                first = err;
            }


        }

    }    return ;

}

// ParseExprFrom is a convenience function for parsing an expression.
// The arguments have the same meaning as for ParseFile, but the source must
// be a valid Go (type or value) expression. Specifically, fset must not
// be nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with ast.Bad* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
//
public static (ast.Expr, error) ParseExprFrom(ptr<token.FileSet> _addr_fset, @string filename, object src, Mode mode) => func((defer, panic, _) => {
    ast.Expr expr = default;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;

    if (fset == null) {
        panic("parser.ParseExprFrom: no token.FileSet provided (fset == nil)");
    }
    var (text, err) = readSource(filename, src);
    if (err != null) {
        return (null, error.As(err)!);
    }
    parser p = default;
    defer(() => {
        {
            var e = recover();

            if (e != null) { 
                // resume same panic if it's not a bailout
                {
                    bailout (_, ok) = e._<bailout>();

                    if (!ok) {
                        panic(e);
                    }

                }

            }

        }

        p.errors.Sort();
        err = p.errors.Err();

    }()); 

    // parse expr
    p.init(fset, filename, text, mode);
    expr = p.parseRhsOrType(); 

    // If a semicolon was inserted, consume it;
    // report an error if there's more tokens.
    if (p.tok == token.SEMICOLON && p.lit == "\n") {
        p.next();
    }
    p.expect(token.EOF);

    return ;

});

// ParseExpr is a convenience function for obtaining the AST of an expression x.
// The position information recorded in the AST is undefined. The filename used
// in error messages is the empty string.
//
// If syntax errors were found, the result is a partial AST (with ast.Bad* nodes
// representing the fragments of erroneous source code). Multiple errors are
// returned via a scanner.ErrorList which is sorted by source position.
//
public static (ast.Expr, error) ParseExpr(@string x) {
    ast.Expr _p0 = default;
    error _p0 = default!;

    return ParseExprFrom(_addr_token.NewFileSet(), "", (slice<byte>)x, 0);
}

} // end parser_package
