using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    [RequireComponent(typeof(Button))]
    public class RestartButton : MonoBehaviour
    {
        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(Restart);
        }
        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(Restart);
        }

        private void Restart()
        {
            ScreenFadeManager.Instance.FadeAround(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }
    }
}
