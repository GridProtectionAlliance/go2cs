//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:49:15 UTC
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
    public static partial class ir_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct bottomUpVisitor
        {
            // Constructors
            public bottomUpVisitor(NilType _)
            {
                this.analyze = default;
                this.visitgen = default;
                this.nodeID = default;
                this.stack = default;
            }

            public bottomUpVisitor(Action<slice<ptr<Func>>, bool> analyze = default, uint visitgen = default, map<ptr<Func>, uint> nodeID = default, slice<ptr<Func>> stack = default)
            {
                this.analyze = analyze;
                this.visitgen = visitgen;
                this.nodeID = nodeID;
                this.stack = stack;
            }

            // Enable comparisons between nil and bottomUpVisitor struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(bottomUpVisitor value, NilType nil) => value.Equals(default(bottomUpVisitor));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(bottomUpVisitor value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, bottomUpVisitor value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, bottomUpVisitor value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator bottomUpVisitor(NilType nil) => default(bottomUpVisitor);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static bottomUpVisitor bottomUpVisitor_cast(dynamic value)
        {
            return new bottomUpVisitor(value.analyze, value.visitgen, value.nodeID, value.stack);
        }
    }
}}}}