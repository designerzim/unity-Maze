using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour 
{

    // public variables to be set in Unity UI
    // size
    public IntVector2 size;

    // building process
	public float generationStepDelay;
    [Range(0f, 1f)]
    public float doorProbability;

    // components
	public MazeCell cellPrefab;
    public MazeDoor doorPrefab;
	public MazePassage passagePrefab;
	public MazeWall[] wallPrefabs;
    public MazeRoomSettings[] roomSettings;

	// private variables for maze building
	private MazeCell[,] cells;
	private int wallCounter = 0;
    private List<MazeRoom> rooms = new List<MazeRoom>();

    /// <summary>
    /// Gets the random coordinates for an IntVector2
    /// </summary>
    /// <value>Generates random coordinates within Maze size x & y.</value>
    public IntVector2 RandomCoordinates
	{
		get {
			return new IntVector2(Random.Range(0, size.x), 
			                      Random.Range(0, size.z));
		}
	}

	/// <summary>
	/// Checks that IntVector2 coordinates are within the maze bounds.
	/// </summary>
	/// <returns><c>true</c>, if coordinates within maze, <c>false</c> otherwise.</returns>
	/// <param name="coordinate">Coordinate.</param>
	public bool ContainsCoordinates (IntVector2 coordinate)
	{
		return coordinate.x >= 0 && coordinate.x < size.x &&
		coordinate.z >= 0 && coordinate.z < size.z;
	}

	/// <summary>
	/// Gets coordinates of MazeCell at specified IntVector2 location.
	/// </summary>
	/// <returns>The cell coordinates.</returns>
	/// <param name="coordinates">IntVector2</param>
	public MazeCell GetCell (IntVector2 coordinates)
	{
		return cells [coordinates.x, coordinates.z];
	}

	/// <summary>
	/// Generating an instance with IEnumerator co-routines to allow a delay.
	/// </summary>
	public IEnumerator Generate ()
	{
		WaitForSeconds delay = new WaitForSeconds (generationStepDelay);
		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell> ();
		
		DoFirstGenerationStep (activeCells);
		while (activeCells.Count > 0) 
		{
			yield return delay;
			DoNextGenerationStep(activeCells);
		}
        for (int i=0; i < rooms.Count; i++)
        {
            rooms[i].Hide();
        }
	}

    /// <summary>
    /// Begins Maze creation at a random location with the Maze size.
    /// </summary>
    /// <param name="activeCells">array of MazeCell</MazeCell></param>
	private void DoFirstGenerationStep (List<MazeCell> activeCells)
	{
        MazeCell newCell = CreateCell(RandomCoordinates);
        newCell.Initialize(CreateRoom(-1));
		activeCells.Add (newCell);
	}

    /// <summary>
    /// Steps through MazeCell array to create all remaining cells.
    /// </summary>
    /// <param name="activeCells">array of MazeCell</param>
	private void DoNextGenerationStep(List<MazeCell> activeCells)
	{
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells [currentIndex];
		if (currentCell.IsFullyInitialized)
		{
			activeCells.RemoveAt(currentIndex);
			return;
		}

		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2 ();

		if (ContainsCoordinates (coordinates)) 
		{
			MazeCell neighbor = GetCell (coordinates);
			if (neighbor == null)
            {
				neighbor = CreateCell (coordinates);
				CreatePassage (currentCell, neighbor, direction);
				activeCells.Add (neighbor);
			}
            else if (currentCell.room.settingsIndex == neighbor.room.settingsIndex)
            {
                CreatePassageInSameRoom(currentCell, neighbor, direction);
            }
            else
            {
				CreateWall (currentCell, neighbor, direction);
			}
		} else
		{
			CreateWall(currentCell,null,direction);
		}
	}
	
	private MazeCell CreateCell (IntVector2 coordinates)
	{
		MazeCell newCell = Instantiate (cellPrefab) as MazeCell;
		cells [coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3 (coordinates.x - size.x * 0.5f + 0.5f, 0f,
		                                               coordinates.z - size.z * 0.5f + 0.5f);
		return newCell;
	}

	private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
        MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
        MazePassage passage = Instantiate (prefab) as MazePassage;
		passage.Initialize (cell, otherCell, direction);
		passage = Instantiate (prefab) as MazePassage;
        if (passage is MazeDoor)
        {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
        }
        else
        {
            otherCell.Initialize(cell.room);
        }
		passage.Initialize (otherCell, cell, direction.GetOpposite ());
	}

    private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room)
        {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        }
    }

    /// <summary>
    /// Wall Builder
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="otherCell"></param>
    /// <param name="direction"></param>
    private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazeWall wall = Instantiate (wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
		wallCounter++;
		wall.Initialize (cell, otherCell, direction);
		if (otherCell != null) 
		{
			wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
			wall.Initialize(otherCell,cell,direction.GetOpposite());
			wallCounter++;
		}
	}

    /// <summary>
    /// Creat Roomstyles
    /// </summary>
    /// <param name="indexToExclude">int</param>
    /// <returns>Mazeroom</returns>
    private MazeRoom CreateRoom(int indexToExclude)
    {
        MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
        newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
        if (newRoom.settingsIndex == indexToExclude)
        {
            newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
        }
        newRoom.settings = roomSettings[newRoom.settingsIndex];
        rooms.Add(newRoom);
        return newRoom;
    }


}
