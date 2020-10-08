// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// Package js gives access to the WebAssembly host environment when using the js/wasm architecture.
// Its API is based on JavaScript semantics.
//
// This package is EXPERIMENTAL. Its current scope is only to allow tests to run, but not yet to provide a
// comprehensive API for users. It is exempt from the Go compatibility promise.
// package js -- go2cs converted at 2020 October 08 03:26:43 UTC
// import "syscall/js" ==> using js = go.syscall.js_package
// Original source: C:\Go\src\syscall\js\js.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace syscall
{
    public static partial class js_package
    {
        // ref is used to identify a JavaScript value, since the value itself can not be passed to WebAssembly.
        //
        // The JavaScript value "undefined" is represented by the value 0.
        // A JavaScript number (64-bit float, except 0 and NaN) is represented by its IEEE 754 binary representation.
        // All other values are represented as an IEEE 754 binary representation of NaN with bits 0-31 used as
        // an ID and bits 32-34 used to differentiate between string, symbol, function and object.
        private partial struct @ref // : ulong
        {
        }

        // nanHead are the upper 32 bits of a ref which are set if the value is not encoded as an IEEE 754 number (see above).
        private static readonly ulong nanHead = (ulong)0x7FF80000UL;

        // Wrapper is implemented by types that are backed by a JavaScript value.


        // Wrapper is implemented by types that are backed by a JavaScript value.
        public partial interface Wrapper
        {
            Value JSValue();
        }

        // Value represents a JavaScript value. The zero value is the JavaScript value "undefined".
        // Values can be checked for equality with the Equal method.
        public partial struct Value
        {
            public array<Action> _; // uncomparable; to make == not compile
            public ref @ref; // identifies a JavaScript value, see ref type
            public ptr<ref> gcPtr; // used to trigger the finalizer when the Value is not referenced any more
        }

 
        // the type flags need to be in sync with wasm_exec.js
        private static readonly var typeFlagNone = (var)iota;
        private static readonly var typeFlagObject = (var)0;
        private static readonly var typeFlagString = (var)1;
        private static readonly var typeFlagSymbol = (var)2;
        private static readonly var typeFlagFunction = (var)3;


        // JSValue implements Wrapper interface.
        public static Value JSValue(this Value v)
        {
            return v;
        }

        private static Value makeValue(ref r)
        {
            ptr<ref> gcPtr;
            var typeFlag = (r >> (int)(32L)) & 7L;
            if ((r >> (int)(32L)) & nanHead == nanHead && typeFlag != typeFlagNone)
            {
                gcPtr = @new<ref>();
                gcPtr.val = r;
                runtime.SetFinalizer(gcPtr, p =>
                {
                    finalizeRef(p.val);
                });

            }

            return new Value(ref:r,gcPtr:gcPtr);

        }

        private static void finalizeRef(ref r)
;

        private static Value predefValue(uint id, byte typeFlag)
        {
            return new Value(ref:(nanHead|ref(typeFlag))<<32|ref(id));
        }

        private static Value floatValue(double f)
        {
            if (f == 0L)
            {>>MARKER:FUNCTION_finalizeRef_BLOCK_PREFIX<<
                return valueZero;
            }

            if (f != f)
            {
                return valueNaN;
            }

            return new Value(ref:*(*ref)(unsafe.Pointer(&f)));

        }

        // Error wraps a JavaScript error.
        public partial struct Error
        {
            public ref Value Value => ref Value_val;
        }

        // Error implements the error interface.
        public static @string Error(this Error e)
        {
            return "JavaScript error: " + e.Get("message").String();
        }

        private static Value valueUndefined = new Value(ref:0);        private static var valueNaN = predefValue(0L, typeFlagNone);        private static var valueZero = predefValue(1L, typeFlagNone);        private static var valueNull = predefValue(2L, typeFlagNone);        private static var valueTrue = predefValue(3L, typeFlagNone);        private static var valueFalse = predefValue(4L, typeFlagNone);        private static var valueGlobal = predefValue(5L, typeFlagObject);        private static var jsGo = predefValue(6L, typeFlagObject);        private static var objectConstructor = valueGlobal.Get("Object");        private static var arrayConstructor = valueGlobal.Get("Array");

        // Equal reports whether v and w are equal according to JavaScript's === operator.
        public static bool Equal(this Value v, Value w)
        {
            return v.@ref == w.@ref && v.@ref != valueNaN.@ref;
        }

        // Undefined returns the JavaScript value "undefined".
        public static Value Undefined()
        {
            return valueUndefined;
        }

        // IsUndefined reports whether v is the JavaScript value "undefined".
        public static bool IsUndefined(this Value v)
        {
            return v.@ref == valueUndefined.@ref;
        }

        // Null returns the JavaScript value "null".
        public static Value Null()
        {
            return valueNull;
        }

        // IsNull reports whether v is the JavaScript value "null".
        public static bool IsNull(this Value v)
        {
            return v.@ref == valueNull.@ref;
        }

        // IsNaN reports whether v is the JavaScript value "NaN".
        public static bool IsNaN(this Value v)
        {
            return v.@ref == valueNaN.@ref;
        }

        // Global returns the JavaScript global object, usually "window" or "global".
        public static Value Global()
        {
            return valueGlobal;
        }

        // ValueOf returns x as a JavaScript value:
        //
        //  | Go                     | JavaScript             |
        //  | ---------------------- | ---------------------- |
        //  | js.Value               | [its value]            |
        //  | js.Func                | function               |
        //  | nil                    | null                   |
        //  | bool                   | boolean                |
        //  | integers and floats    | number                 |
        //  | string                 | string                 |
        //  | []interface{}          | new array              |
        //  | map[string]interface{} | new object             |
        //
        // Panics if x is not one of the expected types.
        public static Value ValueOf(object x) => func((_, panic, __) =>
        {
            switch (x.type())
            {
                case Value x:
                    return x;
                    break;
                case Wrapper x:
                    return x.JSValue();
                    break;
                case 
                    return valueNull;
                    break;
                case bool x:
                    if (x)
                    {
                        return valueTrue;
                    }
                    else
                    {
                        return valueFalse;
                    }

                    break;
                case long x:
                    return floatValue(float64(x));
                    break;
                case sbyte x:
                    return floatValue(float64(x));
                    break;
                case short x:
                    return floatValue(float64(x));
                    break;
                case int x:
                    return floatValue(float64(x));
                    break;
                case long x:
                    return floatValue(float64(x));
                    break;
                case ulong x:
                    return floatValue(float64(x));
                    break;
                case byte x:
                    return floatValue(float64(x));
                    break;
                case ushort x:
                    return floatValue(float64(x));
                    break;
                case uint x:
                    return floatValue(float64(x));
                    break;
                case ulong x:
                    return floatValue(float64(x));
                    break;
                case System.UIntPtr x:
                    return floatValue(float64(x));
                    break;
                case unsafe.Pointer x:
                    return floatValue(float64(uintptr(x)));
                    break;
                case float x:
                    return floatValue(float64(x));
                    break;
                case double x:
                    return floatValue(x);
                    break;
                case @string x:
                    return makeValue(stringVal(x));
                    break;
                case slice<object> x:
                    var a = arrayConstructor.New(len(x));
                    foreach (var (i, s) in x)
                    {
                        a.SetIndex(i, s);
                    }
                    return a;
                    break;
                case 
                    var o = objectConstructor.New();
                    foreach (var (k, v) in x)
                    {
                        o.Set(k, v);
                    }
                    return o;
                    break;
                default:
                {
                    var x = x.type();
                    panic("ValueOf: invalid value");
                    break;
                }
            }

        });

        private static ref stringVal(@string x)
;

        // Type represents the JavaScript type of a Value.
        public partial struct Type // : long
        {
        }

        public static readonly Type TypeUndefined = (Type)iota;
        public static readonly var TypeNull = (var)0;
        public static readonly var TypeBoolean = (var)1;
        public static readonly var TypeNumber = (var)2;
        public static readonly var TypeString = (var)3;
        public static readonly var TypeSymbol = (var)4;
        public static readonly var TypeObject = (var)5;
        public static readonly var TypeFunction = (var)6;


        public static @string String(this Type t) => func((_, panic, __) =>
        {

            if (t == TypeUndefined) 
                return "undefined";
            else if (t == TypeNull) 
                return "null";
            else if (t == TypeBoolean) 
                return "boolean";
            else if (t == TypeNumber) 
                return "number";
            else if (t == TypeString) 
                return "string";
            else if (t == TypeSymbol) 
                return "symbol";
            else if (t == TypeObject) 
                return "object";
            else if (t == TypeFunction) 
                return "function";
            else 
                panic("bad type");
            
        });

        public static bool isObject(this Type t)
        {
            return t == TypeObject || t == TypeFunction;
        }

        // Type returns the JavaScript type of the value v. It is similar to JavaScript's typeof operator,
        // except that it returns TypeNull instead of TypeObject for null.
        public static Type Type(this Value v) => func((_, panic, __) =>
        {

            if (v.@ref == valueUndefined.@ref) 
                return TypeUndefined;
            else if (v.@ref == valueNull.@ref) 
                return TypeNull;
            else if (v.@ref == valueTrue.@ref || v.@ref == valueFalse.@ref) 
                return TypeBoolean;
                        if (v.isNumber())
            {>>MARKER:FUNCTION_stringVal_BLOCK_PREFIX<<
                return TypeNumber;
            }

            var typeFlag = (v.@ref >> (int)(32L)) & 7L;

            if (typeFlag == typeFlagObject) 
                return TypeObject;
            else if (typeFlag == typeFlagString) 
                return TypeString;
            else if (typeFlag == typeFlagSymbol) 
                return TypeSymbol;
            else if (typeFlag == typeFlagFunction) 
                return TypeFunction;
            else 
                panic("bad type flag");
            
        });

        // Get returns the JavaScript property p of value v.
        // It panics if v is not a JavaScript object.
        public static Value Get(this Value v, @string p) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {
                    panic(addr(new ValueError("Value.Get",vType)));
                }

            }

            var r = makeValue(valueGet(v.@ref, p));
            runtime.KeepAlive(v);
            return r;

        });

        private static ref valueGet(ref v, @string p)
;

        // Set sets the JavaScript property p of value v to ValueOf(x).
        // It panics if v is not a JavaScript object.
        public static void Set(this Value v, @string p, object x) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {>>MARKER:FUNCTION_valueGet_BLOCK_PREFIX<<
                    panic(addr(new ValueError("Value.Set",vType)));
                }

            }

            var xv = ValueOf(x);
            valueSet(v.@ref, p, xv.@ref);
            runtime.KeepAlive(v);
            runtime.KeepAlive(xv);

        });

        private static void valueSet(ref v, @string p, ref x)
;

        // Delete deletes the JavaScript property p of value v.
        // It panics if v is not a JavaScript object.
        public static void Delete(this Value v, @string p) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {>>MARKER:FUNCTION_valueSet_BLOCK_PREFIX<<
                    panic(addr(new ValueError("Value.Delete",vType)));
                }

            }

            valueDelete(v.@ref, p);
            runtime.KeepAlive(v);

        });

        private static void valueDelete(ref v, @string p)
;

        // Index returns JavaScript index i of value v.
        // It panics if v is not a JavaScript object.
        public static Value Index(this Value v, long i) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {>>MARKER:FUNCTION_valueDelete_BLOCK_PREFIX<<
                    panic(addr(new ValueError("Value.Index",vType)));
                }

            }

            var r = makeValue(valueIndex(v.@ref, i));
            runtime.KeepAlive(v);
            return r;

        });

        private static ref valueIndex(ref v, long i)
;

        // SetIndex sets the JavaScript index i of value v to ValueOf(x).
        // It panics if v is not a JavaScript object.
        public static void SetIndex(this Value v, long i, object x) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {>>MARKER:FUNCTION_valueIndex_BLOCK_PREFIX<<
                    panic(addr(new ValueError("Value.SetIndex",vType)));
                }

            }

            var xv = ValueOf(x);
            valueSetIndex(v.@ref, i, xv.@ref);
            runtime.KeepAlive(v);
            runtime.KeepAlive(xv);

        });

        private static void valueSetIndex(ref v, long i, ref x)
;

        private static (slice<Value>, slice<ref>) makeArgs(slice<object> args)
        {
            slice<Value> _p0 = default;
            slice<ref> _p0 = default;

            var argVals = make_slice<Value>(len(args));
            var argRefs = make_slice<ref>(len(args));
            foreach (var (i, arg) in args)
            {
                var v = ValueOf(arg);
                argVals[i] = v;
                argRefs[i] = v.@ref;
            }
            return (argVals, argRefs);

        }

        // Length returns the JavaScript property "length" of v.
        // It panics if v is not a JavaScript object.
        public static long Length(this Value v) => func((_, panic, __) =>
        {
            {
                var vType = v.Type();

                if (!vType.isObject())
                {>>MARKER:FUNCTION_valueSetIndex_BLOCK_PREFIX<<
                    panic(addr(new ValueError("Value.SetIndex",vType)));
                }

            }

            var r = valueLength(v.@ref);
            runtime.KeepAlive(v);
            return r;

        });

        private static long valueLength(ref v)
;

        // Call does a JavaScript call to the method m of value v with the given arguments.
        // It panics if v has no method m.
        // The arguments get mapped to JavaScript values according to the ValueOf function.
        public static Value Call(this Value v, @string m, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            var (argVals, argRefs) = makeArgs(args);
            var (res, ok) = valueCall(v.@ref, m, argRefs);
            runtime.KeepAlive(v);
            runtime.KeepAlive(argVals);
            if (!ok)
            {>>MARKER:FUNCTION_valueLength_BLOCK_PREFIX<<
                {
                    var vType = v.Type();

                    if (!vType.isObject())
                    { // check here to avoid overhead in success case
                        panic(addr(new ValueError("Value.Call",vType)));

                    }

                }

                {
                    var propType = v.Get(m).Type();

                    if (propType != TypeFunction)
                    {
                        panic("syscall/js: Value.Call: property " + m + " is not a function, got " + propType.String());
                    }

                }

                panic(new Error(makeValue(res)));

            }

            return makeValue(res);

        });

        private static (ref, bool) valueCall(ref v, @string m, slice<ref> args)
;

        // Invoke does a JavaScript call of the value v with the given arguments.
        // It panics if v is not a JavaScript function.
        // The arguments get mapped to JavaScript values according to the ValueOf function.
        public static Value Invoke(this Value v, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            var (argVals, argRefs) = makeArgs(args);
            var (res, ok) = valueInvoke(v.@ref, argRefs);
            runtime.KeepAlive(v);
            runtime.KeepAlive(argVals);
            if (!ok)
            {>>MARKER:FUNCTION_valueCall_BLOCK_PREFIX<<
                {
                    var vType = v.Type();

                    if (vType != TypeFunction)
                    { // check here to avoid overhead in success case
                        panic(addr(new ValueError("Value.Invoke",vType)));

                    }

                }

                panic(new Error(makeValue(res)));

            }

            return makeValue(res);

        });

        private static (ref, bool) valueInvoke(ref v, slice<ref> args)
;

        // New uses JavaScript's "new" operator with value v as constructor and the given arguments.
        // It panics if v is not a JavaScript function.
        // The arguments get mapped to JavaScript values according to the ValueOf function.
        public static Value New(this Value v, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            var (argVals, argRefs) = makeArgs(args);
            var (res, ok) = valueNew(v.@ref, argRefs);
            runtime.KeepAlive(v);
            runtime.KeepAlive(argVals);
            if (!ok)
            {>>MARKER:FUNCTION_valueInvoke_BLOCK_PREFIX<<
                {
                    var vType = v.Type();

                    if (vType != TypeFunction)
                    { // check here to avoid overhead in success case
                        panic(addr(new ValueError("Value.Invoke",vType)));

                    }

                }

                panic(new Error(makeValue(res)));

            }

            return makeValue(res);

        });

        private static (ref, bool) valueNew(ref v, slice<ref> args)
;

        public static bool isNumber(this Value v)
        {
            return v.@ref == valueZero.@ref || v.@ref == valueNaN.@ref || (v.@ref != valueUndefined.@ref && (v.@ref >> (int)(32L)) & nanHead != nanHead);
        }

        public static double @float(this Value v, @string method) => func((_, panic, __) =>
        {
            if (!v.isNumber())
            {>>MARKER:FUNCTION_valueNew_BLOCK_PREFIX<<
                panic(addr(new ValueError(method,v.Type())));
            }

            if (v.@ref == valueZero.@ref)
            {
                return 0L;
            }

            return new ptr<ptr<ptr<double>>>(@unsafe.Pointer(_addr_v.@ref));

        });

        // Float returns the value v as a float64.
        // It panics if v is not a JavaScript number.
        public static double Float(this Value v)
        {
            return v.@float("Value.Float");
        }

        // Int returns the value v truncated to an int.
        // It panics if v is not a JavaScript number.
        public static long Int(this Value v)
        {
            return int(v.@float("Value.Int"));
        }

        // Bool returns the value v as a bool.
        // It panics if v is not a JavaScript boolean.
        public static bool Bool(this Value v) => func((_, panic, __) =>
        {

            if (v.@ref == valueTrue.@ref) 
                return true;
            else if (v.@ref == valueFalse.@ref) 
                return false;
            else 
                panic(addr(new ValueError("Value.Bool",v.Type())));
            
        });

        // Truthy returns the JavaScript "truthiness" of the value v. In JavaScript,
        // false, 0, "", null, undefined, and NaN are "falsy", and everything else is
        // "truthy". See https://developer.mozilla.org/en-US/docs/Glossary/Truthy.
        public static bool Truthy(this Value v) => func((_, panic, __) =>
        {

            if (v.Type() == TypeUndefined || v.Type() == TypeNull) 
                return false;
            else if (v.Type() == TypeBoolean) 
                return v.Bool();
            else if (v.Type() == TypeNumber) 
                return v.@ref != valueNaN.@ref && v.@ref != valueZero.@ref;
            else if (v.Type() == TypeString) 
                return v.String() != "";
            else if (v.Type() == TypeSymbol || v.Type() == TypeFunction || v.Type() == TypeObject) 
                return true;
            else 
                panic("bad type");
            
        });

        // String returns the value v as a string.
        // String is a special case because of Go's String method convention. Unlike the other getters,
        // it does not panic if v's Type is not TypeString. Instead, it returns a string of the form "<T>"
        // or "<T: V>" where T is v's type and V is a string representation of v's value.
        public static @string String(this Value v) => func((_, panic, __) =>
        {

            if (v.Type() == TypeString) 
                return jsString(v);
            else if (v.Type() == TypeUndefined) 
                return "<undefined>";
            else if (v.Type() == TypeNull) 
                return "<null>";
            else if (v.Type() == TypeBoolean) 
                return "<boolean: " + jsString(v) + ">";
            else if (v.Type() == TypeNumber) 
                return "<number: " + jsString(v) + ">";
            else if (v.Type() == TypeSymbol) 
                return "<symbol>";
            else if (v.Type() == TypeObject) 
                return "<object>";
            else if (v.Type() == TypeFunction) 
                return "<function>";
            else 
                panic("bad type");
            
        });

        private static @string jsString(Value v)
        {
            var (str, length) = valuePrepareString(v.@ref);
            runtime.KeepAlive(v);
            var b = make_slice<byte>(length);
            valueLoadString(str, b);
            finalizeRef(str);
            return string(b);
        }

        private static (ref, long) valuePrepareString(ref v)
;

        private static void valueLoadString(ref v, slice<byte> b)
;

        // InstanceOf reports whether v is an instance of type t according to JavaScript's instanceof operator.
        public static bool InstanceOf(this Value v, Value t)
        {
            var r = valueInstanceOf(v.@ref, t.@ref);
            runtime.KeepAlive(v);
            runtime.KeepAlive(t);
            return r;
        }

        private static bool valueInstanceOf(ref v, ref t)
;

        // A ValueError occurs when a Value method is invoked on
        // a Value that does not support it. Such cases are documented
        // in the description of each method.
        public partial struct ValueError
        {
            public @string Method;
            public Type Type;
        }

        private static @string Error(this ptr<ValueError> _addr_e)
        {
            ref ValueError e = ref _addr_e.val;

            return "syscall/js: call of " + e.Method + " on " + e.Type.String();
        }

        // CopyBytesToGo copies bytes from src to dst.
        // It panics if src is not an Uint8Array or Uint8ClampedArray.
        // It returns the number of bytes copied, which will be the minimum of the lengths of src and dst.
        public static long CopyBytesToGo(slice<byte> dst, Value src) => func((_, panic, __) =>
        {
            var (n, ok) = copyBytesToGo(dst, src.@ref);
            runtime.KeepAlive(src);
            if (!ok)
            {>>MARKER:FUNCTION_valueInstanceOf_BLOCK_PREFIX<<
                panic("syscall/js: CopyBytesToGo: expected src to be an Uint8Array or Uint8ClampedArray");
            }

            return n;

        });

        private static (long, bool) copyBytesToGo(slice<byte> dst, ref src)
;

        // CopyBytesToJS copies bytes from src to dst.
        // It panics if dst is not an Uint8Array or Uint8ClampedArray.
        // It returns the number of bytes copied, which will be the minimum of the lengths of src and dst.
        public static long CopyBytesToJS(Value dst, slice<byte> src) => func((_, panic, __) =>
        {
            var (n, ok) = copyBytesToJS(dst.@ref, src);
            runtime.KeepAlive(dst);
            if (!ok)
            {>>MARKER:FUNCTION_copyBytesToGo_BLOCK_PREFIX<<
                panic("syscall/js: CopyBytesToJS: expected dst to be an Uint8Array or Uint8ClampedArray");
            }

            return n;

        });

        private static (long, bool) copyBytesToJS(ref dst, slice<byte> src)
;
    }
}}
