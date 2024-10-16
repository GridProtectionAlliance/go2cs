//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:41:03 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using _@unsafe_ = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class time_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Time
        {
            // Constructors
            public Time(NilType _)
            {
                this.wall = default;
                this.ext = default;
                this.loc = default;
            }

            public Time(ulong wall = default, long ext = default, ref ptr<Location> loc = default)
            {
                this.wall = wall;
                this.ext = ext;
                this.loc = loc;
            }

            // Enable comparisons between nil and Time struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Time value, NilType nil) => value.Equals(default(Time));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Time value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Time value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Time value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Time(NilType nil) => default(Time);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Time Time_cast(dynamic value)
        {
            return new Time(value.wall, value.ext, ref value.loc);
        }
    }
}