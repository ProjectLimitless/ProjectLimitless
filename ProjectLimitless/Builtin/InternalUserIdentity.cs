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
using System.Collections.Generic;

using Nancy.Security;

using Limitless.Runtime.Types;

namespace Limitless.Builtin
{
    /// <summary>
    /// Wraps the IIdentityProvider's user class in a
    /// Nancy IUserIdentity-compatible format for internal use.
    /// </summary>
    internal class InternalUserIdentity : IUserIdentity
    {
        /// <summary>
        /// Implemented from interface
        /// <see cref="Nancy.Security.IUserIdentity.Claims"/>
        /// </summary>
        public IEnumerable<string> Claims { get; internal set; }
        /// <summary>
        /// Implemented from interface
        /// <see cref="Nancy.Security.IUserIdentity.UserName"/>
        /// </summary>
        public string UserName { get; internal set; }
        /// <summary>
        /// Holds the original user object as provided by the user
        /// implementation for the loaded IIdentityProvider.
        /// </summary>
        public dynamic Meta { get; internal set; }

        /// <summary>
        /// Wraps the provided baseUser into a Nancy Identity
        /// for internal use.
        /// </summary>
        /// <param name="baseUser">The original BaseUser</param>
        /// <returns>The wrapped internal user</returns>
        public static InternalUserIdentity Wrap(BaseUser baseUser)
        {
            InternalUserIdentity internalUser = new InternalUserIdentity();
            if (baseUser == null)
            {
                return null;
            }

            internalUser.UserName = baseUser.UserName;
            internalUser.Claims = baseUser.Claims;
            internalUser.Meta = baseUser;
            return internalUser;
        }
    }
}
