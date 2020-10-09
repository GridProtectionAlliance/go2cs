// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2020 October 09 04:45:56 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\bytealg.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        // Offsets into internal/cpu records for use in assembly.
        private static readonly var offsetX86HasSSE2 = @unsafe.Offsetof(cpu.X86.HasSSE2);
        private static readonly var offsetX86HasSSE42 = @unsafe.Offsetof(cpu.X86.HasSSE42);
        private static readonly var offsetX86HasAVX2 = @unsafe.Offsetof(cpu.X86.HasAVX2);
        private static readonly var offsetX86HasPOPCNT = @unsafe.Offsetof(cpu.X86.HasPOPCNT);

        private static readonly var offsetS390xHasVX = @unsafe.Offsetof(cpu.S390X.HasVX);


        // MaxLen is the maximum length of the string to be searched for (argument b) in Index.
        // If MaxLen is not 0, make sure MaxLen >= 4.
        public static long MaxLen = default;

        // FIXME: the logic of HashStrBytes, HashStrRevBytes, IndexRabinKarpBytes and HashStr, HashStrRev,
        // IndexRabinKarp are exactly the same, except that the types are different. Can we eliminate
        // three of them without causing allocation?

        // PrimeRK is the prime base used in Rabin-Karp algorithm.
        public static readonly long PrimeRK = (long)16777619L;

        // HashStrBytes returns the hash and the appropriate multiplicative
        // factor for use in Rabin-Karp algorithm.


        // HashStrBytes returns the hash and the appropriate multiplicative
        // factor for use in Rabin-Karp algorithm.
        public static (uint, uint) HashStrBytes(slice<byte> sep)
        {
            uint _p0 = default;
            uint _p0 = default;

            var hash = uint32(0L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(sep); i++)
                {
                    hash = hash * PrimeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = PrimeRK;

            {
                long i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }

                    sq *= sq;

                }


                i = i__prev1;
            }
            return (hash, pow);

        }

        // HashStr returns the hash and the appropriate multiplicative
        // factor for use in Rabin-Karp algorithm.
        public static (uint, uint) HashStr(@string sep)
        {
            uint _p0 = default;
            uint _p0 = default;

            var hash = uint32(0L);
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(sep); i++)
                {
                    hash = hash * PrimeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = PrimeRK;

            {
                long i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }

                    sq *= sq;

                }


                i = i__prev1;
            }
            return (hash, pow);

        }

        // HashStrRevBytes returns the hash of the reverse of sep and the
        // appropriate multiplicative factor for use in Rabin-Karp algorithm.
        public static (uint, uint) HashStrRevBytes(slice<byte> sep)
        {
            uint _p0 = default;
            uint _p0 = default;

            var hash = uint32(0L);
            {
                var i__prev1 = i;

                for (var i = len(sep) - 1L; i >= 0L; i--)
                {
                    hash = hash * PrimeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = PrimeRK;

            {
                var i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }

                    sq *= sq;

                }


                i = i__prev1;
            }
            return (hash, pow);

        }

        // HashStrRev returns the hash of the reverse of sep and the
        // appropriate multiplicative factor for use in Rabin-Karp algorithm.
        public static (uint, uint) HashStrRev(@string sep)
        {
            uint _p0 = default;
            uint _p0 = default;

            var hash = uint32(0L);
            {
                var i__prev1 = i;

                for (var i = len(sep) - 1L; i >= 0L; i--)
                {
                    hash = hash * PrimeRK + uint32(sep[i]);
                }


                i = i__prev1;
            }
            uint pow = 1L;            uint sq = PrimeRK;

            {
                var i__prev1 = i;

                i = len(sep);

                while (i > 0L)
                {
                    if (i & 1L != 0L)
                    {
                        pow *= sq;
                    i >>= 1L;
                    }

                    sq *= sq;

                }


                i = i__prev1;
            }
            return (hash, pow);

        }

        // IndexRabinKarpBytes uses the Rabin-Karp search algorithm to return the index of the
        // first occurence of substr in s, or -1 if not present.
        public static long IndexRabinKarpBytes(slice<byte> s, slice<byte> sep)
        { 
            // Rabin-Karp search
            var (hashsep, pow) = HashStrBytes(sep);
            var n = len(sep);
            uint h = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    h = h * PrimeRK + uint32(s[i]);
                }


                i = i__prev1;
            }
            if (h == hashsep && Equal(s[..n], sep))
            {
                return 0L;
            }

            {
                long i__prev1 = i;

                i = n;

                while (i < len(s))
                {
                    h *= PrimeRK;
                    h += uint32(s[i]);
                    h -= pow * uint32(s[i - n]);
                    i++;
                    if (h == hashsep && Equal(s[i - n..i], sep))
                    {
                        return i - n;
                    }

                }


                i = i__prev1;
            }
            return -1L;

        }

        // IndexRabinKarp uses the Rabin-Karp search algorithm to return the index of the
        // first occurence of substr in s, or -1 if not present.
        public static long IndexRabinKarp(@string s, @string substr)
        { 
            // Rabin-Karp search
            var (hashss, pow) = HashStr(substr);
            var n = len(substr);
            uint h = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < n; i++)
                {
                    h = h * PrimeRK + uint32(s[i]);
                }


                i = i__prev1;
            }
            if (h == hashss && s[..n] == substr)
            {
                return 0L;
            }

            {
                long i__prev1 = i;

                i = n;

                while (i < len(s))
                {
                    h *= PrimeRK;
                    h += uint32(s[i]);
                    h -= pow * uint32(s[i - n]);
                    i++;
                    if (h == hashss && s[i - n..i] == substr)
                    {
                        return i - n;
                    }

                }


                i = i__prev1;
            }
            return -1L;

        }
    }
}}
