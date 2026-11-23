# This is the Dockerfile for the Okolariet Exam API.
# Its the file that defines how our docker image is built.
# Compose file defines how to run the API and sql server CONTAINERS.

# Choosing our base image for the build stage, the offical .NET SDK image
# contains the compiler and tools needed to build the application.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the working directory to /source, or create it if it doesn't exist.
# Its here we copy the project files and restore dependencies.
WORKDIR /source

# Copy the project's .csproj from the instance into the container.
# The .csproj files are used to restore dependencies.
COPY Host/Host.csproj Host/
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
# Restore dependencies
RUN dotnet restore Host/Host.csproj

# We copy again, but this time the entire project, including source code.
# We do it this way for speed... for example when only c# code is changed docker
# can reuse the previous dependencies and only rebuild what is necessary.
COPY Host/ Host/
COPY src/Api/ src/Api/
COPY src/Application/ src/Application/
COPY src/Domain/ src/Domain/
COPY src/Infrastructure/ src/Infrastructure/
# Publish the application (gathering everything needed to run the application into one direcotry)
RUN dotnet publish Host/Host.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# The final package, it gets the runtime and the compiled application (published)
# "Shipping the package without shipping all the tools to build it"
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Sets and enviroment variable inside the container ASP uses its 8080 port-value as the one the API listens on.
ENV ASPNETCORE_HTTP_PORTS=8080
# Goofy syntax alert! (8080 doesnt get exposed here, its only here to document) 8080 gets exposed in compose
EXPOSE 8080
# Switched to "non root" user provided by the base image so the API runs with managed permissions
USER $APP_UID
# Starts the backend
ENTRYPOINT ["dotnet", "Host.dll"]
