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

// package driver -- go2cs converted at 2022 March 06 23:23:20 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\cli.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;

using binutils = go.github.com.google.pprof.@internal.binutils_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class driver_package {

private partial struct source {
    public slice<@string> Sources;
    public @string ExecName;
    public @string BuildID;
    public slice<@string> Base;
    public bool DiffBase;
    public bool Normalize;
    public nint Seconds;
    public nint Timeout;
    public @string Symbolize;
    public @string HTTPHostport;
    public bool HTTPDisableBrowser;
    public @string Comment;
}

// parseFlags parses the command lines through the specified flags package
// and returns the source of the profile and optionally the command
// for the kind of report to generate (nil for interactive use).
private static (ptr<source>, slice<@string>, error) parseFlags(ptr<plugin.Options> _addr_o) {
    ptr<source> _p0 = default!;
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref plugin.Options o = ref _addr_o.val;

    var flag = o.Flagset; 
    // Comparisons.
    var flagDiffBase = flag.StringList("diff_base", "", "Source of base profile for comparison");
    var flagBase = flag.StringList("base", "", "Source of base profile for profile subtraction"); 
    // Source options.
    var flagSymbolize = flag.String("symbolize", "", "Options for profile symbolization");
    var flagBuildID = flag.String("buildid", "", "Override build id for first mapping");
    var flagTimeout = flag.Int("timeout", -1, "Timeout in seconds for fetching a profile");
    var flagAddComment = flag.String("add_comment", "", "Annotation string to record in the profile"); 
    // CPU profile options
    var flagSeconds = flag.Int("seconds", -1, "Length of time for dynamic profiles"); 
    // Heap profile options
    var flagInUseSpace = flag.Bool("inuse_space", false, "Display in-use memory size");
    var flagInUseObjects = flag.Bool("inuse_objects", false, "Display in-use object counts");
    var flagAllocSpace = flag.Bool("alloc_space", false, "Display allocated memory size");
    var flagAllocObjects = flag.Bool("alloc_objects", false, "Display allocated object counts"); 
    // Contention profile options
    var flagTotalDelay = flag.Bool("total_delay", false, "Display total delay at each region");
    var flagContentions = flag.Bool("contentions", false, "Display number of delays at each region");
    var flagMeanDelay = flag.Bool("mean_delay", false, "Display mean delay at each region");
    var flagTools = flag.String("tools", os.Getenv("PPROF_TOOLS"), "Path for object tool pathnames");

    var flagHTTP = flag.String("http", "", "Present interactive web UI at the specified http host:port");
    var flagNoBrowser = flag.Bool("no_browser", false, "Skip opening a browswer for the interactive web UI"); 

    // Flags that set configuration properties.
    ref var cfg = ref heap(currentConfig(), out ptr<var> _addr_cfg);
    var configFlagSetter = installConfigFlags(flag, _addr_cfg);

    var flagCommands = make_map<@string, ptr<bool>>();
    var flagParamCommands = make_map<@string, ptr<@string>>();
    {
        var cmd__prev1 = cmd;

        foreach (var (__name, __cmd) in pprofCommands) {
            name = __name;
            cmd = __cmd;
            if (cmd.hasParam) {
                flagParamCommands[name] = flag.String(name, "", "Generate a report in " + name + " format, matching regexp");
            }
            else
 {
                flagCommands[name] = flag.Bool(name, false, "Generate a report in " + name + " format");
            }

        }
        cmd = cmd__prev1;
    }

    var args = flag.Parse(() => {
        o.UI.Print(usageMsgHdr + usage(true) + usageMsgSrc + flag.ExtraUsage() + usageMsgVars);
    });
    if (len(args) == 0) {
        return (_addr_null!, null, error.As(errors.New("no profile source specified"))!);
    }
    @string execName = default; 
    // Recognize first argument as an executable or buildid override.
    if (len(args) > 1) {
        var arg0 = args[0];
        {
            var err__prev2 = err;

            var (file, err) = o.Obj.Open(arg0, 0, ~uint64(0), 0);

            if (err == null) {
                file.Close();
                execName = arg0;
                args = args[(int)1..];
            }
            else if (flagBuildID == "" && isBuildID(arg0).val) {
                flagBuildID.val = arg0;
                args = args[(int)1..];
            }


            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        var err = configFlagSetter();

        if (err != null) {
            return (_addr_null!, null, error.As(err)!);
        }
        err = err__prev1;

    }


    var (cmd, err) = outputFormat(flagCommands, flagParamCommands);
    if (err != null) {
        return (_addr_null!, null, error.As(err)!);
    }
    if (cmd != null && flagHTTP != "".val) {
        return (_addr_null!, null, error.As(errors.New("-http is not compatible with an output format on the command line"))!);
    }
    if (flagNoBrowser && flagHTTP == "".val) {
        return (_addr_null!, null, error.As(errors.New("-no_browser only makes sense with -http"))!);
    }
    var si = cfg.SampleIndex;
    si = sampleIndex(_addr_flagTotalDelay, si, "delay", "-total_delay", o.UI);
    si = sampleIndex(_addr_flagMeanDelay, si, "delay", "-mean_delay", o.UI);
    si = sampleIndex(_addr_flagContentions, si, "contentions", "-contentions", o.UI);
    si = sampleIndex(_addr_flagInUseSpace, si, "inuse_space", "-inuse_space", o.UI);
    si = sampleIndex(_addr_flagInUseObjects, si, "inuse_objects", "-inuse_objects", o.UI);
    si = sampleIndex(_addr_flagAllocSpace, si, "alloc_space", "-alloc_space", o.UI);
    si = sampleIndex(_addr_flagAllocObjects, si, "alloc_objects", "-alloc_objects", o.UI);
    cfg.SampleIndex = si;

    if (flagMeanDelay.val) {
        cfg.Mean = true;
    }
    ptr<source> source = addr(new source(Sources:args,ExecName:execName,BuildID:*flagBuildID,Seconds:*flagSeconds,Timeout:*flagTimeout,Symbolize:*flagSymbolize,HTTPHostport:*flagHTTP,HTTPDisableBrowser:*flagNoBrowser,Comment:*flagAddComment,));

    {
        var err__prev1 = err;

        err = source.addBaseProfiles(flagBase.val, flagDiffBase.val);

        if (err != null) {
            return (_addr_null!, null, error.As(err)!);
        }
        err = err__prev1;

    }


    var normalize = cfg.Normalize;
    if (normalize && len(source.Base) == 0) {
        return (_addr_null!, null, error.As(errors.New("must have base profile to normalize by"))!);
    }
    source.Normalize = normalize;

    {
        ptr<binutils.Binutils> (bu, ok) = o.Obj._<ptr<binutils.Binutils>>();

        if (ok) {
            bu.SetTools(flagTools.val);
        }
    }


    setCurrentConfig(cfg);
    return (_addr_source!, cmd, error.As(null!)!);

}

// addBaseProfiles adds the list of base profiles or diff base profiles to
// the source. This function will return an error if both base and diff base
// profiles are specified.
private static error addBaseProfiles(this ptr<source> _addr_source, slice<ptr<@string>> flagBase, slice<ptr<@string>> flagDiffBase) {
    ref source source = ref _addr_source.val;

    var @base = dropEmpty(flagBase);
    var diffBase = dropEmpty(flagDiffBase);
    if (len(base) > 0 && len(diffBase) > 0) {
        return error.As(errors.New("-base and -diff_base flags cannot both be specified"))!;
    }
    source.Base = base;
    if (len(diffBase) > 0) {
        (source.Base, source.DiffBase) = (diffBase, true);
    }
    return error.As(null!)!;

}

// dropEmpty list takes a slice of string pointers, and outputs a slice of
// non-empty strings associated with the flag.
private static slice<@string> dropEmpty(slice<ptr<@string>> list) {
    slice<@string> l = default;
    foreach (var (_, s) in list) {
        if (s != "".val) {
            l = append(l, s.val);
        }
    }    return l;

}

// installConfigFlags creates command line flags for configuration
// fields and returns a function which can be called after flags have
// been parsed to copy any flags specified on the command line to
// *cfg.
private static Func<error> installConfigFlags(plugin.FlagSet flag, ptr<config> _addr_cfg) {
    ref config cfg = ref _addr_cfg.val;
 
    // List of functions for setting the different parts of a config.
    slice<Action> setters = default;
    error err = default!; // Holds any errors encountered while running setters.

    foreach (var (_, field) in configFields) {
        var n = field.name;
        var help = configHelp[n];
        Action setter = default;
        switch (cfg.fieldPtr(field).type()) {
            case ptr<bool> ptr:
                var f = flag.Bool(n, ptr.val, help);
                setter = () => {
                    ptr.val = f.val;
                }
;
                break;
            case ptr<nint> ptr:
                f = flag.Int(n, ptr.val, help);
                setter = () => {
                    ptr.val = f.val;
                }
;
                break;
            case ptr<double> ptr:
                f = flag.Float64(n, ptr.val, help);
                setter = () => {
                    ptr.val = f.val;
                }
;
                break;
            case ptr<@string> ptr:
                if (len(field.choices) == 0) {
                    f = flag.String(n, ptr.val, help);
                    setter = () => {
                        ptr.val = f.val;
                    }
                else
;

                } { 
                    // Make a separate flag per possible choice.
                    // Set all flags to initially false so we can
                    // identify conflicts.
                    var bools = make_map<@string, ptr<bool>>();
                    foreach (var (_, choice) in field.choices) {
                        bools[choice] = flag.Bool(choice, false, configHelp[choice]);
                    }
                    setter = () => {
                        slice<@string> set = default;
                        foreach (var (k, v) in bools) {
                            if (v.val) {
                                set = append(set, k);
                            }
                        }
                        switch (len(set)) {
                            case 0: 

                                break;
                            case 1: 
                                ptr.val = set[0];
                                break;
                            default: 
                                err = error.As(fmt.Errorf("conflicting options set: %v", set))!;
                                break;
                        }

                    }
;

                }

                break;
        }
        setters = append(setters, setter);

    }    return () => { 
        // Apply the setter for every flag.
        {
            Action setter__prev1 = setter;

            foreach (var (_, __setter) in setters) {
                setter = __setter;
                setter();
                if (err != null) {
                    return err;
                }
            }

            setter = setter__prev1;
        }

        return null;

    };

}

// isBuildID determines if the profile may contain a build ID, by
// checking that it is a string of hex digits.
private static bool isBuildID(@string id) {
    return strings.Trim(id, "0123456789abcdefABCDEF") == "";
}

private static @string sampleIndex(ptr<bool> _addr_flag, @string si, @string sampleType, @string option, plugin.UI ui) {
    ref bool flag = ref _addr_flag.val;

    if (flag) {
        if (si == "") {
            return sampleType;
        }
        ui.PrintErr("Multiple value selections, ignoring ", option);

    }
    return si;

}

private static (slice<@string>, error) outputFormat(map<@string, ptr<bool>> bcmd, map<@string, ptr<@string>> acmd) {
    slice<@string> cmd = default;
    error err = default!;

    {
        var n__prev1 = n;

        foreach (var (__n, __b) in bcmd) {
            n = __n;
            b = __b;
            if (b.val) {
                if (cmd != null) {
                    return (null, error.As(errors.New("must set at most one output format"))!);
                }
                cmd = new slice<@string>(new @string[] { n });
            }
        }
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (__n, __s) in acmd) {
            n = __n;
            s = __s;
            if (s != "".val) {
                if (cmd != null) {
                    return (null, error.As(errors.New("must set at most one output format"))!);
                }
                cmd = new slice<@string>(new @string[] { n, *s });
            }
        }
        n = n__prev1;
    }

    return (cmd, error.As(null!)!);

}

private static @string usageMsgHdr = @"usage:

Produce output in the specified format.

   pprof <format> [options] [binary] <source> ...

Omit the format to get an interactive shell whose commands can be used
to generate various views of a profile

   pprof [options] [binary] <source> ...

Omit the format and provide the ""-http"" flag to get an interactive web
interface at the specified host:port that can be used to navigate through
various views of a profile.

   pprof -http [host]:[port] [options] [binary] <source> ...

Details:
";

private static @string usageMsgSrc = "\n\n" + "  Source options:\n" + "    -seconds              Duration for time-based profile collection\n" + "    -timeout              Timeout in seconds for profile collection\n" + "    -buildid              Override build id for main binary\n" + "    -add_comment          Free-form annotation to add to the profile\n" + "                          Displayed on some reports or with pprof -comments\n" + "    -diff_base source     Source of base profile for comparison\n" + "    -base source          Source of base profile for profile subtraction\n" + "    profile.pb.gz         Profile in compressed protobuf format\n" + "    legacy_profile        Profile in legacy pprof format\n" + "    http://host/profile   URL for profile handler to retrieve\n" + "    -symbolize=           Controls source of symbol information\n" + "      none                  Do not attempt symbolization\n" + "      local                 Examine only local binaries\n" + "      fastlocal             Only get function names from local binaries\n" + "      remote                Do not examine local binaries\n" + "      force                 Force re-symbolization\n" + "    Binary                  Local path or build id of binary for symbolization\n";

private static @string usageMsgVars = "\n\n" + "  Misc options:\n" + "   -http              Provide web interface at host:port.\n" + "                      Host is optional and 'localhost' by default.\n" + "                      Port is optional and a randomly available port by default.\n" + "   -no_browser        Skip opening a browser for the interactive web UI.\n" + "   -tools             Search path for object tools\n" + "\n" + "  Legacy convenience options:\n" + "   -inuse_space           Same as -sample_index=inuse_space\n" + "   -inuse_objects         Same as -sample_index=inuse_objects\n" + "   -alloc_space           Same as -sample_index=alloc_space\n" + "   -alloc_objects         Same as -sample_index=alloc_objects\n" + "   -total_delay           Same as -sample_index=delay\n" + "   -contentions           Same as -sample_index=contentions\n" + "   -mean_delay            Same as -mean -sample_index=delay\n" + "\n" + "  Environment Variables:\n" + "   PPROF_TMPDIR       Location for saved profiles (default $HOME/pprof)\n" + "   PPROF_TOOLS        Search path for object-level tools\n" + "   PPROF_BINARY_PATH  Search path for local binary files\n" + "                      default: $HOME/pprof/binaries\n" + "                      searches $name, $path, $buildid/$name, $path/$buildid\n" + "   * On Windows, %USERPROFILE% is used instead of $HOME";

} // end driver_package
