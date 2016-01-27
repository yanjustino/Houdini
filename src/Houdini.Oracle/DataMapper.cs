using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houdini.Oracle
{
    public class DataMapper
    {
        protected DataContext Context { get; set; }
        protected OracleConnection Connection { get; set; }

        public DataMapper(DataContext context)
        {
            Context = context;
            Connection = context.Transaction.Connection;
        }

        protected EnumerableRowCollection<DataRow> Query(string sql, object param, object cursor, CommandType commandType)
        {
            try
            {
                OracleCommand command = null;

                if (commandType == CommandType.Text)
                    command = QueryEngine.Prepare(Connection, sql, param, cursor);
                else
                    command = QueryEngine.Prepare(Connection, new
                    {
                        command = sql,
                        input = param,
                        cursor = cursor
                    });

                Connection.Open();
                var dataSet = GetDataSet(command);
                var rows = dataSet.Tables[0].AsEnumerable();
                dataSet.Dispose();

                return rows;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        protected IEnumerable<T> Query<T>(string sql, object param, object cursor, CommandType commandType = CommandType.Text)
        {
            var rows = Query(sql, param, cursor, commandType);

            foreach (var row in rows)
            {
                var instance = Activator.CreateInstance<T>();

                foreach (var property in instance.GetType().GetProperties())
                {
                    if (row[property.Name] == null) continue;
                    property.SetValue(instance, row[property.Name]);
                }

                yield return instance;
            }
        }

        protected void Execute(string sql, object param, CommandType commandType = CommandType.Text)
        {
            OracleCommand command = (commandType == CommandType.Text)
                ? QueryEngine.Prepare(Connection, sql, param)
                : QueryEngine.Prepare(Connection, new { command = sql, input = param });

            Context.DataBase.Transaction.BeginTransaction();
            command.ExecuteNonQuery();
        }

        private DataSet GetDataSet(OracleCommand command)
        {
            using (var adapter = new OracleDataAdapter(command))
            {
                var dataset = new DataSet();
                adapter.Fill(dataset);
                return dataset;
            }
        }
    }

    public class DataMapper<T> : DataMapper where T : class
    {
        public DataMapper(DataContext context) : base(context) { }

        protected IEnumerable<T> Query<TMapper>(object param)
            where TMapper : IProcedureMapping
        {
            var Mapping = Context.DataBase.Get<TMapper>();
            var rows = Query(Mapping.GetCommandText, param, Mapping.Cursor, CommandType.StoredProcedure);
            return MappingFields(rows, Mapping);
        }

        private IEnumerable<T> MappingFields(EnumerableRowCollection<DataRow> rows, IProcedureMapping Mapping)
        {
            foreach (var row in rows)
            {
                var instance = Activator.CreateInstance<T>();

                foreach (var property in instance.GetType().GetProperties())
                {
                    var field = Mapping.Properties.FirstOrDefault(x => x.Name == property.Name);

                    if (field == null && row[field.Column] == null)
                        continue;

                    property.SetValue(instance, row[field.Column]);
                }

                yield return instance;
            }
        }
    }
}
