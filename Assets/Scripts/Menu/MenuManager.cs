//using System.Collections.Generic;
//using UnityEngine;

//public class MenuManager : MonoBehaviour
//{
//    public List<GenericWindow> menus;  
//    public Menus defaultMenu = Menus.MainMenuSelect;

//    public Menus CurrentMenu { get; private set; }

//    private void Start()
//    {
//        foreach (var m in menus)
//        {
//            m.Init(this);
//            m.gameObject.SetActive(false);
//        }

//        CurrentMenu = defaultMenu;
//        menus[(int)CurrentMenu].Open();
//    }

//    public void Open(Menus id)
//    {
//        menus[(int)CurrentMenu].Close();
//        CurrentMenu = id;
//        menus[(int)CurrentMenu].Open(); 
//    }

//}

using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public List<GenericWindow> menus;
    public Menus defaultMenu = Menus.MainMenuSelect;

    public Menus CurrentMenu { get; private set; }

    private void Start()
    {
        // --- 여기서 enum 개수랑 매핑 로그 출력 ---
        var enumCount = Enum.GetNames(typeof(Menus)).Length;
        Debug.Log($"[MenuManager] Menus enum count={enumCount}, menus.Count={menus.Count}");

        for (int i = 0; i < menus.Count; i++)
        {
            var m = menus[i];
            var menuName = (i < enumCount) ? ((Menus)i).ToString() : "(out of range)";
            Debug.Log($"[MenuManager] menus[{i}] => {(m ? m.name : "NULL")} (Enum:{menuName})");
        }
        // ------------------------------------------

        foreach (var m in menus)
        {
            if (m == null)
            {
                Debug.LogError("[MenuManager] menus 안에 NULL 항목 있음!");
                continue;
            }

            m.Init(this);
            m.gameObject.SetActive(false);
        }

        CurrentMenu = defaultMenu;
        Debug.Log($"[MenuManager] Default menu: {CurrentMenu}");
        menus[(int)CurrentMenu].Open();
    }

    public void Open(Menus id)
    {
        Debug.Log($"[MenuManager] Open {CurrentMenu} -> {id}");
        menus[(int)CurrentMenu].Close();
        CurrentMenu = id;
        menus[(int)CurrentMenu].Open();
    }
}
