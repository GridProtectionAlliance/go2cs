// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package driver -- go2cs converted at 2022 March 06 23:23:30 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\svg.go
using regexp = go.regexp_package;
using strings = go.strings_package;

using svgpan = go.github.com.google.pprof.third_party.svgpan_package;

namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class driver_package {

private static var viewBox = regexp.MustCompile("<svg\\s*width=\"[^\"]+\"\\s*height=\"[^\"]+\"\\s*viewBox=\"[^\"]+\"");private static var graphID = regexp.MustCompile("<g id=\"graph\\d\"");private static var svgClose = regexp.MustCompile("</svg>");

// massageSVG enhances the SVG output from DOT to provide better
// panning inside a web browser. It uses the svgpan library, which is
// embedded into the svgpan.JSSource variable.
private static @string massageSVG(@string svg) { 
    // Work around for dot bug which misses quoting some ampersands,
    // resulting on unparsable SVG.
    svg = strings.Replace(svg, "&;", "&amp;;", -1); 

    // Dot's SVG output is
    //
    //    <svg width="___" height="___"
    //     viewBox="___" xmlns=...>
    //    <g id="graph0" transform="...">
    //    ...
    //    </g>
    //    </svg>
    //
    // Change it to
    //
    //    <svg width="100%" height="100%"
    //     xmlns=...>

    //    <script type="text/ecmascript"><![CDATA[` ..$(svgpan.JSSource)... `]]></script>`
    //    <g id="viewport" transform="translate(0,0)">
    //    <g id="graph0" transform="...">
    //    ...
    //    </g>
    //    </g>
    //    </svg>

    {
        var loc__prev1 = loc;

        var loc = viewBox.FindStringIndex(svg);

        if (loc != null) {
            svg = svg[..(int)loc[0]] + "<svg width=\"100%\" height=\"100%\"" + svg[(int)loc[1]..];
        }
        loc = loc__prev1;

    }


    {
        var loc__prev1 = loc;

        loc = graphID.FindStringIndex(svg);

        if (loc != null) {
            svg = svg[..(int)loc[0]] + "<script type=\"text/ecmascript\"><![CDATA[" + string(svgpan.JSSource) + "]]></script>" + "<g id=\"viewport\" transform=\"scale(0.5,0.5) translate(0,0)\">" + svg[(int)loc[0]..];
        }
        loc = loc__prev1;

    }


    {
        var loc__prev1 = loc;

        loc = svgClose.FindStringIndex(svg);

        if (loc != null) {
            svg = svg[..(int)loc[0]] + "</g>" + svg[(int)loc[0]..];
        }
        loc = loc__prev1;

    }


    return svg;

}

} // end driver_package
