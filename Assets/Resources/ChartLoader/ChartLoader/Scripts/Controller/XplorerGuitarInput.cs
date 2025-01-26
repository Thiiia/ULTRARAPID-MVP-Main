using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class XplorerGuitarInput : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool DebugController = false;

    [FoldoutGroup("Button Mappings")]
    public int A;
    [FoldoutGroup("Button Mappings")]
    public int B;
    [FoldoutGroup("Button Mappings")]
    public int X;
    [FoldoutGroup("Button Mappings")]
    public int Y;
    [FoldoutGroup("Button Mappings")]
    public int START;
    [FoldoutGroup("Button Mappings")]
    public int SELECT;

    [FoldoutGroup("D-Pad Settings")]
    public int DPadLeft = 0;
    [FoldoutGroup("D-Pad Settings")]
    public int DPadRight = 0;
    [FoldoutGroup("D-Pad Settings")]
    public int DPadUp = 0;
    [FoldoutGroup("D-Pad Settings")]
    public int DPadDown = 0;

    [FoldoutGroup("Shoulder Buttons")]
    public int rightShoulder;
    [FoldoutGroup("Shoulder Buttons")]
    public int leftShoulder;

    [FoldoutGroup("Strum Settings")]
    public int strum = 0;

    // Visual state indicators for keys
    [FoldoutGroup("Key States")]
    [GUIColor(0f, 1f, 0f)] // Green for pressed
    public bool green;
    [FoldoutGroup("Key States")]
    [GUIColor(1f, 0f, 0f)] // Red for pressed
    public bool red;
    [FoldoutGroup("Key States")]
    [GUIColor(1f, 1f, 0f)] // Yellow for pressed
    public bool yellow;
    [FoldoutGroup("Key States")]
    [GUIColor(0f, 0f, 1f)] // Blue for pressed
    public bool blue;
    [FoldoutGroup("Key States")]
    [GUIColor(1f, 0.647f, 0f)] // Orange for pressed
    public bool orange;

    // Update is called once per frame
    void Update()
    {
        // GetControllerInput(); <---- disabled for now
        GetKeyboardInput();
    }

    // Retrieves the current keyboard input.
    private void GetKeyboardInput()
    {
        green = Input.GetKeyDown(KeyCode.A);          // Corresponds to the 'A' button
        red = Input.GetKeyDown(KeyCode.S);            // Corresponds to the 'B' button
        yellow = Input.GetKeyDown(KeyCode.D);         // Corresponds to the 'Y' button
        blue = Input.GetKeyDown(KeyCode.F);           // Corresponds to the 'X' button
        orange = Input.GetKeyDown(KeyCode.LeftShift); // Corresponds to 'Left Shoulder'

        A = returnState(green, A);
        B = returnState(red, B);
        X = returnState(blue, X);
        Y = returnState(yellow, Y);
        START = returnState(Input.GetKey(KeyCode.Return), START);      // Enter key for START
        SELECT = returnState(Input.GetKey(KeyCode.Backspace), SELECT); // Backspace for SELECT
        rightShoulder = returnState(Input.GetKey(KeyCode.RightShift), rightShoulder); // Right Shoulder mapped to Right Shift
        leftShoulder = returnState(orange, leftShoulder);

        // Manage arrow keys for D-pad input
        DPadLeft = returnState(Input.GetKey(KeyCode.LeftArrow), DPadLeft);
        DPadRight = returnState(Input.GetKey(KeyCode.RightArrow), DPadRight);
        DPadUp = returnState(Input.GetKey(KeyCode.UpArrow), DPadUp);
        DPadDown = returnState(Input.GetKey(KeyCode.DownArrow), DPadDown);
    }

    // Retrieves the current controller input.
    private void GetControllerInput()
    {
        float dplr = 0;
        float dpud = 0;

        green = Input.GetButton("A");
        red = Input.GetButton("B");
        yellow = Input.GetButton("Y");
        blue = Input.GetButton("X");
        orange = Input.GetButton("Left Shoulder");

        A = returnState(green, A);
        B = returnState(red, B);
        X = returnState(blue, X);
        Y = returnState(yellow, Y);
        START = returnState(Input.GetButton("START"), START);
        SELECT = returnState(Input.GetButton("SELECT"), SELECT);
        rightShoulder = returnState(Input.GetButton("Right Shoulder"), rightShoulder);
        leftShoulder = returnState(orange, leftShoulder);

        dplr = Input.GetAxisRaw("DPadLeftRight");
        dpud = Input.GetAxisRaw("DPadUpDown");

        bool tempLeft = false;
        bool tempRight = false;
        bool tempUp = false;
        bool tempDown = false;

        // Manage d-pad
        if (dplr == -1)
        {
            tempLeft = true;
        }
        else if (dplr == 1)
        {
            tempRight = true;
        }

        if (dpud == -1)
        {
            tempDown = true;
        }
        else if (dpud == 1)
        {
            tempUp = true;
        }

        // Return dpad state.
        DPadLeft = returnState(tempLeft, DPadLeft);
        DPadRight = returnState(tempRight, DPadRight);
        DPadDown = returnState(tempDown, DPadDown);
        DPadUp = returnState(tempUp, DPadUp);
    }

    /* The return state method checks at which state the controller currently is.
     * 0 = no input.
     * 1 = on input down
     * 2 = input hold
     * 3 = on input up
     */
    int returnState(bool action, int state)
    {
        if (action && state == 0)
        {
            state = 1;
        }
        else if (action && (state == 1 || state == 2))
        {
            state = 2;
        }
        else if (!action && (state == 1 || state == 2))
        {
            state = 3;
        }
        else if (!action && (state == 3 || state == 0))
        {
            state = 0;
        }
        return state;
    }

    // Debugging Controller
    private void OnGUI()
    {
        if (DebugController)
        {
            string tmp;

            tmp = "Guitar Controller Input\n" +
                "  Green: " + green + "\n" +
                "  Red: " + red + "\n" +
                "  Yellow: " + yellow + "\n" +
                "  Blue: " + blue ;

            GUI.Label(new Rect(0, 0, Screen.height, Screen.width), tmp);
        }
    }
}
