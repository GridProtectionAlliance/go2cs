// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:33 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\html.go
using bytes = go.bytes_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using html = go.html_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public partial struct HTMLWriter
        {
            public io.WriteCloser w;
            public ptr<Func> Func;
            public @string path;
            public ptr<dotWriter> dot;
            public slice<byte> prevHash;
            public slice<@string> pendingPhases;
            public slice<@string> pendingTitles;
        }

        public static ptr<HTMLWriter> NewHTMLWriter(@string path, ptr<Func> _addr_f, @string cfgMask)
        {
            ref Func f = ref _addr_f.val;

            var (out, err) = os.OpenFile(path, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0644L);
            if (err != null)
            {
                f.Fatalf("%v", err);
            }

            var (pwd, err) = os.Getwd();
            if (err != null)
            {
                f.Fatalf("%v", err);
            }

            ref HTMLWriter html = ref heap(new HTMLWriter(w:out,Func:f,path:filepath.Join(pwd,path),dot:newDotWriter(cfgMask),), out ptr<HTMLWriter> _addr_html);
            html.start();
            return _addr__addr_html!;

        }

        // Fatalf reports an error and exits.
        private static void Fatalf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] args)
        {
            args = args.Clone();
            ref HTMLWriter w = ref _addr_w.val;

            var fe = w.Func.Frontend();
            fe.Fatalf(src.NoXPos, msg, args);
        }

        // Logf calls the (w *HTMLWriter).Func's Logf method passing along a msg and args.
        private static void Logf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] args)
        {
            args = args.Clone();
            ref HTMLWriter w = ref _addr_w.val;

            w.Func.Logf(msg, args);
        }

        private static void start(this ptr<HTMLWriter> _addr_w)
        {
            ref HTMLWriter w = ref _addr_w.val;

            if (w == null)
            {
                return ;
            }

            w.WriteString("<html>");
            w.WriteString("<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html;charset=UTF-8\">\n<style>" +
    "\n\nbody {\n    font-size: 14px;\n    font-family: Arial, sans-serif;\n}\n\nh1 {\n    fo" +
    "nt-size: 18px;\n    display: inline-block;\n    margin: 0 1em .5em 0;\n}\n\n#helplink" +
    " {\n    display: inline-block;\n}\n\n#help {\n    display: none;\n}\n\n.stats {\n    font" +
    "-size: 60%;\n}\n\ntable {\n    border: 1px solid black;\n    table-layout: fixed;\n   " +
    " width: 300px;\n}\n\nth, td {\n    border: 1px solid black;\n    overflow: hidden;\n  " +
    "  width: 400px;\n    vertical-align: top;\n    padding: 5px;\n}\n\ntd > h2 {\n    curs" +
    "or: pointer;\n    font-size: 120%;\n    margin: 5px 0px 5px 0px;\n}\n\ntd.collapsed {" +
    "\n    font-size: 12px;\n    width: 12px;\n    border: 1px solid white;\n    padding:" +
    " 2px;\n    cursor: pointer;\n    background: #fafafa;\n}\n\ntd.collapsed div {\n    /*" +
    " TODO: Flip the direction of the phase\'s title 90 degrees on a collapsed column." +
    " */\n    writing-mode: vertical-lr;\n    white-space: pre;\n}\n\ncode, pre, .lines, ." +
    "ast {\n    font-family: Menlo, monospace;\n    font-size: 12px;\n}\n\npre {\n    -moz-" +
    "tab-size: 4;\n    -o-tab-size:   4;\n    tab-size:      4;\n}\n\n.allow-x-scroll {\n  " +
    "  overflow-x: scroll;\n}\n\n.lines {\n    float: left;\n    overflow: hidden;\n    tex" +
    "t-align: right;\n    margin-top: 7px;\n}\n\n.lines div {\n    padding-right: 10px;\n  " +
    "  color: gray;\n}\n\ndiv.line-number {\n    font-size: 12px;\n}\n\n.ast {\n    white-spa" +
    "ce: nowrap;\n}\n\ntd.ssa-prog {\n    width: 600px;\n    word-wrap: break-word;\n}\n\nli " +
    "{\n    list-style-type: none;\n}\n\nli.ssa-long-value {\n    text-indent: -2em;  /* i" +
    "ndent wrapped lines */\n}\n\nli.ssa-value-list {\n    display: inline;\n}\n\nli.ssa-sta" +
    "rt-block {\n    padding: 0;\n    margin: 0;\n}\n\nli.ssa-end-block {\n    padding: 0;\n" +
    "    margin: 0;\n}\n\nul.ssa-print-func {\n    padding-left: 0;\n}\n\nli.ssa-start-block" +
    " button {\n    padding: 0 1em;\n    margin: 0;\n    border: none;\n    display: inli" +
    "ne;\n    font-size: 14px;\n    float: right;\n}\n\nbutton:hover {\n    background-colo" +
    "r: #eee;\n    cursor: pointer;\n}\n\ndl.ssa-gen {\n    padding-left: 0;\n}\n\ndt.ssa-pro" +
    "g-src {\n    padding: 0;\n    margin: 0;\n    float: left;\n    width: 4em;\n}\n\ndd.ss" +
    "a-prog {\n    padding: 0;\n    margin-right: 0;\n    margin-left: 4em;\n}\n\n.dead-val" +
    "ue {\n    color: gray;\n}\n\n.dead-block {\n    opacity: 0.5;\n}\n\n.depcycle {\n    font" +
    "-style: italic;\n}\n\n.line-number {\n    font-size: 11px;\n}\n\n.no-line-number {\n    " +
    "font-size: 11px;\n    color: gray;\n}\n\n.zoom {\n\tposition: absolute;\n\tfloat: left;\n" +
    "\twhite-space: nowrap;\n\tbackground-color: #eee;\n}\n\n.zoom a:link, .zoom a:visited " +
    " {\n    text-decoration: none;\n    color: blue;\n    font-size: 16px;\n    padding:" +
    " 4px 2px;\n}\n\nsvg {\n    cursor: default;\n    outline: 1px solid #eee;\n    width: " +
    "100%;\n}\n\nbody.darkmode {\n    background-color: rgb(21, 21, 21);\n    color: rgb(2" +
    "30, 255, 255);\n    opacity: 100%;\n}\n\ntd.darkmode {\n    background-color: rgb(21," +
    " 21, 21);\n    border: 1px solid gray;\n}\n\nbody.darkmode table, th {\n    border: 1" +
    "px solid gray;\n}\n\nbody.darkmode text {\n    fill: white;\n}\n\nbody.darkmode svg pol" +
    "ygon:first-child {\n    fill: rgb(21, 21, 21);\n}\n\n.highlight-aquamarine     { bac" +
    "kground-color: aquamarine; color: black; }\n.highlight-coral          { backgroun" +
    "d-color: coral; color: black; }\n.highlight-lightpink      { background-color: li" +
    "ghtpink; color: black; }\n.highlight-lightsteelblue { background-color: lightstee" +
    "lblue; color: black; }\n.highlight-palegreen      { background-color: palegreen; " +
    "color: black; }\n.highlight-skyblue        { background-color: skyblue; color: bl" +
    "ack; }\n.highlight-lightgray      { background-color: lightgray; color: black; }\n" +
    ".highlight-yellow         { background-color: yellow; color: black; }\n.highlight" +
    "-lime           { background-color: lime; color: black; }\n.highlight-khaki      " +
    "    { background-color: khaki; color: black; }\n.highlight-aqua           { backg" +
    "round-color: aqua; color: black; }\n.highlight-salmon         { background-color:" +
    " salmon; color: black; }\n\n/* Ensure all dead values/blocks continue to have gray" +
    " font color in dark mode with highlights */\n.dead-value span.highlight-aquamarin" +
    "e,\n.dead-block.highlight-aquamarine,\n.dead-value span.highlight-coral,\n.dead-blo" +
    "ck.highlight-coral,\n.dead-value span.highlight-lightpink,\n.dead-block.highlight-" +
    "lightpink,\n.dead-value span.highlight-lightsteelblue,\n.dead-block.highlight-ligh" +
    "tsteelblue,\n.dead-value span.highlight-palegreen,\n.dead-block.highlight-palegree" +
    "n,\n.dead-value span.highlight-skyblue,\n.dead-block.highlight-skyblue,\n.dead-valu" +
    "e span.highlight-lightgray,\n.dead-block.highlight-lightgray,\n.dead-value span.hi" +
    "ghlight-yellow,\n.dead-block.highlight-yellow,\n.dead-value span.highlight-lime,\n." +
    "dead-block.highlight-lime,\n.dead-value span.highlight-khaki,\n.dead-block.highlig" +
    "ht-khaki,\n.dead-value span.highlight-aqua,\n.dead-block.highlight-aqua,\n.dead-val" +
    "ue span.highlight-salmon,\n.dead-block.highlight-salmon {\n    color: gray;\n}\n\n.ou" +
    "tline-blue           { outline: #2893ff solid 2px; }\n.outline-red            { o" +
    "utline: red solid 2px; }\n.outline-blueviolet     { outline: blueviolet solid 2px" +
    "; }\n.outline-darkolivegreen { outline: darkolivegreen solid 2px; }\n.outline-fuch" +
    "sia        { outline: fuchsia solid 2px; }\n.outline-sienna         { outline: si" +
    "enna solid 2px; }\n.outline-gold           { outline: gold solid 2px; }\n.outline-" +
    "orangered      { outline: orangered solid 2px; }\n.outline-teal           { outli" +
    "ne: teal solid 2px; }\n.outline-maroon         { outline: maroon solid 2px; }\n.ou" +
    "tline-black          { outline: black solid 2px; }\n\nellipse.outline-blue        " +
    "   { stroke-width: 2px; stroke: #2893ff; }\nellipse.outline-red            { stro" +
    "ke-width: 2px; stroke: red; }\nellipse.outline-blueviolet     { stroke-width: 2px" +
    "; stroke: blueviolet; }\nellipse.outline-darkolivegreen { stroke-width: 2px; stro" +
    "ke: darkolivegreen; }\nellipse.outline-fuchsia        { stroke-width: 2px; stroke" +
    ": fuchsia; }\nellipse.outline-sienna         { stroke-width: 2px; stroke: sienna;" +
    " }\nellipse.outline-gold           { stroke-width: 2px; stroke: gold; }\nellipse.o" +
    "utline-orangered      { stroke-width: 2px; stroke: orangered; }\nellipse.outline-" +
    "teal           { stroke-width: 2px; stroke: teal; }\nellipse.outline-maroon      " +
    "   { stroke-width: 2px; stroke: maroon; }\nellipse.outline-black          { strok" +
    "e-width: 2px; stroke: black; }\n\n/* Capture alternative for outline-black and ell" +
    "ipse.outline-black when in dark mode */\nbody.darkmode .outline-black        { ou" +
    "tline: gray solid 2px; }\nbody.darkmode ellipse.outline-black { outline: gray sol" +
    "id 2px; }\n\n</style>\n\n<script type=\"text/javascript\">\n// ordered list of all avai" +
    "lable highlight colors\nvar highlights = [\n    \"highlight-aquamarine\",\n    \"highl" +
    "ight-coral\",\n    \"highlight-lightpink\",\n    \"highlight-lightsteelblue\",\n    \"hig" +
    "hlight-palegreen\",\n    \"highlight-skyblue\",\n    \"highlight-lightgray\",\n    \"high" +
    "light-yellow\",\n    \"highlight-lime\",\n    \"highlight-khaki\",\n    \"highlight-aqua\"" +
    ",\n    \"highlight-salmon\"\n];\n\n// state: which value is highlighted this color?\nva" +
    "r highlighted = {};\nfor (var i = 0; i < highlights.length; i++) {\n    highlighte" +
    "d[highlights[i]] = \"\";\n}\n\n// ordered list of all available outline colors\nvar ou" +
    "tlines = [\n    \"outline-blue\",\n    \"outline-red\",\n    \"outline-blueviolet\",\n    " +
    "\"outline-darkolivegreen\",\n    \"outline-fuchsia\",\n    \"outline-sienna\",\n    \"outl" +
    "ine-gold\",\n    \"outline-orangered\",\n    \"outline-teal\",\n    \"outline-maroon\",\n  " +
    "  \"outline-black\"\n];\n\n// state: which value is outlined this color?\nvar outlined" +
    " = {};\nfor (var i = 0; i < outlines.length; i++) {\n    outlined[outlines[i]] = \"" +
    "\";\n}\n\nwindow.onload = function() {\n    if (window.matchMedia && window.matchMedi" +
    "a(\"(prefers-color-scheme: dark)\").matches) {\n        toggleDarkMode();\n        d" +
    "ocument.getElementById(\"dark-mode-button\").checked = true;\n    }\n\n    var ssaEle" +
    "mClicked = function(elem, event, selections, selected) {\n        event.stopPropa" +
    "gation();\n\n        // TODO: pushState with updated state and read it on page loa" +
    "d,\n        // so that state can survive across reloads\n\n        // find all valu" +
    "es with the same name\n        var c = elem.classList.item(0);\n        var x = do" +
    "cument.getElementsByClassName(c);\n\n        // if selected, remove selections fro" +
    "m all of them\n        // otherwise, attempt to add\n\n        var remove = \"\";\n   " +
    "     for (var i = 0; i < selections.length; i++) {\n            var color = selec" +
    "tions[i];\n            if (selected[color] == c) {\n                remove = color" +
    ";\n                break;\n            }\n        }\n\n        if (remove != \"\") {\n  " +
    "          for (var i = 0; i < x.length; i++) {\n                x[i].classList.re" +
    "move(remove);\n            }\n            selected[remove] = \"\";\n            retur" +
    "n;\n        }\n\n        // we\'re adding a selection\n        // find first availabl" +
    "e color\n        var avail = \"\";\n        for (var i = 0; i < selections.length; i" +
    "++) {\n            var color = selections[i];\n            if (selected[color] == " +
    "\"\") {\n                avail = color;\n                break;\n            }\n      " +
    "  }\n        if (avail == \"\") {\n            alert(\"out of selection colors; go ad" +
    "d more\");\n            return;\n        }\n\n        // set that as the selection\n  " +
    "      for (var i = 0; i < x.length; i++) {\n            x[i].classList.add(avail)" +
    ";\n        }\n        selected[avail] = c;\n    };\n\n    var ssaValueClicked = funct" +
    "ion(event) {\n        ssaElemClicked(this, event, highlights, highlighted);\n    }" +
    ";\n\n    var ssaBlockClicked = function(event) {\n        ssaElemClicked(this, even" +
    "t, outlines, outlined);\n    };\n\n    var ssavalues = document.getElementsByClassN" +
    "ame(\"ssa-value\");\n    for (var i = 0; i < ssavalues.length; i++) {\n        ssava" +
    "lues[i].addEventListener(\'click\', ssaValueClicked);\n    }\n\n    var ssalongvalues" +
    " = document.getElementsByClassName(\"ssa-long-value\");\n    for (var i = 0; i < ss" +
    "alongvalues.length; i++) {\n        // don\'t attach listeners to li nodes, just t" +
    "he spans they contain\n        if (ssalongvalues[i].nodeName == \"SPAN\") {\n       " +
    "     ssalongvalues[i].addEventListener(\'click\', ssaValueClicked);\n        }\n    " +
    "}\n\n    var ssablocks = document.getElementsByClassName(\"ssa-block\");\n    for (va" +
    "r i = 0; i < ssablocks.length; i++) {\n        ssablocks[i].addEventListener(\'cli" +
    "ck\', ssaBlockClicked);\n    }\n\n    var lines = document.getElementsByClassName(\"l" +
    "ine-number\");\n    for (var i = 0; i < lines.length; i++) {\n        lines[i].addE" +
    "ventListener(\'click\', ssaValueClicked);\n    }\n\n    // Contains phase names which" +
    " are expanded by default. Other columns are collapsed.\n    var expandedDefault =" +
    " [\n        \"start\",\n        \"deadcode\",\n        \"opt\",\n        \"lower\",\n        " +
    "\"late-deadcode\",\n        \"regalloc\",\n        \"genssa\",\n    ];\n\n    function togg" +
    "ler(phase) {\n        return function() {\n            toggle_cell(phase+\'-col\');\n" +
    "            toggle_cell(phase+\'-exp\');\n        };\n    }\n\n    function toggle_cel" +
    "l(id) {\n        var e = document.getElementById(id);\n        if (e.style.display" +
    " == \'table-cell\') {\n            e.style.display = \'none\';\n        } else {\n     " +
    "       e.style.display = \'table-cell\';\n        }\n    }\n\n    // Go through all co" +
    "lumns and collapse needed phases.\n    const td = document.getElementsByTagName(\"" +
    "td\");\n    for (let i = 0; i < td.length; i++) {\n        const id = td[i].id;\n   " +
    "     const phase = id.substr(0, id.length-4);\n        let show = expandedDefault" +
    ".indexOf(phase) !== -1\n\n        // If show == false, check to see if this is a c" +
    "ombined column (multiple phases).\n        // If combined, check each of the phas" +
    "es to see if they are in our expandedDefaults.\n        // If any are found, that" +
    " entire combined column gets shown.\n        if (!show) {\n            const combi" +
    "ned = phase.split(\'--+--\');\n            const len = combined.length;\n           " +
    " if (len > 1) {\n                for (let i = 0; i < len; i++) {\n                " +
    "    if (expandedDefault.indexOf(combined[i]) !== -1) {\n                        s" +
    "how = true;\n                        break;\n                    }\n               " +
    " }\n            }\n        }\n        if (id.endsWith(\"-exp\")) {\n            const " +
    "h2Els = td[i].getElementsByTagName(\"h2\");\n            const len = h2Els.length;\n" +
    "            if (len > 0) {\n                for (let i = 0; i < len; i++) {\n     " +
    "               h2Els[i].addEventListener(\'click\', toggler(phase));\n             " +
    "   }\n            }\n        } else {\n            td[i].addEventListener(\'click\', " +
    "toggler(phase));\n        }\n        if (id.endsWith(\"-col\") && show || id.endsWit" +
    "h(\"-exp\") && !show) {\n            td[i].style.display = \'none\';\n            cont" +
    "inue;\n        }\n        td[i].style.display = \'table-cell\';\n    }\n\n    // find a" +
    "ll svg block nodes, add their block classes\n    var nodes = document.querySelect" +
    "orAll(\'*[id^=\"graph_node_\"]\');\n    for (var i = 0; i < nodes.length; i++) {\n    " +
    "\tvar node = nodes[i];\n    \tvar name = node.id.toString();\n    \tvar block = name." +
    "substring(name.lastIndexOf(\"_\")+1);\n    \tnode.classList.remove(\"node\");\n    \tnod" +
    "e.classList.add(block);\n        node.addEventListener(\'click\', ssaBlockClicked);" +
    "\n        var ellipse = node.getElementsByTagName(\'ellipse\')[0];\n        ellipse." +
    "classList.add(block);\n        ellipse.addEventListener(\'click\', ssaBlockClicked)" +
    ";\n    }\n\n    // make big graphs smaller\n    var targetScale = 0.5;\n    var nodes" +
    " = document.querySelectorAll(\'*[id^=\"svg_graph_\"]\');\n    // TODO: Implement smar" +
    "ter auto-zoom using the viewBox attribute\n    // and in case of big graphs set t" +
    "he width and height of the svg graph to\n    // maximum allowed.\n    for (var i =" +
    " 0; i < nodes.length; i++) {\n    \tvar node = nodes[i];\n    \tvar name = node.id.t" +
    "oString();\n    \tvar phase = name.substring(name.lastIndexOf(\"_\")+1);\n    \tvar gN" +
    "ode = document.getElementById(\"g_graph_\"+phase);\n    \tvar scale = gNode.transfor" +
    "m.baseVal.getItem(0).matrix.a;\n    \tif (scale > targetScale) {\n    \t\tnode.width." +
    "baseVal.value *= targetScale / scale;\n    \t\tnode.height.baseVal.value *= targetS" +
    "cale / scale;\n    \t}\n    }\n};\n\nfunction toggle_visibility(id) {\n    var e = docu" +
    "ment.getElementById(id);\n    if (e.style.display == \'block\') {\n        e.style.d" +
    "isplay = \'none\';\n    } else {\n        e.style.display = \'block\';\n    }\n}\n\nfuncti" +
    "on hideBlock(el) {\n    var es = el.parentNode.parentNode.getElementsByClassName(" +
    "\"ssa-value-list\");\n    if (es.length===0)\n        return;\n    var e = es[0];\n   " +
    " if (e.style.display === \'block\' || e.style.display === \'\') {\n        e.style.di" +
    "splay = \'none\';\n        el.innerHTML = \'+\';\n    } else {\n        e.style.display" +
    " = \'block\';\n        el.innerHTML = \'-\';\n    }\n}\n\n// TODO: scale the graph with t" +
    "he viewBox attribute.\nfunction graphReduce(id) {\n    var node = document.getElem" +
    "entById(id);\n    if (node) {\n    \t\tnode.width.baseVal.value *= 0.9;\n    \t\tnode.h" +
    "eight.baseVal.value *= 0.9;\n    }\n    return false;\n}\n\nfunction graphEnlarge(id)" +
    " {\n    var node = document.getElementById(id);\n    if (node) {\n    \t\tnode.width." +
    "baseVal.value *= 1.1;\n    \t\tnode.height.baseVal.value *= 1.1;\n    }\n    return f" +
    "alse;\n}\n\nfunction makeDraggable(event) {\n    var svg = event.target;\n    if (win" +
    "dow.PointerEvent) {\n        svg.addEventListener(\'pointerdown\', startDrag);\n    " +
    "    svg.addEventListener(\'pointermove\', drag);\n        svg.addEventListener(\'poi" +
    "nterup\', endDrag);\n        svg.addEventListener(\'pointerleave\', endDrag);\n    } " +
    "else {\n        svg.addEventListener(\'mousedown\', startDrag);\n        svg.addEven" +
    "tListener(\'mousemove\', drag);\n        svg.addEventListener(\'mouseup\', endDrag);\n" +
    "        svg.addEventListener(\'mouseleave\', endDrag);\n    }\n\n    var point = svg." +
    "createSVGPoint();\n    var isPointerDown = false;\n    var pointerOrigin;\n    var " +
    "viewBox = svg.viewBox.baseVal;\n\n    function getPointFromEvent (event) {\n       " +
    " point.x = event.clientX;\n        point.y = event.clientY;\n\n        // We get th" +
    "e current transformation matrix of the SVG and we inverse it\n        var inverte" +
    "dSVGMatrix = svg.getScreenCTM().inverse();\n        return point.matrixTransform(" +
    "invertedSVGMatrix);\n    }\n\n    function startDrag(event) {\n        isPointerDown" +
    " = true;\n        pointerOrigin = getPointFromEvent(event);\n    }\n\n    function d" +
    "rag(event) {\n        if (!isPointerDown) {\n            return;\n        }\n       " +
    " event.preventDefault();\n\n        var pointerPosition = getPointFromEvent(event)" +
    ";\n        viewBox.x -= (pointerPosition.x - pointerOrigin.x);\n        viewBox.y " +
    "-= (pointerPosition.y - pointerOrigin.y);\n    }\n\n    function endDrag(event) {\n " +
    "       isPointerDown = false;\n    }\n}\n\nfunction toggleDarkMode() {\n    document." +
    "body.classList.toggle(\'darkmode\');\n\n    // Collect all of the \"collapsed\" elemen" +
    "ts and apply dark mode on each collapsed column\n    const collapsedEls = documen" +
    "t.getElementsByClassName(\'collapsed\');\n    const len = collapsedEls.length;\n\n   " +
    " for (let i = 0; i < len; i++) {\n        collapsedEls[i].classList.toggle(\'darkm" +
    "ode\');\n    }\n\n    // Collect and spread the appropriate elements from all of the" +
    " svgs on the page into one array\n    const svgParts = [\n        ...document.quer" +
    "ySelectorAll(\'path\'),\n        ...document.querySelectorAll(\'ellipse\'),\n        ." +
    "..document.querySelectorAll(\'polygon\'),\n    ];\n\n    // Iterate over the svgParts" +
    " specifically looking for white and black fill/stroke to be toggled.\n    // The " +
    "verbose conditional is intentional here so that we do not mutate any svg path, e" +
    "llipse, or polygon that is of any color other than white or black.\n    svgParts." +
    "forEach(el => {\n        if (el.attributes.stroke.value === \'white\') {\n          " +
    "  el.attributes.stroke.value = \'black\';\n        } else if (el.attributes.stroke." +
    "value === \'black\') {\n            el.attributes.stroke.value = \'white\';\n        }" +
    "\n        if (el.attributes.fill.value === \'white\') {\n            el.attributes.f" +
    "ill.value = \'black\';\n        } else if (el.attributes.fill.value === \'black\') {\n" +
    "            el.attributes.fill.value = \'white\';\n        }\n    });\n}\n\n</script>\n\n" +
    "</head>");
            w.WriteString("<body>");
            w.WriteString("<h1>");
            w.WriteString(html.EscapeString(w.Func.Name));
            w.WriteString("</h1>");
            w.WriteString(@"
<a href=""#"" onclick=""toggle_visibility('help');return false;"" id=""helplink"">help</a>
<div id=""help"">

<p>
Click on a value or block to toggle highlighting of that value/block
and its uses.  (Values and blocks are highlighted by ID, and IDs of
dead items may be reused, so not all highlights necessarily correspond
to the clicked item.)
</p>

<p>
Faded out values and blocks are dead code that has not been eliminated.
</p>

<p>
Values printed in italics have a dependency cycle.
</p>

<p>
<b>CFG</b>: Dashed edge is for unlikely branches. Blue color is for backward edges.
Edge with a dot means that this edge follows the order in which blocks were laidout.
</p>

</div>
<label for=""dark-mode-button"" style=""margin-left: 15px; cursor: pointer;"">darkmode</label>
<input type=""checkbox"" onclick=""toggleDarkMode();"" id=""dark-mode-button"" style=""cursor: pointer"" />
");
            w.WriteString("<table>");
            w.WriteString("<tr>");

        }

        private static void Close(this ptr<HTMLWriter> _addr_w)
        {
            ref HTMLWriter w = ref _addr_w.val;

            if (w == null)
            {
                return ;
            }

            io.WriteString(w.w, "</tr>");
            io.WriteString(w.w, "</table>");
            io.WriteString(w.w, "</body>");
            io.WriteString(w.w, "</html>");
            w.w.Close();
            fmt.Printf("dumped SSA to %v\n", w.path);

        }

        // WritePhase writes f in a column headed by title.
        // phase is used for collapsing columns and should be unique across the table.
        private static void WritePhase(this ptr<HTMLWriter> _addr_w, @string phase, @string title)
        {
            ref HTMLWriter w = ref _addr_w.val;

            if (w == null)
            {
                return ; // avoid generating HTML just to discard it
            }

            var hash = hashFunc(w.Func);
            w.pendingPhases = append(w.pendingPhases, phase);
            w.pendingTitles = append(w.pendingTitles, title);
            if (!bytes.Equal(hash, w.prevHash))
            {
                w.flushPhases();
            }

            w.prevHash = hash;

        }

        // flushPhases collects any pending phases and titles, writes them to the html, and resets the pending slices.
        private static void flushPhases(this ptr<HTMLWriter> _addr_w)
        {
            ref HTMLWriter w = ref _addr_w.val;

            var phaseLen = len(w.pendingPhases);
            if (phaseLen == 0L)
            {
                return ;
            }

            var phases = strings.Join(w.pendingPhases, "  +  ");
            w.WriteMultiTitleColumn(phases, w.pendingTitles, fmt.Sprintf("hash-%x", w.prevHash), w.Func.HTML(w.pendingPhases[phaseLen - 1L], w.dot));
            w.pendingPhases = w.pendingPhases[..0L];
            w.pendingTitles = w.pendingTitles[..0L];

        }

        // FuncLines contains source code for a function to be displayed
        // in sources column.
        public partial struct FuncLines
        {
            public @string Filename;
            public ulong StartLineno;
            public slice<@string> Lines;
        }

        // ByTopo sorts topologically: target function is on top,
        // followed by inlined functions sorted by filename and line numbers.
        public partial struct ByTopo // : slice<ptr<FuncLines>>
        {
        }

        public static long Len(this ByTopo x)
        {
            return len(x);
        }
        public static void Swap(this ByTopo x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];
        }
        public static bool Less(this ByTopo x, long i, long j)
        {
            var a = x[i];
            var b = x[j];
            if (a.Filename == b.Filename)
            {
                return a.StartLineno < b.StartLineno;
            }

            return a.Filename < b.Filename;

        }

        // WriteSources writes lines as source code in a column headed by title.
        // phase is used for collapsing columns and should be unique across the table.
        private static void WriteSources(this ptr<HTMLWriter> _addr_w, @string phase, slice<ptr<FuncLines>> all)
        {
            ref HTMLWriter w = ref _addr_w.val;

            if (w == null)
            {
                return ; // avoid generating HTML just to discard it
            }

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprint(_addr_buf, "<div class=\"lines\" style=\"width: 8%\">");
            @string filename = "";
            {
                var fl__prev1 = fl;

                foreach (var (_, __fl) in all)
                {
                    fl = __fl;
                    fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
                    if (filename != fl.Filename)
                    {
                        fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
                        filename = fl.Filename;
                    }

                    {
                        var i__prev2 = i;

                        foreach (var (__i) in fl.Lines)
                        {
                            i = __i;
                            var ln = int(fl.StartLineno) + i;
                            fmt.Fprintf(_addr_buf, "<div class=\"l%v line-number\">%v</div>", ln, ln);
                        }

                        i = i__prev2;
                    }
                }

                fl = fl__prev1;
            }

            fmt.Fprint(_addr_buf, "</div><div style=\"width: 92%\"><pre>");
            filename = "";
            {
                var fl__prev1 = fl;

                foreach (var (_, __fl) in all)
                {
                    fl = __fl;
                    fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
                    if (filename != fl.Filename)
                    {
                        fmt.Fprintf(_addr_buf, "<div><strong>%v</strong></div>", fl.Filename);
                        filename = fl.Filename;
                    }

                    {
                        var i__prev2 = i;

                        foreach (var (__i, __line) in fl.Lines)
                        {
                            i = __i;
                            line = __line;
                            ln = int(fl.StartLineno) + i;
                            @string escaped = default;
                            if (strings.TrimSpace(line) == "")
                            {
                                escaped = "&nbsp;";
                            }
                            else
                            {
                                escaped = html.EscapeString(line);
                            }

                            fmt.Fprintf(_addr_buf, "<div class=\"l%v line-number\">%v</div>", ln, escaped);

                        }

                        i = i__prev2;
                    }
                }

                fl = fl__prev1;
            }

            fmt.Fprint(_addr_buf, "</pre></div>");
            w.WriteColumn(phase, phase, "allow-x-scroll", buf.String());

        }

        private static void WriteAST(this ptr<HTMLWriter> _addr_w, @string phase, ptr<bytes.Buffer> _addr_buf)
        {
            ref HTMLWriter w = ref _addr_w.val;
            ref bytes.Buffer buf = ref _addr_buf.val;

            if (w == null)
            {
                return ; // avoid generating HTML just to discard it
            }

            var lines = strings.Split(buf.String(), "\n");
            ref bytes.Buffer @out = ref heap(out ptr<bytes.Buffer> _addr_@out);

            fmt.Fprint(_addr_out, "<div>");
            foreach (var (_, l) in lines)
            {
                l = strings.TrimSpace(l);
                @string escaped = default;
                @string lineNo = default;
                if (l == "")
                {
                    escaped = "&nbsp;";
                }
                else
                {
                    if (strings.HasPrefix(l, "buildssa"))
                    {
                        escaped = fmt.Sprintf("<b>%v</b>", l);
                    }
                    else
                    { 
                        // Parse the line number from the format l(123).
                        var idx = strings.Index(l, " l(");
                        if (idx != -1L)
                        {
                            var subl = l[idx + 3L..];
                            var idxEnd = strings.Index(subl, ")");
                            if (idxEnd != -1L)
                            {
                                {
                                    var (_, err) = strconv.Atoi(subl[..idxEnd]);

                                    if (err == null)
                                    {
                                        lineNo = subl[..idxEnd];
                                    }

                                }

                            }

                        }

                        escaped = html.EscapeString(l);

                    }

                }

                if (lineNo != "")
                {
                    fmt.Fprintf(_addr_out, "<div class=\"l%v line-number ast\">%v</div>", lineNo, escaped);
                }
                else
                {
                    fmt.Fprintf(_addr_out, "<div class=\"ast\">%v</div>", escaped);
                }

            }
            fmt.Fprint(_addr_out, "</div>");
            w.WriteColumn(phase, phase, "allow-x-scroll", @out.String());

        }

        // WriteColumn writes raw HTML in a column headed by title.
        // It is intended for pre- and post-compilation log output.
        private static void WriteColumn(this ptr<HTMLWriter> _addr_w, @string phase, @string title, @string @class, @string html)
        {
            ref HTMLWriter w = ref _addr_w.val;

            w.WriteMultiTitleColumn(phase, new slice<@string>(new @string[] { title }), class, html);
        }

        private static void WriteMultiTitleColumn(this ptr<HTMLWriter> _addr_w, @string phase, slice<@string> titles, @string @class, @string html)
        {
            ref HTMLWriter w = ref _addr_w.val;

            if (w == null)
            {
                return ;
            }

            var id = strings.Replace(phase, " ", "-", -1L); 
            // collapsed column
            w.Printf("<td id=\"%v-col\" class=\"collapsed\"><div>%v</div></td>", id, phase);

            if (class == "")
            {
                w.Printf("<td id=\"%v-exp\">", id);
            }
            else
            {
                w.Printf("<td id=\"%v-exp\" class=\"%v\">", id, class);
            }

            foreach (var (_, title) in titles)
            {
                w.WriteString("<h2>" + title + "</h2>");
            }
            w.WriteString(html);
            w.WriteString("</td>\n");

        }

        private static void Printf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] v)
        {
            v = v.Clone();
            ref HTMLWriter w = ref _addr_w.val;

            {
                var (_, err) = fmt.Fprintf(w.w, msg, v);

                if (err != null)
                {
                    w.Fatalf("%v", err);
                }

            }

        }

        private static void WriteString(this ptr<HTMLWriter> _addr_w, @string s)
        {
            ref HTMLWriter w = ref _addr_w.val;

            {
                var (_, err) = io.WriteString(w.w, s);

                if (err != null)
                {
                    w.Fatalf("%v", err);
                }

            }

        }

        private static @string HTML(this ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;
 
            // TODO: Using the value ID as the class ignores the fact
            // that value IDs get recycled and that some values
            // are transmuted into other values.
            var s = v.String();
            return fmt.Sprintf("<span class=\"%s ssa-value\">%s</span>", s, s);

        }

        private static @string LongHTML(this ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;
 
            // TODO: Any intra-value formatting?
            // I'm wary of adding too much visual noise,
            // but a little bit might be valuable.
            // We already have visual noise in the form of punctuation
            // maybe we could replace some of that with formatting.
            var s = fmt.Sprintf("<span class=\"%s ssa-long-value\">", v.String());

            @string linenumber = "<span class=\"no-line-number\">(?)</span>";
            if (v.Pos.IsKnown())
            {
                linenumber = fmt.Sprintf("<span class=\"l%v line-number\">(%s)</span>", v.Pos.LineNumber(), v.Pos.LineNumberHTML());
            }

            s += fmt.Sprintf("%s %s = %s", v.HTML(), linenumber, v.Op.String());

            s += " &lt;" + html.EscapeString(v.Type.String()) + "&gt;";
            s += html.EscapeString(v.auxString());
            foreach (var (_, a) in v.Args)
            {
                s += fmt.Sprintf(" %s", a.HTML());
            }
            var r = v.Block.Func.RegAlloc;
            if (int(v.ID) < len(r) && r[v.ID] != null)
            {
                s += " : " + html.EscapeString(r[v.ID].String());
            }

            slice<@string> names = default;
            foreach (var (name, values) in v.Block.Func.NamedValues)
            {
                foreach (var (_, value) in values)
                {
                    if (value == v)
                    {
                        names = append(names, name.String());
                        break; // drop duplicates.
                    }

                }

            }
            if (len(names) != 0L)
            {
                s += " (" + strings.Join(names, ", ") + ")";
            }

            s += "</span>";
            return s;

        }

        private static @string HTML(this ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;
 
            // TODO: Using the value ID as the class ignores the fact
            // that value IDs get recycled and that some values
            // are transmuted into other values.
            var s = html.EscapeString(b.String());
            return fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", s, s);

        }

        private static @string LongHTML(this ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;
 
            // TODO: improve this for HTML?
            var s = fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", html.EscapeString(b.String()), html.EscapeString(b.Kind.String()));
            if (b.Aux != null)
            {
                s += html.EscapeString(fmt.Sprintf(" {%v}", b.Aux));
            }

            {
                var t = b.AuxIntString();

                if (t != "")
                {
                    s += html.EscapeString(fmt.Sprintf(" [%v]", t));
                }

            }

            {
                var c__prev1 = c;

                foreach (var (_, __c) in b.ControlValues())
                {
                    c = __c;
                    s += fmt.Sprintf(" %s", c.HTML());
                }

                c = c__prev1;
            }

            if (len(b.Succs) > 0L)
            {
                s += " &#8594;"; // right arrow
                foreach (var (_, e) in b.Succs)
                {
                    var c = e.b;
                    s += " " + c.HTML();
                }

            }


            if (b.Likely == BranchUnlikely) 
                s += " (unlikely)";
            else if (b.Likely == BranchLikely) 
                s += " (likely)";
                        if (b.Pos.IsKnown())
            { 
                // TODO does not begin to deal with the full complexity of line numbers.
                // Maybe we want a string/slice instead, of outer-inner when inlining.
                s += fmt.Sprintf(" <span class=\"l%v line-number\">(%s)</span>", b.Pos.LineNumber(), b.Pos.LineNumberHTML());

            }

            return s;

        }

        private static @string HTML(this ptr<Func> _addr_f, @string phase, ptr<dotWriter> _addr_dot)
        {
            ref Func f = ref _addr_f.val;
            ref dotWriter dot = ref _addr_dot.val;

            ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
            if (dot != null)
            {
                dot.writeFuncSVG(buf, phase, f);
            }

            fmt.Fprint(buf, "<code>");
            htmlFuncPrinter p = new htmlFuncPrinter(w:buf);
            fprintFunc(p, f); 

            // fprintFunc(&buf, f) // TODO: HTML, not text, <br /> for line breaks, etc.
            fmt.Fprint(buf, "</code>");
            return buf.String();

        }

        private static void writeFuncSVG(this ptr<dotWriter> _addr_d, io.Writer w, @string phase, ptr<Func> _addr_f)
        {
            ref dotWriter d = ref _addr_d.val;
            ref Func f = ref _addr_f.val;

            if (d.broken)
            {
                return ;
            }

            {
                var (_, ok) = d.phases[phase];

                if (!ok)
                {
                    return ;
                }

            }

            var cmd = exec.Command(d.path, "-Tsvg");
            var (pipe, err) = cmd.StdinPipe();
            if (err != null)
            {
                d.broken = true;
                fmt.Println(err);
                return ;
            }

            ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
            cmd.Stdout = buf;
            ptr<bytes.Buffer> bufErr = @new<bytes.Buffer>();
            cmd.Stderr = bufErr;
            err = cmd.Start();
            if (err != null)
            {
                d.broken = true;
                fmt.Println(err);
                return ;
            }

            fmt.Fprint(pipe, "digraph \"\" { margin=0; ranksep=.2; ");
            var id = strings.Replace(phase, " ", "-", -1L);
            fmt.Fprintf(pipe, "id=\"g_graph_%s\";", id);
            fmt.Fprintf(pipe, "node [style=filled,fillcolor=white,fontsize=16,fontname=\"Menlo,Times,serif\",margi" +
    "n=\"0.01,0.03\"];");
            fmt.Fprintf(pipe, "edge [fontsize=16,fontname=\"Menlo,Times,serif\"];");
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    if (b.Kind == BlockInvalid)
                    {
                        continue;
                    }

                    @string layout = "";
                    if (f.laidout)
                    {
                        layout = fmt.Sprintf(" #%d", i);
                    }

                    fmt.Fprintf(pipe, "%v [label=\"%v%s\\n%v\",id=\"graph_node_%v_%v\",tooltip=\"%v\"];", b, b, layout, b.Kind.String(), id, b, b.LongString());

                }

                i = i__prev1;
                b = b__prev1;
            }

            var indexOf = make_slice<long>(f.NumBlocks());
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    indexOf[b.ID] = i;
                }

                i = i__prev1;
                b = b__prev1;
            }

            var layoutDrawn = make_slice<bool>(f.NumBlocks());

            var ponums = make_slice<int>(f.NumBlocks());
            _ = postorderWithNumbering(f, ponums);
            Func<ID, ID, bool> isBackEdge = (from, to) =>
            {
                return ponums[from] <= ponums[to];
            }
;

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __s) in b.Succs)
                        {
                            i = __i;
                            s = __s;
                            @string style = "solid";
                            @string color = "black";
                            @string arrow = "vee";
                            if (b.unlikelyIndex() == i)
                            {
                                style = "dashed";
                            }

                            if (f.laidout && indexOf[s.b.ID] == indexOf[b.ID] + 1L)
                            { 
                                // Red color means ordered edge. It overrides other colors.
                                arrow = "dotvee";
                                layoutDrawn[s.b.ID] = true;

                            }
                            else if (isBackEdge(b.ID, s.b.ID))
                            {
                                color = "#2893ff";
                            }

                            fmt.Fprintf(pipe, "%v -> %v [label=\" %d \",style=\"%s\",color=\"%s\",arrowhead=\"%s\"];", b, s.b, i, style, color, arrow);

                        }

                        i = i__prev2;
                    }
                }

                b = b__prev1;
            }

            if (f.laidout)
            {
                fmt.Fprintln(pipe, "edge[constraint=false,color=gray,style=solid,arrowhead=dot];");
                array<@string> colors = new array<@string>(new @string[] { "#eea24f", "#f38385", "#f4d164", "#ca89fc", "gray" });
                long ci = 0L;
                {
                    var i__prev1 = i;

                    for (long i = 1L; i < len(f.Blocks); i++)
                    {
                        if (layoutDrawn[f.Blocks[i].ID])
                        {
                            continue;
                        }

                        fmt.Fprintf(pipe, "%s -> %s [color=\"%s\"];", f.Blocks[i - 1L], f.Blocks[i], colors[ci]);
                        ci = (ci + 1L) % len(colors);

                    }


                    i = i__prev1;
                }

            }

            fmt.Fprint(pipe, "}");
            pipe.Close();
            err = cmd.Wait();
            if (err != null)
            {
                d.broken = true;
                fmt.Printf("dot: %v\n%v\n", err, bufErr.String());
                return ;
            }

            @string svgID = "svg_graph_" + id;
            fmt.Fprintf(w, "<div class=\"zoom\"><button onclick=\"return graphReduce(\'%s\');\">-</button> <button " +
    "onclick=\"return graphEnlarge(\'%s\');\">+</button></div>", svgID, svgID); 
            // For now, an awful hack: edit the html as it passes through
            // our fingers, finding '<svg ' and injecting needed attributes after it.
            err = d.copyUntil(w, buf, "<svg ");
            if (err != null)
            {
                fmt.Printf("injecting attributes: %v\n", err);
                return ;
            }

            fmt.Fprintf(w, " id=\"%s\" onload=\"makeDraggable(evt)\" ", svgID);
            io.Copy(w, buf);

        }

        private static long unlikelyIndex(this ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;


            if (b.Likely == BranchLikely) 
                return 1L;
            else if (b.Likely == BranchUnlikely) 
                return 0L;
                        return -1L;

        }

        private static error copyUntil(this ptr<dotWriter> _addr_d, io.Writer w, ptr<bytes.Buffer> _addr_buf, @string sep)
        {
            ref dotWriter d = ref _addr_d.val;
            ref bytes.Buffer buf = ref _addr_buf.val;

            var i = bytes.Index(buf.Bytes(), (slice<byte>)sep);
            if (i == -1L)
            {
                return error.As(fmt.Errorf("couldn't find dot sep %q", sep))!;
            }

            var (_, err) = io.CopyN(w, buf, int64(i + len(sep)));
            return error.As(err)!;

        }

        private partial struct htmlFuncPrinter
        {
            public io.Writer w;
        }

        private static void header(this htmlFuncPrinter p, ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

        }

        private static void startBlock(this htmlFuncPrinter p, ptr<Block> _addr_b, bool reachable)
        {
            ref Block b = ref _addr_b.val;

            @string dead = default;
            if (!reachable)
            {
                dead = "dead-block";
            }

            fmt.Fprintf(p.w, "<ul class=\"%s ssa-print-func %s\">", b, dead);
            fmt.Fprintf(p.w, "<li class=\"ssa-start-block\">%s:", b.HTML());
            if (len(b.Preds) > 0L)
            {
                io.WriteString(p.w, " &#8592;"); // left arrow
                foreach (var (_, e) in b.Preds)
                {
                    var pred = e.b;
                    fmt.Fprintf(p.w, " %s", pred.HTML());
                }

            }

            if (len(b.Values) > 0L)
            {
                io.WriteString(p.w, "<button onclick=\"hideBlock(this)\">-</button>");
            }

            io.WriteString(p.w, "</li>");
            if (len(b.Values) > 0L)
            { // start list of values
                io.WriteString(p.w, "<li class=\"ssa-value-list\">");
                io.WriteString(p.w, "<ul>");

            }

        }

        private static void endBlock(this htmlFuncPrinter p, ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            if (len(b.Values) > 0L)
            { // end list of values
                io.WriteString(p.w, "</ul>");
                io.WriteString(p.w, "</li>");

            }

            io.WriteString(p.w, "<li class=\"ssa-end-block\">");
            fmt.Fprint(p.w, b.LongHTML());
            io.WriteString(p.w, "</li>");
            io.WriteString(p.w, "</ul>");

        }

        private static void value(this htmlFuncPrinter p, ptr<Value> _addr_v, bool live)
        {
            ref Value v = ref _addr_v.val;

            @string dead = default;
            if (!live)
            {
                dead = "dead-value";
            }

            fmt.Fprintf(p.w, "<li class=\"ssa-long-value %s\">", dead);
            fmt.Fprint(p.w, v.LongHTML());
            io.WriteString(p.w, "</li>");

        }

        private static void startDepCycle(this htmlFuncPrinter p)
        {
            fmt.Fprintln(p.w, "<span class=\"depcycle\">");
        }

        private static void endDepCycle(this htmlFuncPrinter p)
        {
            fmt.Fprintln(p.w, "</span>");
        }

        private static void named(this htmlFuncPrinter p, LocalSlot n, slice<ptr<Value>> vals)
        {
            fmt.Fprintf(p.w, "<li>name %s: ", n);
            foreach (var (_, val) in vals)
            {
                fmt.Fprintf(p.w, "%s ", val.HTML());
            }
            fmt.Fprintf(p.w, "</li>");

        }

        private partial struct dotWriter
        {
            public @string path;
            public bool broken;
            public map<@string, bool> phases; // keys specify phases with CFGs
        }

        // newDotWriter returns non-nil value when mask is valid.
        // dotWriter will generate SVGs only for the phases specified in the mask.
        // mask can contain following patterns and combinations of them:
        // *   - all of them;
        // x-y - x through y, inclusive;
        // x,y - x and y, but not the passes between.
        private static ptr<dotWriter> newDotWriter(@string mask)
        {
            if (mask == "")
            {
                return _addr_null!;
            } 
            // User can specify phase name with _ instead of spaces.
            mask = strings.Replace(mask, "_", " ", -1L);
            var ph = make_map<@string, bool>();
            var ranges = strings.Split(mask, ",");
            foreach (var (_, r) in ranges)
            {
                var spl = strings.Split(r, "-");
                if (len(spl) > 2L)
                {
                    fmt.Printf("range is not valid: %v\n", mask);
                    return _addr_null!;
                }

                long first = default;                long last = default;

                if (mask == "*")
                {
                    first = 0L;
                    last = len(passes) - 1L;
                }
                else
                {
                    first = passIdxByName(spl[0L]);
                    last = passIdxByName(spl[len(spl) - 1L]);
                }

                if (first < 0L || last < 0L || first > last)
                {
                    fmt.Printf("range is not valid: %v\n", r);
                    return _addr_null!;
                }

                for (var p = first; p <= last; p++)
                {
                    ph[passes[p].name] = true;
                }


            }
            var (path, err) = exec.LookPath("dot");
            if (err != null)
            {
                fmt.Println(err);
                return _addr_null!;
            }

            return addr(new dotWriter(path:path,phases:ph));

        }

        private static long passIdxByName(@string name)
        {
            foreach (var (i, p) in passes)
            {
                if (p.name == name)
                {
                    return i;
                }

            }
            return -1L;

        }
    }
}}}}
