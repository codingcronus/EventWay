echo off
SET version=%1
SET project=%2

nuget push Async.EventWay.%project%.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package