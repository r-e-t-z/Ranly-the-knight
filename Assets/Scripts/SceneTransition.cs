using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Номер Сцены")]
    public int sceneNumber;
    public void Transition()
    {
        SceneManager.LoadScene(sceneNumber);
    }

 
}
