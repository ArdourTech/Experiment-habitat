#!/usr/bin/env bash
set -e

# Validation that the developer dependencies are installed and available
fish -c 'java --version'
fish -c 'nvm --version'
fish -c 'node --version'
fish -c 'npm --version'
fish -c 'clojure --version'
fish -c 'lein --version'