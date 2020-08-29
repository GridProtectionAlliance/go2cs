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

// package driver -- go2cs converted at 2020 August 29 10:05:28 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\webhtml.go
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
    public static partial class driver_package
    {
        // addTemplates adds a set of template definitions to templates.
        private static void addTemplates(ref template.Template templates)
        {
            template.Must(templates.Parse("\n{{define \"css\"}}\n<style type=\"text/css\">\nhtml {\n  height: 100%;\n  min-height: 10" +
    "0%;\n  margin: 0px;\n}\nbody {\n  margin: 0px;\n  width: 100%;\n  height: 100%;\n  min-" +
    "height: 100%;\n  overflow: hidden;\n}\n#graphcontainer {\n  display: flex;\n  flex-di" +
    "rection: column;\n  height: 100%;\n  min-height: 100%;\n  width: 100%;\n  min-width:" +
    " 100%;\n  margin: 0px;\n}\n#graph {\n  flex: 1 1 auto;\n  overflow: hidden;\n}\nsvg {\n " +
    " width: 100%;\n  height: auto;\n}\nbutton {\n  margin-top: 5px;\n  margin-bottom: 5px" +
    ";\n}\n#detailtext {\n  display: none;\n  position: fixed;\n  top: 20px;\n  right: 10px" +
    ";\n  background-color: #ffffff;\n  min-width: 160px;\n  border: 1px solid #888;\n  b" +
    "ox-shadow: 4px 4px 4px 0px rgba(0,0,0,0.2);\n  z-index: 1;\n}\n#closedetails {\n  fl" +
    "oat: right;\n  margin: 2px;\n}\n#home {\n  font-size: 14pt;\n  padding-left: 0.5em;\n " +
    " padding-right: 0.5em;\n  float: right;\n}\n.menubar {\n  display: inline-block;\n  b" +
    "ackground-color: #f8f8f8;\n  border: 1px solid #ccc;\n  width: 100%;\n}\n.menu-heade" +
    "r {\n  position: relative;\n  display: inline-block;\n  padding: 2px 2px;\n  font-si" +
    "ze: 14pt;\n}\n.menu {\n  display: none;\n  position: absolute;\n  background-color: #" +
    "f8f8f8;\n  border: 1px solid #888;\n  box-shadow: 4px 4px 4px 0px rgba(0,0,0,0.2);" +
    "\n  z-index: 1;\n  margin-top: 2px;\n  left: 0px;\n  min-width: 5em;\n}\n.menu-header," +
    " .menu {\n  cursor: default;\n  user-select: none;\n  -moz-user-select: none;\n  -ms" +
    "-user-select: none;\n  -webkit-user-select: none;\n}\n.menu hr {\n  background-color" +
    ": #fff;\n  margin-top: 0px;\n  margin-bottom: 0px;\n}\n.menu a, .menu button {\n  dis" +
    "play: block;\n  width: 100%;\n  margin: 0px;\n  padding: 2px 0px 2px 0px;\n  text-al" +
    "ign: left;\n  text-decoration: none;\n  color: #000;\n  background-color: #f8f8f8;\n" +
    "  font-size: 12pt;\n  border: none;\n}\n.menu-header:hover {\n  background-color: #c" +
    "cc;\n}\n.menu a:hover, .menu button:hover {\n  background-color: #ccc;\n}\n.menu a.di" +
    "sabled {\n  color: gray;\n  pointer-events: none;\n}\n#searchbox {\n  margin-left: 10" +
    "pt;\n}\n#bodycontainer {\n  width: 100%;\n  height: 100%;\n  max-height: 100%;\n  over" +
    "flow: scroll;\n  padding-top: 5px;\n}\n#toptable {\n  border-spacing: 0px;\n  width: " +
    "100%;\n  padding-bottom: 1em;\n}\n#toptable tr th {\n  border-bottom: 1px solid blac" +
    "k;\n  text-align: right;\n  padding-left: 1em;\n  padding-top: 0.2em;\n  padding-bot" +
    "tom: 0.2em;\n}\n#toptable tr td {\n  padding-left: 1em;\n  font: monospace;\n  text-a" +
    "lign: right;\n  white-space: nowrap;\n  cursor: default;\n}\n#toptable tr th:nth-chi" +
    "ld(6),\n#toptable tr th:nth-child(7),\n#toptable tr td:nth-child(6),\n#toptable tr " +
    "td:nth-child(7) {\n  text-align: left;\n}\n#toptable tr td:nth-child(6) {\n  max-wid" +
    "th: 30em;  // Truncate very long names\n  overflow: hidden;\n}\n#flathdr1, #flathdr" +
    "2, #cumhdr1, #cumhdr2, #namehdr {\n  cursor: ns-resize;\n}\n.hilite {\n  background-" +
    "color: #ccf;\n}\n</style>\n{{end}}\n\n{{define \"header\"}}\n<div id=\"detailtext\">\n<butt" +
    "on id=\"closedetails\">Close</button>\n{{range .Legend}}<div>{{.}}</div>{{end}}\n</d" +
    "iv>\n\n<div class=\"menubar\">\n\n<div class=\"menu-header\">\nView\n<div class=\"menu\">\n<a" +
    " title=\"{{.Help.top}}\"  href=\"/top\" id=\"topbtn\">Top</a>\n<a title=\"{{.Help.graph}" +
    "}\" href=\"/\" id=\"graphbtn\">Graph</a>\n<a title=\"{{.Help.peek}}\" href=\"/peek\" id=\"p" +
    "eek\">Peek</a>\n<a title=\"{{.Help.list}}\" href=\"/source\" id=\"list\">Source</a>\n<a t" +
    "itle=\"{{.Help.disasm}}\" href=\"/disasm\" id=\"disasm\">Disassemble</a>\n<hr>\n<button " +
    "title=\"{{.Help.details}}\" id=\"details\">Details</button>\n</div>\n</div>\n\n<div clas" +
    "s=\"menu-header\">\nRefine\n<div class=\"menu\">\n<a title=\"{{.Help.focus}}\" href=\"{{.B" +
    "aseURL}}\" id=\"focus\">Focus</a>\n<a title=\"{{.Help.ignore}}\" href=\"{{.BaseURL}}\" i" +
    "d=\"ignore\">Ignore</a>\n<a title=\"{{.Help.hide}}\" href=\"{{.BaseURL}}\" id=\"hide\">Hi" +
    "de</a>\n<a title=\"{{.Help.show}}\" href=\"{{.BaseURL}}\" id=\"show\">Show</a>\n<hr>\n<a " +
    "title=\"{{.Help.reset}}\" href=\"{{.BaseURL}}\">Reset</a>\n</div>\n</div>\n\n<input id=\"" +
    "searchbox\" type=\"text\" placeholder=\"Search regexp\" autocomplete=\"off\" autocapita" +
    "lize=\"none\" size=40>\n\n<span id=\"home\">{{.Title}}</span>\n\n</div> <!-- menubar -->" +
    "\n\n<div id=\"errors\">{{range .Errors}}<div>{{.}}</div>{{end}}</div>\n{{end}}\n\n{{def" +
    "ine \"graph\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"utf-8\">\n<title>{{.T" +
    "itle}}</title>\n{{template \"css\" .}}\n</head>\n<body>\n\n{{template \"header\" .}}\n<div" +
    " id=\"graphcontainer\">\n<div id=\"graph\">\n{{.HTMLBody}}\n</div>\n\n</div>\n{{template \"" +
    "script\" .}}\n<script>viewer({{.BaseURL}}, {{.Nodes}})</script>\n</body>\n</html>\n{{" +
    "end}}\n\n{{define \"script\"}}\n<script>\n// Make svg pannable and zoomable.\n// Call c" +
    "lickHandler(t) if a click event is caught by the pan event handlers.\nfunction in" +
    "itPanAndZoom(svg, clickHandler) {\n  \'use strict\';\n\n  // Current mouse/touch hand" +
    "ling mode\n  const IDLE = 0\n  const MOUSEPAN = 1\n  const TOUCHPAN = 2\n  const TOU" +
    "CHZOOM = 3\n  let mode = IDLE\n\n  // State needed to implement zooming.\n  let curr" +
    "entScale = 1.0\n  const initWidth = svg.viewBox.baseVal.width\n  const initHeight " +
    "= svg.viewBox.baseVal.height\n\n  // State needed to implement panning.\n  let panL" +
    "astX = 0      // Last event X coordinate\n  let panLastY = 0      // Last event Y" +
    " coordinate\n  let moved = false     // Have we seen significant movement\n  let t" +
    "ouchid = null    // Current touch identifier\n\n  // State needed for pinch zoomin" +
    "g\n  let touchid2 = null     // Second id for pinch zooming\n  let initGap = 1.0  " +
    "     // Starting gap between two touches\n  let initScale = 1.0     // currentSca" +
    "le when pinch zoom started\n  let centerPoint = null  // Center point for scaling" +
    "\n\n  // Convert event coordinates to svg coordinates.\n  function toSvg(x, y) {\n  " +
    "  const p = svg.createSVGPoint()\n    p.x = x\n    p.y = y\n    let m = svg.getCTM(" +
    ")\n    if (m == null) m = svg.getScreenCTM()  // Firefox workaround.\n    return p" +
    ".matrixTransform(m.inverse())\n  }\n\n  // Change the scaling for the svg to s, kee" +
    "ping the point denoted\n  // by u (in svg coordinates]) fixed at the same screen " +
    "location.\n  function rescale(s, u) {\n    // Limit to a good range.\n    if (s < 0" +
    ".2) s = 0.2\n    if (s > 10.0) s = 10.0\n\n    currentScale = s\n\n    // svg.viewBox" +
    " defines the visible portion of the user coordinate\n    // system.  So to magnif" +
    "y by s, divide the visible portion by s,\n    // which will then be stretched to " +
    "fit the viewport.\n    const vb = svg.viewBox\n    const w1 = vb.baseVal.width\n   " +
    " const w2 = initWidth / s\n    const h1 = vb.baseVal.height\n    const h2 = initHe" +
    "ight / s\n    vb.baseVal.width = w2\n    vb.baseVal.height = h2\n\n    // We also wa" +
    "nt to adjust vb.baseVal.x so that u.x remains at same\n    // screen X coordinate" +
    ".  In other words, want to change it from x1 to x2\n    // so that:\n    //     (u" +
    ".x - x1) / w1 = (u.x - x2) / w2\n    // Simplifying that, we get\n    //     (u.x " +
    "- x1) * (w2 / w1) = u.x - x2\n    //     x2 = u.x - (u.x - x1) * (w2 / w1)\n    vb" +
    ".baseVal.x = u.x - (u.x - vb.baseVal.x) * (w2 / w1)\n    vb.baseVal.y = u.y - (u." +
    "y - vb.baseVal.y) * (h2 / h1)\n  }\n\n  function handleWheel(e) {\n    if (e.deltaY " +
    "== 0) return\n    // Change scale factor by 1.1 or 1/1.1\n    rescale(currentScale" +
    " * (e.deltaY < 0 ? 1.1 : (1/1.1)),\n            toSvg(e.offsetX, e.offsetY))\n  }\n" +
    "\n  function setMode(m) {\n    mode = m\n    touchid = null\n    touchid2 = null\n  }" +
    "\n\n  function panStart(x, y) {\n    moved = false\n    panLastX = x\n    panLastY = " +
    "y\n  }\n\n  function panMove(x, y) {\n    let dx = x - panLastX\n    let dy = y - pan" +
    "LastY\n    if (Math.abs(dx) <= 2 && Math.abs(dy) <= 2) return  // Ignore tiny mov" +
    "es\n\n    moved = true\n    panLastX = x\n    panLastY = y\n\n    // Firefox workaroun" +
    "d: get dimensions from parentNode.\n    const swidth = svg.clientWidth || svg.par" +
    "entNode.clientWidth\n    const sheight = svg.clientHeight || svg.parentNode.clien" +
    "tHeight\n\n    // Convert deltas from screen space to svg space.\n    dx *= (svg.vi" +
    "ewBox.baseVal.width / swidth)\n    dy *= (svg.viewBox.baseVal.height / sheight)\n\n" +
    "    svg.viewBox.baseVal.x -= dx\n    svg.viewBox.baseVal.y -= dy\n  }\n\n  function " +
    "handleScanStart(e) {\n    if (e.button != 0) return  // Do not catch right-clicks" +
    " etc.\n    setMode(MOUSEPAN)\n    panStart(e.clientX, e.clientY)\n    e.preventDefa" +
    "ult()\n    svg.addEventListener(\"mousemove\", handleScanMove)\n  }\n\n  function hand" +
    "leScanMove(e) {\n    if (e.buttons == 0) {\n      // Missed an end event, perhaps " +
    "because mouse moved outside window.\n      setMode(IDLE)\n      svg.removeEventLis" +
    "tener(\"mousemove\", handleScanMove)\n      return\n    }\n    if (mode == MOUSEPAN) " +
    "panMove(e.clientX, e.clientY)\n  }\n\n  function handleScanEnd(e) {\n    if (mode ==" +
    " MOUSEPAN) panMove(e.clientX, e.clientY)\n    setMode(IDLE)\n    svg.removeEventLi" +
    "stener(\"mousemove\", handleScanMove)\n    if (!moved) clickHandler(e.target)\n  }\n\n" +
    "  // Find touch object with specified identifier.\n  function findTouch(tlist, id" +
    ") {\n    for (const t of tlist) {\n      if (t.identifier == id) return t\n    }\n  " +
    "  return null\n  }\n\n // Return distance between two touch points\n  function touch" +
    "Gap(t1, t2) {\n    const dx = t1.clientX - t2.clientX\n    const dy = t1.clientY -" +
    " t2.clientY\n    return Math.hypot(dx, dy)\n  }\n\n  function handleTouchStart(e) {\n" +
    "    if (mode == IDLE && e.changedTouches.length == 1) {\n      // Start touch bas" +
    "ed panning\n      const t = e.changedTouches[0]\n      setMode(TOUCHPAN)\n      tou" +
    "chid = t.identifier\n      panStart(t.clientX, t.clientY)\n      e.preventDefault(" +
    ")\n    } else if (mode == TOUCHPAN && e.touches.length == 2) {\n      // Start pin" +
    "ch zooming\n      setMode(TOUCHZOOM)\n      const t1 = e.touches[0]\n      const t2" +
    " = e.touches[1]\n      touchid = t1.identifier\n      touchid2 = t2.identifier\n   " +
    "   initScale = currentScale\n      initGap = touchGap(t1, t2)\n      centerPoint =" +
    " toSvg((t1.clientX + t2.clientX) / 2,\n                          (t1.clientY + t2" +
    ".clientY) / 2)\n      e.preventDefault()\n    }\n  }\n\n  function handleTouchMove(e)" +
    " {\n    if (mode == TOUCHPAN) {\n      const t = findTouch(e.changedTouches, touch" +
    "id)\n      if (t == null) return\n      if (e.touches.length != 1) {\n        setMo" +
    "de(IDLE)\n        return\n      }\n      panMove(t.clientX, t.clientY)\n      e.prev" +
    "entDefault()\n    } else if (mode == TOUCHZOOM) {\n      // Get two touches; new g" +
    "ap; rescale to ratio.\n      const t1 = findTouch(e.touches, touchid)\n      const" +
    " t2 = findTouch(e.touches, touchid2)\n      if (t1 == null || t2 == null) return\n" +
    "      const gap = touchGap(t1, t2)\n      rescale(initScale * gap / initGap, cent" +
    "erPoint)\n      e.preventDefault()\n    }\n  }\n\n  function handleTouchEnd(e) {\n    " +
    "if (mode == TOUCHPAN) {\n      const t = findTouch(e.changedTouches, touchid)\n   " +
    "   if (t == null) return\n      panMove(t.clientX, t.clientY)\n      setMode(IDLE)" +
    "\n      e.preventDefault()\n      if (!moved) clickHandler(t.target)\n    } else if" +
    " (mode == TOUCHZOOM) {\n      setMode(IDLE)\n      e.preventDefault()\n    }\n  }\n\n " +
    " svg.addEventListener(\"mousedown\", handleScanStart)\n  svg.addEventListener(\"mous" +
    "eup\", handleScanEnd)\n  svg.addEventListener(\"touchstart\", handleTouchStart)\n  sv" +
    "g.addEventListener(\"touchmove\", handleTouchMove)\n  svg.addEventListener(\"touchen" +
    "d\", handleTouchEnd)\n  svg.addEventListener(\"wheel\", handleWheel, true)\n}\n\nfuncti" +
    "on initMenus() {\n  \'use strict\';\n\n  let activeMenu = null;\n  let activeMenuHdr =" +
    " null;\n\n  function cancelActiveMenu() {\n    if (activeMenu == null) return;\n    " +
    "activeMenu.style.display = \"none\";\n    activeMenu = null;\n    activeMenuHdr = nu" +
    "ll;\n  }\n\n  // Set click handlers on every menu header.\n  for (const menu of docu" +
    "ment.getElementsByClassName(\"menu\")) {\n    const hdr = menu.parentElement;\n    i" +
    "f (hdr == null) return;\n    function showMenu(e) {\n      // menu is a child of h" +
    "dr, so this event can fire for clicks\n      // inside menu. Ignore such clicks.\n" +
    "      if (e.target != hdr) return;\n      activeMenu = menu;\n      activeMenuHdr " +
    "= hdr;\n      menu.style.display = \"block\";\n    }\n    hdr.addEventListener(\"mouse" +
    "down\", showMenu);\n    hdr.addEventListener(\"touchstart\", showMenu);\n  }\n\n  // If" +
    " there is an active menu and a down event outside, retract the menu.\n  for (cons" +
    "t t of [\"mousedown\", \"touchstart\"]) {\n    document.addEventListener(t, (e) => {\n" +
    "      // Note: to avoid unnecessary flicker, if the down event is inside\n      /" +
    "/ the active menu header, do not retract the menu.\n      if (activeMenuHdr != e." +
    "target.closest(\".menu-header\")) {\n        cancelActiveMenu();\n      }\n    }, { p" +
    "assive: true, capture: true });\n  }\n\n  // If there is an active menu and an up e" +
    "vent inside, retract the menu.\n  document.addEventListener(\"mouseup\", (e) => {\n " +
    "   if (activeMenu == e.target.closest(\".menu\")) {\n      cancelActiveMenu();\n    " +
    "}\n  }, { passive: true, capture: true });\n}\n\nfunction viewer(baseUrl, nodes) {\n " +
    " \'use strict\';\n\n  // Elements\n  const search = document.getElementById(\"searchbo" +
    "x\")\n  const graph0 = document.getElementById(\"graph0\")\n  const svg = (graph0 == " +
    "null ? null : graph0.parentElement)\n  const toptable = document.getElementById(\"" +
    "toptable\")\n\n  let regexpActive = false\n  let selected = new Map()\n  let origFill" +
    " = new Map()\n  let searchAlarm = null\n  let buttonsEnabled = true\n\n  function ha" +
    "ndleDetails() {\n    const detailsText = document.getElementById(\"detailtext\")\n  " +
    "  if (detailsText != null) detailsText.style.display = \"block\"\n  }\n\n  function h" +
    "andleCloseDetails() {\n    const detailsText = document.getElementById(\"detailtex" +
    "t\")\n    if (detailsText != null) detailsText.style.display = \"none\"\n  }\n\n  funct" +
    "ion handleKey(e) {\n    if (e.keyCode != 13) return\n    window.location.href =\n  " +
    "      updateUrl(new URL({{.BaseURL}}, window.location.href), \"f\")\n    e.preventD" +
    "efault()\n  }\n\n  function handleSearch() {\n    // Delay expensive processing so a" +
    " flurry of key strokes is handled once.\n    if (searchAlarm != null) {\n      cle" +
    "arTimeout(searchAlarm)\n    }\n    searchAlarm = setTimeout(selectMatching, 300)\n\n" +
    "    regexpActive = true\n    updateButtons()\n  }\n\n  function selectMatching() {\n " +
    "   searchAlarm = null\n    let re = null\n    if (search.value != \"\") {\n      try " +
    "{\n        re = new RegExp(search.value)\n      } catch (e) {\n        // TODO: Dis" +
    "play error state in search box\n        return\n      }\n    }\n\n    function match(" +
    "text) {\n      return re != null && re.test(text)\n    }\n\n    // drop currently se" +
    "lected items that do not match re.\n    selected.forEach(function(v, n) {\n      i" +
    "f (!match(nodes[n])) {\n        unselect(n, document.getElementById(\"node\" + n))\n" +
    "      }\n    })\n\n    // add matching items that are not currently selected.\n    f" +
    "or (let n = 0; n < nodes.length; n++) {\n      if (!selected.has(n) && match(node" +
    "s[n])) {\n        select(n, document.getElementById(\"node\" + n))\n      }\n    }\n\n " +
    "   updateButtons()\n  }\n\n  function toggleSvgSelect(elem) {\n    // Walk up to imm" +
    "ediate child of graph0\n    while (elem != null && elem.parentElement != graph0) " +
    "{\n      elem = elem.parentElement\n    }\n    if (!elem) return\n\n    // Disable re" +
    "gexp mode.\n    regexpActive = false\n\n    const n = nodeId(elem)\n    if (n < 0) r" +
    "eturn\n    if (selected.has(n)) {\n      unselect(n, elem)\n    } else {\n      sele" +
    "ct(n, elem)\n    }\n    updateButtons()\n  }\n\n  function unselect(n, elem) {\n    if" +
    " (elem == null) return\n    selected.delete(n)\n    setBackground(elem, false)\n  }" +
    "\n\n  function select(n, elem) {\n    if (elem == null) return\n    selected.set(n, " +
    "true)\n    setBackground(elem, true)\n  }\n\n  function nodeId(elem) {\n    const id " +
    "= elem.id\n    if (!id) return -1\n    if (!id.startsWith(\"node\")) return -1\n    c" +
    "onst n = parseInt(id.slice(4), 10)\n    if (isNaN(n)) return -1\n    if (n < 0 || " +
    "n >= nodes.length) return -1\n    return n\n  }\n\n  function setBackground(elem, se" +
    "t) {\n    // Handle table row highlighting.\n    if (elem.nodeName == \"TR\") {\n    " +
    "  elem.classList.toggle(\"hilite\", set)\n      return\n    }\n\n    // Handle svg ele" +
    "ment highlighting.\n    const p = findPolygon(elem)\n    if (p != null) {\n      if" +
    " (set) {\n        origFill.set(p, p.style.fill)\n        p.style.fill = \"#ccccff\"\n" +
    "      } else if (origFill.has(p)) {\n        p.style.fill = origFill.get(p)\n     " +
    " }\n    }\n  }\n\n  function findPolygon(elem) {\n    if (elem.localName == \"polygon\"" +
    ") return elem\n    for (const c of elem.children) {\n      const p = findPolygon(c" +
    ")\n      if (p != null) return p\n    }\n    return null\n  }\n\n  // convert a string" +
    " to a regexp that matches that string.\n  function quotemeta(str) {\n    return st" +
    "r.replace(/([\\\\\\.?+*\\[\\](){}|^$])/g, \'\\\\$1\')\n  }\n\n  // Update id\'s href to refle" +
    "ct current selection whenever it is\n  // liable to be followed.\n  function makeL" +
    "inkDynamic(id) {\n    const elem = document.getElementById(id)\n    if (elem == nu" +
    "ll) return\n\n    // Most links copy current selection into the \"f\" parameter,\n   " +
    " // but Refine menu links are different.\n    let param = \"f\"\n    if (id == \"igno" +
    "re\") param = \"i\"\n    if (id == \"hide\") param = \"h\"\n    if (id == \"show\") param =" +
    " \"s\"\n\n    // We update on mouseenter so middle-click/right-click work properly.\n" +
    "    elem.addEventListener(\"mouseenter\", updater)\n    elem.addEventListener(\"touc" +
    "hstart\", updater)\n\n    function updater() {\n      elem.href = updateUrl(new URL(" +
    "elem.href), param)\n    }\n  }\n\n  // Update URL to reflect current selection.\n  fu" +
    "nction updateUrl(url, param) {\n    url.hash = \"\"\n\n    // The selection can be in" +
    " one of two modes: regexp-based or\n    // list-based.  Construct regular express" +
    "ion depending on mode.\n    let re = regexpActive\n        ? search.value\n        " +
    ": Array.from(selected.keys()).map(key => quotemeta(nodes[key])).join(\"|\")\n\n    /" +
    "/ Copy params from this page\'s URL.\n    const params = url.searchParams\n    for " +
    "(const p of new URLSearchParams(window.location.search)) {\n      params.set(p[0]" +
    ", p[1])\n    }\n\n    if (re != \"\") {\n      // For focus/show, forget old parameter" +
    ".  For others, add to re.\n      if (param != \"f\" && param != \"s\" && params.has(p" +
    "aram)) {\n        const old = params.get(param)\n         if (old != \"\") {\n       " +
    "   re += \"|\" + old\n        }\n      }\n      params.set(param, re)\n    } else {\n  " +
    "    params.delete(param)\n    }\n\n    return url.toString()\n  }\n\n  function handle" +
    "TopClick(e) {\n    // Walk back until we find TR and then get the Name column (in" +
    "dex 5)\n    let elem = e.target\n    while (elem != null && elem.nodeName != \"TR\")" +
    " {\n      elem = elem.parentElement\n    }\n    if (elem == null || elem.children.l" +
    "ength < 6) return\n\n    e.preventDefault()\n    const tr = elem\n    const td = ele" +
    "m.children[5]\n    if (td.nodeName != \"TD\") return\n    const name = td.innerText\n" +
    "    const index = nodes.indexOf(name)\n    if (index < 0) return\n\n    // Disable " +
    "regexp mode.\n    regexpActive = false\n\n    if (selected.has(index)) {\n      unse" +
    "lect(index, elem)\n    } else {\n      select(index, elem)\n    }\n    updateButtons" +
    "()\n  }\n\n  function updateButtons() {\n    const enable = (search.value != \"\" || s" +
    "elected.size != 0)\n    if (buttonsEnabled == enable) return\n    buttonsEnabled =" +
    " enable\n    for (const id of [\"focus\", \"ignore\", \"hide\", \"show\"]) {\n      const " +
    "link = document.getElementById(id)\n      if (link != null) {\n        link.classL" +
    "ist.toggle(\"disabled\", !enable)\n      }\n    }\n  }\n\n  // Initialize button states" +
    "\n  updateButtons()\n\n  // Setup event handlers\n  initMenus()\n  if (svg != null) {" +
    "\n    initPanAndZoom(svg, toggleSvgSelect)\n  }\n  if (toptable != null) {\n    topt" +
    "able.addEventListener(\"mousedown\", handleTopClick)\n    toptable.addEventListener" +
    "(\"touchstart\", handleTopClick)\n  }\n\n  const ids = [\"topbtn\", \"graphbtn\", \"peek\"," +
    " \"list\", \"disasm\",\n               \"focus\", \"ignore\", \"hide\", \"show\"]\n  ids.forEa" +
    "ch(makeLinkDynamic)\n\n  // Bind action to button with specified id.\n  function ad" +
    "dAction(id, action) {\n    const btn = document.getElementById(id)\n    if (btn !=" +
    " null) {\n      btn.addEventListener(\"click\", action)\n      btn.addEventListener(" +
    "\"touchstart\", action)\n    }\n  }\n\n  addAction(\"details\", handleDetails)\n  addActi" +
    "on(\"closedetails\", handleCloseDetails)\n\n  search.addEventListener(\"input\", handl" +
    "eSearch)\n  search.addEventListener(\"keydown\", handleKey)\n\n  // Give initial focu" +
    "s to main container so it can be scrolled using keys.\n  const main = document.ge" +
    "tElementById(\"bodycontainer\")\n  if (main) {\n    main.focus()\n  }\n}\n</script>\n{{e" +
    "nd}}\n\n{{define \"top\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"utf-8\">\n<t" +
    "itle>{{.Title}}</title>\n{{template \"css\" .}}\n<style type=\"text/css\">\n</style>\n</" +
    "head>\n<body>\n\n{{template \"header\" .}}\n\n<div id=\"bodycontainer\">\n<table id=\"topta" +
    "ble\">\n<tr>\n<th id=\"flathdr1\">Flat\n<th id=\"flathdr2\">Flat%\n<th>Sum%\n<th id=\"cumhd" +
    "r1\">Cum\n<th id=\"cumhdr2\">Cum%\n<th id=\"namehdr\">Name\n<th>Inlined?</tr>\n<tbody id=" +
    "\"rows\">\n</tbody>\n</table>\n</div>\n\n{{template \"script\" .}}\n<script>\nfunction make" +
    "TopTable(total, entries) {\n  const rows = document.getElementById(\"rows\")\n  if (" +
    "rows == null) return\n\n  // Store initial index in each entry so we have stable n" +
    "ode ids for selection.\n  for (let i = 0; i < entries.length; i++) {\n    entries[" +
    "i].Id = \"node\" + i\n  }\n\n  // Which column are we currently sorted by and in what" +
    " order?\n  let currentColumn = \"\"\n  let descending = false\n  sortBy(\"Flat\")\n\n  fu" +
    "nction sortBy(column) {\n    // Update sort criteria\n    if (column == currentCol" +
    "umn) {\n      descending = !descending  // Reverse order\n    } else {\n      curre" +
    "ntColumn = column\n      descending = (column != \"Name\")\n    }\n\n    // Sort accor" +
    "ding to current criteria.\n    function cmp(a, b) {\n      const av = a[currentCol" +
    "umn]\n      const bv = b[currentColumn]\n      if (av < bv) return -1\n      if (av" +
    " > bv) return +1\n      return 0\n    }\n    entries.sort(cmp)\n    if (descending) " +
    "entries.reverse()\n\n    function addCell(tr, val) {\n      const td = document.cre" +
    "ateElement(\'td\')\n      td.textContent = val\n      tr.appendChild(td)\n    }\n\n    " +
    "function percent(v) {\n      return (v * 100.0 / total).toFixed(2) + \"%\"\n    }\n\n " +
    "   // Generate rows\n    const fragment = document.createDocumentFragment()\n    l" +
    "et sum = 0\n    for (const row of entries) {\n      const tr = document.createElem" +
    "ent(\'tr\')\n      tr.id = row.Id\n      sum += row.Flat\n      addCell(tr, row.FlatF" +
    "ormat)\n      addCell(tr, percent(row.Flat))\n      addCell(tr, percent(sum))\n    " +
    "  addCell(tr, row.CumFormat)\n      addCell(tr, percent(row.Cum))\n      addCell(t" +
    "r, row.Name)\n      addCell(tr, row.InlineLabel)\n      fragment.appendChild(tr)\n " +
    "   }\n\n    rows.textContent = \'\'  // Remove old rows\n    rows.appendChild(fragmen" +
    "t)\n  }\n\n  // Make different column headers trigger sorting.\n  function bindSort(" +
    "id, column) {\n    const hdr = document.getElementById(id)\n    if (hdr == null) r" +
    "eturn\n    const fn = function() { sortBy(column) }\n    hdr.addEventListener(\"cli" +
    "ck\", fn)\n    hdr.addEventListener(\"touch\", fn)\n  }\n  bindSort(\"flathdr1\", \"Flat\"" +
    ")\n  bindSort(\"flathdr2\", \"Flat\")\n  bindSort(\"cumhdr1\", \"Cum\")\n  bindSort(\"cumhdr" +
    "2\", \"Cum\")\n  bindSort(\"namehdr\", \"Name\")\n}\n\nviewer({{.BaseURL}}, {{.Nodes}})\nmak" +
    "eTopTable({{.Total}}, {{.Top}})\n</script>\n</body>\n</html>\n{{end}}\n\n{{define \"sou" +
    "rcelisting\" -}}\n<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"utf-8\">\n<title>{{.T" +
    "itle}}</title>\n{{template \"css\" .}}\n{{template \"weblistcss\" .}}\n{{template \"webl" +
    "istjs\" .}}\n</head>\n<body>\n\n{{template \"header\" .}}\n\n<div id=\"bodycontainer\">\n{{." +
    "HTMLBody}}\n</div>\n\n{{template \"script\" .}}\n<script>viewer({{.BaseURL}}, null)</s" +
    "cript>\n</body>\n</html>\n{{end}}\n\n{{define \"plaintext\" -}}\n<!DOCTYPE html>\n<html>\n" +
    "<head>\n<meta charset=\"utf-8\">\n<title>{{.Title}}</title>\n{{template \"css\" .}}\n</h" +
    "ead>\n<body>\n\n{{template \"header\" .}}\n\n<div id=\"bodycontainer\">\n<pre>\n{{.TextBody" +
    "}}\n</pre>\n</div>\n\n{{template \"script\" .}}\n<script>viewer({{.BaseURL}}, null)</sc" +
    "ript>\n</body>\n</html>\n{{end}}\n"));
        }
    }
}}}}}}}
