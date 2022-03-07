// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
    Package flag implements command-line flag parsing.

    Usage

    Define flags using flag.String(), Bool(), Int(), etc.

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
    slice flag.Args() or individually as flag.Arg(i).
    The arguments are indexed from 0 through flag.NArg()-1.

    Command line flag syntax

    The following forms are permitted:

        -flag
        -flag=x
        -flag x  // non-boolean flags only
    One or two minus signs may be used; they are equivalent.
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
    top-level functions.  The FlagSet type allows one to define
    independent sets of flags, such as to implement subcommands
    in a command-line interface. The methods of FlagSet are
    analogous to the top-level functions for the command-line
    flag set.
*/
// package flag -- go2cs converted at 2022 March 06 22:23:59 UTC
// import "flag" ==> using flag = go.flag_package
// Original source: C:\Program Files\Go\src\flag\flag.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using System;


namespace go;

public static partial class flag_package {

    // ErrHelp is the error returned if the -help or -h flag is invoked
    // but no such flag is defined.
public static var ErrHelp = errors.New("flag: help requested");

// errParse is returned by Set if a flag's value fails to parse, such as with an invalid integer for Int.
// It then gets wrapped through failf to provide more information.
private static var errParse = errors.New("parse error");

// errRange is returned by Set if a flag's value is out of range.
// It then gets wrapped through failf to provide more information.
private static var errRange = errors.New("value out of range");

private static error numError(error err) {
    ptr<strconv.NumError> (ne, ok) = err._<ptr<strconv.NumError>>();
    if (!ok) {
        return error.As(err)!;
    }
    if (ne.Err == strconv.ErrSyntax) {
        return error.As(errParse)!;
    }
    if (ne.Err == strconv.ErrRange) {
        return error.As(errRange)!;
    }
    return error.As(err)!;

}

// -- bool Value
private partial struct boolValue { // : bool
}

private static ptr<boolValue> newBoolValue(bool val, ptr<bool> _addr_p) {
    ref bool p = ref _addr_p.val;

    p = val;
    return _addr_(boolValue.val)(p)!;
}

private static error Set(this ptr<boolValue> _addr_b, @string s) {
    ref boolValue b = ref _addr_b.val;

    var (v, err) = strconv.ParseBool(s);
    if (err != null) {
        err = errParse;
    }
    b.val = boolValue(v);
    return error.As(err)!;

}

private static void Get(this ptr<boolValue> _addr_b) {
    ref boolValue b = ref _addr_b.val;

    return bool(b.val);
}

private static @string String(this ptr<boolValue> _addr_b) {
    ref boolValue b = ref _addr_b.val;

    return strconv.FormatBool(bool(b.val));
}

private static bool IsBoolFlag(this ptr<boolValue> _addr_b) {
    ref boolValue b = ref _addr_b.val;

    return true;
}

// optional interface to indicate boolean flags that can be
// supplied without "=value" text
private partial interface boolFlag {
    bool IsBoolFlag();
}

// -- int Value
private partial struct intValue { // : nint
}

private static ptr<intValue> newIntValue(nint val, ptr<nint> _addr_p) {
    ref nint p = ref _addr_p.val;

    p = val;
    return _addr_(intValue.val)(p)!;
}

private static error Set(this ptr<intValue> _addr_i, @string s) {
    ref intValue i = ref _addr_i.val;

    var (v, err) = strconv.ParseInt(s, 0, strconv.IntSize);
    if (err != null) {
        err = numError(err);
    }
    i.val = intValue(v);
    return error.As(err)!;

}

private static void Get(this ptr<intValue> _addr_i) {
    ref intValue i = ref _addr_i.val;

    return int(i.val);
}

private static @string String(this ptr<intValue> _addr_i) {
    ref intValue i = ref _addr_i.val;

    return strconv.Itoa(int(i.val));
}

// -- int64 Value
private partial struct int64Value { // : long
}

private static ptr<int64Value> newInt64Value(long val, ptr<long> _addr_p) {
    ref long p = ref _addr_p.val;

    p = val;
    return _addr_(int64Value.val)(p)!;
}

private static error Set(this ptr<int64Value> _addr_i, @string s) {
    ref int64Value i = ref _addr_i.val;

    var (v, err) = strconv.ParseInt(s, 0, 64);
    if (err != null) {
        err = numError(err);
    }
    i.val = int64Value(v);
    return error.As(err)!;

}

private static void Get(this ptr<int64Value> _addr_i) {
    ref int64Value i = ref _addr_i.val;

    return int64(i.val);
}

private static @string String(this ptr<int64Value> _addr_i) {
    ref int64Value i = ref _addr_i.val;

    return strconv.FormatInt(int64(i.val), 10);
}

// -- uint Value
private partial struct uintValue { // : nuint
}

private static ptr<uintValue> newUintValue(nuint val, ptr<nuint> _addr_p) {
    ref nuint p = ref _addr_p.val;

    p = val;
    return _addr_(uintValue.val)(p)!;
}

private static error Set(this ptr<uintValue> _addr_i, @string s) {
    ref uintValue i = ref _addr_i.val;

    var (v, err) = strconv.ParseUint(s, 0, strconv.IntSize);
    if (err != null) {
        err = numError(err);
    }
    i.val = uintValue(v);
    return error.As(err)!;

}

private static void Get(this ptr<uintValue> _addr_i) {
    ref uintValue i = ref _addr_i.val;

    return uint(i.val);
}

private static @string String(this ptr<uintValue> _addr_i) {
    ref uintValue i = ref _addr_i.val;

    return strconv.FormatUint(uint64(i.val), 10);
}

// -- uint64 Value
private partial struct uint64Value { // : ulong
}

private static ptr<uint64Value> newUint64Value(ulong val, ptr<ulong> _addr_p) {
    ref ulong p = ref _addr_p.val;

    p = val;
    return _addr_(uint64Value.val)(p)!;
}

private static error Set(this ptr<uint64Value> _addr_i, @string s) {
    ref uint64Value i = ref _addr_i.val;

    var (v, err) = strconv.ParseUint(s, 0, 64);
    if (err != null) {
        err = numError(err);
    }
    i.val = uint64Value(v);
    return error.As(err)!;

}

private static void Get(this ptr<uint64Value> _addr_i) {
    ref uint64Value i = ref _addr_i.val;

    return uint64(i.val);
}

private static @string String(this ptr<uint64Value> _addr_i) {
    ref uint64Value i = ref _addr_i.val;

    return strconv.FormatUint(uint64(i.val), 10);
}

// -- string Value
private partial struct stringValue { // : @string
}

private static ptr<stringValue> newStringValue(@string val, ptr<@string> _addr_p) {
    ref @string p = ref _addr_p.val;

    p = val;
    return _addr_(stringValue.val)(p)!;
}

private static error Set(this ptr<stringValue> _addr_s, @string val) {
    ref stringValue s = ref _addr_s.val;

    s.val = stringValue(val);
    return error.As(null!)!;
}

private static void Get(this ptr<stringValue> _addr_s) {
    ref stringValue s = ref _addr_s.val;

    return string(s.val);
}

private static @string String(this ptr<stringValue> _addr_s) {
    ref stringValue s = ref _addr_s.val;

    return string(s.val);
}

// -- float64 Value
private partial struct float64Value { // : double
}

private static ptr<float64Value> newFloat64Value(double val, ptr<double> _addr_p) {
    ref double p = ref _addr_p.val;

    p = val;
    return _addr_(float64Value.val)(p)!;
}

private static error Set(this ptr<float64Value> _addr_f, @string s) {
    ref float64Value f = ref _addr_f.val;

    var (v, err) = strconv.ParseFloat(s, 64);
    if (err != null) {
        err = numError(err);
    }
    f.val = float64Value(v);
    return error.As(err)!;

}

private static void Get(this ptr<float64Value> _addr_f) {
    ref float64Value f = ref _addr_f.val;

    return float64(f.val);
}

private static @string String(this ptr<float64Value> _addr_f) {
    ref float64Value f = ref _addr_f.val;

    return strconv.FormatFloat(float64(f.val), 'g', -1, 64);
}

// -- time.Duration Value
private partial struct durationValue { // : time.Duration
}

private static ptr<durationValue> newDurationValue(time.Duration val, ptr<time.Duration> _addr_p) {
    ref time.Duration p = ref _addr_p.val;

    p = val;
    return _addr_(durationValue.val)(p)!;
}

private static error Set(this ptr<durationValue> _addr_d, @string s) {
    ref durationValue d = ref _addr_d.val;

    var (v, err) = time.ParseDuration(s);
    if (err != null) {
        err = errParse;
    }
    d.val = durationValue(v);
    return error.As(err)!;

}

private static void Get(this ptr<durationValue> _addr_d) {
    ref durationValue d = ref _addr_d.val;

    return time.Duration(d.val);
}

private static @string String(this ptr<durationValue> _addr_d) {
    ref durationValue d = ref _addr_d.val;

    return (time.Duration.val)(d).String();
}

public delegate  error funcValue(@string);

private static error Set(this funcValue f, @string s) {
    return error.As(f(s))!;
}

private static @string String(this funcValue f) {
    return "";
}

// Value is the interface to the dynamic value stored in a flag.
// (The default value is represented as a string.)
//
// If a Value has an IsBoolFlag() bool method returning true,
// the command-line parser makes -name equivalent to -name=true
// rather than using the next command-line argument.
//
// Set is called once, in command line order, for each flag present.
// The flag package may call the String method with a zero-valued receiver,
// such as a nil pointer.
public partial interface Value {
    error String();
    error Set(@string _p0);
}

// Getter is an interface that allows the contents of a Value to be retrieved.
// It wraps the Value interface, rather than being part of it, because it
// appeared after Go 1 and its compatibility rules. All Value types provided
// by this package satisfy the Getter interface, except the type used by Func.
public partial interface Getter {
    void Get();
}

// ErrorHandling defines how FlagSet.Parse behaves if the parse fails.
public partial struct ErrorHandling { // : nint
}

// These constants cause FlagSet.Parse to behave as described if the parse fails.
public static readonly ErrorHandling ContinueOnError = iota; // Return a descriptive error.
public static readonly var ExitOnError = 0; // Call os.Exit(2) or for -h/-help Exit(0).
public static readonly var PanicOnError = 1; // Call panic with a descriptive error.

// A FlagSet represents a set of defined flags. The zero value of a FlagSet
// has no name and has ContinueOnError error handling.
//
// Flag names must be unique within a FlagSet. An attempt to define a flag whose
// name is already in use will cause a panic.
public partial struct FlagSet {
    public Action Usage;
    public @string name;
    public bool parsed;
    public map<@string, ptr<Flag>> actual;
    public map<@string, ptr<Flag>> formal;
    public slice<@string> args; // arguments after flags
    public ErrorHandling errorHandling;
    public io.Writer output; // nil means stderr; use Output() accessor
}

// A Flag represents the state of a flag.
public partial struct Flag {
    public @string Name; // name as it appears on command line
    public @string Usage; // help message
    public Value Value; // value as set
    public @string DefValue; // default value (as text); for usage message
}

// sortFlags returns the flags as a slice in lexicographical sorted order.
private static slice<ptr<Flag>> sortFlags(map<@string, ptr<Flag>> flags) {
    var result = make_slice<ptr<Flag>>(len(flags));
    nint i = 0;
    foreach (var (_, f) in flags) {
        result[i] = f;
        i++;
    }    sort.Slice(result, (i, j) => {
        return result[i].Name < result[j].Name;
    });
    return result;
}

// Output returns the destination for usage and error messages. os.Stderr is returned if
// output was not set or was set to nil.
private static io.Writer Output(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    if (f.output == null) {
        return os.Stderr;
    }
    return f.output;

}

// Name returns the name of the flag set.
private static @string Name(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return f.name;
}

// ErrorHandling returns the error handling behavior of the flag set.
private static ErrorHandling ErrorHandling(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return f.errorHandling;
}

// SetOutput sets the destination for usage and error messages.
// If output is nil, os.Stderr is used.
private static void SetOutput(this ptr<FlagSet> _addr_f, io.Writer output) {
    ref FlagSet f = ref _addr_f.val;

    f.output = output;
}

// VisitAll visits the flags in lexicographical order, calling fn for each.
// It visits all flags, even those not set.
private static void VisitAll(this ptr<FlagSet> _addr_f, Action<ptr<Flag>> fn) {
    ref FlagSet f = ref _addr_f.val;

    foreach (var (_, flag) in sortFlags(f.formal)) {
        fn(flag);
    }
}

// VisitAll visits the command-line flags in lexicographical order, calling
// fn for each. It visits all flags, even those not set.
public static void VisitAll(Action<ptr<Flag>> fn) {
    CommandLine.VisitAll(fn);
}

// Visit visits the flags in lexicographical order, calling fn for each.
// It visits only those flags that have been set.
private static void Visit(this ptr<FlagSet> _addr_f, Action<ptr<Flag>> fn) {
    ref FlagSet f = ref _addr_f.val;

    foreach (var (_, flag) in sortFlags(f.actual)) {
        fn(flag);
    }
}

// Visit visits the command-line flags in lexicographical order, calling fn
// for each. It visits only those flags that have been set.
public static void Visit(Action<ptr<Flag>> fn) {
    CommandLine.Visit(fn);
}

// Lookup returns the Flag structure of the named flag, returning nil if none exists.
private static ptr<Flag> Lookup(this ptr<FlagSet> _addr_f, @string name) {
    ref FlagSet f = ref _addr_f.val;

    return _addr_f.formal[name]!;
}

// Lookup returns the Flag structure of the named command-line flag,
// returning nil if none exists.
public static ptr<Flag> Lookup(@string name) {
    return _addr_CommandLine.formal[name]!;
}

// Set sets the value of the named flag.
private static error Set(this ptr<FlagSet> _addr_f, @string name, @string value) {
    ref FlagSet f = ref _addr_f.val;

    var (flag, ok) = f.formal[name];
    if (!ok) {
        return error.As(fmt.Errorf("no such flag -%v", name))!;
    }
    var err = flag.Value.Set(value);
    if (err != null) {
        return error.As(err)!;
    }
    if (f.actual == null) {
        f.actual = make_map<@string, ptr<Flag>>();
    }
    f.actual[name] = flag;
    return error.As(null!)!;

}

// Set sets the value of the named command-line flag.
public static error Set(@string name, @string value) {
    return error.As(CommandLine.Set(name, value))!;
}

// isZeroValue determines whether the string represents the zero
// value for a flag.
private static bool isZeroValue(ptr<Flag> _addr_flag, @string value) {
    ref Flag flag = ref _addr_flag.val;
 
    // Build a zero value of the flag's Value type, and see if the
    // result of calling its String method equals the value passed in.
    // This works unless the Value type is itself an interface type.
    var typ = reflect.TypeOf(flag.Value);
    reflect.Value z = default;
    if (typ.Kind() == reflect.Ptr) {
        z = reflect.New(typ.Elem());
    }
    else
 {
        z = reflect.Zero(typ);
    }
    return value == z.Interface()._<Value>().String();

}

// UnquoteUsage extracts a back-quoted name from the usage
// string for a flag and returns it and the un-quoted usage.
// Given "a `name` to show" it returns ("name", "a name to show").
// If there are no back quotes, the name is an educated guess of the
// type of the flag's value, or the empty string if the flag is boolean.
public static (@string, @string) UnquoteUsage(ptr<Flag> _addr_flag) {
    @string name = default;
    @string usage = default;
    ref Flag flag = ref _addr_flag.val;
 
    // Look for a back-quoted name, but avoid the strings package.
    usage = flag.Usage;
    for (nint i = 0; i < len(usage); i++) {
        if (usage[i] == '`') {
            for (var j = i + 1; j < len(usage); j++) {
                if (usage[j] == '`') {
                    name = usage[(int)i + 1..(int)j];
                    usage = usage[..(int)i] + name + usage[(int)j + 1..];
                    return (name, usage);
                }
            }

            break; // Only one back quote; use type name.
        }
    } 
    // No explicit name, so use type if we can find one.
    name = "value";
    switch (flag.Value.type()) {
        case boolFlag _:
            name = "";
            break;
        case ptr<durationValue> _:
            name = "duration";
            break;
        case ptr<float64Value> _:
            name = "float";
            break;
        case ptr<intValue> _:
            name = "int";
            break;
        case ptr<int64Value> _:
            name = "int";
            break;
        case ptr<stringValue> _:
            name = "string";
            break;
        case ptr<uintValue> _:
            name = "uint";
            break;
        case ptr<uint64Value> _:
            name = "uint";
            break;
    }
    return ;

}

// PrintDefaults prints, to standard error unless configured otherwise, the
// default values of all defined command-line flags in the set. See the
// documentation for the global function PrintDefaults for more information.
private static void PrintDefaults(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    f.VisitAll(flag => {
        ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
        fmt.Fprintf(_addr_b, "  -%s", flag.Name); // Two spaces before -; see next two comments.
        var (name, usage) = UnquoteUsage(_addr_flag);
        if (len(name) > 0) {
            b.WriteString(" ");
            b.WriteString(name);
        }
        if (b.Len() <= 4) { // space, space, '-', 'x'.
            b.WriteString("\t");

        }
        else
 { 
            // Four spaces before the tab triggers good alignment
            // for both 4- and 8-space tab stops.
            b.WriteString("\n    \t");

        }
        b.WriteString(strings.ReplaceAll(usage, "\n", "\n    \t"));

        if (!isZeroValue(_addr_flag, flag.DefValue)) {
            {
                ptr<stringValue> (_, ok) = flag.Value._<ptr<stringValue>>();

                if (ok) { 
                    // put quotes on the value
                    fmt.Fprintf(_addr_b, " (default %q)", flag.DefValue);

                }
                else
 {
                    fmt.Fprintf(_addr_b, " (default %v)", flag.DefValue);
                }

            }

        }
        fmt.Fprint(f.Output(), b.String(), "\n");

    });

}

// PrintDefaults prints, to standard error unless configured otherwise,
// a usage message showing the default settings of all defined
// command-line flags.
// For an integer valued flag x, the default output has the form
//    -x int
//        usage-message-for-x (default 7)
// The usage message will appear on a separate line for anything but
// a bool flag with a one-byte name. For bool flags, the type is
// omitted and if the flag name is one byte the usage message appears
// on the same line. The parenthetical default is omitted if the
// default is the zero value for the type. The listed type, here int,
// can be changed by placing a back-quoted name in the flag's usage
// string; the first such item in the message is taken to be a parameter
// name to show in the message and the back quotes are stripped from
// the message when displayed. For instance, given
//    flag.String("I", "", "search `directory` for include files")
// the output will be
//    -I directory
//        search directory for include files.
//
// To change the destination for flag messages, call CommandLine.SetOutput.
public static void PrintDefaults() {
    CommandLine.PrintDefaults();
}

// defaultUsage is the default function to print a usage message.
private static void defaultUsage(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    if (f.name == "") {
        fmt.Fprintf(f.Output(), "Usage:\n");
    }
    else
 {
        fmt.Fprintf(f.Output(), "Usage of %s:\n", f.name);
    }
    f.PrintDefaults();

}

// NOTE: Usage is not just defaultUsage(CommandLine)
// because it serves (via godoc flag Usage) as the example
// for how to write your own usage function.

// Usage prints a usage message documenting all defined command-line flags
// to CommandLine's output, which by default is os.Stderr.
// It is called when an error occurs while parsing flags.
// The function is a variable that may be changed to point to a custom function.
// By default it prints a simple header and calls PrintDefaults; for details about the
// format of the output and how to control it, see the documentation for PrintDefaults.
// Custom usage functions may choose to exit the program; by default exiting
// happens anyway as the command line's error handling strategy is set to
// ExitOnError.
public static Action Usage = () => {
    fmt.Fprintf(CommandLine.Output(), "Usage of %s:\n", os.Args[0]);
    PrintDefaults();
};

// NFlag returns the number of flags that have been set.
private static nint NFlag(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return len(f.actual);
}

// NFlag returns the number of command-line flags that have been set.
public static nint NFlag() {
    return len(CommandLine.actual);
}

// Arg returns the i'th argument. Arg(0) is the first remaining argument
// after flags have been processed. Arg returns an empty string if the
// requested element does not exist.
private static @string Arg(this ptr<FlagSet> _addr_f, nint i) {
    ref FlagSet f = ref _addr_f.val;

    if (i < 0 || i >= len(f.args)) {
        return "";
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
private static nint NArg(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return len(f.args);
}

// NArg is the number of arguments remaining after flags have been processed.
public static nint NArg() {
    return len(CommandLine.args);
}

// Args returns the non-flag arguments.
private static slice<@string> Args(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return f.args;
}

// Args returns the non-flag command-line arguments.
public static slice<@string> Args() {
    return CommandLine.args;
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
private static void BoolVar(this ptr<FlagSet> _addr_f, ptr<bool> _addr_p, @string name, bool value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref bool p = ref _addr_p.val;

    f.Var(newBoolValue(value, _addr_p), name, usage);
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
public static void BoolVar(ptr<bool> _addr_p, @string name, bool value, @string usage) {
    ref bool p = ref _addr_p.val;

    CommandLine.Var(newBoolValue(value, _addr_p), name, usage);
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
private static ptr<bool> Bool(this ptr<FlagSet> _addr_f, @string name, bool value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<bool> p = @new<bool>();
    f.BoolVar(p, name, value, usage);
    return _addr_p!;
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
public static ptr<bool> Bool(@string name, bool value, @string usage) {
    return _addr_CommandLine.Bool(name, value, usage)!;
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
private static void IntVar(this ptr<FlagSet> _addr_f, ptr<nint> _addr_p, @string name, nint value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref nint p = ref _addr_p.val;

    f.Var(newIntValue(value, _addr_p), name, usage);
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
public static void IntVar(ptr<nint> _addr_p, @string name, nint value, @string usage) {
    ref nint p = ref _addr_p.val;

    CommandLine.Var(newIntValue(value, _addr_p), name, usage);
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
private static ptr<nint> Int(this ptr<FlagSet> _addr_f, @string name, nint value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<int> p = @new<int>();
    f.IntVar(p, name, value, usage);
    return _addr_p!;
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
public static ptr<nint> Int(@string name, nint value, @string usage) {
    return _addr_CommandLine.Int(name, value, usage)!;
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
private static void Int64Var(this ptr<FlagSet> _addr_f, ptr<long> _addr_p, @string name, long value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref long p = ref _addr_p.val;

    f.Var(newInt64Value(value, _addr_p), name, usage);
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
public static void Int64Var(ptr<long> _addr_p, @string name, long value, @string usage) {
    ref long p = ref _addr_p.val;

    CommandLine.Var(newInt64Value(value, _addr_p), name, usage);
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
private static ptr<long> Int64(this ptr<FlagSet> _addr_f, @string name, long value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<int64> p = @new<int64>();
    f.Int64Var(p, name, value, usage);
    return _addr_p!;
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
public static ptr<long> Int64(@string name, long value, @string usage) {
    return _addr_CommandLine.Int64(name, value, usage)!;
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
private static void UintVar(this ptr<FlagSet> _addr_f, ptr<nuint> _addr_p, @string name, nuint value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref nuint p = ref _addr_p.val;

    f.Var(newUintValue(value, _addr_p), name, usage);
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
public static void UintVar(ptr<nuint> _addr_p, @string name, nuint value, @string usage) {
    ref nuint p = ref _addr_p.val;

    CommandLine.Var(newUintValue(value, _addr_p), name, usage);
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
private static ptr<nuint> Uint(this ptr<FlagSet> _addr_f, @string name, nuint value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<uint> p = @new<uint>();
    f.UintVar(p, name, value, usage);
    return _addr_p!;
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
public static ptr<nuint> Uint(@string name, nuint value, @string usage) {
    return _addr_CommandLine.Uint(name, value, usage)!;
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
private static void Uint64Var(this ptr<FlagSet> _addr_f, ptr<ulong> _addr_p, @string name, ulong value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref ulong p = ref _addr_p.val;

    f.Var(newUint64Value(value, _addr_p), name, usage);
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
public static void Uint64Var(ptr<ulong> _addr_p, @string name, ulong value, @string usage) {
    ref ulong p = ref _addr_p.val;

    CommandLine.Var(newUint64Value(value, _addr_p), name, usage);
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
private static ptr<ulong> Uint64(this ptr<FlagSet> _addr_f, @string name, ulong value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<uint64> p = @new<uint64>();
    f.Uint64Var(p, name, value, usage);
    return _addr_p!;
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
public static ptr<ulong> Uint64(@string name, ulong value, @string usage) {
    return _addr_CommandLine.Uint64(name, value, usage)!;
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
private static void StringVar(this ptr<FlagSet> _addr_f, ptr<@string> _addr_p, @string name, @string value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref @string p = ref _addr_p.val;

    f.Var(newStringValue(value, _addr_p), name, usage);
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
public static void StringVar(ptr<@string> _addr_p, @string name, @string value, @string usage) {
    ref @string p = ref _addr_p.val;

    CommandLine.Var(newStringValue(value, _addr_p), name, usage);
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
private static ptr<@string> String(this ptr<FlagSet> _addr_f, @string name, @string value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<string> p = @new<string>();
    f.StringVar(p, name, value, usage);
    return _addr_p!;
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
public static ptr<@string> String(@string name, @string value, @string usage) {
    return _addr_CommandLine.String(name, value, usage)!;
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
private static void Float64Var(this ptr<FlagSet> _addr_f, ptr<double> _addr_p, @string name, double value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref double p = ref _addr_p.val;

    f.Var(newFloat64Value(value, _addr_p), name, usage);
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
public static void Float64Var(ptr<double> _addr_p, @string name, double value, @string usage) {
    ref double p = ref _addr_p.val;

    CommandLine.Var(newFloat64Value(value, _addr_p), name, usage);
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
private static ptr<double> Float64(this ptr<FlagSet> _addr_f, @string name, double value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<float64> p = @new<float64>();
    f.Float64Var(p, name, value, usage);
    return _addr_p!;
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
public static ptr<double> Float64(@string name, double value, @string usage) {
    return _addr_CommandLine.Float64(name, value, usage)!;
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
private static void DurationVar(this ptr<FlagSet> _addr_f, ptr<time.Duration> _addr_p, @string name, time.Duration value, @string usage) {
    ref FlagSet f = ref _addr_f.val;
    ref time.Duration p = ref _addr_p.val;

    f.Var(newDurationValue(value, _addr_p), name, usage);
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static void DurationVar(ptr<time.Duration> _addr_p, @string name, time.Duration value, @string usage) {
    ref time.Duration p = ref _addr_p.val;

    CommandLine.Var(newDurationValue(value, _addr_p), name, usage);
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
private static ptr<time.Duration> Duration(this ptr<FlagSet> _addr_f, @string name, time.Duration value, @string usage) {
    ref FlagSet f = ref _addr_f.val;

    ptr<time.Duration> p = @new<time.Duration>();
    f.DurationVar(p, name, value, usage);
    return _addr_p!;
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static ptr<time.Duration> Duration(@string name, time.Duration value, @string usage) {
    return _addr_CommandLine.Duration(name, value, usage)!;
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
private static error Func(this ptr<FlagSet> _addr_f, @string name, @string usage, Func<@string, error> fn) {
    ref FlagSet f = ref _addr_f.val;

    f.Var(funcValue(fn), name, usage);
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static error Func(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.Func(name, usage, fn);
}

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type Value, which
// typically holds a user-defined implementation of Value. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of Value; in particular, Set would
// decompose the comma-separated string into the slice.
private static void Var(this ptr<FlagSet> _addr_f, Value value, @string name, @string usage) => func((_, panic, _) => {
    ref FlagSet f = ref _addr_f.val;
 
    // Flag must not begin "-" or contain "=".
    if (strings.HasPrefix(name, "-")) {
        panic(f.sprintf("flag %q begins with -", name));
    }
    else if (strings.Contains(name, "=")) {
        panic(f.sprintf("flag %q contains =", name));
    }
    ptr<Flag> flag = addr(new Flag(name,usage,value,value.String()));
    var (_, alreadythere) = f.formal[name];
    if (alreadythere) {
        @string msg = default;
        if (f.name == "") {
            msg = f.sprintf("flag redefined: %s", name);
        }
        else
 {
            msg = f.sprintf("%s flag redefined: %s", f.name, name);
        }
        panic(msg); // Happens only if flags are declared with identical names
    }
    if (f.formal == null) {
        f.formal = make_map<@string, ptr<Flag>>();
    }
    f.formal[name] = flag;

});

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type Value, which
// typically holds a user-defined implementation of Value. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of Value; in particular, Set would
// decompose the comma-separated string into the slice.
public static void Var(Value value, @string name, @string usage) {
    CommandLine.Var(value, name, usage);
}

// sprintf formats the message, prints it to output, and returns it.
private static @string sprintf(this ptr<FlagSet> _addr_f, @string format, params object[] a) {
    a = a.Clone();
    ref FlagSet f = ref _addr_f.val;

    var msg = fmt.Sprintf(format, a);
    fmt.Fprintln(f.Output(), msg);
    return msg;
}

// failf prints to standard error a formatted error and usage message and
// returns the error.
private static error failf(this ptr<FlagSet> _addr_f, @string format, params object[] a) {
    a = a.Clone();
    ref FlagSet f = ref _addr_f.val;

    var msg = f.sprintf(format, a);
    f.usage();
    return error.As(errors.New(msg))!;
}

// usage calls the Usage method for the flag set if one is specified,
// or the appropriate default usage function otherwise.
private static void usage(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    if (f.Usage == null) {
        f.defaultUsage();
    }
    else
 {
        f.Usage();
    }
}

// parseOne parses one flag. It reports whether a flag was seen.
private static (bool, error) parseOne(this ptr<FlagSet> _addr_f) {
    bool _p0 = default;
    error _p0 = default!;
    ref FlagSet f = ref _addr_f.val;

    if (len(f.args) == 0) {
        return (false, error.As(null!)!);
    }
    var s = f.args[0];
    if (len(s) < 2 || s[0] != '-') {
        return (false, error.As(null!)!);
    }
    nint numMinuses = 1;
    if (s[1] == '-') {
        numMinuses++;
        if (len(s) == 2) { // "--" terminates the flags
            f.args = f.args[(int)1..];
            return (false, error.As(null!)!);

        }
    }
    var name = s[(int)numMinuses..];
    if (len(name) == 0 || name[0] == '-' || name[0] == '=') {
        return (false, error.As(f.failf("bad flag syntax: %s", s))!);
    }
    f.args = f.args[(int)1..];
    var hasValue = false;
    @string value = "";
    for (nint i = 1; i < len(name); i++) { // equals cannot be first
        if (name[i] == '=') {
            value = name[(int)i + 1..];
            hasValue = true;
            name = name[(int)0..(int)i];
            break;
        }
    }
    var m = f.formal;
    var (flag, alreadythere) = m[name]; // BUG
    if (!alreadythere) {
        if (name == "help" || name == "h") { // special case for nice help message.
            f.usage();
            return (false, error.As(ErrHelp)!);

        }
        return (false, error.As(f.failf("flag provided but not defined: -%s", name))!);

    }
    {
        boolFlag (fv, ok) = boolFlag.As(flag.Value._<boolFlag>())!;

        if (ok && fv.IsBoolFlag()) { // special case: doesn't need an arg
            if (hasValue) {
                {
                    var err__prev3 = err;

                    var err = fv.Set(value);

                    if (err != null) {
                        return (false, error.As(f.failf("invalid boolean value %q for -%s: %v", value, name, err))!);
                    }

                    err = err__prev3;

                }

            }
            else
 {
                {
                    var err__prev3 = err;

                    err = fv.Set("true");

                    if (err != null) {
                        return (false, error.As(f.failf("invalid boolean flag %s: %v", name, err))!);
                    }

                    err = err__prev3;

                }

            }

        }
        else
 { 
            // It must have a value, which might be the next argument.
            if (!hasValue && len(f.args) > 0) { 
                // value is the next arg
                hasValue = true;
                (value, f.args) = (f.args[0], f.args[(int)1..]);
            }

            if (!hasValue) {
                return (false, error.As(f.failf("flag needs an argument: -%s", name))!);
            }

            {
                var err__prev2 = err;

                err = flag.Value.Set(value);

                if (err != null) {
                    return (false, error.As(f.failf("invalid value %q for flag -%s: %v", value, name, err))!);
                }

                err = err__prev2;

            }

        }
    }

    if (f.actual == null) {
        f.actual = make_map<@string, ptr<Flag>>();
    }
    f.actual[name] = flag;
    return (true, error.As(null!)!);

}

// Parse parses flag definitions from the argument list, which should not
// include the command name. Must be called after all flags in the FlagSet
// are defined and before flags are accessed by the program.
// The return value will be ErrHelp if -help or -h were set but not defined.
private static error Parse(this ptr<FlagSet> _addr_f, slice<@string> arguments) => func((_, panic, _) => {
    ref FlagSet f = ref _addr_f.val;

    f.parsed = true;
    f.args = arguments;
    while (true) {
        var (seen, err) = f.parseOne();
        if (seen) {
            continue;
        }
        if (err == null) {
            break;
        }

        if (f.errorHandling == ContinueOnError) 
            return error.As(err)!;
        else if (f.errorHandling == ExitOnError) 
            if (err == ErrHelp) {
                os.Exit(0);
            }
            os.Exit(2);
        else if (f.errorHandling == PanicOnError) 
            panic(err);
        
    }
    return error.As(null!)!;

});

// Parsed reports whether f.Parse has been called.
private static bool Parsed(this ptr<FlagSet> _addr_f) {
    ref FlagSet f = ref _addr_f.val;

    return f.parsed;
}

// Parse parses the command-line flags from os.Args[1:]. Must be called
// after all flags are defined and before flags are accessed by the program.
public static void Parse() { 
    // Ignore errors; CommandLine is set for ExitOnError.
    CommandLine.Parse(os.Args[(int)1..]);

}

// Parsed reports whether the command-line flags have been parsed.
public static bool Parsed() {
    return CommandLine.Parsed();
}

// CommandLine is the default set of command-line flags, parsed from os.Args.
// The top-level functions such as BoolVar, Arg, and so on are wrappers for the
// methods of CommandLine.
public static var CommandLine = NewFlagSet(os.Args[0], ExitOnError);

private static void init() { 
    // Override generic FlagSet default Usage with call to global Usage.
    // Note: This is not CommandLine.Usage = Usage,
    // because we want any eventual call to use any updated value of Usage,
    // not the value it has when this line is run.
    CommandLine.Usage = commandLineUsage;

}

private static void commandLineUsage() {
    Usage();
}

// NewFlagSet returns a new, empty flag set with the specified name and
// error handling property. If the name is not empty, it will be printed
// in the default usage message and in error messages.
public static ptr<FlagSet> NewFlagSet(@string name, ErrorHandling errorHandling) {
    ptr<FlagSet> f = addr(new FlagSet(name:name,errorHandling:errorHandling,));
    f.Usage = f.defaultUsage;
    return _addr_f!;
}

// Init sets the name and error handling property for a flag set.
// By default, the zero FlagSet uses an empty name and the
// ContinueOnError error handling policy.
private static void Init(this ptr<FlagSet> _addr_f, @string name, ErrorHandling errorHandling) {
    ref FlagSet f = ref _addr_f.val;

    f.name = name;
    f.errorHandling = errorHandling;
}

} // end flag_package
