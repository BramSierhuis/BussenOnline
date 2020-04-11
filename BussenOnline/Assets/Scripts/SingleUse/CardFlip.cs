using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    public float speed = 1f;
    public Transform from; //Y rotation has to be 180
    public Transform to; //Y rotation has to be 0
    public Sprite cardFront; //The card has to be instantiated with its back on top

    private SpriteRenderer sr;
    private float startTime;
    private float journeyLength;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(from.position, to.position);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != to.position)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength; //How far it is on its way (1/10th, 1/2, etc)

            if (distCovered > journeyLength / 2)
                sr.sprite = cardFront;

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(from.rotation.y, to.rotation.y, fractionOfJourney));
            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, journeyLength / speed, fractionOfJourney));
        }
    }
}
