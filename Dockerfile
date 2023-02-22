FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5211

ENV ASPNETCORE_URLS=http://+:5211
ENV MongoDbConnection=mongodb://root:password1!@3.234.48.79:27017
ENV RabbitMQConnection=3.234.48.79
ENV RabbitMQUser=rhel_demo
ENV RabbitMQPassword=password1!

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["REDHAT_DEMO.csproj", "./"]
RUN dotnet restore "REDHAT_DEMO.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "REDHAT_DEMO.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "REDHAT_DEMO.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REDHAT_DEMO.dll"]

