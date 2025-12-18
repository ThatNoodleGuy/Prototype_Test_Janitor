using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkStation : MonoBehaviour
{
	StationManager stationManager;
	public int level;
	public float addPoints = 3;
	public float upgradePerc = 3.5f;
	public float upgradeCost;
	public float baseCost = 500;

	public void UpgradeWorkStation()
	{
		addPoints *= upgradePerc * level;
		level++;
	}

	private void Update()
	{
		//calculate upgrade cost
		upgradeCost = baseCost * level;
	}

	private void Start()
	{
		stationManager = FindObjectOfType<StationManager>();
	}

	public void Work()
	{
		foreach (var item in stationManager.rooms)
		{
			item.myTank.amount -= item.myTank.reqAmount * level;
		}
		stationManager.points += addPoints;
	}

}
