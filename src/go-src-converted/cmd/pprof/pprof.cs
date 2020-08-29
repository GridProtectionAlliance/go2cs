// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// pprof is a tool for visualization of profile.data. It is based on
// the upstream version at github.com/google/pprof, with minor
// modifications specific to the Go distribution. Please consider
// upstreaming any modifications to these packages.

// package main -- go2cs converted at 2020 August 29 10:04:50 UTC
// Original source: C:\Go\src\cmd\pprof\pprof.go
using tls = go.crypto.tls_package;
using dwarf = go.debug.dwarf_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using url = go.net.url_package;
using os = go.os_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using objfile = go.cmd.@internal.objfile_package;

using driver = go.github.com.google.pprof.driver_package;
using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            driver.Options options = ref new driver.Options(Fetch:new(fetcher),Obj:new(objTool),);
            {
                var err = driver.PProf(options);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "%v\n", err);
                    os.Exit(2L);
                }
            }
        }

        private partial struct fetcher
        {
        }

        private static (ref profile.Profile, @string, error) Fetch(this ref fetcher f, @string src, time.Duration duration, time.Duration timeout)
        {
            var (sourceURL, timeout) = adjustURL(src, duration, timeout);
            if (sourceURL == "")
            { 
                // Could not recognize URL, let regular pprof attempt to fetch the profile (eg. from a file)
                return (null, "", null);
            }
            fmt.Fprintln(os.Stderr, "Fetching profile over HTTP from", sourceURL);
            if (duration > 0L)
            {
                fmt.Fprintf(os.Stderr, "Please wait... (%v)\n", duration);
            }
            var (p, err) = getProfile(sourceURL, timeout);
            return (p, sourceURL, err);
        }

        private static (ref profile.Profile, error) getProfile(@string source, time.Duration timeout) => func((defer, _, __) =>
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
            http.Client client = ref new http.Client(Transport:&http.Transport{ResponseHeaderTimeout:timeout+5*time.Second,Proxy:http.ProxyFromEnvironment,TLSClientConfig:tlsConfig,},);
            var (resp, err) = client.Get(source);
            if (err != null)
            {
                return (null, err);
            }
            if (resp.StatusCode != http.StatusOK)
            {
                defer(resp.Body.Close());
                return (null, statusCodeError(resp));
            }
            return profile.Parse(resp.Body);
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

        // cpuProfileHandler is the Go pprof CPU profile handler URL.
        private static readonly @string cpuProfileHandler = "/debug/pprof/profile";

        // adjustURL applies the duration/timeout values and Go specific defaults


        // adjustURL applies the duration/timeout values and Go specific defaults
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
            if (u.Path == "" || u.Path == "/")
            {
                u.Path = cpuProfileHandler;
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

        // objTool implements driver.ObjTool using Go libraries
        // (instead of invoking GNU binutils).
        private partial struct objTool
        {
            public sync.Mutex mu;
            public map<@string, ref objfile.Disasm> disasmCache;
        }

        private static (driver.ObjFile, error) Open(this ref objTool _p0, @string name, ulong start, ulong limit, ulong offset)
        {
            var (of, err) = objfile.Open(name);
            if (err != null)
            {
                return (null, err);
            }
            file f = ref new file(name:name,file:of,);
            if (start != 0L)
            {
                {
                    var (load, err) = of.LoadAddress();

                    if (err == null)
                    {
                        f.offset = start - load;
                    }

                }
            }
            return (f, null);
        }

        private static (map<@string, @string>, error) Demangle(this ref objTool _p0, slice<@string> names)
        { 
            // No C++, nothing to demangle.
            return (make_map<@string, @string>(), null);
        }

        private static (slice<driver.Inst>, error) Disasm(this ref objTool t, @string file, ulong start, ulong end)
        {
            var (d, err) = t.cachedDisasm(file);
            if (err != null)
            {
                return (null, err);
            }
            slice<driver.Inst> asm = default;
            d.Decode(start, end, null, (pc, size, file, line, text) =>
            {
                asm = append(asm, new driver.Inst(Addr:pc,File:file,Line:line,Text:text));
            });
            return (asm, null);
        }

        private static (ref objfile.Disasm, error) cachedDisasm(this ref objTool _t, @string file) => func(_t, (ref objTool t, Defer defer, Panic _, Recover __) =>
        {
            t.mu.Lock();
            defer(t.mu.Unlock());
            if (t.disasmCache == null)
            {
                t.disasmCache = make_map<@string, ref objfile.Disasm>();
            }
            var d = t.disasmCache[file];
            if (d != null)
            {
                return (d, null);
            }
            var (f, err) = objfile.Open(file);
            if (err != null)
            {
                return (null, err);
            }
            d, err = f.Disasm();
            f.Close();
            if (err != null)
            {
                return (null, err);
            }
            t.disasmCache[file] = d;
            return (d, null);
        });

        private static void SetConfig(this ref objTool _p0, @string config)
        { 
            // config is usually used to say what binaries to invoke.
            // Ignore entirely.
        }

        // file implements driver.ObjFile using Go libraries
        // (instead of invoking GNU binutils).
        // A file represents a single executable being analyzed.
        private partial struct file
        {
            public @string name;
            public ulong offset;
            public slice<objfile.Sym> sym;
            public ptr<objfile.File> file;
            public objfile.Liner pcln;
            public bool triedDwarf;
            public ptr<dwarf.Data> dwarf;
        }

        private static @string Name(this ref file f)
        {
            return f.name;
        }

        private static ulong Base(this ref file f)
        { 
            // No support for shared libraries.
            return 0L;
        }

        private static @string BuildID(this ref file f)
        { 
            // No support for build ID.
            return "";
        }

        private static (slice<driver.Frame>, error) SourceLine(this ref file f, ulong addr)
        {
            if (f.pcln == null)
            {
                var (pcln, err) = f.file.PCLineTable();
                if (err != null)
                {
                    return (null, err);
                }
                f.pcln = pcln;
            }
            addr -= f.offset;
            var (file, line, fn) = f.pcln.PCToLine(addr);
            if (fn != null)
            {
                driver.Frame frame = new slice<driver.Frame>(new driver.Frame[] { {Func:fn.Name,File:file,Line:line,} });
                return (frame, null);
            }
            var frames = f.dwarfSourceLine(addr);
            if (frames != null)
            {
                return (frames, null);
            }
            return (null, fmt.Errorf("no line information for PC=%#x", addr));
        }

        // dwarfSourceLine tries to get file/line information using DWARF.
        // This is for C functions that appear in the profile.
        // Returns nil if there is no information available.
        private static slice<driver.Frame> dwarfSourceLine(this ref file f, ulong addr)
        {
            if (f.dwarf == null && !f.triedDwarf)
            { 
                // Ignore any error--we don't care exactly why there
                // is no DWARF info.
                f.dwarf, _ = f.file.DWARF();
                f.triedDwarf = true;
            }
            if (f.dwarf != null)
            {
                var r = f.dwarf.Reader();
                var (unit, err) = r.SeekPC(addr);
                if (err == null)
                {
                    {
                        var frames = f.dwarfSourceLineEntry(r, unit, addr);

                        if (frames != null)
                        {
                            return frames;
                        }

                    }
                }
            }
            return null;
        }

        // dwarfSourceLineEntry tries to get file/line information from a
        // DWARF compilation unit. Returns nil if it doesn't find anything.
        private static slice<driver.Frame> dwarfSourceLineEntry(this ref file f, ref dwarf.Reader r, ref dwarf.Entry entry, ulong addr)
        {
            var (lines, err) = f.dwarf.LineReader(entry);
            if (err != null)
            {
                return null;
            }
            dwarf.LineEntry lentry = default;
            {
                var err = lines.SeekPC(addr, ref lentry);

                if (err != null)
                {
                    return null;
                } 

                // Try to find the function name.

            } 

            // Try to find the function name.
            @string name = "";
FindName: 

            // TODO: Report inlined functions.

            {
                var (entry, err) = r.Next();

                while (entry != null && err == null)
                {
                    if (entry.Tag == dwarf.TagSubprogram)
                    {
                        var (ranges, err) = f.dwarf.Ranges(entry);
                        if (err != null)
                        {
                            return null;
                    entry, err = r.Next();
                        }
                        foreach (var (_, pcs) in ranges)
                        {
                            if (pcs[0L] <= addr && addr < pcs[1L])
                            {
                                bool ok = default; 
                                // TODO: AT_linkage_name, AT_MIPS_linkage_name.
                                name, ok = entry.Val(dwarf.AttrName)._<@string>();
                                if (ok)
                                {
                                    _breakFindName = true;
                                    break;
                                }
                            }
                        }
                    }
                } 

                // TODO: Report inlined functions.

            } 

            // TODO: Report inlined functions.
            driver.Frame frames = new slice<driver.Frame>(new driver.Frame[] { {Func:name,File:lentry.File.Name,Line:lentry.Line,} });

            return frames;
        }

        private static (slice<ref driver.Sym>, error) Symbols(this ref file f, ref regexp.Regexp r, ulong addr)
        {
            if (f.sym == null)
            {
                var (sym, err) = f.file.Symbols();
                if (err != null)
                {
                    return (null, err);
                }
                f.sym = sym;
            }
            slice<ref driver.Sym> @out = default;
            foreach (var (_, s) in f.sym)
            { 
                // Ignore a symbol with address 0 and size 0.
                // An ELF STT_FILE symbol will look like that.
                if (s.Addr == 0L && s.Size == 0L)
                {
                    continue;
                }
                if ((r == null || r.MatchString(s.Name)) && (addr == 0L || s.Addr <= addr && addr < s.Addr + uint64(s.Size)))
                {
                    out = append(out, ref new driver.Sym(Name:[]string{s.Name},File:f.name,Start:s.Addr,End:s.Addr+uint64(s.Size)-1,));
                }
            }
            return (out, null);
        }

        private static error Close(this ref file f)
        {
            f.file.Close();
            return error.As(null);
        }
    }
}
