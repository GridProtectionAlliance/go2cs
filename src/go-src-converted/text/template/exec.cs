// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using errors = errors_package;
using fmt = fmt_package;
using fmtsort = @internal.fmtsort_package;
using io = io_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using parse = text.template.parse_package;
using @internal;
using text.template;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// maxExecDepth specifies the maximum stack depth of templates within
// templates. This limit is only practically reached by accidentally
// recursive template invocations. This limit allows us to return
// an error instead of triggering a stack overflow.
internal static nint maxExecDepth = initMaxExecDepth();

internal static nint initMaxExecDepth() {
    if (runtime.GOARCH == "wasm"u8) {
        return 1000;
    }
    return 100000;
}

// state represents the state of an execution. It's not part of the
// template so that multiple executions of the same template
// can execute in parallel.
[GoType] partial struct state {
    internal ж<Template> tmpl;
    internal io_package.Writer wr;
    internal text.template.parse_package.Node node; // current node, for errors
    internal slice<variable> vars; // push-down stack of variable values.
    internal nint depth;       // the height of the stack of executing templates.
}

// variable holds the dynamic value of a variable such as $, $x etc.
[GoType] partial struct variable {
    internal @string name;
    internal reflect_package.ΔValue value;
}

// push pushes a new variable on the stack.
[GoRecv] internal static void push(this ref state s, @string name, reflectꓸValue value) {
    s.vars = append(s.vars, new variable(name, value));
}

// mark returns the length of the variable stack.
[GoRecv] internal static nint mark(this ref state s) {
    return len(s.vars);
}

// pop pops the variable stack up to the mark.
[GoRecv] internal static void pop(this ref state s, nint mark) {
    s.vars = s.vars[0..(int)(mark)];
}

// setVar overwrites the last declared variable with the given name.
// Used by variable assignments.
[GoRecv] internal static void setVar(this ref state s, @string name, reflectꓸValue value) {
    for (nint i = s.mark() - 1; i >= 0; i--) {
        if (s.vars[i].name == name) {
            s.vars[i].value = value;
            return;
        }
    }
    s.errorf("undefined variable: %s"u8, name);
}

// setTopVar overwrites the top-nth variable on the stack. Used by range iterations.
[GoRecv] internal static void setTopVar(this ref state s, nint n, reflectꓸValue value) {
    s.vars[len(s.vars) - n].value = value;
}

// varValue returns the value of the named variable.
[GoRecv] internal static reflectꓸValue varValue(this ref state s, @string name) {
    for (nint i = s.mark() - 1; i >= 0; i--) {
        if (s.vars[i].name == name) {
            return s.vars[i].value;
        }
    }
    s.errorf("undefined variable: %s"u8, name);
    return zero;
}

internal static reflectꓸValue zero;

[GoType] partial struct missingValType {
}

internal static reflectꓸValue missingVal = reflect.ValueOf(new missingValType(nil));

internal static reflectꓸType missingValReflectType = reflect.TypeFor<missingValType>();

internal static bool isMissing(reflectꓸValue v) {
    return v.IsValid() && AreEqual(v.Type(), missingValReflectType);
}

// at marks the state to be on node n, for error reporting.
[GoRecv] internal static void at(this ref state s, parse.Node node) {
    s.node = node;
}

// doublePercent returns the string with %'s replaced by %%, if necessary,
// so it can be used safely inside a Printf format string.
internal static @string doublePercent(@string str) {
    return strings.ReplaceAll(str, "%"u8, "%%"u8);
}

// TODO: It would be nice if ExecError was more broken down, but
// the way ErrorContext embeds the template name makes the
// processing too clumsy.

// ExecError is the custom error type returned when Execute has an
// error evaluating its template. (If a write error occurs, the actual
// error is returned; it will not be of type ExecError.)
[GoType] partial struct ExecError {
    public @string Name; // Name of template.
    public error Err;  // Pre-formatted error.
}

public static @string Error(this ExecError e) {
    return e.Err.Error();
}

public static error Unwrap(this ExecError e) {
    return e.Err;
}

// errorf records an ExecError and terminates processing.
[GoRecv] internal static void errorf(this ref state s, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    @string name = doublePercent(s.tmpl.Name());
    if (s.node == default!){
        format = fmt.Sprintf("template: %s: %s"u8, name, format);
    } else {
        var (location, context) = s.tmpl.ErrorContext(s.node);
        format = fmt.Sprintf("template: %s: executing %q at <%s>: %s"u8, location, name, doublePercent(context), format);
    }
    throw panic(new ExecError(
        Name: s.tmpl.Name(),
        Err: fmt.Errorf(format, args.ꓸꓸꓸ)
    ));
}

// writeError is the wrapper type used internally when Execute has an
// error writing to its output. We strip the wrapper in errRecover.
// Note that this is not an implementation of error, so it cannot escape
// from the package as an error value.
[GoType] partial struct ΔwriteError {
    public error Err; // Original error.
}

[GoRecv] internal static void writeError(this ref state s, error err) {
    throw panic(new ΔwriteError(
        Err: err
    ));
}

// errRecover is the handler that turns panics into returns from the top
// level of Parse.
internal static void errRecover(ж<error> Ꮡerrp) => func((_, recover) => {
    ref var errp = ref Ꮡerrp.val;

    var e = recover();
    if (e != default!) {
        switch (e.type()) {
        case runtimeꓸError err: {
            throw panic(e);
            break;
        }
        case ΔwriteError err: {
            errp = err.Err;
            break;
        }
        case ExecError err: {
            errp = err;
            break;
        }
        default: {
            var err = e.type();
            throw panic(e);
            break;
        }}
    }
});

// Strip the wrapper.
// Keep the wrapper.

// ExecuteTemplate applies the template associated with t that has the given name
// to the specified data object and writes the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
[GoRecv] public static error ExecuteTemplate(this ref Template t, io.Writer wr, @string name, any data) {
    var tmpl = t.Lookup(name);
    if (tmpl == nil) {
        return fmt.Errorf("template: no template %q associated with template %q"u8, name, t.name);
    }
    return tmpl.Execute(wr, data);
}

// Execute applies a parsed template to the specified data object,
// and writes the output to wr.
// If an error occurs executing the template or writing its output,
// execution stops, but partial results may already have been written to
// the output writer.
// A template may be executed safely in parallel, although if parallel
// executions share a Writer the output may be interleaved.
//
// If data is a [reflect.Value], the template applies to the concrete
// value that the reflect.Value holds, as in [fmt.Print].
[GoRecv] public static error Execute(this ref Template t, io.Writer wr, any data) {
    return t.execute(wr, data);
}

[GoRecv] internal static error /*err*/ execute(this ref Template t, io.Writer wr, any data) => func((defer, _) => {
    error err = default!;

    deferǃ(errRecover, Ꮡ(err), defer);
    var (value, ok) = data._<reflectꓸValue>(ᐧ);
    if (!ok) {
        value = reflect.ValueOf(data);
    }
    var state = Ꮡ(new state(
        tmpl: t,
        wr: wr,
        vars: new variable[]{new("$"u8, value)}.slice()
    ));
    if (t.Tree == nil || t.Root == nil) {
        state.errorf("%q is an incomplete or empty template"u8, t.Name());
    }
    state.walk(value, ~t.Root);
    return err;
});

// DefinedTemplates returns a string listing the defined templates,
// prefixed by the string "; defined templates are: ". If there are none,
// it returns the empty string. For generating an error message here
// and in [html/template].
[GoRecv] public static @string DefinedTemplates(this ref Template t) => func((defer, _) => {
    if (t.common == nil) {
        return ""u8;
    }
    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    t.muTmpl.RLock();
    defer(t.muTmpl.RUnlock);
    foreach (var (name, tmpl) in t.tmpl) {
        if ((~tmpl).Tree == nil || tmpl.Root == nil) {
            continue;
        }
        if (b.Len() == 0){
            b.WriteString("; defined templates are: "u8);
        } else {
            b.WriteString(", "u8);
        }
        fmt.Fprintf(~Ꮡb, "%q"u8, name);
    }
    return b.String();
});

// Sentinel errors for use with panic to signal early exits from range loops.
internal static error walkBreak = errors.New("break"u8);

internal static error walkContinue = errors.New("continue"u8);

// Walk functions step through the major pieces of the template structure,
// generating output as they go.
[GoRecv] internal static void walk(this ref state s, reflectꓸValue dot, parse.Node node) {
    s.at(node);
    switch (node.type()) {
    case ж<parse.ActionNode> node: {
        var val = s.evalPipeline(dot, // Do not pop variables so they persist until next end.
 // Also, if the action declares variables, don't print the result.
 (~node).Pipe);
        if (len((~(~node).Pipe).Decl) == 0) {
            s.printValue(~node, val);
        }
        break;
    }
    case ж<parse.BreakNode> node: {
        throw panic(walkBreak);
        break;
    }
    case ж<parse.CommentNode> node: {
        break;
    }
    case ж<parse.ContinueNode> node: {
        throw panic(walkContinue);
        break;
    }
    case ж<parse.IfNode> node: {
        s.walkIfOrWith(parse.NodeIf, dot, node.Pipe, node.List, node.ElseList);
        break;
    }
    case ж<parse.ListNode> node: {
        foreach (var (_, nodeΔ1) in (~node).Nodes) {
            s.walk(dot, nodeΔ1);
        }
        break;
    }
    case ж<parse.RangeNode> node: {
        s.walkRange(dot, Ꮡnode);
        break;
    }
    case ж<parse.TemplateNode> node: {
        s.walkTemplate(dot, Ꮡnode);
        break;
    }
    case ж<parse.TextNode> node: {
        {
            var (_, err) = s.wr.Write((~node).Text); if (err != default!) {
                s.writeError(err);
            }
        }
        break;
    }
    case ж<parse.WithNode> node: {
        s.walkIfOrWith(parse.NodeWith, dot, node.Pipe, node.List, node.ElseList);
        break;
    }
    default: {
        var node = node.type();
        s.errorf("unknown node: %s"u8, node);
        break;
    }}
}

// walkIfOrWith walks an 'if' or 'with' node. The two control structures
// are identical in behavior except that 'with' sets dot.
[GoRecv] internal static void walkIfOrWith(this ref state s, parse.NodeType typ, reflectꓸValue dot, ж<parse.PipeNode> Ꮡpipe, ж<parse.ListNode> Ꮡlist, ж<parse.ListNode> ᏑelseList) => func((defer, _) => {
    ref var pipe = ref Ꮡpipe.val;
    ref var list = ref Ꮡlist.val;
    ref var elseList = ref ᏑelseList.val;

    deferǃ(s.pop, s.mark(), defer);
    var val = s.evalPipeline(dot, Ꮡpipe);
    var (truth, ok) = isTrue(indirectInterface(val));
    if (!ok) {
        s.errorf("if/with can't use %v"u8, val);
    }
    if (truth){
        if (typ == parse.NodeWith){
            s.walk(val, ~list);
        } else {
            s.walk(dot, ~list);
        }
    } else 
    if (elseList != nil) {
        s.walk(dot, ~elseList);
    }
});

// IsTrue reports whether the value is 'true', in the sense of not the zero of its type,
// and whether the value has a meaningful truth value. This is the definition of
// truth used by if and other such actions.
public static (bool truth, bool ok) IsTrue(any val) {
    bool truth = default!;
    bool ok = default!;

    return isTrue(reflect.ValueOf(val));
}

internal static (bool truth, bool ok) isTrue(reflectꓸValue val) {
    bool truth = default!;
    bool ok = default!;

    if (!val.IsValid()) {
        // Something like var x interface{}, never set. It's a form of nil.
        return (false, true);
    }
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.ΔString) {
        truth = val.Len() > 0;
    }
    else if (exprᴛ1 == reflect.ΔBool) {
        truth = val.Bool();
    }
    else if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        truth = val.Complex() != 0;
    }
    else if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔInterface) {
        truth = !val.IsNil();
    }
    else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        truth = val.Int() != 0;
    }
    else if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        truth = val.Float() != 0;
    }
    else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        truth = val.Uint() != 0;
    }
    else if (exprᴛ1 == reflect.Struct) {
        truth = true;
    }
    else { /* default: */
        return (truth, ok);
    }

    // Struct values are always true.
    return (truth, true);
}

[GoRecv] internal static void walkRange(this ref state s, reflectꓸValue dot, ж<parse.RangeNode> Ꮡr) => func((defer, recover) => {
    ref var r = ref Ꮡr.val;

    s.at(~r);
    defer(() => {
        {
            var rΔ1 = recover(); if (rΔ1 != default! && !AreEqual(rΔ1, walkBreak)) {
                throw panic(rΔ1);
            }
        }
    });
    deferǃ(s.pop, s.mark(), defer);
    var (val, _) = indirect(s.evalPipeline(dot, r.Pipe));
    // mark top of stack before any variables in the body are pushed.
    nint mark = s.mark();
    var oneIteration = (reflectꓸValue index, reflectꓸValue elem) => {
        if (len(r.Pipe.Decl) > 0) {
            if (r.Pipe.IsAssign){
                // With two variables, index comes first.
                // With one, we use the element.
                if (len(r.Pipe.Decl) > 1){
                    s.setVar(r.Pipe.Decl[0].Ident[0], index);
                } else {
                    s.setVar(r.Pipe.Decl[0].Ident[0], elem);
                }
            } else {
                // Set top var (lexically the second if there
                // are two) to the element.
                s.setTopVar(1, elem);
            }
        }
        if (len(r.Pipe.Decl) > 1) {
            if (r.Pipe.IsAssign){
                s.setVar(r.Pipe.Decl[1].Ident[0], elem);
            } else {
                // Set next var (lexically the first if there
                // are two) to the index.
                s.setTopVar(2, index);
            }
        }
        deferǃ(s.pop, mark, defer);
        defer(() => {
            // Consume panic(walkContinue)
            {
                var rΔ2 = recover(); if (rΔ2 != default! && !AreEqual(rΔ2, walkContinue)) {
                    throw panic(rΔ2);
                }
            }
        });
        s.walk(elem, ~r.List);
    };
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) {
        if (val.Len() == 0) {
            break;
        }
        for (nint i = 0; i < val.Len(); i++) {
            oneIteration(reflect.ValueOf(i), val.Index(i));
        }
        return;
    }
    if (exprᴛ1 == reflect.Map) {
        if (val.Len() == 0) {
            break;
        }
        var om = fmtsort.Sort(val);
        foreach (var (_, m) in om) {
            oneIteration(m.Key, m.Value);
        }
        return;
    }
    if (exprᴛ1 == reflect.Chan) {
        if (val.IsNil()) {
            break;
        }
        if (val.Type().ChanDir() == reflect.SendDir) {
            s.errorf("range over send-only channel %v"u8, val);
            break;
        }
        nint i = 0;
        for (; ᐧ ; i++) {
            var (elem, ok) = val.Recv();
            if (!ok) {
                break;
            }
            oneIteration(reflect.ValueOf(i), elem);
        }
        if (i == 0) {
            break;
        }
        return;
    }
    if (exprᴛ1 == reflect.Invalid) {
        break;
    }
    else { /* default: */
        s.errorf("range can't iterate over %v"u8, // An invalid value is likely a nil map, etc. and acts like an empty map.
 val);
    }

    if (r.ElseList != nil) {
        s.walk(dot, ~r.ElseList);
    }
});

[GoRecv] internal static void walkTemplate(this ref state s, reflectꓸValue dot, ж<parse.TemplateNode> Ꮡt) {
    ref var t = ref Ꮡt.val;

    s.at(~t);
    var tmpl = s.tmpl.Lookup(t.Name);
    if (tmpl == nil) {
        s.errorf("template %q not defined"u8, t.Name);
    }
    if (s.depth == maxExecDepth) {
        s.errorf("exceeded maximum template depth (%v)"u8, maxExecDepth);
    }
    // Variables declared by the pipeline persist.
    dot = s.evalPipeline(dot, t.Pipe);
    var newState = s;
    newState.depth++;
    newState.tmpl = tmpl;
    // No dynamic scoping: template invocations inherit no variables.
    newState.vars = new variable[]{new("$"u8, dot)}.slice();
    newState.walk(dot, ~tmpl.Root);
}

// Eval functions evaluate pipelines, commands, and their elements and extract
// values from the data structure by examining fields, calling methods, and so on.
// The printing of those values happens only through walk functions.

// evalPipeline returns the value acquired by evaluating a pipeline. If the
// pipeline has a variable declaration, the variable will be pushed on the
// stack. Callers should therefore pop the stack after they are finished
// executing commands depending on the pipeline value.
[GoRecv] internal static reflectꓸValue /*value*/ evalPipeline(this ref state s, reflectꓸValue dot, ж<parse.PipeNode> Ꮡpipe) {
    reflectꓸValue value = default!;

    ref var pipe = ref Ꮡpipe.val;
    if (pipe == nil) {
        return value;
    }
    s.at(~pipe);
    value = missingVal;
    foreach (var (_, cmd) in pipe.Cmds) {
        value = s.evalCommand(dot, cmd, value);
        // previous value is this one's final arg.
        // If the object has type interface{}, dig down one level to the thing inside.
        if (value.Kind() == reflect.ΔInterface && value.Type().NumMethod() == 0) {
            value = value.Elem();
        }
    }
    foreach (var (_, variable) in pipe.Decl) {
        if (pipe.IsAssign){
            s.setVar((~variable).Ident[0], value);
        } else {
            s.push((~variable).Ident[0], value);
        }
    }
    return value;
}

[GoRecv] internal static void notAFunction(this ref state s, slice<parse.Node> args, reflectꓸValue final) {
    if (len(args) > 1 || !isMissing(final)) {
        s.errorf("can't give argument to non-function %s"u8, args[0]);
    }
}

[GoRecv] internal static reflectꓸValue evalCommand(this ref state s, reflectꓸValue dot, ж<parse.CommandNode> Ꮡcmd, reflectꓸValue final) {
    ref var cmd = ref Ꮡcmd.val;

    var firstWord = cmd.Args[0];
    switch (firstWord.type()) {
    case ж<parse.FieldNode> n: {
        return s.evalFieldNode(dot, n, cmd.Args, final);
    }
    case ж<parse.ChainNode> n: {
        return s.evalChainNode(dot, n, cmd.Args, final);
    }
    case ж<parse.IdentifierNode> n: {
        return s.evalFunction(dot, // Must be a function.
 n, ~cmd, cmd.Args, final);
    }
    case ж<parse.PipeNode> n: {
        s.notAFunction(cmd.Args, // Parenthesized pipeline. The arguments are all inside the pipeline; final must be absent.
 final);
        return s.evalPipeline(dot, n);
    }
    case ж<parse.VariableNode> n: {
        return s.evalVariableNode(dot, n, cmd.Args, final);
    }}
    s.at(firstWord);
    s.notAFunction(cmd.Args, final);
    switch (firstWord.type()) {
    case ж<parse.BoolNode> word: {
        return reflect.ValueOf((~word).True);
    }
    case ж<parse.DotNode> word: {
        return dot;
    }
    case ж<parse.NilNode> word: {
        s.errorf("nil is not a command"u8);
        break;
    }
    case ж<parse.NumberNode> word: {
        return s.idealConstant(word);
    }
    case ж<parse.StringNode> word: {
        return reflect.ValueOf((~word).Text);
    }}
    s.errorf("can't evaluate command %q"u8, firstWord);
    throw panic("not reached");
}

// idealConstant is called to return the value of a number in a context where
// we don't know the type. In that case, the syntax of the number tells us
// its type, and we use Go rules to resolve. Note there is no such thing as
// a uint ideal constant in this situation - the value must be of int type.
[GoRecv] internal static reflectꓸValue idealConstant(this ref state s, ж<parse.NumberNode> Ꮡconstant) {
    ref var constant = ref Ꮡconstant.val;

    // These are ideal constants but we don't know the type
    // and we have no context.  (If it was a method argument,
    // we'd know what we need.) The syntax guides us to some extent.
    s.at(~constant);
    switch (ᐧ) {
    case {} when constant.IsComplex: {
        return reflect.ValueOf(constant.Complex128);
    }
    case {} when constant.IsFloat && !isHexInt(constant.Text) && !isRuneInt(constant.Text) && strings.ContainsAny(constant.Text, // incontrovertible.
 ".eEpP"u8): {
        return reflect.ValueOf(constant.Float64);
    }
    case {} when constant.IsInt: {
        nint n = ((nint)constant.Int64);
        if (((int64)n) != constant.Int64) {
            s.errorf("%s overflows int"u8, constant.Text);
        }
        return reflect.ValueOf(n);
    }
    case {} when constant.IsUint: {
        s.errorf("%s overflows int"u8, constant.Text);
        break;
    }}

    return zero;
}

internal static bool isRuneInt(@string s) {
    return len(s) > 0 && s[0] == (rune)'\'';
}

internal static bool isHexInt(@string s) {
    return len(s) > 2 && s[0] == (rune)'0' && (s[1] == (rune)'x' || s[1] == (rune)'X') && !strings.ContainsAny(s, "pP"u8);
}

[GoRecv] internal static reflectꓸValue evalFieldNode(this ref state s, reflectꓸValue dot, ж<parse.FieldNode> Ꮡfield, slice<parse.Node> args, reflectꓸValue final) {
    ref var field = ref Ꮡfield.val;

    s.at(~field);
    return s.evalFieldChain(dot, dot, ~field, field.Ident, args, final);
}

[GoRecv] internal static reflectꓸValue evalChainNode(this ref state s, reflectꓸValue dot, ж<parse.ChainNode> Ꮡchain, slice<parse.Node> args, reflectꓸValue final) {
    ref var chain = ref Ꮡchain.val;

    s.at(~chain);
    if (len(chain.Field) == 0) {
        s.errorf("internal error: no fields in evalChainNode"u8);
    }
    if (chain.Node.Type() == parse.NodeNil) {
        s.errorf("indirection through explicit nil in %s"u8, chain);
    }
    // (pipe).Field1.Field2 has pipe as .Node, fields as .Field. Eval the pipeline, then the fields.
    var pipe = s.evalArg(dot, default!, chain.Node);
    return s.evalFieldChain(dot, pipe, ~chain, chain.Field, args, final);
}

[GoRecv] internal static reflectꓸValue evalVariableNode(this ref state s, reflectꓸValue dot, ж<parse.VariableNode> Ꮡvariable, slice<parse.Node> args, reflectꓸValue final) {
    ref var variable = ref Ꮡvariable.val;

    // $x.Field has $x as the first ident, Field as the second. Eval the var, then the fields.
    s.at(~variable);
    var value = s.varValue(variable.Ident[0]);
    if (len(variable.Ident) == 1) {
        s.notAFunction(args, final);
        return value;
    }
    return s.evalFieldChain(dot, value, ~variable, variable.Ident[1..], args, final);
}

// evalFieldChain evaluates .X.Y.Z possibly followed by arguments.
// dot is the environment in which to evaluate arguments, while
// receiver is the value being walked along the chain.
[GoRecv] internal static reflectꓸValue evalFieldChain(this ref state s, reflectꓸValue dot, reflectꓸValue receiver, parse.Node node, slice<@string> ident, slice<parse.Node> args, reflectꓸValue final) {
    nint n = len(ident);
    for (nint i = 0; i < n - 1; i++) {
        receiver = s.evalField(dot, ident[i], node, default!, missingVal, receiver);
    }
    // Now if it's a method, it gets the arguments.
    return s.evalField(dot, ident[n - 1], node, args, final, receiver);
}

[GoRecv] internal static reflectꓸValue evalFunction(this ref state s, reflectꓸValue dot, ж<parse.IdentifierNode> Ꮡnode, parse.Node cmd, slice<parse.Node> args, reflectꓸValue final) {
    ref var node = ref Ꮡnode.val;

    s.at(~node);
    @string name = node.Ident;
    var (function, isBuiltin, ok) = findFunction(name, s.tmpl);
    if (!ok) {
        s.errorf("%q is not a defined function"u8, name);
    }
    return s.evalCall(dot, function, isBuiltin, cmd, name, args, final);
}

// evalField evaluates an expression like (.Field) or (.Field arg1 arg2).
// The 'final' argument represents the return value from the preceding
// value of the pipeline, if any.
[GoRecv] internal static reflectꓸValue evalField(this ref state s, reflectꓸValue dot, @string fieldName, parse.Node node, slice<parse.Node> args, reflectꓸValue final, reflectꓸValue receiver) {
    if (!receiver.IsValid()) {
        if (s.tmpl.option.missingKey == mapError) {
            // Treat invalid value as missing map key.
            s.errorf("nil data; no entry for key %q"u8, fieldName);
        }
        return zero;
    }
    var typ = receiver.Type();
    var (receiver, isNil) = indirect(receiver);
    if (receiver.Kind() == reflect.ΔInterface && isNil) {
        // Calling a method on a nil interface can't work. The
        // MethodByName method call below would panic.
        s.errorf("nil pointer evaluating %s.%s"u8, typ, fieldName);
        return zero;
    }
    // Unless it's an interface, need to get to a value of type *T to guarantee
    // we see all methods of T and *T.
    var ptr = receiver;
    if (ptr.Kind() != reflect.ΔInterface && ptr.Kind() != reflect.ΔPointer && ptr.CanAddr()) {
        ptr = ptr.Addr();
    }
    {
        var method = ptr.MethodByName(fieldName); if (method.IsValid()) {
            return s.evalCall(dot, method, false, node, fieldName, args, final);
        }
    }
    var hasArgs = len(args) > 1 || !isMissing(final);
    // It's not a method; must be a field of a struct or an element of a map.
    var exprᴛ1 = receiver.Kind();
    if (exprᴛ1 == reflect.Struct) {
        var (tField, ok) = receiver.Type().FieldByName(fieldName);
        if (ok) {
            var (field, err) = receiver.FieldByIndexErr(tField.Index);
            if (!tField.IsExported()) {
                s.errorf("%s is an unexported field of struct type %s"u8, fieldName, typ);
            }
            if (err != default!) {
                s.errorf("%v"u8, err);
            }
            // If it's a function, we must call it.
            if (hasArgs) {
                s.errorf("%s has arguments but cannot be invoked as function"u8, fieldName);
            }
            return field;
        }
    }
    if (exprᴛ1 == reflect.Map) {
        var nameVal = reflect.ValueOf(fieldName);
        if (nameVal.Type().AssignableTo(receiver.Type().Key())) {
            // If it's a map, attempt to use the field name as a key.
            if (hasArgs) {
                s.errorf("%s is not a method but has arguments"u8, fieldName);
            }
            var result = receiver.MapIndex(nameVal);
            if (!result.IsValid()) {
                var exprᴛ2 = s.tmpl.option.missingKey;
                if (exprᴛ2 == mapInvalid) {
                }
                else if (exprᴛ2 == mapZeroValue) {
                    result = reflect.Zero(receiver.Type().Elem());
                }
                else if (exprᴛ2 == mapError) {
                    s.errorf("map has no entry for key %q"u8, // Just use the invalid value.
 fieldName);
                }

            }
            return result;
        }
    }
    if (exprᴛ1 == reflect.ΔPointer) {
        var etyp = receiver.Type().Elem();
        if (etyp.Kind() == reflect.Struct) {
            {
                var (_, ok) = etyp.FieldByName(fieldName); if (!ok) {
                    // If there's no such field, say "can't evaluate"
                    // instead of "nil pointer evaluating".
                    break;
                }
            }
        }
        if (isNil) {
            s.errorf("nil pointer evaluating %s.%s"u8, typ, fieldName);
        }
    }

    s.errorf("can't evaluate field %s in type %s"u8, fieldName, typ);
    throw panic("not reached");
}

internal static reflectꓸType errorType = reflect.TypeFor<error>();
internal static reflectꓸType fmtStringerType = reflect.TypeFor[fmt.Stringer]();
internal static reflectꓸType reflectValueType = reflect.TypeFor[reflectꓸValue]();

// evalCall executes a function or method call. If it's a method, fun already has the receiver bound, so
// it looks just like a function call. The arg list, if non-nil, includes (in the manner of the shell), arg[0]
// as the function itself.
[GoRecv] internal static reflectꓸValue evalCall(this ref state s, reflectꓸValue dot, reflectꓸValue fun, bool isBuiltin, parse.Node node, @string name, slice<parse.Node> args, reflectꓸValue final) {
    if (args != default!) {
        args = args[1..];
    }
    // Zeroth arg is function name/node; not passed to function.
    var typ = fun.Type();
    nint numIn = len(args);
    if (!isMissing(final)) {
        numIn++;
    }
    nint numFixed = len(args);
    if (typ.IsVariadic()){
        numFixed = typ.NumIn() - 1;
        // last arg is the variadic one.
        if (numIn < numFixed) {
            s.errorf("wrong number of args for %s: want at least %d got %d"u8, name, typ.NumIn() - 1, len(args));
        }
    } else 
    if (numIn != typ.NumIn()) {
        s.errorf("wrong number of args for %s: want %d got %d"u8, name, typ.NumIn(), numIn);
    }
    {
        var errΔ1 = goodFunc(name, typ); if (errΔ1 != default!) {
            s.errorf("%v"u8, errΔ1);
        }
    }
    var unwrap = (reflectꓸValue v) => {
        if (AreEqual(vΔ1.Type(), reflectValueType)) {
             = vΔ1.Interface()._<reflectꓸValue>();
        }
        return vΔ1;
    };
    // Special case for builtin and/or, which short-circuit.
    if (isBuiltin && (name == "and"u8 || name == "or"u8)) {
        var argType = typ.In(0);
        reflectꓸValue v = default!;
        foreach (var (_, arg) in args) {
            v = s.evalArg(dot, argType, arg).Interface()._<reflectꓸValue>();
            if (truth(v) == (name == "or"u8)) {
                // This value was already unwrapped
                // by the .Interface().(reflect.Value).
                return v;
            }
        }
        if (final != missingVal) {
            // The last argument to and/or is coming from
            // the pipeline. We didn't short circuit on an earlier
            // argument, so we are going to return this one.
            // We don't have to evaluate final, but we do
            // have to check its type. Then, since we are
            // going to return it, we have to unwrap it.
            v = unwrap(s.validateType(final, argType));
        }
        return v;
    }
    // Build the arg list.
    var argv = new slice<reflectꓸValue>(numIn);
    // Args must be evaluated. Fixed args first.
    nint i = 0;
    for (; i < numFixed && i < len(args); i++) {
        argv[i] = s.evalArg(dot, typ.In(i), args[i]);
    }
    // Now the ... args.
    if (typ.IsVariadic()) {
        var argType = typ.In(typ.NumIn() - 1).Elem();
        // Argument is a slice.
        for (; i < len(args); i++) {
            argv[i] = s.evalArg(dot, argType, args[i]);
        }
    }
    // Add final value if necessary.
    if (!isMissing(final)) {
        var t = typ.In(typ.NumIn() - 1);
        if (typ.IsVariadic()) {
            if (numIn - 1 < numFixed){
                // The added final argument corresponds to a fixed parameter of the function.
                // Validate against the type of the actual parameter.
                t = typ.In(numIn - 1);
            } else {
                // The added final argument corresponds to the variadic part.
                // Validate against the type of the elements of the variadic slice.
                t = t.Elem();
            }
        }
        argv[i] = s.validateType(final, t);
    }
    // Special case for the "call" builtin.
    // Insert the name of the callee function as the first argument.
    if (isBuiltin && name == "call"u8) {
        @string calleeName = args[0].String();
        argv = append(new reflectꓸValue[]{reflect.ValueOf(calleeName)}.slice(), argv.ꓸꓸꓸ);
        fun = reflect.ValueOf(call);
    }
    var (v, err) = safeCall(fun, argv);
    // If we have an error that is not nil, stop execution and return that
    // error to the caller.
    if (err != default!) {
        s.at(node);
        s.errorf("error calling %s: %w"u8, name, err);
    }
    return unwrap(v);
}

// canBeNil reports whether an untyped nil can be assigned to the type. See reflect.Zero.
internal static bool canBeNil(reflectꓸType typ) {
    var exprᴛ1 = typ.Kind();
    if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.ΔInterface || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔSlice) {
        return true;
    }
    if (exprᴛ1 == reflect.Struct) {
        return AreEqual(typ, reflectValueType);
    }

    return false;
}

// validateType guarantees that the value is valid and assignable to the type.
[GoRecv] internal static reflectꓸValue validateType(this ref state s, reflectꓸValue value, reflectꓸType typ) {
    if (!value.IsValid()) {
        if (typ == default!) {
            // An untyped nil interface{}. Accept as a proper nil value.
            return reflect.ValueOf(default!);
        }
        if (canBeNil(typ)) {
            // Like above, but use the zero value of the non-nil type.
            return reflect.Zero(typ);
        }
        s.errorf("invalid value; expected %s"u8, typ);
    }
    if (AreEqual(typ, reflectValueType) && !AreEqual(value.Type(), typ)) {
        return reflect.ValueOf(value);
    }
    if (typ != default! && !value.Type().AssignableTo(typ)) {
        if (value.Kind() == reflect.ΔInterface && !value.IsNil()) {
            value = value.Elem();
            if (value.Type().AssignableTo(typ)) {
                return value;
            }
        }
        // fallthrough
        // Does one dereference or indirection work? We could do more, as we
        // do with method receivers, but that gets messy and method receivers
        // are much more constrained, so it makes more sense there than here.
        // Besides, one is almost always all you need.
        switch (ᐧ) {
        case {} when value.Kind() == reflect.ΔPointer && value.Type().Elem().AssignableTo(typ): {
            value = value.Elem();
            if (!value.IsValid()) {
                s.errorf("dereference of nil pointer of type %s"u8, typ);
            }
            break;
        }
        case {} when reflect.PointerTo(value.Type()).AssignableTo(typ) && value.CanAddr(): {
            value = value.Addr();
            break;
        }
        default: {
            s.errorf("wrong type for value; expected %s; got %s"u8, typ, value.Type());
            break;
        }}

    }
    return value;
}

[GoRecv] internal static reflectꓸValue evalArg(this ref state s, reflectꓸValue dot, reflectꓸType typ, parse.Node n) {
    s.at(n);
    switch (n.type()) {
    case ж<parse.DotNode> arg: {
        return s.validateType(dot, typ);
    }
    case ж<parse.NilNode> arg: {
        if (canBeNil(typ)) {
            return reflect.Zero(typ);
        }
        s.errorf("cannot assign nil to %s"u8, typ);
        break;
    }
    case ж<parse.FieldNode> arg: {
        return s.validateType(s.evalFieldNode(dot, arg, new parse.Node[]{n}.slice(), missingVal), typ);
    }
    case ж<parse.VariableNode> arg: {
        return s.validateType(s.evalVariableNode(dot, arg, default!, missingVal), typ);
    }
    case ж<parse.PipeNode> arg: {
        return s.validateType(s.evalPipeline(dot, arg), typ);
    }
    case ж<parse.IdentifierNode> arg: {
        return s.validateType(s.evalFunction(dot, arg, ~arg, default!, missingVal), typ);
    }
    case ж<parse.ChainNode> arg: {
        return s.validateType(s.evalChainNode(dot, arg, default!, missingVal), typ);
    }}
    var exprᴛ1 = typ.Kind();
    if (exprᴛ1 == reflect.ΔBool) {
        return s.evalBool(typ, n);
    }
    if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        return s.evalComplex(typ, n);
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        return s.evalFloat(typ, n);
    }
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return s.evalInteger(typ, n);
    }
    if (exprᴛ1 == reflect.ΔInterface) {
        if (typ.NumMethod() == 0) {
            return s.evalEmptyInterface(dot, n);
        }
    }
    if (exprᴛ1 == reflect.Struct) {
        if (AreEqual(typ, reflectValueType)) {
            return reflect.ValueOf(s.evalEmptyInterface(dot, n));
        }
    }
    if (exprᴛ1 == reflect.ΔString) {
        return s.evalString(typ, n);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return s.evalUnsignedInteger(typ, n);
    }

    s.errorf("can't handle %s for arg of type %s"u8, n, typ);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalBool(this ref state s, reflectꓸType typ, parse.Node n) {
    s.at(n);
    {
        var (nΔ1, ok) = n._<ж<parse.BoolNode>>(ᐧ); if (ok) {
            var value = reflect.New(typ).Elem();
            value.SetBool((~nΔ1).True);
            return value;
        }
    }
    s.errorf("expected bool; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalString(this ref state s, reflectꓸType typ, parse.Node n) {
    s.at(n);
    {
        var (nΔ1, ok) = n._<ж<parse.StringNode>>(ᐧ); if (ok) {
            var value = reflect.New(typ).Elem();
            value.SetString((~nΔ1).Text);
            return value;
        }
    }
    s.errorf("expected string; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalInteger(this ref state s, reflectꓸType typ, parse.Node n) {
    s.at(n);
    {
        var (nΔ1, ok) = n._<ж<parse.NumberNode>>(ᐧ); if (ok && (~nΔ1).IsInt) {
            var value = reflect.New(typ).Elem();
            value.SetInt((~nΔ1).Int64);
            return value;
        }
    }
    s.errorf("expected integer; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalUnsignedInteger(this ref state s, reflectꓸType typ, parse.Node n) {
    s.at(n);
    {
        var (nΔ1, ok) = n._<ж<parse.NumberNode>>(ᐧ); if (ok && (~nΔ1).IsUint) {
            var value = reflect.New(typ).Elem();
            value.SetUint((~nΔ1).Uint64);
            return value;
        }
    }
    s.errorf("expected unsigned integer; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalFloat(this ref state s, reflectꓸType typ, parse.Node n) {
    s.at(n);
    {
        var (nΔ1, ok) = n._<ж<parse.NumberNode>>(ᐧ); if (ok && (~nΔ1).IsFloat) {
            var value = reflect.New(typ).Elem();
            value.SetFloat((~nΔ1).Float64);
            return value;
        }
    }
    s.errorf("expected float; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalComplex(this ref state s, reflectꓸType typ, parse.Node n) {
    {
        var (nΔ1, ok) = n._<ж<parse.NumberNode>>(ᐧ); if (ok && (~nΔ1).IsComplex) {
            var value = reflect.New(typ).Elem();
            value.SetComplex((~nΔ1).Complex128);
            return value;
        }
    }
    s.errorf("expected complex; found %s"u8, n);
    throw panic("not reached");
}

[GoRecv] internal static reflectꓸValue evalEmptyInterface(this ref state s, reflectꓸValue dot, parse.Node n) {
    s.at(n);
    switch (n.type()) {
    case ж<parse.BoolNode> n: {
        return reflect.ValueOf((~n).True);
    }
    case ж<parse.DotNode> n: {
        return dot;
    }
    case ж<parse.FieldNode> n: {
        return s.evalFieldNode(dot, Ꮡn, default!, missingVal);
    }
    case ж<parse.IdentifierNode> n: {
        return s.evalFunction(dot, Ꮡn, ~n, default!, missingVal);
    }
    case ж<parse.NilNode> n: {
        s.errorf("evalEmptyInterface: nil (can't happen)"u8);
        break;
    }
    case ж<parse.NumberNode> n: {
        return s.idealConstant(Ꮡn);
    }
    case ж<parse.StringNode> n: {
        return reflect.ValueOf((~n).Text);
    }
    case ж<parse.VariableNode> n: {
        return s.evalVariableNode(dot, // NilNode is handled in evalArg, the only place that calls here.
 Ꮡn, default!, missingVal);
    }
    case ж<parse.PipeNode> n: {
        return s.evalPipeline(dot, Ꮡn);
    }}
    s.errorf("can't handle assignment of %s to empty interface argument"u8, n);
    throw panic("not reached");
}

// indirect returns the item at the end of indirection, and a bool to indicate
// if it's nil. If the returned bool is true, the returned value's kind will be
// either a pointer or interface.
internal static (reflectꓸValue rv, bool isNil) indirect(reflectꓸValue v) {
    reflectꓸValue rv = default!;
    bool isNil = default!;

    for (; v.Kind() == reflect.ΔPointer || v.Kind() == reflect.ΔInterface; v = v.Elem()) {
        if (v.IsNil()) {
            return (v, true);
        }
    }
    return (v, false);
}

// indirectInterface returns the concrete value in an interface value,
// or else the zero reflect.Value.
// That is, if v represents the interface value x, the result is the same as reflect.ValueOf(x):
// the fact that x was an interface value is forgotten.
internal static reflectꓸValue indirectInterface(reflectꓸValue v) {
    if (v.Kind() != reflect.ΔInterface) {
        return v;
    }
    if (v.IsNil()) {
        return new reflectꓸValue(nil);
    }
    return v.Elem();
}

// printValue writes the textual representation of the value to the output of
// the template.
[GoRecv] internal static void printValue(this ref state s, parse.Node n, reflectꓸValue v) {
    s.at(n);
    var (iface, ok) = printableValue(v);
    if (!ok) {
        s.errorf("can't print %s of type %s"u8, n, v.Type());
    }
    var (_, err) = fmt.Fprint(s.wr, iface);
    if (err != default!) {
        s.writeError(err);
    }
}

// printableValue returns the, possibly indirected, interface value inside v that
// is best for a call to formatted printer.
internal static (any, bool) printableValue(reflectꓸValue v) {
    if (v.Kind() == reflect.ΔPointer) {
        (v, _) = indirect(v);
    }
    // fmt.Fprint handles nil.
    if (!v.IsValid()) {
        return ("<no value>", true);
    }
    if (!v.Type().Implements(errorType) && !v.Type().Implements(fmtStringerType)) {
        if (v.CanAddr() && (reflect.PointerTo(v.Type()).Implements(errorType) || reflect.PointerTo(v.Type()).Implements(fmtStringerType))){
            v = v.Addr();
        } else {
            var exprᴛ1 = v.Kind();
            if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func) {
                return (default!, false);
            }

        }
    }
    return (v.Interface(), true);
}

} // end template_package
