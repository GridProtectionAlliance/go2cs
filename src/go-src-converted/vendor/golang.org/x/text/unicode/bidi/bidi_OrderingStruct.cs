//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:46:38 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class bidi_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Ordering
        {
            // Constructors
            public Ordering(NilType _)
            {
                this.runes = default;
                this.directions = default;
                this.startpos = default;
            }

            public Ordering(slice<slice<int>> runes = default, slice<Direction> directions = default, slice<nint> startpos = default)
            {
                this.runes = runes;
                this.directions = directions;
                this.startpos = startpos;
            }

            // Enable comparisons between nil and Ordering struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Ordering value, NilType nil) => value.Equals(default(Ordering));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Ordering value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Ordering value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Ordering value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Ordering(NilType nil) => default(Ordering);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Ordering Ordering_cast(dynamic value)
        {
            return new Ordering(value.runes, value.directions, value.startpos);
        }
    }
}}}}}}