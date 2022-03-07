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

// Package driver implements the core pprof functionality. It can be
// parameterized with a flag implementation, fetch and symbolize
// mechanisms.
// package driver -- go2cs converted at 2022 March 06 23:23:23 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\driver.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using report = go.github.com.google.pprof.@internal.report_package;
using profile = go.github.com.google.pprof.profile_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class driver_package {

    // PProf acquires a profile, and symbolizes it using a profile
    // manager. Then it generates a report formatted according to the
    // options selected through the flags package.
public static error PProf(ptr<plugin.Options> _addr_eo) => func((defer, _, _) => {
    ref plugin.Options eo = ref _addr_eo.val;
 
    // Remove any temporary files created during pprof processing.
    defer(cleanupTempFiles());

    var o = setDefaults(eo);

    var (src, cmd, err) = parseFlags(o);
    if (err != null) {
        return error.As(err)!;
    }
    var (p, err) = fetchProfiles(src, o);
    if (err != null) {
        return error.As(err)!;
    }
    if (cmd != null) {
        return error.As(generateReport(_addr_p, cmd, currentConfig(), _addr_o))!;
    }
    if (src.HTTPHostport != "") {
        return error.As(serveWebInterface(src.HTTPHostport, p, o, src.HTTPDisableBrowser))!;
    }
    return error.As(interactive(p, o))!;

});

private static (ptr<command>, ptr<report.Report>, error) generateRawReport(ptr<profile.Profile> _addr_p, slice<@string> cmd, config cfg, ptr<plugin.Options> _addr_o) => func((_, panic, _) => {
    ptr<command> _p0 = default!;
    ptr<report.Report> _p0 = default!;
    error _p0 = default!;
    ref profile.Profile p = ref _addr_p.val;
    ref plugin.Options o = ref _addr_o.val;

    p = p.Copy(); // Prevent modification to the incoming profile.

    // Identify units of numeric tags in profile.
    var numLabelUnits = identifyNumLabelUnits(_addr_p, o.UI); 

    // Get report output format
    var c = pprofCommands[cmd[0]];
    if (c == null) {
        panic("unexpected nil command");
    }
    cfg = applyCommandOverrides(cmd[0], c.format, cfg); 

    // Delay focus after configuring report to get percentages on all samples.
    var relative = cfg.RelativePercentages;
    if (relative) {
        {
            var err__prev2 = err;

            var err = applyFocus(p, numLabelUnits, cfg, o.UI);

            if (err != null) {
                return (_addr_null!, _addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

    }
    var (ropt, err) = reportOptions(_addr_p, numLabelUnits, cfg);
    if (err != null) {
        return (_addr_null!, _addr_null!, error.As(err)!);
    }
    ropt.OutputFormat = c.format;
    if (len(cmd) == 2) {
        var (s, err) = regexp.Compile(cmd[1]);
        if (err != null) {
            return (_addr_null!, _addr_null!, error.As(fmt.Errorf("parsing argument regexp %s: %v", cmd[1], err))!);
        }
        ropt.Symbol = s;

    }
    var rpt = report.New(p, ropt);
    if (!relative) {
        {
            var err__prev2 = err;

            err = applyFocus(p, numLabelUnits, cfg, o.UI);

            if (err != null) {
                return (_addr_null!, _addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

    }
    {
        var err__prev1 = err;

        err = aggregate(_addr_p, cfg);

        if (err != null) {
            return (_addr_null!, _addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    return (_addr_c!, _addr_rpt!, error.As(null!)!);

});

private static error generateReport(ptr<profile.Profile> _addr_p, slice<@string> cmd, config cfg, ptr<plugin.Options> _addr_o) {
    ref profile.Profile p = ref _addr_p.val;
    ref plugin.Options o = ref _addr_o.val;

    var (c, rpt, err) = generateRawReport(_addr_p, cmd, cfg, _addr_o);
    if (err != null) {
        return error.As(err)!;
    }
    ptr<object> dst = @new<bytes.Buffer>();
    {
        var err__prev1 = err;

        var err = report.Generate(dst, rpt, o.Obj);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var src = dst; 

    // If necessary, perform any data post-processing.
    if (c.postProcess != null) {
        dst = @new<bytes.Buffer>();
        {
            var err__prev2 = err;

            err = c.postProcess(src, dst, o.UI);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }

        src = dst;

    }
    var output = cfg.Output;
    if (output == "") {
        if (c.visualizer != null) {
            return error.As(c.visualizer(src, os.Stdout, o.UI))!;
        }
        var (_, err) = src.WriteTo(os.Stdout);
        return error.As(err)!;

    }
    o.UI.PrintErr("Generating report in ", output);
    var (out, err) = o.Writer.Open(output);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err__prev1 = err;

        (_, err) = src.WriteTo(out);

        if (err != null) {
            @out.Close();
            return error.As(err)!;
        }
        err = err__prev1;

    }

    return error.As(@out.Close())!;

}

private static config applyCommandOverrides(@string cmd, nint outputFormat, config cfg) { 
    // Some report types override the trim flag to false below. This is to make
    // sure the default heuristics of excluding insignificant nodes and edges
    // from the call graph do not apply. One example where it is important is
    // annotated source or disassembly listing. Those reports run on a specific
    // function (or functions), but the trimming is applied before the function
    // data is selected. So, with trimming enabled, the report could end up
    // showing no data if the specified function is "uninteresting" as far as the
    // trimming is concerned.
    var trim = cfg.Trim;

    switch (cmd) {
        case "disasm": 
            trim = false;
            cfg.Granularity = "addresses"; 
            // Force the 'noinlines' mode so that source locations for a given address
            // collapse and there is only one for the given address. Without this
            // cumulative metrics would be double-counted when annotating the assembly.
            // This is because the merge is done by address and in case of an inlined
            // stack each of the inlined entries is a separate callgraph node.
            cfg.NoInlines = true;

            break;
        case "weblist": 
            trim = false;
            cfg.Granularity = "addresses";
            cfg.NoInlines = false; // Need inline info to support call expansion
            break;
        case "peek": 
            trim = false;
            break;
        case "list": 
            trim = false;
            cfg.Granularity = "lines"; 
            // Do not force 'noinlines' to be false so that specifying
            // "-list foo -noinlines" is supported and works as expected.
            break;
        case "text": 

        case "top": 

        case "topproto": 
            if (cfg.NodeCount == -1) {
                cfg.NodeCount = 0;
            }
            break;
        default: 
            if (cfg.NodeCount == -1) {
                cfg.NodeCount = 80;
            }
            break;
    }


    if (outputFormat == report.Proto || outputFormat == report.Raw || outputFormat == report.Callgrind) 
        trim = false;
        cfg.Granularity = "addresses";
        cfg.NoInlines = false;
        if (!trim) {
        cfg.NodeCount = 0;
        cfg.NodeFraction = 0;
        cfg.EdgeFraction = 0;
    }
    return cfg;

}

private static error aggregate(ptr<profile.Profile> _addr_prof, config cfg) {
    ref profile.Profile prof = ref _addr_prof.val;

    bool function = default;    bool filename = default;    bool linenumber = default;    bool address = default;

    var inlines = !cfg.NoInlines;
    switch (cfg.Granularity) {
        case "addresses": 
            if (inlines) {
                return error.As(null!)!;
            }
            function = true;
            filename = true;
            linenumber = true;
            address = true;

            break;
        case "lines": 
            function = true;
            filename = true;
            linenumber = true;
            break;
        case "files": 
            filename = true;
            break;
        case "functions": 
            function = true;
            break;
        case "filefunctions": 
            function = true;
            filename = true;
            break;
        default: 
            return error.As(fmt.Errorf("unexpected granularity"))!;
            break;
    }
    return error.As(prof.Aggregate(inlines, function, filename, linenumber, address))!;

}

private static (ptr<report.Options>, error) reportOptions(ptr<profile.Profile> _addr_p, map<@string, @string> numLabelUnits, config cfg) {
    ptr<report.Options> _p0 = default!;
    error _p0 = default!;
    ref profile.Profile p = ref _addr_p.val;

    var si = cfg.SampleIndex;
    var mean = cfg.Mean;
    var (value, meanDiv, sample, err) = sampleFormat(_addr_p, si, mean);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var stype = sample.Type;
    if (mean) {
        stype = "mean_" + stype;
    }
    if (cfg.DivideBy == 0) {
        return (_addr_null!, error.As(fmt.Errorf("zero divisor specified"))!);
    }
    slice<@string> filters = default;
    Action<@string, @string> addFilter = (k, v) => {
        if (v != "") {
            filters = append(filters, k + "=" + v);
        }
    };
    addFilter("focus", cfg.Focus);
    addFilter("ignore", cfg.Ignore);
    addFilter("hide", cfg.Hide);
    addFilter("show", cfg.Show);
    addFilter("show_from", cfg.ShowFrom);
    addFilter("tagfocus", cfg.TagFocus);
    addFilter("tagignore", cfg.TagIgnore);
    addFilter("tagshow", cfg.TagShow);
    addFilter("taghide", cfg.TagHide);

    ptr<report.Options> ropt = addr(new report.Options(CumSort:cfg.Sort=="cum",CallTree:cfg.CallTree,DropNegative:cfg.DropNegative,CompactLabels:cfg.CompactLabels,Ratio:1/cfg.DivideBy,NodeCount:cfg.NodeCount,NodeFraction:cfg.NodeFraction,EdgeFraction:cfg.EdgeFraction,ActiveFilters:filters,NumLabelUnits:numLabelUnits,SampleValue:value,SampleMeanDivisor:meanDiv,SampleType:stype,SampleUnit:sample.Unit,OutputUnit:cfg.Unit,SourcePath:cfg.SourcePath,TrimPath:cfg.TrimPath,IntelSyntax:cfg.IntelSyntax,));

    if (len(p.Mapping) > 0 && p.Mapping[0].File != "") {
        ropt.Title = filepath.Base(p.Mapping[0].File);
    }
    return (_addr_ropt!, error.As(null!)!);

}

// identifyNumLabelUnits returns a map of numeric label keys to the units
// associated with those keys.
private static map<@string, @string> identifyNumLabelUnits(ptr<profile.Profile> _addr_p, plugin.UI ui) {
    ref profile.Profile p = ref _addr_p.val;

    var (numLabelUnits, ignoredUnits) = p.NumLabelUnits(); 

    // Print errors for tags with multiple units associated with
    // a single key.
    foreach (var (k, units) in ignoredUnits) {
        ui.PrintErr(fmt.Sprintf("For tag %s used unit %s, also encountered unit(s) %s", k, numLabelUnits[k], strings.Join(units, ", ")));
    }    return numLabelUnits;

}

public delegate  long sampleValueFunc(slice<long>);

// sampleFormat returns a function to extract values out of a profile.Sample,
// and the type/units of those values.
private static (sampleValueFunc, sampleValueFunc, ptr<profile.ValueType>, error) sampleFormat(ptr<profile.Profile> _addr_p, @string sampleIndex, bool mean) {
    sampleValueFunc value = default;
    sampleValueFunc meanDiv = default;
    ptr<profile.ValueType> v = default!;
    error err = default!;
    ref profile.Profile p = ref _addr_p.val;

    if (len(p.SampleType) == 0) {
        return (null, null, _addr_null!, error.As(fmt.Errorf("profile has no samples"))!);
    }
    var (index, err) = p.SampleIndexByName(sampleIndex);
    if (err != null) {
        return (null, null, _addr_null!, error.As(err)!);
    }
    value = valueExtractor(index);
    if (mean) {
        meanDiv = valueExtractor(0);
    }
    v = p.SampleType[index];
    return ;

}

private static sampleValueFunc valueExtractor(nint ix) {
    return v => {
        return v[ix];
    };
}

} // end driver_package
