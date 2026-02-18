using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using LabApi.Features.Wrappers;
using UncomplicatedCustomItems.API.Extensions;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest;

namespace UncomplicatedCustomItems.API.Components
{
    public class Presence : MonoBehaviour
    {
        public string Endpoint = "https://ucipluginapi.thaumiel-servers.workers.dev";
        public uint Interval;
        public DateTime LastUpload;
        public bool LastUploadSucceeded;
        public uint MaxFailCount = 1;
        public uint FailCount = 0;
        public bool hasExiled;
        public bool CheckedForExiledPlugins;
        public List<string> pluginNames = [];

        public void Init(uint interval, uint maxFailCount)
        {
            Interval = interval;
            MaxFailCount = maxFailCount;
        }

        private void Update()
        {
            if (LastUpload == default)
            {
                LastUpload = DateTime.Now;
                SendPresence();
            }
            else
            {
                TimeSpan timeSinceLastUpload = DateTime.Now - LastUpload;
                if (timeSinceLastUpload.TotalSeconds >= Interval)
                {
                    LastUpload = DateTime.Now;
                    SendPresence();
                }
            }
        }

        public void SendPresence()
        {
            StartCoroutine(SendPresenceCoroutine(result => 
            {
                LastUploadSucceeded = result;
            }));
        }

        private IEnumerator SendPresenceCoroutine(Action<bool> onComplete)
        {
            UnityWebRequest request = new($"{Endpoint}/connect", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(GetJsonPayload());
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool success = false;
            switch (request.result)
            {
                case Result.ConnectionError:
                case Result.ProtocolError:
                case Result.DataProcessingError:
                    if (FailCount < MaxFailCount)
                    {
                        LogManager.Error($"UCI Presence has failed to send: {FailCount}/{MaxFailCount} \n Error: {request.error}");
                        FailCount++;
                    }
                    else
                    {
                        LogManager.Warn($"UCI Presence has failed to send: {FailCount}/{MaxFailCount} stopping automatic uploads.");
                        Destroy(this);
                    }
                    break;
                
                case Result.Success:
                    LogManager.Debug($"Presence posted to {Endpoint} - status {(int)request.responseCode}.");
                    FailCount = 0;
                    success = true;
                    break;
            }

            request.Dispose();
            onComplete?.Invoke(success);
        }

        private string GetJsonPayload()
        {
            try
            {
                Type loaderType = Type.GetType("Exiled.Loader.Loader, Exiled.Loader");
                if (loaderType != null && !CheckedForExiledPlugins)
                {
                    CheckedForExiledPlugins = true;
                    PropertyInfo pluginsProperty = loaderType.GetProperty("Plugins", BindingFlags.Public | BindingFlags.Static);
                    if (pluginsProperty != null)
                    {
                        if (pluginsProperty.GetValue(null) is IEnumerable plugins)
                        {
                            foreach (object plugin in plugins)
                            {
                                PropertyInfo nameProperty = plugin.GetType().GetProperty("Name");
                                if (nameProperty != null)
                                {
                                    string name = nameProperty.GetValue(plugin) as string;
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        if (name.StartsWith("Exiled", StringComparison.OrdinalIgnoreCase))
                                        {                                            
                                            hasExiled = true;
                                        }
                                        else
                                            pluginNames.TryAdd(name);
                                    }
                                }
                            }
                        }
                    }
                }

                LabApi.Loader.PluginLoader.EnabledPlugins.ToArray().ForEach(p =>
                {
                    if (!p.Name.StartsWith("Exiled", StringComparison.OrdinalIgnoreCase))
                    {
                        pluginNames.TryAdd(p.Name);                        
                    }
                    else
                        hasExiled = true;
                });

                if (hasExiled)
                    pluginNames.Add("Exiled");

                HashSet<string> ucsPlugins = new(StringComparer.OrdinalIgnoreCase)
                {
                    "uncomplicatedcustomitems",
                    "uncomplicatedcustomroles",
                    "uncomplicatedcustomescapezones",
                    "uncomplicatedcustomteams",
                    "uncomplicatedcustombots"
                };

                Dictionary<string, object> payload = new()
                {
                    ["serverName"] = Server.ServerListName,
                    ["pluginVersion"] = Plugin.Instance.Version.ToString(3) ?? "unknown",
                    ["serverPort"] = Server.Port,
                    ["serverIp"] = Server.IpAddress,
                    ["hideIP"] = Plugin.Instance.Config.HideipOnList.ToString(),
                    ["scpslVersion"] = GameCore.Version.VersionString,
                    ["showOnList"] = Plugin.Instance.Config.ShowOnuciList.ToString(),
                    ["exiled"] = hasExiled.ToString().ToLower(),
                    ["extra"] = $"PlayerCount: {Player.List.RealList().Count()}, MaxPlayers: {Server.MaxPlayers}, Idling: {Server.IdleModeActive}, EnabledCreditTags: {Plugin.Instance.Config.EnableCreditTags}",
                    ["plugins"] = pluginNames.Where(ucsPlugins.Contains).ToList()
                };

                if (Plugin.Instance.Config.ShowPluginsOnList)
                    payload["plugins"] = pluginNames;

                string json = JsonSerializer.Serialize(payload);
                return json;
            }
            catch (Exception ex)
            {
                LogManager.Error($"Unexpected error getting json data for presence upload: {ex.GetType().FullName}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}