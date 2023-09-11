using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHelper : MonoBehaviour
{
    [SerializeField] List<GameObject> testMenus = new List<GameObject>();
    [SerializeField] GameObject closeButton;

    public void OpenRelatedMenu(GameObject menu)
    {
        OpenCloseMenus(menu);
        closeButton.SetActive(true);
    }
    public void ReturnToMainMenu(GameObject menu)
    {
        OpenCloseMenus(menu);
        closeButton.SetActive(false);
    }

    private void OpenCloseMenus(GameObject menu)
    {
        for(int i = 0; i < testMenus.Count; i++)
        {
            testMenus[i].SetActive(false);
        }
        menu.SetActive(true);
    }
}
