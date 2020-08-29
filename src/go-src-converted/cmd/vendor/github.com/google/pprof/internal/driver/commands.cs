// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package driver -- go2cs converted at 2020 August 29 10:05:17 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\commands.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using report = go.github.com.google.pprof.@internal.report_package;
using svg = go.github.com.google.pprof.third_party.svg_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        // commands describes the commands accepted by pprof.
        private partial struct commands // : map<@string, ref command>
        {
        }

        // command describes the actions for a pprof command. Includes a
        // function for command-line completion, the report format to use
        // during report generation, any postprocessing functions, and whether
        // the command expects a regexp parameter (typically a function name).
        private partial struct command
        {
            public long format; // report format to generate
            public PostProcessor postProcess; // postprocessing to run on report
            public PostProcessor visualizer; // display output using some callback
            public bool hasParam; // collect a parameter from the CLI
            public @string description; // single-line description text saying what the command does
            public @string usage; // multi-line help text saying how the command is used
        }

        // help returns a help string for a command.
        private static @string help(this ref command c, @string name)
        {
            var message = c.description + "\n";
            if (c.usage != "")
            {
                message += "  Usage:\n";
                var lines = strings.Split(c.usage, "\n");
                foreach (var (_, line) in lines)
                {
                    message += fmt.Sprintf("    %s\n", line);
                }
            }
            return message + "\n";
        }

        // AddCommand adds an additional command to the set of commands
        // accepted by pprof. This enables extensions to add new commands for
        // specialized visualization formats. If the command specified already
        // exists, it is overwritten.
        public static void AddCommand(@string cmd, long format, PostProcessor post, @string desc, @string usage)
        {
            pprofCommands[cmd] = ref new command(format,post,nil,false,desc,usage);
        }

        // SetVariableDefault sets the default value for a pprof
        // variable. This enables extensions to set their own defaults.
        public static void SetVariableDefault(@string variable, @string value)
        {
            {
                var v = pprofVariables[variable];

                if (v != null)
                {
                    v.value = value;
                }

            }
        }

        // PostProcessor is a function that applies post-processing to the report output
        public delegate  error PostProcessor(io.Reader,  io.Writer,  plugin.UI);

        // interactiveMode is true if pprof is running on interactive mode, reading
        // commands from its shell.
        private static var interactiveMode = false;

        // pprofCommands are the report generation commands recognized by pprof.
        private static commands pprofCommands = new commands("comments":{report.Comments,nil,nil,false,"Output all profile comments",""},"disasm":{report.Dis,nil,nil,true,"Output assembly listings annotated with samples",listHelp("disasm",true)},"dot":{report.Dot,nil,nil,false,"Outputs a graph in DOT format",reportHelp("dot",false,true)},"list":{report.List,nil,nil,true,"Output annotated source for functions matching regexp",listHelp("list",false)},"peek":{report.Tree,nil,nil,true,"Output callers/callees of functions matching regexp","peek func_regex\nDisplay callers and callees of functions matching func_regex."},"raw":{report.Raw,nil,nil,false,"Outputs a text representation of the raw profile",""},"tags":{report.Tags,nil,nil,false,"Outputs all tags in the profile","tags [tag_regex]* [-ignore_regex]* [>file]\nList tags with key:value matching tag_regex and exclude ignore_regex."},"text":{report.Text,nil,nil,false,"Outputs top entries in text form",reportHelp("text",true,true)},"top":{report.Text,nil,nil,false,"Outputs top entries in text form",reportHelp("top",true,true)},"traces":{report.Traces,nil,nil,false,"Outputs all profile samples in text form",""},"tree":{report.Tree,nil,nil,false,"Outputs a text rendering of call graph",reportHelp("tree",true,true)},"callgrind":{report.Callgrind,nil,awayFromTTY("callgraph.out"),false,"Outputs a graph in callgrind format",reportHelp("callgrind",false,true)},"proto":{report.Proto,nil,awayFromTTY("pb.gz"),false,"Outputs the profile in compressed protobuf format",""},"topproto":{report.TopProto,nil,awayFromTTY("pb.gz"),false,"Outputs top entries in compressed protobuf format",""},"gif":{report.Dot,invokeDot("gif"),awayFromTTY("gif"),false,"Outputs a graph image in GIF format",reportHelp("gif",false,true)},"pdf":{report.Dot,invokeDot("pdf"),awayFromTTY("pdf"),false,"Outputs a graph in PDF format",reportHelp("pdf",false,true)},"png":{report.Dot,invokeDot("png"),awayFromTTY("png"),false,"Outputs a graph image in PNG format",reportHelp("png",false,true)},"ps":{report.Dot,invokeDot("ps"),awayFromTTY("ps"),false,"Outputs a graph in PS format",reportHelp("ps",false,true)},"svg":{report.Dot,massageDotSVG(),awayFromTTY("svg"),false,"Outputs a graph in SVG format",reportHelp("svg",false,true)},"eog":{report.Dot,invokeDot("svg"),invokeVisualizer("svg",[]string{"eog"}),false,"Visualize graph through eog",reportHelp("eog",false,false)},"evince":{report.Dot,invokeDot("pdf"),invokeVisualizer("pdf",[]string{"evince"}),false,"Visualize graph through evince",reportHelp("evince",false,false)},"gv":{report.Dot,invokeDot("ps"),invokeVisualizer("ps",[]string{"gv --noantialias"}),false,"Visualize graph through gv",reportHelp("gv",false,false)},"web":{report.Dot,massageDotSVG(),invokeVisualizer("svg",browsers()),false,"Visualize graph through web browser",reportHelp("web",false,false)},"kcachegrind":{report.Callgrind,nil,invokeVisualizer("grind",kcachegrind),false,"Visualize report in KCachegrind",reportHelp("kcachegrind",false,false)},"weblist":{report.WebList,nil,invokeVisualizer("html",browsers()),true,"Display annotated source in a web browser",listHelp("weblist",false)},);

        // pprofVariables are the configuration parameters that affect the
        // reported generated by pprof.
        private static variables pprofVariables = new variables("output":&variable{stringKind,"","",helpText("Output filename for file-based outputs")},"drop_negative":&variable{boolKind,"f","",helpText("Ignore negative differences","Do not show any locations with values <0.")},"positive_percentages":&variable{boolKind,"f","",helpText("Ignore negative samples when computing percentages","Do not count negative samples when computing the total value","of the profile, used to compute percentages. If set, and the -base","option is used, percentages reported will be computed against the","main profile, ignoring the base profile.")},"call_tree":&variable{boolKind,"f","",helpText("Create a context-sensitive call tree","Treat locations reached through different paths as separate.")},"relative_percentages":&variable{boolKind,"f","",helpText("Show percentages relative to focused subgraph","If unset, percentages are relative to full graph before focusing","to facilitate comparison with original graph.")},"unit":&variable{stringKind,"minimum","",helpText("Measurement units to display","Scale the sample values to this unit.","For time-based profiles, use seconds, milliseconds, nanoseconds, etc.","For memory profiles, use megabytes, kilobytes, bytes, etc.","Using auto will scale each value independently to the most natural unit.")},"compact_labels":&variable{boolKind,"f","","Show minimal headers"},"source_path":&variable{stringKind,"","","Search path for source files"},"nodecount":&variable{intKind,"-1","",helpText("Max number of nodes to show","Uses heuristics to limit the number of locations to be displayed.","On graphs, dotted edges represent paths through nodes that have been removed.")},"nodefraction":&variable{floatKind,"0.005","","Hide nodes below <f>*total"},"edgefraction":&variable{floatKind,"0.001","","Hide edges below <f>*total"},"trim":&variable{boolKind,"t","",helpText("Honor nodefraction/edgefraction/nodecount defaults","Set to false to get the full profile, without any trimming.")},"focus":&variable{stringKind,"","",helpText("Restricts to samples going through a node matching regexp","Discard samples that do not include a node matching this regexp.","Matching includes the function name, filename or object name.")},"ignore":&variable{stringKind,"","",helpText("Skips paths going through any nodes matching regexp","If set, discard samples that include a node matching this regexp.","Matching includes the function name, filename or object name.")},"prune_from":&variable{stringKind,"","",helpText("Drops any functions below the matched frame.","If set, any frames matching the specified regexp and any frames","below it will be dropped from each sample.")},"hide":&variable{stringKind,"","",helpText("Skips nodes matching regexp","Discard nodes that match this location.","Other nodes from samples that include this location will be shown.","Matching includes the function name, filename or object name.")},"show":&variable{stringKind,"","",helpText("Only show nodes matching regexp","If set, only show nodes that match this location.","Matching includes the function name, filename or object name.")},"tagfocus":&variable{stringKind,"","",helpText("Restricts to samples with tags in range or matched by regexp","Use name=value syntax to limit the matching to a specific tag.","Numeric tag filter examples: 1kb, 1kb:10kb, memory=32mb:","String tag filter examples: foo, foo.*bar, mytag=foo.*bar")},"tagignore":&variable{stringKind,"","",helpText("Discard samples with tags in range or matched by regexp","Use name=value syntax to limit the matching to a specific tag.","Numeric tag filter examples: 1kb, 1kb:10kb, memory=32mb:","String tag filter examples: foo, foo.*bar, mytag=foo.*bar")},"tagshow":&variable{stringKind,"","",helpText("Only consider tags matching this regexp","Discard tags that do not match this regexp")},"taghide":&variable{stringKind,"","",helpText("Skip tags matching this regexp","Discard tags that match this regexp")},"divide_by":&variable{floatKind,"1","",helpText("Ratio to divide all samples before visualization","Divide all samples values by a constant, eg the number of processors or jobs.")},"mean":&variable{boolKind,"f","",helpText("Average sample value over first value (count)","For memory profiles, report average memory per allocation.","For time-based profiles, report average time per event.")},"sample_index":&variable{stringKind,"","",helpText("Sample value to report (0-based index or name)","Profiles contain multiple values per sample.","Use sample_index=i to select the ith value (starting at 0).")},"normalize":&variable{boolKind,"f","",helpText("Scales profile based on the base profile.")},"flat":&variable{boolKind,"t","cumulative",helpText("Sort entries based on own weight")},"cum":&variable{boolKind,"f","cumulative",helpText("Sort entries based on cumulative weight")},"functions":&variable{boolKind,"t","granularity",helpText("Aggregate at the function level.","Takes into account the filename/lineno where the function was defined.")},"files":&variable{boolKind,"f","granularity","Aggregate at the file level."},"lines":&variable{boolKind,"f","granularity","Aggregate at the source code line level."},"addresses":&variable{boolKind,"f","granularity",helpText("Aggregate at the function level.","Includes functions' addresses in the output.")},"noinlines":&variable{boolKind,"f","granularity",helpText("Aggregate at the function level.","Attributes inlined functions to their first out-of-line caller.")},"addressnoinlines":&variable{boolKind,"f","granularity",helpText("Aggregate at the function level, including functions' addresses in the output.","Attributes inlined functions to their first out-of-line caller.")},);

        private static @string helpText(params @string[] s)
        {
            s = s.Clone();

            return strings.Join(s, "\n") + "\n";
        }

        // usage returns a string describing the pprof commands and variables.
        // if commandLine is set, the output reflect cli usage.
        private static @string usage(bool commandLine)
        {
            @string prefix = default;
            if (commandLine)
            {
                prefix = "-";
            }
            Func<@string, @string, @string> fmtHelp = (c, d) =>
            {
                return fmt.Sprintf("    %-16s %s", c, strings.SplitN(d, "\n", 2L)[0L]);
            }
;

            slice<@string> commands = default;
            {
                var name__prev1 = name;

                foreach (var (__name, __cmd) in pprofCommands)
                {
                    name = __name;
                    cmd = __cmd;
                    commands = append(commands, fmtHelp(prefix + name, cmd.description));
                }

                name = name__prev1;
            }

            sort.Strings(commands);

            @string help = default;
            if (commandLine)
            {
                help = "  Output formats (select at most one):\n";
            }
            else
            {
                help = "  Commands:\n";
                commands = append(commands, fmtHelp("o/options", "List options and their current values"));
                commands = append(commands, fmtHelp("quit/exit/^D", "Exit pprof"));
            }
            help = help + strings.Join(commands, "\n") + "\n\n" + "  Options:\n"; 

            // Print help for variables after sorting them.
            // Collect radio variables by their group name to print them together.
            var radioOptions = make_map<@string, slice<@string>>();
            slice<@string> variables = default;
            {
                var name__prev1 = name;

                foreach (var (__name, __vr) in pprofVariables)
                {
                    name = __name;
                    vr = __vr;
                    if (vr.group != "")
                    {
                        radioOptions[vr.group] = append(radioOptions[vr.group], name);
                        continue;
                    }
                    variables = append(variables, fmtHelp(prefix + name, vr.help));
                }

                name = name__prev1;
            }

            sort.Strings(variables);

            help = help + strings.Join(variables, "\n") + "\n\n" + "  Option groups (only set one per group):\n";

            slice<@string> radioStrings = default;
            foreach (var (radio, ops) in radioOptions)
            {
                sort.Strings(ops);
                @string s = new slice<@string>(new @string[] { fmtHelp(radio,"") });
                foreach (var (_, op) in ops)
                {
                    s = append(s, "  " + fmtHelp(prefix + op, pprofVariables[op].help));
                }
                radioStrings = append(radioStrings, strings.Join(s, "\n"));
            }
            sort.Strings(radioStrings);
            return help + strings.Join(radioStrings, "\n");
        }

        private static @string reportHelp(@string c, bool cum, bool redirect)
        {
            @string h = new slice<@string>(new @string[] { c+" [n] [focus_regex]* [-ignore_regex]*", "Include up to n samples", "Include samples matching focus_regex, and exclude ignore_regex." });
            if (cum)
            {
                h[0L] += " [-cum]";
                h = append(h, "-cum sorts the output by cumulative weight");
            }
            if (redirect)
            {
                h[0L] += " >f";
                h = append(h, "Optionally save the report on the file f");
            }
            return strings.Join(h, "\n");
        }

        private static @string listHelp(@string c, bool redirect)
        {
            @string h = new slice<@string>(new @string[] { c+"<func_regex|address> [-focus_regex]* [-ignore_regex]*", "Include functions matching func_regex, or including the address specified.", "Include samples matching focus_regex, and exclude ignore_regex." });
            if (redirect)
            {
                h[0L] += " >f";
                h = append(h, "Optionally save the report on the file f");
            }
            return strings.Join(h, "\n");
        }

        // browsers returns a list of commands to attempt for web visualization.
        private static slice<@string> browsers()
        {
            @string cmds = new slice<@string>(new @string[] { "chrome", "google-chrome", "firefox" });
            switch (runtime.GOOS)
            {
                case "darwin": 
                    return append(cmds, "/usr/bin/open");
                    break;
                case "windows": 
                    return append(cmds, "cmd /c start");
                    break;
                default: 
                    var userBrowser = os.Getenv("BROWSER");
                    if (userBrowser != "")
                    {
                        cmds = append(new slice<@string>(new @string[] { userBrowser, "sensible-browser" }), cmds);
                    }
                    else
                    {
                        cmds = append(new slice<@string>(new @string[] { "sensible-browser" }), cmds);
                    }
                    return append(cmds, "xdg-open");
                    break;
            }
        }

        private static @string kcachegrind = new slice<@string>(new @string[] { "kcachegrind" });

        // awayFromTTY saves the output in a file if it would otherwise go to
        // the terminal screen. This is used to avoid dumping binary data on
        // the screen.
        private static PostProcessor awayFromTTY(@string format)
        {
            return (input, output, ui) =>
            {
                if (output == os.Stdout && (ui.IsTerminal() || interactiveMode))
                {
                    var (tempFile, err) = newTempFile("", "profile", "." + format);
                    if (err != null)
                    {
                        return err;
                    }
                    ui.PrintErr("Generating report in ", tempFile.Name());
                    output = tempFile;
                }
                var (_, err) = io.Copy(output, input);
                return err;
            }
;
        }

        private static PostProcessor invokeDot(@string format)
        {
            return (input, output, ui) =>
            {
                var cmd = exec.Command("dot", "-T" + format);
                cmd.Stdin = input;
                cmd.Stdout = output;
                cmd.Stderr = os.Stderr;
                {
                    var err = cmd.Run();

                    if (err != null)
                    {
                        return fmt.Errorf("Failed to execute dot. Is Graphviz installed? Error: %v", err);
                    }

                }
                return null;
            }
;
        }

        // massageDotSVG invokes the dot tool to generate an SVG image and alters
        // the image to have panning capabilities when viewed in a browser.
        private static PostProcessor massageDotSVG()
        {
            var generateSVG = invokeDot("svg");
            return (input, output, ui) =>
            {
                ptr<object> baseSVG = @new<bytes.Buffer>();
                {
                    var err = generateSVG(input, baseSVG, ui);

                    if (err != null)
                    {
                        return err;
                    }

                }
                var (_, err) = output.Write((slice<byte>)svg.Massage(baseSVG.String()));
                return err;
            }
;
        }

        private static PostProcessor invokeVisualizer(@string suffix, slice<@string> visualizers) => func((defer, _, __) =>
        {
            return (input, output, ui) =>
            {
                var (tempFile, err) = newTempFile(os.TempDir(), "pprof", "." + suffix);
                if (err != null)
                {
                    return err;
                }
                deferDeleteTempFile(tempFile.Name());
                {
                    var (_, err) = io.Copy(tempFile, input);

                    if (err != null)
                    {
                        return err;
                    }

                }
                tempFile.Close(); 
                // Try visualizers until one is successful
                foreach (var (_, v) in visualizers)
                { 
                    // Separate command and arguments for exec.Command.
                    var args = strings.Split(v, " ");
                    if (len(args) == 0L)
                    {
                        continue;
                    }
                    var viewer = exec.Command(args[0L], append(args[1L..], tempFile.Name()));
                    viewer.Stderr = os.Stderr;
                    err = viewer.Start();

                    if (err == null)
                    { 
                        // Wait for a second so that the visualizer has a chance to
                        // open the input file. This needs to be done even if we're
                        // waiting for the visualizer as it can be just a wrapper that
                        // spawns a browser tab and returns right away.
                        defer(t =>
                        {
                            t.Receive();
                        }(time.After(time.Second))); 
                        // On interactive mode, let the visualizer run in the background
                        // so other commands can be issued.
                        if (!interactiveMode)
                        {
                            return viewer.Wait();
                        }
                        return null;
                    }
                }
                return err;
            }
;
        });

        // variables describe the configuration parameters recognized by pprof.
        private partial struct variables // : map<@string, ref variable>
        {
        }

        // variable is a single configuration parameter.
        private partial struct variable
        {
            public long kind; // How to interpret the value, must be one of the enums below.
            public @string value; // Effective value. Only values appropriate for the Kind should be set.
            public @string group; // boolKind variables with the same Group != "" cannot be set simultaneously.
            public @string help; // Text describing the variable, in multiple lines separated by newline.
        }

 
        // variable.kind must be one of these variables.
        private static readonly var boolKind = iota;
        private static readonly var intKind = 0;
        private static readonly var floatKind = 1;
        private static readonly var stringKind = 2;

        // set updates the value of a variable, checking that the value is
        // suitable for the variable Kind.
        private static error set(this variables vars, @string name, @string value)
        {
            var v = vars[name];
            if (v == null)
            {
                return error.As(fmt.Errorf("no variable %s", name));
            }
            error err = default;

            if (v.kind == boolKind) 
                bool b = default;
                b, err = stringToBool(value);

                if (err == null)
                {
                    if (v.group != "" && !b)
                    {
                        err = error.As(fmt.Errorf("%q can only be set to true", name));
                    }
                }
            else if (v.kind == intKind) 
                _, err = strconv.Atoi(value);
            else if (v.kind == floatKind) 
                _, err = strconv.ParseFloat(value, 64L);
            else if (v.kind == stringKind) 
                // Remove quotes, particularly useful for empty values.
                if (len(value) > 1L && strings.HasPrefix(value, "\"") && strings.HasSuffix(value, "\""))
                {
                    value = value[1L..len(value) - 1L];
                }
                        if (err != null)
            {
                return error.As(err);
            }
            vars[name].value = value;
            {
                var group = vars[name].group;

                if (group != "")
                {
                    foreach (var (vname, vvar) in vars)
                    {
                        if (vvar.group == group && vname != name)
                        {
                            vvar.value = "f";
                        }
                    }
                }

            }
            return error.As(err);
        }

        // boolValue returns the value of a boolean variable.
        private static bool boolValue(this ref variable _v) => func(_v, (ref variable v, Defer _, Panic panic, Recover __) =>
        {
            var (b, err) = stringToBool(v.value);
            if (err != null)
            {
                panic("unexpected value " + v.value + " for bool ");
            }
            return b;
        });

        // intValue returns the value of an intKind variable.
        private static long intValue(this ref variable _v) => func(_v, (ref variable v, Defer _, Panic panic, Recover __) =>
        {
            var (i, err) = strconv.Atoi(v.value);
            if (err != null)
            {
                panic("unexpected value " + v.value + " for int ");
            }
            return i;
        });

        // floatValue returns the value of a Float variable.
        private static double floatValue(this ref variable _v) => func(_v, (ref variable v, Defer _, Panic panic, Recover __) =>
        {
            var (f, err) = strconv.ParseFloat(v.value, 64L);
            if (err != null)
            {
                panic("unexpected value " + v.value + " for float ");
            }
            return f;
        });

        // stringValue returns a canonical representation for a variable.
        private static @string stringValue(this ref variable v)
        {

            if (v.kind == boolKind) 
                return fmt.Sprint(v.boolValue());
            else if (v.kind == intKind) 
                return fmt.Sprint(v.intValue());
            else if (v.kind == floatKind) 
                return fmt.Sprint(v.floatValue());
                        return v.value;
        }

        private static (bool, error) stringToBool(@string s)
        {
            switch (strings.ToLower(s))
            {
                case "true": 

                case "t": 

                case "yes": 

                case "y": 

                case "1": 

                case "": 
                    return (true, null);
                    break;
                case "false": 

                case "f": 

                case "no": 

                case "n": 

                case "0": 
                    return (false, null);
                    break;
                default: 
                    return (false, fmt.Errorf("illegal value \"%s\" for bool variable", s));
                    break;
            }
        }

        // makeCopy returns a duplicate of a set of shell variables.
        private static variables makeCopy(this variables vars)
        {
            var varscopy = make(variables, len(vars));
            foreach (var (n, v) in vars)
            {
                var vcopy = v.Value;
                varscopy[n] = ref vcopy;
            }
            return varscopy;
        }
    }
}}}}}}}
