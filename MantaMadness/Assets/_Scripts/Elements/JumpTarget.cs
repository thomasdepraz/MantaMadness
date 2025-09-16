using System.Collections;
using UnityEngine;

public class JumpTarget : MonoBehaviour
{
    private SimpleController player;
    [SerializeField] LayerMask playerMask;

    [SerializeField] private ParticleSystem indicator;
    [SerializeField] private Material[] materials;
    [SerializeField] private float respawnCooldown = 1f;

    private void Start()
    {
        player = Game.Instance.player;
    }
    
    public void SwitchIndicatorVisibility(bool validTarget)
    {
        if (!validTarget)
        {
            GetComponent<MeshRenderer>().material = materials[0];
            indicator.Stop();
            indicator.gameObject.SetActive(false);

        }
        else if (validTarget)
        {

            GetComponent<MeshRenderer>().material = materials[1];
            indicator.gameObject.SetActive(true);
            indicator.Play();
        }
    }

    public void DeactivateTarget()
    {
        StartCoroutine(DisableCoroutine());
    }

    private IEnumerator DisableCoroutine()
    {
        ToggleFunctionElements(false);
        yield return new WaitForSeconds(respawnCooldown);
        ToggleFunctionElements(true);
        yield return null;
    }

    private void ToggleFunctionElements(bool toggleValue)
    {
        if (toggleValue)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<Collider>().enabled = true;
            indicator.gameObject.SetActive(true);
        }
        else if (!toggleValue)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
            indicator.gameObject.SetActive(false);
        }

    }
}
