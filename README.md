# Mysql Reverse Engineering

## Using Context And ConnectionStrings From appsettings.json

`dotnet ef dbcontext scaffold Name=ConnectionStrings:Chat --context ChatDBContext Pomelo.EntityFrameworkCore.MySql --output-dir Models --data-annotations --use-database-names --force`