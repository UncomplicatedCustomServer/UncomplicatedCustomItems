using Exiled.Loader;
using System.IO;
using System.Linq;
using UncomplicatedCustomItems.API.Features.Helper;
using UnityEngine;
namespace UncomplicatedCustomItems.API.Features
{
    public class AudioApi
    {

        /// <summary>
        /// If true it enables access to the custom sound custom flag.
        /// </summary>
        public bool EnableAudioApi { get; set; } = false;
        public AudioApi()
        {
            if (!CheckForNVorbisDependency())
            {
                LogManager.Error("You don't have the AudioPlayerApi dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                EnableAudioApi = false;
            }
            else if (!CheckForAudioPlayerApiDependency())
                {
                    LogManager.Error("You don't have the dependency AudioPlayerApi installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
                    EnableAudioApi = false;
                }
            else
                EnableAudioApi = true;
        }
        private bool CheckForAudioPlayerApiDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "AudioPlayerApi");
        private bool CheckForNVorbisDependency() => Loader.Dependencies.Any(assembly => assembly.GetName().Name == "NVorbis");
        public static float Clamp(float? value, float min, float max)
        {
            return (float)((value < min) ? min : (value > max) ? max : value);
        }
        /// <summary>
        /// Plays audio that a custom item requests it.
        /// </summary>
        /// <param name="CustomItem"></param>
        /// <param name="Coords"></param>
        public void PlayAudio(SummonedCustomItem CustomItem, Vector3 Coords)
        {
            LogManager.Debug($"PlayAudio method triggered by {CustomItem.CustomItem.Name} at {Coords}");
            if (EnableAudioApi != false)
            {
                LogManager.Debug($"Audio API is enabled!");
                var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                var flagSetting = flagSettings.FirstOrDefault();
                if (!string.IsNullOrEmpty(flagSetting.AudioPath))
                {
                    LogManager.Debug($"Succesfully loaded audio path {flagSetting.AudioPath}");
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global_Audio_{CustomItem.CustomItem.Id}", onIntialCreation: (p) =>
                    {
                        float maxDistance = flagSetting.AudibleDistance ?? 1f;
                        Speaker speaker = p.AddSpeaker("Main", isSpatial: true, maxDistance: maxDistance);
                        speaker.transform.position = Coords;
                    });
                    float volume = Clamp(flagSetting.SoundVolume, 1f, 100f);
                    audioPlayer.AddClip($"sound_{CustomItem.CustomItem.Id}", volume);
                    LogManager.Debug($"Playing {Path.GetFileName(flagSetting.AudioPath)}");
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
