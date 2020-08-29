// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file defines operands and associated operations.

// package types -- go2cs converted at 2020 August 29 08:47:47 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\operand.go
using bytes = go.bytes_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // An operandMode specifies the (addressing) mode of an operand.
        private partial struct operandMode // : byte
        {
        }

        private static readonly operandMode invalid = iota; // operand is invalid
        private static readonly var novalue = 0; // operand represents no value (result of a function call w/o result)
        private static readonly var builtin = 1; // operand is a built-in function
        private static readonly var typexpr = 2; // operand is a type
        private static readonly var constant_ = 3; // operand is a constant; the operand's typ is a Basic type
        private static readonly var variable = 4; // operand is an addressable variable
        private static readonly var mapindex = 5; // operand is a map index expression (acts like a variable on lhs, commaok on rhs of an assignment)
        private static readonly var value = 6; // operand is a computed value
        private static readonly var commaok = 7; // like value, but operand may be used in a comma,ok expression

        private static array<@string> operandModeString = new array<@string>(InitKeyedValues<@string>((invalid, "invalid operand"), (novalue, "no value"), (builtin, "built-in"), (typexpr, "type"), (constant_, "constant"), (variable, "variable"), (mapindex, "map index expression"), (value, "value"), (commaok, "comma, ok expression")));

        // An operand represents an intermediate value during type checking.
        // Operands have an (addressing) mode, the expression evaluating to
        // the operand, the operand's type, a value for constants, and an id
        // for built-in functions.
        // The zero value of operand is a ready to use invalid operand.
        //
        private partial struct operand
        {
            public operandMode mode;
            public ast.Expr expr;
            public Type typ;
            public constant.Value val;
            public builtinId id;
        }

        // pos returns the position of the expression corresponding to x.
        // If x is invalid the position is token.NoPos.
        //
        private static token.Pos pos(this ref operand x)
        { 
            // x.expr may not be set if x is invalid
            if (x.expr == null)
            {
                return token.NoPos;
            }
            return x.expr.Pos();
        }

        // Operand string formats
        // (not all "untyped" cases can appear due to the type system,
        // but they fall out naturally here)
        //
        // mode       format
        //
        // invalid    <expr> (               <mode>                    )
        // novalue    <expr> (               <mode>                    )
        // builtin    <expr> (               <mode>                    )
        // typexpr    <expr> (               <mode>                    )
        //
        // constant   <expr> (<untyped kind> <mode>                    )
        // constant   <expr> (               <mode>       of type <typ>)
        // constant   <expr> (<untyped kind> <mode> <val>              )
        // constant   <expr> (               <mode> <val> of type <typ>)
        //
        // variable   <expr> (<untyped kind> <mode>                    )
        // variable   <expr> (               <mode>       of type <typ>)
        //
        // mapindex   <expr> (<untyped kind> <mode>                    )
        // mapindex   <expr> (               <mode>       of type <typ>)
        //
        // value      <expr> (<untyped kind> <mode>                    )
        // value      <expr> (               <mode>       of type <typ>)
        //
        // commaok    <expr> (<untyped kind> <mode>                    )
        // commaok    <expr> (               <mode>       of type <typ>)
        //
        private static @string operandString(ref operand x, Qualifier qf)
        {
            bytes.Buffer buf = default;

            @string expr = default;
            if (x.expr != null)
            {
                expr = ExprString(x.expr);
            }
            else
            {

                if (x.mode == builtin) 
                    expr = predeclaredFuncs[x.id].name;
                else if (x.mode == typexpr) 
                    expr = TypeString(x.typ, qf);
                else if (x.mode == constant_) 
                    expr = x.val.String();
                            } 

            // <expr> (
            if (expr != "")
            {
                buf.WriteString(expr);
                buf.WriteString(" (");
            } 

            // <untyped kind>
            var hasType = false;

            if (x.mode == invalid || x.mode == novalue || x.mode == builtin || x.mode == typexpr)             else 
                // should have a type, but be cautious (don't crash during printing)
                if (x.typ != null)
                {
                    if (isUntyped(x.typ))
                    {
                        buf.WriteString(x.typ._<ref Basic>().name);
                        buf.WriteByte(' ');
                        break;
                    }
                    hasType = true;
                }
            // <mode>
            buf.WriteString(operandModeString[x.mode]); 

            // <val>
            if (x.mode == constant_)
            {
                {
                    var s = x.val.String();

                    if (s != expr)
                    {
                        buf.WriteByte(' ');
                        buf.WriteString(s);
                    }

                }
            } 

            // <typ>
            if (hasType)
            {
                if (x.typ != Typ[Invalid])
                {
                    buf.WriteString(" of type ");
                    WriteType(ref buf, x.typ, qf);
                }
                else
                {
                    buf.WriteString(" with invalid type");
                }
            } 

            // )
            if (expr != "")
            {
                buf.WriteByte(')');
            }
            return buf.String();
        }

        private static @string String(this ref operand x)
        {
            return operandString(x, null);
        }

        // setConst sets x to the untyped constant for literal lit.
        private static void setConst(this ref operand x, token.Token tok, @string lit)
        {
            BasicKind kind = default;

            if (tok == token.INT) 
                kind = UntypedInt;
            else if (tok == token.FLOAT) 
                kind = UntypedFloat;
            else if (tok == token.IMAG) 
                kind = UntypedComplex;
            else if (tok == token.CHAR) 
                kind = UntypedRune;
            else if (tok == token.STRING) 
                kind = UntypedString;
            else 
                unreachable();
                        x.mode = constant_;
            x.typ = Typ[kind];
            x.val = constant.MakeFromLiteral(lit, tok, 0L);
        }

        // isNil reports whether x is the nil value.
        private static bool isNil(this ref operand x)
        {
            return x.mode == value && x.typ == Typ[UntypedNil];
        }

        // TODO(gri) The functions operand.assignableTo, checker.convertUntyped,
        //           checker.representable, and checker.assignment are
        //           overlapping in functionality. Need to simplify and clean up.

        // assignableTo reports whether x is assignable to a variable of type T.
        // If the result is false and a non-nil reason is provided, it may be set
        // to a more detailed explanation of the failure (result != "").
        private static bool assignableTo(this ref operand x, ref Config conf, Type T, ref @string reason)
        {
            if (x.mode == invalid || T == Typ[Invalid])
            {
                return true; // avoid spurious errors
            }
            var V = x.typ; 

            // x's type is identical to T
            if (Identical(V, T))
            {
                return true;
            }
            var Vu = V.Underlying();
            var Tu = T.Underlying(); 

            // x is an untyped value representable by a value of type T
            // TODO(gri) This is borrowing from checker.convertUntyped and
            //           checker.representable. Need to clean up.
            if (isUntyped(Vu))
            {
                switch (Tu.type())
                {
                    case ref Basic t:
                        if (x.isNil() && t.kind == UnsafePointer)
                        {
                            return true;
                        }
                        if (x.mode == constant_)
                        {
                            return representableConst(x.val, conf, t, null);
                        } 
                        // The result of a comparison is an untyped boolean,
                        // but may not be a constant.
                        {
                            ref Basic (Vb, _) = Vu._<ref Basic>();

                            if (Vb != null)
                            {
                                return Vb.kind == UntypedBool && isBoolean(Tu);
                            }

                        }
                        break;
                    case ref Interface t:
                        return x.isNil() || t.Empty();
                        break;
                    case ref Pointer t:
                        return x.isNil();
                        break;
                    case ref Signature t:
                        return x.isNil();
                        break;
                    case ref Slice t:
                        return x.isNil();
                        break;
                    case ref Map t:
                        return x.isNil();
                        break;
                    case ref Chan t:
                        return x.isNil();
                        break;
                }
            } 
            // Vu is typed

            // x's type V and T have identical underlying types
            // and at least one of V or T is not a named type
            if (Identical(Vu, Tu) && (!isNamed(V) || !isNamed(T)))
            {
                return true;
            } 

            // T is an interface type and x implements T
            {
                ref Interface (Ti, ok) = Tu._<ref Interface>();

                if (ok)
                {
                    {
                        var (m, wrongType) = MissingMethod(x.typ, Ti, true);

                        if (m != null)
                        {
                            if (reason != null)
                            {
                                if (wrongType)
                                {
                                    reason.Value = "wrong type for method " + m.Name();
                                }
                                else
                                {
                                    reason.Value = "missing method " + m.Name();
                                }
                            }
                            return false;
                        }

                    }
                    return true;
                } 

                // x is a bidirectional channel value, T is a channel
                // type, x's type V and T have identical element types,
                // and at least one of V or T is not a named type

            } 

            // x is a bidirectional channel value, T is a channel
            // type, x's type V and T have identical element types,
            // and at least one of V or T is not a named type
            {
                ref Chan (Vc, ok) = Vu._<ref Chan>();

                if (ok && Vc.dir == SendRecv)
                {
                    {
                        ref Chan (Tc, ok) = Tu._<ref Chan>();

                        if (ok && Identical(Vc.elem, Tc.elem))
                        {
                            return !isNamed(V) || !isNamed(T);
                        }

                    }
                }

            }

            return false;
        }
    }
}}
