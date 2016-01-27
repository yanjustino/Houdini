using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace Houdini.Oracle
{
    public class DataContextTransaction : IDisposable
    {
        internal OracleConnection Connection { get; private set; }
        internal OracleTransaction Transaction { get; private set; }

        public DataContextTransaction(OracleConnection connection)
        {
            Connection = connection;
        }

        public virtual void Open()
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
        }

        public virtual void Close()
        {
            Dispose();
        }

        public virtual void Commit()
        {
            Transaction.Commit();

            if (Connection.State == ConnectionState.Open)
                Connection.Close();
        }

        public virtual void Rollback()
        {
            Transaction.Rollback();
        }

        public DataContextTransaction BeginTransaction()
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();

            if (Transaction == null)
                Transaction = Connection.BeginTransaction();

            return this;
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
