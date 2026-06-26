// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// osSetupTLS is called by needm to set up TLS for non-Go threads.
//
// Defined in assembly.
internal static partial void osSetupTLS(ж<m> mp);

} // end runtime_package
