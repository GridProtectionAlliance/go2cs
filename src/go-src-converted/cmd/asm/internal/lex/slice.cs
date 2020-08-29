// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lex -- go2cs converted at 2020 August 29 08:51:45 UTC
// import "cmd/asm/internal/lex" ==> using lex = go.cmd.asm.@internal.lex_package
// Original source: C:\Go\src\cmd\asm\internal\lex\slice.go
using scanner = go.text.scanner_package;

using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class lex_package
    {
        // A Slice reads from a slice of Tokens.
        public partial struct Slice
        {
            public slice<Token> tokens;
            public ptr<src.PosBase> @base;
            public long line;
            public long pos;
        }

        public static ref Slice NewSlice(ref src.PosBase @base, long line, slice<Token> tokens)
        {
            return ref new Slice(tokens:tokens,base:base,line:line,pos:-1,);
        }

        private static ScanToken Next(this ref Slice s)
        {
            s.pos++;
            if (s.pos >= len(s.tokens))
            {
                return scanner.EOF;
            }
            return s.tokens[s.pos].ScanToken;
        }

        private static @string Text(this ref Slice s)
        {
            return s.tokens[s.pos].text;
        }

        private static @string File(this ref Slice s)
        {
            return s.@base.Filename();
        }

        private static ref src.PosBase Base(this ref Slice s)
        {
            return s.@base;
        }

        private static void SetBase(this ref Slice s, ref src.PosBase @base)
        { 
            // Cannot happen because we only have slices of already-scanned text,
            // but be prepared.
            s.@base = base;
        }

        private static long Line(this ref Slice s)
        {
            return s.line;
        }

        private static long Col(this ref Slice s)
        { 
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

        private static void Close(this ref Slice s)
        {
        }
    }
}}}}
