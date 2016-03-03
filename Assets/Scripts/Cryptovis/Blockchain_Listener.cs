using MiniJSON;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
#if !UNITY_WEBPLAYER
using WebSocketSharp;
#endif

namespace Cryptovis
{
    public class Blockchain_Listener : MonoBehaviour
    {
        public class Transaction
        {
            public string Address { get; private set; }
            public float Amount { get; private set; }

            public Transaction( string address, float amount )
            {
                this.Address = address;
                this.Amount = amount;
            }
        }

        public event Action<Transaction> On_Transaction;

#if !UNITY_WEBPLAYER
        private WebSocket web_socket;

        private void Start()
        {
            this.web_socket = new WebSocket( "wss://ws.blockchain.info/inv" );
            this.web_socket.OnOpen += this.On_Open;
            this.web_socket.OnClose += this.On_Close;
            this.web_socket.OnMessage += this.On_Message;

            this.web_socket.ConnectAsync();
        }

        private void OnDestroy()
        {
            if( this.web_socket.IsConnected )
            {
                this.web_socket.Close();
            }
        }

        private void On_Open( object sender, EventArgs args )
        {
            Debug.Log( "On_Open" );
            this.web_socket.SendAsync( "{\"op\":\"unconfirmed_sub\"}", delegate( bool result ) 
            { 
                if( !result ) 
                { 
                    Debug.LogError( "Send Failed" ); 
                } 
            } );
        }

        private void On_Close( object sender, CloseEventArgs args )
        {
            Debug.Log( "On_Close" );
        }

        private void On_Message( object sender, MessageEventArgs args )
        {
            this.Process_Message( args.Data );
        }
#endif

        private void Process_Message( string message )
        {
            if( this.On_Transaction != null )
            {
                Dictionary<string, object> transaction_data = Json.Deserialize( message ) as Dictionary<string, object>;
                Dictionary<string, object> output = ( ( transaction_data["x"] as Dictionary<string, object> )["out"] as List<object> )[0] as Dictionary<string, object>;
                string address = output["addr"] as string;
                float amount = (float)((Int64)output["value"]) * 0.00000001f;
                
                Transaction transaction = new Transaction( address, amount );
                Debug.Log( "on_transaction" );
                this.On_Transaction( transaction );
            }
        }
    }
}