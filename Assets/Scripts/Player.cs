using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    private MazeCell currentCell;
    private MazeDirection currentDirection;

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.E))
        {
            Move(currentDirection);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Move(currentDirection.GetNextClockwise());
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Move(currentDirection.GetOpposite());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Move(currentDirection.GetNextCounterclockwise());
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Rotate(currentDirection.GetNextCounterclockwise());
        }
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Rotate(currentDirection.GetNextClockwise());
        }
    }

    public void SetLocation(MazeCell cell)
    {
        if (currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition;
        currentCell.OnPlayerEntered();
    }

    public IntVector2 GetLocation()
    {
        return currentCell.coordinates;
    }

    private void Move(MazeDirection direction)
    {
        MazeCellEdge edge = currentCell.GetEdge(direction);
        if (edge is MazePassage)
        {
            SetLocation(edge.otherCell);
        }
    }

    private void Rotate (MazeDirection direction)
    {
        transform.localRotation = direction.ToRotation();
        currentDirection = direction;
    }
}
