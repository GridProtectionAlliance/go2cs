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
// package driver -- go2cs converted at 2020 August 29 10:05:19 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\driver.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
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
        // PProf acquires a profile, and symbolizes it using a profile
        // manager. Then it generates a report formatted according to the
        // options selected through the flags package.
        public static error PProf(ref plugin.Options _eo) => func(_eo, (ref plugin.Options eo, Defer defer, Panic _, Recover __) =>
        { 
            // Remove any temporary files created during pprof processing.
            defer(cleanupTempFiles());

            var o = setDefaults(eo);

            var (src, cmd, err) = parseFlags(o);
            if (err != null)
            {
                return error.As(err);
            }
            var (p, err) = fetchProfiles(src, o);
            if (err != null)
            {
                return error.As(err);
            }
            if (cmd != null)
            {
                return error.As(generateReport(p, cmd, pprofVariables, o));
            }
            if (src.HTTPHostport != "")
            {
                return error.As(serveWebInterface(src.HTTPHostport, p, o));
            }
            return error.As(interactive(p, o));
        });

        private static (ref command, ref report.Report, error) generateRawReport(ref profile.Profile _p, slice<@string> cmd, variables vars, ref plugin.Options _o) => func(_p, _o, (ref profile.Profile p, ref plugin.Options o, Defer _, Panic panic, Recover __) =>
        {
            p = p.Copy(); // Prevent modification to the incoming profile.

            // Identify units of numeric tags in profile.
            var numLabelUnits = identifyNumLabelUnits(p, o.UI);

            vars = applyCommandOverrides(cmd, vars); 

            // Delay focus after configuring report to get percentages on all samples.
            var relative = vars["relative_percentages"].boolValue();
            if (relative)
            {
                {
                    var err__prev2 = err;

                    var err = applyFocus(p, numLabelUnits, vars, o.UI);

                    if (err != null)
                    {
                        return (null, null, err);
                    }

                    err = err__prev2;

                }
            }
            var (ropt, err) = reportOptions(p, numLabelUnits, vars);
            if (err != null)
            {
                return (null, null, err);
            }
            var c = pprofCommands[cmd[0L]];
            if (c == null)
            {
                panic("unexpected nil command");
            }
            ropt.OutputFormat = c.format;
            if (len(cmd) == 2L)
            {
                var (s, err) = regexp.Compile(cmd[1L]);
                if (err != null)
                {
                    return (null, null, fmt.Errorf("parsing argument regexp %s: %v", cmd[1L], err));
                }
                ropt.Symbol = s;
            }
            var rpt = report.New(p, ropt);
            if (!relative)
            {
                {
                    var err__prev2 = err;

                    err = applyFocus(p, numLabelUnits, vars, o.UI);

                    if (err != null)
                    {
                        return (null, null, err);
                    }

                    err = err__prev2;

                }
            }
            {
                var err__prev1 = err;

                err = aggregate(p, vars);

                if (err != null)
                {
                    return (null, null, err);
                }

                err = err__prev1;

            }

            return (c, rpt, null);
        });

        private static error generateReport(ref profile.Profile p, slice<@string> cmd, variables vars, ref plugin.Options o)
        {
            var (c, rpt, err) = generateRawReport(p, cmd, vars, o);
            if (err != null)
            {
                return error.As(err);
            } 

            // Generate the report.
            ptr<object> dst = @new<bytes.Buffer>();
            {
                var err__prev1 = err;

                var err = report.Generate(dst, rpt, o.Obj);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var src = dst; 

            // If necessary, perform any data post-processing.
            if (c.postProcess != null)
            {
                dst = @new<bytes.Buffer>();
                {
                    var err__prev2 = err;

                    err = c.postProcess(src, dst, o.UI);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                src = dst;
            } 

            // If no output is specified, use default visualizer.
            var output = vars["output"].value;
            if (output == "")
            {
                if (c.visualizer != null)
                {
                    return error.As(c.visualizer(src, os.Stdout, o.UI));
                }
                var (_, err) = src.WriteTo(os.Stdout);
                return error.As(err);
            } 

            // Output to specified file.
            o.UI.PrintErr("Generating report in ", output);
            var (out, err) = os.Create(output);
            if (err != null)
            {
                return error.As(err);
            }
            {
                var err__prev1 = err;

                (_, err) = src.WriteTo(out);

                if (err != null)
                {
                    @out.Close();
                    return error.As(err);
                }

                err = err__prev1;

            }
            return error.As(@out.Close());
        }

        private static variables applyCommandOverrides(slice<@string> cmd, variables v)
        {
            var trim = v["trim"].boolValue();
            var focus = true;
            var tagfocus = true;
            var hide = true;

            switch (cmd[0L])
            {
                case "proto": 

                case "raw": 
                    trim = false;
                    focus = false;
                    tagfocus = false;
                    hide = false;
                    v.set("addresses", "t");
                    break;
                case "callgrind": 

                case "kcachegrind": 
                    trim = false;
                    v.set("addresses", "t");
                    break;
                case "disasm": 

                case "weblist": 
                    trim = false;
                    v.set("addressnoinlines", "t");
                    break;
                case "peek": 
                    trim = false;
                    focus = false;
                    hide = false;
                    break;
                case "list": 
                    v.set("nodecount", "0");
                    v.set("lines", "t");
                    break;
                case "text": 

                case "top": 

                case "topproto": 
                    if (v["nodecount"].intValue() == -1L)
                    {
                        v.set("nodecount", "0");
                    }
                    break;
                default: 
                    if (v["nodecount"].intValue() == -1L)
                    {
                        v.set("nodecount", "80");
                    }
                    break;
            }
            if (!trim)
            {
                v.set("nodecount", "0");
                v.set("nodefraction", "0");
                v.set("edgefraction", "0");
            }
            if (!focus)
            {
                v.set("focus", "");
                v.set("ignore", "");
            }
            if (!tagfocus)
            {
                v.set("tagfocus", "");
                v.set("tagignore", "");
            }
            if (!hide)
            {
                v.set("hide", "");
                v.set("show", "");
            }
            return v;
        }

        private static error aggregate(ref profile.Profile prof, variables v)
        {
            bool inlines = default;            bool function = default;            bool filename = default;            bool linenumber = default;            bool address = default;


            if (v["addresses"].boolValue()) 
                return error.As(null);
            else if (v["lines"].boolValue()) 
                inlines = true;
                function = true;
                filename = true;
                linenumber = true;
            else if (v["files"].boolValue()) 
                inlines = true;
                filename = true;
            else if (v["functions"].boolValue()) 
                inlines = true;
                function = true;
            else if (v["noinlines"].boolValue()) 
                function = true;
            else if (v["addressnoinlines"].boolValue()) 
                function = true;
                filename = true;
                linenumber = true;
                address = true;
            else 
                return error.As(fmt.Errorf("unexpected granularity"));
                        return error.As(prof.Aggregate(inlines, function, filename, linenumber, address));
        }

        private static (ref report.Options, error) reportOptions(ref profile.Profile p, map<@string, @string> numLabelUnits, variables vars)
        {
            var si = vars["sample_index"].value;
            var mean = vars["mean"].boolValue();
            var (value, meanDiv, sample, err) = sampleFormat(p, si, mean);
            if (err != null)
            {
                return (null, err);
            }
            var stype = sample.Type;
            if (mean)
            {
                stype = "mean_" + stype;
            }
            if (vars["divide_by"].floatValue() == 0L)
            {
                return (null, fmt.Errorf("zero divisor specified"));
            }
            slice<@string> filters = default;
            foreach (var (_, k) in new slice<@string>(new @string[] { "focus", "ignore", "hide", "show", "tagfocus", "tagignore", "tagshow", "taghide" }))
            {
                var v = vars[k].value;
                if (v != "")
                {
                    filters = append(filters, k + "=" + v);
                }
            }
            report.Options ropt = ref new report.Options(CumSort:vars["cum"].boolValue(),CallTree:vars["call_tree"].boolValue(),DropNegative:vars["drop_negative"].boolValue(),PositivePercentages:vars["positive_percentages"].boolValue(),CompactLabels:vars["compact_labels"].boolValue(),Ratio:1/vars["divide_by"].floatValue(),NodeCount:vars["nodecount"].intValue(),NodeFraction:vars["nodefraction"].floatValue(),EdgeFraction:vars["edgefraction"].floatValue(),ActiveFilters:filters,NumLabelUnits:numLabelUnits,SampleValue:value,SampleMeanDivisor:meanDiv,SampleType:stype,SampleUnit:sample.Unit,OutputUnit:vars["unit"].value,SourcePath:vars["source_path"].stringValue(),);

            if (len(p.Mapping) > 0L && p.Mapping[0L].File != "")
            {
                ropt.Title = filepath.Base(p.Mapping[0L].File);
            }
            return (ropt, null);
        }

        // identifyNumLabelUnits returns a map of numeric label keys to the units
        // associated with those keys.
        private static map<@string, @string> identifyNumLabelUnits(ref profile.Profile p, plugin.UI ui)
        {
            var (numLabelUnits, ignoredUnits) = p.NumLabelUnits(); 

            // Print errors for tags with multiple units associated with
            // a single key.
            foreach (var (k, units) in ignoredUnits)
            {
                ui.PrintErr(fmt.Sprintf("For tag %s used unit %s, also encountered unit(s) %s", k, numLabelUnits[k], strings.Join(units, ", ")));
            }
            return numLabelUnits;
        }

        public delegate  long sampleValueFunc(slice<long>);

        // sampleFormat returns a function to extract values out of a profile.Sample,
        // and the type/units of those values.
        private static (sampleValueFunc, sampleValueFunc, ref profile.ValueType, error) sampleFormat(ref profile.Profile p, @string sampleIndex, bool mean)
        {
            if (len(p.SampleType) == 0L)
            {
                return (null, null, null, fmt.Errorf("profile has no samples"));
            }
            var (index, err) = p.SampleIndexByName(sampleIndex);
            if (err != null)
            {
                return (null, null, null, err);
            }
            value = valueExtractor(index);
            if (mean)
            {
                meanDiv = valueExtractor(0L);
            }
            v = p.SampleType[index];
            return;
        }

        private static sampleValueFunc valueExtractor(long ix)
        {
            return v =>
            {
                return v[ix];
            }
;
        }
    }
}}}}}}}
