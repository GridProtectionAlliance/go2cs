// D3.js is a JavaScript library for manipulating documents based on data.
// https://github.com/d3/d3
// See LICENSE file for license details
// Custom build for pprof (https://github.com/spiermar/d3-pprof)

// package d3 -- go2cs converted at 2022 March 06 23:24:05 UTC
// import "cmd/vendor/github.com/google/pprof/third_party/d3" ==> using d3 = go.cmd.vendor.github.com.google.pprof.third_party.d3_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\third_party\d3\d3.go


namespace go.cmd.vendor.github.com.google.pprof.third_party;

public static partial class d3_package {

    // JSSource returns the d3.js file
public static readonly @string JSSource = "\n(function (global, factory) {\n\ttypeof exports === \'object\' && typeof module !== " +
    "\'undefined\' ? factory(exports) :\n\ttypeof define === \'function\' && define.amd ? d" +
    "efine([\'exports\'], factory) :\n\t(factory((global.d3 = {})));\n}(this, (function (e" +
    "xports) { \'use strict\';\n\nvar xhtml = \"http://www.w3.org/1999/xhtml\";\n\nvar namesp" +
    "aces = {\n  svg: \"http://www.w3.org/2000/svg\",\n  xhtml: xhtml,\n  xlink: \"http://w" +
    "ww.w3.org/1999/xlink\",\n  xml: \"http://www.w3.org/XML/1998/namespace\",\n  xmlns: \"" +
    "http://www.w3.org/2000/xmlns/\"\n};\n\nvar namespace = function(name) {\n  var prefix" +
    " = name += \"\", i = prefix.indexOf(\":\");\n  if (i >= 0 && (prefix = name.slice(0, " +
    "i)) !== \"xmlns\") name = name.slice(i + 1);\n  return namespaces.hasOwnProperty(pr" +
    "efix) ? {space: namespaces[prefix], local: name} : name;\n};\n\nfunction creatorInh" +
    "erit(name) {\n  return function() {\n    var document = this.ownerDocument,\n      " +
    "  uri = this.namespaceURI;\n    return uri === xhtml && document.documentElement." +
    "namespaceURI === xhtml\n        ? document.createElement(name)\n        : document" +
    ".createElementNS(uri, name);\n  };\n}\n\nfunction creatorFixed(fullname) {\n  return " +
    "function() {\n    return this.ownerDocument.createElementNS(fullname.space, fulln" +
    "ame.local);\n  };\n}\n\nvar creator = function(name) {\n  var fullname = namespace(na" +
    "me);\n  return (fullname.local\n      ? creatorFixed\n      : creatorInherit)(fulln" +
    "ame);\n};\n\nvar matcher = function(selector) {\n  return function() {\n    return th" +
    "is.matches(selector);\n  };\n};\n\nif (typeof document !== \"undefined\") {\n  var elem" +
    "ent = document.documentElement;\n  if (!element.matches) {\n    var vendorMatches " +
    "= element.webkitMatchesSelector\n        || element.msMatchesSelector\n        || " +
    "element.mozMatchesSelector\n        || element.oMatchesSelector;\n    matcher = fu" +
    "nction(selector) {\n      return function() {\n        return vendorMatches.call(t" +
    "his, selector);\n      };\n    };\n  }\n}\n\nvar matcher$1 = matcher;\n\nvar filterEvent" +
    "s = {};\n\nexports.event = null;\n\nif (typeof document !== \"undefined\") {\n  var ele" +
    "ment$1 = document.documentElement;\n  if (!(\"onmouseenter\" in element$1)) {\n    f" +
    "ilterEvents = {mouseenter: \"mouseover\", mouseleave: \"mouseout\"};\n  }\n}\n\nfunction" +
    " filterContextListener(listener, index, group) {\n  listener = contextListener(li" +
    "stener, index, group);\n  return function(event) {\n    var related = event.relate" +
    "dTarget;\n    if (!related || (related !== this && !(related.compareDocumentPosit" +
    "ion(this) & 8))) {\n      listener.call(this, event);\n    }\n  };\n}\n\nfunction cont" +
    "extListener(listener, index, group) {\n  return function(event1) {\n    var event0" +
    " = exports.event; // Events can be reentrant (e.g., focus).\n    exports.event = " +
    "event1;\n    try {\n      listener.call(this, this.__data__, index, group);\n    } " +
    "finally {\n      exports.event = event0;\n    }\n  };\n}\n\nfunction parseTypenames(ty" +
    "penames) {\n  return typenames.trim().split(/^|\\s+/).map(function(t) {\n    var na" +
    "me = \"\", i = t.indexOf(\".\");\n    if (i >= 0) name = t.slice(i + 1), t = t.slice(" +
    "0, i);\n    return {type: t, name: name};\n  });\n}\n\nfunction onRemove(typename) {\n" +
    "  return function() {\n    var on = this.__on;\n    if (!on) return;\n    for (var " +
    "j = 0, i = -1, m = on.length, o; j < m; ++j) {\n      if (o = on[j], (!typename.t" +
    "ype || o.type === typename.type) && o.name === typename.name) {\n        this.rem" +
    "oveEventListener(o.type, o.listener, o.capture);\n      } else {\n        on[++i] " +
    "= o;\n      }\n    }\n    if (++i) on.length = i;\n    else delete this.__on;\n  };\n}" +
    "\n\nfunction onAdd(typename, value, capture) {\n  var wrap = filterEvents.hasOwnPro" +
    "perty(typename.type) ? filterContextListener : contextListener;\n  return functio" +
    "n(d, i, group) {\n    var on = this.__on, o, listener = wrap(value, i, group);\n  " +
    "  if (on) for (var j = 0, m = on.length; j < m; ++j) {\n      if ((o = on[j]).typ" +
    "e === typename.type && o.name === typename.name) {\n        this.removeEventListe" +
    "ner(o.type, o.listener, o.capture);\n        this.addEventListener(o.type, o.list" +
    "ener = listener, o.capture = capture);\n        o.value = value;\n        return;\n" +
    "      }\n    }\n    this.addEventListener(typename.type, listener, capture);\n    o" +
    " = {type: typename.type, name: typename.name, value: value, listener: listener, " +
    "capture: capture};\n    if (!on) this.__on = [o];\n    else on.push(o);\n  };\n}\n\nva" +
    "r selection_on = function(typename, value, capture) {\n  var typenames = parseTyp" +
    "enames(typename + \"\"), i, n = typenames.length, t;\n\n  if (arguments.length < 2) " +
    "{\n    var on = this.node().__on;\n    if (on) for (var j = 0, m = on.length, o; j" +
    " < m; ++j) {\n      for (i = 0, o = on[j]; i < n; ++i) {\n        if ((t = typenam" +
    "es[i]).type === o.type && t.name === o.name) {\n          return o.value;\n       " +
    " }\n      }\n    }\n    return;\n  }\n\n  on = value ? onAdd : onRemove;\n  if (capture" +
    " == null) capture = false;\n  for (i = 0; i < n; ++i) this.each(on(typenames[i], " +
    "value, capture));\n  return this;\n};\n\nfunction none() {}\n\nvar selector = function" +
    "(selector) {\n  return selector == null ? none : function() {\n    return this.que" +
    "rySelector(selector);\n  };\n};\n\nvar selection_select = function(select) {\n  if (t" +
    "ypeof select !== \"function\") select = selector(select);\n\n  for (var groups = thi" +
    "s._groups, m = groups.length, subgroups = new Array(m), j = 0; j < m; ++j) {\n   " +
    " for (var group = groups[j], n = group.length, subgroup = subgroups[j] = new Arr" +
    "ay(n), node, subnode, i = 0; i < n; ++i) {\n      if ((node = group[i]) && (subno" +
    "de = select.call(node, node.__data__, i, group))) {\n        if (\"__data__\" in no" +
    "de) subnode.__data__ = node.__data__;\n        subgroup[i] = subnode;\n      }\n   " +
    " }\n  }\n\n  return new Selection(subgroups, this._parents);\n};\n\nfunction empty() {" +
    "\n  return [];\n}\n\nvar selectorAll = function(selector) {\n  return selector == nul" +
    "l ? empty : function() {\n    return this.querySelectorAll(selector);\n  };\n};\n\nva" +
    "r selection_selectAll = function(select) {\n  if (typeof select !== \"function\") s" +
    "elect = selectorAll(select);\n\n  for (var groups = this._groups, m = groups.lengt" +
    "h, subgroups = [], parents = [], j = 0; j < m; ++j) {\n    for (var group = group" +
    "s[j], n = group.length, node, i = 0; i < n; ++i) {\n      if (node = group[i]) {\n" +
    "        subgroups.push(select.call(node, node.__data__, i, group));\n        pare" +
    "nts.push(node);\n      }\n    }\n  }\n\n  return new Selection(subgroups, parents);\n}" +
    ";\n\nvar selection_filter = function(match) {\n  if (typeof match !== \"function\") m" +
    "atch = matcher$1(match);\n\n  for (var groups = this._groups, m = groups.length, s" +
    "ubgroups = new Array(m), j = 0; j < m; ++j) {\n    for (var group = groups[j], n " +
    "= group.length, subgroup = subgroups[j] = [], node, i = 0; i < n; ++i) {\n      i" +
    "f ((node = group[i]) && match.call(node, node.__data__, i, group)) {\n        sub" +
    "group.push(node);\n      }\n    }\n  }\n\n  return new Selection(subgroups, this._par" +
    "ents);\n};\n\nvar sparse = function(update) {\n  return new Array(update.length);\n};" +
    "\n\nvar selection_enter = function() {\n  return new Selection(this._enter || this." +
    "_groups.map(sparse), this._parents);\n};\n\nfunction EnterNode(parent, datum) {\n  t" +
    "his.ownerDocument = parent.ownerDocument;\n  this.namespaceURI = parent.namespace" +
    "URI;\n  this._next = null;\n  this._parent = parent;\n  this.__data__ = datum;\n}\n\nE" +
    "nterNode.prototype = {\n  constructor: EnterNode,\n  appendChild: function(child) " +
    "{ return this._parent.insertBefore(child, this._next); },\n  insertBefore: functi" +
    "on(child, next) { return this._parent.insertBefore(child, next); },\n  querySelec" +
    "tor: function(selector) { return this._parent.querySelector(selector); },\n  quer" +
    "ySelectorAll: function(selector) { return this._parent.querySelectorAll(selector" +
    "); }\n};\n\nvar constant = function(x) {\n  return function() {\n    return x;\n  };\n}" +
    ";\n\nvar keyPrefix = \"$\"; // Protect against keys like “__proto__”.\n\nfunction bind" +
    "Index(parent, group, enter, update, exit, data) {\n  var i = 0,\n      node,\n     " +
    " groupLength = group.length,\n      dataLength = data.length;\n\n  // Put any non-n" +
    "ull nodes that fit into update.\n  // Put any null nodes into enter.\n  // Put any" +
    " remaining data into enter.\n  for (; i < dataLength; ++i) {\n    if (node = group" +
    "[i]) {\n      node.__data__ = data[i];\n      update[i] = node;\n    } else {\n     " +
    " enter[i] = new EnterNode(parent, data[i]);\n    }\n  }\n\n  // Put any non-null nod" +
    "es that don’t fit into exit.\n  for (; i < groupLength; ++i) {\n    if (node = gro" +
    "up[i]) {\n      exit[i] = node;\n    }\n  }\n}\n\nfunction bindKey(parent, group, ente" +
    "r, update, exit, data, key) {\n  var i,\n      node,\n      nodeByKeyValue = {},\n  " +
    "    groupLength = group.length,\n      dataLength = data.length,\n      keyValues " +
    "= new Array(groupLength),\n      keyValue;\n\n  // Compute the key for each node.\n " +
    " // If multiple nodes have the same key, the duplicates are added to exit.\n  for" +
    " (i = 0; i < groupLength; ++i) {\n    if (node = group[i]) {\n      keyValues[i] =" +
    " keyValue = keyPrefix + key.call(node, node.__data__, i, group);\n      if (keyVa" +
    "lue in nodeByKeyValue) {\n        exit[i] = node;\n      } else {\n        nodeByKe" +
    "yValue[keyValue] = node;\n      }\n    }\n  }\n\n  // Compute the key for each datum." +
    "\n  // If there a node associated with this key, join and add it to update.\n  // " +
    "If there is not (or the key is a duplicate), add it to enter.\n  for (i = 0; i < " +
    "dataLength; ++i) {\n    keyValue = keyPrefix + key.call(parent, data[i], i, data)" +
    ";\n    if (node = nodeByKeyValue[keyValue]) {\n      update[i] = node;\n      node." +
    "__data__ = data[i];\n      nodeByKeyValue[keyValue] = null;\n    } else {\n      en" +
    "ter[i] = new EnterNode(parent, data[i]);\n    }\n  }\n\n  // Add any remaining nodes" +
    " that were not bound to data to exit.\n  for (i = 0; i < groupLength; ++i) {\n    " +
    "if ((node = group[i]) && (nodeByKeyValue[keyValues[i]] === node)) {\n      exit[i" +
    "] = node;\n    }\n  }\n}\n\nvar selection_data = function(value, key) {\n  if (!value)" +
    " {\n    data = new Array(this.size()), j = -1;\n    this.each(function(d) { data[+" +
    "+j] = d; });\n    return data;\n  }\n\n  var bind = key ? bindKey : bindIndex,\n     " +
    " parents = this._parents,\n      groups = this._groups;\n\n  if (typeof value !== \"" +
    "function\") value = constant(value);\n\n  for (var m = groups.length, update = new " +
    "Array(m), enter = new Array(m), exit = new Array(m), j = 0; j < m; ++j) {\n    va" +
    "r parent = parents[j],\n        group = groups[j],\n        groupLength = group.le" +
    "ngth,\n        data = value.call(parent, parent && parent.__data__, j, parents),\n" +
    "        dataLength = data.length,\n        enterGroup = enter[j] = new Array(data" +
    "Length),\n        updateGroup = update[j] = new Array(dataLength),\n        exitGr" +
    "oup = exit[j] = new Array(groupLength);\n\n    bind(parent, group, enterGroup, upd" +
    "ateGroup, exitGroup, data, key);\n\n    // Now connect the enter nodes to their fo" +
    "llowing update node, such that\n    // appendChild can insert the materialized en" +
    "ter node before this node,\n    // rather than at the end of the parent node.\n   " +
    " for (var i0 = 0, i1 = 0, previous, next; i0 < dataLength; ++i0) {\n      if (pre" +
    "vious = enterGroup[i0]) {\n        if (i0 >= i1) i1 = i0 + 1;\n        while (!(ne" +
    "xt = updateGroup[i1]) && ++i1 < dataLength);\n        previous._next = next || nu" +
    "ll;\n      }\n    }\n  }\n\n  update = new Selection(update, parents);\n  update._ente" +
    "r = enter;\n  update._exit = exit;\n  return update;\n};\n\nvar selection_exit = func" +
    "tion() {\n  return new Selection(this._exit || this._groups.map(sparse), this._pa" +
    "rents);\n};\n\nvar selection_merge = function(selection$$1) {\n\n  for (var groups0 =" +
    " this._groups, groups1 = selection$$1._groups, m0 = groups0.length, m1 = groups1" +
    ".length, m = Math.min(m0, m1), merges = new Array(m0), j = 0; j < m; ++j) {\n    " +
    "for (var group0 = groups0[j], group1 = groups1[j], n = group0.length, merge = me" +
    "rges[j] = new Array(n), node, i = 0; i < n; ++i) {\n      if (node = group0[i] ||" +
    " group1[i]) {\n        merge[i] = node;\n      }\n    }\n  }\n\n  for (; j < m0; ++j) " +
    "{\n    merges[j] = groups0[j];\n  }\n\n  return new Selection(merges, this._parents)" +
    ";\n};\n\nvar selection_order = function() {\n\n  for (var groups = this._groups, j = " +
    "-1, m = groups.length; ++j < m;) {\n    for (var group = groups[j], i = group.len" +
    "gth - 1, next = group[i], node; --i >= 0;) {\n      if (node = group[i]) {\n      " +
    "  if (next && next !== node.nextSibling) next.parentNode.insertBefore(node, next" +
    ");\n        next = node;\n      }\n    }\n  }\n\n  return this;\n};\n\nvar selection_sort" +
    " = function(compare) {\n  if (!compare) compare = ascending;\n\n  function compareN" +
    "ode(a, b) {\n    return a && b ? compare(a.__data__, b.__data__) : !a - !b;\n  }\n\n" +
    "  for (var groups = this._groups, m = groups.length, sortgroups = new Array(m), " +
    "j = 0; j < m; ++j) {\n    for (var group = groups[j], n = group.length, sortgroup" +
    " = sortgroups[j] = new Array(n), node, i = 0; i < n; ++i) {\n      if (node = gro" +
    "up[i]) {\n        sortgroup[i] = node;\n      }\n    }\n    sortgroup.sort(compareNo" +
    "de);\n  }\n\n  return new Selection(sortgroups, this._parents).order();\n};\n\nfunctio" +
    "n ascending(a, b) {\n  return a < b ? -1 : a > b ? 1 : a >= b ? 0 : NaN;\n}\n\nvar s" +
    "election_call = function() {\n  var callback = arguments[0];\n  arguments[0] = thi" +
    "s;\n  callback.apply(null, arguments);\n  return this;\n};\n\nvar selection_nodes = f" +
    "unction() {\n  var nodes = new Array(this.size()), i = -1;\n  this.each(function()" +
    " { nodes[++i] = this; });\n  return nodes;\n};\n\nvar selection_node = function() {\n" +
    "\n  for (var groups = this._groups, j = 0, m = groups.length; j < m; ++j) {\n    f" +
    "or (var group = groups[j], i = 0, n = group.length; i < n; ++i) {\n      var node" +
    " = group[i];\n      if (node) return node;\n    }\n  }\n\n  return null;\n};\n\nvar sele" +
    "ction_size = function() {\n  var size = 0;\n  this.each(function() { ++size; });\n " +
    " return size;\n};\n\nvar selection_empty = function() {\n  return !this.node();\n};\n\n" +
    "var selection_each = function(callback) {\n\n  for (var groups = this._groups, j =" +
    " 0, m = groups.length; j < m; ++j) {\n    for (var group = groups[j], i = 0, n = " +
    "group.length, node; i < n; ++i) {\n      if (node = group[i]) callback.call(node," +
    " node.__data__, i, group);\n    }\n  }\n\n  return this;\n};\n\nfunction attrRemove(nam" +
    "e) {\n  return function() {\n    this.removeAttribute(name);\n  };\n}\n\nfunction attr" +
    "RemoveNS(fullname) {\n  return function() {\n    this.removeAttributeNS(fullname.s" +
    "pace, fullname.local);\n  };\n}\n\nfunction attrConstant(name, value) {\n  return fun" +
    "ction() {\n    this.setAttribute(name, value);\n  };\n}\n\nfunction attrConstantNS(fu" +
    "llname, value) {\n  return function() {\n    this.setAttributeNS(fullname.space, f" +
    "ullname.local, value);\n  };\n}\n\nfunction attrFunction(name, value) {\n  return fun" +
    "ction() {\n    var v = value.apply(this, arguments);\n    if (v == null) this.remo" +
    "veAttribute(name);\n    else this.setAttribute(name, v);\n  };\n}\n\nfunction attrFun" +
    "ctionNS(fullname, value) {\n  return function() {\n    var v = value.apply(this, a" +
    "rguments);\n    if (v == null) this.removeAttributeNS(fullname.space, fullname.lo" +
    "cal);\n    else this.setAttributeNS(fullname.space, fullname.local, v);\n  };\n}\n\nv" +
    "ar selection_attr = function(name, value) {\n  var fullname = namespace(name);\n\n " +
    " if (arguments.length < 2) {\n    var node = this.node();\n    return fullname.loc" +
    "al\n        ? node.getAttributeNS(fullname.space, fullname.local)\n        : node." +
    "getAttribute(fullname);\n  }\n\n  return this.each((value == null\n      ? (fullname" +
    ".local ? attrRemoveNS : attrRemove) : (typeof value === \"function\"\n      ? (full" +
    "name.local ? attrFunctionNS : attrFunction)\n      : (fullname.local ? attrConsta" +
    "ntNS : attrConstant)))(fullname, value));\n};\n\nvar defaultView = function(node) {" +
    "\n  return (node.ownerDocument && node.ownerDocument.defaultView) // node is a No" +
    "de\n      || (node.document && node) // node is a Window\n      || node.defaultVie" +
    "w; // node is a Document\n};\n\nfunction styleRemove(name) {\n  return function() {\n" +
    "    this.style.removeProperty(name);\n  };\n}\n\nfunction styleConstant(name, value," +
    " priority) {\n  return function() {\n    this.style.setProperty(name, value, prior" +
    "ity);\n  };\n}\n\nfunction styleFunction(name, value, priority) {\n  return function(" +
    ") {\n    var v = value.apply(this, arguments);\n    if (v == null) this.style.remo" +
    "veProperty(name);\n    else this.style.setProperty(name, v, priority);\n  };\n}\n\nva" +
    "r selection_style = function(name, value, priority) {\n  return arguments.length " +
    "> 1\n      ? this.each((value == null\n            ? styleRemove : typeof value ==" +
    "= \"function\"\n            ? styleFunction\n            : styleConstant)(name, valu" +
    "e, priority == null ? \"\" : priority))\n      : styleValue(this.node(), name);\n};\n" +
    "\nfunction styleValue(node, name) {\n  return node.style.getPropertyValue(name)\n  " +
    "    || defaultView(node).getComputedStyle(node, null).getPropertyValue(name);\n}\n" +
    "\nfunction propertyRemove(name) {\n  return function() {\n    delete this[name];\n  " +
    "};\n}\n\nfunction propertyConstant(name, value) {\n  return function() {\n    this[na" +
    "me] = value;\n  };\n}\n\nfunction propertyFunction(name, value) {\n  return function(" +
    ") {\n    var v = value.apply(this, arguments);\n    if (v == null) delete this[nam" +
    "e];\n    else this[name] = v;\n  };\n}\n\nvar selection_property = function(name, val" +
    "ue) {\n  return arguments.length > 1\n      ? this.each((value == null\n          ?" +
    " propertyRemove : typeof value === \"function\"\n          ? propertyFunction\n     " +
    "     : propertyConstant)(name, value))\n      : this.node()[name];\n};\n\nfunction c" +
    "lassArray(string) {\n  return string.trim().split(/^|\\s+/);\n}\n\nfunction classList" +
    "(node) {\n  return node.classList || new ClassList(node);\n}\n\nfunction ClassList(n" +
    "ode) {\n  this._node = node;\n  this._names = classArray(node.getAttribute(\"class\"" +
    ") || \"\");\n}\n\nClassList.prototype = {\n  add: function(name) {\n    var i = this._n" +
    "ames.indexOf(name);\n    if (i < 0) {\n      this._names.push(name);\n      this._n" +
    "ode.setAttribute(\"class\", this._names.join(\" \"));\n    }\n  },\n  remove: function(" +
    "name) {\n    var i = this._names.indexOf(name);\n    if (i >= 0) {\n      this._nam" +
    "es.splice(i, 1);\n      this._node.setAttribute(\"class\", this._names.join(\" \"));\n" +
    "    }\n  },\n  contains: function(name) {\n    return this._names.indexOf(name) >= " +
    "0;\n  }\n};\n\nfunction classedAdd(node, names) {\n  var list = classList(node), i = " +
    "-1, n = names.length;\n  while (++i < n) list.add(names[i]);\n}\n\nfunction classedR" +
    "emove(node, names) {\n  var list = classList(node), i = -1, n = names.length;\n  w" +
    "hile (++i < n) list.remove(names[i]);\n}\n\nfunction classedTrue(names) {\n  return " +
    "function() {\n    classedAdd(this, names);\n  };\n}\n\nfunction classedFalse(names) {" +
    "\n  return function() {\n    classedRemove(this, names);\n  };\n}\n\nfunction classedF" +
    "unction(names, value) {\n  return function() {\n    (value.apply(this, arguments) " +
    "? classedAdd : classedRemove)(this, names);\n  };\n}\n\nvar selection_classed = func" +
    "tion(name, value) {\n  var names = classArray(name + \"\");\n\n  if (arguments.length" +
    " < 2) {\n    var list = classList(this.node()), i = -1, n = names.length;\n    whi" +
    "le (++i < n) if (!list.contains(names[i])) return false;\n    return true;\n  }\n\n " +
    " return this.each((typeof value === \"function\"\n      ? classedFunction : value\n " +
    "     ? classedTrue\n      : classedFalse)(names, value));\n};\n\nfunction textRemove" +
    "() {\n  this.textContent = \"\";\n}\n\nfunction textConstant(value) {\n  return functio" +
    "n() {\n    this.textContent = value;\n  };\n}\n\nfunction textFunction(value) {\n  ret" +
    "urn function() {\n    var v = value.apply(this, arguments);\n    this.textContent " +
    "= v == null ? \"\" : v;\n  };\n}\n\nvar selection_text = function(value) {\n  return ar" +
    "guments.length\n      ? this.each(value == null\n          ? textRemove : (typeof " +
    "value === \"function\"\n          ? textFunction\n          : textConstant)(value))\n" +
    "      : this.node().textContent;\n};\n\nfunction htmlRemove() {\n  this.innerHTML = " +
    "\"\";\n}\n\nfunction htmlConstant(value) {\n  return function() {\n    this.innerHTML =" +
    " value;\n  };\n}\n\nfunction htmlFunction(value) {\n  return function() {\n    var v =" +
    " value.apply(this, arguments);\n    this.innerHTML = v == null ? \"\" : v;\n  };\n}\n\n" +
    "var selection_html = function(value) {\n  return arguments.length\n      ? this.ea" +
    "ch(value == null\n          ? htmlRemove : (typeof value === \"function\"\n         " +
    " ? htmlFunction\n          : htmlConstant)(value))\n      : this.node().innerHTML;" +
    "\n};\n\nfunction raise() {\n  if (this.nextSibling) this.parentNode.appendChild(this" +
    ");\n}\n\nvar selection_raise = function() {\n  return this.each(raise);\n};\n\nfunction" +
    " lower() {\n  if (this.previousSibling) this.parentNode.insertBefore(this, this.p" +
    "arentNode.firstChild);\n}\n\nvar selection_lower = function() {\n  return this.each(" +
    "lower);\n};\n\nvar selection_append = function(name) {\n  var create = typeof name =" +
    "== \"function\" ? name : creator(name);\n  return this.select(function() {\n    retu" +
    "rn this.appendChild(create.apply(this, arguments));\n  });\n};\n\nfunction constantN" +
    "ull() {\n  return null;\n}\n\nvar selection_insert = function(name, before) {\n  var " +
    "create = typeof name === \"function\" ? name : creator(name),\n      select = befor" +
    "e == null ? constantNull : typeof before === \"function\" ? before : selector(befo" +
    "re);\n  return this.select(function() {\n    return this.insertBefore(create.apply" +
    "(this, arguments), select.apply(this, arguments) || null);\n  });\n};\n\nfunction re" +
    "move() {\n  var parent = this.parentNode;\n  if (parent) parent.removeChild(this);" +
    "\n}\n\nvar selection_remove = function() {\n  return this.each(remove);\n};\n\nvar sele" +
    "ction_datum = function(value) {\n  return arguments.length\n      ? this.property(" +
    "\"__data__\", value)\n      : this.node().__data__;\n};\n\nfunction dispatchEvent(node" +
    ", type, params) {\n  var window = defaultView(node),\n      event = window.CustomE" +
    "vent;\n\n  if (typeof event === \"function\") {\n    event = new event(type, params);" +
    "\n  } else {\n    event = window.document.createEvent(\"Event\");\n    if (params) ev" +
    "ent.initEvent(type, params.bubbles, params.cancelable), event.detail = params.de" +
    "tail;\n    else event.initEvent(type, false, false);\n  }\n\n  node.dispatchEvent(ev" +
    "ent);\n}\n\nfunction dispatchConstant(type, params) {\n  return function() {\n    ret" +
    "urn dispatchEvent(this, type, params);\n  };\n}\n\nfunction dispatchFunction(type, p" +
    "arams) {\n  return function() {\n    return dispatchEvent(this, type, params.apply" +
    "(this, arguments));\n  };\n}\n\nvar selection_dispatch = function(type, params) {\n  " +
    "return this.each((typeof params === \"function\"\n      ? dispatchFunction\n      : " +
    "dispatchConstant)(type, params));\n};\n\nvar root = [null];\n\nfunction Selection(gro" +
    "ups, parents) {\n  this._groups = groups;\n  this._parents = parents;\n}\n\nfunction " +
    "selection() {\n  return new Selection([[document.documentElement]], root);\n}\n\nSel" +
    "ection.prototype = selection.prototype = {\n  constructor: Selection,\n  select: s" +
    "election_select,\n  selectAll: selection_selectAll,\n  filter: selection_filter,\n " +
    " data: selection_data,\n  enter: selection_enter,\n  exit: selection_exit,\n  merge" +
    ": selection_merge,\n  order: selection_order,\n  sort: selection_sort,\n  call: sel" +
    "ection_call,\n  nodes: selection_nodes,\n  node: selection_node,\n  size: selection" +
    "_size,\n  empty: selection_empty,\n  each: selection_each,\n  attr: selection_attr," +
    "\n  style: selection_style,\n  property: selection_property,\n  classed: selection_" +
    "classed,\n  text: selection_text,\n  html: selection_html,\n  raise: selection_rais" +
    "e,\n  lower: selection_lower,\n  append: selection_append,\n  insert: selection_ins" +
    "ert,\n  remove: selection_remove,\n  datum: selection_datum,\n  on: selection_on,\n " +
    " dispatch: selection_dispatch\n};\n\nvar select = function(selector) {\n  return typ" +
    "eof selector === \"string\"\n      ? new Selection([[document.querySelector(selecto" +
    "r)]], [document.documentElement])\n      : new Selection([[selector]], root);\n};\n" +
    "\nfunction count(node) {\n  var sum = 0,\n      children = node.children,\n      i =" +
    " children && children.length;\n  if (!i) sum = 1;\n  else while (--i >= 0) sum += " +
    "children[i].value;\n  node.value = sum;\n}\n\nvar node_count = function() {\n  return" +
    " this.eachAfter(count);\n};\n\nvar node_each = function(callback) {\n  var node = th" +
    "is, current, next = [node], children, i, n;\n  do {\n    current = next.reverse()," +
    " next = [];\n    while (node = current.pop()) {\n      callback(node), children = " +
    "node.children;\n      if (children) for (i = 0, n = children.length; i < n; ++i) " +
    "{\n        next.push(children[i]);\n      }\n    }\n  } while (next.length);\n  retur" +
    "n this;\n};\n\nvar node_eachBefore = function(callback) {\n  var node = this, nodes " +
    "= [node], children, i;\n  while (node = nodes.pop()) {\n    callback(node), childr" +
    "en = node.children;\n    if (children) for (i = children.length - 1; i >= 0; --i)" +
    " {\n      nodes.push(children[i]);\n    }\n  }\n  return this;\n};\n\nvar node_eachAfte" +
    "r = function(callback) {\n  var node = this, nodes = [node], next = [], children," +
    " i, n;\n  while (node = nodes.pop()) {\n    next.push(node), children = node.child" +
    "ren;\n    if (children) for (i = 0, n = children.length; i < n; ++i) {\n      node" +
    "s.push(children[i]);\n    }\n  }\n  while (node = next.pop()) {\n    callback(node);" +
    "\n  }\n  return this;\n};\n\nvar node_sum = function(value) {\n  return this.eachAfter" +
    "(function(node) {\n    var sum = +value(node.data) || 0,\n        children = node." +
    "children,\n        i = children && children.length;\n    while (--i >= 0) sum += c" +
    "hildren[i].value;\n    node.value = sum;\n  });\n};\n\nvar node_sort = function(compa" +
    "re) {\n  return this.eachBefore(function(node) {\n    if (node.children) {\n      n" +
    "ode.children.sort(compare);\n    }\n  });\n};\n\nvar node_path = function(end) {\n  va" +
    "r start = this,\n      ancestor = leastCommonAncestor(start, end),\n      nodes = " +
    "[start];\n  while (start !== ancestor) {\n    start = start.parent;\n    nodes.push" +
    "(start);\n  }\n  var k = nodes.length;\n  while (end !== ancestor) {\n    nodes.spli" +
    "ce(k, 0, end);\n    end = end.parent;\n  }\n  return nodes;\n};\n\nfunction leastCommo" +
    "nAncestor(a, b) {\n  if (a === b) return a;\n  var aNodes = a.ancestors(),\n      b" +
    "Nodes = b.ancestors(),\n      c = null;\n  a = aNodes.pop();\n  b = bNodes.pop();\n " +
    " while (a === b) {\n    c = a;\n    a = aNodes.pop();\n    b = bNodes.pop();\n  }\n  " +
    "return c;\n}\n\nvar node_ancestors = function() {\n  var node = this, nodes = [node]" +
    ";\n  while (node = node.parent) {\n    nodes.push(node);\n  }\n  return nodes;\n};\n\nv" +
    "ar node_descendants = function() {\n  var nodes = [];\n  this.each(function(node) " +
    "{\n    nodes.push(node);\n  });\n  return nodes;\n};\n\nvar node_leaves = function() {" +
    "\n  var leaves = [];\n  this.eachBefore(function(node) {\n    if (!node.children) {" +
    "\n      leaves.push(node);\n    }\n  });\n  return leaves;\n};\n\nvar node_links = func" +
    "tion() {\n  var root = this, links = [];\n  root.each(function(node) {\n    if (nod" +
    "e !== root) { // Don’t include the root’s parent, if any.\n      links.push({sour" +
    "ce: node.parent, target: node});\n    }\n  });\n  return links;\n};\n\nfunction hierar" +
    "chy(data, children) {\n  var root = new Node(data),\n      valued = +data.value &&" +
    " (root.value = data.value),\n      node,\n      nodes = [root],\n      child,\n     " +
    " childs,\n      i,\n      n;\n\n  if (children == null) children = defaultChildren;\n" +
    "\n  while (node = nodes.pop()) {\n    if (valued) node.value = +node.data.value;\n " +
    "   if ((childs = children(node.data)) && (n = childs.length)) {\n      node.child" +
    "ren = new Array(n);\n      for (i = n - 1; i >= 0; --i) {\n        nodes.push(chil" +
    "d = node.children[i] = new Node(childs[i]));\n        child.parent = node;\n      " +
    "  child.depth = node.depth + 1;\n      }\n    }\n  }\n\n  return root.eachBefore(comp" +
    "uteHeight);\n}\n\nfunction node_copy() {\n  return hierarchy(this).eachBefore(copyDa" +
    "ta);\n}\n\nfunction defaultChildren(d) {\n  return d.children;\n}\n\nfunction copyData(" +
    "node) {\n  node.data = node.data.data;\n}\n\nfunction computeHeight(node) {\n  var he" +
    "ight = 0;\n  do node.height = height;\n  while ((node = node.parent) && (node.heig" +
    "ht < ++height));\n}\n\nfunction Node(data) {\n  this.data = data;\n  this.depth =\n  t" +
    "his.height = 0;\n  this.parent = null;\n}\n\nNode.prototype = hierarchy.prototype = " +
    "{\n  constructor: Node,\n  count: node_count,\n  each: node_each,\n  eachAfter: node" +
    "_eachAfter,\n  eachBefore: node_eachBefore,\n  sum: node_sum,\n  sort: node_sort,\n " +
    " path: node_path,\n  ancestors: node_ancestors,\n  descendants: node_descendants,\n" +
    "  leaves: node_leaves,\n  links: node_links,\n  copy: node_copy\n};\n\nvar roundNode " +
    "= function(node) {\n  node.x0 = Math.round(node.x0);\n  node.y0 = Math.round(node." +
    "y0);\n  node.x1 = Math.round(node.x1);\n  node.y1 = Math.round(node.y1);\n};\n\nvar t" +
    "reemapDice = function(parent, x0, y0, x1, y1) {\n  var nodes = parent.children,\n " +
    "     node,\n      i = -1,\n      n = nodes.length,\n      k = parent.value && (x1 -" +
    " x0) / parent.value;\n\n  while (++i < n) {\n    node = nodes[i], node.y0 = y0, nod" +
    "e.y1 = y1;\n    node.x0 = x0, node.x1 = x0 += node.value * k;\n  }\n};\n\nvar partiti" +
    "on = function() {\n  var dx = 1,\n      dy = 1,\n      padding = 0,\n      round = f" +
    "alse;\n\n  function partition(root) {\n    var n = root.height + 1;\n    root.x0 =\n " +
    "   root.y0 = padding;\n    root.x1 = dx;\n    root.y1 = dy / n;\n    root.eachBefor" +
    "e(positionNode(dy, n));\n    if (round) root.eachBefore(roundNode);\n    return ro" +
    "ot;\n  }\n\n  function positionNode(dy, n) {\n    return function(node) {\n      if (" +
    "node.children) {\n        treemapDice(node, node.x0, dy * (node.depth + 1) / n, n" +
    "ode.x1, dy * (node.depth + 2) / n);\n      }\n      var x0 = node.x0,\n          y0" +
    " = node.y0,\n          x1 = node.x1 - padding,\n          y1 = node.y1 - padding;\n" +
    "      if (x1 < x0) x0 = x1 = (x0 + x1) / 2;\n      if (y1 < y0) y0 = y1 = (y0 + y" +
    "1) / 2;\n      node.x0 = x0;\n      node.y0 = y0;\n      node.x1 = x1;\n      node.y" +
    "1 = y1;\n    };\n  }\n\n  partition.round = function(x) {\n    return arguments.lengt" +
    "h ? (round = !!x, partition) : round;\n  };\n\n  partition.size = function(x) {\n   " +
    " return arguments.length ? (dx = +x[0], dy = +x[1], partition) : [dx, dy];\n  };\n" +
    "\n  partition.padding = function(x) {\n    return arguments.length ? (padding = +x" +
    ", partition) : padding;\n  };\n\n  return partition;\n};\n\nvar ascending$1 = function" +
    "(a, b) {\n  return a < b ? -1 : a > b ? 1 : a >= b ? 0 : NaN;\n};\n\nvar bisector = " +
    "function(compare) {\n  if (compare.length === 1) compare = ascendingComparator(co" +
    "mpare);\n  return {\n    left: function(a, x, lo, hi) {\n      if (lo == null) lo =" +
    " 0;\n      if (hi == null) hi = a.length;\n      while (lo < hi) {\n        var mid" +
    " = lo + hi >>> 1;\n        if (compare(a[mid], x) < 0) lo = mid + 1;\n        else" +
    " hi = mid;\n      }\n      return lo;\n    },\n    right: function(a, x, lo, hi) {\n " +
    "     if (lo == null) lo = 0;\n      if (hi == null) hi = a.length;\n      while (l" +
    "o < hi) {\n        var mid = lo + hi >>> 1;\n        if (compare(a[mid], x) > 0) h" +
    "i = mid;\n        else lo = mid + 1;\n      }\n      return lo;\n    }\n  };\n};\n\nfunc" +
    "tion ascendingComparator(f) {\n  return function(d, x) {\n    return ascending$1(f" +
    "(d), x);\n  };\n}\n\nvar ascendingBisect = bisector(ascending$1);\nvar bisectRight = " +
    "ascendingBisect.right;\n\nvar e10 = Math.sqrt(50);\nvar e5 = Math.sqrt(10);\nvar e2 " +
    "= Math.sqrt(2);\n\nvar ticks = function(start, stop, count) {\n  var reverse,\n     " +
    " i = -1,\n      n,\n      ticks,\n      step;\n\n  stop = +stop, start = +start, coun" +
    "t = +count;\n  if (start === stop && count > 0) return [start];\n  if (reverse = s" +
    "top < start) n = start, start = stop, stop = n;\n  if ((step = tickIncrement(star" +
    "t, stop, count)) === 0 || !isFinite(step)) return [];\n\n  if (step > 0) {\n    sta" +
    "rt = Math.ceil(start / step);\n    stop = Math.floor(stop / step);\n    ticks = ne" +
    "w Array(n = Math.ceil(stop - start + 1));\n    while (++i < n) ticks[i] = (start " +
    "+ i) * step;\n  } else {\n    start = Math.floor(start * step);\n    stop = Math.ce" +
    "il(stop * step);\n    ticks = new Array(n = Math.ceil(start - stop + 1));\n    whi" +
    "le (++i < n) ticks[i] = (start - i) / step;\n  }\n\n  if (reverse) ticks.reverse();" +
    "\n\n  return ticks;\n};\n\nfunction tickIncrement(start, stop, count) {\n  var step = " +
    "(stop - start) / Math.max(0, count),\n      power = Math.floor(Math.log(step) / M" +
    "ath.LN10),\n      error = step / Math.pow(10, power);\n  return power >= 0\n      ?" +
    " (error >= e10 ? 10 : error >= e5 ? 5 : error >= e2 ? 2 : 1) * Math.pow(10, powe" +
    "r)\n      : -Math.pow(10, -power) / (error >= e10 ? 10 : error >= e5 ? 5 : error " +
    ">= e2 ? 2 : 1);\n}\n\nfunction tickStep(start, stop, count) {\n  var step0 = Math.ab" +
    "s(stop - start) / Math.max(0, count),\n      step1 = Math.pow(10, Math.floor(Math" +
    ".log(step0) / Math.LN10)),\n      error = step0 / step1;\n  if (error >= e10) step" +
    "1 *= 10;\n  else if (error >= e5) step1 *= 5;\n  else if (error >= e2) step1 *= 2;" +
    "\n  return stop < start ? -step1 : step1;\n}\n\nvar prefix = \"$\";\n\nfunction Map() {}" +
    "\n\nMap.prototype = map$1.prototype = {\n  constructor: Map,\n  has: function(key) {" +
    "\n    return (prefix + key) in this;\n  },\n  get: function(key) {\n    return this[" +
    "prefix + key];\n  },\n  set: function(key, value) {\n    this[prefix + key] = value" +
    ";\n    return this;\n  },\n  remove: function(key) {\n    var property = prefix + ke" +
    "y;\n    return property in this && delete this[property];\n  },\n  clear: function(" +
    ") {\n    for (var property in this) if (property[0] === prefix) delete this[prope" +
    "rty];\n  },\n  keys: function() {\n    var keys = [];\n    for (var property in this" +
    ") if (property[0] === prefix) keys.push(property.slice(1));\n    return keys;\n  }" +
    ",\n  values: function() {\n    var values = [];\n    for (var property in this) if " +
    "(property[0] === prefix) values.push(this[property]);\n    return values;\n  },\n  " +
    "entries: function() {\n    var entries = [];\n    for (var property in this) if (p" +
    "roperty[0] === prefix) entries.push({key: property.slice(1), value: this[propert" +
    "y]});\n    return entries;\n  },\n  size: function() {\n    var size = 0;\n    for (v" +
    "ar property in this) if (property[0] === prefix) ++size;\n    return size;\n  },\n " +
    " empty: function() {\n    for (var property in this) if (property[0] === prefix) " +
    "return false;\n    return true;\n  },\n  each: function(f) {\n    for (var property " +
    "in this) if (property[0] === prefix) f(this[property], property.slice(1), this);" +
    "\n  }\n};\n\nfunction map$1(object, f) {\n  var map = new Map;\n\n  // Copy constructor" +
    ".\n  if (object instanceof Map) object.each(function(value, key) { map.set(key, v" +
    "alue); });\n\n  // Index array by numeric index or specified key function.\n  else " +
    "if (Array.isArray(object)) {\n    var i = -1,\n        n = object.length,\n        " +
    "o;\n\n    if (f == null) while (++i < n) map.set(i, object[i]);\n    else while (++" +
    "i < n) map.set(f(o = object[i], i, object), o);\n  }\n\n  // Convert object to map." +
    "\n  else if (object) for (var key in object) map.set(key, object[key]);\n\n  return" +
    " map;\n}\n\nfunction Set() {}\n\nvar proto = map$1.prototype;\n\nSet.prototype = set.pr" +
    "ototype = {\n  constructor: Set,\n  has: proto.has,\n  add: function(value) {\n    v" +
    "alue += \"\";\n    this[prefix + value] = value;\n    return this;\n  },\n  remove: pr" +
    "oto.remove,\n  clear: proto.clear,\n  values: proto.keys,\n  size: proto.size,\n  em" +
    "pty: proto.empty,\n  each: proto.each\n};\n\nfunction set(object, f) {\n  var set = n" +
    "ew Set;\n\n  // Copy constructor.\n  if (object instanceof Set) object.each(functio" +
    "n(value) { set.add(value); });\n\n  // Otherwise, assume it’s an array.\n  else if " +
    "(object) {\n    var i = -1, n = object.length;\n    if (f == null) while (++i < n)" +
    " set.add(object[i]);\n    else while (++i < n) set.add(f(object[i], i, object));\n" +
    "  }\n\n  return set;\n}\n\nvar array$1 = Array.prototype;\n\nvar map$3 = array$1.map;\nv" +
    "ar slice$2 = array$1.slice;\n\nvar define = function(constructor, factory, prototy" +
    "pe) {\n  constructor.prototype = factory.prototype = prototype;\n  prototype.const" +
    "ructor = constructor;\n};\n\nfunction extend(parent, definition) {\n  var prototype " +
    "= Object.create(parent.prototype);\n  for (var key in definition) prototype[key] " +
    "= definition[key];\n  return prototype;\n}\n\nfunction Color() {}\n\nvar darker = 0.7;" +
    "\nvar brighter = 1 / darker;\n\nvar reI = \"\\\\s*([+-]?\\\\d+)\\\\s*\";\nvar reN = \"\\\\s*([+" +
    "-]?\\\\d*\\\\.?\\\\d+(?:[eE][+-]?\\\\d+)?)\\\\s*\";\nvar reP = \"\\\\s*([+-]?\\\\d*\\\\.?\\\\d+(?:[eE" +
    "][+-]?\\\\d+)?)%\\\\s*\";\nvar reHex3 = /^#([0-9a-f]{3})$/;\nvar reHex6 = /^#([0-9a-f]{" +
    "6})$/;\nvar reRgbInteger = new RegExp(\"^rgb\\\\(\" + [reI, reI, reI] + \"\\\\)$\");\nvar " +
    "reRgbPercent = new RegExp(\"^rgb\\\\(\" + [reP, reP, reP] + \"\\\\)$\");\nvar reRgbaInteg" +
    "er = new RegExp(\"^rgba\\\\(\" + [reI, reI, reI, reN] + \"\\\\)$\");\nvar reRgbaPercent =" +
    " new RegExp(\"^rgba\\\\(\" + [reP, reP, reP, reN] + \"\\\\)$\");\nvar reHslPercent = new " +
    "RegExp(\"^hsl\\\\(\" + [reN, reP, reP] + \"\\\\)$\");\nvar reHslaPercent = new RegExp(\"^h" +
    "sla\\\\(\" + [reN, reP, reP, reN] + \"\\\\)$\");\n\nvar named = {\n  aliceblue: 0xf0f8ff,\n" +
    "  antiquewhite: 0xfaebd7,\n  aqua: 0x00ffff,\n  aquamarine: 0x7fffd4,\n  azure: 0xf" +
    "0ffff,\n  beige: 0xf5f5dc,\n  bisque: 0xffe4c4,\n  black: 0x000000,\n  blanchedalmon" +
    "d: 0xffebcd,\n  blue: 0x0000ff,\n  blueviolet: 0x8a2be2,\n  brown: 0xa52a2a,\n  burl" +
    "ywood: 0xdeb887,\n  cadetblue: 0x5f9ea0,\n  chartreuse: 0x7fff00,\n  chocolate: 0xd" +
    "2691e,\n  coral: 0xff7f50,\n  cornflowerblue: 0x6495ed,\n  cornsilk: 0xfff8dc,\n  cr" +
    "imson: 0xdc143c,\n  cyan: 0x00ffff,\n  darkblue: 0x00008b,\n  darkcyan: 0x008b8b,\n " +
    " darkgoldenrod: 0xb8860b,\n  darkgray: 0xa9a9a9,\n  darkgreen: 0x006400,\n  darkgre" +
    "y: 0xa9a9a9,\n  darkkhaki: 0xbdb76b,\n  darkmagenta: 0x8b008b,\n  darkolivegreen: 0" +
    "x556b2f,\n  darkorange: 0xff8c00,\n  darkorchid: 0x9932cc,\n  darkred: 0x8b0000,\n  " +
    "darksalmon: 0xe9967a,\n  darkseagreen: 0x8fbc8f,\n  darkslateblue: 0x483d8b,\n  dar" +
    "kslategray: 0x2f4f4f,\n  darkslategrey: 0x2f4f4f,\n  darkturquoise: 0x00ced1,\n  da" +
    "rkviolet: 0x9400d3,\n  deeppink: 0xff1493,\n  deepskyblue: 0x00bfff,\n  dimgray: 0x" +
    "696969,\n  dimgrey: 0x696969,\n  dodgerblue: 0x1e90ff,\n  firebrick: 0xb22222,\n  fl" +
    "oralwhite: 0xfffaf0,\n  forestgreen: 0x228b22,\n  fuchsia: 0xff00ff,\n  gainsboro: " +
    "0xdcdcdc,\n  ghostwhite: 0xf8f8ff,\n  gold: 0xffd700,\n  goldenrod: 0xdaa520,\n  gra" +
    "y: 0x808080,\n  green: 0x008000,\n  greenyellow: 0xadff2f,\n  grey: 0x808080,\n  hon" +
    "eydew: 0xf0fff0,\n  hotpink: 0xff69b4,\n  indianred: 0xcd5c5c,\n  indigo: 0x4b0082," +
    "\n  ivory: 0xfffff0,\n  khaki: 0xf0e68c,\n  lavender: 0xe6e6fa,\n  lavenderblush: 0x" +
    "fff0f5,\n  lawngreen: 0x7cfc00,\n  lemonchiffon: 0xfffacd,\n  lightblue: 0xadd8e6,\n" +
    "  lightcoral: 0xf08080,\n  lightcyan: 0xe0ffff,\n  lightgoldenrodyellow: 0xfafad2," +
    "\n  lightgray: 0xd3d3d3,\n  lightgreen: 0x90ee90,\n  lightgrey: 0xd3d3d3,\n  lightpi" +
    "nk: 0xffb6c1,\n  lightsalmon: 0xffa07a,\n  lightseagreen: 0x20b2aa,\n  lightskyblue" +
    ": 0x87cefa,\n  lightslategray: 0x778899,\n  lightslategrey: 0x778899,\n  lightsteel" +
    "blue: 0xb0c4de,\n  lightyellow: 0xffffe0,\n  lime: 0x00ff00,\n  limegreen: 0x32cd32" +
    ",\n  linen: 0xfaf0e6,\n  magenta: 0xff00ff,\n  maroon: 0x800000,\n  mediumaquamarine" +
    ": 0x66cdaa,\n  mediumblue: 0x0000cd,\n  mediumorchid: 0xba55d3,\n  mediumpurple: 0x" +
    "9370db,\n  mediumseagreen: 0x3cb371,\n  mediumslateblue: 0x7b68ee,\n  mediumspringg" +
    "reen: 0x00fa9a,\n  mediumturquoise: 0x48d1cc,\n  mediumvioletred: 0xc71585,\n  midn" +
    "ightblue: 0x191970,\n  mintcream: 0xf5fffa,\n  mistyrose: 0xffe4e1,\n  moccasin: 0x" +
    "ffe4b5,\n  navajowhite: 0xffdead,\n  navy: 0x000080,\n  oldlace: 0xfdf5e6,\n  olive:" +
    " 0x808000,\n  olivedrab: 0x6b8e23,\n  orange: 0xffa500,\n  orangered: 0xff4500,\n  o" +
    "rchid: 0xda70d6,\n  palegoldenrod: 0xeee8aa,\n  palegreen: 0x98fb98,\n  paleturquoi" +
    "se: 0xafeeee,\n  palevioletred: 0xdb7093,\n  papayawhip: 0xffefd5,\n  peachpuff: 0x" +
    "ffdab9,\n  peru: 0xcd853f,\n  pink: 0xffc0cb,\n  plum: 0xdda0dd,\n  powderblue: 0xb0" +
    "e0e6,\n  purple: 0x800080,\n  rebeccapurple: 0x663399,\n  red: 0xff0000,\n  rosybrow" +
    "n: 0xbc8f8f,\n  royalblue: 0x4169e1,\n  saddlebrown: 0x8b4513,\n  salmon: 0xfa8072," +
    "\n  sandybrown: 0xf4a460,\n  seagreen: 0x2e8b57,\n  seashell: 0xfff5ee,\n  sienna: 0" +
    "xa0522d,\n  silver: 0xc0c0c0,\n  skyblue: 0x87ceeb,\n  slateblue: 0x6a5acd,\n  slate" +
    "gray: 0x708090,\n  slategrey: 0x708090,\n  snow: 0xfffafa,\n  springgreen: 0x00ff7f" +
    ",\n  steelblue: 0x4682b4,\n  tan: 0xd2b48c,\n  teal: 0x008080,\n  thistle: 0xd8bfd8," +
    "\n  tomato: 0xff6347,\n  turquoise: 0x40e0d0,\n  violet: 0xee82ee,\n  wheat: 0xf5deb" +
    "3,\n  white: 0xffffff,\n  whitesmoke: 0xf5f5f5,\n  yellow: 0xffff00,\n  yellowgreen:" +
    " 0x9acd32\n};\n\ndefine(Color, color, {\n  displayable: function() {\n    return this" +
    ".rgb().displayable();\n  },\n  toString: function() {\n    return this.rgb() + \"\";\n" +
    "  }\n});\n\nfunction color(format) {\n  var m;\n  format = (format + \"\").trim().toLow" +
    "erCase();\n  return (m = reHex3.exec(format)) ? (m = parseInt(m[1], 16), new Rgb(" +
    "(m >> 8 & 0xf) | (m >> 4 & 0x0f0), (m >> 4 & 0xf) | (m & 0xf0), ((m & 0xf) << 4)" +
    " | (m & 0xf), 1)) // #f00\n      : (m = reHex6.exec(format)) ? rgbn(parseInt(m[1]" +
    ", 16)) // #ff0000\n      : (m = reRgbInteger.exec(format)) ? new Rgb(m[1], m[2], " +
    "m[3], 1) // rgb(255, 0, 0)\n      : (m = reRgbPercent.exec(format)) ? new Rgb(m[1" +
    "] * 255 / 100, m[2] * 255 / 100, m[3] * 255 / 100, 1) // rgb(100%, 0%, 0%)\n     " +
    " : (m = reRgbaInteger.exec(format)) ? rgba(m[1], m[2], m[3], m[4]) // rgba(255, " +
    "0, 0, 1)\n      : (m = reRgbaPercent.exec(format)) ? rgba(m[1] * 255 / 100, m[2] " +
    "* 255 / 100, m[3] * 255 / 100, m[4]) // rgb(100%, 0%, 0%, 1)\n      : (m = reHslP" +
    "ercent.exec(format)) ? hsla(m[1], m[2] / 100, m[3] / 100, 1) // hsl(120, 50%, 50" +
    "%)\n      : (m = reHslaPercent.exec(format)) ? hsla(m[1], m[2] / 100, m[3] / 100," +
    " m[4]) // hsla(120, 50%, 50%, 1)\n      : named.hasOwnProperty(format) ? rgbn(nam" +
    "ed[format])\n      : format === \"transparent\" ? new Rgb(NaN, NaN, NaN, 0)\n      :" +
    " null;\n}\n\nfunction rgbn(n) {\n  return new Rgb(n >> 16 & 0xff, n >> 8 & 0xff, n &" +
    " 0xff, 1);\n}\n\nfunction rgba(r, g, b, a) {\n  if (a <= 0) r = g = b = NaN;\n  retur" +
    "n new Rgb(r, g, b, a);\n}\n\nfunction rgbConvert(o) {\n  if (!(o instanceof Color)) " +
    "o = color(o);\n  if (!o) return new Rgb;\n  o = o.rgb();\n  return new Rgb(o.r, o.g" +
    ", o.b, o.opacity);\n}\n\nfunction rgb(r, g, b, opacity) {\n  return arguments.length" +
    " === 1 ? rgbConvert(r) : new Rgb(r, g, b, opacity == null ? 1 : opacity);\n}\n\nfun" +
    "ction Rgb(r, g, b, opacity) {\n  this.r = +r;\n  this.g = +g;\n  this.b = +b;\n  thi" +
    "s.opacity = +opacity;\n}\n\ndefine(Rgb, rgb, extend(Color, {\n  brighter: function(k" +
    ") {\n    k = k == null ? brighter : Math.pow(brighter, k);\n    return new Rgb(thi" +
    "s.r * k, this.g * k, this.b * k, this.opacity);\n  },\n  darker: function(k) {\n   " +
    " k = k == null ? darker : Math.pow(darker, k);\n    return new Rgb(this.r * k, th" +
    "is.g * k, this.b * k, this.opacity);\n  },\n  rgb: function() {\n    return this;\n " +
    " },\n  displayable: function() {\n    return (0 <= this.r && this.r <= 255)\n      " +
    "  && (0 <= this.g && this.g <= 255)\n        && (0 <= this.b && this.b <= 255)\n  " +
    "      && (0 <= this.opacity && this.opacity <= 1);\n  },\n  toString: function() {" +
    "\n    var a = this.opacity; a = isNaN(a) ? 1 : Math.max(0, Math.min(1, a));\n    r" +
    "eturn (a === 1 ? \"rgb(\" : \"rgba(\")\n        + Math.max(0, Math.min(255, Math.roun" +
    "d(this.r) || 0)) + \", \"\n        + Math.max(0, Math.min(255, Math.round(this.g) |" +
    "| 0)) + \", \"\n        + Math.max(0, Math.min(255, Math.round(this.b) || 0))\n     " +
    "   + (a === 1 ? \")\" : \", \" + a + \")\");\n  }\n}));\n\nfunction hsla(h, s, l, a) {\n  i" +
    "f (a <= 0) h = s = l = NaN;\n  else if (l <= 0 || l >= 1) h = s = NaN;\n  else if " +
    "(s <= 0) h = NaN;\n  return new Hsl(h, s, l, a);\n}\n\nfunction hslConvert(o) {\n  if" +
    " (o instanceof Hsl) return new Hsl(o.h, o.s, o.l, o.opacity);\n  if (!(o instance" +
    "of Color)) o = color(o);\n  if (!o) return new Hsl;\n  if (o instanceof Hsl) retur" +
    "n o;\n  o = o.rgb();\n  var r = o.r / 255,\n      g = o.g / 255,\n      b = o.b / 25" +
    "5,\n      min = Math.min(r, g, b),\n      max = Math.max(r, g, b),\n      h = NaN,\n" +
    "      s = max - min,\n      l = (max + min) / 2;\n  if (s) {\n    if (r === max) h " +
    "= (g - b) / s + (g < b) * 6;\n    else if (g === max) h = (b - r) / s + 2;\n    el" +
    "se h = (r - g) / s + 4;\n    s /= l < 0.5 ? max + min : 2 - max - min;\n    h *= 6" +
    "0;\n  } else {\n    s = l > 0 && l < 1 ? 0 : h;\n  }\n  return new Hsl(h, s, l, o.op" +
    "acity);\n}\n\nfunction hsl(h, s, l, opacity) {\n  return arguments.length === 1 ? hs" +
    "lConvert(h) : new Hsl(h, s, l, opacity == null ? 1 : opacity);\n}\n\nfunction Hsl(h" +
    ", s, l, opacity) {\n  this.h = +h;\n  this.s = +s;\n  this.l = +l;\n  this.opacity =" +
    " +opacity;\n}\n\ndefine(Hsl, hsl, extend(Color, {\n  brighter: function(k) {\n    k =" +
    " k == null ? brighter : Math.pow(brighter, k);\n    return new Hsl(this.h, this.s" +
    ", this.l * k, this.opacity);\n  },\n  darker: function(k) {\n    k = k == null ? da" +
    "rker : Math.pow(darker, k);\n    return new Hsl(this.h, this.s, this.l * k, this." +
    "opacity);\n  },\n  rgb: function() {\n    var h = this.h % 360 + (this.h < 0) * 360" +
    ",\n        s = isNaN(h) || isNaN(this.s) ? 0 : this.s,\n        l = this.l,\n      " +
    "  m2 = l + (l < 0.5 ? l : 1 - l) * s,\n        m1 = 2 * l - m2;\n    return new Rg" +
    "b(\n      hsl2rgb(h >= 240 ? h - 240 : h + 120, m1, m2),\n      hsl2rgb(h, m1, m2)" +
    ",\n      hsl2rgb(h < 120 ? h + 240 : h - 120, m1, m2),\n      this.opacity\n    );\n" +
    "  },\n  displayable: function() {\n    return (0 <= this.s && this.s <= 1 || isNaN" +
    "(this.s))\n        && (0 <= this.l && this.l <= 1)\n        && (0 <= this.opacity " +
    "&& this.opacity <= 1);\n  }\n}));\n\n/* From FvD 13.37, CSS Color Module Level 3 */\n" +
    "function hsl2rgb(h, m1, m2) {\n  return (h < 60 ? m1 + (m2 - m1) * h / 60\n      :" +
    " h < 180 ? m2\n      : h < 240 ? m1 + (m2 - m1) * (240 - h) / 60\n      : m1) * 25" +
    "5;\n}\n\nvar deg2rad = Math.PI / 180;\nvar rad2deg = 180 / Math.PI;\n\nvar Kn = 18;\nva" +
    "r Xn = 0.950470;\nvar Yn = 1;\nvar Zn = 1.088830;\nvar t0 = 4 / 29;\nvar t1 = 6 / 29" +
    ";\nvar t2 = 3 * t1 * t1;\nvar t3 = t1 * t1 * t1;\n\nfunction labConvert(o) {\n  if (o" +
    " instanceof Lab) return new Lab(o.l, o.a, o.b, o.opacity);\n  if (o instanceof Hc" +
    "l) {\n    var h = o.h * deg2rad;\n    return new Lab(o.l, Math.cos(h) * o.c, Math." +
    "sin(h) * o.c, o.opacity);\n  }\n  if (!(o instanceof Rgb)) o = rgbConvert(o);\n  va" +
    "r b = rgb2xyz(o.r),\n      a = rgb2xyz(o.g),\n      l = rgb2xyz(o.b),\n      x = xy" +
    "z2lab((0.4124564 * b + 0.3575761 * a + 0.1804375 * l) / Xn),\n      y = xyz2lab((" +
    "0.2126729 * b + 0.7151522 * a + 0.0721750 * l) / Yn),\n      z = xyz2lab((0.01933" +
    "39 * b + 0.1191920 * a + 0.9503041 * l) / Zn);\n  return new Lab(116 * y - 16, 50" +
    "0 * (x - y), 200 * (y - z), o.opacity);\n}\n\nfunction lab(l, a, b, opacity) {\n  re" +
    "turn arguments.length === 1 ? labConvert(l) : new Lab(l, a, b, opacity == null ?" +
    " 1 : opacity);\n}\n\nfunction Lab(l, a, b, opacity) {\n  this.l = +l;\n  this.a = +a;" +
    "\n  this.b = +b;\n  this.opacity = +opacity;\n}\n\ndefine(Lab, lab, extend(Color, {\n " +
    " brighter: function(k) {\n    return new Lab(this.l + Kn * (k == null ? 1 : k), t" +
    "his.a, this.b, this.opacity);\n  },\n  darker: function(k) {\n    return new Lab(th" +
    "is.l - Kn * (k == null ? 1 : k), this.a, this.b, this.opacity);\n  },\n  rgb: func" +
    "tion() {\n    var y = (this.l + 16) / 116,\n        x = isNaN(this.a) ? y : y + th" +
    "is.a / 500,\n        z = isNaN(this.b) ? y : y - this.b / 200;\n    y = Yn * lab2x" +
    "yz(y);\n    x = Xn * lab2xyz(x);\n    z = Zn * lab2xyz(z);\n    return new Rgb(\n   " +
    "   xyz2rgb( 3.2404542 * x - 1.5371385 * y - 0.4985314 * z), // D65 -> sRGB\n     " +
    " xyz2rgb(-0.9692660 * x + 1.8760108 * y + 0.0415560 * z),\n      xyz2rgb( 0.05564" +
    "34 * x - 0.2040259 * y + 1.0572252 * z),\n      this.opacity\n    );\n  }\n}));\n\nfun" +
    "ction xyz2lab(t) {\n  return t > t3 ? Math.pow(t, 1 / 3) : t / t2 + t0;\n}\n\nfuncti" +
    "on lab2xyz(t) {\n  return t > t1 ? t * t * t : t2 * (t - t0);\n}\n\nfunction xyz2rgb" +
    "(x) {\n  return 255 * (x <= 0.0031308 ? 12.92 * x : 1.055 * Math.pow(x, 1 / 2.4) " +
    "- 0.055);\n}\n\nfunction rgb2xyz(x) {\n  return (x /= 255) <= 0.04045 ? x / 12.92 : " +
    "Math.pow((x + 0.055) / 1.055, 2.4);\n}\n\nfunction hclConvert(o) {\n  if (o instance" +
    "of Hcl) return new Hcl(o.h, o.c, o.l, o.opacity);\n  if (!(o instanceof Lab)) o =" +
    " labConvert(o);\n  var h = Math.atan2(o.b, o.a) * rad2deg;\n  return new Hcl(h < 0" +
    " ? h + 360 : h, Math.sqrt(o.a * o.a + o.b * o.b), o.l, o.opacity);\n}\n\nfunction h" +
    "cl(h, c, l, opacity) {\n  return arguments.length === 1 ? hclConvert(h) : new Hcl" +
    "(h, c, l, opacity == null ? 1 : opacity);\n}\n\nfunction Hcl(h, c, l, opacity) {\n  " +
    "this.h = +h;\n  this.c = +c;\n  this.l = +l;\n  this.opacity = +opacity;\n}\n\ndefine(" +
    "Hcl, hcl, extend(Color, {\n  brighter: function(k) {\n    return new Hcl(this.h, t" +
    "his.c, this.l + Kn * (k == null ? 1 : k), this.opacity);\n  },\n  darker: function" +
    "(k) {\n    return new Hcl(this.h, this.c, this.l - Kn * (k == null ? 1 : k), this" +
    ".opacity);\n  },\n  rgb: function() {\n    return labConvert(this).rgb();\n  }\n}));\n" +
    "\nvar A = -0.14861;\nvar B = +1.78277;\nvar C = -0.29227;\nvar D = -0.90649;\nvar E =" +
    " +1.97294;\nvar ED = E * D;\nvar EB = E * B;\nvar BC_DA = B * C - D * A;\n\nfunction " +
    "cubehelixConvert(o) {\n  if (o instanceof Cubehelix) return new Cubehelix(o.h, o." +
    "s, o.l, o.opacity);\n  if (!(o instanceof Rgb)) o = rgbConvert(o);\n  var r = o.r " +
    "/ 255,\n      g = o.g / 255,\n      b = o.b / 255,\n      l = (BC_DA * b + ED * r -" +
    " EB * g) / (BC_DA + ED - EB),\n      bl = b - l,\n      k = (E * (g - l) - C * bl)" +
    " / D,\n      s = Math.sqrt(k * k + bl * bl) / (E * l * (1 - l)), // NaN if l=0 or" +
    " l=1\n      h = s ? Math.atan2(k, bl) * rad2deg - 120 : NaN;\n  return new Cubehel" +
    "ix(h < 0 ? h + 360 : h, s, l, o.opacity);\n}\n\nfunction cubehelix(h, s, l, opacity" +
    ") {\n  return arguments.length === 1 ? cubehelixConvert(h) : new Cubehelix(h, s, " +
    "l, opacity == null ? 1 : opacity);\n}\n\nfunction Cubehelix(h, s, l, opacity) {\n  t" +
    "his.h = +h;\n  this.s = +s;\n  this.l = +l;\n  this.opacity = +opacity;\n}\n\ndefine(C" +
    "ubehelix, cubehelix, extend(Color, {\n  brighter: function(k) {\n    k = k == null" +
    " ? brighter : Math.pow(brighter, k);\n    return new Cubehelix(this.h, this.s, th" +
    "is.l * k, this.opacity);\n  },\n  darker: function(k) {\n    k = k == null ? darker" +
    " : Math.pow(darker, k);\n    return new Cubehelix(this.h, this.s, this.l * k, thi" +
    "s.opacity);\n  },\n  rgb: function() {\n    var h = isNaN(this.h) ? 0 : (this.h + 1" +
    "20) * deg2rad,\n        l = +this.l,\n        a = isNaN(this.s) ? 0 : this.s * l *" +
    " (1 - l),\n        cosh = Math.cos(h),\n        sinh = Math.sin(h);\n    return new" +
    " Rgb(\n      255 * (l + a * (A * cosh + B * sinh)),\n      255 * (l + a * (C * cos" +
    "h + D * sinh)),\n      255 * (l + a * (E * cosh)),\n      this.opacity\n    );\n  }\n" +
    "}));\n\nvar constant$3 = function(x) {\n  return function() {\n    return x;\n  };\n};" +
    "\n\nfunction linear$1(a, d) {\n  return function(t) {\n    return a + t * d;\n  };\n}\n" +
    "\nfunction exponential(a, b, y) {\n  return a = Math.pow(a, y), b = Math.pow(b, y)" +
    " - a, y = 1 / y, function(t) {\n    return Math.pow(a + t * b, y);\n  };\n}\n\nfuncti" +
    "on hue(a, b) {\n  var d = b - a;\n  return d ? linear$1(a, d > 180 || d < -180 ? d" +
    " - 360 * Math.round(d / 360) : d) : constant$3(isNaN(a) ? b : a);\n}\n\nfunction ga" +
    "mma(y) {\n  return (y = +y) === 1 ? nogamma : function(a, b) {\n    return b - a ?" +
    " exponential(a, b, y) : constant$3(isNaN(a) ? b : a);\n  };\n}\n\nfunction nogamma(a" +
    ", b) {\n  var d = b - a;\n  return d ? linear$1(a, d) : constant$3(isNaN(a) ? b : " +
    "a);\n}\n\nvar interpolateRgb = (function rgbGamma(y) {\n  var color$$1 = gamma(y);\n\n" +
    "  function rgb$$1(start, end) {\n    var r = color$$1((start = rgb(start)).r, (en" +
    "d = rgb(end)).r),\n        g = color$$1(start.g, end.g),\n        b = color$$1(sta" +
    "rt.b, end.b),\n        opacity = nogamma(start.opacity, end.opacity);\n    return " +
    "function(t) {\n      start.r = r(t);\n      start.g = g(t);\n      start.b = b(t);\n" +
    "      start.opacity = opacity(t);\n      return start + \"\";\n    };\n  }\n\n  rgb$$1." +
    "gamma = rgbGamma;\n\n  return rgb$$1;\n})(1);\n\nvar array$2 = function(a, b) {\n  var" +
    " nb = b ? b.length : 0,\n      na = a ? Math.min(nb, a.length) : 0,\n      x = new" +
    " Array(nb),\n      c = new Array(nb),\n      i;\n\n  for (i = 0; i < na; ++i) x[i] =" +
    " interpolateValue(a[i], b[i]);\n  for (; i < nb; ++i) c[i] = b[i];\n\n  return func" +
    "tion(t) {\n    for (i = 0; i < na; ++i) c[i] = x[i](t);\n    return c;\n  };\n};\n\nva" +
    "r date = function(a, b) {\n  var d = new Date;\n  return a = +a, b -= a, function(" +
    "t) {\n    return d.setTime(a + b * t), d;\n  };\n};\n\nvar interpolateNumber = functi" +
    "on(a, b) {\n  return a = +a, b -= a, function(t) {\n    return a + b * t;\n  };\n};\n" +
    "\nvar object = function(a, b) {\n  var i = {},\n      c = {},\n      k;\n\n  if (a ===" +
    " null || typeof a !== \"object\") a = {};\n  if (b === null || typeof b !== \"object" +
    "\") b = {};\n\n  for (k in b) {\n    if (k in a) {\n      i[k] = interpolateValue(a[k" +
    "], b[k]);\n    } else {\n      c[k] = b[k];\n    }\n  }\n\n  return function(t) {\n    " +
    "for (k in i) c[k] = i[k](t);\n    return c;\n  };\n};\n\nvar reA = /[-+]?(?:\\d+\\.?\\d*" +
    "|\\.?\\d+)(?:[eE][-+]?\\d+)?/g;\nvar reB = new RegExp(reA.source, \"g\");\n\nfunction ze" +
    "ro(b) {\n  return function() {\n    return b;\n  };\n}\n\nfunction one(b) {\n  return f" +
    "unction(t) {\n    return b(t) + \"\";\n  };\n}\n\nvar interpolateString = function(a, b" +
    ") {\n  var bi = reA.lastIndex = reB.lastIndex = 0, // scan index for next number " +
    "in b\n      am, // current match in a\n      bm, // current match in b\n      bs, /" +
    "/ string preceding current number in b, if any\n      i = -1, // index in s\n     " +
    " s = [], // string constants and placeholders\n      q = []; // number interpolat" +
    "ors\n\n  // Coerce inputs to strings.\n  a = a + \"\", b = b + \"\";\n\n  // Interpolate " +
    "pairs of numbers in a & b.\n  while ((am = reA.exec(a))\n      && (bm = reB.exec(b" +
    "))) {\n    if ((bs = bm.index) > bi) { // a string precedes the next number in b\n" +
    "      bs = b.slice(bi, bs);\n      if (s[i]) s[i] += bs; // coalesce with previou" +
    "s string\n      else s[++i] = bs;\n    }\n    if ((am = am[0]) === (bm = bm[0])) { " +
    "// numbers in a & b match\n      if (s[i]) s[i] += bm; // coalesce with previous " +
    "string\n      else s[++i] = bm;\n    } else { // interpolate non-matching numbers\n" +
    "      s[++i] = null;\n      q.push({i: i, x: interpolateNumber(am, bm)});\n    }\n " +
    "   bi = reB.lastIndex;\n  }\n\n  // Add remains of b.\n  if (bi < b.length) {\n    bs" +
    " = b.slice(bi);\n    if (s[i]) s[i] += bs; // coalesce with previous string\n    e" +
    "lse s[++i] = bs;\n  }\n\n  // Special optimization for only a single match.\n  // Ot" +
    "herwise, interpolate each of the numbers and rejoin the string.\n  return s.lengt" +
    "h < 2 ? (q[0]\n      ? one(q[0].x)\n      : zero(b))\n      : (b = q.length, functi" +
    "on(t) {\n          for (var i = 0, o; i < b; ++i) s[(o = q[i]).i] = o.x(t);\n     " +
    "     return s.join(\"\");\n        });\n};\n\nvar interpolateValue = function(a, b) {\n" +
    "  var t = typeof b, c;\n  return b == null || t === \"boolean\" ? constant$3(b)\n   " +
    "   : (t === \"number\" ? interpolateNumber\n      : t === \"string\" ? ((c = color(b)" +
    ") ? (b = c, interpolateRgb) : interpolateString)\n      : b instanceof color ? in" +
    "terpolateRgb\n      : b instanceof Date ? date\n      : Array.isArray(b) ? array$2" +
    "\n      : typeof b.valueOf !== \"function\" && typeof b.toString !== \"function\" || " +
    "isNaN(b) ? object\n      : interpolateNumber)(a, b);\n};\n\nvar interpolateRound = f" +
    "unction(a, b) {\n  return a = +a, b -= a, function(t) {\n    return Math.round(a +" +
    " b * t);\n  };\n};\n\nvar degrees = 180 / Math.PI;\n\nvar identity$2 = {\n  translateX:" +
    " 0,\n  translateY: 0,\n  rotate: 0,\n  skewX: 0,\n  scaleX: 1,\n  scaleY: 1\n};\n\nvar d" +
    "ecompose = function(a, b, c, d, e, f) {\n  var scaleX, scaleY, skewX;\n  if (scale" +
    "X = Math.sqrt(a * a + b * b)) a /= scaleX, b /= scaleX;\n  if (skewX = a * c + b " +
    "* d) c -= a * skewX, d -= b * skewX;\n  if (scaleY = Math.sqrt(c * c + d * d)) c " +
    "/= scaleY, d /= scaleY, skewX /= scaleY;\n  if (a * d < b * c) a = -a, b = -b, sk" +
    "ewX = -skewX, scaleX = -scaleX;\n  return {\n    translateX: e,\n    translateY: f," +
    "\n    rotate: Math.atan2(b, a) * degrees,\n    skewX: Math.atan(skewX) * degrees,\n" +
    "    scaleX: scaleX,\n    scaleY: scaleY\n  };\n};\n\nvar cssNode;\nvar cssRoot;\nvar cs" +
    "sView;\nvar svgNode;\n\nfunction parseCss(value) {\n  if (value === \"none\") return i" +
    "dentity$2;\n  if (!cssNode) cssNode = document.createElement(\"DIV\"), cssRoot = do" +
    "cument.documentElement, cssView = document.defaultView;\n  cssNode.style.transfor" +
    "m = value;\n  value = cssView.getComputedStyle(cssRoot.appendChild(cssNode), null" +
    ").getPropertyValue(\"transform\");\n  cssRoot.removeChild(cssNode);\n  value = value" +
    ".slice(7, -1).split(\",\");\n  return decompose(+value[0], +value[1], +value[2], +v" +
    "alue[3], +value[4], +value[5]);\n}\n\nfunction parseSvg(value) {\n  if (value == nul" +
    "l) return identity$2;\n  if (!svgNode) svgNode = document.createElementNS(\"http:/" +
    "/www.w3.org/2000/svg\", \"g\");\n  svgNode.setAttribute(\"transform\", value);\n  if (!" +
    "(value = svgNode.transform.baseVal.consolidate())) return identity$2;\n  value = " +
    "value.matrix;\n  return decompose(value.a, value.b, value.c, value.d, value.e, va" +
    "lue.f);\n}\n\nfunction interpolateTransform(parse, pxComma, pxParen, degParen) {\n\n " +
    " function pop(s) {\n    return s.length ? s.pop() + \" \" : \"\";\n  }\n\n  function tra" +
    "nslate(xa, ya, xb, yb, s, q) {\n    if (xa !== xb || ya !== yb) {\n      var i = s" +
    ".push(\"translate(\", null, pxComma, null, pxParen);\n      q.push({i: i - 4, x: in" +
    "terpolateNumber(xa, xb)}, {i: i - 2, x: interpolateNumber(ya, yb)});\n    } else " +
    "if (xb || yb) {\n      s.push(\"translate(\" + xb + pxComma + yb + pxParen);\n    }\n" +
    "  }\n\n  function rotate(a, b, s, q) {\n    if (a !== b) {\n      if (a - b > 180) b" +
    " += 360; else if (b - a > 180) a += 360; // shortest path\n      q.push({i: s.pus" +
    "h(pop(s) + \"rotate(\", null, degParen) - 2, x: interpolateNumber(a, b)});\n    } e" +
    "lse if (b) {\n      s.push(pop(s) + \"rotate(\" + b + degParen);\n    }\n  }\n\n  funct" +
    "ion skewX(a, b, s, q) {\n    if (a !== b) {\n      q.push({i: s.push(pop(s) + \"ske" +
    "wX(\", null, degParen) - 2, x: interpolateNumber(a, b)});\n    } else if (b) {\n   " +
    "   s.push(pop(s) + \"skewX(\" + b + degParen);\n    }\n  }\n\n  function scale(xa, ya," +
    " xb, yb, s, q) {\n    if (xa !== xb || ya !== yb) {\n      var i = s.push(pop(s) +" +
    " \"scale(\", null, \",\", null, \")\");\n      q.push({i: i - 4, x: interpolateNumber(x" +
    "a, xb)}, {i: i - 2, x: interpolateNumber(ya, yb)});\n    } else if (xb !== 1 || y" +
    "b !== 1) {\n      s.push(pop(s) + \"scale(\" + xb + \",\" + yb + \")\");\n    }\n  }\n\n  r" +
    "eturn function(a, b) {\n    var s = [], // string constants and placeholders\n    " +
    "    q = []; // number interpolators\n    a = parse(a), b = parse(b);\n    translat" +
    "e(a.translateX, a.translateY, b.translateX, b.translateY, s, q);\n    rotate(a.ro" +
    "tate, b.rotate, s, q);\n    skewX(a.skewX, b.skewX, s, q);\n    scale(a.scaleX, a." +
    "scaleY, b.scaleX, b.scaleY, s, q);\n    a = b = null; // gc\n    return function(t" +
    ") {\n      var i = -1, n = q.length, o;\n      while (++i < n) s[(o = q[i]).i] = o" +
    ".x(t);\n      return s.join(\"\");\n    };\n  };\n}\n\nvar interpolateTransformCss = int" +
    "erpolateTransform(parseCss, \"px, \", \"px)\", \"deg)\");\nvar interpolateTransformSvg " +
    "= interpolateTransform(parseSvg, \", \", \")\", \")\");\n\nvar rho = Math.SQRT2;\n\nfuncti" +
    "on cubehelix$1(hue$$1) {\n  return (function cubehelixGamma(y) {\n    y = +y;\n\n   " +
    " function cubehelix$$1(start, end) {\n      var h = hue$$1((start = cubehelix(sta" +
    "rt)).h, (end = cubehelix(end)).h),\n          s = nogamma(start.s, end.s),\n      " +
    "    l = nogamma(start.l, end.l),\n          opacity = nogamma(start.opacity, end." +
    "opacity);\n      return function(t) {\n        start.h = h(t);\n        start.s = s" +
    "(t);\n        start.l = l(Math.pow(t, y));\n        start.opacity = opacity(t);\n  " +
    "      return start + \"\";\n      };\n    }\n\n    cubehelix$$1.gamma = cubehelixGamma" +
    ";\n\n    return cubehelix$$1;\n  })(1);\n}\n\ncubehelix$1(hue);\nvar cubehelixLong = cu" +
    "behelix$1(nogamma);\n\nvar constant$4 = function(x) {\n  return function() {\n    re" +
    "turn x;\n  };\n};\n\nvar number$1 = function(x) {\n  return +x;\n};\n\nvar unit = [0, 1]" +
    ";\n\nfunction deinterpolateLinear(a, b) {\n  return (b -= (a = +a))\n      ? functio" +
    "n(x) { return (x - a) / b; }\n      : constant$4(b);\n}\n\nfunction deinterpolateCla" +
    "mp(deinterpolate) {\n  return function(a, b) {\n    var d = deinterpolate(a = +a, " +
    "b = +b);\n    return function(x) { return x <= a ? 0 : x >= b ? 1 : d(x); };\n  };" +
    "\n}\n\nfunction reinterpolateClamp(reinterpolate) {\n  return function(a, b) {\n    v" +
    "ar r = reinterpolate(a = +a, b = +b);\n    return function(t) { return t <= 0 ? a" +
    " : t >= 1 ? b : r(t); };\n  };\n}\n\nfunction bimap(domain, range, deinterpolate, re" +
    "interpolate) {\n  var d0 = domain[0], d1 = domain[1], r0 = range[0], r1 = range[1" +
    "];\n  if (d1 < d0) d0 = deinterpolate(d1, d0), r0 = reinterpolate(r1, r0);\n  else" +
    " d0 = deinterpolate(d0, d1), r0 = reinterpolate(r0, r1);\n  return function(x) { " +
    "return r0(d0(x)); };\n}\n\nfunction polymap(domain, range, deinterpolate, reinterpo" +
    "late) {\n  var j = Math.min(domain.length, range.length) - 1,\n      d = new Array" +
    "(j),\n      r = new Array(j),\n      i = -1;\n\n  // Reverse descending domains.\n  i" +
    "f (domain[j] < domain[0]) {\n    domain = domain.slice().reverse();\n    range = r" +
    "ange.slice().reverse();\n  }\n\n  while (++i < j) {\n    d[i] = deinterpolate(domain" +
    "[i], domain[i + 1]);\n    r[i] = reinterpolate(range[i], range[i + 1]);\n  }\n\n  re" +
    "turn function(x) {\n    var i = bisectRight(domain, x, 1, j) - 1;\n    return r[i]" +
    "(d[i](x));\n  };\n}\n\nfunction copy(source, target) {\n  return target\n      .domain" +
    "(source.domain())\n      .range(source.range())\n      .interpolate(source.interpo" +
    "late())\n      .clamp(source.clamp());\n}\n\n// deinterpolate(a, b)(x) takes a domai" +
    "n value x in [a,b] and returns the corresponding parameter t in [0,1].\n// reinte" +
    "rpolate(a, b)(t) takes a parameter t in [0,1] and returns the corresponding doma" +
    "in value x in [a,b].\nfunction continuous(deinterpolate, reinterpolate) {\n  var d" +
    "omain = unit,\n      range = unit,\n      interpolate$$1 = interpolateValue,\n     " +
    " clamp = false,\n      piecewise,\n      output,\n      input;\n\n  function rescale(" +
    ") {\n    piecewise = Math.min(domain.length, range.length) > 2 ? polymap : bimap;" +
    "\n    output = input = null;\n    return scale;\n  }\n\n  function scale(x) {\n    ret" +
    "urn (output || (output = piecewise(domain, range, clamp ? deinterpolateClamp(dei" +
    "nterpolate) : deinterpolate, interpolate$$1)))(+x);\n  }\n\n  scale.invert = functi" +
    "on(y) {\n    return (input || (input = piecewise(range, domain, deinterpolateLine" +
    "ar, clamp ? reinterpolateClamp(reinterpolate) : reinterpolate)))(+y);\n  };\n\n  sc" +
    "ale.domain = function(_) {\n    return arguments.length ? (domain = map$3.call(_," +
    " number$1), rescale()) : domain.slice();\n  };\n\n  scale.range = function(_) {\n   " +
    " return arguments.length ? (range = slice$2.call(_), rescale()) : range.slice();" +
    "\n  };\n\n  scale.rangeRound = function(_) {\n    return range = slice$2.call(_), in" +
    "terpolate$$1 = interpolateRound, rescale();\n  };\n\n  scale.clamp = function(_) {\n" +
    "    return arguments.length ? (clamp = !!_, rescale()) : clamp;\n  };\n\n  scale.in" +
    "terpolate = function(_) {\n    return arguments.length ? (interpolate$$1 = _, res" +
    "cale()) : interpolate$$1;\n  };\n\n  return rescale();\n}\n\n// Computes the decimal c" +
    "oefficient and exponent of the specified number x with\n// significant digits p, " +
    "where x is positive and p is in [1, 21] or undefined.\n// For example, formatDeci" +
    "mal(1.23) returns [\"123\", 0].\nvar formatDecimal = function(x, p) {\n  if ((i = (x" +
    " = p ? x.toExponential(p - 1) : x.toExponential()).indexOf(\"e\")) < 0) return nul" +
    "l; // NaN, ±Infinity\n  var i, coefficient = x.slice(0, i);\n\n  // The string retu" +
    "rned by toExponential either has the form \\d\\.\\d+e[-+]\\d+\n  // (e.g., 1.2e+3) or" +
    " the form \\de[-+]\\d+ (e.g., 1e+3).\n  return [\n    coefficient.length > 1 ? coeff" +
    "icient[0] + coefficient.slice(2) : coefficient,\n    +x.slice(i + 1)\n  ];\n};\n\nvar" +
    " exponent = function(x) {\n  return x = formatDecimal(Math.abs(x)), x ? x[1] : Na" +
    "N;\n};\n\nvar formatGroup = function(grouping, thousands) {\n  return function(value" +
    ", width) {\n    var i = value.length,\n        t = [],\n        j = 0,\n        g = " +
    "grouping[0],\n        length = 0;\n\n    while (i > 0 && g > 0) {\n      if (length " +
    "+ g + 1 > width) g = Math.max(1, width - length);\n      t.push(value.substring(i" +
    " -= g, i + g));\n      if ((length += g + 1) > width) break;\n      g = grouping[j" +
    " = (j + 1) % grouping.length];\n    }\n\n    return t.reverse().join(thousands);\n  " +
    "};\n};\n\nvar formatNumerals = function(numerals) {\n  return function(value) {\n    " +
    "return value.replace(/[0-9]/g, function(i) {\n      return numerals[+i];\n    });\n" +
    "  };\n};\n\nvar formatDefault = function(x, p) {\n  x = x.toPrecision(p);\n\n  out: fo" +
    "r (var n = x.length, i = 1, i0 = -1, i1; i < n; ++i) {\n    switch (x[i]) {\n     " +
    " case \".\": i0 = i1 = i; break;\n      case \"0\": if (i0 === 0) i0 = i; i1 = i; bre" +
    "ak;\n      case \"e\": break out;\n      default: if (i0 > 0) i0 = 0; break;\n    }\n " +
    " }\n\n  return i0 > 0 ? x.slice(0, i0) + x.slice(i1 + 1) : x;\n};\n\nvar prefixExpone" +
    "nt;\n\nvar formatPrefixAuto = function(x, p) {\n  var d = formatDecimal(x, p);\n  if" +
    " (!d) return x + \"\";\n  var coefficient = d[0],\n      exponent = d[1],\n      i = " +
    "exponent - (prefixExponent = Math.max(-8, Math.min(8, Math.floor(exponent / 3)))" +
    " * 3) + 1,\n      n = coefficient.length;\n  return i === n ? coefficient\n      : " +
    "i > n ? coefficient + new Array(i - n + 1).join(\"0\")\n      : i > 0 ? coefficient" +
    ".slice(0, i) + \".\" + coefficient.slice(i)\n      : \"0.\" + new Array(1 - i).join(\"" +
    "0\") + formatDecimal(x, Math.max(0, p + i - 1))[0]; // less than 1y!\n};\n\nvar form" +
    "atRounded = function(x, p) {\n  var d = formatDecimal(x, p);\n  if (!d) return x +" +
    " \"\";\n  var coefficient = d[0],\n      exponent = d[1];\n  return exponent < 0 ? \"0" +
    ".\" + new Array(-exponent).join(\"0\") + coefficient\n      : coefficient.length > e" +
    "xponent + 1 ? coefficient.slice(0, exponent + 1) + \".\" + coefficient.slice(expon" +
    "ent + 1)\n      : coefficient + new Array(exponent - coefficient.length + 2).join" +
    "(\"0\");\n};\n\nvar formatTypes = {\n  \"\": formatDefault,\n  \"%\": function(x, p) { retu" +
    "rn (x * 100).toFixed(p); },\n  \"b\": function(x) { return Math.round(x).toString(2" +
    "); },\n  \"c\": function(x) { return x + \"\"; },\n  \"d\": function(x) { return Math.ro" +
    "und(x).toString(10); },\n  \"e\": function(x, p) { return x.toExponential(p); },\n  " +
    "\"f\": function(x, p) { return x.toFixed(p); },\n  \"g\": function(x, p) { return x.t" +
    "oPrecision(p); },\n  \"o\": function(x) { return Math.round(x).toString(8); },\n  \"p" +
    "\": function(x, p) { return formatRounded(x * 100, p); },\n  \"r\": formatRounded,\n " +
    " \"s\": formatPrefixAuto,\n  \"X\": function(x) { return Math.round(x).toString(16).t" +
    "oUpperCase(); },\n  \"x\": function(x) { return Math.round(x).toString(16); }\n};\n\n/" +
    "/ [[fill]align][sign][symbol][0][width][,][.precision][type]\nvar re = /^(?:(.)?(" +
    "[<>=^]))?([+\\-\\( ])?([$#])?(0)?(\\d+)?(,)?(\\.\\d+)?([a-z%])?$/i;\n\nfunction formatS" +
    "pecifier(specifier) {\n  return new FormatSpecifier(specifier);\n}\n\nformatSpecifie" +
    "r.prototype = FormatSpecifier.prototype; // instanceof\n\nfunction FormatSpecifier" +
    "(specifier) {\n  if (!(match = re.exec(specifier))) throw new Error(\"invalid form" +
    "at: \" + specifier);\n\n  var match,\n      fill = match[1] || \" \",\n      align = ma" +
    "tch[2] || \">\",\n      sign = match[3] || \"-\",\n      symbol = match[4] || \"\",\n    " +
    "  zero = !!match[5],\n      width = match[6] && +match[6],\n      comma = !!match[" +
    "7],\n      precision = match[8] && +match[8].slice(1),\n      type = match[9] || \"" +
    "\";\n\n  // The \"n\" type is an alias for \",g\".\n  if (type === \"n\") comma = true, ty" +
    "pe = \"g\";\n\n  // Map invalid types to the default format.\n  else if (!formatTypes" +
    "[type]) type = \"\";\n\n  // If zero fill is specified, padding goes after sign and " +
    "before digits.\n  if (zero || (fill === \"0\" && align === \"=\")) zero = true, fill " +
    "= \"0\", align = \"=\";\n\n  this.fill = fill;\n  this.align = align;\n  this.sign = sig" +
    "n;\n  this.symbol = symbol;\n  this.zero = zero;\n  this.width = width;\n  this.comm" +
    "a = comma;\n  this.precision = precision;\n  this.type = type;\n}\n\nFormatSpecifier." +
    "prototype.toString = function() {\n  return this.fill\n      + this.align\n      + " +
    "this.sign\n      + this.symbol\n      + (this.zero ? \"0\" : \"\")\n      + (this.width" +
    " == null ? \"\" : Math.max(1, this.width | 0))\n      + (this.comma ? \",\" : \"\")\n   " +
    "   + (this.precision == null ? \"\" : \".\" + Math.max(0, this.precision | 0))\n     " +
    " + this.type;\n};\n\nvar identity$3 = function(x) {\n  return x;\n};\n\nvar prefixes = " +
    "[\"y\",\"z\",\"a\",\"f\",\"p\",\"n\",\"µ\",\"m\",\"\",\"k\",\"M\",\"G\",\"T\",\"P\",\"E\",\"Z\",\"Y\"];\n\nvar forma" +
    "tLocale = function(locale) {\n  var group = locale.grouping && locale.thousands ?" +
    " formatGroup(locale.grouping, locale.thousands) : identity$3,\n      currency = l" +
    "ocale.currency,\n      decimal = locale.decimal,\n      numerals = locale.numerals" +
    " ? formatNumerals(locale.numerals) : identity$3,\n      percent = locale.percent " +
    "|| \"%\";\n\n  function newFormat(specifier) {\n    specifier = formatSpecifier(speci" +
    "fier);\n\n    var fill = specifier.fill,\n        align = specifier.align,\n        " +
    "sign = specifier.sign,\n        symbol = specifier.symbol,\n        zero = specifi" +
    "er.zero,\n        width = specifier.width,\n        comma = specifier.comma,\n     " +
    "   precision = specifier.precision,\n        type = specifier.type;\n\n    // Compu" +
    "te the prefix and suffix.\n    // For SI-prefix, the suffix is lazily computed.\n " +
    "   var prefix = symbol === \"$\" ? currency[0] : symbol === \"#\" && /[boxX]/.test(t" +
    "ype) ? \"0\" + type.toLowerCase() : \"\",\n        suffix = symbol === \"$\" ? currency" +
    "[1] : /[%p]/.test(type) ? percent : \"\";\n\n    // What format function should we u" +
    "se?\n    // Is this an integer type?\n    // Can this type generate exponential no" +
    "tation?\n    var formatType = formatTypes[type],\n        maybeSuffix = !type || /" +
    "[defgprs%]/.test(type);\n\n    // Set the default precision if not specified,\n    " +
    "// or clamp the specified precision to the supported range.\n    // For significa" +
    "nt precision, it must be in [1, 21].\n    // For fixed precision, it must be in [" +
    "0, 20].\n    precision = precision == null ? (type ? 6 : 12)\n        : /[gprs]/.t" +
    "est(type) ? Math.max(1, Math.min(21, precision))\n        : Math.max(0, Math.min(" +
    "20, precision));\n\n    function format(value) {\n      var valuePrefix = prefix,\n " +
    "         valueSuffix = suffix,\n          i, n, c;\n\n      if (type === \"c\") {\n   " +
    "     valueSuffix = formatType(value) + valueSuffix;\n        value = \"\";\n      } " +
    "else {\n        value = +value;\n\n        // Perform the initial formatting.\n     " +
    "   var valueNegative = value < 0;\n        value = formatType(Math.abs(value), pr" +
    "ecision);\n\n        // If a negative value rounds to zero during formatting, trea" +
    "t as positive.\n        if (valueNegative && +value === 0) valueNegative = false;" +
    "\n\n        // Compute the prefix and suffix.\n        valuePrefix = (valueNegative" +
    " ? (sign === \"(\" ? sign : \"-\") : sign === \"-\" || sign === \"(\" ? \"\" : sign) + val" +
    "uePrefix;\n        valueSuffix = valueSuffix + (type === \"s\" ? prefixes[8 + prefi" +
    "xExponent / 3] : \"\") + (valueNegative && sign === \"(\" ? \")\" : \"\");\n\n        // B" +
    "reak the formatted value into the integer “value” part that can be\n        // gr" +
    "ouped, and fractional or exponential “suffix” part that is not.\n        if (mayb" +
    "eSuffix) {\n          i = -1, n = value.length;\n          while (++i < n) {\n     " +
    "       if (c = value.charCodeAt(i), 48 > c || c > 57) {\n              valueSuffi" +
    "x = (c === 46 ? decimal + value.slice(i + 1) : value.slice(i)) + valueSuffix;\n  " +
    "            value = value.slice(0, i);\n              break;\n            }\n      " +
    "    }\n        }\n      }\n\n      // If the fill character is not \"0\", grouping is " +
    "applied before padding.\n      if (comma && !zero) value = group(value, Infinity)" +
    ";\n\n      // Compute the padding.\n      var length = valuePrefix.length + value.l" +
    "ength + valueSuffix.length,\n          padding = length < width ? new Array(width" +
    " - length + 1).join(fill) : \"\";\n\n      // If the fill character is \"0\", grouping" +
    " is applied after padding.\n      if (comma && zero) value = group(padding + valu" +
    "e, padding.length ? width - valueSuffix.length : Infinity), padding = \"\";\n\n     " +
    " // Reconstruct the final output based on the desired alignment.\n      switch (a" +
    "lign) {\n        case \"<\": value = valuePrefix + value + valueSuffix + padding; b" +
    "reak;\n        case \"=\": value = valuePrefix + padding + value + valueSuffix; bre" +
    "ak;\n        case \"^\": value = padding.slice(0, length = padding.length >> 1) + v" +
    "aluePrefix + value + valueSuffix + padding.slice(length); break;\n        default" +
    ": value = padding + valuePrefix + value + valueSuffix; break;\n      }\n\n      ret" +
    "urn numerals(value);\n    }\n\n    format.toString = function() {\n      return spec" +
    "ifier + \"\";\n    };\n\n    return format;\n  }\n\n  function formatPrefix(specifier, v" +
    "alue) {\n    var f = newFormat((specifier = formatSpecifier(specifier), specifier" +
    ".type = \"f\", specifier)),\n        e = Math.max(-8, Math.min(8, Math.floor(expone" +
    "nt(value) / 3))) * 3,\n        k = Math.pow(10, -e),\n        prefix = prefixes[8 " +
    "+ e / 3];\n    return function(value) {\n      return f(k * value) + prefix;\n    }" +
    ";\n  }\n\n  return {\n    format: newFormat,\n    formatPrefix: formatPrefix\n  };\n};\n" +
    "\nvar locale;\n\nvar formatPrefix;\n\ndefaultLocale({\n  decimal: \".\",\n  thousands: \"," +
    "\",\n  grouping: [3],\n  currency: [\"$\", \"\"]\n});\n\nfunction defaultLocale(definition" +
    ") {\n  locale = formatLocale(definition);\n  exports.format = locale.format;\n  for" +
    "matPrefix = locale.formatPrefix;\n  return locale;\n}\n\nvar precisionFixed = functi" +
    "on(step) {\n  return Math.max(0, -exponent(Math.abs(step)));\n};\n\nvar precisionPre" +
    "fix = function(step, value) {\n  return Math.max(0, Math.max(-8, Math.min(8, Math" +
    ".floor(exponent(value) / 3))) * 3 - exponent(Math.abs(step)));\n};\n\nvar precision" +
    "Round = function(step, max) {\n  step = Math.abs(step), max = Math.abs(max) - ste" +
    "p;\n  return Math.max(0, exponent(max) - exponent(step)) + 1;\n};\n\nvar tickFormat " +
    "= function(domain, count, specifier) {\n  var start = domain[0],\n      stop = dom" +
    "ain[domain.length - 1],\n      step = tickStep(start, stop, count == null ? 10 : " +
    "count),\n      precision;\n  specifier = formatSpecifier(specifier == null ? \",f\" " +
    ": specifier);\n  switch (specifier.type) {\n    case \"s\": {\n      var value = Math" +
    ".max(Math.abs(start), Math.abs(stop));\n      if (specifier.precision == null && " +
    "!isNaN(precision = precisionPrefix(step, value))) specifier.precision = precisio" +
    "n;\n      return formatPrefix(specifier, value);\n    }\n    case \"\":\n    case \"e\":" +
    "\n    case \"g\":\n    case \"p\":\n    case \"r\": {\n      if (specifier.precision == nu" +
    "ll && !isNaN(precision = precisionRound(step, Math.max(Math.abs(start), Math.abs" +
    "(stop))))) specifier.precision = precision - (specifier.type === \"e\");\n      bre" +
    "ak;\n    }\n    case \"f\":\n    case \"%\": {\n      if (specifier.precision == null &&" +
    " !isNaN(precision = precisionFixed(step))) specifier.precision = precision - (sp" +
    "ecifier.type === \"%\") * 2;\n      break;\n    }\n  }\n  return exports.format(specif" +
    "ier);\n};\n\nfunction linearish(scale) {\n  var domain = scale.domain;\n\n  scale.tick" +
    "s = function(count) {\n    var d = domain();\n    return ticks(d[0], d[d.length - " +
    "1], count == null ? 10 : count);\n  };\n\n  scale.tickFormat = function(count, spec" +
    "ifier) {\n    return tickFormat(domain(), count, specifier);\n  };\n\n  scale.nice =" +
    " function(count) {\n    if (count == null) count = 10;\n\n    var d = domain(),\n   " +
    "     i0 = 0,\n        i1 = d.length - 1,\n        start = d[i0],\n        stop = d[" +
    "i1],\n        step;\n\n    if (stop < start) {\n      step = start, start = stop, st" +
    "op = step;\n      step = i0, i0 = i1, i1 = step;\n    }\n\n    step = tickIncrement(" +
    "start, stop, count);\n\n    if (step > 0) {\n      start = Math.floor(start / step)" +
    " * step;\n      stop = Math.ceil(stop / step) * step;\n      step = tickIncrement(" +
    "start, stop, count);\n    } else if (step < 0) {\n      start = Math.ceil(start * " +
    "step) / step;\n      stop = Math.floor(stop * step) / step;\n      step = tickIncr" +
    "ement(start, stop, count);\n    }\n\n    if (step > 0) {\n      d[i0] = Math.floor(s" +
    "tart / step) * step;\n      d[i1] = Math.ceil(stop / step) * step;\n      domain(d" +
    ");\n    } else if (step < 0) {\n      d[i0] = Math.ceil(start * step) / step;\n    " +
    "  d[i1] = Math.floor(stop * step) / step;\n      domain(d);\n    }\n\n    return sca" +
    "le;\n  };\n\n  return scale;\n}\n\nfunction linear() {\n  var scale = continuous(deinte" +
    "rpolateLinear, interpolateNumber);\n\n  scale.copy = function() {\n    return copy(" +
    "scale, linear());\n  };\n\n  return linearish(scale);\n}\n\nvar t0$1 = new Date;\nvar t" +
    "1$1 = new Date;\n\nfunction newInterval(floori, offseti, count, field) {\n\n  functi" +
    "on interval(date) {\n    return floori(date = new Date(+date)), date;\n  }\n\n  inte" +
    "rval.floor = interval;\n\n  interval.ceil = function(date) {\n    return floori(dat" +
    "e = new Date(date - 1)), offseti(date, 1), floori(date), date;\n  };\n\n  interval." +
    "round = function(date) {\n    var d0 = interval(date),\n        d1 = interval.ceil" +
    "(date);\n    return date - d0 < d1 - date ? d0 : d1;\n  };\n\n  interval.offset = fu" +
    "nction(date, step) {\n    return offseti(date = new Date(+date), step == null ? 1" +
    " : Math.floor(step)), date;\n  };\n\n  interval.range = function(start, stop, step)" +
    " {\n    var range = [];\n    start = interval.ceil(start);\n    step = step == null" +
    " ? 1 : Math.floor(step);\n    if (!(start < stop) || !(step > 0)) return range; /" +
    "/ also handles Invalid Date\n    do range.push(new Date(+start)); while (offseti(" +
    "start, step), floori(start), start < stop)\n    return range;\n  };\n\n  interval.fi" +
    "lter = function(test) {\n    return newInterval(function(date) {\n      if (date >" +
    "= date) while (floori(date), !test(date)) date.setTime(date - 1);\n    }, functio" +
    "n(date, step) {\n      if (date >= date) {\n        if (step < 0) while (++step <=" +
    " 0) {\n          while (offseti(date, -1), !test(date)) {} // eslint-disable-line" +
    " no-empty\n        } else while (--step >= 0) {\n          while (offseti(date, +1" +
    "), !test(date)) {} // eslint-disable-line no-empty\n        }\n      }\n    });\n  }" +
    ";\n\n  if (count) {\n    interval.count = function(start, end) {\n      t0$1.setTime" +
    "(+start), t1$1.setTime(+end);\n      floori(t0$1), floori(t1$1);\n      return Mat" +
    "h.floor(count(t0$1, t1$1));\n    };\n\n    interval.every = function(step) {\n      " +
    "step = Math.floor(step);\n      return !isFinite(step) || !(step > 0) ? null\n    " +
    "      : !(step > 1) ? interval\n          : interval.filter(field\n              ?" +
    " function(d) { return field(d) % step === 0; }\n              : function(d) { ret" +
    "urn interval.count(0, d) % step === 0; });\n    };\n  }\n\n  return interval;\n}\n\nvar" +
    " millisecond = newInterval(function() {\n  // noop\n}, function(date, step) {\n  da" +
    "te.setTime(+date + step);\n}, function(start, end) {\n  return end - start;\n});\n\n/" +
    "/ An optimized implementation for this simple case.\nmillisecond.every = function" +
    "(k) {\n  k = Math.floor(k);\n  if (!isFinite(k) || !(k > 0)) return null;\n  if (!(" +
    "k > 1)) return millisecond;\n  return newInterval(function(date) {\n    date.setTi" +
    "me(Math.floor(date / k) * k);\n  }, function(date, step) {\n    date.setTime(+date" +
    " + step * k);\n  }, function(start, end) {\n    return (end - start) / k;\n  });\n};" +
    "\n\nvar durationSecond$1 = 1e3;\nvar durationMinute$1 = 6e4;\nvar durationHour$1 = 3" +
    "6e5;\nvar durationDay$1 = 864e5;\nvar durationWeek$1 = 6048e5;\n\nvar second = newIn" +
    "terval(function(date) {\n  date.setTime(Math.floor(date / durationSecond$1) * dur" +
    "ationSecond$1);\n}, function(date, step) {\n  date.setTime(+date + step * duration" +
    "Second$1);\n}, function(start, end) {\n  return (end - start) / durationSecond$1;\n" +
    "}, function(date) {\n  return date.getUTCSeconds();\n});\n\nvar minute = newInterval" +
    "(function(date) {\n  date.setTime(Math.floor(date / durationMinute$1) * durationM" +
    "inute$1);\n}, function(date, step) {\n  date.setTime(+date + step * durationMinute" +
    "$1);\n}, function(start, end) {\n  return (end - start) / durationMinute$1;\n}, fun" +
    "ction(date) {\n  return date.getMinutes();\n});\n\nvar hour = newInterval(function(d" +
    "ate) {\n  var offset = date.getTimezoneOffset() * durationMinute$1 % durationHour" +
    "$1;\n  if (offset < 0) offset += durationHour$1;\n  date.setTime(Math.floor((+date" +
    " - offset) / durationHour$1) * durationHour$1 + offset);\n}, function(date, step)" +
    " {\n  date.setTime(+date + step * durationHour$1);\n}, function(start, end) {\n  re" +
    "turn (end - start) / durationHour$1;\n}, function(date) {\n  return date.getHours(" +
    ");\n});\n\nvar day = newInterval(function(date) {\n  date.setHours(0, 0, 0, 0);\n}, f" +
    "unction(date, step) {\n  date.setDate(date.getDate() + step);\n}, function(start, " +
    "end) {\n  return (end - start - (end.getTimezoneOffset() - start.getTimezoneOffse" +
    "t()) * durationMinute$1) / durationDay$1;\n}, function(date) {\n  return date.getD" +
    "ate() - 1;\n});\n\nfunction weekday(i) {\n  return newInterval(function(date) {\n    " +
    "date.setDate(date.getDate() - (date.getDay() + 7 - i) % 7);\n    date.setHours(0," +
    " 0, 0, 0);\n  }, function(date, step) {\n    date.setDate(date.getDate() + step * " +
    "7);\n  }, function(start, end) {\n    return (end - start - (end.getTimezoneOffset" +
    "() - start.getTimezoneOffset()) * durationMinute$1) / durationWeek$1;\n  });\n}\n\nv" +
    "ar sunday = weekday(0);\nvar monday = weekday(1);\nvar tuesday = weekday(2);\nvar w" +
    "ednesday = weekday(3);\nvar thursday = weekday(4);\nvar friday = weekday(5);\nvar s" +
    "aturday = weekday(6);\n\nvar month = newInterval(function(date) {\n  date.setDate(1" +
    ");\n  date.setHours(0, 0, 0, 0);\n}, function(date, step) {\n  date.setMonth(date.g" +
    "etMonth() + step);\n}, function(start, end) {\n  return end.getMonth() - start.get" +
    "Month() + (end.getFullYear() - start.getFullYear()) * 12;\n}, function(date) {\n  " +
    "return date.getMonth();\n});\n\nvar year = newInterval(function(date) {\n  date.setM" +
    "onth(0, 1);\n  date.setHours(0, 0, 0, 0);\n}, function(date, step) {\n  date.setFul" +
    "lYear(date.getFullYear() + step);\n}, function(start, end) {\n  return end.getFull" +
    "Year() - start.getFullYear();\n}, function(date) {\n  return date.getFullYear();\n}" +
    ");\n\n// An optimized implementation for this simple case.\nyear.every = function(k" +
    ") {\n  return !isFinite(k = Math.floor(k)) || !(k > 0) ? null : newInterval(funct" +
    "ion(date) {\n    date.setFullYear(Math.floor(date.getFullYear() / k) * k);\n    da" +
    "te.setMonth(0, 1);\n    date.setHours(0, 0, 0, 0);\n  }, function(date, step) {\n  " +
    "  date.setFullYear(date.getFullYear() + step * k);\n  });\n};\n\nvar utcMinute = new" +
    "Interval(function(date) {\n  date.setUTCSeconds(0, 0);\n}, function(date, step) {\n" +
    "  date.setTime(+date + step * durationMinute$1);\n}, function(start, end) {\n  ret" +
    "urn (end - start) / durationMinute$1;\n}, function(date) {\n  return date.getUTCMi" +
    "nutes();\n});\n\nvar utcHour = newInterval(function(date) {\n  date.setUTCMinutes(0," +
    " 0, 0);\n}, function(date, step) {\n  date.setTime(+date + step * durationHour$1);" +
    "\n}, function(start, end) {\n  return (end - start) / durationHour$1;\n}, function(" +
    "date) {\n  return date.getUTCHours();\n});\n\nvar utcDay = newInterval(function(date" +
    ") {\n  date.setUTCHours(0, 0, 0, 0);\n}, function(date, step) {\n  date.setUTCDate(" +
    "date.getUTCDate() + step);\n}, function(start, end) {\n  return (end - start) / du" +
    "rationDay$1;\n}, function(date) {\n  return date.getUTCDate() - 1;\n});\n\nfunction u" +
    "tcWeekday(i) {\n  return newInterval(function(date) {\n    date.setUTCDate(date.ge" +
    "tUTCDate() - (date.getUTCDay() + 7 - i) % 7);\n    date.setUTCHours(0, 0, 0, 0);\n" +
    "  }, function(date, step) {\n    date.setUTCDate(date.getUTCDate() + step * 7);\n " +
    " }, function(start, end) {\n    return (end - start) / durationWeek$1;\n  });\n}\n\nv" +
    "ar utcSunday = utcWeekday(0);\nvar utcMonday = utcWeekday(1);\nvar utcTuesday = ut" +
    "cWeekday(2);\nvar utcWednesday = utcWeekday(3);\nvar utcThursday = utcWeekday(4);\n" +
    "var utcFriday = utcWeekday(5);\nvar utcSaturday = utcWeekday(6);\n\nvar utcMonth = " +
    "newInterval(function(date) {\n  date.setUTCDate(1);\n  date.setUTCHours(0, 0, 0, 0" +
    ");\n}, function(date, step) {\n  date.setUTCMonth(date.getUTCMonth() + step);\n}, f" +
    "unction(start, end) {\n  return end.getUTCMonth() - start.getUTCMonth() + (end.ge" +
    "tUTCFullYear() - start.getUTCFullYear()) * 12;\n}, function(date) {\n  return date" +
    ".getUTCMonth();\n});\n\nvar utcYear = newInterval(function(date) {\n  date.setUTCMon" +
    "th(0, 1);\n  date.setUTCHours(0, 0, 0, 0);\n}, function(date, step) {\n  date.setUT" +
    "CFullYear(date.getUTCFullYear() + step);\n}, function(start, end) {\n  return end." +
    "getUTCFullYear() - start.getUTCFullYear();\n}, function(date) {\n  return date.get" +
    "UTCFullYear();\n});\n\n// An optimized implementation for this simple case.\nutcYear" +
    ".every = function(k) {\n  return !isFinite(k = Math.floor(k)) || !(k > 0) ? null " +
    ": newInterval(function(date) {\n    date.setUTCFullYear(Math.floor(date.getUTCFul" +
    "lYear() / k) * k);\n    date.setUTCMonth(0, 1);\n    date.setUTCHours(0, 0, 0, 0);" +
    "\n  }, function(date, step) {\n    date.setUTCFullYear(date.getUTCFullYear() + ste" +
    "p * k);\n  });\n};\n\nfunction localDate(d) {\n  if (0 <= d.y && d.y < 100) {\n    var" +
    " date = new Date(-1, d.m, d.d, d.H, d.M, d.S, d.L);\n    date.setFullYear(d.y);\n " +
    "   return date;\n  }\n  return new Date(d.y, d.m, d.d, d.H, d.M, d.S, d.L);\n}\n\nfun" +
    "ction utcDate(d) {\n  if (0 <= d.y && d.y < 100) {\n    var date = new Date(Date.U" +
    "TC(-1, d.m, d.d, d.H, d.M, d.S, d.L));\n    date.setUTCFullYear(d.y);\n    return " +
    "date;\n  }\n  return new Date(Date.UTC(d.y, d.m, d.d, d.H, d.M, d.S, d.L));\n}\n\nfun" +
    "ction newYear(y) {\n  return {y: y, m: 0, d: 1, H: 0, M: 0, S: 0, L: 0};\n}\n\nfunct" +
    "ion formatLocale$1(locale) {\n  var locale_dateTime = locale.dateTime,\n      loca" +
    "le_date = locale.date,\n      locale_time = locale.time,\n      locale_periods = l" +
    "ocale.periods,\n      locale_weekdays = locale.days,\n      locale_shortWeekdays =" +
    " locale.shortDays,\n      locale_months = locale.months,\n      locale_shortMonths" +
    " = locale.shortMonths;\n\n  var periodRe = formatRe(locale_periods),\n      periodL" +
    "ookup = formatLookup(locale_periods),\n      weekdayRe = formatRe(locale_weekdays" +
    "),\n      weekdayLookup = formatLookup(locale_weekdays),\n      shortWeekdayRe = f" +
    "ormatRe(locale_shortWeekdays),\n      shortWeekdayLookup = formatLookup(locale_sh" +
    "ortWeekdays),\n      monthRe = formatRe(locale_months),\n      monthLookup = forma" +
    "tLookup(locale_months),\n      shortMonthRe = formatRe(locale_shortMonths),\n     " +
    " shortMonthLookup = formatLookup(locale_shortMonths);\n\n  var formats = {\n    \"a\"" +
    ": formatShortWeekday,\n    \"A\": formatWeekday,\n    \"b\": formatShortMonth,\n    \"B\"" +
    ": formatMonth,\n    \"c\": null,\n    \"d\": formatDayOfMonth,\n    \"e\": formatDayOfMon" +
    "th,\n    \"f\": formatMicroseconds,\n    \"H\": formatHour24,\n    \"I\": formatHour12,\n " +
    "   \"j\": formatDayOfYear,\n    \"L\": formatMilliseconds,\n    \"m\": formatMonthNumber" +
    ",\n    \"M\": formatMinutes,\n    \"p\": formatPeriod,\n    \"Q\": formatUnixTimestamp,\n " +
    "   \"s\": formatUnixTimestampSeconds,\n    \"S\": formatSeconds,\n    \"u\": formatWeekd" +
    "ayNumberMonday,\n    \"U\": formatWeekNumberSunday,\n    \"V\": formatWeekNumberISO,\n " +
    "   \"w\": formatWeekdayNumberSunday,\n    \"W\": formatWeekNumberMonday,\n    \"x\": nul" +
    "l,\n    \"X\": null,\n    \"y\": formatYear,\n    \"Y\": formatFullYear,\n    \"Z\": formatZ" +
    "one,\n    \"%\": formatLiteralPercent\n  };\n\n  var utcFormats = {\n    \"a\": formatUTC" +
    "ShortWeekday,\n    \"A\": formatUTCWeekday,\n    \"b\": formatUTCShortMonth,\n    \"B\": " +
    "formatUTCMonth,\n    \"c\": null,\n    \"d\": formatUTCDayOfMonth,\n    \"e\": formatUTCD" +
    "ayOfMonth,\n    \"f\": formatUTCMicroseconds,\n    \"H\": formatUTCHour24,\n    \"I\": fo" +
    "rmatUTCHour12,\n    \"j\": formatUTCDayOfYear,\n    \"L\": formatUTCMilliseconds,\n    " +
    "\"m\": formatUTCMonthNumber,\n    \"M\": formatUTCMinutes,\n    \"p\": formatUTCPeriod,\n" +
    "    \"Q\": formatUnixTimestamp,\n    \"s\": formatUnixTimestampSeconds,\n    \"S\": form" +
    "atUTCSeconds,\n    \"u\": formatUTCWeekdayNumberMonday,\n    \"U\": formatUTCWeekNumbe" +
    "rSunday,\n    \"V\": formatUTCWeekNumberISO,\n    \"w\": formatUTCWeekdayNumberSunday," +
    "\n    \"W\": formatUTCWeekNumberMonday,\n    \"x\": null,\n    \"X\": null,\n    \"y\": form" +
    "atUTCYear,\n    \"Y\": formatUTCFullYear,\n    \"Z\": formatUTCZone,\n    \"%\": formatLi" +
    "teralPercent\n  };\n\n  var parses = {\n    \"a\": parseShortWeekday,\n    \"A\": parseWe" +
    "ekday,\n    \"b\": parseShortMonth,\n    \"B\": parseMonth,\n    \"c\": parseLocaleDateTi" +
    "me,\n    \"d\": parseDayOfMonth,\n    \"e\": parseDayOfMonth,\n    \"f\": parseMicrosecon" +
    "ds,\n    \"H\": parseHour24,\n    \"I\": parseHour24,\n    \"j\": parseDayOfYear,\n    \"L\"" +
    ": parseMilliseconds,\n    \"m\": parseMonthNumber,\n    \"M\": parseMinutes,\n    \"p\": " +
    "parsePeriod,\n    \"Q\": parseUnixTimestamp,\n    \"s\": parseUnixTimestampSeconds,\n  " +
    "  \"S\": parseSeconds,\n    \"u\": parseWeekdayNumberMonday,\n    \"U\": parseWeekNumber" +
    "Sunday,\n    \"V\": parseWeekNumberISO,\n    \"w\": parseWeekdayNumberSunday,\n    \"W\":" +
    " parseWeekNumberMonday,\n    \"x\": parseLocaleDate,\n    \"X\": parseLocaleTime,\n    " +
    "\"y\": parseYear,\n    \"Y\": parseFullYear,\n    \"Z\": parseZone,\n    \"%\": parseLitera" +
    "lPercent\n  };\n\n  // These recursive directive definitions must be deferred.\n  fo" +
    "rmats.x = newFormat(locale_date, formats);\n  formats.X = newFormat(locale_time, " +
    "formats);\n  formats.c = newFormat(locale_dateTime, formats);\n  utcFormats.x = ne" +
    "wFormat(locale_date, utcFormats);\n  utcFormats.X = newFormat(locale_time, utcFor" +
    "mats);\n  utcFormats.c = newFormat(locale_dateTime, utcFormats);\n\n  function newF" +
    "ormat(specifier, formats) {\n    return function(date) {\n      var string = [],\n " +
    "         i = -1,\n          j = 0,\n          n = specifier.length,\n          c,\n " +
    "         pad,\n          format;\n\n      if (!(date instanceof Date)) date = new D" +
    "ate(+date);\n\n      while (++i < n) {\n        if (specifier.charCodeAt(i) === 37)" +
    " {\n          string.push(specifier.slice(j, i));\n          if ((pad = pads[c = s" +
    "pecifier.charAt(++i)]) != null) c = specifier.charAt(++i);\n          else pad = " +
    "c === \"e\" ? \" \" : \"0\";\n          if (format = formats[c]) c = format(date, pad);" +
    "\n          string.push(c);\n          j = i + 1;\n        }\n      }\n\n      string." +
    "push(specifier.slice(j, i));\n      return string.join(\"\");\n    };\n  }\n\n  functio" +
    "n newParse(specifier, newDate) {\n    return function(string) {\n      var d = new" +
    "Year(1900),\n          i = parseSpecifier(d, specifier, string += \"\", 0),\n       " +
    "   week, day$$1;\n      if (i != string.length) return null;\n\n      // If a UNIX " +
    "timestamp is specified, return it.\n      if (\"Q\" in d) return new Date(d.Q);\n\n  " +
    "    // The am-pm flag is 0 for AM, and 1 for PM.\n      if (\"p\" in d) d.H = d.H %" +
    " 12 + d.p * 12;\n\n      // Convert day-of-week and week-of-year to day-of-year.\n " +
    "     if (\"V\" in d) {\n        if (d.V < 1 || d.V > 53) return null;\n        if (!" +
    "(\"w\" in d)) d.w = 1;\n        if (\"Z\" in d) {\n          week = utcDate(newYear(d." +
    "y)), day$$1 = week.getUTCDay();\n          week = day$$1 > 4 || day$$1 === 0 ? ut" +
    "cMonday.ceil(week) : utcMonday(week);\n          week = utcDay.offset(week, (d.V " +
    "- 1) * 7);\n          d.y = week.getUTCFullYear();\n          d.m = week.getUTCMon" +
    "th();\n          d.d = week.getUTCDate() + (d.w + 6) % 7;\n        } else {\n      " +
    "    week = newDate(newYear(d.y)), day$$1 = week.getDay();\n          week = day$$" +
    "1 > 4 || day$$1 === 0 ? monday.ceil(week) : monday(week);\n          week = day.o" +
    "ffset(week, (d.V - 1) * 7);\n          d.y = week.getFullYear();\n          d.m = " +
    "week.getMonth();\n          d.d = week.getDate() + (d.w + 6) % 7;\n        }\n     " +
    " } else if (\"W\" in d || \"U\" in d) {\n        if (!(\"w\" in d)) d.w = \"u\" in d ? d." +
    "u % 7 : \"W\" in d ? 1 : 0;\n        day$$1 = \"Z\" in d ? utcDate(newYear(d.y)).getU" +
    "TCDay() : newDate(newYear(d.y)).getDay();\n        d.m = 0;\n        d.d = \"W\" in " +
    "d ? (d.w + 6) % 7 + d.W * 7 - (day$$1 + 5) % 7 : d.w + d.U * 7 - (day$$1 + 6) % " +
    "7;\n      }\n\n      // If a time zone is specified, all fields are interpreted as " +
    "UTC and then\n      // offset according to the specified time zone.\n      if (\"Z\"" +
    " in d) {\n        d.H += d.Z / 100 | 0;\n        d.M += d.Z % 100;\n        return " +
    "utcDate(d);\n      }\n\n      // Otherwise, all fields are in local time.\n      ret" +
    "urn newDate(d);\n    };\n  }\n\n  function parseSpecifier(d, specifier, string, j) {" +
    "\n    var i = 0,\n        n = specifier.length,\n        m = string.length,\n       " +
    " c,\n        parse;\n\n    while (i < n) {\n      if (j >= m) return -1;\n      c = s" +
    "pecifier.charCodeAt(i++);\n      if (c === 37) {\n        c = specifier.charAt(i++" +
    ");\n        parse = parses[c in pads ? specifier.charAt(i++) : c];\n        if (!p" +
    "arse || ((j = parse(d, string, j)) < 0)) return -1;\n      } else if (c != string" +
    ".charCodeAt(j++)) {\n        return -1;\n      }\n    }\n\n    return j;\n  }\n\n  funct" +
    "ion parsePeriod(d, string, i) {\n    var n = periodRe.exec(string.slice(i));\n    " +
    "return n ? (d.p = periodLookup[n[0].toLowerCase()], i + n[0].length) : -1;\n  }\n\n" +
    "  function parseShortWeekday(d, string, i) {\n    var n = shortWeekdayRe.exec(str" +
    "ing.slice(i));\n    return n ? (d.w = shortWeekdayLookup[n[0].toLowerCase()], i +" +
    " n[0].length) : -1;\n  }\n\n  function parseWeekday(d, string, i) {\n    var n = wee" +
    "kdayRe.exec(string.slice(i));\n    return n ? (d.w = weekdayLookup[n[0].toLowerCa" +
    "se()], i + n[0].length) : -1;\n  }\n\n  function parseShortMonth(d, string, i) {\n  " +
    "  var n = shortMonthRe.exec(string.slice(i));\n    return n ? (d.m = shortMonthLo" +
    "okup[n[0].toLowerCase()], i + n[0].length) : -1;\n  }\n\n  function parseMonth(d, s" +
    "tring, i) {\n    var n = monthRe.exec(string.slice(i));\n    return n ? (d.m = mon" +
    "thLookup[n[0].toLowerCase()], i + n[0].length) : -1;\n  }\n\n  function parseLocale" +
    "DateTime(d, string, i) {\n    return parseSpecifier(d, locale_dateTime, string, i" +
    ");\n  }\n\n  function parseLocaleDate(d, string, i) {\n    return parseSpecifier(d, " +
    "locale_date, string, i);\n  }\n\n  function parseLocaleTime(d, string, i) {\n    ret" +
    "urn parseSpecifier(d, locale_time, string, i);\n  }\n\n  function formatShortWeekda" +
    "y(d) {\n    return locale_shortWeekdays[d.getDay()];\n  }\n\n  function formatWeekda" +
    "y(d) {\n    return locale_weekdays[d.getDay()];\n  }\n\n  function formatShortMonth(" +
    "d) {\n    return locale_shortMonths[d.getMonth()];\n  }\n\n  function formatMonth(d)" +
    " {\n    return locale_months[d.getMonth()];\n  }\n\n  function formatPeriod(d) {\n   " +
    " return locale_periods[+(d.getHours() >= 12)];\n  }\n\n  function formatUTCShortWee" +
    "kday(d) {\n    return locale_shortWeekdays[d.getUTCDay()];\n  }\n\n  function format" +
    "UTCWeekday(d) {\n    return locale_weekdays[d.getUTCDay()];\n  }\n\n  function forma" +
    "tUTCShortMonth(d) {\n    return locale_shortMonths[d.getUTCMonth()];\n  }\n\n  funct" +
    "ion formatUTCMonth(d) {\n    return locale_months[d.getUTCMonth()];\n  }\n\n  functi" +
    "on formatUTCPeriod(d) {\n    return locale_periods[+(d.getUTCHours() >= 12)];\n  }" +
    "\n\n  return {\n    format: function(specifier) {\n      var f = newFormat(specifier" +
    " += \"\", formats);\n      f.toString = function() { return specifier; };\n      ret" +
    "urn f;\n    },\n    parse: function(specifier) {\n      var p = newParse(specifier " +
    "+= \"\", localDate);\n      p.toString = function() { return specifier; };\n      re" +
    "turn p;\n    },\n    utcFormat: function(specifier) {\n      var f = newFormat(spec" +
    "ifier += \"\", utcFormats);\n      f.toString = function() { return specifier; };\n " +
    "     return f;\n    },\n    utcParse: function(specifier) {\n      var p = newParse" +
    "(specifier, utcDate);\n      p.toString = function() { return specifier; };\n     " +
    " return p;\n    }\n  };\n}\n\nvar pads = {\"-\": \"\", \"_\": \" \", \"0\": \"0\"};\nvar numberRe " +
    "= /^\\s*\\d+/;\nvar percentRe = /^%/;\nvar requoteRe = /[\\\\^$*+?|[\\]().{}]/g;\n\nfunct" +
    "ion pad(value, fill, width) {\n  var sign = value < 0 ? \"-\" : \"\",\n      string = " +
    "(sign ? -value : value) + \"\",\n      length = string.length;\n  return sign + (len" +
    "gth < width ? new Array(width - length + 1).join(fill) + string : string);\n}\n\nfu" +
    "nction requote(s) {\n  return s.replace(requoteRe, \"\\\\$&\");\n}\n\nfunction formatRe(" +
    "names) {\n  return new RegExp(\"^(?:\" + names.map(requote).join(\"|\") + \")\", \"i\");\n" +
    "}\n\nfunction formatLookup(names) {\n  var map = {}, i = -1, n = names.length;\n  wh" +
    "ile (++i < n) map[names[i].toLowerCase()] = i;\n  return map;\n}\n\nfunction parseWe" +
    "ekdayNumberSunday(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i + 1)" +
    ");\n  return n ? (d.w = +n[0], i + n[0].length) : -1;\n}\n\nfunction parseWeekdayNum" +
    "berMonday(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i + 1));\n  ret" +
    "urn n ? (d.u = +n[0], i + n[0].length) : -1;\n}\n\nfunction parseWeekNumberSunday(d" +
    ", string, i) {\n  var n = numberRe.exec(string.slice(i, i + 2));\n  return n ? (d." +
    "U = +n[0], i + n[0].length) : -1;\n}\n\nfunction parseWeekNumberISO(d, string, i) {" +
    "\n  var n = numberRe.exec(string.slice(i, i + 2));\n  return n ? (d.V = +n[0], i +" +
    " n[0].length) : -1;\n}\n\nfunction parseWeekNumberMonday(d, string, i) {\n  var n = " +
    "numberRe.exec(string.slice(i, i + 2));\n  return n ? (d.W = +n[0], i + n[0].lengt" +
    "h) : -1;\n}\n\nfunction parseFullYear(d, string, i) {\n  var n = numberRe.exec(strin" +
    "g.slice(i, i + 4));\n  return n ? (d.y = +n[0], i + n[0].length) : -1;\n}\n\nfunctio" +
    "n parseYear(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i + 2));\n  r" +
    "eturn n ? (d.y = +n[0] + (+n[0] > 68 ? 1900 : 2000), i + n[0].length) : -1;\n}\n\nf" +
    "unction parseZone(d, string, i) {\n  var n = /^(Z)|([+-]\\d\\d)(?::?(\\d\\d))?/.exec(" +
    "string.slice(i, i + 6));\n  return n ? (d.Z = n[1] ? 0 : -(n[2] + (n[3] || \"00\"))" +
    ", i + n[0].length) : -1;\n}\n\nfunction parseMonthNumber(d, string, i) {\n  var n = " +
    "numberRe.exec(string.slice(i, i + 2));\n  return n ? (d.m = n[0] - 1, i + n[0].le" +
    "ngth) : -1;\n}\n\nfunction parseDayOfMonth(d, string, i) {\n  var n = numberRe.exec(" +
    "string.slice(i, i + 2));\n  return n ? (d.d = +n[0], i + n[0].length) : -1;\n}\n\nfu" +
    "nction parseDayOfYear(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i " +
    "+ 3));\n  return n ? (d.m = 0, d.d = +n[0], i + n[0].length) : -1;\n}\n\nfunction pa" +
    "rseHour24(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i + 2));\n  ret" +
    "urn n ? (d.H = +n[0], i + n[0].length) : -1;\n}\n\nfunction parseMinutes(d, string," +
    " i) {\n  var n = numberRe.exec(string.slice(i, i + 2));\n  return n ? (d.M = +n[0]" +
    ", i + n[0].length) : -1;\n}\n\nfunction parseSeconds(d, string, i) {\n  var n = numb" +
    "erRe.exec(string.slice(i, i + 2));\n  return n ? (d.S = +n[0], i + n[0].length) :" +
    " -1;\n}\n\nfunction parseMilliseconds(d, string, i) {\n  var n = numberRe.exec(strin" +
    "g.slice(i, i + 3));\n  return n ? (d.L = +n[0], i + n[0].length) : -1;\n}\n\nfunctio" +
    "n parseMicroseconds(d, string, i) {\n  var n = numberRe.exec(string.slice(i, i + " +
    "6));\n  return n ? (d.L = Math.floor(n[0] / 1000), i + n[0].length) : -1;\n}\n\nfunc" +
    "tion parseLiteralPercent(d, string, i) {\n  var n = percentRe.exec(string.slice(i" +
    ", i + 1));\n  return n ? i + n[0].length : -1;\n}\n\nfunction parseUnixTimestamp(d, " +
    "string, i) {\n  var n = numberRe.exec(string.slice(i));\n  return n ? (d.Q = +n[0]" +
    ", i + n[0].length) : -1;\n}\n\nfunction parseUnixTimestampSeconds(d, string, i) {\n " +
    " var n = numberRe.exec(string.slice(i));\n  return n ? (d.Q = (+n[0]) * 1000, i +" +
    " n[0].length) : -1;\n}\n\nfunction formatDayOfMonth(d, p) {\n  return pad(d.getDate(" +
    "), p, 2);\n}\n\nfunction formatHour24(d, p) {\n  return pad(d.getHours(), p, 2);\n}\n\n" +
    "function formatHour12(d, p) {\n  return pad(d.getHours() % 12 || 12, p, 2);\n}\n\nfu" +
    "nction formatDayOfYear(d, p) {\n  return pad(1 + day.count(year(d), d), p, 3);\n}\n" +
    "\nfunction formatMilliseconds(d, p) {\n  return pad(d.getMilliseconds(), p, 3);\n}\n" +
    "\nfunction formatMicroseconds(d, p) {\n  return formatMilliseconds(d, p) + \"000\";\n" +
    "}\n\nfunction formatMonthNumber(d, p) {\n  return pad(d.getMonth() + 1, p, 2);\n}\n\nf" +
    "unction formatMinutes(d, p) {\n  return pad(d.getMinutes(), p, 2);\n}\n\nfunction fo" +
    "rmatSeconds(d, p) {\n  return pad(d.getSeconds(), p, 2);\n}\n\nfunction formatWeekda" +
    "yNumberMonday(d) {\n  var day$$1 = d.getDay();\n  return day$$1 === 0 ? 7 : day$$1" +
    ";\n}\n\nfunction formatWeekNumberSunday(d, p) {\n  return pad(sunday.count(year(d), " +
    "d), p, 2);\n}\n\nfunction formatWeekNumberISO(d, p) {\n  var day$$1 = d.getDay();\n  " +
    "d = (day$$1 >= 4 || day$$1 === 0) ? thursday(d) : thursday.ceil(d);\n  return pad" +
    "(thursday.count(year(d), d) + (year(d).getDay() === 4), p, 2);\n}\n\nfunction forma" +
    "tWeekdayNumberSunday(d) {\n  return d.getDay();\n}\n\nfunction formatWeekNumberMonda" +
    "y(d, p) {\n  return pad(monday.count(year(d), d), p, 2);\n}\n\nfunction formatYear(d" +
    ", p) {\n  return pad(d.getFullYear() % 100, p, 2);\n}\n\nfunction formatFullYear(d, " +
    "p) {\n  return pad(d.getFullYear() % 10000, p, 4);\n}\n\nfunction formatZone(d) {\n  " +
    "var z = d.getTimezoneOffset();\n  return (z > 0 ? \"-\" : (z *= -1, \"+\"))\n      + p" +
    "ad(z / 60 | 0, \"0\", 2)\n      + pad(z % 60, \"0\", 2);\n}\n\nfunction formatUTCDayOfMo" +
    "nth(d, p) {\n  return pad(d.getUTCDate(), p, 2);\n}\n\nfunction formatUTCHour24(d, p" +
    ") {\n  return pad(d.getUTCHours(), p, 2);\n}\n\nfunction formatUTCHour12(d, p) {\n  r" +
    "eturn pad(d.getUTCHours() % 12 || 12, p, 2);\n}\n\nfunction formatUTCDayOfYear(d, p" +
    ") {\n  return pad(1 + utcDay.count(utcYear(d), d), p, 3);\n}\n\nfunction formatUTCMi" +
    "lliseconds(d, p) {\n  return pad(d.getUTCMilliseconds(), p, 3);\n}\n\nfunction forma" +
    "tUTCMicroseconds(d, p) {\n  return formatUTCMilliseconds(d, p) + \"000\";\n}\n\nfuncti" +
    "on formatUTCMonthNumber(d, p) {\n  return pad(d.getUTCMonth() + 1, p, 2);\n}\n\nfunc" +
    "tion formatUTCMinutes(d, p) {\n  return pad(d.getUTCMinutes(), p, 2);\n}\n\nfunction" +
    " formatUTCSeconds(d, p) {\n  return pad(d.getUTCSeconds(), p, 2);\n}\n\nfunction for" +
    "matUTCWeekdayNumberMonday(d) {\n  var dow = d.getUTCDay();\n  return dow === 0 ? 7" +
    " : dow;\n}\n\nfunction formatUTCWeekNumberSunday(d, p) {\n  return pad(utcSunday.cou" +
    "nt(utcYear(d), d), p, 2);\n}\n\nfunction formatUTCWeekNumberISO(d, p) {\n  var day$$" +
    "1 = d.getUTCDay();\n  d = (day$$1 >= 4 || day$$1 === 0) ? utcThursday(d) : utcThu" +
    "rsday.ceil(d);\n  return pad(utcThursday.count(utcYear(d), d) + (utcYear(d).getUT" +
    "CDay() === 4), p, 2);\n}\n\nfunction formatUTCWeekdayNumberSunday(d) {\n  return d.g" +
    "etUTCDay();\n}\n\nfunction formatUTCWeekNumberMonday(d, p) {\n  return pad(utcMonday" +
    ".count(utcYear(d), d), p, 2);\n}\n\nfunction formatUTCYear(d, p) {\n  return pad(d.g" +
    "etUTCFullYear() % 100, p, 2);\n}\n\nfunction formatUTCFullYear(d, p) {\n  return pad" +
    "(d.getUTCFullYear() % 10000, p, 4);\n}\n\nfunction formatUTCZone() {\n  return \"+000" +
    "0\";\n}\n\nfunction formatLiteralPercent() {\n  return \"%\";\n}\n\nfunction formatUnixTim" +
    "estamp(d) {\n  return +d;\n}\n\nfunction formatUnixTimestampSeconds(d) {\n  return Ma" +
    "th.floor(+d / 1000);\n}\n\nvar locale$1;\nvar timeFormat;\nvar timeParse;\nvar utcForm" +
    "at;\nvar utcParse;\n\ndefaultLocale$1({\n  dateTime: \"%x, %X\",\n  date: \"%-m/%-d/%Y\"," +
    "\n  time: \"%-I:%M:%S %p\",\n  periods: [\"AM\", \"PM\"],\n  days: [\"Sunday\", \"Monday\", \"" +
    "Tuesday\", \"Wednesday\", \"Thursday\", \"Friday\", \"Saturday\"],\n  shortDays: [\"Sun\", \"" +
    "Mon\", \"Tue\", \"Wed\", \"Thu\", \"Fri\", \"Sat\"],\n  months: [\"January\", \"February\", \"Mar" +
    "ch\", \"April\", \"May\", \"June\", \"July\", \"August\", \"September\", \"October\", \"November" +
    "\", \"December\"],\n  shortMonths: [\"Jan\", \"Feb\", \"Mar\", \"Apr\", \"May\", \"Jun\", \"Jul\"," +
    " \"Aug\", \"Sep\", \"Oct\", \"Nov\", \"Dec\"]\n});\n\nfunction defaultLocale$1(definition) {\n" +
    "  locale$1 = formatLocale$1(definition);\n  timeFormat = locale$1.format;\n  timeP" +
    "arse = locale$1.parse;\n  utcFormat = locale$1.utcFormat;\n  utcParse = locale$1.u" +
    "tcParse;\n  return locale$1;\n}\n\nvar isoSpecifier = \"%Y-%m-%dT%H:%M:%S.%LZ\";\n\nfunc" +
    "tion formatIsoNative(date) {\n  return date.toISOString();\n}\n\nvar formatIso = Dat" +
    "e.prototype.toISOString\n    ? formatIsoNative\n    : utcFormat(isoSpecifier);\n\nfu" +
    "nction parseIsoNative(string) {\n  var date = new Date(string);\n  return isNaN(da" +
    "te) ? null : date;\n}\n\nvar parseIso = +new Date(\"2000-01-01T00:00:00.000Z\")\n    ?" +
    " parseIsoNative\n    : utcParse(isoSpecifier);\n\nvar colors = function(s) {\n  retu" +
    "rn s.match(/.{6}/g).map(function(x) {\n    return \"#\" + x;\n  });\n};\n\ncolors(\"1f77" +
    "b4ff7f0e2ca02cd627289467bd8c564be377c27f7f7fbcbd2217becf\");\n\ncolors(\"393b795254a" +
    "36b6ecf9c9ede6379398ca252b5cf6bcedb9c8c6d31bd9e39e7ba52e7cb94843c39ad494ad6616be" +
    "7969c7b4173a55194ce6dbdde9ed6\");\n\ncolors(\"3182bd6baed69ecae1c6dbefe6550dfd8d3cfd" +
    "ae6bfdd0a231a35474c476a1d99bc7e9c0756bb19e9ac8bcbddcdadaeb636363969696bdbdbdd9d9" +
    "d9\");\n\ncolors(\"1f77b4aec7e8ff7f0effbb782ca02c98df8ad62728ff98969467bdc5b0d58c564" +
    "bc49c94e377c2f7b6d27f7f7fc7c7c7bcbd22dbdb8d17becf9edae5\");\n\ncubehelixLong(cubehe" +
    "lix(300, 0.5, 0.0), cubehelix(-240, 0.5, 1.0));\n\nvar warm = cubehelixLong(cubehe" +
    "lix(-100, 0.75, 0.35), cubehelix(80, 1.50, 0.8));\n\nvar cool = cubehelixLong(cube" +
    "helix(260, 0.75, 0.35), cubehelix(80, 1.50, 0.8));\n\nvar rainbow = cubehelix();\n\n" +
    "function ramp(range) {\n  var n = range.length;\n  return function(t) {\n    return" +
    " range[Math.max(0, Math.min(n - 1, Math.floor(t * n)))];\n  };\n}\n\nramp(colors(\"44" +
    "015444025645045745055946075a46085c460a5d460b5e470d60470e614710634711644713654814" +
    "6748166848176948186a481a6c481b6d481c6e481d6f481f70482071482173482374482475482576" +
    "482677482878482979472a7a472c7a472d7b472e7c472f7d46307e46327e46337f46348045358145" +
    "3781453882443983443a83443b84433d84433e85423f854240864241864142874144874045884046" +
    "883f47883f48893e49893e4a893e4c8a3d4d8a3d4e8a3c4f8a3c508b3b518b3b528b3a538b3a548c" +
    "39558c39568c38588c38598c375a8c375b8d365c8d365d8d355e8d355f8d34608d34618d33628d33" +
    "638d32648e32658e31668e31678e31688e30698e306a8e2f6b8e2f6c8e2e6d8e2e6e8e2e6f8e2d70" +
    "8e2d718e2c718e2c728e2c738e2b748e2b758e2a768e2a778e2a788e29798e297a8e297b8e287c8e" +
    "287d8e277e8e277f8e27808e26818e26828e26828e25838e25848e25858e24868e24878e23888e23" +
    "898e238a8d228b8d228c8d228d8d218e8d218f8d21908d21918c20928c20928c20938c1f948c1f95" +
    "8b1f968b1f978b1f988b1f998a1f9a8a1e9b8a1e9c891e9d891f9e891f9f881fa0881fa1881fa187" +
    "1fa28720a38620a48621a58521a68522a78522a88423a98324aa8325ab8225ac8226ad8127ad8128" +
    "ae8029af7f2ab07f2cb17e2db27d2eb37c2fb47c31b57b32b67a34b67935b77937b87838b9773aba" +
    "763bbb753dbc743fbc7340bd7242be7144bf7046c06f48c16e4ac16d4cc26c4ec36b50c46a52c569" +
    "54c56856c66758c7655ac8645cc8635ec96260ca6063cb5f65cb5e67cc5c69cd5b6ccd5a6ece5870" +
    "cf5773d05675d05477d1537ad1517cd2507fd34e81d34d84d44b86d54989d5488bd6468ed64590d7" +
    "4393d74195d84098d83e9bd93c9dd93ba0da39a2da37a5db36a8db34aadc32addc30b0dd2fb2dd2d" +
    "b5de2bb8de29bade28bddf26c0df25c2df23c5e021c8e020cae11fcde11dd0e11cd2e21bd5e21ad8" +
    "e219dae319dde318dfe318e2e418e5e419e7e419eae51aece51befe51cf1e51df4e61ef6e620f8e6" +
    "21fbe723fde725\"));\n\nvar magma = ramp(colors(\"00000401000501010601010802010902020" +
    "b02020d03030f03031204041405041606051806051a07061c08071e0907200a08220b09240c09260" +
    "d0a290e0b2b100b2d110c2f120d31130d34140e36150e38160f3b180f3d19103f1a10421c10441d1" +
    "1471e114920114b21114e22115024125325125527125829115a2a115c2c115f2d11612f116331116" +
    "533106734106936106b38106c390f6e3b0f703d0f713f0f72400f74420f75440f764510774710784" +
    "910784a10794c117a4e117b4f127b51127c52137c54137d56147d57157e59157e5a167e5c167f5d1" +
    "77f5f187f601880621980641a80651a80671b80681c816a1c816b1d816d1d816e1e81701f81721f8" +
    "17320817521817621817822817922827b23827c23827e24828025828125818326818426818627818" +
    "827818928818b29818c29818e2a81902a81912b81932b80942c80962c80982d80992d809b2e7f9c2" +
    "e7f9e2f7fa02f7fa1307ea3307ea5317ea6317da8327daa337dab337cad347cae347bb0357bb2357" +
    "bb3367ab5367ab73779b83779ba3878bc3978bd3977bf3a77c03a76c23b75c43c75c53c74c73d73c" +
    "83e73ca3e72cc3f71cd4071cf4070d0416fd2426fd3436ed5446dd6456cd8456cd9466bdb476adc4" +
    "869de4968df4a68e04c67e24d66e34e65e44f64e55064e75263e85362e95462ea5661eb5760ec586" +
    "0ed5a5fee5b5eef5d5ef05f5ef1605df2625df2645cf3655cf4675cf4695cf56b5cf66c5cf66e5cf" +
    "7705cf7725cf8745cf8765cf9785df9795df97b5dfa7d5efa7f5efa815ffb835ffb8560fb8761fc8" +
    "961fc8a62fc8c63fc8e64fc9065fd9266fd9467fd9668fd9869fd9a6afd9b6bfe9d6cfe9f6dfea16" +
    "efea36ffea571fea772fea973feaa74feac76feae77feb078feb27afeb47bfeb67cfeb77efeb97ff" +
    "ebb81febd82febf84fec185fec287fec488fec68afec88cfeca8dfecc8ffecd90fecf92fed194fed" +
    "395fed597fed799fed89afdda9cfddc9efddea0fde0a1fde2a3fde3a5fde5a7fde7a9fde9aafdeba" +
    "cfcecaefceeb0fcf0b2fcf2b4fcf4b6fcf6b8fcf7b9fcf9bbfcfbbdfcfdbf\"));\n\nvar inferno =" +
    " ramp(colors(\"00000401000501010601010802010a02020c02020e030210040312040314050417" +
    "06041907051b08051d09061f0a07220b07240c08260d08290e092b10092d110a30120a32140b3415" +
    "0b37160b39180c3c190c3e1b0c411c0c431e0c451f0c48210c4a230c4c240c4f260c51280b53290b" +
    "552b0b572d0b592f0a5b310a5c320a5e340a5f3609613809623909633b09643d09653e0966400a67" +
    "420a68440a68450a69470b6a490b6a4a0c6b4c0c6b4d0d6c4f0d6c510e6c520e6d540f6d550f6d57" +
    "106e59106e5a116e5c126e5d126e5f136e61136e62146e64156e65156e67166e69166e6a176e6c18" +
    "6e6d186e6f196e71196e721a6e741a6e751b6e771c6d781c6d7a1d6d7c1d6d7d1e6d7f1e6c801f6c" +
    "82206c84206b85216b87216b88226a8a226a8c23698d23698f246990256892256893266795266797" +
    "27669827669a28659b29649d29649f2a63a02a63a22b62a32c61a52c60a62d60a82e5fa92e5eab2f" +
    "5ead305dae305cb0315bb1325ab3325ab43359b63458b73557b93556ba3655bc3754bd3853bf3952" +
    "c03a51c13a50c33b4fc43c4ec63d4dc73e4cc83f4bca404acb4149cc4248ce4347cf4446d04545d2" +
    "4644d34743d44842d54a41d74b3fd84c3ed94d3dda4e3cdb503bdd513ade5238df5337e05536e156" +
    "35e25734e35933e45a31e55c30e65d2fe75e2ee8602de9612bea632aeb6429eb6628ec6726ed6925" +
    "ee6a24ef6c23ef6e21f06f20f1711ff1731df2741cf3761bf37819f47918f57b17f57d15f67e14f6" +
    "8013f78212f78410f8850ff8870ef8890cf98b0bf98c0af98e09fa9008fa9207fa9407fb9606fb97" +
    "06fb9906fb9b06fb9d07fc9f07fca108fca309fca50afca60cfca80dfcaa0ffcac11fcae12fcb014" +
    "fcb216fcb418fbb61afbb81dfbba1ffbbc21fbbe23fac026fac228fac42afac62df9c72ff9c932f9" +
    "cb35f8cd37f8cf3af7d13df7d340f6d543f6d746f5d949f5db4cf4dd4ff4df53f4e156f3e35af3e5" +
    "5df2e661f2e865f2ea69f1ec6df1ed71f1ef75f1f179f2f27df2f482f3f586f3f68af4f88ef5f992" +
    "f6fa96f8fb9af9fc9dfafda1fcffa4\"));\n\nvar plasma = ramp(colors(\"0d0887100788130789" +
    "16078a19068c1b068d1d068e20068f2206902406912605912805922a05932c05942e05952f059631" +
    "059733059735049837049938049a3a049a3c049b3e049c3f049c41049d43039e44039e46039f4803" +
    "9f4903a04b03a14c02a14e02a25002a25102a35302a35502a45601a45801a45901a55b01a55c01a6" +
    "5e01a66001a66100a76300a76400a76600a76700a86900a86a00a86c00a86e00a86f00a87100a872" +
    "01a87401a87501a87701a87801a87a02a87b02a87d03a87e03a88004a88104a78305a78405a78606" +
    "a68707a68808a68a09a58b0aa58d0ba58e0ca48f0da4910ea3920fa39410a29511a19613a19814a0" +
    "99159f9a169f9c179e9d189d9e199da01a9ca11b9ba21d9aa31e9aa51f99a62098a72197a82296aa" +
    "2395ab2494ac2694ad2793ae2892b02991b12a90b22b8fb32c8eb42e8db52f8cb6308bb7318ab832" +
    "89ba3388bb3488bc3587bd3786be3885bf3984c03a83c13b82c23c81c33d80c43e7fc5407ec6417d" +
    "c7427cc8437bc9447aca457acb4679cc4778cc4977cd4a76ce4b75cf4c74d04d73d14e72d24f71d3" +
    "5171d45270d5536fd5546ed6556dd7566cd8576bd9586ada5a6ada5b69db5c68dc5d67dd5e66de5f" +
    "65de6164df6263e06363e16462e26561e26660e3685fe4695ee56a5de56b5de66c5ce76e5be76f5a" +
    "e87059e97158e97257ea7457eb7556eb7655ec7754ed7953ed7a52ee7b51ef7c51ef7e50f07f4ff0" +
    "804ef1814df1834cf2844bf3854bf3874af48849f48948f58b47f58c46f68d45f68f44f79044f791" +
    "43f79342f89441f89540f9973ff9983ef99a3efa9b3dfa9c3cfa9e3bfb9f3afba139fba238fca338" +
    "fca537fca636fca835fca934fdab33fdac33fdae32fdaf31fdb130fdb22ffdb42ffdb52efeb72dfe" +
    "b82cfeba2cfebb2bfebd2afebe2afec029fdc229fdc328fdc527fdc627fdc827fdca26fdcb26fccd" +
    "25fcce25fcd025fcd225fbd324fbd524fbd724fad824fada24f9dc24f9dd25f8df25f8e125f7e225" +
    "f7e425f6e626f6e826f5e926f5eb27f4ed27f3ee27f3f027f2f227f1f426f1f525f0f724f0f921\")" +
    ");\n\nfunction cubicInOut(t) {\n  return ((t *= 2) <= 1 ? t * t * t : (t -= 2) * t " +
    "* t + 2) / 2;\n}\n\nvar pi = Math.PI;\n\nvar tau = 2 * Math.PI;\n\nvar noop = {value: f" +
    "unction() {}};\n\nfunction dispatch() {\n  for (var i = 0, n = arguments.length, _ " +
    "= {}, t; i < n; ++i) {\n    if (!(t = arguments[i] + \"\") || (t in _)) throw new E" +
    "rror(\"illegal type: \" + t);\n    _[t] = [];\n  }\n  return new Dispatch(_);\n}\n\nfunc" +
    "tion Dispatch(_) {\n  this._ = _;\n}\n\nfunction parseTypenames$1(typenames, types) " +
    "{\n  return typenames.trim().split(/^|\\s+/).map(function(t) {\n    var name = \"\", " +
    "i = t.indexOf(\".\");\n    if (i >= 0) name = t.slice(i + 1), t = t.slice(0, i);\n  " +
    "  if (t && !types.hasOwnProperty(t)) throw new Error(\"unknown type: \" + t);\n    " +
    "return {type: t, name: name};\n  });\n}\n\nDispatch.prototype = dispatch.prototype =" +
    " {\n  constructor: Dispatch,\n  on: function(typename, callback) {\n    var _ = thi" +
    "s._,\n        T = parseTypenames$1(typename + \"\", _),\n        t,\n        i = -1,\n" +
    "        n = T.length;\n\n    // If no callback was specified, return the callback " +
    "of the given type and name.\n    if (arguments.length < 2) {\n      while (++i < n" +
    ") if ((t = (typename = T[i]).type) && (t = get$1(_[t], typename.name))) return t" +
    ";\n      return;\n    }\n\n    // If a type was specified, set the callback for the " +
    "given type and name.\n    // Otherwise, if a null callback was specified, remove " +
    "callbacks of the given name.\n    if (callback != null && typeof callback !== \"fu" +
    "nction\") throw new Error(\"invalid callback: \" + callback);\n    while (++i < n) {" +
    "\n      if (t = (typename = T[i]).type) _[t] = set$3(_[t], typename.name, callbac" +
    "k);\n      else if (callback == null) for (t in _) _[t] = set$3(_[t], typename.na" +
    "me, null);\n    }\n\n    return this;\n  },\n  copy: function() {\n    var copy = {}, " +
    "_ = this._;\n    for (var t in _) copy[t] = _[t].slice();\n    return new Dispatch" +
    "(copy);\n  },\n  call: function(type, that) {\n    if ((n = arguments.length - 2) >" +
    " 0) for (var args = new Array(n), i = 0, n, t; i < n; ++i) args[i] = arguments[i" +
    " + 2];\n    if (!this._.hasOwnProperty(type)) throw new Error(\"unknown type: \" + " +
    "type);\n    for (t = this._[type], i = 0, n = t.length; i < n; ++i) t[i].value.ap" +
    "ply(that, args);\n  },\n  apply: function(type, that, args) {\n    if (!this._.hasO" +
    "wnProperty(type)) throw new Error(\"unknown type: \" + type);\n    for (var t = thi" +
    "s._[type], i = 0, n = t.length; i < n; ++i) t[i].value.apply(that, args);\n  }\n};" +
    "\n\nfunction get$1(type, name) {\n  for (var i = 0, n = type.length, c; i < n; ++i)" +
    " {\n    if ((c = type[i]).name === name) {\n      return c.value;\n    }\n  }\n}\n\nfun" +
    "ction set$3(type, name, callback) {\n  for (var i = 0, n = type.length; i < n; ++" +
    "i) {\n    if (type[i].name === name) {\n      type[i] = noop, type = type.slice(0," +
    " i).concat(type.slice(i + 1));\n      break;\n    }\n  }\n  if (callback != null) ty" +
    "pe.push({name: name, value: callback});\n  return type;\n}\n\nvar frame = 0;\nvar tim" +
    "eout = 0;\nvar interval = 0;\nvar pokeDelay = 1000;\nvar taskHead;\nvar taskTail;\nva" +
    "r clockLast = 0;\nvar clockNow = 0;\nvar clockSkew = 0;\nvar clock = typeof perform" +
    "ance === \"object\" && performance.now ? performance : Date;\nvar setFrame = typeof" +
    " window === \"object\" && window.requestAnimationFrame ? window.requestAnimationFr" +
    "ame.bind(window) : function(f) { setTimeout(f, 17); };\n\nfunction now() {\n  retur" +
    "n clockNow || (setFrame(clearNow), clockNow = clock.now() + clockSkew);\n}\n\nfunct" +
    "ion clearNow() {\n  clockNow = 0;\n}\n\nfunction Timer() {\n  this._call =\n  this._ti" +
    "me =\n  this._next = null;\n}\n\nTimer.prototype = timer.prototype = {\n  constructor" +
    ": Timer,\n  restart: function(callback, delay, time) {\n    if (typeof callback !=" +
    "= \"function\") throw new TypeError(\"callback is not a function\");\n    time = (tim" +
    "e == null ? now() : +time) + (delay == null ? 0 : +delay);\n    if (!this._next &" +
    "& taskTail !== this) {\n      if (taskTail) taskTail._next = this;\n      else tas" +
    "kHead = this;\n      taskTail = this;\n    }\n    this._call = callback;\n    this._" +
    "time = time;\n    sleep();\n  },\n  stop: function() {\n    if (this._call) {\n      " +
    "this._call = null;\n      this._time = Infinity;\n      sleep();\n    }\n  }\n};\n\nfun" +
    "ction timer(callback, delay, time) {\n  var t = new Timer;\n  t.restart(callback, " +
    "delay, time);\n  return t;\n}\n\nfunction timerFlush() {\n  now(); // Get the current" +
    " time, if not already set.\n  ++frame; // Pretend we’ve set an alarm, if we haven" +
    "’t already.\n  var t = taskHead, e;\n  while (t) {\n    if ((e = clockNow - t._time" +
    ") >= 0) t._call.call(null, e);\n    t = t._next;\n  }\n  --frame;\n}\n\nfunction wake(" +
    ") {\n  clockNow = (clockLast = clock.now()) + clockSkew;\n  frame = timeout = 0;\n " +
    " try {\n    timerFlush();\n  } finally {\n    frame = 0;\n    nap();\n    clockNow = " +
    "0;\n  }\n}\n\nfunction poke() {\n  var now = clock.now(), delay = now - clockLast;\n  " +
    "if (delay > pokeDelay) clockSkew -= delay, clockLast = now;\n}\n\nfunction nap() {\n" +
    "  var t0, t1 = taskHead, t2, time = Infinity;\n  while (t1) {\n    if (t1._call) {" +
    "\n      if (time > t1._time) time = t1._time;\n      t0 = t1, t1 = t1._next;\n    }" +
    " else {\n      t2 = t1._next, t1._next = null;\n      t1 = t0 ? t0._next = t2 : ta" +
    "skHead = t2;\n    }\n  }\n  taskTail = t0;\n  sleep(time);\n}\n\nfunction sleep(time) {" +
    "\n  if (frame) return; // Soonest alarm already set, or will be.\n  if (timeout) t" +
    "imeout = clearTimeout(timeout);\n  var delay = time - clockNow; // Strictly less " +
    "than if we recomputed clockNow.\n  if (delay > 24) {\n    if (time < Infinity) tim" +
    "eout = setTimeout(wake, time - clock.now() - clockSkew);\n    if (interval) inter" +
    "val = clearInterval(interval);\n  } else {\n    if (!interval) clockLast = clock.n" +
    "ow(), interval = setInterval(poke, pokeDelay);\n    frame = 1, setFrame(wake);\n  " +
    "}\n}\n\nvar timeout$1 = function(callback, delay, time) {\n  var t = new Timer;\n  de" +
    "lay = delay == null ? 0 : +delay;\n  t.restart(function(elapsed) {\n    t.stop();\n" +
    "    callback(elapsed + delay);\n  }, delay, time);\n  return t;\n};\n\nvar emptyOn = " +
    "dispatch(\"start\", \"end\", \"interrupt\");\nvar emptyTween = [];\n\nvar CREATED = 0;\nva" +
    "r SCHEDULED = 1;\nvar STARTING = 2;\nvar STARTED = 3;\nvar RUNNING = 4;\nvar ENDING " +
    "= 5;\nvar ENDED = 6;\n\nvar schedule = function(node, name, id, index, group, timin" +
    "g) {\n  var schedules = node.__transition;\n  if (!schedules) node.__transition = " +
    "{};\n  else if (id in schedules) return;\n  create(node, id, {\n    name: name,\n   " +
    " index: index, // For context during callback.\n    group: group, // For context " +
    "during callback.\n    on: emptyOn,\n    tween: emptyTween,\n    time: timing.time,\n" +
    "    delay: timing.delay,\n    duration: timing.duration,\n    ease: timing.ease,\n " +
    "   timer: null,\n    state: CREATED\n  });\n};\n\nfunction init(node, id) {\n  var sch" +
    "edule = node.__transition;\n  if (!schedule || !(schedule = schedule[id]) || sche" +
    "dule.state > CREATED) throw new Error(\"too late\");\n  return schedule;\n}\n\nfunctio" +
    "n set$2(node, id) {\n  var schedule = node.__transition;\n  if (!schedule || !(sch" +
    "edule = schedule[id]) || schedule.state > STARTING) throw new Error(\"too late\");" +
    "\n  return schedule;\n}\n\nfunction get(node, id) {\n  var schedule = node.__transiti" +
    "on;\n  if (!schedule || !(schedule = schedule[id])) throw new Error(\"too late\");\n" +
    "  return schedule;\n}\n\nfunction create(node, id, self) {\n  var schedules = node._" +
    "_transition,\n      tween;\n\n  // Initialize the self timer when the transition is" +
    " created.\n  // Note the actual delay is not known until the first callback!\n  sc" +
    "hedules[id] = self;\n  self.timer = timer(schedule, 0, self.time);\n\n  function sc" +
    "hedule(elapsed) {\n    self.state = SCHEDULED;\n    self.timer.restart(start, self" +
    ".delay, self.time);\n\n    // If the elapsed delay is less than our first sleep, s" +
    "tart immediately.\n    if (self.delay <= elapsed) start(elapsed - self.delay);\n  " +
    "}\n\n  function start(elapsed) {\n    var i, j, n, o;\n\n    // If the state is not S" +
    "CHEDULED, then we previously errored on start.\n    if (self.state !== SCHEDULED)" +
    " return stop();\n\n    for (i in schedules) {\n      o = schedules[i];\n      if (o." +
    "name !== self.name) continue;\n\n      // While this element already has a startin" +
    "g transition during this frame,\n      // defer starting an interrupting transiti" +
    "on until that transition has a\n      // chance to tick (and possibly end); see d" +
    "3/d3-transition#54!\n      if (o.state === STARTED) return timeout$1(start);\n\n   " +
    "   // Interrupt the active transition, if any.\n      // Dispatch the interrupt e" +
    "vent.\n      if (o.state === RUNNING) {\n        o.state = ENDED;\n        o.timer." +
    "stop();\n        o.on.call(\"interrupt\", node, node.__data__, o.index, o.group);\n " +
    "       delete schedules[i];\n      }\n\n      // Cancel any pre-empted transitions." +
    " No interrupt event is dispatched\n      // because the cancelled transitions nev" +
    "er started. Note that this also\n      // removes this transition from the pendin" +
    "g list!\n      else if (+i < id) {\n        o.state = ENDED;\n        o.timer.stop(" +
    ");\n        delete schedules[i];\n      }\n    }\n\n    // Defer the first tick to en" +
    "d of the current frame; see d3/d3#1576.\n    // Note the transition may be cancel" +
    "ed after start and before the first tick!\n    // Note this must be scheduled bef" +
    "ore the start event; see d3/d3-transition#16!\n    // Assuming this is successful" +
    ", subsequent callbacks go straight to tick.\n    timeout$1(function() {\n      if " +
    "(self.state === STARTED) {\n        self.state = RUNNING;\n        self.timer.rest" +
    "art(tick, self.delay, self.time);\n        tick(elapsed);\n      }\n    });\n\n    //" +
    " Dispatch the start event.\n    // Note this must be done before the tween are in" +
    "itialized.\n    self.state = STARTING;\n    self.on.call(\"start\", node, node.__dat" +
    "a__, self.index, self.group);\n    if (self.state !== STARTING) return; // interr" +
    "upted\n    self.state = STARTED;\n\n    // Initialize the tween, deleting null twee" +
    "n.\n    tween = new Array(n = self.tween.length);\n    for (i = 0, j = -1; i < n; " +
    "++i) {\n      if (o = self.tween[i].value.call(node, node.__data__, self.index, s" +
    "elf.group)) {\n        tween[++j] = o;\n      }\n    }\n    tween.length = j + 1;\n  " +
    "}\n\n  function tick(elapsed) {\n    var t = elapsed < self.duration ? self.ease.ca" +
    "ll(null, elapsed / self.duration) : (self.timer.restart(stop), self.state = ENDI" +
    "NG, 1),\n        i = -1,\n        n = tween.length;\n\n    while (++i < n) {\n      t" +
    "ween[i].call(null, t);\n    }\n\n    // Dispatch the end event.\n    if (self.state " +
    "=== ENDING) {\n      self.on.call(\"end\", node, node.__data__, self.index, self.gr" +
    "oup);\n      stop();\n    }\n  }\n\n  function stop() {\n    self.state = ENDED;\n    s" +
    "elf.timer.stop();\n    delete schedules[id];\n    for (var i in schedules) return;" +
    " // eslint-disable-line no-unused-vars\n    delete node.__transition;\n  }\n}\n\nvar " +
    "interrupt = function(node, name) {\n  var schedules = node.__transition,\n      sc" +
    "hedule$$1,\n      active,\n      empty = true,\n      i;\n\n  if (!schedules) return;" +
    "\n\n  name = name == null ? null : name + \"\";\n\n  for (i in schedules) {\n    if ((s" +
    "chedule$$1 = schedules[i]).name !== name) { empty = false; continue; }\n    activ" +
    "e = schedule$$1.state > STARTING && schedule$$1.state < ENDING;\n    schedule$$1." +
    "state = ENDED;\n    schedule$$1.timer.stop();\n    if (active) schedule$$1.on.call" +
    "(\"interrupt\", node, node.__data__, schedule$$1.index, schedule$$1.group);\n    de" +
    "lete schedules[i];\n  }\n\n  if (empty) delete node.__transition;\n};\n\nvar selection" +
    "_interrupt = function(name) {\n  return this.each(function() {\n    interrupt(this" +
    ", name);\n  });\n};\n\nfunction tweenRemove(id, name) {\n  var tween0, tween1;\n  retu" +
    "rn function() {\n    var schedule$$1 = set$2(this, id),\n        tween = schedule$" +
    "$1.tween;\n\n    // If this node shared tween with the previous node,\n    // just " +
    "assign the updated shared tween and we’re done!\n    // Otherwise, copy-on-write." +
    "\n    if (tween !== tween0) {\n      tween1 = tween0 = tween;\n      for (var i = 0" +
    ", n = tween1.length; i < n; ++i) {\n        if (tween1[i].name === name) {\n      " +
    "    tween1 = tween1.slice();\n          tween1.splice(i, 1);\n          break;\n   " +
    "     }\n      }\n    }\n\n    schedule$$1.tween = tween1;\n  };\n}\n\nfunction tweenFunc" +
    "tion(id, name, value) {\n  var tween0, tween1;\n  if (typeof value !== \"function\")" +
    " throw new Error;\n  return function() {\n    var schedule$$1 = set$2(this, id),\n " +
    "       tween = schedule$$1.tween;\n\n    // If this node shared tween with the pre" +
    "vious node,\n    // just assign the updated shared tween and we’re done!\n    // O" +
    "therwise, copy-on-write.\n    if (tween !== tween0) {\n      tween1 = (tween0 = tw" +
    "een).slice();\n      for (var t = {name: name, value: value}, i = 0, n = tween1.l" +
    "ength; i < n; ++i) {\n        if (tween1[i].name === name) {\n          tween1[i] " +
    "= t;\n          break;\n        }\n      }\n      if (i === n) tween1.push(t);\n    }" +
    "\n\n    schedule$$1.tween = tween1;\n  };\n}\n\nvar transition_tween = function(name, " +
    "value) {\n  var id = this._id;\n\n  name += \"\";\n\n  if (arguments.length < 2) {\n    " +
    "var tween = get(this.node(), id).tween;\n    for (var i = 0, n = tween.length, t;" +
    " i < n; ++i) {\n      if ((t = tween[i]).name === name) {\n        return t.value;" +
    "\n      }\n    }\n    return null;\n  }\n\n  return this.each((value == null ? tweenRe" +
    "move : tweenFunction)(id, name, value));\n};\n\nfunction tweenValue(transition, nam" +
    "e, value) {\n  var id = transition._id;\n\n  transition.each(function() {\n    var s" +
    "chedule$$1 = set$2(this, id);\n    (schedule$$1.value || (schedule$$1.value = {})" +
    ")[name] = value.apply(this, arguments);\n  });\n\n  return function(node) {\n    ret" +
    "urn get(node, id).value[name];\n  };\n}\n\nvar interpolate = function(a, b) {\n  var " +
    "c;\n  return (typeof b === \"number\" ? interpolateNumber\n      : b instanceof colo" +
    "r ? interpolateRgb\n      : (c = color(b)) ? (b = c, interpolateRgb)\n      : inte" +
    "rpolateString)(a, b);\n};\n\nfunction attrRemove$1(name) {\n  return function() {\n  " +
    "  this.removeAttribute(name);\n  };\n}\n\nfunction attrRemoveNS$1(fullname) {\n  retu" +
    "rn function() {\n    this.removeAttributeNS(fullname.space, fullname.local);\n  };" +
    "\n}\n\nfunction attrConstant$1(name, interpolate$$1, value1) {\n  var value00,\n     " +
    " interpolate0;\n  return function() {\n    var value0 = this.getAttribute(name);\n " +
    "   return value0 === value1 ? null\n        : value0 === value00 ? interpolate0\n " +
    "       : interpolate0 = interpolate$$1(value00 = value0, value1);\n  };\n}\n\nfuncti" +
    "on attrConstantNS$1(fullname, interpolate$$1, value1) {\n  var value00,\n      int" +
    "erpolate0;\n  return function() {\n    var value0 = this.getAttributeNS(fullname.s" +
    "pace, fullname.local);\n    return value0 === value1 ? null\n        : value0 === " +
    "value00 ? interpolate0\n        : interpolate0 = interpolate$$1(value00 = value0," +
    " value1);\n  };\n}\n\nfunction attrFunction$1(name, interpolate$$1, value) {\n  var v" +
    "alue00,\n      value10,\n      interpolate0;\n  return function() {\n    var value0," +
    " value1 = value(this);\n    if (value1 == null) return void this.removeAttribute(" +
    "name);\n    value0 = this.getAttribute(name);\n    return value0 === value1 ? null" +
    "\n        : value0 === value00 && value1 === value10 ? interpolate0\n        : int" +
    "erpolate0 = interpolate$$1(value00 = value0, value10 = value1);\n  };\n}\n\nfunction" +
    " attrFunctionNS$1(fullname, interpolate$$1, value) {\n  var value00,\n      value1" +
    "0,\n      interpolate0;\n  return function() {\n    var value0, value1 = value(this" +
    ");\n    if (value1 == null) return void this.removeAttributeNS(fullname.space, fu" +
    "llname.local);\n    value0 = this.getAttributeNS(fullname.space, fullname.local);" +
    "\n    return value0 === value1 ? null\n        : value0 === value00 && value1 === " +
    "value10 ? interpolate0\n        : interpolate0 = interpolate$$1(value00 = value0," +
    " value10 = value1);\n  };\n}\n\nvar transition_attr = function(name, value) {\n  var " +
    "fullname = namespace(name), i = fullname === \"transform\" ? interpolateTransformS" +
    "vg : interpolate;\n  return this.attrTween(name, typeof value === \"function\"\n    " +
    "  ? (fullname.local ? attrFunctionNS$1 : attrFunction$1)(fullname, i, tweenValue" +
    "(this, \"attr.\" + name, value))\n      : value == null ? (fullname.local ? attrRem" +
    "oveNS$1 : attrRemove$1)(fullname)\n      : (fullname.local ? attrConstantNS$1 : a" +
    "ttrConstant$1)(fullname, i, value + \"\"));\n};\n\nfunction attrTweenNS(fullname, val" +
    "ue) {\n  function tween() {\n    var node = this, i = value.apply(node, arguments)" +
    ";\n    return i && function(t) {\n      node.setAttributeNS(fullname.space, fullna" +
    "me.local, i(t));\n    };\n  }\n  tween._value = value;\n  return tween;\n}\n\nfunction " +
    "attrTween(name, value) {\n  function tween() {\n    var node = this, i = value.app" +
    "ly(node, arguments);\n    return i && function(t) {\n      node.setAttribute(name," +
    " i(t));\n    };\n  }\n  tween._value = value;\n  return tween;\n}\n\nvar transition_att" +
    "rTween = function(name, value) {\n  var key = \"attr.\" + name;\n  if (arguments.len" +
    "gth < 2) return (key = this.tween(key)) && key._value;\n  if (value == null) retu" +
    "rn this.tween(key, null);\n  if (typeof value !== \"function\") throw new Error;\n  " +
    "var fullname = namespace(name);\n  return this.tween(key, (fullname.local ? attrT" +
    "weenNS : attrTween)(fullname, value));\n};\n\nfunction delayFunction(id, value) {\n " +
    " return function() {\n    init(this, id).delay = +value.apply(this, arguments);\n " +
    " };\n}\n\nfunction delayConstant(id, value) {\n  return value = +value, function() {" +
    "\n    init(this, id).delay = value;\n  };\n}\n\nvar transition_delay = function(value" +
    ") {\n  var id = this._id;\n\n  return arguments.length\n      ? this.each((typeof va" +
    "lue === \"function\"\n          ? delayFunction\n          : delayConstant)(id, valu" +
    "e))\n      : get(this.node(), id).delay;\n};\n\nfunction durationFunction(id, value)" +
    " {\n  return function() {\n    set$2(this, id).duration = +value.apply(this, argum" +
    "ents);\n  };\n}\n\nfunction durationConstant(id, value) {\n  return value = +value, f" +
    "unction() {\n    set$2(this, id).duration = value;\n  };\n}\n\nvar transition_duratio" +
    "n = function(value) {\n  var id = this._id;\n\n  return arguments.length\n      ? th" +
    "is.each((typeof value === \"function\"\n          ? durationFunction\n          : du" +
    "rationConstant)(id, value))\n      : get(this.node(), id).duration;\n};\n\nfunction " +
    "easeConstant(id, value) {\n  if (typeof value !== \"function\") throw new Error;\n  " +
    "return function() {\n    set$2(this, id).ease = value;\n  };\n}\n\nvar transition_eas" +
    "e = function(value) {\n  var id = this._id;\n\n  return arguments.length\n      ? th" +
    "is.each(easeConstant(id, value))\n      : get(this.node(), id).ease;\n};\n\nvar tran" +
    "sition_filter = function(match) {\n  if (typeof match !== \"function\") match = mat" +
    "cher$1(match);\n\n  for (var groups = this._groups, m = groups.length, subgroups =" +
    " new Array(m), j = 0; j < m; ++j) {\n    for (var group = groups[j], n = group.le" +
    "ngth, subgroup = subgroups[j] = [], node, i = 0; i < n; ++i) {\n      if ((node =" +
    " group[i]) && match.call(node, node.__data__, i, group)) {\n        subgroup.push" +
    "(node);\n      }\n    }\n  }\n\n  return new Transition(subgroups, this._parents, thi" +
    "s._name, this._id);\n};\n\nvar transition_merge = function(transition$$1) {\n  if (t" +
    "ransition$$1._id !== this._id) throw new Error;\n\n  for (var groups0 = this._grou" +
    "ps, groups1 = transition$$1._groups, m0 = groups0.length, m1 = groups1.length, m" +
    " = Math.min(m0, m1), merges = new Array(m0), j = 0; j < m; ++j) {\n    for (var g" +
    "roup0 = groups0[j], group1 = groups1[j], n = group0.length, merge = merges[j] = " +
    "new Array(n), node, i = 0; i < n; ++i) {\n      if (node = group0[i] || group1[i]" +
    ") {\n        merge[i] = node;\n      }\n    }\n  }\n\n  for (; j < m0; ++j) {\n    merg" +
    "es[j] = groups0[j];\n  }\n\n  return new Transition(merges, this._parents, this._na" +
    "me, this._id);\n};\n\nfunction start(name) {\n  return (name + \"\").trim().split(/^|\\" +
    "s+/).every(function(t) {\n    var i = t.indexOf(\".\");\n    if (i >= 0) t = t.slice" +
    "(0, i);\n    return !t || t === \"start\";\n  });\n}\n\nfunction onFunction(id, name, l" +
    "istener) {\n  var on0, on1, sit = start(name) ? init : set$2;\n  return function()" +
    " {\n    var schedule$$1 = sit(this, id),\n        on = schedule$$1.on;\n\n    // If " +
    "this node shared a dispatch with the previous node,\n    // just assign the updat" +
    "ed shared dispatch and we’re done!\n    // Otherwise, copy-on-write.\n    if (on !" +
    "== on0) (on1 = (on0 = on).copy()).on(name, listener);\n\n    schedule$$1.on = on1;" +
    "\n  };\n}\n\nvar transition_on = function(name, listener) {\n  var id = this._id;\n\n  " +
    "return arguments.length < 2\n      ? get(this.node(), id).on.on(name)\n      : thi" +
    "s.each(onFunction(id, name, listener));\n};\n\nfunction removeFunction(id) {\n  retu" +
    "rn function() {\n    var parent = this.parentNode;\n    for (var i in this.__trans" +
    "ition) if (+i !== id) return;\n    if (parent) parent.removeChild(this);\n  };\n}\n\n" +
    "var transition_remove = function() {\n  return this.on(\"end.remove\", removeFuncti" +
    "on(this._id));\n};\n\nvar transition_select = function(select) {\n  var name = this." +
    "_name,\n      id = this._id;\n\n  if (typeof select !== \"function\") select = select" +
    "or(select);\n\n  for (var groups = this._groups, m = groups.length, subgroups = ne" +
    "w Array(m), j = 0; j < m; ++j) {\n    for (var group = groups[j], n = group.lengt" +
    "h, subgroup = subgroups[j] = new Array(n), node, subnode, i = 0; i < n; ++i) {\n " +
    "     if ((node = group[i]) && (subnode = select.call(node, node.__data__, i, gro" +
    "up))) {\n        if (\"__data__\" in node) subnode.__data__ = node.__data__;\n      " +
    "  subgroup[i] = subnode;\n        schedule(subgroup[i], name, id, i, subgroup, ge" +
    "t(node, id));\n      }\n    }\n  }\n\n  return new Transition(subgroups, this._parent" +
    "s, name, id);\n};\n\nvar transition_selectAll = function(select) {\n  var name = thi" +
    "s._name,\n      id = this._id;\n\n  if (typeof select !== \"function\") select = sele" +
    "ctorAll(select);\n\n  for (var groups = this._groups, m = groups.length, subgroups" +
    " = [], parents = [], j = 0; j < m; ++j) {\n    for (var group = groups[j], n = gr" +
    "oup.length, node, i = 0; i < n; ++i) {\n      if (node = group[i]) {\n        for " +
    "(var children = select.call(node, node.__data__, i, group), child, inherit = get" +
    "(node, id), k = 0, l = children.length; k < l; ++k) {\n          if (child = chil" +
    "dren[k]) {\n            schedule(child, name, id, k, children, inherit);\n        " +
    "  }\n        }\n        subgroups.push(children);\n        parents.push(node);\n    " +
    "  }\n    }\n  }\n\n  return new Transition(subgroups, parents, name, id);\n};\n\nvar Se" +
    "lection$1 = selection.prototype.constructor;\n\nvar transition_selection = functio" +
    "n() {\n  return new Selection$1(this._groups, this._parents);\n};\n\nfunction styleR" +
    "emove$1(name, interpolate$$1) {\n  var value00,\n      value10,\n      interpolate0" +
    ";\n  return function() {\n    var value0 = styleValue(this, name),\n        value1 " +
    "= (this.style.removeProperty(name), styleValue(this, name));\n    return value0 =" +
    "== value1 ? null\n        : value0 === value00 && value1 === value10 ? interpolat" +
    "e0\n        : interpolate0 = interpolate$$1(value00 = value0, value10 = value1);\n" +
    "  };\n}\n\nfunction styleRemoveEnd(name) {\n  return function() {\n    this.style.rem" +
    "oveProperty(name);\n  };\n}\n\nfunction styleConstant$1(name, interpolate$$1, value1" +
    ") {\n  var value00,\n      interpolate0;\n  return function() {\n    var value0 = st" +
    "yleValue(this, name);\n    return value0 === value1 ? null\n        : value0 === v" +
    "alue00 ? interpolate0\n        : interpolate0 = interpolate$$1(value00 = value0, " +
    "value1);\n  };\n}\n\nfunction styleFunction$1(name, interpolate$$1, value) {\n  var v" +
    "alue00,\n      value10,\n      interpolate0;\n  return function() {\n    var value0 " +
    "= styleValue(this, name),\n        value1 = value(this);\n    if (value1 == null) " +
    "value1 = (this.style.removeProperty(name), styleValue(this, name));\n    return v" +
    "alue0 === value1 ? null\n        : value0 === value00 && value1 === value10 ? int" +
    "erpolate0\n        : interpolate0 = interpolate$$1(value00 = value0, value10 = va" +
    "lue1);\n  };\n}\n\nvar transition_style = function(name, value, priority) {\n  var i " +
    "= (name += \"\") === \"transform\" ? interpolateTransformCss : interpolate;\n  return" +
    " value == null ? this\n          .styleTween(name, styleRemove$1(name, i))\n      " +
    "    .on(\"end.style.\" + name, styleRemoveEnd(name))\n      : this.styleTween(name," +
    " typeof value === \"function\"\n          ? styleFunction$1(name, i, tweenValue(thi" +
    "s, \"style.\" + name, value))\n          : styleConstant$1(name, i, value + \"\"), pr" +
    "iority);\n};\n\nfunction styleTween(name, value, priority) {\n  function tween() {\n " +
    "   var node = this, i = value.apply(node, arguments);\n    return i && function(t" +
    ") {\n      node.style.setProperty(name, i(t), priority);\n    };\n  }\n  tween._valu" +
    "e = value;\n  return tween;\n}\n\nvar transition_styleTween = function(name, value, " +
    "priority) {\n  var key = \"style.\" + (name += \"\");\n  if (arguments.length < 2) ret" +
    "urn (key = this.tween(key)) && key._value;\n  if (value == null) return this.twee" +
    "n(key, null);\n  if (typeof value !== \"function\") throw new Error;\n  return this." +
    "tween(key, styleTween(name, value, priority == null ? \"\" : priority));\n};\n\nfunct" +
    "ion textConstant$1(value) {\n  return function() {\n    this.textContent = value;\n" +
    "  };\n}\n\nfunction textFunction$1(value) {\n  return function() {\n    var value1 = " +
    "value(this);\n    this.textContent = value1 == null ? \"\" : value1;\n  };\n}\n\nvar tr" +
    "ansition_text = function(value) {\n  return this.tween(\"text\", typeof value === \"" +
    "function\"\n      ? textFunction$1(tweenValue(this, \"text\", value))\n      : textCo" +
    "nstant$1(value == null ? \"\" : value + \"\"));\n};\n\nvar transition_transition = func" +
    "tion() {\n  var name = this._name,\n      id0 = this._id,\n      id1 = newId();\n\n  " +
    "for (var groups = this._groups, m = groups.length, j = 0; j < m; ++j) {\n    for " +
    "(var group = groups[j], n = group.length, node, i = 0; i < n; ++i) {\n      if (n" +
    "ode = group[i]) {\n        var inherit = get(node, id0);\n        schedule(node, n" +
    "ame, id1, i, group, {\n          time: inherit.time + inherit.delay + inherit.dur" +
    "ation,\n          delay: 0,\n          duration: inherit.duration,\n          ease:" +
    " inherit.ease\n        });\n      }\n    }\n  }\n\n  return new Transition(groups, thi" +
    "s._parents, name, id1);\n};\n\nvar id = 0;\n\nfunction Transition(groups, parents, na" +
    "me, id) {\n  this._groups = groups;\n  this._parents = parents;\n  this._name = nam" +
    "e;\n  this._id = id;\n}\n\nfunction transition(name) {\n  return selection().transiti" +
    "on(name);\n}\n\nfunction newId() {\n  return ++id;\n}\n\nvar selection_prototype = sele" +
    "ction.prototype;\n\nTransition.prototype = transition.prototype = {\n  constructor:" +
    " Transition,\n  select: transition_select,\n  selectAll: transition_selectAll,\n  f" +
    "ilter: transition_filter,\n  merge: transition_merge,\n  selection: transition_sel" +
    "ection,\n  transition: transition_transition,\n  call: selection_prototype.call,\n " +
    " nodes: selection_prototype.nodes,\n  node: selection_prototype.node,\n  size: sel" +
    "ection_prototype.size,\n  empty: selection_prototype.empty,\n  each: selection_pro" +
    "totype.each,\n  on: transition_on,\n  attr: transition_attr,\n  attrTween: transiti" +
    "on_attrTween,\n  style: transition_style,\n  styleTween: transition_styleTween,\n  " +
    "text: transition_text,\n  remove: transition_remove,\n  tween: transition_tween,\n " +
    " delay: transition_delay,\n  duration: transition_duration,\n  ease: transition_ea" +
    "se\n};\n\nvar defaultTiming = {\n  time: null, // Set on use.\n  delay: 0,\n  duration" +
    ": 250,\n  ease: cubicInOut\n};\n\nfunction inherit(node, id) {\n  var timing;\n  while" +
    " (!(timing = node.__transition) || !(timing = timing[id])) {\n    if (!(node = no" +
    "de.parentNode)) {\n      return defaultTiming.time = now(), defaultTiming;\n    }\n" +
    "  }\n  return timing;\n}\n\nvar selection_transition = function(name) {\n  var id,\n  " +
    "    timing;\n\n  if (name instanceof Transition) {\n    id = name._id, name = name." +
    "_name;\n  } else {\n    id = newId(), (timing = defaultTiming).time = now(), name " +
    "= name == null ? null : name + \"\";\n  }\n\n  for (var groups = this._groups, m = gr" +
    "oups.length, j = 0; j < m; ++j) {\n    for (var group = groups[j], n = group.leng" +
    "th, node, i = 0; i < n; ++i) {\n      if (node = group[i]) {\n        schedule(nod" +
    "e, name, id, i, group, timing || inherit(node, id));\n      }\n    }\n  }\n\n  return" +
    " new Transition(groups, this._parents, name, id);\n};\n\nselection.prototype.interr" +
    "upt = selection_interrupt;\nselection.prototype.transition = selection_transition" +
    ";\n\nexports.select = select;\nexports.selection = selection;\nexports.hierarchy = h" +
    "ierarchy;\nexports.partition = partition;\nexports.scaleLinear = linear;\nexports.e" +
    "aseCubic = cubicInOut;\nexports.ascending = ascending$1;\nexports.map = map$1;\nexp" +
    "orts.transition = transition;\n\nObject.defineProperty(exports, \'__esModule\', { va" +
    "lue: true });\n\n})));\n";


} // end d3_package
