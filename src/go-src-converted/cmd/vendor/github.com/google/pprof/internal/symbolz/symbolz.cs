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

// Package symbolz symbolizes a profile using the output from the symbolz
// service.
// package symbolz -- go2cs converted at 2020 October 09 05:53:49 UTC
// import "cmd/vendor/github.com/google/pprof/internal/symbolz" ==> using symbolz = go.cmd.vendor.github.com.google.pprof.@internal.symbolz_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\symbolz\symbolz.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using url = go.net.url_package;
using path = go.path_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
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
    public static partial class symbolz_package
    {
        private static var symbolzRE = regexp.MustCompile("(0x[[:xdigit:]]+)\\s+(.*)");

        // Symbolize symbolizes profile p by parsing data returned by a symbolz
        // handler. syms receives the symbolz query (hex addresses separated by '+')
        // and returns the symbolz output in a string. If force is false, it will only
        // symbolize locations from mappings not already marked as HasFunctions. Never
        // attempts symbolization of addresses from unsymbolizable system
        // mappings as those may look negative - e.g. "[vsyscall]".
        public static error Symbolize(ptr<profile.Profile> _addr_p, bool force, plugin.MappingSources sources, Func<@string, @string, (slice<byte>, error)> syms, plugin.UI ui)
        {
            ref profile.Profile p = ref _addr_p.val;

            foreach (var (_, m) in p.Mapping)
            {
                if (!force && m.HasFunctions)
                { 
                    // Only check for HasFunctions as symbolz only populates function names.
                    continue;

                } 
                // Skip well-known system mappings.
                if (m.Unsymbolizable())
                {
                    continue;
                }

                var mappingSources = sources[m.File];
                if (m.BuildID != "")
                {
                    mappingSources = append(mappingSources, sources[m.BuildID]);
                }

                foreach (var (_, source) in mappingSources)
                {
                    {
                        var symz = symbolz(source.Source);

                        if (symz != "")
                        {
                            {
                                var err = symbolizeMapping(symz, int64(source.Start) - int64(m.Start), syms, _addr_m, _addr_p);

                                if (err != null)
                                {
                                    return error.As(err)!;
                                }

                            }

                            m.HasFunctions = true;
                            break;

                        }

                    }

                }

            }
            return error.As(null!)!;

        }

        // hasGperftoolsSuffix checks whether path ends with one of the suffixes listed in
        // pprof_remote_servers.html from the gperftools distribution
        private static bool hasGperftoolsSuffix(@string path)
        {
            @string suffixes = new slice<@string>(new @string[] { "/pprof/heap", "/pprof/growth", "/pprof/profile", "/pprof/pmuprofile", "/pprof/contention" });
            foreach (var (_, s) in suffixes)
            {
                if (strings.HasSuffix(path, s))
                {
                    return true;
                }

            }
            return false;

        }

        // symbolz returns the corresponding symbolz source for a profile URL.
        private static @string symbolz(@string source)
        {
            {
                var (url, err) = url.Parse(source);

                if (err == null && url.Host != "")
                { 
                    // All paths in the net/http/pprof Go package contain /debug/pprof/
                    if (strings.Contains(url.Path, "/debug/pprof/") || hasGperftoolsSuffix(url.Path))
                    {
                        url.Path = path.Clean(url.Path + "/../symbol");
                    }
                    else
                    {
                        url.Path = "/symbolz";
                    }

                    url.RawQuery = "";
                    return url.String();

                }

            }


            return "";

        }

        // symbolizeMapping symbolizes locations belonging to a Mapping by querying
        // a symbolz handler. An offset is applied to all addresses to take care of
        // normalization occurred for merged Mappings.
        private static error symbolizeMapping(@string source, long offset, Func<@string, @string, (slice<byte>, error)> syms, ptr<profile.Mapping> _addr_m, ptr<profile.Profile> _addr_p)
        {
            ref profile.Mapping m = ref _addr_m.val;
            ref profile.Profile p = ref _addr_p.val;
 
            // Construct query of addresses to symbolize.
            slice<@string> a = default;
            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    if (l.Mapping == m && l.Address != 0L && len(l.Line) == 0L)
                    { 
                        // Compensate for normalization.
                        var (addr, overflow) = adjust(l.Address, offset);
                        if (overflow)
                        {
                            return error.As(fmt.Errorf("cannot adjust address %d by %d, it would overflow (mapping %v)", l.Address, offset, l.Mapping))!;
                        }

                        a = append(a, fmt.Sprintf("%#x", addr));

                    }

                }

                l = l__prev1;
            }

            if (len(a) == 0L)
            { 
                // No addresses to symbolize.
                return error.As(null!)!;

            }

            var lines = make_map<ulong, profile.Line>();
            var functions = make_map<@string, ptr<profile.Function>>();

            var (b, err) = syms(source, strings.Join(a, "+"));
            if (err != null)
            {
                return error.As(err)!;
            }

            var buf = bytes.NewBuffer(b);
            while (true)
            {
                var (l, err) = buf.ReadString('\n');

                if (err != null)
                {
                    if (err == io.EOF)
                    {
                        break;
                    }

                    return error.As(err)!;

                }

                {
                    var symbol = symbolzRE.FindStringSubmatch(l);

                    if (len(symbol) == 3L)
                    {
                        var (origAddr, err) = strconv.ParseUint(symbol[1L], 0L, 64L);
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("unexpected parse failure %s: %v", symbol[1L], err))!;
                        } 
                        // Reapply offset expected by the profile.
                        (addr, overflow) = adjust(origAddr, -offset);
                        if (overflow)
                        {
                            return error.As(fmt.Errorf("cannot adjust symbolz address %d by %d, it would overflow", origAddr, -offset))!;
                        }

                        var name = symbol[2L];
                        var fn = functions[name];
                        if (fn == null)
                        {
                            fn = addr(new profile.Function(ID:uint64(len(p.Function)+1),Name:name,SystemName:name,));
                            functions[name] = fn;
                            p.Function = append(p.Function, fn);
                        }

                        lines[addr] = new profile.Line(Function:fn);

                    }

                }

            }


            {
                var l__prev1 = l;

                foreach (var (_, __l) in p.Location)
                {
                    l = __l;
                    if (l.Mapping != m)
                    {
                        continue;
                    }

                    {
                        var (line, ok) = lines[l.Address];

                        if (ok)
                        {
                            l.Line = new slice<profile.Line>(new profile.Line[] { line });
                        }

                    }

                }

                l = l__prev1;
            }

            return error.As(null!)!;

        }

        // adjust shifts the specified address by the signed offset. It returns the
        // adjusted address. It signals that the address cannot be adjusted without an
        // overflow by returning true in the second return value.
        private static (ulong, bool) adjust(ulong addr, long offset)
        {
            ulong _p0 = default;
            bool _p0 = default;

            var adj = uint64(int64(addr) + offset);
            if (offset < 0L)
            {
                if (adj >= addr)
                {
                    return (0L, true);
                }

            }
            else
            {
                if (adj < addr)
                {
                    return (0L, true);
                }

            }

            return (adj, false);

        }
    }
}}}}}}}
