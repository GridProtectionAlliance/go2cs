// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using parse = text.template.parse_package;
using text.template;

partial class template_package {

// context describes the state an HTML parser must be in when it reaches the
// portion of HTML produced by evaluating a particular template node.
//
// The zero value of type context is the start context for a template that
// produces an HTML fragment as defined at
// https://www.w3.org/TR/html5/syntax.html#the-end
// where the context element is null.
[GoType] partial struct context {
    internal state state;
    internal delim delim;
    internal urlPart urlPart;
    internal jsCtx jsCtx;
    // jsBraceDepth contains the current depth, for each JS template literal
    // string interpolation expression, of braces we've seen. This is used to
    // determine if the next } will close a JS template literal string
    // interpolation expression or not.
    internal slice<nint> jsBraceDepth;
    internal attr attr;
    internal element element;
    internal text.template.parse_package.Node n; // for range break/continue
    internal ж<ΔError> err;
}

internal static @string String(this context c) {
    error err = default!;
    if (c.err != nil) {
        err = ~c.err;
    }
    return fmt.Sprintf("{%v %v %v %v %v %v %v}"u8, c.state, c.delim, c.urlPart, c.jsCtx, c.attr, c.element, err);
}

// eq reports whether two contexts are equal.
internal static bool eq(this context c, context d) {
    return c.state == d.state && c.delim == d.delim && c.urlPart == d.urlPart && c.jsCtx == d.jsCtx && c.attr == d.attr && c.element == d.element && c.err == d.err;
}

// mangle produces an identifier that includes a suffix that distinguishes it
// from template names mangled with different contexts.
internal static @string mangle(this context c, @string templateName) {
    // The mangled name for the default context is the input templateName.
    if (c.state == stateText) {
        return templateName;
    }
    @string s = templateName + "$htmltemplate_"u8 + c.state.String();
    if (c.delim != delimNone) {
        s += "_"u8 + c.delim.String();
    }
    if (c.urlPart != urlPartNone) {
        s += "_"u8 + c.urlPart.String();
    }
    if (c.jsCtx != jsCtxRegexp) {
        s += "_"u8 + c.jsCtx.String();
    }
    if (c.attr != attrNone) {
        s += "_"u8 + c.attr.String();
    }
    if (c.element != elementNone) {
        s += "_"u8 + c.element.String();
    }
    return s;
}

[GoType("num:uint8")] partial struct state;

//go:generate stringer -type state
internal static readonly state stateText = /* iota */ 0;
internal static readonly state stateTag = 1;
internal static readonly state stateAttrName = 2;
internal static readonly state stateAfterName = 3;
internal static readonly state stateBeforeValue = 4;
internal static readonly state stateHTMLCmt = 5;
internal static readonly state stateRCDATA = 6;
internal static readonly state stateAttr = 7;
internal static readonly state stateURL = 8;
internal static readonly state stateSrcset = 9;
internal static readonly state stateJS = 10;
internal static readonly state stateJSDqStr = 11;
internal static readonly state stateJSSqStr = 12;
internal static readonly state stateJSTmplLit = 13;
internal static readonly state stateJSRegexp = 14;
internal static readonly state stateJSBlockCmt = 15;
internal static readonly state stateJSLineCmt = 16;
internal static readonly state stateJSHTMLOpenCmt = 17;
internal static readonly state stateJSHTMLCloseCmt = 18;
internal static readonly state stateCSS = 19;
internal static readonly state stateCSSDqStr = 20;
internal static readonly state stateCSSSqStr = 21;
internal static readonly state stateCSSDqURL = 22;
internal static readonly state stateCSSSqURL = 23;
internal static readonly state stateCSSURL = 24;
internal static readonly state stateCSSBlockCmt = 25;
internal static readonly state stateCSSLineCmt = 26;
internal static readonly state stateError = 27;
internal static readonly state stateDead = 28;

// isComment is true for any state that contains content meant for template
// authors & maintainers, not for end-users or machines.
internal static bool isComment(state s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == stateHTMLCmt || exprᴛ1 == stateJSBlockCmt || exprᴛ1 == stateJSLineCmt || exprᴛ1 == stateJSHTMLOpenCmt || exprᴛ1 == stateJSHTMLCloseCmt || exprᴛ1 == stateCSSBlockCmt || exprᴛ1 == stateCSSLineCmt) {
        return true;
    }

    return false;
}

// isInTag return whether s occurs solely inside an HTML tag.
internal static bool isInTag(state s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == stateTag || exprᴛ1 == stateAttrName || exprᴛ1 == stateAfterName || exprᴛ1 == stateBeforeValue || exprᴛ1 == stateAttr) {
        return true;
    }

    return false;
}

// isInScriptLiteral returns true if s is one of the literal states within a
// <script> tag, and as such occurrences of "<!--", "<script", and "</script"
// need to be treated specially.
internal static bool isInScriptLiteral(state s) {
    // Ignore the comment states (stateJSBlockCmt, stateJSLineCmt,
    // stateJSHTMLOpenCmt, stateJSHTMLCloseCmt) because their content is already
    // omitted from the output.
    var exprᴛ1 = s;
    if (exprᴛ1 == stateJSDqStr || exprᴛ1 == stateJSSqStr || exprᴛ1 == stateJSTmplLit || exprᴛ1 == stateJSRegexp) {
        return true;
    }

    return false;
}

[GoType("num:uint8")] partial struct delim;

//go:generate stringer -type delim
internal static readonly delim delimNone = /* iota */ 0;
internal static readonly delim delimDoubleQuote = 1;
internal static readonly delim delimSingleQuote = 2;
internal static readonly delim delimSpaceOrTagEnd = 3;

[GoType("num:uint8")] partial struct urlPart;

//go:generate stringer -type urlPart
internal static readonly urlPart urlPartNone = /* iota */ 0;
internal static readonly urlPart urlPartPreQuery = 1;
internal static readonly urlPart urlPartQueryOrFrag = 2;
internal static readonly urlPart urlPartUnknown = 3;

[GoType("num:uint8")] partial struct jsCtx;

//go:generate stringer -type jsCtx
internal static readonly jsCtx jsCtxRegexp = /* iota */ 0;
internal static readonly jsCtx jsCtxDivOp = 1;
internal static readonly jsCtx jsCtxUnknown = 2;

[GoType("num:uint8")] partial struct element;

//go:generate stringer -type element
internal static readonly element elementNone = /* iota */ 0;
internal static readonly element elementScript = 1;
internal static readonly element elementStyle = 2;
internal static readonly element elementTextarea = 3;
internal static readonly element elementTitle = 4;

[GoType("num:uint8")] partial struct attr;

//go:generate stringer -type attr
internal static readonly attr attrNone = /* iota */ 0;
internal static readonly attr attrScript = 1;
internal static readonly attr attrScriptType = 2;
internal static readonly attr attrStyle = 3;
internal static readonly attr attrURL = 4;
internal static readonly attr attrSrcset = 5;

} // end template_package
