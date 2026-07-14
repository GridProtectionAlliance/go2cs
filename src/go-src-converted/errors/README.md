# go.errors

> C# package converted from the Go standard library by [go2cs](https://github.com/GridProtectionAlliance/go2cs).
> Go version: 1.23.1

Package errors implements functions to manipulate errors.

The \[New] function creates errors whose only content is a text message.

An error e wraps another error if e's type has one of the methods

	Unwrap() error
	Unwrap() []error

If e.Unwrap() returns a non-nil error w or a slice containing w, then we say that e wraps w. A nil error returned from e.Unwrap() indicates that e does not wrap any error. It is invalid for an Unwrap method to return an \[]error containing a nil error value.

An easy way to create wrapped errors is to call [fmt.Errorf](/fmt#Errorf) and apply the %w verb to the error argument:

	wrapsErr := fmt.Errorf("... %w ...", ..., err, ...)

Successive unwrapping of an error creates a tree. The \[Is] and \[As] functions inspect an error's tree by examining first the error itself followed by the tree of each of its children in turn (pre-order, depth-first traversal).

\[Is] examines the tree of its first argument looking for an error that matches the second. It reports whether it finds a match. It should be used in preference to simple equality checks:

	if errors.Is(err, fs.ErrExist)

is preferable to

	if err == fs.ErrExist

because the former will succeed if err wraps [io/fs.ErrExist](/io/fs#ErrExist).

\[As] examines the tree of its first argument looking for an error that can be assigned to its second argument, which must be a pointer. If it succeeds, it performs the assignment and returns true. Otherwise, it returns false. The form

	var perr *fs.PathError
	if errors.As(err, &perr) {
		fmt.Println(perr.Path)
	}

is preferable to

	if perr, ok := err.(*fs.PathError); ok {
		fmt.Println(perr.Path)
	}

because the former will succeed if err wraps an [\*io/fs.PathError](/io/fs#PathError).

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/GridProtectionAlliance/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.
