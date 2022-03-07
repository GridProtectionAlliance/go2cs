// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package interp -- go2cs converted at 2022 March 06 23:33:40 UTC
// import "golang.org/x/tools/go/ssa/interp" ==> using interp = go.golang.org.x.tools.go.ssa.interp_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\value.go
// Values
//
// All interpreter values are "boxed" in the empty interface, value.
// The range of possible dynamic types within value are:
//
// - bool
// - numbers (all built-in int/float/complex types are distinguished)
// - string
// - map[value]value --- maps for which  usesBuiltinMap(keyType)
//   *hashmap        --- maps for which !usesBuiltinMap(keyType)
// - chan value
// - []value --- slices
// - iface --- interfaces.
// - structure --- structs.  Fields are ordered and accessed by numeric indices.
// - array --- arrays.
// - *value --- pointers.  Careful: *value is a distinct type from *array etc.
// - *ssa.Function \
//   *ssa.Builtin   } --- functions.  A nil 'func' is always of type *ssa.Function.
//   *closure      /
// - tuple --- as returned by Return, Next, "value,ok" modes, etc.
// - iter --- iterators from 'range' over map or string.
// - bad --- a poison pill for locals that have gone out of scope.
// - rtype -- the interpreter's concrete implementation of reflect.Type
//
// Note that nil is not on this list.
//
// Pay close attention to whether or not the dynamic type is a pointer.
// The compiler cannot help you since value is an empty interface.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using io = go.io_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;

using ssa = go.golang.org.x.tools.go.ssa_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;

namespace go.golang.org.x.tools.go.ssa;

public static partial class interp_package {

private partial interface value {
}

private partial struct tuple { // : slice<value>
}

private partial struct array { // : slice<value>
}

private partial struct iface {
    public types.Type t; // never an "untyped" type
    public value v;
}

private partial struct structure { // : slice<value>
}

// For map, array, *array, slice, string or channel.
private partial interface iter {
    tuple next();
}

private partial struct closure {
    public ptr<ssa.Function> Fn;
    public slice<value> Env;
}

private partial struct bad {
}

private partial struct rtype {
    public types.Type t;
}

// Hash functions and equivalence relation:

// hashString computes the FNV hash of s.
private static nint hashString(@string s) {
    uint h = default;
    for (nint i = 0; i < len(s); i++) {
        h ^= uint32(s[i]);
        h *= 16777619;
    }
    return int(h);
}

private static sync.Mutex mu = default;private static var hasher = typeutil.MakeHasher();

// hashType returns a hash for t such that
// types.Identical(x, y) => hashType(x) == hashType(y).
private static nint hashType(types.Type t) {
    mu.Lock();
    var h = int(hasher.Hash(t));
    mu.Unlock();
    return h;
}

// usesBuiltinMap returns true if the built-in hash function and
// equivalence relation for type t are consistent with those of the
// interpreter's representation of type t.  Such types are: all basic
// types (bool, numbers, string), pointers and channels.
//
// usesBuiltinMap returns false for types that require a custom map
// implementation: interfaces, arrays and structs.
//
// Panic ensues if t is an invalid map key type: function, map or slice.
private static bool usesBuiltinMap(types.Type t) => func((_, panic, _) => {
    switch (t.type()) {
        case ptr<types.Basic> t:
            return true;
            break;
        case ptr<types.Chan> t:
            return true;
            break;
        case ptr<types.Pointer> t:
            return true;
            break;
        case ptr<types.Named> t:
            return usesBuiltinMap(t.Underlying());
            break;
        case ptr<types.Interface> t:
            return false;
            break;
        case ptr<types.Array> t:
            return false;
            break;
        case ptr<types.Struct> t:
            return false;
            break;
    }
    panic(fmt.Sprintf("invalid map key type: %T", t));

});

private static bool eq(this array x, types.Type t, object _y) {
    array y = _y._<array>();
    ptr<types.Array> tElt = t.Underlying()._<ptr<types.Array>>().Elem();
    foreach (var (i, xi) in x) {
        if (!equals(tElt, xi, y[i])) {
            return false;
        }
    }    return true;

}

private static nint hash(this array x, types.Type t) {
    nint h = 0;
    ptr<types.Array> tElt = t.Underlying()._<ptr<types.Array>>().Elem();
    foreach (var (_, xi) in x) {
        h += hash(tElt, xi);
    }    return h;
}

private static bool eq(this structure x, types.Type t, object _y) {
    structure y = _y._<structure>();
    ptr<types.Struct> tStruct = t.Underlying()._<ptr<types.Struct>>();
    for (nint i = 0;
    var n = tStruct.NumFields(); i < n; i++) {
        {
            var f = tStruct.Field(i);

            if (!f.Anonymous()) {
                if (!equals(f.Type(), x[i], y[i])) {
                    return false;
                }
            }

        }

    }
    return true;

}

private static nint hash(this structure x, types.Type t) {
    ptr<types.Struct> tStruct = t.Underlying()._<ptr<types.Struct>>();
    nint h = 0;
    for (nint i = 0;
    var n = tStruct.NumFields(); i < n; i++) {
        {
            var f = tStruct.Field(i);

            if (!f.Anonymous()) {
                h += hash(f.Type(), x[i]);
            }

        }

    }
    return h;

}

// nil-tolerant variant of types.Identical.
private static bool sameType(types.Type x, types.Type y) {
    if (x == null) {
        return y == null;
    }
    return y != null && types.Identical(x, y);

}

private static bool eq(this iface x, types.Type t, object _y) {
    iface y = _y._<iface>();
    return sameType(x.t, y.t) && (x.t == null || equals(x.t, x.v, y.v));
}

private static nint hash(this iface x, types.Type _) {
    return hashType(x.t) * 8581 + hash(x.t, x.v);
}

private static nint hash(this rtype x, types.Type _) {
    return hashType(x.t);
}

private static bool eq(this rtype x, types.Type _, object y) {
    return types.Identical(x.t, y._<rtype>().t);
}

// equals returns true iff x and y are equal according to Go's
// linguistic equivalence relation for type t.
// In a well-typed program, the dynamic types of x and y are
// guaranteed equal.
private static bool equals(types.Type t, value x, value y) => func((_, panic, _) => {
    switch (x.type()) {
        case bool x:
            return x == y._<bool>();
            break;
        case nint x:
            return x == y._<nint>();
            break;
        case sbyte x:
            return x == y._<sbyte>();
            break;
        case short x:
            return x == y._<short>();
            break;
        case int x:
            return x == y._<int>();
            break;
        case long x:
            return x == y._<long>();
            break;
        case nuint x:
            return x == y._<nuint>();
            break;
        case byte x:
            return x == y._<byte>();
            break;
        case ushort x:
            return x == y._<ushort>();
            break;
        case uint x:
            return x == y._<uint>();
            break;
        case ulong x:
            return x == y._<ulong>();
            break;
        case System.UIntPtr x:
            return x == y._<System.UIntPtr>();
            break;
        case float x:
            return x == y._<float>();
            break;
        case double x:
            return x == y._<double>();
            break;
        case complex64 x:
            return x == y._<complex64>();
            break;
        case System.Numerics.Complex128 x:
            return x == y._<System.Numerics.Complex128>();
            break;
        case @string x:
            return x == y._<@string>();
            break;
        case ptr<value> x:
            return x == y._<ptr<value>>();
            break;
        case channel<value> x:
            return x == y._<channel<value>>();
            break;
        case structure x:
            return x.eq(t, y);
            break;
        case array x:
            return x.eq(t, y);
            break;
        case iface x:
            return x.eq(t, y);
            break;
        case rtype x:
            return x.eq(t, y);
            break; 

        // Since map, func and slice don't support comparison, this
        // case is only reachable if one of x or y is literally nil
        // (handled in eqnil) or via interface{} values.
    } 

    // Since map, func and slice don't support comparison, this
    // case is only reachable if one of x or y is literally nil
    // (handled in eqnil) or via interface{} values.
    panic(fmt.Sprintf("comparing uncomparable type %s", t));

});

// Returns an integer hash of x such that equals(x, y) => hash(x) == hash(y).
private static nint hash(types.Type t, value x) => func((_, panic, _) => {
    switch (x.type()) {
        case bool x:
            if (x) {
                return 1;
            }
            return 0;
            break;
        case nint x:
            return x;
            break;
        case sbyte x:
            return int(x);
            break;
        case short x:
            return int(x);
            break;
        case int x:
            return int(x);
            break;
        case long x:
            return int(x);
            break;
        case nuint x:
            return int(x);
            break;
        case byte x:
            return int(x);
            break;
        case ushort x:
            return int(x);
            break;
        case uint x:
            return int(x);
            break;
        case ulong x:
            return int(x);
            break;
        case System.UIntPtr x:
            return int(x);
            break;
        case float x:
            return int(x);
            break;
        case double x:
            return int(x);
            break;
        case complex64 x:
            return int(real(x));
            break;
        case System.Numerics.Complex128 x:
            return int(real(x));
            break;
        case @string x:
            return hashString(x);
            break;
        case ptr<value> x:
            return int(uintptr(@unsafe.Pointer(x)));
            break;
        case channel<value> x:
            return int(uintptr(reflect.ValueOf(x).Pointer()));
            break;
        case structure x:
            return x.hash(t);
            break;
        case array x:
            return x.hash(t);
            break;
        case iface x:
            return x.hash(t);
            break;
        case rtype x:
            return x.hash(t);
            break;
    }
    panic(fmt.Sprintf("%T is unhashable", x));

});

// reflect.Value struct values don't have a fixed shape, since the
// payload can be a scalar or an aggregate depending on the instance.
// So store (and load) can't simply use recursion over the shape of the
// rhs value, or the lhs, to copy the value; we need the static type
// information.  (We can't make reflect.Value a new basic data type
// because its "structness" is exposed to Go programs.)

// load returns the value of type T in *addr.
private static value load(types.Type T, ptr<value> _addr_addr) {
    ref value addr = ref _addr_addr.val;

    switch (T.Underlying().type()) {
        case ptr<types.Struct> T:
            structure v = (addr)._<structure>();
            var a = make(structure, len(v));
            {
                var i__prev1 = i;

                foreach (var (__i) in a) {
                    i = __i;
                    a[i] = load(T.Field(i).Type(), _addr_v[i]);
                }

                i = i__prev1;
            }

            return a;
            break;
        case ptr<types.Array> T:
            v = (addr)._<array>();
            a = make(array, len(v));
            {
                var i__prev1 = i;

                foreach (var (__i) in a) {
                    i = __i;
                    a[i] = load(T.Elem(), _addr_v[i]);
                }

                i = i__prev1;
            }

            return a;
            break;
        default:
        {
            var T = T.Underlying().type();
            return addr;
            break;
        }
    }

}

// store stores value v of type T into *addr.
private static void store(types.Type T, ptr<value> _addr_addr, value v) {
    ref value addr = ref _addr_addr.val;

    switch (T.Underlying().type()) {
        case ptr<types.Struct> T:
            structure lhs = (addr)._<structure>();
            structure rhs = v._<structure>();
            {
                var i__prev1 = i;

                foreach (var (__i) in lhs) {
                    i = __i;
                    store(T.Field(i).Type(), _addr_lhs[i], rhs[i]);
                }

                i = i__prev1;
            }
            break;
        case ptr<types.Array> T:
            lhs = (addr)._<array>();
            rhs = v._<array>();
            {
                var i__prev1 = i;

                foreach (var (__i) in lhs) {
                    i = __i;
                    store(T.Elem(), _addr_lhs[i], rhs[i]);
                }

                i = i__prev1;
            }
            break;
        default:
        {
            var T = T.Underlying().type();
            addr = v;
            break;
        }
    }

}

// Prints in the style of built-in println.
// (More or less; in gc println is actually a compiler intrinsic and
// can distinguish println(1) from println(interface{}(1)).)
private static void writeValue(ptr<bytes.Buffer> _addr_buf, value v) {
    ref bytes.Buffer buf = ref _addr_buf.val;

    switch (v.type()) {
        case bool v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case nint v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case sbyte v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case short v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case int v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case long v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case nuint v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case byte v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case ushort v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case uint v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case ulong v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case System.UIntPtr v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case float v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case double v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case complex64 v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case System.Numerics.Complex128 v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case @string v:
            fmt.Fprintf(buf, "%v", v);
            break;
        case map<value, value> v:
            buf.WriteString("map[");
            @string sep = "";
            {
                var e__prev1 = e;

                foreach (var (__k, __e) in v) {
                    k = __k;
                    e = __e;
                    buf.WriteString(sep);
                    sep = " ";
                    writeValue(_addr_buf, k);
                    buf.WriteString(":");
                    writeValue(_addr_buf, e);
                }

                e = e__prev1;
            }

            buf.WriteString("]");
            break;
        case ptr<hashmap> v:
            buf.WriteString("map[");
            sep = " ";
            {
                var e__prev1 = e;

                foreach (var (_, __e) in v.entries()) {
                    e = __e;
                    while (e != null) {
                        buf.WriteString(sep);
                        sep = " ";
                        writeValue(_addr_buf, e.key);
                        buf.WriteString(":");
                        writeValue(_addr_buf, e.value);
                        e = e.next;
                    }
                }

                e = e__prev1;
            }

            buf.WriteString("]");
            break;
        case channel<value> v:
            fmt.Fprintf(buf, "%v", v); // (an address)
            break;
        case ptr<value> v:
            if (v == null) {
                buf.WriteString("<nil>");
            }
            else
 {
                fmt.Fprintf(buf, "%p", v);
            }

            break;
        case iface v:
            fmt.Fprintf(buf, "(%s, ", v.t);
            writeValue(_addr_buf, v.v);
            buf.WriteString(")");
            break;
        case structure v:
            buf.WriteString("{");
            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in v) {
                    i = __i;
                    e = __e;
                    if (i > 0) {
                        buf.WriteString(" ");
                    }
                    writeValue(_addr_buf, e);
                }

                i = i__prev1;
                e = e__prev1;
            }

            buf.WriteString("}");
            break;
        case array v:
            buf.WriteString("[");
            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in v) {
                    i = __i;
                    e = __e;
                    if (i > 0) {
                        buf.WriteString(" ");
                    }
                    writeValue(_addr_buf, e);
                }

                i = i__prev1;
                e = e__prev1;
            }

            buf.WriteString("]");
            break;
        case slice<value> v:
            buf.WriteString("[");
            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in v) {
                    i = __i;
                    e = __e;
                    if (i > 0) {
                        buf.WriteString(" ");
                    }
                    writeValue(_addr_buf, e);
                }

                i = i__prev1;
                e = e__prev1;
            }

            buf.WriteString("]");
            break;
        case ptr<ssa.Function> v:
            fmt.Fprintf(buf, "%p", v); // (an address)
            break;
        case ptr<ssa.Builtin> v:
            fmt.Fprintf(buf, "%p", v); // (an address)
            break;
        case ptr<closure> v:
            fmt.Fprintf(buf, "%p", v); // (an address)
            break;
        case rtype v:
            buf.WriteString(v.t.String());
            break;
        case tuple v:
            buf.WriteString("(");
            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in v) {
                    i = __i;
                    e = __e;
                    if (i > 0) {
                        buf.WriteString(", ");
                    }
                    writeValue(_addr_buf, e);
                }

                i = i__prev1;
                e = e__prev1;
            }

            buf.WriteString(")");
            break;
        default:
        {
            var v = v.type();
            fmt.Fprintf(buf, "<%T>", v);
            break;
        }
    }

}

// Implements printing of Go values in the style of built-in println.
private static @string toString(value v) {
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    writeValue(_addr_b, v);
    return b.String();
}

// ------------------------------------------------------------------------
// Iterators

private partial struct stringIter {
    public ref ptr<strings.Reader> Reader> => ref Reader>_ptr;
    public nint i;
}

private static tuple next(this ptr<stringIter> _addr_it) {
    ref stringIter it = ref _addr_it.val;

    var okv = make(tuple, 3);
    var (ch, n, err) = it.ReadRune();
    var ok = err != io.EOF;
    okv[0] = ok;
    if (ok) {
        okv[1] = it.i;
        okv[2] = ch;
    }
    it.i += n;
    return okv;

}

private partial struct mapIter {
    public ptr<reflect.MapIter> iter;
    public bool ok;
}

private static tuple next(this ptr<mapIter> _addr_it) {
    ref mapIter it = ref _addr_it.val;

    it.ok = it.iter.Next();
    if (!it.ok) {
        return new slice<value>(new value[] { value.As(false)!, value.As(nil)!, value.As(nil)! });
    }
    var k = it.iter.Key().Interface();
    var v = it.iter.Value().Interface();
    return new slice<value>(new value[] { value.As(true)!, value.As(k)!, value.As(v)! });

}

private partial struct hashmapIter {
    public ptr<reflect.MapIter> iter;
    public bool ok;
    public ptr<entry> cur;
}

private static tuple next(this ptr<hashmapIter> _addr_it) {
    ref hashmapIter it = ref _addr_it.val;

    while (true) {
        if (it.cur != null) {
            var k = it.cur.key;
            var v = it.cur.value;
            it.cur = it.cur.next;
            return new slice<value>(new value[] { value.As(true)!, value.As(k)!, value.As(v)! });

        }
        it.ok = it.iter.Next();
        if (!it.ok) {
            return new slice<value>(new value[] { value.As(false)!, value.As(nil)!, value.As(nil)! });
        }
        it.cur = it.iter.Value().Interface()._<ptr<entry>>();

    }

}

} // end interp_package
