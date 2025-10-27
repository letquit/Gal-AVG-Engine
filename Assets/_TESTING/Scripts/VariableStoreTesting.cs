using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace TESTING
{
    /// <summary>
    /// 提供一个可扩展的变量存储系统，支持按数据库分组管理不同类型的变量。
    /// 变量可以具有自定义的 getter 和 setter 方法，并且可以通过名称进行访问和操作。
    /// </summary>
    public class VariableStore
    {
        /// <summary>
        /// 默认数据库名称。
        /// </summary>
        private const string DEFAULT_DATABASE_NAME = "Default";

        /// <summary>
        /// 数据库与变量名之间的分隔符。
        /// </summary>
        private const char DATABASE_VARIABLE_RELATIONAL_ID = '.';

        /// <summary>
        /// 表示一个变量数据库，用于组织和管理一组相关变量。
        /// </summary>
        public class Database
        {
            /// <summary>
            /// 初始化一个新的数据库实例。
            /// </summary>
            /// <param name="name">数据库名称。</param>
            public Database(string name)
            {
                this.name = name;
                variables = new Dictionary<string, Variable>();
            }

            /// <summary>
            /// 数据库名称。
            /// </summary>
            public string name;

            /// <summary>
            /// 存储该数据库中的所有变量，键为变量名，值为对应的 Variable 实例。
            /// </summary>
            public Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        }

        /// <summary>
        /// 抽象基类，表示一个通用变量类型。提供获取和设置变量值的方法接口。
        /// </summary>
        public abstract class Variable
        {
            /// <summary>
            /// 获取变量当前的值。
            /// </summary>
            /// <returns>变量的当前值（object 类型）。</returns>
            public abstract object Get();

            /// <summary>
            /// 设置变量的新值。
            /// </summary>
            /// <param name="value">要设置的新值（object 类型）。</param>
            public abstract void Set(object value);
        }

        /// <summary>
        /// 泛型变量实现类，封装了具体类型的变量及其 getter/setter 委托。
        /// </summary>
        /// <typeparam name="T">变量的数据类型。</typeparam>
        public class Variable<T> : Variable
        {
            /// <summary>
            /// 内部保存的实际变量值。
            /// </summary>
            private T value;

            /// <summary>
            /// 自定义获取变量值的委托方法。
            /// </summary>
            private Func<T> getter;

            /// <summary>
            /// 自定义设置变量值的委托方法。
            /// </summary>
            private Action<T> setter;

            /// <summary>
            /// 初始化一个新的泛型变量实例。
            /// </summary>
            /// <param name="defaultValue">默认值。</param>
            /// <param name="getter">自定义获取变量值的方法，默认使用内部字段。</param>
            /// <param name="setter">自定义设置变量值的方法，默认修改内部字段。</param>
            public Variable(T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
            {
                value = defaultValue;

                if (getter == null)
                    this.getter = () => value;
                else
                    this.getter = getter;

                if (setter == null)
                    this.setter = newValue => value = newValue;
                else
                    this.setter = setter;
            }

            /// <summary>
            /// 获取变量当前的值。
            /// </summary>
            /// <returns>变量的当前值（object 类型）。</returns>
            public override object Get() => getter();

            /// <summary>
            /// 设置变量的新值。
            /// </summary>
            /// <param name="newValue">要设置的新值（object 类型），将被转换为目标类型 T。</param>
            public override void Set(object newValue) => setter((T)newValue);
        }

        /// <summary>
        /// 所有已创建的数据库集合，以数据库名为键，Database 对象为值。
        /// 初始时包含一个默认数据库。
        /// </summary>
        private static Dictionary<string, Database> databases = new Dictionary<string, Database>()
            { { DEFAULT_DATABASE_NAME, new Database(DEFAULT_DATABASE_NAME) } };

        /// <summary>
        /// 获取默认数据库对象。
        /// </summary>
        private static Database defaultDatabase => databases[DEFAULT_DATABASE_NAME];

        /// <summary>
        /// 创建一个新的数据库。
        /// </summary>
        /// <param name="name">新数据库的名称。</param>
        /// <returns>如果成功创建则返回 true；若同名数据库已存在，则返回 false。</returns>
        public static bool CreateDatabase(string name)
        {
            if (!databases.ContainsKey(name))
            {
                databases[name] = new Database(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据名称获取指定数据库对象。如果不存在会自动创建。
        /// </summary>
        /// <param name="name">数据库名称。如果为空字符串，则返回默认数据库。</param>
        /// <returns>对应名称的数据库对象。</returns>
        public static Database GetDatabase(string name)
        {
            if (name == string.Empty)
                return defaultDatabase;

            if (!databases.ContainsKey(name))
                CreateDatabase(name);

            return databases[name];
        }

        /// <summary>
        /// 在指定数据库中创建一个变量。
        /// </summary>
        /// <typeparam name="T">变量的数据类型。</typeparam>
        /// <param name="name">变量全名，格式为 "数据库名.变量名" 或仅 "变量名"（此时使用默认数据库）。</param>
        /// <param name="defaultValue">变量的默认值。</param>
        /// <param name="getter">自定义获取变量值的方法，默认使用内部字段。</param>
        /// <param name="setter">自定义设置变量值的方法，默认修改内部字段。</param>
        /// <returns>如果成功创建变量则返回 true；若变量已存在则返回 false。</returns>
        public static bool CreateVariable<T>(string name, T defaultValue, Func<T> getter = null, Action<T> setter = null)
        {
            (string[] parts, Database db, string variableName) = ExtractInfo(name);

            if (db.variables.ContainsKey(variableName))
                return false;

            db.variables[variableName] = new Variable<T>(defaultValue, getter, setter);

            return true;
        }

        /// <summary>
        /// 尝试根据变量全名获取其值。
        /// </summary>
        /// <param name="name">变量全名，格式为 "数据库名.变量名" 或仅 "变量名"（此时使用默认数据库）。</param>
        /// <param name="variable">输出参数，用于接收获取到的变量值。</param>
        /// <returns>如果找到并成功获取变量值则返回 true；否则返回 false。</returns>
        public static bool TryGetValue(string name, out object variable)
        {
            (string[] parts, Database db, string variableName) = ExtractInfo(name);

            if (!db.variables.ContainsKey(variableName))
            {
                variable = null;
                return false;
            }

            variable = db.variables[variableName].Get();
            return true;
        }

        /// <summary>
        /// 尝试根据变量全名设置其值。
        /// </summary>
        /// <typeparam name="T">变量的数据类型。</typeparam>
        /// <param name="name">变量全名，格式为 "数据库名.变量名" 或仅 "变量名"（此时使用默认数据库）。</param>
        /// <param name="value">要设置的新值。</param>
        /// <returns>如果找到并成功设置变量值则返回 true；否则返回 false。</returns>
        public static bool TrySetValue<T>(string name, T value)
        {
            (string[] parts, Database db, string variableName) = ExtractInfo(name);
            
            if (!db.variables.ContainsKey(variableName))
                return false;
            
            db.variables[variableName].Set(value);
            return true;
        }

        /// <summary>
        /// 解析变量全名，提取出数据库部分、变量名以及原始分割后的数组。
        /// </summary>
        /// <param name="name">变量全名，可能包含数据库前缀。</param>
        /// <returns>元组：(原始分割结果数组, 数据库对象, 变量名)</returns>
        private static (string[], Database, string) ExtractInfo(string name)
        {
            string[] parts = name.Split(DATABASE_VARIABLE_RELATIONAL_ID);
            Database db = parts.Length > 1 ? GetDatabase(parts[0]) : defaultDatabase;
            string variableName = parts.Length > 1 ? parts[1] : parts[0];
            
            return (parts, db, variableName);
        }

        /// <summary>
        /// 移除指定名称的变量。
        /// </summary>
        /// <param name="name">变量全名，格式为 "数据库名.变量名" 或仅 "变量名"（此时使用默认数据库）。</param>
        public static void RemoveVariable(string name)
        {
            (string[] parts, Database db, string variableName) = ExtractInfo(name);
            
            if (db.variables.ContainsKey(variableName))
                db.variables.Remove(variableName);
        }

        /// <summary>
        /// 清空所有数据库及其中的所有变量，并重新初始化默认数据库。
        /// </summary>
        public static void RemoveAllVariables()
        {
            databases.Clear();
            databases[DEFAULT_DATABASE_NAME] = new Database(DEFAULT_DATABASE_NAME);
        }
        
        /// <summary>
        /// 输出所有数据库的名称信息到控制台日志。
        /// </summary>
        public static void PrintAllDatabases()
        {
            foreach (KeyValuePair<string, Database> dbEntry in databases)
            {
                Debug.Log($"Database: '<color=#FFB145>{dbEntry.Key}</color>'");
            }
        }

        /// <summary>
        /// 输出所有数据库中的变量信息到控制台日志。
        /// </summary>
        /// <param name="database">可选参数，只打印特定数据库的信息。如果为 null，则打印全部数据库。</param>
        public static void PrintAllVariables(Database database = null)
        {
            if (database != null)
            {
                PrintAllDatabasesVariables(database);
                return;
            }

            foreach (var dbEntry in databases)
            {
                PrintAllDatabasesVariables(dbEntry.Value);
            }
        }

        /// <summary>
        /// 打印单个数据库内所有变量的信息。
        /// </summary>
        /// <param name="database">需要打印的数据库对象。</param>
        private static void PrintAllDatabasesVariables(Database database)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Database: <color=#F38544>{database.name}</color>");
            foreach (KeyValuePair<string, Variable> variablePair in database.variables)
            {
                string variableName = variablePair.Key;
                object variableValue = variablePair.Value.Get();
                sb.AppendLine($"\t<color=#FFB145>Variable [{variableName}]</color> = <color=#FFD22D>{variableValue}</color>");
            }
            Debug.Log(sb.ToString());
        }
    }
}