// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package quick implements utility functions to help with black box testing.
//
// The testing/quick package is frozen and is not accepting new features.
namespace go.testing;

using flag = flag_package;
using fmt = fmt_package;
using math = math_package;
using rand = math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using time = time_package;
using math;

partial class quick_package {

internal static ж<nint> defaultMaxCount = flag.Int("quickchecks"u8, 100, "The default number of iterations for each check"u8);

// A Generator can generate random values of its own type.
[GoType] partial interface Generator {
    // Generate returns a random instance of the type on which it is a
    // method using the size as a size hint.
    reflectꓸValue Generate(ж<rand.Rand> rand, nint size);
}

// randFloat32 generates a random float taking the full range of a float32.
internal static float32 randFloat32(ж<rand.Rand> Ꮡrand) {
    ref var rand = ref Ꮡrand.val;

    var f = rand.Float64() * math.MaxFloat32;
    if ((nint)(rand.Int() & 1) == 1) {
        f = -f;
    }
    return ((float32)f);
}

// randFloat64 generates a random float taking the full range of a float64.
internal static float64 randFloat64(ж<rand.Rand> Ꮡrand) {
    ref var rand = ref Ꮡrand.val;

    var f = rand.Float64() * math.MaxFloat64;
    if ((nint)(rand.Int() & 1) == 1) {
        f = -f;
    }
    return f;
}

// randInt64 returns a random int64.
internal static int64 randInt64(ж<rand.Rand> Ꮡrand) {
    ref var rand = ref Ꮡrand.val;

    return ((int64)rand.Uint64());
}

// complexSize is the maximum length of arbitrary values that contain other
// values.
internal static readonly UntypedInt complexSize = 50;

// Value returns an arbitrary value of the given type.
// If the type implements the [Generator] interface, that will be used.
// Note: To create arbitrary values for structs, all the fields must be exported.
public static (reflectꓸValue value, bool ok) Value(reflectꓸType t, ж<rand.Rand> Ꮡrand) {
    reflectꓸValue value = default!;
    bool ok = default!;

    ref var rand = ref Ꮡrand.val;
    return sizedValue(t, Ꮡrand, complexSize);
}

// sizedValue returns an arbitrary value of the given type. The size
// hint is used for shrinking as a function of indirection level so
// that recursive data structures will terminate.
internal static (reflectꓸValue value, bool ok) sizedValue(reflectꓸType t, ж<rand.Rand> Ꮡrand, nint size) {
    reflectꓸValue value = default!;
    bool ok = default!;

    ref var rand = ref Ꮡrand.val;
    {
        var (m, okΔ1) = reflect.Zero(t).Interface()._<Generator>(ᐧ); if (okΔ1) {
            return (m.Generate(Ꮡrand, size), true);
        }
    }
    var v = reflect.New(t).Elem();
    {
        var concrete = t;
        var exprᴛ1 = concrete.Kind();
        if (exprᴛ1 == reflect.ΔBool) {
            v.SetBool((nint)(rand.Int() & 1) == 0);
        }
        else if (exprᴛ1 == reflect.Float32) {
            v.SetFloat(((float64)randFloat32(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Float64) {
            v.SetFloat(randFloat64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.Complex64) {
            v.SetComplex(complex(((float64)randFloat32(Ꮡrand)), ((float64)randFloat32(Ꮡrand))));
        }
        else if (exprᴛ1 == reflect.Complex128) {
            v.SetComplex(complex(randFloat64(Ꮡrand), randFloat64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Int16) {
            v.SetInt(randInt64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.Int32) {
            v.SetInt(randInt64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.Int64) {
            v.SetInt(randInt64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.Int8) {
            v.SetInt(randInt64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.ΔInt) {
            v.SetInt(randInt64(Ꮡrand));
        }
        else if (exprᴛ1 == reflect.Uint16) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Uint32) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Uint64) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Uint8) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.ΔUint) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Uintptr) {
            v.SetUint(((uint64)randInt64(Ꮡrand)));
        }
        else if (exprᴛ1 == reflect.Map) {
            nint numElems = rand.Intn(size);
            v.Set(reflect.MakeMap(concrete));
            for (nint i = 0; i < numElems; i++) {
                var (key, ok1) = sizedValue(concrete.Key(), Ꮡrand, size);
                var (valueΔ2, ok2) = sizedValue(concrete.Elem(), Ꮡrand, size);
                if (!ok1 || !ok2) {
                    return (new reflectꓸValue(nil), false);
                }
                v.SetMapIndex(key, valueΔ2);
            }
        }
        else if (exprᴛ1 == reflect.ΔPointer) {
            if (rand.Intn(size) == 0){
                v.SetZero();
            } else {
                // Generate nil pointer.
                var (elem, okΔ6) = sizedValue(concrete.Elem(), Ꮡrand, size);
                if (!okΔ6) {
                    return (new reflectꓸValue(nil), false);
                }
                v.Set(reflect.New(concrete.Elem()));
                v.Elem().Set(elem);
            }
        }
        else if (exprᴛ1 == reflect.ΔSlice) {
            nint numElems = rand.Intn(size);
            nint sizeLeft = size - numElems;
            v.Set(reflect.MakeSlice(concrete, numElems, numElems));
            for (nint i = 0; i < numElems; i++) {
                var (elem, okΔ7) = sizedValue(concrete.Elem(), Ꮡrand, sizeLeft);
                if (!okΔ7) {
                    return (new reflectꓸValue(nil), false);
                }
                v.Index(i).Set(elem);
            }
        }
        else if (exprᴛ1 == reflect.Array) {
            for (nint i = 0; i < v.Len(); i++) {
                var (elem, okΔ8) = sizedValue(concrete.Elem(), Ꮡrand, size);
                if (!okΔ8) {
                    return (new reflectꓸValue(nil), false);
                }
                v.Index(i).Set(elem);
            }
        }
        else if (exprᴛ1 == reflect.ΔString) {
            nint numChars = rand.Intn(complexSize);
            var codePoints = new slice<rune>(numChars);
            for (nint i = 0; i < numChars; i++) {
                codePoints[i] = ((rune)rand.Intn(1114111));
            }
            v.SetString(((@string)codePoints));
        }
        else if (exprᴛ1 == reflect.Struct) {
            nint n = v.NumField();
            nint sizeLeft = size;
            if (n > sizeLeft){
                // Divide sizeLeft evenly among the struct fields.
                sizeLeft = 1;
            } else 
            if (n > 0) {
                sizeLeft /= n;
            }
            for (nint i = 0; i < n; i++) {
                var (elem, okΔ9) = sizedValue(concrete.Field(i).Type, Ꮡrand, sizeLeft);
                if (!okΔ9) {
                    return (new reflectꓸValue(nil), false);
                }
                v.Field(i).Set(elem);
            }
        }
        else { /* default: */
            return (new reflectꓸValue(nil), false);
        }
    }

    return (v, true);
}

// A Config structure contains options for running a test.
[GoType] partial struct Config {
    // MaxCount sets the maximum number of iterations.
    // If zero, MaxCountScale is used.
    public nint MaxCount;
    // MaxCountScale is a non-negative scale factor applied to the
    // default maximum.
    // A count of zero implies the default, which is usually 100
    // but can be set by the -quickchecks flag.
    public float64 MaxCountScale;
    // Rand specifies a source of random numbers.
    // If nil, a default pseudo-random source will be used.
    public ж<math.rand_package.Rand> Rand;
    // Values specifies a function to generate a slice of
    // arbitrary reflect.Values that are congruent with the
    // arguments to the function being tested.
    // If nil, the top-level Value function is used to generate them.
    public rand.Rand) Values;
}

internal static Config defaultConfig;

// getRand returns the *rand.Rand to use for a given Config.
[GoRecv] internal static ж<rand.Rand> getRand(this ref Config c) {
    if (c.Rand == nil) {
        return rand.New(rand.NewSource(time.Now().UnixNano()));
    }
    return c.Rand;
}

// getMaxCount returns the maximum number of iterations to run for a given
// Config.
[GoRecv] internal static nint /*maxCount*/ getMaxCount(this ref Config c) {
    nint maxCount = default!;

    maxCount = c.MaxCount;
    if (maxCount == 0) {
        if (c.MaxCountScale != 0){
            maxCount = ((nint)(c.MaxCountScale * ((float64)(defaultMaxCount.val))));
        } else {
            maxCount = defaultMaxCount.val;
        }
    }
    return maxCount;
}

[GoType("@string")] partial struct SetupError;

public static @string Error(this SetupError s) {
    return ((@string)s);
}

// A CheckError is the result of Check finding an error.
[GoType] partial struct CheckError {
    public nint Count;
    public slice<any> In;
}

[GoRecv] public static @string Error(this ref CheckError s) {
    return fmt.Sprintf("#%d: failed on input %s"u8, s.Count, toString(s.In));
}

// A CheckEqualError is the result [CheckEqual] finding an error.
[GoType] partial struct CheckEqualError {
    public partial ref CheckError CheckError { get; }
    public slice<any> Out1;
    public slice<any> Out2;
}

[GoRecv] public static @string Error(this ref CheckEqualError s) {
    return fmt.Sprintf("#%d: failed on input %s. Output 1: %s. Output 2: %s"u8, s.Count, toString(s.In), toString(s.Out1), toString(s.Out2));
}

// Check looks for an input to f, any function that returns bool,
// such that f returns false. It calls f repeatedly, with arbitrary
// values for each argument. If f returns false on a given input,
// Check returns that input as a *[CheckError].
// For example:
//
//	func TestOddMultipleOfThree(t *testing.T) {
//		f := func(x int) bool {
//			y := OddMultipleOfThree(x)
//			return y%2 == 1 && y%3 == 0
//		}
//		if err := quick.Check(f, nil); err != nil {
//			t.Error(err)
//		}
//	}
public static error Check(any f, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    if (config == nil) {
        Ꮡconfig = Ꮡ(defaultConfig); config = ref Ꮡconfig.val;
    }
    var (fVal, fType, ok) = functionAndType(f);
    if (!ok) {
        return ((SetupError)"argument is not a function"u8);
    }
    if (fType.NumOut() != 1) {
        return ((SetupError)"function does not return one value"u8);
    }
    if (fType.Out(0).Kind() != reflect.ΔBool) {
        return ((SetupError)"function does not return a bool"u8);
    }
    var arguments = new slice<reflectꓸValue>(fType.NumIn());
    var rand = config.getRand();
    nint maxCount = config.getMaxCount();
    for (nint i = 0; i < maxCount; i++) {
        var err = arbitraryValues(arguments, fType, Ꮡconfig, rand);
        if (err != default!) {
            return err;
        }
        if (!fVal.Call(arguments)[0].Bool()) {
            return new CheckError(i + 1, toInterfaces(arguments));
        }
    }
    return default!;
}

// CheckEqual looks for an input on which f and g return different results.
// It calls f and g repeatedly with arbitrary values for each argument.
// If f and g return different answers, CheckEqual returns a *[CheckEqualError]
// describing the input and the outputs.
public static error CheckEqual(any f, any g, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    if (config == nil) {
        Ꮡconfig = Ꮡ(defaultConfig); config = ref Ꮡconfig.val;
    }
    var (x, xType, ok) = functionAndType(f);
    if (!ok) {
        return ((SetupError)"f is not a function"u8);
    }
    var (y, yType, ok) = functionAndType(g);
    if (!ok) {
        return ((SetupError)"g is not a function"u8);
    }
    if (!AreEqual(xType, yType)) {
        return ((SetupError)"functions have different types"u8);
    }
    var arguments = new slice<reflectꓸValue>(xType.NumIn());
    var rand = config.getRand();
    nint maxCount = config.getMaxCount();
    for (nint i = 0; i < maxCount; i++) {
        var err = arbitraryValues(arguments, xType, Ꮡconfig, rand);
        if (err != default!) {
            return err;
        }
        var xOut = toInterfaces(x.Call(arguments));
        var yOut = toInterfaces(y.Call(arguments));
        if (!reflect.DeepEqual(xOut, yOut)) {
            return new CheckEqualError(new CheckError(i + 1, toInterfaces(arguments)), xOut, yOut);
        }
    }
    return default!;
}

// arbitraryValues writes Values to args such that args contains Values
// suitable for calling f.
internal static error /*err*/ arbitraryValues(slice<reflectꓸValue> args, reflectꓸType f, ж<Config> Ꮡconfig, ж<rand.Rand> Ꮡrand) {
    error err = default!;

    ref var config = ref Ꮡconfig.val;
    ref var rand = ref Ꮡrand.val;
    if (config.Values != default!) {
        config.Values(args, rand);
        return err;
    }
    for (nint j = 0; j < len(args); j++) {
        bool ok = default!;
        (args[j], ok) = Value(f.In(j), Ꮡrand);
        if (!ok) {
            err = ((SetupError)fmt.Sprintf("cannot create arbitrary value of type %s for argument %d"u8, f.In(j), j));
            return err;
        }
    }
    return err;
}

internal static (reflectꓸValue v, reflectꓸType t, bool ok) functionAndType(any f) {
    reflectꓸValue v = default!;
    reflectꓸType t = default!;
    bool ok = default!;

    v = reflect.ValueOf(f);
    ok = v.Kind() == reflect.Func;
    if (!ok) {
        return (v, t, ok);
    }
    t = v.Type();
    return (v, t, ok);
}

internal static slice<any> toInterfaces(slice<reflectꓸValue> values) {
    var ret = new slice<any>(len(values));
    foreach (var (i, v) in values) {
        ret[i] = v.Interface();
    }
    return ret;
}

internal static @string toString(slice<any> interfaces) {
    var s = new slice<@string>(len(interfaces));
    foreach (var (i, v) in interfaces) {
        s[i] = fmt.Sprintf("%#v"u8, v);
    }
    return strings.Join(s, ", "u8);
}

} // end quick_package
