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
            if (enabled == false) return;

            targetRenderer.material = defaultMaterial;
        }

        public void Hurt()
        {
            if (enabled == false) return;
            
            timer.StartTimer();
            targetRenderer.material = hurtMaterial;
        }
    }
}