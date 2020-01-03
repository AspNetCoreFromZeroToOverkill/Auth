FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder
WORKDIR /app

# Copy solution and restore as distinct layers to cache dependencies
COPY ./src/CodingMilitia.PlayBall.Auth.Web/*.csproj ./src/CodingMilitia.PlayBall.Auth.Web/
COPY *.sln ./
RUN dotnet restore

# Publish the application
COPY . ./
WORKDIR /app/src/CodingMilitia.PlayBall.Auth.Web
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/src/CodingMilitia.PlayBall.Auth.Web/out .
ENTRYPOINT ["dotnet", "CodingMilitia.PlayBall.Auth.Web.dll"]

# Sample build command
# docker build -t codingmilitia/auth .