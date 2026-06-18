using OutwardSoftcoreMode.Utility.Enums;
using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardSoftcoreMode.Events
{
    public static class EventBusPublisher
    {
        // Outward Game Settings mod has better examples.
        public const string Event_NameFromOtherMod = "GUID";

        //other mods listener uid
        public const string OtherMod_Listener = "GUID";

        public static void SendYourMessage(string yourVariable)
        {
            var payload = new EventPayload
            {
                [EventRegistryParamsHelper.Get(EventRegistryParams.PlaceHolder).key] = yourVariable
            };
            EventBus.Publish(OtherMod_Listener, Event_NameFromOtherMod, payload);
        }
    }
}
