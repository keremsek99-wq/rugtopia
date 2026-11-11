using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Rugtopia.Meta
{
    /// <summary>Pluggable analytics. The default sink prints one JSON object per event (visible in the WebGL console).</summary>
    public interface IAnalyticsSink
    {
        void Track(string name, IReadOnlyDictionary<string, string> props);
    }

    public static class Analytics
    {
        public static IAnalyticsSink Sink = new LogAnalytics();

        public static void Track(string name, params (string key, string value)[] props)
        {
            var map = new Dictionary<string, string>();
            if (props != null)
                for (int i = 0; i < props.Length; i++) map[props[i].key] = props[i].value ?? "";
            if (Sink != null) Sink.Track(name, map);
        }

        public sealed class LogAnalytics : IAnalyticsSink
        {
            public void Track(string name, IReadOnlyDictionary<string, string> props)
            {
                var sb = new StringBuilder();
                sb.Append("{\"event\":\"").Append(name).Append('"');
                if (props != null)
                    foreach (var kv in props)
                        sb.Append(",\"").Append(kv.Key).Append("\":\"").Append(kv.Value).Append('"');
                sb.Append('}');
                Debug.Log(sb.ToString());
            }
        }
    }
}
