//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 04:45:53 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;


#nullable enable

namespace go
{
    public static partial class runtime_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct floatingsavearea
        {
            // Constructors
            public floatingsavearea(NilType _)
            {
                this.controlword = default;
                this.statusword = default;
                this.tagword = default;
                this.erroroffset = default;
                this.errorselector = default;
                this.dataoffset = default;
                this.dataselector = default;
                this.registerarea = default;
                this.cr0npxstate = default;
            }

            public floatingsavearea(uint controlword = default, uint statusword = default, uint tagword = default, uint erroroffset = default, uint errorselector = default, uint dataoffset = default, uint dataselector = default, array<byte> registerarea = default, uint cr0npxstate = default)
            {
                this.controlword = controlword;
                this.statusword = statusword;
                this.tagword = tagword;
                this.erroroffset = erroroffset;
                this.errorselector = errorselector;
                this.dataoffset = dataoffset;
                this.dataselector = dataselector;
                this.registerarea = registerarea;
                this.cr0npxstate = cr0npxstate;
            }

            // Enable comparisons between nil and floatingsavearea struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(floatingsavearea value, NilType nil) => value.Equals(default(floatingsavearea));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(floatingsavearea value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, floatingsavearea value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, floatingsavearea value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator floatingsavearea(NilType nil) => default(floatingsavearea);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static floatingsavearea floatingsavearea_cast(dynamic value)
        {
            return new floatingsavearea(value.controlword, value.statusword, value.tagword, value.erroroffset, value.errorselector, value.dataoffset, value.dataselector, value.registerarea, value.cr0npxstate);
        }
    }
}