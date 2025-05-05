# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["EmployeeManagement.csproj", "./"]
RUN dotnet restore "./EmployeeManagement.csproj"
COPY . .
RUN dotnet publish "EmployeeManagement.csproj" -c Release -o /app/publish

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "EmployeeManagement.dll"]
