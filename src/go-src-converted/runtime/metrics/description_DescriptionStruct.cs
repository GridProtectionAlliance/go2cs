//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:28:33 UTC
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
namespace runtime
{
    public static partial class metrics_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Description
        {
            // Constructors
            public Description(NilType _)
            {
                this.Name = default;
                this.Description = default;
                this.Kind = default;
                this.Cumulative = default;
            }

            public Description(@string Name = default, @string Description = default, ValueKind Kind = default, bool Cumulative = default)
            {
                this.Name = Name;
                this.Description = Description;
                this.Kind = Kind;
                this.Cumulative = Cumulative;
            }

            // Enable comparisons between nil and Description struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Description value, NilType nil) => value.Equals(default(Description));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Description value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Description value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Description value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Description(NilType nil) => default(Description);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Description Description_cast(dynamic value)
        {
            return new Description(value.Name, value.Description, value.Kind, value.Cumulative);
        }
    }
}}