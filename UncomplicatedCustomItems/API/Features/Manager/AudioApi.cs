#if EXILED
using Exiled.Loader;
#endif
using System;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.API.CustomModuleAPI.CustomModules;
using UnityEngine;

namespace UncomplicatedCustomItems.API.Features.Helper
{
    /// <summary>
    /// Handles all audio related methods for <see cref="CustomItem"/>
    /// </summary>
    public class AudioApi
    {
        /// <summary>
        /// If true it enables access to the custom sound custom flag.
        /// </summary>
        public static bool EnableAudioApi { get; set; }
        
        /// <summary>
        /// Checks for dependencies that <see cref="AudioApi"/> requires.
        /// </summary>
        public static void Init()
        {
            if (!CheckForNVorbisDependency())
            {
                LogManager.Error("You don't have the AudioPlayerApi dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV\nError code: 0x400");
                EnableAudioApi = false;
                return;
            }

            if (!CheckForAudioPlayerApiDependency())
            {
                LogManager.Error("You don't have the dependency AudioPlayerApi installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV\nError code: 0x405");
                EnableAudioApi = false;
                return;
            }

            EnableAudioApi = true;
        }
        
        private static bool CheckForAudioPlayerApiDependency() => LabApi.Loader.Features.Misc.AssemblyUtils.GetLoadedAssemblies().Any(assembly => assembly.StartsWith("AudioPlayerApi", StringComparison.OrdinalIgnoreCase));
        private static bool CheckForNVorbisDependency() => LabApi.Loader.Features.Misc.AssemblyUtils.GetLoadedAssemblies().Any(assembly => assembly.StartsWith("NVorbis", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Clamps the value between the minimum and maximum.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float? value, float min, float max)
        {
            return (float)((value < min) ? min : (value > max) ? max : value);
        }

        /// <summary>
        /// Plays audio for a <see cref="SummonedCustomItem"/> at the specified location.
        /// </summary>
        /// <param name="CustomItem"></param>
        /// <param name="Coords"></param>
        public static void PlayAudio(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            if (!CustomItem.TryGetModule<CustomAudio>(out var data))
            {
                LogManager.Warn($"SoundModule not found on {CustomItem.CustomItem.Name}!");
                return;
            }

            LogManager.Debug($"PlayAudio method triggered by {CustomItem.CustomItem.Name} at {Coords}");
            if (EnableAudioApi != false)
            {
                LogManager.Debug($"Audio API is enabled!");
                if (!string.IsNullOrEmpty(data.AudioPath))
                {
                    string clipId = $"sound_{Guid.NewGuid()}";
                    LogManager.Debug($"Succesfully loaded audio path {data.AudioPath}");
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global_Audio_{Guid.NewGuid()}", onIntialCreation: (p) =>
                    {
                        Speaker speaker = p.AddSpeaker("Main", Coords, isSpatial: true, maxDistance: data.AudibleDistance);
                    });

                    float volume = Clamp(data.Volume, 1f, 100f)/100;
                    audioPlayer.AddClip($"{clipId}", volume);
                    AudioClipStorage.LoadClip(data.AudioPath, $"{clipId}");
                    LogManager.Debug($"Playing {Path.GetFileName(data.AudioPath)}");
                    LogManager.Debug($"Audio should have been played.");
                }
                else
                    LogManager.Warn($"Audio path is null please fill out the config properly.");
            }
            else
                LogManager.Warn("You don't have AudioPlayerApi or its dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
        }

        /// <summary>
        /// Plays audio for a <see cref="CustomItem"/> at the specified location.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="volumefloat"></param>
        /// <param name="coords"></param>
        /// <param name="audibledistance"></param>
        public static void PlayAudio(string path, float volumefloat, Vector3 coords, float audibledistance = 1f)
        {
            if (EnableAudioApi != false)
            {
                LogManager.Debug($"Audio API is enabled!");
                if (!string.IsNullOrEmpty(path))
                {
                    string clipId = $"sound_{Guid.NewGuid()}";
                    LogManager.Debug($"Succesfully loaded audio path {path}");
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global_Audio_{Guid.NewGuid()}", onIntialCreation: (p) =>
                    {
                        Speaker speaker = p.AddSpeaker("Main", coords, isSpatial: true, maxDistance: audibledistance);
                    });

                    float volume = Clamp(volumefloat, 1f, 100f);
                    audioPlayer.AddClip($"{clipId}", volume);
                    AudioClipStorage.LoadClip(path, $"{clipId}");
                    LogManager.Debug($"Playing {Path.GetFileName(path)}");
                    LogManager.Debug($"Audio should have been played.");
                }
                else
                    LogManager.Warn($"Audio path is null please fill out the config properly.");
            }
            else
                LogManager.Warn("You don't have AudioPlayerApi or its dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
        }
    }
}