// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rc4 implements RC4 encryption, as defined in Bruce Schneier's
// Applied Cryptography.
//
// RC4 is cryptographically broken and should not be used for secure
// applications.
// package rc4 -- go2cs converted at 2020 October 08 03:36:39 UTC
// import "crypto/rc4" ==> using rc4 = go.crypto.rc4_package
// Original source: C:\Go\src\crypto\rc4\rc4.go
using subtle = go.crypto.@internal.subtle_package;
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
        public static (ptr<Cipher>, error) NewCipher(slice<byte> key)
        {
            ptr<Cipher> _p0 = default!;
            error _p0 = default!;

            var k = len(key);
            if (k < 1L || k > 256L)
            {
                return (_addr_null!, error.As(KeySizeError(k))!);
            }

            ref Cipher c = ref heap(out ptr<Cipher> _addr_c);
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
            return (_addr__addr_c!, error.As(null!)!);

        }

        // Reset zeros the key data and makes the Cipher unusable.
        //
        // Deprecated: Reset can't guarantee that the key will be entirely removed from
        // the process's memory.
        private static void Reset(this ptr<Cipher> _addr_c)
        {
            ref Cipher c = ref _addr_c.val;

            foreach (var (i) in c.s)
            {
                c.s[i] = 0L;
            }
            c.i = 0L;
            c.j = 0L;

        }

        // XORKeyStream sets dst to the result of XORing src with the key stream.
        // Dst and src must overlap entirely or not at all.
        private static void XORKeyStream(this ptr<Cipher> _addr_c, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref Cipher c = ref _addr_c.val;

            if (len(src) == 0L)
            {
                return ;
            }

            if (subtle.InexactOverlap(dst[..len(src)], src))
            {
                panic("crypto/rc4: invalid buffer overlap");
            }

            var i = c.i;
            var j = c.j;
            _ = dst[len(src) - 1L];
            dst = dst[..len(src)]; // eliminate bounds check from loop
            foreach (var (k, v) in src)
            {
                i += 1L;
                var x = c.s[i];
                j += uint8(x);
                var y = c.s[j];
                c.s[i] = y;
                c.s[j] = x;
                dst[k] = v ^ uint8(c.s[uint8(x + y)]);

            }
            c.i = i;
            c.j = j;

        });
    }
}}
