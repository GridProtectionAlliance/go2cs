// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !math_big_pure_go
namespace go.math;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname

partial class big_package {

// implemented in arith_$GOARCH.s

// addVV should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname addVV
//go:noescape
internal static partial Word /*c*/ addVV(slice<Word> z, slice<Word> x, slice<Word> y);

// subVV should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname subVV
//go:noescape
internal static partial Word /*c*/ subVV(slice<Word> z, slice<Word> x, slice<Word> y);

// addVW should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname addVW
//go:noescape
internal static partial Word /*c*/ addVW(slice<Word> z, slice<Word> x, Word y);

// subVW should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname subVW
//go:noescape
internal static partial Word /*c*/ subVW(slice<Word> z, slice<Word> x, Word y);

// shlVU should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname shlVU
//go:noescape
internal static partial Word /*c*/ shlVU(slice<Word> z, slice<Word> x, nuint s);

//go:noescape
internal static partial Word /*c*/ shrVU(slice<Word> z, slice<Word> x, nuint s);

// mulAddVWW should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname mulAddVWW
//go:noescape
internal static partial Word /*c*/ mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r);

// addMulVVW should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/remyoudompheng/bigfft
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname addMulVVW
//go:noescape
internal static partial Word /*c*/ addMulVVW(slice<Word> z, slice<Word> x, Word y);

} // end big_package
