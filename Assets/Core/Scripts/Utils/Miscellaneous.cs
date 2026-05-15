using UnityEngine.InputSystem;

namespace Utils {

    public static class Miscellaneous {
        
        public delegate void ActionFunctionDelegate(InputAction.CallbackContext context);
        /// <summary>
        /// Middleman for InputAction callbacks, which calls the given <paramref name="function"/> if the gamepad that triggered the Action matches the <paramref name="filter"/> gamepad; or if <paramref name="allowNonGamepads"/> is true and the Action was triggered by anything other than a gamepad.
        /// </summary>
        /// <param name="context">Action callback context.</param>
        /// <param name="function">Function to call.</param>
        /// <param name="filter">Gamepad instance allowed to call <paramref name="function"/>; aborts if null.</param>
        /// <param name="allowNonGamepads">Whether to call <paramref name="function"/> if Action was not triggered by a gamepad.</param>
        public static void GamepadFilter(InputAction.CallbackContext context, ActionFunctionDelegate function, Gamepad filter, bool allowNonGamepads = true) {
            if (ValidateContext(context, filter, allowNonGamepads))
                function(context);
        }

        private static bool ValidateContext(InputAction.CallbackContext context, Gamepad filter, bool allowNonGamepads) {
            return context.control == null || context.control.device == null || (context.control.device is Gamepad pad && filter != null && pad == filter) || allowNonGamepads;
        }

    }

}