# Tour of go2cs

Tour of go2cs places the official, locally hosted **Tour of Go** beside a live
Go-to-C# pipeline. The Go lesson remains owned and rendered by the upstream
Tour. A small same-origin bridge reads the current editor text, and the local
server uses that exact text for every stage:

1. `go run`
2. `go2cs`
3. `dotnet build`
4. `dotnet run`

Each stage gets its own output tab. A transpile, compiler, timeout, or runtime
failure stops only the dependent stages and remains visible in the UI.

## Requirements

- Go 1.23.1 or later
- .NET SDK 9.0 or later
- A local clone of this `go2cs` repository
- Network access on the first start, only to install the official offline Tour

The app binds to loopback by default. It executes editor content as local code,
so it should not be exposed to an untrusted network.

## Start on Windows

From the repository root:

```powershell
.\src\tour\scripts\start.ps1
```

The bootstrap script verifies Go and .NET, installs
`golang.org/x/website/tour@latest` when needed, and opens
<http://127.0.0.1:4000>.

To leave the browser closed or use another loopback port:

```powershell
.\src\tour\scripts\start.ps1 -NoOpen -ListenAddress 127.0.0.1:4100
```

## Start on macOS or Linux

```sh
./src/tour/scripts/start.sh
```

## Direct server options

After installing the upstream Tour yourself:

```sh
cd src/tour
go run . -no-open
```

Useful options:

- `-addr=127.0.0.1:4000`: address for Tour of go2cs
- `-tour-addr=127.0.0.1:3999`: private address for the upstream Tour
- `-repo=/path/to/go2cs`: explicit repository root
- `-no-tour`: do not launch the upstream Tour process
- `-no-open`: do not open a browser

`GO_TOUR_BIN` can point to the upstream `tour` executable. `GO2CS_BIN` can point
to a prebuilt go2cs executable; otherwise the server builds and caches one in
`src/tour/.cache`.

## Development checks

```sh
cd src/tour
go test ./...
go vet ./...
```

The first real conversion can take longer because go2cs is built lazily. Each
user program runs in a fresh temporary directory, the request body is limited
to 256 KiB, and each normal pipeline stage has a 20-second timeout.

## Integration design

The official Tour runs unchanged on a private loopback port. This server
reverse-proxies `/tour/` and its socket endpoint, injects only `bridge.js` and a
small thematic stylesheet into the HTML response, and hosts the outer interface
on port 4000. Because the iframe and outer page share an origin, the bridge can
publish CodeMirror changes without weakening browser security or maintaining a
fork of the Tour content.

The server intentionally owns the go2cs/.NET half. Generated files are read
before the temporary workspace is removed, and paths in tool output are
normalized to avoid leaking machine-specific repository locations into the UI.
