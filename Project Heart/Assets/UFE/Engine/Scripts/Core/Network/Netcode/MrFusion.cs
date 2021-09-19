using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFENetcode;

namespace UFE3D
{
    public class MrFusion : MonoBehaviour
    {
        public bool debugger = false;
        [HideInInspector] public bool resetTracks = false;

        private struct TrackableInterface
        {
            public UFEInterface ufeInterface;
            public Dictionary<System.Reflection.MemberInfo, System.Object> tracker;
        }

        private Dictionary<long, TrackableInterface[]> gameHistory = new Dictionary<long, TrackableInterface[]>();
        private UFEInterface[] ufeInterfaces;
        private UFEBehaviour[] ufeBehaviours;
        private TrackableInterface[] track0;


        void Start()
        {
            AssignComponents();
        }

        public void AssignComponents()
        {
            if (ufeInterfaces == null) ufeInterfaces = GetComponentsInChildren<UFEInterface>();
            if (ufeBehaviours == null) ufeBehaviours = GetComponentsInChildren<UFEBehaviour>();
        }

        public void StartBehaviours()
        {
            if (ufeBehaviours == null) return;
            foreach (UFEBehaviour ufeBehaviour in ufeBehaviours)
            {
                ufeBehaviour.UFEStart();
            }
        }

        public void UpdateBehaviours()
        {
            if (ufeBehaviours == null) return;
            foreach (UFEBehaviour ufeBehaviour in ufeBehaviours)
            {
                ufeBehaviour.UFEFixedUpdate();
            }
        }

        public void SaveState(long frame)
        {
            List<TrackableInterface> newTrackableList = new List<TrackableInterface>();
            foreach (UFEInterface ufeInterface in ufeInterfaces)
            {
                TrackableInterface newTrackableInterface;
                newTrackableInterface.ufeInterface = ufeInterface;
                newTrackableInterface.tracker = RecordVar.SaveStateTrackers(ufeInterface, new Dictionary<System.Reflection.MemberInfo, object>());
                newTrackableList.Add(newTrackableInterface);
            }

            if (gameHistory.ContainsKey(frame))
            {
                gameHistory[frame] = newTrackableList.ToArray();
            }
            else
            {
                gameHistory.Add(frame, newTrackableList.ToArray());
                if (track0 == null) track0 = newTrackableList.ToArray();
            }
        }

        public void LoadState(long frame)
        {
            if (gameHistory.ContainsKey(frame))
            {
                TrackableInterface[] loadedInterfaces = gameHistory[frame];
                LoadState(loadedInterfaces);
            }
            else
            {
                Debug.LogError("Frame data not found (" + frame + ")");
            }
        }

        private void LoadState(TrackableInterface[] loadedInterfaces)
        {
            foreach (TrackableInterface trackableInterface in loadedInterfaces)
            {
                UFEInterface reflectionTarget = trackableInterface.ufeInterface;
                reflectionTarget = RecordVar.LoadStateTrackers(trackableInterface.ufeInterface, trackableInterface.tracker);
                if (reflectionTarget == null && debugger) Debug.LogWarning("Empty interface found at '" + trackableInterface.ToString() + "'");
            }
        }

        public void ResetTrack()
        {
            if (track0 != null) LoadState(track0);
        }
    }
}