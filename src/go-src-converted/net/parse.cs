// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Simple file i/o and string manipulation, to avoid
// depending on strconv and bufio and strings.

// package net -- go2cs converted at 2022 March 06 22:16:29 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\parse.go
using bytealg = go.@internal.bytealg_package;
using io = go.io_package;
using os = go.os_package;
using time = go.time_package;
using System;


namespace go;

public static partial class net_package {

private partial struct file {
    public ptr<os.File> file;
    public slice<byte> data;
    public bool atEOF;
}

private static void close(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    f.file.Close();
}

private static (@string, bool) getLineFromData(this ptr<file> _addr_f) {
    @string s = default;
    bool ok = default;
    ref file f = ref _addr_f.val;

    var data = f.data;
    nint i = 0;
    for (i = 0; i < len(data); i++) {
        if (data[i] == '\n') {
            s = string(data[(int)0..(int)i]);
            ok = true; 
            // move data
            i++;
            var n = len(data) - i;
            copy(data[(int)0..], data[(int)i..]);
            f.data = data[(int)0..(int)n];
            return ;

        }
    }
    if (f.atEOF && len(f.data) > 0) { 
        // EOF, return all we have
        s = string(data);
        f.data = f.data[(int)0..(int)0];
        ok = true;

    }
    return ;

}

private static (@string, bool) readLine(this ptr<file> _addr_f) {
    @string s = default;
    bool ok = default;
    ref file f = ref _addr_f.val;

    s, ok = f.getLineFromData();

    if (ok) {
        return ;
    }
    if (len(f.data) < cap(f.data)) {
        var ln = len(f.data);
        var (n, err) = io.ReadFull(f.file, f.data[(int)ln..(int)cap(f.data)]);
        if (n >= 0) {
            f.data = f.data[(int)0..(int)ln + n];
        }
        if (err == io.EOF || err == io.ErrUnexpectedEOF) {
            f.atEOF = true;
        }
    }
    s, ok = f.getLineFromData();
    return ;

}

private static (ptr<file>, error) open(@string name) {
    ptr<file> _p0 = default!;
    error _p0 = default!;

    var (fd, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new file(fd,make([]byte,0,64*1024),false)), error.As(null!)!);

}

private static (time.Time, long, error) stat(@string name) {
    time.Time mtime = default;
    long size = default;
    error err = default!;

    var (st, err) = os.Stat(name);
    if (err != null) {
        return (new time.Time(), 0, error.As(err)!);
    }
    return (st.ModTime(), st.Size(), error.As(null!)!);

}

// Count occurrences in s of any bytes in t.
private static nint countAnyByte(@string s, @string t) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (bytealg.IndexByteString(t, s[i]) >= 0) {
            n++;
        }
    }
    return n;

}

// Split s at any bytes in t.
private static slice<@string> splitAtBytes(@string s, @string t) {
    var a = make_slice<@string>(1 + countAnyByte(s, t));
    nint n = 0;
    nint last = 0;
    for (nint i = 0; i < len(s); i++) {
        if (bytealg.IndexByteString(t, s[i]) >= 0) {
            if (last < i) {
                a[n] = s[(int)last..(int)i];
                n++;
            }
            last = i + 1;
        }
    }
    if (last < len(s)) {
        a[n] = s[(int)last..];
        n++;
    }
    return a[(int)0..(int)n];

}

private static slice<@string> getFields(@string s) {
    return splitAtBytes(s, " \r\t\n");
}

// Bigger than we need, not too big to worry about overflow
private static readonly nuint big = 0xFFFFFF;

// Decimal to integer.
// Returns number, characters consumed, success.


// Decimal to integer.
// Returns number, characters consumed, success.
private static (nint, nint, bool) dtoi(@string s) {
    nint n = default;
    nint i = default;
    bool ok = default;

    n = 0;
    for (i = 0; i < len(s) && '0' <= s[i] && s[i] <= '9'; i++) {
        n = n * 10 + int(s[i] - '0');
        if (n >= big) {
            return (big, i, false);
        }
    }
    if (i == 0) {
        return (0, 0, false);
    }
    return (n, i, true);

}

// Hexadecimal to integer.
// Returns number, characters consumed, success.
private static (nint, nint, bool) xtoi(@string s) {
    nint n = default;
    nint i = default;
    bool ok = default;

    n = 0;
    for (i = 0; i < len(s); i++) {
        if ('0' <= s[i] && s[i] <= '9') {
            n *= 16;
            n += int(s[i] - '0');
        }
        else if ('a' <= s[i] && s[i] <= 'f') {
            n *= 16;
            n += int(s[i] - 'a') + 10;
        }
        else if ('A' <= s[i] && s[i] <= 'F') {
            n *= 16;
            n += int(s[i] - 'A') + 10;
        }
        else
 {
            break;
        }
        if (n >= big) {
            return (0, i, false);
        }
    }
    if (i == 0) {
        return (0, i, false);
    }
    return (n, i, true);

}

// xtoi2 converts the next two hex digits of s into a byte.
// If s is longer than 2 bytes then the third byte must be e.
// If the first two bytes of s are not hex digits or the third byte
// does not match e, false is returned.
private static (byte, bool) xtoi2(@string s, byte e) {
    byte _p0 = default;
    bool _p0 = default;

    if (len(s) > 2 && s[2] != e) {
        return (0, false);
    }
    var (n, ei, ok) = xtoi(s[..(int)2]);
    return (byte(n), ok && ei == 2);

}

// Convert i to a hexadecimal string. Leading zeros are not printed.
private static slice<byte> appendHex(slice<byte> dst, uint i) {
    if (i == 0) {
        return append(dst, '0');
    }
    for (nint j = 7; j >= 0; j--) {
        var v = i >> (int)(uint(j * 4));
        if (v > 0) {
            dst = append(dst, hexDigit[v & 0xf]);
        }
    }
    return dst;

}

// Number of occurrences of b in s.
private static nint count(@string s, byte b) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == b) {
            n++;
        }
    }
    return n;

}

// Index of rightmost occurrence of b in s.
private static nint last(@string s, byte b) {
    var i = len(s);
    i--;

    while (i >= 0) {
        if (s[i] == b) {
            break;
        i--;
        }
    }
    return i;

}

// lowerASCIIBytes makes x ASCII lowercase in-place.
private static void lowerASCIIBytes(slice<byte> x) {
    foreach (var (i, b) in x) {
        if ('A' <= b && b <= 'Z') {
            x[i] += 'a' - 'A';
        }
    }
}

// lowerASCII returns the ASCII lowercase version of b.
private static byte lowerASCII(byte b) {
    if ('A' <= b && b <= 'Z') {
        return b + ('a' - 'A');
    }
    return b;

}

// trimSpace returns x without any leading or trailing ASCII whitespace.
private static slice<byte> trimSpace(slice<byte> x) {
    while (len(x) > 0 && isSpace(x[0])) {
        x = x[(int)1..];
    }
    while (len(x) > 0 && isSpace(x[len(x) - 1])) {
        x = x[..(int)len(x) - 1];
    }
    return x;
}

// isSpace reports whether b is an ASCII space character.
private static bool isSpace(byte b) {
    return b == ' ' || b == '\t' || b == '\n' || b == '\r';
}

// removeComment returns line, removing any '#' byte and any following
// bytes.
private static slice<byte> removeComment(slice<byte> line) {
    {
        var i = bytealg.IndexByte(line, '#');

        if (i != -1) {
            return line[..(int)i];
        }
    }

    return line;

}

// foreachLine runs fn on each line of x.
// Each line (except for possibly the last) ends in '\n'.
// It returns the first non-nil error returned by fn.
private static error foreachLine(slice<byte> x, Func<slice<byte>, error> fn) {
    while (len(x) > 0) {
        var nl = bytealg.IndexByte(x, '\n');
        if (nl == -1) {
            return error.As(fn(x))!;
        }
        var line = x[..(int)nl + 1];
        x = x[(int)nl + 1..];
        {
            var err = fn(line);

            if (err != null) {
                return error.As(err)!;
            }

        }

    }
    return error.As(null!)!;

}

// foreachField runs fn on each non-empty run of non-space bytes in x.
// It returns the first non-nil error returned by fn.
private static error foreachField(slice<byte> x, Func<slice<byte>, error> fn) {
    x = trimSpace(x);
    while (len(x) > 0) {
        var sp = bytealg.IndexByte(x, ' ');
        if (sp == -1) {
            return error.As(fn(x))!;
        }
        {
            var field = trimSpace(x[..(int)sp]);

            if (len(field) > 0) {
                {
                    var err = fn(field);

                    if (err != null) {
                        return error.As(err)!;
                    }

                }

            }

        }

        x = trimSpace(x[(int)sp + 1..]);

    }
    return error.As(null!)!;

}

// stringsHasSuffix is strings.HasSuffix. It reports whether s ends in
// suffix.
private static bool stringsHasSuffix(@string s, @string suffix) {
    return len(s) >= len(suffix) && s[(int)len(s) - len(suffix)..] == suffix;
}

// stringsHasSuffixFold reports whether s ends in suffix,
// ASCII-case-insensitively.
private static bool stringsHasSuffixFold(@string s, @string suffix) {
    return len(s) >= len(suffix) && stringsEqualFold(s[(int)len(s) - len(suffix)..], suffix);
}

// stringsHasPrefix is strings.HasPrefix. It reports whether s begins with prefix.
private static bool stringsHasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s[..(int)len(prefix)] == prefix;
}

// stringsEqualFold is strings.EqualFold, ASCII only. It reports whether s and t
// are equal, ASCII-case-insensitively.
private static bool stringsEqualFold(@string s, @string t) {
    if (len(s) != len(t)) {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        if (lowerASCII(s[i]) != lowerASCII(t[i])) {
            return false;
        }
    }
    return true;

}

private static (slice<byte>, error) readFull(io.Reader r) {
    slice<byte> all = default;
    error err = default!;

    var buf = make_slice<byte>(1024);
    while (true) {
        var (n, err) = r.Read(buf);
        all = append(all, buf[..(int)n]);
        if (err == io.EOF) {
            return (all, error.As(null!)!);
        }
        if (err != null) {
            return (null, error.As(err)!);
        }
    }

}

// goDebugString returns the value of the named GODEBUG key.
// GODEBUG is of the form "key=val,key2=val2"
private static @string goDebugString(@string key) {
    var s = os.Getenv("GODEBUG");
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s) - len(key) - 1; i++) {
            if (i > 0 && s[i - 1] != ',') {
                continue;
            }
            var afterKey = s[(int)i + len(key)..];
            if (afterKey[0] != '=' || s[(int)i..(int)i + len(key)] != key) {
                continue;
            }
            var val = afterKey[(int)1..];
            {
                nint i__prev2 = i;

                foreach (var (__i, __b) in val) {
                    i = __i;
                    b = __b;
                    if (b == ',') {
                        return val[..(int)i];
                    }
                }

                i = i__prev2;
            }

            return val;

        }

        i = i__prev1;
    }
    return "";

}

} // end net_package
