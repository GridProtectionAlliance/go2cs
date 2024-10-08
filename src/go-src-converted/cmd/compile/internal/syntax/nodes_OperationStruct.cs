//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:26:36 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(expr))]
        public partial struct Operation
        {
            // expr structure promotion - sourced from value copy
            private readonly ptr<expr> m_exprRef;

            private ref expr expr_val => ref m_exprRef.Value;

            // Constructors
            public Operation(NilType _)
            {
                this.Op = default;
                this.X = default;
                this.Y = default;
                this.m_exprRef = new ptr<expr>(new expr(nil));
            }

            public Operation(Operator Op = default, Expr X = default, Expr Y = default, expr expr = default)
            {
                this.Op = Op;
                this.X = X;
                this.Y = Y;
                this.m_exprRef = new ptr<expr>(expr);
            }

            // Enable comparisons between nil and Operation struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Operation value, NilType nil) => value.Equals(default(Operation));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Operation value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Operation value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Operation value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Operation(NilType nil) => default(Operation);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Operation Operation_cast(dynamic value)
        {
            return new Operation(value.Op, value.X, value.Y, value.expr);
        }
    }
}}}}