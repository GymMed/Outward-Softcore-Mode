using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardSoftcoreMode.Utility.Enums
{
    // if you want even more less cluster you can use multiple enums instead of one
    public enum EventRegistryParams
    {
        CallerGUID,
        TryEnchantMenu,
        PlaceHolder,
    }

    public static class EventRegistryParamsHelper
    {
        private static readonly Dictionary<EventRegistryParams, (string key, Type type, string description)> _registry
            = new()
            {
                [EventRegistryParams.CallerGUID] = ("callerGUID", typeof(string), "Optional. Generated GUID to know who is publishing event."),
                [EventRegistryParams.TryEnchantMenu] = ("menu", typeof(EnchantmentMenu), "Optional. Generated GUID to know who is publishing event."),
                [EventRegistryParams.PlaceHolder] = ("placeHolder", typeof(object), "Optional. You would change this to your desired param."),
            };

        public static (string key, Type type, string description) Get(EventRegistryParams param) => _registry[param];

        public static (string key, Type type, string description)[] Combine(
            params object[] items)
        {
            var list = new List<(string key, Type type, string description)>();

            foreach (var item in items)
            {
                if (item is ValueTuple<string, Type, string> single)
                {
                    list.Add(single);
                }
                else if (item is ValueTuple<string, Type, string>[] array)
                {
                    list.AddRange(array);
                }
                else
                {
                    throw new ArgumentException(
                        $"Unsupported item type: {item?.GetType().FullName}");
                }
            }

            return list.ToArray();
        }
    }
}
