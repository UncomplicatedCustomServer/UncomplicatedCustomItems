using Exiled.Loader;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
namespace UncomplicatedCustomItems.API.Features
{
    /// <summary>
    /// Handles all audio related methods for <see cref="CustomItem"/>
    /// </summary>
    public class AudioApi
    {

        /// <summary>
        /// If true it enables access to the custom sound custom flag.
        /// </summary>
        public bool EnableAudioApi { get; set; } = false;
        /// <summary>
        /// Checks for dependencies that <see cref="AudioApi"/> requires.
        /// </summary>
        public AudioApi()
        {
            if (!CheckForNVorbisDependency())
            {
                LogManager.Error("You don't have the AudioPlayerApi dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                EnableAudioApi = false;
            }
            if (!CheckForAudioPlayerApiDependency())
            {
                LogManager.Error("You don't have the dependency AudioPlayerApi installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                EnableAudioApi = false;
            }
            else
                EnableAudioApi = true;
        }
        private bool CheckForAudioPlayerApiDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "AudioPlayerApi");
        private bool CheckForNVorbisDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "NVorbis");
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
        /// Plays audio for a <see cref="CustomItem"/> at the specified location.
        /// </summary>
        /// <param name="CustomItem"></param>
        /// <param name="Coords"></param>
        public void PlayAudio(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            foreach (AudioSettings AudioSettings in CustomItem.CustomItem.FlagSettings.AudioSettings)
            {
                LogManager.Debug($"PlayAudio method triggered by {CustomItem.CustomItem.Name} at {Coords}");
                if (EnableAudioApi != false)
                {
                    LogManager.Debug($"Audio API is enabled!");
                    if (!string.IsNullOrEmpty(AudioSettings.AudioPath))
                    {
                        LogManager.Debug($"Succesfully loaded audio path {AudioSettings.AudioPath}");
                        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global_Audio_{CustomItem.Serial}", onIntialCreation: (p) =>
                        {
                            float maxDistance = AudioSettings.AudibleDistance ?? 1f;
                            Speaker speaker = p.AddSpeaker("Main", Coords, isSpatial: true, maxDistance: maxDistance);
                        });
                        float volume = Clamp(AudioSettings.SoundVolume, 1f, 100f);
                        audioPlayer.AddClip($"sound_{CustomItem.Serial}", volume);
                        AudioClipStorage.LoadClip(AudioSettings.AudioPath, $"sound_{CustomItem.Serial}");
                        LogManager.Debug($"Playing {Path.GetFileName(AudioSettings.AudioPath)}");
                        LogManager.Debug($"Audio should have been played.");
                    }
                    else
                    LogManager.Error($"Audio path is null please fill out the config properly.");
                }
                else
                {
                    LogManager.Error("You don't have AudioPlayerApi or its dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                }
            }
        }
    }
}
