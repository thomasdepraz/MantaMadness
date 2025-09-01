using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class FairyTrail : MonoBehaviour
{
    [SerializeField] private SplineAnimate splinePlayer;
    [SerializeField] private SplineContainer path;
    [SerializeField] private ParticleSystem sparkles;
    [SerializeField] private Transform body;
    [SerializeField] private GameObject[] toActivate;

    private bool hasActivated =  false;

    private void Start()
    {
        if (toActivate.Length > 0) 
        {
            foreach (var item in toActivate)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(hasActivated == false)
            {
                hasActivated = true;
                StartCoroutine(spawnCoroutine());
                StartCoroutine(Timer());
            }
        }
    }

    private IEnumerator spawnCoroutine()
    {
        splinePlayer.Play();

        for (int i = 0; i < toActivate.Length; i++)
        {
            if(i == 0)
            {
                toActivate[i].gameObject.SetActive(true);
                toActivate[i].gameObject.transform.position = body.transform.position;
                yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 3));
            }        
            //else if (i == toActivate.Length - 2)
            //{
            //    toActivate[i].gameObject.SetActive(true);
            //    toActivate[i].gameObject.transform.position = body.transform.position;
            //    yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 1));
            //}
            else
            {
                toActivate[i].gameObject.SetActive(true);
                toActivate[i].gameObject.transform.position = body.transform.position;
                yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 3));
            }
        }
        yield return null;
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(splinePlayer.Duration);
        splinePlayer.Pause();
        sparkles.Stop();
    }

}
