using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Prize : MonoBehaviour
{

    public TMP_Dropdown dropdownMenu;
    // Start is called before the first frame update
    void Start()
    {
        dropdownMenu.gameObject.SetActive(false);
    }

    public void EnableDropdown()
    {
        dropdownMenu.gameObject.SetActive(true);
    }

}
