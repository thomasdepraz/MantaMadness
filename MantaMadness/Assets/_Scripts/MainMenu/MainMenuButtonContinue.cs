using UnityEngine;

public class MainMenuButtonContinue : MainMenuButton 
{

    [SerializeField] private Material disabledMat;

    public void setMatDisabled()
    {
        GetComponent<MeshRenderer>().material = disabledMat;
    }
}
