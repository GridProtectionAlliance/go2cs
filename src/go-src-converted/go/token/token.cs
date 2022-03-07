// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package token defines constants representing the lexical tokens of the Go
// programming language and basic operations on tokens (printing, predicates).
//
// package token -- go2cs converted at 2022 March 06 22:25:54 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Program Files\Go\src\go\token\token.go
using strconv = go.strconv_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

namespace go.go;

public static partial class token_package {

    // Token is the set of lexical tokens of the Go programming language.
public partial struct Token { // : nint
}

// The list of tokens.
 
// Special tokens
public static readonly Token ILLEGAL = iota;
public static readonly var EOF = 0;
public static readonly var COMMENT = 1;

private static readonly var literal_beg = 2; 
// Identifiers and basic type literals
// (these tokens stand for classes of literals)
public static readonly var IDENT = 3; // main
public static readonly var INT = 4; // 12345
public static readonly var FLOAT = 5; // 123.45
public static readonly var IMAG = 6; // 123.45i
public static readonly var CHAR = 7; // 'a'
public static readonly var STRING = 8; // "abc"
private static readonly var literal_end = 9;

private static readonly var operator_beg = 10; 
// Operators and delimiters
public static readonly var ADD = 11; // +
public static readonly var SUB = 12; // -
public static readonly var MUL = 13; // *
public static readonly var QUO = 14; // /
public static readonly var REM = 15; // %

public static readonly var AND = 16; // &
public static readonly var OR = 17; // |
public static readonly var XOR = 18; // ^
public static readonly var SHL = 19; // <<
public static readonly var SHR = 20; // >>
public static readonly var AND_NOT = 21; // &^

public static readonly var ADD_ASSIGN = 22; // +=
public static readonly var SUB_ASSIGN = 23; // -=
public static readonly var MUL_ASSIGN = 24; // *=
public static readonly var QUO_ASSIGN = 25; // /=
public static readonly var REM_ASSIGN = 26; // %=

public static readonly var AND_ASSIGN = 27; // &=
public static readonly var OR_ASSIGN = 28; // |=
public static readonly var XOR_ASSIGN = 29; // ^=
public static readonly var SHL_ASSIGN = 30; // <<=
public static readonly var SHR_ASSIGN = 31; // >>=
public static readonly var AND_NOT_ASSIGN = 32; // &^=

public static readonly var LAND = 33; // &&
public static readonly var LOR = 34; // ||
public static readonly var ARROW = 35; // <-
public static readonly var INC = 36; // ++
public static readonly var DEC = 37; // --

public static readonly var EQL = 38; // ==
public static readonly var LSS = 39; // <
public static readonly var GTR = 40; // >
public static readonly var ASSIGN = 41; // =
public static readonly var NOT = 42; // !

public static readonly var NEQ = 43; // !=
public static readonly var LEQ = 44; // <=
public static readonly var GEQ = 45; // >=
public static readonly var DEFINE = 46; // :=
public static readonly var ELLIPSIS = 47; // ...

public static readonly var LPAREN = 48; // (
public static readonly var LBRACK = 49; // [
public static readonly var LBRACE = 50; // {
public static readonly var COMMA = 51; // ,
public static readonly var PERIOD = 52; // .

public static readonly var RPAREN = 53; // )
public static readonly var RBRACK = 54; // ]
public static readonly var RBRACE = 55; // }
public static readonly var SEMICOLON = 56; // ;
public static readonly var COLON = 57; // :
private static readonly var operator_end = 58;

private static readonly var keyword_beg = 59; 
// Keywords
public static readonly var BREAK = 60;
public static readonly var CASE = 61;
public static readonly var CHAN = 62;
public static readonly var CONST = 63;
public static readonly var CONTINUE = 64;

public static readonly var DEFAULT = 65;
public static readonly var DEFER = 66;
public static readonly var ELSE = 67;
public static readonly var FALLTHROUGH = 68;
public static readonly var FOR = 69;

public static readonly var FUNC = 70;
public static readonly var GO = 71;
public static readonly var GOTO = 72;
public static readonly var IF = 73;
public static readonly var IMPORT = 74;

public static readonly var INTERFACE = 75;
public static readonly var MAP = 76;
public static readonly var PACKAGE = 77;
public static readonly var RANGE = 78;
public static readonly var RETURN = 79;

public static readonly var SELECT = 80;
public static readonly var STRUCT = 81;
public static readonly var SWITCH = 82;
public static readonly var TYPE = 83;
public static readonly var VAR = 84;
private static readonly var keyword_end = 85;


private static array<@string> tokens = new array<@string>(InitKeyedValues<@string>((ILLEGAL, "ILLEGAL"), (EOF, "EOF"), (COMMENT, "COMMENT"), (IDENT, "IDENT"), (INT, "INT"), (FLOAT, "FLOAT"), (IMAG, "IMAG"), (CHAR, "CHAR"), (STRING, "STRING"), (ADD, "+"), (SUB, "-"), (MUL, "*"), (QUO, "/"), (REM, "%"), (AND, "&"), (OR, "|"), (XOR, "^"), (SHL, "<<"), (SHR, ">>"), (AND_NOT, "&^"), (ADD_ASSIGN, "+="), (SUB_ASSIGN, "-="), (MUL_ASSIGN, "*="), (QUO_ASSIGN, "/="), (REM_ASSIGN, "%="), (AND_ASSIGN, "&="), (OR_ASSIGN, "|="), (XOR_ASSIGN, "^="), (SHL_ASSIGN, "<<="), (SHR_ASSIGN, ">>="), (AND_NOT_ASSIGN, "&^="), (LAND, "&&"), (LOR, "||"), (ARROW, "<-"), (INC, "++"), (DEC, "--"), (EQL, "=="), (LSS, "<"), (GTR, ">"), (ASSIGN, "="), (NOT, "!"), (NEQ, "!="), (LEQ, "<="), (GEQ, ">="), (DEFINE, ":="), (ELLIPSIS, "..."), (LPAREN, "("), (LBRACK, "["), (LBRACE, "{"), (COMMA, ","), (PERIOD, "."), (RPAREN, ")"), (RBRACK, "]"), (RBRACE, "}"), (SEMICOLON, ";"), (COLON, ":"), (BREAK, "break"), (CASE, "case"), (CHAN, "chan"), (CONST, "const"), (CONTINUE, "continue"), (DEFAULT, "default"), (DEFER, "defer"), (ELSE, "else"), (FALLTHROUGH, "fallthrough"), (FOR, "for"), (FUNC, "func"), (GO, "go"), (GOTO, "goto"), (IF, "if"), (IMPORT, "import"), (INTERFACE, "interface"), (MAP, "map"), (PACKAGE, "package"), (RANGE, "range"), (RETURN, "return"), (SELECT, "select"), (STRUCT, "struct"), (SWITCH, "switch"), (TYPE, "type"), (VAR, "var")));

// String returns the string corresponding to the token tok.
// For operators, delimiters, and keywords the string is the actual
// token character sequence (e.g., for the token ADD, the string is
// "+"). For all other tokens the string corresponds to the token
// constant name (e.g. for the token IDENT, the string is "IDENT").
//
public static @string String(this Token tok) {
    @string s = "";
    if (0 <= tok && tok < Token(len(tokens))) {
        s = tokens[tok];
    }
    if (s == "") {
        s = "token(" + strconv.Itoa(int(tok)) + ")";
    }
    return s;

}

// A set of constants for precedence-based expression parsing.
// Non-operators have lowest precedence, followed by operators
// starting with precedence 1 up to unary operators. The highest
// precedence serves as "catch-all" precedence for selector,
// indexing, and other operator and delimiter tokens.
//
public static readonly nint LowestPrec = 0; // non-operators
public static readonly nint UnaryPrec = 6;
public static readonly nint HighestPrec = 7;


// Precedence returns the operator precedence of the binary
// operator op. If op is not a binary operator, the result
// is LowestPrecedence.
//
public static nint Precedence(this Token op) {

    if (op == LOR) 
        return 1;
    else if (op == LAND) 
        return 2;
    else if (op == EQL || op == NEQ || op == LSS || op == LEQ || op == GTR || op == GEQ) 
        return 3;
    else if (op == ADD || op == SUB || op == OR || op == XOR) 
        return 4;
    else if (op == MUL || op == QUO || op == REM || op == SHL || op == SHR || op == AND || op == AND_NOT) 
        return 5;
        return LowestPrec;

}

private static map<@string, Token> keywords = default;

private static void init() {
    keywords = make_map<@string, Token>();
    for (var i = keyword_beg + 1; i < keyword_end; i++) {
        keywords[tokens[i]] = i;
    }
}

// Lookup maps an identifier to its keyword token or IDENT (if not a keyword).
//
public static Token Lookup(@string ident) {
    {
        var (tok, is_keyword) = keywords[ident];

        if (is_keyword) {
            return tok;
        }
    }

    return IDENT;

}

// Predicates

// IsLiteral returns true for tokens corresponding to identifiers
// and basic type literals; it returns false otherwise.
//
public static bool IsLiteral(this Token tok) {
    return literal_beg < tok && tok < literal_end;
}

// IsOperator returns true for tokens corresponding to operators and
// delimiters; it returns false otherwise.
//
public static bool IsOperator(this Token tok) {
    return operator_beg < tok && tok < operator_end;
}

// IsKeyword returns true for tokens corresponding to keywords;
// it returns false otherwise.
//
public static bool IsKeyword(this Token tok) {
    return keyword_beg < tok && tok < keyword_end;
}

// IsExported reports whether name starts with an upper-case letter.
//
public static bool IsExported(@string name) {
    var (ch, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(ch);
}

// IsKeyword reports whether name is a Go keyword, such as "func" or "return".
//
public static bool IsKeyword(@string name) { 
    // TODO: opt: use a perfect hash function instead of a global map.
    var (_, ok) = keywords[name];
    return ok;

}

// IsIdentifier reports whether name is a Go identifier, that is, a non-empty
// string made up of letters, digits, and underscores, where the first character
// is not a digit. Keywords are not identifiers.
//
public static bool IsIdentifier(@string name) {
    foreach (var (i, c) in name) {
        if (!unicode.IsLetter(c) && c != '_' && (i == 0 || !unicode.IsDigit(c))) {
            return false;
        }
    }    return name != "" && !IsKeyword(name);

}

} // end token_package
