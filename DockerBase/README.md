# Habitat Docker Base

> *STATUS: In Development. Not yet ready for Production*

![docker-version](https://img.shields.io/docker/v/ardourtech/habitat?sort=date)
![docker-size](https://img.shields.io/docker/image-size/ardourtech/habitat?sort=date)

A Dockerized (Ubuntu based) Developer Environment with assistive CLI Tooling

## What's Installed?

Out of the box, Habitat is built from `ubuntu:21.04` and includes:

* [X11 Window Support](https://en.wikipedia.org/wiki/X_Window_System)
  * (use `--env DISPLAY=host.docker.internal:0` or similar while running)
* [Fish](https://fishshell.com/) + [omf](https://github.com/Pyppe/oh-my-fish) as
  the default shell. Including the plugins/theme:
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
