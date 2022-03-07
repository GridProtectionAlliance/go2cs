// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lex implements lexical analysis for the assembler.
// package lex -- go2cs converted at 2022 March 06 22:46:28 UTC
// import "cmd/asm/internal/lex" ==> using lex = go.cmd.asm.@internal.lex_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\lex\lex.go
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;

using src = go.cmd.@internal.src_package;

namespace go.cmd.asm.@internal;

public static partial class lex_package {

    // A ScanToken represents an input item. It is a simple wrapping of rune, as
    // returned by text/scanner.Scanner, plus a couple of extra values.
public partial struct ScanToken { // : int
}

 
// Asm defines some two-character lexemes. We make up
// a rune/ScanToken value for them - ugly but simple.
public static readonly ScanToken LSH = -1000 - iota; // << Left shift.
public static readonly var RSH = 0; // >> Logical right shift.
public static readonly var ARR = 1; // -> Used on ARM for shift type 3, arithmetic right shift.
public static readonly var ROT = 2; // @> Used on ARM for shift type 4, rotate right.
public static readonly var Include = 3; // included file started here
public static readonly var BuildComment = 4; // //go:build or +build comment
private static readonly var macroName = 5; // name of macro that should not be expanded

// IsRegisterShift reports whether the token is one of the ARM register shift operators.
public static bool IsRegisterShift(ScanToken r) {
    return ROT <= r && r <= LSH; // Order looks backwards because these are negative.
}

public static @string String(this ScanToken t) {

    if (t == scanner.EOF) 
        return "EOF";
    else if (t == scanner.Ident) 
        return "identifier";
    else if (t == scanner.Int) 
        return "integer constant";
    else if (t == scanner.Float) 
        return "float constant";
    else if (t == scanner.Char) 
        return "rune constant";
    else if (t == scanner.String) 
        return "string constant";
    else if (t == scanner.RawString) 
        return "raw string constant";
    else if (t == scanner.Comment) 
        return "comment";
    else 
        return fmt.Sprintf("%q", rune(t));
    
}

// NewLexer returns a lexer for the named file and the given link context.
public static TokenReader NewLexer(@string name) {
    var input = NewInput(name);
    var (fd, err) = os.Open(name);
    if (err != null) {
        log.Fatalf("%s\n", err);
    }
    input.Push(NewTokenizer(name, fd, fd));
    return input;

}

// The other files in this directory each contain an implementation of TokenReader.

// A TokenReader is like a reader, but returns lex tokens of type Token. It also can tell you what
// the text of the most recently returned token is, and where it was found.
// The underlying scanner elides all spaces except newline, so the input looks like a stream of
// Tokens; original spacing is lost but we don't need it.
public partial interface TokenReader {
    nint Next(); // The following methods all refer to the most recent token returned by Next.
// Text returns the original string representation of the token.
    nint Text(); // File reports the source file name of the token.
    nint File(); // Base reports the position base of the token.
    nint Base(); // SetBase sets the position base.
    nint SetBase(ptr<src.PosBase> _p0); // Line reports the source line number of the token.
    nint Line(); // Col reports the source column number of the token.
    nint Col(); // Close does any teardown required.
    nint Close();
}

// A Token is a scan token plus its string value.
// A macro is stored as a sequence of Tokens with spaces stripped.
public partial struct Token {
    public ref ScanToken ScanToken => ref ScanToken_val;
    public @string text;
}

// Make returns a Token with the given rune (ScanToken) and text representation.
public static Token Make(ScanToken token, @string text) { 
    // If the symbol starts with center dot, as in ·x, rewrite it as ""·x
    if (token == scanner.Ident && strings.HasPrefix(text, "\u00B7")) {
        text = "\"\"" + text;
    }
    text = strings.Replace(text, "\u00B7", ".", -1);
    text = strings.Replace(text, "\u2215", "/", -1);
    return new Token(ScanToken:token,text:text);

}

public static @string String(this Token l) {
    return l.text;
}

// A Macro represents the definition of a #defined macro.
public partial struct Macro {
    public @string name; // The #define name.
    public slice<@string> args; // Formal arguments.
    public slice<Token> tokens; // Body of macro.
}

// Tokenize turns a string into a list of Tokens; used to parse the -D flag and in tests.
public static slice<Token> Tokenize(@string str) {
    var t = NewTokenizer("command line", strings.NewReader(str), null);
    slice<Token> tokens = default;
    while (true) {
        var tok = t.Next();
        if (tok == scanner.EOF) {
            break;
        }
        tokens = append(tokens, Make(tok, t.Text()));

    }
    return tokens;

}

} // end lex_package
