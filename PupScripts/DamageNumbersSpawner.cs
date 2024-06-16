using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Manager;
using TMPro;

namespace SuperPupSystems
{
    public class DamageNumbersSpawner : MonoBehaviour
    {
        public GameObject prefab;
        public float textLifeTime = 3.0f;
        public float spawnCloser = 1.0f;
        public void SpawnText(ScoreInfo _info)
        {
            if (_info.location != null)
            {
                GameObject text3D = GameObject.Instantiate(prefab, (Vector3)_info.location, Quaternion.identity);
                text3D.GetComponentInChildren<TMP_Text>().text = _info.delta.ToString();
                Destroy(text3D, textLifeTime);
            }
        }
    }
}