#!/bin/sh
set -e
# check to see if nss folder is empty
if [ ! "$(ls -A $HOME/nss)" ]; then
  mkdir -p $HOME/nss;
  cd $HOME/nss
  wget https://github.com/nats-io/nats-streaming-server/releases/download/v0.12.0/nats-streaming-server-v0.12.0-linux-amd64.zip -O nss.zip;
  unzip nss.zip;
  mv $HOME/nss/nats-streaming-server-v0.12.0-linux-amd64/nats-streaming-server nats-streaming-server
else
  echo 'Using cached directory.';
fi
