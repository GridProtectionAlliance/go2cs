//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:53 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using net = go.net_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct UnknownAuthorityError
        {
            // Constructors
            public UnknownAuthorityError(NilType _)
            {
                this.Cert = default;
                this.hintErr = default;
                this.hintCert = default;
            }

            public UnknownAuthorityError(ref ptr<Certificate> Cert = default, error hintErr = default, ref ptr<Certificate> hintCert = default)
            {
                this.Cert = Cert;
                this.hintErr = hintErr;
                this.hintCert = hintCert;
            }

            // Enable comparisons between nil and UnknownAuthorityError struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(UnknownAuthorityError value, NilType nil) => value.Equals(default(UnknownAuthorityError));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(UnknownAuthorityError value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, UnknownAuthorityError value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, UnknownAuthorityError value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator UnknownAuthorityError(NilType nil) => default(UnknownAuthorityError);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static UnknownAuthorityError UnknownAuthorityError_cast(dynamic value)
        {
            return new UnknownAuthorityError(ref value.Cert, value.hintErr, ref value.hintCert);
        }
    }
}}