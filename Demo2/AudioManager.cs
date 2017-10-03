using GameEngine.Assets;

namespace Demo2
{
    public class AudioManager
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null) _instance = new AudioManager();
                return _instance;
            }
        }

        private AudioAsset _shoot, _death, _step_l, _step_r, _reload;

        public AudioAsset Shoot => _shoot;
        public AudioAsset Death => _death;
        public AudioAsset StepL => _step_l;
        public AudioAsset StepR => _step_r;
        public AudioAsset Reload => _reload;

        public void Load()
        {
            _shoot = AudioAsset.LoadRelativePath("shoot.wav");
            _death = AudioAsset.LoadRelativePath("death.wav");
            _step_l = AudioAsset.LoadRelativePath("sfx_step_l.wav");
            _step_r = AudioAsset.LoadRelativePath("sfx_step_r.wav");
            _reload = AudioAsset.LoadRelativePath("reload.wav");
        }
    }
}
