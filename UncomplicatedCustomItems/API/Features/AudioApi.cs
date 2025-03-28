using Exiled.Loader;
using MEC;
using PlayerRoles;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
            if (EnableAudioApi != false)
            {
                var flagSettings = SummonedCustomItem.GetAllFlagSettings();
                var flagSetting = flagSettings.FirstOrDefault();
                if (!string.IsNullOrEmpty(flagSetting.AudioPath))
                {
                    AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Global_Audio_{CustomItem.CustomItem.Id}", onIntialCreation: (p) =>
                    {
                        float maxDistance = flagSetting.AudibleDistance ?? 1f;
                        Speaker speaker = p.AddSpeaker("Main", isSpatial: true, maxDistance: maxDistance);
                        speaker.transform.position = Coords;
                    });
                    float volume = Clamp(flagSetting.SoundVolume, 1f, 100f);
                    audioPlayer.AddClip($"sound_{CustomItem.CustomItem.Id}", volume);
                }
            }
            else
            {
                LogManager.Error("You don't have AudioPlayerApi or its dependency NVorbis installed!\nInstall it to use the custom sound custom flag.\nIf you need support join our Discord server: https://discord.gg/5StRGu8EJV");
            }
        }
    }
}
