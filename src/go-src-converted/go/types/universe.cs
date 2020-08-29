// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file sets up the universe scope and the unsafe package.

// package types -- go2cs converted at 2020 August 29 08:48:08 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\universe.go
using constant = go.go.constant_package;
using token = go.go.token_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        public static ref Scope Universe = default;        public static ref Package Unsafe = default;        private static ref Const universeIota = default;        private static ref Basic universeByte = default;        private static ref Basic universeRune = default;

        // Typ contains the predeclared *Basic types indexed by their
        // corresponding BasicKind.
        //
        // The *Basic type for Typ[Byte] will have the name "uint8".
        // Use Universe.Lookup("byte").Type() to obtain the specific
        // alias basic type named "byte" (and analogous for "rune").
        public static ref Basic Typ = new slice<ref Basic>(InitKeyedValues<ref Basic>((Invalid, {Invalid,0,"invalid type"}), (Bool, {Bool,IsBoolean,"bool"}), (Int, {Int,IsInteger,"int"}), (Int8, {Int8,IsInteger,"int8"}), (Int16, {Int16,IsInteger,"int16"}), (Int32, {Int32,IsInteger,"int32"}), (Int64, {Int64,IsInteger,"int64"}), (Uint, {Uint,IsInteger|IsUnsigned,"uint"}), (Uint8, {Uint8,IsInteger|IsUnsigned,"uint8"}), (Uint16, {Uint16,IsInteger|IsUnsigned,"uint16"}), (Uint32, {Uint32,IsInteger|IsUnsigned,"uint32"}), (Uint64, {Uint64,IsInteger|IsUnsigned,"uint64"}), (Uintptr, {Uintptr,IsInteger|IsUnsigned,"uintptr"}), (Float32, {Float32,IsFloat,"float32"}), (Float64, {Float64,IsFloat,"float64"}), (Complex64, {Complex64,IsComplex,"complex64"}), (Complex128, {Complex128,IsComplex,"complex128"}), (String, {String,IsString,"string"}), (UnsafePointer, {UnsafePointer,0,"Pointer"}), (UntypedBool, {UntypedBool,IsBoolean|IsUntyped,"untyped bool"}), (UntypedInt, {UntypedInt,IsInteger|IsUntyped,"untyped int"}), (UntypedRune, {UntypedRune,IsInteger|IsUntyped,"untyped rune"}), (UntypedFloat, {UntypedFloat,IsFloat|IsUntyped,"untyped float"}), (UntypedComplex, {UntypedComplex,IsComplex|IsUntyped,"untyped complex"}), (UntypedString, {UntypedString,IsString|IsUntyped,"untyped string"}), (UntypedNil, {UntypedNil,IsUntyped,"untyped nil"})));

        private static array<ref Basic> aliases = new array<ref Basic>(new ref Basic[] { {Byte,IsInteger|IsUnsigned,"byte"}, {Rune,IsInteger,"rune"} });

        private static void defPredeclaredTypes()
        {
            {
                var t__prev1 = t;

                foreach (var (_, __t) in Typ)
                {
                    t = __t;
                    def(NewTypeName(token.NoPos, null, t.name, t));
                }

                t = t__prev1;
            }

            {
                var t__prev1 = t;

                foreach (var (_, __t) in aliases)
                {
                    t = __t;
                    def(NewTypeName(token.NoPos, null, t.name, t));
                } 

                // Error has a nil package in its qualified name since it is in no package

                t = t__prev1;
            }

            var res = NewVar(token.NoPos, null, "", Typ[String]);
            Signature sig = ref new Signature(results:NewTuple(res));
            var err = NewFunc(token.NoPos, null, "Error", sig);
            Named typ = ref new Named(underlying:NewInterface([]*Func{err},nil).Complete());
            sig.recv = NewVar(token.NoPos, null, "", typ);
            def(NewTypeName(token.NoPos, null, "error", typ));
        }



        private static void defPredeclaredConsts()
        {
            foreach (var (_, c) in predeclaredConsts)
            {
                def(NewConst(token.NoPos, null, c.name, Typ[c.kind], c.val));
            }
        }

        private static void defPredeclaredNil()
        {
            def(ref new Nil(object{name:"nil",typ:Typ[UntypedNil]}));
        }

        // A builtinId is the id of a builtin function.
        private partial struct builtinId // : long
        {
        }

 
        // universe scope
        private static readonly builtinId _Append = iota;
        private static readonly var _Cap = 0;
        private static readonly var _Close = 1;
        private static readonly var _Complex = 2;
        private static readonly var _Copy = 3;
        private static readonly var _Delete = 4;
        private static readonly var _Imag = 5;
        private static readonly var _Len = 6;
        private static readonly var _Make = 7;
        private static readonly var _New = 8;
        private static readonly var _Panic = 9;
        private static readonly var _Print = 10;
        private static readonly var _Println = 11;
        private static readonly var _Real = 12;
        private static readonly var _Recover = 13; 

        // package unsafe
        private static readonly var _Alignof = 14;
        private static readonly var _Offsetof = 15;
        private static readonly var _Sizeof = 16; 

        // testing support
        private static readonly var _Assert = 17;
        private static readonly var _Trace = 18;



        private static void defPredeclaredFuncs()
        {
            foreach (var (i) in predeclaredFuncs)
            {
                var id = builtinId(i);
                if (id == _Assert || id == _Trace)
                {
                    continue; // only define these in testing environment
                }
                def(newBuiltin(id));
            }
        }

        // DefPredeclaredTestFuncs defines the assert and trace built-ins.
        // These built-ins are intended for debugging and testing of this
        // package only.
        public static void DefPredeclaredTestFuncs()
        {
            if (Universe.Lookup("assert") != null)
            {
                return; // already defined
            }
            def(newBuiltin(_Assert));
            def(newBuiltin(_Trace));
        }

        private static void init()
        {
            Universe = NewScope(null, token.NoPos, token.NoPos, "universe");
            Unsafe = NewPackage("unsafe", "unsafe");
            Unsafe.complete = true;

            defPredeclaredTypes();
            defPredeclaredConsts();
            defPredeclaredNil();
            defPredeclaredFuncs();

            universeIota = Universe.Lookup("iota")._<ref Const>();
            universeByte = Universe.Lookup("byte")._<ref TypeName>().typ._<ref Basic>();
            universeRune = Universe.Lookup("rune")._<ref TypeName>().typ._<ref Basic>();
        }

        // Objects with names containing blanks are internal and not entered into
        // a scope. Objects with exported names are inserted in the unsafe package
        // scope; other objects are inserted in the universe scope.
        //
        private static void def(Object obj) => func((_, panic, __) =>
        {
            var name = obj.Name();
            if (strings.Contains(name, " "))
            {
                return; // nothing to do
            } 
            // fix Obj link for named types
            {
                ref Named (typ, ok) = obj.Type()._<ref Named>();

                if (ok)
                {
                    typ.obj = obj._<ref TypeName>();
                } 
                // exported identifiers go into package unsafe

            } 
            // exported identifiers go into package unsafe
            var scope = Universe;
            if (obj.Exported())
            {
                scope = Unsafe.scope; 
                // set Pkg field
                switch (obj.type())
                {
                    case ref TypeName obj:
                        obj.pkg = Unsafe;
                        break;
                    case ref Builtin obj:
                        obj.pkg = Unsafe;
                        break;
                    default:
                    {
                        var obj = obj.type();
                        unreachable();
                        break;
                    }
                }
            }
            if (scope.Insert(obj) != null)
            {
                panic("internal error: double declaration");
            }
        });
    }
}}
