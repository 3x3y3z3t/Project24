/*  App/Services/InternalTracker/InternalTrackedKeys.cs
 *  Version: v1.1 (2023.09.01)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;

namespace Project24.App.Services
{
    public static class InternalTrackedKeys
    {
        public const string CONFIG_ = nameof(CONFIG_);
        public const string STATE_ = nameof(CONFIG_);

        public const string STATE_UPDATER_STATUS = nameof(STATE_UPDATER_STATUS);
        public const string STATE_UPDATER_QUEUED_ACTION = nameof(STATE_UPDATER_QUEUED_ACTION);
        public const string STATE_UPDATER_QUEUED_ACTION_DUE_TIME = nameof(STATE_UPDATER_QUEUED_ACTION_DUE_TIME);

        public const string CONFIG_GLOBAL_LOCALIZATION = nameof(CONFIG_GLOBAL_LOCALIZATION);
        public const string CONFIG_UPDATER_WAIT_TIME = nameof(CONFIG_UPDATER_WAIT_TIME);


        public static List<string> AllKeys = new()
        {
            STATE_UPDATER_STATUS,
            STATE_UPDATER_QUEUED_ACTION,
            STATE_UPDATER_QUEUED_ACTION_DUE_TIME,

            CONFIG_UPDATER_WAIT_TIME,
        };
    }

    public static class TrackableTabName
    {
        public const string TAB_GLOBAL = nameof(TAB_GLOBAL);

        public const string TAB_UPDATER = nameof(TAB_UPDATER);
        public const string TAB_CONFIG = nameof(TAB_CONFIG);
    }

}
