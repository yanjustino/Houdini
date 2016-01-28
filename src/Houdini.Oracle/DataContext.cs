using System;

namespace Houdini.Oracle
{
    public abstract class DataContext : IDisposable
    {
        public DataContext(string connectionString)
        {
            var transaction = new DataContextTransaction(connectionString);
            DataBase = new DataBaseConfiguration(transaction);
            OnCreating();
        }

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
