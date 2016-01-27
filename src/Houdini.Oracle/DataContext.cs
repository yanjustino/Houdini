using Oracle.ManagedDataAccess.Client;
using System;

namespace Houdini.Oracle
{
    public abstract class DataContext : IDisposable
    {
        public DataContext(string connectionString)
        {
            var connection = new OracleConnection(connectionString);
            var transaction = new DataContextTransaction(connection);
            DataBase = new DataBaseConfiguration(transaction);

            OnCreating();
        }

        public DataContext(string dataSource, string user, string password)
            : this($"data source={dataSource};user id={user};password={password}")
        { }

        public DataBaseConfiguration DataBase { get; private set; }

        protected virtual void OnCreating() { }

        public virtual void SaveChanges()
        {
            DataBase.Transaction.Commit();
        }

        public void Dispose()
        {
            DataBase.Dispose();
        }
    }
}
