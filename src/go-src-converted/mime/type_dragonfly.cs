// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2022 March 06 22:21:17 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\type_dragonfly.go


namespace go;

public static partial class mime_package {

private static void init() {
    typeFiles = append(typeFiles, "/usr/local/etc/mime.types");
}

} // end mime_package
