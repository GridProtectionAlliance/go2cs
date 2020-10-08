// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2020 October 08 05:02:21 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\iter.go
using fmt = go.fmt_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class norm_package
    {
        // MaxSegmentSize is the maximum size of a byte buffer needed to consider any
        // sequence of starter and non-starter runes for the purpose of normalization.
        public static readonly var MaxSegmentSize = (var)maxByteBufferSize;

        // An Iter iterates over a string or byte slice, while normalizing it
        // to a given Form.


        // An Iter iterates over a string or byte slice, while normalizing it
        // to a given Form.
        public partial struct Iter
        {
            public reorderBuffer rb;
            public array<byte> buf;
            public Properties info; // first character saved from previous iteration
            public iterFunc next; // implementation of next depends on form
            public iterFunc asciiF;
            public long p; // current position in input source
            public slice<byte> multiSeg; // remainder of multi-segment decomposition
        }

        public delegate  slice<byte> iterFunc(ptr<Iter>);

        // Init initializes i to iterate over src after normalizing it to Form f.
        private static void Init(this ptr<Iter> _addr_i, Form f, slice<byte> src)
        {
            ref Iter i = ref _addr_i.val;

            i.p = 0L;
            if (len(src) == 0L)
            {
                i.setDone();
                i.rb.nsrc = 0L;
                return ;
            }

            i.multiSeg = null;
            i.rb.init(f, src);
            i.next = i.rb.f.nextMain;
            i.asciiF = nextASCIIBytes;
            i.info = i.rb.f.info(i.rb.src, i.p);
            i.rb.ss.first(i.info);

        }

        // InitString initializes i to iterate over src after normalizing it to Form f.
        private static void InitString(this ptr<Iter> _addr_i, Form f, @string src)
        {
            ref Iter i = ref _addr_i.val;

            i.p = 0L;
            if (len(src) == 0L)
            {
                i.setDone();
                i.rb.nsrc = 0L;
                return ;
            }

            i.multiSeg = null;
            i.rb.initString(f, src);
            i.next = i.rb.f.nextMain;
            i.asciiF = nextASCIIString;
            i.info = i.rb.f.info(i.rb.src, i.p);
            i.rb.ss.first(i.info);

        }

        // Seek sets the segment to be returned by the next call to Next to start
        // at position p.  It is the responsibility of the caller to set p to the
        // start of a segment.
        private static (long, error) Seek(this ptr<Iter> _addr_i, long offset, long whence)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Iter i = ref _addr_i.val;

            long abs = default;
            switch (whence)
            {
                case 0L: 
                    abs = offset;
                    break;
                case 1L: 
                    abs = int64(i.p) + offset;
                    break;
                case 2L: 
                    abs = int64(i.rb.nsrc) + offset;
                    break;
                default: 
                    return (0L, error.As(fmt.Errorf("norm: invalid whence"))!);
                    break;
            }
            if (abs < 0L)
            {
                return (0L, error.As(fmt.Errorf("norm: negative position"))!);
            }

            if (int(abs) >= i.rb.nsrc)
            {
                i.setDone();
                return (int64(i.p), error.As(null!)!);
            }

            i.p = int(abs);
            i.multiSeg = null;
            i.next = i.rb.f.nextMain;
            i.info = i.rb.f.info(i.rb.src, i.p);
            i.rb.ss.first(i.info);
            return (abs, error.As(null!)!);

        }

        // returnSlice returns a slice of the underlying input type as a byte slice.
        // If the underlying is of type []byte, it will simply return a slice.
        // If the underlying is of type string, it will copy the slice to the buffer
        // and return that.
        private static slice<byte> returnSlice(this ptr<Iter> _addr_i, long a, long b)
        {
            ref Iter i = ref _addr_i.val;

            if (i.rb.src.bytes == null)
            {
                return i.buf[..copy(i.buf[..], i.rb.src.str[a..b])];
            }

            return i.rb.src.bytes[a..b];

        }

        // Pos returns the byte position at which the next call to Next will commence processing.
        private static long Pos(this ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            return i.p;
        }

        private static void setDone(this ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            i.next = nextDone;
            i.p = i.rb.nsrc;
        }

        // Done returns true if there is no more input to process.
        private static bool Done(this ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            return i.p >= i.rb.nsrc;
        }

        // Next returns f(i.input[i.Pos():n]), where n is a boundary of i.input.
        // For any input a and b for which f(a) == f(b), subsequent calls
        // to Next will return the same segments.
        // Modifying runes are grouped together with the preceding starter, if such a starter exists.
        // Although not guaranteed, n will typically be the smallest possible n.
        private static slice<byte> Next(this ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            return i.next(i);
        }

        private static slice<byte> nextASCIIBytes(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            var p = i.p + 1L;
            if (p >= i.rb.nsrc)
            {
                var p0 = i.p;
                i.setDone();
                return i.rb.src.bytes[p0..p];
            }

            if (i.rb.src.bytes[p] < utf8.RuneSelf)
            {
                p0 = i.p;
                i.p = p;
                return i.rb.src.bytes[p0..p];
            }

            i.info = i.rb.f.info(i.rb.src, i.p);
            i.next = i.rb.f.nextMain;
            return i.next(i);

        }

        private static slice<byte> nextASCIIString(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            var p = i.p + 1L;
            if (p >= i.rb.nsrc)
            {
                i.buf[0L] = i.rb.src.str[i.p];
                i.setDone();
                return i.buf[..1L];
            }

            if (i.rb.src.str[p] < utf8.RuneSelf)
            {
                i.buf[0L] = i.rb.src.str[i.p];
                i.p = p;
                return i.buf[..1L];
            }

            i.info = i.rb.f.info(i.rb.src, i.p);
            i.next = i.rb.f.nextMain;
            return i.next(i);

        }

        private static slice<byte> nextHangul(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            var p = i.p;
            var next = p + hangulUTF8Size;
            if (next >= i.rb.nsrc)
            {
                i.setDone();
            }
            else if (i.rb.src.hangul(next) == 0L)
            {
                i.rb.ss.next(i.info);
                i.info = i.rb.f.info(i.rb.src, i.p);
                i.next = i.rb.f.nextMain;
                return i.next(i);
            }

            i.p = next;
            return i.buf[..decomposeHangul(i.buf[..], i.rb.src.hangul(p))];

        }

        private static slice<byte> nextDone(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            return null;
        }

        // nextMulti is used for iterating over multi-segment decompositions
        // for decomposing normal forms.
        private static slice<byte> nextMulti(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            long j = 0L;
            var d = i.multiSeg; 
            // skip first rune
            for (j = 1L; j < len(d) && !utf8.RuneStart(d[j]); j++)
            {
            }

            while (j < len(d))
            {
                var info = i.rb.f.info(new input(bytes:d), j);
                if (info.BoundaryBefore())
                {
                    i.multiSeg = d[j..];
                    return d[..j];
                }

                j += int(info.size);

            } 
            // treat last segment as normal decomposition
 
            // treat last segment as normal decomposition
            i.next = i.rb.f.nextMain;
            return i.next(i);

        }

        // nextMultiNorm is used for iterating over multi-segment decompositions
        // for composing normal forms.
        private static slice<byte> nextMultiNorm(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            long j = 0L;
            var d = i.multiSeg;
            while (j < len(d))
            {
                var info = i.rb.f.info(new input(bytes:d), j);
                if (info.BoundaryBefore())
                {
                    i.rb.compose();
                    var seg = i.buf[..i.rb.flushCopy(i.buf[..])];
                    i.rb.insertUnsafe(new input(bytes:d), j, info);
                    i.multiSeg = d[j + int(info.size)..];
                    return seg;
                }

                i.rb.insertUnsafe(new input(bytes:d), j, info);
                j += int(info.size);

            }

            i.multiSeg = null;
            i.next = nextComposed;
            return doNormComposed(_addr_i);

        }

        // nextDecomposed is the implementation of Next for forms NFD and NFKD.
        private static slice<byte> nextDecomposed(ptr<Iter> _addr_i)
        {
            slice<byte> next = default;
            ref Iter i = ref _addr_i.val;

            long outp = 0L;
            var inCopyStart = i.p;
            long outCopyStart = 0L;
            while (true)
            {
                {
                    var sz = int(i.info.size);

                    if (sz <= 1L)
                    {
                        i.rb.ss = 0L;
                        var p = i.p;
                        i.p++; // ASCII or illegal byte.  Either way, advance by 1.
                        if (i.p >= i.rb.nsrc)
                        {
                            i.setDone();
                            return i.returnSlice(p, i.p);
                        }
                        else if (i.rb.src._byte(i.p) < utf8.RuneSelf)
                        {
                            i.next = i.asciiF;
                            return i.returnSlice(p, i.p);
                        }

                        outp++;

                    }                    {
                        var d = i.info.Decomposition();


                        else if (d != null)
                        { 
                            // Note: If leading CCC != 0, then len(d) == 2 and last is also non-zero.
                            // Case 1: there is a leftover to copy.  In this case the decomposition
                            // must begin with a modifier and should always be appended.
                            // Case 2: no leftover. Simply return d if followed by a ccc == 0 value.
                            p = outp + len(d);
                            if (outp > 0L)
                            {
                                i.rb.src.copySlice(i.buf[outCopyStart..], inCopyStart, i.p); 
                                // TODO: this condition should not be possible, but we leave it
                                // in for defensive purposes.
                                if (p > len(i.buf))
                                {
                                    return i.buf[..outp];
                                }

                            }
                            else if (i.info.multiSegment())
                            { 
                                // outp must be 0 as multi-segment decompositions always
                                // start a new segment.
                                if (i.multiSeg == null)
                                {
                                    i.multiSeg = d;
                                    i.next = nextMulti;
                                    return nextMulti(_addr_i);
                                } 
                                // We are in the last segment.  Treat as normal decomposition.
                                d = i.multiSeg;
                                i.multiSeg = null;
                                p = len(d);

                            }

                            var prevCC = i.info.tccc;
                            i.p += sz;

                            if (i.p >= i.rb.nsrc)
                            {
                                i.setDone();
                                i.info = new Properties(); // Force BoundaryBefore to succeed.
                            }
                            else
                            {
                                i.info = i.rb.f.info(i.rb.src, i.p);
                            }


                            if (i.rb.ss.next(i.info) == ssOverflow)
                            {
                                i.next = nextCGJDecompose;
                                fallthrough = true;
                            }
                            if (fallthrough || i.rb.ss.next(i.info) == ssStarter)
                            {
                                if (outp > 0L)
                                {
                                    copy(i.buf[outp..], d);
                                    return i.buf[..p];
                                }

                                return d;
                                goto __switch_break0;
                            }

                            __switch_break0:;
                            copy(i.buf[outp..], d);
                            outp = p;
                            inCopyStart = i.p;
                            outCopyStart = outp;
                            if (i.info.ccc < prevCC)
                            {
                                goto doNorm;
                            }

                            continue;

                        }                        {
                            var r = i.rb.src.hangul(i.p);


                            else if (r != 0L)
                            {
                                outp = decomposeHangul(i.buf[..], r);
                                i.p += hangulUTF8Size;
                                inCopyStart = i.p;
                                outCopyStart = outp;
                                if (i.p >= i.rb.nsrc)
                                {
                                    i.setDone();
                                    break;
                                }
                                else if (i.rb.src.hangul(i.p) != 0L)
                                {
                                    i.next = nextHangul;
                                    return i.buf[..outp];
                                }

                            }
                            else
                            {
                                p = outp + sz;
                                if (p > len(i.buf))
                                {
                                    break;
                                }

                                outp = p;
                                i.p += sz;

                            }

                        }


                    }


                }

                if (i.p >= i.rb.nsrc)
                {
                    i.setDone();
                    break;
                }

                prevCC = i.info.tccc;
                i.info = i.rb.f.info(i.rb.src, i.p);
                {
                    var v = i.rb.ss.next(i.info);

                    if (v == ssStarter)
                    {
                        break;
                    }
                    else if (v == ssOverflow)
                    {
                        i.next = nextCGJDecompose;
                        break;
                    }


                }

                if (i.info.ccc < prevCC)
                {
                    goto doNorm;
                }

            }

            if (outCopyStart == 0L)
            {
                return i.returnSlice(inCopyStart, i.p);
            }
            else if (inCopyStart < i.p)
            {
                i.rb.src.copySlice(i.buf[outCopyStart..], inCopyStart, i.p);
            }

            return i.buf[..outp];
doNorm:
            i.rb.src.copySlice(i.buf[outCopyStart..], inCopyStart, i.p);
            i.rb.insertDecomposed(i.buf[0L..outp]);
            return doNormDecomposed(_addr_i);

        }

        private static slice<byte> doNormDecomposed(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            while (true)
            {
                i.rb.insertUnsafe(i.rb.src, i.p, i.info);
                i.p += int(i.info.size);

                if (i.p >= i.rb.nsrc)
                {
                    i.setDone();
                    break;
                }

                i.info = i.rb.f.info(i.rb.src, i.p);
                if (i.info.ccc == 0L)
                {
                    break;
                }

                {
                    var s = i.rb.ss.next(i.info);

                    if (s == ssOverflow)
                    {
                        i.next = nextCGJDecompose;
                        break;
                    }

                }

            } 
            // new segment or too many combining characters: exit normalization
 
            // new segment or too many combining characters: exit normalization
            return i.buf[..i.rb.flushCopy(i.buf[..])];

        }

        private static slice<byte> nextCGJDecompose(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            i.rb.ss = 0L;
            i.rb.insertCGJ();
            i.next = nextDecomposed;
            i.rb.ss.first(i.info);
            var buf = doNormDecomposed(_addr_i);
            return buf;
        }

        // nextComposed is the implementation of Next for forms NFC and NFKC.
        private static slice<byte> nextComposed(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            long outp = 0L;
            var startp = i.p;
            byte prevCC = default;
            while (true)
            {
                if (!i.info.isYesC())
                {
                    goto doNorm;
                }

                prevCC = i.info.tccc;
                var sz = int(i.info.size);
                if (sz == 0L)
                {
                    sz = 1L; // illegal rune: copy byte-by-byte
                }

                var p = outp + sz;
                if (p > len(i.buf))
                {
                    break;
                }

                outp = p;
                i.p += sz;
                if (i.p >= i.rb.nsrc)
                {
                    i.setDone();
                    break;
                }
                else if (i.rb.src._byte(i.p) < utf8.RuneSelf)
                {
                    i.rb.ss = 0L;
                    i.next = i.asciiF;
                    break;
                }

                i.info = i.rb.f.info(i.rb.src, i.p);
                {
                    var v = i.rb.ss.next(i.info);

                    if (v == ssStarter)
                    {
                        break;
                    }
                    else if (v == ssOverflow)
                    {
                        i.next = nextCGJCompose;
                        break;
                    }


                }

                if (i.info.ccc < prevCC)
                {
                    goto doNorm;
                }

            }

            return i.returnSlice(startp, i.p);
doNorm:
            i.p = startp;
            i.info = i.rb.f.info(i.rb.src, i.p);
            i.rb.ss.first(i.info);
            if (i.info.multiSegment())
            {
                var d = i.info.Decomposition();
                var info = i.rb.f.info(new input(bytes:d), 0L);
                i.rb.insertUnsafe(new input(bytes:d), 0L, info);
                i.multiSeg = d[int(info.size)..];
                i.next = nextMultiNorm;
                return nextMultiNorm(_addr_i);
            }

            i.rb.ss.first(i.info);
            i.rb.insertUnsafe(i.rb.src, i.p, i.info);
            return doNormComposed(_addr_i);

        }

        private static slice<byte> doNormComposed(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;
 
            // First rune should already be inserted.
            while (true)
            {
                i.p += int(i.info.size);

                if (i.p >= i.rb.nsrc)
                {
                    i.setDone();
                    break;
                }

                i.info = i.rb.f.info(i.rb.src, i.p);
                {
                    var s = i.rb.ss.next(i.info);

                    if (s == ssStarter)
                    {
                        break;
                    }
                    else if (s == ssOverflow)
                    {
                        i.next = nextCGJCompose;
                        break;
                    }


                }

                i.rb.insertUnsafe(i.rb.src, i.p, i.info);

            }

            i.rb.compose();
            var seg = i.buf[..i.rb.flushCopy(i.buf[..])];
            return seg;

        }

        private static slice<byte> nextCGJCompose(ptr<Iter> _addr_i)
        {
            ref Iter i = ref _addr_i.val;

            i.rb.ss = 0L; // instead of first
            i.rb.insertCGJ();
            i.next = nextComposed; 
            // Note that we treat any rune with nLeadingNonStarters > 0 as a non-starter,
            // even if they are not. This is particularly dubious for U+FF9E and UFF9A.
            // If we ever change that, insert a check here.
            i.rb.ss.first(i.info);
            i.rb.insertUnsafe(i.rb.src, i.p, i.info);
            return doNormComposed(_addr_i);

        }
    }
}}}}}}
