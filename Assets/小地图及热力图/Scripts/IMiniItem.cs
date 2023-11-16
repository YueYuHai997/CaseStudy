using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMiniItem
{
    MiniType miniType { get; set; }

    Transform Transform { get; set; }
}