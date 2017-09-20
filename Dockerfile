ARG source=.

FROM microsoft/dotnet as builder
WORKDIR /builder
COPY $source .
RUN ["dotnet", "restore"]
RUN ["dotnet", "publish", "-c", "Release"]

FROM microsoft/aspnetcore
WORKDIR /app
EXPOSE 80
COPY --from=builder /builder/bin/Release/netcoreapp2.0/publish .
ENTRYPOINT ["dotnet", "PodcastsSyndicate.dll"]