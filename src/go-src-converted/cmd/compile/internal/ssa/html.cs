// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:30 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\html.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using html = html_package;
using exec = @internal.execabs_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;
using System;

public static partial class ssa_package {

public partial struct HTMLWriter {
    public io.WriteCloser w;
    public ptr<Func> Func;
    public @string path;
    public ptr<dotWriter> dot;
    public slice<byte> prevHash;
    public slice<@string> pendingPhases;
    public slice<@string> pendingTitles;
}

public static ptr<HTMLWriter> NewHTMLWriter(@string path, ptr<Func> _addr_f, @string cfgMask) {
    ref Func f = ref _addr_f.val;

    path = strings.Replace(path, "/", string(filepath.Separator), -1);
    var (out, err) = os.OpenFile(path, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0644);
    if (err != null) {
        f.Fatalf("%v", err);
    }
    var reportPath = path;
    if (!filepath.IsAbs(reportPath)) {
        var (pwd, err) = os.Getwd();
        if (err != null) {
            f.Fatalf("%v", err);
        }
        reportPath = filepath.Join(pwd, path);
    }
    ref HTMLWriter html = ref heap(new HTMLWriter(w:out,Func:f,path:reportPath,dot:newDotWriter(cfgMask),), out ptr<HTMLWriter> _addr_html);
    html.start();
    return _addr__addr_html!;
}

// Fatalf reports an error and exits.
private static void Fatalf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] args) {
    args = args.Clone();
    ref HTMLWriter w = ref _addr_w.val;

    var fe = w.Func.Frontend();
    fe.Fatalf(src.NoXPos, msg, args);
}

// Logf calls the (w *HTMLWriter).Func's Logf method passing along a msg and args.
private static void Logf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] args) {
    args = args.Clone();
    ref HTMLWriter w = ref _addr_w.val;

    w.Func.Logf(msg, args);
}

private static void start(this ptr<HTMLWriter> _addr_w) {
    ref HTMLWriter w = ref _addr_w.val;

    if (w == null) {
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
    " 2px;\n    cursor: pointer;\n    background: #fafafa;\n}\n\ntd.collapsed div {\n    te" +
    "xt-align: right;\n    transform: rotate(180deg);\n    writing-mode: vertical-lr;\n " +
    "   white-space: pre;\n}\n\ncode, pre, .lines, .ast {\n    font-family: Menlo, monosp" +
    "ace;\n    font-size: 12px;\n}\n\npre {\n    -moz-tab-size: 4;\n    -o-tab-size:   4;\n " +
    "   tab-size:      4;\n}\n\n.allow-x-scroll {\n    overflow-x: scroll;\n}\n\n.lines {\n  " +
    "  float: left;\n    overflow: hidden;\n    text-align: right;\n    margin-top: 7px;" +
    "\n}\n\n.lines div {\n    padding-right: 10px;\n    color: gray;\n}\n\ndiv.line-number {\n" +
    "    font-size: 12px;\n}\n\n.ast {\n    white-space: nowrap;\n}\n\ntd.ssa-prog {\n    wid" +
    "th: 600px;\n    word-wrap: break-word;\n}\n\nli {\n    list-style-type: none;\n}\n\nli.s" +
    "sa-long-value {\n    text-indent: -2em;  /* indent wrapped lines */\n}\n\nli.ssa-val" +
    "ue-list {\n    display: inline;\n}\n\nli.ssa-start-block {\n    padding: 0;\n    margi" +
    "n: 0;\n}\n\nli.ssa-end-block {\n    padding: 0;\n    margin: 0;\n}\n\nul.ssa-print-func " +
    "{\n    padding-left: 0;\n}\n\nli.ssa-start-block button {\n    padding: 0 1em;\n    ma" +
    "rgin: 0;\n    border: none;\n    display: inline;\n    font-size: 14px;\n    float: " +
    "right;\n}\n\nbutton:hover {\n    background-color: #eee;\n    cursor: pointer;\n}\n\ndl." +
    "ssa-gen {\n    padding-left: 0;\n}\n\ndt.ssa-prog-src {\n    padding: 0;\n    margin: " +
    "0;\n    float: left;\n    width: 4em;\n}\n\ndd.ssa-prog {\n    padding: 0;\n    margin-" +
    "right: 0;\n    margin-left: 4em;\n}\n\n.dead-value {\n    color: gray;\n}\n\n.dead-block" +
    " {\n    opacity: 0.5;\n}\n\n.depcycle {\n    font-style: italic;\n}\n\n.line-number {\n  " +
    "  font-size: 11px;\n}\n\n.no-line-number {\n    font-size: 11px;\n    color: gray;\n}\n" +
    "\n.zoom {\n\tposition: absolute;\n\tfloat: left;\n\twhite-space: nowrap;\n\tbackground-co" +
    "lor: #eee;\n}\n\n.zoom a:link, .zoom a:visited  {\n    text-decoration: none;\n    co" +
    "lor: blue;\n    font-size: 16px;\n    padding: 4px 2px;\n}\n\nsvg {\n    cursor: defau" +
    "lt;\n    outline: 1px solid #eee;\n    width: 100%;\n}\n\nbody.darkmode {\n    backgro" +
    "und-color: rgb(21, 21, 21);\n    color: rgb(230, 255, 255);\n    opacity: 100%;\n}\n" +
    "\ntd.darkmode {\n    background-color: rgb(21, 21, 21);\n    border: 1px solid gray" +
    ";\n}\n\nbody.darkmode table, th {\n    border: 1px solid gray;\n}\n\nbody.darkmode text" +
    " {\n    fill: white;\n}\n\nbody.darkmode svg polygon:first-child {\n    fill: rgb(21," +
    " 21, 21);\n}\n\n.highlight-aquamarine     { background-color: aquamarine; color: bl" +
    "ack; }\n.highlight-coral          { background-color: coral; color: black; }\n.hig" +
    "hlight-lightpink      { background-color: lightpink; color: black; }\n.highlight-" +
    "lightsteelblue { background-color: lightsteelblue; color: black; }\n.highlight-pa" +
    "legreen      { background-color: palegreen; color: black; }\n.highlight-skyblue  " +
    "      { background-color: skyblue; color: black; }\n.highlight-lightgray      { b" +
    "ackground-color: lightgray; color: black; }\n.highlight-yellow         { backgrou" +
    "nd-color: yellow; color: black; }\n.highlight-lime           { background-color: " +
    "lime; color: black; }\n.highlight-khaki          { background-color: khaki; color" +
    ": black; }\n.highlight-aqua           { background-color: aqua; color: black; }\n." +
    "highlight-salmon         { background-color: salmon; color: black; }\n\n/* Ensure " +
    "all dead values/blocks continue to have gray font color in dark mode with highli" +
    "ghts */\n.dead-value span.highlight-aquamarine,\n.dead-block.highlight-aquamarine," +
    "\n.dead-value span.highlight-coral,\n.dead-block.highlight-coral,\n.dead-value span" +
    ".highlight-lightpink,\n.dead-block.highlight-lightpink,\n.dead-value span.highligh" +
    "t-lightsteelblue,\n.dead-block.highlight-lightsteelblue,\n.dead-value span.highlig" +
    "ht-palegreen,\n.dead-block.highlight-palegreen,\n.dead-value span.highlight-skyblu" +
    "e,\n.dead-block.highlight-skyblue,\n.dead-value span.highlight-lightgray,\n.dead-bl" +
    "ock.highlight-lightgray,\n.dead-value span.highlight-yellow,\n.dead-block.highligh" +
    "t-yellow,\n.dead-value span.highlight-lime,\n.dead-block.highlight-lime,\n.dead-val" +
    "ue span.highlight-khaki,\n.dead-block.highlight-khaki,\n.dead-value span.highlight" +
    "-aqua,\n.dead-block.highlight-aqua,\n.dead-value span.highlight-salmon,\n.dead-bloc" +
    "k.highlight-salmon {\n    color: gray;\n}\n\n.outline-blue           { outline: #289" +
    "3ff solid 2px; }\n.outline-red            { outline: red solid 2px; }\n.outline-bl" +
    "ueviolet     { outline: blueviolet solid 2px; }\n.outline-darkolivegreen { outlin" +
    "e: darkolivegreen solid 2px; }\n.outline-fuchsia        { outline: fuchsia solid " +
    "2px; }\n.outline-sienna         { outline: sienna solid 2px; }\n.outline-gold     " +
    "      { outline: gold solid 2px; }\n.outline-orangered      { outline: orangered " +
    "solid 2px; }\n.outline-teal           { outline: teal solid 2px; }\n.outline-maroo" +
    "n         { outline: maroon solid 2px; }\n.outline-black          { outline: blac" +
    "k solid 2px; }\n\nellipse.outline-blue           { stroke-width: 2px; stroke: #289" +
    "3ff; }\nellipse.outline-red            { stroke-width: 2px; stroke: red; }\nellips" +
    "e.outline-blueviolet     { stroke-width: 2px; stroke: blueviolet; }\nellipse.outl" +
    "ine-darkolivegreen { stroke-width: 2px; stroke: darkolivegreen; }\nellipse.outlin" +
    "e-fuchsia        { stroke-width: 2px; stroke: fuchsia; }\nellipse.outline-sienna " +
    "        { stroke-width: 2px; stroke: sienna; }\nellipse.outline-gold           { " +
    "stroke-width: 2px; stroke: gold; }\nellipse.outline-orangered      { stroke-width" +
    ": 2px; stroke: orangered; }\nellipse.outline-teal           { stroke-width: 2px; " +
    "stroke: teal; }\nellipse.outline-maroon         { stroke-width: 2px; stroke: maro" +
    "on; }\nellipse.outline-black          { stroke-width: 2px; stroke: black; }\n\n/* C" +
    "apture alternative for outline-black and ellipse.outline-black when in dark mode" +
    " */\nbody.darkmode .outline-black        { outline: gray solid 2px; }\nbody.darkmo" +
    "de ellipse.outline-black { outline: gray solid 2px; }\n\n</style>\n\n<script type=\"t" +
    "ext/javascript\">\n\n// Contains phase names which are expanded by default. Other c" +
    "olumns are collapsed.\nlet expandedDefault = [\n    \"start\",\n    \"deadcode\",\n    \"" +
    "opt\",\n    \"lower\",\n    \"late-deadcode\",\n    \"regalloc\",\n    \"genssa\",\n];\nif (his" +
    "tory.state === null) {\n    history.pushState({expandedDefault}, \"\", location.hre" +
    "f);\n}\n\n// ordered list of all available highlight colors\nvar highlights = [\n    " +
    "\"highlight-aquamarine\",\n    \"highlight-coral\",\n    \"highlight-lightpink\",\n    \"h" +
    "ighlight-lightsteelblue\",\n    \"highlight-palegreen\",\n    \"highlight-skyblue\",\n  " +
    "  \"highlight-lightgray\",\n    \"highlight-yellow\",\n    \"highlight-lime\",\n    \"high" +
    "light-khaki\",\n    \"highlight-aqua\",\n    \"highlight-salmon\"\n];\n\n// state: which v" +
    "alue is highlighted this color?\nvar highlighted = {};\nfor (var i = 0; i < highli" +
    "ghts.length; i++) {\n    highlighted[highlights[i]] = \"\";\n}\n\n// ordered list of a" +
    "ll available outline colors\nvar outlines = [\n    \"outline-blue\",\n    \"outline-re" +
    "d\",\n    \"outline-blueviolet\",\n    \"outline-darkolivegreen\",\n    \"outline-fuchsia" +
    "\",\n    \"outline-sienna\",\n    \"outline-gold\",\n    \"outline-orangered\",\n    \"outli" +
    "ne-teal\",\n    \"outline-maroon\",\n    \"outline-black\"\n];\n\n// state: which value is" +
    " outlined this color?\nvar outlined = {};\nfor (var i = 0; i < outlines.length; i+" +
    "+) {\n    outlined[outlines[i]] = \"\";\n}\n\nwindow.onload = function() {\n    if (his" +
    "tory.state !== null) {\n        expandedDefault = history.state.expandedDefault;\n" +
    "    }\n    if (window.matchMedia && window.matchMedia(\"(prefers-color-scheme: dar" +
    "k)\").matches) {\n        toggleDarkMode();\n        document.getElementById(\"dark-" +
    "mode-button\").checked = true;\n    }\n\n    var ssaElemClicked = function(elem, eve" +
    "nt, selections, selected) {\n        event.stopPropagation();\n\n        // find al" +
    "l values with the same name\n        var c = elem.classList.item(0);\n        var " +
    "x = document.getElementsByClassName(c);\n\n        // if selected, remove selectio" +
    "ns from all of them\n        // otherwise, attempt to add\n\n        var remove = \"" +
    "\";\n        for (var i = 0; i < selections.length; i++) {\n            var color =" +
    " selections[i];\n            if (selected[color] == c) {\n                remove =" +
    " color;\n                break;\n            }\n        }\n\n        if (remove != \"\"" +
    ") {\n            for (var i = 0; i < x.length; i++) {\n                x[i].classL" +
    "ist.remove(remove);\n            }\n            selected[remove] = \"\";\n           " +
    " return;\n        }\n\n        // we\'re adding a selection\n        // find first av" +
    "ailable color\n        var avail = \"\";\n        for (var i = 0; i < selections.len" +
    "gth; i++) {\n            var color = selections[i];\n            if (selected[colo" +
    "r] == \"\") {\n                avail = color;\n                break;\n            }\n" +
    "        }\n        if (avail == \"\") {\n            alert(\"out of selection colors;" +
    " go add more\");\n            return;\n        }\n\n        // set that as the select" +
    "ion\n        for (var i = 0; i < x.length; i++) {\n            x[i].classList.add(" +
    "avail);\n        }\n        selected[avail] = c;\n    };\n\n    var ssaValueClicked =" +
    " function(event) {\n        ssaElemClicked(this, event, highlights, highlighted);" +
    "\n    };\n\n    var ssaBlockClicked = function(event) {\n        ssaElemClicked(this" +
    ", event, outlines, outlined);\n    };\n\n    var ssavalues = document.getElementsBy" +
    "ClassName(\"ssa-value\");\n    for (var i = 0; i < ssavalues.length; i++) {\n       " +
    " ssavalues[i].addEventListener(\'click\', ssaValueClicked);\n    }\n\n    var ssalong" +
    "values = document.getElementsByClassName(\"ssa-long-value\");\n    for (var i = 0; " +
    "i < ssalongvalues.length; i++) {\n        // don\'t attach listeners to li nodes, " +
    "just the spans they contain\n        if (ssalongvalues[i].nodeName == \"SPAN\") {\n " +
    "           ssalongvalues[i].addEventListener(\'click\', ssaValueClicked);\n        " +
    "}\n    }\n\n    var ssablocks = document.getElementsByClassName(\"ssa-block\");\n    f" +
    "or (var i = 0; i < ssablocks.length; i++) {\n        ssablocks[i].addEventListene" +
    "r(\'click\', ssaBlockClicked);\n    }\n\n    var lines = document.getElementsByClassN" +
    "ame(\"line-number\");\n    for (var i = 0; i < lines.length; i++) {\n        lines[i" +
    "].addEventListener(\'click\', ssaValueClicked);\n    }\n\n\n    function toggler(phase" +
    ") {\n        return function() {\n            toggle_cell(phase+\'-col\');\n         " +
    "   toggle_cell(phase+\'-exp\');\n            const i = expandedDefault.indexOf(phas" +
    "e);\n            if (i !== -1) {\n                expandedDefault.splice(i, 1);\n  " +
    "          } else {\n                expandedDefault.push(phase);\n            }\n  " +
    "          history.pushState({expandedDefault}, \"\", location.href);\n        };\n  " +
    "  }\n\n    function toggle_cell(id) {\n        var e = document.getElementById(id);" +
    "\n        if (e.style.display == \'table-cell\') {\n            e.style.display = \'n" +
    "one\';\n        } else {\n            e.style.display = \'table-cell\';\n        }\n   " +
    " }\n\n    // Go through all columns and collapse needed phases.\n    const td = doc" +
    "ument.getElementsByTagName(\"td\");\n    for (let i = 0; i < td.length; i++) {\n    " +
    "    const id = td[i].id;\n        const phase = id.substr(0, id.length-4);\n      " +
    "  let show = expandedDefault.indexOf(phase) !== -1\n\n        // If show == false," +
    " check to see if this is a combined column (multiple phases).\n        // If comb" +
    "ined, check each of the phases to see if they are in our expandedDefaults.\n     " +
    "   // If any are found, that entire combined column gets shown.\n        if (!sho" +
    "w) {\n            const combined = phase.split(\'--+--\');\n            const len = " +
    "combined.length;\n            if (len > 1) {\n                for (let i = 0; i < " +
    "len; i++) {\n                    const num = expandedDefault.indexOf(combined[i])" +
    ";\n                    if (num !== -1) {\n                        expandedDefault." +
    "splice(num, 1);\n                        if (expandedDefault.indexOf(phase) === -" +
    "1) {\n                            expandedDefault.push(phase);\n                  " +
    "          show = true;\n                        }\n                    }\n         " +
    "       }\n            }\n        }\n        if (id.endsWith(\"-exp\")) {\n            " +
    "const h2Els = td[i].getElementsByTagName(\"h2\");\n            const len = h2Els.le" +
    "ngth;\n            if (len > 0) {\n                for (let i = 0; i < len; i++) {" +
    "\n                    h2Els[i].addEventListener(\'click\', toggler(phase));\n       " +
    "         }\n            }\n        } else {\n            td[i].addEventListener(\'cl" +
    "ick\', toggler(phase));\n        }\n        if (id.endsWith(\"-col\") && show || id.e" +
    "ndsWith(\"-exp\") && !show) {\n            td[i].style.display = \'none\';\n          " +
    "  continue;\n        }\n        td[i].style.display = \'table-cell\';\n    }\n\n    // " +
    "find all svg block nodes, add their block classes\n    var nodes = document.query" +
    "SelectorAll(\'*[id^=\"graph_node_\"]\');\n    for (var i = 0; i < nodes.length; i++) " +
    "{\n    \tvar node = nodes[i];\n    \tvar name = node.id.toString();\n    \tvar block =" +
    " name.substring(name.lastIndexOf(\"_\")+1);\n    \tnode.classList.remove(\"node\");\n  " +
    "  \tnode.classList.add(block);\n        node.addEventListener(\'click\', ssaBlockCli" +
    "cked);\n        var ellipse = node.getElementsByTagName(\'ellipse\')[0];\n        el" +
    "lipse.classList.add(block);\n        ellipse.addEventListener(\'click\', ssaBlockCl" +
    "icked);\n    }\n\n    // make big graphs smaller\n    var targetScale = 0.5;\n    var" +
    " nodes = document.querySelectorAll(\'*[id^=\"svg_graph_\"]\');\n    // TODO: Implemen" +
    "t smarter auto-zoom using the viewBox attribute\n    // and in case of big graphs" +
    " set the width and height of the svg graph to\n    // maximum allowed.\n    for (v" +
    "ar i = 0; i < nodes.length; i++) {\n    \tvar node = nodes[i];\n    \tvar name = nod" +
    "e.id.toString();\n    \tvar phase = name.substring(name.lastIndexOf(\"_\")+1);\n    \t" +
    "var gNode = document.getElementById(\"g_graph_\"+phase);\n    \tvar scale = gNode.tr" +
    "ansform.baseVal.getItem(0).matrix.a;\n    \tif (scale > targetScale) {\n    \t\tnode." +
    "width.baseVal.value *= targetScale / scale;\n    \t\tnode.height.baseVal.value *= t" +
    "argetScale / scale;\n    \t}\n    }\n};\n\nfunction toggle_visibility(id) {\n    var e " +
    "= document.getElementById(id);\n    if (e.style.display == \'block\') {\n        e.s" +
    "tyle.display = \'none\';\n    } else {\n        e.style.display = \'block\';\n    }\n}\n\n" +
    "function hideBlock(el) {\n    var es = el.parentNode.parentNode.getElementsByClas" +
    "sName(\"ssa-value-list\");\n    if (es.length===0)\n        return;\n    var e = es[0" +
    "];\n    if (e.style.display === \'block\' || e.style.display === \'\') {\n        e.st" +
    "yle.display = \'none\';\n        el.innerHTML = \'+\';\n    } else {\n        e.style.d" +
    "isplay = \'block\';\n        el.innerHTML = \'-\';\n    }\n}\n\n// TODO: scale the graph " +
    "with the viewBox attribute.\nfunction graphReduce(id) {\n    var node = document.g" +
    "etElementById(id);\n    if (node) {\n    \t\tnode.width.baseVal.value *= 0.9;\n    \t\t" +
    "node.height.baseVal.value *= 0.9;\n    }\n    return false;\n}\n\nfunction graphEnlar" +
    "ge(id) {\n    var node = document.getElementById(id);\n    if (node) {\n    \t\tnode." +
    "width.baseVal.value *= 1.1;\n    \t\tnode.height.baseVal.value *= 1.1;\n    }\n    re" +
    "turn false;\n}\n\nfunction makeDraggable(event) {\n    var svg = event.target;\n    i" +
    "f (window.PointerEvent) {\n        svg.addEventListener(\'pointerdown\', startDrag)" +
    ";\n        svg.addEventListener(\'pointermove\', drag);\n        svg.addEventListene" +
    "r(\'pointerup\', endDrag);\n        svg.addEventListener(\'pointerleave\', endDrag);\n" +
    "    } else {\n        svg.addEventListener(\'mousedown\', startDrag);\n        svg.a" +
    "ddEventListener(\'mousemove\', drag);\n        svg.addEventListener(\'mouseup\', endD" +
    "rag);\n        svg.addEventListener(\'mouseleave\', endDrag);\n    }\n\n    var point " +
    "= svg.createSVGPoint();\n    var isPointerDown = false;\n    var pointerOrigin;\n  " +
    "  var viewBox = svg.viewBox.baseVal;\n\n    function getPointFromEvent (event) {\n " +
    "       point.x = event.clientX;\n        point.y = event.clientY;\n\n        // We " +
    "get the current transformation matrix of the SVG and we inverse it\n        var i" +
    "nvertedSVGMatrix = svg.getScreenCTM().inverse();\n        return point.matrixTran" +
    "sform(invertedSVGMatrix);\n    }\n\n    function startDrag(event) {\n        isPoint" +
    "erDown = true;\n        pointerOrigin = getPointFromEvent(event);\n    }\n\n    func" +
    "tion drag(event) {\n        if (!isPointerDown) {\n            return;\n        }\n " +
    "       event.preventDefault();\n\n        var pointerPosition = getPointFromEvent(" +
    "event);\n        viewBox.x -= (pointerPosition.x - pointerOrigin.x);\n        view" +
    "Box.y -= (pointerPosition.y - pointerOrigin.y);\n    }\n\n    function endDrag(even" +
    "t) {\n        isPointerDown = false;\n    }\n}\n\nfunction toggleDarkMode() {\n    doc" +
    "ument.body.classList.toggle(\'darkmode\');\n\n    // Collect all of the \"collapsed\" " +
    "elements and apply dark mode on each collapsed column\n    const collapsedEls = d" +
    "ocument.getElementsByClassName(\'collapsed\');\n    const len = collapsedEls.length" +
    ";\n\n    for (let i = 0; i < len; i++) {\n        collapsedEls[i].classList.toggle(" +
    "\'darkmode\');\n    }\n\n    // Collect and spread the appropriate elements from all " +
    "of the svgs on the page into one array\n    const svgParts = [\n        ...documen" +
    "t.querySelectorAll(\'path\'),\n        ...document.querySelectorAll(\'ellipse\'),\n   " +
    "     ...document.querySelectorAll(\'polygon\'),\n    ];\n\n    // Iterate over the sv" +
    "gParts specifically looking for white and black fill/stroke to be toggled.\n    /" +
    "/ The verbose conditional is intentional here so that we do not mutate any svg p" +
    "ath, ellipse, or polygon that is of any color other than white or black.\n    svg" +
    "Parts.forEach(el => {\n        if (el.attributes.stroke.value === \'white\') {\n    " +
    "        el.attributes.stroke.value = \'black\';\n        } else if (el.attributes.s" +
    "troke.value === \'black\') {\n            el.attributes.stroke.value = \'white\';\n   " +
    "     }\n        if (el.attributes.fill.value === \'white\') {\n            el.attrib" +
    "utes.fill.value = \'black\';\n        } else if (el.attributes.fill.value === \'blac" +
    "k\') {\n            el.attributes.fill.value = \'white\';\n        }\n    });\n}\n\n</scr" +
    "ipt>\n\n</head>");
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

private static void Close(this ptr<HTMLWriter> _addr_w) {
    ref HTMLWriter w = ref _addr_w.val;

    if (w == null) {
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
private static void WritePhase(this ptr<HTMLWriter> _addr_w, @string phase, @string title) {
    ref HTMLWriter w = ref _addr_w.val;

    if (w == null) {
        return ; // avoid generating HTML just to discard it
    }
    var hash = hashFunc(w.Func);
    w.pendingPhases = append(w.pendingPhases, phase);
    w.pendingTitles = append(w.pendingTitles, title);
    if (!bytes.Equal(hash, w.prevHash)) {
        w.flushPhases();
    }
    w.prevHash = hash;
}

// flushPhases collects any pending phases and titles, writes them to the html, and resets the pending slices.
private static void flushPhases(this ptr<HTMLWriter> _addr_w) {
    ref HTMLWriter w = ref _addr_w.val;

    var phaseLen = len(w.pendingPhases);
    if (phaseLen == 0) {
        return ;
    }
    var phases = strings.Join(w.pendingPhases, "  +  ");
    w.WriteMultiTitleColumn(phases, w.pendingTitles, fmt.Sprintf("hash-%x", w.prevHash), w.Func.HTML(w.pendingPhases[phaseLen - 1], w.dot));
    w.pendingPhases = w.pendingPhases[..(int)0];
    w.pendingTitles = w.pendingTitles[..(int)0];
}

// FuncLines contains source code for a function to be displayed
// in sources column.
public partial struct FuncLines {
    public @string Filename;
    public nuint StartLineno;
    public slice<@string> Lines;
}

// ByTopo sorts topologically: target function is on top,
// followed by inlined functions sorted by filename and line numbers.
public partial struct ByTopo { // : slice<ptr<FuncLines>>
}

public static nint Len(this ByTopo x) {
    return len(x);
}
public static void Swap(this ByTopo x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}
public static bool Less(this ByTopo x, nint i, nint j) {
    var a = x[i];
    var b = x[j];
    if (a.Filename == b.Filename) {
        return a.StartLineno < b.StartLineno;
    }
    return a.Filename < b.Filename;
}

// WriteSources writes lines as source code in a column headed by title.
// phase is used for collapsing columns and should be unique across the table.
private static void WriteSources(this ptr<HTMLWriter> _addr_w, @string phase, slice<ptr<FuncLines>> all) {
    ref HTMLWriter w = ref _addr_w.val;

    if (w == null) {
        return ; // avoid generating HTML just to discard it
    }
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    fmt.Fprint(_addr_buf, "<div class=\"lines\" style=\"width: 8%\">");
    @string filename = "";
    {
        var fl__prev1 = fl;

        foreach (var (_, __fl) in all) {
            fl = __fl;
            fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
            if (filename != fl.Filename) {
                fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
                filename = fl.Filename;
            }
            {
                var i__prev2 = i;

                foreach (var (__i) in fl.Lines) {
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

        foreach (var (_, __fl) in all) {
            fl = __fl;
            fmt.Fprint(_addr_buf, "<div>&nbsp;</div>");
            if (filename != fl.Filename) {
                fmt.Fprintf(_addr_buf, "<div><strong>%v</strong></div>", fl.Filename);
                filename = fl.Filename;
            }
            {
                var i__prev2 = i;

                foreach (var (__i, __line) in fl.Lines) {
                    i = __i;
                    line = __line;
                    ln = int(fl.StartLineno) + i;
                    @string escaped = default;
                    if (strings.TrimSpace(line) == "") {
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

private static void WriteAST(this ptr<HTMLWriter> _addr_w, @string phase, ptr<bytes.Buffer> _addr_buf) {
    ref HTMLWriter w = ref _addr_w.val;
    ref bytes.Buffer buf = ref _addr_buf.val;

    if (w == null) {
        return ; // avoid generating HTML just to discard it
    }
    var lines = strings.Split(buf.String(), "\n");
    ref bytes.Buffer @out = ref heap(out ptr<bytes.Buffer> _addr_@out);

    fmt.Fprint(_addr_out, "<div>");
    foreach (var (_, l) in lines) {
        l = strings.TrimSpace(l);
        @string escaped = default;
        @string lineNo = default;
        if (l == "") {
            escaped = "&nbsp;";
        }
        else
 {
            if (strings.HasPrefix(l, "buildssa")) {
                escaped = fmt.Sprintf("<b>%v</b>", l);
            }
            else
 { 
                // Parse the line number from the format l(123).
                var idx = strings.Index(l, " l(");
                if (idx != -1) {
                    var subl = l[(int)idx + 3..];
                    var idxEnd = strings.Index(subl, ")");
                    if (idxEnd != -1) {
                        {
                            var (_, err) = strconv.Atoi(subl[..(int)idxEnd]);

                            if (err == null) {
                                lineNo = subl[..(int)idxEnd];
                            }

                        }
                    }
                }
                escaped = html.EscapeString(l);
            }
        }
        if (lineNo != "") {
            fmt.Fprintf(_addr_out, "<div class=\"l%v line-number ast\">%v</div>", lineNo, escaped);
        }
        else
 {
            fmt.Fprintf(_addr_out, "<div class=\"ast\">%v</div>", escaped);
        }
    }    fmt.Fprint(_addr_out, "</div>");
    w.WriteColumn(phase, phase, "allow-x-scroll", @out.String());
}

// WriteColumn writes raw HTML in a column headed by title.
// It is intended for pre- and post-compilation log output.
private static void WriteColumn(this ptr<HTMLWriter> _addr_w, @string phase, @string title, @string @class, @string html) {
    ref HTMLWriter w = ref _addr_w.val;

    w.WriteMultiTitleColumn(phase, new slice<@string>(new @string[] { title }), class, html);
}

private static void WriteMultiTitleColumn(this ptr<HTMLWriter> _addr_w, @string phase, slice<@string> titles, @string @class, @string html) {
    ref HTMLWriter w = ref _addr_w.val;

    if (w == null) {
        return ;
    }
    var id = strings.Replace(phase, " ", "-", -1); 
    // collapsed column
    w.Printf("<td id=\"%v-col\" class=\"collapsed\"><div>%v</div></td>", id, phase);

    if (class == "") {
        w.Printf("<td id=\"%v-exp\">", id);
    }
    else
 {
        w.Printf("<td id=\"%v-exp\" class=\"%v\">", id, class);
    }
    foreach (var (_, title) in titles) {
        w.WriteString("<h2>" + title + "</h2>");
    }    w.WriteString(html);
    w.WriteString("</td>\n");
}

private static void Printf(this ptr<HTMLWriter> _addr_w, @string msg, params object[] v) {
    v = v.Clone();
    ref HTMLWriter w = ref _addr_w.val;

    {
        var (_, err) = fmt.Fprintf(w.w, msg, v);

        if (err != null) {
            w.Fatalf("%v", err);
        }
    }
}

private static void WriteString(this ptr<HTMLWriter> _addr_w, @string s) {
    ref HTMLWriter w = ref _addr_w.val;

    {
        var (_, err) = io.WriteString(w.w, s);

        if (err != null) {
            w.Fatalf("%v", err);
        }
    }
}

private static @string HTML(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // TODO: Using the value ID as the class ignores the fact
    // that value IDs get recycled and that some values
    // are transmuted into other values.
    var s = v.String();
    return fmt.Sprintf("<span class=\"%s ssa-value\">%s</span>", s, s);
}

private static @string LongHTML(this ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // TODO: Any intra-value formatting?
    // I'm wary of adding too much visual noise,
    // but a little bit might be valuable.
    // We already have visual noise in the form of punctuation
    // maybe we could replace some of that with formatting.
    var s = fmt.Sprintf("<span class=\"%s ssa-long-value\">", v.String());

    @string linenumber = "<span class=\"no-line-number\">(?)</span>";
    if (v.Pos.IsKnown()) {
        linenumber = fmt.Sprintf("<span class=\"l%v line-number\">(%s)</span>", v.Pos.LineNumber(), v.Pos.LineNumberHTML());
    }
    s += fmt.Sprintf("%s %s = %s", v.HTML(), linenumber, v.Op.String());

    s += " &lt;" + html.EscapeString(v.Type.String()) + "&gt;";
    s += html.EscapeString(v.auxString());
    foreach (var (_, a) in v.Args) {
        s += fmt.Sprintf(" %s", a.HTML());
    }    var r = v.Block.Func.RegAlloc;
    if (int(v.ID) < len(r) && r[v.ID] != null) {
        s += " : " + html.EscapeString(r[v.ID].String());
    }
    slice<@string> names = default;
    foreach (var (name, values) in v.Block.Func.NamedValues) {
        foreach (var (_, value) in values) {
            if (value == v) {
                names = append(names, name.String());
                break; // drop duplicates.
            }
        }
    }    if (len(names) != 0) {
        s += " (" + strings.Join(names, ", ") + ")";
    }
    s += "</span>";
    return s;
}

private static @string HTML(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;
 
    // TODO: Using the value ID as the class ignores the fact
    // that value IDs get recycled and that some values
    // are transmuted into other values.
    var s = html.EscapeString(b.String());
    return fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", s, s);
}

private static @string LongHTML(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;
 
    // TODO: improve this for HTML?
    var s = fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", html.EscapeString(b.String()), html.EscapeString(b.Kind.String()));
    if (b.Aux != null) {
        s += html.EscapeString(fmt.Sprintf(" {%v}", b.Aux));
    }
    {
        var t = b.AuxIntString();

        if (t != "") {
            s += html.EscapeString(fmt.Sprintf(" [%v]", t));
        }
    }
    {
        var c__prev1 = c;

        foreach (var (_, __c) in b.ControlValues()) {
            c = __c;
            s += fmt.Sprintf(" %s", c.HTML());
        }
        c = c__prev1;
    }

    if (len(b.Succs) > 0) {
        s += " &#8594;"; // right arrow
        foreach (var (_, e) in b.Succs) {
            var c = e.b;
            s += " " + c.HTML();
        }
    }

    if (b.Likely == BranchUnlikely) 
        s += " (unlikely)";
    else if (b.Likely == BranchLikely) 
        s += " (likely)";
        if (b.Pos.IsKnown()) { 
        // TODO does not begin to deal with the full complexity of line numbers.
        // Maybe we want a string/slice instead, of outer-inner when inlining.
        s += fmt.Sprintf(" <span class=\"l%v line-number\">(%s)</span>", b.Pos.LineNumber(), b.Pos.LineNumberHTML());
    }
    return s;
}

private static @string HTML(this ptr<Func> _addr_f, @string phase, ptr<dotWriter> _addr_dot) {
    ref Func f = ref _addr_f.val;
    ref dotWriter dot = ref _addr_dot.val;

    ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
    if (dot != null) {
        dot.writeFuncSVG(buf, phase, f);
    }
    fmt.Fprint(buf, "<code>");
    htmlFuncPrinter p = new htmlFuncPrinter(w:buf);
    fprintFunc(p, f); 

    // fprintFunc(&buf, f) // TODO: HTML, not text, <br> for line breaks, etc.
    fmt.Fprint(buf, "</code>");
    return buf.String();
}

private static void writeFuncSVG(this ptr<dotWriter> _addr_d, io.Writer w, @string phase, ptr<Func> _addr_f) {
    ref dotWriter d = ref _addr_d.val;
    ref Func f = ref _addr_f.val;

    if (d.broken) {
        return ;
    }
    {
        var (_, ok) = d.phases[phase];

        if (!ok) {
            return ;
        }
    }
    var cmd = exec.Command(d.path, "-Tsvg");
    var (pipe, err) = cmd.StdinPipe();
    if (err != null) {
        d.broken = true;
        fmt.Println(err);
        return ;
    }
    ptr<bytes.Buffer> buf = @new<bytes.Buffer>();
    cmd.Stdout = buf;
    ptr<bytes.Buffer> bufErr = @new<bytes.Buffer>();
    cmd.Stderr = bufErr;
    err = cmd.Start();
    if (err != null) {
        d.broken = true;
        fmt.Println(err);
        return ;
    }
    fmt.Fprint(pipe, "digraph \"\" { margin=0; ranksep=.2; ");
    var id = strings.Replace(phase, " ", "-", -1);
    fmt.Fprintf(pipe, "id=\"g_graph_%s\";", id);
    fmt.Fprintf(pipe, "node [style=filled,fillcolor=white,fontsize=16,fontname=\"Menlo,Times,serif\",margi" +
    "n=\"0.01,0.03\"];");
    fmt.Fprintf(pipe, "edge [fontsize=16,fontname=\"Menlo,Times,serif\"];");
    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in f.Blocks) {
            i = __i;
            b = __b;
            if (b.Kind == BlockInvalid) {
                continue;
            }
            @string layout = "";
            if (f.laidout) {
                layout = fmt.Sprintf(" #%d", i);
            }
            fmt.Fprintf(pipe, "%v [label=\"%v%s\\n%v\",id=\"graph_node_%v_%v\",tooltip=\"%v\"];", b, b, layout, b.Kind.String(), id, b, b.LongString());
        }
        i = i__prev1;
        b = b__prev1;
    }

    var indexOf = make_slice<nint>(f.NumBlocks());
    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in f.Blocks) {
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
    Func<ID, ID, bool> isBackEdge = (from, to) => ponums[from] <= ponums[to];

    {
        var b__prev1 = b;

        foreach (var (_, __b) in f.Blocks) {
            b = __b;
            {
                var i__prev2 = i;

                foreach (var (__i, __s) in b.Succs) {
                    i = __i;
                    s = __s;
                    @string style = "solid";
                    @string color = "black";
                    @string arrow = "vee";
                    if (b.unlikelyIndex() == i) {
                        style = "dashed";
                    }
                    if (f.laidout && indexOf[s.b.ID] == indexOf[b.ID] + 1) { 
                        // Red color means ordered edge. It overrides other colors.
                        arrow = "dotvee";
                        layoutDrawn[s.b.ID] = true;
                    }
                    else if (isBackEdge(b.ID, s.b.ID)) {
                        color = "#2893ff";
                    }
                    fmt.Fprintf(pipe, "%v -> %v [label=\" %d \",style=\"%s\",color=\"%s\",arrowhead=\"%s\"];", b, s.b, i, style, color, arrow);
                }

                i = i__prev2;
            }
        }
        b = b__prev1;
    }

    if (f.laidout) {
        fmt.Fprintln(pipe, "edge[constraint=false,color=gray,style=solid,arrowhead=dot];");
        array<@string> colors = new array<@string>(new @string[] { "#eea24f", "#f38385", "#f4d164", "#ca89fc", "gray" });
        nint ci = 0;
        {
            var i__prev1 = i;

            for (nint i = 1; i < len(f.Blocks); i++) {
                if (layoutDrawn[f.Blocks[i].ID]) {
                    continue;
                }
                fmt.Fprintf(pipe, "%s -> %s [color=\"%s\"];", f.Blocks[i - 1], f.Blocks[i], colors[ci]);
                ci = (ci + 1) % len(colors);
            }


            i = i__prev1;
        }
    }
    fmt.Fprint(pipe, "}");
    pipe.Close();
    err = cmd.Wait();
    if (err != null) {
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
    if (err != null) {
        fmt.Printf("injecting attributes: %v\n", err);
        return ;
    }
    fmt.Fprintf(w, " id=\"%s\" onload=\"makeDraggable(evt)\" ", svgID);
    io.Copy(w, buf);
}

private static nint unlikelyIndex(this ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Likely == BranchLikely) 
        return 1;
    else if (b.Likely == BranchUnlikely) 
        return 0;
        return -1;
}

private static error copyUntil(this ptr<dotWriter> _addr_d, io.Writer w, ptr<bytes.Buffer> _addr_buf, @string sep) {
    ref dotWriter d = ref _addr_d.val;
    ref bytes.Buffer buf = ref _addr_buf.val;

    var i = bytes.Index(buf.Bytes(), (slice<byte>)sep);
    if (i == -1) {
        return error.As(fmt.Errorf("couldn't find dot sep %q", sep))!;
    }
    var (_, err) = io.CopyN(w, buf, int64(i + len(sep)));
    return error.As(err)!;
}

private partial struct htmlFuncPrinter {
    public io.Writer w;
}

private static void header(this htmlFuncPrinter p, ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

}

private static void startBlock(this htmlFuncPrinter p, ptr<Block> _addr_b, bool reachable) {
    ref Block b = ref _addr_b.val;

    @string dead = default;
    if (!reachable) {
        dead = "dead-block";
    }
    fmt.Fprintf(p.w, "<ul class=\"%s ssa-print-func %s\">", b, dead);
    fmt.Fprintf(p.w, "<li class=\"ssa-start-block\">%s:", b.HTML());
    if (len(b.Preds) > 0) {
        io.WriteString(p.w, " &#8592;"); // left arrow
        foreach (var (_, e) in b.Preds) {
            var pred = e.b;
            fmt.Fprintf(p.w, " %s", pred.HTML());
        }
    }
    if (len(b.Values) > 0) {
        io.WriteString(p.w, "<button onclick=\"hideBlock(this)\">-</button>");
    }
    io.WriteString(p.w, "</li>");
    if (len(b.Values) > 0) { // start list of values
        io.WriteString(p.w, "<li class=\"ssa-value-list\">");
        io.WriteString(p.w, "<ul>");
    }
}

private static void endBlock(this htmlFuncPrinter p, ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    if (len(b.Values) > 0) { // end list of values
        io.WriteString(p.w, "</ul>");
        io.WriteString(p.w, "</li>");
    }
    io.WriteString(p.w, "<li class=\"ssa-end-block\">");
    fmt.Fprint(p.w, b.LongHTML());
    io.WriteString(p.w, "</li>");
    io.WriteString(p.w, "</ul>");
}

private static void value(this htmlFuncPrinter p, ptr<Value> _addr_v, bool live) {
    ref Value v = ref _addr_v.val;

    @string dead = default;
    if (!live) {
        dead = "dead-value";
    }
    fmt.Fprintf(p.w, "<li class=\"ssa-long-value %s\">", dead);
    fmt.Fprint(p.w, v.LongHTML());
    io.WriteString(p.w, "</li>");
}

private static void startDepCycle(this htmlFuncPrinter p) {
    fmt.Fprintln(p.w, "<span class=\"depcycle\">");
}

private static void endDepCycle(this htmlFuncPrinter p) {
    fmt.Fprintln(p.w, "</span>");
}

private static void named(this htmlFuncPrinter p, LocalSlot n, slice<ptr<Value>> vals) {
    fmt.Fprintf(p.w, "<li>name %s: ", n);
    foreach (var (_, val) in vals) {
        fmt.Fprintf(p.w, "%s ", val.HTML());
    }    fmt.Fprintf(p.w, "</li>");
}

private partial struct dotWriter {
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
private static ptr<dotWriter> newDotWriter(@string mask) {
    if (mask == "") {
        return _addr_null!;
    }
    mask = strings.Replace(mask, "_", " ", -1);
    var ph = make_map<@string, bool>();
    var ranges = strings.Split(mask, ",");
    foreach (var (_, r) in ranges) {
        var spl = strings.Split(r, "-");
        if (len(spl) > 2) {
            fmt.Printf("range is not valid: %v\n", mask);
            return _addr_null!;
        }
        nint first = default;        nint last = default;

        if (mask == "*") {
            first = 0;
            last = len(passes) - 1;
        }
        else
 {
            first = passIdxByName(spl[0]);
            last = passIdxByName(spl[len(spl) - 1]);
        }
        if (first < 0 || last < 0 || first > last) {
            fmt.Printf("range is not valid: %v\n", r);
            return _addr_null!;
        }
        for (var p = first; p <= last; p++) {
            ph[passes[p].name] = true;
        }
    }    var (path, err) = exec.LookPath("dot");
    if (err != null) {
        fmt.Println(err);
        return _addr_null!;
    }
    return addr(new dotWriter(path:path,phases:ph));
}

private static nint passIdxByName(@string name) {
    foreach (var (i, p) in passes) {
        if (p.name == name) {
            return i;
        }
    }    return -1;
}

} // end ssa_package
