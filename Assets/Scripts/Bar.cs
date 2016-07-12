﻿using UnityEngine;
using System.Collections;

public class Bar : Roots
{
    private static int spriteWidth = 160;
    private static int spriteHeight = 16;

    public Vector2 setLanges(float now, float max, float maxPixel, Vector2? basePosition = null)
    {
        float widthBasePixel = spriteWidth / GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float heightBasePixel = spriteHeight / GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float nowWidth = maxPixel * now / max;
        float nowHeight = Mathf.Min(maxPixel * spriteHeight / spriteWidth, 0.5f);

        transform.localScale = new Vector2(nowWidth / widthBasePixel, nowHeight / heightBasePixel);
        Vector2 parentPosition = transform.parent != null
            ? (Vector2)transform.parent.transform.position
            : new Vector2(0, 0);
        transform.position = parentPosition 
            + (basePosition ?? Camera.main.ViewportToWorldPoint(new Vector2(0, 1)))
            + new Vector2(nowWidth / 2, -nowHeight / 2);

        return new Vector2(nowWidth, nowHeight);
    }
}
