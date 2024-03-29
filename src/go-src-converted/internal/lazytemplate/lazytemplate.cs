// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lazytemplate is a thin wrapper over text/template, allowing the use
// of global template variables without forcing them to be parsed at init.

// package lazytemplate -- go2cs converted at 2022 March 13 06:30:34 UTC
// import "internal/lazytemplate" ==> using lazytemplate = go.@internal.lazytemplate_package
// Original source: C:\Program Files\Go\src\internal\lazytemplate\lazytemplate.go
namespace go.@internal;

using io = io_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;
using template = text.template_package;


// Template is a wrapper around text/template.Template, where the underlying
// template will be parsed the first time it is needed.

public static partial class lazytemplate_package {

public partial struct Template {
    public @string name;
    public @string text;
    public sync.Once once;
    public ptr<template.Template> tmpl;
}

private static ptr<template.Template> tp(this ptr<Template> _addr_r) {
    ref Template r = ref _addr_r.val;

    r.once.Do(r.build);
    return _addr_r.tmpl!;
}

private static void build(this ptr<Template> _addr_r) {
    ref Template r = ref _addr_r.val;

    r.tmpl = template.Must(template.New(r.name).Parse(r.text));
    (r.name, r.text) = ("", "");
}

private static error Execute(this ptr<Template> _addr_r, io.Writer w, object data) {
    ref Template r = ref _addr_r.val;

    return error.As(r.tp().Execute(w, data))!;
}

private static var inTest = len(os.Args) > 0 && strings.HasSuffix(strings.TrimSuffix(os.Args[0], ".exe"), ".test");

// New creates a new lazy template, delaying the parsing work until it is first
// needed. If the code is being run as part of tests, the template parsing will
// happen immediately.
public static ptr<Template> New(@string name, @string text) {
    ptr<Template> lt = addr(new Template(name:name,text:text));
    if (inTest) { 
        // In tests, always parse the templates early.
        lt.tp();
    }
    return _addr_lt!;
}

} // end lazytemplate_package
