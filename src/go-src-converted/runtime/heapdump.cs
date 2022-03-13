// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Implementation of runtime/debug.WriteHeapDump. Writes all
// objects in the heap plus additional info (roots, threads,
// finalizers, etc.) to a file.

// The format of the dumped file is described at
// https://golang.org/s/go15heapdump.

// package runtime -- go2cs converted at 2022 March 13 05:24:31 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\heapdump.go
namespace go;

using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


//go:linkname runtime_debug_WriteHeapDump runtime/debug.WriteHeapDump

using System;
public static partial class runtime_package {

private static void runtime_debug_WriteHeapDump(System.UIntPtr fd) {
    stopTheWorld("write heap dump"); 

    // Keep m on this G's stack instead of the system stack.
    // Both readmemstats_m and writeheapdump_m have pretty large
    // peak stack depths and we risk blowing the system stack.
    // This is safe because the world is stopped, so we don't
    // need to worry about anyone shrinking and therefore moving
    // our stack.
    ref MemStats m = ref heap(out ptr<MemStats> _addr_m);
    systemstack(() => { 
        // Call readmemstats_m here instead of deeper in
        // writeheapdump_m because we might blow the system stack
        // otherwise.
        readmemstats_m(_addr_m);
        writeheapdump_m(fd, _addr_m);
    });

    startTheWorld();
}

private static readonly nint fieldKindEol = 0;
private static readonly nint fieldKindPtr = 1;
private static readonly nint fieldKindIface = 2;
private static readonly nint fieldKindEface = 3;
private static readonly nint tagEOF = 0;
private static readonly nint tagObject = 1;
private static readonly nint tagOtherRoot = 2;
private static readonly nint tagType = 3;
private static readonly nint tagGoroutine = 4;
private static readonly nint tagStackFrame = 5;
private static readonly nint tagParams = 6;
private static readonly nint tagFinalizer = 7;
private static readonly nint tagItab = 8;
private static readonly nint tagOSThread = 9;
private static readonly nint tagMemStats = 10;
private static readonly nint tagQueuedFinalizer = 11;
private static readonly nint tagData = 12;
private static readonly nint tagBSS = 13;
private static readonly nint tagDefer = 14;
private static readonly nint tagPanic = 15;
private static readonly nint tagMemProf = 16;
private static readonly nint tagAllocSample = 17;

private static System.UIntPtr dumpfd = default; // fd to write the dump to.
private static slice<byte> tmpbuf = default;

// buffer of pending write data
private static readonly nint bufSize = 4096;

private static array<byte> buf = new array<byte>(bufSize);
private static System.UIntPtr nbuf = default;

private static void dwrite(unsafe.Pointer data, System.UIntPtr len) {
    if (len == 0) {
        return ;
    }
    if (nbuf + len <= bufSize) {
        copy(buf[(int)nbuf..], new ptr<ptr<array<byte>>>(data)[..(int)len]);
        nbuf += len;
        return ;
    }
    write(dumpfd, @unsafe.Pointer(_addr_buf), int32(nbuf));
    if (len >= bufSize) {
        write(dumpfd, data, int32(len));
        nbuf = 0;
    }
    else
 {
        copy(buf[..], new ptr<ptr<array<byte>>>(data)[..(int)len]);
        nbuf = len;
    }
}

private static void dwritebyte(byte b) {
    dwrite(@unsafe.Pointer(_addr_b), 1);
}

private static void flush() {
    write(dumpfd, @unsafe.Pointer(_addr_buf), int32(nbuf));
    nbuf = 0;
}

// Cache of types that have been serialized already.
// We use a type's hash field to pick a bucket.
// Inside a bucket, we keep a list of types that
// have been serialized so far, most recently used first.
// Note: when a bucket overflows we may end up
// serializing a type more than once. That's ok.
private static readonly nint typeCacheBuckets = 256;
private static readonly nint typeCacheAssoc = 4;

private partial struct typeCacheBucket {
    public array<ptr<_type>> t;
}

private static array<typeCacheBucket> typecache = new array<typeCacheBucket>(typeCacheBuckets);

// dump a uint64 in a varint format parseable by encoding/binary
private static void dumpint(ulong v) {
    ref array<byte> buf = ref heap(new array<byte>(10), out ptr<array<byte>> _addr_buf);
    nint n = default;
    while (v >= 0x80) {
        buf[n] = byte(v | 0x80);
        n++;
        v>>=7;
    }
    buf[n] = byte(v);
    n++;
    dwrite(@unsafe.Pointer(_addr_buf), uintptr(n));
}

private static void dumpbool(bool b) {
    if (b) {
        dumpint(1);
    }
    else
 {
        dumpint(0);
    }
}

// dump varint uint64 length followed by memory contents
private static void dumpmemrange(unsafe.Pointer data, System.UIntPtr len) {
    dumpint(uint64(len));
    dwrite(data, len);
}

private static void dumpslice(slice<byte> b) {
    dumpint(uint64(len(b)));
    if (len(b) > 0) {
        dwrite(@unsafe.Pointer(_addr_b[0]), uintptr(len(b)));
    }
}

private static void dumpstr(@string s) {
    var sp = stringStructOf(_addr_s);
    dumpmemrange(sp.str, uintptr(sp.len));
}

// dump information for a type
private static void dumptype(ptr<_type> _addr_t) {
    ref _type t = ref _addr_t.val;

    if (t == null) {
        return ;
    }
    var b = _addr_typecache[t.hash & (typeCacheBuckets - 1)];
    if (t == b.t[0]) {
        return ;
    }
    for (nint i = 1; i < typeCacheAssoc; i++) {
        if (t == b.t[i]) { 
            // Move-to-front
            {
                var j__prev2 = j;

                for (var j = i; j > 0; j--) {
                    b.t[j] = b.t[j - 1];
                }


                j = j__prev2;
            }
            b.t[0] = t;
            return ;
        }
    } 

    // Might not have been dumped yet. Dump it and
    // remember we did so.
    {
        var j__prev1 = j;

        for (j = typeCacheAssoc - 1; j > 0; j--) {
            b.t[j] = b.t[j - 1];
        }

        j = j__prev1;
    }
    b.t[0] = t; 

    // dump the type
    dumpint(tagType);
    dumpint(uint64(uintptr(@unsafe.Pointer(t))));
    dumpint(uint64(t.size));
    {
        var x = t.uncommon();

        if (x == null || t.nameOff(x.pkgpath).name() == "") {
            dumpstr(t.@string());
        }
        else
 {
            ref var pkgpathstr = ref heap(t.nameOff(x.pkgpath).name(), out ptr<var> _addr_pkgpathstr);
            var pkgpath = stringStructOf(_addr_pkgpathstr);
            ref var namestr = ref heap(t.name(), out ptr<var> _addr_namestr);
            var name = stringStructOf(_addr_namestr);
            dumpint(uint64(uintptr(pkgpath.len) + 1 + uintptr(name.len)));
            dwrite(pkgpath.str, uintptr(pkgpath.len));
            dwritebyte('.');
            dwrite(name.str, uintptr(name.len));
        }
    }
    dumpbool(t.kind & kindDirectIface == 0 || t.ptrdata != 0);
}

// dump an object
private static void dumpobj(unsafe.Pointer obj, System.UIntPtr size, bitvector bv) {
    dumpint(tagObject);
    dumpint(uint64(uintptr(obj)));
    dumpmemrange(obj, size);
    dumpfields(bv);
}

private static void dumpotherroot(@string description, unsafe.Pointer to) {
    dumpint(tagOtherRoot);
    dumpstr(description);
    dumpint(uint64(uintptr(to)));
}

private static void dumpfinalizer(unsafe.Pointer obj, ptr<funcval> _addr_fn, ptr<_type> _addr_fint, ptr<ptrtype> _addr_ot) {
    ref funcval fn = ref _addr_fn.val;
    ref _type fint = ref _addr_fint.val;
    ref ptrtype ot = ref _addr_ot.val;

    dumpint(tagFinalizer);
    dumpint(uint64(uintptr(obj)));
    dumpint(uint64(uintptr(@unsafe.Pointer(fn))));
    dumpint(uint64(uintptr(@unsafe.Pointer(fn.fn))));
    dumpint(uint64(uintptr(@unsafe.Pointer(fint))));
    dumpint(uint64(uintptr(@unsafe.Pointer(ot))));
}

private partial struct childInfo {
    public System.UIntPtr argoff; // where the arguments start in the frame
    public System.UIntPtr arglen; // size of args region
    public bitvector args; // if args.n >= 0, pointer map of args region
    public ptr<byte> sp; // callee sp
    public System.UIntPtr depth; // depth in call stack (0 == most recent)
}

// dump kinds & offsets of interesting fields in bv
private static void dumpbv(ptr<bitvector> _addr_cbv, System.UIntPtr offset) {
    ref bitvector cbv = ref _addr_cbv.val;

    for (var i = uintptr(0); i < uintptr(cbv.n); i++) {
        if (cbv.ptrbit(i) == 1) {
            dumpint(fieldKindPtr);
            dumpint(uint64(offset + i * sys.PtrSize));
        }
    }
}

private static bool dumpframe(ptr<stkframe> _addr_s, unsafe.Pointer arg) {
    ref stkframe s = ref _addr_s.val;

    var child = (childInfo.val)(arg);
    var f = s.fn; 

    // Figure out what we can about our stack map
    var pc = s.pc;
    var pcdata = int32(-1); // Use the entry map at function entry
    if (pc != f.entry) {
        pc--;
        pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, pc, null);
    }
    if (pcdata == -1) { 
        // We do not have a valid pcdata value but there might be a
        // stackmap for this function. It is likely that we are looking
        // at the function prologue, assume so and hope for the best.
        pcdata = 0;
    }
    var stkmap = (stackmap.val)(funcdata(f, _FUNCDATA_LocalsPointerMaps));

    ref bitvector bv = ref heap(out ptr<bitvector> _addr_bv);
    if (stkmap != null && stkmap.n > 0) {
        bv = stackmapdata(stkmap, pcdata);
    }
    else
 {
        bv.n = -1;
    }
    dumpint(tagStackFrame);
    dumpint(uint64(s.sp)); // lowest address in frame
    dumpint(uint64(child.depth)); // # of frames deep on the stack
    dumpint(uint64(uintptr(@unsafe.Pointer(child.sp)))); // sp of child, or 0 if bottom of stack
    dumpmemrange(@unsafe.Pointer(s.sp), s.fp - s.sp); // frame contents
    dumpint(uint64(f.entry));
    dumpint(uint64(s.pc));
    dumpint(uint64(s.continpc));
    var name = funcname(f);
    if (name == "") {
        name = "unknown function";
    }
    dumpstr(name); 

    // Dump fields in the outargs section
    if (child.args.n >= 0) {
        dumpbv(_addr_child.args, child.argoff);
    }
    else
 { 
        // conservative - everything might be a pointer
        {
            var off__prev1 = off;

            var off = child.argoff;

            while (off < child.argoff + child.arglen) {
                dumpint(fieldKindPtr);
                dumpint(uint64(off));
                off += sys.PtrSize;
            }


            off = off__prev1;
        }
    }
    if (stkmap == null) { 
        // No locals information, dump everything.
        {
            var off__prev1 = off;

            off = child.arglen;

            while (off < s.varp - s.sp) {
                dumpint(fieldKindPtr);
                dumpint(uint64(off));
                off += sys.PtrSize;
            }


            off = off__prev1;
        }
    }
    else if (stkmap.n < 0) { 
        // Locals size information, dump just the locals.
        var size = uintptr(-stkmap.n);
        {
            var off__prev1 = off;

            off = s.varp - size - s.sp;

            while (off < s.varp - s.sp) {
                dumpint(fieldKindPtr);
                dumpint(uint64(off));
                off += sys.PtrSize;
            }


            off = off__prev1;
        }
    }
    else if (stkmap.n > 0) { 
        // Locals bitmap information, scan just the pointers in
        // locals.
        dumpbv(_addr_bv, s.varp - uintptr(bv.n) * sys.PtrSize - s.sp);
    }
    dumpint(fieldKindEol); 

    // Record arg info for parent.
    child.argoff = s.argp - s.fp;
    child.arglen = s.arglen;
    child.sp = (uint8.val)(@unsafe.Pointer(s.sp));
    child.depth++;
    stkmap = (stackmap.val)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
    if (stkmap != null) {
        child.args = stackmapdata(stkmap, pcdata);
    }
    else
 {
        child.args.n = -1;
    }
    return true;
}

private static void dumpgoroutine(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    System.UIntPtr sp = default;    System.UIntPtr pc = default;    System.UIntPtr lr = default;

    if (gp.syscallsp != 0) {
        sp = gp.syscallsp;
        pc = gp.syscallpc;
        lr = 0;
    }
    else
 {
        sp = gp.sched.sp;
        pc = gp.sched.pc;
        lr = gp.sched.lr;
    }
    dumpint(tagGoroutine);
    dumpint(uint64(uintptr(@unsafe.Pointer(gp))));
    dumpint(uint64(sp));
    dumpint(uint64(gp.goid));
    dumpint(uint64(gp.gopc));
    dumpint(uint64(readgstatus(gp)));
    dumpbool(isSystemGoroutine(gp, false));
    dumpbool(false); // isbackground
    dumpint(uint64(gp.waitsince));
    dumpstr(gp.waitreason.String());
    dumpint(uint64(uintptr(gp.sched.ctxt)));
    dumpint(uint64(uintptr(@unsafe.Pointer(gp.m))));
    dumpint(uint64(uintptr(@unsafe.Pointer(gp._defer))));
    dumpint(uint64(uintptr(@unsafe.Pointer(gp._panic)))); 

    // dump stack
    ref childInfo child = ref heap(out ptr<childInfo> _addr_child);
    child.args.n = -1;
    child.arglen = 0;
    child.sp = null;
    child.depth = 0;
    gentraceback(pc, sp, lr, gp, 0, null, 0x7fffffff, dumpframe, noescape(@unsafe.Pointer(_addr_child)), 0); 

    // dump defer & panic records
    {
        var d = gp._defer;

        while (d != null) {
            dumpint(tagDefer);
            dumpint(uint64(uintptr(@unsafe.Pointer(d))));
            dumpint(uint64(uintptr(@unsafe.Pointer(gp))));
            dumpint(uint64(d.sp));
            dumpint(uint64(d.pc));
            dumpint(uint64(uintptr(@unsafe.Pointer(d.fn))));
            if (d.fn == null) { 
                // d.fn can be nil for open-coded defers
                dumpint(uint64(0));
            d = d.link;
            }
            else
 {
                dumpint(uint64(uintptr(@unsafe.Pointer(d.fn.fn))));
            }
            dumpint(uint64(uintptr(@unsafe.Pointer(d.link))));
        }
    }
    {
        var p = gp._panic;

        while (p != null) {
            dumpint(tagPanic);
            dumpint(uint64(uintptr(@unsafe.Pointer(p))));
            dumpint(uint64(uintptr(@unsafe.Pointer(gp))));
            var eface = efaceOf(_addr_p.arg);
            dumpint(uint64(uintptr(@unsafe.Pointer(eface._type))));
            dumpint(uint64(uintptr(@unsafe.Pointer(eface.data))));
            dumpint(0); // was p->defer, no longer recorded
            dumpint(uint64(uintptr(@unsafe.Pointer(p.link))));
            p = p.link;
        }
    }
}

private static void dumpgs() {
    assertWorldStopped(); 

    // goroutines & stacks
    forEachG(gp => {
        var status = readgstatus(gp); // The world is stopped so gp will not be in a scan state.

        if (status == _Gdead)         else if (status == _Grunnable || status == _Gsyscall || status == _Gwaiting) 
            dumpgoroutine(_addr_gp);
        else 
            print("runtime: unexpected G.status ", hex(status), "\n");
            throw("dumpgs in STW - bad status");
            });
}

private static void finq_callback(ptr<funcval> _addr_fn, unsafe.Pointer obj, System.UIntPtr nret, ptr<_type> _addr_fint, ptr<ptrtype> _addr_ot) {
    ref funcval fn = ref _addr_fn.val;
    ref _type fint = ref _addr_fint.val;
    ref ptrtype ot = ref _addr_ot.val;

    dumpint(tagQueuedFinalizer);
    dumpint(uint64(uintptr(obj)));
    dumpint(uint64(uintptr(@unsafe.Pointer(fn))));
    dumpint(uint64(uintptr(@unsafe.Pointer(fn.fn))));
    dumpint(uint64(uintptr(@unsafe.Pointer(fint))));
    dumpint(uint64(uintptr(@unsafe.Pointer(ot))));
}

private static void dumproots() { 
    // To protect mheap_.allspans.
    assertWorldStopped(); 

    // TODO(mwhudson): dump datamask etc from all objects
    // data segment
    dumpint(tagData);
    dumpint(uint64(firstmoduledata.data));
    dumpmemrange(@unsafe.Pointer(firstmoduledata.data), firstmoduledata.edata - firstmoduledata.data);
    dumpfields(firstmoduledata.gcdatamask); 

    // bss segment
    dumpint(tagBSS);
    dumpint(uint64(firstmoduledata.bss));
    dumpmemrange(@unsafe.Pointer(firstmoduledata.bss), firstmoduledata.ebss - firstmoduledata.bss);
    dumpfields(firstmoduledata.gcbssmask); 

    // mspan.types
    foreach (var (_, s) in mheap_.allspans) {
        if (s.state.get() == mSpanInUse) { 
            // Finalizers
            {
                var sp = s.specials;

                while (sp != null) {
                    if (sp.kind != _KindSpecialFinalizer) {
                        continue;
                    sp = sp.next;
                    }
                    var spf = (specialfinalizer.val)(@unsafe.Pointer(sp));
                    var p = @unsafe.Pointer(s.@base() + uintptr(spf.special.offset));
                    dumpfinalizer(p, _addr_spf.fn, _addr_spf.fint, _addr_spf.ot);
                }

            }
        }
    }    iterate_finq(finq_callback);
}

// Bit vector of free marks.
// Needs to be as big as the largest number of objects per span.
private static array<bool> freemark = new array<bool>(_PageSize / 8);

private static void dumpobjs() { 
    // To protect mheap_.allspans.
    assertWorldStopped();

    foreach (var (_, s) in mheap_.allspans) {
        if (s.state.get() != mSpanInUse) {
            continue;
        }
        var p = s.@base();
        var size = s.elemsize;
        var n = (s.npages << (int)(_PageShift)) / size;
        if (n > uintptr(len(freemark))) {
            throw("freemark array doesn't have enough entries");
        }
        for (var freeIndex = uintptr(0); freeIndex < s.nelems; freeIndex++) {
            if (s.isFree(freeIndex)) {
                freemark[freeIndex] = true;
            }
        }

        {
            var j = uintptr(0);

            while (j < n) {
                if (freemark[j]) {
                    freemark[j] = false;
                    continue;
                (j, p) = (j + 1, p + size);
                }
                dumpobj(@unsafe.Pointer(p), size, makeheapobjbv(p, size));
            }

        }
    }
}

private static void dumpparams() {
    dumpint(tagParams);
    ref var x = ref heap(uintptr(1), out ptr<var> _addr_x);
    if (new ptr<ptr<ptr<byte>>>(@unsafe.Pointer(_addr_x)) == 1) {
        dumpbool(false); // little-endian ptrs
    }
    else
 {
        dumpbool(true); // big-endian ptrs
    }
    dumpint(sys.PtrSize);
    System.UIntPtr arenaStart = default;    System.UIntPtr arenaEnd = default;

    foreach (var (i1) in mheap_.arenas) {
        if (mheap_.arenas[i1] == null) {
            continue;
        }
        foreach (var (i, ha) in mheap_.arenas[i1]) {
            if (ha == null) {
                continue;
            }
            var @base = arenaBase(arenaIdx(i1) << (int)(arenaL1Shift) | arenaIdx(i));
            if (arenaStart == 0 || base < arenaStart) {
                arenaStart = base;
            }
            if (base + heapArenaBytes > arenaEnd) {
                arenaEnd = base + heapArenaBytes;
            }
        }
    }    dumpint(uint64(arenaStart));
    dumpint(uint64(arenaEnd));
    dumpstr(sys.GOARCH);
    dumpstr(buildVersion);
    dumpint(uint64(ncpu));
}

private static void itab_callback(ptr<itab> _addr_tab) {
    ref itab tab = ref _addr_tab.val;

    var t = tab._type;
    dumptype(_addr_t);
    dumpint(tagItab);
    dumpint(uint64(uintptr(@unsafe.Pointer(tab))));
    dumpint(uint64(uintptr(@unsafe.Pointer(t))));
}

private static void dumpitabs() {
    iterate_itabs(itab_callback);
}

private static void dumpms() {
    {
        var mp = allm;

        while (mp != null) {
            dumpint(tagOSThread);
            dumpint(uint64(uintptr(@unsafe.Pointer(mp))));
            dumpint(uint64(mp.id));
            dumpint(mp.procid);
            mp = mp.alllink;
        }
    }
}

//go:systemstack
private static void dumpmemstats(ptr<MemStats> _addr_m) {
    ref MemStats m = ref _addr_m.val;

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
    dumpint(uint64(m.NumGC));
}

private static void dumpmemprof_callback(ptr<bucket> _addr_b, System.UIntPtr nstk, ptr<System.UIntPtr> _addr_pstk, System.UIntPtr size, System.UIntPtr allocs, System.UIntPtr frees) {
    ref bucket b = ref _addr_b.val;
    ref System.UIntPtr pstk = ref _addr_pstk.val;

    ptr<array<System.UIntPtr>> stk = new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pstk));
    dumpint(tagMemProf);
    dumpint(uint64(uintptr(@unsafe.Pointer(b))));
    dumpint(uint64(size));
    dumpint(uint64(nstk));
    for (var i = uintptr(0); i < nstk; i++) {
        var pc = stk[i];
        var f = findfunc(pc);
        if (!f.valid()) {
            array<byte> buf = new array<byte>(64);
            var n = len(buf);
            n--;
            buf[n] = ')';
            if (pc == 0) {
                n--;
                buf[n] = '0';
            }
            else
 {
                while (pc > 0) {
                    n--;
                    buf[n] = "0123456789abcdef"[pc & 15];
                    pc>>=4;
                }
            }
        else
            n--;
            buf[n] = 'x';
            n--;
            buf[n] = '0';
            n--;
            buf[n] = '(';
            dumpslice(buf[(int)n..]);
            dumpstr("?");
            dumpint(0);
        } {
            dumpstr(funcname(f));
            if (i > 0 && pc > f.entry) {
                pc--;
            }
            var (file, line) = funcline(f, pc);
            dumpstr(file);
            dumpint(uint64(line));
        }
    }
    dumpint(uint64(allocs));
    dumpint(uint64(frees));
}

private static void dumpmemprof() { 
    // To protect mheap_.allspans.
    assertWorldStopped();

    iterate_memprof(dumpmemprof_callback);
    foreach (var (_, s) in mheap_.allspans) {
        if (s.state.get() != mSpanInUse) {
            continue;
        }
        {
            var sp = s.specials;

            while (sp != null) {
                if (sp.kind != _KindSpecialProfile) {
                    continue;
                sp = sp.next;
                }
                var spp = (specialprofile.val)(@unsafe.Pointer(sp));
                var p = s.@base() + uintptr(spp.special.offset);
                dumpint(tagAllocSample);
                dumpint(uint64(p));
                dumpint(uint64(uintptr(@unsafe.Pointer(spp.b))));
            }

        }
    }
}

private static slice<byte> dumphdr = (slice<byte>)"go1.7 heap dump\n";

private static void mdump(ptr<MemStats> _addr_m) {
    ref MemStats m = ref _addr_m.val;

    assertWorldStopped(); 

    // make sure we're done sweeping
    foreach (var (_, s) in mheap_.allspans) {
        if (s.state.get() == mSpanInUse) {
            s.ensureSwept();
        }
    }    memclrNoHeapPointers(@unsafe.Pointer(_addr_typecache), @unsafe.Sizeof(typecache));
    dwrite(@unsafe.Pointer(_addr_dumphdr[0]), uintptr(len(dumphdr)));
    dumpparams();
    dumpitabs();
    dumpobjs();
    dumpgs();
    dumpms();
    dumproots();
    dumpmemstats(_addr_m);
    dumpmemprof();
    dumpint(tagEOF);
    flush();
}

private static void writeheapdump_m(System.UIntPtr fd, ptr<MemStats> _addr_m) {
    ref MemStats m = ref _addr_m.val;

    assertWorldStopped();

    var _g_ = getg();
    casgstatus(_g_.m.curg, _Grunning, _Gwaiting);
    _g_.waitreason = waitReasonDumpingHeap; 

    // Update stats so we can dump them.
    // As a side effect, flushes all the mcaches so the mspan.freelist
    // lists contain all the free objects.
    updatememstats(); 

    // Set dump file.
    dumpfd = fd; 

    // Call dump routine.
    mdump(_addr_m); 

    // Reset dump file.
    dumpfd = 0;
    if (tmpbuf != null) {
        sysFree(@unsafe.Pointer(_addr_tmpbuf[0]), uintptr(len(tmpbuf)), _addr_memstats.other_sys);
        tmpbuf = null;
    }
    casgstatus(_g_.m.curg, _Gwaiting, _Grunning);
}

// dumpint() the kind & offset of each field in an object.
private static void dumpfields(bitvector bv) {
    dumpbv(_addr_bv, 0);
    dumpint(fieldKindEol);
}

private static bitvector makeheapobjbv(System.UIntPtr p, System.UIntPtr size) { 
    // Extend the temp buffer if necessary.
    var nptr = size / sys.PtrSize;
    if (uintptr(len(tmpbuf)) < nptr / 8 + 1) {
        if (tmpbuf != null) {
            sysFree(@unsafe.Pointer(_addr_tmpbuf[0]), uintptr(len(tmpbuf)), _addr_memstats.other_sys);
        }
        var n = nptr / 8 + 1;
        var p = sysAlloc(n, _addr_memstats.other_sys);
        if (p == null) {
            throw("heapdump: out of memory");
        }
        tmpbuf = new ptr<ptr<array<byte>>>(p)[..(int)n];
    }
    {
        var i__prev1 = i;

        for (var i = uintptr(0); i < nptr / 8 + 1; i++) {
            tmpbuf[i] = 0;
        }

        i = i__prev1;
    }
    i = uintptr(0);
    var hbits = heapBitsForAddr(p);
    while (i < nptr) {
        if (!hbits.morePointers()) {
            break; // end of object
        i++;
        }
        if (hbits.isPointer()) {
            tmpbuf[i / 8] |= 1 << (int)((i % 8));
        }
        hbits = hbits.next();
    }
    return new bitvector(int32(i),&tmpbuf[0]);
}

} // end runtime_package
