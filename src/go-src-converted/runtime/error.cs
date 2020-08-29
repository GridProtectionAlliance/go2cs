// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:55 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\error.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    { // for go:linkname

        // The Error interface identifies a run time error.
        public partial interface Error : error
        {
            void RuntimeError();
        }

        // A TypeAssertionError explains a failed type assertion.
        public partial struct TypeAssertionError
        {
            public @string interfaceString;
            public @string concreteString;
            public @string assertedString;
            public @string missingMethod; // one method needed by Interface, missing from Concrete
        }

        private static void RuntimeError(this ref TypeAssertionError _p0)
        {
        }

        private static @string Error(this ref TypeAssertionError e)
        {
            var inter = e.interfaceString;
            if (inter == "")
            {
                inter = "interface";
            }
            if (e.concreteString == "")
            {
                return "interface conversion: " + inter + " is nil, not " + e.assertedString;
            }
            if (e.missingMethod == "")
            {
                return "interface conversion: " + inter + " is " + e.concreteString + ", not " + e.assertedString;
            }
            return "interface conversion: " + e.concreteString + " is not " + e.assertedString + ": missing method " + e.missingMethod;
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

        private partial interface stringer
        {
            @string String();
        }

        private static @string typestring(object x)
        {
            var e = efaceOf(ref x);
            return e._type.@string();
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
                    print("(", typestring(i), ") ", i);
                    break;
                }
            }
        }

        // strings.IndexByte is implemented in runtime/asm_$goarch.s
        // but amusingly we need go:linkname to get access to it here in the runtime.
        //go:linkname stringsIndexByte strings.IndexByte
        private static long stringsIndexByte(@string s, byte c)
;

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
            var i = stringsIndexByte(name, '(');
            if (i < 0L)
            {>>MARKER:FUNCTION_stringsIndexByte_BLOCK_PREFIX<<
                throw("panicwrap: no ( in " + name);
            }
            var pkg = name[..i - 1L];
            if (i + 2L >= len(name) || name[i - 1L..i + 2L] != ".(*")
            {
                throw("panicwrap: unexpected string after package name: " + name);
            }
            name = name[i + 2L..];
            i = stringsIndexByte(name, ')');
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
