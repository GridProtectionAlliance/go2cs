// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains the exported entry points for invoking the parser.
namespace go.go;

using bytes = bytes_package;
using errors = errors_package;
using ast = go.ast_package;
using token = go.token_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using io;
using path;

partial class parser_package {

// If src != nil, readSource converts src to a []byte if possible;
// otherwise it returns an error. If src == nil, readSource returns
// the result of reading the file specified by filename.
internal static (slice<byte>, error) readSource(@string filename, any src) {
    if (src != default!) {
        switch (src.type()) {
        case @string s: {
            return (slice<byte>(s), default!);
        }
        case slice<byte> s: {
            return (s, default!);
        }
        case ж<bytes.Buffer> s: {
            if (s != nil) {
                // is io.Reader, but src is already available in []byte form
                return (s.Bytes(), default!);
            }
            break;
        }
        case io.Reader s: {
            return io.ReadAll(s);
        }}
        return (default!, errors.New("invalid source"u8));
    }
    return os.ReadFile(filename);
}

[GoType("num:nuint")] partial struct Mode;

public static readonly Mode PackageClauseOnly = /* 1 << iota */ 1;                      // stop parsing after package clause
public static readonly Mode ImportsOnly = 2;                            // stop parsing after import declarations
public static readonly Mode ParseComments = 4;                          // parse comments and add them to AST
public static readonly Mode Trace = 8;                                  // print a trace of parsed productions
public static readonly Mode DeclarationErrors = 16;                      // report declaration errors
public static readonly Mode SpuriousErrors = 32;                         // same as AllErrors, for backward-compatibility
public static readonly Mode SkipObjectResolution = 64;                   // skip deprecated identifier resolution; see ParseFile
public static readonly Mode AllErrors = /* SpuriousErrors */ 32;                  // report all errors (not just the first 10 on different lines)

// ParseFile parses the source code of a single Go source file and returns
// the corresponding [ast.File] node. The source code may be provided via
// the filename of the source file, or via the src parameter.
//
// If src != nil, ParseFile parses the source from src and the filename is
// only used when recording position information. The type of the argument
// for the src parameter must be string, []byte, or [io.Reader].
// If src == nil, ParseFile parses the file specified by filename.
//
// The mode parameter controls the amount of source text parsed and
// other optional parser functionality. If the [SkipObjectResolution]
// mode bit is set (recommended), the object resolution phase of
// parsing will be skipped, causing File.Scope, File.Unresolved, and
// all Ident.Obj fields to be nil. Those fields are deprecated; see
// [ast.Object] for details.
//
// Position information is recorded in the file set fset, which must not be
// nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
public static (ж<ast.File> f, error err) ParseFile(ж<token.FileSet> Ꮡfset, @string filename, any src, Mode mode) => func((defer, recover) => {
    ж<ast.File> f = default!;
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    if (fset == nil) {
        throw panic("parser.ParseFile: no token.FileSet provided (fset == nil)");
    }
    // get source
    (text, err) = readSource(filename, src);
    if (err != default!) {
        return (default!, err);
    }
    ref var p = ref heap(new parser(), out var Ꮡp);
    var errʗ1 = err;
    var pʗ1 = p;
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                // resume same panic if it's not a bailout
                var (bail, ok) = e._<bailout>(ᐧ);
                if (!ok){
                    throw panic(e);
                } else 
                if (bail.msg != ""u8) {
                    pʗ1.errors.Add(pʗ1.file.Position(bail.pos), bail.msg);
                }
            }
        }
        // set result values
        if (f == nil) {
            // source is not a valid Go source file - satisfy
            // ParseFile API and return a valid (but) empty
            // *ast.File
            f = Ꮡ(new ast.File(
                Name: @new<ast.Ident>(),
                Scope: ast.NewScope(nil)
            ));
        }
        pʗ1.errors.Sort();
        errʗ1 = pʗ1.errors.Err();
    });
    // parse source
    p.init(Ꮡfset, filename, text, mode);
    f = p.parseFile();
    return (f, err);
});

// ParseDir calls [ParseFile] for all files with names ending in ".go" in the
// directory specified by path and returns a map of package name -> package
// AST with all the packages found.
//
// If filter != nil, only the files with [fs.FileInfo] entries passing through
// the filter (and ending in ".go") are considered. The mode bits are passed
// to [ParseFile] unchanged. Position information is recorded in fset, which
// must not be nil.
//
// If the directory couldn't be read, a nil map and the respective error are
// returned. If a parse error occurred, a non-nil but incomplete map and the
// first error encountered are returned.
public static (ast.Package pkgs, error first) ParseDir(ж<token.FileSet> Ꮡfset, @string path, fs.FileInfo) bool filter, Mode mode) {
    ast.Package pkgs = default!;
    error first = default!;

    ref var fset = ref Ꮡfset.val;
    (list, err) = os.ReadDir(path);
    if (err != default!) {
        return (default!, err);
    }
    pkgs = new ast.Package();
    foreach (var (_, d) in list) {
        if (d.IsDir() || !strings.HasSuffix(d.Name(), ".go"u8)) {
            continue;
        }
        if (filter != default!) {
            (info, errΔ1) = d.Info();
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            if (!filter(info)) {
                continue;
            }
        }
        @string filename = filepath.Join(path, d.Name());
        {
            (src, errΔ2) = ParseFile(Ꮡfset, filename, default!, mode); if (errΔ2 == default!){
                @string name = (~src).Name.val.Name;
                var pkg = pkgs[name];
                var found = pkgs[name];
                if (!found) {
                    pkg = Ꮡ(new ast.Package(
                        Name: name,
                        Files: new ast.File()
                    ));
                    pkgs[name] = pkg;
                }
                (~pkg).Files[filename] = src;
            } else 
            if (first == default!) {
                first = errΔ2;
            }
        }
    }
    return (pkgs, first);
}

// ParseExprFrom is a convenience function for parsing an expression.
// The arguments have the same meaning as for [ParseFile], but the source must
// be a valid Go (type or value) expression. Specifically, fset must not
// be nil.
//
// If the source couldn't be read, the returned AST is nil and the error
// indicates the specific failure. If the source was read but syntax
// errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors
// are returned via a scanner.ErrorList which is sorted by source position.
public static (ast.Expr expr, error err) ParseExprFrom(ж<token.FileSet> Ꮡfset, @string filename, any src, Mode mode) => func((defer, recover) => {
    ast.Expr expr = default!;
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    if (fset == nil) {
        throw panic("parser.ParseExprFrom: no token.FileSet provided (fset == nil)");
    }
    // get source
    (text, err) = readSource(filename, src);
    if (err != default!) {
        return (default!, err);
    }
    ref var p = ref heap(new parser(), out var Ꮡp);
    var errʗ1 = err;
    var pʗ1 = p;
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                // resume same panic if it's not a bailout
                var (bail, ok) = e._<bailout>(ᐧ);
                if (!ok){
                    throw panic(e);
                } else 
                if (bail.msg != ""u8) {
                    pʗ1.errors.Add(pʗ1.file.Position(bail.pos), bail.msg);
                }
            }
        }
        pʗ1.errors.Sort();
        errʗ1 = pʗ1.errors.Err();
    });
    // parse expr
    p.init(Ꮡfset, filename, text, mode);
    expr = p.parseRhs();
    // If a semicolon was inserted, consume it;
    // report an error if there's more tokens.
    if (p.tok == token.SEMICOLON && p.lit == "\n"u8) {
        p.next();
    }
    p.expect(token.EOF);
    return (expr, err);
});

// ParseExpr is a convenience function for obtaining the AST of an expression x.
// The position information recorded in the AST is undefined. The filename used
// in error messages is the empty string.
//
// If syntax errors were found, the result is a partial AST (with [ast.Bad]* nodes
// representing the fragments of erroneous source code). Multiple errors are
// returned via a scanner.ErrorList which is sorted by source position.
public static (ast.Expr, error) ParseExpr(@string x) {
    return ParseExprFrom(token.NewFileSet(), ""u8, slice<byte>(x), 0);
}

} // end parser_package
