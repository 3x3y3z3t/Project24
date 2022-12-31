/*  DrugImportBatch.cs
 *  Version: 1.0 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Project24.Models.Identity;

namespace Project24.Models.Internal.ClinicManager
{
    public class DrugImportBatch : P24MutableObject
    {
        public virtual ICollection<DrugImportation> DrugImportation { get; protected set; }


        public P24IdentityUser ImportedUser { get { return AddedUser; } }


        public DrugImportBatch()
            : base()
        { }

        public DrugImportBatch(P24IdentityUser _addedUser)
            : base(_addedUser)
        { }
    }

}
