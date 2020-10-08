// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Extract example functions from file ASTs.

// package doc -- go2cs converted at 2020 October 08 04:02:44 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\example.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using lazyregexp = go.@internal.lazyregexp_package;
using path = go.path_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        // An Example represents an example function found in a test source file.
        public partial struct Example
        {
            public @string Name; // name of the item being exemplified (including optional suffix)
            public @string Suffix; // example suffix, without leading '_' (only populated by NewFromFiles)
            public @string Doc; // example function doc string
            public ast.Node Code;
            public ptr<ast.File> Play; // a whole program version of the example
            public slice<ptr<ast.CommentGroup>> Comments;
            public @string Output; // expected output
            public bool Unordered;
            public bool EmptyOutput; // expect empty output
            public long Order; // original source code order
        }

        // Examples returns the examples found in testFiles, sorted by Name field.
        // The Order fields record the order in which the examples were encountered.
        // The Suffix field is not populated when Examples is called directly, it is
        // only populated by NewFromFiles for examples it finds in _test.go files.
        //
        // Playable Examples must be in a package whose name ends in "_test".
        // An Example is "playable" (the Play field is non-nil) in either of these
        // circumstances:
        //   - The example function is self-contained: the function references only
        //     identifiers from other packages (or predeclared identifiers, such as
        //     "int") and the test file does not include a dot import.
        //   - The entire test file is the example: the file contains exactly one
        //     example function, zero test or benchmark functions, and at least one
        //     top-level function, type, variable, or constant declaration other
        //     than the example function.
        public static slice<ptr<Example>> Examples(params ptr<ptr<ast.File>>[] _addr_testFiles)
        {
            testFiles = testFiles.Clone();
            ref ast.File testFiles = ref _addr_testFiles.val;

            slice<ptr<Example>> list = default;
            foreach (var (_, file) in testFiles)
            {
                var hasTests = false; // file contains tests or benchmarks
                long numDecl = 0L; // number of non-import declarations in the file
                slice<ptr<Example>> flist = default;
                foreach (var (_, decl) in file.Decls)
                {
                    {
                        ptr<ast.GenDecl> (g, ok) = decl._<ptr<ast.GenDecl>>();

                        if (ok && g.Tok != token.IMPORT)
                        {
                            numDecl++;
                            continue;
                        }

                    }

                    ptr<ast.FuncDecl> (f, ok) = decl._<ptr<ast.FuncDecl>>();
                    if (!ok || f.Recv != null)
                    {
                        continue;
                    }

                    numDecl++;
                    var name = f.Name.Name;
                    if (isTest(name, "Test") || isTest(name, "Benchmark"))
                    {
                        hasTests = true;
                        continue;
                    }

                    if (!isTest(name, "Example"))
                    {
                        continue;
                    }

                    {
                        var @params = f.Type.Params;

                        if (len(@params.List) != 0L)
                        {
                            continue; // function has params; not a valid example
                        }

                    }

                    if (f.Body == null)
                    { // ast.File.Body nil dereference (see issue 28044)
                        continue;

                    }

                    @string doc = default;
                    if (f.Doc != null)
                    {
                        doc = f.Doc.Text();
                    }

                    var (output, unordered, hasOutput) = exampleOutput(_addr_f.Body, file.Comments);
                    flist = append(flist, addr(new Example(Name:name[len("Example"):],Doc:doc,Code:f.Body,Play:playExample(file,f),Comments:file.Comments,Output:output,Unordered:unordered,EmptyOutput:output==""&&hasOutput,Order:len(flist),)));

                }
                if (!hasTests && numDecl > 1L && len(flist) == 1L)
                { 
                    // If this file only has one example function, some
                    // other top-level declarations, and no tests or
                    // benchmarks, use the whole file as the example.
                    flist[0L].Code = file;
                    flist[0L].Play = playExampleFile(_addr_file);

                }

                list = append(list, flist);

            } 
            // sort by name
            sort.Slice(list, (i, j) =>
            {
                return list[i].Name < list[j].Name;
            });
            return list;

        }

        private static var outputPrefix = lazyregexp.New("(?i)^[[:space:]]*(unordered )?output:");

        // Extracts the expected output and whether there was a valid output comment
        private static (@string, bool, bool) exampleOutput(ptr<ast.BlockStmt> _addr_b, slice<ptr<ast.CommentGroup>> comments)
        {
            @string output = default;
            bool unordered = default;
            bool ok = default;
            ref ast.BlockStmt b = ref _addr_b.val;

            {
                var (_, last) = lastComment(_addr_b, comments);

                if (last != null)
                { 
                    // test that it begins with the correct prefix
                    var text = last.Text();
                    {
                        var loc = outputPrefix.FindStringSubmatchIndex(text);

                        if (loc != null)
                        {
                            if (loc[2L] != -1L)
                            {
                                unordered = true;
                            }

                            text = text[loc[1L]..]; 
                            // Strip zero or more spaces followed by \n or a single space.
                            text = strings.TrimLeft(text, " ");
                            if (len(text) > 0L && text[0L] == '\n')
                            {
                                text = text[1L..];
                            }

                            return (text, unordered, true);

                        }

                    }

                }

            }

            return ("", false, false); // no suitable comment found
        }

        // isTest tells whether name looks like a test, example, or benchmark.
        // It is a Test (say) if there is a character after Test that is not a
        // lower-case letter. (We don't want Testiness.)
        private static bool isTest(@string name, @string prefix)
        {
            if (!strings.HasPrefix(name, prefix))
            {
                return false;
            }

            if (len(name) == len(prefix))
            { // "Test" is ok
                return true;

            }

            var (rune, _) = utf8.DecodeRuneInString(name[len(prefix)..]);
            return !unicode.IsLower(rune);

        }

        // playExample synthesizes a new *ast.File based on the provided
        // file with the provided function body as the body of main.
        private static ptr<ast.File> playExample(ptr<ast.File> _addr_file, ptr<ast.FuncDecl> _addr_f)
        {
            ref ast.File file = ref _addr_file.val;
            ref ast.FuncDecl f = ref _addr_f.val;

            var body = f.Body;

            if (!strings.HasSuffix(file.Name.Name, "_test"))
            { 
                // We don't support examples that are part of the
                // greater package (yet).
                return _addr_null!;

            } 

            // Collect top-level declarations in the file.
            var topDecls = make_map<ptr<ast.Object>, ast.Decl>();
            var typMethods = make_map<@string, slice<ast.Decl>>();

            foreach (var (_, decl) in file.Decls)
            {
                switch (decl.type())
                {
                    case ptr<ast.FuncDecl> d:
                        if (d.Recv == null)
                        {
                            topDecls[d.Name.Obj] = d;
                        }
                        else
                        {
                            if (len(d.Recv.List) == 1L)
                            {
                                var t = d.Recv.List[0L].Type;
                                var (tname, _) = baseTypeName(t);
                                typMethods[tname] = append(typMethods[tname], d);
                            }

                        }

                        break;
                    case ptr<ast.GenDecl> d:
                        {
                            var spec__prev2 = spec;

                            foreach (var (_, __spec) in d.Specs)
                            {
                                spec = __spec;
                                switch (spec.type())
                                {
                                    case ptr<ast.TypeSpec> s:
                                        topDecls[s.Name.Obj] = d;
                                        break;
                                    case ptr<ast.ValueSpec> s:
                                        foreach (var (_, name) in s.Names)
                                        {
                                            topDecls[name.Obj] = d;
                                        }
                                        break;
                                }

                            }

                            spec = spec__prev2;
                        }
                        break;
                }

            } 

            // Find unresolved identifiers and uses of top-level declarations.
            var unresolved = make_map<@string, bool>();
            slice<ast.Decl> depDecls = default;
            var hasDepDecls = make_map<ast.Decl, bool>();

            Func<ast.Node, bool> inspectFunc = default;
            inspectFunc = n =>
            {
                switch (n.type())
                {
                    case ptr<ast.Ident> e:
                        if (e.Obj == null && e.Name != "_")
                        {
                            unresolved[e.Name] = true;
                        }                        {
                            var d__prev2 = d;

                            var d = topDecls[e.Obj];


                            else if (d != null)
                            {
                                if (!hasDepDecls[d])
                                {
                                    hasDepDecls[d] = true;
                                    depDecls = append(depDecls, d);
                                }

                            }

                            d = d__prev2;

                        }

                        return _addr_true!;
                        break;
                    case ptr<ast.SelectorExpr> e:
                        ast.Inspect(e.X, inspectFunc);
                        return _addr_false!;
                        break;
                    case ptr<ast.KeyValueExpr> e:
                        ast.Inspect(e.Value, inspectFunc);
                        return _addr_false!;
                        break;
                }
                return _addr_true!;

            }
;
            ast.Inspect(body, inspectFunc);
            for (long i = 0L; i < len(depDecls); i++)
            {
                switch (depDecls[i].type())
                {
                    case ptr<ast.FuncDecl> d:
                        if (d.Type.Params != null)
                        {
                            {
                                var p__prev2 = p;

                                foreach (var (_, __p) in d.Type.Params.List)
                                {
                                    p = __p;
                                    ast.Inspect(p.Type, inspectFunc);
                                }

                                p = p__prev2;
                            }
                        }

                        if (d.Type.Results != null)
                        {
                            foreach (var (_, r) in d.Type.Results.List)
                            {
                                ast.Inspect(r.Type, inspectFunc);
                            }

                        }

                        ast.Inspect(d.Body, inspectFunc);
                        break;
                    case ptr<ast.GenDecl> d:
                        {
                            var spec__prev2 = spec;

                            foreach (var (_, __spec) in d.Specs)
                            {
                                spec = __spec;
                                switch (spec.type())
                                {
                                    case ptr<ast.TypeSpec> s:
                                        ast.Inspect(s.Type, inspectFunc);

                                        depDecls = append(depDecls, typMethods[s.Name.Name]);
                                        break;
                                    case ptr<ast.ValueSpec> s:
                                        if (s.Type != null)
                                        {
                                            ast.Inspect(s.Type, inspectFunc);
                                        }

                                        foreach (var (_, val) in s.Values)
                                        {
                                            ast.Inspect(val, inspectFunc);
                                        }
                                        break;
                                }

                            }

                            spec = spec__prev2;
                        }
                        break;
                }

            } 

            // Remove predeclared identifiers from unresolved list.
 

            // Remove predeclared identifiers from unresolved list.
            {
                var n__prev1 = n;

                foreach (var (__n) in unresolved)
                {
                    n = __n;
                    if (predeclaredTypes[n] || predeclaredConstants[n] || predeclaredFuncs[n])
                    {
                        delete(unresolved, n);
                    }

                } 

                // Use unresolved identifiers to determine the imports used by this
                // example. The heuristic assumes package names match base import
                // paths for imports w/o renames (should be good enough most of the time).

                n = n__prev1;
            }

            var namedImports = make_map<@string, @string>(); // [name]path
            slice<ast.Spec> blankImports = default; // _ imports
            {
                var s__prev1 = s;

                foreach (var (_, __s) in file.Imports)
                {
                    s = __s;
                    var (p, err) = strconv.Unquote(s.Path.Value);
                    if (err != null)
                    {
                        continue;
                    }

                    if (p == "syscall/js")
                    { 
                        // We don't support examples that import syscall/js,
                        // because the package syscall/js is not available in the playground.
                        return _addr_null!;

                    }

                    var n = path.Base(p);
                    if (s.Name != null)
                    {
                        n = s.Name.Name;
                        switch (n)
                        {
                            case "_": 
                                blankImports = append(blankImports, s);
                                continue;
                                break;
                            case ".": 
                                // We can't resolve dot imports (yet).
                                return _addr_null!;
                                break;
                        }

                    }

                    if (unresolved[n])
                    {
                        namedImports[n] = p;
                        delete(unresolved, n);
                    }

                } 

                // If there are other unresolved identifiers, give up because this
                // synthesized file is not going to build.

                s = s__prev1;
            }

            if (len(unresolved) > 0L)
            {
                return _addr_null!;
            } 

            // Include documentation belonging to blank imports.
            slice<ptr<ast.CommentGroup>> comments = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in blankImports)
                {
                    s = __s;
                    {
                        ptr<ast.ImportSpec> c__prev1 = c;

                        ptr<ast.ImportSpec> c = s._<ptr<ast.ImportSpec>>().Doc;

                        if (c != null)
                        {
                            comments = append(comments, c);
                        }

                        c = c__prev1;

                    }

                } 

                // Include comments that are inside the function body.

                s = s__prev1;
            }

            {
                ptr<ast.ImportSpec> c__prev1 = c;

                foreach (var (_, __c) in file.Comments)
                {
                    c = __c;
                    if (body.Pos() <= c.Pos() && c.End() <= body.End())
                    {
                        comments = append(comments, c);
                    }

                } 

                // Strip the "Output:" or "Unordered output:" comment and adjust body
                // end position.

                c = c__prev1;
            }

            body, comments = stripOutputComment(_addr_body, comments); 

            // Include documentation belonging to dependent declarations.
            {
                var d__prev1 = d;

                foreach (var (_, __d) in depDecls)
                {
                    d = __d;
                    switch (d.type())
                    {
                        case ptr<ast.GenDecl> d:
                            if (d.Doc != null)
                            {
                                comments = append(comments, d.Doc);
                            }

                            break;
                        case ptr<ast.FuncDecl> d:
                            if (d.Doc != null)
                            {
                                comments = append(comments, d.Doc);
                            }

                            break;
                    }

                } 

                // Synthesize import declaration.

                d = d__prev1;
            }

            ptr<ast.GenDecl> importDecl = addr(new ast.GenDecl(Tok:token.IMPORT,Lparen:1,Rparen:1,));
            {
                var n__prev1 = n;
                var p__prev1 = p;

                foreach (var (__n, __p) in namedImports)
                {
                    n = __n;
                    p = __p;
                    ptr<ast.ImportSpec> s = addr(new ast.ImportSpec(Path:&ast.BasicLit{Value:strconv.Quote(p)}));
                    if (path.Base(p) != n)
                    {
                        s.Name = ast.NewIdent(n);
                    }

                    importDecl.Specs = append(importDecl.Specs, s);

                }

                n = n__prev1;
                p = p__prev1;
            }

            importDecl.Specs = append(importDecl.Specs, blankImports); 

            // Synthesize main function.
            ptr<ast.FuncDecl> funcDecl = addr(new ast.FuncDecl(Name:ast.NewIdent("main"),Type:f.Type,Body:body,));

            var decls = make_slice<ast.Decl>(0L, 2L + len(depDecls));
            decls = append(decls, importDecl);
            decls = append(decls, depDecls);
            decls = append(decls, funcDecl);

            sort.Slice(decls, (i, j) =>
            {
                return _addr_decls[i].Pos() < decls[j].Pos()!;
            });

            sort.Slice(comments, (i, j) =>
            {
                return _addr_comments[i].Pos() < comments[j].Pos()!;
            }); 

            // Synthesize file.
            return addr(new ast.File(Name:ast.NewIdent("main"),Decls:decls,Comments:comments,));

        }

        // playExampleFile takes a whole file example and synthesizes a new *ast.File
        // such that the example is function main in package main.
        private static ptr<ast.File> playExampleFile(ptr<ast.File> _addr_file)
        {
            ref ast.File file = ref _addr_file.val;
 
            // Strip copyright comment if present.
            var comments = file.Comments;
            if (len(comments) > 0L && strings.HasPrefix(comments[0L].Text(), "Copyright"))
            {
                comments = comments[1L..];
            } 

            // Copy declaration slice, rewriting the ExampleX function to main.
            slice<ast.Decl> decls = default;
            foreach (var (_, d) in file.Decls)
            {
                {
                    ptr<ast.FuncDecl> f__prev1 = f;

                    ptr<ast.FuncDecl> (f, ok) = d._<ptr<ast.FuncDecl>>();

                    if (ok && isTest(f.Name.Name, "Example"))
                    { 
                        // Copy the FuncDecl, as it may be used elsewhere.
                        ref var newF = ref heap(f.val, out ptr<var> _addr_newF);
                        newF.Name = ast.NewIdent("main");
                        newF.Body, comments = stripOutputComment(_addr_f.Body, comments);
                        _addr_d = _addr_newF;
                        d = ref _addr_d.val;

                    }

                    f = f__prev1;

                }

                decls = append(decls, d);

            } 

            // Copy the File, as it may be used elsewhere.
            ref ast.File f = ref heap(file, out ptr<ast.File> _addr_f);
            f.Name = ast.NewIdent("main");
            f.Decls = decls;
            f.Comments = comments;
            return _addr__addr_f!;

        }

        // stripOutputComment finds and removes the "Output:" or "Unordered output:"
        // comment from body and comments, and adjusts the body block's end position.
        private static (ptr<ast.BlockStmt>, slice<ptr<ast.CommentGroup>>) stripOutputComment(ptr<ast.BlockStmt> _addr_body, slice<ptr<ast.CommentGroup>> comments)
        {
            ptr<ast.BlockStmt> _p0 = default!;
            slice<ptr<ast.CommentGroup>> _p0 = default;
            ref ast.BlockStmt body = ref _addr_body.val;
 
            // Do nothing if there is no "Output:" or "Unordered output:" comment.
            var (i, last) = lastComment(_addr_body, comments);
            if (last == null || !outputPrefix.MatchString(last.Text()))
            {
                return (_addr_body!, comments);
            } 

            // Copy body and comments, as the originals may be used elsewhere.
            ptr<ast.BlockStmt> newBody = addr(new ast.BlockStmt(Lbrace:body.Lbrace,List:body.List,Rbrace:last.Pos(),));
            var newComments = make_slice<ptr<ast.CommentGroup>>(len(comments) - 1L);
            copy(newComments, comments[..i]);
            copy(newComments[i..], comments[i + 1L..]);
            return (_addr_newBody!, newComments);

        }

        // lastComment returns the last comment inside the provided block.
        private static (long, ptr<ast.CommentGroup>) lastComment(ptr<ast.BlockStmt> _addr_b, slice<ptr<ast.CommentGroup>> c)
        {
            long i = default;
            ptr<ast.CommentGroup> last = default!;
            ref ast.BlockStmt b = ref _addr_b.val;

            if (b == null)
            {
                return ;
            }

            var pos = b.Pos();
            var end = b.End();
            foreach (var (j, cg) in c)
            {
                if (cg.Pos() < pos)
                {
                    continue;
                }

                if (cg.End() > end)
                {
                    break;
                }

                i = j;
                last = cg;

            }
            return ;

        }

        // classifyExamples classifies examples and assigns them to the Examples field
        // of the relevant Func, Type, or Package that the example is associated with.
        //
        // The classification process is ambiguous in some cases:
        //
        //     - ExampleFoo_Bar matches a type named Foo_Bar
        //       or a method named Foo.Bar.
        //     - ExampleFoo_bar matches a type named Foo_bar
        //       or Foo (with a "bar" suffix).
        //
        // Examples with malformed names are not associated with anything.
        //
        private static void classifyExamples(ptr<Package> _addr_p, slice<ptr<Example>> examples)
        {
            ref Package p = ref _addr_p.val;

            if (len(examples) == 0L)
            {
                return ;
            } 

            // Mapping of names for funcs, types, and methods to the example listing.
            var ids = make_map<@string, ptr<slice<ptr<Example>>>>();
            ids[""] = _addr_p.Examples; // package-level examples have an empty name
            {
                var f__prev1 = f;

                foreach (var (_, __f) in p.Funcs)
                {
                    f = __f;
                    if (!token.IsExported(f.Name))
                    {
                        continue;
                    }

                    ids[f.Name] = _addr_f.Examples;

                }

                f = f__prev1;
            }

            foreach (var (_, t) in p.Types)
            {
                if (!token.IsExported(t.Name))
                {
                    continue;
                }

                ids[t.Name] = _addr_t.Examples;
                {
                    var f__prev2 = f;

                    foreach (var (_, __f) in t.Funcs)
                    {
                        f = __f;
                        if (!token.IsExported(f.Name))
                        {
                            continue;
                        }

                        ids[f.Name] = _addr_f.Examples;

                    }

                    f = f__prev2;
                }

                foreach (var (_, m) in t.Methods)
                {
                    if (!token.IsExported(m.Name) || m.Level != 0L)
                    { // avoid forwarded methods from embedding
                        continue;

                    }

                    ids[strings.TrimPrefix(m.Recv, "*") + "_" + m.Name] = _addr_m.Examples;

                }

            } 

            // Group each example with the associated func, type, or method.
            foreach (var (_, ex) in examples)
            { 
                // Consider all possible split points for the suffix
                // by starting at the end of string (no suffix case),
                // then trying all positions that contain a '_' character.
                //
                // An association is made on the first successful match.
                // Examples with malformed names that match nothing are skipped.
                {
                    var i = len(ex.Name);

                    while (i >= 0L)
                    {
                        var (prefix, suffix, ok) = splitExampleName(ex.Name, i);
                        if (!ok)
                        {
                            continue;
                        i = strings.LastIndexByte(ex.Name[..i], '_');
                        }

                        var (exs, ok) = ids[prefix];
                        if (!ok)
                        {
                            continue;
                        }

                        ex.Suffix = suffix;
                        exs.val = append(exs.val, ex);
                        break;

                    }

                }

            } 

            // Sort list of example according to the user-specified suffix name.
            {
                var exs__prev1 = exs;

                foreach (var (_, __exs) in ids)
                {
                    exs = __exs;
                    sort.Slice((exs.val), (i, j) =>
                    {
                        return (exs.val)[i].Suffix < (exs.val)[j].Suffix;
                    });

                }

                exs = exs__prev1;
            }
        }

        // splitExampleName attempts to split example name s at index i,
        // and reports if that produces a valid split. The suffix may be
        // absent. Otherwise, it must start with a lower-case letter and
        // be preceded by '_'.
        //
        // One of i == len(s) or s[i] == '_' must be true.
        private static (@string, @string, bool) splitExampleName(@string s, long i)
        {
            @string prefix = default;
            @string suffix = default;
            bool ok = default;

            if (i == len(s))
            {
                return (s, "", true);
            }

            if (i == len(s) - 1L)
            {
                return ("", "", false);
            }

            prefix = s[..i];
            suffix = s[i + 1L..];
            return (prefix, suffix, isExampleSuffix(suffix));

        }

        private static bool isExampleSuffix(@string s)
        {
            var (r, size) = utf8.DecodeRuneInString(s);
            return size > 0L && unicode.IsLower(r);
        }
    }
}}
