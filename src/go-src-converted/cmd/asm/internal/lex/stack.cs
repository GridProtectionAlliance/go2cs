// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lex -- go2cs converted at 2020 October 08 04:08:10 UTC
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
        private static void Push(this ptr<Stack> _addr_s, TokenReader tr)
        {
            ref Stack s = ref _addr_s.val;

            s.tr = append(s.tr, tr);
        }

        private static ScanToken Next(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

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

        private static @string Text(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

            return s.tr[len(s.tr) - 1L].Text();
        }

        private static @string File(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

            return s.Base().Filename();
        }

        private static ptr<src.PosBase> Base(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

            return _addr_s.tr[len(s.tr) - 1L].Base()!;
        }

        private static void SetBase(this ptr<Stack> _addr_s, ptr<src.PosBase> _addr_@base)
        {
            ref Stack s = ref _addr_s.val;
            ref src.PosBase @base = ref _addr_@base.val;

            s.tr[len(s.tr) - 1L].SetBase(base);
        }

        private static long Line(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

            return s.tr[len(s.tr) - 1L].Line();
        }

        private static long Col(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;

            return s.tr[len(s.tr) - 1L].Col();
        }

        private static void Close(this ptr<Stack> _addr_s)
        {
            ref Stack s = ref _addr_s.val;
 // Unused.
        }
    }
}}}}
