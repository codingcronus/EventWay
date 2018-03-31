echo off
SET version=%1

nuget push Async.EventWay.Core.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.ApplicationInsights.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.CosmosDb.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.InMemory.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.MsSql.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.Redis.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Infrastructure.Sqlite.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
nuget push Async.EventWay.Query.%version%.nupkg 4ae58e2e-2e9b-4bae-b081-8de9fcbd9097 -Source https://www.nuget.org/api/v2/package
