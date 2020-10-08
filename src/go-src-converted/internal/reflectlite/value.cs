// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package reflectlite -- go2cs converted at 2020 October 08 03:19:03 UTC
// import "internal/reflectlite" ==> using reflectlite = go.@internal.reflectlite_package
// Original source: C:\Go\src\internal\reflectlite\value.go
using unsafeheader = go.@internal.unsafeheader_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class reflectlite_package
    {
        private static readonly long ptrSize = (long)4L << (int)((~uintptr(0L) >> (int)(63L))); // unsafe.Sizeof(uintptr(0)) but an ideal const

        // Value is the reflection interface to a Go value.
        //
        // Not all methods apply to all kinds of values. Restrictions,
        // if any, are noted in the documentation for each method.
        // Use the Kind method to find out the kind of value before
        // calling kind-specific methods. Calling a method
        // inappropriate to the kind of type causes a run time panic.
        //
        // The zero Value represents no value.
        // Its IsValid method returns false, its Kind method returns Invalid,
        // its String method returns "<invalid Value>", and all other methods panic.
        // Most functions and methods never return an invalid value.
        // If one does, its documentation states the conditions explicitly.
        //
        // A Value can be used concurrently by multiple goroutines provided that
        // the underlying Go value can be used concurrently for the equivalent
        // direct operations.
        //
        // To compare two Values, compare the results of the Interface method.
        // Using == on two Values does not compare the underlying values
        // they represent.
 // unsafe.Sizeof(uintptr(0)) but an ideal const

        // Value is the reflection interface to a Go value.
        //
        // Not all methods apply to all kinds of values. Restrictions,
        // if any, are noted in the documentation for each method.
        // Use the Kind method to find out the kind of value before
        // calling kind-specific methods. Calling a method
        // inappropriate to the kind of type causes a run time panic.
        //
        // The zero Value represents no value.
        // Its IsValid method returns false, its Kind method returns Invalid,
        // its String method returns "<invalid Value>", and all other methods panic.
        // Most functions and methods never return an invalid value.
        // If one does, its documentation states the conditions explicitly.
        //
        // A Value can be used concurrently by multiple goroutines provided that
        // the underlying Go value can be used concurrently for the equivalent
        // direct operations.
        //
        // To compare two Values, compare the results of the Interface method.
        // Using == on two Values does not compare the underlying values
        // they represent.
        public partial struct Value
        {
            public ptr<rtype> typ; // Pointer-valued data or, if flagIndir is set, pointer to data.
// Valid when either flagIndir is set or typ.pointers() is true.
            public unsafe.Pointer ptr; // flag holds metadata about the value.
// The lowest bits are flag bits:
//    - flagStickyRO: obtained via unexported not embedded field, so read-only
//    - flagEmbedRO: obtained via unexported embedded field, so read-only
//    - flagIndir: val holds a pointer to the data
//    - flagAddr: v.CanAddr is true (implies flagIndir)
// Value cannot represent method values.
// The next five bits give the Kind of the value.
// This repeats typ.Kind() except for method values.
// The remaining 23+ bits give a method number for method values.
// If flag.kind() != Func, code can assume that flagMethod is unset.
// If ifaceIndir(typ), code can assume that flagIndir is set.
            public ref flag flag => ref flag_val; // A method value represents a curried method invocation
// like r.Read for some receiver r. The typ+val+flag bits describe
// the receiver r, but the flag's Kind bits say Func (methods are
// functions), and the top bits of the flag give the method number
// in r's type's method table.
        }

        private partial struct flag // : System.UIntPtr
        {
        }

        private static readonly long flagKindWidth = (long)5L; // there are 27 kinds
        private static readonly flag flagKindMask = (flag)1L << (int)(flagKindWidth) - 1L;
        private static readonly flag flagStickyRO = (flag)1L << (int)(5L);
        private static readonly flag flagEmbedRO = (flag)1L << (int)(6L);
        private static readonly flag flagIndir = (flag)1L << (int)(7L);
        private static readonly flag flagAddr = (flag)1L << (int)(8L);
        private static readonly flag flagMethod = (flag)1L << (int)(9L);
        private static readonly long flagMethodShift = (long)10L;
        private static readonly flag flagRO = (flag)flagStickyRO | flagEmbedRO;


        private static Kind kind(this flag f)
        {
            return Kind(f & flagKindMask);
        }

        private static flag ro(this flag f)
        {
            if (f & flagRO != 0L)
            {
                return flagStickyRO;
            }

            return 0L;

        }

        // pointer returns the underlying pointer represented by v.
        // v.Kind() must be Ptr, Map, Chan, Func, or UnsafePointer
        public static unsafe.Pointer pointer(this Value v) => func((_, panic, __) =>
        {
            if (v.typ.size != ptrSize || !v.typ.pointers())
            {
                panic("can't call pointer on a non-pointer Value");
            }

            if (v.flag & flagIndir != 0L)
            {
                return new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
            }

            return v.ptr;

        });

        // packEface converts v to the empty interface.
        private static void packEface(Value v) => func((_, panic, __) =>
        {
            var t = v.typ;
            ref var i = ref heap(out ptr<var> _addr_i);
            var e = (emptyInterface.val)(@unsafe.Pointer(_addr_i)); 
            // First, fill in the data portion of the interface.

            if (ifaceIndir(t)) 
                if (v.flag & flagIndir == 0L)
                {
                    panic("bad indir");
                } 
                // Value is indirect, and so is the interface we're making.
                var ptr = v.ptr;
                if (v.flag & flagAddr != 0L)
                { 
                    // TODO: pass safe boolean from valueInterface so
                    // we don't need to copy if safe==true?
                    var c = unsafe_New(_addr_t);
                    typedmemmove(_addr_t, c, ptr);
                    ptr = c;

                }

                e.word = ptr;
            else if (v.flag & flagIndir != 0L) 
                // Value is indirect, but interface is direct. We need
                // to load the data at v.ptr into the interface data word.
                e.word = new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr);
            else 
                // Value is direct, and so is the interface.
                e.word = v.ptr;
            // Now, fill in the type portion. We're very careful here not
            // to have any operation between the e.word and e.typ assignments
            // that would let the garbage collector observe the partially-built
            // interface value.
            e.typ = t;
            return i;

        });

        // unpackEface converts the empty interface i to a Value.
        private static Value unpackEface(object i)
        {
            var e = (emptyInterface.val)(@unsafe.Pointer(_addr_i)); 
            // NOTE: don't read e.word until we know whether it is really a pointer or not.
            var t = e.typ;
            if (t == null)
            {
                return new Value();
            }

            var f = flag(t.Kind());
            if (ifaceIndir(t))
            {
                f |= flagIndir;
            }

            return new Value(t,e.word,f);

        }

        // A ValueError occurs when a Value method is invoked on
        // a Value that does not support it. Such cases are documented
        // in the description of each method.
        public partial struct ValueError
        {
            public @string Method;
            public Kind Kind;
        }

        private static @string Error(this ptr<ValueError> _addr_e)
        {
            ref ValueError e = ref _addr_e.val;

            return "reflect: call of " + e.Method + " on zero Value";
        }

        // methodName returns the name of the calling method,
        // assumed to be two stack frames above.
        private static @string methodName()
        {
            var (pc, _, _, _) = runtime.Caller(2L);
            var f = runtime.FuncForPC(pc);
            if (f == null)
            {
                return "unknown method";
            }

            return f.Name();

        }

        // emptyInterface is the header for an interface{} value.
        private partial struct emptyInterface
        {
            public ptr<rtype> typ;
            public unsafe.Pointer word;
        }

        // mustBeExported panics if f records that the value was obtained using
        // an unexported field.
        private static void mustBeExported(this flag f) => func((_, panic, __) =>
        {
            if (f == 0L)
            {
                panic(addr(new ValueError(methodName(),0)));
            }

            if (f & flagRO != 0L)
            {
                panic("reflect: " + methodName() + " using value obtained using unexported field");
            }

        });

        // mustBeAssignable panics if f records that the value is not assignable,
        // which is to say that either it was obtained using an unexported field
        // or it is not addressable.
        private static void mustBeAssignable(this flag f) => func((_, panic, __) =>
        {
            if (f == 0L)
            {
                panic(addr(new ValueError(methodName(),Invalid)));
            } 
            // Assignable if addressable and not read-only.
            if (f & flagRO != 0L)
            {
                panic("reflect: " + methodName() + " using value obtained using unexported field");
            }

            if (f & flagAddr == 0L)
            {
                panic("reflect: " + methodName() + " using unaddressable value");
            }

        });

        // CanSet reports whether the value of v can be changed.
        // A Value can be changed only if it is addressable and was not
        // obtained by the use of unexported struct fields.
        // If CanSet returns false, calling Set or any type-specific
        // setter (e.g., SetBool, SetInt) will panic.
        public static bool CanSet(this Value v)
        {
            return v.flag & (flagAddr | flagRO) == flagAddr;
        }

        // Elem returns the value that the interface v contains
        // or that the pointer v points to.
        // It panics if v's Kind is not Interface or Ptr.
        // It returns the zero Value if v is nil.
        public static Value Elem(this Value v) => func((_, panic, __) =>
        {
            var k = v.kind();

            if (k == Interface) 
                var eface = default;
                if (v.typ.NumMethod() == 0L)
                {
                }
                else
                {
                }

                var x = unpackEface(eface);
                if (x.flag != 0L)
                {
                    x.flag |= v.flag.ro();
                }

                return x;
            else if (k == Ptr) 
                var ptr = v.ptr;
                if (v.flag & flagIndir != 0L)
                {
                    ptr = new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
                } 
                // The returned value's address is v's value.
                if (ptr == null)
                {
                    return new Value();
                }

                var tt = (ptrType.val)(@unsafe.Pointer(v.typ));
                var typ = tt.elem;
                var fl = v.flag & flagRO | flagIndir | flagAddr;
                fl |= flag(typ.Kind());
                return new Value(typ,ptr,fl);
                        panic(addr(new ValueError("reflectlite.Value.Elem",v.kind())));

        });

        private static void valueInterface(Value v) => func((_, panic, __) =>
        {
            if (v.flag == 0L)
            {
                panic(addr(new ValueError("reflectlite.Value.Interface",0)));
            }

            if (v.kind() == Interface)
            { 
                // Special case: return the element inside the interface.
                // Empty interface has one layout, all interfaces with
                // methods have a second layout.
                if (v.numMethod() == 0L)
                {
                    return ;
                }

                return ;

            } 

            // TODO: pass safe to packEface so we don't need to copy if safe==true?
            return packEface(v);

        });

        // IsNil reports whether its argument v is nil. The argument must be
        // a chan, func, interface, map, pointer, or slice value; if it is
        // not, IsNil panics. Note that IsNil is not always equivalent to a
        // regular comparison with nil in Go. For example, if v was created
        // by calling ValueOf with an uninitialized interface variable i,
        // i==nil will be true but v.IsNil will panic as v will be the zero
        // Value.
        public static bool IsNil(this Value v) => func((_, panic, __) =>
        {
            var k = v.kind();

            if (k == Chan || k == Func || k == Map || k == Ptr || k == UnsafePointer) 
                // if v.flag&flagMethod != 0 {
                //     return false
                // }
                var ptr = v.ptr;
                if (v.flag & flagIndir != 0L)
                {
                    ptr = new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
                }

                return ptr == null;
            else if (k == Interface || k == Slice) 
                // Both interface and slice are nil if first word is 0.
                // Both are always bigger than a word; assume flagIndir.
                return new ptr<ptr<ptr<unsafe.Pointer>>>(v.ptr) == null;
                        panic(addr(new ValueError("reflectlite.Value.IsNil",v.kind())));

        });

        // IsValid reports whether v represents a value.
        // It returns false if v is the zero Value.
        // If IsValid returns false, all other methods except String panic.
        // Most functions and methods never return an invalid Value.
        // If one does, its documentation states the conditions explicitly.
        public static bool IsValid(this Value v)
        {
            return v.flag != 0L;
        }

        // Kind returns v's Kind.
        // If v is the zero Value (IsValid returns false), Kind returns Invalid.
        public static Kind Kind(this Value v)
        {
            return v.kind();
        }

        // implemented in runtime:
        private static long chanlen(unsafe.Pointer _p0)
;
        private static long maplen(unsafe.Pointer _p0)
;

        // Len returns v's length.
        // It panics if v's Kind is not Array, Chan, Map, Slice, or String.
        public static long Len(this Value v) => func((_, panic, __) =>
        {
            var k = v.kind();

            if (k == Array) 
                var tt = (arrayType.val)(@unsafe.Pointer(v.typ));
                return int(tt.len);
            else if (k == Chan) 
                return chanlen(v.pointer());
            else if (k == Map) 
                return maplen(v.pointer());
            else if (k == Slice) 
                // Slice is bigger than a word; assume flagIndir.
                return (unsafeheader.Slice.val)(v.ptr).Len;
            else if (k == String) 
                // String is bigger than a word; assume flagIndir.
                return (unsafeheader.String.val)(v.ptr).Len;
                        panic(addr(new ValueError("reflect.Value.Len",v.kind())));

        });

        // NumMethod returns the number of exported methods in the value's method set.
        public static long numMethod(this Value v) => func((_, panic, __) =>
        {
            if (v.typ == null)
            {>>MARKER:FUNCTION_maplen_BLOCK_PREFIX<<
                panic(addr(new ValueError("reflectlite.Value.NumMethod",Invalid)));
            }

            return v.typ.NumMethod();

        });

        // Set assigns x to the value v.
        // It panics if CanSet returns false.
        // As in Go, x's value must be assignable to v's type.
        public static void Set(this Value v, Value x)
        {
            v.mustBeAssignable();
            x.mustBeExported(); // do not let unexported x leak
            unsafe.Pointer target = default;
            if (v.kind() == Interface)
            {>>MARKER:FUNCTION_chanlen_BLOCK_PREFIX<<
                target = v.ptr;
            }

            x = x.assignTo("reflectlite.Set", v.typ, target);
            if (x.flag & flagIndir != 0L)
            {
                typedmemmove(_addr_v.typ, v.ptr, x.ptr);
            }
            else
            {
                (@unsafe.Pointer.val)(v.ptr).val;

                x.ptr;

            }

        }

        // Type returns v's type.
        public static Type Type(this Value v) => func((_, panic, __) =>
        {
            var f = v.flag;
            if (f == 0L)
            {
                panic(addr(new ValueError("reflectlite.Value.Type",Invalid)));
            } 
            // Method values not supported.
            return v.typ;

        });

        /*
         * constructors
         */

        // implemented in package runtime
        private static unsafe.Pointer unsafe_New(ptr<rtype> _p0)
;

        // ValueOf returns a new Value initialized to the concrete value
        // stored in the interface i. ValueOf(nil) returns the zero Value.
        public static Value ValueOf(object i)
        {
            if (i == null)
            {>>MARKER:FUNCTION_unsafe_New_BLOCK_PREFIX<<
                return new Value();
            } 

            // TODO: Maybe allow contents of a Value to live on the stack.
            // For now we make the contents always escape to the heap. It
            // makes life easier in a few places (see chanrecv/mapassign
            // comment below).
            escapes(i);

            return unpackEface(i);

        }

        // assignTo returns a value v that can be assigned directly to typ.
        // It panics if v is not assignable to typ.
        // For a conversion to an interface type, target is a suggested scratch space to use.
        public static Value assignTo(this Value v, @string context, ptr<rtype> _addr_dst, unsafe.Pointer target) => func((_, panic, __) =>
        {
            ref rtype dst = ref _addr_dst.val;
 
            // if v.flag&flagMethod != 0 {
            //     v = makeMethodValue(context, v)
            // }


            if (directlyAssignable(dst, v.typ)) 
                // Overwrite type so that they match.
                // Same memory layout, so no harm done.
                var fl = v.flag & (flagAddr | flagIndir) | v.flag.ro();
                fl |= flag(dst.Kind());
                return new Value(dst,v.ptr,fl);
            else if (implements(dst, v.typ)) 
                if (target == null)
                {
                    target = unsafe_New(_addr_dst);
                }

                if (v.Kind() == Interface && v.IsNil())
                { 
                    // A nil ReadWriter passed to nil Reader is OK,
                    // but using ifaceE2I below will panic.
                    // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
                    return new Value(dst,nil,flag(Interface));

                }

                var x = valueInterface(v);
                if (dst.NumMethod() == 0L)
                {
                }
                else
                {
                    ifaceE2I(_addr_dst, x, target);
                }

                return new Value(dst,target,flagIndir|flag(Interface));
            // Failed.
            panic(context + ": value of type " + v.typ.String() + " is not assignable to type " + dst.String());

        });

        // arrayAt returns the i-th element of p,
        // an array whose elements are eltSize bytes wide.
        // The array pointed at by p must have at least i+1 elements:
        // it is invalid (but impossible to check here) to pass i >= len,
        // because then the result will point outside the array.
        // whySafe must explain why i < len. (Passing "i < len" is fine;
        // the benefit is to surface this assumption at the call site.)
        private static unsafe.Pointer arrayAt(unsafe.Pointer p, long i, System.UIntPtr eltSize, @string whySafe)
        {
            return add(p, uintptr(i) * eltSize, "i < len");
        }

        private static void ifaceE2I(ptr<rtype> t, object src, unsafe.Pointer dst)
;

        // typedmemmove copies a value of type t to dst from src.
        //go:noescape
        private static void typedmemmove(ptr<rtype> t, unsafe.Pointer dst, unsafe.Pointer src)
;

        // Dummy annotation marking that the value x escapes,
        // for use in cases where the reflect code is so clever that
        // the compiler cannot follow.
        private static void escapes(object x)
        {
            if (dummy.b)
            {>>MARKER:FUNCTION_typedmemmove_BLOCK_PREFIX<<
                dummy.x = x;
            }

        }

        private static var dummy = default;
    }
}}
