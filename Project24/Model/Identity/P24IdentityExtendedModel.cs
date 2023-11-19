/*  Model/Identity/P24IdentityExtendedModel.cs
 *  Version: v1.1 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System;
using System.Text.Json.Serialization;

namespace Project24.Model.Identity
{
    /// <summary>
    ///     Project24's customized <see cref="IdentityUser"/>, which contains extra information about the user.<br />
    ///     <c>P24IdentityUser</c> is also a syncable object (see <see cref="ISyncable"/>).
    /// </summary>
    public class P24IdentityUser : IdentityUser, ISyncable
    {
        /* Frist Name includes Family and Middle name. */
        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(16)]
        public string LastName { get; set; }

        public string PreferredLocale { get; set; }

        public string Status { get; set; }

        public DateTime AddedDateTime { get; set; } = DateTime.Now;
        public DateTime RemovedDateTime { get; set; } = DateTime.MaxValue;


        [JsonIgnore]
        public string FullName
        {
            get
            {
                string name = "";

                if (!string.IsNullOrWhiteSpace(LastName))
                    name += LastName.ToUpper();

                if (!string.IsNullOrWhiteSpace(FirstName))
                {
                    if (name != "")
                        name += ", ";
                    name += FirstName;
                }

                if (name == "")
                    return "[null]";

                return name;
            }
        }

        public ulong Version { get => m_Version; set => m_Version = value; }
        public string Hash { get => m_Hash; set => m_Hash = value; }


        public P24IdentityUser()
        { }

        public P24IdentityUser(DateTime _addedDateTime)
        {
            AddedDateTime = _addedDateTime;
        }


        public bool VersionUp() => Syncable.VersionUp(ref m_Version, ref m_Hash, GetFieldsValuesConcatenatedInternal());

        protected string GetFieldsValuesConcatenatedInternal() => ""
            + Id
            + UserName
            + Email
            + EmailConfirmed
            + PasswordHash
            + SecurityStamp
            + ConcurrencyStamp
            + PhoneNumber
            + PhoneNumberConfirmed
            + TwoFactorEnabled
            + LockoutEnd
            + LockoutEnabled
            + AccessFailedCount
            + FirstName
            + LastName
            + PreferredLocale
            + Status
            + AddedDateTime
            + RemovedDateTime
            ;


        private ulong m_Version = 0UL;
        private string m_Hash = null;
    }


    /// <summary>
    ///     Project24's customized <see cref="IdentityRole"/>, which contains extra information about the role.<br />
    ///     <c>P24IdentityRole</c> is also a syncable object (see <see cref="ISyncable"/>).
    /// </summary>
    public class P24IdentityRole : IdentityRole, ISyncable
    {
        public ulong Version { get => m_Version; set => m_Version = value; }
        public string Hash { get => m_Hash; set => m_Hash = value; }


        public P24IdentityRole()
        { }

        public P24IdentityRole(string _roleName)
            : base(_roleName)
        { }


        public bool VersionUp() => Syncable.VersionUp(ref m_Version, ref m_Hash, GetFieldsValuesConcatenatedInternal());

        protected string GetFieldsValuesConcatenatedInternal() => ""
            + Id
            + Name
            + ConcurrencyStamp
            ;


        private ulong m_Version = 0UL;
        private string m_Hash = null;
    }

}
