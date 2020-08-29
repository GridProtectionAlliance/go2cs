// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux

// package main -- go2cs converted at 2020 August 29 08:24:26 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\gettid.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long gettid()
        {
            return syscall.Gettid();
        }

        private static (bool, bool) tidExists(long tid)
        {
            var (stat, err) = ioutil.ReadFile(fmt.Sprintf("/proc/self/task/%d/stat", tid));
            if (os.IsNotExist(err))
            {
                return (false, true);
            } 
            // Check if it's a zombie thread.
            var state = bytes.Fields(stat)[2L];
            return (!(len(state) == 1L && state[0L] == 'Z'), true);
        }
    }
}
