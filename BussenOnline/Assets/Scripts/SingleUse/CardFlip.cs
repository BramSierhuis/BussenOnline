using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    public float speed = 5f;
    public Transform toRef; //Y rotation has to be 0
    public Sprite cardFront; //The card has to be instantiated with its back on top

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
    }

    public void OnClick_Button()
    {
        StartCoroutine(MoveWithoutRotation(toRef));
    }

    IEnumerator MoveWithRotation(Transform to)
    {
        float time = 0;
        float lerpValue;
        bool frontShown = false;
        Transform from = transform; //From y rotation has to be 180

        from.eulerAngles = -transform.eulerAngles;

        float journeyLength = Vector3.Distance(from.position, to.position);

        while (transform.position != to.position)
        {
            time += Time.deltaTime;
            lerpValue = time / (1 / (speed / journeyLength));

            if (transform.eulerAngles.y < 90 && !frontShown)
            {
                sr.sprite = cardFront;
                frontShown = true;
            }

            transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, Mathf.SmoothStep(0, 1, lerpValue));
            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, 1, lerpValue));

            yield return null;
        }
    }
    
    IEnumerator MoveWithoutRotation(Transform to)
    {
        float time = 0;
        float lerpValue;
        Transform from = transform; //From y rotation has to be 180

        float journeyLength = Vector3.Distance(from.position, to.position);

        while (transform.position != to.position)
        {
            time += Time.deltaTime;
            lerpValue = time / (1 / (speed / journeyLength));

            transform.position = Vector3.Lerp(from.position, to.position, Mathf.SmoothStep(0, 1, lerpValue));

            yield return null;
        }
    }
}
