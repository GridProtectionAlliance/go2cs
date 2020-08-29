// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:28:20 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\reflect.go
using types = go.cmd.compile.@internal.types_package;
using gcprog = go.cmd.@internal.gcprog_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private partial struct itabEntry
        {
            public ptr<types.Type> t;
            public ptr<types.Type> itype;
            public ptr<obj.LSym> lsym; // symbol of the itab itself

// symbols of each method in
// the itab, sorted by byte offset;
// filled in by peekitabs
            public slice<ref obj.LSym> entries;
        }

        private partial struct ptabEntry
        {
            public ptr<types.Sym> s;
            public ptr<types.Type> t;
        }

        // runtime interface and reflection data structures
        private static sync.Mutex signatsetmu = default;        private static var signatset = make();        private static slice<itabEntry> itabs = default;        private static slice<ptabEntry> ptabs = default;

        public partial struct Sig
        {
            public @string name;
            public ptr<types.Pkg> pkg;
            public ptr<types.Sym> isym;
            public ptr<types.Sym> tsym;
            public ptr<types.Type> type_;
            public ptr<types.Type> mtype;
            public int offset;
        }

        // siglt sorts method signatures by name, then package path.
        private static bool siglt(ref Sig a, ref Sig b)
        {
            if (a.name != b.name)
            {
                return a.name < b.name;
            }
            if (a.pkg == b.pkg)
            {
                return false;
            }
            if (a.pkg == null)
            {
                return true;
            }
            if (b.pkg == null)
            {
                return false;
            }
            return a.pkg.Path < b.pkg.Path;
        }

        // Builds a type representing a Bucket structure for
        // the given map type. This type is not visible to users -
        // we include only enough information to generate a correct GC
        // program for it.
        // Make sure this stays in sync with ../../../../runtime/hashmap.go!
        public static readonly long BUCKETSIZE = 8L;
        public static readonly long MAXKEYSIZE = 128L;
        public static readonly long MAXVALSIZE = 128L;

        private static long structfieldSize()
        {
            return 3L * Widthptr;
        } // Sizeof(runtime.structfield{})
        private static long imethodSize()
        {
            return 4L + 4L;
        } // Sizeof(runtime.imethod{})

        private static long uncommonSize(ref types.Type t)
        { // Sizeof(runtime.uncommontype{})
            if (t.Sym == null && len(methods(t)) == 0L)
            {
                return 0L;
            }
            return 4L + 2L + 2L + 4L + 4L;
        }

        private static ref types.Field makefield(@string name, ref types.Type t)
        {
            var f = types.NewField();
            f.Type = t;
            f.Sym = (types.Pkg.Value)(null).Lookup(name);
            return f;
        }

        // bmap makes the map bucket type given the type of the map.
        private static ref types.Type bmap(ref types.Type t)
        {
            if (t.MapType().Bucket != null)
            {
                return t.MapType().Bucket;
            }
            var bucket = types.New(TSTRUCT);
            var keytype = t.Key();
            var valtype = t.Val();
            dowidth(keytype);
            dowidth(valtype);
            if (keytype.Width > MAXKEYSIZE)
            {
                keytype = types.NewPtr(keytype);
            }
            if (valtype.Width > MAXVALSIZE)
            {
                valtype = types.NewPtr(valtype);
            }
            var field = make_slice<ref types.Field>(0L, 5L); 

            // The first field is: uint8 topbits[BUCKETSIZE].
            var arr = types.NewArray(types.Types[TUINT8], BUCKETSIZE);
            field = append(field, makefield("topbits", arr));

            arr = types.NewArray(keytype, BUCKETSIZE);
            arr.SetNoalg(true);
            var keys = makefield("keys", arr);
            field = append(field, keys);

            arr = types.NewArray(valtype, BUCKETSIZE);
            arr.SetNoalg(true);
            var values = makefield("values", arr);
            field = append(field, values); 

            // Make sure the overflow pointer is the last memory in the struct,
            // because the runtime assumes it can use size-ptrSize as the
            // offset of the overflow pointer. We double-check that property
            // below once the offsets and size are computed.
            //
            // BUCKETSIZE is 8, so the struct is aligned to 64 bits to this point.
            // On 32-bit systems, the max alignment is 32-bit, and the
            // overflow pointer will add another 32-bit field, and the struct
            // will end with no padding.
            // On 64-bit systems, the max alignment is 64-bit, and the
            // overflow pointer will add another 64-bit field, and the struct
            // will end with no padding.
            // On nacl/amd64p32, however, the max alignment is 64-bit,
            // but the overflow pointer will add only a 32-bit field,
            // so if the struct needs 64-bit padding (because a key or value does)
            // then it would end with an extra 32-bit padding field.
            // Preempt that by emitting the padding here.
            if (int(valtype.Align) > Widthptr || int(keytype.Align) > Widthptr)
            {
                field = append(field, makefield("pad", types.Types[TUINTPTR]));
            } 

            // If keys and values have no pointers, the map implementation
            // can keep a list of overflow pointers on the side so that
            // buckets can be marked as having no pointers.
            // Arrange for the bucket to have no pointers by changing
            // the type of the overflow field to uintptr in this case.
            // See comment on hmap.overflow in ../../../../runtime/hashmap.go.
            var otyp = types.NewPtr(bucket);
            if (!types.Haspointers(valtype) && !types.Haspointers(keytype))
            {
                otyp = types.Types[TUINTPTR];
            }
            var overflow = makefield("overflow", otyp);
            field = append(field, overflow); 

            // link up fields
            bucket.SetNoalg(true);
            bucket.SetFields(field[..]);
            dowidth(bucket); 

            // Check invariants that map code depends on.
            if (!IsComparable(t.Key()))
            {
                Fatalf("unsupported map key type for %v", t);
            }
            if (BUCKETSIZE < 8L)
            {
                Fatalf("bucket size too small for proper alignment");
            }
            if (keytype.Align > BUCKETSIZE)
            {
                Fatalf("key align too big for %v", t);
            }
            if (valtype.Align > BUCKETSIZE)
            {
                Fatalf("value align too big for %v", t);
            }
            if (keytype.Width > MAXKEYSIZE)
            {
                Fatalf("key size to large for %v", t);
            }
            if (valtype.Width > MAXVALSIZE)
            {
                Fatalf("value size to large for %v", t);
            }
            if (t.Key().Width > MAXKEYSIZE && !keytype.IsPtr())
            {
                Fatalf("key indirect incorrect for %v", t);
            }
            if (t.Val().Width > MAXVALSIZE && !valtype.IsPtr())
            {
                Fatalf("value indirect incorrect for %v", t);
            }
            if (keytype.Width % int64(keytype.Align) != 0L)
            {
                Fatalf("key size not a multiple of key align for %v", t);
            }
            if (valtype.Width % int64(valtype.Align) != 0L)
            {
                Fatalf("value size not a multiple of value align for %v", t);
            }
            if (bucket.Align % keytype.Align != 0L)
            {
                Fatalf("bucket align not multiple of key align %v", t);
            }
            if (bucket.Align % valtype.Align != 0L)
            {
                Fatalf("bucket align not multiple of value align %v", t);
            }
            if (keys.Offset % int64(keytype.Align) != 0L)
            {
                Fatalf("bad alignment of keys in bmap for %v", t);
            }
            if (values.Offset % int64(valtype.Align) != 0L)
            {
                Fatalf("bad alignment of values in bmap for %v", t);
            } 

            // Double-check that overflow field is final memory in struct,
            // with no padding at end. See comment above.
            if (overflow.Offset != bucket.Width - int64(Widthptr))
            {
                Fatalf("bad offset of overflow in bmap for %v", t);
            }
            t.MapType().Bucket = bucket;

            bucket.StructType().Map = t;
            return bucket;
        }

        // hmap builds a type representing a Hmap structure for the given map type.
        // Make sure this stays in sync with ../../../../runtime/hashmap.go.
        private static ref types.Type hmap(ref types.Type t)
        {
            if (t.MapType().Hmap != null)
            {
                return t.MapType().Hmap;
            }
            var bmap = bmap(t); 

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
            // must match ../../../../runtime/hashmap.go:hmap.
            ref types.Field fields = new slice<ref types.Field>(new ref types.Field[] { makefield("count",types.Types[TINT]), makefield("flags",types.Types[TUINT8]), makefield("B",types.Types[TUINT8]), makefield("noverflow",types.Types[TUINT16]), makefield("hash0",types.Types[TUINT32]), makefield("buckets",types.NewPtr(bmap)), makefield("oldbuckets",types.NewPtr(bmap)), makefield("nevacuate",types.Types[TUINTPTR]), makefield("extra",types.Types[TUNSAFEPTR]) });

            var hmap = types.New(TSTRUCT);
            hmap.SetNoalg(true);
            hmap.SetFields(fields);
            dowidth(hmap); 

            // The size of hmap should be 48 bytes on 64 bit
            // and 28 bytes on 32 bit platforms.
            {
                var size = int64(8L + 5L * Widthptr);

                if (hmap.Width != size)
                {
                    Fatalf("hmap size not correct: got %d, want %d", hmap.Width, size);
                }

            }

            t.MapType().Hmap = hmap;
            hmap.StructType().Map = t;
            return hmap;
        }

        // hiter builds a type representing an Hiter structure for the given map type.
        // Make sure this stays in sync with ../../../../runtime/hashmap.go.
        private static ref types.Type hiter(ref types.Type t)
        {
            if (t.MapType().Hiter != null)
            {
                return t.MapType().Hiter;
            }
            var hmap = hmap(t);
            var bmap = bmap(t); 

            // build a struct:
            // type hiter struct {
            //    key         *Key
            //    val         *Value
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
            // must match ../../../../runtime/hashmap.go:hiter.
            ref types.Field fields = new slice<ref types.Field>(new ref types.Field[] { makefield("key",types.NewPtr(t.Key())), makefield("val",types.NewPtr(t.Val())), makefield("t",types.Types[TUNSAFEPTR]), makefield("h",types.NewPtr(hmap)), makefield("buckets",types.NewPtr(bmap)), makefield("bptr",types.NewPtr(bmap)), makefield("overflow",types.Types[TUNSAFEPTR]), makefield("oldoverflow",types.Types[TUNSAFEPTR]), makefield("startBucket",types.Types[TUINTPTR]), makefield("offset",types.Types[TUINT8]), makefield("wrapped",types.Types[TBOOL]), makefield("B",types.Types[TUINT8]), makefield("i",types.Types[TUINT8]), makefield("bucket",types.Types[TUINTPTR]), makefield("checkBucket",types.Types[TUINTPTR]) }); 

            // build iterator struct holding the above fields
            var hiter = types.New(TSTRUCT);
            hiter.SetNoalg(true);
            hiter.SetFields(fields);
            dowidth(hiter);
            if (hiter.Width != int64(12L * Widthptr))
            {
                Fatalf("hash_iter size not correct %d %d", hiter.Width, 12L * Widthptr);
            }
            t.MapType().Hiter = hiter;
            hiter.StructType().Map = t;
            return hiter;
        }

        // f is method type, with receiver.
        // return function type, receiver as first argument (or not).
        private static ref types.Type methodfunc(ref types.Type f, ref types.Type receiver)
        {
            slice<ref Node> @in = default;
            if (receiver != null)
            {
                var d = nod(ODCLFIELD, null, null);
                d.Type = receiver;
                in = append(in, d);
            }
            d = default;
            {
                var t__prev1 = t;

                foreach (var (_, __t) in f.Params().Fields().Slice())
                {
                    t = __t;
                    d = nod(ODCLFIELD, null, null);
                    d.Type = t.Type;
                    d.SetIsddd(t.Isddd());
                    in = append(in, d);
                }

                t = t__prev1;
            }

            slice<ref Node> @out = default;
            {
                var t__prev1 = t;

                foreach (var (_, __t) in f.Results().Fields().Slice())
                {
                    t = __t;
                    d = nod(ODCLFIELD, null, null);
                    d.Type = t.Type;
                    out = append(out, d);
                }

                t = t__prev1;
            }

            var t = functype(null, in, out);
            if (f.Nname() != null)
            { 
                // Link to name of original method function.
                t.SetNname(f.Nname());
            }
            return t;
        }

        // methods returns the methods of the non-interface type t, sorted by name.
        // Generates stub functions as needed.
        private static slice<ref Sig> methods(ref types.Type t)
        { 
            // method type
            var mt = methtype(t);

            if (mt == null)
            {
                return null;
            }
            expandmeth(mt); 

            // type stored in interface word
            var it = t;

            if (!isdirectiface(it))
            {
                it = types.NewPtr(t);
            } 

            // make list of methods for t,
            // generating code if necessary.
            slice<ref Sig> ms = default;
            foreach (var (_, f) in mt.AllMethods().Slice())
            {
                if (f.Type.Etype != TFUNC || f.Type.Recv() == null)
                {
                    Fatalf("non-method on %v method %v %v\n", mt, f.Sym, f);
                }
                if (f.Type.Recv() == null)
                {
                    Fatalf("receiver with no type on %v method %v %v\n", mt, f.Sym, f);
                }
                if (f.Nointerface())
                {
                    continue;
                }
                var method = f.Sym;
                if (method == null)
                {
                    continue;
                } 

                // get receiver type for this particular method.
                // if pointer receiver but non-pointer t and
                // this is not an embedded pointer inside a struct,
                // method does not apply.
                var @this = f.Type.Recv().Type;

                if (@this.IsPtr() && @this.Elem() == t)
                {
                    continue;
                }
                if (@this.IsPtr() && !t.IsPtr() && f.Embedded != 2L && !isifacemethod(f.Type))
                {
                    continue;
                }
                Sig sig = default;
                ms = append(ms, ref sig);

                sig.name = method.Name;
                if (!exportname(method.Name))
                {
                    if (method.Pkg == null)
                    {
                        Fatalf("methods: missing package");
                    }
                    sig.pkg = method.Pkg;
                }
                sig.isym = methodsym(method, it, true);
                sig.tsym = methodsym(method, t, false);
                sig.type_ = methodfunc(f.Type, t);
                sig.mtype = methodfunc(f.Type, null);

                if (!sig.isym.Siggen())
                {
                    sig.isym.SetSiggen(true);
                    if (!eqtype(this, it) || @this.Width < int64(Widthptr))
                    {
                        compiling_wrappers = true;
                        genwrapper(it, f, sig.isym, true);
                        compiling_wrappers = false;
                    }
                }
                if (!sig.tsym.Siggen())
                {
                    sig.tsym.SetSiggen(true);
                    if (!eqtype(this, t))
                    {
                        compiling_wrappers = true;
                        genwrapper(t, f, sig.tsym, false);
                        compiling_wrappers = false;
                    }
                }
            }
            obj.SortSlice(ms, (i, j) => siglt(ms[i], ms[j]));
            return ms;
        }

        // imethods returns the methods of the interface type t, sorted by name.
        private static slice<ref Sig> imethods(ref types.Type t)
        {
            slice<ref Sig> methods = default;
            foreach (var (_, f) in t.Fields().Slice())
            {
                if (f.Type.Etype != TFUNC || f.Sym == null)
                {
                    continue;
                }
                var method = f.Sym;
                Sig sig = new Sig(name:method.Name,);
                if (!exportname(method.Name))
                {
                    if (method.Pkg == null)
                    {
                        Fatalf("imethods: missing package");
                    }
                    sig.pkg = method.Pkg;
                }
                sig.mtype = f.Type;
                sig.offset = 0L;
                sig.type_ = methodfunc(f.Type, null);

                {
                    var n = len(methods);

                    if (n > 0L)
                    {
                        var last = methods[n - 1L];
                        if (!(siglt(last, ref sig)))
                        {
                            Fatalf("sigcmp vs sortinter %s %s", last.name, sig.name);
                        }
                    }

                }
                methods = append(methods, ref sig); 

                // Compiler can only refer to wrappers for non-blank methods.
                if (method.IsBlank())
                {
                    continue;
                } 

                // NOTE(rsc): Perhaps an oversight that
                // IfaceType.Method is not in the reflect data.
                // Generate the method body, so that compiled
                // code can refer to it.
                var isym = methodsym(method, t, false);
                if (!isym.Siggen())
                {
                    isym.SetSiggen(true);
                    genwrapper(t, f, isym, false);
                }
            }
            return methods;
        }

        private static void dimportpath(ref types.Pkg p)
        {
            if (p.Pathsym != null)
            {
                return;
            } 

            // If we are compiling the runtime package, there are two runtime packages around
            // -- localpkg and Runtimepkg. We don't want to produce import path symbols for
            // both of them, so just produce one for localpkg.
            if (myimportpath == "runtime" && p == Runtimepkg)
            {
                return;
            }
            @string str = default;
            if (p == localpkg)
            { 
                // Note: myimportpath != "", or else dgopkgpath won't call dimportpath.
                str = myimportpath;
            }
            else
            {
                str = p.Path;
            }
            var s = Ctxt.Lookup("type..importpath." + p.Prefix + ".");
            var ot = dnameData(s, 0L, str, "", null, false);
            ggloblsym(s, int32(ot), obj.DUPOK | obj.RODATA);
            p.Pathsym = s;
        }

        private static long dgopkgpath(ref obj.LSym s, long ot, ref types.Pkg pkg)
        {
            if (pkg == null)
            {
                return duintptr(s, ot, 0L);
            }
            if (pkg == localpkg && myimportpath == "")
            { 
                // If we don't know the full import path of the package being compiled
                // (i.e. -p was not passed on the compiler command line), emit a reference to
                // type..importpath.""., which the linker will rewrite using the correct import path.
                // Every package that imports this one directly defines the symbol.
                // See also https://groups.google.com/forum/#!topic/golang-dev/myb9s53HxGQ.
                var ns = Ctxt.Lookup("type..importpath.\"\".");
                return dsymptr(s, ot, ns, 0L);
            }
            dimportpath(pkg);
            return dsymptr(s, ot, pkg.Pathsym, 0L);
        }

        // dgopkgpathOff writes an offset relocation in s at offset ot to the pkg path symbol.
        private static long dgopkgpathOff(ref obj.LSym s, long ot, ref types.Pkg pkg)
        {
            if (pkg == null)
            {
                return duint32(s, ot, 0L);
            }
            if (pkg == localpkg && myimportpath == "")
            { 
                // If we don't know the full import path of the package being compiled
                // (i.e. -p was not passed on the compiler command line), emit a reference to
                // type..importpath.""., which the linker will rewrite using the correct import path.
                // Every package that imports this one directly defines the symbol.
                // See also https://groups.google.com/forum/#!topic/golang-dev/myb9s53HxGQ.
                var ns = Ctxt.Lookup("type..importpath.\"\".");
                return dsymptrOff(s, ot, ns, 0L);
            }
            dimportpath(pkg);
            return dsymptrOff(s, ot, pkg.Pathsym, 0L);
        }

        // dnameField dumps a reflect.name for a struct field.
        private static long dnameField(ref obj.LSym lsym, long ot, ref types.Pkg spkg, ref types.Field ft)
        {
            if (!exportname(ft.Sym.Name) && ft.Sym.Pkg != spkg)
            {
                Fatalf("package mismatch for %v", ft.Sym);
            }
            var nsym = dname(ft.Sym.Name, ft.Note, null, exportname(ft.Sym.Name));
            return dsymptr(lsym, ot, nsym, 0L);
        }

        // dnameData writes the contents of a reflect.name into s at offset ot.
        private static long dnameData(ref obj.LSym s, long ot, @string name, @string tag, ref types.Pkg pkg, bool exported)
        {
            if (len(name) > 1L << (int)(16L) - 1L)
            {
                Fatalf("name too long: %s", name);
            }
            if (len(tag) > 1L << (int)(16L) - 1L)
            {
                Fatalf("tag too long: %s", tag);
            } 

            // Encode name and tag. See reflect/type.go for details.
            byte bits = default;
            long l = 1L + 2L + len(name);
            if (exported)
            {
                bits |= 1L << (int)(0L);
            }
            if (len(tag) > 0L)
            {
                l += 2L + len(tag);
                bits |= 1L << (int)(1L);
            }
            if (pkg != null)
            {
                bits |= 1L << (int)(2L);
            }
            var b = make_slice<byte>(l);
            b[0L] = bits;
            b[1L] = uint8(len(name) >> (int)(8L));
            b[2L] = uint8(len(name));
            copy(b[3L..], name);
            if (len(tag) > 0L)
            {
                var tb = b[3L + len(name)..];
                tb[0L] = uint8(len(tag) >> (int)(8L));
                tb[1L] = uint8(len(tag));
                copy(tb[2L..], tag);
            }
            ot = int(s.WriteBytes(Ctxt, int64(ot), b));

            if (pkg != null)
            {
                ot = dgopkgpathOff(s, ot, pkg);
            }
            return ot;
        }

        private static long dnameCount = default;

        // dname creates a reflect.name for a struct field or method.
        private static ref obj.LSym dname(@string name, @string tag, ref types.Pkg pkg, bool exported)
        { 
            // Write out data as "type.." to signal two things to the
            // linker, first that when dynamically linking, the symbol
            // should be moved to a relro section, and second that the
            // contents should not be decoded as a type.
            @string sname = "type..namedata.";
            if (pkg == null)
            { 
                // In the common case, share data with other packages.
                if (name == "")
                {
                    if (exported)
                    {
                        sname += "-noname-exported." + tag;
                    }
                    else
                    {
                        sname += "-noname-unexported." + tag;
                    }
                }
                else
                {
                    if (exported)
                    {
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
            var s = Ctxt.Lookup(sname);
            if (len(s.P) > 0L)
            {
                return s;
            }
            var ot = dnameData(s, 0L, name, tag, pkg, exported);
            ggloblsym(s, int32(ot), obj.DUPOK | obj.RODATA);
            return s;
        }

        // dextratype dumps the fields of a runtime.uncommontype.
        // dataAdd is the offset in bytes after the header where the
        // backing array of the []method field is written (by dextratypeData).
        private static long dextratype(ref obj.LSym lsym, long ot, ref types.Type t, long dataAdd)
        {
            var m = methods(t);
            if (t.Sym == null && len(m) == 0L)
            {
                return ot;
            }
            var noff = int(Rnd(int64(ot), int64(Widthptr)));
            if (noff != ot)
            {
                Fatalf("unexpected alignment in dextratype for %v", t);
            }
            foreach (var (_, a) in m)
            {
                dtypesym(a.type_);
            }
            ot = dgopkgpathOff(lsym, ot, typePkg(t));

            dataAdd += uncommonSize(t);
            var mcount = len(m);
            if (mcount != int(uint16(mcount)))
            {
                Fatalf("too many methods on %v: %d", t, mcount);
            }
            if (dataAdd != int(uint32(dataAdd)))
            {
                Fatalf("methods are too far away on %v: %d", t, dataAdd);
            }
            ot = duint16(lsym, ot, uint16(mcount));
            ot = duint16(lsym, ot, 0L);
            ot = duint32(lsym, ot, uint32(dataAdd));
            ot = duint32(lsym, ot, 0L);
            return ot;
        }

        private static ref types.Pkg typePkg(ref types.Type t)
        {
            var tsym = t.Sym;
            if (tsym == null)
            {

                if (t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TCHAN) 
                    if (t.Elem() != null)
                    {
                        tsym = t.Elem().Sym;
                    }
                            }
            if (tsym != null && t != types.Types[t.Etype] && t != types.Errortype)
            {
                return tsym.Pkg;
            }
            return null;
        }

        // dextratypeData dumps the backing array for the []method field of
        // runtime.uncommontype.
        private static long dextratypeData(ref obj.LSym lsym, long ot, ref types.Type t)
        {
            foreach (var (_, a) in methods(t))
            { 
                // ../../../../runtime/type.go:/method
                var exported = exportname(a.name);
                ref types.Pkg pkg = default;
                if (!exported && a.pkg != typePkg(t))
                {
                    pkg = a.pkg;
                }
                var nsym = dname(a.name, "", pkg, exported);

                ot = dsymptrOff(lsym, ot, nsym, 0L);
                ot = dmethodptrOff(lsym, ot, dtypesym(a.mtype));
                ot = dmethodptrOff(lsym, ot, a.isym.Linksym());
                ot = dmethodptrOff(lsym, ot, a.tsym.Linksym());
            }
            return ot;
        }

        private static long dmethodptrOff(ref obj.LSym s, long ot, ref obj.LSym x)
        {
            duint32(s, ot, 0L);
            var r = obj.Addrel(s);
            r.Off = int32(ot);
            r.Siz = 4L;
            r.Sym = x;
            r.Type = objabi.R_METHODOFF;
            return ot + 4L;
        }

        private static long kinds = new slice<long>(InitKeyedValues<long>((TINT, objabi.KindInt), (TUINT, objabi.KindUint), (TINT8, objabi.KindInt8), (TUINT8, objabi.KindUint8), (TINT16, objabi.KindInt16), (TUINT16, objabi.KindUint16), (TINT32, objabi.KindInt32), (TUINT32, objabi.KindUint32), (TINT64, objabi.KindInt64), (TUINT64, objabi.KindUint64), (TUINTPTR, objabi.KindUintptr), (TFLOAT32, objabi.KindFloat32), (TFLOAT64, objabi.KindFloat64), (TBOOL, objabi.KindBool), (TSTRING, objabi.KindString), (TPTR32, objabi.KindPtr), (TPTR64, objabi.KindPtr), (TSTRUCT, objabi.KindStruct), (TINTER, objabi.KindInterface), (TCHAN, objabi.KindChan), (TMAP, objabi.KindMap), (TARRAY, objabi.KindArray), (TSLICE, objabi.KindSlice), (TFUNC, objabi.KindFunc), (TCOMPLEX64, objabi.KindComplex64), (TCOMPLEX128, objabi.KindComplex128), (TUNSAFEPTR, objabi.KindUnsafePointer)));

        // typeptrdata returns the length in bytes of the prefix of t
        // containing pointer data. Anything after this offset is scalar data.
        private static long typeptrdata(ref types.Type t)
        {
            if (!types.Haspointers(t))
            {
                return 0L;
            }

            if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TFUNC || t.Etype == TCHAN || t.Etype == TMAP) 
                return int64(Widthptr);
            else if (t.Etype == TSTRING) 
                // struct { byte *str; intgo len; }
                return int64(Widthptr);
            else if (t.Etype == TINTER) 
                // struct { Itab *tab;    void *data; } or
                // struct { Type *type; void *data; }
                return 2L * int64(Widthptr);
            else if (t.Etype == TSLICE) 
                // struct { byte *array; uintgo len; uintgo cap; }
                return int64(Widthptr);
            else if (t.Etype == TARRAY) 
                // haspointers already eliminated t.NumElem() == 0.
                return (t.NumElem() - 1L) * t.Elem().Width + typeptrdata(t.Elem());
            else if (t.Etype == TSTRUCT) 
                // Find the last field that has pointers.
                ref types.Field lastPtrField = default;
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (types.Haspointers(t1.Type))
                    {
                        lastPtrField = t1;
                    }
                }
                return lastPtrField.Offset + typeptrdata(lastPtrField.Type);
            else 
                Fatalf("typeptrdata: unexpected type, %v", t);
                return 0L;
                    }

        // tflag is documented in reflect/type.go.
        //
        // tflag values must be kept in sync with copies in:
        //    cmd/compile/internal/gc/reflect.go
        //    cmd/link/internal/ld/decodesym.go
        //    reflect/type.go
        //    runtime/type.go
        private static readonly long tflagUncommon = 1L << (int)(0L);
        private static readonly long tflagExtraStar = 1L << (int)(1L);
        private static readonly long tflagNamed = 1L << (int)(2L);

        private static ref obj.LSym algarray = default;        private static ref obj.LSym memhashvarlen = default;        private static ref obj.LSym memequalvarlen = default;

        // dcommontype dumps the contents of a reflect.rtype (runtime._type).
        private static long dcommontype(ref obj.LSym lsym, long ot, ref types.Type t)
        {
            if (ot != 0L)
            {
                Fatalf("dcommontype %d", ot);
            }
            long sizeofAlg = 2L * Widthptr;
            if (algarray == null)
            {
                algarray = sysfunc("algarray");
            }
            dowidth(t);
            var alg = algtype(t);
            ref obj.LSym algsym = default;
            if (alg == ASPECIAL || alg == AMEM)
            {
                algsym = dalgsym(t);
            }
            var sptrWeak = true;
            ref obj.LSym sptr = default;
            if (!t.IsPtr() || t.PtrBase != null)
            {
                var tptr = types.NewPtr(t);
                if (t.Sym != null || methods(tptr) != null)
                {
                    sptrWeak = false;
                }
                sptr = dtypesym(tptr);
            }
            var (gcsym, useGCProg, ptrdata) = dgcsym(t); 

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
            //        alg           *typeAlg
            //        gcdata        *byte
            //        str           nameOff
            //        ptrToThis     typeOff
            //    }
            ot = duintptr(lsym, ot, uint64(t.Width));
            ot = duintptr(lsym, ot, uint64(ptrdata));
            ot = duint32(lsym, ot, typehash(t));

            byte tflag = default;
            if (uncommonSize(t) != 0L)
            {
                tflag |= tflagUncommon;
            }
            if (t.Sym != null && t.Sym.Name != "")
            {
                tflag |= tflagNamed;
            }
            var exported = false;
            var p = t.LongString(); 
            // If we're writing out type T,
            // we are very likely to write out type *T as well.
            // Use the string "*T"[1:] for "T", so that the two
            // share storage. This is a cheap way to reduce the
            // amount of space taken up by reflect strings.
            if (!strings.HasPrefix(p, "*"))
            {
                p = "*" + p;
                tflag |= tflagExtraStar;
                if (t.Sym != null)
                {
                    exported = exportname(t.Sym.Name);
                }
            }
            else
            {
                if (t.Elem() != null && t.Elem().Sym != null)
                {
                    exported = exportname(t.Elem().Sym.Name);
                }
            }
            ot = duint8(lsym, ot, tflag); 

            // runtime (and common sense) expects alignment to be a power of two.
            var i = int(t.Align);

            if (i == 0L)
            {
                i = 1L;
            }
            if (i & (i - 1L) != 0L)
            {
                Fatalf("invalid alignment %d for %v", t.Align, t);
            }
            ot = duint8(lsym, ot, t.Align); // align
            ot = duint8(lsym, ot, t.Align); // fieldAlign

            i = kinds[t.Etype];
            if (!types.Haspointers(t))
            {
                i |= objabi.KindNoPointers;
            }
            if (isdirectiface(t))
            {
                i |= objabi.KindDirectIface;
            }
            if (useGCProg)
            {
                i |= objabi.KindGCProg;
            }
            ot = duint8(lsym, ot, uint8(i)); // kind
            if (algsym == null)
            {
                ot = dsymptr(lsym, ot, algarray, int(alg) * sizeofAlg);
            }
            else
            {
                ot = dsymptr(lsym, ot, algsym, 0L);
            }
            ot = dsymptr(lsym, ot, gcsym, 0L); // gcdata

            var nsym = dname(p, "", null, exported);
            ot = dsymptrOff(lsym, ot, nsym, 0L); // str
            // ptrToThis
            if (sptr == null)
            {
                ot = duint32(lsym, ot, 0L);
            }
            else if (sptrWeak)
            {
                ot = dsymptrWeakOff(lsym, ot, sptr);
            }
            else
            {
                ot = dsymptrOff(lsym, ot, sptr, 0L);
            }
            return ot;
        }

        // typeHasNoAlg returns whether t does not have any associated hash/eq
        // algorithms because t, or some component of t, is marked Noalg.
        private static bool typeHasNoAlg(ref types.Type t)
        {
            var (a, bad) = algtype1(t);
            return a == ANOEQ && bad.Noalg();
        }

        private static @string typesymname(ref types.Type t)
        {
            var name = t.ShortString(); 
            // Use a separate symbol name for Noalg types for #17752.
            if (typeHasNoAlg(t))
            {
                name = "noalg." + name;
            }
            return name;
        }

        // Fake package for runtime type info (headers)
        // Don't access directly, use typeLookup below.
        private static sync.Mutex typepkgmu = default;        private static var typepkg = types.NewPkg("type", "type");

        private static ref types.Sym typeLookup(@string name)
        {
            typepkgmu.Lock();
            var s = typepkg.Lookup(name);
            typepkgmu.Unlock();
            return s;
        }

        private static ref types.Sym typesym(ref types.Type t)
        {
            return typeLookup(typesymname(t));
        }

        // tracksym returns the symbol for tracking use of field/method f, assumed
        // to be a member of struct/interface type t.
        private static ref types.Sym tracksym(ref types.Type t, ref types.Field f)
        {
            return trackpkg.Lookup(t.ShortString() + "." + f.Sym.Name);
        }

        private static ref types.Sym typesymprefix(@string prefix, ref types.Type t)
        {
            var p = prefix + "." + t.ShortString();
            var s = typeLookup(p); 

            //print("algsym: %s -> %+S\n", p, s);

            return s;
        }

        private static ref types.Sym typenamesym(ref types.Type t)
        {
            if (t == null || (t.IsPtr() && t.Elem() == null) || t.IsUntyped())
            {
                Fatalf("typenamesym %v", t);
            }
            var s = typesym(t);
            signatsetmu.Lock();
            addsignat(t);
            signatsetmu.Unlock();
            return s;
        }

        private static ref Node typename(ref types.Type t)
        {
            var s = typenamesym(t);
            if (s.Def == null)
            {
                var n = newnamel(src.NoXPos, s);
                n.Type = types.Types[TUINT8];
                n.SetClass(PEXTERN);
                n.SetTypecheck(1L);
                s.Def = asTypesNode(n);
            }
            n = nod(OADDR, asNode(s.Def), null);
            n.Type = types.NewPtr(asNode(s.Def).Type);
            n.SetAddable(true);
            n.SetTypecheck(1L);
            return n;
        }

        private static ref Node itabname(ref types.Type t, ref types.Type itype)
        {
            if (t == null || (t.IsPtr() && t.Elem() == null) || t.IsUntyped() || !itype.IsInterface() || itype.IsEmptyInterface())
            {
                Fatalf("itabname(%v, %v)", t, itype);
            }
            var s = itabpkg.Lookup(t.ShortString() + "," + itype.ShortString());
            if (s.Def == null)
            {
                var n = newname(s);
                n.Type = types.Types[TUINT8];
                n.SetClass(PEXTERN);
                n.SetTypecheck(1L);
                s.Def = asTypesNode(n);
                itabs = append(itabs, new itabEntry(t:t,itype:itype,lsym:s.Linksym()));
            }
            n = nod(OADDR, asNode(s.Def), null);
            n.Type = types.NewPtr(asNode(s.Def).Type);
            n.SetAddable(true);
            n.SetTypecheck(1L);
            return n;
        }

        // isreflexive reports whether t has a reflexive equality operator.
        // That is, if x==x for all x of type t.
        private static bool isreflexive(ref types.Type t)
        {

            if (t.Etype == TBOOL || t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TSTRING || t.Etype == TCHAN) 
                return true;
            else if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TINTER) 
                return false;
            else if (t.Etype == TARRAY) 
                return isreflexive(t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (!isreflexive(t1.Type))
                    {
                        return false;
                    }
                }
                return true;
            else 
                Fatalf("bad type for map key: %v", t);
                return false;
                    }

        // needkeyupdate reports whether map updates with t as a key
        // need the key to be updated.
        private static bool needkeyupdate(ref types.Type t)
        {

            if (t.Etype == TBOOL || t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TCHAN) 
                return false;
            else if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TINTER || t.Etype == TSTRING) // strings might have smaller backing stores
                return true;
            else if (t.Etype == TARRAY) 
                return needkeyupdate(t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (needkeyupdate(t1.Type))
                    {
                        return true;
                    }
                }
                return false;
            else 
                Fatalf("bad type for map key: %v", t);
                return true;
                    }

        // formalType replaces byte and rune aliases with real types.
        // They've been separate internally to make error messages
        // better, but we have to merge them in the reflect tables.
        private static ref types.Type formalType(ref types.Type t)
        {
            if (t == types.Bytetype || t == types.Runetype)
            {
                return types.Types[t.Etype];
            }
            return t;
        }

        private static ref obj.LSym dtypesym(ref types.Type t)
        {
            t = formalType(t);
            if (t.IsUntyped())
            {
                Fatalf("dtypesym %v", t);
            }
            var s = typesym(t);
            var lsym = s.Linksym();
            if (s.Siggen())
            {
                return lsym;
            }
            s.SetSiggen(true); 

            // special case (look for runtime below):
            // when compiling package runtime,
            // emit the type structures for int, float, etc.
            var tbase = t;

            if (t.IsPtr() && t.Sym == null && t.Elem().Sym != null)
            {
                tbase = t.Elem();
            }
            long dupok = 0L;
            if (tbase.Sym == null)
            {
                dupok = obj.DUPOK;
            }
            if (myimportpath != "runtime" || (tbase != types.Types[tbase.Etype] && tbase != types.Bytetype && tbase != types.Runetype && tbase != types.Errortype))
            { // int, float, etc
                // named types from other files are defined only by those files
                if (tbase.Sym != null && tbase.Sym.Pkg != localpkg)
                {
                    return lsym;
                } 
                // TODO(mdempsky): Investigate whether this can happen.
                if (isforw[tbase.Etype])
                {
                    return lsym;
                }
            }
            long ot = 0L;

            if (t.Etype == TARRAY) 
                // ../../../../runtime/type.go:/arrayType
                var s1 = dtypesym(t.Elem());
                var t2 = types.NewSlice(t.Elem());
                var s2 = dtypesym(t2);
                ot = dcommontype(lsym, ot, t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dsymptr(lsym, ot, s2, 0L);
                ot = duintptr(lsym, ot, uint64(t.NumElem()));
                ot = dextratype(lsym, ot, t, 0L);
            else if (t.Etype == TSLICE) 
                // ../../../../runtime/type.go:/sliceType
                s1 = dtypesym(t.Elem());
                ot = dcommontype(lsym, ot, t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dextratype(lsym, ot, t, 0L);
            else if (t.Etype == TCHAN) 
                // ../../../../runtime/type.go:/chanType
                s1 = dtypesym(t.Elem());
                ot = dcommontype(lsym, ot, t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = duintptr(lsym, ot, uint64(t.ChanDir()));
                ot = dextratype(lsym, ot, t, 0L);
            else if (t.Etype == TFUNC) 
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Recvs().Fields().Slice())
                    {
                        t1 = __t1;
                        dtypesym(t1.Type);
                    }

                    t1 = t1__prev1;
                }

                var isddd = false;
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Params().Fields().Slice())
                    {
                        t1 = __t1;
                        isddd = t1.Isddd();
                        dtypesym(t1.Type);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Results().Fields().Slice())
                    {
                        t1 = __t1;
                        dtypesym(t1.Type);
                    }

                    t1 = t1__prev1;
                }

                ot = dcommontype(lsym, ot, t);
                var inCount = t.NumRecvs() + t.NumParams();
                var outCount = t.NumResults();
                if (isddd)
                {
                    outCount |= 1L << (int)(15L);
                }
                ot = duint16(lsym, ot, uint16(inCount));
                ot = duint16(lsym, ot, uint16(outCount));
                if (Widthptr == 8L)
                {
                    ot += 4L; // align for *rtype
                }
                var dataAdd = (inCount + t.NumResults()) * Widthptr;
                ot = dextratype(lsym, ot, t, dataAdd); 

                // Array of rtype pointers follows funcType.
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Recvs().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Params().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Results().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }
            else if (t.Etype == TINTER) 
                var m = imethods(t);
                var n = len(m);
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m)
                    {
                        a = __a;
                        dtypesym(a.type_);
                    } 

                    // ../../../../runtime/type.go:/interfaceType

                    a = a__prev1;
                }

                ot = dcommontype(lsym, ot, t);

                ref types.Pkg tpkg = default;
                if (t.Sym != null && t != types.Types[t.Etype] && t != types.Errortype)
                {
                    tpkg = t.Sym.Pkg;
                }
                ot = dgopkgpath(lsym, ot, tpkg);

                ot = dsymptr(lsym, ot, lsym, ot + 3L * Widthptr + uncommonSize(t));
                ot = duintptr(lsym, ot, uint64(n));
                ot = duintptr(lsym, ot, uint64(n));
                dataAdd = imethodSize() * n;
                ot = dextratype(lsym, ot, t, dataAdd);

                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m)
                    {
                        a = __a; 
                        // ../../../../runtime/type.go:/imethod
                        var exported = exportname(a.name);
                        ref types.Pkg pkg = default;
                        if (!exported && a.pkg != tpkg)
                        {
                            pkg = a.pkg;
                        }
                        var nsym = dname(a.name, "", pkg, exported);

                        ot = dsymptrOff(lsym, ot, nsym, 0L);
                        ot = dsymptrOff(lsym, ot, dtypesym(a.type_), 0L);
                    } 

                    // ../../../../runtime/type.go:/mapType

                    a = a__prev1;
                }
            else if (t.Etype == TMAP) 
                s1 = dtypesym(t.Key());
                s2 = dtypesym(t.Val());
                var s3 = dtypesym(bmap(t));
                var s4 = dtypesym(hmap(t));
                ot = dcommontype(lsym, ot, t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dsymptr(lsym, ot, s2, 0L);
                ot = dsymptr(lsym, ot, s3, 0L);
                ot = dsymptr(lsym, ot, s4, 0L);
                if (t.Key().Width > MAXKEYSIZE)
                {
                    ot = duint8(lsym, ot, uint8(Widthptr));
                    ot = duint8(lsym, ot, 1L); // indirect
                }
                else
                {
                    ot = duint8(lsym, ot, uint8(t.Key().Width));
                    ot = duint8(lsym, ot, 0L); // not indirect
                }
                if (t.Val().Width > MAXVALSIZE)
                {
                    ot = duint8(lsym, ot, uint8(Widthptr));
                    ot = duint8(lsym, ot, 1L); // indirect
                }
                else
                {
                    ot = duint8(lsym, ot, uint8(t.Val().Width));
                    ot = duint8(lsym, ot, 0L); // not indirect
                }
                ot = duint16(lsym, ot, uint16(bmap(t).Width));
                ot = duint8(lsym, ot, uint8(obj.Bool2int(isreflexive(t.Key()))));
                ot = duint8(lsym, ot, uint8(obj.Bool2int(needkeyupdate(t.Key()))));
                ot = dextratype(lsym, ot, t, 0L);
            else if (t.Etype == TPTR32 || t.Etype == TPTR64) 
                if (t.Elem().Etype == TANY)
                { 
                    // ../../../../runtime/type.go:/UnsafePointerType
                    ot = dcommontype(lsym, ot, t);
                    ot = dextratype(lsym, ot, t, 0L);

                    break;
                } 

                // ../../../../runtime/type.go:/ptrType
                s1 = dtypesym(t.Elem());

                ot = dcommontype(lsym, ot, t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dextratype(lsym, ot, t, 0L); 

                // ../../../../runtime/type.go:/structType
                // for security, only the exported fields.
            else if (t.Etype == TSTRUCT) 
                var fields = t.Fields().Slice();
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in fields)
                    {
                        t1 = __t1;
                        dtypesym(t1.Type);
                    } 

                    // All non-exported struct field names within a struct
                    // type must originate from a single package. By
                    // identifying and recording that package within the
                    // struct type descriptor, we can omit that
                    // information from the field descriptors.

                    t1 = t1__prev1;
                }

                ref types.Pkg spkg = default;
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in fields)
                    {
                        f = __f;
                        if (!exportname(f.Sym.Name))
                        {
                            spkg = f.Sym.Pkg;
                            break;
                        }
                    }

                    f = f__prev1;
                }

                ot = dcommontype(lsym, ot, t);
                ot = dgopkgpath(lsym, ot, spkg);
                ot = dsymptr(lsym, ot, lsym, ot + 3L * Widthptr + uncommonSize(t));
                ot = duintptr(lsym, ot, uint64(len(fields)));
                ot = duintptr(lsym, ot, uint64(len(fields)));

                dataAdd = len(fields) * structfieldSize();
                ot = dextratype(lsym, ot, t, dataAdd);

                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in fields)
                    {
                        f = __f; 
                        // ../../../../runtime/type.go:/structField
                        ot = dnameField(lsym, ot, spkg, f);
                        ot = dsymptr(lsym, ot, dtypesym(f.Type), 0L);
                        var offsetAnon = uint64(f.Offset) << (int)(1L);
                        if (offsetAnon >> (int)(1L) != uint64(f.Offset))
                        {
                            Fatalf("%v: bad field offset for %s", t, f.Sym.Name);
                        }
                        if (f.Embedded != 0L)
                        {
                            offsetAnon |= 1L;
                        }
                        ot = duintptr(lsym, ot, offsetAnon);
                    }

                    f = f__prev1;
                }
            else 
                ot = dcommontype(lsym, ot, t);
                ot = dextratype(lsym, ot, t, 0L);
                        ot = dextratypeData(lsym, ot, t);
            ggloblsym(lsym, int32(ot), int16(dupok | obj.RODATA)); 

            // The linker will leave a table of all the typelinks for
            // types in the binary, so the runtime can find them.
            //
            // When buildmode=shared, all types are in typelinks so the
            // runtime can deduplicate type pointers.
            var keep = Ctxt.Flag_dynlink;
            if (!keep && t.Sym == null)
            { 
                // For an unnamed type, we only need the link if the type can
                // be created at run time by reflect.PtrTo and similar
                // functions. If the type exists in the program, those
                // functions must return the existing type structure rather
                // than creating a new one.

                if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TARRAY || t.Etype == TCHAN || t.Etype == TFUNC || t.Etype == TMAP || t.Etype == TSLICE || t.Etype == TSTRUCT) 
                    keep = true;
                            } 
            // Do not put Noalg types in typelinks.  See issue #22605.
            if (typeHasNoAlg(t))
            {
                keep = false;
            }
            lsym.Set(obj.AttrMakeTypelink, keep);

            return lsym;
        }

        // for each itabEntry, gather the methods on
        // the concrete type that implement the interface
        private static void peekitabs()
        {
            foreach (var (i) in itabs)
            {
                var tab = ref itabs[i];
                var methods = genfun(tab.t, tab.itype);
                if (len(methods) == 0L)
                {
                    continue;
                }
                tab.entries = methods;
            }
        }

        // for the given concrete type and interface
        // type, return the (sorted) set of methods
        // on the concrete type that implement the interface
        private static slice<ref obj.LSym> genfun(ref types.Type t, ref types.Type it)
        {
            if (t == null || it == null)
            {
                return null;
            }
            var sigs = imethods(it);
            var methods = methods(t);
            var @out = make_slice<ref obj.LSym>(0L, len(sigs));
            if (len(sigs) == 0L)
            {
                return null;
            } 

            // both sigs and methods are sorted by name,
            // so we can find the intersect in a single pass
            foreach (var (_, m) in methods)
            {
                if (m.name == sigs[0L].name)
                {
                    out = append(out, m.isym.Linksym());
                    sigs = sigs[1L..];
                    if (len(sigs) == 0L)
                    {
                        break;
                    }
                }
            }
            return out;
        }

        // itabsym uses the information gathered in
        // peekitabs to de-virtualize interface methods.
        // Since this is called by the SSA backend, it shouldn't
        // generate additional Nodes, Syms, etc.
        private static ref obj.LSym itabsym(ref obj.LSym it, long offset)
        {
            slice<ref obj.LSym> syms = default;
            if (it == null)
            {
                return null;
            }
            foreach (var (i) in itabs)
            {
                var e = ref itabs[i];
                if (e.lsym == it)
                {
                    syms = e.entries;
                    break;
                }
            }
            if (syms == null)
            {
                return null;
            } 

            // keep this arithmetic in sync with *itab layout
            var methodnum = int((offset - 2L * int64(Widthptr) - 8L) / int64(Widthptr));
            if (methodnum >= len(syms))
            {
                return null;
            }
            return syms[methodnum];
        }

        private static void addsignat(ref types.Type t)
        {
            signatset[t] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        }

        private static void addsignats(slice<ref Node> dcls)
        { 
            // copy types from dcl list to signatset
            foreach (var (_, n) in dcls)
            {
                if (n.Op == OTYPE)
                {
                    addsignat(n.Type);
                }
            }
        }

        private static void dumpsignats()
        { 
            // Process signatset. Use a loop, as dtypesym adds
            // entries to signatset while it is being processed.
            var signats = make_slice<typeAndStr>(len(signatset));
            while (len(signatset) > 0L)
            {
                signats = signats[..0L]; 
                // Transfer entries to a slice and sort, for reproducible builds.
                {
                    var t__prev2 = t;

                    foreach (var (__t) in signatset)
                    {
                        t = __t;
                        signats = append(signats, new typeAndStr(t:t,short:typesymname(t),regular:t.String()));
                        delete(signatset, t);
                    }

                    t = t__prev2;
                }

                sort.Sort(typesByString(signats));
                foreach (var (_, ts) in signats)
                {
                    var t = ts.t;
                    dtypesym(t);
                    if (t.Sym != null)
                    {
                        dtypesym(types.NewPtr(t));
                    }
                }
            }

        }

        private static void dumptabs()
        { 
            // process itabs
            foreach (var (_, i) in itabs)
            { 
                // dump empty itab symbol into i.sym
                // type itab struct {
                //   inter  *interfacetype
                //   _type  *_type
                //   hash   uint32
                //   _      [4]byte
                //   fun    [1]uintptr // variable sized
                // }
                var o = dsymptr(i.lsym, 0L, dtypesym(i.itype), 0L);
                o = dsymptr(i.lsym, o, dtypesym(i.t), 0L);
                o = duint32(i.lsym, o, typehash(i.t)); // copy of type hash
                o += 4L; // skip unused field
                foreach (var (_, fn) in genfun(i.t, i.itype))
                {
                    o = dsymptr(i.lsym, o, fn, 0L); // method pointer for each method
                } 
                // Nothing writes static itabs, so they are read only.
                ggloblsym(i.lsym, int32(o), int16(obj.DUPOK | obj.RODATA));
                var ilink = itablinkpkg.Lookup(i.t.ShortString() + "," + i.itype.ShortString()).Linksym();
                dsymptr(ilink, 0L, i.lsym, 0L);
                ggloblsym(ilink, int32(Widthptr), int16(obj.DUPOK | obj.RODATA));
            } 

            // process ptabs
            if (localpkg.Name == "main" && len(ptabs) > 0L)
            {
                long ot = 0L;
                var s = Ctxt.Lookup("go.plugin.tabs");
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in ptabs)
                    {
                        p = __p; 
                        // Dump ptab symbol into go.pluginsym package.
                        //
                        // type ptab struct {
                        //    name nameOff
                        //    typ  typeOff // pointer to symbol
                        // }
                        var nsym = dname(p.s.Name, "", null, true);
                        ot = dsymptrOff(s, ot, nsym, 0L);
                        ot = dsymptrOff(s, ot, dtypesym(p.t), 0L);
                    }

                    p = p__prev1;
                }

                ggloblsym(s, int32(ot), int16(obj.RODATA));

                ot = 0L;
                s = Ctxt.Lookup("go.plugin.exports");
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in ptabs)
                    {
                        p = __p;
                        ot = dsymptr(s, ot, p.s.Linksym(), 0L);
                    }

                    p = p__prev1;
                }

                ggloblsym(s, int32(ot), int16(obj.RODATA));
            }
        }

        private static void dumpimportstrings()
        { 
            // generate import strings for imported packages
            foreach (var (_, p) in types.ImportedPkgList())
            {
                dimportpath(p);
            }
        }

        private static void dumpbasictypes()
        { 
            // do basic types if compiling package runtime.
            // they have to be in at least one package,
            // and runtime is always loaded implicitly,
            // so this is as good as any.
            // another possible choice would be package main,
            // but using runtime means fewer copies in object files.
            if (myimportpath == "runtime")
            {
                for (var i = types.EType(1L); i <= TBOOL; i++)
                {
                    dtypesym(types.NewPtr(types.Types[i]));
                }

                dtypesym(types.NewPtr(types.Types[TSTRING]));
                dtypesym(types.NewPtr(types.Types[TUNSAFEPTR])); 

                // emit type structs for error and func(error) string.
                // The latter is the type of an auto-generated wrapper.
                dtypesym(types.NewPtr(types.Errortype));

                dtypesym(functype(null, new slice<ref Node>(new ref Node[] { anonfield(types.Errortype) }), new slice<ref Node>(new ref Node[] { anonfield(types.Types[TSTRING]) }))); 

                // add paths for runtime and main, which 6l imports implicitly.
                dimportpath(Runtimepkg);

                if (flag_race)
                {
                    dimportpath(racepkg);
                }
                if (flag_msan)
                {
                    dimportpath(msanpkg);
                }
                dimportpath(types.NewPkg("main", ""));
            }
        }

        private partial struct typeAndStr
        {
            public ptr<types.Type> t;
            public @string @short;
            public @string regular;
        }

        private partial struct typesByString // : slice<typeAndStr>
        {
        }

        private static long Len(this typesByString a)
        {
            return len(a);
        }
        private static bool Less(this typesByString a, long i, long j)
        {
            if (a[i].@short != a[j].@short)
            {
                return a[i].@short < a[j].@short;
            } 
            // When the only difference between the types is whether
            // they refer to byte or uint8, such as **byte vs **uint8,
            // the types' ShortStrings can be identical.
            // To preserve deterministic sort ordering, sort these by String().
            return a[i].regular < a[j].regular;
        }
        private static void Swap(this typesByString a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        private static ref obj.LSym dalgsym(ref types.Type t)
        {
            ref obj.LSym lsym = default;
            ref obj.LSym hashfunc = default;
            ref obj.LSym eqfunc = default; 

            // dalgsym is only called for a type that needs an algorithm table,
            // which implies that the type is comparable (or else it would use ANOEQ).

            if (algtype(t) == AMEM)
            { 
                // we use one algorithm table for all AMEM types of a given size
                var p = fmt.Sprintf(".alg%d", t.Width);

                var s = typeLookup(p);
                lsym = s.Linksym();
                if (s.AlgGen())
                {
                    return lsym;
                }
                s.SetAlgGen(true);

                if (memhashvarlen == null)
                {
                    memhashvarlen = sysfunc("memhash_varlen");
                    memequalvarlen = sysfunc("memequal_varlen");
                } 

                // make hash closure
                p = fmt.Sprintf(".hashfunc%d", t.Width);

                hashfunc = typeLookup(p).Linksym();

                long ot = 0L;
                ot = dsymptr(hashfunc, ot, memhashvarlen, 0L);
                ot = duintptr(hashfunc, ot, uint64(t.Width)); // size encoded in closure
                ggloblsym(hashfunc, int32(ot), obj.DUPOK | obj.RODATA); 

                // make equality closure
                p = fmt.Sprintf(".eqfunc%d", t.Width);

                eqfunc = typeLookup(p).Linksym();

                ot = 0L;
                ot = dsymptr(eqfunc, ot, memequalvarlen, 0L);
                ot = duintptr(eqfunc, ot, uint64(t.Width));
                ggloblsym(eqfunc, int32(ot), obj.DUPOK | obj.RODATA);
            }
            else
            { 
                // generate an alg table specific to this type
                s = typesymprefix(".alg", t);
                lsym = s.Linksym();

                var hash = typesymprefix(".hash", t);
                var eq = typesymprefix(".eq", t);
                hashfunc = typesymprefix(".hashfunc", t).Linksym();
                eqfunc = typesymprefix(".eqfunc", t).Linksym();

                genhash(hash, t);
                geneq(eq, t); 

                // make Go funcs (closures) for calling hash and equal from Go
                dsymptr(hashfunc, 0L, hash.Linksym(), 0L);
                ggloblsym(hashfunc, int32(Widthptr), obj.DUPOK | obj.RODATA);
                dsymptr(eqfunc, 0L, eq.Linksym(), 0L);
                ggloblsym(eqfunc, int32(Widthptr), obj.DUPOK | obj.RODATA);
            } 

            // ../../../../runtime/alg.go:/typeAlg
            ot = 0L;

            ot = dsymptr(lsym, ot, hashfunc, 0L);
            ot = dsymptr(lsym, ot, eqfunc, 0L);
            ggloblsym(lsym, int32(ot), obj.DUPOK | obj.RODATA);
            return lsym;
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
        private static readonly long maxPtrmaskBytes = 2048L;

        // dgcsym emits and returns a data symbol containing GC information for type t,
        // along with a boolean reporting whether the UseGCProg bit should be set in
        // the type kind, and the ptrdata field to record in the reflect type information.


        // dgcsym emits and returns a data symbol containing GC information for type t,
        // along with a boolean reporting whether the UseGCProg bit should be set in
        // the type kind, and the ptrdata field to record in the reflect type information.
        private static (ref obj.LSym, bool, long) dgcsym(ref types.Type t)
        {
            ptrdata = typeptrdata(t);
            if (ptrdata / int64(Widthptr) <= maxPtrmaskBytes * 8L)
            {
                lsym = dgcptrmask(t);
                return;
            }
            useGCProg = true;
            lsym, ptrdata = dgcprog(t);
            return;
        }

        // dgcptrmask emits and returns the symbol containing a pointer mask for type t.
        private static ref obj.LSym dgcptrmask(ref types.Type t)
        {
            var ptrmask = make_slice<byte>((typeptrdata(t) / int64(Widthptr) + 7L) / 8L);
            fillptrmask(t, ptrmask);
            var p = fmt.Sprintf("gcbits.%x", ptrmask);

            var sym = Runtimepkg.Lookup(p);
            var lsym = sym.Linksym();
            if (!sym.Uniq())
            {
                sym.SetUniq(true);
                foreach (var (i, x) in ptrmask)
                {
                    duint8(lsym, i, x);
                }
                ggloblsym(lsym, int32(len(ptrmask)), obj.DUPOK | obj.RODATA | obj.LOCAL);
            }
            return lsym;
        }

        // fillptrmask fills in ptrmask with 1s corresponding to the
        // word offsets in t that hold pointers.
        // ptrmask is assumed to fit at least typeptrdata(t)/Widthptr bits.
        private static void fillptrmask(ref types.Type t, slice<byte> ptrmask)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in ptrmask)
                {
                    i = __i;
                    ptrmask[i] = 0L;
                }

                i = i__prev1;
            }

            if (!types.Haspointers(t))
            {
                return;
            }
            var vec = bvalloc(8L * int32(len(ptrmask)));
            onebitwalktype1(t, 0L, vec);

            var nptr = typeptrdata(t) / int64(Widthptr);
            {
                var i__prev1 = i;

                for (var i = int64(0L); i < nptr; i++)
                {
                    if (vec.Get(int32(i)))
                    {
                        ptrmask[i / 8L] |= 1L << (int)((uint(i) % 8L));
                    }
                }


                i = i__prev1;
            }
        }

        // dgcprog emits and returns the symbol containing a GC program for type t
        // along with the size of the data described by the program (in the range [typeptrdata(t), t.Width]).
        // In practice, the size is typeptrdata(t) except for non-trivial arrays.
        // For non-trivial arrays, the program describes the full t.Width size.
        private static (ref obj.LSym, long) dgcprog(ref types.Type t)
        {
            dowidth(t);
            if (t.Width == BADWIDTH)
            {
                Fatalf("dgcprog: %v badwidth", t);
            }
            var lsym = typesymprefix(".gcprog", t).Linksym();
            GCProg p = default;
            p.init(lsym);
            p.emit(t, 0L);
            var offset = p.w.BitIndex() * int64(Widthptr);
            p.end();
            {
                var ptrdata = typeptrdata(t);

                if (offset < ptrdata || offset > t.Width)
                {
                    Fatalf("dgcprog: %v: offset=%d but ptrdata=%d size=%d", t, offset, ptrdata, t.Width);
                }

            }
            return (lsym, offset);
        }

        public partial struct GCProg
        {
            public ptr<obj.LSym> lsym;
            public long symoff;
            public gcprog.Writer w;
        }

        public static long Debug_gcprog = default; // set by -d gcprog

        private static void init(this ref GCProg p, ref obj.LSym lsym)
        {
            p.lsym = lsym;
            p.symoff = 4L; // first 4 bytes hold program length
            p.w.Init(p.writeByte);
            if (Debug_gcprog > 0L)
            {
                fmt.Fprintf(os.Stderr, "compile: start GCProg for %v\n", lsym);
                p.w.Debug(os.Stderr);
            }
        }

        private static void writeByte(this ref GCProg p, byte x)
        {
            p.symoff = duint8(p.lsym, p.symoff, x);
        }

        private static void end(this ref GCProg p)
        {
            p.w.End();
            duint32(p.lsym, 0L, uint32(p.symoff - 4L));
            ggloblsym(p.lsym, int32(p.symoff), obj.DUPOK | obj.RODATA | obj.LOCAL);
            if (Debug_gcprog > 0L)
            {
                fmt.Fprintf(os.Stderr, "compile: end GCProg for %v\n", p.lsym);
            }
        }

        private static void emit(this ref GCProg p, ref types.Type t, long offset)
        {
            dowidth(t);
            if (!types.Haspointers(t))
            {
                return;
            }
            if (t.Width == int64(Widthptr))
            {
                p.w.Ptr(offset / int64(Widthptr));
                return;
            }

            if (t.Etype == TSTRING) 
                p.w.Ptr(offset / int64(Widthptr));
            else if (t.Etype == TINTER) 
                p.w.Ptr(offset / int64(Widthptr));
                p.w.Ptr(offset / int64(Widthptr) + 1L);
            else if (t.Etype == TSLICE) 
                p.w.Ptr(offset / int64(Widthptr));
            else if (t.Etype == TARRAY) 
                if (t.NumElem() == 0L)
                { 
                    // should have been handled by haspointers check above
                    Fatalf("GCProg.emit: empty array");
                } 

                // Flatten array-of-array-of-array to just a big array by multiplying counts.
                var count = t.NumElem();
                var elem = t.Elem();
                while (elem.IsArray())
                {
                    count *= elem.NumElem();
                    elem = elem.Elem();
                }


                if (!p.w.ShouldRepeat(elem.Width / int64(Widthptr), count))
                { 
                    // Cheaper to just emit the bits.
                    for (var i = int64(0L); i < count; i++)
                    {
                        p.emit(elem, offset + i * elem.Width);
                    }

                    return;
                }
                p.emit(elem, offset);
                p.w.ZeroUntil((offset + elem.Width) / int64(Widthptr));
                p.w.Repeat(elem.Width / int64(Widthptr), count - 1L);
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    p.emit(t1.Type, offset + t1.Offset);
                }
            else 
                Fatalf("GCProg.emit: unexpected type %v", t);
                    }

        // zeroaddr returns the address of a symbol with at least
        // size bytes of zeros.
        private static ref Node zeroaddr(long size)
        {
            if (size >= 1L << (int)(31L))
            {
                Fatalf("map value too big %d", size);
            }
            if (zerosize < size)
            {
                zerosize = size;
            }
            var s = mappkg.Lookup("zero");
            if (s.Def == null)
            {
                var x = newname(s);
                x.Type = types.Types[TUINT8];
                x.SetClass(PEXTERN);
                x.SetTypecheck(1L);
                s.Def = asTypesNode(x);
            }
            var z = nod(OADDR, asNode(s.Def), null);
            z.Type = types.NewPtr(types.Types[TUINT8]);
            z.SetAddable(true);
            z.SetTypecheck(1L);
            return z;
        }
    }
}}}}
