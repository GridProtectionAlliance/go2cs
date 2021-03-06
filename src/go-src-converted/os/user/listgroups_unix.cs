// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly darwin freebsd !android,linux netbsd openbsd
// +build cgo,!osusergo

// package user -- go2cs converted at 2020 October 09 05:07:34 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\listgroups_unix.go
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using @unsafe = go.@unsafe_package;
using C = go.C_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static readonly long maxGroups = (long)2048L;



        private static (slice<@string>, error) listGroups(ptr<User> _addr_u)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref User u = ref _addr_u.val;

            var (ug, err) = strconv.Atoi(u.Gid);
            if (err != null)
            {
                return (null, error.As(fmt.Errorf("user: list groups for %s: invalid gid %q", u.Username, u.Gid))!);
            }

            var userGID = C.gid_t(ug);
            var nameC = make_slice<byte>(len(u.Username) + 1L);
            copy(nameC, u.Username);

            ref var n = ref heap(C.@int(256L), out ptr<var> _addr_n);
            ref var gidsC = ref heap(make_slice<C.gid_t>(n), out ptr<var> _addr_gidsC);
            var rv = getGroupList((C.@char.val)(@unsafe.Pointer(_addr_nameC[0L])), userGID, _addr_gidsC[0L], _addr_n);
            if (rv == -1L)
            { 
                // Mac is the only Unix that does not set n properly when rv == -1, so
                // we need to use different logic for Mac vs. the other OS's.
                {
                    var err = groupRetry(u.Username, nameC, userGID, _addr_gidsC, _addr_n);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

            }

            gidsC = gidsC[..n];
            var gids = make_slice<@string>(0L, n);
            foreach (var (_, g) in gidsC[..n])
            {
                gids = append(gids, strconv.Itoa(int(g)));
            }
            return (gids, error.As(null!)!);

        }
    }
}}
