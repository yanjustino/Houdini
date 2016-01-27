using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Houdini.Oracle
{
    public interface IProcedureMapping
    {
        string Owner { get; }
        string Package { get; }
        string Procedure { get; }
        object Cursor { get; }
        PropertyConfig[] Properties { get; }
        string GetCommandText { get; }
    }

    public abstract class ProcedureMapping<T> : IProcedureMapping where T : class
    {
        private List<PropertyConfig> _properties;

        public ProcedureMapping()
        {
            _properties = new List<PropertyConfig>();
        }

        public string Owner { get; private set; }
        public string Package { get; private set; }
        public string Procedure { get; private set; }
        public object Cursor { get; private set; }

        public PropertyConfig[] Properties => _properties.ToArray();
        public string GetCommandText => Owner + Package + Procedure;

        /// <summary>
        /// Set Cursor to retrieve Database results
        /// </summary>
        /// <param name="cursor">ex.: new { pCursor = new { } }</param>
        protected void SetCursor(object cursor) => Cursor = cursor;

        /// <summary>
        /// Owner Name
        /// </summary>
        /// <param name="name"></param>
        protected void SetOwner(string name) => Owner = $"{name}.";

        /// <summary>
        /// Package Name
        /// </summary>
        /// <param name="name"></param>
        protected void SetPackage(string name) => Package = $"{name}.";

        /// <summary>
        /// Procedure Name
        /// </summary>
        /// <param name="name"></param>
        protected void SetProcedure(string name) => Procedure = name;

        /// <summary>
        /// Set property to mapping
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected PropertyConfig Property<U>(Expression<Func<T, U>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                var member = memberExpression.Member;
                var config = new PropertyConfig(member.Name, expression.ReturnType);

                _properties.Add(config);

                return config;
            }

            throw new ArgumentException("Expression is not a member access", "expression");
        }


    }
}
