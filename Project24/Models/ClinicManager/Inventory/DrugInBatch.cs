/*  DrugInBatch.cs
 *  Version: 1.1 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;

namespace Project24.Models.Inventory.ClinicManager
{
    public class DrugInBatch : P24ObjectBase
    {
        public virtual ICollection<DrugInRecord> InRecords { get; protected set; }


        public P24IdentityUser ImportedUser { get { return AddedUser; } }


        public DrugInBatch()
            : base()
        { }

        public DrugInBatch(P24IdentityUser _addedUser)
            : base(_addedUser)
        { }
    }

}
