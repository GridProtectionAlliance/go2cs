// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 08 04:28:31 UTC
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
        private static readonly var _EOF = (var)0; // EOF

        // names and literals
        private static readonly var _Name = (var)1; // name
        private static readonly var _Literal = (var)2; // literal

        // operators and operations
        // _Operator is excluding '*' (_Star)
        private static readonly var _Operator = (var)3; // op
        private static readonly var _AssignOp = (var)4; // op=
        private static readonly var _IncOp = (var)5; // opop
        private static readonly var _Assign = (var)6; // =
        private static readonly var _Define = (var)7; // :=
        private static readonly var _Arrow = (var)8; // <-
        private static readonly var _Star = (var)9; // *

        // delimiters
        private static readonly var _Lparen = (var)10; // (
        private static readonly var _Lbrack = (var)11; // [
        private static readonly var _Lbrace = (var)12; // {
        private static readonly var _Rparen = (var)13; // )
        private static readonly var _Rbrack = (var)14; // ]
        private static readonly var _Rbrace = (var)15; // }
        private static readonly var _Comma = (var)16; // ,
        private static readonly var _Semi = (var)17; // ;
        private static readonly var _Colon = (var)18; // :
        private static readonly var _Dot = (var)19; // .
        private static readonly var _DotDotDot = (var)20; // ...

        // keywords
        private static readonly var _Break = (var)21; // break
        private static readonly var _Case = (var)22; // case
        private static readonly var _Chan = (var)23; // chan
        private static readonly var _Const = (var)24; // const
        private static readonly var _Continue = (var)25; // continue
        private static readonly var _Default = (var)26; // default
        private static readonly var _Defer = (var)27; // defer
        private static readonly var _Else = (var)28; // else
        private static readonly var _Fallthrough = (var)29; // fallthrough
        private static readonly var _For = (var)30; // for
        private static readonly var _Func = (var)31; // func
        private static readonly var _Go = (var)32; // go
        private static readonly var _Goto = (var)33; // goto
        private static readonly var _If = (var)34; // if
        private static readonly var _Import = (var)35; // import
        private static readonly var _Interface = (var)36; // interface
        private static readonly var _Map = (var)37; // map
        private static readonly var _Package = (var)38; // package
        private static readonly var _Range = (var)39; // range
        private static readonly var _Return = (var)40; // return
        private static readonly var _Select = (var)41; // select
        private static readonly var _Struct = (var)42; // struct
        private static readonly var _Switch = (var)43; // switch
        private static readonly var _Type = (var)44; // type
        private static readonly var _Var = (var)45; // var

        // empty line comment to exclude it from .String
        private static readonly var tokenCount = (var)46; //

 
        // for BranchStmt
        public static readonly var Break = (var)_Break;
        public static readonly var Continue = (var)_Continue;
        public static readonly var Fallthrough = (var)_Fallthrough;
        public static readonly var Goto = (var)_Goto; 

        // for CallStmt
        public static readonly var Go = (var)_Go;
        public static readonly var Defer = (var)_Defer;


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
        public static readonly var FloatLit = (var)0;
        public static readonly var ImagLit = (var)1;
        public static readonly var RuneLit = (var)2;
        public static readonly var StringLit = (var)3;


        public partial struct Operator // : ulong
        {
        }

        //go:generate stringer -type Operator -linecomment

        private static readonly Operator _ = (Operator)iota; 

        // Def is the : in :=
        public static readonly var Def = (var)0; // :
        public static readonly var Not = (var)1; // !
        public static readonly var Recv = (var)2; // <-

        // precOrOr
        public static readonly var OrOr = (var)3; // ||

        // precAndAnd
        public static readonly var AndAnd = (var)4; // &&

        // precCmp
        public static readonly var Eql = (var)5; // ==
        public static readonly var Neq = (var)6; // !=
        public static readonly var Lss = (var)7; // <
        public static readonly var Leq = (var)8; // <=
        public static readonly var Gtr = (var)9; // >
        public static readonly var Geq = (var)10; // >=

        // precAdd
        public static readonly var Add = (var)11; // +
        public static readonly var Sub = (var)12; // -
        public static readonly var Or = (var)13; // |
        public static readonly var Xor = (var)14; // ^

        // precMul
        public static readonly var Mul = (var)15; // *
        public static readonly var Div = (var)16; // /
        public static readonly var Rem = (var)17; // %
        public static readonly var And = (var)18; // &
        public static readonly var AndNot = (var)19; // &^
        public static readonly var Shl = (var)20; // <<
        public static readonly var Shr = (var)21; // >>

        // Operator precedences
        private static readonly var _ = (var)iota;
        private static readonly var precOrOr = (var)0;
        private static readonly var precAndAnd = (var)1;
        private static readonly var precCmp = (var)2;
        private static readonly var precAdd = (var)3;
        private static readonly var precMul = (var)4;

    }
}}}}
