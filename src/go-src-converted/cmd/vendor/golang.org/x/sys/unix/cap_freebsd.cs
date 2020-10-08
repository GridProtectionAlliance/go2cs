// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd

// package unix -- go2cs converted at 2020 October 08 04:46:11 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\cap_freebsd.go
using errors = go.errors_package;
using fmt = go.fmt_package;
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
        // Go implementation of C mostly found in /usr/src/sys/kern/subr_capability.c
 
        // This is the version of CapRights this package understands. See C implementation for parallels.
        private static readonly var capRightsGoVersion = (var)CAP_RIGHTS_VERSION_00;
        private static readonly var capArSizeMin = (var)CAP_RIGHTS_VERSION_00 + 2L;
        private static readonly var capArSizeMax = (var)capRightsGoVersion + 2L;


        private static long bit2idx = new slice<long>(new long[] { -1, 0, 1, -1, 2, -1, -1, -1, 3, -1, -1, -1, -1, -1, -1, -1, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 });

        private static long capidxbit(ulong right)
        {
            return int((right >> (int)(57L)) & 0x1fUL);
        }

        private static (long, error) rightToIndex(ulong right)
        {
            long _p0 = default;
            error _p0 = default!;

            var idx = capidxbit(right);
            if (idx < 0L || idx >= len(bit2idx))
            {
                return (-2L, error.As(fmt.Errorf("index for right 0x%x out of range", right))!);
            }

            return (bit2idx[idx], error.As(null!)!);

        }

        private static long caprver(ulong right)
        {
            return int(right >> (int)(62L));
        }

        private static long capver(ptr<CapRights> _addr_rights)
        {
            ref CapRights rights = ref _addr_rights.val;

            return caprver(rights.Rights[0L]);
        }

        private static long caparsize(ptr<CapRights> _addr_rights)
        {
            ref CapRights rights = ref _addr_rights.val;

            return capver(_addr_rights) + 2L;
        }

        // CapRightsSet sets the permissions in setrights in rights.
        public static error CapRightsSet(ptr<CapRights> _addr_rights, slice<ulong> setrights)
        {
            ref CapRights rights = ref _addr_rights.val;
 
            // This is essentially a copy of cap_rights_vset()
            if (capver(_addr_rights) != CAP_RIGHTS_VERSION_00)
            {
                return error.As(fmt.Errorf("bad rights version %d", capver(_addr_rights)))!;
            }

            var n = caparsize(_addr_rights);
            if (n < capArSizeMin || n > capArSizeMax)
            {
                return error.As(errors.New("bad rights size"))!;
            }

            foreach (var (_, right) in setrights)
            {
                if (caprver(right) != CAP_RIGHTS_VERSION_00)
                {
                    return error.As(errors.New("bad right version"))!;
                }

                var (i, err) = rightToIndex(right);
                if (err != null)
                {
                    return error.As(err)!;
                }

                if (i >= n)
                {
                    return error.As(errors.New("index overflow"))!;
                }

                if (capidxbit(rights.Rights[i]) != capidxbit(right))
                {
                    return error.As(errors.New("index mismatch"))!;
                }

                rights.Rights[i] |= right;
                if (capidxbit(rights.Rights[i]) != capidxbit(right))
                {
                    return error.As(errors.New("index mismatch (after assign)"))!;
                }

            }
            return error.As(null!)!;

        }

        // CapRightsClear clears the permissions in clearrights from rights.
        public static error CapRightsClear(ptr<CapRights> _addr_rights, slice<ulong> clearrights)
        {
            ref CapRights rights = ref _addr_rights.val;
 
            // This is essentially a copy of cap_rights_vclear()
            if (capver(_addr_rights) != CAP_RIGHTS_VERSION_00)
            {
                return error.As(fmt.Errorf("bad rights version %d", capver(_addr_rights)))!;
            }

            var n = caparsize(_addr_rights);
            if (n < capArSizeMin || n > capArSizeMax)
            {
                return error.As(errors.New("bad rights size"))!;
            }

            foreach (var (_, right) in clearrights)
            {
                if (caprver(right) != CAP_RIGHTS_VERSION_00)
                {
                    return error.As(errors.New("bad right version"))!;
                }

                var (i, err) = rightToIndex(right);
                if (err != null)
                {
                    return error.As(err)!;
                }

                if (i >= n)
                {
                    return error.As(errors.New("index overflow"))!;
                }

                if (capidxbit(rights.Rights[i]) != capidxbit(right))
                {
                    return error.As(errors.New("index mismatch"))!;
                }

                rights.Rights[i] &= ~(right & 0x01FFFFFFFFFFFFFFUL);
                if (capidxbit(rights.Rights[i]) != capidxbit(right))
                {
                    return error.As(errors.New("index mismatch (after assign)"))!;
                }

            }
            return error.As(null!)!;

        }

        // CapRightsIsSet checks whether all the permissions in setrights are present in rights.
        public static (bool, error) CapRightsIsSet(ptr<CapRights> _addr_rights, slice<ulong> setrights)
        {
            bool _p0 = default;
            error _p0 = default!;
            ref CapRights rights = ref _addr_rights.val;
 
            // This is essentially a copy of cap_rights_is_vset()
            if (capver(_addr_rights) != CAP_RIGHTS_VERSION_00)
            {
                return (false, error.As(fmt.Errorf("bad rights version %d", capver(_addr_rights)))!);
            }

            var n = caparsize(_addr_rights);
            if (n < capArSizeMin || n > capArSizeMax)
            {
                return (false, error.As(errors.New("bad rights size"))!);
            }

            foreach (var (_, right) in setrights)
            {
                if (caprver(right) != CAP_RIGHTS_VERSION_00)
                {
                    return (false, error.As(errors.New("bad right version"))!);
                }

                var (i, err) = rightToIndex(right);
                if (err != null)
                {
                    return (false, error.As(err)!);
                }

                if (i >= n)
                {
                    return (false, error.As(errors.New("index overflow"))!);
                }

                if (capidxbit(rights.Rights[i]) != capidxbit(right))
                {
                    return (false, error.As(errors.New("index mismatch"))!);
                }

                if ((rights.Rights[i] & right) != right)
                {
                    return (false, error.As(null!)!);
                }

            }
            return (true, error.As(null!)!);

        }

        private static ulong capright(ulong idx, ulong bit)
        {
            return ((1L << (int)((57L + idx))) | bit);
        }

        // CapRightsInit returns a pointer to an initialised CapRights structure filled with rights.
        // See man cap_rights_init(3) and rights(4).
        public static (ptr<CapRights>, error) CapRightsInit(slice<ulong> rights)
        {
            ptr<CapRights> _p0 = default!;
            error _p0 = default!;

            ref CapRights r = ref heap(out ptr<CapRights> _addr_r);
            r.Rights[0L] = (capRightsGoVersion << (int)(62L)) | capright(0L, 0L);
            r.Rights[1L] = capright(1L, 0L);

            var err = CapRightsSet(_addr_r, rights);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr__addr_r!, error.As(null!)!);

        }

        // CapRightsLimit reduces the operations permitted on fd to at most those contained in rights.
        // The capability rights on fd can never be increased by CapRightsLimit.
        // See man cap_rights_limit(2) and rights(4).
        public static error CapRightsLimit(System.UIntPtr fd, ptr<CapRights> _addr_rights)
        {
            ref CapRights rights = ref _addr_rights.val;

            return error.As(capRightsLimit(int(fd), rights))!;
        }

        // CapRightsGet returns a CapRights structure containing the operations permitted on fd.
        // See man cap_rights_get(3) and rights(4).
        public static (ptr<CapRights>, error) CapRightsGet(System.UIntPtr fd)
        {
            ptr<CapRights> _p0 = default!;
            error _p0 = default!;

            var (r, err) = CapRightsInit(null);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            err = capRightsGet(capRightsGoVersion, int(fd), r);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_r!, error.As(null!)!);

        }
    }
}}}}}}
