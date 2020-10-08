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

// package binutils -- go2cs converted at 2020 October 08 04:42:49 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\addr2liner_nm.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using io = go.io_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class binutils_package
    {
        private static readonly @string defaultNM = (@string)"nm";


        // addr2LinerNM is a connection to an nm command for obtaining address
        // information from a binary.
        private partial struct addr2LinerNM
        {
            public slice<symbolInfo> m; // Sorted list of addresses from binary.
        }

        private partial struct symbolInfo
        {
            public ulong address;
            public @string name;
        }

        //  newAddr2LinerNM starts the given nm command reporting information about the
        // given executable file. If file is a shared library, base should be
        // the address at which it was mapped in the program under
        // consideration.
        private static (ptr<addr2LinerNM>, error) newAddr2LinerNM(@string cmd, @string file, ulong @base)
        {
            ptr<addr2LinerNM> _p0 = default!;
            error _p0 = default!;

            if (cmd == "")
            {
                cmd = defaultNM;
            }

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            var c = exec.Command(cmd, "-n", file);
            _addr_c.Stdout = _addr_b;
            c.Stdout = ref _addr_c.Stdout.val;
            {
                var err = c.Run();

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            return _addr_parseAddr2LinerNM(base, _addr_b)!;

        }

        private static (ptr<addr2LinerNM>, error) parseAddr2LinerNM(ulong @base, io.Reader nm)
        {
            ptr<addr2LinerNM> _p0 = default!;
            error _p0 = default!;

            ptr<addr2LinerNM> a = addr(new addr2LinerNM(m:[]symbolInfo{},)); 

            // Parse nm output and populate symbol map.
            // Skip lines we fail to parse.
            var buf = bufio.NewReader(nm);
            while (true)
            {
                var (line, err) = buf.ReadString('\n');
                if (line == "" && err != null)
                {
                    if (err == io.EOF)
                    {
                        break;
                    }

                    return (_addr_null!, error.As(err)!);

                }

                line = strings.TrimSpace(line);
                var fields = strings.SplitN(line, " ", 3L);
                if (len(fields) != 3L)
                {
                    continue;
                }

                var (address, err) = strconv.ParseUint(fields[0L], 16L, 64L);
                if (err != null)
                {
                    continue;
                }

                a.m = append(a.m, new symbolInfo(address:address+base,name:fields[2],));

            }


            return (_addr_a!, error.As(null!)!);

        }

        // addrInfo returns the stack frame information for a specific program
        // address. It returns nil if the address could not be identified.
        private static (slice<plugin.Frame>, error) addrInfo(this ptr<addr2LinerNM> _addr_a, ulong addr)
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref addr2LinerNM a = ref _addr_a.val;

            if (len(a.m) == 0L || addr < a.m[0L].address || addr > a.m[len(a.m) - 1L].address)
            {
                return (null, error.As(null!)!);
            } 

            // Binary search. Search until low, high are separated by 1.
            long low = 0L;
            var high = len(a.m);
            while (low + 1L < high)
            {
                var mid = (low + high) / 2L;
                var v = a.m[mid].address;
                if (addr == v)
                {
                    low = mid;
                    break;
                }
                else if (addr > v)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }

            } 

            // Address is between a.m[low] and a.m[high].
            // Pick low, as it represents [low, high).
 

            // Address is between a.m[low] and a.m[high].
            // Pick low, as it represents [low, high).
            plugin.Frame f = new slice<plugin.Frame>(new plugin.Frame[] { {Func:a.m[low].name,} });
            return (f, error.As(null!)!);

        }
    }
}}}}}}}
