using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.Helper
{
    public class DamageIndicator : MonoBehaviour
    {
        public Material defaultMaterial;
        public Material hurtMaterial;
        public Renderer targetRenderer;
        public Timer timer;
        
        public void ResetDefaultMaterial()
        {
            targetRenderer.material = defaultMaterial;
        }

        public void Hurt()
        {
            timer.StartTimer();
            targetRenderer.material = hurtMaterial;
        }
    }
}