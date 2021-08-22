# Habitat

![](https://img.shields.io/docker/v/ardourtech/habitat?sort=date) ![](https://img.shields.io/docker/image-size/ardourtech/habitat?sort=date)

> *STATUS: In Development. Not yet ready for Production*

A Dockerized (Ubuntu based) Developer Environment with assistive CLI Tooling

---

## Reasoning

As a developer, I'm frequently switching between Windows, Linux, and MacOS. In each environment, I may need to perform the same tasks, leverage the same automation, or just want a consistent experience. But it can become a PIMA when the environments are not configured to be identical, or when a tool doesn't operate in the same way (looking at you CLI flags).

[Dotfile](https://dotfiles.github.io/) repositories really only solve half the problem. They fall short when you need to start applying OS specific tools and configuration. Docker *might* be a step in the right direction.

> Docker takes away repetitive, mundane configuration tasks and is used throughout the development lifecycle for fast, easy and portable application development - desktop and cloud ~ <https://www.docker.com/>

---

## CLI

CLI tool for Dockerized Developer Environments. Base docker image can be found at [habitat](https://github.com/ardourtech/habitat)

### Building for Release

```shell
dotnet publish -c release -r win-x64 --output ./target --self-contained=true /p:PublishSingleFile=true
```

### Core Dependencies

* [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet)
* [Docker.DotNet](https://github.com/dotnet/Docker.DotNet)

---

## Docker Base

### Usage

Habitat is intended to be a starting point. As such, we can take the foundation and customize it to suit our needs. We can either create a single all enclusive environment for developing multiple projects and languages, or build a project/language specific container.

```dockerfile
FROM ardourtech/habitat:<tag>
LABEL maintainer="Alexander Scott <xander@axrs.io>"
ARG HABITAT_USER
ARG HABITAT_USER_PASSWORD

# Drop down to the default ubuntu `root` user and change the `docker` user to be something more personal
USER root
RUN groupmod --new-name $HABITAT_USER docker
RUN usermod \
    --home /home/$HABITAT_USER \
    --move-home \
    --gid $HABITAT_USER \
    --login $HABITAT_USER \
    docker
RUN echo "${HABITAT_USER}:${HABITAT_USER_PASSWORD}" | chpasswd

USER $HABITAT_USER
WORKDIR /home/$HABITAT_USER

# Import our own dotfiles, or perform other configuration changes as needed
ADD --chown=$HABITAT_USER:$HABITAT_USER dotfiles /home/$HABITAT_USER/dotfiles
RUN rsync -avz $HOME/dotfiles/ $HOME/
RUN rm -rf dotfiles

ENTRYPOINT ["fish"]
```

Then we can build and run it with Docker.

```bash
 docker build \
   --build-arg HABITAT_USER=<your-user-name> \
   --build-arg HABITAT_USER_PASSWORD=<your-password> \
   --tag my-habitat
   .

 docker run \
    --rm \
    -it \
    --env DISPLAY=host.docker.internal:0 \
    --net=host \
    my-habitat
```

### What's Installed?

Out of the box, Habitat is built from `ubuntu:21.04` and includes:

* [X11 Window Support](https://en.wikipedia.org/wiki/X_Window_System)
  * (use `--env DISPLAY=host.docker.internal:0` or similar while running)
* [Fish](https://fishshell.com/) + [omf](https://github.com/Pyppe/oh-my-fish) as the default shell. Including the plugins/theme:
  * [bobthefish](https://github.com/oh-my-fish/theme-bobthefish)
  * [keychain](https://github.com/fishgretel/pkg-keychain)
  * [fish-nvm](https://github.com/fabioantunes/fish-nvm)
  * [bass](https://github.com/edc/bass)
* [Node.js](https://nodejs.org)
  * installed via [NVM](https://github.com/nvm-sh/nvm)
* [Java](https://www.java.com) version `adopt@1.15.0-2`
  * installed using [jabba](https://github.com/shyiko/jabba)
* [Clojure](https://clojure.org/) version `1.10.3.855`
* [Leiningen](https://Leiningen.org/)
* Various additional build tools:
  * bash
  * curl
  * direnv
  * git
  * keychain
  * neovim
  * rsync
  * unzip
  * wget
  * zip

### build (version)

> Builds the Habitat Container

```bash
docker build --tag ardourtech/habitat:$version .
```
