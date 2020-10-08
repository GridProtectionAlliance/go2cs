// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:38:42 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\elf2.go
using sym = go.cmd.link.@internal.sym_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // Temporary dumping around for sym.Symbol version of helper
        // functions in elf.go, still being used for some archs/oses.
        // FIXME: get rid of this file when dodata() is completely
        // converted and the sym.Symbol functions are not needed.
        private static void elfsetstring(ptr<sym.Symbol> _addr_s, @string str, long off)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (nelfstr >= len(elfstr))
            {
                Errorf(s, "too many elf strings");
                errorexit();
            }
            elfstr[nelfstr].s = str;
            elfstr[nelfstr].off = off;
            nelfstr++;

        }

        private static void elfrelocsect2(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;
 
            // If main section is SHT_NOBITS, nothing to relocate.
            // Also nothing to relocate in .shstrtab.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return ;
            }

            if (sect.Name == ".shstrtab")
            {
                return ;
            }

            sect.Reloff = uint64(ctxt.Out.Offset());
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }

                    if (uint64(s.Value) >= sect.Vaddr)
                    {
                        syms = syms[i..];
                        break;
                    }

                }

                s = s__prev1;
            }

            var eaddr = int32(sect.Vaddr + sect.Length);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }

                    if (s.Value >= int64(eaddr))
                    {
                        break;
                    }

                    foreach (var (ri) in s.R)
                    {
                        var r = _addr_s.R[ri];
                        if (r.Done)
                        {
                            continue;
                        }

                        if (r.Xsym == null)
                        {
                            Errorf(s, "missing xsym in relocation %#v %#v", r.Sym.Name, s);
                            continue;
                        }

                        var esr = ElfSymForReloc(ctxt, r.Xsym);
                        if (esr == 0L)
                        {
                            Errorf(s, "reloc %d (%s) to non-elf symbol %s (outer=%s) %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Sym.Name, r.Xsym.Name, r.Sym.Type, r.Sym.Type);
                        }

                        if (!r.Xsym.Attr.Reachable())
                        {
                            Errorf(s, "unreachable reloc %d (%s) target %v", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Xsym.Name);
                        }

                        if (!thearch.Elfreloc1(ctxt, r, int64(uint64(s.Value + int64(r.Off)) - sect.Vaddr)))
                        {
                            Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Siz, r.Sym.Name);
                        }

                    }

                }

                s = s__prev1;
            }

            sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;

        }
    }
}}}}
