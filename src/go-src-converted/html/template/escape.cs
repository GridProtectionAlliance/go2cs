// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:39:04 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\escape.go
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using html = html_package;
using io = io_package;
using template = text.template_package;
using parse = text.template.parse_package;


// escapeTemplate rewrites the named template, which must be
// associated with t, to guarantee that the output of any of the named
// templates is properly escaped. If no error is returned, then the named templates have
// been modified. Otherwise the named templates have been rendered
// unusable.

using System;
public static partial class template_package {

private static error escapeTemplate(ptr<Template> _addr_tmpl, parse.Node node, @string name) {
    ref Template tmpl = ref _addr_tmpl.val;

    var (c, _) = tmpl.esc.escapeTree(new context(), node, name, 0);
    error err = default!;
    if (c.err != null) {
        (err, c.err.Name) = (c.err, name);
    }
    else if (c.state != stateText) {
        err = error.As(addr(new Error(ErrEndContext,nil,name,0,fmt.Sprintf("ends in a non-text context: %v",c))))!;
    }
    if (err != null) { 
        // Prevent execution of unsafe templates.
        {
            var t__prev2 = t;

            var t = tmpl.set[name];

            if (t != null) {
                t.escapeErr = err;
                t.text.Tree = null;
                t.Tree = null;
            }
            t = t__prev2;

        }
        return error.As(err)!;
    }
    tmpl.esc.commit();
    {
        var t__prev1 = t;

        t = tmpl.set[name];

        if (t != null) {
            t.escapeErr = escapeOK;
            t.Tree = t.text.Tree;
        }
        t = t__prev1;

    }
    return error.As(null!)!;
}

// evalArgs formats the list of arguments into a string. It is equivalent to
// fmt.Sprint(args...), except that it deferences all pointers.
private static @string evalArgs(params object[] args) {
    args = args.Clone();
 
    // Optimization for simple common case of a single string argument.
    if (len(args) == 1) {
        {
            @string (s, ok) = args[0]._<@string>();

            if (ok) {
                return s;
            }

        }
    }
    foreach (var (i, arg) in args) {
        args[i] = indirectToStringerOrError(arg);
    }    return fmt.Sprint(args);
}

// funcMap maps command names to functions that render their inputs safe.
private static template.FuncMap funcMap = new template.FuncMap("_html_template_attrescaper":attrEscaper,"_html_template_commentescaper":commentEscaper,"_html_template_cssescaper":cssEscaper,"_html_template_cssvaluefilter":cssValueFilter,"_html_template_htmlnamefilter":htmlNameFilter,"_html_template_htmlescaper":htmlEscaper,"_html_template_jsregexpescaper":jsRegexpEscaper,"_html_template_jsstrescaper":jsStrEscaper,"_html_template_jsvalescaper":jsValEscaper,"_html_template_nospaceescaper":htmlNospaceEscaper,"_html_template_rcdataescaper":rcdataEscaper,"_html_template_srcsetescaper":srcsetFilterAndEscaper,"_html_template_urlescaper":urlEscaper,"_html_template_urlfilter":urlFilter,"_html_template_urlnormalizer":urlNormalizer,"_eval_args_":evalArgs,);

// escaper collects type inferences about templates and changes needed to make
// templates injection safe.
private partial struct escaper {
    public ptr<nameSpace> ns; // output[templateName] is the output context for a templateName that
// has been mangled to include its input context.
    public map<@string, context> output; // derived[c.mangle(name)] maps to a template derived from the template
// named name templateName for the start context c.
    public map<@string, ptr<template.Template>> derived; // called[templateName] is a set of called mangled template names.
    public map<@string, bool> called; // xxxNodeEdits are the accumulated edits to apply during commit.
// Such edits are not applied immediately in case a template set
// executes a given template in different escaping contexts.
    public map<ptr<parse.ActionNode>, slice<@string>> actionNodeEdits;
    public map<ptr<parse.TemplateNode>, @string> templateNodeEdits;
    public map<ptr<parse.TextNode>, slice<byte>> textNodeEdits;
}

// makeEscaper creates a blank escaper for the given set.
private static escaper makeEscaper(ptr<nameSpace> _addr_n) {
    ref nameSpace n = ref _addr_n.val;

    return new escaper(n,map[string]context{},map[string]*template.Template{},map[string]bool{},map[*parse.ActionNode][]string{},map[*parse.TemplateNode]string{},map[*parse.TextNode][]byte{},);
}

// filterFailsafe is an innocuous word that is emitted in place of unsafe values
// by sanitizer functions. It is not a keyword in any programming language,
// contains no special characters, is not empty, and when it appears in output
// it is distinct enough that a developer can find the source of the problem
// via a search engine.
private static readonly @string filterFailsafe = "ZgotmplZ";

// escape escapes a template node.


// escape escapes a template node.
private static context escape(this ptr<escaper> _addr_e, context c, parse.Node n) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;

    switch (n.type()) {
        case ptr<parse.ActionNode> n:
            return e.escapeAction(c, n);
            break;
        case ptr<parse.CommentNode> n:
            return c;
            break;
        case ptr<parse.IfNode> n:
            return e.escapeBranch(c, _addr_n.BranchNode, "if");
            break;
        case ptr<parse.ListNode> n:
            return e.escapeList(c, n);
            break;
        case ptr<parse.RangeNode> n:
            return e.escapeBranch(c, _addr_n.BranchNode, "range");
            break;
        case ptr<parse.TemplateNode> n:
            return e.escapeTemplate(c, n);
            break;
        case ptr<parse.TextNode> n:
            return e.escapeText(c, n);
            break;
        case ptr<parse.WithNode> n:
            return e.escapeBranch(c, _addr_n.BranchNode, "with");
            break;
    }
    panic("escaping " + n.String() + " is unimplemented");
});

// escapeAction escapes an action template node.
private static context escapeAction(this ptr<escaper> _addr_e, context c, ptr<parse.ActionNode> _addr_n) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;
    ref parse.ActionNode n = ref _addr_n.val;

    if (len(n.Pipe.Decl) != 0) { 
        // A local variable assignment, not an interpolation.
        return c;
    }
    c = nudge(c); 
    // Check for disallowed use of predefined escapers in the pipeline.
    foreach (var (pos, idNode) in n.Pipe.Cmds) {
        ptr<parse.IdentifierNode> (node, ok) = idNode.Args[0]._<ptr<parse.IdentifierNode>>();
        if (!ok) { 
            // A predefined escaper "esc" will never be found as an identifier in a
            // Chain or Field node, since:
            // - "esc.x ..." is invalid, since predefined escapers return strings, and
            //   strings do not have methods, keys or fields.
            // - "... .esc" is invalid, since predefined escapers are global functions,
            //   not methods or fields of any types.
            // Therefore, it is safe to ignore these two node types.
            continue;
        }
        var ident = node.Ident;
        {
            var (_, ok) = predefinedEscapers[ident];

            if (ok) {
                if (pos < len(n.Pipe.Cmds) - 1 || c.state == stateAttr && c.delim == delimSpaceOrTagEnd && ident == "html") {
                    return new context(state:stateError,err:errorf(ErrPredefinedEscaper,n,n.Line,"predefined escaper %q disallowed in template",ident),);
                }
            }

        }
    }    var s = make_slice<@string>(0, 3);

    if (c.state == stateError) 
        return c;
    else if (c.state == stateURL || c.state == stateCSSDqStr || c.state == stateCSSSqStr || c.state == stateCSSDqURL || c.state == stateCSSSqURL || c.state == stateCSSURL) 

        if (c.urlPart == urlPartNone)
        {
            s = append(s, "_html_template_urlfilter");
            fallthrough = true;
        }
        if (fallthrough || c.urlPart == urlPartPreQuery)
        {

            if (c.state == stateCSSDqStr || c.state == stateCSSSqStr) 
                s = append(s, "_html_template_cssescaper");
            else 
                s = append(s, "_html_template_urlnormalizer");
                        goto __switch_break0;
        }
        if (c.urlPart == urlPartQueryOrFrag)
        {
            s = append(s, "_html_template_urlescaper");
            goto __switch_break0;
        }
        if (c.urlPart == urlPartUnknown)
        {
            return new context(state:stateError,err:errorf(ErrAmbigContext,n,n.Line,"%s appears in an ambiguous context within a URL",n),);
            goto __switch_break0;
        }
        // default: 
            panic(c.urlPart.String());

        __switch_break0:;
    else if (c.state == stateJS) 
        s = append(s, "_html_template_jsvalescaper"); 
        // A slash after a value starts a div operator.
        c.jsCtx = jsCtxDivOp;
    else if (c.state == stateJSDqStr || c.state == stateJSSqStr) 
        s = append(s, "_html_template_jsstrescaper");
    else if (c.state == stateJSRegexp) 
        s = append(s, "_html_template_jsregexpescaper");
    else if (c.state == stateCSS) 
        s = append(s, "_html_template_cssvaluefilter");
    else if (c.state == stateText) 
        s = append(s, "_html_template_htmlescaper");
    else if (c.state == stateRCDATA) 
        s = append(s, "_html_template_rcdataescaper");
    else if (c.state == stateAttr)     else if (c.state == stateAttrName || c.state == stateTag) 
        c.state = stateAttrName;
        s = append(s, "_html_template_htmlnamefilter");
    else if (c.state == stateSrcset) 
        s = append(s, "_html_template_srcsetescaper");
    else 
        if (isComment(c.state)) {
            s = append(s, "_html_template_commentescaper");
        }
        else
 {
            panic("unexpected state " + c.state.String());
        }
    
    if (c.delim == delimNone)     else if (c.delim == delimSpaceOrTagEnd) 
        s = append(s, "_html_template_nospaceescaper");
    else 
        s = append(s, "_html_template_attrescaper");
        e.editActionNode(n, s);
    return c;
});

// ensurePipelineContains ensures that the pipeline ends with the commands with
// the identifiers in s in order. If the pipeline ends with a predefined escaper
// (i.e. "html" or "urlquery"), merge it with the identifiers in s.
private static void ensurePipelineContains(ptr<parse.PipeNode> _addr_p, slice<@string> s) {
    ref parse.PipeNode p = ref _addr_p.val;

    if (len(s) == 0) { 
        // Do not rewrite pipeline if we have no escapers to insert.
        return ;
    }
    var pipelineLen = len(p.Cmds);
    if (pipelineLen > 0) {
        var lastCmd = p.Cmds[pipelineLen - 1];
        {
            ptr<parse.IdentifierNode> idNode__prev2 = idNode;

            ptr<parse.IdentifierNode> (idNode, ok) = lastCmd.Args[0]._<ptr<parse.IdentifierNode>>();

            if (ok) {
                {
                    var esc = idNode.Ident;

                    if (predefinedEscapers[esc]) { 
                        // Pipeline ends with a predefined escaper.
                        if (len(p.Cmds) == 1 && len(lastCmd.Args) > 1) { 
                            // Special case: pipeline is of the form {{ esc arg1 arg2 ... argN }},
                            // where esc is the predefined escaper, and arg1...argN are its arguments.
                            // Convert this into the equivalent form
                            // {{ _eval_args_ arg1 arg2 ... argN | esc }}, so that esc can be easily
                            // merged with the escapers in s.
                            lastCmd.Args[0] = parse.NewIdentifier("_eval_args_").SetTree(null).SetPos(lastCmd.Args[0].Position());
                            p.Cmds = appendCmd(p.Cmds, _addr_newIdentCmd(esc, p.Position()));
                            pipelineLen++;
                        } 
                        // If any of the commands in s that we are about to insert is equivalent
                        // to the predefined escaper, use the predefined escaper instead.
                        var dup = false;
                        {
                            var i__prev1 = i;

                            foreach (var (__i, __escaper) in s) {
                                i = __i;
                                escaper = __escaper;
                                if (escFnsEq(esc, escaper)) {
                                    s[i] = idNode.Ident;
                                    dup = true;
                                }
                            }

                            i = i__prev1;
                        }

                        if (dup) { 
                            // The predefined escaper will already be inserted along with the
                            // escapers in s, so do not copy it to the rewritten pipeline.
                            pipelineLen--;
                        }
                    }

                }
            }

            idNode = idNode__prev2;

        }
    }
    var newCmds = make_slice<ptr<parse.CommandNode>>(pipelineLen, pipelineLen + len(s));
    var insertedIdents = make_map<@string, bool>();
    {
        var i__prev1 = i;

        for (nint i = 0; i < pipelineLen; i++) {
            var cmd = p.Cmds[i];
            newCmds[i] = cmd;
            {
                ptr<parse.IdentifierNode> idNode__prev1 = idNode;

                (idNode, ok) = cmd.Args[0]._<ptr<parse.IdentifierNode>>();

                if (ok) {
                    insertedIdents[normalizeEscFn(idNode.Ident)] = true;
                }

                idNode = idNode__prev1;

            }
        }

        i = i__prev1;
    }
    foreach (var (_, name) in s) {
        if (!insertedIdents[normalizeEscFn(name)]) { 
            // When two templates share an underlying parse tree via the use of
            // AddParseTree and one template is executed after the other, this check
            // ensures that escapers that were already inserted into the pipeline on
            // the first escaping pass do not get inserted again.
            newCmds = appendCmd(newCmds, _addr_newIdentCmd(name, p.Position()));
        }
    }    p.Cmds = newCmds;
}

// predefinedEscapers contains template predefined escapers that are equivalent
// to some contextual escapers. Keep in sync with equivEscapers.
private static map predefinedEscapers = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"html":true,"urlquery":true,};

// equivEscapers matches contextual escapers to equivalent predefined
// template escapers.
private static map equivEscapers = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"_html_template_attrescaper":"html","_html_template_htmlescaper":"html","_html_template_rcdataescaper":"html","_html_template_urlescaper":"urlquery","_html_template_urlnormalizer":"urlquery",};

// escFnsEq reports whether the two escaping functions are equivalent.
private static bool escFnsEq(@string a, @string b) {
    return normalizeEscFn(a) == normalizeEscFn(b);
}

// normalizeEscFn(a) is equal to normalizeEscFn(b) for any pair of names of
// escaper functions a and b that are equivalent.
private static @string normalizeEscFn(@string e) {
    {
        var norm = equivEscapers[e];

        if (norm != "") {
            return norm;
        }
    }
    return e;
}

// redundantFuncs[a][b] implies that funcMap[b](funcMap[a](x)) == funcMap[a](x)
// for all x.
private static map redundantFuncs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, map<@string, bool>>{"_html_template_commentescaper":{"_html_template_attrescaper":true,"_html_template_nospaceescaper":true,"_html_template_htmlescaper":true,},"_html_template_cssescaper":{"_html_template_attrescaper":true,},"_html_template_jsregexpescaper":{"_html_template_attrescaper":true,},"_html_template_jsstrescaper":{"_html_template_attrescaper":true,},"_html_template_urlescaper":{"_html_template_urlnormalizer":true,},};

// appendCmd appends the given command to the end of the command pipeline
// unless it is redundant with the last command.
private static slice<ptr<parse.CommandNode>> appendCmd(slice<ptr<parse.CommandNode>> cmds, ptr<parse.CommandNode> _addr_cmd) {
    ref parse.CommandNode cmd = ref _addr_cmd.val;

    {
        var n = len(cmds);

        if (n != 0) {
            ptr<parse.IdentifierNode> (last, okLast) = cmds[n - 1].Args[0]._<ptr<parse.IdentifierNode>>();
            ptr<parse.IdentifierNode> (next, okNext) = cmd.Args[0]._<ptr<parse.IdentifierNode>>();
            if (okLast && okNext && redundantFuncs[last.Ident][next.Ident]) {
                return cmds;
            }
        }
    }
    return append(cmds, cmd);
}

// newIdentCmd produces a command containing a single identifier node.
private static ptr<parse.CommandNode> newIdentCmd(@string identifier, parse.Pos pos) {
    return addr(new parse.CommandNode(NodeType:parse.NodeCommand,Args:[]parse.Node{parse.NewIdentifier(identifier).SetTree(nil).SetPos(pos)},));
}

// nudge returns the context that would result from following empty string
// transitions from the input context.
// For example, parsing:
//     `<a href=`
// will end in context{stateBeforeValue, attrURL}, but parsing one extra rune:
//     `<a href=x`
// will end in context{stateURL, delimSpaceOrTagEnd, ...}.
// There are two transitions that happen when the 'x' is seen:
// (1) Transition from a before-value state to a start-of-value state without
//     consuming any character.
// (2) Consume 'x' and transition past the first value character.
// In this case, nudging produces the context after (1) happens.
private static context nudge(context c) {

    if (c.state == stateTag) 
        // In `<foo {{.}}`, the action should emit an attribute.
        c.state = stateAttrName;
    else if (c.state == stateBeforeValue) 
        // In `<foo bar={{.}}`, the action is an undelimited value.
        (c.state, c.delim, c.attr) = (attrStartStates[c.attr], delimSpaceOrTagEnd, attrNone);    else if (c.state == stateAfterName) 
        // In `<foo bar {{.}}`, the action is an attribute name.
        (c.state, c.attr) = (stateAttrName, attrNone);        return c;
}

// join joins the two contexts of a branch template node. The result is an
// error context if either of the input contexts are error contexts, or if the
// input contexts differ.
private static context join(context a, context b, parse.Node node, @string nodeName) {
    if (a.state == stateError) {
        return a;
    }
    if (b.state == stateError) {
        return b;
    }
    if (a.eq(b)) {
        return a;
    }
    var c = a;
    c.urlPart = b.urlPart;
    if (c.eq(b)) { 
        // The contexts differ only by urlPart.
        c.urlPart = urlPartUnknown;
        return c;
    }
    c = a;
    c.jsCtx = b.jsCtx;
    if (c.eq(b)) { 
        // The contexts differ only by jsCtx.
        c.jsCtx = jsCtxUnknown;
        return c;
    }
    {
        var c__prev1 = c;

        c = nudge(a);
        var d = nudge(b);

        if (!(c.eq(a) && d.eq(b))) {
            {
                var e = join(c, d, node, nodeName);

                if (e.state != stateError) {
                    return e;
                }

            }
        }
        c = c__prev1;

    }

    return new context(state:stateError,err:errorf(ErrBranchEnd,node,0,"{{%s}} branches end in different contexts: %v, %v",nodeName,a,b),);
}

// escapeBranch escapes a branch template node: "if", "range" and "with".
private static context escapeBranch(this ptr<escaper> _addr_e, context c, ptr<parse.BranchNode> _addr_n, @string nodeName) {
    ref escaper e = ref _addr_e.val;
    ref parse.BranchNode n = ref _addr_n.val;

    var c0 = e.escapeList(c, n.List);
    if (nodeName == "range" && c0.state != stateError) { 
        // The "true" branch of a "range" node can execute multiple times.
        // We check that executing n.List once results in the same context
        // as executing n.List twice.
        var (c1, _) = e.escapeListConditionally(c0, n.List, null);
        c0 = join(c0, c1, n, nodeName);
        if (c0.state == stateError) { 
            // Make clear that this is a problem on loop re-entry
            // since developers tend to overlook that branch when
            // debugging templates.
            c0.err.Line = n.Line;
            c0.err.Description = "on range loop re-entry: " + c0.err.Description;
            return c0;
        }
    }
    var c1 = e.escapeList(c, n.ElseList);
    return join(c0, c1, n, nodeName);
}

// escapeList escapes a list template node.
private static context escapeList(this ptr<escaper> _addr_e, context c, ptr<parse.ListNode> _addr_n) {
    ref escaper e = ref _addr_e.val;
    ref parse.ListNode n = ref _addr_n.val;

    if (n == null) {
        return c;
    }
    foreach (var (_, m) in n.Nodes) {
        c = e.escape(c, m);
    }    return c;
}

// escapeListConditionally escapes a list node but only preserves edits and
// inferences in e if the inferences and output context satisfy filter.
// It returns the best guess at an output context, and the result of the filter
// which is the same as whether e was updated.
private static (context, bool) escapeListConditionally(this ptr<escaper> _addr_e, context c, ptr<parse.ListNode> _addr_n, Func<ptr<escaper>, context, bool> filter) {
    context _p0 = default;
    bool _p0 = default;
    ref escaper e = ref _addr_e.val;
    ref parse.ListNode n = ref _addr_n.val;

    ref var e1 = ref heap(makeEscaper(_addr_e.ns), out ptr<var> _addr_e1); 
    // Make type inferences available to f.
    {
        var k__prev1 = k;
        var v__prev1 = v;

        foreach (var (__k, __v) in e.output) {
            k = __k;
            v = __v;
            e1.output[k] = v;
        }
        k = k__prev1;
        v = v__prev1;
    }

    c = e1.escapeList(c, n);
    var ok = filter != null && filter(_addr_e1, c);
    if (ok) { 
        // Copy inferences and edits from e1 back into e.
        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.output) {
                k = __k;
                v = __v;
                e.output[k] = v;
            }

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.derived) {
                k = __k;
                v = __v;
                e.derived[k] = v;
            }

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.called) {
                k = __k;
                v = __v;
                e.called[k] = v;
            }

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.actionNodeEdits) {
                k = __k;
                v = __v;
                e.editActionNode(k, v);
            }

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.templateNodeEdits) {
                k = __k;
                v = __v;
                e.editTemplateNode(k, v);
            }

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in e1.textNodeEdits) {
                k = __k;
                v = __v;
                e.editTextNode(k, v);
            }

            k = k__prev1;
            v = v__prev1;
        }
    }
    return (c, ok);
}

// escapeTemplate escapes a {{template}} call node.
private static context escapeTemplate(this ptr<escaper> _addr_e, context c, ptr<parse.TemplateNode> _addr_n) {
    ref escaper e = ref _addr_e.val;
    ref parse.TemplateNode n = ref _addr_n.val;

    var (c, name) = e.escapeTree(c, n, n.Name, n.Line);
    if (name != n.Name) {
        e.editTemplateNode(n, name);
    }
    return c;
}

// escapeTree escapes the named template starting in the given context as
// necessary and returns its output context.
private static (context, @string) escapeTree(this ptr<escaper> _addr_e, context c, parse.Node node, @string name, nint line) {
    context _p0 = default;
    @string _p0 = default;
    ref escaper e = ref _addr_e.val;
 
    // Mangle the template name with the input context to produce a reliable
    // identifier.
    var dname = c.mangle(name);
    e.called[dname] = true;
    {
        var (out, ok) = e.output[dname];

        if (ok) { 
            // Already escaped.
            return (out, dname);
        }
    }
    var t = e.template(name);
    if (t == null) { 
        // Two cases: The template exists but is empty, or has never been mentioned at
        // all. Distinguish the cases in the error messages.
        if (e.ns.set[name] != null) {
            return (new context(state:stateError,err:errorf(ErrNoSuchTemplate,node,line,"%q is an incomplete or empty template",name),), dname);
        }
        return (new context(state:stateError,err:errorf(ErrNoSuchTemplate,node,line,"no such template %q",name),), dname);
    }
    if (dname != name) { 
        // Use any template derived during an earlier call to escapeTemplate
        // with different top level templates, or clone if necessary.
        var dt = e.template(dname);
        if (dt == null) {
            dt = template.New(dname);
            dt.Tree = addr(new parse.Tree(Name:dname,Root:t.Root.CopyList()));
            e.derived[dname] = dt;
        }
        t = dt;
    }
    return (e.computeOutCtx(c, t), dname);
}

// computeOutCtx takes a template and its start context and computes the output
// context while storing any inferences in e.
private static context computeOutCtx(this ptr<escaper> _addr_e, context c, ptr<template.Template> _addr_t) {
    ref escaper e = ref _addr_e.val;
    ref template.Template t = ref _addr_t.val;
 
    // Propagate context over the body.
    var (c1, ok) = e.escapeTemplateBody(c, t);
    if (!ok) { 
        // Look for a fixed point by assuming c1 as the output context.
        {
            var (c2, ok2) = e.escapeTemplateBody(c1, t);

            if (ok2) {
                (c1, ok) = (c2, true);
            } 
            // Use c1 as the error context if neither assumption worked.

        } 
        // Use c1 as the error context if neither assumption worked.
    }
    if (!ok && c1.state != stateError) {
        return new context(state:stateError,err:errorf(ErrOutputContext,t.Tree.Root,0,"cannot compute output context for template %s",t.Name()),);
    }
    return c1;
}

// escapeTemplateBody escapes the given template assuming the given output
// context, and returns the best guess at the output context and whether the
// assumption was correct.
private static (context, bool) escapeTemplateBody(this ptr<escaper> _addr_e, context c, ptr<template.Template> _addr_t) {
    context _p0 = default;
    bool _p0 = default;
    ref escaper e = ref _addr_e.val;
    ref template.Template t = ref _addr_t.val;

    Func<ptr<escaper>, context, bool> filter = (e1, c1) => {
        if (c1.state == stateError) { 
            // Do not update the input escaper, e.
            return false;
        }
        if (!e1.called[t.Name()]) { 
            // If t is not recursively called, then c1 is an
            // accurate output context.
            return true;
        }
        return c.eq(c1);
    }; 
    // We need to assume an output context so that recursive template calls
    // take the fast path out of escapeTree instead of infinitely recursing.
    // Naively assuming that the input context is the same as the output
    // works >90% of the time.
    e.output[t.Name()] = c;
    return e.escapeListConditionally(c, t.Tree.Root, filter);
}

// delimEnds maps each delim to a string of characters that terminate it.
private static array<@string> delimEnds = new array<@string>(InitKeyedValues<@string>((delimDoubleQuote, `"`), (delimSingleQuote, "'"), (delimSpaceOrTagEnd, " \t\n\f\r>")));

private static slice<byte> doctypeBytes = (slice<byte>)"<!DOCTYPE";

// escapeText escapes a text template node.
private static context escapeText(this ptr<escaper> _addr_e, context c, ptr<parse.TextNode> _addr_n) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;
    ref parse.TextNode n = ref _addr_n.val;

    var s = n.Text;
    nint written = 0;
    nint i = 0;
    ptr<object> b = @new<bytes.Buffer>();
    while (i != len(s)) {
        var (c1, nread) = contextAfterText(c, s[(int)i..]);
        var i1 = i + nread;
        if (c.state == stateText || c.state == stateRCDATA) {
            var end = i1;
            if (c1.state != c.state) {
                {
                    var j__prev2 = j;

                    for (var j = end - 1; j >= i; j--) {
                        if (s[j] == '<') {
                            end = j;
                            break;
                        }
                    }


                    j = j__prev2;
                }
            }
            {
                var j__prev2 = j;

                for (j = i; j < end; j++) {
                    if (s[j] == '<' && !bytes.HasPrefix(bytes.ToUpper(s[(int)j..]), doctypeBytes)) {
                        b.Write(s[(int)written..(int)j]);
                        b.WriteString("&lt;");
                        written = j + 1;
                    }
                }


                j = j__prev2;
            }
        }
        else if (isComment(c.state) && c.delim == delimNone) {

            if (c.state == stateJSBlockCmt) 
                // https://es5.github.com/#x7.4:
                // "Comments behave like white space and are
                // discarded except that, if a MultiLineComment
                // contains a line terminator character, then
                // the entire comment is considered to be a
                // LineTerminator for purposes of parsing by
                // the syntactic grammar."
                if (bytes.ContainsAny(s[(int)written..(int)i1], "\n\r\u2028\u2029")) {
                    b.WriteByte('\n');
                }
                else
 {
                    b.WriteByte(' ');
                }
            else if (c.state == stateCSSBlockCmt) 
                b.WriteByte(' ');
                        written = i1;
        }
        if (c.state != c1.state && isComment(c1.state) && c1.delim == delimNone) { 
            // Preserve the portion between written and the comment start.
            var cs = i1 - 2;
            if (c1.state == stateHTMLCmt) { 
                // "<!--" instead of "/*" or "//"
                cs -= 2;
            }
            b.Write(s[(int)written..(int)cs]);
            written = i1;
        }
        if (i == i1 && c.state == c1.state) {
            panic(fmt.Sprintf("infinite loop from %v to %v on %q..%q", c, c1, s[..(int)i], s[(int)i..]));
        }
        (c, i) = (c1, i1);
    }

    if (written != 0 && c.state != stateError) {
        if (!isComment(c.state) || c.delim != delimNone) {
            b.Write(n.Text[(int)written..]);
        }
        e.editTextNode(n, b.Bytes());
    }
    return c;
});

// contextAfterText starts in context c, consumes some tokens from the front of
// s, then returns the context after those tokens and the unprocessed suffix.
private static (context, nint) contextAfterText(context c, slice<byte> s) {
    context _p0 = default;
    nint _p0 = default;

    if (c.delim == delimNone) {
        var (c1, i) = tSpecialTagEnd(c, s);
        if (i == 0) { 
            // A special end tag (`</script>`) has been seen and
            // all content preceding it has been consumed.
            return (c1, 0);
        }
        return transitionFunc[c.state](c, s[..(int)i]);
    }
    var i = bytes.IndexAny(s, delimEnds[c.delim]);
    if (i == -1) {
        i = len(s);
    }
    if (c.delim == delimSpaceOrTagEnd) { 
        // https://www.w3.org/TR/html5/syntax.html#attribute-value-(unquoted)-state
        // lists the runes below as error characters.
        // Error out because HTML parsers may differ on whether
        // "<a id= onclick=f("     ends inside id's or onclick's value,
        // "<a class=`foo "        ends inside a value,
        // "<a style=font:'Arial'" needs open-quote fixup.
        // IE treats '`' as a quotation character.
        {
            var j = bytes.IndexAny(s[..(int)i], "\"'<=`");

            if (j >= 0) {
                return (new context(state:stateError,err:errorf(ErrBadHTML,nil,0,"%q in unquoted attr: %q",s[j:j+1],s[:i]),), len(s));
            }

        }
    }
    if (i == len(s)) { 
        // Remain inside the attribute.
        // Decode the value so non-HTML rules can easily handle
        //     <button onclick="alert(&quot;Hi!&quot;)">
        // without having to entity decode token boundaries.
        {
            slice<byte> u = (slice<byte>)html.UnescapeString(string(s));

            while (len(u) != 0) {
                var (c1, i1) = transitionFunc[c.state](c, u);
                (c, u) = (c1, u[(int)i1..]);
            }

        }
        return (c, len(s));
    }
    var element = c.element; 

    // If this is a non-JS "type" attribute inside "script" tag, do not treat the contents as JS.
    if (c.state == stateAttr && c.element == elementScript && c.attr == attrScriptType && !isJSType(string(s[..(int)i]))) {
        element = elementNone;
    }
    if (c.delim != delimSpaceOrTagEnd) { 
        // Consume any quote.
        i++;
    }
    return (new context(state:stateTag,element:element), i);
}

// editActionNode records a change to an action pipeline for later commit.
private static void editActionNode(this ptr<escaper> _addr_e, ptr<parse.ActionNode> _addr_n, slice<@string> cmds) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;
    ref parse.ActionNode n = ref _addr_n.val;

    {
        var (_, ok) = e.actionNodeEdits[n];

        if (ok) {
            panic(fmt.Sprintf("node %s shared between templates", n));
        }
    }
    e.actionNodeEdits[n] = cmds;
});

// editTemplateNode records a change to a {{template}} callee for later commit.
private static void editTemplateNode(this ptr<escaper> _addr_e, ptr<parse.TemplateNode> _addr_n, @string callee) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;
    ref parse.TemplateNode n = ref _addr_n.val;

    {
        var (_, ok) = e.templateNodeEdits[n];

        if (ok) {
            panic(fmt.Sprintf("node %s shared between templates", n));
        }
    }
    e.templateNodeEdits[n] = callee;
});

// editTextNode records a change to a text node for later commit.
private static void editTextNode(this ptr<escaper> _addr_e, ptr<parse.TextNode> _addr_n, slice<byte> text) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;
    ref parse.TextNode n = ref _addr_n.val;

    {
        var (_, ok) = e.textNodeEdits[n];

        if (ok) {
            panic(fmt.Sprintf("node %s shared between templates", n));
        }
    }
    e.textNodeEdits[n] = text;
});

// commit applies changes to actions and template calls needed to contextually
// autoescape content and adds any derived templates to the set.
private static void commit(this ptr<escaper> _addr_e) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;

    {
        var name__prev1 = name;

        foreach (var (__name) in e.output) {
            name = __name;
            e.template(name).Funcs(funcMap);
        }
        name = name__prev1;
    }

    var tmpl = e.arbitraryTemplate();
    foreach (var (_, t) in e.derived) {
        {
            var (_, err) = tmpl.text.AddParseTree(t.Name(), t.Tree);

            if (err != null) {
                panic("error adding derived template");
            }

        }
    }    {
        var n__prev1 = n;
        var s__prev1 = s;

        foreach (var (__n, __s) in e.actionNodeEdits) {
            n = __n;
            s = __s;
            ensurePipelineContains(_addr_n.Pipe, s);
        }
        n = n__prev1;
        s = s__prev1;
    }

    {
        var n__prev1 = n;
        var name__prev1 = name;

        foreach (var (__n, __name) in e.templateNodeEdits) {
            n = __n;
            name = __name;
            n.Name = name;
        }
        n = n__prev1;
        name = name__prev1;
    }

    {
        var n__prev1 = n;
        var s__prev1 = s;

        foreach (var (__n, __s) in e.textNodeEdits) {
            n = __n;
            s = __s;
            n.Text = s;
        }
        n = n__prev1;
        s = s__prev1;
    }

    e.called = make_map<@string, bool>();
    e.actionNodeEdits = make_map<ptr<parse.ActionNode>, slice<@string>>();
    e.templateNodeEdits = make_map<ptr<parse.TemplateNode>, @string>();
    e.textNodeEdits = make_map<ptr<parse.TextNode>, slice<byte>>();
});

// template returns the named template given a mangled template name.
private static ptr<template.Template> template(this ptr<escaper> _addr_e, @string name) {
    ref escaper e = ref _addr_e.val;
 
    // Any template from the name space associated with this escaper can be used
    // to look up templates in the underlying text/template name space.
    var t = e.arbitraryTemplate().text.Lookup(name);
    if (t == null) {
        t = e.derived[name];
    }
    return _addr_t!;
}

// arbitraryTemplate returns an arbitrary template from the name space
// associated with e and panics if no templates are found.
private static ptr<Template> arbitraryTemplate(this ptr<escaper> _addr_e) => func((_, panic, _) => {
    ref escaper e = ref _addr_e.val;

    foreach (var (_, t) in e.ns.set) {
        return _addr_t!;
    }    panic("no templates in name space");
});

// Forwarding functions so that clients need only import this package
// to reach the general escaping functions of text/template.

// HTMLEscape writes to w the escaped HTML equivalent of the plain text data b.
public static void HTMLEscape(io.Writer w, slice<byte> b) {
    template.HTMLEscape(w, b);
}

// HTMLEscapeString returns the escaped HTML equivalent of the plain text data s.
public static @string HTMLEscapeString(@string s) {
    return template.HTMLEscapeString(s);
}

// HTMLEscaper returns the escaped HTML equivalent of the textual
// representation of its arguments.
public static @string HTMLEscaper(params object[] args) {
    args = args.Clone();

    return template.HTMLEscaper(args);
}

// JSEscape writes to w the escaped JavaScript equivalent of the plain text data b.
public static void JSEscape(io.Writer w, slice<byte> b) {
    template.JSEscape(w, b);
}

// JSEscapeString returns the escaped JavaScript equivalent of the plain text data s.
public static @string JSEscapeString(@string s) {
    return template.JSEscapeString(s);
}

// JSEscaper returns the escaped JavaScript equivalent of the textual
// representation of its arguments.
public static @string JSEscaper(params object[] args) {
    args = args.Clone();

    return template.JSEscaper(args);
}

// URLQueryEscaper returns the escaped value of the textual representation of
// its arguments in a form suitable for embedding in a URL query.
public static @string URLQueryEscaper(params object[] args) {
    args = args.Clone();

    return template.URLQueryEscaper(args);
}

} // end template_package
