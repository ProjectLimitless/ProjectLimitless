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

using Limitless.Runtime.Types;
using Limitless.Runtime.Interfaces;

namespace Limitless.Builtin
{
    /// <summary>
    /// TODO: Move this
    /// </summary>
    public class JwtToken
    {
        public string sub;
        public long exp;
    }

    public class MyUser : BaseUser
    {
        public string IDNumber { get; set; }

        public MyUser(string username) : base(username)
        {
            
        }
    }

    internal class LocalIdentityProvider : IModule, IIdentityProvider
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Standard constructor with log.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public LocalIdentityProvider(ILogger log)
        {
            _log = log;
        }

        //TODO: Move this module to a separate assembly?
        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.Configure"/>
        /// </summary>
        public void Configure(dynamic settings)
        {
            // Nothing to do
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IModule.GetConfigurationType"/>
        /// </summary>
        public Type GetConfigurationType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IIdentityProvider.ValidateToken"/>
        /// </summary>
        public BaseUser ValidateToken(string token)
        {
            MyUser user = new MyUser("usertest");
            user.IDNumber = "MYIDNUMBERSSS";
            try
            {
                var payload = Jose.JWT.Decode<JwtToken>(token, "mysecretkey");
                var tokenExpires = DateTime.FromBinary(payload.exp);
                if (tokenExpires > DateTime.UtcNow)
                {
                    // CoreContainer.Instance.IdentityProvider.Handler?
                    // TODO: This function must come from IIdentityProvider and return a wrappable user object
                    return user;
                }
            }
            catch (Exception)
            {
                return user;
            }

            return user;
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IIdentityProvider.Login"/>
        /// </summary>
        public BaseUser Login(string username, string password)
        {
            // TODO: Continue here


            // TODO: Find a clean way to handle required parameters
            if (username == string.Empty || username == null)
            {
                throw new MissingFieldException("Username must not be blank");
            }
            if (password == string.Empty || password == null)
            {
                throw new MissingFieldException("password must not be blank");
            }

            // Testing
            if (password != "demopass")
            {
                throw new UnauthorizedAccessException("Username or password is incorrect");
            }

            BaseUser user = new BaseUser(username);
            user.Name = "Ass";
            return user;
        }
    }
}
