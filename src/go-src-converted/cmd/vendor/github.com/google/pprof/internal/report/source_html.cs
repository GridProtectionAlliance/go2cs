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

// package report -- go2cs converted at 2020 August 29 10:06:11 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\report\source_html.go
using template = go.html.template_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class report_package
    {
        // AddSourceTemplates adds templates used by PrintWebList to t.
        public static void AddSourceTemplates(ref template.Template t)
        {
            template.Must(t.Parse("{{define \"weblistcss\"}}" + weblistPageCSS + "{{end}}"));
            template.Must(t.Parse("{{define \"weblistjs\"}}" + weblistPageScript + "{{end}}"));
        }

        private static readonly @string weblistPageCSS = @"<style type=""text/css"">
body {
font-family: sans-serif;
}
h1 {
  font-size: 1.5em;
  margin-bottom: 4px;
}
.legend {
  font-size: 1.25em;
}
.line, .nop, .unimportant {
  color: #aaaaaa;
}
.inlinesrc {
  color: #000066;
}
.deadsrc {
cursor: pointer;
}
.deadsrc:hover {
background-color: #eeeeee;
}
.livesrc {
color: #0000ff;
cursor: pointer;
}
.livesrc:hover {
background-color: #eeeeee;
}
.asm {
color: #008800;
display: none;
}
</style>";



        private static readonly @string weblistPageScript = @"<script type=""text/javascript"">
function pprof_toggle_asm(e) {
  var target;
  if (!e) e = window.event;
  if (e.target) target = e.target;
  else if (e.srcElement) target = e.srcElement;

  if (target) {
    var asm = target.nextSibling;
    if (asm && asm.className == ""asm"") {
      asm.style.display = (asm.style.display == ""block"" ? """" : ""block"");
      e.preventDefault();
      return false;
    }
  }
}
</script>";



        private static readonly @string weblistPageClosing = "\n</body>\n</html>\n";

    }
}}}}}}}
