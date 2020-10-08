// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\net_plan9.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:linkname runtime_ignoreHangup internal/poll.runtime_ignoreHangup
        private static void runtime_ignoreHangup()
        {
            getg().m.ignoreHangup = true;
        }

        //go:linkname runtime_unignoreHangup internal/poll.runtime_unignoreHangup
        private static void runtime_unignoreHangup(@string sig)
        {
            getg().m.ignoreHangup = false;
        }

        private static bool ignoredNote(ptr<byte> _addr_note)
        {
            ref byte note = ref _addr_note.val;

            if (note == null)
            {
                return false;
            }

            if (gostringnocopy(note) != "hangup")
            {
                return false;
            }

            return getg().m.ignoreHangup;

        }
    }
}
