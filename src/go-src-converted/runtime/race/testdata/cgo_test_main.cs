// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:29:17 UTC
// Original source: C:\Program Files\Go\src\runtime\race\testdata\cgo_test_main.go
namespace go;
/*
int sync;

void Notify(void)
{
    __sync_fetch_and_add(&sync, 1);
}

void Wait(void)
{
    while(__sync_fetch_and_add(&sync, 0) == 0) {}
}
*/



} // end main_package
