using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Helper
{
    public class DamageIndicator : MonoBehaviour
    {
        public Material defaultMaterial;
        public Material hurtMaterial;
        public Timer timer;
        
        public void ResetDefaultMaterial()
        {
            gameObject.GetComponent<Renderer>().material = defaultMaterial;
        }

        public void Hurt()
        {
            timer.StartTimer();
            gameObject.GetComponent<Renderer>().material = hurtMaterial;
        }
    }
}