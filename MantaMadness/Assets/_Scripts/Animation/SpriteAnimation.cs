using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    [SerializeField] private Material[] materials;
    

    void Start()
    {
        transform.GetComponent<SpriteRenderer>().material = materials[Random.Range(0,materials.Length)];
    }

}
