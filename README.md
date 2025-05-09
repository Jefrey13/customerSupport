**Comando para ejecutar database first:**

Scaffold-DbContext "Server=LAPTOP-N56GM63T;Database=CustomerSupportDB;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data\context -Context CustomerSupportContext -Schemas auth,chat,crm,admin -UseDatabaseNames -Force


* Cambiar el nombre de la instancia: si es sql server ejecutar el comando SELECT @@SERVERNAME;, para obtenedor.