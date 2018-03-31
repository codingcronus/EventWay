$key = "oy2pqwmsawx7qs3r7xoaedd3ygrakfhezdithr24oawssq"
$version = "1.0.1"
$packages = ls -r Async.EventWayCore.*.$version.nupkg

foreach ($package in $packages.FullName) 
{
    dotnet nuget push $package --api-key $key --source https://api.nuget.org/v3/index.json
}



