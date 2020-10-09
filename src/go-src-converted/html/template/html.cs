// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 October 09 04:59:36 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\html.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // htmlNospaceEscaper escapes for inclusion in unquoted attribute values.
        private static @string htmlNospaceEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeHTML)
            {
                return htmlReplacer(stripTags(s), htmlNospaceNormReplacementTable, false);
            }
            return htmlReplacer(s, htmlNospaceReplacementTable, false);

        }

        // attrEscaper escapes for inclusion in quoted attribute values.
        private static @string attrEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeHTML)
            {
                return htmlReplacer(stripTags(s), htmlNormReplacementTable, true);
            }

            return htmlReplacer(s, htmlReplacementTable, true);

        }

        // rcdataEscaper escapes for inclusion in an RCDATA element body.
        private static @string rcdataEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeHTML)
            {
                return htmlReplacer(s, htmlNormReplacementTable, true);
            }

            return htmlReplacer(s, htmlReplacementTable, true);

        }

        // htmlEscaper escapes for inclusion in HTML text.
        private static @string htmlEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeHTML)
            {
                return s;
            }

            return htmlReplacer(s, htmlReplacementTable, true);

        }

        // htmlReplacementTable contains the runes that need to be escaped
        // inside a quoted attribute value or in a text node.
        private static @string htmlReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, "\uFFFD"), ('"', "&#34;"), ('&', "&amp;"), ('\'', "&#39;"), ('+', "&#43;"), ('<', "&lt;"), ('>', "&gt;")));

        // htmlNormReplacementTable is like htmlReplacementTable but without '&' to
        // avoid over-encoding existing entities.
        private static @string htmlNormReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, "\uFFFD"), ('"', "&#34;"), ('\'', "&#39;"), ('+', "&#43;"), ('<', "&lt;"), ('>', "&gt;")));

        // htmlNospaceReplacementTable contains the runes that need to be escaped
        // inside an unquoted attribute value.
        // The set of runes escaped is the union of the HTML specials and
        // those determined by running the JS below in browsers:
        // <div id=d></div>
        // <script>(function () {
        // var a = [], d = document.getElementById("d"), i, c, s;
        // for (i = 0; i < 0x10000; ++i) {
        //   c = String.fromCharCode(i);
        //   d.innerHTML = "<span title=" + c + "lt" + c + "></span>"
        //   s = d.getElementsByTagName("SPAN")[0];
        //   if (!s || s.title !== c + "lt" + c) { a.push(i.toString(16)); }
        // }
        // document.write(a.join(", "));
        // })()</script>
        private static @string htmlNospaceReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, "&#xfffd;"), ('\t', "&#9;"), ('\n', "&#10;"), ('\v', "&#11;"), ('\f', "&#12;"), ('\r', "&#13;"), (' ', "&#32;"), ('"', "&#34;"), ('&', "&amp;"), ('\'', "&#39;"), ('+', "&#43;"), ('<', "&lt;"), ('=', "&#61;"), ('>', "&gt;"), ('`', "&#96;")));

        // htmlNospaceNormReplacementTable is like htmlNospaceReplacementTable but
        // without '&' to avoid over-encoding existing entities.
        private static @string htmlNospaceNormReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, "&#xfffd;"), ('\t', "&#9;"), ('\n', "&#10;"), ('\v', "&#11;"), ('\f', "&#12;"), ('\r', "&#13;"), (' ', "&#32;"), ('"', "&#34;"), ('\'', "&#39;"), ('+', "&#43;"), ('<', "&lt;"), ('=', "&#61;"), ('>', "&gt;"), ('`', "&#96;")));

        // htmlReplacer returns s with runes replaced according to replacementTable
        // and when badRunes is true, certain bad runes are allowed through unescaped.
        private static @string htmlReplacer(@string s, slice<@string> replacementTable, bool badRunes)
        {
            long written = 0L;
            ptr<object> b = @new<strings.Builder>();
            var r = rune(0L);
            long w = 0L;
            {
                long i = 0L;

                while (i < len(s))
                { 
                    // Cannot use 'for range s' because we need to preserve the width
                    // of the runes in the input. If we see a decoding error, the input
                    // width will not be utf8.Runelen(r) and we will overrun the buffer.
                    r, w = utf8.DecodeRuneInString(s[i..]);
                    if (int(r) < len(replacementTable))
                    {
                        {
                            var repl = replacementTable[r];

                            if (len(repl) != 0L)
                            {
                                if (written == 0L)
                                {
                                    b.Grow(len(s));
                    i += w;
                                }

                                b.WriteString(s[written..i]);
                                b.WriteString(repl);
                                written = i + w;

                            }

                        }

                    }
                    else if (badRunes)
                    { 
                        // No-op.
                        // IE does not allow these ranges in unquoted attrs.
                    }
                    else if (0xfdd0UL <= r && r <= 0xfdefUL || 0xfff0UL <= r && r <= 0xffffUL)
                    {
                        if (written == 0L)
                        {
                            b.Grow(len(s));
                        }

                        fmt.Fprintf(b, "%s&#x%x;", s[written..i], r);
                        written = i + w;

                    }

                }

            }
            if (written == 0L)
            {
                return s;
            }

            b.WriteString(s[written..]);
            return b.String();

        }

        // stripTags takes a snippet of HTML and returns only the text content.
        // For example, `<b>&iexcl;Hi!</b> <script>...</script>` -> `&iexcl;Hi! `.
        private static @string stripTags(@string html)
        {
            bytes.Buffer b = default;
            slice<byte> s = (slice<byte>)html;
            context c = new context();
            long i = 0L;
            var allText = true; 
            // Using the transition funcs helps us avoid mangling
            // `<div title="1>2">` or `I <3 Ponies!`.
            while (i != len(s))
            {
                if (c.delim == delimNone)
                {
                    var st = c.state; 
                    // Use RCDATA instead of parsing into JS or CSS styles.
                    if (c.element != elementNone && !isInTag(st))
                    {
                        st = stateRCDATA;
                    }

                    var (d, nread) = transitionFunc[st](c, s[i..]);
                    var i1 = i + nread;
                    if (c.state == stateText || c.state == stateRCDATA)
                    { 
                        // Emit text up to the start of the tag or comment.
                        var j = i1;
                        if (d.state != c.state)
                        {
                            for (var j1 = j - 1L; j1 >= i; j1--)
                            {
                                if (s[j1] == '<')
                                {
                                    j = j1;
                                    break;
                                }

                            }


                        }
                    else
                        b.Write(s[i..j]);

                    }                    {
                        allText = false;
                    }

                    c = d;
                    i = i1;
                    continue;

                }

                i1 = i + bytes.IndexAny(s[i..], delimEnds[c.delim]);
                if (i1 < i)
                {
                    break;
                }

                if (c.delim != delimSpaceOrTagEnd)
                { 
                    // Consume any quote.
                    i1++;

                }

                c = new context(state:stateTag,element:c.element);
                i = i1;

            }

            if (allText)
            {
                return html;
            }
            else if (c.state == stateText || c.state == stateRCDATA)
            {
                b.Write(s[i..]);
            }

            return b.String();

        }

        // htmlNameFilter accepts valid parts of an HTML attribute or tag name or
        // a known-safe HTML attribute.
        private static @string htmlNameFilter(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeHTMLAttr)
            {
                return s;
            }

            if (len(s) == 0L)
            { 
                // Avoid violation of structure preservation.
                // <input checked {{.K}}={{.V}}>.
                // Without this, if .K is empty then .V is the value of
                // checked, but otherwise .V is the value of the attribute
                // named .K.
                return filterFailsafe;

            }

            s = strings.ToLower(s);
            {
                var t = attrType(s);

                if (t != contentTypePlain)
                { 
                    // TODO: Split attr and element name part filters so we can recognize known attributes.
                    return filterFailsafe;

                }

            }

            foreach (var (_, r) in s)
            {

                if ('0' <= r && r <= '9')                 else if ('a' <= r && r <= 'z')                 else 
                    return filterFailsafe;
                
            }
            return s;

        }

        // commentEscaper returns the empty string regardless of input.
        // Comment content does not correspond to any parsed structure or
        // human-readable content, so the simplest and most secure policy is to drop
        // content interpolated into comments.
        // This approach is equally valid whether or not static comment content is
        // removed from the template.
        private static @string commentEscaper(params object[] args)
        {
            args = args.Clone();

            return "";
        }
    }
}}
