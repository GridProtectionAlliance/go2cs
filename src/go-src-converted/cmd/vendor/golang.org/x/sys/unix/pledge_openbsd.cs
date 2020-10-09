// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 05:56:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\pledge_openbsd.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // Pledge implements the pledge syscall.
        //
        // The pledge syscall does not accept execpromises on OpenBSD releases
        // before 6.3.
        //
        // execpromises must be empty when Pledge is called on OpenBSD
        // releases predating 6.3, otherwise an error will be returned.
        //
        // For more information see pledge(2).
        public static error Pledge(@string promises, @string execpromises)
        {
            var (maj, min, err) = majmin();
            if (err != null)
            {
                return error.As(err)!;
            }
            err = pledgeAvailable(maj, min, execpromises);
            if (err != null)
            {
                return error.As(err)!;
            }
            var (pptr, err) = syscall.BytePtrFromString(promises);
            if (err != null)
            {
                return error.As(err)!;
            }
            unsafe.Pointer expr = default; 

            // If we're running on OpenBSD > 6.2, pass execpromises to the syscall.
            if (maj > 6L || (maj == 6L && min > 2L))
            {
                var (exptr, err) = syscall.BytePtrFromString(execpromises);
                if (err != null)
                {
                    return error.As(err)!;
                }
                expr = @unsafe.Pointer(exptr);

            }
            var (_, _, e) = syscall.Syscall(SYS_PLEDGE, uintptr(@unsafe.Pointer(pptr)), uintptr(expr), 0L);
            if (e != 0L)
            {
                return error.As(e)!;
            }
            return error.As(null!)!;

        }

        // PledgePromises implements the pledge syscall.
        //
        // This changes the promises and leaves the execpromises untouched.
        //
        // For more information see pledge(2).
        public static error PledgePromises(@string promises)
        {
            var (maj, min, err) = majmin();
            if (err != null)
            {
                return error.As(err)!;
            }

            err = pledgeAvailable(maj, min, "");
            if (err != null)
            {
                return error.As(err)!;
            } 

            // This variable holds the execpromises and is always nil.
            unsafe.Pointer expr = default;

            var (pptr, err) = syscall.BytePtrFromString(promises);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (_, _, e) = syscall.Syscall(SYS_PLEDGE, uintptr(@unsafe.Pointer(pptr)), uintptr(expr), 0L);
            if (e != 0L)
            {
                return error.As(e)!;
            }

            return error.As(null!)!;

        }

        // PledgeExecpromises implements the pledge syscall.
        //
        // This changes the execpromises and leaves the promises untouched.
        //
        // For more information see pledge(2).
        public static error PledgeExecpromises(@string execpromises)
        {
            var (maj, min, err) = majmin();
            if (err != null)
            {
                return error.As(err)!;
            }

            err = pledgeAvailable(maj, min, execpromises);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // This variable holds the promises and is always nil.
            unsafe.Pointer pptr = default;

            var (exptr, err) = syscall.BytePtrFromString(execpromises);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (_, _, e) = syscall.Syscall(SYS_PLEDGE, uintptr(pptr), uintptr(@unsafe.Pointer(exptr)), 0L);
            if (e != 0L)
            {
                return error.As(e)!;
            }

            return error.As(null!)!;

        }

        // majmin returns major and minor version number for an OpenBSD system.
        private static (long, long, error) majmin()
        {
            long major = default;
            long minor = default;
            error err = default!;

            ref Utsname v = ref heap(out ptr<Utsname> _addr_v);
            err = Uname(_addr_v);
            if (err != null)
            {
                return ;
            }

            major, err = strconv.Atoi(string(v.Release[0L]));
            if (err != null)
            {
                err = errors.New("cannot parse major version number returned by uname");
                return ;
            }

            minor, err = strconv.Atoi(string(v.Release[2L]));
            if (err != null)
            {
                err = errors.New("cannot parse minor version number returned by uname");
                return ;
            }

            return ;

        }

        // pledgeAvailable checks for availability of the pledge(2) syscall
        // based on the running OpenBSD version.
        private static error pledgeAvailable(long maj, long min, @string execpromises)
        { 
            // If OpenBSD <= 5.9, pledge is not available.
            if ((maj == 5L && min != 9L) || maj < 5L)
            {
                return error.As(fmt.Errorf("pledge syscall is not available on OpenBSD %d.%d", maj, min))!;
            } 

            // If OpenBSD <= 6.2 and execpromises is not empty,
            // return an error - execpromises is not available before 6.3
            if ((maj < 6L || (maj == 6L && min <= 2L)) && execpromises != "")
            {
                return error.As(fmt.Errorf("cannot use execpromises on OpenBSD %d.%d", maj, min))!;
            }

            return error.As(null!)!;

        }
    }
}}}}}}
