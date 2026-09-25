using System.Drawing;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum HauntedType
{
    Break,
    Wall,
    Stay,
}
public class HauntedPuzzleElement : MonoBehaviour
{
    [Header("Components")]
    public HauntedTunnel tunnel;
    public HauntedType type;
    public int id;

    [Header("Stay Type Parameters")]
    bool wasInBox;
    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float checkInterval = 0.2f;
    private Coroutine checkCoroutine;

    public void CheckInteraction()
    {
        if(type  == HauntedType.Break)
        {
            Interaction(id);
        }
        else if(type == HauntedType.Wall)
        {
            Interaction(id);
        }
        else if(type== HauntedType.Stay)
        {
            Interaction(id);
        }
    }
    public void Interaction(int id)
    {
        tunnel.OnInteraction(id);
    }

    private void OnEnable()
    {
        checkCoroutine = StartCoroutine(CheckStayRoutine());
    }

    private void OnDisable()
    {
        if (checkCoroutine != null)
        {
            wasInBox = false;
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }
    

    private void FixedUpdate()
    {
        if(type == HauntedType.Stay)
        {
            CheckStay();

            if(wasInBox == true)
            {
                tunnel.ReduceTimer();
            }
        }
    }

    private IEnumerator CheckStayRoutine()
    {
        CheckStay();

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;
            CheckStay();
        }
    }

    public void CheckStay()
    {
        bool isInBox = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInBox == wasInBox)
            return;

        wasInBox = isInBox;

        if(!isInBox)
        {
            //TIMER RESET + STOP REDUCING TIMER
            tunnel.ResetTimer();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (type != HauntedType.Stay)
            return;

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
