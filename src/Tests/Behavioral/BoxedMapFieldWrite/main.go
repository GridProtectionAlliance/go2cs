package main

import "fmt"

type Header map[string][]string

type Request struct {
	Header Header
	Count  map[string]int
	Body   []string
}

func main() {
	// A pointer LOCAL (not a deref-aliased parameter) renders as its box; an
	// indexer-assignment into a NAMED-map-wrapper field must take the `.Value`
	// write path — the rvalue read form fails CS0131 (net/http client redirect
	// loop / httptest recorder shape).
	req := &Request{Header: Header{}, Count: map[string]int{}, Body: []string{"a", "b"}}

	req.Header["Accept"] = []string{"text/html", "text/plain"}
	req.Header["X-Tag"] = []string{"a"}

	// A plain-map field write through the box takes the same path.
	req.Count["hits"] = req.Count["hits"] + 1

	// Compound assignment and inc/dec through the box are writes too.
	req.Count["hits"] += 2
	req.Count["hits"]++

	// A SLICE field element write through the box.
	req.Body[1] = "z"

	fmt.Println(req.Header["Accept"])
	fmt.Println(req.Header["X-Tag"])
	fmt.Println(req.Count["hits"])
	fmt.Println(req.Body[0], req.Body[1])
}
