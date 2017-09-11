#!/bin/bash

set -e

dotnet restore
dotnet publish -c Release

docker build bin/Release/netcoreapp2.0/publish -t twitchax/podcastssyndicate

