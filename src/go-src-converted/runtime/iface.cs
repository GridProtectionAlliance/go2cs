// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\iface.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private static readonly nint itabInitSize = 512;



private static mutex itabLock = default;private static var itabTable = _addr_itabTableInit;private static itabTableType itabTableInit = new itabTableType(size:itabInitSize);

// Note: change the formula in the mallocgc call in itabAdd if you change these fields.
private partial struct itabTableType {
    public System.UIntPtr size; // length of entries array. Always a power of 2.
    public System.UIntPtr count; // current number of filled entries.
    public array<ptr<itab>> entries; // really [size] large
}

private static System.UIntPtr itabHashFunc(ptr<interfacetype> _addr_inter, ptr<_type> _addr_typ) {
    ref interfacetype inter = ref _addr_inter.val;
    ref _type typ = ref _addr_typ.val;
 
    // compiler has provided some good hash codes for us.
    return uintptr(inter.typ.hash ^ typ.hash);

}

private static ptr<itab> getitab(ptr<interfacetype> _addr_inter, ptr<_type> _addr_typ, bool canfail) => func((_, panic, _) => {
    ref interfacetype inter = ref _addr_inter.val;
    ref _type typ = ref _addr_typ.val;

    if (len(inter.mhdr) == 0) {
        throw("internal error - misuse of itab");
    }
    if (typ.tflag & tflagUncommon == 0) {
        if (canfail) {
            return _addr_null!;
        }
        var name = inter.typ.nameOff(inter.mhdr[0].name);
        panic(addr(new TypeAssertionError(nil,typ,&inter.typ,name.name())));

    }
    ptr<itab> m; 

    // First, look in the existing table to see if we can find the itab we need.
    // This is by far the most common case, so do it without locks.
    // Use atomic to ensure we see any previous writes done by the thread
    // that updates the itabTable field (with atomic.Storep in itabAdd).
    var t = (itabTableType.val)(atomic.Loadp(@unsafe.Pointer(_addr_itabTable)));
    m = t.find(inter, typ);

    if (m != null) {
        goto finish;
    }
    lock(_addr_itabLock);
    m = itabTable.find(inter, typ);

    if (m != null) {
        unlock(_addr_itabLock);
        goto finish;
    }
    m = (itab.val)(persistentalloc(@unsafe.Sizeof(new itab()) + uintptr(len(inter.mhdr) - 1) * sys.PtrSize, 0, _addr_memstats.other_sys));
    m.inter = inter;
    m._type = typ; 
    // The hash is used in type switches. However, compiler statically generates itab's
    // for all interface/type pairs used in switches (which are added to itabTable
    // in itabsinit). The dynamically-generated itab's never participate in type switches,
    // and thus the hash is irrelevant.
    // Note: m.hash is _not_ the hash used for the runtime itabTable hash table.
    m.hash = 0;
    m.init();
    itabAdd(m);
    unlock(_addr_itabLock);
finish:
    if (m.fun[0] != 0) {
        return _addr_m!;
    }
    if (canfail) {
        return _addr_null!;
    }
    panic(addr(new TypeAssertionError(concrete:typ,asserted:&inter.typ,missingMethod:m.init())));

});

// find finds the given interface/type pair in t.
// Returns nil if the given interface/type pair isn't present.
private static ptr<itab> find(this ptr<itabTableType> _addr_t, ptr<interfacetype> _addr_inter, ptr<_type> _addr_typ) {
    ref itabTableType t = ref _addr_t.val;
    ref interfacetype inter = ref _addr_inter.val;
    ref _type typ = ref _addr_typ.val;
 
    // Implemented using quadratic probing.
    // Probe sequence is h(i) = h0 + i*(i+1)/2 mod 2^k.
    // We're guaranteed to hit all table entries using this probe sequence.
    var mask = t.size - 1;
    var h = itabHashFunc(_addr_inter, _addr_typ) & mask;
    for (var i = uintptr(1); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        var p = (itab.val)(add(@unsafe.Pointer(_addr_t.entries), h * sys.PtrSize)); 
        // Use atomic read here so if we see m != nil, we also see
        // the initializations of the fields of m.
        // m := *p
        var m = (itab.val)(atomic.Loadp(@unsafe.Pointer(p)));
        if (m == null) {
            return _addr_null!;
        }
        if (m.inter == inter && m._type == typ) {
            return _addr_m!;
        }
        h += i;
        h &= mask;

    }

}

// itabAdd adds the given itab to the itab hash table.
// itabLock must be held.
private static void itabAdd(ptr<itab> _addr_m) {
    ref itab m = ref _addr_m.val;
 
    // Bugs can lead to calling this while mallocing is set,
    // typically because this is called while panicing.
    // Crash reliably, rather than only when we need to grow
    // the hash table.
    if (getg().m.mallocing != 0) {
        throw("malloc deadlock");
    }
    var t = itabTable;
    if (t.count >= 3 * (t.size / 4)) { // 75% load factor
        // Grow hash table.
        // t2 = new(itabTableType) + some additional entries
        // We lie and tell malloc we want pointer-free memory because
        // all the pointed-to values are not in the heap.
        var t2 = (itabTableType.val)(mallocgc((2 + 2 * t.size) * sys.PtrSize, null, true));
        t2.size = t.size * 2; 

        // Copy over entries.
        // Note: while copying, other threads may look for an itab and
        // fail to find it. That's ok, they will then try to get the itab lock
        // and as a consequence wait until this copying is complete.
        iterate_itabs(t2.add);
        if (t2.count != t.count) {
            throw("mismatched count during itab table copy");
        }
        atomicstorep(@unsafe.Pointer(_addr_itabTable), @unsafe.Pointer(t2)); 
        // Adopt the new table as our own.
        t = itabTable; 
        // Note: the old table can be GC'ed here.
    }
    t.add(m);

}

// add adds the given itab to itab table t.
// itabLock must be held.
private static void add(this ptr<itabTableType> _addr_t, ptr<itab> _addr_m) {
    ref itabTableType t = ref _addr_t.val;
    ref itab m = ref _addr_m.val;
 
    // See comment in find about the probe sequence.
    // Insert new itab in the first empty spot in the probe sequence.
    var mask = t.size - 1;
    var h = itabHashFunc(_addr_m.inter, _addr_m._type) & mask;
    for (var i = uintptr(1); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        var p = (itab.val)(add(@unsafe.Pointer(_addr_t.entries), h * sys.PtrSize));
        var m2 = p.val;
        if (m2 == m) { 
            // A given itab may be used in more than one module
            // and thanks to the way global symbol resolution works, the
            // pointed-to itab may already have been inserted into the
            // global 'hash'.
            return ;

        }
        if (m2 == null) { 
            // Use atomic write here so if a reader sees m, it also
            // sees the correctly initialized fields of m.
            // NoWB is ok because m is not in heap memory.
            // *p = m
            atomic.StorepNoWB(@unsafe.Pointer(p), @unsafe.Pointer(m));
            t.count++;
            return ;

        }
        h += i;
        h &= mask;

    }

}

// init fills in the m.fun array with all the code pointers for
// the m.inter/m._type pair. If the type does not implement the interface,
// it sets m.fun[0] to 0 and returns the name of an interface function that is missing.
// It is ok to call this multiple times on the same m, even concurrently.
private static @string init(this ptr<itab> _addr_m) {
    ref itab m = ref _addr_m.val;

    var inter = m.inter;
    var typ = m._type;
    var x = typ.uncommon(); 

    // both inter and typ have method sorted by name,
    // and interface names are unique,
    // so can iterate over both in lock step;
    // the loop is O(ni+nt) not O(ni*nt).
    var ni = len(inter.mhdr);
    var nt = int(x.mcount);
    ptr<array<method>> xmhdr = new ptr<ptr<array<method>>>(add(@unsafe.Pointer(x), uintptr(x.moff))).slice(-1, nt, nt);
    nint j = 0;
    ptr<array<unsafe.Pointer>> methods = new ptr<ptr<array<unsafe.Pointer>>>(@unsafe.Pointer(_addr_m.fun[0])).slice(-1, ni, ni);
    unsafe.Pointer fun0 = default;
imethods:
    for (nint k = 0; k < ni; k++) {
        var i = _addr_inter.mhdr[k];
        var itype = inter.typ.typeOff(i.ityp);
        var name = inter.typ.nameOff(i.name);
        var iname = name.name();
        var ipkg = name.pkgPath();
        if (ipkg == "") {
            ipkg = inter.pkgpath.name();
        }
        while (j < nt) {
            var t = _addr_xmhdr[j];
            var tname = typ.nameOff(t.name);
            if (typ.typeOff(t.mtyp) == itype && tname.name() == iname) {
                var pkgPath = tname.pkgPath();
                if (pkgPath == "") {
                    pkgPath = typ.nameOff(x.pkgpath).name();
            j++;
                }

                if (tname.isExported() || pkgPath == ipkg) {
                    if (m != null) {
                        var ifn = typ.textOff(t.ifn);
                        if (k == 0) {
                            fun0 = ifn; // we'll set m.fun[0] at the end
                        }
                        else
 {
                            methods[k] = ifn;
                        }

                    }

                    _continueimethods = true;
                    break;
                }

            }

        } 
        // didn't find method
        m.fun[0] = 0;
        return iname;

    }
    m.fun[0] = uintptr(fun0);
    return "";

}

private static void itabsinit() {
    lockInit(_addr_itabLock, lockRankItab);
    lock(_addr_itabLock);
    foreach (var (_, md) in activeModules()) {
        foreach (var (_, i) in md.itablinks) {
            itabAdd(_addr_i);
        }
    }    unlock(_addr_itabLock);

}

// panicdottypeE is called when doing an e.(T) conversion and the conversion fails.
// have = the dynamic type we have.
// want = the static type we're trying to convert to.
// iface = the static type we're converting from.
private static void panicdottypeE(ptr<_type> _addr_have, ptr<_type> _addr_want, ptr<_type> _addr_iface) => func((_, panic, _) => {
    ref _type have = ref _addr_have.val;
    ref _type want = ref _addr_want.val;
    ref _type iface = ref _addr_iface.val;

    panic(addr(new TypeAssertionError(iface,have,want,"")));
});

// panicdottypeI is called when doing an i.(T) conversion and the conversion fails.
// Same args as panicdottypeE, but "have" is the dynamic itab we have.
private static void panicdottypeI(ptr<itab> _addr_have, ptr<_type> _addr_want, ptr<_type> _addr_iface) {
    ref itab have = ref _addr_have.val;
    ref _type want = ref _addr_want.val;
    ref _type iface = ref _addr_iface.val;

    ptr<_type> t;
    if (have != null) {
        t = have._type;
    }
    panicdottypeE(t, _addr_want, _addr_iface);

}

// panicnildottype is called when doing a i.(T) conversion and the interface i is nil.
// want = the static type we're trying to convert to.
private static void panicnildottype(ptr<_type> _addr_want) => func((_, panic, _) => {
    ref _type want = ref _addr_want.val;

    panic(addr(new TypeAssertionError(nil,nil,want,""))); 
    // TODO: Add the static type we're converting from as well.
    // It might generate a better error message.
    // Just to match other nil conversion errors, we don't for now.
});

// The specialized convTx routines need a type descriptor to use when calling mallocgc.
// We don't need the type to be exact, just to have the correct size, alignment, and pointer-ness.
// However, when debugging, it'd be nice to have some indication in mallocgc where the types came from,
// so we use named types here.
// We then construct interface values of these types,
// and then extract the type word to use as needed.
private partial struct uint16InterfacePtr { // : ushort
}
private partial struct uint32InterfacePtr { // : uint
}
private partial struct uint64InterfacePtr { // : ulong
}
private partial struct stringInterfacePtr { // : @string
}
private partial struct sliceInterfacePtr { // : slice<byte>
}
private static var uint16Eface = uint16InterfacePtr(0);private static var uint32Eface = uint32InterfacePtr(0);private static var uint64Eface = uint64InterfacePtr(0);private static var stringEface = stringInterfacePtr("");private static var sliceEface = sliceInterfacePtr(null);private static ptr<_type> uint16TypeefaceOf(_addr_uint16Eface)._type;private static ptr<_type> uint32TypeefaceOf(_addr_uint32Eface)._type;private static ptr<_type> uint64TypeefaceOf(_addr_uint64Eface)._type;private static ptr<_type> stringTypeefaceOf(_addr_stringEface)._type;private static ptr<_type> sliceTypeefaceOf(_addr_sliceEface)._type;

// The conv and assert functions below do very similar things.
// The convXXX functions are guaranteed by the compiler to succeed.
// The assertXXX functions may fail (either panicking or returning false,
// depending on whether they are 1-result or 2-result).
// The convXXX functions succeed on a nil input, whereas the assertXXX
// functions fail on a nil input.

private static eface convT2E(ptr<_type> _addr_t, unsafe.Pointer elem) {
    eface e = default;
    ref _type t = ref _addr_t.val;

    if (raceenabled) {
        raceReadObjectPC(t, elem, getcallerpc(), funcPC(convT2E));
    }
    if (msanenabled) {
        msanread(elem, t.size);
    }
    var x = mallocgc(t.size, t, true); 
    // TODO: We allocate a zeroed object only to overwrite it with actual data.
    // Figure out how to avoid zeroing. Also below in convT2Eslice, convT2I, convT2Islice.
    typedmemmove(t, x, elem);
    e._type = t;
    e.data = x;
    return ;

}

private static unsafe.Pointer convT16(ushort val) {
    unsafe.Pointer x = default;

    if (val < uint16(len(staticuint64s))) {
        x = @unsafe.Pointer(_addr_staticuint64s[val]);
        if (sys.BigEndian) {
            x = add(x, 6);
        }
    }
    else
 {
        x = mallocgc(2, uint16Type, false) * (uint16.val)(x);

        val;

    }
    return ;

}

private static unsafe.Pointer convT32(uint val) {
    unsafe.Pointer x = default;

    if (val < uint32(len(staticuint64s))) {
        x = @unsafe.Pointer(_addr_staticuint64s[val]);
        if (sys.BigEndian) {
            x = add(x, 4);
        }
    }
    else
 {
        x = mallocgc(4, uint32Type, false) * (uint32.val)(x);

        val;

    }
    return ;

}

private static unsafe.Pointer convT64(ulong val) {
    unsafe.Pointer x = default;

    if (val < uint64(len(staticuint64s))) {
        x = @unsafe.Pointer(_addr_staticuint64s[val]);
    }
    else
 {
        x = mallocgc(8, uint64Type, false) * (uint64.val)(x);

        val;

    }
    return ;

}

private static unsafe.Pointer convTstring(@string val) {
    unsafe.Pointer x = default;

    if (val == "") {
        x = @unsafe.Pointer(_addr_zeroVal[0]);
    }
    else
 {
        x = mallocgc(@unsafe.Sizeof(val), stringType, true) * (string.val)(x);

        val;

    }
    return ;

}

private static unsafe.Pointer convTslice(slice<byte> val) {
    unsafe.Pointer x = default;
 
    // Note: this must work for any element type, not just byte.
    if ((slice.val)(@unsafe.Pointer(_addr_val)).array == null) {
        x = @unsafe.Pointer(_addr_zeroVal[0]);
    }
    else
 {
        x = mallocgc(@unsafe.Sizeof(val), sliceType, true);
        new ptr<ptr<ptr<slice<byte>>>>(x) = val;
    }
    return ;

}

private static eface convT2Enoptr(ptr<_type> _addr_t, unsafe.Pointer elem) {
    eface e = default;
    ref _type t = ref _addr_t.val;

    if (raceenabled) {
        raceReadObjectPC(t, elem, getcallerpc(), funcPC(convT2Enoptr));
    }
    if (msanenabled) {
        msanread(elem, t.size);
    }
    var x = mallocgc(t.size, t, false);
    memmove(x, elem, t.size);
    e._type = t;
    e.data = x;
    return ;

}

private static iface convT2I(ptr<itab> _addr_tab, unsafe.Pointer elem) {
    iface i = default;
    ref itab tab = ref _addr_tab.val;

    var t = tab._type;
    if (raceenabled) {
        raceReadObjectPC(t, elem, getcallerpc(), funcPC(convT2I));
    }
    if (msanenabled) {
        msanread(elem, t.size);
    }
    var x = mallocgc(t.size, t, true);
    typedmemmove(t, x, elem);
    i.tab = tab;
    i.data = x;
    return ;

}

private static iface convT2Inoptr(ptr<itab> _addr_tab, unsafe.Pointer elem) {
    iface i = default;
    ref itab tab = ref _addr_tab.val;

    var t = tab._type;
    if (raceenabled) {
        raceReadObjectPC(t, elem, getcallerpc(), funcPC(convT2Inoptr));
    }
    if (msanenabled) {
        msanread(elem, t.size);
    }
    var x = mallocgc(t.size, t, false);
    memmove(x, elem, t.size);
    i.tab = tab;
    i.data = x;
    return ;

}

private static iface convI2I(ptr<interfacetype> _addr_inter, iface i) {
    iface r = default;
    ref interfacetype inter = ref _addr_inter.val;

    var tab = i.tab;
    if (tab == null) {
        return ;
    }
    if (tab.inter == inter) {
        r.tab = tab;
        r.data = i.data;
        return ;
    }
    r.tab = getitab(_addr_inter, _addr_tab._type, false);
    r.data = i.data;
    return ;

}

private static ptr<itab> assertI2I(ptr<interfacetype> _addr_inter, ptr<itab> _addr_tab) => func((_, panic, _) => {
    ref interfacetype inter = ref _addr_inter.val;
    ref itab tab = ref _addr_tab.val;

    if (tab == null) { 
        // explicit conversions require non-nil interface value.
        panic(addr(new TypeAssertionError(nil,nil,&inter.typ,"")));

    }
    if (tab.inter == inter) {
        return _addr_tab!;
    }
    return _addr_getitab(_addr_inter, _addr_tab._type, false)!;

});

private static iface assertI2I2(ptr<interfacetype> _addr_inter, iface i) {
    iface r = default;
    ref interfacetype inter = ref _addr_inter.val;

    var tab = i.tab;
    if (tab == null) {
        return ;
    }
    if (tab.inter != inter) {
        tab = getitab(_addr_inter, _addr_tab._type, true);
        if (tab == null) {
            return ;
        }
    }
    r.tab = tab;
    r.data = i.data;
    return ;

}

private static ptr<itab> assertE2I(ptr<interfacetype> _addr_inter, ptr<_type> _addr_t) => func((_, panic, _) => {
    ref interfacetype inter = ref _addr_inter.val;
    ref _type t = ref _addr_t.val;

    if (t == null) { 
        // explicit conversions require non-nil interface value.
        panic(addr(new TypeAssertionError(nil,nil,&inter.typ,"")));

    }
    return _addr_getitab(_addr_inter, _addr_t, false)!;

});

private static iface assertE2I2(ptr<interfacetype> _addr_inter, eface e) {
    iface r = default;
    ref interfacetype inter = ref _addr_inter.val;

    var t = e._type;
    if (t == null) {
        return ;
    }
    var tab = getitab(_addr_inter, _addr_t, true);
    if (tab == null) {
        return ;
    }
    r.tab = tab;
    r.data = e.data;
    return ;

}

//go:linkname reflect_ifaceE2I reflect.ifaceE2I
private static void reflect_ifaceE2I(ptr<interfacetype> _addr_inter, eface e, ptr<iface> _addr_dst) {
    ref interfacetype inter = ref _addr_inter.val;
    ref iface dst = ref _addr_dst.val;

    dst = new iface(assertE2I(inter,e._type),e.data);
}

//go:linkname reflectlite_ifaceE2I internal/reflectlite.ifaceE2I
private static void reflectlite_ifaceE2I(ptr<interfacetype> _addr_inter, eface e, ptr<iface> _addr_dst) {
    ref interfacetype inter = ref _addr_inter.val;
    ref iface dst = ref _addr_dst.val;

    dst = new iface(assertE2I(inter,e._type),e.data);
}

private static void iterate_itabs(Action<ptr<itab>> fn) { 
    // Note: only runs during stop the world or with itabLock held,
    // so no other locks/atomics needed.
    var t = itabTable;
    for (var i = uintptr(0); i < t.size; i++) {
        ptr<ptr<ptr<itab>>> m = new ptr<ptr<ptr<ptr<itab>>>>(add(@unsafe.Pointer(_addr_t.entries), i * sys.PtrSize));
        if (m != null) {
            fn(m);
        }
    }

}

// staticuint64s is used to avoid allocating in convTx for small integer values.
private static array<ulong> staticuint64s = new array<ulong>(new ulong[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f, 0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae, 0xaf, 0xb0, 0xb1, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf, 0xc0, 0xc1, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xcb, 0xcc, 0xcd, 0xce, 0xcf, 0xd0, 0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xdb, 0xdc, 0xdd, 0xde, 0xdf, 0xe0, 0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xeb, 0xec, 0xed, 0xee, 0xef, 0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff });

// The linker redirects a reference of a method that it determined
// unreachable to a reference to this function, so it will throw if
// ever called.
private static void unreachableMethod() {
    throw("unreachable method called. linker bug?");
}

} // end runtime_package
