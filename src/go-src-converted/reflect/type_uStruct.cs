//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:43:13 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = go.@unsafe_package;

namespace go
{
    public static partial class reflect_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        [PromotedStruct(typeof(ptrType))]
        private partial struct u
        {
            // ptrType structure promotion - sourced from value copy
            private readonly ptr<ptrType> m_ptrTypeRef;

            private ref ptrType ptrType_val => ref m_ptrTypeRef.Value;

            public ref ptr<rtype> elem => ref m_ptrTypeRef.Value.elem;

            // Constructors
            public u(NilType _)
            {
                this.m_ptrTypeRef = new ptr<ptrType>(new ptrType(nil));
                this.u = default;
            }

            public u(ptrType ptrType = default, uncommonType u = default)
            {
                this.m_ptrTypeRef = new ptr<ptrType>(ptrType);
                this.u = u;
            }

            // Enable comparisons between nil and u struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(u value, NilType nil) => value.Equals(default(u));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(u value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, u value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, u value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator u(NilType nil) => default(u);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static u u_cast(dynamic value)
        {
            return new u(value.ptrType, value.u);
        }
    }
}