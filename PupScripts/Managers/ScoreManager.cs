using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Manager
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;
        public UpdateScoreEvent UpdateScore;
        public int Score { get; private set; }
        public int Multiplier { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Multiplier = 1;
            Score = 0;

            if(UpdateScore == null)
                UpdateScore = new UpdateScoreEvent();
        }

        public void AddPoints(int _amount, Vector3? _location = null)
        {
            Score += (_amount * Multiplier);


            UpdateScore.Invoke( new ScoreInfo(Score, _amount, _location));
        }

        public void ResetScore()
        {
            Multiplier = 1;
            Score = 0;

            UpdateScore.Invoke( new ScoreInfo(0, 0, Vector3.zero));
        }
    }

    public class ScoreInfo
    {
        public int Score;
        public int Delta;
        public Vector3? Location;

        public ScoreInfo(int s, int d, Vector3? l)
        {
            Score = s;
            Delta = d;
            Location = l;
        }
    }

    [System.Serializable]
    public class UpdateScoreEvent : UnityEngine.Events.UnityEvent<ScoreInfo> {}
}