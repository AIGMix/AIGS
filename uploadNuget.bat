.\Nuget pack AIGS.csproj
REM .\nuget push AIGS.1.0.2.nupkg apikey -Source https://www.nuget.org/packages
PAUSE

dotnet pack --configuration Release
dotnet nuget push "bin/Release/AIGS.1.0.0.nupkg" --source "github"