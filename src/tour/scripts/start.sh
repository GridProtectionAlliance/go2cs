#!/usr/bin/env sh
set -eu

tour_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
repo_root=$(CDPATH= cd -- "$tour_root/../.." && pwd)

go version
dotnet --version

go_path=$(go env GOPATH)
tour_binary="$go_path/bin/tour"
if [ ! -x "$tour_binary" ]; then
  echo "Installing the official offline Tour of Go..."
  go install golang.org/x/website/tour@latest
fi

export GO_TOUR_BIN="$tour_binary"
cd "$tour_root"
exec go run . -repo="$repo_root" "$@"
