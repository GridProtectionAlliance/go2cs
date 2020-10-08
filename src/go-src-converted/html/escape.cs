// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package html provides functions for escaping and unescaping HTML text.
// package html -- go2cs converted at 2020 October 08 03:42:17 UTC
// import "html" ==> using html = go.html_package
// Original source: C:\Go\src\html\escape.go
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class html_package
    {
        // These replacements permit compatibility with old numeric entities that
        // assumed Windows-1252 encoding.
        // https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state
        private static array<int> replacementTable = new array<int>(new int[] { '\u20AC', '\u0081', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020', '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u008D', '\u017D', '\u008F', '\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u009D', '\u017E', '\u0178' });

        // unescapeEntity reads an entity like "&lt;" from b[src:] and writes the
        // corresponding "<" to b[dst:], returning the incremented dst and src cursors.
        // Precondition: b[src] == '&' && dst <= src.
        private static (long, long) unescapeEntity(slice<byte> b, long dst, long src)
        {
            long dst1 = default;
            long src1 = default;

            const var attribute = (var)false; 

            // http://www.whatwg.org/specs/web-apps/current-work/multipage/tokenization.html#consume-a-character-reference

            // i starts at 1 because we already know that s[0] == '&'.
 

            // http://www.whatwg.org/specs/web-apps/current-work/multipage/tokenization.html#consume-a-character-reference

            // i starts at 1 because we already know that s[0] == '&'.
            long i = 1L;
            var s = b[src..];

            if (len(s) <= 1L)
            {
                b[dst] = b[src];
                return (dst + 1L, src + 1L);
            }

            if (s[i] == '#')
            {
                if (len(s) <= 3L)
                { // We need to have at least "&#.".
                    b[dst] = b[src];
                    return (dst + 1L, src + 1L);

                }

                i++;
                var c = s[i];
                var hex = false;
                if (c == 'x' || c == 'X')
                {
                    hex = true;
                    i++;
                }

                char x = '\x00';
                while (i < len(s))
                {
                    c = s[i];
                    i++;
                    if (hex)
                    {
                        if ('0' <= c && c <= '9')
                        {
                            x = 16L * x + rune(c) - '0';
                            continue;
                        }
                        else if ('a' <= c && c <= 'f')
                        {
                            x = 16L * x + rune(c) - 'a' + 10L;
                            continue;
                        }
                        else if ('A' <= c && c <= 'F')
                        {
                            x = 16L * x + rune(c) - 'A' + 10L;
                            continue;
                        }

                    }
                    else if ('0' <= c && c <= '9')
                    {
                        x = 10L * x + rune(c) - '0';
                        continue;
                    }

                    if (c != ';')
                    {
                        i--;
                    }

                    break;

                }


                if (i <= 3L)
                { // No characters matched.
                    b[dst] = b[src];
                    return (dst + 1L, src + 1L);

                }

                if (0x80UL <= x && x <= 0x9FUL)
                { 
                    // Replace characters from Windows-1252 with UTF-8 equivalents.
                    x = replacementTable[x - 0x80UL];

                }
                else if (x == 0L || (0xD800UL <= x && x <= 0xDFFFUL) || x > 0x10FFFFUL)
                { 
                    // Replace invalid characters with the replacement character.
                    x = '\uFFFD';

                }

                return (dst + utf8.EncodeRune(b[dst..], x), src + i);

            } 

            // Consume the maximum number of characters possible, with the
            // consumed characters matching one of the named references.
            while (i < len(s))
            {
                c = s[i];
                i++; 
                // Lower-cased characters are more common in entities, so we check for them first.
                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9')
                {
                    continue;
                }

                if (c != ';')
                {
                    i--;
                }

                break;

            }


            var entityName = s[1L..i];
            if (len(entityName) == 0L)
            { 
                // No-op.
            }
            else if (attribute && entityName[len(entityName) - 1L] != ';' && len(s) > i && s[i] == '=')
            { 
                // No-op.
            }            {
                char x__prev3 = x;

                x = entity[string(entityName)];


                else if (x != 0L)
                {
                    return (dst + utf8.EncodeRune(b[dst..], x), src + i);
                }                {
                    char x__prev4 = x;

                    x = entity2[string(entityName)];


                    else if (x[0L] != 0L)
                    {
                        var dst1 = dst + utf8.EncodeRune(b[dst..], x[0L]);
                        return (dst1 + utf8.EncodeRune(b[dst1..], x[1L]), src + i);
                    }
                    else if (!attribute)
                    {
                        var maxLen = len(entityName) - 1L;
                        if (maxLen > longestEntityWithoutSemicolon)
                        {
                            maxLen = longestEntityWithoutSemicolon;
                        }

                        for (var j = maxLen; j > 1L; j--)
                        {
                            {
                                char x__prev6 = x;

                                x = entity[string(entityName[..j])];

                                if (x != 0L)
                                {
                                    return (dst + utf8.EncodeRune(b[dst..], x), src + j + 1L);
                                }

                                x = x__prev6;

                            }

                        }


                    }


                    x = x__prev4;

                }



                x = x__prev3;

            }


            dst1 = dst + i;
            src1 = src + i;
            copy(b[dst..dst1], b[src..src1]);
            return (dst1, src1);

        }

        private static var htmlEscaper = strings.NewReplacer("&", "&amp;", "\'", "&#39;", "<", "&lt;", ">", "&gt;", "\"", "&#34;");

        // EscapeString escapes special characters like "<" to become "&lt;". It
        // escapes only five such characters: <, >, &, ' and ".
        // UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
        // always true.
        public static @string EscapeString(@string s)
        {
            return htmlEscaper.Replace(s);
        }

        // UnescapeString unescapes entities like "&lt;" to become "<". It unescapes a
        // larger range of entities than EscapeString escapes. For example, "&aacute;"
        // unescapes to "á", as does "&#225;" and "&#xE1;".
        // UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
        // always true.
        public static @string UnescapeString(@string s)
        {
            populateMapsOnce.Do(populateMaps);
            var i = strings.IndexByte(s, '&');

            if (i < 0L)
            {
                return s;
            }

            slice<byte> b = (slice<byte>)s;
            var (dst, src) = unescapeEntity(b, i, i);
            while (len(s[src..]) > 0L)
            {
                if (s[src] == '&')
                {
                    i = 0L;
                }
                else
                {
                    i = strings.IndexByte(s[src..], '&');
                }

                if (i < 0L)
                {
                    dst += copy(b[dst..], s[src..]);
                    break;
                }

                if (i > 0L)
                {
                    copy(b[dst..], s[src..src + i]);
                }

                dst, src = unescapeEntity(b, dst + i, src + i);

            }

            return string(b[..dst]);

        }
    }
}
