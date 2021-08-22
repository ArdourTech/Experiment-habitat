FROM ubuntu:21.04
LABEL maintainer="Alexander Scott <xander@ardour.tech>"
LABEL name=ardourtech/habitat

RUN sed -i -e 's/http:\/\/archive\.ubuntu\.com\/ubuntu\//mirror:\/\/mirrors\.ubuntu\.com\/mirrors\.txt/' /etc/apt/sources.list

RUN apt update -y && apt upgrade -y &&	apt-get clean

RUN DEBIAN_FRONTEND=noninteractive \
	apt-get install -f -y --fix-missing \
	software-properties-common \

RUN apt-add-repository ppa:fish-shell/release-3

## Common Tools
RUN DEBIAN_FRONTEND=noninteractive \
	apt-get install -y --fix-missing \
	bash \
	build-essential \
	curl \
	direnv \
	fish \
	fontconfig \
	git \
	keychain \
	libxi-dev \
	libxrender-dev \
	libxtst-dev \
	locales \
	neovim \
	rsync \
	shellcheck \
	sudo \
	unzip \
	wget \
	x11-common \
	xauth \
	zip && \
	apt-get clean

ARG JAVA_VERSION=adopt@1.15.0-2
ARG CLOJURE_VERSION=1.10.3.855
ARG USER=habitat
RUN useradd -s $(which fish) -d /home/$USER -m -G sudo $USER
USER $USER
WORKDIR /home/$USER
COPY setup.sh verify.sh ./
RUN bash setup.sh && \
	bash verify.sh && \
	rm -f setup.sh verify.sh
