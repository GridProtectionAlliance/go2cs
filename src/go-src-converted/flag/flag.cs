// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
    Package flag implements command-line flag parsing.

    Usage:

    Define flags using flag.String(), Bool(), Int(), etc.

    This declares an integer flag, -flagname, stored in the pointer ip, with type *int.
        import "flag"
        var ip = flag.Int("flagname", 1234, "help message for flagname")
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

    Command line flag syntax:
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
// package flag -- go2cs converted at 2020 August 29 08:34:18 UTC
// import "flag" ==> using flag = go.flag_package
// Original source: C:\Go\src\flag\flag.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class flag_package
    {
        // ErrHelp is the error returned if the -help or -h flag is invoked
        // but no such flag is defined.
        public static var ErrHelp = errors.New("flag: help requested");

        // -- bool Value
        private partial struct boolValue // : bool
        {
        }

        private static ref boolValue newBoolValue(bool val, ref bool p)
        {
            p.Value = val;
            return (boolValue.Value)(p);
        }

        private static error Set(this ref boolValue b, @string s)
        {
            var (v, err) = strconv.ParseBool(s);
            b.Value = boolValue(v);
            return error.As(err);
        }

        private static void Get(this ref boolValue b)
        {
            return bool(b.Value);
        }

        private static @string String(this ref boolValue b)
        {
            return strconv.FormatBool(bool(b.Value));
        }

        private static bool IsBoolFlag(this ref boolValue b)
        {
            return true;
        }

        // optional interface to indicate boolean flags that can be
        // supplied without "=value" text
        private partial interface boolFlag : Value
        {
            bool IsBoolFlag();
        }

        // -- int Value
        private partial struct intValue // : long
        {
        }

        private static ref intValue newIntValue(long val, ref long p)
        {
            p.Value = val;
            return (intValue.Value)(p);
        }

        private static error Set(this ref intValue i, @string s)
        {
            var (v, err) = strconv.ParseInt(s, 0L, strconv.IntSize);
            i.Value = intValue(v);
            return error.As(err);
        }

        private static void Get(this ref intValue i)
        {
            return int(i.Value);
        }

        private static @string String(this ref intValue i)
        {
            return strconv.Itoa(int(i.Value));
        }

        // -- int64 Value
        private partial struct int64Value // : long
        {
        }

        private static ref int64Value newInt64Value(long val, ref long p)
        {
            p.Value = val;
            return (int64Value.Value)(p);
        }

        private static error Set(this ref int64Value i, @string s)
        {
            var (v, err) = strconv.ParseInt(s, 0L, 64L);
            i.Value = int64Value(v);
            return error.As(err);
        }

        private static void Get(this ref int64Value i)
        {
            return int64(i.Value);
        }

        private static @string String(this ref int64Value i)
        {
            return strconv.FormatInt(int64(i.Value), 10L);
        }

        // -- uint Value
        private partial struct uintValue // : ulong
        {
        }

        private static ref uintValue newUintValue(ulong val, ref ulong p)
        {
            p.Value = val;
            return (uintValue.Value)(p);
        }

        private static error Set(this ref uintValue i, @string s)
        {
            var (v, err) = strconv.ParseUint(s, 0L, strconv.IntSize);
            i.Value = uintValue(v);
            return error.As(err);
        }

        private static void Get(this ref uintValue i)
        {
            return uint(i.Value);
        }

        private static @string String(this ref uintValue i)
        {
            return strconv.FormatUint(uint64(i.Value), 10L);
        }

        // -- uint64 Value
        private partial struct uint64Value // : ulong
        {
        }

        private static ref uint64Value newUint64Value(ulong val, ref ulong p)
        {
            p.Value = val;
            return (uint64Value.Value)(p);
        }

        private static error Set(this ref uint64Value i, @string s)
        {
            var (v, err) = strconv.ParseUint(s, 0L, 64L);
            i.Value = uint64Value(v);
            return error.As(err);
        }

        private static void Get(this ref uint64Value i)
        {
            return uint64(i.Value);
        }

        private static @string String(this ref uint64Value i)
        {
            return strconv.FormatUint(uint64(i.Value), 10L);
        }

        // -- string Value
        private partial struct stringValue // : @string
        {
        }

        private static ref stringValue newStringValue(@string val, ref @string p)
        {
            p.Value = val;
            return (stringValue.Value)(p);
        }

        private static error Set(this ref stringValue s, @string val)
        {
            s.Value = stringValue(val);
            return error.As(null);
        }

        private static void Get(this ref stringValue s)
        {
            return string(s.Value);
        }

        private static @string String(this ref stringValue s)
        {
            return string(s.Value);
        }

        // -- float64 Value
        private partial struct float64Value // : double
        {
        }

        private static ref float64Value newFloat64Value(double val, ref double p)
        {
            p.Value = val;
            return (float64Value.Value)(p);
        }

        private static error Set(this ref float64Value f, @string s)
        {
            var (v, err) = strconv.ParseFloat(s, 64L);
            f.Value = float64Value(v);
            return error.As(err);
        }

        private static void Get(this ref float64Value f)
        {
            return float64(f.Value);
        }

        private static @string String(this ref float64Value f)
        {
            return strconv.FormatFloat(float64(f.Value), 'g', -1L, 64L);
        }

        // -- time.Duration Value
        private partial struct durationValue // : time.Duration
        {
        }

        private static ref durationValue newDurationValue(time.Duration val, ref time.Duration p)
        {
            p.Value = val;
            return (durationValue.Value)(p);
        }

        private static error Set(this ref durationValue d, @string s)
        {
            var (v, err) = time.ParseDuration(s);
            d.Value = durationValue(v);
            return error.As(err);
        }

        private static void Get(this ref durationValue d)
        {
            return time.Duration(d.Value);
        }

        private static @string String(this ref durationValue d)
        {
            return (time.Duration.Value)(d).String();
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
        public partial interface Value
        {
            error String();
            error Set(@string _p0);
        }

        // Getter is an interface that allows the contents of a Value to be retrieved.
        // It wraps the Value interface, rather than being part of it, because it
        // appeared after Go 1 and its compatibility rules. All Value types provided
        // by this package satisfy the Getter interface.
        public partial interface Getter : Value
        {
            void Get();
        }

        // ErrorHandling defines how FlagSet.Parse behaves if the parse fails.
        public partial struct ErrorHandling // : long
        {
        }

        // These constants cause FlagSet.Parse to behave as described if the parse fails.
        public static readonly ErrorHandling ContinueOnError = iota; // Return a descriptive error.
        public static readonly var ExitOnError = 0; // Call os.Exit(2).
        public static readonly var PanicOnError = 1; // Call panic with a descriptive error.

        // A FlagSet represents a set of defined flags. The zero value of a FlagSet
        // has no name and has ContinueOnError error handling.
        public partial struct FlagSet
        {
            public Action Usage;
            public @string name;
            public bool parsed;
            public map<@string, ref Flag> actual;
            public map<@string, ref Flag> formal;
            public slice<@string> args; // arguments after flags
            public ErrorHandling errorHandling;
            public io.Writer output; // nil means stderr; use out() accessor
        }

        // A Flag represents the state of a flag.
        public partial struct Flag
        {
            public @string Name; // name as it appears on command line
            public @string Usage; // help message
            public Value Value; // value as set
            public @string DefValue; // default value (as text); for usage message
        }

        // sortFlags returns the flags as a slice in lexicographical sorted order.
        private static slice<ref Flag> sortFlags(map<@string, ref Flag> flags)
        {
            var list = make(sort.StringSlice, len(flags));
            long i = 0L;
            foreach (var (_, f) in flags)
            {
                list[i] = f.Name;
                i++;
            }
            list.Sort();
            var result = make_slice<ref Flag>(len(list));
            {
                long i__prev1 = i;

                foreach (var (__i, __name) in list)
                {
                    i = __i;
                    name = __name;
                    result[i] = flags[name];
                }

                i = i__prev1;
            }

            return result;
        }

        // Output returns the destination for usage and error messages. os.Stderr is returned if
        // output was not set or was set to nil.
        private static io.Writer Output(this ref FlagSet f)
        {
            if (f.output == null)
            {
                return os.Stderr;
            }
            return f.output;
        }

        // Name returns the name of the flag set.
        private static @string Name(this ref FlagSet f)
        {
            return f.name;
        }

        // ErrorHandling returns the error handling behavior of the flag set.
        private static ErrorHandling ErrorHandling(this ref FlagSet f)
        {
            return f.errorHandling;
        }

        // SetOutput sets the destination for usage and error messages.
        // If output is nil, os.Stderr is used.
        private static void SetOutput(this ref FlagSet f, io.Writer output)
        {
            f.output = output;
        }

        // VisitAll visits the flags in lexicographical order, calling fn for each.
        // It visits all flags, even those not set.
        private static void VisitAll(this ref FlagSet f, Action<ref Flag> fn)
        {
            foreach (var (_, flag) in sortFlags(f.formal))
            {
                fn(flag);
            }
        }

        // VisitAll visits the command-line flags in lexicographical order, calling
        // fn for each. It visits all flags, even those not set.
        public static void VisitAll(Action<ref Flag> fn)
        {
            CommandLine.VisitAll(fn);
        }

        // Visit visits the flags in lexicographical order, calling fn for each.
        // It visits only those flags that have been set.
        private static void Visit(this ref FlagSet f, Action<ref Flag> fn)
        {
            foreach (var (_, flag) in sortFlags(f.actual))
            {
                fn(flag);
            }
        }

        // Visit visits the command-line flags in lexicographical order, calling fn
        // for each. It visits only those flags that have been set.
        public static void Visit(Action<ref Flag> fn)
        {
            CommandLine.Visit(fn);
        }

        // Lookup returns the Flag structure of the named flag, returning nil if none exists.
        private static ref Flag Lookup(this ref FlagSet f, @string name)
        {
            return f.formal[name];
        }

        // Lookup returns the Flag structure of the named command-line flag,
        // returning nil if none exists.
        public static ref Flag Lookup(@string name)
        {
            return CommandLine.formal[name];
        }

        // Set sets the value of the named flag.
        private static error Set(this ref FlagSet f, @string name, @string value)
        {
            var (flag, ok) = f.formal[name];
            if (!ok)
            {
                return error.As(fmt.Errorf("no such flag -%v", name));
            }
            var err = flag.Value.Set(value);
            if (err != null)
            {
                return error.As(err);
            }
            if (f.actual == null)
            {
                f.actual = make_map<@string, ref Flag>();
            }
            f.actual[name] = flag;
            return error.As(null);
        }

        // Set sets the value of the named command-line flag.
        public static error Set(@string name, @string value)
        {
            return error.As(CommandLine.Set(name, value));
        }

        // isZeroValue guesses whether the string represents the zero
        // value for a flag. It is not accurate but in practice works OK.
        private static bool isZeroValue(ref Flag flag, @string value)
        { 
            // Build a zero value of the flag's Value type, and see if the
            // result of calling its String method equals the value passed in.
            // This works unless the Value type is itself an interface type.
            var typ = reflect.TypeOf(flag.Value);
            reflect.Value z = default;
            if (typ.Kind() == reflect.Ptr)
            {
                z = reflect.New(typ.Elem());
            }
            else
            {
                z = reflect.Zero(typ);
            }
            if (value == z.Interface()._<Value>().String())
            {
                return true;
            }
            switch (value)
            {
                case "false": 

                case "": 

                case "0": 
                    return true;
                    break;
            }
            return false;
        }

        // UnquoteUsage extracts a back-quoted name from the usage
        // string for a flag and returns it and the un-quoted usage.
        // Given "a `name` to show" it returns ("name", "a name to show").
        // If there are no back quotes, the name is an educated guess of the
        // type of the flag's value, or the empty string if the flag is boolean.
        public static (@string, @string) UnquoteUsage(ref Flag flag)
        { 
            // Look for a back-quoted name, but avoid the strings package.
            usage = flag.Usage;
            for (long i = 0L; i < len(usage); i++)
            {
                if (usage[i] == '`')
                {
                    for (var j = i + 1L; j < len(usage); j++)
                    {
                        if (usage[j] == '`')
                        {
                            name = usage[i + 1L..j];
                            usage = usage[..i] + name + usage[j + 1L..];
                            return (name, usage);
                        }
                    }

                    break; // Only one back quote; use type name.
                }
            } 
            // No explicit name, so use type if we can find one.
 
            // No explicit name, so use type if we can find one.
            name = "value";
            switch (flag.Value.type())
            {
                case boolFlag _:
                    name = "";
                    break;
                case ref durationValue _:
                    name = "duration";
                    break;
                case ref float64Value _:
                    name = "float";
                    break;
                case ref intValue _:
                    name = "int";
                    break;
                case ref int64Value _:
                    name = "int";
                    break;
                case ref stringValue _:
                    name = "string";
                    break;
                case ref uintValue _:
                    name = "uint";
                    break;
                case ref uint64Value _:
                    name = "uint";
                    break;
            }
            return;
        }

        // PrintDefaults prints, to standard error unless configured otherwise, the
        // default values of all defined command-line flags in the set. See the
        // documentation for the global function PrintDefaults for more information.
        private static void PrintDefaults(this ref FlagSet f)
        {
            f.VisitAll(flag =>
            {
                var s = fmt.Sprintf("  -%s", flag.Name); // Two spaces before -; see next two comments.
                var (name, usage) = UnquoteUsage(flag);
                if (len(name) > 0L)
                {
                    s += " " + name;
                } 
                // Boolean flags of one ASCII letter are so common we
                // treat them specially, putting their usage on the same line.
                if (len(s) <= 4L)
                { // space, space, '-', 'x'.
                    s += "\t";
                }
                else
                { 
                    // Four spaces before the tab triggers good alignment
                    // for both 4- and 8-space tab stops.
                    s += "\n    \t";
                }
                s += strings.Replace(usage, "\n", "\n    \t", -1L);

                if (!isZeroValue(flag, flag.DefValue))
                {
                    {
                        ref stringValue (_, ok) = flag.Value._<ref stringValue>();

                        if (ok)
                        { 
                            // put quotes on the value
                            s += fmt.Sprintf(" (default %q)", flag.DefValue);
                        }
                        else
                        {
                            s += fmt.Sprintf(" (default %v)", flag.DefValue);
                        }

                    }
                }
                fmt.Fprint(f.Output(), s, "\n");
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
        public static void PrintDefaults()
        {
            CommandLine.PrintDefaults();
        }

        // defaultUsage is the default function to print a usage message.
        private static void defaultUsage(this ref FlagSet f)
        {
            if (f.name == "")
            {
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
        public static Action Usage = () =>
        {
            fmt.Fprintf(CommandLine.Output(), "Usage of %s:\n", os.Args[0L]);
            PrintDefaults();
        };

        // NFlag returns the number of flags that have been set.
        private static long NFlag(this ref FlagSet f)
        {
            return len(f.actual);
        }

        // NFlag returns the number of command-line flags that have been set.
        public static long NFlag()
        {
            return len(CommandLine.actual);
        }

        // Arg returns the i'th argument. Arg(0) is the first remaining argument
        // after flags have been processed. Arg returns an empty string if the
        // requested element does not exist.
        private static @string Arg(this ref FlagSet f, long i)
        {
            if (i < 0L || i >= len(f.args))
            {
                return "";
            }
            return f.args[i];
        }

        // Arg returns the i'th command-line argument. Arg(0) is the first remaining argument
        // after flags have been processed. Arg returns an empty string if the
        // requested element does not exist.
        public static @string Arg(long i)
        {
            return CommandLine.Arg(i);
        }

        // NArg is the number of arguments remaining after flags have been processed.
        private static long NArg(this ref FlagSet f)
        {
            return len(f.args);
        }

        // NArg is the number of arguments remaining after flags have been processed.
        public static long NArg()
        {
            return len(CommandLine.args);
        }

        // Args returns the non-flag arguments.
        private static slice<@string> Args(this ref FlagSet f)
        {
            return f.args;
        }

        // Args returns the non-flag command-line arguments.
        public static slice<@string> Args()
        {
            return CommandLine.args;
        }

        // BoolVar defines a bool flag with specified name, default value, and usage string.
        // The argument p points to a bool variable in which to store the value of the flag.
        private static void BoolVar(this ref FlagSet f, ref bool p, @string name, bool value, @string usage)
        {
            f.Var(newBoolValue(value, p), name, usage);
        }

        // BoolVar defines a bool flag with specified name, default value, and usage string.
        // The argument p points to a bool variable in which to store the value of the flag.
        public static void BoolVar(ref bool p, @string name, bool value, @string usage)
        {
            CommandLine.Var(newBoolValue(value, p), name, usage);
        }

        // Bool defines a bool flag with specified name, default value, and usage string.
        // The return value is the address of a bool variable that stores the value of the flag.
        private static ref bool Bool(this ref FlagSet f, @string name, bool value, @string usage)
        {
            ptr<bool> p = @new<bool>();
            f.BoolVar(p, name, value, usage);
            return p;
        }

        // Bool defines a bool flag with specified name, default value, and usage string.
        // The return value is the address of a bool variable that stores the value of the flag.
        public static ref bool Bool(@string name, bool value, @string usage)
        {
            return CommandLine.Bool(name, value, usage);
        }

        // IntVar defines an int flag with specified name, default value, and usage string.
        // The argument p points to an int variable in which to store the value of the flag.
        private static void IntVar(this ref FlagSet f, ref long p, @string name, long value, @string usage)
        {
            f.Var(newIntValue(value, p), name, usage);
        }

        // IntVar defines an int flag with specified name, default value, and usage string.
        // The argument p points to an int variable in which to store the value of the flag.
        public static void IntVar(ref long p, @string name, long value, @string usage)
        {
            CommandLine.Var(newIntValue(value, p), name, usage);
        }

        // Int defines an int flag with specified name, default value, and usage string.
        // The return value is the address of an int variable that stores the value of the flag.
        private static ref long Int(this ref FlagSet f, @string name, long value, @string usage)
        {
            ptr<object> p = @new<int>();
            f.IntVar(p, name, value, usage);
            return p;
        }

        // Int defines an int flag with specified name, default value, and usage string.
        // The return value is the address of an int variable that stores the value of the flag.
        public static ref long Int(@string name, long value, @string usage)
        {
            return CommandLine.Int(name, value, usage);
        }

        // Int64Var defines an int64 flag with specified name, default value, and usage string.
        // The argument p points to an int64 variable in which to store the value of the flag.
        private static void Int64Var(this ref FlagSet f, ref long p, @string name, long value, @string usage)
        {
            f.Var(newInt64Value(value, p), name, usage);
        }

        // Int64Var defines an int64 flag with specified name, default value, and usage string.
        // The argument p points to an int64 variable in which to store the value of the flag.
        public static void Int64Var(ref long p, @string name, long value, @string usage)
        {
            CommandLine.Var(newInt64Value(value, p), name, usage);
        }

        // Int64 defines an int64 flag with specified name, default value, and usage string.
        // The return value is the address of an int64 variable that stores the value of the flag.
        private static ref long Int64(this ref FlagSet f, @string name, long value, @string usage)
        {
            ptr<object> p = @new<int64>();
            f.Int64Var(p, name, value, usage);
            return p;
        }

        // Int64 defines an int64 flag with specified name, default value, and usage string.
        // The return value is the address of an int64 variable that stores the value of the flag.
        public static ref long Int64(@string name, long value, @string usage)
        {
            return CommandLine.Int64(name, value, usage);
        }

        // UintVar defines a uint flag with specified name, default value, and usage string.
        // The argument p points to a uint variable in which to store the value of the flag.
        private static void UintVar(this ref FlagSet f, ref ulong p, @string name, ulong value, @string usage)
        {
            f.Var(newUintValue(value, p), name, usage);
        }

        // UintVar defines a uint flag with specified name, default value, and usage string.
        // The argument p points to a uint variable in which to store the value of the flag.
        public static void UintVar(ref ulong p, @string name, ulong value, @string usage)
        {
            CommandLine.Var(newUintValue(value, p), name, usage);
        }

        // Uint defines a uint flag with specified name, default value, and usage string.
        // The return value is the address of a uint variable that stores the value of the flag.
        private static ref ulong Uint(this ref FlagSet f, @string name, ulong value, @string usage)
        {
            ptr<object> p = @new<uint>();
            f.UintVar(p, name, value, usage);
            return p;
        }

        // Uint defines a uint flag with specified name, default value, and usage string.
        // The return value is the address of a uint variable that stores the value of the flag.
        public static ref ulong Uint(@string name, ulong value, @string usage)
        {
            return CommandLine.Uint(name, value, usage);
        }

        // Uint64Var defines a uint64 flag with specified name, default value, and usage string.
        // The argument p points to a uint64 variable in which to store the value of the flag.
        private static void Uint64Var(this ref FlagSet f, ref ulong p, @string name, ulong value, @string usage)
        {
            f.Var(newUint64Value(value, p), name, usage);
        }

        // Uint64Var defines a uint64 flag with specified name, default value, and usage string.
        // The argument p points to a uint64 variable in which to store the value of the flag.
        public static void Uint64Var(ref ulong p, @string name, ulong value, @string usage)
        {
            CommandLine.Var(newUint64Value(value, p), name, usage);
        }

        // Uint64 defines a uint64 flag with specified name, default value, and usage string.
        // The return value is the address of a uint64 variable that stores the value of the flag.
        private static ref ulong Uint64(this ref FlagSet f, @string name, ulong value, @string usage)
        {
            ptr<object> p = @new<uint64>();
            f.Uint64Var(p, name, value, usage);
            return p;
        }

        // Uint64 defines a uint64 flag with specified name, default value, and usage string.
        // The return value is the address of a uint64 variable that stores the value of the flag.
        public static ref ulong Uint64(@string name, ulong value, @string usage)
        {
            return CommandLine.Uint64(name, value, usage);
        }

        // StringVar defines a string flag with specified name, default value, and usage string.
        // The argument p points to a string variable in which to store the value of the flag.
        private static void StringVar(this ref FlagSet f, ref @string p, @string name, @string value, @string usage)
        {
            f.Var(newStringValue(value, p), name, usage);
        }

        // StringVar defines a string flag with specified name, default value, and usage string.
        // The argument p points to a string variable in which to store the value of the flag.
        public static void StringVar(ref @string p, @string name, @string value, @string usage)
        {
            CommandLine.Var(newStringValue(value, p), name, usage);
        }

        // String defines a string flag with specified name, default value, and usage string.
        // The return value is the address of a string variable that stores the value of the flag.
        private static ref @string String(this ref FlagSet f, @string name, @string value, @string usage)
        {
            ptr<object> p = @new<string>();
            f.StringVar(p, name, value, usage);
            return p;
        }

        // String defines a string flag with specified name, default value, and usage string.
        // The return value is the address of a string variable that stores the value of the flag.
        public static ref @string String(@string name, @string value, @string usage)
        {
            return CommandLine.String(name, value, usage);
        }

        // Float64Var defines a float64 flag with specified name, default value, and usage string.
        // The argument p points to a float64 variable in which to store the value of the flag.
        private static void Float64Var(this ref FlagSet f, ref double p, @string name, double value, @string usage)
        {
            f.Var(newFloat64Value(value, p), name, usage);
        }

        // Float64Var defines a float64 flag with specified name, default value, and usage string.
        // The argument p points to a float64 variable in which to store the value of the flag.
        public static void Float64Var(ref double p, @string name, double value, @string usage)
        {
            CommandLine.Var(newFloat64Value(value, p), name, usage);
        }

        // Float64 defines a float64 flag with specified name, default value, and usage string.
        // The return value is the address of a float64 variable that stores the value of the flag.
        private static ref double Float64(this ref FlagSet f, @string name, double value, @string usage)
        {
            ptr<object> p = @new<float64>();
            f.Float64Var(p, name, value, usage);
            return p;
        }

        // Float64 defines a float64 flag with specified name, default value, and usage string.
        // The return value is the address of a float64 variable that stores the value of the flag.
        public static ref double Float64(@string name, double value, @string usage)
        {
            return CommandLine.Float64(name, value, usage);
        }

        // DurationVar defines a time.Duration flag with specified name, default value, and usage string.
        // The argument p points to a time.Duration variable in which to store the value of the flag.
        // The flag accepts a value acceptable to time.ParseDuration.
        private static void DurationVar(this ref FlagSet f, ref time.Duration p, @string name, time.Duration value, @string usage)
        {
            f.Var(newDurationValue(value, p), name, usage);
        }

        // DurationVar defines a time.Duration flag with specified name, default value, and usage string.
        // The argument p points to a time.Duration variable in which to store the value of the flag.
        // The flag accepts a value acceptable to time.ParseDuration.
        public static void DurationVar(ref time.Duration p, @string name, time.Duration value, @string usage)
        {
            CommandLine.Var(newDurationValue(value, p), name, usage);
        }

        // Duration defines a time.Duration flag with specified name, default value, and usage string.
        // The return value is the address of a time.Duration variable that stores the value of the flag.
        // The flag accepts a value acceptable to time.ParseDuration.
        private static ref time.Duration Duration(this ref FlagSet f, @string name, time.Duration value, @string usage)
        {
            ptr<time.Duration> p = @new<time.Duration>();
            f.DurationVar(p, name, value, usage);
            return p;
        }

        // Duration defines a time.Duration flag with specified name, default value, and usage string.
        // The return value is the address of a time.Duration variable that stores the value of the flag.
        // The flag accepts a value acceptable to time.ParseDuration.
        public static ref time.Duration Duration(@string name, time.Duration value, @string usage)
        {
            return CommandLine.Duration(name, value, usage);
        }

        // Var defines a flag with the specified name and usage string. The type and
        // value of the flag are represented by the first argument, of type Value, which
        // typically holds a user-defined implementation of Value. For instance, the
        // caller could create a flag that turns a comma-separated string into a slice
        // of strings by giving the slice the methods of Value; in particular, Set would
        // decompose the comma-separated string into the slice.
        private static void Var(this ref FlagSet _f, Value value, @string name, @string usage) => func(_f, (ref FlagSet f, Defer _, Panic panic, Recover __) =>
        { 
            // Remember the default value as a string; it won't change.
            Flag flag = ref new Flag(name,usage,value,value.String());
            var (_, alreadythere) = f.formal[name];
            if (alreadythere)
            {
                @string msg = default;
                if (f.name == "")
                {
                    msg = fmt.Sprintf("flag redefined: %s", name);
                }
                else
                {
                    msg = fmt.Sprintf("%s flag redefined: %s", f.name, name);
                }
                fmt.Fprintln(f.Output(), msg);
                panic(msg); // Happens only if flags are declared with identical names
            }
            if (f.formal == null)
            {
                f.formal = make_map<@string, ref Flag>();
            }
            f.formal[name] = flag;
        });

        // Var defines a flag with the specified name and usage string. The type and
        // value of the flag are represented by the first argument, of type Value, which
        // typically holds a user-defined implementation of Value. For instance, the
        // caller could create a flag that turns a comma-separated string into a slice
        // of strings by giving the slice the methods of Value; in particular, Set would
        // decompose the comma-separated string into the slice.
        public static void Var(Value value, @string name, @string usage)
        {
            CommandLine.Var(value, name, usage);
        }

        // failf prints to standard error a formatted error and usage message and
        // returns the error.
        private static error failf(this ref FlagSet f, @string format, params object[] a)
        {
            var err = fmt.Errorf(format, a);
            fmt.Fprintln(f.Output(), err);
            f.usage();
            return error.As(err);
        }

        // usage calls the Usage method for the flag set if one is specified,
        // or the appropriate default usage function otherwise.
        private static void usage(this ref FlagSet f)
        {
            if (f.Usage == null)
            {
                f.defaultUsage();
            }
            else
            {
                f.Usage();
            }
        }

        // parseOne parses one flag. It reports whether a flag was seen.
        private static (bool, error) parseOne(this ref FlagSet f)
        {
            if (len(f.args) == 0L)
            {
                return (false, null);
            }
            var s = f.args[0L];
            if (len(s) < 2L || s[0L] != '-')
            {
                return (false, null);
            }
            long numMinuses = 1L;
            if (s[1L] == '-')
            {
                numMinuses++;
                if (len(s) == 2L)
                { // "--" terminates the flags
                    f.args = f.args[1L..];
                    return (false, null);
                }
            }
            var name = s[numMinuses..];
            if (len(name) == 0L || name[0L] == '-' || name[0L] == '=')
            {
                return (false, f.failf("bad flag syntax: %s", s));
            } 

            // it's a flag. does it have an argument?
            f.args = f.args[1L..];
            var hasValue = false;
            @string value = "";
            for (long i = 1L; i < len(name); i++)
            { // equals cannot be first
                if (name[i] == '=')
                {
                    value = name[i + 1L..];
                    hasValue = true;
                    name = name[0L..i];
                    break;
                }
            }

            var m = f.formal;
            var (flag, alreadythere) = m[name]; // BUG
            if (!alreadythere)
            {
                if (name == "help" || name == "h")
                { // special case for nice help message.
                    f.usage();
                    return (false, ErrHelp);
                }
                return (false, f.failf("flag provided but not defined: -%s", name));
            }
            {
                boolFlag (fv, ok) = flag.Value._<boolFlag>();

                if (ok && fv.IsBoolFlag())
                { // special case: doesn't need an arg
                    if (hasValue)
                    {
                        {
                            var err__prev3 = err;

                            var err = fv.Set(value);

                            if (err != null)
                            {
                                return (false, f.failf("invalid boolean value %q for -%s: %v", value, name, err));
                            }

                            err = err__prev3;

                        }
                    }
                    else
                    {
                        {
                            var err__prev3 = err;

                            err = fv.Set("true");

                            if (err != null)
                            {
                                return (false, f.failf("invalid boolean flag %s: %v", name, err));
                            }

                            err = err__prev3;

                        }
                    }
                }
                else
                { 
                    // It must have a value, which might be the next argument.
                    if (!hasValue && len(f.args) > 0L)
                    { 
                        // value is the next arg
                        hasValue = true;
                        value = f.args[0L];
                        f.args = f.args[1L..];
                    }
                    if (!hasValue)
                    {
                        return (false, f.failf("flag needs an argument: -%s", name));
                    }
                    {
                        var err__prev2 = err;

                        err = flag.Value.Set(value);

                        if (err != null)
                        {
                            return (false, f.failf("invalid value %q for flag -%s: %v", value, name, err));
                        }

                        err = err__prev2;

                    }
                }

            }
            if (f.actual == null)
            {
                f.actual = make_map<@string, ref Flag>();
            }
            f.actual[name] = flag;
            return (true, null);
        }

        // Parse parses flag definitions from the argument list, which should not
        // include the command name. Must be called after all flags in the FlagSet
        // are defined and before flags are accessed by the program.
        // The return value will be ErrHelp if -help or -h were set but not defined.
        private static error Parse(this ref FlagSet _f, slice<@string> arguments) => func(_f, (ref FlagSet f, Defer _, Panic panic, Recover __) =>
        {
            f.parsed = true;
            f.args = arguments;
            while (true)
            {
                var (seen, err) = f.parseOne();
                if (seen)
                {
                    continue;
                }
                if (err == null)
                {
                    break;
                }

                if (f.errorHandling == ContinueOnError) 
                    return error.As(err);
                else if (f.errorHandling == ExitOnError) 
                    os.Exit(2L);
                else if (f.errorHandling == PanicOnError) 
                    panic(err);
                            }

            return error.As(null);
        });

        // Parsed reports whether f.Parse has been called.
        private static bool Parsed(this ref FlagSet f)
        {
            return f.parsed;
        }

        // Parse parses the command-line flags from os.Args[1:]. Must be called
        // after all flags are defined and before flags are accessed by the program.
        public static void Parse()
        { 
            // Ignore errors; CommandLine is set for ExitOnError.
            CommandLine.Parse(os.Args[1L..]);
        }

        // Parsed reports whether the command-line flags have been parsed.
        public static bool Parsed()
        {
            return CommandLine.Parsed();
        }

        // CommandLine is the default set of command-line flags, parsed from os.Args.
        // The top-level functions such as BoolVar, Arg, and so on are wrappers for the
        // methods of CommandLine.
        public static var CommandLine = NewFlagSet(os.Args[0L], ExitOnError);

        private static void init()
        { 
            // Override generic FlagSet default Usage with call to global Usage.
            // Note: This is not CommandLine.Usage = Usage,
            // because we want any eventual call to use any updated value of Usage,
            // not the value it has when this line is run.
            CommandLine.Usage = commandLineUsage;
        }

        private static void commandLineUsage()
        {
            Usage();
        }

        // NewFlagSet returns a new, empty flag set with the specified name and
        // error handling property.
        public static ref FlagSet NewFlagSet(@string name, ErrorHandling errorHandling)
        {
            FlagSet f = ref new FlagSet(name:name,errorHandling:errorHandling,);
            f.Usage = f.defaultUsage;
            return f;
        }

        // Init sets the name and error handling property for a flag set.
        // By default, the zero FlagSet uses an empty name and the
        // ContinueOnError error handling policy.
        private static void Init(this ref FlagSet f, @string name, ErrorHandling errorHandling)
        {
            f.name = name;
            f.errorHandling = errorHandling;
        }
    }
}
