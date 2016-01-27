using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Houdini.Oracle
{
    internal class QueryEngine
    {
        public static OracleCommand Prepare(OracleConnection connection, object procedure)
        {
            var command = new OracleCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = GetCommandText(procedure);
            command.Connection = connection;

            command.Parameters.Clear();
            command.Parameters.AddRange(GetInput(procedure).ToArray());
            command.Parameters.AddRange(GetOutput(procedure).ToArray());
            command.Parameters.AddRange(GetCursor(procedure).ToArray());

            return command;
        }

        public static OracleCommand Prepare(OracleConnection connection, string sql, object param, object cursor = null)
        {
            var command = new OracleCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            command.Connection = connection;

            command.Parameters.Clear();
            command.Parameters.AddRange(GetInput(new { input = param }).ToArray());

            if (cursor != null)
                command.Parameters.AddRange(GetCursor(new { cursor = cursor }).ToArray());

            return command;
        }

        private static string GetCommandText(object procedure)
        {
            var command = GetValueFromProperty(procedure, "command");
            return command.ToString();
        }

        private static IEnumerable<OracleParameter> GetInput(object procedure)
        {
            var propertyInput = GetValueFromProperty(procedure, "input");
            var input = propertyInput?.GetType()?.GetProperties();

            if (input != null)
            {
                foreach (PropertyInfo property in input)
                {
                    yield return new OracleParameter
                    {
                        ParameterName = property.Name,
                        Value = property.GetValue(propertyInput, null),
                        Direction = ParameterDirection.Input
                    };
                }
            }
        }

        private static IEnumerable<OracleParameter> GetOutput(object procedure)
        {
            var outputProperty = GetValueFromProperty(procedure, "output");
            var output = outputProperty?.GetType()?.GetProperties();

            if (output != null)
            {
                foreach (PropertyInfo property in output)
                {
                    yield return new OracleParameter
                    {
                        ParameterName = property.Name,
                        Value = property.GetValue(outputProperty, null),
                        Direction = ParameterDirection.Output
                    };
                }
            }
        }

        private static IEnumerable<OracleParameter> GetCursor(object procedure)
        {
            var cursorProperty = GetValueFromProperty(procedure, "cursor");
            var cursor = cursorProperty?.GetType()?.GetProperties();

            if (cursor != null)
            {
                foreach (PropertyInfo property in cursor)
                {
                    yield return new OracleParameter
                    {
                        ParameterName = property.Name,
                        Direction = ParameterDirection.Output,
                        OracleDbType = OracleDbType.RefCursor
                    };
                }
            }
        }

        private static object GetValueFromProperty(object procedure, string property)
        {
            var prop = procedure.GetType()
                .GetProperties()
                .FirstOrDefault(x => x.Name.ToLower() == property);

            return prop?.GetValue(procedure, null);
        }
    }
}
