/*  Model/Home/UserUploadData.cs
 *  Version: v1.0 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Model
{
    public class UserUploadData
    {
        [Key]
        public long Id { get; protected set; }

        [Required]
        public string Username { get; private set; }

        [Required]
        public short TotalFilesCount { get; protected set; }
        [Required]
        public int TotalFilesSize { get; private set; }
        [Required]
        public string ListFiles { get; private set; }

        [Required]
        public DateTime AddedDate { get; protected set; }


        public UserUploadData()
        { }

        public UserUploadData(string _uploadedUsername, short _filesCount, int _filesSize, string _listFilesAsString)
        {
            Username = _uploadedUsername;

            TotalFilesCount = _filesCount;
            TotalFilesSize = _filesSize;
            ListFiles = _listFilesAsString;

            AddedDate = DateTime.Now;
        }
    }

}
