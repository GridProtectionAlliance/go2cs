// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 August 29 08:34:59 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Go\src\text\template\funcs.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace text
{
    public static partial class template_package
    {
        // FuncMap is the type of the map defining the mapping from names to functions.
        // Each function must have either a single return value, or two return values of
        // which the second has type error. In that case, if the second (error)
        // return value evaluates to non-nil during execution, execution terminates and
        // Execute returns that error.
        //
        // When template execution invokes a function with an argument list, that list
        // must be assignable to the function's parameter types. Functions meant to
        // apply to arguments of arbitrary type can use parameters of type interface{} or
        // of type reflect.Value. Similarly, functions meant to return a result of arbitrary
        // type can return interface{} or reflect.Value.
        private static FuncMap builtins = new FuncMap("and":and,"call":call,"html":HTMLEscaper,"index":index,"js":JSEscaper,"len":length,"not":not,"or":or,"print":fmt.Sprint,"printf":fmt.Sprintf,"println":fmt.Sprintln,"urlquery":URLQueryEscaper,"eq":eq,"ge":ge,"gt":gt,"le":le,"lt":lt,"ne":ne,);

        private static var builtinFuncs = createValueFuncs(builtins);

        // createValueFuncs turns a FuncMap into a map[string]reflect.Value
        private static map<@string, reflect.Value> createValueFuncs(FuncMap funcMap)
        {
            var m = make_map<@string, reflect.Value>();
            addValueFuncs(m, funcMap);
            return m;
        }

        // addValueFuncs adds to values the functions in funcs, converting them to reflect.Values.
        private static void addValueFuncs(map<@string, reflect.Value> @out, FuncMap @in) => func((_, panic, __) =>
        {
            foreach (var (name, fn) in in)
            {
                if (!goodName(name))
                {
                    panic(fmt.Errorf("function name %s is not a valid identifier", name));
                }
                var v = reflect.ValueOf(fn);
                if (v.Kind() != reflect.Func)
                {
                    panic("value for " + name + " not a function");
                }
                if (!goodFunc(v.Type()))
                {
                    panic(fmt.Errorf("can't install method/function %q with %d results", name, v.Type().NumOut()));
                }
                out[name] = v;
            }
        });

        // addFuncs adds to values the functions in funcs. It does no checking of the input -
        // call addValueFuncs first.
        private static void addFuncs(FuncMap @out, FuncMap @in)
        {
            foreach (var (name, fn) in in)
            {
                out[name] = fn;
            }
        }

        // goodFunc reports whether the function or method has the right result signature.
        private static bool goodFunc(reflect.Type typ)
        { 
            // We allow functions with 1 result or 2 results where the second is an error.

            if (typ.NumOut() == 1L) 
                return true;
            else if (typ.NumOut() == 2L && typ.Out(1L) == errorType) 
                return true;
                        return false;
        }

        // goodName reports whether the function name is a valid identifier.
        private static bool goodName(@string name)
        {
            if (name == "")
            {
                return false;
            }
            foreach (var (i, r) in name)
            {

                if (r == '_')                 else if (i == 0L && !unicode.IsLetter(r)) 
                    return false;
                else if (!unicode.IsLetter(r) && !unicode.IsDigit(r)) 
                    return false;
                            }
            return true;
        }

        // findFunction looks for a function in the template, and global map.
        private static (reflect.Value, bool) findFunction(@string name, ref Template _tmpl) => func(_tmpl, (ref Template tmpl, Defer defer, Panic _, Recover __) =>
        {
            if (tmpl != null && tmpl.common != null)
            {
                tmpl.muFuncs.RLock();
                defer(tmpl.muFuncs.RUnlock());
                {
                    var fn__prev2 = fn;

                    var fn = tmpl.execFuncs[name];

                    if (fn.IsValid())
                    {
                        return (fn, true);
                    }

                    fn = fn__prev2;

                }
            }
            {
                var fn__prev1 = fn;

                fn = builtinFuncs[name];

                if (fn.IsValid())
                {
                    return (fn, true);
                }

                fn = fn__prev1;

            }
            return (new reflect.Value(), false);
        });

        // prepareArg checks if value can be used as an argument of type argType, and
        // converts an invalid value to appropriate zero if possible.
        private static (reflect.Value, error) prepareArg(reflect.Value value, reflect.Type argType)
        {
            if (!value.IsValid())
            {
                if (!canBeNil(argType))
                {
                    return (new reflect.Value(), fmt.Errorf("value is nil; should be of type %s", argType));
                }
                value = reflect.Zero(argType);
            }
            if (value.Type().AssignableTo(argType))
            {
                return (value, null);
            }
            if (intLike(value.Kind()) && intLike(argType.Kind()) && value.Type().ConvertibleTo(argType))
            {
                value = value.Convert(argType);
                return (value, null);
            }
            return (new reflect.Value(), fmt.Errorf("value has type %s; should be %s", value.Type(), argType));
        }

        private static bool intLike(reflect.Kind typ)
        {

            if (typ == reflect.Int || typ == reflect.Int8 || typ == reflect.Int16 || typ == reflect.Int32 || typ == reflect.Int64) 
                return true;
            else if (typ == reflect.Uint || typ == reflect.Uint8 || typ == reflect.Uint16 || typ == reflect.Uint32 || typ == reflect.Uint64 || typ == reflect.Uintptr) 
                return true;
                        return false;
        }

        // Indexing.

        // index returns the result of indexing its first argument by the following
        // arguments. Thus "index x 1 2 3" is, in Go syntax, x[1][2][3]. Each
        // indexed item must be a map, slice, or array.
        private static (reflect.Value, error) index(reflect.Value item, params reflect.Value[] indices) => func((_, panic, __) =>
        {
            indices = indices.Clone();

            var v = indirectInterface(item);
            if (!v.IsValid())
            {
                return (new reflect.Value(), fmt.Errorf("index of untyped nil"));
            }
            foreach (var (_, i) in indices)
            {
                var index = indirectInterface(i);
                bool isNil = default;
                v, isNil = indirect(v);

                if (isNil)
                {
                    return (new reflect.Value(), fmt.Errorf("index of nil pointer"));
                }

                if (v.Kind() == reflect.Array || v.Kind() == reflect.Slice || v.Kind() == reflect.String) 
                    long x = default;

                    if (index.Kind() == reflect.Int || index.Kind() == reflect.Int8 || index.Kind() == reflect.Int16 || index.Kind() == reflect.Int32 || index.Kind() == reflect.Int64) 
                        x = index.Int();
                    else if (index.Kind() == reflect.Uint || index.Kind() == reflect.Uint8 || index.Kind() == reflect.Uint16 || index.Kind() == reflect.Uint32 || index.Kind() == reflect.Uint64 || index.Kind() == reflect.Uintptr) 
                        x = int64(index.Uint());
                    else if (index.Kind() == reflect.Invalid) 
                        return (new reflect.Value(), fmt.Errorf("cannot index slice/array with nil"));
                    else 
                        return (new reflect.Value(), fmt.Errorf("cannot index slice/array with type %s", index.Type()));
                                        if (x < 0L || x >= int64(v.Len()))
                    {
                        return (new reflect.Value(), fmt.Errorf("index out of range: %d", x));
                    }
                    v = v.Index(int(x));
                else if (v.Kind() == reflect.Map) 
                    var (index, err) = prepareArg(index, v.Type().Key());
                    if (err != null)
                    {
                        return (new reflect.Value(), err);
                    }
                    {
                        long x__prev1 = x;

                        x = v.MapIndex(index);

                        if (x.IsValid())
                        {
                            v = x;
                        }
                        else
                        {
                            v = reflect.Zero(v.Type().Elem());
                        }

                        x = x__prev1;

                    }
                else if (v.Kind() == reflect.Invalid) 
                    // the loop holds invariant: v.IsValid()
                    panic("unreachable");
                else 
                    return (new reflect.Value(), fmt.Errorf("can't index item of type %s", v.Type()));
                            }
            return (v, null);
        });

        // Length

        // length returns the length of the item, with an error if it has no defined length.
        private static (long, error) length(object item)
        {
            var v = reflect.ValueOf(item);
            if (!v.IsValid())
            {
                return (0L, fmt.Errorf("len of untyped nil"));
            }
            var (v, isNil) = indirect(v);
            if (isNil)
            {
                return (0L, fmt.Errorf("len of nil pointer"));
            }

            if (v.Kind() == reflect.Array || v.Kind() == reflect.Chan || v.Kind() == reflect.Map || v.Kind() == reflect.Slice || v.Kind() == reflect.String) 
                return (v.Len(), null);
                        return (0L, fmt.Errorf("len of type %s", v.Type()));
        }

        // Function invocation

        // call returns the result of evaluating the first argument as a function.
        // The function must return 1 result, or 2 results, the second of which is an error.
        private static (reflect.Value, error) call(reflect.Value fn, params reflect.Value[] args)
        {
            args = args.Clone();

            var v = indirectInterface(fn);
            if (!v.IsValid())
            {
                return (new reflect.Value(), fmt.Errorf("call of nil"));
            }
            var typ = v.Type();
            if (typ.Kind() != reflect.Func)
            {
                return (new reflect.Value(), fmt.Errorf("non-function of type %s", typ));
            }
            if (!goodFunc(typ))
            {
                return (new reflect.Value(), fmt.Errorf("function called with %d args; should be 1 or 2", typ.NumOut()));
            }
            var numIn = typ.NumIn();
            reflect.Type dddType = default;
            if (typ.IsVariadic())
            {
                if (len(args) < numIn - 1L)
                {
                    return (new reflect.Value(), fmt.Errorf("wrong number of args: got %d want at least %d", len(args), numIn - 1L));
                }
                dddType = typ.In(numIn - 1L).Elem();
            }
            else
            {
                if (len(args) != numIn)
                {
                    return (new reflect.Value(), fmt.Errorf("wrong number of args: got %d want %d", len(args), numIn));
                }
            }
            var argv = make_slice<reflect.Value>(len(args));
            foreach (var (i, arg) in args)
            {
                var value = indirectInterface(arg); 
                // Compute the expected type. Clumsy because of variadics.
                reflect.Type argType = default;
                if (!typ.IsVariadic() || i < numIn - 1L)
                {
                    argType = typ.In(i);
                }
                else
                {
                    argType = dddType;
                }
                error err = default;
                argv[i], err = prepareArg(value, argType);

                if (err != null)
                {
                    return (new reflect.Value(), fmt.Errorf("arg %d: %s", i, err));
                }
            }
            var result = v.Call(argv);
            if (len(result) == 2L && !result[1L].IsNil())
            {
                return (result[0L], result[1L].Interface()._<error>());
            }
            return (result[0L], null);
        }

        // Boolean logic.

        private static bool truth(reflect.Value arg)
        {
            var (t, _) = isTrue(indirectInterface(arg));
            return t;
        }

        // and computes the Boolean AND of its arguments, returning
        // the first false argument it encounters, or the last argument.
        private static reflect.Value and(reflect.Value arg0, params reflect.Value[] args)
        {
            args = args.Clone();

            if (!truth(arg0))
            {
                return arg0;
            }
            foreach (var (i) in args)
            {
                arg0 = args[i];
                if (!truth(arg0))
                {
                    break;
                }
            }
            return arg0;
        }

        // or computes the Boolean OR of its arguments, returning
        // the first true argument it encounters, or the last argument.
        private static reflect.Value or(reflect.Value arg0, params reflect.Value[] args)
        {
            args = args.Clone();

            if (truth(arg0))
            {
                return arg0;
            }
            foreach (var (i) in args)
            {
                arg0 = args[i];
                if (truth(arg0))
                {
                    break;
                }
            }
            return arg0;
        }

        // not returns the Boolean negation of its argument.
        private static bool not(reflect.Value arg)
        {
            return !truth(arg);
        }

        // Comparison.

        // TODO: Perhaps allow comparison between signed and unsigned integers.

        private static var errBadComparisonType = errors.New("invalid type for comparison");        private static var errBadComparison = errors.New("incompatible types for comparison");        private static var errNoComparison = errors.New("missing argument for comparison");

        private partial struct kind // : long
        {
        }

        private static readonly kind invalidKind = iota;
        private static readonly var boolKind = 0;
        private static readonly var complexKind = 1;
        private static readonly var intKind = 2;
        private static readonly var floatKind = 3;
        private static readonly var stringKind = 4;
        private static readonly var uintKind = 5;

        private static (kind, error) basicKind(reflect.Value v)
        {

            if (v.Kind() == reflect.Bool) 
                return (boolKind, null);
            else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                return (intKind, null);
            else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                return (uintKind, null);
            else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                return (floatKind, null);
            else if (v.Kind() == reflect.Complex64 || v.Kind() == reflect.Complex128) 
                return (complexKind, null);
            else if (v.Kind() == reflect.String) 
                return (stringKind, null);
                        return (invalidKind, errBadComparisonType);
        }

        // eq evaluates the comparison a == b || a == c || ...
        private static (bool, error) eq(reflect.Value arg1, params reflect.Value[] arg2) => func((_, panic, __) =>
        {
            arg2 = arg2.Clone();

            var v1 = indirectInterface(arg1);
            var (k1, err) = basicKind(v1);
            if (err != null)
            {
                return (false, err);
            }
            if (len(arg2) == 0L)
            {
                return (false, errNoComparison);
            }
            foreach (var (_, arg) in arg2)
            {
                var v2 = indirectInterface(arg);
                var (k2, err) = basicKind(v2);
                if (err != null)
                {
                    return (false, err);
                }
                var truth = false;
                if (k1 != k2)
                { 
                    // Special case: Can compare integer values regardless of type's sign.

                    if (k1 == intKind && k2 == uintKind) 
                        truth = v1.Int() >= 0L && uint64(v1.Int()) == v2.Uint();
                    else if (k1 == uintKind && k2 == intKind) 
                        truth = v2.Int() >= 0L && v1.Uint() == uint64(v2.Int());
                    else 
                        return (false, errBadComparison);
                                    }
                else
                {

                    if (k1 == boolKind) 
                        truth = v1.Bool() == v2.Bool();
                    else if (k1 == complexKind) 
                        truth = v1.Complex() == v2.Complex();
                    else if (k1 == floatKind) 
                        truth = v1.Float() == v2.Float();
                    else if (k1 == intKind) 
                        truth = v1.Int() == v2.Int();
                    else if (k1 == stringKind) 
                        truth = v1.String() == v2.String();
                    else if (k1 == uintKind) 
                        truth = v1.Uint() == v2.Uint();
                    else 
                        panic("invalid kind");
                                    }
                if (truth)
                {
                    return (true, null);
                }
            }
            return (false, null);
        });

        // ne evaluates the comparison a != b.
        private static (bool, error) ne(reflect.Value arg1, reflect.Value arg2)
        { 
            // != is the inverse of ==.
            var (equal, err) = eq(arg1, arg2);
            return (!equal, err);
        }

        // lt evaluates the comparison a < b.
        private static (bool, error) lt(reflect.Value arg1, reflect.Value arg2) => func((_, panic, __) =>
        {
            var v1 = indirectInterface(arg1);
            var (k1, err) = basicKind(v1);
            if (err != null)
            {
                return (false, err);
            }
            var v2 = indirectInterface(arg2);
            var (k2, err) = basicKind(v2);
            if (err != null)
            {
                return (false, err);
            }
            var truth = false;
            if (k1 != k2)
            { 
                // Special case: Can compare integer values regardless of type's sign.

                if (k1 == intKind && k2 == uintKind) 
                    truth = v1.Int() < 0L || uint64(v1.Int()) < v2.Uint();
                else if (k1 == uintKind && k2 == intKind) 
                    truth = v2.Int() >= 0L && v1.Uint() < uint64(v2.Int());
                else 
                    return (false, errBadComparison);
                            }
            else
            {

                if (k1 == boolKind || k1 == complexKind) 
                    return (false, errBadComparisonType);
                else if (k1 == floatKind) 
                    truth = v1.Float() < v2.Float();
                else if (k1 == intKind) 
                    truth = v1.Int() < v2.Int();
                else if (k1 == stringKind) 
                    truth = v1.String() < v2.String();
                else if (k1 == uintKind) 
                    truth = v1.Uint() < v2.Uint();
                else 
                    panic("invalid kind");
                            }
            return (truth, null);
        });

        // le evaluates the comparison <= b.
        private static (bool, error) le(reflect.Value arg1, reflect.Value arg2)
        { 
            // <= is < or ==.
            var (lessThan, err) = lt(arg1, arg2);
            if (lessThan || err != null)
            {
                return (lessThan, err);
            }
            return eq(arg1, arg2);
        }

        // gt evaluates the comparison a > b.
        private static (bool, error) gt(reflect.Value arg1, reflect.Value arg2)
        { 
            // > is the inverse of <=.
            var (lessOrEqual, err) = le(arg1, arg2);
            if (err != null)
            {
                return (false, err);
            }
            return (!lessOrEqual, null);
        }

        // ge evaluates the comparison a >= b.
        private static (bool, error) ge(reflect.Value arg1, reflect.Value arg2)
        { 
            // >= is the inverse of <.
            var (lessThan, err) = lt(arg1, arg2);
            if (err != null)
            {
                return (false, err);
            }
            return (!lessThan, null);
        }

        // HTML escaping.

        private static slice<byte> htmlQuot = (slice<byte>)"&#34;";        private static slice<byte> htmlApos = (slice<byte>)"&#39;";        private static slice<byte> htmlAmp = (slice<byte>)"&amp;";        private static slice<byte> htmlLt = (slice<byte>)"&lt;";        private static slice<byte> htmlGt = (slice<byte>)"&gt;";        private static slice<byte> htmlNull = (slice<byte>)"\uFFFD";

        // HTMLEscape writes to w the escaped HTML equivalent of the plain text data b.
        public static void HTMLEscape(io.Writer w, slice<byte> b)
        {
            long last = 0L;
            foreach (var (i, c) in b)
            {
                slice<byte> html = default;
                switch (c)
                {
                    case ' ': 
                        html = htmlNull;
                        break;
                    case '"': 
                        html = htmlQuot;
                        break;
                    case '\'': 
                        html = htmlApos;
                        break;
                    case '&': 
                        html = htmlAmp;
                        break;
                    case '<': 
                        html = htmlLt;
                        break;
                    case '>': 
                        html = htmlGt;
                        break;
                    default: 
                        continue;
                        break;
                }
                w.Write(b[last..i]);
                w.Write(html);
                last = i + 1L;
            }
            w.Write(b[last..]);
        }

        // HTMLEscapeString returns the escaped HTML equivalent of the plain text data s.
        public static @string HTMLEscapeString(@string s)
        { 
            // Avoid allocation if we can.
            if (!strings.ContainsAny(s, "'\"&<> "))
            {
                return s;
            }
            bytes.Buffer b = default;
            HTMLEscape(ref b, (slice<byte>)s);
            return b.String();
        }

        // HTMLEscaper returns the escaped HTML equivalent of the textual
        // representation of its arguments.
        public static @string HTMLEscaper(params object[] args)
        {
            args = args.Clone();

            return HTMLEscapeString(evalArgs(args));
        }

        // JavaScript escaping.

        private static slice<byte> jsLowUni = (slice<byte>)"\\u00";        private static slice<byte> hex = (slice<byte>)"0123456789ABCDEF";        private static slice<byte> jsBackslash = (slice<byte>)"\\\\";        private static slice<byte> jsApos = (slice<byte>)"\\\'";        private static slice<byte> jsQuot = (slice<byte>)"\\\"";        private static slice<byte> jsLt = (slice<byte>)"\\x3C";        private static slice<byte> jsGt = (slice<byte>)"\\x3E";

        // JSEscape writes to w the escaped JavaScript equivalent of the plain text data b.
        public static void JSEscape(io.Writer w, slice<byte> b)
        {
            long last = 0L;
            for (long i = 0L; i < len(b); i++)
            {
                var c = b[i];

                if (!jsIsSpecial(rune(c)))
                { 
                    // fast path: nothing to do
                    continue;
                }
                w.Write(b[last..i]);

                if (c < utf8.RuneSelf)
                { 
                    // Quotes, slashes and angle brackets get quoted.
                    // Control characters get written as \u00XX.
                    switch (c)
                    {
                        case '\\': 
                            w.Write(jsBackslash);
                            break;
                        case '\'': 
                            w.Write(jsApos);
                            break;
                        case '"': 
                            w.Write(jsQuot);
                            break;
                        case '<': 
                            w.Write(jsLt);
                            break;
                        case '>': 
                            w.Write(jsGt);
                            break;
                        default: 
                            w.Write(jsLowUni);
                            var t = c >> (int)(4L);
                            var b = c & 0x0fUL;
                            w.Write(hex[t..t + 1L]);
                            w.Write(hex[b..b + 1L]);
                            break;
                    }
                }
                else
                { 
                    // Unicode rune.
                    var (r, size) = utf8.DecodeRune(b[i..]);
                    if (unicode.IsPrint(r))
                    {
                        w.Write(b[i..i + size]);
                    }
                    else
                    {
                        fmt.Fprintf(w, "\\u%04X", r);
                    }
                    i += size - 1L;
                }
                last = i + 1L;
            }

            w.Write(b[last..]);
        }

        // JSEscapeString returns the escaped JavaScript equivalent of the plain text data s.
        public static @string JSEscapeString(@string s)
        { 
            // Avoid allocation if we can.
            if (strings.IndexFunc(s, jsIsSpecial) < 0L)
            {
                return s;
            }
            bytes.Buffer b = default;
            JSEscape(ref b, (slice<byte>)s);
            return b.String();
        }

        private static bool jsIsSpecial(int r)
        {
            switch (r)
            {
                case '\\': 

                case '\'': 

                case '"': 

                case '<': 

                case '>': 
                    return true;
                    break;
            }
            return r < ' ' || utf8.RuneSelf <= r;
        }

        // JSEscaper returns the escaped JavaScript equivalent of the textual
        // representation of its arguments.
        public static @string JSEscaper(params object[] args)
        {
            args = args.Clone();

            return JSEscapeString(evalArgs(args));
        }

        // URLQueryEscaper returns the escaped value of the textual representation of
        // its arguments in a form suitable for embedding in a URL query.
        public static @string URLQueryEscaper(params object[] args)
        {
            args = args.Clone();

            return url.QueryEscape(evalArgs(args));
        }

        // evalArgs formats the list of arguments into a string. It is therefore equivalent to
        //    fmt.Sprint(args...)
        // except that each argument is indirected (if a pointer), as required,
        // using the same rules as the default string evaluation during template
        // execution.
        private static @string evalArgs(slice<object> args)
        {
            var ok = false;
            @string s = default; 
            // Fast path for simple common case.
            if (len(args) == 1L)
            {
                s, ok = args[0L]._<@string>();
            }
            if (!ok)
            {
                foreach (var (i, arg) in args)
                {
                    var (a, ok) = printableValue(reflect.ValueOf(arg));
                    if (ok)
                    {
                        args[i] = a;
                    } // else let fmt do its thing
                }
                s = fmt.Sprint(args);
            }
            return s;
        }
    }
}}
