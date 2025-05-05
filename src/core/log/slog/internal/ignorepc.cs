// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log.slog;

partial class internal_package {

// If IgnorePC is true, do not invoke runtime.Callers to get the pc.
// This is solely for benchmarking the slowdown from runtime.Callers.
public static bool IgnorePC = false;

} // end internal_package
