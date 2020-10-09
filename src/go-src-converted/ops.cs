// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package interp -- go2cs converted at 2020 October 09 06:03:38 UTC
// import "golang.org/x/tools/go/ssa/interp" ==> using interp = go.golang.org.x.tools.go.ssa.interp_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\ops.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;

using ssa = go.golang.org.x.tools.go.ssa_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ssa
{
    public static partial class interp_package
    {
        // If the target program panics, the interpreter panics with this type.
        private partial struct targetPanic
        {
            public value v;
        }

        private static @string String(this targetPanic p)
        {
            return toString(p.v);
        }

        // If the target program calls exit, the interpreter panics with this type.
        private partial struct exitPanic // : long
        {
        }

        // constValue returns the value of the constant with the
        // dynamic type tag appropriate for c.Type().
        private static value constValue(ptr<ssa.Const> _addr_c) => func((_, panic, __) =>
        {
            ref ssa.Const c = ref _addr_c.val;

            if (c.IsNil())
            {
                return zero(c.Type()); // typed nil
            }

            {
                ptr<types.Basic> (t, ok) = c.Type().Underlying()._<ptr<types.Basic>>();

                if (ok)
                { 
                    // TODO(adonovan): eliminate untyped constants from SSA form.

                    if (t.Kind() == types.Bool || t.Kind() == types.UntypedBool) 
                        return constant.BoolVal(c.Value);
                    else if (t.Kind() == types.Int || t.Kind() == types.UntypedInt) 
                        // Assume sizeof(int) is same on host and target.
                        return int(c.Int64());
                    else if (t.Kind() == types.Int8) 
                        return int8(c.Int64());
                    else if (t.Kind() == types.Int16) 
                        return int16(c.Int64());
                    else if (t.Kind() == types.Int32 || t.Kind() == types.UntypedRune) 
                        return int32(c.Int64());
                    else if (t.Kind() == types.Int64) 
                        return c.Int64();
                    else if (t.Kind() == types.Uint) 
                        // Assume sizeof(uint) is same on host and target.
                        return uint(c.Uint64());
                    else if (t.Kind() == types.Uint8) 
                        return uint8(c.Uint64());
                    else if (t.Kind() == types.Uint16) 
                        return uint16(c.Uint64());
                    else if (t.Kind() == types.Uint32) 
                        return uint32(c.Uint64());
                    else if (t.Kind() == types.Uint64) 
                        return c.Uint64();
                    else if (t.Kind() == types.Uintptr) 
                        // Assume sizeof(uintptr) is same on host and target.
                        return uintptr(c.Uint64());
                    else if (t.Kind() == types.Float32) 
                        return float32(c.Float64());
                    else if (t.Kind() == types.Float64 || t.Kind() == types.UntypedFloat) 
                        return c.Float64();
                    else if (t.Kind() == types.Complex64) 
                        return complex64(c.Complex128());
                    else if (t.Kind() == types.Complex128 || t.Kind() == types.UntypedComplex) 
                        return c.Complex128();
                    else if (t.Kind() == types.String || t.Kind() == types.UntypedString) 
                        if (c.Value.Kind() == constant.String)
                        {
                            return constant.StringVal(c.Value);
                        }

                        return string(rune(c.Int64()));
                    
                }

            }


            panic(fmt.Sprintf("constValue: %s", c));

        });

        // asInt converts x, which must be an integer, to an int suitable for
        // use as a slice or array index or operand to make().
        private static long asInt(value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case long x:
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
                case ulong x:
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
            }
            panic(fmt.Sprintf("cannot convert %T to int", x));

        });

        // asUint64 converts x, which must be an unsigned integer, to a uint64
        // suitable for use as a bitwise shift count.
        private static ulong asUint64(value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case ulong x:
                    return uint64(x);
                    break;
                case byte x:
                    return uint64(x);
                    break;
                case ushort x:
                    return uint64(x);
                    break;
                case uint x:
                    return uint64(x);
                    break;
                case ulong x:
                    return x;
                    break;
                case System.UIntPtr x:
                    return uint64(x);
                    break;
            }
            panic(fmt.Sprintf("cannot convert %T to uint64", x));

        });

        // zero returns a new "zero" value of the specified type.
        private static value zero(types.Type t) => func((_, panic, __) =>
        {
            switch (t.type())
            {
                case ptr<types.Basic> t:
                    if (t.Kind() == types.UntypedNil)
                    {
                        panic("untyped nil has no zero value");
                    }

                    if (t.Info() & types.IsUntyped != 0L)
                    { 
                        // TODO(adonovan): make it an invariant that
                        // this is unreachable.  Currently some
                        // constants have 'untyped' types when they
                        // should be defaulted by the typechecker.
                        t = types.Default(t)._<ptr<types.Basic>>();

                    }


                    if (t.Kind() == types.Bool) 
                        return false;
                    else if (t.Kind() == types.Int) 
                        return int(0L);
                    else if (t.Kind() == types.Int8) 
                        return int8(0L);
                    else if (t.Kind() == types.Int16) 
                        return int16(0L);
                    else if (t.Kind() == types.Int32) 
                        return int32(0L);
                    else if (t.Kind() == types.Int64) 
                        return int64(0L);
                    else if (t.Kind() == types.Uint) 
                        return uint(0L);
                    else if (t.Kind() == types.Uint8) 
                        return uint8(0L);
                    else if (t.Kind() == types.Uint16) 
                        return uint16(0L);
                    else if (t.Kind() == types.Uint32) 
                        return uint32(0L);
                    else if (t.Kind() == types.Uint64) 
                        return uint64(0L);
                    else if (t.Kind() == types.Uintptr) 
                        return uintptr(0L);
                    else if (t.Kind() == types.Float32) 
                        return float32(0L);
                    else if (t.Kind() == types.Float64) 
                        return float64(0L);
                    else if (t.Kind() == types.Complex64) 
                        return complex64(0L);
                    else if (t.Kind() == types.Complex128) 
                        return complex128(0L);
                    else if (t.Kind() == types.String) 
                        return "";
                    else if (t.Kind() == types.UnsafePointer) 
                        return @unsafe.Pointer(null);
                    else 
                        panic(fmt.Sprint("zero for unexpected type:", t));
                                        break;
                case ptr<types.Pointer> t:
                    return (value.val)(null);
                    break;
                case ptr<types.Array> t:
                    var a = make(array, t.Len());
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in a)
                        {
                            i = __i;
                            a[i] = zero(t.Elem());
                        }

                        i = i__prev1;
                    }

                    return a;
                    break;
                case ptr<types.Named> t:
                    return zero(t.Underlying());
                    break;
                case ptr<types.Interface> t:
                    return new iface(); // nil type, methodset and value
                    break;
                case ptr<types.Slice> t:
                    return (slice<value>)null;
                    break;
                case ptr<types.Struct> t:
                    var s = make(structure, t.NumFields());
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in s)
                        {
                            i = __i;
                            s[i] = zero(t.Field(i).Type());
                        }

                        i = i__prev1;
                    }

                    return s;
                    break;
                case ptr<types.Tuple> t:
                    if (t.Len() == 1L)
                    {
                        return zero(t.At(0L).Type());
                    }

                    s = make(tuple, t.Len());
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in s)
                        {
                            i = __i;
                            s[i] = zero(t.At(i).Type());
                        }

                        i = i__prev1;
                    }

                    return s;
                    break;
                case ptr<types.Chan> t:
                    return (channel<value>)null;
                    break;
                case ptr<types.Map> t:
                    if (usesBuiltinMap(t.Key()))
                    {
                        return (map<value, value>)null;
                    }

                    return (hashmap.val)(null);
                    break;
                case ptr<types.Signature> t:
                    return (ssa.Function.val)(null);
                    break;
            }
            panic(fmt.Sprint("zero: unexpected ", t));

        });

        // slice returns x[lo:hi:max].  Any of lo, hi and max may be nil.
        private static value slice(value x, value lo, value hi, value max) => func((_, panic, __) =>
        {
            long Len = default;            long Cap = default;

            switch (x.type())
            {
                case @string x:
                    Len = len(x);
                    break;
                case slice<value> x:
                    Len = len(x);
                    Cap = cap(x);
                    break;
                case ptr<value> x:
                    array a = (x.val)._<array>();
                    Len = len(a);
                    Cap = cap(a);
                    break;

            }

            long l = 0L;
            if (lo != null)
            {
                l = asInt(lo);
            }

            var h = Len;
            if (hi != null)
            {
                h = asInt(hi);
            }

            var m = Cap;
            if (max != null)
            {
                m = asInt(max);
            }

            switch (x.type())
            {
                case @string x:
                    return x[l..h];
                    break;
                case slice<value> x:
                    return x.slice(l, h, m);
                    break;
                case ptr<value> x:
                    a = (x.val)._<array>();
                    return (slice<value>)a.slice(l, h, m);
                    break;
            }
            panic(fmt.Sprintf("slice: unexpected X type: %T", x));

        });

        // lookup returns x[idx] where x is a map or string.
        private static value lookup(ptr<ssa.Lookup> _addr_instr, value x, value idx) => func((_, panic, __) =>
        {
            ref ssa.Lookup instr = ref _addr_instr.val;

            switch (x.type())
            {
                case map<value, value> x:
                    value v = default;
                    bool ok = default;
                    switch (x.type())
                    {
                        case map<value, value> x:
                            v, ok = x[idx];
                            break;
                        case ptr<hashmap> x:
                            v = x.lookup(idx._<hashable>());
                            ok = v != null;
                            break;
                    }
                    if (!ok)
                    {
                        v = zero(instr.X.Type().Underlying()._<ptr<types.Map>>().Elem());
                    }

                    if (instr.CommaOk)
                    {
                        v = new tuple(v,ok);
                    }

                    return v;
                    break;
                case ptr<hashmap> x:
                    value v = default;
                    bool ok = default;
                    switch (x.type())
                    {
                        case map<value, value> x:
                            v, ok = x[idx];
                            break;
                        case ptr<hashmap> x:
                            v = x.lookup(idx._<hashable>());
                            ok = v != null;
                            break;
                    }
                    if (!ok)
                    {
                        v = zero(instr.X.Type().Underlying()._<ptr<types.Map>>().Elem());
                    }

                    if (instr.CommaOk)
                    {
                        v = new tuple(v,ok);
                    }

                    return v;
                    break;
                case @string x:
                    return x[asInt(idx)];
                    break;
            }
            panic(fmt.Sprintf("unexpected x type in Lookup: %T", x));

        });

        // binop implements all arithmetic and logical binary operators for
        // numeric datatypes and strings.  Both operands must have identical
        // dynamic type.
        //
        private static value binop(token.Token op, types.Type t, value x, value y) => func((_, panic, __) =>
        {

            if (op == token.ADD) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() + y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() + y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() + y._<short>();
                        break;
                    case int _:
                        return x._<int>() + y._<int>();
                        break;
                    case long _:
                        return x._<long>() + y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() + y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() + y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() + y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() + y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() + y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() + y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() + y._<float>();
                        break;
                    case double _:
                        return x._<double>() + y._<double>();
                        break;
                    case complex64 _:
                        return x._<complex64>() + y._<complex64>();
                        break;
                    case System.Numerics.Complex128 _:
                        return x._<System.Numerics.Complex128>() + y._<System.Numerics.Complex128>();
                        break;
                    case @string _:
                        return x._<@string>() + y._<@string>();
                        break;

                }
            else if (op == token.SUB) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() - y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() - y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() - y._<short>();
                        break;
                    case int _:
                        return x._<int>() - y._<int>();
                        break;
                    case long _:
                        return x._<long>() - y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() - y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() - y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() - y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() - y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() - y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() - y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() - y._<float>();
                        break;
                    case double _:
                        return x._<double>() - y._<double>();
                        break;
                    case complex64 _:
                        return x._<complex64>() - y._<complex64>();
                        break;
                    case System.Numerics.Complex128 _:
                        return x._<System.Numerics.Complex128>() - y._<System.Numerics.Complex128>();
                        break;

                }
            else if (op == token.MUL) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() * y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() * y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() * y._<short>();
                        break;
                    case int _:
                        return x._<int>() * y._<int>();
                        break;
                    case long _:
                        return x._<long>() * y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() * y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() * y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() * y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() * y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() * y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() * y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() * y._<float>();
                        break;
                    case double _:
                        return x._<double>() * y._<double>();
                        break;
                    case complex64 _:
                        return x._<complex64>() * y._<complex64>();
                        break;
                    case System.Numerics.Complex128 _:
                        return x._<System.Numerics.Complex128>() * y._<System.Numerics.Complex128>();
                        break;

                }
            else if (op == token.QUO) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() / y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() / y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() / y._<short>();
                        break;
                    case int _:
                        return x._<int>() / y._<int>();
                        break;
                    case long _:
                        return x._<long>() / y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() / y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() / y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() / y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() / y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() / y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() / y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() / y._<float>();
                        break;
                    case double _:
                        return x._<double>() / y._<double>();
                        break;
                    case complex64 _:
                        return x._<complex64>() / y._<complex64>();
                        break;
                    case System.Numerics.Complex128 _:
                        return x._<System.Numerics.Complex128>() / y._<System.Numerics.Complex128>();
                        break;

                }
            else if (op == token.REM) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() % y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() % y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() % y._<short>();
                        break;
                    case int _:
                        return x._<int>() % y._<int>();
                        break;
                    case long _:
                        return x._<long>() % y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() % y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() % y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() % y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() % y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() % y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() % y._<System.UIntPtr>();
                        break;

                }
            else if (op == token.AND) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() & y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() & y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() & y._<short>();
                        break;
                    case int _:
                        return x._<int>() & y._<int>();
                        break;
                    case long _:
                        return x._<long>() & y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() & y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() & y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() & y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() & y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() & y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() & y._<System.UIntPtr>();
                        break;

                }
            else if (op == token.OR) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() | y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() | y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() | y._<short>();
                        break;
                    case int _:
                        return x._<int>() | y._<int>();
                        break;
                    case long _:
                        return x._<long>() | y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() | y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() | y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() | y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() | y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() | y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() | y._<System.UIntPtr>();
                        break;

                }
            else if (op == token.XOR) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() ^ y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() ^ y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() ^ y._<short>();
                        break;
                    case int _:
                        return x._<int>() ^ y._<int>();
                        break;
                    case long _:
                        return x._<long>() ^ y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() ^ y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() ^ y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() ^ y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() ^ y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() ^ y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() ^ y._<System.UIntPtr>();
                        break;

                }
            else if (op == token.AND_NOT) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() & ~y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() & ~y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() & ~y._<short>();
                        break;
                    case int _:
                        return x._<int>() & ~y._<int>();
                        break;
                    case long _:
                        return x._<long>() & ~y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() & ~y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() & ~y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() & ~y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() & ~y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() & ~y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() & ~y._<System.UIntPtr>();
                        break;

                }
            else if (op == token.SHL) 
                var y = asUint64(y);
                switch (x.type())
                {
                    case long _:
                        return x._<long>() << (int)(y);
                        break;
                    case sbyte _:
                        return x._<sbyte>() << (int)(y);
                        break;
                    case short _:
                        return x._<short>() << (int)(y);
                        break;
                    case int _:
                        return x._<int>() << (int)(y);
                        break;
                    case long _:
                        return x._<long>() << (int)(y);
                        break;
                    case ulong _:
                        return x._<ulong>() << (int)(y);
                        break;
                    case byte _:
                        return x._<byte>() << (int)(y);
                        break;
                    case ushort _:
                        return x._<ushort>() << (int)(y);
                        break;
                    case uint _:
                        return x._<uint>() << (int)(y);
                        break;
                    case ulong _:
                        return x._<ulong>() << (int)(y);
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() << (int)(y);
                        break;

                }
            else if (op == token.SHR) 
                y = asUint64(y);
                switch (x.type())
                {
                    case long _:
                        return x._<long>() >> (int)(y);
                        break;
                    case sbyte _:
                        return x._<sbyte>() >> (int)(y);
                        break;
                    case short _:
                        return x._<short>() >> (int)(y);
                        break;
                    case int _:
                        return x._<int>() >> (int)(y);
                        break;
                    case long _:
                        return x._<long>() >> (int)(y);
                        break;
                    case ulong _:
                        return x._<ulong>() >> (int)(y);
                        break;
                    case byte _:
                        return x._<byte>() >> (int)(y);
                        break;
                    case ushort _:
                        return x._<ushort>() >> (int)(y);
                        break;
                    case uint _:
                        return x._<uint>() >> (int)(y);
                        break;
                    case ulong _:
                        return x._<ulong>() >> (int)(y);
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() >> (int)(y);
                        break;

                }
            else if (op == token.LSS) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() < y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() < y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() < y._<short>();
                        break;
                    case int _:
                        return x._<int>() < y._<int>();
                        break;
                    case long _:
                        return x._<long>() < y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() < y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() < y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() < y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() < y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() < y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() < y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() < y._<float>();
                        break;
                    case double _:
                        return x._<double>() < y._<double>();
                        break;
                    case @string _:
                        return x._<@string>() < y._<@string>();
                        break;

                }
            else if (op == token.LEQ) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() <= y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() <= y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() <= y._<short>();
                        break;
                    case int _:
                        return x._<int>() <= y._<int>();
                        break;
                    case long _:
                        return x._<long>() <= y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() <= y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() <= y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() <= y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() <= y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() <= y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() <= y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() <= y._<float>();
                        break;
                    case double _:
                        return x._<double>() <= y._<double>();
                        break;
                    case @string _:
                        return x._<@string>() <= y._<@string>();
                        break;

                }
            else if (op == token.EQL) 
                return eqnil(t, x, y);
            else if (op == token.NEQ) 
                return !eqnil(t, x, y);
            else if (op == token.GTR) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() > y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() > y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() > y._<short>();
                        break;
                    case int _:
                        return x._<int>() > y._<int>();
                        break;
                    case long _:
                        return x._<long>() > y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() > y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() > y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() > y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() > y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() > y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() > y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() > y._<float>();
                        break;
                    case double _:
                        return x._<double>() > y._<double>();
                        break;
                    case @string _:
                        return x._<@string>() > y._<@string>();
                        break;

                }
            else if (op == token.GEQ) 
                switch (x.type())
                {
                    case long _:
                        return x._<long>() >= y._<long>();
                        break;
                    case sbyte _:
                        return x._<sbyte>() >= y._<sbyte>();
                        break;
                    case short _:
                        return x._<short>() >= y._<short>();
                        break;
                    case int _:
                        return x._<int>() >= y._<int>();
                        break;
                    case long _:
                        return x._<long>() >= y._<long>();
                        break;
                    case ulong _:
                        return x._<ulong>() >= y._<ulong>();
                        break;
                    case byte _:
                        return x._<byte>() >= y._<byte>();
                        break;
                    case ushort _:
                        return x._<ushort>() >= y._<ushort>();
                        break;
                    case uint _:
                        return x._<uint>() >= y._<uint>();
                        break;
                    case ulong _:
                        return x._<ulong>() >= y._<ulong>();
                        break;
                    case System.UIntPtr _:
                        return x._<System.UIntPtr>() >= y._<System.UIntPtr>();
                        break;
                    case float _:
                        return x._<float>() >= y._<float>();
                        break;
                    case double _:
                        return x._<double>() >= y._<double>();
                        break;
                    case @string _:
                        return x._<@string>() >= y._<@string>();
                        break;
                }
                        panic(fmt.Sprintf("invalid binary op: %T %s %T", x, op, y));

        });

        // eqnil returns the comparison x == y using the equivalence relation
        // appropriate for type t.
        // If t is a reference type, at most one of x or y may be a nil value
        // of that type.
        //
        private static bool eqnil(types.Type t, value x, value y) => func((_, panic, __) =>
        {
            switch (t.Underlying().type())
            {
                case ptr<types.Map> _:
                    switch (x.type())
                    {
                        case ptr<hashmap> x:
                            return (x != null) == (y._<ptr<hashmap>>() != null);
                            break;
                        case map<value, value> x:
                            return (x != null) == (y._<map<value, value>>() != null);
                            break;
                        case ptr<ssa.Function> x:
                            switch (y.type())
                            {
                                case ptr<ssa.Function> y:
                                    return (x != null) == (y != null);
                                    break;
                                case ptr<closure> y:
                                    return true;
                                    break;
                            }
                            break;
                        case ptr<closure> x:
                            return (x != null) == (y._<ptr<ssa.Function>>() != null);
                            break;
                        case slice<value> x:
                            return (x != null) == (y._<slice<value>>() != null);
                            break;
                    }
                    panic(fmt.Sprintf("eqnil(%s): illegal dynamic type: %T", t, x));
                    break;
                case ptr<types.Signature> _:
                    switch (x.type())
                    {
                        case ptr<hashmap> x:
                            return (x != null) == (y._<ptr<hashmap>>() != null);
                            break;
                        case map<value, value> x:
                            return (x != null) == (y._<map<value, value>>() != null);
                            break;
                        case ptr<ssa.Function> x:
                            switch (y.type())
                            {
                                case ptr<ssa.Function> y:
                                    return (x != null) == (y != null);
                                    break;
                                case ptr<closure> y:
                                    return true;
                                    break;
                            }
                            break;
                        case ptr<closure> x:
                            return (x != null) == (y._<ptr<ssa.Function>>() != null);
                            break;
                        case slice<value> x:
                            return (x != null) == (y._<slice<value>>() != null);
                            break;
                    }
                    panic(fmt.Sprintf("eqnil(%s): illegal dynamic type: %T", t, x));
                    break;
                case ptr<types.Slice> _:
                    switch (x.type())
                    {
                        case ptr<hashmap> x:
                            return (x != null) == (y._<ptr<hashmap>>() != null);
                            break;
                        case map<value, value> x:
                            return (x != null) == (y._<map<value, value>>() != null);
                            break;
                        case ptr<ssa.Function> x:
                            switch (y.type())
                            {
                                case ptr<ssa.Function> y:
                                    return (x != null) == (y != null);
                                    break;
                                case ptr<closure> y:
                                    return true;
                                    break;
                            }
                            break;
                        case ptr<closure> x:
                            return (x != null) == (y._<ptr<ssa.Function>>() != null);
                            break;
                        case slice<value> x:
                            return (x != null) == (y._<slice<value>>() != null);
                            break;
                    }
                    panic(fmt.Sprintf("eqnil(%s): illegal dynamic type: %T", t, x));
                    break;

            }

            return equals(t, x, y);

        });

        private static value unop(ptr<ssa.UnOp> _addr_instr, value x) => func((_, panic, __) =>
        {
            ref ssa.UnOp instr = ref _addr_instr.val;


            if (instr.Op == token.ARROW) // receive
                channel<value> (v, ok) = x._<channel<value>>().Receive();
                if (!ok)
                {
                    v = zero(instr.X.Type().Underlying()._<ptr<types.Chan>>().Elem());
                }

                if (instr.CommaOk)
                {
                    v = new tuple(v,ok);
                }

                return v;
            else if (instr.Op == token.SUB) 
                switch (x.type())
                {
                    case long x:
                        return -x;
                        break;
                    case sbyte x:
                        return -x;
                        break;
                    case short x:
                        return -x;
                        break;
                    case int x:
                        return -x;
                        break;
                    case long x:
                        return -x;
                        break;
                    case ulong x:
                        return -x;
                        break;
                    case byte x:
                        return -x;
                        break;
                    case ushort x:
                        return -x;
                        break;
                    case uint x:
                        return -x;
                        break;
                    case ulong x:
                        return -x;
                        break;
                    case System.UIntPtr x:
                        return -x;
                        break;
                    case float x:
                        return -x;
                        break;
                    case double x:
                        return -x;
                        break;
                    case complex64 x:
                        return -x;
                        break;
                    case System.Numerics.Complex128 x:
                        return -x;
                        break;
                }
            else if (instr.Op == token.MUL) 
                return load(deref(instr.X.Type()), x._<ptr<value>>());
            else if (instr.Op == token.NOT) 
                return !x._<bool>();
            else if (instr.Op == token.XOR) 
                switch (x.type())
                {
                    case long x:
                        return ~x;
                        break;
                    case sbyte x:
                        return ~x;
                        break;
                    case short x:
                        return ~x;
                        break;
                    case int x:
                        return ~x;
                        break;
                    case long x:
                        return ~x;
                        break;
                    case ulong x:
                        return ~x;
                        break;
                    case byte x:
                        return ~x;
                        break;
                    case ushort x:
                        return ~x;
                        break;
                    case uint x:
                        return ~x;
                        break;
                    case ulong x:
                        return ~x;
                        break;
                    case System.UIntPtr x:
                        return ~x;
                        break;
                }
                        panic(fmt.Sprintf("invalid unary op %s %T", instr.Op, x));

        });

        // typeAssert checks whether dynamic type of itf is instr.AssertedType.
        // It returns the extracted value on success, and panics on failure,
        // unless instr.CommaOk, in which case it always returns a "value,ok" tuple.
        //
        private static value typeAssert(ptr<interpreter> _addr_i, ptr<ssa.TypeAssert> _addr_instr, iface itf) => func((_, panic, __) =>
        {
            ref interpreter i = ref _addr_i.val;
            ref ssa.TypeAssert instr = ref _addr_instr.val;

            value v = default;
            @string err = "";
            if (itf.t == null)
            {
                err = fmt.Sprintf("interface conversion: interface is nil, not %s", instr.AssertedType);
            }            {
                ptr<types.Interface> (idst, ok) = instr.AssertedType.Underlying()._<ptr<types.Interface>>();


                else if (ok)
                {
                    v = itf;
                    err = checkInterface(_addr_i, idst, itf);
                }
                else if (types.Identical(itf.t, instr.AssertedType))
                {
                    v = itf.v; // extract value
                }
                else
                {
                    err = fmt.Sprintf("interface conversion: interface is %s, not %s", itf.t, instr.AssertedType);
                }


            }


            if (err != "")
            {
                if (!instr.CommaOk)
                {
                    panic(err);
                }

                return new tuple(zero(instr.AssertedType),false);

            }

            if (instr.CommaOk)
            {
                return new tuple(v,true);
            }

            return v;

        });

        // If CapturedOutput is non-nil, all writes by the interpreted program
        // to file descriptors 1 and 2 will also be written to CapturedOutput.
        //
        // (The $GOROOT/test system requires that the test be considered a
        // failure if "BUG" appears in the combined stdout/stderr output, even
        // if it exits zero.  This is a global variable shared by all
        // interpreters in the same process.)
        //
        public static ptr<bytes.Buffer> CapturedOutput;
        private static sync.Mutex capturedOutputMu = default;

        // write writes bytes b to the target program's standard output.
        // The print/println built-ins and the write() system call funnel
        // through here so they can be captured by the test driver.
        private static (long, error) print(slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;

            if (CapturedOutput != null)
            {
                capturedOutputMu.Lock();
                CapturedOutput.Write(b); // ignore errors
                capturedOutputMu.Unlock();

            }

            return os.Stdout.Write(b);

        }

        // callBuiltin interprets a call to builtin fn with arguments args,
        // returning its result.
        private static value callBuiltin(ptr<frame> _addr_caller, token.Pos callpos, ptr<ssa.Builtin> _addr_fn, slice<value> args) => func((_, panic, __) =>
        {
            ref frame caller = ref _addr_caller.val;
            ref ssa.Builtin fn = ref _addr_fn.val;

            switch (fn.Name())
            {
                case "append": 
                    if (len(args) == 1L)
                    {
                        return args[0L];
                    }

                    {
                        @string (s, ok) = args[1L]._<@string>();

                        if (ok)
                        { 
                            // append([]byte, ...string) []byte
                            slice<value> arg0 = args[0L]._<slice<value>>();
                            {
                                long i__prev1 = i;

                                for (long i = 0L; i < len(s); i++)
                                {
                                    arg0 = append(arg0, s[i]);
                                }


                                i = i__prev1;
                            }
                            return arg0;

                        } 
                        // append([]T, ...[]T) []T

                    } 
                    // append([]T, ...[]T) []T
                    return append(args[0L]._<slice<value>>(), args[1L]._<slice<value>>());
                    break;
                case "copy": // copy([]T, []T) int or copy([]byte, string) int
                    var src = args[1L];
                    {
                        @string (_, ok) = src._<@string>();

                        if (ok)
                        {
                            ptr<types.Signature> @params = fn.Type()._<ptr<types.Signature>>().Params();
                            src = conv(@params.At(0L).Type(), @params.At(1L).Type(), src);
                        }

                    }

                    return copy(args[0L]._<slice<value>>(), src._<slice<value>>());
                    break;
                case "close": // close(chan T)
                    close(args[0L]._<channel<value>>());
                    return null;
                    break;
                case "delete": // delete(map[K]value, K)
                    switch (args[0L].type())
                    {
                        case map<value, value> m:
                            delete(m, args[1L]);
                            break;
                        case ptr<hashmap> m:
                            m.delete(args[1L]._<hashable>());
                            break;
                        default:
                        {
                            var m = args[0L].type();
                            panic(fmt.Sprintf("illegal map type: %T", m));
                            break;
                        }
                    }
                    return null;
                    break;
                case "print": // print(any, ...)

                case "println": // print(any, ...)
                    var ln = fn.Name() == "println";
                    bytes.Buffer buf = default;
                    {
                        long i__prev1 = i;

                        foreach (var (__i, __arg) in args)
                        {
                            i = __i;
                            arg = __arg;
                            if (i > 0L && ln)
                            {
                                buf.WriteRune(' ');
                            }

                            buf.WriteString(toString(arg));

                        }

                        i = i__prev1;
                    }

                    if (ln)
                    {
                        buf.WriteRune('\n');
                    }

                    print(buf.Bytes());
                    return null;
                    break;
                case "len": 
                    switch (args[0L].type())
                    {
                        case @string x:
                            return len(x);
                            break;
                        case array x:
                            return len(x);
                            break;
                        case ptr<value> x:
                            return len((x.val)._<array>());
                            break;
                        case slice<value> x:
                            return len(x);
                            break;
                        case map<value, value> x:
                            return len(x);
                            break;
                        case ptr<hashmap> x:
                            return x.len();
                            break;
                        case channel<value> x:
                            return len(x);
                            break;
                        default:
                        {
                            var x = args[0L].type();
                            panic(fmt.Sprintf("len: illegal operand: %T", x));
                            break;
                        }

                    }
                    break;
                case "cap": 
                    switch (args[0L].type())
                    {
                        case array x:
                            return cap(x);
                            break;
                        case ptr<value> x:
                            return cap((x.val)._<array>());
                            break;
                        case slice<value> x:
                            return cap(x);
                            break;
                        case channel<value> x:
                            return cap(x);
                            break;
                        default:
                        {
                            var x = args[0L].type();
                            panic(fmt.Sprintf("cap: illegal operand: %T", x));
                            break;
                        }

                    }
                    break;
                case "real": 
                    switch (args[0L].type())
                    {
                        case complex64 c:
                            return real(c);
                            break;
                        case System.Numerics.Complex128 c:
                            return real(c);
                            break;
                        default:
                        {
                            var c = args[0L].type();
                            panic(fmt.Sprintf("real: illegal operand: %T", c));
                            break;
                        }

                    }
                    break;
                case "imag": 
                    switch (args[0L].type())
                    {
                        case complex64 c:
                            return imag(c);
                            break;
                        case System.Numerics.Complex128 c:
                            return imag(c);
                            break;
                        default:
                        {
                            var c = args[0L].type();
                            panic(fmt.Sprintf("imag: illegal operand: %T", c));
                            break;
                        }

                    }
                    break;
                case "complex": 
                    switch (args[0L].type())
                    {
                        case float f:
                            return complex(f, args[1L]._<float>());
                            break;
                        case double f:
                            return complex(f, args[1L]._<double>());
                            break;
                        default:
                        {
                            var f = args[0L].type();
                            panic(fmt.Sprintf("complex: illegal operand: %T", f));
                            break;
                        }

                    }
                    break;
                case "panic": 
                    // ssa.Panic handles most cases; this is only for "go
                    // panic" or "defer panic".
                    panic(new targetPanic(args[0]));
                    break;
                case "recover": 
                    return doRecover(caller);
                    break;
                case "ssa:wrapnilchk": 
                    var recv = args[0L];
                    if (recv._<ptr<value>>() == null)
                    {
                        var recvType = args[1L];
                        var methodName = args[2L];
                        panic(fmt.Sprintf("value method (%s).%s called using nil *%s pointer", recvType, methodName, recvType));
                    }

                    return recv;
                    break;
            }

            panic("unknown built-in: " + fn.Name());

        });

        private static iter rangeIter(value x, types.Type t) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case map<value, value> x:
                    return addr(new mapIter(iter:reflect.ValueOf(x).MapRange()));
                    break;
                case ptr<hashmap> x:
                    return addr(new hashmapIter(iter:reflect.ValueOf(x.entries()).MapRange()));
                    break;
                case @string x:
                    return addr(new stringIter(Reader:strings.NewReader(x)));
                    break;
            }
            panic(fmt.Sprintf("cannot range over %T", x));

        });

        // widen widens a basic typed value x to the widest type of its
        // category, one of:
        //   bool, int64, uint64, float64, complex128, string.
        // This is inefficient but reduces the size of the cross-product of
        // cases we have to consider.
        //
        private static value widen(value x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case bool y:
                    return x;
                    break;
                case long y:
                    return x;
                    break;
                case ulong y:
                    return x;
                    break;
                case double y:
                    return x;
                    break;
                case System.Numerics.Complex128 y:
                    return x;
                    break;
                case @string y:
                    return x;
                    break;
                case unsafe.Pointer y:
                    return x;
                    break;
                case long y:
                    return int64(y);
                    break;
                case sbyte y:
                    return int64(y);
                    break;
                case short y:
                    return int64(y);
                    break;
                case int y:
                    return int64(y);
                    break;
                case ulong y:
                    return uint64(y);
                    break;
                case byte y:
                    return uint64(y);
                    break;
                case ushort y:
                    return uint64(y);
                    break;
                case uint y:
                    return uint64(y);
                    break;
                case System.UIntPtr y:
                    return uint64(y);
                    break;
                case float y:
                    return float64(y);
                    break;
                case complex64 y:
                    return complex128(y);
                    break;
            }
            panic(fmt.Sprintf("cannot widen %T", x));

        });

        // conv converts the value x of type t_src to type t_dst and returns
        // the result.
        // Possible cases are described with the ssa.Convert operator.
        //
        private static value conv(types.Type t_dst, types.Type t_src, value x) => func((_, panic, __) =>
        {
            var ut_src = t_src.Underlying();
            var ut_dst = t_dst.Underlying(); 

            // Destination type is not an "untyped" type.
            {
                ptr<types.Basic> b__prev1 = b;

                ptr<types.Basic> (b, ok) = ut_dst._<ptr<types.Basic>>();

                if (ok && b.Info() & types.IsUntyped != 0L)
                {
                    panic("oops: conversion to 'untyped' type: " + b.String());
                } 

                // Nor is it an interface type.

                b = b__prev1;

            } 

            // Nor is it an interface type.
            {
                ptr<types.Interface> (_, ok) = ut_dst._<ptr<types.Interface>>();

                if (ok)
                {
                    {
                        (_, ok) = ut_src._<ptr<types.Interface>>();

                        if (ok)
                        {
                            panic("oops: Convert should be ChangeInterface");
                        }
                        else
                        {
                            panic("oops: Convert should be MakeInterface");
                        }

                    }

                } 

                // Remaining conversions:
                //    + untyped string/number/bool constant to a specific
                //      representation.
                //    + conversions between non-complex numeric types.
                //    + conversions between complex numeric types.
                //    + integer/[]byte/[]rune -> string.
                //    + string -> []byte/[]rune.
                //
                // All are treated the same: first we extract the value to the
                // widest representation (int64, uint64, float64, complex128,
                // or string), then we convert it to the desired type.

            } 

            // Remaining conversions:
            //    + untyped string/number/bool constant to a specific
            //      representation.
            //    + conversions between non-complex numeric types.
            //    + conversions between complex numeric types.
            //    + integer/[]byte/[]rune -> string.
            //    + string -> []byte/[]rune.
            //
            // All are treated the same: first we extract the value to the
            // widest representation (int64, uint64, float64, complex128,
            // or string), then we convert it to the desired type.

            switch (ut_src.type())
            {
                case ptr<types.Pointer> ut_src:
                    switch (ut_dst.type())
                    {
                        case ptr<types.Basic> ut_dst:
                            if (ut_dst.Kind() == types.UnsafePointer)
                            {
                                return @unsafe.Pointer(x._<ptr<value>>());
                            }

                            break;

                    }
                    break;
                case ptr<types.Slice> ut_src:

                    if (ut_src.Elem()._<ptr<types.Basic>>().Kind() == types.Byte) 
                        slice<value> x = x._<slice<value>>();
                        var b = make_slice<byte>(0L, len(x));
                        {
                            var i__prev1 = i;

                            foreach (var (__i) in x)
                            {
                                i = __i;
                                b = append(b, x[i]._<byte>());
                            }

                            i = i__prev1;
                        }

                        return string(b);
                    else if (ut_src.Elem()._<ptr<types.Basic>>().Kind() == types.Rune) 
                        x = x._<slice<value>>();
                        var r = make_slice<int>(0L, len(x));
                        {
                            var i__prev1 = i;

                            foreach (var (__i) in x)
                            {
                                i = __i;
                                r = append(r, x[i]._<int>());
                            }

                            i = i__prev1;
                        }

                        return string(r);
                                        break;
                case ptr<types.Basic> ut_src:
                    x = widen(x); 

                    // integer -> string?
                    // TODO(adonovan): fix: test integer -> named alias of string.
                    if (ut_src.Info() & types.IsInteger != 0L)
                    {
                        {
                            var ut_dst__prev2 = ut_dst;

                            ptr<types.Basic> (ut_dst, ok) = ut_dst._<ptr<types.Basic>>();

                            if (ok && ut_dst.Kind() == types.String)
                            {
                                return fmt.Sprintf("%c", x);
                            }

                            ut_dst = ut_dst__prev2;

                        }

                    } 

                    // string -> []rune, []byte or string?
                    {
                        @string (s, ok) = x._<@string>();

                        if (ok)
                        {
                            switch (ut_dst.type())
                            {
                                case ptr<types.Slice> ut_dst:
                                    slice<value> res = default; 
                                    // TODO(adonovan): fix: test named alias of rune, byte.

                                    if (ut_dst.Elem()._<ptr<types.Basic>>().Kind() == types.Rune) 
                                        {
                                            var r__prev1 = r;

                                            foreach (var (_, __r) in (slice<int>)s)
                                            {
                                                r = __r;
                                                res = append(res, r);
                                            }

                                            r = r__prev1;
                                        }

                                        return res;
                                    else if (ut_dst.Elem()._<ptr<types.Basic>>().Kind() == types.Byte) 
                                        {
                                            ptr<types.Basic> b__prev1 = b;

                                            foreach (var (_, __b) in (slice<byte>)s)
                                            {
                                                b = __b;
                                                res = append(res, b);
                                            }

                                            b = b__prev1;
                                        }

                                        return res;
                                                                        break;
                                case ptr<types.Basic> ut_dst:
                                    if (ut_dst.Kind() == types.String)
                                    {
                                        return x._<@string>();
                                    }

                                    break;
                            }
                            break; // fail: no other conversions for string
                        } 

                        // unsafe.Pointer -> *value

                    } 

                    // unsafe.Pointer -> *value
                    if (ut_src.Kind() == types.UnsafePointer)
                    { 
                        // TODO(adonovan): this is wrong and cannot
                        // really be fixed with the current design.
                        //
                        // return (*value)(x.(unsafe.Pointer))
                        // creates a new pointer of a different
                        // type but the underlying interface value
                        // knows its "true" type and so cannot be
                        // meaningfully used through the new pointer.
                        //
                        // To make this work, the interpreter needs to
                        // simulate the memory layout of a real
                        // compiled implementation.
                        //
                        // To at least preserve type-safety, we'll
                        // just return the zero value of the
                        // destination type.
                        return zero(t_dst);

                    } 

                    // Conversions between complex numeric types?
                    if (ut_src.Info() & types.IsComplex != 0L)
                    {

                        if (ut_dst._<ptr<types.Basic>>().Kind() == types.Complex64) 
                            return complex64(x._<System.Numerics.Complex128>());
                        else if (ut_dst._<ptr<types.Basic>>().Kind() == types.Complex128) 
                            return x._<System.Numerics.Complex128>();
                                                break; // fail: no other conversions for complex
                    } 

                    // Conversions between non-complex numeric types?
                    if (ut_src.Info() & types.IsNumeric != 0L)
                    {
                        ptr<types.Basic> kind = ut_dst._<ptr<types.Basic>>().Kind();
                        switch (x.type())
                        {
                            case long x:

                                if (kind == types.Int) 
                                    return int(x);
                                else if (kind == types.Int8) 
                                    return int8(x);
                                else if (kind == types.Int16) 
                                    return int16(x);
                                else if (kind == types.Int32) 
                                    return int32(x);
                                else if (kind == types.Int64) 
                                    return int64(x);
                                else if (kind == types.Uint) 
                                    return uint(x);
                                else if (kind == types.Uint8) 
                                    return uint8(x);
                                else if (kind == types.Uint16) 
                                    return uint16(x);
                                else if (kind == types.Uint32) 
                                    return uint32(x);
                                else if (kind == types.Uint64) 
                                    return uint64(x);
                                else if (kind == types.Uintptr) 
                                    return uintptr(x);
                                else if (kind == types.Float32) 
                                    return float32(x);
                                else if (kind == types.Float64) 
                                    return float64(x);
                                                                break;
                            case ulong x:

                                if (kind == types.Int) 
                                    return int(x);
                                else if (kind == types.Int8) 
                                    return int8(x);
                                else if (kind == types.Int16) 
                                    return int16(x);
                                else if (kind == types.Int32) 
                                    return int32(x);
                                else if (kind == types.Int64) 
                                    return int64(x);
                                else if (kind == types.Uint) 
                                    return uint(x);
                                else if (kind == types.Uint8) 
                                    return uint8(x);
                                else if (kind == types.Uint16) 
                                    return uint16(x);
                                else if (kind == types.Uint32) 
                                    return uint32(x);
                                else if (kind == types.Uint64) 
                                    return uint64(x);
                                else if (kind == types.Uintptr) 
                                    return uintptr(x);
                                else if (kind == types.Float32) 
                                    return float32(x);
                                else if (kind == types.Float64) 
                                    return float64(x);
                                                                break;
                            case double x:

                                if (kind == types.Int) 
                                    return int(x);
                                else if (kind == types.Int8) 
                                    return int8(x);
                                else if (kind == types.Int16) 
                                    return int16(x);
                                else if (kind == types.Int32) 
                                    return int32(x);
                                else if (kind == types.Int64) 
                                    return int64(x);
                                else if (kind == types.Uint) 
                                    return uint(x);
                                else if (kind == types.Uint8) 
                                    return uint8(x);
                                else if (kind == types.Uint16) 
                                    return uint16(x);
                                else if (kind == types.Uint32) 
                                    return uint32(x);
                                else if (kind == types.Uint64) 
                                    return uint64(x);
                                else if (kind == types.Uintptr) 
                                    return uintptr(x);
                                else if (kind == types.Float32) 
                                    return float32(x);
                                else if (kind == types.Float64) 
                                    return float64(x);
                                                                break;
                        }

                    }

                    break;

            }

            panic(fmt.Sprintf("unsupported conversion: %s  -> %s, dynamic type %T", t_src, t_dst, x));

        });

        // checkInterface checks that the method set of x implements the
        // interface itype.
        // On success it returns "", on failure, an error message.
        //
        private static @string checkInterface(ptr<interpreter> _addr_i, ptr<types.Interface> _addr_itype, iface x)
        {
            ref interpreter i = ref _addr_i.val;
            ref types.Interface itype = ref _addr_itype.val;

            {
                var (meth, _) = types.MissingMethod(x.t, itype, true);

                if (meth != null)
                {
                    return fmt.Sprintf("interface conversion: %v is not %v: missing method %s", x.t, itype, meth.Name());
                }

            }

            return ""; // ok
        }
    }
}}}}}}
