#!/bin/bash

set -e

./push.sh

az webapp restart -n podcastssyndicate -g PodcastsSyndicate
