using Newtonsoft.Json.Linq;
using System;
using System.Net;
using UnityEngine;
using WebSocketSharp;

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
        private WebSocket web_socket;

        private void Start()
        {
            this.web_socket = new WebSocket( "ws://ws.blockchain.info/inv" );
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
            if( this.On_Transaction != null )
            {
                JObject transaction_json = JObject.Parse( args.Data );
                //JArray inputs_json = transaction_json["x"]["inputs"] as JArray;
                JArray outputs_json = transaction_json["x"]["out"] as JArray;

                string address = outputs_json[0]["addr"].ToString();
                float amount = outputs_json[0]["value"].ToObject<float>() * 0.00000001f;
                Transaction transaction = new Transaction( address, amount );

                this.On_Transaction( transaction );
            }
        }
    }
}