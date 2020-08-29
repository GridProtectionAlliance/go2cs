// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of conversions.

// package types -- go2cs converted at 2020 August 29 08:47:25 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\conversions.go
using constant = go.go.constant_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // Conversion type-checks the conversion T(x).
        // The result is in x.
        private static void conversion(this ref Checker check, ref operand x, Type T)
        {
            var constArg = x.mode == constant_;

            bool ok = default;

            if (constArg && isConstType(T)) 
                // constant conversion
                {
                    ref Basic t = T.Underlying()._<ref Basic>();


                    if (representableConst(x.val, check.conf, t, ref x.val)) 
                        ok = true;
                    else if (isInteger(x.typ) && isString(t)) 
                        var codepoint = int64(-1L);
                        {
                            var (i, ok) = constant.Int64Val(x.val);

                            if (ok)
                            {
                                codepoint = i;
                            }
                        } 
                        // If codepoint < 0 the absolute value is too large (or unknown) for
                        // conversion. This is the same as converting any other out-of-range
                        // value - let string(codepoint) do the work.
                        x.val = constant.MakeString(string(codepoint));
                        ok = true;

                }
            else if (x.convertibleTo(check.conf, T)) 
                // non-constant conversion
                x.mode = value;
                ok = true;
                        if (!ok)
            {
                check.errorf(x.pos(), "cannot convert %s to %s", x, T);
                x.mode = invalid;
                return;
            }
            if (isUntyped(x.typ))
            {
                var final = T; 
                // - For conversions to interfaces, use the argument's default type.
                // - For conversions of untyped constants to non-constant types, also
                //   use the default type (e.g., []byte("foo") should report string
                //   not []byte as type for the constant "foo").
                // - Keep untyped nil for untyped nil arguments.
                // - For integer to string conversions, keep the argument type.
                //   (See also the TODO below.)
                if (IsInterface(T) || constArg && !isConstType(T))
                {
                    final = Default(x.typ);
                }
                else if (isInteger(x.typ) && isString(T))
                {
                    final = x.typ;
                }
                check.updateExprType(x.expr, final, true);
            }
            x.typ = T;
        }

        // TODO(gri) convertibleTo checks if T(x) is valid. It assumes that the type
        // of x is fully known, but that's not the case for say string(1<<s + 1.0):
        // Here, the type of 1<<s + 1.0 will be UntypedFloat which will lead to the
        // (correct!) refusal of the conversion. But the reported error is essentially
        // "cannot convert untyped float value to string", yet the correct error (per
        // the spec) is that we cannot shift a floating-point value: 1 in 1<<s should
        // be converted to UntypedFloat because of the addition of 1.0. Fixing this
        // is tricky because we'd have to run updateExprType on the argument first.
        // (Issue #21982.)

        private static bool convertibleTo(this ref operand x, ref Config conf, Type T)
        { 
            // "x is assignable to T"
            if (x.assignableTo(conf, T, null))
            {
                return true;
            } 

            // "x's type and T have identical underlying types if tags are ignored"
            var V = x.typ;
            var Vu = V.Underlying();
            var Tu = T.Underlying();
            if (IdenticalIgnoreTags(Vu, Tu))
            {
                return true;
            } 

            // "x's type and T are unnamed pointer types and their pointer base types
            // have identical underlying types if tags are ignored"
            {
                var V__prev1 = V;

                ref Pointer (V, ok) = V._<ref Pointer>();

                if (ok)
                {
                    {
                        ref Pointer (T, ok) = T._<ref Pointer>();

                        if (ok)
                        {
                            if (IdenticalIgnoreTags(V.@base.Underlying(), T.@base.Underlying()))
                            {
                                return true;
                            }
                        }

                    }
                } 

                // "x's type and T are both integer or floating point types"

                V = V__prev1;

            } 

            // "x's type and T are both integer or floating point types"
            if ((isInteger(V) || isFloat(V)) && (isInteger(T) || isFloat(T)))
            {
                return true;
            } 

            // "x's type and T are both complex types"
            if (isComplex(V) && isComplex(T))
            {
                return true;
            } 

            // "x is an integer or a slice of bytes or runes and T is a string type"
            if ((isInteger(V) || isBytesOrRunes(Vu)) && isString(T))
            {
                return true;
            } 

            // "x is a string and T is a slice of bytes or runes"
            if (isString(V) && isBytesOrRunes(Tu))
            {
                return true;
            } 

            // package unsafe:
            // "any pointer or value of underlying type uintptr can be converted into a unsafe.Pointer"
            if ((isPointer(Vu) || isUintptr(Vu)) && isUnsafePointer(T))
            {
                return true;
            } 
            // "and vice versa"
            if (isUnsafePointer(V) && (isPointer(Tu) || isUintptr(Tu)))
            {
                return true;
            }
            return false;
        }

        private static bool isUintptr(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.kind == Uintptr;
        }

        private static bool isUnsafePointer(Type typ)
        { 
            // TODO(gri): Is this (typ.Underlying() instead of just typ) correct?
            //            The spec does not say so, but gc claims it is. See also
            //            issue 6326.
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.kind == UnsafePointer;
        }

        private static bool isPointer(Type typ)
        {
            ref Pointer (_, ok) = typ.Underlying()._<ref Pointer>();
            return ok;
        }

        private static bool isBytesOrRunes(Type typ)
        {
            {
                ref Slice (s, ok) = typ._<ref Slice>();

                if (ok)
                {
                    ref Basic (t, ok) = s.elem.Underlying()._<ref Basic>();
                    return ok && (t.kind == Byte || t.kind == Rune);
                }

            }
            return false;
        }
    }
}}
