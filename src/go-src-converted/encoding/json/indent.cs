// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package json -- go2cs converted at 2020 October 08 03:42:53 UTC
// import "encoding/json" ==> using json = go.encoding.json_package
// Original source: C:\Go\src\encoding\json\indent.go
using bytes = go.bytes_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class json_package
    {
        // Compact appends to dst the JSON-encoded src with
        // insignificant space characters elided.
        public static error Compact(ptr<bytes.Buffer> _addr_dst, slice<byte> src)
        {
            ref bytes.Buffer dst = ref _addr_dst.val;

            return error.As(compact(_addr_dst, src, false))!;
        }

        private static error compact(ptr<bytes.Buffer> _addr_dst, slice<byte> src, bool escape) => func((defer, _, __) =>
        {
            ref bytes.Buffer dst = ref _addr_dst.val;

            var origLen = dst.Len();
            var scan = newScanner();
            defer(freeScanner(scan));
            long start = 0L;
            foreach (var (i, c) in src)
            {
                if (escape && (c == '<' || c == '>' || c == '&'))
                {
                    if (start < i)
                    {
                        dst.Write(src[start..i]);
                    }

                    dst.WriteString("\\u00");
                    dst.WriteByte(hex[c >> (int)(4L)]);
                    dst.WriteByte(hex[c & 0xFUL]);
                    start = i + 1L;

                } 
                // Convert U+2028 and U+2029 (E2 80 A8 and E2 80 A9).
                if (escape && c == 0xE2UL && i + 2L < len(src) && src[i + 1L] == 0x80UL && src[i + 2L] & ~1L == 0xA8UL)
                {
                    if (start < i)
                    {
                        dst.Write(src[start..i]);
                    }

                    dst.WriteString("\\u202");
                    dst.WriteByte(hex[src[i + 2L] & 0xFUL]);
                    start = i + 3L;

                }

                var v = scan.step(scan, c);
                if (v >= scanSkipSpace)
                {
                    if (v == scanError)
                    {
                        break;
                    }

                    if (start < i)
                    {
                        dst.Write(src[start..i]);
                    }

                    start = i + 1L;

                }

            }
            if (scan.eof() == scanError)
            {
                dst.Truncate(origLen);
                return error.As(scan.err)!;
            }

            if (start < len(src))
            {
                dst.Write(src[start..]);
            }

            return error.As(null!)!;

        });

        private static void newline(ptr<bytes.Buffer> _addr_dst, @string prefix, @string indent, long depth)
        {
            ref bytes.Buffer dst = ref _addr_dst.val;

            dst.WriteByte('\n');
            dst.WriteString(prefix);
            for (long i = 0L; i < depth; i++)
            {
                dst.WriteString(indent);
            }


        }

        // Indent appends to dst an indented form of the JSON-encoded src.
        // Each element in a JSON object or array begins on a new,
        // indented line beginning with prefix followed by one or more
        // copies of indent according to the indentation nesting.
        // The data appended to dst does not begin with the prefix nor
        // any indentation, to make it easier to embed inside other formatted JSON data.
        // Although leading space characters (space, tab, carriage return, newline)
        // at the beginning of src are dropped, trailing space characters
        // at the end of src are preserved and copied to dst.
        // For example, if src has no trailing spaces, neither will dst;
        // if src ends in a trailing newline, so will dst.
        public static error Indent(ptr<bytes.Buffer> _addr_dst, slice<byte> src, @string prefix, @string indent) => func((defer, _, __) =>
        {
            ref bytes.Buffer dst = ref _addr_dst.val;

            var origLen = dst.Len();
            var scan = newScanner();
            defer(freeScanner(scan));
            var needIndent = false;
            long depth = 0L;
            foreach (var (_, c) in src)
            {
                scan.bytes++;
                var v = scan.step(scan, c);
                if (v == scanSkipSpace)
                {
                    continue;
                }

                if (v == scanError)
                {
                    break;
                }

                if (needIndent && v != scanEndObject && v != scanEndArray)
                {
                    needIndent = false;
                    depth++;
                    newline(_addr_dst, prefix, indent, depth);
                } 

                // Emit semantically uninteresting bytes
                // (in particular, punctuation in strings) unmodified.
                if (v == scanContinue)
                {
                    dst.WriteByte(c);
                    continue;
                } 

                // Add spacing around real punctuation.
                switch (c)
                {
                    case '{': 
                        // delay indent so that empty object and array are formatted as {} and [].

                    case '[': 
                        // delay indent so that empty object and array are formatted as {} and [].
                        needIndent = true;
                        dst.WriteByte(c);
                        break;
                    case ',': 
                        dst.WriteByte(c);
                        newline(_addr_dst, prefix, indent, depth);
                        break;
                    case ':': 
                        dst.WriteByte(c);
                        dst.WriteByte(' ');
                        break;
                    case '}': 

                    case ']': 
                        if (needIndent)
                        { 
                            // suppress indent in empty object/array
                            needIndent = false;

                        }
                        else
                        {
                            depth--;
                            newline(_addr_dst, prefix, indent, depth);
                        }

                        dst.WriteByte(c);
                        break;
                    default: 
                        dst.WriteByte(c);
                        break;
                }

            }
            if (scan.eof() == scanError)
            {
                dst.Truncate(origLen);
                return error.As(scan.err)!;
            }

            return error.As(null!)!;

        });
    }
}}
