// A D3.js plugin that produces flame graphs from hierarchical data.
// https://github.com/spiermar/d3-flame-graph
// Version 2.0.0-alpha4
// See LICENSE file for license details

// package d3flamegraph -- go2cs converted at 2022 March 13 06:37:12 UTC
// import "cmd/vendor/github.com/google/pprof/third_party/d3flamegraph" ==> using d3flamegraph = go.cmd.vendor.github.com.google.pprof.third_party.d3flamegraph_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\third_party\d3flamegraph\d3_flame_graph.go
namespace go.cmd.vendor.github.com.google.pprof.third_party;

public static partial class d3flamegraph_package {

// JSSource returns the d3-flamegraph.js file
public static readonly @string JSSource = "\n(function (global, factory) {\n\ttypeof exports === \'object\' && typeof module !== " +
    "\'undefined\' ? factory(exports, require(\'d3\')) :\n\ttypeof define === \'function\' &&" +
    " define.amd ? define([\'exports\', \'d3\'], factory) :\n\t(factory((global.d3 = global" +
    ".d3 || {}),global.d3));\n}(this, (function (exports,d3) { \'use strict\';\n\nvar d3__" +
    "default = \'default\' in d3 ? d3[\'default\'] : d3;\n\nvar commonjsGlobal = typeof win" +
    "dow !== \'undefined\' ? window : typeof global !== \'undefined\' ? global : typeof s" +
    "elf !== \'undefined\' ? self : {};\n\n\n\n\n\nfunction createCommonjsModule(fn, module) " +
    "{\n\treturn module = { exports: {} }, fn(module, module.exports), module.exports;\n" +
    "}\n\nvar d3Tip = createCommonjsModule(function (module) {\n// d3.tip\n// Copyright (" +
    "c) 2013 Justin Palmer\n//\n// Tooltips for d3.js SVG visualizations\n\n(function (ro" +
    "ot, factory) {\n  if (typeof undefined === \'function\' && undefined.amd) {\n    // " +
    "AMD. Register as an anonymous module with d3 as a dependency.\n    undefined([\'d3" +
    "\'], factory);\n  } else if (\'object\' === \'object\' && module.exports) {\n    // Com" +
    "monJS\n    var d3$$1 = d3__default;\n    module.exports = factory(d3$$1);\n  } else" +
    " {\n    // Browser global.\n    root.d3.tip = factory(root.d3);\n  }\n}(commonjsGlob" +
    "al, function (d3$$1) {\n\n  // Public - contructs a new tooltip\n  //\n  // Returns " +
    "a tip\n  return function() {\n    var direction = d3_tip_direction,\n        offset" +
    "    = d3_tip_offset,\n        html      = d3_tip_html,\n        node      = initNo" +
    "de(),\n        svg       = null,\n        point     = null,\n        target    = nu" +
    "ll;\n\n    function tip(vis) {\n      svg = getSVGNode(vis);\n      point = svg.crea" +
    "teSVGPoint();\n      document.body.appendChild(node);\n    }\n\n    // Public - show" +
    " the tooltip on the screen\n    //\n    // Returns a tip\n    tip.show = function()" +
    " {\n      var args = Array.prototype.slice.call(arguments);\n      if(args[args.le" +
    "ngth - 1] instanceof SVGElement) target = args.pop();\n\n      var content = html." +
    "apply(this, args),\n          poffset = offset.apply(this, args),\n          dir  " +
    "   = direction.apply(this, args),\n          nodel   = getNodeEl(),\n          i  " +
    "     = directions.length,\n          coords,\n          scrollTop  = document.docu" +
    "mentElement.scrollTop || document.body.scrollTop,\n          scrollLeft = documen" +
    "t.documentElement.scrollLeft || document.body.scrollLeft;\n\n      nodel.html(cont" +
    "ent)\n        .style(\'opacity\', 1).style(\'pointer-events\', \'all\');\n\n      while(i" +
    "--) nodel.classed(directions[i], false);\n      coords = direction_callbacks.get(" +
    "dir).apply(this);\n      nodel.classed(dir, true)\n      \t.style(\'top\', (coords.to" +
    "p +  poffset[0]) + scrollTop + \'px\')\n      \t.style(\'left\', (coords.left + poffse" +
    "t[1]) + scrollLeft + \'px\');\n\n      return tip;\n    };\n\n    // Public - hide the " +
    "tooltip\n    //\n    // Returns a tip\n    tip.hide = function() {\n      var nodel " +
    "= getNodeEl();\n      nodel.style(\'opacity\', 0).style(\'pointer-events\', \'none\');\n" +
    "      return tip\n    };\n\n    // Public: Proxy attr calls to the d3 tip container" +
    ".  Sets or gets attribute value.\n    //\n    // n - name of the attribute\n    // " +
    "v - value of the attribute\n    //\n    // Returns tip or attribute value\n    tip." +
    "attr = function(n, v) {\n      if (arguments.length < 2 && typeof n === \'string\')" +
    " {\n        return getNodeEl().attr(n)\n      } else {\n        var args =  Array.p" +
    "rototype.slice.call(arguments);\n        d3$$1.selection.prototype.attr.apply(get" +
    "NodeEl(), args);\n      }\n\n      return tip\n    };\n\n    // Public: Proxy style ca" +
    "lls to the d3 tip container.  Sets or gets a style value.\n    //\n    // n - name" +
    " of the property\n    // v - value of the property\n    //\n    // Returns tip or s" +
    "tyle property value\n    tip.style = function(n, v) {\n      if (arguments.length " +
    "< 2 && typeof n === \'string\') {\n        return getNodeEl().style(n)\n      } else" +
    " {\n        var args = Array.prototype.slice.call(arguments);\n        d3$$1.selec" +
    "tion.prototype.style.apply(getNodeEl(), args);\n      }\n\n      return tip\n    };\n" +
    "\n    // Public: Set or get the direction of the tooltip\n    //\n    // v - One of" +
    " n(north), s(south), e(east), or w(west), nw(northwest),\n    //     sw(southwest" +
    "), ne(northeast) or se(southeast)\n    //\n    // Returns tip or direction\n    tip" +
    ".direction = function(v) {\n      if (!arguments.length) return direction\n      d" +
    "irection = v == null ? v : functor(v);\n\n      return tip\n    };\n\n    // Public: " +
    "Sets or gets the offset of the tip\n    //\n    // v - Array of [x, y] offset\n    " +
    "//\n    // Returns offset or\n    tip.offset = function(v) {\n      if (!arguments." +
    "length) return offset\n      offset = v == null ? v : functor(v);\n\n      return t" +
    "ip\n    };\n\n    // Public: sets or gets the html value of the tooltip\n    //\n    " +
    "// v - String value of the tip\n    //\n    // Returns html value or tip\n    tip.h" +
    "tml = function(v) {\n      if (!arguments.length) return html\n      html = v == n" +
    "ull ? v : functor(v);\n\n      return tip\n    };\n\n    // Public: destroys the tool" +
    "tip and removes it from the DOM\n    //\n    // Returns a tip\n    tip.destroy = fu" +
    "nction() {\n      if(node) {\n        getNodeEl().remove();\n        node = null;\n " +
    "     }\n      return tip;\n    };\n\n    function d3_tip_direction() { return \'n\' }\n" +
    "    function d3_tip_offset() { return [0, 0] }\n    function d3_tip_html() { retu" +
    "rn \' \' }\n\n    var direction_callbacks = d3$$1.map({\n      n:  direction_n,\n     " +
    " s:  direction_s,\n      e:  direction_e,\n      w:  direction_w,\n      nw: direct" +
    "ion_nw,\n      ne: direction_ne,\n      sw: direction_sw,\n      se: direction_se\n " +
    "   }),\n\n    directions = direction_callbacks.keys();\n\n    function direction_n()" +
    " {\n      var bbox = getScreenBBox();\n      return {\n        top:  bbox.n.y - nod" +
    "e.offsetHeight,\n        left: bbox.n.x - node.offsetWidth / 2\n      }\n    }\n\n   " +
    " function direction_s() {\n      var bbox = getScreenBBox();\n      return {\n     " +
    "   top:  bbox.s.y,\n        left: bbox.s.x - node.offsetWidth / 2\n      }\n    }\n\n" +
    "    function direction_e() {\n      var bbox = getScreenBBox();\n      return {\n  " +
    "      top:  bbox.e.y - node.offsetHeight / 2,\n        left: bbox.e.x\n      }\n   " +
    " }\n\n    function direction_w() {\n      var bbox = getScreenBBox();\n      return " +
    "{\n        top:  bbox.w.y - node.offsetHeight / 2,\n        left: bbox.w.x - node." +
    "offsetWidth\n      }\n    }\n\n    function direction_nw() {\n      var bbox = getScr" +
    "eenBBox();\n      return {\n        top:  bbox.nw.y - node.offsetHeight,\n        l" +
    "eft: bbox.nw.x - node.offsetWidth\n      }\n    }\n\n    function direction_ne() {\n " +
    "     var bbox = getScreenBBox();\n      return {\n        top:  bbox.ne.y - node.o" +
    "ffsetHeight,\n        left: bbox.ne.x\n      }\n    }\n\n    function direction_sw() " +
    "{\n      var bbox = getScreenBBox();\n      return {\n        top:  bbox.sw.y,\n    " +
    "    left: bbox.sw.x - node.offsetWidth\n      }\n    }\n\n    function direction_se(" +
    ") {\n      var bbox = getScreenBBox();\n      return {\n        top:  bbox.se.y,\n  " +
    "      left: bbox.e.x\n      }\n    }\n\n    function initNode() {\n      var node = d" +
    "3$$1.select(document.createElement(\'div\'));\n      node.style(\'position\', \'absolu" +
    "te\').style(\'top\', 0).style(\'opacity\', 0)\n      \t.style(\'pointer-events\', \'none\')" +
    ".style(\'box-sizing\', \'border-box\');\n\n      return node.node()\n    }\n\n    functio" +
    "n getSVGNode(el) {\n      el = el.node();\n      if(el.tagName.toLowerCase() === \'" +
    "svg\')\n        return el\n\n      return el.ownerSVGElement\n    }\n\n    function get" +
    "NodeEl() {\n      if(node === null) {\n        node = initNode();\n        // re-ad" +
    "d node to DOM\n        document.body.appendChild(node);\n      }\n      return d3$$" +
    "1.select(node);\n    }\n\n    // Private - gets the screen coordinates of a shape\n " +
    "   //\n    // Given a shape on the screen, will return an SVGPoint for the direct" +
    "ions\n    // n(north), s(south), e(east), w(west), ne(northeast), se(southeast), " +
    "nw(northwest),\n    // sw(southwest).\n    //\n    //    +-+-+\n    //    |   |\n    " +
    "//    +   +\n    //    |   |\n    //    +-+-+\n    //\n    // Returns an Object {n, " +
    "s, e, w, nw, sw, ne, se}\n    function getScreenBBox() {\n      var targetel   = t" +
    "arget || d3$$1.event.target;\n\n      while (\'undefined\' === typeof targetel.getSc" +
    "reenCTM && \'undefined\' === targetel.parentNode) {\n          targetel = targetel." +
    "parentNode;\n      }\n\n      var bbox       = {},\n          matrix     = targetel." +
    "getScreenCTM(),\n          tbbox      = targetel.getBBox(),\n          width      " +
    "= tbbox.width,\n          height     = tbbox.height,\n          x          = tbbox" +
    ".x,\n          y          = tbbox.y;\n\n      point.x = x;\n      point.y = y;\n     " +
    " bbox.nw = point.matrixTransform(matrix);\n      point.x += width;\n      bbox.ne " +
    "= point.matrixTransform(matrix);\n      point.y += height;\n      bbox.se = point." +
    "matrixTransform(matrix);\n      point.x -= width;\n      bbox.sw = point.matrixTra" +
    "nsform(matrix);\n      point.y -= height / 2;\n      bbox.w  = point.matrixTransfo" +
    "rm(matrix);\n      point.x += width;\n      bbox.e = point.matrixTransform(matrix)" +
    ";\n      point.x -= width / 2;\n      point.y -= height / 2;\n      bbox.n = point." +
    "matrixTransform(matrix);\n      point.y += height;\n      bbox.s = point.matrixTra" +
    "nsform(matrix);\n\n      return bbox\n    }\n    \n    // Private - replace D3JS 3.X " +
    "d3.functor() function\n    function functor(v) {\n    \treturn typeof v === \"functi" +
    "on\" ? v : function() {\n        return v\n    \t}\n    }\n\n    return tip\n  };\n\n}));\n" +
    "});\n\nvar flamegraph = function () {\n  var w = 960; // graph width\n  var h = null" +
    "; // graph height\n  var c = 18; // cell height\n  var selection = null; // select" +
    "ion\n  var tooltip = true; // enable tooltip\n  var title = \'\'; // graph title\n  v" +
    "ar transitionDuration = 750;\n  var transitionEase = d3.easeCubic; // tooltip off" +
    "set\n  var sort = false;\n  var inverted = false; // invert the graph direction\n  " +
    "var clickHandler = null;\n  var minFrameSize = 0;\n  var details = null;\n\n  var ti" +
    "p = d3Tip()\n    .direction(\'s\')\n    .offset([8, 0])\n    .attr(\'class\', \'d3-flame" +
    "-graph-tip\')\n    .html(function (d) { return label(d) });\n\n  var svg;\n\n  functio" +
    "n name (d) {\n    return d.data.n || d.data.name\n  }\n\n  function libtype (d) {\n  " +
    "  return d.data.l || d.data.libtype\n  }\n\n  function children (d) {\n    return d." +
    "c || d.children\n  }\n\n  function value (d) {\n    return d.v || d.value\n  }\n\n  var" +
    " label = function (d) {\n    return name(d) + \' (\' + d3.format(\'.3f\')(100 * (d.x1" +
    " - d.x0), 3) + \'%, \' + value(d) + \' samples)\'\n  };\n\n  function setDetails (t) {\n" +
    "    if (details) { details.innerHTML = t; }\n  }\n\n  var colorMapper = function (d" +
    ") {\n    return d.highlight ? \'#E600E6\' : colorHash(name(d), libtype(d))\n  };\n\n  " +
    "function generateHash (name) {\n    // Return a vector (0.0->1.0) that is a hash " +
    "of the input string.\n    // The hash is computed to favor early characters over " +
    "later ones, so\n    // that strings with similar starts have similar vectors. Onl" +
    "y the first\n    // 6 characters are considered.\n    const MAX_CHAR = 6;\n\n    var" +
    " hash = 0;\n    var maxHash = 0;\n    var weight = 1;\n    var mod = 10;\n\n    if (n" +
    "ame) {\n      for (var i = 0; i < name.length; i++) {\n        if (i > MAX_CHAR) {" +
    " break }\n        hash += weight * (name.charCodeAt(i) % mod);\n        maxHash +=" +
    " weight * (mod - 1);\n        weight *= 0.70;\n      }\n      if (maxHash > 0) { ha" +
    "sh = hash / maxHash; }\n    }\n    return hash\n  }\n\n  function colorHash (name, li" +
    "btype) {\n    // Return a color for the given name and library type. The library " +
    "type\n    // selects the hue, and the name is hashed to a color in that hue.\n\n   " +
    " var r;\n    var g;\n    var b;\n\n    // Select hue. Order is important.\n    var hu" +
    "e;\n    if (typeof libtype === \'undefined\' || libtype === \'\') {\n      // default " +
    "when libtype is not in use\n      hue = \'warm\';\n    } else {\n      hue = \'red\';\n " +
    "     if (name.match(/::/)) {\n        hue = \'yellow\';\n      }\n      if (libtype =" +
    "== \'kernel\') {\n        hue = \'orange\';\n      } else if (libtype === \'jit\') {\n   " +
    "     hue = \'green\';\n      } else if (libtype === \'inlined\') {\n        hue = \'aqu" +
    "a\';\n      }\n    }\n\n    // calculate hash\n    var vector = 0;\n    if (name) {\n   " +
    "   var nameArr = name.split(\'" + "`" + "\');\n      if (nameArr.length > 1) {\n        name = nameArr[nameArr.length - 1]; /" +
    "/ drop module name if present\n      }\n      name = name.split(\'(\')[0]; // drop e" +
    "xtra info\n      vector = generateHash(name);\n    }\n\n    // calculate color\n    i" +
    "f (hue === \'red\') {\n      r = 200 + Math.round(55 * vector);\n      g = 50 + Math" +
    ".round(80 * vector);\n      b = g;\n    } else if (hue === \'orange\') {\n      r = 1" +
    "90 + Math.round(65 * vector);\n      g = 90 + Math.round(65 * vector);\n      b = " +
    "0;\n    } else if (hue === \'yellow\') {\n      r = 175 + Math.round(55 * vector);\n " +
    "     g = r;\n      b = 50 + Math.round(20 * vector);\n    } else if (hue === \'gree" +
    "n\') {\n      r = 50 + Math.round(60 * vector);\n      g = 200 + Math.round(55 * ve" +
    "ctor);\n      b = r;\n    } else if (hue === \'aqua\') {\n      r = 50 + Math.round(6" +
    "0 * vector);\n      g = 165 + Math.round(55 * vector);\n      b = g;\n    } else {\n" +
    "      // original warm palette\n      r = 200 + Math.round(55 * vector);\n      g " +
    "= 0 + Math.round(230 * (1 - vector));\n      b = 0 + Math.round(55 * (1 - vector)" +
    ");\n    }\n\n    return \'rgb(\' + r + \',\' + g + \',\' + b + \')\'\n  }\n\n  function hide (" +
    "d) {\n    d.data.hide = true;\n    if (children(d)) {\n      children(d).forEach(hi" +
    "de);\n    }\n  }\n\n  function show (d) {\n    d.data.fade = false;\n    d.data.hide =" +
    " false;\n    if (children(d)) {\n      children(d).forEach(show);\n    }\n  }\n\n  fun" +
    "ction getSiblings (d) {\n    var siblings = [];\n    if (d.parent) {\n      var me " +
    "= d.parent.children.indexOf(d);\n      siblings = d.parent.children.slice(0);\n   " +
    "   siblings.splice(me, 1);\n    }\n    return siblings\n  }\n\n  function hideSibling" +
    "s (d) {\n    var siblings = getSiblings(d);\n    siblings.forEach(function (s) {\n " +
    "     hide(s);\n    });\n    if (d.parent) {\n      hideSiblings(d.parent);\n    }\n  " +
    "}\n\n  function fadeAncestors (d) {\n    if (d.parent) {\n      d.parent.data.fade =" +
    " true;\n      fadeAncestors(d.parent);\n    }\n  }\n\n  // function getRoot (d) {\n  /" +
    "/   if (d.parent) {\n  //     return getRoot(d.parent)\n  //   }\n  //   return d\n " +
    " // }\n\n  function zoom (d) {\n    tip.hide(d);\n    hideSiblings(d);\n    show(d);\n" +
    "    fadeAncestors(d);\n    update();\n    if (typeof clickHandler === \'function\') " +
    "{\n      clickHandler(d);\n    }\n  }\n\n  function searchTree (d, term) {\n    var re" +
    " = new RegExp(term);\n    var searchResults = [];\n\n    function searchInner (d) {" +
    "\n      var label = name(d);\n\n      if (children(d)) {\n        children(d).forEac" +
    "h(function (child) {\n          searchInner(child);\n        });\n      }\n\n      if" +
    " (label.match(re)) {\n        d.highlight = true;\n        searchResults.push(d);\n" +
    "      } else {\n        d.highlight = false;\n      }\n    }\n\n    searchInner(d);\n " +
    "   return searchResults\n  }\n\n  function clear (d) {\n    d.highlight = false;\n   " +
    " if (children(d)) {\n      children(d).forEach(function (child) {\n        clear(c" +
    "hild);\n      });\n    }\n  }\n\n  function doSort (a, b) {\n    if (typeof sort === \'" +
    "function\') {\n      return sort(a, b)\n    } else if (sort) {\n      return d3.asce" +
    "nding(name(a), name(b))\n    }\n  }\n\n  var p = d3.partition();\n\n  function filterN" +
    "odes (root) {\n    var nodeList = root.descendants();\n    if (minFrameSize > 0) {" +
    "\n      var kx = w / (root.x1 - root.x0);\n      nodeList = nodeList.filter(functi" +
    "on (el) {\n        return ((el.x1 - el.x0) * kx) > minFrameSize\n      });\n    }\n " +
    "   return nodeList\n  }\n\n  function update () {\n    selection.each(function (root" +
    ") {\n      var x = d3.scaleLinear().range([0, w]);\n      var y = d3.scaleLinear()" +
    ".range([0, c]);\n\n      if (sort) root.sort(doSort);\n      root.sum(function (d) " +
    "{\n        if (d.fade || d.hide) {\n          return 0\n        }\n        // The no" +
    "de\'s self value is its total value minus all children.\n        var v = value(d);" +
    "\n        if (children(d)) {\n          var c = children(d);\n          for (var i " +
    "= 0; i < c.length; i++) {\n            v -= value(c[i]);\n          }\n        }\n  " +
    "      return v\n      });\n      p(root);\n\n      var kx = w / (root.x1 - root.x0);" +
    "\n      function width (d) { return (d.x1 - d.x0) * kx }\n\n      var descendants =" +
    " filterNodes(root);\n      var g = d3.select(this).select(\'svg\').selectAll(\'g\').d" +
    "ata(descendants, function (d) { return d.id });\n\n      g.transition()\n        .d" +
    "uration(transitionDuration)\n        .ease(transitionEase)\n        .attr(\'transfo" +
    "rm\', function (d) { return \'translate(\' + x(d.x0) + \',\' + (inverted ? y(d.depth)" +
    " : (h - y(d.depth) - c)) + \')\' });\n\n      g.select(\'rect\')\n        .attr(\'width\'" +
    ", width);\n\n      var node = g.enter()\n        .append(\'svg:g\')\n        .attr(\'tr" +
    "ansform\', function (d) { return \'translate(\' + x(d.x0) + \',\' + (inverted ? y(d.d" +
    "epth) : (h - y(d.depth) - c)) + \')\' });\n\n      node.append(\'svg:rect\')\n        ." +
    "transition()\n        .delay(transitionDuration / 2)\n        .attr(\'width\', width" +
    ");\n\n      if (!tooltip) { node.append(\'svg:title\'); }\n\n      node.append(\'foreig" +
    "nObject\')\n        .append(\'xhtml:div\');\n\n      // Now we have to re-select to se" +
    "e the new elements (why?).\n      g = d3.select(this).select(\'svg\').selectAll(\'g\'" +
    ").data(descendants, function (d) { return d.id });\n\n      g.attr(\'width\', width)" +
    "\n        .attr(\'height\', function (d) { return c })\n        .attr(\'name\', functi" +
    "on (d) { return name(d) })\n        .attr(\'class\', function (d) { return d.data.f" +
    "ade ? \'frame fade\' : \'frame\' });\n\n      g.select(\'rect\')\n        .attr(\'height\'," +
    " function (d) { return c })\n        .attr(\'fill\', function (d) { return colorMap" +
    "per(d) });\n\n      if (!tooltip) {\n        g.select(\'title\')\n          .text(labe" +
    "l);\n      }\n\n      g.select(\'foreignObject\')\n        .attr(\'width\', width)\n     " +
    "   .attr(\'height\', function (d) { return c })\n        .select(\'div\')\n        .at" +
    "tr(\'class\', \'d3-flame-graph-label\')\n        .style(\'display\', function (d) { ret" +
    "urn (width(d) < 35) ? \'none\' : \'block\' })\n        .transition()\n        .delay(t" +
    "ransitionDuration)\n        .text(name);\n\n      g.on(\'click\', zoom);\n\n      g.exi" +
    "t()\n        .remove();\n\n      g.on(\'mouseover\', function (d) {\n        if (toolt" +
    "ip) tip.show(d, this);\n        setDetails(label(d));\n      }).on(\'mouseout\', fun" +
    "ction (d) {\n        if (tooltip) tip.hide(d);\n        setDetails(\'\');\n      });\n" +
    "    });\n  }\n\n  function merge (data, samples) {\n    samples.forEach(function (sa" +
    "mple) {\n      var node = data.find(function (element) {\n        return (element." +
    "name === sample.name)\n      });\n\n      if (node) {\n        if (node.original) {\n" +
    "          node.original += sample.value;\n        } else {\n          node.value +" +
    "= sample.value;\n        }\n        if (sample.children) {\n          if (!node.chi" +
    "ldren) {\n            node.children = [];\n          }\n          merge(node.childr" +
    "en, sample.children);\n        }\n      } else {\n        data.push(sample);\n      " +
    "}\n    });\n  }\n\n  function s4 () {\n    return Math.floor((1 + Math.random()) * 0x" +
    "10000)\n      .toString(16)\n      .substring(1)\n  }\n\n  function injectIds (node) " +
    "{\n    node.id = s4() + \'-\' + s4() + \'-\' + \'-\' + s4() + \'-\' + s4();\n    var child" +
    "ren = node.c || node.children || [];\n    for (var i = 0; i < children.length; i+" +
    "+) {\n      injectIds(children[i]);\n    }\n  }\n\n  function chart (s) {\n    var roo" +
    "t = d3.hierarchy(\n      s.datum(), function (d) { return children(d) }\n    );\n  " +
    "  injectIds(root);\n    selection = s.datum(root);\n\n    if (!arguments.length) re" +
    "turn chart\n\n    if (!h) {\n      h = (root.height + 2) * c;\n    }\n\n    selection." +
    "each(function (data) {\n      if (!svg) {\n        svg = d3.select(this)\n         " +
    " .append(\'svg:svg\')\n          .attr(\'width\', w)\n          .attr(\'height\', h)\n   " +
    "       .attr(\'class\', \'partition d3-flame-graph\')\n          .call(tip);\n\n       " +
    " svg.append(\'svg:text\')\n          .attr(\'class\', \'title\')\n          .attr(\'text-" +
    "anchor\', \'middle\')\n          .attr(\'y\', \'25\')\n          .attr(\'x\', w / 2)\n      " +
    "    .attr(\'fill\', \'#808080\')\n          .text(title);\n      }\n    });\n\n    // fir" +
    "st draw\n    update();\n  }\n\n  chart.height = function (_) {\n    if (!arguments.le" +
    "ngth) { return h }\n    h = _;\n    return chart\n  };\n\n  chart.width = function (_" +
    ") {\n    if (!arguments.length) { return w }\n    w = _;\n    return chart\n  };\n\n  " +
    "chart.cellHeight = function (_) {\n    if (!arguments.length) { return c }\n    c " +
    "= _;\n    return chart\n  };\n\n  chart.tooltip = function (_) {\n    if (!arguments." +
    "length) { return tooltip }\n    if (typeof _ === \'function\') {\n      tip = _;\n   " +
    " }\n    tooltip = !!_;\n    return chart\n  };\n\n  chart.title = function (_) {\n    " +
    "if (!arguments.length) { return title }\n    title = _;\n    return chart\n  };\n\n  " +
    "chart.transitionDuration = function (_) {\n    if (!arguments.length) { return tr" +
    "ansitionDuration }\n    transitionDuration = _;\n    return chart\n  };\n\n  chart.tr" +
    "ansitionEase = function (_) {\n    if (!arguments.length) { return transitionEase" +
    " }\n    transitionEase = _;\n    return chart\n  };\n\n  chart.sort = function (_) {\n" +
    "    if (!arguments.length) { return sort }\n    sort = _;\n    return chart\n  };\n\n" +
    "  chart.inverted = function (_) {\n    if (!arguments.length) { return inverted }" +
    "\n    inverted = _;\n    return chart\n  };\n\n  chart.label = function (_) {\n    if " +
    "(!arguments.length) { return label }\n    label = _;\n    return chart\n  };\n\n  cha" +
    "rt.search = function (term) {\n    var searchResults = [];\n    selection.each(fun" +
    "ction (data) {\n      searchResults = searchTree(data, term);\n      update();\n   " +
    " });\n    return searchResults\n  };\n\n  chart.clear = function () {\n    selection." +
    "each(function (data) {\n      clear(data);\n      update();\n    });\n  };\n\n  chart." +
    "zoomTo = function (d) {\n    zoom(d);\n  };\n\n  chart.resetZoom = function () {\n   " +
    " selection.each(function (data) {\n      zoom(data); // zoom to root\n    });\n  };" +
    "\n\n  chart.onClick = function (_) {\n    if (!arguments.length) {\n      return cli" +
    "ckHandler\n    }\n    clickHandler = _;\n    return chart\n  };\n\n  chart.merge = fun" +
    "ction (samples) {\n    var newRoot; // Need to re-create hierarchy after data cha" +
    "nges.\n    selection.each(function (root) {\n      merge([root.data], [samples]);\n" +
    "      newRoot = d3.hierarchy(root.data, function (d) { return children(d) });\n  " +
    "    injectIds(newRoot);\n    });\n    selection = selection.datum(newRoot);\n    up" +
    "date();\n  };\n\n  chart.color = function (_) {\n    if (!arguments.length) { return" +
    " colorMapper }\n    colorMapper = _;\n    return chart\n  };\n\n  chart.minFrameSize " +
    "= function (_) {\n    if (!arguments.length) { return minFrameSize }\n    minFrame" +
    "Size = _;\n    return chart\n  };\n\n  chart.details = function (_) {\n    if (!argum" +
    "ents.length) { return details }\n    details = _;\n    return chart\n  };\n\n  return" +
    " chart\n};\n\nexports.flamegraph = flamegraph;\n\nObject.defineProperty(exports, \'__e" +
    "sModule\', { value: true });\n\n})));\n";

// CSSSource returns the d3-flamegraph.css file


// CSSSource returns the d3-flamegraph.css file
public static readonly @string CSSSource = "\n.d3-flame-graph rect {\n  stroke: #EEEEEE;\n  fill-opacity: .8;\n}\n\n.d3-flame-graph" +
    " rect:hover {\n  stroke: #474747;\n  stroke-width: 0.5;\n  cursor: pointer;\n}\n\n.d3-" +
    "flame-graph-label {\n  pointer-events: none;\n  white-space: nowrap;\n  text-overfl" +
    "ow: ellipsis;\n  overflow: hidden;\n  font-size: 12px;\n  font-family: Verdana;\n  m" +
    "argin-left: 4px;\n  margin-right: 4px;\n  line-height: 1.5;\n  padding: 0 0 0;\n  fo" +
    "nt-weight: 400;\n  color: black;\n  text-align: left;\n}\n\n.d3-flame-graph .fade {\n " +
    " opacity: 0.6 !important;\n}\n\n.d3-flame-graph .title {\n  font-size: 20px;\n  font-" +
    "family: Verdana;\n}\n\n.d3-flame-graph-tip {\n  line-height: 1;\n  font-family: Verda" +
    "na;\n  font-size: 12px;\n  padding: 12px;\n  background: rgba(0, 0, 0, 0.8);\n  colo" +
    "r: #fff;\n  border-radius: 2px;\n  pointer-events: none;\n}\n\n/* Creates a small tri" +
    "angle extender for the tooltip */\n.d3-flame-graph-tip:after {\n  box-sizing: bord" +
    "er-box;\n  display: inline;\n  font-size: 10px;\n  width: 100%;\n  line-height: 1;\n " +
    " color: rgba(0, 0, 0, 0.8);\n  position: absolute;\n  pointer-events: none;\n}\n\n/* " +
    "Northward tooltips */\n.d3-flame-graph-tip.n:after {\n  content: \"\\25BC\";\n  margin" +
    ": -1px 0 0 0;\n  top: 100%;\n  left: 0;\n  text-align: center;\n}\n\n/* Eastward toolt" +
    "ips */\n.d3-flame-graph-tip.e:after {\n  content: \"\\25C0\";\n  margin: -4px 0 0 0;\n " +
    " top: 50%;\n  left: -8px;\n}\n\n/* Southward tooltips */\n.d3-flame-graph-tip.s:after" +
    " {\n  content: \"\\25B2\";\n  margin: 0 0 1px 0;\n  top: -8px;\n  left: 0;\n  text-align" +
    ": center;\n}\n\n/* Westward tooltips */\n.d3-flame-graph-tip.w:after {\n  content: \"\\" +
    "25B6\";\n  margin: -4px 0 0 -1px;\n  top: 50%;\n  left: 100%;\n}\n";


} // end d3flamegraph_package
