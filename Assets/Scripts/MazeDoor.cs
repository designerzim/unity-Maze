using UnityEngine;

public class MazeDoor : MazePassage
{

    public Transform hinge;

    private MazeDoor OtherSideOfDoor
    {
        get { return otherCell.GetEdge(direction.GetOpposite()) as MazeDoor; }
    }

    public override void Initialize (MazeCell primary, MazeCell other, MazeDirection direction)
    {
        float localX = hinge.localScale.x;
        float localY = hinge.localScale.y;
        float localZ = hinge.localScale.z;

        base.Initialize(primary, other, direction);
        if (OtherSideOfDoor != null)
        {
            hinge.localScale = new Vector3(-localX, localY, localZ);
            Vector3 p = hinge.localPosition;
            p.x *= -1;
            hinge.localPosition = p;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != hinge)
            {
                child.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
            }
        }
    }
}
