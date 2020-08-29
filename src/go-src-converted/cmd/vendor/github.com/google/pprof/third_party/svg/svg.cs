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

// Package svg provides tools related to handling of SVG files
// package svg -- go2cs converted at 2020 August 29 10:06:35 UTC
// import "cmd/vendor/github.com/google/pprof/third_party/svg" ==> using svg = go.cmd.vendor.github.com.google.pprof.third_party.svg_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\third_party\svg\svg.go
using regexp = go.regexp_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace third_party
{
    public static partial class svg_package
    {
        private static var viewBox = regexp.MustCompile("<svg\\s*width=\"[^\"]+\"\\s*height=\"[^\"]+\"\\s*viewBox=\"[^\"]+\"");        private static var graphID = regexp.MustCompile("<g id=\"graph\\d\"");        private static var svgClose = regexp.MustCompile("</svg>");

        // Massage enhances the SVG output from DOT to provide better
        // panning inside a web browser. It uses the SVGPan library, which is
        // embedded into the svgPanJS variable.
        public static @string Massage(@string svg)
        { 
            // Work around for dot bug which misses quoting some ampersands,
            // resulting on unparsable SVG.
            svg = strings.Replace(svg, "&;", "&amp;;", -1L); 

            //Dot's SVG output is
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

            //    <script type="text/ecmascript"><![CDATA[` ..$(svgPanJS)... `]]></script>`
            //    <g id="viewport" transform="translate(0,0)">
            //    <g id="graph0" transform="...">
            //    ...
            //    </g>
            //    </g>
            //    </svg>

            {
                var loc__prev1 = loc;

                var loc = viewBox.FindStringIndex(svg);

                if (loc != null)
                {
                    svg = svg[..loc[0L]] + "<svg width=\"100%\" height=\"100%\"" + svg[loc[1L]..];
                }

                loc = loc__prev1;

            }

            {
                var loc__prev1 = loc;

                loc = graphID.FindStringIndex(svg);

                if (loc != null)
                {
                    svg = svg[..loc[0L]] + "<script type=\"text/ecmascript\"><![CDATA[" + string(svgPanJS) + "]]></script>" + "<g id=\"viewport\" transform=\"scale(0.5,0.5) translate(0,0)\">" + svg[loc[0L]..];
                }

                loc = loc__prev1;

            }

            {
                var loc__prev1 = loc;

                loc = svgClose.FindStringIndex(svg);

                if (loc != null)
                {
                    svg = svg[..loc[0L]] + "</g>" + svg[loc[0L]..];
                }

                loc = loc__prev1;

            }

            return svg;
        }
    }
}}}}}}}
