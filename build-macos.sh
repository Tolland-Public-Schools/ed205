#/bin/bash
dotnet publish -c release -r osx-x64 --self-contained true
dotnet publish -c release -r osx-arm64 --self-contained true