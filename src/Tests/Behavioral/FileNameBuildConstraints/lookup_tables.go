package main

// This file's name ends in "_tables", which is NOT a GOOS or GOARCH, so Go includes
// it in every build (exactly like the stdlib's math/bits bits_tables.go). The
// converter once treated any unknown "_word" filename suffix as a failing platform
// build constraint and silently dropped the file, losing its declarations. The fix
// restricts filename build constraints to a trailing recognized _GOOS / _GOARCH /
// _GOOS_GOARCH suffix. If this file were dropped, the symbols below would be missing
// and the program would fail to compile — so this project is a regression guard.
var lookupTable = [...]int{1, 4, 9, 16, 25}

const tableName = "squares"
