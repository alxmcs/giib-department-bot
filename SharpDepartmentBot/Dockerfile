FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SharpDepartmentBot/SharpDepartmentBot.csproj", "SharpDepartmentBot/"]
RUN dotnet restore "SharpDepartmentBot/SharpDepartmentBot.csproj"
COPY . .
WORKDIR "/src/SharpDepartmentBot"
RUN dotnet build "SharpDepartmentBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharpDepartmentBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharpDepartmentBot.dll"]