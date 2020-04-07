using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleClickerKit
{
    public class MenuController : MonoBehaviour
    {

        public Button HomeButton;
        public Button FctoryButton;
        public Button PeopleButton;
        public GameObject BottomConvas;
        public GameObject PeoplePanel;
        public GameObject FactoryPanel;
        private GameObject _currentPanel;

        public void Awake()
        {
            PeoplePanel.SetActive(false);
            FactoryPanel.SetActive(false);
            HomeButton.gameObject.SetActive(false);
            BottomConvas.SetActive(true);
            HomeButton.onClick.AddListener(GoHome);
            FctoryButton.onClick.AddListener(GoFactory);
            PeopleButton.onClick.AddListener(GoPeople);
            PeoplePanel.SetActive(false);
            FactoryPanel.SetActive(false);
            GoHome();
        }

        private void GoPeople()
        {
            _currentPanel = PeoplePanel;
            _currentPanel.SetActive(true);
            BottomConvas.SetActive(false);
            HomeButton.gameObject.SetActive(true);
        }

        private void GoFactory()
        {
            _currentPanel = FactoryPanel;
            _currentPanel.SetActive(true);
            BottomConvas.SetActive(false);
            HomeButton.gameObject.SetActive(true);
        }

        private void GoHome()
        {
            if(_currentPanel!=null)
                _currentPanel.SetActive(false);
            HomeButton.gameObject.SetActive(false);
            BottomConvas.SetActive(true);
        }
    }
}