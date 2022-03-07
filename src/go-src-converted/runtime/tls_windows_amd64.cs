// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\tls_windows_amd64.go


namespace go;

public static partial class runtime_package {

    // osSetupTLS is called by needm to set up TLS for non-Go threads.
    //
    // Defined in assembly.
private static void osSetupTLS(ptr<m> mp);

} // end runtime_package
