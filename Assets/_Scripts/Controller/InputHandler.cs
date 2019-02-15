using UnityEngine;

namespace Game.Controller
{
    // TODO: Switch to new input method?
    public class InputHandler : MonoBehaviour
    {
        private float _horizontal;
        private bool _jump;

        private StateManager _states;
        private float _delta;

        private void Start()
        {
            _states = GetComponent<StateManager>();
            _states.Init();
        }

        private void FixedUpdate()
        {
            _delta = Time.fixedDeltaTime;
            UpdateStates();
            _states.FixedTick(_delta);
        }

        private void Update()
        {
            GetInput();
        }

        private void GetInput()
        {
            _horizontal = Input.GetAxis("Horizontal");

            _jump = Input.GetButtonDown("Jump");
        }

        private void UpdateStates()
        {
            _states.horizontal = _horizontal;
            _states.moveAmount = Mathf.Clamp01(Mathf.Abs(_horizontal));
            _states.jumpInput = _jump;
        }
    }

}
