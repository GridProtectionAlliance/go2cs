// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:48 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\numcpu_freebsd.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var cpuSetRE = regexp.MustCompile("(\\d,?)+");

        private static void init()
        {
            register("FreeBSDNumCPU", FreeBSDNumCPU);
            register("FreeBSDNumCPUHelper", FreeBSDNumCPUHelper);
        }

        public static void FreeBSDNumCPUHelper()
        {
            fmt.Printf("%d\n", runtime.NumCPU());
        }

        public static void FreeBSDNumCPU()
        {
            var (_, err) = exec.LookPath("cpuset");
            if (err != null)
            { 
                // Can not test without cpuset command.
                fmt.Println("OK");
                return ;

            }

            _, err = exec.LookPath("sysctl");
            if (err != null)
            { 
                // Can not test without sysctl command.
                fmt.Println("OK");
                return ;

            }

            var cmd = exec.Command("sysctl", "-n", "kern.smp.active");
            var (output, err) = cmd.CombinedOutput();
            if (err != null)
            {
                fmt.Printf("fail to launch '%s', error: %s, output: %s\n", strings.Join(cmd.Args, " "), err, output);
                return ;
            }

            if (bytes.Equal(output, (slice<byte>)"1\n") == false)
            { 
                // SMP mode deactivated in kernel.
                fmt.Println("OK");
                return ;

            }

            var (list, err) = getList();
            if (err != null)
            {
                fmt.Printf("%s\n", err);
                return ;
            }

            err = checkNCPU(list);
            if (err != null)
            {
                fmt.Printf("%s\n", err);
                return ;
            }

            if (len(list) >= 2L)
            {
                err = checkNCPU(list[..len(list) - 1L]);
                if (err != null)
                {
                    fmt.Printf("%s\n", err);
                    return ;
                }

            }

            fmt.Println("OK");
            return ;

        }

        private static (slice<@string>, error) getList()
        {
            slice<@string> _p0 = default;
            error _p0 = default!;

            var pid = syscall.Getpid(); 

            // Launch cpuset to print a list of available CPUs: pid <PID> mask: 0, 1, 2, 3.
            var cmd = exec.Command("cpuset", "-g", "-p", strconv.Itoa(pid));
            var cmdline = strings.Join(cmd.Args, " ");
            var (output, err) = cmd.CombinedOutput();
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("fail to execute '%s': %s", cmdline, err))!);
            }

            var pos = bytes.IndexRune(output, '\n');
            if (pos == -1L)
            {
                return (null, error.As(fmt.Errorf("invalid output from '%s', '\\n' not found: %s", cmdline, output))!);
            }

            output = output[0L..pos];

            pos = bytes.IndexRune(output, ':');
            if (pos == -1L)
            {
                return (null, error.As(fmt.Errorf("invalid output from '%s', ':' not found: %s", cmdline, output))!);
            }

            slice<@string> list = default;
            foreach (var (_, val) in bytes.Split(output[pos + 1L..], (slice<byte>)","))
            {
                var index = string(bytes.TrimSpace(val));
                if (len(index) == 0L)
                {
                    continue;
                }

                list = append(list, index);

            }
            if (len(list) == 0L)
            {
                return (null, error.As(fmt.Errorf("empty CPU list from '%s': %s", cmdline, output))!);
            }

            return (list, error.As(null!)!);

        }

        private static error checkNCPU(slice<@string> list)
        {
            var listString = strings.Join(list, ",");
            if (len(listString) == 0L)
            {
                return error.As(fmt.Errorf("could not check against an empty CPU list"))!;
            }

            var cListString = cpuSetRE.FindString(listString);
            if (len(cListString) == 0L)
            {
                return error.As(fmt.Errorf("invalid cpuset output '%s'", listString))!;
            } 
            // Launch FreeBSDNumCPUHelper() with specified CPUs list.
            var cmd = exec.Command("cpuset", "-l", cListString, os.Args[0L], "FreeBSDNumCPUHelper");
            var cmdline = strings.Join(cmd.Args, " ");
            var (output, err) = cmd.CombinedOutput();
            if (err != null)
            {
                return error.As(fmt.Errorf("fail to launch child '%s', error: %s, output: %s", cmdline, err, output))!;
            } 

            // NumCPU from FreeBSDNumCPUHelper come with '\n'.
            output = bytes.TrimSpace(output);
            var (n, err) = strconv.Atoi(string(output));
            if (err != null)
            {
                return error.As(fmt.Errorf("fail to parse output from child '%s', error: %s, output: %s", cmdline, err, output))!;
            }

            if (n != len(list))
            {
                return error.As(fmt.Errorf("runtime.NumCPU() expected to %d, got %d when run with CPU list %s", len(list), n, cListString))!;
            }

            return error.As(null!)!;

        }
    }
}
