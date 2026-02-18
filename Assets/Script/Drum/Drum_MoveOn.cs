using UnityEngine;

public class Drum_MoveOn : MonoBehaviour
{
    Vector3 startPos;
    public float moveDistance = 7.5f;
    public float waitTime = 2f;
    bool isMoving = false;
    int moveFrames = 0;
    float waitTimer = 0f;
    Vector3 targetPos;
    bool isWaiting = false;

    void Start()
    {
        startPos = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isMoving) return;
        if (other.CompareTag("Player"))
        {
            targetPos = startPos + Vector3.right * moveDistance;
            moveFrames = 0;
            isMoving = true;
            isWaiting = false;
            waitTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        if (!isWaiting)
        {
            moveFrames++;

            if (moveFrames <= 7)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, moveFrames / 7f);
            }
            else
            {
                transform.position = targetPos;
                isWaiting = true;
            }
        }
        else
        {
            waitTimer += Time.fixedDeltaTime;

            if (waitTimer >= waitTime)
            {
                moveFrames++;
                int returnFrame = moveFrames - 7;

                if (returnFrame <= 7)
                {
                    transform.position = Vector3.Lerp(targetPos, startPos, returnFrame / 7f);
                }
                else
                {
                    transform.position = startPos;
                    isMoving = false;
                }
            }
        }
    }
}