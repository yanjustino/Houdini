
#Houdini.Oracle

![houdini](https://www.yanjustino.net/content/images/2016/01/houdini-and-eagle-otrcat-com.png)

Houdini ORM to mapping Procedures and Queries results

##Install Houdini
To install houdini run:
````
Install-Package Houdini.Oracle
```

##Create Context
Context is a Unity Of Work

```csharp
using Houdini.Oracle

namespace Query
{
    public class Context : DataContext
    {
        public Context(string connectionString) : base(connectionString) { }
        public Context(string dataSource, string user, string password) : base(dataSource, user, password) { }

        protected override void OnCreating()
        {
            DataBase.Add(new ProcedureRetornaContador());
        }
    }
}
```

## Mapping Stored Procedures
```csharp
using Houdini.Oracle;
using System.Linq;

namespace QueryModels
{
    internal class Contador
    {
        public string Nome { get; set; }
        public long CRC { get; set; }
        public string UF { get; set; }
        public string Tipo { get; set; }
        public string Endereco { get; set; }
    }

    internal class ProcedureRetornaContador : ProcedureMapping<Contador>
    {
        public ProcedureRetornaContador()
        {
            SetOwner("NOME_OWNER");
            SetPackage("NOME_PACOTE");
            SetProcedure("NOME_PROCEDURE");
            SetCursor(new { retCusrsor = 0 });

            Property(x => x.Nome).Column("NOME_CONTADOR");
            Property(x => x.CRC).Column("NUMERO_CRC");
            Property(x => x.UF).Column("UF");
            Property(x => x.Tipo).Column("TIPOP_CRC");
            Property(x => x.Endereco).Column("ENDERECO");
        }
    }

    internal class ContadorDataMapper : DataMapper<Contador>
    {
        public ContadorDataMapper(Context context) : base(context) { }

        public Contador Localizar(int id)
        {
            return Query<ProcedureRetornaNomeContabilista>(new { pIdContador = id }).FirstOrDefault();
        }
    }
}
```

##Implicit Mapping 

```csharp
using Houdini.Oracle;
using System.Data;

namespace QueryModels
{
    internal class RepresentanteModel
    {
        public string CNPF_CNPJ { get; set; }
        public string REPRESENTANTE { get; set; }
        public string QUALIFICACAO { get; set; }
        public string ENDERECO { get; set; }
    }

    internal class RepresentanteDataMapper : DataMapper
    {
        public RepresentanteDataMapper(Context context) : base(context) { }

        public IEnumerable<RepresentanteModel> Localizar(id id)
        {
            return Query<RepresentanteModel>
                (
                    sql: "OWNER.PACOTE.PROCEDURE",
                    param: new { pId = id },
                    cursor: new { rCursor = 0 },
                    commandType: CommandType.StoredProcedure
                );
        }
    }
}

```

## Executing 

```csharp
using Houdini.Oracle;
using System.Data;

namespace QueryModels
{
    internal class RepresentanteDataMapper : DataMapper
    {
        public RepresentanteDataMapper(Context context) : base(context) { }

        public IEnumerable<RepresentanteModel> Adicionar(int id, string name)
        {
            Execute
                (
                    sql: "INSERT INTO Tabela (id, name) VALUES (:pId, :pName)",
                    param: new { pId = id, pName = name },
                    commandType: CommandType.Text
                );
        }
    }
}

```
