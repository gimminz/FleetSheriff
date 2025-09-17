using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public List<GenericWindow> menus;  
    public Menus defaultMenu = Menus.MainMenuSelect;

    public Menus CurrentMenu { get; private set; }

    private void Start()
    {
        foreach (var m in menus)
        {
            m.Init(this);
            m.gameObject.SetActive(false);
        }

        CurrentMenu = defaultMenu;
        menus[(int)CurrentMenu].Open();
    }

    public void Open(Menus id)
    {
        menus[(int)CurrentMenu].Close();
        CurrentMenu = id;
        menus[(int)CurrentMenu].Open(); 
    }
}
