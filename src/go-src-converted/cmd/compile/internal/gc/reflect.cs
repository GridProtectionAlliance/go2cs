// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:30 UTC
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
            public slice<ptr<obj.LSym>> entries;
        }

        private partial struct ptabEntry
        {
            public ptr<types.Sym> s;
            public ptr<types.Type> t;
        }

        // runtime interface and reflection data structures
        private static sync.Mutex signatmu = default;        private static var signatset = make();        private static slice<ptr<types.Type>> signatslice = default;        private static slice<itabEntry> itabs = default;        private static slice<ptabEntry> ptabs = default;

        public partial struct Sig
        {
            public ptr<types.Sym> name;
            public ptr<types.Sym> isym;
            public ptr<types.Sym> tsym;
            public ptr<types.Type> type_;
            public ptr<types.Type> mtype;
        }

        // Builds a type representing a Bucket structure for
        // the given map type. This type is not visible to users -
        // we include only enough information to generate a correct GC
        // program for it.
        // Make sure this stays in sync with runtime/map.go.
        public static readonly long BUCKETSIZE = (long)8L;
        public static readonly long MAXKEYSIZE = (long)128L;
        public static readonly long MAXELEMSIZE = (long)128L;


        private static long structfieldSize()
        {
            return 3L * Widthptr;
        } // Sizeof(runtime.structfield{})
        private static long imethodSize()
        {
            return 4L + 4L;
        } // Sizeof(runtime.imethod{})

        private static long uncommonSize(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 // Sizeof(runtime.uncommontype{})
            if (t.Sym == null && len(methods(_addr_t)) == 0L)
            {
                return 0L;
            }

            return 4L + 2L + 2L + 4L + 4L;

        }

        private static ptr<types.Field> makefield(@string name, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var f = types.NewField();
            f.Type = t;
            f.Sym = (types.Pkg.val)(null).Lookup(name);
            return _addr_f!;
        }

        // bmap makes the map bucket type given the type of the map.
        private static ptr<types.Type> bmap(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.MapType().Bucket != null)
            {
                return _addr_t.MapType().Bucket!;
            }

            var bucket = types.New(TSTRUCT);
            var keytype = t.Key();
            var elemtype = t.Elem();
            dowidth(keytype);
            dowidth(elemtype);
            if (keytype.Width > MAXKEYSIZE)
            {
                keytype = types.NewPtr(keytype);
            }

            if (elemtype.Width > MAXELEMSIZE)
            {
                elemtype = types.NewPtr(elemtype);
            }

            var field = make_slice<ptr<types.Field>>(0L, 5L); 

            // The first field is: uint8 topbits[BUCKETSIZE].
            var arr = types.NewArray(types.Types[TUINT8], BUCKETSIZE);
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
            var otyp = types.NewPtr(bucket);
            if (!types.Haspointers(elemtype) && !types.Haspointers(keytype))
            {
                otyp = types.Types[TUINTPTR];
            }

            var overflow = makefield("overflow", _addr_otyp);
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

            if (elemtype.Align > BUCKETSIZE)
            {
                Fatalf("elem align too big for %v", t);
            }

            if (keytype.Width > MAXKEYSIZE)
            {
                Fatalf("key size to large for %v", t);
            }

            if (elemtype.Width > MAXELEMSIZE)
            {
                Fatalf("elem size to large for %v", t);
            }

            if (t.Key().Width > MAXKEYSIZE && !keytype.IsPtr())
            {
                Fatalf("key indirect incorrect for %v", t);
            }

            if (t.Elem().Width > MAXELEMSIZE && !elemtype.IsPtr())
            {
                Fatalf("elem indirect incorrect for %v", t);
            }

            if (keytype.Width % int64(keytype.Align) != 0L)
            {
                Fatalf("key size not a multiple of key align for %v", t);
            }

            if (elemtype.Width % int64(elemtype.Align) != 0L)
            {
                Fatalf("elem size not a multiple of elem align for %v", t);
            }

            if (bucket.Align % keytype.Align != 0L)
            {
                Fatalf("bucket align not multiple of key align %v", t);
            }

            if (bucket.Align % elemtype.Align != 0L)
            {
                Fatalf("bucket align not multiple of elem align %v", t);
            }

            if (keys.Offset % int64(keytype.Align) != 0L)
            {
                Fatalf("bad alignment of keys in bmap for %v", t);
            }

            if (elems.Offset % int64(elemtype.Align) != 0L)
            {
                Fatalf("bad alignment of elems in bmap for %v", t);
            } 

            // Double-check that overflow field is final memory in struct,
            // with no padding at end.
            if (overflow.Offset != bucket.Width - int64(Widthptr))
            {
                Fatalf("bad offset of overflow in bmap for %v", t);
            }

            t.MapType().Bucket = bucket;

            bucket.StructType().Map = t;
            return _addr_bucket!;

        }

        // hmap builds a type representing a Hmap structure for the given map type.
        // Make sure this stays in sync with runtime/map.go.
        private static ptr<types.Type> hmap(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.MapType().Hmap != null)
            {
                return _addr_t.MapType().Hmap!;
            }

            var bmap = bmap(_addr_t); 

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
            ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("count",types.Types[TINT]), makefield("flags",types.Types[TUINT8]), makefield("B",types.Types[TUINT8]), makefield("noverflow",types.Types[TUINT16]), makefield("hash0",types.Types[TUINT32]), makefield("buckets",types.NewPtr(bmap)), makefield("oldbuckets",types.NewPtr(bmap)), makefield("nevacuate",types.Types[TUINTPTR]), makefield("extra",types.Types[TUNSAFEPTR]) });

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
            return _addr_hmap!;

        }

        // hiter builds a type representing an Hiter structure for the given map type.
        // Make sure this stays in sync with runtime/map.go.
        private static ptr<types.Type> hiter(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.MapType().Hiter != null)
            {
                return _addr_t.MapType().Hiter!;
            }

            var hmap = hmap(_addr_t);
            var bmap = bmap(_addr_t); 

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
            ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("key",types.NewPtr(t.Key())), makefield("elem",types.NewPtr(t.Elem())), makefield("t",types.Types[TUNSAFEPTR]), makefield("h",types.NewPtr(hmap)), makefield("buckets",types.NewPtr(bmap)), makefield("bptr",types.NewPtr(bmap)), makefield("overflow",types.Types[TUNSAFEPTR]), makefield("oldoverflow",types.Types[TUNSAFEPTR]), makefield("startBucket",types.Types[TUINTPTR]), makefield("offset",types.Types[TUINT8]), makefield("wrapped",types.Types[TBOOL]), makefield("B",types.Types[TUINT8]), makefield("i",types.Types[TUINT8]), makefield("bucket",types.Types[TUINTPTR]), makefield("checkBucket",types.Types[TUINTPTR]) }); 

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
            return _addr_hiter!;

        }

        // deferstruct makes a runtime._defer structure, with additional space for
        // stksize bytes of args.
        private static ptr<types.Type> deferstruct(long stksize)
        {
            Func<@string, ptr<types.Type>, ptr<types.Field>> makefield = (name, typ) =>
            {
                var f = types.NewField();
                f.Type = typ; 
                // Unlike the global makefield function, this one needs to set Pkg
                // because these types might be compared (in SSA CSE sorting).
                // TODO: unify this makefield and the global one above.
                f.Sym = addr(new types.Sym(Name:name,Pkg:localpkg));
                return _addr_f!;

            }
;
            var argtype = types.NewArray(types.Types[TUINT8], stksize);
            argtype.Width = stksize;
            argtype.Align = 1L; 
            // These fields must match the ones in runtime/runtime2.go:_defer and
            // cmd/compile/internal/gc/ssa.go:(*state).call.
            ptr<types.Field> fields = new slice<ptr<types.Field>>(new ptr<types.Field>[] { makefield("siz",types.Types[TUINT32]), makefield("started",types.Types[TBOOL]), makefield("heap",types.Types[TBOOL]), makefield("openDefer",types.Types[TBOOL]), makefield("sp",types.Types[TUINTPTR]), makefield("pc",types.Types[TUINTPTR]), makefield("fn",types.Types[TUINTPTR]), makefield("_panic",types.Types[TUINTPTR]), makefield("link",types.Types[TUINTPTR]), makefield("framepc",types.Types[TUINTPTR]), makefield("varp",types.Types[TUINTPTR]), makefield("fd",types.Types[TUINTPTR]), makefield("args",argtype) }); 

            // build struct holding the above fields
            var s = types.New(TSTRUCT);
            s.SetNoalg(true);
            s.SetFields(fields);
            s.Width = widstruct(s, s, 0L, 1L);
            s.Align = uint8(Widthptr);
            return _addr_s!;

        }

        // f is method type, with receiver.
        // return function type, receiver as first argument (or not).
        private static ptr<types.Type> methodfunc(ptr<types.Type> _addr_f, ptr<types.Type> _addr_receiver)
        {
            ref types.Type f = ref _addr_f.val;
            ref types.Type receiver = ref _addr_receiver.val;

            var inLen = f.Params().Fields().Len();
            if (receiver != null)
            {
                inLen++;
            }

            var @in = make_slice<ptr<Node>>(0L, inLen);

            if (receiver != null)
            {
                var d = anonfield(receiver);
                in = append(in, d);
            }

            {
                var t__prev1 = t;

                foreach (var (_, __t) in f.Params().Fields().Slice())
                {
                    t = __t;
                    d = anonfield(t.Type);
                    d.SetIsDDD(t.IsDDD());
                    in = append(in, d);
                }

                t = t__prev1;
            }

            var outLen = f.Results().Fields().Len();
            var @out = make_slice<ptr<Node>>(0L, outLen);
            {
                var t__prev1 = t;

                foreach (var (_, __t) in f.Results().Fields().Slice())
                {
                    t = __t;
                    d = anonfield(t.Type);
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

            return _addr_t!;

        }

        // methods returns the methods of the non-interface type t, sorted by name.
        // Generates stub functions as needed.
        private static slice<ptr<Sig>> methods(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
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
            slice<ptr<Sig>> ms = default;
            foreach (var (_, f) in mt.AllMethods().Slice())
            {
                if (!f.IsMethod())
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
                    break;
                } 

                // get receiver type for this particular method.
                // if pointer receiver but non-pointer t and
                // this is not an embedded pointer inside a struct,
                // method does not apply.
                if (!isMethodApplicable(t, f))
                {
                    continue;
                }

                ptr<Sig> sig = addr(new Sig(name:method,isym:methodSym(it,method),tsym:methodSym(t,method),type_:methodfunc(f.Type,t),mtype:methodfunc(f.Type,nil),));
                ms = append(ms, sig);

                var @this = f.Type.Recv().Type;

                if (!sig.isym.Siggen())
                {
                    sig.isym.SetSiggen(true);
                    if (!types.Identical(this, it))
                    {
                        genwrapper(it, f, sig.isym);
                    }

                }

                if (!sig.tsym.Siggen())
                {
                    sig.tsym.SetSiggen(true);
                    if (!types.Identical(this, t))
                    {
                        genwrapper(t, f, sig.tsym);
                    }

                }

            }
            return ms;

        }

        // imethods returns the methods of the interface type t, sorted by name.
        private static slice<ptr<Sig>> imethods(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            slice<ptr<Sig>> methods = default;
            foreach (var (_, f) in t.Fields().Slice())
            {
                if (f.Type.Etype != TFUNC || f.Sym == null)
                {
                    continue;
                }

                if (f.Sym.IsBlank())
                {
                    Fatalf("unexpected blank symbol in interface method set");
                }

                {
                    var n = len(methods);

                    if (n > 0L)
                    {
                        var last = methods[n - 1L];
                        if (!last.name.Less(f.Sym))
                        {
                            Fatalf("sigcmp vs sortinter %v %v", last.name, f.Sym);
                        }

                    }

                }


                ptr<Sig> sig = addr(new Sig(name:f.Sym,mtype:f.Type,type_:methodfunc(f.Type,nil),));
                methods = append(methods, sig); 

                // NOTE(rsc): Perhaps an oversight that
                // IfaceType.Method is not in the reflect data.
                // Generate the method body, so that compiled
                // code can refer to it.
                var isym = methodSym(t, f.Sym);
                if (!isym.Siggen())
                {
                    isym.SetSiggen(true);
                    genwrapper(t, f, isym);
                }

            }
            return methods;

        }

        private static void dimportpath(ptr<types.Pkg> _addr_p)
        {
            ref types.Pkg p = ref _addr_p.val;

            if (p.Pathsym != null)
            {
                return ;
            } 

            // If we are compiling the runtime package, there are two runtime packages around
            // -- localpkg and Runtimepkg. We don't want to produce import path symbols for
            // both of them, so just produce one for localpkg.
            if (myimportpath == "runtime" && p == Runtimepkg)
            {
                return ;
            }

            var str = p.Path;
            if (p == localpkg)
            { 
                // Note: myimportpath != "", or else dgopkgpath won't call dimportpath.
                str = myimportpath;

            }

            var s = Ctxt.Lookup("type..importpath." + p.Prefix + ".");
            var ot = dnameData(_addr_s, 0L, str, "", _addr_null, false);
            ggloblsym(s, int32(ot), obj.DUPOK | obj.RODATA);
            p.Pathsym = s;

        }

        private static long dgopkgpath(ptr<obj.LSym> _addr_s, long ot, ptr<types.Pkg> _addr_pkg)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref types.Pkg pkg = ref _addr_pkg.val;

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

            dimportpath(_addr_pkg);
            return dsymptr(s, ot, pkg.Pathsym, 0L);

        }

        // dgopkgpathOff writes an offset relocation in s at offset ot to the pkg path symbol.
        private static long dgopkgpathOff(ptr<obj.LSym> _addr_s, long ot, ptr<types.Pkg> _addr_pkg)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref types.Pkg pkg = ref _addr_pkg.val;

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
                return dsymptrOff(s, ot, ns);

            }

            dimportpath(_addr_pkg);
            return dsymptrOff(s, ot, pkg.Pathsym);

        }

        // dnameField dumps a reflect.name for a struct field.
        private static long dnameField(ptr<obj.LSym> _addr_lsym, long ot, ptr<types.Pkg> _addr_spkg, ptr<types.Field> _addr_ft)
        {
            ref obj.LSym lsym = ref _addr_lsym.val;
            ref types.Pkg spkg = ref _addr_spkg.val;
            ref types.Field ft = ref _addr_ft.val;

            if (!types.IsExported(ft.Sym.Name) && ft.Sym.Pkg != spkg)
            {
                Fatalf("package mismatch for %v", ft.Sym);
            }

            var nsym = dname(ft.Sym.Name, ft.Note, _addr_null, types.IsExported(ft.Sym.Name));
            return dsymptr(lsym, ot, nsym, 0L);

        }

        // dnameData writes the contents of a reflect.name into s at offset ot.
        private static long dnameData(ptr<obj.LSym> _addr_s, long ot, @string name, @string tag, ptr<types.Pkg> _addr_pkg, bool exported)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref types.Pkg pkg = ref _addr_pkg.val;

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
                ot = dgopkgpathOff(_addr_s, ot, _addr_pkg);
            }

            return ot;

        }

        private static long dnameCount = default;

        // dname creates a reflect.name for a struct field or method.
        private static ptr<obj.LSym> dname(@string name, @string tag, ptr<types.Pkg> _addr_pkg, bool exported)
        {
            ref types.Pkg pkg = ref _addr_pkg.val;
 
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
                return _addr_s!;
            }

            var ot = dnameData(_addr_s, 0L, name, tag, _addr_pkg, exported);
            ggloblsym(s, int32(ot), obj.DUPOK | obj.RODATA);
            return _addr_s!;

        }

        // dextratype dumps the fields of a runtime.uncommontype.
        // dataAdd is the offset in bytes after the header where the
        // backing array of the []method field is written (by dextratypeData).
        private static long dextratype(ptr<obj.LSym> _addr_lsym, long ot, ptr<types.Type> _addr_t, long dataAdd)
        {
            ref obj.LSym lsym = ref _addr_lsym.val;
            ref types.Type t = ref _addr_t.val;

            var m = methods(_addr_t);
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
                dtypesym(_addr_a.type_);
            }
            ot = dgopkgpathOff(_addr_lsym, ot, _addr_typePkg(_addr_t));

            dataAdd += uncommonSize(_addr_t);
            var mcount = len(m);
            if (mcount != int(uint16(mcount)))
            {
                Fatalf("too many methods on %v: %d", t, mcount);
            }

            var xcount = sort.Search(mcount, i => !types.IsExported(m[i].name.Name));
            if (dataAdd != int(uint32(dataAdd)))
            {
                Fatalf("methods are too far away on %v: %d", t, dataAdd);
            }

            ot = duint16(lsym, ot, uint16(mcount));
            ot = duint16(lsym, ot, uint16(xcount));
            ot = duint32(lsym, ot, uint32(dataAdd));
            ot = duint32(lsym, ot, 0L);
            return ot;

        }

        private static ptr<types.Pkg> typePkg(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var tsym = t.Sym;
            if (tsym == null)
            {

                if (t.Etype == TARRAY || t.Etype == TSLICE || t.Etype == TPTR || t.Etype == TCHAN) 
                    if (t.Elem() != null)
                    {
                        tsym = t.Elem().Sym;
                    }

                            }

            if (tsym != null && t != types.Types[t.Etype] && t != types.Errortype)
            {
                return _addr_tsym.Pkg!;
            }

            return _addr_null!;

        }

        // dextratypeData dumps the backing array for the []method field of
        // runtime.uncommontype.
        private static long dextratypeData(ptr<obj.LSym> _addr_lsym, long ot, ptr<types.Type> _addr_t)
        {
            ref obj.LSym lsym = ref _addr_lsym.val;
            ref types.Type t = ref _addr_t.val;

            foreach (var (_, a) in methods(_addr_t))
            { 
                // ../../../../runtime/type.go:/method
                var exported = types.IsExported(a.name.Name);
                ptr<types.Pkg> pkg;
                if (!exported && a.name.Pkg != typePkg(_addr_t))
                {
                    pkg = a.name.Pkg;
                }

                var nsym = dname(a.name.Name, "", pkg, exported);

                ot = dsymptrOff(lsym, ot, nsym);
                ot = dmethodptrOff(_addr_lsym, ot, _addr_dtypesym(_addr_a.mtype));
                ot = dmethodptrOff(_addr_lsym, ot, _addr_a.isym.Linksym());
                ot = dmethodptrOff(_addr_lsym, ot, _addr_a.tsym.Linksym());

            }
            return ot;

        }

        private static long dmethodptrOff(ptr<obj.LSym> _addr_s, long ot, ptr<obj.LSym> _addr_x)
        {
            ref obj.LSym s = ref _addr_s.val;
            ref obj.LSym x = ref _addr_x.val;

            duint32(s, ot, 0L);
            var r = obj.Addrel(s);
            r.Off = int32(ot);
            r.Siz = 4L;
            r.Sym = x;
            r.Type = objabi.R_METHODOFF;
            return ot + 4L;
        }

        private static long kinds = new slice<long>(InitKeyedValues<long>((TINT, objabi.KindInt), (TUINT, objabi.KindUint), (TINT8, objabi.KindInt8), (TUINT8, objabi.KindUint8), (TINT16, objabi.KindInt16), (TUINT16, objabi.KindUint16), (TINT32, objabi.KindInt32), (TUINT32, objabi.KindUint32), (TINT64, objabi.KindInt64), (TUINT64, objabi.KindUint64), (TUINTPTR, objabi.KindUintptr), (TFLOAT32, objabi.KindFloat32), (TFLOAT64, objabi.KindFloat64), (TBOOL, objabi.KindBool), (TSTRING, objabi.KindString), (TPTR, objabi.KindPtr), (TSTRUCT, objabi.KindStruct), (TINTER, objabi.KindInterface), (TCHAN, objabi.KindChan), (TMAP, objabi.KindMap), (TARRAY, objabi.KindArray), (TSLICE, objabi.KindSlice), (TFUNC, objabi.KindFunc), (TCOMPLEX64, objabi.KindComplex64), (TCOMPLEX128, objabi.KindComplex128), (TUNSAFEPTR, objabi.KindUnsafePointer)));

        // typeptrdata returns the length in bytes of the prefix of t
        // containing pointer data. Anything after this offset is scalar data.
        private static long typeptrdata(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (!types.Haspointers(t))
            {
                return 0L;
            }


            if (t.Etype == TPTR || t.Etype == TUNSAFEPTR || t.Etype == TFUNC || t.Etype == TCHAN || t.Etype == TMAP) 
                return int64(Widthptr);
            else if (t.Etype == TSTRING) 
                // struct { byte *str; intgo len; }
                return int64(Widthptr);
            else if (t.Etype == TINTER) 
                // struct { Itab *tab;    void *data; } or
                // struct { Type *type; void *data; }
                // Note: see comment in plive.go:onebitwalktype1.
                return 2L * int64(Widthptr);
            else if (t.Etype == TSLICE) 
                // struct { byte *array; uintgo len; uintgo cap; }
                return int64(Widthptr);
            else if (t.Etype == TARRAY) 
                // haspointers already eliminated t.NumElem() == 0.
                return (t.NumElem() - 1L) * t.Elem().Width + typeptrdata(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                // Find the last field that has pointers.
                ptr<types.Field> lastPtrField;
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (types.Haspointers(t1.Type))
                    {
                        lastPtrField = t1;
                    }

                }
                return lastPtrField.Offset + typeptrdata(_addr_lastPtrField.Type);
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
        private static readonly long tflagUncommon = (long)1L << (int)(0L);
        private static readonly long tflagExtraStar = (long)1L << (int)(1L);
        private static readonly long tflagNamed = (long)1L << (int)(2L);
        private static readonly long tflagRegularMemory = (long)1L << (int)(3L);


        private static ptr<obj.LSym> memhashvarlen;        private static ptr<obj.LSym> memequalvarlen;

        // dcommontype dumps the contents of a reflect.rtype (runtime._type).
        private static long dcommontype(ptr<obj.LSym> _addr_lsym, ptr<types.Type> _addr_t)
        {
            ref obj.LSym lsym = ref _addr_lsym.val;
            ref types.Type t = ref _addr_t.val;

            dowidth(t);
            var eqfunc = geneq(t);

            var sptrWeak = true;
            ptr<obj.LSym> sptr;
            if (!t.IsPtr() || t.IsPtrElem())
            {
                var tptr = types.NewPtr(t);
                if (t.Sym != null || methods(_addr_tptr) != null)
                {
                    sptrWeak = false;
                }

                sptr = dtypesym(_addr_tptr);

            }

            var (gcsym, useGCProg, ptrdata) = dgcsym(_addr_t); 

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
            long ot = 0L;
            ot = duintptr(lsym, ot, uint64(t.Width));
            ot = duintptr(lsym, ot, uint64(ptrdata));
            ot = duint32(lsym, ot, typehash(t));

            byte tflag = default;
            if (uncommonSize(_addr_t) != 0L)
            {
                tflag |= tflagUncommon;
            }

            if (t.Sym != null && t.Sym.Name != "")
            {
                tflag |= tflagNamed;
            }

            if (IsRegularMemory(t))
            {
                tflag |= tflagRegularMemory;
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
                    exported = types.IsExported(t.Sym.Name);
                }

            }
            else
            {
                if (t.Elem() != null && t.Elem().Sym != null)
                {
                    exported = types.IsExported(t.Elem().Sym.Name);
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
            if (isdirectiface(t))
            {
                i |= objabi.KindDirectIface;
            }

            if (useGCProg)
            {
                i |= objabi.KindGCProg;
            }

            ot = duint8(lsym, ot, uint8(i)); // kind
            if (eqfunc != null)
            {
                ot = dsymptr(lsym, ot, eqfunc, 0L); // equality function
            }
            else
            {
                ot = duintptr(lsym, ot, 0L); // type we can't do == with
            }

            ot = dsymptr(lsym, ot, gcsym, 0L); // gcdata

            var nsym = dname(p, "", _addr_null, exported);
            ot = dsymptrOff(lsym, ot, nsym); // str
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
                ot = dsymptrOff(lsym, ot, sptr);
            }

            return ot;

        }

        // typeHasNoAlg reports whether t does not have any associated hash/eq
        // algorithms because t, or some component of t, is marked Noalg.
        private static bool typeHasNoAlg(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var (a, bad) = algtype1(t);
            return a == ANOEQ && bad.Noalg();
        }

        private static @string typesymname(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var name = t.ShortString(); 
            // Use a separate symbol name for Noalg types for #17752.
            if (typeHasNoAlg(_addr_t))
            {
                name = "noalg." + name;
            }

            return name;

        }

        // Fake package for runtime type info (headers)
        // Don't access directly, use typeLookup below.
        private static sync.Mutex typepkgmu = default;        private static var typepkg = types.NewPkg("type", "type");

        private static ptr<types.Sym> typeLookup(@string name)
        {
            typepkgmu.Lock();
            var s = typepkg.Lookup(name);
            typepkgmu.Unlock();
            return _addr_s!;
        }

        private static ptr<types.Sym> typesym(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return _addr_typeLookup(typesymname(_addr_t))!;
        }

        // tracksym returns the symbol for tracking use of field/method f, assumed
        // to be a member of struct/interface type t.
        private static ptr<types.Sym> tracksym(ptr<types.Type> _addr_t, ptr<types.Field> _addr_f)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Field f = ref _addr_f.val;

            return _addr_trackpkg.Lookup(t.ShortString() + "." + f.Sym.Name)!;
        }

        private static ptr<types.Sym> typesymprefix(@string prefix, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var p = prefix + "." + t.ShortString();
            var s = typeLookup(p); 

            // This function is for looking up type-related generated functions
            // (e.g. eq and hash). Make sure they are indeed generated.
            signatmu.Lock();
            addsignat(_addr_t);
            signatmu.Unlock(); 

            //print("algsym: %s -> %+S\n", p, s);

            return _addr_s!;

        }

        private static ptr<types.Sym> typenamesym(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null || (t.IsPtr() && t.Elem() == null) || t.IsUntyped())
            {
                Fatalf("typenamesym %v", t);
            }

            var s = typesym(_addr_t);
            signatmu.Lock();
            addsignat(_addr_t);
            signatmu.Unlock();
            return _addr_s!;

        }

        private static ptr<Node> typename(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var s = typenamesym(_addr_t);
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
            n.SetTypecheck(1L);
            return _addr_n!;

        }

        private static ptr<Node> itabname(ptr<types.Type> _addr_t, ptr<types.Type> _addr_itype)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Type itype = ref _addr_itype.val;

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
            n.SetTypecheck(1L);
            return _addr_n!;

        }

        // isreflexive reports whether t has a reflexive equality operator.
        // That is, if x==x for all x of type t.
        private static bool isreflexive(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TBOOL || t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TPTR || t.Etype == TUNSAFEPTR || t.Etype == TSTRING || t.Etype == TCHAN) 
                return true;
            else if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TINTER) 
                return false;
            else if (t.Etype == TARRAY) 
                return isreflexive(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (!isreflexive(_addr_t1.Type))
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
        private static bool needkeyupdate(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TBOOL || t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TPTR || t.Etype == TUNSAFEPTR || t.Etype == TCHAN) 
                return false;
            else if (t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TINTER || t.Etype == TSTRING) // strings might have smaller backing stores
                return true;
            else if (t.Etype == TARRAY) 
                return needkeyupdate(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (needkeyupdate(_addr_t1.Type))
                    {
                        return true;
                    }

                }
                return false;
            else 
                Fatalf("bad type for map key: %v", t);
                return true;
            
        }

        // hashMightPanic reports whether the hash of a map key of type t might panic.
        private static bool hashMightPanic(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (t.Etype == TINTER) 
                return true;
            else if (t.Etype == TARRAY) 
                return hashMightPanic(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (hashMightPanic(_addr_t1.Type))
                    {
                        return true;
                    }

                }
                return false;
            else 
                return false;
            
        }

        // formalType replaces byte and rune aliases with real types.
        // They've been separate internally to make error messages
        // better, but we have to merge them in the reflect tables.
        private static ptr<types.Type> formalType(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == types.Bytetype || t == types.Runetype)
            {
                return _addr_types.Types[t.Etype]!;
            }

            return _addr_t!;

        }

        private static ptr<obj.LSym> dtypesym(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            t = formalType(_addr_t);
            if (t.IsUntyped())
            {
                Fatalf("dtypesym %v", t);
            }

            var s = typesym(_addr_t);
            var lsym = s.Linksym();
            if (s.Siggen())
            {
                return _addr_lsym!;
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
                    return _addr_lsym!;
                } 
                // TODO(mdempsky): Investigate whether this can happen.
                if (tbase.Etype == TFORW)
                {
                    return _addr_lsym!;
                }

            }

            long ot = 0L;

            if (t.Etype == TARRAY) 
                // ../../../../runtime/type.go:/arrayType
                var s1 = dtypesym(_addr_t.Elem());
                var t2 = types.NewSlice(t.Elem());
                var s2 = dtypesym(_addr_t2);
                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dsymptr(lsym, ot, s2, 0L);
                ot = duintptr(lsym, ot, uint64(t.NumElem()));
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L);
            else if (t.Etype == TSLICE) 
                // ../../../../runtime/type.go:/sliceType
                s1 = dtypesym(_addr_t.Elem());
                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L);
            else if (t.Etype == TCHAN) 
                // ../../../../runtime/type.go:/chanType
                s1 = dtypesym(_addr_t.Elem());
                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = duintptr(lsym, ot, uint64(t.ChanDir()));
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L);
            else if (t.Etype == TFUNC) 
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Recvs().Fields().Slice())
                    {
                        t1 = __t1;
                        dtypesym(_addr_t1.Type);
                    }

                    t1 = t1__prev1;
                }

                var isddd = false;
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Params().Fields().Slice())
                    {
                        t1 = __t1;
                        isddd = t1.IsDDD();
                        dtypesym(_addr_t1.Type);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Results().Fields().Slice())
                    {
                        t1 = __t1;
                        dtypesym(_addr_t1.Type);
                    }

                    t1 = t1__prev1;
                }

                ot = dcommontype(_addr_lsym, _addr_t);
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
                ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd); 

                // Array of rtype pointers follows funcType.
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Recvs().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(_addr_t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Params().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(_addr_t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }

                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in t.Results().Fields().Slice())
                    {
                        t1 = __t1;
                        ot = dsymptr(lsym, ot, dtypesym(_addr_t1.Type), 0L);
                    }

                    t1 = t1__prev1;
                }
            else if (t.Etype == TINTER) 
                var m = imethods(_addr_t);
                var n = len(m);
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m)
                    {
                        a = __a;
                        dtypesym(_addr_a.type_);
                    } 

                    // ../../../../runtime/type.go:/interfaceType

                    a = a__prev1;
                }

                ot = dcommontype(_addr_lsym, _addr_t);

                ptr<types.Pkg> tpkg;
                if (t.Sym != null && t != types.Types[t.Etype] && t != types.Errortype)
                {
                    tpkg = t.Sym.Pkg;
                }

                ot = dgopkgpath(_addr_lsym, ot, tpkg);

                ot = dsymptr(lsym, ot, lsym, ot + 3L * Widthptr + uncommonSize(_addr_t));
                ot = duintptr(lsym, ot, uint64(n));
                ot = duintptr(lsym, ot, uint64(n));
                dataAdd = imethodSize() * n;
                ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd);

                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in m)
                    {
                        a = __a; 
                        // ../../../../runtime/type.go:/imethod
                        var exported = types.IsExported(a.name.Name);
                        ptr<types.Pkg> pkg;
                        if (!exported && a.name.Pkg != tpkg)
                        {
                            pkg = a.name.Pkg;
                        }

                        var nsym = dname(a.name.Name, "", pkg, exported);

                        ot = dsymptrOff(lsym, ot, nsym);
                        ot = dsymptrOff(lsym, ot, dtypesym(_addr_a.type_));

                    } 

                    // ../../../../runtime/type.go:/mapType

                    a = a__prev1;
                }
            else if (t.Etype == TMAP) 
                s1 = dtypesym(_addr_t.Key());
                s2 = dtypesym(_addr_t.Elem());
                var s3 = dtypesym(_addr_bmap(_addr_t));
                var hasher = genhash(t.Key());

                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dsymptr(lsym, ot, s2, 0L);
                ot = dsymptr(lsym, ot, s3, 0L);
                ot = dsymptr(lsym, ot, hasher, 0L);
                uint flags = default; 
                // Note: flags must match maptype accessors in ../../../../runtime/type.go
                // and maptype builder in ../../../../reflect/type.go:MapOf.
                if (t.Key().Width > MAXKEYSIZE)
                {
                    ot = duint8(lsym, ot, uint8(Widthptr));
                    flags |= 1L; // indirect key
                }
                else
                {
                    ot = duint8(lsym, ot, uint8(t.Key().Width));
                }

                if (t.Elem().Width > MAXELEMSIZE)
                {
                    ot = duint8(lsym, ot, uint8(Widthptr));
                    flags |= 2L; // indirect value
                }
                else
                {
                    ot = duint8(lsym, ot, uint8(t.Elem().Width));
                }

                ot = duint16(lsym, ot, uint16(bmap(_addr_t).Width));
                if (isreflexive(_addr_t.Key()))
                {
                    flags |= 4L; // reflexive key
                }

                if (needkeyupdate(_addr_t.Key()))
                {
                    flags |= 8L; // need key update
                }

                if (hashMightPanic(_addr_t.Key()))
                {
                    flags |= 16L; // hash might panic
                }

                ot = duint32(lsym, ot, flags);
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L);
            else if (t.Etype == TPTR) 
                if (t.Elem().Etype == TANY)
                { 
                    // ../../../../runtime/type.go:/UnsafePointerType
                    ot = dcommontype(_addr_lsym, _addr_t);
                    ot = dextratype(_addr_lsym, ot, _addr_t, 0L);

                    break;

                } 

                // ../../../../runtime/type.go:/ptrType
                s1 = dtypesym(_addr_t.Elem());

                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dsymptr(lsym, ot, s1, 0L);
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L); 

                // ../../../../runtime/type.go:/structType
                // for security, only the exported fields.
            else if (t.Etype == TSTRUCT) 
                var fields = t.Fields().Slice();
                {
                    var t1__prev1 = t1;

                    foreach (var (_, __t1) in fields)
                    {
                        t1 = __t1;
                        dtypesym(_addr_t1.Type);
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

                    foreach (var (_, __f) in fields)
                    {
                        f = __f;
                        if (!types.IsExported(f.Sym.Name))
                        {
                            spkg = f.Sym.Pkg;
                            break;
                        }

                    }

                    f = f__prev1;
                }

                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dgopkgpath(_addr_lsym, ot, spkg);
                ot = dsymptr(lsym, ot, lsym, ot + 3L * Widthptr + uncommonSize(_addr_t));
                ot = duintptr(lsym, ot, uint64(len(fields)));
                ot = duintptr(lsym, ot, uint64(len(fields)));

                dataAdd = len(fields) * structfieldSize();
                ot = dextratype(_addr_lsym, ot, _addr_t, dataAdd);

                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in fields)
                    {
                        f = __f; 
                        // ../../../../runtime/type.go:/structField
                        ot = dnameField(_addr_lsym, ot, spkg, _addr_f);
                        ot = dsymptr(lsym, ot, dtypesym(_addr_f.Type), 0L);
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
                ot = dcommontype(_addr_lsym, _addr_t);
                ot = dextratype(_addr_lsym, ot, _addr_t, 0L);
                        ot = dextratypeData(_addr_lsym, ot, _addr_t);
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

                if (t.Etype == TPTR || t.Etype == TARRAY || t.Etype == TCHAN || t.Etype == TFUNC || t.Etype == TMAP || t.Etype == TSLICE || t.Etype == TSTRUCT) 
                    keep = true;
                
            } 
            // Do not put Noalg types in typelinks.  See issue #22605.
            if (typeHasNoAlg(_addr_t))
            {
                keep = false;
            }

            lsym.Set(obj.AttrMakeTypelink, keep);

            return _addr_lsym!;

        }

        // for each itabEntry, gather the methods on
        // the concrete type that implement the interface
        private static void peekitabs()
        {
            foreach (var (i) in itabs)
            {
                var tab = _addr_itabs[i];
                var methods = genfun(_addr_tab.t, _addr_tab.itype);
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
        private static slice<ptr<obj.LSym>> genfun(ptr<types.Type> _addr_t, ptr<types.Type> _addr_it)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Type it = ref _addr_it.val;

            if (t == null || it == null)
            {
                return null;
            }

            var sigs = imethods(_addr_it);
            var methods = methods(_addr_t);
            var @out = make_slice<ptr<obj.LSym>>(0L, len(sigs)); 
            // TODO(mdempsky): Short circuit before calling methods(t)?
            // See discussion on CL 105039.
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
            if (len(sigs) != 0L)
            {
                Fatalf("incomplete itab");
            }

            return out;

        }

        // itabsym uses the information gathered in
        // peekitabs to de-virtualize interface methods.
        // Since this is called by the SSA backend, it shouldn't
        // generate additional Nodes, Syms, etc.
        private static ptr<obj.LSym> itabsym(ptr<obj.LSym> _addr_it, long offset)
        {
            ref obj.LSym it = ref _addr_it.val;

            slice<ptr<obj.LSym>> syms = default;
            if (it == null)
            {
                return _addr_null!;
            }

            foreach (var (i) in itabs)
            {
                var e = _addr_itabs[i];
                if (e.lsym == it)
                {
                    syms = e.entries;
                    break;
                }

            }
            if (syms == null)
            {
                return _addr_null!;
            } 

            // keep this arithmetic in sync with *itab layout
            var methodnum = int((offset - 2L * int64(Widthptr) - 8L) / int64(Widthptr));
            if (methodnum >= len(syms))
            {
                return _addr_null!;
            }

            return _addr_syms[methodnum]!;

        }

        // addsignat ensures that a runtime type descriptor is emitted for t.
        private static void addsignat(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            {
                var (_, ok) = signatset[t];

                if (!ok)
                {
                    signatset[t] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                    signatslice = append(signatslice, t);
                }

            }

        }

        private static void addsignats(slice<ptr<Node>> dcls)
        { 
            // copy types from dcl list to signatset
            foreach (var (_, n) in dcls)
            {
                if (n.Op == OTYPE)
                {
                    addsignat(_addr_n.Type);
                }

            }

        }

        private static void dumpsignats()
        { 
            // Process signatset. Use a loop, as dtypesym adds
            // entries to signatset while it is being processed.
            var signats = make_slice<typeAndStr>(len(signatslice));
            while (len(signatslice) > 0L)
            {
                signats = signats[..0L]; 
                // Transfer entries to a slice and sort, for reproducible builds.
                {
                    var t__prev2 = t;

                    foreach (var (_, __t) in signatslice)
                    {
                        t = __t;
                        signats = append(signats, new typeAndStr(t:t,short:typesymname(t),regular:t.String()));
                        delete(signatset, t);
                    }

                    t = t__prev2;
                }

                signatslice = signatslice[..0L];
                sort.Sort(typesByString(signats));
                foreach (var (_, ts) in signats)
                {
                    var t = ts.t;
                    dtypesym(_addr_t);
                    if (t.Sym != null)
                    {
                        dtypesym(_addr_types.NewPtr(t));
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
                var o = dsymptr(i.lsym, 0L, dtypesym(_addr_i.itype), 0L);
                o = dsymptr(i.lsym, o, dtypesym(_addr_i.t), 0L);
                o = duint32(i.lsym, o, typehash(i.t)); // copy of type hash
                o += 4L; // skip unused field
                foreach (var (_, fn) in genfun(_addr_i.t, _addr_i.itype))
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
                        var nsym = dname(p.s.Name, "", _addr_null, true);
                        ot = dsymptrOff(s, ot, nsym);
                        ot = dsymptrOff(s, ot, dtypesym(_addr_p.t));

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
                dimportpath(_addr_p);
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
                    dtypesym(_addr_types.NewPtr(types.Types[i]));
                }

                dtypesym(_addr_types.NewPtr(types.Types[TSTRING]));
                dtypesym(_addr_types.NewPtr(types.Types[TUNSAFEPTR])); 

                // emit type structs for error and func(error) string.
                // The latter is the type of an auto-generated wrapper.
                dtypesym(_addr_types.NewPtr(types.Errortype));

                dtypesym(_addr_functype(null, new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.Errortype) }), new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.Types[TSTRING]) }))); 

                // add paths for runtime and main, which 6l imports implicitly.
                dimportpath(_addr_Runtimepkg);

                if (flag_race)
                {
                    dimportpath(_addr_racepkg);
                }

                if (flag_msan)
                {
                    dimportpath(_addr_msanpkg);
                }

                dimportpath(_addr_types.NewPkg("main", ""));

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
            if (a[i].regular != a[j].regular)
            {
                return a[i].regular < a[j].regular;
            } 
            // Identical anonymous interfaces defined in different locations
            // will be equal for the above checks, but different in DWARF output.
            // Sort by source position to ensure deterministic order.
            // See issues 27013 and 30202.
            if (a[i].t.Etype == types.TINTER && a[i].t.Methods().Len() > 0L)
            {
                return a[i].t.Methods().Index(0L).Pos.Before(a[j].t.Methods().Index(0L).Pos);
            }

            return false;

        }
        private static void Swap(this typesByString a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];
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
        private static readonly long maxPtrmaskBytes = (long)2048L;

        // dgcsym emits and returns a data symbol containing GC information for type t,
        // along with a boolean reporting whether the UseGCProg bit should be set in
        // the type kind, and the ptrdata field to record in the reflect type information.


        // dgcsym emits and returns a data symbol containing GC information for type t,
        // along with a boolean reporting whether the UseGCProg bit should be set in
        // the type kind, and the ptrdata field to record in the reflect type information.
        private static (ptr<obj.LSym>, bool, long) dgcsym(ptr<types.Type> _addr_t)
        {
            ptr<obj.LSym> lsym = default!;
            bool useGCProg = default;
            long ptrdata = default;
            ref types.Type t = ref _addr_t.val;

            ptrdata = typeptrdata(_addr_t);
            if (ptrdata / int64(Widthptr) <= maxPtrmaskBytes * 8L)
            {
                lsym = dgcptrmask(_addr_t);
                return ;
            }

            useGCProg = true;
            lsym, ptrdata = dgcprog(_addr_t);
            return ;

        }

        // dgcptrmask emits and returns the symbol containing a pointer mask for type t.
        private static ptr<obj.LSym> dgcptrmask(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var ptrmask = make_slice<byte>((typeptrdata(_addr_t) / int64(Widthptr) + 7L) / 8L);
            fillptrmask(_addr_t, ptrmask);
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

            return _addr_lsym!;

        }

        // fillptrmask fills in ptrmask with 1s corresponding to the
        // word offsets in t that hold pointers.
        // ptrmask is assumed to fit at least typeptrdata(t)/Widthptr bits.
        private static void fillptrmask(ptr<types.Type> _addr_t, slice<byte> ptrmask)
        {
            ref types.Type t = ref _addr_t.val;

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
                return ;
            }

            var vec = bvalloc(8L * int32(len(ptrmask)));
            onebitwalktype1(t, 0L, vec);

            var nptr = typeptrdata(_addr_t) / int64(Widthptr);
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
        private static (ptr<obj.LSym>, long) dgcprog(ptr<types.Type> _addr_t)
        {
            ptr<obj.LSym> _p0 = default!;
            long _p0 = default;
            ref types.Type t = ref _addr_t.val;

            dowidth(t);
            if (t.Width == BADWIDTH)
            {
                Fatalf("dgcprog: %v badwidth", t);
            }

            var lsym = typesymprefix(".gcprog", _addr_t).Linksym();
            GCProg p = default;
            p.init(lsym);
            p.emit(t, 0L);
            var offset = p.w.BitIndex() * int64(Widthptr);
            p.end();
            {
                var ptrdata = typeptrdata(_addr_t);

                if (offset < ptrdata || offset > t.Width)
                {
                    Fatalf("dgcprog: %v: offset=%d but ptrdata=%d size=%d", t, offset, ptrdata, t.Width);
                }

            }

            return (_addr_lsym!, offset);

        }

        public partial struct GCProg
        {
            public ptr<obj.LSym> lsym;
            public long symoff;
            public gcprog.Writer w;
        }

        public static long Debug_gcprog = default; // set by -d gcprog

        private static void init(this ptr<GCProg> _addr_p, ptr<obj.LSym> _addr_lsym)
        {
            ref GCProg p = ref _addr_p.val;
            ref obj.LSym lsym = ref _addr_lsym.val;

            p.lsym = lsym;
            p.symoff = 4L; // first 4 bytes hold program length
            p.w.Init(p.writeByte);
            if (Debug_gcprog > 0L)
            {
                fmt.Fprintf(os.Stderr, "compile: start GCProg for %v\n", lsym);
                p.w.Debug(os.Stderr);
            }

        }

        private static void writeByte(this ptr<GCProg> _addr_p, byte x)
        {
            ref GCProg p = ref _addr_p.val;

            p.symoff = duint8(p.lsym, p.symoff, x);
        }

        private static void end(this ptr<GCProg> _addr_p)
        {
            ref GCProg p = ref _addr_p.val;

            p.w.End();
            duint32(p.lsym, 0L, uint32(p.symoff - 4L));
            ggloblsym(p.lsym, int32(p.symoff), obj.DUPOK | obj.RODATA | obj.LOCAL);
            if (Debug_gcprog > 0L)
            {
                fmt.Fprintf(os.Stderr, "compile: end GCProg for %v\n", p.lsym);
            }

        }

        private static void emit(this ptr<GCProg> _addr_p, ptr<types.Type> _addr_t, long offset)
        {
            ref GCProg p = ref _addr_p.val;
            ref types.Type t = ref _addr_t.val;

            dowidth(t);
            if (!types.Haspointers(t))
            {
                return ;
            }

            if (t.Width == int64(Widthptr))
            {
                p.w.Ptr(offset / int64(Widthptr));
                return ;
            }


            if (t.Etype == TSTRING) 
                p.w.Ptr(offset / int64(Widthptr));
            else if (t.Etype == TINTER) 
                // Note: the first word isn't a pointer. See comment in plive.go:onebitwalktype1.
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

                    return ;

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
        private static ptr<Node> zeroaddr(long size)
        {
            if (size >= 1L << (int)(31L))
            {
                Fatalf("map elem too big %d", size);
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
            z.SetTypecheck(1L);
            return _addr_z!;

        }
    }
}}}}
