using UnityEngine;
using System.Collections;

public class BasicUI : MonoBehaviour 
{
    public string text;

    private void OnGUI()
    {
        GUILayout.Label( this.text.Replace( "\\n", "\n" ) );
    }
}
