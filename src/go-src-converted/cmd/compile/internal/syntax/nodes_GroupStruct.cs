//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:40:44 UTC
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
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Group
        {
            // Constructors
            public Group(NilType _)
            {
                this.dummy = default;
            }

            public Group(long dummy = default)
            {
                this.dummy = dummy;
            }

            // Enable comparisons between nil and Group struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Group value, NilType nil) => value.Equals(default(Group));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Group value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Group value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Group value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Group(NilType nil) => default(Group);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Group Group_cast(dynamic value)
        {
            return new Group(value.dummy);
        }
    }
}}}}