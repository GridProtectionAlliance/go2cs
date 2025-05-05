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

internal static readonly UntypedInt itabInitSize = 512;

internal static mutex itabLock;                         // lock for accessing itab table
internal static ж<itabTableType> itabTable = Ꮡ(itabTableInit);                    // pointer to current table
internal static itabTableType itabTableInit = new itabTableType(size: itabInitSize); // starter table

// Note: change the formula in the mallocgc call in itabAdd if you change these fields.
[GoType] partial struct itabTableType {
    internal uintptr size;             // length of entries array. Always a power of 2.
    internal uintptr count;             // current number of filled entries.
    internal array<ж<itab>> entries = new(itabInitSize); // really [size] large
}

internal static uintptr itabHashFunc(ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp) {
    ref var inter = ref Ꮡinter.val;
    ref var typ = ref Ꮡtyp.val;

    // compiler has provided some good hash codes for us.
    return ((uintptr)((uint32)(inter.Type.Hash ^ typ.Hash)));
}

// getitab should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname getitab
internal static ж<itab> getitab(ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp, bool canfail) {
    ref var inter = ref Ꮡinter.val;
    ref var typ = ref Ꮡtyp.val;

    if (len(inter.Methods) == 0) {
        @throw("internal error - misuse of itab"u8);
    }
    // easy case
    if ((abi.TFlag)(typ.TFlag & abi.TFlagUncommon) == 0) {
        if (canfail) {
            return default!;
        }
        var name = toRType(Ꮡ(inter.Type)).nameOff(inter.Methods[0].Name);
        throw panic(Ꮡ(new TypeAssertionError(nil, Ꮡtyp, Ꮡ(inter.Type), name.Name())));
    }
    ж<itab> m = default!;
    // First, look in the existing table to see if we can find the itab we need.
    // This is by far the most common case, so do it without locks.
    // Use atomic to ensure we see any previous writes done by the thread
    // that updates the itabTable field (with atomic.Storep in itabAdd).
    var t = (ж<itabTableType>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(itabTable)))));
    {
        m = t.find(Ꮡinter, Ꮡtyp); if (m != nil) {
            goto finish;
        }
    }
    // Not found.  Grab the lock and try again.
    @lock(Ꮡ(itabLock));
    {
        m = itabTable.find(Ꮡinter, Ꮡtyp); if (m != nil) {
            unlock(Ꮡ(itabLock));
            goto finish;
        }
    }
    // Entry doesn't exist yet. Make a new entry & add it.
    m = (ж<itab>)(uintptr)(persistentalloc(@unsafe.Sizeof(new itab{}) + ((uintptr)(len(inter.Methods) - 1)) * goarch.PtrSize, 0, Ꮡmemstats.of(mstats.Ꮡother_sys)));
    m.val.Inter = inter;
    m.val.Type = typ;
    // The hash is used in type switches. However, compiler statically generates itab's
    // for all interface/type pairs used in switches (which are added to itabTable
    // in itabsinit). The dynamically-generated itab's never participate in type switches,
    // and thus the hash is irrelevant.
    // Note: m.Hash is _not_ the hash used for the runtime itabTable hash table.
    m.val.Hash = 0;
    itabInit(m, true);
    itabAdd(m);
    unlock(Ꮡ(itabLock));
finish:
    if ((~m).Fun[0] != 0) {
        return m;
    }
    if (canfail) {
        return default!;
    }
    // this can only happen if the conversion
    // was already done once using the , ok form
    // and we have a cached negative result.
    // The cached result doesn't record which
    // interface function was missing, so initialize
    // the itab again to get the missing function name.
    throw panic(Ꮡ(new TypeAssertionError(concrete: typ, asserted: Ꮡ(inter.Type), missingMethod: itabInit(m, false))));
}

// find finds the given interface/type pair in t.
// Returns nil if the given interface/type pair isn't present.
[GoRecv] internal static ж<itab> find(this ref itabTableType t, ж<interfacetype> Ꮡinter, ж<_type> Ꮡtyp) {
    ref var inter = ref Ꮡinter.val;
    ref var typ = ref Ꮡtyp.val;

    // Implemented using quadratic probing.
    // Probe sequence is h(i) = h0 + i*(i+1)/2 mod 2^k.
    // We're guaranteed to hit all table entries using this probe sequence.
    var mask = t.size - 1;
    var h = (uintptr)(itabHashFunc(Ꮡinter, Ꮡtyp) & mask);
    for (var i = ((uintptr)1); ᐧ ; i++) {
        var Δp = (ж<ж<itab>>)(uintptr)(add(new @unsafe.Pointer(Ꮡ(t.entries)), h * goarch.PtrSize));
        // Use atomic read here so if we see m != nil, we also see
        // the initializations of the fields of m.
        // m := *p
        var m = (ж<itab>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)Δp)));
        if (m == nil) {
            return default!;
        }
        if ((~m).Inter == Ꮡinter && (~m).Type == Ꮡtyp) {
            return m;
        }
        h += i;
        h &= (uintptr)(mask);
    }
}

// itabAdd adds the given itab to the itab hash table.
// itabLock must be held.
internal static void itabAdd(ж<itab> Ꮡm) {
    ref var m = ref Ꮡm.val;

    // Bugs can lead to calling this while mallocing is set,
    // typically because this is called while panicking.
    // Crash reliably, rather than only when we need to grow
    // the hash table.
    if ((~(~getg()).m).mallocing != 0) {
        @throw("malloc deadlock"u8);
    }
    var t = itabTable;
    if ((~t).count >= 3 * ((~t).size / 4)) {
        // 75% load factor
        // Grow hash table.
        // t2 = new(itabTableType) + some additional entries
        // We lie and tell malloc we want pointer-free memory because
        // all the pointed-to values are not in the heap.
        var t2 = (ж<itabTableType>)(uintptr)(mallocgc((2 + 2 * (~t).size) * goarch.PtrSize, nil, true));
        t2.val.size = (~t).size * 2;
        // Copy over entries.
        // Note: while copying, other threads may look for an itab and
        // fail to find it. That's ok, they will then try to get the itab lock
        // and as a consequence wait until this copying is complete.
        iterate_itabs(t2.add);
        if ((~t2).count != (~t).count) {
            @throw("mismatched count during itab table copy"u8);
        }
        // Publish new hash table. Use an atomic write: see comment in getitab.
        atomicstorep(((@unsafe.Pointer)(Ꮡ(itabTable))), new @unsafe.Pointer(t2));
        // Adopt the new table as our own.
        t = itabTable;
    }
    // Note: the old table can be GC'ed here.
    t.add(Ꮡm);
}

// add adds the given itab to itab table t.
// itabLock must be held.
[GoRecv] internal static void add(this ref itabTableType t, ж<itab> Ꮡm) {
    ref var m = ref Ꮡm.val;

    // See comment in find about the probe sequence.
    // Insert new itab in the first empty spot in the probe sequence.
    var mask = t.size - 1;
    var h = (uintptr)(itabHashFunc(m.Inter, m.Type) & mask);
    for (var i = ((uintptr)1); ᐧ ; i++) {
        var Δp = (ж<ж<itab>>)(uintptr)(add(new @unsafe.Pointer(Ꮡ(t.entries)), h * goarch.PtrSize));
        var m2 = Δp.val;
        if (m2 == Ꮡm) {
            // A given itab may be used in more than one module
            // and thanks to the way global symbol resolution works, the
            // pointed-to itab may already have been inserted into the
            // global 'hash'.
            return;
        }
        if (m2 == nil) {
            // Use atomic write here so if a reader sees m, it also
            // sees the correctly initialized fields of m.
            // NoWB is ok because m is not in heap memory.
            // *p = m
            atomic.StorepNoWB(((@unsafe.Pointer)Δp), new @unsafe.Pointer(Ꮡm));
            t.count++;
            return;
        }
        h += i;
        h &= (uintptr)(mask);
    }
}

// itabInit fills in the m.Fun array with all the code pointers for
// the m.Inter/m.Type pair. If the type does not implement the interface,
// it sets m.Fun[0] to 0 and returns the name of an interface function that is missing.
// If !firstTime, itabInit will not write anything to m.Fun (see issue 65962).
// It is ok to call this multiple times on the same m, even concurrently
// (although it will only be called once with firstTime==true).
internal static unsafe @string itabInit(ж<itab> Ꮡm, bool firstTime) {
    ref var m = ref Ꮡm.val;

    var inter = m.Inter;
    var typ = m.Type;
    var x = typ.Uncommon();
    // both inter and typ have method sorted by name,
    // and interface names are unique,
    // so can iterate over both in lock step;
    // the loop is O(ni+nt) not O(ni*nt).
    nint ni = len((~inter).Methods);
    nint nt = ((nint)(~x).Mcount);
    var xmhdr = new Span<abi.Method>((abi.Method*)(uintptr)(add(new @unsafe.Pointer(x), ((uintptr)(~x).Moff))), nt);
    nint j = 0;
    var methods = new Span<@unsafe.Pointer>((@unsafe.Pointer*)(uintptr)(((@unsafe.Pointer)(Ꮡm.Fun.at<uintptr>(0)))), ni);
    @unsafe.Pointer fun0 = default!;
imethods:
    for (nint k = 0; k < ni; k++) {
        var i = Ꮡ((~inter).Methods, k);
        var itype = toRType(Ꮡ((~inter).Type)).typeOff((~i).Typ);
        var name = toRType(Ꮡ((~inter).Type)).nameOff((~i).Name);
        @string iname = name.Name();
        @string ipkg = pkgPath(name);
        if (ipkg == ""u8) {
            ipkg = (~inter).PkgPath.Name();
        }
        for (; j < nt; j++) {
            var t = Ꮡ(xmhdr, j);
            var rtyp = toRType(typ);
            var tname = rtyp.nameOff((~t).Name);
            if (rtyp.typeOff((~t).Mtyp) == itype && tname.Name() == iname) {
                @string pkgPathΔ1 = pkgPath(tname);
                if (pkgPathΔ1 == ""u8) {
                    pkgPathΔ1 = rtyp.nameOff((~x).PkgPath).Name();
                }
                if (tname.IsExported() || pkgPathΔ1 == ipkg) {
                    @unsafe.Pointer ifn = (uintptr)rtyp.textOff((~t).Ifn);
                    if (k == 0){
                        fun0 = ifn;
                    } else 
                    if (firstTime) {
                        // we'll set m.Fun[0] at the end
                        methods[k] = ifn;
                    }
                    goto continue_imethods;
                }
            }
        }
        // didn't find method
        // Leaves m.Fun[0] set to 0.
        return iname;
continue_imethods:;
    }
break_imethods:;
    if (firstTime) {
        m.Fun[0] = ((uintptr)fun0);
    }
    return ""u8;
}

internal static void itabsinit() {
    lockInit(Ꮡ(itabLock), lockRankItab);
    @lock(Ꮡ(itabLock));
    foreach (var (_, md) in activeModules()) {
        foreach (var (_, i) in (~md).itablinks) {
            itabAdd(i);
        }
    }
    unlock(Ꮡ(itabLock));
}

// panicdottypeE is called when doing an e.(T) conversion and the conversion fails.
// have = the dynamic type we have.
// want = the static type we're trying to convert to.
// iface = the static type we're converting from.
internal static void panicdottypeE(ж<_type> Ꮡhave, ж<_type> Ꮡwant, ж<_type> Ꮡiface) {
    ref var have = ref Ꮡhave.val;
    ref var want = ref Ꮡwant.val;
    ref var iface = ref Ꮡiface.val;

    throw panic(Ꮡ(new TypeAssertionError(Ꮡiface, Ꮡhave, Ꮡwant, "")));
}

// panicdottypeI is called when doing an i.(T) conversion and the conversion fails.
// Same args as panicdottypeE, but "have" is the dynamic itab we have.
internal static void panicdottypeI(ж<itab> Ꮡhave, ж<_type> Ꮡwant, ж<_type> Ꮡiface) {
    ref var have = ref Ꮡhave.val;
    ref var want = ref Ꮡwant.val;
    ref var iface = ref Ꮡiface.val;

    ж<_type> t = default!;
    if (have != nil) {
        t = have.Type;
    }
    panicdottypeE(t, Ꮡwant, Ꮡiface);
}

// panicnildottype is called when doing an i.(T) conversion and the interface i is nil.
// want = the static type we're trying to convert to.
internal static void panicnildottype(ж<_type> Ꮡwant) {
    ref var want = ref Ꮡwant.val;

    throw panic(Ꮡ(new TypeAssertionError(nil, nil, Ꮡwant, "")));
}

[GoType("num:uint16")] partial struct uint16InterfacePtr;

[GoType("num:uint32")] partial struct uint32InterfacePtr;

[GoType("num:uint64")] partial struct uint64InterfacePtr;

[GoType("@string")] partial struct stringInterfacePtr;

[GoType("[]byte")] partial struct sliceInterfacePtr;

// TODO: Add the static type we're converting from as well.
// It might generate a better error message.
// Just to match other nil conversion errors, we don't for now.
internal static any uint16Eface = ((uint16InterfacePtr)0);
internal static any uint32Eface = ((uint32InterfacePtr)0);
internal static any uint64Eface = ((uint64InterfacePtr)0);
internal static any stringEface = ((stringInterfacePtr)""u8);
internal static any sliceEface = ((sliceInterfacePtr)default!);
internal static ж<_type> uint16Type = (~efaceOf(Ꮡ(uint16Eface)))._type;
internal static ж<_type> uint32Type = (~efaceOf(Ꮡ(uint32Eface)))._type;
internal static ж<_type> uint64Type = (~efaceOf(Ꮡ(uint64Eface)))._type;
internal static ж<_type> stringType = (~efaceOf(Ꮡ(stringEface)))._type;
internal static ж<_type> sliceType = (~efaceOf(Ꮡ(sliceEface)))._type;

// The conv and assert functions below do very similar things.
// The convXXX functions are guaranteed by the compiler to succeed.
// The assertXXX functions may fail (either panicking or returning false,
// depending on whether they are 1-result or 2-result).
// The convXXX functions succeed on a nil input, whereas the assertXXX
// functions fail on a nil input.

// convT converts a value of type t, which is pointed to by v, to a pointer that can
// be used as the second word of an interface value.
internal static @unsafe.Pointer convT(ж<_type> Ꮡt, @unsafe.Pointer v) {
    ref var t = ref Ꮡt.val;

    if (raceenabled) {
        raceReadObjectPC(Ꮡt, v.val, getcallerpc(), abi.FuncPCABIInternal(convT));
    }
    if (msanenabled) {
        msanread(v.val, t.Size_);
    }
    if (asanenabled) {
        asanread(v.val, t.Size_);
    }
    @unsafe.Pointer x = (uintptr)mallocgc(t.Size_, Ꮡt, true);
    typedmemmove(Ꮡt, x, v.val);
    return x;
}

internal static @unsafe.Pointer convTnoptr(ж<_type> Ꮡt, @unsafe.Pointer v) {
    ref var t = ref Ꮡt.val;

    // TODO: maybe take size instead of type?
    if (raceenabled) {
        raceReadObjectPC(Ꮡt, v.val, getcallerpc(), abi.FuncPCABIInternal(convTnoptr));
    }
    if (msanenabled) {
        msanread(v.val, t.Size_);
    }
    if (asanenabled) {
        asanread(v.val, t.Size_);
    }
    @unsafe.Pointer x = (uintptr)mallocgc(t.Size_, Ꮡt, false);
    memmove(x, v.val, t.Size_);
    return x;
}

internal static @unsafe.Pointer /*x*/ convT16(uint16 val) {
    @unsafe.Pointer x = default!;

    if (val < ((uint16)len(staticuint64s))){
        x = new @unsafe.Pointer(Ꮡstaticuint64s.at<uint64>(val));
        if (goarch.BigEndian) {
            x = (uintptr)add(x, 6);
        }
    } else {
        x = (uintptr)mallocgc(2, uint16Type, false);
        ((ж<uint16>)(uintptr)(x)).val = val;
    }
    return x;
}

internal static @unsafe.Pointer /*x*/ convT32(uint32 val) {
    @unsafe.Pointer x = default!;

    if (val < ((uint32)len(staticuint64s))){
        x = new @unsafe.Pointer(Ꮡstaticuint64s.at<uint64>(val));
        if (goarch.BigEndian) {
            x = (uintptr)add(x, 4);
        }
    } else {
        x = (uintptr)mallocgc(4, uint32Type, false);
        ((ж<uint32>)(uintptr)(x)).val = val;
    }
    return x;
}

// convT64 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convT64
internal static @unsafe.Pointer /*x*/ convT64(uint64 val) {
    @unsafe.Pointer x = default!;

    if (val < ((uint64)len(staticuint64s))){
        x = new @unsafe.Pointer(Ꮡstaticuint64s.at<uint64>(val));
    } else {
        x = (uintptr)mallocgc(8, uint64Type, false);
        ((ж<uint64>)(uintptr)(x)).val = val;
    }
    return x;
}

// convTstring should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convTstring
internal static @unsafe.Pointer /*x*/ convTstring(@string val) {
    @unsafe.Pointer x = default!;

    if (val == ""u8){
        x = new @unsafe.Pointer(ᏑzeroVal.at<byte>(0));
    } else {
        x = (uintptr)mallocgc(@unsafe.Sizeof(val), stringType, true);
        ((ж<@string>)(uintptr)(x)).val = val;
    }
    return x;
}

// convTslice should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convTslice
internal static @unsafe.Pointer /*x*/ convTslice(slice<byte> val) {
    @unsafe.Pointer x = default!;

    // Note: this must work for any element type, not just byte.
    if (((ж<Δslice>)(uintptr)(new @unsafe.Pointer(Ꮡ(val)))).val.Δarray == nil){
        x = new @unsafe.Pointer(ᏑzeroVal.at<byte>(0));
    } else {
        x = (uintptr)mallocgc(@unsafe.Sizeof(val), sliceType, true);
        ((ж<slice<byte>>)(uintptr)(x)).val = val;
    }
    return x;
}

internal static ж<itab> assertE2I(ж<interfacetype> Ꮡinter, ж<_type> Ꮡt) {
    ref var inter = ref Ꮡinter.val;
    ref var t = ref Ꮡt.val;

    if (t == nil) {
        // explicit conversions require non-nil interface value.
        throw panic(Ꮡ(new TypeAssertionError(nil, nil, Ꮡ(inter.Type), "")));
    }
    return getitab(Ꮡinter, Ꮡt, false);
}

internal static ж<itab> assertE2I2(ж<interfacetype> Ꮡinter, ж<_type> Ꮡt) {
    ref var inter = ref Ꮡinter.val;
    ref var t = ref Ꮡt.val;

    if (t == nil) {
        return default!;
    }
    return getitab(Ꮡinter, Ꮡt, true);
}

// typeAssert builds an itab for the concrete type t and the
// interface type s.Inter. If the conversion is not possible it
// panics if s.CanFail is false and returns nil if s.CanFail is true.
internal static ж<itab> typeAssert(ж<abi.TypeAssert> Ꮡs, ж<_type> Ꮡt) {
    ref var s = ref Ꮡs.val;
    ref var t = ref Ꮡt.val;

    ж<itab> tab = default!;
    if (t == nil){
        if (!s.CanFail) {
            throw panic(Ꮡ(new TypeAssertionError(nil, nil, Ꮡ(s.Inter.Type), "")));
        }
    } else {
        tab = getitab(s.Inter, Ꮡt, s.CanFail);
    }
    if (!abi.UseInterfaceSwitchCache(GOARCH)) {
        return tab;
    }
    // Maybe update the cache, so the next time the generated code
    // doesn't need to call into the runtime.
    if ((uint32)(cheaprand() & 1023) != 0) {
        // Only bother updating the cache ~1 in 1000 times.
        return tab;
    }
    // Load the current cache.
    var oldC = (ж<abi.TypeAssertCache>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(s.Cache)))));
    if ((uint32)(cheaprand() & ((uint32)(~oldC).Mask)) != 0) {
        // As cache gets larger, choose to update it less often
        // so we can amortize the cost of building a new cache.
        return tab;
    }
    // Make a new cache.
    var newC = buildTypeAssertCache(oldC, Ꮡt, tab);
    // Update cache. Use compare-and-swap so if multiple threads
    // are fighting to update the cache, at least one of their
    // updates will stick.
    atomic_casPointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(s.Cache)))), new @unsafe.Pointer(oldC), new @unsafe.Pointer(newC));
    return tab;
}

internal static ж<abi.TypeAssertCache> buildTypeAssertCache(ж<abi.TypeAssertCache> ᏑoldC, ж<_type> Ꮡtyp, ж<itab> Ꮡtab) {
    ref var oldC = ref ᏑoldC.val;
    ref var typ = ref Ꮡtyp.val;
    ref var tab = ref Ꮡtab.val;

    var oldEntries = @unsafe.Slice(ᏑoldC.Entries.at<abi.TypeAssertCacheEntry>(0), oldC.Mask + 1);
    // Count the number of entries we need.
    nint n = 1;
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            n++;
        }
    }
    // Figure out how big a table we need.
    // We need at least one more slot than the number of entries
    // so that we are guaranteed an empty slot (for termination).
    nint newN = n * 2;
    // make it at most 50% full
    newN = 1 << (int)(sys.Len64(((uint64)(newN - 1))));
    // round up to a power of 2
    // Allocate the new table.
    var newSize = @unsafe.Sizeof(new abi.TypeAssertCache(nil)) + ((uintptr)(newN - 1)) * @unsafe.Sizeof(new abi.TypeAssertCacheEntry(nil));
    var newC = (ж<abi.TypeAssertCache>)(uintptr)(mallocgc(newSize, nil, true));
    newC.val.Mask = ((uintptr)(newN - 1));
    var newEntries = @unsafe.Slice(Ꮡ(~newC).Entries.at<abi.TypeAssertCacheEntry>(0), newN);
    // Fill the new table.
    var addEntry = 
    var newEntriesʗ1 = newEntries;
    (ж<_type> typ, ж<itab> tab) => {
        nint h = (nint)(((nint)(~typΔ1).Hash) & (newN - 1));
        while (ᐧ) {
            if (newEntriesʗ1[h].Typ == 0) {
                newEntriesʗ1[h].Typ = ((uintptr)new @unsafe.Pointer(ᏑtypΔ1));
                newEntriesʗ1[h].Itab = ((uintptr)new @unsafe.Pointer(ᏑtabΔ1));
                return;
            }
            h = (nint)((h + 1) & (newN - 1));
        }
    };
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            addEntry((ж<_type>)(uintptr)(((@unsafe.Pointer)e.Typ)), (ж<itab>)(uintptr)(((@unsafe.Pointer)e.Itab)));
        }
    }
    addEntry(Ꮡtyp, Ꮡtab);
    return newC;
}

// Empty type assert cache. Contains one entry with a nil Typ (which
// causes a cache lookup to fail immediately.)
internal static abi.TypeAssertCache emptyTypeAssertCache = new abi.TypeAssertCache(Mask: 0);

// interfaceSwitch compares t against the list of cases in s.
// If t matches case i, interfaceSwitch returns the case index i and
// an itab for the pair <t, s.Cases[i]>.
// If there is no match, return N,nil, where N is the number
// of cases.
internal static (nint, ж<itab>) interfaceSwitch(ж<abi.InterfaceSwitch> Ꮡs, ж<_type> Ꮡt) {
    ref var s = ref Ꮡs.val;
    ref var t = ref Ꮡt.val;

    var cases = @unsafe.Slice(Ꮡs.Cases.at<abiꓸInterfaceType>(0), s.NCases);
    // Results if we don't find a match.
    nint case_ = len(cases);
    ж<itab> tab = default!;
    // Look through each case in order.
    foreach (var (i, c) in cases) {
        tab = getitab(c, Ꮡt, true);
        if (tab != nil) {
            case_ = i;
            break;
        }
    }
    if (!abi.UseInterfaceSwitchCache(GOARCH)) {
        return (case_, tab);
    }
    // Maybe update the cache, so the next time the generated code
    // doesn't need to call into the runtime.
    if ((uint32)(cheaprand() & 1023) != 0) {
        // Only bother updating the cache ~1 in 1000 times.
        // This ensures we don't waste memory on switches, or
        // switch arguments, that only happen a few times.
        return (case_, tab);
    }
    // Load the current cache.
    var oldC = (ж<abi.InterfaceSwitchCache>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(s.Cache)))));
    if ((uint32)(cheaprand() & ((uint32)(~oldC).Mask)) != 0) {
        // As cache gets larger, choose to update it less often
        // so we can amortize the cost of building a new cache
        // (that cost is linear in oldc.Mask).
        return (case_, tab);
    }
    // Make a new cache.
    var newC = buildInterfaceSwitchCache(oldC, Ꮡt, case_, tab);
    // Update cache. Use compare-and-swap so if multiple threads
    // are fighting to update the cache, at least one of their
    // updates will stick.
    atomic_casPointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(s.Cache)))), new @unsafe.Pointer(oldC), new @unsafe.Pointer(newC));
    return (case_, tab);
}

// buildInterfaceSwitchCache constructs an interface switch cache
// containing all the entries from oldC plus the new entry
// (typ,case_,tab).
internal static ж<abi.InterfaceSwitchCache> buildInterfaceSwitchCache(ж<abi.InterfaceSwitchCache> ᏑoldC, ж<_type> Ꮡtyp, nint case_, ж<itab> Ꮡtab) {
    ref var oldC = ref ᏑoldC.val;
    ref var typ = ref Ꮡtyp.val;
    ref var tab = ref Ꮡtab.val;

    var oldEntries = @unsafe.Slice(ᏑoldC.Entries.at<abi.InterfaceSwitchCacheEntry>(0), oldC.Mask + 1);
    // Count the number of entries we need.
    nint n = 1;
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            n++;
        }
    }
    // Figure out how big a table we need.
    // We need at least one more slot than the number of entries
    // so that we are guaranteed an empty slot (for termination).
    nint newN = n * 2;
    // make it at most 50% full
    newN = 1 << (int)(sys.Len64(((uint64)(newN - 1))));
    // round up to a power of 2
    // Allocate the new table.
    var newSize = @unsafe.Sizeof(new abi.InterfaceSwitchCache(nil)) + ((uintptr)(newN - 1)) * @unsafe.Sizeof(new abi.InterfaceSwitchCacheEntry(nil));
    var newC = (ж<abi.InterfaceSwitchCache>)(uintptr)(mallocgc(newSize, nil, true));
    newC.val.Mask = ((uintptr)(newN - 1));
    var newEntries = @unsafe.Slice(Ꮡ(~newC).Entries.at<abi.InterfaceSwitchCacheEntry>(0), newN);
    // Fill the new table.
    var addEntry = 
    var newEntriesʗ1 = newEntries;
    (ж<_type> typ, nint case_, ж<itab> tab) => {
        nint h = (nint)(((nint)(~typΔ1).Hash) & (newN - 1));
        while (ᐧ) {
            if (newEntriesʗ1[h].Typ == 0) {
                newEntriesʗ1[h].Typ = ((uintptr)new @unsafe.Pointer(ᏑtypΔ1));
                newEntriesʗ1[h].Case = case_Δ1;
                newEntriesʗ1[h].Itab = ((uintptr)new @unsafe.Pointer(ᏑtabΔ1));
                return;
            }
            h = (nint)((h + 1) & (newN - 1));
        }
    };
    foreach (var (_, e) in oldEntries) {
        if (e.Typ != 0) {
            addEntry((ж<_type>)(uintptr)(((@unsafe.Pointer)e.Typ)), e.Case, (ж<itab>)(uintptr)(((@unsafe.Pointer)e.Itab)));
        }
    }
    addEntry(Ꮡtyp, case_, Ꮡtab);
    return newC;
}

// Empty interface switch cache. Contains one entry with a nil Typ (which
// causes a cache lookup to fail immediately.)
internal static abi.InterfaceSwitchCache emptyInterfaceSwitchCache = new abi.InterfaceSwitchCache(Mask: 0);

// reflect_ifaceE2I is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
//
//go:linkname reflect_ifaceE2I reflect.ifaceE2I
internal static void reflect_ifaceE2I(ж<interfacetype> Ꮡinter, eface e, ж<iface> Ꮡdst) {
    ref var inter = ref Ꮡinter.val;
    ref var dst = ref Ꮡdst.val;

    dst = new iface(assertE2I(Ꮡinter, e._type), e.data);
}

//go:linkname reflectlite_ifaceE2I internal/reflectlite.ifaceE2I
internal static void reflectlite_ifaceE2I(ж<interfacetype> Ꮡinter, eface e, ж<iface> Ꮡdst) {
    ref var inter = ref Ꮡinter.val;
    ref var dst = ref Ꮡdst.val;

    dst = new iface(assertE2I(Ꮡinter, e._type), e.data);
}

internal static void iterate_itabs(Action<ж<itab>> fn) {
    // Note: only runs during stop the world or with itabLock held,
    // so no other locks/atomics needed.
    var t = itabTable;
    for (var i = ((uintptr)0); i < (~t).size; i++) {
        var m = ~(ж<ж<itab>>)(uintptr)(add(new @unsafe.Pointer(Ꮡ((~t).entries)), i * goarch.PtrSize));
        if (m != nil) {
            fn(m);
        }
    }
}

// staticuint64s is used to avoid allocating in convTx for small integer values.
internal static array<uint64> staticuint64s = new uint64[]{
    0, 1, 2, 3, 4, 5, 6, 7,
    8, 9, 10, 11, 12, 13, 14, 15,
    16, 17, 18, 19, 20, 21, 22, 23,
    24, 25, 26, 27, 28, 29, 30, 31,
    32, 33, 34, 35, 36, 37, 38, 39,
    40, 41, 42, 43, 44, 45, 46, 47,
    48, 49, 50, 51, 52, 53, 54, 55,
    56, 57, 58, 59, 60, 61, 62, 63,
    64, 65, 66, 67, 68, 69, 70, 71,
    72, 73, 74, 75, 76, 77, 78, 79,
    80, 81, 82, 83, 84, 85, 86, 87,
    88, 89, 90, 91, 92, 93, 94, 95,
    96, 97, 98, 99, 100, 101, 102, 103,
    104, 105, 106, 107, 108, 109, 110, 111,
    112, 113, 114, 115, 116, 117, 118, 119,
    120, 121, 122, 123, 124, 125, 126, 127,
    128, 129, 130, 131, 132, 133, 134, 135,
    136, 137, 138, 139, 140, 141, 142, 143,
    144, 145, 146, 147, 148, 149, 150, 151,
    152, 153, 154, 155, 156, 157, 158, 159,
    160, 161, 162, 163, 164, 165, 166, 167,
    168, 169, 170, 171, 172, 173, 174, 175,
    176, 177, 178, 179, 180, 181, 182, 183,
    184, 185, 186, 187, 188, 189, 190, 191,
    192, 193, 194, 195, 196, 197, 198, 199,
    200, 201, 202, 203, 204, 205, 206, 207,
    208, 209, 210, 211, 212, 213, 214, 215,
    216, 217, 218, 219, 220, 221, 222, 223,
    224, 225, 226, 227, 228, 229, 230, 231,
    232, 233, 234, 235, 236, 237, 238, 239,
    240, 241, 242, 243, 244, 245, 246, 247,
    248, 249, 250, 251, 252, 253, 254, 255
}.array();

// The linker redirects a reference of a method that it determined
// unreachable to a reference to this function, so it will throw if
// ever called.
internal static void unreachableMethod() {
    @throw("unreachable method called. linker bug?"u8);
}

} // end runtime_package
