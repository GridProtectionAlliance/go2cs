// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\runtime1.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Keep a cached value to make gotraceback fast,
        // since we call it on every call to gentraceback.
        // The cached value is a uint32 in which the low bits
        // are the "crash" and "all" settings and the remaining
        // bits are the traceback value (0 off, 1 on, 2 include system).
        private static readonly long tracebackCrash = (long)1L << (int)(iota);
        private static readonly tracebackShift tracebackAll = (tracebackShift)iota;


        private static uint traceback_cache = 2L << (int)(tracebackShift);
        private static uint traceback_env = default;

        // gotraceback returns the current traceback settings.
        //
        // If level is 0, suppress all tracebacks.
        // If level is 1, show tracebacks, but exclude runtime frames.
        // If level is 2, show tracebacks including runtime frames.
        // If all is set, print all goroutine stacks. Otherwise, print just the current goroutine.
        // If crash is set, crash (core dump, etc) after tracebacking.
        //
        //go:nosplit
        private static (int, bool, bool) gotraceback()
        {
            int level = default;
            bool all = default;
            bool crash = default;

            var _g_ = getg();
            var t = atomic.Load(_addr_traceback_cache);
            crash = t & tracebackCrash != 0L;
            all = _g_.m.throwing > 0L || t & tracebackAll != 0L;
            if (_g_.m.traceback != 0L)
            {
                level = int32(_g_.m.traceback);
            }
            else
            {
                level = int32(t >> (int)(tracebackShift));
            }

            return ;

        }

        private static int argc = default;        private static ptr<ptr<byte>> argv;

        // nosplit for use in linux startup sysargs
        //go:nosplit
        private static ptr<byte> argv_index(ptr<ptr<byte>> _addr_argv, int i)
        {
            ref ptr<byte> argv = ref _addr_argv.val;

            return new ptr<ptr<ptr<ptr<byte>>>>(add(@unsafe.Pointer(argv), uintptr(i) * sys.PtrSize));
        }

        private static void args(int c, ptr<ptr<byte>> _addr_v)
        {
            ref ptr<byte> v = ref _addr_v.val;

            argc = c;
            argv = v;
            sysargs(c, v);
        }

        private static void goargs()
        {
            if (GOOS == "windows")
            {
                return ;
            }

            argslice = make_slice<@string>(argc);
            for (var i = int32(0L); i < argc; i++)
            {
                argslice[i] = gostringnocopy(argv_index(_addr_argv, i));
            }


        }

        private static void goenvs_unix()
        { 
            // TODO(austin): ppc64 in dynamic linking mode doesn't
            // guarantee env[] will immediately follow argv. Might cause
            // problems.
            var n = int32(0L);
            while (argv_index(_addr_argv, argc + 1L + n) != null)
            {
                n++;
            }


            envs = make_slice<@string>(n);
            for (var i = int32(0L); i < n; i++)
            {
                envs[i] = gostring(argv_index(_addr_argv, argc + 1L + i));
            }


        }

        private static slice<@string> environ()
        {
            return envs;
        }

        // TODO: These should be locals in testAtomic64, but we don't 8-byte
        // align stack variables on 386.
        private static ulong test_z64 = default;        private static ulong test_x64 = default;



        private static void testAtomic64()
        {
            test_z64 = 42L;
            test_x64 = 0L;
            if (atomic.Cas64(_addr_test_z64, test_x64, 1L))
            {
                throw("cas64 failed");
            }

            if (test_x64 != 0L)
            {
                throw("cas64 failed");
            }

            test_x64 = 42L;
            if (!atomic.Cas64(_addr_test_z64, test_x64, 1L))
            {
                throw("cas64 failed");
            }

            if (test_x64 != 42L || test_z64 != 1L)
            {
                throw("cas64 failed");
            }

            if (atomic.Load64(_addr_test_z64) != 1L)
            {
                throw("load64 failed");
            }

            atomic.Store64(_addr_test_z64, (1L << (int)(40L)) + 1L);
            if (atomic.Load64(_addr_test_z64) != (1L << (int)(40L)) + 1L)
            {
                throw("store64 failed");
            }

            if (atomic.Xadd64(_addr_test_z64, (1L << (int)(40L)) + 1L) != (2L << (int)(40L)) + 2L)
            {
                throw("xadd64 failed");
            }

            if (atomic.Load64(_addr_test_z64) != (2L << (int)(40L)) + 2L)
            {
                throw("xadd64 failed");
            }

            if (atomic.Xchg64(_addr_test_z64, (3L << (int)(40L)) + 3L) != (2L << (int)(40L)) + 2L)
            {
                throw("xchg64 failed");
            }

            if (atomic.Load64(_addr_test_z64) != (3L << (int)(40L)) + 3L)
            {
                throw("xchg64 failed");
            }

        }

        private static void check()
        {
            sbyte a = default;            byte b = default;            short c = default;            ushort d = default;            ref int e = ref heap(out ptr<int> _addr_e);            uint f = default;            long g = default;            ulong h = default;            ref float i = ref heap(out ptr<float> _addr_i);            ref float i1 = ref heap(out ptr<float> _addr_i1);
            ref double j = ref heap(out ptr<double> _addr_j);            ref double j1 = ref heap(out ptr<double> _addr_j1);
            unsafe.Pointer k = default;            ptr<ushort> l;            array<byte> m = new array<byte>(4L);
            private partial struct x1t
            {
                public byte x;
            }
            private partial struct y1t
            {
                public x1t x1;
                public byte y;
            }
            x1t x1 = default;
            y1t y1 = default;

            if (@unsafe.Sizeof(a) != 1L)
            {
                throw("bad a");
            }

            if (@unsafe.Sizeof(b) != 1L)
            {
                throw("bad b");
            }

            if (@unsafe.Sizeof(c) != 2L)
            {
                throw("bad c");
            }

            if (@unsafe.Sizeof(d) != 2L)
            {
                throw("bad d");
            }

            if (@unsafe.Sizeof(e) != 4L)
            {
                throw("bad e");
            }

            if (@unsafe.Sizeof(f) != 4L)
            {
                throw("bad f");
            }

            if (@unsafe.Sizeof(g) != 8L)
            {
                throw("bad g");
            }

            if (@unsafe.Sizeof(h) != 8L)
            {
                throw("bad h");
            }

            if (@unsafe.Sizeof(i) != 4L)
            {
                throw("bad i");
            }

            if (@unsafe.Sizeof(j) != 8L)
            {
                throw("bad j");
            }

            if (@unsafe.Sizeof(k) != sys.PtrSize)
            {
                throw("bad k");
            }

            if (@unsafe.Sizeof(l) != sys.PtrSize)
            {
                throw("bad l");
            }

            if (@unsafe.Sizeof(x1) != 1L)
            {
                throw("bad unsafe.Sizeof x1");
            }

            if (@unsafe.Offsetof(y1.y) != 1L)
            {
                throw("bad offsetof y1.y");
            }

            if (@unsafe.Sizeof(y1) != 2L)
            {
                throw("bad unsafe.Sizeof y1");
            }

            if (timediv(12345L * 1000000000L + 54321L, 1000000000L, _addr_e) != 12345L || e != 54321L)
            {
                throw("bad timediv");
            }

            ref uint z = ref heap(out ptr<uint> _addr_z);
            z = 1L;
            if (!atomic.Cas(_addr_z, 1L, 2L))
            {
                throw("cas1");
            }

            if (z != 2L)
            {
                throw("cas2");
            }

            z = 4L;
            if (atomic.Cas(_addr_z, 5L, 6L))
            {
                throw("cas3");
            }

            if (z != 4L)
            {
                throw("cas4");
            }

            z = 0xffffffffUL;
            if (!atomic.Cas(_addr_z, 0xffffffffUL, 0xfffffffeUL))
            {
                throw("cas5");
            }

            if (z != 0xfffffffeUL)
            {
                throw("cas6");
            }

            m = new array<byte>(new byte[] { 1, 1, 1, 1 });
            atomic.Or8(_addr_m[1L], 0xf0UL);
            if (m[0L] != 1L || m[1L] != 0xf1UL || m[2L] != 1L || m[3L] != 1L)
            {
                throw("atomicor8");
            }

            m = new array<byte>(new byte[] { 0xff, 0xff, 0xff, 0xff });
            atomic.And8(_addr_m[1L], 0x1UL);
            if (m[0L] != 0xffUL || m[1L] != 0x1UL || m[2L] != 0xffUL || m[3L] != 0xffUL)
            {
                throw("atomicand8");
            }

            (uint64.val)(@unsafe.Pointer(_addr_j)).val;

            ~uint64(0L);
            if (j == j)
            {
                throw("float64nan");
            }

            if (!(j != j))
            {
                throw("float64nan1");
            }

            (uint64.val)(@unsafe.Pointer(_addr_j1)).val;

            ~uint64(1L);
            if (j == j1)
            {
                throw("float64nan2");
            }

            if (!(j != j1))
            {
                throw("float64nan3");
            }

            (uint32.val)(@unsafe.Pointer(_addr_i)).val;

            ~uint32(0L);
            if (i == i)
            {
                throw("float32nan");
            }

            if (i == i)
            {
                throw("float32nan1");
            }

            (uint32.val)(@unsafe.Pointer(_addr_i1)).val;

            ~uint32(1L);
            if (i == i1)
            {
                throw("float32nan2");
            }

            if (i == i1)
            {
                throw("float32nan3");
            }

            testAtomic64();

            if (_FixedStack != round2(_FixedStack))
            {
                throw("FixedStack is not power-of-2");
            }

            if (!checkASM())
            {
                throw("assembly checks failed");
            }

        }

        private partial struct dbgVar
        {
            public @string name;
            public ptr<int> value;
        }

        // Holds variables parsed from GODEBUG env var,
        // except for "memprofilerate" since there is an
        // existing int var for that value, which may
        // already have an initial value.
        private static var debug = default;

        private static dbgVar dbgvars = new slice<dbgVar>(new dbgVar[] { {"allocfreetrace",&debug.allocfreetrace}, {"clobberfree",&debug.clobberfree}, {"cgocheck",&debug.cgocheck}, {"efence",&debug.efence}, {"gccheckmark",&debug.gccheckmark}, {"gcpacertrace",&debug.gcpacertrace}, {"gcshrinkstackoff",&debug.gcshrinkstackoff}, {"gcstoptheworld",&debug.gcstoptheworld}, {"gctrace",&debug.gctrace}, {"invalidptr",&debug.invalidptr}, {"madvdontneed",&debug.madvdontneed}, {"sbrk",&debug.sbrk}, {"scavenge",&debug.scavenge}, {"scavtrace",&debug.scavtrace}, {"scheddetail",&debug.scheddetail}, {"schedtrace",&debug.schedtrace}, {"tracebackancestors",&debug.tracebackancestors}, {"asyncpreemptoff",&debug.asyncpreemptoff} });

        private static void parsedebugvars()
        { 
            // defaults
            debug.cgocheck = 1L;
            debug.invalidptr = 1L;

            {
                var p = gogetenv("GODEBUG");

                while (p != "")
                {
                    @string field = "";
                    var i = index(p, ",");
                    if (i < 0L)
                    {
                        field = p;
                        p = "";

                    }
                    else
                    {
                        field = p[..i];
                        p = p[i + 1L..];

                    }

                    i = index(field, "=");
                    if (i < 0L)
                    {
                        continue;
                    }

                    var key = field[..i];
                    var value = field[i + 1L..]; 

                    // Update MemProfileRate directly here since it
                    // is int, not int32, and should only be updated
                    // if specified in GODEBUG.
                    if (key == "memprofilerate")
                    {
                        {
                            var n__prev2 = n;

                            var (n, ok) = atoi(value);

                            if (ok)
                            {
                                MemProfileRate = n;
                            }

                            n = n__prev2;

                        }

                    }
                    else
                    {
                        foreach (var (_, v) in dbgvars)
                        {
                            if (v.name == key)
                            {
                                {
                                    var n__prev3 = n;

                                    (n, ok) = atoi32(value);

                                    if (ok)
                                    {
                                        v.value.val = n;
                                    }

                                    n = n__prev3;

                                }

                            }

                        }

                    }

                }

            }

            setTraceback(gogetenv("GOTRACEBACK"));
            traceback_env = traceback_cache;

        }

        //go:linkname setTraceback runtime/debug.SetTraceback
        private static void setTraceback(@string level)
        {
            uint t = default;
            switch (level)
            {
                case "none": 
                    t = 0L;
                    break;
                case "single": 

                case "": 
                    t = 1L << (int)(tracebackShift);
                    break;
                case "all": 
                    t = 1L << (int)(tracebackShift) | tracebackAll;
                    break;
                case "system": 
                    t = 2L << (int)(tracebackShift) | tracebackAll;
                    break;
                case "crash": 
                    t = 2L << (int)(tracebackShift) | tracebackAll | tracebackCrash;
                    break;
                default: 
                    t = tracebackAll;
                    {
                        var (n, ok) = atoi(level);

                        if (ok && n == int(uint32(n)))
                        {
                            t |= uint32(n) << (int)(tracebackShift);
                        }

                    }

                    break;
            } 
            // when C owns the process, simply exit'ing the process on fatal errors
            // and panics is surprising. Be louder and abort instead.
            if (islibrary || isarchive)
            {
                t |= tracebackCrash;
            }

            t |= traceback_env;

            atomic.Store(_addr_traceback_cache, t);

        }

        // Poor mans 64-bit division.
        // This is a very special function, do not use it if you are not sure what you are doing.
        // int64 division is lowered into _divv() call on 386, which does not fit into nosplit functions.
        // Handles overflow in a time-specific manner.
        // This keeps us within no-split stack limits on 32-bit processors.
        //go:nosplit
        private static int timediv(long v, int div, ptr<int> _addr_rem)
        {
            ref int rem = ref _addr_rem.val;

            var res = int32(0L);
            for (long bit = 30L; bit >= 0L; bit--)
            {
                if (v >= int64(div) << (int)(uint(bit)))
                {
                    v = v - (int64(div) << (int)(uint(bit))); 
                    // Before this for loop, res was 0, thus all these
                    // power of 2 increments are now just bitsets.
                    res |= 1L << (int)(uint(bit));

                }

            }

            if (v >= int64(div))
            {
                if (rem != null)
                {
                    rem = 0L;
                }

                return 0x7fffffffUL;

            }

            if (rem != null)
            {
                rem = int32(v);
            }

            return res;

        }

        // Helpers for Go. Must be NOSPLIT, must only call NOSPLIT functions, and must not block.

        //go:nosplit
        private static ptr<m> acquirem()
        {
            var _g_ = getg();
            _g_.m.locks++;
            return _addr__g_.m!;
        }

        //go:nosplit
        private static void releasem(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            var _g_ = getg();
            mp.locks--;
            if (mp.locks == 0L && _g_.preempt)
            { 
                // restore the preemption request in case we've cleared it in newstack
                _g_.stackguard0 = stackPreempt;

            }

        }

        //go:linkname reflect_typelinks reflect.typelinks
        private static (slice<unsafe.Pointer>, slice<slice<int>>) reflect_typelinks()
        {
            slice<unsafe.Pointer> _p0 = default;
            slice<slice<int>> _p0 = default;

            var modules = activeModules();
            unsafe.Pointer sections = new slice<unsafe.Pointer>(new unsafe.Pointer[] { unsafe.Pointer(modules[0].types) });
            slice<int> ret = new slice<slice<int>>(new slice<int>[] { modules[0].typelinks });
            foreach (var (_, md) in modules[1L..])
            {
                sections = append(sections, @unsafe.Pointer(md.types));
                ret = append(ret, md.typelinks);
            }
            return (sections, ret);

        }

        // reflect_resolveNameOff resolves a name offset from a base pointer.
        //go:linkname reflect_resolveNameOff reflect.resolveNameOff
        private static unsafe.Pointer reflect_resolveNameOff(unsafe.Pointer ptrInModule, int off)
        {
            return @unsafe.Pointer(resolveNameOff(ptrInModule, nameOff(off)).bytes);
        }

        // reflect_resolveTypeOff resolves an *rtype offset from a base type.
        //go:linkname reflect_resolveTypeOff reflect.resolveTypeOff
        private static unsafe.Pointer reflect_resolveTypeOff(unsafe.Pointer rtype, int off)
        {
            return @unsafe.Pointer((_type.val)(rtype).typeOff(typeOff(off)));
        }

        // reflect_resolveTextOff resolves a function pointer offset from a base type.
        //go:linkname reflect_resolveTextOff reflect.resolveTextOff
        private static unsafe.Pointer reflect_resolveTextOff(unsafe.Pointer rtype, int off)
        {
            return (_type.val)(rtype).textOff(textOff(off));
        }

        // reflectlite_resolveNameOff resolves a name offset from a base pointer.
        //go:linkname reflectlite_resolveNameOff internal/reflectlite.resolveNameOff
        private static unsafe.Pointer reflectlite_resolveNameOff(unsafe.Pointer ptrInModule, int off)
        {
            return @unsafe.Pointer(resolveNameOff(ptrInModule, nameOff(off)).bytes);
        }

        // reflectlite_resolveTypeOff resolves an *rtype offset from a base type.
        //go:linkname reflectlite_resolveTypeOff internal/reflectlite.resolveTypeOff
        private static unsafe.Pointer reflectlite_resolveTypeOff(unsafe.Pointer rtype, int off)
        {
            return @unsafe.Pointer((_type.val)(rtype).typeOff(typeOff(off)));
        }

        // reflect_addReflectOff adds a pointer to the reflection offset lookup map.
        //go:linkname reflect_addReflectOff reflect.addReflectOff
        private static int reflect_addReflectOff(unsafe.Pointer ptr)
        {
            reflectOffsLock();
            if (reflectOffs.m == null)
            {
                reflectOffs.m = make_map<int, unsafe.Pointer>();
                reflectOffs.minv = make_map<unsafe.Pointer, int>();
                reflectOffs.next = -1L;
            }

            var (id, found) = reflectOffs.minv[ptr];
            if (!found)
            {
                id = reflectOffs.next;
                reflectOffs.next--; // use negative offsets as IDs to aid debugging
                reflectOffs.m[id] = ptr;
                reflectOffs.minv[ptr] = id;

            }

            reflectOffsUnlock();
            return id;

        }
    }
}
