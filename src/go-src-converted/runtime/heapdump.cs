// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implementation of runtime/debug.WriteHeapDump. Writes all
// objects in the heap plus additional info (roots, threads,
// finalizers, etc.) to a file.
// The format of the dumped file is described at
// https://golang.org/s/go15heapdump.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

//go:linkname runtime_debug_WriteHeapDump runtime/debug.WriteHeapDump
internal static void runtime_debug_WriteHeapDump(uintptr fd) {
    var stw = stopTheWorld(stwWriteHeapDump);
    // Keep m on this G's stack instead of the system stack.
    // Both readmemstats_m and writeheapdump_m have pretty large
    // peak stack depths and we risk blowing the system stack.
    // This is safe because the world is stopped, so we don't
    // need to worry about anyone shrinking and therefore moving
    // our stack.
    ref var m = ref heap(new MemStats(), out var Ꮡm);
    systemstack(
    var mʗ2 = m;
    () => {
        readmemstats_m(Ꮡmʗ2);
        writeheapdump_m(fd, Ꮡmʗ2);
    });
    startTheWorld(stw);
}

internal static readonly UntypedInt fieldKindEol = 0;
internal static readonly UntypedInt fieldKindPtr = 1;
internal static readonly UntypedInt fieldKindIface = 2;
internal static readonly UntypedInt fieldKindEface = 3;
internal static readonly UntypedInt tagEOF = 0;
internal static readonly UntypedInt tagObject = 1;
internal static readonly UntypedInt tagOtherRoot = 2;
internal static readonly UntypedInt tagType = 3;
internal static readonly UntypedInt tagGoroutine = 4;
internal static readonly UntypedInt tagStackFrame = 5;
internal static readonly UntypedInt tagParams = 6;
internal static readonly UntypedInt tagFinalizer = 7;
internal static readonly UntypedInt tagItab = 8;
internal static readonly UntypedInt tagOSThread = 9;
internal static readonly UntypedInt tagMemStats = 10;
internal static readonly UntypedInt tagQueuedFinalizer = 11;
internal static readonly UntypedInt tagData = 12;
internal static readonly UntypedInt tagBSS = 13;
internal static readonly UntypedInt tagDefer = 14;
internal static readonly UntypedInt tagPanic = 15;
internal static readonly UntypedInt tagMemProf = 16;
internal static readonly UntypedInt tagAllocSample = 17;

internal static uintptr dumpfd; // fd to write the dump to.

internal static slice<byte> tmpbuf;

// buffer of pending write data
internal static readonly UntypedInt bufSize = 4096;

internal static array<byte> buf;

internal static uintptr nbuf;

internal static unsafe void dwrite(@unsafe.Pointer data, uintptr len) {
    if (len == 0) {
        return;
    }
    if (nbuf + len <= bufSize) {
        copy(buf[(int)(nbuf)..], new Span<byte>((byte*)(uintptr)(data), len));
        nbuf += len;
        return;
    }
    write(dumpfd, new @unsafe.Pointer(Ꮡ(buf)), ((int32)nbuf));
    if (len >= bufSize){
        write(dumpfd, data.val, ((int32)len));
        nbuf = 0;
    } else {
        copy(buf[..], new Span<byte>((byte*)(uintptr)(data), len));
        nbuf = len;
    }
}

internal static void dwritebyte(byte b) {
    dwrite(new @unsafe.Pointer(Ꮡ(b)), 1);
}

internal static void flush() {
    write(dumpfd, new @unsafe.Pointer(Ꮡ(buf)), ((int32)nbuf));
    nbuf = 0;
}

// Cache of types that have been serialized already.
// We use a type's hash field to pick a bucket.
// Inside a bucket, we keep a list of types that
// have been serialized so far, most recently used first.
// Note: when a bucket overflows we may end up
// serializing a type more than once. That's ok.
internal static readonly UntypedInt typeCacheBuckets = 256;

internal static readonly UntypedInt typeCacheAssoc = 4;

[GoType] partial struct typeCacheBucket {
    internal array<ж<_type>> t = new(typeCacheAssoc);
}

internal static array<typeCacheBucket> typecache;

// dump a uint64 in a varint format parseable by encoding/binary.
internal static void dumpint(uint64 v) {
    ref var buf = ref heap(new array<byte>(10), out var Ꮡbuf);
    nint n = default!;
    while (v >= 128) {
        buf[n] = ((byte)((uint64)(v | 128)));
        n++;
        v >>= (UntypedInt)(7);
    }
    buf[n] = ((byte)v);
    n++;
    dwrite(new @unsafe.Pointer(Ꮡbuf), ((uintptr)n));
}

internal static void dumpbool(bool b) {
    if (b){
        dumpint(1);
    } else {
        dumpint(0);
    }
}

// dump varint uint64 length followed by memory contents.
internal static void dumpmemrange(@unsafe.Pointer data, uintptr len) {
    dumpint(((uint64)len));
    dwrite(data.val, len);
}

internal static void dumpslice(slice<byte> b) {
    dumpint(((uint64)len(b)));
    if (len(b) > 0) {
        dwrite(new @unsafe.Pointer(Ꮡ(b, 0)), ((uintptr)len(b)));
    }
}

internal static void dumpstr(@string s) {
    dumpmemrange(new @unsafe.Pointer(@unsafe.StringData(s)), ((uintptr)len(s)));
}

// dump information for a type.
internal static void dumptype(ж<_type> Ꮡt) {
    ref var t = ref Ꮡt.val;

    if (t == nil) {
        return;
    }
    // If we've definitely serialized the type before,
    // no need to do it again.
    var b = Ꮡtypecache.at<typeCacheBucket>((uint32)(t.Hash & (typeCacheBuckets - 1)));
    if (Ꮡt == (~b).t[0]) {
        return;
    }
    for (nint i = 1; i < typeCacheAssoc; i++) {
        if (Ꮡt == (~b).t[i]) {
            // Move-to-front
            for (nint jΔ1 = i; jΔ1 > 0; jΔ1--) {
                (~b).t[j] = (~b).t[jΔ1 - 1];
            }
            (~b).t[0] = t;
            return;
        }
    }
    // Might not have been dumped yet. Dump it and
    // remember we did so.
    for (nint j = typeCacheAssoc - 1; j > 0; j--) {
        (~b).t[j] = (~b).t[j - 1];
    }
    (~b).t[0] = t;
    // dump the type
    dumpint(tagType);
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡt))));
    dumpint(((uint64)t.Size_));
    var rt = toRType(Ꮡt);
    {
        var x = t.Uncommon(); if (x == nil || rt.nameOff((~x).PkgPath).Name() == ""u8){
            dumpstr(rt.@string());
        } else {
            @string pkgpath = rt.nameOff((~x).PkgPath).Name();
            @string name = rt.name();
            dumpint(((uint64)(((uintptr)len(pkgpath)) + 1 + ((uintptr)len(name)))));
            dwrite(new @unsafe.Pointer(@unsafe.StringData(pkgpath)), ((uintptr)len(pkgpath)));
            dwritebyte((rune)'.');
            dwrite(new @unsafe.Pointer(@unsafe.StringData(name)), ((uintptr)len(name)));
        }
    }
    dumpbool((abiꓸKind)(t.Kind_ & abi.KindDirectIface) == 0 || t.PtrBytes != 0);
}

// dump an object.
internal static void dumpobj(@unsafe.Pointer obj, uintptr size, bitvector bv) {
    dumpint(tagObject);
    dumpint(((uint64)((uintptr)obj)));
    dumpmemrange(obj.val, size);
    dumpfields(bv);
}

internal static void dumpotherroot(@string description, @unsafe.Pointer to) {
    dumpint(tagOtherRoot);
    dumpstr(description);
    dumpint(((uint64)((uintptr)to)));
}

internal static void dumpfinalizer(@unsafe.Pointer obj, ж<funcval> Ꮡfn, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fn = ref Ꮡfn.val;
    ref var fint = ref Ꮡfint.val;
    ref var ot = ref Ꮡot.val;

    dumpint(tagFinalizer);
    dumpint(((uint64)((uintptr)obj)));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡfn))));
    dumpint(((uint64)((uintptr)((@unsafe.Pointer)fn.fn))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡfint))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡot))));
}

[GoType] partial struct childInfo {
    // Information passed up from the callee frame about
    // the layout of the outargs region.
    internal uintptr argoff;   // where the arguments start in the frame
    internal uintptr arglen;   // size of args region
    internal bitvector args; // if args.n >= 0, pointer map of args region
    internal ж<uint8> sp; // callee sp
    internal uintptr depth;   // depth in call stack (0 == most recent)
}

// dump kinds & offsets of interesting fields in bv.
internal static void dumpbv(ж<bitvector> Ꮡcbv, uintptr offset) {
    ref var cbv = ref Ꮡcbv.val;

    for (var i = ((uintptr)0); i < ((uintptr)cbv.n); i++) {
        if (cbv.ptrbit(i) == 1) {
            dumpint(fieldKindPtr);
            dumpint(((uint64)(offset + i * goarch.PtrSize)));
        }
    }
}

internal static void dumpframe(ж<stkframe> Ꮡs, ж<childInfo> Ꮡchild) {
    ref var s = ref Ꮡs.val;
    ref var child = ref Ꮡchild.val;

    var f = s.fn;
    // Figure out what we can about our stack map
    var pc = s.pc;
    var pcdata = ((int32)(-1));
    // Use the entry map at function entry
    if (pc != f.entry()) {
        pc--;
        pcdata = pcdatavalue(f, abi.PCDATA_StackMapIndex, pc);
    }
    if (pcdata == -1) {
        // We do not have a valid pcdata value but there might be a
        // stackmap for this function. It is likely that we are looking
        // at the function prologue, assume so and hope for the best.
        pcdata = 0;
    }
    var stkmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_LocalsPointerMaps));
    ref var bv = ref heap(new bitvector(), out var Ꮡbv);
    if (stkmap != nil && (~stkmap).n > 0){
        bv = stackmapdata(stkmap, pcdata);
    } else {
        bv.n = -1;
    }
    // Dump main body of stack frame.
    dumpint(tagStackFrame);
    dumpint(((uint64)s.sp));
    // lowest address in frame
    dumpint(((uint64)child.depth));
    // # of frames deep on the stack
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(child.sp))));
    // sp of child, or 0 if bottom of stack
    dumpmemrange(((@unsafe.Pointer)s.sp), s.fp - s.sp);
    // frame contents
    dumpint(((uint64)f.entry()));
    dumpint(((uint64)s.pc));
    dumpint(((uint64)s.continpc));
    @string name = funcname(f);
    if (name == ""u8) {
        name = "unknown function"u8;
    }
    dumpstr(name);
    // Dump fields in the outargs section
    if (child.args.n >= 0){
        dumpbv(Ꮡ(child.args), child.argoff);
    } else {
        // conservative - everything might be a pointer
        for (var off = child.argoff; off < child.argoff + child.arglen; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint(((uint64)off));
        }
    }
    // Dump fields in the local vars section
    if (stkmap == nil){
        // No locals information, dump everything.
        for (var off = child.arglen; off < s.varp - s.sp; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint(((uint64)off));
        }
    } else 
    if ((~stkmap).n < 0){
        // Locals size information, dump just the locals.
        var size = ((uintptr)(-(~stkmap).n));
        for (var off = s.varp - size - s.sp; off < s.varp - s.sp; off += goarch.PtrSize) {
            dumpint(fieldKindPtr);
            dumpint(((uint64)off));
        }
    } else 
    if ((~stkmap).n > 0) {
        // Locals bitmap information, scan just the pointers in
        // locals.
        dumpbv(Ꮡbv, s.varp - ((uintptr)bv.n) * goarch.PtrSize - s.sp);
    }
    dumpint(fieldKindEol);
    // Record arg info for parent.
    child.argoff = s.argp - s.fp;
    child.arglen = s.argBytes();
    child.sp = (ж<uint8>)(uintptr)(((@unsafe.Pointer)s.sp));
    child.depth++;
    stkmap = (ж<stackmap>)(uintptr)(funcdata(f, abi.FUNCDATA_ArgsPointerMaps));
    if (stkmap != nil){
        child.args = stackmapdata(stkmap, pcdata);
    } else {
        child.args.n = -1;
    }
    return;
}

internal static void dumpgoroutine(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.val;

    uintptr sp = default!;
    uintptr pc = default!;
    uintptr lr = default!;
    if (gp.syscallsp != 0){
        sp = gp.syscallsp;
        pc = gp.syscallpc;
        lr = 0;
    } else {
        sp = gp.sched.sp;
        pc = gp.sched.pc;
        lr = gp.sched.lr;
    }
    dumpint(tagGoroutine);
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡgp))));
    dumpint(((uint64)sp));
    dumpint(gp.goid);
    dumpint(((uint64)gp.gopc));
    dumpint(((uint64)readgstatus(Ꮡgp)));
    dumpbool(isSystemGoroutine(Ꮡgp, false));
    dumpbool(false);
    // isbackground
    dumpint(((uint64)gp.waitsince));
    dumpstr(gp.waitreason.String());
    dumpint(((uint64)((uintptr)gp.sched.ctxt)));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(gp.m))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(gp._defer))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(gp._panic))));
    // dump stack
    ref var child = ref heap(new childInfo(), out var Ꮡchild);
    child.args.n = -1;
    child.arglen = 0;
    child.sp = default!;
    child.depth = 0;
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    for (
    u.initAt(pc, sp, lr, Ꮡgp, 0);; u.valid(); 
    u.next();) {
        dumpframe(Ꮡu.of(unwinder.Ꮡframe), Ꮡchild);
    }
    // dump defer & panic records
    for (var d = gp._defer; d != nil; d = d.val.link) {
        dumpint(tagDefer);
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(d))));
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡgp))));
        dumpint(((uint64)(~d).sp));
        dumpint(((uint64)(~d).pc));
        var fn = ~(ж<ж<funcval>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~d).fn)));
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(fn))));
        if ((~d).fn == default!){
            // d.fn can be nil for open-coded defers
            dumpint(((uint64)0));
        } else {
            dumpint(((uint64)((uintptr)((@unsafe.Pointer)(~fn).fn))));
        }
        dumpint(((uint64)((uintptr)new @unsafe.Pointer((~d).link))));
    }
    for (var Δp = gp._panic; Δp != nil; Δp = Δp.val.link) {
        dumpint(tagPanic);
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(Δp))));
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡgp))));
        var eface = efaceOf(Ꮡ((~Δp).arg));
        dumpint(((uint64)((uintptr)new @unsafe.Pointer((~eface)._type))));
        dumpint(((uint64)((uintptr)(~eface).data)));
        dumpint(0);
        // was p->defer, no longer recorded
        dumpint(((uint64)((uintptr)new @unsafe.Pointer((~Δp).link))));
    }
}

internal static void dumpgs() {
    assertWorldStopped();
    // goroutines & stacks
    forEachG((ж<g> gp) => {
        var status = readgstatus(gp);
        var exprᴛ2 = status;
        { /* default: */
            print("runtime: unexpected G.status ", ((Δhex)status), "\n");
            @throw("dumpgs in STW - bad status"u8);
        }
        else if (exprᴛ2 == _Gdead) {
        }
        else if (exprᴛ2 == _Grunnable || exprᴛ2 == _Gsyscall || exprᴛ2 == _Gwaiting) {
            dumpgoroutine(gp);
        }

    });
}

// ok
internal static void finq_callback(ж<funcval> Ꮡfn, @unsafe.Pointer obj, uintptr nret, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fn = ref Ꮡfn.val;
    ref var fint = ref Ꮡfint.val;
    ref var ot = ref Ꮡot.val;

    dumpint(tagQueuedFinalizer);
    dumpint(((uint64)((uintptr)obj)));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡfn))));
    dumpint(((uint64)((uintptr)((@unsafe.Pointer)fn.fn))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡfint))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡot))));
}

internal static void dumproots() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    // TODO(mwhudson): dump datamask etc from all objects
    // data segment
    dumpint(tagData);
    dumpint(((uint64)firstmoduledata.data));
    dumpmemrange(((@unsafe.Pointer)firstmoduledata.data), firstmoduledata.edata - firstmoduledata.data);
    dumpfields(firstmoduledata.gcdatamask);
    // bss segment
    dumpint(tagBSS);
    dumpint(((uint64)firstmoduledata.bss));
    dumpmemrange(((@unsafe.Pointer)firstmoduledata.bss), firstmoduledata.ebss - firstmoduledata.bss);
    dumpfields(firstmoduledata.gcbssmask);
    // mspan.types
    foreach (var (_, s) in mheap_.allspans) {
        if ((~s).state.get() == mSpanInUse) {
            // Finalizers
            for (var sp = s.val.specials; sp != nil; sp = sp.val.next) {
                if ((~sp).kind != _KindSpecialFinalizer) {
                    continue;
                }
                var spf = (ж<specialfinalizer>)(uintptr)(new @unsafe.Pointer(sp));
                @unsafe.Pointer Δp = ((@unsafe.Pointer)(s.@base() + ((uintptr)(~spf).special.offset)));
                dumpfinalizer(Δp, (~spf).fn, (~spf).fint, (~spf).ot);
            }
        }
    }
    // Finalizer queue
    iterate_finq(finq_callback);
}

// Bit vector of free marks.
// Needs to be as big as the largest number of objects per span.
internal static array<bool> freemark;

internal static void dumpobjs() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    foreach (var (_, s) in mheap_.allspans) {
        if ((~s).state.get() != mSpanInUse) {
            continue;
        }
        var Δp = s.@base();
        var size = s.val.elemsize;
        var n = ((~s).npages << (int)(_PageShift)) / size;
        if (n > ((uintptr)len(freemark))) {
            @throw("freemark array doesn't have enough entries"u8);
        }
        for (var freeIndex = ((uint16)0); freeIndex < (~s).nelems; freeIndex++) {
            if (s.isFree(((uintptr)freeIndex))) {
                freemark[freeIndex] = true;
            }
        }
        for (var j = ((uintptr)0); j < n; (j, Δp) = (j + 1, Δp + size)) {
            if (freemark[j]) {
                freemark[j] = false;
                continue;
            }
            dumpobj(((@unsafe.Pointer)Δp), size, makeheapobjbv(Δp, size));
        }
    }
}

internal static void dumpparams() {
    dumpint(tagParams);
    ref var x = ref heap<uintptr>(out var Ꮡx);
    x = ((uintptr)1);
    if (~(ж<byte>)(uintptr)(((@unsafe.Pointer)(Ꮡx))) == 1){
        dumpbool(false);
    } else {
        // little-endian ptrs
        dumpbool(true);
    }
    // big-endian ptrs
    dumpint(goarch.PtrSize);
    uintptr arenaStart = default!;
    uintptr arenaEnd = default!;
    foreach (var (i1, _) in mheap_.arenas) {
        if (mheap_.arenas[i1] == nil) {
            continue;
        }
        foreach (var (i, ha) in mheap_.arenas[i1].val) {
            if (ha == nil) {
                continue;
            }
            var @base = arenaBase((arenaIdx)(((arenaIdx)i1) << (int)(arenaL1Shift) | ((arenaIdx)i)));
            if (arenaStart == 0 || @base < arenaStart) {
                arenaStart = @base;
            }
            if (@base + heapArenaBytes > arenaEnd) {
                arenaEnd = @base + heapArenaBytes;
            }
        }
    }
    dumpint(((uint64)arenaStart));
    dumpint(((uint64)arenaEnd));
    dumpstr(goarch.GOARCH);
    dumpstr(buildVersion);
    dumpint(((uint64)ncpu));
}

internal static void itab_callback(ж<itab> Ꮡtab) {
    ref var tab = ref Ꮡtab.val;

    var t = tab.Type;
    dumptype(t);
    dumpint(tagItab);
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡtab))));
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(t))));
}

internal static void dumpitabs() {
    iterate_itabs(itab_callback);
}

internal static void dumpms() {
    for (var mp = allm; mp != nil; mp = mp.val.alllink) {
        dumpint(tagOSThread);
        dumpint(((uint64)((uintptr)new @unsafe.Pointer(mp))));
        dumpint(((uint64)(~mp).id));
        dumpint((~mp).procid);
    }
}

//go:systemstack
internal static void dumpmemstats(ж<MemStats> Ꮡm) {
    ref var m = ref Ꮡm.val;

    assertWorldStopped();
    // These ints should be identical to the exported
    // MemStats structure and should be ordered the same
    // way too.
    dumpint(tagMemStats);
    dumpint(m.Alloc);
    dumpint(m.TotalAlloc);
    dumpint(m.Sys);
    dumpint(m.Lookups);
    dumpint(m.Mallocs);
    dumpint(m.Frees);
    dumpint(m.HeapAlloc);
    dumpint(m.HeapSys);
    dumpint(m.HeapIdle);
    dumpint(m.HeapInuse);
    dumpint(m.HeapReleased);
    dumpint(m.HeapObjects);
    dumpint(m.StackInuse);
    dumpint(m.StackSys);
    dumpint(m.MSpanInuse);
    dumpint(m.MSpanSys);
    dumpint(m.MCacheInuse);
    dumpint(m.MCacheSys);
    dumpint(m.BuckHashSys);
    dumpint(m.GCSys);
    dumpint(m.OtherSys);
    dumpint(m.NextGC);
    dumpint(m.LastGC);
    dumpint(m.PauseTotalNs);
    for (nint i = 0; i < 256; i++) {
        dumpint(m.PauseNs[i]);
    }
    dumpint(((uint64)m.NumGC));
}

internal static void dumpmemprof_callback(ж<bucket> Ꮡb, uintptr nstk, ж<uintptr> Ꮡpstk, uintptr size, uintptr allocs, uintptr frees) {
    ref var b = ref Ꮡb.val;
    ref var pstk = ref Ꮡpstk.val;

    var stk = (ж<array<uintptr>>)(uintptr)(((@unsafe.Pointer)pstk));
    dumpint(tagMemProf);
    dumpint(((uint64)((uintptr)new @unsafe.Pointer(Ꮡb))));
    dumpint(((uint64)size));
    dumpint(((uint64)nstk));
    for (var i = ((uintptr)0); i < nstk; i++) {
        var pc = stk.val[i];
        var f = findfunc(pc);
        if (!f.valid()){
            array<byte> bufΔ1 = new(64);
            nint n = len(bufΔ1);
            n--;
            bufΔ1[n] = (rune)')';
            if (pc == 0){
                n--;
                bufΔ1[n] = (rune)'0';
            } else {
                while (pc > 0) {
                    n--;
                    bufΔ1[n] = "0123456789abcdef"u8[(uintptr)(pc & 15)];
                    pc >>= (UntypedInt)(4);
                }
            }
            n--;
            bufΔ1[n] = (rune)'x';
            n--;
            bufΔ1[n] = (rune)'0';
            n--;
            bufΔ1[n] = (rune)'(';
            dumpslice(bufΔ1[(int)(n)..]);
            dumpstr("?"u8);
            dumpint(0);
        } else {
            dumpstr(funcname(f));
            if (i > 0 && pc > f.entry()) {
                pc--;
            }
            var (file, line) = funcline(f, pc);
            dumpstr(file);
            dumpint(((uint64)line));
        }
    }
    dumpint(((uint64)allocs));
    dumpint(((uint64)frees));
}

internal static void dumpmemprof() {
    // To protect mheap_.allspans.
    assertWorldStopped();
    iterate_memprof(dumpmemprof_callback);
    foreach (var (_, s) in mheap_.allspans) {
        if ((~s).state.get() != mSpanInUse) {
            continue;
        }
        for (var sp = s.val.specials; sp != nil; sp = sp.val.next) {
            if ((~sp).kind != _KindSpecialProfile) {
                continue;
            }
            var spp = (ж<specialprofile>)(uintptr)(new @unsafe.Pointer(sp));
            var Δp = s.@base() + ((uintptr)(~spp).special.offset);
            dumpint(tagAllocSample);
            dumpint(((uint64)Δp));
            dumpint(((uint64)((uintptr)new @unsafe.Pointer((~spp).b))));
        }
    }
}

internal static slice<byte> dumphdr = slice<byte>("go1.7 heap dump\n");

internal static void mdump(ж<MemStats> Ꮡm) {
    ref var m = ref Ꮡm.val;

    assertWorldStopped();
    // make sure we're done sweeping
    foreach (var (_, s) in mheap_.allspans) {
        if ((~s).state.get() == mSpanInUse) {
            s.ensureSwept();
        }
    }
    memclrNoHeapPointers(new @unsafe.Pointer(Ꮡ(typecache)), @unsafe.Sizeof(typecache));
    dwrite(new @unsafe.Pointer(Ꮡ(dumphdr, 0)), ((uintptr)len(dumphdr)));
    dumpparams();
    dumpitabs();
    dumpobjs();
    dumpgs();
    dumpms();
    dumproots();
    dumpmemstats(Ꮡm);
    dumpmemprof();
    dumpint(tagEOF);
    flush();
}

internal static void writeheapdump_m(uintptr fd, ж<MemStats> Ꮡm) {
    ref var m = ref Ꮡm.val;

    assertWorldStopped();
    var gp = getg();
    casGToWaiting((~(~gp).m).curg, _Grunning, waitReasonDumpingHeap);
    // Set dump file.
    dumpfd = fd;
    // Call dump routine.
    mdump(Ꮡm);
    // Reset dump file.
    dumpfd = 0;
    if (tmpbuf != default!) {
        sysFree(new @unsafe.Pointer(Ꮡ(tmpbuf, 0)), ((uintptr)len(tmpbuf)), Ꮡmemstats.of(mstats.Ꮡother_sys));
        tmpbuf = default!;
    }
    casgstatus((~(~gp).m).curg, _Gwaiting, _Grunning);
}

// dumpint() the kind & offset of each field in an object.
internal static void dumpfields(bitvector bv) {
    dumpbv(Ꮡ(bv), 0);
    dumpint(fieldKindEol);
}

internal static unsafe bitvector makeheapobjbv(uintptr Δp, uintptr size) {
    // Extend the temp buffer if necessary.
    var nptr = size / goarch.PtrSize;
    if (((uintptr)len(tmpbuf)) < nptr / 8 + 1) {
        if (tmpbuf != default!) {
            sysFree(new @unsafe.Pointer(Ꮡ(tmpbuf, 0)), ((uintptr)len(tmpbuf)), Ꮡmemstats.of(mstats.Ꮡother_sys));
        }
        var n = nptr / 8 + 1;
        @unsafe.Pointer pΔ1 = (uintptr)sysAlloc(n, Ꮡmemstats.of(mstats.Ꮡother_sys));
        if (pΔ1 == nil) {
            @throw("heapdump: out of memory"u8);
        }
        tmpbuf = new Span<byte>((byte*)(uintptr)(pΔ1), n);
    }
    // Convert heap bitmap to pointer bitmap.
    clear(tmpbuf[..(int)(nptr / 8 + 1)]);
    var s = spanOf(Δp);
    var tp = s.typePointersOf(Δp, size);
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(Δp + size); if (addr == 0) {
                break;
            }
        }
        var i = (addr - Δp) / goarch.PtrSize;
        tmpbuf[i / 8] |= (byte)(1 << (int)((i % 8)));
    }
    return new bitvector(((int32)nptr), Ꮡ(tmpbuf, 0));
}

} // end runtime_package
