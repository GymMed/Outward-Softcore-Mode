using System;
using System.Collections.Generic;

namespace OutwardSoftcoreMode.Events
{
    public enum EventName
    {
        SaveBackupBefore,
        SaveBackupAfter,
        DeathRollBefore,
        DeathRollAfter
    }

    public enum EventParam
    {
        CallerUID,
        SoftcoreUIDs,
        RollResult,
        AffectedUIDs
    }

    public static class EventBusKeys
    {
        private static readonly Dictionary<EventParam, string> ParamNames = new()
        {
            [EventParam.CallerUID] = "callerUID",
            [EventParam.SoftcoreUIDs] = "softcoreUIDs",
            [EventParam.RollResult] = "rollResult",
            [EventParam.AffectedUIDs] = "affectedUIDs",
        };

        public static string GetEventName(EventName name) => name.ToString();

        public static string GetParamName(EventParam param) =>
            ParamNames.TryGetValue(param, out string value) ? value : throw new ArgumentOutOfRangeException(nameof(param));
    }
}
