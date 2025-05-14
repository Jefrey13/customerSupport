**Comando para ejecutar database first:**

Scaffold-DbContext "Server=LAPTOP-N56GM63T;Database=CustomerSupportDB;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data\context -Context CustomerSupportContext -Schemas auth,chat,crm,admin -UseDatabaseNames -Force
dotnet tool restore
ngrok http https://localhost:7108 --host-header="localhost:7108"
dotnet ef database update --context CustomerSupportContext
dotnet tool install --global dotnet-ef
* Cambiar el nombre de la instancia: si es sql server ejecutar el comando SELECT @@SERVERNAME;, para obtenedor.