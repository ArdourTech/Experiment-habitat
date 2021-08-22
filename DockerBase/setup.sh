#!/usr/bin/env bash
set -e

## Install NVM
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.38.0/install.sh | bash

## Install Jabba
curl -sL https://github.com/shyiko/jabba/raw/master/install.sh | bash -s -- --skip-rc && . "$HOME/.jabba/jabba.sh"

## Configure Fish and OMF
curl -sL https://get.oh-my.fish > install
fish install --noninteractive
rm -f install
fish -c 'omf channel dev'
fish -c 'omf update'
fish -c 'omf install bobthefish keychain https://github.com/fabioantunes/fish-nvm https://github.com/edc/bass'
echo 'set -xg JABBA_HOME $HOME/.jabba' >> "$HOME/.config/omf/before.init.fish"
echo '[ -s "$JABBA_HOME/jabba.fish" ] && source "$JABBA_HOME/jabba.fish"' >> "$HOME/.config/omf/before.init.fish"

### Configure NVM
fish -c 'nvm install node; and nvm use node; and npm --version; and node --version'

### Configure Java
fish -c "jabba install $JAVA_VERSION; and jabba alias default $JAVA_VERSION; and java --version"

### Install and Configure Clojure
curl -sL https://download.clojure.org/install/linux-install-$CLOJURE_VERSION.sh | bash -s -- --prefix "$HOME/.clojure"
echo 'set -xg PATH $HOME/.clojure/bin $PATH' >> "$HOME/.config/omf/before.init.fish"
fish -c 'clojure -e "(inc 1)"'

### Install and Configure Leiningen
mkdir -p "$HOME/.lein"
curl -sL https://raw.githubusercontent.com/technomancy/leiningen/stable/bin/lein > "$HOME/.lein/lein"
chmod u+x "$HOME/.lein/lein"
echo 'set -xg PATH $HOME/.lein $PATH' >> "$HOME/.config/omf/before.init.fish"