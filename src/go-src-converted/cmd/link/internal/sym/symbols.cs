// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/l.h
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

// package sym -- go2cs converted at 2020 August 29 10:02:56 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\symbols.go

using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class sym_package
    {
        public partial struct Symbols
        {
            public slice<Symbol> symbolBatch; // Symbol lookup based on name and indexed by version.
            public slice<map<@string, ref Symbol>> hash;
            public slice<ref Symbol> Allsym;
        }

        public static ref Symbols NewSymbols()
        {
            return ref new Symbols(hash:[]map[string]*Symbol{make(map[string]*Symbol,100000),},Allsym:make([]*Symbol,0,100000),);
        }

        private static ref Symbol Newsym(this ref Symbols syms, @string name, long v)
        {
            var batch = syms.symbolBatch;
            if (len(batch) == 0L)
            {
                batch = make_slice<Symbol>(1000L);
            }
            var s = ref batch[0L];
            syms.symbolBatch = batch[1L..];

            s.Dynid = -1L;
            s.Plt = -1L;
            s.Got = -1L;
            s.Name = name;
            s.Version = int16(v);
            syms.Allsym = append(syms.Allsym, s);

            return s;
        }

        // Look up the symbol with the given name and version, creating the
        // symbol if it is not found.
        private static ref Symbol Lookup(this ref Symbols syms, @string name, long v)
        {
            var m = syms.hash[v];
            var s = m[name];
            if (s != null)
            {
                return s;
            }
            s = syms.Newsym(name, v);
            s.Extname = s.Name;
            m[name] = s;
            return s;
        }

        // Look up the symbol with the given name and version, returning nil
        // if it is not found.
        private static ref Symbol ROLookup(this ref Symbols syms, @string name, long v)
        {
            return syms.hash[v][name];
        }

        // Allocate a new version (i.e. symbol namespace).
        private static long IncVersion(this ref Symbols syms)
        {
            syms.hash = append(syms.hash, make_map<@string, ref Symbol>());
            return len(syms.hash) - 1L;
        }

        // Rename renames a symbol.
        private static void Rename(this ref Symbols syms, @string old, @string @new, long v)
        {
            var s = syms.hash[v][old];
            s.Name = new;
            if (s.Extname == old)
            {
                s.Extname = new;
            }
            delete(syms.hash[v], old);

            var dup = syms.hash[v][new];
            if (dup == null)
            {
                syms.hash[v][new] = s;
            }
            else
            {
                if (s.Type == 0L)
                {
                    s.Value = dup.Value;
                }
                else if (dup.Type == 0L)
                {
                    dup.Value = s.Value;
                    syms.hash[v][new] = s;
                }
            }
        }
    }
}}}}
