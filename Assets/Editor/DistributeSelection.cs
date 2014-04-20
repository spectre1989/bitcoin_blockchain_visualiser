using System;
using UnityEditor;
using UnityEngine;

public class DistributeSelection : EditorWindow
{
    [MenuItem( "Custom/Distribute Selection" )]
    private static void Init()
    {
        EditorWindow.GetWindow<DistributeSelection>();
    }

    private float y;
    private bool distribute;

    private void OnGUI()
    {
        this.y = EditorGUILayout.FloatField( "Distribute In Y", this.y );
        this.distribute = EditorGUILayout.Toggle( "Distribute", this.distribute );
    }

    private void Update()
    {
        if( this.distribute )
        {
            UnityEngine.Object[] transforms = Selection.GetFiltered( typeof( Transform ), SelectionMode.TopLevel );

            Array.Sort( transforms, delegate( UnityEngine.Object a, UnityEngine.Object b )
            {
                return a.name.CompareTo( b.name );
            } );

            float gapPerElement = this.y / (float)transforms.Length;

            for( int i = 1; i < transforms.Length; ++i )
            {
                Transform previousTransform = transforms[i - 1] as Transform;
                Transform transform = transforms[i] as Transform;

                transform.localPosition = previousTransform.localPosition + new Vector3( 0f, gapPerElement, 0f );
            }
        }
    }
}
