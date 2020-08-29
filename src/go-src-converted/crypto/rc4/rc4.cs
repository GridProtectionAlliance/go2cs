// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rc4 implements RC4 encryption, as defined in Bruce Schneier's
// Applied Cryptography.
//
// RC4 is cryptographically broken and should not be used for secure
// applications.
// package rc4 -- go2cs converted at 2020 August 29 08:28:34 UTC
// import "crypto/rc4" ==> using rc4 = go.crypto.rc4_package
// Original source: C:\Go\src\crypto\rc4\rc4.go
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rc4_package
    {
        // A Cipher is an instance of RC4 using a particular key.
        public partial struct Cipher
        {
            public array<uint> s;
            public byte i;
            public byte j;
        }

        public partial struct KeySizeError // : long
        {
        }

        public static @string Error(this KeySizeError k)
        {
            return "crypto/rc4: invalid key size " + strconv.Itoa(int(k));
        }

        // NewCipher creates and returns a new Cipher. The key argument should be the
        // RC4 key, at least 1 byte and at most 256 bytes.
        public static (ref Cipher, error) NewCipher(slice<byte> key)
        {
            var k = len(key);
            if (k < 1L || k > 256L)
            {
                return (null, KeySizeError(k));
            }
            Cipher c = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < 256L; i++)
                {
                    c.s[i] = uint32(i);
                }


                i = i__prev1;
            }
            byte j = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < 256L; i++)
                {
                    j += uint8(c.s[i]) + key[i % k];
                    c.s[i] = c.s[j];
                    c.s[j] = c.s[i];
                }


                i = i__prev1;
            }
            return (ref c, null);
        }

        // Reset zeros the key data so that it will no longer appear in the
        // process's memory.
        private static void Reset(this ref Cipher c)
        {
            foreach (var (i) in c.s)
            {
                c.s[i] = 0L;
            }
            c.i = 0L;
            c.j = 0L;
        }

        // xorKeyStreamGeneric sets dst to the result of XORing src with the
        // key stream. Dst and src must overlap entirely or not at all.
        //
        // This is the pure Go version. rc4_{amd64,386,arm}* contain assembly
        // implementations. This is here for tests and to prevent bitrot.
        private static void xorKeyStreamGeneric(this ref Cipher c, slice<byte> dst, slice<byte> src)
        {
            var i = c.i;
            var j = c.j;
            foreach (var (k, v) in src)
            {
                i += 1L;
                j += uint8(c.s[i]);
                c.s[i] = c.s[j];
                c.s[j] = c.s[i];
                dst[k] = v ^ uint8(c.s[uint8(c.s[i] + c.s[j])]);
            }
            c.i = i;
            c.j = j;
        }
    }
}}
