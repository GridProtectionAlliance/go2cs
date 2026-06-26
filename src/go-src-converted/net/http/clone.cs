// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using multipart = mime.multipart_package;
using textproto = net.textproto_package;
using url = net.url_package;
using _ = unsafe_package; // for linkname
using mime;

partial class http_package {

// cloneURLValues should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneURLValues
internal static url.Values cloneURLValues(url.Values v) {
    if (v == default!) {
        return default!;
    }
    // http.Header and url.Values have the same representation, so temporarily
    // treat it like http.Header, which does have a clone:
    return ((url.Values)((ΔHeader)v).Clone());
}

// cloneURL should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneURL
internal static ж<url.URL> cloneURL(ж<url.URL> Ꮡu) {
    ref var u = ref Ꮡu.val;

    if (u == nil) {
        return default!;
    }
    var u2 = @new<url.URL>();
    u2.val = u;
    if (u.User != nil) {
        u2.val.User = @new<url.Userinfo>();
        (~u2).User.val = u.User;
    }
    return u2;
}

// cloneMultipartForm should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneMultipartForm
internal static ж<multipart.Form> cloneMultipartForm(ж<multipart.Form> Ꮡf) {
    ref var f = ref Ꮡf.val;

    if (f == nil) {
        return default!;
    }
    var f2 = Ꮡ(new multipart.Form(
        Value: (map<@string, slice<@string>>)(((ΔHeader)f.Value).Clone())
    ));
    if (f.File != default!) {
        var m = new multipart.FileHeader();
        foreach (var (k, vv) in f.File) {
            var vv2 = new slice<multipart.FileHeader>(len(vv));
            foreach (var (i, v) in vv) {
                vv2[i] = cloneMultipartFileHeader(v);
            }
            m[k] = vv2;
        }
        f2.val.File = m;
    }
    return f2;
}

// cloneMultipartFileHeader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneMultipartFileHeader
internal static ж<multipart.FileHeader> cloneMultipartFileHeader(ж<multipart.FileHeader> Ꮡfh) {
    ref var fh = ref Ꮡfh.val;

    if (fh == nil) {
        return default!;
    }
    var fh2 = @new<multipart.FileHeader>();
    fh2.val = fh;
    fh2.val.Header = ((textproto.MIMEHeader)((ΔHeader)fh.Header).Clone());
    return fh2;
}

// cloneOrMakeHeader invokes Header.Clone but if the
// result is nil, it'll instead make and return a non-nil Header.
//
// cloneOrMakeHeader should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/searKing/golang
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname cloneOrMakeHeader
internal static ΔHeader cloneOrMakeHeader(ΔHeader hdr) {
    var clone = hdr.Clone();
    if (clone == default!) {
        clone = new ΔHeader();
    }
    return clone;
}

} // end http_package
