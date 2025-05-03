$paths = ".\PUBLISHED", ".\bin\Release\net8.0-windows\win-x64"
Get-Item -LiteralPath $paths -ErrorAction SilentlyContinue | Remove-Item -Recurse

dotnet publish .\mp32desc.csproj --runtime win-x64 --no-self-contained --output .\PUBLISHED

Get-Item -LiteralPath ".\PUBLISHED\mp32desc.pdb" -ErrorAction SilentlyContinue | Remove-Item
