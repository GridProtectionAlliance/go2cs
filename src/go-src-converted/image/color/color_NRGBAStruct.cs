//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:43:45 UTC
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
namespace image
{
    public static partial class color_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct NRGBA
        {
            // Constructors
            public NRGBA(NilType _)
            {
                this.R = default;
                this.G = default;
                this.B = default;
                this.A = default;
            }

            public NRGBA(byte R = default, byte G = default, byte B = default, byte A = default)
            {
                this.R = R;
                this.G = G;
                this.B = B;
                this.A = A;
            }

            // Enable comparisons between nil and NRGBA struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NRGBA value, NilType nil) => value.Equals(default(NRGBA));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NRGBA value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, NRGBA value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, NRGBA value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NRGBA(NilType nil) => default(NRGBA);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static NRGBA NRGBA_cast(dynamic value)
        {
            return new NRGBA(value.R, value.G, value.B, value.A);
        }
    }
}}