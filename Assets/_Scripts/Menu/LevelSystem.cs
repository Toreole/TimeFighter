using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Game.Menu
{
    /// <summary>
    /// The visible map in the "menu"
    /// </summary>
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField]
        internal List<LevelNode> levels;
        [SerializeField]
        protected GameObject levelNodePrefab;

        [SerializeField]
        protected Transform defaultNode;
        [SerializeField]
        protected Transform player;
        [SerializeField]
        protected float speed;
        protected bool moving;

        protected Transform currentLevel;
        private LevelNode CurrentNode => currentLevel.GetComponent<LevelNode>();
        
        internal LevelNode CreateNewLevel()
        {
            var temp = Instantiate(levelNodePrefab, this.transform);
            var l = temp.GetComponent<LevelNode>();
            l.TargetScene = "DefaultScene";
            levels.Add(l);
            return l;
        }

        private void Start()
        {
            if (currentLevel == null)
                currentLevel = defaultNode;
            player.position = currentLevel.position;
        }

        private void Update()
        {
            if (moving)
                return;
            //Load the next level
            if (Input.GetKeyDown(KeyCode.Return))
                SceneManager.LoadScene(CurrentNode.levelData.targetScene);
            //Move inbetween nodes.
            if (Input.GetKey(KeyCode.W))
                StartCoroutine(MoveDirection(Connection.North));
            else if (Input.GetKey(KeyCode.D))
                StartCoroutine(MoveDirection(Connection.East));
            else if (Input.GetKey(KeyCode.S))
                StartCoroutine(MoveDirection(Connection.South));
            else if (Input.GetKey(KeyCode.A))
                StartCoroutine(MoveDirection(Connection.West));
        }

        private IEnumerator MoveDirection(Connection dir)
        {
            moving = true;
            var nextTransform = CurrentNode.GetConnection(dir);
            //break conditions
            if(nextTransform == null)
            {
                moving = false;
                yield break;
            }
            if(!nextTransform.GetComponent<LevelNode>().levelData.isUnlocked)
            {
                moving = false;
                yield break;
            }

            //Move to the next one lmao
            for(float t = 0f; t < speed; t += Time.deltaTime)
            {
                player.position = Vector3.Lerp(currentLevel.position, nextTransform.position, t / speed);
                yield return null;
            }
            player.position = nextTransform.position;
            currentLevel = nextTransform;
            moving = false;
        }

    }
}