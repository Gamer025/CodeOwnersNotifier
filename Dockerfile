# Set the base image as the .NET 8.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./CodeOwnersParser/CodeOwnersParser.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Gamer025 <33846895+Gamer025@users.noreply.github.com>"
LABEL repository="https://github.com/Gamer025/CodeOwnersParser"
LABEL homepage="https://github.com/Gamer025/CodeOwnersParser"

# Label as GitHub action
LABEL com.github.actions.name="CodeOwnersParser"
# Limit to 160 characters
LABEL com.github.actions.description="Github action for parsing codeowners. Output can be used to notify them via comments etc."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="bell"
LABEL com.github.actions.color="white"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/runtime:8.0
COPY --from=build-env /out .
ENTRYPOINT ["dotnet", "/CodeOwnersParser.dll"]