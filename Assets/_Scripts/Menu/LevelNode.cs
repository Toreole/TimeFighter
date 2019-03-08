using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Menu
{
    internal class LevelNode : MonoBehaviour
    {
        [SerializeField]
        internal LevelData levelData;
        [SerializeField]
        internal Transform[] neighbours = new Transform[4];
        [SerializeField]
        internal int depth = 0;
        [SerializeField]
        List<SpriteRenderer> connectors;
        [SerializeField]
        internal Color baseNodeColor;
        [SerializeField]
        internal Color inactiveNodeColor;
        [SerializeField]
        private SpriteRenderer sprite;

        public Vector3 Position { get { return transform.position; } set { transform.position = value; UpdateConnectors(true); } }
        public string TargetScene { get { return levelData.targetScene; } set { levelData.targetScene = value; gameObject.name = "Node:" + value; } }
        public Color BaseColor { get => baseNodeColor; set { baseNodeColor = value; sprite.color = value; } }
        public bool Unlocked { get => levelData.isUnlocked; set { levelData.isUnlocked = value; UpdateLockState(); } }

        private void Start()
        {
            if (sprite == null)
                sprite = GetComponent<SpriteRenderer>();
            UpdateLockState();
        }

        private void UpdateLockState()
        {
            if (levelData.isUnlocked)
                sprite.color = baseNodeColor;
            else
                sprite.color = inactiveNodeColor;
        }

        internal void SetConnection(Transform other, Connection dir)
        {
            neighbours[(int)dir] = other;
        }
        internal Transform GetConnection(Connection dir)
        {
            return neighbours[(int)dir];
        }

        internal void Remove()
        {
            for (int i = 0; i < 4; i++)
            {
                var neighb = neighbours[i];
                if (neighb == null)
                    continue;

                neighb.GetComponent<LevelNode>().neighbours[(int)OppositeOf((Connection)i)] = null;
            }
        }

        internal void UpdateConnectors(bool updateOthers)
        {
            foreach (var con in connectors)
                con.gameObject.SetActive(false);

            if (neighbours[(int)Connection.North]!= null)
                UpdateConnector(Connection.North, true);
            
            if (neighbours[(int)Connection.East] != null)
                UpdateConnector(Connection.East, true);

            if (neighbours[(int)Connection.South] != null)
                UpdateConnector(Connection.South, true);

            if (neighbours[(int)Connection.West] != null)
                UpdateConnector(Connection.West, true);
        }

        private void UpdateConnector(Connection con, bool updateOther)
        {
            var neighbour = neighbours[(int)con].GetComponent<LevelNode>();
            var connector = connectors[(int)con];
            if (neighbour.depth > depth)
                connector.gameObject.SetActive(false);
            else
            {
                var offset = neighbour.Position - Position;
                connector.gameObject.SetActive(true);
                connector.size = new Vector2(connector.size.x, offset.magnitude);
                connector.transform.rotation = Quaternion.LookRotation(Vector3.forward, -offset.normalized);
                connector.transform.position = Position + (offset / 2f);
            }
            if (updateOther)
                neighbours[(int)con].GetComponent<LevelNode>().UpdateConnector(OppositeOf(con), false);
        }

        private Connection OppositeOf(Connection original)
        {
            if (original == Connection.North)
                return Connection.South;
            if (original == Connection.East)
                return Connection.West;
            if (original == Connection.South)
                return Connection.North;
            return Connection.East;
        }
    }
}
