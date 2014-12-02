Before running tests, add app.config file that looks like below:

<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>

  <appSettings>
    <add key="DocDb.Primary.Uri" value="~ your uri ~"/>
    <add key="DocDb.Primary.AuthKey" value="~ your auth key ~"/>
    <add key="DocDb.Primary.DatabaseId" value="~ your database id ~"/>

    <add key="DocDb.Secondary.Uri" value="~ your uri ~"/>
    <add key="DocDb.Secondary.AuthKey" value="~ your auth key ~"/>
    <add key="DocDb.Secondary.DatabaseId" value="~ your database id ~"/>

    <add key="Gateway.StorageAccount" value="~ your connection string ~"/>

    <add key="Table.Primary.StorageAccount" value="~ your connection string ~"/>
    <add key="Table.Secondary.StorageAccount" value="~ your connection string ~"/>
    
  </appSettings>
</configuration>