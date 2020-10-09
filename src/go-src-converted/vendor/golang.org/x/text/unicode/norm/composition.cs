// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2020 October 09 06:08:20 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\composition.go
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class norm_package
    {
        private static readonly long maxNonStarters = (long)30L; 
        // The maximum number of characters needed for a buffer is
        // maxNonStarters + 1 for the starter + 1 for the GCJ
        private static readonly var maxBufferSize = maxNonStarters + 2L;
        private static readonly long maxNFCExpansion = (long)3L; // NFC(0x1D160)
        private static readonly long maxNFKCExpansion = (long)18L; // NFKC(0xFDFA)

        private static readonly var maxByteBufferSize = utf8.UTFMax * maxBufferSize; // 128

        // ssState is used for reporting the segment state after inserting a rune.
        // It is returned by streamSafe.next.
        private partial struct ssState // : long
        {
        }

 
        // Indicates a rune was successfully added to the segment.
        private static readonly ssState ssSuccess = (ssState)iota; 
        // Indicates a rune starts a new segment and should not be added.
        private static readonly var ssStarter = 0; 
        // Indicates a rune caused a segment overflow and a CGJ should be inserted.
        private static readonly var ssOverflow = 1;


        // streamSafe implements the policy of when a CGJ should be inserted.
        private partial struct streamSafe // : byte
        {
        }

        // first inserts the first rune of a segment. It is a faster version of next if
        // it is known p represents the first rune in a segment.
        private static void first(this ptr<streamSafe> _addr_ss, Properties p)
        {
            ref streamSafe ss = ref _addr_ss.val;

            ss.val = streamSafe(p.nTrailingNonStarters());
        }

        // insert returns a ssState value to indicate whether a rune represented by p
        // can be inserted.
        private static ssState next(this ptr<streamSafe> _addr_ss, Properties p) => func((_, panic, __) =>
        {
            ref streamSafe ss = ref _addr_ss.val;

            if (ss > maxNonStarters.val)
            {
                panic("streamSafe was not reset");
            }

            var n = p.nLeadingNonStarters();
            ss.val += streamSafe(n);

            if (ss > maxNonStarters.val)
            {
                ss.val = 0L;
                return ssOverflow;
            } 
            // The Stream-Safe Text Processing prescribes that the counting can stop
            // as soon as a starter is encountered. However, there are some starters,
            // like Jamo V and T, that can combine with other runes, leaving their
            // successive non-starters appended to the previous, possibly causing an
            // overflow. We will therefore consider any rune with a non-zero nLead to
            // be a non-starter. Note that it always hold that if nLead > 0 then
            // nLead == nTrail.
            if (n == 0L)
            {
                ss.val = streamSafe(p.nTrailingNonStarters());
                return ssStarter;
            }

            return ssSuccess;

        });

        // backwards is used for checking for overflow and segment starts
        // when traversing a string backwards. Users do not need to call first
        // for the first rune. The state of the streamSafe retains the count of
        // the non-starters loaded.
        private static ssState backwards(this ptr<streamSafe> _addr_ss, Properties p) => func((_, panic, __) =>
        {
            ref streamSafe ss = ref _addr_ss.val;

            if (ss > maxNonStarters.val)
            {
                panic("streamSafe was not reset");
            }

            var c = ss + streamSafe(p.nTrailingNonStarters()).val;
            if (c > maxNonStarters)
            {
                return ssOverflow;
            }

            ss.val = c;
            if (p.nLeadingNonStarters() == 0L)
            {
                return ssStarter;
            }

            return ssSuccess;

        });

        private static bool isMax(this streamSafe ss)
        {
            return ss == maxNonStarters;
        }

        // GraphemeJoiner is inserted after maxNonStarters non-starter runes.
        public static readonly @string GraphemeJoiner = (@string)"\u034F";

        // reorderBuffer is used to normalize a single segment.  Characters inserted with
        // insert are decomposed and reordered based on CCC. The compose method can
        // be used to recombine characters.  Note that the byte buffer does not hold
        // the UTF-8 characters in order.  Only the rune array is maintained in sorted
        // order. flush writes the resulting segment to a byte array.


        // reorderBuffer is used to normalize a single segment.  Characters inserted with
        // insert are decomposed and reordered based on CCC. The compose method can
        // be used to recombine characters.  Note that the byte buffer does not hold
        // the UTF-8 characters in order.  Only the rune array is maintained in sorted
        // order. flush writes the resulting segment to a byte array.
        private partial struct reorderBuffer
        {
            public array<Properties> rune; // Per character info.
            public array<byte> @byte; // UTF-8 buffer. Referenced by runeInfo.pos.
            public byte nbyte; // Number or bytes.
            public streamSafe ss; // For limiting length of non-starter sequence.
            public long nrune; // Number of runeInfos.
            public formInfo f;
            public input src;
            public long nsrc;
            public input tmpBytes;
            public slice<byte> @out;
            public Func<ptr<reorderBuffer>, bool> flushF;
        }

        private static void init(this ptr<reorderBuffer> _addr_rb, Form f, slice<byte> src)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.f = formTable[f].val;
            rb.src.setBytes(src);
            rb.nsrc = len(src);
            rb.ss = 0L;
        }

        private static void initString(this ptr<reorderBuffer> _addr_rb, Form f, @string src)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.f = formTable[f].val;
            rb.src.setString(src);
            rb.nsrc = len(src);
            rb.ss = 0L;
        }

        private static bool setFlusher(this ptr<reorderBuffer> _addr_rb, slice<byte> @out, Func<ptr<reorderBuffer>, bool> f)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.@out = out;
            rb.flushF = f;
        }

        // reset discards all characters from the buffer.
        private static void reset(this ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.nrune = 0L;
            rb.nbyte = 0L;
        }

        private static bool doFlush(this ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            if (rb.f.composing)
            {
                rb.compose();
            }

            var res = rb.flushF(rb);
            rb.reset();
            return res;

        }

        // appendFlush appends the normalized segment to rb.out.
        private static bool appendFlush(ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            for (long i = 0L; i < rb.nrune; i++)
            {
                var start = rb.rune[i].pos;
                var end = start + rb.rune[i].size;
                rb.@out = append(rb.@out, rb.@byte[start..end]);
            }

            return true;

        }

        // flush appends the normalized segment to out and resets rb.
        private static slice<byte> flush(this ptr<reorderBuffer> _addr_rb, slice<byte> @out)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            for (long i = 0L; i < rb.nrune; i++)
            {
                var start = rb.rune[i].pos;
                var end = start + rb.rune[i].size;
                out = append(out, rb.@byte[start..end]);
            }

            rb.reset();
            return out;

        }

        // flushCopy copies the normalized segment to buf and resets rb.
        // It returns the number of bytes written to buf.
        private static long flushCopy(this ptr<reorderBuffer> _addr_rb, slice<byte> buf)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            long p = 0L;
            for (long i = 0L; i < rb.nrune; i++)
            {
                var runep = rb.rune[i];
                p += copy(buf[p..], rb.@byte[runep.pos..runep.pos + runep.size]);
            }

            rb.reset();
            return p;

        }

        // insertOrdered inserts a rune in the buffer, ordered by Canonical Combining Class.
        // It returns false if the buffer is not large enough to hold the rune.
        // It is used internally by insert and insertString only.
        private static void insertOrdered(this ptr<reorderBuffer> _addr_rb, Properties info)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var n = rb.nrune;
            var b = rb.rune[..];
            var cc = info.ccc;
            if (cc > 0L)
            { 
                // Find insertion position + move elements to make room.
                while (n > 0L)
                {
                    if (b[n - 1L].ccc <= cc)
                    {
                        break;
                    n--;
                    }

                    b[n] = b[n - 1L];

                }


            }

            rb.nrune += 1L;
            var pos = uint8(rb.nbyte);
            rb.nbyte += utf8.UTFMax;
            info.pos = pos;
            b[n] = info;

        }

        // insertErr is an error code returned by insert. Using this type instead
        // of error improves performance up to 20% for many of the benchmarks.
        private partial struct insertErr // : long
        {
        }

        private static readonly insertErr iSuccess = (insertErr)-iota;
        private static readonly var iShortDst = 0;
        private static readonly var iShortSrc = 1;


        // insertFlush inserts the given rune in the buffer ordered by CCC.
        // If a decomposition with multiple segments are encountered, they leading
        // ones are flushed.
        // It returns a non-zero error code if the rune was not inserted.
        private static insertErr insertFlush(this ptr<reorderBuffer> _addr_rb, input src, long i, Properties info)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            {
                var rune = src.hangul(i);

                if (rune != 0L)
                {
                    rb.decomposeHangul(rune);
                    return iSuccess;
                }

            }

            if (info.hasDecomposition())
            {
                return rb.insertDecomposed(info.Decomposition());
            }

            rb.insertSingle(src, i, info);
            return iSuccess;

        }

        // insertUnsafe inserts the given rune in the buffer ordered by CCC.
        // It is assumed there is sufficient space to hold the runes. It is the
        // responsibility of the caller to ensure this. This can be done by checking
        // the state returned by the streamSafe type.
        private static void insertUnsafe(this ptr<reorderBuffer> _addr_rb, input src, long i, Properties info)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            {
                var rune = src.hangul(i);

                if (rune != 0L)
                {
                    rb.decomposeHangul(rune);
                }

            }

            if (info.hasDecomposition())
            { 
                // TODO: inline.
                rb.insertDecomposed(info.Decomposition());

            }
            else
            {
                rb.insertSingle(src, i, info);
            }

        }

        // insertDecomposed inserts an entry in to the reorderBuffer for each rune
        // in dcomp. dcomp must be a sequence of decomposed UTF-8-encoded runes.
        // It flushes the buffer on each new segment start.
        private static insertErr insertDecomposed(this ptr<reorderBuffer> _addr_rb, slice<byte> dcomp)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.tmpBytes.setBytes(dcomp); 
            // As the streamSafe accounting already handles the counting for modifiers,
            // we don't have to call next. However, we do need to keep the accounting
            // intact when flushing the buffer.
            {
                long i = 0L;

                while (i < len(dcomp))
                {
                    var info = rb.f.info(rb.tmpBytes, i);
                    if (info.BoundaryBefore() && rb.nrune > 0L && !rb.doFlush())
                    {
                        return iShortDst;
                    }

                    i += copy(rb.@byte[rb.nbyte..], dcomp[i..i + int(info.size)]);
                    rb.insertOrdered(info);

                }

            }
            return iSuccess;

        }

        // insertSingle inserts an entry in the reorderBuffer for the rune at
        // position i. info is the runeInfo for the rune at position i.
        private static void insertSingle(this ptr<reorderBuffer> _addr_rb, input src, long i, Properties info)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            src.copySlice(rb.@byte[rb.nbyte..], i, i + int(info.size));
            rb.insertOrdered(info);
        }

        // insertCGJ inserts a Combining Grapheme Joiner (0x034f) into rb.
        private static void insertCGJ(this ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            rb.insertSingle(new input(str:GraphemeJoiner), 0L, new Properties(size:uint8(len(GraphemeJoiner))));
        }

        // appendRune inserts a rune at the end of the buffer. It is used for Hangul.
        private static void appendRune(this ptr<reorderBuffer> _addr_rb, int r)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var bn = rb.nbyte;
            var sz = utf8.EncodeRune(rb.@byte[bn..], rune(r));
            rb.nbyte += utf8.UTFMax;
            rb.rune[rb.nrune] = new Properties(pos:bn,size:uint8(sz));
            rb.nrune++;
        }

        // assignRune sets a rune at position pos. It is used for Hangul and recomposition.
        private static void assignRune(this ptr<reorderBuffer> _addr_rb, long pos, int r)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var bn = rb.rune[pos].pos;
            var sz = utf8.EncodeRune(rb.@byte[bn..], rune(r));
            rb.rune[pos] = new Properties(pos:bn,size:uint8(sz));
        }

        // runeAt returns the rune at position n. It is used for Hangul and recomposition.
        private static int runeAt(this ptr<reorderBuffer> _addr_rb, long n)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var inf = rb.rune[n];
            var (r, _) = utf8.DecodeRune(rb.@byte[inf.pos..inf.pos + inf.size]);
            return r;
        }

        // bytesAt returns the UTF-8 encoding of the rune at position n.
        // It is used for Hangul and recomposition.
        private static slice<byte> bytesAt(this ptr<reorderBuffer> _addr_rb, long n)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var inf = rb.rune[n];
            return rb.@byte[inf.pos..int(inf.pos) + int(inf.size)];
        }

        // For Hangul we combine algorithmically, instead of using tables.
        private static readonly ulong hangulBase = (ulong)0xAC00UL; // UTF-8(hangulBase) -> EA B0 80
        private static readonly ulong hangulBase0 = (ulong)0xEAUL;
        private static readonly ulong hangulBase1 = (ulong)0xB0UL;
        private static readonly ulong hangulBase2 = (ulong)0x80UL;

        private static readonly var hangulEnd = hangulBase + jamoLVTCount; // UTF-8(0xD7A4) -> ED 9E A4
        private static readonly ulong hangulEnd0 = (ulong)0xEDUL;
        private static readonly ulong hangulEnd1 = (ulong)0x9EUL;
        private static readonly ulong hangulEnd2 = (ulong)0xA4UL;

        private static readonly ulong jamoLBase = (ulong)0x1100UL; // UTF-8(jamoLBase) -> E1 84 00
        private static readonly ulong jamoLBase0 = (ulong)0xE1UL;
        private static readonly ulong jamoLBase1 = (ulong)0x84UL;
        private static readonly ulong jamoLEnd = (ulong)0x1113UL;
        private static readonly ulong jamoVBase = (ulong)0x1161UL;
        private static readonly ulong jamoVEnd = (ulong)0x1176UL;
        private static readonly ulong jamoTBase = (ulong)0x11A7UL;
        private static readonly ulong jamoTEnd = (ulong)0x11C3UL;

        private static readonly long jamoTCount = (long)28L;
        private static readonly long jamoVCount = (long)21L;
        private static readonly long jamoVTCount = (long)21L * 28L;
        private static readonly long jamoLVTCount = (long)19L * 21L * 28L;


        private static readonly long hangulUTF8Size = (long)3L;



        private static bool isHangul(slice<byte> b)
        {
            if (len(b) < hangulUTF8Size)
            {
                return false;
            }

            var b0 = b[0L];
            if (b0 < hangulBase0)
            {
                return false;
            }

            var b1 = b[1L];

            if (b0 == hangulBase0) 
                return b1 >= hangulBase1;
            else if (b0 < hangulEnd0) 
                return true;
            else if (b0 > hangulEnd0) 
                return false;
            else if (b1 < hangulEnd1) 
                return true;
                        return b1 == hangulEnd1 && b[2L] < hangulEnd2;

        }

        private static bool isHangulString(@string b)
        {
            if (len(b) < hangulUTF8Size)
            {
                return false;
            }

            var b0 = b[0L];
            if (b0 < hangulBase0)
            {
                return false;
            }

            var b1 = b[1L];

            if (b0 == hangulBase0) 
                return b1 >= hangulBase1;
            else if (b0 < hangulEnd0) 
                return true;
            else if (b0 > hangulEnd0) 
                return false;
            else if (b1 < hangulEnd1) 
                return true;
                        return b1 == hangulEnd1 && b[2L] < hangulEnd2;

        }

        // Caller must ensure len(b) >= 2.
        private static bool isJamoVT(slice<byte> b)
        { 
            // True if (rune & 0xff00) == jamoLBase
            return b[0L] == jamoLBase0 && (b[1L] & 0xFCUL) == jamoLBase1;

        }

        private static bool isHangulWithoutJamoT(slice<byte> b)
        {
            var (c, _) = utf8.DecodeRune(b);
            c -= hangulBase;
            return c < jamoLVTCount && c % jamoTCount == 0L;
        }

        // decomposeHangul writes the decomposed Hangul to buf and returns the number
        // of bytes written.  len(buf) should be at least 9.
        private static long decomposeHangul(slice<byte> buf, int r)
        {
            const long JamoUTF8Len = (long)3L;

            r -= hangulBase;
            var x = r % jamoTCount;
            r /= jamoTCount;
            utf8.EncodeRune(buf, jamoLBase + r / jamoVCount);
            utf8.EncodeRune(buf[JamoUTF8Len..], jamoVBase + r % jamoVCount);
            if (x != 0L)
            {
                utf8.EncodeRune(buf[2L * JamoUTF8Len..], jamoTBase + x);
                return 3L * JamoUTF8Len;
            }

            return 2L * JamoUTF8Len;

        }

        // decomposeHangul algorithmically decomposes a Hangul rune into
        // its Jamo components.
        // See https://unicode.org/reports/tr15/#Hangul for details on decomposing Hangul.
        private static void decomposeHangul(this ptr<reorderBuffer> _addr_rb, int r)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            r -= hangulBase;
            var x = r % jamoTCount;
            r /= jamoTCount;
            rb.appendRune(jamoLBase + r / jamoVCount);
            rb.appendRune(jamoVBase + r % jamoVCount);
            if (x != 0L)
            {
                rb.appendRune(jamoTBase + x);
            }

        }

        // combineHangul algorithmically combines Jamo character components into Hangul.
        // See https://unicode.org/reports/tr15/#Hangul for details on combining Hangul.
        private static void combineHangul(this ptr<reorderBuffer> _addr_rb, long s, long i, long k)
        {
            ref reorderBuffer rb = ref _addr_rb.val;

            var b = rb.rune[..];
            var bn = rb.nrune;
            while (i < bn)
            {
                var cccB = b[k - 1L].ccc;
                var cccC = b[i].ccc;
                if (cccB == 0L)
                {
                    s = k - 1L;
                i++;
                }

                if (s != k - 1L && cccB >= cccC)
                { 
                    // b[i] is blocked by greater-equal cccX below it
                    b[k] = b[i];
                    k++;

                }
                else
                {
                    var l = rb.runeAt(s); // also used to compare to hangulBase
                    var v = rb.runeAt(i); // also used to compare to jamoT

                    if (jamoLBase <= l && l < jamoLEnd && jamoVBase <= v && v < jamoVEnd) 
                        // 11xx plus 116x to LV
                        rb.assignRune(s, hangulBase + (l - jamoLBase) * jamoVTCount + (v - jamoVBase) * jamoTCount);
                    else if (hangulBase <= l && l < hangulEnd && jamoTBase < v && v < jamoTEnd && ((l - hangulBase) % jamoTCount) == 0L) 
                        // ACxx plus 11Ax to LVT
                        rb.assignRune(s, l + v - jamoTBase);
                    else 
                        b[k] = b[i];
                        k++;
                    
                }

            }

            rb.nrune = k;

        }

        // compose recombines the runes in the buffer.
        // It should only be used to recompose a single segment, as it will not
        // handle alternations between Hangul and non-Hangul characters correctly.
        private static void compose(this ptr<reorderBuffer> _addr_rb)
        {
            ref reorderBuffer rb = ref _addr_rb.val;
 
            // Lazily load the map used by the combine func below, but do
            // it outside of the loop.
            recompMapOnce.Do(buildRecompMap); 

            // UAX #15, section X5 , including Corrigendum #5
            // "In any character sequence beginning with starter S, a character C is
            //  blocked from S if and only if there is some character B between S
            //  and C, and either B is a starter or it has the same or higher
            //  combining class as C."
            var bn = rb.nrune;
            if (bn == 0L)
            {
                return ;
            }

            long k = 1L;
            var b = rb.rune[..];
            for (long s = 0L;
            long i = 1L; i < bn; i++)
            {
                if (isJamoVT(rb.bytesAt(i)))
                { 
                    // Redo from start in Hangul mode. Necessary to support
                    // U+320E..U+321E in NFKC mode.
                    rb.combineHangul(s, i, k);
                    return ;

                }

                var ii = b[i]; 
                // We can only use combineForward as a filter if we later
                // get the info for the combined character. This is more
                // expensive than using the filter. Using combinesBackward()
                // is safe.
                if (ii.combinesBackward())
                {
                    var cccB = b[k - 1L].ccc;
                    var cccC = ii.ccc;
                    var blocked = false; // b[i] blocked by starter or greater or equal CCC?
                    if (cccB == 0L)
                    {
                        s = k - 1L;
                    }
                    else
                    {
                        blocked = s != k - 1L && cccB >= cccC;
                    }

                    if (!blocked)
                    {
                        var combined = combine(rb.runeAt(s), rb.runeAt(i));
                        if (combined != 0L)
                        {
                            rb.assignRune(s, combined);
                            continue;
                        }

                    }

                }

                b[k] = b[i];
                k++;

            }

            rb.nrune = k;

        }
    }
}}}}}}
