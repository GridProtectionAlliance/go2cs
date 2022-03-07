// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wire -- go2cs converted at 2022 March 06 23:31:42 UTC
// import "golang.org/x/tools/internal/event/export/ocagent/wire" ==> using wire = go.golang.org.x.tools.@internal.@event.export.ocagent.wire_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\ocagent\wire\core.go

using System.ComponentModel;


namespace go.golang.org.x.tools.@internal.@event.export.ocagent;

public static partial class wire_package {

    // This file contains type that match core proto types
public partial struct Timestamp { // : @string
}

public partial struct Int64Value {
    [Description("json:\"value,omitempty\"")]
    public long Value;
}

public partial struct DoubleValue {
    [Description("json:\"value,omitempty\"")]
    public double Value;
}

} // end wire_package
