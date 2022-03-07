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

// package driver -- go2cs converted at 2022 March 06 23:23:29 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\interactive.go
using fmt = go.fmt_package;
using io = go.io_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using report = go.github.com.google.pprof.@internal.report_package;
using profile = go.github.com.google.pprof.profile_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class driver_package {

private static @string commentStart = "//:"; // Sentinel for comments on options
private static var tailDigitsRE = regexp.MustCompile("[0-9]+$");

// interactive starts a shell to read pprof commands.
private static error interactive(ptr<profile.Profile> _addr_p, ptr<plugin.Options> _addr_o) {
    ref profile.Profile p = ref _addr_p.val;
    ref plugin.Options o = ref _addr_o.val;
 
    // Enter command processing loop.
    o.UI.SetAutoComplete(newCompleter(functionNames(_addr_p)));
    configure("compact_labels", "true");
    configHelp["sample_index"] += fmt.Sprintf("Or use sample_index=name, with name in %v.\n", sampleTypes(_addr_p)); 

    // Do not wait for the visualizer to complete, to allow multiple
    // graphs to be visualized simultaneously.
    interactiveMode = true;
    var shortcuts = profileShortcuts(_addr_p);

    greetings(_addr_p, o.UI);
    while (true) {
        var (input, err) = o.UI.ReadLine("(pprof) ");
        if (err != null) {
            if (err != io.EOF) {
                return error.As(err)!;
            }
            if (input == "") {
                return error.As(null!)!;
            }
        }
        {
            var input__prev2 = input;

            foreach (var (_, __input) in shortcuts.expand(input)) {
                input = __input; 
                // Process assignments of the form variable=value
                {
                    var s = strings.SplitN(input, "=", 2);

                    if (len(s) > 0) {
                        var name = strings.TrimSpace(s[0]);
                        @string value = default;
                        if (len(s) == 2) {
                            value = s[1];
                            {
                                var comment = strings.LastIndex(value, commentStart);

                                if (comment != -1) {
                                    value = value[..(int)comment];
                                }

                            }

                            value = strings.TrimSpace(value);

                        }

                        if (isConfigurable(name)) { 
                            // All non-bool options require inputs
                            if (len(s) == 1 && !isBoolConfig(name)) {
                                o.UI.PrintErr(fmt.Errorf("please specify a value, e.g. %s=<val>", name));
                                continue;
                            }

                            if (name == "sample_index") { 
                                // Error check sample_index=xxx to ensure xxx is a valid sample type.
                                var (index, err) = p.SampleIndexByName(value);
                                if (err != null) {
                                    o.UI.PrintErr(err);
                                    continue;
                                }

                                if (index < 0 || index >= len(p.SampleType)) {
                                    o.UI.PrintErr(fmt.Errorf("invalid sample_index %q", value));
                                    continue;
                                }

                                value = p.SampleType[index].Type;

                            }

                            {
                                var err = configure(name, value);

                                if (err != null) {
                                    o.UI.PrintErr(err);
                                }

                            }

                            continue;

                        }

                    }

                }


                var tokens = strings.Fields(input);
                if (len(tokens) == 0) {
                    continue;
                }

                switch (tokens[0]) {
                    case "o": 

                    case "options": 
                        printCurrentOptions(_addr_p, o.UI);
                        continue;
                        break;
                    case "exit": 

                    case "quit": 

                    case "q": 
                        return error.As(null!)!;
                        break;
                    case "help": 
                        commandHelp(strings.Join(tokens[(int)1..], " "), o.UI);
                        continue;
                        break;
                }

                var (args, cfg, err) = parseCommandLine(tokens);
                if (err == null) {
                    err = generateReportWrapper(p, args, cfg, o);
                }

                if (err != null) {
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
private static void greetings(ptr<profile.Profile> _addr_p, plugin.UI ui) {
    ref profile.Profile p = ref _addr_p.val;

    var numLabelUnits = identifyNumLabelUnits(p, ui);
    var (ropt, err) = reportOptions(p, numLabelUnits, currentConfig());
    if (err == null) {
        var rpt = report.New(p, ropt);
        ui.Print(strings.Join(report.ProfileLabels(rpt), "\n"));
        if (rpt.Total() == 0 && len(p.SampleType) > 1) {
            ui.Print("No samples were found with the default sample value type.");
            ui.Print("Try \"sample_index\" command to analyze different sample values.", "\n");
        }
    }
    ui.Print("Entering interactive mode (type \"help\" for commands, \"o\" for options)");

}

// shortcuts represents composite commands that expand into a sequence
// of other commands.
private partial struct shortcuts { // : map<@string, slice<@string>>
}

private static slice<@string> expand(this shortcuts a, @string input) {
    input = strings.TrimSpace(input);
    if (a != null) {
        {
            var (r, ok) = a[input];

            if (ok) {
                return r;
            }

        }

    }
    return new slice<@string>(new @string[] { input });

}

private static shortcuts pprofShortcuts = new shortcuts(":":[]string{"focus=","ignore=","hide=","tagfocus=","tagignore="},);

// profileShortcuts creates macros for convenience and backward compatibility.
private static shortcuts profileShortcuts(ptr<profile.Profile> _addr_p) {
    ref profile.Profile p = ref _addr_p.val;

    var s = pprofShortcuts; 
    // Add shortcuts for sample types
    foreach (var (_, st) in p.SampleType) {
        var command = fmt.Sprintf("sample_index=%s", st.Type);
        s[st.Type] = new slice<@string>(new @string[] { command });
        s["total_" + st.Type] = new slice<@string>(new @string[] { "mean=0", command });
        s["mean_" + st.Type] = new slice<@string>(new @string[] { "mean=1", command });
    }    return s;

}

private static slice<@string> sampleTypes(ptr<profile.Profile> _addr_p) {
    ref profile.Profile p = ref _addr_p.val;

    var types = make_slice<@string>(len(p.SampleType));
    foreach (var (i, t) in p.SampleType) {
        types[i] = t.Type;
    }    return types;
}

private static void printCurrentOptions(ptr<profile.Profile> _addr_p, plugin.UI ui) {
    ref profile.Profile p = ref _addr_p.val;

    slice<@string> args = default;
    var current = currentConfig();
    foreach (var (_, f) in configFields) {
        var n = f.name;
        var v = current.get(f);
        @string comment = "";

        if (len(f.choices) > 0) 
            var values = append(new slice<@string>(new @string[] {  }), f.choices);
            sort.Strings(values);
            comment = "[" + strings.Join(values, " | ") + "]";
        else if (n == "sample_index") 
            var st = sampleTypes(_addr_p);
            if (v == "") { 
                // Apply default (last sample index).
                v = st[len(st) - 1];

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
                if (comment != "") {
            comment = commentStart + " " + comment;
        }
        args = append(args, fmt.Sprintf("  %-25s = %-20s %s", n, v, comment));

    }    sort.Strings(args);
    ui.Print(strings.Join(args, "\n"));

}

// parseCommandLine parses a command and returns the pprof command to
// execute and the configuration to use for the report.
private static (slice<@string>, config, error) parseCommandLine(slice<@string> input) {
    slice<@string> _p0 = default;
    config _p0 = default;
    error _p0 = default!;

    var cmd = input[..(int)1];
    var args = input[(int)1..];
    var name = cmd[0];

    var c = pprofCommands[name];
    if (c == null) { 
        // Attempt splitting digits on abbreviated commands (eg top10)
        {
            var d = tailDigitsRE.FindString(name);

            if (d != "" && d != name) {
                name = name[..(int)len(name) - len(d)];
                (cmd[0], args) = (name, append(new slice<@string>(new @string[] { d }), args));                c = pprofCommands[name];
            }

        }

    }
    if (c == null) {
        {
            var (_, ok) = configHelp[name];

            if (ok) {
                @string value = "<val>";
                if (len(args) > 0) {
                    value = args[0];
                }
                return (null, new config(), error.As(fmt.Errorf("did you mean: %s=%s", name, value))!);
            }

        }

        return (null, new config(), error.As(fmt.Errorf("unrecognized command: %q", name))!);

    }
    if (c.hasParam) {
        if (len(args) == 0) {
            return (null, new config(), error.As(fmt.Errorf("command %s requires an argument", name))!);
        }
        cmd = append(cmd, args[0]);
        args = args[(int)1..];

    }
    var vcopy = currentConfig();

    @string focus = default;    @string ignore = default;

    for (nint i = 0; i < len(args); i++) {
        var t = args[i];
        {
            var (n, err) = strconv.ParseInt(t, 10, 32);

            if (err == null) {
                vcopy.NodeCount = int(n);
                continue;
            }

        }

        switch (t[0]) {
            case '>': 
                var outputFile = t[(int)1..];
                if (outputFile == "") {
                    i++;
                    if (i >= len(args)) {
                        return (null, new config(), error.As(fmt.Errorf("unexpected end of line after >"))!);
                    }
                    outputFile = args[i];
                }
                vcopy.Output = outputFile;
                break;
            case '-': 
                if (t == "--cum" || t == "-cum") {
                    vcopy.Sort = "cum";
                    continue;
                }
                ignore = catRegex(ignore, t[(int)1..]);
                break;
            default: 
                focus = catRegex(focus, t);
                break;
        }

    }

    if (name == "tags") {
        if (focus != "") {
            vcopy.TagFocus = focus;
        }
        if (ignore != "") {
            vcopy.TagIgnore = ignore;
        }
    }
    else
 {
        if (focus != "") {
            vcopy.Focus = focus;
        }
        if (ignore != "") {
            vcopy.Ignore = ignore;
        }
    }
    if (vcopy.NodeCount == -1 && (name == "text" || name == "top")) {
        vcopy.NodeCount = 10;
    }
    return (cmd, vcopy, error.As(null!)!);

}

private static @string catRegex(@string a, @string b) {
    if (a != "" && b != "") {
        return a + "|" + b;
    }
    return a + b;

}

// commandHelp displays help and usage information for all Commands
// and Variables or a specific Command or Variable.
private static void commandHelp(@string args, plugin.UI ui) {
    if (args == "") {
        var help = usage(false);
        help = help + "\n  :   Clear focus/ignore/hide/tagfocus/tagignore\n\n  type \"help <cmd|option>\" for" +
    " more information\n";

        ui.Print(help);
        return ;

    }
    {
        var c = pprofCommands[args];

        if (c != null) {
            ui.Print(c.help(args));
            return ;
        }
    }


    {
        var help__prev1 = help;

        var (help, ok) = configHelp[args];

        if (ok) {
            ui.Print(help + "\n");
            return ;
        }
        help = help__prev1;

    }


    ui.PrintErr("Unknown command: " + args);

}

// newCompleter creates an autocompletion function for a set of commands.
private static Func<@string, @string> newCompleter(slice<@string> fns) {
    return line => {
        {
            var tokens = strings.Fields(line);


            if (len(tokens) == 0)
            {
                goto __switch_break0;
            }
            if (len(tokens) == 1) 
            {
                // Single token -- complete command name
                {
                    var match__prev1 = match;

                    var match = matchVariableOrCommand(tokens[0]);

                    if (match != "") {
                        return match;
                    }

                    match = match__prev1;

                }

                goto __switch_break0;
            }
            if (len(tokens) == 2)
            {
                if (tokens[0] == "help") {
                    {
                        var match__prev2 = match;

                        match = matchVariableOrCommand(tokens[1]);

                        if (match != "") {
                            return tokens[0] + " " + match;
                        }

                        match = match__prev2;

                    }

                    return line;

                }

            }
            // default: 
                // Multiple tokens -- complete using functions, except for tags
                {
                    var cmd = pprofCommands[tokens[0]];

                    if (cmd != null && tokens[0] != "tags") {
                        var lastTokenIdx = len(tokens) - 1;
                        var lastToken = tokens[lastTokenIdx];
                        if (strings.HasPrefix(lastToken, "-")) {
                            lastToken = "-" + functionCompleter(lastToken[(int)1..], fns);
                        }
                        else
 {
                            lastToken = functionCompleter(lastToken, fns);
                        }

                        return strings.Join(append(tokens[..(int)lastTokenIdx], lastToken), " ");

                    }

                }


            __switch_break0:;
        }
        return line;

    };

}

// matchVariableOrCommand attempts to match a string token to the prefix of a Command.
private static @string matchVariableOrCommand(@string token) {
    token = strings.ToLower(token);
    slice<@string> matches = default;
    foreach (var (cmd) in pprofCommands) {
        if (strings.HasPrefix(cmd, token)) {
            matches = append(matches, cmd);
        }
    }    matches = append(matches, completeConfig(token));
    if (len(matches) == 1) {
        return matches[0];
    }
    return "";

}

// functionCompleter replaces provided substring with a function
// name retrieved from a profile if a single match exists. Otherwise,
// it returns unchanged substring. It defaults to no-op if the profile
// is not specified.
private static @string functionCompleter(@string substring, slice<@string> fns) {
    @string found = "";
    foreach (var (_, fName) in fns) {
        if (strings.Contains(fName, substring)) {
            if (found != "") {
                return substring;
            }
            found = fName;
        }
    }    if (found != "") {
        return found;
    }
    return substring;

}

private static slice<@string> functionNames(ptr<profile.Profile> _addr_p) {
    ref profile.Profile p = ref _addr_p.val;

    slice<@string> fns = default;
    foreach (var (_, fn) in p.Function) {
        fns = append(fns, fn.Name);
    }    return fns;
}

} // end driver_package
