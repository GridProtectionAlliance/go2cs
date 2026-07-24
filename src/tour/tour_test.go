package main

import (
	"io"
	"net/http"
	"net/url"
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

func TestModifyResponseAddsGoVersionToTourScript(t *testing.T) {
	body := "code += '-- go.mod --\\n' +\n                    'module example\\n' +\n                    'require golang.org/x/tour v0.1.0\\n' +"
	response := &http.Response{
		Header:  http.Header{"Content-Type": []string{"application/javascript"}},
		Body:    io.NopCloser(strings.NewReader(body)),
		Request: &http.Request{URL: &url.URL{Path: "/tour/script.js"}},
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
	if !strings.Contains(text, "'module example\\n' +\n                    'go 1.23.1\\n' +") {
		t.Fatalf("modified Tour script missing Go language version:\n%s", text)
	}
	if got := response.Header.Get("Cache-Control"); got != "no-store" {
		t.Errorf("Cache-Control = %q, want no-store", got)
	}
}
