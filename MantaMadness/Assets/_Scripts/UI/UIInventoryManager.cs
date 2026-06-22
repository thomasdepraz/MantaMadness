using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UIInventoryManager : MonoBehaviour
{
    //CE SCRIPT GERE l'affichage des items déjà obtenu par le joueur dans son inventaire
    //EN gros au launch > ca gere les mats des items selon si il on etait obtenu ou non
    // A chaque fois qu'un item importan est obtenu, ca update l'affichage


    public List<UIInventoryItems> inventoryItems;

}
