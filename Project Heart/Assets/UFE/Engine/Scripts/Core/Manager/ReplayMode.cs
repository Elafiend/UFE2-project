using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
    public abstract class ReplayMode : UFEScreen
    {
        /// <summary>Is the replay in playback mode?</summary>
        public virtual bool isPlayback { get; set; }
        /// <summary>Is the replay in recording mode?</summary>
        public virtual bool isRecording { get; set; }
        /// <summary>Is the replay in play mode?</summary>
        public virtual bool isPlaying { get; set; }

        /// <summary>Toggle UI tools.</summary>
        public bool enableControls = true;
        /// <summary>Toggle UI controls for the state tracker tools.</summary>
        public bool enableStateTrackerControls = true;
        /// <summary>Toggle UI controls for the recording tools.</summary>
        public bool enableRecordingControls = true;
        /// <summary>Allows recording.</summary>
        public bool enableRecording = true;
        /// <summary>Allows playback.</summary>
        public bool enablePlayback = true;
        /// <summary>Allows to exit playback mode and let players regain control of the characters.</summary>
        public bool enablePlayerControl = true;

        /// <summary>The current recorded frame being displayed.</summary>
        public int currentRecFrame = 0;
        /// <summary>The intial frame the counter should start from (active track).</summary>
        public long currentStartingFrame = 0;
        /// <summary>The intial frame the counter should start from (track 1).</summary>
        public long startingFrame1 = 0;
        /// <summary>The intial frame the counter should start from (track 2).</summary>
        public long startingFrame2 = 0;
        /// <summary>The maximum amount of frames to be recorded.</summary>
        public int maxBuffer = 360;

        /// <summary>The keyboard shortcut used to save an individual state.</summary>
        public KeyCode saveState = KeyCode.F2;
        /// <summary>The keyboard shortcut used to load a previously saved state.</summary>
        public KeyCode loadState = KeyCode.F3;
        /// <summary>The keyboard shortcut used to start/stop recording.</summary>
        public KeyCode recordKey = KeyCode.F12;
        /// <summary>The keyboard shortcut used to playback a recording.</summary>
        public KeyCode playbackKey = KeyCode.F11;

        /// <summary>Toggle debug mode to show error messages when a method cannot be executed.</summary>
        public bool showConsoleMessages = true;

        /// <summary>The current recording on playback. Each element is a previously recorded state.</summary>
        protected virtual List<FluxStates> recordedFrames { get; set; } = new List<FluxStates>();
        /// <summary>Default track storage for the recording. Each element is a previously recorded state.</summary>
        protected virtual List<FluxStates> track1 { get; set; } = new List<FluxStates>();
        /// <summary>Alternative track storage to be used on playback. Each element is a previously recorded state.</summary>
        protected virtual List<FluxStates> track2 { get; set; } = new List<FluxStates>();

        /// <summary>Clears the buffer and start recording.</summary>
        public virtual void StartRecording()
        {
            if (!enableRecording) return;
            if (showConsoleMessages && isPlayback)
            {
                Debug.LogError("Can't record while in playback mode.");
                return;
            }
            if (showConsoleMessages && isRecording)
            {
                Debug.LogError("Already recording. Execute StopRecording first.");
                return;
            }
            currentRecFrame = 0;
            track1.Clear();
            isRecording = true;
        }

        /// <summary>Stops the recording and set a starting frame for UI visualization</summary>
        public virtual void StopRecording()
        {
            isRecording = false;
            if (recordedFrames.Count == 0) recordedFrames = track1;
        }

        /// <summary>Playback the recording.</summary>
        public virtual void Play(bool reset = false)
        {
            if (!enablePlayback) return;
            if (showConsoleMessages && isRecording)
            {
                Debug.LogError("Can't play while in record mode.");
                return;
            }
            if (recordedFrames.Count == 0)
                recordedFrames = track1;

            if (showConsoleMessages && recordedFrames.Count == 0)
            {
                Debug.LogError("Nothing recorded.");
                return;
            }
            if (reset) currentRecFrame = 0;
            isPlayback = true;
            isPlaying = true;
        }

        /// <summary>Pauses playback.</summary>
        public virtual void Pause()
        {
            if (showConsoleMessages && !isPlayback)
            {
                Debug.LogError("Pause only works while in playback mode.");
                return;
            }
            isPlaying = false;
        }

        /// <summary>Stops playback.</summary>
        public virtual void Stop()
        {
            if (!enablePlayerControl) return;
            if (showConsoleMessages && !isPlayback)
            {
                Debug.LogError("Stop only works while in playback mode.");
                return;
            }
            currentRecFrame = 0;
            isPlayback = false;
            isPlaying = false;
        }

        /// <summary>Stops playback or recording if there is any.</summary>
        public virtual void StopAll()
        {
            currentRecFrame = 0;
            isPlayback = false;
            isPlaying = false;
            isRecording = false;
        }

        /// <summary>Set the playback frame.</summary>
        public virtual void SetFrame(int frame)
        {
            if (showConsoleMessages && !isPlayback)
            {
                Debug.LogError("SetFrame only works while in playback mode.");
                return;
            }
            if (showConsoleMessages && frame >= recordedFrames.Count)
            {
                Debug.LogError("Frame not found.");
                return;
            }
            if (frame < 0) frame = 0;
            currentRecFrame = frame;

            UFE.fluxCapacitor.LoadReplayBuffer(recordedFrames, currentRecFrame);
        }

        /// <summary>Set the maximum size for the recording. If timer goes over the limit, it will keep the latest recording.</summary>
        public virtual void SetMaxBuffer(int frameCount)
        {
            if (frameCount < 0) frameCount = 0;
            maxBuffer = frameCount;
        }

        /// <summary>Get the current size of the recorded buffer.</summary>
        public virtual int GetBufferSize(int track = 1)
        {
            return track == 1 ? track1.Count : track2.Count;
        }

        /// <summary>Set the initial frame to be shown in the UI.</summary>
        public virtual void SetStartingFrame(long frame, int track = 1)
        {
            if (track == 1)
            {
                startingFrame1 = frame;
                if (startingFrame1 < 0) startingFrame1 = 0;
                currentStartingFrame = startingFrame1;
            }
            else
            {
                startingFrame2 = frame;
                if (startingFrame2 < 0) startingFrame2 = 0;
                currentStartingFrame = startingFrame2;
            }
        }

        /// <summary>Move the recording one frame forward while paused. If last frame is reached, it moves to the first frame.</summary>
        public virtual void MoveForward()
        {
            if (showConsoleMessages && !isPlayback)
            {
                Debug.LogError("MoveForward only works while in playback mode.");
                return;
            }
            currentRecFrame++;
            if (currentRecFrame >= recordedFrames.Count) currentRecFrame = 0;

            UFE.fluxCapacitor.LoadReplayBuffer(recordedFrames, currentRecFrame);
        }

        /// <summary>Move the recording one frame backwards while paused. If first frame is reached, it moves to the last frame.</summary>
        public virtual void MoveBack()
        {
            if (showConsoleMessages && !isPlayback)
            {
                Debug.LogError("MoveBack only works while in playback mode.");
                return;
            }
            currentRecFrame--;
            if (currentRecFrame < 0) currentRecFrame = recordedFrames.Count - 1;

            UFE.fluxCapacitor.LoadReplayBuffer(recordedFrames, currentRecFrame);
        }

        /// <summary>Override replay frame data.</summary>
        /// <param name="data">The FluxStates data for that frame.</param>
        /// <param name="frame">The target frame in the recording.</param>
        public virtual void OverrideReplayFrameData(FluxStates data, int frame)
        {
            for (int i = 0; i < recordedFrames.Count; i++)
            {
                if (recordedFrames[i].networkFrame == frame)
                {
                    recordedFrames[i] = data;
                    return;
                }
            }
        }

        /// <summary>Override track data.</summary>
        public virtual void OverrideTrack(List<FluxStates> data, int target)
        {
            if (target == 1)
                track1 = data;
            else
                track2 = data;
        }

        /// <summary>Set track for playback (1 or 2).</summary>
        public virtual void SetTrack(int track)
        {
            if (track == 1)
            {
                currentStartingFrame = startingFrame1;
                recordedFrames = track1;
            }
            else
            {
                currentStartingFrame = startingFrame2;
                recordedFrames = track2;
            }

            if (currentRecFrame >= recordedFrames.Count)
                currentRecFrame = recordedFrames.Count - 1;
        }

        /// <summary>Save individual state.</summary>
        public virtual void SaveState()
        {
            if (!enableControls) return;
            if (showConsoleMessages) Debug.Log("State saved (frame " + UFE.currentFrame + ")");
            UFE.fluxCapacitor.savedState = FluxStateTracker.SaveGameState(UFE.currentFrame);

            // UFE internal auto-tracking disabled
            //dictionaryList.Add(RecordVar.SaveStateTrackers(this, new Dictionary<MemberInfo, object>()));
            //testRecording = !testRecording;
        }

        /// <summary>Load previously saved state.</summary>
        public virtual void LoadState()
        {
            if (!enableControls) return;
            if (UFE.fluxCapacitor.savedState == null)
                Debug.LogError("Nothing to load");

            if (showConsoleMessages) Debug.Log("State loaded (frame " + UFE.fluxCapacitor.savedState.Value.networkFrame + ")");
            FluxStateTracker.LoadGameState(UFE.fluxCapacitor.savedState.Value);
            UFE.fluxCapacitor.PlayerManager.Initialize(UFE.fluxCapacitor.savedState.Value.networkFrame);

            // UFE internal auto-tracking disabled
            //UFE ufeInstance = this;
            //ufeInstance = RecordVar.LoadStateTrackers(ufeInstance, dictionaryList[dictionaryList.Count - 1]) as UFE;
            //p1ControlsScript.MoveSet.MecanimControl.Refresh();
            //p2ControlsScript.MoveSet.MecanimControl.Refresh();
        }

        public virtual void UFEUpdate()
        {
            if (enableControls && UFE.fluxCapacitor != null && UFE.gameRunning)
            {
                // Record Match
                if (enableRecordingControls)
                {
                    if (!isRecording)
                    {
                        if (enableRecording && !isPlayback && Input.GetKeyDown(recordKey))
                        {
                            StartRecording();
                        }
                        if (recordedFrames.Count > 0 && Input.GetKeyDown(playbackKey))
                        {
                            currentRecFrame = 0;
                            if (enablePlayback && !isPlayback && Input.GetKeyDown(playbackKey))
                            {
                                Play();
                            }
                            else if (Input.GetKeyDown(playbackKey))
                            {
                                Stop();
                            }
                        }
                    }
                    else if (isRecording && Input.GetKeyDown(recordKey))
                    {
                        StopRecording();
                    }
                }

                // Save and Load State
                if (enableRecordingControls)
                {
                    // Save State
                    if (Input.GetKeyDown(saveState))
                    {
                        SaveState();
                    }
                    // Load State
                    if (UFE.fluxCapacitor.savedState != null && Input.GetKeyDown(loadState))
                    {
                        LoadState();
                    }
                }
            }
        }

        public virtual void UFEFixedUpdate()
        {
            if (!isPlayback && isRecording)
            {
                track1.Add(FluxStateTracker.SaveGameState(UFE.currentFrame));
                currentRecFrame++;
                if (track1.Count > maxBuffer)
                {
                    track1.RemoveAt(0);
                    currentRecFrame = maxBuffer - 1;
                }
            }
            else if (isPlaying)
            {
                currentRecFrame++;
                if (currentRecFrame >= track1.Count) currentRecFrame = 0;
                UFE.fluxCapacitor.LoadReplayBuffer(track1, currentRecFrame);
            }
        }

    }
}