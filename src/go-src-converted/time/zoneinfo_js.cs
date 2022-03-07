// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package time -- go2cs converted at 2022 March 06 22:30:17 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\zoneinfo_js.go
using runtime = go.runtime_package;
using js = go.syscall.js_package;

namespace go;

public static partial class time_package {

private static @string zoneSources = new slice<@string>(new @string[] { "/usr/share/zoneinfo/", "/usr/share/lib/zoneinfo/", "/usr/lib/locale/TZ/", runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

private static void initLocal() {
    localLoc.name = "Local";

    zone z = new zone();
    var d = js.Global().Get("Date").New();
    var offset = d.Call("getTimezoneOffset").Int() * -1;
    z.offset = offset * 60; 
    // According to https://tc39.github.io/ecma262/#sec-timezoneestring,
    // the timezone name from (new Date()).toTimeString() is an implementation-dependent
    // result, and in Google Chrome, it gives the fully expanded name rather than
    // the abbreviation.
    // Hence, we construct the name from the offset.
    z.name = "UTC";
    if (offset < 0) {
        z.name += "-";
        offset *= -1;
    }
    else
 {
        z.name += "+";
    }
    z.name += itoa(offset / 60);
    var min = offset % 60;
    if (min != 0) {
        z.name += ":" + itoa(min);
    }
    localLoc.zone = new slice<zone>(new zone[] { z });

}

// itoa is like strconv.Itoa but only works for values of i in range [0,99].
// It panics if i is out of range.
private static @string itoa(nint i) {
    if (i < 10) {
        return digits[(int)i..(int)i + 1];
    }
    return smallsString[(int)i * 2..(int)i * 2 + 2];

}

private static readonly @string smallsString = "00010203040506070809" + "10111213141516171819" + "20212223242526272829" + "30313233343536373839" + "40414243444546474849" + "50515253545556575859" + "60616263646566676869" + "70717273747576777879" + "80818283848586878889" + "90919293949596979899";

private static readonly @string digits = "0123456789";


} // end time_package
