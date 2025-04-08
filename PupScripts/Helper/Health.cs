using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Helper
{
    [System.Serializable]
    public class HealthChangedEvent : UnityEvent<HealthChangedObject> {}

    [System.Serializable]
    public class HealedEvent : UnityEvent<int> {}
    public class Health : MonoBehaviour
    {
        public HealthChangedEvent healthChanged;
        public HealedEvent healed;
        public UnityEvent hurt;
        public UnityEvent outOfHealth;

        [Tooltip("")]
        public int maxHealth = 100;
        [Tooltip("")]
        public int currentHealth = 0;

        public bool defaultToMaxHealth = true;

        /// <summary>
        /// Start is called in the frame when a script is enable just before any
        /// update methods are called the first time.
        /// </summary>
        void Start()
        {
            if(defaultToMaxHealth)
                currentHealth = maxHealth;

            if (healthChanged == null)
                healthChanged = new HealthChangedEvent();
            
            if (healed == null)
                healed = new HealedEvent();

            if (hurt == null)
                hurt = new UnityEvent();
            
            if (outOfHealth == null)
                outOfHealth = new UnityEvent();
            
            healthChanged.Invoke(new HealthChangedObject{ maxHealth = maxHealth, currentHealth = currentHealth, delta = currentHealth});
        }

        /// <summary>
        /// Reduce the current health by the amount passed in.
        /// </summary>
        /// <param name="_amount">How much of the current health will you lose.</param>
        public void Damage(int _damage)
        {
            if (currentHealth <= 0)
                return;

            currentHealth -= _damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                outOfHealth.Invoke();
            }
            else
            {
                hurt.Invoke();
            }
            
            healthChanged.Invoke(new HealthChangedObject{ maxHealth = maxHealth, currentHealth = currentHealth, delta = -_damage});
        }

        /// <summary>
        /// Regain health if health is already above zero.
        /// </summary>
        /// <param name="_amount">The amount to heal the health class or if null current health will equal to max health.</param>
        public void Heal(int? _amount = null)
        {
            if (currentHealth <= 0)
                return;

            if (_amount == null)
            {
                _amount = maxHealth - currentHealth;
            }

            currentHealth += (int)_amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            healed.Invoke((int)_amount);
            healthChanged.Invoke(new HealthChangedObject{ maxHealth = maxHealth, currentHealth = currentHealth, delta = (int)_amount});
        }

        /// <summary>
        /// Revives health class only works is dead.
        /// </summary>
        /// <param name="_amount">The new current health after getting revived or if null current health will equal to max health.</param>
        public void Revive(int? _amount = null)
        {
            if (currentHealth > 0)
                return;
            
            currentHealth = 0;
            
            if (_amount == null)
                _amount = maxHealth;

            currentHealth = (int)_amount;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
            
            healed.Invoke((int)_amount);
            healthChanged.Invoke(new HealthChangedObject{ maxHealth = maxHealth, currentHealth = currentHealth, delta = (int)_amount});
        }

        /// <summary>
        /// Will set the current health to zero
        /// </summary>
        public void Kill()
        {
            Damage(currentHealth);
        }

        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }
    }

    public struct HealthChangedObject {
        public int maxHealth;
        public int currentHealth;
        public int delta;
    }
}
