using HarmonyLib;
using LabApi.Loader;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules;
using UncomplicatedCustomItems.API.Features;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;
using LabPlugin = LabApi.Loader.Features.Plugins.Plugin;

namespace UncomplicatedCustomItems.Integrations
{
    public class AudioIntegration
    {
        public static Assembly? SLNAudio;
        public static Assembly? AudioPlayerAPI;

        public static Type? APIAudioClipStorage;
        public static Type? APIAudioPlayer;

        public static Type? SLNAudioPlayer;
        public static Type? SLNSpeakerSettings;
        public static Type? SLNAudioPlayerExtensions;

        public static MethodInfo? APILoadClip;
        public static MethodInfo? APICreateOrGet;
        public static MethodInfo? APIAddSpeaker;
        public static MethodInfo? APIAddClip;

        public static MethodInfo? SLNCreate;
        public static MethodInfo? SLNUseFile;

        public static bool FoundAny => FoundSLNAudio || FoundAudioAPI;
        public static bool FoundSLNAudio => SLNAudio != null;
        public static bool FoundAudioAPI => AudioPlayerAPI != null;

        public static void Init()
        {
            if (FoundAny)
                return;

            Task.Run(() =>
            {
                GetAudioPlayerAPI();
                GetSLNAudio();
            });
        }

        public static void Play(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            if (!FoundAny)
            {
                LogManager.Warn("No supported audio plugin found! Install either AudioPlayerApi or SecretLabNAudio to use the custom audio custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                return;
            }

            if (FoundSLNAudio)
            {
                PlayAudioSLN(CustomItem, Coords);
                return;
            }

            if (FoundAudioAPI)
            {
                PlayAudioPlayerAPI(CustomItem, Coords);
                return;
            }

        }

        public static void GetAudioPlayerAPI()
        {
            AudioPlayerAPI = PluginLoader.Dependencies.FirstOrDefault(asm => asm.FullName.StartsWith("AudioPlayerApi"));
            if (AudioPlayerAPI == null)
                return;

            APIAudioClipStorage = AudioPlayerAPI.GetType("AudioClipStorage");
            APIAudioPlayer = AudioPlayerAPI.GetType("AudioPlayer");

            if (APIAudioClipStorage == null || APIAudioPlayer == null)
                return;

            APILoadClip = APIAudioClipStorage.GetMethod("LoadClip", [typeof(string), typeof(string)]);

            APIAddSpeaker = APIAudioPlayer.GetMethod("AddSpeaker", [typeof(string), typeof(Vector3), typeof(float), typeof(bool), typeof(float), typeof(float)]);
            APIAddClip = APIAudioPlayer.GetMethod("AddClip", [typeof(string), typeof(float), typeof(bool), typeof(bool)]);

            APICreateOrGet = APIAudioPlayer.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name == "CreateOrGet");
        }

        private static void PlayAudioPlayerAPI(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            if (!CustomItem.TryGetModule<CustomAudio>(out var data) || data == null)
            {
                LogManager.Warn($"CustomAudio Module not found on {CustomItem.CustomItem.Name}!");
                return;
            }

            if (string.IsNullOrEmpty(data.AudioPath))
            {
                LogManager.Warn("Audio path is null.");
                return;
            }

            if (!File.Exists(data.AudioPath))
            {
                LogManager.Warn($"Audio file not found: '{data.AudioPath}'.");
                return;
            }

            string ext = Path.GetExtension(data.AudioPath).ToLowerInvariant();
            if (ext != ".ogg")
            {
                LogManager.Warn($"Unsupported format '{ext}' — AudioPlayerApi requires .ogg.");
                return;
            }

            if (APILoadClip == null || APICreateOrGet == null || APIAddSpeaker == null || APIAddClip == null)
            {
                LogManager.Warn("AudioPlayerApi method resolution failed — see previous debug output.");
                return;
            }

            string clipId = $"sound_{Guid.NewGuid()}";

            ParameterExpression playerParam = Expression.Parameter(APIAudioPlayer, "p");
            MethodCallExpression callAddSpeaker = Expression.Call(
                playerParam,
                APIAddSpeaker,
                Expression.Constant("Main"),
                Expression.Constant(Coords),
                Expression.Constant(1f),
                Expression.Constant(true),
                Expression.Constant(data.MinAudibleDistance),
                Expression.Constant(data.MaxAudibleDistance)
            );

            object? player = APICreateOrGet.Invoke(null, [
                $"UCI_Audio_{Guid.NewGuid()}",
                null,
                null,
                true,
                true,
                null,
                (byte)255,
                Expression.Lambda(typeof(Action<>).MakeGenericType(APIAudioPlayer), callAddSpeaker, playerParam).Compile(),
                null
            ]);

            if (player == null)
            {
                LogManager.Warn("Failed to create AudioPlayer!");
                return;
            }

            APIAddClip.Invoke(player, [clipId, Clamp(data.Volume, 0f, 100f), false, true]);
            if ((bool)(APILoadClip.Invoke(null, [data.AudioPath, clipId]) ?? false))
            {
                LogManager.Debug($"Playing '{Path.GetFileName(data.AudioPath)}' via AudioPlayerApi at {Coords}");
            }
            else
                LogManager.Warn($"Failed to load audio file '{Path.GetFileName(data.AudioPath)}'");
        }

        private static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(value, max))/100;

        private static void GetSLNAudio()
        {
            SLNAudio = PluginLoader.Plugins.FirstOrDefault(p => p.Key.Name == "SecretLabNAudio").Value;
            if (SLNAudio == null)
                return;

            SLNAudioPlayer = AccessTools.TypeByName("SecretLabNAudio.Core.AudioPlayer");
            SLNSpeakerSettings = AccessTools.TypeByName("SecretLabNAudio.Core.SpeakerSettings");
            SLNAudioPlayerExtensions = AccessTools.TypeByName("SecretLabNAudio.Core.Extensions.AudioPlayerExtensions");

            if (SLNAudioPlayer == null || SLNSpeakerSettings == null)
                return;

            SLNCreate = SLNAudioPlayer.GetMethod("Create", [typeof(byte), SLNSpeakerSettings, typeof(Transform), typeof(Vector3), typeof(bool)]);

            if (SLNAudioPlayerExtensions != null)
            {
                SLNUseFile = SLNAudioPlayerExtensions.GetMethod("UseFile", [SLNAudioPlayer, typeof(string), typeof(bool), typeof(float)]) ?? SLNAudioPlayerExtensions
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "UseFile" &&
                        m.GetParameters() is { Length: 4 } ps &&
                        ps[1].ParameterType == typeof(string) &&
                        ps[2].ParameterType == typeof(bool) &&
                        ps[3].ParameterType == typeof(float));

                if (SLNUseFile == null)
                    LogManager.Warn("SecretLabNAudio: Could not resolve UseFile. API shape may have changed.");
            }
        }

        public static void PlayAudioSLN(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            try
            {
                if (!CustomItem.TryGetModule<CustomAudio>(out var data) || data == null)
                {
                    LogManager.Warn($"CustomAudio Module not found on {CustomItem.CustomItem.Name}!");
                    return;
                }

                if (string.IsNullOrEmpty(data.AudioPath))
                {
                    LogManager.Warn("Audio path is null. please fill out the config properly.");
                    return;
                }

                if (!File.Exists(data.AudioPath))
                {
                    LogManager.Warn($"Audio file not found: '{data.AudioPath}'. check your config path.");
                    return;
                }

                string ext = Path.GetExtension(data.AudioPath).ToLowerInvariant();
                if (ext != ".ogg")
                {
                    LogManager.Warn($"Unsupported audio format '{ext}' for file '{Path.GetFileName(data.AudioPath)}'. SecretLabNAudio only supports .ogg files. ");
                    return;
                }

                if (!FoundSLNAudio || SLNCreate == null || SLNUseFile == null || SLNSpeakerSettings == null)
                {
                    LogManager.Warn("SecretLabNAudio is not loaded or required types/methods were not found.");
                    LogManager.Debug($"{FoundSLNAudio} - {SLNCreate != null} - {SLNUseFile != null} - {SLNSpeakerSettings != null}");
                    return;
                }

                if (string.IsNullOrEmpty(data.AudioPath))
                {
                    LogManager.Warn($"Audio path is null please fill out the config properly.");
                    return;
                }

                PropertyInfo? defaultProp = SLNSpeakerSettings.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                object settings = defaultProp?.GetValue(null) ?? Activator.CreateInstance(SLNSpeakerSettings)!;

                void SetProp(string name, object value)
                {
                    if (SLNSpeakerSettings == null)
                        return;

                    PropertyInfo prop = SLNSpeakerSettings.GetProperty(name);
                    MethodInfo? setter = prop?.GetSetMethod(nonPublic: true);
                    if (setter != null)
                    {
                        object boxed = settings;
                        setter.Invoke(boxed, [value]);
                        settings = boxed;
                    }
                }

                SetProp("IsSpatial", true);
                SetProp("Volume", Clamp(data.Volume, 0f, 100f));
                SetProp("MinDistance", data.MinAudibleDistance);
                SetProp("MaxDistance", data.MaxAudibleDistance);

                object? player = SLNCreate.Invoke(null, [(byte)1, settings, null, Coords, true]);
                if (player == null)
                {
                    LogManager.Warn("Failed to create SecretLabNAudio AudioPlayer!");
                    return;
                }

                SLNUseFile.Invoke(null, [player, data.AudioPath, false, data.Volume]);

                LogManager.Debug($"Playing {Path.GetFileName(data.AudioPath)} via SecretLabNAudio at {Coords}");
            }
            catch (TargetInvocationException tie)
            {
                LogManager.Error($"PlayAudioSLN reflection invoke failed.\n Inner: {tie.InnerException?.GetType().Name}: {tie.InnerException?.Message}\n {tie.InnerException?.StackTrace}");
            }
        }
    }
}