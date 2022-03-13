// Copyright 2017 Google Inc. All Rights Reserved.
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

// package driver -- go2cs converted at 2022 March 13 06:36:35 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\webhtml.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using template = html.template_package;

using d3 = github.com.google.pprof.third_party.d3_package;
using d3flamegraph = github.com.google.pprof.third_party.d3flamegraph_package;


// addTemplates adds a set of template definitions to templates.

public static partial class driver_package {

private static void addTemplates(ptr<template.Template> _addr_templates) {
    ref template.Template templates = ref _addr_templates.val;

    template.Must(templates.Parse("{{define \"d3script\"}}" + d3.JSSource + "{{end}}"));
    template.Must(templates.Parse("{{define \"d3flamegraphscript\"}}" + d3flamegraph.JSSource + "{{end}}"));
    template.Must(templates.Parse("{{define \"d3flamegraphcss\"}}" + d3flamegraph.CSSSource + "{{end}}"));
    template.Must(templates.Parse("\n{{define \"css\"}}\n<style type=\"text/css\">\n* {\n  margin: 0;\n  padding: 0;\n  box-si" +
    "zing: border-box;\n}\nhtml, body {\n  height: 100%;\n}\nbody {\n  font-family: \'Roboto" +
    "\', -apple-system, BlinkMacSystemFont, \'Segoe UI\', Helvetica, Arial, sans-serif, " +
    "\'Apple Color Emoji\', \'Segoe UI Emoji\', \'Segoe UI Symbol\';\n  font-size: 13px;\n  l" +
    "ine-height: 1.4;\n  display: flex;\n  flex-direction: column;\n}\na {\n  color: #2a66" +
    "d9;\n}\n.header {\n  display: flex;\n  align-items: center;\n  height: 44px;\n  min-he" +
    "ight: 44px;\n  background-color: #eee;\n  color: #212121;\n  padding: 0 1rem;\n}\n.he" +
    "ader > div {\n  margin: 0 0.125em;\n}\n.header .title h1 {\n  font-size: 1.75em;\n  m" +
    "argin-right: 1rem;\n  margin-bottom: 4px;\n}\n.header .title a {\n  color: #212121;\n" +
    "  text-decoration: none;\n}\n.header .title a:hover {\n  text-decoration: underline" +
    ";\n}\n.header .description {\n  width: 100%;\n  text-align: right;\n  white-space: no" +
    "wrap;\n}\n@media screen and (max-width: 799px) {\n  .header input {\n    display: no" +
    "ne;\n  }\n}\n#detailsbox {\n  display: none;\n  z-index: 1;\n  position: fixed;\n  top:" +
    " 40px;\n  right: 20px;\n  background-color: #ffffff;\n  box-shadow: 0 1px 5px rgba(" +
    "0,0,0,.3);\n  line-height: 24px;\n  padding: 1em;\n  text-align: left;\n}\n.header in" +
    "put {\n  background: white url(\"data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.or" +
    "g/2000/svg\' viewBox=\'0 0 24 24\' style=\'pointer-events:none;display:block;width:1" +
    "00%25;height:100%25;fill:%23757575\'%3E%3Cpath d=\'M15.5 14h-.79l-.28-.27C15.41 12" +
    ".59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61.0 3.0" +
    "9-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7" +
    ".01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z\'/%3E%3C/svg%3E\") no-repeat 4px cente" +
    "r/20px 20px;\n  border: 1px solid #d1d2d3;\n  border-radius: 2px 0 0 2px;\n  paddin" +
    "g: 0.25em;\n  padding-left: 28px;\n  margin-left: 1em;\n  font-family: \'Roboto\', \'N" +
    "oto\', sans-serif;\n  font-size: 1em;\n  line-height: 24px;\n  color: #212121;\n}\n.do" +
    "wnArrow {\n  border-top: .36em solid #ccc;\n  border-left: .36em solid transparent" +
    ";\n  border-right: .36em solid transparent;\n  margin-bottom: .05em;\n  margin-left" +
    ": .5em;\n  transition: border-top-color 200ms;\n}\n.menu-item {\n  height: 100%;\n  t" +
    "ext-transform: uppercase;\n  font-family: \'Roboto Medium\', -apple-system, BlinkMa" +
    "cSystemFont, \'Segoe UI\', Helvetica, Arial, sans-serif, \'Apple Color Emoji\', \'Seg" +
    "oe UI Emoji\', \'Segoe UI Symbol\';\n  position: relative;\n}\n.menu-item .menu-name:h" +
    "over {\n  opacity: 0.75;\n}\n.menu-item .menu-name:hover .downArrow {\n  border-top-" +
    "color: #666;\n}\n.menu-name {\n  height: 100%;\n  padding: 0 0.5em;\n  display: flex;" +
    "\n  align-items: center;\n  justify-content: center;\n}\n.submenu {\n  display: none;" +
    "\n  z-index: 1;\n  margin-top: -4px;\n  min-width: 10em;\n  position: absolute;\n  le" +
    "ft: 0px;\n  background-color: white;\n  box-shadow: 0 1px 5px rgba(0,0,0,.3);\n  fo" +
    "nt-size: 100%;\n  text-transform: none;\n}\n.menu-item, .submenu {\n  user-select: n" +
    "one;\n  -moz-user-select: none;\n  -ms-user-select: none;\n  -webkit-user-select: n" +
    "one;\n}\n.submenu hr {\n  border: 0;\n  border-top: 2px solid #eee;\n}\n.submenu a {\n " +
    " display: block;\n  padding: .5em 1em;\n  text-decoration: none;\n}\n.submenu a:hove" +
    "r, .submenu a.active {\n  color: white;\n  background-color: #6b82d6;\n}\n.submenu a" +
    ".disabled {\n  color: gray;\n  pointer-events: none;\n}\n.menu-check-mark {\n  positi" +
    "on: absolute;\n  left: 2px;\n}\n.menu-delete-btn {\n  position: absolute;\n  right: 2" +
    "px;\n}\n\n{{/* Used to disable events when a modal dialog is displayed */}}\n#dialog" +
    "-overlay {\n  display: none;\n  position: fixed;\n  left: 0px;\n  top: 0px;\n  width:" +
    " 100%;\n  height: 100%;\n  background-color: rgba(1,1,1,0.1);\n}\n\n.dialog {\n  {{/* " +
    "Displayed centered horizontally near the top */}}\n  display: none;\n  position: f" +
    "ixed;\n  margin: 0px;\n  top: 60px;\n  left: 50%;\n  transform: translateX(-50%);\n\n " +
    " z-index: 3;\n  font-size: 125%;\n  background-color: #ffffff;\n  box-shadow: 0 1px" +
    " 5px rgba(0,0,0,.3);\n}\n.dialog-header {\n  font-size: 120%;\n  border-bottom: 1px " +
    "solid #CCCCCC;\n  width: 100%;\n  text-align: center;\n  background: #EEEEEE;\n  use" +
    "r-select: none;\n}\n.dialog-footer {\n  border-top: 1px solid #CCCCCC;\n  width: 100" +
    "%;\n  text-align: right;\n  padding: 10px;\n}\n.dialog-error {\n  margin: 10px;\n  col" +
    "or: red;\n}\n.dialog input {\n  margin: 10px;\n  font-size: inherit;\n}\n.dialog butto" +
    "n {\n  margin-left: 10px;\n  font-size: inherit;\n}\n#save-dialog, #delete-dialog {\n" +
    "  width: 50%;\n  max-width: 20em;\n}\n#delete-prompt {\n  padding: 10px;\n}\n\n#content" +
    " {\n  overflow-y: scroll;\n  padding: 1em;\n}\n#top {\n  overflow-y: scroll;\n}\n#graph" +
    " {\n  overflow: hidden;\n}\n#graph svg {\n  width: 100%;\n  height: auto;\n  padding: " +
    "10px;\n}\n#content.source .filename {\n  margin-top: 0;\n  margin-bottom: 1em;\n  fon" +
    "t-size: 120%;\n}\n#content.source pre {\n  margin-bottom: 3em;\n}\ntable {\n  border-s" +
    "pacing: 0px;\n  width: 100%;\n  padding-bottom: 1em;\n  white-space: nowrap;\n}\ntabl" +
    "e thead {\n  font-family: \'Roboto Medium\', -apple-system, BlinkMacSystemFont, \'Se" +
    "goe UI\', Helvetica, Arial, sans-serif, \'Apple Color Emoji\', \'Segoe UI Emoji\', \'S" +
    "egoe UI Symbol\';\n}\ntable tr th {\n  position: sticky;\n  top: 0;\n  background-colo" +
    "r: #ddd;\n  text-align: right;\n  padding: .3em .5em;\n}\ntable tr td {\n  padding: ." +
    "3em .5em;\n  text-align: right;\n}\n#top table tr th:nth-child(6),\n#top table tr th" +
    ":nth-child(7),\n#top table tr td:nth-child(6),\n#top table tr td:nth-child(7) {\n  " +
    "text-align: left;\n}\n#top table tr td:nth-child(6) {\n  width: 100%;\n  text-overfl" +
    "ow: ellipsis;\n  overflow: hidden;\n  white-space: nowrap;\n}\n#flathdr1, #flathdr2," +
    " #cumhdr1, #cumhdr2, #namehdr {\n  cursor: ns-resize;\n}\n.hilite {\n  background-co" +
    "lor: #ebf5fb;\n  font-weight: bold;\n}\n</style>\n{{end}}\n\n{{define \"header\"}}\n<div " +
    "class=\"header\">\n  <div class=\"title\">\n    <h1><a href=\"./\">pprof</a></h1>\n  </di" +
    "v>\n\n  <div id=\"view\" class=\"menu-item\">\n    <div class=\"menu-name\">\n      View\n " +
    "     <i class=\"downArrow\"></i>\n    </div>\n    <div class=\"submenu\">\n      <a tit" +
    "le=\"{{.Help.top}}\"  href=\"./top\" id=\"topbtn\">Top</a>\n      <a title=\"{{.Help.gra" +
    "ph}}\" href=\"./\" id=\"graphbtn\">Graph</a>\n      <a title=\"{{.Help.flamegraph}}\" hr" +
    "ef=\"./flamegraph\" id=\"flamegraph\">Flame Graph</a>\n      <a title=\"{{.Help.peek}}" +
    "\" href=\"./peek\" id=\"peek\">Peek</a>\n      <a title=\"{{.Help.list}}\" href=\"./sourc" +
    "e\" id=\"list\">Source</a>\n      <a title=\"{{.Help.disasm}}\" href=\"./disasm\" id=\"di" +
    "sasm\">Disassemble</a>\n    </div>\n  </div>\n\n  {{$sampleLen := len .SampleTypes}}\n" +
    "  {{if gt $sampleLen 1}}\n  <div id=\"sample\" class=\"menu-item\">\n    <div class=\"m" +
    "enu-name\">\n      Sample\n      <i class=\"downArrow\"></i>\n    </div>\n    <div clas" +
    "s=\"submenu\">\n      {{range .SampleTypes}}\n      <a href=\"?si={{.}}\" id=\"{{.}}\">{" +
    "{.}}</a>\n      {{end}}\n    </div>\n  </div>\n  {{end}}\n\n  <div id=\"refine\" class=\"" +
    "menu-item\">\n    <div class=\"menu-name\">\n      Refine\n      <i class=\"downArrow\">" +
    "</i>\n    </div>\n    <div class=\"submenu\">\n      <a title=\"{{.Help.focus}}\" href=" +
    "\"?\" id=\"focus\">Focus</a>\n      <a title=\"{{.Help.ignore}}\" href=\"?\" id=\"ignore\">" +
    "Ignore</a>\n      <a title=\"{{.Help.hide}}\" href=\"?\" id=\"hide\">Hide</a>\n      <a " +
    "title=\"{{.Help.show}}\" href=\"?\" id=\"show\">Show</a>\n      <a title=\"{{.Help.show_" +
    "from}}\" href=\"?\" id=\"show-from\">Show from</a>\n      <hr>\n      <a title=\"{{.Help" +
    ".reset}}\" href=\"?\">Reset</a>\n    </div>\n  </div>\n\n  <div id=\"config\" class=\"menu" +
    "-item\">\n    <div class=\"menu-name\">\n      Config\n      <i class=\"downArrow\"></i>" +
    "\n    </div>\n    <div class=\"submenu\">\n      <a title=\"{{.Help.save_config}}\" id=" +
    "\"save-config\">Save as ...</a>\n      <hr>\n      {{range .Configs}}\n        <a hre" +
    "f=\"{{.URL}}\">\n          {{if .Current}}<span class=\"menu-check-mark\">✓</span>{{e" +
    "nd}}\n          {{.Name}}\n          {{if .UserConfig}}<span class=\"menu-delete-bt" +
    "n\" data-config={{.Name}}>🗙</span>{{end}}\n        </a>\n      {{end}}\n    </div>\n" +
    "  </div>\n\n  <div>\n    <input id=\"search\" type=\"text\" placeholder=\"Search regexp\"" +
    " autocomplete=\"off\" autocapitalize=\"none\" size=40>\n  </div>\n\n  <div class=\"descr" +
    "iption\">\n    <a title=\"{{.Help.details}}\" href=\"#\" id=\"details\">{{.Title}}</a>\n " +
    "   <div id=\"detailsbox\">\n      {{range .Legend}}<div>{{.}}</div>{{end}}\n    </di" +
    "v>\n  </div>\n</div>\n\n<div id=\"dialog-overlay\"></div>\n\n<div class=\"dialog\" id=\"sav" +
    "e-dialog\">\n  <div class=\"dialog-header\">Save options as</div>\n  <datalist id=\"co" +
    "nfig-list\">\n    {{range .Configs}}{{if .UserConfig}}<option value=\"{{.Name}}\" />" +
    "{{end}}{{end}}\n  </datalist>\n  <input id=\"save-name\" type=\"text\" list=\"config-li" +
    "st\" placeholder=\"New config\" />\n  <div class=\"dialog-footer\">\n    <span class=\"d" +
    "ialog-error\" id=\"save-error\"></span>\n    <button id=\"save-cancel\">Cancel</button" +
    ">\n    <button id=\"save-confirm\">Save</button>\n  </div>\n</div>\n\n<div class=\"dialo" +
    "g\" id=\"delete-dialog\">\n  <div class=\"dialog-header\" id=\"delete-dialog-title\">Del" +
    "ete config</div>\n  <div id=\"delete-prompt\"></div>\n  <div class=\"dialog-footer\">\n" +
    "    <span class=\"dialog-error\" id=\"delete-error\"></span>\n    <button id=\"delete-" +
    "cancel\">Cancel</button>\n    <button id=\"delete-confirm\">Delete</button>\n  </div>" +
    "\n</div>\n\n<div id=\"errors\">{{range .Errors}}<div>{{.}}</div>{{end}}</div>\n{{end}}" +
    "\n\n{{define \"graph\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  " +
    "<title>{{.Title}}</title>\n  {{template \"css\" .}}\n</head>\n<body>\n  {{template \"he" +
    "ader\" .}}\n  <div id=\"graph\">\n    {{.HTMLBody}}\n  </div>\n  {{template \"script\" .}" +
    "}\n  <script>viewer(new URL(window.location.href), {{.Nodes}});</script>\n</body>\n" +
    "</html>\n{{end}}\n\n{{define \"script\"}}\n<script>\n// Make svg pannable and zoomable." +
    "\n// Call clickHandler(t) if a click event is caught by the pan event handlers.\nf" +
    "unction initPanAndZoom(svg, clickHandler) {\n  \'use strict\';\n\n  // Current mouse/" +
    "touch handling mode\n  const IDLE = 0;\n  const MOUSEPAN = 1;\n  const TOUCHPAN = 2" +
    ";\n  const TOUCHZOOM = 3;\n  let mode = IDLE;\n\n  // State needed to implement zoom" +
    "ing.\n  let currentScale = 1.0;\n  const initWidth = svg.viewBox.baseVal.width;\n  " +
    "const initHeight = svg.viewBox.baseVal.height;\n\n  // State needed to implement p" +
    "anning.\n  let panLastX = 0;      // Last event X coordinate\n  let panLastY = 0; " +
    "     // Last event Y coordinate\n  let moved = false;     // Have we seen signifi" +
    "cant movement\n  let touchid = null;    // Current touch identifier\n\n  // State n" +
    "eeded for pinch zooming\n  let touchid2 = null;     // Second id for pinch zoomin" +
    "g\n  let initGap = 1.0;       // Starting gap between two touches\n  let initScale" +
    " = 1.0;     // currentScale when pinch zoom started\n  let centerPoint = null;  /" +
    "/ Center point for scaling\n\n  // Convert event coordinates to svg coordinates.\n " +
    " function toSvg(x, y) {\n    const p = svg.createSVGPoint();\n    p.x = x;\n    p.y" +
    " = y;\n    let m = svg.getCTM();\n    if (m == null) m = svg.getScreenCTM(); // Fi" +
    "refox workaround.\n    return p.matrixTransform(m.inverse());\n  }\n\n  // Change th" +
    "e scaling for the svg to s, keeping the point denoted\n  // by u (in svg coordina" +
    "tes]) fixed at the same screen location.\n  function rescale(s, u) {\n    // Limit" +
    " to a good range.\n    if (s < 0.2) s = 0.2;\n    if (s > 10.0) s = 10.0;\n\n    cur" +
    "rentScale = s;\n\n    // svg.viewBox defines the visible portion of the user coord" +
    "inate\n    // system.  So to magnify by s, divide the visible portion by s,\n    /" +
    "/ which will then be stretched to fit the viewport.\n    const vb = svg.viewBox;\n" +
    "    const w1 = vb.baseVal.width;\n    const w2 = initWidth / s;\n    const h1 = vb" +
    ".baseVal.height;\n    const h2 = initHeight / s;\n    vb.baseVal.width = w2;\n    v" +
    "b.baseVal.height = h2;\n\n    // We also want to adjust vb.baseVal.x so that u.x r" +
    "emains at same\n    // screen X coordinate.  In other words, want to change it fr" +
    "om x1 to x2\n    // so that:\n    //     (u.x - x1) / w1 = (u.x - x2) / w2\n    // " +
    "Simplifying that, we get\n    //     (u.x - x1) * (w2 / w1) = u.x - x2\n    //    " +
    " x2 = u.x - (u.x - x1) * (w2 / w1)\n    vb.baseVal.x = u.x - (u.x - vb.baseVal.x)" +
    " * (w2 / w1);\n    vb.baseVal.y = u.y - (u.y - vb.baseVal.y) * (h2 / h1);\n  }\n\n  " +
    "function handleWheel(e) {\n    if (e.deltaY == 0) return;\n    // Change scale fac" +
    "tor by 1.1 or 1/1.1\n    rescale(currentScale * (e.deltaY < 0 ? 1.1 : (1/1.1)),\n " +
    "           toSvg(e.offsetX, e.offsetY));\n  }\n\n  function setMode(m) {\n    mode =" +
    " m;\n    touchid = null;\n    touchid2 = null;\n  }\n\n  function panStart(x, y) {\n  " +
    "  moved = false;\n    panLastX = x;\n    panLastY = y;\n  }\n\n  function panMove(x, " +
    "y) {\n    let dx = x - panLastX;\n    let dy = y - panLastY;\n    if (Math.abs(dx) " +
    "<= 2 && Math.abs(dy) <= 2) return; // Ignore tiny moves\n\n    moved = true;\n    p" +
    "anLastX = x;\n    panLastY = y;\n\n    // Firefox workaround: get dimensions from p" +
    "arentNode.\n    const swidth = svg.clientWidth || svg.parentNode.clientWidth;\n   " +
    " const sheight = svg.clientHeight || svg.parentNode.clientHeight;\n\n    // Conver" +
    "t deltas from screen space to svg space.\n    dx *= (svg.viewBox.baseVal.width / " +
    "swidth);\n    dy *= (svg.viewBox.baseVal.height / sheight);\n\n    svg.viewBox.base" +
    "Val.x -= dx;\n    svg.viewBox.baseVal.y -= dy;\n  }\n\n  function handleScanStart(e)" +
    " {\n    if (e.button != 0) return; // Do not catch right-clicks etc.\n    setMode(" +
    "MOUSEPAN);\n    panStart(e.clientX, e.clientY);\n    e.preventDefault();\n    svg.a" +
    "ddEventListener(\'mousemove\', handleScanMove);\n  }\n\n  function handleScanMove(e) " +
    "{\n    if (e.buttons == 0) {\n      // Missed an end event, perhaps because mouse " +
    "moved outside window.\n      setMode(IDLE);\n      svg.removeEventListener(\'mousem" +
    "ove\', handleScanMove);\n      return;\n    }\n    if (mode == MOUSEPAN) panMove(e.c" +
    "lientX, e.clientY);\n  }\n\n  function handleScanEnd(e) {\n    if (mode == MOUSEPAN)" +
    " panMove(e.clientX, e.clientY);\n    setMode(IDLE);\n    svg.removeEventListener(\'" +
    "mousemove\', handleScanMove);\n    if (!moved) clickHandler(e.target);\n  }\n\n  // F" +
    "ind touch object with specified identifier.\n  function findTouch(tlist, id) {\n  " +
    "  for (const t of tlist) {\n      if (t.identifier == id) return t;\n    }\n    ret" +
    "urn null;\n  }\n\n  // Return distance between two touch points\n  function touchGap" +
    "(t1, t2) {\n    const dx = t1.clientX - t2.clientX;\n    const dy = t1.clientY - t" +
    "2.clientY;\n    return Math.hypot(dx, dy);\n  }\n\n  function handleTouchStart(e) {\n" +
    "    if (mode == IDLE && e.changedTouches.length == 1) {\n      // Start touch bas" +
    "ed panning\n      const t = e.changedTouches[0];\n      setMode(TOUCHPAN);\n      t" +
    "ouchid = t.identifier;\n      panStart(t.clientX, t.clientY);\n      e.preventDefa" +
    "ult();\n    } else if (mode == TOUCHPAN && e.touches.length == 2) {\n      // Star" +
    "t pinch zooming\n      setMode(TOUCHZOOM);\n      const t1 = e.touches[0];\n      c" +
    "onst t2 = e.touches[1];\n      touchid = t1.identifier;\n      touchid2 = t2.ident" +
    "ifier;\n      initScale = currentScale;\n      initGap = touchGap(t1, t2);\n      c" +
    "enterPoint = toSvg((t1.clientX + t2.clientX) / 2,\n                          (t1." +
    "clientY + t2.clientY) / 2);\n      e.preventDefault();\n    }\n  }\n\n  function hand" +
    "leTouchMove(e) {\n    if (mode == TOUCHPAN) {\n      const t = findTouch(e.changed" +
    "Touches, touchid);\n      if (t == null) return;\n      if (e.touches.length != 1)" +
    " {\n        setMode(IDLE);\n        return;\n      }\n      panMove(t.clientX, t.cli" +
    "entY);\n      e.preventDefault();\n    } else if (mode == TOUCHZOOM) {\n      // Ge" +
    "t two touches; new gap; rescale to ratio.\n      const t1 = findTouch(e.touches, " +
    "touchid);\n      const t2 = findTouch(e.touches, touchid2);\n      if (t1 == null " +
    "|| t2 == null) return;\n      const gap = touchGap(t1, t2);\n      rescale(initSca" +
    "le * gap / initGap, centerPoint);\n      e.preventDefault();\n    }\n  }\n\n  functio" +
    "n handleTouchEnd(e) {\n    if (mode == TOUCHPAN) {\n      const t = findTouch(e.ch" +
    "angedTouches, touchid);\n      if (t == null) return;\n      panMove(t.clientX, t." +
    "clientY);\n      setMode(IDLE);\n      e.preventDefault();\n      if (!moved) click" +
    "Handler(t.target);\n    } else if (mode == TOUCHZOOM) {\n      setMode(IDLE);\n    " +
    "  e.preventDefault();\n    }\n  }\n\n  svg.addEventListener(\'mousedown\', handleScanS" +
    "tart);\n  svg.addEventListener(\'mouseup\', handleScanEnd);\n  svg.addEventListener(" +
    "\'touchstart\', handleTouchStart);\n  svg.addEventListener(\'touchmove\', handleTouch" +
    "Move);\n  svg.addEventListener(\'touchend\', handleTouchEnd);\n  svg.addEventListene" +
    "r(\'wheel\', handleWheel, true);\n}\n\nfunction initMenus() {\n  \'use strict\';\n\n  let " +
    "activeMenu = null;\n  let activeMenuHdr = null;\n\n  function cancelActiveMenu() {\n" +
    "    if (activeMenu == null) return;\n    activeMenu.style.display = \'none\';\n    a" +
    "ctiveMenu = null;\n    activeMenuHdr = null;\n  }\n\n  // Set click handlers on ever" +
    "y menu header.\n  for (const menu of document.getElementsByClassName(\'submenu\')) " +
    "{\n    const hdr = menu.parentElement;\n    if (hdr == null) return;\n    if (hdr.c" +
    "lassList.contains(\'disabled\')) return;\n    function showMenu(e) {\n      // menu " +
    "is a child of hdr, so this event can fire for clicks\n      // inside menu. Ignor" +
    "e such clicks.\n      if (e.target.parentElement != hdr) return;\n      activeMenu" +
    " = menu;\n      activeMenuHdr = hdr;\n      menu.style.display = \'block\';\n    }\n  " +
    "  hdr.addEventListener(\'mousedown\', showMenu);\n    hdr.addEventListener(\'touchst" +
    "art\', showMenu);\n  }\n\n  // If there is an active menu and a down event outside, " +
    "retract the menu.\n  for (const t of [\'mousedown\', \'touchstart\']) {\n    document." +
    "addEventListener(t, (e) => {\n      // Note: to avoid unnecessary flicker, if the" +
    " down event is inside\n      // the active menu header, do not retract the menu.\n" +
    "      if (activeMenuHdr != e.target.closest(\'.menu-item\')) {\n        cancelActiv" +
    "eMenu();\n      }\n    }, { passive: true, capture: true });\n  }\n\n  // If there is" +
    " an active menu and an up event inside, retract the menu.\n  document.addEventLis" +
    "tener(\'mouseup\', (e) => {\n    if (activeMenu == e.target.closest(\'.submenu\')) {\n" +
    "      cancelActiveMenu();\n    }\n  }, { passive: true, capture: true });\n}\n\nfunct" +
    "ion sendURL(method, url, done) {\n  fetch(url.toString(), {method: method})\n     " +
    " .then((response) => { done(response.ok); })\n      .catch((error) => { done(fals" +
    "e); });\n}\n\n// Initialize handlers for saving/loading configurations.\nfunction in" +
    "itConfigManager() {\n  \'use strict\';\n\n  // Initialize various elements.\n  functio" +
    "n elem(id) {\n    const result = document.getElementById(id);\n    if (!result) co" +
    "nsole.warn(\'element \' + id + \' not found\');\n    return result;\n  }\n  const overl" +
    "ay = elem(\'dialog-overlay\');\n  const saveDialog = elem(\'save-dialog\');\n  const s" +
    "aveInput = elem(\'save-name\');\n  const saveError = elem(\'save-error\');\n  const de" +
    "lDialog = elem(\'delete-dialog\');\n  const delPrompt = elem(\'delete-prompt\');\n  co" +
    "nst delError = elem(\'delete-error\');\n\n  let currentDialog = null;\n  let currentD" +
    "eleteTarget = null;\n\n  function showDialog(dialog) {\n    if (currentDialog != nu" +
    "ll) {\n      overlay.style.display = \'none\';\n      currentDialog.style.display = " +
    "\'none\';\n    }\n    currentDialog = dialog;\n    if (dialog != null) {\n      overla" +
    "y.style.display = \'block\';\n      dialog.style.display = \'block\';\n    }\n  }\n\n  fu" +
    "nction cancelDialog(e) {\n    showDialog(null);\n  }\n\n  // Show dialog for saving " +
    "the current config.\n  function showSaveDialog(e) {\n    saveError.innerText = \'\';" +
    "\n    showDialog(saveDialog);\n    saveInput.focus();\n  }\n\n  // Commit save config" +
    ".\n  function commitSave(e) {\n    const name = saveInput.value;\n    const url = n" +
    "ew URL(document.URL);\n    // Set path relative to existing path.\n    url.pathnam" +
    "e = new URL(\'./saveconfig\', document.URL).pathname;\n    url.searchParams.set(\'co" +
    "nfig\', name);\n    saveError.innerText = \'\';\n    sendURL(\'POST\', url, (ok) => {\n " +
    "     if (!ok) {\n        saveError.innerText = \'Save failed\';\n      } else {\n    " +
    "    showDialog(null);\n        location.reload();  // Reload to show updated conf" +
    "ig menu\n      }\n    });\n  }\n\n  function handleSaveInputKey(e) {\n    if (e.key ==" +
    "= \'Enter\') commitSave(e);\n  }\n\n  function deleteConfig(e, elem) {\n    e.preventD" +
    "efault();\n    const config = elem.dataset.config;\n    delPrompt.innerText = \'Del" +
    "ete \' + config + \'?\';\n    currentDeleteTarget = elem;\n    showDialog(delDialog);" +
    "\n  }\n\n  function commitDelete(e, elem) {\n    if (!currentDeleteTarget) return;\n " +
    "   const config = currentDeleteTarget.dataset.config;\n    const url = new URL(\'." +
    "/deleteconfig\', document.URL);\n    url.searchParams.set(\'config\', config);\n    d" +
    "elError.innerText = \'\';\n    sendURL(\'DELETE\', url, (ok) => {\n      if (!ok) {\n  " +
    "      delError.innerText = \'Delete failed\';\n        return;\n      }\n      showDi" +
    "alog(null);\n      // Remove menu entry for this config.\n      if (currentDeleteT" +
    "arget && currentDeleteTarget.parentElement) {\n        currentDeleteTarget.parent" +
    "Element.remove();\n      }\n    });\n  }\n\n  // Bind event on elem to fn.\n  function" +
    " bind(event, elem, fn) {\n    if (elem == null) return;\n    elem.addEventListener" +
    "(event, fn);\n    if (event == \'click\') {\n      // Also enable via touch.\n      e" +
    "lem.addEventListener(\'touchstart\', fn);\n    }\n  }\n\n  bind(\'click\', elem(\'save-co" +
    "nfig\'), showSaveDialog);\n  bind(\'click\', elem(\'save-cancel\'), cancelDialog);\n  b" +
    "ind(\'click\', elem(\'save-confirm\'), commitSave);\n  bind(\'keydown\', saveInput, han" +
    "dleSaveInputKey);\n\n  bind(\'click\', elem(\'delete-cancel\'), cancelDialog);\n  bind(" +
    "\'click\', elem(\'delete-confirm\'), commitDelete);\n\n  // Activate deletion button f" +
    "or all config entries in menu.\n  for (const del of Array.from(document.getElemen" +
    "tsByClassName(\'menu-delete-btn\'))) {\n    bind(\'click\', del, (e) => {\n      delet" +
    "eConfig(e, del);\n    });\n  }\n}\n\nfunction viewer(baseUrl, nodes) {\n  \'use strict\'" +
    ";\n\n  // Elements\n  const search = document.getElementById(\'search\');\n  const gra" +
    "ph0 = document.getElementById(\'graph0\');\n  const svg = (graph0 == null ? null : " +
    "graph0.parentElement);\n  const toptable = document.getElementById(\'toptable\');\n\n" +
    "  let regexpActive = false;\n  let selected = new Map();\n  let origFill = new Map" +
    "();\n  let searchAlarm = null;\n  let buttonsEnabled = true;\n\n  function handleDet" +
    "ails(e) {\n    e.preventDefault();\n    const detailsText = document.getElementByI" +
    "d(\'detailsbox\');\n    if (detailsText != null) {\n      if (detailsText.style.disp" +
    "lay === \'block\') {\n        detailsText.style.display = \'none\';\n      } else {\n  " +
    "      detailsText.style.display = \'block\';\n      }\n    }\n  }\n\n  function handleK" +
    "ey(e) {\n    if (e.keyCode != 13) return;\n    setHrefParams(window.location, func" +
    "tion (params) {\n      params.set(\'f\', search.value);\n    });\n    e.preventDefaul" +
    "t();\n  }\n\n  function handleSearch() {\n    // Delay expensive processing so a flu" +
    "rry of key strokes is handled once.\n    if (searchAlarm != null) {\n      clearTi" +
    "meout(searchAlarm);\n    }\n    searchAlarm = setTimeout(selectMatching, 300);\n\n  " +
    "  regexpActive = true;\n    updateButtons();\n  }\n\n  function selectMatching() {\n " +
    "   searchAlarm = null;\n    let re = null;\n    if (search.value != \'\') {\n      tr" +
    "y {\n        re = new RegExp(search.value);\n      } catch (e) {\n        // TODO: " +
    "Display error state in search box\n        return;\n      }\n    }\n\n    function ma" +
    "tch(text) {\n      return re != null && re.test(text);\n    }\n\n    // drop current" +
    "ly selected items that do not match re.\n    selected.forEach(function(v, n) {\n  " +
    "    if (!match(nodes[n])) {\n        unselect(n, document.getElementById(\'node\' +" +
    " n));\n      }\n    })\n\n    // add matching items that are not currently selected." +
    "\n    if (nodes) {\n      for (let n = 0; n < nodes.length; n++) {\n        if (!se" +
    "lected.has(n) && match(nodes[n])) {\n          select(n, document.getElementById(" +
    "\'node\' + n));\n        }\n      }\n    }\n\n    updateButtons();\n  }\n\n  function togg" +
    "leSvgSelect(elem) {\n    // Walk up to immediate child of graph0\n    while (elem " +
    "!= null && elem.parentElement != graph0) {\n      elem = elem.parentElement;\n    " +
    "}\n    if (!elem) return;\n\n    // Disable regexp mode.\n    regexpActive = false;\n" +
    "\n    const n = nodeId(elem);\n    if (n < 0) return;\n    if (selected.has(n)) {\n " +
    "     unselect(n, elem);\n    } else {\n      select(n, elem);\n    }\n    updateButt" +
    "ons();\n  }\n\n  function unselect(n, elem) {\n    if (elem == null) return;\n    sel" +
    "ected.delete(n);\n    setBackground(elem, false);\n  }\n\n  function select(n, elem)" +
    " {\n    if (elem == null) return;\n    selected.set(n, true);\n    setBackground(el" +
    "em, true);\n  }\n\n  function nodeId(elem) {\n    const id = elem.id;\n    if (!id) r" +
    "eturn -1;\n    if (!id.startsWith(\'node\')) return -1;\n    const n = parseInt(id.s" +
    "lice(4), 10);\n    if (isNaN(n)) return -1;\n    if (n < 0 || n >= nodes.length) r" +
    "eturn -1;\n    return n;\n  }\n\n  function setBackground(elem, set) {\n    // Handle" +
    " table row highlighting.\n    if (elem.nodeName == \'TR\') {\n      elem.classList.t" +
    "oggle(\'hilite\', set);\n      return;\n    }\n\n    // Handle svg element highlightin" +
    "g.\n    const p = findPolygon(elem);\n    if (p != null) {\n      if (set) {\n      " +
    "  origFill.set(p, p.style.fill);\n        p.style.fill = \'#ccccff\';\n      } else " +
    "if (origFill.has(p)) {\n        p.style.fill = origFill.get(p);\n      }\n    }\n  }" +
    "\n\n  function findPolygon(elem) {\n    if (elem.localName == \'polygon\') return ele" +
    "m;\n    for (const c of elem.children) {\n      const p = findPolygon(c);\n      if" +
    " (p != null) return p;\n    }\n    return null;\n  }\n\n  // convert a string to a re" +
    "gexp that matches that string.\n  function quotemeta(str) {\n    return str.replac" +
    "e(/([\\\\\\.?+*\\[\\](){}|^$])/g, \'\\\\$1\');\n  }\n\n  function setSampleIndexLink(id) {\n " +
    "   const elem = document.getElementById(id);\n    if (elem != null) {\n      setHr" +
    "efParams(elem, function (params) {\n        params.set(\"si\", id);\n      });\n    }" +
    "\n  }\n\n  // Update id\'s href to reflect current selection whenever it is\n  // lia" +
    "ble to be followed.\n  function makeSearchLinkDynamic(id) {\n    const elem = docu" +
    "ment.getElementById(id);\n    if (elem == null) return;\n\n    // Most links copy c" +
    "urrent selection into the \'f\' parameter,\n    // but Refine menu links are differ" +
    "ent.\n    let param = \'f\';\n    if (id == \'ignore\') param = \'i\';\n    if (id == \'hi" +
    "de\') param = \'h\';\n    if (id == \'show\') param = \'s\';\n    if (id == \'show-from\') " +
    "param = \'sf\';\n\n    // We update on mouseenter so middle-click/right-click work p" +
    "roperly.\n    elem.addEventListener(\'mouseenter\', updater);\n    elem.addEventList" +
    "ener(\'touchstart\', updater);\n\n    function updater() {\n      // The selection ca" +
    "n be in one of two modes: regexp-based or\n      // list-based.  Construct regula" +
    "r expression depending on mode.\n      let re = regexpActive\n        ? search.val" +
    "ue\n        : Array.from(selected.keys()).map(key => quotemeta(nodes[key])).join(" +
    "\'|\');\n\n      setHrefParams(elem, function (params) {\n        if (re != \'\') {\n   " +
    "       // For focus/show/show-from, forget old parameter. For others, add to re." +
    "\n          if (param != \'f\' && param != \'s\' && param != \'sf\' && params.has(param" +
    ")) {\n            const old = params.get(param);\n            if (old != \'\') {\n   " +
    "           re += \'|\' + old;\n            }\n          }\n          params.set(param" +
    ", re);\n        } else {\n          params.delete(param);\n        }\n      });\n    " +
    "}\n  }\n\n  function setHrefParams(elem, paramSetter) {\n    let url = new URL(elem." +
    "href);\n    url.hash = \'\';\n\n    // Copy params from this page\'s URL.\n    const pa" +
    "rams = url.searchParams;\n    for (const p of new URLSearchParams(window.location" +
    ".search)) {\n      params.set(p[0], p[1]);\n    }\n\n    // Give the params to the s" +
    "etter to modify.\n    paramSetter(params);\n\n    elem.href = url.toString();\n  }\n\n" +
    "  function handleTopClick(e) {\n    // Walk back until we find TR and then get th" +
    "e Name column (index 5)\n    let elem = e.target;\n    while (elem != null && elem" +
    ".nodeName != \'TR\') {\n      elem = elem.parentElement;\n    }\n    if (elem == null" +
    " || elem.children.length < 6) return;\n\n    e.preventDefault();\n    const tr = el" +
    "em;\n    const td = elem.children[5];\n    if (td.nodeName != \'TD\') return;\n    co" +
    "nst name = td.innerText;\n    const index = nodes.indexOf(name);\n    if (index < " +
    "0) return;\n\n    // Disable regexp mode.\n    regexpActive = false;\n\n    if (selec" +
    "ted.has(index)) {\n      unselect(index, elem);\n    } else {\n      select(index, " +
    "elem);\n    }\n    updateButtons();\n  }\n\n  function updateButtons() {\n    const en" +
    "able = (search.value != \'\' || selected.size != 0);\n    if (buttonsEnabled == ena" +
    "ble) return;\n    buttonsEnabled = enable;\n    for (const id of [\'focus\', \'ignore" +
    "\', \'hide\', \'show\', \'show-from\']) {\n      const link = document.getElementById(id" +
    ");\n      if (link != null) {\n        link.classList.toggle(\'disabled\', !enable);" +
    "\n      }\n    }\n  }\n\n  // Initialize button states\n  updateButtons();\n\n  // Setup" +
    " event handlers\n  initMenus();\n  if (svg != null) {\n    initPanAndZoom(svg, togg" +
    "leSvgSelect);\n  }\n  if (toptable != null) {\n    toptable.addEventListener(\'mouse" +
    "down\', handleTopClick);\n    toptable.addEventListener(\'touchstart\', handleTopCli" +
    "ck);\n  }\n\n  const ids = [\'topbtn\', \'graphbtn\', \'flamegraph\', \'peek\', \'list\', \'di" +
    "sasm\',\n               \'focus\', \'ignore\', \'hide\', \'show\', \'show-from\'];\n  ids.for" +
    "Each(makeSearchLinkDynamic);\n\n  const sampleIDs = [{{range .SampleTypes}}\'{{.}}\'" +
    ", {{end}}];\n  sampleIDs.forEach(setSampleIndexLink);\n\n  // Bind action to button" +
    " with specified id.\n  function addAction(id, action) {\n    const btn = document." +
    "getElementById(id);\n    if (btn != null) {\n      btn.addEventListener(\'click\', a" +
    "ction);\n      btn.addEventListener(\'touchstart\', action);\n    }\n  }\n\n  addAction" +
    "(\'details\', handleDetails);\n  initConfigManager();\n\n  search.addEventListener(\'i" +
    "nput\', handleSearch);\n  search.addEventListener(\'keydown\', handleKey);\n\n  // Giv" +
    "e initial focus to main container so it can be scrolled using keys.\n  const main" +
    " = document.getElementById(\'bodycontainer\');\n  if (main) {\n    main.focus();\n  }" +
    "\n}\n</script>\n{{end}}\n\n{{define \"top\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta c" +
    "harset=\"utf-8\">\n  <title>{{.Title}}</title>\n  {{template \"css\" .}}\n  <style type" +
    "=\"text/css\">\n  </style>\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"top\"" +
    ">\n    <table id=\"toptable\">\n      <thead>\n        <tr>\n          <th id=\"flathdr" +
    "1\">Flat</th>\n          <th id=\"flathdr2\">Flat%</th>\n          <th>Sum%</th>\n    " +
    "      <th id=\"cumhdr1\">Cum</th>\n          <th id=\"cumhdr2\">Cum%</th>\n          <" +
    "th id=\"namehdr\">Name</th>\n          <th>Inlined?</th>\n        </tr>\n      </thea" +
    "d>\n      <tbody id=\"rows\"></tbody>\n    </table>\n  </div>\n  {{template \"script\" ." +
    "}}\n  <script>\n    function makeTopTable(total, entries) {\n      const rows = doc" +
    "ument.getElementById(\'rows\');\n      if (rows == null) return;\n\n      // Store in" +
    "itial index in each entry so we have stable node ids for selection.\n      for (l" +
    "et i = 0; i < entries.length; i++) {\n        entries[i].Id = \'node\' + i;\n      }" +
    "\n\n      // Which column are we currently sorted by and in what order?\n      let " +
    "currentColumn = \'\';\n      let descending = false;\n      sortBy(\'Flat\');\n\n      f" +
    "unction sortBy(column) {\n        // Update sort criteria\n        if (column == c" +
    "urrentColumn) {\n          descending = !descending; // Reverse order\n        } e" +
    "lse {\n          currentColumn = column;\n          descending = (column != \'Name\'" +
    ");\n        }\n\n        // Sort according to current criteria.\n        function cm" +
    "p(a, b) {\n          const av = a[currentColumn];\n          const bv = b[currentC" +
    "olumn];\n          if (av < bv) return -1;\n          if (av > bv) return +1;\n    " +
    "      return 0;\n        }\n        entries.sort(cmp);\n        if (descending) ent" +
    "ries.reverse();\n\n        function addCell(tr, val) {\n          const td = docume" +
    "nt.createElement(\'td\');\n          td.textContent = val;\n          tr.appendChild" +
    "(td);\n        }\n\n        function percent(v) {\n          return (v * 100.0 / tot" +
    "al).toFixed(2) + \'%\';\n        }\n\n        // Generate rows\n        const fragment" +
    " = document.createDocumentFragment();\n        let sum = 0;\n        for (const ro" +
    "w of entries) {\n          const tr = document.createElement(\'tr\');\n          tr." +
    "id = row.Id;\n          sum += row.Flat;\n          addCell(tr, row.FlatFormat);\n " +
    "         addCell(tr, percent(row.Flat));\n          addCell(tr, percent(sum));\n  " +
    "        addCell(tr, row.CumFormat);\n          addCell(tr, percent(row.Cum));\n   " +
    "       addCell(tr, row.Name);\n          addCell(tr, row.InlineLabel);\n          " +
    "fragment.appendChild(tr);\n        }\n\n        rows.textContent = \'\'; // Remove ol" +
    "d rows\n        rows.appendChild(fragment);\n      }\n\n      // Make different colu" +
    "mn headers trigger sorting.\n      function bindSort(id, column) {\n        const " +
    "hdr = document.getElementById(id);\n        if (hdr == null) return;\n        cons" +
    "t fn = function() { sortBy(column) };\n        hdr.addEventListener(\'click\', fn);" +
    "\n        hdr.addEventListener(\'touch\', fn);\n      }\n      bindSort(\'flathdr1\', \'" +
    "Flat\');\n      bindSort(\'flathdr2\', \'Flat\');\n      bindSort(\'cumhdr1\', \'Cum\');\n  " +
    "    bindSort(\'cumhdr2\', \'Cum\');\n      bindSort(\'namehdr\', \'Name\');\n    }\n\n    vi" +
    "ewer(new URL(window.location.href), {{.Nodes}});\n    makeTopTable({{.Total}}, {{" +
    ".Top}});\n  </script>\n</body>\n</html>\n{{end}}\n\n{{define \"sourcelisting\" -}}\n<!DOC" +
    "TYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  <title>{{.Title}}</title>\n  " +
    "{{template \"css\" .}}\n  {{template \"weblistcss\" .}}\n  {{template \"weblistjs\" .}}\n" +
    "</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"content\" class=\"source\">\n   " +
    " {{.HTMLBody}}\n  </div>\n  {{template \"script\" .}}\n  <script>viewer(new URL(windo" +
    "w.location.href), null);</script>\n</body>\n</html>\n{{end}}\n\n{{define \"plaintext\" " +
    "-}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  <title>{{.Title}}</" +
    "title>\n  {{template \"css\" .}}\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id" +
    "=\"content\">\n    <pre>\n      {{.TextBody}}\n    </pre>\n  </div>\n  {{template \"scri" +
    "pt\" .}}\n  <script>viewer(new URL(window.location.href), null);</script>\n</body>\n" +
    "</html>\n{{end}}\n\n{{define \"flamegraph\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta" +
    " charset=\"utf-8\">\n  <title>{{.Title}}</title>\n  {{template \"css\" .}}\n  <style ty" +
    "pe=\"text/css\">{{template \"d3flamegraphcss\" .}}</style>\n  <style type=\"text/css\">" +
    "\n    .flamegraph-content {\n      width: 90%;\n      min-width: 80%;\n      margin-" +
    "left: 5%;\n    }\n    .flamegraph-details {\n      height: 1.2em;\n      width: 90%;" +
    "\n      min-width: 90%;\n      margin-left: 5%;\n      padding: 15px 0 35px;\n    }\n" +
    "  </style>\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"bodycontainer\">\n " +
    "   <div id=\"flamegraphdetails\" class=\"flamegraph-details\"></div>\n    <div class=" +
    "\"flamegraph-content\">\n      <div id=\"chart\"></div>\n    </div>\n  </div>\n  {{templ" +
    "ate \"script\" .}}\n  <script>viewer(new URL(window.location.href), {{.Nodes}});</s" +
    "cript>\n  <script>{{template \"d3script\" .}}</script>\n  <script>{{template \"d3flam" +
    "egraphscript\" .}}</script>\n  <script>\n    var data = {{.FlameGraph}};\n\n    var w" +
    "idth = document.getElementById(\'chart\').clientWidth;\n\n    var flameGraph = d3.fl" +
    "amegraph()\n      .width(width)\n      .cellHeight(18)\n      .minFrameSize(1)\n    " +
    "  .transitionDuration(750)\n      .transitionEase(d3.easeCubic)\n      .inverted(t" +
    "rue)\n      .sort(true)\n      .title(\'\')\n      .tooltip(false)\n      .details(doc" +
    "ument.getElementById(\'flamegraphdetails\'));\n\n    // <full name> (percentage, val" +
    "ue)\n    flameGraph.label((d) => d.data.f + \' (\' + d.data.p + \', \' + d.data.l + \'" +
    ")\');\n\n    (function(flameGraph) {\n      var oldColorMapper = flameGraph.color();" +
    "\n      function colorMapper(d) {\n        // Hack to force default color mapper t" +
    "o use \'warm\' color scheme by not passing libtype\n        const { data, highlight" +
    " } = d;\n        return oldColorMapper({ data: { n: data.n }, highlight });\n     " +
    " }\n\n      flameGraph.color(colorMapper);\n    }(flameGraph));\n\n    d3.select(\'#ch" +
    "art\')\n      .datum(data)\n      .call(flameGraph);\n\n    function clear() {\n      " +
    "flameGraph.clear();\n    }\n\n    function resetZoom() {\n      flameGraph.resetZoom" +
    "();\n    }\n\n    window.addEventListener(\'resize\', function() {\n      var width = " +
    "document.getElementById(\'chart\').clientWidth;\n      var graphs = document.getEle" +
    "mentsByClassName(\'d3-flame-graph\');\n      if (graphs.length > 0) {\n        graph" +
    "s[0].setAttribute(\'width\', width);\n      }\n      flameGraph.width(width);\n      " +
    "flameGraph.resetZoom();\n    }, true);\n\n    var search = document.getElementById(" +
    "\'search\');\n    var searchAlarm = null;\n\n    function selectMatching() {\n      se" +
    "archAlarm = null;\n\n      if (search.value != \'\') {\n        flameGraph.search(sea" +
    "rch.value);\n      } else {\n        flameGraph.clear();\n      }\n    }\n\n    functi" +
    "on handleSearch() {\n      // Delay expensive processing so a flurry of key strok" +
    "es is handled once.\n      if (searchAlarm != null) {\n        clearTimeout(search" +
    "Alarm);\n      }\n      searchAlarm = setTimeout(selectMatching, 300);\n    }\n\n    " +
    "search.addEventListener(\'input\', handleSearch);\n  </script>\n</body>\n</html>\n{{en" +
    "d}}\n"));
}

} // end driver_package
