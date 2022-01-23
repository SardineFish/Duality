using System.Collections.Generic;
using Duality;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    [RequireComponent(typeof(PlayableTracks))]
    public class CutScene : RuntimeSingleton<CutScene>, IAnimationClipSource
    {
        private string name = "";
        public AnimationClip Animation;
        private PlayableTracks _playableTracks;
        private bool cutting = false;

        protected override void Awake()
        {
            base.Awake();

            _playableTracks = GetComponent<PlayableTracks>();
        }

        public void ChangeScene(string nextScene)
        {
            if (cutting)
                return;
            cutting = true;
            name = nextScene;
            _playableTracks.PlayOnTrack(0, Animation);
        }

        public void DoLoad()
        {
            SceneManager.LoadScene(name);
            cutting = false;
        }

        public void GetAnimationClips(List<AnimationClip> results)
        {
            AnimationManagerBase.GetAnimationClipMember(this, results);
        }
    }
}