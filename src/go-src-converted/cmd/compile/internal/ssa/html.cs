// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:55 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\html.go
using bytes = go.bytes_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using html = go.html_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public partial struct HTMLWriter
        {
            public ref Logger Logger => ref Logger_val;
            public io.WriteCloser w;
        }

        public static ref HTMLWriter NewHTMLWriter(@string path, Logger logger, @string funcname)
        {
            var (out, err) = os.OpenFile(path, os.O_WRONLY | os.O_CREATE | os.O_TRUNC, 0644L);
            if (err != null)
            {
                logger.Fatalf(src.NoXPos, "%v", err);
            }
            HTMLWriter html = new HTMLWriter(w:out,Logger:logger);
            html.start(funcname);
            return ref html;
        }

        private static void start(this ref HTMLWriter w, @string name)
        {
            if (w == null)
            {
                return;
            }
            w.WriteString("<html>");
            w.WriteString("<head>\n<meta http-equiv=\"Content-Type\" content=\"text/html;charset=UTF-8\">\n<style>" +
    "\n\n#helplink {\n    margin-bottom: 15px;\n    display: block;\n    margin-top: -15px" +
    ";\n}\n\n#help {\n    display: none;\n}\n\n.stats {\n\tfont-size: 60%;\n}\n\ntable {\n    bord" +
    "er: 1px solid black;\n    table-layout: fixed;\n    width: 300px;\n}\n\nth, td {\n    " +
    "border: 1px solid black;\n    overflow: hidden;\n    width: 400px;\n    vertical-al" +
    "ign: top;\n    padding: 5px;\n}\n\ntd.ssa-prog {\n    width: 600px;\n    word-wrap: br" +
    "eak-word;\n}\n\nli {\n    list-style-type: none;\n}\n\nli.ssa-long-value {\n    text-ind" +
    "ent: -2em;  /* indent wrapped lines */\n}\n\nli.ssa-value-list {\n    display: inlin" +
    "e;\n}\n\nli.ssa-start-block {\n    padding: 0;\n    margin: 0;\n}\n\nli.ssa-end-block {\n" +
    "    padding: 0;\n    margin: 0;\n}\n\nul.ssa-print-func {\n    padding-left: 0;\n}\n\ndl" +
    ".ssa-gen {\n    padding-left: 0;\n}\n\ndt.ssa-prog-src {\n    padding: 0;\n    margin:" +
    " 0;\n    float: left;\n    width: 4em;\n}\n\ndd.ssa-prog {\n    padding: 0;\n    margin" +
    "-right: 0;\n    margin-left: 4em;\n}\n\n.dead-value {\n    color: gray;\n}\n\n.dead-bloc" +
    "k {\n    opacity: 0.5;\n}\n\n.depcycle {\n    font-style: italic;\n}\n\n.line-number {\n " +
    "   font-style: italic;\n    font-size: 11px;\n}\n\n.highlight-yellow         { backg" +
    "round-color: yellow; }\n.highlight-aquamarine     { background-color: aquamarine;" +
    " }\n.highlight-coral          { background-color: coral; }\n.highlight-lightpink  " +
    "    { background-color: lightpink; }\n.highlight-lightsteelblue { background-colo" +
    "r: lightsteelblue; }\n.highlight-palegreen      { background-color: palegreen; }\n" +
    ".highlight-powderblue     { background-color: powderblue; }\n.highlight-lightgray" +
    "      { background-color: lightgray; }\n\n.outline-blue           { outline: blue " +
    "solid 2px; }\n.outline-red            { outline: red solid 2px; }\n.outline-bluevi" +
    "olet     { outline: blueviolet solid 2px; }\n.outline-darkolivegreen { outline: d" +
    "arkolivegreen solid 2px; }\n.outline-fuchsia        { outline: fuchsia solid 2px;" +
    " }\n.outline-sienna         { outline: sienna solid 2px; }\n.outline-gold         " +
    "  { outline: gold solid 2px; }\n\n</style>\n\n<script type=\"text/javascript\">\n// ord" +
    "ered list of all available highlight colors\nvar highlights = [\n    \"highlight-aq" +
    "uamarine\",\n    \"highlight-coral\",\n    \"highlight-lightpink\",\n    \"highlight-ligh" +
    "tsteelblue\",\n    \"highlight-palegreen\",\n    \"highlight-lightgray\",\n    \"highligh" +
    "t-yellow\"\n];\n\n// state: which value is highlighted this color?\nvar highlighted =" +
    " {};\nfor (var i = 0; i < highlights.length; i++) {\n    highlighted[highlights[i]" +
    "] = \"\";\n}\n\n// ordered list of all available outline colors\nvar outlines = [\n    " +
    "\"outline-blue\",\n    \"outline-red\",\n    \"outline-blueviolet\",\n    \"outline-darkol" +
    "ivegreen\",\n    \"outline-fuchsia\",\n    \"outline-sienna\",\n    \"outline-gold\"\n];\n\n/" +
    "/ state: which value is outlined this color?\nvar outlined = {};\nfor (var i = 0; " +
    "i < outlines.length; i++) {\n    outlined[outlines[i]] = \"\";\n}\n\nwindow.onload = f" +
    "unction() {\n    var ssaElemClicked = function(elem, event, selections, selected)" +
    " {\n        event.stopPropagation()\n\n        // TODO: pushState with updated stat" +
    "e and read it on page load,\n        // so that state can survive across reloads\n" +
    "\n        // find all values with the same name\n        var c = elem.classList.it" +
    "em(0);\n        var x = document.getElementsByClassName(c);\n\n        // if select" +
    "ed, remove selections from all of them\n        // otherwise, attempt to add\n\n   " +
    "     var remove = \"\";\n        for (var i = 0; i < selections.length; i++) {\n    " +
    "        var color = selections[i];\n            if (selected[color] == c) {\n     " +
    "           remove = color;\n                break;\n            }\n        }\n\n     " +
    "   if (remove != \"\") {\n            for (var i = 0; i < x.length; i++) {\n        " +
    "        x[i].classList.remove(remove);\n            }\n            selected[remove" +
    "] = \"\";\n            return;\n        }\n\n        // we\'re adding a selection\n     " +
    "   // find first available color\n        var avail = \"\";\n        for (var i = 0;" +
    " i < selections.length; i++) {\n            var color = selections[i];\n          " +
    "  if (selected[color] == \"\") {\n                avail = color;\n                br" +
    "eak;\n            }\n        }\n        if (avail == \"\") {\n            alert(\"out o" +
    "f selection colors; go add more\");\n            return;\n        }\n\n        // set" +
    " that as the selection\n        for (var i = 0; i < x.length; i++) {\n            " +
    "x[i].classList.add(avail);\n        }\n        selected[avail] = c;\n    };\n\n    va" +
    "r ssaValueClicked = function(event) {\n        ssaElemClicked(this, event, highli" +
    "ghts, highlighted);\n    }\n\n    var ssaBlockClicked = function(event) {\n        s" +
    "saElemClicked(this, event, outlines, outlined);\n    }\n\n    var ssavalues = docum" +
    "ent.getElementsByClassName(\"ssa-value\");\n    for (var i = 0; i < ssavalues.lengt" +
    "h; i++) {\n        ssavalues[i].addEventListener(\'click\', ssaValueClicked);\n    }" +
    "\n\n    var ssalongvalues = document.getElementsByClassName(\"ssa-long-value\");\n   " +
    " for (var i = 0; i < ssalongvalues.length; i++) {\n        // don\'t attach listen" +
    "ers to li nodes, just the spans they contain\n        if (ssalongvalues[i].nodeNa" +
    "me == \"SPAN\") {\n            ssalongvalues[i].addEventListener(\'click\', ssaValueC" +
    "licked);\n        }\n    }\n\n    var ssablocks = document.getElementsByClassName(\"s" +
    "sa-block\");\n    for (var i = 0; i < ssablocks.length; i++) {\n        ssablocks[i" +
    "].addEventListener(\'click\', ssaBlockClicked);\n    }\n};\n\nfunction toggle_visibili" +
    "ty(id) {\n   var e = document.getElementById(id);\n   if(e.style.display == \'block" +
    "\')\n      e.style.display = \'none\';\n   else\n      e.style.display = \'block\';\n}\n</" +
    "script>\n\n</head>");
            w.WriteString("<body>");
            w.WriteString("<h1>");
            w.WriteString(html.EscapeString(name));
            w.WriteString("</h1>");
            w.WriteString(@"
<a href=""#"" onclick=""toggle_visibility('help');"" id=""helplink"">help</a>
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

</div>
");
            w.WriteString("<table>");
            w.WriteString("<tr>");
        }

        private static void Close(this ref HTMLWriter w)
        {
            if (w == null)
            {
                return;
            }
            io.WriteString(w.w, "</tr>");
            io.WriteString(w.w, "</table>");
            io.WriteString(w.w, "</body>");
            io.WriteString(w.w, "</html>");
            w.w.Close();
        }

        // WriteFunc writes f in a column headed by title.
        private static void WriteFunc(this ref HTMLWriter w, @string title, ref Func f)
        {
            if (w == null)
            {
                return; // avoid generating HTML just to discard it
            }
            w.WriteColumn(title, "", f.HTML()); 
            // TODO: Add visual representation of f's CFG.
        }

        // WriteColumn writes raw HTML in a column headed by title.
        // It is intended for pre- and post-compilation log output.
        private static void WriteColumn(this ref HTMLWriter w, @string title, @string @class, @string html)
        {
            if (w == null)
            {
                return;
            }
            if (class == "")
            {
                w.WriteString("<td>");
            }
            else
            {
                w.WriteString("<td class=\"" + class + "\">");
            }
            w.WriteString("<h2>" + title + "</h2>");
            w.WriteString(html);
            w.WriteString("</td>");
        }

        private static void Printf(this ref HTMLWriter w, @string msg, params object[] v)
        {
            {
                var (_, err) = fmt.Fprintf(w.w, msg, v);

                if (err != null)
                {
                    w.Fatalf(src.NoXPos, "%v", err);
                }

            }
        }

        private static void WriteString(this ref HTMLWriter w, @string s)
        {
            {
                var (_, err) = io.WriteString(w.w, s);

                if (err != null)
                {
                    w.Fatalf(src.NoXPos, "%v", err);
                }

            }
        }

        private static @string HTML(this ref Value v)
        { 
            // TODO: Using the value ID as the class ignores the fact
            // that value IDs get recycled and that some values
            // are transmuted into other values.
            var s = v.String();
            return fmt.Sprintf("<span class=\"%s ssa-value\">%s</span>", s, s);
        }

        private static @string LongHTML(this ref Value v)
        { 
            // TODO: Any intra-value formatting?
            // I'm wary of adding too much visual noise,
            // but a little bit might be valuable.
            // We already have visual noise in the form of punctuation
            // maybe we could replace some of that with formatting.
            var s = fmt.Sprintf("<span class=\"%s ssa-long-value\">", v.String());

            @string linenumber = "<span class=\"line-number\">(?)</span>";
            if (v.Pos.IsKnown())
            {
                linenumber = fmt.Sprintf("<span class=\"line-number\">(%d)</span>", v.Pos.Line());
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

        private static @string HTML(this ref Block b)
        { 
            // TODO: Using the value ID as the class ignores the fact
            // that value IDs get recycled and that some values
            // are transmuted into other values.
            var s = html.EscapeString(b.String());
            return fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", s, s);
        }

        private static @string LongHTML(this ref Block b)
        { 
            // TODO: improve this for HTML?
            var s = fmt.Sprintf("<span class=\"%s ssa-block\">%s</span>", html.EscapeString(b.String()), html.EscapeString(b.Kind.String()));
            if (b.Aux != null)
            {
                s += html.EscapeString(fmt.Sprintf(" {%v}", b.Aux));
            }
            if (b.Control != null)
            {
                s += fmt.Sprintf(" %s", b.Control.HTML());
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
                s += fmt.Sprintf(" (line %d)", b.Pos.Line());
            }
            return s;
        }

        private static @string HTML(this ref Func f)
        {
            bytes.Buffer buf = default;
            fmt.Fprint(ref buf, "<code>");
            htmlFuncPrinter p = new htmlFuncPrinter(w:&buf);
            fprintFunc(p, f); 

            // fprintFunc(&buf, f) // TODO: HTML, not text, <br /> for line breaks, etc.
            fmt.Fprint(ref buf, "</code>");
            return buf.String();
        }

        private partial struct htmlFuncPrinter
        {
            public io.Writer w;
        }

        private static void header(this htmlFuncPrinter p, ref Func f)
        {
        }

        private static void startBlock(this htmlFuncPrinter p, ref Block b, bool reachable)
        { 
            // TODO: Make blocks collapsable?
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
            io.WriteString(p.w, "</li>");
            if (len(b.Values) > 0L)
            { // start list of values
                io.WriteString(p.w, "<li class=\"ssa-value-list\">");
                io.WriteString(p.w, "<ul>");
            }
        }

        private static void endBlock(this htmlFuncPrinter p, ref Block b)
        {
            if (len(b.Values) > 0L)
            { // end list of values
                io.WriteString(p.w, "</ul>");
                io.WriteString(p.w, "</li>");
            }
            io.WriteString(p.w, "<li class=\"ssa-end-block\">");
            fmt.Fprint(p.w, b.LongHTML());
            io.WriteString(p.w, "</li>");
            io.WriteString(p.w, "</ul>"); 
            // io.WriteString(p.w, "</span>")
        }

        private static void value(this htmlFuncPrinter p, ref Value v, bool live)
        {
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

        private static void named(this htmlFuncPrinter p, LocalSlot n, slice<ref Value> vals)
        {
            fmt.Fprintf(p.w, "<li>name %s: ", n);
            foreach (var (_, val) in vals)
            {
                fmt.Fprintf(p.w, "%s ", val.HTML());
            }
            fmt.Fprintf(p.w, "</li>");
        }
    }
}}}}
