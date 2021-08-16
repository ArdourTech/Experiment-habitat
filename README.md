# habitat-cli
CLI tool for Dockerized Developer Environments

## Building for Release

```
dotnet publish -c release -r win-x64 --output ./target --self-contained=true /p:PublishSingleFile=true
```
