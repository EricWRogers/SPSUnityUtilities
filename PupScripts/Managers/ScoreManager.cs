using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Manager
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;
        public UpdateScoreEvent updateScore;
        public int score { get; private set; }
        public int multiplier { get; private set; }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            multiplier = 1;
            score = 0;

            if(updateScore == null)
                updateScore = new UpdateScoreEvent();
        }

        public void AddPoints(int _amount, Vector3? _location = null)
        {
            score += (_amount * multiplier);


            updateScore.Invoke( new ScoreInfo(score, _amount, _location));
        }

        public void ResetScore()
        {
            multiplier = 1;
            score = 0;

            updateScore.Invoke( new ScoreInfo(0, 0, Vector3.zero));
        }
    }

    public class ScoreInfo
    {
        public int score;
        public int delta;
        public Vector3? location;

        public ScoreInfo(int _score, int _delta, Vector3? _location)
        {
            score = _score;
            delta = _delta;
            location = _location;
        }
    }

    [System.Serializable]
    public class UpdateScoreEvent : UnityEngine.Events.UnityEvent<ScoreInfo> {}
}