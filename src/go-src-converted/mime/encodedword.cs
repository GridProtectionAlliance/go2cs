// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2022 March 06 22:21:14 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\encodedword.go
using bytes = go.bytes_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go;

public static partial class mime_package {

    // A WordEncoder is an RFC 2047 encoded-word encoder.
public partial struct WordEncoder { // : byte
}

 
// BEncoding represents Base64 encoding scheme as defined by RFC 2045.
public static readonly var BEncoding = WordEncoder('b'); 
// QEncoding represents the Q-encoding scheme as defined by RFC 2047.
public static readonly var QEncoding = WordEncoder('q');


private static var errInvalidWord = errors.New("mime: invalid RFC 2047 encoded-word");

// Encode returns the encoded-word form of s. If s is ASCII without special
// characters, it is returned unchanged. The provided charset is the IANA
// charset name of s. It is case insensitive.
public static @string Encode(this WordEncoder e, @string charset, @string s) {
    if (!needsEncoding(s)) {
        return s;
    }
    return e.encodeWord(charset, s);

}

private static bool needsEncoding(@string s) {
    foreach (var (_, b) in s) {
        if ((b < ' ' || b > '~') && b != '\t') {
            return true;
        }
    }    return false;

}

// encodeWord encodes a string into an encoded-word.
public static @string encodeWord(this WordEncoder e, @string charset, @string s) {
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf); 
    // Could use a hint like len(s)*3, but that's not enough for cases
    // with word splits and too much for simpler inputs.
    // 48 is close to maxEncodedWordLen/2, but adjusted to allocator size class.
    buf.Grow(48);

    e.openWord(_addr_buf, charset);
    if (e == BEncoding) {
        e.bEncode(_addr_buf, charset, s);
    }
    else
 {
        e.qEncode(_addr_buf, charset, s);
    }
    closeWord(_addr_buf);

    return buf.String();

}

 
// The maximum length of an encoded-word is 75 characters.
// See RFC 2047, section 2.
private static readonly nint maxEncodedWordLen = 75; 
// maxContentLen is how much content can be encoded, ignoring the header and
// 2-byte footer.
private static readonly var maxContentLen = maxEncodedWordLen - len("=?UTF-8?q?") - len("?=");


private static var maxBase64Len = base64.StdEncoding.DecodedLen(maxContentLen);

// bEncode encodes s using base64 encoding and writes it to buf.
public static void bEncode(this WordEncoder e, ptr<strings.Builder> _addr_buf, @string charset, @string s) {
    ref strings.Builder buf = ref _addr_buf.val;

    var w = base64.NewEncoder(base64.StdEncoding, buf); 
    // If the charset is not UTF-8 or if the content is short, do not bother
    // splitting the encoded-word.
    if (!isUTF8(charset) || base64.StdEncoding.EncodedLen(len(s)) <= maxContentLen) {
        io.WriteString(w, s);
        w.Close();
        return ;
    }
    nint currentLen = default;    nint last = default;    nint runeLen = default;

    {
        nint i = 0;

        while (i < len(s)) { 
            // Multi-byte characters must not be split across encoded-words.
            // See RFC 2047, section 5.3.
            _, runeLen = utf8.DecodeRuneInString(s[(int)i..]);

            if (currentLen + runeLen <= maxBase64Len) {
                currentLen += runeLen;
            i += runeLen;
            }
            else
 {
                io.WriteString(w, s[(int)last..(int)i]);
                w.Close();
                e.splitWord(buf, charset);
                last = i;
                currentLen = runeLen;
            }

        }
    }
    io.WriteString(w, s[(int)last..]);
    w.Close();

}

// qEncode encodes s using Q encoding and writes it to buf. It splits the
// encoded-words when necessary.
public static void qEncode(this WordEncoder e, ptr<strings.Builder> _addr_buf, @string charset, @string s) {
    ref strings.Builder buf = ref _addr_buf.val;
 
    // We only split encoded-words when the charset is UTF-8.
    if (!isUTF8(charset)) {
        writeQString(_addr_buf, s);
        return ;
    }
    nint currentLen = default;    nint runeLen = default;

    {
        nint i = 0;

        while (i < len(s)) {
            var b = s[i]; 
            // Multi-byte characters must not be split across encoded-words.
            // See RFC 2047, section 5.3.
            nint encLen = default;
            if (b >= ' ' && b <= '~' && b != '=' && b != '?' && b != '_') {
                (runeLen, encLen) = (1, 1);            i += runeLen;
            }
            else
 {
                _, runeLen = utf8.DecodeRuneInString(s[(int)i..]);
                encLen = 3 * runeLen;
            }

            if (currentLen + encLen > maxContentLen) {
                e.splitWord(buf, charset);
                currentLen = 0;
            }

            writeQString(_addr_buf, s[(int)i..(int)i + runeLen]);
            currentLen += encLen;

        }
    }

}

// writeQString encodes s using Q encoding and writes it to buf.
private static void writeQString(ptr<strings.Builder> _addr_buf, @string s) {
    ref strings.Builder buf = ref _addr_buf.val;

    for (nint i = 0; i < len(s); i++) {
        {
            var b = s[i];


            if (b == ' ') 
                buf.WriteByte('_');
            else if (b >= '!' && b <= '~' && b != '=' && b != '?' && b != '_') 
                buf.WriteByte(b);
            else 
                buf.WriteByte('=');
                buf.WriteByte(upperhex[b >> 4]);
                buf.WriteByte(upperhex[b & 0x0f]);

        }

    }

}

// openWord writes the beginning of an encoded-word into buf.
public static void openWord(this WordEncoder e, ptr<strings.Builder> _addr_buf, @string charset) {
    ref strings.Builder buf = ref _addr_buf.val;

    buf.WriteString("=?");
    buf.WriteString(charset);
    buf.WriteByte('?');
    buf.WriteByte(byte(e));
    buf.WriteByte('?');
}

// closeWord writes the end of an encoded-word into buf.
private static void closeWord(ptr<strings.Builder> _addr_buf) {
    ref strings.Builder buf = ref _addr_buf.val;

    buf.WriteString("?=");
}

// splitWord closes the current encoded-word and opens a new one.
public static void splitWord(this WordEncoder e, ptr<strings.Builder> _addr_buf, @string charset) {
    ref strings.Builder buf = ref _addr_buf.val;

    closeWord(_addr_buf);
    buf.WriteByte(' ');
    e.openWord(buf, charset);
}

private static bool isUTF8(@string charset) {
    return strings.EqualFold(charset, "UTF-8");
}

private static readonly @string upperhex = "0123456789ABCDEF";

// A WordDecoder decodes MIME headers containing RFC 2047 encoded-words.


// A WordDecoder decodes MIME headers containing RFC 2047 encoded-words.
public partial struct WordDecoder {
    public Func<@string, io.Reader, (io.Reader, error)> CharsetReader;
}

// Decode decodes an RFC 2047 encoded-word.
private static (@string, error) Decode(this ptr<WordDecoder> _addr_d, @string word) {
    @string _p0 = default;
    error _p0 = default!;
    ref WordDecoder d = ref _addr_d.val;
 
    // See https://tools.ietf.org/html/rfc2047#section-2 for details.
    // Our decoder is permissive, we accept empty encoded-text.
    if (len(word) < 8 || !strings.HasPrefix(word, "=?") || !strings.HasSuffix(word, "?=") || strings.Count(word, "?") != 4) {
        return ("", error.As(errInvalidWord)!);
    }
    word = word[(int)2..(int)len(word) - 2]; 

    // split delimits the first 2 fields
    var split = strings.IndexByte(word, '?'); 

    // split word "UTF-8?q?ascii" into "UTF-8", 'q', and "ascii"
    var charset = word[..(int)split];
    if (len(charset) == 0) {
        return ("", error.As(errInvalidWord)!);
    }
    if (len(word) < split + 3) {
        return ("", error.As(errInvalidWord)!);
    }
    var encoding = word[split + 1]; 
    // the field after split must only be one byte
    if (word[split + 2] != '?') {
        return ("", error.As(errInvalidWord)!);
    }
    var text = word[(int)split + 3..];

    var (content, err) = decode(encoding, text);
    if (err != null) {
        return ("", error.As(err)!);
    }
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);

    {
        var err = d.convert(_addr_buf, charset, content);

        if (err != null) {
            return ("", error.As(err)!);
        }
    }


    return (buf.String(), error.As(null!)!);

}

// DecodeHeader decodes all encoded-words of the given string. It returns an
// error if and only if CharsetReader of d returns an error.
private static (@string, error) DecodeHeader(this ptr<WordDecoder> _addr_d, @string header) {
    @string _p0 = default;
    error _p0 = default!;
    ref WordDecoder d = ref _addr_d.val;
 
    // If there is no encoded-word, returns before creating a buffer.
    var i = strings.Index(header, "=?");
    if (i == -1) {
        return (header, error.As(null!)!);
    }
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);

    buf.WriteString(header[..(int)i]);
    header = header[(int)i..];

    var betweenWords = false;
    while (true) {
        var start = strings.Index(header, "=?");
        if (start == -1) {
            break;
        }
        var cur = start + len("=?");

        i = strings.Index(header[(int)cur..], "?");
        if (i == -1) {
            break;
        }
        var charset = header[(int)cur..(int)cur + i];
        cur += i + len("?");

        if (len(header) < cur + len("Q??=")) {
            break;
        }
        var encoding = header[cur];
        cur++;

        if (header[cur] != '?') {
            break;
        }
        cur++;

        var j = strings.Index(header[(int)cur..], "?=");
        if (j == -1) {
            break;
        }
        var text = header[(int)cur..(int)cur + j];
        var end = cur + j + len("?=");

        var (content, err) = decode(encoding, text);
        if (err != null) {
            betweenWords = false;
            buf.WriteString(header[..(int)start + 2]);
            header = header[(int)start + 2..];
            continue;
        }
        if (start > 0 && (!betweenWords || hasNonWhitespace(header[..(int)start]))) {
            buf.WriteString(header[..(int)start]);
        }
        {
            var err = d.convert(_addr_buf, charset, content);

            if (err != null) {
                return ("", error.As(err)!);
            }

        }


        header = header[(int)end..];
        betweenWords = true;

    }

    if (len(header) > 0) {
        buf.WriteString(header);
    }
    return (buf.String(), error.As(null!)!);

}

private static (slice<byte>, error) decode(byte encoding, @string text) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    switch (encoding) {
        case 'B': 

        case 'b': 
            return base64.StdEncoding.DecodeString(text);
            break;
        case 'Q': 

        case 'q': 
            return qDecode(text);
            break;
        default: 
            return (null, error.As(errInvalidWord)!);
            break;
    }

}

private static error convert(this ptr<WordDecoder> _addr_d, ptr<strings.Builder> _addr_buf, @string charset, slice<byte> content) {
    ref WordDecoder d = ref _addr_d.val;
    ref strings.Builder buf = ref _addr_buf.val;


    if (strings.EqualFold("utf-8", charset)) 
        buf.Write(content);
    else if (strings.EqualFold("iso-8859-1", charset)) 
        {
            var c__prev1 = c;

            foreach (var (_, __c) in content) {
                c = __c;
                buf.WriteRune(rune(c));
            }

            c = c__prev1;
        }
    else if (strings.EqualFold("us-ascii", charset)) 
        {
            var c__prev1 = c;

            foreach (var (_, __c) in content) {
                c = __c;
                if (c >= utf8.RuneSelf) {
                    buf.WriteRune(unicode.ReplacementChar);
                }
                else
 {
                    buf.WriteByte(c);
                }

            }

            c = c__prev1;
        }
    else 
        if (d.CharsetReader == null) {
            return error.As(fmt.Errorf("mime: unhandled charset %q", charset))!;
        }
        var (r, err) = d.CharsetReader(strings.ToLower(charset), bytes.NewReader(content));
        if (err != null) {
            return error.As(err)!;
        }
        _, err = io.Copy(buf, r);

        if (err != null) {
            return error.As(err)!;
        }
        return error.As(null!)!;

}

// hasNonWhitespace reports whether s (assumed to be ASCII) contains at least
// one byte of non-whitespace.
private static bool hasNonWhitespace(@string s) {
    foreach (var (_, b) in s) {
        switch (b) { 
        // Encoded-words can only be separated by linear white spaces which does
        // not include vertical tabs (\v).
            case ' ': 

            case '\t': 

            case '\n': 

            case '\r': 

                break;
            default: 
                return true;
                break;
        }

    }    return false;

}

// qDecode decodes a Q encoded string.
private static (slice<byte>, error) qDecode(@string s) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var dec = make_slice<byte>(len(s));
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        {
            var c = s[i];


            if (c == '_') 
                dec[n] = ' ';
            else if (c == '=') 
                if (i + 2 >= len(s)) {
                    return (null, error.As(errInvalidWord)!);
                }
                var (b, err) = readHexByte(s[i + 1], s[i + 2]);
                if (err != null) {
                    return (null, error.As(err)!);
                }
                dec[n] = b;
                i += 2;
            else if ((c <= '~' && c >= ' ') || c == '\n' || c == '\r' || c == '\t') 
                dec[n] = c;
            else 
                return (null, error.As(errInvalidWord)!);

        }
        n++;

    }

    return (dec[..(int)n], error.As(null!)!);

}

// readHexByte returns the byte from its quoted-printable representation.
private static (byte, error) readHexByte(byte a, byte b) {
    byte _p0 = default;
    error _p0 = default!;

    byte hb = default;    byte lb = default;

    error err = default!;
    hb, err = fromHex(a);

    if (err != null) {
        return (0, error.As(err)!);
    }
    lb, err = fromHex(b);

    if (err != null) {
        return (0, error.As(err)!);
    }
    return (hb << 4 | lb, error.As(null!)!);

}

private static (byte, error) fromHex(byte b) {
    byte _p0 = default;
    error _p0 = default!;


    if (b >= '0' && b <= '9') 
        return (b - '0', error.As(null!)!);
    else if (b >= 'A' && b <= 'F') 
        return (b - 'A' + 10, error.As(null!)!); 
        // Accept badly encoded bytes.
    else if (b >= 'a' && b <= 'f') 
        return (b - 'a' + 10, error.As(null!)!);
        return (0, error.As(fmt.Errorf("mime: invalid hex byte %#02x", b))!);

}

} // end mime_package
