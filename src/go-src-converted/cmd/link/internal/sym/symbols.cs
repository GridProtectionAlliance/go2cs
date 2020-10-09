// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package sym -- go2cs converted at 2020 October 09 05:48:55 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\symbols.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class sym_package
    {
        public partial struct Symbols
        {
            public long versions; // Provided by the loader

// Look up the symbol with the given name and version, creating the
// symbol if it is not found.
            public Func<@string, long, ptr<Symbol>> Lookup; // Look up the symbol with the given name and version, returning nil
// if it is not found.
            public Func<@string, long, ptr<Symbol>> ROLookup;
        }

        public static ptr<Symbols> NewSymbols()
        {
            return addr(new Symbols(versions:SymVerStatic,));
        }

        // Allocate a new version (i.e. symbol namespace).
        private static long IncVersion(this ptr<Symbols> _addr_syms)
        {
            ref Symbols syms = ref _addr_syms.val;

            syms.versions++;
            return syms.versions - 1L;
        }

        // returns the maximum version number
        private static long MaxVersion(this ptr<Symbols> _addr_syms)
        {
            ref Symbols syms = ref _addr_syms.val;

            return syms.versions;
        }
    }
}}}}
