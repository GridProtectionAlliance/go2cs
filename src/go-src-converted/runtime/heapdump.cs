// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Implementation of runtime/debug.WriteHeapDump. Writes all
// objects in the heap plus additional info (roots, threads,
// finalizers, etc.) to a file.

// The format of the dumped file is described at
// https://golang.org/s/go15heapdump.

// package runtime -- go2cs converted at 2020 August 29 08:17:18 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\heapdump.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        //go:linkname runtime_debug_WriteHeapDump runtime/debug.WriteHeapDump
        private static void runtime_debug_WriteHeapDump(System.UIntPtr fd)
        {
            stopTheWorld("write heap dump");

            systemstack(() =>
            {
                writeheapdump_m(fd);
            });

            startTheWorld();
        }

        private static readonly long fieldKindEol = 0L;
        private static readonly long fieldKindPtr = 1L;
        private static readonly long fieldKindIface = 2L;
        private static readonly long fieldKindEface = 3L;
        private static readonly long tagEOF = 0L;
        private static readonly long tagObject = 1L;
        private static readonly long tagOtherRoot = 2L;
        private static readonly long tagType = 3L;
        private static readonly long tagGoroutine = 4L;
        private static readonly long tagStackFrame = 5L;
        private static readonly long tagParams = 6L;
        private static readonly long tagFinalizer = 7L;
        private static readonly long tagItab = 8L;
        private static readonly long tagOSThread = 9L;
        private static readonly long tagMemStats = 10L;
        private static readonly long tagQueuedFinalizer = 11L;
        private static readonly long tagData = 12L;
        private static readonly long tagBSS = 13L;
        private static readonly long tagDefer = 14L;
        private static readonly long tagPanic = 15L;
        private static readonly long tagMemProf = 16L;
        private static readonly long tagAllocSample = 17L;

        private static System.UIntPtr dumpfd = default; // fd to write the dump to.
        private static slice<byte> tmpbuf = default;

        // buffer of pending write data
        private static readonly long bufSize = 4096L;

        private static array<byte> buf = new array<byte>(bufSize);
        private static System.UIntPtr nbuf = default;

        private static void dwrite(unsafe.Pointer data, System.UIntPtr len)
        {
            if (len == 0L)
            {
                return;
            }
            if (nbuf + len <= bufSize)
            {
                copy(buf[nbuf..], new ptr<ref array<byte>>(data)[..len]);
                nbuf += len;
                return;
            }
            write(dumpfd, @unsafe.Pointer(ref buf), int32(nbuf));
            if (len >= bufSize)
            {
                write(dumpfd, data, int32(len));
                nbuf = 0L;
            }
            else
            {
                copy(buf[..], new ptr<ref array<byte>>(data)[..len]);
                nbuf = len;
            }
        }

        private static void dwritebyte(byte b)
        {
            dwrite(@unsafe.Pointer(ref b), 1L);
        }

        private static void flush()
        {
            write(dumpfd, @unsafe.Pointer(ref buf), int32(nbuf));
            nbuf = 0L;
        }

        // Cache of types that have been serialized already.
        // We use a type's hash field to pick a bucket.
        // Inside a bucket, we keep a list of types that
        // have been serialized so far, most recently used first.
        // Note: when a bucket overflows we may end up
        // serializing a type more than once. That's ok.
        private static readonly long typeCacheBuckets = 256L;
        private static readonly long typeCacheAssoc = 4L;

        private partial struct typeCacheBucket
        {
            public array<ref _type> t;
        }

        private static array<typeCacheBucket> typecache = new array<typeCacheBucket>(typeCacheBuckets);

        // dump a uint64 in a varint format parseable by encoding/binary
        private static void dumpint(ulong v)
        {
            array<byte> buf = new array<byte>(10L);
            long n = default;
            while (v >= 0x80UL)
            {
                buf[n] = byte(v | 0x80UL);
                n++;
                v >>= 7L;
            }

            buf[n] = byte(v);
            n++;
            dwrite(@unsafe.Pointer(ref buf), uintptr(n));
        }

        private static void dumpbool(bool b)
        {
            if (b)
            {
                dumpint(1L);
            }
            else
            {
                dumpint(0L);
            }
        }

        // dump varint uint64 length followed by memory contents
        private static void dumpmemrange(unsafe.Pointer data, System.UIntPtr len)
        {
            dumpint(uint64(len));
            dwrite(data, len);
        }

        private static void dumpslice(slice<byte> b)
        {
            dumpint(uint64(len(b)));
            if (len(b) > 0L)
            {
                dwrite(@unsafe.Pointer(ref b[0L]), uintptr(len(b)));
            }
        }

        private static void dumpstr(@string s)
        {
            var sp = stringStructOf(ref s);
            dumpmemrange(sp.str, uintptr(sp.len));
        }

        // dump information for a type
        private static void dumptype(ref _type t)
        {
            if (t == null)
            {
                return;
            } 

            // If we've definitely serialized the type before,
            // no need to do it again.
            var b = ref typecache[t.hash & (typeCacheBuckets - 1L)];
            if (t == b.t[0L])
            {
                return;
            }
            for (long i = 1L; i < typeCacheAssoc; i++)
            {
                if (t == b.t[i])
                { 
                    // Move-to-front
                    {
                        var j__prev2 = j;

                        for (var j = i; j > 0L; j--)
                        {
                            b.t[j] = b.t[j - 1L];
                        }


                        j = j__prev2;
                    }
                    b.t[0L] = t;
                    return;
                }
            } 

            // Might not have been dumped yet. Dump it and
            // remember we did so.
 

            // Might not have been dumped yet. Dump it and
            // remember we did so.
            {
                var j__prev1 = j;

                for (j = typeCacheAssoc - 1L; j > 0L; j--)
                {
                    b.t[j] = b.t[j - 1L];
                }


                j = j__prev1;
            }
            b.t[0L] = t; 

            // dump the type
            dumpint(tagType);
            dumpint(uint64(uintptr(@unsafe.Pointer(t))));
            dumpint(uint64(t.size));
            {
                var x = t.uncommon();

                if (x == null || t.nameOff(x.pkgpath).name() == "")
                {
                    dumpstr(t.@string());
                }
                else
                {
                    var pkgpathstr = t.nameOff(x.pkgpath).name();
                    var pkgpath = stringStructOf(ref pkgpathstr);
                    var namestr = t.name();
                    var name = stringStructOf(ref namestr);
                    dumpint(uint64(uintptr(pkgpath.len) + 1L + uintptr(name.len)));
                    dwrite(pkgpath.str, uintptr(pkgpath.len));
                    dwritebyte('.');
                    dwrite(name.str, uintptr(name.len));
                }

            }
            dumpbool(t.kind & kindDirectIface == 0L || t.kind & kindNoPointers == 0L);
        }

        // dump an object
        private static void dumpobj(unsafe.Pointer obj, System.UIntPtr size, bitvector bv)
        {
            dumpint(tagObject);
            dumpint(uint64(uintptr(obj)));
            dumpmemrange(obj, size);
            dumpfields(bv);
        }

        private static void dumpotherroot(@string description, unsafe.Pointer to)
        {
            dumpint(tagOtherRoot);
            dumpstr(description);
            dumpint(uint64(uintptr(to)));
        }

        private static void dumpfinalizer(unsafe.Pointer obj, ref funcval fn, ref _type fint, ref ptrtype ot)
        {
            dumpint(tagFinalizer);
            dumpint(uint64(uintptr(obj)));
            dumpint(uint64(uintptr(@unsafe.Pointer(fn))));
            dumpint(uint64(uintptr(@unsafe.Pointer(fn.fn))));
            dumpint(uint64(uintptr(@unsafe.Pointer(fint))));
            dumpint(uint64(uintptr(@unsafe.Pointer(ot))));
        }

        private partial struct childInfo
        {
            public System.UIntPtr argoff; // where the arguments start in the frame
            public System.UIntPtr arglen; // size of args region
            public bitvector args; // if args.n >= 0, pointer map of args region
            public ptr<byte> sp; // callee sp
            public System.UIntPtr depth; // depth in call stack (0 == most recent)
        }

        // dump kinds & offsets of interesting fields in bv
        private static void dumpbv(ref bitvector cbv, System.UIntPtr offset)
        {
            var bv = gobv(cbv.Value);
            for (var i = uintptr(0L); i < bv.n; i++)
            {
                if (bv.bytedata[i / 8L] >> (int)((i % 8L)) & 1L == 1L)
                {
                    dumpint(fieldKindPtr);
                    dumpint(uint64(offset + i * sys.PtrSize));
                }
            }

        }

        private static bool dumpframe(ref stkframe s, unsafe.Pointer arg)
        {
            var child = (childInfo.Value)(arg);
            var f = s.fn; 

            // Figure out what we can about our stack map
            var pc = s.pc;
            if (pc != f.entry)
            {
                pc--;
            }
            var pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, pc, null);
            if (pcdata == -1L)
            { 
                // We do not have a valid pcdata value but there might be a
                // stackmap for this function. It is likely that we are looking
                // at the function prologue, assume so and hope for the best.
                pcdata = 0L;
            }
            var stkmap = (stackmap.Value)(funcdata(f, _FUNCDATA_LocalsPointerMaps));

            bitvector bv = default;
            if (stkmap != null && stkmap.n > 0L)
            {
                bv = stackmapdata(stkmap, pcdata);
            }
            else
            {
                bv.n = -1L;
            } 

            // Dump main body of stack frame.
            dumpint(tagStackFrame);
            dumpint(uint64(s.sp)); // lowest address in frame
            dumpint(uint64(child.depth)); // # of frames deep on the stack
            dumpint(uint64(uintptr(@unsafe.Pointer(child.sp)))); // sp of child, or 0 if bottom of stack
            dumpmemrange(@unsafe.Pointer(s.sp), s.fp - s.sp); // frame contents
            dumpint(uint64(f.entry));
            dumpint(uint64(s.pc));
            dumpint(uint64(s.continpc));
            var name = funcname(f);
            if (name == "")
            {
                name = "unknown function";
            }
            dumpstr(name); 

            // Dump fields in the outargs section
            if (child.args.n >= 0L)
            {
                dumpbv(ref child.args, child.argoff);
            }
            else
            { 
                // conservative - everything might be a pointer
                {
                    var off__prev1 = off;

                    var off = child.argoff;

                    while (off < child.argoff + child.arglen)
                    {
                        dumpint(fieldKindPtr);
                        dumpint(uint64(off));
                        off += sys.PtrSize;
                    }


                    off = off__prev1;
                }
            } 

            // Dump fields in the local vars section
            if (stkmap == null)
            { 
                // No locals information, dump everything.
                {
                    var off__prev1 = off;

                    off = child.arglen;

                    while (off < s.varp - s.sp)
                    {
                        dumpint(fieldKindPtr);
                        dumpint(uint64(off));
                        off += sys.PtrSize;
                    }


                    off = off__prev1;
                }
            }
            else if (stkmap.n < 0L)
            { 
                // Locals size information, dump just the locals.
                var size = uintptr(-stkmap.n);
                {
                    var off__prev1 = off;

                    off = s.varp - size - s.sp;

                    while (off < s.varp - s.sp)
                    {
                        dumpint(fieldKindPtr);
                        dumpint(uint64(off));
                        off += sys.PtrSize;
                    }


                    off = off__prev1;
                }
            }
            else if (stkmap.n > 0L)
            { 
                // Locals bitmap information, scan just the pointers in
                // locals.
                dumpbv(ref bv, s.varp - uintptr(bv.n) * sys.PtrSize - s.sp);
            }
            dumpint(fieldKindEol); 

            // Record arg info for parent.
            child.argoff = s.argp - s.fp;
            child.arglen = s.arglen;
            child.sp = (uint8.Value)(@unsafe.Pointer(s.sp));
            child.depth++;
            stkmap = (stackmap.Value)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
            if (stkmap != null)
            {
                child.args = stackmapdata(stkmap, pcdata);
            }
            else
            {
                child.args.n = -1L;
            }
            return true;
        }

        private static void dumpgoroutine(ref g gp)
        {
            System.UIntPtr sp = default;            System.UIntPtr pc = default;            System.UIntPtr lr = default;

            if (gp.syscallsp != 0L)
            {
                sp = gp.syscallsp;
                pc = gp.syscallpc;
                lr = 0L;
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
            dumpbool(isSystemGoroutine(gp));
            dumpbool(false); // isbackground
            dumpint(uint64(gp.waitsince));
            dumpstr(gp.waitreason);
            dumpint(uint64(uintptr(gp.sched.ctxt)));
            dumpint(uint64(uintptr(@unsafe.Pointer(gp.m))));
            dumpint(uint64(uintptr(@unsafe.Pointer(gp._defer))));
            dumpint(uint64(uintptr(@unsafe.Pointer(gp._panic)))); 

            // dump stack
            childInfo child = default;
            child.args.n = -1L;
            child.arglen = 0L;
            child.sp = null;
            child.depth = 0L;
            gentraceback(pc, sp, lr, gp, 0L, null, 0x7fffffffUL, dumpframe, noescape(@unsafe.Pointer(ref child)), 0L); 

            // dump defer & panic records
            {
                var d = gp._defer;

                while (d != null)
                {
                    dumpint(tagDefer);
                    dumpint(uint64(uintptr(@unsafe.Pointer(d))));
                    dumpint(uint64(uintptr(@unsafe.Pointer(gp))));
                    dumpint(uint64(d.sp));
                    dumpint(uint64(d.pc));
                    dumpint(uint64(uintptr(@unsafe.Pointer(d.fn))));
                    dumpint(uint64(uintptr(@unsafe.Pointer(d.fn.fn))));
                    dumpint(uint64(uintptr(@unsafe.Pointer(d.link))));
                    d = d.link;
                }

            }
            {
                var p = gp._panic;

                while (p != null)
                {
                    dumpint(tagPanic);
                    dumpint(uint64(uintptr(@unsafe.Pointer(p))));
                    dumpint(uint64(uintptr(@unsafe.Pointer(gp))));
                    var eface = efaceOf(ref p.arg);
                    dumpint(uint64(uintptr(@unsafe.Pointer(eface._type))));
                    dumpint(uint64(uintptr(@unsafe.Pointer(eface.data))));
                    dumpint(0L); // was p->defer, no longer recorded
                    dumpint(uint64(uintptr(@unsafe.Pointer(p.link))));
                    p = p.link;
                }

            }
        }

        private static void dumpgs()
        { 
            // goroutines & stacks
            for (long i = 0L; uintptr(i) < allglen; i++)
            {
                var gp = allgs[i];
                var status = readgstatus(gp); // The world is stopped so gp will not be in a scan state.

                if (status == _Gdead)                 else if (status == _Grunnable || status == _Gsyscall || status == _Gwaiting) 
                    dumpgoroutine(gp);
                else 
                    print("runtime: unexpected G.status ", hex(status), "\n");
                    throw("dumpgs in STW - bad status");
                            }

        }

        private static void finq_callback(ref funcval fn, unsafe.Pointer obj, System.UIntPtr nret, ref _type fint, ref ptrtype ot)
        {
            dumpint(tagQueuedFinalizer);
            dumpint(uint64(uintptr(obj)));
            dumpint(uint64(uintptr(@unsafe.Pointer(fn))));
            dumpint(uint64(uintptr(@unsafe.Pointer(fn.fn))));
            dumpint(uint64(uintptr(@unsafe.Pointer(fint))));
            dumpint(uint64(uintptr(@unsafe.Pointer(ot))));
        }

        private static void dumproots()
        { 
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

            // MSpan.types
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state == _MSpanInUse)
                { 
                    // Finalizers
                    {
                        var sp = s.specials;

                        while (sp != null)
                        {
                            if (sp.kind != _KindSpecialFinalizer)
                            {
                                continue;
                            sp = sp.next;
                            }
                            var spf = (specialfinalizer.Value)(@unsafe.Pointer(sp));
                            var p = @unsafe.Pointer(s.@base() + uintptr(spf.special.offset));
                            dumpfinalizer(p, spf.fn, spf.fint, spf.ot);
                        }

                    }
                }
            } 

            // Finalizer queue
            iterate_finq(finq_callback);
        }

        // Bit vector of free marks.
        // Needs to be as big as the largest number of objects per span.
        private static array<bool> freemark = new array<bool>(_PageSize / 8L);

        private static void dumpobjs()
        {
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state != _MSpanInUse)
                {
                    continue;
                }
                var p = s.@base();
                var size = s.elemsize;
                var n = (s.npages << (int)(_PageShift)) / size;
                if (n > uintptr(len(freemark)))
                {
                    throw("freemark array doesn't have enough entries");
                }
                for (var freeIndex = uintptr(0L); freeIndex < s.nelems; freeIndex++)
                {
                    if (s.isFree(freeIndex))
                    {
                        freemark[freeIndex] = true;
                    }
                }


                {
                    var j = uintptr(0L);

                    while (j < n)
                    {
                        if (freemark[j])
                        {
                            freemark[j] = false;
                            continue;
                        j = j + 1L;
                    p = p + size;
                        }
                        dumpobj(@unsafe.Pointer(p), size, makeheapobjbv(p, size));
                    }

                }
            }
        }

        private static void dumpparams()
        {
            dumpint(tagParams);
            var x = uintptr(1L);
            if (@unsafe.Pointer(ref x).Value == 1L)
            {
                dumpbool(false); // little-endian ptrs
            }
            else
            {
                dumpbool(true); // big-endian ptrs
            }
            dumpint(sys.PtrSize);
            dumpint(uint64(mheap_.arena_start));
            dumpint(uint64(mheap_.arena_used));
            dumpstr(sys.GOARCH);
            dumpstr(sys.Goexperiment);
            dumpint(uint64(ncpu));
        }

        private static void itab_callback(ref itab tab)
        {
            var t = tab._type;
            dumptype(t);
            dumpint(tagItab);
            dumpint(uint64(uintptr(@unsafe.Pointer(tab))));
            dumpint(uint64(uintptr(@unsafe.Pointer(t))));
        }

        private static void dumpitabs()
        {
            iterate_itabs(itab_callback);
        }

        private static void dumpms()
        {
            {
                var mp = allm;

                while (mp != null)
                {
                    dumpint(tagOSThread);
                    dumpint(uint64(uintptr(@unsafe.Pointer(mp))));
                    dumpint(uint64(mp.id));
                    dumpint(mp.procid);
                    mp = mp.alllink;
                }

            }
        }

        private static void dumpmemstats()
        {
            dumpint(tagMemStats);
            dumpint(memstats.alloc);
            dumpint(memstats.total_alloc);
            dumpint(memstats.sys);
            dumpint(memstats.nlookup);
            dumpint(memstats.nmalloc);
            dumpint(memstats.nfree);
            dumpint(memstats.heap_alloc);
            dumpint(memstats.heap_sys);
            dumpint(memstats.heap_idle);
            dumpint(memstats.heap_inuse);
            dumpint(memstats.heap_released);
            dumpint(memstats.heap_objects);
            dumpint(memstats.stacks_inuse);
            dumpint(memstats.stacks_sys);
            dumpint(memstats.mspan_inuse);
            dumpint(memstats.mspan_sys);
            dumpint(memstats.mcache_inuse);
            dumpint(memstats.mcache_sys);
            dumpint(memstats.buckhash_sys);
            dumpint(memstats.gc_sys);
            dumpint(memstats.other_sys);
            dumpint(memstats.next_gc);
            dumpint(memstats.last_gc_unix);
            dumpint(memstats.pause_total_ns);
            for (long i = 0L; i < 256L; i++)
            {
                dumpint(memstats.pause_ns[i]);
            }

            dumpint(uint64(memstats.numgc));
        }

        private static void dumpmemprof_callback(ref bucket b, System.UIntPtr nstk, ref System.UIntPtr pstk, System.UIntPtr size, System.UIntPtr allocs, System.UIntPtr frees)
        {
            ref array<System.UIntPtr> stk = new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(pstk));
            dumpint(tagMemProf);
            dumpint(uint64(uintptr(@unsafe.Pointer(b))));
            dumpint(uint64(size));
            dumpint(uint64(nstk));
            for (var i = uintptr(0L); i < nstk; i++)
            {
                var pc = stk[i];
                var f = findfunc(pc);
                if (!f.valid())
                {
                    array<byte> buf = new array<byte>(64L);
                    var n = len(buf);
                    n--;
                    buf[n] = ')';
                    if (pc == 0L)
                    {
                        n--;
                        buf[n] = '0';
                    }
                    else
                    {
                        while (pc > 0L)
                        {
                            n--;
                            buf[n] = "0123456789abcdef"[pc & 15L];
                            pc >>= 4L;
                        }

                    }
                else
                    n--;
                    buf[n] = 'x';
                    n--;
                    buf[n] = '0';
                    n--;
                    buf[n] = '(';
                    dumpslice(buf[n..]);
                    dumpstr("?");
                    dumpint(0L);
                }                {
                    dumpstr(funcname(f));
                    if (i > 0L && pc > f.entry)
                    {
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

        private static void dumpmemprof()
        {
            iterate_memprof(dumpmemprof_callback);
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state != _MSpanInUse)
                {
                    continue;
                }
                {
                    var sp = s.specials;

                    while (sp != null)
                    {
                        if (sp.kind != _KindSpecialProfile)
                        {
                            continue;
                        sp = sp.next;
                        }
                        var spp = (specialprofile.Value)(@unsafe.Pointer(sp));
                        var p = s.@base() + uintptr(spp.special.offset);
                        dumpint(tagAllocSample);
                        dumpint(uint64(p));
                        dumpint(uint64(uintptr(@unsafe.Pointer(spp.b))));
                    }

                }
            }
        }

        private static slice<byte> dumphdr = (slice<byte>)"go1.7 heap dump\n";

        private static void mdump()
        { 
            // make sure we're done sweeping
            foreach (var (_, s) in mheap_.allspans)
            {
                if (s.state == _MSpanInUse)
                {
                    s.ensureSwept();
                }
            }
            memclrNoHeapPointers(@unsafe.Pointer(ref typecache), @unsafe.Sizeof(typecache));
            dwrite(@unsafe.Pointer(ref dumphdr[0L]), uintptr(len(dumphdr)));
            dumpparams();
            dumpitabs();
            dumpobjs();
            dumpgs();
            dumpms();
            dumproots();
            dumpmemstats();
            dumpmemprof();
            dumpint(tagEOF);
            flush();
        }

        private static void writeheapdump_m(System.UIntPtr fd)
        {
            var _g_ = getg();
            casgstatus(_g_.m.curg, _Grunning, _Gwaiting);
            _g_.waitreason = "dumping heap"; 

            // Update stats so we can dump them.
            // As a side effect, flushes all the MCaches so the MSpan.freelist
            // lists contain all the free objects.
            updatememstats(); 

            // Set dump file.
            dumpfd = fd; 

            // Call dump routine.
            mdump(); 

            // Reset dump file.
            dumpfd = 0L;
            if (tmpbuf != null)
            {
                sysFree(@unsafe.Pointer(ref tmpbuf[0L]), uintptr(len(tmpbuf)), ref memstats.other_sys);
                tmpbuf = null;
            }
            casgstatus(_g_.m.curg, _Gwaiting, _Grunning);
        }

        // dumpint() the kind & offset of each field in an object.
        private static void dumpfields(bitvector bv)
        {
            dumpbv(ref bv, 0L);
            dumpint(fieldKindEol);
        }

        private static bitvector makeheapobjbv(System.UIntPtr p, System.UIntPtr size)
        { 
            // Extend the temp buffer if necessary.
            var nptr = size / sys.PtrSize;
            if (uintptr(len(tmpbuf)) < nptr / 8L + 1L)
            {
                if (tmpbuf != null)
                {
                    sysFree(@unsafe.Pointer(ref tmpbuf[0L]), uintptr(len(tmpbuf)), ref memstats.other_sys);
                }
                var n = nptr / 8L + 1L;
                var p = sysAlloc(n, ref memstats.other_sys);
                if (p == null)
                {
                    throw("heapdump: out of memory");
                }
                tmpbuf = new ptr<ref array<byte>>(p)[..n];
            } 
            // Convert heap bitmap to pointer bitmap.
            {
                var i__prev1 = i;

                for (var i = uintptr(0L); i < nptr / 8L + 1L; i++)
                {
                    tmpbuf[i] = 0L;
                }


                i = i__prev1;
            }
            i = uintptr(0L);
            var hbits = heapBitsForAddr(p);
            while (i < nptr)
            {
                if (i != 1L && !hbits.morePointers())
                {
                    break; // end of object
                i++;
                }
                if (hbits.isPointer())
                {
                    tmpbuf[i / 8L] |= 1L << (int)((i % 8L));
                }
                hbits = hbits.next();
            }

            return new bitvector(int32(i),&tmpbuf[0]);
        }
    }
}
