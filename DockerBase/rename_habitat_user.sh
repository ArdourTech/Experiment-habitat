#!/usr/bin/env bash
## Dockerfile Usage:
## USER root
## COPY scripts/as_root/rename_habitat_user.sh ./
## RUN bash rename_habitat_user.sh && rm -f rename_habitat_user.sh
set -e
groupmod --new-name $HABITAT_USER habitat
usermod \
    --home /home/$HABITAT_USER \
    --move-home \
    --gid $HABITAT_USER \
    --login $HABITAT_USER \
    habitat
echo "$HABITAT_USER":"$(printenv --null HABITAT_USER_PASSWORD)" | chpasswd
