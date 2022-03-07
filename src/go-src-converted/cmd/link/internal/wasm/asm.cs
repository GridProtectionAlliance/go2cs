// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2022 March 06 23:20:21 UTC
// import "cmd/link/internal/wasm" ==> using wasm = go.cmd.link.@internal.wasm_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\wasm\asm.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using ld = go.cmd.link.@internal.ld_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using regexp = go.regexp_package;

namespace go.cmd.link.@internal;

public static partial class wasm_package {

public static readonly nuint I32 = 0x7F;
public static readonly nuint I64 = 0x7E;
public static readonly nuint F32 = 0x7D;
public static readonly nuint F64 = 0x7C;


private static readonly nint sectionCustom = 0;
private static readonly nint sectionType = 1;
private static readonly nint sectionImport = 2;
private static readonly nint sectionFunction = 3;
private static readonly nint sectionTable = 4;
private static readonly nint sectionMemory = 5;
private static readonly nint sectionGlobal = 6;
private static readonly nint sectionExport = 7;
private static readonly nint sectionStart = 8;
private static readonly nint sectionElement = 9;
private static readonly nint sectionCode = 10;
private static readonly nint sectionData = 11;


// funcValueOffset is the offset between the PC_F value of a function and the index of the function in WebAssembly
private static readonly nuint funcValueOffset = 0x1000; // TODO(neelance): make function addresses play nice with heap addresses

 // TODO(neelance): make function addresses play nice with heap addresses

private static void gentext(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

}

private partial struct wasmFunc {
    public @string Name;
    public uint Type;
    public slice<byte> Code;
}

private partial struct wasmFuncType {
    public slice<byte> Params;
    public slice<byte> Results;
}

private static map wasmFuncTypes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<wasmFuncType>>{"_rt0_wasm_js":{Params:[]byte{}},"wasm_export_run":{Params:[]byte{I32,I32}},"wasm_export_resume":{Params:[]byte{}},"wasm_export_getsp":{Results:[]byte{I32}},"wasm_pc_f_loop":{Params:[]byte{}},"runtime.wasmMove":{Params:[]byte{I32,I32,I32}},"runtime.wasmZero":{Params:[]byte{I32,I32}},"runtime.wasmDiv":{Params:[]byte{I64,I64},Results:[]byte{I64}},"runtime.wasmTruncS":{Params:[]byte{F64},Results:[]byte{I64}},"runtime.wasmTruncU":{Params:[]byte{F64},Results:[]byte{I64}},"runtime.gcWriteBarrier":{Params:[]byte{I64,I64}},"cmpbody":{Params:[]byte{I64,I64,I64,I64},Results:[]byte{I64}},"memeqbody":{Params:[]byte{I64,I64,I64},Results:[]byte{I64}},"memcmp":{Params:[]byte{I32,I32,I32},Results:[]byte{I32}},"memchr":{Params:[]byte{I32,I32,I32},Results:[]byte{I32}},};

private static (ptr<sym.Section>, nint, ulong) assignAddress(ptr<loader.Loader> _addr_ldr, ptr<sym.Section> _addr_sect, nint n, loader.Sym s, ulong va, bool isTramp) {
    ptr<sym.Section> _p0 = default!;
    nint _p0 = default;
    ulong _p0 = default;
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sym.Section sect = ref _addr_sect.val;
 
    // WebAssembly functions do not live in the same address space as the linear memory.
    // Instead, WebAssembly automatically assigns indices. Imported functions (section "import")
    // have indices 0 to n. They are followed by native functions (sections "function" and "code")
    // with indices n+1 and following.
    //
    // The following rules describe how wasm handles function indices and addresses:
    //   PC_F = funcValueOffset + WebAssembly function index (not including the imports)
    //   s.Value = PC = PC_F<<16 + PC_B
    //
    // The funcValueOffset is necessary to avoid conflicts with expectations
    // that the Go runtime has about function addresses.
    // The field "s.Value" corresponds to the concept of PC at runtime.
    // However, there is no PC register, only PC_F and PC_B. PC_F denotes the function,
    // PC_B the resume point inside of that function. The entry of the function has PC_B = 0.
    ldr.SetSymSect(s, sect);
    ldr.SetSymValue(s, int64(funcValueOffset + va / ld.MINFUNC) << 16); // va starts at zero
    va += uint64(ld.MINFUNC);
    return (_addr_sect!, n, va);

}

private partial struct wasmDataSect {
    public ptr<sym.Section> sect;
    public slice<byte> data;
}

private static slice<wasmDataSect> dataSects = default;

private static void asmb(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    ptr<sym.Section> sections = new slice<ptr<sym.Section>>(new ptr<sym.Section>[] { ldr.SymSect(ldr.Lookup("runtime.rodata",0)), ldr.SymSect(ldr.Lookup("runtime.typelink",0)), ldr.SymSect(ldr.Lookup("runtime.itablink",0)), ldr.SymSect(ldr.Lookup("runtime.symtab",0)), ldr.SymSect(ldr.Lookup("runtime.pclntab",0)), ldr.SymSect(ldr.Lookup("runtime.noptrdata",0)), ldr.SymSect(ldr.Lookup("runtime.data",0)) });

    dataSects = make_slice<wasmDataSect>(len(sections));
    foreach (var (i, sect) in sections) {
        var data = ld.DatblkBytes(ctxt, int64(sect.Vaddr), int64(sect.Length));
        dataSects[i] = new wasmDataSect(sect,data);
    }
}

// asmb writes the final WebAssembly module binary.
// Spec: https://webassembly.github.io/spec/core/binary/modules.html
private static void asmb2(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    ref ptr<wasmFuncType> types = ref heap(new slice<ptr<wasmFuncType>>(new ptr<wasmFuncType>[] { {Params:[]byte{I32},Results:[]byte{I32}} }), out ptr<ptr<wasmFuncType>> _addr_types); 

    // collect host imports (functions that get imported from the WebAssembly host, usually JavaScript)
    ptr<wasmFunc> hostImports = new slice<ptr<wasmFunc>>(new ptr<wasmFunc>[] { {Name:"debug",Type:lookupType(&wasmFuncType{Params:[]byte{I32}},&types),} });
    var hostImportMap = make_map<loader.Sym, long>();
    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in ctxt.Textp) {
            fn = __fn;
            var relocs = ldr.Relocs(fn);
            {
                nint ri__prev2 = ri;

                for (nint ri = 0; ri < relocs.Count(); ri++) {
                    var r = relocs.At(ri);
                    if (r.Type() == objabi.R_WASMIMPORT) {
                        hostImportMap[r.Sym()] = int64(len(hostImports));
                        hostImports = append(hostImports, addr(new wasmFunc(Name:ldr.SymName(r.Sym()),Type:lookupType(&wasmFuncType{Params:[]byte{I32}},&types),)));
                    }
                }


                ri = ri__prev2;
            }

        }
        fn = fn__prev1;
    }

    slice<byte> buildid = default;
    var fns = make_slice<ptr<wasmFunc>>(len(ctxt.Textp));
    {
        var fn__prev1 = fn;

        foreach (var (__i, __fn) in ctxt.Textp) {
            i = __i;
            fn = __fn;
            ptr<object> wfn = @new<bytes.Buffer>();
            if (ldr.SymName(fn) == "go.buildid") {
                writeUleb128(wfn, 0); // number of sets of locals
                writeI32Const(wfn, 0);
                wfn.WriteByte(0x0b); // end
                buildid = ldr.Data(fn);

            }
            else
 { 
                // Relocations have variable length, handle them here.
                relocs = ldr.Relocs(fn);
                var P = ldr.Data(fn);
                var off = int32(0);
                {
                    nint ri__prev2 = ri;

                    for (ri = 0; ri < relocs.Count(); ri++) {
                        r = relocs.At(ri);
                        if (r.Siz() == 0) {
                            continue; // skip marker relocations
                        }

                        wfn.Write(P[(int)off..(int)r.Off()]);
                        off = r.Off();
                        var rs = ldr.ResolveABIAlias(r.Sym());

                        if (r.Type() == objabi.R_ADDR) 
                            writeSleb128(wfn, ldr.SymValue(rs) + r.Add());
                        else if (r.Type() == objabi.R_CALL) 
                            writeSleb128(wfn, int64(len(hostImports)) + ldr.SymValue(rs) >> 16 - funcValueOffset);
                        else if (r.Type() == objabi.R_WASMIMPORT) 
                            writeSleb128(wfn, hostImportMap[rs]);
                        else 
                            ldr.Errorf(fn, "bad reloc type %d (%s)", r.Type(), sym.RelocName(ctxt.Arch, r.Type()));
                            continue;
                        
                    }


                    ri = ri__prev2;
                }
                wfn.Write(P[(int)off..]);

            }

            var typ = uint32(0);
            {
                var (sig, ok) = wasmFuncTypes[ldr.SymName(fn)];

                if (ok) {
                    typ = lookupType(_addr_sig, _addr_types);
                }

            }


            var name = nameRegexp.ReplaceAllString(ldr.SymName(fn), "_");
            fns[i] = addr(new wasmFunc(Name:name,Type:typ,Code:wfn.Bytes()));

        }
        fn = fn__prev1;
    }

    ctxt.Out.Write(new slice<byte>(new byte[] { 0x00, 0x61, 0x73, 0x6d })); // magic
    ctxt.Out.Write(new slice<byte>(new byte[] { 0x01, 0x00, 0x00, 0x00 })); // version

    // Add any buildid early in the binary:
    if (len(buildid) != 0) {
        writeBuildID(_addr_ctxt, buildid);
    }
    writeTypeSec(_addr_ctxt, types);
    writeImportSec(_addr_ctxt, hostImports);
    writeFunctionSec(_addr_ctxt, fns);
    writeTableSec(_addr_ctxt, fns);
    writeMemorySec(_addr_ctxt, _addr_ldr);
    writeGlobalSec(_addr_ctxt);
    writeExportSec(_addr_ctxt, _addr_ldr, len(hostImports));
    writeElementSec(_addr_ctxt, uint64(len(hostImports)), uint64(len(fns)));
    writeCodeSec(_addr_ctxt, fns);
    writeDataSec(_addr_ctxt);
    writeProducerSec(_addr_ctxt);
    if (!ld.FlagS.val) {
        writeNameSec(_addr_ctxt, len(hostImports), fns);
    }
}

private static uint lookupType(ptr<wasmFuncType> _addr_sig, ptr<slice<ptr<wasmFuncType>>> _addr_types) {
    ref wasmFuncType sig = ref _addr_sig.val;
    ref slice<ptr<wasmFuncType>> types = ref _addr_types.val;

    foreach (var (i, t) in types.val) {
        if (bytes.Equal(sig.Params, t.Params) && bytes.Equal(sig.Results, t.Results)) {
            return uint32(i);
        }
    }    types.val = append(types.val, sig);
    return uint32(len(types.val) - 1);

}

private static long writeSecHeader(ptr<ld.Link> _addr_ctxt, byte id) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    ctxt.Out.WriteByte(id);
    var sizeOffset = ctxt.Out.Offset();
    ctxt.Out.Write(make_slice<byte>(5)); // placeholder for length
    return sizeOffset;

}

private static void writeSecSize(ptr<ld.Link> _addr_ctxt, long sizeOffset) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var endOffset = ctxt.Out.Offset();
    ctxt.Out.SeekSet(sizeOffset);
    writeUleb128FixedLength(ctxt.Out, uint64(endOffset - sizeOffset - 5), 5);
    ctxt.Out.SeekSet(endOffset);
}

private static void writeBuildID(ptr<ld.Link> _addr_ctxt, slice<byte> buildid) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
    writeName(ctxt.Out, "go.buildid");
    ctxt.Out.Write(buildid);
    writeSecSize(_addr_ctxt, sizeOffset);
}

// writeTypeSec writes the section that declares all function types
// so they can be referenced by index.
private static void writeTypeSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFuncType>> types) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionType);

    writeUleb128(ctxt.Out, uint64(len(types)));

    foreach (var (_, t) in types) {
        ctxt.Out.WriteByte(0x60); // functype
        writeUleb128(ctxt.Out, uint64(len(t.Params)));
        {
            var v__prev2 = v;

            foreach (var (_, __v) in t.Params) {
                v = __v;
                ctxt.Out.WriteByte(byte(v));
            }

            v = v__prev2;
        }

        writeUleb128(ctxt.Out, uint64(len(t.Results)));
        {
            var v__prev2 = v;

            foreach (var (_, __v) in t.Results) {
                v = __v;
                ctxt.Out.WriteByte(byte(v));
            }

            v = v__prev2;
        }
    }    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeImportSec writes the section that lists the functions that get
// imported from the WebAssembly host, usually JavaScript.
private static void writeImportSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> hostImports) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionImport);

    writeUleb128(ctxt.Out, uint64(len(hostImports))); // number of imports
    foreach (var (_, fn) in hostImports) {
        writeName(ctxt.Out, "go"); // provided by the import object in wasm_exec.js
        writeName(ctxt.Out, fn.Name);
        ctxt.Out.WriteByte(0x00); // func import
        writeUleb128(ctxt.Out, uint64(fn.Type));

    }    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeFunctionSec writes the section that declares the types of functions.
// The bodies of these functions will later be provided in the "code" section.
private static void writeFunctionSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionFunction);

    writeUleb128(ctxt.Out, uint64(len(fns)));
    foreach (var (_, fn) in fns) {
        writeUleb128(ctxt.Out, uint64(fn.Type));
    }    writeSecSize(_addr_ctxt, sizeOffset);
}

// writeTableSec writes the section that declares tables. Currently there is only a single table
// that is used by the CallIndirect operation to dynamically call any function.
// The contents of the table get initialized by the "element" section.
private static void writeTableSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionTable);

    var numElements = uint64(funcValueOffset + len(fns));
    writeUleb128(ctxt.Out, 1); // number of tables
    ctxt.Out.WriteByte(0x70); // type: anyfunc
    ctxt.Out.WriteByte(0x00); // no max
    writeUleb128(ctxt.Out, numElements); // min

    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeMemorySec writes the section that declares linear memories. Currently one linear memory is being used.
// Linear memory always starts at address zero. More memory can be requested with the GrowMemory instruction.
private static void writeMemorySec(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionMemory);

    var dataSection = ldr.SymSect(ldr.Lookup("runtime.data", 0));
    var dataEnd = dataSection.Vaddr + dataSection.Length;
    var initialSize = dataEnd + 16 << 20; // 16MB, enough for runtime init without growing

    const nint wasmPageSize = 64 << 10; // 64KB

 // 64KB

    writeUleb128(ctxt.Out, 1); // number of memories
    ctxt.Out.WriteByte(0x00); // no maximum memory size
    writeUleb128(ctxt.Out, initialSize / wasmPageSize); // minimum (initial) memory size

    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeGlobalSec writes the section that declares global variables.
private static void writeGlobalSec(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionGlobal);

    byte globalRegs = new slice<byte>(new byte[] { I32, I64, I64, I64, I64, I64, I64, I32 });

    writeUleb128(ctxt.Out, uint64(len(globalRegs))); // number of globals

    foreach (var (_, typ) in globalRegs) {
        ctxt.Out.WriteByte(typ);
        ctxt.Out.WriteByte(0x01); // var

        if (typ == I32) 
            writeI32Const(ctxt.Out, 0);
        else if (typ == I64) 
            writeI64Const(ctxt.Out, 0);
                ctxt.Out.WriteByte(0x0b); // end
    }    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeExportSec writes the section that declares exports.
// Exports can be accessed by the WebAssembly host, usually JavaScript.
// The wasm_export_* functions and the linear memory get exported.
private static void writeExportSec(ptr<ld.Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, nint lenHostImports) {
    ref ld.Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionExport);

    writeUleb128(ctxt.Out, 4); // number of exports

    foreach (var (_, name) in new slice<@string>(new @string[] { "run", "resume", "getsp" })) {
        var s = ldr.Lookup("wasm_export_" + name, 0);
        var idx = uint32(lenHostImports) + uint32(ldr.SymValue(s) >> 16) - funcValueOffset;
        writeName(ctxt.Out, name); // inst.exports.run/resume/getsp in wasm_exec.js
        ctxt.Out.WriteByte(0x00); // func export
        writeUleb128(ctxt.Out, uint64(idx)); // funcidx
    }    writeName(ctxt.Out, "mem"); // inst.exports.mem in wasm_exec.js
    ctxt.Out.WriteByte(0x02); // mem export
    writeUleb128(ctxt.Out, 0); // memidx

    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeElementSec writes the section that initializes the tables declared by the "table" section.
// The table for CallIndirect gets initialized in a very simple way so that each table index (PC_F value)
// maps linearly to the function index (numImports + PC_F).
private static void writeElementSec(ptr<ld.Link> _addr_ctxt, ulong numImports, ulong numFns) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionElement);

    writeUleb128(ctxt.Out, 1); // number of element segments

    writeUleb128(ctxt.Out, 0); // tableidx
    writeI32Const(ctxt.Out, funcValueOffset);
    ctxt.Out.WriteByte(0x0b); // end

    writeUleb128(ctxt.Out, numFns); // number of entries
    for (var i = uint64(0); i < numFns; i++) {
        writeUleb128(ctxt.Out, numImports + i);
    }

    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeElementSec writes the section that provides the function bodies for the functions
// declared by the "func" section.
private static void writeCodeSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionCode);

    writeUleb128(ctxt.Out, uint64(len(fns))); // number of code entries
    foreach (var (_, fn) in fns) {
        writeUleb128(ctxt.Out, uint64(len(fn.Code)));
        ctxt.Out.Write(fn.Code);
    }    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeDataSec writes the section that provides data that will be used to initialize the linear memory.
private static void writeDataSec(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionData);

    private partial struct dataSegment {
        public int offset;
        public slice<byte> data;
    } 

    // Omit blocks of zeroes and instead emit data segments with offsets skipping the zeroes.
    // This reduces the size of the WebAssembly binary. We use 8 bytes as an estimate for the
    // overhead of adding a new segment (same as wasm-opt's memory-packing optimization uses).
    const nint segmentOverhead = 8; 

    // Generate at most this many segments. A higher number of segments gets rejected by some WebAssembly runtimes.
 

    // Generate at most this many segments. A higher number of segments gets rejected by some WebAssembly runtimes.
    const nint maxNumSegments = 100000;



    slice<ptr<dataSegment>> segments = default;
    foreach (var (secIndex, ds) in dataSects) {
        var data = ds.data;
        var offset = int32(ds.sect.Vaddr); 

        // skip leading zeroes
        while (len(data) > 0 && data[0] == 0) {
            data = data[(int)1..];
            offset++;
        }

        while (len(data) > 0) {
            var dataLen = int32(len(data));
            int segmentEnd = default;            int zeroEnd = default;

            if (len(segments) + (len(dataSects) - secIndex) == maxNumSegments) {
                segmentEnd = dataLen;
                zeroEnd = dataLen;
            }
            else
 {
                while (true) { 
                    // look for beginning of zeroes
                    while (segmentEnd < dataLen && data[segmentEnd] != 0) {
                        segmentEnd++;
                    } 
                    // look for end of zeroes
 
                    // look for end of zeroes
                    zeroEnd = segmentEnd;
                    while (zeroEnd < dataLen && data[zeroEnd] == 0) {
                        zeroEnd++;
                    } 
                    // emit segment if omitting zeroes reduces the output size
 
                    // emit segment if omitting zeroes reduces the output size
                    if (zeroEnd - segmentEnd >= segmentOverhead || zeroEnd == dataLen) {
                        break;
                    }

                    segmentEnd = zeroEnd;

                }


            }

            segments = append(segments, addr(new dataSegment(offset:offset,data:data[:segmentEnd],)));
            data = data[(int)zeroEnd..];
            offset += zeroEnd;

        }

    }    writeUleb128(ctxt.Out, uint64(len(segments))); // number of data entries
    foreach (var (_, seg) in segments) {
        writeUleb128(ctxt.Out, 0); // memidx
        writeI32Const(ctxt.Out, seg.offset);
        ctxt.Out.WriteByte(0x0b); // end
        writeUleb128(ctxt.Out, uint64(len(seg.data)));
        ctxt.Out.Write(seg.data);

    }    writeSecSize(_addr_ctxt, sizeOffset);

}

// writeProducerSec writes an optional section that reports the source language and compiler version.
private static void writeProducerSec(ptr<ld.Link> _addr_ctxt) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
    writeName(ctxt.Out, "producers");

    writeUleb128(ctxt.Out, 2); // number of fields

    writeName(ctxt.Out, "language"); // field name
    writeUleb128(ctxt.Out, 1); // number of values
    writeName(ctxt.Out, "Go"); // value: name
    writeName(ctxt.Out, buildcfg.Version); // value: version

    writeName(ctxt.Out, "processed-by"); // field name
    writeUleb128(ctxt.Out, 1); // number of values
    writeName(ctxt.Out, "Go cmd/compile"); // value: name
    writeName(ctxt.Out, buildcfg.Version); // value: version

    writeSecSize(_addr_ctxt, sizeOffset);

}

private static var nameRegexp = regexp.MustCompile("[^\\w\\.]");

// writeNameSec writes an optional section that assigns names to the functions declared by the "func" section.
// The names are only used by WebAssembly stack traces, debuggers and decompilers.
// TODO(neelance): add symbol table of DATA symbols
private static void writeNameSec(ptr<ld.Link> _addr_ctxt, nint firstFnIndex, slice<ptr<wasmFunc>> fns) {
    ref ld.Link ctxt = ref _addr_ctxt.val;

    var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
    writeName(ctxt.Out, "name");

    var sizeOffset2 = writeSecHeader(_addr_ctxt, 0x01); // function names
    writeUleb128(ctxt.Out, uint64(len(fns)));
    foreach (var (i, fn) in fns) {
        writeUleb128(ctxt.Out, uint64(firstFnIndex + i));
        writeName(ctxt.Out, fn.Name);
    }    writeSecSize(_addr_ctxt, sizeOffset2);

    writeSecSize(_addr_ctxt, sizeOffset);

}

private partial interface nameWriter {
}

private static void writeI32Const(io.ByteWriter w, int v) {
    w.WriteByte(0x41); // i32.const
    writeSleb128(w, int64(v));

}

private static void writeI64Const(io.ByteWriter w, long v) {
    w.WriteByte(0x42); // i64.const
    writeSleb128(w, v);

}

private static void writeName(nameWriter w, @string name) {
    writeUleb128(w, uint64(len(name)));
    w.Write((slice<byte>)name);
}

private static void writeUleb128(io.ByteWriter w, ulong v) {
    if (v < 128) {
        w.WriteByte(uint8(v));
        return ;
    }
    var more = true;
    while (more) {
        var c = uint8(v & 0x7f);
        v>>=7;
        more = v != 0;
        if (more) {
            c |= 0x80;
        }
        w.WriteByte(c);

    }

}

private static void writeUleb128FixedLength(io.ByteWriter w, ulong v, nint length) => func((_, panic, _) => {
    for (nint i = 0; i < length; i++) {
        var c = uint8(v & 0x7f);
        v>>=7;
        if (i < length - 1) {
            c |= 0x80;
        }
        w.WriteByte(c);

    }
    if (v != 0) {
        panic("writeUleb128FixedLength: length too small");
    }
});

private static void writeSleb128(io.ByteWriter w, long v) {
    var more = true;
    while (more) {
        var c = uint8(v & 0x7f);
        var s = uint8(v & 0x40);
        v>>=7;
        more = !((v == 0 && s == 0) || (v == -1 && s != 0));
        if (more) {
            c |= 0x80;
        }
        w.WriteByte(c);

    }

}

} // end wasm_package
