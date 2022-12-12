/*  UserUpload.cs
 *  Version: 1.0 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project24.App;
using Project24.Models.Identity;

namespace Project24.Models
{
    public class UserUpload
    {
        [Key]
        public int Id { get; protected set; }

        [ForeignKey("User")]
        public string UserId { get; protected set; }

        public DateTime UploadedDateTime { get; protected set; }

        public AppModule Module { get; protected set; }

        public int FilesCount { get; protected set; }

        public long BytesCount { get; protected set; }

        public virtual P24IdentityUser User { get; protected set; }


        public UserUpload()
        { }

        public UserUpload(P24IdentityUser _uploadedUser, AppModule _module, int _filesCount, long _bytesCount)
        {
            User = _uploadedUser;

            UploadedDateTime = DateTime.Now;
            Module = _module;
            FilesCount = _filesCount;
            BytesCount = _bytesCount;
        }
    }

}
