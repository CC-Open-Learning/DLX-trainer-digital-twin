using UnityEngine;

namespace VARLab.MPCircuits
{
    public class WorldToScreenMatch : MonoBehaviour
    {
        [SerializeField] private Transform componentOnTheBoard;
        [SerializeField] private Transform screenUIElement;

        private Camera mainCamera;

        void Start ()
        {
            mainCamera = Camera.main;
        }
        void Update()
        {
            screenUIElement.position = mainCamera.WorldToScreenPoint(componentOnTheBoard.transform.position); 
        }
    }
}
