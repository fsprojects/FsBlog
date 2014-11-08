#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  [ ! -f packages/FAKE/tools/FAKE.exe ] && .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion -Prerelease
  packages/FAKE/tools/FAKE.exe build.fsx $@
else
  # use mono
  [ ! -f packages/FAKE/tools/FAKE.exe ] && mono --runtime=v4.0 .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion -Prerelease
  mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi
