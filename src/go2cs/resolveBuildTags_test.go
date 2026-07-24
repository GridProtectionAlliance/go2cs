// resolveBuildTags_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"reflect"
	"testing"
)

// TestResolveBuildTags guards the purego default decision. The regression it locks in: a `-tests`
// run must apply the same purego default as `-stdlib`, so the reconverted PRODUCTION sources select
// the same files the committed go-src-converted tree was built from. Before the fix, `-tests` left
// build tags empty and converted asm variants (crypto/subtle's xor_amd64.go) alongside the pure-Go
// ones, producing a CS0111 duplicate-member collision and a production .cs that diverged from the
// committed purego emission.
func TestResolveBuildTags(t *testing.T) {
	explicit := []string{"foo", "bar"}

	tests := []struct {
		name          string
		convertStdLib bool
		convertTests  bool
		tagsExplicit  bool
		explicit      []string
		want          []string
	}{
		{"stdlib default -> purego", true, false, false, nil, defaultStdLibBuildTags},
		{"tests default -> purego", false, true, false, nil, defaultStdLibBuildTags},
		{"stdlib+tests default -> purego", true, true, false, nil, defaultStdLibBuildTags},
		{"tests with explicit -tags -> explicit honored", false, true, true, explicit, explicit},
		{"stdlib with explicit -tags -> explicit honored", true, false, true, explicit, explicit},
		{"tests with -tags= (explicit clear) -> empty", false, true, true, nil, nil},
		{"neither (single-file/recurse) -> tag-neutral", false, false, false, explicit, explicit},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			got := resolveBuildTags(tt.convertStdLib, tt.convertTests, tt.tagsExplicit, tt.explicit)
			if !reflect.DeepEqual(got, tt.want) {
				t.Fatalf("resolveBuildTags(%v, %v, %v, %v) = %v, want %v",
					tt.convertStdLib, tt.convertTests, tt.tagsExplicit, tt.explicit, got, tt.want)
			}
		})
	}
}
