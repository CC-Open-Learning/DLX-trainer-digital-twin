using UnityEngine;
using UnityEngine.Audio;

namespace VARLab.MPCircuits
{
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class MixerGroupManager : MonoBehaviour
    {
        public const float DecibelMinVolume = -144.0f;


        public enum AudioManagerType
        {
            SFX,
            Voiceover
        }

        public string VolumeParameter;

        public AudioManagerType ManagerType;

        public AudioMixer Mixer { get; set; }
        public AudioMixerGroup Group { get; set; }
        public AudioSource Source { get; set; }

        /// <summary>
        /// Using this instead of the singleton. Additionally, trying this as a solution to many tests using FindObjectOfType<SFXManager> or the same for VoiceOverManager.
        /// </summary>
        /// <param name="managerType"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static MixerGroupManager FindObjectByManagerType(AudioManagerType managerType)
        {
            foreach (var manager in FindObjectsOfType<MixerGroupManager>())
            {
                if (manager.ManagerType != managerType) { continue; }
                return manager;
            }

            return null;
        }


        public static float LinearToDecibel(float linear)
        {
            return linear == 0f ? DecibelMinVolume : 20f * Mathf.Log10(linear);
        }


        private void Awake()
        {
            Source = GetComponent<AudioSource>();
            Source.outputAudioMixerGroup = Group ? Group : Source.outputAudioMixerGroup;    //if Group is null, uses the main audio source output audio Mixer group

            Mixer = Source.outputAudioMixerGroup.audioMixer;   //only needed in order to assign the correct Mixer group automatically (needed for testing)
            Group = Source.outputAudioMixerGroup;

            if (Group != null) return;
            Group = Mixer.FindMatchingGroups(ManagerType.ToString())[0];   //Name of Mixer group must match managerType name
        }

        private void Start()
        {
            SetMixerGroupVolume(VolumeParameter, GetStartingVolumeByManagerType());
        }

        public void PlayAudioClip(AudioClip clip, float volume = 1f)
        {
            Source.PlayOneShot(clip, volume);
        }

        public void SetMixerGroupVolume(string exposedParameter, float volume)
        {
            // Set audio Mixer parameter (adjusted from linear to decibel and clamped between the max and min levels)
            Group.audioMixer.SetFloat(exposedParameter, LinearToDecibel(volume));
        }

        public float GetStartingVolumeByManagerType()
        {
            if (ManagerType == AudioManagerType.Voiceover)
            {
                return Settings.VoiceOverVolume;
            }
            else
            {
                return Settings.SoundEffectVolume;
            }
        }
    }
}
