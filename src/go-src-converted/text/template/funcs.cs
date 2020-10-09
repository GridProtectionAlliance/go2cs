// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 October 09 04:59:34 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Go\src\text\template\funcs.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

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
        private static FuncMap builtins()
        {
            return new FuncMap("and":and,"call":call,"html":HTMLEscaper,"index":index,"slice":slice,"js":JSEscaper,"len":length,"not":not,"or":or,"print":fmt.Sprint,"printf":fmt.Sprintf,"println":fmt.Sprintln,"urlquery":URLQueryEscaper,"eq":eq,"ge":ge,"gt":gt,"le":le,"lt":lt,"ne":ne,);
        }

        private static var builtinFuncsOnce = default;

        // builtinFuncsOnce lazily computes & caches the builtinFuncs map.
        // TODO: revert this back to a global map once golang.org/issue/2559 is fixed.
        private static map<@string, reflect.Value> builtinFuncs()
        {
            builtinFuncsOnce.Do(() =>
            {
                builtinFuncsOnce.v = createValueFuncs(builtins());
            });
            return builtinFuncsOnce.v;

        }

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
                    panic(fmt.Errorf("function name %q is not a valid identifier", name));
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
        private static (reflect.Value, bool) findFunction(@string name, ptr<Template> _addr_tmpl) => func((defer, _, __) =>
        {
            reflect.Value _p0 = default;
            bool _p0 = default;
            ref Template tmpl = ref _addr_tmpl.val;

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

                fn = builtinFuncs()[name];

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
            reflect.Value _p0 = default;
            error _p0 = default!;

            if (!value.IsValid())
            {
                if (!canBeNil(argType))
                {
                    return (new reflect.Value(), error.As(fmt.Errorf("value is nil; should be of type %s", argType))!);
                }

                value = reflect.Zero(argType);

            }

            if (value.Type().AssignableTo(argType))
            {
                return (value, error.As(null!)!);
            }

            if (intLike(value.Kind()) && intLike(argType.Kind()) && value.Type().ConvertibleTo(argType))
            {
                value = value.Convert(argType);
                return (value, error.As(null!)!);
            }

            return (new reflect.Value(), error.As(fmt.Errorf("value has type %s; should be %s", value.Type(), argType))!);

        }

        private static bool intLike(reflect.Kind typ)
        {

            if (typ == reflect.Int || typ == reflect.Int8 || typ == reflect.Int16 || typ == reflect.Int32 || typ == reflect.Int64) 
                return true;
            else if (typ == reflect.Uint || typ == reflect.Uint8 || typ == reflect.Uint16 || typ == reflect.Uint32 || typ == reflect.Uint64 || typ == reflect.Uintptr) 
                return true;
                        return false;

        }

        // indexArg checks if a reflect.Value can be used as an index, and converts it to int if possible.
        private static (long, error) indexArg(reflect.Value index, long cap)
        {
            long _p0 = default;
            error _p0 = default!;

            long x = default;

            if (index.Kind() == reflect.Int || index.Kind() == reflect.Int8 || index.Kind() == reflect.Int16 || index.Kind() == reflect.Int32 || index.Kind() == reflect.Int64) 
                x = index.Int();
            else if (index.Kind() == reflect.Uint || index.Kind() == reflect.Uint8 || index.Kind() == reflect.Uint16 || index.Kind() == reflect.Uint32 || index.Kind() == reflect.Uint64 || index.Kind() == reflect.Uintptr) 
                x = int64(index.Uint());
            else if (index.Kind() == reflect.Invalid) 
                return (0L, error.As(fmt.Errorf("cannot index slice/array with nil"))!);
            else 
                return (0L, error.As(fmt.Errorf("cannot index slice/array with type %s", index.Type()))!);
                        if (x < 0L || int(x) < 0L || int(x) > cap)
            {
                return (0L, error.As(fmt.Errorf("index out of range: %d", x))!);
            }

            return (int(x), error.As(null!)!);

        }

        // Indexing.

        // index returns the result of indexing its first argument by the following
        // arguments. Thus "index x 1 2 3" is, in Go syntax, x[1][2][3]. Each
        // indexed item must be a map, slice, or array.
        private static (reflect.Value, error) index(reflect.Value item, params reflect.Value[] indexes) => func((_, panic, __) =>
        {
            reflect.Value _p0 = default;
            error _p0 = default!;
            indexes = indexes.Clone();

            item = indirectInterface(item);
            if (!item.IsValid())
            {
                return (new reflect.Value(), error.As(fmt.Errorf("index of untyped nil"))!);
            }

            {
                var index__prev1 = index;

                foreach (var (_, __index) in indexes)
                {
                    index = __index;
                    index = indirectInterface(index);
                    bool isNil = default;
                    item, isNil = indirect(item);

                    if (isNil)
                    {
                        return (new reflect.Value(), error.As(fmt.Errorf("index of nil pointer"))!);
                    }


                    if (item.Kind() == reflect.Array || item.Kind() == reflect.Slice || item.Kind() == reflect.String) 
                        var (x, err) = indexArg(index, item.Len());
                        if (err != null)
                        {
                            return (new reflect.Value(), error.As(err)!);
                        }

                        item = item.Index(x);
                    else if (item.Kind() == reflect.Map) 
                        var (index, err) = prepareArg(index, item.Type().Key());
                        if (err != null)
                        {
                            return (new reflect.Value(), error.As(err)!);
                        }

                        {
                            var x__prev1 = x;

                            var x = item.MapIndex(index);

                            if (x.IsValid())
                            {
                                item = x;
                            }
                            else
                            {
                                item = reflect.Zero(item.Type().Elem());
                            }

                            x = x__prev1;

                        }

                    else if (item.Kind() == reflect.Invalid) 
                        // the loop holds invariant: item.IsValid()
                        panic("unreachable");
                    else 
                        return (new reflect.Value(), error.As(fmt.Errorf("can't index item of type %s", item.Type()))!);
                    
                }

                index = index__prev1;
            }

            return (item, error.As(null!)!);

        });

        // Slicing.

        // slice returns the result of slicing its first argument by the remaining
        // arguments. Thus "slice x 1 2" is, in Go syntax, x[1:2], while "slice x"
        // is x[:], "slice x 1" is x[1:], and "slice x 1 2 3" is x[1:2:3]. The first
        // argument must be a string, slice, or array.
        private static (reflect.Value, error) slice(reflect.Value item, params reflect.Value[] indexes)
        {
            reflect.Value _p0 = default;
            error _p0 = default!;
            indexes = indexes.Clone();

            item = indirectInterface(item);
            if (!item.IsValid())
            {
                return (new reflect.Value(), error.As(fmt.Errorf("slice of untyped nil"))!);
            }

            if (len(indexes) > 3L)
            {
                return (new reflect.Value(), error.As(fmt.Errorf("too many slice indexes: %d", len(indexes)))!);
            }

            long cap = default;

            if (item.Kind() == reflect.String) 
                if (len(indexes) == 3L)
                {
                    return (new reflect.Value(), error.As(fmt.Errorf("cannot 3-index slice a string"))!);
                }

                cap = item.Len();
            else if (item.Kind() == reflect.Array || item.Kind() == reflect.Slice) 
                cap = item.Cap();
            else 
                return (new reflect.Value(), error.As(fmt.Errorf("can't slice item of type %s", item.Type()))!);
            // set default values for cases item[:], item[i:].
            array<long> idx = new array<long>(new long[] { 0, item.Len() });
            foreach (var (i, index) in indexes)
            {
                var (x, err) = indexArg(index, cap);
                if (err != null)
                {
                    return (new reflect.Value(), error.As(err)!);
                }

                idx[i] = x;

            } 
            // given item[i:j], make sure i <= j.
            if (idx[0L] > idx[1L])
            {
                return (new reflect.Value(), error.As(fmt.Errorf("invalid slice index: %d > %d", idx[0L], idx[1L]))!);
            }

            if (len(indexes) < 3L)
            {
                return (item.Slice(idx[0L], idx[1L]), error.As(null!)!);
            } 
            // given item[i:j:k], make sure i <= j <= k.
            if (idx[1L] > idx[2L])
            {
                return (new reflect.Value(), error.As(fmt.Errorf("invalid slice index: %d > %d", idx[1L], idx[2L]))!);
            }

            return (item.Slice3(idx[0L], idx[1L], idx[2L]), error.As(null!)!);

        }

        // Length

        // length returns the length of the item, with an error if it has no defined length.
        private static (long, error) length(reflect.Value item)
        {
            long _p0 = default;
            error _p0 = default!;

            var (item, isNil) = indirect(item);
            if (isNil)
            {
                return (0L, error.As(fmt.Errorf("len of nil pointer"))!);
            }


            if (item.Kind() == reflect.Array || item.Kind() == reflect.Chan || item.Kind() == reflect.Map || item.Kind() == reflect.Slice || item.Kind() == reflect.String) 
                return (item.Len(), error.As(null!)!);
                        return (0L, error.As(fmt.Errorf("len of type %s", item.Type()))!);

        }

        // Function invocation

        // call returns the result of evaluating the first argument as a function.
        // The function must return 1 result, or 2 results, the second of which is an error.
        private static (reflect.Value, error) call(reflect.Value fn, params reflect.Value[] args)
        {
            reflect.Value _p0 = default;
            error _p0 = default!;
            args = args.Clone();

            fn = indirectInterface(fn);
            if (!fn.IsValid())
            {
                return (new reflect.Value(), error.As(fmt.Errorf("call of nil"))!);
            }

            var typ = fn.Type();
            if (typ.Kind() != reflect.Func)
            {
                return (new reflect.Value(), error.As(fmt.Errorf("non-function of type %s", typ))!);
            }

            if (!goodFunc(typ))
            {
                return (new reflect.Value(), error.As(fmt.Errorf("function called with %d args; should be 1 or 2", typ.NumOut()))!);
            }

            var numIn = typ.NumIn();
            reflect.Type dddType = default;
            if (typ.IsVariadic())
            {
                if (len(args) < numIn - 1L)
                {
                    return (new reflect.Value(), error.As(fmt.Errorf("wrong number of args: got %d want at least %d", len(args), numIn - 1L))!);
                }

                dddType = typ.In(numIn - 1L).Elem();

            }
            else
            {
                if (len(args) != numIn)
                {
                    return (new reflect.Value(), error.As(fmt.Errorf("wrong number of args: got %d want %d", len(args), numIn))!);
                }

            }

            var argv = make_slice<reflect.Value>(len(args));
            foreach (var (i, arg) in args)
            {
                arg = indirectInterface(arg); 
                // Compute the expected type. Clumsy because of variadics.
                var argType = dddType;
                if (!typ.IsVariadic() || i < numIn - 1L)
                {
                    argType = typ.In(i);
                }

                error err = default!;
                argv[i], err = prepareArg(arg, argType);

                if (err != null)
                {
                    return (new reflect.Value(), error.As(fmt.Errorf("arg %d: %s", i, err))!);
                }

            }
            return safeCall(fn, argv);

        }

        // safeCall runs fun.Call(args), and returns the resulting value and error, if
        // any. If the call panics, the panic value is returned as an error.
        private static (reflect.Value, error) safeCall(reflect.Value fun, slice<reflect.Value> args) => func((defer, _, __) =>
        {
            reflect.Value val = default;
            error err = default!;

            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null)
                    {
                        {
                            error (e, ok) = error.As(r._<error>())!;

                            if (ok)
                            {
                                err = e;
                            }
                            else
                            {
                                err = fmt.Errorf("%v", r);
                            }

                        }

                    }

                }

            }());
            var ret = fun.Call(args);
            if (len(ret) == 2L && !ret[1L].IsNil())
            {
                return (ret[0L], error.As(ret[1L].Interface()._<error>()!)!);
            }

            return (ret[0L], error.As(null!)!);

        });

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

        private static readonly kind invalidKind = (kind)iota;
        private static readonly var boolKind = 0;
        private static readonly var complexKind = 1;
        private static readonly var intKind = 2;
        private static readonly var floatKind = 3;
        private static readonly var stringKind = 4;
        private static readonly var uintKind = 5;


        private static (kind, error) basicKind(reflect.Value v)
        {
            kind _p0 = default;
            error _p0 = default!;


            if (v.Kind() == reflect.Bool) 
                return (boolKind, error.As(null!)!);
            else if (v.Kind() == reflect.Int || v.Kind() == reflect.Int8 || v.Kind() == reflect.Int16 || v.Kind() == reflect.Int32 || v.Kind() == reflect.Int64) 
                return (intKind, error.As(null!)!);
            else if (v.Kind() == reflect.Uint || v.Kind() == reflect.Uint8 || v.Kind() == reflect.Uint16 || v.Kind() == reflect.Uint32 || v.Kind() == reflect.Uint64 || v.Kind() == reflect.Uintptr) 
                return (uintKind, error.As(null!)!);
            else if (v.Kind() == reflect.Float32 || v.Kind() == reflect.Float64) 
                return (floatKind, error.As(null!)!);
            else if (v.Kind() == reflect.Complex64 || v.Kind() == reflect.Complex128) 
                return (complexKind, error.As(null!)!);
            else if (v.Kind() == reflect.String) 
                return (stringKind, error.As(null!)!);
                        return (invalidKind, error.As(errBadComparisonType)!);

        }

        // eq evaluates the comparison a == b || a == c || ...
        private static (bool, error) eq(reflect.Value arg1, params reflect.Value[] arg2)
        {
            bool _p0 = default;
            error _p0 = default!;
            arg2 = arg2.Clone();

            arg1 = indirectInterface(arg1);
            if (arg1 != zero)
            {
                {
                    var t1 = arg1.Type();

                    if (!t1.Comparable())
                    {
                        return (false, error.As(fmt.Errorf("uncomparable type %s: %v", t1, arg1))!);
                    }

                }

            }

            if (len(arg2) == 0L)
            {
                return (false, error.As(errNoComparison)!);
            }

            var (k1, _) = basicKind(arg1);
            foreach (var (_, arg) in arg2)
            {
                arg = indirectInterface(arg);
                var (k2, _) = basicKind(arg);
                var truth = false;
                if (k1 != k2)
                { 
                    // Special case: Can compare integer values regardless of type's sign.

                    if (k1 == intKind && k2 == uintKind) 
                        truth = arg1.Int() >= 0L && uint64(arg1.Int()) == arg.Uint();
                    else if (k1 == uintKind && k2 == intKind) 
                        truth = arg.Int() >= 0L && arg1.Uint() == uint64(arg.Int());
                    else 
                        return (false, error.As(errBadComparison)!);
                    
                }
                else
                {

                    if (k1 == boolKind) 
                        truth = arg1.Bool() == arg.Bool();
                    else if (k1 == complexKind) 
                        truth = arg1.Complex() == arg.Complex();
                    else if (k1 == floatKind) 
                        truth = arg1.Float() == arg.Float();
                    else if (k1 == intKind) 
                        truth = arg1.Int() == arg.Int();
                    else if (k1 == stringKind) 
                        truth = arg1.String() == arg.String();
                    else if (k1 == uintKind) 
                        truth = arg1.Uint() == arg.Uint();
                    else 
                        if (arg == zero)
                        {
                            truth = arg1 == arg;
                        }
                        else
                        {
                            {
                                var t2 = arg.Type();

                                if (!t2.Comparable())
                                {
                                    return (false, error.As(fmt.Errorf("uncomparable type %s: %v", t2, arg))!);
                                }

                            }

                            truth = arg1.Interface() == arg.Interface();

                        }

                                    }

                if (truth)
                {
                    return (true, error.As(null!)!);
                }

            }
            return (false, error.As(null!)!);

        }

        // ne evaluates the comparison a != b.
        private static (bool, error) ne(reflect.Value arg1, reflect.Value arg2)
        {
            bool _p0 = default;
            error _p0 = default!;
 
            // != is the inverse of ==.
            var (equal, err) = eq(arg1, arg2);
            return (!equal, error.As(err)!);

        }

        // lt evaluates the comparison a < b.
        private static (bool, error) lt(reflect.Value arg1, reflect.Value arg2) => func((_, panic, __) =>
        {
            bool _p0 = default;
            error _p0 = default!;

            arg1 = indirectInterface(arg1);
            var (k1, err) = basicKind(arg1);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            arg2 = indirectInterface(arg2);
            var (k2, err) = basicKind(arg2);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            var truth = false;
            if (k1 != k2)
            { 
                // Special case: Can compare integer values regardless of type's sign.

                if (k1 == intKind && k2 == uintKind) 
                    truth = arg1.Int() < 0L || uint64(arg1.Int()) < arg2.Uint();
                else if (k1 == uintKind && k2 == intKind) 
                    truth = arg2.Int() >= 0L && arg1.Uint() < uint64(arg2.Int());
                else 
                    return (false, error.As(errBadComparison)!);
                
            }
            else
            {

                if (k1 == boolKind || k1 == complexKind) 
                    return (false, error.As(errBadComparisonType)!);
                else if (k1 == floatKind) 
                    truth = arg1.Float() < arg2.Float();
                else if (k1 == intKind) 
                    truth = arg1.Int() < arg2.Int();
                else if (k1 == stringKind) 
                    truth = arg1.String() < arg2.String();
                else if (k1 == uintKind) 
                    truth = arg1.Uint() < arg2.Uint();
                else 
                    panic("invalid kind");
                
            }

            return (truth, error.As(null!)!);

        });

        // le evaluates the comparison <= b.
        private static (bool, error) le(reflect.Value arg1, reflect.Value arg2)
        {
            bool _p0 = default;
            error _p0 = default!;
 
            // <= is < or ==.
            var (lessThan, err) = lt(arg1, arg2);
            if (lessThan || err != null)
            {
                return (lessThan, error.As(err)!);
            }

            return eq(arg1, arg2);

        }

        // gt evaluates the comparison a > b.
        private static (bool, error) gt(reflect.Value arg1, reflect.Value arg2)
        {
            bool _p0 = default;
            error _p0 = default!;
 
            // > is the inverse of <=.
            var (lessOrEqual, err) = le(arg1, arg2);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            return (!lessOrEqual, error.As(null!)!);

        }

        // ge evaluates the comparison a >= b.
        private static (bool, error) ge(reflect.Value arg1, reflect.Value arg2)
        {
            bool _p0 = default;
            error _p0 = default!;
 
            // >= is the inverse of <.
            var (lessThan, err) = lt(arg1, arg2);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            return (!lessThan, error.As(null!)!);

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

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            HTMLEscape(_addr_b, (slice<byte>)s);
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

        private static slice<byte> jsLowUni = (slice<byte>)"\\u00";        private static slice<byte> hex = (slice<byte>)"0123456789ABCDEF";        private static slice<byte> jsBackslash = (slice<byte>)"\\\\";        private static slice<byte> jsApos = (slice<byte>)"\\\'";        private static slice<byte> jsQuot = (slice<byte>)"\\\"";        private static slice<byte> jsLt = (slice<byte>)"\\u003C";        private static slice<byte> jsGt = (slice<byte>)"\\u003E";        private static slice<byte> jsAmp = (slice<byte>)"\\u0026";        private static slice<byte> jsEq = (slice<byte>)"\\u003D";

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
                        case '&': 
                            w.Write(jsAmp);
                            break;
                        case '=': 
                            w.Write(jsEq);
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

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            JSEscape(_addr_b, (slice<byte>)s);
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

                case '&': 

                case '=': 
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
