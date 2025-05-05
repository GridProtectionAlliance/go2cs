// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package flag implements command-line flag parsing.

# Usage

Define flags using [flag.String], [Bool], [Int], etc.

This declares an integer flag, -n, stored in the pointer nFlag, with type *int:

	import "flag"
	var nFlag = flag.Int("n", 1234, "help message for flag n")

If you like, you can bind the flag to a variable using the Var() functions.

	var flagvar int
	func init() {
		flag.IntVar(&flagvar, "flagname", 1234, "help message for flagname")
	}

Or you can create custom flags that satisfy the Value interface (with
pointer receivers) and couple them to flag parsing by

	flag.Var(&flagVal, "name", "help message for flagname")

For such flags, the default value is just the initial value of the variable.

After all flags are defined, call

	flag.Parse()

to parse the command line into the defined flags.

Flags may then be used directly. If you're using the flags themselves,
they are all pointers; if you bind to variables, they're values.

	fmt.Println("ip has value ", *ip)
	fmt.Println("flagvar has value ", flagvar)

After parsing, the arguments following the flags are available as the
slice [flag.Args] or individually as [flag.Arg](i).
The arguments are indexed from 0 through [flag.NArg]-1.

# Command line flag syntax

The following forms are permitted:

	-flag
	--flag   // double dashes are also permitted
	-flag=x
	-flag x  // non-boolean flags only

One or two dashes may be used; they are equivalent.
The last form is not permitted for boolean flags because the
meaning of the command

	cmd -x *

where * is a Unix shell wildcard, will change if there is a file
called 0, false, etc. You must use the -flag=false form to turn
off a boolean flag.

Flag parsing stops just before the first non-flag argument
("-" is a non-flag argument) or after the terminator "--".

Integer flags accept 1234, 0664, 0x1234 and may be negative.
Boolean flags may be:

	1, 0, t, f, T, F, true, false, TRUE, FALSE, True, False

Duration flags accept any input valid for time.ParseDuration.

The default set of command-line flags is controlled by
top-level functions.  The [FlagSet] type allows one to define
independent sets of flags, such as to implement subcommands
in a command-line interface. The methods of [FlagSet] are
analogous to the top-level functions for the command-line
flag set.
*/
namespace go;

using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using ꓸꓸꓸany = Span<any>;

partial class flag_package {

// ErrHelp is the error returned if the -help or -h flag is invoked
// but no such flag is defined.
public static error ErrHelp = errors.New("flag: help requested"u8);

// errParse is returned by Set if a flag's value fails to parse, such as with an invalid integer for Int.
// It then gets wrapped through failf to provide more information.
internal static error errParse = errors.New("parse error"u8);

// errRange is returned by Set if a flag's value is out of range.
// It then gets wrapped through failf to provide more information.
internal static error errRange = errors.New("value out of range"u8);

internal static error numError(error err) {
    var (ne, ok) = err._<ж<strconv.NumError>>(ᐧ);
    if (!ok) {
        return err;
    }
    if (AreEqual((~ne).Err, strconv.ErrSyntax)) {
        return errParse;
    }
    if (AreEqual((~ne).Err, strconv.ErrRange)) {
        return errRange;
    }
    return err;
}

[GoType("bool")] partial struct boolValue;

internal static ж<boolValue> newBoolValue(bool val, ж<bool> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<boolValue>)p);
}

[GoRecv] internal static error Set(this ref boolValue b, @string s) {
    var (v, err) = strconv.ParseBool(s);
    if (err != default!) {
        err = errParse;
    }
    b = ((boolValue)v);
    return err;
}

[GoRecv] internal static any Get(this ref boolValue b) {
    return ((bool)(b));
}

[GoRecv] internal static @string String(this ref boolValue b) {
    return strconv.FormatBool(((bool)(b)));
}

[GoRecv] internal static bool IsBoolFlag(this ref boolValue b) {
    return true;
}

// optional interface to indicate boolean flags that can be
// supplied without "=value" text
[GoType] partial interface boolFlag :
    Value
{
    bool IsBoolFlag();
}

[GoType("num:nint")] partial struct intValue;

internal static ж<intValue> newIntValue(nint val, ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<intValue>)p);
}

[GoRecv] internal static error Set(this ref intValue i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, strconv.IntSize);
    if (err != default!) {
        err = numError(err);
    }
    i = ((intValue)v);
    return err;
}

[GoRecv] internal static any Get(this ref intValue i) {
    return ((nint)(i));
}

[GoRecv] internal static @string String(this ref intValue i) {
    return strconv.Itoa(((nint)(i)));
}

[GoType("num:int64")] partial struct int64Value;

internal static ж<int64Value> newInt64Value(int64 val, ж<int64> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<int64Value>)p);
}

[GoRecv] internal static error Set(this ref int64Value i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 64);
    if (err != default!) {
        err = numError(err);
    }
    i = ((int64Value)v);
    return err;
}

[GoRecv] internal static any Get(this ref int64Value i) {
    return ((int64)(i));
}

[GoRecv] internal static @string String(this ref int64Value i) {
    return strconv.FormatInt(((int64)(i)), 10);
}

[GoType("num:nuint")] partial struct uintValue;

internal static ж<uintValue> newUintValue(nuint val, ж<nuint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<uintValue>)p);
}

[GoRecv] internal static error Set(this ref uintValue i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, strconv.IntSize);
    if (err != default!) {
        err = numError(err);
    }
    i = ((uintValue)v);
    return err;
}

[GoRecv] internal static any Get(this ref uintValue i) {
    return ((nuint)(i));
}

[GoRecv] internal static @string String(this ref uintValue i) {
    return strconv.FormatUint(((uint64)(i)), 10);
}

[GoType("num:uint64")] partial struct uint64Value;

internal static ж<uint64Value> newUint64Value(uint64 val, ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<uint64Value>)p);
}

[GoRecv] internal static error Set(this ref uint64Value i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 64);
    if (err != default!) {
        err = numError(err);
    }
    i = ((uint64Value)v);
    return err;
}

[GoRecv] internal static any Get(this ref uint64Value i) {
    return ((uint64)(i));
}

[GoRecv] internal static @string String(this ref uint64Value i) {
    return strconv.FormatUint(((uint64)(i)), 10);
}

[GoType("@string")] partial struct stringValue;

internal static ж<stringValue> newStringValue(@string val, ж<@string> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<stringValue>)p);
}

[GoRecv] internal static error Set(this ref stringValue s, @string val) {
    s = ((stringValue)val);
    return default!;
}

[GoRecv] internal static any Get(this ref stringValue s) {
    return ((@string)(s));
}

[GoRecv] internal static @string String(this ref stringValue s) {
    return ((@string)(s));
}

[GoType("num:float64")] partial struct float64Value;

internal static ж<float64Value> newFloat64Value(float64 val, ж<float64> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<float64Value>)p);
}

[GoRecv] internal static error Set(this ref float64Value f, @string s) {
    var (v, err) = strconv.ParseFloat(s, 64);
    if (err != default!) {
        err = numError(err);
    }
    f = ((float64Value)v);
    return err;
}

[GoRecv] internal static any Get(this ref float64Value f) {
    return ((float64)(f));
}

[GoRecv] internal static @string String(this ref float64Value f) {
    return strconv.FormatFloat(((float64)(f)), (rune)'g', -1, 64);
}
time.Duration
internal static ж<durationValue> newDurationValue(time.Duration val, ж<time.Duration> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = val;
    return ((ж<durationValue>)(p?.val ?? default!));
}

[GoRecv] internal static error Set(this ref durationValue d, @string s) {
    var (v, err) = time.ParseDuration(s);
    if (err != default!) {
        err = errParse;
    }
    d = ((durationValue)v);
    return err;
}

[GoRecv] internal static any Get(this ref durationValue d) {
    return ((time.Duration)(d));
}

[GoRecv] internal static @string String(this ref durationValue d) {
    return ((ж<time.Duration>)(d)).val.String();
}

// -- encoding.TextUnmarshaler Value
[GoType] partial struct textValue {
    internal encoding_package.TextUnmarshaler p;
}

internal static textValue newTextValue(encoding.TextMarshaler val, encoding.TextUnmarshaler p) {
    var ptrVal = reflect.ValueOf(p);
    if (ptrVal.Kind() != reflect.Ptr) {
        throw panic("variable value type must be a pointer");
    }
    var defVal = reflect.ValueOf(val);
    if (defVal.Kind() == reflect.Ptr) {
        defVal = defVal.Elem();
    }
    if (!AreEqual(defVal.Type(), ptrVal.Type().Elem())) {
        throw panic(fmt.Sprintf("default type does not match variable type: %v != %v"u8, defVal.Type(), ptrVal.Type().Elem()));
    }
    ptrVal.Elem().Set(defVal);
    return new textValue(p);
}

internal static error Set(this textValue v, @string s) {
    return v.p.UnmarshalText(slice<byte>(s));
}

internal static any Get(this textValue v) {
    return v.p;
}

internal static @string String(this textValue v) {
    {
        var (m, ok) = v.p._<encoding.TextMarshaler>(ᐧ); if (ok) {
            {
                (b, err) = m.MarshalText(); if (err == default!) {
                    return ((@string)b);
                }
            }
        }
    }
    return ""u8;
}

internal delegate error funcValue(@string _);

internal static error Set(this funcValue f, @string s) {
    return f(s);
}

internal static @string String(this funcValue f) {
    return ""u8;
}

internal delegate error boolFuncValue(@string _);

internal static error Set(this boolFuncValue f, @string s) {
    return f(s);
}

internal static @string String(this boolFuncValue f) {
    return ""u8;
}

internal static bool IsBoolFlag(this boolFuncValue f) {
    return true;
}

// Value is the interface to the dynamic value stored in a flag.
// (The default value is represented as a string.)
//
// If a Value has an IsBoolFlag() bool method returning true,
// the command-line parser makes -name equivalent to -name=true
// rather than using the next command-line argument.
//
// Set is called once, in command line order, for each flag present.
// The flag package may call the [String] method with a zero-valued receiver,
// such as a nil pointer.
[GoType] partial interface Value {
    @string String();
    error Set(@string _);
}

// Getter is an interface that allows the contents of a [Value] to be retrieved.
// It wraps the [Value] interface, rather than being part of it, because it
// appeared after Go 1 and its compatibility rules. All [Value] types provided
// by this package satisfy the [Getter] interface, except the type used by [Func].
[GoType] partial interface Getter :
    Value
{
    any Get();
}

[GoType("num:nint")] partial struct ΔErrorHandling;

// These constants cause [FlagSet.Parse] to behave as described if the parse fails.
public static readonly ΔErrorHandling ContinueOnError = /* iota */ 0; // Return a descriptive error.

public static readonly ΔErrorHandling ExitOnError = 1;    // Call os.Exit(2) or for -h/-help Exit(0).

public static readonly ΔErrorHandling PanicOnError = 2;   // Call panic with a descriptive error.

// A FlagSet represents a set of defined flags. The zero value of a FlagSet
// has no name and has [ContinueOnError] error handling.
//
// [Flag] names must be unique within a FlagSet. An attempt to define a flag whose
// name is already in use will cause a panic.
[GoType] partial struct FlagSet {
    // Usage is the function called when an error occurs while parsing flags.
    // The field is a function (not a method) that may be changed to point to
    // a custom error handler. What happens after Usage is called depends
    // on the ErrorHandling setting; for the command line, this defaults
    // to ExitOnError, which exits the program after calling Usage.
    public Action Usage;
    internal @string name;
    internal bool parsed;
    internal map<@string, ж<Flag>> actual;
    internal map<@string, ж<Flag>> formal;
    internal slice<@string> args; // arguments after flags
    internal ΔErrorHandling errorHandling;
    internal io_package.Writer output;         // nil means stderr; use Output() accessor
    internal map<@string, @string> undef; // flags which didn't exist at the time of Set
}

// A Flag represents the state of a flag.
[GoType] partial struct Flag {
    public @string Name; // name as it appears on command line
    public @string Usage; // help message
    public Value Value;  // value as set
    public @string DefValue; // default value (as text); for usage message
}

// sortFlags returns the flags as a slice in lexicographical sorted order.
internal static slice<ж<Flag>> sortFlags(map<@string, ж<Flag>> flags) {
    var result = new slice<ж<Flag>>(len(flags));
    nint i = 0;
    foreach (var (_, f) in flags) {
        result[i] = f;
        i++;
    }
    slices.SortFunc(result, (ж<Flag> a, ж<Flag> b) => strings.Compare((~a).Name, (~b).Name));
    return result;
}

// Output returns the destination for usage and error messages. [os.Stderr] is returned if
// output was not set or was set to nil.
[GoRecv] public static io.Writer Output(this ref FlagSet f) {
    if (f.output == default!) {
        return ~os.Stderr;
    }
    return f.output;
}

// Name returns the name of the flag set.
[GoRecv] public static @string Name(this ref FlagSet f) {
    return f.name;
}

// ErrorHandling returns the error handling behavior of the flag set.
[GoRecv] public static ΔErrorHandling ErrorHandling(this ref FlagSet f) {
    return f.errorHandling;
}

// SetOutput sets the destination for usage and error messages.
// If output is nil, [os.Stderr] is used.
[GoRecv] public static void SetOutput(this ref FlagSet f, io.Writer output) {
    f.output = output;
}

// VisitAll visits the flags in lexicographical order, calling fn for each.
// It visits all flags, even those not set.
[GoRecv] public static void VisitAll(this ref FlagSet f, Action<ж<Flag>> fn) {
    foreach (var (_, flag) in sortFlags(f.formal)) {
        fn(flag);
    }
}

// VisitAll visits the command-line flags in lexicographical order, calling
// fn for each. It visits all flags, even those not set.
public static void VisitAll(Action<ж<Flag>> fn) {
    CommandLine.VisitAll(fn);
}

// Visit visits the flags in lexicographical order, calling fn for each.
// It visits only those flags that have been set.
[GoRecv] public static void Visit(this ref FlagSet f, Action<ж<Flag>> fn) {
    foreach (var (_, flag) in sortFlags(f.actual)) {
        fn(flag);
    }
}

// Visit visits the command-line flags in lexicographical order, calling fn
// for each. It visits only those flags that have been set.
public static void Visit(Action<ж<Flag>> fn) {
    CommandLine.Visit(fn);
}

// Lookup returns the [Flag] structure of the named flag, returning nil if none exists.
[GoRecv] public static ж<Flag> Lookup(this ref FlagSet f, @string name) {
    return f.formal[name];
}

// Lookup returns the [Flag] structure of the named command-line flag,
// returning nil if none exists.
public static ж<Flag> Lookup(@string name) {
    return (~CommandLine).formal[name];
}

// Set sets the value of the named flag.
[GoRecv] public static error Set(this ref FlagSet f, @string name, @string value) {
    return f.set(name, value);
}

[GoRecv] internal static error set(this ref FlagSet f, @string name, @string value) {
    var flag = f.formal[name];
    var ok = f.formal[name];
    if (!ok) {
        // Remember that a flag that isn't defined is being set.
        // We return an error in this case, but in addition if
        // subsequently that flag is defined, we want to panic
        // at the definition point.
        // This is a problem which occurs if both the definition
        // and the Set call are in init code and for whatever
        // reason the init code changes evaluation order.
        // See issue 57411.
        var (_, file, line, okΔ1) = runtime.Caller(2);
        if (!okΔ1) {
            file = "?"u8;
            line = 0;
        }
        if (f.undef == default!) {
            f.undef = new map<@string, @string>{};
        }
        f.undef[name] = fmt.Sprintf("%s:%d"u8, file, line);
        return fmt.Errorf("no such flag -%v"u8, name);
    }
    var err = (~flag).Value.Set(value);
    if (err != default!) {
        return err;
    }
    if (f.actual == default!) {
        f.actual = new map<@string, ж<Flag>>();
    }
    f.actual[name] = flag;
    return default!;
}

// Set sets the value of the named command-line flag.
public static error Set(@string name, @string value) {
    return CommandLine.set(name, value);
}

// isZeroValue determines whether the string represents the zero
// value for a flag.
internal static (bool ok, error err) isZeroValue(ж<Flag> Ꮡflag, @string value) => func((defer, recover) => {
    bool ok = default!;
    error err = default!;

    ref var flag = ref Ꮡflag.val;
    // Build a zero value of the flag's Value type, and see if the
    // result of calling its String method equals the value passed in.
    // This works unless the Value type is itself an interface type.
    var typ = reflect.TypeOf(flag.Value);
    reflectꓸValue z = default!;
    if (typ.Kind() == reflect.ΔPointer){
        z = reflect.New(typ.Elem());
    } else {
        z = reflect.Zero(typ);
    }
    // Catch panics calling the String method, which shouldn't prevent the
    // usage message from being printed, but that we should report to the
    // user so that they know to fix their code.
    var typʗ1 = typ;
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                if (typʗ1.Kind() == reflect.ΔPointer) {
                    typʗ1 = typʗ1.Elem();
                }
                err = fmt.Errorf("panic calling String method on zero %v for flag %s: %v"u8, typʗ1, flag.Name, e);
            }
        }
    });
    return (value == z.Interface()._<Value>().String(), default!);
});

// UnquoteUsage extracts a back-quoted name from the usage
// string for a flag and returns it and the un-quoted usage.
// Given "a `name` to show" it returns ("name", "a name to show").
// If there are no back quotes, the name is an educated guess of the
// type of the flag's value, or the empty string if the flag is boolean.
public static (@string name, @string usage) UnquoteUsage(ж<Flag> Ꮡflag) {
    @string name = default!;
    @string usage = default!;

    ref var flag = ref Ꮡflag.val;
    // Look for a back-quoted name, but avoid the strings package.
    usage = flag.Usage;
    for (nint i = 0; i < len(usage); i++) {
        if (usage[i] == (rune)'`') {
            for (nint j = i + 1; j < len(usage); j++) {
                if (usage[j] == (rune)'`') {
                    name = usage[(int)(i + 1)..(int)(j)];
                    usage = usage[..(int)(i)] + name + usage[(int)(j + 1)..];
                    return (name, usage);
                }
            }
            break;
        }
    }
    // Only one back quote; use type name.
    // No explicit name, so use type if we can find one.
    name = "value"u8;
    switch (flag.Value.type()) {
    case boolFlag fv: {
        if (fv.IsBoolFlag()) {
            name = ""u8;
        }
        break;
    }
    case durationValue.val fv: {
        name = "duration"u8;
        break;
    }
    case float64Value.val fv: {
        name = "float"u8;
        break;
    }
    case intValue.val fv: {
        name = "int"u8;
        break;
    }
    case int64Value.val fv: {
        name = "int"u8;
        break;
    }
    case stringValue.val fv: {
        name = "string"u8;
        break;
    }
    case uintValue.val fv: {
        name = "uint"u8;
        break;
    }
    case uint64Value.val fv: {
        name = "uint"u8;
        break;
    }}
    return (name, usage);
}

// PrintDefaults prints, to standard error unless configured otherwise, the
// default values of all defined command-line flags in the set. See the
// documentation for the global function PrintDefaults for more information.
[GoRecv] public static void PrintDefaults(this ref FlagSet f) {
    slice<error> isZeroValueErrs = default!;
    f.VisitAll(
    var isZeroValueErrsʗ2 = isZeroValueErrs;
    (ж<Flag> flag) => {
        ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
        fmt.Fprintf(~Ꮡb, "  -%s"u8, (~flag).Name);
        var (name, usage) = UnquoteUsage(flag);
        if (len(name) > 0) {
            b.WriteString(" "u8);
            b.WriteString(name);
        }
        if (b.Len() <= 4){
            b.WriteString("\t"u8);
        } else {
            b.WriteString("\n    \t"u8);
        }
        b.WriteString(strings.ReplaceAll(usage, "\n"u8, "\n    \t"u8));
        {
            var (isZero, err) = isZeroValue(flag, (~flag).DefValue); if (err != default!){
                isZeroValueErrsʗ2 = append(isZeroValueErrsʗ2, err);
            } else 
            if (!isZero) {
                {
                    var (_, ok) = (~flag).Value._<stringValue.val>(ᐧ); if (ok){
                        fmt.Fprintf(~Ꮡb, " (default %q)"u8, (~flag).DefValue);
                    } else {
                        fmt.Fprintf(~Ꮡb, " (default %v)"u8, (~flag).DefValue);
                    }
                }
            }
        }
        fmt.Fprint(f.Output(), b.String(), "\n");
    });
    // If calling String on any zero flag.Values triggered a panic, print
    // the messages after the full set of defaults so that the programmer
    // knows to fix the panic.
    {
        var errs = isZeroValueErrs; if (len(errs) > 0) {
            fmt.Fprintln(f.Output());
            foreach (var (_, err) in errs) {
                fmt.Fprintln(f.Output(), err);
            }
        }
    }
}

// PrintDefaults prints, to standard error unless configured otherwise,
// a usage message showing the default settings of all defined
// command-line flags.
// For an integer valued flag x, the default output has the form
//
//	-x int
//		usage-message-for-x (default 7)
//
// The usage message will appear on a separate line for anything but
// a bool flag with a one-byte name. For bool flags, the type is
// omitted and if the flag name is one byte the usage message appears
// on the same line. The parenthetical default is omitted if the
// default is the zero value for the type. The listed type, here int,
// can be changed by placing a back-quoted name in the flag's usage
// string; the first such item in the message is taken to be a parameter
// name to show in the message and the back quotes are stripped from
// the message when displayed. For instance, given
//
//	flag.String("I", "", "search `directory` for include files")
//
// the output will be
//
//	-I directory
//		search directory for include files.
//
// To change the destination for flag messages, call [CommandLine].SetOutput.
public static void PrintDefaults() {
    CommandLine.PrintDefaults();
}

// defaultUsage is the default function to print a usage message.
[GoRecv] internal static void defaultUsage(this ref FlagSet f) {
    if (f.name == ""u8){
        fmt.Fprintf(f.Output(), "Usage:\n"u8);
    } else {
        fmt.Fprintf(f.Output(), "Usage of %s:\n"u8, f.name);
    }
    f.PrintDefaults();
}

// NOTE: Usage is not just defaultUsage(CommandLine)
// because it serves (via godoc flag Usage) as the example
// for how to write your own usage function.

// Usage prints a usage message documenting all defined command-line flags
// to [CommandLine]'s output, which by default is [os.Stderr].
// It is called when an error occurs while parsing flags.
// The function is a variable that may be changed to point to a custom function.
// By default it prints a simple header and calls [PrintDefaults]; for details about the
// format of the output and how to control it, see the documentation for [PrintDefaults].
// Custom usage functions may choose to exit the program; by default exiting
// happens anyway as the command line's error handling strategy is set to
// [ExitOnError].
public static Action Usage = () => {
    fmt.Fprintf(CommandLine.Output(), "Usage of %s:\n"u8, os.Args[0]);
    PrintDefaults();
};

// NFlag returns the number of flags that have been set.
[GoRecv] public static nint NFlag(this ref FlagSet f) {
    return len(f.actual);
}

// NFlag returns the number of command-line flags that have been set.
public static nint NFlag() {
    return len((~CommandLine).actual);
}

// Arg returns the i'th argument. Arg(0) is the first remaining argument
// after flags have been processed. Arg returns an empty string if the
// requested element does not exist.
[GoRecv] public static @string Arg(this ref FlagSet f, nint i) {
    if (i < 0 || i >= len(f.args)) {
        return ""u8;
    }
    return f.args[i];
}

// Arg returns the i'th command-line argument. Arg(0) is the first remaining argument
// after flags have been processed. Arg returns an empty string if the
// requested element does not exist.
public static @string Arg(nint i) {
    return CommandLine.Arg(i);
}

// NArg is the number of arguments remaining after flags have been processed.
[GoRecv] public static nint NArg(this ref FlagSet f) {
    return len(f.args);
}

// NArg is the number of arguments remaining after flags have been processed.
public static nint NArg() {
    return len((~CommandLine).args);
}

// Args returns the non-flag arguments.
[GoRecv] public static slice<@string> Args(this ref FlagSet f) {
    return f.args;
}

// Args returns the non-flag command-line arguments.
public static slice<@string> Args() {
    return (~CommandLine).args;
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
[GoRecv] public static void BoolVar(this ref FlagSet f, ж<bool> Ꮡp, @string name, bool value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newBoolValue(value, Ꮡp), name, usage);
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
public static void BoolVar(ж<bool> Ꮡp, @string name, bool value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newBoolValue(value, Ꮡp), name, usage);
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
[GoRecv] public static ж<bool> Bool(this ref FlagSet f, @string name, bool value, @string usage) {
    var p = @new<bool>();
    f.BoolVar(p, name, value, usage);
    return p;
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
public static ж<bool> Bool(@string name, bool value, @string usage) {
    return CommandLine.Bool(name, value, usage);
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
[GoRecv] public static void IntVar(this ref FlagSet f, ж<nint> Ꮡp, @string name, nint value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newIntValue(value, Ꮡp), name, usage);
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
public static void IntVar(ж<nint> Ꮡp, @string name, nint value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newIntValue(value, Ꮡp), name, usage);
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
[GoRecv] public static ж<nint> Int(this ref FlagSet f, @string name, nint value, @string usage) {
    var p = @new<nint>();
    f.IntVar(p, name, value, usage);
    return p;
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
public static ж<nint> Int(@string name, nint value, @string usage) {
    return CommandLine.Int(name, value, usage);
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
[GoRecv] public static void Int64Var(this ref FlagSet f, ж<int64> Ꮡp, @string name, int64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newInt64Value(value, Ꮡp), name, usage);
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
public static void Int64Var(ж<int64> Ꮡp, @string name, int64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newInt64Value(value, Ꮡp), name, usage);
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
[GoRecv] public static ж<int64> Int64(this ref FlagSet f, @string name, int64 value, @string usage) {
    var p = @new<int64>();
    f.Int64Var(p, name, value, usage);
    return p;
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
public static ж<int64> Int64(@string name, int64 value, @string usage) {
    return CommandLine.Int64(name, value, usage);
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
[GoRecv] public static void UintVar(this ref FlagSet f, ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newUintValue(value, Ꮡp), name, usage);
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
public static void UintVar(ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newUintValue(value, Ꮡp), name, usage);
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
[GoRecv] public static ж<nuint> Uint(this ref FlagSet f, @string name, nuint value, @string usage) {
    var p = @new<nuint>();
    f.UintVar(p, name, value, usage);
    return p;
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
public static ж<nuint> Uint(@string name, nuint value, @string usage) {
    return CommandLine.Uint(name, value, usage);
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
[GoRecv] public static void Uint64Var(this ref FlagSet f, ж<uint64> Ꮡp, @string name, uint64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newUint64Value(value, Ꮡp), name, usage);
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
public static void Uint64Var(ж<uint64> Ꮡp, @string name, uint64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newUint64Value(value, Ꮡp), name, usage);
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
[GoRecv] public static ж<uint64> Uint64(this ref FlagSet f, @string name, uint64 value, @string usage) {
    var p = @new<uint64>();
    f.Uint64Var(p, name, value, usage);
    return p;
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
public static ж<uint64> Uint64(@string name, uint64 value, @string usage) {
    return CommandLine.Uint64(name, value, usage);
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
[GoRecv] public static void StringVar(this ref FlagSet f, ж<@string> Ꮡp, @string name, @string value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newStringValue(value, Ꮡp), name, usage);
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
public static void StringVar(ж<@string> Ꮡp, @string name, @string value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newStringValue(value, Ꮡp), name, usage);
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
[GoRecv] public static ж<@string> String(this ref FlagSet f, @string name, @string value, @string usage) {
    var p = @new<@string>();
    f.StringVar(p, name, value, usage);
    return p;
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
public static ж<@string> String(@string name, @string value, @string usage) {
    return CommandLine.String(name, value, usage);
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
[GoRecv] public static void Float64Var(this ref FlagSet f, ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newFloat64Value(value, Ꮡp), name, usage);
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
public static void Float64Var(ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newFloat64Value(value, Ꮡp), name, usage);
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
[GoRecv] public static ж<float64> Float64(this ref FlagSet f, @string name, float64 value, @string usage) {
    var p = @new<float64>();
    f.Float64Var(p, name, value, usage);
    return p;
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
public static ж<float64> Float64(@string name, float64 value, @string usage) {
    return CommandLine.Float64(name, value, usage);
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
[GoRecv] public static void DurationVar(this ref FlagSet f, ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    ref var p = ref Ꮡp.val;

    f.Var(~newDurationValue(value, Ꮡp), name, usage);
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static void DurationVar(ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    ref var p = ref Ꮡp.val;

    CommandLine.Var(~newDurationValue(value, Ꮡp), name, usage);
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
[GoRecv] public static ж<time.Duration> Duration(this ref FlagSet f, @string name, time.Duration value, @string usage) {
    var p = @new<time.Duration>();
    f.DurationVar(p, name, value, usage);
    return p;
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static ж<time.Duration> Duration(@string name, time.Duration value, @string usage) {
    return CommandLine.Duration(name, value, usage);
}

// TextVar defines a flag with a specified name, default value, and usage string.
// The argument p must be a pointer to a variable that will hold the value
// of the flag, and p must implement encoding.TextUnmarshaler.
// If the flag is used, the flag value will be passed to p's UnmarshalText method.
// The type of the default value must be the same as the type of p.
[GoRecv] public static void TextVar(this ref FlagSet f, encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    f.Var(newTextValue(value, p), name, usage);
}

// TextVar defines a flag with a specified name, default value, and usage string.
// The argument p must be a pointer to a variable that will hold the value
// of the flag, and p must implement encoding.TextUnmarshaler.
// If the flag is used, the flag value will be passed to p's UnmarshalText method.
// The type of the default value must be the same as the type of p.
public static void TextVar(encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    CommandLine.Var(newTextValue(value, p), name, usage);
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
[GoRecv] public static void Func(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) {
    f.Var(((funcValue)fn), name, usage);
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void Func(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.Func(name, usage, fn);
}

// BoolFunc defines a flag with the specified name and usage string without requiring values.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
[GoRecv] public static void BoolFunc(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) {
    f.Var(((boolFuncValue)fn), name, usage);
}

// BoolFunc defines a flag with the specified name and usage string without requiring values.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void BoolFunc(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.BoolFunc(name, usage, fn);
}

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type [Value], which
// typically holds a user-defined implementation of [Value]. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of [Value]; in particular, [Set] would
// decompose the comma-separated string into the slice.
[GoRecv] public static void Var(this ref FlagSet f, Value value, @string name, @string usage) {
    // Flag must not begin "-" or contain "=".
    if (strings.HasPrefix(name, "-"u8)){
        throw panic(f.sprintf("flag %q begins with -"u8, name));
    } else 
    if (strings.Contains(name, "="u8)) {
        throw panic(f.sprintf("flag %q contains ="u8, name));
    }
    // Remember the default value as a string; it won't change.
    var flag = Ꮡ(new Flag(name, usage, value, value.String()));
    var _ = f.formal[name];
    var alreadythere = f.formal[name];
    if (alreadythere) {
        @string msg = default!;
        if (f.name == ""u8){
            msg = f.sprintf("flag redefined: %s"u8, name);
        } else {
            msg = f.sprintf("%s flag redefined: %s"u8, f.name, name);
        }
        throw panic(msg);
    }
    // Happens only if flags are declared with identical names
    {
        @string pos = f.undef[name]; if (pos != ""u8) {
            throw panic(fmt.Sprintf("flag %s set at %s before being defined"u8, name, pos));
        }
    }
    if (f.formal == default!) {
        f.formal = new map<@string, ж<Flag>>();
    }
    f.formal[name] = flag;
}

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type [Value], which
// typically holds a user-defined implementation of [Value]. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of [Value]; in particular, [Set] would
// decompose the comma-separated string into the slice.
public static void Var(Value value, @string name, @string usage) {
    CommandLine.Var(value, name, usage);
}

// sprintf formats the message, prints it to output, and returns it.
[GoRecv] internal static @string sprintf(this ref FlagSet f, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    @string msg = fmt.Sprintf(format, a.ꓸꓸꓸ);
    fmt.Fprintln(f.Output(), msg);
    return msg;
}

// failf prints to standard error a formatted error and usage message and
// returns the error.
[GoRecv] internal static error failf(this ref FlagSet f, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    @string msg = f.sprintf(format, a.ꓸꓸꓸ);
    f.usage();
    return errors.New(msg);
}

// usage calls the Usage method for the flag set if one is specified,
// or the appropriate default usage function otherwise.
[GoRecv] internal static void usage(this ref FlagSet f) {
    if (f.Usage == default!){
        f.defaultUsage();
    } else {
        f.Usage();
    }
}

// parseOne parses one flag. It reports whether a flag was seen.
[GoRecv] internal static (bool, error) parseOne(this ref FlagSet f) {
    if (len(f.args) == 0) {
        return (false, default!);
    }
    @string s = f.args[0];
    if (len(s) < 2 || s[0] != (rune)'-') {
        return (false, default!);
    }
    nint numMinuses = 1;
    if (s[1] == (rune)'-') {
        numMinuses++;
        if (len(s) == 2) {
            // "--" terminates the flags
            f.args = f.args[1..];
            return (false, default!);
        }
    }
    @string name = s[(int)(numMinuses)..];
    if (len(name) == 0 || name[0] == (rune)'-' || name[0] == (rune)'=') {
        return (false, f.failf("bad flag syntax: %s"u8, s));
    }
    // it's a flag. does it have an argument?
    f.args = f.args[1..];
    var hasValue = false;
    @string value = ""u8;
    for (nint i = 1; i < len(name); i++) {
        // equals cannot be first
        if (name[i] == (rune)'=') {
            value = name[(int)(i + 1)..];
            hasValue = true;
            name = name[0..(int)(i)];
            break;
        }
    }
    var flag = f.formal[name];
    var ok = f.formal[name];
    if (!ok) {
        if (name == "help"u8 || name == "h"u8) {
            // special case for nice help message.
            f.usage();
            return (false, ErrHelp);
        }
        return (false, f.failf("flag provided but not defined: -%s"u8, name));
    }
    {
        var (fv, okΔ1) = (~flag).Value._<boolFlag>(ᐧ); if (okΔ1 && fv.IsBoolFlag()){
            // special case: doesn't need an arg
            if (hasValue){
                {
                    var err = fv.Set(value); if (err != default!) {
                        return (false, f.failf("invalid boolean value %q for -%s: %v"u8, value, name, err));
                    }
                }
            } else {
                {
                    var err = fv.Set("true"u8); if (err != default!) {
                        return (false, f.failf("invalid boolean flag %s: %v"u8, name, err));
                    }
                }
            }
        } else {
            // It must have a value, which might be the next argument.
            if (!hasValue && len(f.args) > 0) {
                // value is the next arg
                hasValue = true;
                (value, f.args) = (f.args[0], f.args[1..]);
            }
            if (!hasValue) {
                return (false, f.failf("flag needs an argument: -%s"u8, name));
            }
            {
                var err = (~flag).Value.Set(value); if (err != default!) {
                    return (false, f.failf("invalid value %q for flag -%s: %v"u8, value, name, err));
                }
            }
        }
    }
    if (f.actual == default!) {
        f.actual = new map<@string, ж<Flag>>();
    }
    f.actual[name] = flag;
    return (true, default!);
}

// Parse parses flag definitions from the argument list, which should not
// include the command name. Must be called after all flags in the [FlagSet]
// are defined and before flags are accessed by the program.
// The return value will be [ErrHelp] if -help or -h were set but not defined.
[GoRecv] public static error Parse(this ref FlagSet f, slice<@string> arguments) {
    f.parsed = true;
    f.args = arguments;
    while (ᐧ) {
        var (seen, err) = f.parseOne();
        if (seen) {
            continue;
        }
        if (err == default!) {
            break;
        }
        var exprᴛ1 = f.errorHandling;
        if (exprᴛ1 == ContinueOnError) {
            return err;
        }
        if (exprᴛ1 == ExitOnError) {
            if (AreEqual(err, ErrHelp)) {
                os.Exit(0);
            }
            os.Exit(2);
        }
        else if (exprᴛ1 == PanicOnError) {
            throw panic(err);
        }

    }
    return default!;
}

// Parsed reports whether f.Parse has been called.
[GoRecv] public static bool Parsed(this ref FlagSet f) {
    return f.parsed;
}

// Parse parses the command-line flags from [os.Args][1:]. Must be called
// after all flags are defined and before flags are accessed by the program.
public static void Parse() {
    // Ignore errors; CommandLine is set for ExitOnError.
    CommandLine.Parse(os.Args[1..]);
}

// Parsed reports whether the command-line flags have been parsed.
public static bool Parsed() {
    return CommandLine.Parsed();
}

// CommandLine is the default set of command-line flags, parsed from [os.Args].
// The top-level functions such as [BoolVar], [Arg], and so on are wrappers for the
// methods of CommandLine.
public static ж<FlagSet> CommandLine = NewFlagSet(os.Args[0], ExitOnError);

[GoInit] internal static void init() {
    // Override generic FlagSet default Usage with call to global Usage.
    // Note: This is not CommandLine.Usage = Usage,
    // because we want any eventual call to use any updated value of Usage,
    // not the value it has when this line is run.
    CommandLine.val.Usage = commandLineUsage;
}

internal static void commandLineUsage() {
    Usage();
}

// NewFlagSet returns a new, empty flag set with the specified name and
// error handling property. If the name is not empty, it will be printed
// in the default usage message and in error messages.
public static ж<FlagSet> NewFlagSet(@string name, ΔErrorHandling errorHandling) {
    var f = Ꮡ(new FlagSet(
        name: name,
        errorHandling: errorHandling
    ));
    f.val.Usage = 
    var fʗ1 = f;
    () => fʗ1.defaultUsage();
    return f;
}

// Init sets the name and error handling property for a flag set.
// By default, the zero [FlagSet] uses an empty name and the
// [ContinueOnError] error handling policy.
[GoRecv] public static void Init(this ref FlagSet f, @string name, ΔErrorHandling errorHandling) {
    f.name = name;
    f.errorHandling = errorHandling;
}

} // end flag_package
