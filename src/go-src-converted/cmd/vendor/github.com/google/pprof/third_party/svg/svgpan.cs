// SVG pan and zoom library.
// See copyright notice in string constant below.

// package svg -- go2cs converted at 2020 August 29 10:06:35 UTC
// import "cmd/vendor/github.com/google/pprof/third_party/svg" ==> using svg = go.cmd.vendor.github.com.google.pprof.third_party.svg_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\third_party\svg\svgpan.go

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
        // https://www.cyberz.org/projects/SVGPan/SVGPan.js
        private static readonly @string svgPanJS = "\n/** \n *  SVGPan library 1.2.1\n * ======================\n *\n * Given an unique ex" +
    "isting element with id \"viewport\" (or when missing, the first g \n * element), in" +
    "cluding the the library into any SVG adds the following capabilities:\n *\n *  - M" +
    "ouse panning\n *  - Mouse zooming (using the wheel)\n *  - Object dragging\n *\n * Y" +
    "ou can configure the behaviour of the pan/zoom/drag with the variables\n * listed" +
    " in the CONFIGURATION section of this file.\n *\n * Known issues:\n *\n *  - Zooming" +
    " (while panning) on Safari has still some issues\n *\n * Releases:\n *\n * 1.2.1, Mo" +
    "n Jul  4 00:33:18 CEST 2011, Andrea Leofreddi\n *\t- Fixed a regression with mouse" +
    " wheel (now working on Firefox 5)\n *\t- Working with viewBox attribute (#4)\n *\t- " +
    "Added \"use strict;\" and fixed resulting warnings (#5)\n *\t- Added configuration v" +
    "ariables, dragging is disabled by default (#3)\n *\n * 1.2, Sat Mar 20 08:42:50 GM" +
    "T 2010, Zeng Xiaohui\n *\tFixed a bug with browser mouse handler interaction\n *\n *" +
    " 1.1, Wed Feb  3 17:39:33 GMT 2010, Zeng Xiaohui\n *\tUpdated the zoom code to sup" +
    "port the mouse wheel on Safari/Chrome\n *\n * 1.0, Andrea Leofreddi\n *\tFirst relea" +
    "se\n *\n * This code is licensed under the following BSD license:\n *\n * Copyright " +
    "2009-2010 Andrea Leofreddi <a.leofreddi@itcharm.com>. All rights reserved.\n * \n " +
    "* Redistribution and use in source and binary forms, with or without modificatio" +
    "n, are\n * permitted provided that the following conditions are met:\n * \n *    1." +
    " Redistributions of source code must retain the above copyright notice, this lis" +
    "t of\n *       conditions and the following disclaimer.\n * \n *    2. Redistributi" +
    "ons in binary form must reproduce the above copyright notice, this list\n *      " +
    " of conditions and the following disclaimer in the documentation and/or other ma" +
    "terials\n *       provided with the distribution.\n * \n * THIS SOFTWARE IS PROVIDE" +
    "D BY Andrea Leofreddi " + "``AS IS''" + " AND ANY EXPRESS OR IMPLIED\n * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMP" +
    "LIED WARRANTIES OF MERCHANTABILITY AND\n * FITNESS FOR A PARTICULAR PURPOSE ARE D" +
    "ISCLAIMED. IN NO EVENT SHALL Andrea Leofreddi OR\n * CONTRIBUTORS BE LIABLE FOR A" +
    "NY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR\n * CONSEQUENTIAL DAMAGES" +
    " (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR\n * SERVICES;" +
    " LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON\n" +
    " * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCL" +
    "UDING\n * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFT" +
    "WARE, EVEN IF\n * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\n * \n * The views and" +
    " conclusions contained in the software and documentation are those of the\n * aut" +
    "hors and should not be interpreted as representing official policies, either exp" +
    "ressed\n * or implied, of Andrea Leofreddi.\n */\n\n\"use strict\";\n\n/// CONFIGURATION" +
    " \n/// ====>\n\nvar enablePan = 1; // 1 or 0: enable or disable panning (default en" +
    "abled)\nvar enableZoom = 1; // 1 or 0: enable or disable zooming (default enabled" +
    ")\nvar enableDrag = 0; // 1 or 0: enable or disable dragging (default disabled)\n\n" +
    "/// <====\n/// END OF CONFIGURATION \n\nvar root = document.documentElement;\n\nvar s" +
    "tate = \'none\', svgRoot, stateTarget, stateOrigin, stateTf;\n\nsetupHandlers(root);" +
    "\n\n/**\n * Register handlers\n */\nfunction setupHandlers(root){\n\tsetAttributes(root" +
    ", {\n\t\t\"onmouseup\" : \"handleMouseUp(evt)\",\n\t\t\"onmousedown\" : \"handleMouseDown(evt" +
    ")\",\n\t\t\"onmousemove\" : \"handleMouseMove(evt)\",\n\t\t//\"onmouseout\" : \"handleMouseUp(" +
    "evt)\", // Decomment this to stop the pan functionality when dragging out of the " +
    "SVG element\n\t});\n\n\tif(navigator.userAgent.toLowerCase().indexOf(\'webkit\') >= 0)\n" +
    "\t\twindow.addEventListener(\'mousewheel\', handleMouseWheel, false); // Chrome/Safa" +
    "ri\n\telse\n\t\twindow.addEventListener(\'DOMMouseScroll\', handleMouseWheel, false); /" +
    "/ Others\n}\n\n/**\n * Retrieves the root element for SVG manipulation. The element " +
    "is then cached into the svgRoot global variable.\n */\nfunction getRoot(root) {\n\ti" +
    "f(typeof(svgRoot) == \"undefined\") {\n\t\tvar g = null;\n\n\t\tg = root.getElementById(\"" +
    "viewport\");\n\n\t\tif(g == null)\n\t\t\tg = root.getElementsByTagName(\'g\')[0];\n\n\t\tif(g =" +
    "= null)\n\t\t\talert(\'Unable to obtain SVG root element\');\n\n\t\tsetCTM(g, g.getCTM());" +
    "\n\n\t\tg.removeAttribute(\"viewBox\");\n\n\t\tsvgRoot = g;\n\t}\n\n\treturn svgRoot;\n}\n\n/**\n *" +
    " Instance an SVGPoint object with given event coordinates.\n */\nfunction getEvent" +
    "Point(evt) {\n\tvar p = root.createSVGPoint();\n\n\tp.x = evt.clientX;\n\tp.y = evt.cli" +
    "entY;\n\n\treturn p;\n}\n\n/**\n * Sets the current transform matrix of an element.\n */" +
    "\nfunction setCTM(element, matrix) {\n\tvar s = \"matrix(\" + matrix.a + \",\" + matrix" +
    ".b + \",\" + matrix.c + \",\" + matrix.d + \",\" + matrix.e + \",\" + matrix.f + \")\";\n\n\t" +
    "element.setAttribute(\"transform\", s);\n}\n\n/**\n * Dumps a matrix to a string (usef" +
    "ul for debug).\n */\nfunction dumpMatrix(matrix) {\n\tvar s = \"[ \" + matrix.a + \", \"" +
    " + matrix.c + \", \" + matrix.e + \"\\n  \" + matrix.b + \", \" + matrix.d + \", \" + mat" +
    "rix.f + \"\\n  0, 0, 1 ]\";\n\n\treturn s;\n}\n\n/**\n * Sets attributes of an element.\n *" +
    "/\nfunction setAttributes(element, attributes){\n\tfor (var i in attributes)\n\t\telem" +
    "ent.setAttributeNS(null, i, attributes[i]);\n}\n\n/**\n * Handle mouse wheel event.\n" +
    " */\nfunction handleMouseWheel(evt) {\n\tif(!enableZoom)\n\t\treturn;\n\n\tif(evt.prevent" +
    "Default)\n\t\tevt.preventDefault();\n\n\tevt.returnValue = false;\n\n\tvar svgDoc = evt.t" +
    "arget.ownerDocument;\n\n\tvar delta;\n\n\tif(evt.wheelDelta)\n\t\tdelta = evt.wheelDelta " +
    "/ 3600; // Chrome/Safari\n\telse\n\t\tdelta = evt.detail / -90; // Mozilla\n\n\tvar z = " +
    "1 + delta; // Zoom factor: 0.9/1.1\n\n\tvar g = getRoot(svgDoc);\n\t\n\tvar p = getEven" +
    "tPoint(evt);\n\n\tp = p.matrixTransform(g.getCTM().inverse());\n\n\t// Compute new sca" +
    "le matrix in current mouse position\n\tvar k = root.createSVGMatrix().translate(p." +
    "x, p.y).scale(z).translate(-p.x, -p.y);\n\n        setCTM(g, g.getCTM().multiply(k" +
    "));\n\n\tif(typeof(stateTf) == \"undefined\")\n\t\tstateTf = g.getCTM().inverse();\n\n\tsta" +
    "teTf = stateTf.multiply(k.inverse());\n}\n\n/**\n * Handle mouse move event.\n */\nfun" +
    "ction handleMouseMove(evt) {\n\tif(evt.preventDefault)\n\t\tevt.preventDefault();\n\n\te" +
    "vt.returnValue = false;\n\n\tvar svgDoc = evt.target.ownerDocument;\n\n\tvar g = getRo" +
    "ot(svgDoc);\n\n\tif(state == \'pan\' && enablePan) {\n\t\t// Pan mode\n\t\tvar p = getEvent" +
    "Point(evt).matrixTransform(stateTf);\n\n\t\tsetCTM(g, stateTf.inverse().translate(p." +
    "x - stateOrigin.x, p.y - stateOrigin.y));\n\t} else if(state == \'drag\' && enableDr" +
    "ag) {\n\t\t// Drag mode\n\t\tvar p = getEventPoint(evt).matrixTransform(g.getCTM().inv" +
    "erse());\n\n\t\tsetCTM(stateTarget, root.createSVGMatrix().translate(p.x - stateOrig" +
    "in.x, p.y - stateOrigin.y).multiply(g.getCTM().inverse()).multiply(stateTarget.g" +
    "etCTM()));\n\n\t\tstateOrigin = p;\n\t}\n}\n\n/**\n * Handle click event.\n */\nfunction han" +
    "dleMouseDown(evt) {\n\tif(evt.preventDefault)\n\t\tevt.preventDefault();\n\n\tevt.return" +
    "Value = false;\n\n\tvar svgDoc = evt.target.ownerDocument;\n\n\tvar g = getRoot(svgDoc" +
    ");\n\n\tif(\n\t\tevt.target.tagName == \"svg\" \n\t\t|| !enableDrag // Pan anyway when drag" +
    " is disabled and the user clicked on an element \n\t) {\n\t\t// Pan mode\n\t\tstate = \'p" +
    "an\';\n\n\t\tstateTf = g.getCTM().inverse();\n\n\t\tstateOrigin = getEventPoint(evt).matr" +
    "ixTransform(stateTf);\n\t} else {\n\t\t// Drag mode\n\t\tstate = \'drag\';\n\n\t\tstateTarget " +
    "= evt.target;\n\n\t\tstateTf = g.getCTM().inverse();\n\n\t\tstateOrigin = getEventPoint(" +
    "evt).matrixTransform(stateTf);\n\t}\n}\n\n/**\n * Handle mouse button release event.\n " +
    "*/\nfunction handleMouseUp(evt) {\n\tif(evt.preventDefault)\n\t\tevt.preventDefault();" +
    "\n\n\tevt.returnValue = false;\n\n\tvar svgDoc = evt.target.ownerDocument;\n\n\tif(state " +
    "== \'pan\' || state == \'drag\') {\n\t\t// Quit pan mode\n\t\tstate = \'\';\n\t}\n}\n\n";

    }
}}}}}}}
