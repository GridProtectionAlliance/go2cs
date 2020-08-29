// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bufio -- go2cs converted at 2020 August 29 08:22:57 UTC
// import "bufio" ==> using bufio = go.bufio_package
// Original source: C:\Go\src\bufio\scan.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class bufio_package
    {
        // Scanner provides a convenient interface for reading data such as
        // a file of newline-delimited lines of text. Successive calls to
        // the Scan method will step through the 'tokens' of a file, skipping
        // the bytes between the tokens. The specification of a token is
        // defined by a split function of type SplitFunc; the default split
        // function breaks the input into lines with line termination stripped. Split
        // functions are defined in this package for scanning a file into
        // lines, bytes, UTF-8-encoded runes, and space-delimited words. The
        // client may instead provide a custom split function.
        //
        // Scanning stops unrecoverably at EOF, the first I/O error, or a token too
        // large to fit in the buffer. When a scan stops, the reader may have
        // advanced arbitrarily far past the last token. Programs that need more
        // control over error handling or large tokens, or must run sequential scans
        // on a reader, should use bufio.Reader instead.
        //
        public partial struct Scanner
        {
            public io.Reader r; // The reader provided by the client.
            public SplitFunc split; // The function to split the tokens.
            public long maxTokenSize; // Maximum size of a token; modified by tests.
            public slice<byte> token; // Last token returned by split.
            public slice<byte> buf; // Buffer used as argument to split.
            public long start; // First non-processed byte in buf.
            public long end; // End of data in buf.
            public error err; // Sticky error.
            public long empties; // Count of successive empty tokens.
            public bool scanCalled; // Scan has been called; buffer is in use.
            public bool done; // Scan has finished.
        }

        // SplitFunc is the signature of the split function used to tokenize the
        // input. The arguments are an initial substring of the remaining unprocessed
        // data and a flag, atEOF, that reports whether the Reader has no more data
        // to give. The return values are the number of bytes to advance the input
        // and the next token to return to the user, plus an error, if any. If the
        // data does not yet hold a complete token, for instance if it has no newline
        // while scanning lines, SplitFunc can return (0, nil, nil) to signal the
        // Scanner to read more data into the slice and try again with a longer slice
        // starting at the same point in the input.
        //
        // If the returned error is non-nil, scanning stops and the error
        // is returned to the client.
        //
        // The function is never called with an empty data slice unless atEOF
        // is true. If atEOF is true, however, data may be non-empty and,
        // as always, holds unprocessed text.
        public delegate  error) SplitFunc(slice<byte>,  bool,  (long,  slice<byte>);

        // Errors returned by Scanner.
        public static var ErrTooLong = errors.New("bufio.Scanner: token too long");        public static var ErrNegativeAdvance = errors.New("bufio.Scanner: SplitFunc returns negative advance count");        public static var ErrAdvanceTooFar = errors.New("bufio.Scanner: SplitFunc returns advance count beyond input");

 
        // MaxScanTokenSize is the maximum size used to buffer a token
        // unless the user provides an explicit buffer with Scan.Buffer.
        // The actual maximum token size may be smaller as the buffer
        // may need to include, for instance, a newline.
        public static readonly long MaxScanTokenSize = 64L * 1024L;

        private static readonly long startBufSize = 4096L; // Size of initial allocation for buffer.

        // NewScanner returns a new Scanner to read from r.
        // The split function defaults to ScanLines.
        public static ref Scanner NewScanner(io.Reader r)
        {
            return ref new Scanner(r:r,split:ScanLines,maxTokenSize:MaxScanTokenSize,);
        }

        // Err returns the first non-EOF error that was encountered by the Scanner.
        private static error Err(this ref Scanner s)
        {
            if (s.err == io.EOF)
            {
                return error.As(null);
            }
            return error.As(s.err);
        }

        // Bytes returns the most recent token generated by a call to Scan.
        // The underlying array may point to data that will be overwritten
        // by a subsequent call to Scan. It does no allocation.
        private static slice<byte> Bytes(this ref Scanner s)
        {
            return s.token;
        }

        // Text returns the most recent token generated by a call to Scan
        // as a newly allocated string holding its bytes.
        private static @string Text(this ref Scanner s)
        {
            return string(s.token);
        }

        // ErrFinalToken is a special sentinel error value. It is intended to be
        // returned by a Split function to indicate that the token being delivered
        // with the error is the last token and scanning should stop after this one.
        // After ErrFinalToken is received by Scan, scanning stops with no error.
        // The value is useful to stop processing early or when it is necessary to
        // deliver a final empty token. One could achieve the same behavior
        // with a custom error value but providing one here is tidier.
        // See the emptyFinalToken example for a use of this value.
        public static var ErrFinalToken = errors.New("final token");

        // Scan advances the Scanner to the next token, which will then be
        // available through the Bytes or Text method. It returns false when the
        // scan stops, either by reaching the end of the input or an error.
        // After Scan returns false, the Err method will return any error that
        // occurred during scanning, except that if it was io.EOF, Err
        // will return nil.
        // Scan panics if the split function returns too many empty
        // tokens without advancing the input. This is a common error mode for
        // scanners.
        private static bool Scan(this ref Scanner _s) => func(_s, (ref Scanner s, Defer _, Panic panic, Recover __) =>
        {
            if (s.done)
            {
                return false;
            }
            s.scanCalled = true; 
            // Loop until we have a token.
            while (true)
            { 
                // See if we can get a token with what we already have.
                // If we've run out of data but have an error, give the split function
                // a chance to recover any remaining, possibly empty token.
                if (s.end > s.start || s.err != null)
                {
                    var (advance, token, err) = s.split(s.buf[s.start..s.end], s.err != null);
                    if (err != null)
                    {
                        if (err == ErrFinalToken)
                        {
                            s.token = token;
                            s.done = true;
                            return true;
                        }
                        s.setErr(err);
                        return false;
                    }
                    if (!s.advance(advance))
                    {
                        return false;
                    }
                    s.token = token;
                    if (token != null)
                    {
                        if (s.err == null || advance > 0L)
                        {
                            s.empties = 0L;
                        }
                        else
                        { 
                            // Returning tokens not advancing input at EOF.
                            s.empties++;
                            if (s.empties > maxConsecutiveEmptyReads)
                            {
                                panic("bufio.Scan: too many empty tokens without progressing");
                            }
                        }
                        return true;
                    }
                } 
                // We cannot generate a token with what we are holding.
                // If we've already hit EOF or an I/O error, we are done.
                if (s.err != null)
                { 
                    // Shut it down.
                    s.start = 0L;
                    s.end = 0L;
                    return false;
                } 
                // Must read more data.
                // First, shift data to beginning of buffer if there's lots of empty space
                // or space is needed.
                if (s.start > 0L && (s.end == len(s.buf) || s.start > len(s.buf) / 2L))
                {
                    copy(s.buf, s.buf[s.start..s.end]);
                    s.end -= s.start;
                    s.start = 0L;
                } 
                // Is the buffer full? If so, resize.
                if (s.end == len(s.buf))
                { 
                    // Guarantee no overflow in the multiplication below.
                    const var maxInt = int(~uint(0L) >> (int)(1L));

                    if (len(s.buf) >= s.maxTokenSize || len(s.buf) > maxInt / 2L)
                    {
                        s.setErr(ErrTooLong);
                        return false;
                    }
                    var newSize = len(s.buf) * 2L;
                    if (newSize == 0L)
                    {
                        newSize = startBufSize;
                    }
                    if (newSize > s.maxTokenSize)
                    {
                        newSize = s.maxTokenSize;
                    }
                    var newBuf = make_slice<byte>(newSize);
                    copy(newBuf, s.buf[s.start..s.end]);
                    s.buf = newBuf;
                    s.end -= s.start;
                    s.start = 0L;
                } 
                // Finally we can read some input. Make sure we don't get stuck with
                // a misbehaving Reader. Officially we don't need to do this, but let's
                // be extra careful: Scanner is for safe, simple jobs.
                {
                    long loop = 0L;

                    while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                    {
                        var (n, err) = s.r.Read(s.buf[s.end..len(s.buf)]);
                        s.end += n;
                        if (err != null)
                        {
                            s.setErr(err);
                            break;
                        }
                        if (n > 0L)
                        {
                            s.empties = 0L;
                            break;
                        }
                        loop++;
                        if (loop > maxConsecutiveEmptyReads)
                        {
                            s.setErr(io.ErrNoProgress);
                            break;
                        }
                    }

                }
            }

        });

        // advance consumes n bytes of the buffer. It reports whether the advance was legal.
        private static bool advance(this ref Scanner s, long n)
        {
            if (n < 0L)
            {
                s.setErr(ErrNegativeAdvance);
                return false;
            }
            if (n > s.end - s.start)
            {
                s.setErr(ErrAdvanceTooFar);
                return false;
            }
            s.start += n;
            return true;
        }

        // setErr records the first error encountered.
        private static void setErr(this ref Scanner s, error err)
        {
            if (s.err == null || s.err == io.EOF)
            {
                s.err = err;
            }
        }

        // Buffer sets the initial buffer to use when scanning and the maximum
        // size of buffer that may be allocated during scanning. The maximum
        // token size is the larger of max and cap(buf). If max <= cap(buf),
        // Scan will use this buffer only and do no allocation.
        //
        // By default, Scan uses an internal buffer and sets the
        // maximum token size to MaxScanTokenSize.
        //
        // Buffer panics if it is called after scanning has started.
        private static void Buffer(this ref Scanner _s, slice<byte> buf, long max) => func(_s, (ref Scanner s, Defer _, Panic panic, Recover __) =>
        {
            if (s.scanCalled)
            {
                panic("Buffer called after Scan");
            }
            s.buf = buf[0L..cap(buf)];
            s.maxTokenSize = max;
        });

        // Split sets the split function for the Scanner.
        // The default split function is ScanLines.
        //
        // Split panics if it is called after scanning has started.
        private static void Split(this ref Scanner _s, SplitFunc split) => func(_s, (ref Scanner s, Defer _, Panic panic, Recover __) =>
        {
            if (s.scanCalled)
            {
                panic("Split called after Scan");
            }
            s.split = split;
        });

        // Split functions

        // ScanBytes is a split function for a Scanner that returns each byte as a token.
        public static (long, slice<byte>, error) ScanBytes(slice<byte> data, bool atEOF)
        {
            if (atEOF && len(data) == 0L)
            {
                return (0L, null, null);
            }
            return (1L, data[0L..1L], null);
        }

        private static slice<byte> errorRune = (slice<byte>)string(utf8.RuneError);

        // ScanRunes is a split function for a Scanner that returns each
        // UTF-8-encoded rune as a token. The sequence of runes returned is
        // equivalent to that from a range loop over the input as a string, which
        // means that erroneous UTF-8 encodings translate to U+FFFD = "\xef\xbf\xbd".
        // Because of the Scan interface, this makes it impossible for the client to
        // distinguish correctly encoded replacement runes from encoding errors.
        public static (long, slice<byte>, error) ScanRunes(slice<byte> data, bool atEOF)
        {
            if (atEOF && len(data) == 0L)
            {
                return (0L, null, null);
            } 

            // Fast path 1: ASCII.
            if (data[0L] < utf8.RuneSelf)
            {
                return (1L, data[0L..1L], null);
            } 

            // Fast path 2: Correct UTF-8 decode without error.
            var (_, width) = utf8.DecodeRune(data);
            if (width > 1L)
            { 
                // It's a valid encoding. Width cannot be one for a correctly encoded
                // non-ASCII rune.
                return (width, data[0L..width], null);
            } 

            // We know it's an error: we have width==1 and implicitly r==utf8.RuneError.
            // Is the error because there wasn't a full rune to be decoded?
            // FullRune distinguishes correctly between erroneous and incomplete encodings.
            if (!atEOF && !utf8.FullRune(data))
            { 
                // Incomplete; get more bytes.
                return (0L, null, null);
            } 

            // We have a real UTF-8 encoding error. Return a properly encoded error rune
            // but advance only one byte. This matches the behavior of a range loop over
            // an incorrectly encoded string.
            return (1L, errorRune, null);
        }

        // dropCR drops a terminal \r from the data.
        private static slice<byte> dropCR(slice<byte> data)
        {
            if (len(data) > 0L && data[len(data) - 1L] == '\r')
            {
                return data[0L..len(data) - 1L];
            }
            return data;
        }

        // ScanLines is a split function for a Scanner that returns each line of
        // text, stripped of any trailing end-of-line marker. The returned line may
        // be empty. The end-of-line marker is one optional carriage return followed
        // by one mandatory newline. In regular expression notation, it is `\r?\n`.
        // The last non-empty line of input will be returned even if it has no
        // newline.
        public static (long, slice<byte>, error) ScanLines(slice<byte> data, bool atEOF)
        {
            if (atEOF && len(data) == 0L)
            {
                return (0L, null, null);
            }
            {
                var i = bytes.IndexByte(data, '\n');

                if (i >= 0L)
                { 
                    // We have a full newline-terminated line.
                    return (i + 1L, dropCR(data[0L..i]), null);
                } 
                // If we're at EOF, we have a final, non-terminated line. Return it.

            } 
            // If we're at EOF, we have a final, non-terminated line. Return it.
            if (atEOF)
            {
                return (len(data), dropCR(data), null);
            } 
            // Request more data.
            return (0L, null, null);
        }

        // isSpace reports whether the character is a Unicode white space character.
        // We avoid dependency on the unicode package, but check validity of the implementation
        // in the tests.
        private static bool isSpace(int r)
        {
            if (r <= '\u00FF')
            { 
                // Obvious ASCII ones: \t through \r plus space. Plus two Latin-1 oddballs.
                switch (r)
                {
                    case ' ': 

                    case '\t': 

                    case '\n': 

                    case '\v': 

                    case '\f': 

                    case '\r': 
                        return true;
                        break;
                    case '\u0085': 

                    case '\u00A0': 
                        return true;
                        break;
                }
                return false;
            } 
            // High-valued ones.
            if ('\u2000' <= r && r <= '\u200a')
            {
                return true;
            }
            switch (r)
            {
                case '\u1680': 

                case '\u2028': 

                case '\u2029': 

                case '\u202f': 

                case '\u205f': 

                case '\u3000': 
                    return true;
                    break;
            }
            return false;
        }

        // ScanWords is a split function for a Scanner that returns each
        // space-separated word of text, with surrounding spaces deleted. It will
        // never return an empty string. The definition of space is set by
        // unicode.IsSpace.
        public static (long, slice<byte>, error) ScanWords(slice<byte> data, bool atEOF)
        { 
            // Skip leading spaces.
            long start = 0L;
            {
                long width__prev1 = width;

                long width = 0L;

                while (start < len(data))
                {
                    int r = default;
                    r, width = utf8.DecodeRune(data[start..]);
                    if (!isSpace(r))
                    {
                        break;
                    start += width;
                    }
                } 
                // Scan until space, marking end of word.


                width = width__prev1;
            } 
            // Scan until space, marking end of word.
            {
                long width__prev1 = width;

                width = 0L;
                var i = start;

                while (i < len(data))
                {
                    r = default;
                    r, width = utf8.DecodeRune(data[i..]);
                    if (isSpace(r))
                    {
                        return (i + width, data[start..i], null);
                    i += width;
                    }
                } 
                // If we're at EOF, we have a final, non-empty, non-terminated word. Return it.


                width = width__prev1;
            } 
            // If we're at EOF, we have a final, non-empty, non-terminated word. Return it.
            if (atEOF && len(data) > start)
            {
                return (len(data), data[start..], null);
            } 
            // Request more data.
            return (start, null, null);
        }
    }
}
