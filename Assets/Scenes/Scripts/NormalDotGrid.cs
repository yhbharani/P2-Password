using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class NormalDotGrid : MonoBehaviour
{
    public GameObject dotPrefab; //Stored Dot Prefab.
    public Sprite[] dotSprites; // Array of dot sprites (Sprite1, Sprite2, Sprite3, Sprite4)
    public List<string> visitedDotNames = new List<string>(); //Storing the visited dots for password. 

    private float spacing = 70f; // Adjust this value to change the distance between dots
    private RectTransform panelRect;

    public LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>(); //Points that render line
    private bool isDrawing = false;
    private GameObject currentDot = null;  //for visiting dot multiple times

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
        lineRenderer.startWidth = 5f; // Set the starting width of the line
        lineRenderer.endWidth = 5f;   // Set the ending width of the line
        panelRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        GenerateGrid();
    }

    void GenerateGrid()
    {// Calculate the total width and height of the grid
        float dotSize = 1f;
        float totalWidth = 2 * spacing + 3 * dotSize;
        float totalHeight = 2 * spacing + 3 * dotSize;

        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)) - new Vector3(0, 100, 0);
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
                Debug.Log("Lost the mouse");
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
        lineRenderer.positionCount = 0;
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
            // Check if the current dot is different from the last visited dot and if it hasn't been visited before
            if ((currentDot == null || currentDot != hitCollider.gameObject) && !visitedDotNames.Contains(hitCollider.gameObject.name))
            {
                // Change the dot's sprite
                ChangeDotSprite(hitCollider.gameObject, 1); // Assuming the second sprite indicates a visited state

                // Adding point to the line
                linePositions.Add(hitCollider.transform.position);
                lineRenderer.positionCount = linePositions.Count;
                lineRenderer.SetPositions(linePositions.ToArray());

                visitedDotNames.Add(hitCollider.gameObject.name); // Adding dot to the password
                currentDot = hitCollider.gameObject; // Set the current dot to the newly visited dot
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

        // Concatenate the names in the list and log the result
        string password = string.Join(",", visitedDotNames);
        Debug.Log("Password: " + password);

        // Trigger the event to check the password only if there are more than 3 dots
        if (visitedDotNames.Count > 3)
        {
            OnDrawingComplete?.Invoke();
        }

        //Clearing line
        linePositions.Clear();
        lineRenderer.positionCount = 0;

        // Clear the list of visited dot names for the next attempt
        visitedDotNames.Clear();

        // Reset all dots sprite back to Sprite[0]
        foreach (GameObject dot in GameObject.FindGameObjectsWithTag("Dot"))
        {
            ChangeDotSprite(dot, 0);
        }

        // Reset the currentDot to null
        currentDot = null;

        Debug.Log("Cleared Line");
    }
}
