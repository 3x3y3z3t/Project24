/*  P24RoleClaimUtils.cs
 *  Version: 1.0 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;

namespace Project24.App.Utils
{

    public static class P24RoleClaimUtils
    {
        public enum Module : byte
        {
            Dashboard,
            P24_ClinicManager,
            P24b_Nas
        }

        public enum AccessAllowance : byte
        {
            NoAccess = 0,
            Restricted,
            FullAccess
        }

    public struct ModuleAccessAllowances
    {
            public AccessAllowance DashboardAccess;
            public AccessAllowance ClinicManagerAccess;
            public AccessAllowance NasAccess;

            public ModuleAccessAllowances(AccessAllowance _dashboardAccess, AccessAllowance _clinicManagerAccess, AccessAllowance _nasAccess)
            {
                DashboardAccess = _dashboardAccess;
                ClinicManagerAccess = _clinicManagerAccess;
                NasAccess = _nasAccess;
            }
    }

        public static ModuleAccessAllowances GetRoleAccessAllowance(string _role)
        {
            switch (_role)
            {
                case P24RoleName.Power:
                    return new ModuleAccessAllowances(AccessAllowance.FullAccess, AccessAllowance.FullAccess, AccessAllowance.FullAccess);
                case P24RoleName.Admin:
                    return new ModuleAccessAllowances(AccessAllowance.FullAccess, AccessAllowance.FullAccess, AccessAllowance.FullAccess);
                case P24RoleName.Manager:
                    return new ModuleAccessAllowances(AccessAllowance.NoAccess, AccessAllowance.FullAccess, AccessAllowance.NoAccess);
                case P24RoleName.NasUser:
                    return new ModuleAccessAllowances(AccessAllowance.NoAccess, AccessAllowance.NoAccess, AccessAllowance.Restricted);
            }

            return new ModuleAccessAllowances();
        }

        public static ModuleAccessAllowances GetHighestAccessAllowance(IList<string> _roles)
        {
            AccessAllowance dashboardAccess = AccessAllowance.NoAccess;
            AccessAllowance clinicManagerAccess = AccessAllowance.NoAccess;
            AccessAllowance nasAccess = AccessAllowance.NoAccess;

            foreach (string role in _roles)
            {
                var access = GetRoleAccessAllowance(role);

                if (dashboardAccess < access.DashboardAccess)
                    dashboardAccess = access.DashboardAccess;

                if (clinicManagerAccess < access.ClinicManagerAccess)
                    clinicManagerAccess = access.ClinicManagerAccess;

                if (nasAccess < access.NasAccess)
                    nasAccess = access.NasAccess;
            }

            return new ModuleAccessAllowances(dashboardAccess, clinicManagerAccess, nasAccess);
        }





    }

}
