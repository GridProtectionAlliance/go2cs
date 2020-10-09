// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 09 05:51:02 UTC
// import "cmd/oldlink/internal/riscv64" ==> using riscv64 = go.cmd.oldlink.@internal.riscv64_package
// Original source: C:\Go\src\cmd\oldlink\internal\riscv64\asm.go
using riscv = go.cmd.@internal.obj.riscv_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class riscv64_package
    {
        private static void gentext(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

        }

        private static void adddynrela(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_rel, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol rel = ref _addr_rel.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            log.Fatalf("adddynrela not implemented");
        }

        private static bool adddynrel(ptr<ld.Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            log.Fatalf("adddynrel not implemented");
            return false;
        }

        private static bool elfreloc1(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;

            log.Fatalf("elfreloc1");
            return false;
        }

        private static void elfsetupplt(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            log.Fatalf("elfsetuplt");
        }

        private static bool machoreloc1(ptr<sys.Arch> _addr_arch, ptr<ld.OutBuf> _addr_@out, ptr<sym.Symbol> _addr_s, ptr<sym.Reloc> _addr_r, long sectoff)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref ld.OutBuf @out = ref _addr_@out.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Reloc r = ref _addr_r.val;

            log.Fatalf("machoreloc1 not implemented");
            return false;
        }

        private static (long, bool) archreloc(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long val) => func((_, panic, __) =>
        {
            long _p0 = default;
            bool _p0 = default;
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;


            if (r.Type == objabi.R_CALLRISCV) 
                // Nothing to do.
                return (val, true);
            else if (r.Type == objabi.R_RISCV_PCREL_ITYPE || r.Type == objabi.R_RISCV_PCREL_STYPE) 
                var pc = s.Value + int64(r.Off);
                var off = ld.Symaddr(r.Sym) + r.Add - pc; 

                // Generate AUIPC and second instruction immediates.
                var (low, high, err) = riscv.Split32BitImmediate(off);
                if (err != null)
                {
                    ld.Errorf(s, "R_RISCV_PCREL_ relocation does not fit in 32-bits: %d", off);
                }

                var (auipcImm, err) = riscv.EncodeUImmediate(high);
                if (err != null)
                {
                    ld.Errorf(s, "cannot encode R_RISCV_PCREL_ AUIPC relocation offset for %s: %v", r.Sym.Name, err);
                }

                long secondImm = default;                long secondImmMask = default;


                if (r.Type == objabi.R_RISCV_PCREL_ITYPE) 
                    secondImmMask = riscv.ITypeImmMask;
                    secondImm, err = riscv.EncodeIImmediate(low);
                    if (err != null)
                    {
                        ld.Errorf(s, "cannot encode R_RISCV_PCREL_ITYPE I-type instruction relocation offset for %s: %v", r.Sym.Name, err);
                    }

                else if (r.Type == objabi.R_RISCV_PCREL_STYPE) 
                    secondImmMask = riscv.STypeImmMask;
                    secondImm, err = riscv.EncodeSImmediate(low);
                    if (err != null)
                    {
                        ld.Errorf(s, "cannot encode R_RISCV_PCREL_STYPE S-type instruction relocation offset for %s: %v", r.Sym.Name, err);
                    }

                else 
                    panic(fmt.Sprintf("Unknown relocation type: %v", r.Type));
                                var auipc = int64(uint32(val));
                var second = int64(uint32(val >> (int)(32L)));

                auipc = (auipc & ~riscv.UTypeImmMask) | int64(uint32(auipcImm));
                second = (second & ~secondImmMask) | int64(uint32(secondImm));

                return (second << (int)(32L) | auipc, true);
                        return (val, false);

        });

        private static long archrelocvariant(ptr<ld.Link> _addr_ctxt, ptr<sym.Reloc> _addr_r, ptr<sym.Symbol> _addr_s, long t)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Reloc r = ref _addr_r.val;
            ref sym.Symbol s = ref _addr_s.val;

            log.Fatalf("archrelocvariant");
            return -1L;
        }

        private static void asmb(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            if (ctxt.IsELF)
            {
                ld.Asmbelfsetup();
            }

            var sect = ld.Segtext.Sections[0L];
            ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff));
            ld.Codeblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
            foreach (var (_, __sect) in ld.Segtext.Sections[1L..])
            {
                sect = __sect;
                ctxt.Out.SeekSet(int64(sect.Vaddr - ld.Segtext.Vaddr + ld.Segtext.Fileoff));
                ld.Datblk(ctxt, int64(sect.Vaddr), int64(sect.Length));
            }

            if (ld.Segrodata.Filelen > 0L)
            {
                ctxt.Out.SeekSet(int64(ld.Segrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrodata.Vaddr), int64(ld.Segrodata.Filelen));
            }

            if (ld.Segrelrodata.Filelen > 0L)
            {
                ctxt.Out.SeekSet(int64(ld.Segrelrodata.Fileoff));
                ld.Datblk(ctxt, int64(ld.Segrelrodata.Vaddr), int64(ld.Segrelrodata.Filelen));
            }

            ctxt.Out.SeekSet(int64(ld.Segdata.Fileoff));
            ld.Datblk(ctxt, int64(ld.Segdata.Vaddr), int64(ld.Segdata.Filelen));

            ctxt.Out.SeekSet(int64(ld.Segdwarf.Fileoff));
            ld.Dwarfblk(ctxt, int64(ld.Segdwarf.Vaddr), int64(ld.Segdwarf.Filelen));

        }

        private static void asmb2(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            ld.Symsize = 0L;
            ld.Lcsize = 0L;
            var symo = uint32(0L);

            if (!ld.FlagS.val)
            {
                if (!ctxt.IsELF)
                {
                    ld.Errorf(null, "unsupported executable format");
                }

                symo = uint32(ld.Segdwarf.Fileoff + ld.Segdwarf.Filelen);
                symo = uint32(ld.Rnd(int64(symo), int64(ld.FlagRound.val)));
                ctxt.Out.SeekSet(int64(symo));

                ld.Asmelfsym(ctxt);
                ctxt.Out.Flush();
                ctxt.Out.Write(ld.Elfstrdat);

                if (ctxt.LinkMode == ld.LinkExternal)
                {
                    ld.Elfemitreloc(ctxt);
                }

            }

            ctxt.Out.SeekSet(0L);

            if (ctxt.HeadType == objabi.Hlinux) 
                ld.Asmbelf(ctxt, int64(symo));
            else 
                ld.Errorf(null, "unsupported operating system");
                        ctxt.Out.Flush();

            if (ld.FlagC.val)
            {
                fmt.Printf("textsize=%d\n", ld.Segtext.Filelen);
                fmt.Printf("datsize=%d\n", ld.Segdata.Filelen);
                fmt.Printf("bsssize=%d\n", ld.Segdata.Length - ld.Segdata.Filelen);
                fmt.Printf("symsize=%d\n", ld.Symsize);
                fmt.Printf("lcsize=%d\n", ld.Lcsize);
                fmt.Printf("total=%d\n", ld.Segtext.Filelen + ld.Segdata.Length + uint64(ld.Symsize) + uint64(ld.Lcsize));
            }

        }
    }
}}}}
