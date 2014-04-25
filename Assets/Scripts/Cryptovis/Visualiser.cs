using System.Collections.Generic;
using UnityEngine;

namespace Cryptovis
{
    public abstract class Visualiser : MonoBehaviour
    {
        public Blockchain_Listener blockchain_listener;
        private Queue<Blockchain_Listener.Transaction> transactions;

        protected virtual void Start()
        {
            if( this.blockchain_listener == null )
            {
                this.blockchain_listener = this.GetComponent<Blockchain_Listener>();
                if( this.blockchain_listener == null )
                {
                    Debug.LogWarning( "No blockchain listener found" );
                    return;
                }
            }
            
            this.blockchain_listener.On_Transaction += this.On_Transaction_Handler;
            this.transactions = new Queue<Blockchain_Listener.Transaction>();
        }

        protected virtual void Update()
        {
            lock( this.transactions )
            {
                while( this.transactions.Count > 0 )
                {
                    Blockchain_Listener.Transaction transaction = this.transactions.Dequeue();
                    this.Process_Transaction( transaction );
                }
            }
        }

        private void On_Transaction_Handler( Blockchain_Listener.Transaction transaction )
        {
            lock( this.transactions )
            {
                this.transactions.Enqueue( transaction );
            }
        }

        protected abstract void Process_Transaction( Blockchain_Listener.Transaction transaction );
    }
}
