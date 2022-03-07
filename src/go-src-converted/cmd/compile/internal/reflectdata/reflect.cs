// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflectdata -- go2cs converted at 2022 March 06 23:09:06 UTC
// import "cmd/compile/internal/reflectdata" ==> using reflectdata = go.cmd.compile.@internal.reflectdata_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\reflectdata\reflect.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.compile.@internal.@base_package;
using bitvec = go.cmd.compile.@internal.bitvec_package;
using escape = go.cmd.compile.@internal.escape_package;
using inline = go.cmd.compile.@internal.inline_package;
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using typebits = go.cmd.compile.@internal.typebits_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using gcprog = go.cmd.@internal.gcprog_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class reflectdata_package {

private partial struct itabEntry {
    public ptr<types.Type> t;
    public ptr<types.Type> itype;
    public ptr<obj.LSym> lsym; // symbol of the itab itself

// symbols of each method in
// the itab, sorted by byte offset;
// filled in by CompileITabs
    public slice<ptr<obj.LSym>> entries;
}

private partial struct ptabEntry {
    public ptr<types.Sym> s;
    public ptr<types.Type> t;
}

public static (nint, nint) CountTabs() {
    nint numPTabs = default;
    nint numITabs = default;

    return (len(ptabs), len(itabs));
}

// runtime interface and reflection data structures
private static sync.Mutex signatmu = default;private static var signatset = make();private static slice<ptr<types.Type>> signatslice = default;private static sync.Mutex gcsymmu = default;private static var gcsymset = make();private static slice<itabEntry> itabs = default;private static slice<ptr<ir.Name>> ptabs = default;

private partial struct typeSig {
    public ptr<types.Sym> name;
    public ptr<obj.LSym> isym;
    public ptr<obj.LSym> tsym;
    public ptr<types.Type> type_;
    public ptr<types.Type> mtype;
}

// Builds a type representing a Bucket structure for
// the given map type. This type is not visible to users -
// we include only enough information to generate a correct GC
// program for it.
// Make sure this stays in sync with runtime/map.go.
public static readonly nint BUCKETSIZE = 8;
public static readonly nint MAXKEYSIZE = 128;
public static readonly nint MAXELEMSIZE = 128;


private static nint structfieldSize() {
    return 3 * types.PtrSize;
} // Sizeof(runtime.structfield{})
private static nint imethodSize() {
    return 4 + 4;
} // Sizeof(runtime.imethod{})
private static nint commonSize() {
    return 4 * types.PtrSize + 8 + 8;
} // Sizeof(runtime._type{})

private static nint uncommonSize(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 // Sizeof(runtime.uncommontype{})
    if (t.Sym() == null && len(methods(_addr_t)) == 0) {
        return 0;
    }
    return 4 + 2 + 2 + 4 + 4;

}

private static ptr<types.Field> makefield(@string name, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var sym = (types.Pkg.val)(null).Lookup(name);
    return _addr_types.NewField(src.NoXPos, sym, t)!;
}

// MapBucketType makes the map bucket type given the type of the map.
public static ptr<types.Type> MapBucketType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.MapType().Bucket != null) {
        return _addr_t.MapType().Bucket!;
    }
    var keytype = t.Key();
    var elemtype = t.Elem();
    types.CalcSize(keytype);
    types.CalcSize(elemtype);
    if (keytype.Width > MAXKEYSIZE) {
        keytype = types.NewPtr(keytype);
    }
    if (elemtype.Width > MAXELEMSIZE) {
        elemtype = types.NewPtr(elemtype);
    }
    var field = make_slice<ptr<types.Field>>(0, 5); 

    // The first field is: uint8 topbits[BUCKETSIZE].
    var arr = types.NewArray(types.Types[types.TUINT8], BUCKETSIZE);
    field = append(field, makefield("topbits", _addr_arr));

    arr = types.NewArray(keytype, BUCKETSIZE);
    arr.SetNoalg(true);
    var keys = makefield("keys", _addr_arr);
    field = append(field, keys);

    arr = types.NewArray(elemtype, BUCKETSIZE);
    arr.SetNoalg(true);
    var elems = makefield("elems", _addr_arr);
    field = append(field, elems); 

    // If keys and elems have no pointers, the map implementation
    // can keep a list of overflow pointers on the side so that
    // buckets can be marked as having no pointers.
    // Arrange for the bucket to have no pointers by changing
    // the type of the overflow field to uintptr in this case.
    // See comment on hmap.overflow in runtime/map.go.
    var otyp = types.Types[types.TUNSAFEPTR];
    if (!elemtype.HasPointers() && !keytype.HasPointers()) {
        otyp = types.Types[types.TUINTPTR];
    }
    var overflow = makefield("overflow", _addr_otyp);
    field = append(field, overflow); 

    // link up fields
    var bucket = types.NewStruct(types.NoPkg, field[..]);
    bucket.SetNoalg(true);
    types.CalcSize(bucket); 

    // Check invariants that map code depends on.
    if (!types.IsComparable(t.Key())) {
        @base.Fatalf("unsupported map key type for %v", t);
    }
    if (BUCKETSIZE < 8) {
        @base.Fatalf("bucket size too small for proper alignment");
    }
    if (keytype.Align > BUCKETSIZE) {
        @base.Fatalf("key align too big for %v", t);
    }
    if (elemtype.Align > BUCKETSIZE) {
        @base.Fatalf("elem align too big for %v", t);
    }
    if (keytype.Width > MAXKEYSIZE) {
        @base.Fatalf("key size to large for %v", t);
    }
    if (elemtype.Width > MAXELEMSIZE) {
        @base.Fatalf("elem size to large for %v", t);
    }
    if (t.Key().Width > MAXKEYSIZE && !keytype.IsPtr()) {
        @base.Fatalf("key indirect incorrect for %v", t);
    }
    if (t.Elem().Width > MAXELEMSIZE && !elemtype.IsPtr()) {
        @base.Fatalf("elem indirect incorrect for %v", t);
    }
    if (keytype.Width % int64(keytype.Align) != 0) {
        @base.Fatalf("key size not a multiple of key align for %v", t);
    }
    if (elemtype.Width % int64(elemtype.Align) != 0) {
        @base.Fatalf("elem size not a multiple of elem align for %v", t);
    }
    if (bucket.Align % keytype.Align != 0) {
        @base.Fatalf("bucket align not multiple of key align %v", t);
    }
    if (bucket.Align % elemtype.Align != 0) {
        @base.Fatalf("bucket align not multiple of elem align %v", t);
    }
    if (keys.Offset % int64(keytype.Align) != 0) {
        @base.Fatalf("bad alignment of keys in bmap for %v", t);
    }
    if (elems.Offset % int64(elemtype.Align) != 0) {
        @base.Fatalf("bad alignment of elems in bmap for %v", t);
    }
    if (overflow.Offset != bucket.Width - int64(types.PtrSize)) {
        @base.Fatalf("bad offset of overflow in bmap for %v", t);
    }
    t.MapType().Bucket = bucket;

    bucket.StructType().Map = t;
    return _addr_bucket!;

}

// MapType builds a type representing a Hmap structure for the given map type.
// Make sure this stays in sync with runtime/map.go.
public static ptr<types.Type> MapType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.MapType().Hmap != null) {
        return _addr_t.MapType().Hmap!;
    }
    var bmap = MapBucketType(_addr_t); 

    // build a struct:
    // type hmap struct {
    //    count      int
    //    flags      uint8
    //    B          uint8
    //    noverflow  uint16
    //    hash0      uint32
    //    buckets    *bmap
    //    oldbuckets *bmap
    //    nevacuate  uintptr
    //    extra      unsafe.Pointer // *mapextra
    // }
    // must match runtime/map.go:hmap.
    ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("count",types.Types[types.TINT]), makefield("flags",types.Types[types.TUINT8]), makefield("B",types.Types[types.TUINT8]), makefield("noverflow",types.Types[types.TUINT16]), makefield("hash0",types.Types[types.TUINT32]), makefield("buckets",types.NewPtr(bmap)), makefield("oldbuckets",types.NewPtr(bmap)), makefield("nevacuate",types.Types[types.TUINTPTR]), makefield("extra",types.Types[types.TUNSAFEPTR]) });

    var hmap = types.NewStruct(types.NoPkg, fields);
    hmap.SetNoalg(true);
    types.CalcSize(hmap); 

    // The size of hmap should be 48 bytes on 64 bit
    // and 28 bytes on 32 bit platforms.
    {
        var size = int64(8 + 5 * types.PtrSize);

        if (hmap.Width != size) {
            @base.Fatalf("hmap size not correct: got %d, want %d", hmap.Width, size);
        }
    }


    t.MapType().Hmap = hmap;
    hmap.StructType().Map = t;
    return _addr_hmap!;

}

// MapIterType builds a type representing an Hiter structure for the given map type.
// Make sure this stays in sync with runtime/map.go.
public static ptr<types.Type> MapIterType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.MapType().Hiter != null) {
        return _addr_t.MapType().Hiter!;
    }
    var hmap = MapType(_addr_t);
    var bmap = MapBucketType(_addr_t); 

    // build a struct:
    // type hiter struct {
    //    key         *Key
    //    elem        *Elem
    //    t           unsafe.Pointer // *MapType
    //    h           *hmap
    //    buckets     *bmap
    //    bptr        *bmap
    //    overflow    unsafe.Pointer // *[]*bmap
    //    oldoverflow unsafe.Pointer // *[]*bmap
    //    startBucket uintptr
    //    offset      uint8
    //    wrapped     bool
    //    B           uint8
    //    i           uint8
    //    bucket      uintptr
    //    checkBucket uintptr
    // }
    // must match runtime/map.go:hiter.
    ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("key",types.NewPtr(t.Key())), makefield("elem",types.NewPtr(t.Elem())), makefield("t",types.Types[types.TUNSAFEPTR]), makefield("h",types.NewPtr(hmap)), makefield("buckets",types.NewPtr(bmap)), makefield("bptr",types.NewPtr(bmap)), makefield("overflow",types.Types[types.TUNSAFEPTR]), makefield("oldoverflow",types.Types[types.TUNSAFEPTR]), makefield("startBucket",types.Types[types.TUINTPTR]), makefield("offset",types.Types[types.TUINT8]), makefield("wrapped",types.Types[types.TBOOL]), makefield("B",types.Types[types.TUINT8]), makefield("i",types.Types[types.TUINT8]), makefield("bucket",types.Types[types.TUINTPTR]), makefield("checkBucket",types.Types[types.TUINTPTR]) }); 

    // build iterator struct holding the above fields
    var hiter = types.NewStruct(types.NoPkg, fields);
    hiter.SetNoalg(true);
    types.CalcSize(hiter);
    if (hiter.Width != int64(12 * types.PtrSize)) {
        @base.Fatalf("hash_iter size not correct %d %d", hiter.Width, 12 * types.PtrSize);
    }
    t.MapType().Hiter = hiter;
    hiter.StructType().Map = t;
    return _addr_hiter!;

}

// methods returns the methods of the non-interface type t, sorted by name.
// Generates stub functions as needed.
private static slice<ptr<typeSig>> methods(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 
    // method type
    var mt = types.ReceiverBaseType(t);

    if (mt == null) {
        return null;
    }
    typecheck.CalcMethods(mt); 

    // type stored in interface word
    var it = t;

    if (!types.IsDirectIface(it)) {
        it = types.NewPtr(t);
    }
    slice<ptr<typeSig>> ms = default;
    foreach (var (_, f) in mt.AllMethods().Slice()) {
        if (f.Sym == null) {
            @base.Fatalf("method with no sym on %v", mt);
        }
        if (!f.IsMethod()) {
            @base.Fatalf("non-method on %v method %v %v", mt, f.Sym, f);
        }
        if (f.Type.Recv() == null) {
            @base.Fatalf("receiver with no type on %v method %v %v", mt, f.Sym, f);
        }
        if (f.Nointerface()) {
            continue;
        }
        if (!types.IsMethodApplicable(t, f)) {
            continue;
        }
        ptr<typeSig> sig = addr(new typeSig(name:f.Sym,isym:methodWrapper(it,f),tsym:methodWrapper(t,f),type_:typecheck.NewMethodType(f.Type,t),mtype:typecheck.NewMethodType(f.Type,nil),));
        ms = append(ms, sig);

    }    return ms;

}

// imethods returns the methods of the interface type t, sorted by name.
private static slice<ptr<typeSig>> imethods(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    slice<ptr<typeSig>> methods = default;
    foreach (var (_, f) in t.AllMethods().Slice()) {
        if (f.Type.Kind() != types.TFUNC || f.Sym == null) {
            continue;
        }
        if (f.Sym.IsBlank()) {
            @base.Fatalf("unexpected blank symbol in interface method set");
        }
        {
            var n = len(methods);

            if (n > 0) {
                var last = methods[n - 1];
                if (!last.name.Less(f.Sym)) {
                    @base.Fatalf("sigcmp vs sortinter %v %v", last.name, f.Sym);
                }
            }

        }


        ptr<typeSig> sig = addr(new typeSig(name:f.Sym,mtype:f.Type,type_:typecheck.NewMethodType(f.Type,nil),));
        methods = append(methods, sig); 

        // NOTE(rsc): Perhaps an oversight that
        // IfaceType.Method is not in the reflect data.
        // Generate the method body, so that compiled
        // code can refer to it.
        methodWrapper(_addr_t, _addr_f);

    }    return methods;

}

private static void dimportpath(ptr<types.Pkg> _addr_p) {
    ref types.Pkg p = ref _addr_p.val;

    if (p.Pathsym != null) {
        return ;
    }
    if (@base.Ctxt.Pkgpath == "runtime" && p == ir.Pkgs.Runtime) {
        return ;
    }
    var str = p.Path;
    if (p == types.LocalPkg) { 
        // Note: myimportpath != "", or else dgopkgpath won't call dimportpath.
        str = @base.Ctxt.Pkgpath;

    }
    var s = @base.Ctxt.Lookup("type..importpath." + p.Prefix + ".");
    var ot = dnameData(_addr_s, 0, str, "", _addr_null, false);
    objw.Global(s, int32(ot), obj.DUPOK | obj.RODATA);
    s.Set(obj.AttrContentAddressable, true);
    p.Pathsym = s;

}

private static nint dgopkgpath(ptr<obj.LSym> _addr_s, nint ot, ptr<types.Pkg> _addr_pkg) {
    ref obj.LSym s = ref _addr_s.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    if (pkg == null) {
        return objw.Uintptr(s, ot, 0);
    }
    if (pkg == types.LocalPkg && @base.Ctxt.Pkgpath == "") { 
        // If we don't know the full import path of the package being compiled
        // (i.e. -p was not passed on the compiler command line), emit a reference to
        // type..importpath.""., which the linker will rewrite using the correct import path.
        // Every package that imports this one directly defines the symbol.
        // See also https://groups.google.com/forum/#!topic/golang-dev/myb9s53HxGQ.
        var ns = @base.Ctxt.Lookup("type..importpath.\"\".");
        return objw.SymPtr(s, ot, ns, 0);

    }
    dimportpath(_addr_pkg);
    return objw.SymPtr(s, ot, pkg.Pathsym, 0);

}

// dgopkgpathOff writes an offset relocation in s at offset ot to the pkg path symbol.
private static nint dgopkgpathOff(ptr<obj.LSym> _addr_s, nint ot, ptr<types.Pkg> _addr_pkg) {
    ref obj.LSym s = ref _addr_s.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    if (pkg == null) {
        return objw.Uint32(s, ot, 0);
    }
    if (pkg == types.LocalPkg && @base.Ctxt.Pkgpath == "") { 
        // If we don't know the full import path of the package being compiled
        // (i.e. -p was not passed on the compiler command line), emit a reference to
        // type..importpath.""., which the linker will rewrite using the correct import path.
        // Every package that imports this one directly defines the symbol.
        // See also https://groups.google.com/forum/#!topic/golang-dev/myb9s53HxGQ.
        var ns = @base.Ctxt.Lookup("type..importpath.\"\".");
        return objw.SymPtrOff(s, ot, ns);

    }
    dimportpath(_addr_pkg);
    return objw.SymPtrOff(s, ot, pkg.Pathsym);

}

// dnameField dumps a reflect.name for a struct field.
private static nint dnameField(ptr<obj.LSym> _addr_lsym, nint ot, ptr<types.Pkg> _addr_spkg, ptr<types.Field> _addr_ft) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Pkg spkg = ref _addr_spkg.val;
    ref types.Field ft = ref _addr_ft.val;

    if (!types.IsExported(ft.Sym.Name) && ft.Sym.Pkg != spkg) {
        @base.Fatalf("package mismatch for %v", ft.Sym);
    }
    var nsym = dname(ft.Sym.Name, ft.Note, _addr_null, types.IsExported(ft.Sym.Name));
    return objw.SymPtr(lsym, ot, nsym, 0);

}

// dnameData writes the contents of a reflect.name into s at offset ot.
private static nint dnameData(ptr<obj.LSym> _addr_s, nint ot, @string name, @string tag, ptr<types.Pkg> _addr_pkg, bool exported) {
    ref obj.LSym s = ref _addr_s.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    if (len(name) >= 1 << 29) {
        @base.Fatalf("name too long: %d %s...", len(name), name[..(int)1024]);
    }
    if (len(tag) >= 1 << 29) {
        @base.Fatalf("tag too long: %d %s...", len(tag), tag[..(int)1024]);
    }
    array<byte> nameLen = new array<byte>(binary.MaxVarintLen64);
    var nameLenLen = binary.PutUvarint(nameLen[..], uint64(len(name)));
    array<byte> tagLen = new array<byte>(binary.MaxVarintLen64);
    var tagLenLen = binary.PutUvarint(tagLen[..], uint64(len(tag))); 

    // Encode name and tag. See reflect/type.go for details.
    byte bits = default;
    nint l = 1 + nameLenLen + len(name);
    if (exported) {
        bits |= 1 << 0;
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= 1 << 1;
    }
    if (pkg != null) {
        bits |= 1 << 2;
    }
    var b = make_slice<byte>(l);
    b[0] = bits;
    copy(b[(int)1..], nameLen[..(int)nameLenLen]);
    copy(b[(int)1 + nameLenLen..], name);
    if (len(tag) > 0) {
        var tb = b[(int)1 + nameLenLen + len(name)..];
        copy(tb, tagLen[..(int)tagLenLen]);
        copy(tb[(int)tagLenLen..], tag);
    }
    ot = int(s.WriteBytes(@base.Ctxt, int64(ot), b));

    if (pkg != null) {
        ot = dgopkgpathOff(_addr_s, ot, _addr_pkg);
    }
    return ot;

}

private static nint dnameCount = default;

// dname creates a reflect.name for a struct field or method.
private static ptr<obj.LSym> dname(@string name, @string tag, ptr<types.Pkg> _addr_pkg, bool exported) {
    ref types.Pkg pkg = ref _addr_pkg.val;
 
    // Write out data as "type.." to signal two things to the
    // linker, first that when dynamically linking, the symbol
    // should be moved to a relro section, and second that the
    // contents should not be decoded as a type.
    @string sname = "type..namedata.";
    if (pkg == null) { 
        // In the common case, share data with other packages.
        if (name == "") {
            if (exported) {
                sname += "-noname-exported." + tag;
            }
            else
 {
                sname += "-noname-unexported." + tag;
            }

        }
        else
 {
            if (exported) {
                sname += name + "." + tag;
            }
            else
 {
                sname += name + "-" + tag;
            }

        }
    }
    else
 {
        sname = fmt.Sprintf("%s\"\".%d", sname, dnameCount);
        dnameCount++;
    }
    var s = @base.Ctxt.Lookup(sname);
    if (len(s.P) > 0) {
        return _addr_s!;
    }
    var ot = dnameData(_addr_s, 0, name, tag, _addr_pkg, exported);
    objw.Global(s, int32(ot), obj.DUPOK | obj.RODATA);
    s.Set(obj.AttrContentAddressable, true);
    return _addr_s!;

}

// dextratype dumps the fields of a runtime.uncommontype.
// dataAdd is the offset in bytes after the header where the
// backing array of the []method field is written (by dextratypeData).
private static nint dextratype(ptr<obj.LSym> _addr_lsym, nint ot, ptr<types.Type> _addr_t, nint dataAdd) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Type t = ref _addr_t.val;

    var m = methods(_addr_t);
    if (t.Sym() == null && len(m) == 0) {
        return ot;
    }
    var noff = int(types.Rnd(int64(ot), int64(types.PtrSize)));
    if (noff != ot) {
        @base.Fatalf("unexpected alignment in dextratype for %v", t);
    }
    foreach (var (_, a) in m) {
        writeType(_addr_a.type_);
    }    ot = dgopkgpathOff(_addr_lsym, ot, _addr_typePkg(_addr_t));

    dataAdd += uncommonSize(_addr_t);
    var mcount = len(m);
    if (mcount != int(uint16(mcount))) {
        @base.Fatalf("too many methods on %v: %d", t, mcount);
    }
    var xcount = sort.Search(mcount, i => !types.IsExported(m[i].name.Name));
    if (dataAdd != int(uint32(dataAdd))) {
        @base.Fatalf("methods are too far away on %v: %d", t, dataAdd);
    }
    ot = objw.Uint16(lsym, ot, uint16(mcount));
    ot = objw.Uint16(lsym, ot, uint16(xcount));
    ot = objw.Uint32(lsym, ot, uint32(dataAdd));
    ot = objw.Uint32(lsym, ot, 0);
    return ot;

}

private static ptr<types.Pkg> typePkg(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var tsym = t.Sym();
    if (tsym == null) {

        if (t.Kind() == types.TARRAY || t.Kind() == types.TSLICE || t.Kind() == types.TPTR || t.Kind() == types.TCHAN) 
            if (t.Elem() != null) {
                tsym = t.Elem().Sym();
            }
        
    }
    if (tsym != null && tsym.Pkg != types.BuiltinPkg) {
        return _addr_tsym.Pkg!;
    }
    return _addr_null!;

}

// dextratypeData dumps the backing array for the []method field of
// runtime.uncommontype.
private static nint dextratypeData(ptr<obj.LSym> _addr_lsym, nint ot, ptr<types.Type> _addr_t) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Type t = ref _addr_t.val;

    foreach (var (_, a) in methods(_addr_t)) { 
        // ../../../../runtime/type.go:/method
        var exported = types.IsExported(a.name.Name);
        ptr<types.Pkg> pkg;
        if (!exported && a.name.Pkg != typePkg(_addr_t)) {
            pkg = a.name.Pkg;
        }
        var nsym = dname(a.name.Name, "", pkg, exported);

        ot = objw.SymPtrOff(lsym, ot, nsym);
        ot = dmethodptrOff(_addr_lsym, ot, _addr_writeType(_addr_a.mtype));
        ot = dmethodptrOff(_addr_lsym, ot, _addr_a.isym);
        ot = dmethodptrOff(_addr_lsym, ot, _addr_a.tsym);

    }    return ot;

}

private static nint dmethodptrOff(ptr<obj.LSym> _addr_s, nint ot, ptr<obj.LSym> _addr_x) {
    ref obj.LSym s = ref _addr_s.val;
    ref obj.LSym x = ref _addr_x.val;

    objw.Uint32(s, ot, 0);
    var r = obj.Addrel(s);
    r.Off = int32(ot);
    r.Siz = 4;
    r.Sym = x;
    r.Type = objabi.R_METHODOFF;
    return ot + 4;
}

private static nint kinds = new slice<nint>(InitKeyedValues<nint>((types.TINT, objabi.KindInt), (types.TUINT, objabi.KindUint), (types.TINT8, objabi.KindInt8), (types.TUINT8, objabi.KindUint8), (types.TINT16, objabi.KindInt16), (types.TUINT16, objabi.KindUint16), (types.TINT32, objabi.KindInt32), (types.TUINT32, objabi.KindUint32), (types.TINT64, objabi.KindInt64), (types.TUINT64, objabi.KindUint64), (types.TUINTPTR, objabi.KindUintptr), (types.TFLOAT32, objabi.KindFloat32), (types.TFLOAT64, objabi.KindFloat64), (types.TBOOL, objabi.KindBool), (types.TSTRING, objabi.KindString), (types.TPTR, objabi.KindPtr), (types.TSTRUCT, objabi.KindStruct), (types.TINTER, objabi.KindInterface), (types.TCHAN, objabi.KindChan), (types.TMAP, objabi.KindMap), (types.TARRAY, objabi.KindArray), (types.TSLICE, objabi.KindSlice), (types.TFUNC, objabi.KindFunc), (types.TCOMPLEX64, objabi.KindComplex64), (types.TCOMPLEX128, objabi.KindComplex128), (types.TUNSAFEPTR, objabi.KindUnsafePointer)));

// tflag is documented in reflect/type.go.
//
// tflag values must be kept in sync with copies in:
//    cmd/compile/internal/reflectdata/reflect.go
//    cmd/link/internal/ld/decodesym.go
//    reflect/type.go
//    runtime/type.go
private static readonly nint tflagUncommon = 1 << 0;
private static readonly nint tflagExtraStar = 1 << 1;
private static readonly nint tflagNamed = 1 << 2;
private static readonly nint tflagRegularMemory = 1 << 3;


private static ptr<obj.LSym> memhashvarlen;private static ptr<obj.LSym> memequalvarlen;

// dcommontype dumps the contents of a reflect.rtype (runtime._type).
private static nint dcommontype(ptr<obj.LSym> _addr_lsym, ptr<types.Type> _addr_t) {
    ref obj.LSym lsym = ref _addr_lsym.val;
    ref types.Type t = ref _addr_t.val;

    types.CalcSize(t);
    var eqfunc = geneq(t);

    var sptrWeak = true;
    ptr<obj.LSym> sptr;
    if (!t.IsPtr() || t.IsPtrElem()) {
        var tptr = types.NewPtr(t);
        if (t.Sym() != null || methods(_addr_tptr) != null) {
            sptrWeak = false;
        }
        sptr = writeType(_addr_tptr);

    }
    var (gcsym, useGCProg, ptrdata) = dgcsym(_addr_t, true);
    delete(gcsymset, t); 

    // ../../../../reflect/type.go:/^type.rtype
    // actual type structure
    //    type rtype struct {
    //        size          uintptr
    //        ptrdata       uintptr
    //        hash          uint32
    //        tflag         tflag
    //        align         uint8
    //        fieldAlign    uint8
    //        kind          uint8
    //        equal         func(unsafe.Pointer, unsafe.Pointer) bool
    //        gcdata        *byte
    //        str           nameOff
    //        ptrToThis     typeOff
    //    }
    nint ot = 0;
    ot = objw.Uintptr(lsym, ot, uint64(t.Width));
    ot = objw.Uintptr(lsym, ot, uint64(ptrdata));
    ot = objw.Uint32(lsym, ot, types.TypeHash(t));

    byte tflag = default;
    if (uncommonSize(_addr_t) != 0) {
        tflag |= tflagUncommon;
    }
    if (t.Sym() != null && t.Sym().Name != "") {
        tflag |= tflagNamed;
    }
    if (isRegularMemory(t)) {
        tflag |= tflagRegularMemory;
    }
    var exported = false;
    var p = t.LongString(); 
    // If we're writing out type T,
    // we are very likely to write out type *T as well.
    // Use the string "*T"[1:] for "T", so that the two
    // share storage. This is a cheap way to reduce the
    // amount of space taken up by reflect strings.
    if (!strings.HasPrefix(p, "*")) {
        p = "*" + p;
        tflag |= tflagExtraStar;
        if (t.Sym() != null) {
            exported = types.IsExported(t.Sym().Name);
        }
    }
    else
 {
        if (t.Elem() != null && t.Elem().Sym() != null) {
            exported = types.IsExported(t.Elem().Sym().Name);
        }
    }
    ot = objw.Uint8(lsym, ot, tflag); 

    // runtime (and common sense) expects alignment to be a power of two.
    var i = int(t.Align);

    if (i == 0) {
        i = 1;
    }
    if (i & (i - 1) != 0) {
        @base.Fatalf("invalid alignment %d for %v", t.Align, t);
    }
    ot = objw.Uint8(lsym, ot, t.Align); // align
    ot = objw.Uint8(lsym, ot, t.Align); // fieldAlign

    i = kinds[t.Kind()];
    if (types.IsDirectIface(t)) {
        i |= objabi.KindDirectIface;
    }
    if (useGCProg) {
        i |= objabi.KindGCProg;
    }
    ot = objw.Uint8(lsym, ot, uint8(i)); // kind
    if (eqfunc != null) {
        ot = objw.SymPtr(lsym, ot, eqfunc, 0); // equality function
    }
    else
 {
        ot = objw.Uintptr(lsym, ot, 0); // type we can't do == with
    }
    ot = objw.SymPtr(lsym, ot, gcsym, 0); // gcdata

    var nsym = dname(p, "", _addr_null, exported);
    ot = objw.SymPtrOff(lsym, ot, nsym); // str
    // ptrToThis
    if (sptr == null) {
        ot = objw.Uint32(lsym, ot, 0);
    }
    else if (sptrWeak) {
        ot = objw.SymPtrWeakOff(lsym, ot, sptr);
    }
    else
 {
        ot = objw.SymPtrOff(lsym, ot, sptr);
    }
    return ot;

}

// TrackSym returns the symbol for tracking use of field/method f, assumed
// to be a member of struct/interface type t.
public static ptr<obj.LSym> TrackSym(ptr<types.Type> _addr_t, ptr<types.Field> _addr_f) {
    ref types.Type t = ref _addr_t.val;
    ref types.Field f = ref _addr_f.val;

    return _addr_@base.PkgLinksym("go.track", t.ShortString() + "." + f.Sym.Name, obj.ABI0)!;
}

public static ptr<types.Sym> TypeSymPrefix(@string prefix, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var p = prefix + "." + t.ShortString();
    var s = types.TypeSymLookup(p); 

    // This function is for looking up type-related generated functions
    // (e.g. eq and hash). Make sure they are indeed generated.
    signatmu.Lock();
    NeedRuntimeType(_addr_t);
    signatmu.Unlock(); 

    //print("algsym: %s -> %+S\n", p, s);

    return _addr_s!;

}

public static ptr<types.Sym> TypeSym(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t == null || (t.IsPtr() && t.Elem() == null) || t.IsUntyped()) {
        @base.Fatalf("TypeSym %v", t);
    }
    if (t.Kind() == types.TFUNC && t.Recv() != null) {
        @base.Fatalf("misuse of method type: %v", t);
    }
    var s = types.TypeSym(t);
    signatmu.Lock();
    NeedRuntimeType(_addr_t);
    signatmu.Unlock();
    return _addr_s!;

}

public static ptr<obj.LSym> TypeLinksymPrefix(@string prefix, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return _addr_TypeSymPrefix(prefix, _addr_t).Linksym()!;
}

public static ptr<obj.LSym> TypeLinksymLookup(@string name) {
    return _addr_types.TypeSymLookup(name).Linksym()!;
}

public static ptr<obj.LSym> TypeLinksym(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return _addr_TypeSym(_addr_t).Linksym()!;
}

public static ptr<ir.AddrExpr> TypePtr(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var n = ir.NewLinksymExpr(@base.Pos, TypeLinksym(_addr_t), types.Types[types.TUINT8]);
    return typecheck.Expr(typecheck.NodAddr(n))._<ptr<ir.AddrExpr>>();
}

public static ptr<ir.AddrExpr> ITabAddr(ptr<types.Type> _addr_t, ptr<types.Type> _addr_itype) {
    ref types.Type t = ref _addr_t.val;
    ref types.Type itype = ref _addr_itype.val;

    if (t == null || (t.IsPtr() && t.Elem() == null) || t.IsUntyped() || !itype.IsInterface() || itype.IsEmptyInterface()) {
        @base.Fatalf("ITabAddr(%v, %v)", t, itype);
    }
    var (s, existed) = ir.Pkgs.Itab.LookupOK(t.ShortString() + "," + itype.ShortString());
    if (!existed) {
        itabs = append(itabs, new itabEntry(t:t,itype:itype,lsym:s.Linksym()));
    }
    var lsym = s.Linksym();
    var n = ir.NewLinksymExpr(@base.Pos, lsym, types.Types[types.TUINT8]);
    return typecheck.Expr(typecheck.NodAddr(n))._<ptr<ir.AddrExpr>>();

}

// needkeyupdate reports whether map updates with t as a key
// need the key to be updated.
private static bool needkeyupdate(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (t.Kind() == types.TBOOL || t.Kind() == types.TINT || t.Kind() == types.TUINT || t.Kind() == types.TINT8 || t.Kind() == types.TUINT8 || t.Kind() == types.TINT16 || t.Kind() == types.TUINT16 || t.Kind() == types.TINT32 || t.Kind() == types.TUINT32 || t.Kind() == types.TINT64 || t.Kind() == types.TUINT64 || t.Kind() == types.TUINTPTR || t.Kind() == types.TPTR || t.Kind() == types.TUNSAFEPTR || t.Kind() == types.TCHAN) 
        return false;
    else if (t.Kind() == types.TFLOAT32 || t.Kind() == types.TFLOAT64 || t.Kind() == types.TCOMPLEX64 || t.Kind() == types.TCOMPLEX128 || t.Kind() == types.TINTER || t.Kind() == types.TSTRING) // strings might have smaller backing stores
        return true;
    else if (t.Kind() == types.TARRAY) 
        return needkeyupdate(_addr_t.Elem());
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            if (needkeyupdate(_addr_t1.Type)) {
                return true;
            }
        }        return false;
    else 
        @base.Fatalf("bad type for map key: %v", t);
        return true;
    
}

// hashMightPanic reports whether the hash of a map key of type t might panic.
private static bool hashMightPanic(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (t.Kind() == types.TINTER) 
        return true;
    else if (t.Kind() == types.TARRAY) 
        return hashMightPanic(_addr_t.Elem());
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            if (hashMightPanic(_addr_t1.Type)) {
                return true;
            }
        }        return false;
    else 
        return false;
    
}

// formalType replaces byte and rune aliases with real types.
// They've been separate internally to make error messages
// better, but we have to merge them in the reflect tables.
private static ptr<types.Type> formalType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t == types.ByteType || t == types.RuneType) {
        return _addr_types.Types[t.Kind()]!;
    }
    return _addr_t!;

}

private static ptr<obj.LSym> writeType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    t = formalType(_addr_t);
    if (t.IsUntyped()) {
        @base.Fatalf("writeType %v", t);
    }
    var s = types.TypeSym(t);
    var lsym = s.Linksym();
    if (s.Siggen()) {
        return _addr_lsym!;
    }
    s.SetSiggen(true); 

    // special case (look for runtime below):
    // when compiling package runtime,
    // emit the type structures for int, float, etc.
    var tbase = t;

    if (t.IsPtr() && t.Sym() == null && t.Elem().Sym() != null) {
        tbase = t.Elem();
    }
    nint dupok = 0;
    if (tbase.Sym() == null) {
        dupok = obj.DUPOK;
    }
    if (@base.Ctxt.Pkgpath != "runtime" || (tbase != types.Types[tbase.Kind()] && tbase != types.ByteType && tbase != types.RuneType && tbase != types.ErrorType)) { // int, float, etc
        // named types from other files are defined only by those files
        if (tbase.Sym() != null && tbase.Sym().Pkg != types.LocalPkg) {
            {
                var i = typecheck.BaseTypeIndex(t);

                if (i >= 0) {
                    lsym.Pkg = tbase.Sym().Pkg.Prefix;
                    lsym.SymIdx = int32(i);
                    lsym.Set(obj.AttrIndexed, true);
                }

            }

            return _addr_lsym!;

        }
        if (tbase.Kind() == types.TFORW) {
            return _addr_lsym!;
        }
    }
    nint ot = 0;

    if (t.Kind() == types.TARRAY) 
        // ../../../../runtime/type.go:/arrayType
        var s1 = writeType(_addr_t.Elem());
        var t2 = types.NewSlice(t.Elem());
        var s2 = writeType(_addr_t2);
        ot = dcommontype(_addr_lsym, _addr_t);
        ot = objw.SymPtr(lsym, ot, s1, 0);
        ot = objw.SymPtr(lsym, ot, s2, 0);
        ot = objw.Uintptr(lsym, ot, uint64(t.NumElem()));
        ot = dextratype(_addr_lsym, ot, _addr_t, 0);
    else if (t.Kind() == types.TSLICE) 
        // ../../../../runtime/type.go:/sliceType
        s1 = writeType(_addr_t.Elem());
        ot = dcommontype(_addr_lsym, _addr_t);
        ot = objw.SymPtr(lsym, ot, s1, 0);
        ot = dextratype(_addr_lsym, ot, _addr_t, 0);
    else if (t.Kind() == types.TCHAN) 
        // ../../../../runtime/type.go:/chanType
        s1 = writeType(_addr_t.Elem());
        ot = dcommontype(_addr_lsym, _addr_t);
        ot = objw.SymPtr(lsym, ot, s1, 0);
        ot = objw.Uintptr(lsym, ot, uint64(t.ChanDir()));
        ot = dextratype(_addr_lsym, ot, _addr_t, 0);
    else if (t.Kind() == types.TFUNC) 
        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Recvs().Fields().Slice()) {
                t1 = __t1;
                writeType(_addr_t1.Type);
            }

            t1 = t1__prev1;
        }

        var isddd = false;
        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Params().Fields().Slice()) {
                t1 = __t1;
                isddd = t1.IsDDD();
                writeType(_addr_t1.Type);
            }

            t1 = t1__prev1;
        }

        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Results().Fields().Slice()) {
                t1 = __t1;
                writeType(_addr_t1.Type);
            }

            t1 = t1__prev1;
        }

        ot = dcommontype(_addr_lsym, _addr_t);
        var inCount = t.NumRecvs() + t.NumParams();
        var outCount = t.NumResults();
        if (isddd) {
            outCount |= 1 << 15;
        }
        ot = objw.Uint16(lsym, ot, uint16(inCount));
        ot = objw.Uint16(lsym, ot, uint16(outCount));
        if (types.PtrSize == 8) {
            ot += 4; // align for *rtype
        }
        var dataAdd = (inCount + t.NumResults()) * types.PtrSize;
        ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd); 

        // Array of rtype pointers follows funcType.
        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Recvs().Fields().Slice()) {
                t1 = __t1;
                ot = objw.SymPtr(lsym, ot, writeType(_addr_t1.Type), 0);
            }

            t1 = t1__prev1;
        }

        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Params().Fields().Slice()) {
                t1 = __t1;
                ot = objw.SymPtr(lsym, ot, writeType(_addr_t1.Type), 0);
            }

            t1 = t1__prev1;
        }

        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in t.Results().Fields().Slice()) {
                t1 = __t1;
                ot = objw.SymPtr(lsym, ot, writeType(_addr_t1.Type), 0);
            }

            t1 = t1__prev1;
        }
    else if (t.Kind() == types.TINTER) 
        var m = imethods(_addr_t);
        var n = len(m);
        {
            var a__prev1 = a;

            foreach (var (_, __a) in m) {
                a = __a;
                writeType(_addr_a.type_);
            } 

            // ../../../../runtime/type.go:/interfaceType

            a = a__prev1;
        }

        ot = dcommontype(_addr_lsym, _addr_t);

        ptr<types.Pkg> tpkg;
        if (t.Sym() != null && t != types.Types[t.Kind()] && t != types.ErrorType) {
            tpkg = t.Sym().Pkg;
        }
        ot = dgopkgpath(_addr_lsym, ot, tpkg);

        ot = objw.SymPtr(lsym, ot, lsym, ot + 3 * types.PtrSize + uncommonSize(_addr_t));
        ot = objw.Uintptr(lsym, ot, uint64(n));
        ot = objw.Uintptr(lsym, ot, uint64(n));
        dataAdd = imethodSize() * n;
        ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd);

        {
            var a__prev1 = a;

            foreach (var (_, __a) in m) {
                a = __a; 
                // ../../../../runtime/type.go:/imethod
                var exported = types.IsExported(a.name.Name);
                ptr<types.Pkg> pkg;
                if (!exported && a.name.Pkg != tpkg) {
                    pkg = a.name.Pkg;
                }

                var nsym = dname(a.name.Name, "", pkg, exported);

                ot = objw.SymPtrOff(lsym, ot, nsym);
                ot = objw.SymPtrOff(lsym, ot, writeType(_addr_a.type_));

            } 

            // ../../../../runtime/type.go:/mapType

            a = a__prev1;
        }
    else if (t.Kind() == types.TMAP) 
        s1 = writeType(_addr_t.Key());
        s2 = writeType(_addr_t.Elem());
        var s3 = writeType(_addr_MapBucketType(_addr_t));
        var hasher = genhash(t.Key());

        ot = dcommontype(_addr_lsym, _addr_t);
        ot = objw.SymPtr(lsym, ot, s1, 0);
        ot = objw.SymPtr(lsym, ot, s2, 0);
        ot = objw.SymPtr(lsym, ot, s3, 0);
        ot = objw.SymPtr(lsym, ot, hasher, 0);
        uint flags = default; 
        // Note: flags must match maptype accessors in ../../../../runtime/type.go
        // and maptype builder in ../../../../reflect/type.go:MapOf.
        if (t.Key().Width > MAXKEYSIZE) {
            ot = objw.Uint8(lsym, ot, uint8(types.PtrSize));
            flags |= 1; // indirect key
        }
        else
 {
            ot = objw.Uint8(lsym, ot, uint8(t.Key().Width));
        }
        if (t.Elem().Width > MAXELEMSIZE) {
            ot = objw.Uint8(lsym, ot, uint8(types.PtrSize));
            flags |= 2; // indirect value
        }
        else
 {
            ot = objw.Uint8(lsym, ot, uint8(t.Elem().Width));
        }
        ot = objw.Uint16(lsym, ot, uint16(MapBucketType(_addr_t).Width));
        if (types.IsReflexive(t.Key())) {
            flags |= 4; // reflexive key
        }
        if (needkeyupdate(_addr_t.Key())) {
            flags |= 8; // need key update
        }
        if (hashMightPanic(_addr_t.Key())) {
            flags |= 16; // hash might panic
        }
        ot = objw.Uint32(lsym, ot, flags);
        ot = dextratype(_addr_lsym, ot, _addr_t, 0);
        {
            var u = t.Underlying();

            if (u != t) { 
                // If t is a named map type, also keep the underlying map
                // type live in the binary. This is important to make sure that
                // a named map and that same map cast to its underlying type via
                // reflection, use the same hash function. See issue 37716.
                var r = obj.Addrel(lsym);
                r.Sym = writeType(_addr_u);
                r.Type = objabi.R_KEEP;

            }

        }


    else if (t.Kind() == types.TPTR) 
        if (t.Elem().Kind() == types.TANY) { 
            // ../../../../runtime/type.go:/UnsafePointerType
            ot = dcommontype(_addr_lsym, _addr_t);
            ot = dextratype(_addr_lsym, ot, _addr_t, 0);

            break;

        }
        s1 = writeType(_addr_t.Elem());

        ot = dcommontype(_addr_lsym, _addr_t);
        ot = objw.SymPtr(lsym, ot, s1, 0);
        ot = dextratype(_addr_lsym, ot, _addr_t, 0); 

        // ../../../../runtime/type.go:/structType
        // for security, only the exported fields.
    else if (t.Kind() == types.TSTRUCT) 
        var fields = t.Fields().Slice();
        {
            var t1__prev1 = t1;

            foreach (var (_, __t1) in fields) {
                t1 = __t1;
                writeType(_addr_t1.Type);
            } 

            // All non-exported struct field names within a struct
            // type must originate from a single package. By
            // identifying and recording that package within the
            // struct type descriptor, we can omit that
            // information from the field descriptors.

            t1 = t1__prev1;
        }

        ptr<types.Pkg> spkg;
        {
            var f__prev1 = f;

            foreach (var (_, __f) in fields) {
                f = __f;
                if (!types.IsExported(f.Sym.Name)) {
                    spkg = f.Sym.Pkg;
                    break;
                }
            }

            f = f__prev1;
        }

        ot = dcommontype(_addr_lsym, _addr_t);
        ot = dgopkgpath(_addr_lsym, ot, spkg);
        ot = objw.SymPtr(lsym, ot, lsym, ot + 3 * types.PtrSize + uncommonSize(_addr_t));
        ot = objw.Uintptr(lsym, ot, uint64(len(fields)));
        ot = objw.Uintptr(lsym, ot, uint64(len(fields)));

        dataAdd = len(fields) * structfieldSize();
        ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd);

        {
            var f__prev1 = f;

            foreach (var (_, __f) in fields) {
                f = __f; 
                // ../../../../runtime/type.go:/structField
                ot = dnameField(_addr_lsym, ot, spkg, _addr_f);
                ot = objw.SymPtr(lsym, ot, writeType(_addr_f.Type), 0);
                var offsetAnon = uint64(f.Offset) << 1;
                if (offsetAnon >> 1 != uint64(f.Offset)) {
                    @base.Fatalf("%v: bad field offset for %s", t, f.Sym.Name);
                }

                if (f.Embedded != 0) {
                    offsetAnon |= 1;
                }

                ot = objw.Uintptr(lsym, ot, offsetAnon);

            }

            f = f__prev1;
        }
    else 
        ot = dcommontype(_addr_lsym, _addr_t);
        ot = dextratype(_addr_lsym, ot, _addr_t, 0);
        ot = dextratypeData(_addr_lsym, ot, _addr_t);
    objw.Global(lsym, int32(ot), int16(dupok | obj.RODATA)); 

    // The linker will leave a table of all the typelinks for
    // types in the binary, so the runtime can find them.
    //
    // When buildmode=shared, all types are in typelinks so the
    // runtime can deduplicate type pointers.
    var keep = @base.Ctxt.Flag_dynlink;
    if (!keep && t.Sym() == null) { 
        // For an unnamed type, we only need the link if the type can
        // be created at run time by reflect.PtrTo and similar
        // functions. If the type exists in the program, those
        // functions must return the existing type structure rather
        // than creating a new one.

        if (t.Kind() == types.TPTR || t.Kind() == types.TARRAY || t.Kind() == types.TCHAN || t.Kind() == types.TFUNC || t.Kind() == types.TMAP || t.Kind() == types.TSLICE || t.Kind() == types.TSTRUCT) 
            keep = true;
        
    }
    if (types.TypeHasNoAlg(t)) {
        keep = false;
    }
    lsym.Set(obj.AttrMakeTypelink, keep);

    return _addr_lsym!;

}

// InterfaceMethodOffset returns the offset of the i-th method in the interface
// type descriptor, ityp.
public static long InterfaceMethodOffset(ptr<types.Type> _addr_ityp, long i) {
    ref types.Type ityp = ref _addr_ityp.val;
 
    // interface type descriptor layout is struct {
    //   _type        // commonSize
    //   pkgpath      // 1 word
    //   []imethod    // 3 words (pointing to [...]imethod below)
    //   uncommontype // uncommonSize
    //   [...]imethod
    // }
    // The size of imethod is 8.
    return int64(commonSize() + 4 * types.PtrSize + uncommonSize(_addr_ityp)) + i * 8;

}

// for each itabEntry, gather the methods on
// the concrete type that implement the interface
public static void CompileITabs() {
    foreach (var (i) in itabs) {
        var tab = _addr_itabs[i];
        var methods = genfun(_addr_tab.t, _addr_tab.itype);
        if (len(methods) == 0) {
            continue;
        }
        tab.entries = methods;

    }
}

// for the given concrete type and interface
// type, return the (sorted) set of methods
// on the concrete type that implement the interface
private static slice<ptr<obj.LSym>> genfun(ptr<types.Type> _addr_t, ptr<types.Type> _addr_it) {
    ref types.Type t = ref _addr_t.val;
    ref types.Type it = ref _addr_it.val;

    if (t == null || it == null) {
        return null;
    }
    var sigs = imethods(_addr_it);
    var methods = methods(_addr_t);
    var @out = make_slice<ptr<obj.LSym>>(0, len(sigs)); 
    // TODO(mdempsky): Short circuit before calling methods(t)?
    // See discussion on CL 105039.
    if (len(sigs) == 0) {
        return null;
    }
    foreach (var (_, m) in methods) {
        if (m.name == sigs[0].name) {
            out = append(out, m.isym);
            sigs = sigs[(int)1..];
            if (len(sigs) == 0) {
                break;
            }
        }
    }    if (len(sigs) != 0) {
        @base.Fatalf("incomplete itab");
    }
    return out;

}

// ITabSym uses the information gathered in
// CompileITabs to de-virtualize interface methods.
// Since this is called by the SSA backend, it shouldn't
// generate additional Nodes, Syms, etc.
public static ptr<obj.LSym> ITabSym(ptr<obj.LSym> _addr_it, long offset) {
    ref obj.LSym it = ref _addr_it.val;

    slice<ptr<obj.LSym>> syms = default;
    if (it == null) {
        return _addr_null!;
    }
    foreach (var (i) in itabs) {
        var e = _addr_itabs[i];
        if (e.lsym == it) {
            syms = e.entries;
            break;
        }
    }    if (syms == null) {
        return _addr_null!;
    }
    var methodnum = int((offset - 2 * int64(types.PtrSize) - 8) / int64(types.PtrSize));
    if (methodnum >= len(syms)) {
        return _addr_null!;
    }
    return _addr_syms[methodnum]!;

}

// NeedRuntimeType ensures that a runtime type descriptor is emitted for t.
public static void NeedRuntimeType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.HasTParam()) { 
        // Generic types don't have a runtime type descriptor (but will
        // have a dictionary)
        return ;

    }
    {
        var (_, ok) = signatset[t];

        if (!ok) {
            signatset[t] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            signatslice = append(signatslice, t);
        }
    }

}

public static void WriteRuntimeTypes() { 
    // Process signatset. Use a loop, as writeType adds
    // entries to signatset while it is being processed.
    var signats = make_slice<typeAndStr>(len(signatslice));
    while (len(signatslice) > 0) {
        signats = signats[..(int)0]; 
        // Transfer entries to a slice and sort, for reproducible builds.
        {
            var t__prev2 = t;

            foreach (var (_, __t) in signatslice) {
                t = __t;
                signats = append(signats, new typeAndStr(t:t,short:types.TypeSymName(t),regular:t.String()));
                delete(signatset, t);
            }

            t = t__prev2;
        }

        signatslice = signatslice[..(int)0];
        sort.Sort(typesByString(signats));
        {
            var ts__prev2 = ts;

            foreach (var (_, __ts) in signats) {
                ts = __ts;
                var t = ts.t;
                writeType(_addr_t);
                if (t.Sym() != null) {
                    writeType(_addr_types.NewPtr(t));
                }
            }

            ts = ts__prev2;
        }
    } 

    // Emit GC data symbols.
    var gcsyms = make_slice<typeAndStr>(0, len(gcsymset));
    {
        var t__prev1 = t;

        foreach (var (__t) in gcsymset) {
            t = __t;
            gcsyms = append(gcsyms, new typeAndStr(t:t,short:types.TypeSymName(t),regular:t.String()));
        }
        t = t__prev1;
    }

    sort.Sort(typesByString(gcsyms));
    {
        var ts__prev1 = ts;

        foreach (var (_, __ts) in gcsyms) {
            ts = __ts;
            dgcsym(_addr_ts.t, true);
        }
        ts = ts__prev1;
    }
}

public static void WriteTabs() { 
    // process itabs
    foreach (var (_, i) in itabs) { 
        // dump empty itab symbol into i.sym
        // type itab struct {
        //   inter  *interfacetype
        //   _type  *_type
        //   hash   uint32
        //   _      [4]byte
        //   fun    [1]uintptr // variable sized
        // }
        var o = objw.SymPtr(i.lsym, 0, writeType(_addr_i.itype), 0);
        o = objw.SymPtr(i.lsym, o, writeType(_addr_i.t), 0);
        o = objw.Uint32(i.lsym, o, types.TypeHash(i.t)); // copy of type hash
        o += 4; // skip unused field
        foreach (var (_, fn) in genfun(_addr_i.t, _addr_i.itype)) {
            o = objw.SymPtrWeak(i.lsym, o, fn, 0); // method pointer for each method
        }        objw.Global(i.lsym, int32(o), int16(obj.DUPOK | obj.RODATA));
        i.lsym.Set(obj.AttrContentAddressable, true);

    }    if (types.LocalPkg.Name == "main" && len(ptabs) > 0) {
        nint ot = 0;
        var s = @base.Ctxt.Lookup("go.plugin.tabs");
        {
            var p__prev1 = p;

            foreach (var (_, __p) in ptabs) {
                p = __p; 
                // Dump ptab symbol into go.pluginsym package.
                //
                // type ptab struct {
                //    name nameOff
                //    typ  typeOff // pointer to symbol
                // }
                var nsym = dname(p.Sym().Name, "", _addr_null, true);
                var t = p.Type();
                if (p.Class != ir.PFUNC) {
                    t = types.NewPtr(t);
                }

                var tsym = writeType(_addr_t);
                ot = objw.SymPtrOff(s, ot, nsym);
                ot = objw.SymPtrOff(s, ot, tsym); 
                // Plugin exports symbols as interfaces. Mark their types
                // as UsedInIface.
                tsym.Set(obj.AttrUsedInIface, true);

            }

            p = p__prev1;
        }

        objw.Global(s, int32(ot), int16(obj.RODATA));

        ot = 0;
        s = @base.Ctxt.Lookup("go.plugin.exports");
        {
            var p__prev1 = p;

            foreach (var (_, __p) in ptabs) {
                p = __p;
                ot = objw.SymPtr(s, ot, p.Linksym(), 0);
            }

            p = p__prev1;
        }

        objw.Global(s, int32(ot), int16(obj.RODATA));

    }
}

public static void WriteImportStrings() { 
    // generate import strings for imported packages
    foreach (var (_, p) in types.ImportedPkgList()) {
        dimportpath(_addr_p);
    }
}

public static void WriteBasicTypes() { 
    // do basic types if compiling package runtime.
    // they have to be in at least one package,
    // and runtime is always loaded implicitly,
    // so this is as good as any.
    // another possible choice would be package main,
    // but using runtime means fewer copies in object files.
    if (@base.Ctxt.Pkgpath == "runtime") {
        for (var i = types.Kind(1); i <= types.TBOOL; i++) {
            writeType(_addr_types.NewPtr(types.Types[i]));
        }
        writeType(_addr_types.NewPtr(types.Types[types.TSTRING]));
        writeType(_addr_types.NewPtr(types.Types[types.TUNSAFEPTR])); 

        // emit type structs for error and func(error) string.
        // The latter is the type of an auto-generated wrapper.
        writeType(_addr_types.NewPtr(types.ErrorType));

        writeType(_addr_types.NewSignature(types.NoPkg, null, null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.ErrorType) }), new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(base.Pos,nil,types.Types[types.TSTRING]) }))); 

        // add paths for runtime and main, which 6l imports implicitly.
        dimportpath(_addr_ir.Pkgs.Runtime);

        if (@base.Flag.Race) {
            dimportpath(_addr_types.NewPkg("runtime/race", ""));
        }
        if (@base.Flag.MSan) {
            dimportpath(_addr_types.NewPkg("runtime/msan", ""));
        }
        dimportpath(_addr_types.NewPkg("main", ""));

    }
}

private partial struct typeAndStr {
    public ptr<types.Type> t;
    public @string @short;
    public @string regular;
}

private partial struct typesByString { // : slice<typeAndStr>
}

private static nint Len(this typesByString a) {
    return len(a);
}
private static bool Less(this typesByString a, nint i, nint j) {
    if (a[i].@short != a[j].@short) {
        return a[i].@short < a[j].@short;
    }
    if (a[i].regular != a[j].regular) {
        return a[i].regular < a[j].regular;
    }
    if (a[i].t.Kind() == types.TINTER && a[i].t.AllMethods().Len() > 0) {
        return a[i].t.AllMethods().Index(0).Pos.Before(a[j].t.AllMethods().Index(0).Pos);
    }
    return false;

}
private static void Swap(this typesByString a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

// maxPtrmaskBytes is the maximum length of a GC ptrmask bitmap,
// which holds 1-bit entries describing where pointers are in a given type.
// Above this length, the GC information is recorded as a GC program,
// which can express repetition compactly. In either form, the
// information is used by the runtime to initialize the heap bitmap,
// and for large types (like 128 or more words), they are roughly the
// same speed. GC programs are never much larger and often more
// compact. (If large arrays are involved, they can be arbitrarily
// more compact.)
//
// The cutoff must be large enough that any allocation large enough to
// use a GC program is large enough that it does not share heap bitmap
// bytes with any other objects, allowing the GC program execution to
// assume an aligned start and not use atomic operations. In the current
// runtime, this means all malloc size classes larger than the cutoff must
// be multiples of four words. On 32-bit systems that's 16 bytes, and
// all size classes >= 16 bytes are 16-byte aligned, so no real constraint.
// On 64-bit systems, that's 32 bytes, and 32-byte alignment is guaranteed
// for size classes >= 256 bytes. On a 64-bit system, 256 bytes allocated
// is 32 pointers, the bits for which fit in 4 bytes. So maxPtrmaskBytes
// must be >= 4.
//
// We used to use 16 because the GC programs do have some constant overhead
// to get started, and processing 128 pointers seems to be enough to
// amortize that overhead well.
//
// To make sure that the runtime's chansend can call typeBitsBulkBarrier,
// we raised the limit to 2048, so that even 32-bit systems are guaranteed to
// use bitmaps for objects up to 64 kB in size.
//
// Also known to reflect/type.go.
//
private static readonly nint maxPtrmaskBytes = 2048;

// GCSym returns a data symbol containing GC information for type t, along
// with a boolean reporting whether the UseGCProg bit should be set in the
// type kind, and the ptrdata field to record in the reflect type information.
// GCSym may be called in concurrent backend, so it does not emit the symbol
// content.


// GCSym returns a data symbol containing GC information for type t, along
// with a boolean reporting whether the UseGCProg bit should be set in the
// type kind, and the ptrdata field to record in the reflect type information.
// GCSym may be called in concurrent backend, so it does not emit the symbol
// content.
public static (ptr<obj.LSym>, bool, long) GCSym(ptr<types.Type> _addr_t) {
    ptr<obj.LSym> lsym = default!;
    bool useGCProg = default;
    long ptrdata = default;
    ref types.Type t = ref _addr_t.val;
 
    // Record that we need to emit the GC symbol.
    gcsymmu.Lock();
    {
        var (_, ok) = gcsymset[t];

        if (!ok) {
            gcsymset[t] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        }
    }

    gcsymmu.Unlock();

    return _addr_dgcsym(_addr_t, false)!;

}

// dgcsym returns a data symbol containing GC information for type t, along
// with a boolean reporting whether the UseGCProg bit should be set in the
// type kind, and the ptrdata field to record in the reflect type information.
// When write is true, it writes the symbol data.
private static (ptr<obj.LSym>, bool, long) dgcsym(ptr<types.Type> _addr_t, bool write) {
    ptr<obj.LSym> lsym = default!;
    bool useGCProg = default;
    long ptrdata = default;
    ref types.Type t = ref _addr_t.val;

    ptrdata = types.PtrDataSize(t);
    if (ptrdata / int64(types.PtrSize) <= maxPtrmaskBytes * 8) {
        lsym = dgcptrmask(_addr_t, write);
        return ;
    }
    useGCProg = true;
    lsym, ptrdata = dgcprog(_addr_t, write);
    return ;

}

// dgcptrmask emits and returns the symbol containing a pointer mask for type t.
private static ptr<obj.LSym> dgcptrmask(ptr<types.Type> _addr_t, bool write) {
    ref types.Type t = ref _addr_t.val;

    var ptrmask = make_slice<byte>((types.PtrDataSize(t) / int64(types.PtrSize) + 7) / 8);
    fillptrmask(_addr_t, ptrmask);
    var p = fmt.Sprintf("runtime.gcbits.%x", ptrmask);

    var lsym = @base.Ctxt.Lookup(p);
    if (write && !lsym.OnList()) {
        foreach (var (i, x) in ptrmask) {
            objw.Uint8(lsym, i, x);
        }        objw.Global(lsym, int32(len(ptrmask)), obj.DUPOK | obj.RODATA | obj.LOCAL);
        lsym.Set(obj.AttrContentAddressable, true);
    }
    return _addr_lsym!;

}

// fillptrmask fills in ptrmask with 1s corresponding to the
// word offsets in t that hold pointers.
// ptrmask is assumed to fit at least types.PtrDataSize(t)/PtrSize bits.
private static void fillptrmask(ptr<types.Type> _addr_t, slice<byte> ptrmask) {
    ref types.Type t = ref _addr_t.val;

    {
        var i__prev1 = i;

        foreach (var (__i) in ptrmask) {
            i = __i;
            ptrmask[i] = 0;
        }
        i = i__prev1;
    }

    if (!t.HasPointers()) {
        return ;
    }
    var vec = bitvec.New(8 * int32(len(ptrmask)));
    typebits.Set(t, 0, vec);

    var nptr = types.PtrDataSize(t) / int64(types.PtrSize);
    {
        var i__prev1 = i;

        for (var i = int64(0); i < nptr; i++) {
            if (vec.Get(int32(i))) {
                ptrmask[i / 8] |= 1 << (int)((uint(i) % 8));
            }
        }

        i = i__prev1;
    }

}

// dgcprog emits and returns the symbol containing a GC program for type t
// along with the size of the data described by the program (in the range
// [types.PtrDataSize(t), t.Width]).
// In practice, the size is types.PtrDataSize(t) except for non-trivial arrays.
// For non-trivial arrays, the program describes the full t.Width size.
private static (ptr<obj.LSym>, long) dgcprog(ptr<types.Type> _addr_t, bool write) {
    ptr<obj.LSym> _p0 = default!;
    long _p0 = default;
    ref types.Type t = ref _addr_t.val;

    types.CalcSize(t);
    if (t.Width == types.BADWIDTH) {
        @base.Fatalf("dgcprog: %v badwidth", t);
    }
    var lsym = TypeLinksymPrefix(".gcprog", _addr_t);
    gcProg p = default;
    p.init(lsym, write);
    p.emit(t, 0);
    var offset = p.w.BitIndex() * int64(types.PtrSize);
    p.end();
    {
        var ptrdata = types.PtrDataSize(t);

        if (offset < ptrdata || offset > t.Width) {
            @base.Fatalf("dgcprog: %v: offset=%d but ptrdata=%d size=%d", t, offset, ptrdata, t.Width);
        }
    }

    return (_addr_lsym!, offset);

}

private partial struct gcProg {
    public ptr<obj.LSym> lsym;
    public nint symoff;
    public gcprog.Writer w;
    public bool write;
}

private static void init(this ptr<gcProg> _addr_p, ptr<obj.LSym> _addr_lsym, bool write) {
    ref gcProg p = ref _addr_p.val;
    ref obj.LSym lsym = ref _addr_lsym.val;

    p.lsym = lsym;
    p.write = write && !lsym.OnList();
    p.symoff = 4; // first 4 bytes hold program length
    if (!write) {
        p.w.Init(_p0 => {
        });
        return ;

    }
    p.w.Init(p.writeByte);
    if (@base.Debug.GCProg > 0) {
        fmt.Fprintf(os.Stderr, "compile: start GCProg for %v\n", lsym);
        p.w.Debug(os.Stderr);
    }
}

private static void writeByte(this ptr<gcProg> _addr_p, byte x) {
    ref gcProg p = ref _addr_p.val;

    p.symoff = objw.Uint8(p.lsym, p.symoff, x);
}

private static void end(this ptr<gcProg> _addr_p) {
    ref gcProg p = ref _addr_p.val;

    p.w.End();
    if (!p.write) {
        return ;
    }
    objw.Uint32(p.lsym, 0, uint32(p.symoff - 4));
    objw.Global(p.lsym, int32(p.symoff), obj.DUPOK | obj.RODATA | obj.LOCAL);
    p.lsym.Set(obj.AttrContentAddressable, true);
    if (@base.Debug.GCProg > 0) {
        fmt.Fprintf(os.Stderr, "compile: end GCProg for %v\n", p.lsym);
    }
}

private static void emit(this ptr<gcProg> _addr_p, ptr<types.Type> _addr_t, long offset) {
    ref gcProg p = ref _addr_p.val;
    ref types.Type t = ref _addr_t.val;

    types.CalcSize(t);
    if (!t.HasPointers()) {
        return ;
    }
    if (t.Width == int64(types.PtrSize)) {
        p.w.Ptr(offset / int64(types.PtrSize));
        return ;
    }

    if (t.Kind() == types.TSTRING) 
        p.w.Ptr(offset / int64(types.PtrSize));
    else if (t.Kind() == types.TINTER) 
        // Note: the first word isn't a pointer. See comment in typebits.Set
        p.w.Ptr(offset / int64(types.PtrSize) + 1);
    else if (t.Kind() == types.TSLICE) 
        p.w.Ptr(offset / int64(types.PtrSize));
    else if (t.Kind() == types.TARRAY) 
        if (t.NumElem() == 0) { 
            // should have been handled by haspointers check above
            @base.Fatalf("gcProg.emit: empty array");

        }
        var count = t.NumElem();
        var elem = t.Elem();
        while (elem.IsArray()) {
            count *= elem.NumElem();
            elem = elem.Elem();
        }

        if (!p.w.ShouldRepeat(elem.Width / int64(types.PtrSize), count)) { 
            // Cheaper to just emit the bits.
            for (var i = int64(0); i < count; i++) {
                p.emit(elem, offset + i * elem.Width);
            }

            return ;

        }
        p.emit(elem, offset);
        p.w.ZeroUntil((offset + elem.Width) / int64(types.PtrSize));
        p.w.Repeat(elem.Width / int64(types.PtrSize), count - 1);
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            p.emit(t1.Type, offset + t1.Offset);
        }    else 
        @base.Fatalf("gcProg.emit: unexpected type %v", t);
    
}

// ZeroAddr returns the address of a symbol with at least
// size bytes of zeros.
public static ir.Node ZeroAddr(long size) {
    if (size >= 1 << 31) {
        @base.Fatalf("map elem too big %d", size);
    }
    if (ZeroSize < size) {
        ZeroSize = size;
    }
    var lsym = @base.PkgLinksym("go.map", "zero", obj.ABI0);
    var x = ir.NewLinksymExpr(@base.Pos, lsym, types.Types[types.TUINT8]);
    return typecheck.Expr(typecheck.NodAddr(x));

}

public static void CollectPTabs() {
    if (!@base.Ctxt.Flag_dynlink || types.LocalPkg.Name != "main") {
        return ;
    }
    foreach (var (_, exportn) in typecheck.Target.Exports) {
        var s = exportn.Sym();
        var nn = ir.AsNode(s.Def);
        if (nn == null) {
            continue;
        }
        if (nn.Op() != ir.ONAME) {
            continue;
        }
        ptr<ir.Name> n = nn._<ptr<ir.Name>>();
        if (!types.IsExported(s.Name)) {
            continue;
        }
        if (s.Pkg.Name != "main") {
            continue;
        }
        ptabs = append(ptabs, n);

    }
}

// Generate a wrapper function to convert from
// a receiver of type T to a receiver of type U.
// That is,
//
//    func (t T) M() {
//        ...
//    }
//
// already exists; this function generates
//
//    func (u U) M() {
//        u.M()
//    }
//
// where the types T and U are such that u.M() is valid
// and calls the T.M method.
// The resulting function is for use in method tables.
//
//    rcvr - U
//    method - M func (t T)(), a TFIELD type struct
private static ptr<obj.LSym> methodWrapper(ptr<types.Type> _addr_rcvr, ptr<types.Field> _addr_method) {
    ref types.Type rcvr = ref _addr_rcvr.val;
    ref types.Field method = ref _addr_method.val;

    var newnam = ir.MethodSym(rcvr, method.Sym);
    var lsym = newnam.Linksym();
    if (newnam.Siggen()) {
        return _addr_lsym!;
    }
    newnam.SetSiggen(true);

    if (types.Identical(rcvr, method.Type.Recv().Type)) {
        return _addr_lsym!;
    }
    if (rcvr.IsPtr() && rcvr.Elem() == method.Type.Recv().Type && rcvr.Elem().Sym() != null && rcvr.Elem().Sym().Pkg != types.LocalPkg) {
        return _addr_lsym!;
    }
    if (rcvr.IsInterface() && rcvr.Sym() != null && rcvr.Sym().Pkg != types.LocalPkg && rcvr != types.ErrorType) {
        return _addr_lsym!;
    }
    @base.Pos = @base.AutogeneratedPos;
    typecheck.DeclContext = ir.PEXTERN;

    var tfn = ir.NewFuncType(@base.Pos, ir.NewField(@base.Pos, typecheck.Lookup(".this"), null, rcvr), typecheck.NewFuncParams(method.Type.Params(), true), typecheck.NewFuncParams(method.Type.Results(), false)); 

    // TODO(austin): SelectorExpr may have created one or more
    // ir.Names for these already with a nil Func field. We should
    // consolidate these and always attach a Func to the Name.
    var fn = typecheck.DeclFunc(newnam, tfn);
    fn.SetDupok(true);

    var nthis = ir.AsNode(tfn.Type().Recv().Nname);

    var methodrcvr = method.Type.Recv().Type; 

    // generate nil pointer check for better error
    if (rcvr.IsPtr() && rcvr.Elem() == methodrcvr) { 
        // generating wrapper from *T to T.
        var n = ir.NewIfStmt(@base.Pos, null, null, null);
        n.Cond = ir.NewBinaryExpr(@base.Pos, ir.OEQ, nthis, typecheck.NodNil());
        var call = ir.NewCallExpr(@base.Pos, ir.OCALL, typecheck.LookupRuntime("panicwrap"), null);
        n.Body = new slice<ir.Node>(new ir.Node[] { call });
        fn.Body.Append(n);

    }
    var dot = typecheck.AddImplicitDots(ir.NewSelectorExpr(@base.Pos, ir.OXDOT, nthis, method.Sym)); 

    // generate call
    // It's not possible to use a tail call when dynamic linking on ppc64le. The
    // bad scenario is when a local call is made to the wrapper: the wrapper will
    // call the implementation, which might be in a different module and so set
    // the TOC to the appropriate value for that module. But if it returns
    // directly to the wrapper's caller, nothing will reset it to the correct
    // value for that function.
    //
    // Disable tailcall for RegabiArgs for now. The IR does not connect the
    // arguments with the OTAILCALL node, and the arguments are not marshaled
    // correctly.
    if (!@base.Flag.Cfg.Instrumenting && rcvr.IsPtr() && methodrcvr.IsPtr() && method.Embedded != 0 && !types.IsInterfaceMethod(method.Type) && !(@base.Ctxt.Arch.Name == "ppc64le" && @base.Ctxt.Flag_dynlink) && !buildcfg.Experiment.RegabiArgs) { 
        // generate tail call: adjust pointer receiver and jump to embedded method.
        var left = dot.X; // skip final .M
        if (!left.Type().IsPtr()) {
            left = typecheck.NodAddr(left);
        }
        var @as = ir.NewAssignStmt(@base.Pos, nthis, typecheck.ConvNop(left, rcvr));
        fn.Body.Append(as);
        fn.Body.Append(ir.NewTailCallStmt(@base.Pos, method.Nname._<ptr<ir.Name>>()));

    }
    else
 {
        fn.SetWrapper(true); // ignore frame for panic+recover matching
        call = ir.NewCallExpr(@base.Pos, ir.OCALL, dot, null);
        call.Args = ir.ParamNames(tfn.Type());
        call.IsDDD = tfn.Type().IsVariadic();
        if (method.Type.NumResults() > 0) {
            var ret = ir.NewReturnStmt(@base.Pos, null);
            ret.Results = new slice<ir.Node>(new ir.Node[] { call });
            fn.Body.Append(ret);
        }
        else
 {
            fn.Body.Append(call);
        }
    }
    typecheck.FinishFuncBody();
    if (@base.Debug.DclStack != 0) {
        types.CheckDclstack();
    }
    typecheck.Func(fn);
    ir.CurFunc = fn;
    typecheck.Stmts(fn.Body); 

    // Inline calls within (*T).M wrappers. This is safe because we only
    // generate those wrappers within the same compilation unit as (T).M.
    // TODO(mdempsky): Investigate why we can't enable this more generally.
    if (rcvr.IsPtr() && rcvr.Elem() == method.Type.Recv().Type && rcvr.Elem().Sym() != null) {
        inline.InlineCalls(fn);
    }
    escape.Batch(new slice<ptr<ir.Func>>(new ptr<ir.Func>[] { fn }), false);

    ir.CurFunc = null;
    typecheck.Target.Decls = append(typecheck.Target.Decls, fn);

    return _addr_lsym!;

}

public static long ZeroSize = default;

// MarkTypeUsedInInterface marks that type t is converted to an interface.
// This information is used in the linker in dead method elimination.
public static void MarkTypeUsedInInterface(ptr<types.Type> _addr_t, ptr<obj.LSym> _addr_from) {
    ref types.Type t = ref _addr_t.val;
    ref obj.LSym from = ref _addr_from.val;

    var tsym = TypeLinksym(_addr_t); 
    // Emit a marker relocation. The linker will know the type is converted
    // to an interface if "from" is reachable.
    var r = obj.Addrel(from);
    r.Sym = tsym;
    r.Type = objabi.R_USEIFACE;

}

// MarkUsedIfaceMethod marks that an interface method is used in the current
// function. n is OCALLINTER node.
public static void MarkUsedIfaceMethod(ptr<ir.CallExpr> _addr_n) {
    ref ir.CallExpr n = ref _addr_n.val;
 
    // skip unnamed functions (func _())
    if (ir.CurFunc.LSym == null) {
        return ;
    }
    ptr<ir.SelectorExpr> dot = n.X._<ptr<ir.SelectorExpr>>();
    var ityp = dot.X.Type();
    var tsym = TypeLinksym(_addr_ityp);
    var r = obj.Addrel(ir.CurFunc.LSym);
    r.Sym = tsym; 
    // dot.Xoffset is the method index * PtrSize (the offset of code pointer
    // in itab).
    var midx = dot.Offset() / int64(types.PtrSize);
    r.Add = InterfaceMethodOffset(_addr_ityp, midx);
    r.Type = objabi.R_USEIFACEMETHOD;

}

} // end reflectdata_package
