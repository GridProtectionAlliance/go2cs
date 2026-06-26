// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using cmp = cmp_package;
using token = go.token_package;
using slices = slices_package;
using strconv = strconv_package;

partial class ast_package {

// SortImports sorts runs of consecutive import lines in import blocks in f.
// It also removes duplicate imports when it is possible to do so without data loss.
public static void SortImports(ж<token.FileSet> Ꮡfset, ж<File> Ꮡf) {
    ref var fset = ref Ꮡfset.val;
    ref var f = ref Ꮡf.val;

    foreach (var (_, d) in f.Decls) {
        var (dΔ1, ok) = d._<GenDecl.val>(ᐧ);
        if (!ok || (~dΔ1).Tok != token.IMPORT) {
            // Not an import declaration, so we're done.
            // Imports are always first.
            break;
        }
        if (!(~dΔ1).Lparen.IsValid()) {
            // Not a block: sorted by default.
            continue;
        }
        // Identify and sort runs of specs on successive lines.
        nint i = 0;
        var specs = (~dΔ1).Specs[..0];
        foreach (var (j, s) in (~dΔ1).Specs) {
            if (j > i && lineAt(Ꮡfset, s.Pos()) > 1 + lineAt(Ꮡfset, (~dΔ1).Specs[j - 1].End())) {
                // j begins a new run. End this one.
                specs = append(specs, sortSpecs(Ꮡfset, Ꮡf, (~dΔ1).Specs[(int)(i)..(int)(j)]).ꓸꓸꓸ);
                i = j;
            }
        }
        specs = append(specs, sortSpecs(Ꮡfset, Ꮡf, (~dΔ1).Specs[(int)(i)..]).ꓸꓸꓸ);
        dΔ1.val.Specs = specs;
        // Deduping can leave a blank line before the rparen; clean that up.
        if (len((~dΔ1).Specs) > 0) {
            var lastSpec = (~dΔ1).Specs[len((~dΔ1).Specs) - 1];
            nint lastLine = lineAt(Ꮡfset, lastSpec.Pos());
            nint rParenLine = lineAt(Ꮡfset, (~dΔ1).Rparen);
            while (rParenLine > lastLine + 1) {
                rParenLine--;
                fset.File((~dΔ1).Rparen).MergeLine(rParenLine);
            }
        }
    }
}

internal static nint lineAt(ж<token.FileSet> Ꮡfset, tokenꓸPos pos) {
    ref var fset = ref Ꮡfset.val;

    return fset.PositionFor(pos, false).Line;
}

internal static @string importPath(Spec s) {
    var (t, err) = strconv.Unquote(s._<ImportSpec.val>().Path.Value);
    if (err == default!) {
        return t;
    }
    return ""u8;
}

internal static @string importName(Spec s) {
    var n = s._<ImportSpec.val>().Name;
    if (n == nil) {
        return ""u8;
    }
    return (~n).Name;
}

internal static @string importComment(Spec s) {
    var c = s._<ImportSpec.val>().Comment;
    if (c == nil) {
        return ""u8;
    }
    return c.Text();
}

// collapse indicates whether prev may be removed, leaving only next.
internal static bool collapse(Spec prev, Spec next) {
    if (importPath(next) != importPath(prev) || importName(next) != importName(prev)) {
        return false;
    }
    return prev._<ImportSpec.val>().Comment == nil;
}

[GoType] partial struct posSpan {
    public go.token_package.ΔPos Start;
    public go.token_package.ΔPos End;
}

[GoType] partial struct cgPos {
    internal bool left; // true if comment is to the left of the spec, false otherwise.
    internal ж<CommentGroup> cg;
}

internal static slice<Spec> sortSpecs(ж<token.FileSet> Ꮡfset, ж<File> Ꮡf, slice<Spec> specs) {
    ref var fset = ref Ꮡfset.val;
    ref var f = ref Ꮡf.val;

    // Can't short-circuit here even if specs are already sorted,
    // since they might yet need deduplication.
    // A lone import, however, may be safely ignored.
    if (len(specs) <= 1) {
        return specs;
    }
    // Record positions for specs.
    var pos = new slice<posSpan>(len(specs));
    foreach (var (i, s) in specs) {
        pos[i] = new posSpan(s.Pos(), s.End());
    }
    // Identify comments in this range.
    tokenꓸPos begSpecs = pos[0].Start;
    tokenꓸPos endSpecs = pos[len(pos) - 1].End;
    tokenꓸPos beg = fset.File(begSpecs).LineStart(lineAt(Ꮡfset, begSpecs));
    nint endLine = lineAt(Ꮡfset, endSpecs);
    var endFile = fset.File(endSpecs);
    tokenꓸPos end = default!;
    if (endLine == endFile.LineCount()){
        end = endSpecs;
    } else {
        end = endFile.LineStart(endLine + 1);
    }
    // beginning of next line
    nint first = len(f.Comments);
    nint last = -1;
    foreach (var (i, g) in f.Comments) {
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
    slice<ж<CommentGroup>> comments = default!;
    if (last >= 0) {
        comments = f.Comments[(int)(first)..(int)(last + 1)];
    }
    // Assign each comment to the import spec on the same line.
    var importComments = new map<ж<ImportSpec>, slice<cgPos>>{};
    nint specIndex = 0;
    foreach (var (_, g) in comments) {
        while (specIndex + 1 < len(specs) && pos[specIndex + 1].Start <= g.Pos()) {
            specIndex++;
        }
        bool left = default!;
        // A block comment can appear before the first import spec.
        if (specIndex == 0 && pos[specIndex].Start > g.Pos()){
            left = true;
        } else 
        if (specIndex + 1 < len(specs) && lineAt(Ꮡfset, // Or it can appear on the left of an import spec.
 pos[specIndex].Start) + 1 == lineAt(Ꮡfset, g.Pos())) {
            specIndex++;
            left = true;
        }
        var s = specs[specIndex]._<ImportSpec.val>();
        importComments[s] = append(importComments[s], new cgPos(left: left, cg: g));
    }
    // Sort the import specs by import path.
    // Remove duplicates, when possible without data loss.
    // Reassign the import paths to have the same position sequence.
    // Reassign each comment to the spec on the same line.
    // Sort the comments by new position.
    slices.SortFunc(specs, (Spec a, Spec b) => {
        @string ipath = importPath(a);
        @string jpath = importPath(b);
        nint r = cmp.Compare(ipath, jpath);
        if (r != 0) {
            return r;
        }
        @string iname = importName(a);
        @string jname = importName(b);
        r = cmp.Compare(iname, jname);
        if (r != 0) {
            return r;
        }
        return cmp.Compare(importComment(a), importComment(b));
    });
    // Dedup. Thanks to our sorting, we can just consider
    // adjacent pairs of imports.
    var deduped = specs[..0];
    foreach (var (i, s) in specs) {
        if (i == len(specs) - 1 || !collapse(s, specs[i + 1])){
            deduped = append(deduped, s);
        } else {
            tokenꓸPos p = s.Pos();
            fset.File(p).MergeLine(lineAt(Ꮡfset, p));
        }
    }
    specs = deduped;
    // Fix up comment positions
    foreach (var (i, s) in specs) {
        var sΔ1 = s._<ImportSpec.val>();
        if ((~sΔ1).Name != nil) {
            (~sΔ1).Name.val.NamePos = pos[i].Start;
        }
        (~sΔ1).Path.val.ValuePos = pos[i].Start;
        sΔ1.val.EndPos = pos[i].End;
        foreach (var (_, g) in importComments[sΔ1]) {
            foreach (var (_, c) in (~g.cg).List) {
                if (g.left){
                    c.val.Slash = pos[i].Start - 1;
                } else {
                    // An import spec can have both block comment and a line comment
                    // to its right. In that case, both of them will have the same pos.
                    // But while formatting the AST, the line comment gets moved to
                    // after the block comment.
                    c.val.Slash = pos[i].End;
                }
            }
        }
    }
    slices.SortFunc(comments, (ж<CommentGroup> a, ж<CommentGroup> b) => cmp.Compare(a.Pos(), b.Pos()));
    return specs;
}

} // end ast_package
