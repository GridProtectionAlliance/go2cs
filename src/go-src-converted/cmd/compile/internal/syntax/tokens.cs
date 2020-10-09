// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 09 05:41:07 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\tokens.go

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

        //go:generate stringer -type token -linecomment

        private static readonly token _ = (token)iota;
        private static readonly var _EOF = 0; // EOF

        // names and literals
        private static readonly var _Name = 1; // name
        private static readonly var _Literal = 2; // literal

        // operators and operations
        // _Operator is excluding '*' (_Star)
        private static readonly var _Operator = 3; // op
        private static readonly var _AssignOp = 4; // op=
        private static readonly var _IncOp = 5; // opop
        private static readonly var _Assign = 6; // =
        private static readonly var _Define = 7; // :=
        private static readonly var _Arrow = 8; // <-
        private static readonly var _Star = 9; // *

        // delimiters
        private static readonly var _Lparen = 10; // (
        private static readonly var _Lbrack = 11; // [
        private static readonly var _Lbrace = 12; // {
        private static readonly var _Rparen = 13; // )
        private static readonly var _Rbrack = 14; // ]
        private static readonly var _Rbrace = 15; // }
        private static readonly var _Comma = 16; // ,
        private static readonly var _Semi = 17; // ;
        private static readonly var _Colon = 18; // :
        private static readonly var _Dot = 19; // .
        private static readonly var _DotDotDot = 20; // ...

        // keywords
        private static readonly var _Break = 21; // break
        private static readonly var _Case = 22; // case
        private static readonly var _Chan = 23; // chan
        private static readonly var _Const = 24; // const
        private static readonly var _Continue = 25; // continue
        private static readonly var _Default = 26; // default
        private static readonly var _Defer = 27; // defer
        private static readonly var _Else = 28; // else
        private static readonly var _Fallthrough = 29; // fallthrough
        private static readonly var _For = 30; // for
        private static readonly var _Func = 31; // func
        private static readonly var _Go = 32; // go
        private static readonly var _Goto = 33; // goto
        private static readonly var _If = 34; // if
        private static readonly var _Import = 35; // import
        private static readonly var _Interface = 36; // interface
        private static readonly var _Map = 37; // map
        private static readonly var _Package = 38; // package
        private static readonly var _Range = 39; // range
        private static readonly var _Return = 40; // return
        private static readonly var _Select = 41; // select
        private static readonly var _Struct = 42; // struct
        private static readonly var _Switch = 43; // switch
        private static readonly var _Type = 44; // type
        private static readonly var _Var = 45; // var

        // empty line comment to exclude it from .String
        private static readonly var tokenCount = 46; //

 
        // for BranchStmt
        public static readonly var Break = _Break;
        public static readonly var Continue = _Continue;
        public static readonly var Fallthrough = _Fallthrough;
        public static readonly var Goto = _Goto; 

        // for CallStmt
        public static readonly var Go = _Go;
        public static readonly var Defer = _Defer;


        // Make sure we have at most 64 tokens so we can use them in a set.
        private static readonly ulong _ = (ulong)1L << (int)((tokenCount - 1L));

        // contains reports whether tok is in tokset.


        // contains reports whether tok is in tokset.
        private static bool contains(ulong tokset, token tok)
        {
            return tokset & (1L << (int)(tok)) != 0L;
        }

        public partial struct LitKind // : byte
        {
        }

        // TODO(gri) With the 'i' (imaginary) suffix now permitted on integer
        //           and floating-point numbers, having a single ImagLit does
        //           not represent the literal kind well anymore. Remove it?
        public static readonly LitKind IntLit = (LitKind)iota;
        public static readonly var FloatLit = 0;
        public static readonly var ImagLit = 1;
        public static readonly var RuneLit = 2;
        public static readonly var StringLit = 3;


        public partial struct Operator // : ulong
        {
        }

        //go:generate stringer -type Operator -linecomment

        private static readonly Operator _ = (Operator)iota; 

        // Def is the : in :=
        public static readonly var Def = 0; // :
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

        // Operator precedences
        private static readonly var _ = iota;
        private static readonly var precOrOr = 0;
        private static readonly var precAndAnd = 1;
        private static readonly var precCmp = 2;
        private static readonly var precAdd = 3;
        private static readonly var precMul = 4;

    }
}}}}
