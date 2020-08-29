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

// package binutils -- go2cs converted at 2020 August 29 10:05:13 UTC
// import "cmd/vendor/github.com/google/pprof/internal/binutils" ==> using binutils = go.cmd.vendor.github.com.google.pprof.@internal.binutils_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\disasm.go
using bytes = go.bytes_package;
using io = go.io_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
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
    public static partial class binutils_package
    {
        private static var nmOutputRE = regexp.MustCompile("^\\s*([[:xdigit:]]+)\\s+(.)\\s+(.*)");        private static var objdumpAsmOutputRE = regexp.MustCompile("^\\s*([[:xdigit:]]+):\\s+(.*)");        private static var objdumpOutputFileLine = regexp.MustCompile("^(.*):([0-9]+)");        private static var objdumpOutputFunction = regexp.MustCompile("^(\\S.*)\\(\\):");

        private static (slice<ref plugin.Sym>, error) findSymbols(slice<byte> syms, @string file, ref regexp.Regexp r, ulong address)
        { 
            // Collect all symbols from the nm output, grouping names mapped to
            // the same address into a single symbol.
            slice<ref plugin.Sym> symbols = default;
            @string names = new slice<@string>(new @string[] {  });
            var start = uint64(0L);
            var buf = bytes.NewBuffer(syms);
            {
                var (symAddr, name, err) = nextSymbol(buf);

                while (err == null)
                {
                    if (err != null)
                    {
                        return (null, err);
                    symAddr, name, err = nextSymbol(buf);
                    }
                    if (start == symAddr)
                    {
                        names = append(names, name);
                        continue;
                    }
                    {
                        var match = matchSymbol(names, start, symAddr - 1L, r, address);

                        if (match != null)
                        {
                            symbols = append(symbols, ref new plugin.Sym(Name:match,File:file,Start:start,End:symAddr-1));
                        }

                    }
                    names = new slice<@string>(new @string[] { name });
                    start = symAddr;
                }

            }

            return (symbols, null);
        }

        // matchSymbol checks if a symbol is to be selected by checking its
        // name to the regexp and optionally its address. It returns the name(s)
        // to be used for the matched symbol, or nil if no match
        private static slice<@string> matchSymbol(slice<@string> names, ulong start, ulong end, ref regexp.Regexp r, ulong address)
        {
            if (address != 0L && address >= start && address <= end)
            {
                return names;
            }
            foreach (var (_, name) in names)
            {
                if (r.MatchString(name))
                {
                    return new slice<@string>(new @string[] { name });
                } 

                // Match all possible demangled versions of the name.
                foreach (var (_, o) in new slice<slice<demangle.Option>>(new slice<demangle.Option>[] { {demangle.NoClones}, {demangle.NoParams}, {demangle.NoParams,demangle.NoTemplateParams} }))
                {
                    {
                        var (demangled, err) = demangle.ToString(name, o);

                        if (err == null && r.MatchString(demangled))
                        {
                            return new slice<@string>(new @string[] { demangled });
                        }

                    }
                }
            }
            return null;
        }

        // disassemble parses the output of the objdump command and returns
        // the assembly instructions in a slice.
        private static (slice<plugin.Inst>, error) disassemble(slice<byte> asm)
        {
            var buf = bytes.NewBuffer(asm);
            @string function = "";
            @string file = "";
            long line = 0L;
            slice<plugin.Inst> assembly = default;
            while (true)
            {
                var (input, err) = buf.ReadString('\n');
                if (err != null)
                {
                    if (err != io.EOF)
                    {
                        return (null, err);
                    }
                    if (input == "")
                    {
                        break;
                    }
                }
                {
                    var fields__prev1 = fields;

                    var fields = objdumpAsmOutputRE.FindStringSubmatch(input);

                    if (len(fields) == 3L)
                    {
                        {
                            var (address, err) = strconv.ParseUint(fields[1L], 16L, 64L);

                            if (err == null)
                            {
                                assembly = append(assembly, new plugin.Inst(Addr:address,Text:fields[2],Function:function,File:file,Line:line,));
                                continue;
                            }

                        }
                    }

                    fields = fields__prev1;

                }
                {
                    var fields__prev1 = fields;

                    fields = objdumpOutputFileLine.FindStringSubmatch(input);

                    if (len(fields) == 3L)
                    {
                        {
                            var (l, err) = strconv.ParseUint(fields[2L], 10L, 32L);

                            if (err == null)
                            {
                                file = fields[1L];
                                line = int(l);
                            }

                        }
                        continue;
                    }

                    fields = fields__prev1;

                }
                {
                    var fields__prev1 = fields;

                    fields = objdumpOutputFunction.FindStringSubmatch(input);

                    if (len(fields) == 2L)
                    {
                        function = fields[1L];
                        continue;
                    } 
                    // Reset on unrecognized lines.

                    fields = fields__prev1;

                } 
                // Reset on unrecognized lines.
                function = "";
                file = "";
                line = 0L;
            }


            return (assembly, null);
        }

        // nextSymbol parses the nm output to find the next symbol listed.
        // Skips over any output it cannot recognize.
        private static (ulong, @string, error) nextSymbol(ref bytes.Buffer buf)
        {
            while (true)
            {
                var (line, err) = buf.ReadString('\n');
                if (err != null)
                {
                    if (err != io.EOF || line == "")
                    {
                        return (0L, "", err);
                    }
                }
                {
                    var fields = nmOutputRE.FindStringSubmatch(line);

                    if (len(fields) == 4L)
                    {
                        {
                            var (address, err) = strconv.ParseUint(fields[1L], 16L, 64L);

                            if (err == null)
                            {
                                return (address, fields[3L], null);
                            }

                        }
                    }

                }
            }

        }
    }
}}}}}}}
