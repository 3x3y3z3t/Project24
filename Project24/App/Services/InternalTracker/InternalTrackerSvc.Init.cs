/*  App/Services/InternalTracker/InternalTrackerSvc.Init.cs
 *  Version: v1.0 (2023.09.02)
 *  
 *  Author
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Project24.Data;
using Project24.Model.Home;

namespace Project24.App.Services
{
    public sealed partial class InternalTrackerSvc
    {
        private bool InitializeTrackables(ApplicationDbContext _dbContext)
        {
            var trackables = (from _trackable in _dbContext.Trackables
                              select _trackable)
                             .ToList();

            foreach (var trackable in trackables)
                m_TrackedValues[trackable.Key] = trackable.Value;

            TryAddTrackable(InternalTrackedKeys.STATE_UPDATER_STATUS, UpdaterStatus.None);
            TryAddTrackable(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION, UpdaterQueuedAction.None);
            TryAddTrackable(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION_DUE_TIME, DateTime.MaxValue);

            TryAddTrackable(InternalTrackedKeys.CONFIG_GLOBAL_LOCALIZATION, P24Localization.VI_VN);
            TryAddTrackable(InternalTrackedKeys.CONFIG_UPDATER_WAIT_TIME, 2);

            return m_AddedValues.Count > 0;
        }

        private bool InitializeTrackableMetadatas(ApplicationDbContext _dbContext)
        {
            var metadatas = (from _metadata in _dbContext.InternalTrackableMetadatas
                             select _metadata)
                            .ToDictionary(_x => _x.ValueKey, _x => _x);

            List<TrackableMetadata> addedMetadata = new();

            #region Updater
            if (!metadatas.ContainsKey(InternalTrackedKeys.STATE_UPDATER_STATUS))
            {
                addedMetadata.Add(new(InternalTrackedKeys.STATE_UPDATER_STATUS)
                {
                    ValueDisplayTab = TrackableTabName.TAB_UPDATER,
                    ValueType = Trackable_ValueType.TRACKABLE_VALUE_TYPE_UNSET
                });
            }

            if (!metadatas.ContainsKey(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION))
            {
                addedMetadata.Add(new(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION)
                {
                    ValueDisplayTab = TrackableTabName.TAB_UPDATER,
                    ValueType = Trackable_ValueType.TRACKABLE_VALUE_TYPE_UNSET
                });
            }

            if (!metadatas.ContainsKey(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION_DUE_TIME))
            {
                addedMetadata.Add(new(InternalTrackedKeys.STATE_UPDATER_QUEUED_ACTION_DUE_TIME)
                {
                    ValueDisplayTab = TrackableTabName.TAB_UPDATER,
                    ValueType = Trackable_ValueType.TRACKABLE_VALUE_TYPE_UNSET
                });
            }
            #endregion

            #region Config
            if (!metadatas.ContainsKey(InternalTrackedKeys.CONFIG_UPDATER_WAIT_TIME))
            {
                addedMetadata.Add(new(InternalTrackedKeys.CONFIG_UPDATER_WAIT_TIME)
                {
                    ValueDisplayTab = TrackableTabName.TAB_CONFIG,
                    ValueType = Trackable_ValueType.TRACKABLE_VALUE_TYPE_INT32,
                    ValueRangeType = Trackable_ValueRangeType.TRACKABLE_VALUE_RANGE_TYPE_RANGE,
                    //ValueRangeAsString = JsonSerializer.Serialize(new string[] { "1", "30" }, P24JsonSerializerContext.Default.Array)
                    ValueRangeAsString = "[\"1\", \"30\"]"
                });
            }

            if (!metadatas.ContainsKey(InternalTrackedKeys.CONFIG_GLOBAL_LOCALIZATION))
            {
                addedMetadata.Add(new(InternalTrackedKeys.CONFIG_GLOBAL_LOCALIZATION)
                {
                    ValueDisplayTab = TrackableTabName.TAB_GLOBAL,
                    ValueType = Trackable_ValueType.TRACKABLE_VALUE_TYPE_SELECT,
                    ValueRangeType = Trackable_ValueRangeType.TRACKABLE_VALUE_RANGE_TYPE_LIST,
                    ValueRangeAsString = "[\"" + P24Localization.EN_US + "\", \"" + P24Localization.JA_JP + "\", \"" + P24Localization.VI_VN+ "\"]"
                });
            }
            #endregion

            if (addedMetadata.Count > 0)
            {
                _dbContext.AddRange(addedMetadata);
                return true;
            }

            return false;
        }

        private void TryAddTrackable<T>(string _key, T _value)
        {
            if (m_TrackedValues.ContainsKey(_key))
                return;

            m_TrackedValues[_key] = _value.ToString();
            m_AddedValues[_key] = _value.ToString();
        }

    }

}
