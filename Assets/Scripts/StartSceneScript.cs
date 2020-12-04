using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneScript : MonoBehaviour
{
    // Start is called before the first frame update
   public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }
}
