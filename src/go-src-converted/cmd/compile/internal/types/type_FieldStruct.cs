//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:24:16 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.builtin;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Field
        {
            // Constructors
            public Field(NilType _)
            {
                this.flags = default;
                this.Embedded = default;
                this.Pos = default;
                this.Sym = default;
                this.Type = default;
                this.Note = default;
                this.Nname = default;
                this.Offset = default;
            }

            public Field(bitset8 flags = default, byte Embedded = default, src.XPos Pos = default, ref ptr<Sym> Sym = default, ref ptr<Type> Type = default, @string Note = default, ref ptr<Node> Nname = default, long Offset = default)
            {
                this.flags = flags;
                this.Embedded = Embedded;
                this.Pos = Pos;
                this.Sym = Sym;
                this.Type = Type;
                this.Note = Note;
                this.Nname = Nname;
                this.Offset = Offset;
            }

            // Enable comparisons between nil and Field struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Field value, NilType nil) => value.Equals(default(Field));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Field value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Field value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Field value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Field(NilType nil) => default(Field);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Field Field_cast(dynamic value)
        {
            return new Field(value.flags, value.Embedded, value.Pos, ref value.Sym, ref value.Type, value.Note, ref value.Nname, value.Offset);
        }
    }
}}}}