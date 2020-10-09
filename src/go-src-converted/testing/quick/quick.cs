// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package quick implements utility functions to help with black box testing.
//
// The testing/quick package is frozen and is not accepting new features.
// package quick -- go2cs converted at 2020 October 09 05:47:49 UTC
// import "testing/quick" ==> using quick = go.testing.quick_package
// Original source: C:\Go\src\testing\quick\quick.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using math = go.math_package;
using rand = go.math.rand_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace testing
{
    public static partial class quick_package
    {
        private static ptr<long> defaultMaxCountflag.Int("quickchecks", 100L, "The default number of iterations for each check");

        // A Generator can generate random values of its own type.
        public partial interface Generator
        {
            reflect.Value Generate(ptr<rand.Rand> rand, long size);
        }

        // randFloat32 generates a random float taking the full range of a float32.
        private static float randFloat32(ptr<rand.Rand> _addr_rand)
        {
            ref rand.Rand rand = ref _addr_rand.val;

            var f = rand.Float64() * math.MaxFloat32;
            if (rand.Int() & 1L == 1L)
            {
                f = -f;
            }

            return float32(f);

        }

        // randFloat64 generates a random float taking the full range of a float64.
        private static double randFloat64(ptr<rand.Rand> _addr_rand)
        {
            ref rand.Rand rand = ref _addr_rand.val;

            var f = rand.Float64() * math.MaxFloat64;
            if (rand.Int() & 1L == 1L)
            {
                f = -f;
            }

            return f;

        }

        // randInt64 returns a random int64.
        private static long randInt64(ptr<rand.Rand> _addr_rand)
        {
            ref rand.Rand rand = ref _addr_rand.val;

            return int64(rand.Uint64());
        }

        // complexSize is the maximum length of arbitrary values that contain other
        // values.
        private static readonly long complexSize = (long)50L;

        // Value returns an arbitrary value of the given type.
        // If the type implements the Generator interface, that will be used.
        // Note: To create arbitrary values for structs, all the fields must be exported.


        // Value returns an arbitrary value of the given type.
        // If the type implements the Generator interface, that will be used.
        // Note: To create arbitrary values for structs, all the fields must be exported.
        public static (reflect.Value, bool) Value(reflect.Type t, ptr<rand.Rand> _addr_rand)
        {
            reflect.Value value = default;
            bool ok = default;
            ref rand.Rand rand = ref _addr_rand.val;

            return sizedValue(t, _addr_rand, complexSize);
        }

        // sizedValue returns an arbitrary value of the given type. The size
        // hint is used for shrinking as a function of indirection level so
        // that recursive data structures will terminate.
        private static (reflect.Value, bool) sizedValue(reflect.Type t, ptr<rand.Rand> _addr_rand, long size)
        {
            reflect.Value value = default;
            bool ok = default;
            ref rand.Rand rand = ref _addr_rand.val;

            {
                Generator (m, ok) = Generator.As(reflect.Zero(t).Interface()._<Generator>())!;

                if (ok)
                {
                    return (m.Generate(rand, size), true);
                }

            }


            var v = reflect.New(t).Elem();
            {
                var concrete = t;


                if (concrete.Kind() == reflect.Bool) 
                    v.SetBool(rand.Int() & 1L == 0L);
                else if (concrete.Kind() == reflect.Float32) 
                    v.SetFloat(float64(randFloat32(_addr_rand)));
                else if (concrete.Kind() == reflect.Float64) 
                    v.SetFloat(randFloat64(_addr_rand));
                else if (concrete.Kind() == reflect.Complex64) 
                    v.SetComplex(complex(float64(randFloat32(_addr_rand)), float64(randFloat32(_addr_rand))));
                else if (concrete.Kind() == reflect.Complex128) 
                    v.SetComplex(complex(randFloat64(_addr_rand), randFloat64(_addr_rand)));
                else if (concrete.Kind() == reflect.Int16) 
                    v.SetInt(randInt64(_addr_rand));
                else if (concrete.Kind() == reflect.Int32) 
                    v.SetInt(randInt64(_addr_rand));
                else if (concrete.Kind() == reflect.Int64) 
                    v.SetInt(randInt64(_addr_rand));
                else if (concrete.Kind() == reflect.Int8) 
                    v.SetInt(randInt64(_addr_rand));
                else if (concrete.Kind() == reflect.Int) 
                    v.SetInt(randInt64(_addr_rand));
                else if (concrete.Kind() == reflect.Uint16) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Uint32) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Uint64) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Uint8) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Uint) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Uintptr) 
                    v.SetUint(uint64(randInt64(_addr_rand)));
                else if (concrete.Kind() == reflect.Map) 
                    var numElems = rand.Intn(size);
                    v.Set(reflect.MakeMap(concrete));
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < numElems; i++)
                        {
                            var (key, ok1) = sizedValue(concrete.Key(), _addr_rand, size);
                            var (value, ok2) = sizedValue(concrete.Elem(), _addr_rand, size);
                            if (!ok1 || !ok2)
                            {
                                return (new reflect.Value(), false);
                            }

                            v.SetMapIndex(key, value);

                        }


                        i = i__prev1;
                    }
                else if (concrete.Kind() == reflect.Ptr) 
                    if (rand.Intn(size) == 0L)
                    {
                        v.Set(reflect.Zero(concrete)); // Generate nil pointer.
                    }
                    else
                    {
                        var (elem, ok) = sizedValue(concrete.Elem(), _addr_rand, size);
                        if (!ok)
                        {
                            return (new reflect.Value(), false);
                        }

                        v.Set(reflect.New(concrete.Elem()));
                        v.Elem().Set(elem);

                    }

                else if (concrete.Kind() == reflect.Slice) 
                    numElems = rand.Intn(size);
                    var sizeLeft = size - numElems;
                    v.Set(reflect.MakeSlice(concrete, numElems, numElems));
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < numElems; i++)
                        {
                            (elem, ok) = sizedValue(concrete.Elem(), _addr_rand, sizeLeft);
                            if (!ok)
                            {
                                return (new reflect.Value(), false);
                            }

                            v.Index(i).Set(elem);

                        }


                        i = i__prev1;
                    }
                else if (concrete.Kind() == reflect.Array) 
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < v.Len(); i++)
                        {
                            (elem, ok) = sizedValue(concrete.Elem(), _addr_rand, size);
                            if (!ok)
                            {
                                return (new reflect.Value(), false);
                            }

                            v.Index(i).Set(elem);

                        }


                        i = i__prev1;
                    }
                else if (concrete.Kind() == reflect.String) 
                    var numChars = rand.Intn(complexSize);
                    var codePoints = make_slice<int>(numChars);
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < numChars; i++)
                        {
                            codePoints[i] = rune(rand.Intn(0x10ffffUL));
                        }


                        i = i__prev1;
                    }
                    v.SetString(string(codePoints));
                else if (concrete.Kind() == reflect.Struct) 
                    var n = v.NumField(); 
                    // Divide sizeLeft evenly among the struct fields.
                    sizeLeft = size;
                    if (n > sizeLeft)
                    {
                        sizeLeft = 1L;
                    }
                    else if (n > 0L)
                    {
                        sizeLeft /= n;
                    }

                    {
                        long i__prev1 = i;

                        for (i = 0L; i < n; i++)
                        {
                            (elem, ok) = sizedValue(concrete.Field(i).Type, _addr_rand, sizeLeft);
                            if (!ok)
                            {
                                return (new reflect.Value(), false);
                            }

                            v.Field(i).Set(elem);

                        }


                        i = i__prev1;
                    }
                else 
                    return (new reflect.Value(), false);

            }

            return (v, true);

        }

        // A Config structure contains options for running a test.
        public partial struct Config
        {
            public long MaxCount; // MaxCountScale is a non-negative scale factor applied to the
// default maximum.
// A count of zero implies the default, which is usually 100
// but can be set by the -quickchecks flag.
            public double MaxCountScale; // Rand specifies a source of random numbers.
// If nil, a default pseudo-random source will be used.
            public ptr<rand.Rand> Rand; // Values specifies a function to generate a slice of
// arbitrary reflect.Values that are congruent with the
// arguments to the function being tested.
// If nil, the top-level Value function is used to generate them.
            public Action<slice<reflect.Value>, ptr<rand.Rand>> Values;
        }

        private static Config defaultConfig = default;

        // getRand returns the *rand.Rand to use for a given Config.
        private static ptr<rand.Rand> getRand(this ptr<Config> _addr_c)
        {
            ref Config c = ref _addr_c.val;

            if (c.Rand == null)
            {
                return _addr_rand.New(rand.NewSource(time.Now().UnixNano()))!;
            }

            return _addr_c.Rand!;

        }

        // getMaxCount returns the maximum number of iterations to run for a given
        // Config.
        private static long getMaxCount(this ptr<Config> _addr_c)
        {
            long maxCount = default;
            ref Config c = ref _addr_c.val;

            maxCount = c.MaxCount;
            if (maxCount == 0L)
            {
                if (c.MaxCountScale != 0L)
                {
                    maxCount = int(c.MaxCountScale * float64(defaultMaxCount.val));
                }
                else
                {
                    maxCount = defaultMaxCount.val;
                }

            }

            return ;

        }

        // A SetupError is the result of an error in the way that check is being
        // used, independent of the functions being tested.
        public partial struct SetupError // : @string
        {
        }

        public static @string Error(this SetupError s)
        {
            return string(s);
        }

        // A CheckError is the result of Check finding an error.
        public partial struct CheckError
        {
            public long Count;
            public slice<object> In;
        }

        private static @string Error(this ptr<CheckError> _addr_s)
        {
            ref CheckError s = ref _addr_s.val;

            return fmt.Sprintf("#%d: failed on input %s", s.Count, toString(s.In));
        }

        // A CheckEqualError is the result CheckEqual finding an error.
        public partial struct CheckEqualError
        {
            public ref CheckError CheckError => ref CheckError_val;
            public slice<object> Out1;
            public slice<object> Out2;
        }

        private static @string Error(this ptr<CheckEqualError> _addr_s)
        {
            ref CheckEqualError s = ref _addr_s.val;

            return fmt.Sprintf("#%d: failed on input %s. Output 1: %s. Output 2: %s", s.Count, toString(s.In), toString(s.Out1), toString(s.Out2));
        }

        // Check looks for an input to f, any function that returns bool,
        // such that f returns false. It calls f repeatedly, with arbitrary
        // values for each argument. If f returns false on a given input,
        // Check returns that input as a *CheckError.
        // For example:
        //
        //     func TestOddMultipleOfThree(t *testing.T) {
        //         f := func(x int) bool {
        //             y := OddMultipleOfThree(x)
        //             return y%2 == 1 && y%3 == 0
        //         }
        //         if err := quick.Check(f, nil); err != nil {
        //             t.Error(err)
        //         }
        //     }
        public static error Check(object f, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

            if (config == null)
            {
                config = _addr_defaultConfig;
            }

            var (fVal, fType, ok) = functionAndType(f);
            if (!ok)
            {
                return error.As(SetupError("argument is not a function"))!;
            }

            if (fType.NumOut() != 1L)
            {
                return error.As(SetupError("function does not return one value"))!;
            }

            if (fType.Out(0L).Kind() != reflect.Bool)
            {
                return error.As(SetupError("function does not return a bool"))!;
            }

            var arguments = make_slice<reflect.Value>(fType.NumIn());
            var rand = config.getRand();
            var maxCount = config.getMaxCount();

            for (long i = 0L; i < maxCount; i++)
            {
                var err = arbitraryValues(arguments, fType, _addr_config, _addr_rand);
                if (err != null)
                {
                    return error.As(err)!;
                }

                if (!fVal.Call(arguments)[0L].Bool())
                {
                    return error.As(addr(new CheckError(i+1,toInterfaces(arguments)))!)!;
                }

            }


            return error.As(null!)!;

        }

        // CheckEqual looks for an input on which f and g return different results.
        // It calls f and g repeatedly with arbitrary values for each argument.
        // If f and g return different answers, CheckEqual returns a *CheckEqualError
        // describing the input and the outputs.
        public static error CheckEqual(object f, object g, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

            if (config == null)
            {
                config = _addr_defaultConfig;
            }

            var (x, xType, ok) = functionAndType(f);
            if (!ok)
            {
                return error.As(SetupError("f is not a function"))!;
            }

            var (y, yType, ok) = functionAndType(g);
            if (!ok)
            {
                return error.As(SetupError("g is not a function"))!;
            }

            if (xType != yType)
            {
                return error.As(SetupError("functions have different types"))!;
            }

            var arguments = make_slice<reflect.Value>(xType.NumIn());
            var rand = config.getRand();
            var maxCount = config.getMaxCount();

            for (long i = 0L; i < maxCount; i++)
            {
                var err = arbitraryValues(arguments, xType, _addr_config, _addr_rand);
                if (err != null)
                {
                    return error.As(err)!;
                }

                var xOut = toInterfaces(x.Call(arguments));
                var yOut = toInterfaces(y.Call(arguments));

                if (!reflect.DeepEqual(xOut, yOut))
                {
                    return error.As(addr(new CheckEqualError(CheckError{i+1,toInterfaces(arguments)},xOut,yOut))!)!;
                }

            }


            return error.As(null!)!;

        }

        // arbitraryValues writes Values to args such that args contains Values
        // suitable for calling f.
        private static error arbitraryValues(slice<reflect.Value> args, reflect.Type f, ptr<Config> _addr_config, ptr<rand.Rand> _addr_rand)
        {
            error err = default!;
            ref Config config = ref _addr_config.val;
            ref rand.Rand rand = ref _addr_rand.val;

            if (config.Values != null)
            {
                config.Values(args, rand);
                return ;
            }

            for (long j = 0L; j < len(args); j++)
            {
                bool ok = default;
                args[j], ok = Value(f.In(j), _addr_rand);
                if (!ok)
                {
                    err = SetupError(fmt.Sprintf("cannot create arbitrary value of type %s for argument %d", f.In(j), j));
                    return ;
                }

            }


            return ;

        }

        private static (reflect.Value, reflect.Type, bool) functionAndType(object f)
        {
            reflect.Value v = default;
            reflect.Type t = default;
            bool ok = default;

            v = reflect.ValueOf(f);
            ok = v.Kind() == reflect.Func;
            if (!ok)
            {
                return ;
            }

            t = v.Type();
            return ;

        }

        private static slice<object> toInterfaces(slice<reflect.Value> values)
        {
            var ret = make_slice<object>(len(values));
            foreach (var (i, v) in values)
            {
                ret[i] = v.Interface();
            }
            return ret;

        }

        private static @string toString(slice<object> interfaces)
        {
            var s = make_slice<@string>(len(interfaces));
            foreach (var (i, v) in interfaces)
            {
                s[i] = fmt.Sprintf("%#v", v);
            }
            return strings.Join(s, ", ");

        }
    }
}}
