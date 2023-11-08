using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utils
{
   [RequireComponent(typeof(Button)), DisallowMultipleComponent]
   public class ButtonHandler : MonoBehaviour
   {
      private void Awake()
      {
         GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadSceneAsync(0));
      }
   }
}
