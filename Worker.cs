using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Worker : MonoBehaviour
{
    [SerializeField] SpriteData hairData, headData, bodyData, legData;

    Sprite[] hair, bodies, legs;

    SpriteRenderer[] rends;

    SpriteRenderer hairRend, headRend, bodyRend, legsRend;

    public GameObject textPopUp;

    Vector3 destination, moveDirection;

    Vector3 origin;

    float moveSpeed;

    bool isWorking, isMoving, fadingOut;

    public bool left, quit;

    private void Awake()
    {
        textPopUp.SetActive(false);

        rends = GetComponentsInChildren<SpriteRenderer>();

        hairRend = rends[1];
        headRend = rends[0];
        bodyRend = rends[2];
        legsRend = rends[3];

        GenerateBody();

        origin = transform.position;
    }

    void Update()
    {
        if (isWorking && isMoving)
        {
            transform.position += moveDirection * Time.deltaTime * moveSpeed;

            if (Vector3.Distance(transform.position, destination) < 0.1f)
            {
                StartCoroutine(Wait());
            }
        }
        else if(!isWorking)
        {
            float dist = Vector3.Distance(transform.position, destination);

            if (dist > 0.1f)
            {
                transform.position += moveDirection * Time.deltaTime * moveSpeed;
            }
            else if(!fadingOut && dist < 0.5f)
            {
                StartCoroutine(Fade(false));
            }
        }
    }

    void GenerateBody()
    {
        hairRend.color = hairData.colours[Random.Range(0, hairData.colours.Length)];
        headRend.color = headData.colours[Random.Range(0, headData.colours.Length)];
        bodyRend.color = bodyData.colours[Random.Range(0, bodyData.colours.Length)];

        int[] indexes = new int[3];
        int[] lengths = new int[] { hairData.forward.Length, bodyData.forward.Length, legData.forward.Length };

        for (int i = 0; i < 3; i++)
        {
            indexes[i] = Random.Range(0, lengths[i]);
        }

        hair = new Sprite[] { hairData.forward[indexes[0]], hairData.right[indexes[0]], hairData.backward[indexes[0]], hairData.left[indexes[0]] };
        bodies = new Sprite[] { bodyData.forward[indexes[1]], bodyData.right[indexes[1]]};
        legs = new Sprite[] { legData.forward[indexes[2]], legData.right[indexes[2]]};
    }

    void NewDestination()
    {
        isMoving = true;

        float xPos = left ? Random.Range(-6.5f, -1.65f) : Random.Range(2f, 6);

        destination = new Vector3(xPos, Random.Range(-4.3f, 4f), 0);

        moveDirection = Vector3.Normalize(destination - transform.position);
        float x = moveDirection.x;
        float y = moveDirection.y;

        if(Mathf.Abs(x) >= Mathf.Abs(y))
        {
            if(x >= 0)
            {
                ChangeLook(1);
            }
            else
            {
                ChangeLook(3);
            }
        }
        else
        {
            if(y >= 0)
            {
                ChangeLook(2);
            }
            else
            {
                ChangeLook(0);
            }
        }
    }

    private void ChangeLook(int direction)
    {
        hairRend.sprite = hair[direction];
        bodyRend.sprite = bodies[direction % 2];
        legsRend.sprite = legs[direction % 2];
    }

    private IEnumerator Wait()
    {
        float time = Random.Range(1f, 4f);

        isMoving = false;

        yield return new WaitForSeconds(time);

        if (isWorking)
        {
            moveSpeed = 0.5f;
            NewDestination();
        }
    }

    public void Arrive()
    {
        fadingOut = false;

        StartCoroutine(Fade(true));

        isWorking = true;

        NewDestination();

        moveSpeed = 2;
    }

    public void Leave()
    {
        isWorking = false;
        isMoving = true;

        Vector3 pos = transform.position;
        destination = origin;
        moveDirection = Vector3.Normalize(destination - pos);

        moveSpeed = 2;
    }


    private IEnumerator Fade(bool arriving)
    {
        if (!arriving)
        {
            fadingOut = true;
        }

        foreach(SpriteRenderer rend in rends)
        {
            rend.DOFade(arriving ? 1:0, 0.5f);
            yield return null;
        }

        if (quit)
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}