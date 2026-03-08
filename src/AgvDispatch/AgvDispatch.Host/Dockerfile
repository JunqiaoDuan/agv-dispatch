FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY app/ ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AgvDispatch.Host.dll"]
