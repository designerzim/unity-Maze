using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{

	public Maze mazePrefab;
    public Player playerPrefab;

    private Player playerInstance;
	private Maze mazeInstance;

	// Use this for initialization
	void Start () {
		StartCoroutine(BeginGame());
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyDown (KeyCode.Space)) {
			RestartGame();
		}

	}

	private IEnumerator BeginGame ()
	{
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
		mazeInstance = Instantiate (mazePrefab) as Maze;
		yield return StartCoroutine(mazeInstance.Generate ());
        playerInstance = Instantiate(playerPrefab) as Player;
        playerInstance.SetLocation(mazeInstance.GetCell(mazeInstance.RandomCoordinates));
        string playerLocation = "player at " + playerInstance.GetLocation().x + "X & " + playerInstance.GetLocation().z + "Z";
        Debug.Log("Player Made at " + playerLocation);
        Camera.main.clearFlags = CameraClearFlags.Depth;
        Camera.main.rect = new Rect(0f, 0f, 0.5f, 0.5f);
	}

	private void RestartGame ()
	{
		StopAllCoroutines ();
		Destroy (mazeInstance.gameObject);
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
		BeginGame ();
	}
}
