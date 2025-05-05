// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.@internal;

using syscall = syscall_package;

partial class socktest_package {
/* visitMapType: map[syscall.Handle]Status */

[GoRecv] internal static ж<Status> sockso(this ref Switch sw, syscallꓸHandle s) => func((defer, _) => {
    sw.smu.RLock();
    defer(sw.smu.RUnlock);
    ref var so = ref heap<Status>(out var Ꮡso);
    so = sw.sotab[s];
    var ok = sw.sotab[s];
    if (!ok) {
        return default!;
    }
    return Ꮡso;
});

// addLocked returns a new Status without locking.
// sw.smu must be held before call.
[GoRecv] internal static ж<Status> addLocked(this ref Switch sw, syscallꓸHandle s, nint family, nint sotype, nint proto) {
    sw.once.Do(sw.init);
    ref var so = ref heap<Status>(out var Ꮡso);
    so = new Status(Cookie: cookie(family, sotype, proto));
    sw.sotab[s] = so;
    return Ꮡso;
}

} // end socktest_package
