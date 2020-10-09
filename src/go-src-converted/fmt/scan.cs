// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fmt -- go2cs converted at 2020 October 09 05:07:59 UTC
// import "fmt" ==> using fmt = go.fmt_package
// Original source: C:\Go\src\fmt\scan.go
using errors = go.errors_package;
using io = go.io_package;
using math = go.math_package;
using os = go.os_package;
using reflect = go.reflect_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class fmt_package
    {
        // ScanState represents the scanner state passed to custom scanners.
        // Scanners may do rune-at-a-time scanning or ask the ScanState
        // to discover the next space-delimited token.
        public partial interface ScanState
        {
            (long, error) ReadRune(); // UnreadRune causes the next call to ReadRune to return the same rune.
            (long, error) UnreadRune(); // SkipSpace skips space in the input. Newlines are treated appropriately
// for the operation being performed; see the package documentation
// for more information.
            (long, error) SkipSpace(); // Token skips space in the input if skipSpace is true, then returns the
// run of Unicode code points c satisfying f(c).  If f is nil,
// !unicode.IsSpace(c) is used; that is, the token will hold non-space
// characters. Newlines are treated appropriately for the operation being
// performed; see the package documentation for more information.
// The returned slice points to shared data that may be overwritten
// by the next call to Token, a call to a Scan function using the ScanState
// as input, or when the calling Scan method returns.
            (long, error) Token(bool skipSpace, Func<int, bool> f); // Width returns the value of the width option and whether it has been set.
// The unit is Unicode code points.
            (long, error) Width(); // Because ReadRune is implemented by the interface, Read should never be
// called by the scanning routines and a valid implementation of
// ScanState may choose always to return an error from Read.
            (long, error) Read(slice<byte> buf);
        }

        // Scanner is implemented by any value that has a Scan method, which scans
        // the input for the representation of a value and stores the result in the
        // receiver, which must be a pointer to be useful. The Scan method is called
        // for any argument to Scan, Scanf, or Scanln that implements it.
        public partial interface Scanner
        {
            error Scan(ScanState state, int verb);
        }

        // Scan scans text read from standard input, storing successive
        // space-separated values into successive arguments. Newlines count
        // as space. It returns the number of items successfully scanned.
        // If that is less than the number of arguments, err will report why.
        public static (long, error) Scan(params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscan(os.Stdin, a);
        }

        // Scanln is similar to Scan, but stops scanning at a newline and
        // after the final item there must be a newline or EOF.
        public static (long, error) Scanln(params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscanln(os.Stdin, a);
        }

        // Scanf scans text read from standard input, storing successive
        // space-separated values into successive arguments as determined by
        // the format. It returns the number of items successfully scanned.
        // If that is less than the number of arguments, err will report why.
        // Newlines in the input must match newlines in the format.
        // The one exception: the verb %c always scans the next rune in the
        // input, even if it is a space (or tab etc.) or newline.
        public static (long, error) Scanf(@string format, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscanf(os.Stdin, format, a);
        }

        private partial struct stringReader // : @string
        {
        }

        private static (long, error) Read(this ptr<stringReader> _addr_r, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref stringReader r = ref _addr_r.val;

            n = copy(b, r.val);
            r.val = (r.val)[n..];
            if (n == 0L)
            {
                err = io.EOF;
            }

            return ;

        }

        // Sscan scans the argument string, storing successive space-separated
        // values into successive arguments. Newlines count as space. It
        // returns the number of items successfully scanned. If that is less
        // than the number of arguments, err will report why.
        public static (long, error) Sscan(@string str, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscan((stringReader.val)(_addr_str), a);
        }

        // Sscanln is similar to Sscan, but stops scanning at a newline and
        // after the final item there must be a newline or EOF.
        public static (long, error) Sscanln(@string str, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscanln((stringReader.val)(_addr_str), a);
        }

        // Sscanf scans the argument string, storing successive space-separated
        // values into successive arguments as determined by the format. It
        // returns the number of items successfully parsed.
        // Newlines in the input must match newlines in the format.
        public static (long, error) Sscanf(@string str, @string format, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            return Fscanf((stringReader.val)(_addr_str), format, a);
        }

        // Fscan scans text read from r, storing successive space-separated
        // values into successive arguments. Newlines count as space. It
        // returns the number of items successfully scanned. If that is less
        // than the number of arguments, err will report why.
        public static (long, error) Fscan(io.Reader r, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            var (s, old) = newScanState(r, true, false);
            n, err = s.doScan(a);
            s.free(old);
            return ;
        }

        // Fscanln is similar to Fscan, but stops scanning at a newline and
        // after the final item there must be a newline or EOF.
        public static (long, error) Fscanln(io.Reader r, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            var (s, old) = newScanState(r, false, true);
            n, err = s.doScan(a);
            s.free(old);
            return ;
        }

        // Fscanf scans text read from r, storing successive space-separated
        // values into successive arguments as determined by the format. It
        // returns the number of items successfully parsed.
        // Newlines in the input must match newlines in the format.
        public static (long, error) Fscanf(io.Reader r, @string format, params object[] a)
        {
            long n = default;
            error err = default!;
            a = a.Clone();

            var (s, old) = newScanState(r, false, false);
            n, err = s.doScanf(format, a);
            s.free(old);
            return ;
        }

        // scanError represents an error generated by the scanning software.
        // It's used as a unique signature to identify such errors when recovering.
        private partial struct scanError
        {
            public error err;
        }

        private static readonly long eof = (long)-1L;

        // ss is the internal implementation of ScanState.


        // ss is the internal implementation of ScanState.
        private partial struct ss
        {
            public io.RuneScanner rs; // where to read input
            public buffer buf; // token accumulator
            public long count; // runes consumed so far.
            public bool atEOF; // already read EOF
            public ref ssave ssave => ref ssave_val;
        }

        // ssave holds the parts of ss that need to be
        // saved and restored on recursive scans.
        private partial struct ssave
        {
            public bool validSave; // is or was a part of an actual ss.
            public bool nlIsEnd; // whether newline terminates scan
            public bool nlIsSpace; // whether newline counts as white space
            public long argLimit; // max value of ss.count for this arg; argLimit <= limit
            public long limit; // max value of ss.count.
            public long maxWid; // width of this arg.
        }

        // The Read method is only in ScanState so that ScanState
        // satisfies io.Reader. It will never be called when used as
        // intended, so there is no need to make it actually work.
        private static (long, error) Read(this ptr<ss> _addr_s, slice<byte> buf)
        {
            long n = default;
            error err = default!;
            ref ss s = ref _addr_s.val;

            return (0L, error.As(errors.New("ScanState's Read should not be called. Use ReadRune"))!);
        }

        private static (int, long, error) ReadRune(this ptr<ss> _addr_s)
        {
            int r = default;
            long size = default;
            error err = default!;
            ref ss s = ref _addr_s.val;

            if (s.atEOF || s.count >= s.argLimit)
            {
                err = io.EOF;
                return ;
            }

            r, size, err = s.rs.ReadRune();
            if (err == null)
            {
                s.count++;
                if (s.nlIsEnd && r == '\n')
                {
                    s.atEOF = true;
                }

            }
            else if (err == io.EOF)
            {
                s.atEOF = true;
            }

            return ;

        }

        private static (long, bool) Width(this ptr<ss> _addr_s)
        {
            long wid = default;
            bool ok = default;
            ref ss s = ref _addr_s.val;

            if (s.maxWid == hugeWid)
            {
                return (0L, false);
            }

            return (s.maxWid, true);

        }

        // The public method returns an error; this private one panics.
        // If getRune reaches EOF, the return value is EOF (-1).
        private static int getRune(this ptr<ss> _addr_s)
        {
            int r = default;
            ref ss s = ref _addr_s.val;

            var (r, _, err) = s.ReadRune();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    return eof;
                }

                s.error(err);

            }

            return ;

        }

        // mustReadRune turns io.EOF into a panic(io.ErrUnexpectedEOF).
        // It is called in cases such as string scanning where an EOF is a
        // syntax error.
        private static int mustReadRune(this ptr<ss> _addr_s)
        {
            int r = default;
            ref ss s = ref _addr_s.val;

            r = s.getRune();
            if (r == eof)
            {
                s.error(io.ErrUnexpectedEOF);
            }

            return ;

        }

        private static error UnreadRune(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            s.rs.UnreadRune();
            s.atEOF = false;
            s.count--;
            return error.As(null!)!;
        }

        private static void error(this ptr<ss> _addr_s, error err) => func((_, panic, __) =>
        {
            ref ss s = ref _addr_s.val;

            panic(new scanError(err));
        });

        private static void errorString(this ptr<ss> _addr_s, @string err) => func((_, panic, __) =>
        {
            ref ss s = ref _addr_s.val;

            panic(new scanError(errors.New(err)));
        });

        private static (slice<byte>, error) Token(this ptr<ss> _addr_s, bool skipSpace, Func<int, bool> f) => func((defer, panic, _) =>
        {
            slice<byte> tok = default;
            error err = default!;
            ref ss s = ref _addr_s.val;

            defer(() =>
            {
                {
                    var e = recover();

                    if (e != null)
                    {
                        {
                            scanError (se, ok) = e._<scanError>();

                            if (ok)
                            {
                                err = se.err;
                            }
                            else
                            {
                                panic(e);
                            }

                        }

                    }

                }

            }());
            if (f == null)
            {
                f = notSpace;
            }

            s.buf = s.buf[..0L];
            tok = s.token(skipSpace, f);
            return ;

        });

        // space is a copy of the unicode.White_Space ranges,
        // to avoid depending on package unicode.
        private static array<ushort> space = new slice<array<ushort>>(new array<ushort>[] { {0x0009,0x000d}, {0x0020,0x0020}, {0x0085,0x0085}, {0x00a0,0x00a0}, {0x1680,0x1680}, {0x2000,0x200a}, {0x2028,0x2029}, {0x202f,0x202f}, {0x205f,0x205f}, {0x3000,0x3000} });

        private static bool isSpace(int r)
        {
            if (r >= 1L << (int)(16L))
            {
                return false;
            }

            var rx = uint16(r);
            foreach (var (_, rng) in space)
            {
                if (rx < rng[0L])
                {
                    return false;
                }

                if (rx <= rng[1L])
                {
                    return true;
                }

            }
            return false;

        }

        // notSpace is the default scanning function used in Token.
        private static bool notSpace(int r)
        {
            return !isSpace(r);
        }

        // readRune is a structure to enable reading UTF-8 encoded code points
        // from an io.Reader. It is used if the Reader given to the scanner does
        // not already implement io.RuneScanner.
        private partial struct readRune
        {
            public io.Reader reader;
            public array<byte> buf; // used only inside ReadRune
            public long pending; // number of bytes in pendBuf; only >0 for bad UTF-8
            public array<byte> pendBuf; // bytes left over
            public int peekRune; // if >=0 next rune; when <0 is ^(previous Rune)
        }

        // readByte returns the next byte from the input, which may be
        // left over from a previous read if the UTF-8 was ill-formed.
        private static (byte, error) readByte(this ptr<readRune> _addr_r)
        {
            byte b = default;
            error err = default!;
            ref readRune r = ref _addr_r.val;

            if (r.pending > 0L)
            {
                b = r.pendBuf[0L];
                copy(r.pendBuf[0L..], r.pendBuf[1L..]);
                r.pending--;
                return ;
            }

            var (n, err) = io.ReadFull(r.reader, r.pendBuf[..1L]);
            if (n != 1L)
            {
                return (0L, error.As(err)!);
            }

            return (r.pendBuf[0L], error.As(err)!);

        }

        // ReadRune returns the next UTF-8 encoded code point from the
        // io.Reader inside r.
        private static (int, long, error) ReadRune(this ptr<readRune> _addr_r)
        {
            int rr = default;
            long size = default;
            error err = default!;
            ref readRune r = ref _addr_r.val;

            if (r.peekRune >= 0L)
            {
                rr = r.peekRune;
                r.peekRune = ~r.peekRune;
                size = utf8.RuneLen(rr);
                return ;
            }

            r.buf[0L], err = r.readByte();
            if (err != null)
            {
                return ;
            }

            if (r.buf[0L] < utf8.RuneSelf)
            { // fast check for common ASCII case
                rr = rune(r.buf[0L]);
                size = 1L; // Known to be 1.
                // Flip the bits of the rune so it's available to UnreadRune.
                r.peekRune = ~rr;
                return ;

            }

            long n = default;
            for (n = 1L; !utf8.FullRune(r.buf[..n]); n++)
            {
                r.buf[n], err = r.readByte();
                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = null;
                        break;
                    }

                    return ;

                }

            }

            rr, size = utf8.DecodeRune(r.buf[..n]);
            if (size < n)
            { // an error, save the bytes for the next read
                copy(r.pendBuf[r.pending..], r.buf[size..n]);
                r.pending += n - size;

            } 
            // Flip the bits of the rune so it's available to UnreadRune.
            r.peekRune = ~rr;
            return ;

        }

        private static error UnreadRune(this ptr<readRune> _addr_r)
        {
            ref readRune r = ref _addr_r.val;

            if (r.peekRune >= 0L)
            {
                return error.As(errors.New("fmt: scanning called UnreadRune with no rune available"))!;
            } 
            // Reverse bit flip of previously read rune to obtain valid >=0 state.
            r.peekRune = ~r.peekRune;
            return error.As(null!)!;

        }

        private static sync.Pool ssFree = new sync.Pool(New:func()interface{}{returnnew(ss)},);

        // newScanState allocates a new ss struct or grab a cached one.
        private static (ptr<ss>, ssave) newScanState(io.Reader r, bool nlIsSpace, bool nlIsEnd)
        {
            ptr<ss> s = default!;
            ssave old = default;

            s = ssFree.Get()._<ptr<ss>>();
            {
                io.RuneScanner (rs, ok) = r._<io.RuneScanner>();

                if (ok)
                {
                    s.rs = rs;
                }
                else
                {
                    s.rs = addr(new readRune(reader:r,peekRune:-1));
                }

            }

            s.nlIsSpace = nlIsSpace;
            s.nlIsEnd = nlIsEnd;
            s.atEOF = false;
            s.limit = hugeWid;
            s.argLimit = hugeWid;
            s.maxWid = hugeWid;
            s.validSave = true;
            s.count = 0L;
            return ;

        }

        // free saves used ss structs in ssFree; avoid an allocation per invocation.
        private static void free(this ptr<ss> _addr_s, ssave old)
        {
            ref ss s = ref _addr_s.val;
 
            // If it was used recursively, just restore the old state.
            if (old.validSave)
            {
                s.ssave = old;
                return ;
            } 
            // Don't hold on to ss structs with large buffers.
            if (cap(s.buf) > 1024L)
            {
                return ;
            }

            s.buf = s.buf[..0L];
            s.rs = null;
            ssFree.Put(s);

        }

        // SkipSpace provides Scan methods the ability to skip space and newline
        // characters in keeping with the current scanning mode set by format strings
        // and Scan/Scanln.
        private static void SkipSpace(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            while (true)
            {
                var r = s.getRune();
                if (r == eof)
                {
                    return ;
                }

                if (r == '\r' && s.peek("\n"))
                {
                    continue;
                }

                if (r == '\n')
                {
                    if (s.nlIsSpace)
                    {
                        continue;
                    }

                    s.errorString("unexpected newline");
                    return ;

                }

                if (!isSpace(r))
                {
                    s.UnreadRune();
                    break;
                }

            }


        }

        // token returns the next space-delimited string from the input. It
        // skips white space. For Scanln, it stops at newlines. For Scan,
        // newlines are treated as spaces.
        private static slice<byte> token(this ptr<ss> _addr_s, bool skipSpace, Func<int, bool> f)
        {
            ref ss s = ref _addr_s.val;

            if (skipSpace)
            {
                s.SkipSpace();
            } 
            // read until white space or newline
            while (true)
            {
                var r = s.getRune();
                if (r == eof)
                {
                    break;
                }

                if (!f(r))
                {
                    s.UnreadRune();
                    break;
                }

                s.buf.writeRune(r);

            }

            return s.buf;

        }

        private static var complexError = errors.New("syntax error scanning complex number");
        private static var boolError = errors.New("syntax error scanning boolean");

        private static long indexRune(@string s, int r)
        {
            foreach (var (i, c) in s)
            {
                if (c == r)
                {
                    return i;
                }

            }
            return -1L;

        }

        // consume reads the next rune in the input and reports whether it is in the ok string.
        // If accept is true, it puts the character into the input token.
        private static bool consume(this ptr<ss> _addr_s, @string ok, bool accept)
        {
            ref ss s = ref _addr_s.val;

            var r = s.getRune();
            if (r == eof)
            {
                return false;
            }

            if (indexRune(ok, r) >= 0L)
            {
                if (accept)
                {
                    s.buf.writeRune(r);
                }

                return true;

            }

            if (r != eof && accept)
            {
                s.UnreadRune();
            }

            return false;

        }

        // peek reports whether the next character is in the ok string, without consuming it.
        private static bool peek(this ptr<ss> _addr_s, @string ok)
        {
            ref ss s = ref _addr_s.val;

            var r = s.getRune();
            if (r != eof)
            {
                s.UnreadRune();
            }

            return indexRune(ok, r) >= 0L;

        }

        private static void notEOF(this ptr<ss> _addr_s) => func((_, panic, __) =>
        {
            ref ss s = ref _addr_s.val;
 
            // Guarantee there is data to be read.
            {
                var r = s.getRune();

                if (r == eof)
                {
                    panic(io.EOF);
                }

            }

            s.UnreadRune();

        });

        // accept checks the next rune in the input. If it's a byte (sic) in the string, it puts it in the
        // buffer and returns true. Otherwise it return false.
        private static bool accept(this ptr<ss> _addr_s, @string ok)
        {
            ref ss s = ref _addr_s.val;

            return s.consume(ok, true);
        }

        // okVerb verifies that the verb is present in the list, setting s.err appropriately if not.
        private static bool okVerb(this ptr<ss> _addr_s, int verb, @string okVerbs, @string typ)
        {
            ref ss s = ref _addr_s.val;

            foreach (var (_, v) in okVerbs)
            {
                if (v == verb)
                {
                    return true;
                }

            }
            s.errorString("bad verb '%" + string(verb) + "' for " + typ);
            return false;

        }

        // scanBool returns the value of the boolean represented by the next token.
        private static bool scanBool(this ptr<ss> _addr_s, int verb)
        {
            ref ss s = ref _addr_s.val;

            s.SkipSpace();
            s.notEOF();
            if (!s.okVerb(verb, "tv", "boolean"))
            {
                return false;
            } 
            // Syntax-checking a boolean is annoying. We're not fastidious about case.
            switch (s.getRune())
            {
                case '0': 
                    return false;
                    break;
                case '1': 
                    return true;
                    break;
                case 't': 

                case 'T': 
                    if (s.accept("rR") && (!s.accept("uU") || !s.accept("eE")))
                    {
                        s.error(boolError);
                    }

                    return true;
                    break;
                case 'f': 

                case 'F': 
                    if (s.accept("aA") && (!s.accept("lL") || !s.accept("sS") || !s.accept("eE")))
                    {
                        s.error(boolError);
                    }

                    return false;
                    break;
            }
            return false;

        }

        // Numerical elements
        private static readonly @string binaryDigits = (@string)"01";
        private static readonly @string octalDigits = (@string)"01234567";
        private static readonly @string decimalDigits = (@string)"0123456789";
        private static readonly @string hexadecimalDigits = (@string)"0123456789aAbBcCdDeEfF";
        private static readonly @string sign = (@string)"+-";
        private static readonly @string period = (@string)".";
        private static readonly @string exponent = (@string)"eEpP";


        // getBase returns the numeric base represented by the verb and its digit string.
        private static (long, @string) getBase(this ptr<ss> _addr_s, int verb)
        {
            long @base = default;
            @string digits = default;
            ref ss s = ref _addr_s.val;

            s.okVerb(verb, "bdoUxXv", "integer"); // sets s.err
            base = 10L;
            digits = decimalDigits;
            switch (verb)
            {
                case 'b': 
                    base = 2L;
                    digits = binaryDigits;
                    break;
                case 'o': 
                    base = 8L;
                    digits = octalDigits;
                    break;
                case 'x': 

                case 'X': 

                case 'U': 
                    base = 16L;
                    digits = hexadecimalDigits;
                    break;
            }
            return ;

        }

        // scanNumber returns the numerical string with specified digits starting here.
        private static @string scanNumber(this ptr<ss> _addr_s, @string digits, bool haveDigits)
        {
            ref ss s = ref _addr_s.val;

            if (!haveDigits)
            {
                s.notEOF();
                if (!s.accept(digits))
                {
                    s.errorString("expected integer");
                }

            }

            while (s.accept(digits))
            {
            }

            return string(s.buf);

        }

        // scanRune returns the next rune value in the input.
        private static long scanRune(this ptr<ss> _addr_s, long bitSize)
        {
            ref ss s = ref _addr_s.val;

            s.notEOF();
            var r = s.getRune();
            var n = uint(bitSize);
            var x = (int64(r) << (int)((64L - n))) >> (int)((64L - n));
            if (x != int64(r))
            {
                s.errorString("overflow on character value " + string(r));
            }

            return int64(r);

        }

        // scanBasePrefix reports whether the integer begins with a base prefix
        // and returns the base, digit string, and whether a zero was found.
        // It is called only if the verb is %v.
        private static (long, @string, bool) scanBasePrefix(this ptr<ss> _addr_s)
        {
            long @base = default;
            @string digits = default;
            bool zeroFound = default;
            ref ss s = ref _addr_s.val;

            if (!s.peek("0"))
            {
                return (0L, decimalDigits + "_", false);
            }

            s.accept("0"); 
            // Special cases for 0, 0b, 0o, 0x.

            if (s.peek("bB")) 
                s.consume("bB", true);
                return (0L, binaryDigits + "_", true);
            else if (s.peek("oO")) 
                s.consume("oO", true);
                return (0L, octalDigits + "_", true);
            else if (s.peek("xX")) 
                s.consume("xX", true);
                return (0L, hexadecimalDigits + "_", true);
            else 
                return (0L, octalDigits + "_", true);
            
        }

        // scanInt returns the value of the integer represented by the next
        // token, checking for overflow. Any error is stored in s.err.
        private static long scanInt(this ptr<ss> _addr_s, int verb, long bitSize)
        {
            ref ss s = ref _addr_s.val;

            if (verb == 'c')
            {
                return s.scanRune(bitSize);
            }

            s.SkipSpace();
            s.notEOF();
            var (base, digits) = s.getBase(verb);
            var haveDigits = false;
            if (verb == 'U')
            {
                if (!s.consume("U", false) || !s.consume("+", false))
                {
                    s.errorString("bad unicode format ");
                }

            }
            else
            {
                s.accept(sign); // If there's a sign, it will be left in the token buffer.
                if (verb == 'v')
                {
                    base, digits, haveDigits = s.scanBasePrefix();
                }

            }

            var tok = s.scanNumber(digits, haveDigits);
            var (i, err) = strconv.ParseInt(tok, base, 64L);
            if (err != null)
            {
                s.error(err);
            }

            var n = uint(bitSize);
            var x = (i << (int)((64L - n))) >> (int)((64L - n));
            if (x != i)
            {
                s.errorString("integer overflow on token " + tok);
            }

            return i;

        }

        // scanUint returns the value of the unsigned integer represented
        // by the next token, checking for overflow. Any error is stored in s.err.
        private static ulong scanUint(this ptr<ss> _addr_s, int verb, long bitSize)
        {
            ref ss s = ref _addr_s.val;

            if (verb == 'c')
            {
                return uint64(s.scanRune(bitSize));
            }

            s.SkipSpace();
            s.notEOF();
            var (base, digits) = s.getBase(verb);
            var haveDigits = false;
            if (verb == 'U')
            {
                if (!s.consume("U", false) || !s.consume("+", false))
                {
                    s.errorString("bad unicode format ");
                }

            }
            else if (verb == 'v')
            {
                base, digits, haveDigits = s.scanBasePrefix();
            }

            var tok = s.scanNumber(digits, haveDigits);
            var (i, err) = strconv.ParseUint(tok, base, 64L);
            if (err != null)
            {
                s.error(err);
            }

            var n = uint(bitSize);
            var x = (i << (int)((64L - n))) >> (int)((64L - n));
            if (x != i)
            {
                s.errorString("unsigned integer overflow on token " + tok);
            }

            return i;

        }

        // floatToken returns the floating-point number starting here, no longer than swid
        // if the width is specified. It's not rigorous about syntax because it doesn't check that
        // we have at least some digits, but Atof will do that.
        private static @string floatToken(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            s.buf = s.buf[..0L]; 
            // NaN?
            if (s.accept("nN") && s.accept("aA") && s.accept("nN"))
            {
                return string(s.buf);
            } 
            // leading sign?
            s.accept(sign); 
            // Inf?
            if (s.accept("iI") && s.accept("nN") && s.accept("fF"))
            {
                return string(s.buf);
            }

            var digits = decimalDigits + "_";
            var exp = exponent;
            if (s.accept("0") && s.accept("xX"))
            {
                digits = hexadecimalDigits + "_";
                exp = "pP";
            } 
            // digits?
            while (s.accept(digits))
            {
            } 
            // decimal point?
 
            // decimal point?
            if (s.accept(period))
            { 
                // fraction?
                while (s.accept(digits))
                {
                }


            } 
            // exponent?
            if (s.accept(exp))
            { 
                // leading sign?
                s.accept(sign); 
                // digits?
                while (s.accept(decimalDigits + "_"))
                {
                }


            }

            return string(s.buf);

        }

        // complexTokens returns the real and imaginary parts of the complex number starting here.
        // The number might be parenthesized and has the format (N+Ni) where N is a floating-point
        // number and there are no spaces within.
        private static (@string, @string) complexTokens(this ptr<ss> _addr_s)
        {
            @string real = default;
            @string imag = default;
            ref ss s = ref _addr_s.val;
 
            // TODO: accept N and Ni independently?
            var parens = s.accept("(");
            real = s.floatToken();
            s.buf = s.buf[..0L]; 
            // Must now have a sign.
            if (!s.accept("+-"))
            {
                s.error(complexError);
            } 
            // Sign is now in buffer
            var imagSign = string(s.buf);
            imag = s.floatToken();
            if (!s.accept("i"))
            {
                s.error(complexError);
            }

            if (parens && !s.accept(")"))
            {
                s.error(complexError);
            }

            return (real, imagSign + imag);

        }

        private static bool hasX(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == 'x' || s[i] == 'X')
                {
                    return true;
                }

            }

            return false;

        }

        // convertFloat converts the string to a float64value.
        private static double convertFloat(this ptr<ss> _addr_s, @string str, long n)
        {
            ref ss s = ref _addr_s.val;
 
            // strconv.ParseFloat will handle "+0x1.fp+2",
            // but we have to implement our non-standard
            // decimal+binary exponent mix (1.2p4) ourselves.
            {
                var p = indexRune(str, 'p');

                if (p >= 0L && !hasX(str))
                { 
                    // Atof doesn't handle power-of-2 exponents,
                    // but they're easy to evaluate.
                    var (f, err) = strconv.ParseFloat(str[..p], n);
                    if (err != null)
                    { 
                        // Put full string into error.
                        {
                            ptr<strconv.NumError> e__prev3 = e;

                            ptr<strconv.NumError> (e, ok) = err._<ptr<strconv.NumError>>();

                            if (ok)
                            {
                                e.Num = str;
                            }

                            e = e__prev3;

                        }

                        s.error(err);

                    }

                    var (m, err) = strconv.Atoi(str[p + 1L..]);
                    if (err != null)
                    { 
                        // Put full string into error.
                        {
                            ptr<strconv.NumError> e__prev3 = e;

                            (e, ok) = err._<ptr<strconv.NumError>>();

                            if (ok)
                            {
                                e.Num = str;
                            }

                            e = e__prev3;

                        }

                        s.error(err);

                    }

                    return math.Ldexp(f, m);

                }

            }

            (f, err) = strconv.ParseFloat(str, n);
            if (err != null)
            {
                s.error(err);
            }

            return f;

        }

        // convertComplex converts the next token to a complex128 value.
        // The atof argument is a type-specific reader for the underlying type.
        // If we're reading complex64, atof will parse float32s and convert them
        // to float64's to avoid reproducing this code for each complex type.
        private static System.Numerics.Complex128 scanComplex(this ptr<ss> _addr_s, int verb, long n)
        {
            ref ss s = ref _addr_s.val;

            if (!s.okVerb(verb, floatVerbs, "complex"))
            {
                return 0L;
            }

            s.SkipSpace();
            s.notEOF();
            var (sreal, simag) = s.complexTokens();
            var real = s.convertFloat(sreal, n / 2L);
            var imag = s.convertFloat(simag, n / 2L);
            return complex(real, imag);

        }

        // convertString returns the string represented by the next input characters.
        // The format of the input is determined by the verb.
        private static @string convertString(this ptr<ss> _addr_s, int verb)
        {
            @string str = default;
            ref ss s = ref _addr_s.val;

            if (!s.okVerb(verb, "svqxX", "string"))
            {
                return "";
            }

            s.SkipSpace();
            s.notEOF();
            switch (verb)
            {
                case 'q': 
                    str = s.quotedString();
                    break;
                case 'x': 

                case 'X': 
                    str = s.hexString();
                    break;
                default: 
                    str = string(s.token(true, notSpace)); // %s and %v just return the next word
                    break;
            }
            return ;

        }

        // quotedString returns the double- or back-quoted string represented by the next input characters.
        private static @string quotedString(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            s.notEOF();
            var quote = s.getRune();
            switch (quote)
            {
                case '`': 
                    // Back-quoted: Anything goes until EOF or back quote.
                    while (true)
                    {
                        var r = s.mustReadRune();
                        if (r == quote)
                        {
                            break;
                        }

                        s.buf.writeRune(r);

                    }

                    return string(s.buf);
                    break;
                case '"': 
                    // Double-quoted: Include the quotes and let strconv.Unquote do the backslash escapes.
                    s.buf.writeByte('"');
                    while (true)
                    {
                        r = s.mustReadRune();
                        s.buf.writeRune(r);
                        if (r == '\\')
                        { 
                            // In a legal backslash escape, no matter how long, only the character
                            // immediately after the escape can itself be a backslash or quote.
                            // Thus we only need to protect the first character after the backslash.
                            s.buf.writeRune(s.mustReadRune());

                        }
                        else if (r == '"')
                        {
                            break;
                        }

                    }

                    var (result, err) = strconv.Unquote(string(s.buf));
                    if (err != null)
                    {
                        s.error(err);
                    }

                    return result;
                    break;
                default: 
                    s.errorString("expected quoted string");
                    break;
            }
            return "";

        }

        // hexDigit returns the value of the hexadecimal digit.
        private static (long, bool) hexDigit(int d)
        {
            long _p0 = default;
            bool _p0 = default;

            var digit = int(d);
            switch (digit)
            {
                case '0': 

                case '1': 

                case '2': 

                case '3': 

                case '4': 

                case '5': 

                case '6': 

                case '7': 

                case '8': 

                case '9': 
                    return (digit - '0', true);
                    break;
                case 'a': 

                case 'b': 

                case 'c': 

                case 'd': 

                case 'e': 

                case 'f': 
                    return (10L + digit - 'a', true);
                    break;
                case 'A': 

                case 'B': 

                case 'C': 

                case 'D': 

                case 'E': 

                case 'F': 
                    return (10L + digit - 'A', true);
                    break;
            }
            return (-1L, false);

        }

        // hexByte returns the next hex-encoded (two-character) byte from the input.
        // It returns ok==false if the next bytes in the input do not encode a hex byte.
        // If the first byte is hex and the second is not, processing stops.
        private static (byte, bool) hexByte(this ptr<ss> _addr_s)
        {
            byte b = default;
            bool ok = default;
            ref ss s = ref _addr_s.val;

            var rune1 = s.getRune();
            if (rune1 == eof)
            {
                return ;
            }

            var (value1, ok) = hexDigit(rune1);
            if (!ok)
            {
                s.UnreadRune();
                return ;
            }

            var (value2, ok) = hexDigit(s.mustReadRune());
            if (!ok)
            {
                s.errorString("illegal hex digit");
                return ;
            }

            return (byte(value1 << (int)(4L) | value2), true);

        }

        // hexString returns the space-delimited hexpair-encoded string.
        private static @string hexString(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            s.notEOF();
            while (true)
            {
                var (b, ok) = s.hexByte();
                if (!ok)
                {
                    break;
                }

                s.buf.writeByte(b);

            }

            if (len(s.buf) == 0L)
            {
                s.errorString("no hex data for %x string");
                return "";
            }

            return string(s.buf);

        }

        private static readonly @string floatVerbs = (@string)"beEfFgGv";

        private static readonly long hugeWid = (long)1L << (int)(30L);

        private static readonly long intBits = (long)32L << (int)((~uint(0L) >> (int)(63L)));
        private static readonly long uintptrBits = (long)32L << (int)((~uintptr(0L) >> (int)(63L)));


        // scanPercent scans a literal percent character.
        private static void scanPercent(this ptr<ss> _addr_s)
        {
            ref ss s = ref _addr_s.val;

            s.SkipSpace();
            s.notEOF();
            if (!s.accept("%"))
            {
                s.errorString("missing literal %");
            }

        }

        // scanOne scans a single value, deriving the scanner from the type of the argument.
        private static void scanOne(this ptr<ss> _addr_s, int verb, object arg)
        {
            ref ss s = ref _addr_s.val;

            s.buf = s.buf[..0L];
            error err = default!; 
            // If the parameter has its own Scan method, use that.
            {
                Scanner v__prev1 = v;

                Scanner (v, ok) = Scanner.As(arg._<Scanner>())!;

                if (ok)
                {
                    err = error.As(v.Scan(s, verb))!;
                    if (err != null)
                    {
                        if (err == io.EOF)
                        {
                            err = error.As(io.ErrUnexpectedEOF)!;
                        }

                        s.error(err);

                    }

                    return ;

                }

                v = v__prev1;

            }


            switch (arg.type())
            {
                case ptr<bool> v:
                    v.val = s.scanBool(verb);
                    break;
                case ptr<complex64> v:
                    v.val = complex64(s.scanComplex(verb, 64L));
                    break;
                case ptr<System.Numerics.Complex128> v:
                    v.val = s.scanComplex(verb, 128L);
                    break;
                case ptr<long> v:
                    v.val = int(s.scanInt(verb, intBits));
                    break;
                case ptr<sbyte> v:
                    v.val = int8(s.scanInt(verb, 8L));
                    break;
                case ptr<short> v:
                    v.val = int16(s.scanInt(verb, 16L));
                    break;
                case ptr<int> v:
                    v.val = int32(s.scanInt(verb, 32L));
                    break;
                case ptr<long> v:
                    v.val = s.scanInt(verb, 64L);
                    break;
                case ptr<ulong> v:
                    v.val = uint(s.scanUint(verb, intBits));
                    break;
                case ptr<byte> v:
                    v.val = uint8(s.scanUint(verb, 8L));
                    break;
                case ptr<ushort> v:
                    v.val = uint16(s.scanUint(verb, 16L));
                    break;
                case ptr<uint> v:
                    v.val = uint32(s.scanUint(verb, 32L));
                    break;
                case ptr<ulong> v:
                    v.val = s.scanUint(verb, 64L);
                    break;
                case ptr<System.UIntPtr> v:
                    v.val = uintptr(s.scanUint(verb, uintptrBits)); 
                    // Floats are tricky because you want to scan in the precision of the result, not
                    // scan in high precision and convert, in order to preserve the correct error condition.
                    break;
                case ptr<float> v:
                    if (s.okVerb(verb, floatVerbs, "float32"))
                    {
                        s.SkipSpace();
                        s.notEOF();
                        v.val = float32(s.convertFloat(s.floatToken(), 32L));
                    }

                    break;
                case ptr<double> v:
                    if (s.okVerb(verb, floatVerbs, "float64"))
                    {
                        s.SkipSpace();
                        s.notEOF();
                        v.val = s.convertFloat(s.floatToken(), 64L);
                    }

                    break;
                case ptr<@string> v:
                    v.val = s.convertString(verb);
                    break;
                case ptr<slice<byte>> v:
                    v.val = (slice<byte>)s.convertString(verb);
                    break;
                default:
                {
                    var v = arg.type();
                    var val = reflect.ValueOf(v);
                    var ptr = val;
                    if (ptr.Kind() != reflect.Ptr)
                    {
                        s.errorString("type not a pointer: " + val.Type().String());
                        return ;
                    }

                    {
                        Scanner v__prev1 = v;

                        var v = ptr.Elem();


                        if (v.Kind() == reflect.Bool) 
                            v.SetBool(s.scanBool(verb));
                        else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                            v.SetInt(s.scanInt(verb, v.Type().Bits()));
                        else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                            v.SetUint(s.scanUint(verb, v.Type().Bits()));
                        else if (v.Kind() == reflect.String) 
                            v.SetString(s.convertString(verb));
                        else if (v.Kind() == reflect.Slice) 
                            // For now, can only handle (renamed) []byte.
                            var typ = v.Type();
                            if (typ.Elem().Kind() != reflect.Uint8)
                            {
                                s.errorString("can't scan type: " + val.Type().String());
                            }

                            var str = s.convertString(verb);
                            v.Set(reflect.MakeSlice(typ, len(str), len(str)));
                            for (long i = 0L; i < len(str); i++)
                            {
                                v.Index(i).SetUint(uint64(str[i]));
                            }
                        else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                            s.SkipSpace();
                            s.notEOF();
                            v.SetFloat(s.convertFloat(s.floatToken(), v.Type().Bits()));
                        else if (v.Kind() == reflect.Complex64 || v.Kind() == reflect.Complex128) 
                            v.SetComplex(s.scanComplex(verb, v.Type().Bits()));
                        else 
                            s.errorString("can't scan type: " + val.Type().String());


                        v = v__prev1;
                    }
                    break;
                }
            }

        }

        // errorHandler turns local panics into error returns.
        private static void errorHandler(ptr<error> _addr_errp) => func((_, panic, __) =>
        {
            ref error errp = ref _addr_errp.val;

            {
                var e = recover();

                if (e != null)
                {
                    {
                        scanError (se, ok) = e._<scanError>();

                        if (ok)
                        { // catch local error
                            errp = error.As(se.err)!;

                        }                        {
                            error (eof, ok) = error.As(e._<error>())!;


                            else if (ok && eof == io.EOF)
                            { // out of input
                                errp = error.As(eof)!;

                            }
                            else
                            {
                                panic(e);
                            }

                        }


                    }

                }

            }

        });

        // doScan does the real work for scanning without a format string.
        private static (long, error) doScan(this ptr<ss> _addr_s, slice<object> a) => func((defer, _, __) =>
        {
            long numProcessed = default;
            error err = default!;
            ref ss s = ref _addr_s.val;

            defer(errorHandler(_addr_err));
            foreach (var (_, arg) in a)
            {
                s.scanOne('v', arg);
                numProcessed++;
            } 
            // Check for newline (or EOF) if required (Scanln etc.).
            if (s.nlIsEnd)
            {
                while (true)
                {
                    var r = s.getRune();
                    if (r == '\n' || r == eof)
                    {
                        break;
                    }

                    if (!isSpace(r))
                    {
                        s.errorString("expected newline");
                        break;
                    }

                }


            }

            return ;

        });

        // advance determines whether the next characters in the input match
        // those of the format. It returns the number of bytes (sic) consumed
        // in the format. All runs of space characters in either input or
        // format behave as a single space. Newlines are special, though:
        // newlines in the format must match those in the input and vice versa.
        // This routine also handles the %% case. If the return value is zero,
        // either format starts with a % (with no following %) or the input
        // is empty. If it is negative, the input did not match the string.
        private static long advance(this ptr<ss> _addr_s, @string format)
        {
            long i = default;
            ref ss s = ref _addr_s.val;

            while (i < len(format))
            {
                var (fmtc, w) = utf8.DecodeRuneInString(format[i..]); 

                // Space processing.
                // In the rest of this comment "space" means spaces other than newline.
                // Newline in the format matches input of zero or more spaces and then newline or end-of-input.
                // Spaces in the format before the newline are collapsed into the newline.
                // Spaces in the format after the newline match zero or more spaces after the corresponding input newline.
                // Other spaces in the format match input of one or more spaces or end-of-input.
                if (isSpace(fmtc))
                {
                    long newlines = 0L;
                    var trailingSpace = false;
                    while (isSpace(fmtc) && i < len(format))
                    {
                        if (fmtc == '\n')
                        {
                            newlines++;
                            trailingSpace = false;
                        }
                        else
                        {
                            trailingSpace = true;
                        }

                        i += w;
                        fmtc, w = utf8.DecodeRuneInString(format[i..]);

                    }

                    for (long j = 0L; j < newlines; j++)
                    {
                        var inputc = s.getRune();
                        while (isSpace(inputc) && inputc != '\n')
                        {
                            inputc = s.getRune();
                        }

                        if (inputc != '\n' && inputc != eof)
                        {
                            s.errorString("newline in format does not match input");
                        }

                    }

                    if (trailingSpace)
                    {
                        inputc = s.getRune();
                        if (newlines == 0L)
                        { 
                            // If the trailing space stood alone (did not follow a newline),
                            // it must find at least one space to consume.
                            if (!isSpace(inputc) && inputc != eof)
                            {
                                s.errorString("expected space in input to match format");
                            }

                            if (inputc == '\n')
                            {
                                s.errorString("newline in input does not match format");
                            }

                        }

                        while (isSpace(inputc) && inputc != '\n')
                        {
                            inputc = s.getRune();
                        }

                        if (inputc != eof)
                        {
                            s.UnreadRune();
                        }

                    }

                    continue;

                } 

                // Verbs.
                if (fmtc == '%')
                { 
                    // % at end of string is an error.
                    if (i + w == len(format))
                    {
                        s.errorString("missing verb: % at end of format string");
                    } 
                    // %% acts like a real percent
                    var (nextc, _) = utf8.DecodeRuneInString(format[i + w..]); // will not match % if string is empty
                    if (nextc != '%')
                    {
                        return ;
                    }

                    i += w; // skip the first %
                } 

                // Literals.
                inputc = s.mustReadRune();
                if (fmtc != inputc)
                {
                    s.UnreadRune();
                    return -1L;
                }

                i += w;

            }

            return ;

        }

        // doScanf does the real work when scanning with a format string.
        // At the moment, it handles only pointers to basic types.
        private static (long, error) doScanf(this ptr<ss> _addr_s, @string format, slice<object> a) => func((defer, _, __) =>
        {
            long numProcessed = default;
            error err = default!;
            ref ss s = ref _addr_s.val;

            defer(errorHandler(_addr_err));
            var end = len(format) - 1L; 
            // We process one item per non-trivial format
            {
                long i = 0L;

                while (i <= end)
                {
                    var w = s.advance(format[i..]);
                    if (w > 0L)
                    {
                        i += w;
                        continue;
                    } 
                    // Either we failed to advance, we have a percent character, or we ran out of input.
                    if (format[i] != '%')
                    { 
                        // Can't advance format. Why not?
                        if (w < 0L)
                        {
                            s.errorString("input does not match format");
                        } 
                        // Otherwise at EOF; "too many operands" error handled below
                        break;

                    }

                    i++; // % is one byte

                    // do we have 20 (width)?
                    bool widPresent = default;
                    s.maxWid, widPresent, i = parsenum(format, i, end);
                    if (!widPresent)
                    {
                        s.maxWid = hugeWid;
                    }

                    var (c, w) = utf8.DecodeRuneInString(format[i..]);
                    i += w;

                    if (c != 'c')
                    {
                        s.SkipSpace();
                    }

                    if (c == '%')
                    {
                        s.scanPercent();
                        continue; // Do not consume an argument.
                    }

                    s.argLimit = s.limit;
                    {
                        var f = s.count + s.maxWid;

                        if (f < s.argLimit)
                        {
                            s.argLimit = f;
                        }

                    }


                    if (numProcessed >= len(a))
                    { // out of operands
                        s.errorString("too few operands for format '%" + format[i - w..] + "'");
                        break;

                    }

                    var arg = a[numProcessed];

                    s.scanOne(c, arg);
                    numProcessed++;
                    s.argLimit = s.limit;

                }

            }
            if (numProcessed < len(a))
            {
                s.errorString("too many operands");
            }

            return ;

        });
    }
}
