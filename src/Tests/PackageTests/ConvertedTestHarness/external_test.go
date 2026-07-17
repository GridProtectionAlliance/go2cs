package convertedtestharness_test

import (
	"testing"

	harness "go2cs/convertedtestharness"
	. "go2cs/convertedtestharness"
)

func TestExternalPackage(t *testing.T) {
	if got := harness.Double(4); got != 8 {
		t.Fatalf("Double(4) = %d, want 8", got)
	}
}

func TestProductionDependency(t *testing.T) {
	if got := harness.Label(4); got != "value=4" {
		t.Fatalf("Label(4) = %q, want value=4", got)
	}
}

// TestDotImportedProduction exercises the DOT self-import form (`. "pkg"`) — the shape the
// unicode/utf8 first-proof suite uses — which must bind to the production partial class compiled
// into this same test assembly.
func TestDotImportedProduction(t *testing.T) {
	if got := Double(21); got != 42 {
		t.Fatalf("Double(21) = %d, want 42", got)
	}
}
