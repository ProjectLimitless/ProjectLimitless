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
    internal class UserManager : IModule, IUserManager
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger _log;

        /// <summary>
        /// Standard constructor with log.
        /// </summary>
        /// <param name="log">The logger to use</param>
        public UserManager(ILogger log)
        {
            _log = log;
        }

        //TODO: Move this module to a separate assembly?
        public void Configure(dynamic settings)
        {
            // Nothing to do
        }

        public Type GetConfigurationType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implemented from interface 
        /// <see cref="Limitless.Runtime.Interface.IUserManager.Login"/>
        /// </summary>
        public BaseUser Login(string username, string password)
        {
            // TODO: UserManager should call IdentityProvider as implemented in a seperate module
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
            user.FirstName = "Ass";
            return user;
        }
    }
}
