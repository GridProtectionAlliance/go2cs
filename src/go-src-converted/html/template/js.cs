// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 October 08 03:42:19 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\js.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // nextJSCtx returns the context that determines whether a slash after the
        // given run of tokens starts a regular expression instead of a division
        // operator: / or /=.
        //
        // This assumes that the token run does not include any string tokens, comment
        // tokens, regular expression literal tokens, or division operators.
        //
        // This fails on some valid but nonsensical JavaScript programs like
        // "x = ++/foo/i" which is quite different than "x++/foo/i", but is not known to
        // fail on any known useful programs. It is based on the draft
        // JavaScript 2.0 lexical grammar and requires one token of lookbehind:
        // https://www.mozilla.org/js/language/js20-2000-07/rationale/syntax.html
        private static jsCtx nextJSCtx(slice<byte> s, jsCtx preceding)
        {
            s = bytes.TrimRight(s, "\t\n\f\r \u2028\u2029");
            if (len(s) == 0L)
            {
                return preceding;
            }
            {
                var c = s[len(s) - 1L];
                var n = len(s);

                switch (c)
                {
                    case '+': 
                        // ++ and -- are not regexp preceders, but + and - are whether
                        // they are used as infix or prefix operators.

                    case '-': 
                        // ++ and -- are not regexp preceders, but + and - are whether
                        // they are used as infix or prefix operators.
                        var start = n - 1L; 
                        // Count the number of adjacent dashes or pluses.
                        while (start > 0L && s[start - 1L] == c)
                        {
                            start--;
                        }
                        if ((n - start) & 1L == 1L)
                        { 
                            // Reached for trailing minus signs since "---" is the
                            // same as "-- -".
                            return jsCtxRegexp;

                        }
                        return jsCtxDivOp;
                        break;
                    case '.': 
                        // Handle "42."
                        if (n != 1L && '0' <= s[n - 2L] && s[n - 2L] <= '9')
                        {
                            return jsCtxDivOp;
                        }
                        return jsCtxRegexp; 
                        // Suffixes for all punctuators from section 7.7 of the language spec
                        // that only end binary operators not handled above.
                        break;
                    case ',': 

                    case '<': 

                    case '>': 

                    case '=': 

                    case '*': 

                    case '%': 

                    case '&': 

                    case '|': 

                    case '^': 

                    case '?': 
                        return jsCtxRegexp; 
                        // Suffixes for all punctuators from section 7.7 of the language spec
                        // that are prefix operators not handled above.
                        break;
                    case '!': 

                    case '~': 
                        return jsCtxRegexp; 
                        // Matches all the punctuators from section 7.7 of the language spec
                        // that are open brackets not handled above.
                        break;
                    case '(': 

                    case '[': 
                        return jsCtxRegexp; 
                        // Matches all the punctuators from section 7.7 of the language spec
                        // that precede expression starts.
                        break;
                    case ':': 

                    case ';': 

                    case '{': 
                        return jsCtxRegexp; 
                        // CAVEAT: the close punctuators ('}', ']', ')') precede div ops and
                        // are handled in the default except for '}' which can precede a
                        // division op as in
                        //    ({ valueOf: function () { return 42 } } / 2
                        // which is valid, but, in practice, developers don't divide object
                        // literals, so our heuristic works well for code like
                        //    function () { ... }  /foo/.test(x) && sideEffect();
                        // The ')' punctuator can precede a regular expression as in
                        //     if (b) /foo/.test(x) && ...
                        // but this is much less likely than
                        //     (a + b) / c
                        break;
                    case '}': 
                        return jsCtxRegexp;
                        break;
                    default: 
                        // Look for an IdentifierName and see if it is a keyword that
                        // can precede a regular expression.
                        var j = n;
                        while (j > 0L && isJSIdentPart(rune(s[j - 1L])))
                        {
                            j--;
                        }
                        if (regexpPrecederKeywords[string(s[j..])])
                        {
                            return jsCtxRegexp;
                        }
                        break;
                }
            } 
            // Otherwise is a punctuator not listed above, or
            // a string which precedes a div op, or an identifier
            // which precedes a div op.
            return jsCtxDivOp;

        }

        // regexpPrecederKeywords is a set of reserved JS keywords that can precede a
        // regular expression in JS source.
        private static map regexpPrecederKeywords = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"break":true,"case":true,"continue":true,"delete":true,"do":true,"else":true,"finally":true,"in":true,"instanceof":true,"return":true,"throw":true,"try":true,"typeof":true,"void":true,};

        private static var jsonMarshalType = reflect.TypeOf((json.Marshaler.val)(null)).Elem();

        // indirectToJSONMarshaler returns the value, after dereferencing as many times
        // as necessary to reach the base type (or nil) or an implementation of json.Marshal.
        private static void indirectToJSONMarshaler(object a)
        { 
            // text/template now supports passing untyped nil as a func call
            // argument, so we must support it. Otherwise we'd panic below, as one
            // cannot call the Type or Interface methods on an invalid
            // reflect.Value. See golang.org/issue/18716.
            if (a == null)
            {
                return null;
            }

            var v = reflect.ValueOf(a);
            while (!v.Type().Implements(jsonMarshalType) && v.Kind() == reflect.Ptr && !v.IsNil())
            {
                v = v.Elem();
            }

            return v.Interface();

        }

        // jsValEscaper escapes its inputs to a JS Expression (section 11.14) that has
        // neither side-effects nor free variables outside (NaN, Infinity).
        private static @string jsValEscaper(params object[] args)
        {
            args = args.Clone();

            var a = default;
            if (len(args) == 1L)
            {
                a = indirectToJSONMarshaler(args[0L]);
                switch (a.type())
                {
                    case JS t:
                        return string(t);
                        break;
                    case JSStr t:
                        return "\"" + string(t) + "\"";
                        break;
                    case json.Marshaler t:
                        break;
                    case fmt.Stringer t:
                        a = t.String();
                        break;
                }

            }
            else
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        args[i] = indirectToJSONMarshaler(arg);
                    }

                    i = i__prev1;
                }

                a = fmt.Sprint(args);

            } 
            // TODO: detect cycles before calling Marshal which loops infinitely on
            // cyclic data. This may be an unacceptable DoS risk.
            var (b, err) = json.Marshal(a);
            if (err != null)
            { 
                // Put a space before comment so that if it is flush against
                // a division operator it is not turned into a line comment:
                //     x/{{y}}
                // turning into
                //     x//* error marshaling y:
                //          second line of error message */null
                return fmt.Sprintf(" /* %s */null ", strings.ReplaceAll(err.Error(), "*/", "* /"));

            } 

            // TODO: maybe post-process output to prevent it from containing
            // "<!--", "-->", "<![CDATA[", "]]>", or "</script"
            // in case custom marshalers produce output containing those.
            // Note: Do not use \x escaping to save bytes because it is not JSON compatible and this escaper
            // supports ld+json content-type.
            if (len(b) == 0L)
            { 
                // In, `x=y/{{.}}*z` a json.Marshaler that produces "" should
                // not cause the output `x=y/*z`.
                return " null ";

            }

            var (first, _) = utf8.DecodeRune(b);
            var (last, _) = utf8.DecodeLastRune(b);
            strings.Builder buf = default; 
            // Prevent IdentifierNames and NumericLiterals from running into
            // keywords: in, instanceof, typeof, void
            var pad = isJSIdentPart(first) || isJSIdentPart(last);
            if (pad)
            {
                buf.WriteByte(' ');
            }

            long written = 0L; 
            // Make sure that json.Marshal escapes codepoints U+2028 & U+2029
            // so it falls within the subset of JSON which is valid JS.
            {
                var i__prev1 = i;

                long i = 0L;

                while (i < len(b))
                {
                    var (rune, n) = utf8.DecodeRune(b[i..]);
                    @string repl = "";
                    if (rune == 0x2028UL)
                    {
                        repl = "\\u2028";
                    }
                    else if (rune == 0x2029UL)
                    {
                        repl = "\\u2029";
                    }

                    if (repl != "")
                    {
                        buf.Write(b[written..i]);
                        buf.WriteString(repl);
                        written = i + n;
                    }

                    i += n;

                }


                i = i__prev1;
            }
            if (buf.Len() != 0L)
            {
                buf.Write(b[written..]);
                if (pad)
                {
                    buf.WriteByte(' ');
                }

                return buf.String();

            }

            return string(b);

        }

        // jsStrEscaper produces a string that can be included between quotes in
        // JavaScript source, in JavaScript embedded in an HTML5 <script> element,
        // or in an HTML5 event handler attribute such as onclick.
        private static @string jsStrEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeJSStr)
            {
                return replace(s, jsStrNormReplacementTable);
            }

            return replace(s, jsStrReplacementTable);

        }

        // jsRegexpEscaper behaves like jsStrEscaper but escapes regular expression
        // specials so the result is treated literally when included in a regular
        // expression literal. /foo{{.X}}bar/ matches the string "foo" followed by
        // the literal text of {{.X}} followed by the string "bar".
        private static @string jsRegexpEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, _) = stringify(args);
            s = replace(s, jsRegexpReplacementTable);
            if (s == "")
            { 
                // /{{.X}}/ should not produce a line comment when .X == "".
                return "(?:)";

            }

            return s;

        }

        // replace replaces each rune r of s with replacementTable[r], provided that
        // r < len(replacementTable). If replacementTable[r] is the empty string then
        // no replacement is made.
        // It also replaces runes U+2028 and U+2029 with the raw strings `\u2028` and
        // `\u2029`.
        private static @string replace(@string s, slice<@string> replacementTable)
        {
            strings.Builder b = default;
            var r = rune(0L);
            long w = 0L;
            long written = 0L;
            {
                long i = 0L;

                while (i < len(s))
                { 
                    // See comment in htmlEscaper.
                    r, w = utf8.DecodeRuneInString(s[i..]);
                    @string repl = default;

                    if (int(r) < len(lowUnicodeReplacementTable)) 
                        repl = lowUnicodeReplacementTable[r];
                    else if (int(r) < len(replacementTable) && replacementTable[r] != "") 
                        repl = replacementTable[r];
                    else if (r == '\u2028') 
                        repl = "\\u2028";
                    else if (r == '\u2029') 
                        repl = "\\u2029";
                    else 
                        continue;
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
            if (written == 0L)
            {
                return s;
            }

            b.WriteString(s[written..]);
            return b.String();

        }

        private static @string lowUnicodeReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, `\u0000`), (1, `\u0001`), (2, `\u0002`), (3, `\u0003`), (4, `\u0004`), (5, `\u0005`), (6, `\u0006`), ('\a', `\u0007`), ('\b', `\u0008`), ('\t', `\t`), ('\n', `\n`), ('\v', `\u000b`), ('\f', `\f`), ('\r', `\r`), (0xe, `\u000e`), (0xf, `\u000f`), (0x10, `\u0010`), (0x11, `\u0011`), (0x12, `\u0012`), (0x13, `\u0013`), (0x14, `\u0014`), (0x15, `\u0015`), (0x16, `\u0016`), (0x17, `\u0017`), (0x18, `\u0018`), (0x19, `\u0019`), (0x1a, `\u001a`), (0x1b, `\u001b`), (0x1c, `\u001c`), (0x1d, `\u001d`), (0x1e, `\u001e`), (0x1f, `\u001f`)));

        private static @string jsStrReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, `\u0000`), ('\t', `\t`), ('\n', `\n`), ('\v', `\u000b`), ('\f', `\f`), ('\r', `\r`), ('"', `\u0022`), ('&', `\u0026`), ('\'', `\u0027`), ('+', `\u002b`), ('/', `\/`), ('<', `\u003c`), ('>', `\u003e`), ('\\', `\\`)));

        // jsStrNormReplacementTable is like jsStrReplacementTable but does not
        // overencode existing escapes since this table has no entry for `\`.
        private static @string jsStrNormReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, `\u0000`), ('\t', `\t`), ('\n', `\n`), ('\v', `\u000b`), ('\f', `\f`), ('\r', `\r`), ('"', `\u0022`), ('&', `\u0026`), ('\'', `\u0027`), ('+', `\u002b`), ('/', `\/`), ('<', `\u003c`), ('>', `\u003e`)));
        private static @string jsRegexpReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, `\u0000`), ('\t', `\t`), ('\n', `\n`), ('\v', `\u000b`), ('\f', `\f`), ('\r', `\r`), ('"', `\u0022`), ('$', `\$`), ('&', `\u0026`), ('\'', `\u0027`), ('(', `\(`), (')', `\)`), ('*', `\*`), ('+', `\u002b`), ('-', `\-`), ('.', `\.`), ('/', `\/`), ('<', `\u003c`), ('>', `\u003e`), ('?', `\?`), ('[', `\[`), ('\\', `\\`), (']', `\]`), ('^', `\^`), ('{', `\{`), ('|', `\|`), ('}', `\}`)));

        // isJSIdentPart reports whether the given rune is a JS identifier part.
        // It does not handle all the non-Latin letters, joiners, and combining marks,
        // but it does handle every codepoint that can occur in a numeric literal or
        // a keyword.
        private static bool isJSIdentPart(int r)
        {

            if (r == '$') 
                return true;
            else if ('0' <= r && r <= '9') 
                return true;
            else if ('A' <= r && r <= 'Z') 
                return true;
            else if (r == '_') 
                return true;
            else if ('a' <= r && r <= 'z') 
                return true;
                        return false;

        }

        // isJSType reports whether the given MIME type should be considered JavaScript.
        //
        // It is used to determine whether a script tag with a type attribute is a javascript container.
        private static bool isJSType(@string mimeType)
        { 
            // per
            //   https://www.w3.org/TR/html5/scripting-1.html#attr-script-type
            //   https://tools.ietf.org/html/rfc7231#section-3.1.1
            //   https://tools.ietf.org/html/rfc4329#section-3
            //   https://www.ietf.org/rfc/rfc4627.txt
            // discard parameters
            {
                var i = strings.Index(mimeType, ";");

                if (i >= 0L)
                {
                    mimeType = mimeType[..i];
                }

            }

            mimeType = strings.ToLower(mimeType);
            mimeType = strings.TrimSpace(mimeType);
            switch (mimeType)
            {
                case "application/ecmascript": 

                case "application/javascript": 

                case "application/json": 

                case "application/ld+json": 

                case "application/x-ecmascript": 

                case "application/x-javascript": 

                case "module": 

                case "text/ecmascript": 

                case "text/javascript": 

                case "text/javascript1.0": 

                case "text/javascript1.1": 

                case "text/javascript1.2": 

                case "text/javascript1.3": 

                case "text/javascript1.4": 

                case "text/javascript1.5": 

                case "text/jscript": 

                case "text/livescript": 

                case "text/x-ecmascript": 

                case "text/x-javascript": 
                    return true;
                    break;
                default: 
                    return false;
                    break;
            }

        }
    }
}}
