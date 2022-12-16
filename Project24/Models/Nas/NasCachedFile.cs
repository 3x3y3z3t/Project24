/*  NasCachedFile.cshtml
 *  Version: 1.1 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models.Nas
{
    public class NasCachedFile
    {
        [Key]
        public int Id { get; set; }
        public DateTime AddedDate { get; set; }
        public int FailCount { get; set; }
        
        public string Path { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public DateTime LastModDate { get; set; }


        public NasCachedFile()
        { }
    }

}
