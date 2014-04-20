using System;
using UnityEngine;

public class Donation_Button : MonoBehaviour 
{
    public UILabel label;

    private void OnClick()
    {
        if( this.label == null )
        {
            this.label = this.GetComponentInChildren<UILabel>();
            if( this.label == null )
            {
                Debug.LogWarning( "No label found" );
                return;
            }
        }

        string[] split = this.label.text.Split( new string[]{": "}, StringSplitOptions.None );
        string donation_address = split[1];

        TextEditor textEditor = new TextEditor();
        textEditor.content.text = donation_address;
        textEditor.SelectAll();
        textEditor.Copy();
    }
}
