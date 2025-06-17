FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj và restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy tất cả files cần thiết cho build
COPY Backend/ ./Backend/
COPY Views/ ./Views/
COPY wwwroot/css/ ./wwwroot/css/
COPY wwwroot/js/ ./wwwroot/js/
COPY wwwroot/images/ ./wwwroot/images/
COPY Program.cs ./
COPY appsettings.json ./
COPY appsettings.Development.json ./

# Tạo thư mục uploads trước khi build
RUN mkdir -p wwwroot/uploads

# Build ứng dụng
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Tạo thư mục uploads
RUN mkdir -p wwwroot/uploads && chmod 777 wwwroot/uploads

# Expose port và set entry point
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "MiniPhotoshop.dll"] 