// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// pprof is a tool for visualization of profile.data. It is based on
// the upstream version at github.com/google/pprof, with minor
// modifications specific to the Go distribution. Please consider
// upstreaming any modifications to these packages.

// package main -- go2cs converted at 2022 March 06 23:22:40 UTC
// Original source: C:\Program Files\Go\src\cmd\pprof\pprof.go
using tls = go.crypto.tls_package;
using dwarf = go.debug.dwarf_package;
using fmt = go.fmt_package;
using io = go.io_package;
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
using System;


namespace go;

public static partial class main_package {

private static void Main() {
    ptr<driver.Options> options = addr(new driver.Options(Fetch:new(fetcher),Obj:new(objTool),UI:newUI(),));
    {
        var err = driver.PProf(options);

        if (err != null) {
            fmt.Fprintf(os.Stderr, "%v\n", err);
            os.Exit(2);
        }
    }

}

private partial struct fetcher {
}

private static (ptr<profile.Profile>, @string, error) Fetch(this ptr<fetcher> _addr_f, @string src, time.Duration duration, time.Duration timeout) {
    ptr<profile.Profile> _p0 = default!;
    @string _p0 = default;
    error _p0 = default!;
    ref fetcher f = ref _addr_f.val;

    var (sourceURL, timeout) = adjustURL(src, duration, timeout);
    if (sourceURL == "") { 
        // Could not recognize URL, let regular pprof attempt to fetch the profile (eg. from a file)
        return (_addr_null!, "", error.As(null!)!);

    }
    fmt.Fprintln(os.Stderr, "Fetching profile over HTTP from", sourceURL);
    if (duration > 0) {
        fmt.Fprintf(os.Stderr, "Please wait... (%v)\n", duration);
    }
    var (p, err) = getProfile(sourceURL, timeout);
    return (_addr_p!, sourceURL, error.As(err)!);

}

private static (ptr<profile.Profile>, error) getProfile(@string source, time.Duration timeout) => func((defer, _, _) => {
    ptr<profile.Profile> _p0 = default!;
    error _p0 = default!;

    var (url, err) = url.Parse(source);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ptr<tls.Config> tlsConfig;
    if (url.Scheme == "https+insecure") {
        tlsConfig = addr(new tls.Config(InsecureSkipVerify:true,));
        url.Scheme = "https";
        source = url.String();
    }
    ptr<http.Client> client = addr(new http.Client(Transport:&http.Transport{ResponseHeaderTimeout:timeout+5*time.Second,Proxy:http.ProxyFromEnvironment,TLSClientConfig:tlsConfig,},));
    var (resp, err) = client.Get(source);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (resp.StatusCode != http.StatusOK) {
        defer(resp.Body.Close());
        return (_addr_null!, error.As(statusCodeError(_addr_resp))!);
    }
    return _addr_profile.Parse(resp.Body)!;

});

private static error statusCodeError(ptr<http.Response> _addr_resp) {
    ref http.Response resp = ref _addr_resp.val;

    if (resp.Header.Get("X-Go-Pprof") != "" && strings.Contains(resp.Header.Get("Content-Type"), "text/plain")) { 
        // error is from pprof endpoint
        {
            var (body, err) = io.ReadAll(resp.Body);

            if (err == null) {
                return error.As(fmt.Errorf("server response: %s - %s", resp.Status, body))!;
            }

        }

    }
    return error.As(fmt.Errorf("server response: %s", resp.Status))!;

}

// cpuProfileHandler is the Go pprof CPU profile handler URL.
private static readonly @string cpuProfileHandler = "/debug/pprof/profile";

// adjustURL applies the duration/timeout values and Go specific defaults


// adjustURL applies the duration/timeout values and Go specific defaults
private static (@string, time.Duration) adjustURL(@string source, time.Duration duration, time.Duration timeout) {
    @string _p0 = default;
    time.Duration _p0 = default;

    var (u, err) = url.Parse(source);
    if (err != null || (u.Host == "" && u.Scheme != "" && u.Scheme != "file")) { 
        // Try adding http:// to catch sources of the form hostname:port/path.
        // url.Parse treats "hostname" as the scheme.
        u, err = url.Parse("http://" + source);

    }
    if (err != null || u.Host == "") {
        return ("", 0);
    }
    if (u.Path == "" || u.Path == "/") {
        u.Path = cpuProfileHandler;
    }
    var values = u.Query();
    if (duration > 0) {
        values.Set("seconds", fmt.Sprint(int(duration.Seconds())));
    }
    else
 {
        {
            var urlSeconds = values.Get("seconds");

            if (urlSeconds != "") {
                {
                    var (us, err) = strconv.ParseInt(urlSeconds, 10, 32);

                    if (err == null) {
                        duration = time.Duration(us) * time.Second;
                    }

                }

            }

        }

    }
    if (timeout <= 0) {
        if (duration > 0) {
            timeout = duration + duration / 2;
        }
        else
 {
            timeout = 60 * time.Second;
        }
    }
    u.RawQuery = values.Encode();
    return (u.String(), timeout);

}

// objTool implements driver.ObjTool using Go libraries
// (instead of invoking GNU binutils).
private partial struct objTool {
    public sync.Mutex mu;
    public map<@string, ptr<objfile.Disasm>> disasmCache;
}

private static (driver.ObjFile, error) Open(this ptr<objTool> _addr__p0, @string name, ulong start, ulong limit, ulong offset) {
    driver.ObjFile _p0 = default;
    error _p0 = default!;
    ref objTool _p0 = ref _addr__p0.val;

    var (of, err) = objfile.Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ptr<file> f = addr(new file(name:name,file:of,));
    if (start != 0) {
        {
            var (load, err) = of.LoadAddress();

            if (err == null) {
                f.offset = start - load;
            }

        }

    }
    return (f, error.As(null!)!);

}

private static (map<@string, @string>, error) Demangle(this ptr<objTool> _addr__p0, slice<@string> names) {
    map<@string, @string> _p0 = default;
    error _p0 = default!;
    ref objTool _p0 = ref _addr__p0.val;
 
    // No C++, nothing to demangle.
    return (make_map<@string, @string>(), error.As(null!)!);

}

private static (slice<driver.Inst>, error) Disasm(this ptr<objTool> _addr_t, @string file, ulong start, ulong end, bool intelSyntax) {
    slice<driver.Inst> _p0 = default;
    error _p0 = default!;
    ref objTool t = ref _addr_t.val;

    if (intelSyntax) {
        return (null, error.As(fmt.Errorf("printing assembly in Intel syntax is not supported"))!);
    }
    var (d, err) = t.cachedDisasm(file);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<driver.Inst> asm = default;
    d.Decode(start, end, null, false, (pc, size, file, line, text) => {
        asm = append(asm, new driver.Inst(Addr:pc,File:file,Line:line,Text:text));
    });
    return (asm, error.As(null!)!);

}

private static (ptr<objfile.Disasm>, error) cachedDisasm(this ptr<objTool> _addr_t, @string file) => func((defer, _, _) => {
    ptr<objfile.Disasm> _p0 = default!;
    error _p0 = default!;
    ref objTool t = ref _addr_t.val;

    t.mu.Lock();
    defer(t.mu.Unlock());
    if (t.disasmCache == null) {
        t.disasmCache = make_map<@string, ptr<objfile.Disasm>>();
    }
    var d = t.disasmCache[file];
    if (d != null) {
        return (_addr_d!, error.As(null!)!);
    }
    var (f, err) = objfile.Open(file);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    d, err = f.Disasm();
    f.Close();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    t.disasmCache[file] = d;
    return (_addr_d!, error.As(null!)!);

});

private static void SetConfig(this ptr<objTool> _addr__p0, @string config) {
    ref objTool _p0 = ref _addr__p0.val;
 
    // config is usually used to say what binaries to invoke.
    // Ignore entirely.
}

// file implements driver.ObjFile using Go libraries
// (instead of invoking GNU binutils).
// A file represents a single executable being analyzed.
private partial struct file {
    public @string name;
    public ulong offset;
    public slice<objfile.Sym> sym;
    public ptr<objfile.File> file;
    public objfile.Liner pcln;
    public bool triedDwarf;
    public ptr<dwarf.Data> dwarf;
}

private static @string Name(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    return f.name;
}

private static (ulong, error) ObjAddr(this ptr<file> _addr_f, ulong addr) {
    ulong _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;
 
    // No support for shared libraries, so translation is a no-op.
    return (addr, error.As(null!)!);

}

private static @string BuildID(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;
 
    // No support for build ID.
    return "";

}

private static (slice<driver.Frame>, error) SourceLine(this ptr<file> _addr_f, ulong addr) {
    slice<driver.Frame> _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;

    if (f.pcln == null) {
        var (pcln, err) = f.file.PCLineTable();
        if (err != null) {
            return (null, error.As(err)!);
        }
        f.pcln = pcln;

    }
    addr -= f.offset;
    var (file, line, fn) = f.pcln.PCToLine(addr);
    if (fn != null) {
        driver.Frame frame = new slice<driver.Frame>(new driver.Frame[] { {Func:fn.Name,File:file,Line:line,} });
        return (frame, error.As(null!)!);
    }
    var frames = f.dwarfSourceLine(addr);
    if (frames != null) {
        return (frames, error.As(null!)!);
    }
    return (null, error.As(fmt.Errorf("no line information for PC=%#x", addr))!);

}

// dwarfSourceLine tries to get file/line information using DWARF.
// This is for C functions that appear in the profile.
// Returns nil if there is no information available.
private static slice<driver.Frame> dwarfSourceLine(this ptr<file> _addr_f, ulong addr) {
    ref file f = ref _addr_f.val;

    if (f.dwarf == null && !f.triedDwarf) { 
        // Ignore any error--we don't care exactly why there
        // is no DWARF info.
        f.dwarf, _ = f.file.DWARF();
        f.triedDwarf = true;

    }
    if (f.dwarf != null) {
        var r = f.dwarf.Reader();
        var (unit, err) = r.SeekPC(addr);
        if (err == null) {
            {
                var frames = f.dwarfSourceLineEntry(r, unit, addr);

                if (frames != null) {
                    return frames;
                }

            }

        }
    }
    return null;

}

// dwarfSourceLineEntry tries to get file/line information from a
// DWARF compilation unit. Returns nil if it doesn't find anything.
private static slice<driver.Frame> dwarfSourceLineEntry(this ptr<file> _addr_f, ptr<dwarf.Reader> _addr_r, ptr<dwarf.Entry> _addr_entry, ulong addr) {
    ref file f = ref _addr_f.val;
    ref dwarf.Reader r = ref _addr_r.val;
    ref dwarf.Entry entry = ref _addr_entry.val;

    var (lines, err) = f.dwarf.LineReader(entry);
    if (err != null) {
        return null;
    }
    ref dwarf.LineEntry lentry = ref heap(out ptr<dwarf.LineEntry> _addr_lentry);
    {
        var err = lines.SeekPC(addr, _addr_lentry);

        if (err != null) {
            return null;
        }
    } 

    // Try to find the function name.
    @string name = "";
FindName: 

    // TODO: Report inlined functions.

    {
        var (entry, err) = r.Next();

        while (entry != null && err == null) {
            if (entry.Tag == dwarf.TagSubprogram) {
                var (ranges, err) = f.dwarf.Ranges(entry);
                if (err != null) {
                    return null;
            entry, err = r.Next();
                }

                foreach (var (_, pcs) in ranges) {
                    if (pcs[0] <= addr && addr < pcs[1]) {
                        bool ok = default; 
                        // TODO: AT_linkage_name, AT_MIPS_linkage_name.
                        name, ok = entry.Val(dwarf.AttrName)._<@string>();
                        if (ok) {
                            _breakFindName = true;
                            break;
                        }

                    }

                }

            }

        }
    } 

    // TODO: Report inlined functions.
    driver.Frame frames = new slice<driver.Frame>(new driver.Frame[] { {Func:name,File:lentry.File.Name,Line:lentry.Line,} });

    return frames;

}

private static (slice<ptr<driver.Sym>>, error) Symbols(this ptr<file> _addr_f, ptr<regexp.Regexp> _addr_r, ulong addr) {
    slice<ptr<driver.Sym>> _p0 = default;
    error _p0 = default!;
    ref file f = ref _addr_f.val;
    ref regexp.Regexp r = ref _addr_r.val;

    if (f.sym == null) {
        var (sym, err) = f.file.Symbols();
        if (err != null) {
            return (null, error.As(err)!);
        }
        f.sym = sym;

    }
    slice<ptr<driver.Sym>> @out = default;
    foreach (var (_, s) in f.sym) { 
        // Ignore a symbol with address 0 and size 0.
        // An ELF STT_FILE symbol will look like that.
        if (s.Addr == 0 && s.Size == 0) {
            continue;
        }
        if ((r == null || r.MatchString(s.Name)) && (addr == 0 || s.Addr <= addr && addr < s.Addr + uint64(s.Size))) {
            out = append(out, addr(new driver.Sym(Name:[]string{s.Name},File:f.name,Start:s.Addr,End:s.Addr+uint64(s.Size)-1,)));
        }
    }    return (out, error.As(null!)!);

}

private static error Close(this ptr<file> _addr_f) {
    ref file f = ref _addr_f.val;

    f.file.Close();
    return error.As(null!)!;
}

// newUI will be set in readlineui.go in some platforms
// for interactive readline functionality.
private static Func<driver.UI> newUI = () => null;

} // end main_package
