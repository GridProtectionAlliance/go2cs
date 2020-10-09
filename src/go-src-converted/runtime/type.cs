// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Runtime type representation.

// package runtime -- go2cs converted at 2020 October 09 04:49:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\type.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // tflag is documented in reflect/type.go.
        //
        // tflag values must be kept in sync with copies in:
        //    cmd/compile/internal/gc/reflect.go
        //    cmd/link/internal/ld/decodesym.go
        //    reflect/type.go
        //      internal/reflectlite/type.go
        private partial struct tflag // : byte
        {
        }

        private static readonly tflag tflagUncommon = (tflag)1L << (int)(0L);
        private static readonly tflag tflagExtraStar = (tflag)1L << (int)(1L);
        private static readonly tflag tflagNamed = (tflag)1L << (int)(2L);
        private static readonly tflag tflagRegularMemory = (tflag)1L << (int)(3L); // equal and hash can treat values of this type as a single region of t.size bytes

        // Needs to be in sync with ../cmd/link/internal/ld/decodesym.go:/^func.commonsize,
        // ../cmd/compile/internal/gc/reflect.go:/^func.dcommontype and
        // ../reflect/type.go:/^type.rtype.
        // ../internal/reflectlite/type.go:/^type.rtype.
        private partial struct _type
        {
            public System.UIntPtr size;
            public System.UIntPtr ptrdata; // size of memory prefix holding all pointers
            public uint hash;
            public tflag tflag;
            public byte align;
            public byte fieldAlign;
            public byte kind; // function for comparing objects of this type
// (ptr to object A, ptr to object B) -> ==?
            public Func<unsafe.Pointer, unsafe.Pointer, bool> equal; // gcdata stores the GC type data for the garbage collector.
// If the KindGCProg bit is set in kind, gcdata is a GC program.
// Otherwise it is a ptrmask bitmap. See mbitmap.go for details.
            public ptr<byte> gcdata;
            public nameOff str;
            public typeOff ptrToThis;
        }

        private static @string @string(this ptr<_type> _addr_t)
        {
            ref _type t = ref _addr_t.val;

            var s = t.nameOff(t.str).name();
            if (t.tflag & tflagExtraStar != 0L)
            {
                return s[1L..];
            }

            return s;

        }

        private static ptr<uncommontype> uncommon(this ptr<_type> _addr_t)
        {
            ref _type t = ref _addr_t.val;

            if (t.tflag & tflagUncommon == 0L)
            {
                return _addr_null!;
            }


            if (t.kind & kindMask == kindStruct) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindPtr) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindFunc) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindSlice) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindArray) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindChan) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindMap) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else if (t.kind & kindMask == kindInterface) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            else 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
            
        }

        private static @string name(this ptr<_type> _addr_t)
        {
            ref _type t = ref _addr_t.val;

            if (t.tflag & tflagNamed == 0L)
            {
                return "";
            }

            var s = t.@string();
            var i = len(s) - 1L;
            while (i >= 0L && s[i] != '.')
            {
                i--;
            }

            return s[i + 1L..];

        }

        // pkgpath returns the path of the package where t was defined, if
        // available. This is not the same as the reflect package's PkgPath
        // method, in that it returns the package path for struct and interface
        // types, not just named types.
        private static @string pkgpath(this ptr<_type> _addr_t)
        {
            ref _type t = ref _addr_t.val;

            {
                var u = t.uncommon();

                if (u != null)
                {
                    return t.nameOff(u.pkgpath).name();
                }

            }


            if (t.kind & kindMask == kindStruct) 
                var st = (structtype.val)(@unsafe.Pointer(t));
                return st.pkgPath.name();
            else if (t.kind & kindMask == kindInterface) 
                var it = (interfacetype.val)(@unsafe.Pointer(t));
                return it.pkgpath.name();
                        return "";

        }

        // reflectOffs holds type offsets defined at run time by the reflect package.
        //
        // When a type is defined at run time, its *rtype data lives on the heap.
        // There are a wide range of possible addresses the heap may use, that
        // may not be representable as a 32-bit offset. Moreover the GC may
        // one day start moving heap memory, in which case there is no stable
        // offset that can be defined.
        //
        // To provide stable offsets, we add pin *rtype objects in a global map
        // and treat the offset as an identifier. We use negative offsets that
        // do not overlap with any compile-time module offsets.
        //
        // Entries are created by reflect.addReflectOff.
        private static var reflectOffs = default;

        private static void reflectOffsLock()
        {
            lock(_addr_reflectOffs.@lock);
            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(_addr_reflectOffs.@lock));
            }

        }

        private static void reflectOffsUnlock()
        {
            if (raceenabled)
            {
                racerelease(@unsafe.Pointer(_addr_reflectOffs.@lock));
            }

            unlock(_addr_reflectOffs.@lock);

        }

        private static name resolveNameOff(unsafe.Pointer ptrInModule, nameOff off)
        {
            if (off == 0L)
            {
                return new name();
            }

            var @base = uintptr(ptrInModule);
            {
                var md = _addr_firstmoduledata;

                while (md != null)
                {
                    if (base >= md.types && base < md.etypes)
                    {
                        var res = md.types + uintptr(off);
                        if (res > md.etypes)
                        {
                            println("runtime: nameOff", hex(off), "out of range", hex(md.types), "-", hex(md.etypes));
                            throw("runtime: name offset out of range");
                    md = md.next;
                        }

                        return new name((*byte)(unsafe.Pointer(res)));

                    }

                } 

                // No module found. see if it is a run time name.

            } 

            // No module found. see if it is a run time name.
            reflectOffsLock();
            var (res, found) = reflectOffs.m[int32(off)];
            reflectOffsUnlock();
            if (!found)
            {
                println("runtime: nameOff", hex(off), "base", hex(base), "not in ranges:");
                {
                    var next = _addr_firstmoduledata;

                    while (next != null)
                    {
                        println("\ttypes", hex(next.types), "etypes", hex(next.etypes));
                        next = next.next;
                    }

                }
                throw("runtime: name offset base pointer out of range");

            }

            return new name((*byte)(res));

        }

        private static name nameOff(this ptr<_type> _addr_t, nameOff off)
        {
            ref _type t = ref _addr_t.val;

            return resolveNameOff(@unsafe.Pointer(t), off);
        }

        private static ptr<_type> resolveTypeOff(unsafe.Pointer ptrInModule, typeOff off)
        {
            if (off == 0L)
            {
                return _addr_null!;
            }

            var @base = uintptr(ptrInModule);
            ptr<moduledata> md;
            {
                var next__prev1 = next;

                var next = _addr_firstmoduledata;

                while (next != null)
                {
                    if (base >= next.types && base < next.etypes)
                    {
                        md = next;
                        break;
                    next = next.next;
                    }

                }


                next = next__prev1;
            }
            if (md == null)
            {
                reflectOffsLock();
                var res = reflectOffs.m[int32(off)];
                reflectOffsUnlock();
                if (res == null)
                {
                    println("runtime: typeOff", hex(off), "base", hex(base), "not in ranges:");
                    {
                        var next__prev1 = next;

                        next = _addr_firstmoduledata;

                        while (next != null)
                        {
                            println("\ttypes", hex(next.types), "etypes", hex(next.etypes));
                            next = next.next;
                        }


                        next = next__prev1;
                    }
                    throw("runtime: type offset base pointer out of range");

                }

                return _addr_(_type.val)(res)!;

            }

            {
                var t = md.typemap[off];

                if (t != null)
                {
                    return _addr_t!;
                }

            }

            res = md.types + uintptr(off);
            if (res > md.etypes)
            {
                println("runtime: typeOff", hex(off), "out of range", hex(md.types), "-", hex(md.etypes));
                throw("runtime: type offset out of range");
            }

            return _addr_(_type.val)(@unsafe.Pointer(res))!;

        }

        private static ptr<_type> typeOff(this ptr<_type> _addr_t, typeOff off)
        {
            ref _type t = ref _addr_t.val;

            return _addr_resolveTypeOff(@unsafe.Pointer(t), off)!;
        }

        private static unsafe.Pointer textOff(this ptr<_type> _addr_t, textOff off)
        {
            ref _type t = ref _addr_t.val;

            var @base = uintptr(@unsafe.Pointer(t));
            ptr<moduledata> md;
            {
                var next__prev1 = next;

                var next = _addr_firstmoduledata;

                while (next != null)
                {
                    if (base >= next.types && base < next.etypes)
                    {
                        md = next;
                        break;
                    next = next.next;
                    }

                }


                next = next__prev1;
            }
            if (md == null)
            {
                reflectOffsLock();
                var res = reflectOffs.m[int32(off)];
                reflectOffsUnlock();
                if (res == null)
                {
                    println("runtime: textOff", hex(off), "base", hex(base), "not in ranges:");
                    {
                        var next__prev1 = next;

                        next = _addr_firstmoduledata;

                        while (next != null)
                        {
                            println("\ttypes", hex(next.types), "etypes", hex(next.etypes));
                            next = next.next;
                        }


                        next = next__prev1;
                    }
                    throw("runtime: text offset base pointer out of range");

                }

                return res;

            }

            res = uintptr(0L); 

            // The text, or instruction stream is generated as one large buffer.  The off (offset) for a method is
            // its offset within this buffer.  If the total text size gets too large, there can be issues on platforms like ppc64 if
            // the target of calls are too far for the call instruction.  To resolve the large text issue, the text is split
            // into multiple text sections to allow the linker to generate long calls when necessary.  When this happens, the vaddr
            // for each text section is set to its offset within the text.  Each method's offset is compared against the section
            // vaddrs and sizes to determine the containing section.  Then the section relative offset is added to the section's
            // relocated baseaddr to compute the method addess.

            if (len(md.textsectmap) > 1L)
            {
                foreach (var (i) in md.textsectmap)
                {
                    var sectaddr = md.textsectmap[i].vaddr;
                    var sectlen = md.textsectmap[i].length;
                    if (uintptr(off) >= sectaddr && uintptr(off) < sectaddr + sectlen)
                    {
                        res = md.textsectmap[i].baseaddr + uintptr(off) - uintptr(md.textsectmap[i].vaddr);
                        break;
                    }

                }
            else
            }            { 
                // single text section
                res = md.text + uintptr(off);

            }

            if (res > md.etext && GOARCH != "wasm")
            { // on wasm, functions do not live in the same address space as the linear memory
                println("runtime: textOff", hex(off), "out of range", hex(md.text), "-", hex(md.etext));
                throw("runtime: text offset out of range");

            }

            return @unsafe.Pointer(res);

        }

        private static slice<ptr<_type>> @in(this ptr<functype> _addr_t)
        {
            ref functype t = ref _addr_t.val;
 
            // See funcType in reflect/type.go for details on data layout.
            var uadd = uintptr(@unsafe.Sizeof(new functype()));
            if (t.typ.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommontype());
            }

            return new ptr<ptr<array<ptr<_type>>>>(add(@unsafe.Pointer(t), uadd))[..t.inCount];

        }

        private static slice<ptr<_type>> @out(this ptr<functype> _addr_t)
        {
            ref functype t = ref _addr_t.val;
 
            // See funcType in reflect/type.go for details on data layout.
            var uadd = uintptr(@unsafe.Sizeof(new functype()));
            if (t.typ.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommontype());
            }

            var outCount = t.outCount & (1L << (int)(15L) - 1L);
            return new ptr<ptr<array<ptr<_type>>>>(add(@unsafe.Pointer(t), uadd))[t.inCount..t.inCount + outCount];

        }

        private static bool dotdotdot(this ptr<functype> _addr_t)
        {
            ref functype t = ref _addr_t.val;

            return t.outCount & (1L << (int)(15L)) != 0L;
        }

        private partial struct nameOff // : int
        {
        }
        private partial struct typeOff // : int
        {
        }
        private partial struct textOff // : int
        {
        }

        private partial struct method
        {
            public nameOff name;
            public typeOff mtyp;
            public textOff ifn;
            public textOff tfn;
        }

        private partial struct uncommontype
        {
            public nameOff pkgpath;
            public ushort mcount; // number of methods
            public ushort xcount; // number of exported methods
            public uint moff; // offset from this uncommontype to [mcount]method
            public uint _; // unused
        }

        private partial struct imethod
        {
            public nameOff name;
            public typeOff ityp;
        }

        private partial struct interfacetype
        {
            public _type typ;
            public name pkgpath;
            public slice<imethod> mhdr;
        }

        private partial struct maptype
        {
            public _type typ;
            public ptr<_type> key;
            public ptr<_type> elem;
            public ptr<_type> bucket; // internal type representing a hash bucket
// function for hashing keys (ptr to key, seed) -> hash
            public Func<unsafe.Pointer, System.UIntPtr, System.UIntPtr> hasher;
            public byte keysize; // size of key slot
            public byte elemsize; // size of elem slot
            public ushort bucketsize; // size of bucket
            public uint flags;
        }

        // Note: flag values must match those used in the TMAP case
        // in ../cmd/compile/internal/gc/reflect.go:dtypesym.
        private static bool indirectkey(this ptr<maptype> _addr_mt)
        {
            ref maptype mt = ref _addr_mt.val;
 // store ptr to key instead of key itself
            return mt.flags & 1L != 0L;

        }
        private static bool indirectelem(this ptr<maptype> _addr_mt)
        {
            ref maptype mt = ref _addr_mt.val;
 // store ptr to elem instead of elem itself
            return mt.flags & 2L != 0L;

        }
        private static bool reflexivekey(this ptr<maptype> _addr_mt)
        {
            ref maptype mt = ref _addr_mt.val;
 // true if k==k for all keys
            return mt.flags & 4L != 0L;

        }
        private static bool needkeyupdate(this ptr<maptype> _addr_mt)
        {
            ref maptype mt = ref _addr_mt.val;
 // true if we need to update key on an overwrite
            return mt.flags & 8L != 0L;

        }
        private static bool hashMightPanic(this ptr<maptype> _addr_mt)
        {
            ref maptype mt = ref _addr_mt.val;
 // true if hash function might panic
            return mt.flags & 16L != 0L;

        }

        private partial struct arraytype
        {
            public _type typ;
            public ptr<_type> elem;
            public ptr<_type> slice;
            public System.UIntPtr len;
        }

        private partial struct chantype
        {
            public _type typ;
            public ptr<_type> elem;
            public System.UIntPtr dir;
        }

        private partial struct slicetype
        {
            public _type typ;
            public ptr<_type> elem;
        }

        private partial struct functype
        {
            public _type typ;
            public ushort inCount;
            public ushort outCount;
        }

        private partial struct ptrtype
        {
            public _type typ;
            public ptr<_type> elem;
        }

        private partial struct structfield
        {
            public name name;
            public ptr<_type> typ;
            public System.UIntPtr offsetAnon;
        }

        private static System.UIntPtr offset(this ptr<structfield> _addr_f)
        {
            ref structfield f = ref _addr_f.val;

            return f.offsetAnon >> (int)(1L);
        }

        private partial struct structtype
        {
            public _type typ;
            public name pkgPath;
            public slice<structfield> fields;
        }

        // name is an encoded type name with optional extra data.
        // See reflect/type.go for details.
        private partial struct name
        {
            public ptr<byte> bytes;
        }

        private static ptr<byte> data(this name n, long off)
        {
            return _addr_(byte.val)(add(@unsafe.Pointer(n.bytes), uintptr(off)))!;
        }

        private static bool isExported(this name n)
        {
            return (n.bytes.val) & (1L << (int)(0L)) != 0L;
        }

        private static long nameLen(this name n)
        {
            return int(uint16(new ptr<ptr<n.data>>(1L)) << (int)(8L) | uint16(new ptr<ptr<n.data>>(2L)));
        }

        private static long tagLen(this name n)
        {
            if (new ptr<ptr<n.data>>(0L) & (1L << (int)(1L)) == 0L)
            {
                return 0L;
            }

            long off = 3L + n.nameLen();
            return int(uint16(new ptr<ptr<n.data>>(off)) << (int)(8L) | uint16(new ptr<ptr<n.data>>(off + 1L)));

        }

        private static @string name(this name n)
        {
            @string s = default;

            if (n.bytes == null)
            {
                return "";
            }

            var nl = n.nameLen();
            if (nl == 0L)
            {
                return "";
            }

            var hdr = (stringStruct.val)(@unsafe.Pointer(_addr_s));
            hdr.str = @unsafe.Pointer(n.data(3L));
            hdr.len = nl;
            return s;

        }

        private static @string tag(this name n)
        {
            @string s = default;

            var tl = n.tagLen();
            if (tl == 0L)
            {
                return "";
            }

            var nl = n.nameLen();
            var hdr = (stringStruct.val)(@unsafe.Pointer(_addr_s));
            hdr.str = @unsafe.Pointer(n.data(3L + nl + 2L));
            hdr.len = tl;
            return s;

        }

        private static @string pkgPath(this name n)
        {
            if (n.bytes == null || new ptr<ptr<n.data>>(0L) & (1L << (int)(2L)) == 0L)
            {
                return "";
            }

            long off = 3L + n.nameLen();
            {
                var tl = n.tagLen();

                if (tl > 0L)
                {
                    off += 2L + tl;
                }

            }

            ref nameOff nameOff = ref heap(out ptr<nameOff> _addr_nameOff);
            copy(new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_nameOff))[..], new ptr<ptr<array<byte>>>(@unsafe.Pointer(n.data(off)))[..]);
            var pkgPathName = resolveNameOff(@unsafe.Pointer(n.bytes), nameOff);
            return pkgPathName.name();

        }

        private static bool isBlank(this name n)
        {
            if (n.bytes == null)
            {
                return false;
            }

            if (n.nameLen() != 1L)
            {
                return false;
            }

            return new ptr<ptr<n.data>>(3L) == '_';

        }

        // typelinksinit scans the types from extra modules and builds the
        // moduledata typemap used to de-duplicate type pointers.
        private static void typelinksinit()
        {
            if (firstmoduledata.next == null)
            {
                return ;
            }

            var typehash = make_map<uint, slice<ptr<_type>>>(len(firstmoduledata.typelinks));

            var modules = activeModules();
            var prev = modules[0L];
            foreach (var (_, md) in modules[1L..])
            { 
                // Collect types from the previous module into typehash.
collect:

                {
                    var tl__prev2 = tl;

                    foreach (var (_, __tl) in prev.typelinks)
                    {
                        tl = __tl;
                        ptr<_type> t;
                        if (prev.typemap == null)
                        {
                            t = (_type.val)(@unsafe.Pointer(prev.types + uintptr(tl)));
                        }
                        else
                        {
                            t = prev.typemap[typeOff(tl)];
                        } 
                        // Add to typehash if not seen before.
                        var tlist = typehash[t.hash];
                        foreach (var (_, tcur) in tlist)
                        {
                            if (tcur == t)
                            {
                                _continuecollect = true;
                                break;
                            }

                        }
                        typehash[t.hash] = append(tlist, t);

                    }

                    tl = tl__prev2;
                }
                if (md.typemap == null)
                { 
                    // If any of this module's typelinks match a type from a
                    // prior module, prefer that prior type by adding the offset
                    // to this module's typemap.
                    var tm = make_map<typeOff, ptr<_type>>(len(md.typelinks));
                    pinnedTypemaps = append(pinnedTypemaps, tm);
                    md.typemap = tm;
                    {
                        var tl__prev2 = tl;

                        foreach (var (_, __tl) in md.typelinks)
                        {
                            tl = __tl;
                            t = (_type.val)(@unsafe.Pointer(md.types + uintptr(tl)));
                            foreach (var (_, candidate) in typehash[t.hash])
                            {
                                if (typesEqual(t, _addr_candidate, seen))
                                {
                                    t = candidate;
                                    break;
                                }

                            }
                            md.typemap[typeOff(tl)] = t;

                        }

                        tl = tl__prev2;
                    }
                }

                prev = md;

            }

        }

        private partial struct _typePair
        {
            public ptr<_type> t1;
            public ptr<_type> t2;
        }

        // typesEqual reports whether two types are equal.
        //
        // Everywhere in the runtime and reflect packages, it is assumed that
        // there is exactly one *_type per Go type, so that pointer equality
        // can be used to test if types are equal. There is one place that
        // breaks this assumption: buildmode=shared. In this case a type can
        // appear as two different pieces of memory. This is hidden from the
        // runtime and reflect package by the per-module typemap built in
        // typelinksinit. It uses typesEqual to map types from later modules
        // back into earlier ones.
        //
        // Only typelinksinit needs this function.
        private static bool typesEqual(ptr<_type> _addr_t, ptr<_type> _addr_v, object seen)
        {
            ref _type t = ref _addr_t.val;
            ref _type v = ref _addr_v.val;

            _typePair tp = new _typePair(t,v);
            {
                var (_, ok) = seen[tp];

                if (ok)
                {
                    return true;
                } 

                // mark these types as seen, and thus equivalent which prevents an infinite loop if
                // the two types are identical, but recursively defined and loaded from
                // different modules

            } 

            // mark these types as seen, and thus equivalent which prevents an infinite loop if
            // the two types are identical, but recursively defined and loaded from
            // different modules
            seen[tp] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

            if (t == v)
            {
                return true;
            }

            var kind = t.kind & kindMask;
            if (kind != v.kind & kindMask)
            {
                return false;
            }

            if (t.@string() != v.@string())
            {
                return false;
            }

            var ut = t.uncommon();
            var uv = v.uncommon();
            if (ut != null || uv != null)
            {
                if (ut == null || uv == null)
                {
                    return false;
                }

                var pkgpatht = t.nameOff(ut.pkgpath).name();
                var pkgpathv = v.nameOff(uv.pkgpath).name();
                if (pkgpatht != pkgpathv)
                {
                    return false;
                }

            }

            if (kindBool <= kind && kind <= kindComplex128)
            {
                return true;
            }


            if (kind == kindString || kind == kindUnsafePointer) 
                return true;
            else if (kind == kindArray) 
                var at = (arraytype.val)(@unsafe.Pointer(t));
                var av = (arraytype.val)(@unsafe.Pointer(v));
                return typesEqual(_addr_at.elem, _addr_av.elem, seen) && at.len == av.len;
            else if (kind == kindChan) 
                var ct = (chantype.val)(@unsafe.Pointer(t));
                var cv = (chantype.val)(@unsafe.Pointer(v));
                return ct.dir == cv.dir && typesEqual(_addr_ct.elem, _addr_cv.elem, seen);
            else if (kind == kindFunc) 
                var ft = (functype.val)(@unsafe.Pointer(t));
                var fv = (functype.val)(@unsafe.Pointer(v));
                if (ft.outCount != fv.outCount || ft.inCount != fv.inCount)
                {
                    return false;
                }

                var tin = ft.@in();
                var vin = fv.@in();
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(tin); i++)
                    {
                        if (!typesEqual(_addr_tin[i], _addr_vin[i], seen))
                        {
                            return false;
                        }

                    }


                    i = i__prev1;
                }
                var tout = ft.@out();
                var vout = fv.@out();
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(tout); i++)
                    {
                        if (!typesEqual(_addr_tout[i], _addr_vout[i], seen))
                        {
                            return false;
                        }

                    }


                    i = i__prev1;
                }
                return true;
            else if (kind == kindInterface) 
                var it = (interfacetype.val)(@unsafe.Pointer(t));
                var iv = (interfacetype.val)(@unsafe.Pointer(v));
                if (it.pkgpath.name() != iv.pkgpath.name())
                {
                    return false;
                }

                if (len(it.mhdr) != len(iv.mhdr))
                {
                    return false;
                }

                {
                    long i__prev1 = i;

                    foreach (var (__i) in it.mhdr)
                    {
                        i = __i;
                        var tm = _addr_it.mhdr[i];
                        var vm = _addr_iv.mhdr[i]; 
                        // Note the mhdr array can be relocated from
                        // another module. See #17724.
                        var tname = resolveNameOff(@unsafe.Pointer(tm), tm.name);
                        var vname = resolveNameOff(@unsafe.Pointer(vm), vm.name);
                        if (tname.name() != vname.name())
                        {
                            return false;
                        }

                        if (tname.pkgPath() != vname.pkgPath())
                        {
                            return false;
                        }

                        var tityp = resolveTypeOff(@unsafe.Pointer(tm), tm.ityp);
                        var vityp = resolveTypeOff(@unsafe.Pointer(vm), vm.ityp);
                        if (!typesEqual(_addr_tityp, _addr_vityp, seen))
                        {
                            return false;
                        }

                    }

                    i = i__prev1;
                }

                return true;
            else if (kind == kindMap) 
                var mt = (maptype.val)(@unsafe.Pointer(t));
                var mv = (maptype.val)(@unsafe.Pointer(v));
                return typesEqual(_addr_mt.key, _addr_mv.key, seen) && typesEqual(_addr_mt.elem, _addr_mv.elem, seen);
            else if (kind == kindPtr) 
                var pt = (ptrtype.val)(@unsafe.Pointer(t));
                var pv = (ptrtype.val)(@unsafe.Pointer(v));
                return typesEqual(_addr_pt.elem, _addr_pv.elem, seen);
            else if (kind == kindSlice) 
                var st = (slicetype.val)(@unsafe.Pointer(t));
                var sv = (slicetype.val)(@unsafe.Pointer(v));
                return typesEqual(_addr_st.elem, _addr_sv.elem, seen);
            else if (kind == kindStruct) 
                st = (structtype.val)(@unsafe.Pointer(t));
                sv = (structtype.val)(@unsafe.Pointer(v));
                if (len(st.fields) != len(sv.fields))
                {
                    return false;
                }

                if (st.pkgPath.name() != sv.pkgPath.name())
                {
                    return false;
                }

                {
                    long i__prev1 = i;

                    foreach (var (__i) in st.fields)
                    {
                        i = __i;
                        var tf = _addr_st.fields[i];
                        var vf = _addr_sv.fields[i];
                        if (tf.name.name() != vf.name.name())
                        {
                            return false;
                        }

                        if (!typesEqual(_addr_tf.typ, _addr_vf.typ, seen))
                        {
                            return false;
                        }

                        if (tf.name.tag() != vf.name.tag())
                        {
                            return false;
                        }

                        if (tf.offsetAnon != vf.offsetAnon)
                        {
                            return false;
                        }

                    }

                    i = i__prev1;
                }

                return true;
            else 
                println("runtime: impossible type kind", kind);
                throw("runtime: impossible type kind");
                return false;
            
        }
    }
}
