using UnityEngine;

public interface ISelectable
{
    void OnSelect();
    void OnDeselect();
    string GetLabel(); // например, "Астероид (железо)"
}