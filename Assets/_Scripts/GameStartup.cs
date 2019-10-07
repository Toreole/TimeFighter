using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Game
{
    public class GameStartup : MonoBehaviour
    {
        [SerializeField]
        protected string[] scenesToLoadInOrder;

        IEnumerator Start()
        {
            for(int i = 0; i<scenesToLoadInOrder.Length; i++)
            {
                SceneManager.LoadScene(scenesToLoadInOrder[i]);
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
            Destroy(gameObject);
        }
    }
}