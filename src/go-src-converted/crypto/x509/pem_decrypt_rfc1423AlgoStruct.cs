//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:34:46 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using md5 = go.crypto.md5_package;
using hex = go.encoding.hex_package;
using pem = go.encoding.pem_package;
using errors = go.errors_package;
using io = go.io_package;
using strings = go.strings_package;
using go;

#nullable enable

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        private partial struct rfc1423Algo
        {
            // Constructors
            public rfc1423Algo(NilType _)
            {
                this.cipher = default;
                this.name = default;
                this.cipherFunc = default;
                this.keySize = default;
                this.blockSize = default;
            }

            public rfc1423Algo(PEMCipher cipher = default, @string name = default, Func<slice<byte>, (cipher.Block, error)> cipherFunc = default, nint keySize = default, nint blockSize = default)
            {
                this.cipher = cipher;
                this.name = name;
                this.cipherFunc = cipherFunc;
                this.keySize = keySize;
                this.blockSize = blockSize;
            }

            // Enable comparisons between nil and rfc1423Algo struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(rfc1423Algo value, NilType nil) => value.Equals(default(rfc1423Algo));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(rfc1423Algo value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, rfc1423Algo value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, rfc1423Algo value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator rfc1423Algo(NilType nil) => default(rfc1423Algo);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static rfc1423Algo rfc1423Algo_cast(dynamic value)
        {
            return new rfc1423Algo(value.cipher, value.name, value.cipherFunc, value.keySize, value.blockSize);
        }
    }
}}