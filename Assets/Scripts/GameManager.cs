using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager> 
{
    public CharacterStats playerStats;

    private CinemachineFreeLook followCamera;

    List<IEndGameObsever> endGameObsevers = new List<IEndGameObsever>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void RigisterPlayer(CharacterStats player)
    {
        playerStats = player;

        followCamera = FindObjectOfType<CinemachineFreeLook>();

        if (followCamera != null)
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    public void AddObsever(IEndGameObsever observer)
    {
        endGameObsevers.Add(observer);
    }

    public void RemoveObsever(IEndGameObsever observer)
    {
        endGameObsevers.Remove(observer);
    }

    public void NotifyObservers()
    {
        
        foreach (var observer in endGameObsevers)
        {
            observer.EndNotify();
            
        }
    }

    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDesition>())
        {
            if (item.destinationTag == TransitionDesition.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}
