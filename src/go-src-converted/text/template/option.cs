// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the code to handle template options.

// package template -- go2cs converted at 2022 March 06 22:24:44 UTC
// import "text/template" ==> using template = go.text.template_package
// Original source: C:\Program Files\Go\src\text\template\option.go
using strings = go.strings_package;

namespace go.text;

public static partial class template_package {

    // missingKeyAction defines how to respond to indexing a map with a key that is not present.
private partial struct missingKeyAction { // : nint
}

private static readonly missingKeyAction mapInvalid = iota; // Return an invalid reflect.Value.
private static readonly var mapZeroValue = 0; // Return the zero value for the map element.
private static readonly var mapError = 1; // Error out

private partial struct option {
    public missingKeyAction missingKey;
}

// Option sets options for the template. Options are described by
// strings, either a simple string or "key=value". There can be at
// most one equals sign in an option string. If the option string
// is unrecognized or otherwise invalid, Option panics.
//
// Known options:
//
// missingkey: Control the behavior during execution if a map is
// indexed with a key that is not present in the map.
//    "missingkey=default" or "missingkey=invalid"
//        The default behavior: Do nothing and continue execution.
//        If printed, the result of the index operation is the string
//        "<no value>".
//    "missingkey=zero"
//        The operation returns the zero value for the map type's element.
//    "missingkey=error"
//        Execution stops immediately with an error.
//
private static ptr<Template> Option(this ptr<Template> _addr_t, params @string[] opt) {
    opt = opt.Clone();
    ref Template t = ref _addr_t.val;

    t.init();
    foreach (var (_, s) in opt) {
        t.setOption(s);
    }    return _addr_t!;
}

private static void setOption(this ptr<Template> _addr_t, @string opt) => func((_, panic, _) => {
    ref Template t = ref _addr_t.val;

    if (opt == "") {
        panic("empty option string");
    }
    var elems = strings.Split(opt, "=");
    switch (len(elems)) {
        case 2: 
            // key=value
            switch (elems[0]) {
                case "missingkey": 
                    switch (elems[1]) {
                        case "invalid": 

                        case "default": 
                            t.option.missingKey = mapInvalid;
                            return ;
                            break;
                        case "zero": 
                            t.option.missingKey = mapZeroValue;
                            return ;
                            break;
                        case "error": 
                            t.option.missingKey = mapError;
                            return ;
                            break;
                    }

                    break;
            }

            break;
    }
    panic("unrecognized option: " + opt);

});

} // end template_package
