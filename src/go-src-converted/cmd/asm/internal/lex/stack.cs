// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lex -- go2cs converted at 2020 August 29 08:51:46 UTC
// import "cmd/asm/internal/lex" ==> using lex = go.cmd.asm.@internal.lex_package
// Original source: C:\Go\src\cmd\asm\internal\lex\stack.go
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
        // A Stack is a stack of TokenReaders. As the top TokenReader hits EOF,
        // it resumes reading the next one down.
        public partial struct Stack
        {
            public slice<TokenReader> tr;
        }

        // Push adds tr to the top (end) of the input stack. (Popping happens automatically.)
        private static void Push(this ref Stack s, TokenReader tr)
        {
            s.tr = append(s.tr, tr);
        }

        private static ScanToken Next(this ref Stack s)
        {
            var tos = s.tr[len(s.tr) - 1L];
            var tok = tos.Next();
            while (tok == scanner.EOF && len(s.tr) > 1L)
            {
                tos.Close(); 
                // Pop the topmost item from the stack and resume with the next one down.
                s.tr = s.tr[..len(s.tr) - 1L];
                tok = s.Next();
            }

            return tok;
        }

        private static @string Text(this ref Stack s)
        {
            return s.tr[len(s.tr) - 1L].Text();
        }

        private static @string File(this ref Stack s)
        {
            return s.Base().Filename();
        }

        private static ref src.PosBase Base(this ref Stack s)
        {
            return s.tr[len(s.tr) - 1L].Base();
        }

        private static void SetBase(this ref Stack s, ref src.PosBase @base)
        {
            s.tr[len(s.tr) - 1L].SetBase(base);
        }

        private static long Line(this ref Stack s)
        {
            return s.tr[len(s.tr) - 1L].Line();
        }

        private static long Col(this ref Stack s)
        {
            return s.tr[len(s.tr) - 1L].Col();
        }

        private static void Close(this ref Stack s)
        { // Unused.
        }
    }
}}}}
