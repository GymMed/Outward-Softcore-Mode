using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;

namespace OutwardSoftcoreMode.Events
{
    public static class EventBusRegister
    {
        public static void RegisterEvents()
        {
            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusKeys.GetEventName(EventName.SaveBackupBefore),
                "Fired before a manual backup is created.",
                (EventBusKeys.GetParamName(EventParam.CallerUID), typeof(string), "The UID of the character whose save triggered the backup.")
            );

            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusKeys.GetEventName(EventName.SaveBackupAfter),
                "Fired after a manual backup is created.",
                (EventBusKeys.GetParamName(EventParam.CallerUID), typeof(string), "The UID of the character whose save was backed up.")
            );

            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusKeys.GetEventName(EventName.DeathRollBefore),
                "Fired before the death roll is made for softcore characters.",
                (EventBusKeys.GetParamName(EventParam.SoftcoreUIDs), typeof(List<string>), "List of UIDs for all softcore characters at 0 HP.")
            );

            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusKeys.GetEventName(EventName.DeathRollAfter),
                "Fired after the death roll is made for softcore characters.",
                (EventBusKeys.GetParamName(EventParam.RollResult), typeof(bool), "True if death was triggered, false if survived."),
                (EventBusKeys.GetParamName(EventParam.AffectedUIDs), typeof(List<string>), "List of UIDs whose death count was incremented (empty if survived).")
            );
        }
    }
}
