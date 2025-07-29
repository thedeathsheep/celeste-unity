using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailAnimation : MonoBehaviour
{
    private int clock = 0;
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    void FixedUpdate()
    {
        int parentState = GetComponentInParent<StateUpdate>().state;
        //首次移动
        if (parentState == 1)
        {
            if ((int)(clock / 2) % 2 == 0)
            {
                sprite.flipX = true;
                sprite.flipY = true;
            }
            else
            {
                sprite.flipX = false;
                sprite.flipY = false;
            }
        }
        //第二次移动
        else if (parentState == 2)
        {
            if ((int)(clock / 3) % 2 == 0)
            {
                sprite.flipX = true;
                sprite.flipY = true;
            }
            else
            {
                sprite.flipX = false;
                sprite.flipY = false;
            }
        }

        //时钟管理
        if (parentState != 0)
        {
            if (clock < 12)
            {
                clock++;
            }
            else
            {
                clock = 0;
            }
        }
        else
        {
            clock = 0;
        }
    }
}
