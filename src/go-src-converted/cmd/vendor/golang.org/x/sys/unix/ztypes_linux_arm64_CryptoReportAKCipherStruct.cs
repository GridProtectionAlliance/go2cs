//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2020 October 09 06:00:37 UTC
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
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        public partial struct CryptoReportAKCipher
        {
            // Constructors
            public CryptoReportAKCipher(NilType _)
            {
                this.Type = default;
            }

            public CryptoReportAKCipher(array<sbyte> Type = default)
            {
                this.Type = Type;
            }

            // Enable comparisons between nil and CryptoReportAKCipher struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(CryptoReportAKCipher value, NilType nil) => value.Equals(default(CryptoReportAKCipher));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(CryptoReportAKCipher value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, CryptoReportAKCipher value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, CryptoReportAKCipher value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator CryptoReportAKCipher(NilType nil) => default(CryptoReportAKCipher);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        public static CryptoReportAKCipher CryptoReportAKCipher_cast(dynamic value)
        {
            return new CryptoReportAKCipher(value.Type);
        }
    }
}}}}}}