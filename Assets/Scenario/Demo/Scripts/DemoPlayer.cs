using UnityEngine;
using UnityEngine.InputSystem;

namespace StudyConstructor.Scenario.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class DemoPlayer : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;

        private Rigidbody body;
        private Vector3 startPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            float x = Axis(keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                           keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            float z = Axis(keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                           keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
            var move = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f) * speed;
            body.linearVelocity = new Vector3(move.x, body.linearVelocity.y, move.z);
        }

        public void ResetPosition()
        {
            body.linearVelocity = Vector3.zero;
            body.position = startPosition;
            transform.position = startPosition;
        }

        private static float Axis(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
