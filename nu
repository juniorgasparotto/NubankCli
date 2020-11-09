DIR="$(dirname "${BASH_SOURCE[0]}")"
dotnet run --project "$DIR/src/NubankCli/NubankCli.csproj" "$@"