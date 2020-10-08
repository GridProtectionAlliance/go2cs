// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tool is a harness for writing Go tools.
// package tool -- go2cs converted at 2020 October 08 04:55:42 UTC
// import "golang.org/x/tools/internal/tool" ==> using tool = go.golang.org.x.tools.@internal.tool_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\tool\tool.go
using context = go.context_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using trace = go.runtime.trace_package;
using time = go.time_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class tool_package
    {
        // This file is a harness for writing your main function.
        // The original version of the file is in golang.org/x/tools/internal/tool.
        //
        // It adds a method to the Application type
        //     Main(name, usage string, args []string)
        // which should normally be invoked from a true main as follows:
        //     func main() {
        //       (&Application{}).Main("myapp", "non-flag-command-line-arg-help", os.Args[1:])
        //     }
        // It recursively scans the application object for fields with a tag containing
        //     `flag:"flagname" help:"short help text"``
        // uses all those fields to build command line flags.
        // It expects the Application type to have a method
        //     Run(context.Context, args...string) error
        // which it invokes only after all command line flag processing has been finished.
        // If Run returns an error, the error will be printed to stderr and the
        // application will quit with a non zero exit status.

        // Profile can be embedded in your application struct to automatically
        // add command line arguments and handling for the common profiling methods.
        public partial struct Profile
        {
            [Description("flag:\"profile.cpu\" help:\"write CPU profile to this file\"")]
            public @string CPU;
            [Description("flag:\"profile.mem\" help:\"write memory profile to this file\"")]
            public @string Memory;
            [Description("flag:\"profile.trace\" help:\"write trace log to this file\"")]
            public @string Trace;
        }

        // Application is the interface that must be satisfied by an object passed to Main.
        public partial interface Application
        {
            error Name(); // Most of the help usage is automatically generated, this string should only
// describe the contents of non flag arguments.
            error Usage(); // ShortHelp returns the one line overview of the command.
            error ShortHelp(); // DetailedHelp should print a detailed help message. It will only ever be shown
// when the ShortHelp is also printed, so there is no need to duplicate
// anything from there.
// It is passed the flag set so it can print the default values of the flags.
// It should use the flag sets configured Output to write the help to.
            error DetailedHelp(ptr<flag.FlagSet> _p0); // Run is invoked after all flag processing, and inside the profiling and
// error handling harness.
            error Run(context.Context ctx, params @string[] args);
        }

        // This is the type returned by CommandLineErrorf, which causes the outer main
        // to trigger printing of the command line help.
        private partial struct commandLineError // : @string
        {
        }

        private static @string Error(this commandLineError e)
        {
            return string(e);
        }

        // CommandLineErrorf is like fmt.Errorf except that it returns a value that
        // triggers printing of the command line help.
        // In general you should use this when generating command line validation errors.
        public static error CommandLineErrorf(@string message, params object[] args)
        {
            args = args.Clone();

            return error.As(commandLineError(fmt.Sprintf(message, args)))!;
        }

        // Main should be invoked directly by main function.
        // It will only return if there was no error.  If an error
        // was encountered it is printed to standard error and the
        // application exits with an exit code of 2.
        public static void Main(context.Context ctx, Application app, slice<@string> args)
        {
            var s = flag.NewFlagSet(app.Name(), flag.ExitOnError);
            s.Usage = () =>
            {
                fmt.Fprint(s.Output(), app.ShortHelp());
                fmt.Fprintf(s.Output(), "\n\nUsage: %v [flags] %v\n", app.Name(), app.Usage());
                app.DetailedHelp(s);
            }
;
            {
                var err = Run(ctx, app, args);

                if (err != null)
                {
                    fmt.Fprintf(s.Output(), "%s: %v\n", app.Name(), err);
                    {
                        commandLineError (_, printHelp) = err._<commandLineError>();

                        if (printHelp)
                        {
                            s.Usage();
                        }

                    }

                    os.Exit(2L);

                }

            }

        }

        // Run is the inner loop for Main; invoked by Main, recursively by
        // Run, and by various tests.  It runs the application and returns an
        // error.
        public static error Run(context.Context ctx, Application app, slice<@string> args) => func((defer, _, __) =>
        {
            var s = flag.NewFlagSet(app.Name(), flag.ExitOnError);
            s.Usage = () =>
            {
                fmt.Fprint(s.Output(), app.ShortHelp());
                fmt.Fprintf(s.Output(), "\n\nUsage: %v [flags] %v\n", app.Name(), app.Usage());
                app.DetailedHelp(s);
            }
;
            var p = addFlags(_addr_s, new reflect.StructField(), reflect.ValueOf(app));
            s.Parse(args);

            if (p != null && p.CPU != "")
            {
                var (f, err) = os.Create(p.CPU);
                if (err != null)
                {
                    return error.As(err)!;
                }

                {
                    var err__prev2 = err;

                    var err = pprof.StartCPUProfile(f);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                defer(pprof.StopCPUProfile());

            }

            if (p != null && p.Trace != "")
            {
                (f, err) = os.Create(p.Trace);
                if (err != null)
                {
                    return error.As(err)!;
                }

                {
                    var err__prev2 = err;

                    err = trace.Start(f);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }

                defer(() =>
                {
                    trace.Stop();
                    log.Printf("To view the trace, run:\n$ go tool trace view %s", p.Trace);
                }());

            }

            if (p != null && p.Memory != "")
            {
                (f, err) = os.Create(p.Memory);
                if (err != null)
                {
                    return error.As(err)!;
                }

                defer(() =>
                {
                    runtime.GC(); // get up-to-date statistics
                    {
                        var err__prev2 = err;

                        err = pprof.WriteHeapProfile(f);

                        if (err != null)
                        {
                            log.Printf("Writing memory profile: %v", err);
                        }

                        err = err__prev2;

                    }

                    f.Close();

                }());

            }

            return error.As(app.Run(ctx, s.Args()))!;

        });

        // addFlags scans fields of structs recursively to find things with flag tags
        // and add them to the flag set.
        private static ptr<Profile> addFlags(ptr<flag.FlagSet> _addr_f, reflect.StructField field, reflect.Value value)
        {
            ref flag.FlagSet f = ref _addr_f.val;
 
            // is it a field we are allowed to reflect on?
            if (field.PkgPath != "")
            {
                return _addr_null!;
            } 
            // now see if is actually a flag
            var (flagName, isFlag) = field.Tag.Lookup("flag");
            var help = field.Tag.Get("help");
            if (!isFlag)
            { 
                // not a flag, but it might be a struct with flags in it
                if (value.Elem().Kind() != reflect.Struct)
                {
                    return _addr_null!;
                }

                ptr<Profile> (p, _) = value.Interface()._<ptr<Profile>>(); 
                // go through all the fields of the struct
                var sv = value.Elem();
                for (long i = 0L; i < sv.Type().NumField(); i++)
                {
                    var child = sv.Type().Field(i);
                    var v = sv.Field(i); 
                    // make sure we have a pointer
                    if (v.Kind() != reflect.Ptr)
                    {
                        v = v.Addr();
                    } 
                    // check if that field is a flag or contains flags
                    {
                        var fp = addFlags(_addr_f, child, v);

                        if (fp != null)
                        {
                            p = fp;
                        }

                    }

                }

                return _addr_p!;

            }

            switch (value.Interface().type())
            {
                case flag.Value v:
                    f.Var(v, flagName, help);
                    break;
                case ptr<bool> v:
                    f.BoolVar(v, flagName, v.val, help);
                    break;
                case ptr<time.Duration> v:
                    f.DurationVar(v, flagName, v.val, help);
                    break;
                case ptr<double> v:
                    f.Float64Var(v, flagName, v.val, help);
                    break;
                case ptr<long> v:
                    f.Int64Var(v, flagName, v.val, help);
                    break;
                case ptr<long> v:
                    f.IntVar(v, flagName, v.val, help);
                    break;
                case ptr<@string> v:
                    f.StringVar(v, flagName, v.val, help);
                    break;
                case ptr<ulong> v:
                    f.UintVar(v, flagName, v.val, help);
                    break;
                case ptr<ulong> v:
                    f.Uint64Var(v, flagName, v.val, help);
                    break;
                default:
                {
                    var v = value.Interface().type();
                    log.Fatalf("Cannot understand flag of type %T", v);
                    break;
                }
            }
            return _addr_null!;

        }
    }
}}}}}
