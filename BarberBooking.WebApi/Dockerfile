FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BarberBooking.WebApi/BarberBooking.WebApi.csproj", "BarberBooking.WebApi/"]
COPY ["BarberBooking.Application/BarberBooking.Application.csproj", "BarberBooking.Application/"]
COPY ["BarberBooking.Domain/BarberBooking.Domain.csproj", "BarberBooking.Domain/"]
COPY ["BarberBooking.Infrastructure/BarberBooking.Infrastructure.csproj", "BarberBooking.Infrastructure/"]

RUN dotnet restore "BarberBooking.WebApi/BarberBooking.WebApi.csproj"

COPY . .

WORKDIR /src/BarberBooking.WebApi
RUN dotnet build "BarberBooking.WebApi.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "BarberBooking.WebApi.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BarberBooking.WebApi.dll"]