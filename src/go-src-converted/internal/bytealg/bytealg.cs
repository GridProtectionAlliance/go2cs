// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2022 March 06 22:08:05 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\bytealg.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;

namespace go.@internal;

public static partial class bytealg_package {

    // Offsets into internal/cpu records for use in assembly.
private static readonly var offsetX86HasSSE2 = @unsafe.Offsetof(cpu.X86.HasSSE2);
private static readonly var offsetX86HasSSE42 = @unsafe.Offsetof(cpu.X86.HasSSE42);
private static readonly var offsetX86HasAVX2 = @unsafe.Offsetof(cpu.X86.HasAVX2);
private static readonly var offsetX86HasPOPCNT = @unsafe.Offsetof(cpu.X86.HasPOPCNT);

private static readonly var offsetS390xHasVX = @unsafe.Offsetof(cpu.S390X.HasVX);

private static readonly var offsetPPC64HasPOWER9 = @unsafe.Offsetof(cpu.PPC64.IsPOWER9);


// MaxLen is the maximum length of the string to be searched for (argument b) in Index.
// If MaxLen is not 0, make sure MaxLen >= 4.
public static nint MaxLen = default;

// FIXME: the logic of HashStrBytes, HashStrRevBytes, IndexRabinKarpBytes and HashStr, HashStrRev,
// IndexRabinKarp are exactly the same, except that the types are different. Can we eliminate
// three of them without causing allocation?

// PrimeRK is the prime base used in Rabin-Karp algorithm.
public static readonly nint PrimeRK = 16777619;

// HashStrBytes returns the hash and the appropriate multiplicative
// factor for use in Rabin-Karp algorithm.


// HashStrBytes returns the hash and the appropriate multiplicative
// factor for use in Rabin-Karp algorithm.
public static (uint, uint) HashStrBytes(slice<byte> sep) {
    uint _p0 = default;
    uint _p0 = default;

    var hash = uint32(0);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(sep); i++) {
            hash = hash * PrimeRK + uint32(sep[i]);
        }

        i = i__prev1;
    }
    uint pow = 1;    uint sq = PrimeRK;

    {
        nint i__prev1 = i;

        i = len(sep);

        while (i > 0) {
            if (i & 1 != 0) {
                pow *= sq;
            i>>=1;
            }

            sq *= sq;

        }

        i = i__prev1;
    }
    return (hash, pow);

}

// HashStr returns the hash and the appropriate multiplicative
// factor for use in Rabin-Karp algorithm.
public static (uint, uint) HashStr(@string sep) {
    uint _p0 = default;
    uint _p0 = default;

    var hash = uint32(0);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(sep); i++) {
            hash = hash * PrimeRK + uint32(sep[i]);
        }

        i = i__prev1;
    }
    uint pow = 1;    uint sq = PrimeRK;

    {
        nint i__prev1 = i;

        i = len(sep);

        while (i > 0) {
            if (i & 1 != 0) {
                pow *= sq;
            i>>=1;
            }

            sq *= sq;

        }

        i = i__prev1;
    }
    return (hash, pow);

}

// HashStrRevBytes returns the hash of the reverse of sep and the
// appropriate multiplicative factor for use in Rabin-Karp algorithm.
public static (uint, uint) HashStrRevBytes(slice<byte> sep) {
    uint _p0 = default;
    uint _p0 = default;

    var hash = uint32(0);
    {
        var i__prev1 = i;

        for (var i = len(sep) - 1; i >= 0; i--) {
            hash = hash * PrimeRK + uint32(sep[i]);
        }

        i = i__prev1;
    }
    uint pow = 1;    uint sq = PrimeRK;

    {
        var i__prev1 = i;

        i = len(sep);

        while (i > 0) {
            if (i & 1 != 0) {
                pow *= sq;
            i>>=1;
            }

            sq *= sq;

        }

        i = i__prev1;
    }
    return (hash, pow);

}

// HashStrRev returns the hash of the reverse of sep and the
// appropriate multiplicative factor for use in Rabin-Karp algorithm.
public static (uint, uint) HashStrRev(@string sep) {
    uint _p0 = default;
    uint _p0 = default;

    var hash = uint32(0);
    {
        var i__prev1 = i;

        for (var i = len(sep) - 1; i >= 0; i--) {
            hash = hash * PrimeRK + uint32(sep[i]);
        }

        i = i__prev1;
    }
    uint pow = 1;    uint sq = PrimeRK;

    {
        var i__prev1 = i;

        i = len(sep);

        while (i > 0) {
            if (i & 1 != 0) {
                pow *= sq;
            i>>=1;
            }

            sq *= sq;

        }

        i = i__prev1;
    }
    return (hash, pow);

}

// IndexRabinKarpBytes uses the Rabin-Karp search algorithm to return the index of the
// first occurrence of substr in s, or -1 if not present.
public static nint IndexRabinKarpBytes(slice<byte> s, slice<byte> sep) { 
    // Rabin-Karp search
    var (hashsep, pow) = HashStrBytes(sep);
    var n = len(sep);
    uint h = default;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            h = h * PrimeRK + uint32(s[i]);
        }

        i = i__prev1;
    }
    if (h == hashsep && Equal(s[..(int)n], sep)) {
        return 0;
    }
    {
        nint i__prev1 = i;

        i = n;

        while (i < len(s)) {
            h *= PrimeRK;
            h += uint32(s[i]);
            h -= pow * uint32(s[i - n]);
            i++;
            if (h == hashsep && Equal(s[(int)i - n..(int)i], sep)) {
                return i - n;
            }
        }

        i = i__prev1;
    }
    return -1;

}

// IndexRabinKarp uses the Rabin-Karp search algorithm to return the index of the
// first occurrence of substr in s, or -1 if not present.
public static nint IndexRabinKarp(@string s, @string substr) { 
    // Rabin-Karp search
    var (hashss, pow) = HashStr(substr);
    var n = len(substr);
    uint h = default;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            h = h * PrimeRK + uint32(s[i]);
        }

        i = i__prev1;
    }
    if (h == hashss && s[..(int)n] == substr) {
        return 0;
    }
    {
        nint i__prev1 = i;

        i = n;

        while (i < len(s)) {
            h *= PrimeRK;
            h += uint32(s[i]);
            h -= pow * uint32(s[i - n]);
            i++;
            if (h == hashss && s[(int)i - n..(int)i] == substr) {
                return i - n;
            }
        }

        i = i__prev1;
    }
    return -1;

}

} // end bytealg_package
