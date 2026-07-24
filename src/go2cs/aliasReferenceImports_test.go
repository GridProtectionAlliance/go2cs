// aliasReferenceImports_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// Blocker D: a test project's references are the DIRECT-import closure plus whatever
// aliasReferenceImports can recover by scanning the emitted `using` aliases, because
// DisableTransitiveProjectReferences hides everything else from the compile view. The scan matched
// only the ROOTED namespace token (`go.hash_package`), but a SINGLE-SEGMENT package emits its alias
// UNROOTED (`using hash = hash_package;` inside `namespace go.math.rand`, where C#'s outward lookup
// finds the class without a qualifier). math/rand/v2's chacha8_test.cs needs `hash` purely because
// `sha256.New()` RETURNS hash.Hash — the package appears in no import list — so the reference went
// missing and the build failed CS0246 on `hash_package`.
func TestAliasReferenceImportsMatchesUnrootedSingleSegmentAlias(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"hash":          {Name: "hash"},
		"hash/maphash":  {Name: "maphash"},
		"crypto/sha256": {Name: "sha256"},
		"io":            {Name: "io"},
	}

	infoFile := filepath.Join(t.TempDir(), "chacha8_test.cs")

	contents := "namespace go.math.rand;\r\n" +
		"using sha256 = crypto.sha256_package;\r\n" +
		"using hash = hash_package;\r\n" +
		"using static go.math.rand.rand_package;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "math/rand/v2", []string{"crypto/sha256"})

	if len(found) != 1 || found[0] != "hash" {
		t.Fatalf("aliasReferenceImports = %v, want [hash]", found)
	}
}

// A MULTI-SEGMENT package emits its alias UNROOTED when the enclosing namespace SHADOWS the root
// `go` (go/doc/comment's std_test.cs in `namespace go.go.doc`, internal/abi's abi_test.cs in
// `namespace go.@internal`): `using exec = os.exec_package;` relies on C# outward lookup to find
// `go.os.exec_package`. The scan built only ROOTED tokens (`go.os.exec_package`), and neither the
// Contains/HasSuffix(target, token) checks (target is the shorter unrooted tail) nor the
// single-segment bareTokens path covered it, so os/exec — reached purely because testenv.Command
// RETURNS *exec.Cmd — went unreferenced and the build failed CS0246 on `os`.
func TestAliasReferenceImportsMatchesUnrootedMultiSegmentAlias(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"os":               {Name: "os"},
		"os/exec":          {Name: "exec"},
		"internal/testenv": {Name: "testenv"},
	}

	infoFile := filepath.Join(t.TempDir(), "std_test.cs")

	contents := "namespace go.go.doc;\r\n" +
		"using testenv = @internal.testenv_package;\r\n" +
		"using exec = os.exec_package;\r\n" +
		"using os;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "go/doc/comment", []string{"internal/testenv"})

	if len(found) != 1 || found[0] != "os/exec" {
		t.Fatalf("aliasReferenceImports = %v, want [os/exec]", found)
	}
}

// The unrooted-tail match is anchored on the leading ".": `os.exec_package` must not match an
// unrelated token like `go.notos.exec_package`, so a package nothing references is never pulled in.
func TestAliasReferenceImportsUnrootedTailAnchoredOnSegmentBoundary(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"notos/exec": {Name: "exec"},
	}

	infoFile := filepath.Join(t.TempDir(), "only_exec.cs")

	if err := os.WriteFile(infoFile, []byte("using exec = os.exec_package;\r\n"), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	if found := aliasReferenceImports([]string{infoFile}, "go/doc/comment", nil); len(found) != 0 {
		t.Fatalf("aliasReferenceImports = %v, want no matches (os.exec must not imply notos/exec)", found)
	}
}

// The bare token is matched on a SEGMENT boundary: a substring test would let `hash_package` match
// `go.hash.maphash_package` and pull a reference to a package nothing actually uses.
func TestAliasReferenceImportsDoesNotMatchAcrossSegmentBoundaries(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"hash": {Name: "hash"},
	}

	infoFile := filepath.Join(t.TempDir(), "only_maphash.cs")

	if err := os.WriteFile(infoFile, []byte("using maphash = go.hash.maphash_package;\r\n"), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	if found := aliasReferenceImports([]string{infoFile}, "math/rand/v2", nil); len(found) != 0 {
		t.Fatalf("aliasReferenceImports = %v, want no matches (maphash must not imply hash)", found)
	}
}
