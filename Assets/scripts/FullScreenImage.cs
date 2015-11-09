using UnityEngine;

public class FullScreenImage:bs
{
    public void Update()
    {
        var txt = guiTexture.texture;
        var localScale = Vector3.one;
        var y = ((float)Screen.width / Screen.height) / ((float)txt.width / txt.height);
        var x = ((float)Screen.height / Screen.width) / ((float)txt.height / txt.width);
        if (y < 1)
            localScale.x = x;
        else
            localScale.y = y;
        transform.localScale = localScale;
    }
}