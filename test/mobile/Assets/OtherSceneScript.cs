using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OtherSceneScript : MonoBehaviour {

	public void ReturnToSample() {
        SceneManager.LoadScene("SampleScene");
    }
}
