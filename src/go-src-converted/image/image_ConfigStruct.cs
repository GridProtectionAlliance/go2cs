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
        public partial struct Config
        {
            // Constructors
            public Config(NilType _)
            {
                this.ColorModel = default;
                this.Width = default;
                this.Height = default;
            }

            public Config(color.Model ColorModel = default, nint Width = default, nint Height = default)
            {
                this.ColorModel = ColorModel;
                this.Width = Width;
                this.Height = Height;
            }

            // Enable comparisons between nil and Config struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Config value, NilType nil) => value.Equals(default(Config));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Config value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Config value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Config value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Config(NilType nil) => default(Config);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Config Config_cast(dynamic value)
        {
            return new Config(value.ColorModel, value.Width, value.Height);
        }
    }
}