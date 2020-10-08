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
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\addr2liner_llvm.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;

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
        private static readonly @string defaultLLVMSymbolizer = (@string)"llvm-symbolizer";


        // llvmSymbolizer is a connection to an llvm-symbolizer command for
        // obtaining address and line number information from a binary.
        private partial struct llvmSymbolizer
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public @string filename;
            public lineReaderWriter rw;
            public ulong @base;
        }

        private partial struct llvmSymbolizerJob
        {
            public ptr<exec.Cmd> cmd;
            public io.WriteCloser @in;
            public ptr<bufio.Reader> @out;
        }

        private static error write(this ptr<llvmSymbolizerJob> _addr_a, @string s)
        {
            ref llvmSymbolizerJob a = ref _addr_a.val;

            var (_, err) = fmt.Fprint(a.@in, s + "\n");
            return error.As(err)!;
        }

        private static (@string, error) readLine(this ptr<llvmSymbolizerJob> _addr_a)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref llvmSymbolizerJob a = ref _addr_a.val;

            return a.@out.ReadString('\n');
        }

        // close releases any resources used by the llvmSymbolizer object.
        private static void close(this ptr<llvmSymbolizerJob> _addr_a)
        {
            ref llvmSymbolizerJob a = ref _addr_a.val;

            a.@in.Close();
            a.cmd.Wait();
        }

        // newLlvmSymbolizer starts the given llvmSymbolizer command reporting
        // information about the given executable file. If file is a shared
        // library, base should be the address at which it was mapped in the
        // program under consideration.
        private static (ptr<llvmSymbolizer>, error) newLLVMSymbolizer(@string cmd, @string file, ulong @base)
        {
            ptr<llvmSymbolizer> _p0 = default!;
            error _p0 = default!;

            if (cmd == "")
            {
                cmd = defaultLLVMSymbolizer;
            }

            ptr<llvmSymbolizerJob> j = addr(new llvmSymbolizerJob(cmd:exec.Command(cmd,"-inlining","-demangle=false"),));

            error err = default!;
            j.@in, err = j.cmd.StdinPipe();

            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (outPipe, err) = j.cmd.StdoutPipe();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            j.@out = bufio.NewReader(outPipe);
            {
                error err__prev1 = err;

                err = j.cmd.Start();

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }


            ptr<llvmSymbolizer> a = addr(new llvmSymbolizer(filename:file,rw:j,base:base,));

            return (_addr_a!, error.As(null!)!);

        }

        private static (@string, error) readString(this ptr<llvmSymbolizer> _addr_d)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref llvmSymbolizer d = ref _addr_d.val;

            var (s, err) = d.rw.readLine();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (strings.TrimSpace(s), error.As(null!)!);

        }

        // readFrame parses the llvm-symbolizer output for a single address. It
        // returns a populated plugin.Frame and whether it has reached the end of the
        // data.
        private static (plugin.Frame, bool) readFrame(this ptr<llvmSymbolizer> _addr_d)
        {
            plugin.Frame _p0 = default;
            bool _p0 = default;
            ref llvmSymbolizer d = ref _addr_d.val;

            var (funcname, err) = d.readString();
            if (err != null)
            {
                return (new plugin.Frame(), true);
            }

            switch (funcname)
            {
                case "": 
                    return (new plugin.Frame(), true);
                    break;
                case "??": 
                    funcname = "";
                    break;
            }

            var (fileline, err) = d.readString();
            if (err != null)
            {
                return (new plugin.Frame(Func:funcname), true);
            }

            long linenumber = 0L;
            if (fileline == "??:0")
            {
                fileline = "";
            }
            else
            {
                {
                    var split = strings.Split(fileline, ":");

                    switch (len(split))
                    {
                        case 1L: 
                            // filename
                            fileline = split[0L];
                            break;
                        case 2L: 
                            // filename:line , or
                            // filename:line:disc , or

                        case 3L: 
                            // filename:line , or
                            // filename:line:disc , or
                            fileline = split[0L];
                            {
                                var (line, err) = strconv.Atoi(split[1L]);

                                if (err == null)
                                {
                                    linenumber = line;
                                }

                            }

                            break;
                        default: 
                            break;
                    }
                }

            }

            return (new plugin.Frame(Func:funcname,File:fileline,Line:linenumber), false);

        }

        // addrInfo returns the stack frame information for a specific program
        // address. It returns nil if the address could not be identified.
        private static (slice<plugin.Frame>, error) addrInfo(this ptr<llvmSymbolizer> _addr_d, ulong addr) => func((defer, _, __) =>
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref llvmSymbolizer d = ref _addr_d.val;

            d.Lock();
            defer(d.Unlock());

            {
                var err = d.rw.write(fmt.Sprintf("%s 0x%x", d.filename, addr - d.@base));

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            slice<plugin.Frame> stack = default;
            while (true)
            {
                var (frame, end) = d.readFrame();
                if (end)
                {
                    break;
                }

                if (frame != (new plugin.Frame()))
                {
                    stack = append(stack, frame);
                }

            }


            return (stack, error.As(null!)!);

        });
    }
}}}}}}}
