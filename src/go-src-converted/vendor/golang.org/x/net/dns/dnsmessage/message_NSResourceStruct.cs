//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:45:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using errors = go.errors_package;
using go;

#nullable enable

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net {
namespace dns
{
    public static partial class dnsmessage_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct NSResource
        {
            // Constructors
            public NSResource(NilType _)
            {
                this.NS = default;
            }

            public NSResource(Name NS = default)
            {
                this.NS = NS;
            }

            // Enable comparisons between nil and NSResource struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NSResource value, NilType nil) => value.Equals(default(NSResource));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NSResource value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, NSResource value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, NSResource value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator NSResource(NilType nil) => default(NSResource);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static NSResource NSResource_cast(dynamic value)
        {
            return new NSResource(value.NS);
        }
    }
}}}}}}