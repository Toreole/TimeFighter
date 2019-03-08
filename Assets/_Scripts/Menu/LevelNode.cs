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
        internal Transform north, east, south, west;
        [SerializeField]
        internal int depth = 0;
        [SerializeField]
        List<SpriteRenderer> connectors;

        public Vector3 Position { get { return transform.position; } set { transform.position = value; } }

        internal void SetConnection(Transform other, Connection dir)
        {
            if (dir == Connection.North)
                north = other;
            else if (dir == Connection.East)
                east = other;
            else if (dir == Connection.South)
                south = other;
            else
                west = other;
        }
        internal Transform GetConnection(Connection dir)
        {
            if (dir == Connection.North)
                return north;
            else if (dir == Connection.East)
                return east;
            else if (dir == Connection.South)
                return south;
            return west;
        }

        internal void UpdateConnectors()
        {
            foreach (var con in connectors)
                con.gameObject.SetActive(false);
            int index = 0;
            var other = north.GetComponent<LevelNode>();
            if (north != null && other.depth <= this.depth)
                UpdateConnector(north, ref index);

            other = east.GetComponent<LevelNode>();
            if (east != null && other.depth <= this.depth)
                UpdateConnector(east, ref index);

            other = south.GetComponent<LevelNode>();
            if (south != null && other.depth <= this.depth)
                UpdateConnector(south, ref index);

            other = west.GetComponent<LevelNode>();
            if (west != null && other.depth <= this.depth)
                UpdateConnector(west, ref index);
        }

        private void UpdateConnector(Transform other, ref int index)
        {
            var offset = other.position - Position;
            var connector = connectors[index];
            connector.gameObject.SetActive(true);
            connector.size = new Vector2(connector.size.x, offset.magnitude);
            connector.transform.rotation = Quaternion.LookRotation(Vector3.forward, offset.normalized);
            connector.transform.position = Position + (offset / 2f);
            index++;
        }
    }
}
