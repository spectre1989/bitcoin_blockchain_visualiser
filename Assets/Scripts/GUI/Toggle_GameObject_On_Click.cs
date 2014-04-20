using UnityEngine;
using System.Collections;

public class Toggle_GameObject_On_Click : MonoBehaviour 
{
    public GameObject[] targets;

    private void OnClick()
    {
        foreach( GameObject target in this.targets )
        {
            if( target != null )
            {
                target.SetActive( !target.activeSelf );
            }
        }
    }
}
