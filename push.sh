#!/bin/bash

set -e

./build.sh

docker push twitchax/podcastssyndicate:latest
