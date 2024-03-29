//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:29 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using bytes = go.bytes_package;
using list = go.container.list_package;
using context = go.context_package;
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using rsa = go.crypto.rsa_package;
using sha512 = go.crypto.sha512_package;
using x509 = go.crypto.x509_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct ClientHelloInfo
        {
            // Constructors
            public ClientHelloInfo(NilType _)
            {
                this.CipherSuites = default;
                this.ServerName = default;
                this.SupportedCurves = default;
                this.SupportedPoints = default;
                this.SignatureSchemes = default;
                this.SupportedProtos = default;
                this.SupportedVersions = default;
                this.Conn = default;
                this.config = default;
                this.ctx = default;
            }

            public ClientHelloInfo(slice<ushort> CipherSuites = default, @string ServerName = default, slice<CurveID> SupportedCurves = default, slice<byte> SupportedPoints = default, slice<SignatureScheme> SignatureSchemes = default, slice<@string> SupportedProtos = default, slice<ushort> SupportedVersions = default, net.Conn Conn = default, ref ptr<Config> config = default, context.Context ctx = default)
            {
                this.CipherSuites = CipherSuites;
                this.ServerName = ServerName;
                this.SupportedCurves = SupportedCurves;
                this.SupportedPoints = SupportedPoints;
                this.SignatureSchemes = SignatureSchemes;
                this.SupportedProtos = SupportedProtos;
                this.SupportedVersions = SupportedVersions;
                this.Conn = Conn;
                this.config = config;
                this.ctx = ctx;
            }

            // Enable comparisons between nil and ClientHelloInfo struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(ClientHelloInfo value, NilType nil) => value.Equals(default(ClientHelloInfo));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(ClientHelloInfo value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, ClientHelloInfo value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, ClientHelloInfo value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ClientHelloInfo(NilType nil) => default(ClientHelloInfo);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static ClientHelloInfo ClientHelloInfo_cast(dynamic value)
        {
            return new ClientHelloInfo(value.CipherSuites, value.ServerName, value.SupportedCurves, value.SupportedPoints, value.SignatureSchemes, value.SupportedProtos, value.SupportedVersions, value.Conn, ref value.config, value.ctx);
        }
    }
}}