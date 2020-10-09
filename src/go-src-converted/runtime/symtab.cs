// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:50 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\symtab.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Frames may be used to get function/file/line information for a
        // slice of PC values returned by Callers.
        public partial struct Frames
        {
            public slice<System.UIntPtr> callers; // frames is a slice of Frames that have yet to be returned.
            public slice<Frame> frames;
            public array<Frame> frameStore;
        }

        // Frame is the information returned by Frames for each call frame.
        public partial struct Frame
        {
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
            public long Line; // Entry point program counter for the function; may be zero
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
        public static ptr<Frames> CallersFrames(slice<System.UIntPtr> callers)
        {
            ptr<Frames> f = addr(new Frames(callers:callers));
            f.frames = f.frameStore[..0L];
            return _addr_f!;
        }

        // Next returns frame information for the next caller.
        // If more is false, there are no more callers (the Frame value is valid).
        private static (Frame, bool) Next(this ptr<Frames> _addr_ci)
        {
            Frame frame = default;
            bool more = default;
            ref Frames ci = ref _addr_ci.val;

            while (len(ci.frames) < 2L)
            { 
                // Find the next frame.
                // We need to look for 2 frames so we know what
                // to return for the "more" result.
                if (len(ci.callers) == 0L)
                {
                    break;
                }

                var pc = ci.callers[0L];
                ci.callers = ci.callers[1L..];
                var funcInfo = findfunc(pc);
                if (!funcInfo.valid())
                {
                    if (cgoSymbolizer != null)
                    { 
                        // Pre-expand cgo frames. We could do this
                        // incrementally, too, but there's no way to
                        // avoid allocation in this case anyway.
                        ci.frames = append(ci.frames, expandCgoFrames(pc));

                    }

                    continue;

                }

                var f = funcInfo._Func();
                var entry = f.Entry();
                if (pc > entry)
                { 
                    // We store the pc of the start of the instruction following
                    // the instruction in question (the call or the inline mark).
                    // This is done for historical reasons, and to make FuncForPC
                    // work correctly for entries in the result of runtime.Callers.
                    pc--;

                }

                var name = funcname(funcInfo);
                {
                    var inldata = funcdata(funcInfo, _FUNCDATA_InlTree);

                    if (inldata != null)
                    {
                        ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                        var ix = pcdatavalue(funcInfo, _PCDATA_InlTreeIndex, pc, _addr_null);
                        if (ix >= 0L)
                        { 
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
 

            // Pop one frame from the frame list. Keep the rest.
            // Avoid allocation in the common case, which is 1 or 2 frames.
            switch (len(ci.frames))
            {
                case 0L: // In the rare case when there are no frames at all, we return Frame{}.
                    return ;
                    break;
                case 1L: 
                    frame = ci.frames[0L];
                    ci.frames = ci.frameStore[..0L];
                    break;
                case 2L: 
                    frame = ci.frames[0L];
                    ci.frameStore[0L] = ci.frames[1L];
                    ci.frames = ci.frameStore[..1L];
                    break;
                default: 
                    frame = ci.frames[0L];
                    ci.frames = ci.frames[1L..];
                    break;
            }
            more = len(ci.frames) > 0L;
            if (frame.funcInfo.valid())
            { 
                // Compute file/line just before we need to return it,
                // as it can be expensive. This avoids computing file/line
                // for the Frame we find but don't return. See issue 32093.
                var (file, line) = funcline1(frame.funcInfo, frame.PC, false);
                frame.File = file;
                frame.Line = int(line);

            }

            return ;

        }

        // runtime_expandFinalInlineFrame expands the final pc in stk to include all
        // "callers" if pc is inline.
        //
        //go:linkname runtime_expandFinalInlineFrame runtime/pprof.runtime_expandFinalInlineFrame
        private static slice<System.UIntPtr> runtime_expandFinalInlineFrame(slice<System.UIntPtr> stk)
        {
            if (len(stk) == 0L)
            {
                return stk;
            }

            var pc = stk[len(stk) - 1L];
            var tracepc = pc - 1L;

            var f = findfunc(tracepc);
            if (!f.valid())
            { 
                // Not a Go function.
                return stk;

            }

            var inldata = funcdata(f, _FUNCDATA_InlTree);
            if (inldata == null)
            { 
                // Nothing inline in f.
                return stk;

            } 

            // Treat the previous func as normal. We haven't actually checked, but
            // since this pc was included in the stack, we know it shouldn't be
            // elided.
            var lastFuncID = funcID_normal; 

            // Remove pc from stk; we'll re-add it below.
            stk = stk[..len(stk) - 1L]; 

            // See inline expansion in gentraceback.
            ref pcvalueCache cache = ref heap(out ptr<pcvalueCache> _addr_cache);
            ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
            while (true)
            {
                var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, _addr_cache);
                if (ix < 0L)
                {
                    break;
                }

                if (inltree[ix].funcID == funcID_wrapper && elideWrapperCalling(lastFuncID))
                { 
                    // ignore wrappers
                }
                else
                {
                    stk = append(stk, pc);
                }

                lastFuncID = inltree[ix].funcID; 
                // Back up to an instruction in the "caller".
                tracepc = f.entry + uintptr(inltree[ix].parentPc);
                pc = tracepc + 1L;

            } 

            // N.B. we want to keep the last parentPC which is not inline.
 

            // N.B. we want to keep the last parentPC which is not inline.
            stk = append(stk, pc);

            return stk;

        }

        // expandCgoFrames expands frame information for pc, known to be
        // a non-Go function, using the cgoSymbolizer hook. expandCgoFrames
        // returns nil if pc could not be expanded.
        private static slice<Frame> expandCgoFrames(System.UIntPtr pc)
        {
            ref cgoSymbolizerArg arg = ref heap(new cgoSymbolizerArg(pc:pc), out ptr<cgoSymbolizerArg> _addr_arg);
            callCgoSymbolizer(_addr_arg);

            if (arg.file == null && arg.funcName == null)
            { 
                // No useful information from symbolizer.
                return null;

            }

            slice<Frame> frames = default;
            while (true)
            {
                frames = append(frames, new Frame(PC:pc,Func:nil,Function:gostring(arg.funcName),File:gostring(arg.file),Line:int(arg.lineno),Entry:arg.entry,));
                if (arg.more == 0L)
                {
                    break;
                }

                callCgoSymbolizer(_addr_arg);

            } 

            // No more frames for this PC. Tell the symbolizer we are done.
            // We don't try to maintain a single cgoSymbolizerArg for the
            // whole use of Frames, because there would be no good way to tell
            // the symbolizer when we are done.
 

            // No more frames for this PC. Tell the symbolizer we are done.
            // We don't try to maintain a single cgoSymbolizerArg for the
            // whole use of Frames, because there would be no good way to tell
            // the symbolizer when we are done.
            arg.pc = 0L;
            callCgoSymbolizer(_addr_arg);

            return frames;

        }

        // NOTE: Func does not expose the actual unexported fields, because we return *Func
        // values to users, and we want to keep them from being able to overwrite the data
        // with (say) *f = Func{}.
        // All code operating on a *Func must call raw() to get the *_func
        // or funcInfo() to get the funcInfo instead.

        // A Func represents a Go function in the running binary.
        public partial struct Func
        {
        }

        private static ptr<_func> raw(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            return _addr_(_func.val)(@unsafe.Pointer(f))!;
        }

        private static funcInfo funcInfo(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var fn = f.raw();
            return new funcInfo(fn,findmoduledatap(fn.entry));
        }

        // PCDATA and FUNCDATA table indexes.
        //
        // See funcdata.h and ../cmd/internal/objabi/funcdata.go.
        private static readonly long _PCDATA_RegMapIndex = (long)0L; // if !go115ReduceLiveness
        private static readonly long _PCDATA_UnsafePoint = (long)0L; // if go115ReduceLiveness
        private static readonly long _PCDATA_StackMapIndex = (long)1L;
        private static readonly long _PCDATA_InlTreeIndex = (long)2L;

        private static readonly long _FUNCDATA_ArgsPointerMaps = (long)0L;
        private static readonly long _FUNCDATA_LocalsPointerMaps = (long)1L;
        private static readonly long _FUNCDATA_RegPointerMaps = (long)2L; // if !go115ReduceLiveness
        private static readonly long _FUNCDATA_StackObjects = (long)3L;
        private static readonly long _FUNCDATA_InlTree = (long)4L;
        private static readonly long _FUNCDATA_OpenCodedDeferInfo = (long)5L;

        private static readonly ulong _ArgsSizeUnknown = (ulong)-0x80000000UL;


 
        // PCDATA_UnsafePoint values.
        private static readonly long _PCDATA_UnsafePointSafe = (long)-1L; // Safe for async preemption
        private static readonly long _PCDATA_UnsafePointUnsafe = (long)-2L; // Unsafe for async preemption

        // _PCDATA_Restart1(2) apply on a sequence of instructions, within
        // which if an async preemption happens, we should back off the PC
        // to the start of the sequence when resume.
        // We need two so we can distinguish the start/end of the sequence
        // in case that two sequences are next to each other.
        private static readonly long _PCDATA_Restart1 = (long)-3L;
        private static readonly long _PCDATA_Restart2 = (long)-4L; 

        // Like _PCDATA_RestartAtEntry, but back to function entry if async
        // preempted.
        private static readonly long _PCDATA_RestartAtEntry = (long)-5L;


        // A FuncID identifies particular functions that need to be treated
        // specially by the runtime.
        // Note that in some situations involving plugins, there may be multiple
        // copies of a particular special runtime function.
        // Note: this list must match the list in cmd/internal/objabi/funcid.go.
        private partial struct funcID // : byte
        {
        }

        private static readonly funcID funcID_normal = (funcID)iota; // not a special function
        private static readonly var funcID_runtime_main = 0;
        private static readonly var funcID_goexit = 1;
        private static readonly var funcID_jmpdefer = 2;
        private static readonly var funcID_mcall = 3;
        private static readonly var funcID_morestack = 4;
        private static readonly var funcID_mstart = 5;
        private static readonly var funcID_rt0_go = 6;
        private static readonly var funcID_asmcgocall = 7;
        private static readonly var funcID_sigpanic = 8;
        private static readonly var funcID_runfinq = 9;
        private static readonly var funcID_gcBgMarkWorker = 10;
        private static readonly var funcID_systemstack_switch = 11;
        private static readonly var funcID_systemstack = 12;
        private static readonly var funcID_cgocallback_gofunc = 13;
        private static readonly var funcID_gogo = 14;
        private static readonly var funcID_externalthreadhandler = 15;
        private static readonly var funcID_debugCallV1 = 16;
        private static readonly var funcID_gopanic = 17;
        private static readonly var funcID_panicwrap = 18;
        private static readonly var funcID_handleAsyncEvent = 19;
        private static readonly var funcID_asyncPreempt = 20;
        private static readonly var funcID_wrapper = 21; // any autogenerated code (hash/eq algorithms, method wrappers, etc.)

        // moduledata records information about the layout of the executable
        // image. It is written by the linker. Any changes here must be
        // matched changes to the code in cmd/internal/ld/symtab.go:symtab.
        // moduledata is stored in statically allocated non-pointer memory;
        // none of the pointers here are visible to the garbage collector.
        private partial struct moduledata
        {
            public slice<byte> pclntable;
            public slice<functab> ftab;
            public slice<uint> filetab;
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
        private partial struct modulehash
        {
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
        private static slice<ptr<moduledata>> activeModules()
        {
            ptr<slice<ptr<moduledata>>> p = new ptr<ptr<slice<ptr<moduledata>>>>(atomic.Loadp(@unsafe.Pointer(_addr_modulesSlice)));
            if (p == null)
            {
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
        private static void modulesinit()
        {
            ptr<var> modules = @new<*moduledata>();
            {
                var md__prev1 = md;

                var md = _addr_firstmoduledata;

                while (md != null)
                {
                    if (md.bad)
                    {
                        continue;
                    md = md.next;
                    }

                    modules.val = append(modules.val, md);
                    if (md.gcdatamask == (new bitvector()))
                    {
                        md.gcdatamask = progToPointerMask((byte.val)(@unsafe.Pointer(md.gcdata)), md.edata - md.data);
                        md.gcbssmask = progToPointerMask((byte.val)(@unsafe.Pointer(md.gcbss)), md.ebss - md.bss);
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

                foreach (var (__i, __md) in modules.val)
                {
                    i = __i;
                    md = __md;
                    if (md.hasmain != 0L)
                    {
                        (modules.val)[0L] = md;
                        (modules.val)[i] = _addr_firstmoduledata;
                        break;
                    }

                }

                md = md__prev1;
            }

            atomicstorep(@unsafe.Pointer(_addr_modulesSlice), @unsafe.Pointer(modules));

        }

        private partial struct functab
        {
            public System.UIntPtr entry;
            public System.UIntPtr funcoff;
        }

        // Mapping information for secondary text sections

        private partial struct textsect
        {
            public System.UIntPtr vaddr; // prelinked section vaddr
            public System.UIntPtr length; // section length
            public System.UIntPtr baseaddr; // relocated section address
        }

        private static readonly long minfunc = (long)16L; // minimum function size
 // minimum function size
        private static readonly long pcbucketsize = (long)256L * minfunc; // size of bucket in the pc->func lookup table

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
        private partial struct findfuncbucket
        {
            public uint idx;
            public array<byte> subbuckets;
        }

        private static void moduledataverify()
        {
            {
                var datap = _addr_firstmoduledata;

                while (datap != null)
                {
                    moduledataverify1(_addr_datap);
                    datap = datap.next;
                }

            }

        }

        private static readonly var debugPcln = false;



        private static void moduledataverify1(ptr<moduledata> _addr_datap)
        {
            ref moduledata datap = ref _addr_datap.val;
 
            // See golang.org/s/go12symtab for header: 0xfffffffb,
            // two zero bytes, a byte giving the PC quantum,
            // and a byte giving the pointer width in bytes.
            ptr<ptr<ptr<array<byte>>>> pcln = new ptr<ptr<ptr<ptr<array<byte>>>>>(@unsafe.Pointer(_addr_datap.pclntable));
            ptr<ptr<ptr<array<uint>>>> pcln32 = new ptr<ptr<ptr<ptr<array<uint>>>>>(@unsafe.Pointer(_addr_datap.pclntable));
            if (pcln32[0L] != 0xfffffffbUL || pcln[4L] != 0L || pcln[5L] != 0L || pcln[6L] != sys.PCQuantum || pcln[7L] != sys.PtrSize)
            {
                println("runtime: function symbol table header:", hex(pcln32[0L]), hex(pcln[4L]), hex(pcln[5L]), hex(pcln[6L]), hex(pcln[7L]));
                throw("invalid function symbol table\n");
            } 

            // ftab is lookup table for function by program counter.
            var nftab = len(datap.ftab) - 1L;
            for (long i = 0L; i < nftab; i++)
            { 
                // NOTE: ftab[nftab].entry is legal; it is the address beyond the final function.
                if (datap.ftab[i].entry > datap.ftab[i + 1L].entry)
                {
                    funcInfo f1 = new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[i].funcoff])),datap);
                    funcInfo f2 = new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[i+1].funcoff])),datap);
                    @string f2name = "end";
                    if (i + 1L < nftab)
                    {
                        f2name = funcname(f2);
                    }

                    println("function symbol table not sorted by program counter:", hex(datap.ftab[i].entry), funcname(f1), ">", hex(datap.ftab[i + 1L].entry), f2name);
                    for (long j = 0L; j <= i; j++)
                    {
                        print("\t", hex(datap.ftab[j].entry), " ", funcname(new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[datap.ftab[j].funcoff])),datap)), "\n");
                    }

                    if (GOOS == "aix" && isarchive)
                    {
                        println("-Wl,-bnoobjreorder is mandatory on aix/ppc64 with c-archive");
                    }

                    throw("invalid runtime symbol table");

                }

            }


            if (datap.minpc != datap.ftab[0L].entry || datap.maxpc != datap.ftab[nftab].entry)
            {
                throw("minpc or maxpc invalid");
            }

            foreach (var (_, modulehash) in datap.modulehashes)
            {
                if (modulehash.linktimehash != modulehash.runtimehash.val)
                {
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
        public static ptr<Func> FuncForPC(System.UIntPtr pc)
        {
            var f = findfunc(pc);
            if (!f.valid())
            {
                return _addr_null!;
            }

            {
                var inldata = funcdata(f, _FUNCDATA_InlTree);

                if (inldata != null)
                { 
                    // Note: strict=false so bad PCs (those between functions) don't crash the runtime.
                    // We just report the preceding function in that situation. See issue 29735.
                    // TODO: Perhaps we should report no function at all in that case.
                    // The runtime currently doesn't have function end info, alas.
                    {
                        var ix = pcdatavalue1(f, _PCDATA_InlTreeIndex, pc, _addr_null, false);

                        if (ix >= 0L)
                        {
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
        private static @string Name(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            if (f == null)
            {
                return "";
            }

            var fn = f.raw();
            if (fn.entry == 0L)
            { // inlined version
                var fi = (funcinl.val)(@unsafe.Pointer(fn));
                return fi.name;

            }

            return funcname(f.funcInfo());

        }

        // Entry returns the entry address of the function.
        private static System.UIntPtr Entry(this ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            var fn = f.raw();
            if (fn.entry == 0L)
            { // inlined version
                var fi = (funcinl.val)(@unsafe.Pointer(fn));
                return fi.entry;

            }

            return fn.entry;

        }

        // FileLine returns the file name and line number of the
        // source code corresponding to the program counter pc.
        // The result will not be accurate if pc is not a program
        // counter within f.
        private static (@string, long) FileLine(this ptr<Func> _addr_f, System.UIntPtr pc)
        {
            @string file = default;
            long line = default;
            ref Func f = ref _addr_f.val;

            var fn = f.raw();
            if (fn.entry == 0L)
            { // inlined version
                var fi = (funcinl.val)(@unsafe.Pointer(fn));
                return (fi.file, fi.line);

            } 
            // Pass strict=false here, because anyone can call this function,
            // and they might just be wrong about targetpc belonging to f.
            var (file, line32) = funcline1(f.funcInfo(), pc, false);
            return (file, int(line32));

        }

        private static ptr<moduledata> findmoduledatap(System.UIntPtr pc)
        {
            {
                var datap = _addr_firstmoduledata;

                while (datap != null)
                {
                    if (datap.minpc <= pc && pc < datap.maxpc)
                    {
                        return _addr_datap!;
                    datap = datap.next;
                    }

                }

            }
            return _addr_null!;

        }

        private partial struct funcInfo
        {
            public ref ptr<_func> ptr<_func> => ref ptr<_func>_ptr;
            public ptr<moduledata> datap;
        }

        private static bool valid(this funcInfo f)
        {
            return f._func != null;
        }

        private static ptr<Func> _Func(this funcInfo f)
        {
            return _addr_(Func.val)(@unsafe.Pointer(f._func))!;
        }

        private static funcInfo findfunc(System.UIntPtr pc)
        {
            var datap = findmoduledatap(pc);
            if (datap == null)
            {
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

            if (idx >= uint32(len(datap.ftab)))
            {
                idx = uint32(len(datap.ftab) - 1L);
            }

            if (pc < datap.ftab[idx].entry)
            { 
                // With multiple text sections, the idx might reference a function address that
                // is higher than the pc being searched, so search backward until the matching address is found.

                while (datap.ftab[idx].entry > pc && idx > 0L)
                {
                    idx--;
                }
            else

                if (idx == 0L)
                {
                    throw("findfunc: bad findfunctab entry idx");
                }

            }            { 
                // linear search to find func with pc >= entry.
                while (datap.ftab[idx + 1L].entry <= pc)
                {
                    idx++;
                }


            }

            var funcoff = datap.ftab[idx].funcoff;
            if (funcoff == ~uintptr(0L))
            { 
                // With multiple text sections, there may be functions inserted by the external
                // linker that are not known by Go. This means there may be holes in the PC
                // range covered by the func table. The invalid funcoff value indicates a hole.
                // See also cmd/link/internal/ld/pcln.go:pclntab
                return new funcInfo();

            }

            return new funcInfo((*_func)(unsafe.Pointer(&datap.pclntable[funcoff])),datap);

        }

        private partial struct pcvalueCache
        {
            public array<array<pcvalueCacheEnt>> entries;
        }

        private partial struct pcvalueCacheEnt
        {
            public System.UIntPtr targetpc;
            public int off; // val is the value of this cached pcvalue entry.
            public int val;
        }

        // pcvalueCacheKey returns the outermost index in a pcvalueCache to use for targetpc.
        // It must be very cheap to calculate.
        // For now, align to sys.PtrSize and reduce mod the number of entries.
        // In practice, this appears to be fairly randomly and evenly distributed.
        private static System.UIntPtr pcvalueCacheKey(System.UIntPtr targetpc)
        {
            return (targetpc / sys.PtrSize) % uintptr(len(new pcvalueCache().entries));
        }

        // Returns the PCData value, and the PC where this value starts.
        // TODO: the start PC is returned only when cache is nil.
        private static (int, System.UIntPtr) pcvalue(funcInfo f, int off, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache, bool strict)
        {
            int _p0 = default;
            System.UIntPtr _p0 = default;
            ref pcvalueCache cache = ref _addr_cache.val;

            if (off == 0L)
            {
                return (-1L, 0L);
            } 

            // Check the cache. This speeds up walks of deep stacks, which
            // tend to have the same recursive functions over and over.
            //
            // This cache is small enough that full associativity is
            // cheaper than doing the hashing for a less associative
            // cache.
            if (cache != null)
            {
                var x = pcvalueCacheKey(targetpc);
                foreach (var (i) in cache.entries[x])
                { 
                    // We check off first because we're more
                    // likely to have multiple entries with
                    // different offsets for the same targetpc
                    // than the other way around, so we'll usually
                    // fail in the first clause.
                    var ent = _addr_cache.entries[x][i];
                    if (ent.off == off && ent.targetpc == targetpc)
                    {
                        return (ent.val, 0L);
                    }

                }

            }

            if (!f.valid())
            {
                if (strict && panicking == 0L)
                {
                    print("runtime: no module data for ", hex(f.entry), "\n");
                    throw("no module data");
                }

                return (-1L, 0L);

            }

            var datap = f.datap;
            var p = datap.pclntable[off..];
            ref var pc = ref heap(f.entry, out ptr<var> _addr_pc);
            var prevpc = pc;
            ref var val = ref heap(int32(-1L), out ptr<var> _addr_val);
            while (true)
            {
                bool ok = default;
                p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
                if (!ok)
                {
                    break;
                }

                if (targetpc < pc)
                { 
                    // Replace a random entry in the cache. Random
                    // replacement prevents a performance cliff if
                    // a recursive stack's cycle is slightly
                    // larger than the cache.
                    // Put the new element at the beginning,
                    // since it is the most likely to be newly used.
                    if (cache != null)
                    {
                        x = pcvalueCacheKey(targetpc);
                        var e = _addr_cache.entries[x];
                        var ci = fastrand() % uint32(len(cache.entries[x]));
                        e[ci] = e[0L];
                        e[0L] = new pcvalueCacheEnt(targetpc:targetpc,off:off,val:val,);
                    }

                    return (val, prevpc);

                }

                prevpc = pc;

            } 

            // If there was a table, it should have covered all program counters.
            // If not, something is wrong.
 

            // If there was a table, it should have covered all program counters.
            // If not, something is wrong.
            if (panicking != 0L || !strict)
            {
                return (-1L, 0L);
            }

            print("runtime: invalid pc-encoded table f=", funcname(f), " pc=", hex(pc), " targetpc=", hex(targetpc), " tab=", p, "\n");

            p = datap.pclntable[off..];
            pc = f.entry;
            val = -1L;
            while (true)
            {
                ok = default;
                p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
                if (!ok)
                {
                    break;
                }

                print("\tvalue=", val, " until pc=", hex(pc), "\n");

            }


            throw("invalid runtime symbol table");
            return (-1L, 0L);

        }

        private static ptr<byte> cfuncname(funcInfo f)
        {
            if (!f.valid() || f.nameoff == 0L)
            {
                return _addr_null!;
            }

            return _addr__addr_f.datap.pclntable[f.nameoff]!;

        }

        private static @string funcname(funcInfo f)
        {
            return gostringnocopy(cfuncname(f));
        }

        private static ptr<byte> cfuncnameFromNameoff(funcInfo f, int nameoff)
        {
            if (!f.valid())
            {
                return _addr_null!;
            }

            return _addr__addr_f.datap.pclntable[nameoff]!;

        }

        private static @string funcnameFromNameoff(funcInfo f, int nameoff)
        {
            return gostringnocopy(cfuncnameFromNameoff(f, nameoff));
        }

        private static @string funcfile(funcInfo f, int fileno)
        {
            var datap = f.datap;
            if (!f.valid())
            {
                return "?";
            }

            return gostringnocopy(_addr_datap.pclntable[datap.filetab[fileno]]);

        }

        private static (@string, int) funcline1(funcInfo f, System.UIntPtr targetpc, bool strict)
        {
            @string file = default;
            int line = default;

            var datap = f.datap;
            if (!f.valid())
            {
                return ("?", 0L);
            }

            var (fileno, _) = pcvalue(f, f.pcfile, targetpc, _addr_null, strict);
            line, _ = pcvalue(f, f.pcln, targetpc, _addr_null, strict);
            if (fileno == -1L || line == -1L || int(fileno) >= len(datap.filetab))
            { 
                // print("looking for ", hex(targetpc), " in ", funcname(f), " got file=", fileno, " line=", lineno, "\n")
                return ("?", 0L);

            }

            file = gostringnocopy(_addr_datap.pclntable[datap.filetab[fileno]]);
            return ;

        }

        private static (@string, int) funcline(funcInfo f, System.UIntPtr targetpc)
        {
            @string file = default;
            int line = default;

            return funcline1(f, targetpc, true);
        }

        private static int funcspdelta(funcInfo f, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache)
        {
            ref pcvalueCache cache = ref _addr_cache.val;

            var (x, _) = pcvalue(f, f.pcsp, targetpc, _addr_cache, true);
            if (x & (sys.PtrSize - 1L) != 0L)
            {
                print("invalid spdelta ", funcname(f), " ", hex(f.entry), " ", hex(targetpc), " ", hex(f.pcsp), " ", x, "\n");
            }

            return x;

        }

        // funcMaxSPDelta returns the maximum spdelta at any point in f.
        private static int funcMaxSPDelta(funcInfo f)
        {
            var datap = f.datap;
            var p = datap.pclntable[f.pcsp..];
            ref var pc = ref heap(f.entry, out ptr<var> _addr_pc);
            ref var val = ref heap(int32(-1L), out ptr<var> _addr_val);
            var max = int32(0L);
            while (true)
            {
                bool ok = default;
                p, ok = step(p, _addr_pc, _addr_val, pc == f.entry);
                if (!ok)
                {
                    return max;
                }

                if (val > max)
                {
                    max = val;
                }

            }


        }

        private static int pcdatastart(funcInfo f, int table)
        {
            return new ptr<ptr<ptr<int>>>(add(@unsafe.Pointer(_addr_f.nfuncdata), @unsafe.Sizeof(f.nfuncdata) + uintptr(table) * 4L));
        }

        private static int pcdatavalue(funcInfo f, int table, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache)
        {
            ref pcvalueCache cache = ref _addr_cache.val;

            if (table < 0L || table >= f.npcdata)
            {
                return -1L;
            }

            var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, _addr_cache, true);
            return r;

        }

        private static int pcdatavalue1(funcInfo f, int table, System.UIntPtr targetpc, ptr<pcvalueCache> _addr_cache, bool strict)
        {
            ref pcvalueCache cache = ref _addr_cache.val;

            if (table < 0L || table >= f.npcdata)
            {
                return -1L;
            }

            var (r, _) = pcvalue(f, pcdatastart(f, table), targetpc, _addr_cache, strict);
            return r;

        }

        // Like pcdatavalue, but also return the start PC of this PCData value.
        // It doesn't take a cache.
        private static (int, System.UIntPtr) pcdatavalue2(funcInfo f, int table, System.UIntPtr targetpc)
        {
            int _p0 = default;
            System.UIntPtr _p0 = default;

            if (table < 0L || table >= f.npcdata)
            {
                return (-1L, 0L);
            }

            return pcvalue(f, pcdatastart(f, table), targetpc, _addr_null, true);

        }

        private static unsafe.Pointer funcdata(funcInfo f, byte i)
        {
            if (i < 0L || i >= f.nfuncdata)
            {
                return null;
            }

            var p = add(@unsafe.Pointer(_addr_f.nfuncdata), @unsafe.Sizeof(f.nfuncdata) + uintptr(f.npcdata) * 4L);
            if (sys.PtrSize == 8L && uintptr(p) & 4L != 0L)
            {
                if (uintptr(@unsafe.Pointer(f._func)) & 4L != 0L)
                {
                    println("runtime: misaligned func", f._func);
                }

                p = add(p, 4L);

            }

            return new ptr<ptr<ptr<unsafe.Pointer>>>(add(p, uintptr(i) * sys.PtrSize));

        }

        // step advances to the next pc, value pair in the encoded table.
        private static (slice<byte>, bool) step(slice<byte> p, ptr<System.UIntPtr> _addr_pc, ptr<int> _addr_val, bool first)
        {
            slice<byte> newp = default;
            bool ok = default;
            ref System.UIntPtr pc = ref _addr_pc.val;
            ref int val = ref _addr_val.val;
 
            // For both uvdelta and pcdelta, the common case (~70%)
            // is that they are a single byte. If so, avoid calling readvarint.
            var uvdelta = uint32(p[0L]);
            if (uvdelta == 0L && !first)
            {
                return (null, false);
            }

            var n = uint32(1L);
            if (uvdelta & 0x80UL != 0L)
            {
                n, uvdelta = readvarint(p);
            }

            val += int32(-(uvdelta & 1L) ^ (uvdelta >> (int)(1L)));
            p = p[n..];

            var pcdelta = uint32(p[0L]);
            n = 1L;
            if (pcdelta & 0x80UL != 0L)
            {
                n, pcdelta = readvarint(p);
            }

            p = p[n..];
            pc += uintptr(pcdelta * sys.PCQuantum);
            return (p, true);

        }

        // readvarint reads a varint from p.
        private static (uint, uint) readvarint(slice<byte> p)
        {
            uint read = default;
            uint val = default;

            uint v = default;            uint shift = default;            uint n = default;

            while (true)
            {
                var b = p[n];
                n++;
                v |= uint32(b & 0x7FUL) << (int)((shift & 31L));
                if (b & 0x80UL == 0L)
                {
                    break;
                }

                shift += 7L;

            }

            return (n, v);

        }

        private partial struct stackmap
        {
            public int n; // number of bitmaps
            public int nbit; // number of bits in each bitmap
            public array<byte> bytedata; // bitmaps, each starting on a byte boundary
        }

        //go:nowritebarrier
        private static bitvector stackmapdata(ptr<stackmap> _addr_stkmap, int n)
        {
            ref stackmap stkmap = ref _addr_stkmap.val;
 
            // Check this invariant only when stackDebug is on at all.
            // The invariant is already checked by many of stackmapdata's callers,
            // and disabling it by default allows stackmapdata to be inlined.
            if (stackDebug > 0L && (n < 0L || n >= stkmap.n))
            {
                throw("stackmapdata: index out of range");
            }

            return new bitvector(stkmap.nbit,addb(&stkmap.bytedata[0],uintptr(n*((stkmap.nbit+7)>>3))));

        }

        // inlinedCall is the encoding of entries in the FUNCDATA_InlTree table.
        private partial struct inlinedCall
        {
            public short parent; // index of parent in the inltree, or < 0
            public funcID funcID; // type of the called function
            public byte _;
            public int file; // fileno index into filetab
            public int line; // line number of the call site
            public int func_; // offset into pclntab for name of called function
            public int parentPc; // position of an instruction whose source position is the call site (offset from entry)
        }
    }
}
