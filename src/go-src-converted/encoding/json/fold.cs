// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2020 October 09 05:00:06 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\fold.go
using bytes = go.bytes_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        private static readonly var caseMask = ~byte(0x20UL); // Mask to ignore case in ASCII.
        private static readonly char kelvin = (char)'\u212a';
        private static readonly char smallLongEss = (char)'\u017f';


        // foldFunc returns one of four different case folding equivalence
        // functions, from most general (and slow) to fastest:
        //
        // 1) bytes.EqualFold, if the key s contains any non-ASCII UTF-8
        // 2) equalFoldRight, if s contains special folding ASCII ('k', 'K', 's', 'S')
        // 3) asciiEqualFold, no special, but includes non-letters (including _)
        // 4) simpleLetterEqualFold, no specials, no non-letters.
        //
        // The letters S and K are special because they map to 3 runes, not just 2:
        //  * S maps to s and to U+017F 'ſ' Latin small letter long s
        //  * k maps to K and to U+212A 'K' Kelvin sign
        // See https://play.golang.org/p/tTxjOc0OGo
        //
        // The returned function is specialized for matching against s and
        // should only be given s. It's not curried for performance reasons.
        private static Func<slice<byte>, slice<byte>, bool> foldFunc(slice<byte> s)
        {
            var nonLetter = false;
            var special = false; // special letter
            foreach (var (_, b) in s)
            {
                if (b >= utf8.RuneSelf)
                {
                    return bytes.EqualFold;
                }

                var upper = b & caseMask;
                if (upper < 'A' || upper > 'Z')
                {
                    nonLetter = true;
                }
                else if (upper == 'K' || upper == 'S')
                { 
                    // See above for why these letters are special.
                    special = true;

                }

            }
            if (special)
            {
                return equalFoldRight;
            }

            if (nonLetter)
            {
                return asciiEqualFold;
            }

            return simpleLetterEqualFold;

        }

        // equalFoldRight is a specialization of bytes.EqualFold when s is
        // known to be all ASCII (including punctuation), but contains an 's',
        // 'S', 'k', or 'K', requiring a Unicode fold on the bytes in t.
        // See comments on foldFunc.
        private static bool equalFoldRight(slice<byte> s, slice<byte> t)
        {
            foreach (var (_, sb) in s)
            {
                if (len(t) == 0L)
                {
                    return false;
                }

                var tb = t[0L];
                if (tb < utf8.RuneSelf)
                {
                    if (sb != tb)
                    {
                        var sbUpper = sb & caseMask;
                        if ('A' <= sbUpper && sbUpper <= 'Z')
                        {
                            if (sbUpper != tb & caseMask)
                            {
                                return false;
                            }

                        }
                        else
                        {
                            return false;
                        }

                    }

                    t = t[1L..];
                    continue;

                } 
                // sb is ASCII and t is not. t must be either kelvin
                // sign or long s; sb must be s, S, k, or K.
                var (tr, size) = utf8.DecodeRune(t);
                switch (sb)
                {
                    case 's': 

                    case 'S': 
                        if (tr != smallLongEss)
                        {
                            return false;
                        }

                        break;
                    case 'k': 

                    case 'K': 
                        if (tr != kelvin)
                        {
                            return false;
                        }

                        break;
                    default: 
                        return false;
                        break;
                }
                t = t[size..];


            }
            if (len(t) > 0L)
            {
                return false;
            }

            return true;

        }

        // asciiEqualFold is a specialization of bytes.EqualFold for use when
        // s is all ASCII (but may contain non-letters) and contains no
        // special-folding letters.
        // See comments on foldFunc.
        private static bool asciiEqualFold(slice<byte> s, slice<byte> t)
        {
            if (len(s) != len(t))
            {
                return false;
            }

            foreach (var (i, sb) in s)
            {
                var tb = t[i];
                if (sb == tb)
                {
                    continue;
                }

                if (('a' <= sb && sb <= 'z') || ('A' <= sb && sb <= 'Z'))
                {
                    if (sb & caseMask != tb & caseMask)
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }

            }
            return true;

        }

        // simpleLetterEqualFold is a specialization of bytes.EqualFold for
        // use when s is all ASCII letters (no underscores, etc) and also
        // doesn't contain 'k', 'K', 's', or 'S'.
        // See comments on foldFunc.
        private static bool simpleLetterEqualFold(slice<byte> s, slice<byte> t)
        {
            if (len(s) != len(t))
            {
                return false;
            }

            foreach (var (i, b) in s)
            {
                if (b & caseMask != t[i] & caseMask)
                {
                    return false;
                }

            }
            return true;

        }
    }
}}
