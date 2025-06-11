using UnityEngine;

namespace VARLab.MPCircuits
{
    public class Singleton : MonoBehaviour
    {
        public static Singleton Instance { get; private set; }

        public MixerGroupManager VoiceOverManager;
        public MixerGroupManager SFXManager;
        public InteractionManager InteractionManager;
        public PanelManager PanelManager;
        public SettingsManager SettingsManager;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // The Singleton object is not a top-level object, so this code
                // will likely not get called.
                // It doesn't need to move itself into DontDestroyOnLoad, the trainer board
                // safely re-initializes itself when the scene is reloaded
                if (transform.parent = null) { DontDestroyOnLoad(gameObject); }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
