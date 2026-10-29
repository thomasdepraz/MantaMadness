using UnityEngine;
using System.Collections;

public enum TutorialType
{
    NearRail,
    OnRailLeft,
    OnRailRight,
    Stomp,
    NearStomp,
    Alien,
    Spin,
    Dynamo,
    DynamoJump,
    DoubleJump,
    NearTarget,
    TargetJump,
}
public class TutorialBox : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;
    
    [SerializeField] private TutorialType tutorialType;

    private SimpleController player;

    private bool playerInside = false;

    private Coroutine checkCoroutine;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.2f;

    private void OnEnable()
    {
        checkCoroutine = StartCoroutine(CheckBoxRoutine());
        StartCoroutine(DelayEnable());
    }

    private IEnumerator DelayEnable()
    {
        yield return new WaitForSeconds(0.1f);
        player = Game.Instance.player;
    }

    private void OnDisable()
    {
        //StopAllCoroutines();
    }

    private void Update()
    {
        if (playerInside)
        {
            TutorialCheck();
        }
    }

    private IEnumerator CheckBoxRoutine()
    {
        CheckBox();

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;
            CheckBox();
        }
    }

    public void CheckBox()
    {
        bool isInZone = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInZone == playerInside)
            return;

        if (!isInZone)
        {
            ResetBox();
        }

        Debug.Log("Player is in zone = " + isInZone);
        playerInside = isInZone;
    }
    private void ResetBox()
    {
        //if (playerInside) return;

        //if (UIManager.Instance.uiTutorialManager.isOn) return;

        UIManager.Instance.uiTutorialManager.ResetTutorialText();
    }


    public void TutorialCheck()
    {
        switch (tutorialType)
        {
            case TutorialType.OnRailLeft:
                CheckOnRail(tutorialType);
                break;
            case TutorialType.OnRailRight:
                CheckOnRail(tutorialType);
                break;
            case TutorialType.NearRail:
                CheckNearRail();
                break;
            case TutorialType.Alien:
                CheckAlien();
                break;
            case TutorialType.DoubleJump:
                CheckDoubleJump();
                break;
            case TutorialType.TargetJump:
                CheckTargetJump();
                break;
            case TutorialType.Dynamo:
                CheckDynamo();
                break;
            case TutorialType.Stomp:
                CheckStomp();
                break;
            case TutorialType.Spin:
                CheckSpin();
                break;
        }
    }

    private void CheckNearRail()
    {
        //SI LE JOUEUR n'a pas la comp grind > affiche une UI qui dit "Need the frutti wings to use"
        if(player.grindAbility == false)
        {
            UIManager.Instance.uiTutorialManager.SetText(TutorialType.NearRail);
        }
    }

    private void CheckOnRail(TutorialType type)
    {
        //SI le joueur est sur un rail, UI pour dire appui sur input de dash au bon moment entre gauche et droite (rajouter un type a la box pour determiner)
        if(player.State == ControllerState.RAIL)
        {
            if(type == TutorialType.OnRailLeft)
            {
                UIManager.Instance.uiTutorialManager.SetText(TutorialType.OnRailLeft);
            }
            else
            {
                UIManager.Instance.uiTutorialManager.SetText(TutorialType.OnRailRight);
            }

        }
    }

    private void CheckAlien()
    {
        //SI le joueur est en stomp state ET qu'il arrive dans la action window STOMP LANDING > UI pour dire appui sur JUMP
        if (player.alienAntennasAbility == true)
        {
            if(player.activeActionWindow != null)
            {
                if (player.activeActionWindow.Type == ActionWindowType.StompLand)
                {
                    UIManager.Instance.uiTutorialManager.SetText(TutorialType.Alien);
                }
            }
        }
    }

    private void CheckDoubleJump()
    {
        // Si le joueur est en jump > UI pour dire jump maintemant pour double jump
    }

    private void CheckTargetJump()
    {
        // Si le joueur est proche d'une target (target in range) > UI pour dire jump maintemant pour target jump

        if(player.doubleJumpAbility == true)
        {
            if(player.State == ControllerState.JUMPING || player.State == ControllerState.FALLING)
            {
                if(CameraTargetDetection.Instance.validJumpTargets.Count >= 1)
                {
                    UIManager.Instance.uiTutorialManager.SetText(TutorialType.TargetJump);
                }
            }
        }
        //Else si le joueur n'à pas la capacité > UI need glasses
        else
        {
            UIManager.Instance.uiTutorialManager.SetText(TutorialType.NearTarget);
        }
    }

    private void CheckDynamo()
    {
        if(player.dynamoAbility == true)
        {
            //Si le joueur est proche d'une metal fence mais pas chargé > UI pour dire hold R2 to charge

            //Si le joueur est proche d'une metal fence et chargé > Ui pour dire double jump through
            if(player.electricBehaviour.IsCharged)
            {
                UIManager.Instance.uiTutorialManager.SetText(TutorialType.DynamoJump);
            }
            else
            {
                UIManager.Instance.uiTutorialManager.SetText(TutorialType.Dynamo);
            }
        }
    }

    private void CheckStomp()
    {
        if(player.stompAbility == true)
        {
            if (player.State == ControllerState.FALLING || player.State == ControllerState.JUMPING)
            {
                UIManager.Instance.uiTutorialManager.SetText(TutorialType.Stomp);
            }
        }
        else
        {
            UIManager.Instance.uiTutorialManager.SetText(TutorialType.NearStomp);
        }
    }

    private void CheckSpin()
    {
        //Si le joueur est proche d'un objet breakable > Ui pour dire hold R2 to spin
        UIManager.Instance.uiTutorialManager.SetText(TutorialType.Spin);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (center == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(
            center.position,
            center.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}
