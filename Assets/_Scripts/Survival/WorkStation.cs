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
		stationManager = StationManager.Instance;
	}

	public void Work()
	{
		foreach (var item in StationManager.Instance.Rooms)
		{
			item.myTank.amount -= item.myTank.reqAmount * level;
		}
		StationManager.Instance.Points += addPoints; // This was already correct!
	}
}