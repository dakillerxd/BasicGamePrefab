using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter2D : MonoBehaviour
{

    [SerializeField] private SceneField sceneToLoad;


    public void GoToSelectedLevel() {
        if (sceneToLoad != null) {
            SceneManager.LoadScene(sceneToLoad.SceneName);
        }
    }
}
