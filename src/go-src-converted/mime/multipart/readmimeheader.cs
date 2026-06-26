// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using textproto = net.textproto_package;
using _ = unsafe_package; // for go:linkname
using net;

partial class multipart_package {

// readMIMEHeader is defined in package [net/textproto].
//
//go:linkname readMIMEHeader net/textproto.readMIMEHeader
internal static partial (textproto.MIMEHeader, error) readMIMEHeader(ж<textproto.Reader> r, int64 maxMemory, int64 maxHeaders);

} // end multipart_package
