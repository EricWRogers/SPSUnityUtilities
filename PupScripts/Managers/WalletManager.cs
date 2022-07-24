using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Manager
{
    public class WalletManager : MonoBehaviour
    {
        public static WalletManager Instance;
        // Events
        public UnityEvent<int, int> coinUpdatedEvent; // delta, current coins
        public UnityEvent<int> earnEvent;
        public UnityEvent purchaseEvent;
        public UnityEvent outOfCashEvent;

        // Variables
        private int coin = 0;

        public int Coin { get { return coin; } }


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (coinUpdatedEvent == null)
                coinUpdatedEvent = new UnityEvent<int, int>();

            if (earnEvent == null)
                earnEvent = new UnityEvent<int>();

            if (purchaseEvent == null)
                purchaseEvent = new UnityEvent();

            if (outOfCashEvent == null)
                outOfCashEvent = new UnityEvent();
        }

        public bool ICanAfford(int _amount)
        {
            if (0 > (coin - _amount))
            {
                return false;
            }

            return true;
        }

        public bool Pay(int _amount)
        {
            bool iCanAfford = ICanAfford(_amount);

            if (!iCanAfford)
                return false;

            coin -= _amount;
            if (coin <= 0)
                outOfCashEvent.Invoke();

            purchaseEvent.Invoke();

            coinUpdatedEvent.Invoke(-_amount, coin);

            return true;
        }

        public void Earn(int _amount, Vector3? _location = null)
        {
            if (coin < 0)
                return;

            coin += _amount;

            earnEvent.Invoke(_amount);
            coinUpdatedEvent.Invoke(_amount, coin);
        }
    }
}