// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package rand -- go2cs converted at 2020 October 08 03:35:32 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_js.go
using js = go.syscall.js_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        private static void init()
        {
            Reader = addr(new reader());
        }

        private static var jsCrypto = js.Global().Get("crypto");
        private static var uint8Array = js.Global().Get("Uint8Array");

        // reader implements a pseudorandom generator
        // using JavaScript crypto.getRandomValues method.
        // See https://developer.mozilla.org/en-US/docs/Web/API/Crypto/getRandomValues.
        private partial struct reader
        {
        }

        private static (long, error) Read(this ptr<reader> _addr_r, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref reader r = ref _addr_r.val;

            var a = uint8Array.New(len(b));
            jsCrypto.Call("getRandomValues", a);
            js.CopyBytesToGo(b, a);
            return (len(b), error.As(null!)!);
        }
    }
}}
