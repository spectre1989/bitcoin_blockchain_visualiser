using UnityEngine;

namespace Cryptovis
{
    public class Test_Visualiser : Visualiser
    {
        public float min_size;
        public float size_per_btc;
        public float radius;

        protected override void Process_Transaction( Blockchain_Listener.Transaction transaction )
        {
            Vector3 spawn_position = UnityEngine.Random.onUnitSphere * this.radius;
            spawn_position.y = 0f;
            spawn_position += this.transform.position;

            float size = Mathf.Max( this.min_size, transaction.Amount * this.size_per_btc );

            Transform spawned = GameObject.CreatePrimitive( PrimitiveType.Sphere ).transform;
            spawned.position = spawn_position;
            spawned.localScale = Vector3.one * size;
        }
    }
}