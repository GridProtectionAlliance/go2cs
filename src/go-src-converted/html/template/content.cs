// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:38:50 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\content.go
namespace go.html;

using fmt = fmt_package;
using reflect = reflect_package;


// Strings of content from a trusted source.

public static partial class template_package {

 
// CSS encapsulates known safe content that matches any of:
//   1. The CSS3 stylesheet production, such as `p { color: purple }`.
//   2. The CSS3 rule production, such as `a[href=~"https:"].foo#bar`.
//   3. CSS3 declaration productions, such as `color: red; margin: 2px`.
//   4. The CSS3 value production, such as `rgba(0, 0, 255, 127)`.
// See https://www.w3.org/TR/css3-syntax/#parsing and
// https://web.archive.org/web/20090211114933/http://w3.org/TR/css3-syntax#style
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct CSS { // : @string
} 

// HTML encapsulates a known safe HTML document fragment.
// It should not be used for HTML from a third-party, or HTML with
// unclosed tags or comments. The outputs of a sound HTML sanitizer
// and a template escaped by this package are fine for use with HTML.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct HTML { // : @string
} 

// HTMLAttr encapsulates an HTML attribute from a trusted source,
// for example, ` dir="ltr"`.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct HTMLAttr { // : @string
} 

// JS encapsulates a known safe EcmaScript5 Expression, for example,
// `(x + y * z())`.
// Template authors are responsible for ensuring that typed expressions
// do not break the intended precedence and that there is no
// statement/expression ambiguity as when passing an expression like
// "{ foo: bar() }\n['foo']()", which is both a valid Expression and a
// valid Program with a very different meaning.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
//
// Using JS to include valid but untrusted JSON is not safe.
// A safe alternative is to parse the JSON with json.Unmarshal and then
// pass the resultant object into the template, where it will be
// converted to sanitized JSON when presented in a JavaScript context.
public partial struct JS { // : @string
} 

// JSStr encapsulates a sequence of characters meant to be embedded
// between quotes in a JavaScript expression.
// The string must match a series of StringCharacters:
//   StringCharacter :: SourceCharacter but not `\` or LineTerminator
//                    | EscapeSequence
// Note that LineContinuations are not allowed.
// JSStr("foo\\nbar") is fine, but JSStr("foo\\\nbar") is not.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct JSStr { // : @string
} 

// URL encapsulates a known safe URL or URL substring (see RFC 3986).
// A URL like `javascript:checkThatFormNotEditedBeforeLeavingPage()`
// from a trusted source should go in the page, but by default dynamic
// `javascript:` URLs are filtered out since they are a frequently
// exploited injection vector.
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct URL { // : @string
} 

// Srcset encapsulates a known safe srcset attribute
// (see https://w3c.github.io/html/semantics-embedded-content.html#element-attrdef-img-srcset).
//
// Use of this type presents a security risk:
// the encapsulated content should come from a trusted source,
// as it will be included verbatim in the template output.
public partial struct Srcset { // : @string
}private partial struct contentType { // : byte
}

private static readonly contentType contentTypePlain = iota;
private static readonly var contentTypeCSS = 0;
private static readonly var contentTypeHTML = 1;
private static readonly var contentTypeHTMLAttr = 2;
private static readonly var contentTypeJS = 3;
private static readonly var contentTypeJSStr = 4;
private static readonly var contentTypeURL = 5;
private static readonly var contentTypeSrcset = 6; 
// contentTypeUnsafe is used in attr.go for values that affect how
// embedded content and network messages are formed, vetted,
// or interpreted; or which credentials network messages carry.
private static readonly var contentTypeUnsafe = 7;

// indirect returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil).
private static void indirect(object a) {
    if (a == null) {
        return null;
    }
    {
        var t = reflect.TypeOf(a);

        if (t.Kind() != reflect.Ptr) { 
            // Avoid creating a reflect.Value if it's not a pointer.
            return a;
        }
    }
    var v = reflect.ValueOf(a);
    while (v.Kind() == reflect.Ptr && !v.IsNil()) {
        v = v.Elem();
    }
    return v.Interface();
}

private static var errorType = reflect.TypeOf((error.val)(null)).Elem();private static var fmtStringerType = reflect.TypeOf((fmt.Stringer.val)(null)).Elem();

// indirectToStringerOrError returns the value, after dereferencing as many times
// as necessary to reach the base type (or nil) or an implementation of fmt.Stringer
// or error,
private static void indirectToStringerOrError(object a) {
    if (a == null) {
        return null;
    }
    var v = reflect.ValueOf(a);
    while (!v.Type().Implements(fmtStringerType) && !v.Type().Implements(errorType) && v.Kind() == reflect.Ptr && !v.IsNil()) {
        v = v.Elem();
    }
    return v.Interface();
}

// stringify converts its arguments to a string and the type of the content.
// All pointers are dereferenced, as in the text/template package.
private static (@string, contentType) stringify(params object[] args) {
    @string _p0 = default;
    contentType _p0 = default;
    args = args.Clone();

    if (len(args) == 1) {
        switch (indirect(args[0]).type()) {
            case @string s:
                return (s, contentTypePlain);
                break;
            case CSS s:
                return (string(s), contentTypeCSS);
                break;
            case HTML s:
                return (string(s), contentTypeHTML);
                break;
            case HTMLAttr s:
                return (string(s), contentTypeHTMLAttr);
                break;
            case JS s:
                return (string(s), contentTypeJS);
                break;
            case JSStr s:
                return (string(s), contentTypeJSStr);
                break;
            case URL s:
                return (string(s), contentTypeURL);
                break;
            case Srcset s:
                return (string(s), contentTypeSrcset);
                break;
        }
    }
    nint i = 0;
    foreach (var (_, arg) in args) { 
        // We skip untyped nil arguments for backward compatibility.
        // Without this they would be output as <nil>, escaped.
        // See issue 25875.
        if (arg == null) {
            continue;
        }
        args[i] = indirectToStringerOrError(arg);
        i++;
    }    return (fmt.Sprint(args[..(int)i]), contentTypePlain);
}

} // end template_package
