using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    [System.Obsolete]
    public class TargetDummy : EnemyBase
    {
        [SerializeField]
        protected internal List<Vector3> railSystem;
        [SerializeField]
        protected internal bool loop = false;

        private List<float> segmentDistances = new List<float>();
        private float offset = 0f;
        private int segment = 0;
        private bool reverse = false;

        public bool Looping { get { return loop; } }
        public List<Vector3> RailSystem { get { return railSystem; } }

        protected override void Move()
        {
            //TODO if this is railed, move on the rails.
            if (settings.Movement != MovementPattern.Railed)
                return;
            var moveDist = Time.fixedDeltaTime * settings.MovementSpeed;
            Move(moveDist);
        }

        private void Move(float distance)
        {
            int maxSegment = railSystem.Count - 1;
            offset += distance;
            int clampedSegmentDistance = Mathf.Clamp(segment, 0, segmentDistances.Count - 1);
            var progress = (offset / segmentDistances[clampedSegmentDistance]);
            if (progress >= 1)
            {
                offset -= segmentDistances[clampedSegmentDistance];
                //PING PONG yay
                if (!loop && (segment == maxSegment || segment == 0))
                    reverse = !reverse;

                segment = NextSegment();
            }
            transform.position = Vector3.Lerp(railSystem[segment], railSystem[NextSegment()], progress);
        }

        private int NextSegment()
        {
            int current = segment;
            int next    = current + (reverse ? -1 : 1);
            int maxSegment = railSystem.Count - 1;
            if (loop)
            {
                next = (next < 0) ? maxSegment : (next > maxSegment) ? 0 : next;
                return next;
            }
            else
            {
                next = (next < 0) ? 1 : (next > maxSegment)? maxSegment - 1 : next;
                return Mathf.Clamp(next, 0, railSystem.Count - 1);
            }
        }

        //Why does this even exist?
        [System.Obsolete]
        protected override void UpdateEnemy()
        {

        }

        public override void ProcessHit(AttackHitData hitData)
        {
            //Apply forces if needed, otherwise this does not take damage.
        }
        public override void ProcessHit(AttackHitData hitData, bool dmg)
        {
            //Apply forces if needed, otherwise this does not take damage.
        }

        protected override void Start()
        {
            base.Start();

            if (railSystem.Count <= 1 || settings.Movement != MovementPattern.Railed)
                return;

            //Calculate the segment distances
            body.isKinematic = true;
            for (int i = 0; i < (railSystem.Count - 1); i++)
            {
                segmentDistances.Add((railSystem[i] - railSystem[i + 1]).magnitude);
            }
            if (loop)
                segmentDistances.Add((railSystem[railSystem.Count - 1] - railSystem[0]).magnitude);
        }

        protected override void FixedUpdate()
        {
            if (!active || settings == null)
                return;
            Move();
        }
    }
}
