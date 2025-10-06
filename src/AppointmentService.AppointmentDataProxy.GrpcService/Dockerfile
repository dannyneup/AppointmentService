FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AppointmentService.AppointmentDataProxy.GrpcService/AppointmentService.AppointmentDataProxy.GrpcService.csproj", "src/AppointmentService.AppointmentDataProxy.GrpcService/"]
RUN dotnet restore "src/AppointmentService.AppointmentDataProxy.GrpcService/AppointmentService.AppointmentDataProxy.GrpcService.csproj"
COPY . .
WORKDIR "/src/src/AppointmentService.AppointmentDataProxy.GrpcService"
RUN dotnet build "./AppointmentService.AppointmentDataProxy.GrpcService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AppointmentService.AppointmentDataProxy.GrpcService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppointmentService.AppointmentDataProxy.GrpcService.dll"]
