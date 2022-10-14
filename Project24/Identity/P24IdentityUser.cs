/*  P24IdentityUser.cs
 *  Version: 1.0 (2022.09.06)
 *
 *  Contributor
 *      Arime-chan
 */
using Microsoft.AspNetCore.Identity;
using System;

namespace Project24.Identity
{
    public class P24IdentityUser : IdentityUser
    {
        public enum AttendanceStatus {
            Unset = 0,
            Absent,
            AttendanceRequested,
            AttendanceRejected,
            Present,
        }

        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime JoinDateTime { get; set; }
        public DateTime LeaveDateTime { get; set; }
        public string AttendanceProfileId { get; set; }

        public P24IdentityUser()
            : base()
        {

        }

        public P24IdentityUser(string _username)
            : base(_username)
        {

        }

        //public string GetDisplayName()
        //{
        //    if (string.IsNullOrEmpty(MiddleName))
        //    {
        //        return FamilyName + " " + LastName;
        //    }

        //    return FamilyName + " " + MiddleName + " " + LastName;
        //}



    }
}
