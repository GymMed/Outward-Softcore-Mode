using OutwardModsCommunicator.EventBus;
using System.Collections.Generic;

namespace OutwardSoftcoreMode.Events
{
    public static class EventBusPublisher
    {
        public static void PublishSaveBackupBefore(string callerUID)
        {
            var payload = new EventPayload
            {
                [EventBusKeys.GetParamName(EventParam.CallerUID)] = callerUID
            };
            EventBus.Publish(OutwardSoftcoreMode.EVENTS_LISTENER_GUID, EventBusKeys.GetEventName(EventName.SaveBackupBefore), payload);
        }

        public static void PublishSaveBackupAfter(string callerUID)
        {
            var payload = new EventPayload
            {
                [EventBusKeys.GetParamName(EventParam.CallerUID)] = callerUID
            };
            EventBus.Publish(OutwardSoftcoreMode.EVENTS_LISTENER_GUID, EventBusKeys.GetEventName(EventName.SaveBackupAfter), payload);
        }

        public static void PublishDeathRollBefore(List<string> softcoreUIDs)
        {
            var payload = new EventPayload
            {
                [EventBusKeys.GetParamName(EventParam.SoftcoreUIDs)] = softcoreUIDs
            };
            EventBus.Publish(OutwardSoftcoreMode.EVENTS_LISTENER_GUID, EventBusKeys.GetEventName(EventName.DeathRollBefore), payload);
        }

        public static void PublishDeathRollAfter(bool rollResult, List<string> affectedUIDs)
        {
            var payload = new EventPayload
            {
                [EventBusKeys.GetParamName(EventParam.RollResult)] = rollResult,
                [EventBusKeys.GetParamName(EventParam.AffectedUIDs)] = affectedUIDs
            };
            EventBus.Publish(OutwardSoftcoreMode.EVENTS_LISTENER_GUID, EventBusKeys.GetEventName(EventName.DeathRollAfter), payload);
        }
    }
}
