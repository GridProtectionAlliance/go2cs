// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\syscall_windows.go
using abi = go.@internal.abi_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // cbs stores all registered Go callbacks.
private static var cbs = default;

// winCallback records information about a registered Go callback.
private partial struct winCallback {
    public ptr<funcval> fn; // Go function
    public System.UIntPtr retPop; // For 386 cdecl, how many bytes to pop on return
    public abiDesc abiMap;
}

// abiPartKind is the action an abiPart should take.
private partial struct abiPartKind { // : nint
}

private static readonly abiPartKind abiPartBad = iota;
private static readonly var abiPartStack = 0; // Move a value from memory to the stack.
private static readonly var abiPartReg = 1; // Move a value from memory to a register.

// abiPart encodes a step in translating between calling ABIs.
private partial struct abiPart {
    public abiPartKind kind;
    public System.UIntPtr srcStackOffset;
    public System.UIntPtr dstStackOffset; // used if kind == abiPartStack
    public nint dstRegister; // used if kind == abiPartReg
    public System.UIntPtr len;
}

private static bool tryMerge(this ptr<abiPart> _addr_a, abiPart b) {
    ref abiPart a = ref _addr_a.val;

    if (a.kind != abiPartStack || b.kind != abiPartStack) {
        return false;
    }
    if (a.srcStackOffset + a.len == b.srcStackOffset && a.dstStackOffset + a.len == b.dstStackOffset) {
        a.len += b.len;
        return true;
    }
    return false;

}

// abiDesc specifies how to translate from a C frame to a Go
// frame. This does not specify how to translate back because
// the result is always a uintptr. If the C ABI is fastcall,
// this assumes the four fastcall registers were first spilled
// to the shadow space.
private partial struct abiDesc {
    public slice<abiPart> parts;
    public System.UIntPtr srcStackSize; // stdcall/fastcall stack space tracking
    public System.UIntPtr dstStackSize; // Go stack space used
    public System.UIntPtr dstSpill; // Extra stack space for argument spill slots
    public nint dstRegisters; // Go ABI int argument registers used

// retOffset is the offset of the uintptr-sized result in the Go
// frame.
    public System.UIntPtr retOffset;
}

private static void assignArg(this ptr<abiDesc> _addr_p, ptr<_type> _addr_t) => func((_, panic, _) => {
    ref abiDesc p = ref _addr_p.val;
    ref _type t = ref _addr_t.val;

    if (t.size > sys.PtrSize) { 
        // We don't support this right now. In
        // stdcall/cdecl, 64-bit ints and doubles are
        // passed as two words (little endian); and
        // structs are pushed on the stack. In
        // fastcall, arguments larger than the word
        // size are passed by reference. On arm,
        // 8-byte aligned arguments round up to the
        // next even register and can be split across
        // registers and the stack.
        panic("compileCallback: argument size is larger than uintptr");

    }
    {
        var k = t.kind & kindMask;

        if (GOARCH != "386" && (k == kindFloat32 || k == kindFloat64)) { 
            // In fastcall, floating-point arguments in
            // the first four positions are passed in
            // floating-point registers, which we don't
            // currently spill. arm passes floating-point
            // arguments in VFP registers, which we also
            // don't support.
            // So basically we only support 386.
            panic("compileCallback: float arguments not supported");

        }
    }


    if (t.size == 0) { 
        // The Go ABI aligns for zero-sized types.
        p.dstStackSize = alignUp(p.dstStackSize, uintptr(t.align));
        return ;

    }
    var oldParts = p.parts;
    if (p.tryRegAssignArg(t, 0)) { 
        // Account for spill space.
        //
        // TODO(mknyszek): Remove this when we no longer have
        // caller reserved spill space.
        p.dstSpill = alignUp(p.dstSpill, uintptr(t.align));
        p.dstSpill += t.size;

    }
    else
 { 
        // Register assignment failed.
        // Undo the work and stack assign.
        p.parts = oldParts; 

        // The Go ABI aligns arguments.
        p.dstStackSize = alignUp(p.dstStackSize, uintptr(t.align)); 

        // Copy just the size of the argument. Note that this
        // could be a small by-value struct, but C and Go
        // struct layouts are compatible, so we can copy these
        // directly, too.
        abiPart part = new abiPart(kind:abiPartStack,srcStackOffset:p.srcStackSize,dstStackOffset:p.dstStackSize,len:t.size,); 
        // Add this step to the adapter.
        if (len(p.parts) == 0 || !p.parts[len(p.parts) - 1].tryMerge(part)) {
            p.parts = append(p.parts, part);
        }
        p.dstStackSize += t.size;

    }
    p.srcStackSize += sys.PtrSize;

});

// tryRegAssignArg tries to register-assign a value of type t.
// If this type is nested in an aggregate type, then offset is the
// offset of this type within its parent type.
// Assumes t.size <= sys.PtrSize and t.size != 0.
//
// Returns whether the assignment succeeded.
private static bool tryRegAssignArg(this ptr<abiDesc> _addr_p, ptr<_type> _addr_t, System.UIntPtr offset) => func((_, panic, _) => {
    ref abiDesc p = ref _addr_p.val;
    ref _type t = ref _addr_t.val;

    {
        var k = t.kind & kindMask;


        if (k == kindBool || k == kindInt || k == kindInt8 || k == kindInt16 || k == kindInt32 || k == kindUint || k == kindUint8 || k == kindUint16 || k == kindUint32 || k == kindUintptr || k == kindPtr || k == kindUnsafePointer) 
            // Assign a register for all these types.
            return p.assignReg(t.size, offset);
        else if (k == kindInt64 || k == kindUint64) 
            // Only register-assign if the registers are big enough.
            if (sys.PtrSize == 8) {
                return p.assignReg(t.size, offset);
            }
        else if (k == kindArray) 
            var at = (arraytype.val)(@unsafe.Pointer(t));
            if (at.len == 1) {
                return p.tryRegAssignArg(at.elem, offset);
            }
        else if (k == kindStruct) 
            var st = (structtype.val)(@unsafe.Pointer(t));
            foreach (var (i) in st.fields) {
                var f = _addr_st.fields[i];
                if (!p.tryRegAssignArg(f.typ, offset + f.offset())) {
                    return false;
                }
            }
            return true;

    } 
    // Pointer-sized types such as maps and channels are currently
    // not supported.
    panic("compileCallabck: type " + t.@string() + " is currently not supported for use in system callbacks");

});

// assignReg attempts to assign a single register for an
// argument with the given size, at the given offset into the
// value in the C ABI space.
//
// Returns whether the assignment was successful.
private static bool assignReg(this ptr<abiDesc> _addr_p, System.UIntPtr size, System.UIntPtr offset) {
    ref abiDesc p = ref _addr_p.val;

    if (p.dstRegisters >= intArgRegs) {
        return false;
    }
    p.parts = append(p.parts, new abiPart(kind:abiPartReg,srcStackOffset:p.srcStackSize+offset,dstRegister:p.dstRegisters,len:size,));
    p.dstRegisters++;
    return true;

}

private partial struct winCallbackKey {
    public ptr<funcval> fn;
    public bool cdecl;
}

private static void callbackasm();

// callbackasmAddr returns address of runtime.callbackasm
// function adjusted by i.
// On x86 and amd64, runtime.callbackasm is a series of CALL instructions,
// and we want callback to arrive at
// correspondent call instruction instead of start of
// runtime.callbackasm.
// On ARM, runtime.callbackasm is a series of mov and branch instructions.
// R12 is loaded with the callback index. Each entry is two instructions,
// hence 8 bytes.
private static System.UIntPtr callbackasmAddr(nint i) => func((_, panic, _) => {
    nint entrySize = default;
    switch (GOARCH) {
        case "386": 

        case "amd64": 
            entrySize = 5;
            break;
        case "arm": 
            // On ARM and ARM64, each entry is a MOV instruction
            // followed by a branch instruction

        case "arm64": 
            // On ARM and ARM64, each entry is a MOV instruction
            // followed by a branch instruction
            entrySize = 8;
            break;
        default: 
            panic("unsupported architecture");
            break;
    }
    return funcPC(callbackasm) + uintptr(i * entrySize);

});

private static readonly nint callbackMaxFrame = 64 * sys.PtrSize;

// compileCallback converts a Go function fn into a C function pointer
// that can be passed to Windows APIs.
//
// On 386, if cdecl is true, the returned C function will use the
// cdecl calling convention; otherwise, it will use stdcall. On amd64,
// it always uses fastcall. On arm, it always uses the ARM convention.
//
//go:linkname compileCallback syscall.compileCallback


// compileCallback converts a Go function fn into a C function pointer
// that can be passed to Windows APIs.
//
// On 386, if cdecl is true, the returned C function will use the
// cdecl calling convention; otherwise, it will use stdcall. On amd64,
// it always uses fastcall. On arm, it always uses the ARM convention.
//
//go:linkname compileCallback syscall.compileCallback
private static System.UIntPtr compileCallback(eface fn, bool cdecl) => func((_, panic, _) => {
    System.UIntPtr code = default;

    if (GOARCH != "386") {>>MARKER:FUNCTION_callbackasm_BLOCK_PREFIX<< 
        // cdecl is only meaningful on 386.
        cdecl = false;

    }
    if (fn._type == null || (fn._type.kind & kindMask) != kindFunc) {
        panic("compileCallback: expected function with one uintptr-sized result");
    }
    var ft = (functype.val)(@unsafe.Pointer(fn._type)); 

    // Check arguments and construct ABI translation.
    abiDesc abiMap = default;
    foreach (var (_, t) in ft.@in()) {
        abiMap.assignArg(t);
    }    abiMap.dstStackSize = alignUp(abiMap.dstStackSize, sys.PtrSize);
    abiMap.retOffset = abiMap.dstStackSize;

    if (len(ft.@out()) != 1) {
        panic("compileCallback: expected function with one uintptr-sized result");
    }
    if (ft.@out()[0].size != sys.PtrSize) {
        panic("compileCallback: expected function with one uintptr-sized result");
    }
    {
        var k = ft.@out()[0].kind & kindMask;

        if (k == kindFloat32 || k == kindFloat64) { 
            // In cdecl and stdcall, float results are returned in
            // ST(0). In fastcall, they're returned in XMM0.
            // Either way, it's not AX.
            panic("compileCallback: float results not supported");

        }
    }

    if (intArgRegs == 0) { 
        // Make room for the uintptr-sized result.
        // If there are argument registers, the return value will
        // be passed in the first register.
        abiMap.dstStackSize += sys.PtrSize;

    }
    var frameSize = alignUp(abiMap.dstStackSize, sys.PtrSize);
    frameSize += abiMap.dstSpill;
    if (frameSize > callbackMaxFrame) {
        panic("compileCallback: function argument frame too large");
    }
    System.UIntPtr retPop = default;
    if (cdecl) {
        retPop = abiMap.srcStackSize;
    }
    winCallbackKey key = new winCallbackKey((*funcval)(fn.data),cdecl);

    lock(_addr_cbs.@lock); // We don't unlock this in a defer because this is used from the system stack.

    // Check if this callback is already registered.
    {
        var n__prev1 = n;

        var (n, ok) = cbs.index[key];

        if (ok) {
            unlock(_addr_cbs.@lock);
            return callbackasmAddr(n);
        }
        n = n__prev1;

    } 

    // Register the callback.
    if (cbs.index == null) {
        cbs.index = make_map<winCallbackKey, nint>();
    }
    var n = cbs.n;
    if (n >= len(cbs.ctxt)) {
        unlock(_addr_cbs.@lock);
        throw("too many callback functions");
    }
    winCallback c = new winCallback(key.fn,retPop,abiMap);
    cbs.ctxt[n] = c;
    cbs.index[key] = n;
    cbs.n++;

    unlock(_addr_cbs.@lock);
    return callbackasmAddr(n);

});

private partial struct callbackArgs {
    public System.UIntPtr index; // args points to the argument block.
//
// For cdecl and stdcall, all arguments are on the stack.
//
// For fastcall, the trampoline spills register arguments to
// the reserved spill slots below the stack arguments,
// resulting in a layout equivalent to stdcall.
//
// For arm, the trampoline stores the register arguments just
// below the stack arguments, so again we can treat it as one
// big stack arguments frame.
    public unsafe.Pointer args; // Below are out-args from callbackWrap
    public System.UIntPtr result;
    public System.UIntPtr retPop; // For 386 cdecl, how many bytes to pop on return
}

// callbackWrap is called by callbackasm to invoke a registered C callback.
private static void callbackWrap(ptr<callbackArgs> _addr_a) => func((_, panic, _) => {
    ref callbackArgs a = ref _addr_a.val;

    var c = cbs.ctxt[a.index];
    a.retPop = c.retPop; 

    // Convert from C to Go ABI.
    ref abi.RegArgs regs = ref heap(out ptr<abi.RegArgs> _addr_regs);
    ref array<byte> frame = ref heap(new array<byte>(callbackMaxFrame), out ptr<array<byte>> _addr_frame);
    var goArgs = @unsafe.Pointer(_addr_frame);
    foreach (var (_, part) in c.abiMap.parts) {

        if (part.kind == abiPartStack) 
            memmove(add(goArgs, part.dstStackOffset), add(a.args, part.srcStackOffset), part.len);
        else if (part.kind == abiPartReg) 
            var goReg = @unsafe.Pointer(_addr_regs.Ints[part.dstRegister]);
            memmove(goReg, add(a.args, part.srcStackOffset), part.len);
        else 
            panic("bad ABI description");
        
    }    var frameSize = alignUp(c.abiMap.dstStackSize, sys.PtrSize);
    frameSize += c.abiMap.dstSpill; 

    // Even though this is copying back results, we can pass a nil
    // type because those results must not require write barriers.
    reflectcall(null, @unsafe.Pointer(c.fn), noescape(goArgs), uint32(c.abiMap.dstStackSize), uint32(c.abiMap.retOffset), uint32(frameSize), _addr_regs); 

    // Extract the result.
    //
    // There's always exactly one return value, one pointer in size.
    // If it's on the stack, then we will have reserved space for it
    // at the end of the frame, otherwise it was passed in a register.
    if (c.abiMap.dstStackSize != c.abiMap.retOffset) {
        a.result = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_frame[c.abiMap.retOffset]));
    }
    else
 {
        nint zero = default; 
        // On architectures with no registers, Ints[0] would be a compile error,
        // so we use a dynamic index. These architectures will never take this
        // branch, so this won't cause a runtime panic.
        a.result = regs.Ints[zero];

    }
});

private static readonly nuint _LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

// When available, this function will use LoadLibraryEx with the filename
// parameter and the important SEARCH_SYSTEM32 argument. But on systems that
// do not have that option, absoluteFilepath should contain a fallback
// to the full path inside of system32 for use with vanilla LoadLibrary.
//go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
//go:nosplit
//go:cgo_unsafe_args


// When available, this function will use LoadLibraryEx with the filename
// parameter and the important SEARCH_SYSTEM32 argument. But on systems that
// do not have that option, absoluteFilepath should contain a fallback
// to the full path inside of system32 for use with vanilla LoadLibrary.
//go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr) syscall_loadsystemlibrary(ptr<ushort> _addr_filename, ptr<ushort> _addr_absoluteFilepath) {
    System.UIntPtr handle = default;
    System.UIntPtr err = default;
    ref ushort filename = ref _addr_filename.val;
    ref ushort absoluteFilepath = ref _addr_absoluteFilepath.val;

    lockOSThread();
    var c = _addr_getg().m.syscall;

    if (useLoadLibraryEx) {
        c.fn = getLoadLibraryEx();
        c.n = 3;
        ref struct{lpFileName*uint16hFileuintptrflagsuint32} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{lpFileName*uint16hFileuintptrflagsuint32}{filename,0,_LOAD_LIBRARY_SEARCH_SYSTEM32}, out ptr<struct{lpFileName*uint16hFileuintptrflagsuint32}> _addr_args);
        c.args = uintptr(noescape(@unsafe.Pointer(_addr_args)));
    }
    else
 {
        c.fn = getLoadLibrary();
        c.n = 1;
        c.args = uintptr(noescape(@unsafe.Pointer(_addr_absoluteFilepath)));
    }
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    handle = c.r1;
    if (handle == 0) {
        err = c.err;
    }
    unlockOSThread(); // not defer'd after the lockOSThread above to save stack frame size.
    return ;

}

//go:linkname syscall_loadlibrary syscall.loadlibrary
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr) syscall_loadlibrary(ptr<ushort> _addr_filename) => func((defer, _, _) => {
    System.UIntPtr handle = default;
    System.UIntPtr err = default;
    ref ushort filename = ref _addr_filename.val;

    lockOSThread();
    defer(unlockOSThread());
    var c = _addr_getg().m.syscall;
    c.fn = getLoadLibrary();
    c.n = 1;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_filename)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    handle = c.r1;
    if (handle == 0) {
        err = c.err;
    }
    return ;

});

//go:linkname syscall_getprocaddress syscall.getprocaddress
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr) syscall_getprocaddress(System.UIntPtr handle, ptr<byte> _addr_procname) => func((defer, _, _) => {
    System.UIntPtr outhandle = default;
    System.UIntPtr err = default;
    ref byte procname = ref _addr_procname.val;

    lockOSThread();
    defer(unlockOSThread());
    var c = _addr_getg().m.syscall;
    c.fn = getGetProcAddress();
    c.n = 2;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_handle)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    outhandle = c.r1;
    if (outhandle == 0) {
        err = c.err;
    }
    return ;

});

//go:linkname syscall_Syscall syscall.Syscall
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((defer, _, _) => {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    defer(unlockOSThread());
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    return (c.r1, c.r2, c.err);
});

//go:linkname syscall_Syscall6 syscall.Syscall6
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) => func((defer, _, _) => {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    defer(unlockOSThread());
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    return (c.r1, c.r2, c.err);
});

//go:linkname syscall_Syscall9 syscall.Syscall9
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall9(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    unlockOSThread();
    return (c.r1, c.r2, c.err);
}

//go:linkname syscall_Syscall12 syscall.Syscall12
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall12(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    unlockOSThread();
    return (c.r1, c.r2, c.err);
}

//go:linkname syscall_Syscall15 syscall.Syscall15
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall15(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    unlockOSThread();
    return (c.r1, c.r2, c.err);
}

//go:linkname syscall_Syscall18 syscall.Syscall18
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall18(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15, System.UIntPtr a16, System.UIntPtr a17, System.UIntPtr a18) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    lockOSThread();
    var c = _addr_getg().m.syscall;
    c.fn = fn;
    c.n = nargs;
    c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    cgocall(asmstdcallAddr, @unsafe.Pointer(c));
    unlockOSThread();
    return (c.r1, c.r2, c.err);
}

} // end runtime_package
