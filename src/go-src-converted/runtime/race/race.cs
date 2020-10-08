// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build race,linux,amd64 race,freebsd,amd64 race,netbsd,amd64 race,darwin,amd64 race,windows,amd64 race,linux,ppc64le race,linux,arm64

// package race -- go2cs converted at 2020 October 08 03:43:30 UTC
// import "runtime/race" ==> using race = go.runtime.race_package
// Original source: C:\Go\src\runtime\race\race.go
// This file merely ensures that we link in runtime/cgo in race build,
// this in turn ensures that runtime uses pthread_create to create threads.
// The prebuilt race runtime lives in race_GOOS_GOARCH.syso.
// Calls to the runtime are done directly from src/runtime/race.go.

// void __race_unused_func(void);
using C = go.C_package;    }

