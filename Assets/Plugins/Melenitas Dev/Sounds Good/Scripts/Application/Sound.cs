/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */
using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MelenitasDev.SoundsGood
{
    public partial class Sound // Fields
    {
        private SourcePoolElement sourcePoolElement;

        private float volume = 1;
        private Vector2 hearDistance = new Vector2(3, 500);
        private float pitch = 1;
        private Vector2 pitchRange = new Vector2(0.85f, 1.15f);
        private string id = null;
        private Vector3 position = Vector3.zero;
        private Transform followTarget = null;
        private bool loop = false;
        private bool spatialSound = true;
        private float fadeOutTime = 0;
        private bool randomClip = true;
        private bool forgetSourcePoolOnStop = false;
        private AudioClip clip = null;
        private AudioMixerGroup output = null;
        private string cachedSoundTag;
    }

    public partial class Sound // Fields (Callbacks)
    {
        private Action onPlay;
        private Action onComplete;
        private Action onLoopCycleComplete;
        private Action onPause;
        private Action onPauseComplete;
        private Action onResume;
    }

    public partial class Sound // Properties
    {
        /// <summary>It's true when it's being used. When it's paused, it's true as well</summary>
        public bool Using => sourcePoolElement != null;
        /// <summary>It's true when audio is playing.</summary>
        public bool Playing => Using && sourcePoolElement.Playing;
        /// <summary>It's true when audio paused (it ignore the fade out time).</summary>
        public bool Paused => Using && sourcePoolElement.Paused;
        /// <summary>Volume level between [0,1].</summary>
        public float Volume => volume;
        /// <summary>Total time in seconds that it have been playing.</summary>
        public float PlayingTime => Using ? sourcePoolElement.PlayingTime : 0;
        /// <summary>Reproduced time in seconds of current loop cycle.</summary>
        public float CurrentLoopCycleTime => Using ? sourcePoolElement.CurrentLoopCycleTime : 0;
        /// <summary>Times it has looped.</summary>
        public int CompletedLoopCycles => Using ? sourcePoolElement.CompletedLoopCycles : 0;
        /// <summary>Duration in seconds of matched clip.</summary>
        public float ClipDuration => clip != null ? clip.length : 0;
        /// <summary>Matched clip.</summary>
        public AudioClip Clip => clip;
    }

    public partial class Sound // Public Methods
    {
        /// <summary>
        /// Create new Sound object given a clip.
        /// </summary>
        /// <param name="sfx">Sound you've created before on Audio Creator window</param>
        public Sound (SFX sfx)
        {
            SetClip(sfx.ToString());
        }
        
        /// <summary>
        /// Create new Sound object given a tag.
        /// </summary>
        /// <param name="tag">The tag you've used to create the sound on Audio Creator window</param>
        public Sound (string tag)
        {
            SetClip(tag);
        }

        /// <summary>
        /// Store volume parameters BEFORE play sound.
        /// </summary>
        /// <param name="volume">Volume: min 0, Max 1</param>
        public Sound SetVolume (float volume)
        {
            this.volume = volume;
            return this;
        }
        
        /// <summary>
        /// Store volume parameters BEFORE play sound.
        /// </summary>
        /// <param name="volume">Volume: min 0, Max 1</param>
        /// <param name="hearDistance">Distance range to hear sound</param>
        public Sound SetVolume (float volume, Vector2 hearDistance)
        {
            this.volume = volume;
            this.hearDistance = hearDistance;
            return this;
        }

        /// <summary>
        /// Change volume while sound is reproducing.
        /// </summary>
        /// <param name="newVolume">New volume: min 0, Max 1</param>
        /// <param name="lerpTime">Time to lerp current to new volume</param>
        public void ChangeVolume (float newVolume, float lerpTime = 0)
        {
            if (volume == newVolume) return;
            
            volume = newVolume;
            
            if (!Using) return;
            
            sourcePoolElement.SetVolume(newVolume, hearDistance, lerpTime);
        }

        /// <summary>
        /// Set given pitch. Make your sounds sound different :)
        /// </summary>
        public Sound SetPitch (float pitch)
        {
            this.pitch = pitch;
            return this;
        }
        
        /// <summary>
        /// Set my recommended random pitch. Range is (0.85, 1.15). It's useful to avoid sounds be repetitive.
        /// </summary>
        public Sound SetRandomPitch ()
        {
            pitch = Random.Range(0.85f, 1.15f);
            return this;
        }
        
        /// <summary>
        /// Set random pitch between given range. It's useful to avoid sounds be repetitive.
        /// </summary>
        /// <param name="pitchRange">Pitch range (min, Max)</param>
        public Sound SetRandomPitch (Vector2 pitchRange)
        {
            pitch = Random.Range(pitchRange.x, pitchRange.y);
            return this;
        }

        /// <summary>
        /// Set an id to identify this sound on AudioManager static methods.
        /// </summary>
        public Sound SetId (string id)
        {
            this.id = id;
            return this;
        }

        /// <summary>
        /// Make your sound loops for infinite time. If you need to stop it, use Stop() method.
        /// </summary>
        public Sound SetLoop (bool loop)
        {
            this.loop = loop;
            return this;
        }
        
        /// <summary>
        /// Change the AudioClip of this Sound BEFORE play it.
        /// </summary>
        /// <param name="tag">The tag you've used to create the sound on Audio Creator</param>
        public Sound SetClip (string tag)
        {
            cachedSoundTag = tag;
            clip = AudioManager.GetSFX(tag);
            return this;
        }
        
        /// <summary>
        /// Change the AudioClip of this Sound BEFORE play it.
        /// </summary>
        /// <param name="sfx">Sound you've created before on Audio Creator</param>
        public Sound SetClip (SFX sfx)
        {
            SetClip(sfx.ToString());
            return this;
        }
        
        /// <summary>
        /// Make the sound clip change with each new Play().
        /// It'll choose a random sound from those you have added with the same tag in the Audio Creator.
        /// </summary>
        /// <param name="random">Use random clip</param>
        public Sound SetRandomClip (bool random)
        {
            randomClip = random;
            return this;
        }
        
        /// <summary>
        /// Set the position of the sound emitter.
        /// </summary>
        public Sound SetPosition (Vector3 position)
        {
            this.position = position;
            return this;
        }
        
        /// <summary>
        /// Set a target to follow. Audio source will update its position every frame.
        /// </summary>
        /// <param name="followTarget">Transform to follow</param>
        public Sound SetFollowTarget (Transform followTarget)
        {
            this.followTarget = followTarget;
            return this;
        }

        /// <summary>
        /// Set spatial sound.
        /// </summary>
        /// <param name="true">Your sound will be 3D</param>
        /// <param name="false">Your sound will be global / 2D</param>
        public Sound SetSpatialSound (bool activate = true)
        {
            spatialSound = activate;
            return this;
        }
        
        /// <summary>
        /// Set fade out duration. It'll be used when sound ends.
        /// </summary>
        /// <param name="fadeOutTime">Seconds that fade out will last</param>
        public Sound SetFadeOut (float fadeOutTime)
        {
            this.fadeOutTime = fadeOutTime;
            return this;
        }
        
        /// <summary>
        /// Set the audio output to manage the volume using the Audio Mixers.
        /// </summary>
        /// <param name="output">Output you've created before inside Master AudioMixer
        /// (Remember reload the outputs database on Output Manager Window)</param>
        public Sound SetOutput (Output output)
        {
            this.output = AudioManager.GetOutput(output);
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on sound start playing.
        /// </summary>
        /// <param name="onPlay">Method will be invoked</param>
        public Sound OnPlay (Action onPlay)
        {
            this.onPlay = onPlay;
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on sound complete.
        /// If "loop" is active, it'll be called when you Stop the sound manually.
        /// </summary>
        /// <param name="onComplete">Method will be invoked</param>
        public Sound OnComplete (Action onComplete)
        {
            this.onComplete = onComplete;
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on loop cycle complete.
        /// You need to set loop on true to use it.
        /// </summary>
        /// <param name="onLoopCycleComplete">Method will be invoked</param>
        public Sound OnLoopCycleComplete (Action onLoopCycleComplete)
        {
            this.onLoopCycleComplete = onLoopCycleComplete;
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on sound pause.
        /// It will ignore the fade out time.
        /// </summary>
        /// <param name="onPause">Method will be invoked</param>
        public Sound OnPause (Action onPause)
        {
            this.onPause = onPause;
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on sound pause and fade out ends.
        /// </summary>
        /// <param name="onPauseComplete">Method will be invoked</param>
        public Sound OnPauseComplete (Action onPauseComplete)
        {
            this.onPauseComplete = onPauseComplete;
            return this;
        }
        
        /// <summary>
        /// Define a callback that will be invoked on resume/unpause sound.
        /// </summary>
        /// <param name="onResume">Method will be invoked</param>
        public Sound OnResume (Action onResume)
        {
            this.onResume = onResume;
            return this;
        }

        /// <summary>
        /// Reproduce sound.
        /// </summary>
        /// <param name="fadeInTime">Seconds that fade in will last</param>
        public void Play (float fadeInTime = 0)
        {
            if (Using && Playing && loop)
            {
                Stop();
                forgetSourcePoolOnStop = true;
            }

            if (randomClip) SetClip(cachedSoundTag);
            
            var finalPitch = pitch;
            if(!loop && RandomPitchEnable(cachedSoundTag))
                finalPitch *= Random.Range(0.9f, 1.1f);

            sourcePoolElement = AudioManager.GetSource();
            sourcePoolElement
                .SetVolume(volume, hearDistance)
                .SetPitch(finalPitch)
                .SetLoop(loop)
                .SetClip(clip)
                .SetPosition(position)
                .SetFollowTarget(followTarget)
                .SetSpatialSound(spatialSound)
                .SetFadeOut(fadeOutTime)
                .SetId(id)
                .SetOutput(output)
                .OnPlay(onPlay)
                .OnComplete(onComplete)
                .OnLoopCycleComplete(onLoopCycleComplete)
                .OnPause(onPause)
                .OnPauseComplete(onPauseComplete)
                .OnResume(onResume)
                .Play(fadeInTime);
        }

        private bool RandomPitchEnable(string tag)
        {
            return AudioManager.GetIsPitchShiftEnabled(tag);
        }

        /// <summary>
        /// Pause sound.
        /// </summary>
        /// <param name="fadeOutTime">Seconds that fade out will last before pause</param>
        public void Pause (float fadeOutTime = 0)
        {
            if (!Using) return;
            
            sourcePoolElement.Pause(fadeOutTime);
        }

        /// <summary>
        /// Resume/Unpause sound.
        /// </summary>
        /// <param name="fadeInTime">Seconds that fade in will last</param>
        public void Resume (float fadeInTime = 0)
        {
            if (!Using) return;
            
            sourcePoolElement.Resume(fadeInTime);
        }

        /// <summary>
        /// Stop sound.
        /// </summary>
        /// <param name="fadeOutTime">Seconds that fade out will last before stop</param>
        public void Stop (float fadeOutTime = 0)
        {
            if (!Using) return;
            
            if (forgetSourcePoolOnStop)
            {
                sourcePoolElement.Stop(fadeOutTime);
                sourcePoolElement = null;
                forgetSourcePoolOnStop = false;
                return;
            }
            sourcePoolElement.Stop(fadeOutTime, () => sourcePoolElement = null);
        }
    }
}
