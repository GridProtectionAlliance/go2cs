//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:00:39 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ir_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(miniType))]
        public partial struct SliceType
        {
            // miniType structure promotion - sourced from value copy
            private readonly ptr<miniType> m_miniTypeRef;

            private ref miniType miniType_val => ref m_miniTypeRef.Value;

            public ref ptr<types.Type> typ => ref m_miniTypeRef.Value.typ;

            // Constructors
            public SliceType(NilType _)
            {
                this.m_miniTypeRef = new ptr<miniType>(new miniType(nil));
                this.Elem = default;
                this.DDD = default;
            }

            public SliceType(miniType miniType = default, Ntype Elem = default, bool DDD = default)
            {
                this.m_miniTypeRef = new ptr<miniType>(miniType);
                this.Elem = Elem;
                this.DDD = DDD;
            }

            // Enable comparisons between nil and SliceType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SliceType value, NilType nil) => value.Equals(default(SliceType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SliceType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SliceType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SliceType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SliceType(NilType nil) => default(SliceType);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static SliceType SliceType_cast(dynamic value)
        {
            return new SliceType(value.miniType, value.Elem, value.DDD);
        }
    }
}}}}