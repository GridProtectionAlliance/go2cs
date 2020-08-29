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

// Package symbolizer provides a routine to populate a profile with
// symbol, file and line number information. It relies on the
// addr2liner and demangle packages to do the actual work.
// package symbolizer -- go2cs converted at 2020 August 29 10:06:13 UTC
// import "cmd/vendor/github.com/google/pprof/internal/symbolizer" ==> using symbolizer = go.cmd.vendor.github.com.google.pprof.@internal.symbolizer_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\symbolizer\symbolizer.go
using tls = go.crypto.tls_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using url = go.net.url_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using binutils = go.github.com.google.pprof.@internal.binutils_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using symbolz = go.github.com.google.pprof.@internal.symbolz_package;
using profile = go.github.com.google.pprof.profile_package;
using demangle = go.github.com.ianlancetaylor.demangle_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class symbolizer_package
    {
        // Symbolizer implements the plugin.Symbolize interface.
        public partial struct Symbolizer
        {
            public plugin.ObjTool Obj;
            public plugin.UI UI;
        }

        // test taps for dependency injection
        private static var symbolzSymbolize = symbolz.Symbolize;
        private static var localSymbolize = doLocalSymbolize;
        private static var demangleFunction = Demangle;

        // Symbolize attempts to symbolize profile p. First uses binutils on
        // local binaries; if the source is a URL it attempts to get any
        // missed entries using symbolz.
        private static error Symbolize(this ref Symbolizer s, @string mode, plugin.MappingSources sources, ref profile.Profile p)
        {
            var remote = true;
            var local = true;
            var fast = false;
            var force = false;
            @string demanglerMode = "";
            foreach (var (_, o) in strings.Split(strings.ToLower(mode), ":"))
            {
                switch (o)
                {
                    case "": 
                        continue;
                        break;
                    case "none": 

                    case "no": 
                        return error.As(null);
                        break;
                    case "local": 
                        remote = false;
                        local = true;
                        break;
                    case "fastlocal": 
                        remote = false;
                        local = true;
                        fast = true;
                        break;
                    case "remote": 
                        remote = true;
                        local = false;
                        break;
                    case "force": 
                        force = true;
                        break;
                    default: 
                        {
                            var d = strings.TrimPrefix(o, "demangle=");

                            switch (d)
                            {
                                case "full": 

                                case "none": 

                                case "templates": 
                                    demanglerMode = d;
                                    force = true;
                                    continue;
                                    break;
                                case "default": 
                                    continue;
                                    break;
                            }
                        }
                        s.UI.PrintErr("ignoring unrecognized symbolization option: " + mode);
                        s.UI.PrintErr("expecting -symbolize=[local|fastlocal|remote|none][:force][:demangle=[none|full|templates|default]");
                        break;
                }
            }
            error err = default;
            if (local)
            { 
                // Symbolize locally using binutils.
                err = error.As(localSymbolize(p, fast, force, s.Obj, s.UI));

                if (err != null)
                {
                    s.UI.PrintErr("local symbolization: " + err.Error());
                }
            }
            if (remote)
            {
                err = error.As(symbolzSymbolize(p, force, sources, postURL, s.UI));

                if (err != null)
                {
                    return error.As(err); // Ran out of options.
                }
            }
            demangleFunction(p, force, demanglerMode);
            return error.As(null);
        }

        // postURL issues a POST to a URL over HTTP.
        private static (slice<byte>, error) postURL(@string source, @string post) => func((defer, _, __) =>
        {
            var (url, err) = url.Parse(source);
            if (err != null)
            {
                return (null, err);
            }
            ref tls.Config tlsConfig = default;
            if (url.Scheme == "https+insecure")
            {
                tlsConfig = ref new tls.Config(InsecureSkipVerify:true,);
                url.Scheme = "https";
                source = url.String();
            }
            http.Client client = ref new http.Client(Transport:&http.Transport{TLSClientConfig:tlsConfig,},);
            var (resp, err) = client.Post(source, "application/octet-stream", strings.NewReader(post));
            if (err != null)
            {
                return (null, fmt.Errorf("http post %s: %v", source, err));
            }
            defer(resp.Body.Close());
            if (resp.StatusCode != http.StatusOK)
            {
                return (null, fmt.Errorf("http post %s: %v", source, statusCodeError(resp)));
            }
            return ioutil.ReadAll(resp.Body);
        });

        private static error statusCodeError(ref http.Response resp)
        {
            if (resp.Header.Get("X-Go-Pprof") != "" && strings.Contains(resp.Header.Get("Content-Type"), "text/plain"))
            { 
                // error is from pprof endpoint
                {
                    var (body, err) = ioutil.ReadAll(resp.Body);

                    if (err == null)
                    {
                        return error.As(fmt.Errorf("server response: %s - %s", resp.Status, body));
                    }

                }
            }
            return error.As(fmt.Errorf("server response: %s", resp.Status));
        }

        // doLocalSymbolize adds symbol and line number information to all locations
        // in a profile. mode enables some options to control
        // symbolization.
        private static error doLocalSymbolize(ref profile.Profile _prof, bool fast, bool force, plugin.ObjTool obj, plugin.UI ui) => func(_prof, (ref profile.Profile prof, Defer defer, Panic _, Recover __) =>
        {
            if (fast)
            {
                {
                    ref binutils.Binutils (bu, ok) = obj._<ref binutils.Binutils>();

                    if (ok)
                    {
                        bu.SetFastSymbolization(true);
                    }

                }
            }
            var (mt, err) = newMapping(prof, obj, ui, force);
            if (err != null)
            {
                return error.As(err);
            }
            defer(mt.close());

            var functions = make_map<profile.Function, ref profile.Function>();
            foreach (var (_, l) in mt.prof.Location)
            {
                var m = l.Mapping;
                var segment = mt.segments[m];
                if (segment == null)
                { 
                    // Nothing to do.
                    continue;
                }
                var (stack, err) = segment.SourceLine(l.Address);
                if (err != null || len(stack) == 0L)
                { 
                    // No answers from addr2line.
                    continue;
                }
                l.Line = make_slice<profile.Line>(len(stack));
                foreach (var (i, frame) in stack)
                {
                    if (frame.Func != "")
                    {
                        m.HasFunctions = true;
                    }
                    if (frame.File != "")
                    {
                        m.HasFilenames = true;
                    }
                    if (frame.Line != 0L)
                    {
                        m.HasLineNumbers = true;
                    }
                    profile.Function f = ref new profile.Function(Name:frame.Func,SystemName:frame.Func,Filename:frame.File,);
                    {
                        var fp = functions[f.Value];

                        if (fp != null)
                        {
                            f = fp;
                        }
                        else
                        {
                            functions[f.Value] = f;
                            f.ID = uint64(len(mt.prof.Function)) + 1L;
                            mt.prof.Function = append(mt.prof.Function, f);
                        }

                    }
                    l.Line[i] = new profile.Line(Function:f,Line:int64(frame.Line),);
                }
                if (len(stack) > 0L)
                {
                    m.HasInlineFrames = true;
                }
            }
            return error.As(null);
        });

        // Demangle updates the function names in a profile with demangled C++
        // names, simplified according to demanglerMode. If force is set,
        // overwrite any names that appear already demangled.
        public static void Demangle(ref profile.Profile prof, bool force, @string demanglerMode)
        {
            if (force)
            { 
                // Remove the current demangled names to force demangling
                foreach (var (_, f) in prof.Function)
                {
                    if (f.Name != "" && f.SystemName != "")
                    {
                        f.Name = f.SystemName;
                    }
                }
            }
            slice<demangle.Option> options = default;
            switch (demanglerMode)
            {
                case "": // demangled, simplified: no parameters, no templates, no return type
                    options = new slice<demangle.Option>(new demangle.Option[] { demangle.NoParams, demangle.NoTemplateParams });
                    break;
                case "templates": // demangled, simplified: no parameters, no return type
                    options = new slice<demangle.Option>(new demangle.Option[] { demangle.NoParams });
                    break;
                case "full": 
                    options = new slice<demangle.Option>(new demangle.Option[] { demangle.NoClones });
                    break;
                case "none": // no demangling
                    return;
                    break;
            } 

            // Copy the options because they may be updated by the call.
            var o = make_slice<demangle.Option>(len(options));
            foreach (var (_, fn) in prof.Function)
            {
                if (fn.Name != "" && fn.SystemName != fn.Name)
                {
                    continue; // Already demangled.
                }
                copy(o, options);
                {
                    var demangled = demangle.Filter(fn.SystemName, o);

                    if (demangled != fn.SystemName)
                    {
                        fn.Name = demangled;
                        continue;
                    } 
                    // Could not demangle. Apply heuristics in case the name is
                    // already demangled.

                } 
                // Could not demangle. Apply heuristics in case the name is
                // already demangled.
                var name = fn.SystemName;
                if (looksLikeDemangledCPlusPlus(name))
                {
                    if (demanglerMode == "" || demanglerMode == "templates")
                    {
                        name = removeMatching(name, '(', ')');
                    }
                    if (demanglerMode == "")
                    {
                        name = removeMatching(name, '<', '>');
                    }
                }
                fn.Name = name;
            }
        }

        // looksLikeDemangledCPlusPlus is a heuristic to decide if a name is
        // the result of demangling C++. If so, further heuristics will be
        // applied to simplify the name.
        private static bool looksLikeDemangledCPlusPlus(@string demangled)
        {
            if (strings.Contains(demangled, ".<"))
            { // Skip java names of the form "class.<init>"
                return false;
            }
            return strings.ContainsAny(demangled, "<>[]") || strings.Contains(demangled, "::");
        }

        // removeMatching removes nested instances of start..end from name.
        private static @string removeMatching(@string name, byte start, byte end)
        {
            var s = string(start) + string(end);
            long nesting = default;            long first = default;            long current = default;

            {
                var index = strings.IndexAny(name[current..], s);

                while (index != -1L)
                {
                    current += index;


                    if (name[current] == start) 
                        nesting++;
                        if (nesting == 1L)
                        {
                            first = current;
                    index = strings.IndexAny(name[current..], s);
                        }
                    else if (name[current] == end) 
                        nesting--;

                        if (nesting < 0L) 
                            return name; // Mismatch, abort
                        else if (nesting == 0L) 
                            name = name[..first] + name[current + 1L..];
                            current = first - 1L;
                        
                    current++;
                }

            }
            return name;
        }

        // newMapping creates a mappingTable for a profile.
        private static (ref mappingTable, error) newMapping(ref profile.Profile prof, plugin.ObjTool obj, plugin.UI ui, bool force)
        {
            mappingTable mt = ref new mappingTable(prof:prof,segments:make(map[*profile.Mapping]plugin.ObjFile),); 

            // Identify used mappings
            var mappings = make_map<ref profile.Mapping, bool>();
            foreach (var (_, l) in prof.Location)
            {
                mappings[l.Mapping] = true;
            }
            var missingBinaries = false;
            foreach (var (midx, m) in prof.Mapping)
            {
                if (!mappings[m])
                {
                    continue;
                } 

                // Do not attempt to re-symbolize a mapping that has already been symbolized.
                if (!force && (m.HasFunctions || m.HasFilenames || m.HasLineNumbers))
                {
                    continue;
                }
                if (m.File == "")
                {
                    if (midx == 0L)
                    {
                        ui.PrintErr("Main binary filename not available.");
                        continue;
                    }
                    missingBinaries = true;
                    continue;
                } 

                // Skip well-known system mappings
                if (m.Unsymbolizable())
                {
                    continue;
                } 

                // Skip mappings pointing to a source URL
                if (m.BuildID == "")
                {
                    {
                        var (u, err) = url.Parse(m.File);

                        if (err == null && u.IsAbs() && strings.Contains(strings.ToLower(u.Scheme), "http"))
                        {
                            continue;
                        }

                    }
                }
                var name = filepath.Base(m.File);
                var (f, err) = obj.Open(m.File, m.Start, m.Limit, m.Offset);
                if (err != null)
                {
                    ui.PrintErr("Local symbolization failed for ", name, ": ", err);
                    missingBinaries = true;
                    continue;
                }
                {
                    var fid = f.BuildID();

                    if (m.BuildID != "" && fid != "" && fid != m.BuildID)
                    {
                        ui.PrintErr("Local symbolization failed for ", name, ": build ID mismatch");
                        f.Close();
                        continue;
                    }

                }

                mt.segments[m] = f;
            }
            if (missingBinaries)
            {
                ui.PrintErr("Some binary filenames not available. Symbolization may be incomplete.\n" + "Try setting PPROF_BINARY_PATH to the search path for local binaries.");
            }
            return (mt, null);
        }

        // mappingTable contains the mechanisms for symbolization of a
        // profile.
        private partial struct mappingTable
        {
            public ptr<profile.Profile> prof;
            public map<ref profile.Mapping, plugin.ObjFile> segments;
        }

        // Close releases any external processes being used for the mapping.
        private static void close(this ref mappingTable mt)
        {
            foreach (var (_, segment) in mt.segments)
            {
                segment.Close();
            }
        }
    }
}}}}}}}
