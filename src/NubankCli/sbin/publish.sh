#!/bin/bash

#dotnet publish --output "./out" --runtime win-x64 --configuration Release \
  #-p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true

dotnet publish --output "./out" -r win-x64 -p:PublishSingleFile=true --self-contained false -c Release