// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the exported entry points for invoking the parser.

// package parser -- go2cs converted at 2020 August 29 08:46:51 UTC
// import "go/parser" ==> using parser = go.go.parser_package
// Original source: C:\Go\src\go\parser\interface.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class parser_package
    {
        // If src != nil, readSource converts src to a []byte if possible;
        // otherwise it returns an error. If src == nil, readSource returns
        // the result of reading the file specified by filename.
        //
        private static (slice<byte>, error) readSource(@string filename, object src)
        {
            if (src != null)
            {
                switch (src.type())
                {
                    case @string s:
                        return ((slice<byte>)s, null);
                        break;
                    case slice<byte> s:
                        return (s, null);
                        break;
                    case ref bytes.Buffer s:
                        if (s != null)
                        {
                            return (s.Bytes(), null);
                        }
                        break;
                    case io.Reader s:
                        bytes.Buffer buf = default;
                        {
                            var (_, err) = io.Copy(ref buf, s);

                            if (err != null)
                            {
                                return (null, err);
                            }
                        }
                        return (buf.Bytes(), null);
                        break;
                }
                return (null, errors.New("invalid source"));
            }
            return ioutil.ReadFile(filename);
        }

        // A Mode value is a set of flags (or 0).
        // They control the amount of source code parsed and other optional
        // parser functionality.
        //
        public partial struct Mode // : ulong
        {
        }

        public static readonly Mode PackageClauseOnly = 1L << (int)(iota); // stop parsing after package clause
        public static readonly var ImportsOnly = 0; // stop parsing after import declarations
        public static readonly var ParseComments = 1; // parse comments and add them to AST
        public static readonly var Trace = 2; // print a trace of parsed productions
        public static readonly var DeclarationErrors = 3; // report declaration errors
        public static readonly AllErrors SpuriousErrors = SpuriousErrors; // report all errors (not just the first 10 on different lines)

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
        // optional parser functionality. Position information is recorded in the
        // file set fset, which must not be nil.
        //
        // If the source couldn't be read, the returned AST is nil and the error
        // indicates the specific failure. If the source was read but syntax
        // errors were found, the result is a partial AST (with ast.Bad* nodes
        // representing the fragments of erroneous source code). Multiple errors
        // are returned via a scanner.ErrorList which is sorted by file position.
        //
        public static (ref ast.File, error) ParseFile(ref token.FileSet _fset, @string filename, object src, Mode mode) => func(_fset, (ref token.FileSet fset, Defer defer, Panic panic, Recover _) =>
        {
            if (fset == null)
            {
                panic("parser.ParseFile: no token.FileSet provided (fset == nil)");
            } 

            // get source
            var (text, err) = readSource(filename, src);
            if (err != null)
            {
                return (null, err);
            }
            parser p = default;
            defer(() =>
            {
                {
                    var e = recover();

                    if (e != null)
                    { 
                        // resume same panic if it's not a bailout
                        {
                            bailout (_, ok) = e._<bailout>();

                            if (!ok)
                            {
                                panic(e);
                            }

                        }
                    } 

                    // set result values

                } 

                // set result values
                if (f == null)
                { 
                    // source is not a valid Go source file - satisfy
                    // ParseFile API and return a valid (but) empty
                    // *ast.File
                    f = ref new ast.File(Name:new(ast.Ident),Scope:ast.NewScope(nil),);
                }
                p.errors.Sort();
                err = p.errors.Err();
            }()); 

            // parse source
            p.init(fset, filename, text, mode);
            f = p.parseFile();

            return;
        });

        // ParseDir calls ParseFile for all files with names ending in ".go" in the
        // directory specified by path and returns a map of package name -> package
        // AST with all the packages found.
        //
        // If filter != nil, only the files with os.FileInfo entries passing through
        // the filter (and ending in ".go") are considered. The mode bits are passed
        // to ParseFile unchanged. Position information is recorded in fset, which
        // must not be nil.
        //
        // If the directory couldn't be read, a nil map and the respective error are
        // returned. If a parse error occurred, a non-nil but incomplete map and the
        // first error encountered are returned.
        //
        public static (map<@string, ref ast.Package>, error) ParseDir(ref token.FileSet _fset, @string path, Func<os.FileInfo, bool> filter, Mode mode) => func(_fset, (ref token.FileSet fset, Defer defer, Panic _, Recover __) =>
        {
            var (fd, err) = os.Open(path);
            if (err != null)
            {
                return (null, err);
            }
            defer(fd.Close());

            var (list, err) = fd.Readdir(-1L);
            if (err != null)
            {
                return (null, err);
            }
            pkgs = make_map<@string, ref ast.Package>();
            foreach (var (_, d) in list)
            {
                if (strings.HasSuffix(d.Name(), ".go") && (filter == null || filter(d)))
                {
                    var filename = filepath.Join(path, d.Name());
                    {
                        var (src, err) = ParseFile(fset, filename, null, mode);

                        if (err == null)
                        {
                            var name = src.Name.Name;
                            var (pkg, found) = pkgs[name];
                            if (!found)
                            {
                                pkg = ref new ast.Package(Name:name,Files:make(map[string]*ast.File),);
                                pkgs[name] = pkg;
                            }
                            pkg.Files[filename] = src;
                        }
                        else if (first == null)
                        {
                            first = err;
                        }

                    }
                }
            }
            return;
        });

        // ParseExprFrom is a convenience function for parsing an expression.
        // The arguments have the same meaning as for ParseFile, but the source must
        // be a valid Go (type or value) expression. Specifically, fset must not
        // be nil.
        //
        public static (ast.Expr, error) ParseExprFrom(ref token.FileSet _fset, @string filename, object src, Mode mode) => func(_fset, (ref token.FileSet fset, Defer defer, Panic panic, Recover _) =>
        {
            if (fset == null)
            {
                panic("parser.ParseExprFrom: no token.FileSet provided (fset == nil)");
            } 

            // get source
            var (text, err) = readSource(filename, src);
            if (err != null)
            {
                return (null, err);
            }
            parser p = default;
            defer(() =>
            {
                {
                    var e__prev1 = e;

                    var e = recover();

                    if (e != null)
                    { 
                        // resume same panic if it's not a bailout
                        {
                            bailout (_, ok) = e._<bailout>();

                            if (!ok)
                            {
                                panic(e);
                            }

                        }
                    }

                    e = e__prev1;

                }
                p.errors.Sort();
                err = p.errors.Err();
            }()); 

            // parse expr
            p.init(fset, filename, text, mode); 
            // Set up pkg-level scopes to avoid nil-pointer errors.
            // This is not needed for a correct expression x as the
            // parser will be ok with a nil topScope, but be cautious
            // in case of an erroneous x.
            p.openScope();
            p.pkgScope = p.topScope;
            e = p.parseRhsOrType();
            p.closeScope();
            assert(p.topScope == null, "unbalanced scopes"); 

            // If a semicolon was inserted, consume it;
            // report an error if there's more tokens.
            if (p.tok == token.SEMICOLON && p.lit == "\n")
            {
                p.next();
            }
            p.expect(token.EOF);

            if (p.errors.Len() > 0L)
            {
                p.errors.Sort();
                return (null, p.errors.Err());
            }
            return (e, null);
        });

        // ParseExpr is a convenience function for obtaining the AST of an expression x.
        // The position information recorded in the AST is undefined. The filename used
        // in error messages is the empty string.
        //
        public static (ast.Expr, error) ParseExpr(@string x)
        {
            return ParseExprFrom(token.NewFileSet(), "", (slice<byte>)x, 0L);
        }
    }
}}
