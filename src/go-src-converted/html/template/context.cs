// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:34:30 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\context.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // context describes the state an HTML parser must be in when it reaches the
        // portion of HTML produced by evaluating a particular template node.
        //
        // The zero value of type context is the start context for a template that
        // produces an HTML fragment as defined at
        // http://www.w3.org/TR/html5/syntax.html#the-end
        // where the context element is null.
        private partial struct context
        {
            public state state;
            public delim delim;
            public urlPart urlPart;
            public jsCtx jsCtx;
            public attr attr;
            public element element;
            public ptr<Error> err;
        }

        private static @string String(this context c)
        {
            return fmt.Sprintf("{%v %v %v %v %v %v %v}", c.state, c.delim, c.urlPart, c.jsCtx, c.attr, c.element, c.err);
        }

        // eq reports whether two contexts are equal.
        private static bool eq(this context c, context d)
        {
            return c.state == d.state && c.delim == d.delim && c.urlPart == d.urlPart && c.jsCtx == d.jsCtx && c.attr == d.attr && c.element == d.element && c.err == d.err;
        }

        // mangle produces an identifier that includes a suffix that distinguishes it
        // from template names mangled with different contexts.
        private static @string mangle(this context c, @string templateName)
        { 
            // The mangled name for the default context is the input templateName.
            if (c.state == stateText)
            {
                return templateName;
            }
            var s = templateName + "$htmltemplate_" + c.state.String();
            if (c.delim != 0L)
            {
                s += "_" + c.delim.String();
            }
            if (c.urlPart != 0L)
            {
                s += "_" + c.urlPart.String();
            }
            if (c.jsCtx != 0L)
            {
                s += "_" + c.jsCtx.String();
            }
            if (c.attr != 0L)
            {
                s += "_" + c.attr.String();
            }
            if (c.element != 0L)
            {
                s += "_" + c.element.String();
            }
            return s;
        }

        // state describes a high-level HTML parser state.
        //
        // It bounds the top of the element stack, and by extension the HTML insertion
        // mode, but also contains state that does not correspond to anything in the
        // HTML5 parsing algorithm because a single token production in the HTML
        // grammar may contain embedded actions in a template. For instance, the quoted
        // HTML attribute produced by
        //     <div title="Hello {{.World}}">
        // is a single token in HTML's grammar but in a template spans several nodes.
        private partial struct state // : byte
        {
        }

 
        // stateText is parsed character data. An HTML parser is in
        // this state when its parse position is outside an HTML tag,
        // directive, comment, and special element body.
        private static readonly state stateText = iota; 
        // stateTag occurs before an HTML attribute or the end of a tag.
        private static readonly var stateTag = 0; 
        // stateAttrName occurs inside an attribute name.
        // It occurs between the ^'s in ` ^name^ = value`.
        private static readonly var stateAttrName = 1; 
        // stateAfterName occurs after an attr name has ended but before any
        // equals sign. It occurs between the ^'s in ` name^ ^= value`.
        private static readonly var stateAfterName = 2; 
        // stateBeforeValue occurs after the equals sign but before the value.
        // It occurs between the ^'s in ` name =^ ^value`.
        private static readonly var stateBeforeValue = 3; 
        // stateHTMLCmt occurs inside an <!-- HTML comment -->.
        private static readonly var stateHTMLCmt = 4; 
        // stateRCDATA occurs inside an RCDATA element (<textarea> or <title>)
        // as described at http://www.w3.org/TR/html5/syntax.html#elements-0
        private static readonly var stateRCDATA = 5; 
        // stateAttr occurs inside an HTML attribute whose content is text.
        private static readonly var stateAttr = 6; 
        // stateURL occurs inside an HTML attribute whose content is a URL.
        private static readonly var stateURL = 7; 
        // stateSrcset occurs inside an HTML srcset attribute.
        private static readonly var stateSrcset = 8; 
        // stateJS occurs inside an event handler or script element.
        private static readonly var stateJS = 9; 
        // stateJSDqStr occurs inside a JavaScript double quoted string.
        private static readonly var stateJSDqStr = 10; 
        // stateJSSqStr occurs inside a JavaScript single quoted string.
        private static readonly var stateJSSqStr = 11; 
        // stateJSRegexp occurs inside a JavaScript regexp literal.
        private static readonly var stateJSRegexp = 12; 
        // stateJSBlockCmt occurs inside a JavaScript /* block comment */.
        private static readonly var stateJSBlockCmt = 13; 
        // stateJSLineCmt occurs inside a JavaScript // line comment.
        private static readonly var stateJSLineCmt = 14; 
        // stateCSS occurs inside a <style> element or style attribute.
        private static readonly var stateCSS = 15; 
        // stateCSSDqStr occurs inside a CSS double quoted string.
        private static readonly var stateCSSDqStr = 16; 
        // stateCSSSqStr occurs inside a CSS single quoted string.
        private static readonly var stateCSSSqStr = 17; 
        // stateCSSDqURL occurs inside a CSS double quoted url("...").
        private static readonly var stateCSSDqURL = 18; 
        // stateCSSSqURL occurs inside a CSS single quoted url('...').
        private static readonly var stateCSSSqURL = 19; 
        // stateCSSURL occurs inside a CSS unquoted url(...).
        private static readonly var stateCSSURL = 20; 
        // stateCSSBlockCmt occurs inside a CSS /* block comment */.
        private static readonly var stateCSSBlockCmt = 21; 
        // stateCSSLineCmt occurs inside a CSS // line comment.
        private static readonly var stateCSSLineCmt = 22; 
        // stateError is an infectious error state outside any valid
        // HTML/CSS/JS construct.
        private static readonly var stateError = 23;

        private static array<@string> stateNames = new array<@string>(InitKeyedValues<@string>((stateText, "stateText"), (stateTag, "stateTag"), (stateAttrName, "stateAttrName"), (stateAfterName, "stateAfterName"), (stateBeforeValue, "stateBeforeValue"), (stateHTMLCmt, "stateHTMLCmt"), (stateRCDATA, "stateRCDATA"), (stateAttr, "stateAttr"), (stateURL, "stateURL"), (stateSrcset, "stateSrcset"), (stateJS, "stateJS"), (stateJSDqStr, "stateJSDqStr"), (stateJSSqStr, "stateJSSqStr"), (stateJSRegexp, "stateJSRegexp"), (stateJSBlockCmt, "stateJSBlockCmt"), (stateJSLineCmt, "stateJSLineCmt"), (stateCSS, "stateCSS"), (stateCSSDqStr, "stateCSSDqStr"), (stateCSSSqStr, "stateCSSSqStr"), (stateCSSDqURL, "stateCSSDqURL"), (stateCSSSqURL, "stateCSSSqURL"), (stateCSSURL, "stateCSSURL"), (stateCSSBlockCmt, "stateCSSBlockCmt"), (stateCSSLineCmt, "stateCSSLineCmt"), (stateError, "stateError")));

        private static @string String(this state s)
        {
            if (int(s) < len(stateNames))
            {
                return stateNames[s];
            }
            return fmt.Sprintf("illegal state %d", int(s));
        }

        // isComment is true for any state that contains content meant for template
        // authors & maintainers, not for end-users or machines.
        private static bool isComment(state s)
        {

            if (s == stateHTMLCmt || s == stateJSBlockCmt || s == stateJSLineCmt || s == stateCSSBlockCmt || s == stateCSSLineCmt) 
                return true;
                        return false;
        }

        // isInTag return whether s occurs solely inside an HTML tag.
        private static bool isInTag(state s)
        {

            if (s == stateTag || s == stateAttrName || s == stateAfterName || s == stateBeforeValue || s == stateAttr) 
                return true;
                        return false;
        }

        // delim is the delimiter that will end the current HTML attribute.
        private partial struct delim // : byte
        {
        }

 
        // delimNone occurs outside any attribute.
        private static readonly delim delimNone = iota; 
        // delimDoubleQuote occurs when a double quote (") closes the attribute.
        private static readonly var delimDoubleQuote = 0; 
        // delimSingleQuote occurs when a single quote (') closes the attribute.
        private static readonly var delimSingleQuote = 1; 
        // delimSpaceOrTagEnd occurs when a space or right angle bracket (>)
        // closes the attribute.
        private static readonly var delimSpaceOrTagEnd = 2;

        private static array<@string> delimNames = new array<@string>(InitKeyedValues<@string>((delimNone, "delimNone"), (delimDoubleQuote, "delimDoubleQuote"), (delimSingleQuote, "delimSingleQuote"), (delimSpaceOrTagEnd, "delimSpaceOrTagEnd")));

        private static @string String(this delim d)
        {
            if (int(d) < len(delimNames))
            {
                return delimNames[d];
            }
            return fmt.Sprintf("illegal delim %d", int(d));
        }

        // urlPart identifies a part in an RFC 3986 hierarchical URL to allow different
        // encoding strategies.
        private partial struct urlPart // : byte
        {
        }

 
        // urlPartNone occurs when not in a URL, or possibly at the start:
        // ^ in "^http://auth/path?k=v#frag".
        private static readonly urlPart urlPartNone = iota; 
        // urlPartPreQuery occurs in the scheme, authority, or path; between the
        // ^s in "h^ttp://auth/path^?k=v#frag".
        private static readonly var urlPartPreQuery = 0; 
        // urlPartQueryOrFrag occurs in the query portion between the ^s in
        // "http://auth/path?^k=v#frag^".
        private static readonly var urlPartQueryOrFrag = 1; 
        // urlPartUnknown occurs due to joining of contexts both before and
        // after the query separator.
        private static readonly var urlPartUnknown = 2;

        private static array<@string> urlPartNames = new array<@string>(InitKeyedValues<@string>((urlPartNone, "urlPartNone"), (urlPartPreQuery, "urlPartPreQuery"), (urlPartQueryOrFrag, "urlPartQueryOrFrag"), (urlPartUnknown, "urlPartUnknown")));

        private static @string String(this urlPart u)
        {
            if (int(u) < len(urlPartNames))
            {
                return urlPartNames[u];
            }
            return fmt.Sprintf("illegal urlPart %d", int(u));
        }

        // jsCtx determines whether a '/' starts a regular expression literal or a
        // division operator.
        private partial struct jsCtx // : byte
        {
        }

 
        // jsCtxRegexp occurs where a '/' would start a regexp literal.
        private static readonly jsCtx jsCtxRegexp = iota; 
        // jsCtxDivOp occurs where a '/' would start a division operator.
        private static readonly var jsCtxDivOp = 0; 
        // jsCtxUnknown occurs where a '/' is ambiguous due to context joining.
        private static readonly var jsCtxUnknown = 1;

        private static @string String(this jsCtx c)
        {

            if (c == jsCtxRegexp) 
                return "jsCtxRegexp";
            else if (c == jsCtxDivOp) 
                return "jsCtxDivOp";
            else if (c == jsCtxUnknown) 
                return "jsCtxUnknown";
                        return fmt.Sprintf("illegal jsCtx %d", int(c));
        }

        // element identifies the HTML element when inside a start tag or special body.
        // Certain HTML element (for example <script> and <style>) have bodies that are
        // treated differently from stateText so the element type is necessary to
        // transition into the correct context at the end of a tag and to identify the
        // end delimiter for the body.
        private partial struct element // : byte
        {
        }

 
        // elementNone occurs outside a special tag or special element body.
        private static readonly element elementNone = iota; 
        // elementScript corresponds to the raw text <script> element
        // with JS MIME type or no type attribute.
        private static readonly var elementScript = 0; 
        // elementStyle corresponds to the raw text <style> element.
        private static readonly var elementStyle = 1; 
        // elementTextarea corresponds to the RCDATA <textarea> element.
        private static readonly var elementTextarea = 2; 
        // elementTitle corresponds to the RCDATA <title> element.
        private static readonly var elementTitle = 3;

        private static array<@string> elementNames = new array<@string>(InitKeyedValues<@string>((elementNone, "elementNone"), (elementScript, "elementScript"), (elementStyle, "elementStyle"), (elementTextarea, "elementTextarea"), (elementTitle, "elementTitle")));

        private static @string String(this element e)
        {
            if (int(e) < len(elementNames))
            {
                return elementNames[e];
            }
            return fmt.Sprintf("illegal element %d", int(e));
        }

        // attr identifies the current HTML attribute when inside the attribute,
        // that is, starting from stateAttrName until stateTag/stateText (exclusive).
        private partial struct attr // : byte
        {
        }

 
        // attrNone corresponds to a normal attribute or no attribute.
        private static readonly attr attrNone = iota; 
        // attrScript corresponds to an event handler attribute.
        private static readonly var attrScript = 0; 
        // attrScriptType corresponds to the type attribute in script HTML element
        private static readonly var attrScriptType = 1; 
        // attrStyle corresponds to the style attribute whose value is CSS.
        private static readonly var attrStyle = 2; 
        // attrURL corresponds to an attribute whose value is a URL.
        private static readonly var attrURL = 3; 
        // attrSrcset corresponds to a srcset attribute.
        private static readonly var attrSrcset = 4;

        private static array<@string> attrNames = new array<@string>(InitKeyedValues<@string>((attrNone, "attrNone"), (attrScript, "attrScript"), (attrScriptType, "attrScriptType"), (attrStyle, "attrStyle"), (attrURL, "attrURL"), (attrSrcset, "attrSrcset")));

        private static @string String(this attr a)
        {
            if (int(a) < len(attrNames))
            {
                return attrNames[a];
            }
            return fmt.Sprintf("illegal attr %d", int(a));
        }
    }
}}
