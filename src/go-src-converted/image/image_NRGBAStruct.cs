//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:43:54 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using color = go.image.color_package;

#nullable enable

namespace go
{
    public static partial class image_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct NRGBA
        {
            // Constructors
            public NRGBA(NilType _)
            {
                this.Pix = default;
                this.Stride = default;
                this.Rect = default;
            }

            public NRGBA(slice<byte> Pix = default, nint Stride = default, Rectangle Rect = default)
            {
                this.Pix = Pix;
                this.Stride = Stride;
                this.Rect = Rect;
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
            return new NRGBA(value.Pix, value.Stride, value.Rect);
        }
    }
}