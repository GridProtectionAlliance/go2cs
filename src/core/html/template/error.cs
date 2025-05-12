// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using parse = text.template.parse_package;
using text.template;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// Error describes a problem encountered during template Escaping.
[GoType] partial struct ΔError {
    // ErrorCode describes the kind of error.
    public ErrorCode ErrorCode;
    // Node is the node that caused the problem, if known.
    // If not nil, it overrides Name and Line.
    public text.template.parse_package.Node Node;
    // Name is the name of the template in which the error was encountered.
    public @string Name;
    // Line is the line number of the error in the template source or 0.
    public nint Line;
    // Description is a human-readable description of the problem.
    public @string Description;
}

[GoType("num:nint")] partial struct ErrorCode;

// We define codes for each error that manifests while escaping templates, but
// escaped templates may also fail at runtime.
//
// Output: "ZgotmplZ"
// Example:
//
//	<img src="{{.X}}">
//	where {{.X}} evaluates to `javascript:...`
//
// Discussion:
//
//	"ZgotmplZ" is a special value that indicates that unsafe content reached a
//	CSS or URL context at runtime. The output of the example will be
//	  <img src="#ZgotmplZ">
//	If the data comes from a trusted source, use content types to exempt it
//	from filtering: URL(`javascript:...`).
public static readonly ErrorCode OK = /* iota */ 0;

public static readonly ErrorCode ErrAmbigContext = 1;

public static readonly ErrorCode ErrBadHTML = 2;

public static readonly ErrorCode ErrBranchEnd = 3;

public static readonly ErrorCode ErrEndContext = 4;

public static readonly ErrorCode ErrNoSuchTemplate = 5;

public static readonly ErrorCode ErrOutputContext = 6;

public static readonly ErrorCode ErrPartialCharset = 7;

public static readonly ErrorCode ErrPartialEscape = 8;

public static readonly ErrorCode ErrRangeLoopReentry = 9;

public static readonly ErrorCode ErrSlashAmbig = 10;

public static readonly ErrorCode ErrPredefinedEscaper = 11;

public static readonly ErrorCode ErrJSTemplate = 12;

[GoRecv] public static @string Error(this ref ΔError e) {
    switch (ᐧ) {
    case {} when e.Node != default!: {
        var (loc, _) = ((ж<parse.Tree>)(default!)).val.ErrorContext(e.Node);
        return fmt.Sprintf("html/template:%s: %s"u8, loc, e.Description);
    }
    case {} when e.Line is != 0: {
        return fmt.Sprintf("html/template:%s:%d: %s"u8, e.Name, e.Line, e.Description);
    }
    case {} when e.Name != ""u8: {
        return fmt.Sprintf("html/template:%s: %s"u8, e.Name, e.Description);
    }}

    return "html/template: "u8 + e.Description;
}

// errorf creates an error given a format string f and args.
// The template Name still needs to be supplied.
internal static ж<ΔError> errorf(ErrorCode k, parse.Node node, nint line, @string f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return Ꮡ(new ΔError(k, node, "", line, fmt.Sprintf(f, args.ꓸꓸꓸ)));
}

} // end template_package
