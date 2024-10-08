// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:38:50 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\attr.go
namespace go.html;

using strings = strings_package;


// attrTypeMap[n] describes the value of the given attribute.
// If an attribute affects (or can mask) the encoding or interpretation of
// other content, or affects the contents, idempotency, or credentials of a
// network message, then the value in this map is contentTypeUnsafe.
// This map is derived from HTML5, specifically
// https://www.w3.org/TR/html5/Overview.html#attributes-1
// as well as "%URI"-typed attributes from
// https://www.w3.org/TR/html4/index/attributes.html

public static partial class template_package {

private static map attrTypeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, contentType>{"accept":contentTypePlain,"accept-charset":contentTypeUnsafe,"action":contentTypeURL,"alt":contentTypePlain,"archive":contentTypeURL,"async":contentTypeUnsafe,"autocomplete":contentTypePlain,"autofocus":contentTypePlain,"autoplay":contentTypePlain,"background":contentTypeURL,"border":contentTypePlain,"checked":contentTypePlain,"cite":contentTypeURL,"challenge":contentTypeUnsafe,"charset":contentTypeUnsafe,"class":contentTypePlain,"classid":contentTypeURL,"codebase":contentTypeURL,"cols":contentTypePlain,"colspan":contentTypePlain,"content":contentTypeUnsafe,"contenteditable":contentTypePlain,"contextmenu":contentTypePlain,"controls":contentTypePlain,"coords":contentTypePlain,"crossorigin":contentTypeUnsafe,"data":contentTypeURL,"datetime":contentTypePlain,"default":contentTypePlain,"defer":contentTypeUnsafe,"dir":contentTypePlain,"dirname":contentTypePlain,"disabled":contentTypePlain,"draggable":contentTypePlain,"dropzone":contentTypePlain,"enctype":contentTypeUnsafe,"for":contentTypePlain,"form":contentTypeUnsafe,"formaction":contentTypeURL,"formenctype":contentTypeUnsafe,"formmethod":contentTypeUnsafe,"formnovalidate":contentTypeUnsafe,"formtarget":contentTypePlain,"headers":contentTypePlain,"height":contentTypePlain,"hidden":contentTypePlain,"high":contentTypePlain,"href":contentTypeURL,"hreflang":contentTypePlain,"http-equiv":contentTypeUnsafe,"icon":contentTypeURL,"id":contentTypePlain,"ismap":contentTypePlain,"keytype":contentTypeUnsafe,"kind":contentTypePlain,"label":contentTypePlain,"lang":contentTypePlain,"language":contentTypeUnsafe,"list":contentTypePlain,"longdesc":contentTypeURL,"loop":contentTypePlain,"low":contentTypePlain,"manifest":contentTypeURL,"max":contentTypePlain,"maxlength":contentTypePlain,"media":contentTypePlain,"mediagroup":contentTypePlain,"method":contentTypeUnsafe,"min":contentTypePlain,"multiple":contentTypePlain,"name":contentTypePlain,"novalidate":contentTypeUnsafe,"open":contentTypePlain,"optimum":contentTypePlain,"pattern":contentTypeUnsafe,"placeholder":contentTypePlain,"poster":contentTypeURL,"profile":contentTypeURL,"preload":contentTypePlain,"pubdate":contentTypePlain,"radiogroup":contentTypePlain,"readonly":contentTypePlain,"rel":contentTypeUnsafe,"required":contentTypePlain,"reversed":contentTypePlain,"rows":contentTypePlain,"rowspan":contentTypePlain,"sandbox":contentTypeUnsafe,"spellcheck":contentTypePlain,"scope":contentTypePlain,"scoped":contentTypePlain,"seamless":contentTypePlain,"selected":contentTypePlain,"shape":contentTypePlain,"size":contentTypePlain,"sizes":contentTypePlain,"span":contentTypePlain,"src":contentTypeURL,"srcdoc":contentTypeHTML,"srclang":contentTypePlain,"srcset":contentTypeSrcset,"start":contentTypePlain,"step":contentTypePlain,"style":contentTypeCSS,"tabindex":contentTypePlain,"target":contentTypePlain,"title":contentTypePlain,"type":contentTypeUnsafe,"usemap":contentTypeURL,"value":contentTypeUnsafe,"width":contentTypePlain,"wrap":contentTypePlain,"xmlns":contentTypeURL,};

// attrType returns a conservative (upper-bound on authority) guess at the
// type of the lowercase named attribute.
private static contentType attrType(@string name) {
    if (strings.HasPrefix(name, "data-")) { 
        // Strip data- so that custom attribute heuristics below are
        // widely applied.
        // Treat data-action as URL below.
        name = name[(int)5..];
    }    {
        var colon = strings.IndexRune(name, ':');


        else if (colon != -1) {
            if (name[..(int)colon] == "xmlns") {
                return contentTypeURL;
            } 
            // Treat svg:href and xlink:href as href below.
            name = name[(int)colon + 1..];
        }
    }
    {
        var (t, ok) = attrTypeMap[name];

        if (ok) {
            return t;
        }
    } 
    // Treat partial event handler names as script.
    if (strings.HasPrefix(name, "on")) {
        return contentTypeJS;
    }
    if (strings.Contains(name, "src") || strings.Contains(name, "uri") || strings.Contains(name, "url")) {
        return contentTypeURL;
    }
    return contentTypePlain;
}

} // end template_package
