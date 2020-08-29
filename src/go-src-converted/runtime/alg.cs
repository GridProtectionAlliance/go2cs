// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\alg.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly var c0 = uintptr((8L - sys.PtrSize) / 4L * 2860486313L + (sys.PtrSize - 4L) / 4L * 33054211828000289L);
        private static readonly var c1 = uintptr((8L - sys.PtrSize) / 4L * 3267000013L + (sys.PtrSize - 4L) / 4L * 23344194077549503L);

        // type algorithms - known to compiler
        private static readonly var alg_NOEQ = iota;
        private static readonly var alg_MEM0 = 0;
        private static readonly var alg_MEM8 = 1;
        private static readonly var alg_MEM16 = 2;
        private static readonly var alg_MEM32 = 3;
        private static readonly var alg_MEM64 = 4;
        private static readonly var alg_MEM128 = 5;
        private static readonly var alg_STRING = 6;
        private static readonly var alg_INTER = 7;
        private static readonly var alg_NILINTER = 8;
        private static readonly var alg_FLOAT32 = 9;
        private static readonly var alg_FLOAT64 = 10;
        private static readonly var alg_CPLX64 = 11;
        private static readonly var alg_CPLX128 = 12;
        private static readonly var alg_max = 13;

        // typeAlg is also copied/used in reflect/type.go.
        // keep them in sync.
        private partial struct typeAlg
        {
            public Func<unsafe.Pointer, System.UIntPtr, System.UIntPtr> hash; // function for comparing objects of this type
// (ptr to object A, ptr to object B) -> ==?
            public Func<unsafe.Pointer, unsafe.Pointer, bool> equal;
        }

        private static System.UIntPtr memhash0(unsafe.Pointer p, System.UIntPtr h)
        {
            return h;
        }

        private static System.UIntPtr memhash8(unsafe.Pointer p, System.UIntPtr h)
        {
            return memhash(p, h, 1L);
        }

        private static System.UIntPtr memhash16(unsafe.Pointer p, System.UIntPtr h)
        {
            return memhash(p, h, 2L);
        }

        private static System.UIntPtr memhash128(unsafe.Pointer p, System.UIntPtr h)
        {
            return memhash(p, h, 16L);
        }

        //go:nosplit
        private static System.UIntPtr memhash_varlen(unsafe.Pointer p, System.UIntPtr h)
        {
            var ptr = getclosureptr();
            *(*System.UIntPtr) size = @unsafe.Pointer(ptr + @unsafe.Sizeof(h)).Value;
            return memhash(p, h, size);
        }

        private static array<typeAlg> algarray = new array<typeAlg>(InitKeyedValues<typeAlg>(alg_max, (alg_NOEQ, {nil,nil}), (alg_MEM0, {memhash0,memequal0}), (alg_MEM8, {memhash8,memequal8}), (alg_MEM16, {memhash16,memequal16}), (alg_MEM32, {memhash32,memequal32}), (alg_MEM64, {memhash64,memequal64}), (alg_MEM128, {memhash128,memequal128}), (alg_STRING, {strhash,strequal}), (alg_INTER, {interhash,interequal}), (alg_NILINTER, {nilinterhash,nilinterequal}), (alg_FLOAT32, {f32hash,f32equal}), (alg_FLOAT64, {f64hash,f64equal}), (alg_CPLX64, {c64hash,c64equal}), (alg_CPLX128, {c128hash,c128equal})));

        private static bool useAeshash = default;

        // in asm_*.s
        private static System.UIntPtr aeshash(unsafe.Pointer p, System.UIntPtr h, System.UIntPtr s)
;
        private static System.UIntPtr aeshash32(unsafe.Pointer p, System.UIntPtr h)
;
        private static System.UIntPtr aeshash64(unsafe.Pointer p, System.UIntPtr h)
;
        private static System.UIntPtr aeshashstr(unsafe.Pointer p, System.UIntPtr h)
;

        private static System.UIntPtr strhash(unsafe.Pointer a, System.UIntPtr h)
        {
            var x = (stringStruct.Value)(a);
            return memhash(x.str, h, uintptr(x.len));
        }

        // NOTE: Because NaN != NaN, a map can contain any
        // number of (mostly useless) entries keyed with NaNs.
        // To avoid long hash chains, we assign a random number
        // as the hash value for a NaN.

        private static System.UIntPtr f32hash(unsafe.Pointer p, System.UIntPtr h)
        {
            *(*float) f = p.Value;

            if (f == 0L) 
                return c1 * (c0 ^ h); // +0, -0
            else if (f != f) 
                return c1 * (c0 ^ h ^ uintptr(fastrand())); // any kind of NaN
            else 
                return memhash(p, h, 4L);
                    }

        private static System.UIntPtr f64hash(unsafe.Pointer p, System.UIntPtr h)
        {
            *(*double) f = p.Value;

            if (f == 0L) 
                return c1 * (c0 ^ h); // +0, -0
            else if (f != f) 
                return c1 * (c0 ^ h ^ uintptr(fastrand())); // any kind of NaN
            else 
                return memhash(p, h, 8L);
                    }

        private static System.UIntPtr c64hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ref array<float> x = new ptr<ref array<float>>(p);
            return f32hash(@unsafe.Pointer(ref x[1L]), f32hash(@unsafe.Pointer(ref x[0L]), h));
        }

        private static System.UIntPtr c128hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ref array<double> x = new ptr<ref array<double>>(p);
            return f64hash(@unsafe.Pointer(ref x[1L]), f64hash(@unsafe.Pointer(ref x[0L]), h));
        }

        private static System.UIntPtr interhash(unsafe.Pointer p, System.UIntPtr h) => func((_, panic, __) =>
        {
            var a = (iface.Value)(p);
            var tab = a.tab;
            if (tab == null)
            {>>MARKER:FUNCTION_aeshashstr_BLOCK_PREFIX<<
                return h;
            }
            var t = tab._type;
            var fn = t.alg.hash;
            if (fn == null)
            {>>MARKER:FUNCTION_aeshash64_BLOCK_PREFIX<<
                panic(errorString("hash of unhashable type " + t.@string()));
            }
            if (isDirectIface(t))
            {>>MARKER:FUNCTION_aeshash32_BLOCK_PREFIX<<
                return c1 * fn(@unsafe.Pointer(ref a.data), h ^ c0);
            }
            else
            {>>MARKER:FUNCTION_aeshash_BLOCK_PREFIX<<
                return c1 * fn(a.data, h ^ c0);
            }
        });

        private static System.UIntPtr nilinterhash(unsafe.Pointer p, System.UIntPtr h) => func((_, panic, __) =>
        {
            var a = (eface.Value)(p);
            var t = a._type;
            if (t == null)
            {
                return h;
            }
            var fn = t.alg.hash;
            if (fn == null)
            {
                panic(errorString("hash of unhashable type " + t.@string()));
            }
            if (isDirectIface(t))
            {
                return c1 * fn(@unsafe.Pointer(ref a.data), h ^ c0);
            }
            else
            {
                return c1 * fn(a.data, h ^ c0);
            }
        });

        private static bool memequal0(unsafe.Pointer p, unsafe.Pointer q)
        {
            return true;
        }
        private static bool memequal8(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool memequal16(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool memequal32(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool memequal64(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool memequal128(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool f32equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool f64equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool c64equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool c128equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool strequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return p.Value == q.Value;
        }
        private static bool interequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            *(*iface) x = p.Value;
            *(*iface) y = q.Value;
            return x.tab == y.tab && ifaceeq(x.tab, x.data, y.data);
        }
        private static bool nilinterequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            *(*eface) x = p.Value;
            *(*eface) y = q.Value;
            return x._type == y._type && efaceeq(x._type, x.data, y.data);
        }
        private static bool efaceeq(ref _type _t, unsafe.Pointer x, unsafe.Pointer y) => func(_t, (ref _type t, Defer _, Panic panic, Recover __) =>
        {
            if (t == null)
            {
                return true;
            }
            var eq = t.alg.equal;
            if (eq == null)
            {
                panic(errorString("comparing uncomparable type " + t.@string()));
            }
            if (isDirectIface(t))
            {
                return eq(noescape(@unsafe.Pointer(ref x)), noescape(@unsafe.Pointer(ref y)));
            }
            return eq(x, y);
        });
        private static bool ifaceeq(ref itab _tab, unsafe.Pointer x, unsafe.Pointer y) => func(_tab, (ref itab tab, Defer _, Panic panic, Recover __) =>
        {
            if (tab == null)
            {
                return true;
            }
            var t = tab._type;
            var eq = t.alg.equal;
            if (eq == null)
            {
                panic(errorString("comparing uncomparable type " + t.@string()));
            }
            if (isDirectIface(t))
            {
                return eq(noescape(@unsafe.Pointer(ref x)), noescape(@unsafe.Pointer(ref y)));
            }
            return eq(x, y);
        });

        // Testing adapters for hash quality tests (see hash_test.go)
        private static System.UIntPtr stringHash(@string s, System.UIntPtr seed)
        {
            return algarray[alg_STRING].hash(noescape(@unsafe.Pointer(ref s)), seed);
        }

        private static System.UIntPtr bytesHash(slice<byte> b, System.UIntPtr seed)
        {
            var s = (slice.Value)(@unsafe.Pointer(ref b));
            return memhash(s.array, seed, uintptr(s.len));
        }

        private static System.UIntPtr int32Hash(uint i, System.UIntPtr seed)
        {
            return algarray[alg_MEM32].hash(noescape(@unsafe.Pointer(ref i)), seed);
        }

        private static System.UIntPtr int64Hash(ulong i, System.UIntPtr seed)
        {
            return algarray[alg_MEM64].hash(noescape(@unsafe.Pointer(ref i)), seed);
        }

        private static System.UIntPtr efaceHash(object i, System.UIntPtr seed)
        {
            return algarray[alg_NILINTER].hash(noescape(@unsafe.Pointer(ref i)), seed);
        }

        private static System.UIntPtr ifaceHash(object i, System.UIntPtr seed)
        {
            return algarray[alg_INTER].hash(noescape(@unsafe.Pointer(ref i)), seed);
        }

        private static readonly var hashRandomBytes = sys.PtrSize / 4L * 64L;

        // used in asm_{386,amd64}.s to seed the hash function


        // used in asm_{386,amd64}.s to seed the hash function
        private static array<byte> aeskeysched = new array<byte>(hashRandomBytes);

        // used in hash{32,64}.go to seed the hash function
        private static array<System.UIntPtr> hashkey = new array<System.UIntPtr>(4L);

        private static void alginit()
        { 
            // Install aes hash algorithm if we have the instructions we need
            if ((GOARCH == "386" || GOARCH == "amd64") && GOOS != "nacl" && support_aes && support_ssse3 && support_sse41)
            { // PINSR{D,Q}
                useAeshash = true;
                algarray[alg_MEM32].hash = aeshash32;
                algarray[alg_MEM64].hash = aeshash64;
                algarray[alg_STRING].hash = aeshashstr; 
                // Initialize with random data so hash collisions will be hard to engineer.
                getRandomData(aeskeysched[..]);
                return;
            }
            getRandomData(new ptr<ref array<byte>>(@unsafe.Pointer(ref hashkey))[..]);
            hashkey[0L] |= 1L; // make sure these numbers are odd
            hashkey[1L] |= 1L;
            hashkey[2L] |= 1L;
            hashkey[3L] |= 1L;
        }
    }
}
