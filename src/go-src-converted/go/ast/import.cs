// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2022 March 13 05:54:07 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\import.go
namespace go.go;

using token = go.token_package;
using sort = sort_package;
using strconv = strconv_package;


// SortImports sorts runs of consecutive import lines in import blocks in f.
// It also removes duplicate imports when it is possible to do so without data loss.

using System;
public static partial class ast_package {

public static void SortImports(ptr<token.FileSet> _addr_fset, ptr<File> _addr_f) {
    ref token.FileSet fset = ref _addr_fset.val;
    ref File f = ref _addr_f.val;

    {
        var d__prev1 = d;

        foreach (var (_, __d) in f.Decls) {
            d = __d;
            ptr<GenDecl> (d, ok) = d._<ptr<GenDecl>>();
            if (!ok || d.Tok != token.IMPORT) { 
                // Not an import declaration, so we're done.
                // Imports are always first.
                break;
            }
            if (!d.Lparen.IsValid()) { 
                // Not a block: sorted by default.
                continue;
            }
            nint i = 0;
            var specs = d.Specs[..(int)0];
            foreach (var (j, s) in d.Specs) {
                if (j > i && lineAt(_addr_fset, s.Pos()) > 1 + lineAt(_addr_fset, d.Specs[j - 1].End())) { 
                    // j begins a new run. End this one.
                    specs = append(specs, sortSpecs(_addr_fset, _addr_f, d.Specs[(int)i..(int)j]));
                    i = j;
                }
            }            specs = append(specs, sortSpecs(_addr_fset, _addr_f, d.Specs[(int)i..]));
            d.Specs = specs; 

            // Deduping can leave a blank line before the rparen; clean that up.
            if (len(d.Specs) > 0) {
                var lastSpec = d.Specs[len(d.Specs) - 1];
                var lastLine = lineAt(_addr_fset, lastSpec.Pos());
                var rParenLine = lineAt(_addr_fset, d.Rparen);
                while (rParenLine > lastLine + 1) {
                    rParenLine--;
                    fset.File(d.Rparen).MergeLine(rParenLine);
                }
            }
        }
        d = d__prev1;
    }
}

private static nint lineAt(ptr<token.FileSet> _addr_fset, token.Pos pos) {
    ref token.FileSet fset = ref _addr_fset.val;

    return fset.PositionFor(pos, false).Line;
}

private static @string importPath(Spec s) {
    var (t, err) = strconv.Unquote(s._<ptr<ImportSpec>>().Path.Value);
    if (err == null) {
        return t;
    }
    return "";
}

private static @string importName(Spec s) {
    ptr<ImportSpec> n = s._<ptr<ImportSpec>>().Name;
    if (n == null) {
        return "";
    }
    return n.Name;
}

private static @string importComment(Spec s) {
    ptr<ImportSpec> c = s._<ptr<ImportSpec>>().Comment;
    if (c == null) {
        return "";
    }
    return c.Text();
}

// collapse indicates whether prev may be removed, leaving only next.
private static bool collapse(Spec prev, Spec next) {
    if (importPath(next) != importPath(prev) || importName(next) != importName(prev)) {
        return false;
    }
    return prev._<ptr<ImportSpec>>().Comment == null;
}

private partial struct posSpan {
    public token.Pos Start;
    public token.Pos End;
}

private partial struct cgPos {
    public bool left; // true if comment is to the left of the spec, false otherwise.
    public ptr<CommentGroup> cg;
}

private static slice<Spec> sortSpecs(ptr<token.FileSet> _addr_fset, ptr<File> _addr_f, slice<Spec> specs) {
    ref token.FileSet fset = ref _addr_fset.val;
    ref File f = ref _addr_f.val;
 
    // Can't short-circuit here even if specs are already sorted,
    // since they might yet need deduplication.
    // A lone import, however, may be safely ignored.
    if (len(specs) <= 1) {
        return specs;
    }
    var pos = make_slice<posSpan>(len(specs));
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in specs) {
            i = __i;
            s = __s;
            pos[i] = new posSpan(s.Pos(),s.End());
        }
        i = i__prev1;
        s = s__prev1;
    }

    var begSpecs = pos[0].Start;
    var endSpecs = pos[len(pos) - 1].End;
    var beg = fset.File(begSpecs).LineStart(lineAt(_addr_fset, begSpecs));
    var endLine = lineAt(_addr_fset, endSpecs);
    var endFile = fset.File(endSpecs);
    token.Pos end = default;
    if (endLine == endFile.LineCount()) {
        end = endSpecs;
    }
    else
 {
        end = endFile.LineStart(endLine + 1); // beginning of next line
    }
    var first = len(f.Comments);
    nint last = -1;
    {
        var i__prev1 = i;
        var g__prev1 = g;

        foreach (var (__i, __g) in f.Comments) {
            i = __i;
            g = __g;
            if (g.End() >= end) {
                break;
            } 
            // g.End() < end
            if (beg <= g.Pos()) { 
                // comment is within the range [beg, end[ of import declarations
                if (i < first) {
                    first = i;
                }
                if (i > last) {
                    last = i;
                }
            }
        }
        i = i__prev1;
        g = g__prev1;
    }

    slice<ptr<CommentGroup>> comments = default;
    if (last >= 0) {
        comments = f.Comments[(int)first..(int)last + 1];
    }
    map importComments = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<ImportSpec>, slice<cgPos>>{};
    nint specIndex = 0;
    {
        var g__prev1 = g;

        foreach (var (_, __g) in comments) {
            g = __g;
            while (specIndex + 1 < len(specs) && pos[specIndex + 1].Start <= g.Pos()) {
                specIndex++;
            }

            bool left = default; 
            // A block comment can appear before the first import spec.
            if (specIndex == 0 && pos[specIndex].Start > g.Pos()) {
                left = true;
            }
            else if (specIndex + 1 < len(specs) && lineAt(_addr_fset, pos[specIndex].Start) + 1 == lineAt(_addr_fset, g.Pos())) {
                specIndex++;
                left = true;
            }
            ptr<ImportSpec> s = specs[specIndex]._<ptr<ImportSpec>>();
            importComments[s] = append(importComments[s], new cgPos(left:left,cg:g));
        }
        g = g__prev1;
    }

    sort.Slice(specs, (i, j) => {
        var ipath = importPath(specs[i]);
        var jpath = importPath(specs[j]);
        if (ipath != jpath) {
            return ipath < jpath;
        }
        var iname = importName(specs[i]);
        var jname = importName(specs[j]);
        if (iname != jname) {
            return iname < jname;
        }
        return importComment(specs[i]) < importComment(specs[j]);
    }); 

    // Dedup. Thanks to our sorting, we can just consider
    // adjacent pairs of imports.
    var deduped = specs[..(int)0];
    {
        var i__prev1 = i;
        var s__prev1 = s;

        foreach (var (__i, __s) in specs) {
            i = __i;
            s = __s;
            if (i == len(specs) - 1 || !collapse(s, specs[i + 1])) {
                deduped = append(deduped, s);
            }
            else
 {
                var p = s.Pos();
                fset.File(p).MergeLine(lineAt(_addr_fset, p));
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

        foreach (var (__i, __s) in specs) {
            i = __i;
            s = __s;
            s = s._<ptr<ImportSpec>>();
            if (s.Name != null) {
                s.Name.NamePos = pos[i].Start;
            }
            s.Path.ValuePos = pos[i].Start;
            s.EndPos = pos[i].End;
            {
                var g__prev2 = g;

                foreach (var (_, __g) in importComments[s]) {
                    g = __g;
                    foreach (var (_, c) in g.cg.List) {
                        if (g.left) {
                            c.Slash = pos[i].Start - 1;
                        }
                        else
 { 
                            // An import spec can have both block comment and a line comment
                            // to its right. In that case, both of them will have the same pos.
                            // But while formatting the AST, the line comment gets moved to
                            // after the block comment.
                            c.Slash = pos[i].End;
                        }
                    }
                }

                g = g__prev2;
            }
        }
        i = i__prev1;
        s = s__prev1;
    }

    sort.Slice(comments, (i, j) => comments[i].Pos() < comments[j].Pos());

    return specs;
}

} // end ast_package
