#!/usr/bin/env bash
## Dockerfile Usage:
## USER root
## COPY scripts/as_root/install_docker.sh ./
## RUN bash install_docker.sh && rm -f install_docker.sh
set -e

groupadd docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh > /dev/null 2>&1
rm -f get-docker.sh
usermod -aG docker $HABITAT_USER
newgrp docker
