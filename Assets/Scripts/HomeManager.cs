using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
	public string sceneToLoad = "Game";


	public void LoadGame()
	{
		SceneManager.LoadScene(sceneToLoad);
	}
}
