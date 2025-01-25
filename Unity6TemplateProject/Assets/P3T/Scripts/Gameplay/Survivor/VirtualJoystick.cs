using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    /// Manages a virtual joystick for playing on touch based devices
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Canvas Canvas;

        [SerializeField] private JoystickType FixedOrFloat = JoystickType.Floating;

        [SerializeField] private Image JoystickBackgroundImage;

        [SerializeField] private Image JoystickOverlay;
    
        [SerializeField] private CanvasGroup CanvasGroup;

        private Vector3 _inputVector;
        public Vector3 InputVector => _inputVector;

        private Vector3 _originalPosition;

        private void Start()
        {
            _originalPosition = JoystickBackgroundImage.rectTransform.position;
        }

        /// <summary>
        ///     Handler pointer drag events
        /// </summary>
        /// <param name="e"> </param>
        public void OnDrag(PointerEventData e)
        {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    JoystickBackgroundImage.rectTransform,
                    e.position,
                    e.pressEventCamera,
                    out pos))
            {
                var sizeDelta = JoystickBackgroundImage.rectTransform.sizeDelta;

                pos.x /= sizeDelta.x;
                pos.y /= sizeDelta.y;

                _inputVector = new Vector3(pos.x * 2, pos.y * 2);
                _inputVector = _inputVector.magnitude > 1.0f ? _inputVector.normalized : _inputVector;

                var joystickPosition = new Vector3(
                    _inputVector.x * (sizeDelta.x * .4f),
                    _inputVector.y * (sizeDelta.y * .4f));
                JoystickOverlay.rectTransform.anchoredPosition = joystickPosition;
            }
        }

        /// <summary>
        ///     Handles pointer down events
        /// </summary>
        /// <param name="e"> </param>
        public void OnPointerDown(PointerEventData e)
        {
            if (FixedOrFloat == JoystickType.Floating) MoveJoystickToCurrentTouchPosition();
            OnDrag(e);
            CanvasGroup.alpha = 0.5f;
        }

        public void OnPointerUp(PointerEventData e)
        {
            _inputVector = Vector3.zero;
            JoystickOverlay.rectTransform.anchoredPosition = Vector3.zero;
            JoystickBackgroundImage.rectTransform.position = _originalPosition;
            CanvasGroup.alpha = 1;
        }

        /// <summary>
        ///     Used to move floating joystick
        /// </summary>
        private void MoveJoystickToCurrentTouchPosition()
        {
            Vector2 pos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, Input.mousePosition,
                null, out pos);
            JoystickBackgroundImage.rectTransform.position = Canvas.transform.TransformPoint(pos);
        }

        private enum JoystickType
        {
            [UsedImplicitly] Fixed = 0,
            Floating
        }
    }
}