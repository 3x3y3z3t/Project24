/*  NasStatDisplayModel.cs
 *  Version: 1.0 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

using Project24.App;

namespace Project24.Models.Nas
{
    public class NasStatDisplayModel
    {
        public string Name { get; set; }
        public DriveUtils Data { get; set; }

        public NasStatDisplayModel()
        { }
    }

}
