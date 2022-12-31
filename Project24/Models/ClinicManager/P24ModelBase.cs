/*  P24ModelBase.cs
 *  Version: 1.1 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */

using Project24.Models.Identity;

namespace Project24.Models.ClinicManager
{
    public abstract class P24ModelBase : P24MutableObject
    {
        protected P24ModelBase()
            : base()
        { }

        protected P24ModelBase(P24IdentityUser _addedUser)
            : base(_addedUser)
        { }
    }

}
