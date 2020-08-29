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
// package symbolz -- go2cs converted at 2020 August 29 10:06:14 UTC
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

        // Symbolize symbolizes profile p by parsing data returned by a
        // symbolz handler. syms receives the symbolz query (hex addresses
        // separated by '+') and returns the symbolz output in a string. If
        // force is false, it will only symbolize locations from mappings
        // not already marked as HasFunctions.
        public static error Symbolize(ref profile.Profile p, bool force, plugin.MappingSources sources, Func<@string, @string, (slice<byte>, error)> syms, plugin.UI ui)
        {
            foreach (var (_, m) in p.Mapping)
            {
                if (!force && m.HasFunctions)
                { 
                    // Only check for HasFunctions as symbolz only populates function names.
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
                                var err = symbolizeMapping(symz, int64(source.Start) - int64(m.Start), syms, m, p);

                                if (err != null)
                                {
                                    return error.As(err);
                                }

                            }
                            m.HasFunctions = true;
                            break;
                        }

                    }
                }
            }
            return error.As(null);
        }

        // symbolz returns the corresponding symbolz source for a profile URL.
        private static @string symbolz(@string source)
        {
            {
                var (url, err) = url.Parse(source);

                if (err == null && url.Host != "")
                {
                    if (strings.Contains(url.Path, "/debug/pprof/"))
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
        private static error symbolizeMapping(@string source, long offset, Func<@string, @string, (slice<byte>, error)> syms, ref profile.Mapping m, ref profile.Profile p)
        { 
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
                        var addr = int64(l.Address) + offset;
                        if (addr < 0L)
                        {
                            return error.As(fmt.Errorf("unexpected negative adjusted address, mapping %v source %d, offset %d", l.Mapping, l.Address, offset));
                        }
                        a = append(a, fmt.Sprintf("%#x", addr));
                    }
                }

                l = l__prev1;
            }

            if (len(a) == 0L)
            { 
                // No addresses to symbolize.
                return error.As(null);
            }
            var lines = make_map<ulong, profile.Line>();
            var functions = make_map<@string, ref profile.Function>();

            var (b, err) = syms(source, strings.Join(a, "+"));
            if (err != null)
            {
                return error.As(err);
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
                    return error.As(err);
                }
                {
                    var symbol = symbolzRE.FindStringSubmatch(l);

                    if (len(symbol) == 3L)
                    {
                        var (addr, err) = strconv.ParseInt(symbol[1L], 0L, 64L);
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("unexpected parse failure %s: %v", symbol[1L], err));
                        }
                        if (addr < 0L)
                        {
                            return error.As(fmt.Errorf("unexpected negative adjusted address, source %s, offset %d", symbol[1L], offset));
                        } 
                        // Reapply offset expected by the profile.
                        addr -= offset;

                        var name = symbol[2L];
                        var fn = functions[name];
                        if (fn == null)
                        {
                            fn = ref new profile.Function(ID:uint64(len(p.Function)+1),Name:name,SystemName:name,);
                            functions[name] = fn;
                            p.Function = append(p.Function, fn);
                        }
                        lines[uint64(addr)] = new profile.Line(Function:fn);
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

            return error.As(null);
        }
    }
}}}}}}}
