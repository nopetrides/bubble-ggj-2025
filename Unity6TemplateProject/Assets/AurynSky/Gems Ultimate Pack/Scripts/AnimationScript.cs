using UnityEngine;

public class AnimationScript : MonoBehaviour {

    public bool isAnimated = false;

    public bool isRotating = false;
    public bool isFloating = false;
    public bool isScaling = false;

    public Vector3 rotationAngle;
    public float rotationSpeed;

    public Vector3 floatSpeed;
    public float floatHeight;
   
    public Vector3 startScale;
    public Vector3 endScale;

    private bool scalingUp = true;
    public float scaleSpeed;
    public float scaleRate;
    private float scaleTimer;

    private Vector3 _posWhenEnabled;
    private Quaternion _rotationWhenEnabled;
    private Vector3 _scaleWhenEnabled;

    private void OnEnable()
    {
        var transform1 = transform;
        _posWhenEnabled = transform1.localPosition;
        _rotationWhenEnabled = transform1.localRotation;
        _scaleWhenEnabled = transform1.localScale;
    }

    private void OnDisable()
    {
        transform.localPosition = _posWhenEnabled;
        transform.localRotation = _rotationWhenEnabled;
        transform.localScale = _scaleWhenEnabled;
    }

    // Update is called once per frame
	void Update()
    {
        if(isAnimated)
        {
            if(isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime);
            }

            if(isFloating)
            {
                float newX = _posWhenEnabled.x + Mathf.Sin(Time.time * floatSpeed.x) * floatHeight;
                float newY = _posWhenEnabled.y + Mathf.Sin(Time.time * floatSpeed.y) * floatHeight;
                float newZ = _posWhenEnabled.z + Mathf.Sin(Time.time * floatSpeed.z) * floatHeight;
                // Update the local position
                transform.localPosition = new Vector3(newX, newY, newZ);
            }

            if(isScaling)
            {
                scaleTimer += Time.deltaTime;

                if (scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, endScale, scaleSpeed * Time.deltaTime);
                }
                else if (!scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale, scaleSpeed * Time.deltaTime);
                }

                if(scaleTimer >= scaleRate)
                {
                    if (scalingUp) { scalingUp = false; }
                    else if (!scalingUp) { scalingUp = true; }
                    scaleTimer = 0;
                }
            }
        }
	}
}
