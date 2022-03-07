// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:10 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\pcln.go
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // pclntab holds the state needed for pclntab generation.
private partial struct pclntab {
    public uint funcSize; // The first and last functions found.
    public loader.Sym firstFunc; // Running total size of pclntab.
    public loader.Sym lastFunc; // Running total size of pclntab.
    public long size; // runtime.pclntab's symbols
    public loader.Sym carrier;
    public loader.Sym pclntab;
    public loader.Sym pcheader;
    public loader.Sym funcnametab;
    public loader.Sym findfunctab;
    public loader.Sym cutab;
    public loader.Sym filetab;
    public loader.Sym pctab; // The number of functions + number of TEXT sections - 1. This is such an
// unexpected value because platforms that have more than one TEXT section
// get a dummy function inserted between because the external linker can place
// functions in those areas. We mark those areas as not covered by the Go
// runtime.
//
// On most platforms this is the number of reachable functions.
    public int nfunc; // The number of filenames in runtime.filetab.
    public uint nfiles;
}

// addGeneratedSym adds a generator symbol to pclntab, returning the new Sym.
// It is the caller's responsibility to save they symbol in state.
private static loader.Sym addGeneratedSym(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, @string name, long size, generatorFunc f) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    size = Rnd(size, int64(ctxt.Arch.PtrSize));
    state.size += size;
    var s = ctxt.createGeneratorSymbol(name, 0, sym.SPCLNTAB, size, f);
    ctxt.loader.SetAttrReachable(s, true);
    ctxt.loader.SetCarrierSym(s, state.carrier);
    ctxt.loader.SetAttrNotInSymbolTable(s, true);
    return s;
}

// makePclntab makes a pclntab object, and assembles all the compilation units
// we'll need to write pclntab. Returns the pclntab structure, a slice of the
// CompilationUnits we need, and a slice of the function symbols we need to
// generate pclntab.
private static (ptr<pclntab>, slice<ptr<sym.CompilationUnit>>, slice<loader.Sym>) makePclntab(ptr<Link> _addr_ctxt, loader.Bitmap container) {
    ptr<pclntab> _p0 = default!;
    slice<ptr<sym.CompilationUnit>> _p0 = default;
    slice<loader.Sym> _p0 = default;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;

    ptr<pclntab> state = addr(new pclntab(funcSize:uint32(ctxt.Arch.PtrSize+9*4),)); 

    // Gather some basic stats and info.
    var seenCUs = make();
    var prevSect = ldr.SymSect(ctxt.Textp[0]);
    ptr<sym.CompilationUnit> compUnits = new slice<ptr<sym.CompilationUnit>>(new ptr<sym.CompilationUnit>[] {  });
    loader.Sym funcs = new slice<loader.Sym>(new loader.Sym[] {  });

    foreach (var (_, s) in ctxt.Textp) {
        if (!emitPcln(_addr_ctxt, s, container)) {
            continue;
        }
        funcs = append(funcs, s);
        state.nfunc++;
        if (state.firstFunc == 0) {
            state.firstFunc = s;
        }
        state.lastFunc = s;
        var ss = ldr.SymSect(s);
        if (ss != prevSect) { 
            // With multiple text sections, the external linker may
            // insert functions between the sections, which are not
            // known by Go. This leaves holes in the PC range covered
            // by the func table. We need to generate an entry to mark
            // the hole.
            state.nfunc++;
            prevSect = ss;

        }
        var cu = ldr.SymUnit(s);
        {
            var (_, ok) = seenCUs[cu];

            if (cu != null && !ok) {
                seenCUs[cu] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                cu.PclnIndex = len(compUnits);
                compUnits = append(compUnits, cu);
            }

        }

    }    return (_addr_state!, compUnits, funcs);

}

private static bool emitPcln(ptr<Link> _addr_ctxt, loader.Sym s, loader.Bitmap container) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // We want to generate func table entries only for the "lowest
    // level" symbols, not containers of subsymbols.
    return !container.Has(s);

}

private static uint computeDeferReturn(ptr<Link> _addr_ctxt, loader.Sym deferReturnSym, loader.Sym s) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var target = ctxt.Target;
    var deferreturn = uint32(0);
    var lastWasmAddr = uint32(0);

    var relocs = ldr.Relocs(s);
    for (nint ri = 0; ri < relocs.Count(); ri++) {
        var r = relocs.At(ri);
        if (target.IsWasm() && r.Type() == objabi.R_ADDR) { 
            // Wasm does not have a live variable set at the deferreturn
            // call itself. Instead it has one identified by the
            // resumption point immediately preceding the deferreturn.
            // The wasm code has a R_ADDR relocation which is used to
            // set the resumption point to PC_B.
            lastWasmAddr = uint32(r.Add());

        }
        if (r.Type().IsDirectCall() && (r.Sym() == deferReturnSym || ldr.IsDeferReturnTramp(r.Sym()))) {
            if (target.IsWasm()) {
                deferreturn = lastWasmAddr - 1;
            }
            else
 { 
                // Note: the relocation target is in the call instruction, but
                // is not necessarily the whole instruction (for instance, on
                // x86 the relocation applies to bytes [1:5] of the 5 byte call
                // instruction).
                deferreturn = uint32(r.Off());

                if (target.Arch.Family == sys.AMD64 || target.Arch.Family == sys.I386) 
                    deferreturn--;
                else if (target.Arch.Family == sys.PPC64 || target.Arch.Family == sys.ARM || target.Arch.Family == sys.ARM64 || target.Arch.Family == sys.MIPS || target.Arch.Family == sys.MIPS64)                 else if (target.Arch.Family == sys.RISCV64) 
                    // TODO(jsing): The JALR instruction is marked with
                    // R_CALLRISCV, whereas the actual reloc is currently
                    // one instruction earlier starting with the AUIPC.
                    deferreturn -= 4;
                else if (target.Arch.Family == sys.S390X) 
                    deferreturn -= 2;
                else 
                    panic(fmt.Sprint("Unhandled architecture:", target.Arch.Family));
                
            }

            break; // only need one
        }
    }
    return deferreturn;

});

// genInlTreeSym generates the InlTree sym for a function with the
// specified FuncInfo.
private static loader.Sym genInlTreeSym(ptr<Link> _addr_ctxt, ptr<sym.CompilationUnit> _addr_cu, loader.FuncInfo fi, ptr<sys.Arch> _addr_arch, map<loader.Sym, uint> nameOffsets) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref sym.CompilationUnit cu = ref _addr_cu.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var ldr = ctxt.loader;
    var its = ldr.CreateExtSym("", 0);
    var inlTreeSym = ldr.MakeSymbolUpdater(its); 
    // Note: the generated symbol is given a type of sym.SGOFUNC, as a
    // signal to the symtab() phase that it needs to be grouped in with
    // other similar symbols (gcdata, etc); the dodata() phase will
    // eventually switch the type back to SRODATA.
    inlTreeSym.SetType(sym.SGOFUNC);
    ldr.SetAttrReachable(its, true);
    var ninl = fi.NumInlTree();
    for (nint i = 0; i < int(ninl); i++) {
        var call = fi.InlTree(i);
        var val = call.File;
        var (nameoff, ok) = nameOffsets[call.Func];
        if (!ok) {
            panic("couldn't find function name offset");
        }
        inlTreeSym.SetUint16(arch, int64(i * 20 + 0), uint16(call.Parent));
        var inlFunc = ldr.FuncInfo(call.Func);

        objabi.FuncID funcID = default;
        if (inlFunc.Valid()) {
            funcID = inlFunc.FuncID();
        }
        inlTreeSym.SetUint8(arch, int64(i * 20 + 2), uint8(funcID)); 

        // byte 3 is unused
        inlTreeSym.SetUint32(arch, int64(i * 20 + 4), uint32(val));
        inlTreeSym.SetUint32(arch, int64(i * 20 + 8), uint32(call.Line));
        inlTreeSym.SetUint32(arch, int64(i * 20 + 12), uint32(nameoff));
        inlTreeSym.SetUint32(arch, int64(i * 20 + 16), uint32(call.ParentPC));

    }
    return its;

});

// makeInlSyms returns a map of loader.Sym that are created inlSyms.
private static map<loader.Sym, loader.Sym> makeInlSyms(ptr<Link> _addr_ctxt, slice<loader.Sym> funcs, map<loader.Sym, uint> nameOffsets) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 
    // Create the inline symbols we need.
    var inlSyms = make_map<loader.Sym, loader.Sym>();
    foreach (var (_, s) in funcs) {
        {
            var fi = ldr.FuncInfo(s);

            if (fi.Valid()) {
                fi.Preload();
                if (fi.NumInlTree() > 0) {
                    inlSyms[s] = genInlTreeSym(_addr_ctxt, _addr_ldr.SymUnit(s), fi, _addr_ctxt.Arch, nameOffsets);
                }
            }

        }

    }    return inlSyms;

}

// generatePCHeader creates the runtime.pcheader symbol, setting it up as a
// generator to fill in its data later.
private static void generatePCHeader(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    Action<ptr<Link>, loader.Sym> writeHeader = (ctxt, s) => {
        var ldr = ctxt.loader;
        var header = ctxt.loader.MakeSymbolUpdater(s);

        Func<long, loader.Sym, long> writeSymOffset = (off, ws) => {
            var diff = ldr.SymValue(ws) - ldr.SymValue(s);
            if (diff <= 0) {
                var name = ldr.SymName(ws);
                panic(fmt.Sprintf("expected runtime.pcheader(%x) to be placed before %s(%x)", ldr.SymValue(s), name, ldr.SymValue(ws)));
            }
            return header.SetUintptr(ctxt.Arch, off, uintptr(diff));
        }; 

        // Write header.
        // Keep in sync with runtime/symtab.go:pcHeader.
        header.SetUint32(ctxt.Arch, 0, 0xfffffffa);
        header.SetUint8(ctxt.Arch, 6, uint8(ctxt.Arch.MinLC));
        header.SetUint8(ctxt.Arch, 7, uint8(ctxt.Arch.PtrSize));
        var off = header.SetUint(ctxt.Arch, 8, uint64(state.nfunc));
        off = header.SetUint(ctxt.Arch, off, uint64(state.nfiles));
        off = writeSymOffset(off, state.funcnametab);
        off = writeSymOffset(off, state.cutab);
        off = writeSymOffset(off, state.filetab);
        off = writeSymOffset(off, state.pctab);
        off = writeSymOffset(off, state.pclntab);

    };

    var size = int64(8 + 7 * ctxt.Arch.PtrSize);
    state.pcheader = state.addGeneratedSym(ctxt, "runtime.pcheader", size, writeHeader);

});

// walkFuncs iterates over the funcs, calling a function for each unique
// function and inlined function.
private static void walkFuncs(ptr<Link> _addr_ctxt, slice<loader.Sym> funcs, Action<loader.Sym> f) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var seen = make();
    foreach (var (_, s) in funcs) {
        {
            var (_, ok) = seen[s];

            if (!ok) {
                f(s);
                seen[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            }

        }


        var fi = ldr.FuncInfo(s);
        if (!fi.Valid()) {
            continue;
        }
        fi.Preload();
        for (nint i = 0;
        var ni = fi.NumInlTree(); i < int(ni); i++) {
            var call = fi.InlTree(i).Func;
            {
                (_, ok) = seen[call];

                if (!ok) {
                    f(call);
                    seen[call] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                }

            }

        }

    }
}

// generateFuncnametab creates the function name table. Returns a map of
// func symbol to the name offset in runtime.funcnamtab.
private static map<loader.Sym, uint> generateFuncnametab(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, slice<loader.Sym> funcs) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var nameOffsets = make_map<loader.Sym, uint>(state.nfunc); 

    // Write the null terminated strings.
    Action<ptr<Link>, loader.Sym> writeFuncNameTab = (ctxt, s) => {
        var symtab = ctxt.loader.MakeSymbolUpdater(s);
        foreach (var (s, off) in nameOffsets) {
            symtab.AddStringAt(int64(off), ctxt.loader.SymName(s));
        }
    }; 

    // Loop through the CUs, and calculate the size needed.
    long size = default;
    walkFuncs(_addr_ctxt, funcs, s => {
        nameOffsets[s] = uint32(size);
        size += int64(ctxt.loader.SymNameLen(s)) + 1; // NULL terminate
    });

    state.funcnametab = state.addGeneratedSym(ctxt, "runtime.funcnametab", size, writeFuncNameTab);
    return nameOffsets;

}

// walkFilenames walks funcs, calling a function for each filename used in each
// function's line table.
private static void walkFilenames(ptr<Link> _addr_ctxt, slice<loader.Sym> funcs, Action<ptr<sym.CompilationUnit>, goobj.CUFileIndex> f) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    // Loop through all functions, finding the filenames we need.
    foreach (var (_, s) in funcs) {
        var fi = ldr.FuncInfo(s);
        if (!fi.Valid()) {
            continue;
        }
        fi.Preload();

        var cu = ldr.SymUnit(s);
        {
            nint i__prev2 = i;

            for (nint i = 0;
            var nf = int(fi.NumFile()); i < nf; i++) {
                f(cu, fi.File(i));
            }


            i = i__prev2;
        }
        {
            nint i__prev2 = i;

            for (i = 0;
            var ninl = int(fi.NumInlTree()); i < ninl; i++) {
                var call = fi.InlTree(i);
                f(cu, call.File);
            }


            i = i__prev2;
        }

    }
}

// generateFilenameTabs creates LUTs needed for filename lookup. Returns a slice
// of the index at which each CU begins in runtime.cutab.
//
// Function objects keep track of the files they reference to print the stack.
// This function creates a per-CU list of filenames if CU[M] references
// files[1-N], the following is generated:
//
//  runtime.cutab:
//    CU[M]
//     offsetToFilename[0]
//     offsetToFilename[1]
//     ..
//
//  runtime.filetab
//     filename[0]
//     filename[1]
//
// Looking up a filename then becomes:
//  0) Given a func, and filename index [K]
//  1) Get Func.CUIndex:       M := func.cuOffset
//  2) Find filename offset:   fileOffset := runtime.cutab[M+K]
//  3) Get the filename:       getcstring(runtime.filetab[fileOffset])
private static slice<uint> generateFilenameTabs(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, slice<ptr<sym.CompilationUnit>> compUnits, slice<loader.Sym> funcs) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // On a per-CU basis, keep track of all the filenames we need.
    //
    // Note, that we store the filenames in a separate section in the object
    // files, and deduplicate based on the actual value. It would be better to
    // store the filenames as symbols, using content addressable symbols (and
    // then not loading extra filenames), and just use the hash value of the
    // symbol name to do this cataloging.
    //
    // TODO: Store filenames as symbols. (Note this would be easiest if you
    // also move strings to ALWAYS using the larger content addressable hash
    // function, and use that hash value for uniqueness testing.)
    var cuEntries = make_slice<goobj.CUFileIndex>(len(compUnits));
    var fileOffsets = make_map<@string, uint>(); 

    // Walk the filenames.
    // We store the total filename string length we need to load, and the max
    // file index we've seen per CU so we can calculate how large the
    // CU->global table needs to be.
    long fileSize = default;
    walkFilenames(_addr_ctxt, funcs, (cu, i) => { 
        // Note we use the raw filename for lookup, but use the expanded filename
        // when we save the size.
        var filename = cu.FileTable[i];
        {
            var (_, ok) = fileOffsets[filename];

            if (!ok) {
                fileOffsets[filename] = uint32(fileSize);
                fileSize += int64(len(expandFile(filename)) + 1); // NULL terminate
            } 

            // Find the maximum file index we've seen.

        } 

        // Find the maximum file index we've seen.
        if (cuEntries[cu.PclnIndex] < i + 1) {
            cuEntries[cu.PclnIndex] = i + 1; // Store max + 1
        }
    }); 

    // Calculate the size of the runtime.cutab variable.
    uint totalEntries = default;
    var cuOffsets = make_slice<uint>(len(cuEntries));
    {
        var i__prev1 = i;

        foreach (var (__i, __entries) in cuEntries) {
            i = __i;
            entries = __entries; 
            // Note, cutab is a slice of uint32, so an offset to a cu's entry is just the
            // running total of all cu indices we've needed to store so far, not the
            // number of bytes we've stored so far.
            cuOffsets[i] = totalEntries;
            totalEntries += uint32(entries);

        }
        i = i__prev1;
    }

    Action<ptr<Link>, loader.Sym> writeCutab = (ctxt, s) => {
        var sb = ctxt.loader.MakeSymbolUpdater(s);

        long off = default;
        {
            var i__prev1 = i;

            foreach (var (__i, __max) in cuEntries) {
                i = __i;
                max = __max; 
                // Write the per CU LUT.
                var cu = compUnits[i];
                for (var j = goobj.CUFileIndex(0); j < max; j++) {
                    var (fileOffset, ok) = fileOffsets[cu.FileTable[j]];
                    if (!ok) { 
                        // We're looping through all possible file indices. It's possible a file's
                        // been deadcode eliminated, and although it's a valid file in the CU, it's
                        // not needed in this binary. When that happens, use an invalid offset.
                        fileOffset = ~uint32(0);

                    }

                    off = sb.SetUint32(ctxt.Arch, off, fileOffset);

                }


            }

            i = i__prev1;
        }
    };
    state.cutab = state.addGeneratedSym(ctxt, "runtime.cutab", int64(totalEntries * 4), writeCutab); 

    // Write filetab.
    Action<ptr<Link>, loader.Sym> writeFiletab = (ctxt, s) => {
        sb = ctxt.loader.MakeSymbolUpdater(s); 

        // Write the strings.
        {
            var filename__prev1 = filename;

            foreach (var (__filename, __loc) in fileOffsets) {
                filename = __filename;
                loc = __loc;
                sb.AddStringAt(int64(loc), expandFile(filename));
            }

            filename = filename__prev1;
        }
    };
    state.nfiles = uint32(len(fileOffsets));
    state.filetab = state.addGeneratedSym(ctxt, "runtime.filetab", fileSize, writeFiletab);

    return cuOffsets;

}

// generatePctab creates the runtime.pctab variable, holding all the
// deduplicated pcdata.
private static void generatePctab(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, slice<loader.Sym> funcs) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    // Pctab offsets of 0 are considered invalid in the runtime. We respect
    // that by just padding a single byte at the beginning of runtime.pctab,
    // that way no real offsets can be zero.
    var size = int64(1); 

    // Walk the functions, finding offset to store each pcdata.
    var seen = make();
    Action<loader.Sym> saveOffset = pcSym => {
        {
            var (_, ok) = seen[pcSym];

            if (!ok) {
                var datSize = ldr.SymSize(pcSym);
                if (datSize != 0) {
                    ldr.SetSymValue(pcSym, size);
                }
                else
 { 
                    // Invalid PC data, record as zero.
                    ldr.SetSymValue(pcSym, 0);

                }

                size += datSize;
                seen[pcSym] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

            }

        }

    };
    foreach (var (_, s) in funcs) {
        var fi = ldr.FuncInfo(s);
        if (!fi.Valid()) {
            continue;
        }
        fi.Preload();

        loader.Sym pcSyms = new slice<loader.Sym>(new loader.Sym[] { fi.Pcsp(), fi.Pcfile(), fi.Pcline() });
        {
            var pcSym__prev2 = pcSym;

            foreach (var (_, __pcSym) in pcSyms) {
                pcSym = __pcSym;
                saveOffset(pcSym);
            }

            pcSym = pcSym__prev2;
        }

        {
            var pcSym__prev2 = pcSym;

            foreach (var (_, __pcSym) in fi.Pcdata()) {
                pcSym = __pcSym;
                saveOffset(pcSym);
            }

            pcSym = pcSym__prev2;
        }

        if (fi.NumInlTree() > 0) {
            saveOffset(fi.Pcinline());
        }
    }    Action<ptr<Link>, loader.Sym> writePctab = (ctxt, s) => {
        ldr = ctxt.loader;
        var sb = ldr.MakeSymbolUpdater(s);
        foreach (var (sym) in seen) {
            sb.SetBytesAt(ldr.SymValue(sym), ldr.Data(sym));
        }
    };

    state.pctab = state.addGeneratedSym(ctxt, "runtime.pctab", size, writePctab);

}

// numPCData returns the number of PCData syms for the FuncInfo.
// NB: Preload must be called on valid FuncInfos before calling this function.
private static uint numPCData(loader.FuncInfo fi) {
    if (!fi.Valid()) {
        return 0;
    }
    var numPCData = uint32(len(fi.Pcdata()));
    if (fi.NumInlTree() > 0) {
        if (numPCData < objabi.PCDATA_InlTreeIndex + 1) {
            numPCData = objabi.PCDATA_InlTreeIndex + 1;
        }
    }
    return numPCData;

}

// Helper types for iterating pclntab.
public delegate  long pclnSetAddr(ptr<loader.SymbolBuilder>,  ptr<sys.Arch>,  long,  loader.Sym,  long);
public delegate  long pclnSetUint(ptr<loader.SymbolBuilder>,  ptr<sys.Arch>,  long,  ulong);

// generateFunctab creates the runtime.functab
//
// runtime.functab contains two things:
//
//   - pc->func look up table.
//   - array of func objects, interleaved with pcdata and funcdata
//
// Because of timing in the linker, generating this table takes two passes.
// The first pass is executed early in the link, and it creates any needed
// relocations to layout the data. The pieces that need relocations are:
//   1) the PC->func table.
//   2) The entry points in the func objects.
//   3) The funcdata.
// (1) and (2) are handled in walkPCToFunc. (3) is handled in walkFuncdata.
//
// After relocations, once we know where to write things in the output buffer,
// we execute the second pass, which is actually writing the data.
private static void generateFunctab(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, slice<loader.Sym> funcs, map<loader.Sym, loader.Sym> inlSyms, slice<uint> cuOffsets, map<loader.Sym, uint> nameOffsets) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // Calculate the size of the table.
    var (size, startLocations) = state.calculateFunctabSize(ctxt, funcs); 

    // If we are internally linking a static executable, the function addresses
    // are known, so we can just use them instead of emitting relocations. For
    // other cases we still need to emit relocations.
    //
    // This boolean just helps us figure out which callback to use.
    var useSymValue = ctxt.IsExe() && ctxt.IsInternal();

    Action<ptr<Link>, loader.Sym> writePcln = (ctxt, s) => {
        var ldr = ctxt.loader;
        var sb = ldr.MakeSymbolUpdater(s); 

        // Create our callbacks.
        pclnSetAddr setAddr = default;
        if (useSymValue) { 
            // We need to write the offset.
            setAddr = (s, arch, off, tgt, add) => {
                {
                    var v__prev2 = v;

                    var v = ldr.SymValue(tgt);

                    if (v != 0) {
                        s.SetUint(arch, off, uint64(v + add));
                    }

                    v = v__prev2;

                }

                return 0;

            }
        else
;

        } { 
            // We already wrote relocations.
            setAddr = (s, arch, off, tgt, add) => 0;

        }
        writePcToFunc(_addr_ctxt, _addr_sb, funcs, startLocations, setAddr, (loader.SymbolBuilder.val).SetUint);
        writeFuncs(_addr_ctxt, _addr_sb, funcs, inlSyms, startLocations, cuOffsets, nameOffsets);
        state.writeFuncData(ctxt, sb, funcs, inlSyms, startLocations, setAddr, (loader.SymbolBuilder.val).SetUint);

    };

    state.pclntab = state.addGeneratedSym(ctxt, "runtime.functab", size, writePcln); 

    // Create the relocations we need.
    ldr = ctxt.loader;
    sb = ldr.MakeSymbolUpdater(state.pclntab);

    setAddr = default;
    if (useSymValue) { 
        // If we should use the symbol value, and we don't have one, write a relocation.
        setAddr = (sb, arch, off, tgt, add) => {
            {
                var v__prev2 = v;

                v = ldr.SymValue(tgt);

                if (v == 0) {
                    sb.SetAddrPlus(arch, off, tgt, add);
                }

                v = v__prev2;

            }

            return 0;

        }
    else
;

    } { 
        // If we're externally linking, write a relocation.
        setAddr = (loader.SymbolBuilder.val).SetAddrPlus;

    }
    Func<ptr<loader.SymbolBuilder>, ptr<sys.Arch>, long, ulong, long> setUintNOP = (_p0, _p0, _p0, _p0) => 0;
    writePcToFunc(_addr_ctxt, _addr_sb, funcs, startLocations, setAddr, setUintNOP);
    if (!useSymValue) { 
        // Generate relocations for funcdata when externally linking.
        state.writeFuncData(ctxt, sb, funcs, inlSyms, startLocations, setAddr, setUintNOP);
        sb.SortRelocs();

    }
}

// funcData returns the funcdata and offsets for the FuncInfo.
// The funcdata and offsets are written into runtime.functab after each func
// object. This is a helper function to make querying the FuncInfo object
// cleaner.
//
// Note, the majority of fdOffsets are 0, meaning there is no offset between
// the compiler's generated symbol, and what the runtime needs. They are
// plumbed through for no loss of generality.
//
// NB: Preload must be called on the FuncInfo before calling.
// NB: fdSyms and fdOffs are used as scratch space.
private static (slice<loader.Sym>, slice<long>) funcData(loader.FuncInfo fi, loader.Sym inlSym, slice<loader.Sym> fdSyms, slice<long> fdOffs) {
    slice<loader.Sym> _p0 = default;
    slice<long> _p0 = default;

    (fdSyms, fdOffs) = (fdSyms[..(int)0], fdOffs[..(int)0]);    if (fi.Valid()) {
        var numOffsets = int(fi.NumFuncdataoff());
        for (nint i = 0; i < numOffsets; i++) {
            fdOffs = append(fdOffs, fi.Funcdataoff(i));
        }
        fdSyms = fi.Funcdata(fdSyms);
        if (fi.NumInlTree() > 0) {
            if (len(fdSyms) < objabi.FUNCDATA_InlTree + 1) {
                fdSyms = append(fdSyms, make_slice<loader.Sym>(objabi.FUNCDATA_InlTree + 1 - len(fdSyms)));
                fdOffs = append(fdOffs, make_slice<long>(objabi.FUNCDATA_InlTree + 1 - len(fdOffs)));
            }
            fdSyms[objabi.FUNCDATA_InlTree] = inlSym;
        }
    }
    return (fdSyms, fdOffs);

}

// calculateFunctabSize calculates the size of the pclntab, and the offsets in
// the output buffer for individual func entries.
private static (long, slice<uint>) calculateFunctabSize(this pclntab state, ptr<Link> _addr_ctxt, slice<loader.Sym> funcs) {
    long _p0 = default;
    slice<uint> _p0 = default;
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var startLocations = make_slice<uint>(len(funcs)); 

    // Allocate space for the pc->func table. This structure consists of a pc
    // and an offset to the func structure. After that, we have a single pc
    // value that marks the end of the last function in the binary.
    var size = int64(int(state.nfunc) * 2 * ctxt.Arch.PtrSize + ctxt.Arch.PtrSize); 

    // Now find the space for the func objects. We do this in a running manner,
    // so that we can find individual starting locations, and because funcdata
    // requires alignment.
    foreach (var (i, s) in funcs) {
        size = Rnd(size, int64(ctxt.Arch.PtrSize));
        startLocations[i] = uint32(size);
        var fi = ldr.FuncInfo(s);
        size += int64(state.funcSize);
        if (fi.Valid()) {
            fi.Preload();
            var numFuncData = int(fi.NumFuncdataoff());
            if (fi.NumInlTree() > 0) {
                if (numFuncData < objabi.FUNCDATA_InlTree + 1) {
                    numFuncData = objabi.FUNCDATA_InlTree + 1;
                }
            }
            size += int64(numPCData(fi) * 4);
            if (numFuncData > 0) { // Func data is aligned.
                size = Rnd(size, int64(ctxt.Arch.PtrSize));

            }

            size += int64(numFuncData * ctxt.Arch.PtrSize);

        }
    }    return (size, startLocations);

}

// writePcToFunc writes the PC->func lookup table.
// This function walks the pc->func lookup table, executing callbacks
// to generate relocations and writing the values for the table.
private static void writePcToFunc(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_sb, slice<loader.Sym> funcs, slice<uint> startLocations, pclnSetAddr setAddr, pclnSetUint setUint) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    var ldr = ctxt.loader;
    loader.Sym prevFunc = default;
    var prevSect = ldr.SymSect(funcs[0]);
    nint funcIndex = 0;
    foreach (var (i, s) in funcs) {
        {
            var thisSect = ldr.SymSect(s);

            if (thisSect != prevSect) { 
                // With multiple text sections, there may be a hole here in the
                // address space. We use an invalid funcoff value to mark the hole.
                // See also runtime/symtab.go:findfunc
                var prevFuncSize = int64(ldr.SymSize(prevFunc));
                setAddr(sb, ctxt.Arch, int64(funcIndex * 2 * ctxt.Arch.PtrSize), prevFunc, prevFuncSize);
                setUint(sb, ctxt.Arch, int64((funcIndex * 2 + 1) * ctxt.Arch.PtrSize), ~uint64(0));
                funcIndex++;
                prevSect = thisSect;

            }

        }

        prevFunc = s; 
        // TODO: We don't actually need these relocations, provided we go to a
        // module->func look-up-table like we do for filenames. We could have a
        // single relocation for the module, and have them all laid out as
        // offsets from the beginning of that module.
        setAddr(sb, ctxt.Arch, int64(funcIndex * 2 * ctxt.Arch.PtrSize), s, 0);
        setUint(sb, ctxt.Arch, int64((funcIndex * 2 + 1) * ctxt.Arch.PtrSize), uint64(startLocations[i]));
        funcIndex++; 

        // Write the entry location.
        setAddr(sb, ctxt.Arch, int64(startLocations[i]), s, 0);

    }    setAddr(sb, ctxt.Arch, int64(funcIndex) * 2 * int64(ctxt.Arch.PtrSize), prevFunc, ldr.SymSize(prevFunc));

}

// writeFuncData writes the funcdata tables.
//
// This function executes a callback for each funcdata needed in
// runtime.functab. It should be called once for internally linked static
// binaries, or twice (once to generate the needed relocations) for other
// build modes.
//
// Note the output of this function is interwoven with writeFuncs, but this is
// a separate function, because it's needed in different passes in
// generateFunctab.
private static void writeFuncData(this ptr<pclntab> _addr_state, ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_sb, slice<loader.Sym> funcs, map<loader.Sym, loader.Sym> inlSyms, slice<uint> startLocations, pclnSetAddr setAddr, pclnSetUint setUint) {
    ref pclntab state = ref _addr_state.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    var ldr = ctxt.loader;
    loader.Sym funcdata = new slice<loader.Sym>(new loader.Sym[] {  });
    long funcdataoff = new slice<long>(new long[] {  });
    foreach (var (i, s) in funcs) {
        var fi = ldr.FuncInfo(s);
        if (!fi.Valid()) {
            continue;
        }
        fi.Preload(); 

        // funcdata, must be pointer-aligned and we're only int32-aligned.
        // Missing funcdata will be 0 (nil pointer).
        var (funcdata, funcdataoff) = funcData(fi, inlSyms[s], funcdata, funcdataoff);
        if (len(funcdata) > 0) {
            var off = int64(startLocations[i] + state.funcSize + numPCData(fi) * 4);
            off = Rnd(off, int64(ctxt.Arch.PtrSize));
            foreach (var (j) in funcdata) {
                var dataoff = off + int64(ctxt.Arch.PtrSize * j);
                if (funcdata[j] == 0) {
                    setUint(sb, ctxt.Arch, dataoff, uint64(funcdataoff[j]));
                    continue;
                } 
                // TODO: Does this need deduping?
                setAddr(sb, ctxt.Arch, dataoff, funcdata[j], funcdataoff[j]);

            }

        }
    }
}

// writeFuncs writes the func structures and pcdata to runtime.functab.
private static void writeFuncs(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_sb, slice<loader.Sym> funcs, map<loader.Sym, loader.Sym> inlSyms, slice<uint> startLocations, slice<uint> cuOffsets, map<loader.Sym, uint> nameOffsets) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    var ldr = ctxt.loader;
    var deferReturnSym = ldr.Lookup("runtime.deferreturn", sym.SymVerABIInternal);
    loader.Sym funcdata = new slice<loader.Sym>(new loader.Sym[] {  });
    long funcdataoff = new slice<long>(new long[] {  }); 

    // Write the individual func objects.
    foreach (var (i, s) in funcs) {
        var fi = ldr.FuncInfo(s);
        if (fi.Valid()) {
            fi.Preload();
        }
        var off = startLocations[i] + uint32(ctxt.Arch.PtrSize); // entry

        // name int32
        var (nameoff, ok) = nameOffsets[s];
        if (!ok) {
            panic("couldn't find function name offset");
        }
        off = uint32(sb.SetUint32(ctxt.Arch, int64(off), uint32(nameoff))); 

        // args int32
        // TODO: Move into funcinfo.
        var args = uint32(0);
        if (fi.Valid()) {
            args = uint32(fi.Args());
        }
        off = uint32(sb.SetUint32(ctxt.Arch, int64(off), args)); 

        // deferreturn
        var deferreturn = computeDeferReturn(_addr_ctxt, deferReturnSym, s);
        off = uint32(sb.SetUint32(ctxt.Arch, int64(off), deferreturn)); 

        // pcdata
        if (fi.Valid()) {
            off = uint32(sb.SetUint32(ctxt.Arch, int64(off), uint32(ldr.SymValue(fi.Pcsp()))));
            off = uint32(sb.SetUint32(ctxt.Arch, int64(off), uint32(ldr.SymValue(fi.Pcfile()))));
            off = uint32(sb.SetUint32(ctxt.Arch, int64(off), uint32(ldr.SymValue(fi.Pcline()))));
        }
        else
 {
            off += 12;
        }
        off = uint32(sb.SetUint32(ctxt.Arch, int64(off), uint32(numPCData(fi)))); 

        // Store the offset to compilation unit's file table.
        var cuIdx = ~uint32(0);
        {
            var cu = ldr.SymUnit(s);

            if (cu != null) {
                cuIdx = cuOffsets[cu.PclnIndex];
            }

        }

        off = uint32(sb.SetUint32(ctxt.Arch, int64(off), cuIdx)); 

        // funcID uint8
        objabi.FuncID funcID = default;
        if (fi.Valid()) {
            funcID = fi.FuncID();
        }
        off = uint32(sb.SetUint8(ctxt.Arch, int64(off), uint8(funcID))); 

        // flag uint8
        objabi.FuncFlag flag = default;
        if (fi.Valid()) {
            flag = fi.FuncFlag();
        }
        off = uint32(sb.SetUint8(ctxt.Arch, int64(off), uint8(flag)));

        off += 1; // pad

        // nfuncdata must be the final entry.
        funcdata, funcdataoff = funcData(fi, 0, funcdata, funcdataoff);
        off = uint32(sb.SetUint8(ctxt.Arch, int64(off), uint8(len(funcdata)))); 

        // Output the pcdata.
        if (fi.Valid()) {
            foreach (var (j, pcSym) in fi.Pcdata()) {
                sb.SetUint32(ctxt.Arch, int64(off + uint32(j * 4)), uint32(ldr.SymValue(pcSym)));
            }
            if (fi.NumInlTree() > 0) {
                sb.SetUint32(ctxt.Arch, int64(off + objabi.PCDATA_InlTreeIndex * 4), uint32(ldr.SymValue(fi.Pcinline())));
            }
        }
    }
});

// pclntab initializes the pclntab symbol with
// runtime function and file name information.

// pclntab generates the pcln table for the link output.
private static ptr<pclntab> pclntab(this ptr<Link> _addr_ctxt, loader.Bitmap container) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // Go 1.2's symtab layout is documented in golang.org/s/go12symtab, but the
    // layout and data has changed since that time.
    //
    // As of August 2020, here's the layout of pclntab:
    //
    //  .gopclntab/__gopclntab [elf/macho section]
    //    runtime.pclntab
    //      Carrier symbol for the entire pclntab section.
    //
    //      runtime.pcheader  (see: runtime/symtab.go:pcHeader)
    //        8-byte magic
    //        nfunc [thearch.ptrsize bytes]
    //        offset to runtime.funcnametab from the beginning of runtime.pcheader
    //        offset to runtime.pclntab_old from beginning of runtime.pcheader
    //
    //      runtime.funcnametab
    //        []list of null terminated function names
    //
    //      runtime.cutab
    //        for i=0..#CUs
    //          for j=0..#max used file index in CU[i]
    //            uint32 offset into runtime.filetab for the filename[j]
    //
    //      runtime.filetab
    //        []null terminated filename strings
    //
    //      runtime.pctab
    //        []byte of deduplicated pc data.
    //
    //      runtime.functab
    //        function table, alternating PC and offset to func struct [each entry thearch.ptrsize bytes]
    //        end PC [thearch.ptrsize bytes]
    //        func structures, pcdata offsets, func data.

    var (state, compUnits, funcs) = makePclntab(_addr_ctxt, container);

    var ldr = ctxt.loader;
    state.carrier = ldr.LookupOrCreateSym("runtime.pclntab", 0);
    ldr.MakeSymbolUpdater(state.carrier).SetType(sym.SPCLNTAB);
    ldr.SetAttrReachable(state.carrier, true);
    setCarrierSym(sym.SPCLNTAB, state.carrier);

    state.generatePCHeader(ctxt);
    var nameOffsets = state.generateFuncnametab(ctxt, funcs);
    var cuOffsets = state.generateFilenameTabs(ctxt, compUnits, funcs);
    state.generatePctab(ctxt, funcs);
    var inlSyms = makeInlSyms(_addr_ctxt, funcs, nameOffsets);
    state.generateFunctab(ctxt, funcs, inlSyms, cuOffsets, nameOffsets);

    return _addr_state!;

}

private static @string gorootFinal() {
    var root = buildcfg.GOROOT;
    {
        var final = os.Getenv("GOROOT_FINAL");

        if (final != "") {
            root = final;
        }
    }

    return root;

}

private static @string expandGoroot(@string s) {
    const var n = len("$GOROOT");

    if (len(s) >= n + 1 && s[..(int)n] == "$GOROOT" && (s[n] == '/' || s[n] == '\\')) {
        return filepath.ToSlash(filepath.Join(gorootFinal(), s[(int)n..]));
    }
    return s;

}

public static readonly nint BUCKETSIZE = 256 * MINFUNC;
public static readonly nint SUBBUCKETS = 16;
public static readonly var SUBBUCKETSIZE = BUCKETSIZE / SUBBUCKETS;
public static readonly nuint NOIDX = 0x7fffffff;


// findfunctab generates a lookup table to quickly find the containing
// function for a pc. See src/runtime/symtab.go:findfunc for details.
private static void findfunctab(this ptr<Link> _addr_ctxt, ptr<pclntab> _addr_state, loader.Bitmap container) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref pclntab state = ref _addr_state.val;

    var ldr = ctxt.loader; 

    // find min and max address
    var min = ldr.SymValue(ctxt.Textp[0]);
    var lastp = ctxt.Textp[len(ctxt.Textp) - 1];
    var max = ldr.SymValue(lastp) + ldr.SymSize(lastp); 

    // for each subbucket, compute the minimum of all symbol indexes
    // that map to that subbucket.
    var n = int32((max - min + SUBBUCKETSIZE - 1) / SUBBUCKETSIZE);

    var nbuckets = int32((max - min + BUCKETSIZE - 1) / BUCKETSIZE);

    nint size = 4 * int64(nbuckets) + int64(n);

    Action<ptr<Link>, loader.Sym> writeFindFuncTab = (_, s) => {
        var t = ldr.MakeSymbolUpdater(s);

        var indexes = make_slice<int>(n);
        {
            var i__prev1 = i;

            for (var i = int32(0); i < n; i++) {
                indexes[i] = NOIDX;
            }


            i = i__prev1;
        }
        var idx = int32(0);
        {
            var i__prev1 = i;

            foreach (var (__i, __s) in ctxt.Textp) {
                i = __i;
                s = __s;
                if (!emitPcln(_addr_ctxt, s, container)) {
                    continue;
                }
                var p = ldr.SymValue(s);
                loader.Sym e = default;
                i++;
                if (i < len(ctxt.Textp)) {
                    e = ctxt.Textp[i];
                }
                while (e != 0 && !emitPcln(_addr_ctxt, e, container) && i < len(ctxt.Textp)) {
                    e = ctxt.Textp[i];
                    i++;
                }

                var q = max;
                if (e != 0) {
                    q = ldr.SymValue(e);
                } 

                //print("%d: [%lld %lld] %s\n", idx, p, q, s->name);
                while (p < q) {
                    i = int((p - min) / SUBBUCKETSIZE);
                    if (indexes[i] > idx) {
                        indexes[i] = idx;
                    p += SUBBUCKETSIZE;
                    }

                }


                i = int((q - 1 - min) / SUBBUCKETSIZE);
                if (indexes[i] > idx) {
                    indexes[i] = idx;
                }

                idx++;

            } 

            // fill in table

            i = i__prev1;
        }

        {
            var i__prev1 = i;

            for (i = int32(0); i < nbuckets; i++) {
                var @base = indexes[i * SUBBUCKETS];
                if (base == NOIDX) {
                    Errorf(null, "hole in findfunctab");
                }
                t.SetUint32(ctxt.Arch, int64(i) * (4 + SUBBUCKETS), uint32(base));
                for (var j = int32(0); j < SUBBUCKETS && i * SUBBUCKETS + j < n; j++) {
                    idx = indexes[i * SUBBUCKETS + j];
                    if (idx == NOIDX) {
                        Errorf(null, "hole in findfunctab");
                    }
                    if (idx - base >= 256) {
                        Errorf(null, "too many functions in a findfunc bucket! %d/%d %d %d", i, nbuckets, j, idx - base);
                    }
                    t.SetUint8(ctxt.Arch, int64(i) * (4 + SUBBUCKETS) + 4 + int64(j), uint8(idx - base));
                }
            }


            i = i__prev1;
        }

    };

    state.findfunctab = ctxt.createGeneratorSymbol("runtime.findfunctab", 0, sym.SRODATA, size, writeFindFuncTab);
    ldr.SetAttrReachable(state.findfunctab, true);
    ldr.SetAttrLocal(state.findfunctab, true);

}

// findContainerSyms returns a bitmap, indexed by symbol number, where there's
// a 1 for every container symbol.
private static loader.Bitmap findContainerSyms(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var container = loader.MakeBitmap(ldr.NSym()); 
    // Find container symbols and mark them as such.
    foreach (var (_, s) in ctxt.Textp) {
        var outer = ldr.OuterSym(s);
        if (outer != 0) {
            container.Set(outer);
        }
    }    return container;

}

} // end ld_package
