using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    public float speed = 5f;
    public Transform to; //Y rotation has to be 0
    public Sprite cardFront; //The card has to be instantiated with its back on top

    private SpriteRenderer sr;
    private float journeyLength;
    private float time;
    private float lerpValue;
    private Transform from;
    private bool frontShown = false;

    private void Awake()
    {
        from = transform; //From y rotation has to be 180
        from.eulerAngles = -transform.eulerAngles;

        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        journeyLength = Vector3.Distance(from.position, to.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != to.position)
        {
            time += Time.deltaTime;
            lerpValue = time / (1 / (speed/journeyLength));

            if (transform.eulerAngles.y < 90 && !frontShown)
            {
                sr.sprite = cardFront;
                frontShown = true;
            }

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(0, 1, lerpValue));
            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, 1, lerpValue));
        }
    }
}
