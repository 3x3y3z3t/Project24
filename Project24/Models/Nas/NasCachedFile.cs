/*  NasCachedFile.cshtml
 *  Version: 1.0 (2022.10.27)
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
        
        public string Path { get; set; }
        public string Name { get; set; }

        public DateTime AddedDate { get; set; }


        public NasCachedFile()
        { }
    }

}
