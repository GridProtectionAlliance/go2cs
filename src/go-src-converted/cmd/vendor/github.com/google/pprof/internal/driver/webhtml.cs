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

// package driver -- go2cs converted at 2020 October 09 05:53:32 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\webhtml.go
using template = go.html.template_package;

using d3 = go.github.com.google.pprof.third_party.d3_package;
using d3flamegraph = go.github.com.google.pprof.third_party.d3flamegraph_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        // addTemplates adds a set of template definitions to templates.
        private static void addTemplates(ptr<template.Template> _addr_templates)
        {
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
    "argin-right: 1rem;\n}\n.header .title a {\n  color: #212121;\n  text-decoration: non" +
    "e;\n}\n.header .title a:hover {\n  text-decoration: underline;\n}\n.header .descripti" +
    "on {\n  width: 100%;\n  text-align: right;\n  white-space: nowrap;\n}\n@media screen " +
    "and (max-width: 799px) {\n  .header input {\n    display: none;\n  }\n}\n#detailsbox " +
    "{\n  display: none;\n  z-index: 1;\n  position: fixed;\n  top: 40px;\n  right: 20px;\n" +
    "  background-color: #ffffff;\n  box-shadow: 0 1px 5px rgba(0,0,0,.3);\n  line-heig" +
    "ht: 24px;\n  padding: 1em;\n  text-align: left;\n}\n.header input {\n  background: wh" +
    "ite url(\"data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' viewBox=\'0" +
    " 0 24 24\' style=\'pointer-events:none;display:block;width:100%25;height:100%25;fi" +
    "ll:%23757575\'%3E%3Cpath d=\'M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16" +
    " 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61.0 3.09-.59 4.23-1.57l.27.28" +
    "v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14" +
    " 9.5 11.99 14 9.5 14z\'/%3E%3C/svg%3E\") no-repeat 4px center/20px 20px;\n  border:" +
    " 1px solid #d1d2d3;\n  border-radius: 2px 0 0 2px;\n  padding: 0.25em;\n  padding-l" +
    "eft: 28px;\n  margin-left: 1em;\n  font-family: \'Roboto\', \'Noto\', sans-serif;\n  fo" +
    "nt-size: 1em;\n  line-height: 24px;\n  color: #212121;\n}\n.downArrow {\n  border-top" +
    ": .36em solid #ccc;\n  border-left: .36em solid transparent;\n  border-right: .36e" +
    "m solid transparent;\n  margin-bottom: .05em;\n  margin-left: .5em;\n  transition: " +
    "border-top-color 200ms;\n}\n.menu-item {\n  height: 100%;\n  text-transform: upperca" +
    "se;\n  font-family: \'Roboto Medium\', -apple-system, BlinkMacSystemFont, \'Segoe UI" +
    "\', Helvetica, Arial, sans-serif, \'Apple Color Emoji\', \'Segoe UI Emoji\', \'Segoe U" +
    "I Symbol\';\n  position: relative;\n}\n.menu-item .menu-name:hover {\n  opacity: 0.75" +
    ";\n}\n.menu-item .menu-name:hover .downArrow {\n  border-top-color: #666;\n}\n.menu-n" +
    "ame {\n  height: 100%;\n  padding: 0 0.5em;\n  display: flex;\n  align-items: center" +
    ";\n  justify-content: center;\n}\n.submenu {\n  display: none;\n  z-index: 1;\n  margi" +
    "n-top: -4px;\n  min-width: 10em;\n  position: absolute;\n  left: 0px;\n  background-" +
    "color: white;\n  box-shadow: 0 1px 5px rgba(0,0,0,.3);\n  font-size: 100%;\n  text-" +
    "transform: none;\n}\n.menu-item, .submenu {\n  user-select: none;\n  -moz-user-selec" +
    "t: none;\n  -ms-user-select: none;\n  -webkit-user-select: none;\n}\n.submenu hr {\n " +
    " border: 0;\n  border-top: 2px solid #eee;\n}\n.submenu a {\n  display: block;\n  pad" +
    "ding: .5em 1em;\n  text-decoration: none;\n}\n.submenu a:hover, .submenu a.active {" +
    "\n  color: white;\n  background-color: #6b82d6;\n}\n.submenu a.disabled {\n  color: g" +
    "ray;\n  pointer-events: none;\n}\n\n#content {\n  overflow-y: scroll;\n  padding: 1em;" +
    "\n}\n#top {\n  overflow-y: scroll;\n}\n#graph {\n  overflow: hidden;\n}\n#graph svg {\n  " +
    "width: 100%;\n  height: auto;\n  padding: 10px;\n}\n#content.source .filename {\n  ma" +
    "rgin-top: 0;\n  margin-bottom: 1em;\n  font-size: 120%;\n}\n#content.source pre {\n  " +
    "margin-bottom: 3em;\n}\ntable {\n  border-spacing: 0px;\n  width: 100%;\n  padding-bo" +
    "ttom: 1em;\n  white-space: nowrap;\n}\ntable thead {\n  font-family: \'Roboto Medium\'" +
    ", -apple-system, BlinkMacSystemFont, \'Segoe UI\', Helvetica, Arial, sans-serif, \'" +
    "Apple Color Emoji\', \'Segoe UI Emoji\', \'Segoe UI Symbol\';\n}\ntable tr th {\n  backg" +
    "round-color: #ddd;\n  text-align: right;\n  padding: .3em .5em;\n}\ntable tr td {\n  " +
    "padding: .3em .5em;\n  text-align: right;\n}\n#top table tr th:nth-child(6),\n#top t" +
    "able tr th:nth-child(7),\n#top table tr td:nth-child(6),\n#top table tr td:nth-chi" +
    "ld(7) {\n  text-align: left;\n}\n#top table tr td:nth-child(6) {\n  width: 100%;\n  t" +
    "ext-overflow: ellipsis;\n  overflow: hidden;\n  white-space: nowrap;\n}\n#flathdr1, " +
    "#flathdr2, #cumhdr1, #cumhdr2, #namehdr {\n  cursor: ns-resize;\n}\n.hilite {\n  bac" +
    "kground-color: #ebf5fb;\n  font-weight: bold;\n}\n</style>\n{{end}}\n\n{{define \"heade" +
    "r\"}}\n<div class=\"header\">\n  <div class=\"title\">\n    <h1><a href=\"./\">pprof</a></" +
    "h1>\n  </div>\n\n  <div id=\"view\" class=\"menu-item\">\n    <div class=\"menu-name\">\n  " +
    "    View\n      <i class=\"downArrow\"></i>\n    </div>\n    <div class=\"submenu\">\n  " +
    "    <a title=\"{{.Help.top}}\"  href=\"./top\" id=\"topbtn\">Top</a>\n      <a title=\"{" +
    "{.Help.graph}}\" href=\"./\" id=\"graphbtn\">Graph</a>\n      <a title=\"{{.Help.flameg" +
    "raph}}\" href=\"./flamegraph\" id=\"flamegraph\">Flame Graph</a>\n      <a title=\"{{.H" +
    "elp.peek}}\" href=\"./peek\" id=\"peek\">Peek</a>\n      <a title=\"{{.Help.list}}\" hre" +
    "f=\"./source\" id=\"list\">Source</a>\n      <a title=\"{{.Help.disasm}}\" href=\"./disa" +
    "sm\" id=\"disasm\">Disassemble</a>\n    </div>\n  </div>\n\n  {{$sampleLen := len .Samp" +
    "leTypes}}\n  {{if gt $sampleLen 1}}\n  <div id=\"sample\" class=\"menu-item\">\n    <di" +
    "v class=\"menu-name\">\n      Sample\n      <i class=\"downArrow\"></i>\n    </div>\n   " +
    " <div class=\"submenu\">\n      {{range .SampleTypes}}\n      <a href=\"?si={{.}}\" id" +
    "=\"{{.}}\">{{.}}</a>\n      {{end}}\n    </div>\n  </div>\n  {{end}}\n\n  <div id=\"refin" +
    "e\" class=\"menu-item\">\n    <div class=\"menu-name\">\n      Refine\n      <i class=\"d" +
    "ownArrow\"></i>\n    </div>\n    <div class=\"submenu\">\n      <a title=\"{{.Help.focu" +
    "s}}\" href=\"?\" id=\"focus\">Focus</a>\n      <a title=\"{{.Help.ignore}}\" href=\"?\" id" +
    "=\"ignore\">Ignore</a>\n      <a title=\"{{.Help.hide}}\" href=\"?\" id=\"hide\">Hide</a>" +
    "\n      <a title=\"{{.Help.show}}\" href=\"?\" id=\"show\">Show</a>\n      <a title=\"{{." +
    "Help.show_from}}\" href=\"?\" id=\"show-from\">Show from</a>\n      <hr>\n      <a titl" +
    "e=\"{{.Help.reset}}\" href=\"?\">Reset</a>\n    </div>\n  </div>\n\n  <div>\n    <input i" +
    "d=\"search\" type=\"text\" placeholder=\"Search regexp\" autocomplete=\"off\" autocapita" +
    "lize=\"none\" size=40>\n  </div>\n\n  <div class=\"description\">\n    <a title=\"{{.Help" +
    ".details}}\" href=\"#\" id=\"details\">{{.Title}}</a>\n    <div id=\"detailsbox\">\n     " +
    " {{range .Legend}}<div>{{.}}</div>{{end}}\n    </div>\n  </div>\n</div>\n\n<div id=\"e" +
    "rrors\">{{range .Errors}}<div>{{.}}</div>{{end}}</div>\n{{end}}\n\n{{define \"graph\" " +
    "-}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  <title>{{.Title}}</" +
    "title>\n  {{template \"css\" .}}\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id" +
    "=\"graph\">\n    {{.HTMLBody}}\n  </div>\n  {{template \"script\" .}}\n  <script>viewer(" +
    "new URL(window.location.href), {{.Nodes}});</script>\n</body>\n</html>\n{{end}}\n\n{{" +
    "define \"script\"}}\n<script>\n// Make svg pannable and zoomable.\n// Call clickHandl" +
    "er(t) if a click event is caught by the pan event handlers.\nfunction initPanAndZ" +
    "oom(svg, clickHandler) {\n  \'use strict\';\n\n  // Current mouse/touch handling mode" +
    "\n  const IDLE = 0;\n  const MOUSEPAN = 1;\n  const TOUCHPAN = 2;\n  const TOUCHZOOM" +
    " = 3;\n  let mode = IDLE;\n\n  // State needed to implement zooming.\n  let currentS" +
    "cale = 1.0;\n  const initWidth = svg.viewBox.baseVal.width;\n  const initHeight = " +
    "svg.viewBox.baseVal.height;\n\n  // State needed to implement panning.\n  let panLa" +
    "stX = 0;      // Last event X coordinate\n  let panLastY = 0;      // Last event " +
    "Y coordinate\n  let moved = false;     // Have we seen significant movement\n  let" +
    " touchid = null;    // Current touch identifier\n\n  // State needed for pinch zoo" +
    "ming\n  let touchid2 = null;     // Second id for pinch zooming\n  let initGap = 1" +
    ".0;       // Starting gap between two touches\n  let initScale = 1.0;     // curr" +
    "entScale when pinch zoom started\n  let centerPoint = null;  // Center point for " +
    "scaling\n\n  // Convert event coordinates to svg coordinates.\n  function toSvg(x, " +
    "y) {\n    const p = svg.createSVGPoint();\n    p.x = x;\n    p.y = y;\n    let m = s" +
    "vg.getCTM();\n    if (m == null) m = svg.getScreenCTM(); // Firefox workaround.\n " +
    "   return p.matrixTransform(m.inverse());\n  }\n\n  // Change the scaling for the s" +
    "vg to s, keeping the point denoted\n  // by u (in svg coordinates]) fixed at the " +
    "same screen location.\n  function rescale(s, u) {\n    // Limit to a good range.\n " +
    "   if (s < 0.2) s = 0.2;\n    if (s > 10.0) s = 10.0;\n\n    currentScale = s;\n\n   " +
    " // svg.viewBox defines the visible portion of the user coordinate\n    // system" +
    ".  So to magnify by s, divide the visible portion by s,\n    // which will then b" +
    "e stretched to fit the viewport.\n    const vb = svg.viewBox;\n    const w1 = vb.b" +
    "aseVal.width;\n    const w2 = initWidth / s;\n    const h1 = vb.baseVal.height;\n  " +
    "  const h2 = initHeight / s;\n    vb.baseVal.width = w2;\n    vb.baseVal.height = " +
    "h2;\n\n    // We also want to adjust vb.baseVal.x so that u.x remains at same\n    " +
    "// screen X coordinate.  In other words, want to change it from x1 to x2\n    // " +
    "so that:\n    //     (u.x - x1) / w1 = (u.x - x2) / w2\n    // Simplifying that, w" +
    "e get\n    //     (u.x - x1) * (w2 / w1) = u.x - x2\n    //     x2 = u.x - (u.x - " +
    "x1) * (w2 / w1)\n    vb.baseVal.x = u.x - (u.x - vb.baseVal.x) * (w2 / w1);\n    v" +
    "b.baseVal.y = u.y - (u.y - vb.baseVal.y) * (h2 / h1);\n  }\n\n  function handleWhee" +
    "l(e) {\n    if (e.deltaY == 0) return;\n    // Change scale factor by 1.1 or 1/1.1" +
    "\n    rescale(currentScale * (e.deltaY < 0 ? 1.1 : (1/1.1)),\n            toSvg(e." +
    "offsetX, e.offsetY));\n  }\n\n  function setMode(m) {\n    mode = m;\n    touchid = n" +
    "ull;\n    touchid2 = null;\n  }\n\n  function panStart(x, y) {\n    moved = false;\n  " +
    "  panLastX = x;\n    panLastY = y;\n  }\n\n  function panMove(x, y) {\n    let dx = x" +
    " - panLastX;\n    let dy = y - panLastY;\n    if (Math.abs(dx) <= 2 && Math.abs(dy" +
    ") <= 2) return; // Ignore tiny moves\n\n    moved = true;\n    panLastX = x;\n    pa" +
    "nLastY = y;\n\n    // Firefox workaround: get dimensions from parentNode.\n    cons" +
    "t swidth = svg.clientWidth || svg.parentNode.clientWidth;\n    const sheight = sv" +
    "g.clientHeight || svg.parentNode.clientHeight;\n\n    // Convert deltas from scree" +
    "n space to svg space.\n    dx *= (svg.viewBox.baseVal.width / swidth);\n    dy *= " +
    "(svg.viewBox.baseVal.height / sheight);\n\n    svg.viewBox.baseVal.x -= dx;\n    sv" +
    "g.viewBox.baseVal.y -= dy;\n  }\n\n  function handleScanStart(e) {\n    if (e.button" +
    " != 0) return; // Do not catch right-clicks etc.\n    setMode(MOUSEPAN);\n    panS" +
    "tart(e.clientX, e.clientY);\n    e.preventDefault();\n    svg.addEventListener(\'mo" +
    "usemove\', handleScanMove);\n  }\n\n  function handleScanMove(e) {\n    if (e.buttons" +
    " == 0) {\n      // Missed an end event, perhaps because mouse moved outside windo" +
    "w.\n      setMode(IDLE);\n      svg.removeEventListener(\'mousemove\', handleScanMov" +
    "e);\n      return;\n    }\n    if (mode == MOUSEPAN) panMove(e.clientX, e.clientY);" +
    "\n  }\n\n  function handleScanEnd(e) {\n    if (mode == MOUSEPAN) panMove(e.clientX," +
    " e.clientY);\n    setMode(IDLE);\n    svg.removeEventListener(\'mousemove\', handleS" +
    "canMove);\n    if (!moved) clickHandler(e.target);\n  }\n\n  // Find touch object wi" +
    "th specified identifier.\n  function findTouch(tlist, id) {\n    for (const t of t" +
    "list) {\n      if (t.identifier == id) return t;\n    }\n    return null;\n  }\n\n  //" +
    " Return distance between two touch points\n  function touchGap(t1, t2) {\n    cons" +
    "t dx = t1.clientX - t2.clientX;\n    const dy = t1.clientY - t2.clientY;\n    retu" +
    "rn Math.hypot(dx, dy);\n  }\n\n  function handleTouchStart(e) {\n    if (mode == IDL" +
    "E && e.changedTouches.length == 1) {\n      // Start touch based panning\n      co" +
    "nst t = e.changedTouches[0];\n      setMode(TOUCHPAN);\n      touchid = t.identifi" +
    "er;\n      panStart(t.clientX, t.clientY);\n      e.preventDefault();\n    } else i" +
    "f (mode == TOUCHPAN && e.touches.length == 2) {\n      // Start pinch zooming\n   " +
    "   setMode(TOUCHZOOM);\n      const t1 = e.touches[0];\n      const t2 = e.touches" +
    "[1];\n      touchid = t1.identifier;\n      touchid2 = t2.identifier;\n      initSc" +
    "ale = currentScale;\n      initGap = touchGap(t1, t2);\n      centerPoint = toSvg(" +
    "(t1.clientX + t2.clientX) / 2,\n                          (t1.clientY + t2.client" +
    "Y) / 2);\n      e.preventDefault();\n    }\n  }\n\n  function handleTouchMove(e) {\n  " +
    "  if (mode == TOUCHPAN) {\n      const t = findTouch(e.changedTouches, touchid);\n" +
    "      if (t == null) return;\n      if (e.touches.length != 1) {\n        setMode(" +
    "IDLE);\n        return;\n      }\n      panMove(t.clientX, t.clientY);\n      e.prev" +
    "entDefault();\n    } else if (mode == TOUCHZOOM) {\n      // Get two touches; new " +
    "gap; rescale to ratio.\n      const t1 = findTouch(e.touches, touchid);\n      con" +
    "st t2 = findTouch(e.touches, touchid2);\n      if (t1 == null || t2 == null) retu" +
    "rn;\n      const gap = touchGap(t1, t2);\n      rescale(initScale * gap / initGap," +
    " centerPoint);\n      e.preventDefault();\n    }\n  }\n\n  function handleTouchEnd(e)" +
    " {\n    if (mode == TOUCHPAN) {\n      const t = findTouch(e.changedTouches, touch" +
    "id);\n      if (t == null) return;\n      panMove(t.clientX, t.clientY);\n      set" +
    "Mode(IDLE);\n      e.preventDefault();\n      if (!moved) clickHandler(t.target);\n" +
    "    } else if (mode == TOUCHZOOM) {\n      setMode(IDLE);\n      e.preventDefault(" +
    ");\n    }\n  }\n\n  svg.addEventListener(\'mousedown\', handleScanStart);\n  svg.addEve" +
    "ntListener(\'mouseup\', handleScanEnd);\n  svg.addEventListener(\'touchstart\', handl" +
    "eTouchStart);\n  svg.addEventListener(\'touchmove\', handleTouchMove);\n  svg.addEve" +
    "ntListener(\'touchend\', handleTouchEnd);\n  svg.addEventListener(\'wheel\', handleWh" +
    "eel, true);\n}\n\nfunction initMenus() {\n  \'use strict\';\n\n  let activeMenu = null;\n" +
    "  let activeMenuHdr = null;\n\n  function cancelActiveMenu() {\n    if (activeMenu " +
    "== null) return;\n    activeMenu.style.display = \'none\';\n    activeMenu = null;\n " +
    "   activeMenuHdr = null;\n  }\n\n  // Set click handlers on every menu header.\n  fo" +
    "r (const menu of document.getElementsByClassName(\'submenu\')) {\n    const hdr = m" +
    "enu.parentElement;\n    if (hdr == null) return;\n    if (hdr.classList.contains(\'" +
    "disabled\')) return;\n    function showMenu(e) {\n      // menu is a child of hdr, " +
    "so this event can fire for clicks\n      // inside menu. Ignore such clicks.\n    " +
    "  if (e.target.parentElement != hdr) return;\n      activeMenu = menu;\n      acti" +
    "veMenuHdr = hdr;\n      menu.style.display = \'block\';\n    }\n    hdr.addEventListe" +
    "ner(\'mousedown\', showMenu);\n    hdr.addEventListener(\'touchstart\', showMenu);\n  " +
    "}\n\n  // If there is an active menu and a down event outside, retract the menu.\n " +
    " for (const t of [\'mousedown\', \'touchstart\']) {\n    document.addEventListener(t," +
    " (e) => {\n      // Note: to avoid unnecessary flicker, if the down event is insi" +
    "de\n      // the active menu header, do not retract the menu.\n      if (activeMen" +
    "uHdr != e.target.closest(\'.menu-item\')) {\n        cancelActiveMenu();\n      }\n  " +
    "  }, { passive: true, capture: true });\n  }\n\n  // If there is an active menu and" +
    " an up event inside, retract the menu.\n  document.addEventListener(\'mouseup\', (e" +
    ") => {\n    if (activeMenu == e.target.closest(\'.submenu\')) {\n      cancelActiveM" +
    "enu();\n    }\n  }, { passive: true, capture: true });\n}\n\nfunction viewer(baseUrl," +
    " nodes) {\n  \'use strict\';\n\n  // Elements\n  const search = document.getElementByI" +
    "d(\'search\');\n  const graph0 = document.getElementById(\'graph0\');\n  const svg = (" +
    "graph0 == null ? null : graph0.parentElement);\n  const toptable = document.getEl" +
    "ementById(\'toptable\');\n\n  let regexpActive = false;\n  let selected = new Map();\n" +
    "  let origFill = new Map();\n  let searchAlarm = null;\n  let buttonsEnabled = tru" +
    "e;\n\n  function handleDetails(e) {\n    e.preventDefault();\n    const detailsText " +
    "= document.getElementById(\'detailsbox\');\n    if (detailsText != null) {\n      if" +
    " (detailsText.style.display === \'block\') {\n        detailsText.style.display = \'" +
    "none\';\n      } else {\n        detailsText.style.display = \'block\';\n      }\n    }" +
    "\n  }\n\n  function handleKey(e) {\n    if (e.keyCode != 13) return;\n    setHrefPara" +
    "ms(window.location, function (params) {\n      params.set(\'f\', search.value);\n   " +
    " });\n    e.preventDefault();\n  }\n\n  function handleSearch() {\n    // Delay expen" +
    "sive processing so a flurry of key strokes is handled once.\n    if (searchAlarm " +
    "!= null) {\n      clearTimeout(searchAlarm);\n    }\n    searchAlarm = setTimeout(s" +
    "electMatching, 300);\n\n    regexpActive = true;\n    updateButtons();\n  }\n\n  funct" +
    "ion selectMatching() {\n    searchAlarm = null;\n    let re = null;\n    if (search" +
    ".value != \'\') {\n      try {\n        re = new RegExp(search.value);\n      } catch" +
    " (e) {\n        // TODO: Display error state in search box\n        return;\n      " +
    "}\n    }\n\n    function match(text) {\n      return re != null && re.test(text);\n  " +
    "  }\n\n    // drop currently selected items that do not match re.\n    selected.for" +
    "Each(function(v, n) {\n      if (!match(nodes[n])) {\n        unselect(n, document" +
    ".getElementById(\'node\' + n));\n      }\n    })\n\n    // add matching items that are" +
    " not currently selected.\n    if (nodes) {\n      for (let n = 0; n < nodes.length" +
    "; n++) {\n        if (!selected.has(n) && match(nodes[n])) {\n          select(n, " +
    "document.getElementById(\'node\' + n));\n        }\n      }\n    }\n\n    updateButtons" +
    "();\n  }\n\n  function toggleSvgSelect(elem) {\n    // Walk up to immediate child of" +
    " graph0\n    while (elem != null && elem.parentElement != graph0) {\n      elem = " +
    "elem.parentElement;\n    }\n    if (!elem) return;\n\n    // Disable regexp mode.\n  " +
    "  regexpActive = false;\n\n    const n = nodeId(elem);\n    if (n < 0) return;\n    " +
    "if (selected.has(n)) {\n      unselect(n, elem);\n    } else {\n      select(n, ele" +
    "m);\n    }\n    updateButtons();\n  }\n\n  function unselect(n, elem) {\n    if (elem " +
    "== null) return;\n    selected.delete(n);\n    setBackground(elem, false);\n  }\n\n  " +
    "function select(n, elem) {\n    if (elem == null) return;\n    selected.set(n, tru" +
    "e);\n    setBackground(elem, true);\n  }\n\n  function nodeId(elem) {\n    const id =" +
    " elem.id;\n    if (!id) return -1;\n    if (!id.startsWith(\'node\')) return -1;\n   " +
    " const n = parseInt(id.slice(4), 10);\n    if (isNaN(n)) return -1;\n    if (n < 0" +
    " || n >= nodes.length) return -1;\n    return n;\n  }\n\n  function setBackground(el" +
    "em, set) {\n    // Handle table row highlighting.\n    if (elem.nodeName == \'TR\') " +
    "{\n      elem.classList.toggle(\'hilite\', set);\n      return;\n    }\n\n    // Handle" +
    " svg element highlighting.\n    const p = findPolygon(elem);\n    if (p != null) {" +
    "\n      if (set) {\n        origFill.set(p, p.style.fill);\n        p.style.fill = " +
    "\'#ccccff\';\n      } else if (origFill.has(p)) {\n        p.style.fill = origFill.g" +
    "et(p);\n      }\n    }\n  }\n\n  function findPolygon(elem) {\n    if (elem.localName " +
    "== \'polygon\') return elem;\n    for (const c of elem.children) {\n      const p = " +
    "findPolygon(c);\n      if (p != null) return p;\n    }\n    return null;\n  }\n\n  // " +
    "convert a string to a regexp that matches that string.\n  function quotemeta(str)" +
    " {\n    return str.replace(/([\\\\\\.?+*\\[\\](){}|^$])/g, \'\\\\$1\');\n  }\n\n  function se" +
    "tSampleIndexLink(id) {\n    const elem = document.getElementById(id);\n    if (ele" +
    "m != null) {\n      setHrefParams(elem, function (params) {\n        params.set(\"s" +
    "i\", id);\n      });\n    }\n  }\n\n  // Update id\'s href to reflect current selection" +
    " whenever it is\n  // liable to be followed.\n  function makeSearchLinkDynamic(id)" +
    " {\n    const elem = document.getElementById(id);\n    if (elem == null) return;\n\n" +
    "    // Most links copy current selection into the \'f\' parameter,\n    // but Refi" +
    "ne menu links are different.\n    let param = \'f\';\n    if (id == \'ignore\') param " +
    "= \'i\';\n    if (id == \'hide\') param = \'h\';\n    if (id == \'show\') param = \'s\';\n   " +
    " if (id == \'show-from\') param = \'sf\';\n\n    // We update on mouseenter so middle-" +
    "click/right-click work properly.\n    elem.addEventListener(\'mouseenter\', updater" +
    ");\n    elem.addEventListener(\'touchstart\', updater);\n\n    function updater() {\n " +
    "     // The selection can be in one of two modes: regexp-based or\n      // list-" +
    "based.  Construct regular expression depending on mode.\n      let re = regexpAct" +
    "ive\n        ? search.value\n        : Array.from(selected.keys()).map(key => quot" +
    "emeta(nodes[key])).join(\'|\');\n\n      setHrefParams(elem, function (params) {\n   " +
    "     if (re != \'\') {\n          // For focus/show/show-from, forget old parameter" +
    ". For others, add to re.\n          if (param != \'f\' && param != \'s\' && param != " +
    "\'sf\' && params.has(param)) {\n            const old = params.get(param);\n        " +
    "    if (old != \'\') {\n              re += \'|\' + old;\n            }\n          }\n  " +
    "        params.set(param, re);\n        } else {\n          params.delete(param);\n" +
    "        }\n      });\n    }\n  }\n\n  function setHrefParams(elem, paramSetter) {\n   " +
    " let url = new URL(elem.href);\n    url.hash = \'\';\n\n    // Copy params from this " +
    "page\'s URL.\n    const params = url.searchParams;\n    for (const p of new URLSear" +
    "chParams(window.location.search)) {\n      params.set(p[0], p[1]);\n    }\n\n    // " +
    "Give the params to the setter to modify.\n    paramSetter(params);\n\n    elem.href" +
    " = url.toString();\n  }\n\n  function handleTopClick(e) {\n    // Walk back until we" +
    " find TR and then get the Name column (index 5)\n    let elem = e.target;\n    whi" +
    "le (elem != null && elem.nodeName != \'TR\') {\n      elem = elem.parentElement;\n  " +
    "  }\n    if (elem == null || elem.children.length < 6) return;\n\n    e.preventDefa" +
    "ult();\n    const tr = elem;\n    const td = elem.children[5];\n    if (td.nodeName" +
    " != \'TD\') return;\n    const name = td.innerText;\n    const index = nodes.indexOf" +
    "(name);\n    if (index < 0) return;\n\n    // Disable regexp mode.\n    regexpActive" +
    " = false;\n\n    if (selected.has(index)) {\n      unselect(index, elem);\n    } els" +
    "e {\n      select(index, elem);\n    }\n    updateButtons();\n  }\n\n  function update" +
    "Buttons() {\n    const enable = (search.value != \'\' || selected.size != 0);\n    i" +
    "f (buttonsEnabled == enable) return;\n    buttonsEnabled = enable;\n    for (const" +
    " id of [\'focus\', \'ignore\', \'hide\', \'show\', \'show-from\']) {\n      const link = do" +
    "cument.getElementById(id);\n      if (link != null) {\n        link.classList.togg" +
    "le(\'disabled\', !enable);\n      }\n    }\n  }\n\n  // Initialize button states\n  upda" +
    "teButtons();\n\n  // Setup event handlers\n  initMenus();\n  if (svg != null) {\n    " +
    "initPanAndZoom(svg, toggleSvgSelect);\n  }\n  if (toptable != null) {\n    toptable" +
    ".addEventListener(\'mousedown\', handleTopClick);\n    toptable.addEventListener(\'t" +
    "ouchstart\', handleTopClick);\n  }\n\n  const ids = [\'topbtn\', \'graphbtn\', \'flamegra" +
    "ph\', \'peek\', \'list\', \'disasm\',\n               \'focus\', \'ignore\', \'hide\', \'show\'," +
    " \'show-from\'];\n  ids.forEach(makeSearchLinkDynamic);\n\n  const sampleIDs = [{{ran" +
    "ge .SampleTypes}}\'{{.}}\', {{end}}];\n  sampleIDs.forEach(setSampleIndexLink);\n\n  " +
    "// Bind action to button with specified id.\n  function addAction(id, action) {\n " +
    "   const btn = document.getElementById(id);\n    if (btn != null) {\n      btn.add" +
    "EventListener(\'click\', action);\n      btn.addEventListener(\'touchstart\', action)" +
    ";\n    }\n  }\n\n  addAction(\'details\', handleDetails);\n\n  search.addEventListener(\'" +
    "input\', handleSearch);\n  search.addEventListener(\'keydown\', handleKey);\n\n  // Gi" +
    "ve initial focus to main container so it can be scrolled using keys.\n  const mai" +
    "n = document.getElementById(\'bodycontainer\');\n  if (main) {\n    main.focus();\n  " +
    "}\n}\n</script>\n{{end}}\n\n{{define \"top\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta " +
    "charset=\"utf-8\">\n  <title>{{.Title}}</title>\n  {{template \"css\" .}}\n  <style typ" +
    "e=\"text/css\">\n  </style>\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"top" +
    "\">\n    <table id=\"toptable\">\n      <thead>\n        <tr>\n          <th id=\"flathd" +
    "r1\">Flat</th>\n          <th id=\"flathdr2\">Flat%</th>\n          <th>Sum%</th>\n   " +
    "       <th id=\"cumhdr1\">Cum</th>\n          <th id=\"cumhdr2\">Cum%</th>\n          " +
    "<th id=\"namehdr\">Name</th>\n          <th>Inlined?</th>\n        </tr>\n      </the" +
    "ad>\n      <tbody id=\"rows\"></tbody>\n    </table>\n  </div>\n  {{template \"script\" " +
    ".}}\n  <script>\n    function makeTopTable(total, entries) {\n      const rows = do" +
    "cument.getElementById(\'rows\');\n      if (rows == null) return;\n\n      // Store i" +
    "nitial index in each entry so we have stable node ids for selection.\n      for (" +
    "let i = 0; i < entries.length; i++) {\n        entries[i].Id = \'node\' + i;\n      " +
    "}\n\n      // Which column are we currently sorted by and in what order?\n      let" +
    " currentColumn = \'\';\n      let descending = false;\n      sortBy(\'Flat\');\n\n      " +
    "function sortBy(column) {\n        // Update sort criteria\n        if (column == " +
    "currentColumn) {\n          descending = !descending; // Reverse order\n        } " +
    "else {\n          currentColumn = column;\n          descending = (column != \'Name" +
    "\');\n        }\n\n        // Sort according to current criteria.\n        function c" +
    "mp(a, b) {\n          const av = a[currentColumn];\n          const bv = b[current" +
    "Column];\n          if (av < bv) return -1;\n          if (av > bv) return +1;\n   " +
    "       return 0;\n        }\n        entries.sort(cmp);\n        if (descending) en" +
    "tries.reverse();\n\n        function addCell(tr, val) {\n          const td = docum" +
    "ent.createElement(\'td\');\n          td.textContent = val;\n          tr.appendChil" +
    "d(td);\n        }\n\n        function percent(v) {\n          return (v * 100.0 / to" +
    "tal).toFixed(2) + \'%\';\n        }\n\n        // Generate rows\n        const fragmen" +
    "t = document.createDocumentFragment();\n        let sum = 0;\n        for (const r" +
    "ow of entries) {\n          const tr = document.createElement(\'tr\');\n          tr" +
    ".id = row.Id;\n          sum += row.Flat;\n          addCell(tr, row.FlatFormat);\n" +
    "          addCell(tr, percent(row.Flat));\n          addCell(tr, percent(sum));\n " +
    "         addCell(tr, row.CumFormat);\n          addCell(tr, percent(row.Cum));\n  " +
    "        addCell(tr, row.Name);\n          addCell(tr, row.InlineLabel);\n         " +
    " fragment.appendChild(tr);\n        }\n\n        rows.textContent = \'\'; // Remove o" +
    "ld rows\n        rows.appendChild(fragment);\n      }\n\n      // Make different col" +
    "umn headers trigger sorting.\n      function bindSort(id, column) {\n        const" +
    " hdr = document.getElementById(id);\n        if (hdr == null) return;\n        con" +
    "st fn = function() { sortBy(column) };\n        hdr.addEventListener(\'click\', fn)" +
    ";\n        hdr.addEventListener(\'touch\', fn);\n      }\n      bindSort(\'flathdr1\', " +
    "\'Flat\');\n      bindSort(\'flathdr2\', \'Flat\');\n      bindSort(\'cumhdr1\', \'Cum\');\n " +
    "     bindSort(\'cumhdr2\', \'Cum\');\n      bindSort(\'namehdr\', \'Name\');\n    }\n\n    v" +
    "iewer(new URL(window.location.href), {{.Nodes}});\n    makeTopTable({{.Total}}, {" +
    "{.Top}});\n  </script>\n</body>\n</html>\n{{end}}\n\n{{define \"sourcelisting\" -}}\n<!DO" +
    "CTYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  <title>{{.Title}}</title>\n " +
    " {{template \"css\" .}}\n  {{template \"weblistcss\" .}}\n  {{template \"weblistjs\" .}}" +
    "\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"content\" class=\"source\">\n  " +
    "  {{.HTMLBody}}\n  </div>\n  {{template \"script\" .}}\n  <script>viewer(new URL(wind" +
    "ow.location.href), null);</script>\n</body>\n</html>\n{{end}}\n\n{{define \"plaintext\"" +
    " -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <meta charset=\"utf-8\">\n  <title>{{.Title}}<" +
    "/title>\n  {{template \"css\" .}}\n</head>\n<body>\n  {{template \"header\" .}}\n  <div i" +
    "d=\"content\">\n    <pre>\n      {{.TextBody}}\n    </pre>\n  </div>\n  {{template \"scr" +
    "ipt\" .}}\n  <script>viewer(new URL(window.location.href), null);</script>\n</body>" +
    "\n</html>\n{{end}}\n\n{{define \"flamegraph\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n  <met" +
    "a charset=\"utf-8\">\n  <title>{{.Title}}</title>\n  {{template \"css\" .}}\n  <style t" +
    "ype=\"text/css\">{{template \"d3flamegraphcss\" .}}</style>\n  <style type=\"text/css\"" +
    ">\n    .flamegraph-content {\n      width: 90%;\n      min-width: 80%;\n      margin" +
    "-left: 5%;\n    }\n    .flamegraph-details {\n      height: 1.2em;\n      width: 90%" +
    ";\n      min-width: 90%;\n      margin-left: 5%;\n      padding: 15px 0 35px;\n    }" +
    "\n  </style>\n</head>\n<body>\n  {{template \"header\" .}}\n  <div id=\"bodycontainer\">\n" +
    "    <div id=\"flamegraphdetails\" class=\"flamegraph-details\"></div>\n    <div class" +
    "=\"flamegraph-content\">\n      <div id=\"chart\"></div>\n    </div>\n  </div>\n  {{temp" +
    "late \"script\" .}}\n  <script>viewer(new URL(window.location.href), {{.Nodes}});</" +
    "script>\n  <script>{{template \"d3script\" .}}</script>\n  <script>{{template \"d3fla" +
    "megraphscript\" .}}</script>\n  <script>\n    var data = {{.FlameGraph}};\n\n    var " +
    "width = document.getElementById(\'chart\').clientWidth;\n\n    var flameGraph = d3.f" +
    "lamegraph()\n      .width(width)\n      .cellHeight(18)\n      .minFrameSize(1)\n   " +
    "   .transitionDuration(750)\n      .transitionEase(d3.easeCubic)\n      .inverted(" +
    "true)\n      .sort(true)\n      .title(\'\')\n      .tooltip(false)\n      .details(do" +
    "cument.getElementById(\'flamegraphdetails\'));\n\n    // <full name> (percentage, va" +
    "lue)\n    flameGraph.label((d) => d.data.f + \' (\' + d.data.p + \', \' + d.data.l + " +
    "\')\');\n\n    (function(flameGraph) {\n      var oldColorMapper = flameGraph.color()" +
    ";\n      function colorMapper(d) {\n        // Hack to force default color mapper " +
    "to use \'warm\' color scheme by not passing libtype\n        const { data, highligh" +
    "t } = d;\n        return oldColorMapper({ data: { n: data.n }, highlight });\n    " +
    "  }\n\n      flameGraph.color(colorMapper);\n    }(flameGraph));\n\n    d3.select(\'#c" +
    "hart\')\n      .datum(data)\n      .call(flameGraph);\n\n    function clear() {\n     " +
    " flameGraph.clear();\n    }\n\n    function resetZoom() {\n      flameGraph.resetZoo" +
    "m();\n    }\n\n    window.addEventListener(\'resize\', function() {\n      var width =" +
    " document.getElementById(\'chart\').clientWidth;\n      var graphs = document.getEl" +
    "ementsByClassName(\'d3-flame-graph\');\n      if (graphs.length > 0) {\n        grap" +
    "hs[0].setAttribute(\'width\', width);\n      }\n      flameGraph.width(width);\n     " +
    " flameGraph.resetZoom();\n    }, true);\n\n    var search = document.getElementById" +
    "(\'search\');\n    var searchAlarm = null;\n\n    function selectMatching() {\n      s" +
    "earchAlarm = null;\n\n      if (search.value != \'\') {\n        flameGraph.search(se" +
    "arch.value);\n      } else {\n        flameGraph.clear();\n      }\n    }\n\n    funct" +
    "ion handleSearch() {\n      // Delay expensive processing so a flurry of key stro" +
    "kes is handled once.\n      if (searchAlarm != null) {\n        clearTimeout(searc" +
    "hAlarm);\n      }\n      searchAlarm = setTimeout(selectMatching, 300);\n    }\n\n   " +
    " search.addEventListener(\'input\', handleSearch);\n  </script>\n</body>\n</html>\n{{e" +
    "nd}}\n"));

        }
    }
}}}}}}}
