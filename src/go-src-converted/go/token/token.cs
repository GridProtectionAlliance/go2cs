// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package token defines constants representing the lexical tokens of the Go
// programming language and basic operations on tokens (printing, predicates).
//
// package token -- go2cs converted at 2020 October 08 03:43:28 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Go\src\go\token\token.go
using strconv = go.strconv_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class token_package
    {
        // Token is the set of lexical tokens of the Go programming language.
        public partial struct Token // : long
        {
        }

        // The list of tokens.
 
        // Special tokens
        public static readonly Token ILLEGAL = (Token)iota;
        public static readonly var EOF = (var)0;
        public static readonly var COMMENT = (var)1;

        private static readonly var literal_beg = (var)2; 
        // Identifiers and basic type literals
        // (these tokens stand for classes of literals)
        public static readonly var IDENT = (var)3; // main
        public static readonly var INT = (var)4; // 12345
        public static readonly var FLOAT = (var)5; // 123.45
        public static readonly var IMAG = (var)6; // 123.45i
        public static readonly var CHAR = (var)7; // 'a'
        public static readonly var STRING = (var)8; // "abc"
        private static readonly var literal_end = (var)9;

        private static readonly var operator_beg = (var)10; 
        // Operators and delimiters
        public static readonly var ADD = (var)11; // +
        public static readonly var SUB = (var)12; // -
        public static readonly var MUL = (var)13; // *
        public static readonly var QUO = (var)14; // /
        public static readonly var REM = (var)15; // %

        public static readonly var AND = (var)16; // &
        public static readonly var OR = (var)17; // |
        public static readonly var XOR = (var)18; // ^
        public static readonly var SHL = (var)19; // <<
        public static readonly var SHR = (var)20; // >>
        public static readonly var AND_NOT = (var)21; // &^

        public static readonly var ADD_ASSIGN = (var)22; // +=
        public static readonly var SUB_ASSIGN = (var)23; // -=
        public static readonly var MUL_ASSIGN = (var)24; // *=
        public static readonly var QUO_ASSIGN = (var)25; // /=
        public static readonly var REM_ASSIGN = (var)26; // %=

        public static readonly var AND_ASSIGN = (var)27; // &=
        public static readonly var OR_ASSIGN = (var)28; // |=
        public static readonly var XOR_ASSIGN = (var)29; // ^=
        public static readonly var SHL_ASSIGN = (var)30; // <<=
        public static readonly var SHR_ASSIGN = (var)31; // >>=
        public static readonly var AND_NOT_ASSIGN = (var)32; // &^=

        public static readonly var LAND = (var)33; // &&
        public static readonly var LOR = (var)34; // ||
        public static readonly var ARROW = (var)35; // <-
        public static readonly var INC = (var)36; // ++
        public static readonly var DEC = (var)37; // --

        public static readonly var EQL = (var)38; // ==
        public static readonly var LSS = (var)39; // <
        public static readonly var GTR = (var)40; // >
        public static readonly var ASSIGN = (var)41; // =
        public static readonly var NOT = (var)42; // !

        public static readonly var NEQ = (var)43; // !=
        public static readonly var LEQ = (var)44; // <=
        public static readonly var GEQ = (var)45; // >=
        public static readonly var DEFINE = (var)46; // :=
        public static readonly var ELLIPSIS = (var)47; // ...

        public static readonly var LPAREN = (var)48; // (
        public static readonly var LBRACK = (var)49; // [
        public static readonly var LBRACE = (var)50; // {
        public static readonly var COMMA = (var)51; // ,
        public static readonly var PERIOD = (var)52; // .

        public static readonly var RPAREN = (var)53; // )
        public static readonly var RBRACK = (var)54; // ]
        public static readonly var RBRACE = (var)55; // }
        public static readonly var SEMICOLON = (var)56; // ;
        public static readonly var COLON = (var)57; // :
        private static readonly var operator_end = (var)58;

        private static readonly var keyword_beg = (var)59; 
        // Keywords
        public static readonly var BREAK = (var)60;
        public static readonly var CASE = (var)61;
        public static readonly var CHAN = (var)62;
        public static readonly var CONST = (var)63;
        public static readonly var CONTINUE = (var)64;

        public static readonly var DEFAULT = (var)65;
        public static readonly var DEFER = (var)66;
        public static readonly var ELSE = (var)67;
        public static readonly var FALLTHROUGH = (var)68;
        public static readonly var FOR = (var)69;

        public static readonly var FUNC = (var)70;
        public static readonly var GO = (var)71;
        public static readonly var GOTO = (var)72;
        public static readonly var IF = (var)73;
        public static readonly var IMPORT = (var)74;

        public static readonly var INTERFACE = (var)75;
        public static readonly var MAP = (var)76;
        public static readonly var PACKAGE = (var)77;
        public static readonly var RANGE = (var)78;
        public static readonly var RETURN = (var)79;

        public static readonly var SELECT = (var)80;
        public static readonly var STRUCT = (var)81;
        public static readonly var SWITCH = (var)82;
        public static readonly var TYPE = (var)83;
        public static readonly var VAR = (var)84;
        private static readonly var keyword_end = (var)85;


        private static array<@string> tokens = new array<@string>(InitKeyedValues<@string>((ILLEGAL, "ILLEGAL"), (EOF, "EOF"), (COMMENT, "COMMENT"), (IDENT, "IDENT"), (INT, "INT"), (FLOAT, "FLOAT"), (IMAG, "IMAG"), (CHAR, "CHAR"), (STRING, "STRING"), (ADD, "+"), (SUB, "-"), (MUL, "*"), (QUO, "/"), (REM, "%"), (AND, "&"), (OR, "|"), (XOR, "^"), (SHL, "<<"), (SHR, ">>"), (AND_NOT, "&^"), (ADD_ASSIGN, "+="), (SUB_ASSIGN, "-="), (MUL_ASSIGN, "*="), (QUO_ASSIGN, "/="), (REM_ASSIGN, "%="), (AND_ASSIGN, "&="), (OR_ASSIGN, "|="), (XOR_ASSIGN, "^="), (SHL_ASSIGN, "<<="), (SHR_ASSIGN, ">>="), (AND_NOT_ASSIGN, "&^="), (LAND, "&&"), (LOR, "||"), (ARROW, "<-"), (INC, "++"), (DEC, "--"), (EQL, "=="), (LSS, "<"), (GTR, ">"), (ASSIGN, "="), (NOT, "!"), (NEQ, "!="), (LEQ, "<="), (GEQ, ">="), (DEFINE, ":="), (ELLIPSIS, "..."), (LPAREN, "("), (LBRACK, "["), (LBRACE, "{"), (COMMA, ","), (PERIOD, "."), (RPAREN, ")"), (RBRACK, "]"), (RBRACE, "}"), (SEMICOLON, ";"), (COLON, ":"), (BREAK, "break"), (CASE, "case"), (CHAN, "chan"), (CONST, "const"), (CONTINUE, "continue"), (DEFAULT, "default"), (DEFER, "defer"), (ELSE, "else"), (FALLTHROUGH, "fallthrough"), (FOR, "for"), (FUNC, "func"), (GO, "go"), (GOTO, "goto"), (IF, "if"), (IMPORT, "import"), (INTERFACE, "interface"), (MAP, "map"), (PACKAGE, "package"), (RANGE, "range"), (RETURN, "return"), (SELECT, "select"), (STRUCT, "struct"), (SWITCH, "switch"), (TYPE, "type"), (VAR, "var")));

        // String returns the string corresponding to the token tok.
        // For operators, delimiters, and keywords the string is the actual
        // token character sequence (e.g., for the token ADD, the string is
        // "+"). For all other tokens the string corresponds to the token
        // constant name (e.g. for the token IDENT, the string is "IDENT").
        //
        public static @string String(this Token tok)
        {
            @string s = "";
            if (0L <= tok && tok < Token(len(tokens)))
            {
                s = tokens[tok];
            }

            if (s == "")
            {
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
        public static readonly long LowestPrec = (long)0L; // non-operators
        public static readonly long UnaryPrec = (long)6L;
        public static readonly long HighestPrec = (long)7L;


        // Precedence returns the operator precedence of the binary
        // operator op. If op is not a binary operator, the result
        // is LowestPrecedence.
        //
        public static long Precedence(this Token op)
        {

            if (op == LOR) 
                return 1L;
            else if (op == LAND) 
                return 2L;
            else if (op == EQL || op == NEQ || op == LSS || op == LEQ || op == GTR || op == GEQ) 
                return 3L;
            else if (op == ADD || op == SUB || op == OR || op == XOR) 
                return 4L;
            else if (op == MUL || op == QUO || op == REM || op == SHL || op == SHR || op == AND || op == AND_NOT) 
                return 5L;
                        return LowestPrec;

        }

        private static map<@string, Token> keywords = default;

        private static void init()
        {
            keywords = make_map<@string, Token>();
            for (var i = keyword_beg + 1L; i < keyword_end; i++)
            {
                keywords[tokens[i]] = i;
            }


        }

        // Lookup maps an identifier to its keyword token or IDENT (if not a keyword).
        //
        public static Token Lookup(@string ident)
        {
            {
                var (tok, is_keyword) = keywords[ident];

                if (is_keyword)
                {
                    return tok;
                }

            }

            return IDENT;

        }

        // Predicates

        // IsLiteral returns true for tokens corresponding to identifiers
        // and basic type literals; it returns false otherwise.
        //
        public static bool IsLiteral(this Token tok)
        {
            return literal_beg < tok && tok < literal_end;
        }

        // IsOperator returns true for tokens corresponding to operators and
        // delimiters; it returns false otherwise.
        //
        public static bool IsOperator(this Token tok)
        {
            return operator_beg < tok && tok < operator_end;
        }

        // IsKeyword returns true for tokens corresponding to keywords;
        // it returns false otherwise.
        //
        public static bool IsKeyword(this Token tok)
        {
            return keyword_beg < tok && tok < keyword_end;
        }

        // IsExported reports whether name starts with an upper-case letter.
        //
        public static bool IsExported(@string name)
        {
            var (ch, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(ch);
        }

        // IsKeyword reports whether name is a Go keyword, such as "func" or "return".
        //
        public static bool IsKeyword(@string name)
        { 
            // TODO: opt: use a perfect hash function instead of a global map.
            var (_, ok) = keywords[name];
            return ok;

        }

        // IsIdentifier reports whether name is a Go identifier, that is, a non-empty
        // string made up of letters, digits, and underscores, where the first character
        // is not a digit. Keywords are not identifiers.
        //
        public static bool IsIdentifier(@string name)
        {
            foreach (var (i, c) in name)
            {
                if (!unicode.IsLetter(c) && c != '_' && (i == 0L || !unicode.IsDigit(c)))
                {
                    return false;
                }

            }
            return name != "" && !IsKeyword(name);

        }
    }
}}
