﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTowerController : MonoBehaviour {

    public GameObject LevelQuad;

    public float ShootingRange = 10;
    public float reloadTime = 1f;

    public float damage = 0.3f;

    private LayerMask EnemiesMask = 1 << 10;

    float lastShotTime;

    public TowerAtLevel towerAtLevel;

    // Use this for initialization
    void Start () {
        lastShotTime = -reloadTime;
    }
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<TowerController>().placed && Physics.CheckSphere(transform.position, ShootingRange, EnemiesMask))
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, ShootingRange, EnemiesMask);
            if (Time.time - lastShotTime > reloadTime)
            {
                int targetNum = ChooseTarget(hitColliders);
                if (targetNum != -1)
                {
                    ShotInfo spot = ChooseSpot(hitColliders[targetNum]);
                    GetComponent<Shoot>().ShootAtTarget(spot);
                    lastShotTime = Time.time;
                }
            }
        }
    }

    private int ChooseTarget(Collider[] hitColliders)
    {
        int targetNum = 0;
        for (int i = 1; i < hitColliders.Length; i++)
        {
            if (!GetComponent<Shoot>().mobsBeingShot.Contains(hitColliders[i].GetComponent<MonsterController>().ID)
                && hitColliders[i].GetComponent<MonsterController>().energy >= 0.05
                && hitColliders[i].GetComponent<MonsterController>().CreationTime < hitColliders[targetNum].GetComponent<MonsterController>().CreationTime
                && !hitColliders[i].GetComponent<MonsterController>().invinsible)
                targetNum = i;
        }
        if (hitColliders[targetNum].GetComponent<MonsterController>().energy < 0.05 || GetComponent<Shoot>().mobsBeingShot.Contains(hitColliders[targetNum].GetComponent<MonsterController>().ID))
            return -1;
        else
            return targetNum;
    }

    private ShotInfo ChooseSpot(Collider target)
    {
        ShotInfo shotInfo = new ShotInfo(target.transform.position, target.transform.TransformDirection(Vector3.forward), target.GetComponent<Unit>().speed, target.GetComponent<MonsterController>().ID);
        return shotInfo;
    }

    public float GetDamage()
    {
        return damage*towerAtLevel.damage_coef;
    }

    public bool CanUpgrade()
    {
        return towerAtLevel.nextUpdateCost != -1 && towerAtLevel.nextUpdateCost <= DataStorage.dataStorage.coins;
    }

    public void Upgrade()
    {
        GameObject.Find("Coins Text").GetComponent<CoinsController>().SubtractCoinsForUpgrade(towerAtLevel.nextUpdateCost);
        GetComponent<TowerController>().level++;
        towerAtLevel = GameObject.Find("UpgradeController").GetComponent<UpgradeController>().towersLevelInfo[GetComponent<TowerController>().type][GetComponent<TowerController>().level - 1];
        LevelQuad.GetComponent<Renderer>().material = towerAtLevel.material;
    }
}
