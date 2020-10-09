// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:43:11 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\ssa.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using html = go.html_package;
using os = go.os_package;
using sort = go.sort_package;

using bufio = go.bufio_package;
using bytes = go.bytes_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ptr<ssa.Config> ssaConfig;
        private static slice<ssa.Cache> ssaCaches = default;

        private static @string ssaDump = default; // early copy of $GOSSAFUNC; the func name to dump output for
        private static bool ssaDumpStdout = default; // whether to dump to stdout
        private static @string ssaDumpCFG = default; // generate CFGs for these phases
        private static readonly @string ssaDumpFile = (@string)"ssa.html";

        // The max number of defers in a function using open-coded defers. We enforce this
        // limit because the deferBits bitmask is currently a single byte (to minimize code size)


        // The max number of defers in a function using open-coded defers. We enforce this
        // limit because the deferBits bitmask is currently a single byte (to minimize code size)
        private static readonly long maxOpenDefers = (long)8L;

        // ssaDumpInlined holds all inlined functions when ssaDump contains a function name.


        // ssaDumpInlined holds all inlined functions when ssaDump contains a function name.
        private static slice<ptr<Node>> ssaDumpInlined = default;

        private static void initssaconfig()
        {
            var types_ = ssa.NewTypes();

            if (thearch.SoftFloat)
            {
                softfloatInit();
            } 

            // Generate a few pointer types that are uncommon in the frontend but common in the backend.
            // Caching is disabled in the backend, so generating these here avoids allocations.
            _ = types.NewPtr(types.Types[TINTER]); // *interface{}
            _ = types.NewPtr(types.NewPtr(types.Types[TSTRING])); // **string
            _ = types.NewPtr(types.NewPtr(types.Idealstring)); // **string
            _ = types.NewPtr(types.NewSlice(types.Types[TINTER])); // *[]interface{}
            _ = types.NewPtr(types.NewPtr(types.Bytetype)); // **byte
            _ = types.NewPtr(types.NewSlice(types.Bytetype)); // *[]byte
            _ = types.NewPtr(types.NewSlice(types.Types[TSTRING])); // *[]string
            _ = types.NewPtr(types.NewSlice(types.Idealstring)); // *[]string
            _ = types.NewPtr(types.NewPtr(types.NewPtr(types.Types[TUINT8]))); // ***uint8
            _ = types.NewPtr(types.Types[TINT16]); // *int16
            _ = types.NewPtr(types.Types[TINT64]); // *int64
            _ = types.NewPtr(types.Errortype); // *error
            types.NewPtrCacheEnabled = false;
            ssaConfig = ssa.NewConfig(thearch.LinkArch.Name, types_.val, Ctxt, Debug['N'] == 0L);
            if (thearch.LinkArch.Name == "386")
            {
                ssaConfig.Set387(thearch.Use387);
            }

            ssaConfig.SoftFloat = thearch.SoftFloat;
            ssaConfig.Race = flag_race;
            ssaCaches = make_slice<ssa.Cache>(nBackendWorkers); 

            // Set up some runtime functions we'll need to call.
            assertE2I = sysfunc("assertE2I");
            assertE2I2 = sysfunc("assertE2I2");
            assertI2I = sysfunc("assertI2I");
            assertI2I2 = sysfunc("assertI2I2");
            deferproc = sysfunc("deferproc");
            deferprocStack = sysfunc("deferprocStack");
            Deferreturn = sysfunc("deferreturn");
            Duffcopy = sysvar("duffcopy"); // asm func with special ABI
            Duffzero = sysvar("duffzero"); // asm func with special ABI
            gcWriteBarrier = sysvar("gcWriteBarrier"); // asm func with special ABI
            goschedguarded = sysfunc("goschedguarded");
            growslice = sysfunc("growslice");
            msanread = sysfunc("msanread");
            msanwrite = sysfunc("msanwrite");
            newobject = sysfunc("newobject");
            newproc = sysfunc("newproc");
            panicdivide = sysfunc("panicdivide");
            panicdottypeE = sysfunc("panicdottypeE");
            panicdottypeI = sysfunc("panicdottypeI");
            panicnildottype = sysfunc("panicnildottype");
            panicoverflow = sysfunc("panicoverflow");
            panicshift = sysfunc("panicshift");
            raceread = sysfunc("raceread");
            racereadrange = sysfunc("racereadrange");
            racewrite = sysfunc("racewrite");
            racewriterange = sysfunc("racewriterange");
            x86HasPOPCNT = sysvar("x86HasPOPCNT"); // bool
            x86HasSSE41 = sysvar("x86HasSSE41"); // bool
            x86HasFMA = sysvar("x86HasFMA"); // bool
            armHasVFPv4 = sysvar("armHasVFPv4"); // bool
            arm64HasATOMICS = sysvar("arm64HasATOMICS"); // bool
            typedmemclr = sysfunc("typedmemclr");
            typedmemmove = sysfunc("typedmemmove");
            Udiv = sysvar("udiv"); // asm func with special ABI
            writeBarrier = sysvar("writeBarrier"); // struct { bool; ... }
            zerobaseSym = sysvar("zerobase"); 

            // asm funcs with special ABI
            if (thearch.LinkArch.Name == "amd64")
            {
                GCWriteBarrierReg = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<short, ptr<obj.LSym>>{x86.REG_AX:sysvar("gcWriteBarrier"),x86.REG_CX:sysvar("gcWriteBarrierCX"),x86.REG_DX:sysvar("gcWriteBarrierDX"),x86.REG_BX:sysvar("gcWriteBarrierBX"),x86.REG_BP:sysvar("gcWriteBarrierBP"),x86.REG_SI:sysvar("gcWriteBarrierSI"),x86.REG_R8:sysvar("gcWriteBarrierR8"),x86.REG_R9:sysvar("gcWriteBarrierR9"),};
            }

            if (thearch.LinkArch.Family == sys.Wasm)
            {
                BoundsCheckFunc[ssa.BoundsIndex] = sysvar("goPanicIndex");
                BoundsCheckFunc[ssa.BoundsIndexU] = sysvar("goPanicIndexU");
                BoundsCheckFunc[ssa.BoundsSliceAlen] = sysvar("goPanicSliceAlen");
                BoundsCheckFunc[ssa.BoundsSliceAlenU] = sysvar("goPanicSliceAlenU");
                BoundsCheckFunc[ssa.BoundsSliceAcap] = sysvar("goPanicSliceAcap");
                BoundsCheckFunc[ssa.BoundsSliceAcapU] = sysvar("goPanicSliceAcapU");
                BoundsCheckFunc[ssa.BoundsSliceB] = sysvar("goPanicSliceB");
                BoundsCheckFunc[ssa.BoundsSliceBU] = sysvar("goPanicSliceBU");
                BoundsCheckFunc[ssa.BoundsSlice3Alen] = sysvar("goPanicSlice3Alen");
                BoundsCheckFunc[ssa.BoundsSlice3AlenU] = sysvar("goPanicSlice3AlenU");
                BoundsCheckFunc[ssa.BoundsSlice3Acap] = sysvar("goPanicSlice3Acap");
                BoundsCheckFunc[ssa.BoundsSlice3AcapU] = sysvar("goPanicSlice3AcapU");
                BoundsCheckFunc[ssa.BoundsSlice3B] = sysvar("goPanicSlice3B");
                BoundsCheckFunc[ssa.BoundsSlice3BU] = sysvar("goPanicSlice3BU");
                BoundsCheckFunc[ssa.BoundsSlice3C] = sysvar("goPanicSlice3C");
                BoundsCheckFunc[ssa.BoundsSlice3CU] = sysvar("goPanicSlice3CU");
            }
            else
            {
                BoundsCheckFunc[ssa.BoundsIndex] = sysvar("panicIndex");
                BoundsCheckFunc[ssa.BoundsIndexU] = sysvar("panicIndexU");
                BoundsCheckFunc[ssa.BoundsSliceAlen] = sysvar("panicSliceAlen");
                BoundsCheckFunc[ssa.BoundsSliceAlenU] = sysvar("panicSliceAlenU");
                BoundsCheckFunc[ssa.BoundsSliceAcap] = sysvar("panicSliceAcap");
                BoundsCheckFunc[ssa.BoundsSliceAcapU] = sysvar("panicSliceAcapU");
                BoundsCheckFunc[ssa.BoundsSliceB] = sysvar("panicSliceB");
                BoundsCheckFunc[ssa.BoundsSliceBU] = sysvar("panicSliceBU");
                BoundsCheckFunc[ssa.BoundsSlice3Alen] = sysvar("panicSlice3Alen");
                BoundsCheckFunc[ssa.BoundsSlice3AlenU] = sysvar("panicSlice3AlenU");
                BoundsCheckFunc[ssa.BoundsSlice3Acap] = sysvar("panicSlice3Acap");
                BoundsCheckFunc[ssa.BoundsSlice3AcapU] = sysvar("panicSlice3AcapU");
                BoundsCheckFunc[ssa.BoundsSlice3B] = sysvar("panicSlice3B");
                BoundsCheckFunc[ssa.BoundsSlice3BU] = sysvar("panicSlice3BU");
                BoundsCheckFunc[ssa.BoundsSlice3C] = sysvar("panicSlice3C");
                BoundsCheckFunc[ssa.BoundsSlice3CU] = sysvar("panicSlice3CU");
            }

            if (thearch.LinkArch.PtrSize == 4L)
            {
                ExtendCheckFunc[ssa.BoundsIndex] = sysvar("panicExtendIndex");
                ExtendCheckFunc[ssa.BoundsIndexU] = sysvar("panicExtendIndexU");
                ExtendCheckFunc[ssa.BoundsSliceAlen] = sysvar("panicExtendSliceAlen");
                ExtendCheckFunc[ssa.BoundsSliceAlenU] = sysvar("panicExtendSliceAlenU");
                ExtendCheckFunc[ssa.BoundsSliceAcap] = sysvar("panicExtendSliceAcap");
                ExtendCheckFunc[ssa.BoundsSliceAcapU] = sysvar("panicExtendSliceAcapU");
                ExtendCheckFunc[ssa.BoundsSliceB] = sysvar("panicExtendSliceB");
                ExtendCheckFunc[ssa.BoundsSliceBU] = sysvar("panicExtendSliceBU");
                ExtendCheckFunc[ssa.BoundsSlice3Alen] = sysvar("panicExtendSlice3Alen");
                ExtendCheckFunc[ssa.BoundsSlice3AlenU] = sysvar("panicExtendSlice3AlenU");
                ExtendCheckFunc[ssa.BoundsSlice3Acap] = sysvar("panicExtendSlice3Acap");
                ExtendCheckFunc[ssa.BoundsSlice3AcapU] = sysvar("panicExtendSlice3AcapU");
                ExtendCheckFunc[ssa.BoundsSlice3B] = sysvar("panicExtendSlice3B");
                ExtendCheckFunc[ssa.BoundsSlice3BU] = sysvar("panicExtendSlice3BU");
                ExtendCheckFunc[ssa.BoundsSlice3C] = sysvar("panicExtendSlice3C");
                ExtendCheckFunc[ssa.BoundsSlice3CU] = sysvar("panicExtendSlice3CU");
            } 

            // GO386=387 runtime definitions
            ControlWord64trunc = sysvar("controlWord64trunc"); // uint16
            ControlWord32 = sysvar("controlWord32"); // uint16

            // Wasm (all asm funcs with special ABIs)
            WasmMove = sysvar("wasmMove");
            WasmZero = sysvar("wasmZero");
            WasmDiv = sysvar("wasmDiv");
            WasmTruncS = sysvar("wasmTruncS");
            WasmTruncU = sysvar("wasmTruncU");
            SigPanic = sysfunc("sigpanic");

        }

        // getParam returns the Field of ith param of node n (which is a
        // function/method/interface call), where the receiver of a method call is
        // considered as the 0th parameter. This does not include the receiver of an
        // interface call.
        private static ptr<types.Field> getParam(ptr<Node> _addr_n, long i)
        {
            ref Node n = ref _addr_n.val;

            var t = n.Left.Type;
            if (n.Op == OCALLMETH)
            {
                if (i == 0L)
                {
                    return _addr_t.Recv()!;
                }

                return _addr_t.Params().Field(i - 1L)!;

            }

            return _addr_t.Params().Field(i)!;

        }

        // dvarint writes a varint v to the funcdata in symbol x and returns the new offset
        private static long dvarint(ptr<obj.LSym> _addr_x, long off, long v) => func((_, panic, __) =>
        {
            ref obj.LSym x = ref _addr_x.val;

            if (v < 0L || v > 1e9F)
            {
                panic(fmt.Sprintf("dvarint: bad offset for funcdata - %v", v));
            }

            if (v < 1L << (int)(7L))
            {
                return duint8(x, off, uint8(v));
            }

            off = duint8(x, off, uint8((v & 127L) | 128L));
            if (v < 1L << (int)(14L))
            {
                return duint8(x, off, uint8(v >> (int)(7L)));
            }

            off = duint8(x, off, uint8(((v >> (int)(7L)) & 127L) | 128L));
            if (v < 1L << (int)(21L))
            {
                return duint8(x, off, uint8(v >> (int)(14L)));
            }

            off = duint8(x, off, uint8(((v >> (int)(14L)) & 127L) | 128L));
            if (v < 1L << (int)(28L))
            {
                return duint8(x, off, uint8(v >> (int)(21L)));
            }

            off = duint8(x, off, uint8(((v >> (int)(21L)) & 127L) | 128L));
            return duint8(x, off, uint8(v >> (int)(28L)));

        });

        // emitOpenDeferInfo emits FUNCDATA information about the defers in a function
        // that is using open-coded defers.  This funcdata is used to determine the active
        // defers in a function and execute those defers during panic processing.
        //
        // The funcdata is all encoded in varints (since values will almost always be less than
        // 128, but stack offsets could potentially be up to 2Gbyte). All "locations" (offsets)
        // for stack variables are specified as the number of bytes below varp (pointer to the
        // top of the local variables) for their starting address. The format is:
        //
        //  - Max total argument size among all the defers
        //  - Offset of the deferBits variable
        //  - Number of defers in the function
        //  - Information about each defer call, in reverse order of appearance in the function:
        //    - Total argument size of the call
        //    - Offset of the closure value to call
        //    - Number of arguments (including interface receiver or method receiver as first arg)
        //    - Information about each argument
        //      - Offset of the stored defer argument in this function's frame
        //      - Size of the argument
        //      - Offset of where argument should be placed in the args frame when making call
        private static void emitOpenDeferInfo(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            var x = Ctxt.Lookup(s.curfn.Func.lsym.Name + ".opendefer");
            s.curfn.Func.lsym.Func.OpenCodedDeferInfo = x;
            long off = 0L; 

            // Compute maxargsize (max size of arguments for all defers)
            // first, so we can output it first to the funcdata
            long maxargsize = default;
            {
                var i__prev1 = i;

                for (var i = len(s.openDefers) - 1L; i >= 0L; i--)
                {
                    var r = s.openDefers[i];
                    var argsize = r.n.Left.Type.ArgWidth();
                    if (argsize > maxargsize)
                    {
                        maxargsize = argsize;
                    }

                }


                i = i__prev1;
            }
            off = dvarint(_addr_x, off, maxargsize);
            off = dvarint(_addr_x, off, -s.deferBitsTemp.Xoffset);
            off = dvarint(_addr_x, off, int64(len(s.openDefers))); 

            // Write in reverse-order, for ease of running in that order at runtime
            {
                var i__prev1 = i;

                for (i = len(s.openDefers) - 1L; i >= 0L; i--)
                {
                    r = s.openDefers[i];
                    off = dvarint(_addr_x, off, r.n.Left.Type.ArgWidth());
                    off = dvarint(_addr_x, off, -r.closureNode.Xoffset);
                    var numArgs = len(r.argNodes);
                    if (r.rcvrNode != null)
                    { 
                        // If there's an interface receiver, treat/place it as the first
                        // arg. (If there is a method receiver, it's already included as
                        // first arg in r.argNodes.)
                        numArgs++;

                    }

                    off = dvarint(_addr_x, off, int64(numArgs));
                    if (r.rcvrNode != null)
                    {
                        off = dvarint(_addr_x, off, -r.rcvrNode.Xoffset);
                        off = dvarint(_addr_x, off, s.config.PtrSize);
                        off = dvarint(_addr_x, off, 0L);
                    }

                    foreach (var (j, arg) in r.argNodes)
                    {
                        var f = getParam(_addr_r.n, j);
                        off = dvarint(_addr_x, off, -arg.Xoffset);
                        off = dvarint(_addr_x, off, f.Type.Size());
                        off = dvarint(_addr_x, off, f.Offset);
                    }

                }


                i = i__prev1;
            }

        }

        // buildssa builds an SSA function for fn.
        // worker indicates which of the backend workers is doing the processing.
        private static ptr<ssa.Func> buildssa(ptr<Node> _addr_fn, long worker) => func((defer, _, __) =>
        {
            ref Node fn = ref _addr_fn.val;

            var name = fn.funcname();
            var printssa = name == ssaDump;
            ptr<bytes.Buffer> astBuf;
            if (printssa)
            {
                astBuf = addr(new bytes.Buffer());
                fdumplist(astBuf, "buildssa-enter", fn.Func.Enter);
                fdumplist(astBuf, "buildssa-body", fn.Nbody);
                fdumplist(astBuf, "buildssa-exit", fn.Func.Exit);
                if (ssaDumpStdout)
                {
                    fmt.Println("generating SSA for", name);
                    fmt.Print(astBuf.String());
                }

            }

            state s = default;
            s.pushLine(fn.Pos);
            defer(s.popLine());

            s.hasdefer = fn.Func.HasDefer();
            if (fn.Func.Pragma & CgoUnsafeArgs != 0L)
            {
                s.cgoUnsafeArgs = true;
            }

            ref ssafn fe = ref heap(new ssafn(curfn:fn,log:printssa&&ssaDumpStdout,), out ptr<ssafn> _addr_fe);
            s.curfn = fn;

            s.f = ssa.NewFunc(_addr_fe);
            s.config = ssaConfig;
            s.f.Type = fn.Type;
            s.f.Config = ssaConfig;
            s.f.Cache = _addr_ssaCaches[worker];
            s.f.Cache.Reset();
            s.f.DebugTest = s.f.DebugHashMatch("GOSSAHASH", name);
            s.f.Name = name;
            s.f.PrintOrHtmlSSA = printssa;
            if (fn.Func.Pragma & Nosplit != 0L)
            {
                s.f.NoSplit = true;
            }

            s.panics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<funcLine, ptr<ssa.Block>>{};
            s.softFloat = s.config.SoftFloat;

            if (printssa)
            {
                s.f.HTMLWriter = ssa.NewHTMLWriter(ssaDumpFile, s.f, ssaDumpCFG); 
                // TODO: generate and print a mapping from nodes to values and blocks
                dumpSourcesColumn(_addr_s.f.HTMLWriter, _addr_fn);
                s.f.HTMLWriter.WriteAST("AST", astBuf);

            } 

            // Allocate starting block
            s.f.Entry = s.f.NewBlock(ssa.BlockPlain); 

            // Allocate starting values
            s.labels = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<ssaLabel>>{};
            s.labeledNodes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<ssaLabel>>{};
            s.fwdVars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<ssa.Value>>{};
            s.startmem = s.entryNewValue0(ssa.OpInitMem, types.TypeMem);

            s.hasOpenDefers = Debug['N'] == 0L && s.hasdefer && !s.curfn.Func.OpenCodedDeferDisallowed();

            if (s.hasOpenDefers && (Ctxt.Flag_shared || Ctxt.Flag_dynlink) && thearch.LinkArch.Name == "386") 
                // Don't support open-coded defers for 386 ONLY when using shared
                // libraries, because there is extra code (added by rewriteToUseGot())
                // preceding the deferreturn/ret code that is generated by gencallret()
                // that we don't track correctly.
                s.hasOpenDefers = false;
                        if (s.hasOpenDefers && s.curfn.Func.Exit.Len() > 0L)
            { 
                // Skip doing open defers if there is any extra exit code (likely
                // copying heap-allocated return values or race detection), since
                // we will not generate that code in the case of the extra
                // deferreturn/ret segment.
                s.hasOpenDefers = false;

            }

            if (s.hasOpenDefers && s.curfn.Func.numReturns * s.curfn.Func.numDefers > 15L)
            { 
                // Since we are generating defer calls at every exit for
                // open-coded defers, skip doing open-coded defers if there are
                // too many returns (especially if there are multiple defers).
                // Open-coded defers are most important for improving performance
                // for smaller functions (which don't have many returns).
                s.hasOpenDefers = false;

            }

            s.sp = s.entryNewValue0(ssa.OpSP, types.Types[TUINTPTR]); // TODO: use generic pointer type (unsafe.Pointer?) instead
            s.sb = s.entryNewValue0(ssa.OpSB, types.Types[TUINTPTR]);

            s.startBlock(s.f.Entry);
            s.vars[_addr_memVar] = s.startmem;
            if (s.hasOpenDefers)
            { 
                // Create the deferBits variable and stack slot.  deferBits is a
                // bitmask showing which of the open-coded defers in this function
                // have been activated.
                var deferBitsTemp = tempAt(src.NoXPos, s.curfn, types.Types[TUINT8]);
                s.deferBitsTemp = deferBitsTemp; 
                // For this value, AuxInt is initialized to zero by default
                var startDeferBits = s.entryNewValue0(ssa.OpConst8, types.Types[TUINT8]);
                s.vars[_addr_deferBitsVar] = startDeferBits;
                s.deferBitsAddr = s.addr(deferBitsTemp);
                s.store(types.Types[TUINT8], s.deferBitsAddr, startDeferBits); 
                // Make sure that the deferBits stack slot is kept alive (for use
                // by panics) and stores to deferBits are not eliminated, even if
                // all checking code on deferBits in the function exit can be
                // eliminated, because the defer statements were all
                // unconditional.
                s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, deferBitsTemp, s.mem(), false);

            } 

            // Generate addresses of local declarations
            s.decladdrs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<ssa.Value>>{};
            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;

                    if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                        s.decladdrs[n] = s.entryNewValue2A(ssa.OpLocalAddr, types.NewPtr(n.Type), n, s.sp, s.startmem);
                        if (n.Class() == PPARAMOUT && s.canSSA(n))
                        { 
                            // Save ssa-able PPARAMOUT variables so we can
                            // store them back to the stack at the end of
                            // the function.
                            s.returns = append(s.returns, n);

                        }

                    else if (n.Class() == PAUTO)                     else if (n.Class() == PAUTOHEAP)                     else if (n.Class() == PFUNC)                     else 
                        s.Fatalf("local variable with class %v unimplemented", n.Class());
                    
                } 

                // Populate SSAable arguments.

                n = n__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;
                    if (n.Class() == PPARAM && s.canSSA(n))
                    {
                        var v = s.newValue0A(ssa.OpArg, n.Type, n);
                        s.vars[n] = v;
                        s.addNamedValue(n, v); // This helps with debugging information, not needed for compilation itself.
                    }

                } 

                // Convert the AST-based IR to the SSA-based IR

                n = n__prev1;
            }

            s.stmtList(fn.Func.Enter);
            s.stmtList(fn.Nbody); 

            // fallthrough to exit
            if (s.curBlock != null)
            {
                s.pushLine(fn.Func.Endlineno);
                s.exit();
                s.popLine();
            }

            foreach (var (_, b) in s.f.Blocks)
            {
                if (b.Pos != src.NoXPos)
                {
                    s.updateUnsetPredPos(b);
                }

            }
            s.insertPhis(); 

            // Main call to ssa package to compile function
            ssa.Compile(s.f);

            if (s.hasOpenDefers)
            {
                s.emitOpenDeferInfo();
            }

            return _addr_s.f!;

        });

        private static void dumpSourcesColumn(ptr<ssa.HTMLWriter> _addr_writer, ptr<Node> _addr_fn)
        {
            ref ssa.HTMLWriter writer = ref _addr_writer.val;
            ref Node fn = ref _addr_fn.val;
 
            // Read sources of target function fn.
            var fname = Ctxt.PosTable.Pos(fn.Pos).Filename();
            var (targetFn, err) = readFuncLines(fname, fn.Pos.Line(), fn.Func.Endlineno.Line());
            if (err != null)
            {
                writer.Logf("cannot read sources for function %v: %v", fn, err);
            } 

            // Read sources of inlined functions.
            slice<ptr<ssa.FuncLines>> inlFns = default;
            foreach (var (_, fi) in ssaDumpInlined)
            {
                src.XPos elno = default;
                if (fi.Name.Defn == null)
                { 
                    // Endlineno is filled from exported data.
                    elno = fi.Func.Endlineno;

                }
                else
                {
                    elno = fi.Name.Defn.Func.Endlineno;
                }

                fname = Ctxt.PosTable.Pos(fi.Pos).Filename();
                var (fnLines, err) = readFuncLines(fname, fi.Pos.Line(), elno.Line());
                if (err != null)
                {
                    writer.Logf("cannot read sources for inlined function %v: %v", fi, err);
                    continue;
                }

                inlFns = append(inlFns, fnLines);

            }
            sort.Sort(ssa.ByTopo(inlFns));
            if (targetFn != null)
            {
                inlFns = append(new slice<ptr<ssa.FuncLines>>(new ptr<ssa.FuncLines>[] { targetFn }), inlFns);
            }

            writer.WriteSources("sources", inlFns);

        }

        private static (ptr<ssa.FuncLines>, error) readFuncLines(@string file, ulong start, ulong end) => func((defer, _, __) =>
        {
            ptr<ssa.FuncLines> _p0 = default!;
            error _p0 = default!;

            var (f, err) = os.Open(os.ExpandEnv(file));
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            defer(f.Close());
            slice<@string> lines = default;
            var ln = uint(1L);
            var scanner = bufio.NewScanner(f);
            while (scanner.Scan() && ln <= end)
            {
                if (ln >= start)
                {
                    lines = append(lines, scanner.Text());
                }

                ln++;

            }

            return (addr(new ssa.FuncLines(Filename:file,StartLineno:start,Lines:lines)), error.As(null!)!);

        });

        // updateUnsetPredPos propagates the earliest-value position information for b
        // towards all of b's predecessors that need a position, and recurs on that
        // predecessor if its position is updated. B should have a non-empty position.
        private static void updateUnsetPredPos(this ptr<state> _addr_s, ptr<ssa.Block> _addr_b)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;

            if (b.Pos == src.NoXPos)
            {
                s.Fatalf("Block %s should have a position", b);
            }

            var bestPos = src.NoXPos;
            foreach (var (_, e) in b.Preds)
            {
                var p = e.Block();
                if (!p.LackingPos())
                {
                    continue;
                }

                if (bestPos == src.NoXPos)
                {
                    bestPos = b.Pos;
                    foreach (var (_, v) in b.Values)
                    {
                        if (v.LackingPos())
                        {
                            continue;
                        }

                        if (v.Pos != src.NoXPos)
                        { 
                            // Assume values are still in roughly textual order;
                            // TODO: could also seek minimum position?
                            bestPos = v.Pos;
                            break;

                        }

                    }

                }

                p.Pos = bestPos;
                s.updateUnsetPredPos(p); // We do not expect long chains of these, thus recursion is okay.
            }

        }

        // Information about each open-coded defer.
        private partial struct openDeferInfo
        {
            public ptr<Node> n; // If defer call is closure call, the address of the argtmp where the
// closure is stored.
            public ptr<ssa.Value> closure; // The node representing the argtmp where the closure is stored - used for
// function, method, or interface call, to store a closure that panic
// processing can use for this defer.
            public ptr<Node> closureNode; // If defer call is interface call, the address of the argtmp where the
// receiver is stored
            public ptr<ssa.Value> rcvr; // The node representing the argtmp where the receiver is stored
            public ptr<Node> rcvrNode; // The addresses of the argtmps where the evaluated arguments of the defer
// function call are stored.
            public slice<ptr<ssa.Value>> argVals; // The nodes representing the argtmps where the args of the defer are stored
            public slice<ptr<Node>> argNodes;
        }

        private partial struct state
        {
            public ptr<ssa.Config> config; // function we're building
            public ptr<ssa.Func> f; // Node for function
            public ptr<Node> curfn; // labels and labeled control flow nodes (OFOR, OFORUNTIL, OSWITCH, OSELECT) in f
            public map<@string, ptr<ssaLabel>> labels;
            public map<ptr<Node>, ptr<ssaLabel>> labeledNodes; // unlabeled break and continue statement tracking
            public ptr<ssa.Block> breakTo; // current target for plain break statement
            public ptr<ssa.Block> continueTo; // current target for plain continue statement

// current location where we're interpreting the AST
            public ptr<ssa.Block> curBlock; // variable assignments in the current block (map from variable symbol to ssa value)
// *Node is the unique identifier (an ONAME Node) for the variable.
// TODO: keep a single varnum map, then make all of these maps slices instead?
            public map<ptr<Node>, ptr<ssa.Value>> vars; // fwdVars are variables that are used before they are defined in the current block.
// This map exists just to coalesce multiple references into a single FwdRef op.
// *Node is the unique identifier (an ONAME Node) for the variable.
            public map<ptr<Node>, ptr<ssa.Value>> fwdVars; // all defined variables at the end of each block. Indexed by block ID.
            public slice<map<ptr<Node>, ptr<ssa.Value>>> defvars; // addresses of PPARAM and PPARAMOUT variables.
            public map<ptr<Node>, ptr<ssa.Value>> decladdrs; // starting values. Memory, stack pointer, and globals pointer
            public ptr<ssa.Value> startmem;
            public ptr<ssa.Value> sp;
            public ptr<ssa.Value> sb; // value representing address of where deferBits autotmp is stored
            public ptr<ssa.Value> deferBitsAddr;
            public ptr<Node> deferBitsTemp; // line number stack. The current line number is top of stack
            public slice<src.XPos> line; // the last line number processed; it may have been popped
            public src.XPos lastPos; // list of panic calls by function name and line number.
// Used to deduplicate panic calls.
            public map<funcLine, ptr<ssa.Block>> panics; // list of PPARAMOUT (return) variables.
            public slice<ptr<Node>> returns;
            public bool cgoUnsafeArgs;
            public bool hasdefer; // whether the function contains a defer statement
            public bool softFloat;
            public bool hasOpenDefers; // whether we are doing open-coded defers

// If doing open-coded defers, list of info about the defer calls in
// scanning order. Hence, at exit we should run these defers in reverse
// order of this list
            public slice<ptr<openDeferInfo>> openDefers; // For open-coded defers, this is the beginning and end blocks of the last
// defer exit code that we have generated so far. We use these to share
// code between exits if the shareDeferExits option (disabled by default)
// is on.
            public ptr<ssa.Block> lastDeferExit; // Entry block of last defer exit code we generated
            public ptr<ssa.Block> lastDeferFinalBlock; // Final block of last defer exit code we generated
            public long lastDeferCount; // Number of defers encountered at that point
        }

        private partial struct funcLine
        {
            public ptr<obj.LSym> f;
            public ptr<src.PosBase> @base;
            public ulong line;
        }

        private partial struct ssaLabel
        {
            public ptr<ssa.Block> target; // block identified by this label
            public ptr<ssa.Block> breakTarget; // block to break to in control flow node identified by this label
            public ptr<ssa.Block> continueTarget; // block to continue to in control flow node identified by this label
        }

        // label returns the label associated with sym, creating it if necessary.
        private static ptr<ssaLabel> label(this ptr<state> _addr_s, ptr<types.Sym> _addr_sym)
        {
            ref state s = ref _addr_s.val;
            ref types.Sym sym = ref _addr_sym.val;

            var lab = s.labels[sym.Name];
            if (lab == null)
            {
                lab = @new<ssaLabel>();
                s.labels[sym.Name] = lab;
            }

            return _addr_lab!;

        }

        private static void Logf(this ptr<state> _addr_s, @string msg, params object[] args)
        {
            args = args.Clone();
            ref state s = ref _addr_s.val;

            s.f.Logf(msg, args);
        }
        private static bool Log(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            return s.f.Log();
        }
        private static void Fatalf(this ptr<state> _addr_s, @string msg, params object[] args)
        {
            args = args.Clone();
            ref state s = ref _addr_s.val;

            s.f.Frontend().Fatalf(s.peekPos(), msg, args);
        }
        private static void Warnl(this ptr<state> _addr_s, src.XPos pos, @string msg, params object[] args)
        {
            args = args.Clone();
            ref state s = ref _addr_s.val;

            s.f.Warnl(pos, msg, args);
        }
        private static bool Debug_checknil(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            return s.f.Frontend().Debug_checknil();
        }

 
        // dummy node for the memory variable
        private static Node memVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"mem"});        private static Node ptrVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"ptr"});        private static Node lenVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"len"});        private static Node newlenVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"newlen"});        private static Node capVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"cap"});        private static Node typVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"typ"});        private static Node okVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"ok"});        private static Node deferBitsVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"deferBits"});

        // startBlock sets the current block we're generating code in to b.
        private static void startBlock(this ptr<state> _addr_s, ptr<ssa.Block> _addr_b)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;

            if (s.curBlock != null)
            {
                s.Fatalf("starting block %v when block %v has not ended", b, s.curBlock);
            }

            s.curBlock = b;
            s.vars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<ssa.Value>>{};
            foreach (var (n) in s.fwdVars)
            {
                delete(s.fwdVars, n);
            }

        }

        // endBlock marks the end of generating code for the current block.
        // Returns the (former) current block. Returns nil if there is no current
        // block, i.e. if no code flows to the current execution point.
        private static ptr<ssa.Block> endBlock(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            var b = s.curBlock;
            if (b == null)
            {
                return _addr_null!;
            }

            while (len(s.defvars) <= int(b.ID))
            {
                s.defvars = append(s.defvars, null);
            }

            s.defvars[b.ID] = s.vars;
            s.curBlock = null;
            s.vars = null;
            if (b.LackingPos())
            { 
                // Empty plain blocks get the line of their successor (handled after all blocks created),
                // except for increment blocks in For statements (handled in ssa conversion of OFOR),
                // and for blocks ending in GOTO/BREAK/CONTINUE.
                b.Pos = src.NoXPos;

            }
            else
            {
                b.Pos = s.lastPos;
            }

            return _addr_b!;

        }

        // pushLine pushes a line number on the line number stack.
        private static void pushLine(this ptr<state> _addr_s, src.XPos line)
        {
            ref state s = ref _addr_s.val;

            if (!line.IsKnown())
            { 
                // the frontend may emit node with line number missing,
                // use the parent line number in this case.
                line = s.peekPos();
                if (Debug['K'] != 0L)
                {
                    Warn("buildssa: unknown position (line 0)");
                }

            }
            else
            {
                s.lastPos = line;
            }

            s.line = append(s.line, line);

        }

        // popLine pops the top of the line number stack.
        private static void popLine(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            s.line = s.line[..len(s.line) - 1L];
        }

        // peekPos peeks the top of the line number stack.
        private static src.XPos peekPos(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            return s.line[len(s.line) - 1L];
        }

        // newValue0 adds a new value with no arguments to the current block.
        private static ptr<ssa.Value> newValue0(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.curBlock.NewValue0(s.peekPos(), op, t)!;
        }

        // newValue0A adds a new value with no arguments and an aux value to the current block.
        private static ptr<ssa.Value> newValue0A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.curBlock.NewValue0A(s.peekPos(), op, t, aux)!;
        }

        // newValue0I adds a new value with no arguments and an auxint value to the current block.
        private static ptr<ssa.Value> newValue0I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long auxint)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.curBlock.NewValue0I(s.peekPos(), op, t, auxint)!;
        }

        // newValue1 adds a new value with one argument to the current block.
        private static ptr<ssa.Value> newValue1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.curBlock.NewValue1(s.peekPos(), op, t, arg)!;
        }

        // newValue1A adds a new value with one argument and an aux value to the current block.
        private static ptr<ssa.Value> newValue1A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.curBlock.NewValue1A(s.peekPos(), op, t, aux, arg)!;
        }

        // newValue1Apos adds a new value with one argument and an aux value to the current block.
        // isStmt determines whether the created values may be a statement or not
        // (i.e., false means never, yes means maybe).
        private static ptr<ssa.Value> newValue1Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg, bool isStmt)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            if (isStmt)
            {
                return _addr_s.curBlock.NewValue1A(s.peekPos(), op, t, aux, arg)!;
            }

            return _addr_s.curBlock.NewValue1A(s.peekPos().WithNotStmt(), op, t, aux, arg)!;

        }

        // newValue1I adds a new value with one argument and an auxint value to the current block.
        private static ptr<ssa.Value> newValue1I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.curBlock.NewValue1I(s.peekPos(), op, t, aux, arg)!;
        }

        // newValue2 adds a new value with two arguments to the current block.
        private static ptr<ssa.Value> newValue2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            return _addr_s.curBlock.NewValue2(s.peekPos(), op, t, arg0, arg1)!;
        }

        // newValue2Apos adds a new value with two arguments and an aux value to the current block.
        // isStmt determines whether the created values may be a statement or not
        // (i.e., false means never, yes means maybe).
        private static ptr<ssa.Value> newValue2Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, bool isStmt)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            if (isStmt)
            {
                return _addr_s.curBlock.NewValue2A(s.peekPos(), op, t, aux, arg0, arg1)!;
            }

            return _addr_s.curBlock.NewValue2A(s.peekPos().WithNotStmt(), op, t, aux, arg0, arg1)!;

        }

        // newValue2I adds a new value with two arguments and an auxint value to the current block.
        private static ptr<ssa.Value> newValue2I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            return _addr_s.curBlock.NewValue2I(s.peekPos(), op, t, aux, arg0, arg1)!;
        }

        // newValue3 adds a new value with three arguments to the current block.
        private static ptr<ssa.Value> newValue3(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;

            return _addr_s.curBlock.NewValue3(s.peekPos(), op, t, arg0, arg1, arg2)!;
        }

        // newValue3I adds a new value with three arguments and an auxint value to the current block.
        private static ptr<ssa.Value> newValue3I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;

            return _addr_s.curBlock.NewValue3I(s.peekPos(), op, t, aux, arg0, arg1, arg2)!;
        }

        // newValue3A adds a new value with three arguments and an aux value to the current block.
        private static ptr<ssa.Value> newValue3A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;

            return _addr_s.curBlock.NewValue3A(s.peekPos(), op, t, aux, arg0, arg1, arg2)!;
        }

        // newValue3Apos adds a new value with three arguments and an aux value to the current block.
        // isStmt determines whether the created values may be a statement or not
        // (i.e., false means never, yes means maybe).
        private static ptr<ssa.Value> newValue3Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, bool isStmt)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;

            if (isStmt)
            {
                return _addr_s.curBlock.NewValue3A(s.peekPos(), op, t, aux, arg0, arg1, arg2)!;
            }

            return _addr_s.curBlock.NewValue3A(s.peekPos().WithNotStmt(), op, t, aux, arg0, arg1, arg2)!;

        }

        // newValue4 adds a new value with four arguments to the current block.
        private static ptr<ssa.Value> newValue4(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, ptr<ssa.Value> _addr_arg3)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;
            ref ssa.Value arg3 = ref _addr_arg3.val;

            return _addr_s.curBlock.NewValue4(s.peekPos(), op, t, arg0, arg1, arg2, arg3)!;
        }

        // newValue4 adds a new value with four arguments and an auxint value to the current block.
        private static ptr<ssa.Value> newValue4I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, ptr<ssa.Value> _addr_arg3)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;
            ref ssa.Value arg2 = ref _addr_arg2.val;
            ref ssa.Value arg3 = ref _addr_arg3.val;

            return _addr_s.curBlock.NewValue4I(s.peekPos(), op, t, aux, arg0, arg1, arg2, arg3)!;
        }

        // entryNewValue0 adds a new value with no arguments to the entry block.
        private static ptr<ssa.Value> entryNewValue0(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.Entry.NewValue0(src.NoXPos, op, t)!;
        }

        // entryNewValue0A adds a new value with no arguments and an aux value to the entry block.
        private static ptr<ssa.Value> entryNewValue0A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.Entry.NewValue0A(src.NoXPos, op, t, aux)!;
        }

        // entryNewValue1 adds a new value with one argument to the entry block.
        private static ptr<ssa.Value> entryNewValue1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.f.Entry.NewValue1(src.NoXPos, op, t, arg)!;
        }

        // entryNewValue1 adds a new value with one argument and an auxint value to the entry block.
        private static ptr<ssa.Value> entryNewValue1I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long auxint, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.f.Entry.NewValue1I(src.NoXPos, op, t, auxint, arg)!;
        }

        // entryNewValue1A adds a new value with one argument and an aux value to the entry block.
        private static ptr<ssa.Value> entryNewValue1A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            return _addr_s.f.Entry.NewValue1A(src.NoXPos, op, t, aux, arg)!;
        }

        // entryNewValue2 adds a new value with two arguments to the entry block.
        private static ptr<ssa.Value> entryNewValue2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            return _addr_s.f.Entry.NewValue2(src.NoXPos, op, t, arg0, arg1)!;
        }

        // entryNewValue2A adds a new value with two arguments and an aux value to the entry block.
        private static ptr<ssa.Value> entryNewValue2A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, object aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            return _addr_s.f.Entry.NewValue2A(src.NoXPos, op, t, aux, arg0, arg1)!;
        }

        // const* routines add a new const value to the entry block.
        private static ptr<ssa.Value> constSlice(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstSlice(t)!;
        }
        private static ptr<ssa.Value> constInterface(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstInterface(t)!;
        }
        private static ptr<ssa.Value> constNil(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstNil(t)!;
        }
        private static ptr<ssa.Value> constEmptyString(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstEmptyString(t)!;
        }
        private static ptr<ssa.Value> constBool(this ptr<state> _addr_s, bool c)
        {
            ref state s = ref _addr_s.val;

            return _addr_s.f.ConstBool(types.Types[TBOOL], c)!;
        }
        private static ptr<ssa.Value> constInt8(this ptr<state> _addr_s, ptr<types.Type> _addr_t, sbyte c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstInt8(t, c)!;
        }
        private static ptr<ssa.Value> constInt16(this ptr<state> _addr_s, ptr<types.Type> _addr_t, short c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstInt16(t, c)!;
        }
        private static ptr<ssa.Value> constInt32(this ptr<state> _addr_s, ptr<types.Type> _addr_t, int c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstInt32(t, c)!;
        }
        private static ptr<ssa.Value> constInt64(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstInt64(t, c)!;
        }
        private static ptr<ssa.Value> constFloat32(this ptr<state> _addr_s, ptr<types.Type> _addr_t, double c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstFloat32(t, c)!;
        }
        private static ptr<ssa.Value> constFloat64(this ptr<state> _addr_s, ptr<types.Type> _addr_t, double c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstFloat64(t, c)!;
        }
        private static ptr<ssa.Value> constInt(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            if (s.config.PtrSize == 8L)
            {
                return _addr_s.constInt64(t, c)!;
            }

            if (int64(int32(c)) != c)
            {
                s.Fatalf("integer constant too big %d", c);
            }

            return _addr_s.constInt32(t, int32(c))!;

        }
        private static ptr<ssa.Value> constOffPtrSP(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_s.f.ConstOffPtrSP(t, c, s.sp)!;
        }

        // newValueOrSfCall* are wrappers around newValue*, which may create a call to a
        // soft-float runtime function instead (when emitting soft-float code).
        private static ptr<ssa.Value> newValueOrSfCall1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg = ref _addr_arg.val;

            if (s.softFloat)
            {
                {
                    var (c, ok) = s.sfcall(op, arg);

                    if (ok)
                    {
                        return _addr_c!;
                    }

                }

            }

            return _addr_s.newValue1(op, t, arg)!;

        }
        private static ptr<ssa.Value> newValueOrSfCall2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value arg0 = ref _addr_arg0.val;
            ref ssa.Value arg1 = ref _addr_arg1.val;

            if (s.softFloat)
            {
                {
                    var (c, ok) = s.sfcall(op, arg0, arg1);

                    if (ok)
                    {
                        return _addr_c!;
                    }

                }

            }

            return _addr_s.newValue2(op, t, arg0, arg1)!;

        }

        private static void instrument(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_addr, bool wr) => func((_, panic, __) =>
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value addr = ref _addr_addr.val;

            if (!s.curfn.Func.InstrumentBody())
            {
                return ;
            }

            var w = t.Size();
            if (w == 0L)
            {
                return ; // can't race on zero-sized things
            }

            if (ssa.IsSanitizerSafeAddr(addr))
            {
                return ;
            }

            ptr<obj.LSym> fn;
            var needWidth = false;

            if (flag_msan)
            {
                fn = msanread;
                if (wr)
                {
                    fn = msanwrite;
                }

                needWidth = true;

            }
            else if (flag_race && t.NumComponents(types.CountBlankFields) > 1L)
            { 
                // for composite objects we have to write every address
                // because a write might happen to any subobject.
                // composites with only one element don't have subobjects, though.
                fn = racereadrange;
                if (wr)
                {
                    fn = racewriterange;
                }

                needWidth = true;

            }
            else if (flag_race)
            { 
                // for non-composite objects we can write just the start
                // address, as any write must write the first byte.
                fn = raceread;
                if (wr)
                {
                    fn = racewrite;
                }

            }
            else
            {
                panic("unreachable");
            }

            ptr<ssa.Value> args = new slice<ptr<ssa.Value>>(new ptr<ssa.Value>[] { addr });
            if (needWidth)
            {
                args = append(args, s.constInt(types.Types[TUINTPTR], w));
            }

            s.rtcall(fn, true, null, args);

        });

        private static ptr<ssa.Value> load(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_src)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value src = ref _addr_src.val;

            s.instrument(t, src, false);
            return _addr_s.rawLoad(t, src)!;
        }

        private static ptr<ssa.Value> rawLoad(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_src)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value src = ref _addr_src.val;

            return _addr_s.newValue2(ssa.OpLoad, t, src, s.mem())!;
        }

        private static void store(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst, ptr<ssa.Value> _addr_val)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value dst = ref _addr_dst.val;
            ref ssa.Value val = ref _addr_val.val;

            s.vars[_addr_memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, dst, val, s.mem());
        }

        private static void zero(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value dst = ref _addr_dst.val;

            s.instrument(t, dst, true);
            var store = s.newValue2I(ssa.OpZero, types.TypeMem, t.Size(), dst, s.mem());
            store.Aux = t;
            s.vars[_addr_memVar] = store;
        }

        private static void move(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst, ptr<ssa.Value> _addr_src)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value dst = ref _addr_dst.val;
            ref ssa.Value src = ref _addr_src.val;

            s.instrument(t, src, false);
            s.instrument(t, dst, true);
            var store = s.newValue3I(ssa.OpMove, types.TypeMem, t.Size(), dst, src, s.mem());
            store.Aux = t;
            s.vars[_addr_memVar] = store;
        }

        // stmtList converts the statement list n to SSA and adds it to s.
        private static void stmtList(this ptr<state> _addr_s, Nodes l)
        {
            ref state s = ref _addr_s.val;

            foreach (var (_, n) in l.Slice())
            {
                s.stmt(n);
            }

        }

        // stmt converts the statement n to SSA and adds it to s.
        private static void stmt(this ptr<state> _addr_s, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (!(n.Op == OVARKILL || n.Op == OVARLIVE || n.Op == OVARDEF))
            { 
                // OVARKILL, OVARLIVE, and OVARDEF are invisible to the programmer, so we don't use their line numbers to avoid confusion in debugging.
                s.pushLine(n.Pos);
                defer(s.popLine());

            } 

            // If s.curBlock is nil, and n isn't a label (which might have an associated goto somewhere),
            // then this code is dead. Stop here.
            if (s.curBlock == null && n.Op != OLABEL)
            {
                return ;
            }

            s.stmtList(n.Ninit);


            if (n.Op == OBLOCK)
            {
                s.stmtList(n.List); 

                // No-ops
                goto __switch_break0;
            }
            if (n.Op == OEMPTY || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OFALL)
            {
                goto __switch_break0;
            }
            if (n.Op == OCALLFUNC)
            {
                if (isIntrinsicCall(_addr_n))
                {
                    s.intrinsicCall(n);
                    return ;
                }

                fallthrough = true;

            }
            if (fallthrough || n.Op == OCALLMETH || n.Op == OCALLINTER)
            {
                s.call(n, callNormal);
                if (n.Op == OCALLFUNC && n.Left.Op == ONAME && n.Left.Class() == PFUNC)
                {
                    {
                        var fn = n.Left.Sym.Name;

                        if (compiling_runtime && fn == "throw" || n.Left.Sym.Pkg == Runtimepkg && (fn == "throwinit" || fn == "gopanic" || fn == "panicwrap" || fn == "block" || fn == "panicmakeslicelen" || fn == "panicmakeslicecap"))
                        {
                            var m = s.mem();
                            var b = s.endBlock();
                            b.Kind = ssa.BlockExit;
                            b.SetControl(m); 
                            // TODO: never rewrite OPANIC to OCALLFUNC in the
                            // first place. Need to wait until all backends
                            // go through SSA.
                        }

                    }

                }

                goto __switch_break0;
            }
            if (n.Op == ODEFER)
            {
                if (Debug_defer > 0L)
                {
                    @string defertype = default;
                    if (s.hasOpenDefers)
                    {
                        defertype = "open-coded";
                    }
                    else if (n.Esc == EscNever)
                    {
                        defertype = "stack-allocated";
                    }
                    else
                    {
                        defertype = "heap-allocated";
                    }

                    Warnl(n.Pos, "%s defer", defertype);

                }

                if (s.hasOpenDefers)
                {
                    s.openDeferRecord(n.Left);
                }
                else
                {
                    var d = callDefer;
                    if (n.Esc == EscNever)
                    {
                        d = callDeferStack;
                    }

                    s.call(n.Left, d);

                }

                goto __switch_break0;
            }
            if (n.Op == OGO)
            {
                s.call(n.Left, callGo);
                goto __switch_break0;
            }
            if (n.Op == OAS2DOTTYPE)
            {
                var (res, resok) = s.dottype(n.Right, true);
                var deref = false;
                if (!canSSAType(_addr_n.Right.Type))
                {
                    if (res.Op != ssa.OpLoad)
                    {
                        s.Fatalf("dottype of non-load");
                    }

                    var mem = s.mem();
                    if (mem.Op == ssa.OpVarKill)
                    {
                        mem = mem.Args[0L];
                    }

                    if (res.Args[1L] != mem)
                    {
                        s.Fatalf("memory no longer live from 2-result dottype load");
                    }

                    deref = true;
                    res = res.Args[0L];

                }

                s.assign(n.List.First(), res, deref, 0L);
                s.assign(n.List.Second(), resok, false, 0L);
                return ;
                goto __switch_break0;
            }
            if (n.Op == OAS2FUNC) 
            {
                // We come here only when it is an intrinsic call returning two values.
                if (!isIntrinsicCall(_addr_n.Right))
                {
                    s.Fatalf("non-intrinsic AS2FUNC not expanded %v", n.Right);
                }

                var v = s.intrinsicCall(n.Right);
                var v1 = s.newValue1(ssa.OpSelect0, n.List.First().Type, v);
                var v2 = s.newValue1(ssa.OpSelect1, n.List.Second().Type, v);
                s.assign(n.List.First(), v1, false, 0L);
                s.assign(n.List.Second(), v2, false, 0L);
                return ;
                goto __switch_break0;
            }
            if (n.Op == ODCL)
            {
                if (n.Left.Class() == PAUTOHEAP)
                {
                    s.Fatalf("DCL %v", n);
                }

                goto __switch_break0;
            }
            if (n.Op == OLABEL)
            {
                var sym = n.Sym;
                var lab = s.label(sym); 

                // Associate label with its control flow node, if any
                {
                    var ctl = n.labeledControl();

                    if (ctl != null)
                    {
                        s.labeledNodes[ctl] = lab;
                    } 

                    // The label might already have a target block via a goto.

                } 

                // The label might already have a target block via a goto.
                if (lab.target == null)
                {
                    lab.target = s.f.NewBlock(ssa.BlockPlain);
                } 

                // Go to that label.
                // (We pretend "label:" is preceded by "goto label", unless the predecessor is unreachable.)
                if (s.curBlock != null)
                {
                    b = s.endBlock();
                    b.AddEdgeTo(lab.target);
                }

                s.startBlock(lab.target);
                goto __switch_break0;
            }
            if (n.Op == OGOTO)
            {
                sym = n.Sym;

                lab = s.label(sym);
                if (lab.target == null)
                {
                    lab.target = s.f.NewBlock(ssa.BlockPlain);
                }

                b = s.endBlock();
                b.Pos = s.lastPos.WithIsStmt(); // Do this even if b is an empty block.
                b.AddEdgeTo(lab.target);
                goto __switch_break0;
            }
            if (n.Op == OAS)
            {
                if (n.Left == n.Right && n.Left.Op == ONAME)
                { 
                    // An x=x assignment. No point in doing anything
                    // here. In addition, skipping this assignment
                    // prevents generating:
                    //   VARDEF x
                    //   COPY x -> x
                    // which is bad because x is incorrectly considered
                    // dead before the vardef. See issue #14904.
                    return ;

                } 

                // Evaluate RHS.
                var rhs = n.Right;
                if (rhs != null)
                {

                    if (rhs.Op == OSTRUCTLIT || rhs.Op == OARRAYLIT || rhs.Op == OSLICELIT) 
                        // All literals with nonzero fields have already been
                        // rewritten during walk. Any that remain are just T{}
                        // or equivalents. Use the zero value.
                        if (!isZero(rhs))
                        {
                            s.Fatalf("literal with nonzero value in SSA: %v", rhs);
                        }

                        rhs = null;
                    else if (rhs.Op == OAPPEND) 
                        // Check whether we're writing the result of an append back to the same slice.
                        // If so, we handle it specially to avoid write barriers on the fast
                        // (non-growth) path.
                        if (!samesafeexpr(n.Left, rhs.List.First()) || Debug['N'] != 0L)
                        {
                            break;
                        } 
                        // If the slice can be SSA'd, it'll be on the stack,
                        // so there will be no write barriers,
                        // so there's no need to attempt to prevent them.
                        if (s.canSSA(n.Left))
                        {
                            if (Debug_append > 0L)
                            { // replicating old diagnostic message
                                Warnl(n.Pos, "append: len-only update (in local slice)");

                            }

                            break;

                        }

                        if (Debug_append > 0L)
                        {
                            Warnl(n.Pos, "append: len-only update");
                        }

                        s.append(rhs, true);
                        return ;
                    
                }

                if (n.Left.isBlank())
                { 
                    // _ = rhs
                    // Just evaluate rhs for side-effects.
                    if (rhs != null)
                    {
                        s.expr(rhs);
                    }

                    return ;

                }

                ptr<types.Type> t;
                if (n.Right != null)
                {
                    t = n.Right.Type;
                }
                else
                {
                    t = n.Left.Type;
                }

                ptr<ssa.Value> r;
                deref = !canSSAType(t);
                if (deref)
                {
                    if (rhs == null)
                    {
                        r = null; // Signal assign to use OpZero.
                    }
                    else
                    {
                        r = s.addr(rhs);
                    }

                }
                else
                {
                    if (rhs == null)
                    {
                        r = s.zeroVal(t);
                    }
                    else
                    {
                        r = s.expr(rhs);
                    }

                }

                skipMask skip = default;
                if (rhs != null && (rhs.Op == OSLICE || rhs.Op == OSLICE3 || rhs.Op == OSLICESTR) && samesafeexpr(rhs.Left, n.Left))
                { 
                    // We're assigning a slicing operation back to its source.
                    // Don't write back fields we aren't changing. See issue #14855.
                    var (i, j, k) = rhs.SliceBounds();
                    if (i != null && (i.Op == OLITERAL && i.Val().Ctype() == CTINT && i.Int64() == 0L))
                    { 
                        // [0:...] is the same as [:...]
                        i = null;

                    } 
                    // TODO: detect defaults for len/cap also.
                    // Currently doesn't really work because (*p)[:len(*p)] appears here as:
                    //    tmp = len(*p)
                    //    (*p)[:tmp]
                    //if j != nil && (j.Op == OLEN && samesafeexpr(j.Left, n.Left)) {
                    //      j = nil
                    //}
                    //if k != nil && (k.Op == OCAP && samesafeexpr(k.Left, n.Left)) {
                    //      k = nil
                    //}
                    if (i == null)
                    {
                        skip |= skipPtr;
                        if (j == null)
                        {
                            skip |= skipLen;
                        }

                        if (k == null)
                        {
                            skip |= skipCap;
                        }

                    }

                }

                s.assign(n.Left, r, deref, skip);
                goto __switch_break0;
            }
            if (n.Op == OIF)
            {
                if (Isconst(n.Left, CTBOOL))
                {
                    s.stmtList(n.Left.Ninit);
                    if (n.Left.Bool())
                    {
                        s.stmtList(n.Nbody);
                    }
                    else
                    {
                        s.stmtList(n.Rlist);
                    }

                    break;

                }

                var bEnd = s.f.NewBlock(ssa.BlockPlain);
                sbyte likely = default;
                if (n.Likely())
                {
                    likely = 1L;
                }

                ptr<ssa.Block> bThen;
                if (n.Nbody.Len() != 0L)
                {
                    bThen = s.f.NewBlock(ssa.BlockPlain);
                }
                else
                {
                    bThen = bEnd;
                }

                ptr<ssa.Block> bElse;
                if (n.Rlist.Len() != 0L)
                {
                    bElse = s.f.NewBlock(ssa.BlockPlain);
                }
                else
                {
                    bElse = bEnd;
                }

                s.condBranch(n.Left, bThen, bElse, likely);

                if (n.Nbody.Len() != 0L)
                {
                    s.startBlock(bThen);
                    s.stmtList(n.Nbody);
                    {
                        var b__prev2 = b;

                        b = s.endBlock();

                        if (b != null)
                        {
                            b.AddEdgeTo(bEnd);
                        }

                        b = b__prev2;

                    }

                }

                if (n.Rlist.Len() != 0L)
                {
                    s.startBlock(bElse);
                    s.stmtList(n.Rlist);
                    {
                        var b__prev2 = b;

                        b = s.endBlock();

                        if (b != null)
                        {
                            b.AddEdgeTo(bEnd);
                        }

                        b = b__prev2;

                    }

                }

                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == ORETURN)
            {
                s.stmtList(n.List);
                b = s.exit();
                b.Pos = s.lastPos.WithIsStmt();
                goto __switch_break0;
            }
            if (n.Op == ORETJMP)
            {
                s.stmtList(n.List);
                b = s.exit();
                b.Kind = ssa.BlockRetJmp; // override BlockRet
                b.Aux = n.Sym.Linksym();
                goto __switch_break0;
            }
            if (n.Op == OCONTINUE || n.Op == OBREAK)
            {
                ptr<ssa.Block> to;
                if (n.Sym == null)
                { 
                    // plain break/continue

                    if (n.Op == OCONTINUE) 
                        to = s.continueTo;
                    else if (n.Op == OBREAK) 
                        to = s.breakTo;
                    
                }
                else
                { 
                    // labeled break/continue; look up the target
                    sym = n.Sym;
                    lab = s.label(sym);

                    if (n.Op == OCONTINUE) 
                        to = lab.continueTarget;
                    else if (n.Op == OBREAK) 
                        to = lab.breakTarget;
                    
                }

                b = s.endBlock();
                b.Pos = s.lastPos.WithIsStmt(); // Do this even if b is an empty block.
                b.AddEdgeTo(to);
                goto __switch_break0;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL) 
            {
                // OFOR: for Ninit; Left; Right { Nbody }
                // cond (Left); body (Nbody); incr (Right)
                //
                // OFORUNTIL: for Ninit; Left; Right; List { Nbody }
                // => body: { Nbody }; incr: Right; if Left { lateincr: List; goto body }; end:
                var bCond = s.f.NewBlock(ssa.BlockPlain);
                var bBody = s.f.NewBlock(ssa.BlockPlain);
                var bIncr = s.f.NewBlock(ssa.BlockPlain);
                bEnd = s.f.NewBlock(ssa.BlockPlain); 

                // ensure empty for loops have correct position; issue #30167
                bBody.Pos = n.Pos; 

                // first, jump to condition test (OFOR) or body (OFORUNTIL)
                b = s.endBlock();
                if (n.Op == OFOR)
                {
                    b.AddEdgeTo(bCond); 
                    // generate code to test condition
                    s.startBlock(bCond);
                    if (n.Left != null)
                    {
                        s.condBranch(n.Left, bBody, bEnd, 1L);
                    }
                    else
                    {
                        b = s.endBlock();
                        b.Kind = ssa.BlockPlain;
                        b.AddEdgeTo(bBody);
                    }

                }
                else
                {
                    b.AddEdgeTo(bBody);
                } 

                // set up for continue/break in body
                var prevContinue = s.continueTo;
                var prevBreak = s.breakTo;
                s.continueTo = bIncr;
                s.breakTo = bEnd;
                lab = s.labeledNodes[n];
                if (lab != null)
                { 
                    // labeled for loop
                    lab.continueTarget = bIncr;
                    lab.breakTarget = bEnd;

                } 

                // generate body
                s.startBlock(bBody);
                s.stmtList(n.Nbody); 

                // tear down continue/break
                s.continueTo = prevContinue;
                s.breakTo = prevBreak;
                if (lab != null)
                {
                    lab.continueTarget = null;
                    lab.breakTarget = null;
                } 

                // done with body, goto incr
                {
                    var b__prev1 = b;

                    b = s.endBlock();

                    if (b != null)
                    {
                        b.AddEdgeTo(bIncr);
                    } 

                    // generate incr (and, for OFORUNTIL, condition)

                    b = b__prev1;

                } 

                // generate incr (and, for OFORUNTIL, condition)
                s.startBlock(bIncr);
                if (n.Right != null)
                {
                    s.stmt(n.Right);
                }

                if (n.Op == OFOR)
                {
                    {
                        var b__prev2 = b;

                        b = s.endBlock();

                        if (b != null)
                        {
                            b.AddEdgeTo(bCond); 
                            // It can happen that bIncr ends in a block containing only VARKILL,
                            // and that muddles the debugging experience.
                            if (n.Op != OFORUNTIL && b.Pos == src.NoXPos)
                            {
                                b.Pos = bCond.Pos;
                            }

                        }

                        b = b__prev2;

                    }

                }
                else
                { 
                    // bCond is unused in OFORUNTIL, so repurpose it.
                    var bLateIncr = bCond; 
                    // test condition
                    s.condBranch(n.Left, bLateIncr, bEnd, 1L); 
                    // generate late increment
                    s.startBlock(bLateIncr);
                    s.stmtList(n.List);
                    s.endBlock().AddEdgeTo(bBody);

                }

                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == OSWITCH || n.Op == OSELECT) 
            {
                // These have been mostly rewritten by the front end into their Nbody fields.
                // Our main task is to correctly hook up any break statements.
                bEnd = s.f.NewBlock(ssa.BlockPlain);

                prevBreak = s.breakTo;
                s.breakTo = bEnd;
                lab = s.labeledNodes[n];
                if (lab != null)
                { 
                    // labeled
                    lab.breakTarget = bEnd;

                } 

                // generate body code
                s.stmtList(n.Nbody);

                s.breakTo = prevBreak;
                if (lab != null)
                {
                    lab.breakTarget = null;
                } 

                // walk adds explicit OBREAK nodes to the end of all reachable code paths.
                // If we still have a current block here, then mark it unreachable.
                if (s.curBlock != null)
                {
                    m = s.mem();
                    b = s.endBlock();
                    b.Kind = ssa.BlockExit;
                    b.SetControl(m);
                }

                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == OVARDEF)
            {
                if (!s.canSSA(n.Left))
                {
                    s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, n.Left, s.mem(), false);
                }

                goto __switch_break0;
            }
            if (n.Op == OVARKILL) 
            {
                // Insert a varkill op to record that a variable is no longer live.
                // We only care about liveness info at call sites, so putting the
                // varkill in the store chain is enough to keep it correctly ordered
                // with respect to call ops.
                if (!s.canSSA(n.Left))
                {
                    s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarKill, types.TypeMem, n.Left, s.mem(), false);
                }

                goto __switch_break0;
            }
            if (n.Op == OVARLIVE) 
            {
                // Insert a varlive op to record that a variable is still live.
                if (!n.Left.Name.Addrtaken())
                {
                    s.Fatalf("VARLIVE variable %v must have Addrtaken set", n.Left);
                }


                if (n.Left.Class() == PAUTO || n.Left.Class() == PPARAM || n.Left.Class() == PPARAMOUT)                 else 
                    s.Fatalf("VARLIVE variable %v must be Auto or Arg", n.Left);
                                s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarLive, types.TypeMem, n.Left, s.mem());
                goto __switch_break0;
            }
            if (n.Op == OCHECKNIL)
            {
                var p = s.expr(n.Left);
                s.nilCheck(p);
                goto __switch_break0;
            }
            if (n.Op == OINLMARK)
            {
                s.newValue1I(ssa.OpInlMark, types.TypeVoid, n.Xoffset, s.mem());
                goto __switch_break0;
            }
            // default: 
                s.Fatalf("unhandled stmt %v", n.Op);

            __switch_break0:;

        });

        // If true, share as many open-coded defer exits as possible (with the downside of
        // worse line-number information)
        private static readonly var shareDeferExits = false;

        // exit processes any code that needs to be generated just before returning.
        // It returns a BlockRet block that ends the control flow. Its control value
        // will be set to the final memory state.


        // exit processes any code that needs to be generated just before returning.
        // It returns a BlockRet block that ends the control flow. Its control value
        // will be set to the final memory state.
        private static ptr<ssa.Block> exit(this ptr<state> _addr_s) => func((_, panic, __) =>
        {
            ref state s = ref _addr_s.val;

            if (s.hasdefer)
            {
                if (s.hasOpenDefers)
                {
                    if (shareDeferExits && s.lastDeferExit != null && len(s.openDefers) == s.lastDeferCount)
                    {
                        if (s.curBlock.Kind != ssa.BlockPlain)
                        {
                            panic("Block for an exit should be BlockPlain");
                        }

                        s.curBlock.AddEdgeTo(s.lastDeferExit);
                        s.endBlock();
                        return _addr_s.lastDeferFinalBlock!;

                    }

                    s.openDeferExit();

                }
                else
                {
                    s.rtcall(Deferreturn, true, null);
                }

            } 

            // Run exit code. Typically, this code copies heap-allocated PPARAMOUT
            // variables back to the stack.
            s.stmtList(s.curfn.Func.Exit); 

            // Store SSAable PPARAMOUT variables back to stack locations.
            foreach (var (_, n) in s.returns)
            {
                var addr = s.decladdrs[n];
                var val = s.variable(n, n.Type);
                s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, n, s.mem());
                s.store(n.Type, addr, val); 
                // TODO: if val is ever spilled, we'd like to use the
                // PPARAMOUT slot for spilling it. That won't happen
                // currently.
            } 

            // Do actual return.
            var m = s.mem();
            var b = s.endBlock();
            b.Kind = ssa.BlockRet;
            b.SetControl(m);
            if (s.hasdefer && s.hasOpenDefers)
            {
                s.lastDeferFinalBlock = b;
            }

            return _addr_b!;

        });

        private partial struct opAndType
        {
            public Op op;
            public types.EType etype;
        }

        private static map opToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndType, ssa.Op>{opAndType{OADD,TINT8}:ssa.OpAdd8,opAndType{OADD,TUINT8}:ssa.OpAdd8,opAndType{OADD,TINT16}:ssa.OpAdd16,opAndType{OADD,TUINT16}:ssa.OpAdd16,opAndType{OADD,TINT32}:ssa.OpAdd32,opAndType{OADD,TUINT32}:ssa.OpAdd32,opAndType{OADD,TINT64}:ssa.OpAdd64,opAndType{OADD,TUINT64}:ssa.OpAdd64,opAndType{OADD,TFLOAT32}:ssa.OpAdd32F,opAndType{OADD,TFLOAT64}:ssa.OpAdd64F,opAndType{OSUB,TINT8}:ssa.OpSub8,opAndType{OSUB,TUINT8}:ssa.OpSub8,opAndType{OSUB,TINT16}:ssa.OpSub16,opAndType{OSUB,TUINT16}:ssa.OpSub16,opAndType{OSUB,TINT32}:ssa.OpSub32,opAndType{OSUB,TUINT32}:ssa.OpSub32,opAndType{OSUB,TINT64}:ssa.OpSub64,opAndType{OSUB,TUINT64}:ssa.OpSub64,opAndType{OSUB,TFLOAT32}:ssa.OpSub32F,opAndType{OSUB,TFLOAT64}:ssa.OpSub64F,opAndType{ONOT,TBOOL}:ssa.OpNot,opAndType{ONEG,TINT8}:ssa.OpNeg8,opAndType{ONEG,TUINT8}:ssa.OpNeg8,opAndType{ONEG,TINT16}:ssa.OpNeg16,opAndType{ONEG,TUINT16}:ssa.OpNeg16,opAndType{ONEG,TINT32}:ssa.OpNeg32,opAndType{ONEG,TUINT32}:ssa.OpNeg32,opAndType{ONEG,TINT64}:ssa.OpNeg64,opAndType{ONEG,TUINT64}:ssa.OpNeg64,opAndType{ONEG,TFLOAT32}:ssa.OpNeg32F,opAndType{ONEG,TFLOAT64}:ssa.OpNeg64F,opAndType{OBITNOT,TINT8}:ssa.OpCom8,opAndType{OBITNOT,TUINT8}:ssa.OpCom8,opAndType{OBITNOT,TINT16}:ssa.OpCom16,opAndType{OBITNOT,TUINT16}:ssa.OpCom16,opAndType{OBITNOT,TINT32}:ssa.OpCom32,opAndType{OBITNOT,TUINT32}:ssa.OpCom32,opAndType{OBITNOT,TINT64}:ssa.OpCom64,opAndType{OBITNOT,TUINT64}:ssa.OpCom64,opAndType{OIMAG,TCOMPLEX64}:ssa.OpComplexImag,opAndType{OIMAG,TCOMPLEX128}:ssa.OpComplexImag,opAndType{OREAL,TCOMPLEX64}:ssa.OpComplexReal,opAndType{OREAL,TCOMPLEX128}:ssa.OpComplexReal,opAndType{OMUL,TINT8}:ssa.OpMul8,opAndType{OMUL,TUINT8}:ssa.OpMul8,opAndType{OMUL,TINT16}:ssa.OpMul16,opAndType{OMUL,TUINT16}:ssa.OpMul16,opAndType{OMUL,TINT32}:ssa.OpMul32,opAndType{OMUL,TUINT32}:ssa.OpMul32,opAndType{OMUL,TINT64}:ssa.OpMul64,opAndType{OMUL,TUINT64}:ssa.OpMul64,opAndType{OMUL,TFLOAT32}:ssa.OpMul32F,opAndType{OMUL,TFLOAT64}:ssa.OpMul64F,opAndType{ODIV,TFLOAT32}:ssa.OpDiv32F,opAndType{ODIV,TFLOAT64}:ssa.OpDiv64F,opAndType{ODIV,TINT8}:ssa.OpDiv8,opAndType{ODIV,TUINT8}:ssa.OpDiv8u,opAndType{ODIV,TINT16}:ssa.OpDiv16,opAndType{ODIV,TUINT16}:ssa.OpDiv16u,opAndType{ODIV,TINT32}:ssa.OpDiv32,opAndType{ODIV,TUINT32}:ssa.OpDiv32u,opAndType{ODIV,TINT64}:ssa.OpDiv64,opAndType{ODIV,TUINT64}:ssa.OpDiv64u,opAndType{OMOD,TINT8}:ssa.OpMod8,opAndType{OMOD,TUINT8}:ssa.OpMod8u,opAndType{OMOD,TINT16}:ssa.OpMod16,opAndType{OMOD,TUINT16}:ssa.OpMod16u,opAndType{OMOD,TINT32}:ssa.OpMod32,opAndType{OMOD,TUINT32}:ssa.OpMod32u,opAndType{OMOD,TINT64}:ssa.OpMod64,opAndType{OMOD,TUINT64}:ssa.OpMod64u,opAndType{OAND,TINT8}:ssa.OpAnd8,opAndType{OAND,TUINT8}:ssa.OpAnd8,opAndType{OAND,TINT16}:ssa.OpAnd16,opAndType{OAND,TUINT16}:ssa.OpAnd16,opAndType{OAND,TINT32}:ssa.OpAnd32,opAndType{OAND,TUINT32}:ssa.OpAnd32,opAndType{OAND,TINT64}:ssa.OpAnd64,opAndType{OAND,TUINT64}:ssa.OpAnd64,opAndType{OOR,TINT8}:ssa.OpOr8,opAndType{OOR,TUINT8}:ssa.OpOr8,opAndType{OOR,TINT16}:ssa.OpOr16,opAndType{OOR,TUINT16}:ssa.OpOr16,opAndType{OOR,TINT32}:ssa.OpOr32,opAndType{OOR,TUINT32}:ssa.OpOr32,opAndType{OOR,TINT64}:ssa.OpOr64,opAndType{OOR,TUINT64}:ssa.OpOr64,opAndType{OXOR,TINT8}:ssa.OpXor8,opAndType{OXOR,TUINT8}:ssa.OpXor8,opAndType{OXOR,TINT16}:ssa.OpXor16,opAndType{OXOR,TUINT16}:ssa.OpXor16,opAndType{OXOR,TINT32}:ssa.OpXor32,opAndType{OXOR,TUINT32}:ssa.OpXor32,opAndType{OXOR,TINT64}:ssa.OpXor64,opAndType{OXOR,TUINT64}:ssa.OpXor64,opAndType{OEQ,TBOOL}:ssa.OpEqB,opAndType{OEQ,TINT8}:ssa.OpEq8,opAndType{OEQ,TUINT8}:ssa.OpEq8,opAndType{OEQ,TINT16}:ssa.OpEq16,opAndType{OEQ,TUINT16}:ssa.OpEq16,opAndType{OEQ,TINT32}:ssa.OpEq32,opAndType{OEQ,TUINT32}:ssa.OpEq32,opAndType{OEQ,TINT64}:ssa.OpEq64,opAndType{OEQ,TUINT64}:ssa.OpEq64,opAndType{OEQ,TINTER}:ssa.OpEqInter,opAndType{OEQ,TSLICE}:ssa.OpEqSlice,opAndType{OEQ,TFUNC}:ssa.OpEqPtr,opAndType{OEQ,TMAP}:ssa.OpEqPtr,opAndType{OEQ,TCHAN}:ssa.OpEqPtr,opAndType{OEQ,TPTR}:ssa.OpEqPtr,opAndType{OEQ,TUINTPTR}:ssa.OpEqPtr,opAndType{OEQ,TUNSAFEPTR}:ssa.OpEqPtr,opAndType{OEQ,TFLOAT64}:ssa.OpEq64F,opAndType{OEQ,TFLOAT32}:ssa.OpEq32F,opAndType{ONE,TBOOL}:ssa.OpNeqB,opAndType{ONE,TINT8}:ssa.OpNeq8,opAndType{ONE,TUINT8}:ssa.OpNeq8,opAndType{ONE,TINT16}:ssa.OpNeq16,opAndType{ONE,TUINT16}:ssa.OpNeq16,opAndType{ONE,TINT32}:ssa.OpNeq32,opAndType{ONE,TUINT32}:ssa.OpNeq32,opAndType{ONE,TINT64}:ssa.OpNeq64,opAndType{ONE,TUINT64}:ssa.OpNeq64,opAndType{ONE,TINTER}:ssa.OpNeqInter,opAndType{ONE,TSLICE}:ssa.OpNeqSlice,opAndType{ONE,TFUNC}:ssa.OpNeqPtr,opAndType{ONE,TMAP}:ssa.OpNeqPtr,opAndType{ONE,TCHAN}:ssa.OpNeqPtr,opAndType{ONE,TPTR}:ssa.OpNeqPtr,opAndType{ONE,TUINTPTR}:ssa.OpNeqPtr,opAndType{ONE,TUNSAFEPTR}:ssa.OpNeqPtr,opAndType{ONE,TFLOAT64}:ssa.OpNeq64F,opAndType{ONE,TFLOAT32}:ssa.OpNeq32F,opAndType{OLT,TINT8}:ssa.OpLess8,opAndType{OLT,TUINT8}:ssa.OpLess8U,opAndType{OLT,TINT16}:ssa.OpLess16,opAndType{OLT,TUINT16}:ssa.OpLess16U,opAndType{OLT,TINT32}:ssa.OpLess32,opAndType{OLT,TUINT32}:ssa.OpLess32U,opAndType{OLT,TINT64}:ssa.OpLess64,opAndType{OLT,TUINT64}:ssa.OpLess64U,opAndType{OLT,TFLOAT64}:ssa.OpLess64F,opAndType{OLT,TFLOAT32}:ssa.OpLess32F,opAndType{OLE,TINT8}:ssa.OpLeq8,opAndType{OLE,TUINT8}:ssa.OpLeq8U,opAndType{OLE,TINT16}:ssa.OpLeq16,opAndType{OLE,TUINT16}:ssa.OpLeq16U,opAndType{OLE,TINT32}:ssa.OpLeq32,opAndType{OLE,TUINT32}:ssa.OpLeq32U,opAndType{OLE,TINT64}:ssa.OpLeq64,opAndType{OLE,TUINT64}:ssa.OpLeq64U,opAndType{OLE,TFLOAT64}:ssa.OpLeq64F,opAndType{OLE,TFLOAT32}:ssa.OpLeq32F,};

        private static types.EType concreteEtype(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var e = t.Etype;

            if (e == TINT) 
                if (s.config.PtrSize == 8L)
                {
                    return TINT64;
                }

                return TINT32;
            else if (e == TUINT) 
                if (s.config.PtrSize == 8L)
                {
                    return TUINT64;
                }

                return TUINT32;
            else if (e == TUINTPTR) 
                if (s.config.PtrSize == 8L)
                {
                    return TUINT64;
                }

                return TUINT32;
            else 
                return e;
            
        }

        private static ssa.Op ssaOp(this ptr<state> _addr_s, Op op, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            var etype = s.concreteEtype(t);
            var (x, ok) = opToSSA[new opAndType(op,etype)];
            if (!ok)
            {
                s.Fatalf("unhandled binary op %v %s", op, etype);
            }

            return x;

        }

        private static ptr<types.Type> floatForComplex(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TCOMPLEX64) 
                return _addr_types.Types[TFLOAT32]!;
            else if (t.Etype == TCOMPLEX128) 
                return _addr_types.Types[TFLOAT64]!;
                        Fatalf("unexpected type: %v", t);
            return _addr_null!;

        }

        private static ptr<types.Type> complexForFloat(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TFLOAT32) 
                return _addr_types.Types[TCOMPLEX64]!;
            else if (t.Etype == TFLOAT64) 
                return _addr_types.Types[TCOMPLEX128]!;
                        Fatalf("unexpected type: %v", t);
            return _addr_null!;

        }

        private partial struct opAndTwoTypes
        {
            public Op op;
            public types.EType etype1;
            public types.EType etype2;
        }

        private partial struct twoTypes
        {
            public types.EType etype1;
            public types.EType etype2;
        }

        private partial struct twoOpsAndType
        {
            public ssa.Op op1;
            public ssa.Op op2;
            public types.EType intermediateType;
        }

        private static map fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TINT8,TFLOAT32}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to32F,TINT32},twoTypes{TINT16,TFLOAT32}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to32F,TINT32},twoTypes{TINT32,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to32F,TINT32},twoTypes{TINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to32F,TINT64},twoTypes{TINT8,TFLOAT64}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to64F,TINT32},twoTypes{TINT16,TFLOAT64}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to64F,TINT32},twoTypes{TINT32,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to64F,TINT32},twoTypes{TINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to64F,TINT64},twoTypes{TFLOAT32,TINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT32,TINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT32,TINT32}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpCopy,TINT32},twoTypes{TFLOAT32,TINT64}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpCopy,TINT64},twoTypes{TFLOAT64,TINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT64,TINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT64,TINT32}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpCopy,TINT32},twoTypes{TFLOAT64,TINT64}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpCopy,TINT64},twoTypes{TUINT8,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to32F,TINT32},twoTypes{TUINT16,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to32F,TINT32},twoTypes{TUINT32,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to32F,TINT64},twoTypes{TUINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,TUINT64},twoTypes{TUINT8,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to64F,TINT32},twoTypes{TUINT16,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to64F,TINT32},twoTypes{TUINT32,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to64F,TINT64},twoTypes{TUINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,TUINT64},twoTypes{TFLOAT32,TUINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT32,TUINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT32,TUINT32}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpTrunc64to32,TINT64},twoTypes{TFLOAT32,TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TUINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT64,TUINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT64,TUINT32}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpTrunc64to32,TINT64},twoTypes{TFLOAT64,TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TFLOAT32}:twoOpsAndType{ssa.OpCvt64Fto32F,ssa.OpCopy,TFLOAT32},twoTypes{TFLOAT64,TFLOAT64}:twoOpsAndType{ssa.OpRound64F,ssa.OpCopy,TFLOAT64},twoTypes{TFLOAT32,TFLOAT32}:twoOpsAndType{ssa.OpRound32F,ssa.OpCopy,TFLOAT32},twoTypes{TFLOAT32,TFLOAT64}:twoOpsAndType{ssa.OpCvt32Fto64F,ssa.OpCopy,TFLOAT64},};

        // this map is used only for 32-bit arch, and only includes the difference
        // on 32-bit arch, don't use int64<->float conversion for uint32
        private static map fpConvOpToSSA32 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TUINT32,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto32F,TUINT32},twoTypes{TUINT32,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto64F,TUINT32},twoTypes{TFLOAT32,TUINT32}:twoOpsAndType{ssa.OpCvt32Fto32U,ssa.OpCopy,TUINT32},twoTypes{TFLOAT64,TUINT32}:twoOpsAndType{ssa.OpCvt64Fto32U,ssa.OpCopy,TUINT32},};

        // uint64<->float conversions, only on machines that have instructions for that
        private static map uint64fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TUINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto32F,TUINT64},twoTypes{TUINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto64F,TUINT64},twoTypes{TFLOAT32,TUINT64}:twoOpsAndType{ssa.OpCvt32Fto64U,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TUINT64}:twoOpsAndType{ssa.OpCvt64Fto64U,ssa.OpCopy,TUINT64},};

        private static map shiftOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndTwoTypes, ssa.Op>{opAndTwoTypes{OLSH,TINT8,TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{OLSH,TUINT8,TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{OLSH,TINT8,TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{OLSH,TUINT8,TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{OLSH,TINT8,TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{OLSH,TUINT8,TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{OLSH,TINT8,TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{OLSH,TUINT8,TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{OLSH,TINT16,TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{OLSH,TUINT16,TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{OLSH,TINT16,TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{OLSH,TUINT16,TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{OLSH,TINT16,TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{OLSH,TUINT16,TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{OLSH,TINT16,TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{OLSH,TUINT16,TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{OLSH,TINT32,TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{OLSH,TUINT32,TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{OLSH,TINT32,TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{OLSH,TUINT32,TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{OLSH,TINT32,TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{OLSH,TUINT32,TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{OLSH,TINT32,TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{OLSH,TUINT32,TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{OLSH,TINT64,TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{OLSH,TUINT64,TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{OLSH,TINT64,TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{OLSH,TUINT64,TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{OLSH,TINT64,TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{OLSH,TUINT64,TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{OLSH,TINT64,TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{OLSH,TUINT64,TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{ORSH,TINT8,TUINT8}:ssa.OpRsh8x8,opAndTwoTypes{ORSH,TUINT8,TUINT8}:ssa.OpRsh8Ux8,opAndTwoTypes{ORSH,TINT8,TUINT16}:ssa.OpRsh8x16,opAndTwoTypes{ORSH,TUINT8,TUINT16}:ssa.OpRsh8Ux16,opAndTwoTypes{ORSH,TINT8,TUINT32}:ssa.OpRsh8x32,opAndTwoTypes{ORSH,TUINT8,TUINT32}:ssa.OpRsh8Ux32,opAndTwoTypes{ORSH,TINT8,TUINT64}:ssa.OpRsh8x64,opAndTwoTypes{ORSH,TUINT8,TUINT64}:ssa.OpRsh8Ux64,opAndTwoTypes{ORSH,TINT16,TUINT8}:ssa.OpRsh16x8,opAndTwoTypes{ORSH,TUINT16,TUINT8}:ssa.OpRsh16Ux8,opAndTwoTypes{ORSH,TINT16,TUINT16}:ssa.OpRsh16x16,opAndTwoTypes{ORSH,TUINT16,TUINT16}:ssa.OpRsh16Ux16,opAndTwoTypes{ORSH,TINT16,TUINT32}:ssa.OpRsh16x32,opAndTwoTypes{ORSH,TUINT16,TUINT32}:ssa.OpRsh16Ux32,opAndTwoTypes{ORSH,TINT16,TUINT64}:ssa.OpRsh16x64,opAndTwoTypes{ORSH,TUINT16,TUINT64}:ssa.OpRsh16Ux64,opAndTwoTypes{ORSH,TINT32,TUINT8}:ssa.OpRsh32x8,opAndTwoTypes{ORSH,TUINT32,TUINT8}:ssa.OpRsh32Ux8,opAndTwoTypes{ORSH,TINT32,TUINT16}:ssa.OpRsh32x16,opAndTwoTypes{ORSH,TUINT32,TUINT16}:ssa.OpRsh32Ux16,opAndTwoTypes{ORSH,TINT32,TUINT32}:ssa.OpRsh32x32,opAndTwoTypes{ORSH,TUINT32,TUINT32}:ssa.OpRsh32Ux32,opAndTwoTypes{ORSH,TINT32,TUINT64}:ssa.OpRsh32x64,opAndTwoTypes{ORSH,TUINT32,TUINT64}:ssa.OpRsh32Ux64,opAndTwoTypes{ORSH,TINT64,TUINT8}:ssa.OpRsh64x8,opAndTwoTypes{ORSH,TUINT64,TUINT8}:ssa.OpRsh64Ux8,opAndTwoTypes{ORSH,TINT64,TUINT16}:ssa.OpRsh64x16,opAndTwoTypes{ORSH,TUINT64,TUINT16}:ssa.OpRsh64Ux16,opAndTwoTypes{ORSH,TINT64,TUINT32}:ssa.OpRsh64x32,opAndTwoTypes{ORSH,TUINT64,TUINT32}:ssa.OpRsh64Ux32,opAndTwoTypes{ORSH,TINT64,TUINT64}:ssa.OpRsh64x64,opAndTwoTypes{ORSH,TUINT64,TUINT64}:ssa.OpRsh64Ux64,};

        private static ssa.Op ssaShiftOp(this ptr<state> _addr_s, Op op, ptr<types.Type> _addr_t, ptr<types.Type> _addr_u)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref types.Type u = ref _addr_u.val;

            var etype1 = s.concreteEtype(t);
            var etype2 = s.concreteEtype(u);
            var (x, ok) = shiftOpToSSA[new opAndTwoTypes(op,etype1,etype2)];
            if (!ok)
            {
                s.Fatalf("unhandled shift op %v etype=%s/%s", op, etype1, etype2);
            }

            return x;

        }

        // expr converts the expression n to ssa, adds it to s and returns the ssa result.
        private static ptr<ssa.Value> expr(this ptr<state> _addr_s, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (!(n.Op == ONAME || n.Op == OLITERAL && n.Sym != null))
            { 
                // ONAMEs and named OLITERALs have the line number
                // of the decl, not the use. See issue 14742.
                s.pushLine(n.Pos);
                defer(s.popLine());

            }

            s.stmtList(n.Ninit);

            if (n.Op == OBYTES2STRTMP)
            {
                var slice = s.expr(n.Left);
                var ptr = s.newValue1(ssa.OpSlicePtr, s.f.Config.Types.BytePtr, slice);
                var len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], slice);
                return _addr_s.newValue2(ssa.OpStringMake, n.Type, ptr, len)!;
                goto __switch_break1;
            }
            if (n.Op == OSTR2BYTESTMP)
            {
                var str = s.expr(n.Left);
                ptr = s.newValue1(ssa.OpStringPtr, s.f.Config.Types.BytePtr, str);
                len = s.newValue1(ssa.OpStringLen, types.Types[TINT], str);
                return _addr_s.newValue3(ssa.OpSliceMake, n.Type, ptr, len, len)!;
                goto __switch_break1;
            }
            if (n.Op == OCFUNC)
            {
                var aux = n.Left.Sym.Linksym();
                return _addr_s.entryNewValue1A(ssa.OpAddr, n.Type, aux, s.sb)!;
                goto __switch_break1;
            }
            if (n.Op == ONAME)
            {
                if (n.Class() == PFUNC)
                { 
                    // "value" of a function is the address of the function's closure
                    var sym = funcsym(n.Sym).Linksym();
                    return _addr_s.entryNewValue1A(ssa.OpAddr, types.NewPtr(n.Type), sym, s.sb)!;

                }

                if (s.canSSA(n))
                {
                    return _addr_s.variable(n, n.Type)!;
                }

                var addr = s.addr(n);
                return _addr_s.load(n.Type, addr)!;
                goto __switch_break1;
            }
            if (n.Op == OCLOSUREVAR)
            {
                addr = s.addr(n);
                return _addr_s.load(n.Type, addr)!;
                goto __switch_break1;
            }
            if (n.Op == OLITERAL)
            {
                switch (n.Val().U.type())
                {
                    case ptr<Mpint> u:
                        var i = u.Int64();
                        switch (n.Type.Size())
                        {
                            case 1L: 
                                return _addr_s.constInt8(n.Type, int8(i))!;
                                break;
                            case 2L: 
                                return _addr_s.constInt16(n.Type, int16(i))!;
                                break;
                            case 4L: 
                                return _addr_s.constInt32(n.Type, int32(i))!;
                                break;
                            case 8L: 
                                return _addr_s.constInt64(n.Type, i)!;
                                break;
                            default: 
                                s.Fatalf("bad integer size %d", n.Type.Size());
                                return _addr_null!;
                                break;
                        }
                        break;
                    case @string u:
                        if (u == "")
                        {
                            return _addr_s.constEmptyString(n.Type)!;
                        }

                        return _addr_s.entryNewValue0A(ssa.OpConstString, n.Type, u)!;
                        break;
                    case bool u:
                        return _addr_s.constBool(u)!;
                        break;
                    case ptr<NilVal> u:
                        var t = n.Type;

                        if (t.IsSlice()) 
                            return _addr_s.constSlice(t)!;
                        else if (t.IsInterface()) 
                            return _addr_s.constInterface(t)!;
                        else 
                            return _addr_s.constNil(t)!;
                                                break;
                    case ptr<Mpflt> u:
                        switch (n.Type.Size())
                        {
                            case 4L: 
                                return _addr_s.constFloat32(n.Type, u.Float32())!;
                                break;
                            case 8L: 
                                return _addr_s.constFloat64(n.Type, u.Float64())!;
                                break;
                            default: 
                                s.Fatalf("bad float size %d", n.Type.Size());
                                return _addr_null!;
                                break;
                        }
                        break;
                    case ptr<Mpcplx> u:
                        var r = _addr_u.Real;
                        i = _addr_u.Imag;
                        switch (n.Type.Size())
                        {
                            case 8L: 
                                var pt = types.Types[TFLOAT32];
                                return _addr_s.newValue2(ssa.OpComplexMake, n.Type, s.constFloat32(pt, r.Float32()), s.constFloat32(pt, i.Float32()))!;
                                break;
                            case 16L: 
                                pt = types.Types[TFLOAT64];
                                return _addr_s.newValue2(ssa.OpComplexMake, n.Type, s.constFloat64(pt, r.Float64()), s.constFloat64(pt, i.Float64()))!;
                                break;
                            default: 
                                s.Fatalf("bad float size %d", n.Type.Size());
                                return _addr_null!;
                                break;
                        }
                        break;
                    default:
                    {
                        var u = n.Val().U.type();
                        s.Fatalf("unhandled OLITERAL %v", n.Val().Ctype());
                        return _addr_null!;
                        break;
                    }
                }
                goto __switch_break1;
            }
            if (n.Op == OCONVNOP)
            {
                var to = n.Type;
                var from = n.Left.Type; 

                // Assume everything will work out, so set up our return value.
                // Anything interesting that happens from here is a fatal.
                var x = s.expr(n.Left); 

                // Special case for not confusing GC and liveness.
                // We don't want pointers accidentally classified
                // as not-pointers or vice-versa because of copy
                // elision.
                if (to.IsPtrShaped() != from.IsPtrShaped())
                {
                    return _addr_s.newValue2(ssa.OpConvert, to, x, s.mem())!;
                }

                var v = s.newValue1(ssa.OpCopy, to, x); // ensure that v has the right type

                // CONVNOP closure
                if (to.Etype == TFUNC && from.IsPtrShaped())
                {
                    return _addr_v!;
                } 

                // named <--> unnamed type or typed <--> untyped const
                if (from.Etype == to.Etype)
                {
                    return _addr_v!;
                } 

                // unsafe.Pointer <--> *T
                if (to.Etype == TUNSAFEPTR && from.IsPtrShaped() || from.Etype == TUNSAFEPTR && to.IsPtrShaped())
                {
                    return _addr_v!;
                } 

                // map <--> *hmap
                if (to.Etype == TMAP && from.IsPtr() && to.MapType().Hmap == from.Elem())
                {
                    return _addr_v!;
                }

                dowidth(from);
                dowidth(to);
                if (from.Width != to.Width)
                {
                    s.Fatalf("CONVNOP width mismatch %v (%d) -> %v (%d)\n", from, from.Width, to, to.Width);
                    return _addr_null!;
                }

                if (etypesign(from.Etype) != etypesign(to.Etype))
                {
                    s.Fatalf("CONVNOP sign mismatch %v (%s) -> %v (%s)\n", from, from.Etype, to, to.Etype);
                    return _addr_null!;
                }

                if (instrumenting)
                { 
                    // These appear to be fine, but they fail the
                    // integer constraint below, so okay them here.
                    // Sample non-integer conversion: map[string]string -> *uint8
                    return _addr_v!;

                }

                if (etypesign(from.Etype) == 0L)
                {
                    s.Fatalf("CONVNOP unrecognized non-integer %v -> %v\n", from, to);
                    return _addr_null!;
                } 

                // integer, same width, same sign
                return _addr_v!;
                goto __switch_break1;
            }
            if (n.Op == OCONV)
            {
                x = s.expr(n.Left);
                var ft = n.Left.Type; // from type
                var tt = n.Type; // to type
                if (ft.IsBoolean() && tt.IsKind(TUINT8))
                { 
                    // Bool -> uint8 is generated internally when indexing into runtime.staticbyte.
                    return _addr_s.newValue1(ssa.OpCopy, n.Type, x)!;

                }

                if (ft.IsInteger() && tt.IsInteger())
                {
                    ssa.Op op = default;
                    if (tt.Size() == ft.Size())
                    {
                        op = ssa.OpCopy;
                    }
                    else if (tt.Size() < ft.Size())
                    { 
                        // truncation
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 21L: 
                                op = ssa.OpTrunc16to8;
                                break;
                            case 41L: 
                                op = ssa.OpTrunc32to8;
                                break;
                            case 42L: 
                                op = ssa.OpTrunc32to16;
                                break;
                            case 81L: 
                                op = ssa.OpTrunc64to8;
                                break;
                            case 82L: 
                                op = ssa.OpTrunc64to16;
                                break;
                            case 84L: 
                                op = ssa.OpTrunc64to32;
                                break;
                            default: 
                                s.Fatalf("weird integer truncation %v -> %v", ft, tt);
                                break;
                        }

                    }
                    else if (ft.IsSigned())
                    { 
                        // sign extension
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 12L: 
                                op = ssa.OpSignExt8to16;
                                break;
                            case 14L: 
                                op = ssa.OpSignExt8to32;
                                break;
                            case 18L: 
                                op = ssa.OpSignExt8to64;
                                break;
                            case 24L: 
                                op = ssa.OpSignExt16to32;
                                break;
                            case 28L: 
                                op = ssa.OpSignExt16to64;
                                break;
                            case 48L: 
                                op = ssa.OpSignExt32to64;
                                break;
                            default: 
                                s.Fatalf("bad integer sign extension %v -> %v", ft, tt);
                                break;
                        }

                    }
                    else
                    { 
                        // zero extension
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 12L: 
                                op = ssa.OpZeroExt8to16;
                                break;
                            case 14L: 
                                op = ssa.OpZeroExt8to32;
                                break;
                            case 18L: 
                                op = ssa.OpZeroExt8to64;
                                break;
                            case 24L: 
                                op = ssa.OpZeroExt16to32;
                                break;
                            case 28L: 
                                op = ssa.OpZeroExt16to64;
                                break;
                            case 48L: 
                                op = ssa.OpZeroExt32to64;
                                break;
                            default: 
                                s.Fatalf("weird integer sign extension %v -> %v", ft, tt);
                                break;
                        }

                    }

                    return _addr_s.newValue1(op, n.Type, x)!;

                }

                if (ft.IsFloat() || tt.IsFloat())
                {
                    var (conv, ok) = fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];
                    if (s.config.RegSize == 4L && thearch.LinkArch.Family != sys.MIPS && !s.softFloat)
                    {
                        {
                            var conv1__prev3 = conv1;

                            var (conv1, ok1) = fpConvOpToSSA32[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                            if (ok1)
                            {
                                conv = conv1;
                            }

                            conv1 = conv1__prev3;

                        }

                    }

                    if (thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.Wasm || thearch.LinkArch.Family == sys.S390X || s.softFloat)
                    {
                        {
                            var conv1__prev3 = conv1;

                            (conv1, ok1) = uint64fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                            if (ok1)
                            {
                                conv = conv1;
                            }

                            conv1 = conv1__prev3;

                        }

                    }

                    if (thearch.LinkArch.Family == sys.MIPS && !s.softFloat)
                    {
                        if (ft.Size() == 4L && ft.IsInteger() && !ft.IsSigned())
                        { 
                            // tt is float32 or float64, and ft is also unsigned
                            if (tt.Size() == 4L)
                            {
                                return _addr_s.uint32Tofloat32(n, x, ft, tt)!;
                            }

                            if (tt.Size() == 8L)
                            {
                                return _addr_s.uint32Tofloat64(n, x, ft, tt)!;
                            }

                        }
                        else if (tt.Size() == 4L && tt.IsInteger() && !tt.IsSigned())
                        { 
                            // ft is float32 or float64, and tt is unsigned integer
                            if (ft.Size() == 4L)
                            {
                                return _addr_s.float32ToUint32(n, x, ft, tt)!;
                            }

                            if (ft.Size() == 8L)
                            {
                                return _addr_s.float64ToUint32(n, x, ft, tt)!;
                            }

                        }

                    }

                    if (!ok)
                    {
                        s.Fatalf("weird float conversion %v -> %v", ft, tt);
                    }

                    var op1 = conv.op1;
                    var op2 = conv.op2;
                    var it = conv.intermediateType;

                    if (op1 != ssa.OpInvalid && op2 != ssa.OpInvalid)
                    { 
                        // normal case, not tripping over unsigned 64
                        if (op1 == ssa.OpCopy)
                        {
                            if (op2 == ssa.OpCopy)
                            {
                                return _addr_x!;
                            }

                            return _addr_s.newValueOrSfCall1(op2, n.Type, x)!;

                        }

                        if (op2 == ssa.OpCopy)
                        {
                            return _addr_s.newValueOrSfCall1(op1, n.Type, x)!;
                        }

                        return _addr_s.newValueOrSfCall1(op2, n.Type, s.newValueOrSfCall1(op1, types.Types[it], x))!;

                    } 
                    // Tricky 64-bit unsigned cases.
                    if (ft.IsInteger())
                    { 
                        // tt is float32 or float64, and ft is also unsigned
                        if (tt.Size() == 4L)
                        {
                            return _addr_s.uint64Tofloat32(n, x, ft, tt)!;
                        }

                        if (tt.Size() == 8L)
                        {
                            return _addr_s.uint64Tofloat64(n, x, ft, tt)!;
                        }

                        s.Fatalf("weird unsigned integer to float conversion %v -> %v", ft, tt);

                    } 
                    // ft is float32 or float64, and tt is unsigned integer
                    if (ft.Size() == 4L)
                    {
                        return _addr_s.float32ToUint64(n, x, ft, tt)!;
                    }

                    if (ft.Size() == 8L)
                    {
                        return _addr_s.float64ToUint64(n, x, ft, tt)!;
                    }

                    s.Fatalf("weird float to unsigned integer conversion %v -> %v", ft, tt);
                    return _addr_null!;

                }

                if (ft.IsComplex() && tt.IsComplex())
                {
                    op = default;
                    if (ft.Size() == tt.Size())
                    {
                        switch (ft.Size())
                        {
                            case 8L: 
                                op = ssa.OpRound32F;
                                break;
                            case 16L: 
                                op = ssa.OpRound64F;
                                break;
                            default: 
                                s.Fatalf("weird complex conversion %v -> %v", ft, tt);
                                break;
                        }

                    }
                    else if (ft.Size() == 8L && tt.Size() == 16L)
                    {
                        op = ssa.OpCvt32Fto64F;
                    }
                    else if (ft.Size() == 16L && tt.Size() == 8L)
                    {
                        op = ssa.OpCvt64Fto32F;
                    }
                    else
                    {
                        s.Fatalf("weird complex conversion %v -> %v", ft, tt);
                    }

                    var ftp = floatForComplex(_addr_ft);
                    var ttp = floatForComplex(_addr_tt);
                    return _addr_s.newValue2(ssa.OpComplexMake, tt, s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexReal, ftp, x)), s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexImag, ftp, x)))!;

                }

                s.Fatalf("unhandled OCONV %s -> %s", n.Left.Type.Etype, n.Type.Etype);
                return _addr_null!;
                goto __switch_break1;
            }
            if (n.Op == ODOTTYPE)
            {
                var (res, _) = s.dottype(n, false);
                return _addr_res!; 

                // binary ops
                goto __switch_break1;
            }
            if (n.Op == OLT || n.Op == OEQ || n.Op == ONE || n.Op == OLE || n.Op == OGE || n.Op == OGT)
            {
                var a = s.expr(n.Left);
                var b = s.expr(n.Right);
                if (n.Left.Type.IsComplex())
                {
                    pt = floatForComplex(_addr_n.Left.Type);
                    op = s.ssaOp(OEQ, pt);
                    r = s.newValueOrSfCall2(op, types.Types[TBOOL], s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b));
                    i = s.newValueOrSfCall2(op, types.Types[TBOOL], s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b));
                    var c = s.newValue2(ssa.OpAndB, types.Types[TBOOL], r, i);

                    if (n.Op == OEQ) 
                        return _addr_c!;
                    else if (n.Op == ONE) 
                        return _addr_s.newValue1(ssa.OpNot, types.Types[TBOOL], c)!;
                    else 
                        s.Fatalf("ordered complex compare %v", n.Op);
                    
                } 

                // Convert OGE and OGT into OLE and OLT.
                op = n.Op;

                if (op == OGE) 
                    op = OLE;
                    a = b;
                    b = a;
                else if (op == OGT) 
                    op = OLT;
                    a = b;
                    b = a;
                                if (n.Left.Type.IsFloat())
                { 
                    // float comparison
                    return _addr_s.newValueOrSfCall2(s.ssaOp(op, n.Left.Type), types.Types[TBOOL], a, b)!;

                } 
                // integer comparison
                return _addr_s.newValue2(s.ssaOp(op, n.Left.Type), types.Types[TBOOL], a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OMUL)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                {
                    var mulop = ssa.OpMul64F;
                    var addop = ssa.OpAdd64F;
                    var subop = ssa.OpSub64F;
                    pt = floatForComplex(_addr_n.Type); // Could be Float32 or Float64
                    var wt = types.Types[TFLOAT64]; // Compute in Float64 to minimize cancellation error

                    var areal = s.newValue1(ssa.OpComplexReal, pt, a);
                    var breal = s.newValue1(ssa.OpComplexReal, pt, b);
                    var aimag = s.newValue1(ssa.OpComplexImag, pt, a);
                    var bimag = s.newValue1(ssa.OpComplexImag, pt, b);

                    if (pt != wt)
                    { // Widen for calculation
                        areal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, areal);
                        breal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, breal);
                        aimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, aimag);
                        bimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, bimag);

                    }

                    var xreal = s.newValueOrSfCall2(subop, wt, s.newValueOrSfCall2(mulop, wt, areal, breal), s.newValueOrSfCall2(mulop, wt, aimag, bimag));
                    var ximag = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, areal, bimag), s.newValueOrSfCall2(mulop, wt, aimag, breal));

                    if (pt != wt)
                    { // Narrow to store back
                        xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                        ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);

                    }

                    return _addr_s.newValue2(ssa.OpComplexMake, n.Type, xreal, ximag)!;

                }

                if (n.Type.IsFloat())
                {
                    return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                }

                return _addr_s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == ODIV)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                { 
                    // TODO this is not executed because the front-end substitutes a runtime call.
                    // That probably ought to change; with modest optimization the widen/narrow
                    // conversions could all be elided in larger expression trees.
                    mulop = ssa.OpMul64F;
                    addop = ssa.OpAdd64F;
                    subop = ssa.OpSub64F;
                    var divop = ssa.OpDiv64F;
                    pt = floatForComplex(_addr_n.Type); // Could be Float32 or Float64
                    wt = types.Types[TFLOAT64]; // Compute in Float64 to minimize cancellation error

                    areal = s.newValue1(ssa.OpComplexReal, pt, a);
                    breal = s.newValue1(ssa.OpComplexReal, pt, b);
                    aimag = s.newValue1(ssa.OpComplexImag, pt, a);
                    bimag = s.newValue1(ssa.OpComplexImag, pt, b);

                    if (pt != wt)
                    { // Widen for calculation
                        areal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, areal);
                        breal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, breal);
                        aimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, aimag);
                        bimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, bimag);

                    }

                    var denom = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, breal, breal), s.newValueOrSfCall2(mulop, wt, bimag, bimag));
                    xreal = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, areal, breal), s.newValueOrSfCall2(mulop, wt, aimag, bimag));
                    ximag = s.newValueOrSfCall2(subop, wt, s.newValueOrSfCall2(mulop, wt, aimag, breal), s.newValueOrSfCall2(mulop, wt, areal, bimag)); 

                    // TODO not sure if this is best done in wide precision or narrow
                    // Double-rounding might be an issue.
                    // Note that the pre-SSA implementation does the entire calculation
                    // in wide format, so wide is compatible.
                    xreal = s.newValueOrSfCall2(divop, wt, xreal, denom);
                    ximag = s.newValueOrSfCall2(divop, wt, ximag, denom);

                    if (pt != wt)
                    { // Narrow to store back
                        xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                        ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);

                    }

                    return _addr_s.newValue2(ssa.OpComplexMake, n.Type, xreal, ximag)!;

                }

                if (n.Type.IsFloat())
                {
                    return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                }

                return _addr_s.intDivide(n, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OMOD)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                return _addr_s.intDivide(n, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OADD || n.Op == OSUB)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                {
                    pt = floatForComplex(_addr_n.Type);
                    op = s.ssaOp(n.Op, pt);
                    return _addr_s.newValue2(ssa.OpComplexMake, n.Type, s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b)), s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b)))!;
                }

                if (n.Type.IsFloat())
                {
                    return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                }

                return _addr_s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OAND || n.Op == OOR || n.Op == OXOR)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                return _addr_s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OLSH || n.Op == ORSH)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                var bt = b.Type;
                if (bt.IsSigned())
                {
                    var cmp = s.newValue2(s.ssaOp(OLE, bt), types.Types[TBOOL], s.zeroVal(bt), b);
                    s.check(cmp, panicshift);
                    bt = bt.ToUnsigned();
                }

                return _addr_s.newValue2(s.ssaShiftOp(n.Op, n.Type, bt), a.Type, a, b)!;
                goto __switch_break1;
            }
            if (n.Op == OANDAND || n.Op == OOROR) 
            {
                // To implement OANDAND (and OOROR), we introduce a
                // new temporary variable to hold the result. The
                // variable is associated with the OANDAND node in the
                // s.vars table (normally variables are only
                // associated with ONAME nodes). We convert
                //     A && B
                // to
                //     var = A
                //     if var {
                //         var = B
                //     }
                // Using var in the subsequent block introduces the
                // necessary phi variable.
                var el = s.expr(n.Left);
                s.vars[n] = el;

                b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(el); 
                // In theory, we should set b.Likely here based on context.
                // However, gc only gives us likeliness hints
                // in a single place, for plain OIF statements,
                // and passing around context is finnicky, so don't bother for now.

                var bRight = s.f.NewBlock(ssa.BlockPlain);
                var bResult = s.f.NewBlock(ssa.BlockPlain);
                if (n.Op == OANDAND)
                {
                    b.AddEdgeTo(bRight);
                    b.AddEdgeTo(bResult);
                }
                else if (n.Op == OOROR)
                {
                    b.AddEdgeTo(bResult);
                    b.AddEdgeTo(bRight);
                }

                s.startBlock(bRight);
                var er = s.expr(n.Right);
                s.vars[n] = er;

                b = s.endBlock();
                b.AddEdgeTo(bResult);

                s.startBlock(bResult);
                return _addr_s.variable(n, types.Types[TBOOL])!;
                goto __switch_break1;
            }
            if (n.Op == OCOMPLEX)
            {
                r = s.expr(n.Left);
                i = s.expr(n.Right);
                return _addr_s.newValue2(ssa.OpComplexMake, n.Type, r, i)!; 

                // unary ops
                goto __switch_break1;
            }
            if (n.Op == ONEG)
            {
                a = s.expr(n.Left);
                if (n.Type.IsComplex())
                {
                    var tp = floatForComplex(_addr_n.Type);
                    var negop = s.ssaOp(n.Op, tp);
                    return _addr_s.newValue2(ssa.OpComplexMake, n.Type, s.newValue1(negop, tp, s.newValue1(ssa.OpComplexReal, tp, a)), s.newValue1(negop, tp, s.newValue1(ssa.OpComplexImag, tp, a)))!;
                }

                return _addr_s.newValue1(s.ssaOp(n.Op, n.Type), a.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == ONOT || n.Op == OBITNOT)
            {
                a = s.expr(n.Left);
                return _addr_s.newValue1(s.ssaOp(n.Op, n.Type), a.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == OIMAG || n.Op == OREAL)
            {
                a = s.expr(n.Left);
                return _addr_s.newValue1(s.ssaOp(n.Op, n.Left.Type), n.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == OPLUS)
            {
                return _addr_s.expr(n.Left)!;
                goto __switch_break1;
            }
            if (n.Op == OADDR)
            {
                return _addr_s.addr(n.Left)!;
                goto __switch_break1;
            }
            if (n.Op == ORESULT)
            {
                addr = s.constOffPtrSP(types.NewPtr(n.Type), n.Xoffset);
                return _addr_s.load(n.Type, addr)!;
                goto __switch_break1;
            }
            if (n.Op == ODEREF)
            {
                var p = s.exprPtr(n.Left, n.Bounded(), n.Pos);
                return _addr_s.load(n.Type, p)!;
                goto __switch_break1;
            }
            if (n.Op == ODOT)
            {
                if (n.Left.Op == OSTRUCTLIT)
                { 
                    // All literals with nonzero fields have already been
                    // rewritten during walk. Any that remain are just T{}
                    // or equivalents. Use the zero value.
                    if (!isZero(n.Left))
                    {
                        s.Fatalf("literal with nonzero value in SSA: %v", n.Left);
                    }

                    return _addr_s.zeroVal(n.Type)!;

                } 
                // If n is addressable and can't be represented in
                // SSA, then load just the selected field. This
                // prevents false memory dependencies in race/msan
                // instrumentation.
                if (islvalue(n) && !s.canSSA(n))
                {
                    p = s.addr(n);
                    return _addr_s.load(n.Type, p)!;
                }

                v = s.expr(n.Left);
                return _addr_s.newValue1I(ssa.OpStructSelect, n.Type, int64(fieldIdx(_addr_n)), v)!;
                goto __switch_break1;
            }
            if (n.Op == ODOTPTR)
            {
                p = s.exprPtr(n.Left, n.Bounded(), n.Pos);
                p = s.newValue1I(ssa.OpOffPtr, types.NewPtr(n.Type), n.Xoffset, p);
                return _addr_s.load(n.Type, p)!;
                goto __switch_break1;
            }
            if (n.Op == OINDEX)
            {

                if (n.Left.Type.IsString()) 
                    if (n.Bounded() && Isconst(n.Left, CTSTR) && Isconst(n.Right, CTINT))
                    { 
                        // Replace "abc"[1] with 'b'.
                        // Delayed until now because "abc"[1] is not an ideal constant.
                        // See test/fixedbugs/issue11370.go.
                        return _addr_s.newValue0I(ssa.OpConst8, types.Types[TUINT8], int64(int8(strlit(n.Left)[n.Right.Int64()])))!;

                    }

                    a = s.expr(n.Left);
                    i = s.expr(n.Right);
                    len = s.newValue1(ssa.OpStringLen, types.Types[TINT], a);
                    i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
                    var ptrtyp = s.f.Config.Types.BytePtr;
                    ptr = s.newValue1(ssa.OpStringPtr, ptrtyp, a);
                    if (Isconst(n.Right, CTINT))
                    {
                        ptr = s.newValue1I(ssa.OpOffPtr, ptrtyp, n.Right.Int64(), ptr);
                    }
                    else
                    {
                        ptr = s.newValue2(ssa.OpAddPtr, ptrtyp, ptr, i);
                    }

                    return _addr_s.load(types.Types[TUINT8], ptr)!;
                else if (n.Left.Type.IsSlice()) 
                    p = s.addr(n);
                    return _addr_s.load(n.Left.Type.Elem(), p)!;
                else if (n.Left.Type.IsArray()) 
                    if (canSSAType(_addr_n.Left.Type))
                    { 
                        // SSA can handle arrays of length at most 1.
                        var bound = n.Left.Type.NumElem();
                        a = s.expr(n.Left);
                        i = s.expr(n.Right);
                        if (bound == 0L)
                        { 
                            // Bounds check will never succeed.  Might as well
                            // use constants for the bounds check.
                            var z = s.constInt(types.Types[TINT], 0L);
                            s.boundsCheck(z, z, ssa.BoundsIndex, false); 
                            // The return value won't be live, return junk.
                            return _addr_s.newValue0(ssa.OpUnknown, n.Type)!;

                        }

                        len = s.constInt(types.Types[TINT], bound);
                        s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded()); // checks i == 0
                        return _addr_s.newValue1I(ssa.OpArraySelect, n.Type, 0L, a)!;

                    }

                    p = s.addr(n);
                    return _addr_s.load(n.Left.Type.Elem(), p)!;
                else 
                    s.Fatalf("bad type for index %v", n.Left.Type);
                    return _addr_null!;
                                goto __switch_break1;
            }
            if (n.Op == OLEN || n.Op == OCAP)
            {

                if (n.Left.Type.IsSlice()) 
                    op = ssa.OpSliceLen;
                    if (n.Op == OCAP)
                    {
                        op = ssa.OpSliceCap;
                    }

                    return _addr_s.newValue1(op, types.Types[TINT], s.expr(n.Left))!;
                else if (n.Left.Type.IsString()) // string; not reachable for OCAP
                    return _addr_s.newValue1(ssa.OpStringLen, types.Types[TINT], s.expr(n.Left))!;
                else if (n.Left.Type.IsMap() || n.Left.Type.IsChan()) 
                    return _addr_s.referenceTypeBuiltin(n, s.expr(n.Left))!;
                else // array
                    return _addr_s.constInt(types.Types[TINT], n.Left.Type.NumElem())!;
                                goto __switch_break1;
            }
            if (n.Op == OSPTR)
            {
                a = s.expr(n.Left);
                if (n.Left.Type.IsSlice())
                {
                    return _addr_s.newValue1(ssa.OpSlicePtr, n.Type, a)!;
                }
                else
                {
                    return _addr_s.newValue1(ssa.OpStringPtr, n.Type, a)!;
                }

                goto __switch_break1;
            }
            if (n.Op == OITAB)
            {
                a = s.expr(n.Left);
                return _addr_s.newValue1(ssa.OpITab, n.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == OIDATA)
            {
                a = s.expr(n.Left);
                return _addr_s.newValue1(ssa.OpIData, n.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == OEFACE)
            {
                var tab = s.expr(n.Left);
                var data = s.expr(n.Right);
                return _addr_s.newValue2(ssa.OpIMake, n.Type, tab, data)!;
                goto __switch_break1;
            }
            if (n.Op == OSLICEHEADER)
            {
                p = s.expr(n.Left);
                var l = s.expr(n.List.First());
                c = s.expr(n.List.Second());
                return _addr_s.newValue3(ssa.OpSliceMake, n.Type, p, l, c)!;
                goto __switch_break1;
            }
            if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR)
            {
                v = s.expr(n.Left);
                i = ;                ptr<ssa.Value> j;                ptr<ssa.Value> k;

                var (low, high, max) = n.SliceBounds();
                if (low != null)
                {
                    i = s.expr(low);
                }

                if (high != null)
                {
                    j = s.expr(high);
                }

                if (max != null)
                {
                    k = s.expr(max);
                }

                var (p, l, c) = s.slice(v, i, j, k, n.Bounded());
                return _addr_s.newValue3(ssa.OpSliceMake, n.Type, p, l, c)!;
                goto __switch_break1;
            }
            if (n.Op == OSLICESTR)
            {
                v = s.expr(n.Left);
                i = ;                j = ;

                var (low, high, _) = n.SliceBounds();
                if (low != null)
                {
                    i = s.expr(low);
                }

                if (high != null)
                {
                    j = s.expr(high);
                }

                var (p, l, _) = s.slice(v, i, j, null, n.Bounded());
                return _addr_s.newValue2(ssa.OpStringMake, n.Type, p, l)!;
                goto __switch_break1;
            }
            if (n.Op == OCALLFUNC)
            {
                if (isIntrinsicCall(_addr_n))
                {
                    return _addr_s.intrinsicCall(n)!;
                }

                fallthrough = true;

            }
            if (fallthrough || n.Op == OCALLINTER || n.Op == OCALLMETH)
            {
                a = s.call(n, callNormal);
                return _addr_s.load(n.Type, a)!;
                goto __switch_break1;
            }
            if (n.Op == OGETG)
            {
                return _addr_s.newValue1(ssa.OpGetG, n.Type, s.mem())!;
                goto __switch_break1;
            }
            if (n.Op == OAPPEND)
            {
                return _addr_s.append(n, false)!;
                goto __switch_break1;
            }
            if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT) 
            {
                // All literals with nonzero fields have already been
                // rewritten during walk. Any that remain are just T{}
                // or equivalents. Use the zero value.
                if (!isZero(n))
                {
                    s.Fatalf("literal with nonzero value in SSA: %v", n);
                }

                return _addr_s.zeroVal(n.Type)!;
                goto __switch_break1;
            }
            if (n.Op == ONEWOBJ)
            {
                if (n.Type.Elem().Size() == 0L)
                {
                    return _addr_s.newValue1A(ssa.OpAddr, n.Type, zerobaseSym, s.sb)!;
                }

                var typ = s.expr(n.Left);
                var vv = s.rtcall(newobject, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type }), typ);
                return _addr_vv[0L]!;
                goto __switch_break1;
            }
            // default: 
                s.Fatalf("unhandled expr %v", n.Op);
                return _addr_null!;

            __switch_break1:;

        });

        // append converts an OAPPEND node to SSA.
        // If inplace is false, it converts the OAPPEND expression n to an ssa.Value,
        // adds it to s, and returns the Value.
        // If inplace is true, it writes the result of the OAPPEND expression n
        // back to the slice being appended to, and returns nil.
        // inplace MUST be set to false if the slice can be SSA'd.
        private static ptr<ssa.Value> append(this ptr<state> _addr_s, ptr<Node> _addr_n, bool inplace)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
 
            // If inplace is false, process as expression "append(s, e1, e2, e3)":
            //
            // ptr, len, cap := s
            // newlen := len + 3
            // if newlen > cap {
            //     ptr, len, cap = growslice(s, newlen)
            //     newlen = len + 3 // recalculate to avoid a spill
            // }
            // // with write barriers, if needed:
            // *(ptr+len) = e1
            // *(ptr+len+1) = e2
            // *(ptr+len+2) = e3
            // return makeslice(ptr, newlen, cap)
            //
            //
            // If inplace is true, process as statement "s = append(s, e1, e2, e3)":
            //
            // a := &s
            // ptr, len, cap := s
            // newlen := len + 3
            // if uint(newlen) > uint(cap) {
            //    newptr, len, newcap = growslice(ptr, len, cap, newlen)
            //    vardef(a)       // if necessary, advise liveness we are writing a new a
            //    *a.cap = newcap // write before ptr to avoid a spill
            //    *a.ptr = newptr // with write barrier
            // }
            // newlen = len + 3 // recalculate to avoid a spill
            // *a.len = newlen
            // // with write barriers, if needed:
            // *(ptr+len) = e1
            // *(ptr+len+1) = e2
            // *(ptr+len+2) = e3

            var et = n.Type.Elem();
            var pt = types.NewPtr(et); 

            // Evaluate slice
            var sn = n.List.First(); // the slice node is the first in the list

            ptr<ssa.Value> slice;            ptr<ssa.Value> addr;

            if (inplace)
            {
                addr = s.addr(sn);
                slice = s.load(n.Type, addr);
            }
            else
            {
                slice = s.expr(sn);
            } 

            // Allocate new blocks
            var grow = s.f.NewBlock(ssa.BlockPlain);
            var assign = s.f.NewBlock(ssa.BlockPlain); 

            // Decide if we need to grow
            var nargs = int64(n.List.Len() - 1L);
            var p = s.newValue1(ssa.OpSlicePtr, pt, slice);
            var l = s.newValue1(ssa.OpSliceLen, types.Types[TINT], slice);
            var c = s.newValue1(ssa.OpSliceCap, types.Types[TINT], slice);
            var nl = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], l, s.constInt(types.Types[TINT], nargs));

            var cmp = s.newValue2(s.ssaOp(OLT, types.Types[TUINT]), types.Types[TBOOL], c, nl);
            s.vars[_addr_ptrVar] = p;

            if (!inplace)
            {
                s.vars[_addr_newlenVar] = nl;
                s.vars[_addr_capVar] = c;
            }
            else
            {
                s.vars[_addr_lenVar] = l;
            }

            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.Likely = ssa.BranchUnlikely;
            b.SetControl(cmp);
            b.AddEdgeTo(grow);
            b.AddEdgeTo(assign); 

            // Call growslice
            s.startBlock(grow);
            var taddr = s.expr(n.Left);
            var r = s.rtcall(growslice, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { pt, types.Types[TINT], types.Types[TINT] }), taddr, p, l, c, nl);

            if (inplace)
            {
                if (sn.Op == ONAME && sn.Class() != PEXTERN)
                { 
                    // Tell liveness we're about to build a new slice
                    s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, sn, s.mem());

                }

                var capaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, sliceCapOffset, addr);
                s.store(types.Types[TINT], capaddr, r[2L]);
                s.store(pt, addr, r[0L]); 
                // load the value we just stored to avoid having to spill it
                s.vars[_addr_ptrVar] = s.load(pt, addr);
                s.vars[_addr_lenVar] = r[1L]; // avoid a spill in the fast path
            }
            else
            {
                s.vars[_addr_ptrVar] = r[0L];
                s.vars[_addr_newlenVar] = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], r[1L], s.constInt(types.Types[TINT], nargs));
                s.vars[_addr_capVar] = r[2L];
            }

            b = s.endBlock();
            b.AddEdgeTo(assign); 

            // assign new elements to slots
            s.startBlock(assign);

            if (inplace)
            {
                l = s.variable(_addr_lenVar, types.Types[TINT]); // generates phi for len
                nl = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], l, s.constInt(types.Types[TINT], nargs));
                var lenaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, sliceLenOffset, addr);
                s.store(types.Types[TINT], lenaddr, nl);

            } 

            // Evaluate args
            private partial struct argRec
            {
                public ptr<ssa.Value> v;
                public bool store;
            }
            var args = make_slice<argRec>(0L, nargs);
            foreach (var (_, n) in n.List.Slice()[1L..])
            {
                if (canSSAType(_addr_n.Type))
                {
                    args = append(args, new argRec(v:s.expr(n),store:true));
                }
                else
                {
                    var v = s.addr(n);
                    args = append(args, new argRec(v:v));
                }

            }
            p = s.variable(_addr_ptrVar, pt); // generates phi for ptr
            if (!inplace)
            {
                nl = s.variable(_addr_newlenVar, types.Types[TINT]); // generates phi for nl
                c = s.variable(_addr_capVar, types.Types[TINT]); // generates phi for cap
            }

            var p2 = s.newValue2(ssa.OpPtrIndex, pt, p, l);
            foreach (var (i, arg) in args)
            {
                addr = s.newValue2(ssa.OpPtrIndex, pt, p2, s.constInt(types.Types[TINT], int64(i)));
                if (arg.store)
                {
                    s.storeType(et, addr, arg.v, 0L, true);
                }
                else
                {
                    s.move(et, addr, arg.v);
                }

            }
            delete(s.vars, _addr_ptrVar);
            if (inplace)
            {
                delete(s.vars, _addr_lenVar);
                return _addr_null!;
            }

            delete(s.vars, _addr_newlenVar);
            delete(s.vars, _addr_capVar); 
            // make result
            return _addr_s.newValue3(ssa.OpSliceMake, n.Type, p, nl, c)!;

        }

        // condBranch evaluates the boolean expression cond and branches to yes
        // if cond is true and no if cond is false.
        // This function is intended to handle && and || better than just calling
        // s.expr(cond) and branching on the result.
        private static void condBranch(this ptr<state> _addr_s, ptr<Node> _addr_cond, ptr<ssa.Block> _addr_yes, ptr<ssa.Block> _addr_no, sbyte likely)
        {
            ref state s = ref _addr_s.val;
            ref Node cond = ref _addr_cond.val;
            ref ssa.Block yes = ref _addr_yes.val;
            ref ssa.Block no = ref _addr_no.val;


            if (cond.Op == OANDAND) 
                var mid = s.f.NewBlock(ssa.BlockPlain);
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, mid, no, max8(likely, 0L));
                s.startBlock(mid);
                s.condBranch(cond.Right, yes, no, likely);
                return ; 
                // Note: if likely==1, then both recursive calls pass 1.
                // If likely==-1, then we don't have enough information to decide
                // whether the first branch is likely or not. So we pass 0 for
                // the likeliness of the first branch.
                // TODO: have the frontend give us branch prediction hints for
                // OANDAND and OOROR nodes (if it ever has such info).
            else if (cond.Op == OOROR) 
                mid = s.f.NewBlock(ssa.BlockPlain);
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, yes, mid, min8(likely, 0L));
                s.startBlock(mid);
                s.condBranch(cond.Right, yes, no, likely);
                return ; 
                // Note: if likely==-1, then both recursive calls pass -1.
                // If likely==1, then we don't have enough info to decide
                // the likelihood of the first branch.
            else if (cond.Op == ONOT) 
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, no, yes, -likely);
                return ;
                        var c = s.expr(cond);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(c);
            b.Likely = ssa.BranchPrediction(likely); // gc and ssa both use -1/0/+1 for likeliness
            b.AddEdgeTo(yes);
            b.AddEdgeTo(no);

        }

        private partial struct skipMask // : byte
        {
        }

        private static readonly skipMask skipPtr = (skipMask)1L << (int)(iota);
        private static readonly var skipLen = 0;
        private static readonly var skipCap = 1;


        // assign does left = right.
        // Right has already been evaluated to ssa, left has not.
        // If deref is true, then we do left = *right instead (and right has already been nil-checked).
        // If deref is true and right == nil, just do left = 0.
        // skip indicates assignments (at the top level) that can be avoided.
        private static void assign(this ptr<state> _addr_s, ptr<Node> _addr_left, ptr<ssa.Value> _addr_right, bool deref, skipMask skip) => func((defer, _, __) =>
        {
            ref state s = ref _addr_s.val;
            ref Node left = ref _addr_left.val;
            ref ssa.Value right = ref _addr_right.val;

            if (left.Op == ONAME && left.isBlank())
            {
                return ;
            }

            var t = left.Type;
            dowidth(t);
            if (s.canSSA(left))
            {
                if (deref)
                {
                    s.Fatalf("can SSA LHS %v but not RHS %s", left, right);
                }

                if (left.Op == ODOT)
                { 
                    // We're assigning to a field of an ssa-able value.
                    // We need to build a new structure with the new value for the
                    // field we're assigning and the old values for the other fields.
                    // For instance:
                    //   type T struct {a, b, c int}
                    //   var T x
                    //   x.b = 5
                    // For the x.b = 5 assignment we want to generate x = T{x.a, 5, x.c}

                    // Grab information about the structure type.
                    t = left.Left.Type;
                    var nf = t.NumFields();
                    var idx = fieldIdx(_addr_left); 

                    // Grab old value of structure.
                    var old = s.expr(left.Left); 

                    // Make new structure.
                    var @new = s.newValue0(ssa.StructMakeOp(t.NumFields()), t); 

                    // Add fields as args.
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < nf; i++)
                        {
                            if (i == idx)
                            {
                                @new.AddArg(right);
                            }
                            else
                            {
                                @new.AddArg(s.newValue1I(ssa.OpStructSelect, t.FieldType(i), int64(i), old));
                            }

                        } 

                        // Recursively assign the new value we've made to the base of the dot op.


                        i = i__prev1;
                    } 

                    // Recursively assign the new value we've made to the base of the dot op.
                    s.assign(left.Left, new, false, 0L); 
                    // TODO: do we need to update named values here?
                    return ;

                }

                if (left.Op == OINDEX && left.Left.Type.IsArray())
                {
                    s.pushLine(left.Pos);
                    defer(s.popLine()); 
                    // We're assigning to an element of an ssa-able array.
                    // a[i] = v
                    t = left.Left.Type;
                    var n = t.NumElem();

                    i = s.expr(left.Right); // index
                    if (n == 0L)
                    { 
                        // The bounds check must fail.  Might as well
                        // ignore the actual index and just use zeros.
                        var z = s.constInt(types.Types[TINT], 0L);
                        s.boundsCheck(z, z, ssa.BoundsIndex, false);
                        return ;

                    }

                    if (n != 1L)
                    {
                        s.Fatalf("assigning to non-1-length array");
                    } 
                    // Rewrite to a = [1]{v}
                    var len = s.constInt(types.Types[TINT], 1L);
                    s.boundsCheck(i, len, ssa.BoundsIndex, false); // checks i == 0
                    var v = s.newValue1(ssa.OpArrayMake1, t, right);
                    s.assign(left.Left, v, false, 0L);
                    return ;

                } 
                // Update variable assignment.
                s.vars[left] = right;
                s.addNamedValue(left, right);
                return ;

            } 

            // If this assignment clobbers an entire local variable, then emit
            // OpVarDef so liveness analysis knows the variable is redefined.
            {
                var @base = clobberBase(_addr_left);

                if (@base.Op == ONAME && @base.Class() != PEXTERN && skip == 0L)
                {
                    s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, base, s.mem(), !@base.IsAutoTmp());
                } 

                // Left is not ssa-able. Compute its address.

            } 

            // Left is not ssa-able. Compute its address.
            var addr = s.addr(left);
            if (isReflectHeaderDataField(left))
            { 
                // Package unsafe's documentation says storing pointers into
                // reflect.SliceHeader and reflect.StringHeader's Data fields
                // is valid, even though they have type uintptr (#19168).
                // Mark it pointer type to signal the writebarrier pass to
                // insert a write barrier.
                t = types.Types[TUNSAFEPTR];

            }

            if (deref)
            { 
                // Treat as a mem->mem move.
                if (right == null)
                {
                    s.zero(t, addr);
                }
                else
                {
                    s.move(t, addr, right);
                }

                return ;

            } 
            // Treat as a store.
            s.storeType(t, addr, right, skip, !left.IsAutoTmp());

        });

        // zeroVal returns the zero value for type t.
        private static ptr<ssa.Value> zeroVal(this ptr<state> _addr_s, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;


            if (t.IsInteger()) 
                switch (t.Size())
                {
                    case 1L: 
                        return _addr_s.constInt8(t, 0L)!;
                        break;
                    case 2L: 
                        return _addr_s.constInt16(t, 0L)!;
                        break;
                    case 4L: 
                        return _addr_s.constInt32(t, 0L)!;
                        break;
                    case 8L: 
                        return _addr_s.constInt64(t, 0L)!;
                        break;
                    default: 
                        s.Fatalf("bad sized integer type %v", t);
                        break;
                }
            else if (t.IsFloat()) 
                switch (t.Size())
                {
                    case 4L: 
                        return _addr_s.constFloat32(t, 0L)!;
                        break;
                    case 8L: 
                        return _addr_s.constFloat64(t, 0L)!;
                        break;
                    default: 
                        s.Fatalf("bad sized float type %v", t);
                        break;
                }
            else if (t.IsComplex()) 
                switch (t.Size())
                {
                    case 8L: 
                        var z = s.constFloat32(types.Types[TFLOAT32], 0L);
                        return _addr_s.entryNewValue2(ssa.OpComplexMake, t, z, z)!;
                        break;
                    case 16L: 
                        z = s.constFloat64(types.Types[TFLOAT64], 0L);
                        return _addr_s.entryNewValue2(ssa.OpComplexMake, t, z, z)!;
                        break;
                    default: 
                        s.Fatalf("bad sized complex type %v", t);
                        break;
                }
            else if (t.IsString()) 
                return _addr_s.constEmptyString(t)!;
            else if (t.IsPtrShaped()) 
                return _addr_s.constNil(t)!;
            else if (t.IsBoolean()) 
                return _addr_s.constBool(false)!;
            else if (t.IsInterface()) 
                return _addr_s.constInterface(t)!;
            else if (t.IsSlice()) 
                return _addr_s.constSlice(t)!;
            else if (t.IsStruct()) 
                var n = t.NumFields();
                var v = s.entryNewValue0(ssa.StructMakeOp(t.NumFields()), t);
                for (long i = 0L; i < n; i++)
                {
                    v.AddArg(s.zeroVal(t.FieldType(i)));
                }

                return _addr_v!;
            else if (t.IsArray()) 
                switch (t.NumElem())
                {
                    case 0L: 
                        return _addr_s.entryNewValue0(ssa.OpArrayMake0, t)!;
                        break;
                    case 1L: 
                        return _addr_s.entryNewValue1(ssa.OpArrayMake1, t, s.zeroVal(t.Elem()))!;
                        break;
                }
                        s.Fatalf("zero for type %v not implemented", t);
            return _addr_null!;

        }

        private partial struct callKind // : sbyte
        {
        }

        private static readonly callKind callNormal = (callKind)iota;
        private static readonly var callDefer = 0;
        private static readonly var callDeferStack = 1;
        private static readonly var callGo = 2;


        private partial struct sfRtCallDef
        {
            public ptr<obj.LSym> rtfn;
            public types.EType rtype;
        }

        private static map<ssa.Op, sfRtCallDef> softFloatOps = default;

        private static void softfloatInit()
        { 
            // Some of these operations get transformed by sfcall.
            softFloatOps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, sfRtCallDef>{ssa.OpAdd32F:sfRtCallDef{sysfunc("fadd32"),TFLOAT32},ssa.OpAdd64F:sfRtCallDef{sysfunc("fadd64"),TFLOAT64},ssa.OpSub32F:sfRtCallDef{sysfunc("fadd32"),TFLOAT32},ssa.OpSub64F:sfRtCallDef{sysfunc("fadd64"),TFLOAT64},ssa.OpMul32F:sfRtCallDef{sysfunc("fmul32"),TFLOAT32},ssa.OpMul64F:sfRtCallDef{sysfunc("fmul64"),TFLOAT64},ssa.OpDiv32F:sfRtCallDef{sysfunc("fdiv32"),TFLOAT32},ssa.OpDiv64F:sfRtCallDef{sysfunc("fdiv64"),TFLOAT64},ssa.OpEq64F:sfRtCallDef{sysfunc("feq64"),TBOOL},ssa.OpEq32F:sfRtCallDef{sysfunc("feq32"),TBOOL},ssa.OpNeq64F:sfRtCallDef{sysfunc("feq64"),TBOOL},ssa.OpNeq32F:sfRtCallDef{sysfunc("feq32"),TBOOL},ssa.OpLess64F:sfRtCallDef{sysfunc("fgt64"),TBOOL},ssa.OpLess32F:sfRtCallDef{sysfunc("fgt32"),TBOOL},ssa.OpLeq64F:sfRtCallDef{sysfunc("fge64"),TBOOL},ssa.OpLeq32F:sfRtCallDef{sysfunc("fge32"),TBOOL},ssa.OpCvt32to32F:sfRtCallDef{sysfunc("fint32to32"),TFLOAT32},ssa.OpCvt32Fto32:sfRtCallDef{sysfunc("f32toint32"),TINT32},ssa.OpCvt64to32F:sfRtCallDef{sysfunc("fint64to32"),TFLOAT32},ssa.OpCvt32Fto64:sfRtCallDef{sysfunc("f32toint64"),TINT64},ssa.OpCvt64Uto32F:sfRtCallDef{sysfunc("fuint64to32"),TFLOAT32},ssa.OpCvt32Fto64U:sfRtCallDef{sysfunc("f32touint64"),TUINT64},ssa.OpCvt32to64F:sfRtCallDef{sysfunc("fint32to64"),TFLOAT64},ssa.OpCvt64Fto32:sfRtCallDef{sysfunc("f64toint32"),TINT32},ssa.OpCvt64to64F:sfRtCallDef{sysfunc("fint64to64"),TFLOAT64},ssa.OpCvt64Fto64:sfRtCallDef{sysfunc("f64toint64"),TINT64},ssa.OpCvt64Uto64F:sfRtCallDef{sysfunc("fuint64to64"),TFLOAT64},ssa.OpCvt64Fto64U:sfRtCallDef{sysfunc("f64touint64"),TUINT64},ssa.OpCvt32Fto64F:sfRtCallDef{sysfunc("f32to64"),TFLOAT64},ssa.OpCvt64Fto32F:sfRtCallDef{sysfunc("f64to32"),TFLOAT32},};

        }

        // TODO: do not emit sfcall if operation can be optimized to constant in later
        // opt phase
        private static (ptr<ssa.Value>, bool) sfcall(this ptr<state> _addr_s, ssa.Op op, params ptr<ptr<ssa.Value>>[] _addr_args)
        {
            ptr<ssa.Value> _p0 = default!;
            bool _p0 = default;
            args = args.Clone();
            ref state s = ref _addr_s.val;
            ref ssa.Value args = ref _addr_args.val;

            {
                var (callDef, ok) = softFloatOps[op];

                if (ok)
                {

                    if (op == ssa.OpLess32F || op == ssa.OpLess64F || op == ssa.OpLeq32F || op == ssa.OpLeq64F) 
                        args[0L] = args[1L];
                        args[1L] = args[0L];
                    else if (op == ssa.OpSub32F || op == ssa.OpSub64F) 
                        args[1L] = s.newValue1(s.ssaOp(ONEG, types.Types[callDef.rtype]), args[1L].Type, args[1L]);
                                        var result = s.rtcall(callDef.rtfn, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { types.Types[callDef.rtype] }), args)[0L];
                    if (op == ssa.OpNeq32F || op == ssa.OpNeq64F)
                    {
                        result = s.newValue1(ssa.OpNot, result.Type, result);
                    }

                    return (_addr_result!, true);

                }

            }

            return (_addr_null!, false);

        }

        private static map<intrinsicKey, intrinsicBuilder> intrinsics = default;

        // An intrinsicBuilder converts a call node n into an ssa value that
        // implements that call as an intrinsic. args is a list of arguments to the func.
        public delegate  ptr<ssa.Value> intrinsicBuilder(ptr<state>,  ptr<Node>,  slice<ptr<ssa.Value>>);

        private partial struct intrinsicKey
        {
            public ptr<sys.Arch> arch;
            public @string pkg;
            public @string fn;
        }

        private static void init() => func((_, panic, __) =>
        {
            intrinsics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<intrinsicKey, intrinsicBuilder>{};

            slice<ptr<sys.Arch>> all = default;
            slice<ptr<sys.Arch>> p4 = default;
            slice<ptr<sys.Arch>> p8 = default;
            slice<ptr<sys.Arch>> lwatomics = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in _addr_sys.Archs)
                {
                    a = __a;
                    all = append(all, a);
                    if (a.PtrSize == 4L)
                    {
                        p4 = append(p4, a);
                    }
                    else
                    {
                        p8 = append(p8, a);
                    }

                    if (a.Family != sys.PPC64)
                    {
                        lwatomics = append(lwatomics, a);
                    }

                } 

                // add adds the intrinsic b for pkg.fn for the given list of architectures.

                a = a__prev1;
            }

            Action<@string, @string, intrinsicBuilder, ptr<ptr<sys.Arch>>[]> add = (pkg, fn, b, archs) =>
            {
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in archs)
                    {
                        a = __a;
                        intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                    }

                    a = a__prev1;
                }
            } 
            // addF does the same as add but operates on architecture families.
; 
            // addF does the same as add but operates on architecture families.
            Action<@string, @string, intrinsicBuilder, sys.ArchFamily[]> addF = (pkg, fn, b, archFamilies) =>
            {
                long m = 0L;
                foreach (var (_, f) in archFamilies)
                {
                    if (f >= 32L)
                    {
                        panic("too many architecture families");
                    }

                    m |= 1L << (int)(uint(f));

                }
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in all)
                    {
                        a = __a;
                        if (m >> (int)(uint(a.Family)) & 1L != 0L)
                        {
                            intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                        }

                    }

                    a = a__prev1;
                }
            } 
            // alias defines pkg.fn = pkg2.fn2 for all architectures in archs for which pkg2.fn2 exists.
; 
            // alias defines pkg.fn = pkg2.fn2 for all architectures in archs for which pkg2.fn2 exists.
            Action<@string, @string, @string, @string, ptr<ptr<sys.Arch>>[]> alias = (pkg, fn, pkg2, fn2, archs) =>
            {
                var aliased = false;
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in archs)
                    {
                        a = __a;
                        {
                            var b__prev1 = b;

                            var (b, ok) = intrinsics[new intrinsicKey(a,pkg2,fn2)];

                            if (ok)
                            {
                                intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                                aliased = true;
                            }

                            b = b__prev1;

                        }

                    }

                    a = a__prev1;
                }

                if (!aliased)
                {
                    panic(fmt.Sprintf("attempted to alias undefined intrinsic: %s.%s", pkg, fn));
                }

            } 

            /******** runtime ********/
; 

            /******** runtime ********/
            if (!instrumenting)
            {
                add("runtime", "slicebytetostringtmp", (s, n, args) =>
                { 
                    // Compiler frontend optimizations emit OBYTES2STRTMP nodes
                    // for the backend instead of slicebytetostringtmp calls
                    // when not instrumenting.
                    return s.newValue2(ssa.OpStringMake, n.Type, args[0L], args[1L]);

                }, all);

            }

            addF("runtime/internal/math", "MulUintptr", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue2(ssa.OpMul32uover, types.NewTuple(types.Types[TUINT], types.Types[TUINT]), args[0L], args[1L]);
                }

                return s.newValue2(ssa.OpMul64uover, types.NewTuple(types.Types[TUINT], types.Types[TUINT]), args[0L], args[1L]);

            }, sys.AMD64, sys.I386, sys.MIPS64);
            add("runtime", "KeepAlive", (s, n, args) =>
            {
                var data = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, args[0L]);
                s.vars[_addr_memVar] = s.newValue2(ssa.OpKeepAlive, types.TypeMem, data, s.mem());
                return null;
            }, all);
            add("runtime", "getclosureptr", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetClosurePtr, s.f.Config.Types.Uintptr);
            }, all);

            add("runtime", "getcallerpc", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetCallerPC, s.f.Config.Types.Uintptr);
            }, all);

            add("runtime", "getcallersp", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetCallerSP, s.f.Config.Types.Uintptr);
            }, all); 

            /******** runtime/internal/sys ********/
            addF("runtime/internal/sys", "Ctz32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("runtime/internal/sys", "Ctz64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("runtime/internal/sys", "Bswap32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBswap32, types.Types[TUINT32], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X);
            addF("runtime/internal/sys", "Bswap64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBswap64, types.Types[TUINT64], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X); 

            /******** runtime/internal/atomic ********/
            addF("runtime/internal/atomic", "Load", (s, n, args) =>
            {
                var v = s.newValue2(ssa.OpAtomicLoad32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Load8", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoad8, types.NewTuple(types.Types[TUINT8], types.TypeMem), args[0L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT8], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Load64", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoad64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "LoadAcq", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoadAcq32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.PPC64, sys.S390X);
            addF("runtime/internal/atomic", "Loadp", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoadPtr, types.NewTuple(s.f.Config.Types.BytePtr, types.TypeMem), args[0L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, s.f.Config.Types.BytePtr, v);
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

            addF("runtime/internal/atomic", "Store", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicStore32, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Store8", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicStore8, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Store64", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicStore64, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "StorepNoWB", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicStorePtrNoWB, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "StoreRel", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicStoreRel32, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.PPC64, sys.S390X);

            addF("runtime/internal/atomic", "Xchg", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicExchange32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Xchg64", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicExchange64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

            addF("runtime/internal/atomic", "Xadd", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicAdd32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Xadd64", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicAdd64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

            Func<ssa.Op, ssa.Op, types.EType, Func<ptr<state>, ptr<Node>, slice<ptr<ssa.Value>>, ptr<ssa.Value>>> makeXaddARM64 = (op0, op1, ty) =>
            {
                return (s, n, args) =>
                { 
                    // Target Atomic feature is identified by dynamic detection
                    var addr = s.entryNewValue1A(ssa.OpAddr, types.Types[TBOOL].PtrTo(), arm64HasATOMICS, s.sb);
                    v = s.load(types.Types[TBOOL], addr);
                    var b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(v);
                    var bTrue = s.f.NewBlock(ssa.BlockPlain);
                    var bFalse = s.f.NewBlock(ssa.BlockPlain);
                    var bEnd = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bTrue);
                    b.AddEdgeTo(bFalse);
                    b.Likely = ssa.BranchUnlikely; // most machines don't have Atomics nowadays

                    // We have atomic instructions - use it directly.
                    s.startBlock(bTrue);
                    var v0 = s.newValue3(op1, types.NewTuple(types.Types[ty], types.TypeMem), args[0L], args[1L], s.mem());
                    s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v0);
                    s.vars[n] = s.newValue1(ssa.OpSelect0, types.Types[ty], v0);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Use original instruction sequence.
                    s.startBlock(bFalse);
                    var v1 = s.newValue3(op0, types.NewTuple(types.Types[ty], types.TypeMem), args[0L], args[1L], s.mem());
                    s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v1);
                    s.vars[n] = s.newValue1(ssa.OpSelect0, types.Types[ty], v1);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Merge results.
                    s.startBlock(bEnd);
                    return s.variable(n, types.Types[ty]);

                };

            }
;

            addF("runtime/internal/atomic", "Xadd", makeXaddARM64(ssa.OpAtomicAdd32, ssa.OpAtomicAdd32Variant, TUINT32), sys.ARM64);
            addF("runtime/internal/atomic", "Xadd64", makeXaddARM64(ssa.OpAtomicAdd64, ssa.OpAtomicAdd64Variant, TUINT64), sys.ARM64);

            addF("runtime/internal/atomic", "Cas", (s, n, args) =>
            {
                v = s.newValue4(ssa.OpAtomicCompareAndSwap32, types.NewTuple(types.Types[TBOOL], types.TypeMem), args[0L], args[1L], args[2L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TBOOL], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "Cas64", (s, n, args) =>
            {
                v = s.newValue4(ssa.OpAtomicCompareAndSwap64, types.NewTuple(types.Types[TBOOL], types.TypeMem), args[0L], args[1L], args[2L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TBOOL], v);
            }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
            addF("runtime/internal/atomic", "CasRel", (s, n, args) =>
            {
                v = s.newValue4(ssa.OpAtomicCompareAndSwap32, types.NewTuple(types.Types[TBOOL], types.TypeMem), args[0L], args[1L], args[2L], s.mem());
                s.vars[_addr_memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TBOOL], v);
            }, sys.PPC64);

            addF("runtime/internal/atomic", "And8", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicAnd8, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.PPC64, sys.S390X);
            addF("runtime/internal/atomic", "Or8", (s, n, args) =>
            {
                s.vars[_addr_memVar] = s.newValue3(ssa.OpAtomicOr8, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.PPC64, sys.S390X);

            alias("runtime/internal/atomic", "Loadint64", "runtime/internal/atomic", "Load64", all);
            alias("runtime/internal/atomic", "Xaddint64", "runtime/internal/atomic", "Xadd64", all);
            alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load", p4);
            alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load64", p8);
            alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load", p4);
            alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load64", p8);
            alias("runtime/internal/atomic", "LoadAcq", "runtime/internal/atomic", "Load", lwatomics);
            alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store", p4);
            alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store64", p8);
            alias("runtime/internal/atomic", "StoreRel", "runtime/internal/atomic", "Store", lwatomics);
            alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg", p4);
            alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg64", p8);
            alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd", p4);
            alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd64", p8);
            alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas", p4);
            alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas64", p8);
            alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas", p4);
            alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas64", p8);
            alias("runtime/internal/atomic", "CasRel", "runtime/internal/atomic", "Cas", lwatomics); 

            /******** math ********/
            addF("math", "Sqrt", (s, n, args) =>
            {
                return s.newValue1(ssa.OpSqrt, types.Types[TFLOAT64], args[0L]);
            }, sys.I386, sys.AMD64, sys.ARM, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X, sys.Wasm);
            addF("math", "Trunc", (s, n, args) =>
            {
                return s.newValue1(ssa.OpTrunc, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
            addF("math", "Ceil", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCeil, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
            addF("math", "Floor", (s, n, args) =>
            {
                return s.newValue1(ssa.OpFloor, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
            addF("math", "Round", (s, n, args) =>
            {
                return s.newValue1(ssa.OpRound, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.PPC64, sys.S390X);
            addF("math", "RoundToEven", (s, n, args) =>
            {
                return s.newValue1(ssa.OpRoundToEven, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.S390X, sys.Wasm);
            addF("math", "Abs", (s, n, args) =>
            {
                return s.newValue1(ssa.OpAbs, types.Types[TFLOAT64], args[0L]);
            }, sys.ARM64, sys.ARM, sys.PPC64, sys.Wasm);
            addF("math", "Copysign", (s, n, args) =>
            {
                return s.newValue2(ssa.OpCopysign, types.Types[TFLOAT64], args[0L], args[1L]);
            }, sys.PPC64, sys.Wasm);
            addF("math", "FMA", (s, n, args) =>
            {
                return s.newValue3(ssa.OpFMA, types.Types[TFLOAT64], args[0L], args[1L], args[2L]);
            }, sys.ARM64, sys.PPC64, sys.S390X);
            addF("math", "FMA", (s, n, args) =>
            {
                if (!s.config.UseFMA)
                {
                    var a = s.call(n, callNormal);
                    s.vars[n] = s.load(types.Types[TFLOAT64], a);
                    return s.variable(n, types.Types[TFLOAT64]);
                }

                v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[TBOOL], x86HasFMA);
                b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(v);
                bTrue = s.f.NewBlock(ssa.BlockPlain);
                bFalse = s.f.NewBlock(ssa.BlockPlain);
                bEnd = s.f.NewBlock(ssa.BlockPlain);
                b.AddEdgeTo(bTrue);
                b.AddEdgeTo(bFalse);
                b.Likely = ssa.BranchLikely; // >= haswell cpus are common

                // We have the intrinsic - use it directly.
                s.startBlock(bTrue);
                s.vars[n] = s.newValue3(ssa.OpFMA, types.Types[TFLOAT64], args[0L], args[1L], args[2L]);
                s.endBlock().AddEdgeTo(bEnd); 

                // Call the pure Go version.
                s.startBlock(bFalse);
                a = s.call(n, callNormal);
                s.vars[n] = s.load(types.Types[TFLOAT64], a);
                s.endBlock().AddEdgeTo(bEnd); 

                // Merge results.
                s.startBlock(bEnd);
                return s.variable(n, types.Types[TFLOAT64]);

            }, sys.AMD64);
            addF("math", "FMA", (s, n, args) =>
            {
                if (!s.config.UseFMA)
                {
                    a = s.call(n, callNormal);
                    s.vars[n] = s.load(types.Types[TFLOAT64], a);
                    return s.variable(n, types.Types[TFLOAT64]);
                }

                addr = s.entryNewValue1A(ssa.OpAddr, types.Types[TBOOL].PtrTo(), armHasVFPv4, s.sb);
                v = s.load(types.Types[TBOOL], addr);
                b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(v);
                bTrue = s.f.NewBlock(ssa.BlockPlain);
                bFalse = s.f.NewBlock(ssa.BlockPlain);
                bEnd = s.f.NewBlock(ssa.BlockPlain);
                b.AddEdgeTo(bTrue);
                b.AddEdgeTo(bFalse);
                b.Likely = ssa.BranchLikely; 

                // We have the intrinsic - use it directly.
                s.startBlock(bTrue);
                s.vars[n] = s.newValue3(ssa.OpFMA, types.Types[TFLOAT64], args[0L], args[1L], args[2L]);
                s.endBlock().AddEdgeTo(bEnd); 

                // Call the pure Go version.
                s.startBlock(bFalse);
                a = s.call(n, callNormal);
                s.vars[n] = s.load(types.Types[TFLOAT64], a);
                s.endBlock().AddEdgeTo(bEnd); 

                // Merge results.
                s.startBlock(bEnd);
                return s.variable(n, types.Types[TFLOAT64]);

            }, sys.ARM);

            Func<ssa.Op, Func<ptr<state>, ptr<Node>, slice<ptr<ssa.Value>>, ptr<ssa.Value>>> makeRoundAMD64 = op =>
            {
                return (s, n, args) =>
                {
                    v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[TBOOL], x86HasSSE41);
                    b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(v);
                    bTrue = s.f.NewBlock(ssa.BlockPlain);
                    bFalse = s.f.NewBlock(ssa.BlockPlain);
                    bEnd = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bTrue);
                    b.AddEdgeTo(bFalse);
                    b.Likely = ssa.BranchLikely; // most machines have sse4.1 nowadays

                    // We have the intrinsic - use it directly.
                    s.startBlock(bTrue);
                    s.vars[n] = s.newValue1(op, types.Types[TFLOAT64], args[0L]);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Call the pure Go version.
                    s.startBlock(bFalse);
                    a = s.call(n, callNormal);
                    s.vars[n] = s.load(types.Types[TFLOAT64], a);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Merge results.
                    s.startBlock(bEnd);
                    return s.variable(n, types.Types[TFLOAT64]);

                };

            }
;
            addF("math", "RoundToEven", makeRoundAMD64(ssa.OpRoundToEven), sys.AMD64);
            addF("math", "Floor", makeRoundAMD64(ssa.OpFloor), sys.AMD64);
            addF("math", "Ceil", makeRoundAMD64(ssa.OpCeil), sys.AMD64);
            addF("math", "Trunc", makeRoundAMD64(ssa.OpTrunc), sys.AMD64); 

            /******** math/bits ********/
            addF("math/bits", "TrailingZeros64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "TrailingZeros32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "TrailingZeros16", (s, n, args) =>
            {
                var x = s.newValue1(ssa.OpZeroExt16to32, types.Types[TUINT32], args[0L]);
                var c = s.constInt32(types.Types[TUINT32], 1L << (int)(16L));
                var y = s.newValue2(ssa.OpOr32, types.Types[TUINT32], x, c);
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], y);
            }, sys.MIPS);
            addF("math/bits", "TrailingZeros16", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz16, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.I386, sys.ARM, sys.ARM64, sys.Wasm);
            addF("math/bits", "TrailingZeros16", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt16to64, types.Types[TUINT64], args[0L]);
                c = s.constInt64(types.Types[TUINT64], 1L << (int)(16L));
                y = s.newValue2(ssa.OpOr64, types.Types[TUINT64], x, c);
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], y);
            }, sys.S390X, sys.PPC64);
            addF("math/bits", "TrailingZeros8", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt8to32, types.Types[TUINT32], args[0L]);
                c = s.constInt32(types.Types[TUINT32], 1L << (int)(8L));
                y = s.newValue2(ssa.OpOr32, types.Types[TUINT32], x, c);
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], y);
            }, sys.MIPS);
            addF("math/bits", "TrailingZeros8", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz8, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM, sys.ARM64, sys.Wasm);
            addF("math/bits", "TrailingZeros8", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt8to64, types.Types[TUINT64], args[0L]);
                c = s.constInt64(types.Types[TUINT64], 1L << (int)(8L));
                y = s.newValue2(ssa.OpOr64, types.Types[TUINT64], x, c);
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], y);
            }, sys.S390X);
            alias("math/bits", "ReverseBytes64", "runtime/internal/sys", "Bswap64", all);
            alias("math/bits", "ReverseBytes32", "runtime/internal/sys", "Bswap32", all); 
            // ReverseBytes inlines correctly, no need to intrinsify it.
            // ReverseBytes16 lowers to a rotate, no need for anything special here.
            addF("math/bits", "Len64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "Len32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitLen32, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64);
            addF("math/bits", "Len32", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], args[0L]);
                }

                x = s.newValue1(ssa.OpZeroExt32to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);

            }, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "Len16", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    x = s.newValue1(ssa.OpZeroExt16to32, types.Types[TUINT32], args[0L]);
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], x);
                }

                x = s.newValue1(ssa.OpZeroExt16to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);

            }, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "Len16", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitLen16, types.Types[TINT], args[0L]);
            }, sys.AMD64);
            addF("math/bits", "Len8", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    x = s.newValue1(ssa.OpZeroExt8to32, types.Types[TUINT32], args[0L]);
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], x);
                }

                x = s.newValue1(ssa.OpZeroExt8to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);

            }, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
            addF("math/bits", "Len8", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitLen8, types.Types[TINT], args[0L]);
            }, sys.AMD64);
            addF("math/bits", "Len", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], args[0L]);
                }

                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], args[0L]);

            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm); 
            // LeadingZeros is handled because it trivially calls Len.
            addF("math/bits", "Reverse64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev64, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev32, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse16", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev16, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse8", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev8, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitRev32, types.Types[TINT], args[0L]);
                }

                return s.newValue1(ssa.OpBitRev64, types.Types[TINT], args[0L]);

            }, sys.ARM64);
            addF("math/bits", "RotateLeft8", (s, n, args) =>
            {
                return s.newValue2(ssa.OpRotateLeft8, types.Types[TUINT8], args[0L], args[1L]);
            }, sys.AMD64);
            addF("math/bits", "RotateLeft16", (s, n, args) =>
            {
                return s.newValue2(ssa.OpRotateLeft16, types.Types[TUINT16], args[0L], args[1L]);
            }, sys.AMD64);
            addF("math/bits", "RotateLeft32", (s, n, args) =>
            {
                return s.newValue2(ssa.OpRotateLeft32, types.Types[TUINT32], args[0L], args[1L]);
            }, sys.AMD64, sys.ARM, sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
            addF("math/bits", "RotateLeft64", (s, n, args) =>
            {
                return s.newValue2(ssa.OpRotateLeft64, types.Types[TUINT64], args[0L], args[1L]);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
            alias("math/bits", "RotateLeft", "math/bits", "RotateLeft64", p8);

            Func<ssa.Op, ssa.Op, Func<ptr<state>, ptr<Node>, slice<ptr<ssa.Value>>, ptr<ssa.Value>>> makeOnesCountAMD64 = (op64, op32) =>
            {
                return (s, n, args) =>
                {
                    v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[TBOOL], x86HasPOPCNT);
                    b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(v);
                    bTrue = s.f.NewBlock(ssa.BlockPlain);
                    bFalse = s.f.NewBlock(ssa.BlockPlain);
                    bEnd = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bTrue);
                    b.AddEdgeTo(bFalse);
                    b.Likely = ssa.BranchLikely; // most machines have popcnt nowadays

                    // We have the intrinsic - use it directly.
                    s.startBlock(bTrue);
                    var op = op64;
                    if (s.config.PtrSize == 4L)
                    {
                        op = op32;
                    }

                    s.vars[n] = s.newValue1(op, types.Types[TINT], args[0L]);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Call the pure Go version.
                    s.startBlock(bFalse);
                    a = s.call(n, callNormal);
                    s.vars[n] = s.load(types.Types[TINT], a);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Merge results.
                    s.startBlock(bEnd);
                    return s.variable(n, types.Types[TINT]);

                };

            }
;
            addF("math/bits", "OnesCount64", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount64), sys.AMD64);
            addF("math/bits", "OnesCount64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount64, types.Types[TINT], args[0L]);
            }, sys.PPC64, sys.ARM64, sys.S390X, sys.Wasm);
            addF("math/bits", "OnesCount32", makeOnesCountAMD64(ssa.OpPopCount32, ssa.OpPopCount32), sys.AMD64);
            addF("math/bits", "OnesCount32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount32, types.Types[TINT], args[0L]);
            }, sys.PPC64, sys.ARM64, sys.S390X, sys.Wasm);
            addF("math/bits", "OnesCount16", makeOnesCountAMD64(ssa.OpPopCount16, ssa.OpPopCount16), sys.AMD64);
            addF("math/bits", "OnesCount16", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount16, types.Types[TINT], args[0L]);
            }, sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
            addF("math/bits", "OnesCount8", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount8, types.Types[TINT], args[0L]);
            }, sys.S390X, sys.PPC64, sys.Wasm);
            addF("math/bits", "OnesCount", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount32), sys.AMD64);
            addF("math/bits", "Mul64", (s, n, args) =>
            {
                return s.newValue2(ssa.OpMul64uhilo, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L]);
            }, sys.AMD64, sys.ARM64, sys.PPC64, sys.S390X, sys.MIPS64);
            alias("math/bits", "Mul", "math/bits", "Mul64", sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64, sys.ArchS390X, sys.ArchMIPS64, sys.ArchMIPS64LE);
            addF("math/bits", "Add64", (s, n, args) =>
            {
                return s.newValue3(ssa.OpAdd64carry, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L], args[2L]);
            }, sys.AMD64, sys.ARM64, sys.PPC64, sys.S390X);
            alias("math/bits", "Add", "math/bits", "Add64", sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64, sys.ArchS390X);
            addF("math/bits", "Sub64", (s, n, args) =>
            {
                return s.newValue3(ssa.OpSub64borrow, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L], args[2L]);
            }, sys.AMD64, sys.ARM64, sys.S390X);
            alias("math/bits", "Sub", "math/bits", "Sub64", sys.ArchAMD64, sys.ArchARM64, sys.ArchS390X);
            addF("math/bits", "Div64", (s, n, args) =>
            { 
                // check for divide-by-zero/overflow and panic with appropriate message
                var cmpZero = s.newValue2(s.ssaOp(ONE, types.Types[TUINT64]), types.Types[TBOOL], args[2L], s.zeroVal(types.Types[TUINT64]));
                s.check(cmpZero, panicdivide);
                var cmpOverflow = s.newValue2(s.ssaOp(OLT, types.Types[TUINT64]), types.Types[TBOOL], args[0L], args[2L]);
                s.check(cmpOverflow, panicoverflow);
                return s.newValue3(ssa.OpDiv128u, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L], args[2L]);

            }, sys.AMD64);
            alias("math/bits", "Div", "math/bits", "Div64", sys.ArchAMD64);

            alias("runtime/internal/sys", "Ctz8", "math/bits", "TrailingZeros8", all);
            alias("runtime/internal/sys", "TrailingZeros8", "math/bits", "TrailingZeros8", all);
            alias("runtime/internal/sys", "TrailingZeros64", "math/bits", "TrailingZeros64", all);
            alias("runtime/internal/sys", "Len8", "math/bits", "Len8", all);
            alias("runtime/internal/sys", "Len64", "math/bits", "Len64", all);
            alias("runtime/internal/sys", "OnesCount64", "math/bits", "OnesCount64", all);

            /******** sync/atomic ********/

            // Note: these are disabled by flag_race in findIntrinsic below.
            alias("sync/atomic", "LoadInt32", "runtime/internal/atomic", "Load", all);
            alias("sync/atomic", "LoadInt64", "runtime/internal/atomic", "Load64", all);
            alias("sync/atomic", "LoadPointer", "runtime/internal/atomic", "Loadp", all);
            alias("sync/atomic", "LoadUint32", "runtime/internal/atomic", "Load", all);
            alias("sync/atomic", "LoadUint64", "runtime/internal/atomic", "Load64", all);
            alias("sync/atomic", "LoadUintptr", "runtime/internal/atomic", "Load", p4);
            alias("sync/atomic", "LoadUintptr", "runtime/internal/atomic", "Load64", p8);

            alias("sync/atomic", "StoreInt32", "runtime/internal/atomic", "Store", all);
            alias("sync/atomic", "StoreInt64", "runtime/internal/atomic", "Store64", all); 
            // Note: not StorePointer, that needs a write barrier.  Same below for {CompareAnd}Swap.
            alias("sync/atomic", "StoreUint32", "runtime/internal/atomic", "Store", all);
            alias("sync/atomic", "StoreUint64", "runtime/internal/atomic", "Store64", all);
            alias("sync/atomic", "StoreUintptr", "runtime/internal/atomic", "Store", p4);
            alias("sync/atomic", "StoreUintptr", "runtime/internal/atomic", "Store64", p8);

            alias("sync/atomic", "SwapInt32", "runtime/internal/atomic", "Xchg", all);
            alias("sync/atomic", "SwapInt64", "runtime/internal/atomic", "Xchg64", all);
            alias("sync/atomic", "SwapUint32", "runtime/internal/atomic", "Xchg", all);
            alias("sync/atomic", "SwapUint64", "runtime/internal/atomic", "Xchg64", all);
            alias("sync/atomic", "SwapUintptr", "runtime/internal/atomic", "Xchg", p4);
            alias("sync/atomic", "SwapUintptr", "runtime/internal/atomic", "Xchg64", p8);

            alias("sync/atomic", "CompareAndSwapInt32", "runtime/internal/atomic", "Cas", all);
            alias("sync/atomic", "CompareAndSwapInt64", "runtime/internal/atomic", "Cas64", all);
            alias("sync/atomic", "CompareAndSwapUint32", "runtime/internal/atomic", "Cas", all);
            alias("sync/atomic", "CompareAndSwapUint64", "runtime/internal/atomic", "Cas64", all);
            alias("sync/atomic", "CompareAndSwapUintptr", "runtime/internal/atomic", "Cas", p4);
            alias("sync/atomic", "CompareAndSwapUintptr", "runtime/internal/atomic", "Cas64", p8);

            alias("sync/atomic", "AddInt32", "runtime/internal/atomic", "Xadd", all);
            alias("sync/atomic", "AddInt64", "runtime/internal/atomic", "Xadd64", all);
            alias("sync/atomic", "AddUint32", "runtime/internal/atomic", "Xadd", all);
            alias("sync/atomic", "AddUint64", "runtime/internal/atomic", "Xadd64", all);
            alias("sync/atomic", "AddUintptr", "runtime/internal/atomic", "Xadd", p4);
            alias("sync/atomic", "AddUintptr", "runtime/internal/atomic", "Xadd64", p8); 

            /******** math/big ********/
            add("math/big", "mulWW", (s, n, args) =>
            {
                return s.newValue2(ssa.OpMul64uhilo, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L]);
            }, sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64LE, sys.ArchPPC64, sys.ArchS390X);
            add("math/big", "divWW", (s, n, args) =>
            {
                return s.newValue3(ssa.OpDiv128u, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L], args[2L]);
            }, sys.ArchAMD64);

        });

        // findIntrinsic returns a function which builds the SSA equivalent of the
        // function identified by the symbol sym.  If sym is not an intrinsic call, returns nil.
        private static intrinsicBuilder findIntrinsic(ptr<types.Sym> _addr_sym)
        {
            ref types.Sym sym = ref _addr_sym.val;

            if (sym == null || sym.Pkg == null)
            {
                return null;
            }

            var pkg = sym.Pkg.Path;
            if (sym.Pkg == localpkg)
            {
                pkg = myimportpath;
            }

            if (flag_race && pkg == "sync/atomic")
            { 
                // The race detector needs to be able to intercept these calls.
                // We can't intrinsify them.
                return null;

            } 
            // Skip intrinsifying math functions (which may contain hard-float
            // instructions) when soft-float
            if (thearch.SoftFloat && pkg == "math")
            {
                return null;
            }

            var fn = sym.Name;
            if (ssa.IntrinsicsDisable)
            {
                if (pkg == "runtime" && (fn == "getcallerpc" || fn == "getcallersp" || fn == "getclosureptr"))
                { 
                    // These runtime functions don't have definitions, must be intrinsics.
                }
                else
                {
                    return null;
                }

            }

            return intrinsics[new intrinsicKey(thearch.LinkArch.Arch,pkg,fn)];

        }

        private static bool isIntrinsicCall(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || n.Left == null)
            {
                return false;
            }

            return findIntrinsic(_addr_n.Left.Sym) != null;

        }

        // intrinsicCall converts a call to a recognized intrinsic function into the intrinsic SSA operation.
        private static ptr<ssa.Value> intrinsicCall(this ptr<state> _addr_s, ptr<Node> _addr_n)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            var v = findIntrinsic(_addr_n.Left.Sym)(s, n, s.intrinsicArgs(n));
            if (ssa.IntrinsicsDebug > 0L)
            {
                var x = v;
                if (x == null)
                {
                    x = s.mem();
                }

                if (x.Op == ssa.OpSelect0 || x.Op == ssa.OpSelect1)
                {
                    x = x.Args[0L];
                }

                Warnl(n.Pos, "intrinsic substitution for %v with %s", n.Left.Sym.Name, x.LongString());

            }

            return _addr_v!;

        }

        // intrinsicArgs extracts args from n, evaluates them to SSA values, and returns them.
        private static slice<ptr<ssa.Value>> intrinsicArgs(this ptr<state> _addr_s, ptr<Node> _addr_n)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
 
            // Construct map of temps; see comments in s.call about the structure of n.
            map temps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Node>, ptr<ssa.Value>>{};
            foreach (var (_, a) in n.List.Slice())
            {
                if (a.Op != OAS)
                {
                    s.Fatalf("non-assignment as a temp function argument %v", a.Op);
                }

                var l = a.Left;
                var r = a.Right;
                if (l.Op != ONAME)
                {
                    s.Fatalf("non-ONAME temp function argument %v", a.Op);
                } 
                // Evaluate and store to "temporary".
                // Walk ensures these temporaries are dead outside of n.
                temps[l] = s.expr(r);

            }
            var args = make_slice<ptr<ssa.Value>>(n.Rlist.Len());
            foreach (var (i, n) in n.Rlist.Slice())
            { 
                // Store a value to an argument slot.
                {
                    var (x, ok) = temps[n];

                    if (ok)
                    { 
                        // This is a previously computed temporary.
                        args[i] = x;
                        continue;

                    } 
                    // This is an explicit value; evaluate it.

                } 
                // This is an explicit value; evaluate it.
                args[i] = s.expr(n);

            }
            return args;

        }

        // openDeferRecord adds code to evaluate and store the args for an open-code defer
        // call, and records info about the defer, so we can generate proper code on the
        // exit paths. n is the sub-node of the defer node that is the actual function
        // call. We will also record funcdata information on where the args are stored
        // (as well as the deferBits variable), and this will enable us to run the proper
        // defer calls during panics.
        private static void openDeferRecord(this ptr<state> _addr_s, ptr<Node> _addr_n)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
 
            // Do any needed expression evaluation for the args (including the
            // receiver, if any). This may be evaluating something like 'autotmp_3 =
            // once.mutex'. Such a statement will create a mapping in s.vars[] from
            // the autotmp name to the evaluated SSA arg value, but won't do any
            // stores to the stack.
            s.stmtList(n.List);

            slice<ptr<ssa.Value>> args = default;
            slice<ptr<Node>> argNodes = default;

            ptr<openDeferInfo> opendefer = addr(new openDeferInfo(n:n,));
            var fn = n.Left;
            if (n.Op == OCALLFUNC)
            { 
                // We must always store the function value in a stack slot for the
                // runtime panic code to use. But in the defer exit code, we will
                // call the function directly if it is a static function.
                var closureVal = s.expr(fn);
                var closure = s.openDeferSave(null, fn.Type, closureVal);
                opendefer.closureNode = closure.Aux._<ptr<Node>>();
                if (!(fn.Op == ONAME && fn.Class() == PFUNC))
                {
                    opendefer.closure = closure;
                }

            }
            else if (n.Op == OCALLMETH)
            {
                if (fn.Op != ODOTMETH)
                {
                    Fatalf("OCALLMETH: n.Left not an ODOTMETH: %v", fn);
                }

                closureVal = s.getMethodClosure(fn); 
                // We must always store the function value in a stack slot for the
                // runtime panic code to use. But in the defer exit code, we will
                // call the method directly.
                closure = s.openDeferSave(null, fn.Type, closureVal);
                opendefer.closureNode = closure.Aux._<ptr<Node>>();

            }
            else
            {
                if (fn.Op != ODOTINTER)
                {
                    Fatalf("OCALLINTER: n.Left not an ODOTINTER: %v", fn.Op);
                }

                var (closure, rcvr) = s.getClosureAndRcvr(fn);
                opendefer.closure = s.openDeferSave(null, closure.Type, closure); 
                // Important to get the receiver type correct, so it is recognized
                // as a pointer for GC purposes.
                opendefer.rcvr = s.openDeferSave(null, fn.Type.Recv().Type, rcvr);
                opendefer.closureNode = opendefer.closure.Aux._<ptr<Node>>();
                opendefer.rcvrNode = opendefer.rcvr.Aux._<ptr<Node>>();

            }

            foreach (var (_, argn) in n.Rlist.Slice())
            {
                ptr<ssa.Value> v;
                if (canSSAType(_addr_argn.Type))
                {
                    v = s.openDeferSave(null, argn.Type, s.expr(argn));
                }
                else
                {
                    v = s.openDeferSave(argn, argn.Type, null);
                }

                args = append(args, v);
                argNodes = append(argNodes, v.Aux._<ptr<Node>>());

            }
            opendefer.argVals = args;
            opendefer.argNodes = argNodes;
            var index = len(s.openDefers);
            s.openDefers = append(s.openDefers, opendefer); 

            // Update deferBits only after evaluation and storage to stack of
            // args/receiver/interface is successful.
            var bitvalue = s.constInt8(types.Types[TUINT8], 1L << (int)(uint(index)));
            var newDeferBits = s.newValue2(ssa.OpOr8, types.Types[TUINT8], s.variable(_addr_deferBitsVar, types.Types[TUINT8]), bitvalue);
            s.vars[_addr_deferBitsVar] = newDeferBits;
            s.store(types.Types[TUINT8], s.deferBitsAddr, newDeferBits);

        }

        // openDeferSave generates SSA nodes to store a value (with type t) for an
        // open-coded defer at an explicit autotmp location on the stack, so it can be
        // reloaded and used for the appropriate call on exit. If type t is SSAable, then
        // val must be non-nil (and n should be nil) and val is the value to be stored. If
        // type t is non-SSAable, then n must be non-nil (and val should be nil) and n is
        // evaluated (via s.addr() below) to get the value that is to be stored. The
        // function returns an SSA value representing a pointer to the autotmp location.
        private static ptr<ssa.Value> openDeferSave(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_val)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value val = ref _addr_val.val;

            var canSSA = canSSAType(_addr_t);
            src.XPos pos = default;
            if (canSSA)
            {
                pos = val.Pos;
            }
            else
            {
                pos = n.Pos;
            }

            var argTemp = tempAt(pos.WithNotStmt(), s.curfn, t);
            argTemp.Name.SetOpenDeferSlot(true);
            ptr<ssa.Value> addrArgTemp; 
            // Use OpVarLive to make sure stack slots for the args, etc. are not
            // removed by dead-store elimination
            if (s.curBlock.ID != s.f.Entry.ID)
            { 
                // Force the argtmp storing this defer function/receiver/arg to be
                // declared in the entry block, so that it will be live for the
                // defer exit code (which will actually access it only if the
                // associated defer call has been activated).
                s.defvars[s.f.Entry.ID][_addr_memVar] = s.entryNewValue1A(ssa.OpVarDef, types.TypeMem, argTemp, s.defvars[s.f.Entry.ID][_addr_memVar]);
                s.defvars[s.f.Entry.ID][_addr_memVar] = s.entryNewValue1A(ssa.OpVarLive, types.TypeMem, argTemp, s.defvars[s.f.Entry.ID][_addr_memVar]);
                addrArgTemp = s.entryNewValue2A(ssa.OpLocalAddr, types.NewPtr(argTemp.Type), argTemp, s.sp, s.defvars[s.f.Entry.ID][_addr_memVar]);

            }
            else
            { 
                // Special case if we're still in the entry block. We can't use
                // the above code, since s.defvars[s.f.Entry.ID] isn't defined
                // until we end the entry block with s.endBlock().
                s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, argTemp, s.mem(), false);
                s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, argTemp, s.mem(), false);
                addrArgTemp = s.newValue2Apos(ssa.OpLocalAddr, types.NewPtr(argTemp.Type), argTemp, s.sp, s.mem(), false);

            }

            if (types.Haspointers(t))
            { 
                // Since we may use this argTemp during exit depending on the
                // deferBits, we must define it unconditionally on entry.
                // Therefore, we must make sure it is zeroed out in the entry
                // block if it contains pointers, else GC may wrongly follow an
                // uninitialized pointer value.
                argTemp.Name.SetNeedzero(true);

            }

            if (!canSSA)
            {
                var a = s.addr(n);
                s.move(t, addrArgTemp, a);
                return _addr_addrArgTemp!;
            } 
            // We are storing to the stack, hence we can avoid the full checks in
            // storeType() (no write barrier) and do a simple store().
            s.store(t, addrArgTemp, val);
            return _addr_addrArgTemp!;

        }

        // openDeferExit generates SSA for processing all the open coded defers at exit.
        // The code involves loading deferBits, and checking each of the bits to see if
        // the corresponding defer statement was executed. For each bit that is turned
        // on, the associated defer call is made.
        private static void openDeferExit(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            var deferExit = s.f.NewBlock(ssa.BlockPlain);
            s.endBlock().AddEdgeTo(deferExit);
            s.startBlock(deferExit);
            s.lastDeferExit = deferExit;
            s.lastDeferCount = len(s.openDefers);
            var zeroval = s.constInt8(types.Types[TUINT8], 0L); 
            // Test for and run defers in reverse order
            for (var i = len(s.openDefers) - 1L; i >= 0L; i--)
            {
                var r = s.openDefers[i];
                var bCond = s.f.NewBlock(ssa.BlockPlain);
                var bEnd = s.f.NewBlock(ssa.BlockPlain);

                var deferBits = s.variable(_addr_deferBitsVar, types.Types[TUINT8]); 
                // Generate code to check if the bit associated with the current
                // defer is set.
                var bitval = s.constInt8(types.Types[TUINT8], 1L << (int)(uint(i)));
                var andval = s.newValue2(ssa.OpAnd8, types.Types[TUINT8], deferBits, bitval);
                var eqVal = s.newValue2(ssa.OpEq8, types.Types[TBOOL], andval, zeroval);
                var b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(eqVal);
                b.AddEdgeTo(bEnd);
                b.AddEdgeTo(bCond);
                bCond.AddEdgeTo(bEnd);
                s.startBlock(bCond); 

                // Clear this bit in deferBits and force store back to stack, so
                // we will not try to re-run this defer call if this defer call panics.
                var nbitval = s.newValue1(ssa.OpCom8, types.Types[TUINT8], bitval);
                var maskedval = s.newValue2(ssa.OpAnd8, types.Types[TUINT8], deferBits, nbitval);
                s.store(types.Types[TUINT8], s.deferBitsAddr, maskedval); 
                // Use this value for following tests, so we keep previous
                // bits cleared.
                s.vars[_addr_deferBitsVar] = maskedval; 

                // Generate code to call the function call of the defer, using the
                // closure/receiver/args that were stored in argtmps at the point
                // of the defer statement.
                var argStart = Ctxt.FixedFrameSize();
                var fn = r.n.Left;
                var stksize = fn.Type.ArgWidth();
                if (r.rcvr != null)
                { 
                    // rcvr in case of OCALLINTER
                    var v = s.load(r.rcvr.Type.Elem(), r.rcvr);
                    var addr = s.constOffPtrSP(s.f.Config.Types.UintptrPtr, argStart);
                    s.store(types.Types[TUINTPTR], addr, v);

                }

                foreach (var (j, argAddrVal) in r.argVals)
                {
                    var f = getParam(_addr_r.n, j);
                    var pt = types.NewPtr(f.Type);
                    addr = s.constOffPtrSP(pt, argStart + f.Offset);
                    if (!canSSAType(_addr_f.Type))
                    {
                        s.move(f.Type, addr, argAddrVal);
                    }
                    else
                    {
                        var argVal = s.load(f.Type, argAddrVal);
                        s.storeType(f.Type, addr, argVal, 0L, false);
                    }

                }
                ptr<ssa.Value> call;
                if (r.closure != null)
                {
                    v = s.load(r.closure.Type.Elem(), r.closure);
                    s.maybeNilCheckClosure(v, callDefer);
                    var codeptr = s.rawLoad(types.Types[TUINTPTR], v);
                    call = s.newValue3(ssa.OpClosureCall, types.TypeMem, codeptr, v, s.mem());
                }
                else
                { 
                    // Do a static call if the original call was a static function or method
                    call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, fn.Sym.Linksym(), s.mem());

                }

                call.AuxInt = stksize;
                s.vars[_addr_memVar] = call; 
                // Make sure that the stack slots with pointers are kept live
                // through the call (which is a pre-emption point). Also, we will
                // use the first call of the last defer exit to compute liveness
                // for the deferreturn, so we want all stack slots to be live.
                if (r.closureNode != null)
                {
                    s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, r.closureNode, s.mem(), false);
                }

                if (r.rcvrNode != null)
                {
                    if (types.Haspointers(r.rcvrNode.Type))
                    {
                        s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, r.rcvrNode, s.mem(), false);
                    }

                }

                foreach (var (_, argNode) in r.argNodes)
                {
                    if (types.Haspointers(argNode.Type))
                    {
                        s.vars[_addr_memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, argNode, s.mem(), false);
                    }

                }
                if (i == len(s.openDefers) - 1L)
                { 
                    // Record the call of the first defer. This will be used
                    // to set liveness info for the deferreturn (which is also
                    // used for any location that causes a runtime panic)
                    s.f.LastDeferExit = call;

                }

                s.endBlock();
                s.startBlock(bEnd);

            }


        }

        // Calls the function n using the specified call type.
        // Returns the address of the return value (or nil if none).
        private static ptr<ssa.Value> call(this ptr<state> _addr_s, ptr<Node> _addr_n, callKind k)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            ptr<types.Sym> sym; // target symbol (if static)
            ptr<ssa.Value> closure; // ptr to closure to run (if dynamic)
            ptr<ssa.Value> codeptr; // ptr to target code (if dynamic)
            ptr<ssa.Value> rcvr; // receiver to set
            var fn = n.Left;

            if (n.Op == OCALLFUNC) 
                if (k == callNormal && fn.Op == ONAME && fn.Class() == PFUNC)
                {
                    sym = fn.Sym;
                    break;
                }

                closure = s.expr(fn);
                if (k != callDefer && k != callDeferStack)
                { 
                    // Deferred nil function needs to panic when the function is invoked,
                    // not the point of defer statement.
                    s.maybeNilCheckClosure(closure, k);

                }

            else if (n.Op == OCALLMETH) 
                if (fn.Op != ODOTMETH)
                {
                    s.Fatalf("OCALLMETH: n.Left not an ODOTMETH: %v", fn);
                }

                if (k == callNormal)
                {
                    sym = fn.Sym;
                    break;
                }

                closure = s.getMethodClosure(fn); 
                // Note: receiver is already present in n.Rlist, so we don't
                // want to set it here.
            else if (n.Op == OCALLINTER) 
                if (fn.Op != ODOTINTER)
                {
                    s.Fatalf("OCALLINTER: n.Left not an ODOTINTER: %v", fn.Op);
                }

                ptr<ssa.Value> iclosure;
                iclosure, rcvr = s.getClosureAndRcvr(fn);
                if (k == callNormal)
                {
                    codeptr = s.load(types.Types[TUINTPTR], iclosure);
                }
                else
                {
                    closure = addr(iclosure);
                }

                        dowidth(fn.Type);
            var stksize = fn.Type.ArgWidth(); // includes receiver, args, and results

            // Run all assignments of temps.
            // The temps are introduced to avoid overwriting argument
            // slots when arguments themselves require function calls.
            s.stmtList(n.List);

            ptr<ssa.Value> call;
            if (k == callDeferStack)
            { 
                // Make a defer struct d on the stack.
                var t = deferstruct(stksize);
                var d = tempAt(n.Pos, s.curfn, t);

                s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, d, s.mem());
                var addr = s.addr(d); 

                // Must match reflect.go:deferstruct and src/runtime/runtime2.go:_defer.
                // 0: siz
                s.store(types.Types[TUINT32], s.newValue1I(ssa.OpOffPtr, types.Types[TUINT32].PtrTo(), t.FieldOff(0L), addr), s.constInt32(types.Types[TUINT32], int32(stksize))); 
                // 1: started, set in deferprocStack
                // 2: heap, set in deferprocStack
                // 3: openDefer
                // 4: sp, set in deferprocStack
                // 5: pc, set in deferprocStack
                // 6: fn
                s.store(closure.Type, s.newValue1I(ssa.OpOffPtr, closure.Type.PtrTo(), t.FieldOff(6L), addr), closure); 
                // 7: panic, set in deferprocStack
                // 8: link, set in deferprocStack
                // 9: framepc
                // 10: varp
                // 11: fd

                // Then, store all the arguments of the defer call.
                var ft = fn.Type;
                var off = t.FieldOff(12L);
                var args = n.Rlist.Slice(); 

                // Set receiver (for interface calls). Always a pointer.
                if (rcvr != null)
                {
                    var p = s.newValue1I(ssa.OpOffPtr, ft.Recv().Type.PtrTo(), off, addr);
                    s.store(types.Types[TUINTPTR], p, rcvr);
                } 
                // Set receiver (for method calls).
                if (n.Op == OCALLMETH)
                {
                    var f = ft.Recv();
                    s.storeArgWithBase(args[0L], f.Type, addr, off + f.Offset);
                    args = args[1L..];
                } 
                // Set other args.
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in ft.Params().Fields().Slice())
                    {
                        f = __f;
                        s.storeArgWithBase(args[0L], f.Type, addr, off + f.Offset);
                        args = args[1L..];
                    }
            else
 

                    // Call runtime.deferprocStack with pointer to _defer record.

                    f = f__prev1;
                }

                var arg0 = s.constOffPtrSP(types.Types[TUINTPTR], Ctxt.FixedFrameSize());
                s.store(types.Types[TUINTPTR], arg0, addr);
                call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, deferprocStack, s.mem());
                if (stksize < int64(Widthptr))
                { 
                    // We need room for both the call to deferprocStack and the call to
                    // the deferred function.
                    stksize = int64(Widthptr);

                }

                call.AuxInt = stksize;

            }            { 
                // Store arguments to stack, including defer/go arguments and receiver for method calls.
                // These are written in SP-offset order.
                var argStart = Ctxt.FixedFrameSize(); 
                // Defer/go args.
                if (k != callNormal)
                { 
                    // Write argsize and closure (args to newproc/deferproc).
                    var argsize = s.constInt32(types.Types[TUINT32], int32(stksize));
                    addr = s.constOffPtrSP(s.f.Config.Types.UInt32Ptr, argStart);
                    s.store(types.Types[TUINT32], addr, argsize);
                    addr = s.constOffPtrSP(s.f.Config.Types.UintptrPtr, argStart + int64(Widthptr));
                    s.store(types.Types[TUINTPTR], addr, closure);
                    stksize += 2L * int64(Widthptr);
                    argStart += 2L * int64(Widthptr);

                } 

                // Set receiver (for interface calls).
                if (rcvr != null)
                {
                    addr = s.constOffPtrSP(s.f.Config.Types.UintptrPtr, argStart);
                    s.store(types.Types[TUINTPTR], addr, rcvr);
                } 

                // Write args.
                t = n.Left.Type;
                args = n.Rlist.Slice();
                if (n.Op == OCALLMETH)
                {
                    f = t.Recv();
                    s.storeArg(args[0L], f.Type, argStart + f.Offset);
                    args = args[1L..];
                }

                foreach (var (i, n) in args)
                {
                    f = t.Params().Field(i);
                    s.storeArg(n, f.Type, argStart + f.Offset);
                } 

                // call target

                if (k == callDefer) 
                    call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, deferproc, s.mem());
                else if (k == callGo) 
                    call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, newproc, s.mem());
                else if (closure != null) 
                    // rawLoad because loading the code pointer from a
                    // closure is always safe, but IsSanitizerSafeAddr
                    // can't always figure that out currently, and it's
                    // critical that we not clobber any arguments already
                    // stored onto the stack.
                    codeptr = s.rawLoad(types.Types[TUINTPTR], closure);
                    call = s.newValue3(ssa.OpClosureCall, types.TypeMem, codeptr, closure, s.mem());
                else if (codeptr != null) 
                    call = s.newValue2(ssa.OpInterCall, types.TypeMem, codeptr, s.mem());
                else if (sym != null) 
                    call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, sym.Linksym(), s.mem());
                else 
                    s.Fatalf("bad call type %v %v", n.Op, n);
                                call.AuxInt = stksize; // Call operations carry the argsize of the callee along with them
            }

            s.vars[_addr_memVar] = call; 

            // Finish block for defers
            if (k == callDefer || k == callDeferStack)
            {
                var b = s.endBlock();
                b.Kind = ssa.BlockDefer;
                b.SetControl(call);
                var bNext = s.f.NewBlock(ssa.BlockPlain);
                b.AddEdgeTo(bNext); 
                // Add recover edge to exit code.
                var r = s.f.NewBlock(ssa.BlockPlain);
                s.startBlock(r);
                s.exit();
                b.AddEdgeTo(r);
                b.Likely = ssa.BranchLikely;
                s.startBlock(bNext);

            }

            var res = n.Left.Type.Results();
            if (res.NumFields() == 0L || k != callNormal)
            { 
                // call has no return value. Continue with the next statement.
                return _addr_null!;

            }

            var fp = res.Field(0L);
            return _addr_s.constOffPtrSP(types.NewPtr(fp.Type), fp.Offset + Ctxt.FixedFrameSize())!;

        }

        // maybeNilCheckClosure checks if a nil check of a closure is needed in some
        // architecture-dependent situations and, if so, emits the nil check.
        private static void maybeNilCheckClosure(this ptr<state> _addr_s, ptr<ssa.Value> _addr_closure, callKind k)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Value closure = ref _addr_closure.val;

            if (thearch.LinkArch.Family == sys.Wasm || objabi.GOOS == "aix" && k != callGo)
            { 
                // On AIX, the closure needs to be verified as fn can be nil, except if it's a call go. This needs to be handled by the runtime to have the "go of nil func value" error.
                // TODO(neelance): On other architectures this should be eliminated by the optimization steps
                s.nilCheck(closure);

            }

        }

        // getMethodClosure returns a value representing the closure for a method call
        private static ptr<ssa.Value> getMethodClosure(this ptr<state> _addr_s, ptr<Node> _addr_fn)
        {
            ref state s = ref _addr_s.val;
            ref Node fn = ref _addr_fn.val;
 
            // Make a name n2 for the function.
            // fn.Sym might be sync.(*Mutex).Unlock.
            // Make a PFUNC node out of that, then evaluate it.
            // We get back an SSA value representing &sync.(*Mutex).Unlock·f.
            // We can then pass that to defer or go.
            var n2 = newnamel(fn.Pos, fn.Sym);
            n2.Name.Curfn = s.curfn;
            n2.SetClass(PFUNC); 
            // n2.Sym already existed, so it's already marked as a function.
            n2.Pos = fn.Pos;
            n2.Type = types.Types[TUINT8]; // dummy type for a static closure. Could use runtime.funcval if we had it.
            return _addr_s.expr(n2)!;

        }

        // getClosureAndRcvr returns values for the appropriate closure and receiver of an
        // interface call
        private static (ptr<ssa.Value>, ptr<ssa.Value>) getClosureAndRcvr(this ptr<state> _addr_s, ptr<Node> _addr_fn)
        {
            ptr<ssa.Value> _p0 = default!;
            ptr<ssa.Value> _p0 = default!;
            ref state s = ref _addr_s.val;
            ref Node fn = ref _addr_fn.val;

            var i = s.expr(fn.Left);
            var itab = s.newValue1(ssa.OpITab, types.Types[TUINTPTR], i);
            s.nilCheck(itab);
            var itabidx = fn.Xoffset + 2L * int64(Widthptr) + 8L; // offset of fun field in runtime.itab
            var closure = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.UintptrPtr, itabidx, itab);
            var rcvr = s.newValue1(ssa.OpIData, types.Types[TUINTPTR], i);
            return (_addr_closure!, _addr_rcvr!);

        }

        // etypesign returns the signed-ness of e, for integer/pointer etypes.
        // -1 means signed, +1 means unsigned, 0 means non-integer/non-pointer.
        private static sbyte etypesign(types.EType e)
        {

            if (e == TINT8 || e == TINT16 || e == TINT32 || e == TINT64 || e == TINT) 
                return -1L;
            else if (e == TUINT8 || e == TUINT16 || e == TUINT32 || e == TUINT64 || e == TUINT || e == TUINTPTR || e == TUNSAFEPTR) 
                return +1L;
                        return 0L;

        }

        // addr converts the address of the expression n to SSA, adds it to s and returns the SSA result.
        // The value that the returned Value represents is guaranteed to be non-nil.
        private static ptr<ssa.Value> addr(this ptr<state> _addr_s, ptr<Node> _addr_n) => func((defer, _, __) =>
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (n.Op != ONAME)
            {
                s.pushLine(n.Pos);
                defer(s.popLine());
            }

            var t = types.NewPtr(n.Type);

            if (n.Op == ONAME) 

                if (n.Class() == PEXTERN) 
                    // global variable
                    var v = s.entryNewValue1A(ssa.OpAddr, t, n.Sym.Linksym(), s.sb); 
                    // TODO: Make OpAddr use AuxInt as well as Aux.
                    if (n.Xoffset != 0L)
                    {
                        v = s.entryNewValue1I(ssa.OpOffPtr, v.Type, n.Xoffset, v);
                    }

                    return _addr_v!;
                else if (n.Class() == PPARAM) 
                    // parameter slot
                    v = s.decladdrs[n];
                    if (v != null)
                    {
                        return _addr_v!;
                    }

                    if (n == nodfp)
                    { 
                        // Special arg that points to the frame pointer (Used by ORECOVER).
                        return _addr_s.entryNewValue2A(ssa.OpLocalAddr, t, n, s.sp, s.startmem)!;

                    }

                    s.Fatalf("addr of undeclared ONAME %v. declared: %v", n, s.decladdrs);
                    return _addr_null!;
                else if (n.Class() == PAUTO) 
                    return _addr_s.newValue2Apos(ssa.OpLocalAddr, t, n, s.sp, s.mem(), !n.IsAutoTmp())!;
                else if (n.Class() == PPARAMOUT) // Same as PAUTO -- cannot generate LEA early.
                    // ensure that we reuse symbols for out parameters so
                    // that cse works on their addresses
                    return _addr_s.newValue2Apos(ssa.OpLocalAddr, t, n, s.sp, s.mem(), true)!;
                else 
                    s.Fatalf("variable address class %v not implemented", n.Class());
                    return _addr_null!;
                            else if (n.Op == ORESULT) 
                // load return from callee
                return _addr_s.constOffPtrSP(t, n.Xoffset)!;
            else if (n.Op == OINDEX) 
                if (n.Left.Type.IsSlice())
                {
                    var a = s.expr(n.Left);
                    var i = s.expr(n.Right);
                    var len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], a);
                    i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
                    var p = s.newValue1(ssa.OpSlicePtr, t, a);
                    return _addr_s.newValue2(ssa.OpPtrIndex, t, p, i)!;
                }
                else
                { // array
                    a = s.addr(n.Left);
                    i = s.expr(n.Right);
                    len = s.constInt(types.Types[TINT], n.Left.Type.NumElem());
                    i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
                    return _addr_s.newValue2(ssa.OpPtrIndex, types.NewPtr(n.Left.Type.Elem()), a, i)!;

                }

            else if (n.Op == ODEREF) 
                return _addr_s.exprPtr(n.Left, n.Bounded(), n.Pos)!;
            else if (n.Op == ODOT) 
                p = s.addr(n.Left);
                return _addr_s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, p)!;
            else if (n.Op == ODOTPTR) 
                p = s.exprPtr(n.Left, n.Bounded(), n.Pos);
                return _addr_s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, p)!;
            else if (n.Op == OCLOSUREVAR) 
                return _addr_s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, s.entryNewValue0(ssa.OpGetClosurePtr, s.f.Config.Types.BytePtr))!;
            else if (n.Op == OCONVNOP) 
                var addr = s.addr(n.Left);
                return _addr_s.newValue1(ssa.OpCopy, t, addr)!; // ensure that addr has the right type
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH) 
                return _addr_s.call(n, callNormal)!;
            else if (n.Op == ODOTTYPE) 
                var (v, _) = s.dottype(n, false);
                if (v.Op != ssa.OpLoad)
                {
                    s.Fatalf("dottype of non-load");
                }

                if (v.Args[1L] != s.mem())
                {
                    s.Fatalf("memory no longer live from dottype load");
                }

                return _addr_v.Args[0L]!;
            else 
                s.Fatalf("unhandled addr %v", n.Op);
                return _addr_null!;
            
        });

        // canSSA reports whether n is SSA-able.
        // n must be an ONAME (or an ODOT sequence with an ONAME base).
        private static bool canSSA(this ptr<state> _addr_s, ptr<Node> _addr_n)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (Debug['N'] != 0L)
            {
                return false;
            }

            while (n.Op == ODOT || (n.Op == OINDEX && n.Left.Type.IsArray()))
            {
                n = n.Left;
            }

            if (n.Op != ONAME)
            {
                return false;
            }

            if (n.Name.Addrtaken())
            {
                return false;
            }

            if (n.isParamHeapCopy())
            {
                return false;
            }

            if (n.Class() == PAUTOHEAP)
            {
                s.Fatalf("canSSA of PAUTOHEAP %v", n);
            }


            if (n.Class() == PEXTERN) 
                return false;
            else if (n.Class() == PPARAMOUT) 
                if (s.hasdefer)
                { 
                    // TODO: handle this case? Named return values must be
                    // in memory so that the deferred function can see them.
                    // Maybe do: if !strings.HasPrefix(n.String(), "~") { return false }
                    // Or maybe not, see issue 18860.  Even unnamed return values
                    // must be written back so if a defer recovers, the caller can see them.
                    return false;

                }

                if (s.cgoUnsafeArgs)
                { 
                    // Cgo effectively takes the address of all result args,
                    // but the compiler can't see that.
                    return false;

                }

                        if (n.Class() == PPARAM && n.Sym != null && n.Sym.Name == ".this")
            { 
                // wrappers generated by genwrapper need to update
                // the .this pointer in place.
                // TODO: treat as a PPARMOUT?
                return false;

            }

            return canSSAType(_addr_n.Type); 
            // TODO: try to make more variables SSAable?
        }

        // canSSA reports whether variables of type t are SSA-able.
        private static bool canSSAType(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            dowidth(t);
            if (t.Width > int64(4L * Widthptr))
            { 
                // 4*Widthptr is an arbitrary constant. We want it
                // to be at least 3*Widthptr so slices can be registerized.
                // Too big and we'll introduce too much register pressure.
                return false;

            }


            if (t.Etype == TARRAY) 
                // We can't do larger arrays because dynamic indexing is
                // not supported on SSA variables.
                // TODO: allow if all indexes are constant.
                if (t.NumElem() <= 1L)
                {
                    return canSSAType(_addr_t.Elem());
                }

                return false;
            else if (t.Etype == TSTRUCT) 
                if (t.NumFields() > ssa.MaxStruct)
                {
                    return false;
                }

                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (!canSSAType(_addr_t1.Type))
                    {
                        return false;
                    }

                }
                return true;
            else 
                return true;
            
        }

        // exprPtr evaluates n to a pointer and nil-checks it.
        private static ptr<ssa.Value> exprPtr(this ptr<state> _addr_s, ptr<Node> _addr_n, bool bounded, src.XPos lineno)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            var p = s.expr(n);
            if (bounded || n.NonNil())
            {
                if (s.f.Frontend().Debug_checknil() && lineno.Line() > 1L)
                {
                    s.f.Warnl(lineno, "removed nil check");
                }

                return _addr_p!;

            }

            s.nilCheck(p);
            return _addr_p!;

        }

        // nilCheck generates nil pointer checking code.
        // Used only for automatically inserted nil checks,
        // not for user code like 'x != nil'.
        private static void nilCheck(this ptr<state> _addr_s, ptr<ssa.Value> _addr_ptr)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Value ptr = ref _addr_ptr.val;

            if (disable_checknil != 0L || s.curfn.Func.NilCheckDisabled())
            {
                return ;
            }

            s.newValue2(ssa.OpNilCheck, types.TypeVoid, ptr, s.mem());

        }

        // boundsCheck generates bounds checking code. Checks if 0 <= idx <[=] len, branches to exit if not.
        // Starts a new block on return.
        // On input, len must be converted to full int width and be nonnegative.
        // Returns idx converted to full int width.
        // If bounded is true then caller guarantees the index is not out of bounds
        // (but boundsCheck will still extend the index to full int width).
        private static ptr<ssa.Value> boundsCheck(this ptr<state> _addr_s, ptr<ssa.Value> _addr_idx, ptr<ssa.Value> _addr_len, ssa.BoundsKind kind, bool bounded)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Value idx = ref _addr_idx.val;
            ref ssa.Value len = ref _addr_len.val;

            idx = s.extendIndex(idx, len, kind, bounded);

            if (bounded || Debug['B'] != 0L)
            { 
                // If bounded or bounds checking is flag-disabled, then no check necessary,
                // just return the extended index.
                //
                // Here, bounded == true if the compiler generated the index itself,
                // such as in the expansion of a slice initializer. These indexes are
                // compiler-generated, not Go program variables, so they cannot be
                // attacker-controlled, so we can omit Spectre masking as well.
                //
                // Note that we do not want to omit Spectre masking in code like:
                //
                //    if 0 <= i && i < len(x) {
                //        use(x[i])
                //    }
                //
                // Lucky for us, bounded==false for that code.
                // In that case (handled below), we emit a bound check (and Spectre mask)
                // and then the prove pass will remove the bounds check.
                // In theory the prove pass could potentially remove certain
                // Spectre masks, but it's very delicate and probably better
                // to be conservative and leave them all in.
                return _addr_idx!;

            }

            var bNext = s.f.NewBlock(ssa.BlockPlain);
            var bPanic = s.f.NewBlock(ssa.BlockExit);

            if (!idx.Type.IsSigned())
            {

                if (kind == ssa.BoundsIndex) 
                    kind = ssa.BoundsIndexU;
                else if (kind == ssa.BoundsSliceAlen) 
                    kind = ssa.BoundsSliceAlenU;
                else if (kind == ssa.BoundsSliceAcap) 
                    kind = ssa.BoundsSliceAcapU;
                else if (kind == ssa.BoundsSliceB) 
                    kind = ssa.BoundsSliceBU;
                else if (kind == ssa.BoundsSlice3Alen) 
                    kind = ssa.BoundsSlice3AlenU;
                else if (kind == ssa.BoundsSlice3Acap) 
                    kind = ssa.BoundsSlice3AcapU;
                else if (kind == ssa.BoundsSlice3B) 
                    kind = ssa.BoundsSlice3BU;
                else if (kind == ssa.BoundsSlice3C) 
                    kind = ssa.BoundsSlice3CU;
                
            }

            ptr<ssa.Value> cmp;
            if (kind == ssa.BoundsIndex || kind == ssa.BoundsIndexU)
            {
                cmp = s.newValue2(ssa.OpIsInBounds, types.Types[TBOOL], idx, len);
            }
            else
            {
                cmp = s.newValue2(ssa.OpIsSliceInBounds, types.Types[TBOOL], idx, len);
            }

            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;
            b.AddEdgeTo(bNext);
            b.AddEdgeTo(bPanic);

            s.startBlock(bPanic);
            if (thearch.LinkArch.Family == sys.Wasm)
            { 
                // TODO(khr): figure out how to do "register" based calling convention for bounds checks.
                // Should be similar to gcWriteBarrier, but I can't make it work.
                s.rtcall(BoundsCheckFunc[kind], false, null, idx, len);

            }
            else
            {
                var mem = s.newValue3I(ssa.OpPanicBounds, types.TypeMem, int64(kind), idx, len, s.mem());
                s.endBlock().SetControl(mem);
            }

            s.startBlock(bNext); 

            // In Spectre index mode, apply an appropriate mask to avoid speculative out-of-bounds accesses.
            if (spectreIndex)
            {
                var op = ssa.OpSpectreIndex;
                if (kind != ssa.BoundsIndex && kind != ssa.BoundsIndexU)
                {
                    op = ssa.OpSpectreSliceIndex;
                }

                idx = s.newValue2(op, types.Types[TINT], idx, len);

            }

            return _addr_idx!;

        }

        // If cmp (a bool) is false, panic using the given function.
        private static void check(this ptr<state> _addr_s, ptr<ssa.Value> _addr_cmp, ptr<obj.LSym> _addr_fn)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Value cmp = ref _addr_cmp.val;
            ref obj.LSym fn = ref _addr_fn.val;

            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;
            var bNext = s.f.NewBlock(ssa.BlockPlain);
            var line = s.peekPos();
            var pos = Ctxt.PosTable.Pos(line);
            funcLine fl = new funcLine(f:fn,base:pos.Base(),line:pos.Line());
            var bPanic = s.panics[fl];
            if (bPanic == null)
            {
                bPanic = s.f.NewBlock(ssa.BlockPlain);
                s.panics[fl] = bPanic;
                s.startBlock(bPanic); 
                // The panic call takes/returns memory to ensure that the right
                // memory state is observed if the panic happens.
                s.rtcall(fn, false, null);

            }

            b.AddEdgeTo(bNext);
            b.AddEdgeTo(bPanic);
            s.startBlock(bNext);

        }

        private static ptr<ssa.Value> intDivide(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_a, ptr<ssa.Value> _addr_b)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value a = ref _addr_a.val;
            ref ssa.Value b = ref _addr_b.val;

            var needcheck = true;

            if (b.Op == ssa.OpConst8 || b.Op == ssa.OpConst16 || b.Op == ssa.OpConst32 || b.Op == ssa.OpConst64) 
                if (b.AuxInt != 0L)
                {
                    needcheck = false;
                }

                        if (needcheck)
            { 
                // do a size-appropriate check for zero
                var cmp = s.newValue2(s.ssaOp(ONE, n.Type), types.Types[TBOOL], b, s.zeroVal(n.Type));
                s.check(cmp, panicdivide);

            }

            return _addr_s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b)!;

        }

        // rtcall issues a call to the given runtime function fn with the listed args.
        // Returns a slice of results of the given result types.
        // The call is added to the end of the current block.
        // If returns is false, the block is marked as an exit block.
        private static slice<ptr<ssa.Value>> rtcall(this ptr<state> _addr_s, ptr<obj.LSym> _addr_fn, bool returns, slice<ptr<types.Type>> results, params ptr<ptr<ssa.Value>>[] _addr_args)
        {
            args = args.Clone();
            ref state s = ref _addr_s.val;
            ref obj.LSym fn = ref _addr_fn.val;
            ref ssa.Value args = ref _addr_args.val;
 
            // Write args to the stack
            var off = Ctxt.FixedFrameSize();
            foreach (var (_, arg) in args)
            {
                var t = arg.Type;
                off = Rnd(off, t.Alignment());
                var ptr = s.constOffPtrSP(t.PtrTo(), off);
                var size = t.Size();
                s.store(t, ptr, arg);
                off += size;
            }
            off = Rnd(off, int64(Widthreg)); 

            // Issue call
            var call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, fn, s.mem());
            s.vars[_addr_memVar] = call;

            if (!returns)
            { 
                // Finish block
                var b = s.endBlock();
                b.Kind = ssa.BlockExit;
                b.SetControl(call);
                call.AuxInt = off - Ctxt.FixedFrameSize();
                if (len(results) > 0L)
                {
                    s.Fatalf("panic call can't have results");
                }

                return null;

            } 

            // Load results
            var res = make_slice<ptr<ssa.Value>>(len(results));
            {
                var t__prev1 = t;

                foreach (var (__i, __t) in results)
                {
                    i = __i;
                    t = __t;
                    off = Rnd(off, t.Alignment());
                    ptr = s.constOffPtrSP(types.NewPtr(t), off);
                    res[i] = s.load(t, ptr);
                    off += t.Size();
                }

                t = t__prev1;
            }

            off = Rnd(off, int64(Widthptr)); 

            // Remember how much callee stack space we needed.
            call.AuxInt = off;

            return res;

        }

        // do *left = right for type t.
        private static void storeType(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right, skipMask skip, bool leftIsStmt)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value left = ref _addr_left.val;
            ref ssa.Value right = ref _addr_right.val;

            s.instrument(t, left, true);

            if (skip == 0L && (!types.Haspointers(t) || ssa.IsStackAddr(left)))
            { 
                // Known to not have write barrier. Store the whole type.
                s.vars[_addr_memVar] = s.newValue3Apos(ssa.OpStore, types.TypeMem, t, left, right, s.mem(), leftIsStmt);
                return ;

            } 

            // store scalar fields first, so write barrier stores for
            // pointer fields can be grouped together, and scalar values
            // don't need to be live across the write barrier call.
            // TODO: if the writebarrier pass knows how to reorder stores,
            // we can do a single store here as long as skip==0.
            s.storeTypeScalars(t, left, right, skip);
            if (skip & skipPtr == 0L && types.Haspointers(t))
            {
                s.storeTypePtrs(t, left, right);
            }

        }

        // do *left = right for all scalar (non-pointer) parts of t.
        private static void storeTypeScalars(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right, skipMask skip)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value left = ref _addr_left.val;
            ref ssa.Value right = ref _addr_right.val;


            if (t.IsBoolean() || t.IsInteger() || t.IsFloat() || t.IsComplex()) 
                s.store(t, left, right);
            else if (t.IsPtrShaped())             else if (t.IsString()) 
                if (skip & skipLen != 0L)
                {
                    return ;
                }

                var len = s.newValue1(ssa.OpStringLen, types.Types[TINT], right);
                var lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
                s.store(types.Types[TINT], lenAddr, len);
            else if (t.IsSlice()) 
                if (skip & skipLen == 0L)
                {
                    len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], right);
                    lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
                    s.store(types.Types[TINT], lenAddr, len);
                }

                if (skip & skipCap == 0L)
                {
                    var cap = s.newValue1(ssa.OpSliceCap, types.Types[TINT], right);
                    var capAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, 2L * s.config.PtrSize, left);
                    s.store(types.Types[TINT], capAddr, cap);
                }

            else if (t.IsInterface()) 
                // itab field doesn't need a write barrier (even though it is a pointer).
                var itab = s.newValue1(ssa.OpITab, s.f.Config.Types.BytePtr, right);
                s.store(types.Types[TUINTPTR], left, itab);
            else if (t.IsStruct()) 
                var n = t.NumFields();
                for (long i = 0L; i < n; i++)
                {
                    var ft = t.FieldType(i);
                    var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
                    var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
                    s.storeTypeScalars(ft, addr, val, 0L);
                }
            else if (t.IsArray() && t.NumElem() == 0L)             else if (t.IsArray() && t.NumElem() == 1L) 
                s.storeTypeScalars(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0L, right), 0L);
            else 
                s.Fatalf("bad write barrier type %v", t);
            
        }

        // do *left = right for all pointer parts of t.
        private static void storeTypePtrs(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right)
        {
            ref state s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value left = ref _addr_left.val;
            ref ssa.Value right = ref _addr_right.val;


            if (t.IsPtrShaped()) 
                s.store(t, left, right);
            else if (t.IsString()) 
                var ptr = s.newValue1(ssa.OpStringPtr, s.f.Config.Types.BytePtr, right);
                s.store(s.f.Config.Types.BytePtr, left, ptr);
            else if (t.IsSlice()) 
                var elType = types.NewPtr(t.Elem());
                ptr = s.newValue1(ssa.OpSlicePtr, elType, right);
                s.store(elType, left, ptr);
            else if (t.IsInterface()) 
                // itab field is treated as a scalar.
                var idata = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, right);
                var idataAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.BytePtrPtr, s.config.PtrSize, left);
                s.store(s.f.Config.Types.BytePtr, idataAddr, idata);
            else if (t.IsStruct()) 
                var n = t.NumFields();
                for (long i = 0L; i < n; i++)
                {
                    var ft = t.FieldType(i);
                    if (!types.Haspointers(ft))
                    {
                        continue;
                    }

                    var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
                    var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
                    s.storeTypePtrs(ft, addr, val);

                }
            else if (t.IsArray() && t.NumElem() == 0L)             else if (t.IsArray() && t.NumElem() == 1L) 
                s.storeTypePtrs(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0L, right));
            else 
                s.Fatalf("bad write barrier type %v", t);
            
        }

        private static void storeArg(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<types.Type> _addr_t, long off)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            s.storeArgWithBase(n, t, s.sp, off);
        }

        private static void storeArgWithBase(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_@base, long off)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;
            ref ssa.Value @base = ref _addr_@base.val;

            var pt = types.NewPtr(t);
            ptr<ssa.Value> addr;
            if (base == s.sp)
            { 
                // Use special routine that avoids allocation on duplicate offsets.
                addr = s.constOffPtrSP(pt, off);

            }
            else
            {
                addr = s.newValue1I(ssa.OpOffPtr, pt, off, base);
            }

            if (!canSSAType(_addr_t))
            {
                var a = s.addr(n);
                s.move(t, addr, a);
                return ;
            }

            a = s.expr(n);
            s.storeType(t, addr, a, 0L, false);

        }

        // slice computes the slice v[i:j:k] and returns ptr, len, and cap of result.
        // i,j,k may be nil, in which case they are set to their default value.
        // v may be a slice, string or pointer to an array.
        private static (ptr<ssa.Value>, ptr<ssa.Value>, ptr<ssa.Value>) slice(this ptr<state> _addr_s, ptr<ssa.Value> _addr_v, ptr<ssa.Value> _addr_i, ptr<ssa.Value> _addr_j, ptr<ssa.Value> _addr_k, bool bounded)
        {
            ptr<ssa.Value> p = default!;
            ptr<ssa.Value> l = default!;
            ptr<ssa.Value> c = default!;
            ref state s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;
            ref ssa.Value i = ref _addr_i.val;
            ref ssa.Value j = ref _addr_j.val;
            ref ssa.Value k = ref _addr_k.val;

            var t = v.Type;
            ptr<ssa.Value> ptr;            ptr<ssa.Value> len;            ptr<ssa.Value> cap;


            if (t.IsSlice()) 
                ptr = s.newValue1(ssa.OpSlicePtr, types.NewPtr(t.Elem()), v);
                len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], v);
                cap = s.newValue1(ssa.OpSliceCap, types.Types[TINT], v);
            else if (t.IsString()) 
                ptr = s.newValue1(ssa.OpStringPtr, types.NewPtr(types.Types[TUINT8]), v);
                len = s.newValue1(ssa.OpStringLen, types.Types[TINT], v);
                cap = addr(len);
            else if (t.IsPtr()) 
                if (!t.Elem().IsArray())
                {
                    s.Fatalf("bad ptr to array in slice %v\n", t);
                }

                s.nilCheck(v);
                ptr = s.newValue1(ssa.OpCopy, types.NewPtr(t.Elem().Elem()), v);
                len = s.constInt(types.Types[TINT], t.Elem().NumElem());
                cap = addr(len);
            else 
                s.Fatalf("bad type in slice %v\n", t);
            // Set default values
            if (i == null)
            {
                i = s.constInt(types.Types[TINT], 0L);
            }

            if (j == null)
            {
                j = len;
            }

            var three = true;
            if (k == null)
            {
                three = false;
                k = cap;
            } 

            // Panic if slice indices are not in bounds.
            // Make sure we check these in reverse order so that we're always
            // comparing against a value known to be nonnegative. See issue 28797.
            if (three)
            {
                if (k != cap)
                {
                    var kind = ssa.BoundsSlice3Alen;
                    if (t.IsSlice())
                    {
                        kind = ssa.BoundsSlice3Acap;
                    }

                    k = s.boundsCheck(k, cap, kind, bounded);

                }

                if (j != k)
                {
                    j = s.boundsCheck(j, k, ssa.BoundsSlice3B, bounded);
                }

                i = s.boundsCheck(i, j, ssa.BoundsSlice3C, bounded);

            }
            else
            {
                if (j != k)
                {
                    kind = ssa.BoundsSliceAlen;
                    if (t.IsSlice())
                    {
                        kind = ssa.BoundsSliceAcap;
                    }

                    j = s.boundsCheck(j, k, kind, bounded);

                }

                i = s.boundsCheck(i, j, ssa.BoundsSliceB, bounded);

            } 

            // Word-sized integer operations.
            var subOp = s.ssaOp(OSUB, types.Types[TINT]);
            var mulOp = s.ssaOp(OMUL, types.Types[TINT]);
            var andOp = s.ssaOp(OAND, types.Types[TINT]); 

            // Calculate the length (rlen) and capacity (rcap) of the new slice.
            // For strings the capacity of the result is unimportant. However,
            // we use rcap to test if we've generated a zero-length slice.
            // Use length of strings for that.
            var rlen = s.newValue2(subOp, types.Types[TINT], j, i);
            var rcap = rlen;
            if (j != k && !t.IsString())
            {
                rcap = s.newValue2(subOp, types.Types[TINT], k, i);
            }

            if ((i.Op == ssa.OpConst64 || i.Op == ssa.OpConst32) && i.AuxInt == 0L)
            { 
                // No pointer arithmetic necessary.
                return (_addr_ptr!, _addr_rlen!, _addr_rcap!);

            } 

            // Calculate the base pointer (rptr) for the new slice.
            //
            // Generate the following code assuming that indexes are in bounds.
            // The masking is to make sure that we don't generate a slice
            // that points to the next object in memory. We cannot just set
            // the pointer to nil because then we would create a nil slice or
            // string.
            //
            //     rcap = k - i
            //     rlen = j - i
            //     rptr = ptr + (mask(rcap) & (i * stride))
            //
            // Where mask(x) is 0 if x==0 and -1 if x>0 and stride is the width
            // of the element type.
            var stride = s.constInt(types.Types[TINT], ptr.Type.Elem().Width); 

            // The delta is the number of bytes to offset ptr by.
            var delta = s.newValue2(mulOp, types.Types[TINT], i, stride); 

            // If we're slicing to the point where the capacity is zero,
            // zero out the delta.
            var mask = s.newValue1(ssa.OpSlicemask, types.Types[TINT], rcap);
            delta = s.newValue2(andOp, types.Types[TINT], delta, mask); 

            // Compute rptr = ptr + delta.
            var rptr = s.newValue2(ssa.OpAddPtr, ptr.Type, ptr, delta);

            return (_addr_rptr!, _addr_rlen!, _addr_rcap!);

        }

        private partial struct u642fcvtTab
        {
            public ssa.Op leq;
            public ssa.Op cvt2F;
            public ssa.Op and;
            public ssa.Op rsh;
            public ssa.Op or;
            public ssa.Op add;
            public Func<ptr<state>, ptr<types.Type>, long, ptr<ssa.Value>> one;
        }

        private static u642fcvtTab u64_f64 = new u642fcvtTab(leq:ssa.OpLeq64,cvt2F:ssa.OpCvt64to64F,and:ssa.OpAnd64,rsh:ssa.OpRsh64Ux64,or:ssa.OpOr64,add:ssa.OpAdd64F,one:(*state).constInt64,);

        private static u642fcvtTab u64_f32 = new u642fcvtTab(leq:ssa.OpLeq64,cvt2F:ssa.OpCvt64to32F,and:ssa.OpAnd64,rsh:ssa.OpRsh64Ux64,or:ssa.OpOr64,add:ssa.OpAdd32F,one:(*state).constInt64,);

        private static ptr<ssa.Value> uint64Tofloat64(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.uint64Tofloat(_addr_u64_f64, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> uint64Tofloat32(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.uint64Tofloat(_addr_u64_f32, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> uint64Tofloat(this ptr<state> _addr_s, ptr<u642fcvtTab> _addr_cvttab, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref u642fcvtTab cvttab = ref _addr_cvttab.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;
 
            // if x >= 0 {
            //    result = (floatY) x
            // } else {
            //       y = uintX(x) ; y = x & 1
            //       z = uintX(x) ; z = z >> 1
            //       z = z >> 1
            //       z = z | y
            //       result = floatY(z)
            //       result = result + result
            // }
            //
            // Code borrowed from old code generator.
            // What's going on: large 64-bit "unsigned" looks like
            // negative number to hardware's integer-to-float
            // conversion. However, because the mantissa is only
            // 63 bits, we don't need the LSB, so instead we do an
            // unsigned right shift (divide by two), convert, and
            // double. However, before we do that, we need to be
            // sure that we do not lose a "1" if that made the
            // difference in the resulting rounding. Therefore, we
            // preserve it, and OR (not ADD) it back in. The case
            // that matters is when the eleven discarded bits are
            // equal to 10000000001; that rounds up, and the 1 cannot
            // be lost else it would round down if the LSB of the
            // candidate mantissa is 0.
            var cmp = s.newValue2(cvttab.leq, types.Types[TBOOL], s.zeroVal(ft), x);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvt2F, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var one = cvttab.one(s, ft, 1L);
            var y = s.newValue2(cvttab.and, ft, x, one);
            var z = s.newValue2(cvttab.rsh, ft, x, one);
            z = s.newValue2(cvttab.or, ft, z, y);
            var a = s.newValue1(cvttab.cvt2F, tt, z);
            var a1 = s.newValue2(cvttab.add, tt, a, a);
            s.vars[n] = a1;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return _addr_s.variable(n, n.Type)!;

        }

        private partial struct u322fcvtTab
        {
            public ssa.Op cvtI2F;
            public ssa.Op cvtF2F;
        }

        private static u322fcvtTab u32_f64 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to64F,cvtF2F:ssa.OpCopy,);

        private static u322fcvtTab u32_f32 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to32F,cvtF2F:ssa.OpCvt64Fto32F,);

        private static ptr<ssa.Value> uint32Tofloat64(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.uint32Tofloat(_addr_u32_f64, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> uint32Tofloat32(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.uint32Tofloat(_addr_u32_f32, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> uint32Tofloat(this ptr<state> _addr_s, ptr<u322fcvtTab> _addr_cvttab, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref u322fcvtTab cvttab = ref _addr_cvttab.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;
 
            // if x >= 0 {
            //     result = floatY(x)
            // } else {
            //     result = floatY(float64(x) + (1<<32))
            // }
            var cmp = s.newValue2(ssa.OpLeq32, types.Types[TBOOL], s.zeroVal(ft), x);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvtI2F, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var a1 = s.newValue1(ssa.OpCvt32to64F, types.Types[TFLOAT64], x);
            var twoToThe32 = s.constFloat64(types.Types[TFLOAT64], float64(1L << (int)(32L)));
            var a2 = s.newValue2(ssa.OpAdd64F, types.Types[TFLOAT64], a1, twoToThe32);
            var a3 = s.newValue1(cvttab.cvtF2F, tt, a2);

            s.vars[n] = a3;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return _addr_s.variable(n, n.Type)!;

        }

        // referenceTypeBuiltin generates code for the len/cap builtins for maps and channels.
        private static ptr<ssa.Value> referenceTypeBuiltin(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;

            if (!n.Left.Type.IsMap() && !n.Left.Type.IsChan())
            {
                s.Fatalf("node must be a map or a channel");
            } 
            // if n == nil {
            //   return 0
            // } else {
            //   // len
            //   return *((*int)n)
            //   // cap
            //   return *(((*int)n)+1)
            // }
            var lenType = n.Type;
            var nilValue = s.constNil(types.Types[TUINTPTR]);
            var cmp = s.newValue2(ssa.OpEqPtr, types.Types[TBOOL], x, nilValue);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchUnlikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain); 

            // length/capacity of a nil map/chan is zero
            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            s.vars[n] = s.zeroVal(lenType);
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);

            if (n.Op == OLEN) 
                // length is stored in the first word for map/chan
                s.vars[n] = s.load(lenType, x);
            else if (n.Op == OCAP) 
                // capacity is stored in the second word for chan
                var sw = s.newValue1I(ssa.OpOffPtr, lenType.PtrTo(), lenType.Width, x);
                s.vars[n] = s.load(lenType, sw);
            else 
                s.Fatalf("op must be OLEN or OCAP");
                        s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return _addr_s.variable(n, lenType)!;

        }

        private partial struct f2uCvtTab
        {
            public ssa.Op ltf;
            public ssa.Op cvt2U;
            public ssa.Op subf;
            public ssa.Op or;
            public Func<ptr<state>, ptr<types.Type>, double, ptr<ssa.Value>> floatValue;
            public Func<ptr<state>, ptr<types.Type>, long, ptr<ssa.Value>> intValue;
            public ulong cutoff;
        }

        private static f2uCvtTab f32_u64 = new f2uCvtTab(ltf:ssa.OpLess32F,cvt2U:ssa.OpCvt32Fto64,subf:ssa.OpSub32F,or:ssa.OpOr64,floatValue:(*state).constFloat32,intValue:(*state).constInt64,cutoff:1<<63,);

        private static f2uCvtTab f64_u64 = new f2uCvtTab(ltf:ssa.OpLess64F,cvt2U:ssa.OpCvt64Fto64,subf:ssa.OpSub64F,or:ssa.OpOr64,floatValue:(*state).constFloat64,intValue:(*state).constInt64,cutoff:1<<63,);

        private static f2uCvtTab f32_u32 = new f2uCvtTab(ltf:ssa.OpLess32F,cvt2U:ssa.OpCvt32Fto32,subf:ssa.OpSub32F,or:ssa.OpOr32,floatValue:(*state).constFloat32,intValue:func(s*state,t*types.Type,vint64)*ssa.Value{returns.constInt32(t,int32(v))},cutoff:1<<31,);

        private static f2uCvtTab f64_u32 = new f2uCvtTab(ltf:ssa.OpLess64F,cvt2U:ssa.OpCvt64Fto32,subf:ssa.OpSub64F,or:ssa.OpOr32,floatValue:(*state).constFloat64,intValue:func(s*state,t*types.Type,vint64)*ssa.Value{returns.constInt32(t,int32(v))},cutoff:1<<31,);

        private static ptr<ssa.Value> float32ToUint64(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.floatToUint(_addr_f32_u64, n, x, ft, tt)!;
        }
        private static ptr<ssa.Value> float64ToUint64(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.floatToUint(_addr_f64_u64, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> float32ToUint32(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.floatToUint(_addr_f32_u32, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> float64ToUint32(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;

            return _addr_s.floatToUint(_addr_f64_u32, n, x, ft, tt)!;
        }

        private static ptr<ssa.Value> floatToUint(this ptr<state> _addr_s, ptr<f2uCvtTab> _addr_cvttab, ptr<Node> _addr_n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt)
        {
            ref state s = ref _addr_s.val;
            ref f2uCvtTab cvttab = ref _addr_cvttab.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value x = ref _addr_x.val;
            ref types.Type ft = ref _addr_ft.val;
            ref types.Type tt = ref _addr_tt.val;
 
            // cutoff:=1<<(intY_Size-1)
            // if x < floatX(cutoff) {
            //     result = uintY(x)
            // } else {
            //     y = x - floatX(cutoff)
            //     z = uintY(y)
            //     result = z | -(cutoff)
            // }
            var cutoff = cvttab.floatValue(s, ft, float64(cvttab.cutoff));
            var cmp = s.newValue2(cvttab.ltf, types.Types[TBOOL], x, cutoff);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvt2U, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var y = s.newValue2(cvttab.subf, ft, x, cutoff);
            y = s.newValue1(cvttab.cvt2U, tt, y);
            var z = cvttab.intValue(s, tt, int64(-cvttab.cutoff));
            var a1 = s.newValue2(cvttab.or, tt, y, z);
            s.vars[n] = a1;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return _addr_s.variable(n, n.Type)!;

        }

        // dottype generates SSA for a type assertion node.
        // commaok indicates whether to panic or return a bool.
        // If commaok is false, resok will be nil.
        private static (ptr<ssa.Value>, ptr<ssa.Value>) dottype(this ptr<state> _addr_s, ptr<Node> _addr_n, bool commaok)
        {
            ptr<ssa.Value> res = default!;
            ptr<ssa.Value> resok = default!;
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            var iface = s.expr(n.Left); // input interface
            var target = s.expr(n.Right); // target type
            var byteptr = s.f.Config.Types.BytePtr;

            if (n.Type.IsInterface())
            {
                if (n.Type.IsEmptyInterface())
                { 
                    // Converting to an empty interface.
                    // Input could be an empty or nonempty interface.
                    if (Debug_typeassert > 0L)
                    {
                        Warnl(n.Pos, "type assertion inlined");
                    } 

                    // Get itab/type field from input.
                    var itab = s.newValue1(ssa.OpITab, byteptr, iface); 
                    // Conversion succeeds iff that field is not nil.
                    var cond = s.newValue2(ssa.OpNeqPtr, types.Types[TBOOL], itab, s.constNil(byteptr));

                    if (n.Left.Type.IsEmptyInterface() && commaok)
                    { 
                        // Converting empty interface to empty interface with ,ok is just a nil check.
                        return (_addr_iface!, _addr_cond!);

                    } 

                    // Branch on nilness.
                    var b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(cond);
                    b.Likely = ssa.BranchLikely;
                    var bOk = s.f.NewBlock(ssa.BlockPlain);
                    var bFail = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bOk);
                    b.AddEdgeTo(bFail);

                    if (!commaok)
                    { 
                        // On failure, panic by calling panicnildottype.
                        s.startBlock(bFail);
                        s.rtcall(panicnildottype, false, null, target); 

                        // On success, return (perhaps modified) input interface.
                        s.startBlock(bOk);
                        if (n.Left.Type.IsEmptyInterface())
                        {
                            res = iface; // Use input interface unchanged.
                            return ;

                        } 
                        // Load type out of itab, build interface with existing idata.
                        var off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(Widthptr), itab);
                        var typ = s.load(byteptr, off);
                        var idata = s.newValue1(ssa.OpIData, n.Type, iface);
                        res = s.newValue2(ssa.OpIMake, n.Type, typ, idata);
                        return ;

                    }

                    s.startBlock(bOk); 
                    // nonempty -> empty
                    // Need to load type from itab
                    off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(Widthptr), itab);
                    s.vars[_addr_typVar] = s.load(byteptr, off);
                    s.endBlock(); 

                    // itab is nil, might as well use that as the nil result.
                    s.startBlock(bFail);
                    s.vars[_addr_typVar] = itab;
                    s.endBlock(); 

                    // Merge point.
                    var bEnd = s.f.NewBlock(ssa.BlockPlain);
                    bOk.AddEdgeTo(bEnd);
                    bFail.AddEdgeTo(bEnd);
                    s.startBlock(bEnd);
                    idata = s.newValue1(ssa.OpIData, n.Type, iface);
                    res = s.newValue2(ssa.OpIMake, n.Type, s.variable(_addr_typVar, byteptr), idata);
                    resok = cond;
                    delete(s.vars, _addr_typVar);
                    return ;

                } 
                // converting to a nonempty interface needs a runtime call.
                if (Debug_typeassert > 0L)
                {
                    Warnl(n.Pos, "type assertion not inlined");
                }

                if (n.Left.Type.IsEmptyInterface())
                {
                    if (commaok)
                    {
                        var call = s.rtcall(assertE2I2, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type, types.Types[TBOOL] }), target, iface);
                        return (_addr_call[0L]!, _addr_call[1L]!);
                    }

                    return (_addr_s.rtcall(assertE2I, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type }), target, iface)[0L]!, _addr_null!);

                }

                if (commaok)
                {
                    call = s.rtcall(assertI2I2, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type, types.Types[TBOOL] }), target, iface);
                    return (_addr_call[0L]!, _addr_call[1L]!);
                }

                return (_addr_s.rtcall(assertI2I, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type }), target, iface)[0L]!, _addr_null!);

            }

            if (Debug_typeassert > 0L)
            {
                Warnl(n.Pos, "type assertion inlined");
            } 

            // Converting to a concrete type.
            var direct = isdirectiface(n.Type);
            itab = s.newValue1(ssa.OpITab, byteptr, iface); // type word of interface
            if (Debug_typeassert > 0L)
            {
                Warnl(n.Pos, "type assertion inlined");
            }

            ptr<ssa.Value> targetITab;
            if (n.Left.Type.IsEmptyInterface())
            { 
                // Looking for pointer to target type.
                targetITab = target;

            }
            else
            { 
                // Looking for pointer to itab for target type and source interface.
                targetITab = s.expr(n.List.First());

            }

            ptr<Node> tmp; // temporary for use with large types
            ptr<ssa.Value> addr; // address of tmp
            if (commaok && !canSSAType(_addr_n.Type))
            { 
                // unSSAable type, use temporary.
                // TODO: get rid of some of these temporaries.
                tmp = tempAt(n.Pos, s.curfn, n.Type);
                s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, tmp, s.mem());
                addr = s.addr(tmp);

            }

            cond = s.newValue2(ssa.OpEqPtr, types.Types[TBOOL], itab, targetITab);
            b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cond);
            b.Likely = ssa.BranchLikely;

            bOk = s.f.NewBlock(ssa.BlockPlain);
            bFail = s.f.NewBlock(ssa.BlockPlain);
            b.AddEdgeTo(bOk);
            b.AddEdgeTo(bFail);

            if (!commaok)
            { 
                // on failure, panic by calling panicdottype
                s.startBlock(bFail);
                var taddr = s.expr(n.Right.Right);
                if (n.Left.Type.IsEmptyInterface())
                {
                    s.rtcall(panicdottypeE, false, null, itab, target, taddr);
                }
                else
                {
                    s.rtcall(panicdottypeI, false, null, itab, target, taddr);
                } 

                // on success, return data from interface
                s.startBlock(bOk);
                if (direct)
                {
                    return (_addr_s.newValue1(ssa.OpIData, n.Type, iface)!, _addr_null!);
                }

                var p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                return (_addr_s.load(n.Type, p)!, _addr_null!);

            } 

            // commaok is the more complicated case because we have
            // a control flow merge point.
            bEnd = s.f.NewBlock(ssa.BlockPlain); 
            // Note that we need a new valVar each time (unlike okVar where we can
            // reuse the variable) because it might have a different type every time.
            ptr<Node> valVar = addr(new Node(Op:ONAME,Sym:&types.Sym{Name:"val"})); 

            // type assertion succeeded
            s.startBlock(bOk);
            if (tmp == null)
            {
                if (direct)
                {
                    s.vars[valVar] = s.newValue1(ssa.OpIData, n.Type, iface);
                }
                else
                {
                    p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                    s.vars[valVar] = s.load(n.Type, p);
                }

            }
            else
            {
                p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                s.move(n.Type, addr, p);
            }

            s.vars[_addr_okVar] = s.constBool(true);
            s.endBlock();
            bOk.AddEdgeTo(bEnd); 

            // type assertion failed
            s.startBlock(bFail);
            if (tmp == null)
            {
                s.vars[valVar] = s.zeroVal(n.Type);
            }
            else
            {
                s.zero(n.Type, addr);
            }

            s.vars[_addr_okVar] = s.constBool(false);
            s.endBlock();
            bFail.AddEdgeTo(bEnd); 

            // merge point
            s.startBlock(bEnd);
            if (tmp == null)
            {
                res = s.variable(valVar, n.Type);
                delete(s.vars, valVar);
            }
            else
            {
                res = s.load(n.Type, addr);
                s.vars[_addr_memVar] = s.newValue1A(ssa.OpVarKill, types.TypeMem, tmp, s.mem());
            }

            resok = s.variable(_addr_okVar, types.Types[TBOOL]);
            delete(s.vars, _addr_okVar);
            return (_addr_res!, _addr_resok!);

        }

        // variable returns the value of a variable at the current location.
        private static ptr<ssa.Value> variable(this ptr<state> _addr_s, ptr<Node> _addr_name, ptr<types.Type> _addr_t)
        {
            ref state s = ref _addr_s.val;
            ref Node name = ref _addr_name.val;
            ref types.Type t = ref _addr_t.val;

            var v = s.vars[name];
            if (v != null)
            {
                return _addr_v!;
            }

            v = s.fwdVars[name];
            if (v != null)
            {
                return _addr_v!;
            }

            if (s.curBlock == s.f.Entry)
            { 
                // No variable should be live at entry.
                s.Fatalf("Value live at entry. It shouldn't be. func %s, node %v, value %v", s.f.Name, name, v);

            } 
            // Make a FwdRef, which records a value that's live on block input.
            // We'll find the matching definition as part of insertPhis.
            v = s.newValue0A(ssa.OpFwdRef, t, name);
            s.fwdVars[name] = v;
            s.addNamedValue(name, v);
            return _addr_v!;

        }

        private static ptr<ssa.Value> mem(this ptr<state> _addr_s)
        {
            ref state s = ref _addr_s.val;

            return _addr_s.variable(_addr_memVar, types.TypeMem)!;
        }

        private static void addNamedValue(this ptr<state> _addr_s, ptr<Node> _addr_n, ptr<ssa.Value> _addr_v)
        {
            ref state s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;
            ref ssa.Value v = ref _addr_v.val;

            if (n.Class() == Pxxx)
            { 
                // Don't track our dummy nodes (&memVar etc.).
                return ;

            }

            if (n.IsAutoTmp())
            { 
                // Don't track temporary variables.
                return ;

            }

            if (n.Class() == PPARAMOUT)
            { 
                // Don't track named output values.  This prevents return values
                // from being assigned too early. See #14591 and #14762. TODO: allow this.
                return ;

            }

            if (n.Class() == PAUTO && n.Xoffset != 0L)
            {
                s.Fatalf("AUTO var with offset %v %d", n, n.Xoffset);
            }

            ssa.LocalSlot loc = new ssa.LocalSlot(N:n,Type:n.Type,Off:0);
            var (values, ok) = s.f.NamedValues[loc];
            if (!ok)
            {
                s.f.Names = append(s.f.Names, loc);
            }

            s.f.NamedValues[loc] = append(values, v);

        }

        // Generate a disconnected call to a runtime routine and a return.
        private static ptr<obj.Prog> gencallret(ptr<Progs> _addr_pp, ptr<obj.LSym> _addr_sym)
        {
            ref Progs pp = ref _addr_pp.val;
            ref obj.LSym sym = ref _addr_sym.val;

            var p = pp.Prog(obj.ACALL);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = sym;
            p = pp.Prog(obj.ARET);
            return _addr_p!;
        }

        // Branch is an unresolved branch.
        public partial struct Branch
        {
            public ptr<obj.Prog> P; // branch instruction
            public ptr<ssa.Block> B; // target
        }

        // SSAGenState contains state needed during Prog generation.
        public partial struct SSAGenState
        {
            public ptr<Progs> pp; // Branches remembers all the branch instructions we've seen
// and where they would like to go.
            public slice<Branch> Branches; // bstart remembers where each block starts (indexed by block ID)
            public slice<ptr<obj.Prog>> bstart; // 387 port: maps from SSE registers (REG_X?) to 387 registers (REG_F?)
            public map<short, short> SSEto387; // Some architectures require a 64-bit temporary for FP-related register shuffling. Examples include x86-387, PPC, and Sparc V8.
            public ptr<Node> ScratchFpMem;
            public long maxarg; // largest frame size for arguments to calls made by the function

// Map from GC safe points to liveness index, generated by
// liveness analysis.
            public LivenessMap livenessMap; // lineRunStart records the beginning of the current run of instructions
// within a single block sharing the same line number
// Used to move statement marks to the beginning of such runs.
            public ptr<obj.Prog> lineRunStart; // wasm: The number of values on the WebAssembly stack. This is only used as a safeguard.
            public long OnWasmStackSkipped; // Liveness index for the first function call in the final defer exit code
// path that we generated. All defer functions and args should be live at
// this point. This will be used to set the liveness for the deferreturn.
            public LivenessIndex lastDeferLiveness;
        }

        // Prog appends a new Prog.
        private static ptr<obj.Prog> Prog(this ptr<SSAGenState> _addr_s, obj.As @as)
        {
            ref SSAGenState s = ref _addr_s.val;

            var p = s.pp.Prog(as);
            if (ssa.LosesStmtMark(as))
            {
                return _addr_p!;
            } 
            // Float a statement start to the beginning of any same-line run.
            // lineRunStart is reset at block boundaries, which appears to work well.
            if (s.lineRunStart == null || s.lineRunStart.Pos.Line() != p.Pos.Line())
            {
                s.lineRunStart = p;
            }
            else if (p.Pos.IsStmt() == src.PosIsStmt)
            {
                s.lineRunStart.Pos = s.lineRunStart.Pos.WithIsStmt();
                p.Pos = p.Pos.WithNotStmt();
            }

            return _addr_p!;

        }

        // Pc returns the current Prog.
        private static ptr<obj.Prog> Pc(this ptr<SSAGenState> _addr_s)
        {
            ref SSAGenState s = ref _addr_s.val;

            return _addr_s.pp.next!;
        }

        // SetPos sets the current source position.
        private static void SetPos(this ptr<SSAGenState> _addr_s, src.XPos pos)
        {
            ref SSAGenState s = ref _addr_s.val;

            s.pp.pos = pos;
        }

        // Br emits a single branch instruction and returns the instruction.
        // Not all architectures need the returned instruction, but otherwise
        // the boilerplate is common to all.
        private static ptr<obj.Prog> Br(this ptr<SSAGenState> _addr_s, obj.As op, ptr<ssa.Block> _addr_target)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Block target = ref _addr_target.val;

            var p = s.Prog(op);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new Branch(P:p,B:target));
            return _addr_p!;
        }

        // DebugFriendlySetPosFrom adjusts Pos.IsStmt subject to heuristics
        // that reduce "jumpy" line number churn when debugging.
        // Spill/fill/copy instructions from the register allocator,
        // phi functions, and instructions with a no-pos position
        // are examples of instructions that can cause churn.
        private static void DebugFriendlySetPosFrom(this ptr<SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpPhi || v.Op == ssa.OpCopy || v.Op == ssa.OpLoadReg || v.Op == ssa.OpStoreReg) 
                // These are not statements
                s.SetPos(v.Pos.WithNotStmt());
            else 
                var p = v.Pos;
                if (p != src.NoXPos)
                { 
                    // If the position is defined, update the position.
                    // Also convert default IsStmt to NotStmt; only
                    // explicit statement boundaries should appear
                    // in the generated code.
                    if (p.IsStmt() != src.PosIsStmt)
                    {
                        p = p.WithNotStmt(); 
                        // Calls use the pos attached to v, but copy the statement mark from SSAGenState
                    }

                    s.SetPos(p);

                }
                else
                {
                    s.SetPos(s.pp.pos.WithNotStmt());
                }

                    }

        // byXoffset implements sort.Interface for []*Node using Xoffset as the ordering.
        private partial struct byXoffset // : slice<ptr<Node>>
        {
        }

        private static long Len(this byXoffset s)
        {
            return len(s);
        }
        private static bool Less(this byXoffset s, long i, long j)
        {
            return s[i].Xoffset < s[j].Xoffset;
        }
        private static void Swap(this byXoffset s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];
        }

        private static void emitStackObjects(ptr<ssafn> _addr_e, ptr<Progs> _addr_pp)
        {
            ref ssafn e = ref _addr_e.val;
            ref Progs pp = ref _addr_pp.val;

            slice<ptr<Node>> vars = default;
            foreach (var (_, n) in e.curfn.Func.Dcl)
            {
                if (livenessShouldTrack(n) && n.Name.Addrtaken())
                {
                    vars = append(vars, n);
                }

            }
            if (len(vars) == 0L)
            {
                return ;
            } 

            // Sort variables from lowest to highest address.
            sort.Sort(byXoffset(vars)); 

            // Populate the stack object data.
            // Format must match runtime/stack.go:stackObjectRecord.
            var x = e.curfn.Func.lsym.Func.StackObjects;
            long off = 0L;
            off = duintptr(x, off, uint64(len(vars)));
            {
                var v__prev1 = v;

                foreach (var (_, __v) in vars)
                {
                    v = __v; 
                    // Note: arguments and return values have non-negative Xoffset,
                    // in which case the offset is relative to argp.
                    // Locals have a negative Xoffset, in which case the offset is relative to varp.
                    off = duintptr(x, off, uint64(v.Xoffset));
                    if (!typesym(v.Type).Siggen())
                    {
                        e.Fatalf(v.Pos, "stack object's type symbol not generated for type %s", v.Type);
                    }

                    off = dsymptr(x, off, dtypesym(v.Type), 0L);

                } 

                // Emit a funcdata pointing at the stack object data.

                v = v__prev1;
            }

            var p = pp.Prog(obj.AFUNCDATA);
            Addrconst(_addr_p.From, objabi.FUNCDATA_StackObjects);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = x;

            if (debuglive != 0L)
            {
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in vars)
                    {
                        v = __v;
                        Warnl(v.Pos, "stack object %v %s", v, v.Type.String());
                    }

                    v = v__prev1;
                }
            }

        }

        // genssa appends entries to pp for each instruction in f.
        private static void genssa(ptr<ssa.Func> _addr_f, ptr<Progs> _addr_pp)
        {
            ref ssa.Func f = ref _addr_f.val;
            ref Progs pp = ref _addr_pp.val;

            ref SSAGenState s = ref heap(out ptr<SSAGenState> _addr_s);

            ptr<ssafn> e = f.Frontend()._<ptr<ssafn>>();

            s.livenessMap = liveness(e, f, pp);
            emitStackObjects(e, _addr_pp);

            var openDeferInfo = e.curfn.Func.lsym.Func.OpenCodedDeferInfo;
            if (openDeferInfo != null)
            { 
                // This function uses open-coded defers -- write out the funcdata
                // info that we computed at the end of genssa.
                var p = pp.Prog(obj.AFUNCDATA);
                Addrconst(_addr_p.From, objabi.FUNCDATA_OpenCodedDeferInfo);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = openDeferInfo;

            } 

            // Remember where each block starts.
            s.bstart = make_slice<ptr<obj.Prog>>(f.NumBlocks());
            s.pp = pp;
            map<ptr<obj.Prog>, ptr<ssa.Value>> progToValue = default;
            map<ptr<obj.Prog>, ptr<ssa.Block>> progToBlock = default;
            slice<ptr<obj.Prog>> valueToProgAfter = default; // The first Prog following computation of a value v; v is visible at this point.
            if (f.PrintOrHtmlSSA)
            {
                progToValue = make_map<ptr<obj.Prog>, ptr<ssa.Value>>(f.NumValues());
                progToBlock = make_map<ptr<obj.Prog>, ptr<ssa.Block>>(f.NumBlocks());
                f.Logf("genssa %s\n", f.Name);
                progToBlock[s.pp.next] = f.Blocks[0L];
            }

            if (thearch.Use387)
            {
                s.SSEto387 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<short, short>{};
            }

            s.ScratchFpMem = e.scratchFpMem;

            if (Ctxt.Flag_locationlists)
            {
                if (cap(f.Cache.ValueToProgAfter) < f.NumValues())
                {
                    f.Cache.ValueToProgAfter = make_slice<ptr<obj.Prog>>(f.NumValues());
                }

                valueToProgAfter = f.Cache.ValueToProgAfter[..f.NumValues()];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in valueToProgAfter)
                    {
                        i = __i;
                        valueToProgAfter[i] = null;
                    }

                    i = i__prev1;
                }
            } 

            // If the very first instruction is not tagged as a statement,
            // debuggers may attribute it to previous function in program.
            var firstPos = src.NoXPos;
            {
                var v__prev1 = v;

                foreach (var (_, __v) in f.Entry.Values)
                {
                    v = __v;
                    if (v.Pos.IsStmt() == src.PosIsStmt)
                    {
                        firstPos = v.Pos;
                        v.Pos = firstPos.WithDefaultStmt();
                        break;
                    }

                } 

                // inlMarks has an entry for each Prog that implements an inline mark.
                // It maps from that Prog to the global inlining id of the inlined body
                // which should unwind to this Prog's location.

                v = v__prev1;
            }

            map<ptr<obj.Prog>, int> inlMarks = default;
            slice<ptr<obj.Prog>> inlMarkList = default; 

            // inlMarksByPos maps from a (column 1) source position to the set of
            // Progs that are in the set above and have that source position.
            map<src.XPos, slice<ptr<obj.Prog>>> inlMarksByPos = default; 

            // Emit basic blocks
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    s.bstart[b.ID] = s.pp.next;
                    s.lineRunStart = null; 

                    // Attach a "default" liveness info. Normally this will be
                    // overwritten in the Values loop below for each Value. But
                    // for an empty block this will be used for its control
                    // instruction. We won't use the actual liveness map on a
                    // control instruction. Just mark it something that is
                    // preemptible, unless this function is "all unsafe".
                    s.pp.nextLive = new LivenessIndex(-1,-1,allUnsafe(f)); 

                    // Emit values in block
                    thearch.SSAMarkMoves(_addr_s, b);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            var x = s.pp.next;
                            s.DebugFriendlySetPosFrom(v);


                            if (v.Op == ssa.OpInitMem)                             else if (v.Op == ssa.OpArg)                             else if (v.Op == ssa.OpSP || v.Op == ssa.OpSB)                             else if (v.Op == ssa.OpSelect0 || v.Op == ssa.OpSelect1)                             else if (v.Op == ssa.OpGetG)                             else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive || v.Op == ssa.OpVarKill)                             else if (v.Op == ssa.OpPhi) 
                                CheckLoweredPhi(_addr_v);
                            else if (v.Op == ssa.OpConvert) 
                                // nothing to do; no-op conversion for liveness
                                if (v.Args[0L].Reg() != v.Reg())
                                {
                                    v.Fatalf("OpConvert should be a no-op: %s; %s", v.Args[0L].LongString(), v.LongString());
                                }

                            else if (v.Op == ssa.OpInlMark) 
                                p = thearch.Ginsnop(s.pp);
                                if (inlMarks == null)
                                {
                                    inlMarks = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<obj.Prog>, int>{};
                                    inlMarksByPos = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<src.XPos, slice<ptr<obj.Prog>>>{};
                                }

                                inlMarks[p] = v.AuxInt32();
                                inlMarkList = append(inlMarkList, p);
                                var pos = v.Pos.AtColumn1();
                                inlMarksByPos[pos] = append(inlMarksByPos[pos], p);
                            else 
                                // Attach this safe point to the next
                                // instruction.
                                s.pp.nextLive = s.livenessMap.Get(v); 

                                // Remember the liveness index of the first defer call of
                                // the last defer exit
                                if (v.Block.Func.LastDeferExit != null && v == v.Block.Func.LastDeferExit)
                                {
                                    s.lastDeferLiveness = s.pp.nextLive;
                                } 

                                // Special case for first line in function; move it to the start.
                                if (firstPos != src.NoXPos)
                                {
                                    s.SetPos(firstPos);
                                    firstPos = src.NoXPos;
                                } 
                                // let the backend handle it
                                thearch.SSAGenValue(_addr_s, v);
                                                        if (Ctxt.Flag_locationlists)
                            {
                                valueToProgAfter[v.ID] = s.pp.next;
                            }

                            if (f.PrintOrHtmlSSA)
                            {
                                while (x != s.pp.next)
                                {
                                    progToValue[x] = v;
                                    x = x.Link;
                                }


                            }

                        } 
                        // If this is an empty infinite loop, stick a hardware NOP in there so that debuggers are less confused.

                        v = v__prev2;
                    }

                    if (s.bstart[b.ID] == s.pp.next && len(b.Succs) == 1L && b.Succs[0L].Block() == b)
                    {
                        p = thearch.Ginsnop(s.pp);
                        p.Pos = p.Pos.WithIsStmt();
                        if (b.Pos == src.NoXPos)
                        {
                            b.Pos = p.Pos; // It needs a file, otherwise a no-file non-zero line causes confusion.  See #35652.
                            if (b.Pos == src.NoXPos)
                            {
                                b.Pos = pp.Text.Pos; // Sometimes p.Pos is empty.  See #35695.
                            }

                        }

                        b.Pos = b.Pos.WithBogusLine(); // Debuggers are not good about infinite loops, force a change in line number
                    } 
                    // Emit control flow instructions for block
                    ptr<ssa.Block> next;
                    if (i < len(f.Blocks) - 1L && Debug['N'] == 0L)
                    { 
                        // If -N, leave next==nil so every block with successors
                        // ends in a JMP (except call blocks - plive doesn't like
                        // select{send,recv} followed by a JMP call).  Helps keep
                        // line numbers for otherwise empty blocks.
                        next = f.Blocks[i + 1L];

                    }

                    x = s.pp.next;
                    s.SetPos(b.Pos);
                    thearch.SSAGenBlock(_addr_s, b, next);
                    if (f.PrintOrHtmlSSA)
                    {
                        while (x != s.pp.next)
                        {
                            progToBlock[x] = b;
                            x = x.Link;
                        }


                    }

                }

                i = i__prev1;
                b = b__prev1;
            }

            if (f.Blocks[len(f.Blocks) - 1L].Kind == ssa.BlockExit)
            { 
                // We need the return address of a panic call to
                // still be inside the function in question. So if
                // it ends in a call which doesn't return, add a
                // nop (which will never execute) after the call.
                thearch.Ginsnop(pp);

            }

            if (openDeferInfo != null)
            { 
                // When doing open-coded defers, generate a disconnected call to
                // deferreturn and a return. This will be used to during panic
                // recovery to unwind the stack and return back to the runtime.
                s.pp.nextLive = s.lastDeferLiveness;
                gencallret(_addr_pp, _addr_Deferreturn);

            }

            if (inlMarks != null)
            { 
                // We have some inline marks. Try to find other instructions we're
                // going to emit anyway, and use those instructions instead of the
                // inline marks.
                {
                    var p__prev1 = p;

                    p = pp.Text;

                    while (p != null)
                    {
                        if (p.As == obj.ANOP || p.As == obj.AFUNCDATA || p.As == obj.APCDATA || p.As == obj.ATEXT || p.As == obj.APCALIGN || thearch.LinkArch.Family == sys.Wasm)
                        { 
                            // Don't use 0-sized instructions as inline marks, because we need
                            // to identify inline mark instructions by pc offset.
                            // (Some of these instructions are sometimes zero-sized, sometimes not.
                            // We must not use anything that even might be zero-sized.)
                            // TODO: are there others?
                            continue;
                        p = p.Link;
                        }

                        {
                            var (_, ok) = inlMarks[p];

                            if (ok)
                            { 
                                // Don't use inline marks themselves. We don't know
                                // whether they will be zero-sized or not yet.
                                continue;

                            }

                        }

                        pos = p.Pos.AtColumn1();
                        s = inlMarksByPos[pos];
                        if (len(s) == 0L)
                        {
                            continue;
                        }

                        foreach (var (_, m) in s)
                        { 
                            // We found an instruction with the same source position as
                            // some of the inline marks.
                            // Use this instruction instead.
                            p.Pos = p.Pos.WithIsStmt(); // promote position to a statement
                            pp.curfn.Func.lsym.Func.AddInlMark(p, inlMarks[m]); 
                            // Make the inline mark a real nop, so it doesn't generate any code.
                            m.As = obj.ANOP;
                            m.Pos = src.NoXPos;
                            m.From = new obj.Addr();
                            m.To = new obj.Addr();

                        }
                        delete(inlMarksByPos, pos);

                    } 
                    // Any unmatched inline marks now need to be added to the inlining tree (and will generate a nop instruction).


                    p = p__prev1;
                } 
                // Any unmatched inline marks now need to be added to the inlining tree (and will generate a nop instruction).
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in inlMarkList)
                    {
                        p = __p;
                        if (p.As != obj.ANOP)
                        {
                            pp.curfn.Func.lsym.Func.AddInlMark(p, inlMarks[p]);
                        }

                    }

                    p = p__prev1;
                }
            }

            if (Ctxt.Flag_locationlists)
            {
                e.curfn.Func.DebugInfo = ssa.BuildFuncDebug(Ctxt, f, Debug_locationlist > 1L, stackOffset);
                var bstart = s.bstart; 
                // Note that at this moment, Prog.Pc is a sequence number; it's
                // not a real PC until after assembly, so this mapping has to
                // be done later.
                e.curfn.Func.DebugInfo.GetPC = (b, v) =>
                {

                    if (v == ssa.BlockStart.ID) 
                        if (b == f.Entry.ID)
                        {
                            return 0L; // Start at the very beginning, at the assembler-generated prologue.
                            // this should only happen for function args (ssa.OpArg)
                        }

                        return bstart[b].Pc;
                    else if (v == ssa.BlockEnd.ID) 
                        return e.curfn.Func.lsym.Size;
                    else 
                        return valueToProgAfter[v].Pc;
                    
                }
;

            } 

            // Resolve branches, and relax DefaultStmt into NotStmt
            foreach (var (_, br) in s.Branches)
            {
                br.P.To.Val = s.bstart[br.B.ID];
                if (br.P.Pos.IsStmt() != src.PosIsStmt)
                {
                    br.P.Pos = br.P.Pos.WithNotStmt();
                }                {
                    var v0 = br.B.FirstPossibleStmtValue();


                    else if (v0 != null && v0.Pos.Line() == br.P.Pos.Line() && v0.Pos.IsStmt() == src.PosIsStmt)
                    {
                        br.P.Pos = br.P.Pos.WithNotStmt();
                    }

                }


            }
            if (e.log)
            { // spew to stdout
                @string filename = "";
                {
                    var p__prev1 = p;

                    p = pp.Text;

                    while (p != null)
                    {
                        if (p.Pos.IsKnown() && p.InnermostFilename() != filename)
                        {
                            filename = p.InnermostFilename();
                            f.Logf("# %s\n", filename);
                        p = p.Link;
                        }

                        s = default;
                        {
                            var v__prev2 = v;

                            var (v, ok) = progToValue[p];

                            if (ok)
                            {
                                s = v.String();
                            }                            {
                                var b__prev3 = b;

                                var (b, ok) = progToBlock[p];


                                else if (ok)
                                {
                                    s = b.String();
                                }
                                else
                                {
                                    s = "   "; // most value and branch strings are 2-3 characters long
                                }

                                b = b__prev3;

                            }


                            v = v__prev2;

                        }

                        f.Logf(" %-6s\t%.5d (%s)\t%s\n", s, p.Pc, p.InnermostLineNumber(), p.InstructionString());

                    }


                    p = p__prev1;
                }

            }

            if (f.HTMLWriter != null)
            { // spew to ssa.html
                bytes.Buffer buf = default;
                buf.WriteString("<code>");
                buf.WriteString("<dl class=\"ssa-gen\">");
                filename = "";
                {
                    var p__prev1 = p;

                    p = pp.Text;

                    while (p != null)
                    { 
                        // Don't spam every line with the file name, which is often huge.
                        // Only print changes, and "unknown" is not a change.
                        if (p.Pos.IsKnown() && p.InnermostFilename() != filename)
                        {
                            filename = p.InnermostFilename();
                            buf.WriteString("<dt class=\"ssa-prog-src\"></dt><dd class=\"ssa-prog\">");
                            buf.WriteString(html.EscapeString("# " + filename));
                            buf.WriteString("</dd>");
                        p = p.Link;
                        }

                        buf.WriteString("<dt class=\"ssa-prog-src\">");
                        {
                            var v__prev2 = v;

                            (v, ok) = progToValue[p];

                            if (ok)
                            {
                                buf.WriteString(v.HTML());
                            }                            {
                                var b__prev3 = b;

                                (b, ok) = progToBlock[p];


                                else if (ok)
                                {
                                    buf.WriteString("<b>" + b.HTML() + "</b>");
                                }

                                b = b__prev3;

                            }


                            v = v__prev2;

                        }

                        buf.WriteString("</dt>");
                        buf.WriteString("<dd class=\"ssa-prog\">");
                        buf.WriteString(fmt.Sprintf("%.5d <span class=\"l%v line-number\">(%s)</span> %s", p.Pc, p.InnermostLineNumber(), p.InnermostLineNumberHTML(), html.EscapeString(p.InstructionString())));
                        buf.WriteString("</dd>");

                    }


                    p = p__prev1;
                }
                buf.WriteString("</dl>");
                buf.WriteString("</code>");
                f.HTMLWriter.WriteColumn("genssa", "genssa", "ssa-prog", buf.String());

            }

            defframe(_addr_s, e);

            f.HTMLWriter.Close();
            f.HTMLWriter = null;

        }

        private static void defframe(ptr<SSAGenState> _addr_s, ptr<ssafn> _addr_e)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssafn e = ref _addr_e.val;

            var pp = s.pp;

            var frame = Rnd(s.maxarg + e.stksize, int64(Widthreg));
            if (thearch.PadFrame != null)
            {
                frame = thearch.PadFrame(frame);
            } 

            // Fill in argument and frame size.
            pp.Text.To.Type = obj.TYPE_TEXTSIZE;
            pp.Text.To.Val = int32(Rnd(e.curfn.Type.ArgWidth(), int64(Widthreg)));
            pp.Text.To.Offset = frame; 

            // Insert code to zero ambiguously live variables so that the
            // garbage collector only sees initialized values when it
            // looks for pointers.
            var p = pp.Text;
            long lo = default;            long hi = default; 

            // Opaque state for backend to use. Current backends use it to
            // keep track of which helper registers have been zeroed.
 

            // Opaque state for backend to use. Current backends use it to
            // keep track of which helper registers have been zeroed.
            ref uint state = ref heap(out ptr<uint> _addr_state); 

            // Iterate through declarations. They are sorted in decreasing Xoffset order.
            foreach (var (_, n) in e.curfn.Func.Dcl)
            {
                if (!n.Name.Needzero())
                {
                    continue;
                }

                if (n.Class() != PAUTO)
                {
                    e.Fatalf(n.Pos, "needzero class %d", n.Class());
                }

                if (n.Type.Size() % int64(Widthptr) != 0L || n.Xoffset % int64(Widthptr) != 0L || n.Type.Size() == 0L)
                {
                    e.Fatalf(n.Pos, "var %L has size %d offset %d", n, n.Type.Size(), n.Xoffset);
                }

                if (lo != hi && n.Xoffset + n.Type.Size() >= lo - int64(2L * Widthreg))
                { 
                    // Merge with range we already have.
                    lo = n.Xoffset;
                    continue;

                } 

                // Zero old range
                p = thearch.ZeroRange(pp, p, frame + lo, hi - lo, _addr_state); 

                // Set new range.
                lo = n.Xoffset;
                hi = lo + n.Type.Size();

            } 

            // Zero final range.
            thearch.ZeroRange(pp, p, frame + lo, hi - lo, _addr_state);

        }

        // For generating consecutive jump instructions to model a specific branching
        public partial struct IndexJump
        {
            public obj.As Jump;
            public long Index;
        }

        private static void oneJump(this ptr<SSAGenState> _addr_s, ptr<ssa.Block> _addr_b, ptr<IndexJump> _addr_jump)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref IndexJump jump = ref _addr_jump.val;

            var p = s.Br(jump.Jump, b.Succs[jump.Index].Block());
            p.Pos = b.Pos;
        }

        // CombJump generates combinational instructions (2 at present) for a block jump,
        // thereby the behaviour of non-standard condition codes could be simulated
        private static void CombJump(this ptr<SSAGenState> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next, ptr<array<array<IndexJump>>> _addr_jumps)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref ssa.Block next = ref _addr_next.val;
            ref array<array<IndexJump>> jumps = ref _addr_jumps.val;


            if (next == b.Succs[0L].Block()) 
                s.oneJump(b, _addr_jumps[0L][0L]);
                s.oneJump(b, _addr_jumps[0L][1L]);
            else if (next == b.Succs[1L].Block()) 
                s.oneJump(b, _addr_jumps[1L][0L]);
                s.oneJump(b, _addr_jumps[1L][1L]);
            else 
                ptr<obj.Prog> q;
                if (b.Likely != ssa.BranchUnlikely)
                {
                    s.oneJump(b, _addr_jumps[1L][0L]);
                    s.oneJump(b, _addr_jumps[1L][1L]);
                    q = s.Br(obj.AJMP, b.Succs[1L].Block());
                }
                else
                {
                    s.oneJump(b, _addr_jumps[0L][0L]);
                    s.oneJump(b, _addr_jumps[0L][1L]);
                    q = s.Br(obj.AJMP, b.Succs[0L].Block());
                }

                q.Pos = b.Pos;
            
        }

        // AddAux adds the offset in the aux fields (AuxInt and Aux) of v to a.
        public static void AddAux(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v)
        {
            ref obj.Addr a = ref _addr_a.val;
            ref ssa.Value v = ref _addr_v.val;

            AddAux2(_addr_a, _addr_v, v.AuxInt);
        }
        public static void AddAux2(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v, long offset)
        {
            ref obj.Addr a = ref _addr_a.val;
            ref ssa.Value v = ref _addr_v.val;

            if (a.Type != obj.TYPE_MEM && a.Type != obj.TYPE_ADDR)
            {
                v.Fatalf("bad AddAux addr %v", a);
            } 
            // add integer offset
            a.Offset += offset; 

            // If no additional symbol offset, we're done.
            if (v.Aux == null)
            {
                return ;
            } 
            // Add symbol's offset from its base register.
            switch (v.Aux.type())
            {
                case ptr<obj.LSym> n:
                    a.Name = obj.NAME_EXTERN;
                    a.Sym = n;
                    break;
                case ptr<Node> n:
                    if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
                    {
                        a.Name = obj.NAME_PARAM;
                        a.Sym = n.Orig.Sym.Linksym();
                        a.Offset += n.Xoffset;
                        break;
                    }

                    a.Name = obj.NAME_AUTO;
                    a.Sym = n.Sym.Linksym();
                    a.Offset += n.Xoffset;
                    break;
                default:
                {
                    var n = v.Aux.type();
                    v.Fatalf("aux in %s not implemented %#v", v, v.Aux);
                    break;
                }
            }

        }

        // extendIndex extends v to a full int width.
        // panic with the given kind if v does not fit in an int (only on 32-bit archs).
        private static ptr<ssa.Value> extendIndex(this ptr<state> _addr_s, ptr<ssa.Value> _addr_idx, ptr<ssa.Value> _addr_len, ssa.BoundsKind kind, bool bounded)
        {
            ref state s = ref _addr_s.val;
            ref ssa.Value idx = ref _addr_idx.val;
            ref ssa.Value len = ref _addr_len.val;

            var size = idx.Type.Size();
            if (size == s.config.PtrSize)
            {
                return _addr_idx!;
            }

            if (size > s.config.PtrSize)
            { 
                // truncate 64-bit indexes on 32-bit pointer archs. Test the
                // high word and branch to out-of-bounds failure if it is not 0.
                ptr<ssa.Value> lo;
                if (idx.Type.IsSigned())
                {
                    lo = s.newValue1(ssa.OpInt64Lo, types.Types[TINT], idx);
                }
                else
                {
                    lo = s.newValue1(ssa.OpInt64Lo, types.Types[TUINT], idx);
                }

                if (bounded || Debug['B'] != 0L)
                {
                    return _addr_lo!;
                }

                var bNext = s.f.NewBlock(ssa.BlockPlain);
                var bPanic = s.f.NewBlock(ssa.BlockExit);
                var hi = s.newValue1(ssa.OpInt64Hi, types.Types[TUINT32], idx);
                var cmp = s.newValue2(ssa.OpEq32, types.Types[TBOOL], hi, s.constInt32(types.Types[TUINT32], 0L));
                if (!idx.Type.IsSigned())
                {

                    if (kind == ssa.BoundsIndex) 
                        kind = ssa.BoundsIndexU;
                    else if (kind == ssa.BoundsSliceAlen) 
                        kind = ssa.BoundsSliceAlenU;
                    else if (kind == ssa.BoundsSliceAcap) 
                        kind = ssa.BoundsSliceAcapU;
                    else if (kind == ssa.BoundsSliceB) 
                        kind = ssa.BoundsSliceBU;
                    else if (kind == ssa.BoundsSlice3Alen) 
                        kind = ssa.BoundsSlice3AlenU;
                    else if (kind == ssa.BoundsSlice3Acap) 
                        kind = ssa.BoundsSlice3AcapU;
                    else if (kind == ssa.BoundsSlice3B) 
                        kind = ssa.BoundsSlice3BU;
                    else if (kind == ssa.BoundsSlice3C) 
                        kind = ssa.BoundsSlice3CU;
                    
                }

                var b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(cmp);
                b.Likely = ssa.BranchLikely;
                b.AddEdgeTo(bNext);
                b.AddEdgeTo(bPanic);

                s.startBlock(bPanic);
                var mem = s.newValue4I(ssa.OpPanicExtend, types.TypeMem, int64(kind), hi, lo, len, s.mem());
                s.endBlock().SetControl(mem);
                s.startBlock(bNext);

                return _addr_lo!;

            } 

            // Extend value to the required size
            ssa.Op op = default;
            if (idx.Type.IsSigned())
            {
                switch (10L * size + s.config.PtrSize)
                {
                    case 14L: 
                        op = ssa.OpSignExt8to32;
                        break;
                    case 18L: 
                        op = ssa.OpSignExt8to64;
                        break;
                    case 24L: 
                        op = ssa.OpSignExt16to32;
                        break;
                    case 28L: 
                        op = ssa.OpSignExt16to64;
                        break;
                    case 48L: 
                        op = ssa.OpSignExt32to64;
                        break;
                    default: 
                        s.Fatalf("bad signed index extension %s", idx.Type);
                        break;
                }

            }
            else
            {
                switch (10L * size + s.config.PtrSize)
                {
                    case 14L: 
                        op = ssa.OpZeroExt8to32;
                        break;
                    case 18L: 
                        op = ssa.OpZeroExt8to64;
                        break;
                    case 24L: 
                        op = ssa.OpZeroExt16to32;
                        break;
                    case 28L: 
                        op = ssa.OpZeroExt16to64;
                        break;
                    case 48L: 
                        op = ssa.OpZeroExt32to64;
                        break;
                    default: 
                        s.Fatalf("bad unsigned index extension %s", idx.Type);
                        break;
                }

            }

            return _addr_s.newValue1(op, types.Types[TINT], idx)!;

        }

        // CheckLoweredPhi checks that regalloc and stackalloc correctly handled phi values.
        // Called during ssaGenValue.
        public static void CheckLoweredPhi(ptr<ssa.Value> _addr_v)
        {
            ref ssa.Value v = ref _addr_v.val;

            if (v.Op != ssa.OpPhi)
            {
                v.Fatalf("CheckLoweredPhi called with non-phi value: %v", v.LongString());
            }

            if (v.Type.IsMemory())
            {
                return ;
            }

            var f = v.Block.Func;
            var loc = f.RegAlloc[v.ID];
            foreach (var (_, a) in v.Args)
            {
                {
                    var aloc = f.RegAlloc[a.ID];

                    if (aloc != loc)
                    { // TODO: .Equal() instead?
                        v.Fatalf("phi arg at different location than phi: %v @ %s, but arg %v @ %s\n%s\n", v, loc, a, aloc, v.Block.Func);

                    }

                }

            }

        }

        // CheckLoweredGetClosurePtr checks that v is the first instruction in the function's entry block.
        // The output of LoweredGetClosurePtr is generally hardwired to the correct register.
        // That register contains the closure pointer on closure entry.
        public static void CheckLoweredGetClosurePtr(ptr<ssa.Value> _addr_v)
        {
            ref ssa.Value v = ref _addr_v.val;

            var entry = v.Block.Func.Entry;
            if (entry != v.Block || entry.Values[0L] != v)
            {
                Fatalf("in %s, badly placed LoweredGetClosurePtr: %v %v", v.Block.Func.Name, v.Block, v);
            }

        }

        // AutoVar returns a *Node and int64 representing the auto variable and offset within it
        // where v should be spilled.
        public static (ptr<Node>, long) AutoVar(ptr<ssa.Value> _addr_v)
        {
            ptr<Node> _p0 = default!;
            long _p0 = default;
            ref ssa.Value v = ref _addr_v.val;

            ssa.LocalSlot loc = v.Block.Func.RegAlloc[v.ID]._<ssa.LocalSlot>();
            if (v.Type.Size() > loc.Type.Size())
            {
                v.Fatalf("spill/restore type %s doesn't fit in slot type %s", v.Type, loc.Type);
            }

            return (loc.N._<ptr<Node>>(), loc.Off);

        }

        public static void AddrAuto(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v)
        {
            ref obj.Addr a = ref _addr_a.val;
            ref ssa.Value v = ref _addr_v.val;

            var (n, off) = AutoVar(_addr_v);
            a.Type = obj.TYPE_MEM;
            a.Sym = n.Sym.Linksym();
            a.Reg = int16(thearch.REGSP);
            a.Offset = n.Xoffset + off;
            if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
            {
                a.Name = obj.NAME_PARAM;
            }
            else
            {
                a.Name = obj.NAME_AUTO;
            }

        }

        private static void AddrScratch(this ptr<SSAGenState> _addr_s, ptr<obj.Addr> _addr_a) => func((_, panic, __) =>
        {
            ref SSAGenState s = ref _addr_s.val;
            ref obj.Addr a = ref _addr_a.val;

            if (s.ScratchFpMem == null)
            {
                panic("no scratch memory available; forgot to declare usesScratch for Op?");
            }

            a.Type = obj.TYPE_MEM;
            a.Name = obj.NAME_AUTO;
            a.Sym = s.ScratchFpMem.Sym.Linksym();
            a.Reg = int16(thearch.REGSP);
            a.Offset = s.ScratchFpMem.Xoffset;

        });

        // Call returns a new CALL instruction for the SSA value v.
        // It uses PrepareCall to prepare the call.
        private static ptr<obj.Prog> Call(this ptr<SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;

            var pPosIsStmt = s.pp.pos.IsStmt(); // The statement-ness fo the call comes from ssaGenState
            s.PrepareCall(v);

            var p = s.Prog(obj.ACALL);
            if (pPosIsStmt == src.PosIsStmt)
            {
                p.Pos = v.Pos.WithIsStmt();
            }
            else
            {
                p.Pos = v.Pos.WithNotStmt();
            }

            {
                ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();

                if (ok)
                {
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_EXTERN;
                    p.To.Sym = sym;
                }
                else
                { 
                    // TODO(mdempsky): Can these differences be eliminated?

                    if (thearch.LinkArch.Family == sys.AMD64 || thearch.LinkArch.Family == sys.I386 || thearch.LinkArch.Family == sys.PPC64 || thearch.LinkArch.Family == sys.RISCV64 || thearch.LinkArch.Family == sys.S390X || thearch.LinkArch.Family == sys.Wasm) 
                        p.To.Type = obj.TYPE_REG;
                    else if (thearch.LinkArch.Family == sys.ARM || thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.MIPS || thearch.LinkArch.Family == sys.MIPS64) 
                        p.To.Type = obj.TYPE_MEM;
                    else 
                        Fatalf("unknown indirect call family");
                                        p.To.Reg = v.Args[0L].Reg();

                }

            }

            return _addr_p!;

        }

        // PrepareCall prepares to emit a CALL instruction for v and does call-related bookkeeping.
        // It must be called immediately before emitting the actual CALL instruction,
        // since it emits PCDATA for the stack map at the call (calls are safe points).
        private static void PrepareCall(this ptr<SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;

            var idx = s.livenessMap.Get(v);
            if (!idx.StackMapValid())
            { 
                // See Liveness.hasStackMap.
                {
                    ptr<obj.LSym> sym__prev2 = sym;

                    ptr<obj.LSym> (sym, _) = v.Aux._<ptr<obj.LSym>>();

                    if (!(sym == typedmemclr || sym == typedmemmove))
                    {
                        Fatalf("missing stack map index for %v", v.LongString());
                    }

                    sym = sym__prev2;

                }

            }

            {
                ptr<obj.LSym> sym__prev1 = sym;

                (sym, _) = v.Aux._<ptr<obj.LSym>>();

                if (sym == Deferreturn)
                { 
                    // Deferred calls will appear to be returning to
                    // the CALL deferreturn(SB) that we are about to emit.
                    // However, the stack trace code will show the line
                    // of the instruction byte before the return PC.
                    // To avoid that being an unrelated instruction,
                    // insert an actual hardware NOP that will have the right line number.
                    // This is different from obj.ANOP, which is a virtual no-op
                    // that doesn't make it into the instruction stream.
                    thearch.Ginsnopdefer(s.pp);

                }

                sym = sym__prev1;

            }


            {
                ptr<obj.LSym> sym__prev1 = sym;

                ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();

                if (ok)
                { 
                    // Record call graph information for nowritebarrierrec
                    // analysis.
                    if (nowritebarrierrecCheck != null)
                    {
                        nowritebarrierrecCheck.recordCall(s.pp.curfn, sym, v.Pos);
                    }

                }

                sym = sym__prev1;

            }


            if (s.maxarg < v.AuxInt)
            {
                s.maxarg = v.AuxInt;
            }

        }

        // UseArgs records the fact that an instruction needs a certain amount of
        // callee args space for its use.
        private static void UseArgs(this ptr<SSAGenState> _addr_s, long n)
        {
            ref SSAGenState s = ref _addr_s.val;

            if (s.maxarg < n)
            {
                s.maxarg = n;
            }

        }

        // fieldIdx finds the index of the field referred to by the ODOT node n.
        private static long fieldIdx(ptr<Node> _addr_n) => func((_, panic, __) =>
        {
            ref Node n = ref _addr_n.val;

            var t = n.Left.Type;
            var f = n.Sym;
            if (!t.IsStruct())
            {
                panic("ODOT's LHS is not a struct");
            }

            long i = default;
            foreach (var (_, t1) in t.Fields().Slice())
            {
                if (t1.Sym != f)
                {
                    i++;
                    continue;
                }

                if (t1.Offset != n.Xoffset)
                {
                    panic("field offset doesn't match");
                }

                return i;

            }
            panic(fmt.Sprintf("can't find field in expr %v\n", n)); 

            // TODO: keep the result of this function somewhere in the ODOT Node
            // so we don't have to recompute it each time we need it.
        });

        // ssafn holds frontend information about a function that the backend is processing.
        // It also exports a bunch of compiler services for the ssa backend.
        private partial struct ssafn
        {
            public ptr<Node> curfn;
            public map<@string, ptr<obj.LSym>> strings; // map from constant string to data symbols
            public ptr<Node> scratchFpMem; // temp for floating point register / memory moves on some architectures
            public long stksize; // stack size for current frame
            public long stkptrsize; // prefix of stack containing pointers
            public bool log; // print ssa debug to the stdout
        }

        // StringData returns a symbol which
        // is the data component of a global string constant containing s.
        private static ptr<obj.LSym> StringData(this ptr<ssafn> _addr_e, @string s)
        {
            ref ssafn e = ref _addr_e.val;

            {
                var (aux, ok) = e.strings[s];

                if (ok)
                {
                    return _addr_aux!;
                }

            }

            if (e.strings == null)
            {
                e.strings = make_map<@string, ptr<obj.LSym>>();
            }

            var data = stringsym(e.curfn.Pos, s);
            e.strings[s] = data;
            return _addr_data!;

        }

        private static ssa.GCNode Auto(this ptr<ssafn> _addr_e, src.XPos pos, ptr<types.Type> _addr_t)
        {
            ref ssafn e = ref _addr_e.val;
            ref types.Type t = ref _addr_t.val;

            var n = tempAt(pos, e.curfn, t); // Note: adds new auto to e.curfn.Func.Dcl list
            return n;

        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitString(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var ptrType = types.NewPtr(types.Types[TUINT8]);
            var lenType = types.Types[TINT];
            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Split this string up into two separate variables.
                var p = e.splitSlot(_addr_name, ".ptr", 0L, ptrType);
                var l = e.splitSlot(_addr_name, ".len", ptrType.Size(), lenType);
                return (p, l);

            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:ptrType,Off:name.Off), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(Widthptr)));

        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitInterface(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var u = types.Types[TUINTPTR];
            var t = types.NewPtr(types.Types[TUINT8]);
            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Split this interface up into two separate variables.
                @string f = ".itab";
                if (n.Type.IsEmptyInterface())
                {
                    f = ".type";
                }

                var c = e.splitSlot(_addr_name, f, 0L, u); // see comment in plive.go:onebitwalktype1.
                var d = e.splitSlot(_addr_name, ".data", u.Size(), t);
                return (c, d);

            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:u,Off:name.Off), new ssa.LocalSlot(N:n,Type:t,Off:name.Off+int64(Widthptr)));

        }

        private static (ssa.LocalSlot, ssa.LocalSlot, ssa.LocalSlot) SplitSlice(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var ptrType = types.NewPtr(name.Type.Elem());
            var lenType = types.Types[TINT];
            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Split this slice up into three separate variables.
                var p = e.splitSlot(_addr_name, ".ptr", 0L, ptrType);
                var l = e.splitSlot(_addr_name, ".len", ptrType.Size(), lenType);
                var c = e.splitSlot(_addr_name, ".cap", ptrType.Size() + lenType.Size(), lenType);
                return (p, l, c);

            } 
            // Return the three parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:ptrType,Off:name.Off), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(Widthptr)), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(2*Widthptr)));

        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitComplex(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var s = name.Type.Size() / 2L;
            ptr<types.Type> t;
            if (s == 8L)
            {
                t = types.Types[TFLOAT64];
            }
            else
            {
                t = types.Types[TFLOAT32];
            }

            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Split this complex up into two separate variables.
                var r = e.splitSlot(_addr_name, ".real", 0L, t);
                var i = e.splitSlot(_addr_name, ".imag", t.Size(), t);
                return (r, i);

            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off), new ssa.LocalSlot(N:n,Type:t,Off:name.Off+s));

        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitInt64(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ssa.LocalSlot _p0 = default;
            ssa.LocalSlot _p0 = default;
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            ptr<types.Type> t;
            if (name.Type.IsSigned())
            {
                t = types.Types[TINT32];
            }
            else
            {
                t = types.Types[TUINT32];
            }

            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Split this int64 up into two separate variables.
                if (thearch.LinkArch.ByteOrder == binary.BigEndian)
                {
                    return (e.splitSlot(_addr_name, ".hi", 0L, t), e.splitSlot(_addr_name, ".lo", t.Size(), types.Types[TUINT32]));
                }

                return (e.splitSlot(_addr_name, ".hi", t.Size(), t), e.splitSlot(_addr_name, ".lo", 0L, types.Types[TUINT32]));

            } 
            // Return the two parts of the larger variable.
            if (thearch.LinkArch.ByteOrder == binary.BigEndian)
            {
                return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off), new ssa.LocalSlot(N:n,Type:types.Types[TUINT32],Off:name.Off+4));
            }

            return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off+4), new ssa.LocalSlot(N:n,Type:types.Types[TUINT32],Off:name.Off));

        }

        private static ssa.LocalSlot SplitStruct(this ptr<ssafn> _addr_e, ssa.LocalSlot name, long i)
        {
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var st = name.Type;
            var ft = st.FieldType(i);
            long offset = default;
            for (long f = 0L; f < i; f++)
            {
                offset += st.FieldType(f).Size();
            }

            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            { 
                // Note: the _ field may appear several times.  But
                // have no fear, identically-named but distinct Autos are
                // ok, albeit maybe confusing for a debugger.
                return e.splitSlot(_addr_name, "." + st.FieldName(i), offset, ft);

            }

            return new ssa.LocalSlot(N:n,Type:ft,Off:name.Off+st.FieldOff(i));

        }

        private static ssa.LocalSlot SplitArray(this ptr<ssafn> _addr_e, ssa.LocalSlot name)
        {
            ref ssafn e = ref _addr_e.val;

            ptr<Node> n = name.N._<ptr<Node>>();
            var at = name.Type;
            if (at.NumElem() != 1L)
            {
                e.Fatalf(n.Pos, "bad array size");
            }

            var et = at.Elem();
            if (n.Class() == PAUTO && !n.Name.Addrtaken())
            {
                return e.splitSlot(_addr_name, "[0]", 0L, et);
            }

            return new ssa.LocalSlot(N:n,Type:et,Off:name.Off);

        }

        private static ptr<obj.LSym> DerefItab(this ptr<ssafn> _addr_e, ptr<obj.LSym> _addr_it, long offset)
        {
            ref ssafn e = ref _addr_e.val;
            ref obj.LSym it = ref _addr_it.val;

            return _addr_itabsym(it, offset)!;
        }

        // splitSlot returns a slot representing the data of parent starting at offset.
        private static ssa.LocalSlot splitSlot(this ptr<ssafn> _addr_e, ptr<ssa.LocalSlot> _addr_parent, @string suffix, long offset, ptr<types.Type> _addr_t)
        {
            ref ssafn e = ref _addr_e.val;
            ref ssa.LocalSlot parent = ref _addr_parent.val;
            ref types.Type t = ref _addr_t.val;

            ptr<types.Sym> s = addr(new types.Sym(Name:parent.N.(*Node).Sym.Name+suffix,Pkg:localpkg));

            ptr<Node> n = addr(new Node(Name:new(Name),Op:ONAME,Pos:parent.N.(*Node).Pos,));
            n.Orig = n;

            s.Def = asTypesNode(n);
            asNode(s.Def).Name.SetUsed(true);
            n.Sym = s;
            n.Type = t;
            n.SetClass(PAUTO);
            n.Esc = EscNever;
            n.Name.Curfn = e.curfn;
            e.curfn.Func.Dcl = append(e.curfn.Func.Dcl, n);
            dowidth(t);
            return new ssa.LocalSlot(N:n,Type:t,Off:0,SplitOf:parent,SplitOffset:offset);
        }

        private static bool CanSSA(this ptr<ssafn> _addr_e, ptr<types.Type> _addr_t)
        {
            ref ssafn e = ref _addr_e.val;
            ref types.Type t = ref _addr_t.val;

            return canSSAType(_addr_t);
        }

        private static @string Line(this ptr<ssafn> _addr_e, src.XPos pos)
        {
            ref ssafn e = ref _addr_e.val;

            return linestr(pos);
        }

        // Log logs a message from the compiler.
        private static void Logf(this ptr<ssafn> _addr_e, @string msg, params object[] args)
        {
            args = args.Clone();
            ref ssafn e = ref _addr_e.val;

            if (e.log)
            {
                fmt.Printf(msg, args);
            }

        }

        private static bool Log(this ptr<ssafn> _addr_e)
        {
            ref ssafn e = ref _addr_e.val;

            return e.log;
        }

        // Fatal reports a compiler error and exits.
        private static void Fatalf(this ptr<ssafn> _addr_e, src.XPos pos, @string msg, params object[] args)
        {
            args = args.Clone();
            ref ssafn e = ref _addr_e.val;

            lineno = pos;
            var nargs = append(args);
            Fatalf("'%s': " + msg, nargs);
        }

        // Warnl reports a "warning", which is usually flag-triggered
        // logging output for the benefit of tests.
        private static void Warnl(this ptr<ssafn> _addr_e, src.XPos pos, @string fmt_, params object[] args)
        {
            args = args.Clone();
            ref ssafn e = ref _addr_e.val;

            Warnl(pos, fmt_, args);
        }

        private static bool Debug_checknil(this ptr<ssafn> _addr_e)
        {
            ref ssafn e = ref _addr_e.val;

            return Debug_checknil != 0L;
        }

        private static bool UseWriteBarrier(this ptr<ssafn> _addr_e)
        {
            ref ssafn e = ref _addr_e.val;

            return use_writebarrier;
        }

        private static ptr<obj.LSym> Syslook(this ptr<ssafn> _addr_e, @string name)
        {
            ref ssafn e = ref _addr_e.val;

            switch (name)
            {
                case "goschedguarded": 
                    return _addr_goschedguarded!;
                    break;
                case "writeBarrier": 
                    return _addr_writeBarrier!;
                    break;
                case "gcWriteBarrier": 
                    return _addr_gcWriteBarrier!;
                    break;
                case "typedmemmove": 
                    return _addr_typedmemmove!;
                    break;
                case "typedmemclr": 
                    return _addr_typedmemclr!;
                    break;
            }
            e.Fatalf(src.NoXPos, "unknown Syslook func %v", name);
            return _addr_null!;

        }

        private static void SetWBPos(this ptr<ssafn> _addr_e, src.XPos pos)
        {
            ref ssafn e = ref _addr_e.val;

            e.curfn.Func.setWBPos(pos);
        }

        private static ptr<types.Type> Typ(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return _addr_n.Type!;
        }
        private static ssa.StorageClass StorageClass(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Class() == PPARAM) 
                return ssa.ClassParam;
            else if (n.Class() == PPARAMOUT) 
                return ssa.ClassParamOut;
            else if (n.Class() == PAUTO) 
                return ssa.ClassAuto;
            else 
                Fatalf("untranslatable storage class for %v: %s", n, n.Class());
                return 0L;
            
        }

        private static ptr<Node> clobberBase(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op == ODOT && n.Left.Type.NumFields() == 1L)
            {
                return _addr_clobberBase(_addr_n.Left)!;
            }

            if (n.Op == OINDEX && n.Left.Type.IsArray() && n.Left.Type.NumElem() == 1L)
            {
                return _addr_clobberBase(_addr_n.Left)!;
            }

            return _addr_n!;

        }
    }
}}}}
