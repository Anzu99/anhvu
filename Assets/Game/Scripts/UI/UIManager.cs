using System.Collections;
using System.Collections.Generic;
using UnityEngine;

   public class UIManager : Singleton<UIManager>
    {
        public UIBase[] listScreen;
        public TopUI topUI;

        public T ShowUI<T>(UIName uIScreen, bool hasTopUI = true, bool hideAllScreen = false) where T : UIBase
        {
            if (hideAllScreen)
            {
                for (int i = 0; i < listScreen.Length; i++)
                {
                    listScreen[i].gameObject.SetActive(false);
                }
            }
            listScreen[(int)uIScreen].Show();
            topUI.gameObject.SetActive(hasTopUI);
            return listScreen[(int)uIScreen] as T;
        }

        public T GetUI<T>(UIName uIScreen) where T : UIBase
        {
            return listScreen[(int)uIScreen] as T;
        }

        public enum UIName
        {
            MAIN = 0,
            GAMEPLAY = 1,
            SETTING = 2,
            SHOP = 3,
            PAUSE = 4
        }
   }

