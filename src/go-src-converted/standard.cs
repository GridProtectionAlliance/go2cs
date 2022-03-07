// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package keys -- go2cs converted at 2022 March 06 23:31:36 UTC
// import "golang.org/x/tools/internal/event/keys" ==> using keys = go.golang.org.x.tools.@internal.@event.keys_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\keys\standard.go


namespace go.golang.org.x.tools.@internal.@event;

public static partial class keys_package {

 
// Msg is a key used to add message strings to label lists.
public static var Msg = NewString("message", "a readable message");public static var Label = NewTag("label", "a label context marker");public static var Start = NewString("start", "span start");public static var End = NewTag("end", "a span end marker");public static var Detach = NewTag("detach", "a span detach marker");public static var Err = NewError("error", "an error that occurred");public static var Metric = NewTag("metric", "a metric event marker");

} // end keys_package
