// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 09:26:28 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\tokens.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        private partial struct token // : ulong
        {
        }

        private static readonly token _ = iota;
        private static readonly var _EOF = 0; 

        // names and literals
        private static readonly var _Name = 1;
        private static readonly var _Literal = 2; 

        // operators and operations
        private static readonly var _Operator = 3; // excluding '*' (_Star)
        private static readonly var _AssignOp = 4;
        private static readonly var _IncOp = 5;
        private static readonly var _Assign = 6;
        private static readonly var _Define = 7;
        private static readonly var _Arrow = 8;
        private static readonly var _Star = 9; 

        // delimiters
        private static readonly var _Lparen = 10;
        private static readonly var _Lbrack = 11;
        private static readonly var _Lbrace = 12;
        private static readonly var _Rparen = 13;
        private static readonly var _Rbrack = 14;
        private static readonly var _Rbrace = 15;
        private static readonly var _Comma = 16;
        private static readonly var _Semi = 17;
        private static readonly var _Colon = 18;
        private static readonly var _Dot = 19;
        private static readonly var _DotDotDot = 20; 

        // keywords
        private static readonly var _Break = 21;
        private static readonly var _Case = 22;
        private static readonly var _Chan = 23;
        private static readonly var _Const = 24;
        private static readonly var _Continue = 25;
        private static readonly var _Default = 26;
        private static readonly var _Defer = 27;
        private static readonly var _Else = 28;
        private static readonly var _Fallthrough = 29;
        private static readonly var _For = 30;
        private static readonly var _Func = 31;
        private static readonly var _Go = 32;
        private static readonly var _Goto = 33;
        private static readonly var _If = 34;
        private static readonly var _Import = 35;
        private static readonly var _Interface = 36;
        private static readonly var _Map = 37;
        private static readonly var _Package = 38;
        private static readonly var _Range = 39;
        private static readonly var _Return = 40;
        private static readonly var _Select = 41;
        private static readonly var _Struct = 42;
        private static readonly var _Switch = 43;
        private static readonly var _Type = 44;
        private static readonly var _Var = 45;

        private static readonly var tokenCount = 46;

 
        // for BranchStmt
        public static readonly var Break = _Break;
        public static readonly var Continue = _Continue;
        public static readonly var Fallthrough = _Fallthrough;
        public static readonly var Goto = _Goto; 

        // for CallStmt
        public static readonly var Go = _Go;
        public static readonly var Defer = _Defer;

        private static array<@string> tokstrings = new array<@string>(InitKeyedValues<@string>((_EOF, "EOF"), (_Name, "name"), (_Literal, "literal"), (_Operator, "op"), (_AssignOp, "op="), (_IncOp, "opop"), (_Assign, "="), (_Define, ":="), (_Arrow, "<-"), (_Star, "*"), (_Lparen, "("), (_Lbrack, "["), (_Lbrace, "{"), (_Rparen, ")"), (_Rbrack, "]"), (_Rbrace, "}"), (_Comma, ","), (_Semi, ";"), (_Colon, ":"), (_Dot, "."), (_DotDotDot, "..."), (_Break, "break"), (_Case, "case"), (_Chan, "chan"), (_Const, "const"), (_Continue, "continue"), (_Default, "default"), (_Defer, "defer"), (_Else, "else"), (_Fallthrough, "fallthrough"), (_For, "for"), (_Func, "func"), (_Go, "go"), (_Goto, "goto"), (_If, "if"), (_Import, "import"), (_Interface, "interface"), (_Map, "map"), (_Package, "package"), (_Range, "range"), (_Return, "return"), (_Select, "select"), (_Struct, "struct"), (_Switch, "switch"), (_Type, "type"), (_Var, "var")));

        private static @string String(this token tok)
        {
            @string s = default;
            if (0L <= tok && int(tok) < len(tokstrings))
            {
                s = tokstrings[tok];
            }
            if (s == "")
            {
                s = fmt.Sprintf("<tok-%d>", tok);
            }
            return s;
        }

        // Make sure we have at most 64 tokens so we can use them in a set.
        private static readonly ulong _ = 1L << (int)((tokenCount - 1L));

        // contains reports whether tok is in tokset.


        // contains reports whether tok is in tokset.
        private static bool contains(ulong tokset, token tok)
        {
            return tokset & (1L << (int)(tok)) != 0L;
        }

        public partial struct LitKind // : ulong
        {
        }

        public static readonly LitKind IntLit = iota;
        public static readonly var FloatLit = 0;
        public static readonly var ImagLit = 1;
        public static readonly var RuneLit = 2;
        public static readonly var StringLit = 3;

        public partial struct Operator // : ulong
        {
        }

        private static readonly Operator _ = iota;
        public static readonly var Def = 0; // :=
        public static readonly var Not = 1; // !
        public static readonly var Recv = 2; // <-

        // precOrOr
        public static readonly var OrOr = 3; // ||

        // precAndAnd
        public static readonly var AndAnd = 4; // &&

        // precCmp
        public static readonly var Eql = 5; // ==
        public static readonly var Neq = 6; // !=
        public static readonly var Lss = 7; // <
        public static readonly var Leq = 8; // <=
        public static readonly var Gtr = 9; // >
        public static readonly var Geq = 10; // >=

        // precAdd
        public static readonly var Add = 11; // +
        public static readonly var Sub = 12; // -
        public static readonly var Or = 13; // |
        public static readonly var Xor = 14; // ^

        // precMul
        public static readonly var Mul = 15; // *
        public static readonly var Div = 16; // /
        public static readonly var Rem = 17; // %
        public static readonly var And = 18; // &
        public static readonly var AndNot = 19; // &^
        public static readonly var Shl = 20; // <<
        public static readonly var Shr = 21; // >>

        private static array<@string> opstrings = new array<@string>(InitKeyedValues<@string>((Def, ":"), (Not, "!"), (Recv, "<-"), (OrOr, "||"), (AndAnd, "&&"), (Eql, "=="), (Neq, "!="), (Lss, "<"), (Leq, "<="), (Gtr, ">"), (Geq, ">="), (Add, "+"), (Sub, "-"), (Or, "|"), (Xor, "^"), (Mul, "*"), (Div, "/"), (Rem, "%"), (And, "&"), (AndNot, "&^"), (Shl, "<<"), (Shr, ">>")));

        public static @string String(this Operator op)
        {
            @string s = default;
            if (0L <= op && int(op) < len(opstrings))
            {
                s = opstrings[op];
            }
            if (s == "")
            {
                s = fmt.Sprintf("<op-%d>", op);
            }
            return s;
        }

        // Operator precedences
        private static readonly var _ = iota;
        private static readonly var precOrOr = 0;
        private static readonly var precAndAnd = 1;
        private static readonly var precCmp = 2;
        private static readonly var precAdd = 3;
        private static readonly var precMul = 4;
    }
}}}}
