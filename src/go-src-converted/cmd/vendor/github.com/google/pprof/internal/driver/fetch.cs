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

// package driver -- go2cs converted at 2020 August 29 10:05:24 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\fetch.go
using bytes = go.bytes_package;
using tls = go.crypto.tls_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using url = go.net.url_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using measurement = go.github.com.google.pprof.@internal.measurement_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;
using System;
using System.Threading;

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
        // fetchProfiles fetches and symbolizes the profiles specified by s.
        // It will merge all the profiles it is able to retrieve, even if
        // there are some failures. It will return an error if it is unable to
        // fetch any profiles.
        private static (ref profile.Profile, error) fetchProfiles(ref source s, ref plugin.Options o)
        {
            var sources = make_slice<profileSource>(0L, len(s.Sources));
            {
                var src__prev1 = src;

                foreach (var (_, __src) in s.Sources)
                {
                    src = __src;
                    sources = append(sources, new profileSource(addr:src,source:s,));
                }
                src = src__prev1;
            }

            var bases = make_slice<profileSource>(0L, len(s.Base));
            {
                var src__prev1 = src;

                foreach (var (_, __src) in s.Base)
                {
                    src = __src;
                    bases = append(bases, new profileSource(addr:src,source:s,));
                }
                src = src__prev1;
            }

            var (p, pbase, m, mbase, save, err) = grabSourcesAndBases(sources, bases, o.Fetch, o.Obj, o.UI);
            if (err != null)
            {
                return (null, err);
            }
            if (pbase != null)
            {
                if (s.Normalize)
                {
                    var err = p.Normalize(pbase);
                    if (err != null)
                    {
                        return (null, err);
                    }
                }
                pbase.Scale(-1L);
                p, m, err = combineProfiles(new slice<ref profile.Profile>(new ref profile.Profile[] { p, pbase }), new slice<plugin.MappingSources>(new plugin.MappingSources[] { m, mbase }));
                if (err != null)
                {
                    return (null, err);
                }
            }
            {
                var err__prev1 = err;

                err = o.Sym.Symbolize(s.Symbolize, m, p);

                if (err != null)
                {
                    return (null, err);
                }
                err = err__prev1;

            }
            p.RemoveUninteresting();
            unsourceMappings(p);

            if (s.Comment != "")
            {
                p.Comments = append(p.Comments, s.Comment);
            }
            if (save)
            {
                var (dir, err) = setTmpDir(o.UI);
                if (err != null)
                {
                    return (null, err);
                }
                @string prefix = "pprof.";
                if (len(p.Mapping) > 0L && p.Mapping[0L].File != "")
                {
                    prefix += filepath.Base(p.Mapping[0L].File) + ".";
                }
                foreach (var (_, s) in p.SampleType)
                {
                    prefix += s.Type + ".";
                }                var (tempFile, err) = newTempFile(dir, prefix, ".pb.gz");
                if (err == null)
                {
                    err = p.Write(tempFile);

                    if (err == null)
                    {
                        o.UI.PrintErr("Saved profile in ", tempFile.Name());
                    }
                }
                if (err != null)
                {
                    o.UI.PrintErr("Could not save profile: ", err);
                }
            }
            {
                var err__prev1 = err;

                err = p.CheckValid();

                if (err != null)
                {
                    return (null, err);
                }
                err = err__prev1;

            }

            return (p, null);
        }

        private static (ref profile.Profile, ref profile.Profile, plugin.MappingSources, plugin.MappingSources, bool, error) grabSourcesAndBases(slice<profileSource> sources, slice<profileSource> bases, plugin.Fetcher fetch, plugin.ObjTool obj, plugin.UI ui) => func((defer, _, __) =>
        {
            sync.WaitGroup wg = new sync.WaitGroup();
            wg.Add(2L);
            ref profile.Profile psrc = default;            ref profile.Profile pbase = default;

            plugin.MappingSources msrc = default;            plugin.MappingSources mbase = default;

            bool savesrc = default;            bool savebase = default;

            error errsrc = default;            error errbase = default;

            long countsrc = default;            long countbase = default;

            go_(() => () =>
            {
                defer(wg.Done());
                psrc, msrc, savesrc, countsrc, errsrc = chunkedGrab(sources, fetch, obj, ui);
            }());
            go_(() => () =>
            {
                defer(wg.Done());
                pbase, mbase, savebase, countbase, errbase = chunkedGrab(bases, fetch, obj, ui);
            }());
            wg.Wait();
            var save = savesrc || savebase;

            if (errsrc != null)
            {
                return (null, null, null, null, false, fmt.Errorf("problem fetching source profiles: %v", errsrc));
            }
            if (errbase != null)
            {
                return (null, null, null, null, false, fmt.Errorf("problem fetching base profiles: %v,", errbase));
            }
            if (countsrc == 0L)
            {
                return (null, null, null, null, false, fmt.Errorf("failed to fetch any source profiles"));
            }
            if (countbase == 0L && len(bases) > 0L)
            {
                return (null, null, null, null, false, fmt.Errorf("failed to fetch any base profiles"));
            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                var want = len(sources);
                var got = countsrc;

                if (want != got)
                {
                    ui.PrintErr(fmt.Sprintf("Fetched %d source profiles out of %d", got, want));
                }

                want = want__prev1;
                got = got__prev1;

            }
            {
                var want__prev1 = want;
                var got__prev1 = got;

                want = len(bases);
                got = countbase;

                if (want != got)
                {
                    ui.PrintErr(fmt.Sprintf("Fetched %d base profiles out of %d", got, want));
                }

                want = want__prev1;
                got = got__prev1;

            }

            return (psrc, pbase, msrc, mbase, save, null);
        });

        // chunkedGrab fetches the profiles described in source and merges them into
        // a single profile. It fetches a chunk of profiles concurrently, with a maximum
        // chunk size to limit its memory usage.
        private static (ref profile.Profile, plugin.MappingSources, bool, long, error) chunkedGrab(slice<profileSource> sources, plugin.Fetcher fetch, plugin.ObjTool obj, plugin.UI ui)
        {
            const long chunkSize = 64L;



            ref profile.Profile p = default;
            plugin.MappingSources msrc = default;
            bool save = default;
            long count = default;

            {
                long start = 0L;

                while (start < len(sources))
                {
                    var end = start + chunkSize;
                    if (end > len(sources))
                    {
                        end = len(sources);
                    start += chunkSize;
                    }
                    var (chunkP, chunkMsrc, chunkSave, chunkCount, chunkErr) = concurrentGrab(sources[start..end], fetch, obj, ui);

                    if (chunkErr != null) 
                        return (null, null, false, 0L, chunkErr);
                    else if (chunkP == null) 
                        continue;
                    else if (p == null) 
                        p = chunkP;
                        msrc = chunkMsrc;
                        save = chunkSave;
                        count = chunkCount;
                    else 
                        p, msrc, chunkErr = combineProfiles(new slice<ref profile.Profile>(new ref profile.Profile[] { p, chunkP }), new slice<plugin.MappingSources>(new plugin.MappingSources[] { msrc, chunkMsrc }));
                        if (chunkErr != null)
                        {
                            return (null, null, false, 0L, chunkErr);
                        }
                        if (chunkSave)
                        {
                            save = true;
                        }
                        count += chunkCount;
                                    }

            }

            return (p, msrc, save, count, null);
        }

        // concurrentGrab fetches multiple profiles concurrently
        private static (ref profile.Profile, plugin.MappingSources, bool, long, error) concurrentGrab(slice<profileSource> sources, plugin.Fetcher fetch, plugin.ObjTool obj, plugin.UI ui) => func((defer, _, __) =>
        {
            sync.WaitGroup wg = new sync.WaitGroup();
            wg.Add(len(sources));
            {
                var i__prev1 = i;

                foreach (var (__i) in sources)
                {
                    i = __i;
                    go_(() => s =>
                    {
                        defer(wg.Done());
                        s.p, s.msrc, s.remote, s.err = grabProfile(s.source, s.addr, fetch, obj, ui);
                    }(ref sources[i]));
                }

                i = i__prev1;
            }

            wg.Wait();

            bool save = default;
            var profiles = make_slice<ref profile.Profile>(0L, len(sources));
            var msrcs = make_slice<plugin.MappingSources>(0L, len(sources));
            {
                var i__prev1 = i;

                foreach (var (__i) in sources)
                {
                    i = __i;
                    var s = ref sources[i];
                    {
                        var err = s.err;

                        if (err != null)
                        {
                            ui.PrintErr(s.addr + ": " + err.Error());
                            continue;
                        }

                    }
                    save = save || s.remote;
                    profiles = append(profiles, s.p);
                    msrcs = append(msrcs, s.msrc);
                    s.Value = new profileSource();
                }

                i = i__prev1;
            }

            if (len(profiles) == 0L)
            {
                return (null, null, false, 0L, null);
            }
            var (p, msrc, err) = combineProfiles(profiles, msrcs);
            if (err != null)
            {
                return (null, null, false, 0L, err);
            }
            return (p, msrc, save, len(profiles), null);
        });

        private static (ref profile.Profile, plugin.MappingSources, error) combineProfiles(slice<ref profile.Profile> profiles, slice<plugin.MappingSources> msrcs)
        { 
            // Merge profiles.
            {
                var err = measurement.ScaleProfiles(profiles);

                if (err != null)
                {
                    return (null, null, err);
                }

            }

            var (p, err) = profile.Merge(profiles);
            if (err != null)
            {
                return (null, null, err);
            } 

            // Combine mapping sources.
            var msrc = make(plugin.MappingSources);
            foreach (var (_, ms) in msrcs)
            {
                foreach (var (m, s) in ms)
                {
                    msrc[m] = append(msrc[m], s);
                }
            }
            return (p, msrc, null);
        }

        private partial struct profileSource
        {
            public @string addr;
            public ptr<source> source;
            public ptr<profile.Profile> p;
            public plugin.MappingSources msrc;
            public bool remote;
            public error err;
        }

        private static @string homeEnv()
        {
            switch (runtime.GOOS)
            {
                case "windows": 
                    return "USERPROFILE";
                    break;
                case "plan9": 
                    return "home";
                    break;
                default: 
                    return "HOME";
                    break;
            }
        }

        // setTmpDir prepares the directory to use to save profiles retrieved
        // remotely. It is selected from PPROF_TMPDIR, defaults to $HOME/pprof, and, if
        // $HOME is not set, falls back to os.TempDir().
        private static (@string, error) setTmpDir(plugin.UI ui)
        {
            slice<@string> dirs = default;
            {
                var profileDir = os.Getenv("PPROF_TMPDIR");

                if (profileDir != "")
                {
                    dirs = append(dirs, profileDir);
                }

            }
            {
                var homeDir = os.Getenv(homeEnv());

                if (homeDir != "")
                {
                    dirs = append(dirs, filepath.Join(homeDir, "pprof"));
                }

            }
            dirs = append(dirs, os.TempDir());
            foreach (var (_, tmpDir) in dirs)
            {
                {
                    var err = os.MkdirAll(tmpDir, 0755L);

                    if (err != null)
                    {
                        ui.PrintErr("Could not use temp dir ", tmpDir, ": ", err.Error());
                        continue;
                    }

                }
                return (tmpDir, null);
            }
            return ("", fmt.Errorf("failed to identify temp dir"));
        }

        private static readonly @string testSourceAddress = "pproftest.local";

        // grabProfile fetches a profile. Returns the profile, sources for the
        // profile mappings, a bool indicating if the profile was fetched
        // remotely, and an error.


        // grabProfile fetches a profile. Returns the profile, sources for the
        // profile mappings, a bool indicating if the profile was fetched
        // remotely, and an error.
        private static (ref profile.Profile, plugin.MappingSources, bool, error) grabProfile(ref source s, @string source, plugin.Fetcher fetcher, plugin.ObjTool obj, plugin.UI ui)
        {
            @string src = default;
            var duration = time.Duration(s.Seconds) * time.Second;
            var timeout = time.Duration(s.Timeout) * time.Second;
            if (fetcher != null)
            {
                p, src, err = fetcher.Fetch(source, duration, timeout);
                if (err != null)
                {
                    return;
                }
            }
            if (err != null || p == null)
            { 
                // Fetch the profile over HTTP or from a file.
                p, src, err = fetch(source, duration, timeout, ui);
                if (err != null)
                {
                    return;
                }
            }
            err = p.CheckValid();

            if (err != null)
            {
                return;
            } 

            // Update the binary locations from command line and paths.
            locateBinaries(p, s, obj, ui); 

            // Collect the source URL for all mappings.
            if (src != "")
            {
                msrc = collectMappingSources(p, src);
                remote = true;
                if (strings.HasPrefix(src, "http://" + testSourceAddress))
                { 
                    // Treat test inputs as local to avoid saving
                    // testcase profiles during driver testing.
                    remote = false;
                }
            }
            return;
        }

        // collectMappingSources saves the mapping sources of a profile.
        private static plugin.MappingSources collectMappingSources(ref profile.Profile p, @string source)
        {
            plugin.MappingSources ms = new plugin.MappingSources();
            foreach (var (_, m) in p.Mapping)
            {
                struct{SourcestringStartuint64} src = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{SourcestringStartuint64}{source,m.Start,};
                var key = m.BuildID;
                if (key == "")
                {
                    key = m.File;
                }
                if (key == "")
                { 
                    // If there is no build id or source file, use the source as the
                    // mapping file. This will enable remote symbolization for this
                    // mapping, in particular for Go profiles on the legacy format.
                    // The source is reset back to empty string by unsourceMapping
                    // which is called after symbolization is finished.
                    m.File = source;
                    key = source;
                }
                ms[key] = append(ms[key], src);
            }
            return ms;
        }

        // unsourceMappings iterates over the mappings in a profile and replaces file
        // set to the remote source URL by collectMappingSources back to empty string.
        private static void unsourceMappings(ref profile.Profile p)
        {
            foreach (var (_, m) in p.Mapping)
            {
                if (m.BuildID == "")
                {
                    {
                        var (u, err) = url.Parse(m.File);

                        if (err == null && u.IsAbs())
                        {
                            m.File = "";
                        }

                    }
                }
            }
        }

        // locateBinaries searches for binary files listed in the profile and, if found,
        // updates the profile accordingly.
        private static void locateBinaries(ref profile.Profile _p, ref source _s, plugin.ObjTool obj, plugin.UI ui) => func(_p, _s, (ref profile.Profile p, ref source s, Defer defer, Panic _, Recover __) =>
        { 
            // Construct search path to examine
            var searchPath = os.Getenv("PPROF_BINARY_PATH");
            if (searchPath == "")
            { 
                // Use $HOME/pprof/binaries as default directory for local symbolization binaries
                searchPath = filepath.Join(os.Getenv(homeEnv()), "pprof", "binaries");
            }
mapping:
            {
                var m__prev1 = m;

                foreach (var (_, __m) in p.Mapping)
                {
                    m = __m;
                    @string baseName = default;
                    if (m.File != "")
                    {
                        baseName = filepath.Base(m.File);
                    }
                    foreach (var (_, path) in filepath.SplitList(searchPath))
                    {
                        slice<@string> fileNames = default;
                        if (m.BuildID != "")
                        {
                            fileNames = new slice<@string>(new @string[] { filepath.Join(path,m.BuildID,baseName) });
                            {
                                var (matches, err) = filepath.Glob(filepath.Join(path, m.BuildID, "*"));

                                if (err == null)
                                {
                                    fileNames = append(fileNames, matches);
                                }

                            }
                        }
                        if (m.File != "")
                        { 
                            // Try both the basename and the full path, to support the same directory
                            // structure as the perf symfs option.
                            if (baseName != "")
                            {
                                fileNames = append(fileNames, filepath.Join(path, baseName));
                            }
                            fileNames = append(fileNames, filepath.Join(path, m.File));
                        }
                        foreach (var (_, name) in fileNames)
                        {
                            {
                                var (f, err) = obj.Open(name, m.Start, m.Limit, m.Offset);

                                if (err == null)
                                {
                                    defer(f.Close());
                                    var fileBuildID = f.BuildID();
                                    if (m.BuildID != "" && m.BuildID != fileBuildID)
                                    {
                                        ui.PrintErr("Ignoring local file " + name + ": build-id mismatch (" + m.BuildID + " != " + fileBuildID + ")");
                                    }
                                    else
                                    {
                                        m.File = name;
                                        _continuemapping = true;
                                        break;
                                    }
                                }

                            }
                        }
                    }
                }

                m = m__prev1;
            }
            if (len(p.Mapping) == 0L)
            { 
                // If there are no mappings, add a fake mapping to attempt symbolization.
                // This is useful for some profiles generated by the golang runtime, which
                // do not include any mappings. Symbolization with a fake mapping will only
                // be successful against a non-PIE binary.
                profile.Mapping m = ref new profile.Mapping(ID:1);
                p.Mapping = new slice<ref profile.Mapping>(new ref profile.Mapping[] { m });
                foreach (var (_, l) in p.Location)
                {
                    l.Mapping = m;
                }
            } 
            // Replace executable filename/buildID with the overrides from source.
            // Assumes the executable is the first Mapping entry.
            {
                var execName = s.ExecName;
                var buildID = s.BuildID;

                if (execName != "" || buildID != "")
                {
                    m = p.Mapping[0L];
                    if (execName != "")
                    {
                        m.File = execName;
                    }
                    if (buildID != "")
                    {
                        m.BuildID = buildID;
                    }
                }

            }
        });

        // fetch fetches a profile from source, within the timeout specified,
        // producing messages through the ui. It returns the profile and the
        // url of the actual source of the profile for remote profiles.
        private static (ref profile.Profile, @string, error) fetch(@string source, time.Duration duration, time.Duration timeout, plugin.UI ui) => func((defer, _, __) =>
        {
            io.ReadCloser f = default;

            {
                var (sourceURL, timeout) = adjustURL(source, duration, timeout);

                if (sourceURL != "")
                {
                    ui.Print("Fetching profile over HTTP from " + sourceURL);
                    if (duration > 0L)
                    {
                        ui.Print(fmt.Sprintf("Please wait... (%v)", duration));
                    }
                    f, err = fetchURL(sourceURL, timeout);
                    src = sourceURL;
                }
                else if (isPerfFile(source))
                {
                    f, err = convertPerfData(source, ui);
                }
                else
                {
                    f, err = os.Open(source);
                }

            }
            if (err == null)
            {
                defer(f.Close());
                p, err = profile.Parse(f);
            }
            return;
        });

        // fetchURL fetches a profile from a URL using HTTP.
        private static (io.ReadCloser, error) fetchURL(@string source, time.Duration timeout) => func((defer, _, __) =>
        {
            var (resp, err) = httpGet(source, timeout);
            if (err != null)
            {
                return (null, fmt.Errorf("http fetch: %v", err));
            }
            if (resp.StatusCode != http.StatusOK)
            {
                defer(resp.Body.Close());
                return (null, statusCodeError(resp));
            }
            return (resp.Body, null);
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

        // isPerfFile checks if a file is in perf.data format. It also returns false
        // if it encounters an error during the check.
        private static bool isPerfFile(@string path) => func((defer, _, __) =>
        {
            var (sourceFile, openErr) = os.Open(path);
            if (openErr != null)
            {
                return false;
            }
            defer(sourceFile.Close()); 

            // If the file is the output of a perf record command, it should begin
            // with the string PERFILE2.
            slice<byte> perfHeader = (slice<byte>)"PERFILE2";
            var actualHeader = make_slice<byte>(len(perfHeader));
            {
                var (_, readErr) = sourceFile.Read(actualHeader);

                if (readErr != null)
                {
                    return false;
                }

            }
            return bytes.Equal(actualHeader, perfHeader);
        });

        // convertPerfData converts the file at path which should be in perf.data format
        // using the perf_to_profile tool and returns the file containing the
        // profile.proto formatted data.
        private static (ref os.File, error) convertPerfData(@string perfPath, plugin.UI ui)
        {
            ui.Print(fmt.Sprintf("Converting %s to a profile.proto... (May take a few minutes)", perfPath));
            var (profile, err) = newTempFile(os.TempDir(), "pprof_", ".pb.gz");
            if (err != null)
            {
                return (null, err);
            }
            deferDeleteTempFile(profile.Name());
            var cmd = exec.Command("perf_to_profile", perfPath, profile.Name());
            {
                var err = cmd.Run();

                if (err != null)
                {
                    profile.Close();
                    return (null, fmt.Errorf("failed to convert perf.data file. Try github.com/google/perf_data_converter: %v", err));
                }

            }
            return (profile, null);
        }

        // adjustURL validates if a profile source is a URL and returns an
        // cleaned up URL and the timeout to use for retrieval over HTTP.
        // If the source cannot be recognized as a URL it returns an empty string.
        private static (@string, time.Duration) adjustURL(@string source, time.Duration duration, time.Duration timeout)
        {
            var (u, err) = url.Parse(source);
            if (err != null || (u.Host == "" && u.Scheme != "" && u.Scheme != "file"))
            { 
                // Try adding http:// to catch sources of the form hostname:port/path.
                // url.Parse treats "hostname" as the scheme.
                u, err = url.Parse("http://" + source);
            }
            if (err != null || u.Host == "")
            {
                return ("", 0L);
            } 

            // Apply duration/timeout overrides to URL.
            var values = u.Query();
            if (duration > 0L)
            {
                values.Set("seconds", fmt.Sprint(int(duration.Seconds())));
            }
            else
            {
                {
                    var urlSeconds = values.Get("seconds");

                    if (urlSeconds != "")
                    {
                        {
                            var (us, err) = strconv.ParseInt(urlSeconds, 10L, 32L);

                            if (err == null)
                            {
                                duration = time.Duration(us) * time.Second;
                            }

                        }
                    }

                }
            }
            if (timeout <= 0L)
            {
                if (duration > 0L)
                {
                    timeout = duration + duration / 2L;
                }
                else
                {
                    timeout = 60L * time.Second;
                }
            }
            u.RawQuery = values.Encode();
            return (u.String(), timeout);
        }

        // httpGet is a wrapper around http.Get; it is defined as a variable
        // so it can be redefined during for testing.
        private static Func<@string, time.Duration, (ref http.Response, error)> httpGet = (source, timeout) =>
        {
            var (url, err) = url.Parse(source);
            if (err != null)
            {
                return (null, err);
            }
            private static ref tls.Config tlsConfig = default;
            if (url.Scheme == "https+insecure")
            {
                tlsConfig = ref new tls.Config(InsecureSkipVerify:true,);
                url.Scheme = "https";
                source = url.String();
            }
            http.Client client = ref new http.Client(Transport:&http.Transport{ResponseHeaderTimeout:timeout+5*time.Second,Proxy:http.ProxyFromEnvironment,TLSClientConfig:tlsConfig,},);
            return client.Get(source);
        };
    }
}}}}}}}
