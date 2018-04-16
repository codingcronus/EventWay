$key = "oy2pqwmsawx7qs3r7xoaedd3ygrakfhezdithr24oawssq"
$version = "1.1.2"
$packages = ls -r -directory Release | ls -r -filter Async.EventWayCore.*.$version.nupkg

foreach ($package in $packages.FullName) 
{
    dotnet nuget push $package --api-key $key --source https://api.nuget.org/v3/index.json
}
