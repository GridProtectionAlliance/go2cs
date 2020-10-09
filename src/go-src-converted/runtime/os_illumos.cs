// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_illumos.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:cgo_import_dynamic libc_getrctl getrctl "libc.so"
        //go:cgo_import_dynamic libc_rctlblk_get_local_action rctlblk_get_local_action "libc.so"
        //go:cgo_import_dynamic libc_rctlblk_get_local_flags rctlblk_get_local_flags "libc.so"
        //go:cgo_import_dynamic libc_rctlblk_get_value rctlblk_get_value "libc.so"
        //go:cgo_import_dynamic libc_rctlblk_size rctlblk_size "libc.so"

        //go:linkname libc_getrctl libc_getrctl
        //go:linkname libc_rctlblk_get_local_action libc_rctlblk_get_local_action
        //go:linkname libc_rctlblk_get_local_flags libc_rctlblk_get_local_flags
        //go:linkname libc_rctlblk_get_value libc_rctlblk_get_value
        //go:linkname libc_rctlblk_size libc_rctlblk_size
        private static libcFunc libc_getrctl = default;        private static libcFunc libc_rctlblk_get_local_action = default;        private static libcFunc libc_rctlblk_get_local_flags = default;        private static libcFunc libc_rctlblk_get_value = default;        private static libcFunc libc_rctlblk_size = default;


        // Return the minimum value seen for the zone CPU cap, or 0 if no cap is
        // detected.
        private static ulong getcpucap()
        { 
            // The resource control block is an opaque object whose size is only
            // known to libc.  In practice, given the contents, it is unlikely to
            // grow beyond 8KB so we'll use a static buffer of that size here.
            const long rblkmaxsize = (long)8L * 1024L;

            if (rctlblk_size() > rblkmaxsize)
            {
                return 0L;
            } 

            // The "zone.cpu-cap" resource control, as described in
            // resource_controls(5), "sets a limit on the amount of CPU time that
            // can be used by a zone.  The unit used is the percentage of a single
            // CPU that can be used by all user threads in a zone, expressed as an
            // integer."  A C string of the name must be passed to getrctl(2).
            slice<byte> name = (slice<byte>)"zone.cpu-cap\x00"; 

            // To iterate over the list of values for a particular resource
            // control, we need two blocks: one for the previously read value and
            // one for the next value.
            array<byte> rblk0 = new array<byte>(rblkmaxsize);
            array<byte> rblk1 = new array<byte>(rblkmaxsize);
            var rblk = _addr_rblk0[0L];
            var rblkprev = _addr_rblk1[0L];

            uint flag = _RCTL_FIRST;
            ulong capval = 0L;

            while (true)
            {
                if (getrctl(@unsafe.Pointer(_addr_name[0L]), @unsafe.Pointer(rblkprev), @unsafe.Pointer(rblk), flag) != 0L)
                { 
                    // The end of the sequence is reported as an ENOENT
                    // failure, but determining the CPU cap is not critical
                    // here.  We'll treat any failure as if it were the end
                    // of sequence.
                    break;

                }

                var lflags = rctlblk_get_local_flags(@unsafe.Pointer(rblk));
                var action = rctlblk_get_local_action(@unsafe.Pointer(rblk));
                if ((lflags & _RCTL_LOCAL_MAXIMAL) == 0L && action == _RCTL_LOCAL_DENY)
                { 
                    // This is a finite (not maximal) value representing a
                    // cap (deny) action.
                    var v = rctlblk_get_value(@unsafe.Pointer(rblk));
                    if (capval == 0L || capval > v)
                    {
                        capval = v;
                    }

                } 

                // Swap the blocks around so that we can fetch the next value
                var t = rblk;
                rblk = rblkprev;
                rblkprev = t;
                flag = _RCTL_NEXT;

            }


            return capval;

        }

        private static int getncpu()
        {
            var n = int32(sysconf(__SC_NPROCESSORS_ONLN));
            if (n < 1L)
            {
                return 1L;
            }

            {
                var cents = int32(getcpucap());

                if (cents > 0L)
                { 
                    // Convert from a percentage of CPUs to a number of CPUs,
                    // rounding up to make use of a fractional CPU
                    // e.g., 336% becomes 4 CPUs
                    var ncap = (cents + 99L) / 100L;
                    if (ncap < n)
                    {
                        return ncap;
                    }

                }

            }


            return n;

        }

        //go:nosplit
        private static System.UIntPtr getrctl(unsafe.Pointer controlname, unsafe.Pointer oldbuf, unsafe.Pointer newbuf, uint flags)
        {
            return sysvicall4(_addr_libc_getrctl, uintptr(controlname), uintptr(oldbuf), uintptr(newbuf), uintptr(flags));
        }

        //go:nosplit
        private static System.UIntPtr rctlblk_get_local_action(unsafe.Pointer buf)
        {
            return sysvicall2(_addr_libc_rctlblk_get_local_action, uintptr(buf), uintptr(0L));
        }

        //go:nosplit
        private static System.UIntPtr rctlblk_get_local_flags(unsafe.Pointer buf)
        {
            return sysvicall1(_addr_libc_rctlblk_get_local_flags, uintptr(buf));
        }

        //go:nosplit
        private static ulong rctlblk_get_value(unsafe.Pointer buf)
        {
            return uint64(sysvicall1(_addr_libc_rctlblk_get_value, uintptr(buf)));
        }

        //go:nosplit
        private static System.UIntPtr rctlblk_size()
        {
            return sysvicall0(_addr_libc_rctlblk_size);
        }
    }
}
