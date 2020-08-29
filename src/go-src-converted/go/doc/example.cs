// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Extract example functions from file ASTs.

// package doc -- go2cs converted at 2020 August 29 08:47:04 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\example.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using path = go.path_package;
using regexp = go.regexp_package;
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
        // An Example represents an example function found in a source files.
        public partial struct Example
        {
            public @string Name; // name of the item being exemplified
            public @string Doc; // example function doc string
            public ast.Node Code;
            public ptr<ast.File> Play; // a whole program version of the example
            public slice<ref ast.CommentGroup> Comments;
            public @string Output; // expected output
            public bool Unordered;
            public bool EmptyOutput; // expect empty output
            public long Order; // original source code order
        }

        // Examples returns the examples found in the files, sorted by Name field.
        // The Order fields record the order in which the examples were encountered.
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
        public static slice<ref Example> Examples(params ptr<ast.File>[] files)
        {
            files = files.Clone();

            slice<ref Example> list = default;
            foreach (var (_, file) in files)
            {
                var hasTests = false; // file contains tests or benchmarks
                long numDecl = 0L; // number of non-import declarations in the file
                slice<ref Example> flist = default;
                foreach (var (_, decl) in file.Decls)
                {
                    {
                        ref ast.GenDecl (g, ok) = decl._<ref ast.GenDecl>();

                        if (ok && g.Tok != token.IMPORT)
                        {
                            numDecl++;
                            continue;
                        }

                    }
                    ref ast.FuncDecl (f, ok) = decl._<ref ast.FuncDecl>();
                    if (!ok)
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
                    @string doc = default;
                    if (f.Doc != null)
                    {
                        doc = f.Doc.Text();
                    }
                    var (output, unordered, hasOutput) = exampleOutput(f.Body, file.Comments);
                    flist = append(flist, ref new Example(Name:name[len("Example"):],Doc:doc,Code:f.Body,Play:playExample(file,f.Body),Comments:file.Comments,Output:output,Unordered:unordered,EmptyOutput:output==""&&hasOutput,Order:len(flist),));
                }
                if (!hasTests && numDecl > 1L && len(flist) == 1L)
                { 
                    // If this file only has one example function, some
                    // other top-level declarations, and no tests or
                    // benchmarks, use the whole file as the example.
                    flist[0L].Code = file;
                    flist[0L].Play = playExampleFile(file);
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

        private static var outputPrefix = regexp.MustCompile("(?i)^[[:space:]]*(unordered )?output:");

        // Extracts the expected output and whether there was a valid output comment
        private static (@string, bool, bool) exampleOutput(ref ast.BlockStmt b, slice<ref ast.CommentGroup> comments)
        {
            {
                var (_, last) = lastComment(b, comments);

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
        private static ref ast.File playExample(ref ast.File file, ref ast.BlockStmt body)
        {
            if (!strings.HasSuffix(file.Name.Name, "_test"))
            { 
                // We don't support examples that are part of the
                // greater package (yet).
                return null;
            } 

            // Find top-level declarations in the file.
            var topDecls = make_map<ref ast.Object, bool>();
            foreach (var (_, decl) in file.Decls)
            {
                switch (decl.type())
                {
                    case ref ast.FuncDecl d:
                        topDecls[d.Name.Obj] = true;
                        break;
                    case ref ast.GenDecl d:
                        foreach (var (_, spec) in d.Specs)
                        {
                            switch (spec.type())
                            {
                                case ref ast.TypeSpec s:
                                    topDecls[s.Name.Obj] = true;
                                    break;
                                case ref ast.ValueSpec s:
                                    {
                                        var id__prev3 = id;

                                        foreach (var (_, __id) in s.Names)
                                        {
                                            id = __id;
                                            topDecls[id.Obj] = true;
                                        }

                                        id = id__prev3;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            } 

            // Find unresolved identifiers and uses of top-level declarations.
            var unresolved = make_map<@string, bool>();
            var usesTopDecl = false;
            Func<ast.Node, bool> inspectFunc = default;
            inspectFunc = n =>
            { 
                // For selector expressions, only inspect the left hand side.
                // (For an expression like fmt.Println, only add "fmt" to the
                // set of unresolved names, not "Println".)
                {
                    ref ast.SelectorExpr e__prev1 = e;

                    ref ast.SelectorExpr (e, ok) = n._<ref ast.SelectorExpr>();

                    if (ok)
                    {
                        ast.Inspect(e.X, inspectFunc);
                        return false;
                    } 
                    // For key value expressions, only inspect the value
                    // as the key should be resolved by the type of the
                    // composite literal.

                    e = e__prev1;

                } 
                // For key value expressions, only inspect the value
                // as the key should be resolved by the type of the
                // composite literal.
                {
                    ref ast.SelectorExpr e__prev1 = e;

                    (e, ok) = n._<ref ast.KeyValueExpr>();

                    if (ok)
                    {
                        ast.Inspect(e.Value, inspectFunc);
                        return false;
                    }

                    e = e__prev1;

                }
                {
                    var id__prev1 = id;

                    ref ast.Ident (id, ok) = n._<ref ast.Ident>();

                    if (ok)
                    {
                        if (id.Obj == null)
                        {
                            unresolved[id.Name] = true;
                        }
                        else if (topDecls[id.Obj])
                        {
                            usesTopDecl = true;
                        }
                    }

                    id = id__prev1;

                }
                return true;
            }
;
            ast.Inspect(body, inspectFunc);
            if (usesTopDecl)
            { 
                // We don't support examples that are not self-contained (yet).
                return null;
            } 

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
                                return null;
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
                return null;
            } 

            // Include documentation belonging to blank imports.
            slice<ref ast.CommentGroup> comments = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in blankImports)
                {
                    s = __s;
                    {
                        ref ast.ImportSpec c__prev1 = c;

                        ref ast.ImportSpec c = s._<ref ast.ImportSpec>().Doc;

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
                ref ast.ImportSpec c__prev1 = c;

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

            body, comments = stripOutputComment(body, comments); 

            // Synthesize import declaration.
            ast.GenDecl importDecl = ref new ast.GenDecl(Tok:token.IMPORT,Lparen:1,Rparen:1,);
            {
                var n__prev1 = n;
                var p__prev1 = p;

                foreach (var (__n, __p) in namedImports)
                {
                    n = __n;
                    p = __p;
                    ast.ImportSpec s = ref new ast.ImportSpec(Path:&ast.BasicLit{Value:strconv.Quote(p)});
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
            ast.FuncDecl funcDecl = ref new ast.FuncDecl(Name:ast.NewIdent("main"),Type:&ast.FuncType{Params:&ast.FieldList{}},Body:body,); 

            // Synthesize file.
            return ref new ast.File(Name:ast.NewIdent("main"),Decls:[]ast.Decl{importDecl,funcDecl},Comments:comments,);
        }

        // playExampleFile takes a whole file example and synthesizes a new *ast.File
        // such that the example is function main in package main.
        private static ref ast.File playExampleFile(ref ast.File file)
        { 
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
                    ref ast.FuncDecl f__prev1 = f;

                    ref ast.FuncDecl (f, ok) = d._<ref ast.FuncDecl>();

                    if (ok && isTest(f.Name.Name, "Example"))
                    { 
                        // Copy the FuncDecl, as it may be used elsewhere.
                        var newF = f.Value;
                        newF.Name = ast.NewIdent("main");
                        newF.Body, comments = stripOutputComment(f.Body, comments);
                        d = ref newF;
                    }

                    f = f__prev1;

                }
                decls = append(decls, d);
            } 

            // Copy the File, as it may be used elsewhere.
            var f = file.Value;
            f.Name = ast.NewIdent("main");
            f.Decls = decls;
            f.Comments = comments;
            return ref f;
        }

        // stripOutputComment finds and removes the "Output:" or "Unordered output:"
        // comment from body and comments, and adjusts the body block's end position.
        private static (ref ast.BlockStmt, slice<ref ast.CommentGroup>) stripOutputComment(ref ast.BlockStmt body, slice<ref ast.CommentGroup> comments)
        { 
            // Do nothing if there is no "Output:" or "Unordered output:" comment.
            var (i, last) = lastComment(body, comments);
            if (last == null || !outputPrefix.MatchString(last.Text()))
            {
                return (body, comments);
            } 

            // Copy body and comments, as the originals may be used elsewhere.
            ast.BlockStmt newBody = ref new ast.BlockStmt(Lbrace:body.Lbrace,List:body.List,Rbrace:last.Pos(),);
            var newComments = make_slice<ref ast.CommentGroup>(len(comments) - 1L);
            copy(newComments, comments[..i]);
            copy(newComments[i..], comments[i + 1L..]);
            return (newBody, newComments);
        }

        // lastComment returns the last comment inside the provided block.
        private static (long, ref ast.CommentGroup) lastComment(ref ast.BlockStmt b, slice<ref ast.CommentGroup> c)
        {
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
            return;
        }
    }
}}
