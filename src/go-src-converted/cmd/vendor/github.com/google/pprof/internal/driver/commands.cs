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

// package driver -- go2cs converted at 2022 March 13 06:36:25 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\commands.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using exec = os.exec_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;

using plugin = github.com.google.pprof.@internal.plugin_package;
using report = github.com.google.pprof.@internal.report_package;


// commands describes the commands accepted by pprof.

using System;
public static partial class driver_package {

private partial struct commands { // : map<@string, ptr<command>>
}

// command describes the actions for a pprof command. Includes a
// function for command-line completion, the report format to use
// during report generation, any postprocessing functions, and whether
// the command expects a regexp parameter (typically a function name).
private partial struct command {
    public nint format; // report format to generate
    public PostProcessor postProcess; // postprocessing to run on report
    public PostProcessor visualizer; // display output using some callback
    public bool hasParam; // collect a parameter from the CLI
    public @string description; // single-line description text saying what the command does
    public @string usage; // multi-line help text saying how the command is used
}

// help returns a help string for a command.
private static @string help(this ptr<command> _addr_c, @string name) {
    ref command c = ref _addr_c.val;

    var message = c.description + "\n";
    if (c.usage != "") {
        message += "  Usage:\n";
        var lines = strings.Split(c.usage, "\n");
        foreach (var (_, line) in lines) {
            message += fmt.Sprintf("    %s\n", line);
        }
    }
    return message + "\n";
}

// AddCommand adds an additional command to the set of commands
// accepted by pprof. This enables extensions to add new commands for
// specialized visualization formats. If the command specified already
// exists, it is overwritten.
public static void AddCommand(@string cmd, nint format, PostProcessor post, @string desc, @string usage) {
    pprofCommands[cmd] = addr(new command(format,post,nil,false,desc,usage));
}

// SetVariableDefault sets the default value for a pprof
// variable. This enables extensions to set their own defaults.
public static void SetVariableDefault(@string variable, @string value) {
    configure(variable, value);
}

// PostProcessor is a function that applies post-processing to the report output
public delegate  error PostProcessor(io.Reader,  io.Writer,  plugin.UI);

// interactiveMode is true if pprof is running on interactive mode, reading
// commands from its shell.
private static var interactiveMode = false;

// pprofCommands are the report generation commands recognized by pprof.
private static commands pprofCommands = new commands("comments":{report.Comments,nil,nil,false,"Output all profile comments",""},"disasm":{report.Dis,nil,nil,true,"Output assembly listings annotated with samples",listHelp("disasm",true)},"dot":{report.Dot,nil,nil,false,"Outputs a graph in DOT format",reportHelp("dot",false,true)},"list":{report.List,nil,nil,true,"Output annotated source for functions matching regexp",listHelp("list",false)},"peek":{report.Tree,nil,nil,true,"Output callers/callees of functions matching regexp","peek func_regex\nDisplay callers and callees of functions matching func_regex."},"raw":{report.Raw,nil,nil,false,"Outputs a text representation of the raw profile",""},"tags":{report.Tags,nil,nil,false,"Outputs all tags in the profile","tags [tag_regex]* [-ignore_regex]* [>file]\nList tags with key:value matching tag_regex and exclude ignore_regex."},"text":{report.Text,nil,nil,false,"Outputs top entries in text form",reportHelp("text",true,true)},"top":{report.Text,nil,nil,false,"Outputs top entries in text form",reportHelp("top",true,true)},"traces":{report.Traces,nil,nil,false,"Outputs all profile samples in text form",""},"tree":{report.Tree,nil,nil,false,"Outputs a text rendering of call graph",reportHelp("tree",true,true)},"callgrind":{report.Callgrind,nil,awayFromTTY("callgraph.out"),false,"Outputs a graph in callgrind format",reportHelp("callgrind",false,true)},"proto":{report.Proto,nil,awayFromTTY("pb.gz"),false,"Outputs the profile in compressed protobuf format",""},"topproto":{report.TopProto,nil,awayFromTTY("pb.gz"),false,"Outputs top entries in compressed protobuf format",""},"gif":{report.Dot,invokeDot("gif"),awayFromTTY("gif"),false,"Outputs a graph image in GIF format",reportHelp("gif",false,true)},"pdf":{report.Dot,invokeDot("pdf"),awayFromTTY("pdf"),false,"Outputs a graph in PDF format",reportHelp("pdf",false,true)},"png":{report.Dot,invokeDot("png"),awayFromTTY("png"),false,"Outputs a graph image in PNG format",reportHelp("png",false,true)},"ps":{report.Dot,invokeDot("ps"),awayFromTTY("ps"),false,"Outputs a graph in PS format",reportHelp("ps",false,true)},"svg":{report.Dot,massageDotSVG(),awayFromTTY("svg"),false,"Outputs a graph in SVG format",reportHelp("svg",false,true)},"eog":{report.Dot,invokeDot("svg"),invokeVisualizer("svg",[]string{"eog"}),false,"Visualize graph through eog",reportHelp("eog",false,false)},"evince":{report.Dot,invokeDot("pdf"),invokeVisualizer("pdf",[]string{"evince"}),false,"Visualize graph through evince",reportHelp("evince",false,false)},"gv":{report.Dot,invokeDot("ps"),invokeVisualizer("ps",[]string{"gv --noantialias"}),false,"Visualize graph through gv",reportHelp("gv",false,false)},"web":{report.Dot,massageDotSVG(),invokeVisualizer("svg",browsers()),false,"Visualize graph through web browser",reportHelp("web",false,false)},"kcachegrind":{report.Callgrind,nil,invokeVisualizer("grind",kcachegrind),false,"Visualize report in KCachegrind",reportHelp("kcachegrind",false,false)},"weblist":{report.WebList,nil,invokeVisualizer("html",browsers()),true,"Display annotated source in a web browser",listHelp("weblist",false)},);

// configHelp contains help text per configuration parameter.
private static map configHelp = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"output":helpText("Output filename for file-based outputs"),"drop_negative":helpText("Ignore negative differences","Do not show any locations with values <0."),"call_tree":helpText("Create a context-sensitive call tree","Treat locations reached through different paths as separate."),"relative_percentages":helpText("Show percentages relative to focused subgraph","If unset, percentages are relative to full graph before focusing","to facilitate comparison with original graph."),"unit":helpText("Measurement units to display","Scale the sample values to this unit.","For time-based profiles, use seconds, milliseconds, nanoseconds, etc.","For memory profiles, use megabytes, kilobytes, bytes, etc.","Using auto will scale each value independently to the most natural unit."),"compact_labels":"Show minimal headers","source_path":"Search path for source files","trim_path":"Path to trim from source paths before search","intel_syntax":helpText("Show assembly in Intel syntax","Only applicable to commands `disasm` and `weblist`"),"nodecount":helpText("Max number of nodes to show","Uses heuristics to limit the number of locations to be displayed.","On graphs, dotted edges represent paths through nodes that have been removed."),"nodefraction":"Hide nodes below <f>*total","edgefraction":"Hide edges below <f>*total","trim":helpText("Honor nodefraction/edgefraction/nodecount defaults","Set to false to get the full profile, without any trimming."),"focus":helpText("Restricts to samples going through a node matching regexp","Discard samples that do not include a node matching this regexp.","Matching includes the function name, filename or object name."),"ignore":helpText("Skips paths going through any nodes matching regexp","If set, discard samples that include a node matching this regexp.","Matching includes the function name, filename or object name."),"prune_from":helpText("Drops any functions below the matched frame.","If set, any frames matching the specified regexp and any frames","below it will be dropped from each sample."),"hide":helpText("Skips nodes matching regexp","Discard nodes that match this location.","Other nodes from samples that include this location will be shown.","Matching includes the function name, filename or object name."),"show":helpText("Only show nodes matching regexp","If set, only show nodes that match this location.","Matching includes the function name, filename or object name."),"show_from":helpText("Drops functions above the highest matched frame.","If set, all frames above the highest match are dropped from every sample.","Matching includes the function name, filename or object name."),"tagfocus":helpText("Restricts to samples with tags in range or matched by regexp","Use name=value syntax to limit the matching to a specific tag.","Numeric tag filter examples: 1kb, 1kb:10kb, memory=32mb:","String tag filter examples: foo, foo.*bar, mytag=foo.*bar"),"tagignore":helpText("Discard samples with tags in range or matched by regexp","Use name=value syntax to limit the matching to a specific tag.","Numeric tag filter examples: 1kb, 1kb:10kb, memory=32mb:","String tag filter examples: foo, foo.*bar, mytag=foo.*bar"),"tagshow":helpText("Only consider tags matching this regexp","Discard tags that do not match this regexp"),"taghide":helpText("Skip tags matching this regexp","Discard tags that match this regexp"),"divide_by":helpText("Ratio to divide all samples before visualization","Divide all samples values by a constant, eg the number of processors or jobs."),"mean":helpText("Average sample value over first value (count)","For memory profiles, report average memory per allocation.","For time-based profiles, report average time per event."),"sample_index":helpText("Sample value to report (0-based index or name)","Profiles contain multiple values per sample.","Use sample_index=i to select the ith value (starting at 0)."),"normalize":helpText("Scales profile based on the base profile."),"flat":helpText("Sort entries based on own weight"),"cum":helpText("Sort entries based on cumulative weight"),"functions":helpText("Aggregate at the function level.","Ignores the filename where the function was defined."),"filefunctions":helpText("Aggregate at the function level.","Takes into account the filename where the function was defined."),"files":"Aggregate at the file level.","lines":"Aggregate at the source code line level.","addresses":helpText("Aggregate at the address level.","Includes functions' addresses in the output."),"noinlines":helpText("Ignore inlines.","Attributes inlined functions to their first out-of-line caller."),};

private static @string helpText(params @string[] s) {
    s = s.Clone();

    return strings.Join(s, "\n") + "\n";
}

// usage returns a string describing the pprof commands and configuration
// options.  if commandLine is set, the output reflect cli usage.
private static @string usage(bool commandLine) {
    @string prefix = default;
    if (commandLine) {
        prefix = "-";
    }
    Func<@string, @string, @string> fmtHelp = (c, d) => fmt.Sprintf("    %-16s %s", c, strings.SplitN(d, "\n", 2)[0]);

    slice<@string> commands = default;
    foreach (var (name, cmd) in pprofCommands) {
        commands = append(commands, fmtHelp(prefix + name, cmd.description));
    }    sort.Strings(commands);

    @string help = default;
    if (commandLine) {
        help = "  Output formats (select at most one):\n";
    }
    else
 {
        help = "  Commands:\n";
        commands = append(commands, fmtHelp("o/options", "List options and their current values"));
        commands = append(commands, fmtHelp("q/quit/exit/^D", "Exit pprof"));
    }
    help = help + strings.Join(commands, "\n") + "\n\n" + "  Options:\n"; 

    // Print help for configuration options after sorting them.
    // Collect choices for multi-choice options print them together.
    slice<@string> variables = default;
    slice<@string> radioStrings = default;
    foreach (var (_, f) in configFields) {
        if (len(f.choices) == 0) {
            variables = append(variables, fmtHelp(prefix + f.name, configHelp[f.name]));
            continue;
        }
        @string s = new slice<@string>(new @string[] { fmtHelp(f.name,"") });
        foreach (var (_, choice) in f.choices) {
            s = append(s, "  " + fmtHelp(prefix + choice, configHelp[choice]));
        }        radioStrings = append(radioStrings, strings.Join(s, "\n"));
    }    sort.Strings(variables);
    sort.Strings(radioStrings);
    return help + strings.Join(variables, "\n") + "\n\n" + "  Option groups (only set one per group):\n" + strings.Join(radioStrings, "\n");
}

private static @string reportHelp(@string c, bool cum, bool redirect) {
    @string h = new slice<@string>(new @string[] { c+" [n] [focus_regex]* [-ignore_regex]*", "Include up to n samples", "Include samples matching focus_regex, and exclude ignore_regex." });
    if (cum) {
        h[0] += " [-cum]";
        h = append(h, "-cum sorts the output by cumulative weight");
    }
    if (redirect) {
        h[0] += " >f";
        h = append(h, "Optionally save the report on the file f");
    }
    return strings.Join(h, "\n");
}

private static @string listHelp(@string c, bool redirect) {
    @string h = new slice<@string>(new @string[] { c+"<func_regex|address> [-focus_regex]* [-ignore_regex]*", "Include functions matching func_regex, or including the address specified.", "Include samples matching focus_regex, and exclude ignore_regex." });
    if (redirect) {
        h[0] += " >f";
        h = append(h, "Optionally save the report on the file f");
    }
    return strings.Join(h, "\n");
}

// browsers returns a list of commands to attempt for web visualization.
private static slice<@string> browsers() {
    slice<@string> cmds = default;
    {
        var userBrowser = os.Getenv("BROWSER");

        if (userBrowser != "") {
            cmds = append(cmds, userBrowser);
        }
    }
    switch (runtime.GOOS) {
        case "darwin": 
            cmds = append(cmds, "/usr/bin/open");
            break;
        case "windows": 
            cmds = append(cmds, "cmd /c start");
            break;
        default: 
            // Commands opening browsers are prioritized over xdg-open, so browser()
            // command can be used on linux to open the .svg file generated by the -web
            // command (the .svg file includes embedded javascript so is best viewed in
            // a browser).
            cmds = append(cmds, new slice<@string>(new @string[] { "chrome", "google-chrome", "chromium", "firefox", "sensible-browser" }));
            if (os.Getenv("DISPLAY") != "") { 
                // xdg-open is only for use in a desktop environment.
                cmds = append(cmds, "xdg-open");
            }
            break;
    }
    return cmds;
}

private static @string kcachegrind = new slice<@string>(new @string[] { "kcachegrind" });

// awayFromTTY saves the output in a file if it would otherwise go to
// the terminal screen. This is used to avoid dumping binary data on
// the screen.
private static PostProcessor awayFromTTY(@string format) {
    return (input, output, ui) => {
        if (output == os.Stdout && (ui.IsTerminal() || interactiveMode)) {
            var (tempFile, err) = newTempFile("", "profile", "." + format);
            if (err != null) {
                return err;
            }
            ui.PrintErr("Generating report in ", tempFile.Name());
            output = tempFile;
        }
        var (_, err) = io.Copy(output, input);
        return err;
    };
}

private static PostProcessor invokeDot(@string format) {
    return (input, output, ui) => {
        var cmd = exec.Command("dot", "-T" + format);
        (cmd.Stdin, cmd.Stdout, cmd.Stderr) = (input, output, os.Stderr);        {
            var err = cmd.Run();

            if (err != null) {
                return fmt.Errorf("failed to execute dot. Is Graphviz installed? Error: %v", err);
            }

        }
        return null;
    };
}

// massageDotSVG invokes the dot tool to generate an SVG image and alters
// the image to have panning capabilities when viewed in a browser.
private static PostProcessor massageDotSVG() {
    var generateSVG = invokeDot("svg");
    return (input, output, ui) => {
        ptr<object> baseSVG = @new<bytes.Buffer>();
        {
            var err = generateSVG(input, baseSVG, ui);

            if (err != null) {
                return err;
            }

        }
        var (_, err) = output.Write((slice<byte>)massageSVG(baseSVG.String()));
        return err;
    };
}

private static PostProcessor invokeVisualizer(@string suffix, slice<@string> visualizers) => func((defer, _, _) => {
    return (input, output, ui) => {
        var (tempFile, err) = newTempFile(os.TempDir(), "pprof", "." + suffix);
        if (err != null) {
            return err;
        }
        deferDeleteTempFile(tempFile.Name());
        {
            var (_, err) = io.Copy(tempFile, input);

            if (err != null) {
                return err;
            }

        }
        tempFile.Close(); 
        // Try visualizers until one is successful
        foreach (var (_, v) in visualizers) { 
            // Separate command and arguments for exec.Command.
            var args = strings.Split(v, " ");
            if (len(args) == 0) {
                continue;
            }
            var viewer = exec.Command(args[0], append(args[(int)1..], tempFile.Name()));
            viewer.Stderr = os.Stderr;
            err = viewer.Start();

            if (err == null) { 
                // Wait for a second so that the visualizer has a chance to
                // open the input file. This needs to be done even if we're
                // waiting for the visualizer as it can be just a wrapper that
                // spawns a browser tab and returns right away.
                defer(t => {
                    t.Receive();
                }(time.After(time.Second))); 
                // On interactive mode, let the visualizer run in the background
                // so other commands can be issued.
                if (!interactiveMode) {
                    return viewer.Wait();
                }
                return null;
            }
        }        return err;
    };
});

// stringToBool is a custom parser for bools. We avoid using strconv.ParseBool
// to remain compatible with old pprof behavior (e.g., treating "" as true).
private static (bool, error) stringToBool(@string s) {
    bool _p0 = default;
    error _p0 = default!;

    switch (strings.ToLower(s)) {
        case "true": 

        case "t": 

        case "yes": 

        case "y": 

        case "1": 

        case "": 
            return (true, error.As(null!)!);
            break;
        case "false": 

        case "f": 

        case "no": 

        case "n": 

        case "0": 
            return (false, error.As(null!)!);
            break;
        default: 
            return (false, error.As(fmt.Errorf("illegal value \"%s\" for bool variable", s))!);
            break;
    }
}

} // end driver_package
