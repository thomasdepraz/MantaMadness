using UnityEngine;

public class TrililiStatueAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int maxAnimCount;
    [SerializeField] private int animID;
    [SerializeField] private bool fixedAnim = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        SetAnim();
    }

    public void SetAnim()
    {
        if(fixedAnim)
        {
            animator.SetInteger("Pose", animID);
        }
        else
        {
            animator.SetInteger("Pose", (int)Random.Range(0, maxAnimCount));
        }
    }
}
