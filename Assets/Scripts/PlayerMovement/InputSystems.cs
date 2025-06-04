using UnityEngine;
using System.Collections;

namespace InputSystems
{
    public interface IPlayerInput
    {
        float GetHorizontalInput();
        bool GetJumpInputDown();
        bool GetJumpInputHeld();
        bool GetDuckInputDown();
        bool GetDuckInputHeld();
    }

    public class DefaultPlayerInput : IPlayerInput
    {
        public float GetHorizontalInput()
        {
            return Input.GetAxisRaw("Horizontal");
        }

        public bool GetJumpInputDown()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        public bool GetJumpInputHeld()
        {
            return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        }
        
        public bool GetDuckInputDown()
        {
            return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
        }
        
        public bool GetDuckInputHeld()
        {
            return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        }
    }

    public class VirtualInput : IPlayerInput
    {
        private float horizontalInput = 0f;
        private bool jumpInputDown = false;
        private bool jumpInputHeld = false;
        private bool duckInputDown = false;
        private bool duckInputHeld = false;

        // Getters implementing the interface
        public float GetHorizontalInput() => horizontalInput;
        public bool GetJumpInputDown() => jumpInputDown;
        public bool GetJumpInputHeld() => jumpInputHeld;
        public bool GetDuckInputDown() => duckInputDown;
        public bool GetDuckInputHeld() => duckInputHeld;

        // Setters for controlling virtual input
        public void SetHorizontalInput(float value) => horizontalInput = Mathf.Clamp(value, -1f, 1f);
        public void SetJumpInputDown(bool value) => jumpInputDown = value;
        public void SetJumpInputHeld(bool value) => jumpInputHeld = value;
        public void SetDuckInputDown(bool value) => duckInputDown = value;
        public void SetDuckInputHeld(bool value) => duckInputHeld = value;

        public void ResetAllInputs()
        {
            horizontalInput = 0f;
            jumpInputDown = false;
            jumpInputHeld = false;
            duckInputDown = false;
            duckInputHeld = false;
        }

        public void TriggerJump(float duration = 0f)
        {
            jumpInputDown = true;
            jumpInputHeld = true;
            
            if (duration > 0f)
            {
                MonoBehaviour host = GameObject.FindObjectOfType<InputManager>();
                if (host != null)
                {
                    host.StartCoroutine(ResetJumpAfterDelay(duration));
                }
            }
        }

        public void TriggerDuck(float duration = 0f)
        {
            duckInputDown = true;
            duckInputHeld = true;

            if (duration > 0f)
            {
                MonoBehaviour host = GameObject.FindObjectOfType<InputManager>();
                if (host != null)
                {
                    host.StartCoroutine(ResetDuckAfterDelay(duration));
                }
            }
        }

        // Missing coroutine methods that were referenced but not defined
        private IEnumerator ResetJumpAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            jumpInputDown = false;
            jumpInputHeld = false;
        }

        private IEnumerator ResetDuckAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            duckInputDown = false;
            duckInputHeld = false;
        }
    }
    
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputMode currentMode = InputMode.Player;

        private IPlayerInput playerInput;
        private VirtualInput virtualInput;
        private IPlayerInput currentInput;

        public enum InputMode
        {
            Player,
            Virtual,
            Disabled
        }

        private void Awake()
        {
            playerInput = new DefaultPlayerInput();
            virtualInput = new VirtualInput();

            SetInputMode(currentMode);
        }

        public void SetInputMode(InputMode mode)
        {
            currentMode = mode;

            switch (mode)
            {
                case InputMode.Player:
                    currentInput = playerInput;
                    break;
                case InputMode.Virtual:
                    currentInput = virtualInput;
                    break;
                case InputMode.Disabled:
                    currentInput = new DisabledInput();
                    break;
            }
        }

        public VirtualInput GetVirtualInput() => virtualInput;

        // Proxy methods to current input
        public float GetHorizontalInput() => currentInput?.GetHorizontalInput() ?? 0f;
        public bool GetJumpInputDown() => currentInput?.GetJumpInputDown() ?? false;
        public bool GetJumpInputHeld() => currentInput?.GetJumpInputHeld() ?? false;
        public bool GetDuckInputDown() => currentInput?.GetDuckInputDown() ?? false;
        public bool GetDuckInputHeld() => currentInput?.GetDuckInputHeld() ?? false;

        // Alternative method for VirtualInput to use this InputManager as coroutine host
        public void StartInputCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }

    public class DisabledInput : IPlayerInput
    {
        public float GetHorizontalInput() => 0f;
        public bool GetJumpInputDown() => false;
        public bool GetJumpInputHeld() => false;
        public bool GetDuckInputDown() => false;
        public bool GetDuckInputHeld() => false;
    }

    // Alternative approach: Better VirtualInput that doesn't rely on finding InputManager
    public class ImprovedVirtualInput : IPlayerInput
    {
        private MonoBehaviour coroutineHost;
        
        private float horizontalInput = 0f;
        private bool jumpInputDown = false;
        private bool jumpInputHeld = false;
        private bool duckInputDown = false;
        private bool duckInputHeld = false;

        public ImprovedVirtualInput(MonoBehaviour host = null)
        {
            coroutineHost = host;
        }

        public void SetCoroutineHost(MonoBehaviour host)
        {
            coroutineHost = host;
        }

        // Interface implementation
        public float GetHorizontalInput() => horizontalInput;
        public bool GetJumpInputDown() => jumpInputDown;
        public bool GetJumpInputHeld() => jumpInputHeld;
        public bool GetDuckInputDown() => duckInputDown;
        public bool GetDuckInputHeld() => duckInputHeld;

        // Setters
        public void SetHorizontalInput(float value) => horizontalInput = Mathf.Clamp(value, -1f, 1f);
        public void SetJumpInputDown(bool value) => jumpInputDown = value;
        public void SetJumpInputHeld(bool value) => jumpInputHeld = value;
        public void SetDuckInputDown(bool value) => duckInputDown = value;
        public void SetDuckInputHeld(bool value) => duckInputHeld = value;

        public void ResetAllInputs()
        {
            horizontalInput = 0f;
            jumpInputDown = false;
            jumpInputHeld = false;
            duckInputDown = false;
            duckInputHeld = false;
        }

        public void TriggerJump(float duration = 0f)
        {
            jumpInputDown = true;
            jumpInputHeld = true;
            
            if (duration > 0f && coroutineHost != null)
            {
                coroutineHost.StartCoroutine(ResetJumpAfterDelay(duration));
            }
        }

        public void TriggerDuck(float duration = 0f)
        {
            duckInputDown = true;
            duckInputHeld = true;

            if (duration > 0f && coroutineHost != null)
            {
                coroutineHost.StartCoroutine(ResetDuckAfterDelay(duration));
            }
        }

        private IEnumerator ResetJumpAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            jumpInputDown = false;
            jumpInputHeld = false;
        }

        private IEnumerator ResetDuckAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            duckInputDown = false;
            duckInputHeld = false;
        }
    }
}