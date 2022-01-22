using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Duality
{
    [Serializable]
    public struct PlayableTrackConfig
    {
        public bool DestroyOnFinish;
    }

    enum PlayableTrackType
    {
        AnimatorController,
        AnimationClip
    }

    class PlayableTrackState
    {
        public PlayableTrackType Type;
        public AnimationClipPlayable AnimationClip;
        public AnimatorControllerPlayable AnimatorController;

        public PlayableTrackConfig Config;

        private readonly PlayableGraph _playable;
        private readonly AnimationMixerPlayable _mixer;

        private TaskCompletionSource<AnimationClip> _completionSource;
        
        public int TrackID { get; }

        public bool IsPlaying
        {
            get
            {
                if (AnimationClip.IsValid() && AnimationClip.GetTime() < AnimationClip.GetAnimationClip().length)
                    return true;
                if (AnimatorController.IsValid())
                    return true;
                return false;
            }
        }

        public PlayableTrackState(PlayableGraph playable, AnimationMixerPlayable mixer, int trackID)
        {
            _playable = playable;
            _mixer = mixer;
            TrackID = trackID;
        }

        public void Stop()
        {
            if (AnimationClip.IsValid())
            {
                _playable.Disconnect(_mixer, TrackID);
                AnimationClip.Destroy();
            }

            if (AnimatorController.IsValid())
            {
                _playable.Disconnect(_mixer, TrackID);
                AnimatorController.Destroy();
            }
        }

        public TaskCompletionSource<AnimationClip> SetupCompleteSource()
        {
            this._completionSource = new TaskCompletionSource<AnimationClip>();
            return this._completionSource;
        }

        public void PlayAnimationClip(AnimationClip clip, PlayableTrackConfig config)
        {
            Stop();
            
            AnimationClip = AnimationClipPlayable.Create(_playable, clip);
            _playable.Connect(AnimationClip, 0, _mixer, TrackID);
            _mixer.SetInputWeight(TrackID, 1);

            Config = config;
        }

        public AnimatorControllerPlayable PlayAnimatorController(RuntimeAnimatorController animatorController, PlayableTrackConfig config)
        {
            Stop();
            
            AnimatorController = AnimatorControllerPlayable.Create(_playable, animatorController);
            _playable.Connect(AnimatorController, 0, _mixer, TrackID);
            _mixer.SetInputWeight(TrackID, 1);
            Config = config;

            return AnimatorController;
        }

        public void Update()
        {
            if(!Config.DestroyOnFinish)
                return;

            Debug.Log($"{AnimationClip.IsValid()} {AnimationClip.GetTime()}");

            if (AnimationClip.IsValid()
                && AnimationClip.GetTime() >= AnimationClip.GetAnimationClip().length
                && !AnimationClip.GetAnimationClip().isLooping)
            {
                if(!(_completionSource is null))
                    _completionSource.SetResult(AnimationClip.GetAnimationClip());
                if(Config.DestroyOnFinish)
                    AnimationClip.Destroy();
                _completionSource = null;
            }
        }
    }
    
    public enum AnimationOverlayMethod
    {
        /// <summary>
        /// Play the new animation on another track with addition mixing
        /// </summary>
        Add,
        
        /// <summary>
        /// Stop the original animation and replay it 
        /// </summary>
        Restart,
        
        /// <summary>
        /// Keep original animation playing
        /// </summary>
        Keep,
    }
    [RequireComponent(typeof(Animator))]
    public class PlayableTracks : MonoBehaviour
    {
        [SerializeField]
        private int tracksCount;

        // [SerializeField]
        // [Tooltip("Specific how to deal with another same animation currently playing on track, " +
        //          "Only work with 'PlayAnyAvailable'" + 
        //          "'Add': Play the new animation on another track with addition mixing, " +
        //          "'Restart': Stop the original animation and replay it, " +
        //          "'Keep': Keep original animation playing")]
        // private AnimationOverlayMethod OverlayMethod = AnimationOverlayMethod.Add;
        
        [SerializeField]
        private bool playOnStart = true;

        [SerializeField] 
        private DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime;

        [SerializeField] 
        private PlayableTrackConfig defaultConfig;

        private Animator animator;
        private PlayableGraph playableGraph;
        private AnimationPlayableOutput output;
        private AnimationMixerPlayable mixer;
        private readonly List<PlayableTrackState> tracks = new List<PlayableTrackState>();

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playableGraph = PlayableGraph.Create(name);
            playableGraph.SetTimeUpdateMode(updateMode);
            output = AnimationPlayableOutput.Create(playableGraph, "PlayableOutput", animator);
            mixer = AnimationMixerPlayable.Create(playableGraph, tracksCount);
            output.SetSourcePlayable(mixer);
            tracks.Capacity = tracksCount;
            tracks.Clear();
            for (var i = 0; i < tracksCount; ++i)
            {
                tracks.Add(new PlayableTrackState(playableGraph, mixer, i));
            }
        }

        private void Start()
        {
            if(playOnStart)
                playableGraph.Play();
        }

        public void PlayOnTrack(int track, AnimationClip animationClip)
            => PlayOnTrack(track, animationClip, defaultConfig);

        public async Task PlayOnTrackWait(int track, AnimationClip animationClip)
        {
            PlayOnTrack(track, animationClip);
            var completeSource = tracks[track].SetupCompleteSource();
            await completeSource.Task;
        }

        public void PlayOnTrack(int track, AnimationClip animationClip, PlayableTrackConfig config)
        {
            Debug.Log($"[PlayableTrack] Play {animationClip.name} on {track}");
            
            tracks[track].PlayAnimationClip(animationClip, config);
        }

        public AnimatorControllerPlayable PlayOnTrack(int track, RuntimeAnimatorController animatorController)
        {
            return tracks[track].PlayAnimatorController(animatorController, defaultConfig);
        }

        public void ShowDebug()
        {
            // GraphVisualizerClient.Show(playableGraph);
        }

        private void LateUpdate()
        {
            for (var track = 0; track < tracksCount; ++track)
            {
                tracks[track].Update();
            }
        }

        /// <summary>
        /// Play animation clip on an empty track
        /// Return -1 if no track available
        /// </summary>
        public int PlayAnyAvailable(AnimationClip animationClip)
        {
            // CleanupTracks();

            foreach (var track in tracks)
            {
                if (!track.IsPlaying)
                {
                    PlayOnTrack(track.TrackID, animationClip, defaultConfig);
                    return track.TrackID;
                }
            }
            
            Debug.Log($"[PlayableTrack] No empty track to play {animationClip.name}");

            return -1;

            // if (OverlayMethod == AnimationOverlayMethod.Keep || OverlayMethod == AnimationOverlayMethod.Restart)
            // {
            //     for (var i = 0; i < tracksCount; ++i)
            //     {
            //         if (tracks[i].IsValid() && tracks[i].GetAnimationClip() == animationClip)
            //         {
            //             switch (OverlayMethod)
            //             {
            //                 case AnimationOverlayMethod.Keep:
            //                     return i;
            //                 case AnimationOverlayMethod.Restart:
            //                     PlayOnTrack(i, animationClip);
            //                     return i;
            //             }
            //         }
            //     }
            // }
            //
            // for (var i = 0; i < tracksCount; ++i)
            // {
            //     if (!tracks[i].IsValid())
            //     {
            //         PlayOnTrack(i, animationClip);
            //         return i;
            //     }
            // }
            //
            // return 0;
        }

        // private void CleanupTracks()
        // {
        //     for (var i = 0; i < tracksCount; ++i)
        //     {
        //         if (tracks[i].IsValid() && tracks[i].GetTime() > tracks[i].GetAnimationClip().length)
        //         {
        //             playableGraph.Disconnect(mixer, i);
        //             tracks[i].Destroy();
        //         }
        //     }
        // }

        public void Play() => playableGraph.Play();

        public void Stop() => playableGraph.Stop();
    }
}