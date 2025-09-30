using UnityEngine;

public class ObjectDescription : MonoBehaviour
{
    [TextArea(3, 10)] // Делает поле многострочным в инспекторе
    public string description;
}

