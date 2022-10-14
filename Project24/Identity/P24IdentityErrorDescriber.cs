﻿/*  P24IdentityErrorDescriber.cs
 *  Version: 1.0 (2022.09.06)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project24.Identity
{
    public class P24IdentityErrorDescriber : IdentityErrorDescriber
    {
        public P24IdentityErrorDescriber()
        { }

        public override IdentityError DuplicateUserName(string _userName)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateUserName),
                Description = "Tài khoản " + _userName + " đã được sử dụng."
            };
        }


    }
}
