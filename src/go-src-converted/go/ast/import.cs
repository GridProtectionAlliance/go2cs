// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2020 August 29 08:48:33 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\import.go
using token = go.go.token_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // SortImports sorts runs of consecutive import lines in import blocks in f.
        // It also removes duplicate imports when it is possible to do so without data loss.
        public static void SortImports(ref token.FileSet fset, ref File f)
        {
            {
                var d__prev1 = d;

                foreach (var (_, __d) in f.Decls)
                {
                    d = __d;
                    ref GenDecl (d, ok) = d._<ref GenDecl>();
                    if (!ok || d.Tok != token.IMPORT)
                    { 
                        // Not an import declaration, so we're done.
                        // Imports are always first.
                        break;
                    }
                    if (!d.Lparen.IsValid())
                    { 
                        // Not a block: sorted by default.
                        continue;
                    }
                    long i = 0L;
                    var specs = d.Specs[..0L];
                    foreach (var (j, s) in d.Specs)
                    {
                        if (j > i && fset.Position(s.Pos()).Line > 1L + fset.Position(d.Specs[j - 1L].End()).Line)
                        { 
                            // j begins a new run. End this one.
                            specs = append(specs, sortSpecs(fset, f, d.Specs[i..j]));
                            i = j;
                        }
                    }                    specs = append(specs, sortSpecs(fset, f, d.Specs[i..]));
                    d.Specs = specs; 

                    // Deduping can leave a blank line before the rparen; clean that up.
                    if (len(d.Specs) > 0L)
                    {
                        var lastSpec = d.Specs[len(d.Specs) - 1L];
                        var lastLine = fset.Position(lastSpec.Pos()).Line;
                        var rParenLine = fset.Position(d.Rparen).Line;
                        while (rParenLine > lastLine + 1L)
                        {
                            rParenLine--;
                            fset.File(d.Rparen).MergeLine(rParenLine);
                        }
                    }
                }
                d = d__prev1;
            }

        }

        private static @string importPath(Spec s)
        {
            var (t, err) = strconv.Unquote(s._<ref ImportSpec>().Path.Value);
            if (err == null)
            {
                return t;
            }
            return "";
        }

        private static @string importName(Spec s)
        {
            ref ImportSpec n = s._<ref ImportSpec>().Name;
            if (n == null)
            {
                return "";
            }
            return n.Name;
        }

        private static @string importComment(Spec s)
        {
            ref ImportSpec c = s._<ref ImportSpec>().Comment;
            if (c == null)
            {
                return "";
            }
            return c.Text();
        }

        // collapse indicates whether prev may be removed, leaving only next.
        private static bool collapse(Spec prev, Spec next)
        {
            if (importPath(next) != importPath(prev) || importName(next) != importName(prev))
            {
                return false;
            }
            return prev._<ref ImportSpec>().Comment == null;
        }

        private partial struct posSpan
        {
            public token.Pos Start;
            public token.Pos End;
        }

        private static slice<Spec> sortSpecs(ref token.FileSet fset, ref File f, slice<Spec> specs)
        { 
            // Can't short-circuit here even if specs are already sorted,
            // since they might yet need deduplication.
            // A lone import, however, may be safely ignored.
            if (len(specs) <= 1L)
            {
                return specs;
            } 

            // Record positions for specs.
            var pos = make_slice<posSpan>(len(specs));
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in specs)
                {
                    i = __i;
                    s = __s;
                    pos[i] = new posSpan(s.Pos(),s.End());
                } 

                // Identify comments in this range.
                // Any comment from pos[0].Start to the final line counts.

                i = i__prev1;
                s = s__prev1;
            }

            var lastLine = fset.Position(pos[len(pos) - 1L].End).Line;
            var cstart = len(f.Comments);
            var cend = len(f.Comments);
            {
                var i__prev1 = i;
                var g__prev1 = g;

                foreach (var (__i, __g) in f.Comments)
                {
                    i = __i;
                    g = __g;
                    if (g.Pos() < pos[0L].Start)
                    {
                        continue;
                    }
                    if (i < cstart)
                    {
                        cstart = i;
                    }
                    if (fset.Position(g.End()).Line > lastLine)
                    {
                        cend = i;
                        break;
                    }
                }

                i = i__prev1;
                g = g__prev1;
            }

            var comments = f.Comments[cstart..cend]; 

            // Assign each comment to the import spec preceding it.
            map importComments = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref ImportSpec, slice<ref CommentGroup>>{};
            long specIndex = 0L;
            {
                var g__prev1 = g;

                foreach (var (_, __g) in comments)
                {
                    g = __g;
                    while (specIndex + 1L < len(specs) && pos[specIndex + 1L].Start <= g.Pos())
                    {
                        specIndex++;
                    }

                    ref ImportSpec s = specs[specIndex]._<ref ImportSpec>();
                    importComments[s] = append(importComments[s], g);
                } 

                // Sort the import specs by import path.
                // Remove duplicates, when possible without data loss.
                // Reassign the import paths to have the same position sequence.
                // Reassign each comment to abut the end of its spec.
                // Sort the comments by new position.

                g = g__prev1;
            }

            sort.Slice(specs, (i, j) =>
            {
                var ipath = importPath(specs[i]);
                var jpath = importPath(specs[j]);
                if (ipath != jpath)
                {
                    return ipath < jpath;
                }
                var iname = importName(specs[i]);
                var jname = importName(specs[j]);
                if (iname != jname)
                {
                    return iname < jname;
                }
                return importComment(specs[i]) < importComment(specs[j]);
            }); 

            // Dedup. Thanks to our sorting, we can just consider
            // adjacent pairs of imports.
            var deduped = specs[..0L];
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in specs)
                {
                    i = __i;
                    s = __s;
                    if (i == len(specs) - 1L || !collapse(s, specs[i + 1L]))
                    {
                        deduped = append(deduped, s);
                    }
                    else
                    {
                        var p = s.Pos();
                        fset.File(p).MergeLine(fset.Position(p).Line);
                    }
                }

                i = i__prev1;
                s = s__prev1;
            }

            specs = deduped; 

            // Fix up comment positions
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in specs)
                {
                    i = __i;
                    s = __s;
                    s = s._<ref ImportSpec>();
                    if (s.Name != null)
                    {
                        s.Name.NamePos = pos[i].Start;
                    }
                    s.Path.ValuePos = pos[i].Start;
                    s.EndPos = pos[i].End;
                    {
                        var g__prev2 = g;

                        foreach (var (_, __g) in importComments[s])
                        {
                            g = __g;
                            foreach (var (_, c) in g.List)
                            {
                                c.Slash = pos[i].End;
                            }
                        }

                        g = g__prev2;
                    }

                }

                i = i__prev1;
                s = s__prev1;
            }

            sort.Slice(comments, (i, j) =>
            {
                return comments[i].Pos() < comments[j].Pos();
            });

            return specs;
        }
    }
}}
