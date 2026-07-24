package main

import (
	"io"
	"net/http"
	"strings"
	"testing"
)

func TestModifyResponseDefaultsTourToDarkWithSyntax(t *testing.T) {
	body := `<!doctype html><html data-theme="auto"><head></head><body>Tour</body></html>`
	response := &http.Response{
		Header: http.Header{"Content-Type": []string{"text/html; charset=utf-8"}},
		Body:   io.NopCloser(strings.NewReader(body)),
	}

	proxy := newTourProxy("127.0.0.1:3999", t.TempDir())
	if err := proxy.modifyResponse(response); err != nil {
		t.Fatal(err)
	}
	updated, err := io.ReadAll(response.Body)
	if err != nil {
		t.Fatal(err)
	}
	text := string(updated)
	for _, want := range []string{
		`data-theme="dark"`,
		`localStorage.getItem("syntax") === null`,
		`localStorage.setItem("syntax", "true")`,
		`href="/bridge.css"`,
		`src="/bridge.js"`,
	} {
		if !strings.Contains(text, want) {
			t.Errorf("modified Tour response missing %q:\n%s", want, text)
		}
	}
}
