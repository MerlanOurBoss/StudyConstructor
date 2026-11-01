using UnityEngine;

namespace StudyConstructor.Scenario
{
    [AddComponentMenu("Scenario/Flow/Flow Block")]
    public class FlowBlock : FlowNode
    {
        [Tooltip("Галочка — шаги внутри доступны одновременно, в любом порядке. Без галочки — строго по очереди.")]
        [SerializeField] private bool parallel;

        public override bool IsParallel => parallel;

        internal void Setup(bool isParallel) => parallel = isParallel;
    }
}
