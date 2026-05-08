using UnityEngine;
using DG.Tweening;

public class AlligatorBehavior : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;

    private Vector3 direction;

    private Vector3 originalScale;

    private bool isDead = false;

    public void Init(Vector3 dir)
    {
        direction = dir;
        transform.forward = direction;
    }

    public void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, 0.25f).SetEase(Ease.OutBounce);    
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if(lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
        }
        else if (lifeTime <= 0 && !isDead)
        {
            isDead = true;
            Death();
        }
    }

    private void Death()
    {
        transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBounce).OnComplete(() =>
        {
            Destroy(this.gameObject);
        });

    }
}
