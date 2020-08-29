// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test go generate variable substitution.

//go:generate echo $GOARCH $GOFILE:$GOLINE ${GOPACKAGE}abc xyz$GOPACKAGE/$GOFILE/123

// package p -- go2cs converted at 2020 August 29 10:02:01 UTC
// import "cmd/go/testdata.p" ==> using p = go.cmd.go.testdata.p_package
// Original source: C:\Go\src\cmd\go\testdata\generate\test3.go
    }

