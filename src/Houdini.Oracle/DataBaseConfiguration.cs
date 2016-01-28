using System;
using System.Collections.Generic;

namespace Houdini.Oracle
{
    public sealed class DataBaseConfiguration : IDisposable
    {
        private Dictionary<Type, IProcedureMapping> _procedures;

        internal DataBaseConfiguration(DataContextTransaction transaction)
        {
            Transaction = transaction;
            ConnectionString = Transaction.Connection.ConnectionString;
            _procedures = new Dictionary<Type, IProcedureMapping>();
        }

        public DataContextTransaction Transaction { get; private set; }
        public string ConnectionString { get; private set; }

        public void Add(IProcedureMapping mapping)
        {
            _procedures.Add(mapping.GetType(), mapping);
        }

        internal IProcedureMapping Get<T>()
        {
            return _procedures[typeof(T)];
        }

        public void Dispose()
        {
            Transaction.Dispose();
            _procedures.Clear();
        }
    }
}
