// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build debuglog

// package runtime -- go2cs converted at 2020 October 08 03:19:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\debuglog_on.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly var dlogEnabled = (var)true;

        // dlogPerM is the per-M debug log data. This is embedded in the m
        // struct.


        // dlogPerM is the per-M debug log data. This is embedded in the m
        // struct.
        private partial struct dlogPerM
        {
            public ptr<dlogger> dlogCache;
        }

        // getCachedDlogger returns a cached dlogger if it can do so
        // efficiently, or nil otherwise. The returned dlogger will be owned.
        private static ptr<dlogger> getCachedDlogger()
        {
            var mp = acquirem(); 
            // We don't return a cached dlogger if we're running on the
            // signal stack in case the signal arrived while in
            // get/putCachedDlogger. (Too bad we don't have non-atomic
            // exchange!)
            ptr<dlogger> l;
            if (getg() != mp.gsignal)
            {
                l = mp.dlogCache;
                mp.dlogCache = null;
            }

            releasem(mp);
            return _addr_l!;

        }

        // putCachedDlogger attempts to return l to the local cache. It
        // returns false if this fails.
        private static bool putCachedDlogger(ptr<dlogger> _addr_l)
        {
            ref dlogger l = ref _addr_l.val;

            var mp = acquirem();
            if (getg() != mp.gsignal && mp.dlogCache == null)
            {
                mp.dlogCache = l;
                releasem(mp);
                return true;
            }

            releasem(mp);
            return false;

        }
    }
}
