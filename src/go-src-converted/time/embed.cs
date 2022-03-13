// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file is used with build tag timetzdata to embed tzdata into
// the binary.

//go:build timetzdata
// +build timetzdata

// package time -- go2cs converted at 2022 March 13 05:23:51 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\embed.go
namespace go;

using _tzdata_ = time.tzdata_package;
} // end time_package
