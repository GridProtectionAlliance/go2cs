# go.image

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package image implements a basic 2-D image library.

The fundamental interface is called \[Image]. An \[Image] contains colors, which are described in the image/color package.

Values of the \[Image] interface are created either by calling functions such as \[NewRGBA] and \[NewPaletted], or by calling \[Decode] on an [io.Reader](/io#Reader) containing image data in a format such as GIF, JPEG or PNG. Decoding any particular image format requires the prior registration of a decoder function. Registration is typically automatic as a side effect of initializing that format's package so that, to decode a PNG image, it suffices to have

	import _ "image/png"

in a program's main package. The \_ means to import a package purely for its initialization side effects.

See "The Go image package" for more details: [https://golang.org/doc/articles/image\_package.html](https://golang.org/doc/articles/image_package.html)

### Security Considerations

The image package can be used to parse arbitrarily large images, which can cause resource exhaustion on machines which do not have enough memory to store them. When operating on arbitrary images, \[DecodeConfig] should be called before \[Decode], so that the program can decide whether the image, as defined in the returned header, can be safely decoded with the available resources. A call to \[Decode] which produces an extremely large image, as defined in the header returned by \[DecodeConfig], is not considered a security issue, regardless of whether the image is itself malformed or not. A call to \[DecodeConfig] which returns a header which does not match the image returned by \[Decode] may be considered a security issue, and should be reported per the \[Go Security Policy]([https://go.dev/security/policy](https://go.dev/security/policy)).

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
