//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 23:12:55 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using syntax = go.cmd.compile.@internal.syntax_package;
using constant = go.go.constant_package;
using sort = go.sort_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types2_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct valueType
        {
            // Constructors
            public valueType(NilType _)
            {
                this.pos = default;
                this.typ = default;
            }

            public valueType(syntax.Pos pos = default, Type typ = default)
            {
                this.pos = pos;
                this.typ = typ;
            }

            // Enable comparisons between nil and valueType struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(valueType value, NilType nil) => value.Equals(default(valueType));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(valueType value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, valueType value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, valueType value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator valueType(NilType nil) => default(valueType);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static valueType valueType_cast(dynamic value)
        {
            return new valueType(value.pos, value.typ);
        }
    }
}}}}