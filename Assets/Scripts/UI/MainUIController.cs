using UnityEngine;

namespace Platformer.UI
{
    public class MainUIController : MonoBehaviour
    {
        public GameObject[] panels;

        void Start()
        {
            SetActivePanel(0);
        }

        public void SetActivePanel(int index)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null)
                {
                    panels[i].SetActive(i == index);
                }
            }
        }

        public void StartGame()
        {
            SetActivePanel(1);
            Time.timeScale = 1f;
        }
    }
}