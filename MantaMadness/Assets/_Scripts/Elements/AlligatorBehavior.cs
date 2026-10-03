using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AlligatorBehavior : MonoBehaviour
{
    private bool isActive;

    public float speed = 20f;
    public float lifeTime = 5f;

    private Vector3 direction;

    private Vector3 originalScale;

    private bool isDead = false;

    [SerializeField] private ParticleSystem spawnParticle;
    [SerializeField] private GameObject[] activeObjects;

    private Coroutine spawnRoutine;

    public void Init(Vector3 dir)
    {
        direction = dir;
        transform.forward = direction;
    }

    public void Start()
    {
        isActive = false;
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, 0.25f).SetEase(Ease.OutBounce);
        spawnRoutine = StartCoroutine(SpawnCoroutine());
    }

    private void Update()
    {
        if (!isActive) return;

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

    private IEnumerator SpawnCoroutine()
    {
        spawnParticle.Play();
        foreach (GameObject g in activeObjects)
        {
            g.SetActive(false);
        }

        yield return new WaitForSeconds(1.2f);

        spawnParticle.Stop();
        foreach(GameObject g in activeObjects)
        {
            g.SetActive(true);
        }

        isActive = true;
    }

    private void Death()
    {
        transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBounce).OnComplete(() =>
        {
            Destroy(this.gameObject);
        });

    }
}
