// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!ppc64,!ppc64le

// package cipher -- go2cs converted at 2020 October 08 03:35:46 UTC
// import "crypto/cipher" ==> using cipher = go.crypto.cipher_package
// Original source: C:\Go\src\crypto\cipher\xor_generic.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class cipher_package
    {
        // xorBytes xors the bytes in a and b. The destination should have enough
        // space, otherwise xorBytes will panic. Returns the number of bytes xor'd.
        private static long xorBytes(slice<byte> dst, slice<byte> a, slice<byte> b)
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

            if (supportsUnaligned) 
                fastXORBytes(dst, a, b, n);
            else 
                // TODO(hanwen): if (dst, a, b) have common alignment
                // we could still try fastXORBytes. It is not clear
                // how often this happens, and it's only worth it if
                // the block encryption itself is hardware
                // accelerated.
                safeXORBytes(dst, a, b, n);
                        return n;

        }

        private static readonly var wordSize = (var)int(@unsafe.Sizeof(uintptr(0L)));

        private static readonly var supportsUnaligned = (var)runtime.GOARCH == "386" || runtime.GOARCH == "ppc64" || runtime.GOARCH == "ppc64le" || runtime.GOARCH == "s390x";

        // fastXORBytes xors in bulk. It only works on architectures that
        // support unaligned read/writes.
        // n needs to be smaller or equal than the length of a and b.


        // fastXORBytes xors in bulk. It only works on architectures that
        // support unaligned read/writes.
        // n needs to be smaller or equal than the length of a and b.
        private static void fastXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b, long n)
        { 
            // Assert dst has enough space
            _ = dst[n - 1L];

            var w = n / wordSize;
            if (w > 0L)
            {
                ptr<ptr<slice<System.UIntPtr>>> dw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_dst));
                ptr<ptr<slice<System.UIntPtr>>> aw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_a));
                ptr<ptr<slice<System.UIntPtr>>> bw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_b));
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

        }

        // n needs to be smaller or equal than the length of a and b.
        private static void safeXORBytes(slice<byte> dst, slice<byte> a, slice<byte> b, long n)
        {
            for (long i = 0L; i < n; i++)
            {
                dst[i] = a[i] ^ b[i];
            }


        }

        // fastXORWords XORs multiples of 4 or 8 bytes (depending on architecture.)
        // The arguments are assumed to be of equal length.
        private static void fastXORWords(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            ptr<ptr<slice<System.UIntPtr>>> dw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_dst));
            ptr<ptr<slice<System.UIntPtr>>> aw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_a));
            ptr<ptr<slice<System.UIntPtr>>> bw = new ptr<ptr<ptr<slice<System.UIntPtr>>>>(@unsafe.Pointer(_addr_b));
            var n = len(b) / wordSize;
            for (long i = 0L; i < n; i++)
            {
                dw[i] = aw[i] ^ bw[i];
            }


        }

        // fastXORWords XORs multiples of 4 or 8 bytes (depending on architecture.)
        // The slice arguments a and b are assumed to be of equal length.
        private static void xorWords(slice<byte> dst, slice<byte> a, slice<byte> b)
        {
            if (supportsUnaligned)
            {
                fastXORWords(dst, a, b);
            }
            else
            {
                safeXORBytes(dst, a, b, len(b));
            }

        }
    }
}}
