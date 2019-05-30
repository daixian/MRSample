using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GCSeries;

namespace UnityEngine.EventSystems
{

    public class GCSeriesInputModule : PointerInputModule
    {
        //[Tooltip("Object which points with Z axis. E.g. stylus form pen")]
        //public Transform rayTransform;
        //[NonSerialized]

        [Header("Standalone Input Module")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";

        /// <summary>
        /// Name of the vertical axis for movement (if axis events are used).
        /// </summary>
        [SerializeField]
        private string m_VerticalAxis = "Vertical";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_SubmitButton = "Submit";

        /// <summary>
        /// Name of the submit button.
        /// </summary>
        [SerializeField]
        private string m_CancelButton = "Cancel";

        [SerializeField]
        private float m_InputActionsPerSecond = 10;

        private float m_NextAction;

        // The raycaster that gets to do pointer interaction (e.g. with a mouse), gaze interaction always works
        //[NonSerialized]
        public GCSeriesRaycaster activeGraphicRaycaster;

        [Header("pen state 0 Scroll")]
        [Tooltip("Enable scrolling with the pen 0 on a gamepad")]
        public bool useRightStickScroll = true;

        [Header("Dragging")]
        [Tooltip("Minimum pointer movement in degrees to start dragging")]
        public float angleDragThreshold = 1;

        protected override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("Application.isFocused  " + Application.isFocused);
        }

        //Record pen key state
        private bool isKey0Up = false;
        private bool isKey0Down = false;
        protected override void Start()
        {
            base.Start();
            FCore.EventKey1Up += Key0Up;
            FCore.EventKey1Down += Key0Down;
        }

        private void Key0Up()
        {
            isKey0Up = true;
        }
        private void Key0Down()
        {
            isKey0Down = true;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            var shouldActivate = Input.GetButtonDown(m_SubmitButton);
            shouldActivate |= Input.GetButtonDown(m_CancelButton);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0.0f);
            shouldActivate |= Input.GetMouseButtonDown(0);
            return shouldActivate;
        }


        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        /// <summary>
        /// Clear pointer state for both types of pointer
        /// </summary>
        protected new void ClearSelection()
        {
            var baseEventData = GetBaseEventData();

            foreach (var pointer in m_PointerData.Values)
            {
                // clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }
            foreach (var pointer in m_VRRayPointerData.Values)
            {
                // clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            m_PointerData.Clear();
            eventSystem.SetSelectedGameObject(null, baseEventData);
        }

        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();
            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            ProcessMouseEvent(GetGazePointerData());

            isKey0Up = false;//放这里
            isKey0Down = false;
        }

        private void ProcessMouseEvent(MouseState mouseData)
        {
            var pressed = mouseData.AnyPressesThisFrame();
            var released = mouseData.AnyReleasesThisFrame();

            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            if (!UseMouse(pressed, released, leftButtonData.buttonData))
                return;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            //HandlePointerExitAndEnter(leftButtonData.buttonData, leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            //ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            //ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            //ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            //ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        /// <summary>
        /// 当前记录按下的Obj
        /// </summary>
        GameObject curPressObj = null;
        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        private void ProcessMousePress(MouseButtonEventData data)
        {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            if (currentOverGo == null)
            {
                pointerEvent.pointerPress = null;
                DeselectIfSelectionChanged(currentOverGo, pointerEvent);
            }
            else
            {
                Selectable component = currentOverGo.transform.GetComponentInParent<Selectable>();
                if (null != component)
                {
                    if (component.gameObject == curPressObj)
                    {
                        pointerEvent.pointerPress = curPressObj;
                        DeselectIfSelectionChanged(curPressObj, pointerEvent);
                    }
                }
            }

            // PointerDown notification
            if (data.PressedThisFrame())
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                if (pointerEvent.IsF3dSpacePointer())
                {
                    pointerEvent.SetSwipeStart(Input.mousePosition);
                }
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);



                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                curPressObj = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            //DeselectIfSelectionChanged(currentOverGo, pointerEvent);
            //if (pointerEvent.pointerEnter != currentOverGo)
            //{
            //    // send a pointer enter to the touched element if it isn't the one to select...
            //    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            //    pointerEvent.pointerEnter = currentOverGo;
            //}

            // PointerUp notification
            if (data.ReleasedThisFrame())
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }
        /// <summary>
        /// Decide if mouse events need to be processed this frame. Same as StandloneInputModule except
        /// that the IsPointerMoving method from this class is used, instead of the method on PointerEventData
        /// </summary>
        private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
        {
            if (pressed || released || IsPointerMoving(pointerData) || pointerData.IsScrolling())
                return true;

            return false;
        }

        private readonly MouseState m_PenState = new MouseState();

        // The following 2 functions are equivalent to PointerInputModule.GetMousePointerEventData but are customized to
        // get data for ray pointers and canvas mouse pointers.

        /// <summary>
        /// State for a pointer controlled by a world space ray. E.g. gaze pointer
        /// </summary>
        /// <returns></returns>
        virtual protected MouseState GetGazePointerData()
        {
            // Get the OVRRayPointerEventData reference
            GCSeriesPointerEventData leftData;
            GetPointerData(kMouseLeftId, out leftData, true);
            leftData.Reset();
            //Now set the world space ray. This ray is what the user uses to point at UI elements
            leftData.worldSpaceRay = new Ray(FCore.penPosition, FCore.penDirection);
            //leftData.scrollDelta = GetExtraScrollDelta();

            //Populate some default values
            leftData.button = PointerEventData.InputButton.Left;
            leftData.useDragThreshold = true;
            // Perform raycast to find intersections with world
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();
            //hitPos = new Vector3(1000f, 1000f, 1000f);
            GCSeriesRaycaster ovrRaycaster = raycast.module as GCSeriesRaycaster;
            // We're only interested in intersections from OVRRaycasters
            if (ovrRaycaster)
            {
                // The Unity UI system expects event data to have a screen position
                // so even though this raycast came from a world space ray we must get a screen
                // space position for the camera attached to this raycaster for compatability
                leftData.position = ovrRaycaster.GetScreenPosition(raycast);


                // Find the world position and normal the Graphic the ray intersected
                RectTransform graphicRect = raycast.gameObject.GetComponent<RectTransform>();
                if (graphicRect != null)
                {
                    // Set are gaze indicator with this world position and normal
                    Vector3 worldPos = raycast.worldPosition;//射到UI上的点
                                                             //Vector3 normal = GetRectTransformNormal(graphicRect);
                                                             //hitPos = worldPos;
                }

                //获取父物体是否有button组件
                Button uiButton = raycast.gameObject.GetComponentInParent<Button>();
                if (uiButton)
                {
                    Transform[] transforms = raycast.gameObject.GetComponentsInParent<Transform>();
                    //在限定的层级里寻找是否存在button组件，存在且不与上次的物体同名，则说明需要响应shake操作
                    int traverseTime = transforms.Length < traverseButtonTime ? transforms.Length : traverseButtonTime;
                    for (int i = 0; i < traverseTime; i++)
                    {
                        if (transforms[i].name == uiButton.name)
                        {
                            if (lastCheckObj)
                            {
                                if (lastCheckObj.name != uiButton.name)
                                {
                                    FCore.PenShake();
                                    lastCheckObj = uiButton.gameObject;
                                }
                            }
                            else
                            {
                                FCore.PenShake();
                                lastCheckObj = uiButton.gameObject;
                            }
                        }
                    }
                }
                else
                {
                    Toggle toggle = raycast.gameObject.GetComponentInParent<Toggle>();
                    if (toggle)
                    {
                        Transform[] transforms = raycast.gameObject.GetComponentsInParent<Transform>();
                        //在限定的层级里寻找是否存在button组件，存在且不与上次的物体同名，则说明需要响应shake操作
                        int traverseTime = transforms.Length < traverseToggleTime ? transforms.Length : traverseToggleTime;
                        for (int i = 0; i < traverseTime; i++)
                        {
                            if (transforms[i].name == toggle.name)
                            {
                                if (lastCheckObj)
                                {
                                    if (lastCheckObj.name != toggle.name)
                                    {
                                        FCore.PenShake();
                                        lastCheckObj = toggle.gameObject;
                                    }
                                }
                                else
                                {
                                    FCore.PenShake();
                                    lastCheckObj = toggle.gameObject;
                                }
                            }
                        }
                    }
                    else
                        lastCheckObj = null;
                }
            }
            else
            {
                //没有点到UI 或者说是移出UI
                lastCheckObj = null;
            }
            m_PenState.SetButtonState(PointerEventData.InputButton.Left, GetPenButtonState(), leftData);
            return m_PenState;
        }

        /// <summary>
        /// 记录上一次操作的对象
        /// </summary>
        GameObject lastCheckObj;
        /// <summary>
        /// 遍历button的次数
        /// </summary>
        int traverseButtonTime = 3;
        /// <summary>
        /// 遍历toggle的次数
        /// </summary>
        int traverseToggleTime = 3;

        static bool IsPointerMoving(PointerEventData pointerEvent)
        {
            if (pointerEvent.IsF3dSpacePointer())
                return true;
            else
            {

            }
            return pointerEvent.IsPointerMoving();
        }
        /// <summary>
        /// Get state of button corresponding to click state 0 form pen pointer to hit obj
        /// </summary>
        /// <returns></returns>
        virtual protected PointerEventData.FramePressState GetPenButtonState()
        {
            var pressed = isKey0Down;
            var released = isKey0Up;
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Gear VR the mouse button events correspond to touch pad events. We only use these as gaze pointer clicks 
            // on Gear VR because on PC the mouse clicks are used for actual mouse pointer interactions.
            pressed |= Input.GetMouseButtonDown(0);
            released |= Input.GetMouseButtonUp(0);
#endif

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        /// <summary>
        /// For RectTransform, calculate it's normal in world space
        /// </summary>
        static Vector3 GetRectTransformNormal(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector3 BottomEdge = corners[3] - corners[0];
            Vector3 LeftEdge = corners[1] - corners[0];
            rectTransform.GetWorldCorners(corners);
            return Vector3.Cross(BottomEdge, LeftEdge).normalized;
        }

        /// <summary>
        /// Process keyboard events.
        /// </summary>
        private bool SendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;

            if (!AllowMoveEventProcessing(time))
                return false;

            Vector2 movement = GetRawMoveVector();
            // Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            if (!Mathf.Approximately(axisEventData.moveVector.x, 0f)
                || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
            {
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            }
            m_NextAction = time + 1f / m_InputActionsPerSecond;
            return axisEventData.used;
        }

        private bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        private bool AllowMoveEventProcessing(float time)
        {
            bool allow = Input.GetButtonDown(m_HorizontalAxis);
            allow |= Input.GetButtonDown(m_VerticalAxis);
            allow |= (time > m_NextAction);
            return allow;
        }

        /// <summary>
        /// Process submit keys.
        /// </summary>
        private bool SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            if (Input.GetButtonDown(m_SubmitButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (Input.GetButtonDown(m_CancelButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            return data.used;
        }

        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxisRaw(m_HorizontalAxis);
            move.y = Input.GetAxisRaw(m_VerticalAxis);

            if (Input.GetButtonDown(m_HorizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (Input.GetButtonDown(m_VerticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }

        // Pool for F3dSpaceRayPointerEventData for ray based pointers
        protected Dictionary<int, GCSeriesPointerEventData> m_VRRayPointerData = new Dictionary<int, GCSeriesPointerEventData>();


        protected bool GetPointerData(int id, out GCSeriesPointerEventData data, bool create)
        {
            if (!m_VRRayPointerData.TryGetValue(id, out data) && create)
            {
                data = new GCSeriesPointerEventData(eventSystem)
                {
                    pointerId = id,
                };

                m_VRRayPointerData.Add(id, data);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Get extra scroll delta from state0 button  鼠标滚轮
        /// </summary>
        protected Vector2 GetExtraScrollDelta()
        {
            Vector2 scrollDelta = new Vector2();
            if (useRightStickScroll)
            {
                //Vector2 s = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                //if (Mathf.Abs(s.x) < rightStickDeadZone) s.x = 0;
                //if (Mathf.Abs(s.y) < rightStickDeadZone) s.y = 0;
                //scrollDelta = s;
            }
            return scrollDelta;
        }

        /// <summary>
        /// Exactly the same as the code from PointerInputModule, except that we call our own
        /// IsPointerMoving.
        /// 
        /// This would also not be necessary if PointerEventData.IsPointerMoving was virtual
        /// </summary>
        /// <param name="pointerEvent"></param>
        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            Vector2 originalPosition = pointerEvent.position;
            bool moving = IsPointerMoving(pointerEvent);
            if (moving && pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                && ShouldStartDrag(pointerEvent))
            {
                if (pointerEvent.IsF3dSpacePointer())
                {
                    //adjust the position used based on swiping action. Allowing the user to
                    //drag items by swiping on the GearVR touchpad
                    pointerEvent.position = originalPosition;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                if (pointerEvent.IsF3dSpacePointer())
                {
                    pointerEvent.position = originalPosition;
                }
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
            //DeselectIfSelectionChanged(pointerEvent.pointerCurrentRaycast.gameObject, pointerEvent);
        }


        /// <summary>
        /// New version of ShouldStartDrag implemented first in PointerInputModule. This version differs in that
        /// for ray based pointers it makes a decision about whether a drag should start based on the angular change
        /// the pointer has made so far, as seen from the camera. This also works when the world space ray is 
        /// translated rather than rotated, since the beginning and end of the movement are considered as angle from
        /// the same point.
        /// </summary>
        private bool ShouldStartDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.useDragThreshold)
                return true;

            if (!pointerEvent.IsF3dSpacePointer())
            {
                // Same as original behaviour for canvas based pointers
                return (pointerEvent.pressPosition - pointerEvent.position).sqrMagnitude >= eventSystem.pixelDragThreshold * eventSystem.pixelDragThreshold;
            }
            else
            {
#if UNITY_ANDROID && !UNITY_EDITOR  // On android allow swiping to start drag
                if (useSwipeScroll && ((Vector3)pointerEvent.GetSwipeStart() - Input.mousePosition).magnitude > swipeDragThreshold)
                {
                    return true;
                }
#endif
                // When it's not a screen space pointer we have to look at the angle it moved rather than the pixels distance
                // For gaze based pointing screen-space distance moved will always be near 0
                Vector3 cameraPos = pointerEvent.pressEventCamera.transform.position;
                Vector3 pressDir = (pointerEvent.pointerPressRaycast.worldPosition - cameraPos).normalized;
                Vector3 currentDir = (pointerEvent.pointerCurrentRaycast.worldPosition - cameraPos).normalized;
                return Vector3.Dot(pressDir, currentDir) < Mathf.Cos(Mathf.Deg2Rad * (angleDragThreshold));
            }
        }
    }

}