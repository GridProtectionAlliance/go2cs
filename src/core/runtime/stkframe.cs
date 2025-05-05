// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using runtime.@internal;

partial class runtime_package {

// A stkframe holds information about a single physical stack frame.
[GoType] partial struct stkframe {
    // fn is the function being run in this frame. If there is
    // inlining, this is the outermost function.
    internal ΔfuncInfo fn;
    // pc is the program counter within fn.
    //
    // The meaning of this is subtle:
    //
    // - Typically, this frame performed a regular function call
    //   and this is the return PC (just after the CALL
    //   instruction). In this case, pc-1 reflects the CALL
    //   instruction itself and is the correct source of symbolic
    //   information.
    //
    // - If this frame "called" sigpanic, then pc is the
    //   instruction that panicked, and pc is the correct address
    //   to use for symbolic information.
    //
    // - If this is the innermost frame, then PC is where
    //   execution will continue, but it may not be the
    //   instruction following a CALL. This may be from
    //   cooperative preemption, in which case this is the
    //   instruction after the call to morestack. Or this may be
    //   from a signal or an un-started goroutine, in which case
    //   PC could be any instruction, including the first
    //   instruction in a function. Conventionally, we use pc-1
    //   for symbolic information, unless pc == fn.entry(), in
    //   which case we use pc.
    internal uintptr pc;
    // continpc is the PC where execution will continue in fn, or
    // 0 if execution will not continue in this frame.
    //
    // This is usually the same as pc, unless this frame "called"
    // sigpanic, in which case it's either the address of
    // deferreturn or 0 if this frame will never execute again.
    //
    // This is the PC to use to look up GC liveness for this frame.
    internal uintptr continpc;
    internal uintptr lr; // program counter at caller aka link register
    internal uintptr sp; // stack pointer at pc
    internal uintptr fp; // stack pointer at caller aka frame pointer
    internal uintptr varp; // top of local variables
    internal uintptr argp; // pointer to function arguments
}

// reflectMethodValue is a partial duplicate of reflect.makeFuncImpl
// and reflect.methodValue.
[GoType] partial struct reflectMethodValue {
    internal uintptr fn;
    internal ж<bitvector> stack; // ptrmap for both args and results
    internal uintptr argLen;    // just args
}

// argBytes returns the argument frame size for a call to frame.fn.
[GoRecv] internal static uintptr argBytes(this ref stkframe frame) {
    if (frame.fn.args != abi.ArgsSizeUnknown) {
        return ((uintptr)frame.fn.args);
    }
    // This is an uncommon and complicated case. Fall back to fully
    // fetching the argument map to compute its size.
    var (argMap, _) = frame.argMapInternal();
    return ((uintptr)argMap.n) * goarch.PtrSize;
}

// argMapInternal is used internally by stkframe to fetch special
// argument maps.
//
// argMap.n is always populated with the size of the argument map.
//
// argMap.bytedata is only populated for dynamic argument maps (used
// by reflect). If the caller requires the argument map, it should use
// this if non-nil, and otherwise fetch the argument map using the
// current PC.
//
// hasReflectStackObj indicates that this frame also has a reflect
// function stack object, which the caller must synthesize.
[GoRecv] internal static (bitvector argMap, bool hasReflectStackObj) argMapInternal(this ref stkframe frame) {
    bitvector argMap = default!;
    bool hasReflectStackObj = default!;

    var f = frame.fn;
    if (f.args != abi.ArgsSizeUnknown) {
        argMap.n = f.args / goarch.PtrSize;
        return (argMap, hasReflectStackObj);
    }
    // Extract argument bitmaps for reflect stubs from the calls they made to reflect.
    var exprᴛ1 = funcname(f);
    if (exprᴛ1 == "reflect.makeFuncStub"u8 || exprᴛ1 == "reflect.methodValueCall"u8) {
        var arg0 = frame.sp + sys.MinFrameSize;
        var minSP = frame.fp;
        if (!usesLR) {
            // These take a *reflect.methodValue as their
            // context register and immediately save it to 0(SP).
            // Get the methodValue from 0(SP).
            // The CALL itself pushes a word.
            // Undo that adjustment.
            minSP -= goarch.PtrSize;
        }
        if (arg0 >= minSP) {
            // The function hasn't started yet.
            // This only happens if f was the
            // start function of a new goroutine
            // that hasn't run yet *and* f takes
            // no arguments and has no results
            // (otherwise it will get wrapped in a
            // closure). In this case, we can't
            // reach into its locals because it
            // doesn't have locals yet, but we
            // also know its argument map is
            // empty.
            if (frame.pc != f.entry()) {
                print("runtime: confused by ", funcname(f), ": no frame (sp=", ((Δhex)frame.sp), " fp=", ((Δhex)frame.fp), ") at entry+", ((Δhex)(frame.pc - f.entry())), "\n");
                @throw("reflect mismatch"u8);
            }
            return (new bitvector(nil), false);
        }
        hasReflectStackObj = true;
        var mv = ~(ж<ж<reflectMethodValue>>)(uintptr)(((@unsafe.Pointer)arg0));
        var retValid = ~(ж<bool>)(uintptr)(((@unsafe.Pointer)(arg0 + 4 * goarch.PtrSize)));
        if ((~mv).fn != f.entry()) {
            // No locals, so also no stack objects
            // Figure out whether the return values are valid.
            // Reflect will update this value after it copies
            // in the return values.
            print("runtime: confused by ", funcname(f), "\n");
            @throw("reflect mismatch"u8);
        }
        argMap = (~mv).stack.val;
        if (!retValid) {
            // argMap.n includes the results, but
            // those aren't valid, so drop them.
            var n = ((int32)(((uintptr)((~mv).argLen & ~(goarch.PtrSize - 1))) / goarch.PtrSize));
            if (n < argMap.n) {
                argMap.n = n;
            }
        }
    }

    return (argMap, hasReflectStackObj);
}

// getStackMap returns the locals and arguments live pointer maps, and
// stack object list for frame.
[GoRecv] internal static (bitvector locals, bitvector args, slice<stackObjectRecord> objs) getStackMap(this ref stkframe frame, bool debug) {
    bitvector locals = default!;
    bitvector args = default!;
    slice<stackObjectRecord> objs = default!;

    var targetpc = frame.continpc;
    if (targetpc == 0) {
        // Frame is dead. Return empty bitvectors.
        return (locals, args, objs);
    }
    var f = frame.fn;
    var pcdata = ((int32)(-1));
    if (targetpc != f.entry()) {
        // Back up to the CALL. If we're at the function entry
        // point, we want to use the entry map (-1), even if
        // the first instruction of the function changes the
        // stack map.
        targetpc--;
        pcdata = pcdatavalue(f, abi.PCDATA_StackMapIndex, targetpc);
    }
    if (pcdata == -1) {
        // We do not have a valid pcdata value but there might be a
        // stackmap for this function. It is likely that we are looking
        // at the function prologue, assume so and hope for the best.
        pcdata = 0;
    }
    // Local variables.
    var size = frame.varp - frame.sp;
    uintptr minsize = default!;
    var exprᴛ1 = goarch.ArchFamily;
    if (exprᴛ1 == goarch.ARM64) {
        minsize = sys.StackAlign;
    }
    else { /* default: */
        minsize = sys.MinFrameSize;
    }

    if (size > minsize) {
        var stackid = pcdata;
        var stkmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_LocalsPointerMaps));
        if (stkmap == nil || (~stkmap).n <= 0) {
            print("runtime: frame ", funcname(f), " untyped locals ", ((Δhex)(frame.varp - size)), "+", ((Δhex)size), "\n");
            @throw("missing stackmap"u8);
        }
        // If nbit == 0, there's no work to do.
        if ((~stkmap).nbit > 0){
            if (stackid < 0 || stackid >= (~stkmap).n) {
                // don't know where we are
                print("runtime: pcdata is ", stackid, " and ", (~stkmap).n, " locals stack map entries for ", funcname(f), " (targetpc=", ((Δhex)targetpc), ")\n");
                @throw("bad symbol table"u8);
            }
            locals = stackmapdata(stkmap, stackid);
            if (stackDebug >= 3 && debug) {
                print("      locals ", stackid, "/", (~stkmap).n, " ", locals.n, " words ", locals.bytedata, "\n");
            }
        } else 
        if (stackDebug >= 3 && debug) {
            print("      no locals to adjust\n");
        }
    }
    // Arguments. First fetch frame size and special-case argument maps.
    bool isReflect = default!;
    (args, isReflect) = frame.argMapInternal();
    if (args.n > 0 && args.bytedata == nil) {
        // Non-empty argument frame, but not a special map.
        // Fetch the argument map at pcdata.
        var stackmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_ArgsPointerMaps));
        if (stackmap == nil || (~stackmap).n <= 0) {
            print("runtime: frame ", funcname(f), " untyped args ", ((Δhex)frame.argp), "+", ((Δhex)(args.n * goarch.PtrSize)), "\n");
            @throw("missing stackmap"u8);
        }
        if (pcdata < 0 || pcdata >= (~stackmap).n) {
            // don't know where we are
            print("runtime: pcdata is ", pcdata, " and ", (~stackmap).n, " args stack map entries for ", funcname(f), " (targetpc=", ((Δhex)targetpc), ")\n");
            @throw("bad symbol table"u8);
        }
        if ((~stackmap).nbit == 0){
            args.n = 0;
        } else {
            args = stackmapdata(stackmap, pcdata);
        }
    }
    // stack objects.
    if ((GOARCH == "amd64"u8 || GOARCH == "arm64"u8 || GOARCH == "loong64"u8 || GOARCH == "ppc64"u8 || GOARCH == "ppc64le"u8 || GOARCH == "riscv64"u8) && @unsafe.Sizeof(new abi.RegArgs(nil)) > 0 && isReflect){
        // For reflect.makeFuncStub and reflect.methodValueCall,
        // we need to fake the stack object record.
        // These frames contain an internal/abi.RegArgs at a hard-coded offset.
        // This offset matches the assembly code on amd64 and arm64.
        objs = methodValueCallFrameObjs[..];
    } else {
        @unsafe.Pointer Δp = (uintptr)funcdata(f, abi.FUNCDATA_StackObjects);
        if (Δp != nil) {
            var n = ~(ж<uintptr>)(uintptr)(Δp);
            Δp = (uintptr)add(Δp, goarch.PtrSize);
            var r0 = (ж<stackObjectRecord>)(uintptr)(noescape(Δp));
            objs = @unsafe.Slice(r0, ((nint)n));
        }
    }
    // Note: the noescape above is needed to keep
    // getStackMap from "leaking param content:
    // frame".  That leak propagates up to getgcmask, then
    // GCMask, then verifyGCInfo, which converts the stack
    // gcinfo tests into heap gcinfo tests :(
    return (locals, args, objs);
}

internal static array<stackObjectRecord> methodValueCallFrameObjs;    // initialized in stackobjectinit

internal static void stkobjinit() {
    any abiRegArgsEface = new abi.RegArgs(nil);
    var abiRegArgsType = efaceOf(Ꮡ(abiRegArgsEface)).val._type;
    if ((abiꓸKind)((~abiRegArgsType).Kind_ & abi.KindGCProg) != 0) {
        @throw("abiRegArgsType needs GC Prog, update methodValueCallFrameObjs"u8);
    }
    // Set methodValueCallFrameObjs[0].gcdataoff so that
    // stackObjectRecord.gcdata() will work correctly with it.
    var ptr = ((uintptr)new @unsafe.Pointer(ᏑmethodValueCallFrameObjs.at<stackObjectRecord>(0)));
    ж<moduledata> mod = default!;
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        if ((~datap).gofunc <= ptr && ptr < (~datap).end) {
            mod = datap;
            break;
        }
    }
    if (mod == nil) {
        @throw("methodValueCallFrameObjs is not in a module"u8);
    }
    methodValueCallFrameObjs[0] = new stackObjectRecord(
        off: -((int32)alignUp((~abiRegArgsType).Size_, 8)), // It's always the highest address local.

        size: ((int32)(~abiRegArgsType).Size_),
        _ptrdata: ((int32)(~abiRegArgsType).PtrBytes),
        gcdataoff: ((uint32)(((uintptr)new @unsafe.Pointer((~abiRegArgsType).GCData)) - (~mod).rodata))
    );
}

} // end runtime_package
