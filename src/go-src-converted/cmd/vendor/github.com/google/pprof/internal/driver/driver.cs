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
// package driver -- go2cs converted at 2020 October 09 05:53:26 UTC
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
        public static error PProf(ptr<plugin.Options> _addr_eo) => func((defer, _, __) =>
        {
            ref plugin.Options eo = ref _addr_eo.val;
 
            // Remove any temporary files created during pprof processing.
            defer(cleanupTempFiles());

            var o = setDefaults(eo);

            var (src, cmd, err) = parseFlags(o);
            if (err != null)
            {
                return error.As(err)!;
            }
            var (p, err) = fetchProfiles(src, o);
            if (err != null)
            {
                return error.As(err)!;
            }
            if (cmd != null)
            {
                return error.As(generateReport(_addr_p, cmd, pprofVariables, _addr_o))!;
            }
            if (src.HTTPHostport != "")
            {
                return error.As(serveWebInterface(src.HTTPHostport, p, o, src.HTTPDisableBrowser))!;
            }
            return error.As(interactive(p, o))!;

        });

        private static (ptr<command>, ptr<report.Report>, error) generateRawReport(ptr<profile.Profile> _addr_p, slice<@string> cmd, variables vars, ptr<plugin.Options> _addr_o) => func((_, panic, __) =>
        {
            ptr<command> _p0 = default!;
            ptr<report.Report> _p0 = default!;
            error _p0 = default!;
            ref profile.Profile p = ref _addr_p.val;
            ref plugin.Options o = ref _addr_o.val;

            p = p.Copy(); // Prevent modification to the incoming profile.

            // Identify units of numeric tags in profile.
            var numLabelUnits = identifyNumLabelUnits(_addr_p, o.UI); 

            // Get report output format
            var c = pprofCommands[cmd[0L]];
            if (c == null)
            {
                panic("unexpected nil command");
            }

            vars = applyCommandOverrides(cmd[0L], c.format, vars); 

            // Delay focus after configuring report to get percentages on all samples.
            var relative = vars["relative_percentages"].boolValue();
            if (relative)
            {
                {
                    var err__prev2 = err;

                    var err = applyFocus(p, numLabelUnits, vars, o.UI);

                    if (err != null)
                    {
                        return (_addr_null!, _addr_null!, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            var (ropt, err) = reportOptions(_addr_p, numLabelUnits, vars);
            if (err != null)
            {
                return (_addr_null!, _addr_null!, error.As(err)!);
            }

            ropt.OutputFormat = c.format;
            if (len(cmd) == 2L)
            {
                var (s, err) = regexp.Compile(cmd[1L]);
                if (err != null)
                {
                    return (_addr_null!, _addr_null!, error.As(fmt.Errorf("parsing argument regexp %s: %v", cmd[1L], err))!);
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
                        return (_addr_null!, _addr_null!, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            {
                var err__prev1 = err;

                err = aggregate(_addr_p, vars);

                if (err != null)
                {
                    return (_addr_null!, _addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }


            return (_addr_c!, _addr_rpt!, error.As(null!)!);

        });

        private static error generateReport(ptr<profile.Profile> _addr_p, slice<@string> cmd, variables vars, ptr<plugin.Options> _addr_o)
        {
            ref profile.Profile p = ref _addr_p.val;
            ref plugin.Options o = ref _addr_o.val;

            var (c, rpt, err) = generateRawReport(_addr_p, cmd, vars, _addr_o);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // Generate the report.
            ptr<object> dst = @new<bytes.Buffer>();
            {
                var err__prev1 = err;

                var err = report.Generate(dst, rpt, o.Obj);

                if (err != null)
                {
                    return error.As(err)!;
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
                        return error.As(err)!;
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
                    return error.As(c.visualizer(src, os.Stdout, o.UI))!;
                }

                var (_, err) = src.WriteTo(os.Stdout);
                return error.As(err)!;

            } 

            // Output to specified file.
            o.UI.PrintErr("Generating report in ", output);
            var (out, err) = o.Writer.Open(output);
            if (err != null)
            {
                return error.As(err)!;
            }

            {
                var err__prev1 = err;

                (_, err) = src.WriteTo(out);

                if (err != null)
                {
                    @out.Close();
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            return error.As(@out.Close())!;

        }

        private static variables applyCommandOverrides(@string cmd, long outputFormat, variables v)
        { 
            // Some report types override the trim flag to false below. This is to make
            // sure the default heuristics of excluding insignificant nodes and edges
            // from the call graph do not apply. One example where it is important is
            // annotated source or disassembly listing. Those reports run on a specific
            // function (or functions), but the trimming is applied before the function
            // data is selected. So, with trimming enabled, the report could end up
            // showing no data if the specified function is "uninteresting" as far as the
            // trimming is concerned.
            var trim = v["trim"].boolValue();

            switch (cmd)
            {
                case "disasm": 

                case "weblist": 
                    trim = false;
                    v.set("addresses", "t"); 
                    // Force the 'noinlines' mode so that source locations for a given address
                    // collapse and there is only one for the given address. Without this
                    // cumulative metrics would be double-counted when annotating the assembly.
                    // This is because the merge is done by address and in case of an inlined
                    // stack each of the inlined entries is a separate callgraph node.
                    v.set("noinlines", "t");
                    break;
                case "peek": 
                    trim = false;
                    break;
                case "list": 
                    trim = false;
                    v.set("lines", "t"); 
                    // Do not force 'noinlines' to be false so that specifying
                    // "-list foo -noinlines" is supported and works as expected.
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


            if (outputFormat == report.Proto || outputFormat == report.Raw || outputFormat == report.Callgrind) 
                trim = false;
                v.set("addresses", "t");
                v.set("noinlines", "f");
                        if (!trim)
            {
                v.set("nodecount", "0");
                v.set("nodefraction", "0");
                v.set("edgefraction", "0");
            }

            return v;

        }

        private static error aggregate(ptr<profile.Profile> _addr_prof, variables v)
        {
            ref profile.Profile prof = ref _addr_prof.val;

            bool function = default;            bool filename = default;            bool linenumber = default;            bool address = default;

            var inlines = !v["noinlines"].boolValue();

            if (v["addresses"].boolValue()) 
                if (inlines)
                {
                    return error.As(null!)!;
                }

                function = true;
                filename = true;
                linenumber = true;
                address = true;
            else if (v["lines"].boolValue()) 
                function = true;
                filename = true;
                linenumber = true;
            else if (v["files"].boolValue()) 
                filename = true;
            else if (v["functions"].boolValue()) 
                function = true;
            else if (v["filefunctions"].boolValue()) 
                function = true;
                filename = true;
            else 
                return error.As(fmt.Errorf("unexpected granularity"))!;
                        return error.As(prof.Aggregate(inlines, function, filename, linenumber, address))!;

        }

        private static (ptr<report.Options>, error) reportOptions(ptr<profile.Profile> _addr_p, map<@string, @string> numLabelUnits, variables vars)
        {
            ptr<report.Options> _p0 = default!;
            error _p0 = default!;
            ref profile.Profile p = ref _addr_p.val;

            var si = vars["sample_index"].value;
            var mean = vars["mean"].boolValue();
            var (value, meanDiv, sample, err) = sampleFormat(_addr_p, si, mean);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var stype = sample.Type;
            if (mean)
            {
                stype = "mean_" + stype;
            }

            if (vars["divide_by"].floatValue() == 0L)
            {
                return (_addr_null!, error.As(fmt.Errorf("zero divisor specified"))!);
            }

            slice<@string> filters = default;
            foreach (var (_, k) in new slice<@string>(new @string[] { "focus", "ignore", "hide", "show", "show_from", "tagfocus", "tagignore", "tagshow", "taghide" }))
            {
                var v = vars[k].value;
                if (v != "")
                {
                    filters = append(filters, k + "=" + v);
                }

            }
            ptr<report.Options> ropt = addr(new report.Options(CumSort:vars["cum"].boolValue(),CallTree:vars["call_tree"].boolValue(),DropNegative:vars["drop_negative"].boolValue(),CompactLabels:vars["compact_labels"].boolValue(),Ratio:1/vars["divide_by"].floatValue(),NodeCount:vars["nodecount"].intValue(),NodeFraction:vars["nodefraction"].floatValue(),EdgeFraction:vars["edgefraction"].floatValue(),ActiveFilters:filters,NumLabelUnits:numLabelUnits,SampleValue:value,SampleMeanDivisor:meanDiv,SampleType:stype,SampleUnit:sample.Unit,OutputUnit:vars["unit"].value,SourcePath:vars["source_path"].stringValue(),TrimPath:vars["trim_path"].stringValue(),));

            if (len(p.Mapping) > 0L && p.Mapping[0L].File != "")
            {
                ropt.Title = filepath.Base(p.Mapping[0L].File);
            }

            return (_addr_ropt!, error.As(null!)!);

        }

        // identifyNumLabelUnits returns a map of numeric label keys to the units
        // associated with those keys.
        private static map<@string, @string> identifyNumLabelUnits(ptr<profile.Profile> _addr_p, plugin.UI ui)
        {
            ref profile.Profile p = ref _addr_p.val;

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
        private static (sampleValueFunc, sampleValueFunc, ptr<profile.ValueType>, error) sampleFormat(ptr<profile.Profile> _addr_p, @string sampleIndex, bool mean)
        {
            sampleValueFunc value = default;
            sampleValueFunc meanDiv = default;
            ptr<profile.ValueType> v = default!;
            error err = default!;
            ref profile.Profile p = ref _addr_p.val;

            if (len(p.SampleType) == 0L)
            {
                return (null, null, _addr_null!, error.As(fmt.Errorf("profile has no samples"))!);
            }

            var (index, err) = p.SampleIndexByName(sampleIndex);
            if (err != null)
            {
                return (null, null, _addr_null!, error.As(err)!);
            }

            value = valueExtractor(index);
            if (mean)
            {
                meanDiv = valueExtractor(0L);
            }

            v = p.SampleType[index];
            return ;

        }

        private static sampleValueFunc valueExtractor(long ix)
        {
            return v =>
            {
                return v[ix];
            };

        }
    }
}}}}}}}
