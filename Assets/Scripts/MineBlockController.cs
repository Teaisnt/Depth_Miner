using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBlockController : MonoBehaviour
{
    public SpriteRenderer progressBar;

    public void ChangeColor(Color newColor)
    {
        progressBar.color = newColor;
    }
}
