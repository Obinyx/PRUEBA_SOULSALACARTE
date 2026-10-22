using UnityEngine;

namespace SoulsALaCarte.Core
{
    public static class InputBridge
    {
        public static float GetHorizontal()
        {
            float val = 0f;
            try
            {
                val = Input.GetAxisRaw("Horizontal");
            }
            catch
            {
                // Fallback si Legacy Input Manager está deshabilitado
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) val += 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) val -= 1f;
            }
            return Mathf.Clamp(val, -1f, 1f);
        }

        public static float GetVertical()
        {
            float val = 0f;
            try
            {
                val = Input.GetAxisRaw("Vertical");
            }
            catch
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) val += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) val -= 1f;
            }
            return Mathf.Clamp(val, -1f, 1f);
        }

        public static bool GetSprint()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public static bool GetInteractDown()
        {
            return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        }

        public static bool GetInteractHeld()
        {
            return Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        }

        public static bool GetSecondaryActionDown()
        {
            return Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1);
        }

        public static bool GetPauseDown()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
        }
    }
}
