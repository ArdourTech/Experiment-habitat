# Habitat

> *STATUS: In Development. Not yet ready for Production*

![docker-version](https://img.shields.io/docker/v/ardourtech/habitat?sort=date)
![docker-size](https://img.shields.io/docker/image-size/ardourtech/habitat?sort=date)

A Dockerized (Ubuntu based) Developer Environment with assistive CLI Tooling

---

## Reasoning

As a developer, I'm frequently switching between Windows, Linux, and MacOS. In
each environment, I may need to perform the same tasks, leverage the same
automation, or just want a consistent experience. But it can become a PIMA
when the environments are not configured to be identical, or when a tool doesn't
operate in the same way (looking at you CLI flags).

[Dotfile](https://dotfiles.github.io/) repositories really only solve half the
problem. They fall short when you need to start applying OS specific tools and
configuration. Docker *might* be a step in the right direction.

> Docker takes away repetitive, mundane configuration tasks and is used
> throughout the development lifecycle for fast, easy and portable application
> development - desktop and cloud ~ <https://www.docker.com/>

---

## CLI

Habitat CLI is a tool (written in .NET) for building, running, and managing
Dockerized Developer Environments.

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

Using Basic Docker markup, we can take a basic Dockerfile foundation and
customize the Environments to suit our needs. We can either create a single all
inclusive Environment for developing multiple projects and languages, or build a
specific container per project or language.

The `habitat` CLI will

* At `build` - Provide `build-arg`s at Environment Build time to supply the
  Image definition with:
  * `HABITAT_USER` - The Environments intended user name
  * `HABITAT_USER_PASSWORD` - The Environments user password
* At `start` - Read the Image definition, looking for specific labels to control
  how the Container should be started
  * `HABITAT_WITH_X11=true` starts the Container with the environment variable
    `DISPLAY="host.docker.internal:0"` for X11 display binding.
  * `HABITAT_WITH_DOCKER=true` starts the Container with the hosts
    `/var/run/docker.sock` bound. Providing Container->Host Docker access.
  * `HABITAT_NETWORKS=network1,network2` starts the Container and attaches it to
    the specified Networks. If the networks do not exists, they will be created
    as `bridge`s.
  * `HABITAT_VOLUMES=volume1,volume2` starts the Container with the specified
    named Volumes. If the volumes do not exist, they will be created.

### Example Dockerfile

```dockerfile
FROM ardourtech/habitat:2021-09-19
LABEL maintainer="Alexander Scott <xander@axrs.io>"

# Build Arguments provided (and if necessary prompted for) by the Habitat CLI
ARG HABITAT_USER
ARG HABITAT_USER_PASSWORD

#Instructs the Habitat CLI to bind the Hosts Display for X11 on Container Start
LABEL HABITAT_WITH_X11=true

#Instructs the Habitat CLI to bind the Hosts Docker sock on Container Start
LABEL HABITAT_WITH_DOCKER=true

#Instructs the Habitat CLI to create the Networks and attach the Container to them
LABEL HABITAT_NETWORKS=habitat,dev

#Instructs the Habitat CLI to create Volumes and to attach them on Contianer Start
# - Volumes are unique by name
# - Volumes can be shared with multiple containers
LABEL HABITAT_VOLUME_ROOT="/volumes/"
LABEL HABITAT_VOLUMES=projects,maven-cache

# Add the User as a new Ubuntu User
USER root
RUN groupmod --new-name $HABITAT_USER docker
RUN usermod \
    --home /home/$HABITAT_USER \
    --move-home \
    --gid $HABITAT_USER \
    --login $HABITAT_USER \
    docker
RUN echo "${HABITAT_USER}:${HABITAT_USER_PASSWORD}" | chpasswd

# Create the volume mount points for Habitat. This can resolve some permission
# issues when using non-root users
RUN mkdir -p /volumes/projects /volumes/maven-cache
RUN chown -R ${HABITAT_USER}:${HABITAT_USER} /volumes \
    && chmod -R 775 /volumes \
    && chmod -R g+s /volumes

USER $HABITAT_USER
WORKDIR /home/$HABITAT_USER

# The default Entrypoint. Habitat will detect this and supply it when using the
# `connect` command
ENTRYPOINT ["fish"]
```

### Building the Environment

With a Developer Environment Image defined we can use the `habitat` CLI to start
the Docker Build process. It is during this time that the `habitat` CLI will
prompt for the `HABITAT_USER` and `HABITAT_PASSWORD` values.

```bash
habitat[.exe] build \
  --user <environment-user-name> \
  --password <environment-user-password> \
  --directory <path-to-docker-build-context> \
  --file <habitat-docker-file> \
  --tag <docker-image-tag>
````

### Starting the Environment

Finally with an Environment Built, we can start a Container and establish a
connection.

```bash
habitat[.exe] start \
  --image <docker-image-tag> \
  --name <docker-container-name>

# Windows
iex "$(habitat.exe connect --name <env-name>)"

# MacOS/Unix
eval "$(habitat connect --name <env-name>)"
```
