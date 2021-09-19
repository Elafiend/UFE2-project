using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class AnnouncerOptions
    {
        [HideInInspector] public string announcerName = string.Empty;
        public AudioClip round1;
        public AudioClip round2;
        public AudioClip round3;
        public AudioClip otherRounds;
        public AudioClip finalRound;
        public AudioClip fight;
        public AudioClip player1Wins;
        public AudioClip player2Wins;
        public AudioClip perfect;
        public AudioClip firstHit;
        public AudioClip counterHit;
        public AudioClip parry;
        public AudioClip timeOver;
        public AudioClip ko;
        public ComboAnnouncer[] combos;
    }
}