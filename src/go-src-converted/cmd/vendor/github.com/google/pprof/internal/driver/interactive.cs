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

// package driver -- go2cs converted at 2020 August 29 10:05:27 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\interactive.go
using fmt = go.fmt_package;
using io = go.io_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using report = go.github.com.google.pprof.@internal.report_package;
using profile = go.github.com.google.pprof.profile_package;
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
        private static @string commentStart = "//:"; // Sentinel for comments on options
        private static var tailDigitsRE = regexp.MustCompile("[0-9]+$");

        // interactive starts a shell to read pprof commands.
        private static error interactive(ref profile.Profile p, ref plugin.Options o)
        { 
            // Enter command processing loop.
            o.UI.SetAutoComplete(newCompleter(functionNames(p)));
            pprofVariables.set("compact_labels", "true");
            pprofVariables["sample_index"].help += fmt.Sprintf("Or use sample_index=name, with name in %v.\n", sampleTypes(p)); 

            // Do not wait for the visualizer to complete, to allow multiple
            // graphs to be visualized simultaneously.
            interactiveMode = true;
            var shortcuts = profileShortcuts(p);

            greetings(p, o.UI);
            while (true)
            {
                var (input, err) = o.UI.ReadLine("(pprof) ");
                if (err != null)
                {
                    if (err != io.EOF)
                    {
                        return error.As(err);
                    }
                    if (input == "")
                    {
                        return error.As(null);
                    }
                }
                {
                    var input__prev2 = input;

                    foreach (var (_, __input) in shortcuts.expand(input))
                    {
                        input = __input; 
                        // Process assignments of the form variable=value
                        {
                            var s = strings.SplitN(input, "=", 2L);

                            if (len(s) > 0L)
                            {
                                var name = strings.TrimSpace(s[0L]);
                                @string value = default;
                                if (len(s) == 2L)
                                {
                                    value = s[1L];
                                    {
                                        var comment = strings.LastIndex(value, commentStart);

                                        if (comment != -1L)
                                        {
                                            value = value[..comment];
                                        }

                                    }
                                    value = strings.TrimSpace(value);
                                }
                                {
                                    var v__prev2 = v;

                                    var v = pprofVariables[name];

                                    if (v != null)
                                    {
                                        if (name == "sample_index")
                                        { 
                                            // Error check sample_index=xxx to ensure xxx is a valid sample type.
                                            var (index, err) = p.SampleIndexByName(value);
                                            if (err != null)
                                            {
                                                o.UI.PrintErr(err);
                                                continue;
                                            }
                                            value = p.SampleType[index].Type;
                                        }
                                        {
                                            var err__prev3 = err;

                                            var err = pprofVariables.set(name, value);

                                            if (err != null)
                                            {
                                                o.UI.PrintErr(err);
                                            }

                                            err = err__prev3;

                                        }
                                        continue;
                                    } 
                                    // Allow group=variable syntax by converting into variable="".

                                    v = v__prev2;

                                } 
                                // Allow group=variable syntax by converting into variable="".
                                {
                                    var v__prev2 = v;

                                    v = pprofVariables[value];

                                    if (v != null && v.group == name)
                                    {
                                        {
                                            var err__prev3 = err;

                                            err = pprofVariables.set(value, "");

                                            if (err != null)
                                            {
                                                o.UI.PrintErr(err);
                                            }

                                            err = err__prev3;

                                        }
                                        continue;
                                    }

                                    v = v__prev2;

                                }
                            }

                        }

                        var tokens = strings.Fields(input);
                        if (len(tokens) == 0L)
                        {
                            continue;
                        }
                        switch (tokens[0L])
                        {
                            case "o": 

                            case "options": 
                                printCurrentOptions(p, o.UI);
                                continue;
                                break;
                            case "exit": 

                            case "quit": 
                                return error.As(null);
                                break;
                            case "help": 
                                commandHelp(strings.Join(tokens[1L..], " "), o.UI);
                                continue;
                                break;
                        }

                        var (args, vars, err) = parseCommandLine(tokens);
                        if (err == null)
                        {
                            err = generateReportWrapper(p, args, vars, o);
                        }
                        if (err != null)
                        {
                            o.UI.PrintErr(err);
                        }
                    }

                    input = input__prev2;
                }

            }

        }

        private static var generateReportWrapper = generateReport; // For testing purposes.

        // greetings prints a brief welcome and some overall profile
        // information before accepting interactive commands.
        private static void greetings(ref profile.Profile p, plugin.UI ui)
        {
            var numLabelUnits = identifyNumLabelUnits(p, ui);
            var (ropt, err) = reportOptions(p, numLabelUnits, pprofVariables);
            if (err == null)
            {
                ui.Print(strings.Join(report.ProfileLabels(report.New(p, ropt)), "\n"));
            }
            ui.Print("Entering interactive mode (type \"help\" for commands, \"o\" for options)");
        }

        // shortcuts represents composite commands that expand into a sequence
        // of other commands.
        private partial struct shortcuts // : map<@string, slice<@string>>
        {
        }

        private static slice<@string> expand(this shortcuts a, @string input)
        {
            input = strings.TrimSpace(input);
            if (a != null)
            {
                {
                    var (r, ok) = a[input];

                    if (ok)
                    {
                        return r;
                    }

                }
            }
            return new slice<@string>(new @string[] { input });
        }

        private static shortcuts pprofShortcuts = new shortcuts(":":[]string{"focus=","ignore=","hide=","tagfocus=","tagignore="},);

        // profileShortcuts creates macros for convenience and backward compatibility.
        private static shortcuts profileShortcuts(ref profile.Profile p)
        {
            var s = pprofShortcuts; 
            // Add shortcuts for sample types
            foreach (var (_, st) in p.SampleType)
            {
                var command = fmt.Sprintf("sample_index=%s", st.Type);
                s[st.Type] = new slice<@string>(new @string[] { command });
                s["total_" + st.Type] = new slice<@string>(new @string[] { "mean=0", command });
                s["mean_" + st.Type] = new slice<@string>(new @string[] { "mean=1", command });
            }
            return s;
        }

        private static slice<@string> sampleTypes(ref profile.Profile p)
        {
            var types = make_slice<@string>(len(p.SampleType));
            foreach (var (i, t) in p.SampleType)
            {
                types[i] = t.Type;
            }
            return types;
        }

        private static void printCurrentOptions(ref profile.Profile p, plugin.UI ui)
        {
            slice<@string> args = default;
            private partial struct groupInfo
            {
                public @string set;
                public slice<@string> values;
            }
            var groups = make_map<@string, ref groupInfo>();
            foreach (var (n, o) in pprofVariables)
            {
                var v = o.stringValue();
                @string comment = "";
                {
                    var g__prev1 = g;

                    var g = o.group;

                    if (g != "")
                    {
                        var (gi, ok) = groups[g];
                        if (!ok)
                        {
                            gi = ref new groupInfo();
                            groups[g] = gi;
                        }
                        if (o.boolValue())
                        {
                            gi.set = n;
                        }
                        gi.values = append(gi.values, n);
                        continue;
                    }

                    g = g__prev1;

                }

                if (n == "sample_index") 
                    var st = sampleTypes(p);
                    if (v == "")
                    { 
                        // Apply default (last sample index).
                        v = st[len(st) - 1L];
                    } 
                    // Add comments for all sample types in profile.
                    comment = "[" + strings.Join(st, " | ") + "]";
                else if (n == "source_path") 
                    continue;
                else if (n == "nodecount" && v == "-1") 
                    comment = "default";
                else if (v == "") 
                    // Add quotes for empty values.
                    v = "\"\"";
                                if (comment != "")
                {
                    comment = commentStart + " " + comment;
                }
                args = append(args, fmt.Sprintf("  %-25s = %-20s %s", n, v, comment));
            }
            {
                var g__prev1 = g;

                foreach (var (__g, __vars) in groups)
                {
                    g = __g;
                    vars = __vars;
                    sort.Strings(vars.values);
                    comment = commentStart + " [" + strings.Join(vars.values, " | ") + "]";
                    args = append(args, fmt.Sprintf("  %-25s = %-20s %s", g, vars.set, comment));
                }

                g = g__prev1;
            }

            sort.Strings(args);
            ui.Print(strings.Join(args, "\n"));
        }

        // parseCommandLine parses a command and returns the pprof command to
        // execute and a set of variables for the report.
        private static (slice<@string>, variables, error) parseCommandLine(slice<@string> input)
        {
            var cmd = input[..1L];
            var args = input[1L..];
            var name = cmd[0L];

            var c = pprofCommands[name];
            if (c == null)
            { 
                // Attempt splitting digits on abbreviated commands (eg top10)
                {
                    var d = tailDigitsRE.FindString(name);

                    if (d != "" && d != name)
                    {
                        name = name[..len(name) - len(d)];
                        cmd[0L] = name;
                        args = append(new slice<@string>(new @string[] { d }), args);
                        c = pprofCommands[name];
                    }

                }
            }
            if (c == null)
            {
                return (null, null, fmt.Errorf("Unrecognized command: %q", name));
            }
            if (c.hasParam)
            {
                if (len(args) == 0L)
                {
                    return (null, null, fmt.Errorf("command %s requires an argument", name));
                }
                cmd = append(cmd, args[0L]);
                args = args[1L..];
            } 

            // Copy the variables as options set in the command line are not persistent.
            var vcopy = pprofVariables.makeCopy();

            @string focus = default;            @string ignore = default;

            for (long i = 0L; i < len(args); i++)
            {
                var t = args[i];
                {
                    var (_, err) = strconv.ParseInt(t, 10L, 32L);

                    if (err == null)
                    {
                        vcopy.set("nodecount", t);
                        continue;
                    }

                }
                switch (t[0L])
                {
                    case '>': 
                        var outputFile = t[1L..];
                        if (outputFile == "")
                        {
                            i++;
                            if (i >= len(args))
                            {
                                return (null, null, fmt.Errorf("Unexpected end of line after >"));
                            }
                            outputFile = args[i];
                        }
                        vcopy.set("output", outputFile);
                        break;
                    case '-': 
                        if (t == "--cum" || t == "-cum")
                        {
                            vcopy.set("cum", "t");
                            continue;
                        }
                        ignore = catRegex(ignore, t[1L..]);
                        break;
                    default: 
                        focus = catRegex(focus, t);
                        break;
                }
            }


            if (name == "tags")
            {
                updateFocusIgnore(vcopy, "tag", focus, ignore);
            }
            else
            {
                updateFocusIgnore(vcopy, "", focus, ignore);
            }
            if (vcopy["nodecount"].intValue() == -1L && (name == "text" || name == "top"))
            {
                vcopy.set("nodecount", "10");
            }
            return (cmd, vcopy, null);
        }

        private static void updateFocusIgnore(variables v, @string prefix, @string f, @string i)
        {
            if (f != "")
            {
                var focus = prefix + "focus";
                v.set(focus, catRegex(v[focus].value, f));
            }
            if (i != "")
            {
                var ignore = prefix + "ignore";
                v.set(ignore, catRegex(v[ignore].value, i));
            }
        }

        private static @string catRegex(@string a, @string b)
        {
            if (a != "" && b != "")
            {
                return a + "|" + b;
            }
            return a + b;
        }

        // commandHelp displays help and usage information for all Commands
        // and Variables or a specific Command or Variable.
        private static void commandHelp(@string args, plugin.UI ui)
        {
            if (args == "")
            {
                var help = usage(false);
                help = help + "\n  :   Clear focus/ignore/hide/tagfocus/tagignore\n\n  type \"help <cmd|option>\" for" +
    " more information\n";

                ui.Print(help);
                return;
            }
            {
                var c = pprofCommands[args];

                if (c != null)
                {
                    ui.Print(c.help(args));
                    return;
                }

            }

            {
                var v = pprofVariables[args];

                if (v != null)
                {
                    ui.Print(v.help + "\n");
                    return;
                }

            }

            ui.PrintErr("Unknown command: " + args);
        }

        // newCompleter creates an autocompletion function for a set of commands.
        private static Func<@string, @string> newCompleter(slice<@string> fns)
        {
            return line =>
            {
                var v = pprofVariables;
                {
                    var tokens = strings.Fields(line);


                    if (len(tokens) == 0L)
                    {
                        goto __switch_break0;
                    }
                    if (len(tokens) == 1L) 
                    {
                        // Single token -- complete command name
                        {
                            var match__prev1 = match;

                            var match = matchVariableOrCommand(v, tokens[0L]);

                            if (match != "")
                            {
                                return match;
                            }

                            match = match__prev1;

                        }
                        goto __switch_break0;
                    }
                    if (len(tokens) == 2L)
                    {
                        if (tokens[0L] == "help")
                        {
                            {
                                var match__prev2 = match;

                                match = matchVariableOrCommand(v, tokens[1L]);

                                if (match != "")
                                {
                                    return tokens[0L] + " " + match;
                                }

                                match = match__prev2;

                            }
                            return line;
                        }
                    }
                    // default: 
                        // Multiple tokens -- complete using functions, except for tags
                        {
                            var cmd = pprofCommands[tokens[0L]];

                            if (cmd != null && tokens[0L] != "tags")
                            {
                                var lastTokenIdx = len(tokens) - 1L;
                                var lastToken = tokens[lastTokenIdx];
                                if (strings.HasPrefix(lastToken, "-"))
                                {
                                    lastToken = "-" + functionCompleter(lastToken[1L..], fns);
                                }
                                else
                                {
                                    lastToken = functionCompleter(lastToken, fns);
                                }
                                return strings.Join(append(tokens[..lastTokenIdx], lastToken), " ");
                            }

                        }

                    __switch_break0:;
                }
                return line;
            }
;
        }

        // matchCommand attempts to match a string token to the prefix of a Command.
        private static @string matchVariableOrCommand(variables v, @string token)
        {
            token = strings.ToLower(token);
            @string found = "";
            foreach (var (cmd) in pprofCommands)
            {
                if (strings.HasPrefix(cmd, token))
                {
                    if (found != "")
                    {
                        return "";
                    }
                    found = cmd;
                }
            }
            foreach (var (variable) in v)
            {
                if (strings.HasPrefix(variable, token))
                {
                    if (found != "")
                    {
                        return "";
                    }
                    found = variable;
                }
            }
            return found;
        }

        // functionCompleter replaces provided substring with a function
        // name retrieved from a profile if a single match exists. Otherwise,
        // it returns unchanged substring. It defaults to no-op if the profile
        // is not specified.
        private static @string functionCompleter(@string substring, slice<@string> fns)
        {
            @string found = "";
            foreach (var (_, fName) in fns)
            {
                if (strings.Contains(fName, substring))
                {
                    if (found != "")
                    {
                        return substring;
                    }
                    found = fName;
                }
            }
            if (found != "")
            {
                return found;
            }
            return substring;
        }

        private static slice<@string> functionNames(ref profile.Profile p)
        {
            slice<@string> fns = default;
            foreach (var (_, fn) in p.Function)
            {
                fns = append(fns, fn.Name);
            }
            return fns;
        }
    }
}}}}}}}
