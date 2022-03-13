// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 13 06:33:34 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\asmb.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using fmt = fmt_package;
using runtime = runtime_package;
using sync = sync_package;


// Assembling the binary is broken into two steps:
//  - writing out the code/data/dwarf Segments, applying relocations on the fly
//  - writing out the architecture specific pieces.
// This function handles the first part.

using System;
using System.Threading;
public static partial class ld_package {

private static void asmb(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // TODO(jfaller): delete me.
    if (thearch.Asmb != null) {
        thearch.Asmb(ctxt, ctxt.loader);
        return ;
    }
    if (ctxt.IsELF) {
        Asmbelfsetup();
    }
    ref sync.WaitGroup wg = ref heap(out ptr<sync.WaitGroup> _addr_wg);
    Action<ptr<Link>, ptr<OutBuf>, long, long> f = (ctxt, @out, start, length) => {
        var pad = thearch.CodePad;
        if (pad == null) {
            pad = zeros[..];
        }
        CodeblkPad(ctxt, out, start, length, pad);
    };

    foreach (var (_, sect) in Segtext.Sections) {
        var offset = sect.Vaddr - Segtext.Vaddr + Segtext.Fileoff; 
        // Handle text sections with Codeblk
        if (sect.Name == ".text") {
            writeParallel(_addr_wg, f, ctxt, offset, sect.Vaddr, sect.Length);
        }
        else
 {
            writeParallel(_addr_wg, datblk, ctxt, offset, sect.Vaddr, sect.Length);
        }
    }    if (Segrodata.Filelen > 0) {
        writeParallel(_addr_wg, datblk, ctxt, Segrodata.Fileoff, Segrodata.Vaddr, Segrodata.Filelen);
    }
    if (Segrelrodata.Filelen > 0) {
        writeParallel(_addr_wg, datblk, ctxt, Segrelrodata.Fileoff, Segrelrodata.Vaddr, Segrelrodata.Filelen);
    }
    writeParallel(_addr_wg, datblk, ctxt, Segdata.Fileoff, Segdata.Vaddr, Segdata.Filelen);

    writeParallel(_addr_wg, dwarfblk, ctxt, Segdwarf.Fileoff, Segdwarf.Vaddr, Segdwarf.Filelen);

    wg.Wait();
}

// Assembling the binary is broken into two steps:
//  - writing out the code/data/dwarf Segments
//  - writing out the architecture specific pieces.
// This function handles the second part.
private static void asmb2(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (thearch.Asmb2 != null) {
        thearch.Asmb2(ctxt, ctxt.loader);
        return ;
    }
    symSize = 0;
    spSize = 0;
    lcSize = 0;


    if (ctxt.HeadType == objabi.Hdarwin) 
        asmbMacho(ctxt); 

        // Plan9
    else if (ctxt.HeadType == objabi.Hplan9) 
        asmbPlan9(_addr_ctxt); 

        // PE
    else if (ctxt.HeadType == objabi.Hwindows) 
        asmbPe(ctxt); 

        // Xcoff
    else if (ctxt.HeadType == objabi.Haix) 
        asmbXcoff(ctxt); 

        // Elf
    else if (ctxt.HeadType == objabi.Hdragonfly || ctxt.HeadType == objabi.Hfreebsd || ctxt.HeadType == objabi.Hlinux || ctxt.HeadType == objabi.Hnetbsd || ctxt.HeadType == objabi.Hopenbsd || ctxt.HeadType == objabi.Hsolaris) 
        asmbElf(ctxt);
    else 
        panic("unknown platform"); 

        // Macho
        if (FlagC.val) {
        fmt.Printf("textsize=%d\n", Segtext.Filelen);
        fmt.Printf("datsize=%d\n", Segdata.Filelen);
        fmt.Printf("bsssize=%d\n", Segdata.Length - Segdata.Filelen);
        fmt.Printf("symsize=%d\n", symSize);
        fmt.Printf("lcsize=%d\n", lcSize);
        fmt.Printf("total=%d\n", Segtext.Filelen + Segdata.Length + uint64(symSize) + uint64(lcSize));
    }
});

// writePlan9Header writes out the plan9 header at the present position in the OutBuf.
private static void writePlan9Header(ptr<OutBuf> _addr_buf, uint magic, long entry, bool is64Bit) {
    ref OutBuf buf = ref _addr_buf.val;

    if (is64Bit) {
        magic |= 0x00008000;
    }
    buf.Write32b(magic);
    buf.Write32b(uint32(Segtext.Filelen));
    buf.Write32b(uint32(Segdata.Filelen));
    buf.Write32b(uint32(Segdata.Length - Segdata.Filelen));
    buf.Write32b(uint32(symSize));
    if (is64Bit) {
        buf.Write32b(uint32(entry & ~0x80000000));
    }
    else
 {
        buf.Write32b(uint32(entry));
    }
    buf.Write32b(uint32(spSize));
    buf.Write32b(uint32(lcSize)); 
    // amd64 includes the entry at the beginning of the symbol table.
    if (is64Bit) {
        buf.Write64b(uint64(entry));
    }
}

// asmbPlan9 assembles a plan 9 binary.
private static void asmbPlan9(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!FlagS.val) {
        FlagS.val = true;
        var symo = int64(Segdata.Fileoff + Segdata.Filelen);
        ctxt.Out.SeekSet(symo);
        asmbPlan9Sym(ctxt);
    }
    ctxt.Out.SeekSet(0);
    writePlan9Header(_addr_ctxt.Out, thearch.Plan9Magic, Entryvalue(ctxt), thearch.Plan9_64Bit);
}

// sizeExtRelocs precomputes the size needed for the reloc records,
// sets the size and offset for relocation records in each section,
// and mmap the output buffer with the proper size.
private static void sizeExtRelocs(ptr<Link> _addr_ctxt, uint relsize) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    if (relsize == 0) {
        panic("sizeExtRelocs: relocation size not set");
    }
    long sz = default;
    foreach (var (_, seg) in Segments) {
        foreach (var (_, sect) in seg.Sections) {
            sect.Reloff = uint64(ctxt.Out.Offset() + sz);
            sect.Rellen = uint64(relsize * sect.Relcount);
            sz += int64(sect.Rellen);
        }
    }    var filesz = ctxt.Out.Offset() + sz;
    var err = ctxt.Out.Mmap(uint64(filesz));
    if (err != null) {
        Exitf("mapping output file failed: %v", err);
    }
});

// relocSectFn wraps the function writing relocations of a section
// for parallel execution. Returns the wrapped function and a wait
// group for which the caller should wait.
private static (Action<ptr<Link>, ptr<sym.Section>, slice<loader.Sym>>, ptr<sync.WaitGroup>) relocSectFn(ptr<Link> _addr_ctxt, Action<ptr<Link>, ptr<OutBuf>, ptr<sym.Section>, slice<loader.Sym>> relocSect) => func((_, panic, _) => {
    Action<ptr<Link>, ptr<sym.Section>, slice<loader.Sym>> _p0 = default;
    ptr<sync.WaitGroup> _p0 = default!;
    ref Link ctxt = ref _addr_ctxt.val;

    Action<ptr<Link>, ptr<sym.Section>, slice<loader.Sym>> fn = default;
    ref sync.WaitGroup wg = ref heap(out ptr<sync.WaitGroup> _addr_wg);
    channel<nint> sem = default;
    if (ctxt.Out.isMmapped()) { 
        // Write sections in parallel.
        sem = make_channel<nint>(2 * runtime.GOMAXPROCS(0));
        fn = (ctxt, sect, syms) => {
            wg.Add(1);
            sem.Send(1);
            var (out, err) = ctxt.Out.View(sect.Reloff);
            if (err != null) {
                panic(err);
            }
            go_(() => () => {
                relocSect(ctxt, out, sect, syms);
                wg.Done().Send(sem);
            }
    else
());
        };
    } { 
        // We cannot Mmap. Write sequentially.
        fn = (ctxt, sect, syms) => {
            relocSect(ctxt, ctxt.Out, sect, syms);
        };
    }
    return (fn, _addr__addr_wg!);
});

} // end ld_package
