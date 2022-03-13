// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lex -- go2cs converted at 2022 March 13 05:57:41 UTC
// import "cmd/asm/internal/lex" ==> using lex = go.cmd.asm.@internal.lex_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\lex\slice.go
namespace go.cmd.asm.@internal;

using scanner = text.scanner_package;

using src = cmd.@internal.src_package;


// A Slice reads from a slice of Tokens.

public static partial class lex_package {

public partial struct Slice {
    public slice<Token> tokens;
    public ptr<src.PosBase> @base;
    public nint line;
    public nint pos;
}

public static ptr<Slice> NewSlice(ptr<src.PosBase> _addr_@base, nint line, slice<Token> tokens) {
    ref src.PosBase @base = ref _addr_@base.val;

    return addr(new Slice(tokens:tokens,base:base,line:line,pos:-1,));
}

private static ScanToken Next(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    s.pos++;
    if (s.pos >= len(s.tokens)) {
        return scanner.EOF;
    }
    return s.tokens[s.pos].ScanToken;
}

private static @string Text(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    return s.tokens[s.pos].text;
}

private static @string File(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    return s.@base.Filename();
}

private static ptr<src.PosBase> Base(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    return _addr_s.@base!;
}

private static void SetBase(this ptr<Slice> _addr_s, ptr<src.PosBase> _addr_@base) {
    ref Slice s = ref _addr_s.val;
    ref src.PosBase @base = ref _addr_@base.val;
 
    // Cannot happen because we only have slices of already-scanned text,
    // but be prepared.
    s.@base = base;
}

private static nint Line(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    return s.line;
}

private static nint Col(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;
 
    // TODO: Col is only called when defining a macro and all it cares about is increasing
    // position to discover whether there is a blank before the parenthesis.
    // We only get here if defining a macro inside a macro.
    // This imperfect implementation means we cannot tell the difference between
    //    #define A #define B(x) x
    // and
    //    #define A #define B (x) x
    // The first has definition of B has an argument, the second doesn't. Because we let
    // text/scanner strip the blanks for us, this is extremely rare, hard to fix, and not worth it.
    return s.pos;
}

private static void Close(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

}

} // end lex_package
