﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// 1. Decides when to show the cursor.
    /// 2. Positions the cursor at the gazed location.
    /// 3. Rotates the cursor to match hologram normals.
    /// </summary>
    public class BasicCursor : MonoBehaviour
    {
        public struct RaycastResult
        {
            public bool Hit;
            public Vector3 Position;
            public Vector3 Normal;
        }

        [Tooltip("Distance, in meters, to offset the cursor from the collision point.")]
        public float DistanceFromCollision = 0.01f;

        private Quaternion cursorDefaultRotation;

        private MeshRenderer meshRenderer;

        private GazeManager gazeManager;

        /// <summary>
        /// The number of frames to wait until we get our GazeManager reference before
        /// throwing an error about it being missing in the scene.
        /// </summary>
        private int framesBeforeError = 10;

        protected virtual void Awake()
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError("This script requires that your cursor asset has a MeshRenderer component on it.");
                return;
            }

            // Hide the Cursor to begin with.
            meshRenderer.enabled = false;

            // Cache the cursor default rotation so the cursor can be rotated with respect to the original orientation.
            cursorDefaultRotation = gameObject.transform.rotation;
        }

        private bool GetGazeManagerReference()
        {
            gazeManager = GazeManager.Instance;

            if (gazeManager == null)
            {
                if (framesBeforeError > 0)
                {
                    framesBeforeError--;
                }

                if (framesBeforeError == 0)
                {
                    Debug.LogError("Must have a GazeManager somewhere in the scene.");
                }

                return false;
            }

            if ((GazeManager.Instance.RaycastLayerMask & (1 << gameObject.layer)) != 0)
            {
                Debug.LogError("The cursor has a layer that is checked in the GazeManager's Raycast Layer Mask.  Change the cursor layer (e.g.: to Ignore Raycast) or uncheck the layer in GazeManager: " +
                    LayerMask.LayerToName(gameObject.layer));
            }

            return true;
        }

        protected virtual RaycastResult CalculateRayIntersect()
        {
            RaycastResult result = new RaycastResult();
            result.Hit = GazeManager.Instance.Hit;
            result.Position = GazeManager.Instance.Position;
            result.Normal = GazeManager.Instance.Normal;
            return result;
        }

        protected virtual void LateUpdate()
        {
            if (meshRenderer == null)
            {
                return;
            }

            if (gazeManager == null)
            {
                if (!GetGazeManagerReference())
                {
                    return;
                }
            }

            // Calculate the raycast result
            RaycastResult rayResult = CalculateRayIntersect();

            // Show or hide the Cursor based on if the user's gaze hit a hologram.
            meshRenderer.enabled = rayResult.Hit;

            // Place the cursor at the calculated position.
            gameObject.transform.position = rayResult.Position + rayResult.Normal * DistanceFromCollision;

            // Reorient the cursor to match the hit object normal.
            gameObject.transform.up = rayResult.Normal;
            gameObject.transform.rotation *= cursorDefaultRotation;
        }
    }
}