// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package html provides functions for escaping and unescaping HTML text.
// package html -- go2cs converted at 2022 March 06 22:24:21 UTC
// import "html" ==> using html = go.html_package
// Original source: C:\Program Files\Go\src\html\escape.go
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

namespace go;

public static partial class html_package {

    // These replacements permit compatibility with old numeric entities that
    // assumed Windows-1252 encoding.
    // https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-end-state
private static array<int> replacementTable = new array<int>(new int[] { '\u20AC', '\u0081', '\u201A', '\u0192', '\u201E', '\u2026', '\u2020', '\u2021', '\u02C6', '\u2030', '\u0160', '\u2039', '\u0152', '\u008D', '\u017D', '\u008F', '\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', '\u2013', '\u2014', '\u02DC', '\u2122', '\u0161', '\u203A', '\u0153', '\u009D', '\u017E', '\u0178' });

// unescapeEntity reads an entity like "&lt;" from b[src:] and writes the
// corresponding "<" to b[dst:], returning the incremented dst and src cursors.
// Precondition: b[src] == '&' && dst <= src.
private static (nint, nint) unescapeEntity(slice<byte> b, nint dst, nint src) {
    nint dst1 = default;
    nint src1 = default;

    const var attribute = false; 

    // http://www.whatwg.org/specs/web-apps/current-work/multipage/tokenization.html#consume-a-character-reference

    // i starts at 1 because we already know that s[0] == '&'.
 

    // http://www.whatwg.org/specs/web-apps/current-work/multipage/tokenization.html#consume-a-character-reference

    // i starts at 1 because we already know that s[0] == '&'.
    nint i = 1;
    var s = b[(int)src..];

    if (len(s) <= 1) {
        b[dst] = b[src];
        return (dst + 1, src + 1);
    }
    if (s[i] == '#') {
        if (len(s) <= 3) { // We need to have at least "&#.".
            b[dst] = b[src];
            return (dst + 1, src + 1);

        }
        i++;
        var c = s[i];
        var hex = false;
        if (c == 'x' || c == 'X') {
            hex = true;
            i++;
        }
        char x = '\x00';
        while (i < len(s)) {
            c = s[i];
            i++;
            if (hex) {
                if ('0' <= c && c <= '9') {
                    x = 16 * x + rune(c) - '0';
                    continue;
                }
                else if ('a' <= c && c <= 'f') {
                    x = 16 * x + rune(c) - 'a' + 10;
                    continue;
                }
                else if ('A' <= c && c <= 'F') {
                    x = 16 * x + rune(c) - 'A' + 10;
                    continue;
                }

            }
            else if ('0' <= c && c <= '9') {
                x = 10 * x + rune(c) - '0';
                continue;
            }

            if (c != ';') {
                i--;
            }

            break;

        }

        if (i <= 3) { // No characters matched.
            b[dst] = b[src];
            return (dst + 1, src + 1);

        }
        if (0x80 <= x && x <= 0x9F) { 
            // Replace characters from Windows-1252 with UTF-8 equivalents.
            x = replacementTable[x - 0x80];

        }
        else if (x == 0 || (0xD800 <= x && x <= 0xDFFF) || x > 0x10FFFF) { 
            // Replace invalid characters with the replacement character.
            x = '\uFFFD';

        }
        return (dst + utf8.EncodeRune(b[(int)dst..], x), src + i);

    }
    while (i < len(s)) {
        c = s[i];
        i++; 
        // Lower-cased characters are more common in entities, so we check for them first.
        if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9') {
            continue;
        }
        if (c != ';') {
            i--;
        }
        break;

    }

    var entityName = s[(int)1..(int)i];
    if (len(entityName) == 0) { 
        // No-op.
    }
    else if (attribute && entityName[len(entityName) - 1] != ';' && len(s) > i && s[i] == '=') { 
        // No-op.
    }    {
        char x__prev3 = x;

        x = entity[string(entityName)];


        else if (x != 0) {
            return (dst + utf8.EncodeRune(b[(int)dst..], x), src + i);
        }        {
            char x__prev4 = x;

            x = entity2[string(entityName)];


            else if (x[0] != 0) {
                var dst1 = dst + utf8.EncodeRune(b[(int)dst..], x[0]);
                return (dst1 + utf8.EncodeRune(b[(int)dst1..], x[1]), src + i);
            }
            else if (!attribute) {
                var maxLen = len(entityName) - 1;
                if (maxLen > longestEntityWithoutSemicolon) {
                    maxLen = longestEntityWithoutSemicolon;
                }
                for (var j = maxLen; j > 1; j--) {
                    {
                        char x__prev6 = x;

                        x = entity[string(entityName[..(int)j])];

                        if (x != 0) {
                            return (dst + utf8.EncodeRune(b[(int)dst..], x), src + j + 1);
                        }

                        x = x__prev6;

                    }

                }


            }


            x = x__prev4;

        }



        x = x__prev3;

    }


    (dst1, src1) = (dst + i, src + i);    copy(b[(int)dst..(int)dst1], b[(int)src..(int)src1]);
    return (dst1, src1);

}

private static var htmlEscaper = strings.NewReplacer("&", "&amp;", "\'", "&#39;", "<", "&lt;", ">", "&gt;", "\"", "&#34;");

// EscapeString escapes special characters like "<" to become "&lt;". It
// escapes only five such characters: <, >, &, ' and ".
// UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
// always true.
public static @string EscapeString(@string s) {
    return htmlEscaper.Replace(s);
}

// UnescapeString unescapes entities like "&lt;" to become "<". It unescapes a
// larger range of entities than EscapeString escapes. For example, "&aacute;"
// unescapes to "á", as does "&#225;" and "&#xE1;".
// UnescapeString(EscapeString(s)) == s always holds, but the converse isn't
// always true.
public static @string UnescapeString(@string s) {
    populateMapsOnce.Do(populateMaps);
    var i = strings.IndexByte(s, '&');

    if (i < 0) {
        return s;
    }
    slice<byte> b = (slice<byte>)s;
    var (dst, src) = unescapeEntity(b, i, i);
    while (len(s[(int)src..]) > 0) {
        if (s[src] == '&') {
            i = 0;
        }
        else
 {
            i = strings.IndexByte(s[(int)src..], '&');
        }
        if (i < 0) {
            dst += copy(b[(int)dst..], s[(int)src..]);
            break;
        }
        if (i > 0) {
            copy(b[(int)dst..], s[(int)src..(int)src + i]);
        }
        dst, src = unescapeEntity(b, dst + i, src + i);

    }
    return string(b[..(int)dst]);

}

} // end html_package
