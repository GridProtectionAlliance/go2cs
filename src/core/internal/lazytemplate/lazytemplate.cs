// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazytemplate is a thin wrapper over text/template, allowing the use
// of global template variables without forcing them to be parsed at init.
namespace go.@internal;

using io = io_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;
using template = text.template_package;
using text;

partial class lazytemplate_package {

// Template is a wrapper around text/template.Template, where the underlying
// template will be parsed the first time it is needed.
[GoType] partial struct Template {
    internal @string name;
    internal @string text;
    internal sync_package.Once once;
    internal ж<text.template_package.Template> tmpl;
}

[GoRecv] internal static ж<template.Template> tp(this ref Template r) {
    r.once.Do(r.build);
    return r.tmpl;
}

[GoRecv] internal static void build(this ref Template r) {
    r.tmpl = template.Must(template.New(r.name).Parse(r.text));
    (r.name, r.text) = (""u8, ""u8);
}

[GoRecv] public static error Execute(this ref Template r, io.Writer w, any data) {
    return r.tp().Execute(w, data);
}

internal static bool inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"u8), ".test"u8);

// New creates a new lazy template, delaying the parsing work until it is first
// needed. If the code is being run as part of tests, the template parsing will
// happen immediately.
public static ж<Template> New(@string name, @string text) {
    var lt = Ꮡ(new Template(name: name, text: text));
    if (inTest) {
        // In tests, always parse the templates early.
        lt.tp();
    }
    return lt;
}

} // end lazytemplate_package
