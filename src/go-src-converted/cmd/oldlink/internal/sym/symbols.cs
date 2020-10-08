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

// package sym -- go2cs converted at 2020 October 08 04:40:32 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\symbols.go

using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        public partial struct Symbols
        {
            public slice<Symbol> symbolBatch; // Symbol lookup based on name and indexed by version.
            public slice<map<@string, ptr<Symbol>>> hash;
            public slice<ptr<Symbol>> Allsym;
        }

        public static ptr<Symbols> NewSymbols()
        {
            var hash = make_slice<map<@string, ptr<Symbol>>>(SymVerStatic); 
            // Preallocate about 2mb for hash of non static symbols
            hash[0L] = make_map<@string, ptr<Symbol>>(100000L); 
            // And another 1mb for internal ABI text symbols.
            hash[SymVerABIInternal] = make_map<@string, ptr<Symbol>>(50000L);
            return addr(new Symbols(hash:hash,Allsym:make([]*Symbol,0,100000),));

        }

        private static ptr<Symbol> Newsym(this ptr<Symbols> _addr_syms, @string name, long v)
        {
            ref Symbols syms = ref _addr_syms.val;

            var batch = syms.symbolBatch;
            if (len(batch) == 0L)
            {
                batch = make_slice<Symbol>(1000L);
            }

            var s = _addr_batch[0L];
            syms.symbolBatch = batch[1L..];

            s.Dynid = -1L;
            s.Name = name;
            s.Version = int16(v);
            syms.Allsym = append(syms.Allsym, s);

            return _addr_s!;

        }

        // Look up the symbol with the given name and version, creating the
        // symbol if it is not found.
        private static ptr<Symbol> Lookup(this ptr<Symbols> _addr_syms, @string name, long v)
        {
            ref Symbols syms = ref _addr_syms.val;

            var m = syms.hash[v];
            var s = m[name];
            if (s != null)
            {
                return _addr_s!;
            }

            s = syms.Newsym(name, v);
            m[name] = s;
            return _addr_s!;

        }

        // Look up the symbol with the given name and version, returning nil
        // if it is not found.
        private static ptr<Symbol> ROLookup(this ptr<Symbols> _addr_syms, @string name, long v)
        {
            ref Symbols syms = ref _addr_syms.val;

            return _addr_syms.hash[v][name]!;
        }

        // Add an existing symbol to the symbol table.
        private static void Add(this ptr<Symbols> _addr_syms, ptr<Symbol> _addr_s) => func((_, panic, __) =>
        {
            ref Symbols syms = ref _addr_syms.val;
            ref Symbol s = ref _addr_s.val;

            var name = s.Name;
            var v = int(s.Version);
            var m = syms.hash[v];
            {
                var (_, ok) = m[name];

                if (ok)
                {
                    panic(name + " already added");
                }

            }

            m[name] = s;

        });

        // Allocate a new version (i.e. symbol namespace).
        private static long IncVersion(this ptr<Symbols> _addr_syms)
        {
            ref Symbols syms = ref _addr_syms.val;

            syms.hash = append(syms.hash, make_map<@string, ptr<Symbol>>());
            return len(syms.hash) - 1L;
        }

        // Rename renames a symbol.
        private static void Rename(this ptr<Symbols> _addr_syms, @string old, @string @new, long v, map<ptr<Symbol>, ptr<Symbol>> reachparent)
        {
            ref Symbols syms = ref _addr_syms.val;

            var s = syms.hash[v][old];
            var oldExtName = s.Extname();
            s.Name = new;
            if (oldExtName == old)
            {
                s.SetExtname(new);
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
                    dup.Attr |= s.Attr;
                    if (s.Attr.Reachable() && reachparent != null)
                    {
                        reachparent[dup] = reachparent[s];
                    }

                    s.val = dup.val;

                }
                else if (dup.Type == 0L)
                {
                    s.Attr |= dup.Attr;
                    if (dup.Attr.Reachable() && reachparent != null)
                    {
                        reachparent[s] = reachparent[dup];
                    }

                    dup.val = s.val;
                    syms.hash[v][new] = s;

                }

            }

        }
    }
}}}}
