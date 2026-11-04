using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class GoldenCar : Car
{
    [SerializeField] private string coinName;

    [SerializeField] private string NpcName;
    [SerializeField] private int NpcUpdateState;

    protected override void OnCarDestroyed()
    {
        if(NpcName != null && NpcUpdateState != 0)
        {
            UpdateNPC();
        }
        CoinManager.Instance.ActivateCoinHolder(coinName);
    }

    public override IEnumerator KillSequence()
    {
        yield return base.KillSequence();
    }

    protected override void CollisionCheck(string type)
    {
        if (isAlive == true)
        {
            if (type == "player")
            {
                switch (carType)
                {
                    case CarType.Truck:
                        player.Kill(DeathType.FLATTEN);
                        return;
                }

                if (player.HorizontalVelocity.magnitude > player.controllerData.maxSpeed / 2 || splineAnimate == null)
                {
                    StartCoroutine(KillSequence());
                }
                else
                {
                    if (splineAnimate != null)
                    {
                        Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
                    }
                }
            }
        }
    }

    private void UpdateNPC()
    {
        Debug.Log("Oue c'est greg");
        NPCManager.instance.UpdateNPCState(NpcName, NpcUpdateState);
    }
}