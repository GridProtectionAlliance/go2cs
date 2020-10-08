// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2020 October 08 04:40:25 UTC
// import "cmd/oldlink/internal/wasm" ==> using wasm = go.cmd.oldlink.@internal.wasm_package
// Original source: C:\Go\src\cmd\oldlink\internal\wasm\asm.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using ld = go.cmd.oldlink.@internal.ld_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using io = go.io_package;
using regexp = go.regexp_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class wasm_package
    {
        public static readonly ulong I32 = (ulong)0x7FUL;
        public static readonly ulong I64 = (ulong)0x7EUL;
        public static readonly ulong F32 = (ulong)0x7DUL;
        public static readonly ulong F64 = (ulong)0x7CUL;


        private static readonly long sectionCustom = (long)0L;
        private static readonly long sectionType = (long)1L;
        private static readonly long sectionImport = (long)2L;
        private static readonly long sectionFunction = (long)3L;
        private static readonly long sectionTable = (long)4L;
        private static readonly long sectionMemory = (long)5L;
        private static readonly long sectionGlobal = (long)6L;
        private static readonly long sectionExport = (long)7L;
        private static readonly long sectionStart = (long)8L;
        private static readonly long sectionElement = (long)9L;
        private static readonly long sectionCode = (long)10L;
        private static readonly long sectionData = (long)11L;


        // funcValueOffset is the offset between the PC_F value of a function and the index of the function in WebAssembly
        private static readonly ulong funcValueOffset = (ulong)0x1000UL; // TODO(neelance): make function addresses play nice with heap addresses

 // TODO(neelance): make function addresses play nice with heap addresses

        private static void gentext(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

        }

        private partial struct wasmFunc
        {
            public @string Name;
            public uint Type;
            public slice<byte> Code;
        }

        private partial struct wasmFuncType
        {
            public slice<byte> Params;
            public slice<byte> Results;
        }

        private static map wasmFuncTypes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<wasmFuncType>>{"_rt0_wasm_js":{Params:[]byte{}},"wasm_export_run":{Params:[]byte{I32,I32}},"wasm_export_resume":{Params:[]byte{}},"wasm_export_getsp":{Results:[]byte{I32}},"wasm_pc_f_loop":{Params:[]byte{}},"runtime.wasmMove":{Params:[]byte{I32,I32,I32}},"runtime.wasmZero":{Params:[]byte{I32,I32}},"runtime.wasmDiv":{Params:[]byte{I64,I64},Results:[]byte{I64}},"runtime.wasmTruncS":{Params:[]byte{F64},Results:[]byte{I64}},"runtime.wasmTruncU":{Params:[]byte{F64},Results:[]byte{I64}},"runtime.gcWriteBarrier":{Params:[]byte{I64,I64}},"cmpbody":{Params:[]byte{I64,I64,I64,I64},Results:[]byte{I64}},"memeqbody":{Params:[]byte{I64,I64,I64},Results:[]byte{I64}},"memcmp":{Params:[]byte{I32,I32,I32},Results:[]byte{I32}},"memchr":{Params:[]byte{I32,I32,I32},Results:[]byte{I32}},};

        private static (ptr<sym.Section>, long, ulong) assignAddress(ptr<ld.Link> _addr_ctxt, ptr<sym.Section> _addr_sect, long n, ptr<sym.Symbol> _addr_s, ulong va, bool isTramp)
        {
            ptr<sym.Section> _p0 = default!;
            long _p0 = default;
            ulong _p0 = default;
            ref ld.Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;
            ref sym.Symbol s = ref _addr_s.val;
 
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
            s.Sect = sect;
            s.Value = int64(funcValueOffset + va / ld.MINFUNC) << (int)(16L); // va starts at zero
            va += uint64(ld.MINFUNC);
            return (_addr_sect!, n, va);

        }

        private static void asmb(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

        } // dummy

        // asmb writes the final WebAssembly module binary.
        // Spec: https://webassembly.github.io/spec/core/binary/modules.html
        private static void asmb2(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            ref ptr<wasmFuncType> types = ref heap(new slice<ptr<wasmFuncType>>(new ptr<wasmFuncType>[] { {Params:[]byte{I32},Results:[]byte{I32}} }), out ptr<ptr<wasmFuncType>> _addr_types); 

            // collect host imports (functions that get imported from the WebAssembly host, usually JavaScript)
            ptr<wasmFunc> hostImports = new slice<ptr<wasmFunc>>(new ptr<wasmFunc>[] { {Name:"debug",Type:lookupType(&wasmFuncType{Params:[]byte{I32}},&types),} });
            var hostImportMap = make_map<ptr<sym.Symbol>, long>();
            {
                var fn__prev1 = fn;

                foreach (var (_, __fn) in ctxt.Textp)
                {
                    fn = __fn;
                    {
                        var r__prev2 = r;

                        foreach (var (_, __r) in fn.R)
                        {
                            r = __r;
                            if (r.Type == objabi.R_WASMIMPORT)
                            {
                                hostImportMap[r.Sym] = int64(len(hostImports));
                                hostImports = append(hostImports, addr(new wasmFunc(Name:r.Sym.Name,Type:lookupType(&wasmFuncType{Params:[]byte{I32}},&types),)));
                            }

                        }

                        r = r__prev2;
                    }
                } 

                // collect functions with WebAssembly body

                fn = fn__prev1;
            }

            slice<byte> buildid = default;
            var fns = make_slice<ptr<wasmFunc>>(len(ctxt.Textp));
            {
                var fn__prev1 = fn;

                foreach (var (__i, __fn) in ctxt.Textp)
                {
                    i = __i;
                    fn = __fn;
                    ptr<object> wfn = @new<bytes.Buffer>();
                    if (fn.Name == "go.buildid")
                    {
                        writeUleb128(wfn, 0L); // number of sets of locals
                        writeI32Const(wfn, 0L);
                        wfn.WriteByte(0x0bUL); // end
                        buildid = fn.P;

                    }
                    else
                    { 
                        // Relocations have variable length, handle them here.
                        var off = int32(0L);
                        {
                            var r__prev2 = r;

                            foreach (var (_, __r) in fn.R)
                            {
                                r = __r;
                                wfn.Write(fn.P[off..r.Off]);
                                off = r.Off;

                                if (r.Type == objabi.R_ADDR) 
                                    writeSleb128(wfn, r.Sym.Value + r.Add);
                                else if (r.Type == objabi.R_CALL) 
                                    writeSleb128(wfn, int64(len(hostImports)) + r.Sym.Value >> (int)(16L) - funcValueOffset);
                                else if (r.Type == objabi.R_WASMIMPORT) 
                                    writeSleb128(wfn, hostImportMap[r.Sym]);
                                else 
                                    ld.Errorf(fn, "bad reloc type %d (%s)", r.Type, sym.RelocName(ctxt.Arch, r.Type));
                                    continue;
                                
                            }

                            r = r__prev2;
                        }

                        wfn.Write(fn.P[off..]);

                    }

                    var typ = uint32(0L);
                    {
                        var (sig, ok) = wasmFuncTypes[fn.Name];

                        if (ok)
                        {
                            typ = lookupType(_addr_sig, _addr_types);
                        }

                    }


                    var name = nameRegexp.ReplaceAllString(fn.Name, "_");
                    fns[i] = addr(new wasmFunc(Name:name,Type:typ,Code:wfn.Bytes()));

                }

                fn = fn__prev1;
            }

            ctxt.Out.Write(new slice<byte>(new byte[] { 0x00, 0x61, 0x73, 0x6d })); // magic
            ctxt.Out.Write(new slice<byte>(new byte[] { 0x01, 0x00, 0x00, 0x00 })); // version

            // Add any buildid early in the binary:
            if (len(buildid) != 0L)
            {
                writeBuildID(_addr_ctxt, buildid);
            }

            writeTypeSec(_addr_ctxt, types);
            writeImportSec(_addr_ctxt, hostImports);
            writeFunctionSec(_addr_ctxt, fns);
            writeTableSec(_addr_ctxt, fns);
            writeMemorySec(_addr_ctxt);
            writeGlobalSec(_addr_ctxt);
            writeExportSec(_addr_ctxt, len(hostImports));
            writeElementSec(_addr_ctxt, uint64(len(hostImports)), uint64(len(fns)));
            writeCodeSec(_addr_ctxt, fns);
            writeDataSec(_addr_ctxt);
            writeProducerSec(_addr_ctxt);
            if (!ld.FlagS.val)
            {
                writeNameSec(_addr_ctxt, len(hostImports), fns);
            }

            ctxt.Out.Flush();

        }

        private static uint lookupType(ptr<wasmFuncType> _addr_sig, ptr<slice<ptr<wasmFuncType>>> _addr_types)
        {
            ref wasmFuncType sig = ref _addr_sig.val;
            ref slice<ptr<wasmFuncType>> types = ref _addr_types.val;

            foreach (var (i, t) in types.val)
            {
                if (bytes.Equal(sig.Params, t.Params) && bytes.Equal(sig.Results, t.Results))
                {
                    return uint32(i);
                }

            }
            types.val = append(types.val, sig);
            return uint32(len(types.val) - 1L);

        }

        private static long writeSecHeader(ptr<ld.Link> _addr_ctxt, byte id)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            ctxt.Out.WriteByte(id);
            var sizeOffset = ctxt.Out.Offset();
            ctxt.Out.Write(make_slice<byte>(5L)); // placeholder for length
            return sizeOffset;

        }

        private static void writeSecSize(ptr<ld.Link> _addr_ctxt, long sizeOffset)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var endOffset = ctxt.Out.Offset();
            ctxt.Out.SeekSet(sizeOffset);
            writeUleb128FixedLength(ctxt.Out, uint64(endOffset - sizeOffset - 5L), 5L);
            ctxt.Out.SeekSet(endOffset);
        }

        private static void writeBuildID(ptr<ld.Link> _addr_ctxt, slice<byte> buildid)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
            writeName(ctxt.Out, "go.buildid");
            ctxt.Out.Write(buildid);
            writeSecSize(_addr_ctxt, sizeOffset);
        }

        // writeTypeSec writes the section that declares all function types
        // so they can be referenced by index.
        private static void writeTypeSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFuncType>> types)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionType);

            writeUleb128(ctxt.Out, uint64(len(types)));

            foreach (var (_, t) in types)
            {
                ctxt.Out.WriteByte(0x60UL); // functype
                writeUleb128(ctxt.Out, uint64(len(t.Params)));
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in t.Params)
                    {
                        v = __v;
                        ctxt.Out.WriteByte(byte(v));
                    }

                    v = v__prev2;
                }

                writeUleb128(ctxt.Out, uint64(len(t.Results)));
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in t.Results)
                    {
                        v = __v;
                        ctxt.Out.WriteByte(byte(v));
                    }

                    v = v__prev2;
                }
            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeImportSec writes the section that lists the functions that get
        // imported from the WebAssembly host, usually JavaScript.
        private static void writeImportSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> hostImports)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionImport);

            writeUleb128(ctxt.Out, uint64(len(hostImports))); // number of imports
            foreach (var (_, fn) in hostImports)
            {
                writeName(ctxt.Out, "go"); // provided by the import object in wasm_exec.js
                writeName(ctxt.Out, fn.Name);
                ctxt.Out.WriteByte(0x00UL); // func import
                writeUleb128(ctxt.Out, uint64(fn.Type));

            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeFunctionSec writes the section that declares the types of functions.
        // The bodies of these functions will later be provided in the "code" section.
        private static void writeFunctionSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionFunction);

            writeUleb128(ctxt.Out, uint64(len(fns)));
            foreach (var (_, fn) in fns)
            {
                writeUleb128(ctxt.Out, uint64(fn.Type));
            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeTableSec writes the section that declares tables. Currently there is only a single table
        // that is used by the CallIndirect operation to dynamically call any function.
        // The contents of the table get initialized by the "element" section.
        private static void writeTableSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionTable);

            var numElements = uint64(funcValueOffset + len(fns));
            writeUleb128(ctxt.Out, 1L); // number of tables
            ctxt.Out.WriteByte(0x70UL); // type: anyfunc
            ctxt.Out.WriteByte(0x00UL); // no max
            writeUleb128(ctxt.Out, numElements); // min

            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeMemorySec writes the section that declares linear memories. Currently one linear memory is being used.
        // Linear memory always starts at address zero. More memory can be requested with the GrowMemory instruction.
        private static void writeMemorySec(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionMemory);

            var dataSection = ctxt.Syms.Lookup("runtime.data", 0L).Sect;
            var dataEnd = dataSection.Vaddr + dataSection.Length;
            var initialSize = dataEnd + 16L << (int)(20L); // 16MB, enough for runtime init without growing

            const long wasmPageSize = (long)64L << (int)(10L); // 64KB

 // 64KB

            writeUleb128(ctxt.Out, 1L); // number of memories
            ctxt.Out.WriteByte(0x00UL); // no maximum memory size
            writeUleb128(ctxt.Out, initialSize / wasmPageSize); // minimum (initial) memory size

            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeGlobalSec writes the section that declares global variables.
        private static void writeGlobalSec(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionGlobal);

            byte globalRegs = new slice<byte>(new byte[] { I32, I64, I64, I64, I64, I64, I64, I32 });

            writeUleb128(ctxt.Out, uint64(len(globalRegs))); // number of globals

            foreach (var (_, typ) in globalRegs)
            {
                ctxt.Out.WriteByte(typ);
                ctxt.Out.WriteByte(0x01UL); // var

                if (typ == I32) 
                    writeI32Const(ctxt.Out, 0L);
                else if (typ == I64) 
                    writeI64Const(ctxt.Out, 0L);
                                ctxt.Out.WriteByte(0x0bUL); // end
            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeExportSec writes the section that declares exports.
        // Exports can be accessed by the WebAssembly host, usually JavaScript.
        // The wasm_export_* functions and the linear memory get exported.
        private static void writeExportSec(ptr<ld.Link> _addr_ctxt, long lenHostImports)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionExport);

            writeUleb128(ctxt.Out, 4L); // number of exports

            foreach (var (_, name) in new slice<@string>(new @string[] { "run", "resume", "getsp" }))
            {
                var idx = uint32(lenHostImports) + uint32(ctxt.Syms.ROLookup("wasm_export_" + name, 0L).Value >> (int)(16L)) - funcValueOffset;
                writeName(ctxt.Out, name); // inst.exports.run/resume/getsp in wasm_exec.js
                ctxt.Out.WriteByte(0x00UL); // func export
                writeUleb128(ctxt.Out, uint64(idx)); // funcidx
            }
            writeName(ctxt.Out, "mem"); // inst.exports.mem in wasm_exec.js
            ctxt.Out.WriteByte(0x02UL); // mem export
            writeUleb128(ctxt.Out, 0L); // memidx

            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeElementSec writes the section that initializes the tables declared by the "table" section.
        // The table for CallIndirect gets initialized in a very simple way so that each table index (PC_F value)
        // maps linearly to the function index (numImports + PC_F).
        private static void writeElementSec(ptr<ld.Link> _addr_ctxt, ulong numImports, ulong numFns)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionElement);

            writeUleb128(ctxt.Out, 1L); // number of element segments

            writeUleb128(ctxt.Out, 0L); // tableidx
            writeI32Const(ctxt.Out, funcValueOffset);
            ctxt.Out.WriteByte(0x0bUL); // end

            writeUleb128(ctxt.Out, numFns); // number of entries
            for (var i = uint64(0L); i < numFns; i++)
            {
                writeUleb128(ctxt.Out, numImports + i);
            }


            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeElementSec writes the section that provides the function bodies for the functions
        // declared by the "func" section.
        private static void writeCodeSec(ptr<ld.Link> _addr_ctxt, slice<ptr<wasmFunc>> fns)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionCode);

            writeUleb128(ctxt.Out, uint64(len(fns))); // number of code entries
            foreach (var (_, fn) in fns)
            {
                writeUleb128(ctxt.Out, uint64(len(fn.Code)));
                ctxt.Out.Write(fn.Code);
            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeDataSec writes the section that provides data that will be used to initialize the linear memory.
        private static void writeDataSec(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionData);

            ptr<sym.Section> sections = new slice<ptr<sym.Section>>(new ptr<sym.Section>[] { ctxt.Syms.Lookup("runtime.rodata",0).Sect, ctxt.Syms.Lookup("runtime.typelink",0).Sect, ctxt.Syms.Lookup("runtime.itablink",0).Sect, ctxt.Syms.Lookup("runtime.symtab",0).Sect, ctxt.Syms.Lookup("runtime.pclntab",0).Sect, ctxt.Syms.Lookup("runtime.noptrdata",0).Sect, ctxt.Syms.Lookup("runtime.data",0).Sect });

            private partial struct dataSegment
            {
                public int offset;
                public slice<byte> data;
            } 

            // Omit blocks of zeroes and instead emit data segments with offsets skipping the zeroes.
            // This reduces the size of the WebAssembly binary. We use 8 bytes as an estimate for the
            // overhead of adding a new segment (same as wasm-opt's memory-packing optimization uses).
            const long segmentOverhead = (long)8L; 

            // Generate at most this many segments. A higher number of segments gets rejected by some WebAssembly runtimes.
 

            // Generate at most this many segments. A higher number of segments gets rejected by some WebAssembly runtimes.
            const long maxNumSegments = (long)100000L;



            slice<ptr<dataSegment>> segments = default;
            foreach (var (secIndex, sec) in sections)
            {
                var data = ld.DatblkBytes(ctxt, int64(sec.Vaddr), int64(sec.Length));
                var offset = int32(sec.Vaddr); 

                // skip leading zeroes
                while (len(data) > 0L && data[0L] == 0L)
                {
                    data = data[1L..];
                    offset++;
                }


                while (len(data) > 0L)
                {
                    var dataLen = int32(len(data));
                    int segmentEnd = default;                    int zeroEnd = default;

                    if (len(segments) + (len(sections) - secIndex) == maxNumSegments)
                    {
                        segmentEnd = dataLen;
                        zeroEnd = dataLen;
                    }
                    else
                    {
                        while (true)
                        { 
                            // look for beginning of zeroes
                            while (segmentEnd < dataLen && data[segmentEnd] != 0L)
                            {
                                segmentEnd++;
                            } 
                            // look for end of zeroes
 
                            // look for end of zeroes
                            zeroEnd = segmentEnd;
                            while (zeroEnd < dataLen && data[zeroEnd] == 0L)
                            {
                                zeroEnd++;
                            } 
                            // emit segment if omitting zeroes reduces the output size
 
                            // emit segment if omitting zeroes reduces the output size
                            if (zeroEnd - segmentEnd >= segmentOverhead || zeroEnd == dataLen)
                            {
                                break;
                            }

                            segmentEnd = zeroEnd;

                        }


                    }

                    segments = append(segments, addr(new dataSegment(offset:offset,data:data[:segmentEnd],)));
                    data = data[zeroEnd..];
                    offset += zeroEnd;

                }


            }
            writeUleb128(ctxt.Out, uint64(len(segments))); // number of data entries
            foreach (var (_, seg) in segments)
            {
                writeUleb128(ctxt.Out, 0L); // memidx
                writeI32Const(ctxt.Out, seg.offset);
                ctxt.Out.WriteByte(0x0bUL); // end
                writeUleb128(ctxt.Out, uint64(len(seg.data)));
                ctxt.Out.Write(seg.data);

            }
            writeSecSize(_addr_ctxt, sizeOffset);

        }

        // writeProducerSec writes an optional section that reports the source language and compiler version.
        private static void writeProducerSec(ptr<ld.Link> _addr_ctxt)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
            writeName(ctxt.Out, "producers");

            writeUleb128(ctxt.Out, 2L); // number of fields

            writeName(ctxt.Out, "language"); // field name
            writeUleb128(ctxt.Out, 1L); // number of values
            writeName(ctxt.Out, "Go"); // value: name
            writeName(ctxt.Out, objabi.Version); // value: version

            writeName(ctxt.Out, "processed-by"); // field name
            writeUleb128(ctxt.Out, 1L); // number of values
            writeName(ctxt.Out, "Go cmd/compile"); // value: name
            writeName(ctxt.Out, objabi.Version); // value: version

            writeSecSize(_addr_ctxt, sizeOffset);

        }

        private static var nameRegexp = regexp.MustCompile("[^\\w\\.]");

        // writeNameSec writes an optional section that assigns names to the functions declared by the "func" section.
        // The names are only used by WebAssembly stack traces, debuggers and decompilers.
        // TODO(neelance): add symbol table of DATA symbols
        private static void writeNameSec(ptr<ld.Link> _addr_ctxt, long firstFnIndex, slice<ptr<wasmFunc>> fns)
        {
            ref ld.Link ctxt = ref _addr_ctxt.val;

            var sizeOffset = writeSecHeader(_addr_ctxt, sectionCustom);
            writeName(ctxt.Out, "name");

            var sizeOffset2 = writeSecHeader(_addr_ctxt, 0x01UL); // function names
            writeUleb128(ctxt.Out, uint64(len(fns)));
            foreach (var (i, fn) in fns)
            {
                writeUleb128(ctxt.Out, uint64(firstFnIndex + i));
                writeName(ctxt.Out, fn.Name);
            }
            writeSecSize(_addr_ctxt, sizeOffset2);

            writeSecSize(_addr_ctxt, sizeOffset);

        }

        private partial interface nameWriter : io.ByteWriter, io.Writer
        {
        }

        private static void writeI32Const(io.ByteWriter w, int v)
        {
            w.WriteByte(0x41UL); // i32.const
            writeSleb128(w, int64(v));

        }

        private static void writeI64Const(io.ByteWriter w, long v)
        {
            w.WriteByte(0x42UL); // i64.const
            writeSleb128(w, v);

        }

        private static void writeName(nameWriter w, @string name)
        {
            writeUleb128(w, uint64(len(name)));
            w.Write((slice<byte>)name);
        }

        private static void writeUleb128(io.ByteWriter w, ulong v)
        {
            if (v < 128L)
            {
                w.WriteByte(uint8(v));
                return ;
            }

            var more = true;
            while (more)
            {
                var c = uint8(v & 0x7fUL);
                v >>= 7L;
                more = v != 0L;
                if (more)
                {
                    c |= 0x80UL;
                }

                w.WriteByte(c);

            }


        }

        private static void writeUleb128FixedLength(io.ByteWriter w, ulong v, long length) => func((_, panic, __) =>
        {
            for (long i = 0L; i < length; i++)
            {
                var c = uint8(v & 0x7fUL);
                v >>= 7L;
                if (i < length - 1L)
                {
                    c |= 0x80UL;
                }

                w.WriteByte(c);

            }

            if (v != 0L)
            {
                panic("writeUleb128FixedLength: length too small");
            }

        });

        private static void writeSleb128(io.ByteWriter w, long v)
        {
            var more = true;
            while (more)
            {
                var c = uint8(v & 0x7fUL);
                var s = uint8(v & 0x40UL);
                v >>= 7L;
                more = !((v == 0L && s == 0L) || (v == -1L && s != 0L));
                if (more)
                {
                    c |= 0x80UL;
                }

                w.WriteByte(c);

            }


        }
    }
}}}}
