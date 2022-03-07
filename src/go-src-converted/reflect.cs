// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package interp -- go2cs converted at 2022 March 06 23:33:38 UTC
// import "golang.org/x/tools/go/ssa/interp" ==> using interp = go.golang.org.x.tools.go.ssa.interp_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\reflect.go
// Emulated "reflect" package.
//
// We completely replace the built-in "reflect" package.
// The only thing clients can depend upon are that reflect.Type is an
// interface and reflect.Value is an (opaque) struct.

using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using @unsafe = go.@unsafe_package;

using ssa = go.golang.org.x.tools.go.ssa_package;

namespace go.golang.org.x.tools.go.ssa;

public static partial class interp_package {

private partial struct opaqueType : types.Type {
    public ref types.Type Type => ref Type_val;
    public @string name;
}

private static @string String(this ptr<opaqueType> _addr_t) {
    ref opaqueType t = ref _addr_t.val;

    return t.name;
}

// A bogus "reflect" type-checker package.  Shared across interpreters.
private static var reflectTypesPackage = types.NewPackage("reflect", "reflect");

// rtype is the concrete type the interpreter uses to implement the
// reflect.Type interface.
//
// type rtype <opaque>
private static var rtypeType = makeNamedType("rtype", addr(new opaqueType(nil,"rtype")));

// error is an (interpreted) named type whose underlying type is string.
// The interpreter uses it for all implementations of the built-in error
// interface that it creates.
// We put it in the "reflect" package for expedience.
//
// type error string
private static var errorType = makeNamedType("error", addr(new opaqueType(nil,"error")));

private static ptr<types.Named> makeNamedType(@string name, types.Type underlying) {
    var obj = types.NewTypeName(token.NoPos, reflectTypesPackage, name, null);
    return _addr_types.NewNamed(obj, underlying, null)!;
}

private static value makeReflectValue(types.Type t, value v) {
    return new structure(rtype{t},v);
}

// Given a reflect.Value, returns its rtype.
private static rtype rV2T(value v) {
    return v._<structure>()[0]._<rtype>();
}

// Given a reflect.Value, returns the underlying interpreter value.
private static value rV2V(value v) {
    return v._<structure>()[1];
}

// makeReflectType boxes up an rtype in a reflect.Type interface.
private static value makeReflectType(rtype rt) {
    return new iface(rtypeType,rt);
}

private static value ext۰reflect۰rtype۰Bits(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) int
    rtype rt = args[0]._<rtype>().t;
    ptr<types.Basic> (basic, ok) = rt.Underlying()._<ptr<types.Basic>>();
    if (!ok) {
        panic(fmt.Sprintf("reflect.Type.Bits(%T): non-basic type", rt));
    }
    return int(fr.i.sizes.Sizeof(basic)) * 8;

});

private static value ext۰reflect۰rtype۰Elem(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) reflect.Type
    return makeReflectType(new rtype(args[0].(rtype).t.Underlying().(interface{Elem()types.Type}).Elem()));

}

private static value ext۰reflect۰rtype۰Field(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype, i int) reflect.StructField
    ptr<types.Struct> st = args[0]._<rtype>().t.Underlying()._<ptr<types.Struct>>();
    nint i = args[1]._<nint>();
    var f = st.Field(i);
    return new structure(f.Name(),f.Pkg().Path(),makeReflectType(rtype{f.Type()}),st.Tag(i),0,[]value{},f.Anonymous(),);

}

private static value ext۰reflect۰rtype۰In(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype, i int) int
    nint i = args[1]._<nint>();
    return makeReflectType(new rtype(args[0].(rtype).t.(*types.Signature).Params().At(i).Type()));

}

private static value ext۰reflect۰rtype۰Kind(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) uint
    return uint(reflectKind(args[0]._<rtype>().t));

}

private static value ext۰reflect۰rtype۰NumField(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) int
    return args[0]._<rtype>().t.Underlying()._<ptr<types.Struct>>().NumFields();

}

private static value ext۰reflect۰rtype۰NumIn(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) int
    return args[0]._<rtype>().t._<ptr<types.Signature>>().Params().Len();

}

private static value ext۰reflect۰rtype۰NumMethod(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) int
    return fr.i.prog.MethodSets.MethodSet(args[0]._<rtype>().t).Len();

}

private static value ext۰reflect۰rtype۰NumOut(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) int
    return args[0]._<rtype>().t._<ptr<types.Signature>>().Results().Len();

}

private static value ext۰reflect۰rtype۰Out(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype, i int) int
    nint i = args[1]._<nint>();
    return makeReflectType(new rtype(args[0].(rtype).t.(*types.Signature).Results().At(i).Type()));

}

private static value ext۰reflect۰rtype۰Size(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) uintptr
    return uintptr(fr.i.sizes.Sizeof(args[0]._<rtype>().t));

}

private static value ext۰reflect۰rtype۰String(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) string
    return args[0]._<rtype>().t.String();

}

private static value ext۰reflect۰New(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.Type) reflect.Value
    rtype t = args[0]._<iface>().v._<rtype>().t;
    ref var alloc = ref heap(zero(t), out ptr<var> _addr_alloc);
    return makeReflectValue(types.NewPointer(t), _addr_alloc);

}

private static value ext۰reflect۰SliceOf(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) Type
    return makeReflectType(new rtype(types.NewSlice(args[0].(iface).v.(rtype).t)));

}

private static value ext۰reflect۰TypeOf(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.rtype) Type
    return makeReflectType(new rtype(args[0].(iface).t));

}

private static value ext۰reflect۰ValueOf(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (interface{}) reflect.Value
    iface itf = args[0]._<iface>();
    return makeReflectValue(itf.t, itf.v);

}

private static value ext۰reflect۰Zero(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (t reflect.Type) reflect.Value
    rtype t = args[0]._<iface>().v._<rtype>().t;
    return makeReflectValue(t, zero(t));

}

private static reflect.Kind reflectKind(types.Type t) => func((_, panic, _) => {
    switch (t.type()) {
        case ptr<types.Named> t:
            return reflectKind(t.Underlying());
            break;
        case ptr<types.Basic> t:

            if (t.Kind() == types.Bool) 
                return reflect.Bool;
            else if (t.Kind() == types.Int) 
                return reflect.Int;
            else if (t.Kind() == types.Int8) 
                return reflect.Int8;
            else if (t.Kind() == types.Int16) 
                return reflect.Int16;
            else if (t.Kind() == types.Int32) 
                return reflect.Int32;
            else if (t.Kind() == types.Int64) 
                return reflect.Int64;
            else if (t.Kind() == types.Uint) 
                return reflect.Uint;
            else if (t.Kind() == types.Uint8) 
                return reflect.Uint8;
            else if (t.Kind() == types.Uint16) 
                return reflect.Uint16;
            else if (t.Kind() == types.Uint32) 
                return reflect.Uint32;
            else if (t.Kind() == types.Uint64) 
                return reflect.Uint64;
            else if (t.Kind() == types.Uintptr) 
                return reflect.Uintptr;
            else if (t.Kind() == types.Float32) 
                return reflect.Float32;
            else if (t.Kind() == types.Float64) 
                return reflect.Float64;
            else if (t.Kind() == types.Complex64) 
                return reflect.Complex64;
            else if (t.Kind() == types.Complex128) 
                return reflect.Complex128;
            else if (t.Kind() == types.String) 
                return reflect.String;
            else if (t.Kind() == types.UnsafePointer) 
                return reflect.UnsafePointer;
                        break;
        case ptr<types.Array> t:
            return reflect.Array;
            break;
        case ptr<types.Chan> t:
            return reflect.Chan;
            break;
        case ptr<types.Signature> t:
            return reflect.Func;
            break;
        case ptr<types.Interface> t:
            return reflect.Interface;
            break;
        case ptr<types.Map> t:
            return reflect.Map;
            break;
        case ptr<types.Pointer> t:
            return reflect.Ptr;
            break;
        case ptr<types.Slice> t:
            return reflect.Slice;
            break;
        case ptr<types.Struct> t:
            return reflect.Struct;
            break;
    }
    panic(fmt.Sprint("unexpected type: ", t));

});

private static value ext۰reflect۰Value۰Kind(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) uint
    return uint(reflectKind(rV2T(args[0]).t));

}

private static value ext۰reflect۰Value۰String(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) string
    return toString(rV2V(args[0]));

}

private static value ext۰reflect۰Value۰Type(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) reflect.Type
    return makeReflectType(rV2T(args[0]));

}

private static value ext۰reflect۰Value۰Uint(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) uint64
    switch (rV2V(args[0]).type()) {
        case nuint v:
            return uint64(v);
            break;
        case byte v:
            return uint64(v);
            break;
        case ushort v:
            return uint64(v);
            break;
        case uint v:
            return uint64(v);
            break;
        case ulong v:
            return uint64(v);
            break;
        case System.UIntPtr v:
            return uint64(v);
            break;
    }
    panic("reflect.Value.Uint");

});

private static value ext۰reflect۰Value۰Len(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) int
    switch (rV2V(args[0]).type()) {
        case @string v:
            return len(v);
            break;
        case array v:
            return len(v);
            break;
        case channel<value> v:
            return cap(v);
            break;
        case slice<value> v:
            return len(v);
            break;
        case ptr<hashmap> v:
            return v.len();
            break;
        case map<value, value> v:
            return len(v);
            break;
        default:
        {
            var v = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).Len(%v)", v));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰MapIndex(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) Value
    ptr<types.Map> tValue = rV2T(args[0]).t.Underlying()._<ptr<types.Map>>().Key();
    var k = rV2V(args[1]);
    switch (rV2V(args[0]).type()) {
        case map<value, value> m:
            {
                var v__prev1 = v;

                var (v, ok) = m[k];

                if (ok) {
                    return makeReflectValue(tValue, v);
                }

                v = v__prev1;

            }


            break;
        case ptr<hashmap> m:
            {
                var v__prev1 = v;

                var v = m.lookup(k._<hashable>());

                if (v != null) {
                    return makeReflectValue(tValue, v);
                }

                v = v__prev1;

            }


            break;
        default:
        {
            var m = rV2V(args[0]).type();
            panic(fmt.Sprintf("(reflect.Value).MapIndex(%T, %T)", m, k));
            break;
        }
    }
    return makeReflectValue(null, null);

});

private static value ext۰reflect۰Value۰MapKeys(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) []Value
    slice<value> keys = default;
    ptr<types.Map> tKey = rV2T(args[0]).t.Underlying()._<ptr<types.Map>>().Key();
    switch (rV2V(args[0]).type()) {
        case map<value, value> v:
            foreach (var (k) in v) {
                keys = append(keys, makeReflectValue(tKey, k));
            }
            break;
        case ptr<hashmap> v:
            foreach (var (_, e) in v.entries()) {
                while (e != null) {
                    keys = append(keys, makeReflectValue(tKey, e.key));
                    e = e.next;
                }


            }
            break;
        default:
        {
            var v = rV2V(args[0]).type();
            panic(fmt.Sprintf("(reflect.Value).MapKeys(%T)", v));
            break;
        }
    }
    return keys;

});

private static value ext۰reflect۰Value۰NumField(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) int
    return len(rV2V(args[0])._<structure>());

}

private static value ext۰reflect۰Value۰NumMethod(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) int
    return fr.i.prog.MethodSets.MethodSet(rV2T(args[0]).t).Len();

}

private static value ext۰reflect۰Value۰Pointer(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value) uintptr
    switch (rV2V(args[0]).type()) {
        case ptr<value> v:
            return uintptr(@unsafe.Pointer(v));
            break;
        case channel<value> v:
            return reflect.ValueOf(v).Pointer();
            break;
        case slice<value> v:
            return reflect.ValueOf(v).Pointer();
            break;
        case ptr<hashmap> v:
            return reflect.ValueOf(v.entries()).Pointer();
            break;
        case map<value, value> v:
            return reflect.ValueOf(v).Pointer();
            break;
        case ptr<ssa.Function> v:
            return uintptr(@unsafe.Pointer(v));
            break;
        case ptr<closure> v:
            return uintptr(@unsafe.Pointer(v));
            break;
        default:
        {
            var v = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).Pointer(%T)", v));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰Index(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value, i int) Value
    nint i = args[1]._<nint>();
    var t = rV2T(args[0]).t.Underlying();
    switch (rV2V(args[0]).type()) {
        case array v:
            return makeReflectValue(t._<ptr<types.Array>>().Elem(), v[i]);
            break;
        case slice<value> v:
            return makeReflectValue(t._<ptr<types.Slice>>().Elem(), v[i]);
            break;
        default:
        {
            var v = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).Index(%T)", v));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰Bool(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) bool
    return rV2V(args[0])._<bool>();

}

private static value ext۰reflect۰Value۰CanAddr(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value) bool
    // Always false for our representation.
    return false;

}

private static value ext۰reflect۰Value۰CanInterface(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value) bool
    // Always true for our representation.
    return true;

}

private static value ext۰reflect۰Value۰Elem(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value) reflect.Value
    switch (rV2V(args[0]).type()) {
        case iface x:
            return makeReflectValue(x.t, x.v);
            break;
        case ptr<value> x:
            return makeReflectValue(rV2T(args[0]).t.Underlying()._<ptr<types.Pointer>>().Elem(), x.val);
            break;
        default:
        {
            var x = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).Elem(%T)", x));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰Field(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value, i int) reflect.Value
    var v = args[0];
    nint i = args[1]._<nint>();
    return makeReflectValue(rV2T(v).t.Underlying()._<ptr<types.Struct>>().Field(i).Type(), rV2V(v)._<structure>()[i]);

}

private static value ext۰reflect۰Value۰Float(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) float64
    switch (rV2V(args[0]).type()) {
        case float v:
            return float64(v);
            break;
        case double v:
            return float64(v);
            break;
    }
    panic("reflect.Value.Float");

});

private static value ext۰reflect۰Value۰Interface(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value) interface{}
    return ext۰reflect۰valueInterface(_addr_fr, args);

}

private static value ext۰reflect۰Value۰Int(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) int64
    switch (rV2V(args[0]).type()) {
        case nint x:
            return int64(x);
            break;
        case sbyte x:
            return int64(x);
            break;
        case short x:
            return int64(x);
            break;
        case int x:
            return int64(x);
            break;
        case long x:
            return x;
            break;
        default:
        {
            var x = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).Int(%T)", x));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰IsNil(ptr<frame> _addr_fr, slice<value> args) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) bool
    switch (rV2V(args[0]).type()) {
        case ptr<value> x:
            return x == null;
            break;
        case channel<value> x:
            return x == null;
            break;
        case map<value, value> x:
            return x == null;
            break;
        case ptr<hashmap> x:
            return x == null;
            break;
        case iface x:
            return x.t == null;
            break;
        case slice<value> x:
            return x == null;
            break;
        case ptr<ssa.Function> x:
            return x == null;
            break;
        case ptr<ssa.Builtin> x:
            return x == null;
            break;
        case ptr<closure> x:
            return x == null;
            break;
        default:
        {
            var x = rV2V(args[0]).type();
            panic(fmt.Sprintf("reflect.(Value).IsNil(%T)", x));
            break;
        }
    }

});

private static value ext۰reflect۰Value۰IsValid(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (reflect.Value) bool
    return rV2V(args[0]) != null;

}

private static value ext۰reflect۰Value۰Set(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // TODO(adonovan): implement.
    return null;

}

private static value ext۰reflect۰valueInterface(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;
 
    // Signature: func (v reflect.Value, safe bool) interface{}
    structure v = args[0]._<structure>();
    return new iface(rV2T(v).t,rV2V(v));

}

private static value ext۰reflect۰error۰Error(ptr<frame> _addr_fr, slice<value> args) {
    ref frame fr = ref _addr_fr.val;

    return args[0];
}

// newMethod creates a new method of the specified name, package and receiver type.
private static ptr<ssa.Function> newMethod(ptr<ssa.Package> _addr_pkg, types.Type recvType, @string name) {
    ref ssa.Package pkg = ref _addr_pkg.val;
 
    // TODO(adonovan): fix: hack: currently the only part of Signature
    // that is needed is the "pointerness" of Recv.Type, and for
    // now, we'll set it to always be false since we're only
    // concerned with rtype.  Encapsulate this better.
    var sig = types.NewSignature(types.NewVar(token.NoPos, null, "recv", recvType), null, null, false);
    var fn = pkg.Prog.NewFunction(name, sig, "fake reflect method");
    fn.Pkg = pkg;
    return _addr_fn!;

}

private static void initReflect(ptr<interpreter> _addr_i) {
    ref interpreter i = ref _addr_i.val;

    i.reflectPackage = addr(new ssa.Package(Prog:i.prog,Pkg:reflectTypesPackage,Members:make(map[string]ssa.Member),)); 

    // Clobber the type-checker's notion of reflect.Value's
    // underlying type so that it more closely matches the fake one
    // (at least in the number of fields---we lie about the type of
    // the rtype field).
    //
    // We must ensure that calls to (ssa.Value).Type() return the
    // fake type so that correct "shape" is used when allocating
    // variables, making zero values, loading, and storing.
    //
    // TODO(adonovan): obviously this is a hack.  We need a cleaner
    // way to fake the reflect package (almost---DeepEqual is fine).
    // One approach would be not to even load its source code, but
    // provide fake source files.  This would guarantee that no bad
    // information leaks into other packages.
    {
        var r = i.prog.ImportedPackage("reflect");

        if (r != null) {
            ptr<types.Named> rV = r.Pkg.Scope().Lookup("Value").Type()._<ptr<types.Named>>(); 

            // delete bodies of the old methods
            var mset = i.prog.MethodSets.MethodSet(rV);
            for (nint j = 0; j < mset.Len(); j++) {
                i.prog.MethodValue(mset.At(j)).Blocks;

                null;

            }


            var tEface = types.NewInterface(null, null).Complete();
            rV.SetUnderlying(types.NewStruct(new slice<ptr<types.Var>>(new ptr<types.Var>[] { types.NewField(token.NoPos,r.Pkg,"t",tEface,false), types.NewField(token.NoPos,r.Pkg,"v",tEface,false) }), null));

        }
    }


    i.rtypeMethods = new methodSet("Bits":newMethod(i.reflectPackage,rtypeType,"Bits"),"Elem":newMethod(i.reflectPackage,rtypeType,"Elem"),"Field":newMethod(i.reflectPackage,rtypeType,"Field"),"In":newMethod(i.reflectPackage,rtypeType,"In"),"Kind":newMethod(i.reflectPackage,rtypeType,"Kind"),"NumField":newMethod(i.reflectPackage,rtypeType,"NumField"),"NumIn":newMethod(i.reflectPackage,rtypeType,"NumIn"),"NumMethod":newMethod(i.reflectPackage,rtypeType,"NumMethod"),"NumOut":newMethod(i.reflectPackage,rtypeType,"NumOut"),"Out":newMethod(i.reflectPackage,rtypeType,"Out"),"Size":newMethod(i.reflectPackage,rtypeType,"Size"),"String":newMethod(i.reflectPackage,rtypeType,"String"),);
    i.errorMethods = new methodSet("Error":newMethod(i.reflectPackage,errorType,"Error"),);

}

} // end interp_package
