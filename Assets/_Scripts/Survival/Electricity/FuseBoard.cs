using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseBoard : MonoBehaviour
{
    //public Transform board;
    public RoomController powerRoomPC;

    float rowGap = 0.25f;
    float colGap = 0.25f;

    public GameObject fusePrefab;

    public ElectricFuse[] fuses;


    private void Update()
    {

        if (!powerRoomPC.HasRecource())
        {
            if (fuses.Length == 0)
            {
                InstantiateBoard();
            }

        }

        powerRoomPC.AlertMassage(ErrorList());

        if (powerRoomPC.isGoingOut && powerRoomPC.alertMsg.text == powerRoomPC.baseMsg)
        {
            DestroyBoard();
        }
    }


    public ElectricFuse[] InstantiateBoard()
    {
        int counter = 0;
        int gridSize = powerRoomPC.myTank.level + 1;
        gridSize = Mathf.Clamp(gridSize, 1, 9);
        fuses = new ElectricFuse[gridSize * gridSize];

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                fuses[counter] = Instantiate(fusePrefab, transform).GetComponent<ElectricFuse>();
                fuses[counter].transform.localPosition = new Vector3(colGap * c, rowGap * r, 0);
                float random = Random.value;
                if (random > 0.5)
                {
                    fuses[counter].ToggleSwitch();
                }
                fuses[counter].correctState = true;
                fuses[counter].name = "[R" + (r + 1) + ", C" + (c + 1) + "]";

                //insta base
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(.2f, .2f, .05f);
                cube.transform.position = fuses[counter].transform.position;
                cube.GetComponent<Renderer>().material.color = Color.yellow;
                cube.transform.parent = transform;

                counter++;
            }
        }

        for (int i = 0; i < powerRoomPC.myTank.level; i++)
        {
            int randomIndex = Random.Range(0, fuses.Length);
            fuses[randomIndex].correctState = false;
        }

        //transform.position = transform.parent.position;

        return fuses;
    }

    public void DestroyBoard()
    {
        foreach (Transform item in transform)
        {
            Destroy(item.gameObject);
        }
        fuses = new ElectricFuse[0];
    }

    public List<string> ErrorList()
    {
        List<string> names = new List<string>();
        string title = "Alert!! Can't fill storage!\nSearching Fuses set wrong way...\n.....\n...\nError Log [Row#,Col#]:\n";
        names.Add(title);
        foreach (var item in fuses)
        {
            if (!item.correctState)
            {
                names.Add(item.name);
            }
        }

        return names;
    }

}
