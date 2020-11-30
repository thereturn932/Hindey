using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtons : MonoBehaviour
{
    public Building building;
    public GameObject gameManager;
    public Text counter;
    private int count = 0;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
    }

    public void OnBuildButtonPressed()
    {

        if (gameManager.GetComponent<GameManager>().myCountry.money >= building.buildingCost)
        {
            if (building.maxBuilding == 0)
            {
                gameManager.GetComponent<GameManager>().myCountry.myBuildings.Add(building);
                gameManager.GetComponent<GameManager>().myCountry.money -= building.buildingCost;
                count++;
                counter.text = count.ToString();
                StartCoroutine(gameManager.GetComponent<GameManager>().WarningMessager(building.name + "builded.", 1));
            }
            else
            {
                foreach (Building build in gameManager.GetComponent<GameManager>().myCountry.myBuildings)
                {
                    var noOfBuilding = 0;
                    if (build == building)
                    {
                        noOfBuilding++;
                        if (noOfBuilding >= building.maxBuilding)
                        {
                            StartCoroutine(gameManager.GetComponent<GameManager>().WarningMessager("Maximum Amount of Building", 2));
                            return;
                        }
                    }
                }
                gameManager.GetComponent<GameManager>().myCountry.myBuildings.Add(building);
                gameManager.GetComponent<GameManager>().myCountry.money -= building.buildingCost;
                count++;
                counter.text = count.ToString();
                StartCoroutine(gameManager.GetComponent<GameManager>().WarningMessager(building.name + "builded.", 1));
            } 
        }
        else
        {
            StartCoroutine(gameManager.GetComponent<GameManager>().WarningMessager("Not Enough Money", 1));
        }
        
    }

    public void OnDestroyButtonPressed()
    {
        print("Destroy" + building.buildingName);
        foreach (var build in gameManager.GetComponent<GameManager>().myCountry.myBuildings)
        {
            if (build == building)
            {
                gameManager.GetComponent<GameManager>().myCountry.myBuildings.Remove(build);
                count--;
                counter.text = count.ToString();
                StartCoroutine(gameManager.GetComponent<GameManager>().WarningMessager(building.name + "destroyed.", 1));
                return;
            }
        }
    }
}
