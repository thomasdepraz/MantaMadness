using UnityEngine;

public class SimpeAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public string clipName;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play(clipName);
    }
}
