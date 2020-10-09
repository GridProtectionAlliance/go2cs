// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gob -- go2cs converted at 2020 October 09 05:00:00 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\type.go
using encoding = go.encoding_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        // userTypeInfo stores the information associated with a type the user has handed
        // to the package. It's computed once and stored in a map keyed by reflection
        // type.
        private partial struct userTypeInfo
        {
            public reflect.Type user; // the type the user handed us
            public reflect.Type @base; // the base type after all indirections
            public long indir; // number of indirections to reach the base type
            public long externalEnc; // xGob, xBinary, or xText
            public long externalDec; // xGob, xBinary or xText
            public sbyte encIndir; // number of indirections to reach the receiver type; may be negative
            public sbyte decIndir; // number of indirections to reach the receiver type; may be negative
        }

        // externalEncoding bits
        private static readonly long xGob = (long)1L + iota; // GobEncoder or GobDecoder
        private static readonly var xBinary = 0; // encoding.BinaryMarshaler or encoding.BinaryUnmarshaler
        private static readonly var xText = 1; // encoding.TextMarshaler or encoding.TextUnmarshaler

        private static sync.Map userTypeCache = default; // map[reflect.Type]*userTypeInfo

        // validType returns, and saves, the information associated with user-provided type rt.
        // If the user type is not valid, err will be non-nil. To be used when the error handler
        // is not set up.
        private static (ptr<userTypeInfo>, error) validUserType(reflect.Type rt)
        {
            ptr<userTypeInfo> _p0 = default!;
            error _p0 = default!;

            {
                var ui__prev1 = ui;
                var ok__prev1 = ok;

                var (ui, ok) = userTypeCache.Load(rt);

                if (ok)
                {
                    return (ui._<ptr<userTypeInfo>>(), error.As(null!)!);
                } 

                // Construct a new userTypeInfo and atomically add it to the userTypeCache.
                // If we lose the race, we'll waste a little CPU and create a little garbage
                // but return the existing value anyway.

                ui = ui__prev1;
                ok = ok__prev1;

            } 

            // Construct a new userTypeInfo and atomically add it to the userTypeCache.
            // If we lose the race, we'll waste a little CPU and create a little garbage
            // but return the existing value anyway.

            ptr<userTypeInfo> ut = @new<userTypeInfo>();
            ut.@base = rt;
            ut.user = rt; 
            // A type that is just a cycle of pointers (such as type T *T) cannot
            // be represented in gobs, which need some concrete data. We use a
            // cycle detection algorithm from Knuth, Vol 2, Section 3.1, Ex 6,
            // pp 539-540.  As we step through indirections, run another type at
            // half speed. If they meet up, there's a cycle.
            var slowpoke = ut.@base; // walks half as fast as ut.base
            while (true)
            {
                var pt = ut.@base;
                if (pt.Kind() != reflect.Ptr)
                {
                    break;
                }

                ut.@base = pt.Elem();
                if (ut.@base == slowpoke)
                { // ut.base lapped slowpoke
                    // recursive pointer type.
                    return (_addr_null!, error.As(errors.New("can't represent recursive pointer type " + ut.@base.String()))!);

                }

                if (ut.indir % 2L == 0L)
                {
                    slowpoke = slowpoke.Elem();
                }

                ut.indir++;

            }


            {
                var ok__prev1 = ok;

                var (ok, indir) = implementsInterface(ut.user, gobEncoderInterfaceType);

                if (ok)
                {
                    ut.externalEnc = xGob;
                    ut.encIndir = indir;

                }                {
                    var ok__prev2 = ok;

                    (ok, indir) = implementsInterface(ut.user, binaryMarshalerInterfaceType);


                    else if (ok)
                    {
                        ut.externalEnc = xBinary;
                        ut.encIndir = indir;

                    } 

                    // NOTE(rsc): Would like to allow MarshalText here, but results in incompatibility
                    // with older encodings for net.IP. See golang.org/issue/6760.
                    // } else if ok, indir := implementsInterface(ut.user, textMarshalerInterfaceType); ok {
                    //     ut.externalEnc, ut.encIndir = xText, indir
                    // }

                    ok = ok__prev2;

                } 

                // NOTE(rsc): Would like to allow MarshalText here, but results in incompatibility
                // with older encodings for net.IP. See golang.org/issue/6760.
                // } else if ok, indir := implementsInterface(ut.user, textMarshalerInterfaceType); ok {
                //     ut.externalEnc, ut.encIndir = xText, indir
                // }


                ok = ok__prev1;

            } 

            // NOTE(rsc): Would like to allow MarshalText here, but results in incompatibility
            // with older encodings for net.IP. See golang.org/issue/6760.
            // } else if ok, indir := implementsInterface(ut.user, textMarshalerInterfaceType); ok {
            //     ut.externalEnc, ut.encIndir = xText, indir
            // }

            {
                var ok__prev1 = ok;

                (ok, indir) = implementsInterface(ut.user, gobDecoderInterfaceType);

                if (ok)
                {
                    ut.externalDec = xGob;
                    ut.decIndir = indir;

                }                {
                    var ok__prev2 = ok;

                    (ok, indir) = implementsInterface(ut.user, binaryUnmarshalerInterfaceType);


                    else if (ok)
                    {
                        ut.externalDec = xBinary;
                        ut.decIndir = indir;

                    } 

                    // See note above.
                    // } else if ok, indir := implementsInterface(ut.user, textUnmarshalerInterfaceType); ok {
                    //     ut.externalDec, ut.decIndir = xText, indir
                    // }

                    ok = ok__prev2;

                } 

                // See note above.
                // } else if ok, indir := implementsInterface(ut.user, textUnmarshalerInterfaceType); ok {
                //     ut.externalDec, ut.decIndir = xText, indir
                // }


                ok = ok__prev1;

            } 

            // See note above.
            // } else if ok, indir := implementsInterface(ut.user, textUnmarshalerInterfaceType); ok {
            //     ut.externalDec, ut.decIndir = xText, indir
            // }

            var (ui, _) = userTypeCache.LoadOrStore(rt, ut);
            return (ui._<ptr<userTypeInfo>>(), error.As(null!)!);

        }

        private static var gobEncoderInterfaceType = reflect.TypeOf((GobEncoder.val)(null)).Elem();        private static var gobDecoderInterfaceType = reflect.TypeOf((GobDecoder.val)(null)).Elem();        private static var binaryMarshalerInterfaceType = reflect.TypeOf((encoding.BinaryMarshaler.val)(null)).Elem();        private static var binaryUnmarshalerInterfaceType = reflect.TypeOf((encoding.BinaryUnmarshaler.val)(null)).Elem();        private static var textMarshalerInterfaceType = reflect.TypeOf((encoding.TextMarshaler.val)(null)).Elem();        private static var textUnmarshalerInterfaceType = reflect.TypeOf((encoding.TextUnmarshaler.val)(null)).Elem();

        // implementsInterface reports whether the type implements the
        // gobEncoder/gobDecoder interface.
        // It also returns the number of indirections required to get to the
        // implementation.
        private static (bool, sbyte) implementsInterface(reflect.Type typ, reflect.Type gobEncDecType)
        {
            bool success = default;
            sbyte indir = default;

            if (typ == null)
            {
                return ;
            }

            var rt = typ; 
            // The type might be a pointer and we need to keep
            // dereferencing to the base type until we find an implementation.
            while (true)
            {
                if (rt.Implements(gobEncDecType))
                {
                    return (true, indir);
                }

                {
                    var p = rt;

                    if (p.Kind() == reflect.Ptr)
                    {
                        indir++;
                        if (indir > 100L)
                        { // insane number of indirections
                            return (false, 0L);

                        }

                        rt = p.Elem();
                        continue;

                    }

                }

                break;

            } 
            // No luck yet, but if this is a base type (non-pointer), the pointer might satisfy.
 
            // No luck yet, but if this is a base type (non-pointer), the pointer might satisfy.
            if (typ.Kind() != reflect.Ptr)
            { 
                // Not a pointer, but does the pointer work?
                if (reflect.PtrTo(typ).Implements(gobEncDecType))
                {
                    return (true, -1L);
                }

            }

            return (false, 0L);

        }

        // userType returns, and saves, the information associated with user-provided type rt.
        // If the user type is not valid, it calls error.
        private static ptr<userTypeInfo> userType(reflect.Type rt)
        {
            var (ut, err) = validUserType(rt);
            if (err != null)
            {
                error_(err);
            }

            return _addr_ut!;

        }

        // A typeId represents a gob Type as an integer that can be passed on the wire.
        // Internally, typeIds are used as keys to a map to recover the underlying type info.
        private partial struct typeId // : int
        {
        }

        private static typeId nextId = default; // incremented for each new type we build
        private static sync.Mutex typeLock = default; // set while building a type
        private static readonly long firstUserId = (long)64L; // lowest id number granted to user

 // lowest id number granted to user

        private partial interface gobType
        {
            @string id();
            @string setId(typeId id);
            @string name();
            @string @string(); // not public; only for debugging
            @string safeString(map<typeId, bool> seen);
        }

        private static var types = make_map<reflect.Type, gobType>();
        private static var idToType = make_map<typeId, gobType>();
        private static map<typeId, gobType> builtinIdToType = default; // set in init() after builtins are established

        private static void setTypeId(gobType typ)
        { 
            // When building recursive types, someone may get there before us.
            if (typ.id() != 0L)
            {
                return ;
            }

            nextId++;
            typ.setId(nextId);
            idToType[nextId] = typ;

        }

        private static gobType gobType(this typeId t)
        {
            if (t == 0L)
            {
                return null;
            }

            return idToType[t];

        }

        // string returns the string representation of the type associated with the typeId.
        private static @string @string(this typeId t)
        {
            if (t.gobType() == null)
            {
                return "<nil>";
            }

            return t.gobType().@string();

        }

        // Name returns the name of the type associated with the typeId.
        private static @string name(this typeId t)
        {
            if (t.gobType() == null)
            {
                return "<nil>";
            }

            return t.gobType().name();

        }

        // CommonType holds elements of all types.
        // It is a historical artifact, kept for binary compatibility and exported
        // only for the benefit of the package's encoding of type descriptors. It is
        // not intended for direct use by clients.
        public partial struct CommonType
        {
            public @string Name;
            public typeId Id;
        }

        private static typeId id(this ptr<CommonType> _addr_t)
        {
            ref CommonType t = ref _addr_t.val;

            return t.Id;
        }

        private static void setId(this ptr<CommonType> _addr_t, typeId id)
        {
            ref CommonType t = ref _addr_t.val;

            t.Id = id;
        }

        private static @string @string(this ptr<CommonType> _addr_t)
        {
            ref CommonType t = ref _addr_t.val;

            return t.Name;
        }

        private static @string safeString(this ptr<CommonType> _addr_t, map<typeId, bool> seen)
        {
            ref CommonType t = ref _addr_t.val;

            return t.Name;
        }

        private static @string name(this ptr<CommonType> _addr_t)
        {
            ref CommonType t = ref _addr_t.val;

            return t.Name;
        }

        // Create and check predefined types
        // The string for tBytes is "bytes" not "[]byte" to signify its specialness.

 
        // Primordial types, needed during initialization.
        // Always passed as pointers so the interface{} type
        // goes through without losing its interfaceness.
        private static var tBool = bootstrapType("bool", (bool.val)(null), 1L);        private static var tInt = bootstrapType("int", (int.val)(null), 2L);        private static var tUint = bootstrapType("uint", (uint.val)(null), 3L);        private static var tFloat = bootstrapType("float", (float64.val)(null), 4L);        private static var tBytes = bootstrapType("bytes", new ptr<ptr<slice<byte>>>(null), 5L);        private static var tString = bootstrapType("string", (string.val)(null), 6L);        private static var tComplex = bootstrapType("complex", (complex128.val)(null), 7L);        private static var tInterface = bootstrapType("interface", 8L);        private static var tReserved7 = bootstrapType("_reserved1", 9L);        private static var tReserved6 = bootstrapType("_reserved1", 10L);        private static var tReserved5 = bootstrapType("_reserved1", 11L);        private static var tReserved4 = bootstrapType("_reserved1", 12L);        private static var tReserved3 = bootstrapType("_reserved1", 13L);        private static var tReserved2 = bootstrapType("_reserved1", 14L);        private static var tReserved1 = bootstrapType("_reserved1", 15L);

        // Predefined because it's needed by the Decoder
        private static var tWireType = mustGetTypeInfo(reflect.TypeOf(new wireType())).id;
        private static ptr<userTypeInfo> wireTypeUserInfo; // userTypeInfo of (*wireType)

        private static void init() => func((_, panic, __) =>
        { 
            // Some magic numbers to make sure there are no surprises.
            checkId(16L, tWireType);
            checkId(17L, mustGetTypeInfo(reflect.TypeOf(new arrayType())).id);
            checkId(18L, mustGetTypeInfo(reflect.TypeOf(new CommonType())).id);
            checkId(19L, mustGetTypeInfo(reflect.TypeOf(new sliceType())).id);
            checkId(20L, mustGetTypeInfo(reflect.TypeOf(new structType())).id);
            checkId(21L, mustGetTypeInfo(reflect.TypeOf(new fieldType())).id);
            checkId(23L, mustGetTypeInfo(reflect.TypeOf(new mapType())).id);

            builtinIdToType = make_map<typeId, gobType>();
            foreach (var (k, v) in idToType)
            {
                builtinIdToType[k] = v;
            } 

            // Move the id space upwards to allow for growth in the predefined world
            // without breaking existing files.
            if (nextId > firstUserId)
            {
                panic(fmt.Sprintln("nextId too large:", nextId));
            }

            nextId = firstUserId;
            registerBasics();
            wireTypeUserInfo = userType(reflect.TypeOf((wireType.val)(null)));

        });

        // Array type
        private partial struct arrayType
        {
            public ref CommonType CommonType => ref CommonType_val;
            public typeId Elem;
            public long Len;
        }

        private static ptr<arrayType> newArrayType(@string name)
        {
            ptr<arrayType> a = addr(new arrayType(CommonType{Name:name},0,0));
            return _addr_a!;
        }

        private static void init(this ptr<arrayType> _addr_a, gobType elem, long len)
        {
            ref arrayType a = ref _addr_a.val;
 
            // Set our type id before evaluating the element's, in case it's our own.
            setTypeId(a);
            a.Elem = elem.id();
            a.Len = len;

        }

        private static @string safeString(this ptr<arrayType> _addr_a, map<typeId, bool> seen)
        {
            ref arrayType a = ref _addr_a.val;

            if (seen[a.Id])
            {
                return a.Name;
            }

            seen[a.Id] = true;
            return fmt.Sprintf("[%d]%s", a.Len, a.Elem.gobType().safeString(seen));

        }

        private static @string @string(this ptr<arrayType> _addr_a)
        {
            ref arrayType a = ref _addr_a.val;

            return a.safeString(make_map<typeId, bool>());
        }

        // GobEncoder type (something that implements the GobEncoder interface)
        private partial struct gobEncoderType
        {
            public ref CommonType CommonType => ref CommonType_val;
        }

        private static ptr<gobEncoderType> newGobEncoderType(@string name)
        {
            ptr<gobEncoderType> g = addr(new gobEncoderType(CommonType{Name:name}));
            setTypeId(g);
            return _addr_g!;
        }

        private static @string safeString(this ptr<gobEncoderType> _addr_g, map<typeId, bool> seen)
        {
            ref gobEncoderType g = ref _addr_g.val;

            return g.Name;
        }

        private static @string @string(this ptr<gobEncoderType> _addr_g)
        {
            ref gobEncoderType g = ref _addr_g.val;

            return g.Name;
        }

        // Map type
        private partial struct mapType
        {
            public ref CommonType CommonType => ref CommonType_val;
            public typeId Key;
            public typeId Elem;
        }

        private static ptr<mapType> newMapType(@string name)
        {
            ptr<mapType> m = addr(new mapType(CommonType{Name:name},0,0));
            return _addr_m!;
        }

        private static void init(this ptr<mapType> _addr_m, gobType key, gobType elem)
        {
            ref mapType m = ref _addr_m.val;
 
            // Set our type id before evaluating the element's, in case it's our own.
            setTypeId(m);
            m.Key = key.id();
            m.Elem = elem.id();

        }

        private static @string safeString(this ptr<mapType> _addr_m, map<typeId, bool> seen)
        {
            ref mapType m = ref _addr_m.val;

            if (seen[m.Id])
            {
                return m.Name;
            }

            seen[m.Id] = true;
            var key = m.Key.gobType().safeString(seen);
            var elem = m.Elem.gobType().safeString(seen);
            return fmt.Sprintf("map[%s]%s", key, elem);

        }

        private static @string @string(this ptr<mapType> _addr_m)
        {
            ref mapType m = ref _addr_m.val;

            return m.safeString(make_map<typeId, bool>());
        }

        // Slice type
        private partial struct sliceType
        {
            public ref CommonType CommonType => ref CommonType_val;
            public typeId Elem;
        }

        private static ptr<sliceType> newSliceType(@string name)
        {
            ptr<sliceType> s = addr(new sliceType(CommonType{Name:name},0));
            return _addr_s!;
        }

        private static void init(this ptr<sliceType> _addr_s, gobType elem)
        {
            ref sliceType s = ref _addr_s.val;
 
            // Set our type id before evaluating the element's, in case it's our own.
            setTypeId(s); 
            // See the comments about ids in newTypeObject. Only slices and
            // structs have mutual recursion.
            if (elem.id() == 0L)
            {
                setTypeId(elem);
            }

            s.Elem = elem.id();

        }

        private static @string safeString(this ptr<sliceType> _addr_s, map<typeId, bool> seen)
        {
            ref sliceType s = ref _addr_s.val;

            if (seen[s.Id])
            {
                return s.Name;
            }

            seen[s.Id] = true;
            return fmt.Sprintf("[]%s", s.Elem.gobType().safeString(seen));

        }

        private static @string @string(this ptr<sliceType> _addr_s)
        {
            ref sliceType s = ref _addr_s.val;

            return s.safeString(make_map<typeId, bool>());
        }

        // Struct type
        private partial struct fieldType
        {
            public @string Name;
            public typeId Id;
        }

        private partial struct structType
        {
            public ref CommonType CommonType => ref CommonType_val;
            public slice<ptr<fieldType>> Field;
        }

        private static @string safeString(this ptr<structType> _addr_s, map<typeId, bool> seen)
        {
            ref structType s = ref _addr_s.val;

            if (s == null)
            {
                return "<nil>";
            }

            {
                var (_, ok) = seen[s.Id];

                if (ok)
                {
                    return s.Name;
                }

            }

            seen[s.Id] = true;
            var str = s.Name + " = struct { ";
            foreach (var (_, f) in s.Field)
            {
                str += fmt.Sprintf("%s %s; ", f.Name, f.Id.gobType().safeString(seen));
            }
            str += "}";
            return str;

        }

        private static @string @string(this ptr<structType> _addr_s)
        {
            ref structType s = ref _addr_s.val;

            return s.safeString(make_map<typeId, bool>());
        }

        private static ptr<structType> newStructType(@string name)
        {
            ptr<structType> s = addr(new structType(CommonType{Name:name},nil)); 
            // For historical reasons we set the id here rather than init.
            // See the comment in newTypeObject for details.
            setTypeId(s);
            return _addr_s!;

        }

        // newTypeObject allocates a gobType for the reflection type rt.
        // Unless ut represents a GobEncoder, rt should be the base type
        // of ut.
        // This is only called from the encoding side. The decoding side
        // works through typeIds and userTypeInfos alone.
        private static (gobType, error) newTypeObject(@string name, ptr<userTypeInfo> _addr_ut, reflect.Type rt) => func((defer, _, __) =>
        {
            gobType _p0 = default;
            error _p0 = default!;
            ref userTypeInfo ut = ref _addr_ut.val;
 
            // Does this type implement GobEncoder?
            if (ut.externalEnc != 0L)
            {
                return (newGobEncoderType(name), error.As(null!)!);
            }

            error err = default!;
            gobType type0 = default!;            gobType type1 = default!;

            defer(() =>
            {
                if (err != null)
                {
                    delete(types, rt);
                }

            }()); 
            // Install the top-level type before the subtypes (e.g. struct before
            // fields) so recursive types can be constructed safely.
            {
                var t__prev1 = t;

                var t = rt;


                // All basic types are easy: they are predefined.
                if (t.Kind() == reflect.Bool) 
                    return (tBool.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Int || t.Kind() == reflect.Int8 || t.Kind() == reflect.Int16 || t.Kind() == reflect.Int32 || t.Kind() == reflect.Int64) 
                    return (tInt.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Uint || t.Kind() == reflect.Uint8 || t.Kind() == reflect.Uint16 || t.Kind() == reflect.Uint32 || t.Kind() == reflect.Uint64 || t.Kind() == reflect.Uintptr) 
                    return (tUint.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Float32 || t.Kind() == reflect.Float64) 
                    return (tFloat.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Complex64 || t.Kind() == reflect.Complex128) 
                    return (tComplex.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.String) 
                    return (tString.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Interface) 
                    return (tInterface.gobType(), error.As(null!)!);
                else if (t.Kind() == reflect.Array) 
                    var at = newArrayType(name);
                    types[rt] = at;
                    type0, err = getBaseType("", t.Elem());
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    } 
                    // Historical aside:
                    // For arrays, maps, and slices, we set the type id after the elements
                    // are constructed. This is to retain the order of type id allocation after
                    // a fix made to handle recursive types, which changed the order in
                    // which types are built. Delaying the setting in this way preserves
                    // type ids while allowing recursive types to be described. Structs,
                    // done below, were already handling recursion correctly so they
                    // assign the top-level id before those of the field.
                    at.init(type0, t.Len());
                    return (at, error.As(null!)!);
                else if (t.Kind() == reflect.Map) 
                    var mt = newMapType(name);
                    types[rt] = mt;
                    type0, err = getBaseType("", t.Key());
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    type1, err = getBaseType("", t.Elem());
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    mt.init(type0, type1);
                    return (mt, error.As(null!)!);
                else if (t.Kind() == reflect.Slice) 
                    // []byte == []uint8 is a special case
                    if (t.Elem().Kind() == reflect.Uint8)
                    {
                        return (tBytes.gobType(), error.As(null!)!);
                    }

                    var st = newSliceType(name);
                    types[rt] = st;
                    type0, err = getBaseType(t.Elem().Name(), t.Elem());
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    st.init(type0);
                    return (st, error.As(null!)!);
                else if (t.Kind() == reflect.Struct) 
                    st = newStructType(name);
                    types[rt] = st;
                    idToType[st.id()] = st;
                    for (long i = 0L; i < t.NumField(); i++)
                    {
                        ref var f = ref heap(t.Field(i), out ptr<var> _addr_f);
                        if (!isSent(_addr_f))
                        {
                            continue;
                        }

                        var typ = userType(f.Type).@base;
                        var tname = typ.Name();
                        if (tname == "")
                        {
                            t = userType(f.Type).@base;
                            tname = t.String();
                        }

                        var (gt, err) = getBaseType(tname, f.Type);
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        } 
                        // Some mutually recursive types can cause us to be here while
                        // still defining the element. Fix the element type id here.
                        // We could do this more neatly by setting the id at the start of
                        // building every type, but that would break binary compatibility.
                        if (gt.id() == 0L)
                        {
                            setTypeId(gt);
                        }

                        st.Field = append(st.Field, addr(new fieldType(f.Name,gt.id())));

                    }

                    return (st, error.As(null!)!);
                else 
                    return (null, error.As(errors.New("gob NewTypeObject can't handle type: " + rt.String()))!);


                t = t__prev1;
            }

        });

        // isExported reports whether this is an exported - upper case - name.
        private static bool isExported(@string name)
        {
            var (rune, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(rune);
        }

        // isSent reports whether this struct field is to be transmitted.
        // It will be transmitted only if it is exported and not a chan or func field
        // or pointer to chan or func.
        private static bool isSent(ptr<reflect.StructField> _addr_field)
        {
            ref reflect.StructField field = ref _addr_field.val;

            if (!isExported(field.Name))
            {
                return false;
            } 
            // If the field is a chan or func or pointer thereto, don't send it.
            // That is, treat it like an unexported field.
            var typ = field.Type;
            while (typ.Kind() == reflect.Ptr)
            {
                typ = typ.Elem();
            }

            if (typ.Kind() == reflect.Chan || typ.Kind() == reflect.Func)
            {
                return false;
            }

            return true;

        }

        // getBaseType returns the Gob type describing the given reflect.Type's base type.
        // typeLock must be held.
        private static (gobType, error) getBaseType(@string name, reflect.Type rt)
        {
            gobType _p0 = default;
            error _p0 = default!;

            var ut = userType(rt);
            return getType(name, _addr_ut, ut.@base);
        }

        // getType returns the Gob type describing the given reflect.Type.
        // Should be called only when handling GobEncoders/Decoders,
        // which may be pointers. All other types are handled through the
        // base type, never a pointer.
        // typeLock must be held.
        private static (gobType, error) getType(@string name, ptr<userTypeInfo> _addr_ut, reflect.Type rt)
        {
            gobType _p0 = default;
            error _p0 = default!;
            ref userTypeInfo ut = ref _addr_ut.val;

            var (typ, present) = types[rt];
            if (present)
            {
                return (typ, error.As(null!)!);
            }

            var (typ, err) = newTypeObject(name, _addr_ut, rt);
            if (err == null)
            {
                types[rt] = typ;
            }

            return (typ, error.As(err)!);

        }

        private static void checkId(typeId want, typeId got) => func((_, panic, __) =>
        {
            if (want != got)
            {
                fmt.Fprintf(os.Stderr, "checkId: %d should be %d\n", int(got), int(want));
                panic("bootstrap type wrong id: " + got.name() + " " + got.@string() + " not " + want.@string());
            }

        });

        // used for building the basic types; called only from init().  the incoming
        // interface always refers to a pointer.
        private static typeId bootstrapType(@string name, object e, typeId expect) => func((_, panic, __) =>
        {
            var rt = reflect.TypeOf(e).Elem();
            var (_, present) = types[rt];
            if (present)
            {
                panic("bootstrap type already present: " + name + ", " + rt.String());
            }

            ptr<CommonType> typ = addr(new CommonType(Name:name));
            types[rt] = typ;
            setTypeId(typ);
            checkId(expect, nextId);
            userType(rt); // might as well cache it now
            return nextId;

        });

        // Representation of the information we send and receive about this type.
        // Each value we send is preceded by its type definition: an encoded int.
        // However, the very first time we send the value, we first send the pair
        // (-id, wireType).
        // For bootstrapping purposes, we assume that the recipient knows how
        // to decode a wireType; it is exactly the wireType struct here, interpreted
        // using the gob rules for sending a structure, except that we assume the
        // ids for wireType and structType etc. are known. The relevant pieces
        // are built in encode.go's init() function.
        // To maintain binary compatibility, if you extend this type, always put
        // the new fields last.
        private partial struct wireType
        {
            public ptr<arrayType> ArrayT;
            public ptr<sliceType> SliceT;
            public ptr<structType> StructT;
            public ptr<mapType> MapT;
            public ptr<gobEncoderType> GobEncoderT;
            public ptr<gobEncoderType> BinaryMarshalerT;
            public ptr<gobEncoderType> TextMarshalerT;
        }

        private static @string @string(this ptr<wireType> _addr_w)
        {
            ref wireType w = ref _addr_w.val;

            const @string unknown = (@string)"unknown type";

            if (w == null)
            {
                return unknown;
            }


            if (w.ArrayT != null) 
                return w.ArrayT.Name;
            else if (w.SliceT != null) 
                return w.SliceT.Name;
            else if (w.StructT != null) 
                return w.StructT.Name;
            else if (w.MapT != null) 
                return w.MapT.Name;
            else if (w.GobEncoderT != null) 
                return w.GobEncoderT.Name;
            else if (w.BinaryMarshalerT != null) 
                return w.BinaryMarshalerT.Name;
            else if (w.TextMarshalerT != null) 
                return w.TextMarshalerT.Name;
                        return unknown;

        }

        private partial struct typeInfo
        {
            public typeId id;
            public sync.Mutex encInit; // protects creation of encoder
            public atomic.Value encoder; // *encEngine
            public ptr<wireType> wire;
        }

        // typeInfoMap is an atomic pointer to map[reflect.Type]*typeInfo.
        // It's updated copy-on-write. Readers just do an atomic load
        // to get the current version of the map. Writers make a full copy of
        // the map and atomically update the pointer to point to the new map.
        // Under heavy read contention, this is significantly faster than a map
        // protected by a mutex.
        private static atomic.Value typeInfoMap = default;

        private static ptr<typeInfo> lookupTypeInfo(reflect.Type rt)
        {
            map<reflect.Type, ptr<typeInfo>> (m, _) = typeInfoMap.Load()._<map<reflect.Type, ptr<typeInfo>>>();
            return _addr_m[rt]!;
        }

        private static (ptr<typeInfo>, error) getTypeInfo(ptr<userTypeInfo> _addr_ut)
        {
            ptr<typeInfo> _p0 = default!;
            error _p0 = default!;
            ref userTypeInfo ut = ref _addr_ut.val;

            var rt = ut.@base;
            if (ut.externalEnc != 0L)
            { 
                // We want the user type, not the base type.
                rt = ut.user;

            }

            {
                var info = lookupTypeInfo(rt);

                if (info != null)
                {
                    return (_addr_info!, error.As(null!)!);
                }

            }

            return _addr_buildTypeInfo(_addr_ut, rt)!;

        }

        // buildTypeInfo constructs the type information for the type
        // and stores it in the type info map.
        private static (ptr<typeInfo>, error) buildTypeInfo(ptr<userTypeInfo> _addr_ut, reflect.Type rt) => func((defer, _, __) =>
        {
            ptr<typeInfo> _p0 = default!;
            error _p0 = default!;
            ref userTypeInfo ut = ref _addr_ut.val;

            typeLock.Lock();
            defer(typeLock.Unlock());

            {
                var info__prev1 = info;

                var info = lookupTypeInfo(rt);

                if (info != null)
                {
                    return (_addr_info!, error.As(null!)!);
                }

                info = info__prev1;

            }


            var (gt, err) = getBaseType(rt.Name(), rt);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            info = addr(new typeInfo(id:gt.id()));

            if (ut.externalEnc != 0L)
            {
                var (userType, err) = getType(rt.Name(), _addr_ut, rt);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                ptr<gobEncoderType> gt = userType.id().gobType()._<ptr<gobEncoderType>>();

                if (ut.externalEnc == xGob) 
                    info.wire = addr(new wireType(GobEncoderT:gt));
                else if (ut.externalEnc == xBinary) 
                    info.wire = addr(new wireType(BinaryMarshalerT:gt));
                else if (ut.externalEnc == xText) 
                    info.wire = addr(new wireType(TextMarshalerT:gt));
                                rt = ut.user;

            }
            else
            {
                var t = info.id.gobType();
                {
                    var typ = rt;


                    if (typ.Kind() == reflect.Array) 
                        info.wire = addr(new wireType(ArrayT:t.(*arrayType)));
                    else if (typ.Kind() == reflect.Map) 
                        info.wire = addr(new wireType(MapT:t.(*mapType)));
                    else if (typ.Kind() == reflect.Slice) 
                        // []byte == []uint8 is a special case handled separately
                        if (typ.Elem().Kind() != reflect.Uint8)
                        {
                            info.wire = addr(new wireType(SliceT:t.(*sliceType)));
                        }

                    else if (typ.Kind() == reflect.Struct) 
                        info.wire = addr(new wireType(StructT:t.(*structType)));

                }

            } 

            // Create new map with old contents plus new entry.
            var newm = make_map<reflect.Type, ptr<typeInfo>>();
            map<reflect.Type, ptr<typeInfo>> (m, _) = typeInfoMap.Load()._<map<reflect.Type, ptr<typeInfo>>>();
            foreach (var (k, v) in m)
            {
                newm[k] = v;
            }
            newm[rt] = info;
            typeInfoMap.Store(newm);
            return (_addr_info!, error.As(null!)!);

        });

        // Called only when a panic is acceptable and unexpected.
        private static ptr<typeInfo> mustGetTypeInfo(reflect.Type rt) => func((_, panic, __) =>
        {
            var (t, err) = getTypeInfo(_addr_userType(rt));
            if (err != null)
            {
                panic("getTypeInfo: " + err.Error());
            }

            return _addr_t!;

        });

        // GobEncoder is the interface describing data that provides its own
        // representation for encoding values for transmission to a GobDecoder.
        // A type that implements GobEncoder and GobDecoder has complete
        // control over the representation of its data and may therefore
        // contain things such as private fields, channels, and functions,
        // which are not usually transmissible in gob streams.
        //
        // Note: Since gobs can be stored permanently, it is good design
        // to guarantee the encoding used by a GobEncoder is stable as the
        // software evolves. For instance, it might make sense for GobEncode
        // to include a version number in the encoding.
        public partial interface GobEncoder
        {
            (slice<byte>, error) GobEncode();
        }

        // GobDecoder is the interface describing data that provides its own
        // routine for decoding transmitted values sent by a GobEncoder.
        public partial interface GobDecoder
        {
            error GobDecode(slice<byte> _p0);
        }

        private static sync.Map nameToConcreteType = default;        private static sync.Map concreteTypeToName = default;

        // RegisterName is like Register but uses the provided name rather than the
        // type's default.
        public static void RegisterName(@string name, object value) => func((_, panic, __) =>
        {
            if (name == "")
            { 
                // reserved for nil
                panic("attempt to register empty name");

            }

            var ut = userType(reflect.TypeOf(value)); 

            // Check for incompatible duplicates. The name must refer to the
            // same user type, and vice versa.

            // Store the name and type provided by the user....
            {
                var (t, dup) = nameToConcreteType.LoadOrStore(name, reflect.TypeOf(value));

                if (dup && t != ut.user)
                {
                    panic(fmt.Sprintf("gob: registering duplicate types for %q: %s != %s", name, t, ut.user));
                } 

                // but the flattened type in the type table, since that's what decode needs.

            } 

            // but the flattened type in the type table, since that's what decode needs.
            {
                var (n, dup) = concreteTypeToName.LoadOrStore(ut.@base, name);

                if (dup && n != name)
                {
                    nameToConcreteType.Delete(name);
                    panic(fmt.Sprintf("gob: registering duplicate names for %s: %q != %q", ut.user, n, name));
                }

            }

        });

        // Register records a type, identified by a value for that type, under its
        // internal type name. That name will identify the concrete type of a value
        // sent or received as an interface variable. Only types that will be
        // transferred as implementations of interface values need to be registered.
        // Expecting to be used only during initialization, it panics if the mapping
        // between types and names is not a bijection.
        public static void Register(object value)
        { 
            // Default to printed representation for unnamed types
            var rt = reflect.TypeOf(value);
            var name = rt.String(); 

            // But for named types (or pointers to them), qualify with import path (but see inner comment).
            // Dereference one pointer looking for a named type.
            @string star = "";
            if (rt.Name() == "")
            {
                {
                    var pt = rt;

                    if (pt.Kind() == reflect.Ptr)
                    {
                        star = "*"; 
                        // NOTE: The following line should be rt = pt.Elem() to implement
                        // what the comment above claims, but fixing it would break compatibility
                        // with existing gobs.
                        //
                        // Given package p imported as "full/p" with these definitions:
                        //     package p
                        //     type T1 struct { ... }
                        // this table shows the intended and actual strings used by gob to
                        // name the types:
                        //
                        // Type      Correct string     Actual string
                        //
                        // T1        full/p.T1          full/p.T1
                        // *T1       *full/p.T1         *p.T1
                        //
                        // The missing full path cannot be fixed without breaking existing gob decoders.
                        rt = pt;

                    }

                }

            }

            if (rt.Name() != "")
            {
                if (rt.PkgPath() == "")
                {
                    name = star + rt.Name();
                }
                else
                {
                    name = star + rt.PkgPath() + "." + rt.Name();
                }

            }

            RegisterName(name, value);

        }

        private static void registerBasics()
        {
            Register(int(0L));
            Register(int8(0L));
            Register(int16(0L));
            Register(int32(0L));
            Register(int64(0L));
            Register(uint(0L));
            Register(uint8(0L));
            Register(uint16(0L));
            Register(uint32(0L));
            Register(uint64(0L));
            Register(float32(0L));
            Register(float64(0L));
            Register(complex64(0iUL));
            Register(complex128(0iUL));
            Register(uintptr(0L));
            Register(false);
            Register("");
            Register((slice<byte>)null);
            Register((slice<long>)null);
            Register((slice<sbyte>)null);
            Register((slice<short>)null);
            Register((slice<int>)null);
            Register((slice<long>)null);
            Register((slice<ulong>)null);
            Register((slice<byte>)null);
            Register((slice<ushort>)null);
            Register((slice<uint>)null);
            Register((slice<ulong>)null);
            Register((slice<float>)null);
            Register((slice<double>)null);
            Register((slice<complex64>)null);
            Register((slice<System.Numerics.Complex128>)null);
            Register((slice<System.UIntPtr>)null);
            Register((slice<bool>)null);
            Register((slice<@string>)null);
        }
    }
}}
