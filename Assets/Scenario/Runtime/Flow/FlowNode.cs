using System.Collections.Generic;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Узел сгенерированного сценария на сцене. Порядок детей — порядок в Hierarchy:
    /// его можно менять перетаскиванием, а выключенный объект пропускается при прохождении.
    /// </summary>
    public abstract class FlowNode : MonoBehaviour
    {
        /// <summary>Дети выполняются одновременно (иначе — по очереди).</summary>
        public virtual bool IsParallel => false;

        public IEnumerable<FlowNode> Children()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf && child.TryGetComponent(out FlowNode node))
                    yield return node;
            }
        }

        /// <summary>Все шаги узла в порядке иерархии.</summary>
        public IEnumerable<FlowStep> Steps()
        {
            if (this is FlowStep step)
            {
                yield return step;
                yield break;
            }

            foreach (var child in Children())
            foreach (var childStep in child.Steps())
                yield return childStep;
        }
    }
}
