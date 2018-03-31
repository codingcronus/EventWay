echo off
SET version=%1

nuget pack EventWay.Core/EventWay.Core.csproj -Version %version%
nuget pack EventWay.Infrastructure/EventWay.Infrastructure.csproj -Version %version%
nuget pack EventWay.Infrastructure.ApplicationInsights/EventWay.Infrastructure.ApplicationInsights.csproj -Version %version%
nuget pack EventWay.Infrastructure.CosmosDb/EventWay.Infrastructure.CosmosDb.csproj -Version %version%
nuget pack EventWay.Infrastructure.InMemory/EventWay.Infrastructure.InMemory.csproj -Version %version%
nuget pack EventWay.Infrastructure.MsSql/EventWay.Infrastructure.MsSql.csproj -Version %version%
nuget pack EventWay.Infrastructure.Redis/EventWay.Infrastructure.Redis.csproj -Version %version%
nuget pack EventWay.Infrastructure.Sqlite/EventWay.Infrastructure.Sqlite.csproj -Version %version%
nuget pack EventWay.Query/EventWay.Query.csproj -Version %version%
