using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Houdini.Oracle
{
    public class DataMapper
    {
        protected DataContext Context { get; set; }

        public DataMapper(DataContext context)
        {
            Context = context;
        }

        internal OracleDataReader QueryReader(string sql, object param, object cursor, CommandType commandType)
        {
            OracleCommand command = null;

            if (commandType == CommandType.Text)
                command = QueryEngine.Prepare(Context.DataBase.Transaction.Connection, sql, param, cursor);
            else
            {
                var procedure = new { command = sql, input = param, cursor = cursor };
                command = QueryEngine.Prepare(Context.DataBase.Transaction.Connection, procedure);
            }

            return command.ExecuteReader();
        }

        public IEnumerable<T> Query<T>(string sql, object param, object cursor, CommandType commandType = CommandType.Text)
        {
            Context.DataBase.Transaction.Open();

            var result = new List<T>();
            var rows = QueryReader(sql, param, cursor, commandType);

            while (rows.Read())
            {
                var instance = Activator.CreateInstance<T>();

                foreach (var property in instance.GetType().GetProperties())
                {
                    if (rows[property.Name] == null) continue;
                    property.SetValue(instance, rows[property.Name]);
                }

                result.Add(instance);
            }

            Context.DataBase.Transaction.Close();
            return result;
        }

        protected void ExecuteNonQuery(string sql, object param, CommandType commandType = CommandType.Text)
        {
            try
            {
                OracleCommand command = (commandType == CommandType.Text)
                ? QueryEngine.Prepare(Context.DataBase.Transaction.Connection, sql, param)
                : QueryEngine.Prepare(Context.DataBase.Transaction.Connection, new { command = sql, input = param });

                command.Transaction.Connection.Open();
                command.ExecuteNonQuery();
                command.Transaction.Connection.Open();
            }
            catch (Exception ex)
            {
                Context.DataBase.Transaction.Rollback();
                throw ex;
            }
        }

        protected void Execute(string sql, object param, CommandType commandType = CommandType.Text)
        {
            try
            {
                OracleCommand command = (commandType == CommandType.Text)
                ? QueryEngine.Prepare(Context.DataBase.Transaction.Connection, sql, param)
                : QueryEngine.Prepare(Context.DataBase.Transaction.Connection, new { command = sql, input = param });

                Context.DataBase.Transaction.BeginTransaction();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Context.DataBase.Transaction.Rollback();
                throw ex;
            }
        }
    }

    public class DataMapper<T> : DataMapper where T : class
    {
        public DataMapper(DataContext context) : base(context) { }

        public IEnumerable<T> Query(string sql, object param, object cursor, CommandType commandType = CommandType.Text)
        {
            return base.Query<T>(sql, param, cursor, CommandType.StoredProcedure);
        }

        public IEnumerable<T> Query<TProcedureMapping>(object param, object cursor = null)
            where TProcedureMapping : IProcedureMapping
        {
            var Mapping = Context.DataBase.Get<TProcedureMapping>();
            var outCursor = cursor ?? Mapping.Cursor;
            var result = new List<T>();

            Context.DataBase.Transaction.Open();
            var reader = QueryReader(Mapping.GetCommandText, param, outCursor, CommandType.StoredProcedure);

            while (reader.Read())
            {
                var instance = Activator.CreateInstance<T>();

                foreach (var property in instance.GetType().GetProperties())
                {
                    var field = Mapping.Properties.FirstOrDefault(x => x.Name == property.Name);

                    if (field == null && reader[field.Column] == null)
                        continue;

                    property.SetValue(instance, reader[field.Column]);
                }

                result.Add(instance);
            }
            Context.DataBase.Transaction.Close();

            return result;
        }
    }
}
