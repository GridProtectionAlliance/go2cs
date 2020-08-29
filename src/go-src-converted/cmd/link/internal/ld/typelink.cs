// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:40 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\typelink.go
using objabi = go.cmd.@internal.objabi_package;
using sym = go.cmd.link.@internal.sym_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private partial struct byTypeStr // : slice<typelinkSortKey>
        {
        }

        private partial struct typelinkSortKey
        {
            public @string TypeStr;
            public ptr<sym.Symbol> Type;
        }

        private static bool Less(this byTypeStr s, long i, long j)
        {
            return s[i].TypeStr < s[j].TypeStr;
        }
        private static long Len(this byTypeStr s)
        {
            return len(s);
        }
        private static void Swap(this byTypeStr s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        // typelink generates the typelink table which is used by reflect.typelinks().
        // Types that should be added to the typelinks table are marked with the
        // MakeTypelink attribute by the compiler.
        private static void typelink(this ref Link ctxt)
        {
            byTypeStr typelinks = new byTypeStr();
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (s.Attr.Reachable() && s.Attr.MakeTypelink())
                    {
                        typelinks = append(typelinks, new typelinkSortKey(decodetypeStr(ctxt.Arch,s),s));
                    }
                }

                s = s__prev1;
            }

            sort.Sort(typelinks);

            var tl = ctxt.Syms.Lookup("runtime.typelink", 0L);
            tl.Type = sym.STYPELINK;
            tl.Attr |= sym.AttrReachable | sym.AttrLocal;
            tl.Size = int64(4L * len(typelinks));
            tl.P = make_slice<byte>(tl.Size);
            tl.R = make_slice<sym.Reloc>(len(typelinks));
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in typelinks)
                {
                    i = __i;
                    s = __s;
                    var r = ref tl.R[i];
                    r.Sym = s.Type;
                    r.Off = int32(i * 4L);
                    r.Siz = 4L;
                    r.Type = objabi.R_ADDROFF;
                }

                s = s__prev1;
            }

        }
    }
}}}}
