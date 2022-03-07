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

// package report -- go2cs converted at 2022 March 06 23:23:48 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\report\source_html.go
using template = go.html.template_package;

namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class report_package {

    // AddSourceTemplates adds templates used by PrintWebList to t.
public static void AddSourceTemplates(ptr<template.Template> _addr_t) {
    ref template.Template t = ref _addr_t.val;

    template.Must(t.Parse("{{define \"weblistcss\"}}" + weblistPageCSS + "{{end}}"));
    template.Must(t.Parse("{{define \"weblistjs\"}}" + weblistPageScript + "{{end}}"));
}

private static readonly @string weblistPageCSS = @"<style type=""text/css"">
body #content{
font-family: sans-serif;
}
h1 {
  font-size: 1.5em;
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
.livesrc {
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


} // end report_package
