// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Simple file i/o and string manipulation, to avoid
// depending on strconv and bufio and strings.

// package net -- go2cs converted at 2020 October 09 04:52:10 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\parse.go
using bytealg = go.@internal.bytealg_package;
using io = go.io_package;
using os = go.os_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        private partial struct file
        {
            public ptr<os.File> file;
            public slice<byte> data;
            public bool atEOF;
        }

        private static void close(this ptr<file> _addr_f)
        {
            ref file f = ref _addr_f.val;

            f.file.Close();
        }

        private static (@string, bool) getLineFromData(this ptr<file> _addr_f)
        {
            @string s = default;
            bool ok = default;
            ref file f = ref _addr_f.val;

            var data = f.data;
            long i = 0L;
            for (i = 0L; i < len(data); i++)
            {
                if (data[i] == '\n')
                {
                    s = string(data[0L..i]);
                    ok = true; 
                    // move data
                    i++;
                    var n = len(data) - i;
                    copy(data[0L..], data[i..]);
                    f.data = data[0L..n];
                    return ;

                }

            }

            if (f.atEOF && len(f.data) > 0L)
            { 
                // EOF, return all we have
                s = string(data);
                f.data = f.data[0L..0L];
                ok = true;

            }

            return ;

        }

        private static (@string, bool) readLine(this ptr<file> _addr_f)
        {
            @string s = default;
            bool ok = default;
            ref file f = ref _addr_f.val;

            s, ok = f.getLineFromData();

            if (ok)
            {
                return ;
            }

            if (len(f.data) < cap(f.data))
            {
                var ln = len(f.data);
                var (n, err) = io.ReadFull(f.file, f.data[ln..cap(f.data)]);
                if (n >= 0L)
                {
                    f.data = f.data[0L..ln + n];
                }

                if (err == io.EOF || err == io.ErrUnexpectedEOF)
                {
                    f.atEOF = true;
                }

            }

            s, ok = f.getLineFromData();
            return ;

        }

        private static (ptr<file>, error) open(@string name)
        {
            ptr<file> _p0 = default!;
            error _p0 = default!;

            var (fd, err) = os.Open(name);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new file(fd,make([]byte,0,64*1024),false)), error.As(null!)!);

        }

        private static (time.Time, long, error) stat(@string name)
        {
            time.Time mtime = default;
            long size = default;
            error err = default!;

            var (st, err) = os.Stat(name);
            if (err != null)
            {
                return (new time.Time(), 0L, error.As(err)!);
            }

            return (st.ModTime(), st.Size(), error.As(null!)!);

        }

        // Count occurrences in s of any bytes in t.
        private static long countAnyByte(@string s, @string t)
        {
            long n = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                if (bytealg.IndexByteString(t, s[i]) >= 0L)
                {
                    n++;
                }

            }

            return n;

        }

        // Split s at any bytes in t.
        private static slice<@string> splitAtBytes(@string s, @string t)
        {
            var a = make_slice<@string>(1L + countAnyByte(s, t));
            long n = 0L;
            long last = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                if (bytealg.IndexByteString(t, s[i]) >= 0L)
                {
                    if (last < i)
                    {
                        a[n] = s[last..i];
                        n++;
                    }

                    last = i + 1L;

                }

            }

            if (last < len(s))
            {
                a[n] = s[last..];
                n++;
            }

            return a[0L..n];

        }

        private static slice<@string> getFields(@string s)
        {
            return splitAtBytes(s, " \r\t\n");
        }

        // Bigger than we need, not too big to worry about overflow
        private static readonly ulong big = (ulong)0xFFFFFFUL;

        // Decimal to integer.
        // Returns number, characters consumed, success.


        // Decimal to integer.
        // Returns number, characters consumed, success.
        private static (long, long, bool) dtoi(@string s)
        {
            long n = default;
            long i = default;
            bool ok = default;

            n = 0L;
            for (i = 0L; i < len(s) && '0' <= s[i] && s[i] <= '9'; i++)
            {
                n = n * 10L + int(s[i] - '0');
                if (n >= big)
                {
                    return (big, i, false);
                }

            }

            if (i == 0L)
            {
                return (0L, 0L, false);
            }

            return (n, i, true);

        }

        // Hexadecimal to integer.
        // Returns number, characters consumed, success.
        private static (long, long, bool) xtoi(@string s)
        {
            long n = default;
            long i = default;
            bool ok = default;

            n = 0L;
            for (i = 0L; i < len(s); i++)
            {
                if ('0' <= s[i] && s[i] <= '9')
                {
                    n *= 16L;
                    n += int(s[i] - '0');
                }
                else if ('a' <= s[i] && s[i] <= 'f')
                {
                    n *= 16L;
                    n += int(s[i] - 'a') + 10L;
                }
                else if ('A' <= s[i] && s[i] <= 'F')
                {
                    n *= 16L;
                    n += int(s[i] - 'A') + 10L;
                }
                else
                {
                    break;
                }

                if (n >= big)
                {
                    return (0L, i, false);
                }

            }

            if (i == 0L)
            {
                return (0L, i, false);
            }

            return (n, i, true);

        }

        // xtoi2 converts the next two hex digits of s into a byte.
        // If s is longer than 2 bytes then the third byte must be e.
        // If the first two bytes of s are not hex digits or the third byte
        // does not match e, false is returned.
        private static (byte, bool) xtoi2(@string s, byte e)
        {
            byte _p0 = default;
            bool _p0 = default;

            if (len(s) > 2L && s[2L] != e)
            {
                return (0L, false);
            }

            var (n, ei, ok) = xtoi(s[..2L]);
            return (byte(n), ok && ei == 2L);

        }

        // Convert integer to decimal string.
        private static @string itoa(long val)
        {
            if (val < 0L)
            {
                return "-" + uitoa(uint(-val));
            }

            return uitoa(uint(val));

        }

        // Convert unsigned integer to decimal string.
        private static @string uitoa(ulong val)
        {
            if (val == 0L)
            { // avoid string allocation
                return "0";

            }

            array<byte> buf = new array<byte>(20L); // big enough for 64bit value base 10
            var i = len(buf) - 1L;
            while (val >= 10L)
            {
                var q = val / 10L;
                buf[i] = byte('0' + val - q * 10L);
                i--;
                val = q;
            } 
            // val < 10
 
            // val < 10
            buf[i] = byte('0' + val);
            return string(buf[i..]);

        }

        // Convert i to a hexadecimal string. Leading zeros are not printed.
        private static slice<byte> appendHex(slice<byte> dst, uint i)
        {
            if (i == 0L)
            {
                return append(dst, '0');
            }

            for (long j = 7L; j >= 0L; j--)
            {
                var v = i >> (int)(uint(j * 4L));
                if (v > 0L)
                {
                    dst = append(dst, hexDigit[v & 0xfUL]);
                }

            }

            return dst;

        }

        // Number of occurrences of b in s.
        private static long count(@string s, byte b)
        {
            long n = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == b)
                {
                    n++;
                }

            }

            return n;

        }

        // Index of rightmost occurrence of b in s.
        private static long last(@string s, byte b)
        {
            var i = len(s);
            i--;

            while (i >= 0L)
            {
                if (s[i] == b)
                {
                    break;
                i--;
                }

            }

            return i;

        }

        // lowerASCIIBytes makes x ASCII lowercase in-place.
        private static void lowerASCIIBytes(slice<byte> x)
        {
            foreach (var (i, b) in x)
            {
                if ('A' <= b && b <= 'Z')
                {
                    x[i] += 'a' - 'A';
                }

            }

        }

        // lowerASCII returns the ASCII lowercase version of b.
        private static byte lowerASCII(byte b)
        {
            if ('A' <= b && b <= 'Z')
            {
                return b + ('a' - 'A');
            }

            return b;

        }

        // trimSpace returns x without any leading or trailing ASCII whitespace.
        private static slice<byte> trimSpace(slice<byte> x)
        {
            while (len(x) > 0L && isSpace(x[0L]))
            {
                x = x[1L..];
            }

            while (len(x) > 0L && isSpace(x[len(x) - 1L]))
            {
                x = x[..len(x) - 1L];
            }

            return x;

        }

        // isSpace reports whether b is an ASCII space character.
        private static bool isSpace(byte b)
        {
            return b == ' ' || b == '\t' || b == '\n' || b == '\r';
        }

        // removeComment returns line, removing any '#' byte and any following
        // bytes.
        private static slice<byte> removeComment(slice<byte> line)
        {
            {
                var i = bytealg.IndexByte(line, '#');

                if (i != -1L)
                {
                    return line[..i];
                }

            }

            return line;

        }

        // foreachLine runs fn on each line of x.
        // Each line (except for possibly the last) ends in '\n'.
        // It returns the first non-nil error returned by fn.
        private static error foreachLine(slice<byte> x, Func<slice<byte>, error> fn)
        {
            while (len(x) > 0L)
            {
                var nl = bytealg.IndexByte(x, '\n');
                if (nl == -1L)
                {
                    return error.As(fn(x))!;
                }

                var line = x[..nl + 1L];
                x = x[nl + 1L..];
                {
                    var err = fn(line);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }

            return error.As(null!)!;

        }

        // foreachField runs fn on each non-empty run of non-space bytes in x.
        // It returns the first non-nil error returned by fn.
        private static error foreachField(slice<byte> x, Func<slice<byte>, error> fn)
        {
            x = trimSpace(x);
            while (len(x) > 0L)
            {
                var sp = bytealg.IndexByte(x, ' ');
                if (sp == -1L)
                {
                    return error.As(fn(x))!;
                }

                {
                    var field = trimSpace(x[..sp]);

                    if (len(field) > 0L)
                    {
                        {
                            var err = fn(field);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                        }

                    }

                }

                x = trimSpace(x[sp + 1L..]);

            }

            return error.As(null!)!;

        }

        // stringsHasSuffix is strings.HasSuffix. It reports whether s ends in
        // suffix.
        private static bool stringsHasSuffix(@string s, @string suffix)
        {
            return len(s) >= len(suffix) && s[len(s) - len(suffix)..] == suffix;
        }

        // stringsHasSuffixFold reports whether s ends in suffix,
        // ASCII-case-insensitively.
        private static bool stringsHasSuffixFold(@string s, @string suffix)
        {
            return len(s) >= len(suffix) && stringsEqualFold(s[len(s) - len(suffix)..], suffix);
        }

        // stringsHasPrefix is strings.HasPrefix. It reports whether s begins with prefix.
        private static bool stringsHasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[..len(prefix)] == prefix;
        }

        // stringsEqualFold is strings.EqualFold, ASCII only. It reports whether s and t
        // are equal, ASCII-case-insensitively.
        private static bool stringsEqualFold(@string s, @string t)
        {
            if (len(s) != len(t))
            {
                return false;
            }

            for (long i = 0L; i < len(s); i++)
            {
                if (lowerASCII(s[i]) != lowerASCII(t[i]))
                {
                    return false;
                }

            }

            return true;

        }

        private static (slice<byte>, error) readFull(io.Reader r)
        {
            slice<byte> all = default;
            error err = default!;

            var buf = make_slice<byte>(1024L);
            while (true)
            {
                var (n, err) = r.Read(buf);
                all = append(all, buf[..n]);
                if (err == io.EOF)
                {
                    return (all, error.As(null!)!);
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


        }

        // goDebugString returns the value of the named GODEBUG key.
        // GODEBUG is of the form "key=val,key2=val2"
        private static @string goDebugString(@string key)
        {
            var s = os.Getenv("GODEBUG");
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s) - len(key) - 1L; i++)
                {
                    if (i > 0L && s[i - 1L] != ',')
                    {
                        continue;
                    }

                    var afterKey = s[i + len(key)..];
                    if (afterKey[0L] != '=' || s[i..i + len(key)] != key)
                    {
                        continue;
                    }

                    var val = afterKey[1L..];
                    {
                        long i__prev2 = i;

                        foreach (var (__i, __b) in val)
                        {
                            i = __i;
                            b = __b;
                            if (b == ',')
                            {
                                return val[..i];
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
    }
}
