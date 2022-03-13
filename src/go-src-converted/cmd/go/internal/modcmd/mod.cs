// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package modcmd implements the ``go mod'' command.

// package modcmd -- go2cs converted at 2022 March 13 06:32:24 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\mod.go
namespace go.cmd.go.@internal;

using @base = cmd.go.@internal.@base_package;

public static partial class modcmd_package {

public static ptr<base.Command> CmdMod = addr(new base.Command(UsageLine:"go mod",Short:"module maintenance",Long:`Go mod provides access to operations on modules.

Note that support for modules is built into all the go commands,
not just 'go mod'. For example, day-to-day adding, removing, upgrading,
and downgrading of dependencies should be done using 'go get'.
See 'go help modules' for an overview of module functionality.
	`,Commands:[]*base.Command{cmdDownload,cmdEdit,cmdGraph,cmdInit,cmdTidy,cmdVendor,cmdVerify,cmdWhy,},));

} // end modcmd_package
