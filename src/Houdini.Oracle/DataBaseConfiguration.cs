using Houdini.Oracle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houdini.Oracle
{
    public class DataBaseConfiguration : IDisposable
    {
        private Dictionary<Type, IProcedureMapping> _procedures;

        internal DataBaseConfiguration(DataContextTransaction transaction)
        {
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
