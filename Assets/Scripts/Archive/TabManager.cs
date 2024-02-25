using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    List<TabButton> tabs = new List<TabButton>();
    TabButton selectedTab;

    public void AddTabButton(TabButton newTab)
    {
        tabs.Add(newTab);
        newTab.tabWindow.SetActive(false);
        if(!selectedTab) SelectTab(newTab);
    }

    public void SelectTab(TabButton curTab)
    {
        selectedTab = curTab;
        selectedTab.tabWindow.SetActive(true);
        ResetTabs();
        curTab.background.color = curTab.selected;
    }

    public void HoverOn(TabButton curTab)
    {
        ResetTabs();
        if(curTab != selectedTab) curTab.background.color = curTab.hover;
    }

    public void HoverOff(TabButton curTab)
    {
        ResetTabs();
    }

    void ResetTabs()
    {
        foreach(TabButton tab in tabs)
        {
            if(tab != selectedTab) 
            {
                tab.background.color = tab.unselected;
                tab.tabWindow.SetActive(false);
            }
        }
    }
}
