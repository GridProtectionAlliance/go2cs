//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:52:56 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using typeparams = go.go.@internal.typeparams_package;
using token = go.go.token_package;
using go;

#nullable enable

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct typeDecl
        {
            // Constructors
            public typeDecl(NilType _)
            {
                this.spec = default;
            }

            public typeDecl(ref ptr<ast.TypeSpec> spec = default)
            {
                this.spec = spec;
            }

            // Enable comparisons between nil and typeDecl struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(typeDecl value, NilType nil) => value.Equals(default(typeDecl));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(typeDecl value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, typeDecl value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, typeDecl value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator typeDecl(NilType nil) => default(typeDecl);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static typeDecl typeDecl_cast(dynamic value)
        {
            return new typeDecl(ref value.spec);
        }
    }
}}