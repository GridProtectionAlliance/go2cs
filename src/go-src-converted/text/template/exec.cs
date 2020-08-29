// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:34:55 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Go\src\text\template\exec.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using parse = go.text.template.parse_package;
using static go.builtin;
using System;

namespace go {
namespace text
{
    public static partial class template_package
    {
        // maxExecDepth specifies the maximum stack depth of templates within
        // templates. This limit is only practically reached by accidentally
        // recursive template invocations. This limit allows us to return
        // an error instead of triggering a stack overflow.
        private static readonly long maxExecDepth = 100000L;

        // state represents the state of an execution. It's not part of the
        // template so that multiple executions of the same template
        // can execute in parallel.


        // state represents the state of an execution. It's not part of the
        // template so that multiple executions of the same template
        // can execute in parallel.
        private partial struct state
        {
            public ptr<Template> tmpl;
            public io.Writer wr;
            public parse.Node node; // current node, for errors
            public slice<variable> vars; // push-down stack of variable values.
            public long depth; // the height of the stack of executing templates.
        }

        // variable holds the dynamic value of a variable such as $, $x etc.
        private partial struct variable
        {
            public @string name;
            public reflect.Value value;
        }

        // push pushes a new variable on the stack.
        private static void push(this ref state s, @string name, reflect.Value value)
        {
            s.vars = append(s.vars, new variable(name,value));
        }

        // mark returns the length of the variable stack.
        private static long mark(this ref state s)
        {
            return len(s.vars);
        }

        // pop pops the variable stack up to the mark.
        private static void pop(this ref state s, long mark)
        {
            s.vars = s.vars[0L..mark];
        }

        // setVar overwrites the top-nth variable on the stack. Used by range iterations.
        private static void setVar(this ref state s, long n, reflect.Value value)
        {
            s.vars[len(s.vars) - n].value = value;
        }

        // varValue returns the value of the named variable.
        private static reflect.Value varValue(this ref state s, @string name)
        {
            for (var i = s.mark() - 1L; i >= 0L; i--)
            {
                if (s.vars[i].name == name)
                {
                    return s.vars[i].value;
                }
            }

            s.errorf("undefined variable: %s", name);
            return zero;
        }

        private static reflect.Value zero = default;

        // at marks the state to be on node n, for error reporting.
        private static void at(this ref state s, parse.Node node)
        {
            s.node = node;
        }

        // doublePercent returns the string with %'s replaced by %%, if necessary,
        // so it can be used safely inside a Printf format string.
        private static @string doublePercent(@string str)
        {
            return strings.Replace(str, "%", "%%", -1L);
        }

        // TODO: It would be nice if ExecError was more broken down, but
        // the way ErrorContext embeds the template name makes the
        // processing too clumsy.

        // ExecError is the custom error type returned when Execute has an
        // error evaluating its template. (If a write error occurs, the actual
        // error is returned; it will not be of type ExecError.)
        public partial struct ExecError
        {
            public @string Name; // Name of template.
            public error Err; // Pre-formatted error.
        }

        public static @string Error(this ExecError e)
        {
            return e.Err.Error();
        }

        // errorf records an ExecError and terminates processing.
        private static void errorf(this ref state _s, @string format, params object[] args) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            var name = doublePercent(s.tmpl.Name());
            if (s.node == null)
            {
                format = fmt.Sprintf("template: %s: %s", name, format);
            }
            else
            {
                var (location, context) = s.tmpl.ErrorContext(s.node);
                format = fmt.Sprintf("template: %s: executing %q at <%s>: %s", location, name, doublePercent(context), format);
            }
            panic(new ExecError(Name:s.tmpl.Name(),Err:fmt.Errorf(format,args...),));
        });

        // writeError is the wrapper type used internally when Execute has an
        // error writing to its output. We strip the wrapper in errRecover.
        // Note that this is not an implementation of error, so it cannot escape
        // from the package as an error value.
        private partial struct writeError
        {
            public error Err; // Original error.
        }

        private static void writeError(this ref state _s, error err) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            panic(new writeError(Err:err,));
        });

        // errRecover is the handler that turns panics into returns from the top
        // level of Parse.
        private static void errRecover(ref error _errp) => func(_errp, (ref error errp, Defer _, Panic panic, Recover __) =>
        {
            var e = recover();
            if (e != null)
            {
                switch (e.type())
                {
                    case runtime.Error err:
                        panic(e);
                        break;
                    case writeError err:
                        errp.Value = err.Err; // Strip the wrapper.
                        break;
                    case ExecError err:
                        errp.Value = err; // Keep the wrapper.
                        break;
                    default:
                    {
                        var err = e.type();
                        panic(e);
                        break;
                    }
                }
            }
        });

        // ExecuteTemplate applies the template associated with t that has the given name
        // to the specified data object and writes the output to wr.
        // If an error occurs executing the template or writing its output,
        // execution stops, but partial results may already have been written to
        // the output writer.
        // A template may be executed safely in parallel, although if parallel
        // executions share a Writer the output may be interleaved.
        private static error ExecuteTemplate(this ref Template t, io.Writer wr, @string name, object data)
        {
            ref Template tmpl = default;
            if (t.common != null)
            {
                tmpl = t.tmpl[name];
            }
            if (tmpl == null)
            {
                return error.As(fmt.Errorf("template: no template %q associated with template %q", name, t.name));
            }
            return error.As(tmpl.Execute(wr, data));
        }

        // Execute applies a parsed template to the specified data object,
        // and writes the output to wr.
        // If an error occurs executing the template or writing its output,
        // execution stops, but partial results may already have been written to
        // the output writer.
        // A template may be executed safely in parallel, although if parallel
        // executions share a Writer the output may be interleaved.
        //
        // If data is a reflect.Value, the template applies to the concrete
        // value that the reflect.Value holds, as in fmt.Print.
        private static error Execute(this ref Template t, io.Writer wr, object data)
        {
            return error.As(t.execute(wr, data));
        }

        private static error execute(this ref Template _t, io.Writer wr, object data) => func(_t, (ref Template t, Defer defer, Panic _, Recover __) =>
        {
            defer(errRecover(ref err));
            reflect.Value (value, ok) = data._<reflect.Value>();
            if (!ok)
            {
                value = reflect.ValueOf(data);
            }
            state state = ref new state(tmpl:t,wr:wr,vars:[]variable{{"$",value}},);
            if (t.Tree == null || t.Root == null)
            {
                state.errorf("%q is an incomplete or empty template", t.Name());
            }
            state.walk(value, t.Root);
            return;
        });

        // DefinedTemplates returns a string listing the defined templates,
        // prefixed by the string "; defined templates are: ". If there are none,
        // it returns the empty string. For generating an error message here
        // and in html/template.
        private static @string DefinedTemplates(this ref Template t)
        {
            if (t.common == null)
            {
                return "";
            }
            bytes.Buffer b = default;
            foreach (var (name, tmpl) in t.tmpl)
            {
                if (tmpl.Tree == null || tmpl.Root == null)
                {
                    continue;
                }
                if (b.Len() > 0L)
                {
                    b.WriteString(", ");
                }
                fmt.Fprintf(ref b, "%q", name);
            }
            @string s = default;
            if (b.Len() > 0L)
            {
                s = "; defined templates are: " + b.String();
            }
            return s;
        }

        // Walk functions step through the major pieces of the template structure,
        // generating output as they go.
        private static void walk(this ref state s, reflect.Value dot, parse.Node node)
        {
            s.at(node);
            switch (node.type())
            {
                case ref parse.ActionNode node:
                    var val = s.evalPipeline(dot, node.Pipe);
                    if (len(node.Pipe.Decl) == 0L)
                    {
                        s.printValue(node, val);
                    }
                    break;
                case ref parse.IfNode node:
                    s.walkIfOrWith(parse.NodeIf, dot, node.Pipe, node.List, node.ElseList);
                    break;
                case ref parse.ListNode node:
                    {
                        var node__prev1 = node;

                        foreach (var (_, __node) in node.Nodes)
                        {
                            node = __node;
                            s.walk(dot, node);
                        }

                        node = node__prev1;
                    }
                    break;
                case ref parse.RangeNode node:
                    s.walkRange(dot, node);
                    break;
                case ref parse.TemplateNode node:
                    s.walkTemplate(dot, node);
                    break;
                case ref parse.TextNode node:
                    {
                        var (_, err) = s.wr.Write(node.Text);

                        if (err != null)
                        {
                            s.writeError(err);
                        }

                    }
                    break;
                case ref parse.WithNode node:
                    s.walkIfOrWith(parse.NodeWith, dot, node.Pipe, node.List, node.ElseList);
                    break;
                default:
                {
                    var node = node.type();
                    s.errorf("unknown node: %s", node);
                    break;
                }
            }
        }

        // walkIfOrWith walks an 'if' or 'with' node. The two control structures
        // are identical in behavior except that 'with' sets dot.
        private static void walkIfOrWith(this ref state _s, parse.NodeType typ, reflect.Value dot, ref parse.PipeNode _pipe, ref parse.ListNode _list, ref parse.ListNode _elseList) => func(_s, _pipe, _list, _elseList, (ref state s, ref parse.PipeNode pipe, ref parse.ListNode list, ref parse.ListNode elseList, Defer defer, Panic _, Recover __) =>
        {
            defer(s.pop(s.mark()));
            var val = s.evalPipeline(dot, pipe);
            var (truth, ok) = isTrue(val);
            if (!ok)
            {
                s.errorf("if/with can't use %v", val);
            }
            if (truth)
            {
                if (typ == parse.NodeWith)
                {
                    s.walk(val, list);
                }
                else
                {
                    s.walk(dot, list);
                }
            }
            else if (elseList != null)
            {
                s.walk(dot, elseList);
            }
        });

        // IsTrue reports whether the value is 'true', in the sense of not the zero of its type,
        // and whether the value has a meaningful truth value. This is the definition of
        // truth used by if and other such actions.
        public static (bool, bool) IsTrue(object val)
        {
            return isTrue(reflect.ValueOf(val));
        }

        private static (bool, bool) isTrue(reflect.Value val)
        {
            if (!val.IsValid())
            { 
                // Something like var x interface{}, never set. It's a form of nil.
                return (false, true);
            }

            if (val.Kind() == reflect.Array || val.Kind() == reflect.Map || val.Kind() == reflect.Slice || val.Kind() == reflect.String) 
                truth = val.Len() > 0L;
            else if (val.Kind() == reflect.Bool) 
                truth = val.Bool();
            else if (val.Kind() == reflect.Complex64 || val.Kind() == reflect.Complex128) 
                truth = val.Complex() != 0L;
            else if (val.Kind() == reflect.Chan || val.Kind() == reflect.Func || val.Kind() == reflect.Ptr || val.Kind() == reflect.Interface) 
                truth = !val.IsNil();
            else if (val.Kind() == reflect.Int || val.Kind() == reflect.Int8 || val.Kind() == reflect.Int16 || val.Kind() == reflect.Int32 || val.Kind() == reflect.Int64) 
                truth = val.Int() != 0L;
            else if (val.Kind() == reflect.Float32 || val.Kind() == reflect.Float64) 
                truth = val.Float() != 0L;
            else if (val.Kind() == reflect.Uint || val.Kind() == reflect.Uint8 || val.Kind() == reflect.Uint16 || val.Kind() == reflect.Uint32 || val.Kind() == reflect.Uint64 || val.Kind() == reflect.Uintptr) 
                truth = val.Uint() != 0L;
            else if (val.Kind() == reflect.Struct) 
                truth = true; // Struct values are always true.
            else 
                return;
                        return (truth, true);
        }

        private static void walkRange(this ref state _s, reflect.Value dot, ref parse.RangeNode _r) => func(_s, _r, (ref state s, ref parse.RangeNode r, Defer defer, Panic _, Recover __) =>
        {
            s.at(r);
            defer(s.pop(s.mark()));
            var (val, _) = indirect(s.evalPipeline(dot, r.Pipe)); 
            // mark top of stack before any variables in the body are pushed.
            var mark = s.mark();
            Action<reflect.Value, reflect.Value> oneIteration = (index, elem) =>
            { 
                // Set top var (lexically the second if there are two) to the element.
                if (len(r.Pipe.Decl) > 0L)
                {
                    s.setVar(1L, elem);
                } 
                // Set next var (lexically the first if there are two) to the index.
                if (len(r.Pipe.Decl) > 1L)
                {
                    s.setVar(2L, index);
                }
                s.walk(elem, r.List);
                s.pop(mark);
            }
;

            if (val.Kind() == reflect.Array || val.Kind() == reflect.Slice) 
                if (val.Len() == 0L)
                {
                    break;
                }
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < val.Len(); i++)
                    {
                        oneIteration(reflect.ValueOf(i), val.Index(i));
                    }


                    i = i__prev1;
                }
                return;
            else if (val.Kind() == reflect.Map) 
                if (val.Len() == 0L)
                {
                    break;
                }
                foreach (var (_, key) in sortKeys(val.MapKeys()))
                {
                    oneIteration(key, val.MapIndex(key));
                }
                return;
            else if (val.Kind() == reflect.Chan) 
                if (val.IsNil())
                {
                    break;
                }
                i = 0L;
                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    var (elem, ok) = val.Recv();
                    if (!ok)
                    {
                        break;
                    i++;
                    }
                    oneIteration(reflect.ValueOf(i), elem);
                }

                if (i == 0L)
                {
                    break;
                }
                return;
            else if (val.Kind() == reflect.Invalid) 
                break; // An invalid value is likely a nil map, etc. and acts like an empty map.
            else 
                s.errorf("range can't iterate over %v", val);
                        if (r.ElseList != null)
            {
                s.walk(dot, r.ElseList);
            }
        });

        private static void walkTemplate(this ref state s, reflect.Value dot, ref parse.TemplateNode t)
        {
            s.at(t);
            var tmpl = s.tmpl.tmpl[t.Name];
            if (tmpl == null)
            {
                s.errorf("template %q not defined", t.Name);
            }
            if (s.depth == maxExecDepth)
            {
                s.errorf("exceeded maximum template depth (%v)", maxExecDepth);
            } 
            // Variables declared by the pipeline persist.
            dot = s.evalPipeline(dot, t.Pipe);
            var newState = s.Value;
            newState.depth++;
            newState.tmpl = tmpl; 
            // No dynamic scoping: template invocations inherit no variables.
            newState.vars = new slice<variable>(new variable[] { {"$",dot} });
            newState.walk(dot, tmpl.Root);
        }

        // Eval functions evaluate pipelines, commands, and their elements and extract
        // values from the data structure by examining fields, calling methods, and so on.
        // The printing of those values happens only through walk functions.

        // evalPipeline returns the value acquired by evaluating a pipeline. If the
        // pipeline has a variable declaration, the variable will be pushed on the
        // stack. Callers should therefore pop the stack after they are finished
        // executing commands depending on the pipeline value.
        private static reflect.Value evalPipeline(this ref state s, reflect.Value dot, ref parse.PipeNode pipe)
        {
            if (pipe == null)
            {
                return;
            }
            s.at(pipe);
            foreach (var (_, cmd) in pipe.Cmds)
            {
                value = s.evalCommand(dot, cmd, value); // previous value is this one's final arg.
                // If the object has type interface{}, dig down one level to the thing inside.
                if (value.Kind() == reflect.Interface && value.Type().NumMethod() == 0L)
                {
                    value = reflect.ValueOf(value.Interface()); // lovely!
                }
            }
            foreach (var (_, variable) in pipe.Decl)
            {
                s.push(variable.Ident[0L], value);
            }
            return value;
        }

        private static void notAFunction(this ref state s, slice<parse.Node> args, reflect.Value final)
        {
            if (len(args) > 1L || final.IsValid())
            {
                s.errorf("can't give argument to non-function %s", args[0L]);
            }
        }

        private static reflect.Value evalCommand(this ref state _s, reflect.Value dot, ref parse.CommandNode _cmd, reflect.Value final) => func(_s, _cmd, (ref state s, ref parse.CommandNode cmd, Defer _, Panic panic, Recover __) =>
        {
            var firstWord = cmd.Args[0L];
            switch (firstWord.type())
            {
                case ref parse.FieldNode n:
                    return s.evalFieldNode(dot, n, cmd.Args, final);
                    break;
                case ref parse.ChainNode n:
                    return s.evalChainNode(dot, n, cmd.Args, final);
                    break;
                case ref parse.IdentifierNode n:
                    return s.evalFunction(dot, n, cmd, cmd.Args, final);
                    break;
                case ref parse.PipeNode n:
                    return s.evalPipeline(dot, n);
                    break;
                case ref parse.VariableNode n:
                    return s.evalVariableNode(dot, n, cmd.Args, final);
                    break;
            }
            s.at(firstWord);
            s.notAFunction(cmd.Args, final);
            switch (firstWord.type())
            {
                case ref parse.BoolNode word:
                    return reflect.ValueOf(word.True);
                    break;
                case ref parse.DotNode word:
                    return dot;
                    break;
                case ref parse.NilNode word:
                    s.errorf("nil is not a command");
                    break;
                case ref parse.NumberNode word:
                    return s.idealConstant(word);
                    break;
                case ref parse.StringNode word:
                    return reflect.ValueOf(word.Text);
                    break;
            }
            s.errorf("can't evaluate command %q", firstWord);
            panic("not reached");
        });

        // idealConstant is called to return the value of a number in a context where
        // we don't know the type. In that case, the syntax of the number tells us
        // its type, and we use Go rules to resolve. Note there is no such thing as
        // a uint ideal constant in this situation - the value must be of int type.
        private static reflect.Value idealConstant(this ref state s, ref parse.NumberNode constant)
        { 
            // These are ideal constants but we don't know the type
            // and we have no context.  (If it was a method argument,
            // we'd know what we need.) The syntax guides us to some extent.
            s.at(constant);

            if (constant.IsComplex) 
                return reflect.ValueOf(constant.Complex128); // incontrovertible.
            else if (constant.IsFloat && !isHexConstant(constant.Text) && strings.ContainsAny(constant.Text, ".eE")) 
                return reflect.ValueOf(constant.Float64);
            else if (constant.IsInt) 
                var n = int(constant.Int64);
                if (int64(n) != constant.Int64)
                {
                    s.errorf("%s overflows int", constant.Text);
                }
                return reflect.ValueOf(n);
            else if (constant.IsUint) 
                s.errorf("%s overflows int", constant.Text);
                        return zero;
        }

        private static bool isHexConstant(@string s)
        {
            return len(s) > 2L && s[0L] == '0' && (s[1L] == 'x' || s[1L] == 'X');
        }

        private static reflect.Value evalFieldNode(this ref state s, reflect.Value dot, ref parse.FieldNode field, slice<parse.Node> args, reflect.Value final)
        {
            s.at(field);
            return s.evalFieldChain(dot, dot, field, field.Ident, args, final);
        }

        private static reflect.Value evalChainNode(this ref state s, reflect.Value dot, ref parse.ChainNode chain, slice<parse.Node> args, reflect.Value final)
        {
            s.at(chain);
            if (len(chain.Field) == 0L)
            {
                s.errorf("internal error: no fields in evalChainNode");
            }
            if (chain.Node.Type() == parse.NodeNil)
            {
                s.errorf("indirection through explicit nil in %s", chain);
            } 
            // (pipe).Field1.Field2 has pipe as .Node, fields as .Field. Eval the pipeline, then the fields.
            var pipe = s.evalArg(dot, null, chain.Node);
            return s.evalFieldChain(dot, pipe, chain, chain.Field, args, final);
        }

        private static reflect.Value evalVariableNode(this ref state s, reflect.Value dot, ref parse.VariableNode variable, slice<parse.Node> args, reflect.Value final)
        { 
            // $x.Field has $x as the first ident, Field as the second. Eval the var, then the fields.
            s.at(variable);
            var value = s.varValue(variable.Ident[0L]);
            if (len(variable.Ident) == 1L)
            {
                s.notAFunction(args, final);
                return value;
            }
            return s.evalFieldChain(dot, value, variable, variable.Ident[1L..], args, final);
        }

        // evalFieldChain evaluates .X.Y.Z possibly followed by arguments.
        // dot is the environment in which to evaluate arguments, while
        // receiver is the value being walked along the chain.
        private static reflect.Value evalFieldChain(this ref state s, reflect.Value dot, reflect.Value receiver, parse.Node node, slice<@string> ident, slice<parse.Node> args, reflect.Value final)
        {
            var n = len(ident);
            for (long i = 0L; i < n - 1L; i++)
            {
                receiver = s.evalField(dot, ident[i], node, null, zero, receiver);
            } 
            // Now if it's a method, it gets the arguments.
 
            // Now if it's a method, it gets the arguments.
            return s.evalField(dot, ident[n - 1L], node, args, final, receiver);
        }

        private static reflect.Value evalFunction(this ref state s, reflect.Value dot, ref parse.IdentifierNode node, parse.Node cmd, slice<parse.Node> args, reflect.Value final)
        {
            s.at(node);
            var name = node.Ident;
            var (function, ok) = findFunction(name, s.tmpl);
            if (!ok)
            {
                s.errorf("%q is not a defined function", name);
            }
            return s.evalCall(dot, function, cmd, name, args, final);
        }

        // evalField evaluates an expression like (.Field) or (.Field arg1 arg2).
        // The 'final' argument represents the return value from the preceding
        // value of the pipeline, if any.
        private static reflect.Value evalField(this ref state _s, reflect.Value dot, @string fieldName, parse.Node node, slice<parse.Node> args, reflect.Value final, reflect.Value receiver) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            if (!receiver.IsValid())
            {
                if (s.tmpl.option.missingKey == mapError)
                { // Treat invalid value as missing map key.
                    s.errorf("nil data; no entry for key %q", fieldName);
                }
                return zero;
            }
            var typ = receiver.Type();
            var (receiver, isNil) = indirect(receiver); 
            // Unless it's an interface, need to get to a value of type *T to guarantee
            // we see all methods of T and *T.
            var ptr = receiver;
            if (ptr.Kind() != reflect.Interface && ptr.Kind() != reflect.Ptr && ptr.CanAddr())
            {
                ptr = ptr.Addr();
            }
            {
                var method = ptr.MethodByName(fieldName);

                if (method.IsValid())
                {
                    return s.evalCall(dot, method, node, fieldName, args, final);
                }

            }
            var hasArgs = len(args) > 1L || final.IsValid(); 
            // It's not a method; must be a field of a struct or an element of a map.

            if (receiver.Kind() == reflect.Struct) 
                var (tField, ok) = receiver.Type().FieldByName(fieldName);
                if (ok)
                {
                    if (isNil)
                    {
                        s.errorf("nil pointer evaluating %s.%s", typ, fieldName);
                    }
                    var field = receiver.FieldByIndex(tField.Index);
                    if (tField.PkgPath != "")
                    { // field is unexported
                        s.errorf("%s is an unexported field of struct type %s", fieldName, typ);
                    } 
                    // If it's a function, we must call it.
                    if (hasArgs)
                    {
                        s.errorf("%s has arguments but cannot be invoked as function", fieldName);
                    }
                    return field;
                }
            else if (receiver.Kind() == reflect.Map) 
                if (isNil)
                {
                    s.errorf("nil pointer evaluating %s.%s", typ, fieldName);
                } 
                // If it's a map, attempt to use the field name as a key.
                var nameVal = reflect.ValueOf(fieldName);
                if (nameVal.Type().AssignableTo(receiver.Type().Key()))
                {
                    if (hasArgs)
                    {
                        s.errorf("%s is not a method but has arguments", fieldName);
                    }
                    var result = receiver.MapIndex(nameVal);
                    if (!result.IsValid())
                    {

                        if (s.tmpl.option.missingKey == mapInvalid)                         else if (s.tmpl.option.missingKey == mapZeroValue) 
                            result = reflect.Zero(receiver.Type().Elem());
                        else if (s.tmpl.option.missingKey == mapError) 
                            s.errorf("map has no entry for key %q", fieldName);
                                            }
                    return result;
                }
                        s.errorf("can't evaluate field %s in type %s", fieldName, typ);
            panic("not reached");
        });

        private static var errorType = reflect.TypeOf((error.Value)(null)).Elem();        private static var fmtStringerType = reflect.TypeOf((fmt.Stringer.Value)(null)).Elem();        private static var reflectValueType = reflect.TypeOf((reflect.Value.Value)(null)).Elem();

        // evalCall executes a function or method call. If it's a method, fun already has the receiver bound, so
        // it looks just like a function call. The arg list, if non-nil, includes (in the manner of the shell), arg[0]
        // as the function itself.
        private static reflect.Value evalCall(this ref state s, reflect.Value dot, reflect.Value fun, parse.Node node, @string name, slice<parse.Node> args, reflect.Value final)
        {
            if (args != null)
            {
                args = args[1L..]; // Zeroth arg is function name/node; not passed to function.
            }
            var typ = fun.Type();
            var numIn = len(args);
            if (final.IsValid())
            {
                numIn++;
            }
            var numFixed = len(args);
            if (typ.IsVariadic())
            {
                numFixed = typ.NumIn() - 1L; // last arg is the variadic one.
                if (numIn < numFixed)
                {
                    s.errorf("wrong number of args for %s: want at least %d got %d", name, typ.NumIn() - 1L, len(args));
                }
            }
            else if (numIn != typ.NumIn())
            {
                s.errorf("wrong number of args for %s: want %d got %d", name, typ.NumIn(), len(args));
            }
            if (!goodFunc(typ))
            { 
                // TODO: This could still be a confusing error; maybe goodFunc should provide info.
                s.errorf("can't call method/function %q with %d results", name, typ.NumOut());
            } 
            // Build the arg list.
            var argv = make_slice<reflect.Value>(numIn); 
            // Args must be evaluated. Fixed args first.
            long i = 0L;
            while (i < numFixed && i < len(args))
            {
                argv[i] = s.evalArg(dot, typ.In(i), args[i]);
                i++;
            } 
            // Now the ... args.
 
            // Now the ... args.
            if (typ.IsVariadic())
            {
                var argType = typ.In(typ.NumIn() - 1L).Elem(); // Argument is a slice.
                while (i < len(args))
                {
                    argv[i] = s.evalArg(dot, argType, args[i]);
                    i++;
                }

            } 
            // Add final value if necessary.
            if (final.IsValid())
            {
                var t = typ.In(typ.NumIn() - 1L);
                if (typ.IsVariadic())
                {
                    if (numIn - 1L < numFixed)
                    { 
                        // The added final argument corresponds to a fixed parameter of the function.
                        // Validate against the type of the actual parameter.
                        t = typ.In(numIn - 1L);
                    }
                    else
                    { 
                        // The added final argument corresponds to the variadic part.
                        // Validate against the type of the elements of the variadic slice.
                        t = t.Elem();
                    }
                }
                argv[i] = s.validateType(final, t);
            }
            var result = fun.Call(argv); 
            // If we have an error that is not nil, stop execution and return that error to the caller.
            if (len(result) == 2L && !result[1L].IsNil())
            {
                s.at(node);
                s.errorf("error calling %s: %s", name, result[1L].Interface()._<error>());
            }
            var v = result[0L];
            if (v.Type() == reflectValueType)
            {
                v = v.Interface()._<reflect.Value>();
            }
            return v;
        }

        // canBeNil reports whether an untyped nil can be assigned to the type. See reflect.Zero.
        private static bool canBeNil(reflect.Type typ)
        {

            if (typ.Kind() == reflect.Chan || typ.Kind() == reflect.Func || typ.Kind() == reflect.Interface || typ.Kind() == reflect.Map || typ.Kind() == reflect.Ptr || typ.Kind() == reflect.Slice) 
                return true;
            else if (typ.Kind() == reflect.Struct) 
                return typ == reflectValueType;
                        return false;
        }

        // validateType guarantees that the value is valid and assignable to the type.
        private static reflect.Value validateType(this ref state s, reflect.Value value, reflect.Type typ)
        {
            if (!value.IsValid())
            {
                if (typ == null || canBeNil(typ))
                { 
                    // An untyped nil interface{}. Accept as a proper nil value.
                    return reflect.Zero(typ);
                }
                s.errorf("invalid value; expected %s", typ);
            }
            if (typ == reflectValueType && value.Type() != typ)
            {
                return reflect.ValueOf(value);
            }
            if (typ != null && !value.Type().AssignableTo(typ))
            {
                if (value.Kind() == reflect.Interface && !value.IsNil())
                {
                    value = value.Elem();
                    if (value.Type().AssignableTo(typ))
                    {
                        return value;
                    } 
                    // fallthrough
                } 
                // Does one dereference or indirection work? We could do more, as we
                // do with method receivers, but that gets messy and method receivers
                // are much more constrained, so it makes more sense there than here.
                // Besides, one is almost always all you need.

                if (value.Kind() == reflect.Ptr && value.Type().Elem().AssignableTo(typ)) 
                    value = value.Elem();
                    if (!value.IsValid())
                    {
                        s.errorf("dereference of nil pointer of type %s", typ);
                    }
                else if (reflect.PtrTo(value.Type()).AssignableTo(typ) && value.CanAddr()) 
                    value = value.Addr();
                else 
                    s.errorf("wrong type for value; expected %s; got %s", typ, value.Type());
                            }
            return value;
        }

        private static reflect.Value evalArg(this ref state _s, reflect.Value dot, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            switch (n.type())
            {
                case ref parse.DotNode arg:
                    return s.validateType(dot, typ);
                    break;
                case ref parse.NilNode arg:
                    if (canBeNil(typ))
                    {
                        return reflect.Zero(typ);
                    }
                    s.errorf("cannot assign nil to %s", typ);
                    break;
                case ref parse.FieldNode arg:
                    return s.validateType(s.evalFieldNode(dot, arg, new slice<parse.Node>(new parse.Node[] { n }), zero), typ);
                    break;
                case ref parse.VariableNode arg:
                    return s.validateType(s.evalVariableNode(dot, arg, null, zero), typ);
                    break;
                case ref parse.PipeNode arg:
                    return s.validateType(s.evalPipeline(dot, arg), typ);
                    break;
                case ref parse.IdentifierNode arg:
                    return s.validateType(s.evalFunction(dot, arg, arg, null, zero), typ);
                    break;
                case ref parse.ChainNode arg:
                    return s.validateType(s.evalChainNode(dot, arg, null, zero), typ);
                    break;
            }

            if (typ.Kind() == reflect.Bool) 
                return s.evalBool(typ, n);
            else if (typ.Kind() == reflect.Complex64 || typ.Kind() == reflect.Complex128) 
                return s.evalComplex(typ, n);
            else if (typ.Kind() == reflect.Float32 || typ.Kind() == reflect.Float64) 
                return s.evalFloat(typ, n);
            else if (typ.Kind() == reflect.Int || typ.Kind() == reflect.Int8 || typ.Kind() == reflect.Int16 || typ.Kind() == reflect.Int32 || typ.Kind() == reflect.Int64) 
                return s.evalInteger(typ, n);
            else if (typ.Kind() == reflect.Interface) 
                if (typ.NumMethod() == 0L)
                {
                    return s.evalEmptyInterface(dot, n);
                }
            else if (typ.Kind() == reflect.Struct) 
                if (typ == reflectValueType)
                {
                    return reflect.ValueOf(s.evalEmptyInterface(dot, n));
                }
            else if (typ.Kind() == reflect.String) 
                return s.evalString(typ, n);
            else if (typ.Kind() == reflect.Uint || typ.Kind() == reflect.Uint8 || typ.Kind() == reflect.Uint16 || typ.Kind() == reflect.Uint32 || typ.Kind() == reflect.Uint64 || typ.Kind() == reflect.Uintptr) 
                return s.evalUnsignedInteger(typ, n);
                        s.errorf("can't handle %s for arg of type %s", n, typ);
            panic("not reached");
        });

        private static reflect.Value evalBool(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            {
                ref parse.BoolNode (n, ok) = n._<ref parse.BoolNode>();

                if (ok)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetBool(n.True);
                    return value;
                }

            }
            s.errorf("expected bool; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalString(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            {
                ref parse.StringNode (n, ok) = n._<ref parse.StringNode>();

                if (ok)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetString(n.Text);
                    return value;
                }

            }
            s.errorf("expected string; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalInteger(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            {
                ref parse.NumberNode (n, ok) = n._<ref parse.NumberNode>();

                if (ok && n.IsInt)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetInt(n.Int64);
                    return value;
                }

            }
            s.errorf("expected integer; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalUnsignedInteger(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            {
                ref parse.NumberNode (n, ok) = n._<ref parse.NumberNode>();

                if (ok && n.IsUint)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetUint(n.Uint64);
                    return value;
                }

            }
            s.errorf("expected unsigned integer; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalFloat(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            {
                ref parse.NumberNode (n, ok) = n._<ref parse.NumberNode>();

                if (ok && n.IsFloat)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetFloat(n.Float64);
                    return value;
                }

            }
            s.errorf("expected float; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalComplex(this ref state _s, reflect.Type typ, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            {
                ref parse.NumberNode (n, ok) = n._<ref parse.NumberNode>();

                if (ok && n.IsComplex)
                {
                    var value = reflect.New(typ).Elem();
                    value.SetComplex(n.Complex128);
                    return value;
                }

            }
            s.errorf("expected complex; found %s", n);
            panic("not reached");
        });

        private static reflect.Value evalEmptyInterface(this ref state _s, reflect.Value dot, parse.Node n) => func(_s, (ref state s, Defer _, Panic panic, Recover __) =>
        {
            s.at(n);
            switch (n.type())
            {
                case ref parse.BoolNode n:
                    return reflect.ValueOf(n.True);
                    break;
                case ref parse.DotNode n:
                    return dot;
                    break;
                case ref parse.FieldNode n:
                    return s.evalFieldNode(dot, n, null, zero);
                    break;
                case ref parse.IdentifierNode n:
                    return s.evalFunction(dot, n, n, null, zero);
                    break;
                case ref parse.NilNode n:
                    s.errorf("evalEmptyInterface: nil (can't happen)");
                    break;
                case ref parse.NumberNode n:
                    return s.idealConstant(n);
                    break;
                case ref parse.StringNode n:
                    return reflect.ValueOf(n.Text);
                    break;
                case ref parse.VariableNode n:
                    return s.evalVariableNode(dot, n, null, zero);
                    break;
                case ref parse.PipeNode n:
                    return s.evalPipeline(dot, n);
                    break;
            }
            s.errorf("can't handle assignment of %s to empty interface argument", n);
            panic("not reached");
        });

        // indirect returns the item at the end of indirection, and a bool to indicate if it's nil.
        private static (reflect.Value, bool) indirect(reflect.Value v)
        {
            while (v.Kind() == reflect.Ptr || v.Kind() == reflect.Interface)
            {
                if (v.IsNil())
                {
                    return (v, true);
                v = v.Elem();
                }
            }

            return (v, false);
        }

        // indirectInterface returns the concrete value in an interface value,
        // or else the zero reflect.Value.
        // That is, if v represents the interface value x, the result is the same as reflect.ValueOf(x):
        // the fact that x was an interface value is forgotten.
        private static reflect.Value indirectInterface(reflect.Value v)
        {
            if (v.Kind() != reflect.Interface)
            {
                return v;
            }
            if (v.IsNil())
            {
                return new reflect.Value();
            }
            return v.Elem();
        }

        // printValue writes the textual representation of the value to the output of
        // the template.
        private static void printValue(this ref state s, parse.Node n, reflect.Value v)
        {
            s.at(n);
            var (iface, ok) = printableValue(v);
            if (!ok)
            {
                s.errorf("can't print %s of type %s", n, v.Type());
            }
            var (_, err) = fmt.Fprint(s.wr, iface);
            if (err != null)
            {
                s.writeError(err);
            }
        }

        // printableValue returns the, possibly indirected, interface value inside v that
        // is best for a call to formatted printer.
        private static (object, bool) printableValue(reflect.Value v)
        {
            if (v.Kind() == reflect.Ptr)
            {
                v, _ = indirect(v); // fmt.Fprint handles nil.
            }
            if (!v.IsValid())
            {
                return ("<no value>", true);
            }
            if (!v.Type().Implements(errorType) && !v.Type().Implements(fmtStringerType))
            {
                if (v.CanAddr() && (reflect.PtrTo(v.Type()).Implements(errorType) || reflect.PtrTo(v.Type()).Implements(fmtStringerType)))
                {
                    v = v.Addr();
                }
                else
                {

                    if (v.Kind() == reflect.Chan || v.Kind() == reflect.Func) 
                        return (null, false);
                                    }
            }
            return (v.Interface(), true);
        }

        // sortKeys sorts (if it can) the slice of reflect.Values, which is a slice of map keys.
        private static slice<reflect.Value> sortKeys(slice<reflect.Value> v)
        {
            if (len(v) <= 1L)
            {
                return v;
            }

            if (v[0L].Kind() == reflect.Float32 || v[0L].Kind() == reflect.Float64) 
                sort.Slice(v, (i, j) =>
                {
                    return v[i].Float() < v[j].Float();
                });
            else if (v[0L].Kind() == reflect.Int || v[0L].Kind() == reflect.Int8 || v[0L].Kind() == reflect.Int16 || v[0L].Kind() == reflect.Int32 || v[0L].Kind() == reflect.Int64) 
                sort.Slice(v, (i, j) =>
                {
                    return v[i].Int() < v[j].Int();
                });
            else if (v[0L].Kind() == reflect.String) 
                sort.Slice(v, (i, j) =>
                {
                    return v[i].String() < v[j].String();
                });
            else if (v[0L].Kind() == reflect.Uint || v[0L].Kind() == reflect.Uint8 || v[0L].Kind() == reflect.Uint16 || v[0L].Kind() == reflect.Uint32 || v[0L].Kind() == reflect.Uint64 || v[0L].Kind() == reflect.Uintptr) 
                sort.Slice(v, (i, j) =>
                {
                    return v[i].Uint() < v[j].Uint();
                });
                        return v;
        }
    }
}}
