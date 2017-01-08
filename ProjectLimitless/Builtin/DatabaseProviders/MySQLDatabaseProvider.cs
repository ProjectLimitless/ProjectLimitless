/** 
* This file is part of Project Limitless.
* Copyright © 2016 Donovan Solms.
* Project Limitless
* https://www.projectlimitless.io
* 
* Project Limitless is free software: you can redistribute it and/or modify
* it under the terms of the Apache License Version 2.0.
* 
* You should have received a copy of the Apache License Version 2.0 with
* Project Limitless. If not, see http://www.apache.org/licenses/LICENSE-2.0.
*/

using System;
using System.Reflection;
using System.Collections.Generic;

using FluentData;

using Limitless.Runtime.Interfaces;

namespace Limitless.Builtin.DatabaseProviders
{
    /// <summary>
    /// Provides MySQL database CRUD operations.
    ///
    /// Note: This should be moved - see issue #1 
    /// https://github.com/ProjectLimitless/ProjectLimitless/issues/1
    /// </summary>
    public class MySQLDatabaseProvider : IModule, IDatabaseProvider
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;
        /// <summary>
        /// FluentData context.
        /// </summary>
        private IDbContext _dbContext;

        /// <summary>
        /// Constructor with log.
        /// </summary>
        /// <param name="log">The logger</param>
        public MySQLDatabaseProvider(ILogger log)
        {
            _log = log;
            _log.Debug("Created with log type '{0}'", _log.GetType());
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IModule.Configure(dynamic)"/>
        /// </summary>
        public void Configure(dynamic settings)
        {
            if (settings == null)
            {
                throw new NullReferenceException("Settings can not be null");
            }

            var config = (DatabaseProviderConfig)settings;
            if (config.ConnectionString == "")
            {
                throw new MissingFieldException("ConnectionString can not be blank");
            }

            _dbContext = new DbContext().ConnectionString(config.ConnectionString, new MySqlProvider());
            _log.Debug("Set up MySQL provider with connection string from configuration");
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IModule.GetConfigurationType"/>
        /// </summary>
        public Type GetConfigurationType()
        {
            return typeof(DatabaseProviderConfig);
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetTitle"/>
        /// </summary>
        public string GetTitle()
        {
            return "Limitless.MySQLDatabaseProvider";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetAuthor"/>
        /// </summary>
        public string GetAuthor()
        {
            var assembly = typeof(MySQLDatabaseProvider).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (attribute != null)
            {
                return attribute.Company;
            }
            return "Unknown";
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetVersion"/>
        /// </summary>
        public string GetVersion()
        {
            var assembly = typeof(MySQLDatabaseProvider).Assembly;
            return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetDescription"/>
        /// </summary>
        public string GetDescription()
        {
            return "A MySQL database provider for Project Limitless";
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.Delete(string, object[])"/>
        /// </summary>
        public int Delete(string sql, params object[] args)
        {
            return _dbContext.Sql(sql, args).Execute();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.Insert(string, object[])"/>
        /// </summary>
        public dynamic Insert(string sql, params object[] args)
        {
            return _dbContext.Sql(sql, args).ExecuteReturnLastId<dynamic>();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.Insert{T}(string, T)"/>
        /// </summary>
        public dynamic Insert<T>(string tableName, T value)
        {
            throw new NotImplementedException("Implement with Automap function");
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.QueryMany{T}(string)"/>
        /// </summary>
        public List<T> QueryMany<T>(string sql)
        {
            return _dbContext.Sql(sql).QueryMany<T>();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.QueryMany{T}(string, object[])"/>
        /// </summary>
        public List<T> QueryMany<T>(string sql, params object[] args)
        {
            return _dbContext.Sql(sql, args).QueryMany<T>();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.QuerySingle{T}(string)"/>
        /// </summary>
        public T QuerySingle<T>(string sql)
        {
            return _dbContext.Sql(sql).QuerySingle<T>();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.QuerySingle{T}(string, object[])"/>
        /// </summary>
        public T QuerySingle<T>(string sql, params object[] args)
        {
            return _dbContext.Sql(sql, args).QuerySingle<T>();
        }

        /// <summary>
        /// Implemented from interface
        /// <see cref="Limitless.Runtime.Interfaces.IDatabaseProvider.Update(string, object[])"/>
        /// </summary>
        public int Update(string sql, params object[] args)
        {
            return _dbContext.Sql(sql, args).Execute();
        }
    }
}
