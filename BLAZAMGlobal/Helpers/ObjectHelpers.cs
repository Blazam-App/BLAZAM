using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class ObjectHelpers
    {
        public static string? ToJson(this object? obj)
        {
            if (obj == null) return null;
            var _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());

            return JsonSerializer.Serialize(obj);
        }
    }
}
