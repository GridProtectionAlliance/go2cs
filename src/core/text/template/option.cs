// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains the code to handle template options.
namespace go.text;

using strings = strings_package;
using ꓸꓸꓸ@string = Span<@string>;

partial class template_package {

[GoType("num:nint")] partial struct missingKeyAction;

internal static readonly missingKeyAction mapInvalid = /* iota */ 0;  // Return an invalid reflect.Value.
internal static readonly missingKeyAction mapZeroValue = 1; // Return the zero value for the map element.
internal static readonly missingKeyAction mapError = 2;    // Error out

[GoType] partial struct option {
    internal missingKeyAction missingKey;
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
//
//	"missingkey=default" or "missingkey=invalid"
//		The default behavior: Do nothing and continue execution.
//		If printed, the result of the index operation is the string
//		"<no value>".
//	"missingkey=zero"
//		The operation returns the zero value for the map type's element.
//	"missingkey=error"
//		Execution stops immediately with an error.
[GoRecv("capture")] public static ж<Template> Option(this ref Template t, params ꓸꓸꓸ@string optʗp) {
    var opt = optʗp.slice();

    t.init();
    foreach (var (_, s) in opt) {
        t.setOption(s);
    }
    return OptionꓸᏑt;
}

[GoRecv] internal static void setOption(this ref Template t, @string opt) {
    if (opt == ""u8) {
        throw panic("empty option string");
    }
    // key=value
    {
        var (key, value, ok) = strings.Cut(opt, "="u8); if (ok) {
            var exprᴛ1 = key;
            if (exprᴛ1 == "missingkey"u8) {
                var exprᴛ2 = value;
                if (exprᴛ2 == "invalid"u8 || exprᴛ2 == "default"u8) {
                    t.option.missingKey = mapInvalid;
                    return;
                }
                if (exprᴛ2 == "zero"u8) {
                    t.option.missingKey = mapZeroValue;
                    return;
                }
                if (exprᴛ2 == "error"u8) {
                    t.option.missingKey = mapError;
                    return;
                }

            }

        }
    }
    throw panic("unrecognized option: "u8 + opt);
}

} // end template_package
