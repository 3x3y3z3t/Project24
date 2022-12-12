/*  P24IdentityUser.cs
 *  Version: 1.2 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models.Identity
{
    public class P24IdentityUser : IdentityUser
    {
        /* Frist Name includes Family and Middle name. */
        [StringLength(128, ErrorMessage = ErrorMessage.FirstNameTooLong)]
        public string FirstName { get; set; }

        [StringLength(16, ErrorMessage = ErrorMessage.FirstNameTooLong)]
        public string LastName { get; set; }

        public DateTime JoinDateTime { get; private set; }

        public virtual ICollection<UserUpload> Uploads { get; protected set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return "[null]";

                if (string.IsNullOrEmpty(LastName))
                    return FirstName;

                if (string.IsNullOrEmpty(FirstName))
                    return LastName;

                return FirstName + " " + LastName;
            }
        }


        public P24IdentityUser()
            : base()
        {
            JoinDateTime = DateTime.Now;
        }

        public P24IdentityUser(DateTime _joinDateTime)
        {
            JoinDateTime = _joinDateTime;
        }
    }

}
