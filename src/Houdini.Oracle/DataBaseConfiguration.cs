using System;
using System.Collections.Generic;

namespace Houdini.Oracle
{
    public class DataBaseConfiguration : IDisposable
    {
        private Dictionary<Type, IProcedureMapping> _procedures;

        internal DataBaseConfiguration(DataContextTransaction transaction)
        {
            Transaction = transaction;
            _procedures = new Dictionary<Type, IProcedureMapping>();
        }

        public DataContextTransaction Transaction { get; private set; }

        public void Add(IProcedureMapping mapping)
        {
            _procedures.Add(mapping.GetType(), mapping);
        }

        public IProcedureMapping Get<T>()
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
