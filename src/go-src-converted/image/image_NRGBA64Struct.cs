//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:05:42 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using color = go.image.color_package;

#nullable enable

namespace go
{
    public static partial class image_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct NRGBA64
        {
            // Constructors
            public NRGBA64(NilType _)
            {
                this.Pix = default;
                this.Stride = default;
                this.Rect = default;
            }

            public NRGBA64(slice<byte> Pix = default, long Stride = default, Rectangle Rect = default)
            {
                this.Pix = Pix;
                this.Stride = Stride;
                this.Rect = Rect;
            }

            // Enable comparisons between nil and NRGBA64 struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NRGBA64 value, NilType nil) => value.Equals(default(NRGBA64));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NRGBA64 value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, NRGBA64 value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, NRGBA64 value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NRGBA64(NilType nil) => default(NRGBA64);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static NRGBA64 NRGBA64_cast(dynamic value)
        {
            return new NRGBA64(value.Pix, value.Stride, value.Rect);
        }
    }
}