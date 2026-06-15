using System;
using Character;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace AI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Attack Nearby Character",
        description: "Attacks the nearest other CharacterCore when it is within range.",
        story: "[Agent] attacks a nearby character",
        category: "Action/Hubris",
        id: "4f589f7987e74f38a451b23b7d5fc6c9")]
    public partial class AttackNearbyCharacterAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> AttackRange = new BlackboardVariable<float>(1.25f);
        [SerializeReference] public BlackboardVariable<float> VerticalAttackDeadZone = new BlackboardVariable<float>(0.2f);
        [SerializeReference] public BlackboardVariable<bool> AttackOnlyWhenTargetAlive = new BlackboardVariable<bool>(true);

        private CharacterCore _agentCore;

        protected override Status OnStart()
        {
            _agentCore = Agent == null || Agent.Value == null
                ? null
                : Agent.Value.GetComponent<CharacterCore>();

            return _agentCore == null ? Status.Failure : Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_agentCore == null || Agent == null || Agent.Value == null)
                return Status.Failure;

            CharacterCore target = FindNearestTarget();

            if (target == null)
                return Status.Running;

            Vector2 delta = target.transform.position - Agent.Value.transform.position;
            CharacterCore.ActionType actionType = CharacterCore.ActionType.Upwards;
            int facingDirection = Mathf.Abs(delta.x) > 0.01f ? (int)Mathf.Sign(delta.x) : 0;

            _agentCore.AIAttack(actionType, facingDirection);
            return Status.Running;
        }

        private CharacterCore FindNearestTarget()
        {
            CharacterCore[] characters = UnityEngine.Object.FindObjectsByType<CharacterCore>(FindObjectsSortMode.None);
            CharacterCore nearest = null;
            float nearestSqrDistance = float.PositiveInfinity;
            float attackRangeSqr = AttackRange.Value * AttackRange.Value;
            Vector2 position = Agent.Value.transform.position;

            foreach (CharacterCore character in characters)
            {
                if (character == null || character == _agentCore)
                    continue;

                if (AttackOnlyWhenTargetAlive.Value && !character.gameObject.activeInHierarchy)
                    continue;

                float sqrDistance = ((Vector2)character.transform.position - position).sqrMagnitude;

                if (sqrDistance > attackRangeSqr || sqrDistance >= nearestSqrDistance)
                    continue;

                nearest = character;
                nearestSqrDistance = sqrDistance;
            }

            return nearest;
        }

      
    }
}
