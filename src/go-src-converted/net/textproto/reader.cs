// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package textproto -- go2cs converted at 2020 August 29 08:32:30 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Go\src\net\textproto\reader.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class textproto_package
    {
        // A Reader implements convenience methods for reading requests
        // or responses from a text protocol network connection.
        public partial struct Reader
        {
            public ptr<bufio.Reader> R;
            public ptr<dotReader> dot;
            public slice<byte> buf; // a re-usable buffer for readContinuedLineSlice
        }

        // NewReader returns a new Reader reading from r.
        //
        // To avoid denial of service attacks, the provided bufio.Reader
        // should be reading from an io.LimitReader or similar Reader to bound
        // the size of responses.
        public static ref Reader NewReader(ref bufio.Reader r)
        {
            return ref new Reader(R:r);
        }

        // ReadLine reads a single line from r,
        // eliding the final \n or \r\n from the returned string.
        private static (@string, error) ReadLine(this ref Reader r)
        {
            var (line, err) = r.readLineSlice();
            return (string(line), err);
        }

        // ReadLineBytes is like ReadLine but returns a []byte instead of a string.
        private static (slice<byte>, error) ReadLineBytes(this ref Reader r)
        {
            var (line, err) = r.readLineSlice();
            if (line != null)
            {
                var buf = make_slice<byte>(len(line));
                copy(buf, line);
                line = buf;
            }
            return (line, err);
        }

        private static (slice<byte>, error) readLineSlice(this ref Reader r)
        {
            r.closeDot();
            slice<byte> line = default;
            while (true)
            {
                var (l, more, err) = r.R.ReadLine();
                if (err != null)
                {
                    return (null, err);
                } 
                // Avoid the copy if the first call produced a full line.
                if (line == null && !more)
                {
                    return (l, null);
                }
                line = append(line, l);
                if (!more)
                {
                    break;
                }
            }

            return (line, null);
        }

        // ReadContinuedLine reads a possibly continued line from r,
        // eliding the final trailing ASCII white space.
        // Lines after the first are considered continuations if they
        // begin with a space or tab character. In the returned data,
        // continuation lines are separated from the previous line
        // only by a single space: the newline and leading white space
        // are removed.
        //
        // For example, consider this input:
        //
        //    Line 1
        //      continued...
        //    Line 2
        //
        // The first call to ReadContinuedLine will return "Line 1 continued..."
        // and the second will return "Line 2".
        //
        // A line consisting of only white space is never continued.
        //
        private static (@string, error) ReadContinuedLine(this ref Reader r)
        {
            var (line, err) = r.readContinuedLineSlice();
            return (string(line), err);
        }

        // trim returns s with leading and trailing spaces and tabs removed.
        // It does not assume Unicode or UTF-8.
        private static slice<byte> trim(slice<byte> s)
        {
            long i = 0L;
            while (i < len(s) && (s[i] == ' ' || s[i] == '\t'))
            {
                i++;
            }

            var n = len(s);
            while (n > i && (s[n - 1L] == ' ' || s[n - 1L] == '\t'))
            {
                n--;
            }

            return s[i..n];
        }

        // ReadContinuedLineBytes is like ReadContinuedLine but
        // returns a []byte instead of a string.
        private static (slice<byte>, error) ReadContinuedLineBytes(this ref Reader r)
        {
            var (line, err) = r.readContinuedLineSlice();
            if (line != null)
            {
                var buf = make_slice<byte>(len(line));
                copy(buf, line);
                line = buf;
            }
            return (line, err);
        }

        private static (slice<byte>, error) readContinuedLineSlice(this ref Reader r)
        { 
            // Read the first line.
            var (line, err) = r.readLineSlice();
            if (err != null)
            {
                return (null, err);
            }
            if (len(line) == 0L)
            { // blank line - no continuation
                return (line, null);
            } 

            // Optimistically assume that we have started to buffer the next line
            // and it starts with an ASCII letter (the next header key), so we can
            // avoid copying that buffered data around in memory and skipping over
            // non-existent whitespace.
            if (r.R.Buffered() > 1L)
            {
                var (peek, err) = r.R.Peek(1L);
                if (err == null && isASCIILetter(peek[0L]))
                {
                    return (trim(line), null);
                }
            } 

            // ReadByte or the next readLineSlice will flush the read buffer;
            // copy the slice into buf.
            r.buf = append(r.buf[..0L], trim(line)); 

            // Read continuation lines.
            while (r.skipSpace() > 0L)
            {
                (line, err) = r.readLineSlice();
                if (err != null)
                {
                    break;
                }
                r.buf = append(r.buf, ' ');
                r.buf = append(r.buf, trim(line));
            }

            return (r.buf, null);
        }

        // skipSpace skips R over all spaces and returns the number of bytes skipped.
        private static long skipSpace(this ref Reader r)
        {
            long n = 0L;
            while (true)
            {
                var (c, err) = r.R.ReadByte();
                if (err != null)
                { 
                    // Bufio will keep err until next read.
                    break;
                }
                if (c != ' ' && c != '\t')
                {
                    r.R.UnreadByte();
                    break;
                }
                n++;
            }

            return n;
        }

        private static (long, bool, @string, error) readCodeLine(this ref Reader r, long expectCode)
        {
            var (line, err) = r.ReadLine();
            if (err != null)
            {
                return;
            }
            return parseCodeLine(line, expectCode);
        }

        private static (long, bool, @string, error) parseCodeLine(@string line, long expectCode)
        {
            if (len(line) < 4L || line[3L] != ' ' && line[3L] != '-')
            {
                err = ProtocolError("short response: " + line);
                return;
            }
            continued = line[3L] == '-';
            code, err = strconv.Atoi(line[0L..3L]);
            if (err != null || code < 100L)
            {
                err = ProtocolError("invalid response code: " + line);
                return;
            }
            message = line[4L..];
            if (1L <= expectCode && expectCode < 10L && code / 100L != expectCode || 10L <= expectCode && expectCode < 100L && code / 10L != expectCode || 100L <= expectCode && expectCode < 1000L && code != expectCode)
            {
                err = ref new Error(code,message);
            }
            return;
        }

        // ReadCodeLine reads a response code line of the form
        //    code message
        // where code is a three-digit status code and the message
        // extends to the rest of the line. An example of such a line is:
        //    220 plan9.bell-labs.com ESMTP
        //
        // If the prefix of the status does not match the digits in expectCode,
        // ReadCodeLine returns with err set to &Error{code, message}.
        // For example, if expectCode is 31, an error will be returned if
        // the status is not in the range [310,319].
        //
        // If the response is multi-line, ReadCodeLine returns an error.
        //
        // An expectCode <= 0 disables the check of the status code.
        //
        private static (long, @string, error) ReadCodeLine(this ref Reader r, long expectCode)
        {
            var (code, continued, message, err) = r.readCodeLine(expectCode);
            if (err == null && continued)
            {
                err = ProtocolError("unexpected multi-line response: " + message);
            }
            return;
        }

        // ReadResponse reads a multi-line response of the form:
        //
        //    code-message line 1
        //    code-message line 2
        //    ...
        //    code message line n
        //
        // where code is a three-digit status code. The first line starts with the
        // code and a hyphen. The response is terminated by a line that starts
        // with the same code followed by a space. Each line in message is
        // separated by a newline (\n).
        //
        // See page 36 of RFC 959 (http://www.ietf.org/rfc/rfc959.txt) for
        // details of another form of response accepted:
        //
        //  code-message line 1
        //  message line 2
        //  ...
        //  code message line n
        //
        // If the prefix of the status does not match the digits in expectCode,
        // ReadResponse returns with err set to &Error{code, message}.
        // For example, if expectCode is 31, an error will be returned if
        // the status is not in the range [310,319].
        //
        // An expectCode <= 0 disables the check of the status code.
        //
        private static (long, @string, error) ReadResponse(this ref Reader r, long expectCode)
        {
            var (code, continued, message, err) = r.readCodeLine(expectCode);
            var multi = continued;
            while (continued)
            {
                var (line, err) = r.ReadLine();
                if (err != null)
                {
                    return (0L, "", err);
                }
                long code2 = default;
                @string moreMessage = default;
                code2, continued, moreMessage, err = parseCodeLine(line, 0L);
                if (err != null || code2 != code)
                {
                    message += "\n" + strings.TrimRight(line, "\r\n");
                    continued = true;
                    continue;
                }
                message += "\n" + moreMessage;
            }

            if (err != null && multi && message != "")
            { 
                // replace one line error message with all lines (full message)
                err = ref new Error(code,message);
            }
            return;
        }

        // DotReader returns a new Reader that satisfies Reads using the
        // decoded text of a dot-encoded block read from r.
        // The returned Reader is only valid until the next call
        // to a method on r.
        //
        // Dot encoding is a common framing used for data blocks
        // in text protocols such as SMTP.  The data consists of a sequence
        // of lines, each of which ends in "\r\n".  The sequence itself
        // ends at a line containing just a dot: ".\r\n".  Lines beginning
        // with a dot are escaped with an additional dot to avoid
        // looking like the end of the sequence.
        //
        // The decoded form returned by the Reader's Read method
        // rewrites the "\r\n" line endings into the simpler "\n",
        // removes leading dot escapes if present, and stops with error io.EOF
        // after consuming (and discarding) the end-of-sequence line.
        private static io.Reader DotReader(this ref Reader r)
        {
            r.closeDot();
            r.dot = ref new dotReader(r:r);
            return r.dot;
        }

        private partial struct dotReader
        {
            public ptr<Reader> r;
            public long state;
        }

        // Read satisfies reads by decoding dot-encoded data read from d.r.
        private static (long, error) Read(this ref dotReader d, slice<byte> b)
        { 
            // Run data through a simple state machine to
            // elide leading dots, rewrite trailing \r\n into \n,
            // and detect ending .\r\n line.
            const var stateBeginLine = iota; // beginning of line; initial state; must be zero
            const var stateDot = 0; // read . at beginning of line
            const var stateDotCR = 1; // read .\r at beginning of line
            const var stateCR = 2; // read \r (possibly at end of line)
            const var stateData = 3; // reading data in middle of line
            const var stateEOF = 4; // reached .\r\n end marker line
            var br = d.r.R;
            while (n < len(b) && d.state != stateEOF)
            {
                byte c = default;
                c, err = br.ReadByte();
                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = io.ErrUnexpectedEOF;
                    }
                    break;
                }

                if (d.state == stateBeginLine) 
                    if (c == '.')
                    {
                        d.state = stateDot;
                        continue;
                    }
                    if (c == '\r')
                    {
                        d.state = stateCR;
                        continue;
                    }
                    d.state = stateData;
                else if (d.state == stateDot) 
                    if (c == '\r')
                    {
                        d.state = stateDotCR;
                        continue;
                    }
                    if (c == '\n')
                    {
                        d.state = stateEOF;
                        continue;
                    }
                    d.state = stateData;
                else if (d.state == stateDotCR) 
                    if (c == '\n')
                    {
                        d.state = stateEOF;
                        continue;
                    } 
                    // Not part of .\r\n.
                    // Consume leading dot and emit saved \r.
                    br.UnreadByte();
                    c = '\r';
                    d.state = stateData;
                else if (d.state == stateCR) 
                    if (c == '\n')
                    {
                        d.state = stateBeginLine;
                        break;
                    } 
                    // Not part of \r\n. Emit saved \r
                    br.UnreadByte();
                    c = '\r';
                    d.state = stateData;
                else if (d.state == stateData) 
                    if (c == '\r')
                    {
                        d.state = stateCR;
                        continue;
                    }
                    if (c == '\n')
                    {
                        d.state = stateBeginLine;
                    }
                                b[n] = c;
                n++;
            }

            if (err == null && d.state == stateEOF)
            {
                err = io.EOF;
            }
            if (err != null && d.r.dot == d)
            {
                d.r.dot = null;
            }
            return;
        }

        // closeDot drains the current DotReader if any,
        // making sure that it reads until the ending dot line.
        private static void closeDot(this ref Reader r)
        {
            if (r.dot == null)
            {
                return;
            }
            var buf = make_slice<byte>(128L);
            while (r.dot != null)
            { 
                // When Read reaches EOF or an error,
                // it will set r.dot == nil.
                r.dot.Read(buf);
            }

        }

        // ReadDotBytes reads a dot-encoding and returns the decoded data.
        //
        // See the documentation for the DotReader method for details about dot-encoding.
        private static (slice<byte>, error) ReadDotBytes(this ref Reader r)
        {
            return ioutil.ReadAll(r.DotReader());
        }

        // ReadDotLines reads a dot-encoding and returns a slice
        // containing the decoded lines, with the final \r\n or \n elided from each.
        //
        // See the documentation for the DotReader method for details about dot-encoding.
        private static (slice<@string>, error) ReadDotLines(this ref Reader r)
        { 
            // We could use ReadDotBytes and then Split it,
            // but reading a line at a time avoids needing a
            // large contiguous block of memory and is simpler.
            slice<@string> v = default;
            error err = default;
            while (true)
            {
                @string line = default;
                line, err = r.ReadLine();
                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        err = error.As(io.ErrUnexpectedEOF);
                    }
                    break;
                } 

                // Dot by itself marks end; otherwise cut one dot.
                if (len(line) > 0L && line[0L] == '.')
                {
                    if (len(line) == 1L)
                    {
                        break;
                    }
                    line = line[1L..];
                }
                v = append(v, line);
            }

            return (v, err);
        }

        // ReadMIMEHeader reads a MIME-style header from r.
        // The header is a sequence of possibly continued Key: Value lines
        // ending in a blank line.
        // The returned map m maps CanonicalMIMEHeaderKey(key) to a
        // sequence of values in the same order encountered in the input.
        //
        // For example, consider this input:
        //
        //    My-Key: Value 1
        //    Long-Key: Even
        //           Longer Value
        //    My-Key: Value 2
        //
        // Given that input, ReadMIMEHeader returns the map:
        //
        //    map[string][]string{
        //        "My-Key": {"Value 1", "Value 2"},
        //        "Long-Key": {"Even Longer Value"},
        //    }
        //
        private static (MIMEHeader, error) ReadMIMEHeader(this ref Reader r)
        { 
            // Avoid lots of small slice allocations later by allocating one
            // large one ahead of time which we'll cut up into smaller
            // slices. If this isn't big enough later, we allocate small ones.
            slice<@string> strs = default;
            var hint = r.upcomingHeaderNewlines();
            if (hint > 0L)
            {
                strs = make_slice<@string>(hint);
            }
            var m = make(MIMEHeader, hint); 

            // The first line cannot start with a leading space.
            {
                var (buf, err) = r.R.Peek(1L);

                if (err == null && (buf[0L] == ' ' || buf[0L] == '\t'))
                {
                    var (line, err) = r.readLineSlice();
                    if (err != null)
                    {
                        return (m, err);
                    }
                    return (m, ProtocolError("malformed MIME header initial line: " + string(line)));
                }

            }

            while (true)
            {
                var (kv, err) = r.readContinuedLineSlice();
                if (len(kv) == 0L)
                {
                    return (m, err);
                } 

                // Key ends at first colon; should not have trailing spaces
                // but they appear in the wild, violating specs, so we remove
                // them if present.
                var i = bytes.IndexByte(kv, ':');
                if (i < 0L)
                {
                    return (m, ProtocolError("malformed MIME header line: " + string(kv)));
                }
                var endKey = i;
                while (endKey > 0L && kv[endKey - 1L] == ' ')
                {
                    endKey--;
                }

                var key = canonicalMIMEHeaderKey(kv[..endKey]); 

                // As per RFC 7230 field-name is a token, tokens consist of one or more chars.
                // We could return a ProtocolError here, but better to be liberal in what we
                // accept, so if we get an empty key, skip it.
                if (key == "")
                {
                    continue;
                } 

                // Skip initial spaces in value.
                i++; // skip colon
                while (i < len(kv) && (kv[i] == ' ' || kv[i] == '\t'))
                {
                    i++;
                }

                var value = string(kv[i..]);

                var vv = m[key];
                if (vv == null && len(strs) > 0L)
                { 
                    // More than likely this will be a single-element key.
                    // Most headers aren't multi-valued.
                    // Set the capacity on strs[0] to 1, so any future append
                    // won't extend the slice into the other strings.
                    vv = strs.slice(-1, 1L, 1L);
                    strs = strs[1L..];
                    vv[0L] = value;
                    m[key] = vv;
                }
                else
                {
                    m[key] = append(vv, value);
                }
                if (err != null)
                {
                    return (m, err);
                }
            }

        }

        // upcomingHeaderNewlines returns an approximation of the number of newlines
        // that will be in this header. If it gets confused, it returns 0.
        private static long upcomingHeaderNewlines(this ref Reader r)
        { 
            // Try to determine the 'hint' size.
            r.R.Peek(1L); // force a buffer load if empty
            var s = r.R.Buffered();
            if (s == 0L)
            {
                return;
            }
            var (peek, _) = r.R.Peek(s);
            while (len(peek) > 0L)
            {
                var i = bytes.IndexByte(peek, '\n');
                if (i < 3L)
                { 
                    // Not present (-1) or found within the next few bytes,
                    // implying we're at the end ("\r\n\r\n" or "\n\n")
                    return;
                }
                n++;
                peek = peek[i + 1L..];
            }

            return;
        }

        // CanonicalMIMEHeaderKey returns the canonical format of the
        // MIME header key s. The canonicalization converts the first
        // letter and any letter following a hyphen to upper case;
        // the rest are converted to lowercase. For example, the
        // canonical key for "accept-encoding" is "Accept-Encoding".
        // MIME header keys are assumed to be ASCII only.
        // If s contains a space or invalid header field bytes, it is
        // returned without modifications.
        public static @string CanonicalMIMEHeaderKey(@string s)
        { 
            // Quick check for canonical encoding.
            var upper = true;
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];
                if (!validHeaderFieldByte(c))
                {
                    return s;
                }
                if (upper && 'a' <= c && c <= 'z')
                {
                    return canonicalMIMEHeaderKey((slice<byte>)s);
                }
                if (!upper && 'A' <= c && c <= 'Z')
                {
                    return canonicalMIMEHeaderKey((slice<byte>)s);
                }
                upper = c == '-';
            }

            return s;
        }

        private static readonly char toLower = 'a' - 'A';

        // validHeaderFieldByte reports whether b is a valid byte in a header
        // field name. RFC 7230 says:
        //   header-field   = field-name ":" OWS field-value OWS
        //   field-name     = token
        //   tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "." /
        //           "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA
        //   token = 1*tchar


        // validHeaderFieldByte reports whether b is a valid byte in a header
        // field name. RFC 7230 says:
        //   header-field   = field-name ":" OWS field-value OWS
        //   field-name     = token
        //   tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "." /
        //           "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA
        //   token = 1*tchar
        private static bool validHeaderFieldByte(byte b)
        {
            return int(b) < len(isTokenTable) && isTokenTable[b];
        }

        // canonicalMIMEHeaderKey is like CanonicalMIMEHeaderKey but is
        // allowed to mutate the provided byte slice before returning the
        // string.
        //
        // For invalid inputs (if a contains spaces or non-token bytes), a
        // is unchanged and a string copy is returned.
        private static @string canonicalMIMEHeaderKey(slice<byte> a)
        { 
            // See if a looks like a header key. If not, return it unchanged.
            {
                var c__prev1 = c;

                foreach (var (_, __c) in a)
                {
                    c = __c;
                    if (validHeaderFieldByte(c))
                    {
                        continue;
                    } 
                    // Don't canonicalize.
                    return string(a);
                }

                c = c__prev1;
            }

            var upper = true;
            {
                var c__prev1 = c;

                foreach (var (__i, __c) in a)
                {
                    i = __i;
                    c = __c; 
                    // Canonicalize: first letter upper case
                    // and upper case after each dash.
                    // (Host, User-Agent, If-Modified-Since).
                    // MIME headers are ASCII only, so no Unicode issues.
                    if (upper && 'a' <= c && c <= 'z')
                    {
                        c -= toLower;
                    }
                    else if (!upper && 'A' <= c && c <= 'Z')
                    {
                        c += toLower;
                    }
                    a[i] = c;
                    upper = c == '-'; // for next time
                } 
                // The compiler recognizes m[string(byteSlice)] as a special
                // case, so a copy of a's bytes into a new string does not
                // happen in this map lookup:

                c = c__prev1;
            }

            {
                var v = commonHeader[string(a)];

                if (v != "")
                {
                    return v;
                }

            }
            return string(a);
        }

        // commonHeader interns common header strings.
        private static var commonHeader = make_map<@string, @string>();

        private static void init()
        {
            foreach (var (_, v) in new slice<@string>(new @string[] { "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language", "Accept-Ranges", "Cache-Control", "Cc", "Connection", "Content-Id", "Content-Language", "Content-Length", "Content-Transfer-Encoding", "Content-Type", "Cookie", "Date", "Dkim-Signature", "Etag", "Expires", "From", "Host", "If-Modified-Since", "If-None-Match", "In-Reply-To", "Last-Modified", "Location", "Message-Id", "Mime-Version", "Pragma", "Received", "Return-Path", "Server", "Set-Cookie", "Subject", "To", "User-Agent", "Via", "X-Forwarded-For", "X-Imforwards", "X-Powered-By" }))
            {
                commonHeader[v] = v;
            }
        }

        // isTokenTable is a copy of net/http/lex.go's isTokenTable.
        // See https://httpwg.github.io/specs/rfc7230.html#rule.token.separators
        private static array<bool> isTokenTable = new array<bool>(InitKeyedValues<bool>(127, ('!', true), ('#', true), ('$', true), ('%', true), ('&', true), ('\'', true), ('*', true), ('+', true), ('-', true), ('.', true), ('0', true), ('1', true), ('2', true), ('3', true), ('4', true), ('5', true), ('6', true), ('7', true), ('8', true), ('9', true), ('A', true), ('B', true), ('C', true), ('D', true), ('E', true), ('F', true), ('G', true), ('H', true), ('I', true), ('J', true), ('K', true), ('L', true), ('M', true), ('N', true), ('O', true), ('P', true), ('Q', true), ('R', true), ('S', true), ('T', true), ('U', true), ('W', true), ('V', true), ('X', true), ('Y', true), ('Z', true), ('^', true), ('_', true), ('`', true), ('a', true), ('b', true), ('c', true), ('d', true), ('e', true), ('f', true), ('g', true), ('h', true), ('i', true), ('j', true), ('k', true), ('l', true), ('m', true), ('n', true), ('o', true), ('p', true), ('q', true), ('r', true), ('s', true), ('t', true), ('u', true), ('v', true), ('w', true), ('x', true), ('y', true), ('z', true), ('|', true), ('~', true)));
    }
}}
