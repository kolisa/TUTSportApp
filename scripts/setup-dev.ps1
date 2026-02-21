# One-command setup for TUTSportApp (Windows)
# 1. Checks .NET 9 SDK, Docker Desktop, dotnet-ef
# 2. Generates JWT key in User Secrets
# 3. Starts SQL Server via Docker Compose
# 4. Applies EF Core migrations
# 5. Builds solution (zero warnings)

Write-Host "Checking .NET 9 SDK..."
dotnet --list-sdks | Select-String "9."
if ($LASTEXITCODE -ne 0) { Write-Error ".NET 9 SDK not found."; exit 1 }

Write-Host "Checking Docker Desktop..."
docker info | Select-String "Server Version"
if ($LASTEXITCODE -ne 0) { Write-Error "Docker Desktop not running."; exit 1 }

Write-Host "Checking dotnet-ef..."
dotnet tool list -g | Select-String "dotnet-ef"
if ($LASTEXITCODE -ne 0) { dotnet tool install -g dotnet-ef }

Write-Host "Generating JWT key in User Secrets..."
dotnet user-secrets init --project "TUTSportApp.Api/TUTSportApp.Api.csproj"
$jwt = [System.Convert]::ToBase64String((1..48 | ForEach-Object {Get-Random -Maximum 256}))
dotnet user-secrets set "Jwt:Key" $jwt --project "TUTSportApp.Api/TUTSportApp.Api.csproj"

Write-Host "Starting SQL Server via Docker Compose..."
docker compose up -d

Write-Host "Applying EF Core migrations..."
dotnet ef database update --project "TUTSportApp.Infrastructure/TUTSportApp.Infrastructure.csproj" --startup-project "TUTSportApp.Api/TUTSportApp.Api.csproj"

Write-Host "Building solution (warnings as errors)..."
dotnet build --no-restore --warnaserror

Write-Host "Setup complete!"
