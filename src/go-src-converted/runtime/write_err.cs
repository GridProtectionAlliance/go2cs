// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !android
namespace go;

partial class runtime_package {

//go:nosplit
internal static void writeErr(slice<byte> b) {
    if (len(b) > 0) {
        writeErrData(Ꮡ(b, 0), (int32)len(b));
    }
}

} // end runtime_package
