using System.Collections;
using UnityEngine;

namespace Cryptovis
{
    public class Fireworks_Visualiser : Visualiser
    {
        public GameObject particle_system_prefab;
        public Particle_System_Customisation[] particle_system_customisations;
        public float min_spawn_x;
        public float max_spawn_x;
        public float spawn_y;
        public float spawn_z;
        public GameObject amount_text_prefab;
        public float min_text_size;
        public float max_text_size;
        public float min_amount;
        public float max_amount;
        public new Camera camera;
        public Bang_Sound[] bang_sounds;

        protected override void Process_Transaction( Blockchain_Listener.Transaction transaction )
        {
            this.StartCoroutine( this.Process_Transaction_Coroutine( transaction ) );
        }

        private IEnumerator Process_Transaction_Coroutine( Blockchain_Listener.Transaction transaction )
        {
            // Wait a small amount of time, helps with many transactions arriving in clusters
            yield return new WaitForSeconds( Random.value );

            // Instantiate and position particle system
            Vector3 spawn_position = new Vector3( this.min_spawn_x + ( ( this.max_spawn_x - this.min_spawn_x ) * Random.value ), this.spawn_y, this.spawn_z );
            GameObject particle_system_gameobject = Object.Instantiate( this.particle_system_prefab ) as GameObject;
            particle_system_gameobject.transform.position = spawn_position;

            // Randomly select a colour (note only activate 2 of the 3 components so we don't
            // end up with washed out pastel colours)
            Color start_colour = new Color( 0f, 0f, 0f, 1f );
            float rand_value = Random.value;
            if( rand_value < 0.33333333333333333f )
            {
                start_colour.r = 1f;

                if( Random.value < 0.5f )
                {
                    start_colour.g = Random.value;
                }
                else
                {
                    start_colour.b = Random.value;
                }
            }
            else if( rand_value < 0.66666666666666667f )
            {
                start_colour.g = 1f;

                if( Random.value < 0.5f )
                {
                    start_colour.r = Random.value;
                }
                else
                {
                    start_colour.b = Random.value;
                }
            }
            else
            {
                start_colour.b = 1f;

                if( Random.value < 0.5f )
                {
                    start_colour.r = Random.value;
                }
                else
                {
                    start_colour.g = Random.value;
                }
            }

            ParticleSystem[] particle_systems = particle_system_gameobject.GetComponentsInChildren<ParticleSystem>();
            foreach( ParticleSystem particle_system in particle_systems )
            {
                particle_system.startColor = start_colour;
            }
            
            // Deal with particle system customisations
            float t = Mathf.InverseLerp( this.min_amount, this.max_amount, transaction.Amount );
            foreach( Particle_System_Customisation customisation in this.particle_system_customisations )
            {
                ParticleSystem particle_system;

                if( customisation.path == "" )
                {
                    particle_system = particle_system_gameobject.GetComponent<ParticleSystem>();
                }
                else
                {
                    Transform child = particle_system_gameobject.transform.Find( customisation.path );
                    if( child == null )
                    {
                        Debug.LogWarning( "Couldn't find child '" + customisation.path + "' on Transform " + particle_system_gameobject.transform );
                        continue;
                    }

                    particle_system = child.GetComponent<ParticleSystem>();
                }

                if( particle_system == null )
                {
                    Debug.LogWarning( "Particle System not found for '" + customisation.path + "'" );
                    continue;
                }
                
                if( customisation.override_emission_rate )
                {
                    particle_system.emissionRate = this.LerpIntRounded( customisation.min_emission_rate, customisation.max_emission_rate, t );
                }

                if( customisation.override_start_speed )
                {
                    particle_system.startSpeed = Mathf.Lerp( customisation.min_start_speed, customisation.max_start_speed, t );
                }
            }

            // Text
            string text = "";
            if( transaction.Amount < 0.001 )
            {
                text = "<0.001";
            }
            else if( transaction.Amount < 1f )
            {
                text = transaction.Amount.ToString( "0.000" );
            }
            else if( transaction.Amount < 10f )
            {
                text = transaction.Amount.ToString( "0.00" );
            }
            else if( transaction.Amount < 100f )
            {
                text = transaction.Amount.ToString( "00.0" );
            }
            else
            {
                text = transaction.Amount.ToString( "000." );
            }

            // Audio
            AudioClip audio_clip = null;
            float pitch = 1f;
            if( this.bang_sounds != null && this.bang_sounds.Length > 0 )
            {
                int index = 0;
                while( index < ( this.bang_sounds.Length - 1 ) &&
                       this.bang_sounds[index + 1].amount <= transaction.Amount )
                {
                    ++index;
                }

                audio_clip = this.bang_sounds[index].audio_clip;
                pitch = Random.Range( this.bang_sounds[index].min_pitch_shift, this.bang_sounds[index].max_pitch_shift );
            }

            ParticleSystem root_particle_system = particle_system_gameobject.GetComponent<ParticleSystem>();
            this.StartCoroutine( this.Do_Text_And_Audio( root_particle_system, text, Mathf.Lerp( this.min_text_size, this.max_text_size, t ), audio_clip, pitch ) );
            this.StartCoroutine( this.Destroy_Particle_System_When_Done( root_particle_system ) );
        }

        private IEnumerator Do_Text_And_Audio( ParticleSystem particle_system, string text, float text_size, AudioClip audio_clip, float pitch )
        {
            // Wait for the initial particle to be fired off
            while( particle_system.particleCount == 0 )
            {
                yield return null;
            }
            
            // Track it's position until it detonates
            ParticleSystem.Particle[] particle = new ParticleSystem.Particle[1];

            while( particle_system.particleCount > 0 )
            {
                particle_system.GetParticles( particle );
                yield return null;
            }

            // Create text/audio
            GameObject text_and_audio_gameobject = GameObject.Instantiate( this.amount_text_prefab ) as GameObject;

            // Set up text
            TextMesh text_mesh = text_and_audio_gameobject.GetComponentInChildren<TextMesh>();
            text_mesh.text = text;
            text_mesh.fontSize = Mathf.RoundToInt( text_size );
            text_and_audio_gameobject.transform.position = particle_system.transform.TransformPoint( particle[0].position ) + new Vector3( 0f, 25f, 0f );
            text_and_audio_gameobject.transform.LookAt( this.camera.transform );

            // Set up audio
            AudioSource audio_source = text_and_audio_gameobject.GetComponent<AudioSource>();
            audio_source.clip = audio_clip;
            audio_source.pitch = pitch;
            audio_source.Play();

            // Animate in using scale
            float t = 0f;
            while( t < 1f )
            {
                t += Time.deltaTime;
                text_and_audio_gameobject.transform.localScale = Vector3.one * this.EaseOut( 0f, 1f, t );
                yield return null;
            }

            // Animate out
            Vector3 from_position = text_and_audio_gameobject.transform.position;
            Vector3 to_position = from_position + new Vector3( 0f, 10f, 0f );
            Color from_colour = new Color( 1f, 1f, 1f, 1f );
            Color to_colour = new Color( 1f, 1f, 1f, 0f );
            t = 0f;
            while( t < 1f )
            {
                t += Time.deltaTime / 2f;
                text_and_audio_gameobject.transform.position = Vector3.Lerp( from_position, to_position, t );
                text_and_audio_gameobject.transform.LookAt( this.camera.transform );
                text_mesh.color = Color.Lerp( from_colour, to_colour, t );
                yield return null;
            }

            Object.Destroy( text_and_audio_gameobject );
        }

        private IEnumerator Destroy_Particle_System_When_Done( ParticleSystem particle_system )
        {
            while( particle_system.IsAlive() )
            {
                yield return new WaitForSeconds( 1f );
            }
            
            Object.Destroy( particle_system.gameObject );
        }

        private float EaseOut( float from, float to, float t )
        {
            return from + ( Mathf.Pow( Mathf.Clamp01( t ), 0.3f ) * ( to - from ) );
        }

        private int LerpIntRounded( int from, int to, float t )
        {
            if( t <= 0f )
            {
                return from;
            }
            else if( t >= 1f )
            {
                return to;
            }

            return from + Mathf.RoundToInt( (float)( to - from ) * t );
        }

        [System.Serializable]
        public class Particle_System_Customisation
        {
            public string path;
            public bool override_emission_rate;
            public int min_emission_rate;
            public int max_emission_rate;
            public bool override_start_speed;
            public float min_start_speed;
            public float max_start_speed;
        }

        [System.Serializable]
        public class Bang_Sound
        {
            public float amount;
            public AudioClip audio_clip;
            public float min_pitch_shift;
            public float max_pitch_shift;
        }
    }
}