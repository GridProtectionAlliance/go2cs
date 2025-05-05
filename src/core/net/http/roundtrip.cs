// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !js
namespace go.net;

using _ = unsafe_package; // for linkname

partial class http_package {

// RoundTrip should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/erda-project/erda-infra
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname badRoundTrip net/http.(*Transport).RoundTrip
internal static partial (ж<Response>, error) badRoundTrip(ж<Transport> _, ж<Request> _);

// RoundTrip implements the [RoundTripper] interface.
//
// For higher-level HTTP client support (such as handling of cookies
// and redirects), see [Get], [Post], and the [Client] type.
//
// Like the RoundTripper interface, the error types returned
// by RoundTrip are unspecified.
[GoRecv] public static (ж<Response>, error) RoundTrip(this ref Transport t, ж<Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    return t.roundTrip(Ꮡreq);
}

} // end http_package
