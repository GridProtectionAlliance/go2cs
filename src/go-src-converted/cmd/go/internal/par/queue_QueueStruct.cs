//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:30:10 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using fmt = go.fmt_package;
using go;

#nullable enable

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class par_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct Queue
        {
            // Constructors
            public Queue(NilType _)
            {
                this.maxActive = default;
                this.st = default;
            }

            public Queue(nint maxActive = default, channel<queueState> st = default)
            {
                this.maxActive = maxActive;
                this.st = st;
            }

            // Enable comparisons between nil and Queue struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Queue value, NilType nil) => value.Equals(default(Queue));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Queue value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, Queue value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, Queue value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Queue(NilType nil) => default(Queue);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static Queue Queue_cast(dynamic value)
        {
            return new Queue(value.maxActive, value.st);
        }
    }
}}}}