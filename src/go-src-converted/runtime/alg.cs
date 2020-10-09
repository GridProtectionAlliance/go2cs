// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\alg.go
using cpu = go.@internal.cpu_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
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
            ptr<ptr<System.UIntPtr>> size = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(ptr + @unsafe.Sizeof(h)));
            return memhash(p, h, size);
        }

        // runtime variable to check if the processor we're running on
        // actually supports the instructions used by the AES-based
        // hash implementation.
        private static bool useAeshash = default;

        // in asm_*.s
        private static System.UIntPtr memhash(unsafe.Pointer p, System.UIntPtr h, System.UIntPtr s)
;
        private static System.UIntPtr memhash32(unsafe.Pointer p, System.UIntPtr h)
;
        private static System.UIntPtr memhash64(unsafe.Pointer p, System.UIntPtr h)
;
        private static System.UIntPtr strhash(unsafe.Pointer p, System.UIntPtr h)
;

        private static System.UIntPtr strhashFallback(unsafe.Pointer a, System.UIntPtr h)
        {
            var x = (stringStruct.val)(a);
            return memhashFallback(x.str, h, uintptr(x.len));
        }

        // NOTE: Because NaN != NaN, a map can contain any
        // number of (mostly useless) entries keyed with NaNs.
        // To avoid long hash chains, we assign a random number
        // as the hash value for a NaN.

        private static System.UIntPtr f32hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ptr<ptr<float>> f = new ptr<ptr<ptr<float>>>(p);

            if (f == 0L) 
                return c1 * (c0 ^ h); // +0, -0
            else if (f != f) 
                return c1 * (c0 ^ h ^ uintptr(fastrand())); // any kind of NaN
            else 
                return memhash(p, h, 4L);
            
        }

        private static System.UIntPtr f64hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ptr<ptr<double>> f = new ptr<ptr<ptr<double>>>(p);

            if (f == 0L) 
                return c1 * (c0 ^ h); // +0, -0
            else if (f != f) 
                return c1 * (c0 ^ h ^ uintptr(fastrand())); // any kind of NaN
            else 
                return memhash(p, h, 8L);
            
        }

        private static System.UIntPtr c64hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ptr<array<float>> x = new ptr<ptr<array<float>>>(p);
            return f32hash(@unsafe.Pointer(_addr_x[1L]), f32hash(@unsafe.Pointer(_addr_x[0L]), h));
        }

        private static System.UIntPtr c128hash(unsafe.Pointer p, System.UIntPtr h)
        {
            ptr<array<double>> x = new ptr<ptr<array<double>>>(p);
            return f64hash(@unsafe.Pointer(_addr_x[1L]), f64hash(@unsafe.Pointer(_addr_x[0L]), h));
        }

        private static System.UIntPtr interhash(unsafe.Pointer p, System.UIntPtr h) => func((_, panic, __) =>
        {
            var a = (iface.val)(p);
            var tab = a.tab;
            if (tab == null)
            {>>MARKER:FUNCTION_strhash_BLOCK_PREFIX<<
                return h;
            }

            var t = tab._type;
            if (t.equal == null)
            {>>MARKER:FUNCTION_memhash64_BLOCK_PREFIX<< 
                // Check hashability here. We could do this check inside
                // typehash, but we want to report the topmost type in
                // the error text (e.g. in a struct with a field of slice type
                // we want to report the struct, not the slice).
                panic(errorString("hash of unhashable type " + t.@string()));

            }

            if (isDirectIface(t))
            {>>MARKER:FUNCTION_memhash32_BLOCK_PREFIX<<
                return c1 * typehash(_addr_t, @unsafe.Pointer(_addr_a.data), h ^ c0);
            }
            else
            {>>MARKER:FUNCTION_memhash_BLOCK_PREFIX<<
                return c1 * typehash(_addr_t, a.data, h ^ c0);
            }

        });

        private static System.UIntPtr nilinterhash(unsafe.Pointer p, System.UIntPtr h) => func((_, panic, __) =>
        {
            var a = (eface.val)(p);
            var t = a._type;
            if (t == null)
            {
                return h;
            }

            if (t.equal == null)
            { 
                // See comment in interhash above.
                panic(errorString("hash of unhashable type " + t.@string()));

            }

            if (isDirectIface(t))
            {
                return c1 * typehash(_addr_t, @unsafe.Pointer(_addr_a.data), h ^ c0);
            }
            else
            {
                return c1 * typehash(_addr_t, a.data, h ^ c0);
            }

        });

        // typehash computes the hash of the object of type t at address p.
        // h is the seed.
        // This function is seldom used. Most maps use for hashing either
        // fixed functions (e.g. f32hash) or compiler-generated functions
        // (e.g. for a type like struct { x, y string }). This implementation
        // is slower but more general and is used for hashing interface types
        // (called from interhash or nilinterhash, above) or for hashing in
        // maps generated by reflect.MapOf (reflect_typehash, below).
        // Note: this function must match the compiler generated
        // functions exactly. See issue 37716.
        private static System.UIntPtr typehash(ptr<_type> _addr_t, unsafe.Pointer p, System.UIntPtr h) => func((_, panic, __) =>
        {
            ref _type t = ref _addr_t.val;

            if (t.tflag & tflagRegularMemory != 0L)
            { 
                // Handle ptr sizes specially, see issue 37086.
                switch (t.size)
                {
                    case 4L: 
                        return memhash32(p, h);
                        break;
                    case 8L: 
                        return memhash64(p, h);
                        break;
                    default: 
                        return memhash(p, h, t.size);
                        break;
                }

            }


            if (t.kind & kindMask == kindFloat32) 
                return f32hash(p, h);
            else if (t.kind & kindMask == kindFloat64) 
                return f64hash(p, h);
            else if (t.kind & kindMask == kindComplex64) 
                return c64hash(p, h);
            else if (t.kind & kindMask == kindComplex128) 
                return c128hash(p, h);
            else if (t.kind & kindMask == kindString) 
                return strhash(p, h);
            else if (t.kind & kindMask == kindInterface) 
                var i = (interfacetype.val)(@unsafe.Pointer(t));
                if (len(i.mhdr) == 0L)
                {
                    return nilinterhash(p, h);
                }

                return interhash(p, h);
            else if (t.kind & kindMask == kindArray) 
                var a = (arraytype.val)(@unsafe.Pointer(t));
                {
                    var i__prev1 = i;

                    for (i = uintptr(0L); i < a.len; i++)
                    {
                        h = typehash(_addr_a.elem, add(p, i * a.elem.size), h);
                    }


                    i = i__prev1;
                }
                return h;
            else if (t.kind & kindMask == kindStruct) 
                var s = (structtype.val)(@unsafe.Pointer(t));
                var memStart = uintptr(0L);
                var memEnd = uintptr(0L);
                foreach (var (_, f) in s.fields)
                {
                    if (memEnd > memStart && (f.name.isBlank() || f.offset() != memEnd || f.typ.tflag & tflagRegularMemory == 0L))
                    { 
                        // flush any pending regular memory hashing
                        h = memhash(add(p, memStart), h, memEnd - memStart);
                        memStart = memEnd;

                    }

                    if (f.name.isBlank())
                    {
                        continue;
                    }

                    if (f.typ.tflag & tflagRegularMemory == 0L)
                    {
                        h = typehash(_addr_f.typ, add(p, f.offset()), h);
                        continue;
                    }

                    if (memStart == memEnd)
                    {
                        memStart = f.offset();
                    }

                    memEnd = f.offset() + f.typ.size;

                }
                if (memEnd > memStart)
                {
                    h = memhash(add(p, memStart), h, memEnd - memStart);
                }

                return h;
            else 
                // Should never happen, as typehash should only be called
                // with comparable types.
                panic(errorString("hash of unhashable type " + t.@string()));
            
        });

        //go:linkname reflect_typehash reflect.typehash
        private static System.UIntPtr reflect_typehash(ptr<_type> _addr_t, unsafe.Pointer p, System.UIntPtr h)
        {
            ref _type t = ref _addr_t.val;

            return typehash(_addr_t, p, h);
        }

        private static bool memequal0(unsafe.Pointer p, unsafe.Pointer q)
        {
            return true;
        }
        private static bool memequal8(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<sbyte>>>(p) == new ptr<ptr<ptr<sbyte>>>(q);
        }
        private static bool memequal16(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<short>>>(p) == new ptr<ptr<ptr<short>>>(q);
        }
        private static bool memequal32(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<int>>>(p) == new ptr<ptr<ptr<int>>>(q);
        }
        private static bool memequal64(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<long>>>(p) == new ptr<ptr<ptr<long>>>(q);
        }
        private static bool memequal128(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<array<long>>>>(p) == new ptr<ptr<ptr<array<long>>>>(q);
        }
        private static bool f32equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<float>>>(p) == new ptr<ptr<ptr<float>>>(q);
        }
        private static bool f64equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<double>>>(p) == new ptr<ptr<ptr<double>>>(q);
        }
        private static bool c64equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<complex64>>>(p) == new ptr<ptr<ptr<complex64>>>(q);
        }
        private static bool c128equal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<System.Numerics.Complex128>>>(p) == new ptr<ptr<ptr<System.Numerics.Complex128>>>(q);
        }
        private static bool strequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            return new ptr<ptr<ptr<@string>>>(p) == new ptr<ptr<ptr<@string>>>(q);
        }
        private static bool interequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            ptr<ptr<iface>> x = new ptr<ptr<ptr<iface>>>(p);
            ptr<ptr<iface>> y = new ptr<ptr<ptr<iface>>>(q);
            return x.tab == y.tab && ifaceeq(_addr_x.tab, x.data, y.data);
        }
        private static bool nilinterequal(unsafe.Pointer p, unsafe.Pointer q)
        {
            ptr<ptr<eface>> x = new ptr<ptr<ptr<eface>>>(p);
            ptr<ptr<eface>> y = new ptr<ptr<ptr<eface>>>(q);
            return x._type == y._type && efaceeq(_addr_x._type, x.data, y.data);
        }
        private static bool efaceeq(ptr<_type> _addr_t, unsafe.Pointer x, unsafe.Pointer y) => func((_, panic, __) =>
        {
            ref _type t = ref _addr_t.val;

            if (t == null)
            {
                return true;
            }

            var eq = t.equal;
            if (eq == null)
            {
                panic(errorString("comparing uncomparable type " + t.@string()));
            }

            if (isDirectIface(t))
            { 
                // Direct interface types are ptr, chan, map, func, and single-element structs/arrays thereof.
                // Maps and funcs are not comparable, so they can't reach here.
                // Ptrs, chans, and single-element items can be compared directly using ==.
                return x == y;

            }

            return eq(x, y);

        });
        private static bool ifaceeq(ptr<itab> _addr_tab, unsafe.Pointer x, unsafe.Pointer y) => func((_, panic, __) =>
        {
            ref itab tab = ref _addr_tab.val;

            if (tab == null)
            {
                return true;
            }

            var t = tab._type;
            var eq = t.equal;
            if (eq == null)
            {
                panic(errorString("comparing uncomparable type " + t.@string()));
            }

            if (isDirectIface(t))
            { 
                // See comment in efaceeq.
                return x == y;

            }

            return eq(x, y);

        });

        // Testing adapters for hash quality tests (see hash_test.go)
        private static System.UIntPtr stringHash(@string s, System.UIntPtr seed)
        {
            return strhash(noescape(@unsafe.Pointer(_addr_s)), seed);
        }

        private static System.UIntPtr bytesHash(slice<byte> b, System.UIntPtr seed)
        {
            var s = (slice.val)(@unsafe.Pointer(_addr_b));
            return memhash(s.array, seed, uintptr(s.len));
        }

        private static System.UIntPtr int32Hash(uint i, System.UIntPtr seed)
        {
            return memhash32(noescape(@unsafe.Pointer(_addr_i)), seed);
        }

        private static System.UIntPtr int64Hash(ulong i, System.UIntPtr seed)
        {
            return memhash64(noescape(@unsafe.Pointer(_addr_i)), seed);
        }

        private static System.UIntPtr efaceHash(object i, System.UIntPtr seed)
        {
            return nilinterhash(noescape(@unsafe.Pointer(_addr_i)), seed);
        }

        private static System.UIntPtr ifaceHash(object i, System.UIntPtr seed)
        {
            return interhash(noescape(@unsafe.Pointer(_addr_i)), seed);
        }

        private static readonly var hashRandomBytes = sys.PtrSize / 4L * 64L;

        // used in asm_{386,amd64,arm64}.s to seed the hash function


        // used in asm_{386,amd64,arm64}.s to seed the hash function
        private static array<byte> aeskeysched = new array<byte>(hashRandomBytes);

        // used in hash{32,64}.go to seed the hash function
        private static array<System.UIntPtr> hashkey = new array<System.UIntPtr>(4L);

        private static void alginit()
        { 
            // Install AES hash algorithms if the instructions needed are present.
            if ((GOARCH == "386" || GOARCH == "amd64") && cpu.X86.HasAES && cpu.X86.HasSSSE3 && cpu.X86.HasSSE41)
            { // PINSR{D,Q}
                initAlgAES();
                return ;

            }

            if (GOARCH == "arm64" && cpu.ARM64.HasAES)
            {
                initAlgAES();
                return ;
            }

            getRandomData(new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_hashkey))[..]);
            hashkey[0L] |= 1L; // make sure these numbers are odd
            hashkey[1L] |= 1L;
            hashkey[2L] |= 1L;
            hashkey[3L] |= 1L;

        }

        private static void initAlgAES()
        {
            useAeshash = true; 
            // Initialize with random data so hash collisions will be hard to engineer.
            getRandomData(aeskeysched[..]);

        }

        // Note: These routines perform the read with a native endianness.
        private static uint readUnaligned32(unsafe.Pointer p)
        {
            ptr<array<byte>> q = new ptr<ptr<array<byte>>>(p);
            if (sys.BigEndian)
            {
                return uint32(q[3L]) | uint32(q[2L]) << (int)(8L) | uint32(q[1L]) << (int)(16L) | uint32(q[0L]) << (int)(24L);
            }

            return uint32(q[0L]) | uint32(q[1L]) << (int)(8L) | uint32(q[2L]) << (int)(16L) | uint32(q[3L]) << (int)(24L);

        }

        private static ulong readUnaligned64(unsafe.Pointer p)
        {
            ptr<array<byte>> q = new ptr<ptr<array<byte>>>(p);
            if (sys.BigEndian)
            {
                return uint64(q[7L]) | uint64(q[6L]) << (int)(8L) | uint64(q[5L]) << (int)(16L) | uint64(q[4L]) << (int)(24L) | uint64(q[3L]) << (int)(32L) | uint64(q[2L]) << (int)(40L) | uint64(q[1L]) << (int)(48L) | uint64(q[0L]) << (int)(56L);
            }

            return uint64(q[0L]) | uint64(q[1L]) << (int)(8L) | uint64(q[2L]) << (int)(16L) | uint64(q[3L]) << (int)(24L) | uint64(q[4L]) << (int)(32L) | uint64(q[5L]) << (int)(40L) | uint64(q[6L]) << (int)(48L) | uint64(q[7L]) << (int)(56L);

        }
    }
}
