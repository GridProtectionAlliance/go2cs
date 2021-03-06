//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:03:30 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using sync = go.sync_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using go;

#nullable enable

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        [PromotedStruct(typeof(register))]
        public partial struct Range
        {
            // register structure promotion - sourced from value copy
            private readonly ptr<register> m_registerRef;

            private ref register register_val => ref m_registerRef.Value;

            public ref long num => ref m_registerRef.Value.num;

            public ref types.Type typ => ref m_registerRef.Value.typ;

            public ref token.Pos pos => ref m_registerRef.Value.pos;

            public ref slice<Instruction> referrers => ref m_registerRef.Value.referrers;

            // Constructors
            public Range(NilType _)
            {
                this.m_registerRef = new ptr<register>(new register(nil));
                this.X = default;
            }

            public Range(register register = default, Value X = default)
            {
                this.m_registerRef = new ptr<register>(register);
                this.X = X;
            }

            // Enable comparisons between nil and Range struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Range value, NilType nil) => value.Equals(default(Range));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Range value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Range value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Range value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Range(NilType nil) => default(Range);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Range Range_cast(dynamic value)
        {
            return new Range(value.register, value.X);
        }
    }
}}}}}