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
        public HealthChangedEvent HealthChanged;
        public HealedEvent Healed;
        public UnityEvent Hurt;
        public UnityEvent OutOfHealth;

        [Tooltip("")]
        public int MaxHealth = 100;
        [Tooltip("")]
        public int CurrentHealth = 0;

        /// <summary>
        /// Start is called in the frame when a script is enable just before any
        /// update methods are called the first time.
        /// </summary>
        void Start()
        {
            if (CurrentHealth != 0)
                CurrentHealth = MaxHealth;

            if (HealthChanged == null)
                HealthChanged = new HealthChangedEvent();
            
            if (Healed == null)
                Healed = new HealedEvent();

            if (Hurt == null)
                Hurt = new UnityEvent();
            
            if (OutOfHealth == null)
                OutOfHealth = new UnityEvent();
            
            HealthChanged.Invoke(new HealthChangedObject{ maxHealth = MaxHealth, currentHealth = CurrentHealth, delta = CurrentHealth});
        }

        /// <summary>
        /// Reduce the current health by the amount passed in.
        /// </summary>
        /// <param name="_amount">How much of the current health will you lose.</param>
        public void Damage(int _damage)
        {
            if (CurrentHealth <= 0)
                return;

            CurrentHealth -= _damage;

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                OutOfHealth.Invoke();
            }
            else
            {
                Hurt.Invoke();
            }
            
            HealthChanged.Invoke(new HealthChangedObject{ maxHealth = MaxHealth, currentHealth = CurrentHealth, delta = -_damage});
        }

        /// <summary>
        /// Regain health if health is already above zero.
        /// </summary>
        /// <param name="_amount">The amount to heal the health class or if null current health will equal to max health.</param>
        public void Heal(int? _amount = null)
        {
            if (CurrentHealth <= 0)
                return;

            if (_amount == null)
            {
                _amount = MaxHealth - CurrentHealth;
            }

            CurrentHealth += (int)_amount;

            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            Healed.Invoke((int)_amount);
            HealthChanged.Invoke(new HealthChangedObject{ maxHealth = MaxHealth, currentHealth = CurrentHealth, delta = (int)_amount});
        }

        /// <summary>
        /// Revives health class only works is dead.
        /// </summary>
        /// <param name="_amount">The new current health after getting revived or if null current health will equal to max health.</param>
        public void Revive(int? _amount = null)
        {
            if (CurrentHealth > 0)
                return;
            
            CurrentHealth = 0;
            
            if (_amount == null)
                _amount = MaxHealth;

            CurrentHealth = (int)_amount;

            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
            
            Healed.Invoke((int)_amount);
            HealthChanged.Invoke(new HealthChangedObject{ maxHealth = MaxHealth, currentHealth = CurrentHealth, delta = (int)_amount});
        }

        /// <summary>
        /// Will set the current health to zero
        /// </summary>
        public void Kill()
        {
            Damage(CurrentHealth);
        }
    }

    public struct HealthChangedObject {
        public int maxHealth;
        public int currentHealth;
        public int delta;
    }
}
