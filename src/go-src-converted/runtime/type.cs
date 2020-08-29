// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Runtime type representation.

// package runtime -- go2cs converted at 2020 August 29 08:21:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\type.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

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
        private partial struct tflag // : byte
        {
        }

        private static readonly tflag tflagUncommon = 1L << (int)(0L);
        private static readonly tflag tflagExtraStar = 1L << (int)(1L);
        private static readonly tflag tflagNamed = 1L << (int)(2L);

        // Needs to be in sync with ../cmd/link/internal/ld/decodesym.go:/^func.commonsize,
        // ../cmd/compile/internal/gc/reflect.go:/^func.dcommontype and
        // ../reflect/type.go:/^type.rtype.
        private partial struct _type
        {
            public System.UIntPtr size;
            public System.UIntPtr ptrdata; // size of memory prefix holding all pointers
            public uint hash;
            public tflag tflag;
            public byte align;
            public byte fieldalign;
            public byte kind;
            public ptr<typeAlg> alg; // gcdata stores the GC type data for the garbage collector.
// If the KindGCProg bit is set in kind, gcdata is a GC program.
// Otherwise it is a ptrmask bitmap. See mbitmap.go for details.
            public ptr<byte> gcdata;
            public nameOff str;
            public typeOff ptrToThis;
        }

        private static @string @string(this ref _type t)
        {
            var s = t.nameOff(t.str).name();
            if (t.tflag & tflagExtraStar != 0L)
            {
                return s[1L..];
            }
            return s;
        }

        private static ref uncommontype uncommon(this ref _type t)
        {
            if (t.tflag & tflagUncommon == 0L)
            {
                return null;
            }

            if (t.kind & kindMask == kindStruct) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindPtr) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindFunc) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindSlice) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindArray) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindChan) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindMap) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.kind & kindMask == kindInterface) 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else 
                private partial struct u
                {
                    public ref structtype structtype => ref structtype_val;
                    public uncommontype u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
                    }

        private static bool hasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[..len(prefix)] == prefix;
        }

        private static @string name(this ref _type t)
        {
            if (t.tflag & tflagNamed == 0L)
            {
                return "";
            }
            var s = t.@string();
            var i = len(s) - 1L;
            while (i >= 0L)
            {
                if (s[i] == '.')
                {
                    break;
                }
                i--;
            }

            return s[i + 1L..];
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
            lock(ref reflectOffs.@lock);
            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(ref reflectOffs.@lock));
            }
        }

        private static void reflectOffsUnlock()
        {
            if (raceenabled)
            {
                racerelease(@unsafe.Pointer(ref reflectOffs.@lock));
            }
            unlock(ref reflectOffs.@lock);
        }

        private static name resolveNameOff(unsafe.Pointer ptrInModule, nameOff off)
        {
            if (off == 0L)
            {
                return new name();
            }
            var @base = uintptr(ptrInModule);
            {
                var md = ref firstmoduledata;

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
                    var next = ref firstmoduledata;

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

        private static name nameOff(this ref _type t, nameOff off)
        {
            return resolveNameOff(@unsafe.Pointer(t), off);
        }

        private static ref _type resolveTypeOff(unsafe.Pointer ptrInModule, typeOff off)
        {
            if (off == 0L)
            {
                return null;
            }
            var @base = uintptr(ptrInModule);
            ref moduledata md = default;
            {
                var next__prev1 = next;

                var next = ref firstmoduledata;

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

                        next = ref firstmoduledata;

                        while (next != null)
                        {
                            println("\ttypes", hex(next.types), "etypes", hex(next.etypes));
                            next = next.next;
                        }


                        next = next__prev1;
                    }
                    throw("runtime: type offset base pointer out of range");
                }
                return (_type.Value)(res);
            }
            {
                var t = md.typemap[off];

                if (t != null)
                {
                    return t;
                }

            }
            res = md.types + uintptr(off);
            if (res > md.etypes)
            {
                println("runtime: typeOff", hex(off), "out of range", hex(md.types), "-", hex(md.etypes));
                throw("runtime: type offset out of range");
            }
            return (_type.Value)(@unsafe.Pointer(res));
        }

        private static ref _type typeOff(this ref _type t, typeOff off)
        {
            return resolveTypeOff(@unsafe.Pointer(t), off);
        }

        private static unsafe.Pointer textOff(this ref _type t, textOff off)
        {
            var @base = uintptr(@unsafe.Pointer(t));
            ref moduledata md = default;
            {
                var next__prev1 = next;

                var next = ref firstmoduledata;

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

                        next = ref firstmoduledata;

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
                    if (uintptr(off) >= sectaddr && uintptr(off) <= sectaddr + sectlen)
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
            if (res > md.etext)
            {
                println("runtime: textOff", hex(off), "out of range", hex(md.text), "-", hex(md.etext));
                throw("runtime: text offset out of range");
            }
            return @unsafe.Pointer(res);
        }

        private static slice<ref _type> @in(this ref functype t)
        { 
            // See funcType in reflect/type.go for details on data layout.
            var uadd = uintptr(@unsafe.Sizeof(new functype()));
            if (t.typ.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommontype());
            }
            return new ptr<ref array<ref _type>>(add(@unsafe.Pointer(t), uadd))[..t.inCount];
        }

        private static slice<ref _type> @out(this ref functype t)
        { 
            // See funcType in reflect/type.go for details on data layout.
            var uadd = uintptr(@unsafe.Sizeof(new functype()));
            if (t.typ.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommontype());
            }
            var outCount = t.outCount & (1L << (int)(15L) - 1L);
            return new ptr<ref array<ref _type>>(add(@unsafe.Pointer(t), uadd))[t.inCount..t.inCount + outCount];
        }

        private static bool dotdotdot(this ref functype t)
        {
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
            public ushort _; // unused
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
            public ptr<_type> hmap; // internal type representing a hmap
            public byte keysize; // size of key slot
            public bool indirectkey; // store ptr to key instead of key itself
            public byte valuesize; // size of value slot
            public bool indirectvalue; // store ptr to value instead of value itself
            public ushort bucketsize; // size of bucket
            public bool reflexivekey; // true if k==k for all keys
            public bool needkeyupdate; // true if we need to update key on an overwrite
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

        private static System.UIntPtr offset(this ref structfield f)
        {
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

        private static ref byte data(this name n, long off)
        {
            return (byte.Value)(add(@unsafe.Pointer(n.bytes), uintptr(off)));
        }

        private static bool isExported(this name n)
        {
            return (n.bytes.Value) & (1L << (int)(0L)) != 0L;
        }

        private static long nameLen(this name n)
        {
            return int(uint16(new ptr<ref n.data>(1L)) << (int)(8L) | uint16(new ptr<ref n.data>(2L)));
        }

        private static long tagLen(this name n)
        {
            if (new ptr<ref n.data>(0L) & (1L << (int)(1L)) == 0L)
            {
                return 0L;
            }
            long off = 3L + n.nameLen();
            return int(uint16(new ptr<ref n.data>(off)) << (int)(8L) | uint16(new ptr<ref n.data>(off + 1L)));
        }

        private static @string name(this name n)
        {
            if (n.bytes == null)
            {
                return "";
            }
            var nl = n.nameLen();
            if (nl == 0L)
            {
                return "";
            }
            var hdr = (stringStruct.Value)(@unsafe.Pointer(ref s));
            hdr.str = @unsafe.Pointer(n.data(3L));
            hdr.len = nl;
            return s;
        }

        private static @string tag(this name n)
        {
            var tl = n.tagLen();
            if (tl == 0L)
            {
                return "";
            }
            var nl = n.nameLen();
            var hdr = (stringStruct.Value)(@unsafe.Pointer(ref s));
            hdr.str = @unsafe.Pointer(n.data(3L + nl + 2L));
            hdr.len = tl;
            return s;
        }

        private static @string pkgPath(this name n)
        {
            if (n.bytes == null || new ptr<ref n.data>(0L) & (1L << (int)(2L)) == 0L)
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
            nameOff nameOff = default;
            copy(new ptr<ref array<byte>>(@unsafe.Pointer(ref nameOff))[..], new ptr<ref array<byte>>(@unsafe.Pointer(n.data(off)))[..]);
            var pkgPathName = resolveNameOff(@unsafe.Pointer(n.bytes), nameOff);
            return pkgPathName.name();
        }

        // typelinksinit scans the types from extra modules and builds the
        // moduledata typemap used to de-duplicate type pointers.
        private static void typelinksinit()
        {
            if (firstmoduledata.next == null)
            {
                return;
            }
            var typehash = make_map<uint, slice<ref _type>>(len(firstmoduledata.typelinks));

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
                        ref _type t = default;
                        if (prev.typemap == null)
                        {
                            t = (_type.Value)(@unsafe.Pointer(prev.types + uintptr(tl)));
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
                    var tm = make_map<typeOff, ref _type>(len(md.typelinks));
                    pinnedTypemaps = append(pinnedTypemaps, tm);
                    md.typemap = tm;
                    {
                        var tl__prev2 = tl;

                        foreach (var (_, __tl) in md.typelinks)
                        {
                            tl = __tl;
                            t = (_type.Value)(@unsafe.Pointer(md.types + uintptr(tl)));
                            foreach (var (_, candidate) in typehash[t.hash])
                            {
                                if (typesEqual(t, candidate, seen))
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
        private static bool typesEqual(ref _type t, ref _type v, object seen)
        {
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
                var at = (arraytype.Value)(@unsafe.Pointer(t));
                var av = (arraytype.Value)(@unsafe.Pointer(v));
                return typesEqual(at.elem, av.elem, seen) && at.len == av.len;
            else if (kind == kindChan) 
                var ct = (chantype.Value)(@unsafe.Pointer(t));
                var cv = (chantype.Value)(@unsafe.Pointer(v));
                return ct.dir == cv.dir && typesEqual(ct.elem, cv.elem, seen);
            else if (kind == kindFunc) 
                var ft = (functype.Value)(@unsafe.Pointer(t));
                var fv = (functype.Value)(@unsafe.Pointer(v));
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
                        if (!typesEqual(tin[i], vin[i], seen))
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
                        if (!typesEqual(tout[i], vout[i], seen))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (kind == kindInterface) 
                var it = (interfacetype.Value)(@unsafe.Pointer(t));
                var iv = (interfacetype.Value)(@unsafe.Pointer(v));
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
                        var tm = ref it.mhdr[i];
                        var vm = ref iv.mhdr[i]; 
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
                        if (!typesEqual(tityp, vityp, seen))
                        {
                            return false;
                        }
                    }

                    i = i__prev1;
                }

                return true;
            else if (kind == kindMap) 
                var mt = (maptype.Value)(@unsafe.Pointer(t));
                var mv = (maptype.Value)(@unsafe.Pointer(v));
                return typesEqual(mt.key, mv.key, seen) && typesEqual(mt.elem, mv.elem, seen);
            else if (kind == kindPtr) 
                var pt = (ptrtype.Value)(@unsafe.Pointer(t));
                var pv = (ptrtype.Value)(@unsafe.Pointer(v));
                return typesEqual(pt.elem, pv.elem, seen);
            else if (kind == kindSlice) 
                var st = (slicetype.Value)(@unsafe.Pointer(t));
                var sv = (slicetype.Value)(@unsafe.Pointer(v));
                return typesEqual(st.elem, sv.elem, seen);
            else if (kind == kindStruct) 
                st = (structtype.Value)(@unsafe.Pointer(t));
                sv = (structtype.Value)(@unsafe.Pointer(v));
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
                        var tf = ref st.fields[i];
                        var vf = ref sv.fields[i];
                        if (tf.name.name() != vf.name.name())
                        {
                            return false;
                        }
                        if (!typesEqual(tf.typ, vf.typ, seen))
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
