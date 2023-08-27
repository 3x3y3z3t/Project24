/*  App/Services/InternalTrackedKeys.cs
 *  Version: v1.0 (2023.08.09)
 *  
 *  Contributor
 *      Arime-chan
 */


namespace Project24.App.Services
{
    public static class InternalTrackedKeys
    {
        public const string STATE_UPDATER_STATUS = nameof(STATE_UPDATER_STATUS);
        //public const string STATE_UPDATER_INTERNAL_STATE = nameof(STATE_UPDATER_INTERNAL_STATE);
        public const string STATE_UPDATER_QUEUED_ACTION = nameof(STATE_UPDATER_QUEUED_ACTION);
        public const string STATE_UPDATER_QUEUED_ACTION_DUE_TIME = nameof(STATE_UPDATER_QUEUED_ACTION_DUE_TIME);

        public const string CONFIG_UPDATER_WAIT_TIME = nameof(CONFIG_UPDATER_WAIT_TIME);
    }

}
