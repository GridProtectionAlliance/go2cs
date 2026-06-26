// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// Frames may be used to get function/file/line information for a
// slice of PC values returned by [Callers].
[GoType] partial struct Frames {
    // callers is a slice of PCs that have not yet been expanded to frames.
    internal slice<uintptr> callers;
    // nextPC is a next PC to expand ahead of processing callers.
    internal uintptr nextPC;
    // frames is a slice of Frames that have yet to be returned.
    internal slice<Frame> frames;
    internal array<Frame> frameStore = new(2);
}

// Frame is the information returned by [Frames] for each call frame.
[GoType] partial struct Frame {
    // PC is the program counter for the location in this frame.
    // For a frame that calls another frame, this will be the
    // program counter of a call instruction. Because of inlining,
    // multiple frames may have the same PC value, but different
    // symbolic information.
    public uintptr PC;
    // Func is the Func value of this call frame. This may be nil
    // for non-Go code or fully inlined functions.
    public ж<Func> Func;
    // Function is the package path-qualified function name of
    // this call frame. If non-empty, this string uniquely
    // identifies a single function in the program.
    // This may be the empty string if not known.
    // If Func is not nil then Function == Func.Name().
    public @string Function;
    // File and Line are the file name and line number of the
    // location in this frame. For non-leaf frames, this will be
    // the location of a call. These may be the empty string and
    // zero, respectively, if not known.
    public @string File;
    public nint Line;
    // startLine is the line number of the beginning of the function in
    // this frame. Specifically, it is the line number of the func keyword
    // for Go functions. Note that //line directives can change the
    // filename and/or line number arbitrarily within a function, meaning
    // that the Line - startLine offset is not always meaningful.
    //
    // This may be zero if not known.
    internal nint startLine;
    // Entry point program counter for the function; may be zero
    // if not known. If Func is not nil then Entry ==
    // Func.Entry().
    public uintptr Entry;
    // The runtime's internal view of the function. This field
    // is set (funcInfo.valid() returns true) only for Go functions,
    // not for C functions.
    internal ΔfuncInfo funcInfo;
}

// CallersFrames takes a slice of PC values returned by [Callers] and
// prepares to return function/file/line information.
// Do not change the slice until you are done with the [Frames].
public static ж<Frames> CallersFrames(slice<uintptr> callers) {
    var f = Ꮡ(new Frames(callers: callers));
    f.val.frames = (~f).frameStore[..0];
    return f;
}

// Next returns a [Frame] representing the next call frame in the slice
// of PC values. If it has already returned all call frames, Next
// returns a zero [Frame].
//
// The more result indicates whether the next call to Next will return
// a valid [Frame]. It does not necessarily indicate whether this call
// returned one.
//
// See the [Frames] example for idiomatic usage.
[GoRecv] public static (Frame frame, bool more) Next(this ref Frames ci) {
    Frame frame = default!;
    bool more = default!;

    while (len(ci.frames) < 2) {
        // Find the next frame.
        // We need to look for 2 frames so we know what
        // to return for the "more" result.
        if (len(ci.callers) == 0) {
            break;
        }
        uintptr pc = default!;
        if (ci.nextPC != 0){
            (pc, ci.nextPC) = (ci.nextPC, 0);
        } else {
            (pc, ci.callers) = (ci.callers[0], ci.callers[1..]);
        }
        var ΔfuncInfo = findfunc(pc);
        if (!ΔfuncInfo.valid()) {
            if (cgoSymbolizer != nil) {
                // Pre-expand cgo frames. We could do this
                // incrementally, too, but there's no way to
                // avoid allocation in this case anyway.
                ci.frames = append(ci.frames, expandCgoFrames(pc).ꓸꓸꓸ);
            }
            continue;
        }
        var f = ΔfuncInfo._Func();
        var entry = f.Entry();
        if (pc > entry) {
            // We store the pc of the start of the instruction following
            // the instruction in question (the call or the inline mark).
            // This is done for historical reasons, and to make FuncForPC
            // work correctly for entries in the result of runtime.Callers.
            pc--;
        }
        // It's important that interpret pc non-strictly as cgoTraceback may
        // have added bogus PCs with a valid funcInfo but invalid PCDATA.
        var (u, uf) = newInlineUnwinder(ΔfuncInfo, pc);
        var sf = u.srcFunc(uf);
        if (u.isInlined(uf)) {
            // Note: entry is not modified. It always refers to a real frame, not an inlined one.
            // File/line from funcline1 below are already correct.
            f = default!;
            // When CallersFrame is invoked using the PC list returned by Callers,
            // the PC list includes virtual PCs corresponding to each outer frame
            // around an innermost real inlined PC.
            // We also want to support code passing in a PC list extracted from a
            // stack trace, and there only the real PCs are printed, not the virtual ones.
            // So check to see if the implied virtual PC for this PC (obtained from the
            // unwinder itself) is the next PC in ci.callers. If not, insert it.
            // The +1 here correspond to the pc-- above: the output of Callers
            // and therefore the input to CallersFrames is return PCs from the stack;
            // The pc-- backs up into the CALL instruction (not the first byte of the CALL
            // instruction, but good enough to find it nonetheless).
            // There are no cycles in implied virtual PCs (some number of frames were
            // inlined, but that number is finite), so this unpacking cannot cause an infinite loop.
            for (var unext = u.next(uf); unext.valid() && len(ci.callers) > 0 && ci.callers[0] != unext.pc + 1; unext = u.next(unext)) {
                var snext = u.srcFunc(unext);
                if (snext.funcID == abi.FuncIDWrapper && elideWrapperCalling(sf.funcID)) {
                    // Skip, because tracebackPCs (inside runtime.Callers) would too.
                    continue;
                }
                ci.nextPC = unext.pc + 1;
                break;
            }
        }
        ci.frames = append(ci.frames, new Frame(
            PC: pc,
            Func: f,
            Function: funcNameForPrint(sf.name()),
            Entry: entry,
            startLine: ((nint)sf.startLine),
            ΔfuncInfo: ΔfuncInfo
        ));
    }
    // Note: File,Line set below
    // Pop one frame from the frame list. Keep the rest.
    // Avoid allocation in the common case, which is 1 or 2 frames.
    switch (len(ci.frames)) {
    case 0: {
        return (frame, more);
    }
    case 1: {
        frame = ci.frames[0];
        ci.frames = ci.frameStore[..0];
        break;
    }
    case 2: {
        frame = ci.frames[0];
        ci.frameStore[0] = ci.frames[1];
        ci.frames = ci.frameStore[..1];
        break;
    }
    default: {
        frame = ci.frames[0];
        ci.frames = ci.frames[1..];
        break;
    }}

    // In the rare case when there are no frames at all, we return Frame{}.
    more = len(ci.frames) > 0;
    if (frame.funcInfo.valid()) {
        // Compute file/line just before we need to return it,
        // as it can be expensive. This avoids computing file/line
        // for the Frame we find but don't return. See issue 32093.
        var (file, line) = funcline1(frame.funcInfo, frame.PC, false);
        (frame.File, frame.Line) = (file, ((nint)line));
    }
    return (frame, more);
}

// runtime_FrameStartLine returns the start line of the function in a Frame.
//
// runtime_FrameStartLine should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/grafana/pyroscope-go/godeltaprof
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname runtime_FrameStartLine runtime/pprof.runtime_FrameStartLine
internal static nint runtime_FrameStartLine(ж<Frame> Ꮡf) {
    ref var f = ref Ꮡf.val;

    return f.startLine;
}

// runtime_FrameSymbolName returns the full symbol name of the function in a Frame.
// For generic functions this differs from f.Function in that this doesn't replace
// the shape name to "...".
//
// runtime_FrameSymbolName should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/grafana/pyroscope-go/godeltaprof
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname runtime_FrameSymbolName runtime/pprof.runtime_FrameSymbolName
internal static @string runtime_FrameSymbolName(ж<Frame> Ꮡf) {
    ref var f = ref Ꮡf.val;

    if (!f.funcInfo.valid()) {
        return f.Function;
    }
    var (u, uf) = newInlineUnwinder(f.funcInfo, f.PC);
    var sf = u.srcFunc(uf);
    return sf.name();
}

// runtime_expandFinalInlineFrame expands the final pc in stk to include all
// "callers" if pc is inline.
//
// runtime_expandFinalInlineFrame should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/grafana/pyroscope-go/godeltaprof
//   - github.com/pyroscope-io/godeltaprof
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname runtime_expandFinalInlineFrame runtime/pprof.runtime_expandFinalInlineFrame
internal static slice<uintptr> runtime_expandFinalInlineFrame(slice<uintptr> stk) {
    // TODO: It would be more efficient to report only physical PCs to pprof and
    // just expand the whole stack.
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
    var (u, uf) = newInlineUnwinder(f, tracepc);
    if (!u.isInlined(uf)) {
        // Nothing inline at tracepc.
        return stk;
    }
    // Treat the previous func as normal. We haven't actually checked, but
    // since this pc was included in the stack, we know it shouldn't be
    // elided.
    var calleeID = abi.FuncIDNormal;
    // Remove pc from stk; we'll re-add it below.
    stk = stk[..(int)(len(stk) - 1)];
    for (; uf.valid(); uf = u.next(uf)) {
        var funcID = u.srcFunc(uf).funcID;
        if (funcID == abi.FuncIDWrapper && elideWrapperCalling(calleeID)){
        } else {
            // ignore wrappers
            stk = append(stk, uf.pc + 1);
        }
        calleeID = funcID;
    }
    return stk;
}

// expandCgoFrames expands frame information for pc, known to be
// a non-Go function, using the cgoSymbolizer hook. expandCgoFrames
// returns nil if pc could not be expanded.
internal static slice<Frame> expandCgoFrames(uintptr pc) {
    ref var arg = ref heap<cgoSymbolizerArg>(out var Ꮡarg);
    arg = new cgoSymbolizerArg(pc: pc);
    callCgoSymbolizer(Ꮡarg);
    if (arg.file == nil && arg.funcName == nil) {
        // No useful information from symbolizer.
        return default!;
    }
    slice<Frame> frames = default!;
    while (ᐧ) {
        frames = append(frames, new Frame(
            PC: pc,
            Func: default!,
            Function: gostring(arg.funcName),
            File: gostring(arg.file),
            Line: ((nint)arg.lineno),
            Entry: arg.entry
        ));
        // funcInfo is zero, which implies !funcInfo.valid().
        // That ensures that we use the File/Line info given here.
        if (arg.more == 0) {
            break;
        }
        callCgoSymbolizer(Ꮡarg);
    }
    // No more frames for this PC. Tell the symbolizer we are done.
    // We don't try to maintain a single cgoSymbolizerArg for the
    // whole use of Frames, because there would be no good way to tell
    // the symbolizer when we are done.
    arg.pc = 0;
    callCgoSymbolizer(Ꮡarg);
    return frames;
}

// unexported field to disallow conversions
[GoType("dyn")] partial struct Func_opaque {
}

// NOTE: Func does not expose the actual unexported fields, because we return *Func
// values to users, and we want to keep them from being able to overwrite the data
// with (say) *f = Func{}.
// All code operating on a *Func must call raw() to get the *_func
// or funcInfo() to get the funcInfo instead.

// A Func represents a Go function in the running binary.
[GoType] partial struct Func {
    internal Func_opaque opaque;
}

[GoRecv] internal static ж<_func> raw(this ref Func f) {
    return (ж<_func>)(uintptr)(@unsafe.Pointer.FromRef(ref f));
}

[GoRecv] internal static ΔfuncInfo funcInfo(this ref Func f) {
    return f.raw().funcInfo();
}

[GoRecv] internal static ΔfuncInfo funcInfo(this ref _func f) {
    // Find the module containing fn. fn is located in the pclntable.
    // The unsafe.Pointer to uintptr conversions and arithmetic
    // are safe because we are working with module addresses.
    var ptr = ((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref f));
    ж<moduledata> mod = default!;
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        if (len((~datap).pclntable) == 0) {
            continue;
        }
        var @base = ((uintptr)new @unsafe.Pointer(Ꮡ((~datap).pclntable, 0)));
        if (@base <= ptr && ptr < @base + ((uintptr)len((~datap).pclntable))) {
            mod = datap;
            break;
        }
    }
    return new ΔfuncInfo(f, mod);
}

// pcHeader holds data used by the pclntab lookups.
[GoType] partial struct pcHeader {
    internal uint32 magic;  // 0xFFFFFFF1
    internal uint8 pad1;   // 0,0
    internal uint8 pad2;
    internal uint8 minLC;   // min instruction size
    internal uint8 ptrSize;   // size of a ptr in bytes
    internal nint nfunc;    // number of functions in the module
    internal nuint nfiles;   // number of entries in the file tab
    internal uintptr textStart; // base for function entry PC offsets in this module, equal to moduledata.text
    internal uintptr funcnameOffset; // offset to the funcnametab variable from pcHeader
    internal uintptr cuOffset; // offset to the cutab variable from pcHeader
    internal uintptr filetabOffset; // offset to the filetab variable from pcHeader
    internal uintptr pctabOffset; // offset to the pctab variable from pcHeader
    internal uintptr pclnOffset; // offset to the pclntab variable from pcHeader
}

// moduledata records information about the layout of the executable
// image. It is written by the linker. Any changes here must be
// matched changes to the code in cmd/link/internal/ld/symtab.go:symtab.
// moduledata is stored in statically allocated non-pointer memory;
// none of the pointers here are visible to the garbage collector.
[GoType] partial struct moduledata {
    public partial ref runtime.@internal.sys_package.NotInHeap NotInHeap { get; } // Only in static data
    internal ж<pcHeader> pcHeader;
    internal slice<byte> funcnametab;
    internal slice<uint32> cutab;
    internal slice<byte> filetab;
    internal slice<byte> pctab;
    internal slice<byte> pclntable;
    internal slice<functab> ftab;
    internal uintptr findfunctab;
    internal uintptr minpc;
    internal uintptr maxpc;
    internal uintptr text;
    internal uintptr etext;
    internal uintptr noptrdata;
    internal uintptr enoptrdata;
    internal uintptr data;
    internal uintptr edata;
    internal uintptr bss;
    internal uintptr ebss;
    internal uintptr noptrbss;
    internal uintptr enoptrbss;
    internal uintptr covctrs;
    internal uintptr ecovctrs;
    internal uintptr end;
    internal uintptr gcdata;
    internal uintptr gcbss;
    internal uintptr types;
    internal uintptr etypes;
    internal uintptr rodata;
    internal uintptr gofunc; // go.func.*
    internal slice<textsect> textsectmap;
    internal slice<int32> typelinks; // offsets from types
    internal slice<ж<itab>> itablinks;
    internal slice<ptabEntry> ptab;
    internal @string pluginpath;
    internal slice<modulehash> pkghashes;
    // This slice records the initializing tasks that need to be
    // done to start up the program. It is built by the linker.
    internal slice<ж<initTask>> inittasks;
    internal @string modulename;
    internal slice<modulehash> modulehashes;
    internal uint8 hasmain; // 1 if module contains the main function, 0 otherwise
    internal bool bad;  // module failed to load and should be ignored
    internal bitvector gcdatamask;
    internal bitvector gcbssmask;
    internal map<typeOff, ж<runtime._type>> typemap; // offset to *_rtype in previous module
    internal ж<moduledata> next;
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
[GoType] partial struct modulehash {
    internal @string modulename;
    internal @string linktimehash;
    internal ж<@string> runtimehash;
}

// pinnedTypemaps are the map[typeOff]*_type from the moduledata objects.
//
// These typemap objects are allocated at run time on the heap, but the
// only direct reference to them is in the moduledata, created by the
// linker and marked SNOPTRDATA so it is ignored by the GC.
//
// To make sure the map isn't collected, we keep a second reference here.
internal static slice<map<typeOff, ж<runtime._type>>> pinnedTypemaps;

internal static moduledata firstmoduledata; // linker symbol

// lastmoduledatap should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname lastmoduledatap
internal static ж<moduledata> lastmoduledatap; // linker symbol

internal static ж<slice<ж<moduledata>>> modulesSlice;    // see activeModules

// activeModules returns a slice of active modules.
//
// A module is active once its gcdatamask and gcbssmask have been
// assembled and it is usable by the GC.
//
// This is nosplit/nowritebarrier because it is called by the
// cgo pointer checking code.
//
//go:nosplit
//go:nowritebarrier
internal static slice<ж<moduledata>> activeModules() {
    var Δp = (ж<slice<ж<moduledata>>>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(modulesSlice)))));
    if (Δp == nil) {
        return default!;
    }
    return Δp.val;
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
internal static void modulesinit() {
    var modules = @new<slice<ж<moduledata>>>();
    for (var md = Ꮡ(firstmoduledata); md != nil; md = md.val.next) {
        if ((~md).bad) {
            continue;
        }
        modules.val = append(modules.val, md);
        if ((~md).gcdatamask == (new bitvector(nil))) {
            var scanDataSize = (~md).edata - (~md).data;
            md.val.gcdatamask = progToPointerMask((ж<byte>)(uintptr)(((@unsafe.Pointer)(~md).gcdata)), scanDataSize);
            var scanBSSSize = (~md).ebss - (~md).bss;
            md.val.gcbssmask = progToPointerMask((ж<byte>)(uintptr)(((@unsafe.Pointer)(~md).gcbss)), scanBSSSize);
            gcController.addGlobals(((int64)(scanDataSize + scanBSSSize)));
        }
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
    foreach (var (i, md) in modules.val) {
        if ((~md).hasmain != 0) {
            (ж<ж<slice<ж<moduledata>>>>)[0] = md;
            (ж<ж<slice<ж<moduledata>>>>)[i] = Ꮡ(firstmoduledata);
            break;
        }
    }
    atomicstorep(((@unsafe.Pointer)(Ꮡ(modulesSlice))), new @unsafe.Pointer(modules));
}

[GoType] partial struct functab {
    internal uint32 entryoff; // relative to runtime.text
    internal uint32 funcoff;
}

// Mapping information for secondary text sections
[GoType] partial struct textsect {
    internal uintptr vaddr; // prelinked section vaddr
    internal uintptr end; // vaddr + section length
    internal uintptr baseaddr; // relocated section address
}

// findfuncbucket is an array of these structures.
// Each bucket represents 4096 bytes of the text segment.
// Each subbucket represents 256 bytes of the text segment.
// To find a function given a pc, locate the bucket and subbucket for
// that pc. Add together the idx and subbucket value to obtain a
// function index. Then scan the functab array starting at that
// index to find the target function.
// This table uses 20 bytes for every 4096 bytes of code, or ~0.5% overhead.
[GoType] partial struct findfuncbucket {
    internal uint32 idx;
    internal array<byte> subbuckets = new(16);
}

internal static void moduledataverify() {
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        moduledataverify1(datap);
    }
}

internal const bool debugPcln = false;

// moduledataverify1 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname moduledataverify1
internal static void moduledataverify1(ж<moduledata> Ꮡdatap) {
    ref var datap = ref Ꮡdatap.val;

    // Check that the pclntab's format is valid.
    var hdr = datap.pcHeader;
    if ((~hdr).magic != (nint)4294967281L || (~hdr).pad1 != 0 || (~hdr).pad2 != 0 || (~hdr).minLC != sys.PCQuantum || (~hdr).ptrSize != goarch.PtrSize || (~hdr).textStart != datap.text) {
        println("runtime: pcHeader: magic=", ((Δhex)(~hdr).magic), "pad1=", (~hdr).pad1, "pad2=", (~hdr).pad2,
            "minLC=", (~hdr).minLC, "ptrSize=", (~hdr).ptrSize, "pcHeader.textStart=", ((Δhex)(~hdr).textStart),
            "text=", ((Δhex)datap.text), "pluginpath=", datap.pluginpath);
        @throw("invalid function symbol table"u8);
    }
    // ftab is lookup table for function by program counter.
    nint nftab = len(datap.ftab) - 1;
    for (nint i = 0; i < nftab; i++) {
        // NOTE: ftab[nftab].entry is legal; it is the address beyond the final function.
        if (datap.ftab[i].entryoff > datap.ftab[i + 1].entryoff) {
            var f1 = new ΔfuncInfo((ж<_func>)(uintptr)(new @unsafe.Pointer(Ꮡ(datap.pclntable, datap.ftab[i].funcoff))), Ꮡdatap);
            var f2 = new ΔfuncInfo((ж<_func>)(uintptr)(new @unsafe.Pointer(Ꮡ(datap.pclntable, datap.ftab[i + 1].funcoff))), Ꮡdatap);
            @string f2name = "end"u8;
            if (i + 1 < nftab) {
                f2name = funcname(f2);
            }
            println("function symbol table not sorted by PC offset:", ((Δhex)datap.ftab[i].entryoff), funcname(f1), ">", ((Δhex)datap.ftab[i + 1].entryoff), f2name, ", plugin:", datap.pluginpath);
            for (nint j = 0; j <= i; j++) {
                println("\t", ((Δhex)datap.ftab[j].entryoff), funcname(new ΔfuncInfo((ж<_func>)(uintptr)(new @unsafe.Pointer(Ꮡ(datap.pclntable, datap.ftab[j].funcoff))), Ꮡdatap)));
            }
            if (GOOS == "aix"u8 && isarchive) {
                println("-Wl,-bnoobjreorder is mandatory on aix/ppc64 with c-archive");
            }
            @throw("invalid runtime symbol table"u8);
        }
    }
    var min = datap.textAddr(datap.ftab[0].entryoff);
    var max = datap.textAddr(datap.ftab[nftab].entryoff);
    if (datap.minpc != min || datap.maxpc != max) {
        println("minpc=", ((Δhex)datap.minpc), "min=", ((Δhex)min), "maxpc=", ((Δhex)datap.maxpc), "max=", ((Δhex)max));
        @throw("minpc or maxpc invalid"u8);
    }
    foreach (var (_, modulehash) in datap.modulehashes) {
        if (modulehash.linktimehash != modulehash.runtimehash.val) {
            println("abi mismatch detected between", datap.modulename, "and", modulehash.modulename);
            @throw("abi mismatch"u8);
        }
    }
}

// textAddr returns md.text + off, with special handling for multiple text sections.
// off is a (virtual) offset computed at internal linking time,
// before the external linker adjusts the sections' base addresses.
//
// The text, or instruction stream is generated as one large buffer.
// The off (offset) for a function is its offset within this buffer.
// If the total text size gets too large, there can be issues on platforms like ppc64
// if the target of calls are too far for the call instruction.
// To resolve the large text issue, the text is split into multiple text sections
// to allow the linker to generate long calls when necessary.
// When this happens, the vaddr for each text section is set to its offset within the text.
// Each function's offset is compared against the section vaddrs and ends to determine the containing section.
// Then the section relative offset is added to the section's
// relocated baseaddr to compute the function address.
//
// It is nosplit because it is part of the findfunc implementation.
//
//go:nosplit
[GoRecv] internal static uintptr textAddr(this ref moduledata md, uint32 off32) {
    var off = ((uintptr)off32);
    var res = md.text + off;
    if (len(md.textsectmap) > 1) {
        foreach (var (i, sect) in md.textsectmap) {
            // For the last section, include the end address (etext), as it is included in the functab.
            if (off >= sect.vaddr && off < sect.end || (i == len(md.textsectmap) - 1 && off == sect.end)) {
                res = sect.baseaddr + off - sect.vaddr;
                break;
            }
        }
        if (res > md.etext && GOARCH != "wasm"u8) {
            // on wasm, functions do not live in the same address space as the linear memory
            println("runtime: textAddr", ((Δhex)res), "out of range", ((Δhex)md.text), "-", ((Δhex)md.etext));
            @throw("runtime: text offset out of range"u8);
        }
    }
    return res;
}

// textOff is the opposite of textAddr. It converts a PC to a (virtual) offset
// to md.text, and returns if the PC is in any Go text section.
//
// It is nosplit because it is part of the findfunc implementation.
//
//go:nosplit
[GoRecv] internal static (uint32, bool) textOff(this ref moduledata md, uintptr pc) {
    var res = ((uint32)(pc - md.text));
    if (len(md.textsectmap) > 1) {
        foreach (var (i, sect) in md.textsectmap) {
            if (sect.baseaddr > pc) {
                // pc is not in any section.
                return (0, false);
            }
            var end = sect.baseaddr + (sect.end - sect.vaddr);
            // For the last section, include the end address (etext), as it is included in the functab.
            if (i == len(md.textsectmap) - 1) {
                end++;
            }
            if (pc < end) {
                res = ((uint32)(pc - sect.baseaddr + sect.vaddr));
                break;
            }
        }
    }
    return (res, true);
}

// funcName returns the string at nameOff in the function name table.
[GoRecv] internal static @string funcName(this ref moduledata md, int32 nameOff) {
    if (nameOff == 0) {
        return ""u8;
    }
    return gostringnocopy(Ꮡ(md.funcnametab[nameOff]));
}

// FuncForPC returns a *[Func] describing the function that contains the
// given program counter address, or else nil.
//
// If pc represents multiple functions because of inlining, it returns
// the *Func describing the innermost function, but with an entry of
// the outermost function.
//
// For completely unclear reasons, even though they can import runtime,
// some widely used packages access this using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname FuncForPC
public static ж<Func> FuncForPC(uintptr pc) {
    var f = findfunc(pc);
    if (!f.valid()) {
        return default!;
    }
    // This must interpret PC non-strictly so bad PCs (those between functions) don't crash the runtime.
    // We just report the preceding function in that situation. See issue 29735.
    // TODO: Perhaps we should report no function at all in that case.
    // The runtime currently doesn't have function end info, alas.
    var (u, uf) = newInlineUnwinder(f, pc);
    if (!u.isInlined(uf)) {
        return f._Func();
    }
    var sf = u.srcFunc(uf);
    var (file, line) = u.fileLine(uf);
    var fi = Ꮡ(new funcinl(
        ones: ~((uint32)0),
        entry: f.entry(), // entry of the real (the outermost) function.

        name: sf.name(),
        file: file,
        line: ((int32)line),
        startLine: sf.startLine
    ));
    return (ж<Func>)(uintptr)(new @unsafe.Pointer(fi));
}

// Name returns the name of the function.
[GoRecv] public static @string Name(this ref Func f) {
    if (f == nil) {
        return ""u8;
    }
    var fn = f.raw();
    if (fn.isInlined()) {
        // inlined version
        var fi = (ж<funcinl>)(uintptr)(new @unsafe.Pointer(fn));
        return funcNameForPrint((~fi).name);
    }
    return funcNameForPrint(funcname(f.funcInfo()));
}

// Entry returns the entry address of the function.
[GoRecv] public static uintptr Entry(this ref Func f) {
    var fn = f.raw();
    if (fn.isInlined()) {
        // inlined version
        var fi = (ж<funcinl>)(uintptr)(new @unsafe.Pointer(fn));
        return (~fi).entry;
    }
    return fn.funcInfo().entry();
}

// FileLine returns the file name and line number of the
// source code corresponding to the program counter pc.
// The result will not be accurate if pc is not a program
// counter within f.
[GoRecv] public static (@string file, nint line) FileLine(this ref Func f, uintptr pc) {
    @string file = default!;
    nint line = default!;

    var fn = f.raw();
    if (fn.isInlined()) {
        // inlined version
        var fi = (ж<funcinl>)(uintptr)(new @unsafe.Pointer(fn));
        return ((~fi).file, ((nint)(~fi).line));
    }
    // Pass strict=false here, because anyone can call this function,
    // and they might just be wrong about targetpc belonging to f.
    var (file, line32) = funcline1(f.funcInfo(), pc, false);
    return (file, ((nint)line32));
}

// startLine returns the starting line number of the function. i.e., the line
// number of the func keyword.
[GoRecv] internal static int32 startLine(this ref Func f) {
    var fn = f.raw();
    if (fn.isInlined()) {
        // inlined version
        var fi = (ж<funcinl>)(uintptr)(new @unsafe.Pointer(fn));
        return (~fi).startLine;
    }
    return fn.funcInfo().startLine;
}

// findmoduledatap looks up the moduledata for a PC.
//
// It is nosplit because it's part of the isgoexception
// implementation.
//
//go:nosplit
internal static ж<moduledata> findmoduledatap(uintptr pc) {
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        if ((~datap).minpc <= pc && pc < (~datap).maxpc) {
            return datap;
        }
    }
    return default!;
}

[GoType] partial struct ΔfuncInfo {
    public partial ref ж<_func> _func { get; }
    internal ж<moduledata> datap;
}

internal static bool valid(this ΔfuncInfo f) {
    return f._func != nil;
}

internal static ж<Func> _Func(this ΔfuncInfo f) {
    return (ж<Func>)(uintptr)(new @unsafe.Pointer(f._func));
}

// isInlined reports whether f should be re-interpreted as a *funcinl.
[GoRecv] internal static bool isInlined(this ref _func f) {
    return f.entryOff == ~((uint32)0);
}

// see comment for funcinl.ones

// entry returns the entry PC for f.
//
// entry should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
internal static uintptr entry(this ΔfuncInfo f) {
    return f.datap.textAddr(f.entryOff);
}

//go:linkname badFuncInfoEntry runtime.funcInfo.entry
internal static partial uintptr badFuncInfoEntry(ΔfuncInfo _);

// findfunc looks up function metadata for a PC.
//
// It is nosplit because it's part of the isgoexception
// implementation.
//
// findfunc should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:nosplit
//go:linkname findfunc
internal static ΔfuncInfo findfunc(uintptr pc) {
    var datap = findmoduledatap(pc);
    if (datap == nil) {
        return new ΔfuncInfo(nil);
    }
    const uintptr nsub = /* uintptr(len(findfuncbucket{}.subbuckets)) */ 16;
    var (pcOff, ok) = datap.textOff(pc);
    if (!ok) {
        return new ΔfuncInfo(nil);
    }
    var x = ((uintptr)pcOff) + (~datap).text - (~datap).minpc;
    // TODO: are datap.text and datap.minpc always equal?
    var b = x / abi.FuncTabBucketSize;
    var i = x % abi.FuncTabBucketSize / (abi.FuncTabBucketSize / nsub);
    var ffb = (ж<findfuncbucket>)(uintptr)(add(((@unsafe.Pointer)(~datap).findfunctab), b * @unsafe.Sizeof(new findfuncbucket(nil))));
    var idx = (~ffb).idx + ((uint32)(~ffb).subbuckets[i]);
    // Find the ftab entry.
    while ((~datap).ftab[idx + 1].entryoff <= pcOff) {
        idx++;
    }
    var funcoff = (~datap).ftab[idx].funcoff;
    return new ΔfuncInfo((ж<_func>)(uintptr)(new @unsafe.Pointer(Ꮡ((~datap).pclntable, funcoff))), datap);
}

// A srcFunc represents a logical function in the source code. This may
// correspond to an actual symbol in the binary text, or it may correspond to a
// source function that has been inlined.
[GoType] partial struct ΔsrcFunc {
    internal ж<moduledata> datap;
    internal int32 nameOff;
    internal int32 startLine;
    internal @internal.abi_package.FuncID funcID;
}

internal static ΔsrcFunc srcFunc(this ΔfuncInfo f) {
    if (!f.valid()) {
        return new ΔsrcFunc(nil);
    }
    return new ΔsrcFunc(f.datap, f.nameOff, f.startLine, f.funcID);
}

// name should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
internal static @string name(this ΔsrcFunc s) {
    if (s.datap == nil) {
        return ""u8;
    }
    return s.datap.funcName(s.nameOff);
}

//go:linkname badSrcFuncName runtime.srcFunc.name
internal static partial @string badSrcFuncName(ΔsrcFunc _);

[GoType] partial struct pcvalueCache {
    internal array<array<pcvalueCacheEnt>> entries = new(2);
    internal nint inUse;
}

[GoType] partial struct pcvalueCacheEnt {
    // targetpc and off together are the key of this cache entry.
    internal uintptr targetpc;
    internal uint32 off;
    internal int32 val;   // The value of this entry.
    internal uintptr valPC; // The PC at which val starts
}

// pcvalueCacheKey returns the outermost index in a pcvalueCache to use for targetpc.
// It must be very cheap to calculate.
// For now, align to goarch.PtrSize and reduce mod the number of entries.
// In practice, this appears to be fairly randomly and evenly distributed.
internal static uintptr pcvalueCacheKey(uintptr targetpc) {
    return (targetpc / goarch.PtrSize) % ((uintptr)len(new pcvalueCache(nil).entries));
}

// Returns the PCData value, and the PC where this value starts.
internal static (int32, uintptr) pcvalue(ΔfuncInfo f, uint32 off, uintptr targetpc, bool strict) {
    // If true, when we get a cache hit, still look up the data and make sure it
    // matches the cached contents.
    const bool debugCheckCache = false;
    if (off == 0) {
        return (-1, 0);
    }
    // Check the cache. This speeds up walks of deep stacks, which
    // tend to have the same recursive functions over and over,
    // or repetitive stacks between goroutines.
    int32 checkVal = default!;
    uintptr checkPC = default!;
    var ck = pcvalueCacheKey(targetpc);
    {
        var mp = acquirem();
        var cache = Ꮡ((~mp).pcvalueCache);
        // The cache can be used by the signal handler on this M. Avoid
        // re-entrant use of the cache. The signal handler can also write inUse,
        // but will always restore its value, so we can use a regular increment
        // even if we get signaled in the middle of it.
        (~cache).inUse++;
        if ((~cache).inUse == 1){
            foreach (var (i, _) in (~cache).entries[ck]) {
                // We check off first because we're more
                // likely to have multiple entries with
                // different offsets for the same targetpc
                // than the other way around, so we'll usually
                // fail in the first clause.
                var ent = Ꮡ(~cache).entries[ck].at<pcvalueCacheEnt>(i);
                if ((~ent).off == off && (~ent).targetpc == targetpc) {
                    var (valΔ1, pcΔ1) = (ent.val.val, ent.val.valPC);
                    if (debugCheckCache){
                        (checkVal, checkPC) = (ent.val.val, ent.val.valPC);
                        break;
                    } else {
                        (~cache).inUse--;
                        releasem(mp);
                        return (valΔ1, pcΔ1);
                    }
                }
            }
        } else 
        if (debugCheckCache && ((~cache).inUse < 1 || (~cache).inUse > 2)) {
            // Catch accounting errors or deeply reentrant use. In principle
            // "inUse" should never exceed 2.
            @throw("cache.inUse out of range"u8);
        }
        (~cache).inUse--;
        releasem(mp);
    }
    if (!f.valid()) {
        if (strict && panicking.Load() == 0) {
            println("runtime: no module data for", ((Δhex)f.entry()));
            @throw("no module data"u8);
        }
        return (-1, 0);
    }
    var datap = f.datap;
    var Δp = (~datap).pctab[(int)(off)..];
    ref var pc = ref heap<uintptr>(out var Ꮡpc);
    pc = f.entry();
    var prevpc = pc;
    ref var val = ref heap<int32>(out var Ꮡval);
    val = ((int32)(-1));
    while (ᐧ) {
        bool okΔ1 = default!;
        (Δp, ) = step(Δp, Ꮡpc, Ꮡval, pc == f.entry());
        if (!okΔ1) {
            break;
        }
        if (targetpc < pc) {
            // Replace a random entry in the cache. Random
            // replacement prevents a performance cliff if
            // a recursive stack's cycle is slightly
            // larger than the cache.
            // Put the new element at the beginning,
            // since it is the most likely to be newly used.
            if (debugCheckCache && checkPC != 0){
                if (checkVal != val || checkPC != prevpc) {
                    print("runtime: table value ", val, "@", prevpc, " != cache value ", checkVal, "@", checkPC, " at PC ", targetpc, " off ", off, "\n");
                    @throw("bad pcvalue cache"u8);
                }
            } else {
                var mp = acquirem();
                var cache = Ꮡ((~mp).pcvalueCache);
                (~cache).inUse++;
                if ((~cache).inUse == 1) {
                    var e = Ꮡ(~cache).entries.at<array<pcvalueCacheEnt>>(ck);
                    var ci = cheaprandn(((uint32)len((~cache).entries[ck])));
                    e.val[ci] = e.val[0];
                    e.val[0] = new pcvalueCacheEnt(
                        targetpc: targetpc,
                        off: off,
                        val: val,
                        valPC: prevpc
                    );
                }
                (~cache).inUse--;
                releasem(mp);
            }
            return (val, prevpc);
        }
        prevpc = pc;
    }
    // If there was a table, it should have covered all program counters.
    // If not, something is wrong.
    if (panicking.Load() != 0 || !strict) {
        return (-1, 0);
    }
    print("runtime: invalid pc-encoded table f=", funcname(f), " pc=", ((Δhex)pc), " targetpc=", ((Δhex)targetpc), " tab=", Δp, "\n");
    Δp = (~datap).pctab[(int)(off)..];
    pc = f.entry();
    val = -1;
    while (ᐧ) {
        bool ok = default!;
        (Δp, ok) = step(Δp, Ꮡpc, Ꮡval, pc == f.entry());
        if (!ok) {
            break;
        }
        print("\tvalue=", val, " until pc=", ((Δhex)pc), "\n");
    }
    @throw("invalid runtime symbol table"u8);
    return (-1, 0);
}

internal static @string funcname(ΔfuncInfo f) {
    if (!f.valid()) {
        return ""u8;
    }
    return f.datap.funcName(f.nameOff);
}

internal static @string funcpkgpath(ΔfuncInfo f) {
    @string name = funcNameForPrint(funcname(f));
    nint i = len(name) - 1;
    for (; i > 0; i--) {
        if (name[i] == (rune)'/') {
            break;
        }
    }
    for (; i < len(name); i++) {
        if (name[i] == (rune)'.') {
            break;
        }
    }
    return name[..(int)(i)];
}

internal static @string funcfile(ΔfuncInfo f, int32 fileno) {
    var datap = f.datap;
    if (!f.valid()) {
        return "?"u8;
    }
    // Make sure the cu index and file offset are valid
    {
        ref var fileoff = ref heap<uint32>(out var Ꮡfileoff);
        fileoff = (~datap).cutab[f.cuOffset + ((uint32)fileno)]; if (fileoff != ~((uint32)0)) {
            return gostringnocopy(Ꮡ((~datap).filetab, fileoff));
        }
    }
    // pcln section is corrupt.
    return "?"u8;
}

// funcline1 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/phuslu/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname funcline1
internal static (@string file, int32 line) funcline1(ΔfuncInfo f, uintptr targetpc, bool strict) {
    @string file = default!;
    int32 line = default!;

    var datap = f.datap;
    if (!f.valid()) {
        return ("?", 0);
    }
    var (fileno, _) = pcvalue(f, f.pcfile, targetpc, strict);
    (line, _) = pcvalue(f, f.pcln, targetpc, strict);
    if (fileno == -1 || line == -1 || ((nint)fileno) >= len((~datap).filetab)) {
        // print("looking for ", hex(targetpc), " in ", funcname(f), " got file=", fileno, " line=", lineno, "\n")
        return ("?", 0);
    }
    file = funcfile(f, fileno);
    return (file, line);
}

internal static (@string file, int32 line) funcline(ΔfuncInfo f, uintptr targetpc) {
    @string file = default!;
    int32 line = default!;

    return funcline1(f, targetpc, true);
}

internal static int32 funcspdelta(ΔfuncInfo f, uintptr targetpc) {
    var (x, _) = pcvalue(f, f.pcsp, targetpc, true);
    if (debugPcln && (int32)(x & (goarch.PtrSize - 1)) != 0) {
        print("invalid spdelta ", funcname(f), " ", ((Δhex)f.entry()), " ", ((Δhex)targetpc), " ", ((Δhex)f.pcsp), " ", x, "\n");
        @throw("bad spdelta"u8);
    }
    return x;
}

// funcMaxSPDelta returns the maximum spdelta at any point in f.
internal static int32 funcMaxSPDelta(ΔfuncInfo f) {
    var datap = f.datap;
    var Δp = (~datap).pctab[(int)(f.pcsp)..];
    ref var pc = ref heap<uintptr>(out var Ꮡpc);
    pc = f.entry();
    ref var val = ref heap<int32>(out var Ꮡval);
    val = ((int32)(-1));
    var most = ((int32)0);
    while (ᐧ) {
        bool ok = default!;
        (Δp, ok) = step(Δp, Ꮡpc, Ꮡval, pc == f.entry());
        if (!ok) {
            return most;
        }
        most = max(most, val);
    }
}

internal static uint32 pcdatastart(ΔfuncInfo f, uint32 table) {
    return ~(ж<uint32>)(uintptr)(add(new @unsafe.Pointer(Ꮡf.of(funcInfo.Ꮡnfuncdata)), @unsafe.Sizeof(f.nfuncdata) + ((uintptr)table) * 4));
}

internal static int32 pcdatavalue(ΔfuncInfo f, uint32 table, uintptr targetpc) {
    if (table >= f.npcdata) {
        return -1;
    }
    var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, true);
    return r;
}

internal static int32 pcdatavalue1(ΔfuncInfo f, uint32 table, uintptr targetpc, bool strict) {
    if (table >= f.npcdata) {
        return -1;
    }
    var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, strict);
    return r;
}

// Like pcdatavalue, but also return the start PC of this PCData value.
//
// pcdatavalue2 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname pcdatavalue2
internal static (int32, uintptr) pcdatavalue2(ΔfuncInfo f, uint32 table, uintptr targetpc) {
    if (table >= f.npcdata) {
        return (-1, 0);
    }
    return pcvalue(f, pcdatastart(f, table), targetpc, true);
}

// funcdata returns a pointer to the ith funcdata for f.
// funcdata should be kept in sync with cmd/link:writeFuncs.
internal static @unsafe.Pointer funcdata(ΔfuncInfo f, uint8 i) {
    if (i < 0 || i >= f.nfuncdata) {
        return default!;
    }
    var @base = f.datap.gofunc;
    // load gofunc address early so that we calculate during cache misses
    var Δp = ((uintptr)new @unsafe.Pointer(Ꮡf.of(funcInfo.Ꮡnfuncdata))) + @unsafe.Sizeof(f.nfuncdata) + ((uintptr)f.npcdata) * 4 + ((uintptr)i) * 4;
    var off = ~(ж<uint32>)(uintptr)(((@unsafe.Pointer)Δp));
    // Return off == ^uint32(0) ? 0 : f.datap.gofunc + uintptr(off), but without branches.
    // The compiler calculates mask on most architectures using conditional assignment.
    uintptr mask = default!;
    if (off == ~((uint32)0)) {
        mask = 1;
    }
    mask--;
    var raw = @base + ((uintptr)off);
    return ((@unsafe.Pointer)((uintptr)(raw & mask)));
}

// step advances to the next pc, value pair in the encoded table.
//
// step should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname step
internal static (slice<byte> newp, bool ok) step(slice<byte> Δp, ж<uintptr> Ꮡpc, ж<int32> Ꮡval, bool first) {
    slice<byte> newp = default!;
    bool ok = default!;

    ref var pc = ref Ꮡpc.val;
    ref var val = ref Ꮡval.val;
    // For both uvdelta and pcdelta, the common case (~70%)
    // is that they are a single byte. If so, avoid calling readvarint.
    var uvdelta = ((uint32)Δp[0]);
    if (uvdelta == 0 && !first) {
        return (default!, false);
    }
    var n = ((uint32)1);
    if ((uint32)(uvdelta & 128) != 0) {
        (n, uvdelta) = readvarint(Δp);
    }
    val += ((int32)((uint32)(-((uint32)(uvdelta & 1)) ^ (uvdelta >> (int)(1)))));
    Δp = Δp[(int)(n)..];
    var pcdelta = ((uint32)Δp[0]);
    n = 1;
    if ((uint32)(pcdelta & 128) != 0) {
        (n, pcdelta) = readvarint(Δp);
    }
    Δp = Δp[(int)(n)..];
    pc += ((uintptr)(pcdelta * sys.PCQuantum));
    return (Δp, true);
}

// readvarint reads a varint from p.
internal static (uint32 read, uint32 val) readvarint(slice<byte> Δp) {
    uint32 read = default!;
    uint32 val = default!;

    uint32 v = default!;
    uint32 shift = default!;
    uint32 n = default!;
    while (ᐧ) {
        var b = Δp[n];
        n++;
        v |= (uint32)(((uint32)((byte)(b & 127))) << (int)(((uint32)(shift & 31))));
        if ((byte)(b & 128) == 0) {
            break;
        }
        shift += 7;
    }
    return (n, v);
}

[GoType] partial struct stackmap {
    internal int32 n;   // number of bitmaps
    internal int32 nbit;   // number of bits in each bitmap
    internal array<byte> bytedata = new(1); // bitmaps, each starting on a byte boundary
}

// stackmapdata should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname stackmapdata
//go:nowritebarrier
internal static bitvector stackmapdata(ж<stackmap> Ꮡstkmap, int32 n) {
    ref var stkmap = ref Ꮡstkmap.val;

    // Check this invariant only when stackDebug is on at all.
    // The invariant is already checked by many of stackmapdata's callers,
    // and disabling it by default allows stackmapdata to be inlined.
    if (stackDebug > 0 && (n < 0 || n >= stkmap.n)) {
        @throw("stackmapdata: index out of range"u8);
    }
    return new bitvector(stkmap.nbit, addb(Ꮡstkmap.bytedata.at<byte>(0), ((uintptr)(n * ((stkmap.nbit + 7) >> (int)(3))))));
}

} // end runtime_package
