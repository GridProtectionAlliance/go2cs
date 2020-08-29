// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cipher -- go2cs converted at 2020 August 29 08:28:54 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\xor.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static unsafe partial class cipher_package
    {
        private static readonly var wordSize = int(@unsafe.Sizeof(uintptr(0L)));

        private static readonly var supportsUnaligned = runtime.GOARCH == "386" || runtime.GOARCH == "amd64" || runtime.GOARCH == "ppc64" || runtime.GOARCH == "ppc64le" || runtime.GOARCH == "s390x";

        // fastXORBytes xors in bulk. It only works on architectures that
        // support unaligned read/writes.


        // fastXORBytes xors in bulk. It only works on architectures that
        // support unaligned read/writes.
        private static long fastXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            var n = len(a);
            if (len(b) < n)
            {
                n = len(b);
            }
            if (n == 0L)
            {
                return 0L;
            } 
            // Assert dst has enough space
            _ = dst[n - 1L];

            var w = n / wordSize;
            if (w > 0L)
            {
                *(*slice<System.UIntPtr>) dw = @unsafe.Pointer(ref dst).Value;
                *(*slice<System.UIntPtr>) aw = @unsafe.Pointer(ref a).Value;
                *(*slice<System.UIntPtr>) bw = @unsafe.Pointer(ref b).Value;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < w; i++)
                    {
                        dw[i] = aw[i] ^ bw[i];
                    }


                    i = i__prev1;
                }
            }
            {
                long i__prev1 = i;

                for (i = (n - n % wordSize); i < n; i++)
                {
                    dst[i] = a[i] ^ b[i];
                }


                i = i__prev1;
            }

            return n;
        }

        private static long safeXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            var n = len(a);
            if (len(b) < n)
            {
                n = len(b);
            }
            for (long i = 0L; i < n; i++)
            {
                dst[i] = a[i] ^ b[i];
            }

            return n;
        }

        // xorBytes xors the bytes in a and b. The destination should have enough
        // space, otherwise xorBytes will panic. Returns the number of bytes xor'd.
        private static long xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            if (supportsUnaligned)
            {
                return fastXORBytes(dst, a, b);
            }
            else
            { 
                // TODO(hanwen): if (dst, a, b) have common alignment
                // we could still try fastXORBytes. It is not clear
                // how often this happens, and it's only worth it if
                // the block encryption itself is hardware
                // accelerated.
                return safeXORBytes(dst, a, b);
            }
        }

        // fastXORWords XORs multiples of 4 or 8 bytes (depending on architecture.)
        // The arguments are assumed to be of equal length.
        private static void fastXORWords(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            *(*slice<System.UIntPtr>) dw = @unsafe.Pointer(ref dst).Value;
            *(*slice<System.UIntPtr>) aw = @unsafe.Pointer(ref a).Value;
            *(*slice<System.UIntPtr>) bw = @unsafe.Pointer(ref b).Value;
            var n = len(b) / wordSize;
            for (long i = 0L; i < n; i++)
            {
                dw[i] = aw[i] ^ bw[i];
            }

        }

        private static void xorWords(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            if (supportsUnaligned)
            {
                fastXORWords(dst, a, b);
            }
            else
            {
                safeXORBytes(dst, a, b);
            }
        }
    }
}}
