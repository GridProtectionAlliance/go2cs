//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 05:48:55 UTC
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
namespace link {
namespace @internal
{
    public static partial class sym_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct Symbols
        {
            // Constructors
            public Symbols(NilType _)
            {
                this.versions = default;
                this.Lookup = default;
                this.ROLookup = default;
            }

            public Symbols(long versions = default, Func<@string, long, ptr<Symbol>> Lookup = default, Func<@string, long, ptr<Symbol>> ROLookup = default)
            {
                this.versions = versions;
                this.Lookup = Lookup;
                this.ROLookup = ROLookup;
            }

            // Enable comparisons between nil and Symbols struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Symbols value, NilType nil) => value.Equals(default(Symbols));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Symbols value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Symbols value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Symbols value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Symbols(NilType nil) => default(Symbols);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static Symbols Symbols_cast(dynamic value)
        {
            return new Symbols(value.versions, value.Lookup, value.ROLookup);
        }
    }
}}}}