// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssagen -- go2cs converted at 2022 March 13 06:23:54 UTC
// import "cmd/compile/internal/ssagen" ==> using ssagen = go.cmd.compile.@internal.ssagen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssagen\ssa.go
namespace go.cmd.compile.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using abi = cmd.compile.@internal.abi_package;
using fmt = fmt_package;
using constant = go.constant_package;
using html = html_package;
using buildcfg = @internal.buildcfg_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using liveness = cmd.compile.@internal.liveness_package;
using objw = cmd.compile.@internal.objw_package;
using reflectdata = cmd.compile.@internal.reflectdata_package;
using ssa = cmd.compile.@internal.ssa_package;
using staticdata = cmd.compile.@internal.staticdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using x86 = cmd.@internal.obj.x86_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;
using sys = cmd.@internal.sys_package;
using System;

public static partial class ssagen_package {

private static ptr<ssa.Config> ssaConfig;
private static slice<ssa.Cache> ssaCaches = default;

private static @string ssaDump = default; // early copy of $GOSSAFUNC; the func name to dump output for
private static @string ssaDir = default; // optional destination for ssa dump file
private static bool ssaDumpStdout = default; // whether to dump to stdout
private static @string ssaDumpCFG = default; // generate CFGs for these phases
private static readonly @string ssaDumpFile = "ssa.html";

// ssaDumpInlined holds all inlined functions when ssaDump contains a function name.


// ssaDumpInlined holds all inlined functions when ssaDump contains a function name.
private static slice<ptr<ir.Func>> ssaDumpInlined = default;

public static void DumpInline(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (ssaDump != "" && ssaDump == ir.FuncName(fn)) {
        ssaDumpInlined = append(ssaDumpInlined, fn);
    }
}

public static void InitEnv() {
    ssaDump = os.Getenv("GOSSAFUNC");
    ssaDir = os.Getenv("GOSSADIR");
    if (ssaDump != "") {
        if (strings.HasSuffix(ssaDump, "+")) {
            ssaDump = ssaDump[..(int)len(ssaDump) - 1];
            ssaDumpStdout = true;
        }
        var spl = strings.Split(ssaDump, ":");
        if (len(spl) > 1) {
            ssaDump = spl[0];
            ssaDumpCFG = spl[1];
        }
    }
}

public static void InitConfig() {
    var types_ = ssa.NewTypes();

    if (Arch.SoftFloat) {
        softfloatInit();
    }
    _ = types.NewPtr(types.Types[types.TINTER]); // *interface{}
    _ = types.NewPtr(types.NewPtr(types.Types[types.TSTRING])); // **string
    _ = types.NewPtr(types.NewSlice(types.Types[types.TINTER])); // *[]interface{}
    _ = types.NewPtr(types.NewPtr(types.ByteType)); // **byte
    _ = types.NewPtr(types.NewSlice(types.ByteType)); // *[]byte
    _ = types.NewPtr(types.NewSlice(types.Types[types.TSTRING])); // *[]string
    _ = types.NewPtr(types.NewPtr(types.NewPtr(types.Types[types.TUINT8]))); // ***uint8
    _ = types.NewPtr(types.Types[types.TINT16]); // *int16
    _ = types.NewPtr(types.Types[types.TINT64]); // *int64
    _ = types.NewPtr(types.ErrorType); // *error
    types.NewPtrCacheEnabled = false;
    ssaConfig = ssa.NewConfig(@base.Ctxt.Arch.Name, types_.val, @base.Ctxt, @base.Flag.N == 0);
    ssaConfig.SoftFloat = Arch.SoftFloat;
    ssaConfig.Race = @base.Flag.Race;
    ssaCaches = make_slice<ssa.Cache>(@base.Flag.LowerC); 

    // Set up some runtime functions we'll need to call.
    ir.Syms.AssertE2I = typecheck.LookupRuntimeFunc("assertE2I");
    ir.Syms.AssertE2I2 = typecheck.LookupRuntimeFunc("assertE2I2");
    ir.Syms.AssertI2I = typecheck.LookupRuntimeFunc("assertI2I");
    ir.Syms.AssertI2I2 = typecheck.LookupRuntimeFunc("assertI2I2");
    ir.Syms.Deferproc = typecheck.LookupRuntimeFunc("deferproc");
    ir.Syms.DeferprocStack = typecheck.LookupRuntimeFunc("deferprocStack");
    ir.Syms.Deferreturn = typecheck.LookupRuntimeFunc("deferreturn");
    ir.Syms.Duffcopy = typecheck.LookupRuntimeFunc("duffcopy");
    ir.Syms.Duffzero = typecheck.LookupRuntimeFunc("duffzero");
    ir.Syms.GCWriteBarrier = typecheck.LookupRuntimeFunc("gcWriteBarrier");
    ir.Syms.Goschedguarded = typecheck.LookupRuntimeFunc("goschedguarded");
    ir.Syms.Growslice = typecheck.LookupRuntimeFunc("growslice");
    ir.Syms.Msanread = typecheck.LookupRuntimeFunc("msanread");
    ir.Syms.Msanwrite = typecheck.LookupRuntimeFunc("msanwrite");
    ir.Syms.Msanmove = typecheck.LookupRuntimeFunc("msanmove");
    ir.Syms.Newobject = typecheck.LookupRuntimeFunc("newobject");
    ir.Syms.Newproc = typecheck.LookupRuntimeFunc("newproc");
    ir.Syms.Panicdivide = typecheck.LookupRuntimeFunc("panicdivide");
    ir.Syms.PanicdottypeE = typecheck.LookupRuntimeFunc("panicdottypeE");
    ir.Syms.PanicdottypeI = typecheck.LookupRuntimeFunc("panicdottypeI");
    ir.Syms.Panicnildottype = typecheck.LookupRuntimeFunc("panicnildottype");
    ir.Syms.Panicoverflow = typecheck.LookupRuntimeFunc("panicoverflow");
    ir.Syms.Panicshift = typecheck.LookupRuntimeFunc("panicshift");
    ir.Syms.Raceread = typecheck.LookupRuntimeFunc("raceread");
    ir.Syms.Racereadrange = typecheck.LookupRuntimeFunc("racereadrange");
    ir.Syms.Racewrite = typecheck.LookupRuntimeFunc("racewrite");
    ir.Syms.Racewriterange = typecheck.LookupRuntimeFunc("racewriterange");
    ir.Syms.X86HasPOPCNT = typecheck.LookupRuntimeVar("x86HasPOPCNT"); // bool
    ir.Syms.X86HasSSE41 = typecheck.LookupRuntimeVar("x86HasSSE41"); // bool
    ir.Syms.X86HasFMA = typecheck.LookupRuntimeVar("x86HasFMA"); // bool
    ir.Syms.ARMHasVFPv4 = typecheck.LookupRuntimeVar("armHasVFPv4"); // bool
    ir.Syms.ARM64HasATOMICS = typecheck.LookupRuntimeVar("arm64HasATOMICS"); // bool
    ir.Syms.Staticuint64s = typecheck.LookupRuntimeVar("staticuint64s");
    ir.Syms.Typedmemclr = typecheck.LookupRuntimeFunc("typedmemclr");
    ir.Syms.Typedmemmove = typecheck.LookupRuntimeFunc("typedmemmove");
    ir.Syms.Udiv = typecheck.LookupRuntimeVar("udiv"); // asm func with special ABI
    ir.Syms.WriteBarrier = typecheck.LookupRuntimeVar("writeBarrier"); // struct { bool; ... }
    ir.Syms.Zerobase = typecheck.LookupRuntimeVar("zerobase"); 

    // asm funcs with special ABI
    if (@base.Ctxt.Arch.Name == "amd64") {
        GCWriteBarrierReg = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<short, ptr<obj.LSym>>{x86.REG_AX:typecheck.LookupRuntimeFunc("gcWriteBarrier"),x86.REG_CX:typecheck.LookupRuntimeFunc("gcWriteBarrierCX"),x86.REG_DX:typecheck.LookupRuntimeFunc("gcWriteBarrierDX"),x86.REG_BX:typecheck.LookupRuntimeFunc("gcWriteBarrierBX"),x86.REG_BP:typecheck.LookupRuntimeFunc("gcWriteBarrierBP"),x86.REG_SI:typecheck.LookupRuntimeFunc("gcWriteBarrierSI"),x86.REG_R8:typecheck.LookupRuntimeFunc("gcWriteBarrierR8"),x86.REG_R9:typecheck.LookupRuntimeFunc("gcWriteBarrierR9"),};
    }
    if (Arch.LinkArch.Family == sys.Wasm) {
        BoundsCheckFunc[ssa.BoundsIndex] = typecheck.LookupRuntimeFunc("goPanicIndex");
        BoundsCheckFunc[ssa.BoundsIndexU] = typecheck.LookupRuntimeFunc("goPanicIndexU");
        BoundsCheckFunc[ssa.BoundsSliceAlen] = typecheck.LookupRuntimeFunc("goPanicSliceAlen");
        BoundsCheckFunc[ssa.BoundsSliceAlenU] = typecheck.LookupRuntimeFunc("goPanicSliceAlenU");
        BoundsCheckFunc[ssa.BoundsSliceAcap] = typecheck.LookupRuntimeFunc("goPanicSliceAcap");
        BoundsCheckFunc[ssa.BoundsSliceAcapU] = typecheck.LookupRuntimeFunc("goPanicSliceAcapU");
        BoundsCheckFunc[ssa.BoundsSliceB] = typecheck.LookupRuntimeFunc("goPanicSliceB");
        BoundsCheckFunc[ssa.BoundsSliceBU] = typecheck.LookupRuntimeFunc("goPanicSliceBU");
        BoundsCheckFunc[ssa.BoundsSlice3Alen] = typecheck.LookupRuntimeFunc("goPanicSlice3Alen");
        BoundsCheckFunc[ssa.BoundsSlice3AlenU] = typecheck.LookupRuntimeFunc("goPanicSlice3AlenU");
        BoundsCheckFunc[ssa.BoundsSlice3Acap] = typecheck.LookupRuntimeFunc("goPanicSlice3Acap");
        BoundsCheckFunc[ssa.BoundsSlice3AcapU] = typecheck.LookupRuntimeFunc("goPanicSlice3AcapU");
        BoundsCheckFunc[ssa.BoundsSlice3B] = typecheck.LookupRuntimeFunc("goPanicSlice3B");
        BoundsCheckFunc[ssa.BoundsSlice3BU] = typecheck.LookupRuntimeFunc("goPanicSlice3BU");
        BoundsCheckFunc[ssa.BoundsSlice3C] = typecheck.LookupRuntimeFunc("goPanicSlice3C");
        BoundsCheckFunc[ssa.BoundsSlice3CU] = typecheck.LookupRuntimeFunc("goPanicSlice3CU");
        BoundsCheckFunc[ssa.BoundsConvert] = typecheck.LookupRuntimeFunc("goPanicSliceConvert");
    }
    else
 {
        BoundsCheckFunc[ssa.BoundsIndex] = typecheck.LookupRuntimeFunc("panicIndex");
        BoundsCheckFunc[ssa.BoundsIndexU] = typecheck.LookupRuntimeFunc("panicIndexU");
        BoundsCheckFunc[ssa.BoundsSliceAlen] = typecheck.LookupRuntimeFunc("panicSliceAlen");
        BoundsCheckFunc[ssa.BoundsSliceAlenU] = typecheck.LookupRuntimeFunc("panicSliceAlenU");
        BoundsCheckFunc[ssa.BoundsSliceAcap] = typecheck.LookupRuntimeFunc("panicSliceAcap");
        BoundsCheckFunc[ssa.BoundsSliceAcapU] = typecheck.LookupRuntimeFunc("panicSliceAcapU");
        BoundsCheckFunc[ssa.BoundsSliceB] = typecheck.LookupRuntimeFunc("panicSliceB");
        BoundsCheckFunc[ssa.BoundsSliceBU] = typecheck.LookupRuntimeFunc("panicSliceBU");
        BoundsCheckFunc[ssa.BoundsSlice3Alen] = typecheck.LookupRuntimeFunc("panicSlice3Alen");
        BoundsCheckFunc[ssa.BoundsSlice3AlenU] = typecheck.LookupRuntimeFunc("panicSlice3AlenU");
        BoundsCheckFunc[ssa.BoundsSlice3Acap] = typecheck.LookupRuntimeFunc("panicSlice3Acap");
        BoundsCheckFunc[ssa.BoundsSlice3AcapU] = typecheck.LookupRuntimeFunc("panicSlice3AcapU");
        BoundsCheckFunc[ssa.BoundsSlice3B] = typecheck.LookupRuntimeFunc("panicSlice3B");
        BoundsCheckFunc[ssa.BoundsSlice3BU] = typecheck.LookupRuntimeFunc("panicSlice3BU");
        BoundsCheckFunc[ssa.BoundsSlice3C] = typecheck.LookupRuntimeFunc("panicSlice3C");
        BoundsCheckFunc[ssa.BoundsSlice3CU] = typecheck.LookupRuntimeFunc("panicSlice3CU");
        BoundsCheckFunc[ssa.BoundsConvert] = typecheck.LookupRuntimeFunc("panicSliceConvert");
    }
    if (Arch.LinkArch.PtrSize == 4) {
        ExtendCheckFunc[ssa.BoundsIndex] = typecheck.LookupRuntimeVar("panicExtendIndex");
        ExtendCheckFunc[ssa.BoundsIndexU] = typecheck.LookupRuntimeVar("panicExtendIndexU");
        ExtendCheckFunc[ssa.BoundsSliceAlen] = typecheck.LookupRuntimeVar("panicExtendSliceAlen");
        ExtendCheckFunc[ssa.BoundsSliceAlenU] = typecheck.LookupRuntimeVar("panicExtendSliceAlenU");
        ExtendCheckFunc[ssa.BoundsSliceAcap] = typecheck.LookupRuntimeVar("panicExtendSliceAcap");
        ExtendCheckFunc[ssa.BoundsSliceAcapU] = typecheck.LookupRuntimeVar("panicExtendSliceAcapU");
        ExtendCheckFunc[ssa.BoundsSliceB] = typecheck.LookupRuntimeVar("panicExtendSliceB");
        ExtendCheckFunc[ssa.BoundsSliceBU] = typecheck.LookupRuntimeVar("panicExtendSliceBU");
        ExtendCheckFunc[ssa.BoundsSlice3Alen] = typecheck.LookupRuntimeVar("panicExtendSlice3Alen");
        ExtendCheckFunc[ssa.BoundsSlice3AlenU] = typecheck.LookupRuntimeVar("panicExtendSlice3AlenU");
        ExtendCheckFunc[ssa.BoundsSlice3Acap] = typecheck.LookupRuntimeVar("panicExtendSlice3Acap");
        ExtendCheckFunc[ssa.BoundsSlice3AcapU] = typecheck.LookupRuntimeVar("panicExtendSlice3AcapU");
        ExtendCheckFunc[ssa.BoundsSlice3B] = typecheck.LookupRuntimeVar("panicExtendSlice3B");
        ExtendCheckFunc[ssa.BoundsSlice3BU] = typecheck.LookupRuntimeVar("panicExtendSlice3BU");
        ExtendCheckFunc[ssa.BoundsSlice3C] = typecheck.LookupRuntimeVar("panicExtendSlice3C");
        ExtendCheckFunc[ssa.BoundsSlice3CU] = typecheck.LookupRuntimeVar("panicExtendSlice3CU");
    }
    ir.Syms.WasmMove = typecheck.LookupRuntimeVar("wasmMove");
    ir.Syms.WasmZero = typecheck.LookupRuntimeVar("wasmZero");
    ir.Syms.WasmDiv = typecheck.LookupRuntimeVar("wasmDiv");
    ir.Syms.WasmTruncS = typecheck.LookupRuntimeVar("wasmTruncS");
    ir.Syms.WasmTruncU = typecheck.LookupRuntimeVar("wasmTruncU");
    ir.Syms.SigPanic = typecheck.LookupRuntimeFunc("sigpanic");
}

// AbiForBodylessFuncStackMap returns the ABI for a bodyless function's stack map.
// This is not necessarily the ABI used to call it.
// Currently (1.17 dev) such a stack map is always ABI0;
// any ABI wrapper that is present is nosplit, hence a precise
// stack map is not needed there (the parameters survive only long
// enough to call the wrapped assembly function).
// This always returns a freshly copied ABI.
public static ptr<abi.ABIConfig> AbiForBodylessFuncStackMap(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    return _addr_ssaConfig.ABI0.Copy()!; // No idea what races will result, be safe
}

// These are disabled but remain ready for use in case they are needed for the next regabi port.
// TODO if they are not needed for 1.18 / next register abi port, delete them.
private static readonly @string magicNameDotSuffix = ".*disabled*MagicMethodNameForTestingRegisterABI";

private static readonly @string magicLastTypeName = "*disabled*MagicLastTypeNameForTestingRegisterABI";

// abiForFunc implements ABI policy for a function, but does not return a copy of the ABI.
// Passing a nil function returns the default ABI based on experiment configuration.


// abiForFunc implements ABI policy for a function, but does not return a copy of the ABI.
// Passing a nil function returns the default ABI based on experiment configuration.
private static ptr<abi.ABIConfig> abiForFunc(ptr<ir.Func> _addr_fn, ptr<abi.ABIConfig> _addr_abi0, ptr<abi.ABIConfig> _addr_abi1) => func((_, panic, _) => {
    ref ir.Func fn = ref _addr_fn.val;
    ref abi.ABIConfig abi0 = ref _addr_abi0.val;
    ref abi.ABIConfig abi1 = ref _addr_abi1.val;

    if (buildcfg.Experiment.RegabiArgs) { 
        // Select the ABI based on the function's defining ABI.
        if (fn == null) {
            return _addr_abi1!;
        }

        if (fn.ABI == obj.ABI0) 
            return _addr_abi0!;
        else if (fn.ABI == obj.ABIInternal) 
            // TODO(austin): Clean up the nomenclature here.
            // It's not clear that "abi1" is ABIInternal.
            return _addr_abi1!;
                @base.Fatalf("function %v has unknown ABI %v", fn, fn.ABI);
        panic("not reachable");
    }
    var a = abi0;
    if (fn != null) {
        var name = ir.FuncName(fn);
        var magicName = strings.HasSuffix(name, magicNameDotSuffix);
        if (fn.Pragma & ir.RegisterParams != 0) { // TODO(register args) remove after register abi is working
            if (strings.Contains(name, ".")) {
                if (!magicName) {
                    @base.ErrorfAt(fn.Pos(), "Calls to //go:registerparams method %s won't work, remove the pragma from the declaration.", name);
                }
            }
            a = abi1;
        }
        else if (magicName) {
            if (@base.FmtPos(fn.Pos()) == "<autogenerated>:1") { 
                // no way to put a pragma here, and it will error out in the real source code if they did not do it there.
                a = abi1;
            }
            else
 {
                @base.ErrorfAt(fn.Pos(), "Methods with magic name %s (method %s) must also specify //go:registerparams", magicNameDotSuffix[(int)1..], name);
            }
        }
        if (regAbiForFuncType(_addr_fn.Type().FuncType())) { 
            // fmt.Printf("Saw magic last type name for function %s\n", name)
            a = abi1;
        }
    }
    return _addr_a!;
});

private static bool regAbiForFuncType(ptr<types.Func> _addr_ft) {
    ref types.Func ft = ref _addr_ft.val;

    var np = ft.Params.NumFields();
    return np > 0 && strings.Contains(ft.Params.FieldType(np - 1).String(), magicLastTypeName);
}

// getParam returns the Field of ith param of node n (which is a
// function/method/interface call), where the receiver of a method call is
// considered as the 0th parameter. This does not include the receiver of an
// interface call.
private static ptr<types.Field> getParam(ptr<ir.CallExpr> _addr_n, nint i) {
    ref ir.CallExpr n = ref _addr_n.val;

    var t = n.X.Type();
    if (n.Op() == ir.OCALLMETH) {
        @base.Fatalf("OCALLMETH missed by walkCall");
    }
    return _addr_t.Params().Field(i)!;
}

// dvarint writes a varint v to the funcdata in symbol x and returns the new offset
private static nint dvarint(ptr<obj.LSym> _addr_x, nint off, long v) => func((_, panic, _) => {
    ref obj.LSym x = ref _addr_x.val;

    if (v < 0 || v > 1e9F) {
        panic(fmt.Sprintf("dvarint: bad offset for funcdata - %v", v));
    }
    if (v < 1 << 7) {
        return objw.Uint8(x, off, uint8(v));
    }
    off = objw.Uint8(x, off, uint8((v & 127) | 128));
    if (v < 1 << 14) {
        return objw.Uint8(x, off, uint8(v >> 7));
    }
    off = objw.Uint8(x, off, uint8(((v >> 7) & 127) | 128));
    if (v < 1 << 21) {
        return objw.Uint8(x, off, uint8(v >> 14));
    }
    off = objw.Uint8(x, off, uint8(((v >> 14) & 127) | 128));
    if (v < 1 << 28) {
        return objw.Uint8(x, off, uint8(v >> 21));
    }
    off = objw.Uint8(x, off, uint8(((v >> 21) & 127) | 128));
    return objw.Uint8(x, off, uint8(v >> 28));
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
private static void emitOpenDeferInfo(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    var x = @base.Ctxt.Lookup(s.curfn.LSym.Name + ".opendefer");
    s.curfn.LSym.Func().OpenCodedDeferInfo = x;
    nint off = 0; 

    // Compute maxargsize (max size of arguments for all defers)
    // first, so we can output it first to the funcdata
    long maxargsize = default;
    {
        var i__prev1 = i;

        for (var i = len(s.openDefers) - 1; i >= 0; i--) {
            var r = s.openDefers[i];
            var argsize = r.n.X.Type().ArgWidth(); // TODO register args: but maybe use of abi0 will make this easy
            if (argsize > maxargsize) {
                maxargsize = argsize;
            }
        }

        i = i__prev1;
    }
    off = dvarint(_addr_x, off, maxargsize);
    off = dvarint(_addr_x, off, -s.deferBitsTemp.FrameOffset());
    off = dvarint(_addr_x, off, int64(len(s.openDefers))); 

    // Write in reverse-order, for ease of running in that order at runtime
    {
        var i__prev1 = i;

        for (i = len(s.openDefers) - 1; i >= 0; i--) {
            r = s.openDefers[i];
            off = dvarint(_addr_x, off, r.n.X.Type().ArgWidth());
            off = dvarint(_addr_x, off, -r.closureNode.FrameOffset());
            var numArgs = len(r.argNodes);
            if (r.rcvrNode != null) { 
                // If there's an interface receiver, treat/place it as the first
                // arg. (If there is a method receiver, it's already included as
                // first arg in r.argNodes.)
                numArgs++;
            }
            off = dvarint(_addr_x, off, int64(numArgs));
            nint argAdjust = 0; // presence of receiver offsets the parameter count.
            if (r.rcvrNode != null) {
                off = dvarint(_addr_x, off, -okOffset(r.rcvrNode.FrameOffset()));
                off = dvarint(_addr_x, off, s.config.PtrSize);
                off = dvarint(_addr_x, off, 0); // This is okay because defer records use ABI0 (for now)
                argAdjust++;
            } 

            // TODO(register args) assume abi0 for this?
            var ab = s.f.ABI0;
            var pri = ab.ABIAnalyzeFuncType(r.n.X.Type().FuncType());
            foreach (var (j, arg) in r.argNodes) {
                var f = getParam(_addr_r.n, j);
                off = dvarint(_addr_x, off, -okOffset(arg.FrameOffset()));
                off = dvarint(_addr_x, off, f.Type.Size());
                off = dvarint(_addr_x, off, okOffset(pri.InParam(j + argAdjust).FrameOffset(pri)));
            }
        }

        i = i__prev1;
    }
}

private static long okOffset(long offset) => func((_, panic, _) => {
    if (offset == types.BOGUS_FUNARG_OFFSET) {
        panic(fmt.Errorf("Bogus offset %d", offset));
    }
    return offset;
});

// buildssa builds an SSA function for fn.
// worker indicates which of the backend workers is doing the processing.
private static ptr<ssa.Func> buildssa(ptr<ir.Func> _addr_fn, nint worker) => func((defer, _, _) => {
    ref ir.Func fn = ref _addr_fn.val;

    var name = ir.FuncName(fn);
    var printssa = false;
    if (ssaDump != "") { // match either a simple name e.g. "(*Reader).Reset", package.name e.g. "compress/gzip.(*Reader).Reset", or subpackage name "gzip.(*Reader).Reset"
        var pkgDotName = @base.Ctxt.Pkgpath + "." + name;
        printssa = name == ssaDump || strings.HasSuffix(pkgDotName, ssaDump) && (pkgDotName == ssaDump || strings.HasSuffix(pkgDotName, "/" + ssaDump));
    }
    ptr<bytes.Buffer> astBuf;
    if (printssa) {
        astBuf = addr(new bytes.Buffer());
        ir.FDumpList(astBuf, "buildssa-enter", fn.Enter);
        ir.FDumpList(astBuf, "buildssa-body", fn.Body);
        ir.FDumpList(astBuf, "buildssa-exit", fn.Exit);
        if (ssaDumpStdout) {
            fmt.Println("generating SSA for", name);
            fmt.Print(astBuf.String());
        }
    }
    state s = default;
    s.pushLine(fn.Pos());
    defer(s.popLine());

    s.hasdefer = fn.HasDefer();
    if (fn.Pragma & ir.CgoUnsafeArgs != 0) {
        s.cgoUnsafeArgs = true;
    }
    ref ssafn fe = ref heap(new ssafn(curfn:fn,log:printssa&&ssaDumpStdout,), out ptr<ssafn> _addr_fe);
    s.curfn = fn;

    s.f = ssa.NewFunc(_addr_fe);
    s.config = ssaConfig;
    s.f.Type = fn.Type();
    s.f.Config = ssaConfig;
    s.f.Cache = _addr_ssaCaches[worker];
    s.f.Cache.Reset();
    s.f.Name = name;
    s.f.DebugTest = s.f.DebugHashMatch("GOSSAHASH");
    s.f.PrintOrHtmlSSA = printssa;
    if (fn.Pragma & ir.Nosplit != 0) {
        s.f.NoSplit = true;
    }
    s.f.ABI0 = ssaConfig.ABI0.Copy(); // Make a copy to avoid racy map operations in type-register-width cache.
    s.f.ABI1 = ssaConfig.ABI1.Copy();
    s.f.ABIDefault = abiForFunc(_addr_null, _addr_s.f.ABI0, _addr_s.f.ABI1);
    s.f.ABISelf = abiForFunc(_addr_fn, _addr_s.f.ABI0, _addr_s.f.ABI1);

    s.panics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<funcLine, ptr<ssa.Block>>{};
    s.softFloat = s.config.SoftFloat; 

    // Allocate starting block
    s.f.Entry = s.f.NewBlock(ssa.BlockPlain);
    s.f.Entry.Pos = fn.Pos();

    if (printssa) {
        var ssaDF = ssaDumpFile;
        if (ssaDir != "") {
            ssaDF = filepath.Join(ssaDir, @base.Ctxt.Pkgpath + "." + name + ".html");
            var ssaD = filepath.Dir(ssaDF);
            os.MkdirAll(ssaD, 0755);
        }
        s.f.HTMLWriter = ssa.NewHTMLWriter(ssaDF, s.f, ssaDumpCFG); 
        // TODO: generate and print a mapping from nodes to values and blocks
        dumpSourcesColumn(_addr_s.f.HTMLWriter, _addr_fn);
        s.f.HTMLWriter.WriteAST("AST", astBuf);
    }
    s.labels = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<ssaLabel>>{};
    s.fwdVars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ir.Node, ptr<ssa.Value>>{};
    s.startmem = s.entryNewValue0(ssa.OpInitMem, types.TypeMem);

    s.hasOpenDefers = @base.Flag.N == 0 && s.hasdefer && !s.curfn.OpenCodedDeferDisallowed();

    if (@base.Debug.NoOpenDefer != 0) 
        s.hasOpenDefers = false;
    else if (s.hasOpenDefers && (@base.Ctxt.Flag_shared || @base.Ctxt.Flag_dynlink) && @base.Ctxt.Arch.Name == "386") 
        // Don't support open-coded defers for 386 ONLY when using shared
        // libraries, because there is extra code (added by rewriteToUseGot())
        // preceding the deferreturn/ret code that we don't track correctly.
        s.hasOpenDefers = false;
        if (s.hasOpenDefers && len(s.curfn.Exit) > 0) { 
        // Skip doing open defers if there is any extra exit code (likely
        // race detection), since we will not generate that code in the
        // case of the extra deferreturn/ret segment.
        s.hasOpenDefers = false;
    }
    if (s.hasOpenDefers) { 
        // Similarly, skip if there are any heap-allocated result
        // parameters that need to be copied back to their stack slots.
        foreach (var (_, f) in s.curfn.Type().Results().FieldSlice()) {
            if (!f.Nname._<ptr<ir.Name>>().OnStack()) {
                s.hasOpenDefers = false;
                break;
            }
        }
    }
    if (s.hasOpenDefers && s.curfn.NumReturns * s.curfn.NumDefers > 15) { 
        // Since we are generating defer calls at every exit for
        // open-coded defers, skip doing open-coded defers if there are
        // too many returns (especially if there are multiple defers).
        // Open-coded defers are most important for improving performance
        // for smaller functions (which don't have many returns).
        s.hasOpenDefers = false;
    }
    s.sp = s.entryNewValue0(ssa.OpSP, types.Types[types.TUINTPTR]); // TODO: use generic pointer type (unsafe.Pointer?) instead
    s.sb = s.entryNewValue0(ssa.OpSB, types.Types[types.TUINTPTR]);

    s.startBlock(s.f.Entry);
    s.vars[memVar] = s.startmem;
    if (s.hasOpenDefers) { 
        // Create the deferBits variable and stack slot.  deferBits is a
        // bitmask showing which of the open-coded defers in this function
        // have been activated.
        var deferBitsTemp = typecheck.TempAt(src.NoXPos, s.curfn, types.Types[types.TUINT8]);
        deferBitsTemp.SetAddrtaken(true);
        s.deferBitsTemp = deferBitsTemp; 
        // For this value, AuxInt is initialized to zero by default
        var startDeferBits = s.entryNewValue0(ssa.OpConst8, types.Types[types.TUINT8]);
        s.vars[deferBitsVar] = startDeferBits;
        s.deferBitsAddr = s.addr(deferBitsTemp);
        s.store(types.Types[types.TUINT8], s.deferBitsAddr, startDeferBits); 
        // Make sure that the deferBits stack slot is kept alive (for use
        // by panics) and stores to deferBits are not eliminated, even if
        // all checking code on deferBits in the function exit can be
        // eliminated, because the defer statements were all
        // unconditional.
        s.vars[memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, deferBitsTemp, s.mem(), false);
    }
    ptr<abi.ABIParamResultInfo> @params;
    params = s.f.ABISelf.ABIAnalyze(fn.Type(), true); 

    // Generate addresses of local declarations
    s.decladdrs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<ir.Name>, ptr<ssa.Value>>{};
    {
        var n__prev1 = n;

        foreach (var (_, __n) in fn.Dcl) {
            n = __n;

            if (n.Class == ir.PPARAM) 
                // Be aware that blank and unnamed input parameters will not appear here, but do appear in the type
                s.decladdrs[n] = s.entryNewValue2A(ssa.OpLocalAddr, types.NewPtr(n.Type()), n, s.sp, s.startmem);
            else if (n.Class == ir.PPARAMOUT) 
                s.decladdrs[n] = s.entryNewValue2A(ssa.OpLocalAddr, types.NewPtr(n.Type()), n, s.sp, s.startmem);
            else if (n.Class == ir.PAUTO)             else 
                s.Fatalf("local variable with class %v unimplemented", n.Class);
                    }
        n = n__prev1;
    }

    s.f.OwnAux = ssa.OwnAuxCall(fn.LSym, params); 

    // Populate SSAable arguments.
    {
        var n__prev1 = n;

        foreach (var (_, __n) in fn.Dcl) {
            n = __n;
            if (n.Class == ir.PPARAM) {
                if (s.canSSA(n)) {
                    var v = s.newValue0A(ssa.OpArg, n.Type(), n);
                    s.vars[n] = v;
                    s.addNamedValue(n, v); // This helps with debugging information, not needed for compilation itself.
                }
                else
 { // address was taken AND/OR too large for SSA
                    var paramAssignment = ssa.ParamAssignmentForArgName(s.f, n);
                    if (len(paramAssignment.Registers) > 0) {
                        if (TypeOK(_addr_n.Type())) { // SSA-able type, so address was taken -- receive value in OpArg, DO NOT bind to var, store immediately to memory.
                            v = s.newValue0A(ssa.OpArg, n.Type(), n);
                            s.store(n.Type(), s.decladdrs[n], v);
                        }
                        else
 { // Too big for SSA.
                            // Brute force, and early, do a bunch of stores from registers
                            // TODO fix the nasty storeArgOrLoad recursion in ssa/expand_calls.go so this Just Works with store of a big Arg.
                            s.storeParameterRegsToStack(s.f.ABISelf, paramAssignment, n, s.decladdrs[n], false);
                        }
                    }
                }
            }
        }
        n = n__prev1;
    }

    if (!fn.ClosureCalled()) {
        var clo = s.entryNewValue0(ssa.OpGetClosurePtr, s.f.Config.Types.BytePtr);
        var offset = int64(types.PtrSize); // PtrSize to skip past function entry PC field
        {
            var n__prev1 = n;

            foreach (var (_, __n) in fn.ClosureVars) {
                n = __n;
                var typ = n.Type();
                if (!n.Byval()) {
                    typ = types.NewPtr(typ);
                }
                offset = types.Rnd(offset, typ.Alignment());
                var ptr = s.newValue1I(ssa.OpOffPtr, types.NewPtr(typ), offset, clo);
                offset += typ.Size(); 

                // If n is a small variable captured by value, promote
                // it to PAUTO so it can be converted to SSA.
                //
                // Note: While we never capture a variable by value if
                // the user took its address, we may have generated
                // runtime calls that did (#43701). Since we don't
                // convert Addrtaken variables to SSA anyway, no point
                // in promoting them either.
                if (n.Byval() && !n.Addrtaken() && TypeOK(_addr_n.Type())) {
                    n.Class = ir.PAUTO;
                    fn.Dcl = append(fn.Dcl, n);
                    s.assign(n, s.load(n.Type(), ptr), false, 0);
                    continue;
                }
                if (!n.Byval()) {
                    ptr = s.load(typ, ptr);
                }
                s.setHeapaddr(fn.Pos(), n, ptr);
            }

            n = n__prev1;
        }
    }
    s.stmtList(fn.Enter);
    s.zeroResults();
    s.paramsToHeap();
    s.stmtList(fn.Body); 

    // fallthrough to exit
    if (s.curBlock != null) {
        s.pushLine(fn.Endlineno);
        s.exit();
        s.popLine();
    }
    foreach (var (_, b) in s.f.Blocks) {
        if (b.Pos != src.NoXPos) {
            s.updateUnsetPredPos(b);
        }
    }    s.f.HTMLWriter.WritePhase("before insert phis", "before insert phis");

    s.insertPhis(); 

    // Main call to ssa package to compile function
    ssa.Compile(s.f);

    if (s.hasOpenDefers) {
        s.emitOpenDeferInfo();
    }
    foreach (var (_, p) in @params.InParams()) {
        var (typs, offs) = p.RegisterTypesAndOffsets();
        foreach (var (i, t) in typs) {
            var o = offs[i]; // offset within parameter
            var fo = p.FrameOffset(params); // offset of parameter in frame
            var reg = ssa.ObjRegForAbiReg(p.Registers[i], s.f.Config);
            s.f.RegArgs = append(s.f.RegArgs, new ssa.Spill(Reg:reg,Offset:fo+o,Type:t));
        }
    }    return _addr_s.f!;
});

private static void storeParameterRegsToStack(this ptr<state> _addr_s, ptr<abi.ABIConfig> _addr_abi, ptr<abi.ABIParamAssignment> _addr_paramAssignment, ptr<ir.Name> _addr_n, ptr<ssa.Value> _addr_addr, bool pointersOnly) {
    ref state s = ref _addr_s.val;
    ref abi.ABIConfig abi = ref _addr_abi.val;
    ref abi.ABIParamAssignment paramAssignment = ref _addr_paramAssignment.val;
    ref ir.Name n = ref _addr_n.val;
    ref ssa.Value addr = ref _addr_addr.val;

    var (typs, offs) = paramAssignment.RegisterTypesAndOffsets();
    foreach (var (i, t) in typs) {
        if (pointersOnly && !t.IsPtrShaped()) {
            continue;
        }
        var r = paramAssignment.Registers[i];
        var o = offs[i];
        var (op, reg) = ssa.ArgOpAndRegisterFor(r, abi);
        ptr<ssa.AuxNameOffset> aux = addr(new ssa.AuxNameOffset(Name:n,Offset:o));
        var v = s.newValue0I(op, t, reg);
        v.Aux = aux;
        var p = s.newValue1I(ssa.OpOffPtr, types.NewPtr(t), o, addr);
        s.store(t, p, v);
    }
}

// zeroResults zeros the return values at the start of the function.
// We need to do this very early in the function.  Defer might stop a
// panic and show the return values as they exist at the time of
// panic.  For precise stacks, the garbage collector assumes results
// are always live, so we need to zero them before any allocations,
// even allocations to move params/results to the heap.
private static void zeroResults(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    foreach (var (_, f) in s.curfn.Type().Results().FieldSlice()) {
        ptr<ir.Name> n = f.Nname._<ptr<ir.Name>>();
        if (!n.OnStack()) { 
            // The local which points to the return value is the
            // thing that needs zeroing. This is already handled
            // by a Needzero annotation in plive.go:(*liveness).epilogue.
            continue;
        }
        {
            var typ = n.Type();

            if (TypeOK(_addr_typ)) {
                s.assign(n, s.zeroVal(typ), false, 0);
            }
            else
 {
                s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, n, s.mem());
                s.zero(n.Type(), s.decladdrs[n]);
            }

        }
    }
}

// paramsToHeap produces code to allocate memory for heap-escaped parameters
// and to copy non-result parameters' values from the stack.
private static void paramsToHeap(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    Action<ptr<types.Type>> @do = @params => {
        foreach (var (_, f) in @params.FieldSlice()) {
            if (f.Nname == null) {
                continue; // anonymous or blank parameter
            }
            ptr<ir.Name> n = f.Nname._<ptr<ir.Name>>();
            if (ir.IsBlank(n) || n.OnStack()) {
                continue;
            }
            s.newHeapaddr(n);
            if (n.Class == ir.PPARAM) {
                s.move(n.Type(), s.expr(n.Heapaddr), s.decladdrs[n]);
            }
        }
    };

    var typ = s.curfn.Type();
    do(typ.Recvs());
    do(typ.Params());
    do(typ.Results());
}

// newHeapaddr allocates heap memory for n and sets its heap address.
private static void newHeapaddr(this ptr<state> _addr_s, ptr<ir.Name> _addr_n) {
    ref state s = ref _addr_s.val;
    ref ir.Name n = ref _addr_n.val;

    s.setHeapaddr(n.Pos(), n, s.newObject(n.Type()));
}

// setHeapaddr allocates a new PAUTO variable to store ptr (which must be non-nil)
// and then sets it as n's heap address.
private static void setHeapaddr(this ptr<state> _addr_s, src.XPos pos, ptr<ir.Name> _addr_n, ptr<ssa.Value> _addr_ptr) {
    ref state s = ref _addr_s.val;
    ref ir.Name n = ref _addr_n.val;
    ref ssa.Value ptr = ref _addr_ptr.val;

    if (!ptr.Type.IsPtr() || !types.Identical(n.Type(), ptr.Type.Elem())) {
        @base.FatalfAt(n.Pos(), "setHeapaddr %L with type %v", n, ptr.Type);
    }
    var addr = ir.NewNameAt(pos, addr(new types.Sym(Name:"&"+n.Sym().Name,Pkg:types.LocalPkg)));
    addr.SetType(types.NewPtr(n.Type()));
    addr.Class = ir.PAUTO;
    addr.SetUsed(true);
    addr.Curfn = s.curfn;
    s.curfn.Dcl = append(s.curfn.Dcl, addr);
    types.CalcSize(addr.Type());

    if (n.Class == ir.PPARAMOUT) {
        addr.SetIsOutputParamHeapAddr(true);
    }
    n.Heapaddr = addr;
    s.assign(addr, ptr, false, 0);
}

// newObject returns an SSA value denoting new(typ).
private static ptr<ssa.Value> newObject(this ptr<state> _addr_s, ptr<types.Type> _addr_typ) {
    ref state s = ref _addr_s.val;
    ref types.Type typ = ref _addr_typ.val;

    if (typ.Size() == 0) {
        return _addr_s.newValue1A(ssa.OpAddr, types.NewPtr(typ), ir.Syms.Zerobase, s.sb)!;
    }
    return _addr_s.rtcall(ir.Syms.Newobject, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { types.NewPtr(typ) }), s.reflectType(typ))[0]!;
}

// reflectType returns an SSA value representing a pointer to typ's
// reflection type descriptor.
private static ptr<ssa.Value> reflectType(this ptr<state> _addr_s, ptr<types.Type> _addr_typ) {
    ref state s = ref _addr_s.val;
    ref types.Type typ = ref _addr_typ.val;

    var lsym = reflectdata.TypeLinksym(typ);
    return _addr_s.entryNewValue1A(ssa.OpAddr, types.NewPtr(types.Types[types.TUINT8]), lsym, s.sb)!;
}

private static void dumpSourcesColumn(ptr<ssa.HTMLWriter> _addr_writer, ptr<ir.Func> _addr_fn) {
    ref ssa.HTMLWriter writer = ref _addr_writer.val;
    ref ir.Func fn = ref _addr_fn.val;
 
    // Read sources of target function fn.
    var fname = @base.Ctxt.PosTable.Pos(fn.Pos()).Filename();
    var (targetFn, err) = readFuncLines(fname, fn.Pos().Line(), fn.Endlineno.Line());
    if (err != null) {
        writer.Logf("cannot read sources for function %v: %v", fn, err);
    }
    slice<ptr<ssa.FuncLines>> inlFns = default;
    foreach (var (_, fi) in ssaDumpInlined) {
        var elno = fi.Endlineno;
        fname = @base.Ctxt.PosTable.Pos(fi.Pos()).Filename();
        var (fnLines, err) = readFuncLines(fname, fi.Pos().Line(), elno.Line());
        if (err != null) {
            writer.Logf("cannot read sources for inlined function %v: %v", fi, err);
            continue;
        }
        inlFns = append(inlFns, fnLines);
    }    sort.Sort(ssa.ByTopo(inlFns));
    if (targetFn != null) {
        inlFns = append(new slice<ptr<ssa.FuncLines>>(new ptr<ssa.FuncLines>[] { targetFn }), inlFns);
    }
    writer.WriteSources("sources", inlFns);
}

private static (ptr<ssa.FuncLines>, error) readFuncLines(@string file, nuint start, nuint end) => func((defer, _, _) => {
    ptr<ssa.FuncLines> _p0 = default!;
    error _p0 = default!;

    var (f, err) = os.Open(os.ExpandEnv(file));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(f.Close());
    slice<@string> lines = default;
    var ln = uint(1);
    var scanner = bufio.NewScanner(f);
    while (scanner.Scan() && ln <= end) {
        if (ln >= start) {
            lines = append(lines, scanner.Text());
        }
        ln++;
    }
    return (addr(new ssa.FuncLines(Filename:file,StartLineno:start,Lines:lines)), error.As(null!)!);
});

// updateUnsetPredPos propagates the earliest-value position information for b
// towards all of b's predecessors that need a position, and recurs on that
// predecessor if its position is updated. B should have a non-empty position.
private static void updateUnsetPredPos(this ptr<state> _addr_s, ptr<ssa.Block> _addr_b) {
    ref state s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;

    if (b.Pos == src.NoXPos) {
        s.Fatalf("Block %s should have a position", b);
    }
    var bestPos = src.NoXPos;
    foreach (var (_, e) in b.Preds) {
        var p = e.Block();
        if (!p.LackingPos()) {
            continue;
        }
        if (bestPos == src.NoXPos) {
            bestPos = b.Pos;
            foreach (var (_, v) in b.Values) {
                if (v.LackingPos()) {
                    continue;
                }
                if (v.Pos != src.NoXPos) { 
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
private partial struct openDeferInfo {
    public ptr<ir.CallExpr> n; // If defer call is closure call, the address of the argtmp where the
// closure is stored.
    public ptr<ssa.Value> closure; // The node representing the argtmp where the closure is stored - used for
// function, method, or interface call, to store a closure that panic
// processing can use for this defer.
    public ptr<ir.Name> closureNode; // If defer call is interface call, the address of the argtmp where the
// receiver is stored
    public ptr<ssa.Value> rcvr; // The node representing the argtmp where the receiver is stored
    public ptr<ir.Name> rcvrNode; // The addresses of the argtmps where the evaluated arguments of the defer
// function call are stored.
    public slice<ptr<ssa.Value>> argVals; // The nodes representing the argtmps where the args of the defer are stored
    public slice<ptr<ir.Name>> argNodes;
}

private partial struct state {
    public ptr<ssa.Config> config; // function we're building
    public ptr<ssa.Func> f; // Node for function
    public ptr<ir.Func> curfn; // labels in f
    public map<@string, ptr<ssaLabel>> labels; // unlabeled break and continue statement tracking
    public ptr<ssa.Block> breakTo; // current target for plain break statement
    public ptr<ssa.Block> continueTo; // current target for plain continue statement

// current location where we're interpreting the AST
    public ptr<ssa.Block> curBlock; // variable assignments in the current block (map from variable symbol to ssa value)
// *Node is the unique identifier (an ONAME Node) for the variable.
// TODO: keep a single varnum map, then make all of these maps slices instead?
    public map<ir.Node, ptr<ssa.Value>> vars; // fwdVars are variables that are used before they are defined in the current block.
// This map exists just to coalesce multiple references into a single FwdRef op.
// *Node is the unique identifier (an ONAME Node) for the variable.
    public map<ir.Node, ptr<ssa.Value>> fwdVars; // all defined variables at the end of each block. Indexed by block ID.
    public slice<map<ir.Node, ptr<ssa.Value>>> defvars; // addresses of PPARAM and PPARAMOUT variables on the stack.
    public map<ptr<ir.Name>, ptr<ssa.Value>> decladdrs; // starting values. Memory, stack pointer, and globals pointer
    public ptr<ssa.Value> startmem;
    public ptr<ssa.Value> sp;
    public ptr<ssa.Value> sb; // value representing address of where deferBits autotmp is stored
    public ptr<ssa.Value> deferBitsAddr;
    public ptr<ir.Name> deferBitsTemp; // line number stack. The current line number is top of stack
    public slice<src.XPos> line; // the last line number processed; it may have been popped
    public src.XPos lastPos; // list of panic calls by function name and line number.
// Used to deduplicate panic calls.
    public map<funcLine, ptr<ssa.Block>> panics;
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
    public nint lastDeferCount; // Number of defers encountered at that point

    public ptr<ssa.Value> prevCall; // the previous call; use this to tie results to the call op.
}

private partial struct funcLine {
    public ptr<obj.LSym> f;
    public ptr<src.PosBase> @base;
    public nuint line;
}

private partial struct ssaLabel {
    public ptr<ssa.Block> target; // block identified by this label
    public ptr<ssa.Block> breakTarget; // block to break to in control flow node identified by this label
    public ptr<ssa.Block> continueTarget; // block to continue to in control flow node identified by this label
}

// label returns the label associated with sym, creating it if necessary.
private static ptr<ssaLabel> label(this ptr<state> _addr_s, ptr<types.Sym> _addr_sym) {
    ref state s = ref _addr_s.val;
    ref types.Sym sym = ref _addr_sym.val;

    var lab = s.labels[sym.Name];
    if (lab == null) {
        lab = @new<ssaLabel>();
        s.labels[sym.Name] = lab;
    }
    return _addr_lab!;
}

private static void Logf(this ptr<state> _addr_s, @string msg, params object[] args) {
    args = args.Clone();
    ref state s = ref _addr_s.val;

    s.f.Logf(msg, args);
}
private static bool Log(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    return s.f.Log();
}
private static void Fatalf(this ptr<state> _addr_s, @string msg, params object[] args) {
    args = args.Clone();
    ref state s = ref _addr_s.val;

    s.f.Frontend().Fatalf(s.peekPos(), msg, args);
}
private static void Warnl(this ptr<state> _addr_s, src.XPos pos, @string msg, params object[] args) {
    args = args.Clone();
    ref state s = ref _addr_s.val;

    s.f.Warnl(pos, msg, args);
}
private static bool Debug_checknil(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    return s.f.Frontend().Debug_checknil();
}

private static ptr<ir.Name> ssaMarker(@string name) {
    return _addr_typecheck.NewName(addr(new types.Sym(Name:name)))!;
}

 
// marker node for the memory variable
private static var memVar = ssaMarker("mem");private static var ptrVar = ssaMarker("ptr");private static var lenVar = ssaMarker("len");private static var newlenVar = ssaMarker("newlen");private static var capVar = ssaMarker("cap");private static var typVar = ssaMarker("typ");private static var okVar = ssaMarker("ok");private static var deferBitsVar = ssaMarker("deferBits");

// startBlock sets the current block we're generating code in to b.
private static void startBlock(this ptr<state> _addr_s, ptr<ssa.Block> _addr_b) {
    ref state s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;

    if (s.curBlock != null) {
        s.Fatalf("starting block %v when block %v has not ended", b, s.curBlock);
    }
    s.curBlock = b;
    s.vars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ir.Node, ptr<ssa.Value>>{};
    foreach (var (n) in s.fwdVars) {
        delete(s.fwdVars, n);
    }
}

// endBlock marks the end of generating code for the current block.
// Returns the (former) current block. Returns nil if there is no current
// block, i.e. if no code flows to the current execution point.
private static ptr<ssa.Block> endBlock(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    var b = s.curBlock;
    if (b == null) {
        return _addr_null!;
    }
    while (len(s.defvars) <= int(b.ID)) {
        s.defvars = append(s.defvars, null);
    }
    s.defvars[b.ID] = s.vars;
    s.curBlock = null;
    s.vars = null;
    if (b.LackingPos()) { 
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
private static void pushLine(this ptr<state> _addr_s, src.XPos line) {
    ref state s = ref _addr_s.val;

    if (!line.IsKnown()) { 
        // the frontend may emit node with line number missing,
        // use the parent line number in this case.
        line = s.peekPos();
        if (@base.Flag.K != 0) {
            @base.Warn("buildssa: unknown position (line 0)");
        }
    }
    else
 {
        s.lastPos = line;
    }
    s.line = append(s.line, line);
}

// popLine pops the top of the line number stack.
private static void popLine(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    s.line = s.line[..(int)len(s.line) - 1];
}

// peekPos peeks the top of the line number stack.
private static src.XPos peekPos(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    return s.line[len(s.line) - 1];
}

// newValue0 adds a new value with no arguments to the current block.
private static ptr<ssa.Value> newValue0(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.curBlock.NewValue0(s.peekPos(), op, t)!;
}

// newValue0A adds a new value with no arguments and an aux value to the current block.
private static ptr<ssa.Value> newValue0A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.curBlock.NewValue0A(s.peekPos(), op, t, aux)!;
}

// newValue0I adds a new value with no arguments and an auxint value to the current block.
private static ptr<ssa.Value> newValue0I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long auxint) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.curBlock.NewValue0I(s.peekPos(), op, t, auxint)!;
}

// newValue1 adds a new value with one argument to the current block.
private static ptr<ssa.Value> newValue1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.curBlock.NewValue1(s.peekPos(), op, t, arg)!;
}

// newValue1A adds a new value with one argument and an aux value to the current block.
private static ptr<ssa.Value> newValue1A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.curBlock.NewValue1A(s.peekPos(), op, t, aux, arg)!;
}

// newValue1Apos adds a new value with one argument and an aux value to the current block.
// isStmt determines whether the created values may be a statement or not
// (i.e., false means never, yes means maybe).
private static ptr<ssa.Value> newValue1Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg, bool isStmt) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    if (isStmt) {
        return _addr_s.curBlock.NewValue1A(s.peekPos(), op, t, aux, arg)!;
    }
    return _addr_s.curBlock.NewValue1A(s.peekPos().WithNotStmt(), op, t, aux, arg)!;
}

// newValue1I adds a new value with one argument and an auxint value to the current block.
private static ptr<ssa.Value> newValue1I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.curBlock.NewValue1I(s.peekPos(), op, t, aux, arg)!;
}

// newValue2 adds a new value with two arguments to the current block.
private static ptr<ssa.Value> newValue2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    return _addr_s.curBlock.NewValue2(s.peekPos(), op, t, arg0, arg1)!;
}

// newValue2A adds a new value with two arguments and an aux value to the current block.
private static ptr<ssa.Value> newValue2A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    return _addr_s.curBlock.NewValue2A(s.peekPos(), op, t, aux, arg0, arg1)!;
}

// newValue2Apos adds a new value with two arguments and an aux value to the current block.
// isStmt determines whether the created values may be a statement or not
// (i.e., false means never, yes means maybe).
private static ptr<ssa.Value> newValue2Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, bool isStmt) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    if (isStmt) {
        return _addr_s.curBlock.NewValue2A(s.peekPos(), op, t, aux, arg0, arg1)!;
    }
    return _addr_s.curBlock.NewValue2A(s.peekPos().WithNotStmt(), op, t, aux, arg0, arg1)!;
}

// newValue2I adds a new value with two arguments and an auxint value to the current block.
private static ptr<ssa.Value> newValue2I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    return _addr_s.curBlock.NewValue2I(s.peekPos(), op, t, aux, arg0, arg1)!;
}

// newValue3 adds a new value with three arguments to the current block.
private static ptr<ssa.Value> newValue3(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;
    ref ssa.Value arg2 = ref _addr_arg2.val;

    return _addr_s.curBlock.NewValue3(s.peekPos(), op, t, arg0, arg1, arg2)!;
}

// newValue3I adds a new value with three arguments and an auxint value to the current block.
private static ptr<ssa.Value> newValue3I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;
    ref ssa.Value arg2 = ref _addr_arg2.val;

    return _addr_s.curBlock.NewValue3I(s.peekPos(), op, t, aux, arg0, arg1, arg2)!;
}

// newValue3A adds a new value with three arguments and an aux value to the current block.
private static ptr<ssa.Value> newValue3A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2) {
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
private static ptr<ssa.Value> newValue3Apos(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, bool isStmt) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;
    ref ssa.Value arg2 = ref _addr_arg2.val;

    if (isStmt) {
        return _addr_s.curBlock.NewValue3A(s.peekPos(), op, t, aux, arg0, arg1, arg2)!;
    }
    return _addr_s.curBlock.NewValue3A(s.peekPos().WithNotStmt(), op, t, aux, arg0, arg1, arg2)!;
}

// newValue4 adds a new value with four arguments to the current block.
private static ptr<ssa.Value> newValue4(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, ptr<ssa.Value> _addr_arg3) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;
    ref ssa.Value arg2 = ref _addr_arg2.val;
    ref ssa.Value arg3 = ref _addr_arg3.val;

    return _addr_s.curBlock.NewValue4(s.peekPos(), op, t, arg0, arg1, arg2, arg3)!;
}

// newValue4 adds a new value with four arguments and an auxint value to the current block.
private static ptr<ssa.Value> newValue4I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1, ptr<ssa.Value> _addr_arg2, ptr<ssa.Value> _addr_arg3) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;
    ref ssa.Value arg2 = ref _addr_arg2.val;
    ref ssa.Value arg3 = ref _addr_arg3.val;

    return _addr_s.curBlock.NewValue4I(s.peekPos(), op, t, aux, arg0, arg1, arg2, arg3)!;
}

private static ptr<ssa.Block> entryBlock(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    var b = s.f.Entry;
    if (@base.Flag.N > 0 && s.curBlock != null) { 
        // If optimizations are off, allocate in current block instead. Since with -N
        // we're not doing the CSE or tighten passes, putting lots of stuff in the
        // entry block leads to O(n^2) entries in the live value map during regalloc.
        // See issue 45897.
        b = s.curBlock;
    }
    return _addr_b!;
}

// entryNewValue0 adds a new value with no arguments to the entry block.
private static ptr<ssa.Value> entryNewValue0(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.entryBlock().NewValue0(src.NoXPos, op, t)!;
}

// entryNewValue0A adds a new value with no arguments and an aux value to the entry block.
private static ptr<ssa.Value> entryNewValue0A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.entryBlock().NewValue0A(src.NoXPos, op, t, aux)!;
}

// entryNewValue1 adds a new value with one argument to the entry block.
private static ptr<ssa.Value> entryNewValue1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.entryBlock().NewValue1(src.NoXPos, op, t, arg)!;
}

// entryNewValue1 adds a new value with one argument and an auxint value to the entry block.
private static ptr<ssa.Value> entryNewValue1I(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, long auxint, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.entryBlock().NewValue1I(src.NoXPos, op, t, auxint, arg)!;
}

// entryNewValue1A adds a new value with one argument and an aux value to the entry block.
private static ptr<ssa.Value> entryNewValue1A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    return _addr_s.entryBlock().NewValue1A(src.NoXPos, op, t, aux, arg)!;
}

// entryNewValue2 adds a new value with two arguments to the entry block.
private static ptr<ssa.Value> entryNewValue2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    return _addr_s.entryBlock().NewValue2(src.NoXPos, op, t, arg0, arg1)!;
}

// entryNewValue2A adds a new value with two arguments and an aux value to the entry block.
private static ptr<ssa.Value> entryNewValue2A(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ssa.Aux aux, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    return _addr_s.entryBlock().NewValue2A(src.NoXPos, op, t, aux, arg0, arg1)!;
}

// const* routines add a new const value to the entry block.
private static ptr<ssa.Value> constSlice(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstSlice(t)!;
}
private static ptr<ssa.Value> constInterface(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstInterface(t)!;
}
private static ptr<ssa.Value> constNil(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstNil(t)!;
}
private static ptr<ssa.Value> constEmptyString(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstEmptyString(t)!;
}
private static ptr<ssa.Value> constBool(this ptr<state> _addr_s, bool c) {
    ref state s = ref _addr_s.val;

    return _addr_s.f.ConstBool(types.Types[types.TBOOL], c)!;
}
private static ptr<ssa.Value> constInt8(this ptr<state> _addr_s, ptr<types.Type> _addr_t, sbyte c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstInt8(t, c)!;
}
private static ptr<ssa.Value> constInt16(this ptr<state> _addr_s, ptr<types.Type> _addr_t, short c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstInt16(t, c)!;
}
private static ptr<ssa.Value> constInt32(this ptr<state> _addr_s, ptr<types.Type> _addr_t, int c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstInt32(t, c)!;
}
private static ptr<ssa.Value> constInt64(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstInt64(t, c)!;
}
private static ptr<ssa.Value> constFloat32(this ptr<state> _addr_s, ptr<types.Type> _addr_t, double c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstFloat32(t, c)!;
}
private static ptr<ssa.Value> constFloat64(this ptr<state> _addr_s, ptr<types.Type> _addr_t, double c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstFloat64(t, c)!;
}
private static ptr<ssa.Value> constInt(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    if (s.config.PtrSize == 8) {
        return _addr_s.constInt64(t, c)!;
    }
    if (int64(int32(c)) != c) {
        s.Fatalf("integer constant too big %d", c);
    }
    return _addr_s.constInt32(t, int32(c))!;
}
private static ptr<ssa.Value> constOffPtrSP(this ptr<state> _addr_s, ptr<types.Type> _addr_t, long c) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_s.f.ConstOffPtrSP(t, c, s.sp)!;
}

// newValueOrSfCall* are wrappers around newValue*, which may create a call to a
// soft-float runtime function instead (when emitting soft-float code).
private static ptr<ssa.Value> newValueOrSfCall1(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg = ref _addr_arg.val;

    if (s.softFloat) {
        {
            var (c, ok) = s.sfcall(op, arg);

            if (ok) {
                return _addr_c!;
            }

        }
    }
    return _addr_s.newValue1(op, t, arg)!;
}
private static ptr<ssa.Value> newValueOrSfCall2(this ptr<state> _addr_s, ssa.Op op, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_arg0, ptr<ssa.Value> _addr_arg1) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value arg0 = ref _addr_arg0.val;
    ref ssa.Value arg1 = ref _addr_arg1.val;

    if (s.softFloat) {
        {
            var (c, ok) = s.sfcall(op, arg0, arg1);

            if (ok) {
                return _addr_c!;
            }

        }
    }
    return _addr_s.newValue2(op, t, arg0, arg1)!;
}

private partial struct instrumentKind { // : byte
}

private static readonly var instrumentRead = iota;
private static readonly var instrumentWrite = 0;
private static readonly var instrumentMove = 1;

private static void instrument(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_addr, instrumentKind kind) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value addr = ref _addr_addr.val;

    s.instrument2(t, addr, null, kind);
}

// instrumentFields instruments a read/write operation on addr.
// If it is instrumenting for MSAN and t is a struct type, it instruments
// operation for each field, instead of for the whole struct.
private static void instrumentFields(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_addr, instrumentKind kind) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value addr = ref _addr_addr.val;

    if (!@base.Flag.MSan || !t.IsStruct()) {
        s.instrument(t, addr, kind);
        return ;
    }
    foreach (var (_, f) in t.Fields().Slice()) {
        if (f.Sym.IsBlank()) {
            continue;
        }
        var offptr = s.newValue1I(ssa.OpOffPtr, types.NewPtr(f.Type), f.Offset, addr);
        s.instrumentFields(f.Type, offptr, kind);
    }
}

private static void instrumentMove(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst, ptr<ssa.Value> _addr_src) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value dst = ref _addr_dst.val;
    ref ssa.Value src = ref _addr_src.val;

    if (@base.Flag.MSan) {
        s.instrument2(t, dst, src, instrumentMove);
    }
    else
 {
        s.instrument(t, src, instrumentRead);
        s.instrument(t, dst, instrumentWrite);
    }
}

private static void instrument2(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_addr, ptr<ssa.Value> _addr_addr2, instrumentKind kind) => func((_, panic, _) => {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value addr = ref _addr_addr.val;
    ref ssa.Value addr2 = ref _addr_addr2.val;

    if (!s.curfn.InstrumentBody()) {
        return ;
    }
    var w = t.Size();
    if (w == 0) {
        return ; // can't race on zero-sized things
    }
    if (ssa.IsSanitizerSafeAddr(addr)) {
        return ;
    }
    ptr<obj.LSym> fn;
    var needWidth = false;

    if (addr2 != null && kind != instrumentMove) {
        panic("instrument2: non-nil addr2 for non-move instrumentation");
    }
    if (@base.Flag.MSan) {

        if (kind == instrumentRead) 
            fn = ir.Syms.Msanread;
        else if (kind == instrumentWrite) 
            fn = ir.Syms.Msanwrite;
        else if (kind == instrumentMove) 
            fn = ir.Syms.Msanmove;
        else 
            panic("unreachable");
                needWidth = true;
    }
    else if (@base.Flag.Race && t.NumComponents(types.CountBlankFields) > 1) { 
        // for composite objects we have to write every address
        // because a write might happen to any subobject.
        // composites with only one element don't have subobjects, though.

        if (kind == instrumentRead) 
            fn = ir.Syms.Racereadrange;
        else if (kind == instrumentWrite) 
            fn = ir.Syms.Racewriterange;
        else 
            panic("unreachable");
                needWidth = true;
    }
    else if (@base.Flag.Race) { 
        // for non-composite objects we can write just the start
        // address, as any write must write the first byte.

        if (kind == instrumentRead) 
            fn = ir.Syms.Raceread;
        else if (kind == instrumentWrite) 
            fn = ir.Syms.Racewrite;
        else 
            panic("unreachable");
            }
    else
 {
        panic("unreachable");
    }
    ptr<ssa.Value> args = new slice<ptr<ssa.Value>>(new ptr<ssa.Value>[] { addr });
    if (addr2 != null) {
        args = append(args, addr2);
    }
    if (needWidth) {
        args = append(args, s.constInt(types.Types[types.TUINTPTR], w));
    }
    s.rtcall(fn, true, null, args);
});

private static ptr<ssa.Value> load(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_src) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value src = ref _addr_src.val;

    s.instrumentFields(t, src, instrumentRead);
    return _addr_s.rawLoad(t, src)!;
}

private static ptr<ssa.Value> rawLoad(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_src) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value src = ref _addr_src.val;

    return _addr_s.newValue2(ssa.OpLoad, t, src, s.mem())!;
}

private static void store(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst, ptr<ssa.Value> _addr_val) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value dst = ref _addr_dst.val;
    ref ssa.Value val = ref _addr_val.val;

    s.vars[memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, dst, val, s.mem());
}

private static void zero(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value dst = ref _addr_dst.val;

    s.instrument(t, dst, instrumentWrite);
    var store = s.newValue2I(ssa.OpZero, types.TypeMem, t.Size(), dst, s.mem());
    store.Aux = t;
    s.vars[memVar] = store;
}

private static void move(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_dst, ptr<ssa.Value> _addr_src) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value dst = ref _addr_dst.val;
    ref ssa.Value src = ref _addr_src.val;

    s.instrumentMove(t, dst, src);
    var store = s.newValue3I(ssa.OpMove, types.TypeMem, t.Size(), dst, src, s.mem());
    store.Aux = t;
    s.vars[memVar] = store;
}

// stmtList converts the statement list n to SSA and adds it to s.
private static void stmtList(this ptr<state> _addr_s, ir.Nodes l) {
    ref state s = ref _addr_s.val;

    foreach (var (_, n) in l) {
        s.stmt(n);
    }
}

// stmt converts the statement n to SSA and adds it to s.
private static void stmt(this ptr<state> _addr_s, ir.Node n) => func((defer, _, _) => {
    ref state s = ref _addr_s.val;

    if (!(n.Op() == ir.OVARKILL || n.Op() == ir.OVARLIVE || n.Op() == ir.OVARDEF)) { 
        // OVARKILL, OVARLIVE, and OVARDEF are invisible to the programmer, so we don't use their line numbers to avoid confusion in debugging.
        s.pushLine(n.Pos());
        defer(s.popLine());
    }
    if (s.curBlock == null && n.Op() != ir.OLABEL) {
        return ;
    }
    s.stmtList(n.Init());


    if (n.Op() == ir.OBLOCK)
    {
        ptr<ir.BlockStmt> n = n._<ptr<ir.BlockStmt>>();
        s.stmtList(n.List); 

        // No-ops
        goto __switch_break0;
    }
    if (n.Op() == ir.ODCLCONST || n.Op() == ir.ODCLTYPE || n.Op() == ir.OFALL)
    {
        goto __switch_break0;
    }
    if (n.Op() == ir.OCALLFUNC)
    {
        n = n._<ptr<ir.CallExpr>>();
        if (ir.IsIntrinsicCall(n)) {
            s.intrinsicCall(n);
            return ;
        }
        fallthrough = true;

    }
    if (fallthrough || n.Op() == ir.OCALLINTER)
    {
        n = n._<ptr<ir.CallExpr>>();
        s.callResult(n, callNormal);
        if (n.Op() == ir.OCALLFUNC && n.X.Op() == ir.ONAME && n.X._<ptr<ir.Name>>().Class == ir.PFUNC) {
            {
                var fn = n.X.Sym().Name;

                if (@base.Flag.CompilingRuntime && fn == "throw" || n.X.Sym().Pkg == ir.Pkgs.Runtime && (fn == "throwinit" || fn == "gopanic" || fn == "panicwrap" || fn == "block" || fn == "panicmakeslicelen" || fn == "panicmakeslicecap")) {
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
    if (n.Op() == ir.ODEFER)
    {
        n = n._<ptr<ir.GoDeferStmt>>();
        if (@base.Debug.Defer > 0) {
            @string defertype = default;
            if (s.hasOpenDefers) {
                defertype = "open-coded";
            }
            else if (n.Esc() == ir.EscNever) {
                defertype = "stack-allocated";
            }
            else
 {
                defertype = "heap-allocated";
            }
            @base.WarnfAt(n.Pos(), "%s defer", defertype);
        }
        if (s.hasOpenDefers) {
            s.openDeferRecord(n.Call._<ptr<ir.CallExpr>>());
        }
        else
 {
            var d = callDefer;
            if (n.Esc() == ir.EscNever) {
                d = callDeferStack;
            }
            s.callResult(n.Call._<ptr<ir.CallExpr>>(), d);
        }
        goto __switch_break0;
    }
    if (n.Op() == ir.OGO)
    {
        n = n._<ptr<ir.GoDeferStmt>>();
        s.callResult(n.Call._<ptr<ir.CallExpr>>(), callGo);
        goto __switch_break0;
    }
    if (n.Op() == ir.OAS2DOTTYPE)
    {
        n = n._<ptr<ir.AssignListStmt>>();
        var (res, resok) = s.dottype(n.Rhs[0]._<ptr<ir.TypeAssertExpr>>(), true);
        var deref = false;
        if (!TypeOK(_addr_n.Rhs[0].Type())) {
            if (res.Op != ssa.OpLoad) {
                s.Fatalf("dottype of non-load");
            }
            var mem = s.mem();
            if (mem.Op == ssa.OpVarKill) {
                mem = mem.Args[0];
            }
            if (res.Args[1] != mem) {
                s.Fatalf("memory no longer live from 2-result dottype load");
            }
            deref = true;
            res = res.Args[0];
        }
        s.assign(n.Lhs[0], res, deref, 0);
        s.assign(n.Lhs[1], resok, false, 0);
        return ;
        goto __switch_break0;
    }
    if (n.Op() == ir.OAS2FUNC) 
    {
        // We come here only when it is an intrinsic call returning two values.
        n = n._<ptr<ir.AssignListStmt>>();
        ptr<ir.CallExpr> call = n.Rhs[0]._<ptr<ir.CallExpr>>();
        if (!ir.IsIntrinsicCall(call)) {
            s.Fatalf("non-intrinsic AS2FUNC not expanded %v", call);
        }
        var v = s.intrinsicCall(call);
        var v1 = s.newValue1(ssa.OpSelect0, n.Lhs[0].Type(), v);
        var v2 = s.newValue1(ssa.OpSelect1, n.Lhs[1].Type(), v);
        s.assign(n.Lhs[0], v1, false, 0);
        s.assign(n.Lhs[1], v2, false, 0);
        return ;
        goto __switch_break0;
    }
    if (n.Op() == ir.ODCL)
    {
        n = n._<ptr<ir.Decl>>();
        {
            var v__prev1 = v;

            v = n.X;

            if (v.Esc() == ir.EscHeap) {
                s.newHeapaddr(v);
            }

            v = v__prev1;

        }
        goto __switch_break0;
    }
    if (n.Op() == ir.OLABEL)
    {
        n = n._<ptr<ir.LabelStmt>>();
        var sym = n.Label;
        var lab = s.label(sym); 

        // The label might already have a target block via a goto.
        if (lab.target == null) {
            lab.target = s.f.NewBlock(ssa.BlockPlain);
        }
        if (s.curBlock != null) {
            b = s.endBlock();
            b.AddEdgeTo(lab.target);
        }
        s.startBlock(lab.target);
        goto __switch_break0;
    }
    if (n.Op() == ir.OGOTO)
    {
        n = n._<ptr<ir.BranchStmt>>();
        sym = n.Label;

        lab = s.label(sym);
        if (lab.target == null) {
            lab.target = s.f.NewBlock(ssa.BlockPlain);
        }
        b = s.endBlock();
        b.Pos = s.lastPos.WithIsStmt(); // Do this even if b is an empty block.
        b.AddEdgeTo(lab.target);
        goto __switch_break0;
    }
    if (n.Op() == ir.OAS)
    {
        n = n._<ptr<ir.AssignStmt>>();
        if (n.X == n.Y && n.X.Op() == ir.ONAME) { 
            // An x=x assignment. No point in doing anything
            // here. In addition, skipping this assignment
            // prevents generating:
            //   VARDEF x
            //   COPY x -> x
            // which is bad because x is incorrectly considered
            // dead before the vardef. See issue #14904.
            return ;
        }
        var rhs = n.Y;
        if (rhs != null) {

            if (rhs.Op() == ir.OSTRUCTLIT || rhs.Op() == ir.OARRAYLIT || rhs.Op() == ir.OSLICELIT) 
                // All literals with nonzero fields have already been
                // rewritten during walk. Any that remain are just T{}
                // or equivalents. Use the zero value.
                if (!ir.IsZero(rhs)) {
                    s.Fatalf("literal with nonzero value in SSA: %v", rhs);
                }
                rhs = null;
            else if (rhs.Op() == ir.OAPPEND) 
                rhs = rhs._<ptr<ir.CallExpr>>(); 
                // Check whether we're writing the result of an append back to the same slice.
                // If so, we handle it specially to avoid write barriers on the fast
                // (non-growth) path.
                if (!ir.SameSafeExpr(n.X, rhs.Args[0]) || @base.Flag.N != 0) {
                    break;
                } 
                // If the slice can be SSA'd, it'll be on the stack,
                // so there will be no write barriers,
                // so there's no need to attempt to prevent them.
                if (s.canSSA(n.X)) {
                    if (@base.Debug.Append > 0) { // replicating old diagnostic message
                        @base.WarnfAt(n.Pos(), "append: len-only update (in local slice)");
                    }
                    break;
                }
                if (@base.Debug.Append > 0) {
                    @base.WarnfAt(n.Pos(), "append: len-only update");
                }
                s.append(rhs, true);
                return ;
                    }
        if (ir.IsBlank(n.X)) { 
            // _ = rhs
            // Just evaluate rhs for side-effects.
            if (rhs != null) {
                s.expr(rhs);
            }
            return ;
        }
        ptr<types.Type> t;
        if (n.Y != null) {
            t = n.Y.Type();
        }
        else
 {
            t = n.X.Type();
        }
        ptr<ssa.Value> r;
        deref = !TypeOK(t);
        if (deref) {
            if (rhs == null) {
                r = null; // Signal assign to use OpZero.
            }
            else
 {
                r = s.addr(rhs);
            }
        }
        else
 {
            if (rhs == null) {
                r = s.zeroVal(t);
            }
            else
 {
                r = s.expr(rhs);
            }
        }
        skipMask skip = default;
        if (rhs != null && (rhs.Op() == ir.OSLICE || rhs.Op() == ir.OSLICE3 || rhs.Op() == ir.OSLICESTR) && ir.SameSafeExpr(rhs._<ptr<ir.SliceExpr>>().X, n.X)) { 
            // We're assigning a slicing operation back to its source.
            // Don't write back fields we aren't changing. See issue #14855.
            rhs = rhs._<ptr<ir.SliceExpr>>();
            var i = rhs.Low;
            var j = rhs.High;
            var k = rhs.Max;
            if (i != null && (i.Op() == ir.OLITERAL && i.Val().Kind() == constant.Int && ir.Int64Val(i) == 0)) { 
                // [0:...] is the same as [:...]
                i = null;
            } 
            // TODO: detect defaults for len/cap also.
            // Currently doesn't really work because (*p)[:len(*p)] appears here as:
            //    tmp = len(*p)
            //    (*p)[:tmp]
            //if j != nil && (j.Op == OLEN && SameSafeExpr(j.Left, n.Left)) {
            //      j = nil
            //}
            //if k != nil && (k.Op == OCAP && SameSafeExpr(k.Left, n.Left)) {
            //      k = nil
            //}
            if (i == null) {
                skip |= skipPtr;
                if (j == null) {
                    skip |= skipLen;
                }
                if (k == null) {
                    skip |= skipCap;
                }
            }
        }
        s.assign(n.X, r, deref, skip);
        goto __switch_break0;
    }
    if (n.Op() == ir.OIF)
    {
        n = n._<ptr<ir.IfStmt>>();
        if (ir.IsConst(n.Cond, constant.Bool)) {
            s.stmtList(n.Cond.Init());
            if (ir.BoolVal(n.Cond)) {
                s.stmtList(n.Body);
            }
            else
 {
                s.stmtList(n.Else);
            }
            break;
        }
        var bEnd = s.f.NewBlock(ssa.BlockPlain);
        sbyte likely = default;
        if (n.Likely) {
            likely = 1;
        }
        ptr<ssa.Block> bThen;
        if (len(n.Body) != 0) {
            bThen = s.f.NewBlock(ssa.BlockPlain);
        }
        else
 {
            bThen = bEnd;
        }
        ptr<ssa.Block> bElse;
        if (len(n.Else) != 0) {
            bElse = s.f.NewBlock(ssa.BlockPlain);
        }
        else
 {
            bElse = bEnd;
        }
        s.condBranch(n.Cond, bThen, bElse, likely);

        if (len(n.Body) != 0) {
            s.startBlock(bThen);
            s.stmtList(n.Body);
            {
                var b__prev2 = b;

                b = s.endBlock();

                if (b != null) {
                    b.AddEdgeTo(bEnd);
                }

                b = b__prev2;

            }
        }
        if (len(n.Else) != 0) {
            s.startBlock(bElse);
            s.stmtList(n.Else);
            {
                var b__prev2 = b;

                b = s.endBlock();

                if (b != null) {
                    b.AddEdgeTo(bEnd);
                }

                b = b__prev2;

            }
        }
        s.startBlock(bEnd);
        goto __switch_break0;
    }
    if (n.Op() == ir.ORETURN)
    {
        n = n._<ptr<ir.ReturnStmt>>();
        s.stmtList(n.Results);
        b = s.exit();
        b.Pos = s.lastPos.WithIsStmt();
        goto __switch_break0;
    }
    if (n.Op() == ir.OTAILCALL)
    {
        n = n._<ptr<ir.TailCallStmt>>();
        b = s.exit();
        b.Kind = ssa.BlockRetJmp; // override BlockRet
        b.Aux = callTargetLSym(_addr_n.Target);
        goto __switch_break0;
    }
    if (n.Op() == ir.OCONTINUE || n.Op() == ir.OBREAK)
    {
        n = n._<ptr<ir.BranchStmt>>();
        ptr<ssa.Block> to;
        if (n.Label == null) { 
            // plain break/continue

            if (n.Op() == ir.OCONTINUE) 
                to = s.continueTo;
            else if (n.Op() == ir.OBREAK) 
                to = s.breakTo;
                    }
        else
 { 
            // labeled break/continue; look up the target
            sym = n.Label;
            lab = s.label(sym);

            if (n.Op() == ir.OCONTINUE) 
                to = lab.continueTarget;
            else if (n.Op() == ir.OBREAK) 
                to = lab.breakTarget;
                    }
        b = s.endBlock();
        b.Pos = s.lastPos.WithIsStmt(); // Do this even if b is an empty block.
        b.AddEdgeTo(to);
        goto __switch_break0;
    }
    if (n.Op() == ir.OFOR || n.Op() == ir.OFORUNTIL) 
    {
        // OFOR: for Ninit; Left; Right { Nbody }
        // cond (Left); body (Nbody); incr (Right)
        //
        // OFORUNTIL: for Ninit; Left; Right; List { Nbody }
        // => body: { Nbody }; incr: Right; if Left { lateincr: List; goto body }; end:
        n = n._<ptr<ir.ForStmt>>();
        var bCond = s.f.NewBlock(ssa.BlockPlain);
        var bBody = s.f.NewBlock(ssa.BlockPlain);
        var bIncr = s.f.NewBlock(ssa.BlockPlain);
        bEnd = s.f.NewBlock(ssa.BlockPlain); 

        // ensure empty for loops have correct position; issue #30167
        bBody.Pos = n.Pos(); 

        // first, jump to condition test (OFOR) or body (OFORUNTIL)
        b = s.endBlock();
        if (n.Op() == ir.OFOR) {
            b.AddEdgeTo(bCond); 
            // generate code to test condition
            s.startBlock(bCond);
            if (n.Cond != null) {
                s.condBranch(n.Cond, bBody, bEnd, 1);
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
        var prevContinue = s.continueTo;
        var prevBreak = s.breakTo;
        s.continueTo = bIncr;
        s.breakTo = bEnd;
        lab = ;
        {
            var sym__prev1 = sym;

            sym = n.Label;

            if (sym != null) { 
                // labeled for loop
                lab = s.label(sym);
                lab.continueTarget = bIncr;
                lab.breakTarget = bEnd;
            } 

            // generate body

            sym = sym__prev1;

        } 

        // generate body
        s.startBlock(bBody);
        s.stmtList(n.Body); 

        // tear down continue/break
        s.continueTo = prevContinue;
        s.breakTo = prevBreak;
        if (lab != null) {
            lab.continueTarget = null;
            lab.breakTarget = null;
        }
        {
            var b__prev1 = b;

            b = s.endBlock();

            if (b != null) {
                b.AddEdgeTo(bIncr);
            } 

            // generate incr (and, for OFORUNTIL, condition)

            b = b__prev1;

        } 

        // generate incr (and, for OFORUNTIL, condition)
        s.startBlock(bIncr);
        if (n.Post != null) {
            s.stmt(n.Post);
        }
        if (n.Op() == ir.OFOR) {
            {
                var b__prev2 = b;

                b = s.endBlock();

                if (b != null) {
                    b.AddEdgeTo(bCond); 
                    // It can happen that bIncr ends in a block containing only VARKILL,
                    // and that muddles the debugging experience.
                    if (b.Pos == src.NoXPos) {
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
            s.condBranch(n.Cond, bLateIncr, bEnd, 1); 
            // generate late increment
            s.startBlock(bLateIncr);
            s.stmtList(n.Late);
            s.endBlock().AddEdgeTo(bBody);
        }
        s.startBlock(bEnd);
        goto __switch_break0;
    }
    if (n.Op() == ir.OSWITCH || n.Op() == ir.OSELECT) 
    {
        // These have been mostly rewritten by the front end into their Nbody fields.
        // Our main task is to correctly hook up any break statements.
        bEnd = s.f.NewBlock(ssa.BlockPlain);

        prevBreak = s.breakTo;
        s.breakTo = bEnd;
        sym = ;
        ir.Nodes body = default;
        if (n.Op() == ir.OSWITCH) {
            n = n._<ptr<ir.SwitchStmt>>();
            sym = n.Label;
            body = n.Compiled;
        }
        else
 {
            n = n._<ptr<ir.SelectStmt>>();
            sym = n.Label;
            body = n.Compiled;
        }
        lab = ;
        if (sym != null) { 
            // labeled
            lab = s.label(sym);
            lab.breakTarget = bEnd;
        }
        s.stmtList(body);

        s.breakTo = prevBreak;
        if (lab != null) {
            lab.breakTarget = null;
        }
        if (s.curBlock != null) {
            m = s.mem();
            b = s.endBlock();
            b.Kind = ssa.BlockExit;
            b.SetControl(m);
        }
        s.startBlock(bEnd);
        goto __switch_break0;
    }
    if (n.Op() == ir.OVARDEF)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        if (!s.canSSA(n.X)) {
            s.vars[memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, n.X._<ptr<ir.Name>>(), s.mem(), false);
        }
        goto __switch_break0;
    }
    if (n.Op() == ir.OVARKILL) 
    {
        // Insert a varkill op to record that a variable is no longer live.
        // We only care about liveness info at call sites, so putting the
        // varkill in the store chain is enough to keep it correctly ordered
        // with respect to call ops.
        n = n._<ptr<ir.UnaryExpr>>();
        if (!s.canSSA(n.X)) {
            s.vars[memVar] = s.newValue1Apos(ssa.OpVarKill, types.TypeMem, n.X._<ptr<ir.Name>>(), s.mem(), false);
        }
        goto __switch_break0;
    }
    if (n.Op() == ir.OVARLIVE) 
    {
        // Insert a varlive op to record that a variable is still live.
        n = n._<ptr<ir.UnaryExpr>>();
        v = n.X._<ptr<ir.Name>>();
        if (!v.Addrtaken()) {
            s.Fatalf("VARLIVE variable %v must have Addrtaken set", v);
        }

        if (v.Class == ir.PAUTO || v.Class == ir.PPARAM || v.Class == ir.PPARAMOUT)         else 
            s.Fatalf("VARLIVE variable %v must be Auto or Arg", v);
                s.vars[memVar] = s.newValue1A(ssa.OpVarLive, types.TypeMem, v, s.mem());
        goto __switch_break0;
    }
    if (n.Op() == ir.OCHECKNIL)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        var p = s.expr(n.X);
        s.nilCheck(p);
        goto __switch_break0;
    }
    if (n.Op() == ir.OINLMARK)
    {
        n = n._<ptr<ir.InlineMarkStmt>>();
        s.newValue1I(ssa.OpInlMark, types.TypeVoid, n.Index, s.mem());
        goto __switch_break0;
    }
    // default: 
        s.Fatalf("unhandled stmt %v", n.Op());

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
private static ptr<ssa.Block> exit(this ptr<state> _addr_s) => func((_, panic, _) => {
    ref state s = ref _addr_s.val;

    if (s.hasdefer) {
        if (s.hasOpenDefers) {
            if (shareDeferExits && s.lastDeferExit != null && len(s.openDefers) == s.lastDeferCount) {
                if (s.curBlock.Kind != ssa.BlockPlain) {
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
            s.rtcall(ir.Syms.Deferreturn, true, null);
        }
    }
    ptr<ssa.Block> b;
    ptr<ssa.Value> m; 
    // Do actual return.
    // These currently turn into self-copies (in many cases).
    var resultFields = s.curfn.Type().Results().FieldSlice();
    var results = make_slice<ptr<ssa.Value>>(len(resultFields) + 1, len(resultFields) + 1);
    m = s.newValue0(ssa.OpMakeResult, s.f.OwnAux.LateExpansionResultType()); 
    // Store SSAable and heap-escaped PPARAMOUT variables back to stack locations.
    foreach (var (i, f) in resultFields) {
        ptr<ir.Name> n = f.Nname._<ptr<ir.Name>>();
        if (s.canSSA(n)) { // result is in some SSA variable
            if (!n.IsOutputParamInRegisters()) { 
                // We are about to store to the result slot.
                s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, n, s.mem());
            }
            results[i] = s.variable(n, n.Type());
        }
        else if (!n.OnStack()) { // result is actually heap allocated
            // We are about to copy the in-heap result to the result slot.
            s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, n, s.mem());
            var ha = s.expr(n.Heapaddr);
            s.instrumentFields(n.Type(), ha, instrumentRead);
            results[i] = s.newValue2(ssa.OpDereference, n.Type(), ha, s.mem());
        }
        else
 { // result is not SSA-able; not escaped, so not on heap, but too large for SSA.
            // Before register ABI this ought to be a self-move, home=dest,
            // With register ABI, it's still a self-move if parameter is on stack (i.e., too big or overflowed)
            // No VarDef, as the result slot is already holding live value.
            results[i] = s.newValue2(ssa.OpDereference, n.Type(), s.addr(n), s.mem());
        }
    }    s.stmtList(s.curfn.Exit);

    results[len(results) - 1] = s.mem();
    m.AddArgs(results);

    b = s.endBlock();
    b.Kind = ssa.BlockRet;
    b.SetControl(m);
    if (s.hasdefer && s.hasOpenDefers) {
        s.lastDeferFinalBlock = b;
    }
    return _addr_b!;
});

private partial struct opAndType {
    public ir.Op op;
    public types.Kind etype;
}

private static map opToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndType, ssa.Op>{opAndType{ir.OADD,types.TINT8}:ssa.OpAdd8,opAndType{ir.OADD,types.TUINT8}:ssa.OpAdd8,opAndType{ir.OADD,types.TINT16}:ssa.OpAdd16,opAndType{ir.OADD,types.TUINT16}:ssa.OpAdd16,opAndType{ir.OADD,types.TINT32}:ssa.OpAdd32,opAndType{ir.OADD,types.TUINT32}:ssa.OpAdd32,opAndType{ir.OADD,types.TINT64}:ssa.OpAdd64,opAndType{ir.OADD,types.TUINT64}:ssa.OpAdd64,opAndType{ir.OADD,types.TFLOAT32}:ssa.OpAdd32F,opAndType{ir.OADD,types.TFLOAT64}:ssa.OpAdd64F,opAndType{ir.OSUB,types.TINT8}:ssa.OpSub8,opAndType{ir.OSUB,types.TUINT8}:ssa.OpSub8,opAndType{ir.OSUB,types.TINT16}:ssa.OpSub16,opAndType{ir.OSUB,types.TUINT16}:ssa.OpSub16,opAndType{ir.OSUB,types.TINT32}:ssa.OpSub32,opAndType{ir.OSUB,types.TUINT32}:ssa.OpSub32,opAndType{ir.OSUB,types.TINT64}:ssa.OpSub64,opAndType{ir.OSUB,types.TUINT64}:ssa.OpSub64,opAndType{ir.OSUB,types.TFLOAT32}:ssa.OpSub32F,opAndType{ir.OSUB,types.TFLOAT64}:ssa.OpSub64F,opAndType{ir.ONOT,types.TBOOL}:ssa.OpNot,opAndType{ir.ONEG,types.TINT8}:ssa.OpNeg8,opAndType{ir.ONEG,types.TUINT8}:ssa.OpNeg8,opAndType{ir.ONEG,types.TINT16}:ssa.OpNeg16,opAndType{ir.ONEG,types.TUINT16}:ssa.OpNeg16,opAndType{ir.ONEG,types.TINT32}:ssa.OpNeg32,opAndType{ir.ONEG,types.TUINT32}:ssa.OpNeg32,opAndType{ir.ONEG,types.TINT64}:ssa.OpNeg64,opAndType{ir.ONEG,types.TUINT64}:ssa.OpNeg64,opAndType{ir.ONEG,types.TFLOAT32}:ssa.OpNeg32F,opAndType{ir.ONEG,types.TFLOAT64}:ssa.OpNeg64F,opAndType{ir.OBITNOT,types.TINT8}:ssa.OpCom8,opAndType{ir.OBITNOT,types.TUINT8}:ssa.OpCom8,opAndType{ir.OBITNOT,types.TINT16}:ssa.OpCom16,opAndType{ir.OBITNOT,types.TUINT16}:ssa.OpCom16,opAndType{ir.OBITNOT,types.TINT32}:ssa.OpCom32,opAndType{ir.OBITNOT,types.TUINT32}:ssa.OpCom32,opAndType{ir.OBITNOT,types.TINT64}:ssa.OpCom64,opAndType{ir.OBITNOT,types.TUINT64}:ssa.OpCom64,opAndType{ir.OIMAG,types.TCOMPLEX64}:ssa.OpComplexImag,opAndType{ir.OIMAG,types.TCOMPLEX128}:ssa.OpComplexImag,opAndType{ir.OREAL,types.TCOMPLEX64}:ssa.OpComplexReal,opAndType{ir.OREAL,types.TCOMPLEX128}:ssa.OpComplexReal,opAndType{ir.OMUL,types.TINT8}:ssa.OpMul8,opAndType{ir.OMUL,types.TUINT8}:ssa.OpMul8,opAndType{ir.OMUL,types.TINT16}:ssa.OpMul16,opAndType{ir.OMUL,types.TUINT16}:ssa.OpMul16,opAndType{ir.OMUL,types.TINT32}:ssa.OpMul32,opAndType{ir.OMUL,types.TUINT32}:ssa.OpMul32,opAndType{ir.OMUL,types.TINT64}:ssa.OpMul64,opAndType{ir.OMUL,types.TUINT64}:ssa.OpMul64,opAndType{ir.OMUL,types.TFLOAT32}:ssa.OpMul32F,opAndType{ir.OMUL,types.TFLOAT64}:ssa.OpMul64F,opAndType{ir.ODIV,types.TFLOAT32}:ssa.OpDiv32F,opAndType{ir.ODIV,types.TFLOAT64}:ssa.OpDiv64F,opAndType{ir.ODIV,types.TINT8}:ssa.OpDiv8,opAndType{ir.ODIV,types.TUINT8}:ssa.OpDiv8u,opAndType{ir.ODIV,types.TINT16}:ssa.OpDiv16,opAndType{ir.ODIV,types.TUINT16}:ssa.OpDiv16u,opAndType{ir.ODIV,types.TINT32}:ssa.OpDiv32,opAndType{ir.ODIV,types.TUINT32}:ssa.OpDiv32u,opAndType{ir.ODIV,types.TINT64}:ssa.OpDiv64,opAndType{ir.ODIV,types.TUINT64}:ssa.OpDiv64u,opAndType{ir.OMOD,types.TINT8}:ssa.OpMod8,opAndType{ir.OMOD,types.TUINT8}:ssa.OpMod8u,opAndType{ir.OMOD,types.TINT16}:ssa.OpMod16,opAndType{ir.OMOD,types.TUINT16}:ssa.OpMod16u,opAndType{ir.OMOD,types.TINT32}:ssa.OpMod32,opAndType{ir.OMOD,types.TUINT32}:ssa.OpMod32u,opAndType{ir.OMOD,types.TINT64}:ssa.OpMod64,opAndType{ir.OMOD,types.TUINT64}:ssa.OpMod64u,opAndType{ir.OAND,types.TINT8}:ssa.OpAnd8,opAndType{ir.OAND,types.TUINT8}:ssa.OpAnd8,opAndType{ir.OAND,types.TINT16}:ssa.OpAnd16,opAndType{ir.OAND,types.TUINT16}:ssa.OpAnd16,opAndType{ir.OAND,types.TINT32}:ssa.OpAnd32,opAndType{ir.OAND,types.TUINT32}:ssa.OpAnd32,opAndType{ir.OAND,types.TINT64}:ssa.OpAnd64,opAndType{ir.OAND,types.TUINT64}:ssa.OpAnd64,opAndType{ir.OOR,types.TINT8}:ssa.OpOr8,opAndType{ir.OOR,types.TUINT8}:ssa.OpOr8,opAndType{ir.OOR,types.TINT16}:ssa.OpOr16,opAndType{ir.OOR,types.TUINT16}:ssa.OpOr16,opAndType{ir.OOR,types.TINT32}:ssa.OpOr32,opAndType{ir.OOR,types.TUINT32}:ssa.OpOr32,opAndType{ir.OOR,types.TINT64}:ssa.OpOr64,opAndType{ir.OOR,types.TUINT64}:ssa.OpOr64,opAndType{ir.OXOR,types.TINT8}:ssa.OpXor8,opAndType{ir.OXOR,types.TUINT8}:ssa.OpXor8,opAndType{ir.OXOR,types.TINT16}:ssa.OpXor16,opAndType{ir.OXOR,types.TUINT16}:ssa.OpXor16,opAndType{ir.OXOR,types.TINT32}:ssa.OpXor32,opAndType{ir.OXOR,types.TUINT32}:ssa.OpXor32,opAndType{ir.OXOR,types.TINT64}:ssa.OpXor64,opAndType{ir.OXOR,types.TUINT64}:ssa.OpXor64,opAndType{ir.OEQ,types.TBOOL}:ssa.OpEqB,opAndType{ir.OEQ,types.TINT8}:ssa.OpEq8,opAndType{ir.OEQ,types.TUINT8}:ssa.OpEq8,opAndType{ir.OEQ,types.TINT16}:ssa.OpEq16,opAndType{ir.OEQ,types.TUINT16}:ssa.OpEq16,opAndType{ir.OEQ,types.TINT32}:ssa.OpEq32,opAndType{ir.OEQ,types.TUINT32}:ssa.OpEq32,opAndType{ir.OEQ,types.TINT64}:ssa.OpEq64,opAndType{ir.OEQ,types.TUINT64}:ssa.OpEq64,opAndType{ir.OEQ,types.TINTER}:ssa.OpEqInter,opAndType{ir.OEQ,types.TSLICE}:ssa.OpEqSlice,opAndType{ir.OEQ,types.TFUNC}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TMAP}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TCHAN}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TPTR}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TUINTPTR}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TUNSAFEPTR}:ssa.OpEqPtr,opAndType{ir.OEQ,types.TFLOAT64}:ssa.OpEq64F,opAndType{ir.OEQ,types.TFLOAT32}:ssa.OpEq32F,opAndType{ir.ONE,types.TBOOL}:ssa.OpNeqB,opAndType{ir.ONE,types.TINT8}:ssa.OpNeq8,opAndType{ir.ONE,types.TUINT8}:ssa.OpNeq8,opAndType{ir.ONE,types.TINT16}:ssa.OpNeq16,opAndType{ir.ONE,types.TUINT16}:ssa.OpNeq16,opAndType{ir.ONE,types.TINT32}:ssa.OpNeq32,opAndType{ir.ONE,types.TUINT32}:ssa.OpNeq32,opAndType{ir.ONE,types.TINT64}:ssa.OpNeq64,opAndType{ir.ONE,types.TUINT64}:ssa.OpNeq64,opAndType{ir.ONE,types.TINTER}:ssa.OpNeqInter,opAndType{ir.ONE,types.TSLICE}:ssa.OpNeqSlice,opAndType{ir.ONE,types.TFUNC}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TMAP}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TCHAN}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TPTR}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TUINTPTR}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TUNSAFEPTR}:ssa.OpNeqPtr,opAndType{ir.ONE,types.TFLOAT64}:ssa.OpNeq64F,opAndType{ir.ONE,types.TFLOAT32}:ssa.OpNeq32F,opAndType{ir.OLT,types.TINT8}:ssa.OpLess8,opAndType{ir.OLT,types.TUINT8}:ssa.OpLess8U,opAndType{ir.OLT,types.TINT16}:ssa.OpLess16,opAndType{ir.OLT,types.TUINT16}:ssa.OpLess16U,opAndType{ir.OLT,types.TINT32}:ssa.OpLess32,opAndType{ir.OLT,types.TUINT32}:ssa.OpLess32U,opAndType{ir.OLT,types.TINT64}:ssa.OpLess64,opAndType{ir.OLT,types.TUINT64}:ssa.OpLess64U,opAndType{ir.OLT,types.TFLOAT64}:ssa.OpLess64F,opAndType{ir.OLT,types.TFLOAT32}:ssa.OpLess32F,opAndType{ir.OLE,types.TINT8}:ssa.OpLeq8,opAndType{ir.OLE,types.TUINT8}:ssa.OpLeq8U,opAndType{ir.OLE,types.TINT16}:ssa.OpLeq16,opAndType{ir.OLE,types.TUINT16}:ssa.OpLeq16U,opAndType{ir.OLE,types.TINT32}:ssa.OpLeq32,opAndType{ir.OLE,types.TUINT32}:ssa.OpLeq32U,opAndType{ir.OLE,types.TINT64}:ssa.OpLeq64,opAndType{ir.OLE,types.TUINT64}:ssa.OpLeq64U,opAndType{ir.OLE,types.TFLOAT64}:ssa.OpLeq64F,opAndType{ir.OLE,types.TFLOAT32}:ssa.OpLeq32F,};

private static types.Kind concreteEtype(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var e = t.Kind();

    if (e == types.TINT) 
        if (s.config.PtrSize == 8) {
            return types.TINT64;
        }
        return types.TINT32;
    else if (e == types.TUINT) 
        if (s.config.PtrSize == 8) {
            return types.TUINT64;
        }
        return types.TUINT32;
    else if (e == types.TUINTPTR) 
        if (s.config.PtrSize == 8) {
            return types.TUINT64;
        }
        return types.TUINT32;
    else 
        return e;
    }

private static ssa.Op ssaOp(this ptr<state> _addr_s, ir.Op op, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var etype = s.concreteEtype(t);
    var (x, ok) = opToSSA[new opAndType(op,etype)];
    if (!ok) {
        s.Fatalf("unhandled binary op %v %s", op, etype);
    }
    return x;
}

private partial struct opAndTwoTypes {
    public ir.Op op;
    public types.Kind etype1;
    public types.Kind etype2;
}

private partial struct twoTypes {
    public types.Kind etype1;
    public types.Kind etype2;
}

private partial struct twoOpsAndType {
    public ssa.Op op1;
    public ssa.Op op2;
    public types.Kind intermediateType;
}

private static map fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{types.TINT8,types.TFLOAT32}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to32F,types.TINT32},twoTypes{types.TINT16,types.TFLOAT32}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to32F,types.TINT32},twoTypes{types.TINT32,types.TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to32F,types.TINT32},twoTypes{types.TINT64,types.TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to32F,types.TINT64},twoTypes{types.TINT8,types.TFLOAT64}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to64F,types.TINT32},twoTypes{types.TINT16,types.TFLOAT64}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to64F,types.TINT32},twoTypes{types.TINT32,types.TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to64F,types.TINT32},twoTypes{types.TINT64,types.TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to64F,types.TINT64},twoTypes{types.TFLOAT32,types.TINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,types.TINT32},twoTypes{types.TFLOAT32,types.TINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,types.TINT32},twoTypes{types.TFLOAT32,types.TINT32}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpCopy,types.TINT32},twoTypes{types.TFLOAT32,types.TINT64}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpCopy,types.TINT64},twoTypes{types.TFLOAT64,types.TINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,types.TINT32},twoTypes{types.TFLOAT64,types.TINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,types.TINT32},twoTypes{types.TFLOAT64,types.TINT32}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpCopy,types.TINT32},twoTypes{types.TFLOAT64,types.TINT64}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpCopy,types.TINT64},twoTypes{types.TUINT8,types.TFLOAT32}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to32F,types.TINT32},twoTypes{types.TUINT16,types.TFLOAT32}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to32F,types.TINT32},twoTypes{types.TUINT32,types.TFLOAT32}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to32F,types.TINT64},twoTypes{types.TUINT64,types.TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,types.TUINT64},twoTypes{types.TUINT8,types.TFLOAT64}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to64F,types.TINT32},twoTypes{types.TUINT16,types.TFLOAT64}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to64F,types.TINT32},twoTypes{types.TUINT32,types.TFLOAT64}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to64F,types.TINT64},twoTypes{types.TUINT64,types.TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,types.TUINT64},twoTypes{types.TFLOAT32,types.TUINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,types.TINT32},twoTypes{types.TFLOAT32,types.TUINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,types.TINT32},twoTypes{types.TFLOAT32,types.TUINT32}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpTrunc64to32,types.TINT64},twoTypes{types.TFLOAT32,types.TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,types.TUINT64},twoTypes{types.TFLOAT64,types.TUINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,types.TINT32},twoTypes{types.TFLOAT64,types.TUINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,types.TINT32},twoTypes{types.TFLOAT64,types.TUINT32}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpTrunc64to32,types.TINT64},twoTypes{types.TFLOAT64,types.TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,types.TUINT64},twoTypes{types.TFLOAT64,types.TFLOAT32}:twoOpsAndType{ssa.OpCvt64Fto32F,ssa.OpCopy,types.TFLOAT32},twoTypes{types.TFLOAT64,types.TFLOAT64}:twoOpsAndType{ssa.OpRound64F,ssa.OpCopy,types.TFLOAT64},twoTypes{types.TFLOAT32,types.TFLOAT32}:twoOpsAndType{ssa.OpRound32F,ssa.OpCopy,types.TFLOAT32},twoTypes{types.TFLOAT32,types.TFLOAT64}:twoOpsAndType{ssa.OpCvt32Fto64F,ssa.OpCopy,types.TFLOAT64},};

// this map is used only for 32-bit arch, and only includes the difference
// on 32-bit arch, don't use int64<->float conversion for uint32
private static map fpConvOpToSSA32 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{types.TUINT32,types.TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto32F,types.TUINT32},twoTypes{types.TUINT32,types.TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto64F,types.TUINT32},twoTypes{types.TFLOAT32,types.TUINT32}:twoOpsAndType{ssa.OpCvt32Fto32U,ssa.OpCopy,types.TUINT32},twoTypes{types.TFLOAT64,types.TUINT32}:twoOpsAndType{ssa.OpCvt64Fto32U,ssa.OpCopy,types.TUINT32},};

// uint64<->float conversions, only on machines that have instructions for that
private static map uint64fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{types.TUINT64,types.TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto32F,types.TUINT64},twoTypes{types.TUINT64,types.TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto64F,types.TUINT64},twoTypes{types.TFLOAT32,types.TUINT64}:twoOpsAndType{ssa.OpCvt32Fto64U,ssa.OpCopy,types.TUINT64},twoTypes{types.TFLOAT64,types.TUINT64}:twoOpsAndType{ssa.OpCvt64Fto64U,ssa.OpCopy,types.TUINT64},};

private static map shiftOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndTwoTypes, ssa.Op>{opAndTwoTypes{ir.OLSH,types.TINT8,types.TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{ir.OLSH,types.TUINT8,types.TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{ir.OLSH,types.TINT8,types.TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{ir.OLSH,types.TUINT8,types.TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{ir.OLSH,types.TINT8,types.TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{ir.OLSH,types.TUINT8,types.TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{ir.OLSH,types.TINT8,types.TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{ir.OLSH,types.TUINT8,types.TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{ir.OLSH,types.TINT16,types.TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{ir.OLSH,types.TUINT16,types.TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{ir.OLSH,types.TINT16,types.TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{ir.OLSH,types.TUINT16,types.TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{ir.OLSH,types.TINT16,types.TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{ir.OLSH,types.TUINT16,types.TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{ir.OLSH,types.TINT16,types.TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{ir.OLSH,types.TUINT16,types.TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{ir.OLSH,types.TINT32,types.TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{ir.OLSH,types.TUINT32,types.TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{ir.OLSH,types.TINT32,types.TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{ir.OLSH,types.TUINT32,types.TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{ir.OLSH,types.TINT32,types.TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{ir.OLSH,types.TUINT32,types.TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{ir.OLSH,types.TINT32,types.TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{ir.OLSH,types.TUINT32,types.TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{ir.OLSH,types.TINT64,types.TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{ir.OLSH,types.TUINT64,types.TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{ir.OLSH,types.TINT64,types.TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{ir.OLSH,types.TUINT64,types.TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{ir.OLSH,types.TINT64,types.TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{ir.OLSH,types.TUINT64,types.TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{ir.OLSH,types.TINT64,types.TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{ir.OLSH,types.TUINT64,types.TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{ir.ORSH,types.TINT8,types.TUINT8}:ssa.OpRsh8x8,opAndTwoTypes{ir.ORSH,types.TUINT8,types.TUINT8}:ssa.OpRsh8Ux8,opAndTwoTypes{ir.ORSH,types.TINT8,types.TUINT16}:ssa.OpRsh8x16,opAndTwoTypes{ir.ORSH,types.TUINT8,types.TUINT16}:ssa.OpRsh8Ux16,opAndTwoTypes{ir.ORSH,types.TINT8,types.TUINT32}:ssa.OpRsh8x32,opAndTwoTypes{ir.ORSH,types.TUINT8,types.TUINT32}:ssa.OpRsh8Ux32,opAndTwoTypes{ir.ORSH,types.TINT8,types.TUINT64}:ssa.OpRsh8x64,opAndTwoTypes{ir.ORSH,types.TUINT8,types.TUINT64}:ssa.OpRsh8Ux64,opAndTwoTypes{ir.ORSH,types.TINT16,types.TUINT8}:ssa.OpRsh16x8,opAndTwoTypes{ir.ORSH,types.TUINT16,types.TUINT8}:ssa.OpRsh16Ux8,opAndTwoTypes{ir.ORSH,types.TINT16,types.TUINT16}:ssa.OpRsh16x16,opAndTwoTypes{ir.ORSH,types.TUINT16,types.TUINT16}:ssa.OpRsh16Ux16,opAndTwoTypes{ir.ORSH,types.TINT16,types.TUINT32}:ssa.OpRsh16x32,opAndTwoTypes{ir.ORSH,types.TUINT16,types.TUINT32}:ssa.OpRsh16Ux32,opAndTwoTypes{ir.ORSH,types.TINT16,types.TUINT64}:ssa.OpRsh16x64,opAndTwoTypes{ir.ORSH,types.TUINT16,types.TUINT64}:ssa.OpRsh16Ux64,opAndTwoTypes{ir.ORSH,types.TINT32,types.TUINT8}:ssa.OpRsh32x8,opAndTwoTypes{ir.ORSH,types.TUINT32,types.TUINT8}:ssa.OpRsh32Ux8,opAndTwoTypes{ir.ORSH,types.TINT32,types.TUINT16}:ssa.OpRsh32x16,opAndTwoTypes{ir.ORSH,types.TUINT32,types.TUINT16}:ssa.OpRsh32Ux16,opAndTwoTypes{ir.ORSH,types.TINT32,types.TUINT32}:ssa.OpRsh32x32,opAndTwoTypes{ir.ORSH,types.TUINT32,types.TUINT32}:ssa.OpRsh32Ux32,opAndTwoTypes{ir.ORSH,types.TINT32,types.TUINT64}:ssa.OpRsh32x64,opAndTwoTypes{ir.ORSH,types.TUINT32,types.TUINT64}:ssa.OpRsh32Ux64,opAndTwoTypes{ir.ORSH,types.TINT64,types.TUINT8}:ssa.OpRsh64x8,opAndTwoTypes{ir.ORSH,types.TUINT64,types.TUINT8}:ssa.OpRsh64Ux8,opAndTwoTypes{ir.ORSH,types.TINT64,types.TUINT16}:ssa.OpRsh64x16,opAndTwoTypes{ir.ORSH,types.TUINT64,types.TUINT16}:ssa.OpRsh64Ux16,opAndTwoTypes{ir.ORSH,types.TINT64,types.TUINT32}:ssa.OpRsh64x32,opAndTwoTypes{ir.ORSH,types.TUINT64,types.TUINT32}:ssa.OpRsh64Ux32,opAndTwoTypes{ir.ORSH,types.TINT64,types.TUINT64}:ssa.OpRsh64x64,opAndTwoTypes{ir.ORSH,types.TUINT64,types.TUINT64}:ssa.OpRsh64Ux64,};

private static ssa.Op ssaShiftOp(this ptr<state> _addr_s, ir.Op op, ptr<types.Type> _addr_t, ptr<types.Type> _addr_u) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref types.Type u = ref _addr_u.val;

    var etype1 = s.concreteEtype(t);
    var etype2 = s.concreteEtype(u);
    var (x, ok) = shiftOpToSSA[new opAndTwoTypes(op,etype1,etype2)];
    if (!ok) {
        s.Fatalf("unhandled shift op %v etype=%s/%s", op, etype1, etype2);
    }
    return x;
}

private static ptr<ssa.Value> conv(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_v, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    if (ft.IsBoolean() && tt.IsKind(types.TUINT8)) { 
        // Bool -> uint8 is generated internally when indexing into runtime.staticbyte.
        return _addr_s.newValue1(ssa.OpCopy, tt, v)!;
    }
    if (ft.IsInteger() && tt.IsInteger()) {
        ssa.Op op = default;
        if (tt.Size() == ft.Size()) {
            op = ssa.OpCopy;
        }
        else if (tt.Size() < ft.Size()) { 
            // truncation
            switch (10 * ft.Size() + tt.Size()) {
                case 21: 
                    op = ssa.OpTrunc16to8;
                    break;
                case 41: 
                    op = ssa.OpTrunc32to8;
                    break;
                case 42: 
                    op = ssa.OpTrunc32to16;
                    break;
                case 81: 
                    op = ssa.OpTrunc64to8;
                    break;
                case 82: 
                    op = ssa.OpTrunc64to16;
                    break;
                case 84: 
                    op = ssa.OpTrunc64to32;
                    break;
                default: 
                    s.Fatalf("weird integer truncation %v -> %v", ft, tt);
                    break;
            }
        }
        else if (ft.IsSigned()) { 
            // sign extension
            switch (10 * ft.Size() + tt.Size()) {
                case 12: 
                    op = ssa.OpSignExt8to16;
                    break;
                case 14: 
                    op = ssa.OpSignExt8to32;
                    break;
                case 18: 
                    op = ssa.OpSignExt8to64;
                    break;
                case 24: 
                    op = ssa.OpSignExt16to32;
                    break;
                case 28: 
                    op = ssa.OpSignExt16to64;
                    break;
                case 48: 
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
            switch (10 * ft.Size() + tt.Size()) {
                case 12: 
                    op = ssa.OpZeroExt8to16;
                    break;
                case 14: 
                    op = ssa.OpZeroExt8to32;
                    break;
                case 18: 
                    op = ssa.OpZeroExt8to64;
                    break;
                case 24: 
                    op = ssa.OpZeroExt16to32;
                    break;
                case 28: 
                    op = ssa.OpZeroExt16to64;
                    break;
                case 48: 
                    op = ssa.OpZeroExt32to64;
                    break;
                default: 
                    s.Fatalf("weird integer sign extension %v -> %v", ft, tt);
                    break;
            }
        }
        return _addr_s.newValue1(op, tt, v)!;
    }
    if (ft.IsFloat() || tt.IsFloat()) {
        var (conv, ok) = fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];
        if (s.config.RegSize == 4 && Arch.LinkArch.Family != sys.MIPS && !s.softFloat) {
            {
                var conv1__prev3 = conv1;

                var (conv1, ok1) = fpConvOpToSSA32[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                if (ok1) {
                    conv = conv1;
                }

                conv1 = conv1__prev3;

            }
        }
        if (Arch.LinkArch.Family == sys.ARM64 || Arch.LinkArch.Family == sys.Wasm || Arch.LinkArch.Family == sys.S390X || s.softFloat) {
            {
                var conv1__prev3 = conv1;

                (conv1, ok1) = uint64fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                if (ok1) {
                    conv = conv1;
                }

                conv1 = conv1__prev3;

            }
        }
        if (Arch.LinkArch.Family == sys.MIPS && !s.softFloat) {
            if (ft.Size() == 4 && ft.IsInteger() && !ft.IsSigned()) { 
                // tt is float32 or float64, and ft is also unsigned
                if (tt.Size() == 4) {
                    return _addr_s.uint32Tofloat32(n, v, ft, tt)!;
                }
                if (tt.Size() == 8) {
                    return _addr_s.uint32Tofloat64(n, v, ft, tt)!;
                }
            }
            else if (tt.Size() == 4 && tt.IsInteger() && !tt.IsSigned()) { 
                // ft is float32 or float64, and tt is unsigned integer
                if (ft.Size() == 4) {
                    return _addr_s.float32ToUint32(n, v, ft, tt)!;
                }
                if (ft.Size() == 8) {
                    return _addr_s.float64ToUint32(n, v, ft, tt)!;
                }
            }
        }
        if (!ok) {
            s.Fatalf("weird float conversion %v -> %v", ft, tt);
        }
        var op1 = conv.op1;
        var op2 = conv.op2;
        var it = conv.intermediateType;

        if (op1 != ssa.OpInvalid && op2 != ssa.OpInvalid) { 
            // normal case, not tripping over unsigned 64
            if (op1 == ssa.OpCopy) {
                if (op2 == ssa.OpCopy) {
                    return _addr_v!;
                }
                return _addr_s.newValueOrSfCall1(op2, tt, v)!;
            }
            if (op2 == ssa.OpCopy) {
                return _addr_s.newValueOrSfCall1(op1, tt, v)!;
            }
            return _addr_s.newValueOrSfCall1(op2, tt, s.newValueOrSfCall1(op1, types.Types[it], v))!;
        }
        if (ft.IsInteger()) { 
            // tt is float32 or float64, and ft is also unsigned
            if (tt.Size() == 4) {
                return _addr_s.uint64Tofloat32(n, v, ft, tt)!;
            }
            if (tt.Size() == 8) {
                return _addr_s.uint64Tofloat64(n, v, ft, tt)!;
            }
            s.Fatalf("weird unsigned integer to float conversion %v -> %v", ft, tt);
        }
        if (ft.Size() == 4) {
            return _addr_s.float32ToUint64(n, v, ft, tt)!;
        }
        if (ft.Size() == 8) {
            return _addr_s.float64ToUint64(n, v, ft, tt)!;
        }
        s.Fatalf("weird float to unsigned integer conversion %v -> %v", ft, tt);
        return _addr_null!;
    }
    if (ft.IsComplex() && tt.IsComplex()) {
        op = default;
        if (ft.Size() == tt.Size()) {
            switch (ft.Size()) {
                case 8: 
                    op = ssa.OpRound32F;
                    break;
                case 16: 
                    op = ssa.OpRound64F;
                    break;
                default: 
                    s.Fatalf("weird complex conversion %v -> %v", ft, tt);
                    break;
            }
        }
        else if (ft.Size() == 8 && tt.Size() == 16) {
            op = ssa.OpCvt32Fto64F;
        }
        else if (ft.Size() == 16 && tt.Size() == 8) {
            op = ssa.OpCvt64Fto32F;
        }
        else
 {
            s.Fatalf("weird complex conversion %v -> %v", ft, tt);
        }
        var ftp = types.FloatForComplex(ft);
        var ttp = types.FloatForComplex(tt);
        return _addr_s.newValue2(ssa.OpComplexMake, tt, s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexReal, ftp, v)), s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexImag, ftp, v)))!;
    }
    s.Fatalf("unhandled OCONV %s -> %s", ft.Kind(), tt.Kind());
    return _addr_null!;
}

// expr converts the expression n to ssa, adds it to s and returns the ssa result.
private static ptr<ssa.Value> expr(this ptr<state> _addr_s, ir.Node n) => func((defer, panic, _) => {
    ref state s = ref _addr_s.val;

    if (ir.HasUniquePos(n)) { 
        // ONAMEs and named OLITERALs have the line number
        // of the decl, not the use. See issue 14742.
        s.pushLine(n.Pos());
        defer(s.popLine());
    }
    s.stmtList(n.Init());

    if (n.Op() == ir.OBYTES2STRTMP)
    {
        ptr<ir.ConvExpr> n = n._<ptr<ir.ConvExpr>>();
        var slice = s.expr(n.X);
        var ptr = s.newValue1(ssa.OpSlicePtr, s.f.Config.Types.BytePtr, slice);
        var len = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], slice);
        return _addr_s.newValue2(ssa.OpStringMake, n.Type(), ptr, len)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSTR2BYTESTMP)
    {
        n = n._<ptr<ir.ConvExpr>>();
        var str = s.expr(n.X);
        ptr = s.newValue1(ssa.OpStringPtr, s.f.Config.Types.BytePtr, str);
        len = s.newValue1(ssa.OpStringLen, types.Types[types.TINT], str);
        return _addr_s.newValue3(ssa.OpSliceMake, n.Type(), ptr, len, len)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OCFUNC)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        ptr<ir.Name> aux = n.X._<ptr<ir.Name>>().Linksym(); 
        // OCFUNC is used to build function values, which must
        // always reference ABIInternal entry points.
        if (aux.ABI() != obj.ABIInternal) {
            s.Fatalf("expected ABIInternal: %v", aux.ABI());
        }
        return _addr_s.entryNewValue1A(ssa.OpAddr, n.Type(), aux, s.sb)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ONAME)
    {
        n = n._<ptr<ir.Name>>();
        if (n.Class == ir.PFUNC) { 
            // "value" of a function is the address of the function's closure
            var sym = staticdata.FuncLinksym(n);
            return _addr_s.entryNewValue1A(ssa.OpAddr, types.NewPtr(n.Type()), sym, s.sb)!;
        }
        if (s.canSSA(n)) {
            return _addr_s.variable(n, n.Type())!;
        }
        return _addr_s.load(n.Type(), s.addr(n))!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OLINKSYMOFFSET)
    {
        n = n._<ptr<ir.LinksymOffsetExpr>>();
        return _addr_s.load(n.Type(), s.addr(n))!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ONIL)
    {
        n = n._<ptr<ir.NilExpr>>();
        var t = n.Type();

        if (t.IsSlice()) 
            return _addr_s.constSlice(t)!;
        else if (t.IsInterface()) 
            return _addr_s.constInterface(t)!;
        else 
            return _addr_s.constNil(t)!;
                goto __switch_break1;
    }
    if (n.Op() == ir.OLITERAL)
    {
        {
            var u = n.Val();


            if (u.Kind() == constant.Int) 
                var i = ir.IntVal(n.Type(), u);
                switch (n.Type().Size()) {
                    case 1: 
                        return _addr_s.constInt8(n.Type(), int8(i))!;
                        break;
                    case 2: 
                        return _addr_s.constInt16(n.Type(), int16(i))!;
                        break;
                    case 4: 
                        return _addr_s.constInt32(n.Type(), int32(i))!;
                        break;
                    case 8: 
                        return _addr_s.constInt64(n.Type(), i)!;
                        break;
                    default: 
                        s.Fatalf("bad integer size %d", n.Type().Size());
                        return _addr_null!;
                        break;
                }
            else if (u.Kind() == constant.String) 
                i = constant.StringVal(u);
                if (i == "") {
                    return _addr_s.constEmptyString(n.Type())!;
                }
                return _addr_s.entryNewValue0A(ssa.OpConstString, n.Type(), ssa.StringToAux(i))!;
            else if (u.Kind() == constant.Bool) 
                return _addr_s.constBool(constant.BoolVal(u))!;
            else if (u.Kind() == constant.Float) 
                var (f, _) = constant.Float64Val(u);
                switch (n.Type().Size()) {
                    case 4: 
                        return _addr_s.constFloat32(n.Type(), f)!;
                        break;
                    case 8: 
                        return _addr_s.constFloat64(n.Type(), f)!;
                        break;
                    default: 
                        s.Fatalf("bad float size %d", n.Type().Size());
                        return _addr_null!;
                        break;
                }
            else if (u.Kind() == constant.Complex) 
                var (re, _) = constant.Float64Val(constant.Real(u));
                var (im, _) = constant.Float64Val(constant.Imag(u));
                switch (n.Type().Size()) {
                    case 8: 
                        var pt = types.Types[types.TFLOAT32];
                        return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), s.constFloat32(pt, re), s.constFloat32(pt, im))!;
                        break;
                    case 16: 
                        pt = types.Types[types.TFLOAT64];
                        return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), s.constFloat64(pt, re), s.constFloat64(pt, im))!;
                        break;
                    default: 
                        s.Fatalf("bad complex size %d", n.Type().Size());
                        return _addr_null!;
                        break;
                }
            else 
                s.Fatalf("unhandled OLITERAL %v", u.Kind());
                return _addr_null!;

        }
        goto __switch_break1;
    }
    if (n.Op() == ir.OCONVNOP)
    {
        n = n._<ptr<ir.ConvExpr>>();
        var to = n.Type();
        var from = n.X.Type(); 

        // Assume everything will work out, so set up our return value.
        // Anything interesting that happens from here is a fatal.
        var x = s.expr(n.X);
        if (to == from) {
            return _addr_x!;
        }
        if (to.IsPtrShaped() != from.IsPtrShaped()) {
            return _addr_s.newValue2(ssa.OpConvert, to, x, s.mem())!;
        }
        var v = s.newValue1(ssa.OpCopy, to, x); // ensure that v has the right type

        // CONVNOP closure
        if (to.Kind() == types.TFUNC && from.IsPtrShaped()) {
            return _addr_v!;
        }
        if (from.Kind() == to.Kind()) {
            return _addr_v!;
        }
        if (to.IsUnsafePtr() && from.IsPtrShaped() || from.IsUnsafePtr() && to.IsPtrShaped()) {
            return _addr_v!;
        }
        if (to.Kind() == types.TMAP && from.IsPtr() && to.MapType().Hmap == from.Elem()) {
            return _addr_v!;
        }
        types.CalcSize(from);
        types.CalcSize(to);
        if (from.Width != to.Width) {
            s.Fatalf("CONVNOP width mismatch %v (%d) -> %v (%d)\n", from, from.Width, to, to.Width);
            return _addr_null!;
        }
        if (etypesign(from.Kind()) != etypesign(to.Kind())) {
            s.Fatalf("CONVNOP sign mismatch %v (%s) -> %v (%s)\n", from, from.Kind(), to, to.Kind());
            return _addr_null!;
        }
        if (@base.Flag.Cfg.Instrumenting) { 
            // These appear to be fine, but they fail the
            // integer constraint below, so okay them here.
            // Sample non-integer conversion: map[string]string -> *uint8
            return _addr_v!;
        }
        if (etypesign(from.Kind()) == 0) {
            s.Fatalf("CONVNOP unrecognized non-integer %v -> %v\n", from, to);
            return _addr_null!;
        }
        return _addr_v!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OCONV)
    {
        n = n._<ptr<ir.ConvExpr>>();
        x = s.expr(n.X);
        return _addr_s.conv(n, x, n.X.Type(), n.Type())!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ODOTTYPE)
    {
        n = n._<ptr<ir.TypeAssertExpr>>();
        var (res, _) = s.dottype(n, false);
        return _addr_res!; 

        // binary ops
        goto __switch_break1;
    }
    if (n.Op() == ir.OLT || n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLE || n.Op() == ir.OGE || n.Op() == ir.OGT)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        var a = s.expr(n.X);
        var b = s.expr(n.Y);
        if (n.X.Type().IsComplex()) {
            pt = types.FloatForComplex(n.X.Type());
            var op = s.ssaOp(ir.OEQ, pt);
            var r = s.newValueOrSfCall2(op, types.Types[types.TBOOL], s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b));
            i = s.newValueOrSfCall2(op, types.Types[types.TBOOL], s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b));
            var c = s.newValue2(ssa.OpAndB, types.Types[types.TBOOL], r, i);

            if (n.Op() == ir.OEQ) 
                return _addr_c!;
            else if (n.Op() == ir.ONE) 
                return _addr_s.newValue1(ssa.OpNot, types.Types[types.TBOOL], c)!;
            else 
                s.Fatalf("ordered complex compare %v", n.Op());
                    }
        op = n.Op();

        if (op == ir.OGE) 
            (op, a, b) = (ir.OLE, b, a);        else if (op == ir.OGT) 
            (op, a, b) = (ir.OLT, b, a);                if (n.X.Type().IsFloat()) { 
            // float comparison
            return _addr_s.newValueOrSfCall2(s.ssaOp(op, n.X.Type()), types.Types[types.TBOOL], a, b)!;
        }
        return _addr_s.newValue2(s.ssaOp(op, n.X.Type()), types.Types[types.TBOOL], a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OMUL)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        if (n.Type().IsComplex()) {
            var mulop = ssa.OpMul64F;
            var addop = ssa.OpAdd64F;
            var subop = ssa.OpSub64F;
            pt = types.FloatForComplex(n.Type()); // Could be Float32 or Float64
            var wt = types.Types[types.TFLOAT64]; // Compute in Float64 to minimize cancellation error

            var areal = s.newValue1(ssa.OpComplexReal, pt, a);
            var breal = s.newValue1(ssa.OpComplexReal, pt, b);
            var aimag = s.newValue1(ssa.OpComplexImag, pt, a);
            var bimag = s.newValue1(ssa.OpComplexImag, pt, b);

            if (pt != wt) { // Widen for calculation
                areal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, areal);
                breal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, breal);
                aimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, aimag);
                bimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, bimag);
            }
            var xreal = s.newValueOrSfCall2(subop, wt, s.newValueOrSfCall2(mulop, wt, areal, breal), s.newValueOrSfCall2(mulop, wt, aimag, bimag));
            var ximag = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, areal, bimag), s.newValueOrSfCall2(mulop, wt, aimag, breal));

            if (pt != wt) { // Narrow to store back
                xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);
            }
            return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), xreal, ximag)!;
        }
        if (n.Type().IsFloat()) {
            return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        }
        return _addr_s.newValue2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ODIV)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        if (n.Type().IsComplex()) { 
            // TODO this is not executed because the front-end substitutes a runtime call.
            // That probably ought to change; with modest optimization the widen/narrow
            // conversions could all be elided in larger expression trees.
            mulop = ssa.OpMul64F;
            addop = ssa.OpAdd64F;
            subop = ssa.OpSub64F;
            var divop = ssa.OpDiv64F;
            pt = types.FloatForComplex(n.Type()); // Could be Float32 or Float64
            wt = types.Types[types.TFLOAT64]; // Compute in Float64 to minimize cancellation error

            areal = s.newValue1(ssa.OpComplexReal, pt, a);
            breal = s.newValue1(ssa.OpComplexReal, pt, b);
            aimag = s.newValue1(ssa.OpComplexImag, pt, a);
            bimag = s.newValue1(ssa.OpComplexImag, pt, b);

            if (pt != wt) { // Widen for calculation
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

            if (pt != wt) { // Narrow to store back
                xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);
            }
            return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), xreal, ximag)!;
        }
        if (n.Type().IsFloat()) {
            return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        }
        return _addr_s.intDivide(n, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OMOD)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        return _addr_s.intDivide(n, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OADD || n.Op() == ir.OSUB)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        if (n.Type().IsComplex()) {
            pt = types.FloatForComplex(n.Type());
            op = s.ssaOp(n.Op(), pt);
            return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b)), s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b)))!;
        }
        if (n.Type().IsFloat()) {
            return _addr_s.newValueOrSfCall2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        }
        return _addr_s.newValue2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OAND || n.Op() == ir.OOR || n.Op() == ir.OXOR)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        return _addr_s.newValue2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OANDNOT)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        b = s.newValue1(s.ssaOp(ir.OBITNOT, b.Type), b.Type, b);
        return _addr_s.newValue2(s.ssaOp(ir.OAND, n.Type()), a.Type, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OLSH || n.Op() == ir.ORSH)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        a = s.expr(n.X);
        b = s.expr(n.Y);
        var bt = b.Type;
        if (bt.IsSigned()) {
            var cmp = s.newValue2(s.ssaOp(ir.OLE, bt), types.Types[types.TBOOL], s.zeroVal(bt), b);
            s.check(cmp, ir.Syms.Panicshift);
            bt = bt.ToUnsigned();
        }
        return _addr_s.newValue2(s.ssaShiftOp(n.Op(), n.Type(), bt), a.Type, a, b)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
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
        n = n._<ptr<ir.LogicalExpr>>();
        var el = s.expr(n.X);
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
        if (n.Op() == ir.OANDAND) {
            b.AddEdgeTo(bRight);
            b.AddEdgeTo(bResult);
        }
        else if (n.Op() == ir.OOROR) {
            b.AddEdgeTo(bResult);
            b.AddEdgeTo(bRight);
        }
        s.startBlock(bRight);
        var er = s.expr(n.Y);
        s.vars[n] = er;

        b = s.endBlock();
        b.AddEdgeTo(bResult);

        s.startBlock(bResult);
        return _addr_s.variable(n, types.Types[types.TBOOL])!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OCOMPLEX)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        r = s.expr(n.X);
        i = s.expr(n.Y);
        return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), r, i)!; 

        // unary ops
        goto __switch_break1;
    }
    if (n.Op() == ir.ONEG)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        if (n.Type().IsComplex()) {
            var tp = types.FloatForComplex(n.Type());
            var negop = s.ssaOp(n.Op(), tp);
            return _addr_s.newValue2(ssa.OpComplexMake, n.Type(), s.newValue1(negop, tp, s.newValue1(ssa.OpComplexReal, tp, a)), s.newValue1(negop, tp, s.newValue1(ssa.OpComplexImag, tp, a)))!;
        }
        return _addr_s.newValue1(s.ssaOp(n.Op(), n.Type()), a.Type, a)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ONOT || n.Op() == ir.OBITNOT)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        return _addr_s.newValue1(s.ssaOp(n.Op(), n.Type()), a.Type, a)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OIMAG || n.Op() == ir.OREAL)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        return _addr_s.newValue1(s.ssaOp(n.Op(), n.X.Type()), n.Type(), a)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OPLUS)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        return _addr_s.expr(n.X)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OADDR)
    {
        n = n._<ptr<ir.AddrExpr>>();
        return _addr_s.addr(n.X)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ORESULT)
    {
        n = n._<ptr<ir.ResultExpr>>();
        if (s.prevCall == null || s.prevCall.Op != ssa.OpStaticLECall && s.prevCall.Op != ssa.OpInterLECall && s.prevCall.Op != ssa.OpClosureLECall) {
            panic("Expected to see a previous call");
        }
        var which = n.Index;
        if (which == -1) {
            panic(fmt.Errorf("ORESULT %v does not match call %s", n, s.prevCall));
        }
        return _addr_s.resultOfCall(s.prevCall, which, n.Type())!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ODEREF)
    {
        n = n._<ptr<ir.StarExpr>>();
        var p = s.exprPtr(n.X, n.Bounded(), n.Pos());
        return _addr_s.load(n.Type(), p)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ODOT)
    {
        n = n._<ptr<ir.SelectorExpr>>();
        if (n.X.Op() == ir.OSTRUCTLIT) { 
            // All literals with nonzero fields have already been
            // rewritten during walk. Any that remain are just T{}
            // or equivalents. Use the zero value.
            if (!ir.IsZero(n.X)) {
                s.Fatalf("literal with nonzero value in SSA: %v", n.X);
            }
            return _addr_s.zeroVal(n.Type())!;
        }
        if (ir.IsAddressable(n) && !s.canSSA(n)) {
            p = s.addr(n);
            return _addr_s.load(n.Type(), p)!;
        }
        v = s.expr(n.X);
        return _addr_s.newValue1I(ssa.OpStructSelect, n.Type(), int64(fieldIdx(n)), v)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ODOTPTR)
    {
        n = n._<ptr<ir.SelectorExpr>>();
        p = s.exprPtr(n.X, n.Bounded(), n.Pos());
        p = s.newValue1I(ssa.OpOffPtr, types.NewPtr(n.Type()), n.Offset(), p);
        return _addr_s.load(n.Type(), p)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OINDEX)
    {
        n = n._<ptr<ir.IndexExpr>>();

        if (n.X.Type().IsString()) 
            if (n.Bounded() && ir.IsConst(n.X, constant.String) && ir.IsConst(n.Index, constant.Int)) { 
                // Replace "abc"[1] with 'b'.
                // Delayed until now because "abc"[1] is not an ideal constant.
                // See test/fixedbugs/issue11370.go.
                return _addr_s.newValue0I(ssa.OpConst8, types.Types[types.TUINT8], int64(int8(ir.StringVal(n.X)[ir.Int64Val(n.Index)])))!;
            }
            a = s.expr(n.X);
            i = s.expr(n.Index);
            len = s.newValue1(ssa.OpStringLen, types.Types[types.TINT], a);
            i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
            var ptrtyp = s.f.Config.Types.BytePtr;
            ptr = s.newValue1(ssa.OpStringPtr, ptrtyp, a);
            if (ir.IsConst(n.Index, constant.Int)) {
                ptr = s.newValue1I(ssa.OpOffPtr, ptrtyp, ir.Int64Val(n.Index), ptr);
            }
            else
 {
                ptr = s.newValue2(ssa.OpAddPtr, ptrtyp, ptr, i);
            }
            return _addr_s.load(types.Types[types.TUINT8], ptr)!;
        else if (n.X.Type().IsSlice()) 
            p = s.addr(n);
            return _addr_s.load(n.X.Type().Elem(), p)!;
        else if (n.X.Type().IsArray()) 
            if (TypeOK(_addr_n.X.Type())) { 
                // SSA can handle arrays of length at most 1.
                var bound = n.X.Type().NumElem();
                a = s.expr(n.X);
                i = s.expr(n.Index);
                if (bound == 0) { 
                    // Bounds check will never succeed.  Might as well
                    // use constants for the bounds check.
                    var z = s.constInt(types.Types[types.TINT], 0);
                    s.boundsCheck(z, z, ssa.BoundsIndex, false); 
                    // The return value won't be live, return junk.
                    return _addr_s.newValue0(ssa.OpUnknown, n.Type())!;
                }
                len = s.constInt(types.Types[types.TINT], bound);
                s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded()); // checks i == 0
                return _addr_s.newValue1I(ssa.OpArraySelect, n.Type(), 0, a)!;
            }
            p = s.addr(n);
            return _addr_s.load(n.X.Type().Elem(), p)!;
        else 
            s.Fatalf("bad type for index %v", n.X.Type());
            return _addr_null!;
                goto __switch_break1;
    }
    if (n.Op() == ir.OLEN || n.Op() == ir.OCAP)
    {
        n = n._<ptr<ir.UnaryExpr>>();

        if (n.X.Type().IsSlice()) 
            op = ssa.OpSliceLen;
            if (n.Op() == ir.OCAP) {
                op = ssa.OpSliceCap;
            }
            return _addr_s.newValue1(op, types.Types[types.TINT], s.expr(n.X))!;
        else if (n.X.Type().IsString()) // string; not reachable for OCAP
            return _addr_s.newValue1(ssa.OpStringLen, types.Types[types.TINT], s.expr(n.X))!;
        else if (n.X.Type().IsMap() || n.X.Type().IsChan()) 
            return _addr_s.referenceTypeBuiltin(n, s.expr(n.X))!;
        else // array
            return _addr_s.constInt(types.Types[types.TINT], n.X.Type().NumElem())!;
                goto __switch_break1;
    }
    if (n.Op() == ir.OSPTR)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        if (n.X.Type().IsSlice()) {
            return _addr_s.newValue1(ssa.OpSlicePtr, n.Type(), a)!;
        }
        else
 {
            return _addr_s.newValue1(ssa.OpStringPtr, n.Type(), a)!;
        }
        goto __switch_break1;
    }
    if (n.Op() == ir.OITAB)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        return _addr_s.newValue1(ssa.OpITab, n.Type(), a)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OIDATA)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        a = s.expr(n.X);
        return _addr_s.newValue1(ssa.OpIData, n.Type(), a)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OEFACE)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        var tab = s.expr(n.X);
        var data = s.expr(n.Y);
        return _addr_s.newValue2(ssa.OpIMake, n.Type(), tab, data)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSLICEHEADER)
    {
        n = n._<ptr<ir.SliceHeaderExpr>>();
        p = s.expr(n.Ptr);
        var l = s.expr(n.Len);
        c = s.expr(n.Cap);
        return _addr_s.newValue3(ssa.OpSliceMake, n.Type(), p, l, c)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICEARR || n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR)
    {
        n = n._<ptr<ir.SliceExpr>>();
        v = s.expr(n.X);
        i = ;        ptr<ssa.Value> j;        ptr<ssa.Value> k;

        if (n.Low != null) {
            i = s.expr(n.Low);
        }
        if (n.High != null) {
            j = s.expr(n.High);
        }
        if (n.Max != null) {
            k = s.expr(n.Max);
        }
        var (p, l, c) = s.slice(v, i, j, k, n.Bounded());
        return _addr_s.newValue3(ssa.OpSliceMake, n.Type(), p, l, c)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSLICESTR)
    {
        n = n._<ptr<ir.SliceExpr>>();
        v = s.expr(n.X);
        i = ;        j = ;

        if (n.Low != null) {
            i = s.expr(n.Low);
        }
        if (n.High != null) {
            j = s.expr(n.High);
        }
        var (p, l, _) = s.slice(v, i, j, null, n.Bounded());
        return _addr_s.newValue2(ssa.OpStringMake, n.Type(), p, l)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSLICE2ARRPTR) 
    {
        // if arrlen > slice.len {
        //   panic(...)
        // }
        // slice.ptr
        n = n._<ptr<ir.ConvExpr>>();
        v = s.expr(n.X);
        var arrlen = s.constInt(types.Types[types.TINT], n.Type().Elem().NumElem());
        var cap = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], v);
        s.boundsCheck(arrlen, cap, ssa.BoundsConvert, false);
        return _addr_s.newValue1(ssa.OpSlicePtrUnchecked, n.Type(), v)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OCALLFUNC)
    {
        n = n._<ptr<ir.CallExpr>>();
        if (ir.IsIntrinsicCall(n)) {
            return _addr_s.intrinsicCall(n)!;
        }
        fallthrough = true;

    }
    if (fallthrough || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH)
    {
        n = n._<ptr<ir.CallExpr>>();
        return _addr_s.callResult(n, callNormal)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OGETG)
    {
        n = n._<ptr<ir.CallExpr>>();
        return _addr_s.newValue1(ssa.OpGetG, n.Type(), s.mem())!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OAPPEND)
    {
        return _addr_s.append(n._<ptr<ir.CallExpr>>(), false)!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OSTRUCTLIT || n.Op() == ir.OARRAYLIT) 
    {
        // All literals with nonzero fields have already been
        // rewritten during walk. Any that remain are just T{}
        // or equivalents. Use the zero value.
        n = n._<ptr<ir.CompLitExpr>>();
        if (!ir.IsZero(n)) {
            s.Fatalf("literal with nonzero value in SSA: %v", n);
        }
        return _addr_s.zeroVal(n.Type())!;
        goto __switch_break1;
    }
    if (n.Op() == ir.ONEW)
    {
        n = n._<ptr<ir.UnaryExpr>>();
        return _addr_s.newObject(n.Type().Elem())!;
        goto __switch_break1;
    }
    if (n.Op() == ir.OUNSAFEADD)
    {
        n = n._<ptr<ir.BinaryExpr>>();
        ptr = s.expr(n.X);
        len = s.expr(n.Y);
        return _addr_s.newValue2(ssa.OpAddPtr, n.Type(), ptr, len)!;
        goto __switch_break1;
    }
    // default: 
        s.Fatalf("unhandled expr %v", n.Op());
        return _addr_null!;

    __switch_break1:;
});

private static ptr<ssa.Value> resultOfCall(this ptr<state> _addr_s, ptr<ssa.Value> _addr_c, long which, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref ssa.Value c = ref _addr_c.val;
    ref types.Type t = ref _addr_t.val;

    ptr<ssa.AuxCall> aux = c.Aux._<ptr<ssa.AuxCall>>();
    var pa = aux.ParamAssignmentForResult(which); 
    // TODO(register args) determine if in-memory TypeOK is better loaded early from SelectNAddr or later when SelectN is expanded.
    // SelectN is better for pattern-matching and possible call-aware analysis we might want to do in the future.
    if (len(pa.Registers) == 0 && !TypeOK(_addr_t)) {
        var addr = s.newValue1I(ssa.OpSelectNAddr, types.NewPtr(t), which, c);
        return _addr_s.rawLoad(t, addr)!;
    }
    return _addr_s.newValue1I(ssa.OpSelectN, t, which, c)!;
}

private static ptr<ssa.Value> resultAddrOfCall(this ptr<state> _addr_s, ptr<ssa.Value> _addr_c, long which, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref ssa.Value c = ref _addr_c.val;
    ref types.Type t = ref _addr_t.val;

    ptr<ssa.AuxCall> aux = c.Aux._<ptr<ssa.AuxCall>>();
    var pa = aux.ParamAssignmentForResult(which);
    if (len(pa.Registers) == 0) {
        return _addr_s.newValue1I(ssa.OpSelectNAddr, types.NewPtr(t), which, c)!;
    }
    var (_, addr) = s.temp(c.Pos, t);
    var rval = s.newValue1I(ssa.OpSelectN, t, which, c);
    s.vars[memVar] = s.newValue3Apos(ssa.OpStore, types.TypeMem, t, addr, rval, s.mem(), false);
    return _addr_addr!;
}

// append converts an OAPPEND node to SSA.
// If inplace is false, it converts the OAPPEND expression n to an ssa.Value,
// adds it to s, and returns the Value.
// If inplace is true, it writes the result of the OAPPEND expression n
// back to the slice being appended to, and returns nil.
// inplace MUST be set to false if the slice can be SSA'd.
private static ptr<ssa.Value> append(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n, bool inplace) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;
 
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

    var et = n.Type().Elem();
    var pt = types.NewPtr(et); 

    // Evaluate slice
    var sn = n.Args[0]; // the slice node is the first in the list

    ptr<ssa.Value> slice;    ptr<ssa.Value> addr;

    if (inplace) {
        addr = s.addr(sn);
        slice = s.load(n.Type(), addr);
    }
    else
 {
        slice = s.expr(sn);
    }
    var grow = s.f.NewBlock(ssa.BlockPlain);
    var assign = s.f.NewBlock(ssa.BlockPlain); 

    // Decide if we need to grow
    var nargs = int64(len(n.Args) - 1);
    var p = s.newValue1(ssa.OpSlicePtr, pt, slice);
    var l = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], slice);
    var c = s.newValue1(ssa.OpSliceCap, types.Types[types.TINT], slice);
    var nl = s.newValue2(s.ssaOp(ir.OADD, types.Types[types.TINT]), types.Types[types.TINT], l, s.constInt(types.Types[types.TINT], nargs));

    var cmp = s.newValue2(s.ssaOp(ir.OLT, types.Types[types.TUINT]), types.Types[types.TBOOL], c, nl);
    s.vars[ptrVar] = p;

    if (!inplace) {
        s.vars[newlenVar] = nl;
        s.vars[capVar] = c;
    }
    else
 {
        s.vars[lenVar] = l;
    }
    var b = s.endBlock();
    b.Kind = ssa.BlockIf;
    b.Likely = ssa.BranchUnlikely;
    b.SetControl(cmp);
    b.AddEdgeTo(grow);
    b.AddEdgeTo(assign); 

    // Call growslice
    s.startBlock(grow);
    var taddr = s.expr(n.X);
    var r = s.rtcall(ir.Syms.Growslice, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { pt, types.Types[types.TINT], types.Types[types.TINT] }), taddr, p, l, c, nl);

    if (inplace) {
        if (sn.Op() == ir.ONAME) {
            sn = sn._<ptr<ir.Name>>();
            if (sn.Class != ir.PEXTERN) { 
                // Tell liveness we're about to build a new slice
                s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, sn, s.mem());
            }
        }
        var capaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, types.SliceCapOffset, addr);
        s.store(types.Types[types.TINT], capaddr, r[2]);
        s.store(pt, addr, r[0]); 
        // load the value we just stored to avoid having to spill it
        s.vars[ptrVar] = s.load(pt, addr);
        s.vars[lenVar] = r[1]; // avoid a spill in the fast path
    }
    else
 {
        s.vars[ptrVar] = r[0];
        s.vars[newlenVar] = s.newValue2(s.ssaOp(ir.OADD, types.Types[types.TINT]), types.Types[types.TINT], r[1], s.constInt(types.Types[types.TINT], nargs));
        s.vars[capVar] = r[2];
    }
    b = s.endBlock();
    b.AddEdgeTo(assign); 

    // assign new elements to slots
    s.startBlock(assign);

    if (inplace) {
        l = s.variable(lenVar, types.Types[types.TINT]); // generates phi for len
        nl = s.newValue2(s.ssaOp(ir.OADD, types.Types[types.TINT]), types.Types[types.TINT], l, s.constInt(types.Types[types.TINT], nargs));
        var lenaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, types.SliceLenOffset, addr);
        s.store(types.Types[types.TINT], lenaddr, nl);
    }
    private partial struct argRec {
        public ptr<ssa.Value> v;
        public bool store;
    }
    var args = make_slice<argRec>(0, nargs);
    foreach (var (_, n) in n.Args[(int)1..]) {
        if (TypeOK(_addr_n.Type())) {
            args = append(args, new argRec(v:s.expr(n),store:true));
        }
        else
 {
            var v = s.addr(n);
            args = append(args, new argRec(v:v));
        }
    }    p = s.variable(ptrVar, pt); // generates phi for ptr
    if (!inplace) {
        nl = s.variable(newlenVar, types.Types[types.TINT]); // generates phi for nl
        c = s.variable(capVar, types.Types[types.TINT]); // generates phi for cap
    }
    var p2 = s.newValue2(ssa.OpPtrIndex, pt, p, l);
    foreach (var (i, arg) in args) {
        addr = s.newValue2(ssa.OpPtrIndex, pt, p2, s.constInt(types.Types[types.TINT], int64(i)));
        if (arg.store) {
            s.storeType(et, addr, arg.v, 0, true);
        }
        else
 {
            s.move(et, addr, arg.v);
        }
    }    delete(s.vars, ptrVar);
    if (inplace) {
        delete(s.vars, lenVar);
        return _addr_null!;
    }
    delete(s.vars, newlenVar);
    delete(s.vars, capVar); 
    // make result
    return _addr_s.newValue3(ssa.OpSliceMake, n.Type(), p, nl, c)!;
}

// condBranch evaluates the boolean expression cond and branches to yes
// if cond is true and no if cond is false.
// This function is intended to handle && and || better than just calling
// s.expr(cond) and branching on the result.
private static void condBranch(this ptr<state> _addr_s, ir.Node cond, ptr<ssa.Block> _addr_yes, ptr<ssa.Block> _addr_no, sbyte likely) {
    ref state s = ref _addr_s.val;
    ref ssa.Block yes = ref _addr_yes.val;
    ref ssa.Block no = ref _addr_no.val;


    if (cond.Op() == ir.OANDAND) 
        ptr<ir.LogicalExpr> cond = cond._<ptr<ir.LogicalExpr>>();
        var mid = s.f.NewBlock(ssa.BlockPlain);
        s.stmtList(cond.Init());
        s.condBranch(cond.X, mid, no, max8(likely, 0));
        s.startBlock(mid);
        s.condBranch(cond.Y, yes, no, likely);
        return ; 
        // Note: if likely==1, then both recursive calls pass 1.
        // If likely==-1, then we don't have enough information to decide
        // whether the first branch is likely or not. So we pass 0 for
        // the likeliness of the first branch.
        // TODO: have the frontend give us branch prediction hints for
        // OANDAND and OOROR nodes (if it ever has such info).
    else if (cond.Op() == ir.OOROR) 
        cond = cond._<ptr<ir.LogicalExpr>>();
        mid = s.f.NewBlock(ssa.BlockPlain);
        s.stmtList(cond.Init());
        s.condBranch(cond.X, yes, mid, min8(likely, 0));
        s.startBlock(mid);
        s.condBranch(cond.Y, yes, no, likely);
        return ; 
        // Note: if likely==-1, then both recursive calls pass -1.
        // If likely==1, then we don't have enough info to decide
        // the likelihood of the first branch.
    else if (cond.Op() == ir.ONOT) 
        cond = cond._<ptr<ir.UnaryExpr>>();
        s.stmtList(cond.Init());
        s.condBranch(cond.X, no, yes, -likely);
        return ;
    else if (cond.Op() == ir.OCONVNOP) 
        cond = cond._<ptr<ir.ConvExpr>>();
        s.stmtList(cond.Init());
        s.condBranch(cond.X, yes, no, likely);
        return ;
        var c = s.expr(cond);
    var b = s.endBlock();
    b.Kind = ssa.BlockIf;
    b.SetControl(c);
    b.Likely = ssa.BranchPrediction(likely); // gc and ssa both use -1/0/+1 for likeliness
    b.AddEdgeTo(yes);
    b.AddEdgeTo(no);
}

private partial struct skipMask { // : byte
}

private static readonly skipMask skipPtr = 1 << (int)(iota);
private static readonly var skipLen = 0;
private static readonly var skipCap = 1;

// assign does left = right.
// Right has already been evaluated to ssa, left has not.
// If deref is true, then we do left = *right instead (and right has already been nil-checked).
// If deref is true and right == nil, just do left = 0.
// skip indicates assignments (at the top level) that can be avoided.
private static void assign(this ptr<state> _addr_s, ir.Node left, ptr<ssa.Value> _addr_right, bool deref, skipMask skip) => func((defer, _, _) => {
    ref state s = ref _addr_s.val;
    ref ssa.Value right = ref _addr_right.val;

    if (left.Op() == ir.ONAME && ir.IsBlank(left)) {
        return ;
    }
    var t = left.Type();
    types.CalcSize(t);
    if (s.canSSA(left)) {
        if (deref) {
            s.Fatalf("can SSA LHS %v but not RHS %s", left, right);
        }
        if (left.Op() == ir.ODOT) { 
            // We're assigning to a field of an ssa-able value.
            // We need to build a new structure with the new value for the
            // field we're assigning and the old values for the other fields.
            // For instance:
            //   type T struct {a, b, c int}
            //   var T x
            //   x.b = 5
            // For the x.b = 5 assignment we want to generate x = T{x.a, 5, x.c}

            // Grab information about the structure type.
            ptr<ir.SelectorExpr> left = left._<ptr<ir.SelectorExpr>>();
            t = left.X.Type();
            var nf = t.NumFields();
            var idx = fieldIdx(left); 

            // Grab old value of structure.
            var old = s.expr(left.X); 

            // Make new structure.
            var @new = s.newValue0(ssa.StructMakeOp(t.NumFields()), t); 

            // Add fields as args.
            {
                nint i__prev1 = i;

                for (nint i = 0; i < nf; i++) {
                    if (i == idx) {
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
            s.assign(left.X, new, false, 0); 
            // TODO: do we need to update named values here?
            return ;
        }
        if (left.Op() == ir.OINDEX && left._<ptr<ir.IndexExpr>>().X.Type().IsArray()) {
            left = left._<ptr<ir.IndexExpr>>();
            s.pushLine(left.Pos());
            defer(s.popLine()); 
            // We're assigning to an element of an ssa-able array.
            // a[i] = v
            t = left.X.Type();
            var n = t.NumElem();

            i = s.expr(left.Index); // index
            if (n == 0) { 
                // The bounds check must fail.  Might as well
                // ignore the actual index and just use zeros.
                var z = s.constInt(types.Types[types.TINT], 0);
                s.boundsCheck(z, z, ssa.BoundsIndex, false);
                return ;
            }
            if (n != 1) {
                s.Fatalf("assigning to non-1-length array");
            } 
            // Rewrite to a = [1]{v}
            var len = s.constInt(types.Types[types.TINT], 1);
            s.boundsCheck(i, len, ssa.BoundsIndex, false); // checks i == 0
            var v = s.newValue1(ssa.OpArrayMake1, t, right);
            s.assign(left.X, v, false, 0);
            return ;
        }
        left = left._<ptr<ir.Name>>(); 
        // Update variable assignment.
        s.vars[left] = right;
        s.addNamedValue(left, right);
        return ;
    }
    {
        ptr<ir.Name> (base, ok) = clobberBase(left)._<ptr<ir.Name>>();

        if (ok && @base.OnStack() && skip == 0) {
            s.vars[memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, base, s.mem(), !ir.IsAutoTmp(base));
        }
    } 

    // Left is not ssa-able. Compute its address.
    var addr = s.addr(left);
    if (ir.IsReflectHeaderDataField(left)) { 
        // Package unsafe's documentation says storing pointers into
        // reflect.SliceHeader and reflect.StringHeader's Data fields
        // is valid, even though they have type uintptr (#19168).
        // Mark it pointer type to signal the writebarrier pass to
        // insert a write barrier.
        t = types.Types[types.TUNSAFEPTR];
    }
    if (deref) { 
        // Treat as a mem->mem move.
        if (right == null) {
            s.zero(t, addr);
        }
        else
 {
            s.move(t, addr, right);
        }
        return ;
    }
    s.storeType(t, addr, right, skip, !ir.IsAutoTmp(left));
});

// zeroVal returns the zero value for type t.
private static ptr<ssa.Value> zeroVal(this ptr<state> _addr_s, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;


    if (t.IsInteger()) 
        switch (t.Size()) {
            case 1: 
                return _addr_s.constInt8(t, 0)!;
                break;
            case 2: 
                return _addr_s.constInt16(t, 0)!;
                break;
            case 4: 
                return _addr_s.constInt32(t, 0)!;
                break;
            case 8: 
                return _addr_s.constInt64(t, 0)!;
                break;
            default: 
                s.Fatalf("bad sized integer type %v", t);
                break;
        }
    else if (t.IsFloat()) 
        switch (t.Size()) {
            case 4: 
                return _addr_s.constFloat32(t, 0)!;
                break;
            case 8: 
                return _addr_s.constFloat64(t, 0)!;
                break;
            default: 
                s.Fatalf("bad sized float type %v", t);
                break;
        }
    else if (t.IsComplex()) 
        switch (t.Size()) {
            case 8: 
                var z = s.constFloat32(types.Types[types.TFLOAT32], 0);
                return _addr_s.entryNewValue2(ssa.OpComplexMake, t, z, z)!;
                break;
            case 16: 
                z = s.constFloat64(types.Types[types.TFLOAT64], 0);
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
        for (nint i = 0; i < n; i++) {
            v.AddArg(s.zeroVal(t.FieldType(i)));
        }
        return _addr_v!;
    else if (t.IsArray()) 
        switch (t.NumElem()) {
            case 0: 
                return _addr_s.entryNewValue0(ssa.OpArrayMake0, t)!;
                break;
            case 1: 
                return _addr_s.entryNewValue1(ssa.OpArrayMake1, t, s.zeroVal(t.Elem()))!;
                break;
        }
        s.Fatalf("zero for type %v not implemented", t);
    return _addr_null!;
}

private partial struct callKind { // : sbyte
}

private static readonly callKind callNormal = iota;
private static readonly var callDefer = 0;
private static readonly var callDeferStack = 1;
private static readonly var callGo = 2;

private partial struct sfRtCallDef {
    public ptr<obj.LSym> rtfn;
    public types.Kind rtype;
}

private static map<ssa.Op, sfRtCallDef> softFloatOps = default;

private static void softfloatInit() { 
    // Some of these operations get transformed by sfcall.
    softFloatOps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, sfRtCallDef>{ssa.OpAdd32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fadd32"),types.TFLOAT32},ssa.OpAdd64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fadd64"),types.TFLOAT64},ssa.OpSub32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fadd32"),types.TFLOAT32},ssa.OpSub64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fadd64"),types.TFLOAT64},ssa.OpMul32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fmul32"),types.TFLOAT32},ssa.OpMul64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fmul64"),types.TFLOAT64},ssa.OpDiv32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fdiv32"),types.TFLOAT32},ssa.OpDiv64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fdiv64"),types.TFLOAT64},ssa.OpEq64F:sfRtCallDef{typecheck.LookupRuntimeFunc("feq64"),types.TBOOL},ssa.OpEq32F:sfRtCallDef{typecheck.LookupRuntimeFunc("feq32"),types.TBOOL},ssa.OpNeq64F:sfRtCallDef{typecheck.LookupRuntimeFunc("feq64"),types.TBOOL},ssa.OpNeq32F:sfRtCallDef{typecheck.LookupRuntimeFunc("feq32"),types.TBOOL},ssa.OpLess64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fgt64"),types.TBOOL},ssa.OpLess32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fgt32"),types.TBOOL},ssa.OpLeq64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fge64"),types.TBOOL},ssa.OpLeq32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fge32"),types.TBOOL},ssa.OpCvt32to32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fint32to32"),types.TFLOAT32},ssa.OpCvt32Fto32:sfRtCallDef{typecheck.LookupRuntimeFunc("f32toint32"),types.TINT32},ssa.OpCvt64to32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fint64to32"),types.TFLOAT32},ssa.OpCvt32Fto64:sfRtCallDef{typecheck.LookupRuntimeFunc("f32toint64"),types.TINT64},ssa.OpCvt64Uto32F:sfRtCallDef{typecheck.LookupRuntimeFunc("fuint64to32"),types.TFLOAT32},ssa.OpCvt32Fto64U:sfRtCallDef{typecheck.LookupRuntimeFunc("f32touint64"),types.TUINT64},ssa.OpCvt32to64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fint32to64"),types.TFLOAT64},ssa.OpCvt64Fto32:sfRtCallDef{typecheck.LookupRuntimeFunc("f64toint32"),types.TINT32},ssa.OpCvt64to64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fint64to64"),types.TFLOAT64},ssa.OpCvt64Fto64:sfRtCallDef{typecheck.LookupRuntimeFunc("f64toint64"),types.TINT64},ssa.OpCvt64Uto64F:sfRtCallDef{typecheck.LookupRuntimeFunc("fuint64to64"),types.TFLOAT64},ssa.OpCvt64Fto64U:sfRtCallDef{typecheck.LookupRuntimeFunc("f64touint64"),types.TUINT64},ssa.OpCvt32Fto64F:sfRtCallDef{typecheck.LookupRuntimeFunc("f32to64"),types.TFLOAT64},ssa.OpCvt64Fto32F:sfRtCallDef{typecheck.LookupRuntimeFunc("f64to32"),types.TFLOAT32},};
}

// TODO: do not emit sfcall if operation can be optimized to constant in later
// opt phase
private static (ptr<ssa.Value>, bool) sfcall(this ptr<state> _addr_s, ssa.Op op, params ptr<ptr<ssa.Value>>[] _addr_args) {
    ptr<ssa.Value> _p0 = default!;
    bool _p0 = default;
    args = args.Clone();
    ref state s = ref _addr_s.val;
    ref ssa.Value args = ref _addr_args.val;

    {
        var (callDef, ok) = softFloatOps[op];

        if (ok) {

            if (op == ssa.OpLess32F || op == ssa.OpLess64F || op == ssa.OpLeq32F || op == ssa.OpLeq64F) 
                (args[0], args[1]) = (args[1], args[0]);            else if (op == ssa.OpSub32F || op == ssa.OpSub64F) 
                args[1] = s.newValue1(s.ssaOp(ir.ONEG, types.Types[callDef.rtype]), args[1].Type, args[1]);
                        var result = s.rtcall(callDef.rtfn, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { types.Types[callDef.rtype] }), args)[0];
            if (op == ssa.OpNeq32F || op == ssa.OpNeq64F) {
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
public delegate  ptr<ssa.Value> intrinsicBuilder(ptr<state>,  ptr<ir.CallExpr>,  slice<ptr<ssa.Value>>);

private partial struct intrinsicKey {
    public ptr<sys.Arch> arch;
    public @string pkg;
    public @string fn;
}

public static void InitTables() => func((_, panic, _) => {
    intrinsics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<intrinsicKey, intrinsicBuilder>{};

    slice<ptr<sys.Arch>> all = default;
    slice<ptr<sys.Arch>> p4 = default;
    slice<ptr<sys.Arch>> p8 = default;
    slice<ptr<sys.Arch>> lwatomics = default;
    {
        var a__prev1 = a;

        foreach (var (_, __a) in _addr_sys.Archs) {
            a = __a;
            all = append(all, a);
            if (a.PtrSize == 4) {
                p4 = append(p4, a);
            }
            else
 {
                p8 = append(p8, a);
            }
            if (a.Family != sys.PPC64) {
                lwatomics = append(lwatomics, a);
            }
        }
        a = a__prev1;
    }

    Action<@string, @string, intrinsicBuilder, ptr<ptr<sys.Arch>>[]> add = (pkg, fn, b, archs) => {
        {
            var a__prev1 = a;

            foreach (var (_, __a) in archs) {
                a = __a;
                intrinsics[new intrinsicKey(a,pkg,fn)] = b;
            }

            a = a__prev1;
        }
    }; 
    // addF does the same as add but operates on architecture families.
    Action<@string, @string, intrinsicBuilder, sys.ArchFamily[]> addF = (pkg, fn, b, archFamilies) => {
        nint m = 0;
        foreach (var (_, f) in archFamilies) {
            if (f >= 32) {
                panic("too many architecture families");
            }
            m |= 1 << (int)(uint(f));
        }        {
            var a__prev1 = a;

            foreach (var (_, __a) in all) {
                a = __a;
                if (m >> (int)(uint(a.Family)) & 1 != 0) {
                    intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                }
            }

            a = a__prev1;
        }
    }; 
    // alias defines pkg.fn = pkg2.fn2 for all architectures in archs for which pkg2.fn2 exists.
    Action<@string, @string, @string, @string, ptr<ptr<sys.Arch>>[]> alias = (pkg, fn, pkg2, fn2, archs) => {
        var aliased = false;
        {
            var a__prev1 = a;

            foreach (var (_, __a) in archs) {
                a = __a;
                {
                    var b__prev1 = b;

                    var (b, ok) = intrinsics[new intrinsicKey(a,pkg2,fn2)];

                    if (ok) {
                        intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                        aliased = true;
                    }

                    b = b__prev1;

                }
            }

            a = a__prev1;
        }

        if (!aliased) {
            panic(fmt.Sprintf("attempted to alias undefined intrinsic: %s.%s", pkg, fn));
        }
    }; 

    /******** runtime ********/
    if (!@base.Flag.Cfg.Instrumenting) {
        add("runtime", "slicebytetostringtmp", (s, n, args) => { 
            // Compiler frontend optimizations emit OBYTES2STRTMP nodes
            // for the backend instead of slicebytetostringtmp calls
            // when not instrumenting.
            return s.newValue2(ssa.OpStringMake, n.Type(), args[0], args[1]);
        }, all);
    }
    addF("runtime/internal/math", "MulUintptr", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            return s.newValue2(ssa.OpMul32uover, types.NewTuple(types.Types[types.TUINT], types.Types[types.TUINT]), args[0], args[1]);
        }
        return s.newValue2(ssa.OpMul64uover, types.NewTuple(types.Types[types.TUINT], types.Types[types.TUINT]), args[0], args[1]);
    }, sys.AMD64, sys.I386, sys.MIPS64);
    add("runtime", "KeepAlive", (s, n, args) => {
        var data = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, args[0]);
        s.vars[memVar] = s.newValue2(ssa.OpKeepAlive, types.TypeMem, data, s.mem());
        return null;
    }, all);
    add("runtime", "getclosureptr", (s, n, args) => s.newValue0(ssa.OpGetClosurePtr, s.f.Config.Types.Uintptr), all);

    add("runtime", "getcallerpc", (s, n, args) => s.newValue0(ssa.OpGetCallerPC, s.f.Config.Types.Uintptr), all);

    add("runtime", "getcallersp", (s, n, args) => s.newValue0(ssa.OpGetCallerSP, s.f.Config.Types.Uintptr), all); 

    /******** runtime/internal/sys ********/
    addF("runtime/internal/sys", "Ctz32", (s, n, args) => s.newValue1(ssa.OpCtz32, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
    addF("runtime/internal/sys", "Ctz64", (s, n, args) => s.newValue1(ssa.OpCtz64, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
    addF("runtime/internal/sys", "Bswap32", (s, n, args) => s.newValue1(ssa.OpBswap32, types.Types[types.TUINT32], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X);
    addF("runtime/internal/sys", "Bswap64", (s, n, args) => s.newValue1(ssa.OpBswap64, types.Types[types.TUINT64], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X); 

    /******** runtime/internal/atomic ********/
    addF("runtime/internal/atomic", "Load", (s, n, args) => {
        var v = s.newValue2(ssa.OpAtomicLoad32, types.NewTuple(types.Types[types.TUINT32], types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT32], v);
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Load8", (s, n, args) => {
        v = s.newValue2(ssa.OpAtomicLoad8, types.NewTuple(types.Types[types.TUINT8], types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT8], v);
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Load64", (s, n, args) => {
        v = s.newValue2(ssa.OpAtomicLoad64, types.NewTuple(types.Types[types.TUINT64], types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT64], v);
    }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "LoadAcq", (s, n, args) => {
        v = s.newValue2(ssa.OpAtomicLoadAcq32, types.NewTuple(types.Types[types.TUINT32], types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT32], v);
    }, sys.PPC64, sys.S390X);
    addF("runtime/internal/atomic", "LoadAcq64", (s, n, args) => {
        v = s.newValue2(ssa.OpAtomicLoadAcq64, types.NewTuple(types.Types[types.TUINT64], types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT64], v);
    }, sys.PPC64);
    addF("runtime/internal/atomic", "Loadp", (s, n, args) => {
        v = s.newValue2(ssa.OpAtomicLoadPtr, types.NewTuple(s.f.Config.Types.BytePtr, types.TypeMem), args[0], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, s.f.Config.Types.BytePtr, v);
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

    addF("runtime/internal/atomic", "Store", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStore32, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Store8", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStore8, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Store64", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStore64, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.ARM64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "StorepNoWB", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStorePtrNoWB, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.MIPS64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "StoreRel", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStoreRel32, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.PPC64, sys.S390X);
    addF("runtime/internal/atomic", "StoreRel64", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicStoreRel64, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.PPC64);

    addF("runtime/internal/atomic", "Xchg", (s, n, args) => {
        v = s.newValue3(ssa.OpAtomicExchange32, types.NewTuple(types.Types[types.TUINT32], types.TypeMem), args[0], args[1], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT32], v);
    }, sys.AMD64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Xchg64", (s, n, args) => {
        v = s.newValue3(ssa.OpAtomicExchange64, types.NewTuple(types.Types[types.TUINT64], types.TypeMem), args[0], args[1], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT64], v);
    }, sys.AMD64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

    public delegate void atomicOpEmitter(ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ssa.Op, types.Kind);

    Func<ssa.Op, ssa.Op, types.Kind, types.Kind, atomicOpEmitter, intrinsicBuilder> makeAtomicGuardedIntrinsicARM64 = (op0, op1, typ, rtyp, emit) => (s, n, args) => { 
            // Target Atomic feature is identified by dynamic detection
            var addr = s.entryNewValue1A(ssa.OpAddr, types.Types[types.TBOOL].PtrTo(), ir.Syms.ARM64HasATOMICS, s.sb);
            v = s.load(types.Types[types.TBOOL], addr);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(v);
            var bTrue = s.f.NewBlock(ssa.BlockPlain);
            var bFalse = s.f.NewBlock(ssa.BlockPlain);
            var bEnd = s.f.NewBlock(ssa.BlockPlain);
            b.AddEdgeTo(bTrue);
            b.AddEdgeTo(bFalse);
            b.Likely = ssa.BranchLikely; 

            // We have atomic instructions - use it directly.
            s.startBlock(bTrue);
            emit(s, n, args, op1, typ);
            s.endBlock().AddEdgeTo(bEnd); 

            // Use original instruction sequence.
            s.startBlock(bFalse);
            emit(s, n, args, op0, typ);
            s.endBlock().AddEdgeTo(bEnd); 

            // Merge results.
            s.startBlock(bEnd);
            if (rtyp == types.TNIL) {
                return null;
            }
            else
 {
                return s.variable(n, types.Types[rtyp]);
            }
        };

    Action<ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ssa.Op, types.Kind> atomicXchgXaddEmitterARM64 = (s, n, args, op, typ) => {
        v = s.newValue3(op, types.NewTuple(types.Types[typ], types.TypeMem), args[0], args[1], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        s.vars[n] = s.newValue1(ssa.OpSelect0, types.Types[typ], v);
    };
    addF("runtime/internal/atomic", "Xchg", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicExchange32, ssa.OpAtomicExchange32Variant, types.TUINT32, types.TUINT32, atomicXchgXaddEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "Xchg64", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicExchange64, ssa.OpAtomicExchange64Variant, types.TUINT64, types.TUINT64, atomicXchgXaddEmitterARM64), sys.ARM64);

    addF("runtime/internal/atomic", "Xadd", (s, n, args) => {
        v = s.newValue3(ssa.OpAtomicAdd32, types.NewTuple(types.Types[types.TUINT32], types.TypeMem), args[0], args[1], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT32], v);
    }, sys.AMD64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Xadd64", (s, n, args) => {
        v = s.newValue3(ssa.OpAtomicAdd64, types.NewTuple(types.Types[types.TUINT64], types.TypeMem), args[0], args[1], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TUINT64], v);
    }, sys.AMD64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);

    addF("runtime/internal/atomic", "Xadd", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicAdd32, ssa.OpAtomicAdd32Variant, types.TUINT32, types.TUINT32, atomicXchgXaddEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "Xadd64", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicAdd64, ssa.OpAtomicAdd64Variant, types.TUINT64, types.TUINT64, atomicXchgXaddEmitterARM64), sys.ARM64);

    addF("runtime/internal/atomic", "Cas", (s, n, args) => {
        v = s.newValue4(ssa.OpAtomicCompareAndSwap32, types.NewTuple(types.Types[types.TBOOL], types.TypeMem), args[0], args[1], args[2], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TBOOL], v);
    }, sys.AMD64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Cas64", (s, n, args) => {
        v = s.newValue4(ssa.OpAtomicCompareAndSwap64, types.NewTuple(types.Types[types.TBOOL], types.TypeMem), args[0], args[1], args[2], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TBOOL], v);
    }, sys.AMD64, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "CasRel", (s, n, args) => {
        v = s.newValue4(ssa.OpAtomicCompareAndSwap32, types.NewTuple(types.Types[types.TBOOL], types.TypeMem), args[0], args[1], args[2], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        return s.newValue1(ssa.OpSelect0, types.Types[types.TBOOL], v);
    }, sys.PPC64);

    Action<ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ssa.Op, types.Kind> atomicCasEmitterARM64 = (s, n, args, op, typ) => {
        v = s.newValue4(op, types.NewTuple(types.Types[types.TBOOL], types.TypeMem), args[0], args[1], args[2], s.mem());
        s.vars[memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
        s.vars[n] = s.newValue1(ssa.OpSelect0, types.Types[typ], v);
    };

    addF("runtime/internal/atomic", "Cas", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicCompareAndSwap32, ssa.OpAtomicCompareAndSwap32Variant, types.TUINT32, types.TBOOL, atomicCasEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "Cas64", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicCompareAndSwap64, ssa.OpAtomicCompareAndSwap64Variant, types.TUINT64, types.TBOOL, atomicCasEmitterARM64), sys.ARM64);

    addF("runtime/internal/atomic", "And8", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicAnd8, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.MIPS, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "And", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicAnd32, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.MIPS, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Or8", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicOr8, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.ARM64, sys.MIPS, sys.PPC64, sys.RISCV64, sys.S390X);
    addF("runtime/internal/atomic", "Or", (s, n, args) => {
        s.vars[memVar] = s.newValue3(ssa.OpAtomicOr32, types.TypeMem, args[0], args[1], s.mem());
        return null;
    }, sys.AMD64, sys.MIPS, sys.PPC64, sys.RISCV64, sys.S390X);

    Action<ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ssa.Op, types.Kind> atomicAndOrEmitterARM64 = (s, n, args, op, typ) => {
        s.vars[memVar] = s.newValue3(op, types.TypeMem, args[0], args[1], s.mem());
    };

    addF("runtime/internal/atomic", "And8", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicAnd8, ssa.OpAtomicAnd8Variant, types.TNIL, types.TNIL, atomicAndOrEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "And", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicAnd32, ssa.OpAtomicAnd32Variant, types.TNIL, types.TNIL, atomicAndOrEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "Or8", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicOr8, ssa.OpAtomicOr8Variant, types.TNIL, types.TNIL, atomicAndOrEmitterARM64), sys.ARM64);
    addF("runtime/internal/atomic", "Or", makeAtomicGuardedIntrinsicARM64(ssa.OpAtomicOr32, ssa.OpAtomicOr32Variant, types.TNIL, types.TNIL, atomicAndOrEmitterARM64), sys.ARM64); 

    // Aliases for atomic load operations
    alias("runtime/internal/atomic", "Loadint32", "runtime/internal/atomic", "Load", all);
    alias("runtime/internal/atomic", "Loadint64", "runtime/internal/atomic", "Load64", all);
    alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load", p4);
    alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load64", p8);
    alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load", p4);
    alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load64", p8);
    alias("runtime/internal/atomic", "LoadAcq", "runtime/internal/atomic", "Load", lwatomics);
    alias("runtime/internal/atomic", "LoadAcq64", "runtime/internal/atomic", "Load64", lwatomics);
    alias("runtime/internal/atomic", "LoadAcquintptr", "runtime/internal/atomic", "LoadAcq", p4);
    alias("sync", "runtime_LoadAcquintptr", "runtime/internal/atomic", "LoadAcq", p4); // linknamed
    alias("runtime/internal/atomic", "LoadAcquintptr", "runtime/internal/atomic", "LoadAcq64", p8);
    alias("sync", "runtime_LoadAcquintptr", "runtime/internal/atomic", "LoadAcq64", p8); // linknamed

    // Aliases for atomic store operations
    alias("runtime/internal/atomic", "Storeint32", "runtime/internal/atomic", "Store", all);
    alias("runtime/internal/atomic", "Storeint64", "runtime/internal/atomic", "Store64", all);
    alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store", p4);
    alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store64", p8);
    alias("runtime/internal/atomic", "StoreRel", "runtime/internal/atomic", "Store", lwatomics);
    alias("runtime/internal/atomic", "StoreRel64", "runtime/internal/atomic", "Store64", lwatomics);
    alias("runtime/internal/atomic", "StoreReluintptr", "runtime/internal/atomic", "StoreRel", p4);
    alias("sync", "runtime_StoreReluintptr", "runtime/internal/atomic", "StoreRel", p4); // linknamed
    alias("runtime/internal/atomic", "StoreReluintptr", "runtime/internal/atomic", "StoreRel64", p8);
    alias("sync", "runtime_StoreReluintptr", "runtime/internal/atomic", "StoreRel64", p8); // linknamed

    // Aliases for atomic swap operations
    alias("runtime/internal/atomic", "Xchgint32", "runtime/internal/atomic", "Xchg", all);
    alias("runtime/internal/atomic", "Xchgint64", "runtime/internal/atomic", "Xchg64", all);
    alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg", p4);
    alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg64", p8); 

    // Aliases for atomic add operations
    alias("runtime/internal/atomic", "Xaddint32", "runtime/internal/atomic", "Xadd", all);
    alias("runtime/internal/atomic", "Xaddint64", "runtime/internal/atomic", "Xadd64", all);
    alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd", p4);
    alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd64", p8); 

    // Aliases for atomic CAS operations
    alias("runtime/internal/atomic", "Casint32", "runtime/internal/atomic", "Cas", all);
    alias("runtime/internal/atomic", "Casint64", "runtime/internal/atomic", "Cas64", all);
    alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas", p4);
    alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas64", p8);
    alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas", p4);
    alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas64", p8);
    alias("runtime/internal/atomic", "CasRel", "runtime/internal/atomic", "Cas", lwatomics); 

    /******** math ********/
    addF("math", "Sqrt", (s, n, args) => s.newValue1(ssa.OpSqrt, types.Types[types.TFLOAT64], args[0]), sys.I386, sys.AMD64, sys.ARM, sys.ARM64, sys.MIPS, sys.MIPS64, sys.PPC64, sys.RISCV64, sys.S390X, sys.Wasm);
    addF("math", "Trunc", (s, n, args) => s.newValue1(ssa.OpTrunc, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
    addF("math", "Ceil", (s, n, args) => s.newValue1(ssa.OpCeil, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
    addF("math", "Floor", (s, n, args) => s.newValue1(ssa.OpFloor, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.PPC64, sys.S390X, sys.Wasm);
    addF("math", "Round", (s, n, args) => s.newValue1(ssa.OpRound, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.PPC64, sys.S390X);
    addF("math", "RoundToEven", (s, n, args) => s.newValue1(ssa.OpRoundToEven, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.S390X, sys.Wasm);
    addF("math", "Abs", (s, n, args) => s.newValue1(ssa.OpAbs, types.Types[types.TFLOAT64], args[0]), sys.ARM64, sys.ARM, sys.PPC64, sys.Wasm);
    addF("math", "Copysign", (s, n, args) => s.newValue2(ssa.OpCopysign, types.Types[types.TFLOAT64], args[0], args[1]), sys.PPC64, sys.Wasm);
    addF("math", "FMA", (s, n, args) => s.newValue3(ssa.OpFMA, types.Types[types.TFLOAT64], args[0], args[1], args[2]), sys.ARM64, sys.PPC64, sys.S390X);
    addF("math", "FMA", (s, n, args) => {
        if (!s.config.UseFMA) {
            s.vars[n] = s.callResult(n, callNormal); // types.Types[TFLOAT64]
            return s.variable(n, types.Types[types.TFLOAT64]);
        }
        v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[types.TBOOL], ir.Syms.X86HasFMA);
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
        s.vars[n] = s.newValue3(ssa.OpFMA, types.Types[types.TFLOAT64], args[0], args[1], args[2]);
        s.endBlock().AddEdgeTo(bEnd); 

        // Call the pure Go version.
        s.startBlock(bFalse);
        s.vars[n] = s.callResult(n, callNormal); // types.Types[TFLOAT64]
        s.endBlock().AddEdgeTo(bEnd); 

        // Merge results.
        s.startBlock(bEnd);
        return s.variable(n, types.Types[types.TFLOAT64]);
    }, sys.AMD64);
    addF("math", "FMA", (s, n, args) => {
        if (!s.config.UseFMA) {
            s.vars[n] = s.callResult(n, callNormal); // types.Types[TFLOAT64]
            return s.variable(n, types.Types[types.TFLOAT64]);
        }
        addr = s.entryNewValue1A(ssa.OpAddr, types.Types[types.TBOOL].PtrTo(), ir.Syms.ARMHasVFPv4, s.sb);
        v = s.load(types.Types[types.TBOOL], addr);
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
        s.vars[n] = s.newValue3(ssa.OpFMA, types.Types[types.TFLOAT64], args[0], args[1], args[2]);
        s.endBlock().AddEdgeTo(bEnd); 

        // Call the pure Go version.
        s.startBlock(bFalse);
        s.vars[n] = s.callResult(n, callNormal); // types.Types[TFLOAT64]
        s.endBlock().AddEdgeTo(bEnd); 

        // Merge results.
        s.startBlock(bEnd);
        return s.variable(n, types.Types[types.TFLOAT64]);
    }, sys.ARM);

    Func<ssa.Op, Func<ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ptr<ssa.Value>>> makeRoundAMD64 = op => (s, n, args) => {
            v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[types.TBOOL], ir.Syms.X86HasSSE41);
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
            s.vars[n] = s.newValue1(op, types.Types[types.TFLOAT64], args[0]);
            s.endBlock().AddEdgeTo(bEnd); 

            // Call the pure Go version.
            s.startBlock(bFalse);
            s.vars[n] = s.callResult(n, callNormal); // types.Types[TFLOAT64]
            s.endBlock().AddEdgeTo(bEnd); 

            // Merge results.
            s.startBlock(bEnd);
            return s.variable(n, types.Types[types.TFLOAT64]);
        };
    addF("math", "RoundToEven", makeRoundAMD64(ssa.OpRoundToEven), sys.AMD64);
    addF("math", "Floor", makeRoundAMD64(ssa.OpFloor), sys.AMD64);
    addF("math", "Ceil", makeRoundAMD64(ssa.OpCeil), sys.AMD64);
    addF("math", "Trunc", makeRoundAMD64(ssa.OpTrunc), sys.AMD64); 

    /******** math/bits ********/
    addF("math/bits", "TrailingZeros64", (s, n, args) => s.newValue1(ssa.OpCtz64, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "TrailingZeros32", (s, n, args) => s.newValue1(ssa.OpCtz32, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "TrailingZeros16", (s, n, args) => {
        var x = s.newValue1(ssa.OpZeroExt16to32, types.Types[types.TUINT32], args[0]);
        var c = s.constInt32(types.Types[types.TUINT32], 1 << 16);
        var y = s.newValue2(ssa.OpOr32, types.Types[types.TUINT32], x, c);
        return s.newValue1(ssa.OpCtz32, types.Types[types.TINT], y);
    }, sys.MIPS);
    addF("math/bits", "TrailingZeros16", (s, n, args) => s.newValue1(ssa.OpCtz16, types.Types[types.TINT], args[0]), sys.AMD64, sys.I386, sys.ARM, sys.ARM64, sys.Wasm);
    addF("math/bits", "TrailingZeros16", (s, n, args) => {
        x = s.newValue1(ssa.OpZeroExt16to64, types.Types[types.TUINT64], args[0]);
        c = s.constInt64(types.Types[types.TUINT64], 1 << 16);
        y = s.newValue2(ssa.OpOr64, types.Types[types.TUINT64], x, c);
        return s.newValue1(ssa.OpCtz64, types.Types[types.TINT], y);
    }, sys.S390X, sys.PPC64);
    addF("math/bits", "TrailingZeros8", (s, n, args) => {
        x = s.newValue1(ssa.OpZeroExt8to32, types.Types[types.TUINT32], args[0]);
        c = s.constInt32(types.Types[types.TUINT32], 1 << 8);
        y = s.newValue2(ssa.OpOr32, types.Types[types.TUINT32], x, c);
        return s.newValue1(ssa.OpCtz32, types.Types[types.TINT], y);
    }, sys.MIPS);
    addF("math/bits", "TrailingZeros8", (s, n, args) => s.newValue1(ssa.OpCtz8, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM, sys.ARM64, sys.Wasm);
    addF("math/bits", "TrailingZeros8", (s, n, args) => {
        x = s.newValue1(ssa.OpZeroExt8to64, types.Types[types.TUINT64], args[0]);
        c = s.constInt64(types.Types[types.TUINT64], 1 << 8);
        y = s.newValue2(ssa.OpOr64, types.Types[types.TUINT64], x, c);
        return s.newValue1(ssa.OpCtz64, types.Types[types.TINT], y);
    }, sys.S390X);
    alias("math/bits", "ReverseBytes64", "runtime/internal/sys", "Bswap64", all);
    alias("math/bits", "ReverseBytes32", "runtime/internal/sys", "Bswap32", all); 
    // ReverseBytes inlines correctly, no need to intrinsify it.
    // ReverseBytes16 lowers to a rotate, no need for anything special here.
    addF("math/bits", "Len64", (s, n, args) => s.newValue1(ssa.OpBitLen64, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "Len32", (s, n, args) => s.newValue1(ssa.OpBitLen32, types.Types[types.TINT], args[0]), sys.AMD64, sys.ARM64);
    addF("math/bits", "Len32", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            return s.newValue1(ssa.OpBitLen32, types.Types[types.TINT], args[0]);
        }
        x = s.newValue1(ssa.OpZeroExt32to64, types.Types[types.TUINT64], args[0]);
        return s.newValue1(ssa.OpBitLen64, types.Types[types.TINT], x);
    }, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "Len16", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            x = s.newValue1(ssa.OpZeroExt16to32, types.Types[types.TUINT32], args[0]);
            return s.newValue1(ssa.OpBitLen32, types.Types[types.TINT], x);
        }
        x = s.newValue1(ssa.OpZeroExt16to64, types.Types[types.TUINT64], args[0]);
        return s.newValue1(ssa.OpBitLen64, types.Types[types.TINT], x);
    }, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "Len16", (s, n, args) => s.newValue1(ssa.OpBitLen16, types.Types[types.TINT], args[0]), sys.AMD64);
    addF("math/bits", "Len8", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            x = s.newValue1(ssa.OpZeroExt8to32, types.Types[types.TUINT32], args[0]);
            return s.newValue1(ssa.OpBitLen32, types.Types[types.TINT], x);
        }
        x = s.newValue1(ssa.OpZeroExt8to64, types.Types[types.TUINT64], args[0]);
        return s.newValue1(ssa.OpBitLen64, types.Types[types.TINT], x);
    }, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm);
    addF("math/bits", "Len8", (s, n, args) => s.newValue1(ssa.OpBitLen8, types.Types[types.TINT], args[0]), sys.AMD64);
    addF("math/bits", "Len", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            return s.newValue1(ssa.OpBitLen32, types.Types[types.TINT], args[0]);
        }
        return s.newValue1(ssa.OpBitLen64, types.Types[types.TINT], args[0]);
    }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64, sys.Wasm); 
    // LeadingZeros is handled because it trivially calls Len.
    addF("math/bits", "Reverse64", (s, n, args) => s.newValue1(ssa.OpBitRev64, types.Types[types.TINT], args[0]), sys.ARM64);
    addF("math/bits", "Reverse32", (s, n, args) => s.newValue1(ssa.OpBitRev32, types.Types[types.TINT], args[0]), sys.ARM64);
    addF("math/bits", "Reverse16", (s, n, args) => s.newValue1(ssa.OpBitRev16, types.Types[types.TINT], args[0]), sys.ARM64);
    addF("math/bits", "Reverse8", (s, n, args) => s.newValue1(ssa.OpBitRev8, types.Types[types.TINT], args[0]), sys.ARM64);
    addF("math/bits", "Reverse", (s, n, args) => {
        if (s.config.PtrSize == 4) {
            return s.newValue1(ssa.OpBitRev32, types.Types[types.TINT], args[0]);
        }
        return s.newValue1(ssa.OpBitRev64, types.Types[types.TINT], args[0]);
    }, sys.ARM64);
    addF("math/bits", "RotateLeft8", (s, n, args) => s.newValue2(ssa.OpRotateLeft8, types.Types[types.TUINT8], args[0], args[1]), sys.AMD64);
    addF("math/bits", "RotateLeft16", (s, n, args) => s.newValue2(ssa.OpRotateLeft16, types.Types[types.TUINT16], args[0], args[1]), sys.AMD64);
    addF("math/bits", "RotateLeft32", (s, n, args) => s.newValue2(ssa.OpRotateLeft32, types.Types[types.TUINT32], args[0], args[1]), sys.AMD64, sys.ARM, sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
    addF("math/bits", "RotateLeft64", (s, n, args) => s.newValue2(ssa.OpRotateLeft64, types.Types[types.TUINT64], args[0], args[1]), sys.AMD64, sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
    alias("math/bits", "RotateLeft", "math/bits", "RotateLeft64", p8);

    Func<ssa.Op, ssa.Op, Func<ptr<state>, ptr<ir.CallExpr>, slice<ptr<ssa.Value>>, ptr<ssa.Value>>> makeOnesCountAMD64 = (op64, op32) => (s, n, args) => {
            v = s.entryNewValue0A(ssa.OpHasCPUFeature, types.Types[types.TBOOL], ir.Syms.X86HasPOPCNT);
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
            if (s.config.PtrSize == 4) {
                op = op32;
            }
            s.vars[n] = s.newValue1(op, types.Types[types.TINT], args[0]);
            s.endBlock().AddEdgeTo(bEnd); 

            // Call the pure Go version.
            s.startBlock(bFalse);
            s.vars[n] = s.callResult(n, callNormal); // types.Types[TINT]
            s.endBlock().AddEdgeTo(bEnd); 

            // Merge results.
            s.startBlock(bEnd);
            return s.variable(n, types.Types[types.TINT]);
        };
    addF("math/bits", "OnesCount64", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount64), sys.AMD64);
    addF("math/bits", "OnesCount64", (s, n, args) => s.newValue1(ssa.OpPopCount64, types.Types[types.TINT], args[0]), sys.PPC64, sys.ARM64, sys.S390X, sys.Wasm);
    addF("math/bits", "OnesCount32", makeOnesCountAMD64(ssa.OpPopCount32, ssa.OpPopCount32), sys.AMD64);
    addF("math/bits", "OnesCount32", (s, n, args) => s.newValue1(ssa.OpPopCount32, types.Types[types.TINT], args[0]), sys.PPC64, sys.ARM64, sys.S390X, sys.Wasm);
    addF("math/bits", "OnesCount16", makeOnesCountAMD64(ssa.OpPopCount16, ssa.OpPopCount16), sys.AMD64);
    addF("math/bits", "OnesCount16", (s, n, args) => s.newValue1(ssa.OpPopCount16, types.Types[types.TINT], args[0]), sys.ARM64, sys.S390X, sys.PPC64, sys.Wasm);
    addF("math/bits", "OnesCount8", (s, n, args) => s.newValue1(ssa.OpPopCount8, types.Types[types.TINT], args[0]), sys.S390X, sys.PPC64, sys.Wasm);
    addF("math/bits", "OnesCount", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount32), sys.AMD64);
    addF("math/bits", "Mul64", (s, n, args) => s.newValue2(ssa.OpMul64uhilo, types.NewTuple(types.Types[types.TUINT64], types.Types[types.TUINT64]), args[0], args[1]), sys.AMD64, sys.ARM64, sys.PPC64, sys.S390X, sys.MIPS64);
    alias("math/bits", "Mul", "math/bits", "Mul64", sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64, sys.ArchPPC64LE, sys.ArchS390X, sys.ArchMIPS64, sys.ArchMIPS64LE);
    alias("runtime/internal/math", "Mul64", "math/bits", "Mul64", sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64, sys.ArchPPC64LE, sys.ArchS390X, sys.ArchMIPS64, sys.ArchMIPS64LE);
    addF("math/bits", "Add64", (s, n, args) => s.newValue3(ssa.OpAdd64carry, types.NewTuple(types.Types[types.TUINT64], types.Types[types.TUINT64]), args[0], args[1], args[2]), sys.AMD64, sys.ARM64, sys.PPC64, sys.S390X);
    alias("math/bits", "Add", "math/bits", "Add64", sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64, sys.ArchPPC64LE, sys.ArchS390X);
    addF("math/bits", "Sub64", (s, n, args) => s.newValue3(ssa.OpSub64borrow, types.NewTuple(types.Types[types.TUINT64], types.Types[types.TUINT64]), args[0], args[1], args[2]), sys.AMD64, sys.ARM64, sys.S390X);
    alias("math/bits", "Sub", "math/bits", "Sub64", sys.ArchAMD64, sys.ArchARM64, sys.ArchS390X);
    addF("math/bits", "Div64", (s, n, args) => { 
        // check for divide-by-zero/overflow and panic with appropriate message
        var cmpZero = s.newValue2(s.ssaOp(ir.ONE, types.Types[types.TUINT64]), types.Types[types.TBOOL], args[2], s.zeroVal(types.Types[types.TUINT64]));
        s.check(cmpZero, ir.Syms.Panicdivide);
        var cmpOverflow = s.newValue2(s.ssaOp(ir.OLT, types.Types[types.TUINT64]), types.Types[types.TBOOL], args[0], args[2]);
        s.check(cmpOverflow, ir.Syms.Panicoverflow);
        return s.newValue3(ssa.OpDiv128u, types.NewTuple(types.Types[types.TUINT64], types.Types[types.TUINT64]), args[0], args[1], args[2]);
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
    add("math/big", "mulWW", (s, n, args) => s.newValue2(ssa.OpMul64uhilo, types.NewTuple(types.Types[types.TUINT64], types.Types[types.TUINT64]), args[0], args[1]), sys.ArchAMD64, sys.ArchARM64, sys.ArchPPC64LE, sys.ArchPPC64, sys.ArchS390X);
});

// findIntrinsic returns a function which builds the SSA equivalent of the
// function identified by the symbol sym.  If sym is not an intrinsic call, returns nil.
private static intrinsicBuilder findIntrinsic(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    if (sym == null || sym.Pkg == null) {
        return null;
    }
    var pkg = sym.Pkg.Path;
    if (sym.Pkg == types.LocalPkg) {
        pkg = @base.Ctxt.Pkgpath;
    }
    if (sym.Pkg == ir.Pkgs.Runtime) {
        pkg = "runtime";
    }
    if (@base.Flag.Race && pkg == "sync/atomic") { 
        // The race detector needs to be able to intercept these calls.
        // We can't intrinsify them.
        return null;
    }
    if (Arch.SoftFloat && pkg == "math") {
        return null;
    }
    var fn = sym.Name;
    if (ssa.IntrinsicsDisable) {
        if (pkg == "runtime" && (fn == "getcallerpc" || fn == "getcallersp" || fn == "getclosureptr")) { 
            // These runtime functions don't have definitions, must be intrinsics.
        }
        else
 {
            return null;
        }
    }
    return intrinsics[new intrinsicKey(Arch.LinkArch.Arch,pkg,fn)];
}

public static bool IsIntrinsicCall(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;

    if (n == null) {
        return false;
    }
    ptr<ir.Name> (name, ok) = n.X._<ptr<ir.Name>>();
    if (!ok) {
        return false;
    }
    return findIntrinsic(_addr_name.Sym()) != null;
}

// intrinsicCall converts a call to a recognized intrinsic function into the intrinsic SSA operation.
private static ptr<ssa.Value> intrinsicCall(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    var v = findIntrinsic(_addr_n.X.Sym())(s, n, s.intrinsicArgs(n));
    if (ssa.IntrinsicsDebug > 0) {
        var x = v;
        if (x == null) {
            x = s.mem();
        }
        if (x.Op == ssa.OpSelect0 || x.Op == ssa.OpSelect1) {
            x = x.Args[0];
        }
        @base.WarnfAt(n.Pos(), "intrinsic substitution for %v with %s", n.X.Sym().Name, x.LongString());
    }
    return _addr_v!;
}

// intrinsicArgs extracts args from n, evaluates them to SSA values, and returns them.
private static slice<ptr<ssa.Value>> intrinsicArgs(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    var args = make_slice<ptr<ssa.Value>>(len(n.Args));
    foreach (var (i, n) in n.Args) {
        args[i] = s.expr(n);
    }    return args;
}

// openDeferRecord adds code to evaluate and store the args for an open-code defer
// call, and records info about the defer, so we can generate proper code on the
// exit paths. n is the sub-node of the defer node that is the actual function
// call. We will also record funcdata information on where the args are stored
// (as well as the deferBits variable), and this will enable us to run the proper
// defer calls during panics.
private static void openDeferRecord(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    slice<ptr<ssa.Value>> args = default;
    slice<ptr<ir.Name>> argNodes = default;

    if (buildcfg.Experiment.RegabiDefer && (len(n.Args) != 0 || n.Op() == ir.OCALLINTER || n.X.Type().NumResults() != 0)) {
        s.Fatalf("defer call with arguments or results: %v", n);
    }
    ptr<openDeferInfo> opendefer = addr(new openDeferInfo(n:n,));
    var fn = n.X;
    if (n.Op() == ir.OCALLFUNC) { 
        // We must always store the function value in a stack slot for the
        // runtime panic code to use. But in the defer exit code, we will
        // call the function directly if it is a static function.
        var closureVal = s.expr(fn);
        var closure = s.openDeferSave(null, fn.Type(), closureVal);
        opendefer.closureNode = closure.Aux._<ptr<ir.Name>>();
        if (!(fn.Op() == ir.ONAME && fn._<ptr<ir.Name>>().Class == ir.PFUNC)) {
            opendefer.closure = closure;
        }
    }
    else if (n.Op() == ir.OCALLMETH) {
        @base.Fatalf("OCALLMETH missed by walkCall");
    }
    else
 {
        if (fn.Op() != ir.ODOTINTER) {
            @base.Fatalf("OCALLINTER: n.Left not an ODOTINTER: %v", fn.Op());
        }
        fn = fn._<ptr<ir.SelectorExpr>>();
        var (closure, rcvr) = s.getClosureAndRcvr(fn);
        opendefer.closure = s.openDeferSave(null, closure.Type, closure); 
        // Important to get the receiver type correct, so it is recognized
        // as a pointer for GC purposes.
        opendefer.rcvr = s.openDeferSave(null, fn.Type().Recv().Type, rcvr);
        opendefer.closureNode = opendefer.closure.Aux._<ptr<ir.Name>>();
        opendefer.rcvrNode = opendefer.rcvr.Aux._<ptr<ir.Name>>();
    }
    foreach (var (_, argn) in n.Args) {
        ptr<ssa.Value> v;
        if (TypeOK(_addr_argn.Type())) {
            v = s.openDeferSave(null, argn.Type(), s.expr(argn));
        }
        else
 {
            v = s.openDeferSave(argn, argn.Type(), null);
        }
        args = append(args, v);
        argNodes = append(argNodes, v.Aux._<ptr<ir.Name>>());
    }    opendefer.argVals = args;
    opendefer.argNodes = argNodes;
    var index = len(s.openDefers);
    s.openDefers = append(s.openDefers, opendefer); 

    // Update deferBits only after evaluation and storage to stack of
    // args/receiver/interface is successful.
    var bitvalue = s.constInt8(types.Types[types.TUINT8], 1 << (int)(uint(index)));
    var newDeferBits = s.newValue2(ssa.OpOr8, types.Types[types.TUINT8], s.variable(deferBitsVar, types.Types[types.TUINT8]), bitvalue);
    s.vars[deferBitsVar] = newDeferBits;
    s.store(types.Types[types.TUINT8], s.deferBitsAddr, newDeferBits);
}

// openDeferSave generates SSA nodes to store a value (with type t) for an
// open-coded defer at an explicit autotmp location on the stack, so it can be
// reloaded and used for the appropriate call on exit. If type t is SSAable, then
// val must be non-nil (and n should be nil) and val is the value to be stored. If
// type t is non-SSAable, then n must be non-nil (and val should be nil) and n is
// evaluated (via s.addr() below) to get the value that is to be stored. The
// function returns an SSA value representing a pointer to the autotmp location.
private static ptr<ssa.Value> openDeferSave(this ptr<state> _addr_s, ir.Node n, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_val) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value val = ref _addr_val.val;

    var canSSA = TypeOK(_addr_t);
    src.XPos pos = default;
    if (canSSA) {
        pos = val.Pos;
    }
    else
 {
        pos = n.Pos();
    }
    var argTemp = typecheck.TempAt(pos.WithNotStmt(), s.curfn, t);
    argTemp.SetOpenDeferSlot(true);
    ptr<ssa.Value> addrArgTemp; 
    // Use OpVarLive to make sure stack slots for the args, etc. are not
    // removed by dead-store elimination
    if (s.curBlock.ID != s.f.Entry.ID) { 
        // Force the argtmp storing this defer function/receiver/arg to be
        // declared in the entry block, so that it will be live for the
        // defer exit code (which will actually access it only if the
        // associated defer call has been activated).
        s.defvars[s.f.Entry.ID][memVar] = s.f.Entry.NewValue1A(src.NoXPos, ssa.OpVarDef, types.TypeMem, argTemp, s.defvars[s.f.Entry.ID][memVar]);
        s.defvars[s.f.Entry.ID][memVar] = s.f.Entry.NewValue1A(src.NoXPos, ssa.OpVarLive, types.TypeMem, argTemp, s.defvars[s.f.Entry.ID][memVar]);
        addrArgTemp = s.f.Entry.NewValue2A(src.NoXPos, ssa.OpLocalAddr, types.NewPtr(argTemp.Type()), argTemp, s.sp, s.defvars[s.f.Entry.ID][memVar]);
    }
    else
 { 
        // Special case if we're still in the entry block. We can't use
        // the above code, since s.defvars[s.f.Entry.ID] isn't defined
        // until we end the entry block with s.endBlock().
        s.vars[memVar] = s.newValue1Apos(ssa.OpVarDef, types.TypeMem, argTemp, s.mem(), false);
        s.vars[memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, argTemp, s.mem(), false);
        addrArgTemp = s.newValue2Apos(ssa.OpLocalAddr, types.NewPtr(argTemp.Type()), argTemp, s.sp, s.mem(), false);
    }
    if (t.HasPointers()) { 
        // Since we may use this argTemp during exit depending on the
        // deferBits, we must define it unconditionally on entry.
        // Therefore, we must make sure it is zeroed out in the entry
        // block if it contains pointers, else GC may wrongly follow an
        // uninitialized pointer value.
        argTemp.SetNeedzero(true);
    }
    if (!canSSA) {
        var a = s.addr(n);
        s.move(t, addrArgTemp, a);
        return _addr_addrArgTemp!;
    }
    s.store(t, addrArgTemp, val);
    return _addr_addrArgTemp!;
}

// openDeferExit generates SSA for processing all the open coded defers at exit.
// The code involves loading deferBits, and checking each of the bits to see if
// the corresponding defer statement was executed. For each bit that is turned
// on, the associated defer call is made.
private static void openDeferExit(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    var deferExit = s.f.NewBlock(ssa.BlockPlain);
    s.endBlock().AddEdgeTo(deferExit);
    s.startBlock(deferExit);
    s.lastDeferExit = deferExit;
    s.lastDeferCount = len(s.openDefers);
    var zeroval = s.constInt8(types.Types[types.TUINT8], 0); 
    // Test for and run defers in reverse order
    for (var i = len(s.openDefers) - 1; i >= 0; i--) {
        var r = s.openDefers[i];
        var bCond = s.f.NewBlock(ssa.BlockPlain);
        var bEnd = s.f.NewBlock(ssa.BlockPlain);

        var deferBits = s.variable(deferBitsVar, types.Types[types.TUINT8]); 
        // Generate code to check if the bit associated with the current
        // defer is set.
        var bitval = s.constInt8(types.Types[types.TUINT8], 1 << (int)(uint(i)));
        var andval = s.newValue2(ssa.OpAnd8, types.Types[types.TUINT8], deferBits, bitval);
        var eqVal = s.newValue2(ssa.OpEq8, types.Types[types.TBOOL], andval, zeroval);
        var b = s.endBlock();
        b.Kind = ssa.BlockIf;
        b.SetControl(eqVal);
        b.AddEdgeTo(bEnd);
        b.AddEdgeTo(bCond);
        bCond.AddEdgeTo(bEnd);
        s.startBlock(bCond); 

        // Clear this bit in deferBits and force store back to stack, so
        // we will not try to re-run this defer call if this defer call panics.
        var nbitval = s.newValue1(ssa.OpCom8, types.Types[types.TUINT8], bitval);
        var maskedval = s.newValue2(ssa.OpAnd8, types.Types[types.TUINT8], deferBits, nbitval);
        s.store(types.Types[types.TUINT8], s.deferBitsAddr, maskedval); 
        // Use this value for following tests, so we keep previous
        // bits cleared.
        s.vars[deferBitsVar] = maskedval; 

        // Generate code to call the function call of the defer, using the
        // closure/receiver/args that were stored in argtmps at the point
        // of the defer statement.
        var fn = r.n.X;
        var stksize = fn.Type().ArgWidth();
        slice<ptr<types.Type>> ACArgs = default;
        slice<ptr<types.Type>> ACResults = default;
        slice<ptr<ssa.Value>> callArgs = default;
        if (r.rcvr != null) { 
            // rcvr in case of OCALLINTER
            var v = s.load(r.rcvr.Type.Elem(), r.rcvr);
            ACArgs = append(ACArgs, types.Types[types.TUINTPTR]);
            callArgs = append(callArgs, v);
        }
        foreach (var (j, argAddrVal) in r.argVals) {
            var f = getParam(_addr_r.n, j);
            ACArgs = append(ACArgs, f.Type);
            ptr<ssa.Value> a;
            if (!TypeOK(_addr_f.Type)) {
                a = s.newValue2(ssa.OpDereference, f.Type, argAddrVal, s.mem());
            }
            else
 {
                a = s.load(f.Type, argAddrVal);
            }
            callArgs = append(callArgs, a);
        }        ptr<ssa.Value> call;
        if (r.closure != null) {
            v = s.load(r.closure.Type.Elem(), r.closure);
            s.maybeNilCheckClosure(v, callDefer);
            var codeptr = s.rawLoad(types.Types[types.TUINTPTR], v);
            var aux = ssa.ClosureAuxCall(s.f.ABIDefault.ABIAnalyzeTypes(null, ACArgs, ACResults));
            call = s.newValue2A(ssa.OpClosureLECall, aux.LateExpansionResultType(), aux, codeptr, v);
        }
        else
 {
            aux = ssa.StaticAuxCall(fn._<ptr<ir.Name>>().Linksym(), s.f.ABIDefault.ABIAnalyzeTypes(null, ACArgs, ACResults));
            call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux);
        }
        callArgs = append(callArgs, s.mem());
        call.AddArgs(callArgs);
        call.AuxInt = stksize;
        s.vars[memVar] = s.newValue1I(ssa.OpSelectN, types.TypeMem, int64(len(ACResults)), call); 
        // Make sure that the stack slots with pointers are kept live
        // through the call (which is a pre-emption point). Also, we will
        // use the first call of the last defer exit to compute liveness
        // for the deferreturn, so we want all stack slots to be live.
        if (r.closureNode != null) {
            s.vars[memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, r.closureNode, s.mem(), false);
        }
        if (r.rcvrNode != null) {
            if (r.rcvrNode.Type().HasPointers()) {
                s.vars[memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, r.rcvrNode, s.mem(), false);
            }
        }
        foreach (var (_, argNode) in r.argNodes) {
            if (argNode.Type().HasPointers()) {
                s.vars[memVar] = s.newValue1Apos(ssa.OpVarLive, types.TypeMem, argNode, s.mem(), false);
            }
        }        s.endBlock();
        s.startBlock(bEnd);
    }
}

private static ptr<ssa.Value> callResult(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n, callKind k) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    return _addr_s.call(n, k, false)!;
}

private static ptr<ssa.Value> callAddr(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n, callKind k) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    return _addr_s.call(n, k, true)!;
}

// Calls the function n using the specified call type.
// Returns the address of the return value (or nil if none).
private static ptr<ssa.Value> call(this ptr<state> _addr_s, ptr<ir.CallExpr> _addr_n, callKind k, bool returnResultAddr) {
    ref state s = ref _addr_s.val;
    ref ir.CallExpr n = ref _addr_n.val;

    s.prevCall = null;
    ptr<ir.Name> callee; // target function (if static)
    ptr<ssa.Value> closure; // ptr to closure to run (if dynamic)
    ptr<ssa.Value> codeptr; // ptr to target code (if dynamic)
    ptr<ssa.Value> rcvr; // receiver to set
    var fn = n.X;
    slice<ptr<types.Type>> ACArgs = default; // AuxCall args
    slice<ptr<types.Type>> ACResults = default; // AuxCall results
    slice<ptr<ssa.Value>> callArgs = default; // For late-expansion, the args themselves (not stored, args to the call instead).

    var callABI = s.f.ABIDefault;

    if (!buildcfg.Experiment.RegabiArgs) {
        ptr<types.Sym> magicFnNameSym;
        if (fn.Name() != null) {
            magicFnNameSym = fn.Name().Sym();
            var ss = magicFnNameSym.Name;
            if (strings.HasSuffix(ss, magicNameDotSuffix)) {
                callABI = s.f.ABI1;
            }
        }
        if (magicFnNameSym == null && n.Op() == ir.OCALLINTER) {
            magicFnNameSym = fn._<ptr<ir.SelectorExpr>>().Sym();
            ss = magicFnNameSym.Name;
            if (strings.HasSuffix(ss, magicNameDotSuffix[(int)1..])) {
                callABI = s.f.ABI1;
            }
        }
    }
    if (buildcfg.Experiment.RegabiDefer && k != callNormal && (len(n.Args) != 0 || n.Op() == ir.OCALLINTER || n.X.Type().NumResults() != 0)) {
        s.Fatalf("go/defer call with arguments: %v", n);
    }

    if (n.Op() == ir.OCALLFUNC) 
        if (k == callNormal && fn.Op() == ir.ONAME && fn._<ptr<ir.Name>>().Class == ir.PFUNC) {
            fn = fn._<ptr<ir.Name>>();
            callee = fn;
            if (buildcfg.Experiment.RegabiArgs) { 
                // This is a static call, so it may be
                // a direct call to a non-ABIInternal
                // function. fn.Func may be nil for
                // some compiler-generated functions,
                // but those are all ABIInternal.
                if (fn.Func != null) {
                    callABI = abiForFunc(_addr_fn.Func, _addr_s.f.ABI0, _addr_s.f.ABI1);
                }
            }
            else
 { 
                // TODO(register args) remove after register abi is working
                var inRegistersImported = fn.Pragma() & ir.RegisterParams != 0;
                var inRegistersSamePackage = fn.Func != null && fn.Func.Pragma & ir.RegisterParams != 0;
                if (inRegistersImported || inRegistersSamePackage) {
                    callABI = s.f.ABI1;
                }
            }
            break;
        }
        closure = s.expr(fn);
        if (k != callDefer && k != callDeferStack) { 
            // Deferred nil function needs to panic when the function is invoked,
            // not the point of defer statement.
            s.maybeNilCheckClosure(closure, k);
        }
    else if (n.Op() == ir.OCALLMETH) 
        @base.Fatalf("OCALLMETH missed by walkCall");
    else if (n.Op() == ir.OCALLINTER) 
        if (fn.Op() != ir.ODOTINTER) {
            s.Fatalf("OCALLINTER: n.Left not an ODOTINTER: %v", fn.Op());
        }
        fn = fn._<ptr<ir.SelectorExpr>>();
        ptr<ssa.Value> iclosure;
        iclosure, rcvr = s.getClosureAndRcvr(fn);
        if (k == callNormal) {
            codeptr = s.load(types.Types[types.TUINTPTR], iclosure);
        }
        else
 {
            closure = addr(iclosure);
        }
        if (!buildcfg.Experiment.RegabiArgs) {
        if (regAbiForFuncType(_addr_n.X.Type().FuncType())) { 
            // Magic last type in input args to call
            callABI = s.f.ABI1;
        }
    }
    var @params = callABI.ABIAnalyze(n.X.Type(), false);
    types.CalcSize(fn.Type());
    var stksize = @params.ArgWidth(); // includes receiver, args, and results

    var res = n.X.Type().Results();
    if (k == callNormal) {
        {
            var p__prev1 = p;

            foreach (var (_, __p) in @params.OutParams()) {
                p = __p;
                ACResults = append(ACResults, p.Type);
            }

            p = p__prev1;
        }
    }
    ptr<ssa.Value> call;
    if (k == callDeferStack) { 
        // Make a defer struct d on the stack.
        var t = deferstruct(stksize);
        var d = typecheck.TempAt(n.Pos(), s.curfn, t);

        s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, d, s.mem());
        var addr = s.addr(d); 

        // Must match reflect.go:deferstruct and src/runtime/runtime2.go:_defer.
        // 0: siz
        s.store(types.Types[types.TUINT32], s.newValue1I(ssa.OpOffPtr, types.Types[types.TUINT32].PtrTo(), t.FieldOff(0), addr), s.constInt32(types.Types[types.TUINT32], int32(stksize))); 
        // 1: started, set in deferprocStack
        // 2: heap, set in deferprocStack
        // 3: openDefer
        // 4: sp, set in deferprocStack
        // 5: pc, set in deferprocStack
        // 6: fn
        s.store(closure.Type, s.newValue1I(ssa.OpOffPtr, closure.Type.PtrTo(), t.FieldOff(6), addr), closure); 
        // 7: panic, set in deferprocStack
        // 8: link, set in deferprocStack
        // 9: framepc
        // 10: varp
        // 11: fd

        // Then, store all the arguments of the defer call.
        var ft = fn.Type();
        var off = t.FieldOff(12); // TODO register args: be sure this isn't a hardcoded param stack offset.
        var args = n.Args;
        nint i0 = 0; 

        // Set receiver (for interface calls). Always a pointer.
        if (rcvr != null) {
            var p = s.newValue1I(ssa.OpOffPtr, ft.Recv().Type.PtrTo(), off, addr);
            s.store(types.Types[types.TUINTPTR], p, rcvr);
            i0 = 1;
        }
        if (n.Op() == ir.OCALLMETH) {
            @base.Fatalf("OCALLMETH missed by walkCall");
        }
        {
            var i__prev1 = i;

            foreach (var (__i, __f) in ft.Params().Fields().Slice()) {
                i = __i;
                f = __f;
                s.storeArgWithBase(args[0], f.Type, addr, off + @params.InParam(i + i0).FrameOffset(params));
                args = args[(int)1..];
            }
    else
 

            // Call runtime.deferprocStack with pointer to _defer record.

            i = i__prev1;
        }

        ACArgs = append(ACArgs, types.Types[types.TUINTPTR]);
        var aux = ssa.StaticAuxCall(ir.Syms.DeferprocStack, s.f.ABIDefault.ABIAnalyzeTypes(null, ACArgs, ACResults));
        callArgs = append(callArgs, addr, s.mem());
        call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux);
        call.AddArgs(callArgs);
        if (stksize < int64(types.PtrSize)) { 
            // We need room for both the call to deferprocStack and the call to
            // the deferred function.
            stksize = int64(types.PtrSize);
        }
        call.AuxInt = stksize;
    } { 
        // Store arguments to stack, including defer/go arguments and receiver for method calls.
        // These are written in SP-offset order.
        var argStart = @base.Ctxt.FixedFrameSize(); 
        // Defer/go args.
        if (k != callNormal) { 
            // Write argsize and closure (args to newproc/deferproc).
            var argsize = s.constInt32(types.Types[types.TUINT32], int32(stksize));
            ACArgs = append(ACArgs, types.Types[types.TUINT32]); // not argExtra
            callArgs = append(callArgs, argsize);
            ACArgs = append(ACArgs, types.Types[types.TUINTPTR]);
            callArgs = append(callArgs, closure);
            stksize += 2 * int64(types.PtrSize);
            argStart += 2 * int64(types.PtrSize);
        }
        if (rcvr != null) {
            callArgs = append(callArgs, rcvr);
        }
        t = n.X.Type();
        args = n.Args;
        if (n.Op() == ir.OCALLMETH) {
            @base.Fatalf("OCALLMETH missed by walkCall");
        }
        {
            var p__prev1 = p;

            foreach (var (_, __p) in @params.InParams()) {
                p = __p; // includes receiver for interface calls
                ACArgs = append(ACArgs, p.Type);
            }

            p = p__prev1;
        }

        {
            var i__prev1 = i;

            foreach (var (__i, __n) in args) {
                i = __i;
                n = __n;
                callArgs = append(callArgs, s.putArg(n, t.Params().Field(i).Type));
            }

            i = i__prev1;
        }

        callArgs = append(callArgs, s.mem()); 

        // call target

        if (k == callDefer) 
            aux = ssa.StaticAuxCall(ir.Syms.Deferproc, s.f.ABIDefault.ABIAnalyzeTypes(null, ACArgs, ACResults)); // TODO paramResultInfo for DeferProc
            call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux);
        else if (k == callGo) 
            aux = ssa.StaticAuxCall(ir.Syms.Newproc, s.f.ABIDefault.ABIAnalyzeTypes(null, ACArgs, ACResults));
            call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux); // TODO paramResultInfo for NewProc
        else if (closure != null) 
            // rawLoad because loading the code pointer from a
            // closure is always safe, but IsSanitizerSafeAddr
            // can't always figure that out currently, and it's
            // critical that we not clobber any arguments already
            // stored onto the stack.
            codeptr = s.rawLoad(types.Types[types.TUINTPTR], closure);
            aux = ssa.ClosureAuxCall(callABI.ABIAnalyzeTypes(null, ACArgs, ACResults));
            call = s.newValue2A(ssa.OpClosureLECall, aux.LateExpansionResultType(), aux, codeptr, closure);
        else if (codeptr != null) 
            // Note that the "receiver" parameter is nil because the actual receiver is the first input parameter.
            aux = ssa.InterfaceAuxCall(params);
            call = s.newValue1A(ssa.OpInterLECall, aux.LateExpansionResultType(), aux, codeptr);
        else if (callee != null) 
            aux = ssa.StaticAuxCall(callTargetLSym(callee), params);
            call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux);
        else 
            s.Fatalf("bad call type %v %v", n.Op(), n);
                call.AddArgs(callArgs);
        call.AuxInt = stksize; // Call operations carry the argsize of the callee along with them
    }
    s.prevCall = call;
    s.vars[memVar] = s.newValue1I(ssa.OpSelectN, types.TypeMem, int64(len(ACResults)), call); 
    // Insert OVARLIVE nodes
    foreach (var (_, name) in n.KeepAlive) {
        s.stmt(ir.NewUnaryExpr(n.Pos(), ir.OVARLIVE, name));
    }    if (k == callDefer || k == callDeferStack) {
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
    if (res.NumFields() == 0 || k != callNormal) { 
        // call has no return value. Continue with the next statement.
        return _addr_null!;
    }
    var fp = res.Field(0);
    if (returnResultAddr) {
        return _addr_s.resultAddrOfCall(call, 0, fp.Type)!;
    }
    return _addr_s.newValue1I(ssa.OpSelectN, fp.Type, 0, call)!;
}

// maybeNilCheckClosure checks if a nil check of a closure is needed in some
// architecture-dependent situations and, if so, emits the nil check.
private static void maybeNilCheckClosure(this ptr<state> _addr_s, ptr<ssa.Value> _addr_closure, callKind k) {
    ref state s = ref _addr_s.val;
    ref ssa.Value closure = ref _addr_closure.val;

    if (Arch.LinkArch.Family == sys.Wasm || buildcfg.GOOS == "aix" && k != callGo) { 
        // On AIX, the closure needs to be verified as fn can be nil, except if it's a call go. This needs to be handled by the runtime to have the "go of nil func value" error.
        // TODO(neelance): On other architectures this should be eliminated by the optimization steps
        s.nilCheck(closure);
    }
}

// getClosureAndRcvr returns values for the appropriate closure and receiver of an
// interface call
private static (ptr<ssa.Value>, ptr<ssa.Value>) getClosureAndRcvr(this ptr<state> _addr_s, ptr<ir.SelectorExpr> _addr_fn) {
    ptr<ssa.Value> _p0 = default!;
    ptr<ssa.Value> _p0 = default!;
    ref state s = ref _addr_s.val;
    ref ir.SelectorExpr fn = ref _addr_fn.val;

    var i = s.expr(fn.X);
    var itab = s.newValue1(ssa.OpITab, types.Types[types.TUINTPTR], i);
    s.nilCheck(itab);
    var itabidx = fn.Offset() + 2 * int64(types.PtrSize) + 8; // offset of fun field in runtime.itab
    var closure = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.UintptrPtr, itabidx, itab);
    var rcvr = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, i);
    return (_addr_closure!, _addr_rcvr!);
}

// etypesign returns the signed-ness of e, for integer/pointer etypes.
// -1 means signed, +1 means unsigned, 0 means non-integer/non-pointer.
private static sbyte etypesign(types.Kind e) {

    if (e == types.TINT8 || e == types.TINT16 || e == types.TINT32 || e == types.TINT64 || e == types.TINT) 
        return -1;
    else if (e == types.TUINT8 || e == types.TUINT16 || e == types.TUINT32 || e == types.TUINT64 || e == types.TUINT || e == types.TUINTPTR || e == types.TUNSAFEPTR) 
        return +1;
        return 0;
}

// addr converts the address of the expression n to SSA, adds it to s and returns the SSA result.
// The value that the returned Value represents is guaranteed to be non-nil.
private static ptr<ssa.Value> addr(this ptr<state> _addr_s, ir.Node n) => func((defer, _, _) => {
    ref state s = ref _addr_s.val;

    if (n.Op() != ir.ONAME) {
        s.pushLine(n.Pos());
        defer(s.popLine());
    }
    if (s.canSSA(n)) {
        s.Fatalf("addr of canSSA expression: %+v", n);
    }
    var t = types.NewPtr(n.Type());
    Func<ptr<obj.LSym>, long, ptr<ssa.Value>> linksymOffset = (lsym, offset) => {
        var v = s.entryNewValue1A(ssa.OpAddr, t, lsym, s.sb); 
        // TODO: Make OpAddr use AuxInt as well as Aux.
        if (offset != 0) {
            v = s.entryNewValue1I(ssa.OpOffPtr, v.Type, offset, v);
        }
        return _addr_v!;
    };

    if (n.Op() == ir.OLINKSYMOFFSET) 
        ptr<ir.LinksymOffsetExpr> no = n._<ptr<ir.LinksymOffsetExpr>>();
        return _addr_linksymOffset(no.Linksym, no.Offset_)!;
    else if (n.Op() == ir.ONAME) 
        ptr<ir.Name> n = n._<ptr<ir.Name>>();
        if (n.Heapaddr != null) {
            return _addr_s.expr(n.Heapaddr)!;
        }

        if (n.Class == ir.PEXTERN) 
            // global variable
            return _addr_linksymOffset(n.Linksym(), 0)!;
        else if (n.Class == ir.PPARAM) 
            // parameter slot
            v = s.decladdrs[n];
            if (v != null) {
                return _addr_v!;
            }
            s.Fatalf("addr of undeclared ONAME %v. declared: %v", n, s.decladdrs);
            return _addr_null!;
        else if (n.Class == ir.PAUTO) 
            return _addr_s.newValue2Apos(ssa.OpLocalAddr, t, n, s.sp, s.mem(), !ir.IsAutoTmp(n))!;
        else if (n.Class == ir.PPARAMOUT) // Same as PAUTO -- cannot generate LEA early.
            // ensure that we reuse symbols for out parameters so
            // that cse works on their addresses
            return _addr_s.newValue2Apos(ssa.OpLocalAddr, t, n, s.sp, s.mem(), true)!;
        else 
            s.Fatalf("variable address class %v not implemented", n.Class);
            return _addr_null!;
            else if (n.Op() == ir.ORESULT) 
        // load return from callee
        n = n._<ptr<ir.ResultExpr>>();
        return _addr_s.resultAddrOfCall(s.prevCall, n.Index, n.Type())!;
    else if (n.Op() == ir.OINDEX) 
        n = n._<ptr<ir.IndexExpr>>();
        if (n.X.Type().IsSlice()) {
            var a = s.expr(n.X);
            var i = s.expr(n.Index);
            var len = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], a);
            i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
            var p = s.newValue1(ssa.OpSlicePtr, t, a);
            return _addr_s.newValue2(ssa.OpPtrIndex, t, p, i)!;
        }
        else
 { // array
            a = s.addr(n.X);
            i = s.expr(n.Index);
            len = s.constInt(types.Types[types.TINT], n.X.Type().NumElem());
            i = s.boundsCheck(i, len, ssa.BoundsIndex, n.Bounded());
            return _addr_s.newValue2(ssa.OpPtrIndex, types.NewPtr(n.X.Type().Elem()), a, i)!;
        }
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        return _addr_s.exprPtr(n.X, n.Bounded(), n.Pos())!;
    else if (n.Op() == ir.ODOT) 
        n = n._<ptr<ir.SelectorExpr>>();
        p = s.addr(n.X);
        return _addr_s.newValue1I(ssa.OpOffPtr, t, n.Offset(), p)!;
    else if (n.Op() == ir.ODOTPTR) 
        n = n._<ptr<ir.SelectorExpr>>();
        p = s.exprPtr(n.X, n.Bounded(), n.Pos());
        return _addr_s.newValue1I(ssa.OpOffPtr, t, n.Offset(), p)!;
    else if (n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        if (n.Type() == n.X.Type()) {
            return _addr_s.addr(n.X)!;
        }
        var addr = s.addr(n.X);
        return _addr_s.newValue1(ssa.OpCopy, t, addr)!; // ensure that addr has the right type
    else if (n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER) 
        n = n._<ptr<ir.CallExpr>>();
        return _addr_s.callAddr(n, callNormal)!;
    else if (n.Op() == ir.ODOTTYPE) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        var (v, _) = s.dottype(n, false);
        if (v.Op != ssa.OpLoad) {
            s.Fatalf("dottype of non-load");
        }
        if (v.Args[1] != s.mem()) {
            s.Fatalf("memory no longer live from dottype load");
        }
        return _addr_v.Args[0]!;
    else 
        s.Fatalf("unhandled addr %v", n.Op());
        return _addr_null!;
    });

// canSSA reports whether n is SSA-able.
// n must be an ONAME (or an ODOT sequence with an ONAME base).
private static bool canSSA(this ptr<state> _addr_s, ir.Node n) {
    ref state s = ref _addr_s.val;

    if (@base.Flag.N != 0) {
        return false;
    }
    while (true) {
        var nn = n;
        if (nn.Op() == ir.ODOT) {
            nn = nn._<ptr<ir.SelectorExpr>>();
            n = nn.X;
            continue;
        }
        if (nn.Op() == ir.OINDEX) {
            nn = nn._<ptr<ir.IndexExpr>>();
            if (nn.X.Type().IsArray()) {
                n = nn.X;
                continue;
            }
        }
        break;
    }
    if (n.Op() != ir.ONAME) {
        return false;
    }
    return s.canSSAName(n._<ptr<ir.Name>>()) && TypeOK(_addr_n.Type());
}

private static bool canSSAName(this ptr<state> _addr_s, ptr<ir.Name> _addr_name) {
    ref state s = ref _addr_s.val;
    ref ir.Name name = ref _addr_name.val;

    if (name.Addrtaken() || !name.OnStack()) {
        return false;
    }

    if (name.Class == ir.PPARAMOUT) 
        if (s.hasdefer) { 
            // TODO: handle this case? Named return values must be
            // in memory so that the deferred function can see them.
            // Maybe do: if !strings.HasPrefix(n.String(), "~") { return false }
            // Or maybe not, see issue 18860.  Even unnamed return values
            // must be written back so if a defer recovers, the caller can see them.
            return false;
        }
        if (s.cgoUnsafeArgs) { 
            // Cgo effectively takes the address of all result args,
            // but the compiler can't see that.
            return false;
        }
        if (name.Class == ir.PPARAM && name.Sym() != null && name.Sym().Name == ".this") { 
        // wrappers generated by genwrapper need to update
        // the .this pointer in place.
        // TODO: treat as a PPARAMOUT?
        return false;
    }
    return true; 
    // TODO: try to make more variables SSAable?
}

// TypeOK reports whether variables of type t are SSA-able.
public static bool TypeOK(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    types.CalcSize(t);
    if (t.Width > int64(4 * types.PtrSize)) { 
        // 4*Widthptr is an arbitrary constant. We want it
        // to be at least 3*Widthptr so slices can be registerized.
        // Too big and we'll introduce too much register pressure.
        return false;
    }

    if (t.Kind() == types.TARRAY) 
        // We can't do larger arrays because dynamic indexing is
        // not supported on SSA variables.
        // TODO: allow if all indexes are constant.
        if (t.NumElem() <= 1) {
            return TypeOK(_addr_t.Elem());
        }
        return false;
    else if (t.Kind() == types.TSTRUCT) 
        if (t.NumFields() > ssa.MaxStruct) {
            return false;
        }
        foreach (var (_, t1) in t.Fields().Slice()) {
            if (!TypeOK(_addr_t1.Type)) {
                return false;
            }
        }        return true;
    else 
        return true;
    }

// exprPtr evaluates n to a pointer and nil-checks it.
private static ptr<ssa.Value> exprPtr(this ptr<state> _addr_s, ir.Node n, bool bounded, src.XPos lineno) {
    ref state s = ref _addr_s.val;

    var p = s.expr(n);
    if (bounded || n.NonNil()) {
        if (s.f.Frontend().Debug_checknil() && lineno.Line() > 1) {
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
private static void nilCheck(this ptr<state> _addr_s, ptr<ssa.Value> _addr_ptr) {
    ref state s = ref _addr_s.val;
    ref ssa.Value ptr = ref _addr_ptr.val;

    if (@base.Debug.DisableNil != 0 || s.curfn.NilCheckDisabled()) {
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
private static ptr<ssa.Value> boundsCheck(this ptr<state> _addr_s, ptr<ssa.Value> _addr_idx, ptr<ssa.Value> _addr_len, ssa.BoundsKind kind, bool bounded) {
    ref state s = ref _addr_s.val;
    ref ssa.Value idx = ref _addr_idx.val;
    ref ssa.Value len = ref _addr_len.val;

    idx = s.extendIndex(idx, len, kind, bounded);

    if (bounded || @base.Flag.B != 0) { 
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

    if (!idx.Type.IsSigned()) {

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
    if (kind == ssa.BoundsIndex || kind == ssa.BoundsIndexU) {
        cmp = s.newValue2(ssa.OpIsInBounds, types.Types[types.TBOOL], idx, len);
    }
    else
 {
        cmp = s.newValue2(ssa.OpIsSliceInBounds, types.Types[types.TBOOL], idx, len);
    }
    var b = s.endBlock();
    b.Kind = ssa.BlockIf;
    b.SetControl(cmp);
    b.Likely = ssa.BranchLikely;
    b.AddEdgeTo(bNext);
    b.AddEdgeTo(bPanic);

    s.startBlock(bPanic);
    if (Arch.LinkArch.Family == sys.Wasm) { 
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
    if (@base.Flag.Cfg.SpectreIndex) {
        var op = ssa.OpSpectreIndex;
        if (kind != ssa.BoundsIndex && kind != ssa.BoundsIndexU) {
            op = ssa.OpSpectreSliceIndex;
        }
        idx = s.newValue2(op, types.Types[types.TINT], idx, len);
    }
    return _addr_idx!;
}

// If cmp (a bool) is false, panic using the given function.
private static void check(this ptr<state> _addr_s, ptr<ssa.Value> _addr_cmp, ptr<obj.LSym> _addr_fn) {
    ref state s = ref _addr_s.val;
    ref ssa.Value cmp = ref _addr_cmp.val;
    ref obj.LSym fn = ref _addr_fn.val;

    var b = s.endBlock();
    b.Kind = ssa.BlockIf;
    b.SetControl(cmp);
    b.Likely = ssa.BranchLikely;
    var bNext = s.f.NewBlock(ssa.BlockPlain);
    var line = s.peekPos();
    var pos = @base.Ctxt.PosTable.Pos(line);
    funcLine fl = new funcLine(f:fn,base:pos.Base(),line:pos.Line());
    var bPanic = s.panics[fl];
    if (bPanic == null) {
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

private static ptr<ssa.Value> intDivide(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_a, ptr<ssa.Value> _addr_b) {
    ref state s = ref _addr_s.val;
    ref ssa.Value a = ref _addr_a.val;
    ref ssa.Value b = ref _addr_b.val;

    var needcheck = true;

    if (b.Op == ssa.OpConst8 || b.Op == ssa.OpConst16 || b.Op == ssa.OpConst32 || b.Op == ssa.OpConst64) 
        if (b.AuxInt != 0) {
            needcheck = false;
        }
        if (needcheck) { 
        // do a size-appropriate check for zero
        var cmp = s.newValue2(s.ssaOp(ir.ONE, n.Type()), types.Types[types.TBOOL], b, s.zeroVal(n.Type()));
        s.check(cmp, ir.Syms.Panicdivide);
    }
    return _addr_s.newValue2(s.ssaOp(n.Op(), n.Type()), a.Type, a, b)!;
}

// rtcall issues a call to the given runtime function fn with the listed args.
// Returns a slice of results of the given result types.
// The call is added to the end of the current block.
// If returns is false, the block is marked as an exit block.
private static slice<ptr<ssa.Value>> rtcall(this ptr<state> _addr_s, ptr<obj.LSym> _addr_fn, bool returns, slice<ptr<types.Type>> results, params ptr<ptr<ssa.Value>>[] _addr_args) {
    args = args.Clone();
    ref state s = ref _addr_s.val;
    ref obj.LSym fn = ref _addr_fn.val;
    ref ssa.Value args = ref _addr_args.val;

    s.prevCall = null; 
    // Write args to the stack
    var off = @base.Ctxt.FixedFrameSize();
    slice<ptr<ssa.Value>> callArgs = default;
    slice<ptr<types.Type>> callArgTypes = default;

    foreach (var (_, arg) in args) {
        var t = arg.Type;
        off = types.Rnd(off, t.Alignment());
        var size = t.Size();
        callArgs = append(callArgs, arg);
        callArgTypes = append(callArgTypes, t);
        off += size;
    }    off = types.Rnd(off, int64(types.RegSize)); 

    // Accumulate results types and offsets
    var offR = off;
    {
        var t__prev1 = t;

        foreach (var (_, __t) in results) {
            t = __t;
            offR = types.Rnd(offR, t.Alignment());
            offR += t.Size();
        }
        t = t__prev1;
    }

    ptr<ssa.Value> call;
    var aux = ssa.StaticAuxCall(fn, s.f.ABIDefault.ABIAnalyzeTypes(null, callArgTypes, results));
    callArgs = append(callArgs, s.mem());
    call = s.newValue0A(ssa.OpStaticLECall, aux.LateExpansionResultType(), aux);
    call.AddArgs(callArgs);
    s.vars[memVar] = s.newValue1I(ssa.OpSelectN, types.TypeMem, int64(len(results)), call);

    if (!returns) { 
        // Finish block
        var b = s.endBlock();
        b.Kind = ssa.BlockExit;
        b.SetControl(call);
        call.AuxInt = off - @base.Ctxt.FixedFrameSize();
        if (len(results) > 0) {
            s.Fatalf("panic call can't have results");
        }
        return null;
    }
    var res = make_slice<ptr<ssa.Value>>(len(results));
    {
        var t__prev1 = t;

        foreach (var (__i, __t) in results) {
            i = __i;
            t = __t;
            off = types.Rnd(off, t.Alignment());
            res[i] = s.resultOfCall(call, int64(i), t);
            off += t.Size();
        }
        t = t__prev1;
    }

    off = types.Rnd(off, int64(types.PtrSize)); 

    // Remember how much callee stack space we needed.
    call.AuxInt = off;

    return res;
}

// do *left = right for type t.
private static void storeType(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right, skipMask skip, bool leftIsStmt) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value left = ref _addr_left.val;
    ref ssa.Value right = ref _addr_right.val;

    s.instrument(t, left, instrumentWrite);

    if (skip == 0 && (!t.HasPointers() || ssa.IsStackAddr(left))) { 
        // Known to not have write barrier. Store the whole type.
        s.vars[memVar] = s.newValue3Apos(ssa.OpStore, types.TypeMem, t, left, right, s.mem(), leftIsStmt);
        return ;
    }
    s.storeTypeScalars(t, left, right, skip);
    if (skip & skipPtr == 0 && t.HasPointers()) {
        s.storeTypePtrs(t, left, right);
    }
}

// do *left = right for all scalar (non-pointer) parts of t.
private static void storeTypeScalars(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right, skipMask skip) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value left = ref _addr_left.val;
    ref ssa.Value right = ref _addr_right.val;


    if (t.IsBoolean() || t.IsInteger() || t.IsFloat() || t.IsComplex()) 
        s.store(t, left, right);
    else if (t.IsPtrShaped()) 
        if (t.IsPtr() && t.Elem().NotInHeap()) {
            s.store(t, left, right); // see issue 42032
        }
    else if (t.IsString()) 
        if (skip & skipLen != 0) {
            return ;
        }
        var len = s.newValue1(ssa.OpStringLen, types.Types[types.TINT], right);
        var lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
        s.store(types.Types[types.TINT], lenAddr, len);
    else if (t.IsSlice()) 
        if (skip & skipLen == 0) {
            len = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], right);
            lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
            s.store(types.Types[types.TINT], lenAddr, len);
        }
        if (skip & skipCap == 0) {
            var cap = s.newValue1(ssa.OpSliceCap, types.Types[types.TINT], right);
            var capAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, 2 * s.config.PtrSize, left);
            s.store(types.Types[types.TINT], capAddr, cap);
        }
    else if (t.IsInterface()) 
        // itab field doesn't need a write barrier (even though it is a pointer).
        var itab = s.newValue1(ssa.OpITab, s.f.Config.Types.BytePtr, right);
        s.store(types.Types[types.TUINTPTR], left, itab);
    else if (t.IsStruct()) 
        var n = t.NumFields();
        for (nint i = 0; i < n; i++) {
            var ft = t.FieldType(i);
            var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
            var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
            s.storeTypeScalars(ft, addr, val, 0);
        }
    else if (t.IsArray() && t.NumElem() == 0)     else if (t.IsArray() && t.NumElem() == 1) 
        s.storeTypeScalars(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0, right), 0);
    else 
        s.Fatalf("bad write barrier type %v", t);
    }

// do *left = right for all pointer parts of t.
private static void storeTypePtrs(this ptr<state> _addr_s, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_left, ptr<ssa.Value> _addr_right) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value left = ref _addr_left.val;
    ref ssa.Value right = ref _addr_right.val;


    if (t.IsPtrShaped()) 
        if (t.IsPtr() && t.Elem().NotInHeap()) {
            break; // see issue 42032
        }
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
        for (nint i = 0; i < n; i++) {
            var ft = t.FieldType(i);
            if (!ft.HasPointers()) {
                continue;
            }
            var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
            var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
            s.storeTypePtrs(ft, addr, val);
        }
    else if (t.IsArray() && t.NumElem() == 0)     else if (t.IsArray() && t.NumElem() == 1) 
        s.storeTypePtrs(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0, right));
    else 
        s.Fatalf("bad write barrier type %v", t);
    }

// putArg evaluates n for the purpose of passing it as an argument to a function and returns the value for the call.
private static ptr<ssa.Value> putArg(this ptr<state> _addr_s, ir.Node n, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    ptr<ssa.Value> a;
    if (!TypeOK(_addr_t)) {
        a = s.newValue2(ssa.OpDereference, t, s.addr(n), s.mem());
    }
    else
 {
        a = s.expr(n);
    }
    return _addr_a!;
}

private static void storeArgWithBase(this ptr<state> _addr_s, ir.Node n, ptr<types.Type> _addr_t, ptr<ssa.Value> _addr_@base, long off) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;
    ref ssa.Value @base = ref _addr_@base.val;

    var pt = types.NewPtr(t);
    ptr<ssa.Value> addr;
    if (base == s.sp) { 
        // Use special routine that avoids allocation on duplicate offsets.
        addr = s.constOffPtrSP(pt, off);
    }
    else
 {
        addr = s.newValue1I(ssa.OpOffPtr, pt, off, base);
    }
    if (!TypeOK(_addr_t)) {
        var a = s.addr(n);
        s.move(t, addr, a);
        return ;
    }
    a = s.expr(n);
    s.storeType(t, addr, a, 0, false);
}

// slice computes the slice v[i:j:k] and returns ptr, len, and cap of result.
// i,j,k may be nil, in which case they are set to their default value.
// v may be a slice, string or pointer to an array.
private static (ptr<ssa.Value>, ptr<ssa.Value>, ptr<ssa.Value>) slice(this ptr<state> _addr_s, ptr<ssa.Value> _addr_v, ptr<ssa.Value> _addr_i, ptr<ssa.Value> _addr_j, ptr<ssa.Value> _addr_k, bool bounded) {
    ptr<ssa.Value> p = default!;
    ptr<ssa.Value> l = default!;
    ptr<ssa.Value> c = default!;
    ref state s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;
    ref ssa.Value i = ref _addr_i.val;
    ref ssa.Value j = ref _addr_j.val;
    ref ssa.Value k = ref _addr_k.val;

    var t = v.Type;
    ptr<ssa.Value> ptr;    ptr<ssa.Value> len;    ptr<ssa.Value> cap;


    if (t.IsSlice()) 
        ptr = s.newValue1(ssa.OpSlicePtr, types.NewPtr(t.Elem()), v);
        len = s.newValue1(ssa.OpSliceLen, types.Types[types.TINT], v);
        cap = s.newValue1(ssa.OpSliceCap, types.Types[types.TINT], v);
    else if (t.IsString()) 
        ptr = s.newValue1(ssa.OpStringPtr, types.NewPtr(types.Types[types.TUINT8]), v);
        len = s.newValue1(ssa.OpStringLen, types.Types[types.TINT], v);
        cap = addr(len);
    else if (t.IsPtr()) 
        if (!t.Elem().IsArray()) {
            s.Fatalf("bad ptr to array in slice %v\n", t);
        }
        s.nilCheck(v);
        ptr = s.newValue1(ssa.OpCopy, types.NewPtr(t.Elem().Elem()), v);
        len = s.constInt(types.Types[types.TINT], t.Elem().NumElem());
        cap = addr(len);
    else 
        s.Fatalf("bad type in slice %v\n", t);
    // Set default values
    if (i == null) {
        i = s.constInt(types.Types[types.TINT], 0);
    }
    if (j == null) {
        j = len;
    }
    var three = true;
    if (k == null) {
        three = false;
        k = cap;
    }
    if (three) {
        if (k != cap) {
            var kind = ssa.BoundsSlice3Alen;
            if (t.IsSlice()) {
                kind = ssa.BoundsSlice3Acap;
            }
            k = s.boundsCheck(k, cap, kind, bounded);
        }
        if (j != k) {
            j = s.boundsCheck(j, k, ssa.BoundsSlice3B, bounded);
        }
        i = s.boundsCheck(i, j, ssa.BoundsSlice3C, bounded);
    }
    else
 {
        if (j != k) {
            kind = ssa.BoundsSliceAlen;
            if (t.IsSlice()) {
                kind = ssa.BoundsSliceAcap;
            }
            j = s.boundsCheck(j, k, kind, bounded);
        }
        i = s.boundsCheck(i, j, ssa.BoundsSliceB, bounded);
    }
    var subOp = s.ssaOp(ir.OSUB, types.Types[types.TINT]);
    var mulOp = s.ssaOp(ir.OMUL, types.Types[types.TINT]);
    var andOp = s.ssaOp(ir.OAND, types.Types[types.TINT]); 

    // Calculate the length (rlen) and capacity (rcap) of the new slice.
    // For strings the capacity of the result is unimportant. However,
    // we use rcap to test if we've generated a zero-length slice.
    // Use length of strings for that.
    var rlen = s.newValue2(subOp, types.Types[types.TINT], j, i);
    var rcap = rlen;
    if (j != k && !t.IsString()) {
        rcap = s.newValue2(subOp, types.Types[types.TINT], k, i);
    }
    if ((i.Op == ssa.OpConst64 || i.Op == ssa.OpConst32) && i.AuxInt == 0) { 
        // No pointer arithmetic necessary.
        return (_addr_ptr!, _addr_rlen!, _addr_rcap!);
    }
    var stride = s.constInt(types.Types[types.TINT], ptr.Type.Elem().Width); 

    // The delta is the number of bytes to offset ptr by.
    var delta = s.newValue2(mulOp, types.Types[types.TINT], i, stride); 

    // If we're slicing to the point where the capacity is zero,
    // zero out the delta.
    var mask = s.newValue1(ssa.OpSlicemask, types.Types[types.TINT], rcap);
    delta = s.newValue2(andOp, types.Types[types.TINT], delta, mask); 

    // Compute rptr = ptr + delta.
    var rptr = s.newValue2(ssa.OpAddPtr, ptr.Type, ptr, delta);

    return (_addr_rptr!, _addr_rlen!, _addr_rcap!);
}

private partial struct u642fcvtTab {
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

private static ptr<ssa.Value> uint64Tofloat64(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.uint64Tofloat(_addr_u64_f64, n, x, ft, tt)!;
}

private static ptr<ssa.Value> uint64Tofloat32(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.uint64Tofloat(_addr_u64_f32, n, x, ft, tt)!;
}

private static ptr<ssa.Value> uint64Tofloat(this ptr<state> _addr_s, ptr<u642fcvtTab> _addr_cvttab, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref u642fcvtTab cvttab = ref _addr_cvttab.val;
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
    var cmp = s.newValue2(cvttab.leq, types.Types[types.TBOOL], s.zeroVal(ft), x);
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
    var one = cvttab.one(s, ft, 1);
    var y = s.newValue2(cvttab.and, ft, x, one);
    var z = s.newValue2(cvttab.rsh, ft, x, one);
    z = s.newValue2(cvttab.or, ft, z, y);
    var a = s.newValue1(cvttab.cvt2F, tt, z);
    var a1 = s.newValue2(cvttab.add, tt, a, a);
    s.vars[n] = a1;
    s.endBlock();
    bElse.AddEdgeTo(bAfter);

    s.startBlock(bAfter);
    return _addr_s.variable(n, n.Type())!;
}

private partial struct u322fcvtTab {
    public ssa.Op cvtI2F;
    public ssa.Op cvtF2F;
}

private static u322fcvtTab u32_f64 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to64F,cvtF2F:ssa.OpCopy,);

private static u322fcvtTab u32_f32 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to32F,cvtF2F:ssa.OpCvt64Fto32F,);

private static ptr<ssa.Value> uint32Tofloat64(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.uint32Tofloat(_addr_u32_f64, n, x, ft, tt)!;
}

private static ptr<ssa.Value> uint32Tofloat32(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.uint32Tofloat(_addr_u32_f32, n, x, ft, tt)!;
}

private static ptr<ssa.Value> uint32Tofloat(this ptr<state> _addr_s, ptr<u322fcvtTab> _addr_cvttab, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref u322fcvtTab cvttab = ref _addr_cvttab.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;
 
    // if x >= 0 {
    //     result = floatY(x)
    // } else {
    //     result = floatY(float64(x) + (1<<32))
    // }
    var cmp = s.newValue2(ssa.OpLeq32, types.Types[types.TBOOL], s.zeroVal(ft), x);
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
    var a1 = s.newValue1(ssa.OpCvt32to64F, types.Types[types.TFLOAT64], x);
    var twoToThe32 = s.constFloat64(types.Types[types.TFLOAT64], float64(1 << 32));
    var a2 = s.newValue2(ssa.OpAdd64F, types.Types[types.TFLOAT64], a1, twoToThe32);
    var a3 = s.newValue1(cvttab.cvtF2F, tt, a2);

    s.vars[n] = a3;
    s.endBlock();
    bElse.AddEdgeTo(bAfter);

    s.startBlock(bAfter);
    return _addr_s.variable(n, n.Type())!;
}

// referenceTypeBuiltin generates code for the len/cap builtins for maps and channels.
private static ptr<ssa.Value> referenceTypeBuiltin(this ptr<state> _addr_s, ptr<ir.UnaryExpr> _addr_n, ptr<ssa.Value> _addr_x) {
    ref state s = ref _addr_s.val;
    ref ir.UnaryExpr n = ref _addr_n.val;
    ref ssa.Value x = ref _addr_x.val;

    if (!n.X.Type().IsMap() && !n.X.Type().IsChan()) {
        s.Fatalf("node must be a map or a channel");
    }
    var lenType = n.Type();
    var nilValue = s.constNil(types.Types[types.TUINTPTR]);
    var cmp = s.newValue2(ssa.OpEqPtr, types.Types[types.TBOOL], x, nilValue);
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

    if (n.Op() == ir.OLEN) 
        // length is stored in the first word for map/chan
        s.vars[n] = s.load(lenType, x);
    else if (n.Op() == ir.OCAP) 
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

private partial struct f2uCvtTab {
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

private static ptr<ssa.Value> float32ToUint64(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.floatToUint(_addr_f32_u64, n, x, ft, tt)!;
}
private static ptr<ssa.Value> float64ToUint64(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.floatToUint(_addr_f64_u64, n, x, ft, tt)!;
}

private static ptr<ssa.Value> float32ToUint32(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.floatToUint(_addr_f32_u32, n, x, ft, tt)!;
}

private static ptr<ssa.Value> float64ToUint32(this ptr<state> _addr_s, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref ssa.Value x = ref _addr_x.val;
    ref types.Type ft = ref _addr_ft.val;
    ref types.Type tt = ref _addr_tt.val;

    return _addr_s.floatToUint(_addr_f64_u32, n, x, ft, tt)!;
}

private static ptr<ssa.Value> floatToUint(this ptr<state> _addr_s, ptr<f2uCvtTab> _addr_cvttab, ir.Node n, ptr<ssa.Value> _addr_x, ptr<types.Type> _addr_ft, ptr<types.Type> _addr_tt) {
    ref state s = ref _addr_s.val;
    ref f2uCvtTab cvttab = ref _addr_cvttab.val;
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
    var cmp = s.newValue2(cvttab.ltf, types.Types[types.TBOOL], x, cutoff);
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
    return _addr_s.variable(n, n.Type())!;
}

// dottype generates SSA for a type assertion node.
// commaok indicates whether to panic or return a bool.
// If commaok is false, resok will be nil.
private static (ptr<ssa.Value>, ptr<ssa.Value>) dottype(this ptr<state> _addr_s, ptr<ir.TypeAssertExpr> _addr_n, bool commaok) {
    ptr<ssa.Value> res = default!;
    ptr<ssa.Value> resok = default!;
    ref state s = ref _addr_s.val;
    ref ir.TypeAssertExpr n = ref _addr_n.val;

    var iface = s.expr(n.X); // input interface
    var target = s.reflectType(n.Type()); // target type
    var byteptr = s.f.Config.Types.BytePtr;

    if (n.Type().IsInterface()) {
        if (n.Type().IsEmptyInterface()) { 
            // Converting to an empty interface.
            // Input could be an empty or nonempty interface.
            if (@base.Debug.TypeAssert > 0) {
                @base.WarnfAt(n.Pos(), "type assertion inlined");
            } 

            // Get itab/type field from input.
            var itab = s.newValue1(ssa.OpITab, byteptr, iface); 
            // Conversion succeeds iff that field is not nil.
            var cond = s.newValue2(ssa.OpNeqPtr, types.Types[types.TBOOL], itab, s.constNil(byteptr));

            if (n.X.Type().IsEmptyInterface() && commaok) { 
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

            if (!commaok) { 
                // On failure, panic by calling panicnildottype.
                s.startBlock(bFail);
                s.rtcall(ir.Syms.Panicnildottype, false, null, target); 

                // On success, return (perhaps modified) input interface.
                s.startBlock(bOk);
                if (n.X.Type().IsEmptyInterface()) {
                    res = iface; // Use input interface unchanged.
                    return ;
                } 
                // Load type out of itab, build interface with existing idata.
                var off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(types.PtrSize), itab);
                var typ = s.load(byteptr, off);
                var idata = s.newValue1(ssa.OpIData, byteptr, iface);
                res = s.newValue2(ssa.OpIMake, n.Type(), typ, idata);
                return ;
            }
            s.startBlock(bOk); 
            // nonempty -> empty
            // Need to load type from itab
            off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(types.PtrSize), itab);
            s.vars[typVar] = s.load(byteptr, off);
            s.endBlock(); 

            // itab is nil, might as well use that as the nil result.
            s.startBlock(bFail);
            s.vars[typVar] = itab;
            s.endBlock(); 

            // Merge point.
            var bEnd = s.f.NewBlock(ssa.BlockPlain);
            bOk.AddEdgeTo(bEnd);
            bFail.AddEdgeTo(bEnd);
            s.startBlock(bEnd);
            idata = s.newValue1(ssa.OpIData, byteptr, iface);
            res = s.newValue2(ssa.OpIMake, n.Type(), s.variable(typVar, byteptr), idata);
            resok = cond;
            delete(s.vars, typVar);
            return ;
        }
        if (@base.Debug.TypeAssert > 0) {
            @base.WarnfAt(n.Pos(), "type assertion not inlined");
        }
        if (!commaok) {
            var fn = ir.Syms.AssertI2I;
            if (n.X.Type().IsEmptyInterface()) {
                fn = ir.Syms.AssertE2I;
            }
            var data = s.newValue1(ssa.OpIData, types.Types[types.TUNSAFEPTR], iface);
            var tab = s.newValue1(ssa.OpITab, byteptr, iface);
            tab = s.rtcall(fn, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { byteptr }), target, tab)[0];
            return (_addr_s.newValue2(ssa.OpIMake, n.Type(), tab, data)!, _addr_null!);
        }
        fn = ir.Syms.AssertI2I2;
        if (n.X.Type().IsEmptyInterface()) {
            fn = ir.Syms.AssertE2I2;
        }
        res = s.rtcall(fn, true, new slice<ptr<types.Type>>(new ptr<types.Type>[] { n.Type() }), target, iface)[0];
        resok = s.newValue2(ssa.OpNeqInter, types.Types[types.TBOOL], res, s.constInterface(n.Type()));
        return ;
    }
    if (@base.Debug.TypeAssert > 0) {
        @base.WarnfAt(n.Pos(), "type assertion inlined");
    }
    var direct = types.IsDirectIface(n.Type());
    itab = s.newValue1(ssa.OpITab, byteptr, iface); // type word of interface
    if (@base.Debug.TypeAssert > 0) {
        @base.WarnfAt(n.Pos(), "type assertion inlined");
    }
    ptr<ssa.Value> targetITab;
    if (n.X.Type().IsEmptyInterface()) { 
        // Looking for pointer to target type.
        targetITab = target;
    }
    else
 { 
        // Looking for pointer to itab for target type and source interface.
        targetITab = s.expr(n.Itab);
    }
    ir.Node tmp = default; // temporary for use with large types
    ptr<ssa.Value> addr; // address of tmp
    if (commaok && !TypeOK(_addr_n.Type())) { 
        // unSSAable type, use temporary.
        // TODO: get rid of some of these temporaries.
        tmp, addr = s.temp(n.Pos(), n.Type());
    }
    cond = s.newValue2(ssa.OpEqPtr, types.Types[types.TBOOL], itab, targetITab);
    b = s.endBlock();
    b.Kind = ssa.BlockIf;
    b.SetControl(cond);
    b.Likely = ssa.BranchLikely;

    bOk = s.f.NewBlock(ssa.BlockPlain);
    bFail = s.f.NewBlock(ssa.BlockPlain);
    b.AddEdgeTo(bOk);
    b.AddEdgeTo(bFail);

    if (!commaok) { 
        // on failure, panic by calling panicdottype
        s.startBlock(bFail);
        var taddr = s.reflectType(n.X.Type());
        if (n.X.Type().IsEmptyInterface()) {
            s.rtcall(ir.Syms.PanicdottypeE, false, null, itab, target, taddr);
        }
        else
 {
            s.rtcall(ir.Syms.PanicdottypeI, false, null, itab, target, taddr);
        }
        s.startBlock(bOk);
        if (direct) {
            return (_addr_s.newValue1(ssa.OpIData, n.Type(), iface)!, _addr_null!);
        }
        var p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type()), iface);
        return (_addr_s.load(n.Type(), p)!, _addr_null!);
    }
    bEnd = s.f.NewBlock(ssa.BlockPlain); 
    // Note that we need a new valVar each time (unlike okVar where we can
    // reuse the variable) because it might have a different type every time.
    var valVar = ssaMarker("val"); 

    // type assertion succeeded
    s.startBlock(bOk);
    if (tmp == null) {
        if (direct) {
            s.vars[valVar] = s.newValue1(ssa.OpIData, n.Type(), iface);
        }
        else
 {
            p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type()), iface);
            s.vars[valVar] = s.load(n.Type(), p);
        }
    }
    else
 {
        p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type()), iface);
        s.move(n.Type(), addr, p);
    }
    s.vars[okVar] = s.constBool(true);
    s.endBlock();
    bOk.AddEdgeTo(bEnd); 

    // type assertion failed
    s.startBlock(bFail);
    if (tmp == null) {
        s.vars[valVar] = s.zeroVal(n.Type());
    }
    else
 {
        s.zero(n.Type(), addr);
    }
    s.vars[okVar] = s.constBool(false);
    s.endBlock();
    bFail.AddEdgeTo(bEnd); 

    // merge point
    s.startBlock(bEnd);
    if (tmp == null) {
        res = s.variable(valVar, n.Type());
        delete(s.vars, valVar);
    }
    else
 {
        res = s.load(n.Type(), addr);
        s.vars[memVar] = s.newValue1A(ssa.OpVarKill, types.TypeMem, tmp._<ptr<ir.Name>>(), s.mem());
    }
    resok = s.variable(okVar, types.Types[types.TBOOL]);
    delete(s.vars, okVar);
    return (_addr_res!, _addr_resok!);
}

// temp allocates a temp of type t at position pos
private static (ptr<ir.Name>, ptr<ssa.Value>) temp(this ptr<state> _addr_s, src.XPos pos, ptr<types.Type> _addr_t) {
    ptr<ir.Name> _p0 = default!;
    ptr<ssa.Value> _p0 = default!;
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var tmp = typecheck.TempAt(pos, s.curfn, t);
    s.vars[memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, tmp, s.mem());
    var addr = s.addr(tmp);
    return (_addr_tmp!, _addr_addr!);
}

// variable returns the value of a variable at the current location.
private static ptr<ssa.Value> variable(this ptr<state> _addr_s, ir.Node n, ptr<types.Type> _addr_t) {
    ref state s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var v = s.vars[n];
    if (v != null) {
        return _addr_v!;
    }
    v = s.fwdVars[n];
    if (v != null) {
        return _addr_v!;
    }
    if (s.curBlock == s.f.Entry) { 
        // No variable should be live at entry.
        s.Fatalf("Value live at entry. It shouldn't be. func %s, node %v, value %v", s.f.Name, n, v);
    }
    v = s.newValue0A(ssa.OpFwdRef, t, new fwdRefAux(N:n));
    s.fwdVars[n] = v;
    if (n.Op() == ir.ONAME) {
        s.addNamedValue(n._<ptr<ir.Name>>(), v);
    }
    return _addr_v!;
}

private static ptr<ssa.Value> mem(this ptr<state> _addr_s) {
    ref state s = ref _addr_s.val;

    return _addr_s.variable(memVar, types.TypeMem)!;
}

private static void addNamedValue(this ptr<state> _addr_s, ptr<ir.Name> _addr_n, ptr<ssa.Value> _addr_v) {
    ref state s = ref _addr_s.val;
    ref ir.Name n = ref _addr_n.val;
    ref ssa.Value v = ref _addr_v.val;

    if (n.Class == ir.Pxxx) { 
        // Don't track our marker nodes (memVar etc.).
        return ;
    }
    if (ir.IsAutoTmp(n)) { 
        // Don't track temporary variables.
        return ;
    }
    if (n.Class == ir.PPARAMOUT) { 
        // Don't track named output values.  This prevents return values
        // from being assigned too early. See #14591 and #14762. TODO: allow this.
        return ;
    }
    ref ssa.LocalSlot loc = ref heap(new ssa.LocalSlot(N:n,Type:n.Type(),Off:0), out ptr<ssa.LocalSlot> _addr_loc);
    var (values, ok) = s.f.NamedValues[loc];
    if (!ok) {
        s.f.Names = append(s.f.Names, _addr_loc);
        _addr_s.f.CanonicalLocalSlots[loc] = _addr_loc;
        s.f.CanonicalLocalSlots[loc] = ref _addr_s.f.CanonicalLocalSlots[loc].val;
    }
    s.f.NamedValues[loc] = append(values, v);
}

// Branch is an unresolved branch.
public partial struct Branch {
    public ptr<obj.Prog> P; // branch instruction
    public ptr<ssa.Block> B; // target
}

// State contains state needed during Prog generation.
public partial struct State {
    public obj.ABI ABI;
    public ptr<objw.Progs> pp; // Branches remembers all the branch instructions we've seen
// and where they would like to go.
    public slice<Branch> Branches; // bstart remembers where each block starts (indexed by block ID)
    public slice<ptr<obj.Prog>> bstart;
    public long maxarg; // largest frame size for arguments to calls made by the function

// Map from GC safe points to liveness index, generated by
// liveness analysis.
    public liveness.Map livenessMap; // partLiveArgs includes arguments that may be partially live, for which we
// need to generate instructions that spill the argument registers.
    public map<ptr<ir.Name>, bool> partLiveArgs; // lineRunStart records the beginning of the current run of instructions
// within a single block sharing the same line number
// Used to move statement marks to the beginning of such runs.
    public ptr<obj.Prog> lineRunStart; // wasm: The number of values on the WebAssembly stack. This is only used as a safeguard.
    public nint OnWasmStackSkipped;
}

private static ptr<obj.FuncInfo> FuncInfo(this ptr<State> _addr_s) {
    ref State s = ref _addr_s.val;

    return _addr_s.pp.CurFunc.LSym.Func()!;
}

// Prog appends a new Prog.
private static ptr<obj.Prog> Prog(this ptr<State> _addr_s, obj.As @as) {
    ref State s = ref _addr_s.val;

    var p = s.pp.Prog(as);
    if (objw.LosesStmtMark(as)) {
        return _addr_p!;
    }
    if (s.lineRunStart == null || s.lineRunStart.Pos.Line() != p.Pos.Line()) {
        s.lineRunStart = p;
    }
    else if (p.Pos.IsStmt() == src.PosIsStmt) {
        s.lineRunStart.Pos = s.lineRunStart.Pos.WithIsStmt();
        p.Pos = p.Pos.WithNotStmt();
    }
    return _addr_p!;
}

// Pc returns the current Prog.
private static ptr<obj.Prog> Pc(this ptr<State> _addr_s) {
    ref State s = ref _addr_s.val;

    return _addr_s.pp.Next!;
}

// SetPos sets the current source position.
private static void SetPos(this ptr<State> _addr_s, src.XPos pos) {
    ref State s = ref _addr_s.val;

    s.pp.Pos = pos;
}

// Br emits a single branch instruction and returns the instruction.
// Not all architectures need the returned instruction, but otherwise
// the boilerplate is common to all.
private static ptr<obj.Prog> Br(this ptr<State> _addr_s, obj.As op, ptr<ssa.Block> _addr_target) {
    ref State s = ref _addr_s.val;
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
private static void DebugFriendlySetPosFrom(this ptr<State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpPhi || v.Op == ssa.OpCopy || v.Op == ssa.OpLoadReg || v.Op == ssa.OpStoreReg) 
        // These are not statements
        s.SetPos(v.Pos.WithNotStmt());
    else 
        var p = v.Pos;
        if (p != src.NoXPos) { 
            // If the position is defined, update the position.
            // Also convert default IsStmt to NotStmt; only
            // explicit statement boundaries should appear
            // in the generated code.
            if (p.IsStmt() != src.PosIsStmt) {
                p = p.WithNotStmt(); 
                // Calls use the pos attached to v, but copy the statement mark from State
            }
            s.SetPos(p);
        }
        else
 {
            s.SetPos(s.pp.Pos.WithNotStmt());
        }
    }

// emit argument info (locations on stack) for traceback.
private static void emitArgInfo(ptr<ssafn> _addr_e, ptr<ssa.Func> _addr_f, ptr<objw.Progs> _addr_pp) {
    ref ssafn e = ref _addr_e.val;
    ref ssa.Func f = ref _addr_f.val;
    ref objw.Progs pp = ref _addr_pp.val;

    var ft = e.curfn.Type();
    if (ft.NumRecvs() == 0 && ft.NumParams() == 0) {
        return ;
    }
    var x = EmitArgInfo(_addr_e.curfn, _addr_f.OwnAux.ABIInfo());
    e.curfn.LSym.Func().ArgInfo = x; 

    // Emit a funcdata pointing at the arg info data.
    var p = pp.Prog(obj.AFUNCDATA);
    p.From.SetConst(objabi.FUNCDATA_ArgInfo);
    p.To.Type = obj.TYPE_MEM;
    p.To.Name = obj.NAME_EXTERN;
    p.To.Sym = x;
}

// emit argument info (locations on stack) of f for traceback.
public static ptr<obj.LSym> EmitArgInfo(ptr<ir.Func> _addr_f, ptr<abi.ABIParamResultInfo> _addr_abiInfo) {
    ref ir.Func f = ref _addr_f.val;
    ref abi.ABIParamResultInfo abiInfo = ref _addr_abiInfo.val;

    var x = @base.Ctxt.Lookup(fmt.Sprintf("%s.arginfo%d", f.LSym.Name, f.ABI));

    var PtrSize = int64(types.PtrSize);
    var uintptrTyp = types.Types[types.TUINTPTR];

    Func<ptr<types.Type>, bool> isAggregate = t => _addr_t.IsStruct() || t.IsArray() || t.IsComplex() || t.IsInterface() || t.IsString() || t.IsSlice()!; 

    // Populate the data.
    // The data is a stream of bytes, which contains the offsets and sizes of the
    // non-aggregate arguments or non-aggregate fields/elements of aggregate-typed
    // arguments, along with special "operators". Specifically,
    // - for each non-aggrgate arg/field/element, its offset from FP (1 byte) and
    //   size (1 byte)
    // - special operators:
    //   - 0xff - end of sequence
    //   - 0xfe - print { (at the start of an aggregate-typed argument)
    //   - 0xfd - print } (at the end of an aggregate-typed argument)
    //   - 0xfc - print ... (more args/fields/elements)
    //   - 0xfb - print _ (offset too large)
    // These constants need to be in sync with runtime.traceback.go:printArgs.
    const nuint _endSeq = 0xff;
    const nuint _startAgg = 0xfe;
    const nuint _endAgg = 0xfd;
    const nuint _dotdotdot = 0xfc;
    const nuint _offsetTooLarge = 0xfb;
    const nuint _special = 0xf0; // above this are operators, below this are ordinary offsets

    const nint limit = 10; // print no more than 10 args/components
    const nint maxDepth = 5; // no more than 5 layers of nesting

    // maxLen is a (conservative) upper bound of the byte stream length. For
    // each arg/component, it has no more than 2 bytes of data (size, offset),
    // and no more than one {, }, ... at each level (it cannot have both the
    // data and ... unless it is the last one, just be conservative). Plus 1
    // for _endSeq.
    const var maxLen = (maxDepth * 3 + 2) * limit + 1;

    nint wOff = 0;
    nint n = 0;
    Action<byte> writebyte = o => {
        wOff = objw.Uint8(x, wOff, o);
    }; 

    // Write one non-aggrgate arg/field/element.
    Action<long, long> write1 = (sz, offset) => {
        if (offset >= _special) {
            writebyte(_offsetTooLarge);
        }
        else
 {
            writebyte(uint8(offset));
            writebyte(uint8(sz));
        }
        n++;
    }; 

    // Visit t recursively and write it out.
    // Returns whether to continue visiting.
    Func<long, ptr<types.Type>, nint, bool> visitType = default;
    visitType = (baseOffset, t, depth) => {
        if (n >= limit) {
            writebyte(_dotdotdot);
            return _addr_false!;
        }
        if (!isAggregate(t)) {
            write1(t.Size(), baseOffset);
            return _addr_true!;
        }
        writebyte(_startAgg);
        depth++;
        if (depth >= maxDepth) {
            writebyte(_dotdotdot);
            writebyte(_endAgg);
            n++;
            return _addr_true!;
        }

        if (t.IsInterface() || t.IsString()) 
            _ = visitType(baseOffset, uintptrTyp, depth) && visitType(baseOffset + PtrSize, uintptrTyp, depth);
        else if (t.IsSlice()) 
            _ = visitType(baseOffset, uintptrTyp, depth) && visitType(baseOffset + PtrSize, uintptrTyp, depth) && visitType(baseOffset + PtrSize * 2, uintptrTyp, depth);
        else if (t.IsComplex()) 
            _ = visitType(baseOffset, types.FloatForComplex(t), depth) && visitType(baseOffset + t.Size() / 2, types.FloatForComplex(t), depth);
        else if (t.IsArray()) 
            if (t.NumElem() == 0) {
                n++; // {} counts as a component
                break;
            }
            for (var i = int64(0); i < t.NumElem(); i++) {
                if (!visitType(baseOffset, t.Elem(), depth)) {
                    break;
                }
                baseOffset += t.Elem().Size();
            }
        else if (t.IsStruct()) 
            if (t.NumFields() == 0) {
                n++; // {} counts as a component
                break;
            }
            foreach (var (_, field) in t.Fields().Slice()) {
                if (!visitType(baseOffset + field.Offset, field.Type, depth)) {
                    break;
                }
            }
                writebyte(_endAgg);
        return _addr_true!;
    };

    foreach (var (_, a) in abiInfo.InParams()) {
        if (!visitType(a.FrameOffset(abiInfo), a.Type, 0)) {
            break;
        }
    }    writebyte(_endSeq);
    if (wOff > maxLen) {
        @base.Fatalf("ArgInfo too large");
    }
    return _addr_x!;
}

// genssa appends entries to pp for each instruction in f.
private static void genssa(ptr<ssa.Func> _addr_f, ptr<objw.Progs> _addr_pp) {
    ref ssa.Func f = ref _addr_f.val;
    ref objw.Progs pp = ref _addr_pp.val;

    ref State s = ref heap(out ptr<State> _addr_s);
    s.ABI = f.OwnAux.Fn.ABI();

    ptr<ssafn> e = f.Frontend()._<ptr<ssafn>>();

    s.livenessMap, s.partLiveArgs = liveness.Compute(e.curfn, f, e.stkptrsize, pp);
    emitArgInfo(e, _addr_f, _addr_pp);

    var openDeferInfo = e.curfn.LSym.Func().OpenCodedDeferInfo;
    if (openDeferInfo != null) { 
        // This function uses open-coded defers -- write out the funcdata
        // info that we computed at the end of genssa.
        var p = pp.Prog(obj.AFUNCDATA);
        p.From.SetConst(objabi.FUNCDATA_OpenCodedDeferInfo);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = openDeferInfo;
    }
    s.bstart = make_slice<ptr<obj.Prog>>(f.NumBlocks());
    s.pp = pp;
    map<ptr<obj.Prog>, ptr<ssa.Value>> progToValue = default;
    map<ptr<obj.Prog>, ptr<ssa.Block>> progToBlock = default;
    slice<ptr<obj.Prog>> valueToProgAfter = default; // The first Prog following computation of a value v; v is visible at this point.
    if (f.PrintOrHtmlSSA) {
        progToValue = make_map<ptr<obj.Prog>, ptr<ssa.Value>>(f.NumValues());
        progToBlock = make_map<ptr<obj.Prog>, ptr<ssa.Block>>(f.NumBlocks());
        f.Logf("genssa %s\n", f.Name);
        progToBlock[s.pp.Next] = f.Blocks[0];
    }
    if (@base.Ctxt.Flag_locationlists) {
        if (cap(f.Cache.ValueToProgAfter) < f.NumValues()) {
            f.Cache.ValueToProgAfter = make_slice<ptr<obj.Prog>>(f.NumValues());
        }
        valueToProgAfter = f.Cache.ValueToProgAfter[..(int)f.NumValues()];
        {
            var i__prev1 = i;

            foreach (var (__i) in valueToProgAfter) {
                i = __i;
                valueToProgAfter[i] = null;
            }

            i = i__prev1;
        }
    }
    var firstPos = src.NoXPos;
    {
        var v__prev1 = v;

        foreach (var (_, __v) in f.Entry.Values) {
            v = __v;
            if (v.Pos.IsStmt() == src.PosIsStmt) {
                firstPos = v.Pos;
                v.Pos = firstPos.WithDefaultStmt();
                break;
            }
        }
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

        foreach (var (__i, __b) in f.Blocks) {
            i = __i;
            b = __b;
            s.bstart[b.ID] = s.pp.Next;
            s.lineRunStart = null; 

            // Attach a "default" liveness info. Normally this will be
            // overwritten in the Values loop below for each Value. But
            // for an empty block this will be used for its control
            // instruction. We won't use the actual liveness map on a
            // control instruction. Just mark it something that is
            // preemptible, unless this function is "all unsafe".
            s.pp.NextLive = new objw.LivenessIndex(StackMapIndex:-1,IsUnsafePoint:liveness.IsUnsafe(f)); 

            // Emit values in block
            Arch.SSAMarkMoves(_addr_s, b);
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    var x = s.pp.Next;
                    s.DebugFriendlySetPosFrom(v);

                    if (v.Op.ResultInArg0() && v.ResultReg() != v.Args[0].Reg()) {
                        v.Fatalf("input[0] and output not in same register %s", v.LongString());
                    }

                    if (v.Op == ssa.OpInitMem)                     else if (v.Op == ssa.OpArg)                     else if (v.Op == ssa.OpSP || v.Op == ssa.OpSB)                     else if (v.Op == ssa.OpSelect0 || v.Op == ssa.OpSelect1 || v.Op == ssa.OpSelectN || v.Op == ssa.OpMakeResult)                     else if (v.Op == ssa.OpGetG)                     else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive || v.Op == ssa.OpVarKill)                     else if (v.Op == ssa.OpPhi) 
                        CheckLoweredPhi(_addr_v);
                    else if (v.Op == ssa.OpConvert) 
                        // nothing to do; no-op conversion for liveness
                        if (v.Args[0].Reg() != v.Reg()) {
                            v.Fatalf("OpConvert should be a no-op: %s; %s", v.Args[0].LongString(), v.LongString());
                        }
                    else if (v.Op == ssa.OpInlMark) 
                        p = Arch.Ginsnop(s.pp);
                        if (inlMarks == null) {
                            inlMarks = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<obj.Prog>, int>{};
                            inlMarksByPos = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<src.XPos, slice<ptr<obj.Prog>>>{};
                        }
                        inlMarks[p] = v.AuxInt32();
                        inlMarkList = append(inlMarkList, p);
                        var pos = v.Pos.AtColumn1();
                        inlMarksByPos[pos] = append(inlMarksByPos[pos], p);
                    else 
                        // Special case for first line in function; move it to the start (which cannot be a register-valued instruction)
                        if (firstPos != src.NoXPos && v.Op != ssa.OpArgIntReg && v.Op != ssa.OpArgFloatReg && v.Op != ssa.OpLoadReg && v.Op != ssa.OpStoreReg) {
                            s.SetPos(firstPos);
                            firstPos = src.NoXPos;
                        } 
                        // Attach this safe point to the next
                        // instruction.
                        s.pp.NextLive = s.livenessMap.Get(v); 

                        // let the backend handle it
                        Arch.SSAGenValue(_addr_s, v);
                                        if (@base.Ctxt.Flag_locationlists) {
                        valueToProgAfter[v.ID] = s.pp.Next;
                    }
                    if (f.PrintOrHtmlSSA) {
                        while (x != s.pp.Next) {
                            progToValue[x] = v;
                            x = x.Link;
                        }
                    }
                } 
                // If this is an empty infinite loop, stick a hardware NOP in there so that debuggers are less confused.

                v = v__prev2;
            }

            if (s.bstart[b.ID] == s.pp.Next && len(b.Succs) == 1 && b.Succs[0].Block() == b) {
                p = Arch.Ginsnop(s.pp);
                p.Pos = p.Pos.WithIsStmt();
                if (b.Pos == src.NoXPos) {
                    b.Pos = p.Pos; // It needs a file, otherwise a no-file non-zero line causes confusion.  See #35652.
                    if (b.Pos == src.NoXPos) {
                        b.Pos = pp.Text.Pos; // Sometimes p.Pos is empty.  See #35695.
                    }
                }
                b.Pos = b.Pos.WithBogusLine(); // Debuggers are not good about infinite loops, force a change in line number
            } 
            // Emit control flow instructions for block
            ptr<ssa.Block> next;
            if (i < len(f.Blocks) - 1 && @base.Flag.N == 0) { 
                // If -N, leave next==nil so every block with successors
                // ends in a JMP (except call blocks - plive doesn't like
                // select{send,recv} followed by a JMP call).  Helps keep
                // line numbers for otherwise empty blocks.
                next = f.Blocks[i + 1];
            }
            x = s.pp.Next;
            s.SetPos(b.Pos);
            Arch.SSAGenBlock(_addr_s, b, next);
            if (f.PrintOrHtmlSSA) {
                while (x != s.pp.Next) {
                    progToBlock[x] = b;
                    x = x.Link;
                }
            }
        }
        i = i__prev1;
        b = b__prev1;
    }

    if (f.Blocks[len(f.Blocks) - 1].Kind == ssa.BlockExit) { 
        // We need the return address of a panic call to
        // still be inside the function in question. So if
        // it ends in a call which doesn't return, add a
        // nop (which will never execute) after the call.
        Arch.Ginsnop(pp);
    }
    if (openDeferInfo != null) { 
        // When doing open-coded defers, generate a disconnected call to
        // deferreturn and a return. This will be used to during panic
        // recovery to unwind the stack and return back to the runtime.
        s.pp.NextLive = s.livenessMap.DeferReturn;
        p = pp.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ir.Syms.Deferreturn; 

        // Load results into registers. So when a deferred function
        // recovers a panic, it will return to caller with right results.
        // The results are already in memory, because they are not SSA'd
        // when the function has defers (see canSSAName).
        if (f.OwnAux.ABIInfo().OutRegistersUsed() != 0) {
            Arch.LoadRegResults(_addr_s, f);
        }
        pp.Prog(obj.ARET);
    }
    if (inlMarks != null) { 
        // We have some inline marks. Try to find other instructions we're
        // going to emit anyway, and use those instructions instead of the
        // inline marks.
        {
            var p__prev1 = p;

            p = pp.Text;

            while (p != null) {
                if (p.As == obj.ANOP || p.As == obj.AFUNCDATA || p.As == obj.APCDATA || p.As == obj.ATEXT || p.As == obj.APCALIGN || Arch.LinkArch.Family == sys.Wasm) { 
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

                    if (ok) { 
                        // Don't use inline marks themselves. We don't know
                        // whether they will be zero-sized or not yet.
                        continue;
                    }

                }
                pos = p.Pos.AtColumn1();
                s = inlMarksByPos[pos];
                if (len(s) == 0) {
                    continue;
                }
                foreach (var (_, m) in s) { 
                    // We found an instruction with the same source position as
                    // some of the inline marks.
                    // Use this instruction instead.
                    p.Pos = p.Pos.WithIsStmt(); // promote position to a statement
                    pp.CurFunc.LSym.Func().AddInlMark(p, inlMarks[m]); 
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

            foreach (var (_, __p) in inlMarkList) {
                p = __p;
                if (p.As != obj.ANOP) {
                    pp.CurFunc.LSym.Func().AddInlMark(p, inlMarks[p]);
                }
            }

            p = p__prev1;
        }
    }
    if (@base.Ctxt.Flag_locationlists) {
        ptr<ssa.FuncDebug> debugInfo;
        if (e.curfn.ABI == obj.ABIInternal && @base.Flag.N != 0) {
            debugInfo = ssa.BuildFuncDebugNoOptimized(@base.Ctxt, f, @base.Debug.LocationLists > 1, StackOffset);
        }
        else
 {
            debugInfo = ssa.BuildFuncDebug(@base.Ctxt, f, @base.Debug.LocationLists > 1, StackOffset);
        }
        e.curfn.DebugInfo = debugInfo;
        var bstart = s.bstart;
        var idToIdx = make_slice<nint>(f.NumBlocks());
        {
            var i__prev1 = i;
            var b__prev1 = b;

            foreach (var (__i, __b) in f.Blocks) {
                i = __i;
                b = __b;
                idToIdx[b.ID] = i;
            } 
            // Note that at this moment, Prog.Pc is a sequence number; it's
            // not a real PC until after assembly, so this mapping has to
            // be done later.

            i = i__prev1;
            b = b__prev1;
        }

        debugInfo.GetPC = (b, v) => {

            if (v == ssa.BlockStart.ID) 
                if (b == f.Entry.ID) {
                    return 0; // Start at the very beginning, at the assembler-generated prologue.
                    // this should only happen for function args (ssa.OpArg)
                }
                return bstart[b].Pc;
            else if (v == ssa.BlockEnd.ID) 
                var blk = f.Blocks[idToIdx[b]];
                var nv = len(blk.Values);
                return valueToProgAfter[blk.Values[nv - 1].ID].Pc;
            else if (v == ssa.FuncEnd.ID) 
                return e.curfn.LSym.Size;
            else 
                return valueToProgAfter[v].Pc;
                    };
    }
    foreach (var (_, br) in s.Branches) {
        br.P.To.SetTarget(s.bstart[br.B.ID]);
        if (br.P.Pos.IsStmt() != src.PosIsStmt) {
            br.P.Pos = br.P.Pos.WithNotStmt();
        }        {
            var v0 = br.B.FirstPossibleStmtValue();


            else if (v0 != null && v0.Pos.Line() == br.P.Pos.Line() && v0.Pos.IsStmt() == src.PosIsStmt) {
                br.P.Pos = br.P.Pos.WithNotStmt();
            }

        }
    }    if (e.log) { // spew to stdout
        @string filename = "";
        {
            var p__prev1 = p;

            p = pp.Text;

            while (p != null) {
                if (p.Pos.IsKnown() && p.InnermostFilename() != filename) {
                    filename = p.InnermostFilename();
                    f.Logf("# %s\n", filename);
                p = p.Link;
                }
                s = default;
                {
                    var v__prev2 = v;

                    var (v, ok) = progToValue[p];

                    if (ok) {
                        s = v.String();
                    }                    {
                        var b__prev3 = b;

                        var (b, ok) = progToBlock[p];


                        else if (ok) {
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
    if (f.HTMLWriter != null) { // spew to ssa.html
        bytes.Buffer buf = default;
        buf.WriteString("<code>");
        buf.WriteString("<dl class=\"ssa-gen\">");
        filename = "";
        {
            var p__prev1 = p;

            p = pp.Text;

            while (p != null) { 
                // Don't spam every line with the file name, which is often huge.
                // Only print changes, and "unknown" is not a change.
                if (p.Pos.IsKnown() && p.InnermostFilename() != filename) {
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

                    if (ok) {
                        buf.WriteString(v.HTML());
                    }                    {
                        var b__prev3 = b;

                        (b, ok) = progToBlock[p];


                        else if (ok) {
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
    defframe(_addr_s, e, _addr_f);

    f.HTMLWriter.Close();
    f.HTMLWriter = null;
}

private static void defframe(ptr<State> _addr_s, ptr<ssafn> _addr_e, ptr<ssa.Func> _addr_f) {
    ref State s = ref _addr_s.val;
    ref ssafn e = ref _addr_e.val;
    ref ssa.Func f = ref _addr_f.val;

    var pp = s.pp;

    var frame = types.Rnd(s.maxarg + e.stksize, int64(types.RegSize));
    if (Arch.PadFrame != null) {
        frame = Arch.PadFrame(frame);
    }
    pp.Text.To.Type = obj.TYPE_TEXTSIZE;
    pp.Text.To.Val = int32(types.Rnd(f.OwnAux.ArgWidth(), int64(types.RegSize)));
    pp.Text.To.Offset = frame;

    var p = pp.Text; 

    // Insert code to spill argument registers if the named slot may be partially
    // live. That is, the named slot is considered live by liveness analysis,
    // (because a part of it is live), but we may not spill all parts into the
    // slot. This can only happen with aggregate-typed arguments that are SSA-able
    // and not address-taken (for non-SSA-able or address-taken arguments we always
    // spill upfront).
    // Note: spilling is unnecessary in the -N/no-optimize case, since all values
    // will be considered non-SSAable and spilled up front.
    // TODO(register args) Make liveness more fine-grained to that partial spilling is okay.
    if (f.OwnAux.ABIInfo().InRegistersUsed() != 0 && @base.Flag.N == 0) { 
        // First, see if it is already spilled before it may be live. Look for a spill
        // in the entry block up to the first safepoint.
        private partial struct nameOff {
            public ptr<ir.Name> n;
            public long off;
        }
        var partLiveArgsSpilled = make_map<nameOff, bool>();
        foreach (var (_, v) in f.Entry.Values) {
            if (v.Op.IsCall()) {
                break;
            }
            if (v.Op != ssa.OpStoreReg || v.Args[0].Op != ssa.OpArgIntReg) {
                continue;
            }
            var (n, off) = ssa.AutoVar(v);
            if (n.Class != ir.PPARAM || n.Addrtaken() || !TypeOK(_addr_n.Type()) || !s.partLiveArgs[n]) {
                continue;
            }
            partLiveArgsSpilled[new nameOff(n,off)] = true;
        }        foreach (var (_, a) in f.OwnAux.ABIInfo().InParams()) {
            ptr<ir.Name> (n, ok) = a.Name._<ptr<ir.Name>>();
            if (!ok || n.Addrtaken() || !TypeOK(_addr_n.Type()) || !s.partLiveArgs[n] || len(a.Registers) <= 1) {
                continue;
            }
            var (rts, offs) = a.RegisterTypesAndOffsets();
            foreach (var (i) in a.Registers) {
                if (!rts[i].HasPointers()) {
                    continue;
                }
                if (partLiveArgsSpilled[new nameOff(n,offs[i])]) {
                    continue; // already spilled
                }
                var reg = ssa.ObjRegForAbiReg(a.Registers[i], f.Config);
                p = Arch.SpillArgReg(pp, p, f, rts[i], reg, n, offs[i]);
            }
        }
    }
    long lo = default;    long hi = default; 

    // Opaque state for backend to use. Current backends use it to
    // keep track of which helper registers have been zeroed.
 

    // Opaque state for backend to use. Current backends use it to
    // keep track of which helper registers have been zeroed.
    ref uint state = ref heap(out ptr<uint> _addr_state); 

    // Iterate through declarations. Autos are sorted in decreasing
    // frame offset order.
    {
        var n__prev1 = n;

        foreach (var (_, __n) in e.curfn.Dcl) {
            n = __n;
            if (!n.Needzero()) {
                continue;
            }
            if (n.Class != ir.PAUTO) {
                e.Fatalf(n.Pos(), "needzero class %d", n.Class);
            }
            if (n.Type().Size() % int64(types.PtrSize) != 0 || n.FrameOffset() % int64(types.PtrSize) != 0 || n.Type().Size() == 0) {
                e.Fatalf(n.Pos(), "var %L has size %d offset %d", n, n.Type().Size(), n.Offset_);
            }
            if (lo != hi && n.FrameOffset() + n.Type().Size() >= lo - int64(2 * types.RegSize)) { 
                // Merge with range we already have.
                lo = n.FrameOffset();
                continue;
            } 

            // Zero old range
            p = Arch.ZeroRange(pp, p, frame + lo, hi - lo, _addr_state); 

            // Set new range.
            lo = n.FrameOffset();
            hi = lo + n.Type().Size();
        }
        n = n__prev1;
    }

    Arch.ZeroRange(pp, p, frame + lo, hi - lo, _addr_state);
}

// For generating consecutive jump instructions to model a specific branching
public partial struct IndexJump {
    public obj.As Jump;
    public nint Index;
}

private static void oneJump(this ptr<State> _addr_s, ptr<ssa.Block> _addr_b, ptr<IndexJump> _addr_jump) {
    ref State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref IndexJump jump = ref _addr_jump.val;

    var p = s.Br(jump.Jump, b.Succs[jump.Index].Block());
    p.Pos = b.Pos;
}

// CombJump generates combinational instructions (2 at present) for a block jump,
// thereby the behaviour of non-standard condition codes could be simulated
private static void CombJump(this ptr<State> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next, ptr<array<array<IndexJump>>> _addr_jumps) {
    ref State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref ssa.Block next = ref _addr_next.val;
    ref array<array<IndexJump>> jumps = ref _addr_jumps.val;


    if (next == b.Succs[0].Block()) 
        s.oneJump(b, _addr_jumps[0][0]);
        s.oneJump(b, _addr_jumps[0][1]);
    else if (next == b.Succs[1].Block()) 
        s.oneJump(b, _addr_jumps[1][0]);
        s.oneJump(b, _addr_jumps[1][1]);
    else 
        ptr<obj.Prog> q;
        if (b.Likely != ssa.BranchUnlikely) {
            s.oneJump(b, _addr_jumps[1][0]);
            s.oneJump(b, _addr_jumps[1][1]);
            q = s.Br(obj.AJMP, b.Succs[1].Block());
        }
        else
 {
            s.oneJump(b, _addr_jumps[0][0]);
            s.oneJump(b, _addr_jumps[0][1]);
            q = s.Br(obj.AJMP, b.Succs[0].Block());
        }
        q.Pos = b.Pos;
    }

// AddAux adds the offset in the aux fields (AuxInt and Aux) of v to a.
public static void AddAux(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v) {
    ref obj.Addr a = ref _addr_a.val;
    ref ssa.Value v = ref _addr_v.val;

    AddAux2(_addr_a, _addr_v, v.AuxInt);
}
public static void AddAux2(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v, long offset) {
    ref obj.Addr a = ref _addr_a.val;
    ref ssa.Value v = ref _addr_v.val;

    if (a.Type != obj.TYPE_MEM && a.Type != obj.TYPE_ADDR) {
        v.Fatalf("bad AddAux addr %v", a);
    }
    a.Offset += offset; 

    // If no additional symbol offset, we're done.
    if (v.Aux == null) {
        return ;
    }
    switch (v.Aux.type()) {
        case ptr<ssa.AuxCall> n:
            a.Name = obj.NAME_EXTERN;
            a.Sym = n.Fn;
            break;
        case ptr<obj.LSym> n:
            a.Name = obj.NAME_EXTERN;
            a.Sym = n;
            break;
        case ptr<ir.Name> n:
            if (n.Class == ir.PPARAM || (n.Class == ir.PPARAMOUT && !n.IsOutputParamInRegisters())) {
                a.Name = obj.NAME_PARAM;
                a.Sym = ir.Orig(n)._<ptr<ir.Name>>().Linksym();
                a.Offset += n.FrameOffset();
                break;
            }
            a.Name = obj.NAME_AUTO;
            if (n.Class == ir.PPARAMOUT) {
                a.Sym = ir.Orig(n)._<ptr<ir.Name>>().Linksym();
            }
            else
 {
                a.Sym = n.Linksym();
            }
            a.Offset += n.FrameOffset();
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
private static ptr<ssa.Value> extendIndex(this ptr<state> _addr_s, ptr<ssa.Value> _addr_idx, ptr<ssa.Value> _addr_len, ssa.BoundsKind kind, bool bounded) {
    ref state s = ref _addr_s.val;
    ref ssa.Value idx = ref _addr_idx.val;
    ref ssa.Value len = ref _addr_len.val;

    var size = idx.Type.Size();
    if (size == s.config.PtrSize) {
        return _addr_idx!;
    }
    if (size > s.config.PtrSize) { 
        // truncate 64-bit indexes on 32-bit pointer archs. Test the
        // high word and branch to out-of-bounds failure if it is not 0.
        ptr<ssa.Value> lo;
        if (idx.Type.IsSigned()) {
            lo = s.newValue1(ssa.OpInt64Lo, types.Types[types.TINT], idx);
        }
        else
 {
            lo = s.newValue1(ssa.OpInt64Lo, types.Types[types.TUINT], idx);
        }
        if (bounded || @base.Flag.B != 0) {
            return _addr_lo!;
        }
        var bNext = s.f.NewBlock(ssa.BlockPlain);
        var bPanic = s.f.NewBlock(ssa.BlockExit);
        var hi = s.newValue1(ssa.OpInt64Hi, types.Types[types.TUINT32], idx);
        var cmp = s.newValue2(ssa.OpEq32, types.Types[types.TBOOL], hi, s.constInt32(types.Types[types.TUINT32], 0));
        if (!idx.Type.IsSigned()) {

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
    ssa.Op op = default;
    if (idx.Type.IsSigned()) {
        switch (10 * size + s.config.PtrSize) {
            case 14: 
                op = ssa.OpSignExt8to32;
                break;
            case 18: 
                op = ssa.OpSignExt8to64;
                break;
            case 24: 
                op = ssa.OpSignExt16to32;
                break;
            case 28: 
                op = ssa.OpSignExt16to64;
                break;
            case 48: 
                op = ssa.OpSignExt32to64;
                break;
            default: 
                s.Fatalf("bad signed index extension %s", idx.Type);
                break;
        }
    }
    else
 {
        switch (10 * size + s.config.PtrSize) {
            case 14: 
                op = ssa.OpZeroExt8to32;
                break;
            case 18: 
                op = ssa.OpZeroExt8to64;
                break;
            case 24: 
                op = ssa.OpZeroExt16to32;
                break;
            case 28: 
                op = ssa.OpZeroExt16to64;
                break;
            case 48: 
                op = ssa.OpZeroExt32to64;
                break;
            default: 
                s.Fatalf("bad unsigned index extension %s", idx.Type);
                break;
        }
    }
    return _addr_s.newValue1(op, types.Types[types.TINT], idx)!;
}

// CheckLoweredPhi checks that regalloc and stackalloc correctly handled phi values.
// Called during ssaGenValue.
public static void CheckLoweredPhi(ptr<ssa.Value> _addr_v) {
    ref ssa.Value v = ref _addr_v.val;

    if (v.Op != ssa.OpPhi) {
        v.Fatalf("CheckLoweredPhi called with non-phi value: %v", v.LongString());
    }
    if (v.Type.IsMemory()) {
        return ;
    }
    var f = v.Block.Func;
    var loc = f.RegAlloc[v.ID];
    foreach (var (_, a) in v.Args) {
        {
            var aloc = f.RegAlloc[a.ID];

            if (aloc != loc) { // TODO: .Equal() instead?
                v.Fatalf("phi arg at different location than phi: %v @ %s, but arg %v @ %s\n%s\n", v, loc, a, aloc, v.Block.Func);
            }

        }
    }
}

// CheckLoweredGetClosurePtr checks that v is the first instruction in the function's entry block,
// except for incoming in-register arguments.
// The output of LoweredGetClosurePtr is generally hardwired to the correct register.
// That register contains the closure pointer on closure entry.
public static void CheckLoweredGetClosurePtr(ptr<ssa.Value> _addr_v) {
    ref ssa.Value v = ref _addr_v.val;

    var entry = v.Block.Func.Entry;
    if (entry != v.Block) {
        @base.Fatalf("in %s, badly placed LoweredGetClosurePtr: %v %v", v.Block.Func.Name, v.Block, v);
    }
    foreach (var (_, w) in entry.Values) {
        if (w == v) {
            break;
        }

        if (w.Op == ssa.OpArgIntReg || w.Op == ssa.OpArgFloatReg)         else 
            @base.Fatalf("in %s, badly placed LoweredGetClosurePtr: %v %v", v.Block.Func.Name, v.Block, v);
            }
}

// CheckArgReg ensures that v is in the function's entry block.
public static void CheckArgReg(ptr<ssa.Value> _addr_v) {
    ref ssa.Value v = ref _addr_v.val;

    var entry = v.Block.Func.Entry;
    if (entry != v.Block) {
        @base.Fatalf("in %s, badly placed ArgIReg or ArgFReg: %v %v", v.Block.Func.Name, v.Block, v);
    }
}

public static void AddrAuto(ptr<obj.Addr> _addr_a, ptr<ssa.Value> _addr_v) {
    ref obj.Addr a = ref _addr_a.val;
    ref ssa.Value v = ref _addr_v.val;

    var (n, off) = ssa.AutoVar(v);
    a.Type = obj.TYPE_MEM;
    a.Sym = n.Linksym();
    a.Reg = int16(Arch.REGSP);
    a.Offset = n.FrameOffset() + off;
    if (n.Class == ir.PPARAM || (n.Class == ir.PPARAMOUT && !n.IsOutputParamInRegisters())) {
        a.Name = obj.NAME_PARAM;
    }
    else
 {
        a.Name = obj.NAME_AUTO;
    }
}

// Call returns a new CALL instruction for the SSA value v.
// It uses PrepareCall to prepare the call.
private static ptr<obj.Prog> Call(this ptr<State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;

    var pPosIsStmt = s.pp.Pos.IsStmt(); // The statement-ness fo the call comes from ssaGenState
    s.PrepareCall(v);

    var p = s.Prog(obj.ACALL);
    if (pPosIsStmt == src.PosIsStmt) {
        p.Pos = v.Pos.WithIsStmt();
    }
    else
 {
        p.Pos = v.Pos.WithNotStmt();
    }
    {
        ptr<ssa.AuxCall> (sym, ok) = v.Aux._<ptr<ssa.AuxCall>>();

        if (ok && sym.Fn != null) {
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = sym.Fn;
        }
        else
 { 
            // TODO(mdempsky): Can these differences be eliminated?

            if (Arch.LinkArch.Family == sys.AMD64 || Arch.LinkArch.Family == sys.I386 || Arch.LinkArch.Family == sys.PPC64 || Arch.LinkArch.Family == sys.RISCV64 || Arch.LinkArch.Family == sys.S390X || Arch.LinkArch.Family == sys.Wasm) 
                p.To.Type = obj.TYPE_REG;
            else if (Arch.LinkArch.Family == sys.ARM || Arch.LinkArch.Family == sys.ARM64 || Arch.LinkArch.Family == sys.MIPS || Arch.LinkArch.Family == sys.MIPS64) 
                p.To.Type = obj.TYPE_MEM;
            else 
                @base.Fatalf("unknown indirect call family");
                        p.To.Reg = v.Args[0].Reg();
        }
    }
    return _addr_p!;
}

// PrepareCall prepares to emit a CALL instruction for v and does call-related bookkeeping.
// It must be called immediately before emitting the actual CALL instruction,
// since it emits PCDATA for the stack map at the call (calls are safe points).
private static void PrepareCall(this ptr<State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;

    var idx = s.livenessMap.Get(v);
    if (!idx.StackMapValid()) { 
        // See Liveness.hasStackMap.
        {
            ptr<ssa.AuxCall> (sym, ok) = v.Aux._<ptr<ssa.AuxCall>>();

            if (!ok || !(sym.Fn == ir.Syms.Typedmemclr || sym.Fn == ir.Syms.Typedmemmove)) {
                @base.Fatalf("missing stack map index for %v", v.LongString());
            }

        }
    }
    ptr<ssa.AuxCall> (call, ok) = v.Aux._<ptr<ssa.AuxCall>>();

    if (ok && call.Fn == ir.Syms.Deferreturn) { 
        // Deferred calls will appear to be returning to
        // the CALL deferreturn(SB) that we are about to emit.
        // However, the stack trace code will show the line
        // of the instruction byte before the return PC.
        // To avoid that being an unrelated instruction,
        // insert an actual hardware NOP that will have the right line number.
        // This is different from obj.ANOP, which is a virtual no-op
        // that doesn't make it into the instruction stream.
        Arch.Ginsnopdefer(s.pp);
    }
    if (ok) { 
        // Record call graph information for nowritebarrierrec
        // analysis.
        if (nowritebarrierrecCheck != null) {
            nowritebarrierrecCheck.recordCall(s.pp.CurFunc, call.Fn, v.Pos);
        }
    }
    if (s.maxarg < v.AuxInt) {
        s.maxarg = v.AuxInt;
    }
}

// UseArgs records the fact that an instruction needs a certain amount of
// callee args space for its use.
private static void UseArgs(this ptr<State> _addr_s, long n) {
    ref State s = ref _addr_s.val;

    if (s.maxarg < n) {
        s.maxarg = n;
    }
}

// fieldIdx finds the index of the field referred to by the ODOT node n.
private static nint fieldIdx(ptr<ir.SelectorExpr> _addr_n) => func((_, panic, _) => {
    ref ir.SelectorExpr n = ref _addr_n.val;

    var t = n.X.Type();
    if (!t.IsStruct()) {
        panic("ODOT's LHS is not a struct");
    }
    foreach (var (i, f) in t.Fields().Slice()) {
        if (f.Sym == n.Sel) {
            if (f.Offset != n.Offset()) {
                panic("field offset doesn't match");
            }
            return i;
        }
    }    panic(fmt.Sprintf("can't find field in expr %v\n", n)); 

    // TODO: keep the result of this function somewhere in the ODOT Node
    // so we don't have to recompute it each time we need it.
});

// ssafn holds frontend information about a function that the backend is processing.
// It also exports a bunch of compiler services for the ssa backend.
private partial struct ssafn {
    public ptr<ir.Func> curfn;
    public map<@string, ptr<obj.LSym>> strings; // map from constant string to data symbols
    public long stksize; // stack size for current frame
    public long stkptrsize; // prefix of stack containing pointers
    public bool log; // print ssa debug to the stdout
}

// StringData returns a symbol which
// is the data component of a global string constant containing s.
private static ptr<obj.LSym> StringData(this ptr<ssafn> _addr_e, @string s) {
    ref ssafn e = ref _addr_e.val;

    {
        var (aux, ok) = e.strings[s];

        if (ok) {
            return _addr_aux!;
        }
    }
    if (e.strings == null) {
        e.strings = make_map<@string, ptr<obj.LSym>>();
    }
    var data = staticdata.StringSym(e.curfn.Pos(), s);
    e.strings[s] = data;
    return _addr_data!;
}

private static ptr<ir.Name> Auto(this ptr<ssafn> _addr_e, src.XPos pos, ptr<types.Type> _addr_t) {
    ref ssafn e = ref _addr_e.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_typecheck.TempAt(pos, e.curfn, t)!; // Note: adds new auto to e.curfn.Func.Dcl list
}

private static ptr<obj.LSym> DerefItab(this ptr<ssafn> _addr_e, ptr<obj.LSym> _addr_it, long offset) {
    ref ssafn e = ref _addr_e.val;
    ref obj.LSym it = ref _addr_it.val;

    return _addr_reflectdata.ITabSym(it, offset)!;
}

// SplitSlot returns a slot representing the data of parent starting at offset.
private static ssa.LocalSlot SplitSlot(this ptr<ssafn> _addr_e, ptr<ssa.LocalSlot> _addr_parent, @string suffix, long offset, ptr<types.Type> _addr_t) {
    ref ssafn e = ref _addr_e.val;
    ref ssa.LocalSlot parent = ref _addr_parent.val;
    ref types.Type t = ref _addr_t.val;

    var node = parent.N;

    if (node.Class != ir.PAUTO || node.Addrtaken()) { 
        // addressed things and non-autos retain their parents (i.e., cannot truly be split)
        return new ssa.LocalSlot(N:node,Type:t,Off:parent.Off+offset);
    }
    ptr<types.Sym> s = addr(new types.Sym(Name:node.Sym().Name+suffix,Pkg:types.LocalPkg));
    var n = ir.NewNameAt(parent.N.Pos(), s);
    s.Def = n;
    ir.AsNode(s.Def).Name().SetUsed(true);
    n.SetType(t);
    n.Class = ir.PAUTO;
    n.SetEsc(ir.EscNever);
    n.Curfn = e.curfn;
    e.curfn.Dcl = append(e.curfn.Dcl, n);
    types.CalcSize(t);
    return new ssa.LocalSlot(N:n,Type:t,Off:0,SplitOf:parent,SplitOffset:offset);
}

private static bool CanSSA(this ptr<ssafn> _addr_e, ptr<types.Type> _addr_t) {
    ref ssafn e = ref _addr_e.val;
    ref types.Type t = ref _addr_t.val;

    return TypeOK(_addr_t);
}

private static @string Line(this ptr<ssafn> _addr_e, src.XPos pos) {
    ref ssafn e = ref _addr_e.val;

    return @base.FmtPos(pos);
}

// Log logs a message from the compiler.
private static void Logf(this ptr<ssafn> _addr_e, @string msg, params object[] args) {
    args = args.Clone();
    ref ssafn e = ref _addr_e.val;

    if (e.log) {
        fmt.Printf(msg, args);
    }
}

private static bool Log(this ptr<ssafn> _addr_e) {
    ref ssafn e = ref _addr_e.val;

    return e.log;
}

// Fatal reports a compiler error and exits.
private static void Fatalf(this ptr<ssafn> _addr_e, src.XPos pos, @string msg, params object[] args) {
    args = args.Clone();
    ref ssafn e = ref _addr_e.val;

    @base.Pos = pos;
    var nargs = append(args);
    @base.Fatalf("'%s': " + msg, nargs);
}

// Warnl reports a "warning", which is usually flag-triggered
// logging output for the benefit of tests.
private static void Warnl(this ptr<ssafn> _addr_e, src.XPos pos, @string fmt_, params object[] args) {
    args = args.Clone();
    ref ssafn e = ref _addr_e.val;

    @base.WarnfAt(pos, fmt_, args);
}

private static bool Debug_checknil(this ptr<ssafn> _addr_e) {
    ref ssafn e = ref _addr_e.val;

    return @base.Debug.Nil != 0;
}

private static bool UseWriteBarrier(this ptr<ssafn> _addr_e) {
    ref ssafn e = ref _addr_e.val;

    return @base.Flag.WB;
}

private static ptr<obj.LSym> Syslook(this ptr<ssafn> _addr_e, @string name) {
    ref ssafn e = ref _addr_e.val;

    switch (name) {
        case "goschedguarded": 
            return _addr_ir.Syms.Goschedguarded!;
            break;
        case "writeBarrier": 
            return _addr_ir.Syms.WriteBarrier!;
            break;
        case "gcWriteBarrier": 
            return _addr_ir.Syms.GCWriteBarrier!;
            break;
        case "typedmemmove": 
            return _addr_ir.Syms.Typedmemmove!;
            break;
        case "typedmemclr": 
            return _addr_ir.Syms.Typedmemclr!;
            break;
    }
    e.Fatalf(src.NoXPos, "unknown Syslook func %v", name);
    return _addr_null!;
}

private static void SetWBPos(this ptr<ssafn> _addr_e, src.XPos pos) {
    ref ssafn e = ref _addr_e.val;

    e.curfn.SetWBPos(pos);
}

private static @string MyImportPath(this ptr<ssafn> _addr_e) {
    ref ssafn e = ref _addr_e.val;

    return @base.Ctxt.Pkgpath;
}

private static ir.Node clobberBase(ir.Node n) {
    if (n.Op() == ir.ODOT) {
        ptr<ir.SelectorExpr> n = n._<ptr<ir.SelectorExpr>>();
        if (n.X.Type().NumFields() == 1) {
            return clobberBase(n.X);
        }
    }
    if (n.Op() == ir.OINDEX) {
        n = n._<ptr<ir.IndexExpr>>();
        if (n.X.Type().IsArray() && n.X.Type().NumElem() == 1) {
            return clobberBase(n.X);
        }
    }
    return n;
}

// callTargetLSym returns the correct LSym to call 'callee' using its ABI.
private static ptr<obj.LSym> callTargetLSym(ptr<ir.Name> _addr_callee) {
    ref ir.Name callee = ref _addr_callee.val;

    if (callee.Func == null) { 
        // TODO(austin): This happens in a few cases of
        // compiler-generated functions. These are all
        // ABIInternal. It would be better if callee.Func was
        // never nil and we didn't need this case.
        return _addr_callee.Linksym()!;
    }
    return _addr_callee.LinksymABI(callee.Func.ABI)!;
}

private static sbyte min8(sbyte a, sbyte b) {
    if (a < b) {
        return a;
    }
    return b;
}

private static sbyte max8(sbyte a, sbyte b) {
    if (a > b) {
        return a;
    }
    return b;
}

// deferstruct makes a runtime._defer structure, with additional space for
// stksize bytes of args.
private static ptr<types.Type> deferstruct(long stksize) {
    Func<@string, ptr<types.Type>, ptr<types.Field>> makefield = (name, typ) => { 
        // Unlike the global makefield function, this one needs to set Pkg
        // because these types might be compared (in SSA CSE sorting).
        // TODO: unify this makefield and the global one above.
        ptr<types.Sym> sym = addr(new types.Sym(Name:name,Pkg:types.LocalPkg));
        return _addr_types.NewField(src.NoXPos, sym, typ)!;
    };
    var argtype = types.NewArray(types.Types[types.TUINT8], stksize);
    argtype.Width = stksize;
    argtype.Align = 1; 
    // These fields must match the ones in runtime/runtime2.go:_defer and
    // cmd/compile/internal/gc/ssa.go:(*state).call.
    ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("siz",types.Types[types.TUINT32]), makefield("started",types.Types[types.TBOOL]), makefield("heap",types.Types[types.TBOOL]), makefield("openDefer",types.Types[types.TBOOL]), makefield("sp",types.Types[types.TUINTPTR]), makefield("pc",types.Types[types.TUINTPTR]), makefield("fn",types.Types[types.TUINTPTR]), makefield("_panic",types.Types[types.TUINTPTR]), makefield("link",types.Types[types.TUINTPTR]), makefield("framepc",types.Types[types.TUINTPTR]), makefield("varp",types.Types[types.TUINTPTR]), makefield("fd",types.Types[types.TUINTPTR]), makefield("args",argtype) }); 

    // build struct holding the above fields
    var s = types.NewStruct(types.NoPkg, fields);
    s.SetNoalg(true);
    types.CalcStructSize(s);
    return _addr_s!;
}

// SlotAddr uses LocalSlot information to initialize an obj.Addr
// The resulting addr is used in a non-standard context -- in the prologue
// of a function, before the frame has been constructed, so the standard
// addressing for the parameters will be wrong.
public static obj.Addr SpillSlotAddr(ssa.Spill spill, short baseReg, long extraOffset) {
    return new obj.Addr(Name:obj.NAME_NONE,Type:obj.TYPE_MEM,Reg:baseReg,Offset:spill.Offset+extraOffset,);
}

public static array<ptr<obj.LSym>> BoundsCheckFunc = new array<ptr<obj.LSym>>(ssa.BoundsKindCount);public static array<ptr<obj.LSym>> ExtendCheckFunc = new array<ptr<obj.LSym>>(ssa.BoundsKindCount);

// GCWriteBarrierReg maps from registers to gcWriteBarrier implementation LSyms.
public static map<short, ptr<obj.LSym>> GCWriteBarrierReg = default;

} // end ssagen_package
