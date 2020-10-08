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
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\binutils\addr2liner.go
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
        private static readonly @string defaultAddr2line = (@string)"addr2line"; 

        // addr2line may produce multiple lines of output. We
        // use this sentinel to identify the end of the output.
        private static readonly var sentinel = (var)~uint64(0L);


        // addr2Liner is a connection to an addr2line command for obtaining
        // address and line number information from a binary.
        private partial struct addr2Liner
        {
            public sync.Mutex mu;
            public lineReaderWriter rw;
            public ulong @base; // nm holds an addr2Liner using nm tool. Certain versions of addr2line
// produce incomplete names due to
// https://sourceware.org/bugzilla/show_bug.cgi?id=17541. As a workaround,
// the names from nm are used when they look more complete. See addrInfo()
// code below for the exact heuristic.
            public ptr<addr2LinerNM> nm;
        }

        // lineReaderWriter is an interface to abstract the I/O to an addr2line
        // process. It writes a line of input to the job, and reads its output
        // one line at a time.
        private partial interface lineReaderWriter
        {
            (@string, error) write(@string _p0);
            (@string, error) readLine();
            (@string, error) close();
        }

        private partial struct addr2LinerJob
        {
            public ptr<exec.Cmd> cmd;
            public io.WriteCloser @in;
            public ptr<bufio.Reader> @out;
        }

        private static error write(this ptr<addr2LinerJob> _addr_a, @string s)
        {
            ref addr2LinerJob a = ref _addr_a.val;

            var (_, err) = fmt.Fprint(a.@in, s + "\n");
            return error.As(err)!;
        }

        private static (@string, error) readLine(this ptr<addr2LinerJob> _addr_a)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref addr2LinerJob a = ref _addr_a.val;

            return a.@out.ReadString('\n');
        }

        // close releases any resources used by the addr2liner object.
        private static void close(this ptr<addr2LinerJob> _addr_a)
        {
            ref addr2LinerJob a = ref _addr_a.val;

            a.@in.Close();
            a.cmd.Wait();
        }

        // newAddr2liner starts the given addr2liner command reporting
        // information about the given executable file. If file is a shared
        // library, base should be the address at which it was mapped in the
        // program under consideration.
        private static (ptr<addr2Liner>, error) newAddr2Liner(@string cmd, @string file, ulong @base)
        {
            ptr<addr2Liner> _p0 = default!;
            error _p0 = default!;

            if (cmd == "")
            {
                cmd = defaultAddr2line;
            }

            ptr<addr2LinerJob> j = addr(new addr2LinerJob(cmd:exec.Command(cmd,"-aif","-e",file),));

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


            ptr<addr2Liner> a = addr(new addr2Liner(rw:j,base:base,));

            return (_addr_a!, error.As(null!)!);

        }

        private static (@string, error) readString(this ptr<addr2Liner> _addr_d)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref addr2Liner d = ref _addr_d.val;

            var (s, err) = d.rw.readLine();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (strings.TrimSpace(s), error.As(null!)!);

        }

        // readFrame parses the addr2line output for a single address. It
        // returns a populated plugin.Frame and whether it has reached the end of the
        // data.
        private static (plugin.Frame, bool) readFrame(this ptr<addr2Liner> _addr_d)
        {
            plugin.Frame _p0 = default;
            bool _p0 = default;
            ref addr2Liner d = ref _addr_d.val;

            var (funcname, err) = d.readString();
            if (err != null)
            {
                return (new plugin.Frame(), true);
            }

            if (strings.HasPrefix(funcname, "0x"))
            { 
                // If addr2line returns a hex address we can assume it is the
                // sentinel. Read and ignore next two lines of output from
                // addr2line
                d.readString();
                d.readString();
                return (new plugin.Frame(), true);

            }

            var (fileline, err) = d.readString();
            if (err != null)
            {
                return (new plugin.Frame(), true);
            }

            long linenumber = 0L;

            if (funcname == "??")
            {
                funcname = "";
            }

            if (fileline == "??:0")
            {
                fileline = "";
            }
            else
            {
                {
                    var i = strings.LastIndex(fileline, ":");

                    if (i >= 0L)
                    { 
                        // Remove discriminator, if present
                        {
                            var disc = strings.Index(fileline, " (discriminator");

                            if (disc > 0L)
                            {
                                fileline = fileline[..disc];
                            } 
                            // If we cannot parse a number after the last ":", keep it as
                            // part of the filename.

                        } 
                        // If we cannot parse a number after the last ":", keep it as
                        // part of the filename.
                        {
                            var (line, err) = strconv.Atoi(fileline[i + 1L..]);

                            if (err == null)
                            {
                                linenumber = line;
                                fileline = fileline[..i];
                            }

                        }

                    }

                }

            }

            return (new plugin.Frame(Func:funcname,File:fileline,Line:linenumber), false);

        }

        private static (slice<plugin.Frame>, error) rawAddrInfo(this ptr<addr2Liner> _addr_d, ulong addr) => func((defer, _, __) =>
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref addr2Liner d = ref _addr_d.val;

            d.mu.Lock();
            defer(d.mu.Unlock());

            {
                var err__prev1 = err;

                var err = d.rw.write(fmt.Sprintf("%x", addr - d.@base));

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }


            {
                var err__prev1 = err;

                err = d.rw.write(fmt.Sprintf("%x", sentinel));

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }


            var (resp, err) = d.readString();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (!strings.HasPrefix(resp, "0x"))
            {
                return (null, error.As(fmt.Errorf("unexpected addr2line output: %s", resp))!);
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

            return (stack, error.As(err)!);

        });

        // addrInfo returns the stack frame information for a specific program
        // address. It returns nil if the address could not be identified.
        private static (slice<plugin.Frame>, error) addrInfo(this ptr<addr2Liner> _addr_d, ulong addr)
        {
            slice<plugin.Frame> _p0 = default;
            error _p0 = default!;
            ref addr2Liner d = ref _addr_d.val;

            var (stack, err) = d.rawAddrInfo(addr);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Certain versions of addr2line produce incomplete names due to
            // https://sourceware.org/bugzilla/show_bug.cgi?id=17541. Attempt to replace
            // the name with a better one from nm.
            if (len(stack) > 0L && d.nm != null)
            {
                var (nm, err) = d.nm.addrInfo(addr);
                if (err == null && len(nm) > 0L)
                { 
                    // Last entry in frame list should match since it is non-inlined. As a
                    // simple heuristic, we only switch to the nm-based name if it is longer
                    // by 2 or more characters. We consider nm names that are longer by 1
                    // character insignificant to avoid replacing foo with _foo on MacOS (for
                    // unknown reasons read2line produces the former and nm produces the
                    // latter on MacOS even though both tools are asked to produce mangled
                    // names).
                    var nmName = nm[len(nm) - 1L].Func;
                    var a2lName = stack[len(stack) - 1L].Func;
                    if (len(nmName) > len(a2lName) + 1L)
                    {
                        stack[len(stack) - 1L].Func = nmName;
                    }

                }

            }

            return (stack, error.As(null!)!);

        }
    }
}}}}}}}
