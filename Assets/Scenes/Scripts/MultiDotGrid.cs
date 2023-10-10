using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MultiDotGrid : MonoBehaviour
{
    public GameObject dotPrefab; //Stored Dot Prefab.
    public Sprite[] dotSprites; // Array of dot sprites (Sprite1, Sprite2, Sprite3, Sprite4) 
    public List<List<string>> patterns = new List<List<string>>(); //Storing all the patterns for check.
    private List<string> visitedDotNames = new List<string>(); //Storing the visited dots for password.

    public Button submitButton;
    public Button cancelButton;

    private float spacing = 70f; // Adjust this value to change the distance between dots

    private RectTransform panelRect;
    public LineRenderer lineRenderer;
    private LineRenderer originalLineRenderer;
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private List<Vector3> linePositions = new List<Vector3>(); //Points that render line
    private bool isDrawing = false;
    private GameObject currentDot = null;  //for visiting dot multiple times
    private Dictionary<GameObject, int> dotStates = new Dictionary<GameObject, int>(); // To store the current state of each dot

    public UnityEvent OnDrawingComplete; // Event that gets triggered when drawing stops to check the password

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isDrawing)
        {
            StopDrawing();
        }
    }

    private void Awake()
    {
        originalLineRenderer = lineRenderer; // Store the original lineRenderer
        lineRenderer.startWidth = 5f; // Set the starting width of the line
        lineRenderer.endWidth = 5f;   // Set the ending width of the line
        panelRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        GenerateGrid();

        // Initialize buttons to deactivated state
        DeactivateButton(submitButton);
        DeactivateButton(cancelButton);

        // Add listeners to the buttons
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    void GenerateGrid()
    {// Calculate the total width and height of the grid
        float dotSize = 1f;
        float totalWidth = 2 * spacing + 3 * dotSize;
        float totalHeight = 2 * spacing + 3 * dotSize;

        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)) - new Vector3(0, 70, 0);
        Vector3 startPos = new Vector3(center.x - totalWidth / 2, center.y - totalHeight / 2, 0);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Vector3 dotPosition = new Vector3(startPos.x + x * (spacing + dotSize), startPos.y + y * (spacing + dotSize), 0);
                GameObject dot = Instantiate(dotPrefab, dotPosition, Quaternion.identity);

                // Set the name for the dot
                dot.name = x + "_" + y;

                dot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust the scale as needed
                dot.transform.SetParent(transform);

                dotStates[dot] = 0; // Initialize the state of each dot to 0 (i.e., dotPrefabs[0])
            }
        }
    }

    private void Update()
    {
        // Check if the parent GameObject is active
        if (!gameObject.activeInHierarchy)
            return;

        Vector3 inputPos;

        if (Input.GetMouseButtonDown(0))
        {
            inputPos = Input.mousePosition;

            // Check if the input position is inside the panel's rectangle
            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, inputPos, Camera.main))
            {
                return; // Ignore the click if it's outside the panel
            }

            Debug.Log("Got the mouse");
            StartDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            inputPos = Input.mousePosition;

            // Only stop drawing if the mouse release is inside the panel
            if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, inputPos, Camera.main))
            {
                StopDrawing();
            }
        }

        // Handle Touch Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPos = touch.position;

            // Check if the input position is inside the panel's rectangle only when touch begins
            if (touch.phase == TouchPhase.Began && !RectTransformUtility.RectangleContainsScreenPoint(panelRect, inputPos, Camera.main))
            {
                return; // Ignore the touch if it's outside the panel
            }

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Touch started");
                StartDrawing();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // Only stop drawing if the touch release is inside the panel
                if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, inputPos, Camera.main))
                {
                    StopDrawing();
                }
            }
        }

        if (isDrawing)
        {
            DrawLine();
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        linePositions.Clear();

        // Reset the currentDot to null
        currentDot = null;

        // Instantiate a new LineRenderer for the new pattern
        GameObject newLine = new GameObject("Line");
        newLine.transform.SetParent(transform);
        LineRenderer newLineRenderer = newLine.AddComponent<LineRenderer>();

        // Copy properties from the original lineRenderer to the new one
        newLineRenderer.material = lineRenderer.material;
        newLineRenderer.startColor = lineRenderer.startColor;
        newLineRenderer.endColor = lineRenderer.endColor;
        newLineRenderer.startWidth = lineRenderer.startWidth;
        newLineRenderer.endWidth = lineRenderer.endWidth;
        newLineRenderer.positionCount = 0;

        lineRenderers.Add(newLineRenderer);
        lineRenderer = newLineRenderer; // Set the current lineRenderer to the new one
    }

    private void DrawLine()
    {
        Vector3 inputPos;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));

        Collider2D hitCollider = null;  // Declare and initialize hitCollider here


        // Check if it's a touch input or mouse input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Mathf.Abs(Camera.main.transform.position.z)));
        }
        else
        {
            inputPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
        }

        hitCollider = Physics2D.OverlapPoint(inputPos);  // Assign value to hitCollider here


        if (hitCollider != null)
        {
            // Check if the current dot is different from the last visited dot
            if (currentDot == null || currentDot != hitCollider.gameObject)
            {
                // Check if the dot has been visited less than 3 times
                if (dotStates[hitCollider.gameObject] < 3)
                {
                    // Increment the state of the dot and change its sprite
                    dotStates[hitCollider.gameObject]++;

                    if (dotStates[hitCollider.gameObject] >= dotSprites.Length)
                    {
                        dotStates[hitCollider.gameObject] = 0; // Reset to the first sprite if we exceed the available sprites
                    }
                    ChangeDotSprite(hitCollider.gameObject, dotStates[hitCollider.gameObject]);

                    //Adding point to the line
                    linePositions.Add(hitCollider.transform.position);
                    lineRenderer.positionCount = linePositions.Count;
                    lineRenderer.SetPositions(linePositions.ToArray());

                    visitedDotNames.Add(hitCollider.gameObject.name); //Adding dot to the password
                    currentDot = hitCollider.gameObject; // Set the current dot to the newly visited dot
                }
            }
            else
            {
                // If no dot is under the input position, update the lineRenderer to follow the input position
                if (linePositions.Count > 0)
                {
                    lineRenderer.positionCount = linePositions.Count + 1;
                    lineRenderer.SetPosition(linePositions.Count, inputPos);
                }
            }
        }
        else
        {
            // If no dot is under the input position, update the lineRenderer to follow the input position
            if (linePositions.Count > 0)
            {
                lineRenderer.positionCount = linePositions.Count + 1;
                lineRenderer.SetPosition(linePositions.Count, inputPos);
            }
        }
    }

    private void ChangeDotSprite(GameObject dot, int spriteIndex)
    {
        if (spriteIndex < dotSprites.Length)
        {
            Image img = dot.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = dotSprites[spriteIndex];
            }
        }
    }

    private void StopDrawing()
    {
        isDrawing = false;

        // Store the current pattern and clear visitedDotNames for the next pattern
        patterns.Add(new List<string>(visitedDotNames));
        visitedDotNames.Clear();

        // Activate the buttons when a pattern is drawn
        ActivateButton(submitButton);
        ActivateButton(cancelButton);
    }

    // Function to deactivate a button
    private void DeactivateButton(Button btn)
    {
        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        Image btnIcon = btn.transform.Find("icon").GetComponent<Image>();

        btn.interactable = false;
        btnText.alpha = 0.5f;
        btnIcon.color = new Color(btnIcon.color.r, btnIcon.color.g, btnIcon.color.b, 0.5f);

        // Update the visual appearance of the button
        btn.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
        btnText.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
        btnIcon.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
    }

    // Function to activate a button
    private void ActivateButton(Button btn)
    {
        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        Image btnIcon = btn.transform.Find("icon").GetComponent<Image>();

        btn.interactable = true;
        btnText.alpha = 1f;
        btnIcon.color = new Color(btnIcon.color.r, btnIcon.color.g, btnIcon.color.b, 1f);

        // Update the visual appearance of the button
        btn.GetComponent<CanvasRenderer>().SetAlpha(1f);
        btnText.GetComponent<CanvasRenderer>().SetAlpha(1f);
        btnIcon.GetComponent<CanvasRenderer>().SetAlpha(1f);
    }

    private void OnCancelButtonClicked()
    {
        ResetEverything();

        DeactivateButton(submitButton);
        DeactivateButton(cancelButton);
    }

    private void OnSubmitButtonClicked()
    {
        // Convert each pattern list to a string and then join them with '|'
        string allPatterns = string.Join(" | ", patterns.Select(p => string.Join(",", p)));
        Debug.Log("You entered: " + allPatterns);

        // Check if there's at least one pattern and if that pattern has more than 4 visited dots
        if (patterns.Any(p => p.Count > 3))
        {
            OnDrawingComplete?.Invoke();
        }

        ResetEverything();

        DeactivateButton(submitButton);
        DeactivateButton(cancelButton);
    }

    private void ResetEverything()
    {
        //Clearing line
        linePositions.Clear();
        // Check if lineRenderer is not null before accessing it
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        // Clear all lines in the lineRenderers list
        foreach (LineRenderer lr in lineRenderers)
        {
            if (lr != originalLineRenderer) // Ensure that the original lineRenderer is not destroyed
            {
                lr.positionCount = 0;
                Destroy(lr.gameObject); // Destroy the GameObject associated with the LineRenderer
            }
        }
        lineRenderers.Clear(); // Clear the list after destroying all LineRenderers

        // Reassign the original lineRenderer and add it back to the list
        lineRenderer = originalLineRenderer;
        lineRenderers.Add(lineRenderer);

        // Clear the list of patterns and the current pattern
        patterns.Clear();
        visitedDotNames.Clear();

        // Reset all dots sprite back to Sprite[0] and reset their states to 0
        foreach (GameObject dot in GameObject.FindGameObjectsWithTag("Dot"))
        {
            ChangeDotSprite(dot, 0);
            dotStates[dot] = 0;
        }

        // Reset the currentDot to null
        currentDot = null;

        Debug.Log("Cleared Line");
    }
}
