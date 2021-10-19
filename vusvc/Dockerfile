#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["vusvc/vusvc.csproj", "vusvc/"]
RUN dotnet restore "vusvc/vusvc.csproj"
COPY . .
WORKDIR "/src/vusvc"
RUN dotnet build "vusvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vusvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vusvc.dll"]