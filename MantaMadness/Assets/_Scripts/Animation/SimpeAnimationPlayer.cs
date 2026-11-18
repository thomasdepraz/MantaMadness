using UnityEngine;

public class SimpeAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public string clipName;

    private void OnEnable()
    {
        if(GetComponent<Animator>() != null)
        {
            animator = GetComponent<Animator>();
        }
        if(animator != null)
        {
            animator.Play(clipName);
        }
    }
}
