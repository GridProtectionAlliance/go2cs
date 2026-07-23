# Tour of go2cs

Tour of go2cs places the official, locally hosted **Tour of Go** beside a live
Go-to-C# workspace. The Go lesson remains owned and rendered by the upstream
Tour. A same-origin bridge reads the current editor text and sends that exact
text to the local go2cs server.

The interface is deliberately parallel:

- The original Tour occupies the left two-thirds of the window.
- Generated C# occupies the right third, with a draggable divider.
- **Code** and **Project** tabs show the matching `.cs` and `.csproj`.
- **Transpile**, **Build**, and **.NET Run** keep their output separate.
- Navigating to a Tour page converts it automatically.
- Editing Go marks the C# stale until **Convert** is selected.
- **Run** builds and executes the current conversion; it becomes **Kill** while
  the process is active.

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

## Keyboard controls

- `Ctrl`/`Cmd` + `Enter`: convert edited Go
- `Shift` + `Enter`: run the current .NET conversion
- Focus the divider and use `Left Arrow` / `Right Arrow`: resize the panes

## Development checks

```sh
cd src/tour
go test ./...
go vet ./...
```

The first conversion can take longer because go2cs is built lazily. Converted
workspaces are isolated in temporary directories and expire after 30 minutes.
The request body is limited to 256 KiB, and each normal tool stage has a
20-second timeout. Aborting the Run request cancels the active build or program.

## Integration design

The official Tour runs unchanged on a private loopback port. This server
reverse-proxies `/tour/`, `/images/`, and the websocket endpoint, injects only
the source bridge and a small thematic stylesheet, and hosts the outer
interface on port 4000.

The websocket proxy translates the browser-facing origin to the private Tour
origin so the upstream same-origin check remains effective. The server owns
only the go2cs/.NET half. A successful conversion is retained briefly so Run
can build and execute the exact project shown in the Project tab.
