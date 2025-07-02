dotnet run --project ./CakeBuild/CakeBuild.csproj -- "$@"
rm -rf "$VINTAGE_STORY/Mods/riftwardplus"
cp -r ./Releases/riftwardplus "$VINTAGE_STORY/Mods/riftwardplus"