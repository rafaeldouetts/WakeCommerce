# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER root

WORKDIR /dotnet

COPY --from=pyroscope/pyroscope-dotnet:0.9.4-glibc /Pyroscope.Profiler.Native.so ./Pyroscope.Profiler.Native.so
COPY --from=pyroscope/pyroscope-dotnet:0.9.4-glibc /Pyroscope.Linux.ApiWrapper.x64.so ./Pyroscope.Linux.ApiWrapper.x64.so

RUN ls -l /var/log && mkdir -p /var/log/pyroscope/dotnet && chown -R 1000:1000 /var/log/pyroscope

RUN mkdir -p /var/log/pyroscope/dotnet && chown -R 1000:1000 /var/log/pyroscope

EXPOSE 8080
EXPOSE 8081

# config Pyroscope
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={BD1A650D-AC5D-4896-B64F-D6FA25D6B26A}
ENV CORECLR_PROFILER_PATH=/dotnet/Pyroscope.Profiler.Native.so
ENV LD_PRELOAD=/dotnet/Pyroscope.Linux.ApiWrapper.x64.so
ENV DD_INTERNAL_OPERATIONAL_METRICS_ENABLED=true
ENV DD_TRACE_DEBUG=true

ENV PYROSCOPE_APPLICATION_NAME=WakeCommerce
ENV PYROSCOPE_SERVER_ADDRESS=http://pyroscope:4040/
ENV PYROSCOPE_LOG_LEVEL=debug
ENV PYROSCOPE_PROFILING_ENABLED=1
ENV PYROSCOPE_PROFILING_ALLOCATION_ENABLED=true
ENV PYROSCOPE_PROFILING_CONTENTION_ENABLED=true
ENV PYROSCOPE_PROFILING_EXCEPTION_ENABLED=true


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/WakeCommerce.ApiService/WakeCommerce.ApiService.csproj", "src/WakeCommerce.ApiService/"]
COPY ["src/WakeCommerce.Application/WakeCommerce.Application.csproj", "src/WakeCommerce.Application/"]
COPY ["src/WakeCommerce.Core/WakeCommerce.Core.csproj", "src/WakeCommerce.Core/"]
COPY ["src/WakeCommerce.Domain/WakeCommerce.Domain.csproj", "src/WakeCommerce.Domain/"]
COPY ["src/WakeCommerce.Infrastructure/WakeCommerce.Infrastructure.csproj", "src/WakeCommerce.Infrastructure/"]
COPY ["src/WakeCommerce.ServiceDefaults/WakeCommerce.ServiceDefaults.csproj", "src/WakeCommerce.ServiceDefaults/"]
RUN dotnet restore "./src/WakeCommerce.ApiService/WakeCommerce.ApiService.csproj"
COPY . .
WORKDIR "/src/src/WakeCommerce.ApiService"
RUN dotnet build "./WakeCommerce.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WakeCommerce.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WakeCommerce.ApiService.dll"]