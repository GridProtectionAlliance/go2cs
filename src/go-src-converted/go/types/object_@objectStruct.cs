//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 August 29 08:47:45 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using go;

namespace go {
namespace go
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct @object
        {
            // Constructors
            public @object(NilType _)
            {
                this.parent = default;
                this.pos = default;
                this.pkg = default;
                this.name = default;
                this.typ = default;
                this.order_ = default;
                this.scopePos_ = default;
            }

            public @object(ref ptr<Scope> parent = default, token.Pos pos = default, ref ptr<Package> pkg = default, @string name = default, Type typ = default, uint order_ = default, token.Pos scopePos_ = default)
            {
                this.parent = parent;
                this.pos = pos;
                this.pkg = pkg;
                this.name = name;
                this.typ = typ;
                this.order_ = order_;
                this.scopePos_ = scopePos_;
            }

            // Enable comparisons between nil and @object struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(@object value, NilType nil) => value.Equals(default(@object));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(@object value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, @object value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, @object value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator @object(NilType nil) => default(@object);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static @object @object_cast(dynamic value)
        {
            return new @object(ref value.parent, value.pos, ref value.pkg, value.name, value.typ, value.order_, value.scopePos_);
        }
    }
}}