using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StudyConstructor.Scenario.Demo
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Tooltip("На каком расстоянии можно взаимодействовать с объектом.")]
        [SerializeField] private float radius = 1.8f;

        [Tooltip("Текст подсказки. Его родительский объект показывается, только когда рядом есть объект.")]
        [SerializeField] private Text hint;

        private readonly Collider[] hits = new Collider[16];

        private void Update()
        {
            var target = FindNearest();
            if (hint != null)
            {
                hint.transform.parent.gameObject.SetActive(target != null);
                if (target != null)
                    hint.text = target.Hint;
            }

            var keyboard = Keyboard.current;
            if (target == null || keyboard == null)
                return;

            if (keyboard.eKey.wasPressedThisFrame)
                target.UsePrimary();
            else if (keyboard.qKey.wasPressedThisFrame)
                target.UseSecondary();
        }

        private IInteractable FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, ~0, QueryTriggerInteraction.Collide);
            IInteractable nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var interactable = hits[i].GetComponentInParent<IInteractable>();
                if (interactable == null)
                    continue;

                float distance = (hits[i].bounds.center - transform.position).sqrMagnitude;
                if (distance < best)
                {
                    nearest = interactable;
                    best = distance;
                }
            }
            return nearest;
        }
    }
}
