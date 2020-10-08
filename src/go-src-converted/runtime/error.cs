// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:19:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\error.go
using bytealg = go.@internal.bytealg_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // The Error interface identifies a run time error.
        public partial interface Error : error
        {
            void RuntimeError();
        }

        // A TypeAssertionError explains a failed type assertion.
        public partial struct TypeAssertionError
        {
            public ptr<_type> _interface;
            public ptr<_type> concrete;
            public ptr<_type> asserted;
            public @string missingMethod; // one method needed by Interface, missing from Concrete
        }

        private static void RuntimeError(this ptr<TypeAssertionError> _addr__p0)
        {
            ref TypeAssertionError _p0 = ref _addr__p0.val;

        }

        private static @string Error(this ptr<TypeAssertionError> _addr_e)
        {
            ref TypeAssertionError e = ref _addr_e.val;

            @string inter = "interface";
            if (e._interface != null)
            {
                inter = e._interface.@string();
            }

            var @as = e.asserted.@string();
            if (e.concrete == null)
            {
                return "interface conversion: " + inter + " is nil, not " + as;
            }

            var cs = e.concrete.@string();
            if (e.missingMethod == "")
            {
                @string msg = "interface conversion: " + inter + " is " + cs + ", not " + as;
                if (cs == as)
                { 
                    // provide slightly clearer error message
                    if (e.concrete.pkgpath() != e.asserted.pkgpath())
                    {
                        msg += " (types from different packages)";
                    }
                    else
                    {
                        msg += " (types from different scopes)";
                    }

                }

                return msg;

            }

            return "interface conversion: " + cs + " is not " + as + ": missing method " + e.missingMethod;

        }

        //go:nosplit
        // itoa converts val to a decimal representation. The result is
        // written somewhere within buf and the location of the result is returned.
        // buf must be at least 20 bytes.
        private static slice<byte> itoa(slice<byte> buf, ulong val)
        {
            var i = len(buf) - 1L;
            while (val >= 10L)
            {
                buf[i] = byte(val % 10L + '0');
                i--;
                val /= 10L;
            }

            buf[i] = byte(val + '0');
            return buf[i..];

        }

        // An errorString represents a runtime error described by a single string.
        private partial struct errorString // : @string
        {
        }

        private static void RuntimeError(this errorString e)
        {
        }

        private static @string Error(this errorString e)
        {
            return "runtime error: " + string(e);
        }

        // plainError represents a runtime error described a string without
        // the prefix "runtime error: " after invoking errorString.Error().
        // See Issue #14965.
        private partial struct plainError // : @string
        {
        }

        private static void RuntimeError(this plainError e)
        {
        }

        private static @string Error(this plainError e)
        {
            return string(e);
        }

        // A boundsError represents an indexing or slicing operation gone wrong.
        private partial struct boundsError
        {
            public long x;
            public long y; // Values in an index or slice expression can be signed or unsigned.
// That means we'd need 65 bits to encode all possible indexes, from -2^63 to 2^64-1.
// Instead, we keep track of whether x should be interpreted as signed or unsigned.
// y is known to be nonnegative and to fit in an int.
            public bool signed;
            public boundsErrorCode code;
        }

        private partial struct boundsErrorCode // : byte
        {
        }

        private static readonly boundsErrorCode boundsIndex = (boundsErrorCode)iota; // s[x], 0 <= x < len(s) failed

        private static readonly var boundsSliceAlen = (var)0; // s[?:x], 0 <= x <= len(s) failed
        private static readonly var boundsSliceAcap = (var)1; // s[?:x], 0 <= x <= cap(s) failed
        private static readonly var boundsSliceB = (var)2; // s[x:y], 0 <= x <= y failed (but boundsSliceA didn't happen)

        private static readonly var boundsSlice3Alen = (var)3; // s[?:?:x], 0 <= x <= len(s) failed
        private static readonly var boundsSlice3Acap = (var)4; // s[?:?:x], 0 <= x <= cap(s) failed
        private static readonly var boundsSlice3B = (var)5; // s[?:x:y], 0 <= x <= y failed (but boundsSlice3A didn't happen)
        private static readonly var boundsSlice3C = (var)6; // s[x:y:?], 0 <= x <= y failed (but boundsSlice3A/B didn't happen)

        // Note: in the above, len(s) and cap(s) are stored in y

        // boundsErrorFmts provide error text for various out-of-bounds panics.
        // Note: if you change these strings, you should adjust the size of the buffer
        // in boundsError.Error below as well.
        private static array<@string> boundsErrorFmts = new array<@string>(InitKeyedValues<@string>((boundsIndex, "index out of range [%x] with length %y"), (boundsSliceAlen, "slice bounds out of range [:%x] with length %y"), (boundsSliceAcap, "slice bounds out of range [:%x] with capacity %y"), (boundsSliceB, "slice bounds out of range [%x:%y]"), (boundsSlice3Alen, "slice bounds out of range [::%x] with length %y"), (boundsSlice3Acap, "slice bounds out of range [::%x] with capacity %y"), (boundsSlice3B, "slice bounds out of range [:%x:%y]"), (boundsSlice3C, "slice bounds out of range [%x:%y:]")));

        // boundsNegErrorFmts are overriding formats if x is negative. In this case there's no need to report y.
        private static array<@string> boundsNegErrorFmts = new array<@string>(InitKeyedValues<@string>((boundsIndex, "index out of range [%x]"), (boundsSliceAlen, "slice bounds out of range [:%x]"), (boundsSliceAcap, "slice bounds out of range [:%x]"), (boundsSliceB, "slice bounds out of range [%x:]"), (boundsSlice3Alen, "slice bounds out of range [::%x]"), (boundsSlice3Acap, "slice bounds out of range [::%x]"), (boundsSlice3B, "slice bounds out of range [:%x:]"), (boundsSlice3C, "slice bounds out of range [%x::]")));

        private static void RuntimeError(this boundsError e)
        {
        }

        private static slice<byte> appendIntStr(slice<byte> b, long v, bool signed)
        {
            if (signed && v < 0L)
            {
                b = append(b, '-');
                v = -v;
            }

            array<byte> buf = new array<byte>(20L);
            b = append(b, itoa(buf[..], uint64(v)));
            return b;

        }

        private static @string Error(this boundsError e)
        {
            var fmt = boundsErrorFmts[e.code];
            if (e.signed && e.x < 0L)
            {
                fmt = boundsNegErrorFmts[e.code];
            } 
            // max message length is 99: "runtime error: slice bounds out of range [::%x] with capacity %y"
            // x can be at most 20 characters. y can be at most 19.
            var b = make_slice<byte>(0L, 100L);
            b = append(b, "runtime error: ");
            for (long i = 0L; i < len(fmt); i++)
            {
                var c = fmt[i];
                if (c != '%')
                {
                    b = append(b, c);
                    continue;
                }

                i++;
                switch (fmt[i])
                {
                    case 'x': 
                        b = appendIntStr(b, e.x, e.signed);
                        break;
                    case 'y': 
                        b = appendIntStr(b, int64(e.y), true);
                        break;
                }

            }

            return string(b);

        }

        private partial interface stringer
        {
            @string String();
        }

        // printany prints an argument passed to panic.
        // If panic is called with a value that has a String or Error method,
        // it has already been converted into a string by preprintpanics.
        private static void printany(object i)
        {
            switch (i.type())
            {
                case 
                    print("nil");
                    break;
                case bool v:
                    print(v);
                    break;
                case long v:
                    print(v);
                    break;
                case sbyte v:
                    print(v);
                    break;
                case short v:
                    print(v);
                    break;
                case int v:
                    print(v);
                    break;
                case long v:
                    print(v);
                    break;
                case ulong v:
                    print(v);
                    break;
                case byte v:
                    print(v);
                    break;
                case ushort v:
                    print(v);
                    break;
                case uint v:
                    print(v);
                    break;
                case ulong v:
                    print(v);
                    break;
                case System.UIntPtr v:
                    print(v);
                    break;
                case float v:
                    print(v);
                    break;
                case double v:
                    print(v);
                    break;
                case complex64 v:
                    print(v);
                    break;
                case System.Numerics.Complex128 v:
                    print(v);
                    break;
                case @string v:
                    print(v);
                    break;
                default:
                {
                    var v = i.type();
                    printanycustomtype(i);
                    break;
                }
            }

        }

        private static void printanycustomtype(object i)
        {
            var eface = efaceOf(_addr_i);
            var typestring = eface._type.@string();


            if (eface._type.kind == kindString) 
                print(typestring, "(\"", new ptr<ptr<ptr<@string>>>(eface.data), "\")");
            else if (eface._type.kind == kindBool) 
                print(typestring, "(", new ptr<ptr<ptr<bool>>>(eface.data), ")");
            else if (eface._type.kind == kindInt) 
                print(typestring, "(", new ptr<ptr<ptr<long>>>(eface.data), ")");
            else if (eface._type.kind == kindInt8) 
                print(typestring, "(", new ptr<ptr<ptr<sbyte>>>(eface.data), ")");
            else if (eface._type.kind == kindInt16) 
                print(typestring, "(", new ptr<ptr<ptr<short>>>(eface.data), ")");
            else if (eface._type.kind == kindInt32) 
                print(typestring, "(", new ptr<ptr<ptr<int>>>(eface.data), ")");
            else if (eface._type.kind == kindInt64) 
                print(typestring, "(", new ptr<ptr<ptr<long>>>(eface.data), ")");
            else if (eface._type.kind == kindUint) 
                print(typestring, "(", new ptr<ptr<ptr<ulong>>>(eface.data), ")");
            else if (eface._type.kind == kindUint8) 
                print(typestring, "(", new ptr<ptr<ptr<byte>>>(eface.data), ")");
            else if (eface._type.kind == kindUint16) 
                print(typestring, "(", new ptr<ptr<ptr<ushort>>>(eface.data), ")");
            else if (eface._type.kind == kindUint32) 
                print(typestring, "(", new ptr<ptr<ptr<uint>>>(eface.data), ")");
            else if (eface._type.kind == kindUint64) 
                print(typestring, "(", new ptr<ptr<ptr<ulong>>>(eface.data), ")");
            else if (eface._type.kind == kindUintptr) 
                print(typestring, "(", new ptr<ptr<ptr<System.UIntPtr>>>(eface.data), ")");
            else if (eface._type.kind == kindFloat32) 
                print(typestring, "(", new ptr<ptr<ptr<float>>>(eface.data), ")");
            else if (eface._type.kind == kindFloat64) 
                print(typestring, "(", new ptr<ptr<ptr<double>>>(eface.data), ")");
            else if (eface._type.kind == kindComplex64) 
                print(typestring, new ptr<ptr<ptr<complex64>>>(eface.data));
            else if (eface._type.kind == kindComplex128) 
                print(typestring, new ptr<ptr<ptr<System.Numerics.Complex128>>>(eface.data));
            else 
                print("(", typestring, ") ", eface.data);
            
        }

        // panicwrap generates a panic for a call to a wrapped value method
        // with a nil pointer receiver.
        //
        // It is called from the generated wrapper code.
        private static void panicwrap() => func((_, panic, __) =>
        {
            var pc = getcallerpc();
            var name = funcname(findfunc(pc)); 
            // name is something like "main.(*T).F".
            // We want to extract pkg ("main"), typ ("T"), and meth ("F").
            // Do it by finding the parens.
            var i = bytealg.IndexByteString(name, '(');
            if (i < 0L)
            {
                throw("panicwrap: no ( in " + name);
            }

            var pkg = name[..i - 1L];
            if (i + 2L >= len(name) || name[i - 1L..i + 2L] != ".(*")
            {
                throw("panicwrap: unexpected string after package name: " + name);
            }

            name = name[i + 2L..];
            i = bytealg.IndexByteString(name, ')');
            if (i < 0L)
            {
                throw("panicwrap: no ) in " + name);
            }

            if (i + 2L >= len(name) || name[i..i + 2L] != ").")
            {
                throw("panicwrap: unexpected string after type name: " + name);
            }

            var typ = name[..i];
            var meth = name[i + 2L..];
            panic(plainError("value method " + pkg + "." + typ + "." + meth + " called using nil *" + typ + " pointer"));

        });
    }
}
