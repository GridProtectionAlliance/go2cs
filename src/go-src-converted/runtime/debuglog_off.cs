// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !debuglog
namespace go;

partial class runtime_package {

internal const bool dlogEnabled = false;

[GoType] partial struct dlogPerM {
}

internal static ж<dlogger> getCachedDlogger() {
    return default!;
}

internal static bool putCachedDlogger(ж<dlogger> Ꮡl) {
    ref var l = ref Ꮡl.Value;

    return false;
}

} // end runtime_package
