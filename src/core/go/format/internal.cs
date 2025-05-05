// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO(gri): This file and the file src/cmd/gofmt/internal.go are
// the same (but for this comment and the package name). Do not modify
// one without the other. Determine if we can factor out functionality
// in a public API. See also #11844 for context.
namespace go.go;

using bytes = bytes_package;
using ast = go.ast_package;
using parser = go.parser_package;
using printer = go.printer_package;
using token = go.token_package;
using strings = strings_package;

partial class format_package {

// parse parses src, which was read from the named file,
// as a Go source file, declaration, or statement list.
internal static (ж<ast.File> file, Func<slice<byte>, nint, slice<byte>> sourceAdj, nint indentAdj, error err) parse(ж<token.FileSet> Ꮡfset, @string filename, slice<byte> src, bool fragmentOk) {
    ж<ast.File> file = default!;
    Func<slice<byte>, nint, slice<byte>> sourceAdj = default!;
    nint indentAdj = default!;
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    // Try as whole source file.
    (file, err) = parser.ParseFile(Ꮡfset, filename, src, parserMode);
    // If there's no error, return. If the error is that the source file didn't begin with a
    // package line and source fragments are ok, fall through to
    // try as a source fragment. Stop and return on any other error.
    if (err == default! || !fragmentOk || !strings.Contains(err.Error(), "expected 'package'"u8)) {
        return (file, sourceAdj, indentAdj, err);
    }
    // If this is a declaration list, make it a source file
    // by inserting a package clause.
    // Insert using a ';', not a newline, so that the line numbers
    // in psrc match the ones in src.
    var psrc = append(slice<byte>("package p;"), src.ꓸꓸꓸ);
    (file, err) = parser.ParseFile(Ꮡfset, filename, psrc, parserMode);
    if (err == default!) {
        sourceAdj = (slice<byte> src, nint indent) => {
            // Remove the package clause.
            // Gofmt has turned the ';' into a '\n'.
            srcΔ1 = srcΔ1[(int)(indent + len("package p\n"))..];
            return bytes.TrimSpace(srcΔ1);
        };
        return (file, sourceAdj, indentAdj, err);
    }
    // If the error is that the source file didn't begin with a
    // declaration, fall through to try as a statement list.
    // Stop and return on any other error.
    if (!strings.Contains(err.Error(), "expected declaration"u8)) {
        return (file, sourceAdj, indentAdj, err);
    }
    // If this is a statement list, make it a source file
    // by inserting a package clause and turning the list
    // into a function body. This handles expressions too.
    // Insert using a ';', not a newline, so that the line numbers
    // in fsrc match the ones in src. Add an extra '\n' before the '}'
    // to make sure comments are flushed before the '}'.
    var fsrc = append(append(slice<byte>("package p; func _() {"), src.ꓸꓸꓸ), (rune)'\n', (rune)'\n', (rune)'}');
    (file, err) = parser.ParseFile(Ꮡfset, filename, fsrc, parserMode);
    if (err == default!) {
        sourceAdj = (slice<byte> src, nint indent) => {
            // Cap adjusted indent to zero.
            if (indent < 0) {
                indent = 0;
            }
            // Remove the wrapping.
            // Gofmt has turned the "; " into a "\n\n".
            // There will be two non-blank lines with indent, hence 2*indent.
            srcΔ2 = srcΔ2[(int)(2 * indent + len("package p\n\nfunc _() {"))..];
            // Remove only the "}\n" suffix: remaining whitespaces will be trimmed anyway
            srcΔ2 = srcΔ2[..(int)(len(srcΔ2) - len("}\n"))];
            return bytes.TrimSpace(srcΔ2);
        };
        // Gofmt has also indented the function body one level.
        // Adjust that with indentAdj.
        indentAdj = -1;
    }
    // Succeeded, or out of options.
    return (file, sourceAdj, indentAdj, err);
}

// format formats the given package file originally obtained from src
// and adjusts the result based on the original source via sourceAdj
// and indentAdj.
internal static (slice<byte>, error) format(ж<token.FileSet> Ꮡfset, ж<ast.File> Ꮡfile, Func<slice<byte>, nint, slice<byte>> sourceAdj, nint indentAdj, slice<byte> src, printer.Config cfg) {
    ref var fset = ref Ꮡfset.val;
    ref var file = ref Ꮡfile.val;

    if (sourceAdj == default!) {
        // Complete source file.
        ref var bufΔ1 = ref heap(new bytes_package.Buffer(), out var ᏑbufΔ1);
        var errΔ1 = cfg.Fprint(~ᏑbufΔ1, Ꮡfset, file);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        return (bufΔ1.Bytes(), default!);
    }
    // Partial source file.
    // Determine and prepend leading space.
    nint i = 0;
    nint j = 0;
    while (j < len(src) && isSpace(src[j])) {
        if (src[j] == (rune)'\n') {
            i = j + 1;
        }
        // byte offset of last line in leading space
        j++;
    }
    slice<byte> res = default!;
    res = append(res, src[..(int)(i)].ꓸꓸꓸ);
    // Determine and prepend indentation of first code line.
    // Spaces are ignored unless there are no tabs,
    // in which case spaces count as one tab.
    nint indent = 0;
    var hasSpace = false;
    foreach (var (_, b) in src[(int)(i)..(int)(j)]) {
        switch (b) {
        case (rune)' ': {
            hasSpace = true;
            break;
        }
        case (rune)'\t': {
            indent++;
            break;
        }}

    }
    if (indent == 0 && hasSpace) {
        indent = 1;
    }
    for (nint iΔ1 = 0; iΔ1 < indent; iΔ1++) {
        res = append(res, (rune)'\t');
    }
    // Format the source.
    // Write it without any leading and trailing space.
    cfg.Indent = indent + indentAdj;
    ref var buf = ref heap(new bytes_package.Buffer(), out var Ꮡbuf);
    var err = cfg.Fprint(~Ꮡbuf, Ꮡfset, file);
    if (err != default!) {
        return (default!, err);
    }
    var @out = sourceAdj(buf.Bytes(), cfg.Indent);
    // If the adjusted output is empty, the source
    // was empty but (possibly) for white space.
    // The result is the incoming source.
    if (len(@out) == 0) {
        return (src, default!);
    }
    // Otherwise, append output to leading space.
    res = append(res, @out.ꓸꓸꓸ);
    // Determine and append trailing space.
    i = len(src);
    while (i > 0 && isSpace(src[i - 1])) {
        i--;
    }
    return (append(res, src[(int)(i)..].ꓸꓸꓸ), default!);
}

// isSpace reports whether the byte is a space character.
// isSpace defines a space as being among the following bytes: ' ', '\t', '\n' and '\r'.
internal static bool isSpace(byte b) {
    return b == (rune)' ' || b == (rune)'\t' || b == (rune)'\n' || b == (rune)'\r';
}

} // end format_package
