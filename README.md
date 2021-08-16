# habitat-cli

CLI tool for Dockerized Developer Environments

## Building for Release

```shell
dotnet publish -c release -r win-x64 --output ./target --self-contained=true /p:PublishSingleFile=true
```

## Core Dependencies

* [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet)
* [Docker.DotNet](https://github.com/dotnet/Docker.DotNet)

## Commands

### Build

```text
Builds the Habitat Environment

Usage: habitat.exe build

Options:

  -u | --user       <TEXT>      [<current-user>]
  Habitat User

  -p | --password   <TEXT>
  Habitat User Password

  -d | --directory  <PATH>      [<cwd>]
  Working Directory

  -f | --file       <FILENAME>  [<cwd>\Dockerfile]
  DockerFile

  --no-cache
  No Cache

  -t | --tag        <TEXT>
  Image Tag
```

### Connect

```text
Connects to a running Habitat Environment by name

Usage: habitat.exe connect

Options:

  -n | --name     <TEXT>  [habitat]
  Name for the Container

  -c | --command  <TEXT>  [fish]
  Command to exec
```

### Start

```text
Starts a Habitat Environment

Usage: habitat.exe start

Options:

  -i | --image  <TEXT>
  Image to Run

  -n | --name   <TEXT>  [habitat]
  Name for the Container
```

### Stop

```text
Stops a named Habitat Environment

Usage: habitat.exe stop

Options:

  -n | --name  <TEXT>  [habitat]
  Name of the Container to Stop
```
