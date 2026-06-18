using OutwardSoftcoreMode.Utility.Enums;
using OutwardModsCommunicator.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardSoftcoreMode.Events
{
    public static class EventBusRegister
    {
        private static readonly (string key, Type type, string description)[] ExampleGroupParams =
        {
            EventRegistryParamsHelper.Get(EventRegistryParams.CallerGUID),
            EventRegistryParamsHelper.Get(EventRegistryParams.TryEnchantMenu),
        };

        public static void RegisterEvents()
        {
            // Let's register our listener event to provide what kind of parameters we expect for others to see
            // This is not required. Just helper for others and good practice.
            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusSubscriber.Event_ExecuteMyCode,
                "My code/method description",
                EventRegistryParamsHelper.Get(EventRegistryParams.CallerGUID)
            );

            /*
            // Example of grouping groups and single parameters for reusable cases
            EventBus.RegisterEvent(
                OutwardSoftcoreMode.EVENTS_LISTENER_GUID,
                EventBusSubscriber.Event_ExecuteMyCode,
                "My code/method description",
                EventRegistryParamsHelper.Combine(
                    EventRegistryParamsHelper.Get(EventRegistryParams.CallerGUID),
                    EventRegistryParamsHelper.Get(EventRegistryParams.TryEnchantMenu),
                    ExampleGroupParams
                )
            );
            */
        }
    }
}
