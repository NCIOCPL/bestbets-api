#!/bin/bash

# Used in CI builds to abstract any API-specific start up steps.
export CGBestBetsIndex__PreviewAliasName=bestbets_v1
export CGBestBetsIndex__LiveAliasName=bestbets_v1
dotnet NCI.OCPL.Api.BestBets.dll