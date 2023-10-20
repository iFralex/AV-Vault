using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Vector2 direz = Vector2.left;
    public int vel;
    public Transform origine;
    float aspetta;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = direz * vel;
    }

    void Update()
    {
        aspetta -= Time.deltaTime;
        ScegliDirez();
        GetComponent<Rigidbody2D>().velocity = direz * vel;
    }

    void ScegliDirez()
    {
        List<Vector2> pos = new List<Vector2>();
        if (aspetta > 0)
            return;
        Vector2 d = direz;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + (direz == Vector2.down ? -.5f : .5f)), Vector2.right, 1);
        if (hit.collider == null)
        {
            print("left");
            if (direz != Vector2.right)
                pos.Add(Vector2.right);
        }
        RaycastHit2D hit1 = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + (direz == Vector2.down ? -.5f : .5f)), Vector2.left, 1);
        if (hit1.collider == null)
        {
            print("left");
            if (direz != Vector2.left)
                pos.Add(Vector2.left);
        }
        RaycastHit2D hit2 = Physics2D.Raycast(new Vector2(transform.position.x + (direz == Vector2.right ? -.5f : .5f), transform.position.y), Vector2.up, 1);
        if (hit2.collider == null)
        {
            if (direz != Vector2.up)
                pos.Add(Vector2.up);
        }
        RaycastHit2D hit3 = Physics2D.Raycast(new Vector2(transform.position.x + (direz == Vector2.right ? -.5f : .5f), transform.position.y), Vector2.down, 1);
        if (hit3.collider == null)
        {
            if (direz != Vector2.down)
                pos.Add(Vector2.down);
        }

        if (pos.Count > 0)
        {
            if (pos.Count != 1)
            for (; ; )
            {
                int n = Random.Range(0, pos.Count);
                if (pos[n] != direz * -1)
                {
                    direz = pos[n];
                    break;
                }
            }
            else
                direz = pos[0];
            print(direz);
        }

        RaycastHit2D hit4 = Physics2D.Raycast(transform.position, direz, .5f);
        if (hit4.collider != null)
        {
            direz *= -1;
        }
    }
}
