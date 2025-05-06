**Comando para ejecutar database first:**

```Scaffold-DbContext "Server=DESKTOP-91LKTJV\SQLEXPRESS;Database=CustomerSupportDB;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Data\Models -ContextDir Data -Context CustomerSupportContext -Schemas auth,chat,crm,admin ` -UseDatabaseNames```

* Cambiar el nombre de la instancia: si es sql server ejecutar el comando SELECT @@SERVERNAME;, para obtenedor.