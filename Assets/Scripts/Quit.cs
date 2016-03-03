using UnityEngine;
using System.Collections;

public class Quit : MonoBehaviour 
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
