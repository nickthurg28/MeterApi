# MeterApi

Entity Framework
Ensure that the dotnet global tool is installed fr net8
dotnet tool install --global dotnet-ef --version 8.*

To create the db
dotnet ef migrations add InitialCreate

To update the db
dotnet ef database update