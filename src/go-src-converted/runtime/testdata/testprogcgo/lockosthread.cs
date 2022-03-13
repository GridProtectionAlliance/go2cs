// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2022 March 13 05:29:32 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\lockosthread.go
namespace go;

using os = os_package;
using runtime = runtime_package;
using atomic = sync.atomic_package;
using time = time_package;
using @unsafe = @unsafe_package;


/*
#include <pthread.h>
#include <stdint.h>

extern uint32_t threadExited;

void setExited(void *x);
*/


} // end main_package
