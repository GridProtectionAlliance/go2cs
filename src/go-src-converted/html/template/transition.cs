// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:36:16 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\transition.go
using bytes = go.bytes_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // transitionFunc is the array of context transition functions for text nodes.
        // A transition function takes a context and template text input, and returns
        // the updated context and the number of bytes consumed from the front of the
        // input.
        private static array<Func<context, slice<byte>, (context, long)>> transitionFunc = new array<Func<context, slice<byte>, (context, long)>>(InitKeyedValues<Func<context, slice<byte>, (context, long)>>((stateText, tText), (stateTag, tTag), (stateAttrName, tAttrName), (stateAfterName, tAfterName), (stateBeforeValue, tBeforeValue), (stateHTMLCmt, tHTMLCmt), (stateRCDATA, tSpecialTagEnd), (stateAttr, tAttr), (stateURL, tURL), (stateSrcset, tURL), (stateJS, tJS), (stateJSDqStr, tJSDelimited), (stateJSSqStr, tJSDelimited), (stateJSRegexp, tJSDelimited), (stateJSBlockCmt, tBlockCmt), (stateJSLineCmt, tLineCmt), (stateCSS, tCSS), (stateCSSDqStr, tCSSStr), (stateCSSSqStr, tCSSStr), (stateCSSDqURL, tCSSStr), (stateCSSSqURL, tCSSStr), (stateCSSURL, tCSSStr), (stateCSSBlockCmt, tBlockCmt), (stateCSSLineCmt, tLineCmt), (stateError, tError)));

        private static slice<byte> commentStart = (slice<byte>)"<!--";
        private static slice<byte> commentEnd = (slice<byte>)"-->";

        // tText is the context transition function for the text state.
        private static (context, long) tText(context c, slice<byte> s)
        {
            long k = 0L;
            while (true)
            {
                var i = k + bytes.IndexByte(s[k..], '<');
                if (i < k || i + 1L == len(s))
                {
                    return (c, len(s));
                }
                else if (i + 4L <= len(s) && bytes.Equal(commentStart, s[i..i + 4L]))
                {
                    return (new context(state:stateHTMLCmt), i + 4L);
                }
                i++;
                var end = false;
                if (s[i] == '/')
                {
                    if (i + 1L == len(s))
                    {
                        return (c, len(s));
                    }
                    end = true;
                    i = i + 1L;
                }
                var (j, e) = eatTagName(s, i);
                if (j != i)
                {
                    if (end)
                    {
                        e = elementNone;
                    } 
                    // We've found an HTML tag.
                    return (new context(state:stateTag,element:e), j);
                }
                k = j;
            }

        }

        private static array<state> elementContentType = new array<state>(InitKeyedValues<state>((elementNone, stateText), (elementScript, stateJS), (elementStyle, stateCSS), (elementTextarea, stateRCDATA), (elementTitle, stateRCDATA)));

        // tTag is the context transition function for the tag state.
        private static (context, long) tTag(context c, slice<byte> s)
        { 
            // Find the attribute name.
            var i = eatWhiteSpace(s, 0L);
            if (i == len(s))
            {
                return (c, len(s));
            }
            if (s[i] == '>')
            {
                return (new context(state:elementContentType[c.element],element:c.element,), i + 1L);
            }
            var (j, err) = eatAttrName(s, i);
            if (err != null)
            {
                return (new context(state:stateError,err:err), len(s));
            }
            var state = stateTag;
            var attr = attrNone;
            if (i == j)
            {
                return (new context(state:stateError,err:errorf(ErrBadHTML,nil,0,"expected space, attr name, or end of tag, but got %q",s[i:]),), len(s));
            }
            var attrName = strings.ToLower(string(s[i..j]));
            if (c.element == elementScript && attrName == "type")
            {
                attr = attrScriptType;
            }
            else
            {

                if (attrType(attrName) == contentTypeURL) 
                    attr = attrURL;
                else if (attrType(attrName) == contentTypeCSS) 
                    attr = attrStyle;
                else if (attrType(attrName) == contentTypeJS) 
                    attr = attrScript;
                else if (attrType(attrName) == contentTypeSrcset) 
                    attr = attrSrcset;
                            }
            if (j == len(s))
            {
                state = stateAttrName;
            }
            else
            {
                state = stateAfterName;
            }
            return (new context(state:state,element:c.element,attr:attr), j);
        }

        // tAttrName is the context transition function for stateAttrName.
        private static (context, long) tAttrName(context c, slice<byte> s)
        {
            var (i, err) = eatAttrName(s, 0L);
            if (err != null)
            {
                return (new context(state:stateError,err:err), len(s));
            }
            else if (i != len(s))
            {
                c.state = stateAfterName;
            }
            return (c, i);
        }

        // tAfterName is the context transition function for stateAfterName.
        private static (context, long) tAfterName(context c, slice<byte> s)
        { 
            // Look for the start of the value.
            var i = eatWhiteSpace(s, 0L);
            if (i == len(s))
            {
                return (c, len(s));
            }
            else if (s[i] != '=')
            { 
                // Occurs due to tag ending '>', and valueless attribute.
                c.state = stateTag;
                return (c, i);
            }
            c.state = stateBeforeValue; 
            // Consume the "=".
            return (c, i + 1L);
        }

        private static array<state> attrStartStates = new array<state>(InitKeyedValues<state>((attrNone, stateAttr), (attrScript, stateJS), (attrScriptType, stateAttr), (attrStyle, stateCSS), (attrURL, stateURL), (attrSrcset, stateSrcset)));

        // tBeforeValue is the context transition function for stateBeforeValue.
        private static (context, long) tBeforeValue(context c, slice<byte> s)
        {
            var i = eatWhiteSpace(s, 0L);
            if (i == len(s))
            {
                return (c, len(s));
            } 
            // Find the attribute delimiter.
            var delim = delimSpaceOrTagEnd;
            switch (s[i])
            {
                case '\'': 
                    delim = delimSingleQuote;
                    i = i + 1L;
                    break;
                case '"': 
                    delim = delimDoubleQuote;
                    i = i + 1L;
                    break;
            }
            c.state = attrStartStates[c.attr];
            c.delim = delim;
            return (c, i);
        }

        // tHTMLCmt is the context transition function for stateHTMLCmt.
        private static (context, long) tHTMLCmt(context c, slice<byte> s)
        {
            {
                var i = bytes.Index(s, commentEnd);

                if (i != -1L)
                {
                    return (new context(), i + 3L);
                }

            }
            return (c, len(s));
        }

        // specialTagEndMarkers maps element types to the character sequence that
        // case-insensitively signals the end of the special tag body.
        private static array<slice<byte>> specialTagEndMarkers = new array<slice<byte>>(InitKeyedValues<slice<byte>>((elementScript, []byte("script")), (elementStyle, []byte("style")), (elementTextarea, []byte("textarea")), (elementTitle, []byte("title"))));

        private static slice<byte> specialTagEndPrefix = (slice<byte>)"</";        private static slice<byte> tagEndSeparators = (slice<byte>)"> \t\n\f/";

        // tSpecialTagEnd is the context transition function for raw text and RCDATA
        // element states.
        private static (context, long) tSpecialTagEnd(context c, slice<byte> s)
        {
            if (c.element != elementNone)
            {
                {
                    var i = indexTagEnd(s, specialTagEndMarkers[c.element]);

                    if (i != -1L)
                    {
                        return (new context(), i);
                    }

                }
            }
            return (c, len(s));
        }

        // indexTagEnd finds the index of a special tag end in a case insensitive way, or returns -1
        private static long indexTagEnd(slice<byte> s, slice<byte> tag)
        {
            long res = 0L;
            var plen = len(specialTagEndPrefix);
            while (len(s) > 0L)
            { 
                // Try to find the tag end prefix first
                var i = bytes.Index(s, specialTagEndPrefix);
                if (i == -1L)
                {
                    return i;
                }
                s = s[i + plen..]; 
                // Try to match the actual tag if there is still space for it
                if (len(tag) <= len(s) && bytes.EqualFold(tag, s[..len(tag)]))
                {
                    s = s[len(tag)..]; 
                    // Check the tag is followed by a proper separator
                    if (len(s) > 0L && bytes.IndexByte(tagEndSeparators, s[0L]) != -1L)
                    {
                        return res + i;
                    }
                    res += len(tag);
                }
                res += i + plen;
            }

            return -1L;
        }

        // tAttr is the context transition function for the attribute state.
        private static (context, long) tAttr(context c, slice<byte> s)
        {
            return (c, len(s));
        }

        // tURL is the context transition function for the URL state.
        private static (context, long) tURL(context c, slice<byte> s)
        {
            if (bytes.ContainsAny(s, "#?"))
            {
                c.urlPart = urlPartQueryOrFrag;
            }
            else if (len(s) != eatWhiteSpace(s, 0L) && c.urlPart == urlPartNone)
            { 
                // HTML5 uses "Valid URL potentially surrounded by spaces" for
                // attrs: http://www.w3.org/TR/html5/index.html#attributes-1
                c.urlPart = urlPartPreQuery;
            }
            return (c, len(s));
        }

        // tJS is the context transition function for the JS state.
        private static (context, long) tJS(context c, slice<byte> s) => func((_, panic, __) =>
        {
            var i = bytes.IndexAny(s, "\"\'/");
            if (i == -1L)
            { 
                // Entire input is non string, comment, regexp tokens.
                c.jsCtx = nextJSCtx(s, c.jsCtx);
                return (c, len(s));
            }
            c.jsCtx = nextJSCtx(s[..i], c.jsCtx);
            switch (s[i])
            {
                case '"': 
                    c.state = stateJSDqStr;
                    c.jsCtx = jsCtxRegexp;
                    break;
                case '\'': 
                    c.state = stateJSSqStr;
                    c.jsCtx = jsCtxRegexp;
                    break;
                case '/': 

                    if (i + 1L < len(s) && s[i + 1L] == '/') 
                        c.state = stateJSLineCmt;
                        i = i + 1L;
                    else if (i + 1L < len(s) && s[i + 1L] == '*') 
                        c.state = stateJSBlockCmt;
                        i = i + 1L;
                    else if (c.jsCtx == jsCtxRegexp) 
                        c.state = stateJSRegexp;
                    else if (c.jsCtx == jsCtxDivOp) 
                        c.jsCtx = jsCtxRegexp;
                    else 
                        return (new context(state:stateError,err:errorf(ErrSlashAmbig,nil,0,"'/' could start a division or regexp: %.32q",s[i:]),), len(s));
                    break;
                default: 
                    panic("unreachable");
                    break;
            }
            return (c, i + 1L);
        });

        // tJSDelimited is the context transition function for the JS string and regexp
        // states.
        private static (context, long) tJSDelimited(context c, slice<byte> s)
        {
            @string specials = "\\\"";

            if (c.state == stateJSSqStr) 
                specials = "\\\'";
            else if (c.state == stateJSRegexp) 
                specials = "\\/[]";
                        long k = 0L;
            var inCharset = false;
            while (true)
            {
                var i = k + bytes.IndexAny(s[k..], specials);
                if (i < k)
                {
                    break;
                }
                switch (s[i])
                {
                    case '\\': 
                        i++;
                        if (i == len(s))
                        {
                            return (new context(state:stateError,err:errorf(ErrPartialEscape,nil,0,"unfinished escape sequence in JS string: %q",s),), len(s));
                        }
                        break;
                    case '[': 
                        inCharset = true;
                        break;
                    case ']': 
                        inCharset = false;
                        break;
                    default: 
                        // end delimiter
                        if (!inCharset)
                        {
                            c.state = stateJS;
                            c.jsCtx = jsCtxDivOp;
                            return (c, i + 1L);
                        }
                        break;
                }
                k = i + 1L;
            }


            if (inCharset)
            { 
                // This can be fixed by making context richer if interpolation
                // into charsets is desired.
                return (new context(state:stateError,err:errorf(ErrPartialCharset,nil,0,"unfinished JS regexp charset: %q",s),), len(s));
            }
            return (c, len(s));
        }

        private static slice<byte> blockCommentEnd = (slice<byte>)"*/";

        // tBlockCmt is the context transition function for /*comment*/ states.
        private static (context, long) tBlockCmt(context c, slice<byte> s) => func((_, panic, __) =>
        {
            var i = bytes.Index(s, blockCommentEnd);
            if (i == -1L)
            {
                return (c, len(s));
            }

            if (c.state == stateJSBlockCmt) 
                c.state = stateJS;
            else if (c.state == stateCSSBlockCmt) 
                c.state = stateCSS;
            else 
                panic(c.state.String());
                        return (c, i + 2L);
        });

        // tLineCmt is the context transition function for //comment states.
        private static (context, long) tLineCmt(context c, slice<byte> s) => func((_, panic, __) =>
        {
            @string lineTerminators = default;
            state endState = default;

            if (c.state == stateJSLineCmt) 
                lineTerminators = "\n\r\u2028\u2029";
                endState = stateJS;
            else if (c.state == stateCSSLineCmt) 
                lineTerminators = "\n\f\r";
                endState = stateCSS; 
                // Line comments are not part of any published CSS standard but
                // are supported by the 4 major browsers.
                // This defines line comments as
                //     LINECOMMENT ::= "//" [^\n\f\d]*
                // since http://www.w3.org/TR/css3-syntax/#SUBTOK-nl defines
                // newlines:
                //     nl ::= #xA | #xD #xA | #xD | #xC
            else 
                panic(c.state.String());
                        var i = bytes.IndexAny(s, lineTerminators);
            if (i == -1L)
            {
                return (c, len(s));
            }
            c.state = endState; 
            // Per section 7.4 of EcmaScript 5 : http://es5.github.com/#x7.4
            // "However, the LineTerminator at the end of the line is not
            // considered to be part of the single-line comment; it is
            // recognized separately by the lexical grammar and becomes part
            // of the stream of input elements for the syntactic grammar."
            return (c, i);
        });

        // tCSS is the context transition function for the CSS state.
        private static (context, long) tCSS(context c, slice<byte> s)
        { 
            // CSS quoted strings are almost never used except for:
            // (1) URLs as in background: "/foo.png"
            // (2) Multiword font-names as in font-family: "Times New Roman"
            // (3) List separators in content values as in inline-lists:
            //    <style>
            //    ul.inlineList { list-style: none; padding:0 }
            //    ul.inlineList > li { display: inline }
            //    ul.inlineList > li:before { content: ", " }
            //    ul.inlineList > li:first-child:before { content: "" }
            //    </style>
            //    <ul class=inlineList><li>One<li>Two<li>Three</ul>
            // (4) Attribute value selectors as in a[href="http://example.com/"]
            //
            // We conservatively treat all strings as URLs, but make some
            // allowances to avoid confusion.
            //
            // In (1), our conservative assumption is justified.
            // In (2), valid font names do not contain ':', '?', or '#', so our
            // conservative assumption is fine since we will never transition past
            // urlPartPreQuery.
            // In (3), our protocol heuristic should not be tripped, and there
            // should not be non-space content after a '?' or '#', so as long as
            // we only %-encode RFC 3986 reserved characters we are ok.
            // In (4), we should URL escape for URL attributes, and for others we
            // have the attribute name available if our conservative assumption
            // proves problematic for real code.

            long k = 0L;
            while (true)
            {
                var i = k + bytes.IndexAny(s[k..], "(\"\'/");
                if (i < k)
                {
                    return (c, len(s));
                }
                switch (s[i])
                {
                    case '(': 
                        // Look for url to the left.
                        var p = bytes.TrimRight(s[..i], "\t\n\f\r ");
                        if (endsWithCSSKeyword(p, "url"))
                        {
                            var j = len(s) - len(bytes.TrimLeft(s[i + 1L..], "\t\n\f\r "));

                            if (j != len(s) && s[j] == '"') 
                                c.state = stateCSSDqURL;
                                j = j + 1L;
                            else if (j != len(s) && s[j] == '\'') 
                                c.state = stateCSSSqURL;
                                j = j + 1L;
                            else 
                                c.state = stateCSSURL;
                                                    return (c, j);
                        }
                        break;
                    case '/': 
                        if (i + 1L < len(s))
                        {
                            switch (s[i + 1L])
                            {
                                case '/': 
                                    c.state = stateCSSLineCmt;
                                    return (c, i + 2L);
                                    break;
                                case '*': 
                                    c.state = stateCSSBlockCmt;
                                    return (c, i + 2L);
                                    break;
                            }
                        }
                        break;
                    case '"': 
                        c.state = stateCSSDqStr;
                        return (c, i + 1L);
                        break;
                    case '\'': 
                        c.state = stateCSSSqStr;
                        return (c, i + 1L);
                        break;
                }
                k = i + 1L;
            }

        }

        // tCSSStr is the context transition function for the CSS string and URL states.
        private static (context, long) tCSSStr(context c, slice<byte> s) => func((_, panic, __) =>
        {
            @string endAndEsc = default;

            if (c.state == stateCSSDqStr || c.state == stateCSSDqURL) 
                endAndEsc = "\\\"";
            else if (c.state == stateCSSSqStr || c.state == stateCSSSqURL) 
                endAndEsc = "\\\'";
            else if (c.state == stateCSSURL) 
                // Unquoted URLs end with a newline or close parenthesis.
                // The below includes the wc (whitespace character) and nl.
                endAndEsc = "\\\t\n\f\r )";
            else 
                panic(c.state.String());
                        long k = 0L;
            while (true)
            {
                var i = k + bytes.IndexAny(s[k..], endAndEsc);
                if (i < k)
                {
                    var (c, nread) = tURL(c, decodeCSS(s[k..]));
                    return (c, k + nread);
                }
                if (s[i] == '\\')
                {
                    i++;
                    if (i == len(s))
                    {
                        return (new context(state:stateError,err:errorf(ErrPartialEscape,nil,0,"unfinished escape sequence in CSS string: %q",s),), len(s));
                    }
                }
                else
                {
                    c.state = stateCSS;
                    return (c, i + 1L);
                }
                c, _ = tURL(c, decodeCSS(s[..i + 1L]));
                k = i + 1L;
            }

        });

        // tError is the context transition function for the error state.
        private static (context, long) tError(context c, slice<byte> s)
        {
            return (c, len(s));
        }

        // eatAttrName returns the largest j such that s[i:j] is an attribute name.
        // It returns an error if s[i:] does not look like it begins with an
        // attribute name, such as encountering a quote mark without a preceding
        // equals sign.
        private static (long, ref Error) eatAttrName(slice<byte> s, long i)
        {
            for (var j = i; j < len(s); j++)
            {
                switch (s[j])
                {
                    case ' ': 

                    case '\t': 

                    case '\n': 

                    case '\f': 

                    case '\r': 

                    case '=': 

                    case '>': 
                        return (j, null);
                        break;
                    case '\'': 
                        // These result in a parse warning in HTML5 and are
                        // indicative of serious problems if seen in an attr
                        // name in a template.

                    case '"': 
                        // These result in a parse warning in HTML5 and are
                        // indicative of serious problems if seen in an attr
                        // name in a template.

                    case '<': 
                        // These result in a parse warning in HTML5 and are
                        // indicative of serious problems if seen in an attr
                        // name in a template.
                        return (-1L, errorf(ErrBadHTML, null, 0L, "%q in attribute name: %.32q", s[j..j + 1L], s));
                        break;
                    default: 
                        break;
                }
            }

            return (len(s), null);
        }

        private static map elementNameMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, element>{"script":elementScript,"style":elementStyle,"textarea":elementTextarea,"title":elementTitle,};

        // asciiAlpha reports whether c is an ASCII letter.
        private static bool asciiAlpha(byte c)
        {
            return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z';
        }

        // asciiAlphaNum reports whether c is an ASCII letter or digit.
        private static bool asciiAlphaNum(byte c)
        {
            return asciiAlpha(c) || '0' <= c && c <= '9';
        }

        // eatTagName returns the largest j such that s[i:j] is a tag name and the tag type.
        private static (long, element) eatTagName(slice<byte> s, long i)
        {
            if (i == len(s) || !asciiAlpha(s[i]))
            {
                return (i, elementNone);
            }
            var j = i + 1L;
            while (j < len(s))
            {
                var x = s[j];
                if (asciiAlphaNum(x))
                {
                    j++;
                    continue;
                } 
                // Allow "x-y" or "x:y" but not "x-", "-y", or "x--y".
                if ((x == ':' || x == '-') && j + 1L < len(s) && asciiAlphaNum(s[j + 1L]))
                {
                    j += 2L;
                    continue;
                }
                break;
            }

            return (j, elementNameMap[strings.ToLower(string(s[i..j]))]);
        }

        // eatWhiteSpace returns the largest j such that s[i:j] is white space.
        private static long eatWhiteSpace(slice<byte> s, long i)
        {
            for (var j = i; j < len(s); j++)
            {
                switch (s[j])
                {
                    case ' ': 

                    case '\t': 

                    case '\n': 

                    case '\f': 

                    case '\r': 
                        break;
                    default: 
                        return j;
                        break;
                }
            }

            return len(s);
        }
    }
}}
