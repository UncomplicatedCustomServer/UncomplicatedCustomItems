using HarmonyLib;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine.Networking;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class APIRequest
    {
        public static IEnumerable<APIRequest> ActiveRequests = [];
        public static IReadOnlyList<APIRequest> RunningRequests = ActiveRequests.Where(r => r.Handle.IsRunning).ToList().AsReadOnly();

        public static void Cleanup()
        {
            foreach (APIRequest request in ActiveRequests.Where(r => !r.Handle.IsRunning).ToArray())
            {
                request.RemoveRequest();
            }
        }

        public void RemoveRequest()
        {
            List<APIRequest> requests = ActiveRequests.ToList();
            requests.Remove(this);
            ActiveRequests = requests;
        }

        public class RequestSettings
        {
            public float WaitTime;
            public bool Loop;
            public bool Cancel;
        }

        public enum RequestType
        {
            Get,
            Post,
            Put,
            Delete,
            Options,
        }

        public CoroutineHandle Handle;

        public virtual string Name => string.Empty;

        public virtual string Endpoint => string.Empty;

        public virtual string CustomEndpoint => string.Empty;

        public virtual RequestType Type => RequestType.Get;

        public virtual Dictionary<string, object> Payload { get; set; } = [];

        public virtual Dictionary<string, string> Headers { get; set; } = [];

        public virtual byte[] RawBody { get; set; } = [];

        public virtual RequestSettings Settings { get; set; } = new()
        {
            WaitTime = 0f,
            Loop = false,
            Cancel = false,
        };

        public virtual bool UseUCIEndpoint => true;
        
        public virtual bool UseCustomEndpoint { get; set; }

        public string DefaultEndpoint => $"https://api.ucserver.it/";

        public string UCIEndpoint => $"{DefaultEndpoint}v3/plugin/uci/{Endpoint}";

        public string UCSCEndpoint => $"{DefaultEndpoint}{Endpoint}";

        /// <summary>
        /// Resolves which endpoint to actually hit.
        /// Priority: CustomEndpoint > UCIEndpoint > UCSCEndpoint
        /// </summary>
        public string ResolvedEndpoint
        {
            get
            {
                if (UseCustomEndpoint && !string.IsNullOrWhiteSpace(CustomEndpoint))
                    return CustomEndpoint;

                return UseUCIEndpoint ? UCIEndpoint : UCSCEndpoint;
            }
        }

        public virtual void OnRequestCompleted(UnityWebRequest request) => LogManager.Debug($"Succeeded Sending request for {Name}.");
        public virtual void OnRequestFailed(UnityWebRequest request) => LogManager.Debug($"Failed to send request for {Name}.");

        public virtual void SendRequest(Action<UnityWebRequest> onComplete = null!)
        {
            ActiveRequests.AddItem(this);
            Handle = Timing.RunCoroutine(RequestCoroutine(OnRequestCompleted, OnRequestFailed, onComplete));
        }

        public IEnumerator<float> RequestCoroutine(Action<UnityWebRequest> onComplete, Action<UnityWebRequest> onFailed, Action<UnityWebRequest> action)
        {
            if (Settings.Loop)
            {
                while (!Settings.Cancel)
                {
                    yield return Timing.WaitForSeconds(Settings.WaitTime);
                    using UnityWebRequest looprequest = new(ResolvedEndpoint, Type.ToString().ToUpper());
                    looprequest.downloadHandler = new DownloadHandlerBuffer();

                    if ((Type == RequestType.Post || Type == RequestType.Put))
                    {
                        if (!Payload.IsEmpty())
                        {
                            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Payload));
                            looprequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        }
                        else if (!RawBody.IsEmpty())
                        {
                            looprequest.uploadHandler = new UploadHandlerRaw(RawBody);
                        }
                    }

                    foreach (KeyValuePair<string, string> kvp in Headers)
                    {
                        looprequest.SetRequestHeader(kvp.Key, kvp.Value);
                    }

                    yield return Timing.WaitUntilDone(looprequest.SendWebRequest());
                    if (looprequest.result == UnityWebRequest.Result.Success)
                    {
                        onComplete.Invoke(looprequest);
                        action.Invoke(looprequest);
                        RemoveRequest();
                    }
                    else
                    {
                        onFailed.Invoke(looprequest);
                        action.Invoke(looprequest);
                        RemoveRequest();
                    }
                }

                yield break;
            }

            if (Settings.WaitTime > 0f)
                yield return Timing.WaitForSeconds(Settings.WaitTime);

            using UnityWebRequest request = new(ResolvedEndpoint, Type.ToString().ToUpper());
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");

            if ((Type == RequestType.Post || Type == RequestType.Put))
            {
                if (!Payload.IsEmpty())
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Payload));
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                else if (!RawBody.IsEmpty())
                {
                    request.uploadHandler = new UploadHandlerRaw(RawBody);
                }
            }

            foreach (KeyValuePair<string, string> kvp in Headers)
            {
                request.SetRequestHeader(kvp.Key, kvp.Value);
            }

            yield return Timing.WaitUntilDone(request.SendWebRequest());
            if (request.result == UnityWebRequest.Result.Success)
            {
                onComplete.Invoke(request);
                action.Invoke(request);
                RemoveRequest();
            }
            else
            {
                onFailed.Invoke(request);
                action.Invoke(request);
                RemoveRequest();
            }
        }
    }
}
