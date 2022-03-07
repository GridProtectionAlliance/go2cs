// SVG pan and zoom library.
// See copyright notice in string constant below.

// package svgpan -- go2cs converted at 2022 March 06 23:24:05 UTC
// import "cmd/vendor/github.com/google/pprof/third_party/svgpan" ==> using svgpan = go.cmd.vendor.github.com.google.pprof.third_party.svgpan_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\third_party\svgpan\svgpan.go


namespace go.cmd.vendor.github.com.google.pprof.third_party;

public static partial class svgpan_package {

    // https://github.com/aleofreddi/svgpan

    // JSSource returns the svgpan.js file
public static readonly @string JSSource = "\n/**\n *  SVGPan library 1.2.2\n * ======================\n *\n * Given an unique exi" +
    "sting element with id \"viewport\" (or when missing, the\n * first g-element), incl" +
    "uding the library into any SVG adds the following\n * capabilities:\n *\n *  - Mous" +
    "e panning\n *  - Mouse zooming (using the wheel)\n *  - Object dragging\n *\n * You " +
    "can configure the behaviour of the pan/zoom/drag with the variables\n * listed in" +
    " the CONFIGURATION section of this file.\n *\n * Known issues:\n *\n *  - Zooming (w" +
    "hile panning) on Safari has still some issues\n *\n * Releases:\n *\n * 1.2.2, Tue A" +
    "ug 30 17:21:56 CEST 2011, Andrea Leofreddi\n *\t- Fixed viewBox on root tag (#7)\n " +
    "*\t- Improved zoom speed (#2)\n *\n * 1.2.1, Mon Jul  4 00:33:18 CEST 2011, Andrea " +
    "Leofreddi\n *\t- Fixed a regression with mouse wheel (now working on Firefox 5)\n *" +
    "\t- Working with viewBox attribute (#4)\n *\t- Added \"use strict;\" and fixed result" +
    "ing warnings (#5)\n *\t- Added configuration variables, dragging is disabled by de" +
    "fault (#3)\n *\n * 1.2, Sat Mar 20 08:42:50 GMT 2010, Zeng Xiaohui\n *\tFixed a bug " +
    "with browser mouse handler interaction\n *\n * 1.1, Wed Feb  3 17:39:33 GMT 2010, " +
    "Zeng Xiaohui\n *\tUpdated the zoom code to support the mouse wheel on Safari/Chrom" +
    "e\n *\n * 1.0, Andrea Leofreddi\n *\tFirst release\n *\n * This code is licensed under" +
    " the following BSD license:\n *\n * Copyright 2009-2017 Andrea Leofreddi <a.leofre" +
    "ddi@vleo.net>. All rights reserved.\n *\n * Redistribution and use in source and b" +
    "inary forms, with or without modification, are\n * permitted provided that the fo" +
    "llowing conditions are met:\n *\n *    1. Redistributions of source code must reta" +
    "in the above copyright\n *       notice, this list of conditions and the followin" +
    "g disclaimer.\n *    2. Redistributions in binary form must reproduce the above c" +
    "opyright\n *       notice, this list of conditions and the following disclaimer i" +
    "n the\n *       documentation and/or other materials provided with the distributi" +
    "on.\n *    3. Neither the name of the copyright holder nor the names of its\n *   " +
    "    contributors may be used to endorse or promote products derived from\n *     " +
    "  this software without specific prior written permission.\n *\n * THIS SOFTWARE I" +
    "S PROVIDED BY COPYRIGHT HOLDERS AND CONTRIBUTORS \'\'AS IS\'\' AND ANY EXPRESS\n * OR" +
    " IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF ME" +
    "RCHANTABILITY\n * AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVEN" +
    "T SHALL COPYRIGHT HOLDERS OR\n * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT," +
    " INCIDENTAL, SPECIAL, EXEMPLARY, OR\n * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT" +
    " LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR\n * SERVICES; LOSS OF USE, DATA, " +
    "OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON\n * ANY THEORY OF LIA" +
    "BILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING\n * NEGLIGENCE " +
    "OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF\n * ADV" +
    "ISED OF THE POSSIBILITY OF SUCH DAMAGE.\n *\n * The views and conclusions containe" +
    "d in the software and documentation are those of the\n * authors and should not b" +
    "e interpreted as representing official policies, either expressed\n * or implied," +
    " of Andrea Leofreddi.\n */\n\n\"use strict\";\n\n/// CONFIGURATION\n/// ====>\n\nvar enabl" +
    "ePan = 1; // 1 or 0: enable or disable panning (default enabled)\nvar enableZoom " +
    "= 1; // 1 or 0: enable or disable zooming (default enabled)\nvar enableDrag = 0; " +
    "// 1 or 0: enable or disable dragging (default disabled)\nvar zoomScale = 0.2; //" +
    " Zoom sensitivity\n\n/// <====\n/// END OF CONFIGURATION\n\nvar root = document.docum" +
    "entElement;\n\nvar state = \'none\', svgRoot = null, stateTarget, stateOrigin, state" +
    "Tf;\n\nsetupHandlers(root);\n\n/**\n * Register handlers\n */\nfunction setupHandlers(r" +
    "oot){\n\tsetAttributes(root, {\n\t\t\"onmouseup\" : \"handleMouseUp(evt)\",\n\t\t\"onmousedow" +
    "n\" : \"handleMouseDown(evt)\",\n\t\t\"onmousemove\" : \"handleMouseMove(evt)\",\n\t\t//\"onmo" +
    "useout\" : \"handleMouseUp(evt)\", // Decomment this to stop the pan functionality " +
    "when dragging out of the SVG element\n\t});\n\n\tif(navigator.userAgent.toLowerCase()" +
    ".indexOf(\'webkit\') >= 0)\n\t\twindow.addEventListener(\'mousewheel\', handleMouseWhee" +
    "l, false); // Chrome/Safari\n\telse\n\t\twindow.addEventListener(\'DOMMouseScroll\', ha" +
    "ndleMouseWheel, false); // Others\n}\n\n/**\n * Retrieves the root element for SVG m" +
    "anipulation. The element is then cached into the svgRoot global variable.\n */\nfu" +
    "nction getRoot(root) {\n\tif(svgRoot == null) {\n\t\tvar r = root.getElementById(\"vie" +
    "wport\") ? root.getElementById(\"viewport\") : root.documentElement, t = r;\n\n\t\twhil" +
    "e(t != root) {\n\t\t\tif(t.getAttribute(\"viewBox\")) {\n\t\t\t\tsetCTM(r, t.getCTM());\n\n\t\t" +
    "\t\tt.removeAttribute(\"viewBox\");\n\t\t\t}\n\n\t\t\tt = t.parentNode;\n\t\t}\n\n\t\tsvgRoot = r;\n\t" +
    "}\n\n\treturn svgRoot;\n}\n\n/**\n * Instance an SVGPoint object with given event coord" +
    "inates.\n */\nfunction getEventPoint(evt) {\n\tvar p = root.createSVGPoint();\n\n\tp.x " +
    "= evt.clientX;\n\tp.y = evt.clientY;\n\n\treturn p;\n}\n\n/**\n * Sets the current transf" +
    "orm matrix of an element.\n */\nfunction setCTM(element, matrix) {\n\tvar s = \"matri" +
    "x(\" + matrix.a + \",\" + matrix.b + \",\" + matrix.c + \",\" + matrix.d + \",\" + matrix" +
    ".e + \",\" + matrix.f + \")\";\n\n\telement.setAttribute(\"transform\", s);\n}\n\n/**\n * Dum" +
    "ps a matrix to a string (useful for debug).\n */\nfunction dumpMatrix(matrix) {\n\tv" +
    "ar s = \"[ \" + matrix.a + \", \" + matrix.c + \", \" + matrix.e + \"\\n  \" + matrix.b +" +
    " \", \" + matrix.d + \", \" + matrix.f + \"\\n  0, 0, 1 ]\";\n\n\treturn s;\n}\n\n/**\n * Sets" +
    " attributes of an element.\n */\nfunction setAttributes(element, attributes){\n\tfor" +
    " (var i in attributes)\n\t\telement.setAttributeNS(null, i, attributes[i]);\n}\n\n/**\n" +
    " * Handle mouse wheel event.\n */\nfunction handleMouseWheel(evt) {\n\tif(!enableZoo" +
    "m)\n\t\treturn;\n\n\tif(evt.preventDefault)\n\t\tevt.preventDefault();\n\n\tevt.returnValue " +
    "= false;\n\n\tvar svgDoc = evt.target.ownerDocument;\n\n\tvar delta;\n\n\tif(evt.wheelDel" +
    "ta)\n\t\tdelta = evt.wheelDelta / 360; // Chrome/Safari\n\telse\n\t\tdelta = evt.detail " +
    "/ -9; // Mozilla\n\n\tvar z = Math.pow(1 + zoomScale, delta);\n\n\tvar g = getRoot(svg" +
    "Doc);\n\t\n\tvar p = getEventPoint(evt);\n\n\tp = p.matrixTransform(g.getCTM().inverse(" +
    "));\n\n\t// Compute new scale matrix in current mouse position\n\tvar k = root.create" +
    "SVGMatrix().translate(p.x, p.y).scale(z).translate(-p.x, -p.y);\n\n        setCTM(" +
    "g, g.getCTM().multiply(k));\n\n\tif(typeof(stateTf) == \"undefined\")\n\t\tstateTf = g.g" +
    "etCTM().inverse();\n\n\tstateTf = stateTf.multiply(k.inverse());\n}\n\n/**\n * Handle m" +
    "ouse move event.\n */\nfunction handleMouseMove(evt) {\n\tif(evt.preventDefault)\n\t\te" +
    "vt.preventDefault();\n\n\tevt.returnValue = false;\n\n\tvar svgDoc = evt.target.ownerD" +
    "ocument;\n\n\tvar g = getRoot(svgDoc);\n\n\tif(state == \'pan\' && enablePan) {\n\t\t// Pan" +
    " mode\n\t\tvar p = getEventPoint(evt).matrixTransform(stateTf);\n\n\t\tsetCTM(g, stateT" +
    "f.inverse().translate(p.x - stateOrigin.x, p.y - stateOrigin.y));\n\t} else if(sta" +
    "te == \'drag\' && enableDrag) {\n\t\t// Drag mode\n\t\tvar p = getEventPoint(evt).matrix" +
    "Transform(g.getCTM().inverse());\n\n\t\tsetCTM(stateTarget, root.createSVGMatrix().t" +
    "ranslate(p.x - stateOrigin.x, p.y - stateOrigin.y).multiply(g.getCTM().inverse()" +
    ").multiply(stateTarget.getCTM()));\n\n\t\tstateOrigin = p;\n\t}\n}\n\n/**\n * Handle click" +
    " event.\n */\nfunction handleMouseDown(evt) {\n\tif(evt.preventDefault)\n\t\tevt.preven" +
    "tDefault();\n\n\tevt.returnValue = false;\n\n\tvar svgDoc = evt.target.ownerDocument;\n" +
    "\n\tvar g = getRoot(svgDoc);\n\n\tif(\n\t\tevt.target.tagName == \"svg\"\n\t\t|| !enableDrag " +
    "// Pan anyway when drag is disabled and the user clicked on an element\n\t) {\n\t\t//" +
    " Pan mode\n\t\tstate = \'pan\';\n\n\t\tstateTf = g.getCTM().inverse();\n\n\t\tstateOrigin = g" +
    "etEventPoint(evt).matrixTransform(stateTf);\n\t} else {\n\t\t// Drag mode\n\t\tstate = \'" +
    "drag\';\n\n\t\tstateTarget = evt.target;\n\n\t\tstateTf = g.getCTM().inverse();\n\n\t\tstateO" +
    "rigin = getEventPoint(evt).matrixTransform(stateTf);\n\t}\n}\n\n/**\n * Handle mouse b" +
    "utton release event.\n */\nfunction handleMouseUp(evt) {\n\tif(evt.preventDefault)\n\t" +
    "\tevt.preventDefault();\n\n\tevt.returnValue = false;\n\n\tvar svgDoc = evt.target.owne" +
    "rDocument;\n\n\tif(state == \'pan\' || state == \'drag\') {\n\t\t// Quit pan mode\n\t\tstate " +
    "= \'\';\n\t}\n}\n";


} // end svgpan_package
