FROM microsoft/aspnetcore
ENTRYPOINT ["dotnet", "PodcastsSyndicate.dll"]
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .