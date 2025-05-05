// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using json = encoding.json_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using encoding;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// jsWhitespace contains all of the JS whitespace characters, as defined
// by the \s character class.
// See https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions/Character_classes.
internal static readonly @string jsWhitespace = "\f\n\r\t\v\u0020\u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000\ufeff"u8;

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
internal static jsCtx nextJSCtx(slice<byte> s, jsCtx preceding) {
    // Trim all JS whitespace characters
    s = bytes.TrimRight(s, jsWhitespace);
    if (len(s) == 0) {
        return preceding;
    }
    // All cases below are in the single-byte UTF-8 group.
    {
        var c = s[len(s) - 1];
        nint n = len(s);
        switch (c) {
        case (rune)'+' or (rune)'-': {
            nint start = n - 1;
            while (start > 0 && s[start - 1] == c) {
                // ++ and -- are not regexp preceders, but + and - are whether
                // they are used as infix or prefix operators.
                // Count the number of adjacent dashes or pluses.
                start--;
            }
            if ((nint)((n - start) & 1) == 1) {
                // Reached for trailing minus signs since "---" is the
                // same as "-- -".
                return jsCtxRegexp;
            }
            return jsCtxDivOp;
        }
        case (rune)'.': {
            if (n != 1 && (rune)'0' <= s[n - 2] && s[n - 2] <= (rune)'9') {
                // Handle "42."
                return jsCtxDivOp;
            }
            return jsCtxRegexp;
        }
        case (rune)',' or (rune)'<' or (rune)'>' or (rune)'=' or (rune)'*' or (rune)'%' or (rune)'&' or (rune)'|' or (rune)'^' or (rune)'?': {
            return jsCtxRegexp;
        }
        case (rune)'!' or (rune)'~': {
            return jsCtxRegexp;
        }
        case (rune)'(' or (rune)'[': {
            return jsCtxRegexp;
        }
        case (rune)':' or (rune)';' or (rune)'{': {
            return jsCtxRegexp;
        }
        case (rune)'}': {
            return jsCtxRegexp;
        }
        default: {
            nint j = n;
            while (j > 0 && isJSIdentPart(((rune)s[j - 1]))) {
                // Suffixes for all punctuators from section 7.7 of the language spec
                // that only end binary operators not handled above.
                // Suffixes for all punctuators from section 7.7 of the language spec
                // that are prefix operators not handled above.
                // Matches all the punctuators from section 7.7 of the language spec
                // that are open brackets not handled above.
                // Matches all the punctuators from section 7.7 of the language spec
                // that precede expression starts.
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
                // Look for an IdentifierName and see if it is a keyword that
                // can precede a regular expression.
                j--;
            }
            if (regexpPrecederKeywords[((@string)(s[(int)(j)..]))]) {
                return jsCtxRegexp;
            }
            break;
        }}
    }

    // Otherwise is a punctuator not listed above, or
    // a string which precedes a div op, or an identifier
    // which precedes a div op.
    return jsCtxDivOp;
}

// regexpPrecederKeywords is a set of reserved JS keywords that can precede a
// regular expression in JS source.
internal static map<@string, bool> regexpPrecederKeywords = new map<@string, bool>{
    ["break"u8] = true,
    ["case"u8] = true,
    ["continue"u8] = true,
    ["delete"u8] = true,
    ["do"u8] = true,
    ["else"u8] = true,
    ["finally"u8] = true,
    ["in"u8] = true,
    ["instanceof"u8] = true,
    ["return"u8] = true,
    ["throw"u8] = true,
    ["try"u8] = true,
    ["typeof"u8] = true,
    ["void"u8] = true
};

internal static reflectꓸType jsonMarshalType = reflect.TypeFor[json.Marshaler]();

// indirectToJSONMarshaler returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil) or an implementation of json.Marshal.
internal static any indirectToJSONMarshaler(any a) {
    // text/template now supports passing untyped nil as a func call
    // argument, so we must support it. Otherwise we'd panic below, as one
    // cannot call the Type or Interface methods on an invalid
    // reflect.Value. See golang.org/issue/18716.
    if (a == default!) {
        return default!;
    }
    var v = reflect.ValueOf(a);
    while (!v.Type().Implements(jsonMarshalType) && v.Kind() == reflect.ΔPointer && !v.IsNil()) {
        v = v.Elem();
    }
    return v.Interface();
}

// jsValEscaper escapes its inputs to a JS Expression (section 11.14) that has
// neither side-effects nor free variables outside (NaN, Infinity).
internal static @string jsValEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    any a = default!;
    if (len(args) == 1){
        a = indirectToJSONMarshaler(args[0]);
        switch (a.type()) {
        case JS t: {
            return ((@string)t);
        }
        case JSStr t: {
            return @""""u8 + ((@string)t) + @""""u8;
        }
        case json.Marshaler t: {
        }
        case fmt.Stringer t: {
            a = t.String();
            break;
        }}
    } else {
        // TODO: normalize quotes.
        // Do not treat as a Stringer.
        foreach (var (iΔ1, arg) in args) {
            args[iΔ1] = indirectToJSONMarshaler(arg);
        }
        a = fmt.Sprint(args.ꓸꓸꓸ);
    }
    // TODO: detect cycles before calling Marshal which loops infinitely on
    // cyclic data. This may be an unacceptable DoS risk.
    (b, err) = json.Marshal(a);
    if (err != default!) {
        // While the standard JSON marshaler does not include user controlled
        // information in the error message, if a type has a MarshalJSON method,
        // the content of the error message is not guaranteed. Since we insert
        // the error into the template, as part of a comment, we attempt to
        // prevent the error from either terminating the comment, or the script
        // block itself.
        //
        // In particular we:
        //   * replace "*/" comment end tokens with "* /", which does not
        //     terminate the comment
        //   * replace "</script" with "\x3C/script", and "<!--" with
        //     "\x3C!--", which prevents confusing script block termination
        //     semantics
        //
        // We also put a space before the comment so that if it is flush against
        // a division operator it is not turned into a line comment:
        //     x/{{y}}
        // turning into
        //     x//* error marshaling y:
        //          second line of error message */null
        @string errStr = err.Error();
        errStr = strings.ReplaceAll(errStr, "*/"u8, "* /"u8);
        errStr = strings.ReplaceAll(errStr, "</script"u8, @"\x3C/script"u8);
        errStr = strings.ReplaceAll(errStr, "<!--"u8, @"\x3C!--"u8);
        return fmt.Sprintf(" /* %s */null "u8, errStr);
    }
    // TODO: maybe post-process output to prevent it from containing
    // "<!--", "-->", "<![CDATA[", "]]>", or "</script"
    // in case custom marshalers produce output containing those.
    // Note: Do not use \x escaping to save bytes because it is not JSON compatible and this escaper
    // supports ld+json content-type.
    if (len(b) == 0) {
        // In, `x=y/{{.}}*z` a json.Marshaler that produces "" should
        // not cause the output `x=y/*z`.
        return " null "u8;
    }
    var (first, _) = utf8.DecodeRune(b);
    var (last, _) = utf8.DecodeLastRune(b);
    strings.Builder buf = default!;
    // Prevent IdentifierNames and NumericLiterals from running into
    // keywords: in, instanceof, typeof, void
    var pad = isJSIdentPart(first) || isJSIdentPart(last);
    if (pad) {
        buf.WriteByte((rune)' ');
    }
    nint written = 0;
    // Make sure that json.Marshal escapes codepoints U+2028 & U+2029
    // so it falls within the subset of JSON which is valid JS.
    for (nint i = 0; i < len(b); ) {
        var (rune, n) = utf8.DecodeRune(b[(int)(i)..]);
        @string repl = ""u8;
        if (rune == 8232){
            repl = @"\u2028"u8;
        } else 
        if (rune == 8233) {
            repl = @"\u2029"u8;
        }
        if (repl != ""u8) {
            buf.Write(b[(int)(written)..(int)(i)]);
            buf.WriteString(repl);
            written = i + n;
        }
        i += n;
    }
    if (buf.Len() != 0) {
        buf.Write(b[(int)(written)..]);
        if (pad) {
            buf.WriteByte((rune)' ');
        }
        return buf.String();
    }
    return ((@string)b);
}

// jsStrEscaper produces a string that can be included between quotes in
// JavaScript source, in JavaScript embedded in an HTML5 <script> element,
// or in an HTML5 event handler attribute such as onclick.
internal static @string jsStrEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeJSStr) {
        return replace(s, jsStrNormReplacementTable);
    }
    return replace(s, jsStrReplacementTable);
}

internal static @string jsTmplLitEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, _) = stringify(args.ꓸꓸꓸ);
    return replace(s, jsBqStrReplacementTable);
}

// jsRegexpEscaper behaves like jsStrEscaper but escapes regular expression
// specials so the result is treated literally when included in a regular
// expression literal. /foo{{.X}}bar/ matches the string "foo" followed by
// the literal text of {{.X}} followed by the string "bar".
internal static @string jsRegexpEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, _) = stringify(args.ꓸꓸꓸ);
    s = replace(s, jsRegexpReplacementTable);
    if (s == ""u8) {
        // /{{.X}}/ should not produce a line comment when .X == "".
        return "(?:)"u8;
    }
    return s;
}

// replace replaces each rune r of s with replacementTable[r], provided that
// r < len(replacementTable). If replacementTable[r] is the empty string then
// no replacement is made.
// It also replaces runes U+2028 and U+2029 with the raw strings `\u2028` and
// `\u2029`.
internal static @string replace(@string s, slice<@string> replacementTable) {
    strings.Builder b = default!;
    var r = ((rune)0);
    nint w = 0;
    nint written = 0;
    for (nint i = 0; i < len(s); i += w) {
        // See comment in htmlEscaper.
        (r, w) = utf8.DecodeRuneInString(s[(int)(i)..]);
        @string repl = default!;
        switch (ᐧ) {
        case {} when ((nint)r) < len(lowUnicodeReplacementTable): {
            repl = lowUnicodeReplacementTable[r];
            break;
        }
        case {} when ((nint)r) < len(replacementTable) && replacementTable[r] != "": {
            repl = replacementTable[r];
            break;
        }
        case {} when r is (rune)'\u2028': {
            repl = @"\u2028"u8;
            break;
        }
        case {} when r is (rune)'\u2029': {
            repl = @"\u2029"u8;
            break;
        }
        default: {
            continue;
            break;
        }}

        if (written == 0) {
            b.Grow(len(s));
        }
        b.WriteString(s[(int)(written)..(int)(i)]);
        b.WriteString(repl);
        written = i + w;
    }
    if (written == 0) {
        return s;
    }
    b.WriteString(s[(int)(written)..]);
    return b.String();
}

// "\v" == "v" on IE 6.
internal static slice<@string> lowUnicodeReplacementTable = new slice<@string>(93){
    [0] = @"\u0000"u8, [1] = @"\u0001"u8, [2] = @"\u0002"u8, [3] = @"\u0003"u8, [4] = @"\u0004"u8, [5] = @"\u0005"u8, [6] = @"\u0006"u8,
    [(rune)'\'] = @"\u0007"u8,
    [(rune)'\'] = @"\u0008"u8,
    [(rune)'\'] = @"\t"u8,
    [(rune)'\'] = @"\n"u8,
    [(rune)'\'] = @"\u000b"u8,
    [(rune)'\'] = @"\f"u8,
    [(rune)'\'] = @"\r"u8,
    [14] = @"\u000e"u8, [15] = @"\u000f"u8, [16] = @"\u0010"u8, [17] = @"\u0011"u8, [18] = @"\u0012"u8, [19] = @"\u0013"u8,
    [20] = @"\u0014"u8, [21] = @"\u0015"u8, [22] = @"\u0016"u8, [23] = @"\u0017"u8, [24] = @"\u0018"u8, [25] = @"\u0019"u8,
    [26] = @"\u001a"u8, [27] = @"\u001b"u8, [28] = @"\u001c"u8, [29] = @"\u001d"u8, [30] = @"\u001e"u8, [31] = @"\u001f"u8
};

// "\v" == "v" on IE 6.
// Encode HTML specials as hex so the output can be embedded
// in HTML attributes without further encoding.
internal static slice<@string> jsStrReplacementTable = new slice<@string>(97){
    [0] = @"\u0000"u8,
    [(rune)'\'] = @"\t"u8,
    [(rune)'\'] = @"\n"u8,
    [(rune)'\'] = @"\u000b"u8,
    [(rune)'\'] = @"\f"u8,
    [(rune)'\'] = @"\r"u8,
    [(rune)'"'] = @"\u0022"u8,
    [(rune)'`'] = @"\u0060"u8,
    [(rune)'&'] = @"\u0026"u8,
    [(rune)'\'] = @"\u0027"u8,
    [(rune)'+'] = @"\u002b"u8,
    [(rune)'/'] = @"\/"u8,
    [(rune)'<'] = @"\u003c"u8,
    [(rune)'>'] = @"\u003e"u8,
    [(rune)'\'] = @"\\"u8
};

// "\v" == "v" on IE 6.
// Encode HTML specials as hex so the output can be embedded
// in HTML attributes without further encoding.
// jsBqStrReplacementTable is like jsStrReplacementTable except it also contains
// the special characters for JS template literals: $, {, and }.
internal static slice<@string> jsBqStrReplacementTable = new slice<@string>(126){
    [0] = @"\u0000"u8,
    [(rune)'\'] = @"\t"u8,
    [(rune)'\'] = @"\n"u8,
    [(rune)'\'] = @"\u000b"u8,
    [(rune)'\'] = @"\f"u8,
    [(rune)'\'] = @"\r"u8,
    [(rune)'"'] = @"\u0022"u8,
    [(rune)'`'] = @"\u0060"u8,
    [(rune)'&'] = @"\u0026"u8,
    [(rune)'\'] = @"\u0027"u8,
    [(rune)'+'] = @"\u002b"u8,
    [(rune)'/'] = @"\/"u8,
    [(rune)'<'] = @"\u003c"u8,
    [(rune)'>'] = @"\u003e"u8,
    [(rune)'\'] = @"\\"u8,
    [(rune)'$'] = @"\u0024"u8,
    [(rune)'{'] = @"\u007b"u8,
    [(rune)'}'] = @"\u007d"u8
};

// "\v" == "v" on IE 6.
// Encode HTML specials as hex so the output can be embedded
// in HTML attributes without further encoding.
// jsStrNormReplacementTable is like jsStrReplacementTable but does not
// overencode existing escapes since this table has no entry for `\`.
internal static slice<@string> jsStrNormReplacementTable = new slice<@string>(97){
    [0] = @"\u0000"u8,
    [(rune)'\'] = @"\t"u8,
    [(rune)'\'] = @"\n"u8,
    [(rune)'\'] = @"\u000b"u8,
    [(rune)'\'] = @"\f"u8,
    [(rune)'\'] = @"\r"u8,
    [(rune)'"'] = @"\u0022"u8,
    [(rune)'&'] = @"\u0026"u8,
    [(rune)'\'] = @"\u0027"u8,
    [(rune)'`'] = @"\u0060"u8,
    [(rune)'+'] = @"\u002b"u8,
    [(rune)'/'] = @"\/"u8,
    [(rune)'<'] = @"\u003c"u8,
    [(rune)'>'] = @"\u003e"u8
};

// "\v" == "v" on IE 6.
// Encode HTML specials as hex so the output can be embedded
// in HTML attributes without further encoding.
internal static slice<@string> jsRegexpReplacementTable = new slice<@string>(126){
    [0] = @"\u0000"u8,
    [(rune)'\'] = @"\t"u8,
    [(rune)'\'] = @"\n"u8,
    [(rune)'\'] = @"\u000b"u8,
    [(rune)'\'] = @"\f"u8,
    [(rune)'\'] = @"\r"u8,
    [(rune)'"'] = @"\u0022"u8,
    [(rune)'$'] = @"\$"u8,
    [(rune)'&'] = @"\u0026"u8,
    [(rune)'\'] = @"\u0027"u8,
    [(rune)'('] = @"\("u8,
    [(rune)')'] = @"\)"u8,
    [(rune)'*'] = @"\*"u8,
    [(rune)'+'] = @"\u002b"u8,
    [(rune)'-'] = @"\-"u8,
    [(rune)'.'] = @"\."u8,
    [(rune)'/'] = @"\/"u8,
    [(rune)'<'] = @"\u003c"u8,
    [(rune)'>'] = @"\u003e"u8,
    [(rune)'?'] = @"\?"u8,
    [(rune)'['] = @"\["u8,
    [(rune)'\'] = @"\\"u8,
    [(rune)']'] = @"\]"u8,
    [(rune)'^'] = @"\^"u8,
    [(rune)'{'] = @"\{"u8,
    [(rune)'|'] = @"\|"u8,
    [(rune)'}'] = @"\}"u8
};

// isJSIdentPart reports whether the given rune is a JS identifier part.
// It does not handle all the non-Latin letters, joiners, and combining marks,
// but it does handle every codepoint that can occur in a numeric literal or
// a keyword.
internal static bool isJSIdentPart(rune r) {
    switch (ᐧ) {
    case {} when r is (rune)'$': {
        return true;
    }
    case {} when (rune)'0' <= r && r <= (rune)'9': {
        return true;
    }
    case {} when (rune)'A' <= r && r <= (rune)'Z': {
        return true;
    }
    case {} when r is (rune)'_': {
        return true;
    }
    case {} when (rune)'a' <= r && r <= (rune)'z': {
        return true;
    }}

    return false;
}

// isJSType reports whether the given MIME type should be considered JavaScript.
//
// It is used to determine whether a script tag with a type attribute is a javascript container.
internal static bool isJSType(@string mimeType) {
    // per
    //   https://www.w3.org/TR/html5/scripting-1.html#attr-script-type
    //   https://tools.ietf.org/html/rfc7231#section-3.1.1
    //   https://tools.ietf.org/html/rfc4329#section-3
    //   https://www.ietf.org/rfc/rfc4627.txt
    // discard parameters
    (mimeType, _, _) = strings.Cut(mimeType, ";"u8);
    mimeType = strings.ToLower(mimeType);
    mimeType = strings.TrimSpace(mimeType);
    var exprᴛ1 = mimeType;
    if (exprᴛ1 == "application/ecmascript"u8 || exprᴛ1 == "application/javascript"u8 || exprᴛ1 == "application/json"u8 || exprᴛ1 == "application/ld+json"u8 || exprᴛ1 == "application/x-ecmascript"u8 || exprᴛ1 == "application/x-javascript"u8 || exprᴛ1 == "module"u8 || exprᴛ1 == "text/ecmascript"u8 || exprᴛ1 == "text/javascript"u8 || exprᴛ1 == "text/javascript1.0"u8 || exprᴛ1 == "text/javascript1.1"u8 || exprᴛ1 == "text/javascript1.2"u8 || exprᴛ1 == "text/javascript1.3"u8 || exprᴛ1 == "text/javascript1.4"u8 || exprᴛ1 == "text/javascript1.5"u8 || exprᴛ1 == "text/jscript"u8 || exprᴛ1 == "text/livescript"u8 || exprᴛ1 == "text/x-ecmascript"u8 || exprᴛ1 == "text/x-javascript"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

} // end template_package
