// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package textproto -- go2cs converted at 2022 March 06 22:21:11 UTC
// import "net/textproto" ==> using textproto = go.net.textproto_package
// Original source: C:\Program Files\Go\src\net\textproto\reader.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using System;


namespace go.net;

public static partial class textproto_package {

    // A Reader implements convenience methods for reading requests
    // or responses from a text protocol network connection.
public partial struct Reader {
    public ptr<bufio.Reader> R;
    public ptr<dotReader> dot;
    public slice<byte> buf; // a re-usable buffer for readContinuedLineSlice
}

// NewReader returns a new Reader reading from r.
//
// To avoid denial of service attacks, the provided bufio.Reader
// should be reading from an io.LimitReader or similar Reader to bound
// the size of responses.
public static ptr<Reader> NewReader(ptr<bufio.Reader> _addr_r) {
    ref bufio.Reader r = ref _addr_r.val;

    commonHeaderOnce.Do(initCommonHeader);
    return addr(new Reader(R:r));
}

// ReadLine reads a single line from r,
// eliding the final \n or \r\n from the returned string.
private static (@string, error) ReadLine(this ptr<Reader> _addr_r) {
    @string _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    var (line, err) = r.readLineSlice();
    return (string(line), error.As(err)!);
}

// ReadLineBytes is like ReadLine but returns a []byte instead of a string.
private static (slice<byte>, error) ReadLineBytes(this ptr<Reader> _addr_r) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    var (line, err) = r.readLineSlice();
    if (line != null) {
        var buf = make_slice<byte>(len(line));
        copy(buf, line);
        line = buf;
    }
    return (line, error.As(err)!);

}

private static (slice<byte>, error) readLineSlice(this ptr<Reader> _addr_r) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    r.closeDot();
    slice<byte> line = default;
    while (true) {
        var (l, more, err) = r.R.ReadLine();
        if (err != null) {
            return (null, error.As(err)!);
        }
        if (line == null && !more) {
            return (l, error.As(null!)!);
        }
        line = append(line, l);
        if (!more) {
            break;
        }
    }
    return (line, error.As(null!)!);

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
// Empty lines are never continued.
//
private static (@string, error) ReadContinuedLine(this ptr<Reader> _addr_r) {
    @string _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    var (line, err) = r.readContinuedLineSlice(noValidation);
    return (string(line), error.As(err)!);
}

// trim returns s with leading and trailing spaces and tabs removed.
// It does not assume Unicode or UTF-8.
private static slice<byte> trim(slice<byte> s) {
    nint i = 0;
    while (i < len(s) && (s[i] == ' ' || s[i] == '\t')) {
        i++;
    }
    var n = len(s);
    while (n > i && (s[n - 1] == ' ' || s[n - 1] == '\t')) {
        n--;
    }
    return s[(int)i..(int)n];
}

// ReadContinuedLineBytes is like ReadContinuedLine but
// returns a []byte instead of a string.
private static (slice<byte>, error) ReadContinuedLineBytes(this ptr<Reader> _addr_r) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    var (line, err) = r.readContinuedLineSlice(noValidation);
    if (line != null) {
        var buf = make_slice<byte>(len(line));
        copy(buf, line);
        line = buf;
    }
    return (line, error.As(err)!);

}

// readContinuedLineSlice reads continued lines from the reader buffer,
// returning a byte slice with all lines. The validateFirstLine function
// is run on the first read line, and if it returns an error then this
// error is returned from readContinuedLineSlice.
private static (slice<byte>, error) readContinuedLineSlice(this ptr<Reader> _addr_r, Func<slice<byte>, error> validateFirstLine) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    if (validateFirstLine == null) {
        return (null, error.As(fmt.Errorf("missing validateFirstLine func"))!);
    }
    var (line, err) = r.readLineSlice();
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (len(line) == 0) { // blank line - no continuation
        return (line, error.As(null!)!);

    }
    {
        var err = validateFirstLine(line);

        if (err != null) {
            return (null, error.As(err)!);
        }
    } 

    // Optimistically assume that we have started to buffer the next line
    // and it starts with an ASCII letter (the next header key), or a blank
    // line, so we can avoid copying that buffered data around in memory
    // and skipping over non-existent whitespace.
    if (r.R.Buffered() > 1) {
        var (peek, _) = r.R.Peek(2);
        if (len(peek) > 0 && (isASCIILetter(peek[0]) || peek[0] == '\n') || len(peek) == 2 && peek[0] == '\r' && peek[1] == '\n') {
            return (trim(line), error.As(null!)!);
        }
    }
    r.buf = append(r.buf[..(int)0], trim(line)); 

    // Read continuation lines.
    while (r.skipSpace() > 0) {
        (line, err) = r.readLineSlice();
        if (err != null) {
            break;
        }
        r.buf = append(r.buf, ' ');
        r.buf = append(r.buf, trim(line));

    }
    return (r.buf, error.As(null!)!);

}

// skipSpace skips R over all spaces and returns the number of bytes skipped.
private static nint skipSpace(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    nint n = 0;
    while (true) {
        var (c, err) = r.R.ReadByte();
        if (err != null) { 
            // Bufio will keep err until next read.
            break;

        }
        if (c != ' ' && c != '\t') {
            r.R.UnreadByte();
            break;
        }
        n++;

    }
    return n;

}

private static (nint, bool, @string, error) readCodeLine(this ptr<Reader> _addr_r, nint expectCode) {
    nint code = default;
    bool continued = default;
    @string message = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    var (line, err) = r.ReadLine();
    if (err != null) {
        return ;
    }
    return parseCodeLine(line, expectCode);

}

private static (nint, bool, @string, error) parseCodeLine(@string line, nint expectCode) {
    nint code = default;
    bool continued = default;
    @string message = default;
    error err = default!;

    if (len(line) < 4 || line[3] != ' ' && line[3] != '-') {
        err = ProtocolError("short response: " + line);
        return ;
    }
    continued = line[3] == '-';
    code, err = strconv.Atoi(line[(int)0..(int)3]);
    if (err != null || code < 100) {
        err = ProtocolError("invalid response code: " + line);
        return ;
    }
    message = line[(int)4..];
    if (1 <= expectCode && expectCode < 10 && code / 100 != expectCode || 10 <= expectCode && expectCode < 100 && code / 10 != expectCode || 100 <= expectCode && expectCode < 1000 && code != expectCode) {
        err = addr(new Error(code,message));
    }
    return ;

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
private static (nint, @string, error) ReadCodeLine(this ptr<Reader> _addr_r, nint expectCode) {
    nint code = default;
    @string message = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    var (code, continued, message, err) = r.readCodeLine(expectCode);
    if (err == null && continued) {
        err = ProtocolError("unexpected multi-line response: " + message);
    }
    return ;

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
// See page 36 of RFC 959 (https://www.ietf.org/rfc/rfc959.txt) for
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
private static (nint, @string, error) ReadResponse(this ptr<Reader> _addr_r, nint expectCode) {
    nint code = default;
    @string message = default;
    error err = default!;
    ref Reader r = ref _addr_r.val;

    var (code, continued, message, err) = r.readCodeLine(expectCode);
    var multi = continued;
    while (continued) {
        var (line, err) = r.ReadLine();
        if (err != null) {
            return (0, "", error.As(err)!);
        }
        nint code2 = default;
        @string moreMessage = default;
        code2, continued, moreMessage, err = parseCodeLine(line, 0);
        if (err != null || code2 != code) {
            message += "\n" + strings.TrimRight(line, "\r\n");
            continued = true;
            continue;
        }
        message += "\n" + moreMessage;

    }
    if (err != null && multi && message != "") { 
        // replace one line error message with all lines (full message)
        err = addr(new Error(code,message));

    }
    return ;

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
private static io.Reader DotReader(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    r.closeDot();
    r.dot = addr(new dotReader(r:r));
    return r.dot;
}

private partial struct dotReader {
    public ptr<Reader> r;
    public nint state;
}

// Read satisfies reads by decoding dot-encoded data read from d.r.
private static (nint, error) Read(this ptr<dotReader> _addr_d, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref dotReader d = ref _addr_d.val;
 
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
    while (n < len(b) && d.state != stateEOF) {
        byte c = default;
        c, err = br.ReadByte();
        if (err != null) {
            if (err == io.EOF) {
                err = io.ErrUnexpectedEOF;
            }
            break;
        }

        if (d.state == stateBeginLine) 
            if (c == '.') {
                d.state = stateDot;
                continue;
            }
            if (c == '\r') {
                d.state = stateCR;
                continue;
            }
            d.state = stateData;
        else if (d.state == stateDot) 
            if (c == '\r') {
                d.state = stateDotCR;
                continue;
            }
            if (c == '\n') {
                d.state = stateEOF;
                continue;
            }
            d.state = stateData;
        else if (d.state == stateDotCR) 
            if (c == '\n') {
                d.state = stateEOF;
                continue;
            } 
            // Not part of .\r\n.
            // Consume leading dot and emit saved \r.
            br.UnreadByte();
            c = '\r';
            d.state = stateData;
        else if (d.state == stateCR) 
            if (c == '\n') {
                d.state = stateBeginLine;
                break;
            } 
            // Not part of \r\n. Emit saved \r
            br.UnreadByte();
            c = '\r';
            d.state = stateData;
        else if (d.state == stateData) 
            if (c == '\r') {
                d.state = stateCR;
                continue;
            }
            if (c == '\n') {
                d.state = stateBeginLine;
            }
                b[n] = c;
        n++;

    }
    if (err == null && d.state == stateEOF) {
        err = io.EOF;
    }
    if (err != null && d.r.dot == d) {
        d.r.dot = null;
    }
    return ;

}

// closeDot drains the current DotReader if any,
// making sure that it reads until the ending dot line.
private static void closeDot(this ptr<Reader> _addr_r) {
    ref Reader r = ref _addr_r.val;

    if (r.dot == null) {
        return ;
    }
    var buf = make_slice<byte>(128);
    while (r.dot != null) { 
        // When Read reaches EOF or an error,
        // it will set r.dot == nil.
        r.dot.Read(buf);

    }

}

// ReadDotBytes reads a dot-encoding and returns the decoded data.
//
// See the documentation for the DotReader method for details about dot-encoding.
private static (slice<byte>, error) ReadDotBytes(this ptr<Reader> _addr_r) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;

    return io.ReadAll(r.DotReader());
}

// ReadDotLines reads a dot-encoding and returns a slice
// containing the decoded lines, with the final \r\n or \n elided from each.
//
// See the documentation for the DotReader method for details about dot-encoding.
private static (slice<@string>, error) ReadDotLines(this ptr<Reader> _addr_r) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;
 
    // We could use ReadDotBytes and then Split it,
    // but reading a line at a time avoids needing a
    // large contiguous block of memory and is simpler.
    slice<@string> v = default;
    error err = default!;
    while (true) {
        @string line = default;
        line, err = r.ReadLine();
        if (err != null) {
            if (err == io.EOF) {
                err = error.As(io.ErrUnexpectedEOF)!;
            }
            break;
        }
        if (len(line) > 0 && line[0] == '.') {
            if (len(line) == 1) {
                break;
            }
            line = line[(int)1..];
        }
        v = append(v, line);

    }
    return (v, error.As(err)!);

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
private static (MIMEHeader, error) ReadMIMEHeader(this ptr<Reader> _addr_r) {
    MIMEHeader _p0 = default;
    error _p0 = default!;
    ref Reader r = ref _addr_r.val;
 
    // Avoid lots of small slice allocations later by allocating one
    // large one ahead of time which we'll cut up into smaller
    // slices. If this isn't big enough later, we allocate small ones.
    slice<@string> strs = default;
    var hint = r.upcomingHeaderNewlines();
    if (hint > 0) {
        strs = make_slice<@string>(hint);
    }
    var m = make(MIMEHeader, hint); 

    // The first line cannot start with a leading space.
    {
        var (buf, err) = r.R.Peek(1);

        if (err == null && (buf[0] == ' ' || buf[0] == '\t')) {
            var (line, err) = r.readLineSlice();
            if (err != null) {
                return (m, error.As(err)!);
            }
            return (m, error.As(ProtocolError("malformed MIME header initial line: " + string(line)))!);
        }
    }


    while (true) {
        var (kv, err) = r.readContinuedLineSlice(mustHaveFieldNameColon);
        if (len(kv) == 0) {
            return (m, error.As(err)!);
        }
        var i = bytes.IndexByte(kv, ':');
        if (i < 0) {
            return (m, error.As(ProtocolError("malformed MIME header line: " + string(kv)))!);
        }
        var key = canonicalMIMEHeaderKey(kv[..(int)i]); 

        // As per RFC 7230 field-name is a token, tokens consist of one or more chars.
        // We could return a ProtocolError here, but better to be liberal in what we
        // accept, so if we get an empty key, skip it.
        if (key == "") {
            continue;
        }
        i++; // skip colon
        while (i < len(kv) && (kv[i] == ' ' || kv[i] == '\t')) {
            i++;
        }
        var value = string(kv[(int)i..]);

        var vv = m[key];
        if (vv == null && len(strs) > 0) { 
            // More than likely this will be a single-element key.
            // Most headers aren't multi-valued.
            // Set the capacity on strs[0] to 1, so any future append
            // won't extend the slice into the other strings.
            (vv, strs) = (strs.slice(-1, 1, 1), strs[(int)1..]);            vv[0] = value;
            m[key] = vv;

        }
        else
 {
            m[key] = append(vv, value);
        }
        if (err != null) {
            return (m, error.As(err)!);
        }
    }

}

// noValidation is a no-op validation func for readContinuedLineSlice
// that permits any lines.
private static error noValidation(slice<byte> _) {
    return error.As(null!)!;
}

// mustHaveFieldNameColon ensures that, per RFC 7230, the
// field-name is on a single line, so the first line must
// contain a colon.
private static error mustHaveFieldNameColon(slice<byte> line) {
    if (bytes.IndexByte(line, ':') < 0) {
        return error.As(ProtocolError(fmt.Sprintf("malformed MIME header: missing colon: %q", line)))!;
    }
    return error.As(null!)!;

}

// upcomingHeaderNewlines returns an approximation of the number of newlines
// that will be in this header. If it gets confused, it returns 0.
private static nint upcomingHeaderNewlines(this ptr<Reader> _addr_r) {
    nint n = default;
    ref Reader r = ref _addr_r.val;
 
    // Try to determine the 'hint' size.
    r.R.Peek(1); // force a buffer load if empty
    var s = r.R.Buffered();
    if (s == 0) {
        return ;
    }
    var (peek, _) = r.R.Peek(s);
    while (len(peek) > 0) {
        var i = bytes.IndexByte(peek, '\n');
        if (i < 3) { 
            // Not present (-1) or found within the next few bytes,
            // implying we're at the end ("\r\n\r\n" or "\n\n")
            return ;

        }
        n++;
        peek = peek[(int)i + 1..];

    }
    return ;

}

// CanonicalMIMEHeaderKey returns the canonical format of the
// MIME header key s. The canonicalization converts the first
// letter and any letter following a hyphen to upper case;
// the rest are converted to lowercase. For example, the
// canonical key for "accept-encoding" is "Accept-Encoding".
// MIME header keys are assumed to be ASCII only.
// If s contains a space or invalid header field bytes, it is
// returned without modifications.
public static @string CanonicalMIMEHeaderKey(@string s) {
    commonHeaderOnce.Do(initCommonHeader); 

    // Quick check for canonical encoding.
    var upper = true;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (!validHeaderFieldByte(c)) {
            return s;
        }
        if (upper && 'a' <= c && c <= 'z') {
            return canonicalMIMEHeaderKey((slice<byte>)s);
        }
        if (!upper && 'A' <= c && c <= 'Z') {
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
private static bool validHeaderFieldByte(byte b) {
    return int(b) < len(isTokenTable) && isTokenTable[b];
}

// canonicalMIMEHeaderKey is like CanonicalMIMEHeaderKey but is
// allowed to mutate the provided byte slice before returning the
// string.
//
// For invalid inputs (if a contains spaces or non-token bytes), a
// is unchanged and a string copy is returned.
private static @string canonicalMIMEHeaderKey(slice<byte> a) { 
    // See if a looks like a header key. If not, return it unchanged.
    {
        var c__prev1 = c;

        foreach (var (_, __c) in a) {
            c = __c;
            if (validHeaderFieldByte(c)) {
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

        foreach (var (__i, __c) in a) {
            i = __i;
            c = __c; 
            // Canonicalize: first letter upper case
            // and upper case after each dash.
            // (Host, User-Agent, If-Modified-Since).
            // MIME headers are ASCII only, so no Unicode issues.
            if (upper && 'a' <= c && c <= 'z') {
                c -= toLower;
            }
            else if (!upper && 'A' <= c && c <= 'Z') {
                c += toLower;
            }

            a[i] = c;
            upper = c == '-'; // for next time
        }
        c = c__prev1;
    }

    {
        var v = commonHeader[string(a)];

        if (v != "") {
            return v;
        }
    }

    return string(a);

}

// commonHeader interns common header strings.
private static map<@string, @string> commonHeader = default;

private static sync.Once commonHeaderOnce = default;

private static void initCommonHeader() {
    commonHeader = make_map<@string, @string>();
    foreach (var (_, v) in new slice<@string>(new @string[] { "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language", "Accept-Ranges", "Cache-Control", "Cc", "Connection", "Content-Id", "Content-Language", "Content-Length", "Content-Transfer-Encoding", "Content-Type", "Cookie", "Date", "Dkim-Signature", "Etag", "Expires", "From", "Host", "If-Modified-Since", "If-None-Match", "In-Reply-To", "Last-Modified", "Location", "Message-Id", "Mime-Version", "Pragma", "Received", "Return-Path", "Server", "Set-Cookie", "Subject", "To", "User-Agent", "Via", "X-Forwarded-For", "X-Imforwards", "X-Powered-By" })) {
        commonHeader[v] = v;
    }
}

// isTokenTable is a copy of net/http/lex.go's isTokenTable.
// See https://httpwg.github.io/specs/rfc7230.html#rule.token.separators
private static array<bool> isTokenTable = new array<bool>(InitKeyedValues<bool>(127, ('!', true), ('#', true), ('$', true), ('%', true), ('&', true), ('\'', true), ('*', true), ('+', true), ('-', true), ('.', true), ('0', true), ('1', true), ('2', true), ('3', true), ('4', true), ('5', true), ('6', true), ('7', true), ('8', true), ('9', true), ('A', true), ('B', true), ('C', true), ('D', true), ('E', true), ('F', true), ('G', true), ('H', true), ('I', true), ('J', true), ('K', true), ('L', true), ('M', true), ('N', true), ('O', true), ('P', true), ('Q', true), ('R', true), ('S', true), ('T', true), ('U', true), ('W', true), ('V', true), ('X', true), ('Y', true), ('Z', true), ('^', true), ('_', true), ('`', true), ('a', true), ('b', true), ('c', true), ('d', true), ('e', true), ('f', true), ('g', true), ('h', true), ('i', true), ('j', true), ('k', true), ('l', true), ('m', true), ('n', true), ('o', true), ('p', true), ('q', true), ('r', true), ('s', true), ('t', true), ('u', true), ('v', true), ('w', true), ('x', true), ('y', true), ('z', true), ('|', true), ('~', true)));

} // end textproto_package
