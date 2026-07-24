# Tour of go2cs

Tour of go2cs places the official, locally hosted **Tour of Go** beside a live
Go-to-C# workspace. The Go lesson remains owned and rendered by the upstream
Tour. A same-origin bridge reads the current editor text and sends that exact
text to the local go2cs server.

The interface is deliberately parallel:

- The original Tour occupies the left two-thirds of the window.
- Generated C# occupies the right third, with a draggable divider.
- **Code** and **Project** tabs show the matching `.cs` and `.csproj`.
- The **Runtime** selector chooses live source, a deployed stdlib, or NuGet
  packages without changing the Go lesson.
- **Transpile**, **Build**, and **.NET Run** keep their output separate.
- Navigating to a Tour page converts it automatically.
- Editing Go marks the C# stale until **Convert** is selected.
- **Run with Go** optionally converts, builds, and runs .NET whenever the Go
  program runs.
- **Run** builds and executes the current conversion; it becomes **Kill** while
  the process is active.

## Requirements

- Go 1.23.1 or later
- .NET SDK 9.0 or later
- A local clone of this `go2cs` repository
- Network access on the first start to install the official offline Tour, and when an exercise first
  imports a Go module that is not already in the local module cache

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

To start with a different .NET runtime source:

```powershell
.\src\tour\scripts\start.ps1 -Runtime deployed
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
- `-runtime=core|deployed|nuget`: initial .NET runtime source
- `-deployed-root=/path/to/go2cs`: root created by `deploy-core.ps1 stdlib`
- `-nuget-source=/path/or/feed`: folder or feed containing go2cs packages
- `-nuget-version=1.23.1.1`: package version to restore
- `-no-tour`: do not launch the upstream Tour process
- `-no-open`: do not open a browser

`GO_TOUR_BIN` can point to the upstream `tour` executable. `GO2CS_BIN` can point
to an explicitly managed prebuilt go2cs executable; otherwise each server process builds the
current checkout once into `src/tour/.cache` and reuses it for that process. Rebuilding on restart
prevents a converter cached by an older checkout from being used with a newer Tour pipeline.

## .NET runtime sources

**Core source** is the default (`-runtime=core`). It converts and builds
against the current checkout's `src/core`, `src/gen`, and converted package projects, which is the
best mode while developing go2cs itself.

**Deployed stdlib** (`-runtime=deployed`) uses the compiled/full
standard-library tree produced by:

```powershell
.\src\deploy-core.ps1 stdlib
```

The server discovers this at `$GOPATH/src/go2cs`. Override it with
`-deployed-root` or `GO2CS_DEPLOYED_ROOT`.

**NuGet packages** (`-runtime=nuget`) rewrites the generated project
references to `go.gen`,
`go.lib`, and the required `go.*` packages. The server prefers the local
`src/artifacts/nupkg` feed when packages exist, then falls back to nuget.org.
The version comes from `src/version.props`. Override either value with
`-nuget-source` / `GO2CS_NUGET_SOURCE` and
`-nuget-version` / `GO2CS_NUGET_VERSION`.

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

The first conversion can take longer because go2cs is built lazily. Each submission gets a temporary
Go module; the server runs `go mod tidy`, recursively converts imported third-party packages, and keeps
the generated app/dependency graph isolated from the selected runtime tree. The Code and Project tabs
still show only the submitted app. Converted workspaces expire after 30 minutes. The request body is
limited to 256 KiB; normal tool stages have a 20-second timeout and dependency resolution has two minutes
for an initial download. Aborting the Run request cancels the active build or program.

## Integration design

The official Tour runs unchanged on a private loopback port. This server
reverse-proxies `/tour/`, `/images/`, and the websocket endpoint, injects only
the source bridge and a small thematic stylesheet, and hosts the outer
interface on port 4000.

The websocket proxy translates the browser-facing origin to the private Tour
origin so the upstream same-origin check remains effective. The server owns
only the go2cs/.NET half. A successful conversion is retained briefly so Run
can build and execute the exact project shown in the Project tab.
