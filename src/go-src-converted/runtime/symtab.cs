// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\symtab.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // Frames may be used to get function/file/line information for a
    // slice of PC values returned by Callers.
public partial struct Frames {
    public slice<System.UIntPtr> callers; // frames is a slice of Frames that have yet to be returned.
    public slice<Frame> frames;
    public array<Frame> frameStore;
}

// Frame is the information returned by Frames for each call frame.
public partial struct Frame {
    public System.UIntPtr PC; // Func is the Func value of this call frame. This may be nil
// for non-Go code or fully inlined functions.
    public ptr<Func> Func; // Function is the package path-qualified function name of
// this call frame. If non-empty, this string uniquely
// identifies a single function in the program.
// This may be the empty string if not known.
// If Func is not nil then Function == Func.Name().
    public @string Function; // File and Line are the file name and line number of the
// location in this frame. For non-leaf frames, this will be
// the location of a call. These may be the empty string and
// zero, respectively, if not known.
    public @string File;
    public nint Line; // Entry point program counter for the function; may be zero
// if not known. If Func is not nil then Entry ==
// Func.Entry().
    public System.UIntPtr Entry; // The runtime's internal view of the function. This field
// is set (funcInfo.valid() returns true) only for Go functions,
// not for C functions.
    public funcInfo funcInfo;
}

// CallersFrames takes a slice of PC values returned by Callers and
// prepares to return function/file/line information.
// Do not change the slice until you are done with the Frames.
public static ptr<Frames> CallersFrames(slice<System.UIntPtr> callers) {
    ptr<Frames> f = addr(new Frames(callers:callers));
    f.frames = f.frameStore[..(int)0];
    return _addr_f!;
}

// Next returns a Frame representing the next call frame in the slice
// of PC values. If it has already returned all call frames, Next
// returns a zero Frame.
//
// The more result indicates whether the next call to Next will return
// a valid Frame. It does not necessarily indicate whether this call
// returned one.
//
// See the Frames example for idiomatic usage.
private static (Frame, bool) Next(this ptr<Frames> _addr_ci) {
    Frame frame = default;
    bool more = default;
    ref Frames ci = ref _addr_ci.val;

    while (len(ci.frames) < 2) { 
        // Find the next frame.
        // We need to look for 2 frames so we know what
        // to return for the "more" result.
        if (len(ci.callers) == 0) {
            break;
        }
        var pc = ci.callers[0];
        ci.callers = ci.callers[(int)1..];
        var funcInfo = findfunc(pc);
        if (!funcInfo.valid()) {
            if (cgoSymbolizer != null) { 
                // Pre-expand cgo frames. We could do this
                // incrementally, too, but there's no way to
                // avoid allocation in this case anyway.
                ci.frames = append(ci.frames, expandCgoFrames(pc));

            }

            continue;

        }
        var f = funcInfo._Func();
        var entry = f.Entry();
        if (pc > entry) { 
            // We store the pc of the start of the instruction following
            // the instruction in question (the call or the inline mark).
            // This is done for historical reasons, and to make FuncForPC
            // work correctly for entries in the result of runtime.Callers.
            pc--;

        }
        var name = funcname(funcInfo);
        {
            var inldata = funcdata(funcInfo, _FUNCDATA_InlTree);

            if (inldata != null) {
                ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata); 
                // Non-strict as cgoTraceback may have added bogus PCs
                // with a valid funcInfo but invalid PCDATA.
                var ix = pcdatavalue1(funcInfo, _PCDATA_InlTreeIndex, pc, _addr_null, false);
                if (ix >= 0) { 
                    // Note: entry is not modified. It always refers to a real frame, not an inlined one.
                    f = null;
                    name = funcnameFromNameoff(funcInfo, inltree[ix].func_); 
                    // File/line is already correct.
                    // TODO: remove file/line from InlinedCall?
                }

            }

        }

        ci.frames = append(ci.frames, new Frame(PC:pc,Func:f,Function:name,Entry:entry,funcInfo:funcInfo,));

    } 

    // Pop one frame from the frame list. Keep the rest.
    // Avoid allocation in the common case, which is 1 or 2 frames.
    switch (len(ci.frames)) {
        case 0: // In the rare case when there are no frames at all, we return Frame{}.
            return ;
            break;
        case 1: 
            frame = ci.frames[0];
            ci.frames = ci.frameStore[..(int)0];
            break;
        case 2: 
            frame = ci.frames[0];
            ci.frameStore[0] = ci.frames[1];
            ci.frames = ci.frameStore[..(int)1];
            break;
        default: 
            frame = ci.frames[0];
            ci.frames = ci.frames[(int)1..];
            break;
    }
    more = len(ci.frames) > 0;
    if (frame.funcInfo.valid()) { 
        // Compute file/line just before we need to return it,
        // as it can be expensive. This avoids computing file/line
        // for the Frame we find but don't return. See issue 32093.
        var (file, line) = funcline1(frame.funcInfo, frame.PC, false);
        (frame.File, frame.Line) = (file, int(line));
    }
    return ;

}

// runtime_expandFinalInlineFrame expands the final pc in stk to include all
// "callers" if pc is inline.
//
//go:linkname runtime_expandFinalInlineFrame runtime/pprof.runtime_expandFinalInlineFrame
private static slice<System.UIntPtr> runtime_expandFinalInlineFrame(slice<System.UIntPtr> stk) {
    if (len(stk) == 0) {
        return stk;
    }
    var pc = stk[len(stk) - 1];
    var tracepc = pc - 1;

    var f = findfunc(tracepc);
    if (!f.valid()) { 
        // Not a Go function.
        return stk;

    }
    var inldata = funcdata(f, _FUNCDATA_InlTree);
    if (inldata == null) { 
        // Nothing inline in f.
        return stk;

    }
    var lastFuncID = funcID_normal; 

    // Remove pc from stk; we'll re-add it below.
    stk = stk[..(int)len(stk) - 1]; 

    // See inline expansion in gentraceback.
    ref pcvalueCache cache = ref heap(out ptr<pcvalueCache> _addr_cache);
    ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
    while (true) { 
        // Non-strict as cgoTraceback may have added bogus PCs
        // with a valid funcInfo but invalid PCDATA.
        var ix = pcdatavalue1(f, _PCDATA_InlTreeIndex, tracepc, _addr_cache, false);
        if (ix < 0) {
            break;
        }
        if (inltree[ix].funcID == funcID_wrapper && elideWrapperCalling(lastFuncID)) { 
            // ignore wrappers
        }
        else
 {
            stk = append(stk, pc);
        }
        lastFuncID = inltree[ix].funcID; 
        // Back up to an instruction in the "caller".
        tracepc = f.entry + uintptr(inltree[ix].parentPc);
        pc = tracepc + 1;

    } 

    // N.B. we want to keep the last parentPC which is not inline.
    stk = append(stk, pc);

    return stk;

}

// expandCgoFrames expands frame information for pc, known to be
// a non-Go function, using the cgoSymbolizer hook. expandCgoFrames
// returns nil if pc could not be expanded.
private static slice<Frame> expandCgoFrames(System.UIntPtr pc) {
    ref cgoSymbolizerArg arg = ref heap(new cgoSymbolizerArg(pc:pc), out ptr<cgoSymbolizerArg> _addr_arg);
    callCgoSymbolizer(_addr_arg);

    if (arg.file == null && arg.funcName == null) { 
        // No useful information from symbolizer.
        return null;

    }
    slice<Frame> frames = default;
    while (true) {
        frames = append(frames, new Frame(PC:pc,Func:nil,Function:gostring(arg.funcName),File:gostring(arg.file),Line:int(arg.lineno),Entry:arg.entry,));
        if (arg.more == 0) {
            break;
        }
        callCgoSymbolizer(_addr_arg);

    } 

    // No more frames for this PC. Tell the symbolizer we are done.
    // We don't try to maintain a single cgoSymbolizerArg for the
    // whole use of Frames, because there would be no good way to tell
    // the symbolizer when we are done.
    arg.pc = 0;
    callCgoSymbolizer(_addr_arg);

    return frames;

}

// NOTE: Func does not expose the actual unexported fields, because we return *Func
// values to users, and we want to keep them from being able to overwrite the data
// with (say) *f = Func{}.
// All code operating on a *Func must call raw() to get the *_func
// or funcInfo() to get the funcInfo instead.

// A Func represents a Go function in the running binary.
public partial struct Func {
}

private static ptr<_func> raw(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return _addr_(_func.val)(@unsafe.Pointer(f))!;
}

private static funcInfo funcInfo(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var fn = f.raw();
    return new funcInfo(fn,findmoduledatap(fn.entry));
}

// PCDATA and FUNCDATA table indexes.
//
// See funcdata.h and ../cmd/internal/objabi/funcdata.go.
private static readonly nint _PCDATA_UnsafePoint = 0;
private static readonly nint _PCDATA_StackMapIndex = 1;
private static readonly nint _PCDATA_InlTreeIndex = 2;

private static readonly nint _FUNCDATA_ArgsPointerMaps = 0;
private static readonly nint _FUNCDATA_LocalsPointerMaps = 1;
private static readonly nint _FUNCDATA_StackObjects = 2;
private static readonly nint _FUNCDATA_InlTree = 3;
private static readonly nint _FUNCDATA_OpenCodedDeferInfo = 4;
private static readonly nint _FUNCDATA_ArgInfo = 5;

private static readonly nuint _ArgsSizeUnknown = -0x80000000;


 
// PCDATA_UnsafePoint values.
private static readonly nint _PCDATA_UnsafePointSafe = -1; // Safe for async preemption
private static readonly nint _PCDATA_UnsafePointUnsafe = -2; // Unsafe for async preemption

// _PCDATA_Restart1(2) apply on a sequence of instructions, within
// which if an async preemption happens, we should back off the PC
// to the start of the sequence when resume.
// We need two so we can distinguish the start/end of the sequence
// in case that two sequences are next to each other.
private static readonly nint _PCDATA_Restart1 = -3;
private static readonly nint _PCDATA_Restart2 = -4; 

// Like _PCDATA_RestartAtEntry, but back to function entry if async
// preempted.
private static readonly nint _PCDATA_RestartAtEntry = -5;


// A FuncID identifies particular functions that need to be treated
// specially by the runtime.
// Note that in some situations involving plugins, there may be multiple
// copies of a particular special runtime function.
// Note: this list must match the list in cmd/internal/objabi/funcid.go.
private partial struct funcID { // : byte
}

private static readonly funcID funcID_normal = iota; // not a special function
private static readonly var funcID_abort = 0;
private static readonly var funcID_asmcgocall = 1;
private static readonly var funcID_asyncPreempt = 2;
private static readonly var funcID_cgocallback = 3;
private static readonly var funcID_debugCallV2 = 4;
private static readonly var funcID_gcBgMarkWorker = 5;
private static readonly var funcID_goexit = 6;
private static readonly var funcID_gogo = 7;
private static readonly var funcID_gopanic = 8;
private static readonly var funcID_handleAsyncEvent = 9;
private static readonly var funcID_jmpdefer = 10;
private static readonly var funcID_mcall = 11;
private static readonly var funcID_morestack = 12;
private static readonly var funcID_mstart = 13;
private static readonly var funcID_panicwrap = 14;
private static readonly var funcID_rt0_go = 15;
private static readonly var funcID_runfinq = 16;
private static readonly var funcID_runtime_main = 17;
private static readonly var funcID_sigpanic = 18;
private static readonly var funcID_systemstack = 19;
private static readonly var funcID_systemstack_switch = 20;
private static readonly var funcID_wrapper = 21; // any autogenerated code (hash/eq algorithms, method wrappers, etc.)

// A FuncFlag holds bits about a function.
// This list must match the list in cmd/internal/objabi/funcid.go.
private partial struct funcFlag { // : byte
}

 
// TOPFRAME indicates a function that appears at the top of its stack.
// The traceback routine stop at such a function and consider that a
// successful, complete traversal of the stack.
// Examples of TOPFRAME functions include goexit, which appears
// at the top of a user goroutine stack, and mstart, which appears
// at the top of a system goroutine stack.
private static readonly funcFlag funcFlag_TOPFRAME = 1 << (int)(iota); 

// SPWRITE indicates a function that writes an arbitrary value to SP
// (any write other than adding or subtracting a constant amount).
// The traceback routines cannot encode such changes into the
// pcsp tables, so the function traceback cannot safely unwind past
// SPWRITE functions. Stopping at an SPWRITE function is considered
// to be an incomplete unwinding of the stack. In certain contexts
// (in particular garbage collector stack scans) that is a fatal error.
private static readonly var funcFlag_SPWRITE = 0;


// pcHeader holds data used by the pclntab lookups.
private partial struct pcHeader {
    public uint magic; // 0xFFFFFFFA
    public byte pad1; // 0,0
    public byte pad2; // 0,0
    public byte minLC; // min instruction size
    public byte ptrSize; // size of a ptr in bytes
    public nint nfunc; // number of functions in the module
    public nuint nfiles; // number of entries in the file tab.
    public System.UIntPtr funcnameOffset; // offset to the funcnametab variable from pcHeader
    public System.UIntPtr cuOffset; // offset to the cutab variable from pcHeader
    public System.UIntPtr filetabOffset; // offset to the filetab variable from pcHeader
    public System.UIntPtr pctabOffset; // offset to the pctab varible from pcHeader
    public System.UIntPtr pclnOffset; // offset to the pclntab variable from pcHeader
}

// moduledata records information about the layout of the executable
// image. It is written by the linker. Any changes here must be
// matched changes to the code in cmd/internal/ld/symtab.go:symtab.
// moduledata is stored in statically allocated non-pointer memory;
// none of the pointers here are visible to the garbage collector.
private partial struct moduledata {
    public ptr<pcHeader> pcHeader;
    public slice<byte> funcnametab;
    public slice<uint> cutab;
    public slice<byte> filetab;
    public slice<byte> pctab;
    public slice<byte> pclntable;
    public slice<functab> ftab;
    public System.UIntPtr findfunctab;
    public System.UIntPtr minpc;
    public System.UIntPtr maxpc;
    public System.UIntPtr text;
    public System.UIntPtr etext;
    public System.UIntPtr noptrdata;
    public System.UIntPtr enoptrdata;
    public System.UIntPtr data;
    public System.UIntPtr edata;
    public System.UIntPtr bss;
    public System.UIntPtr ebss;
    public System.UIntPtr noptrbss;
    public System.UIntPtr enoptrbss;
    public System.UIntPtr end;
    public System.UIntPtr gcdata;
    public System.UIntPtr gcbss;
    public System.UIntPtr types;
    public System.UIntPtr etypes;
    public slice<textsect> textsectmap;
    public slice<int> typelinks; // offsets from types
    public slice<ptr<itab>> itablinks;
    public slice<ptabEntry> ptab;
    public @string pluginpath;
    public slice<modulehash> pkghashes;
    public @string modulename;
    public slice<modulehash> modulehashes;
    public byte hasmain; // 1 if module contains the main function, 0 otherwise

    public bitvector gcdatamask;
    public bitvector gcbssmask;
    public map<typeOff, ptr<_type>> typemap; // offset to *_rtype in previous module

    public bool bad; // module failed to load and should be ignored

    public ptr<moduledata> next;
}

// A modulehash is used to compare the ABI of a new module or a
// package in a new module with the loaded program.
//
// For each shared library a module links against, the linker creates an entry in the
// moduledata.modulehashes slice containing the name of the module, the abi hash seen
// at link time and a pointer to the runtime abi hash. These are checked in
// moduledataverify1 below.
//
// For each loaded plugin, the pkghashes slice has a modulehash of the
// newly loaded package that can be used to check the plugin's version of
// a package against any previously loaded version of the package.
// This is done in plugin.lastmoduleinit.
private partial struct modulehash {
    public @string modulename;
    public @string linktimehash;
    public ptr<@string> runtimehash;
}

// pinnedTypemaps are the map[typeOff]*_type from the moduledata objects.
//
// These typemap objects are allocated at run time on the heap, but the
// only direct reference to them is in the moduledata, created by the
// linker and marked SNOPTRDATA so it is ignored by the GC.
//
// To make sure the map isn't collected, we keep a second reference here.
private static slice<map<typeOff, ptr<_type>>> pinnedTypemaps = default;

private static moduledata firstmoduledata = default; // linker symbol
private static ptr<moduledata> lastmoduledatap; // linker symbol
private static ptr<slice<ptr<moduledata>>> modulesSlice; // see activeModules

// activeModules returns a slice of active modules.
//
// A module is active once its gcdatamask and gcbssmask have been
// assembled and it is usable by the GC.
//
// This is nosplit/nowritebarrier because it is called by the
// cgo pointer checking code.
//go:nosplit
//go:nowritebarrier
private static slice<ptr<moduledata>> activeModules() {
    ptr<slice<ptr<moduledata>>> p = new ptr<ptr<slice<ptr<moduledata>>>>(atomic.Loadp(@unsafe.Pointer(_addr_modulesSlice)));
    if (p == null) {
        return null;
    }
    return p.val;

}

// modulesinit creates the active modules slice out of all loaded modules.
//
// When a module is first loaded by the dynamic linker, an .init_array
// function (written by cmd/link) is invoked to call addmoduledata,
// appending to the module to the linked list that starts with
// firstmoduledata.
//
// There are two times this can happen in the lifecycle of a Go
// program. First, if compiled with -linkshared, a number of modules
// built with -buildmode=shared can be loaded at program initialization.
// Second, a Go program can load a module while running that was built
// with -buildmode=plugin.
//
// After loading, this function is called which initializes the
// moduledata so it is usable by the GC and creates a new activeModules
// list.
//
// Only one goroutine may call modulesinit at a time.
private static void modulesinit() {
    ptr<var> modules = @new<*moduledata>();
    {
        var md__prev1 = md;

        var md = _addr_firstmoduledata;

        while (md != null) {
            if (md.bad) {
                continue;
            md = md.next;
            }

            modules.val = append(modules.val, md);
            if (md.gcdatamask == (new bitvector())) {
                md.gcdatamask = progToPointerMask((byte.val)(@unsafe.Pointer(md.gcdata)), md.edata - md.data);
                md.gcbssmask = progToPointerMask((byte.val)(@unsafe.Pointer(md.gcbss)), md.ebss - md.bss);
            }

        }

        md = md__prev1;
    } 

    // Modules appear in the moduledata linked list in the order they are
    // loaded by the dynamic loader, with one exception: the
    // firstmoduledata itself the module that contains the runtime. This
    // is not always the first module (when using -buildmode=shared, it
    // is typically libstd.so, the second module). The order matters for
    // typelinksinit, so we swap the first module with whatever module
    // contains the main function.
    //
    // See Issue #18729.
    {
        var md__prev1 = md;

        foreach (var (__i, __md) in modules.val) {
            i = __i;
            md = __md;
            if (md.hasmain != 0) {
                (modules.val)[0] = md;
                (modules.val)[i] = _addr_firstmoduledata;
                break;
            }
        }
        md = md__prev1;
    }

    atomicstorep(@unsafe.Pointer(_addr_modulesSlice), @unsafe.Pointer(modules));

}

private partial struct functab {
    public System.UIntPtr entry;
    public System.UIntPtr funcoff;
}

// Mapping information for secondary text sections

private partial struct textsect {
    public System.UIntPtr vaddr; // prelinked section vaddr
    public System.UIntPtr length; // section length
    public System.UIntPtr baseaddr; // relocated section address
}

private static readonly nint minfunc = 16; // minimum function size
 // minimum function size
private static readonly nint pcbucketsize = 256 * minfunc; // size of bucket in the pc->func lookup table

// findfunctab is an array of these structures.
// Each bucket represents 4096 bytes of the text segment.
// Each subbucket represents 256 bytes of the text segment.
// To find a function given a pc, locate the bucket and subbucket for
// that pc. Add together the idx and subbucket value to obtain a
// function index. Then scan the functab array starting at that
// index to find the target function.
// This table uses 20 bytes for every 4096 bytes of code, or ~0.5% overhead.
 // size of bucket in the pc->func lookup table

// findfunctab is an array of these structures.
// Each bucket represents 4096 bytes of the text segment.
// Each subbucket represents 256 bytes of the text segment.
// To find a function given a pc, locate the bucket and subbucket for
// that pc. Add together the idx and subbucket value to obtain a
// function index. Then scan the functab array starting at that
// index to find the target function.
// This table uses 20 bytes for every 4096 bytes of code, or ~0.5% overhead.
private partial struct findfuncbucket {
    public uint idx;
    public array<byte> subbuckets;
}

private static void moduledataverify() {
    {
        var datap = _addr_firstmoduledata;

        while (datap != null) {
            moduledataverify1(_addr_datap);
            datap = datap.next;
        }
    }

}

private static readonly var debugPcln = false;



private static void moduledataverify1(ptr<moduledata> _addr_datap) {
    ref moduledata datap = ref _addr_datap.val;
 
    // Check that the pclntab's format is valid.
    var hdr = datap.pcHeader;
    if (hdr.magic != 0xfffffffa || hdr.pad1 != 0 || hdr.pad2 != 0 || hdr.minLC != sys.PCQuantum || hdr.ptrSize != sys.PtrSize) {
        print("runtime: function symbol table header:", hex(hdr.magic), hex(hdr.pad1), hex(hdr.pad2), hex(hdr.minLC), hex(hdr.ptrSize));
        if (datap.pluginpath != "") {
            print(", plugin:", datap.pluginpath);
        }
        println();
        throw("invalid function symbol table\n");

    }
    var nftab = len(datap.ftab) - 1;
    for (nint i = 0; i < nftab; i++) { 
        // NOTE: ftab[nftab].entry is legal; it is the address beyond the final function.
        if (datap.ftab[i].entry > datap.ftab[i + 1].entry) {
            funcInfo f1 = new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[i].funcoff])),datap);
            funcInfo f2 = new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[i+1].funcoff])),datap);
            @string f2name = "end";
            if (i + 1 < nftab) {
                f2name = funcname(f2);
            }
            print("function symbol table not sorted by program counter:", hex(datap.ftab[i].entry), funcname(f1), ">", hex(datap.ftab[i + 1].entry), f2name);
            if (datap.pluginpath != "") {
                print(", plugin:", datap.pluginpath);
            }
            println();
            for (nint j = 0; j <= i; j++) {
                print("\t", hex(datap.ftab[j].entry), " ", funcname(new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[j].funcoff])),datap)), "\n");
            }

            if (GOOS == "aix" && isarchive) {
                println("-Wl,-bnoobjreorder is mandatory on aix/ppc64 with c-archive");
            }
            throw("invalid runtime symbol table");
        }
    }

    if (datap.minpc != datap.ftab[0].entry || datap.maxpc != datap.ftab[nftab].entry) {
        throw("minpc or maxpc invalid");
    }
    foreach (var (_, modulehash) in datap.modulehashes) {
        if (modulehash.linktimehash != modulehash.runtimehash.val) {
            println("abi mismatch detected between", datap.modulename, "and", modulehash.modulename);
            throw("abi mismatch");
        }
    }
}

// FuncForPC returns a *Func describing the function that contains the
// given program counter address, or else nil.
//
// If pc represents multiple functions because of inlining, it returns
// the *Func describing the innermost function, but with an entry of
// the outermost function.
public static ptr<Func> FuncForPC(System.UIntPtr pc) {
    var f = findfunc(pc);
    if (!f.valid()) {
        return _addr_null!;
    }
    {
        var inldata = funcdata(f, _FUNCDATA_InlTree);

        if (inldata != null) { 
            // Note: strict=false so bad PCs (those between functions) don't crash the runtime.
            // We just report the preceding function in that situation. See issue 29735.
            // TODO: Perhaps we should report no function at all in that case.
            // The runtime currently doesn't have function end info, alas.
            {
                var ix = pcdatavalue1(f, _PCDATA_InlTreeIndex, pc, _addr_null, false);

                if (ix >= 0) {
                    ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                    var name = funcnameFromNameoff(f, inltree[ix].func_);
                    var (file, line) = funcline(f, pc);
                    ptr<funcinl> fi = addr(new funcinl(entry:f.entry,name:name,file:file,line:int(line),));
                    return _addr_(Func.val)(@unsafe.Pointer(fi))!;
                }

            }

        }
    }

    return _addr_f._Func()!;

}

// Name returns the name of the function.
private static @string Name(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f == null) {
        return "";
    }
    var fn = f.raw();
    if (fn.entry == 0) { // inlined version
        var fi = (funcinl.val)(@unsafe.Pointer(fn));
        return fi.name;

    }
    return funcname(f.funcInfo());

}

// Entry returns the entry address of the function.
private static System.UIntPtr Entry(this ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var fn = f.raw();
    if (fn.entry == 0) { // inlined version
        var fi = (funcinl.val)(@unsafe.Pointer(fn));
        return fi.entry;

    }
    return fn.entry;

}

// FileLine returns the file name and line number of the
// source code corresponding to the program counter pc.
// The result will not be accurate if pc is not a program
// counter within f.
private static (@string, nint) FileLine(this ptr<Func> _addr_f, System.UIntPtr pc) {
    @string file = default;
    nint line = default;
    ref Func f = ref _addr_f.val;

    var fn = f.raw();
    if (fn.entry == 0) { // inlined version
        var fi = (funcinl.val)(@unsafe.Pointer(fn));
        return (fi.file, fi.line);

    }
    var (file, line32) = funcline1(f.funcInfo(), pc, false);
    return (file, int(line32));

}

// findmoduledatap looks up the moduledata for a PC.
//
// It is nosplit because it's part of the isgoexception
// implementation.
//
//go:nosplit
private static ptr<moduledata> findmoduledatap(System.UIntPtr pc) {
    {
        var datap = _addr_firstmoduledata;

        while (datap != null) {
            if (datap.minpc <= pc && pc < datap.maxpc) {
                return _addr_datap!;
            datap = datap.next;
            }

        }
    }
    return _addr_null!;

}

private partial struct funcInfo {
    public ref ptr<_func> ptr<_func> => ref ptr<_func>_ptr;
    public ptr<moduledata> datap;
}

private static bool valid(this funcInfo f) {
    return f._func != null;
}

private static ptr<Func> _Func(this funcInfo f) {
    return _addr_(Func.val)(@unsafe.Pointer(f._func))!;
}

// findfunc looks up function metadata for a PC.
//
// It is nosplit because it's part of the isgoexception
// implementation.
//
//go:nosplit
private static funcInfo findfunc(System.UIntPtr pc) {
    var datap = findmoduledatap(pc);
    if (datap == null) {
        return new funcInfo();
    }
    const var nsub = uintptr(len(new findfuncbucket().subbuckets));



    var x = pc - datap.minpc;
    var b = x / pcbucketsize;
    var i = x % pcbucketsize / (pcbucketsize / nsub);

    var ffb = (findfuncbucket.val)(add(@unsafe.Pointer(datap.findfunctab), b * @unsafe.Sizeof(new findfuncbucket())));
    var idx = ffb.idx + uint32(ffb.subbuckets[i]); 

    // If the idx is beyond the end of the ftab, set it to the end of the table and search backward.
    // This situation can occur if multiple text sections are generated to handle large text sections
    // and the linker has inserted jump tables between them.

    if (idx >= uint32(len(datap.ftab))) {
        idx = uint32(len(datap.ftab) - 1);
    }
    if (pc < datap.ftab[idx].entry) { 
        // With multiple text sections, the idx might reference a function address that
        // is higher than the pc being searched, so search backward until the matching address is found.

        while (datap.ftab[idx].entry > pc && idx > 0) {
            idx--;
        }
    else

        if (idx == 0) {
            throw("findfunc: bad findfunctab entry idx");
        }
    } { 
        // linear search to find func with pc >= entry.
        while (datap.ftab[idx + 1].entry <= pc) {
            idx++;
        }

    }
    var funcoff = datap.ftab[idx].funcoff;
    if (funcoff == ~uintptr(0)) { 
        // With multiple text sections, there may be functions inserted by the external
        // linker that are not known by Go. This means there may be holes in the PC
        // range covered by the func table. The invalid funcoff value indicates a hole.
        // See also cmd/link/internal/ld/pcln.go:pclntab
        return new funcInfo();

    }
    return new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[funcoff])),datap);

}

private partial struct pcvalueCache {
    public array<array<pcvalueCacheEnt>> entries;
}

private partial struct pcvalueCacheEnt {
    public System.UIntPtr targetpc;
    public uint off; // val is the value of this cached pcvalue entry.
    public int val;
}

// pcvalueCacheKey returns the outermost index in a pcvalueCache to use for targetpc.
// It must be very cheap to calculate.
// For now, align to sys.PtrSize and reduce mod the number of entries.
// In practice, this appears to be fairly randomly and evenly distributed.
private static System.UIntPtr pcvalueCacheKey(System.UIntPtr targetpc) {
    return (targetpc / sys.PtrSize) % uintptr(len(new pcvalueCache().entries));
}

// Returns the PCData value, and the PC where this value starts.
// TODO: the start PC is returned only when cache is nil.
private static (int, System.UIntPtr) pcvalue(funcInfo f, uint off, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache, bool strict) {
    int _p0 = default;
    System.UIntPtr _p0 = default;
    ref pcvalueCache cache = ref _addr_cache.val;

    if (off == 0) {
        return (-1, 0);
    }
    if (cache != null) {
        var x = pcvalueCacheKey(targetpc);
        foreach (var (i) in cache.entries[x]) { 
            // We check off first because we're more
            // likely to have multiple entries with
            // different offsets for the same targetpc
            // than the other way around, so we'll usually
            // fail in the first clause.
            var ent = _addr_cache.entries[x][i];
            if (ent.off == off && ent.targetpc == targetpc) {
                return (ent.val, 0);
            }

        }
    }
    if (!f.valid()) {
        if (strict && panicking == 0) {
            print("runtime: no module data for ", hex(f.entry), "\n");
            throw("no module data");
        }
        return (-1, 0);

    }
    var datap = f.datap;
    var p = datap.pctab[(int)off..];
    ref var pc = ref heap(f.entry, out ptr<var> _addr_pc);
    var prevpc = pc;
    ref var val = ref heap(int32(-1), out ptr<var> _addr_val);
    while (true) {
        bool ok = default;
        p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
        if (!ok) {
            break;
        }
        if (targetpc < pc) { 
            // Replace a random entry in the cache. Random
            // replacement prevents a performance cliff if
            // a recursive stack's cycle is slightly
            // larger than the cache.
            // Put the new element at the beginning,
            // since it is the most likely to be newly used.
            if (cache != null) {
                x = pcvalueCacheKey(targetpc);
                var e = _addr_cache.entries[x];
                var ci = fastrand() % uint32(len(cache.entries[x]));
                e[ci] = e[0];
                e[0] = new pcvalueCacheEnt(targetpc:targetpc,off:off,val:val,);
            }

            return (val, prevpc);

        }
        prevpc = pc;

    } 

    // If there was a table, it should have covered all program counters.
    // If not, something is wrong.
    if (panicking != 0 || !strict) {
        return (-1, 0);
    }
    print("runtime: invalid pc-encoded table f=", funcname(f), " pc=", hex(pc), " targetpc=", hex(targetpc), " tab=", p, "\n");

    p = datap.pctab[(int)off..];
    pc = f.entry;
    val = -1;
    while (true) {
        ok = default;
        p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
        if (!ok) {
            break;
        }
        print("\tvalue=", val, " until pc=", hex(pc), "\n");

    }

    throw("invalid runtime symbol table");
    return (-1, 0);

}

private static ptr<byte> cfuncname(funcInfo f) {
    if (!f.valid() || f.nameoff == 0) {
        return _addr_null!;
    }
    return _addr__addr_f.datap.funcnametab[f.nameoff]!;

}

private static @string funcname(funcInfo f) {
    return gostringnocopy(cfuncname(f));
}

private static @string funcpkgpath(funcInfo f) {
    var name = funcname(f);
    var i = len(name) - 1;
    while (i > 0) {
        if (name[i] == '/') {
            break;
        i--;
        }
    }
    while (i < len(name)) {
        if (name[i] == '.') {
            break;
        i++;
        }
    }
    return name[..(int)i];

}

private static ptr<byte> cfuncnameFromNameoff(funcInfo f, int nameoff) {
    if (!f.valid()) {
        return _addr_null!;
    }
    return _addr__addr_f.datap.funcnametab[nameoff]!;

}

private static @string funcnameFromNameoff(funcInfo f, int nameoff) {
    return gostringnocopy(cfuncnameFromNameoff(f, nameoff));
}

private static @string funcfile(funcInfo f, int fileno) {
    var datap = f.datap;
    if (!f.valid()) {
        return "?";
    }
    {
        var fileoff = datap.cutab[f.cuOffset + uint32(fileno)];

        if (fileoff != ~uint32(0)) {
            return gostringnocopy(_addr_datap.filetab[fileoff]);
        }
    } 
    // pcln section is corrupt.
    return "?";

}

private static (@string, int) funcline1(funcInfo f, System.UIntPtr targetpc, bool strict) {
    @string file = default;
    int line = default;

    var datap = f.datap;
    if (!f.valid()) {
        return ("?", 0);
    }
    var (fileno, _) = pcvalue(f, f.pcfile, targetpc, _addr_null, strict);
    line, _ = pcvalue(f, f.pcln, targetpc, _addr_null, strict);
    if (fileno == -1 || line == -1 || int(fileno) >= len(datap.filetab)) { 
        // print("looking for ", hex(targetpc), " in ", funcname(f), " got file=", fileno, " line=", lineno, "\n")
        return ("?", 0);

    }
    file = funcfile(f, fileno);
    return ;

}

private static (@string, int) funcline(funcInfo f, System.UIntPtr targetpc) {
    @string file = default;
    int line = default;

    return funcline1(f, targetpc, true);
}

private static int funcspdelta(funcInfo f, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache) {
    ref pcvalueCache cache = ref _addr_cache.val;

    var (x, _) = pcvalue(f, f.pcsp, targetpc, _addr_cache, true);
    if (x & (sys.PtrSize - 1) != 0) {
        print("invalid spdelta ", funcname(f), " ", hex(f.entry), " ", hex(targetpc), " ", hex(f.pcsp), " ", x, "\n");
    }
    return x;

}

// funcMaxSPDelta returns the maximum spdelta at any point in f.
private static int funcMaxSPDelta(funcInfo f) {
    var datap = f.datap;
    var p = datap.pctab[(int)f.pcsp..];
    ref var pc = ref heap(f.entry, out ptr<var> _addr_pc);
    ref var val = ref heap(int32(-1), out ptr<var> _addr_val);
    var max = int32(0);
    while (true) {
        bool ok = default;
        p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
        if (!ok) {
            return max;
        }
        if (val > max) {
            max = val;
        }
    }

}

private static uint pcdatastart(funcInfo f, uint table) {
    return new ptr<ptr<ptr<uint>>>(add(@unsafe.Pointer(_addr_f.nfuncdata), @unsafe.Sizeof(f.nfuncdata) + uintptr(table) * 4));
}

private static int pcdatavalue(funcInfo f, uint table, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache) {
    ref pcvalueCache cache = ref _addr_cache.val;

    if (table >= f.npcdata) {
        return -1;
    }
    var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, _addr_cache, true);
    return r;

}

private static int pcdatavalue1(funcInfo f, uint table, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache, bool strict) {
    ref pcvalueCache cache = ref _addr_cache.val;

    if (table >= f.npcdata) {
        return -1;
    }
    var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, _addr_cache, strict);
    return r;

}

// Like pcdatavalue, but also return the start PC of this PCData value.
// It doesn't take a cache.
private static (int, System.UIntPtr) pcdatavalue2(funcInfo f, uint table, System.UIntPtr targetpc) {
    int _p0 = default;
    System.UIntPtr _p0 = default;

    if (table >= f.npcdata) {
        return (-1, 0);
    }
    return pcvalue(f, pcdatastart(f, table), targetpc, _addr_null, true);

}

private static unsafe.Pointer funcdata(funcInfo f, byte i) {
    if (i < 0 || i >= f.nfuncdata) {
        return null;
    }
    var p = add(@unsafe.Pointer(_addr_f.nfuncdata), @unsafe.Sizeof(f.nfuncdata) + uintptr(f.npcdata) * 4);
    if (sys.PtrSize == 8 && uintptr(p) & 4 != 0) {
        if (uintptr(@unsafe.Pointer(f._func)) & 4 != 0) {
            println("runtime: misaligned func", f._func);
        }
        p = add(p, 4);

    }
    return new ptr<ptr<ptr<unsafe.Pointer>>>(add(p, uintptr(i) * sys.PtrSize));

}

// step advances to the next pc, value pair in the encoded table.
private static (slice<byte>, bool) step(slice<byte> p, ptr<System.UIntPtr> _addr_pc, ptr<int> _addr_val, bool first) {
    slice<byte> newp = default;
    bool ok = default;
    ref System.UIntPtr pc = ref _addr_pc.val;
    ref int val = ref _addr_val.val;
 
    // For both uvdelta and pcdelta, the common case (~70%)
    // is that they are a single byte. If so, avoid calling readvarint.
    var uvdelta = uint32(p[0]);
    if (uvdelta == 0 && !first) {
        return (null, false);
    }
    var n = uint32(1);
    if (uvdelta & 0x80 != 0) {
        n, uvdelta = readvarint(p);
    }
    val += int32(-(uvdelta & 1) ^ (uvdelta >> 1));
    p = p[(int)n..];

    var pcdelta = uint32(p[0]);
    n = 1;
    if (pcdelta & 0x80 != 0) {
        n, pcdelta = readvarint(p);
    }
    p = p[(int)n..];
    pc += uintptr(pcdelta * sys.PCQuantum);
    return (p, true);

}

// readvarint reads a varint from p.
private static (uint, uint) readvarint(slice<byte> p) {
    uint read = default;
    uint val = default;

    uint v = default;    uint shift = default;    uint n = default;

    while (true) {
        var b = p[n];
        n++;
        v |= uint32(b & 0x7F) << (int)((shift & 31));
        if (b & 0x80 == 0) {
            break;
        }
        shift += 7;

    }
    return (n, v);

}

private partial struct stackmap {
    public int n; // number of bitmaps
    public int nbit; // number of bits in each bitmap
    public array<byte> bytedata; // bitmaps, each starting on a byte boundary
}

//go:nowritebarrier
private static bitvector stackmapdata(ptr<stackmap> _addr_stkmap, int n) {
    ref stackmap stkmap = ref _addr_stkmap.val;
 
    // Check this invariant only when stackDebug is on at all.
    // The invariant is already checked by many of stackmapdata's callers,
    // and disabling it by default allows stackmapdata to be inlined.
    if (stackDebug > 0 && (n < 0 || n >= stkmap.n)) {
        throw("stackmapdata: index out of range");
    }
    return new bitvector(stkmap.nbit,addb(&stkmap.bytedata[0],uintptr(n*((stkmap.nbit+7)>>3))));

}

// inlinedCall is the encoding of entries in the FUNCDATA_InlTree table.
private partial struct inlinedCall {
    public short parent; // index of parent in the inltree, or < 0
    public funcID funcID; // type of the called function
    public byte _;
    public int file; // perCU file index for inlined call. See cmd/link:pcln.go
    public int line; // line number of the call site
    public int func_; // offset into pclntab for name of called function
    public int parentPc; // position of an instruction whose source position is the call site (offset from entry)
}

} // end runtime_package
