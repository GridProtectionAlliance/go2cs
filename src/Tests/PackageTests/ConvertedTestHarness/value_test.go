package convertedtestharness

import (
	"os"
	"testing"
)

func TestSamePackageUnexported(t *testing.T) {
	if got := add(2, 3); got != 5 {
		t.Fatalf("add(2, 3) = %d, want 5", got)
	}
}

func TestSubtestsCleanupAndParallel(t *testing.T) {
	cleaned := false
	t.Cleanup(func() {
		cleaned = true
	})

	for _, name := range []string{"duplicate", "duplicate"} {
		name := name
		t.Run(name, func(sub *testing.T) {
			sub.Parallel()
			if sub.Name() == "" {
				sub.Error("subtest name was empty")
			}
		})
	}

	if cleaned {
		t.Error("cleanup ran before the parent test completed")
	}
}

func TestTempDir(t *testing.T) {
	if t.TempDir() == "" {
		t.Error("TempDir returned an empty path")
	}
}

// TestFixtureRead proves the testdata fixture is readable from the isolated working directory on
// the GO SIDE TOO (requirements §10 — fixture reads were previously only proven by the MSTest
// runtime guards).
func TestFixtureRead(t *testing.T) {
	data, err := os.ReadFile("testdata/message.txt")
	if err != nil {
		t.Fatalf("read testdata fixture: %v", err)
	}

	if len(data) < 7 || string(data[:7]) != "fixture" {
		t.Fatalf("unexpected fixture content: %q", string(data))
	}
}

func TestMain(m *testing.M) {
	m.Run()
}

// NOTE: the branch fixture carried an intentionally malformed `Testlower` declaration to prove
// non-registration, but Go 1.23's `go test` runs the vet `tests` analyzer by default and REFUSES
// to build the package ("Testlower has malformed name") — the Go side of the oracle could never
// run. Invalid-name rejection stays guarded at the converter level (TestIsGoTestName).
