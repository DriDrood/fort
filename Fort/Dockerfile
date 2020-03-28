FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /Fort

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /Fort
COPY --from=build-env /Fort/out .
ENTRYPOINT ["dotnet", "Fort.dll"]
