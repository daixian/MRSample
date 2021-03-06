﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GCSeries
{
    public class StylusObjectManipulationSample : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////
        // Unity Monobehaviour Callbacks
        //////////////////////////////////////////////////////////////////

        void Start()
        {

            // Create the stylus beam's GameObject.
            _stylusBeamObject = new GameObject("StylusBeam");
            _stylusBeamRenderer = _stylusBeamObject.AddComponent<LineRenderer>();
            _stylusBeamRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            _stylusBeamRenderer.SetColors(Color.white, Color.white);
        }

        void Update()
        {
            // Grab the latest stylus pose and button state information.
            //ZCore.Pose pose = _core.GetTargetPose(ZCore.TargetType.Primary, ZCore.CoordinateSpace.World);
            bool isButtonPressed = FCore.isKey0Down;

            switch (_stylusState)
            {
                case StylusState.Idle:
                    {
                        _stylusBeamLength = DEFAULT_STYLUS_BEAM_LENGTH;

                        // Perform a raycast on the entire scene to determine what the
                        // stylus is currently colliding with.
                        RaycastHit hit;
                        if (Physics.Raycast(FCore.penRay, out hit))
                        {
                            // Update the stylus beam length.
                            _stylusBeamLength = hit.distance / FCore.ViewerScale;

                            // If the front stylus button was pressed, initiate a grab.
                            if (isButtonPressed && !_wasButtonPressed)
                            {
                                // Begin the grab.
                                this.BeginGrab(hit.collider.gameObject, hit.distance, FCore.penPosition, FCore.penRotation);

                                _stylusState = StylusState.Grab;
                            }
                        }
                    }
                    break;

                case StylusState.Grab:
                    {
                        // Update the grab.
                        this.UpdateGrab(FCore.penPosition, FCore.penRotation);

                        // End the grab if the front stylus button was released.
                        if (!isButtonPressed && _wasButtonPressed)
                        {
                            _stylusState = StylusState.Idle;
                        }
                    }
                    break;

                default:
                    break;
            }

            // Update the stylus beam.
            this.UpdateStylusBeam(FCore.penPosition, FCore.penDirection);

            // Cache state for next frame.
            _wasButtonPressed = isButtonPressed;
        }


        //////////////////////////////////////////////////////////////////
        // Private Helpers
        //////////////////////////////////////////////////////////////////

        private void BeginGrab(GameObject hitObject, float hitDistance, Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * hitDistance));

            // Cache the initial grab state.
            _grabObject          = hitObject;
            _initialGrabOffset   = Quaternion.Inverse(hitObject.transform.localRotation) * (hitObject.transform.localPosition - inputEndPosition);
            _initialGrabRotation = Quaternion.Inverse(inputRotation) * hitObject.transform.localRotation;
            _initialGrabDistance = hitDistance;
        }

        private void UpdateGrab(Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * _initialGrabDistance));

            // Update the grab object's rotation.
            Quaternion objectRotation = inputRotation * _initialGrabRotation;
            _grabObject.transform.rotation = objectRotation;

            // Update the grab object's position.
            Vector3 objectPosition = inputEndPosition + (objectRotation * _initialGrabOffset);
            _grabObject.transform.position = objectPosition;
        }

        private void UpdateStylusBeam(Vector3 stylusPosition, Vector3 stylusDirection)
        {
            if (_stylusBeamRenderer != null)
            {
                float stylusBeamWidth  = DEFAULT_STYLUS_BEAM_WIDTH * FCore.ViewerScale;
                float stylusBeamLength = _stylusBeamLength * FCore.ViewerScale;
                _stylusBeamRenderer.SetWidth(0.002f, 0.002f);
                //_stylusBeamRenderer.SetWidth(stylusBeamWidth, stylusBeamWidth);
                _stylusBeamRenderer.SetPosition(0, stylusPosition);
                _stylusBeamRenderer.SetPosition(1, stylusPosition + (stylusDirection * stylusBeamLength));
            }
        }


        //////////////////////////////////////////////////////////////////
        // Private Enumerations
        //////////////////////////////////////////////////////////////////

        private enum StylusState
        {
            Idle = 0,
            Grab = 1,
        }


        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////

        private static readonly float DEFAULT_STYLUS_BEAM_WIDTH  = 0.0002f;
        private static readonly float DEFAULT_STYLUS_BEAM_LENGTH = 2f;

        //private ZCore         _core = null;
        private bool         _wasButtonPressed = false;

        private GameObject   _stylusBeamObject   = null;
        private LineRenderer _stylusBeamRenderer = null;
        private float        _stylusBeamLength   = DEFAULT_STYLUS_BEAM_LENGTH;

        private StylusState  _stylusState         = StylusState.Idle;
        private GameObject   _grabObject          = null;
        private Vector3      _initialGrabOffset   = Vector3.zero;
        private Quaternion   _initialGrabRotation = Quaternion.identity;
        private float        _initialGrabDistance = 0.0f;
    }
}