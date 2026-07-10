// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !gc || purego || !s390x
namespace go.vendor.golang.org.x.crypto;

partial class sha3_package {

internal static ж<state> new224() {
    return new224Generic();
}

internal static ж<state> new256() {
    return new256Generic();
}

internal static ж<state> new384() {
    return new384Generic();
}

internal static ж<state> new512() {
    return new512Generic();
}

} // end sha3_package
