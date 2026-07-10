// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static readonly uintptr c0 = /* uintptr((8-goarch.PtrSize)/4*2860486313 + (goarch.PtrSize-4)/4*33054211828000289) */ unchecked((uintptr)33054211828000289);
internal static readonly uintptr c1 = /* uintptr((8-goarch.PtrSize)/4*3267000013 + (goarch.PtrSize-4)/4*23344194077549503) */ unchecked((uintptr)23344194077549503);

internal static uintptr memhash0(@unsafe.Pointer Δp, uintptr h) {
    return h;
}

internal static uintptr memhash8(@unsafe.Pointer Δp, uintptr h) {
    return memhash(Δp, h, 1);
}

internal static uintptr memhash16(@unsafe.Pointer Δp, uintptr h) {
    return memhash(Δp, h, 2);
}

internal static uintptr memhash128(@unsafe.Pointer Δp, uintptr h) {
    return memhash(Δp, h, 16);
}

//go:nosplit
internal static uintptr memhash_varlen(@unsafe.Pointer Δp, uintptr h) {
    var ptr = getclosureptr();
    var size = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(ptr + @unsafe.Sizeof(h)));
    return memhash(Δp, h, size);
}

// runtime variable to check if the processor we're running on
// actually supports the instructions used by the AES-based
// hash implementation.
internal static bool useAeshash;

// in asm_*.s

// memhash should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/aacfactory/fns
//   - github.com/dgraph-io/ristretto
//   - github.com/minio/simdjson-go
//   - github.com/nbd-wtf/go-nostr
//   - github.com/outcaste-io/ristretto
//   - github.com/puzpuzpuz/xsync/v2
//   - github.com/puzpuzpuz/xsync/v3
//   - github.com/segmentio/parquet-go
//   - github.com/parquet-go/parquet-go
//   - github.com/authzed/spicedb
//   - github.com/pingcap/badger
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memhash
internal static partial uintptr memhash(@unsafe.Pointer Δp, uintptr h, uintptr s);

// memhash32 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/segmentio/parquet-go
//   - github.com/parquet-go/parquet-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memhash32
internal static partial uintptr memhash32(@unsafe.Pointer Δp, uintptr h);

// memhash64 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/segmentio/parquet-go
//   - github.com/parquet-go/parquet-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memhash64
internal static partial uintptr memhash64(@unsafe.Pointer Δp, uintptr h);

// strhash should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/aristanetworks/goarista
//   - github.com/bytedance/sonic
//   - github.com/bytedance/go-tagexpr/v2
//   - github.com/cloudwego/frugal
//   - github.com/cloudwego/dynamicgo
//   - github.com/v2fly/v2ray-core/v5
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname strhash
internal static partial uintptr strhash(@unsafe.Pointer Δp, uintptr h);

internal static uintptr strhashFallback(@unsafe.Pointer a, uintptr h) {
    var x = (ж<stringStruct>)(uintptr)(a);
    return memhashFallback((~x).str, h, (uintptr)(~x).len);
}

// NOTE: Because NaN != NaN, a map can contain any
// number of (mostly useless) entries keyed with NaNs.
// To avoid long hash chains, we assign a random number
// as the hash value for a NaN.
internal static uintptr f32hash(@unsafe.Pointer Δp, uintptr h) {
    var f = ~(ж<float32>)(uintptr)(Δp);
    switch (ᐧ) {
    case {} when f is 0: {
        return c1 * ((uintptr)(c0 ^ h));
    }
    case {} when f != f: {
        return c1 * ((uintptr)((uintptr)(c0 ^ h) ^ (uintptr)rand()));
    }
    default: {
        return memhash(Δp, // +0, -0
 // any kind of NaN
 h, 4);
    }}

}

internal static uintptr f64hash(@unsafe.Pointer Δp, uintptr h) {
    var f = ~(ж<float64>)(uintptr)(Δp);
    switch (ᐧ) {
    case {} when f is 0: {
        return c1 * ((uintptr)(c0 ^ h));
    }
    case {} when f != f: {
        return c1 * ((uintptr)((uintptr)(c0 ^ h) ^ (uintptr)rand()));
    }
    default: {
        return memhash(Δp, // +0, -0
 // any kind of NaN
 h, 8);
    }}

}

internal static uintptr c64hash(@unsafe.Pointer Δp, uintptr h) {
    var x = (ж<array<float32>>)(uintptr)(Δp);
    return f32hash(new @unsafe.Pointer(Ꮡ(x.Value[1])), f32hash(new @unsafe.Pointer(Ꮡ(x.Value[0])), h));
}

internal static uintptr c128hash(@unsafe.Pointer Δp, uintptr h) {
    var x = (ж<array<float64>>)(uintptr)(Δp);
    return f64hash(new @unsafe.Pointer(Ꮡ(x.Value[1])), f64hash(new @unsafe.Pointer(Ꮡ(x.Value[0])), h));
}

internal static uintptr interhash(@unsafe.Pointer Δp, uintptr h) {
    var a = (ж<iface>)(uintptr)(Δp);
    var tab = a.Value.tab;
    if (tab == nil) {
        return h;
    }
    var t = tab.Value.Type;
    if ((~t).Equal == default!) {
        // Check hashability here. We could do this check inside
        // typehash, but we want to report the topmost type in
        // the error text (e.g. in a struct with a field of slice type
        // we want to report the struct, not the slice).
        throw panic(((errorString)("hash of unhashable type "u8 + toRType(t).@string())));
    }
    if (isDirectIface(t)){
        return c1 * typehash(t, @unsafe.Pointer.FromRef(ref (a.of(iface.Ꮡdata)).Value), (uintptr)(h ^ c0));
    } else {
        return c1 * typehash(t, (~a).data, (uintptr)(h ^ c0));
    }
}

// nilinterhash should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/anacrolix/stm
//   - github.com/aristanetworks/goarista
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname nilinterhash
internal static uintptr nilinterhash(@unsafe.Pointer Δp, uintptr h) {
    var a = (ж<eface>)(uintptr)(Δp);
    var t = a.Value._type;
    if (t == nil) {
        return h;
    }
    if ((~t).Equal == default!) {
        // See comment in interhash above.
        throw panic(((errorString)("hash of unhashable type "u8 + toRType(t).@string())));
    }
    if (isDirectIface(t)){
        return c1 * typehash(t, @unsafe.Pointer.FromRef(ref (a.of(eface.Ꮡdata)).Value), (uintptr)(h ^ c0));
    } else {
        return c1 * typehash(t, (~a).data, (uintptr)(h ^ c0));
    }
}

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
//
// typehash should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/puzpuzpuz/xsync/v2
//   - github.com/puzpuzpuz/xsync/v3
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname typehash
internal static uintptr typehash(ж<_type> Ꮡt, @unsafe.Pointer Δp, uintptr h) {
    ref var t = ref Ꮡt.Value;

    if ((abi.TFlag)(t.TFlag & abi.TFlagRegularMemory) != 0) {
        // Handle ptr sizes specially, see issue 37086.
        var exprᴛ1 = t.Size_;
        if (exprᴛ1 == 4) {
            return memhash32(Δp, h);
        }
        if (exprᴛ1 == 8) {
            return memhash64(Δp, h);
        }
        { /* default: */
            return memhash(Δp, h, t.Size_);
        }

    }
    var exprᴛ2 = (abiꓸKind)(t.Kind_ & abi.KindMask);
    if (exprᴛ2 == abi.Float32) {
        return f32hash(Δp, h);
    }
    if (exprᴛ2 == abi.Float64) {
        return f64hash(Δp, h);
    }
    if (exprᴛ2 == abi.Complex64) {
        return c64hash(Δp, h);
    }
    if (exprᴛ2 == abi.Complex128) {
        return c128hash(Δp, h);
    }
    if (exprᴛ2 == abi.ΔString) {
        return strhash(Δp, h);
    }
    if (exprᴛ2 == abi.Interface) {
        var i = (ж<interfacetype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        if (len((~i).Methods) == 0) {
            return nilinterhash(Δp, h);
        }
        return interhash(Δp, h);
    }
    if (exprᴛ2 == abi.Array) {
        var a = (ж<arraytype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        for (var i = (uintptr)0; i < (~a).Len; i++) {
            h = typehash((~a).Elem, (uintptr)add(Δp, i * (~(~a).Elem).Size_), h);
        }
        return h;
    }
    if (exprᴛ2 == abi.Struct) {
        var s = (ж<structtype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (_, f) in (~s).Fields) {
            if (f.Name.IsBlank()) {
                continue;
            }
            h = typehash(f.Typ, (uintptr)add(Δp, f.Offset), h);
        }
        return h;
    }
    { /* default: */
        throw panic(((errorString)("hash of unhashable type "u8 + toRType(Ꮡt).@string())));
    }

}

// Should never happen, as typehash should only be called
// with comparable types.
internal static error mapKeyError(ж<maptype> Ꮡt, @unsafe.Pointer Δp) {
    ref var t = ref Ꮡt.Value;

    if (!t.HashMightPanic()) {
        return default!;
    }
    return mapKeyError2(t.Key, Δp);
}

internal static error mapKeyError2(ж<_type> Ꮡt, @unsafe.Pointer Δp) {
    ref var t = ref Ꮡt.Value;

    if ((abi.TFlag)(t.TFlag & abi.TFlagRegularMemory) != 0) {
        return default!;
    }
    var exprᴛ1 = (abiꓸKind)(t.Kind_ & abi.KindMask);
    if (exprᴛ1 == abi.Float32 || exprᴛ1 == abi.Float64 || exprᴛ1 == abi.Complex64 || exprᴛ1 == abi.Complex128 || exprᴛ1 == abi.ΔString) {
        return default!;
    }
    if (exprᴛ1 == abi.Interface) {
        var i = (ж<interfacetype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        ж<_type> tΔ2 = default!;
        ж<@unsafe.Pointer> pdata = default!;
        if (len((~i).Methods) == 0){
            var a = (ж<eface>)(uintptr)(Δp);
            tΔ2 = a.Value._type;
            if (tΔ2 == nil) {
                return default!;
            }
            pdata = a.of(eface.Ꮡdata);
        } else {
            var a = (ж<iface>)(uintptr)(Δp);
            if ((~a).tab == nil) {
                return default!;
            }
            tΔ2 = a.Value.tab.Value.Type;
            pdata = a.of(iface.Ꮡdata);
        }
        if ((~tΔ2).Equal == default!) {
            return ((errorString)("hash of unhashable type "u8 + toRType(tΔ2).@string()));
        }
        if (isDirectIface(tΔ2)){
            return mapKeyError2(tΔ2, @unsafe.Pointer.FromRef(ref (pdata).Value));
        } else {
            return mapKeyError2(tΔ2, pdata.Value);
        }
    }
    if (exprᴛ1 == abi.Array) {
        var a = (ж<arraytype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        for (var i = (uintptr)0; i < (~a).Len; i++) {
            {
                var err = mapKeyError2((~a).Elem, (uintptr)add(Δp, i * (~(~a).Elem).Size_)); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    }
    if (exprᴛ1 == abi.Struct) {
        var s = (ж<structtype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (_, f) in (~s).Fields) {
            if (f.Name.IsBlank()) {
                continue;
            }
            {
                var err = mapKeyError2(f.Typ, (uintptr)add(Δp, f.Offset)); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    }
    { /* default: */
        return ((errorString)("hash of unhashable type "u8 + toRType(Ꮡt).@string()));
    }

}

// Should never happen, keep this case for robustness.

//go:linkname reflect_typehash reflect.typehash
internal static uintptr reflect_typehash(ж<_type> Ꮡt, @unsafe.Pointer Δp, uintptr h) {
    ref var t = ref Ꮡt.Value;

    return typehash(Ꮡt, Δp, h);
}

internal static bool memequal0(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return true;
}

internal static bool memequal8(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<int8>)(uintptr)(Δp) == ~(ж<int8>)(uintptr)(q);
}

internal static bool memequal16(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<int16>)(uintptr)(Δp) == ~(ж<int16>)(uintptr)(q);
}

internal static bool memequal32(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<int32>)(uintptr)(Δp) == ~(ж<int32>)(uintptr)(q);
}

internal static bool memequal64(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<int64>)(uintptr)(Δp) == ~(ж<int64>)(uintptr)(q);
}

internal static bool memequal128(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<array<int64>>)(uintptr)(Δp) == ~(ж<array<int64>>)(uintptr)(q);
}

internal static bool f32equal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<float32>)(uintptr)(Δp) == ~(ж<float32>)(uintptr)(q);
}

internal static bool f64equal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<float64>)(uintptr)(Δp) == ~(ж<float64>)(uintptr)(q);
}

internal static bool c64equal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<complex64>)(uintptr)(Δp) == ~(ж<complex64>)(uintptr)(q);
}

internal static bool c128equal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<complex128>)(uintptr)(Δp) == ~(ж<complex128>)(uintptr)(q);
}

internal static bool strequal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    return ~(ж<@string>)(uintptr)(Δp) == ~(ж<@string>)(uintptr)(q);
}

internal static bool interequal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    var x = ~(ж<iface>)(uintptr)(Δp);
    var y = ~(ж<iface>)(uintptr)(q);
    return x.tab == y.tab && ifaceeq(x.tab, x.data, y.data);
}

internal static bool nilinterequal(@unsafe.Pointer Δp, @unsafe.Pointer q) {
    var x = ~(ж<eface>)(uintptr)(Δp);
    var y = ~(ж<eface>)(uintptr)(q);
    return x._type == y._type && efaceeq(x._type, x.data, y.data);
}

internal static bool efaceeq(ж<_type> Ꮡt, @unsafe.Pointer x, @unsafe.Pointer y) {
    ref var t = ref Ꮡt.DerefOrNil();

    if (Ꮡt == nil) {
        return true;
    }
    var eq = t.Equal;
    if (eq == default!) {
        throw panic(((errorString)("comparing uncomparable type "u8 + toRType(Ꮡt).@string())));
    }
    if (isDirectIface(Ꮡt)) {
        // Direct interface types are ptr, chan, map, func, and single-element structs/arrays thereof.
        // Maps and funcs are not comparable, so they can't reach here.
        // Ptrs, chans, and single-element items can be compared directly using ==.
        return x.Value == y.Value;
    }
    return eq(x, y);
}

internal static bool ifaceeq(ж<itab> Ꮡtab, @unsafe.Pointer x, @unsafe.Pointer y) {
    ref var tab = ref Ꮡtab.DerefOrNil();

    if (Ꮡtab == nil) {
        return true;
    }
    var t = tab.Type;
    var eq = t.Value.Equal;
    if (eq == default!) {
        throw panic(((errorString)("comparing uncomparable type "u8 + toRType(t).@string())));
    }
    if (isDirectIface(t)) {
        // See comment in efaceeq.
        return x.Value == y.Value;
    }
    return eq(x, y);
}

// Testing adapters for hash quality tests (see hash_test.go)
//
// stringHash should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/k14s/starlark-go
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname stringHash
internal static uintptr stringHash(@string s, uintptr seed) {
    return strhash((uintptr)noescape(new @unsafe.Pointer(Ꮡ(s))), seed);
}

internal static uintptr bytesHash(slice<byte> b, uintptr seed) {
    var s = (ж<Δsliceᴛ>)(uintptr)(new @unsafe.Pointer(Ꮡ(b)));
    return memhash((~s).Δarray, seed, (uintptr)(~s).len);
}

internal static uintptr int32Hash(uint32 i, uintptr seed) {
    return memhash32((uintptr)noescape(new @unsafe.Pointer(Ꮡ(i))), seed);
}

internal static uintptr int64Hash(uint64 i, uintptr seed) {
    return memhash64((uintptr)noescape(new @unsafe.Pointer(Ꮡ(i))), seed);
}

internal static uintptr efaceHash(any i, uintptr seed) {
    return nilinterhash((uintptr)noescape(new @unsafe.Pointer(Ꮡ(i))), seed);
}

[GoType("dyn")] partial interface ifaceHash_i {
    void F();
}

internal static uintptr ifaceHash(ifaceHash_i i, uintptr seed) {
    return interhash((uintptr)noescape(new @unsafe.Pointer(Ꮡ(i))), seed);
}

internal static readonly UntypedInt hashRandomBytes = /* goarch.PtrSize / 4 * 64 */ 128;

// used in asm_{386,amd64,arm64}.s to seed the hash function
internal static ж<array<byte>> Ꮡaeskeysched = new(new array<byte>(128));
internal static ref array<byte> aeskeysched => ref Ꮡaeskeysched.Value;

// used in hash{32,64}.go to seed the hash function
internal static array<uintptr> hashkey = new(4);

internal static void alginit() {
    // Install AES hash algorithms if the instructions needed are present.
    if ((GOARCH == "386"u8 || GOARCH == "amd64"u8) && cpu.X86.HasAES && cpu.X86.HasSSSE3 && cpu.X86.HasSSE41) {
        // AESENC
        // PSHUFB
        // PINSR{D,Q}
        initAlgAES();
        return;
    }
    if (GOARCH == "arm64"u8 && cpu.ARM64.HasAES) {
        initAlgAES();
        return;
    }
    foreach (var (i, _) in hashkey) {
        hashkey[i] = (uintptr)bootstrapRand();
    }
}

internal static void initAlgAES() {
    useAeshash = true;
    // Initialize with random data so hash collisions will be hard to engineer.
    var key = (ж<array<uint64>>)(uintptr)(new @unsafe.Pointer(Ꮡaeskeysched));
    foreach (var (i, _) in key.Value) {
        key.Value[i] = bootstrapRand();
    }
}

// Note: These routines perform the read with a native endianness.
internal static uint32 readUnaligned32(@unsafe.Pointer Δp) {
    var q = (ж<array<byte>>)(uintptr)(Δp);
    if (goarch.BigEndian) {
        return (uint32)((uint32)((uint32)((uint32)q.Value[3] | ((uint32)q.Value[2] << (int)(8))) | ((uint32)q.Value[1] << (int)(16))) | ((uint32)q.Value[0] << (int)(24)));
    }
    return (uint32)((uint32)((uint32)((uint32)q.Value[0] | ((uint32)q.Value[1] << (int)(8))) | ((uint32)q.Value[2] << (int)(16))) | ((uint32)q.Value[3] << (int)(24)));
}

internal static uint64 readUnaligned64(@unsafe.Pointer Δp) {
    var q = (ж<array<byte>>)(uintptr)(Δp);
    if (goarch.BigEndian) {
        return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)q.Value[7] | ((uint64)q.Value[6] << (int)(8))) | ((uint64)q.Value[5] << (int)(16))) | ((uint64)q.Value[4] << (int)(24))) | ((uint64)q.Value[3] << (int)(32))) | ((uint64)q.Value[2] << (int)(40))) | ((uint64)q.Value[1] << (int)(48))) | ((uint64)q.Value[0] << (int)(56)));
    }
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)q.Value[0] | ((uint64)q.Value[1] << (int)(8))) | ((uint64)q.Value[2] << (int)(16))) | ((uint64)q.Value[3] << (int)(24))) | ((uint64)q.Value[4] << (int)(32))) | ((uint64)q.Value[5] << (int)(40))) | ((uint64)q.Value[6] << (int)(48))) | ((uint64)q.Value[7] << (int)(56)));
}

} // end runtime_package
