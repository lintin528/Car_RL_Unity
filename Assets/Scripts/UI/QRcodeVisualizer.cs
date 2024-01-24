using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CJM.BBox2DToolkit
{
    /// <summary>
    /// BoundingBox2DVisualizer is a MonoBehaviour class responsible for displaying 2D bounding boxes, labels, and label backgrounds
    /// on a Unity canvas. It creates, updates, and manages UI elements for visualizing the bounding boxes based on the provided
    /// BBox2DInfo array. This class supports customizable settings such as bounding box transparency and the ability to toggle
    /// the display of bounding boxes.
    /// </summary>
    public class QRcodeVisualizer : MonoBehaviour
    {
        // UI components
        [Header("Components")]
        [Tooltip("Container for holding the bounding box UI elements")]
        [SerializeField] private RectTransform boundingBoxContainer;
        [Tooltip("Container for holding the label UI elements")]
        [SerializeField] private RectTransform labelContainer;

        // Prefabs for creating UI elements
        [Header("Prefabs")]
        [Tooltip("Prefab for the bounding box UI element")]
        [SerializeField] private RectTransform boundingBoxPrefab;
        [Tooltip("Prefab for the label UI element")]
        [SerializeField] private TMP_Text labelPrefab;
        [Tooltip("Prefab for the label background UI element")]
        [SerializeField] private Image labelBackgroundPrefab;
        [Tooltip("Prefab for the dot UI element")]
        [SerializeField] private Image dotPrefab;

        // Settings for customizing the bounding box visualizer
        [Header("Settings")]
        [Tooltip("Flag to control whether bounding boxes should be displayed or not")]
        [SerializeField] private bool displayBoundingBoxes = true;
        [Tooltip("Transparency value for the bounding boxes, ranging from 0 (completely transparent) to 1 (completely opaque)")]
        [SerializeField, Range(0f, 1f)] private float bboxTransparency = 1f;

        bool useMainCamera = true;

        // Lists for storing and managing instantiated UI elements
        private List<RectTransform> boundingBoxes = new List<RectTransform>(); // List of instantiated bounding box UI elements
        private List<TMP_Text> labels = new List<TMP_Text>(); // List of instantiated label UI elements
        private List<Image> labelBackgrounds = new List<Image>(); // List of instantiated label background UI elements
        

        /// <summary>
        /// Update the visualization of bounding boxes based on the given BBox2DInfo array.
        /// </summary>
        /// <param name="bboxInfoArray">An array of BBox2DInfo objects containing bounding box information</param>
        public void UpdateBoundingBoxVisualizations(BBox2DInfo[] bboxInfoArray)
        {
            // Depending on the displayBoundingBoxes flag, either update or disable bounding box UI elements
            if (displayBoundingBoxes && !useMainCamera)
            {
                UpdateBoundingBoxes(bboxInfoArray);
            }
            else
            {
                // Disable bounding boxes, labels, and label backgrounds for all existing UI elements
                for (int i = 0; i < boundingBoxes.Count; i++)
                {
                    boundingBoxes[i].gameObject.SetActive(false);
                    labelBackgrounds[i].gameObject.SetActive(false);
                    labels[i].gameObject.SetActive(false);             
                }
            }
        }

        /// <summary>
        /// Convert a screen point to a local point in the RectTransform space of the given canvas.
        /// </summary>
        /// <param name="canvas">The RectTransform object of the canvas</param>
        /// <param name="screenPoint">The screen point to be converted</param>
        /// <returns>A Vector2 object representing the local point in the RectTransform space of the canvas</returns>
        private Vector2 ScreenToCanvasPoint(RectTransform canvas, Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPoint, null, out Vector2 localPoint);
            localPoint.y = -localPoint.y;
            return localPoint;
        }

        /// <summary>
        /// Update bounding box UI elements to match the provided BBox2DInfo array.
        /// </summary>
        /// <param name="bboxInfoArray">An array of BBox2DInfo objects containing bounding box information</param>
        private void UpdateBoundingBoxes(BBox2DInfo[] bboxInfoArray)
        {
            // Create or remove bounding box UI elements to match the number of detected objects
            while (boundingBoxes.Count < bboxInfoArray.Length)
            {
                RectTransform newBoundingBox = Instantiate(boundingBoxPrefab, boundingBoxContainer);
                boundingBoxes.Add(newBoundingBox);

                Image newLabelBackground = Instantiate(labelBackgroundPrefab, labelContainer);
                labelBackgrounds.Add(newLabelBackground);

                TMP_Text newLabel = Instantiate(labelPrefab, labelContainer);
                labels.Add(newLabel);
            }

            // Update bounding boxes, labels, and label backgrounds for each detected object, or disable UI elements if not needed
            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                if (i < bboxInfoArray.Length)
                {
                    BBox2DInfo bboxInfo = bboxInfoArray[i];

                    // Get UI elements for the current bounding box, label, and label background
                    RectTransform boundingBox = boundingBoxes[i];
                    TMP_Text label = labels[i];
                    Image labelBackground = labelBackgrounds[i];

                    UpdateBoundingBox(boundingBox, bboxInfo);
                    UpdateLabelAndBackground(label, labelBackground, bboxInfo);

                    // Enable bounding box, label, and label background UI elements
                    boundingBox.gameObject.SetActive(true);
                    labelBackground.gameObject.SetActive(true);
                    label.gameObject.SetActive(true);
                }
                else
                {
                    // Disable UI elements for extra bounding boxes, labels, and label backgrounds
                    boundingBoxes[i].gameObject.SetActive(false);
                    labelBackgrounds[i].gameObject.SetActive(false);
                    labels[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update the bounding box UI element with the information from the given BBox2DInfo object.
        /// </summary>
        /// <param name="boundingBox">The RectTransform object representing the bounding box UI element</param>
        /// <param name="bboxInfo">The BBox2DInfo object containing the information for the bounding box</param>
        private void UpdateBoundingBox(RectTransform boundingBox, BBox2DInfo bboxInfo)
        {
            // Convert the screen point to a local point in the RectTransform space of the bounding box container
            Vector2 localPosition = ScreenToCanvasPoint(boundingBoxContainer, new Vector2(bboxInfo.bbox.x0, bboxInfo.bbox.y0));
            boundingBox.anchoredPosition = localPosition;
            boundingBox.sizeDelta = new Vector2(bboxInfo.bbox.width, bboxInfo.bbox.height);

            // Set the color of the bounding box with the specified transparency
            Color color = GetColorWithTransparency(bboxInfo.color);
            Image[] sides = boundingBox.GetComponentsInChildren<Image>();
            foreach (Image side in sides)
            {
                side.color = color;
            }
        }

        /// <summary>
        /// Update the label and label background UI elements with the information from the given BBox2DInfo object.
        /// </summary>
        /// <param name="label">The TMP_Text object representing the label UI element</param>
        /// <param name="labelBackground">The Image object representing the label background UI element</param>
        /// <param name="bboxInfo">The BBox2DInfo object containing the information for the label and label background</param>
        private void UpdateLabelAndBackground(TMP_Text label, Image labelBackground, BBox2DInfo bboxInfo)
        {
            // Convert the screen point to a local point in the RectTransform space of the bounding box container
            Vector2 localPosition = ScreenToCanvasPoint(boundingBoxContainer, new Vector2(bboxInfo.bbox.x0, bboxInfo.bbox.y0));

            // Set the label text and position
            label.text = $"{bboxInfo.label}%";
            label.rectTransform.anchoredPosition = new Vector2(localPosition.x, localPosition.y - label.preferredHeight);

            // Set the label color based on the grayscale value of the bounding box color
            Color color = GetColorWithTransparency(bboxInfo.color);
            label.color = color.grayscale > 0.5 ? Color.black : Color.white;

            // Set the label background position and size
            labelBackground.rectTransform.anchoredPosition = new Vector2(localPosition.x, localPosition.y - label.preferredHeight);
            labelBackground.rectTransform.sizeDelta = new Vector2(Mathf.Max(label.preferredWidth, bboxInfo.bbox.width), label.preferredHeight);

            // Set the label background color with the specified transparency
            labelBackground.color = color;
        }

        /// <summary>
        /// Get a new color based on the input color with the adjusted transparency.
        /// </summary>
        /// <param name="color">The input color to be modified</param>
        /// <returns>A new color with the specified transparency</returns>
        private Color GetColorWithTransparency(Color color)
        {
            color.a = bboxTransparency;
            return color;
        }

        public void SetDisplayBoundingBoxes(bool decision)
        {
            displayBoundingBoxes = decision;
        }

        public void UseMainCamera(bool decision)
        {
            useMainCamera = decision;
        }
    }
}