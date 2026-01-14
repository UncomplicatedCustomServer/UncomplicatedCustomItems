using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using UnityEngine.Networking;
using Logger = LabApi.Features.Console.Logger;

namespace UncomplicatedCustomItems.API.Components
{
    public class AdminMessageWatcher : MonoBehaviour
    {
        private class AdminMessage
        {
            [JsonPropertyName("guid")]
            public Guid ID { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        private string _url = "https://ucscadminmsg.thaumiel-servers.workers.dev/";
        private List<Guid> respondedtoids = [];
        private DateTime _lastattempt;
        private float _waittime = 10f;

        private void Update()
        {
            if (!Plugin.Instance.Config.DoEnableAdminMessages)
                Destroy(this);

            if (_lastattempt == default)
                _lastattempt = DateTime.Now;

            if ((DateTime.Now - _lastattempt).TotalSeconds > _waittime)
            {
                _lastattempt = DateTime.Now;
                StartCoroutine(FetchData());
            }
        }

        private IEnumerator FetchData()
        {
            using UnityWebRequest request = UnityWebRequest.Get(_url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                AdminMessage msg = JsonSerializer.Deserialize<AdminMessage>(jsonResponse);
                if (!respondedtoids.Contains(msg.ID))
                {
                    Logger.Raw($"[UCSC Admin Message] {msg.Message}", ConsoleColor.DarkYellow);
                    respondedtoids.Add(msg.ID);
                    LogManager.Debug($"Request successful: {jsonResponse}");
                }
            }
            else
            {
                LogManager.Error($"Request failed: {request.error}");
            }
        }
    }
}