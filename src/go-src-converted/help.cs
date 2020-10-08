// package analysisflags -- go2cs converted at 2020 October 08 04:54:36 UTC
// import "golang.org/x/tools/go/analysis/internal/analysisflags" ==> using analysisflags = go.golang.org.x.tools.go.analysis.@internal.analysisflags_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\internal\analysisflags\help.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace @internal
{
    public static partial class analysisflags_package
    {
        private static readonly @string help = (@string)@"PROGNAME is a tool for static analysis of Go programs.

PROGNAME examines Go source code and reports suspicious constructs,
such as Printf calls whose arguments do not align with the format
string. It uses heuristics that do not guarantee all reports are
genuine problems, but it can find errors not caught by the compilers.
";

        // Help implements the help subcommand for a multichecker or unitchecker
        // style command. The optional args specify the analyzers to describe.
        // Help calls log.Fatal if no such analyzer exists.


        // Help implements the help subcommand for a multichecker or unitchecker
        // style command. The optional args specify the analyzers to describe.
        // Help calls log.Fatal if no such analyzer exists.
        public static void Help(@string progname, slice<ptr<analysis.Analyzer>> analyzers, slice<@string> args)
        { 
            // No args: show summary of all analyzers.
            if (len(args) == 0L)
            {
                fmt.Println(strings.Replace(help, "PROGNAME", progname, -1L));
                fmt.Println("Registered analyzers:");
                fmt.Println();
                sort.Slice(analyzers, (i, j) =>
                {
                    return analyzers[i].Name < analyzers[j].Name;
                });
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in analyzers)
                    {
                        a = __a;
                        var title = strings.Split(a.Doc, "\n\n")[0L];
                        fmt.Printf("    %-12s %s\n", a.Name, title);
                    }

                    a = a__prev1;
                }

                fmt.Println("\nBy default all analyzers are run.");
                fmt.Println("To select specific analyzers, use the -NAME flag for each one,");
                fmt.Println(" or -NAME=false to run all analyzers not explicitly disabled."); 

                // Show only the core command-line flags.
                fmt.Println("\nCore flags:");
                fmt.Println();
                var fs = flag.NewFlagSet("", flag.ExitOnError);
                flag.VisitAll(f =>
                {
                    if (!strings.Contains(f.Name, "."))
                    {
                        fs.Var(f.Value, f.Name, f.Usage);
                    }

                });
                fs.SetOutput(os.Stdout);
                fs.PrintDefaults();

                fmt.Printf("\nTo see details and flags of a specific analyzer, run '%s help name'.\n", progname);

                return ;

            } 

            // Show help on specific analyzer(s).
outer:
            foreach (var (_, arg) in args)
            {
                {
                    var a__prev2 = a;

                    foreach (var (_, __a) in analyzers)
                    {
                        a = __a;
                        if (a.Name == arg)
                        {
                            var paras = strings.Split(a.Doc, "\n\n");
                            title = paras[0L];
                            fmt.Printf("%s: %s\n", a.Name, title); 

                            // Show only the flags relating to this analysis,
                            // properly prefixed.
                            var first = true;
                            fs = flag.NewFlagSet(a.Name, flag.ExitOnError);
                            a.Flags.VisitAll(f =>
                            {
                                if (first)
                                {
                                    first = false;
                                    fmt.Println("\nAnalyzer flags:");
                                    fmt.Println();
                                }

                                fs.Var(f.Value, a.Name + "." + f.Name, f.Usage);

                            });
                            fs.SetOutput(os.Stdout);
                            fs.PrintDefaults();

                            if (len(paras) > 1L)
                            {
                                fmt.Printf("\n%s\n", strings.Join(paras[1L..], "\n\n"));
                            }

                            _continueouter = true;
                            break;
                        }

                    }

                    a = a__prev2;
                }

                log.Fatalf("Analyzer %q not registered", arg);

            }

        }
    }
}}}}}}}
